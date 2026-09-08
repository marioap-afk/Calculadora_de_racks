#Requires -Version 7.0

<#
.SYNOPSIS
    Mide, de forma reproducible, lo que cuesta validar un cambio en este repositorio.

.DESCRIPTION
    I-45 gate G1. Este script NO optimiza nada, NO cambia la poblacion de pruebas y NO
    reduce ninguna obligacion: solo mide, con procedencia automatica, lo que hoy ya se
    hace. El metodo esta descrito en docs/initiatives/I-45-measurement-method.md; este
    archivo es su implementacion ejecutable.

    Todo resultado se escribe bajo artifacts/ (ignorado por git). Ningun numero producido
    aqui se pega a mano en un documento normativo.

.PARAMETER Measure
    Que medir. Combinable. 'all' equivale a core, ui, build-ui, build-plugin, cycle y ordinary.
    'none' no mide nada (util para pedir solo -Sessions, -Ci o -SelfTest).

.PARAMETER Repeat
    Repeticiones solicitadas. Alias -N. Cada medida tiene un MINIMO documentado
    (core/ui = 3, builds y ciclo = 2). Si se pide menos, el script SUBE hasta el minimo
    y lo declara en el JSON (repeatRequested / repeatEffective): una linea base por
    debajo del minimo no se puede producir por accidente ni en silencio.

.PARAMETER Sessions
    Reconstruye el calendario de una iniciativa a partir de un commit de merge: rango
    <merge>^1..<merge>^2, --no-merges, marca de tiempo de AUTOR.

.PARAMETER Ci
    Mide una corrida de CI ('latest' o un id). Usa gh si esta; si no, se reporta la
    indisponibilidad y el procedimiento manual queda en el documento del metodo.

.PARAMETER SelfTest
    Verifica las guardas de fallo-cerrado sin tocar el repositorio, sin filtrar una
    suite real y sin modificar ninguna prueba.

.EXAMPLE
    pwsh -File eng/validation/measure-validation.ps1 -Measure core,ui -N 3

.EXAMPLE
    pwsh -File eng/validation/measure-validation.ps1 -Measure none -Sessions bd40ef7
#>

[CmdletBinding()]
param(
    [ValidateSet('all', 'none', 'core', 'ui', 'build-ui', 'build-plugin', 'cycle', 'ordinary')]
    [string[]]$Measure = @('none'),

    [Alias('N')]
    [ValidateRange(1, 50)]
    [int]$Repeat = 3,

    [string]$Sessions,

    [double[]]$Thresholds = @(0.5, 1, 2, 3, 6, 12),

    [string]$Ci,

    [switch]$SelfTest,

    [string]$OutputPath,

    [string]$DotnetPath,

    [switch]$NoBuild
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$SchemaName = 'rackcad.validation-measurement'
# v2 (G5): la forma cambio — cada metrica de secuencia publica ahora `steps`, y existe la clave nueva
# OrdinaryIterationLocalSeconds. La regla del propio metodo es que la version sube cuando cambia la
# forma, para que un consumidor que no la reconozca se detenga en vez de adivinar.
$SchemaVersion = 2

# Minimos de repeticion por medida. Documentados, no negociables desde la linea de comandos.
$RepeatMinimums = @{
    'core'         = 3
    'ui'           = 3
    'build-ui'     = 2
    'build-plugin' = 2
    'cycle'        = 2
    'ordinary'     = 2
}

# Pasos de cada secuencia. La del ciclo obligatorio NO se toca: es la linea base de G1.
$CycleSteps = @{
    'cycle'    = @(
        @{ name = 'build-ui'; kind = 'build'; args = @('build', 'src/RackCad.UI/RackCad.UI.csproj', '-c', 'Debug', '-v:minimal') }
        @{ name = 'build-plugin'; kind = 'build'; args = @('build', 'src/RackCad.Plugin/RackCad.Plugin.csproj', '-c', 'Debug', '-v:minimal') }
        @{ name = 'core-full'; kind = 'test'; args = @('test', 'tests/RackCad.Tests/RackCad.Tests.csproj', '-c', 'Debug', '-v:minimal') }
        @{ name = 'ui-full'; kind = 'test'; args = @('test', 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj', '-c', 'Debug', '-v:minimal') }
    )
    'ordinary' = @(
        @{ name = 'build-ui'; kind = 'build'; args = @('build', 'src/RackCad.UI/RackCad.UI.csproj', '-c', 'Debug', '-v:minimal') }
        @{ name = 'build-plugin'; kind = 'build'; args = @('build', 'src/RackCad.Plugin/RackCad.Plugin.csproj', '-c', 'Debug', '-v:minimal') }
        @{ name = 'core-full'; kind = 'test'; args = @('test', 'tests/RackCad.Tests/RackCad.Tests.csproj', '-c', 'Debug', '-v:minimal') }
    )
}

$script:Failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure {
    param([string]$Message)
    $script:Failures.Add($Message)
}

function Write-Section {
    param([string]$Title)
    Write-Host ''
    Write-Host "== $Title" -ForegroundColor Cyan
}

# ---------------------------------------------------------------------------
# Procedencia
# ---------------------------------------------------------------------------

function Get-RepositoryRoot {
    param([string]$StartDirectory)

    Push-Location -LiteralPath $StartDirectory
    try {
        $root = & git rev-parse --show-toplevel 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($root)) {
            throw "No hay repositorio git en '$StartDirectory'."
        }
        return (Resolve-Path -LiteralPath $root.Trim()).Path
    }
    finally {
        Pop-Location
    }
}

function Resolve-Dotnet {
    param([string]$Explicit)

    if ($Explicit) {
        if (-not (Test-Path -LiteralPath $Explicit)) { throw "dotnet no encontrado en '$Explicit'." }
        return (Resolve-Path -LiteralPath $Explicit).Path
    }

    # El SDK de este equipo vive a nivel de usuario; Program Files solo trae runtimes.
    $userSdk = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet\dotnet.exe'
    if (Test-Path -LiteralPath $userSdk) { return $userSdk }

    $onPath = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($onPath) { return $onPath.Source }

    throw 'No se encontro dotnet (ni SDK de usuario ni en PATH).'
}

function Get-Provenance {
    param([string]$RepositoryRoot, [string]$Dotnet)

    Push-Location -LiteralPath $RepositoryRoot
    try {
        $sha = (& git rev-parse HEAD 2>$null)
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sha)) {
            # Fallo-cerrado: sin SHA la medicion no es atribuible a nada y no vale.
            throw 'No se pudo determinar el SHA de HEAD. Una medicion sin procedencia no es evidencia.'
        }
        $sha = $sha.Trim()

        $branch = (& git rev-parse --abbrev-ref HEAD).Trim()
        $status = @(& git status --porcelain)
        $subject = (& git log -1 --pretty=format:'%s')
        $sdks = @(& $Dotnet --list-sdks) | ForEach-Object { $_.Trim() }

        return [pscustomobject]@{
            timestampUtc     = (Get-Date).ToUniversalTime().ToString('o')
            timestampLocal   = (Get-Date).ToString('o')
            repositoryRoot   = $RepositoryRoot
            gitSha           = $sha
            gitShaShort      = $sha.Substring(0, 7)
            gitBranch        = $branch
            gitSubject       = $subject
            workingTreeClean = ($status.Count -eq 0)
            dirtyPaths       = $status
            os               = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
            osArchitecture   = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString()
            machineName      = $env:COMPUTERNAME
            processorCount   = [Environment]::ProcessorCount
            powershell       = $PSVersionTable.PSVersion.ToString()
            dotnetPath       = $Dotnet
            dotnetVersion    = (& $Dotnet --version).Trim()
            dotnetSdks       = $sdks
            autocadRunning   = [bool](Get-Process -Name 'acad' -ErrorAction SilentlyContinue)
        }
    }
    finally {
        Pop-Location
    }
}

