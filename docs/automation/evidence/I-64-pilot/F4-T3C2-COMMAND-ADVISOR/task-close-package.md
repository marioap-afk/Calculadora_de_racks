# I-64 — F4-T3C2-COMMAND-ADVISOR: paquete de cierre

Lo prepara la sesión responsable el 2026-11-13. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E49 y F4-E50.

**Autoridad del cierre:** la autorización del Owner y del Coordinator (`F4-DRAFTS/coordinator/owner-f4t3c2-authorization.md`, F4-E49).
- Fija la cadena hasta «12. custody» y «13. close» sin más aprobación del Owner para la ejecución ordinaria.
- No reabre por menores arrastrados.

Este paquete cierra una subtarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `4e34bb56740ea7bd4a53f9a11a143d0af588f2b0`, CI `43510803096` push 4/4 |
| Verificación | `R20261113T215110Z-28eb`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `3b1ee950` (la emisión del contrato); RED `730dd7da`, GREEN `4e34bb56` |
| Contrato | `gate-contract.json` (AuthorityRevision `5ef3cbec`, el paquete de F4-T3C2 v3) |
| Intentos | 1 de 3 |
| Config | Snapshot H (F4-E46) en todas las comprobaciones de la cadena |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E50; limitaciones en el §5) |
| F4-T3 | T3A, T3B1, T3B2, T3C1 y T3C2 cerradas: **F4-T3 completa en el nivel de subtareas** |

## 2. Planificación y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `1d52` | aceptada a la primera | fidelidad PASS en las ocho colecciones, A1-A8 8/8, Horizonte Medium; los cuatro paquetes de nombre casi igual fijados por índice |
| `f47e` (nc4) | con su oráculo | solo A3 en fail |
| `54b1` | entrega | Worker `claude-sonnet-5-5`/high, 264 turnos y 0 denegaciones. RED `730dd7da`: fallan 79 pruebas nuevas o cambiadas (76 de Core y 3 de UI), ninguna existente, y los censos de fuera del filtro pasan. GREEN `4e34bb56`: las 20 rutas del contrato; no toca `tests/` |
| `28eb` | `EXECUTION_VERIFIED`, 14/14 | sobre `4e34bb56`, con el RED acreditado |

**CI exacta:**
- RED `43508172794`: Core y UI en failure, como piden los dos `ExpectRed` true; el build del Plugin no corre en el RED (`needs: tests`, declarado).
- GREEN `43510803096`: los cuatro jobs en success.
  - Filtros: 2668/2668 en Core y 458/458 en UI.
  - Suites completas: Core 18206/18206; UI con 2095 pasadas, 0 fallidas y 17 condicionales ya existentes sin ejecutar.

## 3. Revisiones y controles

- **Revisión directa:** producto conforme, sin hallazgos (`direct-review.md`).
- **Revisión adversarial independiente:** ACEPTABLE CON MENORES (0/0/8) (`independent-review.md`; clasificación en `analysis.md`).
- **Controles** sobre las entradas de `28eb`, con su oráculo ordinario y conservando cada salida ordinaria:

| Control | Corrida | Resultado |
|---|---|---|
| nc1 (CurrentSha falso) | `R20261113T215510Z-e8d4` | CONTROL NOT PASSED con la firma exacta de PD-F4-04: Handoff primera en fail (la entrega contiene el SHA falso), Identity en fail con evidencia que nombra el SHA falso y el HEAD, y Ci en cascada; `EXECUTION_BLOCKED` / STOP con S-04; solo difieren consumidores del CurrentSha. Se conserva tal cual |
| sustituto de nc1 `F4T3C2-NC1-IDENTITY-SIGNAL-01` | `R20261113T215949Z-935f` | **PASS**: las 7 propiedades y la autoprueba determinista; copia del de F4-T2C con solo el identificador y el ámbito. nc1 = SATISFIED_UNDER_F4T3C2_NC1_IDENTITY_SIGNAL_SUBSTITUTE |
| nc2 (sin la primera ruta del diff en el alcance) | `R20261113T215958Z-9fe0` | CONTROL PASS: Scope única en fail, sobre `IWorkspaceLifecycleAdvisor.cs`, con F4T3C2-C01; `EXECUTION_BLOCKED` / STOP. PD-F4-01 no se reproduce |
| nc3 («GATE PASS» en `WorkCompleted`) | `R20261113T220343Z-6a16` | CONTROL PASS: FreeText única en fail, `EXECUTION_REWORK_REQUIRED` / REWORK |

## 4. Producto entregado

Integración de comando y asesor de F4-T3, **sin escritura authored ni semántica de F4-T4**.

- **`RACKPANELDESCARTAR`:**
  - núcleo `WorkspaceDocumentDiscardCommand` en Application;
  - adaptador en `WorkspacePanelCommand.cs` con `WorkspaceController.Existing` (nunca crea el controlador);
  - destino inequívoco (`isTarget` con `Port.Authorize`, C-F4-03);
  - lista por RackId de los racks en riesgo y de los inciertos, confirmación Sí/No con No por defecto y TOCTOU en las dos listas;
  - la única operación con efecto es `DiscardDocument`;
  - el desconocido es un fallo explícito, y el incierto de un comparador que falla se descarta con la confirmación.
