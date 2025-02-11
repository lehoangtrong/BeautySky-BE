using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string ConfirmPassword { get; set; } = null!;

    public int? RoleId { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public DateTime? DateCreate { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<UserCarePlan> UserCarePlans { get; set; } = new List<UserCarePlan>();

    public virtual ICollection<UserQuiz> UserQuizzes { get; set; } = new List<UserQuiz>();
}
