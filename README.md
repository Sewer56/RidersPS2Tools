# Sonic Riders PS2 Tools

A set of basic tools for working with the PS2 version of Sonic Riders.<br/>
Used for the Riders 0.931 Proto Restoration Project.

## Prerequisites

- **ImgBurn:** Creating new ISOs to test with the emulator.
- **.NET 5:** For compiling my tools.

## Getting Started
- Download and install the prerequisites.
- Create a folder called `Working` and extract your game ISO.
- Open `Settings.ps1`, update `$imgburnPath`. 
- Run `BuildTools.ps1` in Powershell to compile the tools.

## Scripts
These high level scripts abstract the lower level functionality of the tools I created.

- **BuildISO.ps1:** Builds a new ISO file for testing.
- **ExtractDat.ps1:** Extracts a .DAT archive by finding the hardcoded file metadata in the SLUS executable.
- **ExtractAllArchives.ps1:** Extracts all (uncompressed) archives that were inside the DAT archive.

- **PackAllArchives.ps1:** Packs all archives that were unpacked by `ExtractAllArchives`.
- **PackAndInjectDat.ps1:** Creates a new .DAT archive and injects new hardcoded file metadata into the SLUS executable.

Extra Tools:
- **MemoryRip.ps1:** Extracts uncompressed files from game memory with a special version of the 0.931 Prototype Restoration SLUS executable. See more details inside the script itself.

## Project Structure

**Codes:** Various miscellaneous assembly codes personally created by me for various versions of Sonic Riders. Intended to be used with Gtlcpimp's CodeDesigner3. [My own fork here](https://github.com/Sewer56/CodeDesigner3).

**Tools:** Various low level programs created by me to work with the PS2 version of the game. Higher level abstractions provided via scripts.