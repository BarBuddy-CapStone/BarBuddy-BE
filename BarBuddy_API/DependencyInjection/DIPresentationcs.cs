using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using Microsoft.OpenApi.Models;
using Persistence.DependencyInjection;

namespace BarBuddy_API.DependencyInjection
{
    public static class DIPresentationcs
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {

            //Swagger
            services.AddSwagger();

            //DI Infrastructure
            services.AddInfrastructure(configuration);

            //DI Persistence
            services.AddPersistence(configuration);

            //DI Application
            services.AddApplication();
            return services;
        }

        public static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "BarBuddy API", Version = "v1" });

                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
            });
        }

    }
}
