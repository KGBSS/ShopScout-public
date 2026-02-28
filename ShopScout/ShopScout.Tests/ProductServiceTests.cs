using Microsoft.AspNetCore.Identity;
using Moq;
using ShopScout.Data;
using ShopScout.Services;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;
using Xunit;

namespace ShopScout.Tests;

public class ProductServiceTests
{
    private readonly Mock<ApplicationDbContext> _contextMock;

    public ProductServiceTests()
    {
        _contextMock = new Mock<ApplicationDbContext>();
    }

    // Helper class for testing
    public class TestNToNTable : INToNTable
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void AttachToCollection_WithValidStrings_AddsFormattedItems()
    {
        // Arrange
        var inputCollection = new List<string>
        {
            "prefix:item-name",
            "another_item_name",
            "simple"
        };
        var attachingCollection = new List<TestNToNTable>();

        // Act
        ProductService.AttachToCollection(inputCollection, attachingCollection);

        // Assert
        Assert.Equal(3, attachingCollection.Count);
        Assert.Equal("item name", attachingCollection[0].Name);
        Assert.Equal("another item name", attachingCollection[1].Name);
        Assert.Equal("simple", attachingCollection[2].Name);
    }

    [Fact]
    public void AttachToCollection_WithNullCollection_DoesNothing()
    {
        // Arrange
        List<string>? inputCollection = null;
        var attachingCollection = new List<TestNToNTable>();

        // Act
        ProductService.AttachToCollection(inputCollection, attachingCollection);

        // Assert
        Assert.Empty(attachingCollection);
    }

    [Fact]
    public void AttachToCollection_WithNullItem_StopsProcessing()
    {
        // Arrange
        // The implementation stops looping (returns) when it encounters a null item
        var inputCollection = new List<string?> { "first-item", null, "second-item" };
        var attachingCollection = new List<TestNToNTable>();

        // Act
        ProductService.AttachToCollection(inputCollection!, attachingCollection);

        // Assert
        Assert.Single(attachingCollection);
        Assert.Equal("first item", attachingCollection[0].Name);
    }

    [Fact]
    public void AttachToCollection_AppendsToExistingCollection()
    {
        // Arrange
        var inputCollection = new List<string> { "new-item" };
        var attachingCollection = new List<TestNToNTable>
        {
            new TestNToNTable { Name = "existing" }
        };

        // Act
        ProductService.AttachToCollection(inputCollection, attachingCollection);

        // Assert
        Assert.Equal(2, attachingCollection.Count);
        Assert.Equal("existing", attachingCollection[0].Name);
        Assert.Equal("new item", attachingCollection[1].Name);
    }

    [Fact]
    public void AttachToProperty_WithValidString_SetsTargetProperty()
    {
        // Arrange
        var inputItem = "prefix:test-item_name";
        TestNToNTable? result = null;

        // Act
        ProductService.AttachToProperty<TestNToNTable>(inputItem, val => result = val);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test item name", result!.Name);
    }

    [Fact]
    public void AttachToProperty_WithNullItem_DoesNotInvokeAction()
    {
        // Arrange
        string? inputItem = null;
        var wasCalled = false;

        // Act
        ProductService.AttachToProperty<TestNToNTable>(inputItem, _ => wasCalled = true);

        // Assert
        Assert.False(wasCalled);
    }
}