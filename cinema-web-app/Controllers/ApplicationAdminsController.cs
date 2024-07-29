﻿using cinema_web_app.Data;
using Microsoft.AspNetCore.Mvc;

namespace cinema_web_app.Controllers
{
    public class ApplicationAdminsController : Controller
    {
        private readonly ApplicationDbContext _context;
        
        public ApplicationAdminsController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
