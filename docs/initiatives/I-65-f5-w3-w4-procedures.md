# I-65 — F5: procedimientos de W3 (AutoCAD) y W4 (RAM Elements) para OV-I65-02 y matriz AC-F5

Documento de la unidad (Coordinator, 2026-10-19), por la disposición del Owner «F5 Technical Acceptance / Candidate Preparation / Privacy
Gate» ([texto literal](../automation/decisions/I-65-owner-disposition-f5-acceptance-candidate-privacy-gate.original.txt); decisiones
[§87](../automation/decisions/I-65.md)). Concreta la [preparación de F5](I-65-f5-preparation.md) y reutiliza el método de
[W1](I-65-f4-w1-autocad-procedure.md) y [W2](I-65-f4-w2-ram-procedure.md).

> **Estado: PREPARADO. W3 AUTOCAD = NOT YET AUTHORIZED; W4 RAM = NOT YET AUTHORIZED.** La ejecución exige, en este orden: contención de
> privacidad verificada (repositorio privado, comprobado por lectura), declaración del Candidato de hito de F5 por el Owner y autorización
> específica de cada ventana. F5 GATE PASS = NO.

## 1. Identidad (se fija en la declaración del Candidato de F5)

| Elemento | Valor |
|---|---|
| Candidato de hito de F5 | el SHA que declare el Owner (propuesto en la evidencia del Candidato); producto = árbol de `51fc82f1` |
| DLL | `D:\r65-smoke\<sha8>\bin-Debug-net8.0-windows\RackCad.Plugin.dll`, SHA-256 y ProductVersion `1.0.0+<sha>` del bloque de declaración (guía §7.1); salida completa con su manifiesto |
| Tabla junto a la DLL | `fc3d91464ac48a15730661efcd50ecb770114559c68bdefae67e0816972db619` (blob `d088e2a9…`), sin cambios |
| Herramienta de F4 | `D:\r65-smoke\<sha8>\ramtool\` (comparador y `dump-script`), compilada desde el mismo SHA |
| Archivo de cargas de OV-I65-02 | copia congelada en `D:\r65-smoke\<sha8>\inputs\ov-i65-02.loads.json`, SHA-256 de bytes LF `d675d9f6bb537083a63571f08fb9884c1ab085a0eca4ad1555ee98bea4f8ba4c` (= `docs/automation/evidence/I-65-f5-prep/ov-i65-02.loads.json`) |
| Valores esperados del validador v2 | `D:\r65-smoke\<sha8>\expected-inputs.json`, calculado con el `RackCad.Application.dll` de esa salida, con la tabla y el archivo de cargas de OV-I65-02 |
| Hoja manual | `docs/automation/evidence/I-65-f5-prep/ov-i65-02-manual-entry-sheet.json` (solo de la especificación congelada y de D-10b) |
| Tabla de valores esperados | preparación §4 (6/6 en seco con las DLL de `5a5ca577`; se repite en seco con las del Candidato de F5 antes de W3) |
| RAM, API y bases | `RAMElements.exe` `7dcb4855…`, `ramelements` 0.1.0 `f71a017f…`, `W.sec` `dffd9e00…`, `HSS_SQR.sec` `992ddac9…`, `Steel.mat` `2a8b4239…` |
| Biblioteca de bloques | override `D:\Base_de_datos_AutoCAD_V.0.dwg`, SHA-256 `b4ca2248…60e8` (se mide de nuevo) |

## 2. Reglas de privacidad durante las ventanas (PRIVACY HOLD; C-4)

- No se abre ninguna ventana mientras el repositorio sea público.
- **Capturas:** solo recortes de la zona técnica (consola, rejillas, barra de estado, barra de ruta y lista de archivos de los diálogos);
  nunca la barra de título, el panel de navegación de los diálogos, la barra de tareas ni ventanas ajenas a la tarea.
- **Registros:** rutas con `<usuario>`; ningún JSON crudo de la API de GitHub (se versionan solo `id`, evento, rama, `head_sha`, estado y
  jobs); los `.trx` no se versionan (traen usuario y equipo).
- Antes de versionar, una comprobación mecánica de los textos y una revisión visual de cada recorte. Si aparece un dato identificable, el
  archivo **no** se versiona hasta redactarlo.
- Si aparece cualquier credencial, token o secreto: se informa de inmediato del tipo y del alcance, sin reproducir su valor.

## 3. W3 — AutoCAD (generación de `pkg-OV02`)

### 3.1 Condiciones de entrada (si falta una, no se abre AutoCAD)

| # | Condición |
|---|---|
| W3-PR-01 | Repositorio privado, comprobado por lectura (API autenticada `private: true` y consulta anónima ≠ 200) |
| W3-PR-02 | Candidato de hito de F5 **declarado** por el Owner y autorización específica de W3 |
| W3-PR-03 | Confirmación expresa del Coordinator de I-64 dada en su chat (ventana relativa a la recepción); aviso a I-52 |
| W3-PR-04 | `acad.exe` = 0 (ninguna sesión del usuario abierta) y `RAMElements.exe` = 0; nadie usando el equipo (inactividad ≥ 120 s) |
| W3-PR-05 | BASELINE limpia frente a las referencias (perfil, `TRUSTEDPATHS`, `SECURELOAD`, `acad.exe`, Applications, plugins, variables, ACL medida por el mismo camino) |
| W3-PR-06 | DLL, salida, tabla, biblioteca, valores esperados y archivo de cargas con los SHA-256 de §1; carpeta `pkg-OV02` inexistente |
| W3-PR-07 | Comprobación en seco con las DLL del Candidato de F5: 6/6 iguales a la tabla de la preparación §4 |
| W3-PR-08 | K1 de los checkouts |

### 3.2 Pasos y oráculos

| # | Paso | Oráculo |
|---|---|---|
| W3-01 | BASELINE; aviso previo | — |
| W3-02 | Abrir AutoCAD (una instancia) con un dibujo nuevo desechable | — |
| W3-03 | SHA-256 de la DLL antes de cargar; NETLOAD navegando hasta la carpeta del Candidato de F5; **solo «Load Once»**; lista de módulos | módulo único desde esa carpeta y con el SHA-256 declarado |
| W3-04 | REF-CANT-01 con `RACKCANTILEVER` exactamente como en W1 §4.1 | resumen «2 estaciones · 1 intervalos · altura común 90.5 in …» |
| W3-05 | `RACKRAMPAQUETE` → estación `0` → `D:\r65-smoke\<sha8>\inputs\ov-i65-02.loads.json` → carpeta nueva `D:\r65-smoke\<sha8>\f5-<RunId>\pkg-OV02` (con `\`, en trozos ≤ 10, sin portapapeles); **un único intento** | `Valid`, huella y tres «Archivo escrito», sin diagnósticos |
| W3-06 | Validador v2 con los valores esperados de §1 | **16/16** |
| W3-07 | Contraste de la carga útil de `pkg-OV02` con la tabla de la preparación §4 (seis cargas: miembro, valor y porcentaje) | **6/6** (≤ 1e-9); Q2 al 99.99999999999997 % se registra como diferencia de representación |
| W3-08 | Cerrar sin guardar; POST y DIFF; K2; aviso de cierre | sin PROHIBIDO; K1/K2 sin cambios |

### 3.3 Presupuesto y paradas

Una ventana de ~40 min; 1 NETLOAD; 1 generación; 0 reintentos; 0 Codex. **STOP:** cualquier condición de entrada que falle; aviso de
seguridad distinto del estándar; DLL o módulo distintos; resultado distinto de `Valid` o con diagnósticos; validador < 16/16; carga útil
≠ tabla; DIFF con PROHIBIDO; necesidad de cambiar configuración o código; un dato identificable en la pantalla que no pueda recortarse.

## 4. W4 — RAM Elements (OV-I65-02)

### 4.1 Condiciones de entrada (si falta una, no se abre RAM)

| # | Condición |
|---|---|
| W4-PR-01 | W3 satisfactoria y aceptada; repositorio privado (comprobado de nuevo) |
| W4-PR-02 | Autorización específica de W4 (incluye a Claude con Computer Use como operador del modelo manual) |
| W4-PR-03 | Confirmación expresa de I-64 en su chat; aviso a I-52; nadie usando el equipo |
| W4-PR-04 | `RAMElements.exe` = 0 y `acad.exe` = 0; identidades de RAM, API y bases de §1 |
| W4-PR-05 | `pkg-OV02` con sus tres archivos y sin `ram-readback.json`; carpetas nuevas `dump-OV02-gen` y `dump-OV02-man` con sus scripts (`dump-script --adapter-version 1.0.0`), iguales a la plantilla de P2 salvo `dumpFolder`, sin `ram-dump.json` |
| W4-PR-06 | Copia de `Config.ini`; K1 |

### 4.2 Pasos y oráculos

| # | Paso | Oráculo / evidencia |
|---|---|---|
| W4-01 | Abrir RAM (una instancia); modelo nuevo vacío; `session_custody.ps1 -Phase session-start` | una sola `RAMElements.exe`; identidad de RAM y API; PID e inicio registrados |
| W4-02 | `pre-run` del script de `pkg-OV02` (ruta y SHA-256); «Ejecutar archivo…» (doble clic en el icono de cada carpeta; capturar solo la barra de ruta y la lista de archivos) | consola `READBACK OK` y `AnalysisInputFingerprint` = manifiesto; `post-run` de `ram-readback.json` (tiempos, SHA-256, estado y huella) |
| W4-03 | **Misma sesión y mismo modelo:** `pre-run` y ejecución de `dump-OV02-gen\rackcad-ram-dump.py` | `DUMP OK`; `post-run` del volcado; `combinations` [] |
| W4-04 | Descartar sin guardar (File > New → No); modelo nuevo vacío | «Elementos: 0» |
| W4-05 | **Modelo manual nuevo** desde la hoja `ov-i65-02-manual-entry-sheet.json` (preparación §5): 8 nudos, apoyos, 7 miembros con sección, material y giro, caso `REF1` (`DL`), seis cargas puntuales en % y kip; revisión contra la hoja (una corrección controlada, como en P2) | rejillas conformes a la hoja; ningún valor tomado del paquete ni de la lectura de vuelta |
| W4-06 | `pre-run` y ejecución de `dump-OV02-man\rackcad-ram-dump.py` | `DUMP OK`; `post-run`; `combinations` [] |
| W4-07 | Descartar sin guardar; cerrar RAM; `session-end` | `RAMElements.exe` = 0 |
| W4-08 | Fuera de RAM: `compare --package pkg-OV02 --generated dump-OV02-gen\ram-dump.json --manual dump-OV02-man\ram-dump.json --report <nuevo>` **sin** `--*-unread` (CMB-3) | **PASS (0)**; fila `LOADS` PASS (valor 0.1 %, posición 0.02 in, dirección exacta); el resto de filas como en OV-I65-01 (propiedades NOT_EVALUATED, D-09; categoría manual y peso propio INFORMATIVE) |
| W4-09 | Contraste `readback_dump_contrast.py` entre `ram-readback.json` y el volcado del generado | **IGUAL** |
| W4-10 | Comprobación de la cadena: inicio de RAM ≤ creación de la lectura de vuelta ≤ creación del volcado del generado ≤ cierre, mismo PID | coherente (evidencia operativa, **no** certificación criptográfica) |
| W4-11 | POST (identidades, bases, `Config.ini`); K2; aviso de cierre | sin cambios de identidades; K1/K2 sin cambios |

### 4.3 Q2 en el extremo (FRACTION 1.0 = RETAIN)

El producto emite la Q2 al 99.99999999999997 % (comprobación en seco); la hoja manual la introduce como 100.000000 %. La diferencia de
representación (≈ 2.8e-14 % frente a 100 exacto, ≈ 1.2e-14 in) está muy por debajo de la tolerancia de posición (0.02 in) y no se corrige
(decisiones CD-I65-F5-08). **STOP de la validación** si RAM rechaza la carga, la desplaza fuera de la tolerancia, la lee en otro
miembro o con otro valor, en el generado o en el manual; se registra como hallazgo, sin cambiar el algoritmo ni las tolerancias.

### 4.4 Presupuesto y paradas

Una ventana de ~2 h; 1 ejecución del script de construcción, 2 volcados; 1 corrección controlada del modelo manual; 0 reintentos del
generado; 0 Codex; ningún modelo guardado; sin análisis. **STOP:** condiciones de entrada; identidad distinta; licencia o diálogo no
previsto; script con otra ruta u otro hash; salida que ya existía; `READBACK` distinto de OK; `DUMP` distinto de OK; comparador ≠ 0; Q2
(§4.3); contraste DISTINTO; pegado o renombrado accidental que altere algo fuera del modelo; un dato identificable no recortable.

## 5. Matriz de aceptación AC-F5-01..10

| # | Criterio | Evidencia | Estado al preparar |
|---|---|---|---|
| AC-F5-01 | Orden del Owner con alcance (solo OV-I65-02) y presupuesto (Codex 50/52, `attempts` 7/9, 0 Workers) | decisiones §85-§87 | CUMPLE |
| AC-F5-02 | Freeze y ADR-0050 sin cambios; sin A-n | blobs `4e6e1feb…` y `c18def42…` | CUMPLE (se verifica en el Candidato) |
| AC-F5-03 | Candidato de hito de F5 completo (LIFECYCLE §8) y DLL según la guía | evidencia del Candidato; declaración del Owner | PENDIENTE (preparación en curso; declaración del Owner) |
| AC-F5-04 | `pkg-OV02` `Valid`, 16/16, carga útil = tabla (6/6) | W3 | PENDIENTE (W3) |
| AC-F5-05 | OV-I65-02 PASS: `READBACK OK` con la huella; volcados `DUMP OK`; comparador PASS con `LOADS` en PASS; `manualDumpSha256` del modelo manual nuevo; Q2 reproducida | W4 | PENDIENTE (W4) |
| AC-F5-06 | Cadena de custodia de la opción 0 completa; contraste IGUAL | W4 | PENDIENTE (W4) |
| AC-F5-07 | Evidencias conformes a C-4 (sin datos identificables) | revisión mecánica y visual | CUMPLE en la preparación; PENDIENTE en W3/W4 |
| AC-F5-08 | Producto sin cambios; ningún modelo de RAM guardado; sin análisis | diff del Candidato; W4 | CUMPLE en el Candidato; PENDIENTE en W4 |
| AC-F5-09 | Revisión del Coordinator y del Architect sin REQUIRED | conformidades | PENDIENTE (revisión del Candidato y revisión de cierre) |
| AC-F5-10 | LIFECYCLE §7: commit de cierre → árbol limpio → Core Full local → push → CI → GATE PASS registrado después | cierre de F5 | PENDIENTE (después de W4) |
