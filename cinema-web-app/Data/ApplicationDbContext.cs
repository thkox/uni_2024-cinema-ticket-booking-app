using cinema_web_app.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace cinema_web_app.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cinema> Cinemas { get; set; }
        public DbSet<ScreeningRoom> ScreeningRooms { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Screening> Screenings { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<ContentCinemaAdmin> ContentCinemaAdmins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.ContentCinemaAdmins)
                .WithOne(ca => ca.User)
                .HasForeignKey(ca => ca.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Cinema
            modelBuilder.Entity<Cinema>()
                .HasMany(c => c.ScreeningRooms)
                .WithOne(sr => sr.Cinema)
                .HasForeignKey(sr => sr.CinemaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cinema>()
                .HasMany(c => c.Announcements)
                .WithOne(a => a.Cinema)
                .HasForeignKey(a => a.CinemaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Announcement
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ScreeningRoom
            modelBuilder.Entity<ScreeningRoom>()
                .HasMany(sr => sr.Screenings)
                .WithOne(s => s.ScreeningRoom)
                .HasForeignKey(s => s.ScreeningRoomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Movie
            modelBuilder.Entity<Movie>()
                .HasMany(m => m.Screenings)
                .WithOne(s => s.Movie)
                .HasForeignKey(s => s.MovieId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Screening
            modelBuilder.Entity<Screening>()
                .HasMany(s => s.Reservations)
                .WithOne(r => r.Screening)
                .HasForeignKey(r => r.ScreeningId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Reservation
            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ContentCinemaAdmin
            modelBuilder.Entity<ContentCinemaAdmin>()
                .HasOne(ca => ca.User)
                .WithMany(u => u.ContentCinemaAdmins)
                .HasForeignKey(ca => ca.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Genre
            modelBuilder.Entity<Genre>()
                .HasMany(g => g.Movies)
                .WithOne(m => m.Genre)
                .HasForeignKey(m => m.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
