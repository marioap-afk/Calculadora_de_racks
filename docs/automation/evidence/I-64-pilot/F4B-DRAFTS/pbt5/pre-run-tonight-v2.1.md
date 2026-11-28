# I-64 F4B PB-T5 — La corrida previa a la emisión de esta noche, bajo la orden directa del Owner (v2.1)

> **Redacción para el repo público:** las rutas locales van como marcadores. Ningún nombre personal ni de cliente. La hora del Owner, en UTC.

**Cambios respecto de la v2** (la recomprobación de la v2, en «Recomprobación 1 (v2)» de [IR1]):
- **I-07:** el paso de una copia a la siguiente se rehace (§4.7): primero se abre la siguiente, después se cierra la paleta de referencias externas y al final se cierra la anterior. Sin COM. La recuperación, en §4.8.
- **M-18:**
  - ninguna escritura sobrescribe: se para si el destino ya existe;
  - el SHA-256 de D1 se comprueba **antes** del respaldo;
  - las carpetas de trabajo, fuera del worktree y de todo repositorio (§4.3).
- **M-20:** la línea de base de `DBMOD` y de Deshacer se toma **después** de seleccionar y de la vista previa del panel (SPB-04, SPB-15 y SPB-35).
- **M-21:**
  - un control para `rcblk`;
  - `rcnest` dice si el contenedor es anónimo o dinámico, y el análisis de SPB-47 excluye el control positivo.
- **M-22:** la cabecera del registro lleva el sha256 de este documento, del paquete y de la guía redactada.
- **M-25:** SPB-62 a SPB-65, como bloque opcional al final (§5.2), solo si queda tiempo.
- **Las capturas:** dónde van y cómo se redactan antes de custodiarlas (§8).
- **Lo no verificado de la recomprobación:**
  - que la instancia de AutoCAD sobreviva a la orden que la lanza: se comprueba su PID (§4.5);
  - `PICKFIRST`: se lee en P-05;
  - `gh`: se comprueba su versión.

