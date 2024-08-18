using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using cinema_web_app.Models;
using cinema_web_app.Data;
using Microsoft.EntityFrameworkCore;

namespace cinema_web_app.Controllers
{
    [Authorize(Roles = "ApplicationAdmin")]
    public class ApplicationAdminsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _context;

        public ApplicationAdminsController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: ApplicationAdmin/UsersByRole
        public async Task<IActionResult> UsersByRole(string role = "Customer")
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserViewModel>();
            bool showCinemaColumn = role == "ContentCinemaAdmin"; // Check if the role is ContentCinemaAdmin

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains(role))
                {
                    var contentCinemaAdmins = showCinemaColumn
                        ? await _context.ContentCinemaAdmins
                            .Where(ca => ca.UserId == user.Id)
                            .Include(ca => ca.Cinema)
                            .ToListAsync()
                        : new List<ContentCinemaAdmin>();

                    var cinemaNames = showCinemaColumn
                        ? contentCinemaAdmins.Select(ca => ca.Cinema.Name).ToList()
                        : new List<string>();

                    var thisViewModel = new UserViewModel
                    {
                        UserId = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = roles,
                        CinemaNames = cinemaNames
                    };
                    userRolesViewModel.Add(thisViewModel);
                }
            }

            ViewData["Role"] = role;
            ViewData["ShowCinemaColumn"] = showCinemaColumn; // Pass the flag to the view
            return View(userRolesViewModel);
        }



        // GET: ApplicationAdmin/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var cinemaNames = new List<string>();

            if (roles.Contains("ContentCinemaAdmin"))
            {
                var contentCinemaAdmins = await _context.ContentCinemaAdmins
                    .Where(ca => ca.UserId == user.Id)
                    .Include(ca => ca.Cinema)
                    .ToListAsync();

                cinemaNames = contentCinemaAdmins.Select(ca => ca.Cinema.Name).ToList();
            }

            var model = new UserViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles,
                CinemaNames = cinemaNames
            };

            return View(model);
        }

        // GET: ApplicationAdmin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ApplicationAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,Password,ConfirmPassword,Role")] CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: ApplicationAdmins/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            var cinemas = await _context.Cinemas.ToListAsync();
            var contentCinemaAdmin = await _context.ContentCinemaAdmins.FirstOrDefaultAsync(ca => ca.UserId == user.Id);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CinemaId = contentCinemaAdmin?.CinemaId // Set the selected cinema ID if the user is a ContentCinemaAdmin
            };

            ViewBag.Cinemas = cinemas;
            return View(model);
        }

        // POST: ApplicationAdmins/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FirstName,LastName,Email,CinemaId")] EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id.ToString());
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Retrieve the role of the user to redirect correctly
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = roles.FirstOrDefault(); // Assuming a single role per user, adjust if necessary

                    if (role == "ContentCinemaAdmin")
                    {
                        var contentCinemaAdmin = await _context.ContentCinemaAdmins.FirstOrDefaultAsync(ca => ca.UserId == user.Id);
                        if (contentCinemaAdmin != null)
                        {
                            contentCinemaAdmin.CinemaId = model.CinemaId.Value;
                            _context.Update(contentCinemaAdmin);
                            await _context.SaveChangesAsync();
                        }
                    }

                    return RedirectToAction(nameof(UsersByRole), new { role });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Repopulate the cinemas list in case of an error
            ViewBag.Cinemas = await _context.Cinemas.ToListAsync();
            return View(model);
        }
        
        // POST: ApplicationAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, string role)
        {
            // Get the currently signed-in user
            var currentUser = await _userManager.GetUserAsync(User);

            // Check if the user to be deleted is the currently signed-in user
            if (currentUser != null && currentUser.Id == id)
            {
                ModelState.AddModelError(string.Empty, "You cannot delete your own account while signed in.");
                return RedirectToAction(nameof(UsersByRole), new { role });
            }

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(UsersByRole), new { role });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return RedirectToAction(nameof(UsersByRole), new { role });
        }
    }
}
