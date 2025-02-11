using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class PaymentType
{
    public int PaymentTypeId { get; set; }

    public string PaymentTypeName { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
