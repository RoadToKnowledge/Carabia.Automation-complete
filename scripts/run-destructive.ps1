# Write/destructive tests (DEV only). They still skip unless the matching Allow*Tests switch is true in
# config/appsettings.Dev.json. Never run against production (SafetyGuard blocks it anyway).
param([ValidateSet("Chrome", "Edge")][string]$Browser = "Chrome")
. "$PSScriptRoot\_common.ps1"
Invoke-CarabiaTests -Filter "TestCategory=WritesData|TestCategory=Destructive" -Environment "Dev" -Browser $Browser
