using Microsoft.AspNetCore.Mvc;
using ProMeet.Data;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Driver;

namespace ProMeet.Controllers
{
    public class ServiceController : Controller
    {
        private readonly MongoDbContext _context;

        public ServiceController(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Search(string query)
        {
            var services = await _context.Services.Find(_ => true).ToListAsync();

            if (!string.IsNullOrEmpty(query))
            {
                services = services.Where(s => s.Name.Contains(query, System.StringComparison.OrdinalIgnoreCase) || 
                                               s.Description.Contains(query, System.StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(services);
        }
    }
}