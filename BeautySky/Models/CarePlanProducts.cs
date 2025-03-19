using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class CarePlanProducts
{
    public int CarePlanProductId { get; set; }
    public int CarePlanId { get; set; }

    public int StepId { get; set; }

    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public int? UserId { get; set; }
    [JsonIgnore]

    public virtual CarePlan CarePlan { get; set; } = null!;
    [JsonIgnore]
    public virtual CarePlanStep Step { get; set; } = null!;
    [JsonIgnore]

    public virtual User? User { get; set; }
}
