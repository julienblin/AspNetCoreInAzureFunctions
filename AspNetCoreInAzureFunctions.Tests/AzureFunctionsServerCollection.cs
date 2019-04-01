using Xunit;

namespace AspNetCoreInAzureFunctions.Tests
{
    [CollectionDefinition(AzureFunctionsServerCollection.Name)]
    public class AzureFunctionsServerCollection : ICollectionFixture<AzureFunctionsServerFixture>
    {
        public const string Name = "AzureFunctionsServer";
    }
}
