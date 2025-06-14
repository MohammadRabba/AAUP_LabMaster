namespace AAUP_LabMaster.Models
{
    public class Admin : User
    {
        public Admin() : base() { }

        public Admin(int id, string fullName, string email, string role)
            : base(id, fullName, email, role)
        {
        }
    }
}
