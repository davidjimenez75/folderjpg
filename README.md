# folderjpg

Create full size icons to Windows folders recursively from all the "folder.jpg" and "cover.jpg" files.


### Compilation

For compilation, you need to have the .NET Core SDK installed. You can download it from [here](https://dotnet.microsoft.com/download).

The NuGet packages are not included in the repository. 

You can restore them by running the following command in the root folder of the repository:

```
dotnet restore
```


#### Compile Release version with libraries

By default the compilation is copied to c:\ulb\folderjpg\

You can change the output folder by changing the value of the `OutputPath` property in the `folderjpg.csproj` file.

Compile the project by running the following commands in the root folder of the repository:

```
dotnet build -c Release
```


#### Compile Release version in only one folderjpg.exe file

By default the compilation is copied to c:\ulb\folderjpg\

You can change the output folder by changing the value of the `OutputPath` property in the `folderjpg.csproj` file.

Compile the project by running the following commands in the root folder of the repository:

```
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```


### Help

Show the help message by running the following command in the root folder of the repository:

```
folderjpg --help
```


### Usage

For security reasons a path in mandatory to avoid unwanted operations on full disk.

```
folderjpg [options] <path>
```






