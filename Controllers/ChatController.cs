using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;

namespace ProMeet.Controllers
{
    public class ChatController : Controller
    {
        private static List<Chat> _chats = new List<Chat>();

        public IActionResult Index()
        {
            return View(_chats);
        }

        public IActionResult Details(int id)
        {
            var chat = _chats.FirstOrDefault(c => c.ChatID == id);
            if (chat == null) return NotFound();
            return View(chat);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Chat chat)
        {
            chat.ChatID = _chats.Count > 0 ? _chats.Max(c => c.ChatID) + 1 : 1;
            _chats.Add(chat);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var chat = _chats.FirstOrDefault(c => c.ChatID == id);
            if (chat == null) return NotFound();
            return View(chat);
        }

        [HttpPost]
        public IActionResult Edit(Chat chat)
        {
            var existing = _chats.FirstOrDefault(c => c.ChatID == chat.ChatID);
            if (existing == null) return NotFound();
            existing.ClientID = chat.ClientID;
            existing.ProfessionalID = chat.ProfessionalID;
            existing.DateStarted = chat.DateStarted;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var chat = _chats.FirstOrDefault(c => c.ChatID == id);
            if (chat == null) return NotFound();
            return View(chat);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var chat = _chats.FirstOrDefault(c => c.ChatID == id);
            if (chat != null) _chats.Remove(chat);
            return RedirectToAction("Index");
        }
    }
}