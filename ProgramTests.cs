using Xunit;
using System;
using System.IO;
using System.Text.RegularExpressions;
using ImageMagick;
using System.Globalization;
using System.Threading;
using System.Collections.Generic; // Added for IEnumerable
using System.Linq; // Added for Linq methods

// Renamed class to follow English naming conventions
public class ProgramIntegrationTests // Or ProgramFunctionalityTests, ProgramEndToEndTests etc.
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

    // Renamed test method
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

    // Renamed test method
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

    // --- Tests for Command-line argument parsing --- // Comment translated

    // Helper method name unchanged, but internal comments could be translated if needed
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

    // Data source name translated
    public static IEnumerable<object[]> HelpArgumentsData =>
        new List<object[]> {
            new object[] { new string[] { "--help" } },
            new object[] { new string[] { "-help" } },
            new object[] { new string[] { "-h" } }
        };

    [Theory]
    [MemberData(nameof(HelpArgumentsData))]
    public void Main_HelpArguments_DisplaysHelpMessage(string[] args) // Renamed test
    {
        // Act
        string output = RunMainAndCaptureOutput(args);

        // Assert
        Assert.Contains("Usage:", output, StringComparison.OrdinalIgnoreCase); // Common string in help messages
        Assert.Contains("folderjpg [options] <path>", output, StringComparison.OrdinalIgnoreCase);
    }

    // Data source name translated
    public static IEnumerable<object[]> VersionArgumentsData =>
        new List<object[]> {
            new object[] { new string[] { "--version" } },
            new object[] { new string[] { "-version" } },
            new object[] { new string[] { "--v" } },
            new object[] { new string[] { "-v" } }
        };

    [Theory]
    [MemberData(nameof(VersionArgumentsData))]
    public void Main_VersionArguments_DisplaysVersionInfo(string[] args) // Renamed test
    {
        // Act
        string output = RunMainAndCaptureOutput(args);

        // Assert
        // Using Regex for more robust version checking
        Assert.Matches(new Regex($"folderjpg v{Regex.Escape(Program.VERSION)}"), output);
    }

    [Fact]
    public void Main_NoArguments_DisplaysNoPathProvidedMessage() // Renamed test
    {
        // Act
        string output = RunMainAndCaptureOutput(Array.Empty<string>()); // Use Array.Empty

        // Assert
        Assert.Contains("No path provided.", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Use --help", output, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Main_InvalidPathArgument_DisplaysPathNotFoundMessage() // Renamed test
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
    public void Main_FilePathArgument_DisplaysPathNotFoundOrNotDirectoryMessage() // Renamed test and comment
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
            // TODO: Ideally, this should be a specific error like "Path must be a directory" - Comment remains valid
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
    public void Main_ValidPathArgument_InitiatesProcessing() // Renamed test
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

    // --- Tests for Language detection logic --- // Comment translated

    [Theory]
    [InlineData("es", "Uso:")] // Spanish help contains "Uso:"
    [InlineData("de", "Verwendung:")] // German help contains "Verwendung:"
    [InlineData("fr", "Utilisation:")] // French help contains "Utilisation:"
    [InlineData("it", "Utilizzo:")] // Italian help contains "Utilizzo:"
    [InlineData("pt", "Uso:")] // Portuguese help contains "Uso:"
    [InlineData("zh", "用法:")] // Chinese help contains "用法:"
    [InlineData("en", "Usage:")] // Default/English help contains "Usage:"
    [InlineData("xx", "Usage:")] // Unknown language defaults to English
    public void Main_LangArgument_DisplaysCorrectLanguageHelp(string langCode, string expectedHelpSnippet) // Renamed test
    {
        // Act
        string output = RunMainAndCaptureOutput(new string[] { "--lang", langCode });

        // Assert
        Assert.Contains(expectedHelpSnippet, output, StringComparison.OrdinalIgnoreCase); // Use OrdinalIgnoreCase for robustness
    }

    // Note: Testing GetSystemLanguage directly is difficult without mocking.
    // Instead, we test the DisplayHelp's behavior which relies on it.
    [Fact]
    public void DisplayHelp_UsesCurrentUICultureByDefault() // Renamed test
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

    // --- Tests for Directory processing logic --- // Comment translated

    // Helper method name unchanged
    private void CreateDummyJpg(string path)
    {
        // Use a specific color for easier debugging if needed
        using (var image = new MagickImage(MagickColors.BlueViolet, 50, 50))
        {
            image.Format = MagickFormat.Jpg;
            image.Write(path);
        }
    }

    [Fact]
    public void ProcessDirectory_WithFolderJpg_CreatesIconAndIni() // Renamed test
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
            Assert.True(File.Exists(desktopIniPath), "desktop.ini should be created.");

            // Find the generated ico file (name contains random part)
            var icoFiles = Directory.GetFiles(testDirectory, "folderjpg-*.ico");
            Assert.Single(icoFiles); // Ensure exactly one .ico file is created
            Assert.True(File.Exists(icoFiles[0]), ".ico file should exist.");

            // Check desktop.ini content points to the ico file
            string iniContent = File.ReadAllText(desktopIniPath);
            Assert.Contains($"IconResource={Path.GetFileName(icoFiles[0])},0", iniContent, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(testDirectory);
                // Ensure directory is not read-only before deleting
                if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    di.Attributes &= ~FileAttributes.ReadOnly;
                }
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_WithCoverJpg_CreatesIconAndIni() // Renamed test
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
            Assert.True(File.Exists(Path.Combine(testDirectory, "desktop.ini")), "desktop.ini should be created for cover.jpg.");
            var icoFiles = Directory.GetFiles(testDirectory, "folderjpg-*.ico");
            Assert.Single(icoFiles); // Ensure exactly one .ico file is created
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(testDirectory);
                if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReadOnly))
                {
                    di.Attributes &= ~FileAttributes.ReadOnly;
                }
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_WithFrontJpg_CreatesIconAndIni() // Renamed test
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
            Assert.True(File.Exists(Path.Combine(testDirectory, "desktop.ini")), "desktop.ini should be created for front.jpg.");
            var icoFiles = Directory.GetFiles(testDirectory, "folderjpg-*.ico");
            Assert.Single(icoFiles); // Ensure exactly one .ico file is created
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(testDirectory);
                 if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReadOnly))
                 {
                    di.Attributes &= ~FileAttributes.ReadOnly;
                 }
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_SkipsProcessing_IfDesktopIniExists() // Renamed test
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
            Assert.Empty(icoFiles); // Assert that the collection is empty
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(testDirectory);
                // desktop.ini creation might make dir readonly, ensure it's not before delete
                 if (di.Exists && di.Attributes.HasFlag(FileAttributes.ReadOnly))
                 {
                    di.Attributes &= ~FileAttributes.ReadOnly;
                 }
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_ProcessesSubdirectoriesRecursively()
    {
        // Arrange
        string rootDir = Path.Combine(Path.GetTempPath(), "folderjpg_test_" + Path.GetRandomFileName()); // More specific name
        string subDir1 = Path.Combine(rootDir, "Sub1");
        string subDir2 = Path.Combine(subDir1, "Sub2");
        Directory.CreateDirectory(rootDir);
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);

        CreateDummyJpg(Path.Combine(rootDir, "folder.jpg"));
        CreateDummyJpg(Path.Combine(subDir1, "cover.jpg"));
        CreateDummyJpg(Path.Combine(subDir2, "front.jpg"));

        // Define the recursive action for cleanup *within* the finally block scope if possible,
        // or ensure it's robustly handled. Here, we define it outside but check for null.
        Action<string> removeReadOnlyRecursive = null!; // Initialize with null forgiving operator

        removeReadOnlyRecursive = (dirPath) =>
        {
            if (!Directory.Exists(dirPath)) return;
            DirectoryInfo di = new DirectoryInfo(dirPath);
            if (di.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                 di.Attributes &= ~FileAttributes.ReadOnly;
            }
            // Use try-catch for GetDirectories in case of access issues during cleanup
            try
            {
                foreach (var subDirPath in Directory.GetDirectories(dirPath))
                {
                    removeReadOnlyRecursive(subDirPath); // Recursive call
                }
            }
            catch (Exception ex)
            {
                 Console.WriteLine($"Warning: Error accessing subdirectories of '{dirPath}' during cleanup: {ex.Message}");
            }
        };


        try
        {
            // Act
            Program.ProcessDirectory(rootDir);

            // Assert
            // Check root directory
            Assert.True(File.Exists(Path.Combine(rootDir, "desktop.ini")), "desktop.ini missing in root.");
            Assert.Single(Directory.GetFiles(rootDir, "folderjpg-*.ico"));

            // Check subDir1
            Assert.True(File.Exists(Path.Combine(subDir1, "desktop.ini")), "desktop.ini missing in Sub1.");
            Assert.Single(Directory.GetFiles(subDir1, "folderjpg-*.ico"));

            // Check subDir2
            Assert.True(File.Exists(Path.Combine(subDir2, "desktop.ini")), "desktop.ini missing in Sub2.");
            Assert.Single(Directory.GetFiles(subDir2, "folderjpg-*.ico"));
        }
        finally
        {
            // Cleanup with robust read-only removal
            if (Directory.Exists(rootDir))
            {
                try
                {
                    // removeReadOnlyRecursive should be assigned by now if the try block was entered
                    removeReadOnlyRecursive?.Invoke(rootDir); // Use null-conditional invocation
                    Directory.Delete(rootDir, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to fully cleanup test directory '{rootDir}': {ex.Message}");
                }
            }
        }
    }

    // Test for Error handling (directory not found during recursion) - Renamed test
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
            Assert.Contains($"Error processing directory {nonExistentDirectory}", output, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Console.SetOut(originalOut);
            // No cleanup needed as the directory wasn't created
        }
    }

    // --- Tests for desktop.ini file creation logic --- // Comment translated
    // Covered by CreateDesktopIniFile_CreatesFileWithCorrectContentAndAttributes

    // --- Tests for Icon cache refresh logic --- // Comment translated
    // Skipping direct test due to platform dependency and external process call.
    // The code correctly checks for non-Unix platforms before attempting refresh.

} // End of class ProgramIntegrationTests