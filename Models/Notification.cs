using System;
namespace AAUP_LabMaster.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty; // ✅ FIX: Add this line

        public DateTime DateCreated { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User? User { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
