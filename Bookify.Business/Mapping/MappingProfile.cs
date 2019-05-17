using AutoMapper;
using Bookify.Business.Models;
using Bookify.Business.Models.Request;
using Bookify.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;

namespace Bookify.Business.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EmailTemplate, EmailTemplateModel>().ReverseMap();

            CreateMap<RequestRegisterViewModel, IdentityUser>()/*.ForMember(x => x.Token, opt => opt.MapFrom(t => Guid.NewGuid()))
                                                .ForMember(x => x.TokenCreatedDateTimeUtc, opt => opt.MapFrom(t => DateTime.Now))*/
                                                .ForMember(x => x.UserName, opt => opt.MapFrom(y => y.Email))
                                                .ReverseMap();
        }
    }
}
