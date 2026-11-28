# Paquete de cierre — F4B-PBT2B-WORKSPACE-MODE-WINDOW (PB-T2b)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `03b16c88c01ebfd29a642fc6a9324eb76926bcba`, con attempts **1 de 3** y una reparación del RED (RR, F4B-E35). La ventana de Push Back tiene dos modos explícitos:
- **CLASSIC:** sin cambio observable. Lo abren los constructores de hoy, los cuatro sitios que la abren, WX, WC, Application y el Plugin.
- **WORKSPACE:** solo por la factoría tipada `ForDraft(PushBackDraftEditorLaunch)`.
  - Carga una copia del estado del borrador, sin pasar por un diseño.
  - «Aplicar al borrador» usa el mismo paso que «Actualizar» (`ApplyEditsToEditingState`) y devuelve una copia por el puerto de PB-T2a, sin cerrar.
  - «Cerrar» pregunta si hay cambios sin aplicar.
  - «Insertar» y el lote se rechazan.
  - BOM y biblioteca siguen, con dos costuras de prueba.

**Autoridades:**
- **El contrato** `F4B-PBT2B-WORKSPACE-MODE-WINDOW/coordinator/gate-contract.json`: paquete de PB-T2 v2.8, AuthorityRevision `a12ea09a` y ChainBaseSha `59839c71`.
- **Las decisiones del Owner:**
  - F4B-E28;
  - F4B-E35 (RR);
  - F4B-E37 (Snapshot K);
  - F4B-E38 (`F4B-DRAFTS/coordinator/owner-pbt2b-close-decisions.md`).
- **Las disposiciones:** F4B-E36 a F4B-E38.

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `a12ea09a` y BASE `59839c71` | F4B-E36, por la autorización condicional de F4B-E28 (pregunta 4). CA-T2b y sus recomprobaciones: la última, la §12 sobre la v2.8, EMITIBLE 0/0/2 |
| Preparación parada | `R20261119T154005Z-3194` | P-01: la app de Codex había reescrito `config.toml`. No se lanzó nada. **F4B-E37:** sonda neutra `2a9a` en PASS, y la Snapshot K |
| Planificación | `R20261119T154505Z-69c3` | Aceptada al primer lanzamiento: fidelidad 8/8 (`session-inject`), A1-A8 8/8 y 19 criterios. nc4 `R20261119T155813Z-2540`: A3 fail, oráculo cumplido |
| Worker | `R20261119T155931Z-1a87` | IMPLEMENTATION_COMPLETE. RED `a3b86134`, **RED repair `6bc3497f`** y GREEN `03b16c88` (solo W) |
| CI del RED y del RED reparado | `44572405777` y `44581043462` | tests y ui-tests en failure (CT-PBT2B-08 en Core; CT-PBT2B-01 a 07 en UI, por aserción); build-ui en success; build-plugin omitido |
| CI del GREEN | `44586045707` | 4/4. Core 19 094; UI 2 447 (2 426 pasadas y 21 omitidas), como se preveía |
| Revisión directa | — | Sin hallazgos |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/6**, más OWNER-01 |
| Verificación | `R20261119T182732Z-422f` | **EXECUTION_VERIFIED** / NONE, 14/14. `rr_check` PASS |
| nc1 | `R20261119T183134Z-bfda` | CONTROL NOT PASSED, una variación de PD-F4-04: Handoff pasa; fallan Identity, Remote, Ci, Tests y Trailer; STOP S-04. **El Owner: «Aceptarlo solo esta vez»** → `nc1 = SATISFIED_BY_OWNER_DISPOSITION` |
| nc2 | `R20261119T183325Z-14dd` | **CONTROL PASS** (Scope, STOP) |
| nc3 | `R20261119T183456Z-a3bc` | **CONTROL PASS** (FreeText, REWORK) |

La tabla de comparación de nc1 está en F4B-E38.

**Relevos:** 0 participantes en toda la cadena, la Snapshot K desde F4B-E37, el binario acreditado, y `main` = `812baf87` sin tocar.

## 2. Desviaciones, límites y arrastres

| Id | Qué | Va a |
|---|---|---|
| RR (la primera de F4B) | El primer `Sta` del proceso pisaba el aviso de descarte sustituido. La reparación arranca el hilo STA antes de sustituirlo. Solo hay líneas añadidas y ninguna línea esperada cambia | Declarada; verificada por el Controller y por `rr_check` |
| Corridas de diagnóstico | Cinco corridas locales con instrumentación temporal, revertida y fuera de todo commit | La revisión: cumplen 16.8 y RR; M-03 |
| Incidencia de la sesión | La comparación se ejecutó sobre la salida cruda y sobrescribió la copia de `byte-fidelity-check.json`; se restauró desde `session-inject` | Declarada en el relevo de la planificación |
| PBT2B-IR-M01 a M06 | La precisión de CT-08, CT-05 y la evidencia de INV-06; el centinela de `diseno-igual`; el texto del §5.0 sobre la sonda; las corridas de diagnóstico | `docs/ideas-futuras.md` |
| A-T2b-OWNER-01 | «Cerrar» con un módulo escenificado y cambios sin aplicar solo hace la pregunta del módulo | PB-T4 (decisión del Owner) |
| La disposición del configurador | Cambia por pruebas existentes (INV-PBT2B-10) | Ya anotado (F4B-E30) |

**Siguiente:** emitir PB-T2c con el paquete de PB-T2 v2.9 tras la remedición sobre este cierre, la tabla de CT-PBT2C-03 medida y su recomprobación final (la §13 de CA-T2c), por F4B-E28. Después, PB-T3, por F4B-E35 (pregunta 3).
