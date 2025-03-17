using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? UserId { get; set; }

    public int? PaymentTypeId { get; set; }

    public int? PaymentStatusId { get; set; }

    public DateTime? PaymentDate { get; set; }
    [JsonIgnore]

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    [JsonIgnore]
    public virtual PaymentStatus? PaymentStatus { get; set; }
    [JsonIgnore]
    public virtual PaymentType? PaymentType { get; set; }
    [JsonIgnore]
    public virtual User? User { get; set; }
}
