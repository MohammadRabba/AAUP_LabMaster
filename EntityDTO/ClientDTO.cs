namespace AAUP_LabMaster.Models
{
    public class ClientDTO
    {
        public enum Type
        {
            Student, Researcher, Faculty

        }
        public Type type { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public string? Password { get; set; }


    }
}
