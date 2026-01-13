using property_lease_saas.Models.Entities;

namespace property_lease_saas.Tests.Builders
{
    public class LeaseRequestBuilder
    {
        private LeaseRequest _leaseRequest = new LeaseRequest();

        public LeaseRequestBuilder WithId(Guid id)
        {
            _leaseRequest.Id = id;
            return this;
        }

        public LeaseRequestBuilder WithProperty(Guid propertyId, string landlordId)
        {
            _leaseRequest.PropertyId = propertyId;
            _leaseRequest.LandlordId = landlordId;
            return this;
        }

        public LeaseRequestBuilder WithTenant(string tenantId)
        {
            _leaseRequest.TenantId = tenantId;
            return this;
        }

        public LeaseRequestBuilder WithStatus(LeaseRequestStatus status)
        {
            _leaseRequest.Status = status;
            return this;
        }

        public LeaseRequest Build()
        {
            return _leaseRequest;
        }

        public static LeaseRequestBuilder Create()
        {
            return new LeaseRequestBuilder();
        }
    }
}