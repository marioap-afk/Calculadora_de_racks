"""Genera pre-run-tonight-v2.4.md desde la v2.3 con M-34 y M-35, y escribe el diff unificado.

Cada texto viejo tiene que aparecer exactamente una vez; si no, para sin escribir nada. No sobrescribe.
"""
import difflib
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
base = os.path.dirname(here)
SRC, DST, DIFF = "pre-run-tonight-v2.3.md", "pre-run-tonight-v2.4.md", "pre-run-tonight-v2.3-to-v2.4.diff"

PAIRS = [
    ("# I-64 F4B PB-T5 — La corrida previa a la emisión de esta noche, bajo la orden directa del Owner (v2.3)\n",
     "# I-64 F4B PB-T5 — La corrida previa a la emisión de esta noche, bajo la orden directa del Owner (v2.4)\n"),
    ("## Cambios respecto de la v2.2\n",
     "## Cambios respecto de la v2.3\n\n"
     "De la «Recomprobación 4» de [IR1]:\n"
     "- **M-34:** en los bloques 3, 8 y 10, cada condición de parada que se imprimía como texto pasa a un `throw \"STOP …\"`. Lo informativo se sigue imprimiendo, y el `throw` va al final, con todas las condiciones que fallen.\n"
     "- **M-35:** el bloque 5 para si la ruta de `acad.exe` es una ruta corta 8.3 (lleva `~`) o no es la ruta completa canónica.\n\n"
     "## Cambios respecto de la v2.2\n"),
    # M-34, bloque 3
    ("""  if ($props -and ($props.PSObject.Properties.Name -contains 'PICKFIRST')) { "PICKFIRST $($props.PICKFIRST) en $($k.Name)" }
}
exit 0
""",
     """  if ($props -and ($props.PSObject.Properties.Name -contains 'PICKFIRST')) { "PICKFIRST $($props.PICKFIRST) en $($k.Name)" }
}
if ($hits.Count) { throw ("STOP: hay entradas de RackCad en Applications, Loaded o AssemblyMap: " + ($hits -join '; ')) }
exit 0
"""),
    # M-35, bloque 5
    ("""if ($copyName -notmatch '^c\\d\\d\\.dwg$' -or $n -lt 1) { throw "STOP: la copia o el numero no son validos" }
""",
     """if ($copyName -notmatch '^c\\d\\d\\.dwg$' -or $n -lt 1) { throw "STOP: la copia o el numero no son validos" }
if ($acad -match '~') { throw "STOP: la ruta de acad.exe es una ruta corta 8.3" }
if (-not [IO.Path]::IsPathFullyQualified($acad) -or [IO.Path]::GetFullPath($acad) -cne $acad) { throw "STOP: la ruta de acad.exe no es la ruta completa canonica" }
"""),
    # M-34, bloque 8
    ("""   [pscustomobject]@{
     dwlAnteriorExiste  = Test-Path -LiteralPath (Join-Path $r1 "$prev.dwl")
     anteriorIgualD1    = (Get-FileHash -LiteralPath (Join-Path $r1 "$prev.dwg") -Algorithm SHA256).Hash -eq $known
     dwlSiguienteExiste = Test-Path -LiteralPath (Join-Path $r1 "$next.dwl")
     titulo             = $q.MainWindowTitle
   }
   exit 0
""",
     """   $chk = [pscustomobject]@{
     dwlAnteriorExiste  = Test-Path -LiteralPath (Join-Path $r1 "$prev.dwl")
     anteriorIgualD1    = (Get-FileHash -LiteralPath (Join-Path $r1 "$prev.dwg") -Algorithm SHA256).Hash -eq $known
     dwlSiguienteExiste = Test-Path -LiteralPath (Join-Path $r1 "$next.dwl")
     titulo             = $q.MainWindowTitle
   }
   $chk
   $fail = @()
   if ($chk.dwlAnteriorExiste) { $fail += "la copia anterior $prev sigue abierta (.dwl)" }
   if (-not $chk.anteriorIgualD1) { $fail += "la copia anterior $prev ya no es igual a D1" }
   if (-not $chk.dwlSiguienteExiste) { $fail += "la copia siguiente $next no esta abierta (.dwl)" }
   if ($chk.titulo -notmatch [regex]::Escape("$next.dwg")) { $fail += "el titulo no nombra $next.dwg" }
   if ($fail.Count) { throw ("STOP: " + ($fail -join '; ')) }
   exit 0
"""),
    # M-34, bloque 10
    (""""Original igual: " + ((Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -eq $h0)
"Respaldo igual: " + ((Get-FileHash -LiteralPath $bakFile -Algorithm SHA256).Hash -eq $h0)
foreach ($c in $man.copias) {
  $f = Join-Path $work ("r1\\" + $c.copia); $dwl = [IO.Path]::ChangeExtension($f, '.dwl')
  try { $eq = (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash -eq $h0 } catch { $eq = 'no legible (abierta)' }
  "{0} igual={1} dwl={2}" -f $c.copia, $eq, (Test-Path -LiteralPath $dwl)
}
""",
     """$fail = @()
$origEq = (Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -eq $h0
"Original igual: $origEq"; if (-not $origEq) { $fail += "el original cambio (PBT5-C02)" }
$bakEq = (Get-FileHash -LiteralPath $bakFile -Algorithm SHA256).Hash -eq $h0
"Respaldo igual: $bakEq"; if (-not $bakEq) { $fail += "el respaldo cambio" }
foreach ($c in $man.copias) {
  $f = Join-Path $work ("r1\\" + $c.copia); $dwl = [IO.Path]::ChangeExtension($f, '.dwl')
  if (-not (Test-Path -LiteralPath $f -PathType Leaf)) { $fail += "falta $($c.copia)"; continue }
  try { $eq = (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash -eq $h0 } catch { $eq = 'no legible (abierta)' }
  "{0} igual={1} dwl={2}" -f $c.copia, $eq, (Test-Path -LiteralPath $dwl)
  if ($eq -is [bool] -and -not $eq) { $fail += "$($c.copia) ya no es igual a D1" }
}
"""),
    (""""Worktree limpio: " + (-not $st)
"HEAD = Candidato: " + ((& git -C $wt rev-parse HEAD) -eq $cand)
exit 0
""",
     """$clean = -not $st
"Worktree limpio: $clean"; if (-not $clean) { $fail += "el worktree no esta limpio" }
$headOk = (& git -C $wt rev-parse HEAD) -eq $cand
"HEAD = Candidato: $headOk"; if (-not $headOk) { $fail += "HEAD no es el Candidato" }
if ($fail.Count) { throw ("STOP: " + ($fail -join '; ')) }
exit 0
"""),
]

src = os.path.join(base, SRC)
dst = os.path.join(base, DST)
dpath = os.path.join(base, DIFF)
for p in (dst, dpath):
    if os.path.exists(p):
        sys.exit(f"STOP: {os.path.basename(p)} ya existe")
old = open(src, encoding="utf-8").read()
text = old
for i, (a, b) in enumerate(PAIRS, 1):
    n = text.count(a)
    if n != 1:
        sys.exit(f"STOP: reemplazo {i}: aparece {n} veces: {a[:70]!r}")
    text = text.replace(a, b)
with open(dst, "x", encoding="utf-8", newline="\n") as f:
    f.write(text)
diff = difflib.unified_diff(old.splitlines(keepends=True), text.splitlines(keepends=True),
                            fromfile=SRC, tofile=DST, n=0)
with open(dpath, "x", encoding="utf-8", newline="\n") as f:
    f.writelines(diff)
print(f"{DST} y {DIFF}: {len(PAIRS)} reemplazos")
