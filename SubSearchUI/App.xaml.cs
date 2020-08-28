using GalaSoft.MvvmLight.Ioc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.AspNetCore;
using SubSearchUI.Models;
using SubSearchUI.Services.Abstract;
using SubSearchUI.Services.Concrete;
using SubSearchUI.Services.Extensions;
using SubSearchUI.ViewModels;
using SubSearchUI.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Dynamic;
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

        public IServiceProvider GetServiceProvider()
        {
            return _serviceProvider;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _config = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Each window goes into the DI container so they can also have dependencies injected into them
            services.AddSingleton(typeof(MainWindow));
            services.AddTransient(typeof(PreferencesWindow));

            // Configuration & writable options
            services.AddSingleton(x => _config);
            services.ConfigureWritable<AppSettings>(_config.GetSection(nameof(AppSettings)));

            // Create & inject logger
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.MainWindowLogSink()
                .CreateLogger();

            services.AddSingleton<ILoggerFactory>(new SerilogLoggerFactory(Log.Logger));
            services.AddLogging(x => x.AddSerilog(dispose: true));

            // Add culture info
            services.AddSingleton<IList<CultureInfo>>(CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures).ToList());

            // Add plugin state info
            services.AddSingleton<ObservableCollection<PluginStatus>>(new ObservableCollection<PluginStatus>());

            // Filename regex processor
            services.AddTransient<IFilenameProcessor, FilenameProcessor>();

            // Add viewmodels
            services.AddTransient<MainWindowViewModel>();

            // Add scheduler
            services.AddSingleton<Scheduler>();
        }

        public static string GetCaller([System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
        {
            return memberName;
        }
    }
}
