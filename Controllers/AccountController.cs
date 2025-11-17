using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace ProMeet.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
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
                // TODO: Implement actual authentication logic
                // For now, we'll simulate a successful login
                _logger.LogInformation($"Login attempt for email: {model.Email}");
                
                // Redirect to home page after successful login
                return RedirectToAction("Index", "Home");
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
                // TODO: Implement actual registration logic
                // For now, we'll simulate a successful registration
                _logger.LogInformation($"Registration attempt for email: {model.Email}");
                
                // Redirect to login page after successful registration
                return RedirectToAction("Login", "Account");
            }
            
            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // TODO: Implement actual logout logic
            _logger.LogInformation("User logged out");
            
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        public IActionResult Profile()
        {
            return View();
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

                // TODO: Update user's PhotoURL in database
                // For now, we'll just return the URL

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