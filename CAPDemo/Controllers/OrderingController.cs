using CAPDemo.Application.IntegrationEvents.Events;
using CAPDemo.Models;
using EventBus.EventBus.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CAPDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderingController : ControllerBase
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger<OrderingController> _logger;
        public OrderingController(
            ILogger<OrderingController> logger,
            IEventBus eventBus)
        { 
            _eventBus = eventBus; 
            _logger = logger;
        }
        [HttpPost]
        [Route("CreateUser")]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> CreateUser([FromBody] UserCheckout basketCheckout, [FromHeader(Name = "x-requestid")] string requestId)
        {
            string userId = Guid.NewGuid().ToString("N");
            string userName = Convert.ToString(basketCheckout.userName);
            string City = Convert.ToString(basketCheckout.City);
            string Street = Convert.ToString(basketCheckout.Street);
            string State = Convert.ToString(basketCheckout.State);
            string Country = Convert.ToString(basketCheckout.Country);
            string ZipCode = Convert.ToString(basketCheckout.ZipCode);

            Guid AndrequestId =  (Guid.TryParse(requestId, out Guid guid) && guid != Guid.Empty) ?
            guid : Guid.NewGuid();

            var eventMessage = new UserCheckoutAcceptedIntegrationEvent(userId, userName, City, Street,
            State, Country, ZipCode, AndrequestId);

            try
            {
                _eventBus.Publish(eventMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName}", eventMessage.Id, Program.AppName);

                throw;
            }

            return Accepted();
        }
    }
}
