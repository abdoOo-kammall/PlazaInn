using System.Text.Json;
using AutoMapper;
using PlazaCore.Entites;
using Shared.DTO.Event;
using Shared.Helper;
using Shared.Security;

namespace Plaza.Mapping.Events
{
    public class EventMapping : Profile
    {
        public EventMapping()
        {
            CreateMap<CreateEventDto, Event>()
                .ForMember(dest => dest.ImageIds, opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));

            CreateMap<UpdateEventDto, Event>()
                .IncludeBase<CreateEventDto, Event>();

            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => Methods.ConvertIdsToFileDTO(src.ImageIds, "event")));
        }

        private static string SerializeIds(List<int> ids) =>
            ids == null ? "[]" : JsonSerializer.Serialize(ids);
    }
}
