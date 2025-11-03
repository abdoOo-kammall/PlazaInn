using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.User;

namespace Plaza.Mapping.Users
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<ApplicationUser , UserDTO>().ReverseMap();
            CreateMap<CreateUserDTO, ApplicationUser>();

        }
    }
}
