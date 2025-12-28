using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProMeet.Models;
using ProMeet.ViewModels;
using System.Threading.Tasks;
using ProMeet.Data;
using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;

namespace ProMeet.Controllers
{
    /// Handles user authentication and account management (Login, Register, Logout, Profile).
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly MongoDbContext _context;

        public AccountController(MongoDbContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /Account/Login
        /// Displays the login page. Redirects to Home if already authenticated.
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        /// Processes the login attempt.
        /// <param name="model">Login credentials (email, password).</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {model.Email} logged in.");

                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("Professional"))
                        {
                            return RedirectToAction("Dashboard", "Professional");
                        }
                    }
                    
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }
            
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // GET: /Account/Register
        /// Displays the registration page.
        public IActionResult Register()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        /// Processes a new user registration.
        /// Creates a Professional profile if the user type is "Professional".
        /// <param name="model">Registration details.</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                {
                    UserName = model.Email, 
                    Email = model.Email, 
                    Name = $"{model.FirstName} {model.LastName}",
                    UserType = model.UserType,
                    ProfessionType = model.UserType == "Professional" ? model.ProfessionType : null,
                    OrganizationName = model.OrganizationName,
                    City = model.City,
                    Country = model.Country,
                    DateJoined = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    
                    // Assign role
                    if (model.UserType == "Professional")
                    {
                        if (!await _userManager.IsInRoleAsync(user, "Professional"))
                        {
                            await _userManager.AddToRoleAsync(user, "Professional");
                        }

                        // Create the Professional profile
                        var professional = new Professional
                        {
                            User = user, // Embed user info
                            Specialty = model.ProfessionType ?? "",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            ProfileActive = true,
                            IsValidated = false, // Default to unvalidated
                            Rating = 0,
                            Price = 0 // Default base price
                        };

                        await _context.Professionals.InsertOneAsync(professional);
                    }
                    else
                    {
                        if (!await _userManager.IsInRoleAsync(user, "Client"))
                        {
                            await _userManager.AddToRoleAsync(user, "Client");
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    if (model.UserType == "Professional")
                    {
                         return RedirectToAction("Dashboard", "Professional");
                    }
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: /Account/Logout
        /// Logs out the current user.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }
        
        // GET: /Account/Profile
        /// Displays the user's profile (mainly for Clients).
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }
            
            // If professional, redirect to manage profile
            if (User.IsInRole("Professional"))
            {
                 return RedirectToAction("ManageProfile", "Professional");
            }

            return View(user);
        }

        // POST: /Account/Profile
        /// Updates the user's profile information.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                user.Name = model.Name;
                user.Phone = model.Phone;
                user.City = model.City;
                user.Country = model.Country;
                user.OrganizationName = model.OrganizationName;
                user.Birthday = model.Birthday;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["StatusMessage"] = "Profile updated successfully";
                    return RedirectToAction(nameof(Profile));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: /Account/Security
        /// Displays the change password page.
        [Authorize]
        public IActionResult Security()
        {
            return View();
        }

        // POST: /Account/Security
        /// Processes the password change request.
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Security(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Your password has been changed.";
            return RedirectToAction("Security");
        }
    }
}
