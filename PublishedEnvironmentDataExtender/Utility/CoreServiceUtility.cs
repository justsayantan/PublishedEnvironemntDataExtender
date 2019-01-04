using System;
using System.ServiceModel;
using System.Xml;
using Tridion.ContentManager.CoreService.Client;

namespace PublishedEnvironmentDataExtender.Utility
{
    public class CoreServiceUtility : IDisposable
    {
        private readonly SessionAwareCoreServiceClient _client;

        public CoreServiceUtility()
        {
            var netTcpBinding = new NetTcpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxStringContentLength = 2147483647,
                    MaxArrayLength = 2147483647
                }
            };

            var remoteAddress = new EndpointAddress("net.tcp://localhost:2660/CoreService/201603/netTcp");
            _client = new SessionAwareCoreServiceClient(netTcpBinding, remoteAddress);
        }

        public void Dispose()
        {
            if (_client.State == CommunicationState.Faulted)
            {
                _client.Abort();
            }
            else
            {
                _client.Close();
            }
        }

        internal PublishInfoData[] GetListPublishInfo(string id)
        {
            return _client.GetListPublishInfo(id);
        }

        internal bool IsPublished(string id, string targetPurpose)
        {
            return _client.IsPublished(id, targetPurpose, true);
        }
    }
}
