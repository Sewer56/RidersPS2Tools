# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Load Settings
. ".\Settings.ps1"


if ($args.Count -lt 1) 
{
    echo "Usage: ./ExtractAllArchives.ps1 <DatName>"
    echo "Tries to extract all archives inside $archivePath/<DatName>"
    exit -1
}

$extractFolderPath = "$archivePath/" + $args[0]
$allFiles = Get-ChildItem -File -Path $extractFolderPath

foreach ($file in $allFiles) 
{
    $savePath = $file.FullName + "-out"
    & $toolsPath/Build/RidersArchiveTool.exe extract --source $file.FullName --savepath $savePath
}

# Restore Working Directory
Pop-Location