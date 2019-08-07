using GalaSoft.MvvmLight;
using ProviderPluginTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SubSearchUI.Models
{
    public enum PluginLoadStatus
    {
        [Description("Not loaded")]
        NotLoaded,
        [Description("Scheduled to load")]
        Scheduled,
        [Description("Loading")]
        Loading,
        [Description("Loaded")]
        Loaded
    }
    // Not part of the application settings. PluginStatus exists in memory only
    public class PluginStatus : ObservableObject
    {
        public PluginStatus(Plugin pluginInfo)
        {
            Name = pluginInfo.Name;
            File = pluginInfo.File;
        }

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
        /// The <see cref="Interface" /> property's name.
        /// </summary>
        public const string InterfacePropertyName = "Interface";

        private IProviderPlugin _interface;

        /// <summary>
        /// Sets and gets the Interface property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IProviderPlugin Interface
        {
            get
            {
                return _interface;
            }

            set
            {
                if (_interface == value)
                {
                    return;
                }

                _interface = value;
                RaisePropertyChanged(InterfacePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Status" /> property's name.
        /// </summary>
        public const string StatusPropertyName = "Status";

        private PluginLoadStatus _status = PluginLoadStatus.NotLoaded;

        /// <summary>
        /// Sets and gets the IsLoaded property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public PluginLoadStatus Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;
                RaisePropertyChanged(StatusPropertyName);
            }
        }
    }
}
