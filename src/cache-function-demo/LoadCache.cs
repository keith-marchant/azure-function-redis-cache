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
            try
            {
                log.LogInformation("LoadCache function processed a request.");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                string name = data?.name;
                string value = data?.value;

                var result = await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(name, new RedisValue(value)));

                return new NoContentResult();
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Failed to save value");
                return new InternalServerErrorResult();
            }
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
                name ??= data?.name;

                var result = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(name));

                return string.IsNullOrEmpty((string)result) ? new NotFoundResult() : new OkObjectResult(new { value = (string)result});
            }
            catch(Exception ex)
            {
                log.LogError("Failed to read value");
                return new InternalServerErrorResult();
            }
        }
    }
}
