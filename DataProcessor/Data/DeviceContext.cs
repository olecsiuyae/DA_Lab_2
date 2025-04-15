using Microsoft.EntityFrameworkCore;
using SharedModels;

namespace DataProcessor.Data
{
    public class DeviceContext : DbContext
    {
        public DeviceContext(DbContextOptions<DeviceContext> options)
            : base(options)
        {
        }

        public DbSet<Device> Devices { get; set; }
        public DbSet<DeviceType> DeviceTypes { get; set; }
        public DbSet<DevicePort> DevicePorts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<DeviceType>()
                .HasKey(dt => dt.Id);

            modelBuilder.Entity<DevicePort>()
                .HasKey(dp => dp.Id);

            modelBuilder.Entity<DevicePort>()
                .HasOne(dp => dp.DeviceType)
                .WithMany(dt => dt.Ports)
                .HasForeignKey(dp => dp.DeviceTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
} 