# ---------------------------------------------------------------------------
# Ejecucion cronometrada y lectura de resultados
# ---------------------------------------------------------------------------

function Invoke-Measured {
    param(
        [string]$Label,
        [string]$FilePath,
        [string[]]$ArgumentList,
        [string]$WorkingDirectory
    )

    Push-Location -LiteralPath $WorkingDirectory
    try {
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        $output = @(& $FilePath @ArgumentList 2>&1 | ForEach-Object { $_.ToString() })
        $exit = $LASTEXITCODE
        $sw.Stop()

        return [pscustomobject]@{
            label    = $Label
            seconds  = [math]::Round($sw.Elapsed.TotalSeconds, 2)
            exitCode = $exit
            output   = $output
        }
    }
    finally {
        Pop-Location
    }
}

<#
    Lee el resumen de VSTest. Funcion PURA sobre las lineas de salida: es lo que permite
    verificar la guarda de cero pruebas con material sintetico, sin filtrar una suite real
    ni tocar ninguna prueba (ver Invoke-SelfTest).

    Soporta las dos localizaciones en que este repositorio ve la salida: espanol
    ("Correctas! - Con error: 0, Superado: 4643, ... Total: 4643") e ingles
    ("Passed! - Failed: 0, Passed: 4643, ... Total: 4643").
#>
function Get-TestOutcome {
    param([string[]]$OutputLines)

    $total = $null; $passed = $null; $failed = $null; $skipped = $null
    $summaryLine = $null
    $sawNoTestsBanner = $false

    foreach ($line in $OutputLines) {
        if ($null -eq $line) { continue }

        if ($line -match 'No test is available in|No hay ninguna prueba disponible') {
            $sawNoTestsBanner = $true
        }

        if ($line -match 'Total:\s*(\d+)') {
            $summaryLine = ($line -replace '\s+', ' ').Trim()
            $total = [int]$Matches[1]
            if ($line -match '(?:Con error|Failed):\s*(\d+)') { $failed = [int]$Matches[1] }
            if ($line -match '(?:Superado|Passed):\s*(\d+)') { $passed = [int]$Matches[1] }
            if ($line -match '(?:Omitido|Skipped):\s*(\d+)') { $skipped = [int]$Matches[1] }
        }
    }

    return [pscustomobject]@{
        total            = $total
        passed           = $passed
        failed           = $failed
        skipped          = $skipped
        summaryLine      = $summaryLine
        sawNoTestsBanner = $sawNoTestsBanner
        summaryFound     = ($null -ne $total)
    }
}

<#
    Guarda de fallo-cerrado sobre el resultado de una corrida de pruebas.

    AGENTS.md: "0 pruebas seleccionadas = FALLO". Aqui se aplica ademas a la corrida
    completa: si no se observa NINGUNA prueba, o no se encuentra el resumen, la medicion
    no vale — un cronometro sobre cero pruebas mide el arranque del runner, no la
    validacion. Devuelve el motivo, o $null si el resultado es admisible.
#>
function Test-OutcomeAdmissible {
    param([pscustomobject]$Outcome, [int]$ExitCode)

    if ($Outcome.sawNoTestsBanner) {
        return 'el runner declaro que no habia pruebas disponibles'
    }
    if (-not $Outcome.summaryFound) {
        return 'no se encontro el resumen de VSTest en la salida (no se puede afirmar que corrio nada)'
    }
    if ($Outcome.total -le 0) {
        return "se observaron $($Outcome.total) pruebas: una seleccion que no selecciona nada es un FALLO"
    }
    if ($ExitCode -ne 0) {
        return "dotnet test salio con codigo $ExitCode"
    }
    return $null
}

function Get-Stats {
    param([double[]]$Values)

    if ($null -eq $Values -or $Values.Count -eq 0) {
        return [pscustomobject]@{ n = 0; min = $null; median = $null; max = $null; mean = $null; spreadRatio = $null }
    }

    $sorted = @($Values | Sort-Object)
    $n = $sorted.Count
    if ($n % 2 -eq 1) {
        $median = $sorted[[int](($n - 1) / 2)]
    }
    else {
        $median = ($sorted[$n / 2 - 1] + $sorted[$n / 2]) / 2
    }

    $min = $sorted[0]
    $max = $sorted[$n - 1]
    $mean = ($sorted | Measure-Object -Average).Average

    return [pscustomobject]@{
        n           = $n
        min         = [math]::Round($min, 2)
        median      = [math]::Round($median, 2)
        max         = [math]::Round($max, 2)
        mean        = [math]::Round($mean, 2)
        spreadRatio = $(if ($min -gt 0) { [math]::Round($max / $min, 2) } else { $null })
    }
}

