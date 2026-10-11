# Administrator tests (all Areas/Administrator modules, read-only by default). First run: log in manually as an admin.
param([string]$Environment = "Dev", [ValidateSet("Chrome", "Edge")][string]$Browser = "Chrome")
. "$PSScriptRoot\_common.ps1"
Invoke-CarabiaTests -Filter "TestCategory=Admin" -Environment $Environment -Browser $Browser
