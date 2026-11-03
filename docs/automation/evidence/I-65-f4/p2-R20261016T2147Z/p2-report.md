# I-65 F4 — P2: primera corrida real en RAM Elements (custodia, 2026-10-16)

- **Autoridad:** «OWNER AUTHORIZATION / P2 — First Real RAM Elements Transfer and Equivalence Test»
  ([texto literal](../../../decisions/I-65-owner-authorization-p2-ram-first-run.original.txt)); decisiones §72-§73. RAM EXECUTION =
  AUTHORIZED FOR P2 ONLY; ONE AUTOMATIC BUILD; ONE MANUAL REFERENCE BUILD (OPERATOR = CLAUDE WITH COMPUTER USE); ONE CONTROLLED MANUAL
  CORRECTION IF NEEDED; STRUCTURAL ANALYSIS = NO; F4 GATE PASS = NO. Es una validación experimental previa al Candidato de hito.
- **Ejecutor:** Coordinator con Computer Use (acceso a «RAM Elements 2026», nivel completo, sin permiso de portapapeles). Corrida local
  `D:\r65-smoke\51fc82f1\run-R20261016T1918Z\p2-R20261016T2147Z\`. Procedimiento: [P2](../../../../initiatives/I-65-f4-p2-ram-first-run-procedure.md).

## 1. Horas (UTC)

| Hito | Hora |
|---|---|
| Custodia de la autorización (`37fc388b`) y K1 | ~21:47Z |
| Preflight (identidades, copia de `Config.ini`, herramienta congelada, scripts de volcado) | 21:47:27Z |
| I-64: sin BASELINE/POST en curso ni prevista en 2 h, sin objeción; aviso de cortesía a I-52 | ~21:48Z |
| RAM abierto (`RAMElements.exe` 0 → 1; `acad.exe` 0) | 21:48:54Z |
| SHA-256 del paquete justo antes de ejecutar; `ram-readback.json` inexistente | 21:51:10Z |
| `READBACK OK` (`ram-readback.json` escrito) | 21:51:18Z |
| Volcado del generado `DUMP OK` | ~21:56:58Z |
| Generado descartado sin guardar | ~21:57:30Z |
| Modelo manual construido | ~21:58Z-22:16Z |
| Volcado del manual `DUMP OK` | ~22:16:30Z |
| Manual descartado sin guardar; RAM cerrado (`Config.ini` reescrito a las 22:17:16Z) | ~22:17:10Z-22:17:20Z |
| POST: `RAMElements.exe` = 0, `acad.exe` = 0 | 22:17:34Z |
| K2 sin hallazgos; avisos de cierre a I-64 e I-52 | ~22:18Z |
| Comparador | 22:18:12Z |

## 2. Identidades ([preflight](preflight.txt), [POST](post.txt))

`RAMElements.exe` `7dcb48555741fa368f5d5f6a14e760fe0662dd5c0b3cc53f6e88f1c0231e7970` (26.00.01.15) antes y después; `ramelements` 0.1.0,
árbol `f71a017f49573030219a4213465c9ec454d3b6282f3f60b648a4300bfeba0a60` (30 archivos). Licencia en la barra de estado: «Licencia:
Practicante» ([captura 02](captures/02-barra-estado-licencia-elementos-0.jpg)). Herramienta de F4 **congelada** en la corrida
(`ramtool-ad595b42`, copia idéntica a `bin\Debug\net8.0`, `1.0.0+ad595b42…`, `.exe` `01869e1b…1cb15`). Scripts de volcado generados con
ella: `dump-gen` `d30c9802…ed0e` y `dump-man` `af262489…7f29`. Solo difieren de los verificados en M-05c en la carpeta embebida (línea
11), así que CMB-3 sigue vigente.

## 3. Nivel 1 — construcción automática

Modelo nuevo y vacío («Elementos: 0», un solo caso `CM`; [captura 01](captures/01-ram-modelo-nuevo-vacio.jpg)). El diálogo «Ejecutar
archivo…» se abrió en la carpeta de M-05c, de donde no se ejecutó nada; se navegó hasta `pkg-01`
([capturas 03-04](captures/04-script-del-paquete-seleccionado.jpg)). **Una sola** ejecución de `rackcad-ram-build.py`:

```text
--- Ejecutando script ---
READBACK OK
AnalysisInputFingerprint 505941e2ca42a02dd85b72629579bcd27e4900f80c69ebd41bb8ea73c0a5ba61
--- Finalizado ---
```

«Script ejecutado con éxito»; «Elementos: 15» (8 nudos y 7 miembros); estado `REF1`
([captura 05](captures/05-consola-readback-ok-huella.jpg)).

**Inspección visual** (solo mirar):
- en vista lateral, columna vertical y tres brazos hacia −Z de RAM con la pendiente prevista;
- tres cargas hacia abajo, a ~58 %, ~36 % y ~80 % de los brazos 1, 2 y 3, la mayor en el brazo 2 ([captura 06](captures/06-generado-cargas-vista-lateral.jpg));
- ejes locales: en las columnas, eje 1 hacia arriba y eje 2 hacia −Z (hacia los brazos); en los brazos, eje 1 hacia la punta y eje 2 hacia arriba ([captura 07](captures/07-generado-ejes-locales.jpg));
- hoja de miembros: `HSS_SQR 4X4X1_4` / `A500 GrC rectangular` / giro 0 en los brazos y `W 12X26` / `A992 Gr50` / −90 en las columnas ([capturas 08-10](captures/08-generado-secciones.jpg)).

## 4. Nivel 2 — lectura de vuelta y volcado independiente

- [`ram-readback.json`](ram-readback.json) `e622f429…1511`, en la carpeta del paquete: `status` `READBACK OK`, `differences` vacío,
  `adapterVersion` 1.0.0, la misma huella; 8 nudos, 7 miembros y 3 cargas con sus valores; caso `REF1` `OTHER` con peso propio
  (0, 0, 0). Los tres archivos originales del paquete conservan sus SHA-256 (`19fba962…`, `d9b41aab…`, `25f48e7f…`), tamaños y fechas
  (19:25:15Z); el único archivo nuevo de la carpeta es `ram-readback.json`.
- [Volcado del generado](generated-ram-dump.json) `f5789d1c…4be0`, en la misma sesión y sin guardar ni reabrir (A65-F4R-02): `DUMP OK`,
  unidades `LB_IN` (fijadas por el script), 8 nudos, 7 miembros, casos `CM` (DL, sin cargas) y `REF1` (`OTHER`, 3 cargas), peso propio
  0, **`combinations` []**, `errors` [] ([captura 14](captures/14-consola-dump-ok-generado.jpg)).
- Descartado con File > New → «¿Desea salvarlo?» → **No** ([captura 15](captures/15-descartar-generado-no.jpg)).

## 5. Construcción manual de `REF-CANT-01` (Claude con Computer Use)

- **Fuente:** solo la especificación congelada. La [hoja de entrada](manual-entry-sheet.json) se generó mecánicamente
  ([script](p2-manual-entry-sheet.py.txt)) a partir de las tablas del [procedimiento manual](../../../../initiatives/I-65-f4-ref-cant-01-manual-procedure.md),
  sin leer el paquete, la lectura de vuelta ni el volcado del generado.
- **Unidades (desviación documentada del paso 2.2 del procedimiento manual):** el diálogo «Configurar unidades» no indica que el cambio
  se limite al modelo. Para **no modificar deliberadamente la configuración** (disposición §7), se canceló sin cambios
  ([captura 16](captures/16-unidades-sin-cambios-cancelado.jpg)) y se tecleó en las unidades vigentes de la interfaz («Inglés»: ft y kip).
  Los valores son conversiones exactas de la especificación (in/12 con 8 decimales y lbf/1000), lo que da un error de tecleo inferior
  a 1e-7 in, muy por debajo de la tolerancia de 0.02 in. El volcado lee en `LB_IN` y restauró la unidad inicial (`KIP_FT` → `LB_IN` →
  restaurado).
- **Entrada en la interfaz:**
  - **Nudos:** 8 con las coordenadas de la especificación. La descripción `COL.FOOT` en el nudo 1 y las demás en blanco: es solo
    sugerida, porque el comparador empareja por posición ([captura 19](captures/19-manual-nudos.jpg)).
  - **Apoyos:** `COL.FOOT` con TX TY TZ RX RY RZ; los otros siete con TX RY RZ ([captura 20](captures/20-manual-apoyos.jpg)).
  - **Miembros I → J:** columnas 1→2, 2→3, 3→4, 4→5; brazos 2→6, 3→7, 4→8 ([captura 21](captures/21-manual-conectividad.jpg)).
  - **Secciones, materiales y giros:** `W 12X26` y `A992 Gr50` con giro −90 en las columnas; `HSS_SQR 4X4X1_4` y `A500 GrC rectangular`
    con giro 0 en los brazos ([capturas 22-24](captures/22-manual-secciones.jpg)). Los nombres se teclearon en la celda: no se cambió el
    grupo del selector de secciones ni el de materiales.
  - **Liberaciones:** ninguna, por omisión.
  - **Caso `REF1`:** descripción «REF manual», categoría `DL` (la interfaz no ofrece «Other»), **sin combinaciones**
    ([captura 25](captures/25-manual-estados-de-carga-sin-combinaciones.jpg)).
  - **Cargas puntuales `Y` en %:** −1 kip a 57.501282 % (M5), −2 kip a 36.251923 % (M6), −1.102311311 kip a 78.750641 % (M7)
    ([captura 26](captures/26-manual-cargas-puntuales.jpg)).
- **Volcado del manual** [manual-ram-dump.json](manual-ram-dump.json) `c4a98fb0…a23b`: `DUMP OK`, `errors` [], **`combinations` []**
  ([captura 28](captures/28-consola-dump-ok-manual.jpg)). Antes de comparar se revisó contra la especificación (no contra el generado):
  - coordenadas dentro de ±4e-8 in (por ejemplo, 0.24999996 y 20.26716168);
  - banderas, secciones, materiales, giros y valores por omisión exactos;
  - cargas −1000, −2000 y −1102.311311 lbf en sus porcentajes;
  - peso propio 0.

  **Sin error de entrada identificable: no se usó la corrección controlada.**
- Descartado sin guardar ([captura 29](captures/29-descartar-manual-no.jpg)); RAM cerrado sin aviso de guardado (modelo vacío).

## 6. Nivel 3 — comparador ([informe](comparison-report.json), [consola](comparison-console.txt))

`compare --package pkg-01 --generated dump-gen\ram-dump.json --manual dump-man\ram-dump.json --report …`, **sin**
`--*-combinations-unread` (CMB-3), con la herramienta congelada. **Veredicto: PASS** (código de salida 0). Huellas del manifiesto y de la
lectura de vuelta iguales (`505941e2…`); SHA-256 de entrada registrados en el informe.

| Fila | Veredicto | Delta máximo |
|---|---|---|
| Coordenadas de nudos (0.02 in; emparejadas por posición) | PASS | 0 in (informado) |
| Conectividad I → J | PASS | — |
| Ejes locales l1, l2, l3 (0.1°) | PASS | 0° |
| Nombre de sección | PASS | — |
| Nombre de material | PASS | — |
| Propiedades de sección (A, Ix, Iy, J) | NOT_EVALUATED | hoja firmada (D-09); la API no las expone |
| Propiedades de material (E, ν o G, peso unitario) | NOT_EVALUATED | hoja firmada (D-09) |
| Punto cardinal, offsets, factores, axial, resortes | PASS | — |
| Liberaciones y apoyos | PASS | — |
| Cargas puntuales (valor 0.1 %, posición 0.02 in, dirección) | PASS | 0 % / 0 in |
| Combinaciones (ninguna) | PASS | 0 en ambos |
| Categoría del caso generado | PASS | `OTHER` |
| Categoría del caso manual | INFORMATIVE | `DL` |
| Peso propio | INFORMATIVE | (0, 0, 0) en ambos |

## 7. Nivel 4 — limitaciones y supuestos de ingeniería

- P2 demuestra la **equivalencia de transferencia** (API frente a interfaz) para `REF-CANT-01`; no es validación estructural (Q-O3) y no
  se analizó, diseñó ni leyó ningún resultado.
- Supuestos del manifiesto: `ASM-PROP-01`, `CON-01`, `IDL-NO-OFFSETS`, `LOD-01`, `SUP-01`, `SUP-02`, `SUP-03`. Exclusiones: sin base,
  placas, cartelas, separadores, arriostramiento ni redondos; **sin peso propio ni combinaciones**.
- **DIF-01** (486.94 pcf de RAM frente a 490 pcf, −0.62 %) sigue siendo informativa, solo porque no hay peso propio. La edición formal
  del catálogo de RAM sigue sin verificar (DIF-04).
- Las propiedades de sección y material se evalúan con la hoja firmada (D-09), no con el comparador.
- La construcción manual comparte con el generado el origen de los valores (especificación `[CÓDIGO]`); lo que se compara es el camino
  API frente a la interfaz, no la derivación de la geometría.

## 8. Incidencias

1. **Pegado accidental (portapapeles):** la herramienta `type` de Computer Use envió una descripción de 17 caracteres por el
   portapapeles. Como la sesión no tiene permiso de escritura en él, pegó su contenido previo (el texto de la autorización del Owner)
   en la rejilla de nudos y creó 189 elementos basura ([captura 17](captures/17-incidente-pegado-189-elementos.jpg)). Se deshizo con
   **un** Ctrl+Z, que dejó 1 elemento correcto ([captura 18](captures/18-deshacer-pegado-1-elemento.jpg)), antes de cualquier volcado.
   Desde entonces se teclearon fragmentos de ≤ 10 caracteres. `Config.ini` registra `AccHeClipboardPaste=1`. **Interpretación del
   Coordinator:** fue una corrección durante la entrada, antes de completar el modelo y de su volcado. No usé la corrección controlada,
   que reservaba para un error detectado tras el volcado y que no hizo falta. Lo decide el Owner.
2. **Edición de celdas:** las celdas no en modo edición solo conservan el último carácter tecleado. Hubo valores extraños (`T00`,
   rechazado por RAM; `30`; un `3` en una descripción), corregidos en el momento con doble clic, borrado y nuevo tecleo; todos antes del
   volcado.
3. **Tamaño de fuente:** un clic en el botón «Aumentar el tamaño de fuente» de la barra de estado; se revirtió con «Disminuir»
   ([captura 11](captures/11-incidente-tamano-fuente.jpg)). Efecto neto: `3DViewFontZoom=1` en `Config.ini`.
4. **Renombrar:** un doble clic lento en el diálogo «Abrir» activó el renombrado de la carpeta de la corrida. Se salió sin cambiar el
   nombre (comprobado en disco; [captura 12](captures/12-incidente-renombrar-cancelado.jpg)).
5. **Vista:** un arrastre orbitó la vista con el modelo generado. En el manual, el arrastre hizo una selección vacía y la órbita no se
   reprodujo. La comprobación visual del manual se limitó a la vista de frente y la sustituyeron el volcado y el comparador.
6. **Unidades** sin cambiar (§5); `frameSeccionesDataBaseName=Montilla` sin cambio.

## 9. Cambios del host ([diferencias de `Config.ini`](config-ini-diff.txt))

`Config.ini` `2505ca6e…` → `2c593957…` (22:17:16Z): contadores de uso (`acc*`, `pan*IncreaseFontSize`/`Decrease`), `PythonScripts` →
carpeta `dump-man`, sección `[DlgGeneralConfiguration]` con `3DViewFontZoom=1` (efecto neto de +1/−1) y últimos registros vistos en los
selectores (`frmInfoTables*`). Todo es estado de interfaz y se conserva (CD-I65-F4-73); no se restauró. Nada de AutoCAD. Checkouts K1/K2
sin hallazgos.
