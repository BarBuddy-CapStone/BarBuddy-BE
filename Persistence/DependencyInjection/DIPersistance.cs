using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.DependencyInjection
{
    public static class DIPersistance
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            //services.AddScoped<IItemDemoRepository, ItemDemoRepository>();
            return services;
        }

        public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
        {

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 23));
            services.AddDbContext<MyDbContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("MyDB");
                options.UseMySql(connectionString, serverVersion, options => options.MigrationsAssembly("Persistence"));
            });
        }
            
    }
}
