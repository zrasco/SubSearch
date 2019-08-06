using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SubSearchUI.Models
{
    public class Scheduler : ObservableObject
    {
        public event Action<QueueItem> QueueItemStatusChange;

        public Scheduler(Action<QueueItem> onQueueItemStatusChangeEventHandler)
        {
            ItemsQueue = new ObservableCollection<QueueItem>();
            QueueItemStatusChange += onQueueItemStatusChangeEventHandler;
        }

        public QueueItem AddItem(string text, Func<QueueItem, bool> actionWithResult)
        {
            QueueItem queueItem = new QueueItem() { Text = text, Status = QueueStatus.InQueue, Work = actionWithResult };

            ItemsQueue.Add(queueItem);

            return queueItem;
        }

        private void StartItemAsync(QueueItem queueItem)
        {
            ItemsRunning++;
            Task<bool> worker = queueItem.DoWorkAsync();

            worker.ContinueWith(x =>
            {
                ItemsRunning--;

                if (queueItem.Status != QueueStatus.Aborted)
                {
                    if (x.Result == false)
                        queueItem.Status = QueueStatus.Failed;
                    else
                        queueItem.Status = QueueStatus.Complete;

                    QueueItemStatusChange.Invoke(queueItem);
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        
        public void Cycle(int maxBackgroundJobs)
        {
            if (ItemsRunning < maxBackgroundJobs)
            {
                var inQueueJobs = ItemsQueue.Where(x => x.Status == QueueStatus.InQueue);

                while (ItemsQueue.Where(x => x.Status == QueueStatus.InQueue).Count() > 0 && ItemsRunning < maxBackgroundJobs)
                {
                    var nextJob = ItemsQueue.Where(x => x.Status == QueueStatus.InQueue).FirstOrDefault();

                    if (nextJob != null)
                    {
                        nextJob.Status = QueueStatus.InProgress;
                        QueueItemStatusChange.Invoke(nextJob);
                        StartItemAsync(nextJob);
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="ItemsQueue" /> property's name.
        /// </summary>
        public const string ItemsQueuePropertyName = "ItemsQueue";

        private ObservableCollection<QueueItem> _itemsQueue;

        /// <summary>
        /// Sets and gets the ItemsQueue property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<QueueItem> ItemsQueue
        {
            get
            {
                return _itemsQueue;
            }

            set
            {
                if (_itemsQueue == value)
                {
                    return;
                }

                _itemsQueue = value;
                RaisePropertyChanged(ItemsQueuePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ItemsRunning" /> property's name.
        /// </summary>
        public const string ItemsRunningPropertyName = "ItemsRunning";

        private int _itemsRunning;

        /// <summary>
        /// Sets and gets the ItemsRunning property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public int ItemsRunning
        {
            get
            {
                return _itemsRunning;
            }

            set
            {
                if (_itemsRunning == value)
                {
                    return;
                }

                _itemsRunning = value;
                RaisePropertyChanged(ItemsRunningPropertyName);
            }
        }
    }
}
