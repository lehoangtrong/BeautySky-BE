using BeautySky.Models;

namespace BeautySky.DTO
{
    public class CartDTO
    {
  
            public int CartId { get; set; }

            public int? UserId { get; set; }

            public int? ProductId { get; set; }

            public int Quantity { get; set; }

            //public decimal TotalPrice { get; set; }
            //[JsonIgnore]
            //public virtual Product? Product { get; set; }
            //[JsonIgnore]
            //public virtual User? User { get; set; }


    }
}
