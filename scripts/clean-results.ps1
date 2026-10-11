# Deletes run outputs (reports, artifacts, TestResults). Saved sessions are kept unless -Sessions is given.
param([switch]$Sessions)
$root = Split-Path -Parent $PSScriptRoot
foreach ($dir in "artifacts", "reports") {
    Get-ChildItem -LiteralPath (Join-Path $root $dir) -Force -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -notin ".gitkeep", "README.md" } | Remove-Item -Recurse -Force
}
Get-ChildItem -Path $root -Recurse -Directory -Filter TestResults -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
if ($Sessions) {
    Get-ChildItem -LiteralPath (Join-Path $root "sessions") -Directory -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
    Write-Host "Saved sessions deleted; the next authenticated run asks for manual login."
}
Write-Host "Results cleaned."
