using Xunit;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic; // Added for IEnumerable

public class CommandLineArgsTests
{
    // Helper method
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

    // Data source for help arguments
    public static IEnumerable<object[]> HelpArgumentsData =>
        new List<object[]> {
            new object[] { new string[] { "--help" } },
            new object[] { new string[] { "-help" } },
            new object[] { new string[] { "-h" } }
        };

    [Theory]
    [MemberData(nameof(HelpArgumentsData))]
    public void Main_HelpArguments_DisplaysHelpMessage(string[] args)
    {
        // Act
        string output = RunMainAndCaptureOutput(args);

        // Assert
        Assert.Contains("Usage:", output, StringComparison.OrdinalIgnoreCase); // Common string in help messages
        Assert.Contains("folderjpg [options] <path>", output, StringComparison.OrdinalIgnoreCase);
    }

    // Data source for version arguments
    public static IEnumerable<object[]> VersionArgumentsData =>
        new List<object[]> {
            new object[] { new string[] { "--version" } },
            new object[] { new string[] { "-version" } },
            new object[] { new string[] { "--v" } },
            new object[] { new string[] { "-v" } }
        };

    [Theory]
    [MemberData(nameof(VersionArgumentsData))]
    public void Main_VersionArguments_DisplaysVersionInfo(string[] args)
    {
        // Act
        string output = RunMainAndCaptureOutput(args);

        // Assert
        // Using Regex for more robust version checking
        Assert.Matches(new Regex($"folderjpg v{Regex.Escape(Program.VERSION)}"), output);
    }

    [Fact]
    public void Main_NoArguments_DisplaysNoPathProvidedMessage()
    {
        // Act
        string output = RunMainAndCaptureOutput(Array.Empty<string>()); // Use Array.Empty

        // Assert
        Assert.Contains("No path provided.", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Use --help", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Main_InvalidPathArgument_DisplaysPathNotFoundMessage()
    {
        // Arrange
        string invalidPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()); // A path that doesn't exist

        // Act
        string output = RunMainAndCaptureOutput(new string[] { invalidPath });

        // Assert
        Assert.Contains("Path not found.", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Use --help", output, StringComparison.OrdinalIgnoreCase);
    }

     [Fact]
    public void Main_FilePathArgument_DisplaysPathNotFoundOrNotDirectoryMessage()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string filePath = Path.Combine(testDirectory, "testfile.txt");
        File.WriteAllText(filePath, "hello");

        try
        {
            // Act
            string output = RunMainAndCaptureOutput(new string[] { filePath });

            // Assert
            // Current behavior treats existing files as "Path not found" because it only checks Directory.Exists
            Assert.Contains("Path not found.", output, StringComparison.OrdinalIgnoreCase);
            // TODO: Ideally, this should be a specific error like "Path must be a directory"
        }
        finally
        {
             if (Directory.Exists(testDirectory))
             {
                 Directory.Delete(testDirectory, true);
             }
        }
    }

    [Fact]
    public void Main_ValidPathArgument_InitiatesProcessing()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);

        try
        {
            // Act
            string output = RunMainAndCaptureOutput(new string[] { testDirectory });

            // Assert
            Assert.Matches(new Regex($"folderjpg v{Regex.Escape(Program.VERSION)}"), output); // Check version output
            Assert.Contains("Job Finished", output, StringComparison.OrdinalIgnoreCase); // Indicates processing completed
            Assert.DoesNotContain("Path not found", output, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("No path provided", output, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
             if (Directory.Exists(testDirectory))
             {
                 // Ensure directory is not read-only before deleting
                 DirectoryInfo di = new DirectoryInfo(testDirectory);
                 if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReadOnly))
                 {
                    di.Attributes &= ~FileAttributes.ReadOnly;
                 }
                 Directory.Delete(testDirectory, true);
             }
        }
    }
}
