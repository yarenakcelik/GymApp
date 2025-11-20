using Microsoft.AspNetCore.Identity;
namespace GymApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public MemberProfile? MemberProfile { get; set; }
    }
}
