using AutoMapper;
using Bookify.Business.Models;
using Bookify.Business.Models.Request;
using Bookify.Business.Models.Response;
using Bookify.Business.Services.Interfaces;
using Bookify.Business.Settings;
using Bookify.DataAccess.Entities.Identity;
using Bookify.Infrastructure.Enums;
using Bookify.Infrastructure.Http;
using IdentityModel.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;

namespace Bookify.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        //TODO: Use this in email confirm functionality
        private const int UserRegistrationExpirationHours = 24;

        public UserService(IMapper mapper,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<UserModel> CreateUserAsync(RequestRegisterViewModel model)
        {
            var checkedUser = await _userManager.FindByEmailAsync(model.Email);

            if (checkedUser != null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest,
                    new ResponseErrorModel("User already exists."));
            }

            var user = _mapper.Map<User>(model);
            var role = await _roleManager.Roles.FirstOrDefaultAsync(x => x.Name == RolesEnum.User.ToString());

            if (role == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound, new ResponseErrorModel("The role does not exist"));
            }

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(user, role.Name);

                if (roleResult.Succeeded)
                {
                    var claimResult = await _userManager.AddClaimAsync(user, new Claim("Role", "User"));

                    if (claimResult.Succeeded)
                    {
                        return _mapper.Map<UserModel>(user);
                    }
                }

                throw new HttpResponseException(HttpStatusCode.BadRequest,
                    new ResponseErrorModel(roleResult.Errors.Select(x => x.Description).ToArray()));
            }

            throw new HttpResponseException(HttpStatusCode.BadRequest,
                new ResponseErrorModel(result.Errors.Select(x => x.Description).ToArray()));
        }

        /// <summary>
        /// Verify the user by emailed link
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task VerifyUserEmailAsync(string token)
        {
            string tokenGuidString = default;

            try
            {
                tokenGuidString = Base64UrlEncoder.Decode(token);
            }
            catch
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, new ResponseErrorModel("CryptoRates.Users.UserVerification.BadRequest.InvalidConfirmationURL.Base64UrlEncoder"));
            }

            if (!Guid.TryParse(tokenGuidString, out var tokenGuid))
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, new ResponseErrorModel("CryptoRates.Users.UserVerification.BadRequest.InvalidConfirmationURL.Guid.TryParse"));
            }

            var user = await _userManager.Users.SingleOrDefaultAsync(x => x.Token == tokenGuid);

            if (user == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, new ResponseErrorModel("Unable to load the user."));
            }

            if (user.TokenCreatedDateTimeUtc?.AddHours(UserRegistrationExpirationHours) < DateTime.UtcNow)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, new ResponseErrorModel("CryptRates.UserService.UserVerification.Unauthorized.TokenExpired"));
            }

            if (user.EmailConfirmed)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest, new ResponseErrorModel("CryptRates.UserService.UserVerification.BadRequest.UserEmailConfirmed"));
            }

            user.EmailConfirmed = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception("Email is not confirmed.");
            }
        }

        /// <summary>
        /// Authenticate
        /// </summary>
        /// <param name="model"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public async Task<TokenResponse> GetTokenResponseAsync(RequestLoginViewModel model, AuthSettings authSettings)
        {
            var tokenResponse = await GetTokenAsync(model, authSettings);

            return tokenResponse;
        }

        public async Task<ResponseLoginViewModel> LoginAsync(RequestLoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (!user.EmailConfirmed)
                {
                    throw new HttpResponseException(HttpStatusCode.Unauthorized, new ResponseErrorModel("Please confirm your e-mail."));
                }

                var claims = await _userManager.GetClaimsAsync(user);

                var response = _mapper.Map<ResponseLoginViewModel>(user);

                response.Claims = _mapper.Map<List<UserClaimsModel>>(claims);

                return response;

            }

            throw new HttpResponseException(HttpStatusCode.Unauthorized, new ResponseErrorModel("Log in failed."));
        }

        #region Private
        private async Task<TokenResponse> GetTokenAsync(RequestLoginViewModel model, AuthSettings authSettings)
        {
            // discover endpoints from metadata
            //var cl = new DiscoveryEndpoint(authSettings.AuthServiceAddress);
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("https://demo.identityserver.io");
            if (disco.IsError) throw new Exception(disco.Error);
            var tokenEndpoint = disco.TokenEndpoint;

            // request token

            var tokenResponse = await client.RequestTokenAsync(new TokenRequest
            {
                Address = tokenEndpoint,
                GrantType = OidcConstants.GrantTypes.Password,
                ClientId = authSettings.ClientId,
                ClientSecret = authSettings.ClientSecret,

                Parameters =
                {
                    { "email", model.Email },
                    { "password", model.Password }
                }
            });

            if (tokenResponse.IsError)
            {
                //todo:  log here
                var errorCode = (ErrorCode)Enum.Parse(typeof(ErrorCode), tokenResponse.ErrorDescription);
                switch (errorCode)
                {
                    case ErrorCode.Forbidden:
                        throw new HttpResponseException(HttpStatusCode.Forbidden, new ResponseErrorModel("Email is not verified."));
                    case ErrorCode.NotFound:
                        throw new HttpResponseException(HttpStatusCode.NotFound, new ResponseErrorModel("Login and Password do not match."));
                    case ErrorCode.Unauthorized:
                        throw new HttpResponseException(HttpStatusCode.Unauthorized, new ResponseErrorModel("Login and Password do not match."));
                    default:
                        throw new HttpResponseException(HttpStatusCode.BadRequest, new ResponseErrorModel(tokenResponse.ErrorDescription));
                }
            }
            return tokenResponse;
        }
    }
    #endregion
}

