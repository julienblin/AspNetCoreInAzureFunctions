using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreInAzureFunctions.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AspNetCoreInAzureFunctions
{
    /// <summary>
    /// <see cref="IServer"/> implementation for Azure Functions.
    /// </summary>
    public sealed class AzureFunctionsServer : IServer
    {
        /// <summary>
        /// Gets or sets the <see cref="IHttpApplication{TContext}" /> that processes the requests.
        /// </summary>
        public IHttpApplication<HostingApplication.Context> Application { get; set; }

        /// <inheritdoc />
        IFeatureCollection IServer.Features { get; } = new FeatureCollection();

        /// <summary>
        /// Configure and start the ASP.Net Core <see cref="IWebHost"/>
        /// and returns the ready to run <see cref="AzureFunctionsServer"/>.
        /// </summary>
        /// <typeparam name="TStartup">The ASP.Net Core Startup class to use.</typeparam>
        /// <param name="webHostBuilder">
        /// Allow the customization of the <see cref="IWebHostBuilder"/> / <see cref="IWebHost"/>.
        /// If not provided, the defaults are:
        ///   - Use current directory as content root
        ///   - Use only environment variables as configuration source
        ///   - Configure logging with Console logger using Logging configuration section.
        /// </param>
        /// <returns>Ready-to-run <see cref="AzureFunctionsServer"/>.</returns>
        public static AzureFunctionsServer UseStartup<TStartup>(Action<IWebHostBuilder> webHostBuilder = null)
            where TStartup : class
        {
            var hostBuilder = new WebHostBuilder();
            if (webHostBuilder != null)
            {
                webHostBuilder(hostBuilder);
            }
            else
            {
                hostBuilder
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        config.AddEnvironmentVariables();
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                    });
            }

            var host = hostBuilder
                .UseAzureFunctionsServer()
                .UseStartup<TStartup>()
                .Build();

            host.Start();
            return host.Services.GetRequiredService<IServer>() as AzureFunctionsServer;
        }

        /// <summary>
        /// Processes the <paramref name="request"/> incoming from the Azure Function
        /// through the ASP.NET Core stack (via <see cref="Application"/>
        /// and produces a <see cref="HttpResponseMessage"/> suitable for Azure Function response.
        /// </summary>
        /// <param name="request">The incoming <see cref="HttpRequest"/>.</param>
        /// <returns>The final <see cref="HttpResponseMessage"/>.</returns>
        public async Task<HttpResponseMessage> ProcessRequestAsync(HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (Application == null)
            {
                throw new Exception($"The {nameof(Application)} hasn't been initialized. Make sure that the host has started prior to calling {nameof(ProcessRequestAsync)}");
            }

            var features = new AzureFunctionsFeatures(request);
            var context = Application.CreateContext(features);

            try
            {
                await Application.ProcessRequestAsync(context);
            }
            catch (Exception ex)
            {
                Application.DisposeContext(context, ex);
                throw;
            }

            Application.DisposeContext(context, null);

            return context.HttpContext.Features.Get<IHttpResponseMessageFeature>()
                .GetHttpResponseMessage();
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        Task IServer.StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            Application = application as IHttpApplication<HostingApplication.Context>;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        Task IServer.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
