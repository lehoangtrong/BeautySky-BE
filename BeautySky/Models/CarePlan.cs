using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class CarePlan
{
    public int CarePlanId { get; set; }

    public int? SkinTypeId { get; set; }

    public string PlanName { get; set; } = null!;

    public string? Description { get; set; }
    [JsonIgnore]
    public virtual ICollection<CarePlanProducts> CarePlanProducts { get; set; } = new List<CarePlanProducts>();
    [JsonIgnore]
    public virtual ICollection<CarePlanStep> CarePlanSteps { get; set; } = new List<CarePlanStep>();
    [JsonIgnore]
    public virtual SkinType? SkinType { get; set; }
    [JsonIgnore]
    public virtual ICollection<UserCarePlan> UserCarePlans { get; set; } = new List<UserCarePlan>();
}
