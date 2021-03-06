using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPDemo.Domain.AggregatesModel.OrderAggregate
{
    public interface IOrderRepository :IRepository<Order>
    {
        Order Add(Order order);

        void Update(Order order);

        Task<Order> GetAsync(int orderId);
    }
}
