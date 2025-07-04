using AAUP_LabMaster.Models;
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
           
            var clientUsers = context.Users.Where(u => u.Role == "Client").ToList();

            if (clientUsers == null || clientUsers.Count == 0)
            {
                Console.WriteLine("No client users found to send notifications to.");
                return; 
            }

            foreach (var user in clientUsers)
            {
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

            context.SaveChanges();
            Console.WriteLine($"Successfully sent notification to {clientUsers.Count} client(s).");
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
