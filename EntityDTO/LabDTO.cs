using AAUP_LabMaster.Models;
using System.ComponentModel.DataAnnotations;

namespace AAUP_LabMaster.EntityDTO
{
    public class LabDTO
    {
        [Required(ErrorMessage = "Lab Name is required.")]
        [StringLength(100, ErrorMessage = "Lab Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; } // Nullable if not always required

        [Required(ErrorMessage = "Please select a supervisor.")]
        [Display(Name = "Supervisor")]
        public string SelectedSupervisorId { get; set; }

        public List<string> EquipmentNames { get; set; } = new List<string>();
    }
}
