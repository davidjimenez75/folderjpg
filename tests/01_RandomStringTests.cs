using Xunit;
using System;
using System.Linq; // Added for Linq methods

public class RandomStringTests
{
    private const string TestPath = "C:/test/path"; // Example path for testing

    [Fact]
    public void GenerateDeterministicString_ReturnsCorrectLength()
    {
        // Arrange
        int expectedLength = 6;

        // Act
        // Update method name and provide a test path
        string result = Program.GenerateDeterministicString(TestPath, expectedLength);

        // Assert
        Assert.Equal(expectedLength, result.Length);
    }

    [Fact]
    public void GenerateDeterministicString_ContainsOnlyValidCharacters()
    {
        // Arrange
        // Updated to reflect the new character set in GenerateDeterministicString
        string validChars = "abcdefghijklmnopqrstuvwxyz0123456789"; 
        int length = 10;

        // Act
        // Update method name and provide a test path
        string result = Program.GenerateDeterministicString(TestPath, length);

        // Assert
        Assert.True(result.All(c => validChars.Contains(c)));
    }

    [Fact]
    public void GenerateDeterministicString_ReturnsSameStringForSamePath()
    {
        // Arrange
        int length = 8;
        string path1 = "C:/users/test/folderA";
        string path2 = "C:/users/test/folderA"; // Same path as path1

        // Act
        string result1 = Program.GenerateDeterministicString(path1, length);
        string result2 = Program.GenerateDeterministicString(path2, length);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GenerateDeterministicString_ReturnsDifferentStringsForDifferentPaths()
    {
        // Arrange
        int length = 8;
        string path1 = "C:/users/test/folderA";
        string path2 = "C:/users/test/folderB"; // Different path

        // Act
        string result1 = Program.GenerateDeterministicString(path1, length);
        string result2 = Program.GenerateDeterministicString(path2, length);

        // Assert
        Assert.NotEqual(result1, result2);
    }
    
    [Fact]
    public void GenerateDeterministicString_PathNormalizationEnsuresConsistency()
    {
        // Arrange
        int length = 6;
        string path1 = "C:/Users/Test/Folder"; // Windows style path
        string path2 = "c:/users/test/folder"; // Unix style, lowercase path
    
        // Act
        string result1 = Program.GenerateDeterministicString(path1, length);
        string result2 = Program.GenerateDeterministicString(path2, length);
    
        // Assert
        Assert.Equal(result1, result2); // Expect same result due to normalization
    }
}
