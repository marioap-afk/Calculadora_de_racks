# I-65 — F4: informe de cierre PARCIAL de la parte implementable

> **Clasificación: F4 INCOMPLETE / BLOCKED.** F4 queda detenida en su estado parcialmente implementado por decisión del Owner
> («Owner Decision: Option C and Checkout Incident Containment», texto literal en
> [I-65-owner-decision-f4-option-c.original.txt](../automation/decisions/I-65-owner-decision-f4-option-c.original.txt)). **No es un cierre
> satisfactorio**, no hay GATE PASS de F4 ni Candidato de hito, y **IMPLEMENTATION AUTHORIZATION = NO**: la continuación de F4 requiere una nueva
> decisión del Owner. En el estado canónico (`docs/automation/state/I-65.yml`) esto se expresa con `state: waiting` y el Owner como siguiente
> rol, que son los valores que admite AUTOMATION_PLAN; este documento es la fuente de la clasificación.

Fecha: 2026-10-12. Rama `architecture/interop-analisis-estructural` (no integrada), base `bb0d5522`. Código de F4 en `7efacd78` (T1-T4);
último commit de la sesión antes de este informe: `1b518628` (CI `38179661148` 4/4 `success`).

## 1. Estado de las tareas

| Tarea | Alcance | Cadena (RED → GREEN) | `VerifiedSha` y verificación | CI | Pruebas | Estado |
|---|---|---|---|---|---|---|
| F4-T1-PACKAGE-HARDENING | A65-F3-01, 02, 04, 05, 06, 07, 08, 09, 11 y T4-21: puertas de la plantilla, giro módulo 360, escritura robusta, regla de carpeta | RED `53bfcd94` (CI `38154104233` Core `failure`), GREEN `a6fa5a35` (CI `38154505343` Core `failure`: 13 635/13 636 por una aserción defectuosa del RED), corrección de prueba `f5c6b885` (DEV-I65-F4-02) | `f5c6b885` — `R20261011T164003Z-6ecb` `EXECUTION_VERIFIED`, 14/14 | `38156339767` 4/4 `success` | Core 13 637/13 637; filtro 154/154 | **VERIFICADA** |
| F4-T2-SOURCE-ENTRY | Entrada de una sola lectura con decodificador inyectado; A65-F1-03, 08, 09, 10, 11 y 12 | intento 1 sin commits (STOP), corrección: RED `2e92a0df` (CI `38161386081` Core `failure`), GREEN `0070ae40` | `0070ae40` — `R20261011T180413Z-2afc` `EXECUTION_VERIFIED`, 14/14 | `38161863583` 4/4 `success` | Core 13 703/13 703; filtro 66/66 | **VERIFICADA** |
| F4-T3-DUMP-AND-READERS | Script de volcado de solo lectura, lectores estrictos (volcado, lectura de vuelta, manifiesto) y tipos neutrales de lectura | RED `a283e0cc` (CI `38166596013` Core `failure`), GREEN `76db7642` | `76db7642` — `R20261011T195706Z-dc0b` `EXECUTION_VERIFIED`, 14/14 (con la disposición del Owner sobre C-08) | `38166926118` 4/4 `success` | Core 13 997/13 997; filtro 294/294; ida y vuelta REF-CANT-01 ≤ 3.55e-15 | **VERIFICADA** |
| F4-T4-COMPARATOR-AND-TOOL | Comparador neutral `CompareReadbacks` e informe determinista; herramienta de consola `tools/RackCad.StructuralAnalysis.RamTool` | intento 2: RED `b51adf28`, GREEN `0e9bbc29` (STOP); corrección: RED `c4660703` (CI `38175189302` Core `failure`), GREEN `7efacd78` | `7efacd78` — `R20261011T211738Z-74b4` `EXECUTION_VERIFIED`, 14/14 | `38175418046` 4/4 `success` (incluida la solución en Release sin advertencias) | Core 14 078/14 078; filtro 81/81 | **VERIFICADA** |
| F4-T5-PLUGIN-COMMAND | Comando `RACKRAMPAQUETE` del Plugin y distribución de la tabla en la salida de compilación | sin commits (STOP del Worker: C-08 y C-05) | — | — | — | **DETENIDA** (S-11; sin nuevas delegaciones por decisión del Owner) |

