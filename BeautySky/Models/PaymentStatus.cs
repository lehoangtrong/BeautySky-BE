using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class PaymentStatus
{
    public int PaymentStatusId { get; set; }

    public string PaymentStatus1 { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
