

namespace CAPDemo
{
    [Table("person")]
    public class Person
    {
        [Key] //主键 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"Name:{Name}, Id:{Id}";
        }
    }


    public class AppDbContext : DbContext
    {
        public const string ConnectionString = "server=188.128.0.244;database=capDemo;uid=root;pwd=root;SslMode=none;";

        public DbSet<Person> Persons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(ConnectionString, new MySqlServerVersion(ServerVersion.AutoDetect(ConnectionString)));
        }
    }
}
