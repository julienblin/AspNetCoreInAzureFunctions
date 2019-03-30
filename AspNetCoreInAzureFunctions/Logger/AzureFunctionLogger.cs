using System;
using AspNetCoreInAzureFunctions.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AspNetCoreInAzureFunctions.Logger
{
    /// <summary>
    /// <see cref="ILogger"/> implementation that uses the provided Azure Function <see cref="ILogger"/>
    /// through <see cref="IAzureFunctionLoggerFeature"/>.
    /// </summary>
    public class AzureFunctionLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Func<string, LogLevel, bool> _filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionLogger"/> class.
        /// </summary>
        /// <param name="categoryName">The category name.</param>
        /// <param name="httpContextAccessor">The <see cref="IHttpContextAccessor"/></param>
        /// <param name="filter">The log filter.</param>
        public AzureFunctionLogger(string categoryName, IHttpContextAccessor httpContextAccessor, Func<string, LogLevel, bool> filter)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _filter = filter;
        }

        /// <summary>
        /// NOT SUPPORTED.
        /// </summary>
        /// <typeparam name="TState">The type of identifier for the scope.</typeparam>
        /// <param name="state">The identifier for the scope.</param>
        /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return NoOpDisposable.Instance;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter == null || _filter(_categoryName, logLevel);
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var azureFunctionLoggerFeature = _httpContextAccessor.HttpContext?.Features?.Get<IAzureFunctionLoggerFeature>();
            if (azureFunctionLoggerFeature?.Logger == null)
            {
                return;
            }

            azureFunctionLoggerFeature.Logger.Log(logLevel, eventId, state, exception, formatter);
        }

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance { get; } = new NoOpDisposable();

            public void Dispose()
            {
            }
        }
    }
}
