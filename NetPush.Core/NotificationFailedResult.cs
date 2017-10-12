using System;

namespace NetPush.Core
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
