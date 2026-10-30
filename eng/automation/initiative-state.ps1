#Requires -Version 7.0

<#
.SYNOPSIS
    Lee, valida y actualiza el estado versionado de una iniciativa (docs/automation/state/<I>.yml).

.DESCRIPTION
    I-62 E1 (A-8). Trabaja sobre la forma minima rackcad-automation-state/v1 de docs/AUTOMATION_PLAN.md seccion 8, en el
    worktree de la iniciativa:

      resume  Solo lectura. Elige el archivo cuyo branch es la rama actual, comprueba contra Git la rama, el remoto,
              el arbol y last_evidence_commit, valida el archivo y muestra un veredicto (CONTINUE, WAIT, DONE o STOP)
              con la accion siguiente.
      check   Solo lectura. La misma validacion, sin veredicto.
      update  Al terminar una tarea: fija next_action y, si se indican, state, gate, current_phase y attempts; pone
              last_evidence_commit = HEAD y deja intactas las demas lineas. -Init crea la forma minima. No hace commit.

    Los veredictos son informativos: no autorizan nada que docs/AUTOMATION_PLAN.md o docs/WORKFLOW.md no autoricen,
    y Git prevalece sobre el archivo. Un archivo invalido no se corrige ni se convierte.

.PARAMETER Command
    resume, check o update.

.PARAMETER Path
    Directorio dentro del worktree. Por defecto, el actual.

.PARAMETER NextAction
    update: la accion siguiente, en una linea.

.PARAMETER State
    update: nuevo state (vocabulario de seccion 8).

.PARAMETER Gate
    update: nuevo gate (vocabulario de seccion 8).

.PARAMETER Phase
    update: nuevo current_phase.

.PARAMETER Attempts
    update: nuevo attempts (entero no negativo).

.PARAMETER Init
    update: crea docs/automation/state/<Initiative>.yml para la rama actual. Exige -Initiative, -ClaimId (el Claim-Id del
    commit de reclamo), -Phase, -State, -Gate y -NextAction; pone attempts = 0 y no inventa ningun valor. Rechaza si ya
    existe ese archivo o un estado para la rama.

.PARAMETER Initiative
    update -Init: identificador de la iniciativa, que tambien nombra el archivo.

.PARAMETER ClaimId
    update -Init: claim_id del reclamo.

.PARAMETER Json
    Escribe el resultado como un objeto JSON (ASCII) en lugar de texto.

.EXAMPLE
    pwsh eng/automation/initiative-state.ps1 resume

.EXAMPLE
    pwsh eng/automation/initiative-state.ps1 update -NextAction "Coordinator: revisar el cierre de G1" -Gate owner-decision
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory, Position = 0)]
    [ValidateSet('resume', 'check', 'update')]
    [string] $Command,

    [string] $Path = (Get-Location).Path,
    [string] $NextAction,
    [string] $State,
    [string] $Gate,
    [string] $Phase,
    [Nullable[int]] $Attempts,
    [switch] $Init,
    [string] $Initiative,
    [string] $ClaimId,
    [switch] $Json
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

$Schema = 'rackcad-automation-state/v1'
$StateDir = 'docs/automation/state'
$Fields = @('initiative', 'branch', 'claim_id', 'current_phase', 'state', 'gate', 'attempts', 'next_action', 'last_evidence_commit')
$States = @('claimed', 'implementing', 'validating', 'ci-failed', 'waiting', 'review-ready', 'integration-ready', 'completed')
$Gates = @('none', 'owner-decision', 'owner-validation', 'autocad', 'plugin-build', 'ci', 'dependency', 'conflict', 'permissions', 'scope')
$ContinueStates = @('claimed', 'implementing', 'validating', 'ci-failed')

function Invoke-Git([string] $Root, [string[]] $GitArgs) {
    $out = & git -C $Root @GitArgs 2>$null
    [pscustomobject]@{ Code = $LASTEXITCODE; Text = (@($out) -join "`n").Trim() }
}

