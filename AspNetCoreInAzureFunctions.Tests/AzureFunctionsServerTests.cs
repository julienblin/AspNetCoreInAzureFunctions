using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCoreInAzureFunctions.Tests.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Xunit;

namespace AspNetCoreInAzureFunctions.Tests
{
    [Collection(AzureFunctionsServerCollection.Name)]
    public class AzureFunctionsServerTests
    {
        private readonly AzureFunctionsServerFixture _fixture;

        public AzureFunctionsServerTests(AzureFunctionsServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ItShouldGetNotFound()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "GET",
                Path = "/notfound",
            };

            var response = await _fixture.Server.ProcessRequestAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ItShouldGetModel()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "GET",
                Path = $"/{ApiController.ModelUri}",
            };

            var response = await _fixture.Server.ProcessRequestAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var model = await response.Content.ReadAsAsync<ApiModel>();
            model.Name.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ItShouldGetSwagger()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "GET",
                Path = $"/swagger/v1/swagger.json",
            };

            var response = await _fixture.Server.ProcessRequestAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
