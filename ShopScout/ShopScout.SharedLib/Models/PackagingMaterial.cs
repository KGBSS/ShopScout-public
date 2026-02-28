using ShopScout.SharedLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class PackagingMaterial : INToNTable
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Navigation Property
    public virtual ICollection<ProductPackaging> ProductPackagings { get; set; } = new List<ProductPackaging>();
}