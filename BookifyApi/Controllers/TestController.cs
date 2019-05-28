using System.Net.Mime;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookifyApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Policy = "Admin")]
    [EnableCors("CorsPolicy")]
    [Produces(MediaTypeNames.Application.Json)]
    public class TestController : BaseController
    {
        public TestController(ILoggerFactory loggerFactory, IMapper mapper)
            : base(loggerFactory.CreateLogger<TestController>(), mapper)
        {
        }

        [HttpGet]
        public async Task<IActionResult> TestAccessTokenAsync()
        {
            await Task.Delay(1000);

            return Ok("Success!");
        }
    }
}