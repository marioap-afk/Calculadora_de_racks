#Requires -Version 7.0

<#
.SYNOPSIS
    Reconstruye y clasifica las ejecuciones de evidencia de un corpus de iniciativas.

.DESCRIPTION
    I-45 gate G3. Cuenta QUE validaciones se repitieron y POR QUE, con la taxonomia fijada en G1:
    NEW_EVIDENCE / EXACT_SHA_RECONFIRMATION / CROSS_CHANNEL / POLICY_REQUIRED / UNKNOWN.

    G3 MIDE. No elimina obligaciones, no aplica LC-UI, no aplica reutilizacion por SHA exacto y no
    toca el CI. El metodo esta descrito en docs/initiatives/I-45-repetition-measurement.md.

    POR QUE ES UN SCRIPT APARTE Y NO UNA EXTENSION DE measure-validation.ps1:
    aquel es un cronometro sobre comandos VIVOS, y toda su semantica de fallo-cerrado gira sobre el
    conteo de pruebas de una corrida que el mismo lanza ("cero pruebas observadas = FALLO"). Este no
    ejecuta ninguna suite: lee historia —git y la API de Actions— y clasifica. Meter un clasificador
    historico dentro del cronometro le impondria una semantica de fallo que aqui no significa nada, y
    obligaria a convivir a dos esquemas de salida distintos en un mismo archivo. Comparten
    deliberadamente la forma de la procedencia y la disciplina de escribir a artifacts/.

.PARAMETER ClaimsPath
    JSON con las afirmaciones documentales de ejecucion (canal local y del dueno), producido por la
    reconstruccion documental. Sin el, solo se reconstruye el canal de CI y el resto queda declarado
    como no observado — nunca como cero.

.PARAMETER PolicyPath
    JSON opcional con las reglas normativas vigentes y CITABLES que obligaban a repetir. Sin el,
    ninguna ejecucion se clasifica como POLICY_REQUIRED: la costumbre no es norma.

.EXAMPLE
    pwsh -File eng/validation/measure-repetition.ps1 -ClaimsPath artifacts/validation/g3-claims.json
#>

[CmdletBinding()]
param(
    [string]$ClaimsPath,
    [string]$PolicyPath,
    [string]$OutputPath,
    [string]$CiCachePath,
    [switch]$RefreshCi,
    [switch]$Controls
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$SchemaName = 'rackcad-validation-repetition'
# v2 (G6): cada ejecucion publica `crossChannelBasis`, que declara que el cruce de canal se detecta por
# CO-UBICACION en el mismo commit y no por identidad de SHA. La forma cambia, asi que la version sube.
$SchemaVersion = 2

# Corpus de G3. Es el corpus comparativo de Discovery, que ya tiene evidencia suficiente.
$Corpus = @(
    [pscustomobject]@{ id = 'I-40'; merge = 'bf327b3'; contract = 'docs/initiatives/I-40-cabeceras-push-back.md' }
    [pscustomobject]@{ id = 'I-42'; merge = 'e6bb6d7'; contract = 'docs/initiatives/I-42-push-back-compuesto.md' }
    [pscustomobject]@{ id = 'I-43'; merge = 'bd40ef7'; contract = 'docs/initiatives/I-43-selectivo-scopes-fondos.md' }
    [pscustomobject]@{ id = 'I-44'; merge = '1165240'; contract = $null }
)

# Clases de evidencia locales y su correspondencia con el canal de CI. Sirve para decidir si una
# corrida de CI sobre un commit es CROSS_CHANNEL respecto de una afirmacion local del mismo commit.
$LocalClasses = @('CoreFullLocal', 'UiFullLocal', 'BuildUiDebugLocal', 'BuildPluginDebugLocal')

$script:Notes = [System.Collections.Generic.List[string]]::new()
function Add-Note { param([string]$m) $script:Notes.Add($m) }

function Get-RepositoryRoot {
    param([string]$StartDirectory)
    Push-Location -LiteralPath $StartDirectory
    try {
        $root = & git rev-parse --show-toplevel 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($root)) { throw "No hay repositorio git en '$StartDirectory'." }
        return (Resolve-Path -LiteralPath $root.Trim()).Path
    }
    finally { Pop-Location }
}

function Get-Provenance {
    param([string]$RepositoryRoot)
    Push-Location -LiteralPath $RepositoryRoot
    try {
        $sha = & git rev-parse HEAD 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($sha)) {
            throw 'No se pudo determinar el SHA de HEAD. Una reconstruccion sin procedencia no es evidencia.'
        }
        $sha = $sha.Trim()
        $status = @(& git status --porcelain)
        return [pscustomobject]@{
            timestampUtc     = (Get-Date).ToUniversalTime().ToString('o')
            repositoryRoot   = $RepositoryRoot
            gitSha           = $sha
            gitShaShort      = $sha.Substring(0, 7)
            gitBranch        = (& git rev-parse --abbrev-ref HEAD).Trim()
            workingTreeClean = ($status.Count -eq 0)
            machineName      = $env:COMPUTERNAME
            powershell       = $PSVersionTable.PSVersion.ToString()
        }
    }
    finally { Pop-Location }
}

# ---------------------------------------------------------------------------
# Canal de CI: evidencia de maquina
# ---------------------------------------------------------------------------

