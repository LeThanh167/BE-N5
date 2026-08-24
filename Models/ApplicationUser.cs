using Microsoft.AspNetCore.Identity;

namespace PlantShopAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "USER"; // ADMIN hoặc USER
    }
}