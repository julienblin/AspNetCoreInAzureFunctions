using System;

namespace AspNetCoreInAzureFunctions.Tests
{
    public class AzureFunctionsServerFixture : IDisposable
    {
        public AzureFunctionsServerFixture()
        {
            Server = AzureFunctionsServer.UseStartup<Startup>();
        }

        public AzureFunctionsServer Server { get; }

        public void Dispose()
        {
            Server?.Dispose();
        }
    }
}
