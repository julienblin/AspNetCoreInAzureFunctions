using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreInAzureFunctions.Sample
{
    [ApiController]
    public class SampleController : ControllerBase
    {
        [HttpGet("info")]
        public IActionResult GetInfo([FromQuery] string name = null)
        {
            return Ok(new { Info = $"Hello, {name ?? "world"}" });
        }
    }
}
