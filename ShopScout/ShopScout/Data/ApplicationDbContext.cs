using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShopScout.SharedLib.Models;

namespace ShopScout.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options), IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDetails> ProductDetails { get; set; }
        public DbSet<ProductIngredient> ProductIngredients { get; set; }
        public DbSet<ProductAllergen> ProductAllergens { get; set; }
        public DbSet<ProductAdditive> ProductAdditives { get; set; }
        public DbSet<ProductLabel> ProductLabels { get; set; }
        public DbSet<ProductPackaging> ProductPackagings { get; set; }
        public DbSet<PackagingMaterial> PackagingMaterials { get; set; }
        public DbSet<PackagingPart> PackagingParts { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<StoreBrand> StoreBrands { get; set; }
        public DbSet<Wall> Walls { get; set; }
        public DbSet<Shelf> Shelves { get; set; }
        public DbSet<LayoutObject> LayoutObjects { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductCountry> ProductCountries { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Change> Changes { get; set; }
        public DbSet<ReviewQueue> ReviewQueue { get; set; }
        public DbSet<StoreAttribute> StoreAttributes { get; set; }
        public DbSet<ProductPerStore> ProductPerStore { get; set; }
        public DbSet<ProductProductIngredient> ProductProductIngredient { get; set; }
        public DbSet<StoreStoreAttribute> StoreStoreAttribute { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<City>().HasIndex(c => c.Name);
            modelBuilder.Entity<ProductCategory>().HasIndex(c => c.Name);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.ProductName);

            // Product - Store pivot table configuration
            modelBuilder.Entity<ProductPerStore>()
                .HasKey(ps => new { ps.ProductId, ps.StoreId});

            modelBuilder.Entity<ProductPerStore>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductPerStore)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductPerStore>()
                .HasOne(pps => pps.Store)
                .WithMany(s => s.ProductPerStore)
                .HasForeignKey(pps => pps.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductPerStore>()
                .HasOne(pps => pps.Shelf)
                .WithMany(s => s.Products)
                .HasForeignKey(pps => pps.ShelfId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Product - Ingredient pivot table configuration
            modelBuilder.Entity<ProductProductIngredient>()
                .HasKey(ps => new { ps.ProductId, ps.IngredientId });

            modelBuilder.Entity<ProductProductIngredient>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductIngredients)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductProductIngredient>()
                .HasOne(pps => pps.Ingredient)
                .WithMany(s => s.Products)
                .HasForeignKey(pps => pps.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Store - StoreAttribute pivot table configuration
            modelBuilder.Entity<StoreStoreAttribute>()
                .HasKey(ps => new { ps.StoreAttributeId, ps.StoreId });

            modelBuilder.Entity<StoreStoreAttribute>()
                .HasOne(ps => ps.StoreAttribute)
                .WithMany(p => p.Stores)
                .HasForeignKey(ps => ps.StoreAttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoreStoreAttribute>()
                .HasOne(pps => pps.Store)
                .WithMany(s => s.StoreAttributes)
                .HasForeignKey(pps => pps.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1-to-1 relationship between Store and LayoutObject
            modelBuilder.Entity<Store>()
                .HasOne(s => s.Layout)
                .WithOne(l => l.Store)
                .HasForeignKey<LayoutObject>(l => l.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed Roles
            modelBuilder.Entity<Change>()
                .HasDiscriminator<ChangeEntityType>("Discriminator")
                .HasValue<Change>(ChangeEntityType.None)
                .HasValue<StoreChange>(ChangeEntityType.Store)
                .HasValue<ProductChange>(ChangeEntityType.Product);

            // Configure StoreChange relationship
            modelBuilder.Entity<StoreChange>()
                .HasOne(c => c.Store)
                .WithMany(s => s.Changes)
                .HasForeignKey(c => c.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ProductChange relationship
            modelBuilder.Entity<ProductChange>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Changes)
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "280c3565-042b-4d59-a314-7793fb8692f6",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "90ed46af-b8fa-4468-8371-cada0946e537",
                    Name = "User",
                    NormalizedName = "USER"
                }
            );
        }
    }
}