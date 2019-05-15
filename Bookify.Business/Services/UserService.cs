using AutoMapper;
using Bookify.Business.Models;
using Bookify.Business.Models.Request;
using Bookify.Business.Services.Interfaces;
using Bookify.Infrastructure.Enums;
using Bookify.Infrastructure.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Bookify.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        //TODO: Use this in email confirm functionality
        private const int UserRegistrationExpirationHours = 24;

        public UserService(IMapper mapper,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
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

            var user = _mapper.Map<IdentityUser>(model);
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

    }
}
