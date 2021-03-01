using System;
using Microsoft.Extensions.DependencyInjection;

namespace Ascend.Services.ApiClients
{
    public class ApiClientFactory : IApiClientFactory
    {
        private readonly ServiceProvider _container;

        public ApiClientFactory(ServiceProvider container)
        {
            _container = container;
        }

        public T Create<T>() where T : IApiClient
        {
            return _container.GetService<T>();
        }

        public T Create<T>(Guid dealerId) where T : IStratusDealerClient
        {
            return _container.GetService<T>();
            //return _container.With(dealerId).GetInstance<T>();
        }

        public T Create<T>(Guid dealerId, Guid locationId) where T : IStratusLocationClient
        {
            var locationInfo = new LocationInformation(dealerId, locationId);
            return _container.GetService<T>();
            //return _container.With(locationInfo).GetInstance<T>();
        }
    }
}
