using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSearchUI.Models
{
    class ListViewItemTag : ObservableObject
    {
        public enum ImageType { OK, Info, Warning, Error };

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
        /// The <see cref="Message" /> property's name.
        /// </summary>
        public const string MessagePropertyName = "Message";

        private string _message = "";

        /// <summary>
        /// Sets and gets the FullPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                if (_message == value)
                {
                    return;
                }

                _message = value;
                RaisePropertyChanged(MessagePropertyName);
            }
        }

        public ListViewItemTag(string message, ImageType imageType)
        {
            Message = message;
            if (imageType == ImageType.OK)
                ImageUri = "pack://application:,,,/Images/ok.png";
            else if (imageType == ImageType.Info)
                ImageUri = "pack://application:,,,/Images/info.png";
            else if (imageType == ImageType.Warning)
                ImageUri = "pack://application:,,,/Images/warning.png";
            else if (imageType == ImageType.Error)
                ImageUri = "pack://application:,,,/Images/error.png";
        }
    }
}
