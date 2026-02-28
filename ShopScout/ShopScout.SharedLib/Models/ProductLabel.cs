using ShopScout.SharedLib.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class ProductLabel : INToNTable
{
    [Key]
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = "";

    // Navigation Property
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
