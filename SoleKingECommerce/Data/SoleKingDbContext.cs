using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Models;

namespace SoleKingECommerce.Data
{
    public class SoleKingDbContext : IdentityDbContext<User>
    {
        public SoleKingDbContext(DbContextOptions<SoleKingDbContext> options) : base(options) {}


        /* DB SETS */
        #region DB SETS
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<DiscountCondition> DiscountConditions { get; set; }
        public DbSet<ProductDiscount> ProductDiscounts { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherUsage> VoucherUsages { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Import> Imports { get; set; }
        public DbSet<ImportItem> ImportItems { get; set; }
        public DbSet<ImportReference> ImportReferences { get; set; }
        public DbSet<ImportReferenceItem> ImportReferenceItems { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Post> Posts { get; set; }

        // Các bảng cho Hệ thống Gợi ý bằng Jaccard
        public DbSet<UserPurchaseHistory> UserPurchaseHistories { get; set; }
        public DbSet<UserSimilarity> UserSimilarities { get; set; }
        public DbSet<UserProductRecommendation> UserProductRecommendations { get; set; }
        public DbSet<RecommendationBatch> RecommendationBatches { get; set; }
        #endregion DB SETS
        /* END DB SETS */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Xóa tiền tố AspNet khỏi tên bảng của các thực thể Identity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName()!;
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }

            /* CÁC BẢNG THỰC THỂ YẾU */
            #region CartItem
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => new { ci.CartId, ci.ProductVariantId });
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CartItem>()
               .HasOne(ci => ci.ProductVariant)
               .WithMany(pv => pv.CartItems)
               .HasForeignKey(ci => ci.ProductVariantId)
               .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => new { oi.OrderId, oi.ProductVariantId });
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant)
                .WithMany(pv => pv.OrderItems)
                .HasForeignKey(oi => oi.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
            #region ImportItem
            modelBuilder.Entity<ImportItem>()
                .HasKey(ii => new { ii.ImportId, ii.ProductVariantId });
            modelBuilder.Entity<ImportItem>()
                .HasOne(ii => ii.Import)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.ImportId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ImportItem>()
                .HasOne(ii => ii.ProductVariant)
                .WithMany(pv => pv.ImportItems)
                .HasForeignKey(ii => ii.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
            #region ProductReview
            modelBuilder.Entity<ProductReview>()
                .HasKey(pr => new { pr.ProductId, pr.UserId });
            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(pr => pr.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ProductReview>()
                .HasOne(pr => pr.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region Wishlist
            modelBuilder.Entity<Wishlist>()
                .HasKey(w => new { w.ProductId, w.UserId });
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region ProductDiscount
            modelBuilder.Entity<ProductDiscount>()
                .HasKey(pd => new { pd.ProductId, pd.DiscountId });

            modelBuilder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Product)
                .WithMany(p => p.ProductDiscounts)
                .HasForeignKey(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Discount)
                .WithMany(d => d.ProductDiscounts)
                .HasForeignKey(pd => pd.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            /* CÁC BẢNG QUAN HỆ 1-N */
            #region Category - Product
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
            #region Product - ProductVariant
            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region Color - ProductVariant
            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Color)
                .WithMany(c => c.ProductVariants)
                .HasForeignKey(pv => pv.ColorId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
            #region Product - ProductImage
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region User - Cart
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region User - Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
            #region Order - Transaction
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Order)
                .WithMany(o => o.Transactions)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region Supplier - Import
            modelBuilder.Entity<Import>()
                .HasOne(i => i.Supplier)
                .WithMany(s => s.Imports)
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
            #region User - Import
            modelBuilder.Entity<Import>()
                .HasOne(i => i.User)
                .WithMany(u => u.Imports)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
            #region Discount - DiscountCondition
            modelBuilder.Entity<DiscountCondition>()
                .HasOne(dc => dc.Discount)
                .WithMany()
                .HasForeignKey(dc => dc.DiscountId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region Product - DiscountCondition
            modelBuilder.Entity<DiscountCondition>()
                .HasOne(dc => dc.Product)
                .WithMany(p => p.DiscountConditions)
                .HasForeignKey(dc => dc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region Voucher - VoucherUsage
            modelBuilder.Entity<VoucherUsage>()
                .HasOne(vu => vu.Voucher)
                .WithMany(v => v.VoucherUsages)
                .HasForeignKey(vu => vu.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region Order - VoucherUsage
            modelBuilder.Entity<VoucherUsage>()
                .HasOne(vu => vu.Order)
                .WithMany(o => o.VoucherUsages)
                .HasForeignKey(vu => vu.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region Product - Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Product)
                .WithMany(pr => pr.Posts)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region User - Post
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion

            // PHÂN CẤP DANH MỤC (Parent-Child)
            modelBuilder.Entity<Category>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            /* THIẾT LẬP GIÁ TRỊ CHO CÁC DECIMAL Ở CÁC BẢNG */
            #region Decimal
            modelBuilder.Entity<Product>()
                .Property(p => p.BasePrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ProductVariant>()
                .Property(pv => pv.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ImportItem>()
                .Property(ii => ii.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Import>()
                .Property(i => i.TotalPrice)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Voucher>()
                .Property(v => v.DiscountPercent)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<Voucher>()
                .Property(v => v.MaxDiscount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Voucher>()
                .Property(v => v.MinOrderAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<DiscountCondition>()
                .Property(dc => dc.PercentDiscount)
                .HasColumnType("decimal(5,2)");
            #endregion

            /* ĐÁNH INDEX */
            #region Indexes
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<Post>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique();

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.SKU)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            #endregion

            /* CÁC BẢNG CHO HỆ THỐNG GỢI Ý BẰNG JACCARD */
            #region UserPurchaseHistory
            modelBuilder.Entity<UserPurchaseHistory>()
                .HasKey(h => h.Id);

            modelBuilder.Entity<UserPurchaseHistory>()
                .HasIndex(h => new { h.UserId, h.ProductId })
                .IsUnique(); // Mỗi user chỉ có 1 record cho 1 product

            modelBuilder.Entity<UserPurchaseHistory>()
                .HasIndex(h => h.ProductId); // Query reverse lookup

            modelBuilder.Entity<UserPurchaseHistory>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPurchaseHistory>()
                .HasOne(h => h.Product)
                .WithMany()
                .HasForeignKey(h => h.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region UserSimilarity
            modelBuilder.Entity<UserSimilarity>()
                .HasKey(s => new { s.UserId1, s.UserId2 });

            modelBuilder.Entity<UserSimilarity>()
                .HasOne(s => s.User1)
                .WithMany()
                .HasForeignKey(s => s.UserId1)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserSimilarity>()
                .HasOne(s => s.User2)
                .WithMany()
                .HasForeignKey(s => s.UserId2)
                .OnDelete(DeleteBehavior.Restrict);
            #endregion
            #region UserProductRecommendation
            modelBuilder.Entity<UserProductRecommendation>()
                .HasKey(r => r.Id);

            modelBuilder.Entity<UserProductRecommendation>()
                .HasIndex(r => new { r.ForUserId, r.ExpiresAt }); // Query active recommendations

            modelBuilder.Entity<UserProductRecommendation>()
                .HasIndex(r => r.Score); // Order by score

            modelBuilder.Entity<UserProductRecommendation>()
                .HasOne(r => r.ForUser)
                .WithMany()
                .HasForeignKey(r => r.ForUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProductRecommendation>()
                .HasOne(r => r.RecommendedProduct)
                .WithMany()
                .HasForeignKey(r => r.RecommendedProductId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
            #region Decimal
            modelBuilder.Entity<UserSimilarity>()
                .Property(s => s.JaccardScore)
                .HasColumnType("decimal(5,4)"); // 0.0000 - 1.0000
            modelBuilder.Entity<UserProductRecommendation>()
                .Property(r => r.Score)
                .HasColumnType("decimal(10,4)"); // Accumulated score có thể > 1
            #endregion

            #region Bổ sung thêm cho nhập hàng
            modelBuilder.Entity<ImportReference>()
                .HasOne(ir => ir.FromImport)
                .WithMany()
                .HasForeignKey(ir => ir.FromImportId)
                .OnDelete(DeleteBehavior.Restrict); // tránh xóa lan truyền

            modelBuilder.Entity<ImportReference>()
                .HasOne(ir => ir.ToImport)
                .WithMany()
                .HasForeignKey(ir => ir.ToImportId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ImportReferenceItem>()
                .HasOne(iri => iri.ImportReference)
                .WithMany(ir => ir.Items)
                .HasForeignKey(iri => iri.ImportReferenceId);

            modelBuilder.Entity<ImportReferenceItem>()
                .HasOne(iri => iri.ProductVariant)
                .WithMany()
                .HasForeignKey(iri => iri.ProductVariantId);
            #endregion
        }
    }
}
