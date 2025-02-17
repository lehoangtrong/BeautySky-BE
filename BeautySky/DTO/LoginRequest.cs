using System.ComponentModel.DataAnnotations;

namespace BeautySky.DTO
{
    public class LoginRequest
    {
        public string? UserName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string Password { get; set; }
    }

}
