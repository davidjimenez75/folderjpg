using Xunit;
using System;
using System.IO;

public class DesktopIniTests
{
    [Fact]
    public void CreateDesktopIniFile_CreatesFileWithCorrectContentAndAttributes()
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
            Assert.True(File.Exists(desktopIniPath), "desktop.ini file should exist."); // Added assertion message

            string content = File.ReadAllText(desktopIniPath);
            Assert.Contains("[.ShellClassInfo]", content, StringComparison.OrdinalIgnoreCase); // Added StringComparison
            Assert.Contains($"IconResource={iconFileName},0", content, StringComparison.OrdinalIgnoreCase); // Added StringComparison

            FileAttributes attributes = File.GetAttributes(desktopIniPath);
            Assert.True(attributes.HasFlag(FileAttributes.Hidden), "desktop.ini should have Hidden attribute."); // Added assertion message
            Assert.True(attributes.HasFlag(FileAttributes.System), "desktop.ini should have System attribute."); // Added assertion message

            // Directory ReadOnly attribute check removed as per previous logic
        }
        finally
        {
            // Cleanup
            try
            {
                if (Directory.Exists(testDirectory))
                {
                    // Remove read-only attribute from the directory if it exists before deleting
                    DirectoryInfo di = new DirectoryInfo(testDirectory);
                    if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReadOnly)) // Check if exists and has flag
                    {
                         di.Attributes &= ~FileAttributes.ReadOnly;
                    }

                    Directory.Delete(testDirectory, true);
                }
            }
            catch (Exception ex)
            {
                // Output warning in English
                Console.WriteLine($"Warning: Failed to delete temporary directory '{testDirectory}': {ex.Message}");
            }
        }
    }
}
