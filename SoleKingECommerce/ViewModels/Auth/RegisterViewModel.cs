using System.ComponentModel.DataAnnotations;

namespace SoleKingECommerce.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required] 
        public string FirstName { get; set; }

        [Required] 
        public string LastName { get; set; }

        [Required]
        [EmailAddress] 
        public string Email { get; set; }

        [Required] 
        public string PhoneNumber { get; set; }

        [Required] 
        public string Password { get; set; }

        [Required] 
        public string ConfirmPassword { get; set; }

        public DateTime? BirthDate { get; set; }

        public string Gender { get; set; }

        [Required] public bool AcceptTerms { get; set; }

        public bool ReceiveNewsletter { get; set; }
    }
}
