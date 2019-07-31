using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SubSearchUI.Models;
using SubSearchUI.Services.Abstract;
using SubSearchUI.ViewModels;
using SubSearchUI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
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

        public MainWindow(IServiceProvider services, IWritableOptions<AppSettings> settings, MainWindowViewModel vm)
        {
            InitializeComponent();

            Application.Current.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Current_DispatcherUnhandledException);

            _appSettings = settings.Value;
            _vm = vm;
            _services = services;

            DataContext = vm;

            RefreshTVFromPath(_appSettings.RootDirectory);

            AddLogEntry("Sample OK entry", ImageType.OK);
            AddLogEntry("Sample info entry", ImageType.Info);
            AddLogEntry("Sample warning entry", ImageType.Warning);
            AddLogEntry("Sample error entry", ImageType.Error);
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
            this.AddLogEntry(e.Exception.Message, ImageType.Error);

            e.Handled = true;
        }

        private void MnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MnuPreferences_Click(object sender, RoutedEventArgs e)
        {
            // Create new window with DI
            var pWindow = _services.GetRequiredService<PreferencesWindow>();

            pWindow.Owner = this;

            var result = pWindow.ShowDialog();

            if (result == true)
                ReloadFileList((tvFolders.SelectedValue as TVDirectoryItem).FullPath);
        }

        private void TvFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                TVDirectoryItem tvdi = (e.OriginalSource as TreeView).SelectedItem as TVDirectoryItem;
                ReloadFileList(tvdi.FullPath);
                
            }
            catch (Exception ex)
            {
                AddLogEntry(ex.Message, ImageType.Error);
            }
        }

        private void ReloadFileList(string fullPath)
        {
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
                    fileItem.GenerateSubtitleInfo(_appSettings.DefaultLanguage);

                    _vm.FileList.Add(fileItem);
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
        }

        public enum ImageType { OK, Info, Warning, Error };

        public void AddLogEntry(string message, ImageType image)
        {
            string imageSource = "/Images/";

            switch (image)
            {
                case ImageType.Error:
                    imageSource += "error.png";
                    break;
                case ImageType.Info:
                    imageSource += "info.png";
                    break;
                case ImageType.Warning:
                    imageSource += "warning.png";
                    break;
                case ImageType.OK:
                    imageSource += "ok.png";
                    break;
                default:
                    imageSource = null;
                    break;
            }
            /*
            ListViewItem item = new ListViewItem();
            item.Content = message;
            item.Tag = new ListViewItemTag(message, image);
            item.FontWeight = FontWeights.Normal;

            lvLog.Items.Add(item);

            */
            var logEntry = new ItemWithImage()
            {
                ImageSource = imageSource,
                Text = message
            };

            _vm.LogItems.Add(logEntry);
            lvLog.ScrollIntoView(logEntry);
        }

        private void RefreshTVFromPath(string rootDirectory)
        {
            string rootDirName;
            _vm.DirectoryList = new ObservableCollection<TVDirectoryItem>();

            // TODO: Add support for multiple root directories

            if (rootDirectory.Length == 3 && rootDirectory.Contains(@":\"))
                rootDirName = rootDirectory;
            else
                rootDirName = System.IO.Path.GetDirectoryName(rootDirectory);

            var rootItem = new TVDirectoryItem()
            {
                FullPath = rootDirectory,
                ImageSource = "/Images/folder.png",
                Text = rootDirName,
                SubItems = new ObservableCollection<TVDirectoryItem>()
            };

            rootItem.SubItems.Add(TVDirectoryItem.GetDummyItem());

            _vm.DirectoryList.Add(rootItem);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var tvi = FindTviFromObjectRecursive(tvFolders, _vm.DirectoryList[0]);
            if (tvi != null) tvi.IsSelected = true;
        }

        private void LvFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            //var menu = FindResource("videoContextMenu") as ContextMenu;

            //if (menu != null && menu.IsOpen)
            //    menu.Visibility = Visibility.Collapsed;

            //if (menu != null && !menu.IsOpen)
            //{
                //_vm.RaisePropertyChanged(nameof(_vm.SelectedVideoMenuItems));
                //_vm.RaisePropertyChanged(nameof(_vm.SelectedVideoMenuVisible));
            //}
        }

        private void VideoFileContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            // Refresh the context menu items/visibility before opening
            _vm.RaisePropertyChanged(nameof(_vm.SelectedVideoMenuItems));
            _vm.RaisePropertyChanged(nameof(_vm.SelectedVideoMenuVisible));

            // Close the context menu if not visible
            if (_vm.SelectedVideoMenuVisible != Visibility.Visible)
                (sender as ContextMenu).IsOpen = false;
        }
    }
}
