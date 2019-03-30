using System.Net.Http;

namespace AspNetCoreInAzureFunctions.Features
{
    /// <summary>
    /// ASP.NET Core features that allows the retrieval of the response as <see cref="HttpResponseMessage"/>
    /// </summary>
    public interface IHttpResponseMessageFeature
    {
        /// <summary>
        /// Gets the response as <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        HttpResponseMessage GetHttpResponseMessage();
    }
}
