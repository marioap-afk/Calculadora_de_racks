# I-65 — Conformidad del Coordinator: gate correctivo R-D11

- **Fecha:** 2026-10-29.
- **Rango:** `95aae3f1..bf6ef110` más el commit de cierre que contiene este documento.
- **Autoridades:**
  - orden literal del Owner ([decisiones](../../decisions/I-65.md) §105);
  - contrato del gate [I-65-rd11-autoloader-gate.md](../../../initiatives/I-65-rd11-autoloader-gate.md);
  - LIFECYCLE §7.
- **Insumo:** revisión del Architect [architect-rd11-conformance.md](architect-rd11-conformance.md), CONFORMANT WITH OBSERVATIONS, 0 REQUIRED.

## 1. Veredicto

**CONFORMING.** Fundamentos:

- **Producto:** el cambio se limita al alcance autorizado (orden §3.1-§3.6) y conserva la tabla (blob `d088e2a9`), el Freeze (`4e6e1feb`),
  ADR-0050 (`c18def42`) y el contrato de cuatro DLL de ADR-0003.
- **Protocolo:** la cadena delegada está verificada, 14/14 y nc1-nc4 con su oráculo (§106).
- **Evidencia local:** documenta las cuatro diferencias que pide la orden §6 (§107).
- **Pendiente:** el registro formal queda sujeto a la regresión y a la CI del SHA de cierre (AC-RD11-09 y 10), por el orden de
  LIFECYCLE §7.

## 2. Criterios AC-RD11-01..12

| ID | Estado | Evidencia |
|---|---|---|
| AC-RD11-01 | Cumple | Revisión de alcance en `91f82d6d`, antes de la planificación |
| AC-RD11-02 | **Cumple por sustitución** (criterio mal especificado por el Coordinator; A65-RD11-03) | Core del RED en `failure` por aserción (40797120699). El job del Plugin se omite por `needs: [tests, build-ui]`, así que nunca podía fallar en el RED. RED del harness reproducido dos veces en local (Worker y sesión): código 1 |
| AC-RD11-03 | Cumple | GREEN 40797712877 4/4; `git diff RED..GREEN` sin pruebas ni harness |
| AC-RD11-04 | Cumple | `EXECUTION_VERIFIED` 14/14; nc4, nc1, nc2 y nc3 con su oráculo |
| AC-RD11-05 | Cumple en `efe7cc06`; se repite sobre el SHA de cierre | 25 archivos; verificador 158 comprobaciones; harness 14 casos |
| AC-RD11-06 | Cumple, con la observación A65-RD11-01 | Tabla `FC3D9146…` = blob en el checkout de la sesión (LF); en la CI, `B22F7D9D…` = blob con CRLF; lector `Valid` con dos filas `verified` |
| AC-RD11-07 | Cumple | Tabla ausente, modificada, en ruta no autorizada y archivo extra: rechazados con su motivo |
| AC-RD11-08 | Cumple (equivalencia estática) | Ruta resuelta por el comando = ruta del bundle e instalación desechable con la tabla. HB-01..05 preparada y no ejecutada |
| AC-RD11-09 | **Pendiente del cierre** | Core y UI Full, builds Debug y publicación del bundle sobre el SHA de cierre |
| AC-RD11-10 | **Pendiente del cierre** | CI `push` del SHA de cierre, 4/4 |
| AC-RD11-11 | Cumple | Architect CONFORMANT WITH OBSERVATIONS sin REQUIRED; Coordinator CONFORMING |
| AC-RD11-12 | Cumple | `git diff --name-only 95aae3f1 bf6ef110`: los siete archivos del alcance, tres documentos de la unidad y `docs/automation/` (50 archivos) |

## 3. Disposición de los hallazgos del Architect (A65-RD11-01..14)

El resumen del Architect dice «13 OPTIONAL» y su tabla tiene 14. Se disponen los 14. Ninguno es REQUIRED ni se convierte en trabajo de
producto dentro de este gate.

