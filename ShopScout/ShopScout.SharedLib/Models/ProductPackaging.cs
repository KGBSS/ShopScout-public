using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class ProductPackaging
{
    [Key]
    public int Id { get; set; }

    public bool? Recyclable { get; set; }

    [MaxLength(100)]
    public string? QuantityPerUnit { get; set; }

    // Navigation Property
    public virtual PackagingMaterial? Material { get; set; }

    public virtual PackagingPart? Part { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}