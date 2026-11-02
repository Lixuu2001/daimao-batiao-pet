$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
.\build.ps1
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path $compiler)) { $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
& $compiler /nologo /target:exe /main:WindowTests /r:System.Drawing.dll /r:System.Windows.Forms.dll /resource:assets\spritesheet.png,Batiao.Sprite /out:dist\WindowTests.exe src\Animation.cs src\LayeredWindow.cs src\Program.cs tests\WindowTests.cs
if ($LASTEXITCODE -ne 0) { throw 'Window tests compilation failed.' }
& .\dist\WindowTests.exe
if ($LASTEXITCODE -ne 0) { throw 'Window tests failed.' }
