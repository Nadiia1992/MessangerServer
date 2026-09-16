using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MessangerServer
{
    public class MessangerContext : DbContext
    {
        static DbContextOptions<MessangerContext> _options;

        static MessangerContext()
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory());
            builder.AddJsonFile("appsettings.json");
            var config = builder.Build();
            string? connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<MessangerContext>();
            _options = optionsBuilder.UseSqlServer(connectionString).Options;

        }
        public MessangerContext() : base(_options)
        {
            Database.EnsureCreated();
        }


        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
      
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}

