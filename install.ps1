$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

# Display menu and get user choice
function Show-Menu {
    Clear-Host
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "   XSOverlay Keyboard Chatbox Installer" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Please select an option using your number keys:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1. Install" -ForegroundColor Green
    Write-Host "  2. Update" -ForegroundColor Blue
    Write-Host "  3. Remove BepInEx & Plugins (Uninstall)" -ForegroundColor Red
    Write-Host "  4. Install XSO Font Changer by chaixshot (fix fonts being squares in some locales)" -ForegroundColor Gray
    Write-Host "  5. Exit" -ForegroundColor Gray
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
}

function Get-XSOverlayPath {
    Write-Host ""
    Write-Host "Attempting to locate XSOverlay installation..." -ForegroundColor Yellow
    
    $commonPaths = @(
        "C:\Program Files (x86)\Steam\steamapps\common\XSOverlay_Beta",
        "C:\SteamLibrary\steamapps\common\XSOverlay_Beta",
        "D:\SteamLibrary\steamapps\common\XSOverlay_Beta",
        "E:\SteamLibrary\steamapps\common\XSOverlay_Beta",
        "F:\SteamLibrary\steamapps\common\XSOverlay_Beta"
        "G:\SteamLibrary\steamapps\common\XSOverlay_Beta"
        "H:\SteamLibrary\steamapps\common\XSOverlay_Beta"
        # no i am NOT adding all of the possible drive letters
    )
    
    # Check common paths or manually enter
    foreach ($path in $commonPaths) {
        if (Test-Path $path) {
            Write-Host "Found XSOverlay at: $path" -ForegroundColor Green
            $confirm = Read-Host "Use this path? (Y/n)"
            if ($confirm -eq '' -or $confirm -eq 'Y' -or $confirm -eq 'y') {
                return $path
            }
        }
    }
    
    Write-Host ""
    Write-Host "Please enter the full path to your XSOverlay installation folder:" -ForegroundColor Yellow
    Write-Host "(Example: H:\SteamLibrary\steamapps\common\XSOverlay_Beta)" -ForegroundColor Gray
    
    do {
        $path = Read-Host "Path"
        if (Test-Path $path) {
            # Verify it's the correct folder by checking for XSOverlay.exe
            if (Test-Path (Join-Path $path "XSOverlay.exe")) {
                Write-Host "Valid XSOverlay installation found!" -ForegroundColor Green
                return $path
            } else {
                Write-Host "Warning: XSOverlay.exe was not found in this folder. Continue anyway? (y/N)" -ForegroundColor Yellow
                $confirm = Read-Host
                if ($confirm -eq 'y' -or $confirm -eq 'Y') {
                    return $path
                }
            }
        } else {
            Write-Host "Path not found. Please try again." -ForegroundColor Red
        }
    } while ($true)
}

function Install-Plugin {
    param($xsoPath)
    
    Write-Host ""
    Write-Host "=== Installing KeyboardChatbox plugin ===" -ForegroundColor Green
    Write-Host ""
    
    $bepInExUrl = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip"
    $cfgUrl = "https://github.com/nyakowint/xsoverlay-keyboard-osc/releases/latest/download/BepInEx.cfg"
    $pluginUrl = "https://github.com/nyakowint/xsoverlay-keyboard-osc/releases/latest/download/KeyboardOSC.dll"
    
    $tempZip = Join-Path $env:TEMP "BepInEx.zip"
    
    try {
        # Check if BepInEx is already installed
        $bepInExPath = Join-Path $xsoPath "BepInEx"
        if (Test-Path $bepInExPath) {
            Write-Host "BepInEx is already installed. Skipping BepInEx installation..." -ForegroundColor Yellow
        } else {
            Write-Host "Downloading BepInEx..." -ForegroundColor Cyan
            Invoke-WebRequest -Uri $bepInExUrl -OutFile $tempZip
            
            Write-Host "Extracting BepInEx..." -ForegroundColor Cyan
            Expand-Archive -Path $tempZip -DestinationPath $xsoPath -Force
            Remove-Item -Path $tempZip -Force
            
            Write-Host "BepInEx installed successfully!" -ForegroundColor Green
        }
        
        # Create necessary directories
        $configPath = Join-Path $xsoPath "BepInEx\config"
        $pluginsPath = Join-Path $xsoPath "BepInEx\plugins"
        
        New-Item -ItemType Directory -Path $configPath -Force | Out-Null
        New-Item -ItemType Directory -Path $pluginsPath -Force | Out-Null
        
        Write-Host "Downloading KeyboardChatbox plugin..." -ForegroundColor Cyan
        Invoke-WebRequest -Uri $cfgUrl -OutFile (Join-Path $configPath "BepInEx.cfg")
        Invoke-WebRequest -Uri $pluginUrl -OutFile (Join-Path $pluginsPath "KeyboardOSC.dll")
        
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "Installation complete!" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "Check the plugin's README for usage instructions and important info :D" -ForegroundColor Yellow
        Write-Host "!! / Please remove the plugin before reporting XSOverlay bugs to ensure they are not caused by KBChat! \ !!" -ForegroundColor Yellow
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "Error during installation: $_" -ForegroundColor Red
        Write-Host ""
    }
}

