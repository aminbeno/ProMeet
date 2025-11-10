using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class CategoryController : Controller
    {
        private static List<Category> _categories = new List<Category>();

        public IActionResult Index()
        {
            return View(_categories);
        }

        public IActionResult Details(int id)
        {
            var category = _categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null) return NotFound();
            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            category.CategoryID = _categories.Count > 0 ? _categories.Max(c => c.CategoryID) + 1 : 1;
            _categories.Add(category);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var category = _categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            var existing = _categories.FirstOrDefault(c => c.CategoryID == category.CategoryID);
            if (existing == null) return NotFound();
            existing.Name = category.Name;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var category = _categories.FirstOrDefault(c => c.CategoryID == id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _categories.FirstOrDefault(c => c.CategoryID == id);
            if (category != null) _categories.Remove(category);
            return RedirectToAction("Index");
        }
    }
}