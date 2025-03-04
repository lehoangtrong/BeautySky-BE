using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class UserCarePlan
{
    public int UserCarePlanId { get; set; }

    public int? UserId { get; set; }

    public int? CarePlanId { get; set; }

    public DateTime? DateCreate { get; set; }
    [JsonIgnore]
    public virtual CarePlan? CarePlan { get; set; }
    [JsonIgnore]
    public virtual User? User { get; set; }
}
