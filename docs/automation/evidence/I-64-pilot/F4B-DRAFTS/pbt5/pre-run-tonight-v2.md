# I-64 F4B PB-T5 — La corrida previa a la emisión de esta noche, bajo la orden directa del Owner (v2)

> **Redacción para el repo público:** las rutas locales van como marcadores. Ningún nombre personal ni de cliente. La hora del Owner, en UTC.

**Marcadores:**
- `<worktree>`: el worktree de I-64;
- `<dotnet-del-usuario>`: el `dotnet.exe` del usuario, el que resuelve `global.json`;
- `<acad.exe>` y `<dir-del-acceso-directo>`: el ejecutable de AutoCAD 2025 y el directorio de trabajo de su acceso directo;
- `<original-D1>`: el original D1 de Smoke-2 y de [BM];
- `<carpeta-de-respaldo>` y `<carpeta-del-Smoke-PB>` (con `r1\`, `inv\`, `salidas\` y `screens\`);
- `<carpeta-de-registros-de-RackCad>`;
- `<Candidato>` y `<producto>` **[T4e]**.

**Fuentes:**
- `I-64-f4b-pbt5-package-v2.md` ([P]): los pasos, con su detalle, sus orígenes y sus criterios;
- la revisión de la v1 ([IR1]), §6, «Esta noche», que fija qué entra.

Este documento dice qué se ejecuta esta noche, con qué órdenes y en qué orden.

---

## 1. Qué es, y qué no es

- **Es una corrida previa a la emisión de PB-T5,** bajo la orden directa del Owner. Hay un precedente: el bloque M de PB-T0 lo operó la sesión por orden del Owner ([DISP] F4B-E03, pregunta 3), en paralelo con la preparación del contrato ([F] §6.1), y el contrato fue en el commit siguiente (F4B-E04).
- **No es el Smoke-PB contratado,** y no emite PB-T5.
  - El Owner decide después si la acepta como parte de él, y si cuenta como intento (Q-PBT5-9).
  - Para que pueda contar, tiene que cumplir la regla de reutilización: el mismo SHA del Candidato, la misma versión de AutoCAD y la misma biblioteca de bloques ([G] §7; AGENTS).
- **No decide nada del Owner:**
  - no aplica ningún valor por defecto de ninguna pregunta;
  - no juzga los pasos S+O;
  - no firma;
  - no hace el gate-close.

## 2. La autoridad y la frontera

**La orden del Owner**, literal, con la zona horaria redactada:

> «En caso de que se complete antes de las 6 am y quieras ejecutar el smoke, ejecutalo tú ya que estaré dormido, a menos que sean después de las 7 am de `<zona-horaria-del-Owner>`»

**La frontera** (lectura de la sesión principal):
- **Se empieza solo si la preparación del §3 está cumplida antes de las 13:00 UTC del 2026-11-23** (las 7:00 del Owner).
- **A las 13:00 UTC,** la corrida para en el siguiente límite de paso y espera:
  - la sesión no empieza otro paso;
  - deja AutoCAD y la copia abierta como estén, sin guardar;
  - registra el punto exacto;
  - y espera al Owner, aunque no haya escrito.
- **Antes de empezar cada paso,** la sesión lee la hora UTC y la anota en el registro.
- **La franja de 12:00 a 13:00 UTC:** el literal pone como condición «se complete antes de las 6 am», que son las 12:00 UTC. La sesión principal la trata como «antes de las 7:00». Es su lectura, declarada; el Owner la confirma o la corrige en Q-PBT5-5.

## 3. La preparación (P-01 a P-08, sin la UI Full local)

| Id | Esta noche | Criterio (si no se cumple, no se opera) |
|---|---|---|
| P-01 | **El Candidato = la punta del cierre de PB-T4e [T4e]**, sin commit de emisión de PB-T5. Producto = el último commit de `src/` o `tests/` **[T4e]**. La CI exacta, con `event = push` y `ref` de la rama (§4.1) | Árbol limpio; HEAD = Candidato; diff vacío; la corrida de push con los cuatro trabajos en `success` y los totales del cierre de PB-T4e. **Si una prueba falla:** PBT5-C15, no se opera y se avisa |
| P-02 | **Solo** Core Full local y los filtros de UI de los `RequiredTests` del contrato de PB-T4e, cada uno en su proceso. **Ninguna suite completa de UI en local** (F4-E59; Q-PBT5-1). El archivo de disposición del configurador, medido antes y después | Totales iguales a los de la CI. El Candidato queda «pendiente de Q-PBT5-1» |
| P-03 | Los builds Debug de UI y del Plugin, con AutoCAD cerrado | 0 errores y ningún `warning CS`; la `InformationalVersion` identifica el Candidato; los SHA-256. **Si hay un AutoCAD abierto que no abrió la sesión, no se toca y no se compila:** PBT5-C08 |
| P-04 | `catalogs` frente a `assets/catalogs` | Iguales byte a byte |
| P-05 | Variables, bundles, el perfil, la biblioteca de bloques, la carpeta de la biblioteca de diseños, el registro de RackCad, el archivo de disposición y keep-awake (§4.1) | Ninguna `RACKCAD_DEBUG_*` en el usuario, la máquina ni el shell; ningún bundle; nada de RackCad en `Applications`, `Loaded` ni `AssemblyMap`. Las `Dialogs\RackCad.*`, solo contadas |
| P-06 | El respaldo de `<original-D1>` y, después, las 30 copias de §4.4 | Respaldo y copias = original por SHA-256; el original, igual antes y después |
| P-07 | Ningún Worker ni otra sesión en el worktree; PB-T4e cerrada | Sin delegaciones abiertas |
| P-08 | La guía del operador: los pasos de §5, con los literales copiados del `src/` del Candidato por objetos git y los A/B rotulados | Hecha antes de abrir AutoCAD |

**Si PB-T4e no cierra a tiempo, o su cierre cambia un texto que cita [P] [T4e]:** se prepara solo lo que no depende del Candidato (la guía, el respaldo, las copias y el texto del inventario), y **no se opera**.

## 4. Las órdenes exactas de la sesión, en orden

Son en PowerShell, salvo lo tecleado en AutoCAD. Todas son de lectura, salvo el build, el respaldo y las copias.

> **Una desviación declarada del orden del encargo** («el build, las copias, el respaldo»): el respaldo va **antes** que las copias. La condición del encargo («una copia de seguridad antes de usar un dibujo importante») y P-06 de [P] lo piden así. Está en el traspaso.

### 4.1 Las comprobaciones (P-01, P-05 y P-07)

```powershell
git -C "<worktree>" status --porcelain
git -C "<worktree>" rev-parse HEAD
git -C "<worktree>" diff --stat <producto> <Candidato> -- src tests assets
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
$s = Join-Path $env:APPDATA 'RackCad\settings.json'
$cfg = if (Test-Path -LiteralPath $s) { Get-Content -LiteralPath $s -Raw | ConvertFrom-Json } else { $null }
$d = $cfg.DesignLibraryPath; if ([string]::IsNullOrWhiteSpace($d)) { $d = Join-Path $env:APPDATA 'RackCad\Designs' }
Test-Path -LiteralPath $d
$lay = Join-Path $env:LOCALAPPDATA 'RackCad\RackFrameConfigurator.layout'
if (Test-Path -LiteralPath $lay) { (Get-FileHash -LiteralPath $lay -Algorithm SHA256).Hash; (Get-Item -LiteralPath $lay).LastWriteTimeUtc }
Get-ChildItem "<carpeta-de-registros-de-RackCad>" -Filter ("rackcad-{0:yyyyMMdd}.log" -f (Get-Date).ToUniversalTime()) -ErrorAction SilentlyContinue | Select-Object Name, Length
```

**La biblioteca de bloques:** el fragmento de [G] §7.1, tal cual, con `<catalogs>` = `<worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\catalogs` después de P-03.

### 4.2 El build (P-02 y P-03), con AutoCAD cerrado

```powershell
Push-Location "<worktree>"
& "<dotnet-del-usuario>" --version
& "<dotnet-del-usuario>" test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
# Un proceso por filtro: los de UI de RequiredTests en
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

- La `ProductVersion` tiene que terminar en `+<Candidato>`. Si no, se vuelve a compilar el Plugin con `--no-incremental` y se mide otra vez ([P] §5.2).
- El archivo de disposición se mide otra vez después de los filtros de UI (las órdenes de §4.1).

### 4.3 El respaldo (P-06, primero)

```powershell
New-Item -ItemType Directory -Force -Path "<carpeta-de-respaldo>", "<carpeta-del-Smoke-PB>\r1", "<carpeta-del-Smoke-PB>\inv", "<carpeta-del-Smoke-PB>\salidas", "<carpeta-del-Smoke-PB>\screens" | Out-Null
$o  = "<original-D1>"
$h0 = (Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash
Copy-Item -LiteralPath $o -Destination "<carpeta-de-respaldo>\D1-respaldo.dwg"
(Get-FileHash -LiteralPath "<carpeta-de-respaldo>\D1-respaldo.dwg" -Algorithm SHA256).Hash -eq $h0
```

- **`$h0` se compara con el SHA-256 de D1** que registraron Smoke-2 y [BM] (`7A868624…47A5`, `F4-SMOKE-2/run/integrity.md`).
- Si difiere, el original cambió desde entonces: se registra, y se pregunta antes de seguir.

### 4.4 Las copias (P-06)

Cada copia sale del original, nunca de otra copia ni del respaldo.

```powershell
$copias = 'c01','c02','c03','c04','c05','c06','c07','c08','c09','c10','c11','c12','c13','c14','c15',
          'c16','c17','c18','c19','c20','c21','c22','c23','c24','c25','c26','c27','c28','c29','c30'
$m = foreach ($c in $copias) {
  $dst = "<carpeta-del-Smoke-PB>\r1\$c.dwg"
  Copy-Item -LiteralPath $o -Destination $dst
  [pscustomobject]@{ copia = "$c.dwg"; sha256 = (Get-FileHash -LiteralPath $dst -Algorithm SHA256).Hash; original = $h0 }
}
$m | ConvertTo-Json | Set-Content -LiteralPath "<carpeta-del-Smoke-PB>\copias.json" -Encoding utf8
($m | Where-Object { $_.sha256 -ne $h0 }).Count
(Get-FileHash -LiteralPath $o -Algorithm SHA256).Hash -eq $h0
```

**El reparto,** una copia por escenario contaminable:

| Copia | Pasos |
|---|---|
| c01 | SPB-01 a 05, 07 (a) y 08; el `RCDUMP pre`, que sirve de base a SPB-47, 48 y 56 |
| c02 | SPB-06 |
| c03 | SPB-09 |
| c04 | SPB-10 |
| c05 | SPB-15 primero, y luego SPB-11 a 14, 16 y 49 |
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
| c20 y c21 | SPB-37, 38 y 39 (el panel y la gemela), **CONSTRUCTED REPRESENTATIVE TEST CASES** |
| c22 y c23 | SPB-37 con el frontal B y SPB-40 (el panel y la gemela), **CONSTRUCTED** |
| c24 y c25 | SPB-37 y 41 (el panel y la gemela), **CONSTRUCTED** |
| c26 | SPB-37 y 42, **CONSTRUCTED** |
| c27 | SPB-44 |
| c28 | SPB-53 |
| c29 | SPB-54 |
| c30 | SPB-55, **CONSTRUCTED** |

**Los handles de los racks:**
- son los del `RCDUMP pre` de c01, y valen en todas las copias, porque son iguales byte a byte;
- la guía fija cuál se usa en cada paso: un rack de un sentido con planta y lateral para los bloques A a E; el más grande para SPB-23 y SPB-36;
- [BM] usó el que llamó PB E (planta `2F3B9B`), y conviene volver a comprobarlo en el inventario.

### 4.5 La apertura de la primera copia, la carga y el panel (SPB-01 a SPB-04)

```powershell
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "<acad.exe>"
$psi.Arguments = '/product ACAD /language "en-US" "<carpeta-del-Smoke-PB>\r1\c01.dwg"'
$psi.WorkingDirectory = "<dir-del-acceso-directo>"
$psi.UseShellExecute = $false
$bad = @($psi.Environment.Keys | Where-Object { $_ -like 'RACKCAD_DEBUG_*' })
if ($bad.Count) { throw "STOP PBT5-C04" }
$p = [System.Diagnostics.Process]::Start($psi); $p.Id
```

**Es una instancia nueva.** Un AutoCAD que no abrió la sesión no se toca.

**Los diálogos de arranque** ([S2], el relanzamiento):
- «Proxy Information» → «OK», sin cambiar su opción;
- «References - Not Found Files» → «Ignore unresolved reference files», **sin marcar «Always ignore»**.

**Tecleado en la línea de órdenes de AutoCAD,** en trozos de 16 caracteres como máximo, con una captura antes de cada clic:
1. `(vl-load-com)`
2. `(command "NETLOAD" "<worktree-con-/>/src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll")`. Es la carga por expresión de [S2] y [BM].
   - En el aviso de seguridad, «Load Once». **Nunca «Always Load».**
   - `FILEDIA` no se cambia.
3. `RACKCAD`: el menú, sin excepciones, y la ruta de la biblioteca. Se cierra el menú.
4. `RACKPANEL`: «RackCad: panel Workspace mostrado.».

**Los módulos cargados (SPB-02),** en PowerShell:

```powershell
(Get-Process -Id <PID>).Modules | Where-Object ModuleName -like 'RackCad.*' | ForEach-Object { "{0} {1}" -f $_.ModuleName, (Get-FileHash -LiteralPath $_.FileName -Algorithm SHA256).Hash }
```

Tienen que salir los SHA-256 de §4.2. Domain y UI pueden cargarse más tarde; se miden otra vez tras «Editar».

### 4.6 El inventario, tecleado en cada documento al abrirlo y antes de cualquier orden medida

Son las funciones de [BM] (`block-m/tools/lisp_src.txt`), con la carpeta del Smoke-PB, más `rcblk` y `rcnest`, de solo lectura. Van en trozos de 16 caracteres como máximo, con 3 s entre `defun` y líneas de 1024 como máximo. `SECURELOAD` impide `load`, así que se teclean ([BM] §1).

```lisp
(vl-load-com)
(defun rcj (bn / be x r) (setq be (cdr (assoc 330 (entget (tblobjname "BLOCK" bn)))) x (cdr (assoc 360 (entget be)))) (if x (if (setq r (dictsearch x "RACKCAD_SELECTIVE")) (apply 'strcat (mapcar 'cdr (vl-remove-if-not '(lambda (p) (= (car p) 1)) r))))))
(defun rcn (bn / e n) (setq n 0 e (entnext (tblobjname "BLOCK" bn))) (while e (setq n (1+ n) e (entnext e))) n)
(defun rcref (f / s i e d o en mn mx r) (setq s (ssget "_X" '((0 . "INSERT"))) i 0) (if s (repeat (sslength s) (setq e (ssname s i) i (1+ i) d (entget e) o (vlax-ename->vla-object e) en (vla-get-EffectiveName o)) (if (rcj en) (progn (setq r (vl-catch-all-apply 'vla-getboundingbox (list o 'mn 'mx))) (write-line (strcat "REF\t" (cdr (assoc 5 d)) "\t" en "\t" (vl-princ-to-string (cdr (assoc 10 d))) "\t" (rtos (cdr (assoc 50 d)) 2 6) "\t" (rtos (cdr (assoc 41 d)) 2 6) "\t" (cdr (assoc 8 d)) "\t" (if (vl-catch-all-error-p r) "NA" (strcat (vl-princ-to-string (vlax-safearray->list mn)) " " (vl-princ-to-string (vlax-safearray->list mx))))) f))))))
(defun rcdef (f / b bn j) (setq b (tblnext "BLOCK" T)) (while b (setq bn (cdr (assoc 2 b))) (if (and (= (logand (cdr (assoc 70 b)) 4) 0) (setq j (rcj bn))) (progn (write-line (strcat "DEF\t" bn "\t" (itoa (rcn bn)) "\t" (itoa (strlen j))) f) (write-line j f))) (setq b (tblnext "BLOCK"))))
(defun rcblk (f / b) (setq b (tblnext "BLOCK" T)) (while b (write-line (strcat "BLK\t" (cdr (assoc 2 b)) "\t" (itoa (cdr (assoc 70 b)))) f) (setq b (tblnext "BLOCK"))))
(defun rcnest (f / b bn e d n) (setq b (tblnext "BLOCK" T)) (while b (setq bn (cdr (assoc 2 b))) (if (= (logand (cdr (assoc 70 b)) 4) 0) (progn (setq e (entnext (tblobjname "BLOCK" bn))) (while e (setq d (entget e)) (if (and (= (cdr (assoc 0 d)) "INSERT") (rcj (setq n (cdr (assoc 2 d))))) (write-line (strcat "NEST\t" bn "\t" n "\t" (cdr (assoc 5 d))) f)) (setq e (entnext e))))) (setq b (tblnext "BLOCK"))))
(defun c:RCDUMP (/ tg f) (setq tg (getstring "\nEtiqueta: ") f (open (strcat "<carpeta-del-Smoke-PB-con-/>/inv/" (vl-filename-base (getvar "DWGNAME")) "-" tg ".txt") "w")) (write-line (strcat "CLAYER\t" (getvar "CLAYER")) f) (rcref f) (rcdef f) (rcblk f) (rcnest f) (close f) (princ "\nRCDUMP ok") (princ))
```

**Lo que hace cada parte:**
- **`rcj`, `rcn`, `rcref` y `rcdef`** son las de [BM], byte a byte. Solo cambia la carpeta de `c:RCDUMP`.
- **`rcblk`** lista todas las definiciones con su indicador. Sirve para la purga de SPB-18 (f).
- **`rcnest`** lista las referencias de vistas de rack dentro de otras definiciones, también anónimas o dinámicas. Sirve para SPB-47.
- **El control positivo,** en c01 y antes de usarlas: `rcnest` tiene que dar la referencia anidada de la planta que [BM] encontró dentro de un bloque huérfano. Si no la da, las dos funciones nuevas no se usan, y SPB-18 (f) y SPB-47 quedan LIMITATION (R-5).

**Otras órdenes de lectura:**
- `RCDUMP` y la etiqueta;
- `(getvar "DBMOD")`;
- la selección por expresión, `(sssetfirst nil (ssadd (handent "<handle>")))`. Su forma no está copiada de [S2]: se comprueba en c01 antes de usarla;
- en RACKEDITAR, `(handent "<handle>")` en la petición de objeto, como en [BM].

**La regla N-I1:** nada de esto entre una orden medida y sus `U`.

**El análisis es posterior y fuera de AutoCAD:**
- con `inv_analyze.py` y `envdiff.py` de [BM], sobre los `.txt`;
- más la lectura de `SchemaVersion`, el GUID, los metadatos de sesión y la `ExtensionData` de cada sobre desde su JSON (I-04);
- y las líneas `BLK` y `NEST`.

### 4.7 La apertura de cada copia siguiente, y su cierre

**El cierre de la copia anterior** (SPB-50):
1. si el panel tiene borradores en riesgo, «Descartar borradores de este documento» → «Descartar»;
2. después, `CLOSE` → «No».

**La apertura,** tecleada en la línea de órdenes, en la misma instancia:

```lisp
(vla-activate (vla-open (vla-get-documents (vlax-get-acad-object)) "<carpeta-del-Smoke-PB-con-/>/r1/cNN.dwg"))
```

- Se comprueba que el `.dwl` aparece junto a la copia, en `r1\`.
- Si no queda ningún documento abierto, no hay LISP. Entonces se abre por COM (`Documents.Open`), como en el paso 77 de [S2].
- **El inventario se teclea otra vez** en cada documento nuevo (§4.6). NETLOAD no se repite en la misma instancia.

## 5. Los bloques y pasos que se ejecutan

Es el detalle de [P] §4.2, con lo que pide [IR1] §6 y nada más.

| Bloque | Pasos | Esta noche |
|---|---|---|
| 0 | SPB-01 a 04 | Enteros |
| A | SPB-05 a 10 | Enteros. **SPB-07 (a):** solo el recuento en las copias de D1, sin fabricar; si es 0, NOT AVAILABLE, y el criterio queda pendiente del Owner. **SPB-09** con la geometría: captura de cada vista y las entidades de cada definición en `r1` y `r2` (I-05) |
| B | SPB-11 a 17 | Enteros. SPB-12 y 13 con coordenadas y captura de después. SPB-17 con la carpeta de diseños comprobada en P-05: si no existe, la parte de «Guardar en biblioteca» para |
| C | SPB-18 a 21 y 23; SPB-22 se registra NO VERIFICABLE | SPB-18 con la purga (f), las líneas `BLK`. SPB-19 con las capturas de antes y después. SPB-20 con la captura de cada vista (I-06). SPB-23 con el coste de `Equals` |
| D | SPB-24 a 28 | Enteros. SPB-26 y 28 con los campos del sobre que no son del editor (I-04). SPB-26 con el tiempo de cada «Sí» |
| E | SPB-29 a 36 | Enteros |
| F | SPB-37 a 42, rotulados **CONSTRUCTED REPRESENTATIVE TEST CASES**; SPB-43 se registra NOT AVAILABLE | SPB-40 con las capturas de antes y después |
| G | SPB-44; el recuento de SPB-47 (`rcnest`) y de SPB-48 (desde el JSON) | Sin construir nada |
| H | SPB-49 | Entero |
| I ([T3] §5.5) | SPB-53 (la colisión y `NameDiffersPrefix`), SPB-54 (un `U` con renombrado), SPB-55 (un `U` con la conversión A/B, rotulado) y SPB-56 (la barrera, si un rack real la da; si no, NOT AVAILABLE) | Lo que no exige construir ni inyectar |
| Z | SPB-50 a 52 | Con el camino de «Descartar» (M-16) y las `Dialogs` solo contadas (M-17) |

### 5.1 Recuento de esta noche

| | Pasos |
|---|---|
| **Total** | **54** |
| Operados | 52 |
| Solo registrados | 2: SPB-22, NO VERIFICABLE, y SPB-43, NOT AVAILABLE |
| S | 45 |
| S+O, PENDIENTE DE REVISIÓN DEL OWNER | 9: SPB-08, 09, 16, 18, 20, 23, 25, 36 y 39 |

**Por bloque:** 0: 4; A: 6; B: 7; C: 6; D: 5; E: 8; F: 7; G: 3; H: 1; Z: 3; I: 4.

## 6. Lo que NO se hace esta noche

- **Emitir PB-T5,** o adoptar la forma del contrato (Q-PBT5-7 y Q-PBT5-8).
- **La suite completa de UI en local** (F4-E59; Q-PBT5-1).
- **Las inyecciones de Debug,** SPB-59 a 61 (Q-PBT5-10). Ninguna `RACKCAD_DEBUG_*` en ningún sitio, tampoco en el entorno de un proceso.
- **Los contextos construidos,** SPB-45, 46, 57 y 58 (Q-PBT5-4): espacio papel, bloque del usuario, `LOCKED_LAYER` y último vínculo.
- **Fabricar el estilo de cotas,** o contarlo en otros dibujos (Q-PBT5-2).
- **Los pasos nuevos de M-10,** SPB-62 a 65. No están en la lista de «Esta noche» de [IR1], y van a la corrida contratada.
- **Aplicar el valor de ninguna pregunta.**
- **Los juicios S+O.** Se miden, se capturan y quedan PENDIENTE DE REVISIÓN DEL OWNER.
- **La disposición, la firma, el gate-close y el cierre de F4B.**
- **Seguir operando pasadas las 13:00 UTC.**
- **Guardar una copia,** abrir un original o tocar un AutoCAD que no abrió la sesión.
- **Cambiar Trusted Locations, SECURELOAD, «Always Load»,** el registro o una variable de sistema.

## 7. El registro

En `<carpeta-del-Smoke-PB>`, y después en `F4B-SMOKE-PB/prerun/`, redactado.

- **`run-log.md`:** por cada paso:
  - la hora UTC al empezar;
  - la copia;
  - lo que se hizo;
  - lo observado, con los literales;
  - el resultado **propuesto** (PASS, FAIL, NOT AVAILABLE, NO VERIFICABLE, OBSERVADO o INFORMATIVO);
  - la evidencia: el inventario, la captura y las coordenadas de los clics en los resultados negativos.

  Los S+O, con **PENDIENTE DE REVISIÓN DEL OWNER**. Los errores del operador, sus repeticiones (dos como máximo) y los incidentes, declarados.
- **En la cabecera:**
  - «Corrida previa a la emisión de PB-T5, bajo la orden directa del Owner. No es el Smoke-PB contratado. El Owner decide si la acepta como parte de él (Q-PBT5-9)»;
  - el Candidato, la versión de AutoCAD y el SHA-256 de la biblioteca de bloques, que hacen falta para la reutilización;
  - la hora de inicio frente a las 13:00 UTC.
- **`screens/`:**
  - una captura antes de cada clic, abierta y revisada al tomarla;
  - las de antes y después de SPB-09, 19, 20 y 40;
  - las de cada S+O.

  Recortadas, sin rutas ni nombres, con `derivation-record.txt`. Las de los A/B, rotuladas.
- **`inventories/`:** los crudos se quedan en local; se custodian los resúmenes y el SHA-256 de cada crudo.
- **`integrity.md`** y **`environment-pre.md` / `environment-post.md`.**
- **`owner-summary.md`:**
  - una fila por paso;
  - la sección «Para tu revisión», con los 9 S+O y sus capturas;
  - los LIMITATION con su motivo;
  - los FAIL y los OBSERVADO con toda su evidencia;
  - la información de R-T4-39;
  - dónde se paró, si se llegó a las 13:00 UTC.
- **La custodia:** un commit documental de la sesión, con una entrada en [DISP] y una sección en `I-64-evidence.md`. No emite nada ni cambia el Candidato.

## 8. Cómo se carga el plugin, en resumen

| Regla | Fuente |
|---|---|
| El DLL Debug del worktree, compilado desde el Candidato, con AutoCAD cerrado | [G] §1 y §2; WORKFLOW §6 |
| Nunca se cierra ni se toca un AutoCAD que no abrió la sesión; si hay uno abierto al compilar, no se compila | PBT5-C08 |
| La `InformationalVersion` y el SHA-256, antes de NETLOAD | [G] §7.2 |
| NETLOAD del DLL exacto, por expresión como en [S2] y [BM], y «Load Once» en el aviso | [G] §3; [F] §6.3 |
| Sin tocar Trusted Locations, SECURELOAD, «Always Load», `FILEDIA` ni ninguna otra variable; sin «Always ignore» | [F] §6.3; [S2] |
| `RACKCAD` y la biblioteca; `RACKPANEL` | [G] §3, pasos 4 y 5 |
| Al final, nada de RackCad en `Applications`, `Loaded` ni `AssemblyMap` | SPB-52 |
