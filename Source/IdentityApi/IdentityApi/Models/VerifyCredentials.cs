using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Models
{
    public class VerifyCredentials
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
