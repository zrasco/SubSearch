using GalaSoft.MvvmLight.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubSearchUI.Models;
using SubSearchUI.Services.Extensions;
using SubSearchUI.ViewModels;
using SubSearchUI.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SubSearchUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider _serviceProvider { get; private set; }
        public IConfigurationRoot _config { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _config = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            // Set up global variable for culture infos
            Current.Properties["CultureInfo"] = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures);

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Each window goes into the DI container so they can also have dependencies injected into them
            services.AddTransient(typeof(MainWindow));
            services.AddTransient(typeof(PreferencesWindow));

            // Configuration & writable options
            services.AddSingleton(x => _config);
            //services.Configure<AppSettings>(_config.GetSection(nameof(AppSettings)));
            services.ConfigureWritable<AppSettings>(_config.GetSection(nameof(AppSettings)));
        }
    }
}
