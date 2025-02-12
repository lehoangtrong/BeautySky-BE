using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? UserId { get; set; }

    public int? PaymentTypeId { get; set; }

    public int? PaymentStatusId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual PaymentStatus? PaymentStatus { get; set; }

    public virtual PaymentType? PaymentType { get; set; }

    public virtual User? User { get; set; }
}
