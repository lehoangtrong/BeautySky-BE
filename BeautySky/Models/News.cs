using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class News
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }
}
