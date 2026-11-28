# Paquete de cierre — F4B-PBT4-EDIT-ALL-RELOAD (PB-T4e)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `0eac7911a025a59628b446db8b768035175f96de`, con attempts **1 de 3** y sin reparación del RED.

La recarga de «Editar todo» (D-PBT4-22): antes de abrir la ventana, un borrador Clean marcado como posiblemente obsoleto se recarga desde el dibujo, sin aviso, con la misma recarga que «Editar». Si la recarga no recarga, la ventana no se abre y el campo muestra lo mismo que «Editar». Lo decidió el Owner en la 15.ª tanda: «Solo la recarga», «No abrir y avisar», «Antes de PB-T5» y «Sí, con condiciones».
- **El GREEN toca solo `src/`, con el texto literal de la pieza** (`pbt4e-green-v56.txt`): los 4 archivos `UNIT_CHANGE` cambian, con 34 líneas añadidas y 7 quitadas.
  - `WorkspaceHostCore.cs`: en `EditAllOfPilot`, la rama `if (!begunHere && draft.NeedsLazyReload)` llama a `TryReloadPilotEdit(request)` tras las tres comprobaciones de la vista y antes de `ShowModal` (D-S17 y D-S19), y devuelve `EditAllOutcome.NotBegun(reload)` si no recarga.
  - `WorkspacePilotFieldViewModel.cs`: el aviso de `EditAll` con `NotBegun` y `NotAdmitted` sobre un borrador que ya existía (D-S18), y la constante `NotReloadedText`, que también usa `BeginEdit`, con el mismo texto.
  - `WorkspaceEditIntent.cs` e `IWorkspacePilotEditPort.cs`: solo comentarios.

  Entra una sola declaración nueva (C13), la constante, y no sale ninguna.
- **El RED solo añade las dos clases de las CT, con el texto literal de la pieza** (`pbt4e-red-v56.txt`), sin esqueleto:
  - `PushBackEditAllReloadTests` (Core): CT-PBT4E-01, 02 y 04, con 16 líneas;
  - `PushBackEditAllReloadFieldTests` (UI): CT-PBT4E-03, con 9 líneas.

  Las dos, con `ExpectRed` true. La lista está vacía: ninguna prueba existente se edita, se salta, se reactiva, se borra ni se sustituye.
- **Ningún otro cambio visible:** ni «Editar», ni el botón, ni la ventana, ni RACKEDITAR, ni el Plugin; ningún texto ni estado nuevo; cero escrituras en el DWG. Como mucho una lectura del dibujo por gesto, y ninguna durante `ShowModal`.

**Autoridades:**
- **El contrato:** `F4B-PBT4-EDIT-ALL-RELOAD/coordinator/gate-contract.json` (sha256 `279a638c…6351`), con:
  - el paquete de PB-T4 v5.6;
  - AuthorityRevision `2e790aa9`;
  - BaseSha = ChainBaseSha `7da361eb`.
- **Las decisiones del Owner:**
  - la 15.ª tanda sobre PB-T4e, en F4B-E60;
  - F4B-E35 (RR), F4B-E40 (PD-F4B-01 y el sustituto de nc1), F4B-E43 (la lista cerrada de literales del sustituto de nc2), F4B-E46 (el total exacto de cada rama), F4B-E57 (la Snapshot L) y F4B-E58 (la v3 del sustituto de nc2).
  - en este cierre, F4B-E61, la decisión sobre nc1: «Aceptarlo solo esta vez» (`F4B-DRAFTS/coordinator/owner-pbt4e-nc1-decision.md`). El único menor de la revisión es informativo y no pide decisión.
