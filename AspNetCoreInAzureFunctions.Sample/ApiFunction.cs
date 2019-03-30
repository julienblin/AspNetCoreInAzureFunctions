using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCoreInAzureFunctions.Sample
{
    public static class ApiFunction
    {
        private static AzureFunctionsServer Server { get; } = AzureFunctionsServer.UseStartup<Startup>();

        [FunctionName("ApiFunction")]
        public static Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = "/{*proxy}")] HttpRequest request,
            ExecutionContext executionContext,
            ILogger logger)
            => Server.ProcessRequestAsync(request, executionContext, logger);
    }
}
