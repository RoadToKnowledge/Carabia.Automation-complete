# Seller tests (sell flow to summary, My Garage selling, offer responses). First run: log in manually as the SELLER test account.
param([string]$Environment = "Dev", [ValidateSet("Chrome", "Edge")][string]$Browser = "Chrome")
. "$PSScriptRoot\_common.ps1"
Invoke-CarabiaTests -Filter "TestCategory=Seller" -Environment $Environment -Browser $Browser
