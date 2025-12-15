using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using ProMeet.Data;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ProMeet.ViewModels;

namespace ProMeet.Controllers
{
    [Authorize(Roles = "Professional")]
    public class ProfessionalController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfessionalController(MongoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var professional = await _context.Professionals
                .Find(p => p.User != null && p.User.Id == user.Id)
                .FirstOrDefaultAsync();

            if (professional == null)
            {
                return NotFound();
            }
            
            // Ensure we have the latest user info
            professional.User = user;

            var appointments = await _context.Appointments
                .Find(a => a.ProfessionalID == professional.Id)
                .ToListAsync();

            var upcomingAppointmentsCount = appointments
                .Count(a => (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed) && a.Date >= DateTime.Today);

            var totalClients = appointments
                .Select(a => a.ClientID)
                .Distinct()
                .Count();

            var reviews = await _context.Reviews
                .Find(r => r.ProfessionalID == professional.Id)
                .ToListAsync();
            
            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            var reviewsCount = reviews.Count;

            var recentAppointments = appointments.OrderByDescending(a => a.Date).Take(5).ToList();

            foreach (var appointment in recentAppointments)
            {
                if (appointment.Client == null)
                {
                    appointment.Client = await _userManager.FindByIdAsync(appointment.ClientID);
                }
            }

            var viewModel = new ProfessionalDashboardViewModel
            {
                Professional = professional,
                UpcomingAppointmentsCount = upcomingAppointmentsCount,
                TotalClients = totalClients,
                AverageRating = averageRating,
                ReviewsCount = reviewsCount,
                RecentAppointments = recentAppointments
            };

            return View(viewModel);
        }


        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var professionals = await _context.Professionals.Find(_ => true).ToListAsync();
            
            // Populate User info and Services for each professional
            foreach (var p in professionals)
            {
                if (p.User != null)
                {
                    var user = await _userManager.FindByIdAsync(p.User.Id.ToString());
                    if (user != null) p.User = user;
                }
                
                // Populate Services
                p.Services = await _context.Services.Find(s => s.ProfessionalID == p.Id).ToListAsync();
            }
            
            return View(professionals);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            var professional = await _context.Professionals.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();
            
            // Populate User info
            if (professional.User != null)
            {
                var user = await _userManager.FindByIdAsync(professional.User.Id.ToString());
                if (user != null) professional.User = user;
            }

            // Populate Reviews
            var reviews = await _context.Reviews
                .Find(r => r.ProfessionalID == professional.Id)
                .SortByDescending(r => r.DateProvided)
                .ToListAsync();
            
            // Populate Services
            var services = await _context.Services
                .Find(s => s.ProfessionalID == professional.Id)
                .ToListAsync();
            professional.Services = services;
            
            foreach (var review in reviews)
            {
                if (!string.IsNullOrEmpty(review.ClientID))
                {
                    var client = await _userManager.FindByIdAsync(review.ClientID);
                    
                    // Fallback: If FindByIdAsync fails, try manual lookup via Guid
                    if (client == null && Guid.TryParse(review.ClientID, out var guidId))
                    {
                        client = _userManager.Users.FirstOrDefault(u => u.Id == guidId);
                    }
                    
                    review.Client = client;
                }
            }

            professional.Reviews = reviews;

            // Calculate Rating for display
            if (reviews.Any())
            {
                professional.Rating = (float)reviews.Average(r => r.Rating);
            }
            else
            {
                professional.Rating = 0;
            }
            
