using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models
{
    public class LayoutObject
    {
        public int Id { get; set; }

        public int EntranceX1 { get; set; }
        public int EntranceY1 { get; set; }
        public int EntranceX2 { get; set; }
        public int EntranceY2 { get; set; }

        [ForeignKey(nameof(Store))]
        public int StoreId { get; set; }
        public virtual Store Store { get; set; } = null!;

        public virtual ICollection<Wall> Walls { get; set; } = new List<Wall>();
        public virtual ICollection<Shelf> Shelves { get; set; } = new List<Shelf>();
    }
}
