using System.Diagnostics;
using cinema_web_app.Data;
using cinema_web_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace cinema_web_app.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    // GET: Home/Index
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var playingNowMovies = await _context.Movies
            .Include(m => m.Genre)
            .Where(m => m.ReleaseDate <= today)
            .ToListAsync();

        var announcements = await _context.Announcements
            .Include(a => a.Cinema)
            .ToListAsync();

        var model = new Tuple<IEnumerable<Movie>, IEnumerable<Announcement>>(playingNowMovies, announcements);
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}