UI: 1 637 ejecutadas y superadas de 1 654 en cada CI (17 pruebas con atributo Skip).

**Controles negativos** (una ejecución con salida válida por cadena): T1 nc1 detectado en `Handoff` (DEV-I65-F4-03), nc2 y nc3 cumplidos; T2
los tres cumplidos; T3 nc1 y nc3 cumplidos, **nc2 no** (DEV-I65-F4-08, compensado con la comprobación mecánica de alcance); T4 nc1 detectado en
`Handoff` (DEV-I65-F4-13), nc2 y nc3 cumplidos; nc4 cumplido en las cinco primeras delegaciones.

## 2. Consumo

- **Codex:** 28 de 30 (tope del Owner, 28 hasta su disposición de F4-T3): T1 7 (planificación, verificación con REWORK, planificación y
  verificación de la corrección, nc1-nc3); T2 6; T3 7 (planificación, una verificación incoherente, una invalidada por P-08, la válida, nc1-nc3);
  T4 7 (planificación, una planificación de corrección rechazada, la reemitida, verificación, nc1-nc3); T5 1 (planificación).
- **`attempts`:** 3 de 3 (correcciones de T1, T2 y T4).
- **Worker:** 8 invocaciones de subagente en F4 (T1, T1 corrección, T2, T2 corrección, T3, T4, T4 corrección y T5).
- **Architect:** 1 revisión de T1-T4 (subagente, sin Codex).

## 3. Desviaciones del protocolo (detalle en `docs/automation/decisions/I-65.md` §27-§38)

| Clase | Registros |
|---|---|
| **Violación con consecuencias reales** | DEV-I65-F4-16: el Worker de F4-T4 intento 2 reescribió el `RackCad.sln` del checkout principal, sin declararlo ni ser detectado; contenido sin restaurar ([evidencia del incidente](../automation/evidence/I-65-f4/incident-main-checkout/README.md)) |
| Violaciones transitorias o fuera de alcance sin residuo | DEV-I65-F4-15 (`placeholder.txt` en el checkout principal, borrado en 3 s); DEV-I65-F4-18 (borrador del comando de T5 escrito en el área temporal de la sesión; edición de T1 corrección por una ruta que pasaba por el checkout principal y resolvía dentro del worktree) |
| Infracciones de procedimiento sin efectos observados | Intérprete Python: DEV-I65-F4-01 (T1), DEV-I65-F4-05 (T2 corrección), DEV-I65-F4-06 (T3, aceptada por el Owner solo para esa ejecución), DEV-I65-F4-14 (T5), y DEV-I65-F4-17 (dos en F1 descubiertas ahora); en F3, CD-I65-F3-12 |
| Desviaciones de protocolo declaradas | DEV-I65-F4-02 (corrección de prueba sin RED), DEV-I65-F4-04 y DEV-I65-F4-09 (STOP del Worker aplicado sin verificación del Controller), DEV-I65-F4-11 (C-07 abreviada por el Controller en la delegación aceptada de la corrección de T4) |
| Errores de la sesión | DEV-I65-F4-07 (P-08: lectura del worktree durante una cesión; verificación invalidada y repetida); DEV-I65-F4-10 (hash en mayúsculas en un prompt; primera evaluación de A8 sin `ChainBaseSha`) |
| Fallos de los controles | DEV-I65-F4-03 y DEV-I65-F4-13 (nc1 en `Handoff`), DEV-I65-F4-08 (nc2 no discriminó `Scope`); salida incoherente del Controller en la primera verificación de T3 (README §8) y transcripción corrompida en una planificación (CD-I65-F4-19) |

## 4. Observaciones pendientes del Architect (revisión de T1-T4, CONFORMANT WITH OBSERVATIONS, ningún REQUIRED)

