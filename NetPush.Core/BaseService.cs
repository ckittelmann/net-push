using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NetPush.Core
{
    public abstract class BaseService<TNotification, TConfiguration> where TNotification : BaseNotification where TConfiguration : BaseConfiguration
    {
		private readonly ConcurrentQueue<TNotification> _queue = new ConcurrentQueue<TNotification>();


	    private Timer _timer;

	    public TConfiguration Configuration { get; protected set; }

		public void QueueNotification(TNotification notification)
	    {
			_queue.Enqueue(notification);
		}

	    public TNotification GetNextQueueElement()
	    {
		    _queue.TryDequeue(out var notification);

		    return notification;
	    }

		public void StartService()
	    {
		    _timer = new Timer(HandleTimeTickInternal, _queue, 0, Timeout.Infinite);
	    }

		public void StopService()
	    {
		    _timer.Dispose();
		}

	    private void HandleTimeTickInternal(object state)
	    {
		    if (_queue.IsEmpty)
		    {
			    _timer.Change(_queue.IsEmpty ? 10000 : 0, Timeout.Infinite);
			    return;
		    }

		    var nextNotification = GetNextQueueElement();

			try
			{
				SendMessage(nextNotification);
			}
		    catch (Exception e)
		    {
			    // TODO: logging and maximum retry
				_queue.Enqueue(nextNotification);
		    }

		    _timer.Change(0, Timeout.Infinite);
		}

	    protected abstract void SendMessage(TNotification notification);
    }
}
