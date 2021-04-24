# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Load Settings
. ".\Settings.ps1"

# Build
dotnet publish $toolsPath/RidersPS2ArchiveExtractor/RidersPS2ArchiveExtractor.sln -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishReadyToRun=true --self-contained=false -o $toolsPath/Build
dotnet publish $toolsPath/RidersArchiveMemoryRipTool/RidersArchiveMemoryRipTool.sln -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishReadyToRun=true --self-contained=false -o $toolsPath/Build
dotnet publish $toolsPath/RidersArchiveTool/RidersArchiveTool.sln -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishReadyToRun=true --self-contained=false -o ./Tools/Build
dotnet publish $toolsPath/RidersTextureArchiveTool/RidersTextureArchiveTool.sln -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishReadyToRun=true --self-contained=false -o ./Tools/Build


# Restore Working Directory
Pop-Location