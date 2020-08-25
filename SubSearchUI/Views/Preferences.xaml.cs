using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SubSearchUI.Models;
using SubSearchUI.Services.Abstract;
using SubSearchUI.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SubSearchUI.Views
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private readonly PreferencesViewModel _vm;
        private readonly IWritableOptions<AppSettings> _appSettingsOpt;
        private readonly IFilenameProcessor _filenameProcessor;

        public PreferencesWindow(   IWritableOptions<AppSettings> appSettingsOpt,
                                    IList<CultureInfo> allCultureInfos,
                                    ObservableCollection<PluginStatus> pluginStatus,
                                    IFilenameProcessor filenameProcessor)
        {
            InitializeComponent();

            _vm = new PreferencesViewModel();
            _appSettingsOpt = appSettingsOpt;

            // Settings
            _vm.RootDirectory = _appSettingsOpt.Value.RootDirectory;

            // Language list
            _vm.LanguageList = allCultureInfos;

            // Plugin status
            _vm.PluginList = pluginStatus;

            // Regular expressions
            _vm.RegExList = new ObservableCollection<RegExTVItem>();
            foreach (var expr in _appSettingsOpt.Value.RegExTV)
            {
                _vm.RegExList.Add(new RegExTVItem() { Expression = expr });
            }

            // Filename processor
            _filenameProcessor = filenameProcessor;

            _vm.SampleText = "In Living Color - S01E01 - Pilot [Unknown] [1990-04-15]";

            DataContext = _vm;
        }
        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog() { IsFolderPicker = true };

            if (dlg.ShowDialog(this) == CommonFileDialogResult.Ok)
            {
                _vm.RootDirectory = dlg.FileName;
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            _appSettingsOpt.Update(o =>
            {
                o.RootDirectory = _vm.RootDirectory;
                o.DefaultLanguage = _vm.SelectedLanguage.DisplayName;
            });

            DialogResult = true;

            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.SelectedLanguage = _vm.LanguageList.Where(x => x.DisplayName == _appSettingsOpt.Value.DefaultLanguage).FirstOrDefault();
        }

        private void TxtBoxSampleText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_vm.SampleText != null)
            {
                foreach (var regExVal in _vm.RegExList)
                {
                    // Evaluate each regular expression
                    var info = _filenameProcessor.InfoFromFilebase(_vm.SampleText, regExVal.Expression);

                    regExVal.FillFrom(info);

                    if (regExVal.IsComplete())
                    {
                        regExVal.BGColor = Brushes.Green;
                        regExVal.TooltipText = "All items were found in the expression.";
                    }
                    else
                    {
                        regExVal.BGColor = Brushes.Red;
                        regExVal.TooltipText = "No expression groups were captured.";
                    }

                }
            }         
        }
    }
}
