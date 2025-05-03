using ImageMagick;
using System;
using System.IO;
using System.Linq;

public class Program
{
    public const string VERSION = "2025.05.03.2052";
    private const bool DEBUG = false;
    private static readonly Random _random = new Random();

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
                        switch (lang)
                        {
                            case "es":
                                DisplayHelpSpanish();
                                break;
                            case "de":
                                DisplayHelpGerman();
                                break;
                            case "fr":
                                DisplayHelpFrench();
                                break;
                            case "it":
                                DisplayHelpItalian();
                                break;
                            case "pt":
                                DisplayHelpPortuguese();
                                break;
                            case "zh":
                                DisplayHelpChinese();
                                break;
                            default:
                                DisplayHelpEnglish();
                                break;
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
        // If no arguments are passed, show a clear message
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("No path provided. Use --help to see the available options.");
        Console.WriteLine();
        Console.WriteLine("Is you want to proceed the current directory, use: folderjpg .");
    }



    // Show the version of the program on console   
    static void DisplayVersion()
    {
        Console.WriteLine($"folderjpg v{VERSION}");
    }

    // Process the directory and its subdirectories
    public static void ProcessDirectory(string directory)
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
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => _random.Next(s.Length)).Select(i => chars[i]).ToArray());
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
        switch (language)
        {
            case "es":
                DisplayHelpSpanish();
                break;
            case "de":
                DisplayHelpGerman();
                break;
            case "fr":
                DisplayHelpFrench();
                break;
            case "it":
                DisplayHelpItalian();
                break;
            case "pt":
                DisplayHelpPortuguese();
                break;
            case "zh":
                DisplayHelpChinese();
                break;
            default:
                DisplayHelpEnglish();
                break;
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

    // HELP DE - help in german
    static void DisplayHelpGerman()
    {
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("Kommandozeilen-Tool zum rekursiven Erstellen von Ordner-Icons aus allen 'folder.jpg' und 'cover.jpg'-Dateien.");
        Console.WriteLine();
        Console.WriteLine("Verwendung:");
        Console.WriteLine("  folderjpg [optionen] <pfad>");
        Console.WriteLine();
        Console.WriteLine("Optionen:");
        Console.WriteLine("  --help     Zeigt diesen Hilfetext an");
        Console.WriteLine("  --lang xx  Sprache erzwingen");
        Console.WriteLine("  --version  Zeigt die Programmversion an");
        Console.WriteLine();
        Console.WriteLine("Argumente:");
        Console.WriteLine("  pfad       Der zu verarbeitende Verzeichnispfad");
        Console.WriteLine("             Wenn kein Pfad angegeben ist, passiert nichts");
        Console.WriteLine("             Wenn der Pfad nicht gefunden wird, erscheint eine Fehlermeldung");
        Console.WriteLine("             Wenn der Pfad kein Verzeichnis ist, erscheint eine Fehlermeldung");
        Console.WriteLine("             Wenn der Pfad eine Datei ist, erscheint eine Fehlermeldung");
        Console.WriteLine("             Wenn der Pfad ein Verzeichnis ist, wird es rekursiv verarbeitet");
        Console.WriteLine();
        Console.WriteLine("Beispiele:");
        Console.WriteLine("  folderjpg");
        Console.WriteLine("  folderjpg --help");
        Console.WriteLine("  folderjpg --lang de");
        Console.WriteLine("  folderjpg --version");
        Console.WriteLine("  folderjpg \"C:\\Users\\Benutzer\\Bilder\"");
    }

    // HELP FR - help in french
    static void DisplayHelpFrench()
    {
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("Outil en ligne de commande pour créer des icônes de dossier à partir de tous les fichiers 'folder.jpg' et 'cover.jpg' de façon récursive.");
        Console.WriteLine();
        Console.WriteLine("Utilisation:");
        Console.WriteLine("  folderjpg [options] <chemin>");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --help     Affiche ce texte d'aide");
        Console.WriteLine("  --lang xx  Force la langue");
        Console.WriteLine("  --version  Affiche la version du programme");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  chemin     Le chemin du dossier à traiter");
        Console.WriteLine("             Si aucun chemin n'est fourni, rien ne se passe");
        Console.WriteLine("             Si le chemin n'est pas trouvé, un message d'erreur s'affiche");
        Console.WriteLine("             Si le chemin n'est pas un dossier, un message d'erreur s'affiche");
        Console.WriteLine("             Si le chemin est un fichier, un message d'erreur s'affiche");
        Console.WriteLine("             Si le chemin est un dossier, il est traité récursivement");
        Console.WriteLine();
        Console.WriteLine("Exemples:");
        Console.WriteLine("  folderjpg");
        Console.WriteLine("  folderjpg --help");
        Console.WriteLine("  folderjpg --lang fr");
        Console.WriteLine("  folderjpg --version");
        Console.WriteLine("  folderjpg \"C:\\Users\\Utilisateur\\Images\"");
    }

    // HELP IT - help in italian
    static void DisplayHelpItalian()
    {
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("Strumento da riga di comando per creare icone di cartelle ricorsivamente da tutti i file 'folder.jpg' e 'cover.jpg'.");
        Console.WriteLine();
        Console.WriteLine("Utilizzo:");
        Console.WriteLine("  folderjpg [opzioni] <percorso>");
        Console.WriteLine();
        Console.WriteLine("Opzioni:");
        Console.WriteLine("  --help     Mostra questo testo di aiuto");
        Console.WriteLine("  --lang xx  Forza la lingua");
        Console.WriteLine("  --version  Mostra la versione del programma");
        Console.WriteLine();
        Console.WriteLine("Argomenti:");
        Console.WriteLine("  percorso   Il percorso della cartella da elaborare");
        Console.WriteLine("             Se non viene fornito alcun percorso, non viene eseguita alcuna operazione");
        Console.WriteLine("             Se il percorso non viene trovato, viene visualizzato un messaggio di errore");
        Console.WriteLine("             Se il percorso non è una cartella, viene visualizzato un messaggio di errore");
        Console.WriteLine("             Se il percorso è un file, viene visualizzato un messaggio di errore");
        Console.WriteLine("             Se il percorso è una cartella, il programma la elabora ricorsivamente");
        Console.WriteLine();
        Console.WriteLine("Esempi:");
        Console.WriteLine("  folderjpg");
        Console.WriteLine("  folderjpg --help");
        Console.WriteLine("  folderjpg --lang it");
        Console.WriteLine("  folderjpg --version");
        Console.WriteLine("  folderjpg \"C:\\Users\\Utente\\Immagini\"");
    }

    // HELP PT - help in portuguese
    static void DisplayHelpPortuguese()
    {
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("Ferramenta de linha de comando para criar ícones de pastas a partir de todos os arquivos 'folder.jpg' e 'cover.jpg' de forma recursiva.");
        Console.WriteLine();
        Console.WriteLine("Uso:");
        Console.WriteLine("  folderjpg [opções] <caminho>");
        Console.WriteLine();
        Console.WriteLine("Opções:");
        Console.WriteLine("  --help     Exibe este texto de ajuda");
        Console.WriteLine("  --lang xx  Força o idioma");
        Console.WriteLine("  --version  Exibe a versão do programa");
        Console.WriteLine();
        Console.WriteLine("Argumentos:");
        Console.WriteLine("  caminho    O caminho da pasta a ser processada");
        Console.WriteLine("             Se nenhum caminho for fornecido, nada será feito");
        Console.WriteLine("             Se o caminho não for encontrado, uma mensagem de erro será exibida");
        Console.WriteLine("             Se o caminho não for uma pasta, uma mensagem de erro será exibida");
        Console.WriteLine("             Se o caminho for um arquivo, uma mensagem de erro será exibida");
        Console.WriteLine("             Se o caminho for uma pasta, o programa a processará recursivamente");
        Console.WriteLine();
        Console.WriteLine("Exemplos:");
        Console.WriteLine("  folderjpg");
        Console.WriteLine("  folderjpg --help");
        Console.WriteLine("  folderjpg --lang pt");
        Console.WriteLine("  folderjpg --version");
        Console.WriteLine("  folderjpg \"C:\\Users\\Usuario\\Imagens\"");
    }

    // HELP ZH - help in chinese
    static void DisplayHelpChinese()
    {
        Console.WriteLine("folderjpg v" + VERSION);
        Console.WriteLine();
        Console.WriteLine("命令行工具，可递归地为所有 'folder.jpg' 和 'cover.jpg' 文件创建文件夹图标。");
        Console.WriteLine();
        Console.WriteLine("用法:");
        Console.WriteLine("  folderjpg [选项] <路径>");
        Console.WriteLine();
        Console.WriteLine("选项:");
        Console.WriteLine("  --help     显示此帮助文本");
        Console.WriteLine("  --lang xx  强制语言");
        Console.WriteLine("  --version  显示程序版本");
        Console.WriteLine();
        Console.WriteLine("参数:");
        Console.WriteLine("  路径       要处理的目录路径");
        Console.WriteLine("             如果未提供路径，则不执行任何操作");
        Console.WriteLine("             如果找不到路径，则显示错误消息");
        Console.WriteLine("             如果路径不是目录，则显示错误消息");
        Console.WriteLine("             如果路径是文件，则显示错误消息");
        Console.WriteLine("             如果路径是目录，则程序会递归处理");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  folderjpg");
        Console.WriteLine("  folderjpg --help");
        Console.WriteLine("  folderjpg --lang zh");
        Console.WriteLine("  folderjpg --version");
        Console.WriteLine("  folderjpg \"C:\\Users\\用户\\图片\"");
    }

}// End of Program
