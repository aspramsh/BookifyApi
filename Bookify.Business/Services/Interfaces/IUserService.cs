using Bookify.Business.Models.Request;
using System.Threading.Tasks;
using Bookify.Business.Models;

namespace Bookify.Business.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserModel> CreateUserAsync(RequestRegisterViewModel model);

        Task VerifyUserEmailAsync(string token);
    }
}
