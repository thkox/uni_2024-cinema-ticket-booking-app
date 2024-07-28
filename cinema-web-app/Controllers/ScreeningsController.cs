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
    public class ScreeningsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScreeningsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Screenings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Screenings.Include(s => s.Movie).Include(s => s.ScreeningRoom);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Screenings/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.ScreeningRoom)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (screening == null)
            {
                return NotFound();
            }

            return View(screening);
        }

        // GET: Screenings/Create
        public IActionResult Create()
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Name");
            ViewData["ScreeningRoomId"] = new SelectList(_context.ScreeningRooms, "Id", "Name");
            return View();
        }

        // POST: Screenings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ScreeningRoomId,MovieId,StartTime,RemainingNoOfSeats")] Screening screening)
        {
            ModelState.Remove(nameof(screening.Movie));
            ModelState.Remove(nameof(screening.ScreeningRoom));
            
            if (ModelState.IsValid)
            {
                var remainingNoOfSeats = _context.ScreeningRooms.Where(s => s.Id == screening.ScreeningRoomId)
                    .Select(s => s.TotalNoOfSeats).FirstOrDefault();
                screening.RemainingNoOfSeats = remainingNoOfSeats;
                screening.Id = Guid.NewGuid();
                _context.Add(screening);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Name", screening.MovieId);
            ViewData["ScreeningRoomId"] = new SelectList(_context.ScreeningRooms, "Id", "Name", screening.ScreeningRoomId);
            return View(screening);
        }

        // GET: Screenings/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var screening = await _context.Screenings.FindAsync(id);
            if (screening == null)
            {
                return NotFound();
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", screening.MovieId);
            ViewData["ScreeningRoomId"] = new SelectList(_context.ScreeningRooms, "Id", "Name", screening.ScreeningRoomId);
            return View(screening);
        }

        // POST: Screenings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ScreeningRoomId,MovieId,StartTime,RemainingNoOfSeats")] Screening screening)
        {
            ModelState.Remove(nameof(screening.Movie));
            ModelState.Remove(nameof(screening.ScreeningRoom));
            
            if (id != screening.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(screening);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScreeningExists(screening.Id))
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
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", screening.MovieId);
            ViewData["ScreeningRoomId"] = new SelectList(_context.ScreeningRooms, "Id", "Name", screening.ScreeningRoomId);
            return View(screening);
        }

        // GET: Screenings/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.ScreeningRoom)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (screening == null)
            {
                return NotFound();
            }

            return View(screening);
        }

        // POST: Screenings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var screening = await _context.Screenings.Where(s => s.Id == id).Include(s => s.Reservations).FirstOrDefaultAsync();
            if (screening != null)
            {
                _context.Reservations.RemoveRange(screening.Reservations);
                _context.Screenings.Remove(screening);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScreeningExists(Guid id)
        {
            return _context.Screenings.Any(e => e.Id == id);
        }
    }
}