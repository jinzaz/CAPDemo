using CAPDemo.Domain.AggregatesModel.TestAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPDemo.Domain.Events
{
    public class UserStartedDomainEvent :INotification
    {
        public User User { get; }

        public UserStartedDomainEvent(User user)
        {
            User = user;
        }
    }
}
