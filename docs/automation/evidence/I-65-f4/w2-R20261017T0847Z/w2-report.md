# I-65 F4 — W2 (RAM Elements) de la validación final sobre el Candidato de hito `5a5ca577` (custodia, 2026-10-17)

- **Autoridad:** «F4 Final Validation — W2 RAM Elements»
  ([texto literal](../../../decisions/I-65-owner-authorization-w2-ram-final-validation.original.txt)); decisiones §80-§81.
  W2 RAM EXECUTION = CONDITIONAL GO; AUTOCAD = NO; STRUCTURAL ANALYSIS = NO; PRODUCT CODE CHANGES = NO; F4 GATE PASS = NOT YET AUTHORIZED.
- **Procedimiento:** [I-65-f4-w2-ram-procedure.md](../../../../initiatives/I-65-f4-w2-ram-procedure.md) (`f0f0aabf`, CI 39047302707 4/4).
- **Ejecutor:** Coordinator con Computer Use (acceso concedido solo a «RAM Elements 2026»); sin Codex ni Workers; sin AutoCAD.
- **Carpeta local:** `D:\r65-smoke\5a5ca577\ov-R20261017T0847Z\` (`w2-pre\`, `w2-post\`, `w2-captures\` y las carpetas de los paquetes).

## 1. Condiciones de entrada (W2-PR-01..10)

| # | Resultado |
|---|---|
| W2-PR-01 | procedimiento en `origin` (`f0f0aabf`), CI 39047302707 4/4 `success` |
| W2-PR-02 | inactividad del usuario 143 s a las 16:10:24Z, proceso en primer plano `claude` (a las 16:09Z eran 70 s: se esperó) |
| W2-PR-03 | `RAMElements.exe` = 0, `acad.exe` = 0 (PRE 16:09:09Z) |
| W2-PR-04 | `RAMElements.exe` 26.00.01.15 `7dcb4855…`; `ramelements` 0.1.0, árbol `f71a017f…` |
| W2-PR-05 | `W.sec` `dffd9e00…`, `HSS_SQR.sec` `992ddac9…`, `Steel.mat` `2a8b4239…`; árboles con el método del procedimiento: secciones US `ccdc2346…`, materiales US `4cb742a5…` ([registro](db-tree-python-method.txt)). El script PRE/POST calcula los árboles con otro formato (`e70bfff0…`, `b8e46d28…`) y se usa solo para comparar PRE con POST |
| W2-PR-06 | los 6 paquetes, sus scripts, el script de volcado y el volcado manual `c4a98fb0…` con los SHA-256 del procedimiento |
| W2-PR-07 | ningún `ram-readback.json` ni `ram-dump.json` previo |
| W2-PR-08 | copia de `Config.ini` (`2c593957…`) en `w2-pre\` |
| W2-PR-09 | **I-64 HOST COORDINATION ACK = CONFIRMED — NO CONFLICT FOR I-65 W2**, recibida a las 16:08:16Z ([coordinación](../host-coordination-w2-2026-10-17.md)); aviso de cortesía a I-52 |
| W2-PR-10 | K1 hecha |

## 2. Horas reales (UTC)

| Hito | Hora |
|---|---|
| Aviso previo a I-64 e I-52 | ~16:10Z |
| Apertura de RAM (una instancia) | 16:10:37Z |
| OV-01 construcción / volcado / descarte | 16:13:02Z / 16:13:52Z / 16:14:27Z |
| OV-01 comparador | ~16:14:50Z |
| OV-04 / OV-05 / OV-06 / OV-07a / OV-07b ejecutados | 16:16:15Z / 16:17:31Z / 16:18:53Z / 16:20:07Z / 16:21:09Z |
| RAM cerrado; `RAMElements.exe` = 0 | ≤ 16:21:38Z |
| Control de OV-05, POST, K2, aviso de cierre | 16:21-16:23Z |

## 3. Resultados por caso (un intento por paquete; un modelo desechable por escenario)

Antes de cada ejecución se comprobaron en la captura la carpeta y el nombre del script y se midió su SHA-256 (todos iguales a §1 del
procedimiento), y se confirmó que no existía la lectura de vuelta. Cada modelo se descartó con File > New → «¿Desea salvarlo?» → **No**,
salvo los de OV-07a y OV-07b, que no se modificaron: RAM abrió el modelo nuevo y se cerró sin preguntar.

### OV-I65-01 — equivalencia final ([capturas 03-08](captures/04-ov01-consola-readback-ok-huella.jpg))

Modelo inicial nuevo y vacío («Elementos: 0», estado `CM`; [captura 01](captures/01-ram-modelo-nuevo-vacio.jpg)).

```text
--- Ejecutando script ---
READBACK OK
AnalysisInputFingerprint f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663
--- Finalizado ---
--- Ejecutando script ---
DUMP OK
--- Finalizado ---
```

- Lectura de vuelta ([`pkg-OV01-ram-readback.json`](pkg-OV01-ram-readback.json), `0848b785…`): `READBACK OK`, `differences` vacío, huella =
  manifiesto, 8 nudos, 7 miembros, 3 cargas, caso `REF1` `OTHER` con peso propio 0. «Elementos: 15».
- Volcado en la misma sesión y el mismo modelo ([`dump-OV01-gen-ram-dump.json`](dump-OV01-gen-ram-dump.json), `f5789d1c…`): `DUMP OK`, 8 nudos,
  7 miembros, casos `CM` (DL) y `REF1` (OTHER), **`combinations` []**, `errors` []. Sus bytes son **idénticos** al volcado del generado de
  P2: el mismo modelo, escrito de nuevo por la API, se lee igual (reproducibilidad); el archivo es nuevo y la lectura de vuelta de P2 no se
  usa en ningún sitio.
- **Comparador** (`compare --package pkg-OV01 --generated dump-OV01-gen\ram-dump.json --manual <volcado manual de P2>`, sin
  `--*-combinations-unread`; [informe](w2-OV01-comparison.json), [consola](w2-OV01-comparison-console.txt)): **PASS, código 0**;
  `inputs.manualDumpSha256` = `c4a98fb0ea798031de2b3052d81eae89f42b731a92f06dbfa97f148ade9ba23b`.

| Fila del comparador | Resultado |
|---|---|
| Coordenadas de nudos (0.02 in) | PASS, Δ máx. 0 |
| Conectividad | PASS |
| Ejes locales (0.1°) | PASS, Δ máx. 0 |
| Nombre de sección / de material | PASS / PASS |
| Propiedades de sección / de material | NOT_EVALUATED (la API no las expone; hoja firmada D-09) |
| Valores por omisión de miembro (punto cardinal, offsets, factores, axial, resortes) | PASS |
| Liberaciones y apoyos | PASS |
| Cargas (0.1 % / 0.02 in) | PASS, Δ máx. 0 |
| **Combinaciones (CMB-3)** | **PASS: 0 en el generado y 0 en el manual** |
| Categoría del caso generado | PASS |
| Categoría del caso manual / peso propio | INFORMATIVE / INFORMATIVE (DIF-01 informativa; sin peso propio) |

**OV-I65-01 = PASS** (AutoCAD en W1 y RAM en W2).

### OV-I65-04 — modelo no vacío ([capturas 09-15](captures/12-ov04-consola-gate-1-nodo.jpg))

Modelo inicial: nuevo, **distinto** del de OV-01, con un nudo de prueba tecleado en la rejilla, nudo 1 en (0, 0, 0), «Elementos: 1»,
solo el estado `CM`. Para crear la fila hubo que confirmar la celda X con Enter: teclear «0» sobre el 0 por omisión no la creaba.

```text
--- Ejecutando script ---
FAILED — DISCARD MODEL
 - gate: the model has 1 nodes
