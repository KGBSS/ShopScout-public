using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class ProductDetails
{
    [Key]
    public int Id { get; set; }
    [MaxLength(100)]
    [ForeignKey("Product")]
    public int ProductId { get; set; }
    public string? Quantity { get; set; }
    [MaxLength(4000)]
    public string? IngredientsText { get; set; }
    public NutriScore NutriScore { get; set; }
    public byte NovaGroup { get; set; }
    public double? EnergyKcal { get; set; }
    public double? Fat { get; set; }
    public double? SaturatedFat { get; set; }
    public double? Carbohydrates { get; set; }
    public double? Sugars { get; set; }
    public double? Proteins { get; set; }
    public double? Salt { get; set; }
    [MaxLength(100)]
    public string? ServingSize { get; set; }
    public NutrientLevel? FatLevel { get; set; }
    public NutrientLevel? SaturatedFatLevel { get; set; }
    public NutrientLevel? SaltLevel { get; set; }
    public NutrientLevel? SugarsLevel { get; set; }
    // Navigation Property
    public virtual Product Product { get; set; } = null!;
}

public enum NutriScore : byte
{
    Unknown, A, B, C, D, E
}