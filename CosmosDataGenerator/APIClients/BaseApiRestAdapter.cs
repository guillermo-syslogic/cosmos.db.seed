using Ascend.Configuration;
using Ascend.Net.Http;
using RestSharp.Authenticators;

namespace Ascend.Services.ApiClients
{
    public abstract class BaseApiRestAdapter
    {
        private readonly IRestAdapterFactory _restAdapterFactory;
        private readonly IConfigurationProvider _configurationProvider;

        public BaseApiRestAdapter(IConfigurationProvider configurationProvider,
            IRestAdapterFactory restAdapterFactory)
        {
            _restAdapterFactory = restAdapterFactory;
            _configurationProvider = configurationProvider;
        }

        public IRestAdapter Create(string endpointKey, IAuthenticator authenticator)
        {
            var baseUrl = _configurationProvider.GetValue(endpointKey);
            return _restAdapterFactory.Create(baseUrl, authenticator);
        }
    }
}
