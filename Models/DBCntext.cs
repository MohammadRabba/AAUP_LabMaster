using AAUP_LabMaster.Models;
using Microsoft.EntityFrameworkCore;

namespace AAUP_LabMaster.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<Supervisour> Supervisours { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Lab> Labs { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Client)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Equipment>()
            .Ignore(e => e.ImageFile);
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Equipment)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
