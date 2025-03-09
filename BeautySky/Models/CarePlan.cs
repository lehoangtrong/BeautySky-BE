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
    public virtual ICollection<CarePlanProduct> CarePlanProducts { get; set; } = new List<CarePlanProduct>();

    public virtual ICollection<CarePlanStep> CarePlanStep { get; set; } = new List<CarePlanStep>();

    [JsonIgnore]
    public virtual SkinType? SkinType { get; set; }

    [JsonIgnore]
    public virtual ICollection<UserCarePlan> UserCarePlans { get; set; } = new List<UserCarePlan>();
}