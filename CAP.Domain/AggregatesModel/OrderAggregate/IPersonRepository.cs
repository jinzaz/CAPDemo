using CAPDemo.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPDemo.Domain.AggregatesModel.OrderAggregate
{
    public interface IPersonRepository : IRepository<Person>
    {
        Person Add(Person buyer);
        Person Update(Person buyer);
        Task<Person> FindAsync(string BuyerIdentityGuid);
        Task<Person> FindByIdAsync(string id);
    }
}
