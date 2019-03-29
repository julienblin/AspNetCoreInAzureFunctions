using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace AspNetCoreInAzureFunctions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements must be documented
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class AzureFunctionsFeatures : IFeatureCollection
    {
        private readonly HttpRequest _request;

        public AzureFunctionsFeatures(HttpRequest request)
        {
            _request = request;
        }

        public bool IsReadOnly => false;

        public int Revision => 0;

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
#pragma warning disable CA1043 // Use Integral Or String Argument For Indexers
        public object this[Type key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TFeature Get<TFeature>()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Set<TFeature>(TFeature instance)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
