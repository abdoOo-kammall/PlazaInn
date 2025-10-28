
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Plaza.Extensions;
using PlazaCore.Entites;
using PlazaRepository;
using PlazaRepository.Data.DataSeed;
using Microsoft.Extensions.FileProviders;

namespace Plaza
{
    public class Program
    {
        public static async Task  Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
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

            builder.Services.addApplicationServices();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        policy
                            .AllowAnyOrigin()   
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            #region jWT Setting
            // JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            #endregion

            var app = builder.Build();
            #region dataSeeding

            //await SeedDataAsync(app);

            //#region DataSeeding
            //async Task SeedDataAsync(WebApplication app)
            //{
            //    using var scope = app.Services.CreateScope();
            //    var services = scope.ServiceProvider;

            //    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            //    await DefaultRole.SeedAsync(roleManager);

            //    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            //    await DefaultUser.SeedAsync(userManager);
            //}
            #endregion


           
            app.UseSwagger();
                app.UseSwaggerUI();
           

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");

            //app.UseStaticFiles();
            var rootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(rootPath),
                RequestPath = ""
            });


            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