function ConvertFrom-YamlScalar([string] $Raw) {
    $v = $Raw.Trim()
    if ($v.StartsWith('"')) {
        try { return [System.Text.Json.JsonDocument]::Parse($v, [System.Text.Json.JsonDocumentOptions]::new()).RootElement.GetString() }
        catch { throw "valor entre comillas dobles ilegible: $v" }
    }
    if ($v.StartsWith("'")) {
        if ($v.Length -lt 2 -or -not $v.EndsWith("'")) { throw "valor entre comillas simples sin cerrar: $v" }
        return $v.Substring(1, $v.Length - 2).Replace("''", "'")
    }
    $hash = $v.IndexOf(' #')
    if ($hash -ge 0) { $v = $v.Substring(0, $hash).TrimEnd() }
    if ($v.Contains(': ') -or $v.EndsWith(':') -or $v -match '^[\[\]{}&*!|>%@`,]' -or $v -match '^- ') {
        throw "valor sin comillas que YAML no admite (usa comillas dobles): $v"
    }
    if ($v -eq '~' -or $v -eq 'null') { return '' }
    return $v
}

# Lee el bloque automation_state: claves de dos espacios hasta la siguiente clave de columna 0. Devuelve las lineas,
# el fin de linea y, por clave, su indice de linea y su valor.
function Read-StateFile([string] $File) {
    $bytes = [System.IO.File]::ReadAllBytes($File)
    $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $text = [System.Text.UTF8Encoding]::new($false, $true).GetString($bytes, $(if ($bom) { 3 } else { 0 }), $bytes.Length - $(if ($bom) { 3 } else { 0 }))
    $eol = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
    $lines = $text.Replace("`r`n", "`n").Split("`n")
    $errors = [System.Collections.Generic.List[string]]::new()
    $map = [ordered]@{}
    $schemaLine = $lines | Where-Object { $_ -match '^schema:' } | Select-Object -First 1
    if ($null -eq $schemaLine -or (ConvertFrom-YamlScalar ($schemaLine -replace '^schema:', '')) -ne $Schema) {
        $errors.Add("schema distinto de $Schema")
    }
    $start = [array]::IndexOf($lines, 'automation_state:')
    if ($start -lt 0) {
        $errors.Add('falta el bloque automation_state')
    }
    else {
        for ($i = $start + 1; $i -lt $lines.Length; $i++) {
            $l = $lines[$i]
            if ($l -match '^\S') { break }
            if ($l -match '^  ([a-z_]+):(?: (.*))?$') {
                $key = $Matches[1]
                if ($map.Contains($key)) { $errors.Add("campo repetido: $key"); continue }
                try { $value = ConvertFrom-YamlScalar ([string]$Matches[2]) } catch { $errors.Add("${key}: $($_.Exception.Message)"); $value = $null }
                $map[$key] = [pscustomobject]@{ Line = $i; Value = $value }
            }
        }
    }
    [pscustomobject]@{ Lines = $lines; Eol = $eol; Bom = $bom; Map = $map; Errors = $errors }
}

function Get-Short([string] $Sha) {
    if ($null -ne $Sha -and $Sha.Length -gt 8) { return $Sha.Substring(0, 8) }
    return $Sha
}

function Get-Value($Parsed, [string] $Key) {
    if ($Parsed.Map.Contains($Key)) { return $Parsed.Map[$Key].Value }
    return $null
}

