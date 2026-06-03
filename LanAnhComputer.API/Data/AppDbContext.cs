using LanAnhComputer.API.Data.Entities;
using LanAnhComputer.Data.Entities;
using Microsoft.EntityFrameworkCore;
// Connect Database và Sql Server thông qua Entity Framework Core (EF Core) 
namespace LanAnhComputer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
        public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
        public DbSet<ChatbotHistory> ChatbotHistories => Set<ChatbotHistory>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        protected override void OnModelCreating(ModelBuilder modelBuilder) // fluent API để cấu hình chi tiết hơn về cách ánh xạ giữa các class và bảng trong cơ sở dữ liệu, bao gồm ràng buộc, chỉ mục, kiểu dữ liệu, v.v.
        {
            base.OnModelCreating(modelBuilder);
            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(x => x.CategoryId);
                entity.Property(x => x.CategoryCode).HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(500); //định dạng kiểu dữ liệu HasMaxLength(255), IsUnicode(false) (biến string thành varchar thay vì nvarchar)
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(x => x.CategoryCode).IsUnique(); // đảm bảo mã danh mục không trùng lặp (duy nhất)
                entity.HasOne(x => x.ParentCategory)
                      .WithMany(x => x.ChildCategories)
                      .HasForeignKey(x => x.ParentCategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(x => x.UserId);
                entity.Property(x => x.FullName).HasMaxLength(150).IsRequired(); // Tương ứng với not null trong SQL Server
                entity.Property(x => x.Email).HasMaxLength(255).IsUnicode(false).IsRequired();
                entity.Property(x => x.Phone).HasMaxLength(20).IsUnicode(false);
                entity.Property(x => x.PasswordHash).HasMaxLength(255).IsUnicode(false).IsRequired();
                entity.Property(x => x.Role).HasMaxLength(20).IsUnicode(false).HasDefaultValue("Customer");
                entity.Property(x => x.Gender).HasMaxLength(10).IsUnicode(false);
                entity.Property(x => x.AddressLine).HasMaxLength(255);
                entity.Property(x => x.Ward).HasMaxLength(100);
                entity.Property(x => x.District).HasMaxLength(100);
                entity.Property(x => x.City).HasMaxLength(100);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(x => x.Email).IsUnique();
                entity.HasIndex(x => x.Phone).IsUnique().HasFilter("[Phone] IS NOT NULL");
                entity.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Admin','Staff','Customer')");
                entity.HasCheckConstraint("CK_Users_Gender", "[Gender] IS NULL OR [Gender] IN ('Male','Female','Other')");
            });
            // Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(x => x.ProductId);
                entity.Property(x => x.ProductCode).HasMaxLength(50).IsUnicode(false).IsRequired();
                entity.Property(x => x.ProductName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.ProductType).HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(x => x.Brand).HasMaxLength(100);
                entity.Property(x => x.Model).HasMaxLength(100);
                entity.Property(x => x.ShortDescription).HasMaxLength(500);
                entity.Property(x => x.Description);
                entity.Property(x => x.ThumbnailUrl).HasMaxLength(500);
                entity.Property(x => x.WarrantyMonths).HasDefaultValue(0);
                entity.Property(x => x.CostPrice).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(x => x.SalePrice).HasColumnType("decimal(18,2)");
                entity.Property(x => x.StockQuantity).HasDefaultValue(0);
                entity.Property(x => x.ReorderLevel).HasDefaultValue(0);
                entity.Property(x => x.ImageUrl).HasMaxLength(500);
                entity.Property(x => x.ViewCount).HasDefaultValue(0);
                entity.Property(x => x.AverageRating).HasColumnType("float").HasDefaultValue(0);
                entity.Property(x => x.TotalReviews).HasDefaultValue(0);
                entity.Property(x => x.SoldQuantity).HasDefaultValue(0);
                entity.Property(x => x.LowStockThreshold).HasDefaultValue(0);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(x => x.ProductCode).IsUnique();
                entity.HasIndex(x => x.CategoryId);
                entity.HasIndex(x => x.ProductType);
                entity.HasOne(x => x.Category)
                      .WithMany(x => x.Products)
                      .HasForeignKey(x => x.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasCheckConstraint("CK_Products_ProductType", "[ProductType] IN ('Computer','Component')");
            });
            // ProductImage
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(x => x.ProductImageId);
                entity.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
                entity.Property(x => x.AltText).HasMaxLength(255);
                entity.Property(x => x.IsPrimary).HasDefaultValue(false);
                entity.Property(x => x.SortOrder).HasDefaultValue(0);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(x => x.ProductId);
                entity.HasOne(x => x.Product)
                      .WithMany(x => x.ProductImages)
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // ProductSpecification
            modelBuilder.Entity<ProductSpecification>(entity =>
            {
                entity.ToTable("ProductSpecifications");
                entity.HasKey(x => x.ProductSpecificationId);
                entity.Property(x => x.GroupName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.SpecKey).HasMaxLength(100).IsRequired();
                entity.Property(x => x.SpecValue).HasMaxLength(500).IsRequired();
                entity.Property(x => x.SortOrder).HasDefaultValue(0);
                entity.HasIndex(x => new { x.ProductId, x.SpecKey });
                entity.HasOne(x => x.Product)
                      .WithMany(x => x.ProductSpecifications)
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // ProductReview
            modelBuilder.Entity<ProductReview>(entity =>
            {
                entity.ToTable("ProductReviews");
                entity.HasKey(x => x.ProductReviewId);
                entity.Property(x => x.Rating).IsRequired();
                entity.Property(x => x.Comment).HasMaxLength(1000);
                entity.Property(x => x.IsVisible).HasDefaultValue(true);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(x => new { x.ProductId, x.UserId }).IsUnique();
                entity.HasIndex(x => new { x.ProductId, x.IsVisible });
                entity.HasOne(x => x.Product)
                      .WithMany(x => x.ProductReviews)
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasCheckConstraint("CK_ProductReviews_Rating", "[Rating] BETWEEN 1 AND 5");
            });
            // Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(x => x.OrderId);
                entity.Property(x => x.OrderCode).HasMaxLength(30).IsUnicode(false).IsRequired();
                entity.Property(x => x.OrderDate).HasDefaultValueSql("SYSDATETIME()");
                entity.Property(x => x.OrderStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValue(LanAnhComputer.Constants.OrderStatuses.Pending);
                entity.Property(x => x.PaymentMethod).HasMaxLength(20).IsUnicode(false).HasDefaultValue("COD");
                entity.Property(x => x.PaymentStatus).HasMaxLength(20).IsUnicode(false).HasDefaultValue(LanAnhComputer.Constants.PaymentStatuses.Pending);
                entity.Property(x => x.ShippingFullName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.ShippingPhone).HasMaxLength(20).IsUnicode(false).IsRequired();
                entity.Property(x => x.ShippingAddressLine).HasMaxLength(255).IsRequired();
                entity.Property(x => x.ShippingWard).HasMaxLength(100);
                
                entity.Property(x => x.ShippingProvince).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Note).HasMaxLength(500);
                entity.Property(x => x.SubTotal).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(x => x.ShippingFee).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(x => x.OrderCode).IsUnique();
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.OrderDate);
                entity.HasOne(x => x.User)
                      .WithMany(x => x.Orders)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetails");
                entity.HasKey(x => x.OrderDetailId);
                entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(x => x.Quantity).IsRequired();
                entity.Property(x => x.DiscountPercent).HasColumnType("decimal(5,2)").HasDefaultValue(0);
                entity.Property(x => x.LineTotal).HasColumnType("decimal(18,2)");
                entity.HasIndex(x => x.OrderId);
                entity.HasIndex(x => x.ProductId);
                entity.HasIndex(x => new { x.OrderId, x.ProductId }).IsUnique();
                entity.HasOne(x => x.Order)
                      .WithMany(x => x.OrderDetails)
                      .HasForeignKey(x => x.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Product)
                      .WithMany(x => x.OrderDetails)
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            // ChatbotHistory
            modelBuilder.Entity<ChatbotHistory>(entity =>
            {
                entity.ToTable("ChatbotHistories");
                entity.HasKey(x => x.ChatHistoryId);
                entity.Property(x => x.SessionId).HasMaxLength(100).IsUnicode(false).IsRequired();
                entity.Property(x => x.UserMessage).IsRequired();
                entity.Property(x => x.BotResponse).IsRequired();
                entity.Property(x => x.Intent).HasMaxLength(100);
                entity.Property(x => x.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.SessionId);
                entity.HasIndex(x => x.CreatedAt);
                entity.HasOne(x => x.User)
                      .WithMany(x => x.ChatbotHistories)
                      .HasForeignKey(x => x.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.ProductSuggested)
                      .WithMany(x => x.ChatbotHistories)
                      .HasForeignKey(x => x.ProductIdSuggested)
                      .OnDelete(DeleteBehavior.SetNull);
            });
            // Cart
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Carts");
                entity.HasKey(x => x.CartId);

                entity.Property(x => x.CreatedAt)
                      .HasDefaultValueSql("SYSDATETIME()");

                entity.Property(x => x.UpdatedAt)
                      .IsRequired(false);

                entity.HasIndex(x => x.UserId).IsUnique(); // 1 user = 1 cart

                entity.HasOne(x => x.User)
                      .WithOne()
                      .HasForeignKey<Cart>(x => x.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItems");
                entity.HasKey(x => x.CartItemId);

                entity.Property(x => x.Quantity)
                      .HasDefaultValue(1);

                entity.Property(x => x.CreatedAt)
                      .HasDefaultValueSql("SYSDATETIME()");

                entity.Property(x => x.UpdatedAt)
                      .IsRequired(false);

                entity.HasIndex(x => x.CartId);
                entity.HasIndex(x => x.ProductId);

                entity.HasIndex(x => new { x.CartId, x.ProductId })
                      .IsUnique();

                entity.HasOne(x => x.Cart)
                      .WithMany(x => x.CartItems)
                      .HasForeignKey(x => x.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Product)
                      .WithMany()
                      .HasForeignKey(x => x.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
