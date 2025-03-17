using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class User
{
    private ICollection<UserQuiz> userQuizzes = new List<UserQuiz>();

    public int UserId { get; set; }
    [Required]
    public string UserName { get; set; } = null!;
    [Required]
    public string? FullName { get; set; }
    [Required, EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = null!;
    [Required]

    public string Password { get; set; } = null!;
    [Required]

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
    [JsonIgnore]

    public virtual ICollection<CarePlanProduct> CarePlanProducts { get; set; } = new List<CarePlanProduct>();
    [JsonIgnore]

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
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

    public virtual ICollection<UserQuiz> UserQuizzes { get => userQuizzes; set => userQuizzes = value; }
}
