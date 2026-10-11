# Full regression. Without -IncludeAuthenticated: everything a visitor can test.
# With it: also Buyer, Seller and Admin suites (manual login once per role, then saved sessions).
# Write tests still need the Allow*Tests switches in config/appsettings.Dev.json.
param([string]$Environment = "Dev", [ValidateSet("Chrome", "Edge")][string]$Browser = "Chrome", [switch]$Headless, [switch]$IncludeAuthenticated)
. "$PSScriptRoot\_common.ps1"
$filter = if ($IncludeAuthenticated) { "TestCategory=Regression|TestCategory=Smoke" } else { "(TestCategory=Regression|TestCategory=Smoke)&TestCategory!=RequiresManualAuthentication" }
Invoke-CarabiaTests -Filter $filter -Environment $Environment -Browser $Browser -Headless:$Headless
