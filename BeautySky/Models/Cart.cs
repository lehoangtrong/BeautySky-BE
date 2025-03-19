using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? UserId { get; set; }

    public int? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal TotalPrice { get; set; }
    [JsonIgnore]

    public virtual Product? Product { get; set; }
    [JsonIgnore]

    public virtual User? User { get; set; }
}
