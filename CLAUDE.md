# Working with the folderjpg Project

## Project Overview

folderjpg is a command-line utility that creates custom folder icons in Windows by:
1. Recursively scanning directories for image files named "folder.jpg", "cover.jpg", or "front.jpg"
2. Converting these images to multi-resolution .ico files
3. Creating and configuring desktop.ini files to apply these icons to their respective folders
4. Setting appropriate file attributes to ensure Windows displays the custom icons

This tool is particularly useful for media libraries (music albums, ebook collections, etc.) where cover art images are already present in folders.

## Development Environment Setup

### Prerequisites
- .NET SDK (latest version recommended)
- Visual Studio, Visual Studio Code, or any .NET-compatible IDE
- Git (for version control)
- Magick.NET library (handled via NuGet package management)

### Getting Started
1. Clone the repository or download the source code
2. Open a terminal in the project directory
3. Restore NuGet packages:
   ```
   dotnet restore
   ```

## Project Structure

- **Program.cs**: Main application code containing:
  - Entry point and command-line argument handling
  - Directory processing logic
  - Image conversion functionality
  - desktop.ini file creation
  - Helper methods for random string generation and language detection

- **folderjpg.csproj**: Project configuration file defining:
  - Target framework
  - Dependencies (Magick.NET)
  - Build settings

## Build Instructions

### Standard Debug Build
```
dotnet build
```

### Release Build with Libraries
```
dotnet publish -c Release
```
By default, output is copied to `c:\ulb\folderjpg\`. You can modify this in the .csproj file.

### Single-File Windows Executable
```
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

### Single-File Linux Executable
```
dotnet publish -r linux-x64 -c Release --self-contained=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

## Key Code Components

### Main Processing Logic
The application recursively processes directories looking for specific image files:
- `ProcessDirectory()` method handles the recursive directory traversal
- For each directory containing a target image file, it:
  1. Checks if desktop.ini already exists (skips if it does)
  2. Generates a random string for the icon filename
  3. Converts the image to an icon file
  4. Creates a desktop.ini file pointing to the icon
  5. Sets appropriate file attributes
  6. Refreshes the icon cache (Windows only)

### Image Conversion
The `ConvertToIcon()` method uses Magick.NET to:
- Load the source image
- Create multiple icon sizes (16, 32, 48, 64, 128, 256 pixels)
- Maintain aspect ratio during resizing
- Save the result as a multi-resolution .ico file

### Command-Line Interface
The application supports several command-line options:
- `--help`, `-help`, `-h`: Display help information
- `--lang [language]`: Force a specific language (en/es)
- `--version`, `-version`, `--v`, `-v`: Display version information
- `[path]`: Directory path to process recursively

## Modifying the Code

### Version Updates
Update the VERSION constant in Program.cs when making changes:
```csharp
public const string VERSION = "2025.05.03.1412";
```

### Debug Mode
Set the DEBUG constant to "true" to enable debug mode:
```csharp
private const string DEBUG = "true";
```
In debug mode, the application will only display information without making changes.

### Adding Support for New Image Files
To add support for additional image filenames:
1. Add a new array to collect the files in `ProcessDirectory()`:
   ```csharp
   string[] newImageFiles = Directory.GetFiles(directory, "newname.jpg", SearchOption.TopDirectoryOnly);
   ```
2. Concatenate with the existing array:
   ```csharp
   jpgFiles = jpgFiles.Concat(newImageFiles).ToArray();
   ```

### Adding New Languages
To add support for a new language:
1. Create a new method similar to `DisplayHelpEnglish()` or `DisplayHelpSpanish()`
2. Update the `DisplayHelp()` method to include the new language

## Testing

The project includes a ProgramTests.cs file for unit tests. Run tests with:
```
dotnet test
```

## Common Issues

1. **Missing NuGet Packages**: If you encounter errors about missing Magick.NET, run `dotnet restore`

2. **Permission Issues**: The application requires write access to the directories it processes. Run with appropriate permissions.

3. **Icon Cache Refresh**: Windows may not immediately display the new icons. The application attempts to refresh the icon cache, but a system restart may be required in some cases.

4. **Linux Compatibility**: While the application can be compiled for Linux, the icon functionality is Windows-specific.

## Deployment

For deployment, use the single-file executable build option for the target platform. The resulting executable is self-contained and doesn't require additional dependencies.
