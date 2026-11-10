#Requires -Version 7.0
<#
  I-64 Smoke-NAV SN-09: mide con UI Automation cuanto tarda el panel en mostrar el resultado de una accion de navegacion.
  Solo acciona los botones del panel que el operador ya usa (Ver y Siguiente copia) y lee textos. No toca registro, perfil ni archivos del host.
  La primera medida es la accion tal como esta (cache fria si el dibujo acaba de abrirse): la deteccion del texto recorre el arbol y es una cota superior.
  Las siguientes alternan Siguiente copia y Ver y leen el texto ya localizado.
  Uso: Measure-NavAction.ps1 -OutFile <csv> -AcadPid <pid> -Label <etiqueta> -Warm <n>
#>
param(
    [Parameter(Mandatory = $true)][string]$OutFile,
    [Parameter(Mandatory = $true)][int]$AcadPid,
    [Parameter(Mandatory = $true)][string]$Label,
    [int]$Warm = 5,
    [int]$TimeoutMs = 20000
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
    return $null
}


function Invoke-El($e) { $e.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke() }


$view = Find-ById 'NavigationViewButton'
$next = Find-ById 'NavigationNextCopyButton'
if ($null -eq $view -or $null -eq $next) { throw 'no se encontraron los botones' }
$sw = [System.Diagnostics.Stopwatch]::new()
$rows = [System.Collections.Generic.List[object]]::new()

# Primera accion: el texto de resultado aun no existe o es de otro rack
$prev = Find-ById 'NavigationResultText'
$before = if ($null -ne $prev) { $prev.Current.Name } else { '' }
$sw.Restart()
Invoke-El $view
$text = $null
while ($sw.ElapsedMilliseconds -lt $TimeoutMs) {
    $text = Find-ById 'NavigationResultText'
    if ($null -ne $text -and $text.Current.Name -ne $before -and $text.Current.Name -ne '') { break }
    $text = $null
}
$sw.Stop()
if ($null -eq $text) { throw 'tiempo agotado en la primera accion' }
$rows.Add([pscustomobject]@{ Label = $Label; Name = 'first-upper-bound'; Ms = [math]::Round($sw.Elapsed.TotalMilliseconds, 1); Text = $text.Current.Name })
Start-Sleep -Milliseconds 400

for ($i = 1; $i -le $Warm; $i++) {
    Invoke-El $next
    Start-Sleep -Milliseconds 300
    $b = $text.Current.Name
    $sw.Restart()
    Invoke-El $view
    while ($sw.ElapsedMilliseconds -lt $TimeoutMs) {
        $n = $text.Current.Name
        if ($n -ne $b -and $n -ne '') { break }
    }
    $sw.Stop()
    $rows.Add([pscustomobject]@{ Label = $Label; Name = "warm$i"; Ms = [math]::Round($sw.Elapsed.TotalMilliseconds, 1); Text = $text.Current.Name })
    Start-Sleep -Milliseconds 400
}
$rows | Export-Csv -LiteralPath $OutFile -NoTypeInformation -Encoding utf8NoBOM -Append
$rows | ForEach-Object { '{0} {1} {2} ms | {3}' -f $_.Label, $_.Name, $_.Ms, $_.Text }
