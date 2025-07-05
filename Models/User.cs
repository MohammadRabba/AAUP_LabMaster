namespace AAUP_LabMaster.Models

{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; }
        public string Discriminator { get; set; }

        public User(int id, string FullName, string email, string role)
        {
            Id = id;
            this.FullName = FullName;
            Email = email;
            Role = role;
            Discriminator = role;
        }
        public User() { }
    }
}
