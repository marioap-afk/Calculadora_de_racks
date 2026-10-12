# run-all.ps1 - ejecuta los experimentos (a)-(f) en procesos hijos, en serie (CPU moderada), y escribe un manifiesto
# con el SHA-256 de cada script y de cada resultado. Comprueba que ninguna salida contiene el nombre de usuario ni correos.
. "$PSScriptRoot\lib\c1c3-lib.ps1"
$exps = @('exp-a-relative-write.ps1', 'exp-b-sibling-write.ps1', 'exp-c-interpreter.ps1', 'exp-d-false-positives.ps1', 'exp-e-limitations.ps1', 'exp-f-canaries-and-negatives.ps1')
$runLog = [System.Collections.Generic.List[object]]::new()
foreach ($e in $exps) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $r = Invoke-Child -Exe $script:PwshExe -ArgList @('-NoProfile', '-NonInteractive', '-File', (Join-Path $PSScriptRoot $e)) -Cwd $PSScriptRoot -TimeoutSec 600
    $sw.Stop()
    $runLog.Add([ordered]@{ Script = $e; Command = "pwsh -NoProfile -NonInteractive -File $e"; Cwd = $PSScriptRoot; ExitCode = $r.ExitCode; DurationMs = $sw.ElapsedMilliseconds; Stdout = @($r.Stdout -split "`r?`n" | Where-Object { $_ } | ForEach-Object { ConvertTo-Sanitized $_ }); Stderr = (ConvertTo-Sanitized $r.Stderr.Trim()) })
}
$files = @(Get-ChildItem -LiteralPath $PSScriptRoot -File -Filter '*.ps1') + @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'lib') -File -Filter '*.ps1') + @(Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'results') -File -Filter 'exp-*.json')
$leak = [System.Collections.Generic.List[string]]::new()
$tokens = @($env:USERNAME, (Get-ShortUserSegment)) | Where-Object { $_ } | Select-Object -Unique
foreach ($f in $files) {
    $txt = [IO.File]::ReadAllText($f.FullName)
    foreach ($t in $tokens) { if ($txt.IndexOf($t, [StringComparison]::OrdinalIgnoreCase) -ge 0) { $leak.Add("$($f.Name): usuario") } }
    if ($txt -match '[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}') { $leak.Add("$($f.Name): correo") }
}
$manifest = [ordered]@{
    Generated = [DateTime]::UtcNow.ToString('o')
    Host = [ordered]@{ Pwsh = $PSVersionTable.PSVersion.ToString(); Git = (& git --version); Os = [Environment]::OSVersion.VersionString; DriveDFileSystem = (Get-Volume -DriveLetter D).FileSystemType }
    Runs = $runLog
    Files = @($files | Sort-Object Name | ForEach-Object { [ordered]@{ Path = [IO.Path]::GetRelativePath($PSScriptRoot, $_.FullName).Replace('\', '/'); Sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(); Bytes = $_.Length } })
    PrivacyScan = [ordered]@{ Tokens = 'nombre de usuario largo y corto (derivados del entorno, no escritos aqui) y patron de correo'; Findings = @($leak) }
}
$sha = Write-JsonResult (Join-Path $PSScriptRoot 'results\run-manifest.json') $manifest
foreach ($r in $runLog) { '{0} exit={1} {2} ms' -f $r.Script, $r.ExitCode, $r.DurationMs; $r.Stdout | ForEach-Object { '   ' + $_ } }
"privacy findings: $($leak.Count)"
"manifest sha256 $sha"
