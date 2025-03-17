using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string PromotionName { get; set; } = null!;

    public decimal DiscountPercentage { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool? IsActive { get; set; }
    [JsonIgnore]

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
