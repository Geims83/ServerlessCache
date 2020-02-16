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

namespace CacheAPI.Models
{

    public interface ICacheInt
    {
        void Add(long amount);
        void Set(long value);
        Task<long> Get();
        void Delete();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CacheInt : ICacheInt
    {
        [JsonProperty("value")]
        public long CurrentValue { get; set; }

        public void Add(long amount) => this.CurrentValue += amount;

        public void Set(long value) => this.CurrentValue = value;

        public async Task<long> Get() => this.CurrentValue;

        public void Delete() => Entity.Current.DeleteState();

        [FunctionName("int")]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<CacheInt>();
    }
}

