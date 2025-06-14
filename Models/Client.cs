namespace AAUP_LabMaster.Models
{
    public class Client: User
    {
        public enum Type
        {
            Student, Researcher, Faculty

        }
        public List<Booking> Bookings { set; get; }
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public Type type { get; set; }
        public Client(int id, string FullName, string email,string role, Type type) : base(id, FullName, email,role)
        {
            this.type = type;

        }
    }
}