AnalysisInputFingerprint f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663
--- Finalizado ---
```

Después: «Elementos: 1», rejilla con el nudo 1 en (0, 0, 0) y nada más, estado `CM` (sin `REF1`). Lectura de vuelta
([`pkg-OV04-ram-readback.json`](pkg-OV04-ram-readback.json)) `FAILED — DISCARD MODEL` con `gate: the model has 1 nodes` y `values`
vacíos. **Rechazo antes de escribir; el modelo conserva exactamente su contenido inicial: PASS.**

### OV-I65-05 — sección inexistente ([capturas 16-21](captures/17-ov05-consola-seccion-rechazada.jpg))

Modelo inicial nuevo y vacío.

```text
--- Ejecutando script ---
FAILED — DISCARD MODEL
 - exception: ValueError: Section not found: "HSS_SQR INEXISTENTE-99". Section names must include a space and match the database exactly (e.g. "W 14X22", not "W14X22"). Check your instructions for valid name examples and conventions.
AnalysisInputFingerprint f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663
--- Finalizado ---
```

(La consola corta la línea larga; el texto completo está en la [lectura de vuelta](inj-OV05-ram-readback.json).) **Elementos parciales,
documentados antes de descartar:** «Elementos: 9», es decir, los 8 nudos con su descripción y 1 miembro (1→2, sin descripción), el que
recibía la sección inexistente ([nudos](captures/19-ov05-parcial-nudos.jpg), [miembros](captures/20-ov05-parcial-miembros.jpg)). Ningún
resultado válido. Control fuera de RAM: `compare --package inj-OV05 …` → **REJECTED, código 2** («The package is a fault injection
(unknown-section): its read-back is not approved: exception: ValueError: Section not found …»;
[informe](w2-OV05-control.json)). **PASS.**

### OV-I65-06 — orientación invertida ([capturas 22-25](captures/23-ov06-consola-eje-local-2.jpg))

Modelo inicial nuevo y vacío.

```text
--- Ejecutando script ---
FAILED — DISCARD MODEL
 - member ARM.1.PositiveY: local axis 2 (0.0, 0.9993360567820188, 0.036434127070177715)
