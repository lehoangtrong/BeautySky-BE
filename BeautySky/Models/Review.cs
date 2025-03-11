using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? ProductId { get; set; }

    public int? UserId { get; set; }
    [Range(0, 5, ErrorMessage = "Rating cannot exceed 5 stars")]
    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? ReviewDate { get; set; }
    [JsonIgnore]
    public virtual Product? Product { get; set; }
    [JsonIgnore]
    public virtual User? User { get; set; }
}