| ID | Disposición |
|---|---|
| 01 | **Elevado al Owner (decisión).** El verificador cumple «identidad exacta contra la tabla fuente aprobada» comparando con la fuente del checkout de compilación. La identidad con los bytes del blob solo se da con LF, que en el worktree de la sesión es accidental: la CI, un worktree nuevo y `main` dan CRLF (`B22F7D9D…`). Opciones: **(a)** aceptar la lectura actual de §3.3 (contenido del blob salvo el fin de línea; sin efecto funcional ni en las huellas); **(b)** autorizar en una unidad posterior el atributo `-text` para la tabla (no cambia el blob y fija LF en toda copia). Sin decisión, rige (a) como limitación declarada (RD11-OBS-01) |
| 02 | **Corregido en este commit:** el oráculo de HB-02 compara con la fuente del checkout de compilación y registra el fin de línea; admite `FC3D9146…` (LF) o `B22F7D9D…` (CRLF) |
| 03 | **Corregido en el registro:** AC-RD11-02 «cumple por sustitución» (decisiones §108, que precisan CD-I65-RG-12 sin reescribirla) |
| 04 | Aceptado como límite del RED (la precondición del caso «valido» detiene el harness del RED). Seguimiento propuesto para una unidad posterior: una mutación del verificador sin comparación de hash debe hacer fallar «tabla-modificada» |
| 05 | Aceptado y declarado: las pruebas C# del verificador y del harness son de presencia de texto. El comportamiento lo prueba el harness en el job del Plugin de la CI, que se omite si Core o la compilación de UI fallan |
| 06 | **Para la preparación de READY** (documentos de producto, READY-02): `despliegue.md` §5 («solo DLL y catálogos») y §2. Quedaban fuera del contrato del gate (§4 de esa guía). No se tocan después de la verificación para no alterar el trabajo delegado terminado (AUTOMATION_PLAN 16.1) |
| 07 | Unidad futura autorizada: comentarios de `build-bundle.ps1` y `eng/ci/verify-autocad-references.ps1` (archivos prohibidos en este gate) |
| 08 | **Para la preparación de READY:** la guía de I-65 §10 recomendará `install-bundle.ps1 -Build` o un bundle generado con `build-bundle.ps1`, porque con `-SourceBundlePath` el instalador no ejecuta el verificador |
| 09 | Corregido en `I-65-foundations-entry.md` («el contenido del blob aprobado salvo el fin de línea»). En la guía de I-65 §2, para la preparación de READY, según la decisión de 01 |
| 10 | Anotado: `transcript-facts.json` del trabajo contiene una entrada `python_commands` que es la escritura de `worker-handoff.json` (falso positivo del heurístico laxo). El barrido estricto de K5 da 0 intérpretes |
| 11 | Sin acción: corregido por D1 / DEV-I65-RG-02 |
| 12 | Se completa en el cierre (AC-RD11-09 y 10) |
| 13 | Declarado: sondas no versionadas de la sesión, `scratchpad/i65/rd11/bundle_evidence.ps1` (SHA-256 `A0CDE570…174B`) y `scratchpad/i65/reader_check_f4d.ps1` (`FC9901DD…7012`), de solo lectura sobre el bundle (precedente A65-F4D-07) |
| 14 | Para la conciliación de READY-01..09: el diferimiento de la tabla fuera del Autoloader queda **resuelto por R-D11 B**; RD11-OBS-01 pasa a las limitaciones. Los documentos históricos no se reescriben |

## 4. Lo que el gate no afirma

- No afirma que `RACKRAMPAQUETE` funcione dentro de AutoCAD cargado por el Autoloader: eso es HB-01..05, que no está autorizada.
- No afirma ninguna validación estructural, ni cambia DIF-01, la edición del catálogo de RAM (UNKNOWN), A65-F4R-07 ni las propiedades
  NOT_EVALUATED.
- No declara READY, no fija `FINAL_CANDIDATE_SHA` y no integra.
