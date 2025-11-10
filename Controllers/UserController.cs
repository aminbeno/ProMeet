using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class UserController : Controller
    {
        private static List<User> _users = new List<User>();

        public IActionResult Index()
        {
            return View(_users);
        }

        public IActionResult Details(int id)
        {
            var user = _users.FirstOrDefault(u => u.UserID == id);
            if (user == null) return NotFound();
            return View(user);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            user.UserID = _users.Count > 0 ? _users.Max(u => u.UserID) + 1 : 1;
            _users.Add(user);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var user = _users.FirstOrDefault(u => u.UserID == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            var existing = _users.FirstOrDefault(u => u.UserID == user.UserID);
            if (existing == null) return NotFound();
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Password = user.Password;
            existing.City = user.City;
            existing.Country = user.Country;
            existing.PhotoURL = user.PhotoURL;
            existing.DateJoined = user.DateJoined;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var user = _users.FirstOrDefault(u => u.UserID == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _users.FirstOrDefault(u => u.UserID == id);
            if (user != null) _users.Remove(user);
            return RedirectToAction("Index");
        }
    }
}