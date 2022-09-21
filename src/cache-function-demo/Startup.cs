using cache_function_demo;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Demo.Function.Startup))]
namespace Demo.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var redisConnection = RedisConnection.InitializeAsync(connectionString: "YOUR CONNECTION STRING HERE").GetAwaiter().GetResult();
            builder.Services.AddSingleton<RedisConnection>(redisConnection);
            builder.Services.AddHttpContextAccessor();
        }
    }
}