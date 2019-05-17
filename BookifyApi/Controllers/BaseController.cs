using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BookifyApi.Controllers
{
    /// <summary>
    /// Base Controller
    /// </summary>
    public class BaseController : ControllerBase
    {
        protected ILogger Logger;
        protected IMapper Mapper;

        protected BaseController(ILogger logger, IMapper mapper)
        {
            Logger = logger;
            Mapper = mapper;
        }
    }
}