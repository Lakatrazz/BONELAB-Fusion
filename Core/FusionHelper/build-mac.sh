#!/bin/bash

# Check if the script is run with sudo
if [ "$EUID" -ne 0 ]; then
    echo "Please run this script with superuser privileges (sudo)."
    exit 1
fi

# Check for the number of arguments
if [ "$#" -ne 2 ]; then
    echo "Usage: $0 <BuildType> <Architecture>"
    exit 1
fi

# Assign the command-line arguments to variables
build_type=$1
architecture=$2

# Set the appropriate configuration and architecture flags
configuration="${build_type}-Mac"
architecture_flag="x64"

if [ "$(echo "$architecture" | tr '[:upper:]' '[:lower:]')" = "arm64" ]; then
    architecture_flag="arm64"
fi

# Set the path to the FusionHelper.app
fusion_helper_path="./bin/${configuration}/net6.0-macos11.0/osx-${architecture_flag}/FusionHelper.app"

# Clear old data
echo "Deleting old FusionHelper build..."
rm -R "${fusion_helper_path}"

# Perform the dotnet publish command with the specified configuration and architecture flags
echo "Compiling FusionHelper for ${architecture_flag} in ${build_type} mode..."
dotnet publish -a $architecture_flag -c $configuration --sc true

echo "Replacing default application launcher with terminal launcher..."

# Rename the file FusionHelper.app/Contents/MacOS/FusionHelper to FusionHelper.app/Contents/MacOS/FH_Loader
mv "${fusion_helper_path}/Contents/MacOS/FusionHelper" "${fusion_helper_path}/Contents/MacOS/FH_Loader"

# Copy a file from a variable path to FusionHelper.app/Contents/MacOS/FusionHelper
cp "./Resources/FusionHelperTerminalLauncher" "${fusion_helper_path}/Contents/MacOS/FusionHelper"

# Write SteamVR app id to FusionHelper.app/Contents/MacOS/steam_appid.txt
echo "Writing SteamVR app id..."
echo "250820" > "${fusion_helper_path}/Contents/MacOS/steam_appid.txt"

echo "Adding icon to FusionHelper.app..."

# Create the folder FusionHelper.app/Contents/Resources
mkdir -p "${fusion_helper_path}/Contents/Resources"

# Copy a variable file path to FusionHelper.app/Contents/Resources/fusion.icns
cp "./Resources/fusion.icns" "${fusion_helper_path}/Contents/Resources/fusion.icns"

# Modify FusionHelper.app/Contents/Info.plist to add an 'Icon file' entry set to 'fusion.icns'
plutil -insert "CFBundleIconFile" -string "fusion.icns" "${fusion_helper_path}/Contents/Info.plist"

echo "Done."