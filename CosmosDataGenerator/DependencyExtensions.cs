using System;
using Ascend.Configuration;
using Ascend.Net.Http;
using Ascend.Services.ApiClients;
using Microsoft.Extensions.DependencyInjection;

namespace CosmosDataGenerator
{
    public class DependencyExtensions : IDisposable
    {
        private static ServiceProvider _serviceProvider;

        public DependencyExtensions()
        {
            RegisterServices();
        }

        private static void RegisterServices()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IConfigurationProvider, ConfigurationProvider>();
            services.AddSingleton<IRestAdapterFactory, RestAdapterFactory>();
            _serviceProvider = services.BuildServiceProvider(true);
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }

        public void Dispose()
        {
            DisposeServices();
        }
    }
}
