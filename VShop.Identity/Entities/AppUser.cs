using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VShop.Identity.Entities
{
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public required string LastName { get; set; }

        [StringLength(100)]
        public string? Cedula { get; set; }

        public bool Status { get; set; } // true = Activo, false = Inactivo

    }
}
