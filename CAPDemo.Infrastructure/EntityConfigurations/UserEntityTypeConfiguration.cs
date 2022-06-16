using CAPDemo.Domain.AggregatesModel.TestAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAPDemo.Infrastructure.EntityConfigurations
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> userConfiguration)
        {
            userConfiguration.ToTable("users", OrderingContext.DEFAULT_SCHEMA);

            userConfiguration.HasKey(t => t.Id);

            userConfiguration.Ignore(b => b.DomainEvents);

            //userConfiguration.Property(o => o.Id).UseHiLo("userIdseq", OrderingContext.DEFAULT_SCHEMA);

            //userConfiguration.OwnsOne(o => o.Address, a => 
            //{
            //    a.Property<int>("UserId")
            //    .UseHiLo("userIdseq", OrderingContext.DEFAULT_SCHEMA);
            //    a.WithOwner();
            //});

            userConfiguration
                .Property<string>("Name")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("Name")
                .IsRequired();

            userConfiguration
                .Property<string>("IdentityGuid")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("IdentityGuid")
                .IsRequired();

            userConfiguration
                .Property<DateTime>("_CreateTime")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("CreateTime")
                .IsRequired();
        }
    }
}
