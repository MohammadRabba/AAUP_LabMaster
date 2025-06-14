using AAUP_LabMaster.Models;
using Microsoft.EntityFrameworkCore;

namespace AAUP_LabMaster.EntityDTO
{
    public class BookingDTO
    {

        public string Description { get; set; } 
        public DateTime Date { get; set; }
    
        public int ClientId { get; set; }
        public Client Client { get; set; }
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }

        public string Notes { get; set; } = string.Empty;
        [Precision(18, 2)]

        public decimal Price { get; set; }
    }
}
