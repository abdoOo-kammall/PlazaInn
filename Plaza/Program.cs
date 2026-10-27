using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Plaza.Extensions;
using PlazaCore.Entites;
using PlazaRepository;
using PlazaRepository.Data.DataSeed;
using Microsoft.Extensions.FileProviders;
using Plaza.MiddelWare;
using Shared.Helper;
using Shared.ImagePathGeneration;

namespace Plaza
{
    public class Program
    {
        private const string CorsPolicyName = "PlazaCors";

        public static async Task  Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // One hop: the hosting proxy appends the client, and a spoofed prefix is ignored.
                options.ForwardLimit = 1;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.Any, 0));
                options.KnownNetworks.Add(new Microsoft.AspNetCore.HttpOverrides.IPNetwork(IPAddress.IPv6Any, 0));
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste the JWT token returned by /api/Account/login"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddDbContext<PlazaDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 10   ,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    )
                );
            });

            builder.Services.AddApplicationServices();
            builder.Services.AddPlazaRateLimiting();

            var baseUrl = builder.Configuration["BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("BaseUrl is not configured.");
            ImagePathCreation.Configure(baseUrl);

            var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:4200", "https://localhost:4200", "http://plazainn.runasp.net"];

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName,
                    policy =>
                    {
                        policy
                            .WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            #region jWT Setting
            // JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var jwtKey = jwtSettings["Key"];
            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("Jwt:Key is not configured.");
            var key = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // Hosting is HTTP-only for now; flip Jwt:RequireHttpsMetadata to true once SSL is available.
                options.RequireHttpsMetadata = jwtSettings.GetValue("RequireHttpsMetadata", false);
                options.SaveToken = true;
                // Keep claim names exactly as issued by JwtTokenGenerator ("role", "Email", "Id", "UserName").
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = "role",
                    NameClaimType = "UserName"
                };
            });

            #endregion

            var app = builder.Build();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseForwardedHeaders();


            await SeedDataAsync(app);

            #region DataSeeding
            static async Task SeedDataAsync(WebApplication app)
            {
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    await DefaultRole.SeedAsync(roleManager);

                    var adminEmail = app.Configuration["AdminSeed:Email"];
                    var adminPassword = app.Configuration["AdminSeed:Password"];
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    await DefaultUser.SeedAsync(
                        userManager,
                        string.IsNullOrWhiteSpace(adminEmail) ? DefaultUser.DefaultAdminEmail : adminEmail,
                        string.IsNullOrWhiteSpace(adminPassword) ? DefaultUser.DefaultAdminPassword : adminPassword);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database seeding failed. The application will keep running without seeded roles/admin.");
                }
            }
            #endregion

        

            app.UseSwagger();
                app.UseSwaggerUI();
           

            // Hosting is HTTP-only for now; enable via "UseHttpsRedirection": true once SSL is available.
            if (app.Configuration.GetValue("UseHttpsRedirection", false))
            {
                app.UseHttpsRedirection();
            }
            app.UseCors(CorsPolicyName);

            //app.UseStaticFiles();
            var rootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            Methods.ConfigureFileStorage(rootPath);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(rootPath),
                RequestPath = ""
            });


            app.UseAuthentication();

            app.UseRateLimiter();

            app.UseAuthorization();


            app.MapControllers();
            app.MapHealthChecks("/health").AllowAnonymous();


            app.Run();
        }
    }
}
