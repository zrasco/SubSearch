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

        public PreferencesWindow(   IWritableOptions<AppSettings> appSettingsOpt,
                                    IList<CultureInfo> allCultureInfos,
                                    ObservableCollection<PluginStatus> pluginStatus)
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
            const string SERIES_STR = "series";
            const string SEASON_STR = "season";
            const string EPISODE_STR = "episode";
            const string TITLE_STR = "title";
            const string QUALITY_STR = "quality";
            const string DATE_STR = "date";

            if (_vm.SampleText != null)
            {
                foreach (var regExVal in _vm.RegExList)
                {
                    Regex r = new Regex(regExVal.Expression);

                    Match m = r.Match(_vm.SampleText);

                    if (m.Captures.Count > 0)
                    {
                        GroupCollection gc = m.Groups;

                        if (m.Groups.ContainsKey(SERIES_STR))
                            regExVal.Series = m.Groups[SERIES_STR].Value.Trim();

                        if (m.Groups.ContainsKey(SEASON_STR))
                            regExVal.SeasonNbr = m.Groups[SEASON_STR].Value.Trim();

                        if (m.Groups.ContainsKey(EPISODE_STR))
                            regExVal.EpNbr = m.Groups[EPISODE_STR].Value.Trim();

                        if (m.Groups.ContainsKey(TITLE_STR))
                            regExVal.Title = m.Groups[TITLE_STR].Value.Trim();

                        if (m.Groups.ContainsKey(QUALITY_STR))
                            regExVal.Quality = m.Groups[QUALITY_STR].Value.Trim();

                        if (m.Groups.ContainsKey(DATE_STR))
                            regExVal.Date = m.Groups[DATE_STR].Value.Trim();

                        regExVal.BGColor = Brushes.Green;
                        regExVal.TooltipText = "All items were found in the expression.";
                    }
                    else
                    {
                        regExVal.Series = regExVal.SeasonNbr = regExVal.EpNbr = regExVal.Title = regExVal.Quality = regExVal.Date = "";
                        regExVal.BGColor = Brushes.Red;
                        regExVal.TooltipText = "No expression groups were captured.";
                    }


                }
            }
            // Re-evaluate each regular expression

            
        }
    }
}