<#
    La API de Actions manda sobre cualquier afirmacion en prosa. Se cachea en artifacts/ para que la
    reconstruccion sea repetible sin depender de la red, pero el cache NO es la autoridad: -RefreshCi
    lo rehace.
#>
function Get-CiRuns {
    param([string]$RepositoryRoot, [string]$CachePath, [bool]$Refresh)

    if ((-not $Refresh) -and $CachePath -and (Test-Path -LiteralPath $CachePath)) {
        $cached = Get-Content -LiteralPath $CachePath -Raw | ConvertFrom-Json
        Add-Note "Corridas de CI leidas del cache '$CachePath' ($($cached.runs.Count) corridas, capturado $($cached.capturedUtc))."
        return $cached
    }

    $gh = Get-Command gh -ErrorAction SilentlyContinue
    if (-not $gh) { throw 'gh no esta disponible y no hay cache de corridas de CI. La evidencia de maquina es obligatoria en G3.' }

    Push-Location -LiteralPath $RepositoryRoot
    try {
        $totalRaw = & gh api 'repos/{owner}/{repo}/actions/runs?per_page=1' --jq '.total_count' 2>&1
        if ($LASTEXITCODE -ne 0) { throw "gh no pudo consultar el total de corridas: $totalRaw" }
        $total = [int](@($totalRaw) -join '')

        # Se pagina la API en vez de usar `gh run list`, y NO por gusto: un RE-INTENTO de GitHub
        # conserva el mismo id de corrida e incrementa `run_attempt`. `gh run list` devuelve una fila
        # por corrida, de modo que un rerun autentico es INVISIBLE en esa lista y se contaria como una
        # sola ejecucion. Solo el campo run_attempt de la API los revela.
        $raw = [System.Collections.Generic.List[object]]::new()
        $page = 1
        while ($true) {
            $pageRaw = & gh api "repos/{owner}/{repo}/actions/runs?per_page=100&page=$page" 2>&1
            if ($LASTEXITCODE -ne 0) { throw "gh api fallo en la pagina ${page}: $pageRaw" }
            $parsed = (@($pageRaw) -join '') | ConvertFrom-Json
            if (-not $parsed.workflow_runs -or $parsed.workflow_runs.Count -eq 0) { break }
            foreach ($w in $parsed.workflow_runs) { $raw.Add($w) }
            $page++
            if ($page -gt 50) { throw 'Paginacion desbocada al leer corridas de CI.' }
        }

        # Fallo-cerrado sobre la completitud: si lo recuperado no cubre el total declarado por la API,
        # cualquier conclusion sobre "SHAs con una sola corrida" seria un artefacto de paginacion.
        if ($raw.Count -lt $total) {
            throw "Recuperadas $($raw.Count) corridas de $total declaradas por la API. La reconstruccion seria incompleta y no se emite."
        }

        # Cada corrida se expande en tantas EJECUCIONES como intentos tuvo. Para las que tienen mas de
        # uno se consulta cada intento, porque la conclusion del intento anterior decide si el
        # siguiente reconfirma algo o persigue un fallo.
        $runs = [System.Collections.Generic.List[object]]::new()
        $rerunCount = 0
        foreach ($w in $raw) {
            $attempts = [int]$w.run_attempt
            if ($attempts -le 1) {
                $runs.Add([pscustomobject]@{
                        databaseId = $w.id; attempt = 1; headSha = $w.head_sha; headBranch = $w.head_branch
                        createdAt = $w.created_at; conclusion = $w.conclusion; event = $w.event; isRerun = $false
                    })
                continue
            }
            $rerunCount++
            for ($a = 1; $a -le $attempts; $a++) {
                $attRaw = & gh api "repos/{owner}/{repo}/actions/runs/$($w.id)/attempts/$a" 2>&1
                if ($LASTEXITCODE -ne 0) { throw "No se pudo leer el intento $a de la corrida $($w.id): un rerun sin detalle haria incompleto el conteo." }
                $att = (@($attRaw) -join '') | ConvertFrom-Json
                $runs.Add([pscustomobject]@{
                        databaseId = $w.id; attempt = $a; headSha = $att.head_sha; headBranch = $att.head_branch
                        createdAt = $att.run_started_at; conclusion = $att.conclusion; event = $att.event; isRerun = ($a -gt 1)
                    })
            }
        }

        $payload = [pscustomobject]@{
            capturedUtc   = (Get-Date).ToUniversalTime().ToString('o')
            apiTotalCount = $total
            runsWithRerun = $rerunCount
            runs          = @($runs)
        }
        if ($CachePath) {
            $dir = Split-Path -Parent $CachePath
            if ($dir -and -not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
            $payload | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $CachePath -Encoding utf8
        }
        return $payload
    }
    finally { Pop-Location }
}

# ---------------------------------------------------------------------------
# Invalidadores (V4). Sin procedencia, UNKNOWN: nunca se asume estabilidad.
# ---------------------------------------------------------------------------

<#
    Devuelve el estado de invalidacion de una posible reconfirmacion por SHA exacto.

    CI: el runner se crea limpio y hace checkout del SHA exacto, asi que "arbol sucio" no aplica. Pero
    el flujo fija `dotnet-version: "8.0.x"`, que es un pin FLOTANTE: dos corridas del mismo SHA en
    fechas distintas pueden resolver parches de SDK distintos. Si no se puede leer el SDK resuelto de
    ambas corridas, el invalidador "SDK resuelto cambio" queda en UNKNOWN.

    Local: la historia NO registra si el arbol estaba limpio al producir la evidencia, de modo que el
    invalidador acordado en V4 es indeterminable por construccion.
#>
function Get-InvalidationStatus {
    param([string]$Channel)

    switch ($Channel) {
        'ci' {
            return [pscustomobject]@{
                status = 'UNKNOWN'
                reason = 'el flujo fija dotnet-version 8.0.x (pin flotante): sin el SDK resuelto de ambas corridas no se puede descartar el invalidador "SDK resuelto cambio"'
            }
        }
        'local' {
            return [pscustomobject]@{
                status = 'UNKNOWN'
                reason = 'la historia no registra si el arbol de trabajo estaba limpio al producir la evidencia local (invalidador acordado en V4)'
            }
        }
        default {
            return [pscustomobject]@{
                status = 'UNKNOWN'
                reason = 'no hay procedencia registrada de version de AutoCAD ni de biblioteca de bloques externa'
            }
        }
    }
}

# ---------------------------------------------------------------------------
# Clasificacion
# ---------------------------------------------------------------------------

<#
    Prioridad, para que ninguna ejecucion caiga en dos numeradores (V4 §8):

      1. commit de merge  -> NEW_EVIDENCE, sin excepcion (SHA nuevo, y RackCad lo estampa en los
                             ensamblados; decision cerrada, no se reabre)
      2. norma citable     -> POLICY_REQUIRED (+ wouldOtherwiseBeExactShaReconfirmation)
      3. mismo SHA, misma clase, sin invalidador conocido -> EXACT_SHA_RECONFIRMATION
      4. mismo SHA, misma clase, invalidacion UNKNOWN     -> UNKNOWN
      5. mismo SHA, evidencia previa en OTRO canal        -> CROSS_CHANNEL
      6. resto                                            -> NEW_EVIDENCE
#>
function Get-Classification {
    param(
        [bool]$IsMergeCommit,
        [bool]$PriorSameClassSameSha,
        [bool]$PriorOtherChannelSameSha,
        [string]$Channel,
        [pscustomobject]$PolicyRule
    )

    if ($IsMergeCommit) {
        return [pscustomobject]@{
            classification = 'NEW_EVIDENCE'
            reason         = 'commit de merge: SHA nuevo estampado en los ensamblados; post-merge es evidencia nueva por definicion (V4)'
            wouldOtherwiseBeExactSha = $false
        }
    }

    if ($null -ne $PolicyRule) {
        return [pscustomobject]@{
            classification = 'POLICY_REQUIRED'
            reason         = "norma vigente citable: $($PolicyRule.citation)"
            wouldOtherwiseBeExactSha = $PriorSameClassSameSha
        }
    }

    if ($PriorSameClassSameSha) {
        $inv = Get-InvalidationStatus -Channel $Channel
        if ($inv.status -eq 'CLEAN') {
            return [pscustomobject]@{
                classification = 'EXACT_SHA_RECONFIRMATION'
                reason         = 'misma clase sobre el mismo SHA, sin invalidador conocido'
                wouldOtherwiseBeExactSha = $false
            }
        }
        return [pscustomobject]@{
            classification = 'UNKNOWN'
            reason         = "misma clase sobre el mismo SHA, pero la invalidacion no es determinable: $($inv.reason)"
            wouldOtherwiseBeExactSha = $true
        }
    }

    if ($PriorOtherChannelSameSha) {
        return [pscustomobject]@{
            classification = 'CROSS_CHANNEL'
            reason         = 'mismo commit ya tenia evidencia declarada en otro canal; las clases no son intercambiables'
            wouldOtherwiseBeExactSha = $false
        }
    }

    return [pscustomobject]@{
        classification = 'NEW_EVIDENCE'
        reason         = 'primera evidencia de esta clase sobre este estado'
        wouldOtherwiseBeExactSha = $false
    }
}

# ---------------------------------------------------------------------------
# Reconstruccion por iniciativa
# ---------------------------------------------------------------------------

function Get-InitiativeReconstruction {
    param(
        [pscustomobject]$Initiative,
        [string]$RepositoryRoot,
        [hashtable]$RunsBySha,
        [object[]]$Claims,
        [object[]]$PolicyRules
    )

    Push-Location -LiteralPath $RepositoryRoot
    try {
        $mergeSha = (& git rev-parse $Initiative.merge).Trim()

        # TODOS los commits del rango, merges internos de rama INCLUIDOS. No se usa --no-merges: un
        # merge de origin/main dentro de la rama es un SHA propio que RECIBE su corrida de CI, y
        # excluirlo perderia una ejecucion de evidencia real. (Verificado: I-43 tiene uno, d582dee,
        # y tiene corrida.) El merge de INTEGRACION a main se trata aparte, por la regla de V4.
        $raw = @(& git log --format='%H%x1f%at%x1f%p%x1f%s' "$($Initiative.merge)^1..$($Initiative.merge)^2")
        $commits = @($raw | Where-Object { $_ } | ForEach-Object {
                $f = $_ -split "`u{001f}"
                [pscustomobject]@{
                    sha            = $f[0]
                    epoch          = [long]$f[1]
                    isBranchMerge  = ((@($f[2] -split '\s+' | Where-Object { $_ })).Count -gt 1)
                    subject        = $f[3]
                }
            } | Sort-Object epoch)

        if ($commits.Count -eq 0) { throw "El rango de $($Initiative.id) no contiene commits." }

        $initClaims = @($Claims | Where-Object { $_.initiative -eq $Initiative.id })

        # Afirmaciones locales indexadas por el commit que las CONTIENE.
        #
        # ATENCION, y es la razon de que este indice se llame asi: el commit contenedor NO es el SHA
        # que esa evidencia atestigua. La suite local se ejecuta ANTES de que el commit exista, y
        # Directory.Build.targets estampa `git rev-parse HEAD`, que en ese momento es el PADRE. Este
        # indice solo sirve para detectar CO-UBICACION —"en este commit alguien afirmo haber corrido
        # algo"—, que es una base MAS DEBIL que la identidad de SHA y se declara como tal en cada
        # ejecucion (`crossChannelBasis`).
        #
        # Este indice NO rellena el campo `sha`. Ese campo solo se llena por dos vias, ambas
        # declaradas en `shaBasis`: VERBATIM, cuando la frase nombra el SHA, y STATED_AFTER_COMMIT,
        # cuando el texto afirma explicitamente que la corrida fue POSTERIOR a crear ese commit —el
        # unico caso en que el commit contenedor SI es el SHA atestiguado, y por lo que el texto dice,
        # no por co-ubicacion—. Cualquier otra afirmacion local se queda con `sha = $null`.
        $localByCommit = @{}
        foreach ($c in $initClaims) {
            if ($c.channel -ne 'local') { continue }
            if ($c.sourceType -ne 'commit-body') { continue }
            if (-not $localByCommit.ContainsKey($c.source)) { $localByCommit[$c.source] = @() }
            $localByCommit[$c.source] += $c
        }

        $executions = [System.Collections.Generic.List[object]]::new()
        $commitsWithoutCi = [System.Collections.Generic.List[object]]::new()
        $crossChannelPairs = [System.Collections.Generic.List[object]]::new()

        # --- Canal local y del dueno, desde la reconstruccion documental ---
        foreach ($c in $initClaims) {
            if ($c.channel -eq 'ci') { continue }   # el CI se toma de la API, no de la prosa

            $inv = Get-InvalidationStatus -Channel $c.channel

            # Un SHA solo se acepta si la propia frase lo nombra. Si la afirmacion vive en el cuerpo de
            # un commit y no nombra SHA, el SHA atestiguado es INDETERMINABLE: la suite local se ejecuto
            # antes de que ese commit existiera, y Directory.Build.targets estampa `git rev-parse HEAD`,
            # que en ese momento es el PADRE. Rellenarlo con el commit contenedor seria inventar.
            $sha = $null
            $shaBasis = 'NONE'
            if ($c.claimedShaVerbatim) { $sha = $c.claimedShaVerbatim; $shaBasis = 'VERBATIM' }
            elseif ($c.ranAfterCommitStated) { $sha = $c.source; $shaBasis = 'STATED_AFTER_COMMIT' }

            $cls = Get-Classification -IsMergeCommit $false -PriorSameClassSameSha $false `
                -PriorOtherChannelSameSha $false -Channel $c.channel -PolicyRule $null

            $executions.Add([pscustomobject]@{
                    initiative               = $Initiative.id
                    timestamp                = $c.timestamp
                    source                   = $c.source
                    sourceType               = $c.sourceType
                    attempt                  = 1
                    isRerun                  = $false
                    mergeKind                = $null
                    crossChannelBasis        = $null
                    priorAttemptConclusion   = $null
                    sha                      = $sha
                    shaBasis                 = $shaBasis
                    evidenceClass            = $c.evidenceClass
                    channel                  = $c.channel
                    candidateDeclared        = [bool]$c.candidateDeclared
                    classification           = $cls.classification
                    reason                   = $cls.reason
                    exactShaEligible         = ($shaBasis -ne 'NONE' -and $cls.classification -ne 'UNKNOWN')
                    exactShaIneligibleReason = $(if ($shaBasis -eq 'NONE') { 'el SHA atestiguado por esta evidencia local es indeterminable: la suite se ejecuto antes de que existiera el commit que la describe, y el build estampa git rev-parse HEAD, que entonces es el PADRE' } elseif ($cls.classification -eq 'UNKNOWN') { $inv.reason } else { $null })
                    wouldOtherwiseBeExactShaReconfirmation = $cls.wouldOtherwiseBeExactSha
                    confidence               = $c.confidence
                    quote                    = $c.quote
                })
        }

        # --- Canal de CI, desde la API ---
        $branchMergeShas = @($commits | Where-Object { $_.isBranchMerge } | ForEach-Object { $_.sha })
        $allShas = @($commits | ForEach-Object { $_.sha }) + @($mergeSha)
        foreach ($sha in $allShas) {
            $isIntegrationMerge = ($sha -eq $mergeSha)
            $isMerge = ($isIntegrationMerge -or ($sha -in $branchMergeShas))
            $mergeKind = $(if ($isIntegrationMerge) { 'integracion-a-main' } elseif ($isMerge) { 'merge-interno-de-rama' } else { $null })
            $runs = @()
            if ($RunsBySha.ContainsKey($sha)) { $runs = @($RunsBySha[$sha] | Sort-Object createdAt) }

            if ($runs.Count -eq 0) {
                if (-not $isMerge) {
                    # Push agrupado: la corrida del tip NO se propaga hacia atras (decision de V4).
                    $commitsWithoutCi.Add([pscustomobject]@{ sha = $sha; ciEvidence = 'NONE' })
                }
                else {
                    $commitsWithoutCi.Add([pscustomobject]@{ sha = $sha; ciEvidence = 'NONE'; isMerge = $true })
                }
                continue
            }

            $hasLocalClaim = $localByCommit.ContainsKey($sha)
            for ($i = 0; $i -lt $runs.Count; $i++) {
                $run = $runs[$i]

                # Solo se puede RECONFIRMAR lo que antes quedo confirmado. Una ejecucion anterior que
                # termino en rojo o cancelada no establecio nada, asi que la siguiente sobre el mismo
                # SHA no reconfirma: persigue un fallo, y es la PRIMERA evidencia verde de esa clase.
                # Contarla como reconfirmacion la presentaria como ahorro disponible, y quitarla
                # significaria "no reintentar nunca un CI fallido", que no es lo que propone la regla.
                $priorGreen = 0
                for ($k = 0; $k -lt $i; $k++) { if ($runs[$k].conclusion -eq 'success') { $priorGreen++ } }
                $priorSame = ($priorGreen -gt 0)

                # CO-UBICACION, no identidad de SHA. El cruce de canal se detecta porque el cuerpo de
                # ESTE commit afirma una corrida local; que esa corrida atestiguara este SHA es
                # indemostrable (se ejecuto antes de que el commit existiera). La base queda declarada
                # en `crossChannelBasis` para que ningun consumidor la lea como identidad.
                $priorOther = ($i -eq 0 -and $hasLocalClaim -and -not $isMerge)
                $crossBasis = $(if ($priorOther) { 'commit-colocated (NO es identidad de SHA)' } else { $null })
                $priorConclusion = $(if ($i -gt 0) { $runs[$i - 1].conclusion } else { $null })

                $rule = $null
                foreach ($p in $PolicyRules) {
                    if ($p.appliesTo -eq 'ci' -and $p.initiative -eq $Initiative.id) { $rule = $p; break }
                }

                $cls = Get-Classification -IsMergeCommit $isMerge -PriorSameClassSameSha $priorSame `
                    -PriorOtherChannelSameSha $priorOther -Channel 'ci' -PolicyRule $rule

                if ($cls.classification -eq 'CROSS_CHANNEL') {
                    $crossChannelPairs.Add([pscustomobject]@{
                            sha           = $sha
                            localClasses  = @($localByCommit[$sha] | ForEach-Object { $_.evidenceClass } | Sort-Object -Unique)
                            ciRunId       = $run.databaseId
                        })
                }

                $inv = Get-InvalidationStatus -Channel 'ci'
                $executions.Add([pscustomobject]@{
                        initiative               = $Initiative.id
                        timestamp                = $run.createdAt
                        source                   = "github-actions-run:$($run.databaseId)#attempt$($run.attempt)"
                        sourceType               = 'github-actions'
                        attempt                  = $run.attempt
                        isRerun                  = $run.isRerun
                        mergeKind                = $mergeKind
                        crossChannelBasis        = $crossBasis
                        priorAttemptConclusion   = $priorConclusion
                        sha                      = $sha
                        shaBasis                 = 'API'
                        evidenceClass            = 'CI'
                        channel                  = 'ci'
                        candidateDeclared        = $false
                        classification           = $cls.classification
                        reason                   = $cls.reason
                        exactShaEligible         = ($cls.classification -ne 'UNKNOWN')
                        exactShaIneligibleReason = $(if ($cls.classification -eq 'UNKNOWN') { $inv.reason } else { $null })
                        wouldOtherwiseBeExactShaReconfirmation = $cls.wouldOtherwiseBeExactSha
                        confidence               = 'MEASURED'
                        quote                    = "$($run.conclusion) en $($run.headBranch)"
                    })
            }
        }

        # Pushes agrupados: commits sin corrida propia, atribuidos al siguiente commit que si la tiene.
        $groupedPushes = 0
        $pending = 0
        foreach ($c in $commits) {
            if ($RunsBySha.ContainsKey($c.sha)) {
                if ($pending -gt 0) { $groupedPushes++ }
                $pending = 0
            }
            else { $pending++ }
        }

        return [pscustomobject]@{
            initiative        = $Initiative.id
            mergeSha          = $mergeSha
            contract          = $Initiative.contract
            commitCount       = $commits.Count
            mergeHasCiRun     = $RunsBySha.ContainsKey($mergeSha)
            commitsWithCi     = @($commits | Where-Object { $RunsBySha.ContainsKey($_.sha) }).Count
            commitsWithoutCi  = $commitsWithoutCi.Count
            groupedPushes     = $groupedPushes
            crossChannelPairs = $crossChannelPairs
            executions        = $executions
        }
    }
    finally { Pop-Location }
}

