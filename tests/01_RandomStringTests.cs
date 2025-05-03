using Xunit;
using System;
using System.Linq; // Added for Linq methods

public class RandomStringTests
{
    [Fact]
    public void GenerateRandomString_ReturnsCorrectLength()
    {
        // Arrange
        int expectedLength = 6;

        // Act
        string result = Program.GenerateRandomString(expectedLength);

        // Assert
        Assert.Equal(expectedLength, result.Length);
    }

    [Fact]
    public void GenerateRandomString_ContainsOnlyValidCharacters()
    {
        // Arrange
        string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        // Act
        string result = Program.GenerateRandomString(10);

        // Assert
        Assert.True(result.All(c => validChars.Contains(c)));
    }
}
