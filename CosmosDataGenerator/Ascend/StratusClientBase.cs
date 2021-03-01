using System;
using Ascend.Functions.Domain.Services.ApiClients.Adapter;
using Ascend.Net.Http;

namespace Ascend.Services.ApiClients
{
    // Note: In order to provide quick typing (preventing developers from even creating the wrong client with the wrong method)
    // We'll want to keep these abstract classes and interfaces separate!
    public abstract class ClientBase<T> where T : IRestApiAdapter
    {
        protected readonly IRestAdapter RestAdapter;

        protected abstract string ApiRoot { get; }

        protected ClientBase(IRestApiAdapterFactory restApiAdapterFactory)
        {
            RestAdapter = restApiAdapterFactory.Create<T>();
        }

        protected string ActionUrl(string action)
        {
            return $"{ApiRoot}/{action}";
        }
    }

    public interface IApiClient { }

    public abstract class StratusClientBase<T> : ClientBase<T>, IApiClient where T : IRestApiAdapter
    {
        protected StratusClientBase(IRestApiAdapterFactory restApiAdapterFactory)
            : base(restApiAdapterFactory) { }

    }

    public interface IStratusDealerClient { }

    public abstract class StratusDealerClientBase<T>: ClientBase<T>, IStratusDealerClient where T : IRestApiAdapter
    {
        protected readonly Guid DealerId;

        protected StratusDealerClientBase(Guid dealerId, IRestApiAdapterFactory restApiAdapterFactory)
            : base(restApiAdapterFactory)
        {
            DealerId = dealerId;
        }
    }

    public interface IStratusLocationClient { }

    public struct LocationInformation
    {
        public Guid DealerId { get; }
        public Guid LocationId { get; }

        public LocationInformation(Guid dealerId, Guid locationId)
        {
            DealerId = dealerId;
            LocationId = locationId;
        }
    }

    public abstract class StratusLocationClientBase<T> : ClientBase<T>, IStratusLocationClient where T : IRestApiAdapter
    {
        protected readonly Guid DealerId;
        protected readonly Guid LocationId;

        protected StratusLocationClientBase(LocationInformation locationInfo, IRestApiAdapterFactory restApiAdapterFactory)
            : base(restApiAdapterFactory)
        {
            DealerId = locationInfo.DealerId;
            LocationId = locationInfo.LocationId;
        }
    }

}
