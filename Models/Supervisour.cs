namespace AAUP_LabMaster.Models
{
    public class Supervisour: User
    {
        public string Specialist {  get; set; }
        
        public Supervisour(int id,string name,string email,string role,string Speatialist):base(id,name,email,role) {
            this.Specialist = Specialist;
        }
        public Supervisour() { }
    }
}
