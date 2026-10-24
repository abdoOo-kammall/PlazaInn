using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Hotel;
using Shared.Security;
using System.Text.Json;
using Shared.Helper;

public class HotelMapping : Profile
{
    public HotelMapping()
    {
        CreateMap<Hotel, HotelDto>()
            .ForMember(dest => dest.Images,
                       opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds, "hotel")))
            .ForMember(dest => dest.Logo,
                opt => opt.MapFrom(src =>
               Methods.ConvertIdsToFileDTO(src.LogoId  , "logo").FirstOrDefault()))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
            .ForMember(dest => dest.Insight, opt => opt.MapFrom(src => src.InsightHotel));

        CreateMap<CreateHotelDto, Hotel>()
            .ForMember(dest => dest.ImageIds,
                  opt => opt.MapFrom(src => SerializeIds(src.ImageIds)))
            .ForMember(dest => dest.LogoId,
                  opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.LogoId })));


        CreateMap<UpdateHotelDTO, Hotel>()
            .ForMember(dest => dest.ImageIds,
               opt => opt.MapFrom(src => SerializeIds(src.ImageIds)))
            .ForMember(dest => dest.LogoId,
                  opt => opt.MapFrom(src => SerializeIds(new List<int>() { src.LogoId })));
    }

    private static string SerializeIds(List<int> ids)
    {
        return ids == null ? "[]" : JsonSerializer.Serialize(ids);
    }
}
