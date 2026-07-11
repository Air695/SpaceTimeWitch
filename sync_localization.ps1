<#
.SYNOPSIS
    Localization auto-completion script for Slay the Spire 2 mod.
    Scans .cs files under Cards/Powers/Relics and extension/ (all subdirs),
    then fills missing entries in zhs localization JSON.
    Usage: powershell -ExecutionPolicy Bypass -File sync_localization.ps1
    Or double-click run_sync.bat
#>

$ErrorActionPreference = "Continue"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

# ── Read file (with fallback) ───────────────────────────

function ReadFile([string]$path) {
    if (-not (Test-Path $path)) { return $null }
    try {
        $raw = Get-Content $path -Raw -Encoding UTF8 -ErrorAction Stop
        if ($raw) { return $raw }
    } catch {}
    try {
        $raw = Get-Content $path -Raw -ErrorAction Stop
        if ($raw) { return $raw }
    } catch {}
    return $null
}

# ── Helpers ─────────────────────────────────────────────

function PascalToUpperSnake([string]$name) {
    if (-not $name) { return "" }
    $r = $name -creplace '(?<=[a-z])(?=[A-Z])', '_'
    $r = $r -creplace '(?<=[A-Z])(?=[A-Z][a-z])', '_'
    return $r.ToUpper()
}

function IsCommentedOut([string]$path) {
    $text = ReadFile $path
    if (-not $text) { return $false }
    $trimmed = $text.Trim()
    if (-not $trimmed) { return $false }
    return $trimmed.StartsWith("/*") -and $trimmed.Contains("*/")
}

function LoadJson([string]$path) {
    $content = ReadFile $path
    if (-not $content) { return @{} }
    try {
        $obj = $content | ConvertFrom-Json
        $ht = @{}
        foreach ($prop in $obj.PSObject.Properties) { $ht[$prop.Name] = $prop.Value }
        return $ht
    } catch { return @{} }
}

function SaveJson([string]$path, [hashtable]$data) {
    $ordered = [ordered]@{}
    foreach ($key in ($data.Keys | Sort-Object)) { $ordered[$key] = $data[$key] }
    $ordered | ConvertTo-Json -Depth 2 | Out-File $path -Encoding UTF8
}

# ── Detect mod id ───────────────────────────────────────

$modId = $null
$jsonFiles = Get-ChildItem $scriptDir -Filter "*.json" -ErrorAction SilentlyContinue
if ($jsonFiles) {
    foreach ($f in $jsonFiles) {
        $content = ReadFile $f.FullName
        if (-not $content) { continue }
        try {
            $j = $content | ConvertFrom-Json
            if ($j.id -and $j.has_dll) { $modId = $j.id; break }
        } catch {}
    }
}
if (-not $modId) { $modId = Split-Path $scriptDir -Leaf }
Write-Host "Mod ID: $modId"

# ── Find localization directory ─────────────────────────

$locDir = $null
$candidates = @($modId, $modId.ToLower(), $modId.ToUpper())
foreach ($c in $candidates) {
    $p = Join-Path $scriptDir "$c\localization\zhs"
    if (Test-Path $p) { $locDir = $p; break }
}
if (-not $locDir) {
    $subdirs = Get-ChildItem $scriptDir -Directory -ErrorAction SilentlyContinue
    if ($subdirs) {
        foreach ($d in $subdirs) {
            $p = Join-Path $d.FullName "localization\zhs"
            if (Test-Path $p) { $locDir = $p; break }
        }
    }
}
if (-not $locDir) {
    Write-Host "[ERROR] cannot find localization/zhs" -ForegroundColor Red
    exit 1
}

# ── Type configuration ──────────────────────────────────

$types = @(
    @{ Attr="RegisterCard";  Type="CARD";  File="cards.json";   Props=@("title","description") },
    @{ Attr="RegisterPower"; Type="POWER"; File="powers.json";  Props=@("title","description","smartDescription") },
    @{ Attr="RegisterRelic"; Type="RELIC"; File="relics.json";  Props=@("title","description","flavor") }
)

# ── Scan and generate ───────────────────────────────────

$modIdUpper = PascalToUpperSnake $modId
$totalAdded = 0
$csDirs = @("Cards", "Powers", "Relics")

# Collect ALL .cs files under extension/ (recursive, any subdirectory)
$extCsFiles = @()
$extRoot = Join-Path $scriptDir "extension"
if (Test-Path $extRoot) {
    $extCsFiles = @(Get-ChildItem $extRoot -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue)
    if ($extCsFiles.Count -gt 0) {
        Write-Host "Found $($extCsFiles.Count) .cs file(s) under extension/"
    }
}

function ProcessCsFile([string]$csPath, $t, $data) {
    if (IsCommentedOut $csPath) { return $data, 0 }
    $text = ReadFile $csPath
    if (-not $text) { return $data, 0 }
    $added = 0

    $classMatches = [regex]::Matches($text, 'class\s+(\w+)\s*:')
    foreach ($match in $classMatches) {
        $className = $match.Groups[1].Value
        $before = $text.Substring(0, $match.Index)
        if ($before -notmatch "\[$($t.Attr)(?:\]|\()") { continue }

        $prefix = "$modIdUpper`_$($t.Type)`_$(PascalToUpperSnake $className)"
        foreach ($prop in $t.Props) {
            $key = "$prefix.$prop"
            if (-not $data.ContainsKey($key)) {
                $data[$key] = "TODO"
                $added++
                Write-Host "  + $key"
            }
        }
    }
    return $data, $added
}

foreach ($t in $types) {
    $jsonPath = Join-Path $locDir $t.File
    $data = LoadJson $jsonPath

    # Scan main directories: Cards / Powers / Relics
    foreach ($csDir in $csDirs) {
        $fullDir = Join-Path $scriptDir $csDir
        if (-not (Test-Path $fullDir)) { continue }

        $csFiles = Get-ChildItem $fullDir -Recurse -Filter "*.cs" -ErrorAction SilentlyContinue
        if (-not $csFiles) { continue }
        foreach ($csFile in $csFiles) {
            $result = ProcessCsFile $csFile.FullName $t $data
            $data = $result[0]; $totalAdded += $result[1]
        }
    }

    # Scan extension/ recursively (all subdirectories)
    foreach ($csFile in $extCsFiles) {
        $result = ProcessCsFile $csFile.FullName $t $data
        $data = $result[0]; $totalAdded += $result[1]
    }

    SaveJson $jsonPath $data
}

if ($totalAdded -eq 0) {
    Write-Host "All entries up to date."
} else {
    Write-Host ""
    Write-Host "Added $totalAdded missing entries."
}
