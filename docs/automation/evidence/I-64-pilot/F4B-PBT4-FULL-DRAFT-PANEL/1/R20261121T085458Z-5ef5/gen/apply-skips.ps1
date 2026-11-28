# I-64 F4B PB-T4b: aplica los 478 Skip literales (la UNICA linea que cambia es la de atributo). Guion del Worker, no se commitea.
# Uso: pwsh -NoProfile -File apply-skips.ps1 -Root <worktree> -Gen <carpeta gen> [-DryRun]
param([Parameter(Mandatory = $true)][string]$Root, [Parameter(Mandatory = $true)][string]$Gen, [switch]$DryRun)
$ErrorActionPreference = 'Stop'
$lat = [Text.Encoding]::GetEncoding(28591)
$utf8 = New-Object Text.UTF8Encoding($false)

$REG = 'registro declarado superseding-characterization.md'
$Q3 = 'decision del Owner owner-pbt4-decisions.md, pregunta 3'
$LIST2 = 'decision del Owner owner-pbt4b-retire-list-decisions.md, pregunta 2'
$BREAKS = 'decision del Owner owner-pbt4b-breaks-decision.md'
$LIST477 = 'decision del Owner owner-pbt4b-list-477-decision.md'
$LIST478 = 'decision del Owner owner-pbt4b-list-478-decision.md'
$T4AMARK = '(PB-T4a) '

function Get-Stem([string]$path) {
    $n = [IO.Path]::GetFileNameWithoutExtension($path)
    if ($n.EndsWith('Tests')) { return $n.Substring(0, $n.Length - 5) }
    return $n
}

# La lista de 478 (columnas: lista, archivo, metodo, lineas, causa)
$listRows = [IO.File]::ReadAllText((Join-Path $Gen 'pbt4b-substitution-list-478.tsv'), $utf8).Split("`n") | Where-Object { $_ -ne '' } | Select-Object -Skip 1
if ($listRows.Count -ne 478) { throw "la lista no tiene 478 filas: $($listRows.Count)" }

# Las de copia a mano, del mapa v5 (clase, metodo, lineas, mapa, destino)
$hand = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($l in ([IO.File]::ReadAllText((Join-Path $Gen 'pbt4b-copy-map-v5.tsv'), $utf8).Split("`n") | Where-Object { $_ -ne '' } | Select-Object -Skip 1)) {
    $c = $l.TrimEnd("`r").Split("`t")
    if ($c[4] -ne 'mecanica') { [void]$hand.Add("tests/RackCad.Tests/Workspace/$($c[0]).cs|$($c[1])") }
}
if ($hand.Count -ne 72) { throw "el mapa no da 72 a mano: $($hand.Count)" }

# {archivo -> {metodo -> motivo}}
$byFile = @{}
foreach ($r in $listRows) {
    $f = $r.TrimEnd("`r").Split("`t")
    $lista = $f[0]; $path = $f[1]; $method = $f[2]; $cause = $f[4]
    $isT4a = $path.StartsWith($T4AMARK)
    if ($isT4a) { $path = $path.Substring($T4AMARK.Length); $method = ($method -split ' \(')[0] }
    $stem = Get-Stem $path
    if ($isT4a) {
        $reason = "I-64 F4B PB-T4b: sustituida por CT-PBT4B-13 de PushBackFullPanelCensusGuardTests ($REG; $LIST2)."
    } elseif ($lista -eq 'PB-T4b C-0') {
        $what = if ($hand.Contains("$path|$method")) { 'su equivalente escrito a mano en' } else { 'su copia en' }
        $reason = "I-64 F4B PB-T4b: sustituida por $what ${stem}Full ($REG; $Q3)."
    } elseif ($lista -eq 'PB-T4b C-5') {
        $dec = if ($cause.Contains('owner-pbt4b-list-478-decision.md')) { $LIST478 } elseif ($cause.Contains('owner-pbt4b-list-477-decision.md')) { $LIST477 } elseif ($cause.Contains('owner-pbt4b-breaks-decision.md')) { $BREAKS } else { $LIST2 }
        $reason = "I-64 F4B PB-T4b: sustituida por su equivalente en ${stem}Full ($REG; $dec)."
    } elseif ($lista -eq 'PB-T4b UI') {
        $reason = "I-64 F4B PB-T4b: sustituida por su copia en ${stem}Full ($REG; $LIST2)."
    } else { throw "lista desconocida: $lista" }
    foreach ($ch in $reason.ToCharArray()) { if ([int]$ch -ge 128) { throw "motivo no ASCII: $reason" } }
    if (-not $byFile.ContainsKey($path)) { $byFile[$path] = [ordered]@{} }
    $dict = $byFile[$path]
    $key = if ($isT4a) { $method + '_*' } else { $method }
    if ($dict.Contains($key)) { throw "metodo repetido en la lista: $path $key" }
    $dict[$key] = $reason
}
if ($byFile.Count -ne 21) { throw "no son 21 archivos: $($byFile.Count)" }

$declRe = [regex]'\bpublic\s+(?:async\s+)?(?:void|Task)\s+(\w+)\s*\('
$attrRe = [regex]'^([ \t]*)\[(Fact|Theory)\]([ \t\r]*)$'
$total = 0
foreach ($path in ($byFile.Keys | Sort-Object)) {
    $full = Join-Path $Root $path
    $bytes = [IO.File]::ReadAllBytes($full)
    $text = $lat.GetString($bytes)
    $lines = $text.Split("`n")
    $reasons = $byFile[$path]
    $done = @{}
    for ($i = 0; $i -lt $lines.Length; $i++) {
        $m = $declRe.Match($lines[$i])
        if (-not $m.Success) { continue }
        $name = $m.Groups[1].Value
        $key = $null
        if ($reasons.Contains($name)) { $key = $name }
        else {
            foreach ($k in $reasons.Keys) { if ($k.EndsWith('_*') -and $name.StartsWith($k.Substring(0, $k.Length - 1))) { $key = $k } }
        }
        if ($null -eq $key) { continue }
        if ($done.ContainsKey($key)) { throw "$path : $name aparece mas de una vez" }
        $j = $i - 1
        while ($j -ge 0 -and -not $attrRe.IsMatch($lines[$j])) {
            $t = $lines[$j].Trim()
            if (-not ($t.StartsWith('[') -or $t -eq '' -or $t.StartsWith('//'))) { throw "$path : $name : linea inesperada entre el atributo y el metodo: $t" }
            if ($t.Contains('Skip')) { throw "$path : $name ya lleva Skip" }
            $j--
            if ($i - $j -gt 40) { throw "$path : $name : sin atributo cerca" }
        }
        $a = $attrRe.Match($lines[$j])
        if (-not $a.Success) { throw "$path : $name : sin [Fact] ni [Theory]" }
        $lines[$j] = $a.Groups[1].Value + '[' + $a.Groups[2].Value + '(Skip = "' + $reasons[$key].Replace('"', '\"') + '")]' + $a.Groups[3].Value
        $done[$key] = $name
    }
    $missing = @($reasons.Keys | Where-Object { -not $done.ContainsKey($_) })
    if ($missing.Count -gt 0) { throw "$path : pruebas de la lista que no estan: $($missing -join ', ')" }
    $total += $done.Count
    Write-Output ("{0} skip={1}" -f $path, $done.Count)
    if (-not $DryRun) {
        [IO.File]::WriteAllBytes($full, $lat.GetBytes(($lines -join "`n")))
    }
}
Write-Output "archivos=$($byFile.Count) skip=$total"
if ($total -ne 478) { throw "no son 478: $total" }
