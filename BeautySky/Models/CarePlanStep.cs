using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class CarePlanStep
{
    public int StepId { get; set; }

    public int? CarePlanId { get; set; }

    public int StepOrder { get; set; }

    public string StepName { get; set; } = null!;

    public string? StepDescription { get; set; }

    public virtual CarePlan? CarePlan { get; set; }

    public virtual ICollection<CarePlanProduct> CarePlanProducts { get; set; } = new List<CarePlanProduct>();
}
