using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace SubSearchUI.Models
{
    public class SubtitleFileInfo
    {
        public CultureInfo CultureInfo { get; set; }
        public string Filebase { get; set; }
        public string FullPath { get; set; }
        public string Filename { get; set; }
        public bool Exists { get; set; }
    }

    public class VideoFileItem : ItemWithImage
    {
        public VideoFileItem()
        {
            SubtitleFileList = new List<SubtitleFileInfo>();
        }
        /// <summary>
        /// The <see cref="IsSelected" /> property's name.
        /// </summary>
        public const string IsSelectedPropertyName = "IsSelected";

        private bool _isSelected = false;

        /// <summary>
        /// Sets and gets the IsSelected property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                RaisePropertyChanged(IsSelectedPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="FullPath" /> property's name.
        /// </summary>
        public const string FullPathPropertyName = "FullPath";

        private string _fullPath;

        /// <summary>
        /// Sets and gets the FullPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string FullPath
        {
            get
            {
                return _fullPath;
            }

            set
            {
                if (_fullPath == value)
                {
                    return;
                }

                _fullPath = value;
                RaisePropertyChanged(FullPathPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SubtitleFileList" /> property's name.
        /// </summary>
        public const string SubtitleFileListPropertyName = "SubtitleFileList";

        private IList<SubtitleFileInfo> _subtitleFileList;

        /// <summary>
        /// Sets and gets the SubtitleFileList property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public IList<SubtitleFileInfo> SubtitleFileList
        {
            get
            {
                return _subtitleFileList;
            }

            set
            {
                if (_subtitleFileList == value)
                {
                    return;
                }

                _subtitleFileList = value;
                RaisePropertyChanged(SubtitleFileListPropertyName);
            }
        }

        private RelayCommand<string> _deleteFileCommand;

        /// <summary>
        /// Gets the DeleteFileCommand.
        /// </summary>
        public RelayCommand<string> DeleteFileCommand
        {
            get
            {
                return _deleteFileCommand
                    ?? (_deleteFileCommand = new RelayCommand<string>(DeleteFileCommandMethod));
            }
        }

        private void DeleteFileCommandMethod(string filename)
        {
            var result = MessageBox.Show($"Are you sure you wish to delete {filename}?", "Delete confirmation", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

            if (result == MessageBoxResult.Yes)
            {
                // Delete file
            }
        }

        public void GenerateSubtitleInfo(string defaultLanguage, IList<CultureInfo> allCultureInfos)
        {
            string fileBase = $"{Path.GetDirectoryName(FullPath)}\\{Path.GetFileNameWithoutExtension(FullPath)}";
            CultureInfo defaultCultureInfo = allCultureInfos.Where(x => x.DisplayName == defaultLanguage).FirstOrDefault();

            if (defaultCultureInfo == null)
            {
                // TODO: Add logging
                //AddLogEntry($"Unable to look up culture info for language ''", ImageType.Error);
            }
            else
            {
                SubtitleFileList = new List<SubtitleFileInfo>();

                SubtitleFileList.Add(new SubtitleFileInfo() { CultureInfo = defaultCultureInfo, Filebase = Path.GetFileName(fileBase), FullPath = fileBase + ".srt" });
                SubtitleFileList.Add(new SubtitleFileInfo() { CultureInfo = defaultCultureInfo, Filebase = Path.GetFileName(fileBase), FullPath = fileBase + "." + defaultCultureInfo.TwoLetterISOLanguageName + ".srt" });

                if (defaultCultureInfo.TwoLetterISOLanguageName != defaultCultureInfo.Name)
                    SubtitleFileList.Add(new SubtitleFileInfo() { CultureInfo = defaultCultureInfo, Filebase = Path.GetFileName(fileBase), FullPath = fileBase + "." + defaultCultureInfo.Name + ".srt" });

                foreach (var info in SubtitleFileList)
                {
                    info.Filename = Path.GetFileName(info.FullPath);
                    info.Exists = File.Exists(info.FullPath);
                }
            }

            if (SubtitleFileList.Where(x => x.Exists).Any())
                TextColor = Brushes.Green;
            else
                TextColor = Brushes.Red;
        }
    }
}
