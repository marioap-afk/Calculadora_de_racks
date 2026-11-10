#Requires -Version 7.0
<#
  I-64 Smoke-NAV SN-09: mide con UI Automation cuanto tarda el panel en mostrar el resultado de una accion de navegacion.
  Solo acciona los botones del panel que el operador ya usa (Ver y Siguiente copia) y lee textos. No toca registro, perfil ni archivos del host.
  Uso: Measure-NavAction.ps1 -OutFile <csv> -AcadPid <pid> -Label <etiqueta> -Warm <n>
  La primera medida es la accion tal como esta (caché fria si el dibujo acaba de abrirse); las siguientes alternan Siguiente copia y Ver.
#>
param(
    [Parameter(Mandatory = $true)][string]$OutFile,
    [Parameter(Mandatory = $true)][int]$AcadPid,
    [Parameter(Mandatory = $true)][string]$Label,
    [int]$Warm = 5,
    [int]$TimeoutMs = 15000
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
$AE = [System.Windows.Automation.AutomationElement]
$PC = [System.Windows.Automation.PropertyCondition]
$root = $AE::RootElement
$procCond = [System.Windows.Automation.PropertyCondition]::new($AE::ProcessIdProperty, $AcadPid)
$tops = $root.FindAll([System.Windows.Automation.TreeScope]::Children, $procCond)


function Find-ById([string]$id) {
    $c = [System.Windows.Automation.PropertyCondition]::new($AE::AutomationIdProperty, $id)
    foreach ($w in $tops) {
        $e = $w.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $c)
        if ($null -ne $e) { return $e }
    }
    throw "no se encontro $id"
}


function Invoke-El($e) { $e.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }


$view = Find-ById 'NavigationViewButton'
$next = Find-ById 'NavigationNextCopyButton'
$text = Find-ById 'NavigationResultText'
$sw = [System.Diagnostics.Stopwatch]::new()


function Measure-One([string]$name) {
    $before = $text.Current.Name
    $sw.Restart()
    Invoke-El $view
    $polls = 0
    while ($sw.ElapsedMilliseconds -lt $TimeoutMs) {
        $polls++
        $now = $text.Current.Name
        if ($now -ne $before -and $now -ne '') {
            $sw.Stop()
            return [pscustomobject]@{ Label = $Label; Name = $name; Ms = [math]::Round($sw.Elapsed.TotalMilliseconds, 1); Polls = $polls; Text = $now }
        }
    }
    throw "tiempo agotado en $name"
}


$rows = [System.Collections.Generic.List[object]]::new()
$rows.Add((Measure-One 'first'))
Start-Sleep -Milliseconds 400
for ($i = 1; $i -le $Warm; $i++) {
    Invoke-El $next
    Start-Sleep -Milliseconds 250
    $rows.Add((Measure-One "warm$i"))
    Start-Sleep -Milliseconds 400
}
$rows | Export-Csv -LiteralPath $OutFile -NoTypeInformation -Encoding utf8NoBOM -Append
$rows | ForEach-Object { '{0} {1} {2} ms polls={3} | {4}' -f $_.Label, $_.Name, $_.Ms, $_.Polls, $_.Text }
