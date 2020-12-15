echo "Applying Settings"
[Environment]::CurrentDirectory = $PSScriptRoot

# Common Settings
$sourcePath = "./Working"					  # Path to extracted game ISO.
$toolsPath  = "./Tools"						  
$elfPath    = $sourcePath + "/SLUS_213.31"    # PS2 Executable
$tempPath   = [System.IO.Path]::GetTempPath() # Currently unused.
$archivePath = "./Data"					      # Where archives are saved.

# ISO Building Settings
$imgburnPath = "C:\Program Files (x86)\ImgBurn\ImgBurn.exe"
$isoName = "Build.iso"
$isoLabel = "Riders 0.931 Proto Restoration"

# Just in Case
[System.IO.Directory]::CreateDirectory($tempPath) | Out-Null