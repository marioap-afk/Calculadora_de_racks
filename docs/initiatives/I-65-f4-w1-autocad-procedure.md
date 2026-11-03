# I-65 — F4: procedimiento de W1 (AutoCAD) de la validación final sobre el Candidato de hito `5a5ca577`

Documento de la unidad (Coordinator, 2026-10-17), por la autorización del Owner «F4 Final Validation — W1 AutoCAD Preparation and
Conditional Execution» ([texto literal](../automation/decisions/I-65-owner-authorization-w1-autocad-final-validation.original.txt);
decisiones [§78](../automation/decisions/I-65.md)). Concreta la matriz de la validación final
([preparación](I-65-f4-final-candidate-preparation.md) §6, §7, §9 y §10 y la adenda §11; matriz del hito en la evidencia §81) para la
ventana W1. Reutiliza el método de BASELINE/POST/DIFF del segundo smoke ([procedimiento](I-65-f4-second-smoke-procedure.md);
[preflight/POST](I-65-f4-host-release-and-preflight.md) §3-§6).

> **Estado (2026-10-17): EJECUTADO** (decisiones §79; [informe](../automation/evidence/I-65-f4/w1-R20261017T0847Z/w1-report.md)): W1 = SATISFACTORIA (OV-01 y OV-04 `Valid` 16/16, carga útil 117/117, OV-03 (a)-(e) con su código y sin carpeta, DIFF sin PROHIBIDO). Texto original de preparación:
>
> **Estado: PREPARADO.** W1 AUTOCAD EXECUTION = CONDITIONAL GO: se ejecuta sin otra aprobación solo si se cumplen todas las condiciones
> de entrada de §3. W2 RAM EXECUTION = NO. PRODUCT CODE CHANGES = NO. F4 GATE PASS = NO.

## 1. Identidad fija (cualquier diferencia invalida W1)

