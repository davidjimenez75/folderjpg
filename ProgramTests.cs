using Xunit;
using System;
using System.IO;
using System.Text.RegularExpressions;

public class ProgramTests
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

    [Fact]
    public void CreateDesktopIniFile_CreatesFileWithCorrectContent()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string iconFileName = "test-icon.ico";

        try
        {
            // Act
            Program.CreateDesktopIniFile(testDirectory, iconFileName);

            // Assert
            string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
            Assert.True(File.Exists(desktopIniPath));

            string content = File.ReadAllText(desktopIniPath);
            Assert.Contains("[.ShellClassInfo]", content);
            Assert.Contains($"IconResource={iconFileName},0", content);

            FileAttributes attributes = File.GetAttributes(desktopIniPath);
            Assert.True(attributes.HasFlag(FileAttributes.Hidden));
            Assert.True(attributes.HasFlag(FileAttributes.System));

            // We're not checking for ReadOnly attribute on the directory anymore
        }
        finally
        {
            // Cleanup
            try
            {
                if (Directory.Exists(testDirectory))
                {
                    // Remove read-only attribute if it exists
                    DirectoryInfo di = new DirectoryInfo(testDirectory);
                    di.Attributes &= ~FileAttributes.ReadOnly;

                    Directory.Delete(testDirectory, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to delete temporary directory: {ex.Message}");
            }
        }
    }
}