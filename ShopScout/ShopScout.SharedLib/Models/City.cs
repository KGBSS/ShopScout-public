using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }

    // Navigation Property
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}