- **Envoltorio del asesor `WorkspaceCloseAdvisorRelay`** en Application, instalado por el controlador sobre el host:
  - parte de Veto y da Allow solo con el del host;
  - el aviso de veto va en la línea de comandos;
  - la pérdida se lee antes de delegar y se escribe en el siguiente relevo con documento activo;
  - todo lo propio va contenido, y la delegación es siempre y una vez.
- **El aviso del panel** nombra las dos vías: el panel y `RACKPANELDESCARTAR`.
- **Los censos de comandos** quedan por nombre, y `GetKeywords` se exime solo en el archivo exacto del comando.
- **Los 10 motivos del Plugin** que llegan al campo, en español.
- **M-1 de F4-T2B2B** en sus dos mitades, como guardas de invariante: la instalación del envoltorio en orden y el reenvío del puente.
- **R-1, R-2 y R-4 de F4-T3C1.**

## 5. Limitaciones y abierto

**Menores de la revisión independiente → F4-T4,** como cambios declarados de su paquete. F4-T3 queda completa, y F4-T4 es la siguiente dueña del área de comando y contexto de comando:
- **C2-M-1:** la guarda de `GetKeywords` no fija que verdadero exija `PromptStatus.OK` y `"Si"`. Hay que reforzarla, porque es la protección estática de «explicit discard only» hasta Smoke-2;
- **C2-M-2:** la guarda del cuerpo de `RackPanelDiscard` solo exige `.Authorize(`. Hay que fijar `== RequestDecision.Accepted` y los dos primeros argumentos de `Run`;
- **C2-M-3:** siete guardas nuevas sin autoprueba en negativo, y una mira `SessionDestroyed` en lugar de `RememberLoss`;
- **C2-M-4:** la guarda de los dos miembros del asesor. Será un cambio declarado previsible si F4-T4 añade un miembro; conviene acotar su nombre a F4-T3C2;
- **C2-M-5:** nombres de prueba y comentarios de guardas existentes desfasados;
- **C2-M-6:** la frase «the one writer of the command line of the panel»;
- **C2-M-7:** la documentación R-4 del VM cuando la lectura del riesgo lanza;
- **C2-M-8:** una teoría con tres casos idénticos.

Ninguno cambia el comportamiento exigido (detalle en `analysis.md`).

**Limitaciones declaradas de D-T3C2-13:**
- la pérdida que escribe el envoltorio es informativa; el aviso del panel es la autoridad;
- con QUIT, solo el registro;
- la pérdida no nombra el documento;
- **la desviación declarada de D-T3B2-04 e INV-F4T3B2-08:** la fila de veto queda tras un descarte por el comando, con su prueba de evidencia;
- leer `PilotEdit` crea el host de navegación;
- si `write` lanza al escribir C-11 a C-14, el texto se pierde;
- el desconocido residual, hoy inalcanzable, deja el documento vetado sin descarte del documento posible;
- P-01 a P-03 esperan al siguiente `DocumentActivated` cuando el destruido no era el activo;
- el comando no entra en la ayuda, igual que RACKPANEL; se descubre por V-01, V-02 y las filas del panel;
- los motivos de F3 siguen en inglés (F7);
- `ReadPreviewWitness` queda en inglés.

**Sigue siendo de F4-T4** (autorización de T3C2):
- el conflicto o la caducidad externa;
- Actualizar explícito, PREPARE/MUTATE/POST y la autoridad de escritura authored;
- la mutación en el contexto de comando y la relectura tras la mutación;
- la contención de las pistas de mutación propia, un único paso de Undo y la atomicidad;
- R-3 de F4-T3C1 («`ErrorReason` solo en Error»).

**Smoke-2** (con AutoCAD real):
- `GetKeywords` (Intro, Esc, Sí) y el foco con la paleta visible;
- la escritura en `BeginDocumentClose` y en `DocumentActivated`;
- el cambio de documento durante la pregunta;
- el panel tras un descarte por comando;
- RACKPANELDESCARTAR antes de RACKPANEL;
- los motivos del campo en español;
- desplegar juntos Application, UI y Plugin;
- más lo heredado de F4-T3A a F4-T3C1.

**Proceso:**
- PD-F4-04 se repite en nc1 con su firma exacta (tercera tarea, tras F4-T2B2B y F4-T2C); sustituto de la subtarea por la regla de I-64/F4. PD-F4-01, PD-F4-05 y PD-F4-06 no aparecieron. Ninguna se resuelve de forma global dentro de I-64.
- **Lección:** el prompt del Worker es un f-string; las llaves de los fragmentos de C# del prompt se escriben dobles.

**Sin AutoCAD:** el comando y el envoltorio se prueban en Core con dobles y con el puente y el host reales; el Plugin, con guardas de texto y el build Release de la CI.
