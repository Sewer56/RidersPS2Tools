# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location

# Load Settings
. ".\Settings.ps1"

# Start ImgBurn to build ISO
& $imgburnPath /mode build /src $sourcePath /dest ./$isoName /filesystem `"ISO9660 + UDF`" /udfrevision `"1.02`" /volumelabel $isoLabel /rootfolder yes /noimagedetails /overwrite yes /start /close

# Restore Working Directory
Pop-Location