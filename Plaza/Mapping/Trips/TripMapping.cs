using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Trip;
using Shared.Helper;
using Shared.Security;

namespace Plaza.Mapping.Trips
{
    public class TripMapping : Profile
    {
        public TripMapping()
        {
            CreateMap<CreateTripDto, Trip>()
                .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));

            CreateMap<UpdateTripDto, Trip>()
                .IncludeBase<CreateTripDto, Trip>();

            CreateMap<Trip, TripDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds, "trip")));
        }

        private static string SerializeIds(List<int> ids) =>
            ids == null ? "[]" : JsonSerializer.Serialize(ids);
    }
}
