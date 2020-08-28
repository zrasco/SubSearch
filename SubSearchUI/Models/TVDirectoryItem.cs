using Microsoft.Extensions.DependencyInjection;
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
        private readonly ILogger<TVDirectoryItem> _logger;
        public TVDirectoryItem Parent { get; }

        public TVDirectoryItem(string fullPath, TVDirectoryItem parent = null)
        {
            var serviceProvider = (App.Current as App).GetServiceProvider();
            _logger = serviceProvider.GetRequiredService<ILogger<TVDirectoryItem>>();

            Parent = parent;
            FullPath = fullPath;
            ImageSource = "/Images/folder.png";

            if (parent == null)
                Text = fullPath;
            else
                Text = System.IO.Path.GetRelativePath(Parent.FullPath, fullPath);

            // Add items for subdirectories, if any exist
            try
            {
                var dirList = Directory.GetDirectories(FullPath);

                if (dirList.Length > 0)
                {
                    SubItems = new ObservableCollection<TVDirectoryItem>();

                    // Add the subdirectories
                    foreach (string s in dirList)
                    {
                        TVDirectoryItem subDirEntry = new TVDirectoryItem(s, this);
                        SubItems.Add(subDirEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Unable to process directory ({fullPath}): {ex.Message}");
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
                else
                    _isNodeExpanded = value;

                RaisePropertyChanged(IsNodeExpandedPropertyName);
            }
        }
    }
}
