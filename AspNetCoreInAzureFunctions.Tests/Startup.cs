using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace AspNetCoreInAzureFunctions.Tests
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Default ASP.Net Core startup class.")]
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new Info { Title = $"UnitTests", Version = typeof(Startup).Assembly.GetName().Version.ToString() });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
            app.UseSwagger();
        }
    }
}
