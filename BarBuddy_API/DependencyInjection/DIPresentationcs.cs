using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Put Bearer + your token in the box below",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                option.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme, Array.Empty<string>()
                    }
                });

            });

        }

    }
}
