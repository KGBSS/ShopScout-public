using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ShopScout.SharedLib.Models;

public class OpenFoodFactsProduct
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("product_name")]
    public string ProductName { get; set; } = "";

    [JsonPropertyName("generic_name")]
    public string? GenericName { get; set; }

    [JsonPropertyName("brands")]
    [JsonConverter(typeof(FlexibleStringListConverter))]
    public List<string>? Brands { get; set; }

    [JsonPropertyName("quantity")]
    public string? Quantity { get; set; }

    [JsonPropertyName("categories")]
    [JsonConverter(typeof(FlexibleStringListConverter))]
    public List<string>? Categories { get; set; }

    [JsonPropertyName("countries")]
    [JsonConverter(typeof(FlexibleStringListConverter))]
    public List<string>? Countries { get; set; }

    [JsonPropertyName("ingredients_text")]
    public string? IngredientsText { get; set; }

    [JsonPropertyName("nutriscore_grade")]
    public string? NutriScore { get; set; }

    [JsonPropertyName("nova_group")]
    public int NovaGroup { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("image_ingredients_url")]
    public string? ImageIngredientsUrl { get; set; }

    [JsonPropertyName("image_nutrition_url")]
    public string? ImageNutritionUrl { get; set; }

    [JsonPropertyName("image_packaging_url")]
    public string? ImagePackagingUrl { get; set; }

    [JsonPropertyName("serving_size")]
    public string? ServingSize { get; set; }

    [JsonPropertyName("nutriments")]
    public OpenFoodFactsNutriments? Nutriments { get; set; }

    [JsonPropertyName("nutrient_levels")]
    public Dictionary<string, string>? NutrientLevels { get; set; }

    [JsonPropertyName("ingredients")]
    public List<OpenFoodFactsIngredient>? Ingredients { get; set; }

    [JsonPropertyName("allergens_tags")]
    [JsonConverter(typeof(FlexibleStringListConverter))]
    public List<string>? Allergens { get; set; }

    [JsonPropertyName("additives_tags")]
    [JsonConverter(typeof(FlexibleStringListConverter))]
    public List<string>? Additives { get; set; }

    [JsonPropertyName("labels_tags")]
    [JsonConverter(typeof(FlexibleStringListConverter))]
    public List<string>? Labels { get; set; }

    [JsonPropertyName("packagings")]
    public List<OpenFoodFactsPackaging>? Packaging { get; set; }

    [JsonPropertyName("ingredients_analysis_tags")]
    [JsonConverter(typeof(FlexibleStringListConverter))]
    public List<string>? IngredientsAnalysisTags { get; set; }
}

public class OpenFoodFactsNutriments
{
    [JsonPropertyName("energy-kcal_100g")]
    public double? EnergyKcal { get; set; }

    [JsonPropertyName("fat_100g")]
    public double? Fat { get; set; }

    [JsonPropertyName("saturated-fat_100g")]
    public double? SaturatedFat { get; set; }

    [JsonPropertyName("carbohydrates_100g")]
    public double? Carbohydrates { get; set; }

    [JsonPropertyName("sugars_100g")]
    public double? Sugars { get; set; }

    [JsonPropertyName("proteins_100g")]
    public double? Proteins { get; set; }

    [JsonPropertyName("salt_100g")]
    public double? Salt { get; set; }
}

public class OpenFoodFactsIngredient
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("percent_estimate")]
    public double? PercentEstimate { get; set; }

    [JsonPropertyName("has_sub_ingredients")]
    public string? HasSubIngredients { get; set; }
}

public class OpenFoodFactsPackaging
{
    [JsonPropertyName("material")]
    public string? Material { get; set; }

    [JsonPropertyName("shape")]
    public string? Shape { get; set; }

    [JsonPropertyName("recycling")]
    public string? Recycling { get; set; }

    [JsonPropertyName("quantity_per_unit")]
    public string? QuantityPerUnit { get; set; }
}

public class OpenFoodFactsResponse
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("product")]
    public OpenFoodFactsProduct? Product { get; set; }
}

public class FlexibleStringListConverter : JsonConverter<List<string>?>
{
    public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            return string.IsNullOrEmpty(stringValue) ? new List<string>() : stringValue.Split(",").Select(x => x.Trim()).ToList();
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<string>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.String)
                {
                    var item = reader.GetString();
                    if (!string.IsNullOrEmpty(item))
                        list.Add(item);
                }
            }
            return list;
        }

        throw new JsonException($"Cannot convert {reader.TokenType} to List<string>");
    }

    public override void Write(Utf8JsonWriter writer, List<string>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }
        writer.WriteEndArray();
    }
}