function Get-Metrics {
    param([object[]]$Executions)

    $byClass = @{}
    foreach ($k in @('NEW_EVIDENCE', 'EXACT_SHA_RECONFIRMATION', 'CROSS_CHANNEL', 'POLICY_REQUIRED', 'UNKNOWN')) { $byClass[$k] = 0 }
    foreach ($e in $Executions) { $byClass[$e.classification] = $byClass[$e.classification] + 1 }

    # `eligible` = ejecuciones sobre las que la pregunta "¿fue esto una reconfirmacion por SHA exacto?"
    # SE PUEDE contestar, en un sentido o en otro. Son las que cumplen las dos condiciones:
    #
    #   (a) el SHA que la evidencia atestigua es determinable, y
    #   (b) la clasificacion quedo resuelta, es decir NO es UNKNOWN.
    #
    # Una primera y unica corrida sobre un SHA ES elegible: se puede afirmar definitivamente que no fue
    # reconfirmacion, y el invalidador de deriva del SDK no interviene porque no hay nada que invalidar.
    # El invalidador solo decide cuando existe una segunda ejecucion de la misma clase sobre el mismo
    # SHA; ahi, si no es determinable, la ejecucion cae en UNKNOWN y sale del denominador.
    #
    # El denominador NO son commits. Los commits son una metrica secundaria.
    $eligible = @($Executions | Where-Object { $_.exactShaEligible })
    $excluded = @($Executions | Where-Object { -not $_.exactShaEligible })
    $rate = $null
    if ($eligible.Count -gt 0) {
        $rate = [math]::Round($byClass['EXACT_SHA_RECONFIRMATION'] / $eligible.Count, 4)
    }

    return [pscustomobject]@{
        EvidenceExecutionsTotal   = $Executions.Count
        NewEvidence               = $byClass['NEW_EVIDENCE']
        ExactShaReconfirmations   = $byClass['EXACT_SHA_RECONFIRMATION']
        CrossChannelExecutions    = $byClass['CROSS_CHANNEL']
        PolicyRequiredRepetitions = $byClass['POLICY_REQUIRED']
        UnknownExecutions         = $byClass['UNKNOWN']
        EligibleForExactShaRule   = $eligible.Count
        ExcludedFromDenominator   = $excluded.Count
        ReconfirmationRate        = $rate
        WouldOtherwiseBeExactSha  = @($Executions | Where-Object { $_.wouldOtherwiseBeExactShaReconfirmation }).Count
    }
}

