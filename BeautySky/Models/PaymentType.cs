using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class PaymentType
{
    public int PaymentTypeId { get; set; }

    public string PaymentTypeName { get; set; } = null!;
    [JsonIgnore]

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
