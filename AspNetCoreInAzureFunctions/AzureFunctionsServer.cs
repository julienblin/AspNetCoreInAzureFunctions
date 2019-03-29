using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

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
        public IFeatureCollection Features { get; } = new FeatureCollection();

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            Application = application as IHttpApplication<HostingApplication.Context>;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
