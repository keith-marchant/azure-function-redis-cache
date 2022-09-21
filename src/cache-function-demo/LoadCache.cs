using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Web.Http;
using cache_function_demo;

namespace Demo.Function
{
    public class CacheFunctions
    {
        private readonly RedisConnection _redisConnection;

        public CacheFunctions(RedisConnection redisConnection)
        {
            _redisConnection = redisConnection;
        }

        [FunctionName("LoadCache")]
        public async Task<IActionResult> LoadCache(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("LoadCache function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string name = data?.name;
            string value = data?.value;

            var result = await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(name, new RedisValue(value)));

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        [FunctionName("ReadCache")]
        public async Task<IActionResult> ReadCache(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("ReadCache function processed a request.");

                string name = req.Query["name"];
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                name = name ?? data?.name;

                var result = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(name));

                string responseMessage = string.IsNullOrEmpty((string)result)
                    ? "This HTTP triggered function executed successfully. Could not find a cached value."
                    : $"Hello, {(string)result}. This HTTP triggered function executed successfully.";

                return new OkObjectResult(responseMessage);
            }
            catch(Exception ex)
            {
                return new InternalServerErrorResult();
            }
        }
    }
}
