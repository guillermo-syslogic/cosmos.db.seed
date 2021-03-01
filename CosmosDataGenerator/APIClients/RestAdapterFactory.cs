using Ascend.Net.Http;
using RestSharp.Authenticators;

namespace Ascend.Services.ApiClients
{
    public class RestAdapterFactory : IRestAdapterFactory
    {
        public IRestAdapter Create(string baseUrl)
        {
            return new RestAdapter(baseUrl);
        }

        public IRestAdapter Create(string baseUrl, int timeout)
        {
            return new RestAdapter(baseUrl, timeout);
        }

        public IRestAdapter Create(string baseUrl, IAuthenticator authenticator)
        {
            return new RestAdapter(baseUrl, authenticator);
        }

        public IRestAdapter Create(string baseUrl, int timeout, IAuthenticator authenticator)
        {
            return new RestAdapter(baseUrl, timeout, authenticator);
        }
    }
}
