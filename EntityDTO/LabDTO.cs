using AAUP_LabMaster.Models;

namespace AAUP_LabMaster.EntityDTO
{
    public class LabDTO
    {
        public string Name { get; set; } = string.Empty;
        public int SupervisorId { get; set; }
        public Supervisour Supervisour { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<Equipment> Equipment { get; set; }
    }
}
