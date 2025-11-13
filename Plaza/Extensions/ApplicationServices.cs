using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Plaza.Mapping;
using PlazaCore.Entites;
using PlazaCore.RepositoryContract;
using PlazaCore.ServiceContract;
using PlazaCore.ServiceContract.Account;
using PlazaRepository;
using PlazaRepository.UserRepo;
using PlazaService.Account;
using PlazaService.Hotels;
using PlazaService.InsightHotels;
using PlazaService.Users;

namespace Plaza.Extensions
{
    public static class ApplicationServices
    {
        public static IServiceCollection addApplicationServices(this IServiceCollection services)
        {

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {

                options.Password.RequireDigit = false;          
                options.Password.RequireLowercase = false;    
                options.Password.RequireUppercase = false;        
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;

            })
                .AddEntityFrameworkStores<PlazaDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<IHotelService, HotelService>();
            //services.AddAutoMapper(typeof(HotelMapping).Assembly);
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IRoomService , RoomService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService , EmailService>();
            services.AddScoped<IInsightHotelService, InsightHotelService>();

            





            return services;
        }
    }
}