AnalysisInputFingerprint f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663
--- Finalizado ---
```

El modelo quedó escrito entero («Elementos: 15», `REF1`); la lectura de vuelta comparó el eje local 2 real del primer miembro con el
esperado invertido de la inyección y lo marcó ([lectura de vuelta](inj-OV06-ram-readback.json)). Sin `READBACK OK`; modelo descartado sin
guardar. **PASS.**

### OV-I65-07a — versión de la API ([capturas 26-29](captures/28-ov07a-consola-gate-version.jpg))

Modelo inicial nuevo y vacío («Elementos: 0»).

```text
--- Ejecutando script ---
FAILED — DISCARD MODEL
 - gate: package version is '0.1.0', expected '999.0.0'
AnalysisInputFingerprint f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663
--- Finalizado ---
```

Después: «Elementos: 0», sin «Modificado», estado `CM`. Lectura de vuelta FAILED sin valores ([archivo](inj-OV07a-ram-readback.json)).
**Rechazo antes de crear elementos; el modelo sigue vacío: PASS.**

### OV-I65-07b — función de la API inexistente ([capturas 30-33](captures/32-ov07b-consola-gate-funcion.jpg))

Modelo inicial nuevo y vacío.

```text
--- Ejecutando script ---
FAILED — DISCARD MODEL
 - gate: core function missing: rackcad_nonexistent_function
AnalysisInputFingerprint f5ce900c14ee768bb85670e814099d58947857778077ced8f81d371a8858d663
--- Finalizado ---
```

Después: «Elementos: 0», sin «Modificado». Lectura de vuelta FAILED sin valores ([archivo](inj-OV07b-ram-readback.json)). **PASS.**

## 4. Integridad de los paquetes

En las seis carpetas, `manifest.json`, `rackcad-ram-build.py` y `structural-loads.json` tienen **los mismos SHA-256 antes y después**
([PRE](w2-pre-summary.txt), [POST](w2-post-summary.txt)). El único archivo nuevo de cada una es su `ram-readback.json` (modo `"x"`, diseño
del paquete), y en `dump-OV01-gen`, el `ram-dump.json`. El volcado manual de P2 no cambió (`c4a98fb0…`). Ningún paquete se regeneró ni se
editó.

## 5. PRE / POST y estado del host

- Identidades sin cambios entre PRE y POST: `RAMElements.exe`, `ramelements` 0.1.0, bases de secciones y materiales US.
- `Config.ini` `2c593957…` → `505a3592…` ([diferencias](w2-config-ini-diff.txt), solo nombres de clave): `DefaultFolders|PythonScripts` →
  `…\inj-OV07b` (la última carpeta usada) y los contadores de uso `accPythonConsole` y `accQANew`; 83 claves iguales. Estado de
  interfaz, como en P2; no se restaura (CD-I65-F4-73).
- K1/K2: sin hallazgos ([comparación](k1-k2-w2-compare.json)). `acad.exe` = 0 durante toda la ventana; AutoCAD no se abrió.
- Avisos previo y de cierre a I-64 e I-52.

## 6. Desviaciones e incidencias

1. **OV-04:** la fila del nudo de prueba solo se creó al confirmar la celda X con Enter (teclear «0» y Tab no la creaba, porque el valor
   coincide con el de omisión). Era la preparación del estado inicial, no un intento.
2. Para aislar cada caso se usó «Limpiar» en la consola de Python antes de cada ejecución, y en OV-05 se abrió la pestaña «Miembros»
   solo para documentar el estado parcial. Las dos cosas son de interfaz y no cambian el modelo.
3. Los modelos de OV-07a y OV-07b no se modificaron: «Nuevo» y el cierre de RAM no preguntaron si guardar.
4. El volcado del generado coincide byte a byte con el de P2 (reproducibilidad). No es una reutilización: es un archivo nuevo, producido
   en la misma sesión que la construcción de OV-01.
5. Sin desviaciones del procedimiento en los oráculos; ningún reintento; ningún cambio de código, de paquetes ni de configuración de
   seguridad.

## 7. Resumen

| OV | Resultado |
|---|---|
| OV-I65-01 | **PASS** (W1 + W2: `READBACK OK`, huella, volcado en la misma sesión, comparador PASS con `c4a98fb0…`, combinaciones 0) |
| OV-I65-03 | **PASS** (W1; «fila sin verificar» por las pruebas de fixture) |
| OV-I65-04 | **PASS** |
| OV-I65-05 | **PASS** (con elementos parciales documentados y descartados; control REJECTED) |
| OV-I65-06 | **PASS** |
| OV-I65-07 (a y b) | **PASS** |

**W2 = SATISFACTORIA.** Con W1, la matriz de la validación final del hito queda completa (AC-F4-07). **No es GATE PASS**: falta la
revisión final de cierre de F4 (Architect y Owner).
