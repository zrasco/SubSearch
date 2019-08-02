using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubSearchUI.Models
{
    public class QueueItem : ObservableObject
    {
        /// <summary>
        /// The <see cref="Text" /> property's name.
        /// </summary>
        public const string TextPropertyName = "Text";

        private string _text;

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
        /// The <see cref="Status" /> property's name.
        /// </summary>
        public const string StatusPropertyName = "Status";

        private string _status;

        /// <summary>
        /// Sets and gets the Status property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (_status == value)
                {
                    return;
                }

                _status = value;
                RaisePropertyChanged(StatusPropertyName);
            }
        }
    }
}
