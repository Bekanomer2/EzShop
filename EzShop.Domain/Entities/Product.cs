namespace EzShop.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Img { get; set; } = string.Empty;
    public int Ram { get; set; }
    public int Storage { get; set; }
    public string Color { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public int Battery { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool InStock { get; set; } = true;
    public double Rating { get; set; } = 5.0;
}
