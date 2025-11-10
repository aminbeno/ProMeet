using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class ReviewController : Controller
    {
        private static List<Review> _reviews = new List<Review>();

        public IActionResult Index()
        {
            return View(_reviews);
        }

        public IActionResult Details(int id)
        {
            var review = _reviews.FirstOrDefault(r => r.ReviewID == id);
            if (review == null) return NotFound();
            return View(review);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Review review)
        {
            review.ReviewID = _reviews.Count > 0 ? _reviews.Max(r => r.ReviewID) + 1 : 1;
            _reviews.Add(review);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var review = _reviews.FirstOrDefault(r => r.ReviewID == id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost]
        public IActionResult Edit(Review review)
        {
            var existing = _reviews.FirstOrDefault(r => r.ReviewID == review.ReviewID);
            if (existing == null) return NotFound();
            existing.AppointmentID = review.AppointmentID;
            existing.Rating = review.Rating;
            existing.Comment = review.Comment;
            existing.DateProvided = review.DateProvided;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var review = _reviews.FirstOrDefault(r => r.ReviewID == id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var review = _reviews.FirstOrDefault(r => r.ReviewID == id);
            if (review != null) _reviews.Remove(review);
            return RedirectToAction("Index");
        }
    }
}