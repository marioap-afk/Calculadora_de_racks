#Requires -Version 7.0
<#
  I-64 Smoke-1: guarda en PNG una REGION de la pantalla (paleta o linea de ordenes), para conservar las capturas que pide T3D (seccion
  10: operator/ con capturas de la paleta; seccion 6: solo paleta y linea de ordenes, nunca la barra de titulo). BORRADOR propuesto para
  la ocurrencia bajo OPERATOR_EXCEPTION. Solo lee pixeles de la pantalla; escribe solo OutFile y OutFile.sha256. No toca AutoCAD ni el
  foco. Las coordenadas son pixeles FISICOS de la pantalla: se convierten desde la captura de computer use con su factor de escala.
#>
param(
    [Parameter(Mandatory = $true)][int]$X, [Parameter(Mandatory = $true)][int]$Y,
    [Parameter(Mandatory = $true)][int]$Width, [Parameter(Mandatory = $true)][int]$Height,
    [Parameter(Mandatory = $true)][string]$OutFile
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($Width -le 0 -or $Height -le 0 -or $Width -gt 4000 -or $Height -gt 4000) { Write-Output 'INVALID: tamano de region'; exit 3 }
if (Test-Path -LiteralPath $OutFile) { Write-Output 'INVALID: ya existe (no se sobrescribe)'; exit 3 }
Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::new($Width, $Height)
try {
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    try { $g.CopyFromScreen($X, $Y, 0, 0, [System.Drawing.Size]::new($Width, $Height)) } finally { $g.Dispose() }
    $bmp.Save($OutFile, [System.Drawing.Imaging.ImageFormat]::Png)
} finally { $bmp.Dispose() }
$h = (Get-FileHash -LiteralPath $OutFile -Algorithm SHA256).Hash
[System.IO.File]::WriteAllText("$OutFile.sha256", "$h  $(Split-Path $OutFile -Leaf)`n", [System.Text.UTF8Encoding]::new($false))
Write-Output ("SAVED {0} {1}x{2} at {3},{4} sha256={5}" -f (Split-Path $OutFile -Leaf), $Width, $Height, $X, $Y, $h)