function Get-EffectiveRepeat {
    param([string]$Key, [int]$Requested)

    $minimum = $RepeatMinimums[$Key]
    $effective = [math]::Max($minimum, $Requested)
    return [pscustomobject]@{
        repeatRequested = $Requested
        repeatMinimum   = $minimum
        repeatEffective = $effective
        raisedToMinimum = ($effective -gt $Requested)
    }
}

# ---------------------------------------------------------------------------
# Medidas
# ---------------------------------------------------------------------------

function Measure-TestSuite {
    param(
        [string]$Key,
        [string]$MetricName,
        [string]$ProjectPath,
        [string]$RepositoryRoot,
        [string]$Dotnet,
        [int]$Requested,
        [bool]$SkipBuild
    )

    $plan = Get-EffectiveRepeat -Key $Key -Requested $Requested
    Write-Section "$MetricName  ($($plan.repeatEffective) repeticiones)"
    if ($plan.raisedToMinimum) {
        Write-Host "  (se pidieron $($plan.repeatRequested); el minimo documentado de esta medida es $($plan.repeatMinimum))" -ForegroundColor Yellow
    }

    $testArgs = @('test', $ProjectPath, '-c', 'Debug', '-v:minimal')
    if ($SkipBuild) { $testArgs += '--no-build' }

    $samples = [System.Collections.Generic.List[object]]::new()
    $seconds = [System.Collections.Generic.List[double]]::new()

    for ($i = 1; $i -le $plan.repeatEffective; $i++) {
        $run = Invoke-Measured -Label $MetricName -FilePath $Dotnet -ArgumentList $testArgs -WorkingDirectory $RepositoryRoot
        $outcome = Get-TestOutcome -OutputLines $run.output
        $reason = Test-OutcomeAdmissible -Outcome $outcome -ExitCode $run.exitCode

        $samples.Add([pscustomobject]@{
                iteration    = $i
                seconds      = $run.seconds
                exitCode     = $run.exitCode
                testsTotal   = $outcome.total
                testsPassed  = $outcome.passed
                testsSkipped = $outcome.skipped
                summaryLine  = $outcome.summaryLine
                admissible   = ($null -eq $reason)
                reason       = $reason
            })

        if ($reason) {
            Add-Failure "$MetricName iteracion ${i}: $reason"
            Write-Host ("  it {0}: {1,7:N1}s  RECHAZADA - {2}" -f $i, $run.seconds, $reason) -ForegroundColor Red
        }
        else {
            $seconds.Add($run.seconds)
            Write-Host ("  it {0}: {1,7:N1}s  {2} pruebas ({3} pasadas, {4} omitidas)" -f $i, $run.seconds, $outcome.total, $outcome.passed, $outcome.skipped)
        }
    }

    $observed = @($samples | ForEach-Object { $_.testsTotal } | Where-Object { $null -ne $_ } | Sort-Object -Unique)

    return [pscustomobject]@{
        metric          = $MetricName
        project         = $ProjectPath
        configuration   = 'Debug'
        includesBuild   = (-not $SkipBuild)
        repeatRequested = $plan.repeatRequested
        repeatMinimum   = $plan.repeatMinimum
        repeatEffective = $plan.repeatEffective
        testCountsSeen  = $observed
        stableTestCount = ($observed.Count -eq 1)
        stats           = Get-Stats -Values $seconds.ToArray()
        samples         = $samples
    }
}

function Measure-Build {
    param(
        [string]$Key,
        [string]$MetricName,
        [string]$ProjectPath,
        [string]$RepositoryRoot,
        [string]$Dotnet,
        [int]$Requested
    )

    $plan = Get-EffectiveRepeat -Key $Key -Requested $Requested
    Write-Section "$MetricName  ($($plan.repeatEffective) repeticiones)"

    $samples = [System.Collections.Generic.List[object]]::new()
    $seconds = [System.Collections.Generic.List[double]]::new()
    $blockedBy = $null

    for ($i = 1; $i -le $plan.repeatEffective; $i++) {
        $run = Invoke-Measured -Label $MetricName -FilePath $Dotnet `
            -ArgumentList @('build', $ProjectPath, '-c', 'Debug', '-v:minimal') `
            -WorkingDirectory $RepositoryRoot

        $lockText = @($run.output | Where-Object { $_ -match 'MSB3021|MSB3027|MSB3026|being used by another process|siendo utilizado por otro proceso' })
        $isLock = ($run.exitCode -ne 0 -and $lockText.Count -gt 0)

        $samples.Add([pscustomobject]@{
                iteration = $i
                seconds   = $run.seconds
                exitCode  = $run.exitCode
                fileLock  = $isLock
                errors    = @($run.output | Where-Object { $_ -match ': error ' } | Select-Object -First 5)
            })

        if ($run.exitCode -ne 0) {
            if ($isLock) {
                # No es una medicion mala: es una IMPOSIBILIDAD del entorno, y se declara como tal.
                $blockedBy = 'autocad-bloquea-el-ensamblado'
                Write-Host ("  it {0}: {1,7:N1}s  BLOQUEADA (archivo en uso; AutoCAD tiene cargado el ensamblado)" -f $i, $run.seconds) -ForegroundColor Yellow
            }
            else {
                Add-Failure "$MetricName iteracion ${i}: dotnet build salio con codigo $($run.exitCode)"
                Write-Host ("  it {0}: {1,7:N1}s  RECHAZADA (exit {2})" -f $i, $run.seconds, $run.exitCode) -ForegroundColor Red
            }
        }
        else {
            $seconds.Add($run.seconds)
            Write-Host ("  it {0}: {1,7:N1}s  ok" -f $i, $run.seconds)
        }
    }

    return [pscustomobject]@{
        metric          = $MetricName
        project         = $ProjectPath
        configuration   = 'Debug'
        repeatRequested = $plan.repeatRequested
        repeatMinimum   = $plan.repeatMinimum
        repeatEffective = $plan.repeatEffective
        blockedBy       = $blockedBy
        stats           = Get-Stats -Values $seconds.ToArray()
        samples         = $samples
    }
}