- **Las decisiones de la sesión:** D-S17 a D-S19 (`F4B-DRAFTS/pbt4/session-pbt4e-design-decisions.md`).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `2e790aa9` y BASE `7da361eb` | F4B-E60, con la v5.6 y la lista vacía. Sin esqueleto: el RED y el GREEN son el texto literal del paquete, que dan los generadores custodiados (`red_pbt4e_v56.py` y `green_pbt4e_v56.py`) con sus piezas v5.6 |
| Planificación | `R20261123T045429Z-48a1` | Aceptada al primer lanzamiento, con la cesión de 04:54:50Z a 04:56:38Z: inyección PASS 8/8, A1-A8 8/8 y 13 criterios. nc4 `R20261123T045726Z-77d7`: solo A3 en fail, y el oráculo relativo se cumple |
| Worker | `R20261123T045827Z-6c9e` | IMPLEMENTATION_COMPLETE (claude-sonnet-5-5/high), sin reparación del RED y sin desviaciones. RED `265f9e31`: 2 rutas nuevas de `tests/` (2 A), el texto literal de la pieza. GREEN `0eac7911`: 4 rutas de `src/` (4 M), el texto literal de la pieza |
| CI del RED | `45119881969` | tests y ui-tests en failure; build-ui en success; build-plugin omitido. Core 19 421 = 19 015 + 3 Failed + 403 Skipped; UI 2 465 = 2 416 + 1 + 48. Solo caen las cuatro CT, por aserción en su `Finish` |
| CI del GREEN | `45123886125` | 4/4, con el Plugin. Core 19 421 = 19 018 + 0 + 403; UI 2 465 = 2 417 + 0 + 48. Son los totales exactos de su rama (F4B-E46) |
| Relevo del trabajo | — | Válido: el RED literal (sin esqueleto, 0 copias y 0 `Skip`) y el GREEN literal; la lista vacía; los dos filtros con `ExpectRed` enteros en Failed en el RED; cero `warning CS` en 15 registros de build |
| Revisión directa | — | Conforme |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/1.** M-01 (= OWNER-01), informativo: la paleta no se repinta con la ventana de «Editar todo» abierta. Ninguna decisión que el contrato no le dejara al Worker |
| Verificación | `R20261123T070111Z-b46b` | **EXECUTION_VERIFIED** / NONE, 14/14, sin hallazgos ni paradas, con `VerifiedSha` `0eac7911` y `rr_check` PASS; la cesión, de 07:01:53Z a 07:06:12Z. Su primera preparación contó 2 participantes falsos, por la ruta del worktree en la línea de órdenes de la sesión (la lección de I-61); repetida desde el scratchpad, 0/0, antes de lanzar |
| nc1 | `R20261123T070642Z-446d`; sustituto `R20261123T072321Z-9cda` | CONTROL NOT PASSED con la cascada del SHA falso: EXECUTION_BLOCKED / STOP, FailureClass Handoff, con S-04; difieren Handoff, Identity, Remote, Scope, Ci y Tests. El sustituto revisado de PD-F4B-01 (F4B-E40, `41440edc…`, `--task PBT4`) da FAIL solo en `e2_OtherFailCitesFakeSha`: Ci y Tests nombran «the handoff CurrentSha» o «CurrentSha is not a commit», sin el valor del SHA falso. Es la séptima variación de nc1 en F4B. El Owner: «Aceptarlo solo esta vez (Recomendado)», así que **SATISFIED_BY_OWNER_DISPOSITION**, solo para `446d`; el sustituto no cambia |
| nc2 | `R20261123T071150Z-beaa`; v3 del sustituto `R20261123T072425Z-00b5` | CONTROL NOT PASSED en Contract y Scope, con S-04, P-03 y PBT4E-C01: la firma de PD-F4-01 en el modo F4B-E21. La v3 de su sustituto (`E68B6E59…`, F4B-E58, `--task PBT4`) da PASS: SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B. Lo autoriza el contrato (PBT4E-C11); no es una variación nueva |
| nc3 | `R20261123T071718Z-ba7f` | **CONTROL PASS** ordinario: FreeText, con EXECUTION_REWORK_REQUIRED / REWORK |
| Decisiones del Owner sobre PB-T5 | — | Tomadas en la misma sesión: la emisión condicional, la adaptación con revisión de la evidencia, la definición de intento con las reglas de Smoke-2, la dispensa de la UI completa, las inyecciones, los casos construidos y el recuento del estilo de cotas. Su registro va con la emisión de PB-T5, no con este cierre |

