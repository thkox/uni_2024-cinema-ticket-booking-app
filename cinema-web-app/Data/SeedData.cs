using cinema_web_app.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            await CreateUserAndAssignRole(userManager, "appadmin1@example.com", "ApplicationAdmin", "Alice", "Admin", "Admin@123");
            await CreateUserAndAssignRole(userManager, "cinemaadmin1@example.com", "ContentCinemaAdmin", "Bob", "Admin", "Admin@123");
            await CreateUserAndAssignRole(userManager, "contentappadmin1@example.com", "ContentAppAdmin", "Carol", "Admin", "Admin@123");
            await CreateUserAndAssignRole(userManager, "customer1@example.com", "Customer", "David", "Customer", "Customer@123");
            await CreateUserAndAssignRole(userManager, "customer2@example.com", "Customer", "Eve", "Customer", "Customer@123");

            // Insert dummy data if not already present
            if (!context.Cinemas.Any())
            {
                var cinemaOne = new Cinema
                {
                    Name = "Cinema One",
                    Address = "123 Movie St",
                    City = "Movietown",
                    ZipCode = "12345",
                    Email = "info@cinemaone.com",
                    NoOfScreeningRooms = 5
                };
                var cinemaTwo = new Cinema
                {
                    Name = "Cinema Two",
                    Address = "456 Film Rd",
                    City = "Filmtown",
                    ZipCode = "67890",
                    Email = "info@cinematwo.com",
                    NoOfScreeningRooms = 3
                };

                context.Cinemas.AddRange(cinemaOne, cinemaTwo);
                await context.SaveChangesAsync();

                var screeningRoom1 = new ScreeningRoom
                {
                    CinemaId = cinemaOne.Id,
                    Name = "Screening Room 1",
                    TotalNoOfSeats = 100,
                    Is3D = true
                };
                var screeningRoom2 = new ScreeningRoom
                {
                    CinemaId = cinemaOne.Id,
                    Name = "Screening Room 2",
                    TotalNoOfSeats = 80,
                    Is3D = false
                };
                var screeningRoom3 = new ScreeningRoom
                {
                    CinemaId = cinemaTwo.Id,
                    Name = "Screening Room 3",
                    TotalNoOfSeats = 120,
                    Is3D = true
                };
                var screeningRoom4 = new ScreeningRoom
                {
                    CinemaId = cinemaTwo.Id,
                    Name = "Screening Room 4",
                    TotalNoOfSeats = 60,
                    Is3D = false
                };

                context.ScreeningRooms.AddRange(screeningRoom1, screeningRoom2, screeningRoom3, screeningRoom4);
                await context.SaveChangesAsync();

                var actionGenre = new Genre { Name = "Action" };
                var dramaGenre = new Genre { Name = "Drama" };
                context.Genres.AddRange(actionGenre, dramaGenre);
                await context.SaveChangesAsync();

                var movieOne = new Movie
                {
                    Title = "Movie One",
                    GenreId = actionGenre.Id,
                    Duration = 120,
                    Content = "Action content",
                    Description = "An action-packed adventure.",
                    ReleaseDate = DateTime.UtcNow.Date.AddMonths(6),
                    Director = "John Doe",
                    ImageUrl = "http://example.com/movie1.jpg"
                };
                var movieTwo = new Movie
                {
                    Title = "Movie Two",
                    GenreId = dramaGenre.Id,
                    Duration = 90,
                    Content = "Drama content",
                    Description = "A touching drama.",
                    ReleaseDate = DateTime.UtcNow.Date.AddMonths(7),
                    Director = "Jane Smith",
                    ImageUrl = "http://example.com/movie2.jpg"
                };

                context.Movies.AddRange(movieOne, movieTwo);
                await context.SaveChangesAsync();

                var screeningOne = new Screening
                {
                    ScreeningRoomId = screeningRoom1.Id,
                    MovieId = movieOne.Id,
                    StartTime = DateTime.UtcNow.AddDays(10).AddHours(18),
                    RemainingNoOfSeats = screeningRoom1.TotalNoOfSeats
                };
                var screeningTwo = new Screening
                {
                    ScreeningRoomId = screeningRoom3.Id,
                    MovieId = movieTwo.Id,
                    StartTime = DateTime.UtcNow.AddDays(11).AddHours(20),
                    RemainingNoOfSeats = screeningRoom3.TotalNoOfSeats
                };

                context.Screenings.AddRange(screeningOne, screeningTwo);
                await context.SaveChangesAsync();

                var adminUser = await userManager.FindByEmailAsync("appadmin1@example.com");
                var cinemaAdminUser = await userManager.FindByEmailAsync("cinemaadmin1@example.com");
                var contentAppAdminUser = await userManager.FindByEmailAsync("contentappadmin1@example.com");

                // Add ApplicationAdmin
                context.ApplicationAdmins.Add(new ApplicationAdmin { UserId = adminUser.Id });
                await context.SaveChangesAsync();

                // Add ContentCinemaAdmin with a link to Cinema One
                context.ContentCinemaAdmins.Add(new ContentCinemaAdmin { UserId = cinemaAdminUser.Id, CinemaId = cinemaOne.Id });
                await context.SaveChangesAsync();

                // Add ContentAppAdmin
                context.ContentAppAdmins.Add(new ContentAppAdmin { UserId = contentAppAdminUser.Id });
                await context.SaveChangesAsync();

                // Add Customers
                var customerUser1 = await userManager.FindByEmailAsync("customer1@example.com");
                var customerUser2 = await userManager.FindByEmailAsync("customer2@example.com");
                if (customerUser1 == null || customerUser2 == null)
                {
                    throw new Exception("One or both customer users were not created.");
                }

                context.Customers.AddRange(
                    new Customer { UserId = customerUser1.Id },
                    new Customer { UserId = customerUser2.Id }
                );
                await context.SaveChangesAsync();

                // Verify Customers
                var customers = context.Customers.ToList();
                Console.WriteLine($"Number of customers: {customers.Count}");
                foreach (var customer in customers)
                {
                    Console.WriteLine($"Customer ID: {customer.UserId}");
                }

                // Add Reservations
                var reservation1 = new Reservation
                {
                    ScreeningId = screeningOne.Id,
                    CustomerId = customerUser1.Id,
                    NoOfBookedSeats = 2
                };
                var reservation2 = new Reservation
                {
                    ScreeningId = screeningTwo.Id,
                    CustomerId = customerUser2.Id,
                    NoOfBookedSeats = 1
                };

                context.Reservations.AddRange(reservation1, reservation2);
                await context.SaveChangesAsync();


                // Add Announcements
                var announcement1 = new Announcement
                {
                    CinemaId = cinemaOne.Id,
                    Title = "New Movie Release",
                    Message = "We are excited to announce the release of Movie One!"
                };
                var announcement2 = new Announcement
                {
                    CinemaId = cinemaTwo.Id,
                    Title = "Special Screening",
                    Message = "Join us for a special screening of Movie Two."
                };

                context.Announcements.AddRange(announcement1, announcement2);
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
