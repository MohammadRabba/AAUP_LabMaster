using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using static AAUP_LabMaster.Models.Equipment;

namespace AAUP_LabMaster.EntityDTO
{
    public class EquipmentDTO
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Precision(18, 2)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public Availability status { get; set; }

        [Required(ErrorMessage = "Lab selection is required")]
        public int LabId { get; set; }

    }
}
