using Ascend.Net.Http;

namespace Ascend.Functions.Domain.Services.ApiClients.Adapter
{
    public interface IRestApiAdapterFactory
    {
        IRestAdapter Create<T>() where T : IRestApiAdapter;
    }
}