using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ShopScout.SharedLib.Models;

public class Change
{
    public int Id { get; set; }
    public ApplicationUser User { get; set; }
    public string ChangeJson { get; set; }
    public DateTime ChangeRequestedAt { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class StoreChange : Change
{
    public int StoreId { get; set; }
    public virtual Store Store { get; set; }
}

public class ProductChange : Change
{
    public int ProductId { get; set; }
    public virtual Product Product { get; set; }
}

public enum ChangeEntityType : byte
{
    None = 0,      // For the base class
    Store = 1,
    Product = 2
}

public class ReviewQueue
{
    public int Id { get; set; }
    public Change Change { get; set; }
    public IdentityRole RequiredRole { get; set; }
}