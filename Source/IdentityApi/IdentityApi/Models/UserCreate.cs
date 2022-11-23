using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Models
{
    public class UserCreate : UserBase
    {
        [Required]
        public string Password { get; set; }
    }
}
