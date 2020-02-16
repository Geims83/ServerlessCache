using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CacheClient
{
    public class ServerlessCacheClient
    {
        private Lazy<HttpClient> _lazyClient;

        private HttpClient _client => _lazyClient.Value;

        private HttpContent Jsonize(object content)
        {
            
            return new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
        }

        public ServerlessCacheClient(string ServerlessCacheEngineUrl, string ServerlessCacheKey)
        {
            _lazyClient = new Lazy<HttpClient>(() =>
            {

                var c = new HttpClient();
                c.DefaultRequestHeaders.Add("x-functions-key", ServerlessCacheKey);
                c.BaseAddress = new Uri(ServerlessCacheEngineUrl + "/api/");
                return c;
            });
        }

        public async Task<bool> StringSetAsync(string key, string value)
        {
            var resp = await _client.PostAsync(_client.BaseAddress + "string/" + HttpUtility.UrlEncode(key) + "/set", Jsonize(value));
            return resp.IsSuccessStatusCode;
        }

        public async Task<string> StringGetAsync(string key)
        {
            var resp = await _client.GetAsync(_client.BaseAddress + "string/" + HttpUtility.UrlEncode(key) + "/get");
            return await resp.Content.ReadAsStringAsync();
        }

        public async Task<bool> ListAddAsync(string key, string value)
        {
            var resp = await _client.PostAsync(_client.BaseAddress + "list/" + HttpUtility.UrlEncode(key) + "/set", Jsonize(value));
            return resp.IsSuccessStatusCode;
        }

        public async Task<List<string>> ListGetAsync(string key)
        {
            var resp = await _client.GetAsync(_client.BaseAddress + "list/" + HttpUtility.UrlEncode(key) + "/add");
            return JsonConvert.DeserializeObject<List<string>>(await resp.Content.ReadAsStringAsync());
        }

        public async Task<bool> ListContainsAsync(string key, string value)
        {
            var resp = await _client.GetAsync(_client.BaseAddress + "list/" + HttpUtility.UrlEncode(key) + "/contains");
            return JsonConvert.DeserializeObject<bool>(await resp.Content.ReadAsStringAsync());
        }
    }
}