# ---------------------------------------------------------------------------
# Controles falsadores A-D
# ---------------------------------------------------------------------------

function Invoke-Controls {
    param([string]$RepositoryRoot, [hashtable]$RunsBySha, [object[]]$AllExecutions)

    $results = [System.Collections.Generic.List[object]]::new()

    # A — un mismo SHA con dos corridas de CI conocidas.
    $multi = @($RunsBySha.Keys | Where-Object { $RunsBySha[$_].Count -gt 1 })
    if ($multi.Count -eq 0) {
        $results.Add([pscustomobject]@{ control = 'A'; present = $false; verdict = 'NO_EXAMPLE'; detail = 'no existe ningun SHA con mas de una corrida de CI en toda la historia recuperada' })
    }
    else {
        $detail = foreach ($sha in $multi) {
            $runs = @($RunsBySha[$sha] | Sort-Object createdAt)
            $second = $runs[1]
            $priorGreen = @($runs[0..($runs.Count - 2)] | Where-Object { $_.conclusion -eq 'success' }).Count -gt 0
            $inv = Get-InvalidationStatus -Channel 'ci'
            $cls = Get-Classification -IsMergeCommit $false -PriorSameClassSameSha $priorGreen -PriorOtherChannelSameSha $false -Channel 'ci' -PolicyRule $null
            [pscustomobject]@{
                sha              = $sha.Substring(0, 7)
                executions       = $runs.Count
                # Dos formas MUY distintas de tener el mismo SHA dos veces, y solo la primera es un
                # rerun: (a) re-intento del mismo run (run_attempt > 1); (b) el mismo commit empujado
                # bajo una segunda referencia, que crea una corrida nueva y no es un reintento.
                kind             = $(if (@($runs | Where-Object { $_.isRerun }).Count -gt 0) { 'RERUN (run_attempt>1)' } else { 'RE-PUSH DE LA MISMA REVISION BAJO OTRA REFERENCIA' })
                branches         = @($runs | ForEach-Object { $_.headBranch } | Sort-Object -Unique)
                conclusions      = @($runs | ForEach-Object { $_.conclusion })
                priorWasGreen    = $priorGreen
                secondRunId      = $second.databaseId
                classification   = $cls.classification
                invalidation     = $inv.status
            }
        }
        $detail = @($detail)
        # El control PASA si ninguna de estas repeticiones acaba mal clasificada: una repeticion sobre
        # un verde previo debe ser EXACT_SHA_RECONFIRMATION o UNKNOWN (nunca NEW_EVIDENCE), y una sobre
        # un rojo/cancelado previo debe ser NEW_EVIDENCE (nunca reconfirmacion).
        $bad = @($detail | Where-Object {
                ($_.priorWasGreen -and $_.classification -notin @('EXACT_SHA_RECONFIRMATION', 'UNKNOWN')) -or
                ((-not $_.priorWasGreen) -and $_.classification -ne 'NEW_EVIDENCE')
            })
        $results.Add([pscustomobject]@{
                control = 'A'; present = $true
                verdict = $(if ($bad.Count -eq 0) { 'PASS' } else { 'FAIL' })
                detail  = $detail
                note    = "$(@($detail | Where-Object { $_.priorWasGreen }).Count) de $($detail.Count) repiten sobre una ejecucion previa VERDE"
            })
    }

    # B — un cambio de SHA entre dos ejecuciones de la misma clase NO puede dar reconfirmacion por
    # SHA exacto. Se comprueba de dos maneras, y la primera PUEDE fallar de verdad:
    #   B1 (invariante) toda EXACT_SHA_RECONFIRMATION debe tener una ejecucion ESTRICTAMENTE ANTERIOR
    #      con el MISMO sha y la MISMA clase. Si alguna no la tiene, el clasificador esta roto.
    #   B2 (ejemplo)    debe existir al menos un par real de la misma clase sobre SHAs distintos, y
    #      ninguno de sus miembros puede estar etiquetado como reconfirmacion.
    $ordered = @($AllExecutions | Sort-Object timestamp)
    $b1Violations = [System.Collections.Generic.List[string]]::new()
    foreach ($e in @($ordered | Where-Object { $_.classification -eq 'EXACT_SHA_RECONFIRMATION' })) {
        $prior = @($ordered | Where-Object {
                $_.sha -eq $e.sha -and $_.evidenceClass -eq $e.evidenceClass -and $_.timestamp -lt $e.timestamp
            })
        if ($prior.Count -eq 0) { $b1Violations.Add("$($e.source) sobre $($e.sha) sin ejecucion previa de la misma clase") }
    }

    $b2Example = $null
    $b2Violations = [System.Collections.Generic.List[string]]::new()
    foreach ($grp in @($ordered | Where-Object { $_.sha } | Group-Object evidenceClass)) {
        $distinct = @($grp.Group | Group-Object sha)
        if ($distinct.Count -lt 2) { continue }
        $pair = @($grp.Group | Where-Object { $_.sha -in @($distinct[0].Name, $distinct[1].Name) } | Select-Object -First 2)
        if (-not $b2Example) {
            $b2Example = "clase $($grp.Name): $($pair[0].sha.Substring(0,7)) -> $($pair[1].sha.Substring(0,7)) = $($pair[0].classification) / $($pair[1].classification)"
        }
        foreach ($p in $pair) {
            if ($p.classification -eq 'EXACT_SHA_RECONFIRMATION') {
                $b2Violations.Add("$($p.source): SHA distinto etiquetado como reconfirmacion")
            }
        }
    }

    $bOk = ($b1Violations.Count -eq 0 -and $b2Violations.Count -eq 0)
    $results.Add([pscustomobject]@{
            control = 'B'
            present = ($null -ne $b2Example)
            verdict = $(if (-not $b2Example) { 'NO_EXAMPLE' } elseif ($bOk) { 'PASS' } else { 'FAIL' })
            detail  = [pscustomobject]@{
                b1InvariantViolations = @($b1Violations)
                b2Violations          = @($b2Violations)
                b2Example             = $b2Example
            }
        })

    # C — local -> CI sobre el mismo commit debe ser CROSS_CHANNEL.
    $cc = @($AllExecutions | Where-Object { $_.classification -eq 'CROSS_CHANNEL' })
    $results.Add([pscustomobject]@{
            control = 'C'; present = ($cc.Count -gt 0)
            verdict = $(if ($cc.Count -gt 0) { 'PASS' } else { 'NO_EXAMPLE' })
            detail  = "$($cc.Count) ejecuciones de CI sobre commits con evidencia local declarada, clasificadas CROSS_CHANNEL y NO como reconfirmacion"
        })

    # D — el CI posterior al merge debe ser NEW_EVIDENCE.
    $merges = @($AllExecutions | Where-Object { $_.reason -like 'commit de merge*' })
    $badMerges = @($merges | Where-Object { $_.classification -ne 'NEW_EVIDENCE' })
    $results.Add([pscustomobject]@{
            control = 'D'; present = ($merges.Count -gt 0)
            verdict = $(if ($merges.Count -eq 0) { 'NO_EXAMPLE' } elseif ($badMerges.Count -eq 0) { 'PASS' } else { 'FAIL' })
            detail  = "$($merges.Count) corridas post-merge, todas NEW_EVIDENCE"
        })

    return $results
}

