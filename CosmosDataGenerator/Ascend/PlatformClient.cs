using System;
using System.Threading.Tasks;
using Ascend.Configuration;
using Ascend.IO;
using Ascend.Net.Http;
using Ascend.Storage.Caching;

namespace CosmosDataGenerator.Ascend
{
    public class PlatformClient
    {
        private const string AscendPlatformUrlKey = "AscendPlatformApiUrl";
        private const string BaseAddressUrlKey = "BaseAddressUrl";
        private const string AuthenticationTokenCacheName = "PlatformToken";

        private IRestAdapter _restAdapter;
        private readonly IRestAdapterFactory _restAdapterFactory;
        private readonly IJsonService _jsonService;
        private readonly IConfigurationProvider _configurationProvider;

        public PlatformClient(IConfigurationProvider configurationProvider, IRestAdapterFactory restAdapterFactory, IJsonService jsonService)
        {
            _configurationProvider = configurationProvider;
            _platformUrl = new Lazy<string>(() => _configurationProvider.GetValue(AscendPlatformUrlKey));
            _baseAddress = new Lazy<string>(() => _configurationProvider.GetValue(BaseAddressUrlKey));
            _restAdapterFactory = restAdapterFactory;
            _jsonService = jsonService;
        }

        private readonly ICacheProvider _cacheProvider;
        private readonly Lazy<string> _platformUrl;
        private readonly Lazy<string> _baseAddress;

        public PlatformClient()
        {
        }

        private async Task<IRestAdapter> GetRestAdapter()
        {
            if (_restAdapter == null)
            {
                var token = await GetAuthenticationToken().ConfigureAwait(false);
                _restAdapter = _restAdapterFactory.Create(_platformUrl.Value, new GenericTokenAuthenticator(token));
            }

            return _restAdapter;
        }

        private string GetDealerApiRoot()
        {
            return "dealers/";
        }

        private async Task<string> GetAuthenticationToken()
        {
            //var cacheData = await _cacheProvider.GetAsync<string>(AuthenticationTokenCacheName, true)
            //    .ConfigureAwait(false);
            //if (cacheData != default)
            //{
            //    return cacheData;
            //}

            var token = await RequestAuthenticationToken().ConfigureAwait(false);
            var cacheData = token.access_token;
            if (token == default)
            {
                return cacheData;
            }

            //cacheData = token.access_token;
            //await _cacheProvider.PutAsync(
            //    AuthenticationTokenCacheName,
            //    cacheData,
            //    true,
            //    TimeSpan.FromSeconds(token.expires_in - 60))
            //    .ConfigureAwait(false); // Remove a minute to expire from the cache before the token expires

            return cacheData;
        }

        private async Task<AccessToken> RequestAuthenticationToken()
        {
            var baseAddress = _baseAddress.Value;
            var restAdapter = _restAdapterFactory.Create(baseAddress);
            var result = await restAdapter.PostAsync<AccessToken>(string.Empty, null).ConfigureAwait(false);

            return _jsonService.Deserialize<AccessToken>(result.Content);
        }
    }
}
