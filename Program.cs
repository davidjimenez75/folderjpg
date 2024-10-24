using ImageMagick;
using System;
using System.IO;
using System.Linq;

public class Program
{
    public const string VERSION = "2024.10.24.2228";
    private const string DEBUG = "false";

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

                // Help options
                case "-help":
                    DisplayHelp();
                    return;

                // Help options
                case "-h":
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
                
                // Version options
                case "-version":
                    DisplayVersion();
                    return;
                
                // Version options
                case "--v":
                    DisplayVersion();
                    return;

                // Version options
                case "-v":
                    DisplayVersion();
                    return;

                // Detect if the user is inserting a path as an argument
                default:
                    if (Directory.Exists(args[0]))
                    {
                        // if the path exist, process recursively the directory
                        Console.WriteLine("folderjpg v" + VERSION);
                        Console.WriteLine();
                        ProcessDirectory(args[0]);
                        Console.WriteLine("Job Finished");
                        return;
                    }
                    else
                    {
                        // if the path does not exist, show an error message
                        Console.WriteLine("folderjpg v" + VERSION);
                        Console.WriteLine();
                        Console.WriteLine("Path not found. Use --help to see the available options.");
                        return;
                    }
            }
        }
        // If no arguments are passed, we dont to nothing
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("Path not found. Use --help to see the available options.");
    }



    // Show the version of the program on console   
    static void DisplayVersion()
    {
        Console.WriteLine($"folderjpg v{VERSION}");
    }

    // Process the directory and its subdirectories
    static void ProcessDirectory(string directory)
    {
        try
        {
            // Get directory name if folder.jpg exists
            string[] folderJpgFiles = Directory.GetFiles(directory, "folder.jpg", SearchOption.TopDirectoryOnly);

            // Get directory name if cover.jpg exists
            string[] coverJpgFiles = Directory.GetFiles(directory, "cover.jpg", SearchOption.TopDirectoryOnly);

            // Get directory name if front.jpg exists
            string[] frontJpgFiles = Directory.GetFiles(directory, "front.jpg", SearchOption.TopDirectoryOnly);

            // Combine the folder.jpg and cover.jpg files into a single array to process
            string[] jpgFiles = folderJpgFiles.Concat(coverJpgFiles).ToArray();

            // Combine the folder.jpg, cover.jpg and front.jpg files into a single array to process
            jpgFiles = jpgFiles.Concat(frontJpgFiles).ToArray();

            // loop through the jpg files
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
                    Console.WriteLine($"- desktop.ini already exists in: \"{directoryName}\"");
                    continue;
                }

                if (DEBUG == "true")
                {
                    // If debug mode exist only show information
                    Console.WriteLine($"### \"{directoryName}\"");

                    if (File.Exists(Path.Combine(directoryName, "desktop.ini")))
                    {
                        Console.WriteLine($"- \"{directoryName}\\desktop.ini\"");
                    }
                    if (File.Exists(Path.Combine(directoryName, "folder.jpg")))
                    {
                        Console.WriteLine($"- \"{directoryName}\\folder.jpg\"");
                    }
                    if (File.Exists(Path.Combine(directoryName, "cover.jpg")))
                    {
                        Console.WriteLine($"- \"{directoryName}\\cover.jpg\"");
                    }

                }
                else
                {
                    string randomString = GenerateRandomString(6);
                    string icoFileName = Path.Combine(directoryName, $"folderjpg-{randomString}.ico");

                    // New line to separate directories
                    Console.WriteLine();

                    // Show the current directory being processed
                    Console.WriteLine($"### folderjpg \"{directoryName}\\\"");
                    Console.WriteLine();

                    // Convert the jpg file to a 256x256 icon
                    Console.WriteLine($"- Found jpg: \"{jpgFile}\"");
                    ConvertToIcon(jpgFile, icoFileName);

                    // Create the desktop.ini file
                    Console.WriteLine($"- Creating icon: \"{directoryName}\\folderjpg-{randomString}.ico\"");
                    CreateDesktopIniFile(directoryName, $"folderjpg-{randomString}.ico");

                    // FIXME: Refreshing icon cache for current folder only for Window environment
                    if (Environment.OSVersion.Platform != PlatformID.Unix)
                    {
                        Console.WriteLine($"- Refreshing icon cache");
                        System.Diagnostics.Process.Start("ie4uinit.exe", "-show");
                    }
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
    static void ConvertToIcon(string inputPath, string outputPath)
    {
        // Convert the jpg file to an icon file with multiple sizes from 16 to 256 pixels
        try
        {
            using (var collection = new MagickImageCollection())
            {
                // Load the original image
                using (var originalImage = new MagickImage(inputPath))
                {
                    // Define standard ICO sizes
                    int[] sizes = { 16, 32, 48, 64, 128, 256 };

                    foreach (int size in sizes)
                    {
                        using (var image = originalImage.Clone())
                        {
                            // Resize the image while maintaining aspect ratio
                            image.Resize(size, size);

                            // Create a new image with the correct size and transparent background
                            using (var resizedImage = new MagickImage(MagickColors.Transparent, size, size))
                            {
                                // Composite the resized image onto the transparent background
                                resizedImage.Composite(image, Gravity.Center, CompositeOperator.Over);

                                // Add the image to the collection
                                collection.Add(resizedImage.Clone());
                            }
                        }
                    }
                }

                // Save as ICO
                collection.Write(outputPath, MagickFormat.Ico);
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
    static string GetSystemLanguage()
    {
        return System.Globalization.CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;
    }

    // Show the help information on console
    static void DisplayHelp()
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
    static void DisplayHelpEnglish()
    {
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("Command line tool to create full size icons to Windows folders recursively from all the \"folder.jpg\" and \"cover.jpg\" files.");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  folderjpg [options] <path>");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --help     Display this help text");
        Console.WriteLine("  --lang xx  Force the language");
        Console.WriteLine("  --version  Display the version of the program");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  path       The path to the directory to process");
        Console.WriteLine("             If no path is provided, nothing is done");
        Console.WriteLine("             If the path is not found, an error message is displayed");
        Console.WriteLine("             If the path is not a directory, an error message is displayed");
        Console.WriteLine("             If the path is a file, an error message is displayed");
        Console.WriteLine("             If the path is a directory, the program processes it recursively");
        Console.WriteLine("");
        Console.WriteLine("Examples:");
        Console.WriteLine("  folderjpg");
        Console.WriteLine("  folderjpg --help");
        Console.WriteLine("  folderjpg --lang es");
        Console.WriteLine("  folderjpg --version");
        Console.WriteLine("  folderjpg \"C:\\Users\\User\\Pictures\"");
        Console.WriteLine("  folderjpg \"C:\\Users\\User\\Music\"");
        Console.WriteLine("  folderjpg \"C:\\Users\\User\\Calibre library\"");


    }

    // HELP ES - help in spanish
    static void DisplayHelpSpanish()
    {
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("Crea iconos en las subcarpetas a partir de los archivos folder.jpg y cover.jpg");
        Console.WriteLine();
        Console.WriteLine("Uso:");
        Console.WriteLine("  folderjpg [opciones] <ruta>");
        Console.WriteLine();
        Console.WriteLine("Opciones:");
        Console.WriteLine("  --help     Muestra este texto de ayuda");
        Console.WriteLine("  --lang xx  Fuerza el idioma");
        Console.WriteLine("  --version  Muestra la versión del programa");
        Console.WriteLine();
        Console.WriteLine("Argumentos:");
        Console.WriteLine("  ruta       La ruta al directorio a procesar");
        Console.WriteLine("             Si no se proporciona ninguna ruta, no se hace nada");
        Console.WriteLine("             Si no se encuentra la ruta, se muestra un mensaje de error");
        Console.WriteLine("             Si la ruta no es un directorio, se muestra un mensaje de error");
        Console.WriteLine("             Si la ruta es un archivo, se muestra un mensaje de error");
        Console.WriteLine("             Si la ruta es un directorio, el programa lo procesa de forma recursiva");
        Console.WriteLine("");
        Console.WriteLine("Ejemplos:");
        Console.WriteLine("  folderjpg");
        Console.WriteLine("  folderjpg --help");
        Console.WriteLine("  folderjpg --lang en");
        Console.WriteLine("  folderjpg --version");
        Console.WriteLine("  folderjpg \"C:\\Users\\usuario\\Pictures\"");
        Console.WriteLine("  folderjpg \"C:\\Users\\usuario\\Music\"");
        Console.WriteLine("  folderjpg \"C:\\Users\\usuario\\Biblioteca de calibre\"");
    }

}// End of Program
