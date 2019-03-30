using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCoreInAzureFunctions.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace AspNetCoreInAzureFunctions
{
    /// <summary>
    /// Implementation for HTTP Features supporting the ASP.Net Core execution model.
    /// </summary>
    [SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "HttpResponseMessage will be disposed by the functions runtime.")]
    [SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Collection suffix conflict with standard ASP.Net core naming conventions.")]
    public sealed class AzureFunctionsFeatures : IFeatureCollection, IHttpRequestFeature, IHttpResponseFeature, IHttpResponseMessageFeature
    {
        private readonly IDictionary<Type, object> _features = new Dictionary<Type, object>();
        private readonly HttpRequest _request;
        private readonly HttpResponseMessage _responseMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureFunctionsFeatures"/> class using <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The incoming <see cref="HttpRequest"/></param>
        public AzureFunctionsFeatures(HttpRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _responseMessage = new HttpResponseMessage();

            _features[typeof(IHttpRequestFeature)] = this;
            _features[typeof(IHttpResponseFeature)] = this;
            _features[typeof(IHttpResponseMessageFeature)] = this;
        }

        /// <inheritdoc />
        bool IFeatureCollection.IsReadOnly => false;

        /// <inheritdoc />
        int IFeatureCollection.Revision => 0;

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
        object IFeatureCollection.this[Type key]
        {
            get
            {
                if (_features.TryGetValue(key, out var feature))
                {
                    return feature;
                }

                return null;
            }

            set
            {
                _features[key] = value;
            }
        }

        /// <inheritdoc />
        TFeature IFeatureCollection.Get<TFeature>()
        {
            return (TFeature)((IFeatureCollection)this)[typeof(TFeature)];
        }

        /// <inheritdoc />
        IEnumerator<KeyValuePair<Type, object>> IEnumerable<KeyValuePair<Type, object>>.GetEnumerator() => _features.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => _features.GetEnumerator();

        /// <inheritdoc />
        void IFeatureCollection.Set<TFeature>(TFeature instance) => _features[typeof(TFeature)] = instance;

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

            if (httpResponseFeature.Body != null)
            {
                var contentLength = httpResponseFeature.Body.Length;
                httpResponseFeature.Body.Seek(0, SeekOrigin.Begin);
                _responseMessage.Content = new StreamContent(httpResponseFeature.Body);
                _responseMessage.Content.Headers.ContentLength = contentLength;
            }

            foreach (var header in httpResponseFeature.Headers)
            {
                _responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                _responseMessage.Content?.Headers?.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            return _responseMessage;
        }
    }
}
