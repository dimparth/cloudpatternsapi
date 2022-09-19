using cloudpatternsapi.automapperprofile;
using cloudpatternsapi.implementation;
using cloudpatternsapi.implementation.repositories;
using cloudpatternsapi.implementation.services;
using cloudpatternsapi.interfaces;
using cloudpatternsapi.interfaces.services;
using cloudpatternsapi.models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace cloudpatternsapi.dependencyinjections
{
    public static class DIExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, string connectionString, Dictionary<string, TimeSpan> configuration)
        {
            services.AddDbContext<BookingContext>(x =>
                x.UseSqlServer(connectionString));
            services.AddCors();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IHallRepository, HallRepository>();
            services.AddScoped<IShowRepository, ShowRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IHallsService,HallsService>();
            services.AddScoped<IShowService, ShowService>();
            services.AddScoped<IHealthCheckService, HealthcheckService>();
            services.AddSingleton<ICacheService>(service => new CacheService(service.GetService<IMemoryCache>(), configuration));
            services.AddMemoryCache();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            return services;
        }

        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options =>
               {
                   options.TokenValidationParameters = new TokenValidationParameters
                   {
                       ValidateIssuerSigningKey = true,
                       IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"])),
                       ValidateIssuer = false,
                       ValidateAudience = false
                   };

                   options.Events = new JwtBearerEvents
                   {
                       OnMessageReceived = context =>
                       {
                           var accessToken = context.Request.Query["access_token"];

                           var path = context.HttpContext.Request.Path;

                           if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                           {
                               context.Token = accessToken;
                           }

                           return Task.CompletedTask;
                       }
                   };
               });
            services.AddIdentityCore<AppUser>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = false;
            })
                .AddRoles<AppRole>()
                .AddRoleManager<RoleManager<AppRole>>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddRoleValidator<RoleValidator<AppRole>>()
                .AddEntityFrameworkStores<BookingContext>();

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
                opt.AddPolicy("RequireModeratorRole", policy => policy.RequireRole("Admin", "Moderator"));
            });

            return services;
        }
    }
}

