using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class CarePlan
{
    public int CarePlanId { get; set; }

    public int? SkinTypeId { get; set; }

    public string PlanName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<CarePlanProduct> CarePlanProducts { get; set; } = new List<CarePlanProduct>();

    public virtual ICollection<CarePlanStep> CarePlanSteps { get; set; } = new List<CarePlanStep>();

    public virtual SkinType? SkinType { get; set; }

    public virtual ICollection<UserCarePlan> UserCarePlans { get; set; } = new List<UserCarePlan>();
}
