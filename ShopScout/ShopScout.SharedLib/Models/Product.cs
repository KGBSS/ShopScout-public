using ShopScout.SharedLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = "";

    public bool FromArfigyelo { get; set; } = false;

    [MaxLength(500)]
    public string ProductName { get; set; } = "";

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Navigation Properties
    public virtual ProductDetails? Details { get; set; }
    public virtual ICollection<ProductProductIngredient> ProductIngredients { get; set; } = new List<ProductProductIngredient>();
    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductAllergen> Allergens { get; set; } = new List<ProductAllergen>();
    public virtual ICollection<ProductAdditive> Additives { get; set; } = new List<ProductAdditive>();
    public virtual ICollection<ProductLabel> Labels { get; set; } = new List<ProductLabel>();
    public virtual ICollection<ProductPackaging> Packaging { get; set; } = new List<ProductPackaging>();
    public virtual ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    public virtual ICollection<ProductPerStore> ProductPerStore { get; set; } = new List<ProductPerStore>();
    public virtual ICollection<ProductBrand> Brands { get; set; } = new List<ProductBrand>();
    public virtual ICollection<ProductCountry> Countries { get; set; } = new List<ProductCountry>();
    public virtual ICollection<ProductCategory> Categories { get; set; } = new List<ProductCategory>();
    public virtual ICollection<ProductChange> Changes { get; set; } = new List<ProductChange>();
    public virtual ICollection<ApplicationUser> FavouritedBy { get; set; } = new List<ApplicationUser>();
    
}

public class ProductPerStore : IVerifiable
{
    // Composite Key: ProductId + StoreId
    public int ProductId { get; set; }
    public int StoreId { get; set; }
    public int? ShelfId { get; set; }
    public float? DistanceFromP1 { get; set; }

    public virtual Store Store { get; set; }
    public virtual Product Product { get; set; }
    public int? Price { get; set; }
    public int? DiscountedPrice { get; set; }
    public bool Verified { get; set; } = false;
    public virtual Shelf? Shelf { get; set; }
}

public class ProductProductIngredient
{
    // Composite Key: ProductId + IngredientId
    public int ProductId { get; set; }
    public int IngredientId { get; set; }

    public virtual ProductIngredient Ingredient { get; set; }
    public virtual Product Product { get; set; }
    public double? PercentEstimate { get; set; }
}

public enum NutrientLevel : byte
{
    low,
    moderate,
    high
}