using System;

namespace NetPush.Core
{
    public class BaseNotification
    {
        internal int ErrorCount { get; set; }

        internal DateTime NextTry { get; set; } = DateTime.UtcNow;
    }
}
