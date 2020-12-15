# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Load Settings
. ".\Settings.ps1"

if ($args.Count -lt 1) 
{
    echo "Usage: ./PackAndInjectDat.ps1 <DatName>"
    echo "DatName corresponds to a folder inside $archivePath and is case sensitive, using camelcase. e.g. Stage.dat, StTex.dat"
    echo "If you are working with the final version of the game, you should add a backslash e.g. \Stage.dat"
    exit -1
}

# Scan the executable.
$datName       = $args[0];

$datOutputPath = "$sourcePath/$datName"

$datSourcePath = "$archivePath/$datName"
$jsonPath      = "$archivePath/$datName.json"

& $toolsPath/Build/RidersPS2ArchiveTool.exe pack --sourcefolder "$datSourcePath" --sourcejson "$jsonPath" --jsonpath "$jsonPath" --datpath "$datOutputPath"
& $toolsPath/Build/RidersPS2ArchiveTool.exe inject --file "$elfPath" --jsonpath "$jsonPath" --dat "$datName"

# Restore Working Directory
Pop-Location