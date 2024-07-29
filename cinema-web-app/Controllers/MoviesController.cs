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
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movies/PlayingNow
        public IActionResult PlayingNow()
        {
            var today = DateTime.UtcNow.Date;
            var movies = _context.Movies
                .Include(m => m.Genre)
                .Where(m => m.ReleaseDate <= today)
                .ToList();
            return View(movies);
        }

        // GET: Movies/ComingSoon
        public IActionResult ComingSoon()
        {
            var today = DateTime.UtcNow.Date;
            var movies = _context.Movies
                .Include(m => m.Genre)
                .Where(m => m.ReleaseDate > today)
                .ToList();
            return View(movies);
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Movies.Include(m => m.Genre);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name");
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,GenreId,Duration,Content,Description,ReleaseDate,Director,ImageUrl")] Movie movie)
        {
            ModelState.Remove(nameof(movie.Genre));
            
            if (!ModelState.IsValid)
            {
                ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
                return View(movie);
            }
            
            bool  movieExists = _context.Movies.Any(m => m.Title == movie.Title && m.ReleaseDate == movie.ReleaseDate);

            if (movieExists)
            {
                ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
                ViewData["Error"] = "Movie already exists.";
                return View(movie);
            }
            
            movie.Id = Guid.NewGuid();
            _context.Add(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Title,GenreId,Duration,Content,Description,ReleaseDate,Director,ImageUrl")] Movie movie)
        {
            ModelState.Remove(nameof(movie.Genre));
            
            if (id != movie.Id)
            {
                return NotFound();
            }
            
            if (!ModelState.IsValid)
            {
                ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
                return View(movie);
            }
            
            bool movieExists = _context.Movies.Any(m => m.Title == movie.Title && m.ReleaseDate == movie.ReleaseDate && m.Id != movie.Id);
    
            if (movieExists)
            {
                ViewData["GenreId"] = new SelectList(_context.Genres, "Id", "Name", movie.GenreId);
                ViewData["Error"] = "Movie already exists.";
                return View(movie);
            }
            
            try
            {
                _context.Update(movie);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.Id))
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

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.Genre)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var movie = await _context.Movies.Where(m => m.Id == id).Include(m => m.Screenings).ThenInclude(s => s.Reservations).FirstAsync();
            if (movie != null)
            {
                _context.Reservations.RemoveRange(movie.Screenings.SelectMany(s => s.Reservations));
                _context.Screenings.RemoveRange(movie.Screenings);
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(Guid id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}
