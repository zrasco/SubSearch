using GalaSoft.MvvmLight;
using Microsoft.Extensions.Options;
using SubSearchUI.Models;
using SubSearchUI.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace SubSearchUI.ViewModels
{
    public class PreferencesViewModel : ViewModelBase
    {
        /// <summary>
        /// The <see cref="RootDirectory" /> property's name.
        /// </summary>
        public const string RootDirectoryPropertyName = "RootDirectory";

        private string rootDirectory = "";

        /// <summary>
        /// Sets and gets the RootDirectory property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RootDirectory
        {
            get
            {
                return rootDirectory;
            }
            set
            {
                if (rootDirectory == value)
                {
                    return;
                }

                rootDirectory = value;
                RaisePropertyChanged(RootDirectoryPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LanguageList" /> property's name.
        /// </summary>
        public const string LanguageListPropertyName = "LanguageList";

        private IList<CultureInfo> _languageList;

        /// <summary>
        /// Sets and gets the LanguageList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IList<CultureInfo> LanguageList
        {
            get
            {
                return _languageList;
            }

            set
            {
                if (_languageList == value)
                {
                    return;
                }

                _languageList = value;
                RaisePropertyChanged(LanguageListPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedLanguage" /> property's name.
        /// </summary>
        public const string SelectedLanguagePropertyName = "SelectedLanguage";

        private CultureInfo _selectedLanguage;

        /// <summary>
        /// Sets and gets the SelectedLanguage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public CultureInfo SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }

            set
            {
                if (_selectedLanguage == value)
                {
                    return;
                }

                _selectedLanguage = value;
                RaisePropertyChanged(SelectedLanguagePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="PluginList" /> property's name.
        /// </summary>
        public const string PluginListPropertyName = "PluginList";

        private ObservableCollection<PluginStatus> _pluginList;

        /// <summary>
        /// Sets and gets the PluginList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<PluginStatus> PluginList
        {
            get
            {
                return _pluginList;
            }

            set
            {
                if (_pluginList == value)
                {
                    return;
                }

                _pluginList = value;
                RaisePropertyChanged(PluginListPropertyName);
            }
        }

        public PreferencesViewModel()
        {
        }
    }
}
