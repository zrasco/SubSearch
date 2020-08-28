using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProviderPluginTypes;
using SubSearchUI.Models;
using SubSearchUI.Services.Abstract;
using SubSearchUI.Services.Concrete;
using SubSearchUI.ViewModels;
using SubSearchUI.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace SubSearchUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings _appSettings;
        private readonly MainWindowViewModel _vm;
        private readonly IServiceProvider _services;
        private readonly ILogger<MainWindow> _logger;
        private readonly IList<CultureInfo> _allCultureInfos;

        void QueueItemStatusChangeEventHandler(QueueItem item)
        {
            lvQueue.ScrollIntoView(item);
        }

        public MainWindow(  IServiceProvider services,
                            IWritableOptions<AppSettings> settings,
                            ILogger<MainWindow> logger,
                            IList<CultureInfo> allCultureInfos,
                            ObservableCollection<PluginStatus> pluginStatus,
                            IFilenameProcessor filenameProcessor)
        {
            InitializeComponent();

            _allCultureInfos = allCultureInfos;
            _logger = logger;
            _appSettings = settings.Value;
            _vm = services.GetRequiredService<MainWindowViewModel>();
            
            _vm.Scheduler = services.GetRequiredService<Scheduler>();
            _vm.Scheduler.SetQueueItemStatusChangeEventHandler(QueueItemStatusChangeEventHandler);

            //_vm.Scheduler = new Scheduler(QueueItemStatusChangeEventHandler);
            //_vm = new MainWindowViewModel(QueueItemStatusChangeEventHandler, filenameProcessor);
            _services = services;
            _vm.PluginStatusList = pluginStatus;

            // Global failsafe for unhandled exceptions
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);

            // Add test items to queue
            Random rand = new Random();            
            
            for (int x = 1; x <= 1; x++)
            {
                _vm.Scheduler.AddItem($"Test #{x}", (item, cancellationToken) =>
                {
                    while (item.ProgressPercentage < 1)
                    {
                        Thread.Sleep(rand.Next(10, 50));

                        if (cancellationToken.IsCancellationRequested)
                            cancellationToken.ThrowIfCancellationRequested();

                        item.ProgressPercentage += rand.NextDouble() * (.01 - .0001) + .0001;
                    }

                    item.ProgressPercentage = 1;

                    return true;
                });
            }
            
            DataContext = _vm;

            RefreshTVFromPath(_appSettings.RootDirectory);
        }

        private static TreeViewItem FindTviFromObjectRecursive(ItemsControl ic, object o)
        {
            //Search for the object model in first level children (recursively)
            TreeViewItem tvi = ic.ItemContainerGenerator.ContainerFromItem(o) as TreeViewItem;
            if (tvi != null) return tvi;
            //Loop through user object models
            foreach (object i in ic.Items)
            {
                //Get the TreeViewItem associated with the iterated object model
                TreeViewItem tvi2 = ic.ItemContainerGenerator.ContainerFromItem(i) as TreeViewItem;
                tvi = FindTviFromObjectRecursive(tvi2, o);
                if (tvi != null) return tvi;
            }
            return null;
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, "General exception");

            e.Handled = true;
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            Application.Current.Shutdown();

            // Should never get here
            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }

        private void MnuPreferences_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            // Create new window with DI
            var pWindow = _services.GetRequiredService<PreferencesWindow>();

            pWindow.Owner = this;

            var result = pWindow.ShowDialog();

            if (result == true)
            {
                // Reload options
                string oldRoot = _appSettings.RootDirectory;
                string oldLang = _appSettings.DefaultLanguage;

                _appSettings = _services.GetRequiredService<IWritableOptions<AppSettings>>().Value;

                if (oldRoot != _appSettings.RootDirectory)
                    RefreshTVFromPath(_appSettings.RootDirectory);

                if (oldRoot != _appSettings.RootDirectory || oldLang != _appSettings.DefaultLanguage)
                    ReloadFileList(_vm.SelectedDirectory.FullPath);
            }

            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }

        private void TvFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            // We must set the selected directory here, because inexplicably, the treeview control does not allow you
            // to bind to the selected item directly
            TreeView tvSender = (sender as TreeView);
            try
            {
                TVDirectoryItem tvdi = (tvSender.SelectedItem as TVDirectoryItem);

                if (tvdi != null)
                {
                    _vm.SelectedDirectory = tvdi;
                    ReloadFileList(_vm.SelectedDirectory.FullPath);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in TvFolders_SelectedItemChanged()");
            }

            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }

        private void ReloadFileList(string fullPath)
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            bool foundVideos = false;
            var videoExtensions = _appSettings.VideoExtensions.Split(',').Select(p => p.Trim().ToUpper()).ToList();

            _vm.FileList = new ObservableCollection<VideoFileItem>();

            foreach (string f in Directory.GetFiles(fullPath))
            {
                // Only display valid video files
                if (videoExtensions.Contains(System.IO.Path.GetExtension(f).ToUpper()))
                {
                    foundVideos = true;
                    var fileItem = new VideoFileItem()
                    {
                        ImageSource = "/Images/video.png",
                        FullPath = f,
                        Text = Path.GetFileName(f)
                    };

                    // Determine whether this video has subtitles
                    _appSettings = _services.GetRequiredService<IWritableOptions<AppSettings>>().Value;

                    if (fileItem.GenerateSubtitleInfo(_appSettings.DefaultLanguage, _allCultureInfos) == true)
                        _vm.FileList.Add(fileItem);
                    else
                        _logger.LogError($"Unable to look up culture info for language '{_appSettings.DefaultLanguage}'");                    
                }
            }

            if (!foundVideos)
            {
                var notFoundItem = new VideoFileItem()
                {
                    Text = $"(No videos were found in folder '{new DirectoryInfo(fullPath).Name}')"
                };

                _vm.FileList.Add(notFoundItem);
            }

            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }

        public void AddLogEntry(Exception ex, DateTimeOffset timeStamp, string sourceContext, string message, LogLevel logLevel)
        {
            AddLogEntry(timeStamp, sourceContext, message + $" ({ex.Message})", logLevel);
        }

        public void AddLogEntry(DateTimeOffset timeStamp, string sourceContext, string message, LogLevel logLevel)
        {
            // Note: Any images referenced here must be assigned as resources to the project in order for the ListView
            // control to display them

            string imageSource = "/Images/";

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    imageSource += "error.png";

                    if (logLevel == LogLevel.Critical)
                        message = "CRITICAL: " + message;

                    break;
                case LogLevel.Information:
                    imageSource += "info.png";
                    break;
                case LogLevel.Warning:
                    imageSource += "warning.jpg";
                    break;
                case LogLevel.Debug:
                    // Source: https://thenounproject.com/term/debug/83827/
                    imageSource += "debug.png";
                    break;
                case LogLevel.Trace:
                    // Source: https://depositphotos.com/vector-images/verbose.html
                    imageSource += "verbose.jpg";
                    break;
                default:
                    imageSource = null;
                    break;
            }

            var logEntry = new LogItem()
            {
                TimeStamp = timeStamp.ToString("M-d-yy HH:mm:ss.fff zzz"),
                ImageSource = imageSource,
                SourceContext = $"[{sourceContext}]",
                Text = message
            };

            _vm.LogItems.Add(logEntry);
            lvLog.ScrollIntoView(logEntry);
        }

        private void RefreshTVFromPath(string rootDirectory)
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            var newDirectoryList = new ObservableCollection<TVDirectoryItem>();

            // TODO: Add support for multiple root directories

            /*
            if (rootDirectory.Length == 3 && rootDirectory.Contains(@":\"))
                rootDirName = rootDirectory;
            else
                rootDirName = System.IO.Path.GetDirectoryName(rootDirectory);
            */

            var rootItem = new TVDirectoryItem(rootDirectory);

            newDirectoryList.Add(rootItem);

            // Change this in one go to avoid setting the listview into an intermediate state
            _vm.DirectoryList = newDirectoryList;

            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            try
            {
                // Select the first item progmatically
                var tvi = FindTviFromObjectRecursive(tvFolders, _vm.DirectoryList[0]);

                if (tvi != null)
                    tvi.IsSelected = true;

                /*
                // Start the background jobs scheduler
                Dispatcher.InvokeAsync(new Action(BackgroundJobScheduler), DispatcherPriority.ApplicationIdle);

                */

                // Load the plugins
                LoadPlugins();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception in {App.GetCaller()}()");
            }
            finally
            {
                //Directory.SetCurrentDirectory(System.IO.Path.GetFullPath(currentDirectory));
            }

            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void LoadPlugins()
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            _vm.PluginStatusList.Clear();

            // Load the plugins              
            foreach (Plugin pluginInfo in _appSettings.Plugins)
            {
                try
                {
                    if (_vm.PluginStatusList.Where(x => x.Name == pluginInfo.Name).FirstOrDefault() != null)
                    {
                        // Can't have more than 1 plugin with the same name in the configuration
                        _vm.SetStatusText($"Duplicate Plugin {pluginInfo.Name} was skipped. Plugins must have unique names.", true, LogLevel.Warning);
                    }
                    else
                    {
                        // Add to global list
                        _vm.PluginStatusList.Add(new PluginStatus(pluginInfo));

                        // Get the entry we just added
                        PluginStatus pluginStatus = _vm.PluginStatusList.Where(x => x.Name == pluginInfo.Name).FirstOrDefault();

                        _vm.SetStatusText($"Loading provider plugin {pluginInfo.Name} ({pluginInfo.File})...");

                        // Load the .NET assembly & dependencies
                        // TODO: Eventually add capability to use unloadable plugins. .NET Core 3.0 supports *some* of this but has issues with .NET dependency conflicts
                        //
                        // Code below was from the most recent attempt on 8/8/19. I was unable to get an instance of the plugin object because the plugin had a different ILogger<T>
                        // than the main program, even though the UI program and plugin have the exact same version.

                        /*
                         * #1
                        var context = new CollectibleAssemblyLoadContext(pluginInfo.File);
                        var pluginPath = Path.GetDirectoryName(pluginInfo.File);
                        var assembly = context.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginInfo.File)));
                        var assyTypes = assembly.GetTypes();
                        */
                        /*
                         * #2
                            var assembly = Assembly.LoadFrom(pluginInfo.File);
                            var providerPluginType = assembly.GetTypes().Where(x => x.Name == "ProviderPlugin").FirstOrDefault();
                        */
                        /*
                        var context = new CollectibleAssemblyLoadContext(pluginInfo.File);
                        var pluginPath = Path.GetDirectoryName(pluginInfo.File);
                        var assembly = context.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginInfo.File)));
                        var assyTypes = assembly.GetTypes();
                        */

                        //var assembly2 = Assembly.LoadFrom(pluginInfo.File);
                        //var providerPluginType = assembly2.GetTypes().Where(x => x.Name == "ProviderPlugin").FirstOrDefault();


                        // TODO: Temporarily put this until I figure out how to inject the ILogger dependency



                        //var paramType = assembly.GetTypes().Where(x => x.Name == "ProviderPlugin").FirstOrDefault().GetTypeInfo().DeclaredConstructors.FirstOrDefault().GetParameters()[0].ParameterType;
                        //var paramType2 = assembly2.GetTypes().Where(x => x.Name == "ProviderPlugin").FirstOrDefault().GetTypeInfo().DeclaredConstructors.FirstOrDefault().GetParameters()[0].ParameterType;

                        //var svc2 = _services.GetService(paramType2);
                        //var svc = _services.GetService(paramType);

                        var assembly = Assembly.LoadFrom(pluginInfo.File);
                        var providerPluginType = assembly.GetTypes().Where(x => x.Name == "ProviderPlugin").FirstOrDefault();

                        // Find the first (and hopefully only) concrete class which implements IProviderPlugin in the assembly
                        var assyTypes = assembly.GetTypes();

                        for (int x = 0; x < assyTypes.Count() && pluginStatus.Interface == null; x++)
                        {
                            if (typeof(IProviderPlugin).IsAssignableFrom(assyTypes[x]))
                                pluginStatus.Interface = ActivatorUtilities.CreateInstance(_services, assyTypes[x]) as IProviderPlugin;
                        }

                        if (pluginStatus.Interface == null)
                        {
                            // Error
                            throw new Exception("Nothing in the plugin file implements IProviderPlugin");
                        }

                        _vm.SetStatusText($"Loaded plugin {pluginInfo.Name}. Initializing in background...");
                        pluginStatus.Status = PluginLoadStatus.Scheduled;

                        //context.Unload();

                        _vm.Scheduler.AddItem($"Initializing plugin ({pluginInfo.Name})", (queueItem, cancellation) =>
                        {
                            try
                            {
                                ConcurrentQueue<ProviderSignal> cQueue = new ConcurrentQueue<ProviderSignal>();

                                // Change the status from the UI thread when it gets around to it
                                Dispatcher.BeginInvoke(() =>
                                {
                                    pluginStatus.Status = PluginLoadStatus.Loading;
                                }, DispatcherPriority.ApplicationIdle);

                                _logger.LogDebug($"Plugin {pluginInfo.Name} initialization scheduled for execution.");

                                var initTask = pluginStatus.Interface.InitAsync(cQueue);

                                while (!cQueue.IsEmpty || initTask.Status == TaskStatus.WaitingForActivation || initTask.Status == TaskStatus.Running)
                                {
                                    // Check for signals from provider task
                                    if (cQueue.TryDequeue(out ProviderSignal result) == true)
                                    {
                                        if (result.Type == ProviderSignal.SignalType.ChangePercentage)
                                            queueItem.ProgressPercentage = result.DoubleValue;
                                        else if (result.Type == ProviderSignal.SignalType.ChangeDetails)
                                            queueItem.DetailsText = result.TextValue;
                                    }

                                    cancellation.ThrowIfCancellationRequested();
                                    System.Threading.Thread.Sleep(10);
                                }

                                _logger.LogDebug($"Plugin {pluginInfo.Name} initialization complete.");

                                _logger.LogDebug($"Plugin {pluginInfo.Name} successfully loaded.");
                                return true;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug(ex, $"Plugin {pluginInfo.Name} initialization failed.");
                                pluginStatus.Interface = null;
                                // Exception should already be logged in the provider, so no need to log it a second time here. Return false to indicate failure.
                                return false;
                            }
                        },
                         (queueItem) =>
                         {
                             string statusText = $"Done initializing plugin {pluginInfo.Name}.";
                             pluginStatus.Status = PluginLoadStatus.Loaded;

                             int loadedPlugins = _vm.PluginStatusList.Where(x => x.Status == PluginLoadStatus.Loaded).Count();
                             int loadingPlugins = _vm.PluginStatusList.Where(x => x.Status == PluginLoadStatus.Loading).Count();
                             int scheduledPlugins = _vm.PluginStatusList.Where(x => x.Status == PluginLoadStatus.Scheduled).Count();
                             int notLoadedPlugins = _vm.PluginStatusList.Where(x => x.Status == PluginLoadStatus.NotLoaded).Count();

                             // See if any plugins are still left
                             if (loadingPlugins > 0)
                                 statusText += $" {loadingPlugins} plugins loading.";
                             if (scheduledPlugins > 0)
                                 statusText += $" {loadingPlugins} plugins scheduled.";
                             if (notLoadedPlugins > 0)
                                 statusText += $" {loadingPlugins} plugins not loaded.";

                             if ((loadingPlugins + scheduledPlugins + notLoadedPlugins) == 0)
                                 statusText += " All plugins loaded.";

                             _vm.SetStatusText(statusText);
                         });

                        string ver = pluginStatus.Interface.Version();
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unable to log plugin ({pluginInfo.Name})");
                }
            }

            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }
        
        /*
        private async void BackgroundJobScheduler()
        {
            // Trace logging commented out here for the sake of our sanity. Re-enable at your own peril!

            //_logger.LogTrace("BackgroundJobScheduler() entered");

            // Start background jobs as nessecary
            var appSettings = _services.GetRequiredService<IWritableOptions<AppSettings>>().Value;

            // Cycle the scheduler items
            _vm.Scheduler.Cycle(appSettings.MaxBackgroundJobs);

            // Continue the background jobs scheduler. Default quantum value is 50ms but this can be adjusted thru settings.
            await Task.Delay(appSettings.SchedulerQuantum);
            await Dispatcher.BeginInvoke(new Action(BackgroundJobScheduler), DispatcherPriority.ApplicationIdle);

            //_logger.LogTrace("BackgroundJobScheduler() exiting");
        }
        */

        private void LvFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            // If you press ctrl+A to select all of the items, only the selected ones get IsSelected set to true.
            // This is because WPF uses a virtualizing stackpanel by default
            // Source: https://stackoverflow.com/questions/7372893/how-to-bind-isselected-in-a-listview-for-non-visible-virtualized-items
            //
            // We'll use a workaround here that shouldn't affect normal performance, since the list of files in a single directory should never be extremely large

            foreach (VideoFileItem item in lvFiles.SelectedItems)
                item.IsSelected = true;

            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }

        private void VideoFileContextMenu_Opening(object sender, RoutedEventArgs e)
        {
            _logger.LogTrace($"{App.GetCaller()}() entered");

            // Refresh the context menu items/visibility before opening
            _vm.RaisePropertyChanged(nameof(_vm.SelectedVideoMenuItems));
            _vm.RaisePropertyChanged(nameof(_vm.SelectedVideoMenuVisible));

            // Close the context menu if not visible
            if (_vm.SelectedVideoMenuVisible != Visibility.Visible)
                (sender as ContextMenu).IsOpen = false;

            _logger.LogTrace($"{App.GetCaller()}() exiting");
        }

        private void QueueItemContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Manually raise the event before showing to update the enabled/disabled status. This isn't automatic
            _vm.CancelSelectedQueueItemsCommand.RaiseCanExecuteChanged();
        }

        private void LvQueue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If you press ctrl+A to select all of the items, only the selected ones get IsSelected set to true.
            // This is because WPF uses a virtualizing stackpanel by default
            // Source: https://stackoverflow.com/questions/7372893/how-to-bind-isselected-in-a-listview-for-non-visible-virtualized-items
            //
            // We'll use a workaround here that shouldn't affect normal performance, since the queue should never be extremely large

            foreach (QueueItem item in lvQueue.SelectedItems)
                item.IsSelected = true;
        }


    }
}
