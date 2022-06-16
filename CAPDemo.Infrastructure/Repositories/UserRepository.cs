using CAPDemo.Domain.AggregatesModel.TestAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPDemo.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {

        private readonly OrderingContext _context;
        public IUnitOfWork UnitOfWork => _context;

        public UserRepository(OrderingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public User Add(User user)
        {
            return _context.Users.Add(user).Entity;
        }


        public async Task<User> FindByIdAsync(int id)
        {
            return await _context.Users.Include(x => x.Name).FirstOrDefaultAsync(o => o.Id == id);
        }

        public void Update(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}
