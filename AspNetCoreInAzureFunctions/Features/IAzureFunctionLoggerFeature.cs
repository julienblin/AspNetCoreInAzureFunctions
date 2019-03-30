using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AspNetCoreInAzureFunctions.Features
{
    /// <summary>
    /// ASP.NET Core features that allows the retrieval of the Azure Function <see cref="ILogger"/>
    /// </summary>
    public interface IAzureFunctionLoggerFeature
    {
        /// <summary>
        /// Gets the Azure Function <see cref="ILogger"/>.
        /// </summary>
        ILogger Logger { get; }
    }
}
