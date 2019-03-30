using AspNetCoreInAzureFunctions.Features;
using AspNetCoreInAzureFunctions.Logger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// <see cref="ILoggingBuilder"/> extensions methods.
    /// </summary>
    public static class LoggingBuilderExtensions
    {
        /// <summary>
        /// Add a logger that uses the provided Azure Function <see cref="ILogger"/>
        /// through <see cref="IAzureFunctionLoggerFeature"/>.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/>.</param>
        /// <returns>The same <see cref="ILoggingBuilder"/></returns>
        public static ILoggingBuilder AddAzureFunction(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AzureFunctionLoggerProvider>());
            return builder;
        }
    }
}
