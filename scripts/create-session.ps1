# Creates or refreshes the saved session for one role. A visible browser opens; if no valid session exists
# you see "MANUAL AUTHENTICATION REQUIRED FOR: <ROLE>" and log in yourself (phone + OTP, or admin password).
# The session is saved to sessions/<role>/session.json (git-ignored).
param([Parameter(Mandatory = $true)][ValidateSet("Buyer", "Seller", "Administrator")][string]$Role,
      [string]$Environment = "Dev", [switch]$Force)
$root = Split-Path -Parent $PSScriptRoot
if ($Force) { Remove-Item -LiteralPath (Join-Path $root "sessions/$($Role.ToLower())/session.json") -ErrorAction SilentlyContinue }
. "$PSScriptRoot\_common.ps1"
Invoke-CarabiaTests -Filter "FullyQualifiedName~Authentication.$($Role)SessionSetupTests" -Environment $Environment
