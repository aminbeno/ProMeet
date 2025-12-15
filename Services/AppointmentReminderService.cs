using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using ProMeet.Data;
using ProMeet.Hubs;
using ProMeet.Models;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProMeet.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<AppointmentReminderService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AppointmentReminderService(
            IServiceProvider services,
            ILogger<AppointmentReminderService> logger,
            IHubContext<NotificationHub> hubContext)
        {
            _services = services;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Reminder Service starting.");

            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await CheckAppointments(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for appointment reminders.");
                }
            }
        }

        private async Task CheckAppointments(CancellationToken stoppingToken)
        {
            using (var scope = _services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
                
                // Define "soon" as within the next 2 hours
                // Note: DateTime.Today depends on server time. 
                // Ensure consistency between how appointments are saved and checked.
                var today = DateTime.Today;
                var now = DateTime.Now; 
                
                // Find confirmed appointments for today that haven't been notified
                var appointments = await context.Appointments
                    .Find(a => a.Status == AppointmentStatus.Confirmed 
                            && !a.Notified 
                            && a.Date == today)
                    .ToListAsync(stoppingToken);

                foreach (var appointment in appointments)
                {
                    // Calculate appointment start datetime
                    // Date is usually midnight, StartTime is TimeSpan
                    var appointmentStart = appointment.Date.Date + appointment.StartTime;
                    var timeUntilStart = appointmentStart - now;

                    // If appointment is within 2 hours (and hasn't passed more than 15 mins ago)
                    if (timeUntilStart.TotalHours <= 2 && timeUntilStart.TotalMinutes > -15)
                    {
                        await SendReminder(context, appointment, stoppingToken);
                    }
                }
            }
        }

        private async Task SendReminder(MongoDbContext context, Appointment appointment, CancellationToken stoppingToken)
        {
             _logger.LogInformation($"Sending reminder for appointment {appointment.Id}");

            // Create Notification
            var message = $"Reminder: You have an appointment with your professional at {appointment.StartTime:hh\\:mm}.";
            var notification = new Notification
            {
                UserId = appointment.ClientID,
                Title = "Upcoming Appointment",
                Message = message,
                Type = NotificationType.Appointment,
                RelatedId = appointment.Id,
                CreatedAt = DateTime.UtcNow
            };

            await context.Notifications.InsertOneAsync(notification, cancellationToken: stoppingToken);

            // Send SignalR notification
            if (!string.IsNullOrEmpty(appointment.ClientID))
            {
                await _hubContext.Clients.User(appointment.ClientID).SendAsync("ReceiveNotification", message, cancellationToken: stoppingToken);
            }

            // Update Appointment Notified status
            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, appointment.Id);
            var update = Builders<Appointment>.Update.Set(a => a.Notified, true);
            await context.Appointments.UpdateOneAsync(filter, update, cancellationToken: stoppingToken);
        }
    }
}
