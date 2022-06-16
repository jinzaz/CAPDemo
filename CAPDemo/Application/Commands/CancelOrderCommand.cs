using System.Runtime.Serialization;

namespace CAPDemo.Application.Commands;

public class CancelOrderCommand : IRequest<bool>
{

    [DataMember]
    public int OrderNumber { get; set; }
    public CancelOrderCommand()
    {

    }
    public CancelOrderCommand(int orderNumber)
    {
        OrderNumber = orderNumber;
    }
}
