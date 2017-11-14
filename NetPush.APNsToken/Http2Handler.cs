using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetPush.APNsToken
{
    public class Http2Handler : WinHttpHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Version = new Version("2.0");
            return base.SendAsync(request, cancellationToken);
        }
    }
}
