# Paquete de cierre — F4B-PBT4-RETIRE-PILOT (PB-T4d)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2`, con attempts **1 de 3** y sin reparación del RED.

Se retira el código del piloto de Push Back que el panel ya no usa desde PB-T4b (D-PBT4-10), con el alcance B que eligió el Owner.
- **El GREEN toca solo `Pilot/`, con el texto literal de la pieza** (`pbt4d-green-v54.txt`): 4 archivos cambian y 2 se borran.
  - El comparador (`PushBackPilotStateComparer.cs`, con `StructuralEquality`) y el adaptador del campo (`PushBackPilotFieldAdapter.cs`) se borran.
  - `ComposeForWrite`, `KeepPilotEditsOn` y `PushBackPilotComposition` salen de `PushBackPilotMaterializer.cs`.
  - `PushBackPilotPrepareInput`, `Prepare` y `Build` salen de `PushBackPilotUpdatePlan.cs`.
  - El `Precheck`, el `Execute` y el `ComparePost` del piloto salen de `PushBackPilotUpdateRun.cs`.
  - `BuildCore` queda sin `allowErase` (la rama muerta), y `BuildFull` sin su argumento.
  - Cambian los cuatro `cref` y el comentario de D-S15.

  Salen exactamente las 57 declaraciones de C13.
- **El RED lleva todo `tests/`, con el texto literal de la pieza** (`pbt4d-red-v54.txt`; D-S11):
  - las 271 filas con acción de la lista, con registro: 198 bajas, 39 ediciones, 33 sustituciones y 1 cota;
  - los 3 `Skip`, con su equivalente (D-S12);
  - los vacíos de M-04, en su sitio (D-S13);
  - la prueba lenta, acotada a cada 97.º toque y al último (D-S14);
  - las 29 sustitutas de WPUR, sobre el modelo completo;
  - los 3 archivos que se borran;
  - los 5 nuevos, con las dos CT.
- **Ningún cambio de comportamiento:** ni el panel, ni la ventana, ni RACKEDITAR, ni el Plugin. `BuildCore` sin `allowErase` es equivalente para `BuildFull`, su único llamador, y no hay ninguna escritura en el DWG.

**Autoridades:**
- **El contrato:** `F4B-PBT4-RETIRE-PILOT/coordinator/gate-contract.json`, con:
  - el paquete de PB-T4 v5.4;
  - AuthorityRevision `23c29236`;
  - BaseSha = ChainBaseSha `399c9bec`.
- **Las decisiones del Owner:**
  - las tandas 10 a 14 sobre PB-T4d, en F4B-E56;
  - la Snapshot L, en F4B-E57;
  - la v3 del sustituto de nc2, en F4B-E58: «Sustituto v3 y revisión»;
  - en este cierre, F4B-E59, tres decisiones sobre el código: M-02 como límite del diseño, los caminos de `BuildCore` al Smoke-PB y la limpieza de M-03 a ideas futuras;
  - F4B-E35 (RR), F4B-E40 (PD-F4B-01 y el sustituto de nc1), F4B-E43 (la v2 del sustituto de nc2) y F4B-E46 (el total exacto de cada rama).
- **Las decisiones de la sesión:** D-S11 a D-S16 (`F4B-DRAFTS/pbt4/session-pbt4d-design-decisions.md`).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `23c29236` y BASE `399c9bec` | F4B-E56, con la v5.4 y la lista de 271 (`05a4d3d1`). Sin esqueleto: el RED y el GREEN son el texto literal del paquete, que dan los generadores custodiados (`red_pbt4d_v53.py` y `green_pbt4d_v53.py`) con sus piezas v5.4 |
| CI de la emisión | `45043090710` | En rojo: el detector de cuelgue abortó Core en la prueba lenta que PB-T4d acota (OWNER-01). Es una causa conocida, resuelta en la cadena |
| Preparación parada | `R20261122T171951Z-bc04` | P-01: `config.toml` reescrito por la actualización de la app de Codex, aún sin aceptar. No lanzó nada |
| Sonda neutra | `F4-ENVIRONMENT/probe-R20261122T172638Z-2c14` | PASS. La Snapshot L sustituye a la K (F4B-E57) |
| Planificación | `R20261122T172759Z-039a` | Aceptada al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 12 criterios. nc4 `R20261122T173145Z-27af`: solo A3 en fail |
| Worker | `R20261122T173325Z-c9e1` | IMPLEMENTATION_COMPLETE (claude-sonnet-5-5/high), sin reparación del RED y sin desviaciones. RED `aa65ecfd`: 31 rutas de `tests/` (23 M, 5 A y 3 D), el texto literal de la pieza. GREEN `7db6fd0c`: 6 rutas de `Pilot/` (4 M y 2 D), el texto literal de la pieza |
| CI del RED | `45054587789` | tests en failure; ui-tests y build-ui en success; build-plugin omitido. Core 19 418 = 19 014 + 1 Failed + 403 Skipped; UI 2 464 = 2 416 + 0 + 48. Solo cae CT-PBT4D-01, por aserción. La prueba acotada, 7,6 s |
| CI del GREEN | `45057868915` | 4/4. Core 19 418 = 19 015 + 0 + 403; UI 2 464 = 2 416 + 0 + 48. Son los totales exactos de su rama (F4B-E46). La prueba acotada, 9,2 s |
| Relevo del trabajo | — | Válido: el RED literal y el GREEN literal; C02 de la retirada (138 borradas y 45 cambiadas, cada una con su fila); las tres clases nuevas que no son CT, con su `NewFileSha`; cero `warning CS` |
| Revisión directa | — | Conforme, con un límite del diseño que el Owner ratificó (M-02) |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/3,** más OWNER-01 a OWNER-03, informativos. Ninguna decisión que el contrato no le dejara al Worker |
| Decisiones del Owner | — | M-02: «Ratificar como límite del diseño». OWNER-02: «Sí, sumarlos». OWNER-03: «Apuntarla en ideas futuras» |
| Verificación | `R20261122T194536Z-4c59` | **EXECUTION_VERIFIED** / NONE, 14/14, sin hallazgos y con `rr_check` PASS. Llevaba como entrada la disposición de M-02, pero no la cita, porque el Controller no evaluó las autopruebas una a una |
| nc1 | `R20261122T195309Z-fc6e`; sustituto `R20261122T201052Z-07ea` | CONTROL NOT PASSED con la firma exacta de PD-F4B-01: Handoff es la primera en fail, con S-04, y fallan Handoff, Identity, Remote, Scope, Ci y Tests. El sustituto revisado (F4B-E40, `--task PBT4`) da **PASS**: SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE. Lo autoriza el contrato (PBT4D-C11); no es una variación nueva |
| nc2 | `R20261122T195517Z-096b`; sustitutos v2 `R20261122T201101Z-bb30` y v3 `R20261122T212159Z-50c9` | CONTROL NOT PASSED en Contract y Scope, con S-04, P-03 y PBT4D-C01: la firma de PD-F4-01 en el modo F4B-E21. La v2 acabó sin veredicto (MemoryError: su propiedad 10 enumera 2^37 subconjuntos). La v3, que decidió el Owner (F4B-E58), con su revisión APTO (B 0 / I 0 / M 4), da PASS: SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B |
| nc3 | `R20261122T200420Z-4be7` | **CONTROL PASS** ordinario: FreeText, con EXECUTION_REWORK_REQUIRED / REWORK |

