using Application.Interfaces;
using Application.IService;
using Application.Mappers;
using Application.Service;
using Domain.Entities;
using Domain.Interfaces;
using Domain.IRepository;
using Firebase.Auth;
using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Data;
using Persistence.Repository;

namespace Infrastructure.DependencyInjection
{
    public static class DIInfrastructure
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //MediatR
            //services.AddMediatR(NewMethod().Assembly);

            // CORS
            services.AddCORS();

            // Repository
            services.AddRepositories();

            // Mapper
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

            // Services
            services.AddServices();

            // Authentications
            services.AddAuthentication(configuration);

            return services;
        }




        public static void AddServices(this IServiceCollection services)
        {
            //services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFirebase, Firebase>();

            services.AddScoped<IEmotionalDrinkCategoryService, EmotionalDrinkCategoryService>();
            services.AddScoped<IFeedBackService, FeedBackService>();
            services.AddScoped<IBarService, BarService>();
            services.AddScoped<IDrinkCategoryService, DrinkCategoryService>();
            services.AddScoped<IDrinkService, DrinkService>();
            services.AddScoped<IAccountService, AccountService>();

        }

        public static void AddCORS(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowReactBarBuddy",
                    builder =>
                    {
                        //builder.AllowAnyOrigin()
                        //	   .WithOrigins("https://localhost:5173")
                        //	   .AllowAnyHeader()
                        //	   .AllowAnyMethod()
                        //	   .AllowCredentials();
                        builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
                    });
            });
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            //Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddScoped<EmailTemplateBuilder>();
            //services.AddIdentity<User, Role>().AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders();
            //services.AddAuthentication(options =>
            //{
            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //}).AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateIssuer = true,
            //        ValidateAudience = true,
            //        ValidateIssuerSigningKey = true,
            //        ValidateLifetime = true,
            //        ValidIssuer = configuration["Jwt:Issuer"],
            //        ValidAudience = configuration["Jwt:Audience"],
            //        IssuerSigningKey = new SymmetricSecurityKey
            //        (Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
            //    };
            //});

            //services.Configure<IdentityOptions>(options =>
            //{
            //    // Set your desired password requirements here
            //    options.Password.RequireDigit = false;
            //    options.Password.RequireLowercase = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequiredUniqueChars = 0;

            //    // Lockout settings
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            //    options.Lockout.MaxFailedAccessAttempts = 5;
            //    options.Lockout.AllowedForNewUsers = true;

            //    // User settings
            //    options.User.RequireUniqueEmail = true;
            //    options.SignIn.RequireConfirmedEmail = true;
            //});

            services.AddScoped<IAuthentication, Authen>();
        }
    }
}
