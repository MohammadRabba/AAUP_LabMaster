using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Required for IEnumerable<ValidationResult>

namespace AAUP_LabMaster.Models
{
    public class UserDTO : IValidatableObject // IMPORTANT: Implement IValidatableObject
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)] // Good practice for password fields
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        public string SelectedRoleName { get; set; }

        // Make Specialist nullable if it's not always required
        // 'string?' indicates it can be null.
        public string? Specialist { get; set; }

        // 'Client.Type?' is already nullable, which is good.
        public Client.Type? type { get; set; }


        // This is the core fix for your "Please fill in all required fields correctly." issue
        // It provides conditional server-side validation.
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Conditional validation for "Supervisour" role
            // Note: Your HTML has "Supervisour" (with 'our'), make sure it matches here.
            if (SelectedRoleName == "Supervisour")
            {
                if (string.IsNullOrWhiteSpace(Specialist))
                {
                    yield return new ValidationResult("Specialist is required for Supervisors.", new[] { nameof(Specialist) });
                }
                // Ensure 'type' is NOT set if Supervisor is selected.
                // This prevents validation errors if a value lingers from a previous selection.
                if (type.HasValue) // Or if type is a string, check !string.IsNullOrEmpty(type)
                {
                    yield return new ValidationResult("Client Type should not be selected for Supervisors.", new[] { nameof(type) });
                }
            }
            // Conditional validation for "Client" role
            else if (SelectedRoleName == "Client")
            {
                if (!type.HasValue) // Check if the nullable enum has a value
                {
                    yield return new ValidationResult("Client Type is required for Clients.", new[] { nameof(type) });
                }
                // Ensure 'Specialist' is NOT set if Client is selected.
                if (!string.IsNullOrWhiteSpace(Specialist))
                {
                    yield return new ValidationResult("Specialist should not be entered for Clients.", new[] { nameof(Specialist) });
                }
            }
            // Validation for other roles (e.g., Guest) if Specialist or type are not applicable
            else if (SelectedRoleName == "Guest")
            {
                // Ensure these fields are empty/null for Guests
                if (!string.IsNullOrWhiteSpace(Specialist))
                {
                    yield return new ValidationResult("Specialist should not be entered for Guests.", new[] { nameof(Specialist) });
                }
                if (type.HasValue)
                {
                    yield return new ValidationResult("Client Type should not be selected for Guests.", new[] { nameof(type) });
                }
            }


            // No yield return here if no validation errors for the current context.
            // DataAnnotations like [Required], [EmailAddress], [Compare] are processed automatically
            // before this Validate method is called.
        }
    }
}