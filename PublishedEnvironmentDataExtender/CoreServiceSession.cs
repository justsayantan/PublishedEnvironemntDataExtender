using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Tridion.ContentManager.CoreService.Client;

namespace PublishedEnvironmentDataExtender
{
    class CoreServiceSession: IDisposable
    {
        private readonly SessionAwareCoreServiceClient _client;

        public CoreServiceSession()
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
           // _client.Impersonate("SDLDEMO\\administrator");
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

        public UserData GetCurrentUser()
        {
            return _client.GetCurrentUser();
        }

        public PublishInfoData[] GetListPublishInfo(string id)
        {

            return _client.GetListPublishInfo(id);
        }


        public void SaveApplicationData(string subjectId, ApplicationData[] applicationData)
        {
            _client.SaveApplicationData(subjectId, applicationData);
        }

        public ApplicationData ReadApplicationData(string subjectId, string applicationId)
        {
            return _client.ReadApplicationData(subjectId, applicationId);
        }

        internal bool IsPublishedFromCurrentPublication(string id, string purpose)
        {
            return _client.IsPublished(id, purpose, true);
        }
        
    }
}