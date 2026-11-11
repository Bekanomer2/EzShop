using EzShop.Domain.Entities;
using EzShop.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EzShop.DAL.Context;

public class EzShopDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public EzShopDbContext(DbContextOptions<EzShopDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<UserFavorite> Favorites => Set<UserFavorite>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Бренды
        modelBuilder.Entity<Brand>(b =>
        {
            b.ToTable("brands");
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Name).HasColumnName("name");
            b.Property(x => x.Country).HasColumnName("country");
        });

        // 2. Смартфоны
        modelBuilder.Entity<Product>(p =>
        {
            p.ToTable("products");
            p.Property(x => x.Id).HasColumnName("id");
            p.Property(x => x.Name).HasColumnName("name");
            p.Property(x => x.Brand).HasColumnName("brand");
            p.Property(x => x.Price).HasColumnName("price");
            p.Property(x => x.Img).HasColumnName("img");
            p.Property(x => x.Ram).HasColumnName("ram");
            p.Property(x => x.Storage).HasColumnName("storage");
            p.Property(x => x.Color).HasColumnName("color");
            p.Property(x => x.Display).HasColumnName("display");
            p.Property(x => x.Battery).HasColumnName("battery");
            p.Property(x => x.Description).HasColumnName("description");
            p.Property(x => x.InStock).HasColumnName("in_stock");
            p.Property(x => x.Rating).HasColumnName("rating");
        });

        // 3. Отзывы
        modelBuilder.Entity<Review>(r =>
        {
            r.ToTable("reviews");
            r.Property(x => x.Id).HasColumnName("id");
            r.Property(x => x.ProductId).HasColumnName("product_id");
            r.Property(x => x.Author).HasColumnName("author");
            r.Property(x => x.Text).HasColumnName("text");
            r.Property(x => x.Rating).HasColumnName("rating");
            r.Property(x => x.CreatedAt).HasColumnName("created_at");

            r.HasOne<Product>()
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Заказы и элементы заказов
        var statusConverter = new ValueConverter<OrderStatus, string>(
            v => ToStatusString(v),
            v => FromStatusString(v)
        );

        modelBuilder.Entity<Order>(o =>
        {
            o.ToTable("orders");
            o.Property(x => x.Id).HasColumnName("id");
            o.Property(x => x.UserId).HasColumnName("user_order_id");
            o.Ignore(x => x.Username);
            o.Property(x => x.Total).HasColumnName("total").HasConversion<double>();
            o.Property(x => x.CustomerName).HasColumnName("customer_name");
            o.Property(x => x.CustomerPhone).HasColumnName("customer_phone");
            o.Property(x => x.CustomerAddress).HasColumnName("customer_address");
            o.Property(x => x.CreatedAt).HasColumnName("created_at");
            o.Property(x => x.Status).HasColumnName("status").HasConversion(statusConverter);
            o.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId);
        });

        modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.ToTable("order_items");
            oi.Property(x => x.Id).HasColumnName("id");
            oi.Property(x => x.OrderId).HasColumnName("order_id");
            oi.Property(x => x.ProductId).HasColumnName("product_id");
            oi.Property(x => x.Name).HasColumnName("name");
            oi.Property(x => x.Price).HasColumnName("price");
            oi.Property(x => x.Qty).HasColumnName("qty");
        });

        // 5. Избранное пользователей
        modelBuilder.Entity<UserFavorite>(f =>
        {
            f.ToTable("user_favorites");
            f.Property(x => x.Id).HasColumnName("id");
            f.Property(x => x.UserId).HasColumnName("user_id");
            f.Property(x => x.ProductId).HasColumnName("product_id");
            f.Property(x => x.CreatedAt).HasColumnName("created_at");

            f.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();

            f.HasOne<Product>()
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // 6. Корзина пользователей
        modelBuilder.Entity<CartItem>(c =>
        {
            c.ToTable("cart_items");
            c.Property(x => x.Id).HasColumnName("id");
            c.Property(x => x.UserId).HasColumnName("user_id");
            c.Property(x => x.ProductId).HasColumnName("product_id");
            c.Property(x => x.Qty).HasColumnName("qty");
            c.Property(x => x.CreatedAt).HasColumnName("created_at");
            c.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            c.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();

            c.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static string ToStatusString(OrderStatus s) => s switch
    {
        OrderStatus.Pending => "В обработке",
        OrderStatus.InProgress => "В работе",
        OrderStatus.Delivered => "Доставлен",
        OrderStatus.Returned => "Возврат",
        OrderStatus.Cancelled => "Отменен",
        _ => "В обработке"
    };

    private static OrderStatus FromStatusString(string s) => s switch
    {
        "Доставлен" or "Delivered" => OrderStatus.Delivered,
        "Возврат" or "Returned" => OrderStatus.Returned,
        "Отменен" or "Cancelled" => OrderStatus.Cancelled,
        "В работе" or "InProgress" => OrderStatus.InProgress,
        "В обработке" or "Pending" => OrderStatus.Pending,
        _ => OrderStatus.Pending
    };
}
