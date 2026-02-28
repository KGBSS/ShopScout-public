using ShopScout.SharedLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class ProductCountry : INToNTable
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation Property
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}