# Errores de forma y vocabulario de seccion 8 (sin Git).
function Test-StateFields($Parsed, [string] $FileName) {
    $errors = [System.Collections.Generic.List[string]]::new()
    foreach ($e in $Parsed.Errors) { $errors.Add($e) }
    foreach ($f in $Fields) {
        # A value that could not be read was already reported by Read-StateFile.
        if ($Parsed.Map.Contains($f) -and $null -eq $Parsed.Map[$f].Value) { continue }
        $v = Get-Value $Parsed $f
        if ([string]::IsNullOrWhiteSpace($v)) { $errors.Add("campo ausente o vacio: $f") }
    }
    $s = Get-Value $Parsed 'state'
    if (-not [string]::IsNullOrWhiteSpace($s) -and $States -notcontains $s) { $errors.Add("state fuera del vocabulario de AUTOMATION_PLAN seccion 8: $s") }
    $g = Get-Value $Parsed 'gate'
    if (-not [string]::IsNullOrWhiteSpace($g) -and $Gates -notcontains $g) { $errors.Add("gate fuera del vocabulario de AUTOMATION_PLAN seccion 8: $g") }
    $a = Get-Value $Parsed 'attempts'
    if (-not [string]::IsNullOrWhiteSpace($a) -and $a -notmatch '^\d+$') { $errors.Add("attempts no es un entero no negativo: $a") }
    $n = Get-Value $Parsed 'next_action'
    if ($null -ne $n -and ($n.Contains("`n") -or $n.Contains("`r"))) { $errors.Add('next_action ocupa mas de una linea') }
    $c = Get-Value $Parsed 'last_evidence_commit'
    if (-not [string]::IsNullOrWhiteSpace($c) -and $c -notmatch '^[0-9a-f]{40}$') { $errors.Add("last_evidence_commit no es un SHA completo: $c") }
    $i = Get-Value $Parsed 'initiative'
    $expected = [System.IO.Path]::GetFileNameWithoutExtension($FileName)
    if (-not [string]::IsNullOrWhiteSpace($i) -and $i -ne $expected) { $errors.Add("initiative ($i) distinto del nombre del archivo ($expected)") }
    return , $errors
}

function Get-LastHeading([string] $File) {
    if (-not (Test-Path -LiteralPath $File)) { return $null }
    $last = $null
    foreach ($l in [System.IO.File]::ReadLines($File)) { if ($l -match '^## \d+\.') { $last = $l } }
    if ($null -ne $last -and $last.Length -gt 140) { $last = $last.Substring(0, 140) + '...' }
    return $last
}

# Hechos de Git y archivo de la rama actual. Reasons = motivos de STOP.
function Get-Context([string] $Start) {
    $r = [ordered]@{
        Root = $null; Branch = $null; Head = $null; Remote = $null; Clean = $null; StateFile = $null; Parsed = $null
        CommitsSinceEvidence = $null; Reasons = [System.Collections.Generic.List[string]]::new()
    }
    $top = Invoke-Git $Start @('rev-parse', '--show-toplevel')
    if ($top.Code -ne 0) { $r.Reasons.Add("no es un repositorio Git: $Start"); return [pscustomobject]$r }
    $r.Root = $top.Text
    $r.Branch = (Invoke-Git $r.Root @('rev-parse', '--abbrev-ref', 'HEAD')).Text
    $r.Head = (Invoke-Git $r.Root @('rev-parse', 'HEAD')).Text
    if ($r.Branch -eq 'HEAD' -or [string]::IsNullOrWhiteSpace($r.Branch)) { $r.Reasons.Add('HEAD separado: no hay rama'); return [pscustomobject]$r }

    $dir = Join-Path $r.Root $StateDir
    $found = @()
    if (Test-Path -LiteralPath $dir) {
        foreach ($f in Get-ChildItem -LiteralPath $dir -Filter '*.yml' -File | Sort-Object Name) {
            try { $p = Read-StateFile $f.FullName } catch { continue }
            if ((Get-Value $p 'branch') -eq $r.Branch) { $found += [pscustomobject]@{ File = $f; Parsed = $p } }
        }
    }
    if ($found.Count -eq 0) {
        $r.Reasons.Add("ningun $StateDir/*.yml declara branch: $($r.Branch) (update -Init crea uno)")
    }
    elseif ($found.Count -gt 1) {
        $r.Reasons.Add("varios archivos declaran branch: $($r.Branch): " + (($found | ForEach-Object { $_.File.Name }) -join ', '))
    }
    else {
        $r.StateFile = "$StateDir/$($found[0].File.Name)"
        $r.Parsed = $found[0].Parsed
        foreach ($e in (Test-StateFields $r.Parsed $found[0].File.Name)) { $r.Reasons.Add($e) }
    }

    $status = Invoke-Git $r.Root @('--no-optional-locks', 'status', '--porcelain')
    $r.Clean = $status.Code -eq 0 -and $status.Text.Length -eq 0
    if (-not $r.Clean) { $r.Reasons.Add('el arbol tiene cambios sin commit') }
    $ls = Invoke-Git $r.Root @('ls-remote', 'origin', "refs/heads/$($r.Branch)")
    if ($ls.Code -ne 0) { $r.Reasons.Add('no se pudo consultar origin') }
    elseif ($ls.Text.Length -eq 0) { $r.Reasons.Add("no existe origin/$($r.Branch)") }
    else {
        $r.Remote = ($ls.Text -split '\s+')[0]
        if ($r.Remote -ne $r.Head) { $r.Reasons.Add("HEAD ($(Get-Short $r.Head)) distinto de origin/$($r.Branch) ($(Get-Short $r.Remote))") }
    }
    $lec = if ($null -ne $r.Parsed) { Get-Value $r.Parsed 'last_evidence_commit' } else { $null }
    if ($lec -match '^[0-9a-f]{40}$') {
        if ((Invoke-Git $r.Root @('cat-file', '-e', "$lec^{commit}")).Code -ne 0) { $r.Reasons.Add("last_evidence_commit no existe en el repositorio: $lec") }
        elseif ((Invoke-Git $r.Root @('merge-base', '--is-ancestor', $lec, 'HEAD')).Code -ne 0) { $r.Reasons.Add("last_evidence_commit no es antecesor de HEAD: $lec") }
        else { $r.CommitsSinceEvidence = [int](Invoke-Git $r.Root @('rev-list', '--count', "$lec..HEAD")).Text }
    }
    return [pscustomobject]$r
}

