using ImageMagick;
using System;
using System.IO;
using System.Linq;

public class Program
{
    private const string VERSION = "24.10.05";

    // Entry point of the application
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            switch (args[0])
            {
                case "--help":
                    DisplayHelp();
                    return;
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
            string[] jpgFiles = Directory.GetFiles(directory, "folder.jpg", SearchOption.TopDirectoryOnly);

            foreach (string jpgFile in jpgFiles)
            {
                string? directoryName = Path.GetDirectoryName(jpgFile);
                if (directoryName == null)
                {
                    Console.WriteLine($"No se pudo obtener el directorio para {jpgFile}");
                    continue;
                }

                if (File.Exists(Path.Combine(directoryName, "desktop.ini")))
                {
                    Console.WriteLine($"Ya existe un archivo desktop.ini en el directorio {directoryName}");
                    continue;
                }

                string randomString = GenerateRandomString(6);
                string icoFileName = Path.Combine(directoryName, $"folderjpg-{randomString}.ico");

                // Salto de línea para separar los directorios
                Console.WriteLine();
                Console.WriteLine();

                // Mostramos la carpeta actual que estamos procesando
                Console.WriteLine($"### Procesando directorio: {directoryName}");

                // Convertimos el archivo jpg a icono de 256x256
                Console.WriteLine($"- ConvertToIcon({jpgFile}, {icoFileName});");
                ConvertToIcon(jpgFile, icoFileName);

                // Creamos el archivo desktop.ini
                Console.WriteLine($"- CreateDesktopIniFile({directoryName}, folderjpg-{randomString}.ico);");
                CreateDesktopIniFile(directoryName, $"folderjpg-{randomString}.ico");

                // FIXME: Refrescando el caché de iconos para la carpeta actual
                Console.WriteLine($"- Refreshing icon cache for current folder...");
                System.Diagnostics.Process.Start("ie4uinit.exe", "-show");

                // Procesado el fichero jpg
                Console.WriteLine($"- Procesado: {jpgFile}");
            }

            string[] subdirectories = Directory.GetDirectories(directory);
            foreach (string subdirectory in subdirectories)
            {
                ProcessDirectory(subdirectory);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al procesar el directorio {directory}: {ex.Message}");
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


    // Return the laguage of the system
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
        Console.WriteLine("  --version  Muestra la versión del programa");
    }

}// End of Program