using EzShop.Domain.Entities;

namespace EzShop.Domain.Interfaces;

public interface IBrandRepository
{
    Task<IReadOnlyList<Brand>> GetAllAsync();
    Task<Brand?> GetByIdAsync(int id);
    Task<Brand> AddAsync(Brand brand);
    Task<bool> DeleteAsync(int id);
}
