using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopScout.SharedLib.Models;

public class ApplicationUser : IdentityUser
{
    public virtual DateTime LastLoginTime { get; set; }
    public virtual DateTime RegistrationDate { get; set; }
    public bool IsAnonymous { get; set; } = false;
    public bool IsBanned { get; set; } = false;

    // Language-Country
    public string LanguageCode { get; set; } = "hu";
    public string CountryCode { get; set; } = "hu";

    // Navigation properties
    public virtual ICollection<City> FavoriteCities { get; set; } = new List<City>();
    public virtual ICollection<ProductCategory> FavoriteCategories { get; set; } = new List<ProductCategory>();
    public virtual ICollection<Store> FavoriteStores { get; set; } = new List<Store>();
    public virtual ICollection<Product> FavoriteProducts { get; set; } = new List<Product>();

    [Obsolete("Not used", true)]
    [NotMapped]
    public override string? PhoneNumber { get => null; set { } }

    [Obsolete("Not used", true)]
    [NotMapped]
    public override bool PhoneNumberConfirmed { get => false; set { } }
}
