using EventBus.EventBus.Events;

namespace CAPDemo.Application.IntegrationEvents.Events
{
    public record UserStatusChangedTosubmittedIntegrationEvent : IntegrationEvent
    {
        public int UserId { get; set; }
        public string IdentityGuid { get; set; }
        public string UserName { get; set; }

        public UserStatusChangedTosubmittedIntegrationEvent(int userId, string identityGuid,string userName)
        {
            UserId = userId;
            UserName = userName;
            IdentityGuid = identityGuid;
        }
    }
}
