# I-65 — Propuesta de plan para READY final (READY-01..09)

- **Estado:** PROPUESTA del Coordinator (orden del Owner §99, CD-I65-F7-04). **No** declara READY, **no** fija `FINAL_CANDIDATE_SHA` y
  **no** integra. Toda ejecución exige una orden expresa del Owner.
- **Autoridades:** [INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §8-§9, [WORKFLOW](../WORKFLOW.md) §11.5,
  [AGENTS.md](../../AGENTS.md) («Pruebas — definición de terminado»), Freeze [Proposal V3.1](I-65-proposal-v3.1.md) §5 y
  [guía manual](../guias/validacion-manual-autocad.md).
- **Punto de partida:** F1, F3, F4, F5 y (al cerrar) F7 con GATE PASS; producto = árbol de `51fc82f1`; los Candidatos de hito
  `5a5ca577` (F4) y `bd1fcf8a` (F5) **no** sustituyen al Candidato final (LIFECYCLE §8).

## 1. Decisiones del Owner antes de iniciar READY

| ID | Decisión | Opciones | Recomendación del Coordinator |
|---|---|---|---|
| R-D1 | **Orden de integración con I-64** (`architecture/workspace-persistente-rackcad`, `09f2f1c3`, sin actividad desde 2026-10-13). Archivos tocados por ambas ramas desde `bb0d5522`: `docs/ROADMAP.md` (inserciones separadas; fusión probablemente limpia, no verificada), `tests/RackCad.Tests/CustomPropertiesCommandGuardTests.cs` y `SelectiveEditorOpenTests.cs` (mismos fragmentos: `Baseline` y el censo 38 → conflicto textual seguro). Comprobación del Coordinator del 2026-10-28 con `git merge-tree --write-tree --name-only <rama> HEAD` (no mueve refs ni el árbol de trabajo, pero escribe objetos en la base de datos de Git): conflicto en esas dos pruebas. I-62 e I-52: solo `docs/ROADMAP.md` en común, sin conflicto | (a) I-65 integra primero; I-64 concilia al integrar. (b) I-64 primero; I-65 rebasa en READY-04 y concilia dos pruebas (cambio de código → Worker, Codex y `attempts`, SHA nuevo) | **(a)**, salvo que el Owner reactive I-64 antes; evita consumir Codex |
| R-D2 | **Referencia manual** para OV-I65-01 y 02 sobre el Candidato final | A: reutilizar los volcados manuales custodiados (`c4a98fb0…` de OV-01, `12c89479…` de OV-02) si RAM, su API, sus bases y la plantilla de volcado no cambian. **Es una reducción de OV-I65-01 y 02 que el Freeze §5 reserva al Owner**, y la estrategia A se aprobó **solo para F4** (CD-I65-F4-220). B: guardar modelos manuales de referencia (exige autorizar guardar y abrir modelos de RAM). C: reconstruir a mano | **A con caída a C** si cambia cualquier identidad, **solo con autorización expresa del Owner para READY**; sin ella, C. Con A, decidir también A65-F5F-08 (abajo) |
| R-D3 | **A65-F4F-04** (volcado ligado a la sesión) | opción 0 (registro operativo de sesión, sin código) u opción 1 (huella o nonce en el volcado: A-n y código) | **opción 0** como en F5, declarada como evidencia operativa |
| R-D4 | **A65-F5F-08** (independencia del operador del modelo manual) | operador distinto (ingeniero) o conservar la comprobación de firma numérica contra la hoja | depende de R-D2: con A no aplica (los volcados ya existen); con C, operador distinto si está disponible y, siempre, la comprobación de firma |
| R-D5 | **A65-F5F-03/04/05** (reglas de procedimiento) | fijarlas en el procedimiento de READY | un solo método para los árboles de bases de RAM; contraste lectura de vuelta ↔ volcado después del cierre de RAM; proceso de RAM nuevo por ventana y cierre de RAM tras un STOP de licencia |
| R-D6 | **A65-F4F-08** (codificación del script de captura del host) | corregir la herramienta del Coordinator (fuera del producto) antes de la próxima ventana de AutoCAD o mantener la conciliación por página de códigos | corregir la herramienta (SDDL de `Get-Acl` o UTF-8 fijo), registrar su hash nuevo y medir BASELINE y POST por el mismo camino |
| R-D7 | **C-5** (exposición de información de terceros en evidencia histórica) | sin acción, notificación o redacción hacia delante | decisión reservada al Owner; no bloquea READY técnicamente salvo que el Owner lo disponga |
| R-D8 | **F7-OBS-01** (`RACKAYUDA` sin `RACKRAMPAQUETE`) | seguimiento en `docs/ideas-futuras.md` o cambio de código autorizado antes del Candidato final | seguimiento (no lo exige el Freeze; evita SHA nuevo y consumo) |
| R-D9 | **OV-I65-08** (round-trip tras reabrir el DWG) | autorizar guardar y reabrir un DWG desechable en la carpeta de la corrida | autorizar, con el DWG desechable fuera del repositorio |
| R-D10 | **Presupuesto** | ver §5 | sin ampliar si se eligen R-D1 (a) y R-D3 opción 0 |
| R-D11 | **Tabla de secciones y materiales en el Autoloader** (CD-I65-F4-10, diferido): hoy solo viaja en la salida de compilación del Plugin | mantener el diferimiento como limitación de V1 o autorizar el cambio de `deploy/verify-bundle.ps1` y del paquete (cambio fuera de `docs/`, SHA nuevo) | mantener el diferimiento, declarado en la guía y en la entrada de FOUNDATIONS |

## 2. Secuencia READY-01..09 para I-65

| Paso | Contenido para I-65 |
|---|---|
| READY-01 | alcance congelado completo; F6 fuera (Q-O5); diferimientos (A65-F4R-07, DIF-01, edición del catálogo de RAM, F7-OBS-01, tabla fuera del Autoloader CD-I65-F4-10) decididos por el Owner; sin A-n pendiente |
| READY-02 | gates F1, F3, F4, F5 y F7 cerrados; decisiones R-D1..R-D11 versionadas **antes** de READY-04 (la lectura de commits documentales hijos no vale para READY) |
| READY-03 | sin REQUIRED abierto, EXP-01 A ni decisión material pendiente |
| READY-04 | fetch, preflight y rebase final según WORKFLOW (con R-D1 (a) y `main` sin avanzar: sin rebase; si `main` avanzó: rebase y evidencia nueva); comprobar el número 0050 del ADR contra `main` |
| READY-05 | focales y relevantes del SHA resultante y su CI propio con los 4 jobs requeridos |
| READY-06 | conformidad completa del Architect y del Coordinator (`CONFORMING`) de ese SHA contra Freeze, sin A-n; revisión de las entradas de FOUNDATIONS redactadas |
| READY-07 | árbol limpio, HEAD identificado, sin cambios pendientes de producto |
| READY-08 | matriz OV completa: OV-I65-01..07 asignadas al SHA final, OV-I65-08 preparada, ninguna retirada sin el Owner |
| READY-09 | identidad e inmutabilidad del Freeze, diff permitido y ausencia de A-n comprobados |

Solo entonces se fija `FINAL_CANDIDATE_SHA`.

## 3. Evidencia sobre `FINAL_CANDIDATE_SHA`

- Core Full y UI Full locales, builds Debug de UI y Plugin (DLL con su ProductVersion `1.0.0+<sha>` y salida en una carpeta nueva),
  herramienta compilada del mismo SHA y CI exacto con `ui-tests`.
- **OV-I65-01..07 repetidas** (Freeze §5) con paquetes nuevos generados por el DLL final: ventana de AutoCAD (OV-01, 02, 03, 04 y la
  generación de inyecciones) y ventana de RAM (OV-01, 02, 04..07), con la custodia de la opción 0 y los controles K1/K2.
- **OV-I65-02 repetida** con el mismo archivo de cargas (`d675d9f6…`).
- **OV-I65-08:** lista de la guía manual (NETLOAD y menú, dibujo, actualización en sitio, round-trip tras reabrir el DWG, multivista,
  BOM, persistencia y legacy, escenarios de I-65), con la confirmación explícita del validador e identidad de AutoCAD y de la biblioteca
  de bloques.
- Custodia: cada fila de hash comparada con los blobs antes de cada commit de evidencia; evidencia nueva conforme a la política de §90.

## 4. Integración (fuera de este plan de ejecución; requiere otra orden)

Tras READY y la Owner Validation: commit de cierre de la rama con HANDOFF §8-12, ROADMAP, índice de ADR (ADR-0050), entradas de
FOUNDATIONS ya conformadas y evidencia final; merge serializado según WORKFLOW §4.5 y §11.5; CI del merge con su cobertura; limpieza solo
con ambas coberturas.

## 5. Presupuesto propuesto

| Recurso | Con R-D1 (a) y R-D3 opción 0 | Si R-D1 (b) | Si R-D3 opción 1 |
|---|---|---|---|
| Codex (hoy 50 de 52) | 0 | +1 (conciliar dos pruebas) | +1-2 y A-n |
| `attempts` (hoy 7 de 9) | 0 | +1 | +1 |
| Workers | 0 | 1 | 1 |
| Architect | READY-06 (+1 relectura si surge un REQUIRED corregible) | igual | igual |
| Host | AutoCAD 2-2.5 h; RAM 1.5-2 h con R-D2 A (3 h o más con C) | igual | igual |

Con las opciones recomendadas, el presupuesto vigente basta. Cualquier ampliación (p. ej., Codex 56 y `max_attempts` 11) se decide solo si
el Owner elige R-D1 (b) o la opción 1 de R-D3.

## 6. Riesgos

- La licencia de RAM depende del inicio de sesión del Owner en Bentley CONNECTION; una ventana de RAM puede quedar bloqueada.
- Un cambio de RAM, de su API o de sus bases invalida la reutilización de volcados (R-D2 A) y obliga a C.
- Si `main` avanza antes de READY-04, el rebase crea un SHA nuevo y reinicia READY-02.
- Las limitaciones del hito siguen vigentes en READY: equivalencia de transferencia, no validación estructural; propiedades
  NOT_EVALUATED (D-09); peso propio excluido; DIF-01; edición del catálogo de RAM UNKNOWN; A65-F4R-07.
