# ASP.NET Core In Azure Functions

Run ASP.NET Core apps in Azure Functions.

AspNetCoreInAzureFunctions allows you to create an Azure Function V2 app/HTTP endpoint that can host a completely functional ASP.NET Core app.

## Caveat emptor

!! Be warned though, as it is **not the most performant way** to host an ASP.NET Core app, as:

- It seems that the [Azure Functions Runtime V2 for .Net Core](https://github.com/Azure/azure-functions-host/wiki/Azure-Functions-Runtime-2.0-Overview) runs at least a partial ASP.NET Core stack (see the project references); this project enables you to run an additional ASP.NET Core stack **on top of** the existing one and provides a bridge between the 2; as such, some efforts and computations are clearly duplicated during the function execution
- Due to the nature of Azure Functions, the response from the ASP.NET Core stack is buffered in-memory, and only returned to the Functions host when the ASP.NET Core stack has finished execution (in the form of an `HttpResponseMessage`). This is less efficient than the execution model when running in Kestrel or IIS where the response content is streamed back directly to the server. This only gets worse as the execution time and response size increase (more wait time for the Azure Functions runtime before starting to send the response and increased memory consumption/GC cycles)
- This is really a lot more suitable when hosting APIs (as opposed to Server-side rendered web app), as the execution model for Azure Functions is very inefficient when it comes to serving web pages resources (multiple connections, static files hosting, etc.).
- The observed cold start in the Azure Functions consumption plan right now is also not great and may be inappropriate depending on your use case; [YMMV](https://en.wiktionary.org/wiki/YMMV).

## Get Started

1. [Create a new Azure Functions V2 project with an HTTP trigger](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs#create-an-azure-functions-project):

   - Configure the Access rights with "Anonymous" for now

2. Add a reference to the `Microsoft.AspNetCore.App` [NuGet package](https://www.nuget.org/packages/Microsoft.AspNetCore.App)

   1. Choose the appropriate version of the package based on the .Net Core runtime version of the app.

3. Add a reference to the `AspNetCoreInAzureFunctions ` NuGet package

4. Create the baseline ASP.NET Core app in the project:

   1. Add a new class named `Startup.cs` file with the following content:

      ```c#
      using Microsoft.AspNetCore.Builder;
      using Microsoft.Extensions.DependencyInjection;
      
      public class Startup
      {
      	public void ConfigureServices(IServiceCollection services)
      	{
      		services.AddMvc();
      	}
      
      	public void Configure(IApplicationBuilder app)
      	{
      		app.UseMvc();
      	}
      }
      ```
   2. Add a new controller with the following content:
      ```c#
      using Microsoft.AspNetCore.Mvc;
      
      [ApiController]
      public class SampleController : ControllerBase
      {
      	[HttpGet("hello")]
      	public IActionResult Hello([FromQuery] string name)
      	{
      		return Ok($"Hello, {name ?? "world"}");
      	}
      }
      ```

5. Update the code in the Azure Functions to look like this:
   ```c#
   using System.Threading.Tasks;
   using Microsoft.Azure.WebJobs;
   using Microsoft.Azure.WebJobs.Extensions.Http;
   using Microsoft.AspNetCore.Http;
   using AspNetCoreInAzureFunctions;
   using System.Net.Http;
   
   public static class Function1
   {
       // Initialize ASP.Net Core Server using the Startup class.
   	private static AzureFunctionsServer Server { get; } = AzureFunctionsServer.UseStartup<Startup>();
   
   	[FunctionName("Function1")]
       public static Task<HttpResponseMessage> Run(
   		[HttpTrigger(AuthorizationLevel.Anonymous, Route = "/{*proxy}")] HttpRequest req)
   	{
           return Server.ProcessRequestAsync(req);
   	}
   }
   ```

6. Update your `host.json` file to remove the `/api` prefix for HTTP trigger:
   ```json
   {
     "version": "2.0",
     "extensions": {
       "http": {
         "routePrefix": ""
       }
     }
   }
   ```
7. Run !
   ```shell
   curl http://localhost:7071/hello
   ```

At this point, any standard feature of ASP.NET Core should work.

## Advanced features

### Access the Azure Function ExecutionContext

1. Update the Azure Function signature to pass the `ExecutionContext`

   ```c#
   [FunctionName("Function1")]
   public static Task<HttpResponseMessage> Run(
   [HttpTrigger(AuthorizationLevel.Anonymous, Route = "/{*proxy}")] HttpRequest req,
   ExecutionContext executionContext)
   {
   return Server.ProcessRequestAsync(req, executionContext: executionContext);
   }
   ```

2. Get the `IAzureFunctionExecutionContextFeature` feature from the HttpContext in your controller code:

   ```c#
   var azureFunctionExecutionContextFeature = HttpContext.Features.Get<IAzureFunctionExecutionContextFeature>();
   			var azureFunctionExecutionContext = azureFunctionExecutionContextFeature.ExecutionContext;
   ```

*Additionally, the ASP.NET Core `HttpContext.TraceIdentifier` will be set to the Azure Function `ExecutionContext.InvocationId`.*

### Log with Azure Function Logger

Update the Azure Function signature to pass the  Azure Function `ILogger`

```c#
[FunctionName("Function1")]
public static Task<HttpResponseMessage> Run(
[HttpTrigger(AuthorizationLevel.Anonymous, Route = "/{*proxy}")] HttpRequest req,
ILogger logger)
{
return Server.ProcessRequestAsync(req, logger: logger);
}
```

All ASP.NET Core loggers are then forwarded to the Azure Functions logger.

To customize the log level, use the standard ASP.NET Core configuration standard, e.g. using environment variables edit your `local.settings.json` and set:

```json
{
  ...
  "Values": {
    ...
    "Logging:LogLevel:Default": "Error"
  }
}
```

### Pass the Azure Function Authentication information

Update the Azure Function signature to pass the  Azure Function `ILogger`

```c#
[FunctionName("Function1")]
public static Task<HttpResponseMessage> Run(
[HttpTrigger(AuthorizationLevel.Anonymous, Route = "/{*proxy}")] HttpRequest req,
ClaimsPrincipal claimsPrincipal
ILogger logger)
{
return Server.ProcessRequestAsync(req, claimsPrincipal: claimsPrincipal);
}
```

The `ClaimsPrincipal` is then accessible in the controllers and authentication sub-system through the standard `HttpContext.User` property.

### Customize the ASP.NET Core Web Host

The following line in your Azure Functions:

```c#
private static AzureFunctionsServer Server { get; } = AzureFunctionsServer.UseStartup<Startup>();
```

is functionally equivalent to the standard ASP.NET Core line in `Program.cs`:

```c#
WebHost.CreateDefaultBuilder()
       .UseStartup<Startup>()
       .Build();
```

It initialize and starts a `IWebHost` instance with the following configuration:

- Use current directory as content root
- Use `appsettings.*.json` and environment variables as configuration source
- Use user secrets configuration in development
- Configure logging with Azure Function logger using Logging configuration section

To customize, use the overload that gives you an instance of `IWebHostBuilder`:

```c#
private static AzureFunctionsServer Server { get; } = AzureFunctionsServer.UseStartup<Startup>(builder =>
	{
		builder
			.UseContentRoot(Directory.GetCurrentDirectory())
			.ConfigureLogging((hostingContext, logging) =>
			{
				logging.AddConsole();
			});
	});
```

## How does it work?

This project does NOT use Kestrel and does NOT run a web server in the Azure Function.

It provides an alternative [`IServer`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.server.iserver) implementation that accepts the Azure Function `HttpRequest` and allows returning a `HttpResponseMessage` suitable for Azure Functions response.

It provides the following [ASP.NET Core features](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/request-features):

- `IHttpRequestFeature`, `IHttpResponseFeature`, `IHttpRequestIdentifierFeature`, `IHttpAuthenticationFeature`: standard ASP.NET Core features
- `IHttpResponseMessageFeature`: Allows the retrieval of a `HttpResponseMessage` from the current context
- `IAzureFunctionExecutionContextFeature`: Allow access to the Azure Function `ExecutionContext`
- `IAzureFunctionLoggerFeature`: Allow access to the current Azure Function `ILogger`

## Inspiration

- AWS Lambda ASP.NET Core template: <https://github.com/aws/aws-lambda-dotnet>

