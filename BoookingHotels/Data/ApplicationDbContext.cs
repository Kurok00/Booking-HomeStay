using BoookingHotels.Models;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Data;
using System.Reflection.Metadata;

namespace BoookingHotels.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }
        public DbSet<Photos> Photoss { get; set; }
        public DbSet<RoomAmenitie> RoomAmenities { get; set; }
        public DbSet<Amenities> Amenities { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewPhoto> ReviewPhotos { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Hotel>()
    .HasOne(h => h.Creator)
    .WithMany()
    .HasForeignKey(h => h.CreatedBy)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RoomAmenitie>()
                .HasKey(ra => new { ra.RoomId, ra.AmenityId });

            modelBuilder.Entity<UserRole>()
          .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<Booking>()
            .Property(b => b.Status)
            .HasConversion(new EnumToStringConverter<BookingStatus>());
            modelBuilder.Entity<Photos>()
             .HasKey(p => p.PhotoId);

            // Cấu hình relationship cho AdminLog
            modelBuilder.Entity<AdminLog>()
                .HasOne(al => al.Admin)
                .WithMany()
                .HasForeignKey(al => al.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
