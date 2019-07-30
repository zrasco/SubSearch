using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace SubSearchUI.Models
{
    class TreeViewItemTag : ObservableObject
    {
        public enum ImageType { DiskDrive, Folder, File };

        private bool IsDirectory()
        {
            bool retval = false;

            try
            {
                if ((File.GetAttributes(FullPath) & FileAttributes.Directory) == FileAttributes.Directory)
                    retval = true;
            }
            catch (Exception ex)
            {
                // Do nothing. Temporarily display a message box
                MessageBox.Show(ex.Message);
            }

            return retval;
        }
        /// <summary>
        /// The <see cref="ImageUri" /> property's name.
        /// </summary>
        public const string ImageUriPropertyName = "ImageUri";

        private string _imageUri = null;

        /// <summary>
        /// Sets and gets the ImageUri property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ImageUri
        {
            get
            {
                return _imageUri;
            }

            set
            {
                if (_imageUri == value)
                {
                    return;
                }

                _imageUri = value;
                RaisePropertyChanged(ImageUriPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="FullPath" /> property's name.
        /// </summary>
        public const string FullPathPropertyName = "FullPath";

        private string _fullPath = "";

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

        public TreeViewItemTag(string fullpath, ImageType? imageType = null)
        {
            FullPath = fullpath;

            if (imageType == null)
            {
                // Figure it out
                if (FullPath.Length == 3 && FullPath.Contains(@":\"))
                    ImageUri = "pack://application:,,,/Images/diskdrive.png";
                else if (IsDirectory())
                    ImageUri = "pack://application:,,,/Images/folder.png";
                else
                    ImageUri = "pack://application:,,,/Images/video.png";
            }
            else if (imageType == ImageType.DiskDrive)
                ImageUri = "pack://application:,,,/Images/diskdrive.png";
            else if (imageType == ImageType.Folder)
                ImageUri = "pack://application:,,,/Images/folder.png";
            else if (imageType == ImageType.File)
                ImageUri = "pack://application:,,,/Images/video.png";
        }
    }
}
