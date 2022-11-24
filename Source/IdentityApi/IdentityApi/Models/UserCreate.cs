using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Models
{
    public class UserCreate : UserBase
    {
        [Required]
        [StringLength(128, MinimumLength = 8)]
        public string Password { get; set; } = default!;
    }
}
