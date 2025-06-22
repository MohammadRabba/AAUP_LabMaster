using System.ComponentModel.DataAnnotations;

namespace AAUP_LabMaster.EntityDTO
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string NewPassword { get; set; }

        [Required, Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

}