<#
    Cronometra una SECUENCIA local completa, como UN reloj de pared, no como suma de
    partes medidas por separado: las partes medidas aisladas no comparten el estado de
    compilacion incremental, y su suma no es un numero que nadie viva.

    Hay dos secuencias, y son medidas DISTINTAS que no se comparan como "antes y despues"
    de una optimizacion, porque no significan lo mismo:

    - MandatoryLocalCycleSeconds (G1) — el ciclo obligatorio TAL COMO ERA: build UI +
      build Plugin + Core Full + UI Full. Su definicion NO cambia con LC-UI y se conserva
      para poder seguir reproduciendo la linea base historica.

    - OrdinaryIterationLocalSeconds (G5) — lo que la norma exige en LOCAL antes del push
      en una iteracion ORDINARIA una vez aplicado LC-UI: build UI + build Plugin +
      Core Full. La evidencia de UI de esa iteracion la aporta el CI sobre el SHA exacto
      empujado, y por eso NO aparece aqui. Esta metrica no incluye reloj de CI: la
      preparacion local y la realimentacion del CI se reportan por separado y no se suman
      como si fueran trabajo serial local.
#>
function Measure-Cycle {
    param(
        [string]$RepositoryRoot,
        [string]$Dotnet,
        [int]$Requested,
        [string]$Key,
        [string]$MetricName,
        [string]$DefinitionOf,
        [bool]$LcUiApplied,
        [object[]]$Steps
    )

    $plan = Get-EffectiveRepeat -Key $Key -Requested $Requested
    Write-Section "$MetricName  ($($plan.repeatEffective) repeticiones)"

    $steps = $Steps

    $samples = [System.Collections.Generic.List[object]]::new()
    $completeSeconds = [System.Collections.Generic.List[double]]::new()
    $lowerBoundSeconds = [System.Collections.Generic.List[double]]::new()
    $blockedBy = $null

    for ($i = 1; $i -le $plan.repeatEffective; $i++) {
        $stepResults = [System.Collections.Generic.List[object]]::new()
        $total = 0.0
        $complete = $true

        foreach ($step in $steps) {
            $run = Invoke-Measured -Label $step.name -FilePath $Dotnet -ArgumentList $step.args -WorkingDirectory $RepositoryRoot
            $total += $run.seconds

            $stepOk = ($run.exitCode -eq 0)
            $note = $null
            $tests = $null

            if ($step.kind -eq 'test') {
                $outcome = Get-TestOutcome -OutputLines $run.output
                $tests = $outcome.total
                $reason = Test-OutcomeAdmissible -Outcome $outcome -ExitCode $run.exitCode
                if ($reason) { $stepOk = $false; $note = $reason }
            }
            elseif (-not $stepOk) {
                $lockText = @($run.output | Where-Object { $_ -match 'MSB3021|MSB3027|MSB3026|being used by another process|siendo utilizado por otro proceso' })
                if ($lockText.Count -gt 0) {
                    $note = 'archivo en uso: AutoCAD tiene cargado el ensamblado'
                    $blockedBy = 'autocad-bloquea-el-ensamblado'
                }
                else {
                    $note = "exit $($run.exitCode)"
                }
            }

            $stepResults.Add([pscustomobject]@{ step = $step.name; seconds = $run.seconds; exitCode = $run.exitCode; ok = $stepOk; testsTotal = $tests; note = $note })
            if (-not $stepOk) { $complete = $false }

            Write-Host ("  it {0} / {1,-13} {2,7:N1}s  {3}" -f $i, $step.name, $run.seconds, $(if ($stepOk) { 'ok' } else { "BLOQUEADO/FALLO: $note" }))
        }

        $total = [math]::Round($total, 2)
        $samples.Add([pscustomobject]@{ iteration = $i; seconds = $total; complete = $complete; steps = $stepResults })

        if ($complete) { $completeSeconds.Add($total) } else { $lowerBoundSeconds.Add($total) }
        Write-Host ("  it {0} TOTAL: {1,7:N1}s  ({2})" -f $i, $total, $(if ($complete) { 'ciclo completo' } else { 'COTA INFERIOR: el ciclo no pudo completarse' })) -ForegroundColor $(if ($complete) { 'Green' } else { 'Yellow' })
    }

    return [pscustomobject]@{
        metric          = $MetricName
        definitionOf    = $DefinitionOf
        lcUiApplied     = $LcUiApplied
        steps           = @($steps | ForEach-Object { $_.name })
        repeatRequested = $plan.repeatRequested
        repeatMinimum   = $plan.repeatMinimum
        repeatEffective = $plan.repeatEffective
        blockedBy       = $blockedBy
        statsComplete   = Get-Stats -Values $completeSeconds.ToArray()
        statsLowerBound = Get-Stats -Values $lowerBoundSeconds.ToArray()
        samples         = $samples
    }
}

# ---------------------------------------------------------------------------
# Reconstruccion de calendario (resultado descriptivo, no tiempo humano)
# ---------------------------------------------------------------------------

