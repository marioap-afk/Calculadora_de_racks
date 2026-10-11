#Requires -Version 7.0
<#
.SYNOPSIS
  Prepara el clon de lectura del Architect para la sonda NATIVE_ARCHITECT del kit a2p2-v4 (sonda 2 del bloque A2-P2). NO EJECUTADO.

.DESCRIPTION
  Lo ejecuta la supervisión (plano a) solo con la disposición del Coordinator que fije este directorio (README §7, ítem D-3). Modelado sobre
  make-A2-clone.ps1 (S03), sin lo que el Architect no necesita: sin remoto github (ningún acceso de red desde la sonda), sin identidad de autor
  (no hay commits) y sin la comprobación de la orden O4 (el objeto revisado es el contrato de T1 en d30fb6a9, ancestro de la punta).

  Receta: git clone --no-local desde D:\r62-fixture\fixture-origin.git, rama fx/u1, en la punta esperada; core.autocrlf=false; remoto origin
  (el origen del fixture) y ningún otro.

  Se niega a correr, sin crear nada, si:
    - el directorio ya existe (código 10);
    - la punta de fx/u1 del origen no es -ExpectedTip (código 11);
    - main del origen no es -ExpectedMain (código 12);
    - existe un directorio de proyecto de Claude para el directorio (código 13);
    - el directorio es uno de los no admitidos por run_probe.sh (A, A2, B, B2, B3, R, A6, arch, arch02, evidence-out*, supervisor*, fx05*,
      probe-od4-1, *.git) (código 16).
  Tras clonar, una comprobación fallida deja el clon intacto (no borra nada) y termina con el código 20.

  Lecturas: el origen del fixture (git de lectura) y la existencia de rutas de %USERPROFILE%\.claude. No invoca codex, no lee config.toml ni nada
  bajo %USERPROFILE%\.codex. Escrituras: solo el clon nuevo y el informe (rutas del usuario como %USERPROFILE% y %LOCALAPPDATA%).

.EXAMPLE
  pwsh -NoProfile -File make-arch-clone.ps1 -ExpectedTip 95bdc29d428b35d2188a10e1c5462d6caf2be4df
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string]$ExpectedTip,
    [string]$OriginPath = 'D:\r62-fixture\fixture-origin.git',
    [string]$ClonePath = 'D:\r62-fixture\arch-a2p2v4',
    [ValidatePattern('^[0-9a-f]{40}$')][string]$ExpectedMain = 'fbe25347799e5b801ef708454212335537448bd1',
    [ValidatePattern('^[0-9a-f]{40}$')][string]$TargetCommit = 'd30fb6a9d26c0b5cb189a30b98ec97d0edcb7b99',
    [string]$ContractPath = 'docs/automation/decisions/FX-U1-T1.gate-contract.json',
    [ValidatePattern('^[0-9a-f]{40}$')][string]$TargetBlob = '628d89af5f21e4e14cbe7c3c14f0dd8fb5a490e0',
    [string]$ReportPath = (Join-Path $PSScriptRoot 'out\clone-arch-report.json')
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$Branch = 'fx/u1'

$report = [ordered]@{
    Schema     = 'a2p2-v4-arch-clone (preparación; supervisión, plano a)'
    StartedUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    Parameters = [ordered]@{ ExpectedTip = $ExpectedTip; ExpectedMain = $ExpectedMain; ClonePath = $ClonePath; OriginPath = $OriginPath
                             TargetCommit = $TargetCommit; ContractPath = $ContractPath; TargetBlob = $TargetBlob }
    PreChecks  = [ordered]@{}
    Clone      = $null
    Checks     = [ordered]@{}
    Result     = 'NOT_RUN'
    ExitCode   = $null
}

function Hide-UserPath([string]$s) {
    if ($null -eq $s) { return $s }
    if ($env:LOCALAPPDATA) { $s = $s.Replace($env:LOCALAPPDATA, '%LOCALAPPDATA%') }
    if ($env:USERPROFILE) { $s = $s.Replace($env:USERPROFILE, '%USERPROFILE%') }
    return $s
}

function Save-Report([int]$code, [string]$result) {
    $report.Result = $result
    $report.ExitCode = $code
    $report.FinishedUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    $dir = Split-Path -Parent $ReportPath
    if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
    Set-Content -LiteralPath $ReportPath -Value (Hide-UserPath ($report | ConvertTo-Json -Depth 8)) -Encoding utf8NoBOM
    Write-Host "$result (exit $code). Informe: $(Hide-UserPath $ReportPath)"
    exit $code
}

function Stop-Refusal([int]$code, [string]$why) { $report.PreChecks.Refusal = $why; Save-Report $code "REFUSED: $why" }

function Invoke-Git {
    $out = & git @args 2>$null
    if ($LASTEXITCODE -ne 0) { throw "git $($args -join ' ') -> exit $LASTEXITCODE" }
    return (@($out) -join "`n").Trim()
}

function Get-ClaudeProjectDir([string]$path) {
    $name = ($path.ToCharArray() | ForEach-Object { if ([char]::IsLetterOrDigit($_)) { $_ } else { '-' } }) -join ''
    return Join-Path $env:USERPROFILE (Join-Path '.claude\projects' $name)
}

