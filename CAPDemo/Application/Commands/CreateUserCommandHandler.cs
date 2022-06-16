using CAPDemo.Application.Commands;
using CAPDemo.Application.IntegrationEvents;
using CAPDemo.Application.IntegrationEvents.Events;
using CAPDemo.Domain.AggregatesModel.OrderAggregate;
using CAPDemo.Domain.AggregatesModel.TestAggregate;

namespace CAPDemo.Application.Commands
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, bool>
    {

        private readonly IUserRepository _userRepository;
        private readonly IMediator _mediator;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;
        private readonly ILogger<CreateUserCommandHandler> _logger;

        // Using DI to inject infrastructure persistence Repositories
        public CreateUserCommandHandler(IMediator mediator,
            IOrderingIntegrationEventService orderingIntegrationEventService,
            IUserRepository userRepository,
            ILogger<CreateUserCommandHandler> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
           
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderingIntegrationEventService = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<bool> Handle(CreateUserCommand message, CancellationToken cancellationToken)
        {
            var userStartedIntegrationEvent = new UserStartedIntegrationEvent(message.IdentityGuid);
            await _orderingIntegrationEventService.AddAndSaveEventAsync(userStartedIntegrationEvent);

            var address = new Address(message.Street, message.City, message.State, message.Country, message.ZipCode);
            var user = new User(message.Name,message.IdentityGuid, address);

            _logger.LogInformation("----- Creating User - User: {@User}", user);

            _userRepository.Add(user);

            return await _userRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }
}
