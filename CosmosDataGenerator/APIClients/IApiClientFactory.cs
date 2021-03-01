using System;

namespace Ascend.Services.ApiClients
{
    public interface IApiClientFactory
    {
        T Create<T>() where T : IApiClient;
        T Create<T>(Guid dealerId) where T : IStratusDealerClient;
        T Create<T>(Guid dealerId, Guid locationId) where T : IStratusLocationClient;
    }
}