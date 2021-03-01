using RestSharp;
using RestSharp.Authenticators;

namespace Ascend.Services.ApiClients
{
    public class GenericTokenAuthenticator : IAuthenticator
    {
        private readonly string _token;
        private readonly string _tokenPrefix;

        public GenericTokenAuthenticator(string token, string tokenPrefix = "Bearer")
        {
            _token = token;
            _tokenPrefix = tokenPrefix;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddParameter("Authorization", $"{_tokenPrefix} {_token}", ParameterType.HttpHeader);
        }
    }
}
