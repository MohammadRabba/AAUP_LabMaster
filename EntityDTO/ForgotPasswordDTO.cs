using System.ComponentModel.DataAnnotations;

namespace AAUP_LabMaster.EntityDTO
{
    public class ForgotPasswordDTO
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
    }

}
