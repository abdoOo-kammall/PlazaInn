using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Plaza.DTO.Account;
using PlazaCore.Entites;

namespace Shared.Mapping.Account
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<RegisterDTO, ApplicationUser>()
           .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
           .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        }
    }
    }

