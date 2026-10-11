# Public tests in Chrome.
param([string]$Environment = "Dev", [switch]$Headless)
. "$PSScriptRoot\_common.ps1"
Invoke-CarabiaTests -Filter "TestCategory=Public&TestCategory!=RequiresManualAuthentication" -Environment $Environment -Browser Chrome -Headless:$Headless
