using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class AvailabilityController : Controller
    {
        private static List<Availability> _availabilities = new List<Availability>();

        public IActionResult Index()
        {
            return View(_availabilities);
        }

        public IActionResult Details(int id)
        {
            var availability = _availabilities.FirstOrDefault(a => a.AvailabilityID == id);
            if (availability == null) return NotFound();
            return View(availability);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Availability availability)
        {
            availability.AvailabilityID = _availabilities.Count > 0 ? _availabilities.Max(a => a.AvailabilityID) + 1 : 1;
            _availabilities.Add(availability);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var availability = _availabilities.FirstOrDefault(a => a.AvailabilityID == id);
            if (availability == null) return NotFound();
            return View(availability);
        }

        [HttpPost]
        public IActionResult Edit(Availability availability)
        {
            var existing = _availabilities.FirstOrDefault(a => a.AvailabilityID == availability.AvailabilityID);
            if (existing == null) return NotFound();
            existing.ProfessionalID = availability.ProfessionalID;
            existing.DayOfWeek = availability.DayOfWeek;
            existing.StartHour = availability.StartHour;
            existing.EndHour = availability.EndHour;
            existing.IsRestDay = availability.IsRestDay;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var availability = _availabilities.FirstOrDefault(a => a.AvailabilityID == id);
            if (availability == null) return NotFound();
            return View(availability);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var availability = _availabilities.FirstOrDefault(a => a.AvailabilityID == id);
            if (availability != null) _availabilities.Remove(availability);
            return RedirectToAction("Index");
        }
    }
}