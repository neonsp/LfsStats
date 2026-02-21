# Creates a versioned release package (LfsStats_vX.Y.Z.zip) in bin/ReleaseDelivery
param([string]$ProjectDir, [string]$OutputDir)

# Clean trailing quotes, backslashes and spaces from MSBuild paths
$ProjectDir = $ProjectDir.Trim().Trim('"').TrimEnd('\')
$OutputDir = $OutputDir.Trim().Trim('"').TrimEnd('\')

# If OutputDir is relative, resolve against ProjectDir
if (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
    $OutputDir = Join-Path $ProjectDir $OutputDir
}

# Read version from AssemblyInfo.cs
$asmFile = Join-Path $ProjectDir "Properties\AssemblyInfo.cs"
$match = Select-String -Path $asmFile -Pattern '^\[assembly: AssemblyVersion\("([^"]+)"\)'
$parts = $match.Matches[0].Groups[1].Value.Split('.')
$patch = $parts[2]
$ver = if ($patch -ne '*' -and [int]$patch -gt 0) { "$($parts[0]).$($parts[1]).$patch" } else { "$($parts[0]).$($parts[1])" }

$deliveryDir = Join-Path $ProjectDir "bin\ReleaseDelivery"
$folderName = "LfsStats_v$ver"
$packageDir = Join-Path $deliveryDir $folderName
$zipFile = Join-Path $deliveryDir "$folderName.zip"

# Clean previous delivery
if (Test-Path $packageDir) { Remove-Item $packageDir -Recurse -Force }
if (Test-Path $zipFile) { Remove-Item $zipFile -Force }

# Copy entire Release output as the versioned folder
Copy-Item $OutputDir $packageDir -Recurse

# Create zip (contains LfsStats_vX.Y.Z/ folder with all contents)
Compress-Archive -Path $packageDir -DestinationPath $zipFile -Force

Write-Host "Package created: $zipFile"
