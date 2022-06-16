using EventBus.EventBus.Events;

namespace CAPDemo.Application.IntegrationEvents.Events
{
    public record UserCheckoutAcceptedIntegrationEvent : IntegrationEvent
    {
        public string IdentityGuid { get; }

        public string UserName { get; }

        public string City { get; set; }

        public string Street { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string ZipCode { get; set; }

        public Guid RequestId { get; set; }

        public UserCheckoutAcceptedIntegrationEvent(string identityGuid, string userName, string city, string street,
        string state, string country, string zipCode, Guid requestId)
        {
            IdentityGuid = identityGuid;
            UserName = userName;
            City = city;
            Street = street;
            State = state;
            Country = country;
            ZipCode = zipCode;
            RequestId = requestId;
        }
    }
}
