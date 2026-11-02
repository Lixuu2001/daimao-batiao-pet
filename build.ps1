$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
if (!(Test-Path $compiler)) { throw 'Install .NET Framework 4.8 before building.' }
New-Item -ItemType Directory -Force dist | Out-Null
& $compiler /nologo /target:exe /r:System.Drawing.dll /out:dist\Tests.exe src\Animation.cs tests\Tests.cs
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
& .\dist\Tests.exe
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
& $compiler /nologo /target:winexe /optimize+ /r:System.Drawing.dll /r:System.Windows.Forms.dll /resource:assets\spritesheet.png,Batiao.Sprite /out:dist\DaimaoBatiao.exe src\Animation.cs src\LayeredWindow.cs src\Program.cs
if ($LASTEXITCODE -ne 0) { throw 'App compilation failed.' }
Write-Host 'Built dist\DaimaoBatiao.exe'
