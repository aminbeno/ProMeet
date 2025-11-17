using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class ProfessionalController : Controller
    {
        private static List<Professional> _professionals = new List<Professional>
        {
            new Professional 
            { 
                ProfessionalID = 1, 
                UserID = 1, 
                JobTitle = "Software Engineer", 
                Specialty = "Web Development", 
                Bio = "Experienced software engineer with expertise in web development and modern frameworks.", 
                Experience = "5 years", 
                Degrees = "Bachelor of Computer Science", 
                ConsultationType = "Online", 
                Price = 100, 
                IsValidated = true, 
                Rating = 4.8f, 
                ProfileActive = true, 
                CategoryID = 1,
                User = new User 
                { 
                    UserID = 1, 
                    Name = "John Doe", 
                    Email = "john@example.com", 
                    PhotoURL = "https://via.placeholder.com/300x200?text=John+Doe",
                    City = "New York"
                }
            },
            new Professional 
            { 
                ProfessionalID = 2, 
                UserID = 2, 
                JobTitle = "Data Scientist", 
                Specialty = "Machine Learning", 
                Bio = "Data scientist with expertise in machine learning and artificial intelligence.", 
                Experience = "7 years", 
                Degrees = "PhD in Computer Science", 
                ConsultationType = "Online", 
                Price = 150, 
                IsValidated = true, 
                Rating = 4.9f, 
                ProfileActive = true, 
                CategoryID = 2,
                User = new User 
                { 
                    UserID = 2, 
                    Name = "Jane Smith", 
                    Email = "jane@example.com", 
                    PhotoURL = "https://via.placeholder.com/300x200?text=Jane+Smith",
                    City = "San Francisco"
                }
            }
        };

        public IActionResult Index()
        {
            return View(_professionals);
        }

        public IActionResult Details(int id)
        {
            var professional = _professionals.FirstOrDefault(p => p.ProfessionalID == id);
            if (professional == null) return NotFound();
            return View(professional);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Professional professional)
        {
            professional.ProfessionalID = _professionals.Count > 0 ? _professionals.Max(p => p.ProfessionalID) + 1 : 1;
            _professionals.Add(professional);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var professional = _professionals.FirstOrDefault(p => p.ProfessionalID == id);
            if (professional == null) return NotFound();
            return View(professional);
        }

        [HttpPost]
        public IActionResult Edit(Professional professional)
        {
            var existing = _professionals.FirstOrDefault(p => p.ProfessionalID == professional.ProfessionalID);
            if (existing == null) return NotFound();
            existing.UserID = professional.UserID;
            existing.JobTitle = professional.JobTitle;
            existing.Specialty = professional.Specialty;
            existing.Bio = professional.Bio;
            existing.Experience = professional.Experience;
            existing.Degrees = professional.Degrees;
            existing.ConsultationType = professional.ConsultationType;
            existing.Price = professional.Price;
            existing.IsValidated = professional.IsValidated;
            existing.Rating = professional.Rating;
            existing.ProfileActive = professional.ProfileActive;
            existing.CategoryID = professional.CategoryID;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var professional = _professionals.FirstOrDefault(p => p.ProfessionalID == id);
            if (professional == null) return NotFound();
            return View(professional);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var professional = _professionals.FirstOrDefault(p => p.ProfessionalID == id);
            if (professional != null) _professionals.Remove(professional);
            return RedirectToAction("Index");
        }

        public static List<Professional> GetProfessionals()
        {
            return _professionals;
        }

        // GET: /Professional/Search
        public IActionResult Search(string? query, string? category, string? city, string? sortBy, decimal? maxPrice)
        {
            var professionals = _professionals.AsQueryable();

            // Filter by search query
            if (!string.IsNullOrEmpty(query))
            {
                professionals = professionals.Where(p => 
                    p.User.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.JobTitle.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.Specialty.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    p.Bio.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by category
            if (!string.IsNullOrEmpty(category))
            {
                professionals = professionals.Where(p => 
                    p.Specialty.Contains(category, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by city
            if (!string.IsNullOrEmpty(city))
            {
                professionals = professionals.Where(p => 
                    p.User.City.Contains(city, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by max price
            if (maxPrice.HasValue)
            {
                professionals = professionals.Where(p => p.Price <= maxPrice.Value);
            }

            // Sort results
            professionals = sortBy?.ToLower() switch
            {
                "rating" => professionals.OrderByDescending(p => p.Rating),
                "price-low" => professionals.OrderBy(p => p.Price),
                "price-high" => professionals.OrderByDescending(p => p.Price),
                "experience" => professionals.OrderByDescending(p => p.Experience),
                _ => professionals.OrderBy(p => p.User.Name)
            };

            var viewModel = new ProfessionalSearchViewModel
            {
                Professionals = professionals.ToList(),
                SearchQuery = query,
                SelectedCategory = category,
                SelectedCity = city,
                SelectedSortBy = sortBy,
                MaxPrice = maxPrice,
                TotalResults = professionals.Count()
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
        public decimal? MaxPrice { get; set; }
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