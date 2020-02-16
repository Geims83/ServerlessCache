using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace CacheAPI.Models
{

    public interface ICacheString
    {
        void Set(String value);
        void Reset();
        Task<string> Get();
        void Delete();
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CacheString : ICacheString
    {
        [JsonProperty("value")]
        public string CurrentValue { get; set; }

        public void Set(string value) => this.CurrentValue = value;

        public void Reset() => this.CurrentValue = "";

        public async Task<string> Get() => this.CurrentValue;

        public void Delete() => Entity.Current.DeleteState();

        [FunctionName("string")]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<CacheString>();
    }
}

