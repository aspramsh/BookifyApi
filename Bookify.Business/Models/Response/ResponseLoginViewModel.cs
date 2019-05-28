using System;
using System.Collections.Generic;

namespace Bookify.Business.Models.Response
{
    public class ResponseLoginViewModel
    {
        public Guid UserId { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }

        public List<UserClaimsModel> Claims { get; set; }
    }
}
