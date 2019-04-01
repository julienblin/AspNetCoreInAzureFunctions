using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreInAzureFunctions.Tests.Api
{
    [ApiController]
    public class ApiController : ControllerBase
    {
        public const string ModelUri = "";

        [HttpGet(ModelUri)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiModel))]
        public IActionResult GetModel()
        {
            return Ok(new ApiModel { Name = "AspNetCoreInAzureFunctions" });
        }
    }
}
