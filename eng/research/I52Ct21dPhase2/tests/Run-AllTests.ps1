# Runs all phase 2 tests WITHOUT AutoCAD:  pwsh -NoProfile -File .\tests\Run-AllTests.ps1  [-Only functional|static]
# Exit 0 = all passed, 1 = at least one failure. Prints the counts. Creates and removes its own temp folder under the user temp dir.
# (The tests start one benign stand-in child process, a renamed copy of ping.exe, and stop it again; the production scripts never do.)

param([ValidateSet('all', 'functional', 'static')][string]$Only = 'all')

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path -Path $PSScriptRoot -ChildPath 'TestSupport.ps1')
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    if ($Only -in 'all', 'static') { . (Join-Path -Path $PSScriptRoot -ChildPath 'StaticScan.Tests.ps1') }
    if ($Only -in 'all', 'functional') {
        . (Join-Path -Path $PSScriptRoot -ChildPath 'Phase2.Tests.ps1')
        . (Join-Path -Path $PSScriptRoot -ChildPath 'Phase2.Remediation.Tests.ps1')
        . (Join-Path -Path $PSScriptRoot -ChildPath 'Phase2.Round2.Tests.ps1')
    }
} catch {
    $script:TestState.Fail++
    $script:TestState.Failures.Add('UNHANDLED :: ' + $_.Exception.Message + ' @ ' + $_.ScriptStackTrace)
    Write-Host ('UNHANDLED ' + $_.Exception.Message)
    Write-Host $_.ScriptStackTrace
} finally {
    Remove-TestTree
}
$sw.Stop()
Write-Host ''
Write-Host ('TESTS passed=' + $script:TestState.Pass + ' failed=' + $script:TestState.Fail + ' total=' + ($script:TestState.Pass + $script:TestState.Fail) + ' seconds=' + [int]$sw.Elapsed.TotalSeconds)
foreach ($f in $script:TestState.Failures) { Write-Host ('  FAILED: ' + $f) }
if ($script:TestState.Fail -gt 0) { exit 1 }
exit 0
