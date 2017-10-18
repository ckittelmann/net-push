using System;

namespace NetPush.Core.Notification
{
    public class BaseNotification
    {
        internal int ErrorCount { get; set; }

        internal DateTime NextTry { get; set; } = DateTime.UtcNow;
    }
}
