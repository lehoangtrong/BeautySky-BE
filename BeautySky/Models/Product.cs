using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BeautySky.Models;

public partial class Product
{
    public int ProductId { get; set; }
    [Required]
    public string ProductName { get; set; } = null!;
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
    [Range(0, 1000)]
    public int Quantity { get; set; }
    [Required]
    public string? Description { get; set; }
    [Required]
    public string? Ingredient { get; set; }
    [Required]
    public int? CategoryId { get; set; }
    [Required]
    public int? SkinTypeId { get; set; }

    [JsonIgnore]
    public virtual ICollection<CarePlanProduct> CarePlanProducts { get; set; } = new List<CarePlanProduct>();
    [JsonIgnore]
    public virtual Category? Category { get; set; }
    [JsonIgnore]
    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    [JsonIgnore]
    public virtual ICollection<ProductsImage> ProductsImages { get; set; } = new List<ProductsImage>();
    [JsonIgnore]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    [JsonIgnore]
    public virtual SkinType? SkinType { get; set; }
}
