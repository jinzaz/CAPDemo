using CAPDemo.Application.IntegrationEvents.Events;
using EventBus.EventBus.Abstractions;
using EventBus.EventBus.Extensions;
using Serilog.Context;

namespace CAPDemo.Application.IntegrationEvents.EventHandling
{
    public class UserCheckoutAcceptedIntegrationEventHandler : IIntegrationEventHandler<UserCheckoutAcceptedIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly ServiceFactory _serviceFactory;
        private readonly ILogger<UserCheckoutAcceptedIntegrationEventHandler> _logger;

        public UserCheckoutAcceptedIntegrationEventHandler(
            IMediator mediator,
            ILogger<UserCheckoutAcceptedIntegrationEventHandler> logger,
            ServiceFactory serviceFactory)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceFactory = serviceFactory;
        }
        public async Task Handle(UserCheckoutAcceptedIntegrationEvent @event)
        {
            using (LogContext.PushProperty("IntegrationEventContext", $"{@event.Id}-{Program.AppName}"))
            {
                _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);

                var result = false;

                if (@event.RequestId != Guid.Empty)
                {
                    using (LogContext.PushProperty("IdentifiedCommandId", @event.RequestId))
                    {
                        var createUserCommand = new CreateUserCommand(@event.IdentityGuid, @event.UserName, @event.City, @event.Street, @event.State, @event.Country, @event.ZipCode);

                        var requestCreateUser = new IdentifiedCommand<IRequest<bool>, bool>(createUserCommand, @event.RequestId);

                        _logger.LogInformation(
                            "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                            requestCreateUser.GetGenericTypeName(),
                            nameof(requestCreateUser.Id),
                            requestCreateUser.Id,
                            requestCreateUser);

                        result = await _mediator.Send(requestCreateUser);
                        
                        

                        if (result)
                        {
                            _logger.LogInformation("----- CreateOrderCommand suceeded - RequestId: {RequestId}", @event.RequestId);
                        }
                        else
                        {
                            _logger.LogWarning("CreateOrderCommand failed - RequestId: {RequestId}", @event.RequestId);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Invalid IntegrationEvent - RequestId is missing - {@IntegrationEvent}", @event);
                }
            }
        }
    }
}
