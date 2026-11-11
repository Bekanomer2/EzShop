namespace EzShop.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
