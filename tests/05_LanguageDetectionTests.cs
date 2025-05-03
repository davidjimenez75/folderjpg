using Xunit;
using System;
using System.IO;
using System.Globalization;
using System.Threading;

// Apply the non-parallel collection attribute
[Collection("NonParallelConsoleTests")]
public class LanguageDetectionTests
{
     // Helper method (copied from CommandLineArgsTests - consider a shared helper class later)
    private string RunMainAndCaptureOutput(string[] args)
    {
        var consoleOutput = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(consoleOutput);
        try
        {
            Program.Main(args);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
        return consoleOutput.ToString();
    }

    [Theory]
    [InlineData("es", "Uso:")] // Spanish help contains "Uso:"
    [InlineData("de", "Verwendung:")] // German help contains "Verwendung:"
    [InlineData("fr", "Utilisation:")] // French help contains "Utilisation:"
    [InlineData("it", "Utilizzo:")] // Italian help contains "Utilizzo:"
    [InlineData("pt", "Uso:")] // Portuguese help contains "Uso:"
    [InlineData("zh", "用法:")] // Chinese help contains "用法:"
    [InlineData("en", "Usage:")] // Default/English help contains "Usage:"
    [InlineData("xx", "Usage:")] // Unknown language defaults to English
    public void Main_LangArgument_DisplaysCorrectLanguageHelp(string langCode, string expectedHelpSnippet)
    {
        // Act
        string output = RunMainAndCaptureOutput(new string[] { "--lang", langCode });

        // Assert
        Assert.Contains(expectedHelpSnippet, output, StringComparison.OrdinalIgnoreCase); // Use OrdinalIgnoreCase for robustness
    }

    // Note: Testing GetSystemLanguage directly is difficult without mocking.
    // Instead, we test the DisplayHelp's behavior which relies on it.
    [Fact]
    public void DisplayHelp_UsesCurrentUICultureByDefault()
    {
        // Arrange
        var originalCulture = Thread.CurrentThread.CurrentUICulture;
        var originalOut = Console.Out;
        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        try
        {
            // Set culture to Spanish for this test part
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

            // Act
            Program.Main(new string[] { "--help" }); // Trigger DisplayHelp via Main
            string outputEs = consoleOutput.ToString();

            // Assert
            Assert.Contains("Uso:", outputEs, StringComparison.OrdinalIgnoreCase); // Check for Spanish help snippet

            // Reset console and set culture to German for next part
            consoleOutput = new StringWriter(); // Reset writer
            Console.SetOut(consoleOutput); // Reassign writer
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");

             // Act again with German
            Program.Main(new string[] { "--help" });
            string outputDe = consoleOutput.ToString();

             // Assert
            Assert.Contains("Verwendung:", outputDe, StringComparison.OrdinalIgnoreCase); // Check for German help snippet

        }
        finally
        {
            // Restore original culture and console output
            Thread.CurrentThread.CurrentUICulture = originalCulture;
            Console.SetOut(originalOut);
        }
    }
}
