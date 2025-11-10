using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ProMeet.Controllers
{
    public class AppointmentController : Controller
    {
        private static List<Appointment> _appointments = new List<Appointment>();

        public IActionResult Index()
        {
            return View(_appointments);
        }

        public IActionResult Details(int id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.AppointmentID == id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Appointment appointment)
        {
            appointment.AppointmentID = _appointments.Count > 0 ? _appointments.Max(a => a.AppointmentID) + 1 : 1;
            _appointments.Add(appointment);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.AppointmentID == id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        [HttpPost]
        public IActionResult Edit(Appointment appointment)
        {
            var existing = _appointments.FirstOrDefault(a => a.AppointmentID == appointment.AppointmentID);
            if (existing == null) return NotFound();
            existing.ClientID = appointment.ClientID;
            existing.ProfessionalID = appointment.ProfessionalID;
            existing.Date = appointment.Date;
            existing.StartTime = appointment.StartTime;
            existing.EndTime = appointment.EndTime;
            existing.Status = appointment.Status;
            existing.Notified = appointment.Notified;
            existing.Client = appointment.Client;
            existing.Professional = appointment.Professional;
            existing.Review = appointment.Review;
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.AppointmentID == id);
            if (appointment == null) return NotFound();
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var appointment = _appointments.FirstOrDefault(a => a.AppointmentID == id);
            if (appointment == null) return NotFound();
            _appointments.Remove(appointment);
            return RedirectToAction("Index");
        }
    }
}