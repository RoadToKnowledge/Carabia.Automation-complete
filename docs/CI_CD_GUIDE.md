# CI/CD guide

## What runs in GitHub Actions (`.github/workflows/selenium-tests.yml`)

| Job | Runs | Notes |
|---|---|---|
| build | restore, build, `dotnet test --list-tests` | proves compilation and NUnit discovery |
| public-tests | `(Public or Smoke) and not RequiresManualAuthentication and not WritesData and not Destructive`, headless Chrome | `CI=true` makes any authenticated fixture fail fast instead of waiting |

Results (`reports/`, `artifacts/`) are uploaded as the `carabia-automation-results` artifact on every run.
`workflow_dispatch` lets you pick `Dev` or `Prod` (Prod is read-only: SafetyGuard turns all writes off).

The runner must reach the environment URL. If DEV is IP-restricted, use a self-hosted runner inside the allowed
network (Windows: change `runs-on` and use `pwsh` scripts; Chrome/Edge must be installed).

## Authenticated suites in CI

Not enabled by default, because login needs a person (OTP / admin password). If you decide to support it later:

1. Create the session locally: `scripts/create-session.ps1 -Role Seller`.
2. Store the **content** of `sessions/seller/session.json` as an encrypted repository/environment secret.
3. In the workflow, write the secret back to `sessions/seller/session.json` before `dotnet test`, run with
   `--filter TestCategory=Seller`, and delete the file in an `always()` step.
4. Sessions expire (30-day sliding cookie); a CI run with an expired session fails fast with
   "MANUAL AUTHENTICATION REQUIRED FOR: SELLER".

Never commit session files, cookies or tokens. Never enable write switches in CI against shared environments.

## Azure DevOps / other CI

Same commands: `dotnet build`, then `dotnet test --settings carabia.runsettings --filter "<filter>" --logger trx
-- NUnit.TestOutputXml=<abs path> NUnit.OutputXmlFolderMode=AbsoluteFolder`. Set `TF_BUILD=true` (Azure sets it)
so authenticated fixtures do not wait for a login.