**Relevos:**
- 0 participantes persistentes en toda la cadena, también en la verificación (tras repetir su preparación) y en los tres controles;
- la Snapshot L desde la planificación, y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Límites, hallazgos y arrastres

| Id | Qué | Va a |
|---|---|---|
| M-01 y OWNER-01, la paleta durante «Editar todo» | La recarga ocurre dentro de `TryEditAll` y el modal se abre en la misma llamada; el campo solo se repinta al volver `TryEditAll`. Con la ventana abierta, la paleta, deshabilitada, sigue mostrando «Limpio (posiblemente obsoleto)» y el valor de antes del cambio de fuera, mientras la ventana enseña el valor recargado. Al cerrar queda bien (CT-PBT4E-03). Es la misma falta de repintado que PB-T4c ya tiene | Smoke-PB de PB-T5: la guía lo anota en sus pasos 3 y 8 (§5.5), para que no se lea como un fallo de la recarga. Informativo: sin código, sin intento y sin decisión |
| Los resúmenes del puerto y de `BeginEdit` | Dicen que solo «Editar» lee el dibujo, inexacto desde PB-T4c; la primera frase la fija una guarda, y D-PBT4-22 la conservó. Observación de la revisión, no hallazgo, fuera del alcance (C10) | `docs/ideas-futuras.md` |
| El paso 5 del Smoke-PB | Supone que la pestaña del rack y su borrador Clean marcado siguen en el panel tras borrar fuera todas sus vistas; CT-PBT4E-02 lo modela con el lector (`OriginMissing`) | El Smoke-PB de PB-T5, y `docs/ideas-futuras.md` |
| La cobertura (INV-PBT4E-06) | La da la precondición 6, que PASA sobre el mismo texto literal. La revisión no la midió otra vez | De constancia |
| Los Skipped de UI frente a la base | La revisión solo comparó su recuento (48), no sus nombres: no tenía la TRX de UI de la CI base | De constancia |
| La preparación de la verificación | 2 participantes falsos en su primera corrida, por la ruta del worktree en la línea de órdenes de la sesión; repetida desde el scratchpad, 0/0 | De constancia, y `docs/ideas-futuras.md` |
| nc1, la séptima variación | El sustituto de PD-F4B-01 falla solo en `e2_OtherFailCitesFakeSha`, porque Ci y Tests citan el SHA falso por el nombre del campo y no por su valor, como en PB-T4a | Satisfecho por disposición del Owner, solo esta vez; el sustituto sigue siendo candidato a revisarse, sin cambio ahora. Si vuelve, vuelve al Owner |
| Los deslices | De la sesión, un `python -I -c ""` vacío que no ejecutó nada, y la preparación de la verificación de arriba; del revisor, un `python -c` de solo lectura; del Worker, las órdenes largas en segundo plano, `gh run watch` con `&` o `Start-Process` y una espera con `ping` que paró enseguida | De constancia: ninguno cambia nada de lo que fija el contrato |
| Sin AutoCAD | El gesto real, con el modal real y la lectura real del DWG, y lo que dejan PB-T3c, PB-T4a, PB-T4b, PB-T4c y PB-T4d | El Smoke-PB de PB-T5 |
| El registro declarado de PB-T4e | Completado en este cierre (cinco líneas) | Hecho |

**Siguiente:** PB-T5: la CI exacta, el Smoke-PB con la firma del Owner y el gate-close de F4B. Las decisiones del Owner sobre PB-T5 están tomadas; lo siguiente es la v2.3 de su paquete, su comprobación acotada y la emisión condicional. Este cierre no custodia nada de PB-T5, que tendrá su propia emisión.
