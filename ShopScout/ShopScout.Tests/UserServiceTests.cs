using Microsoft.AspNetCore.Identity;
using Moq;
using ShopScout.Services;
using ShopScout.SharedLib.Models;
using Xunit;

namespace ShopScout.Tests;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null);
        
        _userService = new UserService(_userManagerMock.Object, null, null, null, null, null);
    }

    [Fact]
    public async Task SetUniqueUserName_WithValidEmail_SetsUsernameFromEmailPrefix()
    {
        // Arrange
        var user = new ApplicationUser { Email = "testuser@example.com" };
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        await _userService.SetUniqueUserName(user);

        // Assert
        Assert.Equal("testuser", user.UserName);
    }

    [Fact]
    public async Task SetUniqueUserName_WithShortEmail_PadsUsernameToFourCharacters()
    {
        // Arrange
        var user = new ApplicationUser { Email = "abc@example.com" };
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        await _userService.SetUniqueUserName(user);

        // Assert
        Assert.Equal("abc0", user.UserName);
    }

    [Fact]
    public async Task SetUniqueUserName_WhenUsernameExists_AppendsNumericSuffix()
    {
        // Arrange
        var user = new ApplicationUser { Email = "testuser@example.com" };
        var existingUser = new ApplicationUser { UserName = "testuser" };
        
        _userManagerMock.SetupSequence(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(existingUser)  // First call returns existing user
            .ReturnsAsync((ApplicationUser?)null);  // Second call returns null

        // Act
        await _userService.SetUniqueUserName(user);

        // Assert
        Assert.Equal("testuser1", user.UserName);
    }

    [Fact]
    public async Task SetUniqueUserName_WhenMultipleUsernamesExist_IncrementsUntilUnique()
    {
        // Arrange
        var user = new ApplicationUser { Email = "testuser@example.com" };
        var existingUser1 = new ApplicationUser { UserName = "testuser" };
        var existingUser2 = new ApplicationUser { UserName = "testuser1" };
        var existingUser3 = new ApplicationUser { UserName = "testuser2" };
        
        _userManagerMock.SetupSequence(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(existingUser1)
            .ReturnsAsync(existingUser2)
            .ReturnsAsync(existingUser3)
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        await _userService.SetUniqueUserName(user);

        // Assert
        Assert.Equal("testuser3", user.UserName);
    }

    [Fact]
    public async Task SetUniqueUserName_WithTwoCharacterEmail_PadsAndSetsUsername()
    {
        // Arrange
        var user = new ApplicationUser { Email = "ab@example.com" };
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        await _userService.SetUniqueUserName(user);

        // Assert
        Assert.Equal("ab00", user.UserName);
    }

    [Fact]
    public async Task SetUniqueUserName_WithFourCharacterEmail_DoesNotPad()
    {
        // Arrange
        var user = new ApplicationUser { Email = "test@example.com" };
        _userManagerMock.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        await _userService.SetUniqueUserName(user);

        // Assert
        Assert.Equal("test", user.UserName);
    }
}