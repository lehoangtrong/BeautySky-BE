namespace BeautySky.DTO
{
    public class NewsDTO
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IFormFile? File { get; set; }
        public bool? IsActive { get; set; }
    }
}