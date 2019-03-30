using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreInAzureFunctions.Sample
{
    [ApiController]
    public class SampleController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return Ok(new { Message = "Index" });
        }

        [HttpGet("info")]
        public IActionResult GetInfo([FromQuery] string name = null)
        {
            return Ok(new { Info = $"Hello, {name ?? "world"}" });
        }
    }
}
