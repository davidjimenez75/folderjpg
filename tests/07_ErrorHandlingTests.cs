using Xunit;
using System;
using System.IO;
using System.Globalization;
using System.Threading;

// Apply the non-parallel collection attribute
[Collection("NonParallelConsoleTests")]
public class ErrorHandlingTests
{
    [Fact]
    public void ProcessDirectory_HandlesDirectoryNotFoundExceptionDuringProcessing()
    {
        // Arrange: Test the general exception handling in ProcessDirectory.
        string nonExistentDirectory = Path.Combine(Path.GetTempPath(), "folderjpg_nonexistent_" + Path.GetRandomFileName());
        // Intentionally *don't* create the directory.

        var consoleOutput = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(consoleOutput);

        try
        {
            // Act
            Program.ProcessDirectory(nonExistentDirectory);

            // Assert
            string output = consoleOutput.ToString();
            // Check that an error message was logged, but the program didn't crash
            // Note: The original code didn't explicitly log for the top-level non-existent dir,
            // it would log if a *sub*directory was missing during recursion.
            // Adjusting assertion based on expected behavior if the top dir doesn't exist.
            // If Program.ProcessDirectory has internal checks, this might need adjustment.
            // Assuming the main loop might not even start or logs differently.
            // Let's assume it *should* log an error if the initial path is bad.
            // If the current Program.ProcessDirectory doesn't log for the *initial* non-existent path,
            // this test might fail or need adjustment based on actual Program.cs logic.
            // For now, asserting based on the *intended* behavior of handling errors.
            Assert.Contains($"Error processing directory {nonExistentDirectory}", output, StringComparison.OrdinalIgnoreCase);
            // Or, if it silently fails for the top-level non-existent dir:
            // Assert.DoesNotContain("Exception", output, StringComparison.OrdinalIgnoreCase); // Ensure no unhandled exception bubbles up.
        }
        finally
        {
            Console.SetOut(originalOut);
            // No cleanup needed as the directory wasn't created
        }
    }
}
