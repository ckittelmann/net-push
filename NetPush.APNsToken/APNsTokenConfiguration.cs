using System;
using System.Security.Cryptography;
using NetPush.Core;

namespace NetPush.APNsToken
{
    public class APNsTokenConfiguration : BaseConfiguration
    {
        public enum Environment
        {
            Production,

            Development
        }

        private const string DevelopmentUrl = "api.development.push.apple.com";
        private const string ProductionUrl = "api.push.apple.com";

        public string Host { get; }

        public CngKey Key { get; }

        public string KeyId { get; }

        public string TeamId { get; }

        public int Port { get; }

        public APNsTokenConfiguration(Environment environment, string privateKey, string privateKeyId, string teamId, int port = 443)
        {
            Host = environment == Environment.Production ? ProductionUrl : DevelopmentUrl;
            KeyId = privateKeyId;
            TeamId = teamId;
            Port = port;

            var tokenBytes = Convert.FromBase64String(privateKey);
            Key = CngKey.Import(tokenBytes, CngKeyBlobFormat.Pkcs8PrivateBlob);
        }
    }
}
