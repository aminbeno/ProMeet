using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using ProMeet.Data;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using ProMeet.ViewModels;
using ProMeet.Models.ViewModels;

namespace ProMeet.Controllers
{
    public class AvailabilityController : Controller
    {
        private readonly MongoDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AvailabilityController(MongoDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var availabilities = await _context.Availabilities.Find(_ => true).ToListAsync();
            return View(availabilities);
        }

        public async Task<IActionResult> Details(int id)
        {
            var availability = await _context.Availabilities.Find(a => a.AvailabilityID == id).FirstOrDefaultAsync();
            if (availability == null) return NotFound();
            return View(availability);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Availability availability)
        {
            var count = await _context.Availabilities.CountDocumentsAsync(_ => true);
            availability.AvailabilityID = (int)count + 1;
            await _context.Availabilities.InsertOneAsync(availability);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var availability = await _context.Availabilities.Find(a => a.AvailabilityID == id).FirstOrDefaultAsync();
            if (availability == null) return NotFound();
            return View(availability);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Availability availability)
        {
            var filter = Builders<Availability>.Filter.Eq(a => a.AvailabilityID, availability.AvailabilityID);
            await _context.Availabilities.ReplaceOneAsync(filter, availability);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var availability = await _context.Availabilities.Find(a => a.AvailabilityID == id).FirstOrDefaultAsync();
            if (availability == null) return NotFound();
            return View(availability);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _context.Availabilities.DeleteOneAsync(a => a.AvailabilityID == id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var professional = await _context.Professionals.Find(p => p.User != null && p.User.Id == user.Id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            // 1. Get Date Range (Next 12 months) using UTC to match storage
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddMonths(12);

            // 2. Get All Availabilities for this Pro
            var allAvailabilities = await _context.Availabilities
                .Find(a => a.ProfessionalID == professional.Id)
                .ToListAsync();

            var baseAvailabilities = allAvailabilities.Where(a => a.Date == null).ToList();
            var specificAvailabilities = allAvailabilities.Where(a => a.Date != null).ToList();

            var viewModel = new CalendarViewModel
            {
                StartDate = startDate,
                EndDate = endDate
            };

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayOfWeekInt = (int)date.DayOfWeek;
                
                // Check for specific override (Compare strictly by Date value)
                var overrideAvail = specificAvailabilities.FirstOrDefault(a => a.Date.HasValue && a.Date.Value.Date == date);
                
                // Check for base rule
                var baseAvail = baseAvailabilities.FirstOrDefault(a => a.DayOfWeek == dayOfWeekInt);

                var calendarDay = new CalendarDay
                {
                    Date = date,
                    IsPast = date < DateTime.UtcNow.Date,
                    IsToday = date == DateTime.UtcNow.Date
                };

                if (overrideAvail != null)
                {
                    calendarDay.IsAvailable = overrideAvail.IsAvailable;
                    calendarDay.StartTime = overrideAvail.StartTime;
                    calendarDay.EndTime = overrideAvail.EndTime;
                    calendarDay.HasOverride = true;
                }
                else if (baseAvail != null)
                {
                    calendarDay.IsAvailable = baseAvail.IsAvailable;
                    calendarDay.StartTime = baseAvail.StartTime;
                    calendarDay.EndTime = baseAvail.EndTime;
                    calendarDay.HasOverride = false;
                }
                else
                {
                    // Default if nothing set: Weekdays 9-5
                    bool isWeekend = date.DayOfWeek == System.DayOfWeek.Saturday || date.DayOfWeek == System.DayOfWeek.Sunday;
                    calendarDay.IsAvailable = !isWeekend;
                    calendarDay.StartTime = new TimeSpan(9, 0, 0);
                    calendarDay.EndTime = new TimeSpan(17, 0, 0);
                    calendarDay.HasOverride = false;
                }

                viewModel.Days.Add(calendarDay);
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveDailyAvailability([FromBody] SaveAvailabilityRequest model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var professional = await _context.Professionals.Find(p => p.User != null && p.User.Id == user.Id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            // Manually parse the date string "yyyy-MM-dd" to avoid timezone issues
            // We want the exact date represented by the string, at midnight UTC.
            if (!DateTime.TryParseExact(model.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format");
            }

            var dateToSave = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, 0, 0, 0, DateTimeKind.Utc);

            // Check if exists
            var existing = await _context.Availabilities.Find(a => 
                a.ProfessionalID == professional.Id && 
                a.Date == dateToSave).FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.IsAvailable = model.IsAvailable;
                existing.StartTime = model.StartTime;
                existing.EndTime = model.EndTime;
                existing.UpdatedAt = DateTime.UtcNow;
                
                // Use ReplaceOne to update
                await _context.Availabilities.ReplaceOneAsync(a => a.Id == existing.Id, existing);
            }
            else
            {
                var count = await _context.Availabilities.CountDocumentsAsync(_ => true);
                var newAvail = new Availability
                {
                    AvailabilityID = (int)count + 1, // Maintain ID sequence
                    ProfessionalID = professional.Id,
                    Date = dateToSave,
                    DayOfWeek = (int)dateToSave.DayOfWeek,
                    IsAvailable = model.IsAvailable,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                 await _context.Availabilities.InsertOneAsync(newAvail);
            }

            return Ok(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSchedule(ManageAvailabilityViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var professional = await _context.Professionals.Find(p => p.User != null && p.User.Id == user.Id).FirstOrDefaultAsync();
            if (professional == null) return NotFound();

            foreach (var day in model.Schedule)
            {
                var existing = await _context.Availabilities.Find(a => a.ProfessionalID == professional.Id && a.DayOfWeek == day.DayOfWeek).FirstOrDefaultAsync();
                
                if (existing != null)
                {
                    existing.IsAvailable = day.IsAvailable;
                    existing.StartTime = day.StartTime;
                    existing.EndTime = day.EndTime;
                    existing.UpdatedAt = DateTime.UtcNow;
                    
                    await _context.Availabilities.ReplaceOneAsync(a => a.AvailabilityID == existing.AvailabilityID, existing);
                }
                else
                {
                    // Generate new ID (simple auto-increment simulation)
                    // Note: In production, this is race-condition prone. Better to use ObjectId or a proper sequence.
                    // But sticking to existing pattern for now.
                    var count = await _context.Availabilities.CountDocumentsAsync(_ => true);
                    
                    var newAvailability = new Availability
                    {
                        AvailabilityID = (int)count + 1 + day.DayOfWeek, // Temporary hack for unique ID
                        ProfessionalID = professional.Id,
                        DayOfWeek = day.DayOfWeek,
                        IsAvailable = day.IsAvailable,
                        StartTime = day.StartTime,
                        EndTime = day.EndTime,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    await _context.Availabilities.InsertOneAsync(newAvailability);
                }
            }

            TempData["SuccessMessage"] = "Availability schedule updated successfully!";
            return RedirectToAction("Manage");
        }
    }
}