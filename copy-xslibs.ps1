# ai generated script lolol
# Set the paths for source and destination folders
$sourceFolder = "C:\Program Files (x86)\Steam\steamapps\common\XSOverlay_Beta\XSOverlay_Data\Managed"
$destinationFolder = "D:\Stuff\XSOMod\refs"

# Get a list of files in the source folder
$sourceFiles = Get-ChildItem -Path $sourceFolder

# Iterate through each file in the source folder
foreach ($file in $sourceFiles) {
    # Construct the full path for the corresponding file in the destination folder
    $destinationFile = Join-Path $destinationFolder $file.Name

    # Check if the file already exists in the destination folder
    if (Test-Path $destinationFile) {
        # Copy the file to the destination folder
        Copy-Item -Path $file.FullName -Destination $destinationFolder -Force
        Write-Host "Copied $($file.Name) to $($destinationFolder)"
    }
}

PAUSE