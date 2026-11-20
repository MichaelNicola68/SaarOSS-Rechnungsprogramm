$ErrorActionPreference="Stop"
$root=Split-Path -Parent $PSScriptRoot
$exe=Join-Path $root "src\SaarOSS.Rechnungsprogramm\bin\Release\net10.0-windows\SaarOSS.Rechnungsprogramm.exe"
if(-not(Test-Path $exe)){throw "Programm nicht gefunden. Zuerst Build-Release.ps1 ausführen."}
Start-Process $exe
