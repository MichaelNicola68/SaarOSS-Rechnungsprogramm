$ErrorActionPreference="Stop"
$root=Split-Path -Parent $PSScriptRoot
dotnet build "$root\SaarOSS-Rechnungsprogramm.sln" -c Release
Write-Host "Release-Build erfolgreich." -ForegroundColor Green
