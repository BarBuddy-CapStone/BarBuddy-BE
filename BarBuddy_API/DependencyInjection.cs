using Application.Mappers;
using MediatR;

namespace BarBuddy_API
{
    public static class DependencyInjection
    {
        public static void AddPackages(this IServiceCollection services)
        {
            //MediatR
            services.AddMediatR(typeof(Program).Assembly);

            //Mappers
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);
            //CORS
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

        public static void AddMasterServices(this IServiceCollection services)
        {

        }
    }
}
