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
        /// Initializes a new instance of the <see cref="AzureFunctionsServer"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> injected via DI.</param>
        public AzureFunctionsServer(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Gets the <see cref="IHttpApplication{TContext}" /> that processes the requests.
        /// </summary>
        public IHttpApplication<HostingApplication.Context> Application { get; private set; }

        /// <summary>
        /// Gets the root <see cref="IServiceProvider"/>.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <inheritdoc />
        IFeatureCollection IServer.Features { get; } = new FeatureCollection();

        /// <summary>
        /// Configure and start the ASP.Net Core <see cref="IWebHost"/>
        /// and returns a ready to run <see cref="AzureFunctionsServer"/>.
        /// </summary>
        /// <typeparam name="TStartup">The ASP.Net Core Startup class to use.</typeparam>
        /// <param name="webHostBuilder">
        /// Allow the customization of the <see cref="IWebHostBuilder"/> / <see cref="IWebHost"/>.
        /// If not provided, the defaults are:
        /// <list type="bullet">
        ///     <item>Use current directory as content root</item>
        ///     <item>Use only environment variables as configuration source</item>
        ///     <item>Configure logging with Console logger using Logging configuration section.</item>
        /// </list>
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
        /// <param name="executionContext">The Azure Function <see cref="Microsoft.Azure.WebJobs.ExecutionContext"/></param>
        /// <returns>The final <see cref="HttpResponseMessage"/>.</returns>
        public async Task<HttpResponseMessage> ProcessRequestAsync(HttpRequest request, Microsoft.Azure.WebJobs.ExecutionContext executionContext = null)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (Application == null)
            {
                throw new Exception($"The {nameof(Application)} hasn't been initialized. Make sure that the host has started prior to calling {nameof(ProcessRequestAsync)}");
            }

            var features = new AzureFunctionsFeatures(request, executionContext);
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

            var responseMessage = context.HttpContext.Features.Get<IHttpResponseMessageFeature>()
                .GetHttpResponseMessage();

            Application.DisposeContext(context, null);

            return responseMessage;
        }

        /// <summary>
        /// Executes <paramref name="execution"/> in a scoped <see cref="IServiceProvider"/>.
        /// To be used by other Azure Functions in the Function App that are not triggered by HTTP.
        /// </summary>
        /// <param name="execution">The function to execute.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task ExecuteInScope(Func<IServiceProvider, Task> execution)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                await execution(scope.ServiceProvider);
            }
        }

        /// <summary>
        /// Executes <paramref name="execution"/> in a scoped <see cref="IServiceProvider"/>.
        /// To be used by other Azure Functions in the Function App that are not triggered by HTTP.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="execution">The function to execute.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task<T> ExecuteInScope<T>(Func<IServiceProvider, Task<T>> execution)
        {
            using (var scope = ServiceProvider.CreateScope())
            {
                return await execution(scope.ServiceProvider);
            }
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
