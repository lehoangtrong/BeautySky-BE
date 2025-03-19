using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class PaymentStatus
{
    public int PaymentStatusId { get; set; }

    public string PaymentStatus1 { get; set; } = null!;
    [JsonIgnore]

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
