using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class ProductImage
{
    public int Id { get; set; }
    public string? URL { get; set; }
    public ProductImageType ImageType { get; set; }
    public virtual Product Product { get; set; }
}

public enum ProductImageType
{
    Primary,
    Ingredients,
    Nutrition,
    Packaging
}