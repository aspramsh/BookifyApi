using Bookify.Business.Models.Request;
using System.Threading.Tasks;
using Bookify.Business.Models;
using Bookify.Business.Settings;
using IdentityModel.Client;
using Bookify.Business.Models.Response;

namespace Bookify.Business.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserModel> CreateUserAsync(RequestRegisterViewModel model);

        Task VerifyUserEmailAsync(string token);

        /// <summary>
        /// Authenticate
        /// </summary>
        /// <param name="model"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        Task<string> GetTokenResponseAsync(RequestLoginViewModel model, AuthSettings authSettings);

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResponseLoginViewModel> LoginAsync(RequestLoginViewModel model);
    }
}
