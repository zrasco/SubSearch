using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SubSearchUI.Models;
using System;
using System.Collections.Concurrent;
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

        private SubtitleFileInfo _commandParameter;

        /// <summary>
        /// Sets and gets the CommandParameter property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public SubtitleFileInfo CommandParameter
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
                            retval.Add(new VFIContextMenuItem() { Displayname = $"Delete {info.Filename}...", Icon = "/Images/error.png", Command = DeleteSubtitleCommand, CommandParameter = info });
                        else
                            retval.Add(new VFIContextMenuItem() { Displayname = $"Download {info.Filename}...", Icon = "/Images/download.png", Command = DownloadSubtitleCommand, CommandParameter = info });
                    }
                }

                return retval;
            }
        }

        #region RelayCommands

        private RelayCommand<SubtitleFileInfo> _deleteSubtitleCommand;

        /// <summary>
        /// Gets the DeleteSubtitleCommand.
        /// </summary>
        public RelayCommand<SubtitleFileInfo> DeleteSubtitleCommand
        {
            get
            {
                return _deleteSubtitleCommand ?? (_deleteSubtitleCommand = new RelayCommand<SubtitleFileInfo>(
                    ExecuteDeleteSubtitleCommand,
                    CanExecuteDeleteSubtitleCommand));
            }
        }

        private void ExecuteDeleteSubtitleCommand(SubtitleFileInfo parameter)
        {
            MessageBox.Show($"Deleting subtitle '{parameter.FullPath}'...");
        }

        private bool CanExecuteDeleteSubtitleCommand(SubtitleFileInfo parameter)
        {
            return true;
        }

        private RelayCommand<SubtitleFileInfo> _downloadSubtitleCommand;

        /// <summary>
        /// Gets the DownloadSubtitleCommand.
        /// </summary>
        public RelayCommand<SubtitleFileInfo> DownloadSubtitleCommand
        {
            get
            {
                return _downloadSubtitleCommand ?? (_downloadSubtitleCommand = new RelayCommand<SubtitleFileInfo>(
                    ExecuteDownloadSubtitleCommand,
                    CanExecuteDownloadSubtitleCommand));
            }
        }

        private void ExecuteDownloadSubtitleCommand(SubtitleFileInfo parameter)
        {
            MessageBox.Show($"Downloading subtitle '{parameter.FullPath}'...");
        }

        private bool CanExecuteDownloadSubtitleCommand(SubtitleFileInfo parameter)
        {
            return true;
        }

        private RelayCommand _cancelSelectedQueueItemsCommand;

        /// <summary>
        /// Gets the CancelSelectedQueueItemsCommand.
        /// </summary>
        public RelayCommand CancelSelectedQueueItemsCommand
        {
            get
            {
                return _cancelSelectedQueueItemsCommand ?? (_cancelSelectedQueueItemsCommand = new RelayCommand(
                    ExecuteCancelSelectedQueueItemsCommand,
                    CanExecuteCancelSelectedQueueItemsCommand));
            }
        }

        private void ExecuteCancelSelectedQueueItemsCommand()
        {
            foreach (var queueItem in Scheduler.ItemsQueue.Where(x => x.IsSelected == true))
            {
                queueItem.CancelWork();
            }
        }

        private bool CanExecuteCancelSelectedQueueItemsCommand()
        {
            foreach (var queueItem in Scheduler.ItemsQueue.Where(x => x.IsSelected == true))
            {
                if (queueItem.CanCancel())
                    return true;
            }

            return false;
        }

        #endregion // RelayCommands

        /// <summary>
        /// The <see cref="Scheduler" /> property's name.
        /// </summary>
        public const string SchedulerPropertyName = "Scheduler";

        private Scheduler _scheduler;

        /// <summary>
        /// Sets and gets the Scheduler property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Scheduler Scheduler
        {
            get
            {
                return _scheduler;
            }

            set
            {
                if (_scheduler == value)
                {
                    return;
                }

                _scheduler = value;
                RaisePropertyChanged(SchedulerPropertyName);
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
        /// The <see cref="SelectedDirectory" /> property's name.
        /// </summary>
        public const string SelectedDirectoryPropertyName = "SelectedDirectory";

        private TVDirectoryItem _selectedDirectory;

        /// <summary>
        /// Sets and gets the SelectedDirectory property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public TVDirectoryItem SelectedDirectory
        {
            get
            {
                return _selectedDirectory;
            }

            set
            {
                if (_selectedDirectory == value)
                {
                    return;
                }

                _selectedDirectory = value;
                RaisePropertyChanged(SelectedDirectoryPropertyName);
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

        public MainWindowViewModel(Action<QueueItem> QueueItemFinishedEventHandler)
        {
            DirectoryList = new ObservableCollection<TVDirectoryItem>();
            FileList = new ObservableCollection<VideoFileItem>();
            LogItems = new ObservableCollection<ItemWithImage>();
            Scheduler = new Scheduler(QueueItemFinishedEventHandler);
        }
    }
}