**Relevos:**
- 0 participantes persistentes en toda la cadena, también en la verificación y en los tres controles;
- la Snapshot L desde la planificación, y el binario acreditado;
- `main` = `812baf87`, sin tocar.

## 2. Límites, hallazgos y arrastres

| Id | Qué | Va a |
|---|---|---|
| M-02, CT-PBT4D-01 | En el RED, las dos autopruebas de archivo (`PushBackRetirePilotGuardTests.cs:111-115`) no discriminan: inyectan un archivo retirado que allí aún existe. Las otras 12 de CT-01 y todas las de CT-02 sí, y en el GREEN las 14. Choca con la letra de INV-PBT4D-02. Es el diseño literal de la clase del paquete | Ratificado como límite del diseño y errata de INV-PBT4D-02 (Owner), solo este caso y sin cambio de código. Va como disposición a la verificación |
| OWNER-02, `BuildCore` | Tres caminos cuyo texto cambia, sin cambio de comportamiento, que Core cubre: un plan que borra un lateral obsoleto; un borrador con el nombre en blanco, que escribe el del sobre de origen en todas las vistas; el rechazo de la frontal B de un rack sin lado B | Smoke-PB de PB-T5, junto a la pasada de «Editar todo» → «Actualizar» (Owner) |
| M-03 y OWNER-03, los nombres | Nombres de pruebas y comentarios que describen como presente el código retirado; renombrar cambia la identidad de la prueba y los filtros | `docs/ideas-futuras.md`, limpieza opcional; nada en PB-T4d ni en PB-T5 (Owner) |
| M-01, `RedFiles` de WPUR | El JSON de la precondición 6 lleva el RED de la v5.3 en `WorkspacePilotUpdateRunTests.cs`. El contrato manda la pieza v5.4, y contra ella el RED es exacto. Es M-16 de la emisión | De constancia, y `docs/ideas-futuras.md` |
| OWNER-01, la CI de la emisión | `45043090710` en rojo por el cuelgue de la prueba lenta | Resuelto en esta cadena (D-S14); de constancia |
| La etiqueta de C02 | `C02PorArchivo` del relevo cuenta como «cambiadas» las 29 de WPUR, que en WPUR se borran, porque su sustituta lleva el mismo nombre. Sus filas dicen «sustituir con registro» | De constancia |
| La cobertura (INV-PBT4D-06) | La da la precondición 6, que PASA. La revisión no la midió otra vez, porque pide coverlet | De constancia |
| Lo que la revisión no vio | Que no queden en `tests/` ayudantes `internal` o `public` sin uso: IDE0051 solo ve los privados | De constancia |
| RR-3 de las dos CT | La lectura de la cadena deja pendientes las dos filas. Sin reparación del RED, no tuvo efecto | `docs/ideas-futuras.md` |
| nc2, el sustituto v2 | Su propiedad 10 enumera los 2^n subconjuntos del alcance del contrato: con las 37 rutas de PB-T4d acaba en MemoryError, sin veredicto | La v3, con la forma cerrada (Owner, F4B-E58), sustituye a la v2 de aquí en adelante, PB-T5 incluida. La corrida de la v2 se conserva tal cual |
| Sin AutoCAD | Los tres caminos de `BuildCore` en el dibujo real y lo que dejan PB-T3c, PB-T4a, PB-T4b y PB-T4c | El Smoke-PB de PB-T5 |
| El registro declarado de las 271 | Completado en este cierre (cinco líneas) | Hecho |

**Siguiente:** la tarea propia de la recarga de «Editar todo» sobre un borrador Clean posiblemente obsoleto. Es OWNER-02 de PB-T4c, y el Owner la decidió así en `owner-pbt4d-list-decisions.md`, pregunta 2: «Tarea propia después». Vuelve al Owner con su lista antes de emitirse. Después viene PB-T5: la CI exacta, el Smoke-PB con la firma del Owner y el gate-close de F4B.
