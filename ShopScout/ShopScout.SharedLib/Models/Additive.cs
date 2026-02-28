namespace ShopScout.SharedLib.Models;

public class Additive
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public AdditiveRiskLevel Risk { get; set; }
}

public enum AdditiveRiskLevel
{
    Low,
    Medium,
    High,
    Unknown
}
