# Public tests in Microsoft Edge.
param([string]$Environment = "Dev", [switch]$Headless)
. "$PSScriptRoot\_common.ps1"
Invoke-CarabiaTests -Filter "TestCategory=Public&TestCategory!=RequiresManualAuthentication" -Environment $Environment -Browser Edge -Headless:$Headless