function Get-Verdict($Ctx) {
    if ($Ctx.Reasons.Count -gt 0) { return 'STOP' }
    $s = Get-Value $Ctx.Parsed 'state'
    $g = Get-Value $Ctx.Parsed 'gate'
    if ($s -eq 'completed') { return 'DONE' }
    if ($g -ne 'none') { return "WAIT $g" }
    if ($ContinueStates -notcontains $s) { return "WAIT $s" }
    return 'CONTINUE'
}

function Write-Result($Object, [string[]] $TextLines) {
    if ($Json) { $Object | ConvertTo-Json -Depth 4 -EscapeHandling EscapeNonAscii }
    else { $TextLines | ForEach-Object { Write-Output $_ } }
}

function Format-Scalar([string] $Key, [string] $Value) {
    if ($Key -eq 'next_action' -or $Value -notmatch '^[A-Za-z0-9][A-Za-z0-9._/+-]*$') { return (ConvertTo-Json -InputObject $Value -Compress) }
    return $Value
}

function Invoke-Read([bool] $WithVerdict) {
    $ctx = Get-Context $Path
    $p = $ctx.Parsed
    $val = { param($k) if ($null -ne $p) { Get-Value $p $k } else { $null } }
    $initiative = & $val 'initiative'
    $result = [ordered]@{
        Command = $Command; StateFile = $ctx.StateFile; Initiative = $initiative; Branch = $ctx.Branch; Head = $ctx.Head; Remote = $ctx.Remote
        Clean = $ctx.Clean; LastEvidenceCommit = (& $val 'last_evidence_commit'); CommitsSinceEvidence = $ctx.CommitsSinceEvidence
        CurrentPhase = (& $val 'current_phase'); State = (& $val 'state'); Gate = (& $val 'gate'); Attempts = (& $val 'attempts')
        NextAction = (& $val 'next_action'); Reasons = @($ctx.Reasons)
    }
    $decisions = $null; $evidence = $null
    if ($null -ne $ctx.Root -and -not [string]::IsNullOrWhiteSpace($initiative) -and $initiative -notmatch '[\\/]') {
        $decisions = Get-LastHeading (Join-Path $ctx.Root "docs/automation/decisions/$initiative.md")
        $evidence = Get-LastHeading (Join-Path $ctx.Root "docs/automation/evidence/$initiative-evidence.md")
    }
    $result.LastDecision = $decisions
    $result.LastEvidence = $evidence
    $text = [System.Collections.Generic.List[string]]::new()
    $text.Add("Estado:      $($ctx.StateFile)  (iniciativa $initiative)")
    $text.Add("Rama:        $($ctx.Branch)  HEAD $($ctx.Head)  origin $($ctx.Remote)  arbol limpio: $($ctx.Clean)")
    $text.Add("Evidencia:   last_evidence_commit $($result.LastEvidenceCommit)  commits posteriores: $($ctx.CommitsSinceEvidence)")
    $text.Add("Fase:        $($result.CurrentPhase)  state $($result.State)  gate $($result.Gate)  attempts $($result.Attempts)")
    $text.Add("Siguiente:   $($result.NextAction)")
    if ($decisions) { $text.Add("Decisiones:  $decisions") }
    if ($evidence) { $text.Add("Registro:    $evidence") }
    foreach ($reason in $ctx.Reasons) { $text.Add("Motivo:      $reason") }
    if ($WithVerdict) {
        $verdict = Get-Verdict $ctx
        $result.Verdict = $verdict
        $text.Add("VEREDICTO:   $verdict")
        $text.Add('Informativo: no autoriza nada; manda docs/WORKFLOW.md y Git prevalece sobre el archivo.')
        Write-Result $result $text
        exit $(if ($verdict -eq 'STOP') { 2 } else { 0 })
    }
    $result.Valid = $ctx.Reasons.Count -eq 0
    $text.Add("VALIDO:      $($result.Valid)")
    Write-Result $result $text
    exit $(if ($result.Valid) { 0 } else { 1 })
}

