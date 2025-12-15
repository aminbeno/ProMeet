using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class UserController : Controller
    {
        private static List<ApplicationUser> _users = new List<ApplicationUser>();

        public IActionResult Index()
        {
            return View(_users);
        }

        public IActionResult Details(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == new System.Guid());
            if (user == null) return NotFound();
            return View(user);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ApplicationUser user)
        {
            _users.Add(user);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == new System.Guid());
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(ApplicationUser user)
        {
            var existing = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existing == null) return NotFound();
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.City = user.City;
            existing.Country = user.Country;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == new System.Guid());
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == new System.Guid());
            if (user != null) _users.Remove(user);
            return RedirectToAction("Index");
        }
    }
}