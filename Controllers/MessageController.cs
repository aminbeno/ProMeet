using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class MessageController : Controller
    {
        private static List<Message> _messages = new List<Message>();

        public IActionResult Index()
        {
            return View(_messages);
        }

        public IActionResult Details(int id)
        {
            var message = _messages.FirstOrDefault(m => m.MessageID == id);
            if (message == null) return NotFound();
            return View(message);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Message message)
        {
            message.MessageID = _messages.Count > 0 ? _messages.Max(m => m.MessageID) + 1 : 1;
            _messages.Add(message);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var message = _messages.FirstOrDefault(m => m.MessageID == id);
            if (message == null) return NotFound();
            return View(message);
        }

        [HttpPost]
        public IActionResult Edit(Message message)
        {
            var existing = _messages.FirstOrDefault(m => m.MessageID == message.MessageID);
            if (existing == null) return NotFound();
            existing.ChatID = message.ChatID;
            existing.SenderID = message.SenderID;
            existing.Content = message.Content;
            existing.Timestamp = message.Timestamp;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var message = _messages.FirstOrDefault(m => m.MessageID == id);
            if (message == null) return NotFound();
            return View(message);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var message = _messages.FirstOrDefault(m => m.MessageID == id);
            if (message != null) _messages.Remove(message);
            return RedirectToAction("Index");
        }
    }
}