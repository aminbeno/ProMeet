using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using ProMeet.Data;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using ProMeet.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace ProMeet.Controllers
{
    /// <summary>
    /// Manages appointment booking, details, and listing.
    /// </summary>
    public class AppointmentController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AppointmentController(MongoDbContext context, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // GET: /Appointment/Index
        /// <summary>
        /// Main entry point for appointments. Currently redirects to Client appointments view.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Redirect to the client-specific appointments list
            return RedirectToAction("Appointments", "Client");
        }

        // GET: /Appointment/Details/{id}
        /// <summary>
        /// Displays details of a specific appointment.
        /// Supports looking up by both legacy Integer ID and MongoDB ObjectId string.
        /// </summary>
        /// <param name="id">Appointment ID (int or string).</param>
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            Appointment appointment = null;
            
            // Try finding by AppointmentID (int) first if id is parseable
            if (int.TryParse(id, out int appId))
            {
                appointment = await _context.Appointments.Find(a => a.AppointmentID == appId).FirstOrDefaultAsync();
            }

            // If not found or not int, try finding by Id (string/ObjectId)
            if (appointment == null)
            {
                appointment = await _context.Appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
            }

            if (appointment == null) return NotFound();
            
            // Populate Pro and Client details if needed
             if (appointment.Professional == null && !string.IsNullOrEmpty(appointment.ProfessionalID))
            {
                appointment.Professional = await _context.Professionals.Find(p => p.Id == appointment.ProfessionalID).FirstOrDefaultAsync();
            }

            if (appointment.Professional != null && appointment.Professional.User == null)
            {
                // Attempt to recover user if possible, or handle gracefully
            }
            // If User is present but incomplete (e.g. just ID), refresh it
             if (appointment.Professional != null && appointment.Professional.User != null)
            {
                 var user = await _userManager.FindByIdAsync(appointment.Professional.User.Id.ToString());
                 if (user != null) appointment.Professional.User = user;
            }

            return View(appointment);
        }

        // GET: /Appointment/Book
        /// <summary>
        /// Displays the booking form for a specific professional and optional service.
        /// Calculates available slots based on professional's availability.
        /// </summary>
        /// <param name="professionalId">ID of the professional.</param>
        /// <param name="serviceId">Optional ID of the service.</param>
        [HttpGet]
        public async Task<IActionResult> Book(string professionalId, string? serviceId = null)
        {
            var professional = await _context.Professionals.Find(p => p.Id == professionalId).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            // Ensure User is loaded
            if (professional.User != null)
            {
                 var user = await _userManager.FindByIdAsync(professional.User.Id.ToString());
                 if (user != null) professional.User = user;
            }

            // Calculate OffDays
            // 1. Get default availabilities
            var availabilities = await _context.Availabilities
                .Find(a => a.ProfessionalID == professionalId && a.Date == null)
                .ToListAsync();

            // 2. Determine which days are available (IsAvailable = true)
            // ... Logic continues in View or here ... 
            // Note: The rest of the logic seems to be handled in the View or JS, 
            // but we pass the professional and service info.

            ViewBag.ServiceId = serviceId;
            return View(professional);
        }
        
        // POST: /Appointment/Book
        /// <summary>
        /// Processes the booking request.
        /// Creates a new appointment with status 'Pending'.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Book(string professionalId, DateTime date, TimeSpan time, string? serviceId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var professional = await _context.Professionals.Find(p => p.Id == professionalId).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            decimal price = professional.Price;
            string serviceName = "General Consultation";

            if (!string.IsNullOrEmpty(serviceId))
            {
                // Try finding service by ServiceID (int) first (legacy?) or Id (string)
                // Assuming ServiceID is int based on model, but parameter might be passed as string
                if (int.TryParse(serviceId, out int sId))
                {
                     var service = await _context.Services.Find(s => s.ServiceID == sId).FirstOrDefaultAsync();
                     if (service != null)
                     {
                         price = service.Price;
                         serviceName = service.Name;
                     }
                }
                else
                {
                    // Try by string Id
                     var service = await _context.Services.Find(s => s.Id == serviceId).FirstOrDefaultAsync();
                     if (service != null)
                     {
                         price = service.Price;
                         serviceName = service.Name;
                     }
                }
            }

            var appointment = new Appointment
            {
                ClientID = user.Id.ToString(),
                ProfessionalID = professionalId,
                ServiceID = serviceId,
                ServiceName = serviceName,
                Date = date,
                StartTime = time,
                EndTime = time.Add(TimeSpan.FromHours(1)), // Default 1 hour duration
                Price = price,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Appointments.InsertOneAsync(appointment);

            // Send notification to Professional
            // TODO: Implement real-time notification via SignalR
            
            return RedirectToAction("Appointments", "Client");
        }
    }
}
