using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HttpTwo;
using Jose;
using NetPush.Core;
using Newtonsoft.Json.Linq;

namespace NetPush.APNsToken
{
	public class APNsTokenService : BaseService<APNsNotification, APNsTokenConfiguration>
	{
		private readonly HttpClient _httpClient = new HttpClient(new Http2MessageHandler());

		public APNsTokenService(APNsTokenConfiguration configuration)
		{
			Configuration = configuration;
		}

		protected override void SendMessage(APNsNotification notification)
		{
			var url = $"https://{Configuration.Host}:{Configuration.Port}/3/device/{notification.DeviceToken}";
			var uri = new Uri(url);
			
			var expiration = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var expirationSeconds = (long)expiration.TotalSeconds;

			var payload = new Dictionary<string, object>
				{
					{ "iss", Configuration.TeamId },
					{ "iat", expirationSeconds }
				};

			var extraHeaders = new Dictionary<string, object>
				{
					{ "alg", JwsAlgorithm.ES256.ToString() },
					{ "kid", Configuration.KeyId }
				};

			string accessToken = JWT.Encode(payload, Configuration.Key, JwsAlgorithm.ES256, extraHeaders);

			var notificationPayload = JObject.FromObject(new
			{
				aps = new
				{
					alert = notification.Alert,
					badge = notification.Badge,
					sound = notification.Sound
				}
			});

			byte[] data = System.Text.Encoding.UTF8.GetBytes(notificationPayload.ToString());

			var requestMessage = new HttpRequestMessage();
			requestMessage.RequestUri = uri;
			requestMessage.Headers.Add("authorization", $"bearer {accessToken}");
			requestMessage.Headers.Add("apns-id", Guid.NewGuid().ToString());
			requestMessage.Headers.Add("apns-expiration", "0");
			requestMessage.Headers.Add("apns-priority", "10");
			requestMessage.Headers.Add("apns-topic", notification.BundleId);
			requestMessage.Method = HttpMethod.Post;
			requestMessage.Content = new ByteArrayContent(data);

			try
			{
				// Send podcast
				var responseMessage = _httpClient.SendAsync(requestMessage);

				// Get the result of the display
				if (responseMessage.Result.StatusCode == System.Net.HttpStatusCode.OK)
				{
					string responseUuid = string.Empty;
					IEnumerable<string> values;

					if (responseMessage.Result.Headers.TryGetValues("apns-id", out values))
					{
						responseUuid = values.First();
					}

					// Console.WriteLine(string.Format("\n\r*******Send Success [{0}]", responseUuid));
				}
				else
				{
					var body = responseMessage.Result.Content.ReadAsStringAsync();
					var json = new JObject();
					//json = JObject.Parse(body);

					//var reasonStr = json.Value<string>("reason");
					// Console.WriteLine("\n\r*******Failure reason => " + reasonStr);
				}
			}
			catch (Exception ex)
			{
				// Console.WriteLine("\n\r*******Exception message => " + ex.Message);
			}
		}
	}
}
