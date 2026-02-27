using BusinessObjects;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repositories
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RoomType> RoomTypes => Set<RoomType>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Wallet> Wallets => Set<Wallet>();

        public DbSet<RoomImage> RoomImages => Set<RoomImage>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>().ToTable("Users");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");

            // unique room number
            modelBuilder.Entity<Room>()
                .HasIndex(x => x.RoomNumber)
                .IsUnique();

            // N-N BookingRoom
            modelBuilder.Entity<BookingRoom>()
                .HasKey(x => new { x.BookingId, x.RoomId });

            modelBuilder.Entity<BookingRoom>()
                .HasOne(x => x.Booking)
                .WithMany(b => b.BookingRooms)
                .HasForeignKey(x => x.BookingId);

            modelBuilder.Entity<BookingRoom>()
                .HasOne(x => x.Room)
                .WithMany(r => r.BookingRooms)
                .HasForeignKey(x => x.RoomId);

            // Booking - Review 1-1
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Review)
                .WithOne(r => r.Booking)
                .HasForeignKey<Review>(r => r.BookingId);

            // Booking - Customer (Many-to-One)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId);

            modelBuilder.Entity<RoomImage>()
                .HasOne(x => x.Room)
                .WithMany(r => r.RoomImages)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);


            // money precision
            modelBuilder.Entity<RoomType>().Property(x => x.PricePerNight).HasPrecision(18, 2);
            modelBuilder.Entity<Booking>().Property(x => x.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);

            // TxnRef unique (để sau xử lý IPN không trùng)
            modelBuilder.Entity<Payment>()
                .HasIndex(x => x.TxnRef)
                .IsUnique()
                .HasFilter("[TxnRef] IS NOT NULL");
        }
    }
}
