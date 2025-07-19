using AAUP_LabMaster.Models;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace AAUP_LabMaster.EntityManager
{
    public class NotificationManager
    {
        private readonly ApplicationDbContext context;

        public NotificationManager(ApplicationDbContext context )

        {
            this.context = context;
        }
        public void SendNote(Notification note)
        {
            try
            {
                Console.WriteLine($"Attempting to send note with subject: {note.Subject}");

                var clientUsers = context.Users.Where(u => u.Role == "Client").ToList();
                Console.WriteLine($"Found {clientUsers.Count} client users");

                if (clientUsers == null || clientUsers.Count == 0)
                {
                    Console.WriteLine("No client users found to send notifications to.");
                    return;
                }

                foreach (var user in clientUsers)
                {
                    Console.WriteLine($"Sending to user ID: {user.Id}");
                    var newNote = new Notification
                    {
                        UserId = user.Id,
                        Subject = note.Subject,
                        Body = note.Body,
                        IsRead = false,
                        DateCreated = DateTime.Now
                    };
                    context.Notifications.Add(newNote);
                }

                var changes = context.SaveChanges();
                Console.WriteLine($"Saved {changes} changes to database");
                Console.WriteLine($"Successfully sent notification to {clientUsers.Count} client(s).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendNote: {ex.Message}");
                throw;
            }
        }

        public void SendNotetest(Notification note)
        {
            if (note == null)
            {
                throw new ArgumentNullException(nameof(note), "Notification cannot be null");
            }

            if (note.User == null)
            {
                throw new ArgumentException("Notification must have an associated user", nameof(note));
            }

            try
            {
                var newNote = new Notification
                {
                    UserId = note.User.Id,
                    Subject = note.Subject ?? "No Subject", // Handle null subject
                    Body = note.Body ?? string.Empty,        // Handle null body
                    IsRead = false,
                    DateCreated = DateTime.Now
                };

                context.Notifications.Add(newNote);
                context.SaveChanges();

                Console.WriteLine($"Successfully sent notification to user {note.User.Id}");
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Failed to save notification: {ex.Message}");
                // Consider rethrowing or handling differently based on your needs
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error sending notification: {ex.Message}");
                throw;
            }
        }
        public void UpdateNoteStatus(int userid)
        {
            var notes = context.Notifications
                .Where(n => n.UserId == userid )
                .ToList();
            foreach (var note in notes)
            {
                note.IsRead = true;
            }
            context.SaveChanges();
        }
    }
}
