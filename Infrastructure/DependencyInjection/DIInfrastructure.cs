using Application.Interfaces;
using Application.IService;
using Application.Mappers;
using Application.Service;
using Infrastructure.Integrations;
using Infrastructure.Payment.Service;
using Infrastructure.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Persistence.Repository;
using System.Text;
using Domain.IRepository;
using Infrastructure.SignalR;

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

            //SignalR
            services.AddSignalR();
            return services;
        }



        public static void AddServices(this IServiceCollection services)
        {
            //services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFirebase, FirebaseService>();
            services.AddTransient<IAuthentication, Authentication>();
            services.AddMemoryCache(); 
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IOtpSender, OtpSender>();
            services.AddTransient<IBookingHubService, BookingHubService>();

            services.AddScoped<IEmotionalDrinkCategoryService, EmotionalDrinkCategoryService>();
            services.AddScoped<IFeedBackService, FeedBackService>();
            services.AddScoped<IBarService, BarService>();
            services.AddScoped<IDrinkCategoryService, DrinkCategoryService>();
            services.AddScoped<IDrinkService, DrinkService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IPaymentHistoryService, PaymentHistoryService>();
            services.AddScoped<ITableTypeService, TableTypeService>();
            services.AddScoped<ITableService, TableService>();
            services.AddScoped<IAuthenService, AuthenService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IBookingTableService, BookingTableService>();
        }

        //public static void AddCORS(this IServiceCollection services)
        //{
        //    services.AddCors(options =>
        //    {
        //        options.AddPolicy("AllowReactBarBuddy",
        //            builder =>
        //            {
        //                builder.AllowAnyOrigin()
        //                	   .WithOrigins("https://localhost:5173")
        //                	   .AllowAnyHeader()
        //                	   .AllowAnyMethod()
        //                	   .AllowCredentials();
        //                builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
        //            });
        //    });
        //}

        public static void AddCORS(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowReactBarBuddy",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:5173") // Thay đổi URL này thành URL của frontend của bạn
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
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

        public static void AddAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                    };
                });
        }
    }
}
