namespace CAPDemo.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValuesController : ControllerBase
{
    private readonly ICapPublisher _capBus;
    private readonly IMediator _mediator;

    public ValuesController(ICapPublisher capPublisher,IMediator mediator)
    {
        _capBus = capPublisher;
        _mediator = mediator;
    }

    [Route("transaction")]
    [HttpGet]
    public async Task<IActionResult> WithoutTransaction()
    {
        await _capBus.PublishAsync("sample.rabbitmq.mysql", DateTime.Now);

        return Ok();
    }

    [Route("adonet/transaction")]
    [HttpGet]
    public IActionResult AdonetWithTransaction()
    {
        using (var connection = new MySqlConnection(AppDbContext.ConnectionString))
        {
            using (var transaction = connection.BeginTransaction(_capBus, true))
            {
                //your business code
                connection.Execute("insert into test(name) values('test')", transaction: (IDbTransaction)transaction.DbTransaction);

                //for (int i = 0; i < 5; i++)
                //{
                _capBus.Publish("sample.rabbitmq.mysql", DateTime.Now);
                //}
            }
        }

        return Ok();
    }

    [Route("ef/transaction")]
    [HttpGet]
    public IActionResult EntityFrameworkWithTransaction([FromServices] AppDbContext dbContext)
    {
        using (var trans = dbContext.Database.BeginTransaction(_capBus, autoCommit: false))
        {
            dbContext.Persons.Add(new Person() { Name = "ef.transaction" });

            for (int i = 0; i < 1; i++)
            {
                _capBus.Publish("sample.rabbitmq.mysql", DateTime.Now);
            }

            dbContext.SaveChanges();

            trans.Commit();
        }
        return Ok();
    }


    [NonAction]
    [CapSubscribe("sample.rabbitmq.mysql")]
    public void Subscriber(DateTime p)
    {
        Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {p}");
    }

    [NonAction]
    [CapSubscribe("sample.rabbitmq.mysql", Group = "group.test2")]
    public void Subscriber2(DateTime p, [FromCap] CapHeader header)
    {
        Console.WriteLine($@"{DateTime.Now} Subscriber invoked, Info: {p}");
    }
}

