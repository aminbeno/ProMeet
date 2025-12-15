using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProMeet.Models;
using ProMeet.Data;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace ProMeet.Controllers
{
    public class NotificationController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(MongoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Find(n => n.UserId == user.Id.ToString())
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            await _context.Notifications.UpdateOneAsync(n => n.Id == id, update);
            return Ok();
        }
        
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
            await _context.Notifications.UpdateManyAsync(n => n.UserId == user.Id.ToString() && !n.IsRead, update);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Ok(0);

            var count = await _context.Notifications
                .CountDocumentsAsync(n => n.UserId == user.Id.ToString() && !n.IsRead);

            return Ok(count);
        }
    }
}
