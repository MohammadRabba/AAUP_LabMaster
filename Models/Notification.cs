using System;
namespace AAUP_LabMaster.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public User User { get; set; } = new User();
        public bool IsRead { get; set; } = false;
    }
}