| Elemento | Valor |
|---|---|
| Candidato de hito | `5a5ca57731ebbbbc41ed4f702698709d5e128a86` (decisiones §77, evidencia §81) |
| DLL | `D:\r65-smoke\5a5ca577\bin-Debug-net8.0-windows\RackCad.Plugin.dll`, SHA-256 **`c72b759d2ecf5bf9e9ea8172e4035abee966c0a57005748c9b1d421dad19ac07`**, ProductVersion `1.0.0+5a5ca57731ebbbbc41ed4f702698709d5e128a86` |
| Salida completa | 32 archivos; manifiesto `D:\r65-smoke\5a5ca577\output-manifest.txt`, SHA-256 `ed2875e42fb467f0294df15f8e688d968ff058942e7e98b5bb0c8cbf7b5db22b`. Incluye el catálogo de secciones (`catalogs\`), que el producto lee junto a `RackCad.Application.dll` (`CatalogDirectory.Resolve`) |
| Tabla junto a la DLL | `structural-analysis\ram-elements\section-material-map.json`, SHA-256 `fc3d91464ac48a15730661efcd50ecb770114559c68bdefae67e0816972db619` = blob `d088e2a9…` (dos filas `verified`) |
| Valores esperados del validador | `D:\r65-smoke\5a5ca577\expected-inputs.json`, SHA-256 `b3d101c1331632bb3f6ccd19c9d28f5c0a713baff12d628bb621e2f10c409911` |
| Validador del paquete (v2) | `validate_smoke2_package_v2.py`, SHA-256 `1f56b8cba1a64f908c46eb5691930891c3dc0ef1d9bf8ed9c45f8e5f8bdd297a` (copia versionada de la revalidación de V-11; 16 comprobaciones) |
| Contraste de la carga útil | `p2_payload_vs_manual.py`, SHA-256 `a10d5b46938cc88ce4700e491c00a4beb6a90167c21c684ec2c57d8c45f959fa` (el de P2), contra el [paquete de validación de ingeniería](I-65-f4-engineering-validation-package.md) §2.2-§2.5 |
| Biblioteca de bloques | override de `%APPDATA%\RackCad\settings.json`: `D:\Base_de_datos_AutoCAD_V.0.dwg`, SHA-256 `b4ca2248db9c3d72487ac8b5b1e5510cdd8aba231ab340541d91bebca2d560e8`; no se abre ni se modifica |
| AutoCAD | AutoCAD 2025 «English», `acad.exe` R25.0.171.0.0, SHA-256 `2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422` (se vuelve a medir en la BASELINE); perfil `<<Unnamed Profile>>` |
| Dibujo | un único dibujo **nuevo desechable** (el que crea AutoCAD al abrir, `Drawing1.dwg`, plantilla por omisión del perfil), cerrado **sin guardar**. Bloques que crea el producto: uno por línea, `RACKCAD_CANTILEVER_<nombre>_-_frontal_FRONTAL` |
| Script de BASELINE/POST | `w1_hostsnap.ps1` (derivado de `smoke2_hostsnap.ps1` cambiando solo la carpeta base, la lista de archivos de cargas, las carpetas de paquete que se vigilan y dos datos nuevos: segundos de inactividad del usuario y biblioteca de bloques) y `smoke2_regdiff.ps1` para los nombres de valor del registro |

### 1.1 Archivos de cargas (copias congeladas en `D:\r65-smoke\5a5ca577\inputs\`, iguales a las versionadas en `docs/automation/evidence/I-65-f4/w1-prep/loads/`)

| Archivo | SHA-256 (bytes, LF) | Uso | Coherencia con la geometría |
|---|---|---|---|
| `ref-cant-01.loads.json` | `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05` | OV-01 y OV-04 | REF-CANT-01: P1-P3 en los niveles 1-3, lado `PositiveY` (el aprobado) |
| `ov03a-double-face.loads.json` | `8e05faadbfc2304b0cebea1bf4f1187c3734391ccaaff0792c5f661444ebe87b` | OV-03 (a) | doble cara: P1-P3 en `PositiveY` y N1-N3 en `NegativeY`, niveles 1-3 |
| `ov03b-negative-y.loads.json` | `87e642305c6aa758ec7e6b07d7940272c91557ca23f03737c137959cba80de90` | OV-03 (b) | brazos solo en `NegativeY`: N1-N3 en `NegativeY`, niveles 1-3 |
| `ov03c-slope-over-10deg.loads.json` | `25f48e7f…ae05` (mismos bytes que REF-CANT-01) | OV-03 (c) | un brazo `PositiveY` por nivel, como REF-CANT-01 |
| `ov03d-paired-channels.loads.json` | `25f48e7f…ae05` | OV-03 (d) | un cuerpo de brazo `PositiveY` por nivel (los dos canales son el mismo brazo) |
| `ov03e-section-without-row.loads.json` | `25f48e7f…ae05` | OV-03 (e) | geometría idéntica a REF-CANT-01 (HSS 4×4 de otro espesor) |

Las magnitudes, fracciones y unidades son las de REF-CANT-01; N1-N3 repiten las de P1-P3.

### 1.2 Comprobación en seco (preparación; no es evidencia de ninguna OV)

[`w1-dryrun.ps1.txt`](../automation/evidence/I-65-f4/w1-prep/w1-dryrun.ps1.txt) ejecuta `CantileverStructuralExport.Build` de las DLL
congeladas del Candidato **fuera de AutoCAD**, con el diseño de REF-CANT-01
([`ref-cant-01.design.json`](../automation/evidence/I-65-f1/ref-cant-01.design.json)) y la variante de cada caso, sin llamar a la fábrica
ni al publicador. [Resultado](../automation/evidence/I-65-f4/w1-prep/w1-dryrun-output.txt): REF-CANT-01 `Valid` sin diagnósticos; cada
caso de OV-03 da `Unsupported` con **solo** el código esperado. Dos controles muestran por qué cada caso lleva su propio archivo: con
cargas en `NegativeY`, el caso (e) daría `Invalid` (`LOAD-TARGET-UNKNOWN`). En el código, las comprobaciones de cara, lado y arreglo
rechazan antes de resolver las cargas (`CantileverStructuralModelSource.Describe`), la de pendiente también (`Idealise`, antes del
bucle de cargas) y la de fila de mapeo, después de colocarlas (`StructuralModelBuilder`). La validación real es la de AutoCAD (§5).

## 2. Carpetas de la corrida

`D:\r65-smoke\5a5ca577\ov-<RunId>\` (nueva; `<RunId>` = `R<AAAAMMDD>T<HHMM>Z` de la BASELINE) con `baseline\`, `post\` y `captures\`.
Carpetas de paquete que **no** existen al empezar (se teclean con `\`, en trozos de ≤ 10 caracteres y nunca por el portapapeles):

| Paquete | Carpeta tecleada | Debe existir al final |
|---|---|---|
| `pkg-OV01` | `D:\r65-smoke\5a5ca577\ov-<RunId>\pkg-OV01` | sí, con 3 archivos |
| `pkg-OV04` | `D:\r65-smoke\5a5ca577\ov-<RunId>\pkg-OV04` | sí, con 3 archivos |
| `pkg-OV03a` … `pkg-OV03e` | `D:\r65-smoke\5a5ca577\ov-<RunId>\pkg-OV03a` … `pkg-OV03e` | **no** |

## 3. Condiciones de entrada (todas; si falta una, no se abre AutoCAD)

| # | Condición |
|---|---|
| W1-PR-01 | Este procedimiento versionado y en `origin` antes de abrir AutoCAD |
| W1-PR-02 | Ventana e identidad de la DLL comunicadas a I-52 y a I-64 por su canal |
| W1-PR-03 | Confirmación **expresa** de I-64 de que no hay conflicto (un acuse de recibo no basta) |
| W1-PR-04 | `acad.exe` = 0 y `RAMElements.exe` = 0 |
| W1-PR-05 | Nadie usa el equipo: segundos de inactividad del usuario (X-03) ≥ 120 justo antes de empezar y ninguna aplicación de trabajo del usuario abierta en primer plano; si el usuario está trabajando, no se empieza |
| W1-PR-06 | BASELINE completa y limpia frente a las referencias del segundo smoke (perfil, `TRUSTEDPATHS` 19 entradas `a9ba0fd1…`, `SECURELOAD` 1, `acad.exe`, `Applications`/`InstalledApplications`/`Loaded`/HKLM, `ApplicationPlugins` sin RackCad, 0 variables `RackCad`, ACL) |
| W1-PR-07 | DLL, manifiesto de salida (recalculado), tabla, biblioteca de bloques, valores esperados y los seis archivos de cargas con los SHA-256 de §1 |
| W1-PR-08 | Ninguna diferencia protegida; ninguna carpeta de paquete de §2 existe |
| W1-PR-09 | Instantánea K1 de los checkouts |

## 4. Pasos

| # | Paso |
|---|---|
| W1-01 | Crear `D:\r65-smoke\5a5ca577\ov-<RunId>\`; K1; `w1_hostsnap.ps1 -Phase baseline -Run <carpeta>` |
| W1-02 | Aviso previo a I-52 e I-64 con la hora de apertura |
| W1-03 | Abrir AutoCAD 2025 (una sola instancia, la de Computer Use) con un dibujo nuevo desechable; captura del título |
| W1-04 | SHA-256 de la DLL justo antes de cargar. `NETLOAD` navegando hasta `D:\r65-smoke\5a5ca577\bin-Debug-net8.0-windows\RackCad.Plugin.dll` (el diálogo abre en la última carpeta usada, que es la del segundo smoke: no cargar nada de ahí); en el aviso estándar «Security - Unsigned Executable File», **solo «Load Once»**; nunca «Always Load». Después, lista de módulos de `acad.exe`: un único `RackCad.Plugin.dll` desde esa carpeta y con ese SHA-256 |
| W1-05 | **REF-CANT-01** con `RACKCANTILEVER` (§4.1). «Actualizar» y comprobar el resumen «2 estaciones · 1 intervalos · altura común 90.5 in …»; «Insertar frontal» en un punto libre |
| W1-06 | **OV-01:** `RACKRAMPAQUETE` → rack REF-CANT-01 → estación `0` → `D:\r65-smoke\5a5ca577\inputs\ref-cant-01.loads.json` → carpeta `…\pkg-OV01`. Esperado: `RackCad paquete estructural: Valid. Paquete publicado en …\pkg-OV01`, la huella y los tres `Archivo escrito`, sin diagnósticos. Captura |
| W1-07 | Validador v2 sobre `pkg-OV01` (carpeta tecleada exacta) → **PASS 16/16**; huella de la consola = manifiesto = script; contraste de la carga útil → **117/117** con `rowsSha256` `54ca9f6c…98c` y cargas `25f48e7f…` (condición 3 de la estrategia A, sobre la carga útil y no sobre la huella) |
| W1-08 | **OV-04:** `RACKRAMPAQUETE` → el mismo rack → estación `0` → `ref-cant-01.loads.json` → carpeta `…\pkg-OV04`. Esperado igual que W1-06; validador v2 **16/16**. Es el segundo paquete válido que W2 ejecutará sobre un modelo no vacío |
| W1-09 | **OV-03 (a)-(e)** (§4.2), en este orden. Para cada caso: `RACKCANTILEVER` con la línea del caso (REF-CANT-01 con un único cambio), «Actualizar», «Insertar frontal» en un punto libre distinto; `RACKRAMPAQUETE` → ese rack → estación `0` → su archivo de cargas → su carpeta. **Un único intento**. Captura de la consola; `Test-Path` de la carpeta (debe ser `False`) y de `*.tmp-*` hermanos (0) |
| W1-10 | Cerrar AutoCAD **sin guardar** («No» al guardar); `acad.exe` = 0 |
| W1-11 | `w1_hostsnap.ps1 -Phase post` y `smoke2_regdiff.ps1`; clasificar el DIFF (PROHIBIDO / INFORMAR / LIMPIO); K2 |
| W1-12 | Aviso de cierre a I-52 e I-64 con `acad.exe` = 0 y el resultado del DIFF |
| W1-13 | Custodia (§6) |

### 4.1 Construcción exacta de REF-CANT-01 (igual que en el segundo smoke; [especificación](I-65-f1-ref-cant-01.md) §1)

| Campo de la ventana | Valor |
|---|---|
| Nombre | `REF-CANT-01` |
| Estaciones / Separación entre centros (in) | `2` / `48` |
| Caras / Lado (solo en sencilla) | `Sencilla` / `Lado +Y` |
| Niveles / Troquel del primer nivel (índice) / Claro solicitado entre niveles (in) | `3` / `0` / `24` |
| Resto de la topología, arriostramiento, separador y tensor | valores por omisión, sin tocar |
| Columna y base → Configurar | columna `AISC-W-W12X26`, base `AISC-W-W12X26`, longitud de base `48`; resto por omisión |
| Brazo (por omisión) → Configurar | Arreglo `Sencillo`; sección `AISC-HSS-RECT-HSS4X4X_250`; longitud de corte `36`; pendiente por omisión `0.438` (7/16); placas por omisión |
| Matriz estación × nivel × lado | sin sustituciones por celda |

### 4.2 Casos de OV-03 (cada uno es REF-CANT-01 con un único cambio)

| Caso | Nombre de la línea | Único cambio en la ventana | Archivo de cargas | Diagnóstico esperado (consola) |
|---|---|---|---|---|
| (a) estación de doble cara | `OV03-A-DOBLE` | Caras = `Doble` | `ov03a-double-face.loads.json` | `Unsupported` + `Blocking STATION-FACE-UNSUPPORTED (station-0)` |
| (b) brazo `NegativeY` | `OV03-B-NEGY` | Lado = `Lado −Y` (caras `Sencilla`) | `ov03b-negative-y.loads.json` | `Unsupported` + `Blocking STATION-SIDE-UNSUPPORTED (station-0)` |
| (c) brazo con más de 10° | `OV03-C-PEND` | Brazo → Pendiente (subida por 12 in) = `3` (atan(3/12) = 14.04°) | `ov03c-slope-over-10deg.loads.json` | `Unsupported` + `Blocking ARM-SLOPE-UNSUPPORTED (ARM.1.PositiveY)`, `(ARM.2.PositiveY)`, `(ARM.3.PositiveY)` |
| (d) canales pareados | `OV03-D-CANAL` | Brazo → Arreglo = `Canal doble enfrentada` y sección `AISC-C-C4X5_4` | `ov03d-paired-channels.loads.json` | `Unsupported` + `Blocking ARM-ARRANGEMENT-UNSUPPORTED (ARM.1.PositiveY)`, `(ARM.2.PositiveY)`, `(ARM.3.PositiveY)` |
| (e) perfil sin fila de mapeo | `OV03-E-SINFILA` | Brazo → sección `AISC-HSS-RECT-HSS4X4X_313` (HSS 4×4×5/16, familia admitida, sin fila en la tabla) | `ov03e-section-without-row.loads.json` | `Unsupported` + `Blocking MATERIAL-ROW-MISSING (AISC-HSS-RECT-HSS4X4X_313)` |

En todos, la primera línea esperada es `RackCad paquete estructural: Unsupported. El modelo estructural de la estacion no es valido; no se
escribio nada.` Un caso **pasa** solo si aparece el resultado `Unsupported`, aparecen exactamente los códigos esperados (ningún otro, y
en particular ningún `Invalid` por cargas o por carpeta) y la carpeta no existe después. Para (d) se elige la variante de canales
pareados (IDL-10 la nombra; adenda §11: «canales pareados → `ARM-ARRANGEMENT-UNSUPPORTED`»); la de familia C/L no se ejecuta.

El caso «fila sin verificar» **no** se ejecuta en W1: lo cubren las pruebas de fixture del Candidato (decisiones §77, CD-I65-F4-186).

## 5. Condiciones de aceptación de W1

| # | Condición |
|---|---|
| W1-AC-01 | La DLL cargada es la del Candidato: mismo SHA-256 en la BASELINE, justo antes de cargar, en la lista de módulos y en el POST |
| W1-AC-02 | OV-01 y OV-04: `Valid`, paquetes completos (3 archivos, sin `*.tmp-*`) y validador v2 **16/16** cada uno |
| W1-AC-03 | Carga útil de `pkg-OV01` = especificación de REF-CANT-01 (117/117) |
| W1-AC-04 | OV-03 (a)-(e): el rechazo esperado de §4.2 y ninguna carpeta. Un `Invalid` por cargas no sustituye al `Unsupported` esperado |
| W1-AC-05 | AutoCAD cerrado sin guardar; RAM nunca iniciado |
| W1-AC-06 | DIFF sin clase `PROHIBIDO` (las clases `INFORMAR` de los smokes anteriores —MRU de NETLOAD, posiciones de ventanas, `VIEWBACKSTATUS`— se registran) |
| W1-AC-07 | K1/K2 sin cambios en los checkouts |

## 6. Evidencia que se custodia

Capturas (título del dibujo, NETLOAD, aviso «Load Once», ventanas de cada línea, consola de cada `RACKRAMPAQUETE`, cierre sin guardar);
texto literal de cada consola; lista de módulos; informes del validador v2 de `pkg-OV01` y `pkg-OV04`; contraste de la carga útil;
`manifest.json` de los dos paquetes (versionados) y SHA-256 de sus scripts y cargas; `Test-Path` de las carpetas de OV-03;
resúmenes de BASELINE y POST, DIFF del registro y K1/K2; horas UTC de cada hito. Las exportaciones del registro y las capturas
completas quedan en la carpeta local de la corrida (no se versionan los datos personales).

## 7. Condiciones de parada

| # | Parada |
|---|---|
| W1-ST-01 | Falta una condición de §3 (no se abre AutoCAD) |
| W1-ST-02 | Otra `acad.exe`, o una sesión de AutoCAD de I-52 o I-64, antes o durante la ventana; el usuario empieza a usar el equipo |
| W1-ST-03 | Aviso de seguridad distinto del estándar, petición de confianza o de licencia, o diálogo modal no previsto |
| W1-ST-04 | SHA-256 de la DLL distinto, módulo cargado desde otra carpeta, o tabla o cargas con otro SHA-256 |
| W1-ST-05 | OV-01 u OV-04 distinto de `Valid`, con diagnósticos, publicación fallida, paquete parcial o en otro lugar, o validador distinto de 16/16: se captura, se cierra sin guardar y **no se reintenta** |
| W1-ST-06 | Un caso de OV-03 con otro resultado (otro código, `Invalid`, `Valid` o una carpeta creada): se registra literal como hallazgo, **sin reintento ni sustitución**; si se escribió un paquete, STOP de W1 |
| W1-ST-07 | Un caso de OV-03 que no se puede construir con la ventana tal como dice §4.2: se registra el bloqueo de ese caso, no se sustituye por otro escenario y se sigue con los demás |
| W1-ST-08 | Pegado accidental del portapapeles o renombrado accidental en un diálogo: Ctrl+Z inmediato, registro y comprobación en disco; si alteró algo fuera del dibujo desechable, STOP |
| W1-ST-09 | DIFF con clase `PROHIBIDO` |
| W1-ST-10 | Necesidad de RAM, de cambiar `TRUSTEDPATHS`, `SECURELOAD`, perfil, ACL o cualquier configuración persistente, o de tocar código |

Las correcciones de datos en la ventana del configurador **antes** de ejecutar `RACKRAMPAQUETE` no son intentos (se registran); si una
línea insertada resulta mal construida antes de exportarla, se deshace con Ctrl+Z y se vuelve a insertar, con registro. El intento es la
ejecución de `RACKRAMPAQUETE`. Toda parada: registro literal, aviso de cierre a I-52 e I-64 y decisión del Owner. Ninguna parada se
resuelve cambiando código.

## 8. Después de W1 (sin AutoCAD ni RAM)

Si W1 es satisfactoria: custodia; inyecciones `inj-OV05` (`unknown-section`), `inj-OV06` (`inverted-axes`), `inj-OV07a`
(`wrong-version`) e `inj-OV07b` (`missing-core-function`) desde `pkg-OV01` con la herramienta del Candidato
(`D:\r65-smoke\5a5ca577\ramtool\`, `0b56e198…`), en carpetas nuevas, sin `--embed-folder`, comprobando que los SHA-256 de `pkg-OV01`
no cambian; scripts de volcado de W2 con `dump-script --out <carpeta nueva> --adapter-version 1.0.0` y comparación con
`manual-rackcad-ram-dump.py.txt` (`af262489…`): solo puede cambiar la línea de la carpeta (adenda §11, A65-F4C-05 (a)). Después, estado
de preparación de RAM Elements. **W2 no se ejecuta.**
