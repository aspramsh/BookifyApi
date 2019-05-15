using AutoMapper;
using Bookify.Business.Services.Interfaces;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Mime;

namespace BookifyApi.Controllers
{
    /// <summary>
    /// Users controller
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [EnableCors("CorsPolicy")]
    [Produces(MediaTypeNames.Application.Json)]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly IEmailSender _emailSender;

        public UsersController(ILoggerFactory loggerFactory,
                               IMapper mapper,
                               IUserService userService,
                               IEmailSender emailSender)
            : base(loggerFactory.CreateLogger<UsersController>(), mapper)
        {
            _logger = loggerFactory.CreateLogger<UsersController>();
            _userService = userService;
        }
    }
}