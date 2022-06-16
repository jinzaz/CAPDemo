using System.Runtime.Serialization;

namespace CAPDemo.Application.Commands
{
    [DataContract]
    public class CreateUserCommand :IRequest<bool>
    {
        [DataMember]
        public string IdentityGuid { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string City { get; private set; }
        [DataMember]
        public string Street { get; private set; }

        [DataMember]
        public string State { get; private set; }

        [DataMember]
        public string Country { get; private set; }

        [DataMember]
        public string ZipCode { get; private set; }

        public CreateUserCommand(string identityGuid, string userName, string city, string street, string state, string country, string zipcode)
        {
            IdentityGuid = identityGuid;
            Name = userName;
            City = city;
            Street = street;
            State = state;
            Country = country;
            ZipCode = zipcode;
        }
    }
}
