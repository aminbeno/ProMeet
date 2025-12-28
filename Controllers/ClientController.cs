using Microsoft.AspNetCore.Mvc;
using ProMeet.Data;
using ProMeet.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using MongoDB.Driver;

namespace ProMeet.Controllers
{
    /// <summary>
    /// Manages client-specific functionalities such as viewing their appointments.
    /// </summary>
    public class ClientController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientController(MongoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays the list of appointments for the currently logged-in client.
        /// </summary>
        public async Task<IActionResult> Appointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var appointments = await _context.Appointments.Find(a => a.ClientID == user.Id.ToString()).ToListAsync();
            
            // Populate Professional details for each appointment
            foreach (var appointment in appointments)
            {
                if (!string.IsNullOrEmpty(appointment.ProfessionalID))
                {
                    appointment.Professional = await _context.Professionals.Find(p => p.Id == appointment.ProfessionalID).FirstOrDefaultAsync();
                    
                    if (appointment.Professional != null && appointment.Professional.User != null)
                    {
                        var proUser = await _userManager.FindByIdAsync(appointment.Professional.User.Id.ToString());
                        if (proUser != null) appointment.Professional.User = proUser;
                    }
                }
            }
            
            return View(appointments);
        }
    }
}