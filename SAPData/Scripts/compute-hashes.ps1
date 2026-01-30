param (
    [string]$RawFolder = "SAPData/Data/Raw",
    [string]$HashFolder = "SAPData/hashes",
    [switch]$CompareOnly
)

if (-not (Test-Path $HashFolder)) {
    New-Item -ItemType Directory -Path $HashFolder | Out-Null
}

# Track if anything changed
$global:Changed = $false

# Iterate over all raw CSVs
Get-ChildItem -Path $RawFolder -Filter *.csv | ForEach-Object {
    $file = $_.FullName
    $name = $_.Name
    $hashPath = Join-Path $HashFolder "$name.hash"

    # Compute hash
    $newHash = (Get-FileHash -Algorithm SHA256 -Path $file).Hash

    if (Test-Path $hashPath) {
        $oldHash = Get-Content $hashPath -Raw
        if ($oldHash -ne $newHash) {
            Write-Host "CHANGED: $name"
            $global:Changed = $true

            if (-not $CompareOnly) {
                Set-Content -Path $hashPath -Value $newHash
            }
        }
        else {
            Write-Host "UNCHANGED: $name"
        }
    }
    else {
        Write-Host "NEW FILE: $name"
        $global:Changed = $true

        if (-not $CompareOnly) {
            Set-Content -Path $hashPath -Value $newHash
        }
    }
}

if ($CompareOnly) {
    if ($global:Changed) {
        Write-Output "CHANGED"
        exit 0
    } else {
        Write-Output "UNCHANGED"
        exit 1
    }
}
