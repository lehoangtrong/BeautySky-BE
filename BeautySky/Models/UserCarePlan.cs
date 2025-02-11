using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class UserCarePlan
{
    public int UserCarePlanId { get; set; }

    public int? UserId { get; set; }

    public int? CarePlanId { get; set; }

    public DateTime? DateCreate { get; set; }

    public virtual CarePlan? CarePlan { get; set; }

    public virtual User? User { get; set; }
}
