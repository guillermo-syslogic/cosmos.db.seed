using Ascend.Net.Http;

namespace Ascend.Functions.Domain.Services.ApiClients.Adapter
{
    public interface IRestApiAdapter
    {
        IRestAdapter CreateAdapter();
    }
}