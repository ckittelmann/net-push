using System;
using NetPush.Core.Notification;

namespace NetPush.Core.NotificationResult
{
    public class NotificationFailedResult<T> : NotificationSucceededResult<T> where T: BaseNotification
    {
        public Exception Exception { get; set; }

        public NotificationFailedResult(T notification, string message, Exception ex) : base(notification, message)
        {
            Exception = ex;
        }
    }
}
