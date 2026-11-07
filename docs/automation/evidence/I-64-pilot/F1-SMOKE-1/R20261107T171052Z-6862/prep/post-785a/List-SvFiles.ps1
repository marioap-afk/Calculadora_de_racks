#Requires -Version 7.0
# I-64 preparacion tras 785a. Solo lectura: autoguardados .sv$ en TEMP, con fecha y tamano, sin nombres (los nombres contienen el nombre del dibujo).
$now = [DateTime]::UtcNow
"UTC $($now.ToString('o')); acad = $(@(Get-Process -Name acad -ErrorAction SilentlyContinue).Count)"
Get-ChildItem -LiteralPath $env:TEMP -File -Filter '*.sv$' -ErrorAction SilentlyContinue | Sort-Object LastWriteTimeUtc | ForEach-Object {
    "lastWriteUtc {0} bytes {1} sha256 {2}" -f $_.LastWriteTimeUtc.ToString('o'), $_.Length, (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash
}
