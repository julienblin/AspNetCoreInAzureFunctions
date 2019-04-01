using AspNetCoreInAzureFunctions.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;

namespace AspNetCoreInAzureFunctions.Tests.Api
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        public const string ModelUri = "";
        public const string ExecutionContextUri = "executionContext";
        public const string ClaimsPrincipalUri = "claimsPrincipal";

        [HttpGet(ModelUri)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiModel))]
        public IActionResult GetModel()
        {
            return Ok(new ApiModel { Name = "AspNetCoreInAzureFunctions" });
        }

        [HttpPost(ExecutionContextUri)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ExecutionContext))]
        public IActionResult PostExecutionContext()
        {
            var feature = HttpContext.Features.Get<IAzureFunctionExecutionContextFeature>();
            return Ok(feature.ExecutionContext);
        }

        [HttpPut(ClaimsPrincipalUri)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        public IActionResult PutClaimsPrincipal()
        {
            return Ok(HttpContext.User.Identity.Name);
        }
    }
}
