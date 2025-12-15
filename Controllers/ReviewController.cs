using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using ProMeet.Data;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using ProMeet.Hubs;

namespace ProMeet.Controllers
{
    public class ReviewController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ReviewController(MongoDbContext context, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews.Find(_ => true).ToListAsync();
            return View(reviews);
        }

        public async Task<IActionResult> Details(int id)
        {
            var review = await _context.Reviews.Find(r => r.ReviewID == id).FirstOrDefaultAsync();
            if (review == null) return NotFound();
            return View(review);
        }

        public async Task<IActionResult> Create(string professionalId)
        {
            if (string.IsNullOrEmpty(professionalId)) return BadRequest("Professional ID is required");
            
            var professional = await _context.Professionals.Find(p => p.Id == professionalId).FirstOrDefaultAsync();
            if (professional == null) return NotFound("Professional not found");

            // Populate user info to show who we are reviewing
            if (professional.User != null)
            {
                var proUser = await _userManager.FindByIdAsync(professional.User.Id.ToString());
                if (proUser != null) professional.User = proUser;
            }

            ViewBag.ProfessionalName = professional.User?.Name ?? "Professional";
            ViewBag.ProfessionalId = professionalId;

            return View(new Review { ProfessionalID = professionalId });
        }

        [HttpPost]
        public async Task<IActionResult> Create(Review review)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Populate server-side fields before validation checks that matter
            review.ClientID = user.Id.ToString();
            
            // Remove ModelState errors for fields we set programmatically
            ModelState.Remove("ClientID");
            ModelState.Remove("ReviewID");
            ModelState.Remove("DateProvided");
            ModelState.Remove("AppointmentID"); 

            // Validate
            if (!ModelState.IsValid)
            {
                // Debug logging
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        Console.WriteLine($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }
                return View(review);
            }

            var count = await _context.Reviews.CountDocumentsAsync(_ => true);
            review.ReviewID = (int)count + 1;
            review.DateProvided = DateTime.UtcNow;

            await _context.Reviews.InsertOneAsync(review);
            
            // Update professional rating
            var reviews = await _context.Reviews.Find(r => r.ProfessionalID == review.ProfessionalID).ToListAsync();
            if (reviews.Any())
            {
                var newRating = (float)reviews.Average(r => r.Rating);
                var update = Builders<Professional>.Update.Set(p => p.Rating, newRating);
                await _context.Professionals.UpdateOneAsync(p => p.Id == review.ProfessionalID, update);
            }

            // Notify Professional
            var professional = await _context.Professionals.Find(p => p.Id == review.ProfessionalID).FirstOrDefaultAsync();
            if (professional != null && professional.User != null)
            {
                var notification = new Notification
                {
                    UserId = professional.User.Id.ToString(),
                    Title = "New Review Received",
                    Message = $"You received a {review.Rating}-star review from {user.Name}.",
                    Type = NotificationType.Review,
                    RelatedId = review.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Notifications.InsertOneAsync(notification);
                await _hubContext.Clients.User(professional.User.Id.ToString()).SendAsync("ReceiveNotification", notification.Message);
            }
            
            return RedirectToAction("Details", "Professional", new { id = review.ProfessionalID }, "reviews");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.Reviews.Find(r => r.ReviewID == id).FirstOrDefaultAsync();
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Review review)
        {
            var filter = Builders<Review>.Filter.Eq(r => r.ReviewID, review.ReviewID);
            await _context.Reviews.ReplaceOneAsync(filter, review);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.Find(r => r.ReviewID == id).FirstOrDefaultAsync();
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var review = await _context.Reviews.Find(r => r.ReviewID == id).FirstOrDefaultAsync();
            if (review == null) return NotFound();

            // Security check: Only allow author to delete
            if (review.ClientID != user.Id.ToString())
            {
                return Forbid();
            }

            var professionalId = review.ProfessionalID;

            await _context.Reviews.DeleteOneAsync(r => r.ReviewID == id);
            
            // Recalculate and update professional rating
            var reviews = await _context.Reviews.Find(r => r.ProfessionalID == professionalId).ToListAsync();
            float newRating = 0;
            if (reviews.Any())
            {
                newRating = (float)reviews.Average(r => r.Rating);
            }
            
            var update = Builders<Professional>.Update.Set(p => p.Rating, newRating);
            await _context.Professionals.UpdateOneAsync(p => p.Id == professionalId, update);

            return RedirectToAction("Details", "Professional", new { id = professionalId }, "reviews");
        }

        public async Task<IActionResult> List()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var professional = await _context.Professionals.Find(p => p.User != null && p.User.Id == user.Id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            var reviews = await _context.Reviews.Find(r => r.ProfessionalID == professional.Id).ToListAsync();
            
            foreach (var review in reviews)
            {
                if (!string.IsNullOrEmpty(review.ClientID))
                {
                    var client = await _userManager.FindByIdAsync(review.ClientID);
                    if (client == null)
                    {
                         // Fallback: try to find by Guid if stored as such
                         if (Guid.TryParse(review.ClientID, out Guid clientGuid))
                         {
                              client = _userManager.Users.FirstOrDefault(u => u.Id == clientGuid);
                         }
                    }
                    review.Client = client;
                }
            }
            
            return View(reviews);
        }
    }
}