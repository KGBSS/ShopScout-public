using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ShopScout.Data;
using ShopScout.Services;
using ShopScout.SharedLib.Models;
using System.Text.Json;
using Xunit;

namespace ShopScout.Tests;

public class ArfigyeloFetchServiceTests
{
    [Fact]
    public void MapCategoriesToIds_ShouldReturnCorrectIds_WhenCategoriesExist()
    {
        // Arrange
        var json = @"
        {
            ""categories"": [
                {
                    ""id"": 1,
                    ""name"": ""Tejtermékek"",
                    ""categoryNodes"": [
                        { ""id"": 10, ""name"": ""Sajt"", ""categoryNodes"": [] },
                        { ""id"": 11, ""name"": ""Tej"", ""categoryNodes"": [] }
                    ]
                },
                {
                    ""id"": 2,
                    ""name"": ""Pékáru"",
                    ""categoryNodes"": []
                }
            ]
        }";
        var requestedNames = new List<string> { "Sajt", "Pékáru", "NemLétező" };

        // Act
        var result = ArfigyeloFetchService.MapCategoriesToIds(requestedNames, json);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey(10));
        Assert.Equal("Sajt", result[10]);
        Assert.True(result.ContainsKey(2));
        Assert.Equal("Pékáru", result[2]);
    }

    [Fact]
    public void MapCategoriesToIds_ShouldMatchCaseInsensitive()
    {
        // Arrange
        var json = @"{ ""categories"": [ { ""id"": 1, ""name"": ""Tej"", ""categoryNodes"": [] } ] }";
        var requestedNames = new List<string> { "tEj" };

        // Act
        var result = ArfigyeloFetchService.MapCategoriesToIds(requestedNames, json);

        // Assert
        Assert.Single(result);
        Assert.True(result.ContainsKey(1));
    }

    [Fact]
    public void GetIdsFromJsonString_ShouldExtractIdsAtCorrectDepth()
    {
        // Arrange
        // A kód 3-as mélységet vár (CurrentDepth == 3) a JSON readerben.
        var json = @"
        [
            {
                ""wrapper"": {
                    ""id"": ""product123"",
                    ""name"": ""Test Product""
                }
            },
            {
                ""wrapper"": {
                    ""id"": ""product456""
                }
            }
        ]";

        // Act
        var result = ArfigyeloFetchService.GetIdsFromJsonString(json);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains("product123", result);
        Assert.Contains("product456", result);
    }

    [Fact]
    public void UpdateEntitiesFromApiData_ShouldUpdatePrices_WhenPricesChange()
    {
        // Arrange
        var dbEntities = new List<ProductPerStore>
        {
            new ProductPerStore
            {
                Price = 1000,
                DiscountedPrice = 900,
                Product = new Product { Code = "PROD-1" },
                Store = new Store { StoreBrand = new StoreBrand { Name = "Tesco" } }
            }
        };

        var apiDataDict = new Dictionary<string, ArfigyeloFetchService.ApiProductResponse>
        {
            {
                "PROD-1", new ArfigyeloFetchService.ApiProductResponse
                {
                    Id = "PROD-1",
                    ChainStores = new List<ArfigyeloFetchService.ApiChainStore>
                    {
                        new ArfigyeloFetchService.ApiChainStore
                        {
                            Name = "Tesco",
                            Prices = new List<ArfigyeloFetchService.ApiPrice>
                            {
                                new ArfigyeloFetchService.ApiPrice { Type = "NORMAL", Amount = 1200 }, // Változott
                                new ArfigyeloFetchService.ApiPrice { Type = "DISCOUNTED", Amount = 850 } // Változott
                            }
                        }
                    }
                }
            }
        };

        // Act
        int changesCount = ArfigyeloFetchService.UpdateEntitiesFromApiData(dbEntities, apiDataDict);

        // Assert
        Assert.Equal(1, changesCount);
        Assert.Equal(1200, dbEntities[0].Price);
        Assert.Equal(850, dbEntities[0].DiscountedPrice);
    }

    [Fact]
    public void UpdateEntitiesFromApiData_ShouldNotCountAsChange_WhenPricesAreSame()
    {
        // Arrange
        var dbEntities = new List<ProductPerStore>
        {
            new ProductPerStore
            {
                Price = 1000,
                DiscountedPrice = 900,
                Product = new Product { Code = "PROD-1" },
                Store = new Store { StoreBrand = new StoreBrand { Name = "Spar" } }
            }
        };

        var apiDataDict = new Dictionary<string, ArfigyeloFetchService.ApiProductResponse>
        {
            {
                "PROD-1", new ArfigyeloFetchService.ApiProductResponse
                {
                    Id = "PROD-1",
                    ChainStores = new List<ArfigyeloFetchService.ApiChainStore>
                    {
                        new ArfigyeloFetchService.ApiChainStore
                        {
                            Name = "Spar",
                            Prices = new List<ArfigyeloFetchService.ApiPrice>
                            {
                                new ArfigyeloFetchService.ApiPrice { Type = "NORMAL", Amount = 1000 },
                                new ArfigyeloFetchService.ApiPrice { Type = "DISCOUNTED", Amount = 900 }
                            }
                        }
                    }
                }
            }
        };

        // Act
        int changesCount = ArfigyeloFetchService.UpdateEntitiesFromApiData(dbEntities, apiDataDict);

        // Assert
        Assert.Equal(0, changesCount); // Nem volt módosítás
    }

    [Fact]
    public void Service_CanBeInstantiated_WithMocks()
    {
        // Arrange
        var mockHttpClient = new HttpClient();
        var mockContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
        var mockLogger = new Mock<ILogger<ArfigyeloFetchService>>();

        // Act
        var service = new ArfigyeloFetchService(mockHttpClient, mockContextFactory.Object, mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }
}