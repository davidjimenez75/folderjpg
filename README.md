# folderjpg

folderjpg is a command line tool to create full size icons to Windows folders recursively from all the "folder.jpg" and "cover.jpg" files.



## COMPILATION

For compilation, you need to have the .NET Core SDK installed. You can download it from [here](https://dotnet.microsoft.com/download).

The NuGet packages are not included in the repository. 

You can restore them by running the following command in the root folder of the repository:

```
dotnet restore
```


### Compile Release version with libraries

By default the compilation is copied to c:\ulb\folderjpg\

You can change the output folder by changing the value of the `OutputPath` property in the `folderjpg.csproj` file.

Publish the project by running the following commands in the root folder of the repository:

```
dotnet publish -c Release
```


### Compile Release version in only one folderjpg.exe file

Compile the project by running the following commands in the root folder of the repository:

```
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```


### Compile Release verson in only one folderjpg binary file for Linux

```
dotnet publish -r linux-x64 -c Release --self-contained=true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```


## HELP

Show the help message by running the following command in the root folder of the repository:

```
folderjpg --help
```



## USAGE

For security reasons a path in mandatory to avoid unwanted operations on full disk.

```
folderjpg [options] <path>
```



## EXAMPLES

#### Music folder process all albums

Process the "Music" folder in the user profile folder recursively:

```
folderjpg "%USERPROFILE%\Music"
```

#### Calibre library folder process all books

```
folderjpg "%USERPROFILE%\Calibre Library"
```

## TESTING

The project includes a ProgramTests.cs file for unit tests. Run tests with:
```
dotnet test
```

## Functional Diagram

A functional diagram describing the application's workflow is available in PlantUML format at [docs/functional_diagram.puml](docs/functional_diagram.puml).

To view the diagram, you can use an online PlantUML renderer (like the official one at www.plantuml.com/plantuml) or a local PlantUML tool. Simply copy the content of the `.puml` file into the renderer.
