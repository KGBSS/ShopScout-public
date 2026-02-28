using ShopScout.SharedLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class ProductCategory : INToNTable, IVerifiable
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Verified { get; set; } = false;
    public ProductCategory? ParentCategory { get; set; }

    // Navigation Property
    [JsonIgnore]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public virtual ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();
}