function Update-Plugin {
    param($xsoPath)
    
    Write-Host ""
    Write-Host "=== Updating KeyboardChatbox plugin ===" -ForegroundColor Blue
    Write-Host ""
    
    $cfgUrl = "https://github.com/nyakowint/xsoverlay-keyboard-osc/releases/latest/download/BepInEx.cfg"
    $pluginUrl = "https://github.com/nyakowint/xsoverlay-keyboard-osc/releases/latest/download/KeyboardOSC.dll"
    
    $configPath = Join-Path $xsoPath "BepInEx\config"
    $pluginsPath = Join-Path $xsoPath "BepInEx\plugins"
    
    try {
        if (-not (Test-Path $pluginsPath)) {
            Write-Host "KeyboardChatbox does not appear to be installed." -ForegroundColor Red
            Write-Host "Please install it first (option 1)." -ForegroundColor Yellow
            return
        }
        
        Write-Host "Downloading latest version..." -ForegroundColor Cyan
        Invoke-WebRequest -Uri $cfgUrl -OutFile (Join-Path $configPath "BepInEx.cfg")
        Invoke-WebRequest -Uri $pluginUrl -OutFile (Join-Path $pluginsPath "KeyboardOSC.dll")
        
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "Update complete!" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "Error during update: $_" -ForegroundColor Red
        Write-Host ""
    }
}

function Remove-Plugin {
    param($xsoPath)
    
    Write-Host ""
    Write-Host "=== Removing KeyboardChatbox plugin ===" -ForegroundColor Red
    Write-Host ""
    
    $bepInExPath = Join-Path $xsoPath "BepInEx"
    
    if (-not (Test-Path $bepInExPath)) {
        Write-Host "BepInEx/KeyboardChatbox does not appear to be installed." -ForegroundColor Yellow
        return
    }
    
    Write-Host "HOLD UP! This will remove the BepInEx folder and all plugins." -ForegroundColor Red
    Write-Host "Do you want to continue? (y/N)" -ForegroundColor Yellow
    $confirm = Read-Host
    
    if ($confirm -eq 'y' -or $confirm -eq 'Y') {
        try {
            Write-Host "Removing BepInEx and all plugins..." -ForegroundColor Cyan
            Remove-Item -Path $bepInExPath -Recurse -Force
            
            # Also remove other BepInEx files
            $doorstopConfig = Join-Path $xsoPath "doorstop_config.ini"
            $winhttp = Join-Path $xsoPath "winhttp.dll"
            $changelog = Join-Path $xsoPath "changelog.txt"
            
            if (Test-Path $doorstopConfig) { Remove-Item $doorstopConfig -Force }
            if (Test-Path $winhttp) { Remove-Item $winhttp -Force }
            if (Test-Path $changelog) { Remove-Item $changelog -Force }
            
            Write-Host ""
            Write-Host "=====================================" -ForegroundColor Green
            Write-Host "Removal complete!" -ForegroundColor Green
            Write-Host "=====================================" -ForegroundColor Green
            Write-Host ""
        }
        catch {
            Write-Host ""
            Write-Host "Error during removal: $_" -ForegroundColor Red
            Write-Host ""
        }
    } else {
        Write-Host "Removal cancelled." -ForegroundColor Yellow
    }
}

