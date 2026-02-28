using ShopScout.SharedLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class ProductIngredient : INToNTable
{
    public int Id { get; set; }

    public string Name { get; set; }

    // Navigation Property
    public virtual ICollection<ProductProductIngredient> Products { get; set; } = new List<ProductProductIngredient>();
}