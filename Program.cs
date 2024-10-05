using ImageMagick;
using System;
using System.IO;
using System.Linq;

public class Program
{
    private const string VERSION = "24.10.05";
    private const string DEBUG = "true";

    // Entry point of the application
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            switch (args[0])
            {
                // Help options
                case "--help":
                    DisplayHelp();
                    return;

                // Language options
                case "--lang":
                    if (args.Length > 1)
                    {
                        string lang = args[1];
                        if (lang == "es")
                        {
                            DisplayHelpSpanish();
                        }
                        else
                        {
                            DisplayHelpEnglish();
                        }
                    }
                    else
                    {
                        DisplayHelp();
                    }
                    return;

                // Version options
                case "--version":
                    DisplayVersion();
                    return;
            }
        }

        string currentDirectory = Directory.GetCurrentDirectory();
        ProcessDirectory(currentDirectory);
        Console.WriteLine("Proceso completado.");
    }



    // Show the version of the program on console   
    private static void DisplayVersion()
    {
        Console.WriteLine($"folderjpg v{VERSION}");
    }

    // Process the directory and its subdirectories
    public static void ProcessDirectory(string directory)
    {
        try
        {
            // Get all folder.jpg files in the current directory
            string[] jpgFiles = Directory.GetFiles(directory, "folder.jpg", SearchOption.TopDirectoryOnly);
            
            // Get all cover.jpg files in the current directory
            string[] jpgFiles = Directory.GetFiles(directory, "cover.jpg", SearchOption.TopDirectoryOnly);


            foreach (string jpgFile in jpgFiles)
            {
                string? directoryName = Path.GetDirectoryName(jpgFile);
                if (directoryName == null)
                {
                    Console.WriteLine($"Failed to get the directory for {jpgFile}");
                    continue;
                }

                if (File.Exists(Path.Combine(directoryName, "desktop.ini")))
                {
                    Console.WriteLine($"A desktop.ini file already exists in the directory {directoryName}");
                    continue;
                }

                if (DEBUG)
                {
                    // If debug mode exist only show information
                    Console.WriteLine($"- FOLDER={directoryName}");

                    if (File.Exists(Path.Combine(directoryName, "desktop.ini")))
                    {
                        Console.WriteLine($"- {directoryName}/desktop.ini");
                    }
                    if (File.Exists(Path.Combine(directoryName, "folder.jpg")))
                    {
                        Console.WriteLine($"-  {directoryName}/folder.jpg");
                    }
                    if (File.Exists(Path.Combine(directoryName, "cover.jpg")))
                    {
                        Console.WriteLine($"-  {directoryName}/cover.jpg");
                    }

                }
                else
                {
                    string randomString = GenerateRandomString(6);
                    string icoFileName = Path.Combine(directoryName, $"folderjpg-{randomString}.ico");

                    // New line to separate directories
                    Console.WriteLine();
                    Console.WriteLine();

                    // Show the current directory being processed
                    Console.WriteLine($"### Processing directory: {directoryName}");

                    // Convert the jpg file to a 256x256 icon
                    Console.WriteLine($"- ConvertToIcon({jpgFile}, {icoFileName});");
                    ConvertToIcon(jpgFile, icoFileName);

                    // Create the desktop.ini file
                    Console.WriteLine($"- CreateDesktopIniFile({directoryName}, folderjpg-{randomString}.ico);");
                    CreateDesktopIniFile(directoryName, $"folderjpg-{randomString}.ico");

                    // FIXME: Refreshing icon cache for current folder
                    Console.WriteLine($"- Refreshing icon cache for current folder...");
                    System.Diagnostics.Process.Start("ie4uinit.exe", "-show");

                    // Processed the jpg file
                    Console.WriteLine($"- Processed: {jpgFile}");
                }
                Console.WriteLine();
            }

            string[] subdirectories = Directory.GetDirectories(directory);
            foreach (string subdirectory in subdirectories)
            {
                ProcessDirectory(subdirectory);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing directory {directory}: {ex.Message}");
        }
    }

    // Convert and resize the image to an icon of 256x256
    public static void ConvertToIcon(string inputPath, string outputPath)
    {
        try
        {
            using (var image = new MagickImage(inputPath))
            {
                image.Resize(256, 256);
                image.Format = MagickFormat.Ico;
                image.Write(outputPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"- ERROR: {ex.Message}");
        }
    }

    // Create the desktop.ini in the directory processed
    public static void CreateDesktopIniFile(string directory, string iconFileName)
    {
        string desktopIniPath = Path.Combine(directory, "desktop.ini");
        string content = $"[.ShellClassInfo]\r\nIconResource={iconFileName},0";

        File.WriteAllText(desktopIniPath, content);

        File.SetAttributes(desktopIniPath, File.GetAttributes(desktopIniPath) | FileAttributes.Hidden | FileAttributes.System);

        DirectoryInfo di = new DirectoryInfo(directory);
        di.Attributes |= FileAttributes.ReadOnly;
    }

    // Generate a random string of a given length
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    // Return the language of the system
    public static string GetSystemLanguage()
    {
        return System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
    }

    // Show the help information on console
    private static void DisplayHelp()
    {
        string language = GetSystemLanguage();
        if (language == "es")
        {
            DisplayHelpSpanish();
        }
        else
        {
            DisplayHelpEnglish();
        }
    }

    // HELP EN - help in english
    private static void DisplayHelpEnglish()
    {
        Console.WriteLine("folderjpg - Automatically set folder icons from folder.jpg files");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  folderjpg [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --help     Display this help text");
        Console.WriteLine("  --lang xx  Force the language");
        Console.WriteLine("  --version  Display the version of the program");
    }

    // HELP ES - help in spanish
    private static void DisplayHelpSpanish()
    {
        Console.WriteLine("folderjpg - Establece automáticamente los iconos de las carpetas a partir de los archivos folder.jpg");
        Console.WriteLine();
        Console.WriteLine("Uso:");
        Console.WriteLine("  folderjpg [opciones]");
        Console.WriteLine();
        Console.WriteLine("Opciones:");
        Console.WriteLine("  --help     Muestra este texto de ayuda");
        Console.WriteLine("  --lang xx  Fuerza el idioma");
        Console.WriteLine("  --version  Muestra la versión del programa");
    }

}// End of Program
