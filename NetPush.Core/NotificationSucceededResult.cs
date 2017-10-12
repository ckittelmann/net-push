namespace NetPush.Core
{
    public class NotificationSucceededResult<TNotification> where TNotification : BaseNotification
    {
        public TNotification Notification { get; set; }

        public string Message { get; set; }

        public NotificationSucceededResult(TNotification notification, string message)
        {
            Notification = notification;
            Message = message;
        }
    }
}
