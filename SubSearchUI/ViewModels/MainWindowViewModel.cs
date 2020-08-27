using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Extensions.Logging;
using ProviderPluginTypes;
using Serilog.Events;
using SubSearchUI.Models;
using SubSearchUI.Services.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

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
                    // TODO: Add more useful context menu for multiple file selection
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
            if (MessageBox.Show($"Are you sure you want to delete {parameter.Filename}?","Subtitle deletion confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                File.Delete(parameter.FullPath);
                parameter.Exists = false;
                parameter.Parent.RefreshColor();
            }
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

        private async void ExecuteDownloadSubtitleCommand(SubtitleFileInfo parameter)
        {
            // Extract the relevant info from the filename
            TVShowValue fileInfo = _filenameProcessor.GetTVShowInfo(parameter);

            if (fileInfo != null)
            {
                // Add subtitle download to queue
                Scheduler.AddItem($"Downloading {parameter.Filename}...", (item, cancellation) =>
                {
                    try
                    {
                        foreach (var pluginStatus in PluginStatusList)
                        {
                            // Cancel if needed
                            cancellation.ThrowIfCancellationRequested();

                            if (pluginStatus.Status == PluginLoadStatus.Loaded)
                            {
                                if ((pluginStatus.Interface.ProviderCapabilities() & SearchCapabilities.TV) == SearchCapabilities.TV)
                                {
                                    IList<DownloadedSubtitle> downloadedSubs = new List<DownloadedSubtitle>();
                                    ConcurrentQueue<ProviderSignal> cQueue = new ConcurrentQueue<ProviderSignal>();

                                    //downloadedSubs = pluginStatus.Interface.SearchSubtitlesForTVAsync(fileInfo.Series, fileInfo.Season.GetValueOrDefault(), fileInfo.EpisodeNbr.GetValueOrDefault(), new List<CultureInfo>() { parameter.CultureInfo }).Result;
                                    var downloadTask = pluginStatus.Interface.SearchSubtitlesForTVAsync(fileInfo.Series, fileInfo.Season.GetValueOrDefault(), fileInfo.EpisodeNbr.GetValueOrDefault(), new List<CultureInfo>() { parameter.CultureInfo }, cQueue);

                                    while (!cQueue.IsEmpty || downloadTask.Status == TaskStatus.WaitingForActivation || downloadTask.Status == TaskStatus.Running)
                                    {
                                        // Check for signals from provider task
                                        if (cQueue.TryDequeue(out ProviderSignal result) == true)
                                        {
                                            if (result.Type == ProviderSignal.SignalType.ChangePercentage)
                                                item.ProgressPercentage = result.DoubleValue;
                                            else if (result.Type == ProviderSignal.SignalType.ChangeDetails)
                                                item.DetailsText = result.TextValue;
                                        }

                                        cancellation.ThrowIfCancellationRequested();
                                        System.Threading.Thread.Sleep(10);
                                    }

                                    // Cancel if needed
                                    //cancellation.ThrowIfCancellationRequested();

                                    // Get the result and process
                                    downloadedSubs = downloadTask.Result;

                                    // TODO: Decide what to do if multiple subs are downloaded. For now, just pick this one.
                                    if (downloadedSubs.Count > 0)
                                    {
                                        using (FileStream fs = new FileStream(parameter.FullPath, FileMode.Create))
                                        {
                                            downloadedSubs[0].Contents.CopyTo(fs);
                                            downloadedSubs[0].Contents.Close();
                                        }

                                        parameter.Exists = true;
                                        parameter.Parent.RefreshColor();

                                        item.DetailsText = $"Successfully downloaded subtitle to path ({parameter.FullPath})";
                                        return true;
                                    }

                                }
                            }
                        }

                        string error = $"Unable to find matching subtitle for ({parameter.Filename})";
                        _logger.LogError(error);
                        item.DetailsText = error;
                        return false;
                        //throw new Exception("Unable to find matching subtitle");
                    }
                    catch (Exception ex)
                    {
                        string error = $"Exception finding subtitle for ({parameter.Filename}): {ex.Message}";
                        _logger.LogError(error);
                        item.DetailsText = error;
                        return false;
                    }



                });
            }
            else
            {
                _logger.LogError($"Unable to obtain series, season, or episode info from file ({parameter.Filename}). Please rename file using standard naming conventions.");
            }
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

        public void SetStatusText(string text, bool alsoLog = true, LogLevel logLevel = LogLevel.Information)
        {
            // TODO: Add status text color for different logLevels
            StatusText = text;

            if (alsoLog)
                _logger.Log(logLevel, text);
        }
        /// <summary>
        /// The <see cref="StatusText" /> property's name.
        /// </summary>
        public const string StatusTextPropertyName = "StatusText";

        private string _statusText;

        /// <summary>
        /// Sets and gets the StatusText property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string StatusText
        {
            get
            {
                return _statusText;
            }

            set
            {
                if (_statusText == value)
                {
                    return;
                }

                _statusText = value;
                RaisePropertyChanged(StatusTextPropertyName);
            }
        }
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

        private ObservableCollection<LogItem> _logItems;

        /// <summary>
        /// Sets and gets the LogItems property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<LogItem> LogItems
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

        /// <summary>
        /// The <see cref="PluginStatusList" /> property's name.
        /// </summary>
        public const string PluginStatusListPropertyName = "PluginStatusList";

        private ObservableCollection<PluginStatus> pluginStatusList;

        /// <summary>
        /// Sets and gets the PluginStatusList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<PluginStatus> PluginStatusList
        {
            get
            {
                return pluginStatusList;;
            }

            set
            {
                if (pluginStatusList == value)
                {
                    return;
                }

                pluginStatusList = value;
                RaisePropertyChanged(PluginStatusListPropertyName);
            }
        }

        private readonly IFilenameProcessor _filenameProcessor;
        private readonly ILogger<MainWindowViewModel> _logger;

        public MainWindowViewModel(//Action<QueueItem> QueueItemFinishedEventHandler,
                                    IFilenameProcessor filenameProcessor,
                                    ILogger<MainWindowViewModel> logger)
        {
            DirectoryList = new ObservableCollection<TVDirectoryItem>();
            FileList = new ObservableCollection<VideoFileItem>();
            LogItems = new ObservableCollection<LogItem>();
            //Scheduler = new Scheduler(QueueItemFinishedEventHandler);

            _filenameProcessor = filenameProcessor;
            _logger = logger;
        }
    }
}
