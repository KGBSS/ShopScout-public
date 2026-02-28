using System.Collections.Generic;

namespace ShopScout.SharedLib.Models;

public class ProductFilterParams
{
    public bool? FromArfigyelo { get; set; }
    public List<int> AllergenIds { get; set; } = new();
    public List<int> LabelIds { get; set; } = new();
    public List<int> AttributeIds { get; set; } = new();
    public List<int> BrandIds { get; set; } = new();
    public List<int> CountryIds { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
    public List<int> StoreBrandIds { get; set; } = new();

    public bool IsEmpty =>
        FromArfigyelo == null &&
        AllergenIds.Count == 0 &&
        LabelIds.Count == 0 &&
        AttributeIds.Count == 0 &&
        BrandIds.Count == 0 &&
        CountryIds.Count == 0 &&
        CategoryIds.Count == 0 &&
        StoreBrandIds.Count == 0;
}

