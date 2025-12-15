using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using ProMeet.Data;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using ProMeet.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;
using ProMeet.Hubs;

namespace ProMeet.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // If user is pro, show pro appointments? Or client appointments?
            // Assuming this is general list, maybe redirect to Client/Appointments or Professional/Dashboard
            return RedirectToAction("Appointments", "Client");
        }

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
                // Cannot use ProfessionalID (int) to find user. 
                // If User is missing, we might need to rely on embedded data being there.
            }
            // If User is present but incomplete (e.g. just ID), refresh it
             if (appointment.Professional != null && appointment.Professional.User != null)
            {
                 var user = await _userManager.FindByIdAsync(appointment.Professional.User.Id.ToString());
                 if (user != null) appointment.Professional.User = user;
            }

            return View(appointment);
        }

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
            var availableDays = availabilities
                .Where(a => a.IsAvailable)
                .Select(a => a.DayOfWeek)
                .ToHashSet();

            // 3. OffDays are all days (0-6) that are NOT in availableDays
            var allDays = Enumerable.Range(0, 7).ToList();
            var offDays = allDays.Where(d => !availableDays.Contains(d)).ToList();

            // 4. Get specific dates that are OFF (Overrides with IsAvailable = false)
            // We fetch future overrides (e.g. from today onwards)
            var offDateOverrides = await _context.Availabilities
                .Find(a => a.ProfessionalID == professionalId && a.Date != null && a.Date >= DateTime.UtcNow.Date && a.IsAvailable == false)
                .ToListAsync();

            var offDates = offDateOverrides.Select(a => a.Date.Value.ToString("yyyy-MM-dd")).ToList();

            decimal price = (decimal)professional.Price;
            string jobTitle = professional.JobTitle;
            string? serviceName = null;

            if (!string.IsNullOrEmpty(serviceId))
            {
                var service = await _context.Services.Find(s => s.Id == serviceId).FirstOrDefaultAsync();
                if (service != null)
                {
                    price = service.Price;
                    serviceName = service.Name;
                    // Optionally append service name to job title for display context
                    // jobTitle = $"{service.Name} ({professional.JobTitle})";
                }
            }

            var viewModel = new BookAppointmentViewModel
            {
                ProfessionalID = professional.Id,
                ServiceID = serviceId,
                ServiceName = serviceName,
                User = professional.User,
                JobTitle = jobTitle,
                Price = price,
                Rating = (float)professional.Rating,
                ConsultationType = professional.ConsultationType ?? "Video Consultation",
                OffDays = offDays,
                OffDates = offDates
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(string professionalId, string date)
        {
            if (!DateTime.TryParse(date, out DateTime parsedDate))
            {
                return BadRequest("Invalid date");
            }

            // Normalize date to UTC midnight for query
            var queryDate = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, 0, 0, 0, DateTimeKind.Utc);
            var dayOfWeek = queryDate.DayOfWeek;

            // 1. Check for specific date override
            var availability = await _context.Availabilities
                .Find(a => a.ProfessionalID == professionalId && a.Date == queryDate)
                .FirstOrDefaultAsync();

            // 2. If no override, check default day schedule
            if (availability == null)
            {
                availability = await _context.Availabilities
                    .Find(a => a.ProfessionalID == professionalId && a.DayOfWeek == (int)dayOfWeek && a.Date == null)
                    .FirstOrDefaultAsync();
            }

            if (availability == null || !availability.IsAvailable)
            {
                // Return a specific structure to indicate "Day Off"
                return Json(new { error = "DayOff", message = "The professional is not available on this day." }); 
            }

            // 3. Generate slots (1 hour intervals)
            var slots = new List<string>();
            var start = availability.StartTime;
            var end = availability.EndTime;

            // Simple logic: 1 hour slots
            for (var time = start; time.Add(TimeSpan.FromHours(1)) <= end; time = time.Add(TimeSpan.FromHours(1)))
            {
                slots.Add(time.ToString(@"hh\:mm"));
            }

            // 4. Filter out booked slots
            var booked = await _context.Appointments
                .Find(a => a.ProfessionalID == professionalId && a.Date == queryDate && a.Status != AppointmentStatus.Canceled)
                .ToListAsync();

            var bookedTimes = booked.Select(a => a.StartTime.ToString(@"hh\:mm")).ToHashSet();

            slots = slots.Where(s => !bookedTimes.Contains(s)).ToList();

            return Json(slots);
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(BookAppointmentViewModel model)
        {
            Console.WriteLine("Entering Confirm method");

            // Validate Date Availability (Off Day Check)
            if (model.Date.HasValue)
            {
                 // 1. Check override
                var availability = await _context.Availabilities
                    .Find(a => a.ProfessionalID == model.ProfessionalID && a.Date == model.Date)
                    .FirstOrDefaultAsync();
                
                // 2. Check default
                if (availability == null)
                {
                    availability = await _context.Availabilities
                        .Find(a => a.ProfessionalID == model.ProfessionalID && a.DayOfWeek == (int)model.Date.Value.DayOfWeek && a.Date == null)
                        .FirstOrDefaultAsync();
                }

                if (availability == null || !availability.IsAvailable)
                {
                    ModelState.AddModelError("Date", "The professional is not available on this date.");
                }
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                        if (error.Exception != null)
                        {
                             Console.WriteLine($"Exception: {error.Exception.Message}");
                        }
                    }
                }

                // Re-populate User info for the view
                var pro = await _context.Professionals.Find(p => p.Id == model.ProfessionalID).FirstOrDefaultAsync();
                if (pro != null)
                {
                    if (pro.User != null)
                    {
                         var userObj = await _userManager.FindByIdAsync(pro.User.Id.ToString());
                         if (userObj != null) model.User = userObj;
                    }
                    
                    // FIX: Re-calculate Price based on ServiceID
                    if (!string.IsNullOrEmpty(model.ServiceID))
                    {
                        var service = await _context.Services.Find(s => s.Id == model.ServiceID).FirstOrDefaultAsync();
                        if (service != null)
                        {
                            model.Price = service.Price;
                            model.ServiceName = service.Name;
                        }
                        else
                        {
                             model.Price = (decimal)pro.Price;
                        }
                    }
                    else
                    {
                        model.Price = (decimal)pro.Price;
                    }

                    model.JobTitle = pro.JobTitle;
                    model.Rating = (float)pro.Rating;
                    model.ConsultationType = pro.ConsultationType;

                    // Re-populate OffDays for the view
                    var availabilities = await _context.Availabilities
                        .Find(a => a.ProfessionalID == model.ProfessionalID && a.Date == null)
                        .ToListAsync();

                    var availableDays = availabilities
                        .Where(a => a.IsAvailable)
                        .Select(a => a.DayOfWeek)
                        .ToHashSet();

                    var allDays = Enumerable.Range(0, 7).ToList();
                    model.OffDays = allDays.Where(d => !availableDays.Contains(d)).ToList();
                    
                    // Re-populate OffDates
                    var offDateOverrides = await _context.Availabilities
                        .Find(a => a.ProfessionalID == model.ProfessionalID && a.Date != null && a.Date >= DateTime.UtcNow.Date && a.IsAvailable == false)
                        .ToListAsync();
                    model.OffDates = offDateOverrides.Select(a => a.Date.Value.ToString("yyyy-MM-dd")).ToList();
                }
                return View("Book", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
            {
                Console.WriteLine("User is null, redirecting to Login");
                return RedirectToAction("Login", "Account");
            }
            Console.WriteLine($"User found: {user.Id}");

            var professional = await _context.Professionals.Find(p => p.Id == model.ProfessionalID).FirstOrDefaultAsync();
            if (professional == null) 
            {
                Console.WriteLine($"Professional not found for ID: {model.ProfessionalID}");
                return NotFound();
            }
            Console.WriteLine($"Professional found: {professional.Id}");

            // Parse TimeSlot
            if (!TimeSpan.TryParse(model.TimeSlot, out TimeSpan startTime))
            {
                Console.WriteLine($"Invalid TimeSlot: {model.TimeSlot}");
                ModelState.AddModelError("TimeSlot", "Invalid time slot");
                return View("Book", model);
            }
            
            var endTime = startTime.Add(TimeSpan.FromHours(1)); // Default 1 hour
            
            // Fix Date to UTC
            var date = new DateTime(model.Date.Value.Year, model.Date.Value.Month, model.Date.Value.Day, 0, 0, 0, DateTimeKind.Utc);

            // Double check availability (Race condition)
             var booked = await _context.Appointments
                .Find(a => a.ProfessionalID == model.ProfessionalID && 
                           a.Date == date && 
                           a.StartTime == startTime && 
                           a.Status != AppointmentStatus.Canceled)
                .AnyAsync();

            if (booked)
            {
                Console.WriteLine("Slot already booked");
                ModelState.AddModelError("TimeSlot", "This slot was just booked by someone else.");
                return View("Book", model);
            }

            // improved ID generation
            var lastAppointment = await _context.Appointments
                .Find(_ => true)
                .SortByDescending(a => a.AppointmentID)
                .FirstOrDefaultAsync();
            
            var newId = (lastAppointment?.AppointmentID ?? 0) + 1;
            
            try 
            {
                Console.WriteLine($"Creating appointment with ID: {newId}");

                // SECURE PRICE CALCULATION
                double finalPrice = (double)model.Price; // Fallback
                string? finalServiceName = model.ServiceName;

                if (!string.IsNullOrEmpty(model.ServiceID))
                {
                    var service = await _context.Services.Find(s => s.Id == model.ServiceID).FirstOrDefaultAsync();
                    if (service != null)
                    {
                        finalPrice = (double)service.Price;
                        finalServiceName = service.Name;
                    }
                }
                else
                {
                    // Fallback to professional base price if no service selected
                    var proForPrice = await _context.Professionals.Find(p => p.Id == model.ProfessionalID).FirstOrDefaultAsync();
                    if (proForPrice != null)
                    {
                        finalPrice = (double)proForPrice.Price;
                    }
                }

                var appointment = new Appointment
                {
                    AppointmentID = newId,
                    ClientID = user.Id.ToString(),
                    ProfessionalID = model.ProfessionalID,
                    ServiceID = model.ServiceID,
                    ServiceName = finalServiceName,
                    Date = date,
                    StartTime = startTime,
                    EndTime = endTime,
                    Price = finalPrice,
                    Status = AppointmentStatus.Pending,
                    ReasonForVisit = model.Reason,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    // Embed Client for easier display
                    Client = user,
                    // Embed Professional for easier display
                    Professional = professional
                };

                Console.WriteLine($"Inserting appointment: ClientID={appointment.ClientID}, ProID={appointment.ProfessionalID}, Date={appointment.Date}");
                await _context.Appointments.InsertOneAsync(appointment);
                Console.WriteLine("Appointment inserted successfully");

                // Notify Professional
                if (professional.User != null)
                {
                    var notification = new Notification
                    {
                        UserId = professional.User.Id.ToString(),
                        Title = "New Appointment Request",
                        Message = $"You have a new appointment request from {user.Name} for {date.ToShortDateString()} at {startTime}",
                        Type = NotificationType.Appointment,
                        RelatedId = appointment.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Notifications.InsertOneAsync(notification);
                    await _hubContext.Clients.User(professional.User.Id.ToString()).SendAsync("ReceiveNotification", notification.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving appointment: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while saving the appointment. Please try again.");
                return View("Book", model);
            }

            return RedirectToAction("Appointments", "Client");
        }
        
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _context.Appointments.Find(a => a.AppointmentID == id).FirstOrDefaultAsync();
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Appointment appointment)
        {
            var filter = Builders<Appointment>.Filter.Eq(a => a.AppointmentID, appointment.AppointmentID);
            await _context.Appointments.ReplaceOneAsync(filter, appointment);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.Find(a => a.AppointmentID == id).FirstOrDefaultAsync();
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _context.Appointments.DeleteOneAsync(a => a.AppointmentID == id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Accept(int id)
        {
            var appointment = await _context.Appointments.Find(a => a.AppointmentID == id).FirstOrDefaultAsync();
            if (appointment == null) return NotFound();

            var filter = Builders<Appointment>.Filter.Eq(a => a.AppointmentID, id);
            var update = Builders<Appointment>.Update.Set(a => a.Status, AppointmentStatus.Confirmed);
            await _context.Appointments.UpdateOneAsync(filter, update);

            // Notify Client
            var notification = new Notification
            {
                UserId = appointment.ClientID,
                Title = "Appointment Confirmed",
                Message = $"Your appointment on {appointment.Date.ToShortDateString()} has been confirmed.",
                Type = NotificationType.Appointment,
                RelatedId = appointment.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Notifications.InsertOneAsync(notification);
            await _hubContext.Clients.User(appointment.ClientID).SendAsync("ReceiveNotification", notification.Message);

            return RedirectToAction("Dashboard", "Professional");
        }

        public async Task<IActionResult> Decline(int id)
        {
            var appointment = await _context.Appointments.Find(a => a.AppointmentID == id).FirstOrDefaultAsync();
            if (appointment == null) return NotFound();

            var filter = Builders<Appointment>.Filter.Eq(a => a.AppointmentID, id);
            var update = Builders<Appointment>.Update.Set(a => a.Status, AppointmentStatus.Canceled);
            await _context.Appointments.UpdateOneAsync(filter, update);

            // Notify Client
            var notification = new Notification
            {
                UserId = appointment.ClientID,
                Title = "Appointment Declined",
                Message = $"Your appointment on {appointment.Date.ToShortDateString()} has been declined.",
                Type = NotificationType.Appointment,
                RelatedId = appointment.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Notifications.InsertOneAsync(notification);
            await _hubContext.Clients.User(appointment.ClientID).SendAsync("ReceiveNotification", notification.Message);

            return RedirectToAction("Dashboard", "Professional");
        }

        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var professional = await _context.Professionals.Find(p => p.User != null && p.User.Id == user.Id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            var appointments = await _context.Appointments.Find(a => a.ProfessionalID == professional.Id).ToListAsync();
            
            // Populate Client details for display
            foreach(var app in appointments)
            {
                if(app.Client == null && !string.IsNullOrEmpty(app.ClientID))
                {
                    app.Client = await _userManager.FindByIdAsync(app.ClientID);
                }
            }
            
            return View(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> SuggestNewTime(string id, DateTime date, TimeSpan time)
        {
            var appointment = await _context.Appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (appointment == null) return NotFound();

            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, id);
            var update = Builders<Appointment>.Update
                .Set(a => a.SuggestedDate, date)
                .Set(a => a.SuggestedStartTime, time)
                .Set(a => a.IsRescheduleRequested, true);
                
            await _context.Appointments.UpdateOneAsync(filter, update);

            // Notify Client
            var notification = new Notification
            {
                UserId = appointment.ClientID,
                Title = "Reschedule Requested",
                Message = $"The professional has requested to reschedule your appointment to {date.ToShortDateString()} at {time}.",
                Type = NotificationType.Appointment,
                RelatedId = appointment.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Notifications.InsertOneAsync(notification);
            await _hubContext.Clients.User(appointment.ClientID).SendAsync("ReceiveNotification", notification.Message);

            return RedirectToAction("Manage"); // Or Professional/Appointments
        }

        [HttpPost]
        public async Task<IActionResult> AcceptSuggestion(string id)
        {
            var appointment = await _context.Appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (appointment == null) return NotFound();

            if (!appointment.IsRescheduleRequested || !appointment.SuggestedDate.HasValue || !appointment.SuggestedStartTime.HasValue)
            {
                return BadRequest("No suggestion to accept.");
            }

            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, id);
            var update = Builders<Appointment>.Update
                .Set(a => a.Date, appointment.SuggestedDate.Value)
                .Set(a => a.StartTime, appointment.SuggestedStartTime.Value)
                .Set(a => a.EndTime, appointment.SuggestedStartTime.Value.Add(TimeSpan.FromHours(1))) // Assuming 1 hour duration
                .Set(a => a.Status, AppointmentStatus.Confirmed)
                .Set(a => a.IsRescheduleRequested, false)
                .Unset(a => a.SuggestedDate)
                .Unset(a => a.SuggestedStartTime);

            await _context.Appointments.UpdateOneAsync(filter, update);

            // Notify Professional
            var notification = new Notification
            {
                UserId = appointment.Professional.User.Id.ToString(), // Assuming Professional embedded has User
                Title = "Reschedule Accepted",
                Message = $"Client accepted the new time: {appointment.SuggestedDate.Value.ToShortDateString()} at {appointment.SuggestedStartTime.Value}.",
                Type = NotificationType.Appointment,
                RelatedId = appointment.Id,
                CreatedAt = DateTime.UtcNow
            };
            
            // Need to get Pro User ID reliably. 
            // The embedded Professional object might be null or outdated.
            // Better to use ProfessionalID to find Professional then User.
            var pro = await _context.Professionals.Find(p => p.Id == appointment.ProfessionalID).FirstOrDefaultAsync();
            if (pro != null && pro.User != null)
            {
                 notification.UserId = pro.User.Id.ToString();
                 await _context.Notifications.InsertOneAsync(notification);
                 await _hubContext.Clients.User(pro.User.Id.ToString()).SendAsync("ReceiveNotification", notification.Message);
            }

            return RedirectToAction("Appointments", "Client");
        }
        
        [HttpPost]
        public async Task<IActionResult> DeclineSuggestion(string id)
        {
            var appointment = await _context.Appointments.Find(a => a.Id == id).FirstOrDefaultAsync();
            if (appointment == null) return NotFound();

            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, id);
            var update = Builders<Appointment>.Update
                .Set(a => a.IsRescheduleRequested, false)
                .Unset(a => a.SuggestedDate)
                .Unset(a => a.SuggestedStartTime);
                // Optionally cancel? For now just clear suggestion.
            
            await _context.Appointments.UpdateOneAsync(filter, update);

            return RedirectToAction("Appointments", "Client");
        }
    }
}