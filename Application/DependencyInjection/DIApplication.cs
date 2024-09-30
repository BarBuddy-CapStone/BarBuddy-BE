using Application.IService;
using Application.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DependencyInjection
{
    public static class DIApplication
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddServices();
            return services;
        }

        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<IBarService, BarService>();
            services.AddScoped<IAccountService, AccountService>();
        }
    }
}