Pendientes de una autorización posterior (decisión del Owner): **A65-F4-01** (métrica del emparejamiento de nudos: euclídea ≤ 0.02 in con
prueba diagonal), **A65-F4-03** (ligar el volcado del generado a la lectura de vuelta y rechazar entradas iguales), **A65-F4-04** (`compare` sobre
paquetes de inyección), **A65-F4-05** (categoría vacía en casos sin cargas) y **A65-F4-06** (ensayo general con un volcado manual realista). Quedan
como limitaciones o mejoras: A65-F4-02, 07, 08, 09, 10 y 11
([revisión](../automation/evidence/I-65-f4/architect-f4-t1-t4-conformance.md)).

## 5. Estado de los mapeos de secciones y materiales

- `assets/structural-analysis/ram-elements/section-material-map.json`: dos filas, **ambas `proposed`**, con `expectedMaterial`,
  `verifiedBy` y `verifiedOn` nulos: `AISC-W-W12X26` → `W 12X26` / `ASTM-A992-GR50` → `A992 Gr50`, y `AISC-HSS-RECT-HSS4X4X_250` →
  `HSS_SQR 4X4X1_4` / `ASTM-A500-GRC` → `A500 GrC rectangular`. Con filas `proposed`, el constructor del paquete responde `Unsupported`.
- Fuente provisional (OD-F4-03): la base instalada de RAM, leída en pasivo; A, Ix, Iy y J de las dos secciones coinciden con el catálogo AISC
  v16; E = 29 000 ksi y ν = 0.3. Peso unitario de RAM 0.28179435 lb/in³ = 486.94 pcf, **−0.62 % frente a 490 pcf: reconocido, NO aprobado**
  (OD-F4-04); las tolerancias no se alteraron. Ninguna fila está verificada por un ingeniero (responsable, fecha y fuentes pendientes).
- La distribución de la tabla con el Plugin era parte de T5: **no se hizo**.

## 6. Condiciones necesarias para completar F4

1. Decisión del Owner sobre la restauración del `RackCad.sln` del checkout principal (procedimiento preparado, no ejecutado).
2. Controles mecánicos del entorno del Worker en I-62 (o un mecanismo alternativo aprobado por el Owner) antes de delegar más implementación
   ([paquete de escalamiento](../automation/evidence/I-65-f4/i62-escalation-package.md)).
3. Nueva autorización de implementación con presupuesto de Codex e intentos para: T5 (comando del Plugin y distribución de la tabla) y las
   observaciones A65-F4-01, 03, 04, 05 y 06.
4. Revisión del Architect de T5 y de esas correcciones; cierre de la parte implementable con la suite Core completa y la CI del SHA exacto.
5. Validación de ingeniería: las dos filas verificadas por un ingeniero (responsable, fecha y fuentes), y resolución del −0.62 % de peso unitario
   antes de cualquier análisis que dependa del peso propio.
6. Referencia manual REF-CANT-01 preparada según el paquete de validación de ingeniería §2.
7. Paquete generado y validado por pruebas automatizadas; plan de ejecución y condiciones de parada confirmados.
8. Autorización específica del Owner para la primera corrida en RAM Elements; la decisión sobre F4 solo tras la comparación real.

## 7. Confirmación: RAM Elements no se ejecutó

Durante F4 ningún participante ejecutó RAM Elements ni los scripts generados (construcción o volcado). Comprobación mecánica sobre las 26
transcripciones de subagentes de la unidad y los 28 registros de eventos de Codex de F4
([ram-execution-scan.json](../automation/evidence/I-65-f4/incident-main-checkout/ram-execution-scan.json)): ninguna ejecución de
`RAMElements.exe` ni de `rackcad-ram-build.py` o `rackcad-ram-dump.py`; las únicas coincidencias con el ejecutable (2026-10-10, fase de
descubrimiento) son lecturas de sus bytes con `ReadAllBytes`. La fuente instalada del paquete del programa solo se leyó en pasivo. Las
invocaciones de un intérprete de los Workers fueron sondas de versión o de impresión y, en F3, una lectura de `p1-report.json`; ninguna ejecutó
scripts del producto ni el programa.
