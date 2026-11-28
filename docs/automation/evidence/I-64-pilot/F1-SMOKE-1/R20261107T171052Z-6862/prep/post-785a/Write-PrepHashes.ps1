#Requires -Version 7.0
# I-64 preparacion tras 785a. Solo lectura de esta carpeta: disposicion de ventanas final y SHA-256 de cada archivo. Escribe window-layout-final.txt y evidence-hashes.txt.
$here = $PSScriptRoot
$layout = Join-Path $here 'window-layout-final.txt'
& pwsh -NoProfile -File (Join-Path $here 'List-TopWindows.ps1') | Set-Content -LiteralPath $layout -Encoding utf8NoBOM
$out = Join-Path $here 'evidence-hashes.txt'
$skip = @('evidence-hashes.txt', 'return-package.md')
$lines = @("manifiesto de F1-SMOKE-1-RETRY-PREP/post-785a UTC $([DateTime]::UtcNow.ToString('o'))")
Get-ChildItem -LiteralPath $here -File | Where-Object { $_.Name -notin $skip } | Sort-Object Name | ForEach-Object {
    $lines += "{0}  {1}  {2}" -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash, $_.Length, $_.Name
}
$lines | Set-Content -LiteralPath $out -Encoding utf8NoBOM
$lines
"evidence-hashes.txt sha256 " + (Get-FileHash -LiteralPath $out -Algorithm SHA256).Hash
"--- disposicion"
Get-Content -LiteralPath $layout
