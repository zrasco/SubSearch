using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SubSearchUI.Services.Abstract;
using SubSearchUI.Services.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSearchUI.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureWritable<T>(
            this IServiceCollection services,
            IConfigurationSection section,
            string file = "appsettings.json") where T : class, new()
        {
            services.Configure<T>(section);
            services.AddTransient<IWritableOptions<T>>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfigurationRoot>();
                var options = provider.GetService<IOptionsMonitor<T>>();
                return new WritableOptions<T>(options, configuration, section.Key, file);
            });
        }
    }
}
