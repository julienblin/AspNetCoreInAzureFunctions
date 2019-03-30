using System;
using AspNetCoreInAzureFunctions.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCoreInAzureFunctions.Logger
{
    /// <summary>
    /// <see cref="ILoggerProvider"/> that uses the provided Azure Function <see cref="ILogger"/>
    /// through <see cref="IAzureFunctionLoggerFeature"/>.
    /// </summary>
    public sealed class AzureFunctionLoggerProvider : ILoggerProvider
    {
        private readonly IOptionsMonitor<AzureFunctionLoggerOptions> _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionLoggerProvider"/> class.
        /// </summary>
        /// <param name="options">The log filter.</param>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/></param>
        public AzureFunctionLoggerProvider(IOptionsMonitor<AzureFunctionLoggerOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string categoryName)
        {
            return new AzureFunctionLogger(categoryName, _httpContextAccessor, _options.CurrentValue.Filter);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}
