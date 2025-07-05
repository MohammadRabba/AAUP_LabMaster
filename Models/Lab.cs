namespace AAUP_LabMaster.Models
{
    public class Lab
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SupervisorId {  get; set; }
        public int SupervisourId { get; set; }

        public Supervisour Supervisour { get; set; } 
        public string Description { get; set; } = string.Empty;
        public List<Equipment> Equipment { get; set; }

    }
}
