using AutoMapper;
using Bookify.Business.Models;
using Bookify.Business.Models.Request;
using Bookify.Business.Services.Interfaces;
using Bookify.DataAccess.Entities.Identity;
using Bookify.Infrastructure.Enums;
using Bookify.Infrastructure.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
                    return _mapper.Map<UserModel>(user);
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

    }
}
