$ErrorActionPreference = 'Inquire'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$bepInExUrl = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip"
$cfgUrl = "https://github.com/nyakowint/xsoverlay-keyboard-osc/releases/latest/download/BepInEx.cfg"
$modUrl = "https://github.com/nyakowint/xsoverlay-keyboard-osc/releases/latest/download/KeyboardOSC.dll"

Write-Host "Downloading BepInEx..."
Invoke-WebRequest -Uri $bepInExUrl -OutFile "./BepInEx.zip"

Write-Host "Extracting into current directory..."
Expand-Archive -Path "./BepInEx.zip" -DestinationPath "./" -Force
Remove-Item -Path "./BepInEx.zip"

New-Item -ItemType Directory -Path "./BepInEx/config" -Force
New-Item -ItemType Directory -Path "./BepInEx/plugins" -Force

Write-Host "Downloading KeyboardOSC..."
Invoke-WebRequest -Uri $cfgUrl -OutFile "./BepInEx/config/BepInEx.cfg"
Invoke-WebRequest -Uri $modUrl -OutFile "./BepInEx/plugins/KeyboardOSC.dll"

Write-Host "Install complete! Check the plugin's README for usage instructions :D" -ForegroundColor Green