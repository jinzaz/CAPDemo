using Autofac;
using CAPDemo.Application.DomainEventHandler;
using CAPDemo.Application.Queries;
using CAPDemo.Domain.AggregatesModel.BuyerAggregate;
using CAPDemo.Domain.AggregatesModel.OrderAggregate;
using CAPDemo.Domain.AggregatesModel.TestAggregate;
using CAPDemo.Infrastructure.Idempotency;
using CAPDemo.Infrastructure.Repositories;
using EventBus.EventBus.Abstractions;

namespace CAPDemo.infrastructure.AutofacModules
{
    public class ApplicationModule :Autofac.Module
    {
        public string QueriesConnectionString { get; }

        public ApplicationModule(string qconstr)
        {
            QueriesConnectionString = qconstr;
        }

        protected override void Load(ContainerBuilder builder)
        {

            builder.Register(c => new OrderQueries(QueriesConnectionString))
                .As<IOrderQueries>()
                .InstancePerLifetimeScope();
            builder.RegisterType<OrderRepository>()
                .As<IOrderRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<BuyerRepository>()
                .As<IBuyerRepository>()
                .InstancePerLifetimeScope();
            builder.RegisterType<RequestManager>()
                .As<IRequestManager>()
                .InstancePerLifetimeScope();
            builder.RegisterType<UserRepository>()
                .As<IUserRepository>()
                .InstancePerLifetimeScope();

            builder.RegisterAssemblyTypes(typeof(UserStartedDomainEventHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IIntegrationEventHandler<>));

            builder.RegisterAssemblyTypes(typeof(CreateUserCommandHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IIntegrationEventHandler<>));

            builder.RegisterAssemblyTypes(typeof(CreateOrderCommandHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IIntegrationEventHandler<>));

            builder.RegisterAssemblyTypes(typeof(IdentifiedCommandHandler<,>).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IIntegrationEventHandler<>));
        }
    }
}
