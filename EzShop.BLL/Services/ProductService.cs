using EzShop.BLL.DTO;
using EzShop.BLL.Interfaces;
using EzShop.Domain.Entities;
using EzShop.Domain.Interfaces;

namespace EzShop.BLL.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;

    public ProductService(IProductRepository productRepo)
    {
        _productRepo = productRepo;
    }

    public async Task<IReadOnlyList<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepo.GetAllAsync();
        return products.Select(p => MapToDto(p)).ToList();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var p = await _productRepo.GetByIdAsync(id);
        return p == null ? null : MapToDto(p);
    }

    public async Task<ProductDto> CreateProductAsync(UpsertProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Brand = request.Brand,
            Price = request.Price,
            Img = request.Img,
            Ram = request.Ram,
            Storage = request.Storage,
            Color = request.Color,
            Display = request.Display,
            Battery = request.Battery,
            Description = request.Description,
            InStock = request.InStock
        };

        var created = await _productRepo.AddAsync(product);
        return MapToDto(created);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpsertProductRequest request)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product == null) return null;

        product.Name = request.Name;
        product.Brand = request.Brand;
        product.Price = request.Price;
        product.Img = request.Img;
        product.Ram = request.Ram;
        product.Storage = request.Storage;
        product.Color = request.Color;
        product.Display = request.Display;
        product.Battery = request.Battery;
        product.Description = request.Description;
        product.InStock = request.InStock;

        await _productRepo.UpdateAsync(product);
        return MapToDto(product);
    }

    public async Task<bool> ToggleStockAsync(int id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product == null) return false;

        product.InStock = !product.InStock;
        await _productRepo.UpdateAsync(product);
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product == null) return false;

        await _productRepo.DeleteAsync(id);
        return true;
    }

    private static ProductDto MapToDto(Product p) =>
        new(
            p.Id, p.Name, p.Brand, p.Price, p.Img,
            p.Ram, p.Storage, p.Color, p.Display,
            p.Battery, p.Description, p.InStock, p.Rating
        );
}
