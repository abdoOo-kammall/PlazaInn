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
using PlazaRepository.PartnerRepo;
using PlazaRepository.UserRepo;
using PlazaService.AboutUsImages;
using PlazaService.Account;
using PlazaService.BookingService;

//using PlazaService.BookingService;
using PlazaService.ContactUsWithWorkingHours;
using PlazaService.HomeSliders;
using PlazaService.Hotels;
using PlazaService.InsightHotels;
using PlazaService.Partner;
using PlazaService.Payment;
using PlazaService.SectionImagesService;
using PlazaService.Users;
using Shared.Options;

namespace Plaza.Extensions
{
    public static class ApplicationServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddIdentity<ApplicationUser, IdentityRole>(options => {

                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

            })
                .AddEntityFrameworkStores<PlazaDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<IHotelService, HotelService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IRoomService , RoomService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService , EmailService>();
            services.AddScoped<IInsightHotelService, InsightHotelService>();
            services.AddScoped<IPartnerService, PartnerService>();
            services.AddScoped<IPartnerRepository, PartnerRepository>();
            services.AddHttpClient<IPaymobService, PaymentService>();
            services.AddScoped<IHomeSliderService, HomeSliderService>();
            services.AddScoped<IAboutUsService, AboutUsService>();
            services.AddScoped<ISectionImageService, SectionImageService>();
            services.AddScoped<IContactUsService, ContactUsService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IRoomAvailabilityService, RoomAvailabilityService>();

            services.AddOptions<BookingOptions>()
                .BindConfiguration(BookingOptions.SectionName)
                .Validate(o => o.TaxRate >= 0 && o.TaxRate < 1, "Booking:TaxRate must be between 0 and 1 (e.g. 0.28).")
                .Validate(o => o.PendingExpiryMinutes > 0, "Booking:PendingExpiryMinutes must be positive.")
                .Validate(o => o.CleanupIntervalMinutes > 0, "Booking:CleanupIntervalMinutes must be positive.")
                .ValidateOnStart();
            services.AddHostedService<ExpiredBookingsCleanupService>();

            services.AddHealthChecks()
                .AddDbContextCheck<PlazaDbContext>("database");
            return services;
        }
    }
}
