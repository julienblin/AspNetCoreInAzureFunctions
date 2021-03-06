﻿using Microsoft.Azure.WebJobs;

namespace AspNetCoreInAzureFunctions.Features
{
    /// <summary>
    /// ASP.NET Core features that allows the retrieval of the Azure Function <see cref="ExecutionContext"/>
    /// </summary>
    public interface IAzureFunctionExecutionContextFeature
    {
        /// <summary>
        /// Gets the Azure Function <see cref="ExecutionContext"/>.
        /// </summary>
        ExecutionContext ExecutionContext { get; }
    }
}
