﻿using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SubSearchUI.Services.Abstract;
using SubSearchUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
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

        public PreferencesWindow(PreferencesViewModel vm, IWritableOptions<AppSettings> appSettingsOpt)
        {
            InitializeComponent();

            _vm = vm;
            _appSettingsOpt = appSettingsOpt;

            // Settings
            _vm.RootDirectory = _appSettingsOpt.Value.RootDirectory;

            DataContext = vm;
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
            });

            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
