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

$extractFolderPath = "$archivePath/" + $args[0]
$allDirectories = Get-ChildItem -Directory -Path $extractFolderPath | where { $_.FullName.EndsWith("-out") }

foreach ($directory in $allDirectories) 
{
    $saveFolder = [System.IO.Path]::GetDirectoryName($directory.FullName)
    $saveFile = [System.IO.Path]::Combine($saveFolder, $directory.FullName.Substring(0, $directory.FullName.Length - 4))
 
    echo "Saving: $saveFile"   
    & $toolsPath/Build/RidersArchiveTool.exe pack --source $directory.FullName --savepath $saveFile
}

# Restore Working Directory
Pop-Location