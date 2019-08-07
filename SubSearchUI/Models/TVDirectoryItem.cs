using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;

namespace SubSearchUI.Models
{
    public class TVDirectoryItem : TVItemWithImage
    {
        public static TVDirectoryItem GetDummyItem()
        {
            return new TVDirectoryItem()
            {
                FullPath = "",
                ImageSource = "/Images/Folder.png",
                Text = "",
                SubItems = null
            };
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
        /// The <see cref="IsNodeExpanded" /> property's name.
        /// </summary>
        public const string IsNodeExpandedPropertyName = "IsNodeExpanded";

        private bool _isNodeExpanded = false;

        /// <summary>
        /// Sets and gets the IsNodeExpanded property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNodeExpanded
        {
            get
            {
                return _isNodeExpanded;
            }

            set
            {
                if (_isNodeExpanded == value)
                {
                    return;
                }

                _isNodeExpanded = value;

                SubItems = new ObservableCollection<TVDirectoryItem>();

                if (IsNodeExpanded == true)
                {
                    try
                    {
                        // Add the subdirectories
                        foreach (string s in Directory.GetDirectories(FullPath))
                        {
                            TVDirectoryItem subDirEntry = new TVDirectoryItem()
                            {
                                FullPath = s,
                                ImageSource = "/Images/folder.png",
                                Text = System.IO.Path.GetRelativePath(FullPath, s),
                                SubItems = new ObservableCollection<TVDirectoryItem>()
                            };

                            subDirEntry.SubItems.Add(GetDummyItem());

                            SubItems.Add(subDirEntry);
                        }

                        //if (emptyDir == true)
                        //    IsNodeExpanded = false;
                    }
                    catch (Exception ex)
                    {
                        // TODO: Add exception to logger the "correct" way and remove tight coupling

                        (Application.Current.MainWindow as MainWindow).AddLogEntry(ex.Message, LogLevel.Error);
                        IsNodeExpanded = false;

                        //throw ex;
                    }


                }
                else
                {
                    SubItems.Add(GetDummyItem());
                }


                RaisePropertyChanged(IsNodeExpandedPropertyName);
            }
        }
    }
}
