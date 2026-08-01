$cscPaths = @(
    "$env:windir\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "$env:windir\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)
$csc = $null
foreach ($p in $cscPaths) { if (Test-Path $p) { $csc = $p; break } }

if (-not $csc) {
    Write-Host "ERROR: C# compiler (csc.exe) not found."
    Write-Host "Install .NET Framework 4.8 SDK or Visual Studio."
    exit 1
}

$src = @("FluentCalculator.cs", "Program.cs")
$refs = @("System.dll", "System.Data.dll", "System.Drawing.dll", "System.Windows.Forms.dll")
$cscArgs = New-Object System.Collections.Generic.List[string]
$cscArgs.Add("-target:winexe")
$cscArgs.Add("-win32icon:icon.ico")
$cscArgs.Add("-win32manifest:app.manifest")
$cscArgs.Add("-out:Calculator.exe")
foreach ($r in $refs) { $cscArgs.Add("-reference:$r") }
foreach ($s in $src) { $cscArgs.Add($s) }

Write-Host "Compiling..."
$proc = Start-Process -FilePath $csc -ArgumentList $cscArgs -NoNewWindow -Wait -PassThru

if ($proc.ExitCode -eq 0) {
    Write-Host "DONE: Calculator.exe created."
    Write-Host "Launching..."
    Start-Process (Join-Path (Get-Location) "Calculator.exe")
} else {
    Write-Host "FAILED (exit code $($proc.ExitCode))."
    Write-Host "Try building manually:"
    Write-Host "& `"$csc`" -target:winexe -win32icon:icon.ico -win32manifest:app.manifest -out:Calculator.exe -reference:System.Data.dll FluentCalculator.cs Program.cs"
}
