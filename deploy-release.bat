@echo off
set DOTNET_CLI_TELEMETRY_OPTOUT=1
echo Compilando folderjpg en modo RELEASE (single-file)...
dotnet publish -r win-x64 -c Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true

if %ERRORLEVEL% neq 0 (
    echo ERROR: La compilacion fallo.
    pause
    exit /b %ERRORLEVEL%
)

echo Copiando folderjpg.exe a c:\ulb\folderjpg\...
if not exist "c:\ulb\folderjpg\" mkdir "c:\ulb\folderjpg\"
copy /Y "bin\Release\net8.0\win-x64\publish\folderjpg.exe" "c:\ulb\folderjpg\folderjpg.exe"

if %ERRORLEVEL% neq 0 (
    echo ERROR: La copia fallo.
    pause
    exit /b %ERRORLEVEL%
)

echo Listo. folderjpg.exe desplegado en c:\ulb\folderjpg\
pause
