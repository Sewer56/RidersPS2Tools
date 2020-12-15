# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Load Settings
. ".\Settings.ps1"

if ($args.Count -lt 1) 
{
    echo "Usage: ./ExtractDat.ps1 <DatName>"
    echo "DatName is case sensitive and uses camelcase. e.g. Stage.dat, StTex.dat"
    echo "If you are working with the final version of the game, you should add a backslash e.g. \Stage.dat"
    exit -1
}

# Scan the executable.
$datName        = $args[0];
$sourceDatPath  = "$sourcePath/$datName"

$jsonPath       = "$archivePath/$datName.json"
$outputDatPath  = "$archivePath/$datName"

& $toolsPath/Build/RidersPS2ArchiveTool.exe scan --file "$elfPath" --dat "$datName" --output "$jsonPath"
& $toolsPath/Build/RidersPS2ArchiveTool.exe extract --file "$sourceDatPath" --jsonpath "$jsonPath" --output "$outputDatPath"

# Restore Working Directory
Pop-Location