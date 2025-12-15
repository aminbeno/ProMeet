using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Diagnostics;
using ProMeet.Data;
using MongoDB.Driver;
using ProMeet.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ProMeet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, MongoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeViewModel();

            // 1. Featured Professionals (Top rated)
            viewModel.FeaturedProfessionals = await _context.Professionals
                .Find(p => p.ProfileActive)
                .SortByDescending(p => p.Rating)
                .Limit(4)
                .ToListAsync();
            
            // Populate User info if missing (though it should be embedded)
            foreach (var pro in viewModel.FeaturedProfessionals)
            {
                if (pro.User != null)
                {
                    var user = await _userManager.FindByIdAsync(pro.User.Id.ToString());
                    if (user != null) pro.User = user;
                }
            }

            // 2. Stats
            viewModel.TotalProfessionals = (int)await _context.Professionals.CountDocumentsAsync(p => p.ProfileActive);
            
            var usersCollection = _context.Database.GetCollection<ApplicationUser>("Users");
            viewModel.TotalClients = (int)await usersCollection.CountDocumentsAsync(u => u.UserType == "Client");

            var ratings = await _context.Professionals.Find(p => p.ProfileActive).Project(p => p.Rating).ToListAsync();
            viewModel.AverageRating = ratings.Any() ? Math.Round(ratings.Average(), 1) : 0;

            // 3. Recent Reviews
            viewModel.RecentReviews = await _context.Reviews
                .Find(_ => true)
                .SortByDescending(r => r.CreatedAt)
                .Limit(3)
                .ToListAsync();

            // Populate Client info for reviews
            foreach (var review in viewModel.RecentReviews)
            {
                if (!string.IsNullOrEmpty(review.ClientID))
                {
                    review.Client = await _userManager.FindByIdAsync(review.ClientID);
                }
            }

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Services()
        {
            var services = await _context.Services.Find(_ => true).ToListAsync();
            return View(services);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}