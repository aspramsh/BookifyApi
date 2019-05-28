using AutoMapper;
using Bookify.Business.Models.Request;
using Bookify.Business.Services.Interfaces;
using Bookify.Business.Settings;
using Bookify.Infrastructure.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mime;
using System.Threading.Tasks;

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
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly EmailVerificationSettings _emailVerificationSettings;
        private readonly AuthSettings _authSettings;

        public UsersController(ILoggerFactory loggerFactory,
                               IMapper mapper,
                               IUserService userService,
                               IEmailSender emailSender,
                               IEmailTemplateService emailTemplateService,
                               IOptions<EmailVerificationSettings> emailVerificationSettings,
                               IOptions<AuthSettings> authSettings)
            : base(loggerFactory.CreateLogger<UsersController>(), mapper)
        {
            _logger = loggerFactory.CreateLogger<UsersController>();
            _userService = userService;
            _emailTemplateService = emailTemplateService;
            _emailVerificationSettings = emailVerificationSettings.Value;
            _emailSender = emailSender;
            _authSettings = authSettings.Value;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync([FromBody] RequestRegisterViewModel model)
        {
            var userModel = await _userService.CreateUserAsync(model);

            var emailTemplateName = EmailTemplateTypes.UserRegistration.ToString();

            var link =
                $"{_emailVerificationSettings.BaseUrl}/{_emailVerificationSettings.UserVerificationAddress}/{userModel.EncodedToken}";

            var email = _emailVerificationSettings.EmailAddress;

            var emailTemplate = await _emailTemplateService.GetSingleAsync(x => x.Name == emailTemplateName);

            // send email
            await _emailSender.SendEmailAsync(userModel.Email, emailTemplate.Subject,
                string.Format(emailTemplate.Body, link, email));

            return Ok(userModel);
        }

        /// <summary>
        /// Verification of user
        /// </summary>
        /// <param name="token">Base64 token</param>
        /// <returns>No Content if success</returns>
        [AllowAnonymous]
        [HttpPut("verifications/{token}")]
        public async Task<IActionResult> UserVerificationAsync(string token)
        {
            await _userService.VerifyUserEmailAsync(token);

            return NoContent();
        }

        /// <summary>
        /// User log for token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("accessToken")]
        public async Task<IActionResult> GetTokenResponseAsync([FromBody]RequestLoginViewModel model)
        {
            var result = await _userService.GetTokenResponseAsync(model, _authSettings);

            _logger.LogInformation("User logged in.");

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("authorization")]
        public async Task<IActionResult> LoginAsync([FromBody]RequestLoginViewModel model)
        {
            var response = await _userService.LoginAsync(model);

            return Ok(response);
        }
    }
}