function Get-SessionReconstruction {
    param(
        [string]$RepositoryRoot,
        [string]$MergeRef,
        [double[]]$ThresholdHours
    )

    Write-Section "Reconstruccion de calendario desde el merge $MergeRef"

    Push-Location -LiteralPath $RepositoryRoot
    try {
        $parents = @((& git rev-list --parents -n 1 $MergeRef 2>$null) -split '\s+' | Where-Object { $_ })
        if ($LASTEXITCODE -ne 0 -or $parents.Count -lt 3) {
            throw "'$MergeRef' no es un commit de merge con dos padres; el rango <merge>^1..<merge>^2 no existe."
        }

        $range = "$MergeRef^1..$MergeRef^2"
        # %at = AUTOR (cuando se escribio). %ct = COMMITTER (lo reescribe un rebase).
        $raw = @(& git log --no-merges --pretty=format:'%H%x1f%at%x1f%ct%x1f%an%x1f%s' $range)
        if ($LASTEXITCODE -ne 0) { throw "git log fallo sobre el rango $range." }

        $commits = @($raw | Where-Object { $_ } | ForEach-Object {
                $f = $_ -split "`u{001f}"
                [pscustomobject]@{
                    sha            = $f[0]
                    authorEpoch    = [long]$f[1]
                    committerEpoch = [long]$f[2]
                    author         = $f[3]
                    subject        = $f[4]
                }
            } | Sort-Object authorEpoch)

        if ($commits.Count -eq 0) {
            throw "El rango $range no contiene commits sin merge."
        }

        # REBASED: si el committer se separa del autor en la mayoria de los commits, las
        # marcas de committer son la hora del rebase, no la del trabajo. Se declara para que
        # nadie las use por error; las cuentas de abajo ya usan solo la marca de AUTOR.
        $shifted = @($commits | Where-Object { [math]::Abs($_.committerEpoch - $_.authorEpoch) -gt 60 })
        $rebased = ($shifted.Count * 2 -ge $commits.Count)

        $byThreshold = foreach ($t in $ThresholdHours) {
            $gapSeconds = $t * 3600
            $groups = [System.Collections.Generic.List[object]]::new()
            $current = [System.Collections.Generic.List[object]]::new()
            $previous = $null

            foreach ($c in $commits) {
                if ($null -ne $previous -and ($c.authorEpoch - $previous.authorEpoch) -gt $gapSeconds) {
                    $groups.Add($current)
                    $current = [System.Collections.Generic.List[object]]::new()
                }
                $current.Add($c)
                $previous = $c
            }
            if ($current.Count -gt 0) { $groups.Add($current) }

            # foreach, no pipeline: canalizar una List[object] cuyos elementos son a su vez
            # List[object] hace que PowerShell intente enumerar el elemento y falla con
            # "Argument types do not match". Verificado en este equipo.
            $sumHours = 0.0
            $largest = 0
            $singles = 0
            foreach ($grp in $groups) {
                $g = @($grp)
                $sumHours += ($g[$g.Count - 1].authorEpoch - $g[0].authorEpoch) / 3600.0
                if ($g.Count -gt $largest) { $largest = $g.Count }
                if ($g.Count -eq 1) { $singles++ }
            }

            [pscustomobject]@{
                thresholdHours      = $t
                blockCount          = $groups.Count
                summedSpanHours     = [math]::Round($sumHours, 2)
                largestBlockCommits = $largest
                singleCommitBlocks  = $singles
            }
        }

        $first = $commits[0]
        $last = $commits[$commits.Count - 1]
        $elapsed = [math]::Round(($last.authorEpoch - $first.authorEpoch) / 3600.0, 2)

        $sensitivity = $null
        $spanValues = @($byThreshold | ForEach-Object { $_.summedSpanHours } | Where-Object { $_ -gt 0 })
        if ($spanValues.Count -ge 2) {
            $lo = ($spanValues | Measure-Object -Minimum).Minimum
            $hi = ($spanValues | Measure-Object -Maximum).Maximum
            if ($lo -gt 0) { $sensitivity = [math]::Round($hi / $lo, 1) }
        }

        Write-Host "  rango .............. $range"
        Write-Host "  commits (sin merge)  $($commits.Count)"
        Write-Host "  autores ............ $((@($commits | ForEach-Object { $_.author } | Sort-Object -Unique)) -join ', ')"
        Write-Host "  primero (autor) .... $([DateTimeOffset]::FromUnixTimeSeconds($first.authorEpoch).ToLocalTime().ToString('yyyy-MM-dd HH:mm'))"
        Write-Host "  ultimo  (autor) .... $([DateTimeOffset]::FromUnixTimeSeconds($last.authorEpoch).ToLocalTime().ToString('yyyy-MM-dd HH:mm'))"
        Write-Host "  calendario total ... $elapsed h"
        Write-Host "  rebase detectado ... $(if ($rebased) { 'SI (marcas de committer inservibles; se usa AUTOR)' } else { 'no' })"
        Write-Host ''
        Write-Host '  umbral(h)  bloques  suma de tramos(h)  mayor bloque  bloques de 1 commit'
        foreach ($r in $byThreshold) {
            Write-Host ("  {0,8}  {1,7}  {2,17}  {3,12}  {4,19}" -f $r.thresholdHours, $r.blockCount, $r.summedSpanHours, $r.largestBlockCommits, $r.singleCommitBlocks)
        }
        if ($sensitivity) {
            Write-Host ''
            Write-Host "  sensibilidad al umbral: x$sensitivity entre el minimo y el maximo" -ForegroundColor Yellow
        }

        return [pscustomobject]@{
            mergeRef             = $MergeRef
            range                = $range
            interpretation       = 'RESULTADO DESCRIPTIVO DE CALENDARIO. Los bloques agrupan commits por proximidad de la marca de AUTOR. NO son tiempo de espera, NO son trabajo humano activo y NO son duracion de sesion.'
            timestampSource      = 'author'
            rebaseDetected       = $rebased
            rebaseShiftedCommits = $shifted.Count
            commitCount          = $commits.Count
            authors              = @($commits | ForEach-Object { $_.author } | Sort-Object -Unique)
            firstAuthorUtc       = [DateTimeOffset]::FromUnixTimeSeconds($first.authorEpoch).UtcDateTime.ToString('o')
            lastAuthorUtc        = [DateTimeOffset]::FromUnixTimeSeconds($last.authorEpoch).UtcDateTime.ToString('o')
            elapsedCalendarHours = $elapsed
            thresholdSensitivity = $sensitivity
            byThreshold          = @($byThreshold)
        }
    }
    finally {
        Pop-Location
    }
}

# ---------------------------------------------------------------------------
# Medicion de CI (gh opcional)
# ---------------------------------------------------------------------------

