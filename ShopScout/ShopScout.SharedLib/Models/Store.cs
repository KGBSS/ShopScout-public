using ShopScout.SharedLib.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopScout.SharedLib.Models;

public class Store : IVerifiable
{
    public int Id { get; set; }
    public long? OsmId { get; set; }
    public string Latitude { get; set; }
    public string Longitude { get; set; }
    public string? Address { get; set; }
    public bool Verified { get; set; } = false;
    public string? OpeningHours { get; set; }

    //[ForeignKey(nameof(Layout))]
    //public int? LayoutId { get; set; }
    public virtual LayoutObject? Layout { get; set; }

    // Navigation Property
    public virtual StoreBrand? StoreBrand { get; set; }
    public virtual ICollection<ProductPerStore> ProductPerStore { get; set; } = new List<ProductPerStore>();
    public virtual ICollection<ApplicationUser> FavoritedBy { get; set; } = new List<ApplicationUser>();
    public virtual ICollection<StoreStoreAttribute> StoreAttributes { get; set; } = new List<StoreStoreAttribute>();
    public virtual ICollection<StoreChange> Changes { get; set; } = new List<StoreChange>();
}

public class StoreAttribute
{
    public int Id { get; set; }
    public string Name { get; set; }
    public StoreAttributeType Type { get; set; }

    // Navigation Properties
    public virtual ICollection<StoreStoreAttribute> Stores { get; set; } = new List<StoreStoreAttribute>();
}

public class StoreStoreAttribute
{
    // Composite Key: StoreAttributeId + StoreId
    public int StoreAttributeId { get; set; }
    public int StoreId { get; set; }

    public virtual Store Store { get; set; }
    public virtual StoreAttribute StoreAttribute { get; set; }
    public string? Value { get; set; }
}

public enum StoreAttributeType : byte
{
    Payment,
    Contact,
    Other
}