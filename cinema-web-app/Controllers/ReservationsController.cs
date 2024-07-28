using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cinema_web_app.Data;
using cinema_web_app.Models;

namespace cinema_web_app.Controllers
{
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            
            var applicationDbContext = _context.Reservations
                .Include(r => r.Customer)
                .Where(r => userId == r.CustomerId.ToString())
                .Include(r => r.Screening)
                .Include(r => r.Screening.ScreeningRoom)
                .Include(r => r.Screening.Movie)
                .OrderBy(r => r.Screening.StartTime);
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create/{ScreeningId}
        public IActionResult Create(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            Guid guidId = new Guid(id.ToString());
            
            var existingReservation = _context.Reservations
                .Include(r => r.Screening.ScreeningRoom)
                .FirstOrDefault(r => r.CustomerId.ToString() == userId && r.ScreeningId == guidId);
            
            if (existingReservation != null)
            {
                // Reservation already exists, set ViewData to indicate that
                ViewData["ReservationExists"] = true;
                ViewData["ReservationId"] = existingReservation.Id; // Pass the existing reservation id if needed
                ViewData["CinemaId"] = existingReservation.Screening.ScreeningRoom.CinemaId; // Pass the cinema id
                return View();
            }
            
            var screening = _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.ScreeningRoom)
                .FirstOrDefault(s => s.Id == guidId);
            
            if (screening == null)
            {
                // Handle case where screening is not found
                return NotFound();
            }
            
            
            ViewData["Screening"] = screening;
            ViewData["CinemaId"] = screening.ScreeningRoom.CinemaId;
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ScreeningId,CustomerId,NoOfBookedSeats")] Reservation reservation)
        {
            ModelState.Remove("Customer");
            ModelState.Remove("Screening");
            
            var screening_db = await _context.Screenings.FindAsync(reservation.ScreeningId);

            if (screening_db == null)
            {
                return NotFound();
            }
            
            int remainingNoOfSeats = screening_db.RemainingNoOfSeats;
            
            if (reservation.NoOfBookedSeats <= 0)
            {
                ModelState.AddModelError("NoOfBookedSeats", "Please enter a positive number");
            }
            
            if (reservation.NoOfBookedSeats > remainingNoOfSeats)
            {
                ModelState.AddModelError("NoOfBookedSeats", "Not enough remaining seats");
            }

            if (!ModelState.IsValid)
            {
                var screening = _context.Screenings
                    .Include(s => s.Movie)
                    .Include(s => s.ScreeningRoom)
                    .FirstOrDefault(s => s.Id == reservation.ScreeningId);
                
                if (screening == null)
                {
                    // Handle case where customer or screening is not found
                    return NotFound();
                }
                
                ViewData["Screening"] = screening;
                return View(new List<Reservation> { reservation });
            }
            
            if (ModelState.IsValid)
            {
                reservation.Id = Guid.NewGuid();
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Update the remaining number of seats in the screening
            screening_db.RemainingNoOfSeats -= reservation.NoOfBookedSeats;
            _context.Update(screening_db);

            // Add the reservation to the context
            _context.Add(reservation);
            
            // Save changes to the database
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
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
