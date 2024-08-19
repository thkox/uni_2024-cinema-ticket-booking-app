using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cinema_web_app.Data;
using cinema_web_app.Models;
using Microsoft.AspNetCore.Identity;

namespace cinema_web_app.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnnouncementsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Announcements/Index
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var user = await _userManager.Users
                .Include(u => u.ContentCinemaAdmins)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var userCinemaId = user?.ContentCinemaAdmins?.FirstOrDefault()?.CinemaId;
            
            ViewData["UserCinemaId"] = userCinemaId;
            
            var applicationDbContext = _context.Announcements.Include(a => a.Cinema).Include(a => a.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Announcements/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var userId = Guid.Parse(_userManager.GetUserId(User));
            var user = await _userManager.Users
                .Include(u => u.ContentCinemaAdmins)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var userCinemaId = user?.ContentCinemaAdmins?.FirstOrDefault()?.CinemaId;
            
            ViewData["UserCinemaId"] = userCinemaId;

            var announcement = await _context.Announcements
                .Include(a => a.Cinema)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // GET: Announcements/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Announcements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CinemaId,UserId,Title,Message,PublicationDate")] Announcement announcement)
        {
            ModelState.Remove(nameof(Announcement.Cinema));
            ModelState.Remove(nameof(Announcement.User));
            var userId = await _userManager.GetUserIdAsync(await _userManager.GetUserAsync(User));
            announcement.UserId = Guid.Parse(userId);
            var cinemaId = await _context.ContentCinemaAdmins.Where(c => c.UserId == announcement.UserId).Select(c => c.CinemaId).FirstOrDefaultAsync();
            announcement.CinemaId = cinemaId;
            if (ModelState.IsValid)
            {
                announcement.PublicationDate = DateTime.UtcNow;
                
                announcement.Id = Guid.NewGuid();
                _context.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        // GET: Announcements/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        // POST: Announcements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CinemaId,UserId,Title,Message,PublicationDate")] Announcement announcement)
        {
            ModelState.Remove(nameof(Announcement.Cinema));
            ModelState.Remove(nameof(Announcement.User));
            
            if (id != announcement.Id)
                
            {
                return NotFound();
            }
            
            var userId = await _userManager.GetUserIdAsync(await _userManager.GetUserAsync(User));
            announcement.UserId = Guid.Parse(userId);
            
            announcement.PublicationDate = DateTime.UtcNow;
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(announcement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnouncementExists(announcement.Id))
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
            ViewData["CinemaId"] = new SelectList(_context.Cinemas, "Id", "Id", announcement.CinemaId);
            return View(announcement);
        }

        // GET: Announcements/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements
                .Include(a => a.Cinema)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // POST: Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnnouncementExists(Guid id)
        {
            return _context.Announcements.Any(e => e.Id == id);
        }
    }
}
