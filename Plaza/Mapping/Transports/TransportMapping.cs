using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Transport;
using Shared.Helper;
using Shared.Security;

namespace Plaza.Mapping.Transports
{
    public class TransportMapping : Profile
    {
        public TransportMapping()
        {
            CreateMap<CreateTransportDto, Transport>()
                .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));

            CreateMap<UpdateTransportDto, Transport>()
                .IncludeBase<CreateTransportDto, Transport>();

            CreateMap<Transport, TransportDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds, "transport")));
        }

        private static string SerializeIds(List<int> ids) =>
            ids == null ? "[]" : JsonSerializer.Serialize(ids);
    }
}