function Get-CiMeasurement {
    param([string]$RepositoryRoot, [string]$RunSelector)

    Write-Section "Corrida de CI ($RunSelector)"

    $gh = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $gh) {
        Write-Host '  gh no esta disponible: la medida de CI se omite (es opcional).' -ForegroundColor Yellow
        return [pscustomobject]@{
            available = $false
            reason    = 'gh no instalado'
            procedure = 'docs/initiatives/I-45-measurement-method.md, seccion "CI sin gh"'
        }
    }

    Push-Location -LiteralPath $RepositoryRoot
    try {
        $runId = $RunSelector
        if ($RunSelector -eq 'latest') {
            $listJson = & gh run list --limit 1 --json databaseId 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Host '  gh no pudo listar corridas (sin autenticacion o sin remoto).' -ForegroundColor Yellow
                return [pscustomobject]@{ available = $false; reason = 'gh presente pero sin acceso'; detail = (@($listJson) -join ' ') }
            }
            $runId = [string]((@($listJson) -join '' | ConvertFrom-Json)[0].databaseId)
        }

        $runJson = & gh run view $runId --json databaseId,displayTitle,headSha,status,conclusion,createdAt,startedAt,updatedAt,jobs 2>&1
        if ($LASTEXITCODE -ne 0) {
            return [pscustomobject]@{ available = $false; reason = 'gh run view fallo'; detail = (@($runJson) -join ' ') }
        }

        $run = @($runJson) -join '' | ConvertFrom-Json

        $created = [datetimeoffset]::Parse($run.createdAt)
        $started = $(if ($run.startedAt) { [datetimeoffset]::Parse($run.startedAt) } else { $created })
        $updated = [datetimeoffset]::Parse($run.updatedAt)

        $jobs = @($run.jobs | ForEach-Object {
                $js = [datetimeoffset]::Parse($_.startedAt)
                $jc = $(if ($_.completedAt -and $_.completedAt -notlike '0001-01-01*') { [datetimeoffset]::Parse($_.completedAt) } else { $null })
                [pscustomobject]@{
                    name = $_.name
                    conclusion = $_.conclusion
                    startedAt = $js.UtcDateTime.ToString('o')
                    completedAt = $(if ($jc) { $jc.UtcDateTime.ToString('o') } else { $null })
                    # startOffset, NO "cola". Para un job con needs: esto mezcla la cola del
                    # runner con la espera por sus dependencias, y llamarlo cola seria falso.
                    # La cola del runner solo es aislable a nivel de CORRIDA (queueDelaySec).
                    startOffsetSec = [math]::Round(($js - $created).TotalSeconds, 1)
                    durationSec = $(if ($jc) { [math]::Round(($jc - $js).TotalSeconds, 1) } else { $null })
                    endOffsetSec = $(if ($jc) { [math]::Round(($jc - $created).TotalSeconds, 1) } else { $null })
                }
            })

        $finished = @($jobs | Where-Object { $null -ne $_.endOffsetSec } | Sort-Object endOffsetSec -Descending)
        $critical = $(if ($finished.Count -gt 0) { $finished[0] } else { $null })

        $result = [pscustomobject]@{
            available         = $true
            runId             = $run.databaseId
            title             = $run.displayTitle
            headSha           = $run.headSha
            conclusion        = $run.conclusion
            createdAtUtc      = $created.UtcDateTime.ToString('o')
            runWallClockSec   = [math]::Round(($updated - $created).TotalSeconds, 1)
            queueDelaySec     = [math]::Round(($started - $created).TotalSeconds, 1)
            criticalJob       = $(if ($critical) { $critical.name } else { $null })
            criticalJobEndSec = $(if ($critical) { $critical.endOffsetSec } else { $null })
            jobs              = $jobs
        }

        Write-Host "  corrida ................ $($result.runId)  ($($result.conclusion))  sha $($result.headSha.Substring(0,7))"
        Write-Host "  reloj de pared ......... $($result.runWallClockSec) s"
        Write-Host "  cola del runner ........ $($result.queueDelaySec) s  (nivel de corrida; es lo unico aislable como cola)"
        Write-Host "  trabajo critico ........ $($result.criticalJob)  (termina a los $($result.criticalJobEndSec) s)"
        Write-Host ''
        Write-Host '  trabajo                                        inicio(s)  duracion(s)  fin(s)'
        foreach ($j in ($jobs | Sort-Object endOffsetSec)) {
            Write-Host ("  {0,-45} {1,9} {2,12} {3,7}" -f $j.name, $j.startOffsetSec, $j.durationSec, $j.endOffsetSec)
        }
        Write-Host '  (inicio = desplazamiento desde la creacion de la corrida; en un job con needs:'
        Write-Host '   incluye la espera por sus dependencias y NO es cola del runner)'

        return $result
    }
    finally {
        Pop-Location
    }
}

# ---------------------------------------------------------------------------
# Auto-verificacion de las guardas de fallo-cerrado
# ---------------------------------------------------------------------------

<#
    Demuestra que las guardas fallan cuando deben, SIN filtrar una suite real, SIN
    modificar ninguna prueba y SIN escribir nada en el arbol de trabajo.

    La guarda de cero pruebas se ejerce contra las funciones PURAS Get-TestOutcome /
    Test-OutcomeAdmissible con salida sintetica: es exactamente el mismo codigo que juzga
    las corridas reales. La propagacion del codigo de salida se ejerce con un comando
    temporal controlado que no toca el repositorio.