function Install-Font-Plugin {
    param($xsoPath)
    
    Write-Host ""
    Write-Host "=== Installing xsoverlay-font-changer plugin ===" -ForegroundColor Green
    Write-Host "Font changer is created & maintained at https://github.com/chaixshot/xsoverlay-font-changer, this installer simply provides it for convenience!" -ForegroundColor Yellow
    Write-Host ""
    
    $bepInExUrl = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.22/BepInEx_x64_5.4.22.0.zip"
    $pluginUrl = "https://github.com/chaixshot/xsoverlay-font-changer/releases/download/1.0.0/xsoverlay_font_changer-1.0.0.zip"
    
    $tempZip = Join-Path $env:TEMP "BepInEx.zip"
    $tempFontZip = Join-Path $env:TEMP "xso-font-changer.zip"
    
    try {
        # Check if BepInEx is already installed
        $bepInExPath = Join-Path $xsoPath "BepInEx"
        if (Test-Path $bepInExPath) {
            Write-Host "BepInEx is already installed. Skipping BepInEx installation..." -ForegroundColor Yellow
        } else {
            Write-Host "Downloading BepInEx..." -ForegroundColor Cyan
            Invoke-WebRequest -Uri $bepInExUrl -OutFile $tempZip
            
            Write-Host "Extracting BepInEx..." -ForegroundColor Cyan
            Expand-Archive -Path $tempZip -DestinationPath $xsoPath -Force
            Remove-Item -Path $tempZip -Force
            
            Write-Host "BepInEx installed successfully!" -ForegroundColor Green
        }
        
        Write-Host "Downloading xsoverlay-font-changer plugin..." -ForegroundColor Cyan
        Invoke-WebRequest -Uri $pluginUrl -OutFile $tempFontZip
         
        Write-Host "Extracting font changer..." -ForegroundColor Cyan
        Expand-Archive -Path $tempFontZip -DestinationPath $bepInExPath -Force
        Remove-Item -Path $tempFontZip -Force
        
        Write-Host ""
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "Installation complete!" -ForegroundColor Green
        Write-Host "=====================================" -ForegroundColor Green
        Write-Host "Check the plugin's README for more info: https://github.com/chaixshot/xsoverlay-font-changer" -ForegroundColor Yellow
        Write-Host "!! / Please remove plugins before reporting XSOverlay bugs to ensure they are not caused by any installed plugins! \ !!" -ForegroundColor Yellow
        Write-Host ""
    }
    catch {
        Write-Host ""
        Write-Host "Error during installation: $_" -ForegroundColor Red
        Write-Host ""
    }
}

# Main script execution
do {
    Show-Menu
    $choice = Read-Host "Enter your choice"
    
    switch ($choice) {
        '1' {
            $xsoPath = Get-XSOverlayPath
            Install-Plugin -xsoPath $xsoPath
            Write-Host "Press any key to return to menu..." -ForegroundColor Gray
            $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        }
        '2' {
            $xsoPath = Get-XSOverlayPath
            Update-Plugin -xsoPath $xsoPath
            Write-Host "Press any key to return to menu..." -ForegroundColor Gray
            $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        }
        '3' {
            $xsoPath = Get-XSOverlayPath
            Remove-Plugin -xsoPath $xsoPath
            Write-Host "Press any key to return to menu..." -ForegroundColor Gray
            $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        }
        '4' {
            $xsoPath = Get-XSOverlayPath
            Install-Font-Plugin -xsoPath $xsoPath
            Write-Host "Press any key to return to menu..." -ForegroundColor Gray
            $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
        }
        '5' {
            Write-Host ""
            Write-Host "Goodbye!" -ForegroundColor Cyan
            Write-Host ""
            exit
        }
        default {
            Write-Host ""
            Write-Host "Please select one of the numbered options..." -ForegroundColor Red
            Start-Sleep -Seconds 2
        }
    }
} while ($true)
