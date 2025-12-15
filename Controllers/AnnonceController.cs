using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProMeet.Data;
using ProMeet.Models;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Security.Claims;

namespace ProMeet.Controllers
{
    [Authorize(Roles = "Professional")]
    public class AnnonceController : Controller
    {
        private readonly MongoDbContext _context;

        public AnnonceController(MongoDbContext context)
        {
            _context = context;
        }

        // GET: Annonce
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var annonces = await _context.Annonces.Find(a => a.ProfessionalId == userId).ToListAsync();
            return View(annonces);
        }

        // GET: Annonce/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Annonce/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Category,Specialty,Description,YearsOfExperience,Location,Availability")] Annonce annonce)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                annonce.ProfessionalId = userId;
                await _context.Annonces.InsertOneAsync(annonce);
                return RedirectToAction(nameof(Index));
            }
            return View(annonce);
        }
    }
}