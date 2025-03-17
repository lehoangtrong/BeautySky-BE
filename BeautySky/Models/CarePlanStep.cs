using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class CarePlanStep
{
    public int StepId { get; set; }

    public int? CarePlanId { get; set; }

    public int StepOrder { get; set; }

    public string StepName { get; set; } = null!;

    public string? StepDescription { get; set; }
    [JsonIgnore]
    public virtual CarePlan? CarePlan { get; set; }
    [JsonIgnore]
    public virtual ICollection<CarePlanProducts> CarePlanProducts { get; set; } = new List<CarePlanProducts>();
}
