param(
    [string]$ConfigFile = "SAPData/raw_sources.json",
    [string]$RawFolder = "SAPData/Data/Raw"
)

$json = Get-Content $ConfigFile | ConvertFrom-Json

if (-not (Test-Path $RawFolder)) {
    New-Item -ItemType Directory -Path $RawFolder | Out-Null
}

function Download-Direct($src) {
    Write-Host "Downloading direct file: $($src.name)"
    Invoke-WebRequest -Uri $src.url -OutFile $src.target -UseBasicParsing
}

function Download-EES($src) {
    Write-Host "Downloading EES file: $($src.name)"

    # 1. Get all releases for the theme
    $releases = Invoke-WebRequest `
        -Uri "https://api-beta.explore-education-statistics.service.gov.uk/api/v1/releases" `
        -UseBasicParsing | ConvertFrom-Json

    # 2. Find the latest matching release
    $release = $releases.results | Where-Object { $_.title -like "*$($src.release)*" } |
               Sort-Object published -Descending | Select-Object -First 1

    if (-not $release) {
        Write-Host "ERROR: No release found for $($src.name)"
        return
    }

    # 3. Get all files in that release
    $files = Invoke-WebRequest `
        -Uri "https://api-beta.explore-education-statistics.service.gov.uk/api/v1/releases/$($release.id)/files" `
        -UseBasicParsing | ConvertFrom-Json

    # 4. Find the specific file
    $file = $files.results | Where-Object { $_.fileName -eq $src.file }

    if (-not $file) {
        Write-Host "ERROR: File not found in release: $($src.file)"
        return
    }

    # 5. Download
    Invoke-WebRequest `
        -Uri "https://api-beta.explore-education-statistics.service.gov.uk/api/v1/releases/$($release.id)/files/$($file.id)/download" `
        -OutFile $src.target `
        -UseBasicParsing
}

foreach ($src in $json.sources) {
    if ($src.type -eq "direct")   { Download-Direct $src }
    elseif ($src.type -eq "ees") { Download-EES $src }
    else { Write-Host "Unknown source type: $($src.type)" }
}
