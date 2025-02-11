using System;
using System.Collections.Generic;

namespace BeautySky.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string? Description { get; set; }

    public string? Ingredient { get; set; }

    public int? CategoryId { get; set; }

    public int? SkinTypeId { get; set; }

    public virtual ICollection<CarePlanProduct> CarePlanProducts { get; set; } = new List<CarePlanProduct>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual ICollection<ProductsImage> ProductsImages { get; set; } = new List<ProductsImage>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual SkinType? SkinType { get; set; }
}
