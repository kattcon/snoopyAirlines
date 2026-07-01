# Selenium Tests

This project contains end-to-end tests for the frontend using Selenium WebDriver.

## Run prerequisites

- Frontend running (default base URL: `http://localhost:8080`)
- Google Chrome installed
- Environment variable `RUN_SELENIUM_TESTS=true`

Optional:

- `E2E_BASE_URL` to target a different frontend URL

## Run commands

From repository root:

```powershell
dotnet test selenium.Tests\selenium.Tests.csproj
```

or run all tests including Selenium (when enabled):

```powershell
dotnet test snoopyAirlines.sln
```
