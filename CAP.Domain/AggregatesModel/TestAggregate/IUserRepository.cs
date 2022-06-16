using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPDemo.Domain.AggregatesModel.TestAggregate
{
    public interface IUserRepository :IRepository<User>
    {
        User Add(User user);
        void Update(User user);
        Task<User> FindByIdAsync(int id);
    }
}
