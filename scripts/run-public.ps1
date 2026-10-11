# Public (visitor) tests: no login needed. Safe for DEV and production (read-only).
param([string]$Environment = "Dev", [ValidateSet("Chrome", "Edge")][string]$Browser = "Chrome", [switch]$Headless, [int]$Workers = 1)
. "$PSScriptRoot\_common.ps1"
Invoke-CarabiaTests -Filter "TestCategory=Public&TestCategory!=RequiresManualAuthentication" -Environment $Environment -Browser $Browser -Headless:$Headless -Workers $Workers
