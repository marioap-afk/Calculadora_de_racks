# worker-call-relative.ps1 - reproduce la CLASE de la llamada 25 de F4-T4 (I-65): Set-Location al worktree
# y despues API .NET con ruta relativa. Se ejecuta SIEMPRE como proceso hijo, sobre un fixture desechable.
# Modo EditSln: lee y reescribe "RackCad.sln" con [IO.File] (como la llamada 25: WriteAllText quita el BOM).
# Modo CreateNew: crea "notes-relative.txt" con [IO.File] (variante para el directorio trampa vacio).
# Modo CmdletEdit: igual que EditSln pero con Get-Content/Set-Content (resuelven contra la ubicacion de PowerShell).
param(
    [Parameter(Mandatory)][string]$TargetDir,
    [Parameter(Mandatory)][string]$ExpectedCwd,
    [ValidateSet('EditSln', 'CreateNew', 'CmdletEdit')][string]$Mode = 'EditSln'
)
$ErrorActionPreference = 'Stop'
function Norm([string]$p) { return [IO.Path]::GetFullPath($p).TrimEnd('\') }
# C2: primer hecho de la orden = directorio de proceso (.NET), antes de cualquier otra cosa
$first = [ordered]@{
    Fact        = 'C2-FirstFact'
    ProcessCwd  = [Environment]::CurrentDirectory
    PSLocation  = (Get-Location).ProviderPath
    ExpectedCwd = $ExpectedCwd
}
$first.C2 = if ((Norm $first.ProcessCwd) -ieq (Norm $ExpectedCwd)) { 'PASS' } else { 'STOP_C2_CWD_MISMATCH' }

Set-Location -LiteralPath $TargetDir
$after = [ordered]@{ ProcessCwd = [Environment]::CurrentDirectory; PSLocation = (Get-Location).ProviderPath }

$out = [ordered]@{ FirstFact = $first; AfterSetLocation = $after; Mode = $Mode; Error = $null }
try {
    switch ($Mode) {
        'EditSln' {
            $p = 'RackCad.sln'
            $out.ResolvedByDotNet = [IO.Path]::GetFullPath($p)
            $out.ResolvedByPowerShell = Join-Path (Get-Location).ProviderPath $p
            $t = [IO.File]::ReadAllText($p)
            [IO.File]::WriteAllText($p, $t.Replace('EndGlobal', "`tAddedByWorker = 1`r`nEndGlobal"))
            $out.Wrote = [IO.Path]::GetFullPath($p)
        }
        'CreateNew' {
            $p = 'notes-relative.txt'
            $out.ResolvedByDotNet = [IO.Path]::GetFullPath($p)
            $out.ResolvedByPowerShell = Join-Path (Get-Location).ProviderPath $p
            [IO.File]::WriteAllText($p, "borrador del worker`r`n")
            $out.Wrote = [IO.Path]::GetFullPath($p)
        }
        'CmdletEdit' {
            $p = 'RackCad.sln'
            $out.ResolvedByDotNet = [IO.Path]::GetFullPath($p)
            $out.ResolvedByPowerShell = (Resolve-Path -LiteralPath $p).ProviderPath
            $t = Get-Content -LiteralPath $p -Raw
            Set-Content -LiteralPath $p -Value ($t.Replace('EndGlobal', "`tAddedByWorker = 1`r`nEndGlobal")) -NoNewline
            $out.Wrote = $out.ResolvedByPowerShell
        }
    }
} catch {
    $out.Error = $_.Exception.GetType().Name + ': ' + $_.Exception.Message
}
$out | ConvertTo-Json -Depth 5 -Compress
