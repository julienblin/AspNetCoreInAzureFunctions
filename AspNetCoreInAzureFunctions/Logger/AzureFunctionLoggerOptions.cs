using System;
using Microsoft.Extensions.Logging;

namespace AspNetCoreInAzureFunctions.Logger
{
    /// <summary>
    /// Options for <see cref="AzureFunctionLogger"/>.
    /// </summary>
    public class AzureFunctionLoggerOptions
    {
        /// <summary>
        /// Gets or sets the function used to filter events based on the log level.
        /// Default value is null and will instruct logger to log everything.
        /// </summary>
        public Func<string, LogLevel, bool> Filter { get; set; }
    }
}
