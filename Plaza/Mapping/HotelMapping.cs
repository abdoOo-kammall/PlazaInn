        using AutoMapper;
        using PlazaCore.Entites;
        using Shared.DTO.Hotel;
    using Shared.Security;
    using System.Text.Json;

        public class HotelMapping : Profile
        {
            public HotelMapping()
            {
            CreateMap<Hotel, HotelDto>()
                .ForMember(dest => dest.ImageUrls,
                           opt => opt.MapFrom(src => ConvertIdsToUrls(src.ImageIds, "hotel")))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => IdEncoder.EncodeId(src.Id)));
        //.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));


        CreateMap<CreateHotelDto, Hotel>()
                    .ForMember(dest => dest.ImageIds,
                               opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));

            CreateMap<UpdateHotelDTO, Hotel>()
                .ForMember(dest => dest.ImageIds,
                           opt => opt.MapFrom(src => SerializeIds(src.ImageIds)));
                        

            }

        // 🧩 Helper methods

        private static List<string> ConvertIdsToUrls(string imageIdsJson, string entityType = "general")
        {
            if (string.IsNullOrEmpty(imageIdsJson))
                return new List<string>();

            try
            {
                var ids = JsonSerializer.Deserialize<List<int>>(imageIdsJson);
                var baseUrl = "http://plazainn.runasp.net";
                return ids?.Select(id => $"{baseUrl}/images/{entityType}/{id}.jpg").ToList() ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }



        private static string SerializeIds(List<int> ids)
            {
                return ids == null ? "[]" : JsonSerializer.Serialize(ids);
            }
        }