#>
function Invoke-SelfTest {
    Write-Section 'Auto-verificacion de las guardas de fallo-cerrado'

    $cases = @(
        @{ name = 'resumen en espanol con 0 pruebas'; lines = @('Correctas! - Con error: 0, Superado: 0, Omitido: 0, Total: 0, Duracion: 1 ms'); exit = 0; mustReject = $true }
        @{ name = 'resumen en ingles con 0 pruebas'; lines = @('Passed! - Failed: 0, Passed: 0, Skipped: 0, Total: 0, Duration: 1 ms'); exit = 0; mustReject = $true }
        @{ name = 'banner "no test is available"'; lines = @('No test is available in C:\x\y.dll.'); exit = 0; mustReject = $true }
        @{ name = 'banner en espanol'; lines = @('No hay ninguna prueba disponible en C:\x\y.dll.'); exit = 0; mustReject = $true }
        @{ name = 'salida sin resumen'; lines = @('Determinando los proyectos que se van a restaurar...'); exit = 0; mustReject = $true }
        @{ name = 'salida vacia'; lines = @(); exit = 0; mustReject = $true }
        @{ name = 'verde pero exit distinto de 0'; lines = @('Correctas! - Con error: 0, Superado: 10, Omitido: 0, Total: 10'); exit = 1; mustReject = $true }
        @{ name = 'rojo declarado'; lines = @('Con errores! - Con error: 3, Superado: 4640, Omitido: 0, Total: 4643'); exit = 1; mustReject = $true }
        @{ name = 'verde real en espanol'; lines = @('Correctas! - Con error: 0, Superado: 4643, Omitido: 0, Total: 4643, Duracion: 1 m 5 s'); exit = 0; mustReject = $false }
        @{ name = 'verde real en ingles'; lines = @('Passed! - Failed: 0, Passed: 1216, Skipped: 17, Total: 1233, Duration: 3 m 1 s'); exit = 0; mustReject = $false }
    )

    $checks = [System.Collections.Generic.List[object]]::new()

    foreach ($c in $cases) {
        $outcome = Get-TestOutcome -OutputLines $c.lines
        $reason = Test-OutcomeAdmissible -Outcome $outcome -ExitCode $c.exit
        $rejected = ($null -ne $reason)
        $ok = ($rejected -eq $c.mustReject)

        $checks.Add([pscustomobject]@{
                check    = "guarda: $($c.name)"
                expected = $(if ($c.mustReject) { 'rechaza' } else { 'admite' })
                actual   = $(if ($rejected) { 'rechaza' } else { 'admite' })
                ok       = $ok
                reason   = $reason
            })
        Write-Host ("  [{0}] {1,-34} -> {2}" -f $(if ($ok) { 'ok' } else { 'XX' }), $c.name, $(if ($rejected) { "rechazada: $reason" } else { 'admitida' })) -ForegroundColor $(if ($ok) { 'Gray' } else { 'Red' })
        if (-not $ok) { Add-Failure "auto-verificacion: la guarda no se comporto como debe en el caso '$($c.name)'." }
    }

    # Propagacion del codigo de salida: comando temporal controlado, fuera del repositorio.
    $probe = Invoke-Measured -Label 'exit-probe' -FilePath "$env:SystemRoot\System32\cmd.exe" -ArgumentList @('/c', 'exit 7') -WorkingDirectory ([System.IO.Path]::GetTempPath())
    $propagates = ($probe.exitCode -eq 7)
    $checks.Add([pscustomobject]@{ check = 'propagacion de codigo de salida distinto de cero'; expected = '7'; actual = "$($probe.exitCode)"; ok = $propagates; reason = $null })
    Write-Host ("  [{0}] {1,-34} -> observado {2}" -f $(if ($propagates) { 'ok' } else { 'XX' }), 'exit distinto de cero se propaga', $probe.exitCode) -ForegroundColor $(if ($propagates) { 'Gray' } else { 'Red' })
    if (-not $propagates) { Add-Failure "auto-verificacion: un exit 7 se observo como $($probe.exitCode)." }

    # Sin SHA no hay medicion: la procedencia debe fallar fuera de un repositorio.
    #
    # GIT_CEILING_DIRECTORIES no es decoracion. En este equipo el propio directorio de
    # usuario es un repositorio git, de modo que %TEMP% cae DENTRO de un repositorio y
    # git resuelve un toplevel desde cualquier carpeta temporal. Sin el techo, esta
    # comprobacion pasaria por la razon equivocada. El techo obliga a git a detener la
    # busqueda hacia arriba y deja el directorio de sonda realmente fuera de todo repo.
    $shaFailed = $false
    $shaDetail = $null
    $tempRoot = [System.IO.Path]::GetTempPath()
    $tempProbe = Join-Path $tempRoot ('rackcad-g1-' + [guid]::NewGuid().ToString('n'))
    New-Item -ItemType Directory -Path $tempProbe -Force | Out-Null
    $previousCeiling = $env:GIT_CEILING_DIRECTORIES
    $env:GIT_CEILING_DIRECTORIES = $tempRoot.TrimEnd('\')
    try {
        $null = Get-RepositoryRoot -StartDirectory $tempProbe
    }
    catch {
        $shaFailed = $true
        $shaDetail = $_.Exception.Message
    }
    finally {
        if ($null -eq $previousCeiling) { Remove-Item Env:\GIT_CEILING_DIRECTORIES -ErrorAction SilentlyContinue }
        else { $env:GIT_CEILING_DIRECTORIES = $previousCeiling }
        Remove-Item -LiteralPath $tempProbe -Recurse -Force -ErrorAction SilentlyContinue
    }
    $checks.Add([pscustomobject]@{ check = 'sin repositorio no hay procedencia'; expected = 'lanza'; actual = $(if ($shaFailed) { 'lanza' } else { 'no lanza' }); ok = $shaFailed; reason = $shaDetail })
    Write-Host ("  [{0}] {1,-34} -> {2}" -f $(if ($shaFailed) { 'ok' } else { 'XX' }), 'sin repositorio no hay procedencia', $(if ($shaFailed) { 'lanza como debe' } else { 'NO lanzo' })) -ForegroundColor $(if ($shaFailed) { 'Gray' } else { 'Red' })
    if (-not $shaFailed) { Add-Failure 'auto-verificacion: se pudo obtener procedencia fuera de un repositorio git.' }

    $passed = @($checks | Where-Object { $_.ok }).Count
    Write-Host ''
    Write-Host "  auto-verificacion: $passed / $($checks.Count) comprobaciones correctas" -ForegroundColor $(if ($passed -eq $checks.Count) { 'Green' } else { 'Red' })

    return [pscustomobject]@{ total = $checks.Count; passed = $passed; checks = $checks }
}

# ---------------------------------------------------------------------------
# Principal
# ---------------------------------------------------------------------------

$repoRoot = Get-RepositoryRoot -StartDirectory $PSScriptRoot
$dotnet = Resolve-Dotnet -Explicit $DotnetPath
$provenance = Get-Provenance -RepositoryRoot $repoRoot -Dotnet $dotnet

Write-Host ''
Write-Host 'RackCad - metodo de medicion de validacion (I-45, G1)' -ForegroundColor Cyan
Write-Host "  sha $($provenance.gitShaShort) en $($provenance.gitBranch)  |  arbol $(if ($provenance.workingTreeClean) { 'limpio' } else { 'SUCIO' })  |  dotnet $($provenance.dotnetVersion)  |  $($provenance.processorCount) cpu"
if (-not $provenance.workingTreeClean) {
    Write-Host '  AVISO: el arbol de trabajo esta sucio. La medida no es atribuible al SHA por si solo.' -ForegroundColor Yellow
}
if ($provenance.autocadRunning) {
    Write-Host '  AVISO: AutoCAD esta abierto. El build del Plugin puede quedar bloqueado por el ensamblado en uso.' -ForegroundColor Yellow
}

$selected = $(
    if ($Measure -contains 'all') { @('core', 'ui', 'build-ui', 'build-plugin', 'cycle', 'ordinary') }
    elseif ($Measure -contains 'none') { @() }
    else { @($Measure) }
)

$measurements = [ordered]@{}
$selfTestResult = $(if ($SelfTest) { Invoke-SelfTest } else { $null })

if ($selected -contains 'core') {
    $measurements['CoreFullSeconds'] = Measure-TestSuite -Key 'core' -MetricName 'CoreFullSeconds' `
        -ProjectPath 'tests/RackCad.Tests/RackCad.Tests.csproj' -RepositoryRoot $repoRoot -Dotnet $dotnet `
        -Requested $Repeat -SkipBuild:$NoBuild
}
if ($selected -contains 'ui') {
    $measurements['UiFullSeconds'] = Measure-TestSuite -Key 'ui' -MetricName 'UiFullSeconds' `
        -ProjectPath 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj' -RepositoryRoot $repoRoot -Dotnet $dotnet `
        -Requested $Repeat -SkipBuild:$NoBuild
}
if ($selected -contains 'build-ui') {
    $measurements['BuildUiDebugSeconds'] = Measure-Build -Key 'build-ui' -MetricName 'BuildUiDebugSeconds' `
        -ProjectPath 'src/RackCad.UI/RackCad.UI.csproj' -RepositoryRoot $repoRoot -Dotnet $dotnet -Requested $Repeat
}
if ($selected -contains 'build-plugin') {
    $measurements['BuildPluginDebugSeconds'] = Measure-Build -Key 'build-plugin' -MetricName 'BuildPluginDebugSeconds' `
        -ProjectPath 'src/RackCad.Plugin/RackCad.Plugin.csproj' -RepositoryRoot $repoRoot -Dotnet $dotnet -Requested $Repeat
}
if ($selected -contains 'cycle') {
    $measurements['MandatoryLocalCycleSeconds'] = Measure-Cycle -RepositoryRoot $repoRoot -Dotnet $dotnet -Requested $Repeat `
        -Key 'cycle' -MetricName 'MandatoryLocalCycleSeconds' -LcUiApplied $false -Steps $CycleSteps['cycle'] `
        -DefinitionOf 'AGENTS.md "Pruebas - definicion de terminado" puntos 1 y 3: build UI + build Plugin (Debug, 0 errores) + Core Full + UI Full'
}
if ($selected -contains 'ordinary') {
    $measurements['OrdinaryIterationLocalSeconds'] = Measure-Cycle -RepositoryRoot $repoRoot -Dotnet $dotnet -Requested $Repeat `
        -Key 'ordinary' -MetricName 'OrdinaryIterationLocalSeconds' -LcUiApplied $true -Steps $CycleSteps['ordinary'] `
        -DefinitionOf 'Con LC-UI aplicada: lo que la norma exige en LOCAL antes del push en una iteracion ORDINARIA — build UI + build Plugin (Debug) + Core Full. La evidencia de UI de esa iteracion la aporta el CI sobre el SHA exacto empujado, y NO se suma aqui.'
}

$sessionResult = $(if ($Sessions) { Get-SessionReconstruction -RepositoryRoot $repoRoot -MergeRef $Sessions -ThresholdHours $Thresholds } else { $null })
$ciResult = $(if ($Ci) { Get-CiMeasurement -RepositoryRoot $repoRoot -RunSelector $Ci } else { $null })

$status = $(if ($script:Failures.Count -eq 0) { 'OK' } else { 'FAIL' })

$report = [ordered]@{
    schema        = $SchemaName
    schemaVersion = $SchemaVersion
    gate          = 'I-45 G1'
    status        = $status
    failures      = @($script:Failures)
    invocation    = [ordered]@{
        measure    = @($selected)
        repeat     = $Repeat
        noBuild    = [bool]$NoBuild
        thresholds = @($Thresholds)
        selfTest   = [bool]$SelfTest
    }
    provenance    = $provenance
    measurements  = $measurements
    sessions      = $sessionResult
    ci            = $ciResult
    selfTest      = $selfTestResult
}

if (-not $OutputPath) {
    $stamp = (Get-Date).ToString('yyyyMMdd-HHmmss')
    $OutputPath = Join-Path $repoRoot "artifacts/validation/measure-$stamp-$($provenance.gitShaShort).json"
}
$outDir = Split-Path -Parent $OutputPath
if ($outDir -and -not (Test-Path -LiteralPath $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }

$report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath -Encoding utf8
Copy-Item -LiteralPath $OutputPath -Destination (Join-Path (Split-Path -Parent $OutputPath) 'measure-latest.json') -Force

Write-Host ''
Write-Host "== Resultado: $status" -ForegroundColor $(if ($status -eq 'OK') { 'Green' } else { 'Red' })
foreach ($f in $script:Failures) { Write-Host "   - $f" -ForegroundColor Red }
Write-Host "   JSON: $OutputPath"
Write-Host ''

if ($status -ne 'OK') { exit 1 }
exit 0
