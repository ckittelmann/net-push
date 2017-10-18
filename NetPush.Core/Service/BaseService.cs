using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetPush.Core.Configuration;
using NetPush.Core.Notification;
using NetPush.Core.NotificationResult;

namespace NetPush.Core.Service
{
    public abstract class BaseService<TNotification, TConfiguration> where TNotification : BaseNotification where TConfiguration : BaseConfiguration
    {
        private readonly List<TNotification> _queue = new List<TNotification>();
        private readonly object _listLock = new object();
        
        public event Action<NotificationFailedResult<TNotification>> NotificationFailed;
        public event Action<NotificationSucceededResult<TNotification>> NotificationSucceeded;

        private Timer _timer;

        public TConfiguration Configuration { get; protected set; }

        public void QueueNotification(TNotification notification)
        {
            lock (_listLock)
            {
                _queue.Add(notification);
            }
        }

        public TNotification GetNextQueueElement()
        {
            if (_queue.Any())
            {
                lock (_listLock)
                {
                    var notification = _queue.FirstOrDefault(n => n.NextTry <= DateTime.UtcNow);
                    _queue.Remove(notification);
                    return notification;
                }
            }
            
            return null;
        }

        public void StartService()
        {
            _timer = new Timer(HandleTimeTickInternal, null, 0, Timeout.Infinite);
        }

        public void StopService()
        {
            _timer.Dispose();
        }

        private void HandleTimeTickInternal(object state)
        {
            var nextNotification = GetNextQueueElement();

            if (nextNotification == null)
            {
                _timer.Change(10000, Timeout.Infinite);
                return;
            }

            try
            {
                SendMessage(nextNotification);
            }
            catch (Exception ex)
            {
                FireNotificationFailed(nextNotification, "unexpected error", ex);
            }
            finally
            {
                _timer.Change(0, Timeout.Infinite);
            }
        }

        protected void FireNotificationSucceeded(TNotification notification, string message)
        {
            NotificationSucceeded?.Invoke(new NotificationSucceededResult<TNotification>(notification, message));
        }

        protected void FireNotificationFailed(TNotification notification, string message, Exception ex = null)
        {
            NotificationFailed?.Invoke(new NotificationFailedResult<TNotification>(notification, message, ex));

            notification.ErrorCount++;

            // try at least 3 times
            if (notification.ErrorCount < 3)
            {
                notification.NextTry = DateTime.UtcNow.AddMinutes(2);
                QueueNotification(notification);
            }
        }

        protected abstract void SendMessage(TNotification notification);
    }
}
