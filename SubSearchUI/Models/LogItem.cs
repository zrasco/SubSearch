using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace SubSearchUI.Models
{
    public class LogItem : ItemWithImage
    {
        /// <summary>
        /// The <see cref="TimeStampColor" /> property's name.
        /// </summary>
        public const string TimeStampColorPropertyName = "TimeStampColor";

        private Brush _timeStampTextColor = Brushes.Gray;

        /// <summary>
        /// Sets and gets the TextColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush TimeStampColor
        {
            get
            {
                return _timeStampTextColor;
            }

            set
            {
                if (_timeStampTextColor == value)
                {
                    return;
                }

                _timeStampTextColor = value;
                RaisePropertyChanged(TimeStampColorPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="TimeStamp" /> property's name.
        /// </summary>
        public const string TimeStampPropertyName = "TimeStamp";

        private string _timeStampText = null;

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TimeStamp
        {
            get
            {
                return _timeStampText;
            }

            set
            {
                if (_timeStampText == value)
                {
                    return;
                }

                _timeStampText = value;
                RaisePropertyChanged(TimeStampPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SourceContextColor" /> property's name.
        /// </summary>
        public const string SourceContextColorPropertyName = "SourceContextColor";

        private Brush _sourceContextTextColor = Brushes.Gray;

        /// <summary>
        /// Sets and gets the TextColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Brush SourceContextColor
        {
            get
            {
                return _sourceContextTextColor;
            }

            set
            {
                if (_sourceContextTextColor == value)
                {
                    return;
                }

                _sourceContextTextColor = value;
                RaisePropertyChanged(SourceContextColorPropertyName);
            }
        }
        /// <summary>
        /// The <see cref="SourceContext" /> property's name.
        /// </summary>
        public const string SourceContextPropertyName = "SourceContext";

        private string _sourceContextText = null;

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string SourceContext
        {
            get
            {
                return _sourceContextText;
            }

            set
            {
                if (_sourceContextText == value)
                {
                    return;
                }

                _sourceContextText = value;
                RaisePropertyChanged(SourceContextPropertyName);
            }
        }
    }
}
