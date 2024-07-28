using cinema_web_app.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cinema_web_app.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Define roles
            string[] roleNames = { "ApplicationAdmin", "ContentCinemaAdmin", "ContentAppAdmin", "Customer" };
            IdentityResult roleResult;

            // Create roles if they don't already exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"Failed to create role '{roleName}'. Errors: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                    }
                }
            }

            // Create and assign roles to users
            var users = new List<(string Email, string Role, string FirstName, string LastName, string Password)>
            {
                ("appadmin1@example.com", "ApplicationAdmin", "Alice", "Admin", "Admin@123"),
                ("cinemaadmin1@example.com", "ContentCinemaAdmin", "Bob", "Admin", "Admin@123"),
                ("contentappadmin1@example.com", "ContentAppAdmin", "Carol", "Admin", "Admin@123"),
                ("customer1@example.com", "Customer", "David", "Customer", "Customer@123"),
                ("customer2@example.com", "Customer", "Eve", "Customer", "Customer@123")
            };

            foreach (var (Email, Role, FirstName, LastName, Password) in users)
            {
                await CreateUserAndAssignRole(userManager, Email, Role, FirstName, LastName, Password);
            }

            // Insert dummy data if not already present
            if (!context.Cinemas.Any())
            {
                var cinemas = new List<Cinema>
                {
                    new Cinema { Name = "Cinema One", Address = "123 Movie St", City = "Movietown", ZipCode = "12345", Email = "info@cinemaone.com", NoOfScreeningRooms = 5 },
                    new Cinema { Name = "Cinema Two", Address = "456 Film Rd", City = "Filmtown", ZipCode = "67890", Email = "info@cinematwo.com", NoOfScreeningRooms = 3 }
                };
                context.Cinemas.AddRange(cinemas);
                await context.SaveChangesAsync();

                var screeningRooms = new List<ScreeningRoom>
                {
                    new ScreeningRoom { CinemaId = cinemas[0].Id, Name = "Screening Room 1", TotalNoOfSeats = 100, Is3D = true },
                    new ScreeningRoom { CinemaId = cinemas[0].Id, Name = "Screening Room 2", TotalNoOfSeats = 80, Is3D = false },
                    new ScreeningRoom { CinemaId = cinemas[1].Id, Name = "Screening Room 3", TotalNoOfSeats = 120, Is3D = true },
                    new ScreeningRoom { CinemaId = cinemas[1].Id, Name = "Screening Room 4", TotalNoOfSeats = 60, Is3D = false }
                };
                context.ScreeningRooms.AddRange(screeningRooms);
                await context.SaveChangesAsync();

                var genres = new List<Genre>
                {
                    new Genre { Name = "Action" },
                    new Genre { Name = "Drama" }
                };
                context.Genres.AddRange(genres);
                await context.SaveChangesAsync();

                var movies = new List<Movie>
                {
                    new Movie { Title = "Movie One", GenreId = genres[0].Id, Duration = 120, Content = "Action content", Description = "An action-packed adventure.", ReleaseDate = DateTime.UtcNow.Date.AddMonths(6), Director = "John Doe", ImageUrl = "http://example.com/movie1.jpg" },
                    new Movie { Title = "Movie Two", GenreId = genres[1].Id, Duration = 90, Content = "Drama content", Description = "A touching drama.", ReleaseDate = DateTime.UtcNow.Date.AddMonths(7), Director = "Jane Smith", ImageUrl = "http://example.com/movie2.jpg" }
                };
                context.Movies.AddRange(movies);
                await context.SaveChangesAsync();

                var screenings = new List<Screening>
                {
                    new Screening { ScreeningRoomId = screeningRooms[0].Id, MovieId = movies[0].Id, StartTime = DateTime.UtcNow.AddDays(10).AddHours(18), RemainingNoOfSeats = screeningRooms[0].TotalNoOfSeats },
                    new Screening { ScreeningRoomId = screeningRooms[2].Id, MovieId = movies[1].Id, StartTime = DateTime.UtcNow.AddDays(11).AddHours(20), RemainingNoOfSeats = screeningRooms[2].TotalNoOfSeats }
                };
                context.Screenings.AddRange(screenings);
                await context.SaveChangesAsync();

                var cinemaAdminUser = await userManager.FindByEmailAsync("cinemaadmin1@example.com");

                // Add ContentCinemaAdmin with a link to Cinema One
                context.ContentCinemaAdmins.Add(new ContentCinemaAdmin { UserId = cinemaAdminUser.Id, CinemaId = cinemas[0].Id });
                await context.SaveChangesAsync();

                // Add Customers
                var customerUser1 = await userManager.FindByEmailAsync("customer1@example.com");
                var customerUser2 = await userManager.FindByEmailAsync("customer2@example.com");
                await context.SaveChangesAsync();

                // Add Reservations
                var reservations = new List<Reservation>
                {
                    new Reservation { ScreeningId = screenings[0].Id, CustomerId = customerUser1.Id, NoOfBookedSeats = 2 },
                    new Reservation { ScreeningId = screenings[1].Id, CustomerId = customerUser2.Id, NoOfBookedSeats = 1 }
                };
                context.Reservations.AddRange(reservations);
                await context.SaveChangesAsync();

                // Add Announcements
                var announcements = new List<Announcement>
                {
                    new Announcement { CinemaId = cinemas[0].Id, Title = "New Movie Release", Message = "We are excited to announce the release of Movie One!" },
                    new Announcement { CinemaId = cinemas[1].Id, Title = "Special Screening", Message = "Join us for a special screening of Movie Two." }
                };
                context.Announcements.AddRange(announcements);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateUserAndAssignRole(UserManager<ApplicationUser> userManager, string email, string role, string firstName, string lastName, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };
                var createResult = await userManager.CreateAsync(user, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    throw new Exception($"Failed to create user '{email}'. Errors: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