            return View(professional);
        }

        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var professional = await _context.Professionals
                .Find(p => p.User != null && p.User.Id == user.Id)
                .FirstOrDefaultAsync();

            if (professional == null) return NotFound();

            var appointments = await _context.Appointments
                .Find(a => a.ProfessionalID == professional.Id)
                .ToListAsync();

            // Populate Client details for each appointment
            foreach (var appointment in appointments)
            {
                if (!string.IsNullOrEmpty(appointment.ClientID))
                {
                    appointment.Client = await _userManager.FindByIdAsync(appointment.ClientID);
                }
            }

            return View(appointments);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Professional professional)
        {
            await _context.Professionals.InsertOneAsync(professional);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string id)
        {
            var professional = await _context.Professionals.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();
            return View(professional);
        }

        public async Task<IActionResult> ManageProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var professional = await _context.Professionals
                .Find(p => p.User != null && p.User.Id == user.Id)
                .FirstOrDefaultAsync();

            if (professional == null)
            {
                return NotFound();
            }

            return View(professional);
        }

        [HttpPost]
        public async Task<IActionResult> ManageProfile(Professional professional)
        {
            var filter = Builders<Professional>.Filter.Eq(p => p.Id, professional.Id);
            var existingProfessional = await _context.Professionals.Find(filter).FirstOrDefaultAsync();
            
            if (existingProfessional == null)
            {
                return NotFound();
            }

            // Update Professional fields
            existingProfessional.Specialty = professional.Specialty;
            existingProfessional.Experience = professional.Experience;
            
            // Update embedded User fields if User is not null
            if (professional.User != null && existingProfessional.User != null)
            {
                existingProfessional.User.Name = professional.User.Name;
                existingProfessional.User.City = professional.User.City;
                existingProfessional.User.Phone = professional.User.Phone;
                existingProfessional.User.Email = professional.User.Email;
                
                // Update the actual User in Identity collection
                var user = await _userManager.FindByIdAsync(existingProfessional.User.Id.ToString());
                if (user != null)
                {
                    user.Name = professional.User.Name;
                    user.City = professional.User.City;
                    user.Phone = professional.User.Phone;
                    
                    await _userManager.UpdateAsync(user);
                }
            }

            await _context.Professionals.ReplaceOneAsync(filter, existingProfessional);
            
            // Pass a status message
            TempData["StatusMessage"] = "Profile updated successfully";
            
            return RedirectToAction("ManageProfile");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Professional professional)
        {
            var filter = Builders<Professional>.Filter.Eq(p => p.Id, professional.Id);
            await _context.Professionals.ReplaceOneAsync(filter, professional);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(string id)
        {
            var professional = await _context.Professionals.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();
            return View(professional);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _context.Professionals.DeleteOneAsync(p => p.Id == id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ManageServices()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var professional = await _context.Professionals.Find(p => p.User != null && p.User.Id == user.Id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            var services = await _context.Services.Find(s => s.ProfessionalID == professional.Id).ToListAsync();
            return View(services);
        }

        public IActionResult CreateService()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateService(Service service)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var professional = await _context.Professionals.Find(p => p.User != null && p.User.Id == user.Id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            service.ProfessionalID = professional.Id;
            var count = await _context.Services.CountDocumentsAsync(_ => true);
            service.ServiceID = (int)count + 1;

            await _context.Services.InsertOneAsync(service);
            return RedirectToAction("ManageServices");
        }

        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services.Find(s => s.ServiceID == id).FirstOrDefaultAsync();
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> EditService(Service service)
        {
            var filter = Builders<Service>.Filter.Eq(s => s.ServiceID, service.ServiceID);
            await _context.Services.ReplaceOneAsync(filter, service);
            return RedirectToAction("ManageServices");
        }

        public async Task<IActionResult> ServiceDetails(int id)
        {
            var service = await _context.Services.Find(s => s.ServiceID == id).FirstOrDefaultAsync();
            if (service == null) return NotFound();
            return View(service);
        }

        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.Find(s => s.ServiceID == id).FirstOrDefaultAsync();
            if (service == null) return NotFound();
            return View(service);
        }

        [HttpPost, ActionName("DeleteService")]
        public async Task<IActionResult> DeleteServiceConfirmed(int id)
        {
            await _context.Services.DeleteOneAsync(s => s.ServiceID == id);
            return RedirectToAction("ManageServices");
        }



        [AllowAnonymous]
        // GET: /Professional/Search
        public async Task<IActionResult> Search(string? query, string? category, string? city, string? sortBy, double? maxPrice)
        {
            // 1. Fetch all professionals
            var professionals = await _context.Professionals.Find(_ => true).ToListAsync();

            // 2. Populate User info for each professional
            foreach (var p in professionals)
            {
                if (p.User != null)
                {
                    var user = await _userManager.FindByIdAsync(p.User.Id.ToString());
                    if (user != null) p.User = user;
                }
                
                // Populate Services
                p.Services = await _context.Services.Find(s => s.ProfessionalID == p.Id).ToListAsync();
            }

            // 3. Filter in memory
            var filteredList = professionals.AsEnumerable();

            // Filter by search query
            if (!string.IsNullOrEmpty(query))
            {
                filteredList = filteredList.Where(p => 
                    (p.User != null && p.User.Name != null && p.User.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.JobTitle != null && p.JobTitle.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.Specialty != null && p.Specialty.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (p.Bio != null && p.Bio.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                filteredList = filteredList.Where(p => 
                    p.Specialty != null && p.Specialty.IndexOf(category, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Filter by city
            if (!string.IsNullOrEmpty(city))
            {
                filteredList = filteredList.Where(p => 
                    p.User != null && p.User.City != null && p.User.City.IndexOf(city, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            // Filter by max price
            if (maxPrice.HasValue)
            {
                filteredList = filteredList.Where(p => p.Price <= maxPrice.Value);
            }

            // Sort results
            switch (sortBy?.ToLower())
            {
                case "rating":
                    filteredList = filteredList.OrderByDescending(p => p.Rating);
                    break;
                case "price-low":
                    filteredList = filteredList.OrderBy(p => p.Price);
                    break;
                case "price-high":
                    filteredList = filteredList.OrderByDescending(p => p.Price);
                    break;
                case "experience":
                    filteredList = filteredList.OrderByDescending(p => p.Experience);
                    break;
                default:
                    filteredList = filteredList.OrderBy(p => (p.User != null && p.User.Name != null) ? p.User.Name : "");
                    break;
            }

            var finalProfessionals = filteredList.ToList();

            var viewModel = new ProfessionalSearchViewModel
            {
                Professionals = finalProfessionals,
                SearchQuery = query,
                SelectedCategory = category,
                SelectedCity = city,
                SelectedSortBy = sortBy,
                MaxPrice = maxPrice,
                TotalResults = finalProfessionals.Count
            };

            return View(viewModel);
        }
    }

    // View Model for Professional Search Results
    public class ProfessionalSearchViewModel
    {
        public List<Professional> Professionals { get; set; } = new List<Professional>();
        public string? SearchQuery { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedCity { get; set; }
        public string? SelectedSortBy { get; set; }
        public double? MaxPrice { get; set; }
        public int TotalResults { get; set; }
        
        // Filter options for the view
        public List<string> Categories { get; set; } = new List<string> 
        { 
            "Web Development", "Machine Learning", "Data Science", "Mobile Development", 
            "UI/UX Design", "DevOps", "Cybersecurity", "Database Administration" 
        };
        
        public List<string> Cities { get; set; } = new List<string> 
        { 
            "New York", "San Francisco", "Los Angeles", "Chicago", "Boston", "Seattle", "Austin" 
        };
        
        public List<string> SortOptions { get; set; } = new List<string> 
        { 
            "name", "rating", "price-low", "price-high", "experience" 
        };
    }
}