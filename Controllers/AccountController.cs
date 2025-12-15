using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using ProMeet.ViewModels;
using System.Threading.Tasks;
using ProMeet.Data;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MongoDB.Driver;

namespace ProMeet.Controllers
{
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
        public IActionResult Login()
        {
            // If user is already logged in, redirect to home
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
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
        public IActionResult Register()
        {
            // If user is already logged in, redirect to home
            if (User?.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
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
                    Phone = model.Phone,
                    Birthday = model.Birthday
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {model.Email} created a new account with password.");

                    // Assign role to user
                    await _userManager.AddToRoleAsync(user, model.UserType);

                    if (model.UserType == "Professional")
                    {
                        var professional = new Professional
                        {
                            User = user,
                            Specialty = model.ProfessionType ?? ""
                        };
                        await _context.Professionals.InsertOneAsync(professional);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ApplicationUser model, string firstName, string lastName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            // Note: UserName changes might require re-login or security checks, but we'll allow it for now if that's the requirement.
            // Usually UserName is not editable or handled separately.
            if (model.UserName != user.UserName)
            {
                var userNameExists = await _userManager.FindByNameAsync(model.UserName);
                if (userNameExists != null && userNameExists.Id != user.Id)
                {
                    ModelState.AddModelError("UserName", "Username is already taken.");
                    return View(user);
                }
                user.UserName = model.UserName;
            }

            user.Name = $"{firstName} {lastName}".Trim();
            user.OrganizationName = model.OrganizationName;
            user.City = model.City;
            user.Country = model.Country;
            user.Phone = model.Phone;
            user.Birthday = model.Birthday;
            
            // Email is usually not updated here or requires verification, but if requested:
            if (model.Email != user.Email)
            {
                 var emailExists = await _userManager.FindByEmailAsync(model.Email);
                 if (emailExists != null && emailExists.Id != user.Id)
                 {
                     ModelState.AddModelError("Email", "Email is already taken.");
                     return View(user);
                 }
                 user.Email = model.Email;
            }

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Sync with Professional collection if user is a professional
                if (user.UserType == "Professional")
                {
                    var updateFilter = Builders<Professional>.Filter.Eq("User._id", user.Id);
                    var updateDefinition = Builders<Professional>.Update.Set(p => p.User, user);
                    await _context.Professionals.UpdateOneAsync(updateFilter, updateDefinition);
                }

                TempData["StatusMessage"] = "Profile updated successfully";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        // POST: /Account/UploadProfilePhoto
        [HttpPost]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                {
                    return Json(new { success = false, message = "No file uploaded" });
                }

                // Validate file type
                var validTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
                if (!validTypes.Contains(photo.ContentType))
                {
                    return Json(new { success = false, message = "Invalid file type. Only JPEG, JPG, and PNG are allowed." });
                }

                // Validate file size (max 5MB)
                if (photo.Length > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "File size must be less than 5MB." });
                }

                // Create images directory if it doesn't exist
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileName = $"profile_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // Generate the URL for the saved image
                var photoUrl = $"/images/profiles/{fileName}";

                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    user.PhotoURL = photoUrl;
                    await _userManager.UpdateAsync(user);

                    // Sync with Professional collection if user is a professional
                    if (user.UserType == "Professional")
                    {
                        var updateFilter = Builders<Professional>.Filter.Eq("User._id", user.Id);
                        var updateDefinition = Builders<Professional>.Update.Set(p => p.User, user);
                        await _context.Professionals.UpdateOneAsync(updateFilter, updateDefinition);
                    }
                }

                return Json(new { success = true, photoUrl = photoUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile photo");
                return Json(new { success = false, message = "Error uploading photo. Please try again." });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // View Models for Login and Registration
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email address is too long")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can only contain letters and spaces")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last name can only contain letters and spaces")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a user type")]
        [Display(Name = "I am a")]
        public string UserType { get; set; } = string.Empty;

        [Display(Name = "Type of Profession")]
        public string? ProfessionType { get; set; }

        [Display(Name = "Organization Name")]
        public string? OrganizationName { get; set; }

        [Required(ErrorMessage = "City is required")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public string Country { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birthday is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Birthday")]
        public DateTime? Birthday { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email address is too long")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "You must agree to the terms and conditions")]
        [Display(Name = "I agree to the terms and conditions")]
        public bool Terms { get; set; }
    }
}