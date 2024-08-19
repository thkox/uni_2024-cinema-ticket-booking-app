using cinema_web_app.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cinema_web_app.Utilities;

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
                ("cinemaadmin2@example.com", "ContentCinemaAdmin", "Mario", "Admin", "Admin@123"),
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
                    new Genre { Name = "Drama" },
                    new Genre { Name = "Science Fiction" },
                    new Genre { Name = "Comedy" },
                    new Genre { Name = "Adventure" },
                    new Genre { Name = "Animation" }
                };
                context.Genres.AddRange(genres);
                await context.SaveChangesAsync();

                // Add hardcoded movies if not already present
                if (!context.Movies.Any())
                {
                    var releasedMovies = GetReleasedMovies(genres);
                    var upcomingMovies = GetUpcomingMovies(genres);

                    context.Movies.AddRange(releasedMovies);
                    context.Movies.AddRange(upcomingMovies);
                    await context.SaveChangesAsync();
                }

                var screenings = GetScreenings(context.Movies.ToList(), screeningRooms);
                context.Screenings.AddRange(screenings);
                await context.SaveChangesAsync();

                var cinemaAdminUser1 = await userManager.FindByEmailAsync("cinemaadmin1@example.com");
                var cinemaAdminUser2 = await userManager.FindByEmailAsync("cinemaadmin2@example.com");

                // Add ContentCinemaAdmin with a link to Cinema One
                context.ContentCinemaAdmins.Add(new ContentCinemaAdmin { UserId = cinemaAdminUser1.Id, CinemaId = cinemas[0].Id });
                context.ContentCinemaAdmins.Add(new ContentCinemaAdmin { UserId = cinemaAdminUser2.Id, CinemaId = cinemas[1].Id });
                
                await context.SaveChangesAsync();

                // Add Customers
                var customerUser1 = await userManager.FindByEmailAsync("customer1@example.com");
                var customerUser2 = await userManager.FindByEmailAsync("customer2@example.com");
                
                // Add Reservations
                var reservations = new List<Reservation>
                {
                    new Reservation
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customerUser1.Id,
                        NoOfBookedSeats = 2,
                        ScreeningId = screenings.First().Id,
                        ShortReferenceId = ReferenceIdGenerator.GenerateReferenceId()
                    },
                    new Reservation
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = customerUser2.Id,
                        NoOfBookedSeats = 3,
                        ScreeningId = screenings.Skip(1).First().Id,
                        ShortReferenceId = ReferenceIdGenerator.GenerateReferenceId()
                    }
                };
                context.Reservations.AddRange(reservations);
                await context.SaveChangesAsync();

                // Add Announcements
                var announcements = new List<Announcement>
                {
                    new Announcement
                    {
                        CinemaId = cinemas[0].Id,
                        UserId = cinemaAdminUser1.Id,
                        Title = "New Movie Release",
                        Message = "We are excited to announce the release of 'Doctor Strange in the Multiverse of Madness'!",
                        PublicationDate = DateTime.UtcNow.AddDays(-10)
                    },
                    new Announcement
                    {
                        CinemaId = cinemas[1].Id,
                        UserId = cinemaAdminUser2.Id,
                        Title = "Special Screening",
                        Message = "Join us for a special screening of 'Indiana Jones and the Dial of Destiny'.",
                        PublicationDate = DateTime.UtcNow.AddDays(-5) 
                    }
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

        private static List<Movie> GetReleasedMovies(List<Genre> genres)
        {
            var actionGenre = genres.First(g => g.Name == "Action").Id;
            var scienceFictionGenre = genres.First(g => g.Name == "Science Fiction").Id;
            var animationGenre = genres.First(g => g.Name == "Animation").Id;
            var comedyGenre = genres.First(g => g.Name == "Comedy").Id;

            return new List<Movie>
            {
                new Movie
                {
                    Title = "Spider-Man: No Way Home",
                    GenreId = actionGenre,
                    Duration = 148,
                    Content = "Peter Parker's secret identity is revealed to the entire world.",
                    Description = "Peter Parker navigates a world where everyone knows he is Spider-Man.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-100),
                    Director = "Jon Watts",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg"
                },
                new Movie
                {
                    Title = "Dune",
                    GenreId = scienceFictionGenre,
                    Duration = 155,
                    Content = "Paul Atreides, a brilliant and gifted young man born into a great destiny.",
                    Description = "Paul Atreides must travel to the most dangerous planet in the universe.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-200),
                    Director = "Denis Villeneuve",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/d5NXSklXo0qyIYkgV94XAgMIckC.jpg"
                },
                new Movie
                {
                    Title = "The Batman",
                    GenreId = actionGenre,
                    Duration = 176,
                    Content = "Batman ventures into Gotham City's underworld.",
                    Description = "Batman investigates the corruption in Gotham City.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-150),
                    Director = "Matt Reeves",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/74xTEgt7R36Fpooo50r9T25onhq.jpg"
                },
                new Movie
                {
                    Title = "No Time to Die",
                    GenreId = actionGenre,
                    Duration = 163,
                    Content = "James Bond has left active service.",
                    Description = "James Bond is pulled back into action to face a new threat.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-250),
                    Director = "Cary Joji Fukunaga",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/iUgygt3fscRoKWCV1d0C7FbM9TP.jpg"
                },
                new Movie
                {
                    Title = "The Matrix Resurrections",
                    GenreId = scienceFictionGenre,
                    Duration = 148,
                    Content = "Return to a world of two realities.",
                    Description = "Neo must decide to follow the white rabbit once more.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-50),
                    Director = "Lana Wachowski",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/8c4a8kE7PizaGQQnditMmI1xbRp.jpg"
                },
                new Movie
                {
                    Title = "Top Gun: Maverick",
                    GenreId = actionGenre,
                    Duration = 131,
                    Content = "After more than thirty years of service as one of the Navy's top aviators, Pete 'Maverick' Mitchell is where he belongs.",
                    Description = "Maverick must confront the ghosts of his past.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-20),
                    Director = "Joseph Kosinski",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/62HCnUTziyWcpDaBO2i1DX17ljH.jpg"
                },
                new Movie
                {
                    Title = "Mission: Impossible - Dead Reckoning Part One",
                    GenreId = actionGenre,
                    Duration = 160,
                    Content = "Ethan Hunt and his IMF team embark on their most dangerous mission yet.",
                    Description = "Ethan Hunt must track down a terrifying new weapon.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-80),
                    Director = "Christopher McQuarrie",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/NNxYkU70HPurnNCSiCjYAmacwm.jpg"
                },
                new Movie
                {
                    Title = "Black Panther: Wakanda Forever",
                    GenreId = actionGenre,
                    Duration = 161,
                    Content = "The people of Wakanda fight to protect their home.",
                    Description = "Wakanda must protect its people after King T'Challa's death.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-120),
                    Director = "Ryan Coogler",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/sv1xJUazXeYqALzczSZ3O6nkH75.jpg"
                },
                new Movie
                {
                    Title = "Avatar: The Way of Water",
                    GenreId = scienceFictionGenre,
                    Duration = 190,
                    Content = "Jake Sully lives with his newfound family formed on the planet of Pandora.",
                    Description = "Jake must protect Pandora from a new threat.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-300),
                    Director = "James Cameron",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/t6HIqrRAclMCA60NsSmeqe9RmNV.jpg"
                },
                new Movie
                {
                    Title = "The Flash",
                    GenreId = actionGenre,
                    Duration = 144,
                    Content = "Barry Allen uses his super speed to change the past.",
                    Description = "Barry Allen enters the multiverse.",
                    ReleaseDate = DateTime.UtcNow.AddDays(-60),
                    Director = "Andy Muschietti",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/rktDFPbfHfUbArZ6OOOKsXcv0Bm.jpg"
                }
            };
        }

        private static List<Movie> GetUpcomingMovies(List<Genre> genres)
        {
            var actionGenre = genres.First(g => g.Name == "Action").Id;
            var scienceFictionGenre = genres.First(g => g.Name == "Science Fiction").Id;
            var animationGenre = genres.First(g => g.Name == "Animation").Id;
            var adventureGenre = genres.First(g => g.Name == "Adventure").Id;

            return new List<Movie>
            {
                new Movie
                {
                    Title = "Indiana Jones and the Dial of Destiny",
                    GenreId = adventureGenre,
                    Duration = 130,
                    Content = "Indiana Jones returns for another adventure.",
                    Description = "Indiana Jones must uncover the Dial of Destiny.",
                    ReleaseDate = DateTime.UtcNow.AddDays(90),
                    Director = "James Mangold",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/Af4bXE63pVsb2FtbW8uYIyPBadD.jpg"
                },
                new Movie
                {
                    Title = "Thor: Love and Thunder",
                    GenreId = actionGenre,
                    Duration = 119,
                    Content = "Thor must face Gorr the God Butcher.",
                    Description = "Thor's journey of self-discovery is interrupted by a galactic killer.",
                    ReleaseDate = DateTime.UtcNow.AddDays(60),
                    Director = "Taika Waititi",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/pIkRyD18kl4FhoCNQuWxWu5cBLM.jpg"
                },
                new Movie
                {
                    Title = "Doctor Strange in the Multiverse of Madness",
                    GenreId = actionGenre,
                    Duration = 126,
                    Content = "Doctor Strange unlocks the Multiverse.",
                    Description = "Doctor Strange navigates the Multiverse to face a new threat.",
                    ReleaseDate = DateTime.UtcNow.AddDays(45),
                    Director = "Sam Raimi",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/9Gtg2DzBhmYamXBS1hKAhiwbBKS.jpg"
                },
                new Movie
                {
                    Title = "Guardians of the Galaxy Vol. 3",
                    GenreId = actionGenre,
                    Duration = 140,
                    Content = "The Guardians embark on a new mission.",
                    Description = "The Guardians of the Galaxy face their most difficult mission yet.",
                    ReleaseDate = DateTime.UtcNow.AddDays(75),
                    Director = "James Gunn",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/r2J02Z2OpNTctfOSN1Ydgii51I3.jpg"
                },
                new Movie
                {
                    Title = "Jurassic World: Dominion",
                    GenreId = adventureGenre,
                    Duration = 146,
                    Content = "Humans and dinosaurs must learn to coexist.",
                    Description = "A new threat emerges in a world with dinosaurs.",
                    ReleaseDate = DateTime.UtcNow.AddDays(120),
                    Director = "Colin Trevorrow",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/kAVRgw7GgK1CfYEJq8ME6EvRIgU.jpg"
                },
                new Movie
                {
                    Title = "Lightyear",
                    GenreId = animationGenre,
                    Duration = 105,
                    Content = "The origin story of Buzz Lightyear.",
                    Description = "Follow the adventures of the young test pilot that inspired the toy.",
                    ReleaseDate = DateTime.UtcNow.AddDays(30),
                    Director = "Angus MacLane",
                    ImageUrl = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2/b9t3w1loraDh7hjdWmpc9ZsaYns.jpg"
                }
            };
        }

        private static List<Screening> GetScreenings(List<Movie> movies, List<ScreeningRoom> screeningRooms)
        {
            var screenings = new List<Screening>();
            var startTime = DateTime.UtcNow.AddDays(11).AddHours(20);

            foreach (var movie in movies)
            {
                foreach (var screeningRoom in screeningRooms)
                {
                    screenings.Add(new Screening
                    {
                        ScreeningRoomId = screeningRoom.Id,
                        MovieId = movie.Id,
                        StartTime = startTime,
                        RemainingNoOfSeats = screeningRoom.TotalNoOfSeats
                    });
                    startTime = startTime.AddDays(1).AddHours(2); // Ensure unique StartTime for each screening
                }
            }
            return screenings;
        }
    }
}