# ---------------------------------------------------------------------------
# Principal
# ---------------------------------------------------------------------------

$repoRoot = Get-RepositoryRoot -StartDirectory $PSScriptRoot
$provenance = Get-Provenance -RepositoryRoot $repoRoot

if (-not $CiCachePath) { $CiCachePath = Join-Path $repoRoot 'artifacts/validation/ci-runs-cache.json' }

Write-Host ''
Write-Host 'RackCad - telemetria de repeticion de validacion (I-45, G3)' -ForegroundColor Cyan
Write-Host "  sha $($provenance.gitShaShort) en $($provenance.gitBranch)  |  arbol $(if ($provenance.workingTreeClean) { 'limpio' } else { 'SUCIO' })"

$ci = Get-CiRuns -RepositoryRoot $repoRoot -CachePath $CiCachePath -Refresh:$RefreshCi
$runsBySha = @{}
foreach ($r in $ci.runs) {
    if (-not $runsBySha.ContainsKey($r.headSha)) { $runsBySha[$r.headSha] = @() }
    $runsBySha[$r.headSha] += $r
}
Write-Host "  CI: $($ci.runs.Count) corridas, $($runsBySha.Keys.Count) SHAs distintos (API declara $($ci.apiTotalCount))"

$claims = @()
if ($ClaimsPath) {
    if (-not (Test-Path -LiteralPath $ClaimsPath)) { throw "No existe el JSON de afirmaciones '$ClaimsPath'." }
    $claims = @((Get-Content -LiteralPath $ClaimsPath -Raw | ConvertFrom-Json).claims)
    Write-Host "  afirmaciones documentales: $($claims.Count)"
}
else {
    Add-Note 'Sin -ClaimsPath: los canales local y del dueno quedan NO OBSERVADOS, no en cero.'
    Write-Host '  AVISO: sin reconstruccion documental; los canales local y del dueno quedan NO OBSERVADOS.' -ForegroundColor Yellow
}

