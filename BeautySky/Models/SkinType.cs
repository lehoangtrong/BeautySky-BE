using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class SkinType
{
    public int SkinTypeId { get; set; }

    public string SkinType1 { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<CarePlan> CarePlans { get; set; } = new List<CarePlan>();
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
