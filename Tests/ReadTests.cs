using NUnit.Framework;
using Net.SByteStream.Utils.EnableCORS;
using System;

namespace Net.SByteStream.Utils.EnableCORS.Tests
{
    public class ReadTests
    {
        [TestCase()]
        public void ReadFileTest()
        {
            string input_file = @"..\..\..\..\Docs\sample_input.txt";            
            ConfigFileReader reader = new ConfigFileReader(input_file);
            int entryCount = 0;
            reader.EntryFoundEvent += new EventHandler<ConfigEntry>((sender, entry) => 
            {
                entryCount++;

                Assert.IsNotNull(entry.Path);
                Assert.IsNotNull(entry.AllowedHeaders);
                Assert.IsNotNull(entry.AllowedMethods);
                Assert.IsNotNull(entry.AllowedOrigins);

                if (entry.Path == "/roles")
                {
                    Assert.AreEqual(entry.AllowedHeaders, "Content-Type,x-requested-with,Access-Control-Allow-Headers,Access-Control-Allow-Origin,Access-Control-Allow-Methods");
                    Assert.AreEqual(entry.AllowedMethods, "get,options");
                    Assert.AreEqual(entry.AllowedOrigins, "*");
                }
                else if (entry.Path == "/categories")
                {
                    Assert.AreEqual(entry.AllowedHeaders, "Content-Type,x-requested-with,Access-Control-Allow-Headers,Access-Control-Allow-Origin,Access-Control-Allow-Methods");
                    Assert.AreEqual(entry.AllowedMethods, "*");
                    Assert.AreEqual(entry.AllowedOrigins, "mydomain.com,myhost.com");
                }
                else if (entry.Path == "/posts")
                {
                    Assert.AreEqual(entry.AllowedHeaders, "Content-Type,x-requested-with,Access-Control-Allow-Headers,Access-Control-Allow-Origin,Access-Control-Allow-Methods");
                    Assert.AreEqual(entry.AllowedMethods, "get,options");
                    Assert.AreEqual(entry.AllowedOrigins, "*");
                }
                else
                {
                    Assert.Fail("Invalid path has been read");
                }                
            });
            reader.Read();
            Assert.AreEqual(entryCount, 2);
        }
    }
}
