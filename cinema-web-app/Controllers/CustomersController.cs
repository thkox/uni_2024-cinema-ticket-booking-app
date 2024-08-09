using cinema_web_app.Data;
using Microsoft.AspNetCore.Mvc;

namespace cinema_web_app.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            return View();
        }
    }
}
