using GalaSoft.MvvmLight;
using Microsoft.Extensions.Options;
using SubSearchUI.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace SubSearchUI.ViewModels
{
    public class PluginInfo : ObservableObject
    {
        /// <summary>
        /// The <see cref="Name" /> property's name.
        /// </summary>
        public const string NamePropertyName = "Name";

        private string _name;

        /// <summary>
        /// Sets and gets the Name property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                RaisePropertyChanged(NamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="File" /> property's name.
        /// </summary>
        public const string FilePropertyName = "File";

        private string _file;

        /// <summary>
        /// Sets and gets the File property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string File
        {
            get
            {
                return _file;
            }

            set
            {
                if (_file == value)
                {
                    return;
                }

                _file = value;
                RaisePropertyChanged(FilePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="IsLoaded" /> property's name.
        /// </summary>
        public const string IsLoadedPropertyName = "IsLoaded";

        private bool _isLoaded = false;

        /// <summary>
        /// Sets and gets the IsLoaded property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return _isLoaded;
            }

            set
            {
                if (_isLoaded == value)
                {
                    return;
                }

                _isLoaded = value;
                RaisePropertyChanged(IsLoadedPropertyName);
            }
        }
    }
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

        private List<CultureInfo> _languageList;

        /// <summary>
        /// Sets and gets the LanguageList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public List<CultureInfo> LanguageList
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

        private ObservableCollection<PluginInfo> _pluginList;

        /// <summary>
        /// Sets and gets the PluginList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<PluginInfo> PluginList
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
            PluginList = new ObservableCollection<PluginInfo>();
        }
    }
}