$policyRules = @()
if ($PolicyPath) {
    if (-not (Test-Path -LiteralPath $PolicyPath)) { throw "No existe el JSON de politica '$PolicyPath'." }
    $policyRules = @((Get-Content -LiteralPath $PolicyPath -Raw | ConvertFrom-Json).rules)
    Write-Host "  reglas normativas citables: $($policyRules.Count)"
}
else {
    Add-Note 'Sin -PolicyPath: ninguna ejecucion se clasifica POLICY_REQUIRED. La costumbre no es norma; hace falta una cita vigente.'
}

$byInitiative = foreach ($init in $Corpus) {
    Get-InitiativeReconstruction -Initiative $init -RepositoryRoot $repoRoot -RunsBySha $runsBySha -Claims $claims -PolicyRules $policyRules
}
$byInitiative = @($byInitiative)

$allExecutions = @($byInitiative | ForEach-Object { $_.executions })
$metrics = Get-Metrics -Executions $allExecutions
$controlResults = $(if ($Controls) { Invoke-Controls -RepositoryRoot $repoRoot -RunsBySha $runsBySha -AllExecutions $allExecutions } else { $null })

Write-Host ''
Write-Host '  iniciativa  commits  con CI  sin CI  pushes agrupados  merge con CI  ejecuciones'
foreach ($b in $byInitiative) {
    Write-Host ("  {0,-10} {1,8} {2,7} {3,6} {4,17} {5,13} {6,12}" -f $b.initiative, $b.commitCount, $b.commitsWithCi, $b.commitsWithoutCi, $b.groupedPushes, $(if ($b.mergeHasCiRun) { 'si' } else { 'NO' }), $b.executions.Count)
}

