using AutoMapper;
using Bookify.Business.Models;
using Bookify.Business.Models.Request;
using Bookify.Business.Models.Response;
using Bookify.DataAccess.Entities;
using Bookify.DataAccess.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;

namespace Bookify.Business.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EmailTemplate, EmailTemplateModel>().ReverseMap();

            CreateMap<RequestRegisterViewModel, User>().ForMember(x => x.Token, opt => opt.MapFrom(t => Guid.NewGuid()))
                                                .ForMember(x => x.TokenCreatedDateTimeUtc, opt => opt.MapFrom(t => DateTime.Now))
                                                .ForMember(x => x.UserName, opt => opt.MapFrom(y => y.Email))
                                                .ReverseMap();

            CreateMap<User, UserModel>().ForMember(x => x.EncodedToken, opt => opt.MapFrom(u => Base64UrlEncoder.Encode(u.Token.ToString()))).ReverseMap();

            CreateMap<User, ResponseLoginViewModel>().ForMember(x => x.UserId, opt => opt.MapFrom(y => y.Id)).ReverseMap();

            CreateMap<Claim, UserClaimsModel>().ReverseMap();
        }
    }
}
