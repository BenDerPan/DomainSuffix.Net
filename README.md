# DomainSuffix.Net
Domain suffix library for Domain or Subdomain extract. 域名后缀库，用于识别合法域名或抽取子域名的合法主域名。

#### example

```C#
 [TestMethod]
        public void TestMethod1()
        {
            var url = "1.com";
            var isOk = DomainValidator.TryParse(url, out var mainDomain, out var subPart, out var suffix);
            Assert.AreEqual(isOk, true);
            Assert.AreEqual(mainDomain, "1.com");
            Assert.AreEqual(subPart, string.Empty);
            Assert.AreEqual(suffix, "com");
        }

        [TestMethod]
        public void TestMethod2()
        {
            var url = "dns.1.com.cn";
            var isOk = DomainValidator.TryParse(url, out var mainDomain, out var subPart, out var suffix);
            Assert.AreEqual(isOk, true);
            Assert.AreEqual(mainDomain, "1.com.cn");
            Assert.AreEqual(subPart, "dns");
            Assert.AreEqual(suffix, "com.cn");
        }

        [TestMethod]
        public void TestMethod3()
        {
            var url = "abc.com.cxxx";
            var isOk = DomainValidator.TryParse(url, out var mainDomain, out var subPart, out var suffix);
            Assert.AreEqual(isOk, false);
           
        }

        [TestMethod]
        public void TestMethod4()
        {
            // update source online
            var isOk = DomainValidator.UpdateOnlineSourceAsync().Result;
            
            Assert.AreEqual(isOk, true);

            var isDownloadOk = File.Exists(DomainValidator.DefaultOfflineSourceFilePath);
            Assert.AreEqual(isDownloadOk,true);

            var url = "dns.1.com.cn";
            isOk = DomainValidator.TryParse(url, out var mainDomain, out var subPart, out var suffix);
            Assert.AreEqual(isOk, true);
            Assert.AreEqual(mainDomain, "1.com.cn");
            Assert.AreEqual(subPart, "dns");
            Assert.AreEqual(suffix, "com.cn");
        }
```