Write-Host ''
Write-Host '  === metricas agregadas ==='
foreach ($p in $metrics.PSObject.Properties) { Write-Host ("  {0,-28} {1}" -f $p.Name, $p.Value) }

if ($controlResults) {
    Write-Host ''
    Write-Host '  === controles falsadores ==='
    foreach ($c in $controlResults) { Write-Host ("  {0}: {1}" -f $c.control, $c.verdict) }
}

$report = [ordered]@{
    schema        = $SchemaName
    schemaVersion = $SchemaVersion
    gate          = 'I-45 G3'
    provenance    = $provenance
    corpus        = @($Corpus)
    ciCapture     = [ordered]@{ capturedUtc = $ci.capturedUtc; apiTotalCount = $ci.apiTotalCount; runsRecovered = $ci.runs.Count; distinctShas = $runsBySha.Keys.Count }
    metrics       = $metrics
    byInitiative  = @($byInitiative | ForEach-Object {
            [ordered]@{
                initiative = $_.initiative; mergeSha = $_.mergeSha; commitCount = $_.commitCount
                commitsWithCi = $_.commitsWithCi; commitsWithoutCi = $_.commitsWithoutCi
                groupedPushes = $_.groupedPushes; mergeHasCiRun = $_.mergeHasCiRun
                crossChannelPairs = @($_.crossChannelPairs)
                metrics = (Get-Metrics -Executions @($_.executions))
            }
        })
    controls      = $controlResults
    executions    = $allExecutions
    notes         = @($script:Notes)
}

if (-not $OutputPath) { $OutputPath = Join-Path $repoRoot 'artifacts/validation/repetition-latest.json' }
$outDir = Split-Path -Parent $OutputPath
if ($outDir -and -not (Test-Path -LiteralPath $outDir)) { New-Item -ItemType Directory -Path $outDir -Force | Out-Null }
$report | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $OutputPath -Encoding utf8

Write-Host ''
Write-Host "  JSON: $OutputPath"
foreach ($n in $script:Notes) { Write-Host "  nota: $n" -ForegroundColor Yellow }
Write-Host ''
exit 0
