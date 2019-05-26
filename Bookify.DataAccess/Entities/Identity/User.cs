using Microsoft.AspNetCore.Identity;
using System;

namespace Bookify.DataAccess.Entities.Identity
{
    public class User : IdentityUser
    {
        public Guid? Token { get; set; }

        public DateTime? TokenCreatedDateTimeUtc { get; set; }

        public string JwtToken { get; set; }
    }
}
