using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AspNetCoreInAzureFunctions.Sample
{
    [ApiController]
    public class SampleController : ControllerBase
    {
        [HttpGet("")]
        [SwaggerOperation(Summary = "Simple index response")]
        public IActionResult Index()
        {
            return Ok(new { Message = "Index" });
        }

        [HttpGet("hello")]
        [SwaggerOperation(Summary = "Hello world!")]
        public IActionResult Hello([FromQuery] string name = null)
        {
            return Ok(new { Info = $"Hello, {name ?? "world"}" });
        }
    }
}
