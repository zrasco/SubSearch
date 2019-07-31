using GalaSoft.MvvmLight;
using SubSearchUI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SubSearchUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
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

        private ObservableCollection<ItemWithImage> _fileList;

        /// <summary>
        /// Sets and gets the DirectoryList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<ItemWithImage> FileList
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
            FileList = new ObservableCollection<ItemWithImage>
            {
                new ItemWithImage() { Text = "Test 1", ImageSource = "/Images/video.png" },
                new ItemWithImage() { Text = "Test 2", ImageSource = "/Images/video.png" },
                new ItemWithImage() { Text = "Test 3 (no image)" }
            };

            LogItems = new ObservableCollection<ItemWithImage>();

            TestValue = "Wheeeeeee!";
        }
    }
}
