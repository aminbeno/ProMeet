using Microsoft.AspNetCore.Mvc;
using ProMeet.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ProMeet.Controllers
{
    public class AppointmentController : Controller
    {
        private static List<Appointment> _appointments = new List<Appointment>();
        private static List<Professional> _professionals = new List<Professional>();

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

        public IActionResult Book(int professionalId)
        {
            // Get professional details from ProfessionalController's static list
            var professionals = ProfessionalController.GetProfessionals();
            var professional = professionals.FirstOrDefault(p => p.ProfessionalID == professionalId);
            if (professional == null) return NotFound();
            return View(professional);
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

    // View Models for Appointments
    public class BookAppointmentViewModel
    {
        [Required(ErrorMessage = "Professional ID is required")]
        public string ProfessionalID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Appointment Date")]
        [CustomValidation(typeof(BookAppointmentViewModel), "ValidateAppointmentDate")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Time slot is required")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid time format")]
        [Display(Name = "Time Slot")]
        public string TimeSlot { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        [Display(Name = "Reason for Visit")]
        public string? Reason { get; set; }

        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Display(Name = "Rating")]
        public float Rating { get; set; }

        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = "";

        [Display(Name = "Consultation Type")]
        public string ConsultationType { get; set; } = "";

        public User User { get; set; } = new User();

        public static ValidationResult? ValidateAppointmentDate(DateTime date, ValidationContext context)
        {
            if (date < DateTime.Today)
            {
                return new ValidationResult("Appointment date cannot be in the past");
            }
            
            if (date > DateTime.Today.AddMonths(3))
            {
                return new ValidationResult("Appointment date cannot be more than 3 months in the future");
            }
            
            return ValidationResult.Success;
        }
    }
}