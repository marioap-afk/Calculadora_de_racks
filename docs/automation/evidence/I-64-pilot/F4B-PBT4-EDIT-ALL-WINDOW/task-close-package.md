# Paquete de cierre — F4B-PBT4-EDIT-ALL-WINDOW (PB-T4c)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `7cc1076eae8159294a1498ab556851977223b64c`, con attempts **1 de 3** y sin reparación del RED.

«Editar todo» llega al panel del Workspace de Push Back, sobre el borrador completo de PB-T4b.
- **El botón y su gesto** (D-PBT4-12): confirman el texto pendiente con `CommitField` antes de abrir (R-PB-10).
- **`TryEditAll`** (D-PBT4-13): en el puerto con su cuerpo por defecto, en el anfitrión y en sus dos reenvíos (la compuerta de avisos y el adaptador del panel del Plugin). Empieza el borrador si hace falta y mantiene la marca de lectura mientras la ventana está abierta (K = 1; D-PBT4-14).
- **El modal y el abridor, en dos piezas** (D-PBT4-15):
  - el modal del panel, en una clase anidada de `WorkspaceController` sobre `IWorkspaceFullEditHost`;
  - la ventana de Push Back en WORKSPACE, sin dueño, en `Plugin/Views/RackWorkspaceEditorWindow.cs`.
  El lanzamiento y el adaptador del puerto de `Draft/` conservan `Origin` y acaban la marca *known clean*.
- **La guarda de texto de OB-01.3** sobre los seis sujetos del camino (D-PBT4-16).
- **«Cerrar» en WORKSPACE** con una sola pregunta si hay a la vez un módulo escenificado y cambios sin aplicar (D-PBT4-17; A-T2b-OWNER-01). El resumen y «Modificado» se muestran al cerrar.
- **Las 4 pruebas de la lista** quedan saltadas con su motivo literal, cada una sustituida por su equivalente, byte a byte su `EquivalentSha`.
- **El código del piloto queda sin uso;** lo retira PB-T4d.

**Autoridades:**
- **El contrato:** `F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json`, con:
  - el paquete de PB-T4 v5.1;
  - AuthorityRevision `a8e9cbe6`;
  - BaseSha = ChainBaseSha `d5026014`.
- **Las decisiones del Owner:**
  - F4B-E49 y las tandas sobre PB-T4, con la octava (la lista pasa a 3) y la novena (la lista pasa a 4), en F4B-E54;
  - en este cierre, F4B-E55, dos decisiones sobre el código: la errata de CT-PBT4C-11 y «Editar todo» sobre un borrador Clean posiblemente obsoleto;
  - F4B-E35 (RR), F4B-E37 (Snapshot K), F4B-E38 (A-T2b-OWNER-01), F4B-E40 (PD-F4B-01), F4B-E43 (el sustituto de nc2) y F4B-E46 (el total exacto de cada rama).
- **Las decisiones de la sesión:** D-S7 a D-S10 (`F4B-DRAFTS/pbt4/session-pbt4c-design-decisions.md`).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `a8e9cbe6` y BASE `d5026014` | F4B-E54, con la v5.1 y la lista de 4 (`9bd4c17a`). El esqueleto lo da el generador custodiado (`green_pbt4c_v51.py`), registrado en la precondición 6 (`cc4c/r5j-rg/apply-red.txt`). Sin copias: los 4 equivalentes van por su `EquivalentSha`, y el GREEN es el texto literal del paquete |
| Planificación | `R20261121T205457Z-af49` | Aceptada al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 14 criterios. nc4 `R20261121T205711Z-fbe4`: solo A3 en fail |
| Worker | `R20261121T205731Z-41db` | IMPLEMENTATION_COMPLETE (claude-sonnet-5-5/high), sin reparación del RED. RED `280c79be` (21 rutas: el esqueleto, los 4 `Skip`, los 4 equivalentes y las cinco clases de las CT) y GREEN `7cc1076e` (10 archivos de `src/`, iguales byte a byte al GREEN literal). Dos desviaciones de procedimiento local declaradas: la suite local de Core en tres particiones, con suma exacta, y más tiempo del detector de cuelgue local para una clase lenta |
| CI del RED | `44956801926` | tests y ui-tests en failure; build-ui en success; build-plugin omitido. Core 19 619 = 19 149 + 9 Failed + 461 Skipped; UI 2 486 = 2 432 + 5 + 49 |
| CI del GREEN | `44959653142` | 4/4. Core 19 619 = 19 158 + 0 + 461; UI 2 486 = 2 437 + 0 + 49. Los totales exactos de su rama (F4B-E46) |
| Relevo del trabajo | — | Válido: la forma del RED, el esqueleto, los `Skip`, los `EquivalentSha`, el GREEN literal y cero `warning CS` |
| Revisión directa | — | Conforme, con la desviación de CT-PBT4C-11, que el Owner ratificó |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/1/7,** condicionada a I-01, más OWNER-01 a OWNER-05. Da las dos desviaciones de procedimiento por procedimiento local, que el contrato no regula |
| Decisiones del Owner | — | I-01: «Ratificar como errata». OWNER-02: «Aceptarlo y corregir luego» |
| Verificación | `R20261121T234430Z-dd13` | **EXECUTION_VERIFIED** / NONE, 14/14, con la disposición de CT-PBT4C-11 como entrada |
| nc1 | `R20261121T235038Z-94db` | **CONTROL PASS** ordinario: Identity, con S-04 |
| nc2 | `R20261121T235550Z-2fc5`; sustituto `R20261122T000958Z-4719` | CONTROL NOT PASSED en Contract y Scope, con S-04 y PBT4C-C01: la firma de PD-F4-01 en el modo F4B-E21. El sustituto v2 revisado (F4B-E43, `--task PBT4`) da **PASS**: SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B |
| nc3 | `R20261122T000101Z-3ec1` | **CONTROL PASS** ordinario: FreeText, EXECUTION_REWORK_REQUIRED / REWORK |

