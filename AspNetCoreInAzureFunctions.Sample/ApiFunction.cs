using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCoreInAzureFunctions.Sample
{
    public static class ApiFunction
    {
        private static AzureFunctionsServer Server { get; } = InitializeServer();

        [FunctionName("ApiFunction")]
        public static async Task<HttpResponse> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = "/")] HttpRequest request)
        {
            var features = new AzureFunctionsFeatures(request);
            var context = Server.Application.CreateContext(features);

            Exception exception = null;
            try
            {
                await Server.Application.ProcessRequestAsync(context);
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                Server.Application.DisposeContext(context, exception);
            }

            throw new NotImplementedException();
        }

        private static AzureFunctionsServer InitializeServer()
        {
            var host = InitializeWebHost();
            host.Start();
            return host.Services.GetRequiredService<IServer>() as AzureFunctionsServer;
        }

        private static IWebHost InitializeWebHost()
        {
            return new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseAzureFunctionsServer()
                .UseStartup<Startup>()
                .Build();
        }
    }
}