**Marcadores:**
- `<worktree>` y `<checkout-principal>`;
- `<dotnet-del-usuario>`;
- `<acad.exe>` y `<dir-del-acceso-directo>`;
- `<original-D1>`;
- `<carpeta-de-respaldo>` y `<carpeta-del-Smoke-PB>` (con `r1\`, `inv\`, `salidas\`, `screens\raw\` y `screens\crop\`), y su forma con barras normales para el LISP, `<carpeta-del-Smoke-PB-con-/>`;
- `<carpeta-de-registros-de-RackCad>`;
- `<Candidato>`, `<producto>` y `<run>` **[T4e]**;
- `<PID>`, el de la instancia que lanza la sesión.

**Fuentes:**
- `I-64-f4b-pbt5-package-v2.1.md` ([P]): los pasos, con sus orígenes y sus criterios;
- [IR1] §6 y su recomprobación: qué entra esta noche.

---

## 1. Qué es, y qué no es

- **Es una corrida previa a la emisión de PB-T5,** bajo la orden directa del Owner. Hay un precedente: el bloque M de PB-T0 lo operó la sesión por orden del Owner (F4B-E03, pregunta 3), en paralelo con la preparación del contrato, y el contrato fue en el commit siguiente (F4B-E04).
- **No es el Smoke-PB contratado,** y no emite PB-T5.
  - El Owner decide después si la acepta como parte de él, y si cuenta como intento (Q-PBT5-9).
  - Para que pueda contar, tiene que cumplir la regla de reutilización: el mismo SHA del Candidato, la misma versión de AutoCAD y la misma biblioteca de bloques.
- **No decide nada del Owner:** no aplica el valor de ninguna pregunta, no juzga los S+O, no firma y no hace el gate-close.

## 2. La autoridad y la frontera

**La orden del Owner**, literal, con la zona horaria redactada:

> «En caso de que se complete antes de las 6 am y quieras ejecutar el smoke, ejecutalo tú ya que estaré dormido, a menos que sean después de las 7 am de `<zona-horaria-del-Owner>`»

**La frontera** (lectura de la sesión principal):
- **Se empieza solo si la preparación del §3 está cumplida antes de las 13:00 UTC del 2026-11-23.**
- **A las 13:00 UTC,** se para en el siguiente límite de paso y se espera:
  - no se empieza otro paso;
  - AutoCAD y la copia abierta se quedan como estén, sin guardar;
  - se registra el punto exacto.
- **Antes de cada paso,** la hora UTC va al registro.
- **El bloque opcional** (§5.2) solo empieza si quedan al menos 30 minutos para las 13:00 UTC, y se para igual.
- **La franja de 12:00 a 13:00 UTC:** el literal pone como condición «se complete antes de las 6 am», que son las 12:00 UTC. La sesión principal la trata como «antes de las 7:00». Es su lectura, declarada; el Owner la confirma en Q-PBT5-5.

## 3. La preparación (P-01 a P-08, sin la UI Full local)

| Id | Esta noche | Criterio (si no se cumple, no se opera) |
|---|---|---|
| P-01 | **El Candidato = la punta del cierre de PB-T4e [T4e]**, sin commit de emisión de PB-T5. La CI exacta (§4.1) | Árbol limpio; HEAD = Candidato; `git diff <producto> <Candidato> -- src tests assets` vacío; la corrida con `event = push`, la `ref` de la rama, el `head_sha` = Candidato, los cuatro trabajos en `success` y los totales del cierre de PB-T4e. **Si falla una prueba:** PBT5-C15; no se opera |
| P-02 | **Solo** Core Full local y los 19 filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso. **Ninguna suite completa de UI en local** (F4-E59). El archivo de disposición, medido antes y después | Totales iguales a los de la CI. El Candidato queda «pendiente de Q-PBT5-1» |
| P-03 | Los builds Debug de UI y del Plugin, con AutoCAD cerrado | 0 errores y ningún `warning CS`; la `ProductVersion` termina en `+<Candidato>`; los SHA-256. **Si hay un AutoCAD que no abrió la sesión, no se toca y no se compila** (PBT5-C08) |
| P-04 | `catalogs` frente a `assets/catalogs` | Iguales byte a byte |
| P-05 | Variables, bundles, el perfil, `PICKFIRST` (del perfil, en el registro), la biblioteca de bloques, la carpeta de diseños, el registro de RackCad, el archivo de disposición y keep-awake (§4.1) | Ninguna `RACKCAD_DEBUG_*` en el usuario, la máquina ni el shell; ningún bundle; nada de RackCad en `Applications`, `Loaded` ni `AssemblyMap`. Las `Dialogs\RackCad.*`, solo contadas. **`PICKFIRST` = 1;** si vale 0, los pasos con selección por expresión no se hacen, y la variable no se cambia |
| P-06 | Las carpetas, fuera del worktree y de todo repositorio; el SHA-256 de D1 frente al registrado; el respaldo; las 33 copias de §4.4 (§4.3 y §4.4) | Ninguna escritura sobre un archivo que ya exista; el respaldo y las copias = original; el original, igual antes y después |
| P-07 | Ningún Worker ni otra sesión en el worktree; PB-T4e cerrada | Sin delegaciones abiertas |
| P-08 | La guía del operador: los pasos de §5, con los literales copiados del `src/` del Candidato por objetos git, los A/B rotulados y las reglas de las capturas del §8 | Hecha antes de abrir AutoCAD, y su sha256 en el registro |

**Si PB-T4e no cierra a tiempo, o su cierre cambia un texto que cita [P] [T4e]:** se prepara solo lo que no depende del Candidato (la guía, el respaldo, las copias y el texto del inventario), y no se opera.

## 4. Las órdenes exactas de la sesión, en orden

En PowerShell, salvo lo tecleado en AutoCAD. Todas son de lectura, salvo el build, el respaldo y las copias. **Todas las rutas son absolutas.**

### 4.1 Las comprobaciones (P-01, P-05 y P-07)

```powershell
git -C "<worktree>" status --porcelain
git -C "<worktree>" rev-parse HEAD
git -C "<worktree>" diff --stat <producto> <Candidato> -- src tests assets
gh --version
Push-Location "<worktree>"
gh run list --branch architecture/workspace-persistente-rackcad --commit <Candidato> --event push --json databaseId,headSha,event,headBranch,status,conclusion
gh run view <run> --json headSha,event,headBranch,jobs --jq '{headSha,event,headBranch,jobs:[.jobs[]|{name,conclusion}]}'
gh run view <run> --log | Select-String -Pattern 'Passed!|Failed!'
Pop-Location
Get-Process acad, accoreconsole -ErrorAction SilentlyContinue | Select-Object Id, ProcessName, StartTime
'User','Machine' | ForEach-Object { $s = $_; [Environment]::GetEnvironmentVariables($s).Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' } | ForEach-Object { "$s $_" } }
Get-ChildItem Env: | Where-Object Name -like 'RACKCAD_DEBUG_*'
Get-ChildItem "$env:APPDATA\Autodesk\ApplicationPlugins", "$env:ProgramData\Autodesk\ApplicationPlugins" -Filter '*RackCad*' -ErrorAction SilentlyContinue
$root = 'HKCU:\Software\Autodesk\AutoCAD'
$keys = Get-ChildItem -Path $root -Recurse -ErrorAction SilentlyContinue
$keys | Where-Object { $_.PSChildName -in 'Applications','Loaded','AssemblyMap' } | ForEach-Object {
  $k = $_
  Get-ChildItem -LiteralPath $k.PSPath -ErrorAction SilentlyContinue | Where-Object PSChildName -match 'RackCad' | ForEach-Object { "SUBKEY $($_.Name)" }
  $props = Get-ItemProperty -LiteralPath $k.PSPath -ErrorAction SilentlyContinue
  if ($props) { $props.PSObject.Properties | Where-Object { $_.Name -notlike 'PS*' -and ($_.Name -match 'RackCad' -or "$($_.Value)" -match 'RackCad') } | ForEach-Object { "VALUE $($k.Name)\$($_.Name)" } }
}
($keys | Where-Object { $_.Name -match '\\Dialogs\\RackCad' }).Count
$keys | Where-Object { $_.PSChildName -eq 'General' } | ForEach-Object { $v = (Get-ItemProperty -LiteralPath $_.PSPath -ErrorAction SilentlyContinue).PICKFIRST; if ($null -ne $v) { "PICKFIRST $v en $($_.Name)" } }
$s = Join-Path $env:APPDATA 'RackCad\settings.json'
$cfg = if (Test-Path -LiteralPath $s) { Get-Content -LiteralPath $s -Raw | ConvertFrom-Json } else { $null }
$d = $cfg.DesignLibraryPath; if ([string]::IsNullOrWhiteSpace($d)) { $d = Join-Path $env:APPDATA 'RackCad\Designs' }
Test-Path -LiteralPath $d
$lay = Join-Path $env:LOCALAPPDATA 'RackCad\RackFrameConfigurator.layout'
if (Test-Path -LiteralPath $lay) { (Get-FileHash -LiteralPath $lay -Algorithm SHA256).Hash; (Get-Item -LiteralPath $lay).LastWriteTimeUtc }
Get-ChildItem "<carpeta-de-registros-de-RackCad>" -Filter ("rackcad-{0:yyyyMMdd}.log" -f (Get-Date).ToUniversalTime()) -ErrorAction SilentlyContinue | Select-Object Name, Length
```

- **`PICKFIRST`:** se lee en el registro, sin cambiarla. Si el perfil no la guarda ahí, se lee en AutoCAD con `(getvar "PICKFIRST")` en SPB-04, antes de cualquier selección.
- **La biblioteca de bloques:** el fragmento de [G] §7.1, tal cual, con `<catalogs>` = `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\catalogs`, después de P-03.

### 4.2 El build (P-02 y P-03), con AutoCAD cerrado

```powershell
if (Get-Process acad, accoreconsole -ErrorAction SilentlyContinue) { throw "STOP PBT5-C08: hay un AutoCAD abierto" }
Push-Location "<worktree>"
& "<dotnet-del-usuario>" --version
& "<dotnet-del-usuario>" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
# Un proceso por filtro: los 19 de UI de RequiredTests en
# docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-RELOAD/coordinator/gate-contract.json, con su --filter literal
& "<dotnet-del-usuario>" test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug --filter "<filtro literal n>"
& "<dotnet-del-usuario>" build src/RackCad.UI/RackCad.UI.csproj -c Debug
& "<dotnet-del-usuario>" build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
Pop-Location
$bin = "<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows"
[System.Diagnostics.FileVersionInfo]::GetVersionInfo("$bin\RackCad.Plugin.dll").ProductVersion
'RackCad.Plugin','RackCad.Application','RackCad.Domain','RackCad.UI' | ForEach-Object { "{0} {1} {2:o}" -f $_, (Get-FileHash -LiteralPath "$bin\$_.dll" -Algorithm SHA256).Hash, (Get-Item -LiteralPath "$bin\$_.dll").LastWriteTimeUtc }
$a = Get-ChildItem "$bin\catalogs" -Recurse -File | ForEach-Object { "{0} {1}" -f $_.FullName.Substring("$bin\catalogs".Length), (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
$b = Get-ChildItem "<worktree>\assets\catalogs" -Recurse -File | ForEach-Object { "{0} {1}" -f $_.FullName.Substring("<worktree>\assets\catalogs".Length), (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash }
Compare-Object $a $b
```

Si la `ProductVersion` no termina en `+<Candidato>`, se vuelve a compilar el Plugin con `--no-incremental` y se mide otra vez. El archivo de disposición se mide otra vez tras los filtros de UI (§4.1).

### 4.3 Las carpetas y el respaldo (P-06; el respaldo va primero)

```powershell
$ErrorActionPreference = 'Stop'
$work = "<carpeta-del-Smoke-PB>"; $bak = "<carpeta-de-respaldo>"
foreach ($dir in $bak, $work) {
  $f = [IO.Path]::GetFullPath($dir)
  foreach ($r in "<worktree>", "<checkout-principal>") {
    if ($f.StartsWith([IO.Path]::GetFullPath($r), [StringComparison]::OrdinalIgnoreCase)) { throw "STOP: $dir esta dentro de un repositorio" }
  }
}
foreach ($dir in $bak, "$work\r1", "$work\inv", "$work\salidas", "$work\screens\raw", "$work\screens\crop") {
  if (-not (Test-Path -LiteralPath $dir)) { New-Item -ItemType Directory -Path $dir | Out-Null }
}
foreach ($dir in $bak, $work) {
  $g = & git -C $dir rev-parse --show-toplevel 2>$null
  if ($LASTEXITCODE -eq 0 -and $g) { throw "STOP: $dir esta dentro de un repositorio git ($g); no se escribe nada en ella" }
}
$o  = "<original-D1>"
$known = '7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5'
$h0 = (Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash
if ($h0 -ne $known) { throw "STOP: D1 no tiene el SHA-256 registrado en F4-SMOKE-2/run/integrity.md" }
$dst = "$bak\D1-respaldo.dwg"
if (Test-Path -LiteralPath $dst) { throw "STOP: el respaldo ya existe" }
Copy-Item -LiteralPath $o -Destination $dst -ErrorAction Stop
if ((Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash -ne $h0) { throw "STOP: el respaldo no es igual al original" }
```

- La comprobación de la carpeta usa `git -C` sobre la propia carpeta, y la sesión no hace ningún `Set-Location`.
- **Si la carpeta existe de una preparación a medias, no se reutiliza en silencio:** se registra y se pregunta.

### 4.4 Las copias (P-06)

```powershell
$copias = 1..33 | ForEach-Object { 'c{0:d2}' -f $_ }
$manifest = "$work\copias.json"
if (Test-Path -LiteralPath $manifest) { throw "STOP: copias.json ya existe" }
foreach ($c in $copias) { if (Test-Path -LiteralPath "$work\r1\$c.dwg") { throw "STOP: $c.dwg ya existe" } }
$m = foreach ($c in $copias) {
  $dst = "$work\r1\$c.dwg"
  Copy-Item -LiteralPath $o -Destination $dst -ErrorAction Stop
  [pscustomobject]@{ copia = "$c.dwg"; sha256 = (Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash; original = $h0 }
}
$m | ConvertTo-Json | Out-File -LiteralPath $manifest -Encoding utf8 -NoClobber
if (($m | Where-Object { $_.sha256 -ne $h0 }).Count -ne 0) { throw "STOP: una copia no es igual al original" }
if ((Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -ne $h0) { throw "STOP PBT5-C02: el original cambio" }
```

**El reparto,** una copia por escenario contaminable:

| Copia | Pasos |
|---|---|
| c01 | SPB-01 a 05, 07 (a) y 08; el `RCDUMP pre`, que sirve a SPB-47, 48 y 56, y los controles de `rcblk` y `rcnest` |
| c02 | SPB-06 |
| c03 | SPB-09 |
| c04 | SPB-10 |
| c05 | SPB-15 primero; después SPB-11 a 14, 16 y 49 |
| c06 | SPB-17 |
| c07 y c08 | SPB-18 (el panel y la gemela) y SPB-23 |
| c09 | SPB-19 |
| c10 | SPB-20 |
| c11 | SPB-21 |
| c12 | SPB-24 a 26 |
| c13 | SPB-27 |
| c14 | SPB-28 |
| c15 | SPB-29 a 32 |
| c16 | SPB-33 |
| c17 y c18 | SPB-34 (el panel y la gemela) |
| c19 | SPB-35 y 36 |
| c20 y c21 | SPB-37 a 39, **CONSTRUCTED REPRESENTATIVE TEST CASES** |
| c22 y c23 | SPB-37 con el frontal B y SPB-40, **CONSTRUCTED** |
| c24 y c25 | SPB-37 y 41, **CONSTRUCTED** |
| c26 | SPB-37 y 42, **CONSTRUCTED** |
| c27 | SPB-44 |
| c28 | SPB-53 |
| c29 | SPB-54 |
| c30 | SPB-55, **CONSTRUCTED** |
| c31 a c33 | El bloque opcional: SPB-62, 63 y 64. Si no se usan, quedan intactas, y SPB-51 lo comprueba |

**Los handles** salen del `RCDUMP pre` de c01, y valen en todas las copias. La guía fija qué rack se usa en cada paso. Para SPB-63 hace falta el rack de D1 con dos laterales enlazadas, una con el sufijo `_1`.

### 4.5 La primera copia: el lanzamiento, la carga y el panel (SPB-01 a SPB-04)

```powershell
$others = @(Get-Process acad -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Id)
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "<acad.exe>"
$psi.Arguments = '/product ACAD /language "en-US" "<carpeta-del-Smoke-PB>\r1\c01.dwg"'
$psi.WorkingDirectory = "<dir-del-acceso-directo>"
$psi.UseShellExecute = $false
if (@($psi.Environment.Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' }).Count) { throw "STOP PBT5-C04" }
$p = [System.Diagnostics.Process]::Start($psi); "PID $($p.Id); otros acad antes: $($others -join ',')"
```

**En la orden siguiente, siempre:**

```powershell
Get-Process -Id <PID> -ErrorAction SilentlyContinue | Select-Object Id, ProcessName, StartTime, MainWindowTitle
```

- **Si la instancia no sigue viva** (la recomprobación no pudo comprobar que sobreviva a la orden que la lanza):
  - se lanza una vez más, con `Start-Process -FilePath "<acad.exe>" -ArgumentList '/product ACAD /language "en-US" "<carpeta-del-Smoke-PB>\r1\c01.dwg"' -WorkingDirectory "<dir-del-acceso-directo>" -PassThru`;
  - antes, se comprueba que el shell no tiene `RACKCAD_DEBUG_*`, que es lo que hereda.
- **Si tampoco sigue viva:** STOP, y se registra.
- **El PID de la sesión** es el único que se toca en toda la corrida. Los de `$others`, y cualquier AutoCAD que aparezca después, no se tocan.

**Los diálogos de arranque:**
- «Proxy Information» → «OK», sin cambiar su opción;
- «References - Not Found Files» → «Ignore unresolved reference files», sin marcar «Always ignore».

**Tecleado en la línea de órdenes,** en trozos de 16 caracteres como máximo y con una captura antes de cada clic:
1. `EXTERNALREFERENCESCLOSE`: D1 abre la paleta de referencias externas al abrirse. **Va antes de cualquier captura que se vaya a custodiar** (§8).
2. `(vl-load-com)`
3. `(command "NETLOAD" "<worktree-con-/>/src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll")`. En el aviso de seguridad, «Load Once»; **nunca «Always Load»**. El aviso muestra la ruta del DLL, así que su captura no se custodia (§8).
4. `RACKCAD`: el menú, sin excepciones, y la ruta de la biblioteca. Se cierra el menú.
5. `RACKPANEL`: «RackCad: panel Workspace mostrado.».

**Los módulos cargados (SPB-02):**

```powershell
(Get-Process -Id <PID>).Modules | Where-Object ModuleName -like 'RackCad.*' | ForEach-Object { "{0} {1}" -f $_.ModuleName, (Get-FileHash -LiteralPath $_.FileName -Algorithm SHA256).Hash }
```

**La línea de base (SPB-04, M-20):**
- primero, seleccionar el rack del paso con la selección por expresión (§4.6) y esperar a la vista previa del panel;
- **después**, `(getvar "DBMOD")` y la captura de la lista de Deshacer;
- en SPB-15 y SPB-35, lo mismo.

Smoke-2 midió que seleccionar un rack de D1 con el panel abierto pone `DBMOD` = 1 por la vista previa (run-log-2, paso 75).

### 4.6 El inventario, tecleado en cada documento al abrirlo y antes de cualquier orden medida

Son las funciones de [BM] (`block-m/tools/lisp_src.txt`), más `rcblk` y `rcnest`, de solo lectura. Van en trozos de 16 caracteres como máximo, con 3 s entre `defun` y líneas de 1024 como máximo.

```lisp
(vl-load-com)
(defun rcj (bn / be x r) (setq be (cdr (assoc 330 (entget (tblobjname "BLOCK" bn)))) x (cdr (assoc 360 (entget be)))) (if x (if (setq r (dictsearch x "RACKCAD_SELECTIVE")) (apply 'strcat (mapcar 'cdr (vl-remove-if-not '(lambda (p) (= (car p) 1)) r))))))
(defun rcn (bn / e n) (setq n 0 e (entnext (tblobjname "BLOCK" bn))) (while e (setq n (1+ n) e (entnext e))) n)
(defun rcref (f / s i e d o en mn mx r) (setq s (ssget "_X" '((0 . "INSERT"))) i 0) (if s (repeat (sslength s) (setq e (ssname s i) i (1+ i) d (entget e) o (vlax-ename->vla-object e) en (vla-get-EffectiveName o)) (if (rcj en) (progn (setq r (vl-catch-all-apply 'vla-getboundingbox (list o 'mn 'mx))) (write-line (strcat "REF\t" (cdr (assoc 5 d)) "\t" en "\t" (vl-princ-to-string (cdr (assoc 10 d))) "\t" (rtos (cdr (assoc 50 d)) 2 6) "\t" (rtos (cdr (assoc 41 d)) 2 6) "\t" (cdr (assoc 8 d)) "\t" (if (vl-catch-all-error-p r) "NA" (strcat (vl-princ-to-string (vlax-safearray->list mn)) " " (vl-princ-to-string (vlax-safearray->list mx))))) f))))))
(defun rcdef (f / b bn j) (setq b (tblnext "BLOCK" T)) (while b (setq bn (cdr (assoc 2 b))) (if (and (= (logand (cdr (assoc 70 b)) 4) 0) (setq j (rcj bn))) (progn (write-line (strcat "DEF\t" bn "\t" (itoa (rcn bn)) "\t" (itoa (strlen j))) f) (write-line j f))) (setq b (tblnext "BLOCK"))))
(defun rcblk (f / b) (setq b (tblnext "BLOCK" T)) (while b (write-line (strcat "BLK\t" (cdr (assoc 2 b)) "\t" (itoa (cdr (assoc 70 b)))) f) (setq b (tblnext "BLOCK"))))
(defun rcdyn (bn / x) (setq x (cdr (assoc 360 (entget (cdr (assoc 330 (entget (tblobjname "BLOCK" bn)))))))) (if (and x (dictsearch x "ACAD_ENHANCEDBLOCK")) "1" "0"))
(defun rcnest (f / b bn e d n) (setq b (tblnext "BLOCK" T)) (while b (setq bn (cdr (assoc 2 b))) (if (= (logand (cdr (assoc 70 b)) 4) 0) (progn (setq e (entnext (tblobjname "BLOCK" bn))) (while e (setq d (entget e)) (if (and (= (cdr (assoc 0 d)) "INSERT") (rcj (setq n (cdr (assoc 2 d))))) (write-line (strcat "NEST\t" bn "\t" (itoa (cdr (assoc 70 b))) "\t" (rcdyn bn) "\t" n "\t" (cdr (assoc 5 d))) f)) (setq e (entnext e))))) (setq b (tblnext "BLOCK"))))
(defun c:RCDUMP (/ tg f) (setq tg (getstring "\nEtiqueta: ") f (open (strcat "<carpeta-del-Smoke-PB-con-/>/inv/" (vl-filename-base (getvar "DWGNAME")) "-" tg ".txt") "w")) (write-line (strcat "CLAYER\t" (getvar "CLAYER")) f) (rcref f) (rcdef f) (rcblk f) (rcnest f) (close f) (princ "\nRCDUMP ok") (princ))
```

**Lo que hace cada parte:**
- **`rcj`, `rcn`, `rcref` y `rcdef`** son las de [BM], byte a byte. Solo cambia la carpeta de `c:RCDUMP`.
- **`rcblk`** lista todas las definiciones con su indicador, para la purga de SPB-18 (f).
- **`rcnest`** lista las referencias de vistas de rack dentro de otras definiciones, con el indicador del contenedor y si es dinámico. Lo dinámico lo da `rcdyn`, que busca `ACAD_ENHANCEDBLOCK` en el diccionario de extensión de su registro: es nueva, de solo lectura y sin probar.
- **La comprobación de que se cargaron:** `(mapcar 'type (list rcj rcn rcref rcdef rcblk rcdyn rcnest c:RCDUMP))`. Ningún `nil`, como el `(type …)` de [BM].

**Los controles, en c01, antes de usar las funciones nuevas (M-21):**
- **`rcblk`:** el `RCDUMP pre` tiene más de 0 líneas `BLK`, y entre ellas todas las definiciones de rack que listan las líneas `DEF`.
- **`rcnest`:** tiene que dar la referencia anidada de la planta dentro del bloque huérfano pegado («A$C…») que encontró [BM].
  - Ese contenedor no es anónimo ni dinámico, así que **no cuenta como caso de SPB-47**.
  - SPB-47 solo cuenta contenedores dinámicos o anónimos `*U…`.
- **Si un control falla,** esa función no se usa, y su paso (SPB-18 (f) o SPB-47) queda LIMITATION.

**Otras órdenes de lectura:**
- `RCDUMP` y la etiqueta;
- `(getvar "DBMOD")`;
- la selección por expresión, `(sssetfirst nil (ssadd (handent "<handle>")))`. Es la forma de Smoke-2 (`sssetfirst` con un conjunto) con `handent` en lugar de `ssget` por nombre, y es equivalente para una referencia;
- en RACKEDITAR, `(handent "<handle>")` en la petición de objeto.

**N-I1:** nada de esto entre una orden medida y sus `U`.

### 4.7 El paso de una copia a la siguiente (I-07)

**El principio:** primero se abre la siguiente, y la anterior se cierra desde ella. AutoCAD nunca se queda sin un documento con el LISP cargado, y no se usa COM, que podría engancharse a un AutoCAD que no abrió la sesión (PBT5-C08). Es lo que hicieron [BM] (`run-log-m.md`, el cambio a M2 y el cierre) y Smoke-2 (run-log-2, pasos 31 y 34).

**Con la copia `cNN` activa y `cMM` como la siguiente:**
1. **El panel:** «Descartar borradores de este documento» → «Descartar». Si no hay borradores en riesgo, se registra el mensaje que dé.
2. **Tecleado:**

   ```lisp
   (vla-activate (vla-open (vla-get-documents (vlax-get-acad-object)) "<carpeta-del-Smoke-PB-con-/>/r1/cMM.dwg"))
   ```

3. **En `cMM`, ya activa, y antes de cualquier captura que se vaya a custodiar:**

   ```text
   EXTERNALREFERENCESCLOSE
   ```

   Se teclea siempre. Si la paleta no está abierta, no hace nada.
4. **La comprobación de que es la copia buena:** `(getvar "DWGNAME")` tiene que dar `"cMM.dwg"`. No se imprime `DWGPREFIX`, que lleva la ruta.
5. **El inventario,** tecleado en `cMM` (§4.6), con la comprobación de los `type`.
6. **El cierre de la anterior, sin guardar, desde `cMM`:**

   ```lisp
   (vla-close (vla-item (vla-get-documents (vlax-get-acad-object)) "cNN.dwg") :vlax-false)
   ```

7. **En PowerShell:**

   ```powershell
   Test-Path -LiteralPath "<carpeta-del-Smoke-PB>\r1\cNN.dwl"
   (Get-FileHash -LiteralPath "<carpeta-del-Smoke-PB>\r1\cNN.dwg" -Algorithm SHA256).Hash -eq '7A868624226EE956CC411CAAA54EA2DE6B95849EEE946995917CFF70FB9F47A5'
   Test-Path -LiteralPath "<carpeta-del-Smoke-PB>\r1\cMM.dwl"
   Get-Process -Id <PID> | Select-Object Id, MainWindowTitle
   ```

   Lo esperado: `False`, `True`, `True`, y el título con `cMM.dwg`.

**Si el paso 6 falla:**
- **un veto de RackCad, por borradores en riesgo:** `(vla-activate (vla-item (vla-get-documents (vlax-get-acad-object)) "cNN.dwg"))` → «Descartar borradores de este documento» → «Descartar» → volver a `cMM` con la misma orden, cambiando el nombre → repetir el paso 6;
- **cualquier otro error:** se registra literal, y `cNN` se deja abierta. Se cierra al final, en SPB-50.

**`NETLOAD` no se repite** en la misma instancia. El panel sigue abierto entre documentos.

### 4.8 Si AutoCAD se queda sin documentos, o falla la apertura

- **No se usa COM.**
- **Si la instancia de la sesión se queda sin documentos** (por ejemplo, por un `CLOSE` del último):
  1. se cierra esa instancia de forma ordinaria, por su PID: `(Get-Process -Id <PID>).CloseMainWindow()`. Sin documentos no hay nada que guardar;
  2. se comprueba que ha salido;
  3. se relanza con la orden de §4.5, con la siguiente copia como argumento;
  4. vuelve a hacer falta `NETLOAD`, `RACKPANEL` y el inventario.
- **Si `vla-open` falla:** se registra, y no se cierra la copia activa.
- **Si sale otro `acad.exe` que no es el de la sesión:** no se toca, y se registra.
- **Si AutoCAD se cuelga o se cierra solo:** PBT5-C12.

## 5. Los bloques y pasos que se ejecutan

### 5.1 Los obligatorios

Es el detalle de [P] §4.2, con lo que pide [IR1] §6 y su recomprobación.

| Bloque | Pasos | Esta noche |
|---|---|---|
| 0 | SPB-01 a 04 | Enteros, con la línea de base de M-20 |
| A | SPB-05 a 10 | Enteros. **SPB-07 (a):** solo el recuento en las copias de D1; si es 0, NOT AVAILABLE, y el criterio queda pendiente del Owner. **SPB-09** con la geometría: la captura de cada vista y las entidades de cada definición en `r1` y `r2` |
| B | SPB-11 a 17 | Enteros. SPB-12 y 13 con las coordenadas y la captura de después. SPB-15 con la línea de base de M-20. SPB-17, solo si existe la carpeta de diseños; si no, su parte de la biblioteca para |
| C | SPB-18 a 21 y 23; SPB-22 se registra NO VERIFICABLE | SPB-18 con la purga (f), si pasó el control de `rcblk`. SPB-19 con las capturas de antes y después. SPB-20 con la captura de cada vista. SPB-23 con el coste de `Equals` |
| D | SPB-24 a 28 | Enteros. SPB-26 y 28 con los campos del sobre que no son del editor. SPB-26 con el tiempo de cada «Sí» |
| E | SPB-29 a 36 | Enteros. SPB-35 con la línea de base de M-20 |
| F | SPB-37 a 42, rotulados **CONSTRUCTED REPRESENTATIVE TEST CASES**; SPB-43 se registra NOT AVAILABLE | SPB-40 con las capturas de antes y después |
| G | SPB-44; el recuento de SPB-47 (`rcnest`, solo contenedores dinámicos o `*U…`) y de SPB-48 (desde el JSON) | Sin construir nada |
| H | SPB-49 | Entero |
| I | SPB-53, 54, 55 (rotulado) y 56 | Lo que no exige construir ni inyectar |
| Z | SPB-50 a 52 | Con el camino de «Descartar», el cierre de §4.7 y las `Dialogs` solo contadas |

**Recuento de lo obligatorio:**

| | Pasos |
|---|---|
| **Total** | **54** |
| Operados | 52 |
| Solo registrados | 2: SPB-22 y SPB-43 |
| S | 45 |
| S+O, PENDIENTE DE REVISIÓN DEL OWNER | 9: SPB-08, 09, 16, 18, 20, 23, 25, 36 y 39 |

**Por bloque:** 0: 4; A: 6; B: 7; C: 6; D: 5; E: 8; F: 7; G: 3; H: 1; Z: 3; I: 4.

### 5.2 El bloque opcional, J (M-25)

**Solo si** quedan al menos 30 minutos para las 13:00 UTC al terminar lo obligatorio, antes del bloque Z. Sin construir nada, y con la misma frontera.

| Id | Copia | Qué se hace |
|---|---|---|
| SPB-62 | c31 | «Editar» desde un lateral → `ERASE` fuera de la referencia de ese lateral (un gesto del usuario, como el de SPB-33) → «Comprobar ahora» |
| SPB-63 | c32 | El rack con dos laterales enlazadas → «Editar» desde una → «Comprobar ahora» |
| SPB-64 | c33 | «Editar» desde un lateral → «Comprobar ahora» y «Volver a leer» |
| SPB-65 | — | Se registra NO VERIFICABLE FÍSICAMENTE |

Son 4 pasos más, todos S: 3 operados y 1 registrado. **Con el bloque J, la corrida llega a 58 pasos, 49 S y 9 S+O.** Si no hay tiempo, los cuatro van a la corrida contratada.

## 6. Lo que NO se hace esta noche

- **Emitir PB-T5,** o adoptar la forma del contrato (Q-PBT5-7 y Q-PBT5-8).
- **La suite completa de UI en local** (F4-E59; Q-PBT5-1).
- **Las inyecciones de Debug,** SPB-59 a 61 (Q-PBT5-10). Ninguna `RACKCAD_DEBUG_*` en ningún sitio.
- **Los contextos construidos,** SPB-45, 46, 57 y 58 (Q-PBT5-4).
- **Fabricar el estilo de cotas,** o contarlo en otros dibujos (Q-PBT5-2).
- **Aplicar el valor de ninguna pregunta.**
- **Los juicios S+O.** Se miden y quedan PENDIENTE DE REVISIÓN DEL OWNER.
- **La disposición, la firma, el gate-close y el cierre de F4B.**
- **Seguir pasadas las 13:00 UTC.**
- **Guardar una copia,** abrir un original, sobrescribir un archivo o tocar un AutoCAD que no abrió la sesión.
- **Usar COM** para abrir o cerrar documentos.
- **Cambiar Trusted Locations, SECURELOAD, «Always Load»,** el registro o una variable de sistema.
- **Custodiar una captura de la paleta de referencias externas, del aviso de NETLOAD o de un diálogo de archivos,** o una captura sin redactar (§8).

## 7. El registro

En `<carpeta-del-Smoke-PB>`, y después en `F4B-SMOKE-PB/prerun/`, redactado.

**La cabecera de `run-log.md` (M-22):**
- «Corrida previa a la emisión de PB-T5, bajo la orden directa del Owner. No es el Smoke-PB contratado. El Owner decide si la acepta como parte de él (Q-PBT5-9)»;
- **el sha256 de este documento, el del paquete y el de la guía redactada,** que son los textos con que se opera cada paso;
- el Candidato y el producto; la versión de AutoCAD; el SHA-256 de la biblioteca de bloques;
- el PID de la instancia de la sesión;
- la hora de inicio frente a las 13:00 UTC.

**Por cada paso:**
- la hora UTC;
- la copia;
- lo hecho;
- lo observado, con los literales;
- el resultado **propuesto**;
- la evidencia: el inventario, las capturas y las coordenadas de los clics en los resultados negativos.

Los S+O llevan **PENDIENTE DE REVISIÓN DEL OWNER**. Los errores del operador (dos repeticiones como máximo) y los incidentes, declarados. Cada cambio de copia queda con las salidas del paso 7 de §4.7.

**Lo demás:**
- `inventories/`: los crudos, en local, y se custodian sus resúmenes con su SHA-256;
- `integrity.md`, `environment-pre.md` y `environment-post.md`;
- `owner-summary.md`, con la sección «Para tu revisión» con los 9 S+O, los LIMITATION, los FAIL y los OBSERVADO con toda su evidencia, R-T4-39 y dónde se paró.

**La custodia:** un commit documental de la sesión, con una entrada en [DISP] y una sección en `I-64-evidence.md`. No emite nada ni cambia el Candidato.

## 8. Las capturas: dónde van y cómo se redactan antes de custodiarlas

El repositorio es público. Las capturas de la herramienta de control de pantalla copian la pantalla real entera. Pueden salir:
- otras ventanas, incluida la de la propia sesión;
- la barra de tareas;
- rutas en diálogos, en avisos y en la línea de órdenes;
- y textos del dibujo con datos del cliente.

**Dónde van:**
- **`<carpeta-del-Smoke-PB>\screens\raw\`:** las capturas tal como se toman, con el nombre `<hora UTC>-<paso>-<antes|despues|n>.png`. **Nunca se custodian.** Se quedan en local, fuera de todo repositorio (§4.3).
- **`<carpeta-del-Smoke-PB>\screens\crop\`:** las versiones para custodiar, hechas siempre desde una copia de la cruda, nunca editando la cruda. Al custodiar, van a `F4B-SMOKE-PB/prerun/screens/`.

**Cómo se redactan:**
1. **Recortar** al área que prueba el paso: el panel, la ventana de RackCad, la línea de órdenes, la lista de Deshacer o la vista del rack. Es lo preferido.
2. **Tapar** con un rectángulo opaco (no un desenfoque) lo que quede dentro del recorte y no se pueda publicar, por ejemplo una línea de la línea de órdenes con una ruta.
3. **Nunca se custodian, ni recortadas:** la paleta de referencias externas; el aviso de seguridad de NETLOAD; los diálogos de archivos (el BOM y «Guardar en biblioteca» de SPB-17), que muestran carpetas; y cualquier captura con otra aplicación.
   - De esos momentos, el registro guarda el texto del botón pulsado y el resultado, no la imagen.
4. **La lista de antes de custodiar,** captura por captura, abriéndola:
   - sin letra de unidad ni ruta;
   - sin nombre de usuario;
   - sin nombre de cliente, de proyecto ni de lámina;
   - sin la barra de tareas ni otras ventanas.

   Los nombres de las copias (`cNN.dwg`) y de los racks (`PB E` y similares) sí pueden salir, como en Smoke-2. Una captura que no pase la lista no se custodia.
5. **`derivation-record.txt`,** como en Smoke-2: por cada captura custodiada, la cruda de la que sale (por su nombre y su SHA-256, sin ruta), el rectángulo del recorte, las zonas tapadas con su motivo, el paso, y el SHA-256 del resultado.
6. **Los A/B y los contextos construidos** llevan `CONSTRUCTED` en el nombre del archivo.
7. **En los S+O,** la captura custodiada tiene que bastar para el juicio del Owner. Si no puede bastar sin dejar ver algo prohibido, el paso queda PENDIENTE DE REVISIÓN DEL OWNER **en directo**, y no se custodia la imagen.

## 9. Cómo se carga el plugin, en resumen

| Regla | Fuente |
|---|---|
| El DLL Debug del worktree, compilado desde el Candidato, con AutoCAD cerrado | [G] §1 y §2; WORKFLOW §6 |
| Nunca se cierra ni se toca un AutoCAD que no abrió la sesión; si hay uno abierto al compilar, no se compila; sin COM | PBT5-C08; I-07 |
| La `ProductVersion` y el SHA-256, antes de NETLOAD | [G] §7.2 |
| NETLOAD del DLL exacto, por expresión, y «Load Once» | [G] §3; [F] §6.3; [BM] |
| Sin tocar Trusted Locations, SECURELOAD, «Always Load», `FILEDIA`, `PICKFIRST` ni otra variable; sin «Always ignore» | [F] §6.3; [S2] |
| `RACKCAD` y la biblioteca; `RACKPANEL` | [G] §3, pasos 4 y 5 |
| Al final, nada de RackCad en `Applications`, `Loaded` ni `AssemblyMap` | SPB-52 |
