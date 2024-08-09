using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cinema_web_app.Data;
using cinema_web_app.Models;
using cinema_web_app.Utilities;
using Microsoft.AspNetCore.Identity;

namespace cinema_web_app.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PDFGeneratorService _pdfService;

        public ReservationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, PDFGeneratorService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _pdfService = pdfService;
        }
        
        // GET: Reservations/DownloadPdf/5
        public async Task<IActionResult> DownloadPdf(Guid id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.Screening)
                .ThenInclude(s => s.Movie)
                .Include(r => r.Screening.ScreeningRoom)
                .ThenInclude(sr => sr.Cinema)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            // Generate PDF asynchronously
            byte[] pdfBytes;
            try
            {
                pdfBytes = await _pdfService.GenerateReservationPdfAsync(reservation);
            }
            catch (Exception ex)
            {
                // Log the exception and return an appropriate error response
                // You might want to use a logging framework like Serilog, NLog, etc.
                Console.WriteLine($"An error occurred while generating the PDF: {ex.Message}");
                return StatusCode(500, "An error occurred while generating the PDF.");
            }

            // Return the PDF file
            return File(pdfBytes, "application/pdf", "ReservationDetails.pdf");
        }


        // GET: Reservations/MyReservations
        public async Task<IActionResult> MyReservations()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var reservations = _context.Reservations
                    .Include(r => r.Customer)
                    .Include(r => r.Screening)
                    .Include(r => r.Screening.ScreeningRoom)
                    .Include(r => r.Screening.Movie)
                    .Where(r => r.CustomerId == userId)
                    .OrderBy(r => r.Screening.StartTime);

                return View(await reservations.ToListAsync());
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Reservations.Include(r => r.Customer).Include(r => r.Screening);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Screening)
                .ThenInclude(s => s.Movie) // Include Movie details
                .ThenInclude(m => m.Genre) // If Genre is needed
                .Include(r => r.Screening.ScreeningRoom) // Include Screening Room details
                .ThenInclude(sr => sr.Cinema) // Include Cinema details
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "FirstName");
            ViewData["ScreeningId"] = new SelectList(_context.Screenings, "Id", "Id");
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ScreeningId,CustomerId,NoOfBookedSeats")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                reservation.Id = Guid.NewGuid();
                reservation.ShortReferenceId = ReferenceIdGenerator.GenerateReferenceId();

                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "FirstName", reservation.CustomerId);
            ViewData["ScreeningId"] = new SelectList(_context.Screenings, "Id", "Id", reservation.ScreeningId);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "FirstName", reservation.CustomerId);
            ViewData["ScreeningId"] = new SelectList(_context.Screenings, "Id", "Id", reservation.ScreeningId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ScreeningId,CustomerId,NoOfBookedSeats")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Users, "Id", "FirstName", reservation.CustomerId);
            ViewData["ScreeningId"] = new SelectList(_context.Screenings, "Id", "Id", reservation.ScreeningId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.Screening)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/BookTickets
        [HttpGet]
        public IActionResult BookTickets(Guid? movieId)
        {
            if (movieId.HasValue)
            {
                Console.WriteLine($"Received movieId: {movieId.Value}");
            }
            else
            {
                Console.WriteLine("No movieId provided.");
            }

            var movies = _context.Movies
                .Include(m => m.Screenings)
                .ThenInclude(s => s.ScreeningRoom)
                .ThenInclude(sr => sr.Cinema)
                .ToList();

            ViewData["Movies"] = new SelectList(movies, "Id", "Title");

            var model = new ReservationViewModel();
            if (movieId.HasValue)
            {
                model.MovieId = movieId.Value;
            }

            return View(model);
        }


        // POST: Reservations/BookTickets
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookTickets(ReservationViewModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user.Id;

                var screening = await _context.Screenings
                    .Include(s => s.ScreeningRoom)
                    .FirstOrDefaultAsync(s => s.Id == model.ScreeningId);

                if (screening != null && screening.RemainingNoOfSeats >= model.NoOfSeats)
                {
                    var reservation = new Reservation
                    {
                        Id = Guid.NewGuid(),
                        ScreeningId = model.ScreeningId,
                        CustomerId = userId,
                        NoOfBookedSeats = model.NoOfSeats,
                        ShortReferenceId = ReferenceIdGenerator.GenerateReferenceId()
                    };

                    screening.RemainingNoOfSeats -= model.NoOfSeats;

                    _context.Add(reservation);
                    _context.Update(screening);
                    await _context.SaveChangesAsync();

                    // Redirect to the reservation details page
                    return RedirectToAction("Details", "Reservations", new { id = reservation.Id });
                }
                else
                {
                    ModelState.AddModelError("", "Not enough seats available.");
                }
            }

            // Set error message if reservation failed
            ViewData["ErrorMessage"] = "An error occurred while booking your reservation.";
            var movies = _context.Movies
                .Include(m => m.Screenings)
                .ThenInclude(s => s.ScreeningRoom)
                .ThenInclude(sr => sr.Cinema)
                .ToList();

            ViewData["Movies"] = new SelectList(movies, "Id", "Title");
            return View(model);
        }

        // Get cinemas for a specific movie
        public JsonResult GetCinemasForMovie(Guid movieId)
        {
            var cinemas = _context.Screenings
                .Include(s => s.ScreeningRoom)
                .ThenInclude(sr => sr.Cinema)
                .Where(s => s.MovieId == movieId)
                .Select(s => s.ScreeningRoom.Cinema)
                .Distinct()
                .Select(c => new { c.Id, c.Name })
                .ToList();

            return Json(cinemas);
        }

        // Get screenings for a specific cinema and movie
        public JsonResult GetScreeningsForCinemaAndMovie(Guid cinemaId, Guid movieId)
        {
            var screenings = _context.Screenings
                .Include(s => s.ScreeningRoom)
                .Where(s => s.MovieId == movieId && s.ScreeningRoom.CinemaId == cinemaId && s.RemainingNoOfSeats > 0)
                .Select(s => new
                {
                    s.Id,
                    DateTime = s.StartTime.ToString("MMMM d, yyyy - h:mm tt"),
                    ScreeningRoom = s.ScreeningRoom.Name
                })
                .ToList();

            return Json(screenings);
        }

        // Get remaining seats for a specific screening
        public JsonResult GetRemainingSeats(Guid screeningId)
        {
            var remainingSeats = _context.Screenings
                .Where(s => s.Id == screeningId)
                .Select(s => s.RemainingNoOfSeats)
                .FirstOrDefault();

            return Json(remainingSeats);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(Guid id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
