using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SubSearchUI.Models
{
    public enum QueueStatus {
        [Description("In Queue")]
        InQueue,
        [Description("In Progress")]
        InProgress,
        [Description("Complete")]
        Complete,
        [Description("Failed")]
        Failed,
        [Description("Cancelled")]
        Cancelled,
        [Description("Aborted")]
        Aborted
    };
    public class QueueItem : ObservableObject
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
        /// The <see cref="TaskText" /> property's name.
        /// </summary>
        public const string TaskTextPropertyName = "TaskText";

        private string _text;

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string TaskText
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
                RaisePropertyChanged(TaskTextPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Status" /> property's name.
        /// </summary>
        public const string StatusPropertyName = "Status";

        private QueueStatus _status;

        /// <summary>
        /// Sets and gets the Status property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public QueueStatus Status
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

                if (_status == QueueStatus.Aborted || _status == QueueStatus.Failed)
                    TextColor = Brushes.Red;
                else if (_status == QueueStatus.Cancelled)
                    TextColor = Brushes.Gray;
                else if (_status == QueueStatus.Complete)
                {
                    if (ProgressPercentage != 1)
                        ProgressPercentage = 1;

                    TextColor = Brushes.Green;
                }
                else if (_status == QueueStatus.InProgress)
                    TextColor = Brushes.Blue;
                else
                    TextColor = Brushes.Black;

                RaisePropertyChanged(ProgressTextPropertyName);
                RaisePropertyChanged(StatusPropertyName);
            }
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
        /// The <see cref="ProgressPercentage" /> property's name.
        /// </summary>
        public const string ProgressPercentagePropertyName = "ProgressPercentage";

        private double _progressPercentage;

        /// <summary>
        /// Sets and gets the ProgressPercentage property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public double ProgressPercentage
        {
            get
            {
                return _progressPercentage;
            }

            set
            {
                if (_progressPercentage == value)
                {
                    return;
                }

                _progressPercentage = value;
                RaisePropertyChanged(ProgressTextPropertyName);
                RaisePropertyChanged(ProgressPercentagePropertyName);
            }
        }

        public const string ProgressTextPropertyName = "ProgressText";
        public string ProgressText
        {
            get
            {
                if (Status == QueueStatus.InQueue)
                    return "Waiting";
                else if (Status == QueueStatus.Cancelled)
                    return "Cancelled";
                else
                    return String.Format("{0:P2}", ProgressPercentage);
            }
        }

        /// <summary>
        /// The <see cref="TaskText" /> property's name.
        /// </summary>
        public const string DetailsTextPropertyName = "DetailsText";

        private string _detailsText;

        /// <summary>
        /// Sets and gets the Text property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string DetailsText
        {
            get
            {
                return _detailsText;
            }

            set
            {
                if (_detailsText == value)
                {
                    return;
                }

                _detailsText = value;
                RaisePropertyChanged(DetailsTextPropertyName);
            }
        }

        public Task<bool> DoWorkAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            var retval = Task.Run(() =>
            {
                // Get the current thread in case we need to cancel the task
                try
                {
                    return Work(this, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Status = QueueStatus.Aborted;
                    return false;
                }
                
            }, _cancellationTokenSource.Token );

            return retval;
        }

        public bool CanCancel()
        {
            if (Status == QueueStatus.InProgress || Status == QueueStatus.InQueue)
                return true;
            else
                return false;
        }

        public void CancelWork()
        {
            if (CanCancel())
            {
                if (_cancellationTokenSource != null)
                {
                    // Task has already started
                    Status = QueueStatus.Aborted;
                    _cancellationTokenSource.Cancel();

                    //_threadToCancel.Abort();
                }
                else
                    Status = QueueStatus.Cancelled;
            }
        }

        public Func<QueueItem, CancellationToken, bool> Work { get; set; }
        public Action<QueueItem> DoWhenDone { get; set; }

        private CancellationTokenSource _cancellationTokenSource;
    }
}
