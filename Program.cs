using ImageMagick;
using System;
using System.IO;
using System.Linq;

public class Program
{
    // Punto de entrada de la aplicación
    public static void Main(string[] args)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        ProcessDirectory(currentDirectory);
        Console.WriteLine("Proceso completado.");
    }

    // Procesa un directorio y todos sus subdirectorios
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

                // Refrescando el caché de iconos para la carpeta actual
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

    // Convierte un archivo jpg a un icono de 256x256
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

    // Crea un archivo desktop.ini en el directorio especificado
    public static void CreateDesktopIniFile(string directory, string iconFileName)
    {
        string desktopIniPath = Path.Combine(directory, "desktop.ini");
        string content = $"[.ShellClassInfo]\r\nIconResource={iconFileName},0";

        File.WriteAllText(desktopIniPath, content);

        File.SetAttributes(desktopIniPath, File.GetAttributes(desktopIniPath) | FileAttributes.Hidden | FileAttributes.System);

        DirectoryInfo di = new DirectoryInfo(directory);
        di.Attributes |= FileAttributes.ReadOnly;
    }

    // Genera una cadena aleatoria de la longitud especificada
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}