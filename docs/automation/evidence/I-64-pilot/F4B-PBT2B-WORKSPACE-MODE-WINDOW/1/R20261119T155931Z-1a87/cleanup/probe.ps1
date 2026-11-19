$lay = Join-Path $env:<datos-locales-del-usuario> 'RackCad\RackFrameConfigurator.layout'
$out = @()
if (Test-Path $lay) { $i=Get-Item $lay; $out += "layout sha256=$((Get-FileHash $lay -Algorithm SHA256).Hash) mtimeUtc=$($i.LastWriteTimeUtc.ToString('o'))" } else { $out += 'layout ausente' }
$settings = Join-Path <datos-de-aplicacion-del-usuario> 'RackCad\settings.json'
$lib = Join-Path <datos-de-aplicacion-del-usuario> 'RackCad\Designs'
if (Test-Path $settings) { try { $c = (Get-Content $settings -Raw | ConvertFrom-Json).DesignLibraryPath; if ($c -and $c.Trim()) { $lib = $c } } catch {} }
if (Test-Path $lib) {
  $i=Get-Item $lib
  $list = (Get-ChildItem $lib -Recurse -Force | Sort-Object FullName | ForEach-Object { $_.FullName.Substring($lib.Length) + '|' + $_.Length + '|' + $_.LastWriteTimeUtc.Ticks }) -join "`n"
  $bytes=[Text.Encoding]::UTF8.GetBytes($list)
  $sha=[BitConverter]::ToString([Security.Cryptography.SHA256]::Create().ComputeHash($bytes)).Replace('-','')
  $out += "biblioteca mtimeUtc=$($i.LastWriteTimeUtc.ToString('o')) listado-sha256=$sha entradas=$((Get-ChildItem $lib -Recurse -Force | Measure-Object).Count)"
} else { $out += 'biblioteca ausente' }
$tmp = Join-Path ([IO.Path]::GetTempPath()) 'rackcad-pbt2-0001'
$out += "ArchivoTemporal-carpeta-existe=$(Test-Path $tmp)"
$out
