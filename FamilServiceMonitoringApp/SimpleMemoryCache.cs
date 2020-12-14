using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FamilServiceMonitoringApp
{
    public class SimpleMemoryCache
    {
        private static volatile SimpleMemoryCache _instance;

        public static SimpleMemoryCache Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SimpleMemoryCache();
                }

                return _instance;
            }
        }

        private string _identity = string.Empty;

        private MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public List<Result> Products { get; set; }

        public List<Result> Contacts { get; set; }

        public List<Result> Shops { get; set; }

        public List<Result> GetOrCreate(object key/*, Func<List<Result>> createItem*/)
        {
            List<Result> cacheEntry;
            if (!_cache.TryGetValue(key, out cacheEntry))
            {
                cacheEntry = Select(key as string);

                //cacheEntry = createItem();

                _cache.Set(key, cacheEntry);
            }
            return cacheEntry;
        }

        public void Clear()
        {
            _cache.Remove("shop");
            _cache.Remove("contact");
            _cache.Remove("product");
        }

        public List<Result> Select(string path)
        {
            var list = new List<Result>();
            try
            {
                string body = @"{
									""count"": ""5""
								}";

                GlobalCacheReader.GetValue(GlobalCacheReader.CacheKeys.HelperServiceUrl, out string url);
                //var req = (HttpWebRequest)WebRequest.Create("http://localhost:55252/api/select/" + path);
                var req = (HttpWebRequest)WebRequest.Create(url + path);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Accept = "application/json";
                req.Credentials = System.Net.CredentialCache.DefaultCredentials;
                req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                if (_identity == string.Empty)
                {
                    _identity = GetIdentity();
                }
                req.Headers.Add("Authorization", "Bearer " + _identity);

                using (var requestStream = req.GetRequestStream())
                {
                    using (var streamWriter = new StreamWriter(requestStream))
                    {
                        streamWriter.Write(body);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }


                using (var response = req.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(dataStream);
                        var str = reader.ReadToEnd();
                        list = JsonConvert.DeserializeObject<Result[]>(str).ToList();
                    }
                }

            }
            catch (Exception ex)
            {
                if (ex.Message == "The remote server returned an error: (401) Unauthorized.")
                {
                    var token = GetIdentity();
                }
            }
            return list;
        }

        public string GetIdentity()
        {
            string identity = string.Empty;
            string body = @"{
									""Login"": ""Random"",
                                    ""Password"": ""TyRaNiD""
								}";
            var req = (HttpWebRequest)WebRequest.Create("http://localhost:55252/api/Identity/token");
            req.Method = "POST";
            req.ContentType = "application/json";
            req.Accept = "application/json";
            req.Credentials = System.Net.CredentialCache.DefaultCredentials;
            req.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

            using (var requestStream = req.GetRequestStream())
            {
                using (var streamWriter = new StreamWriter(requestStream))
                {
                    streamWriter.Write(body);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }

            using (var response = req.GetResponse())
            {
                using (Stream dataStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream);
                    identity = reader.ReadToEnd();
                }
            }
            return identity.Replace("\"", "");
        }
    }

    public class Result
    {
        public Guid Id { get; set; }

        public string Code { get; set; }
    }
}