**Relevos:**
- 0 participantes persistentes en toda la cadena;
- la Snapshot K y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Límites, hallazgos y arrastres

| Id | Qué | Va a |
|---|---|---|
| I-01, CT-PBT4C-11 | La segunda receta sustituye el informe de módulos (`Editor.SideA.LastModuleReconciliation`) por un objeto vacío, y no la línea base resuelta por la de otro fixture (§5.3, fila 11). La línea esperada se cumple. Es la misma forma que CT-PBT4B-10. No estaba declarada | Ratificada como errata del paquete (Owner), solo este caso |
| OWNER-02, la recarga | Con un borrador Clean marcado como posiblemente obsoleto, «Editar» recarga antes y «Editar todo» no: abre el editor sobre la base vieja. Al editar, el borrador pasa a sucio y obsoleto, y al aplicar sale «Conservar mi borrador», que avisa antes de escribir | Aceptado en PB-T4c (Owner). El Smoke-PB lo comprueba con un cambio externo del dibujo; la corrección, que «Editar todo» recargue antes, va a PB-T4d o después, con su lista |
| OWNER-03, BOM y biblioteca | Desde la ventana de «Editar todo», en WORKSPACE, el BOM y «Guardar en biblioteca» siguen visibles (D-PB-07) y escriben en disco. «Guardar en biblioteca» también ejecuta `EnsureId` y `SetName` sobre la identidad de la ventana | Smoke-PB: las dos salidas, y si el `SetName` llega al borrador con «Aplicar al borrador» |
| OWNER-04, lo que §5.5 no lista | La pregunta combinada de «Cerrar» con el aviso real; el resumen y «Modificado» tras cerrar; el aviso «No se abrió el editor» si una excepción del editor llega tras aplicar (M-04) | Smoke-PB |
| OWNER-05, la prueba lenta | `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuild…` tarda 3-5,3 min en la CI, frente al detector de cuelgue de 5 min (`ci.yml`). La hereda PB-T4c de PB-T4b | La lista de PB-T4d |
| M-01 a M-04 | CT-10 sin la compuerta; las debilidades de discriminación de CT-07 y CT-04; la carga de `VentanaBorrador` fuera del `try`; el aviso falso de M-04 | `docs/ideas-futuras.md` |
| M-05 | Una errata del texto del paquete: en el RED, CT-04 y CT-11 caen porque el puerto responde `NotAvailable`, no `NotEditable` | De constancia |
| M-06 | La copia `rv/` de la revisión no traía `relay-subst-check.json` ni `clean/` | De constancia. La custodia lleva la unidad del Worker entera |
| M-07 | Un separador duplicado y un encabezado heredados en `PushBackDraftLaunchGuardTests.cs` | De constancia, cosmético |
| Las dos desviaciones de procedimiento | Las particiones de la suite local y el tiempo extra de la clase lenta | Procedimiento local, con latitud (la revisión independiente) |
| Sin AutoCAD | El modal real con la paleta deshabilitada, el foco y el teclado, «Actualizar» tras editar, los eventos del puente durante el modal y el redibujo | El Smoke-PB de PB-T5 |
| El registro declarado de las 4 | Completado en este cierre (cinco líneas) | Hecho |

**Siguiente:** PB-T4d, la retirada del código del piloto. Vuelve al Owner con su lista antes de emitirse. La lista lleva:
- los vacíos de M-04 de PB-T4b;
- la recarga de «Editar todo» (OWNER-02);
- la prueba lenta (OWNER-05).

La emisión condicional de F4B-E49 (pregunta 4) termina con PB-T4c.
