# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Load Settings
. ".\Settings.ps1"

if ($args.Count -lt 1) 
{
    echo "Usage: ./MemoryRip.ps1 <DatName> <PCSX2LogFile> (<MinRamAddress>)"
    echo "<DatName>: Name of the dat file e.g. Stage.dat"
    echo "<PCSX2LogFile>: Full path to the PCSX2 log file. Hint: PCSX2\logs\emuLog.txt"
    echo "<MinRamAddress>: (Optional, Default 0x30000000) Address to the start of emulated
                 memory for PCSX2. Older versions use (0x20000000). Would
                 suggest launching PCSX2 with Console and Dev/Verbose source
                 enabled and get `EE Main Memory` address range from there.
                 Then convert to decimal and add as argument. Use this if having issues."


    echo ""
    echo "This command is to be used with dat(s) names previously extracted by ExtractDat.ps1"
    echo "This tool is only to be used with SLUS_213.31_MemRip, a specialized version of Sonic Riders 0.931 Restoration project by Sewer56."
    echo "You can find SLUS_213.31_MemRip in the release version of the restoration project."
    echo "Simply swap it with the original SLUS_213.31 and rebuild the ISO."
    exit -1
}

# File Paths
$datName       = $args[0];
$pcsx2LogFile  = $args[1];

$sourceDatPath  = "$archivePath/$datName"
$jsonPath       = "$archivePath/$datName-filesizes.json"
$rippedDataPath = "$archivePath/Ripped/"

& $toolsPath/Build/RidersArchiveMemoryRipTool.exe scan --source "$sourceDatPath" --savepath "$jsonPath"

if ($args.Count -eq 3) 
{
    & $toolsPath/Build/RidersArchiveMemoryRipTool.exe rip --jsonpath "$jsonPath" --logpath "$pcsx2LogFile" --outputpath "$rippedDataPath" --minramaddress $args[2]
}
else 
{
    & $toolsPath/Build/RidersArchiveMemoryRipTool.exe rip --jsonpath "$jsonPath" --logpath "$pcsx2LogFile" --outputpath "$rippedDataPath"
}



# Restore Working Directory
Pop-Location