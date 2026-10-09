#Requires -Version 7.0
<#
.SYNOPSIS
  S03 de FX-02 (kit FX-02, README §2, fila S03): crea el clon limpio D:\r62-fixture\A2 y ejecuta las comprobaciones automáticas, de solo lectura,
  de la parte 1 de launch-card-A2.md (#1 en parte, #2, #4, #5, #6 en parte, #8 y #13).

.DESCRIPTION
  Preparación de la supervisión (plano a; clasificación de FX-02, §3 fila P5). Solo se ejecuta tras S02: la orden FX-U1-O4 publicada en fx/u1 de
  origin y de github (decisiones §65, punto 7: O4 → clon A2 → sonda A4-1 → preflight/P-07 → apertura de A2). Por decisiones §65, punto 11, el clon no
  depende de la huella de codex-cli: #8 solo registra el binario y la versión de la app; la huella de config.toml la comprueba la medición de la
  sonda A4-1 antes y después de su única invocación.

  Receta (README S03 y tarjeta #4): git clone --no-local desde D:\r62-fixture\fixture-origin.git, rama fx/u1, en la punta vigente tras S02;
  core.autocrlf=false; autor y committer `fixture <fixture@example.invalid>` locales al clon; remotos origin (el origen del fixture) y github
  (repositorio público del fixture), y ningún otro.

  Se niega a correr, sin crear nada, si:
    - D:\r62-fixture\A2 ya existe (código 10);
    - la punta de fx/u1 del origen no es -ExpectedTip (código 11);
    - main del origen no es -ExpectedMain (código 12: sería T22, no T16; lo decide una persona);
    - existe un directorio de proyecto de Claude para D:\r62-fixture\A2 (código 13: rastro de una sesión previa, tarjeta #5);
    - la punta no contiene la orden FX-U1-O4 en docs/automation/decisions/FX-U1.md, o su bloque conserva un marcador sin resolver (código 14);
    - fx/u1 de github no es -ExpectedTip (código 15; se omite con -SkipGithubLsRemote, y entonces #1 queda MANUAL para github).
  Tras clonar, una comprobación fallida deja el clon intacto (no borra nada) y termina con el código 20.

  Lecturas: el origen del fixture (git de lectura), github (git ls-remote de lectura, salvo -SkipGithubLsRemote), la existencia de rutas de
  %USERPROFILE%\.claude, el SHA-256 del binario de Codex y la versión de la app (config.toml no se lee: decisiones §65, punto 11). Escrituras: solo
  el clon nuevo y el informe. Nunca
  escribe en el origen ni en github. El informe no contiene el nombre del usuario de Windows: las rutas se registran como %USERPROFILE% y
  %LOCALAPPDATA%.

.EXAMPLE
  pwsh -NoProfile -File make-A2-clone.ps1 -ExpectedTip <sha de la orden FX-U1-O4 en fx/u1>
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string]$ExpectedTip,
    [string]$OriginPath = 'D:\r62-fixture\fixture-origin.git',
    [string]$ClonePath = 'D:\r62-fixture\A2',
    [string]$GithubUrl = 'https://github.com/marioap-afk/rackcad-i62-fixture.git',
    [ValidatePattern('^[0-9a-f]{40}$')][string]$ExpectedMain = 'fbe25347799e5b801ef708454212335537448bd1',
    [string]$ReportPath = (Join-Path $PSScriptRoot 'out\clone-A2-report.json'),
    [switch]$SkipGithubLsRemote
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Binario y app del trío aceptado (OD-2e = A, decisiones §59; huella sustituida por OD-2f = A, decisiones §65). Solo se registran (#8 informativo);
# no se invoca codex para comprobarlos (ninguna invocación de codex-cli fuera de una autorización).
$ExpectBin = '3553cd6e7df5a093d8cb8301cd8088a57e0971aba71ddbe0e67f7f44a15cdf68'
$ExpectBinLabel = '9691020b546a15b2'
$ExpectApp = '26.1002.7124.0'
$ExpectCli = 'codex-cli 0.162.0-alpha.2'
$FixtureName = 'fixture'
$FixtureEmail = 'fixture@example.invalid'
$Branch = 'fx/u1'

$report = [ordered]@{
    Schema        = 'fx02-prelaunch-A2-clone (staging; supervisión, plano a)'
    StartedUtc    = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
    Parameters    = [ordered]@{ ExpectedTip = $ExpectedTip; ExpectedMain = $ExpectedMain; ClonePath = $ClonePath; OriginPath = $OriginPath; GithubUrl = $GithubUrl }
    PreChecks     = [ordered]@{}
    Clone         = $null
    Checks        = [ordered]@{}
    ManualChecks  = @('#0 registro de CD-26 resuelta por hecho', '#1 corrida de CI de la orden', '#3 P-07 con la disposición de CD-08 (U-14)',
                      '#6 salida del gancho, plugins habilitados y contexto que inyecta la app', '#7 área transitoria según CD-02 (U-08)',
                      '#8 huella de config.toml y sonda medida de A4-1 (F-A4-PROBE), con los invalidadores que no se observan aquí (autenticación, modelo, effort, blobs de catálogo y routing, instancia del host)',
                      '#9-#12 listas A y B, archivos sellados y oráculos', '#10 modo de permisos (después de S04)')
    Result        = 'NOT_RUN'
    ExitCode      = $null
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
    $json = Hide-UserPath ($report | ConvertTo-Json -Depth 8)
    Set-Content -LiteralPath $ReportPath -Value $json -Encoding utf8NoBOM
    Write-Host "$result (exit $code). Informe: $(Hide-UserPath $ReportPath)"
    exit $code
}

function Stop-Refusal([int]$code, [string]$why) {
    $report.PreChecks.Refusal = $why
    Save-Report $code "REFUSED: $why"
}

function Invoke-Git {
    # Función simple (sin param): los argumentos, incluido -C, pasan tal cual a git por $args.
    $out = & git @args 2>$null
    if ($LASTEXITCODE -ne 0) { throw "git $($args -join ' ') -> exit $LASTEXITCODE" }
    return (@($out) -join "`n").Trim()
}

function Get-ClaudeProjectDir([string]$path) {
    # Nombre del directorio de proyecto de Claude: cada carácter no alfanumérico pasa a '-' (D:\r62-fixture\A2 -> D--r62-fixture-A2).
    $name = ($path.ToCharArray() | ForEach-Object { if ([char]::IsLetterOrDigit($_)) { $_ } else { '-' } }) -join ''
    return Join-Path $env:USERPROFILE (Join-Path '.claude\projects' $name)
}

function Get-FileFact([string]$path, [string]$gitRepo, [string]$relPath) {
    $fact = [ordered]@{ Path = (Hide-UserPath $path); Exists = (Test-Path -LiteralPath $path -PathType Leaf) }
    if ($fact.Exists) {
        $fact.Sha256 = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
        if ($gitRepo) { $fact.GitBlobAtHead = (Invoke-Git -C $gitRepo rev-parse "HEAD:$relPath") }
    }
    return $fact
}

$originUrl = $OriginPath -replace '\\', '/'
$claudeProject = Get-ClaudeProjectDir $ClonePath

# ------------------------------------------------------------------ pre-checks (solo lectura; nada se crea si alguno falla)
try {
    if (Test-Path -LiteralPath $ClonePath) { Stop-Refusal 10 "$ClonePath ya existe" }
    if (-not (Test-Path -LiteralPath $OriginPath)) { Stop-Refusal 11 "no existe el origen $OriginPath" }

    $tip = Invoke-Git -C $OriginPath rev-parse --verify "refs/heads/$Branch"
    $report.PreChecks.OriginTip = $tip
    if ($tip -ne $ExpectedTip) { Stop-Refusal 11 "la punta de $Branch del origen es $tip, no la esperada $ExpectedTip" }

    $main = Invoke-Git -C $OriginPath rev-parse --verify 'refs/heads/main'
    $report.Checks.'#2 main' = [ordered]@{ Observed = $main; Expected = $ExpectedMain; Result = $(if ($main -eq $ExpectedMain) { 'pass' } else { 'fail' }) }
    if ($main -ne $ExpectedMain) { Stop-Refusal 12 "main del origen es $main, no $ExpectedMain (T22, no T16)" }

    $report.PreChecks.ClaudeProjectDirAbsent = -not (Test-Path -LiteralPath $claudeProject)
    if (Test-Path -LiteralPath $claudeProject) { Stop-Refusal 13 "existe un directorio de proyecto de Claude para $ClonePath" }

    # #1 en parte: la punta contiene la orden FX-U1-O4 publicada y sin marcadores sin resolver dentro de su bloque.
    $decisions = & git -C $OriginPath show "${tip}:docs/automation/decisions/FX-U1.md" 2>$null
    if ($LASTEXITCODE -ne 0) { Stop-Refusal 14 'la punta no contiene docs/automation/decisions/FX-U1.md' }
    $decText = (@($decisions) -join "`n")
    $i0 = $decText.IndexOf('<<<BEGIN FX-U1-O4>>>'); $i1 = $decText.IndexOf('<<<END FX-U1-O4>>>')
    $hasOrder = $decText.Contains('FIXTURE-ORDER: FX-U1-O4')
    $orderBlock = if ($i0 -ge 0 -and $i1 -gt $i0) { $decText.Substring($i0, $i1 - $i0) } else { $decText }
    $unresolved = @([regex]::Matches($orderBlock, '\{[A-Z_]+\}|<PENDIENTE') | ForEach-Object { $_.Value } | Select-Object -Unique)
    $changedAtTip = Invoke-Git -C $OriginPath diff-tree --no-commit-id --name-only -r $tip
    $report.Checks.'#1 orden en la punta' = [ordered]@{
        FixtureOrderPresent = $hasOrder; UnresolvedMarkers = $unresolved; PathsChangedByTip = @($changedAtTip -split "`n")
        Result = $(if ($hasOrder -and $unresolved.Count -eq 0) { 'pass' } else { 'fail' }); Note = 'la corrida de CI de la orden se comprueba aparte (MANUAL)'
    }
    if (-not $hasOrder -or $unresolved.Count -gt 0) { Stop-Refusal 14 'la orden FX-U1-O4 no está en la punta o conserva marcadores sin resolver' }

    if (-not $SkipGithubLsRemote) {
        $gh = Invoke-Git ls-remote $GithubUrl "refs/heads/$Branch"
        $ghSha = ($gh -split '\s+')[0]
        $report.Checks.'#1 github' = [ordered]@{ Observed = $ghSha; Expected = $ExpectedTip; Result = $(if ($ghSha -eq $ExpectedTip) { 'pass' } else { 'fail' }) }
        if ($ghSha -ne $ExpectedTip) { Stop-Refusal 15 "fx/u1 de github es $ghSha, no $ExpectedTip" }
    } else {
        $report.Checks.'#1 github' = [ordered]@{ Result = 'MANUAL'; Note = 'omitido con -SkipGithubLsRemote' }
    }
} catch {
    $report.PreChecks.Error = Hide-UserPath $_.Exception.Message
    Save-Report 11 'REFUSED: error en las comprobaciones previas'
}

# ------------------------------------------------------------------ S03: clon
$cloneLog = [ordered]@{}
try {
    & git -c core.autocrlf=false clone --no-local --branch $Branch --config core.autocrlf=false $originUrl $ClonePath 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "git clone -> exit $LASTEXITCODE" }
    Invoke-Git -C $ClonePath config --local user.name $FixtureName | Out-Null
    Invoke-Git -C $ClonePath config --local user.email $FixtureEmail | Out-Null
    Invoke-Git -C $ClonePath remote add github $GithubUrl | Out-Null
    $cloneLog.Created = $true
    $cloneLog.CreatedUtc = (Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ')
} catch {
    $cloneLog.Created = Test-Path -LiteralPath $ClonePath
    $cloneLog.Error = Hide-UserPath $_.Exception.Message
    $report.Clone = $cloneLog
    Save-Report 20 'FAIL: el clon no se completó (no se borra nada; revisión manual)'
}
$report.Clone = $cloneLog

# ------------------------------------------------------------------ comprobaciones posteriores (solo lectura)
$fails = 0
function Set-Check([string]$id, $obj) { $report.Checks[$id] = $obj; if ($obj.Result -eq 'fail') { $script:fails++ } }

try {
    # #4 clon
    $head = Invoke-Git -C $ClonePath rev-parse HEAD
    $cur = Invoke-Git -C $ClonePath symbolic-ref --short HEAD
    $porcelain = Invoke-Git -C $ClonePath status --porcelain
    $ignored = Invoke-Git -C $ClonePath status --porcelain --ignored
    $autocrlf = Invoke-Git -C $ClonePath config --local --get core.autocrlf
    $uname = Invoke-Git -C $ClonePath config --local --get user.name
    $umail = Invoke-Git -C $ClonePath config --local --get user.email
    $remotes = @((Invoke-Git -C $ClonePath remote) -split "`n" | Where-Object { $_ } | Sort-Object)
    $originRemote = Invoke-Git -C $ClonePath config --get remote.origin.url
    $githubRemote = Invoke-Git -C $ClonePath config --get remote.github.url
    $ok4 = ($head -eq $ExpectedTip) -and ($cur -eq $Branch) -and ($porcelain -eq '') -and ($autocrlf -eq 'false') -and
           ($uname -eq $FixtureName) -and ($umail -eq $FixtureEmail) -and (($remotes -join ',') -eq 'github,origin') -and
           ($originRemote -eq $originUrl) -and ($githubRemote -eq $GithubUrl)
    Set-Check '#4 clon' ([ordered]@{
        Head = $head; Branch = $cur; StatusPorcelainEmpty = ($porcelain -eq ''); StatusIgnoredLines = @($ignored -split "`n" | Where-Object { $_ }).Count
        CoreAutocrlf = $autocrlf; UserName = $uname; UserEmail = $umail; Remotes = $remotes; Origin = $originRemote; Github = $githubRemote
        Result = $(if ($ok4) { 'pass' } else { 'fail' })
    })

    # #5 sin rastros de sesiones previas
    $absent = -not (Test-Path -LiteralPath $claudeProject)
    Set-Check '#5 sin rastros' ([ordered]@{ ClaudeProjectDir = (Hide-UserPath $claudeProject); Absent = $absent; Result = $(if ($absent) { 'pass' } else { 'fail' })
        Note = 'si la sonda de A4-1 corre después con -C en este clon, #4 y #5 se repiten tras ella; su registro de sesión de Codex va a LB-1' })

    # #6 entradas automáticas: solo ruta y SHA-256 (la salida del gancho, los plugins y el contexto de la app, MANUAL)
    Set-Check '#6 entradas automáticas (hashes)' ([ordered]@{
        GlobalClaudeMd       = Get-FileFact (Join-Path $env:USERPROFILE '.claude\CLAUDE.md') $null $null
        GlobalSettings       = Get-FileFact (Join-Path $env:USERPROFILE '.claude\settings.json') $null $null
        GlobalSettingsLocal  = Get-FileFact (Join-Path $env:USERPROFILE '.claude\settings.local.json') $null $null
        CloneAgentsMd        = Get-FileFact (Join-Path $ClonePath 'AGENTS.md') $ClonePath 'AGENTS.md'
        CloneClaudeMd        = Get-FileFact (Join-Path $ClonePath 'CLAUDE.md') $ClonePath 'CLAUDE.md'
        CloneClaudeSettings  = Get-FileFact (Join-Path $ClonePath '.claude\settings.json') $ClonePath '.claude/settings.json'
        Result               = 'recorded'
        Note                 = 'el contenido no se lee aquí; la comprobación de que ninguna entrada trae hechos de FX-U1, del oráculo o de RackCad es MANUAL'
    })

    # #8 binario y app de codex-cli: informativo (decisiones §65, punto 11: el clon no depende de la huella; config.toml no se lee aquí)
    $bins = @(Get-ChildItem -Path (Join-Path $env:LOCALAPPDATA 'OpenAI\Codex\bin\*\codex.exe') -ErrorAction SilentlyContinue)
    $binPath = Join-Path $env:LOCALAPPDATA "OpenAI\Codex\bin\$ExpectBinLabel\codex.exe"
    $binHash = if (Test-Path -LiteralPath $binPath) { (Get-FileHash -LiteralPath $binPath -Algorithm SHA256).Hash.ToLowerInvariant() } else { $null }
    $app = (& powershell.exe -NoProfile -Command '(Get-AppxPackage *OpenAI.Codex*).Version' 2>$null | Select-Object -First 1)
    $app = if ($app) { $app.Trim() } else { $null }
    Set-Check '#8 binario y app de codex-cli (informativo)' ([ordered]@{
        CodexExeCount = $bins.Count; BinaryPath = (Hide-UserPath $binPath); BinarySha256 = $binHash; AppVersion = $app; ExpectedCli = $ExpectCli
        MatchesAcceptedTrio = (($bins.Count -eq 1) -and ($binHash -eq $ExpectBin) -and ($app -eq $ExpectApp))
        Result = 'recorded'
        Note = 'la huella de config.toml y el trío completo los comprueba la medición de la sonda A4-1 antes y después de su invocación; una diferencia es P-01'
    })

    # #13 directorio -C del Controller
    Set-Check '#13 -C del Controller' ([ordered]@{ WorkingDirectory = $ClonePath; Basis = 'decisiones §55, U-03'; Result = $(if ($ClonePath -eq 'D:\r62-fixture\A2') { 'pass' } else { 'fail' }) })
} catch {
    $report.Checks.Error = Hide-UserPath $_.Exception.Message
    $fails++
}

if ($fails -gt 0) { Save-Report 20 "FAIL: $fails comprobación(es) fallida(s); el clon queda intacto para revisión manual" }
Save-Report 0 'PASS (comprobaciones automáticas); las MANUAL siguen pendientes'
