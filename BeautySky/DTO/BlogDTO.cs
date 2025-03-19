namespace BeautySky.DTO
{
    public class BlogDTO
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int? AuthorId { get; set; }
        public string Status { get; set; } = null!;
        public string? SkinType { get; set; }
        public string? Category { get; set; }
        public IFormFile? File { get; set; } // Thêm thuộc tính File để upload hình ảnh
        public bool? IsActive { get; set; }
    }
}
