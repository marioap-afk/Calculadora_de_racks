param([string]$OutPath, [int]$SessionPid = 99012)
$wt = '<worktree-de-A-14>'
$wtF = $wt.Replace('\', '/')
$names = '^(codex.*\.exe|claude\.exe|node\.exe|git\.exe|pwsh\.exe|powershell\.exe|bash\.exe|dotnet\.exe)$'
$ownerApps = '^(ChatGPT\.exe|Claude\.exe|Codex\.exe)$'
$all = @(Get-CimInstance Win32_Process)
$byPid = @{}; foreach ($p in $all) { $byPid[[int]$p.ProcessId] = $p }
# cadena del comprobador: $PID y sus ancestros, incluido el conhost hijo de esa cadena
$own = New-Object 'System.Collections.Generic.HashSet[int]'
$cur = [int]$PID
while ($byPid.ContainsKey($cur) -and $own.Add($cur)) { $cur = [int]$byPid[$cur].ParentProcessId }
foreach ($p in $all) { if ($p.Name -eq 'conhost.exe' -and $own.Contains([int]$p.ParentProcessId)) { [void]$own.Add([int]$p.ProcessId) } }
function AncestorNames([int]$id) {
  $r = @(); $seen = @{}
  $c = $id
  while ($byPid.ContainsKey($c) -and -not $seen.ContainsKey($c)) {
    $seen[$c] = 1; $par = [int]$byPid[$c].ParentProcessId
    if ($byPid.ContainsKey($par) -and $byPid[$par].CreationDate -le $byPid[$c].CreationDate) { $r += $byPid[$par].Name; $c = $par } else { break }
  }
  return $r
}
# propiedad de la sesion (disposicion P-02 del Coordinator de T3B): la cadena por ParentProcessId, con CreationDate no decreciente, llega al proceso de la sesion
function SessionOwned([int]$id) {
  $seen = @{}; $c = $id
  while ($byPid.ContainsKey($c) -and -not $seen.ContainsKey($c)) {
    $seen[$c] = 1; $par = [int]$byPid[$c].ParentProcessId
    if ($par -eq $SessionPid) { return $true }
    if ($byPid.ContainsKey($par) -and $byPid[$par].CreationDate -le $byPid[$c].CreationDate) { $c = $par } else { return $false }
  }
  return $false
}
$rows = @(); $readableOther = 0
foreach ($p in $all) {
  $id = [int]$p.ProcessId; $cl = $p.CommandLine
  $inList = $p.Name -match $names
  $hitsWt = ($cl -and ($cl.ToLower().Contains($wt.ToLower()) -or $cl.ToLower().Contains($wtF.ToLower())))
  if (-not $inList -and -not $hitsWt) { continue }
  $cls = $null
  if ($own.Contains($id)) { $cls = 'own-tree' }
  elseif ($p.Name -eq 'codex-windows-sandbox-service.exe') { $cls = 'nominal-exclusion' }
  elseif ($cl) { if ($hitsWt) { $cls = 'participant' } else { $readableOther++; continue } }
  else {
    $anc = AncestorNames $id
    $cls = 'unattributable'
  }
  $rows += [pscustomobject]@{ Pid = $id; ParentPid = [int]$p.ParentProcessId; Name = $p.Name; CreationDateUtc = $p.CreationDate.ToUniversalTime().ToString('o'); Classification = $cls; Reread = $null; SessionOwned = (SessionOwned $id) }
}
$un = @($rows | Where-Object { $_.Classification -eq 'unattributable' -or $_.Classification -eq 'participant' })
if ($un.Count -gt 0) {
  Start-Sleep -Seconds 2
  foreach ($r in $un) {
    $q = Get-CimInstance Win32_Process -Filter "ProcessId=$($r.Pid)" -ErrorAction SilentlyContinue
    if (-not $q -or $q.CreationDate.ToUniversalTime().ToString('o') -ne $r.CreationDateUtc) { $r.Reread = 'GONE' } else { $r.Reread = 'PRESENT' }
  }
}
$res = [pscustomobject]@{
  CheckedUtc = (Get-Date).ToUniversalTime().ToString('o'); CheckerPid = $PID
  Rows = $rows; ReadableNonParticipants = $readableOther
  Participants = @($rows | Where-Object { $_.Classification -eq 'participant' }).Count
  ParticipantsSessionOwned = @($rows | Where-Object { $_.Classification -eq 'participant' -and $_.SessionOwned }).Count
  ParticipantsForeign = @($rows | Where-Object { $_.Classification -eq 'participant' -and -not $_.SessionOwned }).Count
  SessionPid = $SessionPid
  UnattributablePersistent = @($rows | Where-Object { $_.Classification -eq 'unattributable' -and $_.Reread -eq 'PRESENT' }).Count
  UnattributableCleared = @($rows | Where-Object { $_.Classification -eq 'unattributable' -and $_.Reread -eq 'GONE' }).Count
}
$json = $res | ConvertTo-Json -Depth 5
if ($OutPath) { [IO.File]::WriteAllText($OutPath, $json + "`n", (New-Object Text.UTF8Encoding($false))) }
$json
