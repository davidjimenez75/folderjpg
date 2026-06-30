using Xunit;
using System;
using System.IO;
using ImageMagick;

public class DirectoryProcessingTests
{
    // Helper method
    private void CreateDummyJpg(string path)
    {
        // Use a specific color for easier debugging if needed
        using (var image = new MagickImage(MagickColors.BlueViolet, 50, 50))
        {
            image.Format = MagickFormat.Jpg;
            image.Write(path);
        }
    }

    // Helper for recursive cleanup
    private void RemoveReadOnlyRecursive(string dirPath)
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
                RemoveReadOnlyRecursive(subDirPath); // Recursive call
            }
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Warning: Error accessing subdirectories of '{dirPath}' during cleanup: {ex.Message}");
        }
    }

    [Fact]
    public void ProcessDirectory_WithFolderJpg_CreatesIconAndIni()
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
                try
                {
                    RemoveReadOnlyRecursive(testDirectory);
                    Directory.Delete(testDirectory, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to fully cleanup test directory '{testDirectory}': {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public void ProcessDirectory_WithCoverJpg_CreatesIconAndIni()
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
                 try
                {
                    RemoveReadOnlyRecursive(testDirectory);
                    Directory.Delete(testDirectory, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to fully cleanup test directory '{testDirectory}': {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public void ProcessDirectory_WithFrontJpg_CreatesIconAndIni()
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
                 try
                {
                    RemoveReadOnlyRecursive(testDirectory);
                    Directory.Delete(testDirectory, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to fully cleanup test directory '{testDirectory}': {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public void ProcessDirectory_SkipsProcessing_IfDesktopIniExists()
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
                 try
                {
                    RemoveReadOnlyRecursive(testDirectory);
                    Directory.Delete(testDirectory, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to fully cleanup test directory '{testDirectory}': {ex.Message}");
                }
            }
        }
    }

    [Fact]
    public void ProcessDirectory_FolderIco_TakesPriorityOverJpg()
    {
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        CreateDummyJpg(Path.Combine(testDirectory, "folder.jpg"));
        File.WriteAllBytes(Path.Combine(testDirectory, "folder.ico"), new byte[] { 0 });

        try
        {
            Program.ProcessDirectory(testDirectory);

            string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
            Assert.True(File.Exists(desktopIniPath));
            string content = File.ReadAllText(desktopIniPath);
            Assert.Contains("IconResource=folder.ico,0", content, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(Directory.GetFiles(testDirectory, "folderjpg-*.ico"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                RemoveReadOnlyRecursive(testDirectory);
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_ColorIco_TakesPriorityOverJpg()
    {
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        CreateDummyJpg(Path.Combine(testDirectory, "folder.jpg"));
        File.WriteAllBytes(Path.Combine(testDirectory, "azure.ico"), new byte[] { 0 });

        try
        {
            Program.ProcessDirectory(testDirectory);

            string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
            Assert.True(File.Exists(desktopIniPath));
            string content = File.ReadAllText(desktopIniPath);
            Assert.Contains("IconResource=azure.ico,0", content, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(Directory.GetFiles(testDirectory, "folderjpg-*.ico"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                RemoveReadOnlyRecursive(testDirectory);
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_FolderjpgDesktopIni_UpToDate_IsNotRegenerated()
    {
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string icoName      = "folderjpg-abc123.ico";
        string icoPath      = Path.Combine(testDirectory, icoName);
        string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
        string jpgPath      = Path.Combine(testDirectory, "folder.jpg");

        CreateDummyJpg(jpgPath);
        File.WriteAllBytes(icoPath, new byte[] { 0 });
        File.WriteAllText(desktopIniPath, $"[.ShellClassInfo]\r\nIconResource={icoName},0");

        // ico is newer than source → should NOT regenerate
        File.SetLastWriteTime(jpgPath,  DateTime.Now.AddHours(-2));
        File.SetLastWriteTime(icoPath,  DateTime.Now.AddHours(-1));

        try
        {
            Program.ProcessDirectory(testDirectory);

            Assert.True(File.Exists(desktopIniPath));
            Assert.True(File.Exists(icoPath));
            string content = File.ReadAllText(desktopIniPath);
            Assert.Contains($"IconResource={icoName},0", content, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                RemoveReadOnlyRecursive(testDirectory);
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_FolderjpgDesktopIni_SourceImageNewer_IsRegenerated()
    {
        string testDirectory  = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        string oldIcoName     = "folderjpg-old123.ico";
        string oldIcoPath     = Path.Combine(testDirectory, oldIcoName);
        string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
        string jpgPath        = Path.Combine(testDirectory, "folder.jpg");

        CreateDummyJpg(jpgPath);
        File.WriteAllBytes(oldIcoPath, new byte[] { 0 });
        File.WriteAllText(desktopIniPath, $"[.ShellClassInfo]\r\nIconResource={oldIcoName},0");

        // source is newer than ico → should regenerate
        File.SetLastWriteTime(oldIcoPath, DateTime.Now.AddHours(-2));
        File.SetLastWriteTime(jpgPath,    DateTime.Now.AddHours(-1));

        try
        {
            Program.ProcessDirectory(testDirectory);

            Assert.False(File.Exists(oldIcoPath), "Old ico should be deleted after regeneration.");
            Assert.Single(Directory.GetFiles(testDirectory, "folderjpg-*.ico"));
            Assert.True(File.Exists(desktopIniPath), "desktop.ini should be recreated.");
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                RemoveReadOnlyRecursive(testDirectory);
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_FallbackIndexIco_UsedWhenNoImageExists()
    {
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        File.WriteAllBytes(Path.Combine(testDirectory, "index.ico"), new byte[] { 0 });

        try
        {
            Program.ProcessDirectory(testDirectory);

            string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
            Assert.True(File.Exists(desktopIniPath));
            string content = File.ReadAllText(desktopIniPath);
            Assert.Contains("IconResource=index.ico,0", content, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(Directory.GetFiles(testDirectory, "folderjpg-*.ico"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                RemoveReadOnlyRecursive(testDirectory);
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_FallbackFaviconIco_UsedWhenNoImageAndNoIndexIco()
    {
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        File.WriteAllBytes(Path.Combine(testDirectory, "favicon.ico"), new byte[] { 0 });

        try
        {
            Program.ProcessDirectory(testDirectory);

            string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
            Assert.True(File.Exists(desktopIniPath));
            string content = File.ReadAllText(desktopIniPath);
            Assert.Contains("IconResource=favicon.ico,0", content, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(Directory.GetFiles(testDirectory, "folderjpg-*.ico"));
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                RemoveReadOnlyRecursive(testDirectory);
                Directory.Delete(testDirectory, true);
            }
        }
    }

    [Fact]
    public void ProcessDirectory_FallbackIndexIco_TakesPriorityOverFaviconIco()
    {
        string testDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(testDirectory);
        File.WriteAllBytes(Path.Combine(testDirectory, "index.ico"),   new byte[] { 0 });
        File.WriteAllBytes(Path.Combine(testDirectory, "favicon.ico"), new byte[] { 0 });

        try
        {
            Program.ProcessDirectory(testDirectory);

            string desktopIniPath = Path.Combine(testDirectory, "desktop.ini");
            Assert.True(File.Exists(desktopIniPath));
            string content = File.ReadAllText(desktopIniPath);
            Assert.Contains("IconResource=index.ico,0", content, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(testDirectory))
            {
                RemoveReadOnlyRecursive(testDirectory);
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
                    RemoveReadOnlyRecursive(rootDir);
                    Directory.Delete(rootDir, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to fully cleanup test directory '{rootDir}': {ex.Message}");
                }
            }
        }
    }
}
