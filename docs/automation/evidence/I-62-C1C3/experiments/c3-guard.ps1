# c3-guard.ps1 - SIMULACION de un gancho previo a cada orden (C3, variante "pre-command hook").
# No se instala en ninguna sesion real. Lee por stdin un JSON con la forma documentada de los ganchos previos
# a herramientas ({ "tool_name": ..., "tool_input": { "command": ... }, "cwd": ... }) y decide:
#   - bloqueo: codigo de salida 2 y una linea "RACKCAD-GUARD C3_INTERPRETER_BLOCKED rule=<id> ruleset=<sha>" en stderr,
#     y una entrada JSONL en el registro del guardian ($env:RACKCAD_GUARD_LOG);
#   - permiso: codigo 0.
# La semantica "salida 2 = bloqueo, stderr al modelo" es la documentada para los ganchos PreToolUse (INFERENCE: no medida aqui).
param([switch]$Hook)
Set-StrictMode -Version 3

$script:InterpNames = 'python(?:w)?(?:3(?:\.\d+)?)?|py|pip(?:3(?:\.\d+)?)?|idle(?:3(?:\.\d+)?)?'
$script:TextTools = @('grep', 'rg', 'select-string', 'sls', 'findstr', 'echo', 'write-output', 'write-host', 'get-content', 'cat', 'type', 'gc')
$script:RulesetText = "C3-R1 interp-word;C3-R2 encoded-command;C3-R3 dynamic-invocation-warn;names=$script:InterpNames;texttools=$($script:TextTools -join ',')"
$script:RulesetSha = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($script:RulesetText))).ToLowerInvariant()

function Split-Segments([string]$Cmd) {
    # separa por ; && || | y saltos de linea fuera de comillas (aproximado)
    $segs = [System.Collections.Generic.List[string]]::new(); $sb = [Text.StringBuilder]::new(); $q = [char]0
    for ($i = 0; $i -lt $Cmd.Length; $i++) {
        $ch = $Cmd[$i]
        if ($q -ne [char]0) { if ($ch -eq $q) { $q = [char]0 }; [void]$sb.Append($ch); continue }
        if ($ch -eq "'" -or $ch -eq '"') { $q = $ch; [void]$sb.Append($ch); continue }
        if ($ch -eq ';' -or $ch -eq "`n" -or $ch -eq '|' -or ($ch -eq '&' -and $i + 1 -lt $Cmd.Length -and $Cmd[$i + 1] -eq '&')) {
            if ($sb.Length) { $segs.Add($sb.ToString()); [void]$sb.Clear() }
            if (($ch -eq '&' -or $ch -eq '|') -and $i + 1 -lt $Cmd.Length -and $Cmd[$i + 1] -eq $ch) { $i++ }
            continue
        }
        [void]$sb.Append($ch)
    }
    if ($sb.Length) { $segs.Add($sb.ToString()) }
    return @($segs | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}

function Get-C3Decision([string]$Command, [int]$Depth = 0) {
    $hits = [System.Collections.Generic.List[object]]::new()
    $warn = [System.Collections.Generic.List[string]]::new()
    foreach ($seg in (Split-Segments $Command)) {
        $lead = ($seg -replace '^[&.(\s]+', '' -split '\s+')[0].Trim('"', "'").ToLowerInvariant()
        $leadLeaf = ($lead -split '[\\/]')[-1] -replace '\.exe$', ''
        $pattern = "(?i)(?<pre>^|[\s;&|(){}'""``=,])(?<path>(?:[A-Za-z]:)?(?:[^\s;&|'""]*[\\/]))?(?<name>$script:InterpNames)(?:\.exe)?(?=$|[\s;&|)'""``,])"
        foreach ($m in [regex]::Matches($seg, $pattern)) {
            $before = $seg.Substring(0, $m.Index + $m.Groups['pre'].Length)
            $inQuotes = ((@($before.ToCharArray() | Where-Object { $_ -eq "'" }).Count % 2) -eq 1) -or ((@($before.ToCharArray() | Where-Object { $_ -eq '"' }).Count % 2) -eq 1)
            # los argumentos de una herramienta de texto son datos, no ordenes
            if ($script:TextTools -contains $leadLeaf -or ($lead -eq 'git' -and $seg -match '(?i)^git\s+(grep|log)\b')) { continue }
            $hits.Add([ordered]@{ Rule = 'C3-R1'; Segment = $seg; Name = $m.Groups['name'].Value; InQuotes = $inQuotes })
        }
        if ($seg -match '(?i)-e(nc(odedcommand)?)?\s+([A-Za-z0-9+/=]{8,})' -and $Depth -lt 2) {
            try {
                $decoded = [Text.Encoding]::Unicode.GetString([Convert]::FromBase64String($Matches[3]))
                $inner = Get-C3Decision $decoded ($Depth + 1)
                if ($inner.Decision -eq 'BLOCK') { $hits.Add([ordered]@{ Rule = 'C3-R2'; Segment = $seg; Name = 'encoded'; InQuotes = $false }) }
            } catch {}
        }
        if ($seg -match '(?i)(^|\s)&\s*[\(\$]|invoke-expression|(^|\s)iex\s|::Start\(') { $warn.Add('C3-R3 dynamic-invocation: ' + $seg) }
    }
    $decision = if ($hits.Count -gt 0) { 'BLOCK' } else { 'ALLOW' }
    return [ordered]@{ Decision = $decision; Class = $(if ($hits.Count) { 'C3_INTERPRETER_BLOCKED' } else { 'NONE' }); Hits = @($hits); Warnings = @($warn); RulesetSha256 = $script:RulesetSha }
}

if ($Hook) {
    $in = [Console]::In.ReadToEnd() | ConvertFrom-Json
    $cmd = "$($in.tool_input.command)"
    $d = Get-C3Decision $cmd
    $evt = [ordered]@{ Utc = [DateTime]::UtcNow.ToString('o'); Tool = $in.tool_name; Cwd = $in.cwd; Command = $cmd; Decision = $d.Decision; Class = $d.Class; Rules = @($d.Hits | ForEach-Object { $_.Rule }); RulesetSha256 = $d.RulesetSha256 }
    if ($env:RACKCAD_GUARD_LOG) { [IO.File]::AppendAllText($env:RACKCAD_GUARD_LOG, ($evt | ConvertTo-Json -Compress) + "`n") }
    if ($d.Decision -eq 'BLOCK') {
        [Console]::Error.WriteLine("RACKCAD-GUARD C3_INTERPRETER_BLOCKED rule=$(@($d.Hits)[0].Rule) ruleset=$($d.RulesetSha256)")
        exit 2
    }
    exit 0
}
