using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class SkinType
{
    public int SkinTypeId { get; set; }

    public string SkinTypeName { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<CarePlan> CarePlans { get; set; } = new List<CarePlan>();
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    [JsonIgnore]
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
