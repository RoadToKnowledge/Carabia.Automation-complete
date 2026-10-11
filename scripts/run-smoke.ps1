# Smoke tests. By default only the public smoke set; -IncludeAuthenticated adds Buyer/Seller/Admin smoke
# (uses saved sessions or asks you to log in manually once per role).
param([string]$Environment = "Dev", [ValidateSet("Chrome", "Edge")][string]$Browser = "Chrome", [switch]$Headless, [switch]$IncludeAuthenticated)
. "$PSScriptRoot\_common.ps1"
$filter = if ($IncludeAuthenticated) { "TestCategory=Smoke" } else { "TestCategory=Smoke&TestCategory!=RequiresManualAuthentication" }
Invoke-CarabiaTests -Filter $filter -Environment $Environment -Browser $Browser -Headless:$Headless
