# Instantánea de los procesos codex*.exe (operación 7 del descriptor de codex-cli: confirmar la terminación del árbol lanzado). Informativa.
# Solo registra nombre, PID, PID padre, hora de creación (UTC) y si la línea de órdenes contiene el directorio -C de la sonda; nunca guarda la
# línea de órdenes (puede llevar el texto de la sonda o rutas del perfil del usuario). No lee ~/.codex.
param([Parameter(Mandatory)][string]$Out, [Parameter(Mandatory)][string]$Dir)
$ErrorActionPreference = 'Stop'
$d1 = $Dir.ToLowerInvariant().Replace('/', '\')
$d2 = $d1.Replace('\', '/')
$list = @(Get-CimInstance Win32_Process -Filter "Name LIKE 'codex%'" | ForEach-Object {
    $cl = [string]$_.CommandLine
    $l = $cl.ToLowerInvariant()
    [ordered]@{
        Name                = $_.Name
        ProcessId           = $_.ProcessId
        ParentProcessId     = $_.ParentProcessId
        CreationDateUtc     = $(if ($_.CreationDate) { $_.CreationDate.ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ssZ') } else { $null })
        CommandLineReadable = [bool]$cl
        CmdHasProbeDir      = ($l.Contains($d1) -or $l.Contains($d2))
    }
})
ConvertTo-Json -InputObject $list -Depth 3 | Set-Content -LiteralPath $Out -Encoding utf8NoBOM
