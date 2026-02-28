using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models
{
    public class Shelf
    {
        public int Id { get; set; }
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public ShelfType Type { get; set; } = ShelfType.Shelf;
        public ShelfSide Side { get; set; }

        public virtual ICollection<ProductPerStore> Products { get; set; } = null!;
        public virtual LayoutObject LayoutObject { get; set; } = null!;
    }

    public enum ShelfType
    {
        [Display(Name = "Polc")] Shelf,
        [Display(Name = "Hűtő, fagyasztó")] Fridge,
        [Display(Name = "Zöldség, gyümölcs")] Produce,
        [Display(Name = "Pékáru")] Bakery,
        [Display(Name = "Húspult")] Butcher,
    }

    public enum ShelfSide
    {
        [Display(Name = "Bal")] Left,
        [Display(Name = "Jobb")] Right,
        [Display(Name = "Mindkettő")] Both,
    }
}