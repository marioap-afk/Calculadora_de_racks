#Requires -Version 7.0
<#
  I-64 Smoke-NAV: observador de SOLO LECTURA del panel durante una accion (UI Automation). No acciona nada.
  Anota cada cambio de NavigationResultText y NavigationBlockedText, con milisegundos desde el inicio.
  Uso: Watch-NavText.ps1 -AcadPid <pid> -Seconds <n> -OutFile <txt>
#>
param(
    [Parameter(Mandatory = $true)][int]$AcadPid,
    [int]$Seconds = 10,
    [Parameter(Mandatory = $true)][string]$OutFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
$AE = [System.Windows.Automation.AutomationElement]
$tops = $AE::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.PropertyCondition]::new($AE::ProcessIdProperty, $AcadPid))


function Find-ById([string]$id) {
    $c = [System.Windows.Automation.PropertyCondition]::new($AE::AutomationIdProperty, $id)
    foreach ($w in $tops) {
        $e = $w.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $c)
        if ($null -ne $e) { return $e }
    }
    throw "no se encontro $id"
}


$view = Find-ById 'NavigationViewButton'
$win = $null
foreach ($w in $tops) { if ($null -ne $w.FindFirst([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.PropertyCondition]::new($AE::AutomationIdProperty, 'NavigationViewButton'))) { $win = $w; break } }


function Read-ById([string]$id) {
    $e = $win.FindFirst([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.PropertyCondition]::new($AE::AutomationIdProperty, $id))
    if ($null -eq $e) { return '(ausente)' }
    return $e.Current.Name
}


$sw = [System.Diagnostics.Stopwatch]::StartNew()
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("inicio UTC $([DateTime]::UtcNow.ToString('o'))")
Write-Output 'READY'
$lastR = $null; $lastB = $null; $lastE = $null; $n = 0
while ($sw.Elapsed.TotalSeconds -lt $Seconds) {
    $n++
    $r = Read-ById 'NavigationResultText'; $b = Read-ById 'NavigationBlockedText'; $e = $view.Current.IsEnabled
    if ($r -cne $lastR) { $lines.Add(('{0,9:F1} ms RESULT  [{1}]' -f $sw.Elapsed.TotalMilliseconds, $r)); $lastR = $r }
    if ($b -cne $lastB) { $lines.Add(('{0,9:F1} ms BLOCKED [{1}]' -f $sw.Elapsed.TotalMilliseconds, $b)); $lastB = $b }
    if ($e -ne $lastE) { $lines.Add(('{0,9:F1} ms VerEnabled = {1}' -f $sw.Elapsed.TotalMilliseconds, $e)); $lastE = $e }
}
$lines.Add("muestras $n")
Set-Content -LiteralPath $OutFile -Value $lines -Encoding utf8NoBOM
$lines | ForEach-Object { $_ }
