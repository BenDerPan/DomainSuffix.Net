using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DomainSuffix.Net
{
    public class DomainValidator
    {
        /// <summary>
        /// public domain suffix online source url address.
        /// </summary>
        public const string DefaultOnlineSourceUrl = "https://publicsuffix.org/list/public_suffix_list.dat";
        /// <summary>
        /// default local cache data file.
        /// </summary>
        public static readonly string DefaultOfflineSourceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "public_suffix_list.dat");
        
        static readonly HashSet<string> _validSuffixDict = new HashSet<string>();
        static DomainValidator()
        {
            ReloadDataSource();
        }
        static bool ReloadDataSource()
        {
            _validSuffixDict.Clear();
            try
            {
                Stream stream = null;
                if (File.Exists(DefaultOfflineSourceFilePath))
                {
                    stream = new FileStream(DefaultOfflineSourceFilePath, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    string assembleName = typeof(DomainValidator).Assembly.GetName().Name;
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{assembleName}.public_suffix_list.dat");
                }
                using (stream)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var line = string.Empty;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var suffix = line.Trim();
                            if (suffix.StartsWith("/") || string.IsNullOrEmpty(suffix))
                            {
                                continue;
                            }
                            if (!_validSuffixDict.Contains(suffix))
                            {
                                _validSuffixDict.Add(suffix);
                            }

                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        static async Task<bool> ReloadDataSourceAsync()
        {
            _validSuffixDict.Clear();
            try
            {
                Stream stream = null;
                if (File.Exists(DefaultOfflineSourceFilePath))
                {
                    stream = new FileStream(DefaultOfflineSourceFilePath, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("public_suffix_list.dat");
                }
                using (stream)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var line = string.Empty;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            var suffix = line.Trim();
                            if (suffix.StartsWith("/") || string.IsNullOrEmpty(suffix))
                            {
                                continue;
                            }
                            if (!_validSuffixDict.Contains(suffix))
                            {
                                _validSuffixDict.Add(suffix);
                            }

                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// update public domain suffix online
        /// </summary>
        /// <returns>if success return: true, if not return: false</returns>
        public static async Task<bool> UpdateOnlineSourceAsync()
        {
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var resp = await client.SendAsync(new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(DefaultOnlineSourceUrl),
                });
                if (resp.IsSuccessStatusCode)
                {
                    var data = await resp.Content.ReadAsStringAsync();
                    File.WriteAllText(DefaultOfflineSourceFilePath, data);
                    return await ReloadDataSourceAsync();
                }

            }
            catch (Exception e)
            {
            }


            return false;
        }

        /// <summary>
        /// try parse a domain to diffirent part
        /// </summary>
        /// <param name="source">the target domain</param>
        /// <param name="mainDomain">main domain,such as "google.com"</param>
        /// <param name="subDomainPart">sub domain part, such as "www"</param>
        /// <param name="suffixPart">suffix part, such as "com"</param>
        /// <returns>if is valid domain return: true, if not return: false</returns>
        public static bool TryParse(string source,out string mainDomain,out string subDomainPart,out string suffixPart)
        {
            mainDomain = null;
            subDomainPart = null;
            suffixPart = null;
            // except ip condition
            source = source.Trim();
            if (IPAddress.TryParse(source,out _))
            {
                return false;
            }

            if (Uri.TryCreate($"http://{source}",UriKind.Absolute,out var u))
            {
                var urlParts = u.Host.Split(new[] { '.' });
                if (urlParts.Length<2)
                {
                    return false;
                }
                if (!_validSuffixDict.Contains(urlParts[urlParts.Length-1]))
                {
                    //invalid domain suffix
                    return false;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append(urlParts[urlParts.Length - 1]);

                for (int i = urlParts.Length-1; i>0 ; i--)
                {
                    var last = builder.ToString();
                    builder.Clear();
                    builder.Append(urlParts[i-1]);
                    builder.Append(".");
                    builder.Append(last);

                    var current = builder.ToString();
                    if (_validSuffixDict.Contains(last)&&!_validSuffixDict.Contains(current))
                    {
                        //maindomain
                        mainDomain = current;
                        suffixPart = last;
                        var subIndex = source.LastIndexOf($".{current}");
                        if (subIndex<0)
                        {
                            subDomainPart = string.Empty;
                        }
                        else
                        {
                            subDomainPart = source.Substring(0, subIndex);
                        }
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }

        }
        
    }
}
