using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCoreInAzureFunctions.Tests.Api;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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

        [Fact]
        public async Task ItShouldGetExecutionContext()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "POST",
                Path = $"/{ApiController.ExecutionContextUri}",
            };

            var executionContext = new ExecutionContext();

            var response = await _fixture.Server.ProcessRequestAsync(request, executionContext: executionContext);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var model = await response.Content.ReadAsAsync<ExecutionContext>();
            model.Should().BeEquivalentTo(executionContext);
        }

        [Fact]
        public async Task ItShouldLog()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "GET",
                Path = $"/{ApiController.ModelUri}",
            };

            var logger = new TestLogger();

            var response = await _fixture.Server.ProcessRequestAsync(request, logger: logger);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            logger.Messages.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ItShouldHandleClaimsPrincipal()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = "PUT",
                Path = $"/{ApiController.ClaimsPrincipalUri}",
            };

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "John Doe") }));

            var response = await _fixture.Server.ProcessRequestAsync(request, claimsPrincipal: claimsPrincipal);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var name = await response.Content.ReadAsStringAsync();
            name.Should().Be(claimsPrincipal.Identity.Name);
        }

        private class TestLogger : ILogger
        {
            public IList<object> Messages { get; } = new List<object>();

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Messages.Add(state);
            }
        }
    }
}
