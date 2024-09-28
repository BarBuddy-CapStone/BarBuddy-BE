using Application.Mappers;
using Domain.IRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            return services;
        }




        public static void AddServices(this IServiceCollection services)
        {
            //services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<Firebase>();
            
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
    }
}
