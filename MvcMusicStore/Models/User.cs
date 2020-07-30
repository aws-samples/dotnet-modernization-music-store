using Microsoft.AspNetCore.Identity;

namespace MvcMusicStore.Models
{
    public class User : IdentityUser
    {
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public bool IsApproved { get; set; }
    }
}