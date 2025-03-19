using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class Blog
{
    public int BlogId { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int? AuthorId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Category { get; set; }

    public string? SkinType { get; set; }

    public string? ImgUrl { get; set; }

    public bool IsActive { get; set; }
}
