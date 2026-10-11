# Buyer tests (personal area, profile, offers as buyer). First run: log in manually as the BUYER test account.
param([string]$Environment = "Dev", [ValidateSet("Chrome", "Edge")][string]$Browser = "Chrome")
. "$PSScriptRoot\_common.ps1"
Invoke-CarabiaTests -Filter "TestCategory=Buyer" -Environment $Environment -Browser $Browser
