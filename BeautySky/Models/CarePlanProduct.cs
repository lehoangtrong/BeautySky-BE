using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class CarePlanProduct
{
    public int CarePlanId { get; set; }

    public int StepId { get; set; }

    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public virtual CarePlan CarePlan { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual CarePlanStep Step { get; set; } = null!;
}
