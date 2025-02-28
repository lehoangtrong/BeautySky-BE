using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class User
{
    public int UserId { get; set; }
    [Required]
    public string UserName { get; set; } = null!;
    [Required]
    public string? FullName { get; set; }
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
    
    public string Password { get; set; } = null!;
    
    public string ConfirmPassword { get; set; } = null!;

    public int? RoleId { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "Số điện thoại phải có đúng 10 chữ số.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Số điện thoại chỉ được chứa chữ số.")]
    [DefaultValue("string")]
    public string? Phone { get; set; }
    [Required]
    public string? Address { get; set; }

    public DateTime? DateCreate { get; set; }

    public bool? IsActive { get; set; }

    [JsonIgnore] // Ẩn khi trả dữ liệu ra Swagger
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [JsonIgnore]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [JsonIgnore]
    public virtual Role? Role { get; set; }

    [JsonIgnore]
    public virtual ICollection<UserCarePlan> UserCarePlans { get; set; } = new List<UserCarePlan>();

    [JsonIgnore]
    public virtual ICollection<UserQuiz> UserQuizzes { get; set; } = new List<UserQuiz>();
}
