# Reads version from AssemblyInfo.cs and stamps it into viewer files
param([string]$ProjectDir)

# Clean trailing backslash and quotes from MSBuild $(ProjectDir)
$ProjectDir = $ProjectDir.Trim('"').TrimEnd('\')

$asmFile = Join-Path $ProjectDir "Properties\AssemblyInfo.cs"
$jsFile  = Join-Path $ProjectDir "viewer\stats_renderer.js"
$htmlFile = Join-Path $ProjectDir "viewer\stats_viewer.html"

$match = Select-String -Path $asmFile -Pattern '^\[assembly: AssemblyVersion\("([^"]+)"\)'
$parts = $match.Matches[0].Groups[1].Value.Split('.')
$patch = $parts[2]
$ver = if ($patch -ne '*' -and [int]$patch -gt 0) { "$($parts[0]).$($parts[1]).$patch" } else { "$($parts[0]).$($parts[1])" }

# Stamp JS version constant
if (Test-Path $jsFile) {
    (Get-Content $jsFile -Encoding UTF8) -replace "const LFS_STATS_VERSION = '[^']*'", "const LFS_STATS_VERSION = '$ver'" | Set-Content $jsFile -Encoding UTF8
}

# Stamp HTML cache busting (?v=x.y.z)
if (Test-Path $htmlFile) {
    (Get-Content $htmlFile -Encoding UTF8) -replace '\?v=[0-9]+\.[0-9]+(\.[0-9]+)?', "?v=$ver" | Set-Content $htmlFile -Encoding UTF8
}

Write-Host "Stamped viewer version: $ver"
