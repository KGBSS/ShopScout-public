using ShopScout.SharedLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models
{
    public class StoreBrand : INToNTable, IVerifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Verified { get; set; } = false;

        public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}