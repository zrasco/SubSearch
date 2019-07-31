using GalaSoft.MvvmLight;
using SubSearchUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SubSearchUI.ViewModels
{
    public class VFIContextMenuItem : ObservableObject
    {
        /// <summary>
        /// The <see cref="Displayname" /> property's name.
        /// </summary>
        public const string DisplaynamePropertyName = "Displayname";

        private string _displayName;

        /// <summary>
        /// Sets and gets the Displayname property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Displayname
        {
            get
            {
                return _displayName;
            }

            set
            {
                if (_displayName == value)
                {
                    return;
                }

                _displayName = value;
                RaisePropertyChanged(DisplaynamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Command" /> property's name.
        /// </summary>
        public const string CommandPropertyName = "Command";

        private ICommand _command;

        /// <summary>
        /// Sets and gets the Command property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ICommand Command
        {
            get
            {
                return _command;
            }

            set
            {
                if (_command == value)
                {
                    return;
                }

                _command = value;
                RaisePropertyChanged(CommandPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="CommandParameter" /> property's name.
        /// </summary>
        public const string CommandParameterPropertyName = "CommandParameter";

        private string _commandParameter;

        /// <summary>
        /// Sets and gets the CommandParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string CommandParameter
        {
            get
            {
                return _commandParameter;
            }

            set
            {
                if (_commandParameter == value)
                {
                    return;
                }

                _commandParameter = value;
                RaisePropertyChanged(CommandParameterPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Icon" /> property's name.
        /// </summary>
        public const string IconPropertyName = "Icon";

        private string _icon;

        /// <summary>
        /// Sets and gets the Icon property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Icon
        {
            get
            {
                return _icon;
            }

            set
            {
                if (_icon == value)
                {
                    return;
                }

                _icon = value;
                RaisePropertyChanged(IconPropertyName);
            }
        }
    }

    public class MainWindowViewModel : ViewModelBase
    {
        public Visibility SelectedVideoMenuVisible
        {
            get
            {
                var SelectedVideo = FileList.Where(x => x.IsSelected == true).FirstOrDefault();

                if (SelectedVideo != null && !string.IsNullOrEmpty(SelectedVideo.FullPath))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public ObservableCollection<VFIContextMenuItem> SelectedVideoMenuItems
        {
            get
            {
                var retval = new ObservableCollection<VFIContextMenuItem>();
                var SelectedVideos = FileList.Where(x => x.IsSelected == true).ToList();

                if (SelectedVideos.Count > 1)
                {
                    retval.Add(new VFIContextMenuItem() { Displayname = $"{SelectedVideos.Count} files selected", Icon = "/Images/error.png", CommandParameter = null });
                }
                else if (SelectedVideos.Count == 1)
                {
                    foreach (var info in SelectedVideos[0].SubtitleFileList)
                    {
                        if (info.Exists)
                            retval.Add(new VFIContextMenuItem() { Displayname = $"Delete {info.Filename}...", Icon = "/Images/error.png", CommandParameter = info.FullPath });
                        else
                            retval.Add(new VFIContextMenuItem() { Displayname = $"Download {info.Filename}...", Icon = "/Images/download.png", CommandParameter = info.FullPath });
                    }
                }

                return retval;
            }
        }

        /// <summary>
        /// The <see cref="DirectoryList" /> property's name.
        /// </summary>
        public const string DirectoryListPropertyName = "DirectoryList";

        private ObservableCollection<TVDirectoryItem> _directoryList;

        /// <summary>
        /// Sets and gets the DirectoryList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<TVDirectoryItem> DirectoryList
        {
            get
            {
                return _directoryList;
            }

            set
            {
                if (_directoryList == value)
                {
                    return;
                }

                _directoryList = value;
                RaisePropertyChanged(() => DirectoryList);
            }
        }

        /// <summary>
        /// The <see cref="DirectoryList" /> property's name.
        /// </summary>
        public const string FileListPropertyName = "FileList";

        private ObservableCollection<VideoFileItem> _fileList;

        /// <summary>
        /// Sets and gets the DirectoryList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<VideoFileItem> FileList
        {
            get
            {
                return _fileList;
            }

            set
            {
                if (_fileList == value)
                {
                    return;
                }

                _fileList = value;
                RaisePropertyChanged(() => FileList);
            }
        }

        /// <summary>
        /// The <see cref="LogItems" /> property's name.
        /// </summary>
        public const string LogItemsPropertyName = "LogItems";

        private ObservableCollection<ItemWithImage> _logItems;

        /// <summary>
        /// Sets and gets the LogItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ItemWithImage> LogItems
        {
            get
            {
                return _logItems;
            }

            set
            {
                if (_logItems == value)
                {
                    return;
                }

                _logItems = value;
                RaisePropertyChanged(LogItemsPropertyName);
            }
        }

        public string TestValue { get; set; }

        public MainWindowViewModel()
        {
            DirectoryList = new ObservableCollection<TVDirectoryItem>();
            FileList = new ObservableCollection<VideoFileItem>();
            LogItems = new ObservableCollection<ItemWithImage>();

            TestValue = "Wheeeeeee!";
        }
    }
}
