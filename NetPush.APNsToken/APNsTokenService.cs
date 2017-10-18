using System;
using System.Collections.Generic;
using System.Net.Http;
using HttpTwo;
using Jose;
using NetPush.Core.Service;
using Newtonsoft.Json.Linq;

namespace NetPush.APNsToken
{
    public class APNsTokenService : BaseService<APNsNotification, APNsTokenConfiguration>
    {
        public APNsTokenService(APNsTokenConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override async void SendMessage(APNsNotification notification)
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
                // TODO: we shouldn't create new httpClient every run
                // but at the moment there is no stable http2 implementation and this one works only for some requests until ip of apple service changes
                using (var httpClient = new HttpClient(new Http2MessageHandler()))
                {
                    var responseMessage = httpClient.SendAsync(requestMessage);
                    // TODO: async can't be used, think its a problem of HttpTwo
                    if (responseMessage.Result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        FireNotificationSucceeded(notification, "Message successfully sent");
                    }
                    else
                    {
                        var body = await responseMessage.Result.Content.ReadAsStringAsync();
                        var jObject = JObject.Parse(body);
                        var reason = jObject.Value<string>("reason");

                        FireNotificationFailed(notification, $"Failed to send message: {reason}");
                    }
                }
            }
            catch (Exception ex)
            {
                FireNotificationFailed(notification, "unexpected exception", ex);
            }
        }
    }
}
