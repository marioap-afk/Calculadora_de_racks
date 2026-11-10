#Requires -Version 7.0
<#
  I-64 Smoke-NAV, diagnostico: acciona un boton del panel por UI Automation (sin raton) y sale.
  Uso: Invoke-NavButton.ps1 -AcadPid <pid> -Id <AutomationId>
#>
param(
    [Parameter(Mandatory = $true)][int]$AcadPid,
    [Parameter(Mandatory = $true)][string]$Id
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes
$AE = [System.Windows.Automation.AutomationElement]
$tops = $AE::RootElement.FindAll([System.Windows.Automation.TreeScope]::Children, [System.Windows.Automation.PropertyCondition]::new($AE::ProcessIdProperty, $AcadPid))
$c = [System.Windows.Automation.PropertyCondition]::new($AE::AutomationIdProperty, $Id)
$sw = [System.Diagnostics.Stopwatch]::StartNew()
$e = $null
foreach ($w in $tops) { $e = $w.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $c); if ($null -ne $e) { break } }
if ($null -eq $e) { throw "no se encontro $Id" }
"encontrado en $($sw.ElapsedMilliseconds) ms; IsEnabled=$($e.Current.IsEnabled)"
$e.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
"invocado UTC $([DateTime]::UtcNow.ToString('o'))"
