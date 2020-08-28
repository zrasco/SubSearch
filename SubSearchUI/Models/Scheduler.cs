using GalaSoft.MvvmLight;
using Microsoft.Extensions.Logging;
using SubSearchUI.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SubSearchUI.Models
{
    public class Scheduler : ObservableObject
    {
        public event Action<QueueItem> QueueItemStatusChange;
        private AppSettings _appSettings;
        private readonly ILogger<Scheduler> _logger;

        public Scheduler(IWritableOptions<AppSettings> settings,
                            ILogger<Scheduler> logger)
        {
            ItemsQueue = new ObservableCollection<QueueItem>();

            _appSettings = settings.Value;
            _logger = logger;
        }

        public void SetQueueItemStatusChangeEventHandler(Action<QueueItem> onQueueItemStatusChangeEventHandler)
        {
            QueueItemStatusChange += onQueueItemStatusChangeEventHandler;
        }

        public void RemoveQueueItemStatusChangeEventHandler(Action<QueueItem> onQueueItemStatusChangeEventHandler)
        {
            QueueItemStatusChange -= onQueueItemStatusChangeEventHandler;
        }

        public QueueItem AddItem(string text, Func<QueueItem, CancellationToken, bool> actionWithResult, Action<QueueItem> actionWhenDone = null)
        {
            // Can't set default parameter to this in the function signature because it's not a compile-time constant
            QueueItem queueItem = new QueueItem() { TaskText = text, Status = QueueStatus.InQueue, Work = actionWithResult, DoWhenDone = actionWhenDone };

            ItemsQueue.Add(queueItem);

            Cycle();

            return queueItem;
        }

        private void StartItem(QueueItem queueItem)
        {
            ItemsRunning++;

            //Task<bool> worker = queueItem.DoWorkAsync();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var taskToRun = new Task<bool>(() =>
            {
                // Get the current thread in case we need to cancel the task
                try
                {
                    return queueItem.Work(queueItem, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    queueItem.Status = QueueStatus.Aborted;
                    return false;
                }

            }, cancellationTokenSource.Token);

            // Use TaskScheduler.FromCurrentSynchronizationContext() to force the continuation to run on the UI thread instead of a worker thread
            taskToRun.ContinueWith(x => {
                ItemDone(x, queueItem);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            if (queueItem.DoWhenDone != null)
                taskToRun.ContinueWith((ignoreme) => queueItem.DoWhenDone(queueItem), TaskScheduler.Default);

            // Schedule the task to run. Force it to run in the UI thread
            taskToRun.Start(TaskScheduler.Default);
        }

        public void ItemDone(Task<bool> task, QueueItem queueItem)
        {
            ItemsRunning--;

            if (queueItem.Status != QueueStatus.Aborted)
            {
                if (task.Result == false)
                    queueItem.Status = QueueStatus.Failed;
                else
                    queueItem.Status = QueueStatus.Complete;

                // Fire event when item done
                QueueItemStatusChange.Invoke(queueItem);
            }

            // Execute a cycle
            Cycle();
        }

        public void Cycle()
        {
            if (ItemsRunning < _appSettings.MaxBackgroundJobs)
            {
                var inQueueJobs = ItemsQueue.Where(x => x.Status == QueueStatus.InQueue);

                while (ItemsQueue.Where(x => x.Status == QueueStatus.InQueue).Count() > 0 && ItemsRunning < _appSettings.MaxBackgroundJobs)
                {
                    var nextJob = ItemsQueue.Where(x => x.Status == QueueStatus.InQueue).FirstOrDefault();

                    if (nextJob != null)
                    {
                        nextJob.Status = QueueStatus.InProgress;
                        QueueItemStatusChange.Invoke(nextJob);
                        StartItem(nextJob);
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
