using EventBus.EventBus.Events;

namespace CAPDemo.Application.IntegrationEvents.Events
{
    public record UserStartedIntegrationEvent : IntegrationEvent
    {
        public string IdentityGuid { get; init; }

        public UserStartedIntegrationEvent(string identityGuid) => IdentityGuid = identityGuid;
    }
}
