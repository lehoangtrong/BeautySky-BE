using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BeautySky.Models;

public partial class User
{
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

    public virtual ICollection<CarePlanProduct> CarePlanProducts { get; set; } = new List<CarePlanProduct>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<UserCarePlan> UserCarePlans { get; set; } = new List<UserCarePlan>();

    public virtual ICollection<UserQuiz> UserQuizzes { get; set; } = new List<UserQuiz>();
}
