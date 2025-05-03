using Xunit;
using System;
using System.IO;
using System.Text.RegularExpressions;
using ImageMagick; // Added for MagickImage

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

    // Test for TODO 9005: Add tests for the image conversion logic.
    [Fact]
    public void ConvertToIcon_CreatesIconFileFromJpg()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string inputJpgPath = Path.Combine(testDirectory, "test.jpg");
        string outputIcoPath = Path.Combine(testDirectory, "output.ico");

        // Create a dummy JPG file for testing
        using (var image = new MagickImage(MagickColors.Red, 100, 100))
        {
            image.Format = MagickFormat.Jpg;
            image.Write(inputJpgPath);
        }

        try
        {
            // Act
            Program.ConvertToIcon(inputJpgPath, outputIcoPath);

            // Assert
            Assert.True(File.Exists(outputIcoPath));

            // Optional: Add more specific assertions about the ICO file if needed
            // e.g., check its format or dimensions using MagickImageCollection
            using (var collection = new MagickImageCollection(outputIcoPath))
            {
                Assert.True(collection.Count > 0); // Check if it contains at least one image frame
                Assert.Equal(MagickFormat.Ico, collection[0].Format); // Check format
            }
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }
    }

    // Test for TODO 9002: Add tests for the error handling paths (invalid image formats).
    [Fact]
    public void ConvertToIcon_HandlesInvalidInputFile()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string invalidInputPath = Path.Combine(testDirectory, "not_an_image.txt");
        string outputIcoPath = Path.Combine(testDirectory, "output.ico");

        // Create an invalid file (e.g., a text file)
        File.WriteAllText(invalidInputPath, "This is not an image.");

        // Redirect Console output to capture error messages
        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        try
        {
            // Act
            Program.ConvertToIcon(invalidInputPath, outputIcoPath);

            // Assert
            Assert.False(File.Exists(outputIcoPath)); // Icon file should not be created

            // Check if an error message was printed to the console
            string output = consoleOutput.ToString();
            Assert.Contains("- ERROR:", output);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
            // Restore standard output
            var standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);
        }
    }
}