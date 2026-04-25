using events.domain.DomainConfig;
using events.domain.Entites;
using events.domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace events.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<VenueImage> VenueImages { get; set; }
        public DbSet<VenueAvailability> VenueAvailabilities { get; set; }
        public DbSet<VenueEventType> VenueEventTypes { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<OwnerRequest> OwnerRequests { get; set; }
        public DbSet<EditRequest> EditRequests { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<VenueServiceOption> VenueServiceOptions { get; set; }
        public DbSet<BookingSelectedService> BookingSelectedServices { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new UserConfig());
            builder.ApplyConfiguration(new UserRoleConfig());
            builder.ApplyConfiguration(new CompanyConfig());
          
            builder.ApplyConfiguration(new VenueConfig());
            builder.ApplyConfiguration(new VenueImageConfig());
            builder.ApplyConfiguration(new VenueAvailabilityConfig());
            builder.ApplyConfiguration(new VenueEventTypeConfig());
            builder.ApplyConfiguration(new EventTypeConfig());
            builder.ApplyConfiguration(new BookingConfig());
            builder.ApplyConfiguration(new PaymentConfig());
            builder.ApplyConfiguration(new ReviewConfig());
            builder.ApplyConfiguration(new EditRequestConfig());
            builder.ApplyConfiguration(new ServiceConfig());
            builder.ApplyConfiguration(new VenueServiceOptionConfig());
            builder.ApplyConfiguration(new BookingSelectedServiceConfig());


            builder.Entity<UserRole>().HasData(
                new UserRole { Id = 1, Name = "Admin", Permation = new string[] { "all" }, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new UserRole { Id = 2, Name = "User", Permation = new string[] { "read" }, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new UserRole { Id = 3, Name = "Owner", Permation = new string[] { "manage_venue" }, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            builder.Entity<EventType>().HasData(
                new EventType { Id = 1, Name = "Party", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new EventType { Id = 2, Name = "Engagement", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new EventType { Id = 3, Name = "Wedding", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            builder.Entity<Service>().HasData(
                new { Id = 1, Name = "Catering", Description = "Food and beverage service", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 2, Name = "Decoration", Description = "Decor and venue styling", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 3, Name = "Photography", Description = "Photo coverage package", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new { Id = 4, Name = "DJ", Description = "Music and sound setup", CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
