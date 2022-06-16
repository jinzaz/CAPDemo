using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPDemo.Domain.AggregatesModel.TestAggregate
{
    public class User : Entity, IAggregateRoot
    {
        public string Name { get; private set; }

        private DateTime _CreateTime;

        public string IdentityGuid { get; private set; }

        //public Address Address { get; private set; }


        protected User()
        {

        }
        public User(string name,string identityGuid, Address address)
        {
            Name = name;
            _CreateTime = DateTime.UtcNow;
            //Address = address;
            IdentityGuid = identityGuid;


            AddUserStartedDomainEvent();

        }

        private void AddUserStartedDomainEvent()
        {
            var UserStartedDomainEvent = new UserStartedDomainEvent(this);

            this.AddDomainEvent(UserStartedDomainEvent);
        }
    }
}
