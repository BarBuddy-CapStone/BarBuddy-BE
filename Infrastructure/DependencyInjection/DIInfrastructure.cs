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
using Infrastructure.Vnpay.Config;
using Infrastructure.Zalopay.Config;
using Infrastructure.Momo.Config;

using Quartz;
using Infrastructure.QRService;
using Infrastructure.Quartz;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Infrastructure.Configurations;
using System.IO;

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

            //Config
            services.Configure<ZalopayConfig>(configuration.GetSection(ZalopayConfig.ConfigName));
            services.Configure<VnpayConfig>(configuration.GetSection(VnpayConfig.ConfigName));
            services.Configure<MomoConfig>(configuration.GetSection(MomoConfig.ConfigName));

            // Add Firebase Configuration
            services.Configure<FirebaseConfig>(
                configuration.GetSection("Firebase"));

            // Initialize Firebase if not initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                try 
                {
                    var credentialPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, 
                        configuration["Firebase:CredentialFile"]);
                    
                    if (!File.Exists(credentialPath))
                    {
                        throw new FileNotFoundException(
                            $"Firebase credential file not found at {credentialPath}");
                    }

                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(credentialPath)
                    });

                    services.AddSingleton(FirebaseMessaging.DefaultInstance);

                    Console.WriteLine("Firebase initialized successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing Firebase: {ex.Message}");
                    throw;
                }
            }

            // Services
            services.AddServices();

            // Authentications
            services.AddAuthentication(configuration);

            //SignalR
            services.AddSignalR();

            services.AddSingleton<IConnectionMapping, ConnectionMapping>();

            //Quartz
            services.AddQuartz();

            services.AddHttpClient();

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
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<IQRCodeService, QRCodeService>();
            services.AddScoped<NotificationJob>();

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
            services.AddScoped<INotificationDetailService, NotificationDetailService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEventVoucherService, EventVoucherService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventTimeService, EventTimeService>();
            services.AddScoped<IBarTimeService, BarTimeService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IDrinkRecommendationService, DrinkRecommendationService>();

            services.AddScoped<IFcmService, FcmService>();
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
                //options.AddPolicy("AllowEditorSwagger",
                //    builder =>
                //    {
                //        builder.WithOrigins("https://editor.swagger.io")
                //               .AllowAnyHeader()
                //               .AllowAnyMethod()
                //               .AllowCredentials();
                //    });
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
                        RequireExpirationTime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidIssuer = config["Jwt:Issuer"],
                        ValidAudience = config["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                    };
                });
        }

        public static void AddQuartz(this IServiceCollection services)
        {
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                //Add cac Job vao day 
                AddJobWithTrigger<NotificationJob>(options, nameof(NotificationJob));
                AddJobWithTrigger<EventJob>(options, nameof(EventJob));
                AddJobWithTrigger<TokenJob>(options, nameof(TokenJob));
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        }

        public static void AddJobWithTrigger<TJob>(IServiceCollectionQuartzConfigurator options, string jobName) where TJob : IJob
        {
            var jobKey = JobKey.Create(jobName);
            options.AddJob<TJob>(joinBuilder => joinBuilder.WithIdentity(jobKey))
                   .AddTrigger(trigger => trigger.ForJob(jobKey)
                   .WithCronSchedule("0 * * ? * *"));
        }
    }
}
