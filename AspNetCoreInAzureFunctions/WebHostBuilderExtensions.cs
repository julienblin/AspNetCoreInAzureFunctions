using System.Linq;
using AspNetCoreInAzureFunctions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    /// <summary>
    /// <see cref="IWebHostBuilder"/> extensions methods
    /// </summary>
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Configure the ASP.NET Core Host to use <see cref="AzureFunctionsServer"/>
        /// (instead of Kestrel for example).
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder"/>.</param>
        /// <returns>The configured <see cref="IWebHostBuilder"/>.</returns>
        public static IWebHostBuilder UseAzureFunctionsServer(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                var serviceDescription = services.FirstOrDefault(x => x.ServiceType == typeof(IServer));
                if (serviceDescription != null)
                {
                    if (serviceDescription.ImplementationType == typeof(AzureFunctionsServer))
                    {
                        return;
                    }
                    else
                    {
                        services.Remove(serviceDescription);
                    }
                }

                services.AddSingleton<IServer, AzureFunctionsServer>();
            });
        }
    }
}
