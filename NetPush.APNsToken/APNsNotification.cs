using System;
using System.Collections.Generic;
using NetPush.Core;

namespace NetPush.APNsToken
{
    public class APNsNotification : BaseNotification
    {
	    public string DeviceToken { get; set; }

	    public string BundleId { get; set; }

		public string CollapseId { get; set; }

	    public int Priority { get; set; } = 10;

	    public Dictionary<string, string> Alert { get; set; } = new Dictionary<string, string>();

	    public int Badge { get; set; } = 0;

	    public string Sound { get; set; } = "default";

	    public APNsNotification(string deviceToken, string bundleId, string body, string title = "")
	    {
		    if (string.IsNullOrWhiteSpace(body))
		    {
			    throw new ArgumentException("Body must be set!");
			}

		    Alert.Add("body", body);

			if (!string.IsNullOrWhiteSpace(title))
		    {
			    Alert.Add("title", title);
		    }

			DeviceToken = deviceToken;
		    BundleId = bundleId;
	    }
    }
}
