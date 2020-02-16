using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;

namespace CacheAPI.Models
{

    public interface ICacheList
    {
        void Add(string value);
        void Reset();
        Task<List<string>> Get();
        Task<bool> Contains(string value);
        void Delete();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CacheList : ICacheList
    {
        [JsonProperty("value")]
        public List<string> Values { get; set; }

        public void Add(string value)
        {
            if (this.Values == null)
                this.Values = new List<string>();

            if (!this.Values.Contains(value))
                this.Values.Add(value);
        }

        public void Reset() => this.Values = new List<string>();

        public async Task<List<string>> Get() => this.Values;
        public async Task<bool> Contains(string value) => this.Values.Contains(value);

        public void Delete() => Entity.Current.DeleteState();

        [FunctionName("list")]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx) => ctx.DispatchAsync<CacheList>();
       
    }
}

