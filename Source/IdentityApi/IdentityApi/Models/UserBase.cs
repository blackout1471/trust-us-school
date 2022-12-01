using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Models
{
    public class UserBase
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        public string PhoneNumber { get; set; } = default!;
    }
}
