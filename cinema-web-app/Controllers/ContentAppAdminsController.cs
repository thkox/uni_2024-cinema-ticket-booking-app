using cinema_web_app.Data;
using Microsoft.AspNetCore.Mvc;

namespace cinema_web_app.Controllers
{
    public class ContentAppAdminsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContentAppAdminsController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
