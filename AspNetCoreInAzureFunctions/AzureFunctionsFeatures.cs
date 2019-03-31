using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreInAzureFunctions.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AspNetCoreInAzureFunctions
{
    /// <summary>
    /// Implementation for HTTP Features supporting the ASP.Net Core execution model.
    /// </summary>
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "HttpResponseMessage will be disposed by the functions runtime.")]
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Collection suffix conflict with standard ASP.Net core naming conventions.")]
    public sealed class AzureFunctionsFeatures
        : FeatureCollection,
        IHttpRequestFeature, IHttpResponseFeature, IHttpRequestIdentifierFeature, IHttpAuthenticationFeature,
        IHttpResponseMessageFeature, IAzureFunctionExecutionContextFeature, IAzureFunctionLoggerFeature
    {
        private readonly HttpRequest _request;
        private readonly ILogger _logger;
        private readonly ExecutionContext _executionContext;
        private readonly HttpResponseMessage _responseMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsFeatures"/> class using <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The incoming <see cref="HttpRequest"/></param>
        /// <param name="executionContext">The Azure Function <see cref="ExecutionContext"/></param>
        /// <param name="logger">The Azure Function <see cref="ILogger"/></param>
        /// <param name="claimsPrincipal">The Azure Function <see cref="ClaimsPrincipal"/></param>
        public AzureFunctionsFeatures(
            HttpRequest request,
            ExecutionContext executionContext = null,
            ILogger logger = null,
            ClaimsPrincipal claimsPrincipal = null)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _responseMessage = new HttpResponseMessage();
            Set<IHttpRequestFeature>(this);
            Set<IHttpResponseFeature>(this);
            Set<IHttpResponseMessageFeature>(this);

            if (executionContext != null)
            {
                _executionContext = executionContext;
                Set<IAzureFunctionExecutionContextFeature>(this);
                Set<IHttpRequestIdentifierFeature>(this);
                ((IHttpRequestIdentifierFeature)this).TraceIdentifier = _executionContext.InvocationId.ToString();
            }

            if (logger != null)
            {
                _logger = logger;
                Set<IAzureFunctionLoggerFeature>(this);
            }

            if (claimsPrincipal != null)
            {
                Set<IHttpAuthenticationFeature>(this);
                ((IHttpAuthenticationFeature)this).User = claimsPrincipal;
            }
        }

        /// <inheritdoc />
        string IHttpRequestFeature.Protocol { get => _request.Protocol; set => _request.Protocol = value; }

        /// <inheritdoc />
        string IHttpRequestFeature.Scheme { get => _request.Scheme; set => _request.Scheme = value; }

        /// <inheritdoc />
        string IHttpRequestFeature.Method { get => _request.Method; set => _request.Method = value; }

        /// <inheritdoc />
        string IHttpRequestFeature.PathBase { get => _request.PathBase; set => _request.PathBase = value; }

        /// <inheritdoc />
        string IHttpRequestFeature.Path { get => _request.Path; set => _request.Path = value; }

        /// <inheritdoc />
        string IHttpRequestFeature.QueryString { get => _request.QueryString.ToString(); set => _request.QueryString = QueryString.FromUriComponent(value); }

        /// <inheritdoc />
        string IHttpRequestFeature.RawTarget { get; set; }

        /// <inheritdoc />
        IHeaderDictionary IHttpRequestFeature.Headers
        {
            get => _request.Headers;
            set
            {
                _request.Headers.Clear();
                foreach (var item in value)
                {
                    _request.Headers.Add(item);
                }
            }
        }

        /// <inheritdoc />
        Stream IHttpRequestFeature.Body { get => _request.Body; set => _request.Body = value; }

        /// <inheritdoc />
        int IHttpResponseFeature.StatusCode { get => (int)_responseMessage.StatusCode; set => _responseMessage.StatusCode = (HttpStatusCode)value; }

        /// <inheritdoc />
        string IHttpResponseFeature.ReasonPhrase { get => _responseMessage.ReasonPhrase; set => _responseMessage.ReasonPhrase = value; }

        /// <inheritdoc />
        IHeaderDictionary IHttpResponseFeature.Headers { get; set; } = new HeaderDictionary();

        /// <inheritdoc />
        Stream IHttpResponseFeature.Body { get; set; } = new MemoryStream();

        /// <inheritdoc />
        public bool HasStarted { get; private set; }

        /// <inheritdoc />
        ExecutionContext IAzureFunctionExecutionContextFeature.ExecutionContext => _executionContext;

        /// <inheritdoc />
        ILogger IAzureFunctionLoggerFeature.Logger => _logger;

        /// <inheritdoc />
        string IHttpRequestIdentifierFeature.TraceIdentifier { get; set; }

        /// <inheritdoc />
        ClaimsPrincipal IHttpAuthenticationFeature.User { get; set; }

        /// <inheritdoc />
        IAuthenticationHandler IHttpAuthenticationFeature.Handler { get; set; }

        /// <inheritdoc />
        void IHttpResponseFeature.OnStarting(Func<object, Task> callback, object state)
        {
        }

        /// <inheritdoc />
        void IHttpResponseFeature.OnCompleted(Func<object, Task> callback, object state)
        {
        }

        /// <inheritdoc />
        HttpResponseMessage IHttpResponseMessageFeature.GetHttpResponseMessage()
        {
            HasStarted = true;
            var httpResponseFeature = (IHttpResponseFeature)this;

            if (httpResponseFeature.Body != null && httpResponseFeature.Body.Length > 0)
            {
                var contentLength = httpResponseFeature.Body.Length;
                httpResponseFeature.Body.Seek(0, SeekOrigin.Begin);
                _responseMessage.Content = new StreamContent(httpResponseFeature.Body);
            }

            foreach (var header in httpResponseFeature.Headers)
            {
                if (!_responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                {
                    _responseMessage.Content?.Headers?.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }

            return _responseMessage;
        }
    }
}
