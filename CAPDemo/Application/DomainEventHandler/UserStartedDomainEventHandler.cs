using CAPDemo.Application.IntegrationEvents;
using CAPDemo.Application.IntegrationEvents.Events;
using CAPDemo.Domain.AggregatesModel.BuyerAggregate;
using CAPDemo.Domain.AggregatesModel.TestAggregate;
using CAPDemo.Domain.Events;

namespace CAPDemo.Application.DomainEventHandler
{
    public class UserStartedDomainEventHandler : INotificationHandler<UserStartedDomainEvent>
    {
        private readonly ILoggerFactory _logger;
        private readonly IBuyerRepository _buyerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;

        public UserStartedDomainEventHandler(
            ILoggerFactory logger,
            IBuyerRepository buyerRepository,
            IUserRepository userRepository,
            IOrderingIntegrationEventService orderingIntegrationEventService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
        }

        public async Task Handle(UserStartedDomainEvent userstartedEvent, CancellationToken cancellationToken)
        {
            var buyer = await _buyerRepository.FindAsync(userstartedEvent.User.IdentityGuid);
            bool buyerOriginallyExisted = (buyer == null) ? false : true;

            if (!buyerOriginallyExisted)
            {
                buyer = new Buyer(userstartedEvent.User.IdentityGuid, userstartedEvent.User.Name);
            }

            var buyerUpdated = buyerOriginallyExisted ?
                _buyerRepository.Update(buyer) :
                _buyerRepository.Add(buyer);

            await _buyerRepository.UnitOfWork
                 .SaveEntitiesAsync(cancellationToken);

            var userStatusChangedTosubmittedIntegrationEvent = new UserStatusChangedTosubmittedIntegrationEvent(userstartedEvent.User.Id,userstartedEvent.User.IdentityGuid,userstartedEvent.User.Name);

            await _orderingIntegrationEventService.AddAndSaveEventAsync(userStatusChangedTosubmittedIntegrationEvent);
            _logger.CreateLogger<UserStartedDomainEventHandler>()
                .LogTrace("Buyer {BuyerId} and related payment method were validated or updated for UserId: {UserId}.",
                    buyerUpdated.Id, userstartedEvent.User.Id);

        }
    }
}
