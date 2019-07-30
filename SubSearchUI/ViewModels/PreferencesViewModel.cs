using GalaSoft.MvvmLight;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SubSearchUI.ViewModels
{
    public class PreferencesViewModel : ViewModelBase
    {
        private readonly AppSettings _appSettings;

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

        public PreferencesViewModel(IOptions<AppSettings> appsettings)
        {
            _appSettings = appsettings.Value;

            RootDirectory = _appSettings.RootDirectory;
        }
    }
}