function Stop-Update([string[]] $Reasons) {
    $result = [ordered]@{ Command = 'update'; Written = $false; StateFile = $null; Reasons = @($Reasons) }
    Write-Result $result (@('NO ESCRITO') + ($Reasons | ForEach-Object { "Motivo:      $_" }))
    exit 1
}

function Invoke-Update {
    $reasons = [System.Collections.Generic.List[string]]::new()
    if ([string]::IsNullOrWhiteSpace($NextAction)) { $reasons.Add('falta -NextAction') }
    elseif ($NextAction.Contains("`n") -or $NextAction.Contains("`r")) { $reasons.Add('-NextAction ocupa mas de una linea') }
    if ($State -and $States -notcontains $State) { $reasons.Add("state fuera del vocabulario de AUTOMATION_PLAN seccion 8: $State") }
    if ($Gate -and $Gates -notcontains $Gate) { $reasons.Add("gate fuera del vocabulario de AUTOMATION_PLAN seccion 8: $Gate") }
    if ($null -ne $Attempts -and $Attempts -lt 0) { $reasons.Add("attempts no es un entero no negativo: $Attempts") }
    if (-not $Init -and ($Initiative -or $ClaimId)) { $reasons.Add('initiative y claim_id son inmutables: solo se dan con -Init') }

    $ctx = Get-Context $Path
    if ($null -eq $ctx.Root) { Stop-Update (@($reasons) + @($ctx.Reasons)) }
    if (-not $ctx.Clean) { $reasons.Add('el arbol tiene cambios sin commit') }
    if ($null -eq $ctx.Remote -or $ctx.Remote -ne $ctx.Head) { $reasons.Add("HEAD no publicado en origin/$($ctx.Branch)") }

    if ($Init) {
        foreach ($pair in @(@('Initiative', $Initiative), @('ClaimId', $ClaimId), @('Phase', $Phase), @('State', $State), @('Gate', $Gate))) {
            if ([string]::IsNullOrWhiteSpace($pair[1])) { $reasons.Add("-Init exige -$($pair[0])") }
        }
        if ($null -ne $Attempts) { $reasons.Add('-Init pone attempts = 0: no se da -Attempts') }
        if ($Initiative -match '[\\/:*?"<>|\s]') { $reasons.Add("initiative no sirve como nombre de archivo: $Initiative") }
        if ($null -ne $ctx.StateFile -or ($ctx.Reasons | Where-Object { $_ -like 'varios archivos declaran*' })) { $reasons.Add("ya existe un estado para la rama $($ctx.Branch)") }
        $target = if ($Initiative) { Join-Path $ctx.Root "$StateDir/$Initiative.yml" } else { $null }
        if ($target -and (Test-Path -LiteralPath $target)) { $reasons.Add("ya existe $StateDir/$Initiative.yml") }
        if ($reasons.Count -gt 0) { Stop-Update $reasons }
        $values = [ordered]@{
            initiative = $Initiative; branch = $ctx.Branch; claim_id = $ClaimId; current_phase = $Phase; state = $State; gate = $Gate
            attempts = '0'; next_action = $NextAction; last_evidence_commit = $ctx.Head
        }
        $lines = @("schema: $Schema", 'automation_state:') + ($Fields | ForEach-Object { "  ${_}: $(Format-Scalar $_ $values[$_])" })
        New-Item -ItemType Directory -Force -Path (Split-Path $target) | Out-Null
        [System.IO.File]::WriteAllText($target, (($lines -join "`n") + "`n"), [System.Text.UTF8Encoding]::new($false))
        $written = "$StateDir/$Initiative.yml"
    }
    else {
        if ($null -eq $ctx.StateFile) { Stop-Update (@($reasons) + @($ctx.Reasons | Where-Object { $_ -like 'ningun*' -or $_ -like 'varios*' })) }
        $parsed = $ctx.Parsed
        $changes = [ordered]@{ next_action = $NextAction; last_evidence_commit = $ctx.Head }
        if ($State) { $changes.state = $State }
        if ($Gate) { $changes.gate = $Gate }
        if ($Phase) { $changes.current_phase = $Phase }
        if ($null -ne $Attempts) { $changes.attempts = [string]$Attempts }
        foreach ($k in $changes.Keys) { if (-not $parsed.Map.Contains($k)) { $reasons.Add("campo ausente en el archivo, no se inserta: $k") } }
        if ($reasons.Count -gt 0) { Stop-Update $reasons }
        $lines = [string[]]$parsed.Lines.Clone()
        foreach ($k in $changes.Keys) { $lines[$parsed.Map[$k].Line] = "  ${k}: $(Format-Scalar $k $changes[$k])" }
        $candidate = Join-Path ([System.IO.Path]::GetTempPath()) ("initiative-state-" + [guid]::NewGuid().ToString('N') + '.yml')
        try {
            [System.IO.File]::WriteAllText($candidate, ($lines -join "`n"), [System.Text.UTF8Encoding]::new($false))
            $check = Test-StateFields (Read-StateFile $candidate) (Split-Path $ctx.StateFile -Leaf)
        }
        finally { Remove-Item -LiteralPath $candidate -ErrorAction SilentlyContinue }
        if ($check.Count -gt 0) { Stop-Update $check }
        $bytes = [System.Text.UTF8Encoding]::new($false).GetBytes(($lines -join $parsed.Eol))
        if ($parsed.Bom) { $bytes = [byte[]](0xEF, 0xBB, 0xBF) + $bytes }
        [System.IO.File]::WriteAllBytes((Join-Path $ctx.Root $ctx.StateFile), $bytes)
        $written = $ctx.StateFile
    }
    $result = [ordered]@{ Command = 'update'; Written = $true; StateFile = $written; LastEvidenceCommit = $ctx.Head; Reasons = @() }
    Write-Result $result @("ESCRITO:     $written  (last_evidence_commit $($ctx.Head))", 'Falta:       commit y push de ese archivo.')
    exit 0
}

try {
    switch ($Command) {
        'resume' { Invoke-Read $true }
        'check' { Invoke-Read $false }
        'update' { Invoke-Update }
    }
}
catch {
    $reason = "error interno: $($_.Exception.Message)"
    $result = [ordered]@{ Command = $Command; Verdict = 'STOP'; Written = $false; Reasons = @($reason) }
    Write-Result $result @("Motivo:      $reason", 'VEREDICTO:   STOP')
    exit 3
}
