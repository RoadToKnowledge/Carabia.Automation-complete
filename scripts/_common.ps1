# Shared runner used by all run-*.ps1 scripts. Not meant to be called directly.
# Builds the solution once and runs dotnet test with an NUnit category filter, writing:
#   reports/nunit/<runId>/   NUnit 3 XML result (NUnit adapter TestOutputXml)
#   reports/trx/<runId>.trx  VS test result
#   reports/html/<runId>/report.html and reports/html/latest.html  (framework HTML report)
#   artifacts/<runId>/       failure screenshots, URL, console log, stack trace
function Invoke-CarabiaTests {
    param(
        [Parameter(Mandatory = $true)][string]$Filter,
        [string]$Environment = "Dev",
        [ValidateSet("Chrome", "Edge")][string]$Browser = "Chrome",
        [switch]$Headless,
        [int]$Workers = 1,
        [string]$Configuration = "Debug",
        [switch]$NoBuild
    )

    $root = Split-Path -Parent $PSScriptRoot
    Push-Location $root
    try {
        $runId = Get-Date -Format "yyyyMMdd-HHmmss"
        $env:CARABIA_RUN_ID = $runId
        $env:CARABIA_AUTOMATION_ENV = $Environment
        $env:CARABIA_AUTOMATION__Execution__Browser = $Browser
        if ($Headless) { $env:CARABIA_AUTOMATION__Execution__Headless = "true" }
        else { Remove-Item Env:\CARABIA_AUTOMATION__Execution__Headless -ErrorAction SilentlyContinue }

        $nunitDir = Join-Path $root "reports/nunit/$runId"
        $trxDir = Join-Path $root "reports/trx"
        New-Item -ItemType Directory -Force -Path $nunitDir, $trxDir | Out-Null

        Write-Host "Run $runId | env=$Environment | browser=$Browser | headless=$($Headless.IsPresent) | filter: $Filter" -ForegroundColor Cyan

        if (-not $NoBuild) {
            dotnet build .\Carabia.Automation.sln --configuration $Configuration
            if ($LASTEXITCODE -ne 0) { throw "Build failed." }
        }

        dotnet test .\Carabia.Automation.sln --configuration $Configuration --no-build `
            --settings .\carabia.runsettings `
            --filter $Filter `
            --logger "trx;LogFileName=$runId.trx" --results-directory $trxDir `
            --logger "console;verbosity=normal" `
            -- NUnit.TestOutputXml=$nunitDir NUnit.OutputXmlFolderMode=AbsoluteFolder NUnit.NumberOfTestWorkers=$Workers

        $exit = $LASTEXITCODE
        Write-Host ""
        Write-Host "HTML report : $(Join-Path $root "reports/html/$runId/report.html")" -ForegroundColor Green
        Write-Host "NUnit XML   : $nunitDir" -ForegroundColor Green
        Write-Host "Artifacts   : $(Join-Path $root "artifacts/$runId")" -ForegroundColor Green
        exit $exit
    }
    finally {
        Pop-Location
    }
}
