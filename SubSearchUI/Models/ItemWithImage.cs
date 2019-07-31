using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace SubSearchUI.Models
{
    public class ItemWithImage : ObservableObject
    {
        /// <summary>
        /// The <see cref="TextColor" /> property's name.
        /// </summary>
        public const string TextColorPropertyName = "TextColor";

        private Brush _textColor = Brushes.Black;

        /// <summary>
        /// Sets and gets the TextColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush TextColor
        {
            get
            {
                return _textColor;
            }

            set
            {
                if (_textColor == value)
                {
                    return;
                }

                _textColor = value;
                RaisePropertyChanged(TextColorPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private string _text = null;

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                if (_text == value)
                {
                    return;
                }

                _text = value;
                RaisePropertyChanged(TextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ImageSource" /> property's name.
        /// </summary>
        public const string ImageSourcePropertyName = "ImageSource";

        private string _imageSource = null;

        /// <summary>
        /// Sets and gets the ImageSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ImageSource
        {
            get
            {
                return _imageSource;
            }

            set
            {
                if (_imageSource == value)
                {
                    return;
                }

                _imageSource = value;
                RaisePropertyChanged(ImageSourcePropertyName);
            }
        }
    }
}
