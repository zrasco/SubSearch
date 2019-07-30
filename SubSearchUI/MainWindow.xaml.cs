using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SubSearchUI.Models;
using SubSearchUI.ViewModels;
using SubSearchUI.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace SubSearchUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private object dummyNode = null;

        private readonly AppSettings _appSettings;
        private readonly MainWindowViewModel _vm;
        private readonly IServiceProvider _services;

        public MainWindow(IServiceProvider services, IOptions<AppSettings> settings, MainWindowViewModel vm)
        {
            InitializeComponent();

            _appSettings = settings.Value;
            _vm = vm;
            _services = services;

            DataContext = vm;

            RefreshTV();

            AddLogEntry("Sample OK entry", ListViewItemTag.ImageType.OK);
            AddLogEntry("Sample info entry", ListViewItemTag.ImageType.Info);
            AddLogEntry("Sample warning entry", ListViewItemTag.ImageType.Warning);
            AddLogEntry("Sample error entry", ListViewItemTag.ImageType.Error);

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
            pWindow.ShowDialog();
        }

        private void TvFolders_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        private void AddLogEntry(string message, ListViewItemTag.ImageType image)
        {
            ListViewItem item = new ListViewItem();
            item.Content = message;
            item.Tag = new ListViewItemTag(message, image);
            item.FontWeight = FontWeights.Normal;

            lvLog.Items.Add(item);
        }

        private void RefreshTV()
        {
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = new TreeViewItemTag(s, TreeViewItemTag.ImageType.DiskDrive);
                item.FontWeight = FontWeights.Normal;
                item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                tvFolders.Items.Add(item);
            }
        }

        private void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items.Count == 1 && item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    foreach (string s in Directory.GetDirectories((item.Tag as TreeViewItemTag).FullPath))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = new TreeViewItemTag(s, TreeViewItemTag.ImageType.Folder);
                        subitem.FontWeight = FontWeights.Normal;
                        subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        item.Items.Add(subitem);
                    }

                    foreach (string s in Directory.GetFiles((item.Tag as TreeViewItemTag).FullPath))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = s.Substring(s.LastIndexOf("\\") + 1);
                        subitem.Tag = new TreeViewItemTag(s, TreeViewItemTag.ImageType.File);
                        subitem.FontWeight = FontWeights.Normal;
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception) { }
            }
        }
    }
}
