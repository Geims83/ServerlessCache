using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CacheAPI.Models;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using System.Dynamic;
using System.Web;

namespace CacheAPI
{
    public static class Functions
    {
        [FunctionName("Main")]
        public static async Task<IActionResult> Main(
            [HttpTrigger(AuthorizationLevel.Admin, "get", "post", Route = "{type}/{entityKey}/{operation?}")] HttpRequest req,
            [DurableClient] IDurableEntityClient client,
            string type,
            string entityKey,
            string operation,
            ILogger log)
        {
            if (String.IsNullOrEmpty(operation))
                operation = "get";

            log.LogInformation($"{type}/{entityKey}/{operation}");

            type = type.ToLower();
            operation = HttpUtility.UrlDecode(operation).ToLower();

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(requestBody);

            var permittedTypes = new List<string>() { "int", "string", "list" };

            if (!permittedTypes.Contains(type))
                return new BadRequestObjectResult($"Error: Type unrecognized, permitted values are {JsonConvert.SerializeObject(permittedTypes)}");

            var entityId = new EntityId(type, entityKey);

            if (type == "int")
            {
                var proxyState = await client.ReadEntityStateAsync<CacheInt>(entityId);
                //var proxy = context.CreateEntityProxy<ICacheInt>(entityId);
                if (!proxyState.EntityExists && operation != "set")
                    return new BadRequestObjectResult($"Error: Entity {type}/{entityKey} does not exists");

                var proxy = proxyState.EntityState;
                switch (operation)
                {
                    case "get":
                        return new OkObjectResult(await proxy.Get());
                    case "set":
                        await client.SignalEntityAsync<ICacheInt>(entityId, p => p.Set(data.value));
                        return new OkResult();
                    case "add":
                        await client.SignalEntityAsync<ICacheInt>(entityId, p => p.Add(data.value));
                        return new OkResult();
                    case "delete":
                        await client.SignalEntityAsync<ICacheInt>(entityId, p => p.Delete());
                        return new OkResult();
                }
            }
            else if (type == "string")
            {
                var proxyState = await client.ReadEntityStateAsync<CacheString>(entityId);
                //var proxy = context.CreateEntityProxy<ICacheInt>(entityId);
                if (!proxyState.EntityExists && operation != "set")
                    return new BadRequestObjectResult($"Error: Entity {type}/{entityKey} does not exists");

                var proxy = proxyState.EntityState;
                switch (operation)
                {
                    case "get":
                        return new OkObjectResult(await proxy.Get());
                    case "set":
                        await client.SignalEntityAsync<ICacheString>(entityId, p => p.Set(data.value));
                        return new OkResult();
                    case "delete":
                        await client.SignalEntityAsync<ICacheString>(entityId, p => p.Delete());
                        return new OkResult();
                }
            }
            else if (type == "list")
            {
                var proxyState = await client.ReadEntityStateAsync<CacheList>(entityId);
                //var proxy = context.CreateEntityProxy<ICacheInt>(entityId);
                if (!proxyState.EntityExists && operation != "add")
                    return new BadRequestObjectResult($"Error: Entity {type}/{entityKey} does not exists");

                var proxy = proxyState.EntityState;
                switch (operation)
                {
                    case "contains":
                        return new OkObjectResult(await proxy.Contains(data.value));
                    case "get":
                        return new OkObjectResult(await proxy.Get());
                    case "add":
                        await client.SignalEntityAsync<ICacheList>(entityId, p => p.Add(data.value));
                        return new OkResult();
                    case "delete":
                        await client.SignalEntityAsync<ICacheList>(entityId, p => p.Delete());
                        return new OkResult();
                }
            }

            return new BadRequestObjectResult("Error: WTF?");
        }
    }
}