$originUrl = $OriginPath -replace '\\', '/'
$claudeProject = Get-ClaudeProjectDir $ClonePath
$leaf = (Split-Path -Leaf $ClonePath).ToLowerInvariant()
$parent = (Split-Path -Parent $ClonePath).ToLowerInvariant().TrimEnd('\')

try {
    if ($parent -eq 'd:\r62-fixture' -and ($leaf -in @('a', 'a2', 'b', 'b2', 'b3', 'r', 'a6', 'arch', 'arch02', 'probe-od4-1') -or
            $leaf -like 'evidence-out*' -or $leaf -like 'supervisor*' -or $leaf -like 'fx05*') -or $leaf -like '*.git') {
        Stop-Refusal 16 "$ClonePath es un directorio no admitido para la sonda del Architect"
    }
    if (Test-Path -LiteralPath $ClonePath) { Stop-Refusal 10 "$ClonePath ya existe" }
    if (-not (Test-Path -LiteralPath $OriginPath)) { Stop-Refusal 11 "no existe el origen $OriginPath" }
    $tip = Invoke-Git -C $OriginPath rev-parse --verify "refs/heads/$Branch"
    $report.PreChecks.OriginTip = $tip
    if ($tip -ne $ExpectedTip) { Stop-Refusal 11 "la punta de $Branch del origen es $tip, no la esperada $ExpectedTip" }
    $main = Invoke-Git -C $OriginPath rev-parse --verify 'refs/heads/main'
    $report.PreChecks.OriginMain = $main
    if ($main -ne $ExpectedMain) { Stop-Refusal 12 "main del origen es $main, no $ExpectedMain" }
    $report.PreChecks.ClaudeProjectDirAbsent = -not (Test-Path -LiteralPath $claudeProject)
    if (Test-Path -LiteralPath $claudeProject) { Stop-Refusal 13 "existe un directorio de proyecto de Claude para $ClonePath" }
} catch {
    $report.PreChecks.Error = Hide-UserPath $_.Exception.Message
    Save-Report 11 'REFUSED: error en las comprobaciones previas'
}

$cloneLog = [ordered]@{}
try {
    & git -c core.autocrlf=false clone --no-local --branch $Branch --config core.autocrlf=false $originUrl $ClonePath 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "git clone -> exit $LASTEXITCODE" }
    $cloneLog.Created = $true
    $cloneLog.CreatedUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
} catch {
    $cloneLog.Created = Test-Path -LiteralPath $ClonePath
    $cloneLog.Error = Hide-UserPath $_.Exception.Message
    $report.Clone = $cloneLog
    Save-Report 20 'FAIL: el clon no se completó (no se borra nada; revisión manual)'
}
$report.Clone = $cloneLog

$fails = 0
function Set-Check([string]$id, $obj) { $report.Checks[$id] = $obj; if ($obj.Result -eq 'fail') { $script:fails++ } }
try {
    $head = Invoke-Git -C $ClonePath rev-parse HEAD
    $cur = Invoke-Git -C $ClonePath symbolic-ref --short HEAD
    $porcelain = Invoke-Git -C $ClonePath status --porcelain
    $ignored = Invoke-Git -C $ClonePath status --porcelain --ignored
    $autocrlf = Invoke-Git -C $ClonePath config --local --get core.autocrlf
    $remotes = @((Invoke-Git -C $ClonePath remote) -split "`n" | Where-Object { $_ })
    $originRemote = Invoke-Git -C $ClonePath config --get remote.origin.url
    $lsr = ((Invoke-Git -C $ClonePath ls-remote origin "refs/heads/$Branch") -split '\s+')[0]
    $ok = ($head -eq $ExpectedTip) -and ($cur -eq $Branch) -and ($porcelain -eq '') -and ($ignored -eq '') -and ($autocrlf -eq 'false') -and
          (($remotes -join ',') -eq 'origin') -and ($originRemote -eq $originUrl) -and ($lsr -eq $head)
    Set-Check 'clon' ([ordered]@{ Head = $head; Branch = $cur; StatusPorcelainEmpty = ($porcelain -eq ''); StatusIgnoredEmpty = ($ignored -eq '')
        CoreAutocrlf = $autocrlf; Remotes = $remotes; Origin = $originRemote; LsRemoteBranch = $lsr; Result = $(if ($ok) { 'pass' } else { 'fail' }) })

    $blob = Invoke-Git -C $ClonePath rev-parse "${TargetCommit}:$ContractPath"
    & git -C $ClonePath merge-base --is-ancestor $TargetCommit HEAD 2>$null
    $anc = ($LASTEXITCODE -eq 0)
    $originMain = Invoke-Git -C $ClonePath rev-parse refs/remotes/origin/main
    Set-Check 'objeto revisado' ([ordered]@{ TargetCommit = $TargetCommit; Blob = $blob; ExpectedBlob = $TargetBlob; AncestorOfHead = $anc
        OriginMain = $originMain; Result = $(if ($blob -eq $TargetBlob -and $anc -and $originMain -eq $ExpectedMain) { 'pass' } else { 'fail' }) })

    $absent = -not (Test-Path -LiteralPath $claudeProject)
    Set-Check 'sin rastros' ([ordered]@{ ClaudeProjectDir = (Hide-UserPath $claudeProject); Absent = $absent; Result = $(if ($absent) { 'pass' } else { 'fail' }) })
} catch {
    $report.Checks.Error = Hide-UserPath $_.Exception.Message
    $fails++
}
if ($fails -gt 0) { Save-Report 20 "FAIL: $fails comprobación(es) fallida(s); el clon queda intacto para revisión manual" }
Save-Report 0 'PASS'
