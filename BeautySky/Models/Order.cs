using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public DateTime? OrderDate { get; set; }

    public int? UserId { get; set; }

    public decimal? TotalAmount { get; set; }

    public int? PromotionId { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? FinalAmount { get; set; }

    public int? PaymentId { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Payment? Payment { get; set; }

    public virtual Promotion? Promotion { get; set; }

    public virtual User? User { get; set; }
}
