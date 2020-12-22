# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Load Settings
. ".\Settings.ps1"


if ($args.Count -lt 1) 
{
    echo "Usage: ./PackAllArchives.ps1 <DatName>"
    echo "Tries to pack all archives inside $archivePath/<DatName>"
    exit -1
}

# Find extracted data.
$extractFolderPath = "$archivePath/" + $args[0]
$allDirectories = Get-ChildItem -Directory -Path $extractFolderPath | where { $_.FullName.EndsWith("-out") }

# Get all sources and destinations
$sources = New-Object string[] $allDirectories.Length;
$destinations = New-Object string[] $allDirectories.Length;

for($x = 0; $x -lt $allDirectories.Length; $x++)
{
    $directory  = $allDirectories[$x]
    $saveFolder = [System.IO.Path]::GetDirectoryName($directory.FullName)
    $saveFile   = [System.IO.Path]::Combine($saveFolder, $directory.FullName.Substring(0, $directory.FullName.Length - 4))
 
    $sources[$x] = $directory.FullName
    $destinations[$x] = $saveFile
}

# Write parameters to disk.
$sourcesFilePath      = "$tempPath/Sources.txt"
$destinationsFilePath = "$tempPath/Destinations.txt"

[System.IO.File]::WriteAllLines($sourcesFilePath, $sources)
[System.IO.File]::WriteAllLines($destinationsFilePath, $destinations)

# Run tool
& $toolsPath/Build/RidersArchiveTool.exe packall --sources $sourcesFilePath --savepaths $destinationsFilePath

# Restore Working Directory
Pop-Location