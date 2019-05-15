namespace Bookify.Business.Models
{
    public class UserModel
    {
        public string Email { get; set; }

        public string UserName { get; set; }

        public bool EmailConfirmed { get; set; }

        public string EncodedToken { get; set; }

        public string JwtToken { get; set; }
    }
}
