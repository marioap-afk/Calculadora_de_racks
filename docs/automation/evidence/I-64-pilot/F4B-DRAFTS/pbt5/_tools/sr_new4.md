### 4.10 El orden de la corrida

1. **La preparación, sin AutoCAD:**
   - el bloque 1 (P-01), con HEAD = Candidato;
   - B1 y B2, en segundo plano, y después B3 (P-02 a P-04);
   - los bloques 2, 3 y 4 (P-05) y el 4b;
   - `emit_pbt5.py --dry` y después `--commit` (la emisión, F4B-E62), que solo añade un commit documental: el DLL de B3 no se vuelve a compilar;
   - el bloque de §4.3 (P-06);
   - el bloque 13 (P-09, las copias para el estilo);
   - P-07 y P-08.
2. **La instancia principal de AutoCAD,** con el bloque 5 y `c01`:
   - c01, con SPB-01 a 05, 07 (a) y 08, el `RCDUMP pre` y los controles;
   - **las copias `eNN` del estilo,** abiertas una a una con el paso de §4.7, solo para el `RCDUMP estilo`;
   - el bloque 14, el recuento;
   - si algún dibujo guarda un estilo, `s01` y `s02` para SPB-07 (a) y (b) (§4.13);
   - c02 a c41, por la tabla de §4.3. Los contextos construidos son c34 a c41 (§4.12);
   - el cierre de la instancia (SPB-50 y el bloque 9).
3. **Las tres inyecciones,** cada una en su AutoCAD propio (§4.11): c42, c43 y c44.
4. **El cierre:** el bloque 10 (SPB-51) y los bloques 2, 3 y 4 (SPB-52).

### 4.11 Las inyecciones (SPB-59 a SPB-61; Q-PBT5-10, «Hacerlas como en Smoke-2»)

Cada inyección va en **un AutoCAD propio**, lanzado con la variable **solo en el entorno de ese proceso**. Ninguna otra instancia de la sesión puede estar viva.

| Paso | Copia | `$n` | Variable | Valor |
|---|---|---|---|---|
| SPB-59 | c42 | el siguiente libre | `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT` | `2` |
| SPB-60 | c43 | el siguiente | `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL` | `before` |
| SPB-61 | c44 | el siguiente | `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL` | `after` |

