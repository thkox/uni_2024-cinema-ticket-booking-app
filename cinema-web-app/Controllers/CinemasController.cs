using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cinema_web_app.Data;
using cinema_web_app.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace cinema_web_app.Controllers
{
    public class CinemasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CinemasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cinemas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Cinemas.ToListAsync());
        }

        // GET: Cinemas/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            DateTime currentDate = DateTime.Now;
            DateTime next7Days = currentDate.AddDays(7);

            var cinemaScreenings = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.ScreeningRoom)
                .Where(s => s.ScreeningRoom.CinemaId == id && s.StartTime >= currentDate && s.StartTime <= next7Days)
                .OrderBy(s => s.StartTime)
                .GroupBy(s => s.Movie)
                .ToListAsync();
            
            if (cinemaScreenings == null)
            {
                return NotFound();
            }
            
            if (cinemaScreenings.Count == 0)
            {
                ViewData["message"] = "No screenings in the next 7 days.";
            }

            return View(cinemaScreenings);
        }

        // GET: Cinemas/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cinemas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,City,ZipCode,Email,NoOfScreeningRooms")] Cinema cinema)
        {
            ModelState.Remove(nameof(Cinema.Announcements));
            ModelState.Remove(nameof(Cinema.ScreeningRooms));
            ModelState.Remove(nameof(Cinema.ContentCinemaAdmins));
            
            if (ModelState.IsValid)
            {
                cinema.Id = Guid.NewGuid();
                _context.Add(cinema);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cinema);
        }

        // GET: Cinemas/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema == null)
            {
                return NotFound();
            }
            return View(cinema);
        }

        // POST: Cinemas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Address,City,ZipCode,Email,NoOfScreeningRooms")] Cinema cinema)
        {
            if (id != cinema.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cinema);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CinemaExists(cinema.Id))
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
            return View(cinema);
        }

        // GET: Cinemas/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cinema = await _context.Cinemas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cinema == null)
            {
                return NotFound();
            }

            return View(cinema);
        }

        // POST: Cinemas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cinema = await _context.Cinemas.FindAsync(id);
            if (cinema != null)
            {
                _context.Cinemas.Remove(cinema);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CinemaExists(Guid id)
        {
            return _context.Cinemas.Any(e => e.Id == id);
        }
    }
}
