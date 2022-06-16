using Autofac;
using Autofac.Extensions.DependencyInjection;
using CAPDemo.Application;
using CAPDemo.Application.IntegrationEvents;
using CAPDemo.Application.IntegrationEvents.Events;
using CAPDemo.Extensions;
using CAPDemo.infrastructure.AutofacModules;
using CAPDemo.Infrastructure;
using EventBus.EventBus;
using EventBus.EventBus.Abstractions;
using EventBus.EventBusRabbitMQ;
using EventBus.IntegrationEventLogEF;
using EventBus.IntegrationEventLogEF.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Data.Common;
using zipkin4net;
using zipkin4net.Middleware;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;
 using Serilog.Context;
 using Serilog;
 using Microsoft.Extensions.Logging;

IConfiguration Configuration = GetConfiguration();
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddCustomMVC(Configuration)
    .AddCustomDbContext(Configuration)
    .AddCustomIntegrations(Configuration)
    .AddEventBus(Configuration)
    .AddCap(Configuration);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerbuilder =>
{
    //containerbuilder.Populate(builder.Services);
    containerbuilder.RegisterModule(new MediatorModule());
    containerbuilder.RegisterModule(new ApplicationModule(Configuration["ConnectionString"]));;
});
builder.Host.ConfigureLogging((host, builder) => builder.UseSerilog(host.Configuration).AddSerilog());


var app = builder.Build();
ConfigureEventBus(app);
app.MigrateDbContext<OrderingContext>((context, services) => 
{
    var env = services.GetService<IWebHostEnvironment>();
    var settings = services.GetService<IOptions<OrderingSettings>>();
    var logger = services.GetService<ILogger<OrderingContextSeed>>();

    new OrderingContextSeed()
        .SeedAsync(context, env, settings, logger)
        .Wait();
}).MigrateDbContext<IntegrationEventLogContext>((context, __) => { context.Database.EnsureCreated(); });
RegisterZipkinTrace(app, app.Services.GetRequiredService<ILoggerFactory>(),app.Lifetime);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{

    public static string Namespace = typeof(OrderingSettings).Namespace;
    //public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
    public static string AppName = "CAPDemo";
    static IConfiguration GetConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        var config = builder.Build();

        return config;
    }

    private static void ConfigureEventBus(IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<EventBus.EventBus.Abstractions.IEventBus>();

        eventBus.Subscribe<UserCheckoutAcceptedIntegrationEvent, IIntegrationEventHandler<UserCheckoutAcceptedIntegrationEvent>>();
    }

    public static void RegisterZipkinTrace(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostApplicationLifetime lifetime)
    {
        lifetime.ApplicationStarted.Register(() =>
        {
            TraceManager.SamplingRate = 1.0f;
            var logger = new TracingLogger(loggerFactory, "zipkin4net");
            var httpSender = new HttpZipkinSender("http://localhost:9411", "application/json");
            var tracer = new ZipkinTracer(httpSender, new JSONSpanSerializer(), new Statistics());
            var consoleTracer = new zipkin4net.Tracers.ConsoleTracer();

            TraceManager.RegisterTracer(tracer);
            TraceManager.RegisterTracer(consoleTracer);
            TraceManager.Start(logger);
        });

        lifetime.ApplicationStopped.Register(() => TraceManager.Stop());
        app.UseTracing("service_CAPDemo");
    }
}



static class CustomExtensionsMethods
{
    public static IServiceCollection AddCustomMVC(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        return services;
    }

    public static IServiceCollection AddCustomDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>();
        // Add services to the container.



        services.AddDbContext<OrderingContext>(options =>
        {
            options.UseSqlServer(configuration["ConnectionString"],
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                });
        },
                    ServiceLifetime.Scoped  //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request)
                );

        services.AddDbContext<IntegrationEventLogContext>(options =>
        {
            options.UseSqlServer(configuration["ConnectionString"],
                                    sqlServerOptionsAction: sqlOptions =>
                                    {
                                        sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                                        //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                    });
        });

        return services;
    }


    public static IServiceCollection AddCustomIntegrations(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<Func<DbConnection, IIntegrationEventLogService>>(
            sp => (DbConnection c) => new IntegrationEventLogService(c));

        services.AddTransient<IOrderingIntegrationEventService, OrderingIntegrationEventService>();

        services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();


            var factory = new ConnectionFactory()
            {
                HostName = configuration["EventBusConnection"],
                DispatchConsumersAsync = true
            };

            if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
            {
                factory.UserName = configuration["EventBusUserName"];
            }

            if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
            {
                factory.Password = configuration["EventBusPassword"];
            }

            var retryCount = 5;
            if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
            {
                retryCount = int.Parse(configuration["EventBusRetryCount"]);
            }

            return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
        });

        return services;

    }

    public static IServiceCollection AddCustomConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<OrderingSettings>(configuration);
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details."
                };

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json", "application/problem+xml" }
                };
            };
        });

        return services;
    }

    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var subscriptionClientName = configuration["SubscriptionClientName"];
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = 5;
                if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(configuration["EventBusRetryCount"]);
                }

                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });

        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        return services;
    }

    public static ILoggingBuilder UseSerilog(this ILoggingBuilder builder, IConfiguration configuration)
    {
        var seqServerUrl = configuration["Serilog:SeqServerUrl"];
        var logstashUrl = configuration["Serilog:LogstashgUrl"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithProperty("ApplicationContext", Program.AppName)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://localhost:5341" : seqServerUrl)
            .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        return builder;
    }

    public static IServiceCollection AddCap(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCap(x => {
            x.UseEntityFramework<AppDbContext>();
            x.UseRabbitMQ(x => {
                x.HostName = "188.128.0.244";
                x.Port = 5672;
                x.UserName = "guest";
                x.Password = "guest";
            });
            x.UseDashboard();
            x.FailedRetryCount = 5;
            x.FailedThresholdCallback = failed =>
            {
                var logger = failed.ServiceProvider.GetService<ILogger<Program>>();
                logger.LogError($@"A message of type {failed.MessageType} failed after executing {x.FailedRetryCount} several times, 
                        requiring manual troubleshooting. Message name: {failed.Message.GetName()}");
            };
        });
        return services;
    }

}