**Bloque 11. El lanzamiento con la inyección:**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'; $acad = '<acad.exe>'; $dir = '<dir-del-acceso-directo>'
$copyName = 'c42.dwg'; $n = '<n>'; $injName = 'RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT'; $injValue = '2'
foreach ($v in $work, $acad, $dir, $copyName, $n, $injName, $injValue) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
$n = [int]$n
$ok = @{ 'RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT' = @('2'); 'RACKCAD_DEBUG_FAIL_PILOT_UPDATE_SEAL' = @('before', 'after') }
if (-not $ok.ContainsKey($injName) -or $injValue -notin $ok[$injName]) { throw "STOP: inyeccion no prevista" }
if ($copyName -notin 'c42.dwg', 'c43.dwg', 'c44.dwg') { throw "STOP: la copia no es una de las inyecciones" }
if ($acad -match '~') { throw "STOP: la ruta de acad.exe es una ruta corta 8.3" }
if (-not [IO.Path]::IsPathFullyQualified($acad) -or [IO.Path]::GetFullPath($acad) -cne $acad) { throw "STOP: la ruta de acad.exe no es la ruta completa canonica" }
$copy = Join-Path $work "r1\$copyName"
foreach ($p in $work, $acad, $dir, $copy) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
$dbg = @()
foreach ($s in 'User', 'Machine') { $dbg += @([Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' }) }
$dbg += @(Get-ChildItem Env: | Where-Object Name -like 'RACKCAD_DEBUG_*' | ForEach-Object Name)
if ($dbg.Count) { throw ("STOP PBT5-C04 antes de lanzar: " + ($dbg -join ', ')) }
foreach ($f in Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json') {
  $r = Get-Content -LiteralPath $f.FullName -Raw | ConvertFrom-Json
  $q = Get-Process -Id ([int]$r.Id) -ErrorAction SilentlyContinue
  if ($q -and $q.ProcessName -eq 'acad' -and [long]$r.StartTicksUtc -eq $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: sigue viva la instancia $($r.Id) de la sesion" }
}
$rec = Join-Path $work ("acad-session-{0}.json" -f $n)
if (Test-Path -LiteralPath $rec) { throw "STOP: $rec ya existe" }
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = $acad
$psi.Arguments = '/product ACAD /language "en-US" "' + $copy + '"'
$psi.WorkingDirectory = $dir
$psi.UseShellExecute = $false
foreach ($k in @($psi.Environment.Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' })) { throw "STOP PBT5-C04: $k en el entorno heredado" }
$psi.Environment[$injName] = $injValue
$p = [System.Diagnostics.Process]::Start($psi)
[pscustomobject]@{ Id = $p.Id; Path = $psi.FileName; StartTicksUtc = $p.StartTime.ToUniversalTime().Ticks; Copy = $copyName; Injection = "$injName=$injValue" } |
  ConvertTo-Json | Out-File -LiteralPath $rec -Encoding utf8 -NoClobber
$after = @(Get-ChildItem Env: | Where-Object Name -like 'RACKCAD_DEBUG_*' | ForEach-Object Name)
foreach ($s in 'User', 'Machine') { $after += @([Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' }) }
if ($after.Count) { throw ("STOP PBT5-C04: la variable salio del proceso: " + ($after -join ', ')) }
try { $idle = $p.WaitForInputIdle(180000); "WaitForInputIdle: $idle" } catch { "WaitForInputIdle fallo: $($_.Exception.Message). El registro ya esta escrito: seguir con el bloque 6" }
Get-Content -LiteralPath $rec -Raw
exit 0
```

**Después, en esa instancia:**
1. el bloque 6;
2. los diálogos de arranque, sin «Always ignore»;
3. `EXTERNALREFERENCESCLOSE`, `(vl-load-com)`, el `NETLOAD` por expresión con «Load Once», `RACKPANEL` y el bloque 7;
4. el inventario (§4.6);
5. **la preparación:** RACKEDITAR → «Insertar lateral» del último poste → el punto `9000,-1642` (el de [BM] M-6a) → `RCDUMP ins`;
6. **el paso:** seleccionar el rack → «Editar» → «Editar todo» → un frente menos → «Aplicar al borrador» → «Actualizar» → `RCDUMP post`. Se registra el texto literal y la lista de Deshacer;
7. **el cierre:** «Descartar borradores de este documento» → «Descartar» → `QUIT` → «No»;
8. el bloque 12.

**Bloque 12. La instancia de la inyección salió, y la variable no está en ninguna parte:**

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'
if (-not (Test-Path -LiteralPath $work -PathType Container)) { throw "STOP: no existe $work" }
$recFile = Get-ChildItem -LiteralPath $work -Filter 'acad-session-*.json' | Sort-Object { [int]($_.BaseName -replace '\D', '') } | Select-Object -Last 1
if (-not $recFile) { throw "STOP: no hay registro de la instancia" }
$rec = Get-Content -LiteralPath $recFile.FullName -Raw | ConvertFrom-Json
if (-not ($rec.PSObject.Properties.Name -contains 'Injection')) { throw "STOP: el ultimo registro no es el de una inyeccion" }
$q = Get-Process -Id ([int]$rec.Id) -ErrorAction SilentlyContinue
if ($q -and $q.ProcessName -eq 'acad' -and [long]$rec.StartTicksUtc -eq $q.StartTime.ToUniversalTime().Ticks) { throw "STOP: la instancia de la inyeccion sigue viva" }
$dbg = @(Get-ChildItem Env: | Where-Object Name -like 'RACKCAD_DEBUG_*' | ForEach-Object Name)
foreach ($s in 'User', 'Machine') { $dbg += @([Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' }) }
if ($dbg.Count) { throw ("STOP PBT5-C04 despues: " + ($dbg -join ', ')) }
"OK: salio la instancia $($rec.Id) ($($rec.Injection)) y no hay RACKCAD_DEBUG_* en el usuario, la maquina ni el shell"
exit 0
```

### 4.12 Los contextos construidos (SPB-45, 46, 57 y 58; Q-PBT5-4, «Construirlos y rotularlos»)

**Lo común:**
- **cada contexto** va en una copia para el panel y una **gemela** para RACKEDITAR, construidas igual;
- **todo va rotulado** **CONSTRUCTED REPRESENTATIVE TEST CASES**: las etiquetas de `RCDUMP` empiezan por `CONSTRUCTED-`, y las capturas llevan `CONSTRUCTED` en el nombre;
- **todo se teclea** en la línea de órdenes, en trozos de 16 caracteres como máximo;
- `<h…>` son handles y `<…>` nombres, tomados del `RCDUMP ins` de esa copia;
- **la preparación**, igual en las cuatro: RACKEDITAR sobre el rack → «Insertar lateral» del último poste → el punto `9000,-1642` → `RCDUMP CONSTRUCTED-ins`. Es un gesto del producto.

**SPB-45, la referencia en una presentación** (c34, el panel; c35, la gemela):

```lisp
(command "_.-LAYOUT" "_S" (car (layoutlist)))
(command "_.PSPACE")
(command "_.-INSERT" "<nombre-de-la-definicion-del-lateral>" "0,0" "1" "1" "0")
(command "_.MODEL")
```

Después, `RCDUMP CONSTRUCTED-construido`. Las líneas `REF` cuentan las referencias de modelo y de papel, porque `ssget "_X"` las lee todas.

**SPB-46, la referencia dentro de un bloque del usuario** (c36 y c37):

```lisp
(command "_.-BLOCK" "ZZ-CONSTRUCTED-SPB46" "0,0" (handent "<h-de-la-referencia-del-lateral>") "")
(command "_.-INSERT" "ZZ-CONSTRUCTED-SPB46" "0,0" "1" "1" "0")
```

Después, `RCDUMP CONSTRUCTED-construido`: una línea `NEST` con `ZZ-CONSTRUCTED-SPB46` como contenedor.

**SPB-57, la capa bloqueada** (c38 y c39):

```lisp
(command "_.-LAYER" "_LO" "<capa-de-la-referencia-del-lateral>" "")
```

La capa sale de la línea `REF` del lateral en `RCDUMP CONSTRUCTED-ins`. Después, `RCDUMP CONSTRUCTED-construido`.

**SPB-58, el último vínculo** (c40 y c41), sobre un rack sin referencias anidadas (el que [BM] llamó PB E):

```lisp
(command "_.ERASE" (handent "<h-de-otra-referencia-del-rack>") "")
(command "_.-PURGE" "_B" "<nombre-de-esa-definicion>" "_N")
```

- Se repiten las dos órdenes por cada referencia y cada definición del rack que no sea el lateral.
- Después, `RCDUMP CONSTRUCTED-construido`: el rack solo tiene la definición y la referencia del lateral.
- La selección del paso es el propio lateral.

**El paso, en las cuatro:**
- **en el panel:** seleccionar el rack → «Editar» → «Editar todo» → un frente menos (el del lateral) → «Aplicar al borrador» → «Actualizar» → `RCDUMP CONSTRUCTED-post`, y el texto literal;
- **en la gemela:** RACKEDITAR → «- frente» → «Actualizar» → `RCDUMP CONSTRUCTED-post`, y el mensaje de la línea de órdenes;
- **la comparación** de los dos `post`, con las herramientas de [BM], va al registro.

### 4.13 El recuento del estilo de cotas (P-09 y SPB-07; Q-PBT5-2, «Contar antes en otros»)

**Bloque 13. Respaldos y copias de los demás originales de la selección de Smoke-2.** Va después del bloque de §4.3. La lista es local y no lleva D1 ni D2: D1 se cuenta en c01, y D2 ya está contado en [BM].

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = '<raiz-de-trabajo>'; $list = '<lista-local-de-la-seleccion>'
if ([string]::IsNullOrWhiteSpace($list) -or $list -match '[<>]') { throw "STOP: un valor sin rellenar" }
$work = Join-Path $root 'smoke'; $bak = Join-Path $root 'respaldo'; $est = Join-Path $work 'estilo'
foreach ($p in $work, $bak) { if (-not (Test-Path -LiteralPath $p -PathType Container)) { throw "STOP: no existe $p (primero el bloque de 4.3)" } }
if (-not (Test-Path -LiteralPath $list -PathType Leaf)) { throw "STOP: no existe la lista" }
if (Test-Path -LiteralPath $est) { throw "STOP: $est ya existe" }
$orig = @(Get-Content -LiteralPath $list | Where-Object { $_.Trim() })
if ($orig.Count -eq 0 -or $orig.Count -gt 99) { throw "STOP: la lista esta vacia o es demasiado larga" }
New-Item -ItemType Directory -Path $est | Out-Null
$rows = @(); $hs = @{}
for ($i = 0; $i -lt $orig.Count; $i++) {
  $o = $orig[$i].Trim(); $tag = 'e{0:d2}' -f ($i + 1)
  if (-not (Test-Path -LiteralPath $o -PathType Leaf)) { throw "STOP: no existe el original de $tag" }
  $h = (Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash; $hs[$tag] = $h
  $b = Join-Path $bak "$tag-respaldo.dwg"; $c = Join-Path $est "$tag.dwg"
  foreach ($d in $b, $c) { if (Test-Path -LiteralPath $d) { throw "STOP: ya existe $d" } }
  Copy-Item -LiteralPath $o -Destination $b
  if ((Get-FileHash -LiteralPath $b -Algorithm SHA256).Hash -ne $h) { throw "STOP: el respaldo de $tag no es igual a su original" }
  Copy-Item -LiteralPath $o -Destination $c
  if ((Get-FileHash -LiteralPath $c -Algorithm SHA256).Hash -ne $h) { throw "STOP: la copia de $tag no es igual a su original" }
  $rows += [pscustomobject]@{ copia = "$tag.dwg"; sha256 = $h }
}
for ($i = 0; $i -lt $orig.Count; $i++) {
  $tag = 'e{0:d2}' -f ($i + 1)
  if ((Get-FileHash -LiteralPath $orig[$i].Trim() -Algorithm SHA256).Hash -ne $hs[$tag]) { throw "STOP PBT5-C02: cambio el original de $tag" }
}
$rows | ConvertTo-Json | Out-File -LiteralPath (Join-Path $est 'estilo.json') -Encoding utf8 -NoClobber
"OK: $($rows.Count) respaldos y copias para el recuento del estilo"
exit 0
```

**En la instancia principal,** después de c01: cada `eNN` se abre con el paso de §4.7, y la ruta va con `/estilo/eNN.dwg` en lugar de `/r1/cNN.dwg`. En cada una:
1. `EXTERNALREFERENCESCLOSE`;
2. el inventario;
3. `RCDUMP estilo`;
4. y al pasar a la siguiente, `vla-close … :vlax-false`.

En las `eNN` no se hace nada más. El bloque 8 no aplica, porque compara con D1; en su lugar, `Test-Path` del `.dwl` y el hash frente a `estilo.json`.

**Bloque 14. El recuento,** fuera de AutoCAD, con `dimkeys.py` de [BM], sacado byte a byte de los objetos git:

```powershell
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$work = '<carpeta-del-Smoke-PB>'; $main = '<checkout-principal>'; $py = '<python-del-usuario>'
foreach ($v in $main, $py) { if ([string]::IsNullOrWhiteSpace($v) -or $v -match '[<>]') { throw "STOP: un valor sin rellenar" } }
foreach ($p in $work, $main, $py) { if (-not (Test-Path -LiteralPath $p)) { throw "STOP: no existe $p" } }
$rev = '7da361eb:docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/block-m/tools/dimkeys.py'
$tools = Join-Path $work 'tools'; $dk = Join-Path $tools 'dimkeys.py'
if (-not (Test-Path -LiteralPath $tools)) { New-Item -ItemType Directory -Path $tools | Out-Null }
if (Test-Path -LiteralPath $dk) { throw "STOP: $dk ya existe" }
& git -C $main show $rev > $dk; if ($LASTEXITCODE -ne 0) { throw "STOP: git show" }
$want = & git -C $main rev-parse $rev; $got = & git -C $main hash-object $dk
if ($LASTEXITCODE -ne 0 -or $want -ne $got) { throw "STOP: dimkeys.py no es byte a byte el del repositorio" }
$inv = @(Get-ChildItem -LiteralPath (Join-Path $work 'inv') -File | Where-Object { $_.Name -eq 'c01-pre.txt' -or $_.Name -match '^e\d\d-estilo\.txt$' } | Sort-Object Name)
if ($inv.Count -lt 2) { throw "STOP: faltan inventarios (c01-pre y los eNN-estilo)" }
foreach ($f in $inv) {
  "== $($f.Name)"
  & $py -I $dk $f.FullName; if ($LASTEXITCODE -ne 0) { throw "STOP: dimkeys.py en $($f.Name)" }
}
"OK: recuento hecho; se lee en la salida: los sobres Push Back con DimensionStyle no nulo, por dibujo"
exit 0
```

**Después del recuento:**
- **Si algún sobre Push Back guarda un `DimensionStyle` no nulo:** dos copias nuevas de ese original, `s01.dwg` y `s02.dwg`, en `<carpeta-del-Smoke-PB>\r1\`, con un bloque como el 13 (sin sobrescribir, con el hash comprobado).
  - **SPB-07 (a)** va en `s01`: RACKEDITAR → «Actualizar» → el estilo de las cotas.
  - **SPB-07 (b)** va en `s02`: «Editar todo» → «Actualizar».
- **Si ninguno:** SPB-07 queda **PENDIENTE DEL OWNER**, para que decida si acepta el criterio 3 cumplido en parte. No se fabrica nada.

