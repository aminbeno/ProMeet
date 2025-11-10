using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class ProfessionalController : Controller
    {
        private static List<Professional> _professionals = new List<Professional>();

        public IActionResult Index()
        {
            return View(_professionals);
        }

        public IActionResult Details(int id)
        {
            var professional = _professionals.FirstOrDefault(p => p.ProfessionalID == id);
            if (professional == null) return NotFound();
            return View(professional);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Professional professional)
        {
            professional.ProfessionalID = _professionals.Count > 0 ? _professionals.Max(p => p.ProfessionalID) + 1 : 1;
            _professionals.Add(professional);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var professional = _professionals.FirstOrDefault(p => p.ProfessionalID == id);
            if (professional == null) return NotFound();
            return View(professional);
        }

        [HttpPost]
        public IActionResult Edit(Professional professional)
        {
            var existing = _professionals.FirstOrDefault(p => p.ProfessionalID == professional.ProfessionalID);
            if (existing == null) return NotFound();
            existing.UserID = professional.UserID;
            existing.JobTitle = professional.JobTitle;
            existing.Specialty = professional.Specialty;
            existing.Bio = professional.Bio;
            existing.Experience = professional.Experience;
            existing.Degrees = professional.Degrees;
            existing.ConsultationType = professional.ConsultationType;
            existing.Price = professional.Price;
            existing.IsValidated = professional.IsValidated;
            existing.Rating = professional.Rating;
            existing.ProfileActive = professional.ProfileActive;
            existing.CategoryID = professional.CategoryID;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var professional = _professionals.FirstOrDefault(p => p.ProfessionalID == id);
            if (professional == null) return NotFound();
            return View(professional);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var professional = _professionals.FirstOrDefault(p => p.ProfessionalID == id);
            if (professional != null) _professionals.Remove(professional);
            return RedirectToAction("Index");
        }
    }
}