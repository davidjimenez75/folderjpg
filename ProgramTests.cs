using Xunit;
using System;
using System.IO;
using System.Text.RegularExpressions;
using ImageMagick;
using System.Globalization;
using System.Threading;
using System.Collections.Generic; // Added for IEnumerable
using System.Linq; // Added for Linq methods

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

    // --- Tests for TODO 9003: Command-line argument parsing ---

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
    public void Main_HelpArguments_DisplaysHelp(string[] args)
    {
        // Act
        string output = RunMainAndCaptureOutput(args);

        // Assert
        Assert.Contains("Usage:", output); // A common string in all help messages
        Assert.Contains("folderjpg [options] <path>", output);
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
    public void Main_VersionArguments_DisplaysVersion(string[] args)
    {
        // Act
        string output = RunMainAndCaptureOutput(args);

        // Assert
        Assert.Contains($"folderjpg v{Program.VERSION}", output);
    }

    [Fact]
    public void Main_NoArguments_DisplaysNoPathMessage()
    {
        // Act
        string output = RunMainAndCaptureOutput(new string[] { });

        // Assert
        Assert.Contains("No path provided.", output);
        Assert.Contains("Use --help", output);
    }

    [Fact]
    public void Main_InvalidPathArgument_DisplaysPathNotFoundMessage()
    {
        // Arrange
        string invalidPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()); // A path that doesn't exist

        // Act
        string output = RunMainAndCaptureOutput(new string[] { invalidPath });

        // Assert
        Assert.Contains("Path not found.", output);
        Assert.Contains("Use --help", output);
    }

     [Fact]
    public void Main_FilePathArgument_DisplaysPathNotFoundMessage() // Or a specific "not a directory" message if implemented
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
            Assert.Contains("Path not found.", output);
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
    public void Main_ValidPathArgument_StartsProcessing()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);

        try
        {
            // Act
            string output = RunMainAndCaptureOutput(new string[] { testDirectory });

            // Assert
            Assert.Contains($"folderjpg v{Program.VERSION}", output);
            Assert.Contains("Job Finished", output); // Indicates processing started and completed
            Assert.DoesNotContain("Path not found", output);
            Assert.DoesNotContain("No path provided", output);
        }
        finally
        {
             if (Directory.Exists(testDirectory))
             {
                 Directory.Delete(testDirectory, true);
             }
        }
    }

    // --- Tests for TODO 9004: Language detection logic ---

    [Theory]
    [InlineData("es", "Uso:")] // Spanish help contains "Uso:"
    [InlineData("de", "Verwendung:")] // German help contains "Verwendung:"
    [InlineData("fr", "Utilisation:")] // French help contains "Utilisation:"
    [InlineData("it", "Utilizzo:")] // Italian help contains "Utilizzo:"
    [InlineData("pt", "Uso:")] // Portuguese help contains "Uso:"
    [InlineData("zh", "用法:")] // Chinese help contains "用法:"
    [InlineData("en", "Usage:")] // Default/English help contains "Usage:"
    [InlineData("xx", "Usage:")] // Unknown language defaults to English
    public void Main_LangArgument_DisplaysCorrectHelp(string langCode, string expectedHelpSnippet)
    {
        // Act
        string output = RunMainAndCaptureOutput(new string[] { "--lang", langCode });

        // Assert
        Assert.Contains(expectedHelpSnippet, output);
    }

    // Note: Testing GetSystemLanguage directly is difficult without mocking.
    // Instead, we test the DisplayHelp's behavior which relies on it.
    [Fact]
    public void DisplayHelp_UsesSystemLanguageByDefault()
    {
        // Arrange
        var originalCulture = Thread.CurrentThread.CurrentUICulture;
        var originalOut = Console.Out;
        var consoleOutput = new StringWriter();
        Console.SetOut(consoleOutput);

        try
        {
            // Set culture to Spanish for this test
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

            // Act
            Program.Main(new string[] { "--help" }); // Trigger DisplayHelp via Main
            string output = consoleOutput.ToString();

            // Assert
            Assert.Contains("Uso:", output); // Check for Spanish help snippet

            // Reset console and culture
            Console.SetOut(originalOut);
            consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");

             // Act again with German
            Program.Main(new string[] { "--help" });
            output = consoleOutput.ToString();

             // Assert
            Assert.Contains("Verwendung:", output); // Check for German help snippet

        }
        finally
        {
            // Restore original culture and console output
            Thread.CurrentThread.CurrentUICulture = originalCulture;
            Console.SetOut(originalOut);
        }
    }

    // --- Tests for TODO 9001/9006: Directory processing logic ---

    private void CreateDummyJpg(string path)
    {
        using (var image = new MagickImage(MagickColors.Blue, 50, 50))
        {
            image.Format = MagickFormat.Jpg;
            image.Write(path);
        }
    }

    [Fact]
    public void ProcessDirectory_FindsFolderJpg_CreatesIconAndIni()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string folderJpgPath = Path.Combine(testDirectory, "folder.jpg");
        CreateDummyJpg(folderJpgPath);

        try
        {
            // Act
            Program.ProcessDirectory(testDirectory);

            // Assert
            string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
            Assert.True(File.Exists(desktopIniPath));

            // Find the generated ico file (name contains random part)
            var icoFiles = Directory.GetFiles(testDirectory, "folderjpg-*.ico");
            Assert.Single(icoFiles);
            Assert.True(File.Exists(icoFiles[0]));

            // Check desktop.ini content points to the ico file
            string iniContent = File.ReadAllText(desktopIniPath);
            Assert.Contains($"IconResource={Path.GetFileName(icoFiles[0])},0", iniContent);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(testDirectory);
                di.Attributes &= ~FileAttributes.ReadOnly; // Ensure directory is not read-only before deleting
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_FindsCoverJpg_CreatesIconAndIni()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string coverJpgPath = Path.Combine(testDirectory, "cover.jpg");
        CreateDummyJpg(coverJpgPath);

        try
        {
            // Act
            Program.ProcessDirectory(testDirectory);

            // Assert
            Assert.True(File.Exists(Path.Combine(testDirectory, "desktop.ini")));
            var icoFiles = Directory.GetFiles(testDirectory, "folderjpg-*.ico");
            Assert.Single(icoFiles);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(testDirectory);
                di.Attributes &= ~FileAttributes.ReadOnly;
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_FindsFrontJpg_CreatesIconAndIni()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string frontJpgPath = Path.Combine(testDirectory, "front.jpg");
        CreateDummyJpg(frontJpgPath);

        try
        {
            // Act
            Program.ProcessDirectory(testDirectory);

            // Assert
            Assert.True(File.Exists(Path.Combine(testDirectory, "desktop.ini")));
            var icoFiles = Directory.GetFiles(testDirectory, "folderjpg-*.ico");
            Assert.Single(icoFiles);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(testDirectory);
                di.Attributes &= ~FileAttributes.ReadOnly;
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_SkipsDirectory_IfDesktopIniExists()
    {
        // Arrange
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string folderJpgPath = Path.Combine(testDirectory, "folder.jpg");
        CreateDummyJpg(folderJpgPath);
        // Create an existing desktop.ini
        File.WriteAllText(Path.Combine(testDirectory, "desktop.ini"), "[.ShellClassInfo]");

        try
        {
            // Act
            Program.ProcessDirectory(testDirectory);

            // Assert
            // No .ico file should be created because desktop.ini already exists
            var icoFiles = Directory.GetFiles(testDirectory, "folderjpg-*.ico");
            Assert.Empty(icoFiles);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(testDirectory);
                di.Attributes &= ~FileAttributes.ReadOnly; // desktop.ini creation makes dir readonly
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_ProcessesSubdirectoriesRecursively()
    {
        // Arrange
        string rootDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        string subDir1 = Path.Combine(rootDir, "Sub1");
        string subDir2 = Path.Combine(subDir1, "Sub2");
        Directory.CreateDirectory(rootDir);
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);

        CreateDummyJpg(Path.Combine(rootDir, "folder.jpg"));
        CreateDummyJpg(Path.Combine(subDir1, "cover.jpg"));
        CreateDummyJpg(Path.Combine(subDir2, "front.jpg"));

        // Define the recursive action directly
        Action<string> removeReadOnlyRecursive = null; // Keep declaration separate
        removeReadOnlyRecursive = (dirPath) =>
        {
            if (!Directory.Exists(dirPath)) return; // Add guard clause
            DirectoryInfo di = new DirectoryInfo(dirPath);
            // Only remove ReadOnly if it's set
            if (di.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                 di.Attributes &= ~FileAttributes.ReadOnly;
            }
            foreach (var subDirPath in Directory.GetDirectories(dirPath))
            {
                removeReadOnlyRecursive(subDirPath); // Recursive call is safe now
            }
        };

        try
        {
            // Act
            Program.ProcessDirectory(rootDir);

            // Assert
            // Check root directory
            Assert.True(File.Exists(Path.Combine(rootDir, "desktop.ini")));
            Assert.Single(Directory.GetFiles(rootDir, "folderjpg-*.ico"));

            // Check subDir1
            Assert.True(File.Exists(Path.Combine(subDir1, "desktop.ini")));
            Assert.Single(Directory.GetFiles(subDir1, "folderjpg-*.ico"));

            // Check subDir2
            Assert.True(File.Exists(Path.Combine(subDir2, "desktop.ini")));
            Assert.Single(Directory.GetFiles(subDir2, "folderjpg-*.ico"));
        }
        finally
        {
            if (Directory.Exists(rootDir))
            {
                // Need to remove ReadOnly from all processed directories before deleting
                removeReadOnlyRecursive(rootDir);
                Directory.Delete(rootDir, true);
            }
        }
    }

    // Test for TODO 9002: Error handling (directory not found during recursion)
    [Fact]
    public void ProcessDirectory_HandlesDirectoryNotFoundExceptionGracefully()
    {
        // Arrange: Create a structure where a subdirectory exists initially but is deleted before ProcessDirectory recurses into it.
        // This is hard to simulate perfectly without modifying the SUT or complex mocking.
        // Instead, we test the general exception handling in ProcessDirectory.

        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        // Intentionally *don't* create the directory to cause an error when GetFiles is called.

        var consoleOutput = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(consoleOutput);

        try
        {
            // Act
            Program.ProcessDirectory(testDirectory);

            // Assert
            string output = consoleOutput.ToString();
            // Check that an error message was logged, but the program didn't crash
            Assert.Contains($"Error processing directory {testDirectory}", output);
        }
        finally
        {
            Console.SetOut(originalOut);
            // No cleanup needed as the directory wasn't created
        }
    }

    // --- Tests for TODO 9007: desktop.ini file creation logic ---
    // Covered by CreateDesktopIniFile_CreatesFileWithCorrectContent

    // --- Tests for TODO 9008: Icon cache refresh logic ---
    // Skipping direct test due to platform dependency and external process call.
    // The code correctly checks for non-Unix platforms before attempting refresh.

}