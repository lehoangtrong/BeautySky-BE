using System.Text.Json.Serialization;

namespace BeautySky.Models.Vnpay
{
    public class PaymentInformationModel
    {
        public string? OrderType { get; set; }
        //[JsonIgnore]  // Bỏ qua amount từ request
        public decimal Amount { get; set; } // Cho phép nhập Amount
        public string? OrderDescription { get; set; }
        public string? Name { get; set; }
        public int UserId { get; set; }



    }
}
