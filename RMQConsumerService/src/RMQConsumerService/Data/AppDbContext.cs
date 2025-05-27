using Microsoft.EntityFrameworkCore;
using RMQConsumerService.Models;

namespace RMQConsumerService.Data
{
    using Microsoft.EntityFrameworkCore;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<SendNotiMessage> NotiMessages { get; set; }
        public DbSet<SendSMSMessage> SMSMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // สร้าง index
            modelBuilder.Entity<SendNotiMessage>().HasIndex(m => m.UserId);
            modelBuilder.Entity<SendNotiMessage>().HasIndex(m => m.deviceId);

            // สร้าง index
            modelBuilder.Entity<SendSMSMessage>().HasIndex(m => m.UserId);
            modelBuilder.Entity<SendSMSMessage>().HasIndex(m => m.mobileNumber);
        }
    }
}