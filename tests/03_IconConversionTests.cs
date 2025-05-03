using Xunit;
using System;
using System.IO;
using ImageMagick;

public class IconConversionTests
{
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

    [Fact]
    public void ConvertToIcon_HandlesInvalidInputFileGracefully() // Renamed for clarity
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
        var originalOut = Console.Out; // Store original console out
        Console.SetOut(consoleOutput);

        try
        {
            // Act
            Program.ConvertToIcon(invalidInputPath, outputIcoPath);

            // Assert
            Assert.False(File.Exists(outputIcoPath), "Icon file should not be created for invalid input."); // Added assertion message

            // Check if an error message was printed to the console (in English)
            string output = consoleOutput.ToString();
            Assert.Contains("- ERROR:", output, StringComparison.OrdinalIgnoreCase); // Check for error prefix
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
            // Restore standard output
            Console.SetOut(originalOut); // Restore original console out
        }
    }
}
