> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`, `<worktree-de-I-64>`). El resto no cambia.

# I-64 F4B PB-T3 — Revisión independiente adversarial del paquete v1

**Veredicto: NECESITA v2.** B/I/M = **1 / 11 / 16**.

- **El bloqueante:** si la vista de origen del borrador es un lateral que queda obsoleto, el diseño la borra en MUTATE y POST la busca para releer: la escritura se confirma y el resultado sale `AppliedPostRereadFailed` (B-01).
- **Lo importante, en tres grupos:**
  - **D-1 está mal enunciada** ante el Owner. S-6 no falla: queda vacía. Y tres de las seis sustituciones las impone la forma del diseño elegido, no el [OD] (I-01, I-02).
  - **Tres contratos que no se pueden cumplir tal como están escritos:**
    - la API cerrada de PB-T3a no tiene constructores para tres tipos (I-06, otra vez la lección M10);
    - el `ForbiddenWriteScope` de PB-T3b corta su propio `AllowedWriteScope` (I-05);
    - dos clases de prueba no se comportan en el RED como dice el contrato (I-03, I-04).
  - **Pruebas que no discriminan o no tienen receta:** CT-PBT3B-10 (I-07), la barrera del piloto (I-08) y el clasificador (I-11). A eso se suman un hecho anticuado (la Snapshot I, I-09) y el registro declarado que falta para D-2 (I-10).
- **Decisiones del Owner:** **una**, D-1, que sigue siendo material, pero **reformulada** (§6). No aparece ninguna otra decisión material nueva. D-2 no es una decisión, pero sí necesita su registro declarado.

---

## 1. Método y límites

- **Solo lectura.** Solo objetos git del checkout principal: `git show 2f5d3dec:<ruta>`, `git grep`, `git ls-tree` y `git log`. Sin compilar, sin ejecutar pruebas y sin órdenes git que escriban.
- **El worktree de la rama no se leyó, no se listó y su ruta no se escribió en ninguna orden.**
- **Ayudas, todas en `<carpeta-del-paquete>`:**
  - un script de solo lectura escrito con la herramienta de archivos y lanzado como archivo con `python -I`: `rv_anchors.py`, con su salida `rv_anchors_out.txt`;
  - dos copias en texto de guardas de F4, sacadas con `git show` para leerlas: `_rv_updateguard.cs` y `_rv_integguard.cs`.
- **Lo que se leyó entero:**
  - el paquete v1 y su registro;
  - el encargo del redactor;
  - la fila de PB-T3 del [OD] y su texto completo;
  - D2 (`owner-d1-d2-decisions.md`) y la decisión de la ExtensionData;
  - F4B-E21, F4B-E22 y F4B-E19 (Snapshot J);
  - las secciones OB-02 a OB-05, §6.4 y OB-T1B de [P];
  - `block-m-analysis.md`;
  - el §5.0 y el A-2 de la v2.8 en borrador de PB-T1;
  - `.github/workflows/ci.yml`;
  - AUTOMATION_PLAN §16.3, 16.5, 16.6, 16.8, 16.9 y 16.11.
- **Las guardas de F4, leídas método a método** donde leen PP, UR o WR: `WorkspacePilotPrepareGuardTests` (17), `WorkspacePilotUpdateGuardTests` (42), `WorkspacePilotUpdateIntegrationGuardTests` (43) y `WorkspaceDraftPilotIntentGuardTests` (13).
- **Las guardas de I-54, D2 e I-47:** `CustomPropertiesEnvelopeGuardTests`, `PushBackSharedReferenceGuardTests`, `CallerOwnedFacadeGuardTests`, `RedefinedViewReferenceUndoTouchGuardTests` y `NamespaceFolderGuardTests`.
- **El código de producto, entero:** PP, UR, PM, IR, SR, WR, GATE, la mitad de PES, PESO, RVA, `RackViewCodec.DecodePushBack`, `RCS:215-280`, RPC (las líneas citadas) y W (`:143-147`, `:436-441`, `:655-706`).
- **Límites:**
  - Todo se dedujo leyendo el código, sin ejecutarlo. «Pasa en vacío» y «falla» se dicen por lectura del texto de las guardas y de `WorkspaceHostSourceSupport.cs:67-89` (`BodyOf` lanza con `Assert.True`).
  - No se midió ningún recuento en la CI.
  - Se leyó la v2.8 en borrador de PB-T1 solo en lo que PB-T3 hereda.

## 2. El Owner

| Punto de la fila de PB-T3 del [OD] y decisiones | ¿Lo cumple el diseño? | Notas |
|---|---|---|
| «field changes» | Sí | `ForWrite` compone el estado editable actual (D-PBT3-05); CT-PBT3A-06 y CT-PBT3B-01 |
| «modules/fronts» | Sí | CT-PBT3B-02 y 03; los frentes sin crear vistas (BM:20, :94) |
| «side B» | Sí, en el modelo sin conectar | El lector completo admite A/B solo tras AUTH-13 (IR:205-233); el rechazo del panel se queda hasta PB-T4 (OB-03.2, A-T3-2) |
| «rename» | Sí | Nombre del borrador en el sobre; los nombres de las definiciones, por `DesiredNamesOf` y `SyncName` en POST (UR:190-223; WR:177-180) |
| «deleting obsolete views» | **Con un defecto** | Regla del clásico (RPC:320-356) y fachada caller-owned (OB-04.1). **B-01:** borrar el **origen** rompe POST |
| «blocking authored write on diagnostics/errors» | Sí, en diseño | `PushBackEditorOutputGate` en PREPARE, para los dos modelos. **I-08:** sobre el piloto, que es lo único que llega al panel antes de PB-T4, no hay prueba |
| «view creation only if PB-T0 subsequently authorizes it» | Sí | El bloque M no crea vistas (BM:90-97); D-PBT3-14 |
| «PREPARE -> MUTATE -> POST and one logical Update» | Sí, salvo B-01 | Una transacción con los borrados dentro (D-PBT3-10); un `U` para el Smoke-PB |
| D-PB-05 / OB-05.2: componer **sin conectar** | Sí | CT-PBT3A-12 y CT-PBT3B-13; `KeepPilotEditsOn` sigue conectada (WHC:694) |
| D-PB-05, pasos 4 a 7 | Sí | Todo el estado editable, salvo `$.SchemaVersion` de la carga útil, es `Editor` (PESO:137-172). El sobre, el proyecto y la carga útil salen de las fuentes releídas (PP:421-438; `RackEmbedComposer.cs:43-60`; `SystemRegistry.Default.cs:168`) |
| La ExtensionData, «Basta así»: AUTH-13 fijado por prueba | Sí en la lectura (CT-PBT3A-04). **En la escritura, la receta no llega a AUTH-13** (I-07) | |
| D2 y su límite | Sí | C17 en los tres contratos, con las condiciones 1-4, 6-8 y 10 (D-PBT3-12). **M-02:** el RED de CT-PBT3B-12 está sin precisar |

**La regla del frontal B** (§6.4, fila 3; BM:96): la revisión **mantiene el valor por defecto, el rechazo**.
- Borrarlo destruiría una vista que el clásico conserva (BM:25).
- Redibujarlo deja `Section` 2 en un rack de un sentido, con un choque de nombres sin medir (BM:72).

No es materia del Owner. Ver M-08 sobre el texto.

## 3. Los hechos (anclas comprobadas en `2f5d3dec`)

| Ancla del paquete o del registro | Lo que afirma | Lo que hay | Estado |
|---|---|---|---|
| R-0: `git log` y `I-64.yml` | Base `2f5d3dec` ← `a0a5f699` ← `2d9febc6` ← `4b0498e3`; `attempts: 2` | Igual; `I-64.yml:9` `attempts: 2` | OK |
| R-0: `direct-review.md:23` | CI `44405531606`, Core 19 086, UI 2 437 (2 416 y 21) | Igual | OK |
| PP:293-295 | `ViewUnavailablePrefix` y su sufijo | Igual | OK |
| PP:309-334 y :337-430 | Orden de `Prepare`; `Build` | Igual; `CompareWith` en :322 | OK |
| PP:347-373 y :375-388 | Miembros, origen, fuentes interiores; diseño, sistema y hechos en un `try` | Igual; `system` es local al `try` (:381) | OK |
| PP:390-401 | Ninguna vista no disponible se borra: `Invalid` | Igual | OK |
| PP:421-422, :434-438, :436 | Compone sobre el sobre releído, con `source.Name` | Igual | OK |
| UR:122-134, :139-157 | `Precheck`; `Execute` con `var execution = new Execution(...)` antes del `try` | Igual | OK |
| UR:163-183 | `ComparePost`: claves, sobres y escala | Igual | OK |
| UR:190-223 (base :202) | `DesiredNamesOf` | Igual | OK |
| UR:250-289, :347-432, :391-456, :460-493 | Run, MUTATE, Write y aborto, POST | Igual. **POST relee con `plan.OriginKey`** (UR:481) | OK (y base de B-01) |
| UR:536-547 | Supervivientes y capas | Igual | OK |
| WR:26 | «It never erases a view.» | Igual. También WR:225: «It never purges, renames, regenerates or erases» | OK (M-16) |
| WR:146-157, :155, :160-207, :301-307 | `BeginMutation`, `StartTransaction`, POST, `Commit` (:305) | Igual | OK |
| IR:35, :50, :56 | Diagnósticos y pista | Igual | OK |
| IR:205-222, :225-229 | AUTH-13 (reglas 10 y 11) y materialización (12 a 14) | Igual; el compuesto solo se rechaza tras AUTH-13 | OK |
| PM:106, :108, :148, :169-170 | Los dos motivos «fuera del piloto» | Igual | OK |
| PM:196-234 (comparación en :212-215, :217-224, :227); PM:201; PM:242-251 | `ComposeForWrite`; «Falta el estado inicial o el actual.»; `KeepPilotEditsOn` | Igual | OK |
| SR:46-55 | `BuildForMember` construye desde el texto y pone el `Name` del sobre | Igual (`system.Name = envelope.Name`, :53) | OK |
| PES:38-95, :100-126, :130-203, :367-405, :415-465 | El API de PB-T1b | Igual; `SupportOf` en :454-465 | OK |
| PESC:29-63 | `Instance`, `Equals`, `GetHashCode`, `Copy` | Igual. `Instance` usa el constructor implícito público (:31) | OK (I-06) |
| PESO:9-176 (:82-113, :133-175, :142, :165-172) | El mapa | Igual | OK |
| GATE:16-40 | `Diagnostics`, `IsBlocked`, `StatusOr` sobre `(PushBackCompositeEditorState, PushBackSystem)` | Igual | OK |
| RVA:94-116, :136-146 | Disponibilidad de Push Back | Igual. `DecodePushBack` solo da planta, lateral y corte (`RackViewCodec.cs:263-286`): el «cualquier otro → `NoAddress`» de D-PBT3-06 no se alcanza | OK |
| DS:239-264, :469-486 | Genérico; `KeepMineOnCurrentVersion` copia otra vez | Igual | OK |
| WHC:347-352, :694, :844, :895 y los nueve `Find<` | El anfitrión del piloto | Igual (nueve `Find<PushBackPilotDraftState>`) | OK |
| RPC:186-190, :193-198, :268-271, :276-333, :348-356, :358-383 | El «Actualizar» clásico | Igual | OK |
| RCS:218-275 (:229-231, :256-262, :269, :274) | `EraseViewBlocks` | Igual; salta en silencio `IsNull`, `IsErased` e `IsLayout` (:235-243) | OK |
| `RackSelectivoInsertIntegration.cs:281-289` | Precedente de borrado caller-owned | Igual | OK |
| `RackSiblingMembership.cs:64` | `IsSurvivor` | Igual | OK |
| `RackEmbedDocument.cs:74`, `RackProjectDocument.cs:51`, `PushBackDesignDocument.cs:74-76`, `:92-98`; `RackEmbedComposer.cs:43-60`; `RackProjectStore.cs:60`; `SystemRegistry.Default.cs:168` | Cajones de campos desconocidos y su conservación | Igual. `SystemRegistry.Default.cs` está en `src/RackCad.Application/Systems/Shared/` | OK |
| `I58F1Matrix.cs:58-64`, `:97-102` | AUTH-13 rechaza las capas de arriba y las anidadas (`Modules.0`, `SideB`…) | Igual | OK |
| `PushBackPilotFieldAdapter.cs:24`, `:37-40`; `PushBackPilotStateComparer.cs:58` | Solo la escala; el copiador | Igual | OK |
| `RackCad.Application.csproj:11`; `PushBackPilotIntentResult.cs:27-41` | `InternalsVisibleTo`; constructor interno | Igual | OK |
| `F4-SMOKE-2/run/run-log-2.md:50-52` | Una entrada de Deshacer por «Actualizar» | Igual (S2-03 PASS) | OK |
| BM:20, :24, :25, :31-45, :49-53, :54-60, :61, :62-66, :67-72, :90-97, :133-139, :141 | Los hechos del bloque M | Igual | OK |
| R-6: PrepareGuard :81-98, :102-139, :154-190, :194-227, :229-251 | Lo que fijan | Igual | OK |
| R-6: UpdateGuard :57-124, :126-141, :143-159, :182-255, :410-479 | Lo que fijan | Igual; prohibiciones en :185-211 | OK |
| R-6: IntegrationGuard :81, :351-365, :651-675, :681-719 (cuentas de WR), :880-910 | Lo que fijan | Igual | OK |
| R-6: S-6, «Sí, solo por el tipo» | Que el diseño la rompe | **DIFF.** Con `Execution<TState>`, `RunEscapeViolations` ya da una violación sobre el texto real (`IndexOf("var execution = new Execution(")` = −1). Las tres sustituciones de :907-909 no encuentran su texto o no cambian nada, y `Assert.NotEmpty` solo mira el primer elemento. **S-6 pasa en vacío; no falla.** Igual pasan tres de las cinco filas de S-2 | **DIFF** (I-01) |
| R-6: «las dos fábricas pasan por un único `Create` privado» | La fábrica del piloto | **DIFF con el paquete:** D-PBT3-09 da a `PushBackFullPrepareInput` un tipo propio con su constructor privado. Inocuo | DIFF (M-16) |
| `CustomPropertiesEnvelopeGuardTests.cs:194-207` | El censo por (ruta, miembro, primer argumento) | Igual: `name` como parámetro no lo cambia | OK |
| `PushBackSharedReferenceGuardTests.cs:173-195`, `:278-282` | CT-14: lista cerrada más `Application/Workspace/`; CT-15: archivos del Plugin con `PushBack` | Igual: `Pilot/` y `Draft/` están fuera de CT-14 | OK |
| R-8: los recuentos estáticos | 67/49/41/35/44/28/63/27; 17/42/43/13; 2 y 8; 9 y 7 | Igual | OK |
| R-8: `bloqueado=True` en `PushBackStateNetFingerprintTests.cs` | 0 | 0 (y 105 `bloqueado=False`) | OK |
| `AUTOMATION_PLAN.md:557-558` | Una delegación abierta por unidad | Igual | OK |
| §1: [PKC2] «§6.7, `:655`» | Las 17 paradas y las celdas | **DIFF menor:** §6.7 está en `:579-581`; `:655` es §6.8, las celdas | DIFF (M-05) |
| Precondición 6 y §6.0: «P-01 con la Snapshot I» | La línea base del Controller | **DIFF:** la línea base activa es la **Snapshot J** (F4B-E19, `owner-config-snapshot-j-decision.md`; `f4b-dispositions.md:572-596`; `I-64.yml:57`) | **DIFF** (I-09) |
| §6.0: el `ForbiddenWriteScope` con «las listas curadas de PB-T1» | — | Esa lista (la de PB-T0, 18 entradas) **incluye el prefijo `tests/RackCad.Tests/Workspace/`** (`F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json`; `I-64-f4b-pbt1-measurements.md:155`), y el contrato de PB-T1b lo prohíbe | **DIFF** (I-05) |

**Recuento:** unas 75 anclas comprobadas, con cuatro diferencias (S-6, la Snapshot, el alcance prohibido y [PKC2]) y una incoherencia interna del registro.

## 4. La división

- **Cada subtarea, ¿la entrega un Worker?**
  - **Sí, con su RED, su GREEN y su alcance:**
    - PB-T3a: 8 archivos nuevos y 14 CT, del tamaño de PB-T1b;
    - PB-T3b: dos archivos de producto generalizados, una interfaz y 14 CT;
    - PB-T3c: el Plugin, con 5 CT de texto.
  - **Pero con los contratos actuales, no:**
    - PB-T3a se para en C13 (I-06);
    - PB-T3b no puede pasar `Scope` (I-05) ni su RED (I-03);
    - PB-T3c no puede pasar su RED (I-04).
- **Las dependencias son correctas:**
  - PB-T3b usa los tipos de `Draft/`;
  - PB-T3c implementa la interfaz de PB-T3b;
  - PB-T3a necesita el cierre de PB-T1c, por la costura `EditableStateForTest` de CT-PBT3A-01. Es la precondición 1.
- **¿PB-T3a rompe alguna guarda?** No. Censo propio, por las reglas de cada guarda:
  - G-9: los archivos de `Draft/` nombran el piloto, pero no usan ningún término de escritura;
  - G-7: ninguno llama a `.PrepareUnits(`, `.BeginMutation(`, `.WriteUnit(` ni `.Seal(`;
  - el censo de `Task`: `Draft/` no entra;
  - `Pilot/` sin `Transaction`: `Draft/` no está en `Pilot/`;
  - I-54 TGrd01 y TGrd02: ningún archivo construye ni compone un sobre;
  - `NamespaceFolderGuardTests`: el cubo es `PushBack` y el namespace es de bloque;
  - CT-PBT1B-14 y 15: no los alcanza;
  - HT1-21: no añade campos.
- **¿Es segura la fusión de PB-T3b y PB-T3c?** Es posible, pero **no se recomienda**.
  - «No cambia ningún CT» vale para los identificadores. No vale para:
    - las invariantes (INV-PBT3B-02 «el GREEN solo toca PP y UR» e INV-PBT3C-01);
    - las paradas (C16 de b y de c dicen cosas distintas);
    - los filtros y su `MinSelected`;
    - el registro de D-2.
  - El GREEN fundido sería el mayor cambio de producto de F4B en un solo 1/3.
  - Separadas, la fachada del Plugin queda aislada con sus guardas.

## 5. Las pruebas

**El RED frente a `ci.yml`:**
- `build-plugin` tiene `needs: [tests, build-ui]`, así que sale omitido en los tres RED, porque Core falla a propósito.
- La lectura de M14 del §6.0 es correcta: `tests` en `failure`, `build-ui` en `success` y `ui-tests` en `failure` (PB-T3a) o en `success` (PB-T3b y PB-T3c).
- El Plugin solo se compila en el GREEN de PB-T3c.

**Lo que no se puede cumplir o no discrimina:**
- **CT-PBT3B-14, E-2 y E-6 (I-03):** son autopruebas. Si no afirman antes que su texto existe, en el RED pasan (en vacío), y el filtro tiene `ExpectRed` en `true`. Eso da `RedPart` en `fail` en `Tests` (16.9 n.º 10), o la parada C04.
- **CT-PBT3C-05 (I-04):** «pasa en el RED», pero lee la fachada, que el RED no crea. Con la regla `existing.Count == files.Count` de CT-PBT1B-14 (`PushBackSharedReferenceGuardTests.cs:211-213`), falla.
- **CT-PBT3B-10 (I-07):** con un borrador nacido de una lectura limpia, la relectura con un campo desconocido da **Conflict** antes de la admisión (PP:322-323 y UR:402-405), nunca `NotAdmitted`.
- **CT-PBT3B-06 (I-08):** no prueba la barrera en el piloto.
- **CT-PBT3A-09 y 10 (I-11):** sin líneas literales, y sin analizar las hojas derivadas.
- **CT-PBT3B-04 (iv) (M-01):** compara `AbsentSlotsB` con `Pass` sobre el mismo estado y el mismo ensamblador. Es tautológico; el hecho medido de M-i (BM:62-66) no entra.
- **CT-PBT3B-12 (M-02):** si su línea no lleva `Ready` ni `Applied`, pasa sobre el esqueleto, porque el esqueleto no muta nada.
- **Sin prueba (M-04):** el `Precheck` completo (el esqueleto da `Go()`) y el orden del `Prepare` completo.
- **El hueco más grave:** no hay ningún CT sobre el borrado del origen (B-01).

**Lo que discrimina bien:**
- CT-PBT3A-01: la ventana como oráculo;
- CT-PBT3A-02, 03 y 04: el piloto como oráculo;
- CT-PBT3A-05, 07 y 11;
- CT-PBT3B-01, 02, 03, 05, 07, 08, 09 y 11;
- CT-PBT3C-01 a 04: RED por aserción, porque el texto no existe.

**Los `Skip` de PB-T3b:**
- Solo son admisibles **con D-1 aprobada**. La lista cerrada de OB-T1B.2 (D-PB-06 y D-PB-08) no incluye renombrar ni borrar, y OB-02.2 prohíbe reescribir F4 por analogía.
- Con D-1, el patrón de I-39 es el correcto: `Skip` con motivo, un equivalente que se ejecuta y el registro declarado.
- **Pero hay que saltar también lo que queda vacío:** S-6 y las tres filas vacías de S-2 (I-01).

**Lo que cubre de verdad la red sin AutoCAD:**
- el modelo, el lector, la composición, la disposición y el clasificador;
- el plan y la corrida con dobles;
- el texto del escritor y de la fachada.

**Lo que no cubre:**
- el borrado real (referencias, anidadas, `IsLayout`, la purga);
- el Deshacer con borrado;
- la colisión de nombres;
- el coste.

Todo eso queda para el Smoke-PB de PB-T5, y solo cuando PB-T4 conecte el modelo. **Entre PB-T3c y PB-T5, el borrado es código muerto en producción.** Está declarado; es aceptable.

## 6. D-1 y D-2

**¿La lista es exacta y completa?**
- **Completa:** sí. El censo método a método de las cuatro clases de F4 y de I-54 frente a lo que cambia en PP, UR, WR y la interfaz no encuentra otra guarda que falle.
  - Lo comprobado en PB-T3b:
    - pureza de PP (:30-79);
    - tipos y fábrica (:81-98);
    - `AfterMatch` (:156-190);
    - orden de `Prepare` (:194-227): `BodyOf` toma el primero;
    - pureza de UR, UP y SR (:57-115);
    - `Pilot/` sin `Transaction`;
    - G-4, G-7 y G-9, y el censo de `Task`;
    - M04;
    - el censo de I-54: mismo miembro y mismo primer argumento.
  - Lo comprobado en PB-T3c:
    - las prohibiciones del escritor (`\bErase\s*\(` no casa con `EraseUnit(` ni con `EraseInTransaction(`);
    - la estructura del escritor (tres `PrepareRedraw`, un `StartTransaction`, un `Commit` y una purga);
    - la inyección de Debug (`WriteUnit` sigue con su orden; dos `[Conditional]`);
    - la relectura caller-owned;
    - G-1 y G-2;
    - G-9: `EraseInTransaction` no es ningún término de `WriteTerms`;
    - los textos de las autopruebas de F4, que siguen presentes («internal sealed class RackPilotMutationScope», «public void Seal()»…).
- **Exacta:** **no.**
  - **S-6 no falla: pasa en vacío** (§3). Tres filas de S-2, también.
  - Hay que saltarlas igual, pero la pregunta al Owner dice «no pueden seguir en verde», y eso es falso para S-6.
  - **La comprobación de exactitud «sobre el esqueleto»** (§6.0 y §9) **no puede funcionar**: el RED deja el piloto sin tocar, así que las seis siguen verdes en el RED. Hay que medirla sobre el texto del GREEN, con una corrida local temporal de las seis sin `Skip` y el censo de las precondiciones de las autopruebas.

**¿Hacen falta las seis?**
- **S-1 y S-2, sí.** El único compositor de PP (ComposeViolations, `:102-119`) tiene que llevar el nombre del borrador. Componer en otro archivo rompería el censo cerrado de I-54. Y cambiar el `Name` del sobre releído antes de componer cumpliría la letra, pero debilitaría la premisa en silencio.
- **S-3 a S-6, no:** los impone el diseño elegido (`BuildCore` compartido y `Execution<TState>`).
  - Hay una alternativa **(A′)**: dejar textualmente intactos el `Build` y la `Execution` del piloto y añadir caminos privados paralelos para el modelo completo, en los mismos archivos y con el mismo puerto. No es un segundo escritor: G-2 y G-7 siguen en verde.
  - (A′) solo exige S-1 y S-2, a cambio de duplicar código interino, que PB-T4 retira.
  - Por [OD] §7.3, «Do not silently weaken», la pregunta debe ofrecer (A′) o justificar por qué no (I-01).

**¿Los equivalentes conservan la seguridad?**
- **E-1, E-4 y E-5:** sí.
- **E-2 y E-6:** sí en el GREEN, pero no en el RED (I-03).
- **E-3:** **no cubre todo lo que cubría S-3**:
  - pierde `PreflightInnerSources(` y `RackSystemKind.PushBack`;
  - la disponibilidad (`Decode`, `Evaluate`, `Cortes(`, `IsComposite`) se va a `PushBackViewDispositions.Of`, que E-3 no lee (I-02).

**D-2:**
- **Como decisión, sí es informativa.** D-PB-04 autoriza borrar, y OB-04.1 de [P] ya prevé «una primitiva caller-owned y su guarda de fachada».
- **Como registro, no.** [OD] §7.3 manda «create a declared superseding characterization/change record» para todo cambio de comportamiento autorizado. La premisa «el escritor nunca borra» está escrita en la guarda (UpdateGuard:188, «Never: the lock, an erasure…») y en WR:26 y WR:225.
- Le corresponde una entrada en `superseding-characterization.md`, sin `Skip`, en la emisión de PB-T3c (I-10).

## 7. Los contratos

**Lo que está bien:**
- **Los `AllowedWriteScope`:**
  - las cuentas cuadran: 14, 11 y 4;
  - los `TaskId` tienen 26, 24 y 26 caracteres;
  - los filtros no se pisan: `PushBackFullDraft`, `PushBackFullWrite`, `PushBackWritePathGuardTests` frente a `…PluginGuardTests`, `Workspace`.
- **Las paradas estándar y las de [DP]**, copiadas como en PB-T1.
- **C17**, con el texto literal del Owner.
- **El tamaño:** PB-T3b subiría unas 75 entradas al enumerar `Workspace/` (I-05). Aun así, los tres contratos son menores que los 56 KB de PB-T1b, y F4B-E21 los cubre.
- **AUTOMATION_PLAN §16:**
  - las delegaciones son sucesivas (16.6);
  - la cadena de RED y la `AuthorityRevision` de una corrección siguen 16.3 y 16.8;
  - el `Ci` del RED sigue 16.9 n.º 9.

**Lo que no se puede cumplir:**
- **El `ForbiddenWriteScope` de PB-T3b** contiene `tests/RackCad.Tests/Workspace/`, porque viene de la lista curada. Su `AllowedWriteScope`, en cambio, tiene cuatro archivos dentro de esa carpeta. La sintaxis de prefijos (16.5) no admite excepciones, y `Scope` (16.9 n.º 7) exige que no se corten (I-05).
- **La lista cerrada de PB-T3a** (INV-PBT3A-03 e INV-13):
  - no admite los constructores `internal` que necesitan `PushBackViewDispositionPlan`, `PushBackChangeEntry` y `PushBackConflictClassification`, porque los construye otra clase;
  - deja en duda los constructores privados de los tipos con fábrica y el constructor implícito público de `PushBackFullDraftStateComparer`;
  - es la lección M10 repetida (I-06).
- **INV-PBT3B-04 («El piloto no cambia»)** choca con la barrera, que sí cambia el piloto sobre un diseño bloqueado. C03 la exceptúa, pero ninguna prueba lo fija (I-08).
- **La Snapshot** del contrato está anticuada (I-09).

## 8. Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **B-01** | **BLOQUEANTE** | **Borrar la vista de origen rompe POST.** El origen del plan es uno de los miembros mutables: puede ser un lateral (PP:357-361). Si ese lateral queda obsoleto, va a `Erasures` y se borra en MUTATE. POST relee con `plan.OriginKey` (UR:481) → la regla 2 del lector da «el origen ya no está en el dibujo» (IR:162-166) → `POST_NOT_ADMITTED` (UR:167) → `AppliedPostRereadFailed` (UR:483), **con la escritura ya confirmada**. Ni una regla, ni un CT, ni una parada lo cubren | PP:357-361; UR:399-405, :481-483; IR:162-166 | Una regla en PREPARE, para el modelo completo:<br>• o `Invalid` con un texto propio cuando `OriginKey` ∈ `Erasures` («la vista desde la que se abrió el rack quedaría obsoleta…»);<br>• o un origen de POST reanclado a un miembro `Redraw`, en ordinal, con la base nueva del borrador definida.<br><br>Más un CT en PB-T3b (CT-02 y 07) y una fila en el Smoke-PB |
| **I-01** | IMPORTANTE | **D-1 está mal enunciada.**<br>(a) S-6, y tres filas de S-2, **pasan en vacío**; no fallan.<br>(b) La exactitud no se puede medir «sobre el esqueleto».<br>(c) S-3 a S-6 dependen de una elección de diseño, y la alternativa (A′) solo exige S-1 y S-2: no se ofrece al Owner | §3 (S-6); IntegrationGuard:880-910; `WorkspaceHostSourceSupport.cs:67-89`; PrepareGuard:229-251; UpdateGuard:126-141 | Reescribir D-1: «seis que fallan o quedan vacías», medidas sobre el GREEN con una corrida temporal. Añadir la opción (A′) con sus costes, o justificar el diseño genérico. Ajustar R-6 y §9 |
| **I-02** | IMPORTANTE | **E-3 no cubre todo lo de S-3:** faltan `PreflightInnerSources(` y `RackSystemKind.PushBack`, y no lee el cuerpo de `Of`, donde pasan a vivir `Decode`, `Evaluate`, `Cortes(` e `IsComposite` | PrepareGuard:235-250; D-PBT3-06; D-PBT3-09 (e) | E-3, sobre `BuildCore` **y** el cuerpo de `PushBackViewDispositions.Of`, con las mismas agujas de S-3 |
| **I-03** | IMPORTANTE | **E-2 y E-6 pasan en el RED.** Son autopruebas: sobre un texto que ya viola, sus mutaciones no cambian nada, o no encuentran su texto y siguen violando. El filtro tiene `ExpectRed` en `true` → `RedPart` en `fail` o C04 | IntegrationGuard:902-910 (S-6 no tiene precondición); PrepareGuard:133-139 (S-2 sí: `Assert.Contains(from, plan)`); 16.9 n.º 10 | Cada E-2 y E-6 empieza con `Assert.Contains` de cada literal de la forma nueva y con `Assert.Empty(<reglas>(real))`, y se dice así |
| **I-04** | IMPORTANTE | **CT-PBT3C-05 «pasa en el RED» pero lee la fachada, que el RED no crea.** Con la regla de CT-PBT1B-14, falla | §6.3 «El RED: solo las dos clases de prueba»; `PushBackSharedReferenceGuardTests.cs:211-213` | El RED de PB-T3c crea un esqueleto inerte de la fachada (la clase y el método, que lanza): CT-01 y CT-04 siguen fallando. O bien CT-05 trata la fachada como opcional hasta el GREEN, declarado |
| **I-05** | IMPORTANTE | **El `ForbiddenWriteScope` de PB-T3b corta su `AllowedWriteScope`.** La lista curada de PB-T1 lleva `tests/RackCad.Tests/Workspace/`, y el alcance permitido tiene cuatro archivos dentro. El §6.0 se contradice: «las listas curadas» para todas, y `Workspace/` «también en PB-T3a y PB-T3c» | `F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json`; `F4B-PBT1B-…/coordinator/gate-contract.json`; `I-64-f4b-pbt1-measurements.md:155`; 16.5 y 16.9 n.º 7 | En PB-T3b, sustituir el prefijo por la lista de los 79 archivos de `Workspace/` menos los tres de guardas (y el doble, o mover el doble a `tests/RackCad.Tests/`). Lo mide el generador |
| **I-06** | IMPORTANTE | **La API cerrada de PB-T3a no se puede implementar:**<br>• tres tipos sin constructor ni fábrica los construye otra clase y necesitan un constructor `internal`, que INV-03 prohíbe e INV-13 no admite;<br>• quedan en duda los constructores privados de los tipos con fábrica y el constructor implícito público del comparador.<br><br>Es la lección M10 | D-PBT3-06 y 07; INV-PBT3A-03; INV-13 (§6.0); PESC:31 | Poner en el §3 los constructores (`internal`, con su firma) de esos tres tipos. Ampliar INV-13 a los constructores privados y a los campos `readonly` privados de instancia sin estado mutable, o declararlos parada con nombre |
| **I-07** | IMPORTANTE | **CT-PBT3B-10 no llega a AUTH-13 en la escritura.** Con la relectura de CT-PBT3A-04 y un borrador de lectura limpia, sale **Conflict** antes de la admisión, nunca `NotAdmitted` ni `NotAppliedAdmission`. Para llegar hay que fabricar la base del borrador, y el paquete no da la receta | PP:322-326; UR:315-324, :402-405; el patrón de CT-PBT3B-11 (relectura fabricada) | La receta: un borrador iniciado con una lectura admitida fabricada (el constructor interno), cuya `StaleBase` es la del escaneo con el campo desconocido, y sus líneas literales. Declarar que en uso real AUTH-13 actúa al abrir (sin borrador) y que al escribir solo aparece el Conflict |
| **I-08** | IMPORTANTE | **La barrera del piloto, que es lo único de PB-T3 que llega al panel antes de PB-T4, no tiene prueba.** Un Worker que la omita pasa todos los CT. El resumen dice «sin ningún cambio de comportamiento» y INV-PBT3B-04, «el piloto no cambia». Además, R-8 midió el corpus de PB-T1a, no los diseños de las clases de F4: la conclusión es por inferencia | D-PBT3-01; D-PBT3-09 (d2); CT-PBT3B-06; R-8 | Añadir a CT-06 un borrador del **piloto** sobre el diseño de un sentido bloqueado (la celda que no cabe), con la escala cambiada → `Invalid(BlockedPrefix + StatusOr)`. Corregir el resumen e INV-04. Declarar el cambio visible y el texto `BlockedPrefix` en el panel. Añadir el censo de los diseños de las clases de F4 a la remedición |
| **I-09** | IMPORTANTE | **La Snapshot I está anticuada:** la línea base activa del Controller es la **J** | `owner-config-snapshot-j-decision.md`; `f4b-dispositions.md:572-596`; `I-64.yml:57` | Snapshot J en la precondición 6 y en P-01, y la Snapshot vigente en la lista de la remedición |
| **I-10** | IMPORTANTE | **D-2 necesita un registro declarado,** aunque no sea una decisión: [OD] §7.3 lo manda para todo cambio autorizado. La premisa está en la guarda y en dos comentarios del escritor | [OD] (el párrafo de T1B); UpdateGuard:188; WR:26 y :225 | Una entrada en `superseding-characterization.md`, sin `Skip`, en la emisión de PB-T3c, con la autoridad (D-PB-04, OB-04.1), la guarda que sigue y las nuevas (CT-PBT3C-01 a 04). Cambiar también el comentario de WR:225 |
| **I-11** | IMPORTANTE | **CT-PBT3A-09 y 10 no tienen líneas literales** («aplicadas a mano»), y el clasificador compara hojas del diseño **persistido**: un cambio de campo puede arrastrar hojas derivadas (por ejemplo, longitudes de módulos por la tolerancia o el frente, `DynamicRackLevelGeometry.cs:269`). Saldrían entradas de `Modules` estructurales que nadie previó. Quedan dos salidas: que el Worker escriba lo esperado desde su implementación (tautología) o la parada C07 | D-PBT3-07; CT-09 y 10 | Dar las líneas literales, o un oráculo mecánico independiente. Declarar en el diseño qué pasa con las hojas derivadas: si se informan, con qué grupo, o si se excluyen las no editables |
| M-01 | MENOR | CT-PBT3B-04 (iv) es tautológico | BM:62-66 | Oráculo: el hecho medido (las 72 ranuras con `SideB` nulo) o la ventana |
| M-02 | MENOR | CT-PBT3B-12 puede pasar sobre el esqueleto | §5.3 | Que la línea lleve la clase del `Prepare` (`Ready`) y de la corrida (`Applied`) |
| M-03 | MENOR | Sin precisar:<br>• la sobrecarga del constructor y `Equals` de `PushBackPilotPostExpectation` con `ErasedKeys`;<br>• que el doble con borrado devuelva `PreparedBlockMatches` también para las claves a borrar | D-PBT3-09; §5.0 | Escribirlo |
| M-04 | MENOR | Ningún CT del `Precheck` completo ni del orden del `Prepare` completo; el esqueleto `Go()` pasa | D-PBT3-09 y 10 | Un caso por resultado en CT-PBT3B-01 o en uno nuevo |
| M-05 | MENOR | Ancla de [PKC2]: §6.7 está en `:579-581`; `:655` es §6.8 | §1 | Corregir |
| M-06 | MENOR | El §1 atribuye §7.3, §7.4 y D-PB-10 al [OD] con la numeración de [P] | §1 | Citar como «[P] §7.3» o con el texto del [OD] |
| M-07 | MENOR | La decisión de la ExtensionData no está en el repo (A-T3-7) y no figura entre las precondiciones de emisión | §1; §10 | Precondición: registrada en `F4B-DRAFTS/` con su F4B-E y dentro de la `AuthorityRevision` |
| M-08 | MENOR | El texto del frontal B («el rack deja de tener lado B») es inexacto para un rack de un sentido que ya trae un frontal con `Section` 2 de una conversión clásica (BM:25). Ese rack queda **para siempre** fuera del panel, como hoy con el piloto | D-PBT3-09; BM:25 | Ajustar el texto y declararlo como limitación |
| M-09 | MENOR | El coste: cada «Actualizar» completo hace varias pasadas completas (la materialización del piloto y tres `Pass` al leer, otra vez en `ForWrite` y otra en POST), no solo el comparador | D-PBT3-03 y 04; R-T3-07 | Añadirlo a R-T3-07 y a R-PB-06 |
| M-10 | MENOR | `Pass` en producto duplica la adopción de la ventana (W:659-704): dos implementaciones de la misma secuencia, que solo ata CT-PBT3A-01 | D-PBT3-03 | Declararlo interino y llevarlo a PB-T2 o PB-T4 (que la ventana use el asiento) |
| M-11 | MENOR | CT-PBT3A-04 toma 2 de las rutas anidadas de AUTH-13; «a cualquier nivel» queda por muestra | `I58F1Matrix.cs:97-102` | Recorrer la lista `Nested("pushback")`, copiada |
| M-12 | MENOR | La incidencia de procedimiento de R-12 (`python -I -` con heredoc) **repite** la de la medición H-12 de PB-T1. Es un modo de fallo repetido | `I-64-f4b-pbt1-measurements.md:227`; R-12 | Devolverlo al Coordinator como modo repetido, no solo declararlo |
| M-13 | MENOR | CT-PBT3C-01 no prohíbe que la fachada nombre el piloto (`NamesThePilot` de G-9). G-9 lo cazaría como C16 | IntegrationGuard:679 | Añadir la prohibición a CT-01 |
| M-14 | MENOR | El motivo del `Skip` cita «D-1», sin su F4B-E. El `MinSelected` del filtro `Workspace` depende de cómo cuente xUnit una `Theory` saltada (probablemente un caso, no cinco) | §5.3; §6.2 | Citar el F4B-E y fijar en la emisión qué cuenta como `Selected` |
| M-15 | MENOR | Dependencia cíclica de namespaces `Pilot` ↔ `Draft` (PP y UR usan `Draft`; `Draft` usa las constantes y el lector del piloto). Ninguna guarda la ve | D-PBT3-04, 09 y 10 | Declararla interina hasta PB-T4 |
| M-16 | MENOR | Erratas:<br>• R-6 habla de un «único `Create`», frente al tipo propio de D-PBT3-09;<br>• INV-PBT3A-06 dice «fuera de `Draft/`», frente a CT-12, que admite PP y UR;<br>• falta el comentario de WR:225 | §3 de este informe | Corregir |

## 9. Veredicto

**NECESITA v2.** **B/I/M = 1 / 11 / 16.**

- **El diseño cumple el [OD]** en campos, módulos y frentes, lado B, renombrado, la barrera y no crear vistas. Respeta D-PB-05 sin conectar, la decisión de la ExtensionData y D2. **No lo cumple** en el borrado del origen (B-01).
- **Los hechos se sostienen** salvo cuatro: S-6, la Snapshot, el alcance prohibido y una ancla de [PKC2].
- **Los contratos y las pruebas** tienen siete defectos que harían parar o fallar la cadena: I-03 a I-08 e I-11.

**Decisiones para el Owner:**
- **D-1 (material; antes del contrato de PB-T3b):** se mantiene, **reformulada**:
  - la lista exacta, con S-6 y las filas de S-2 que quedan vacías, medidas sobre el GREEN;
  - y la opción (A′), que solo sustituye S-1 y S-2 a cambio de duplicar código interino, frente a la (A) del paquete, que sustituye seis con un núcleo genérico.
- **No es del Owner:**
  - D-2: no es decisión, pero necesita su registro (I-10);
  - el frontal B: se queda el valor por defecto;
  - los textos nuevos y la división en tres;
  - la regla para el origen borrado (B-01), si se resuelve con rechazo y remisión o con reanclaje en POST, como regla técnica del escritor.
- **No aparece ninguna otra decisión material nueva.**

---

## §10. Recomprobación de la v2

**Veredicto: NECESITA v3.** B/I/M = **0 / 3 / 4**.
- **B-01 está resuelto,** y bien: el reanclaje es correcto y completo, y la afirmación sobre el clásico es cierta.
- **D-1 es exacta:** seis con (A) y tres con (A′).
- **Los 28 hallazgos de la v1 están resueltos,** o tienen una razón válida para aplicarse de otro modo.
- **Quedan tres importantes, todos de texto y baratos de arreglar:**
  - la pregunta de D-1 no es simétrica en lo que deja a PB-T4;
  - la parada C13 de PB-T3b prohíbe lo que el diseño (A) exige;
  - la medición sobre el GREEN choca con C02.
- **Ninguno necesita al Owner.** No hay ninguna decisión material nueva.

### 10.1 Método

Las mismas reglas que en §1. Se leyó:
- la v2 entera (1 327 líneas), `v2-delta.md` y `pbt3-measurements.md` (R-13 a R-17);
- por objetos git, en `95373af6`:
  - `git diff --stat 2f5d3dec 95373af6 -- src` sale vacío. Entre las dos solo cambian cinco archivos de pruebas de PB-T1b y documentos de evidencia;
  - [P] y `AUTOMATION_PLAN.md` no cambian;
- RPC:120-266 y :318-384;
- `PushBackCompositeDiagnostics.cs:90-150`, `PushBackCompositeEditorState.cs:620-635`, `PushBackDesign.cs:70` y `PushBackSystem.cs:47`;
- `PushBackOutputGateTests.cs:30-46` y `PushBackCellDepth.cs:35`;
- `PilotKit.JsonDiff` (`WorkspaceDraftPilotStateTests.cs:106-150`);
- WHC (las seis `ReadIntent` con `ChosenReference`);
- `f4b-dispositions.md:641-711` (F4B-E22 a F4B-E26).

### 10.2 Los 28 hallazgos de la v1

| Hallazgo | Estado en la v2 | Comprobación |
|---|---|---|
| B-01 | **Resuelto** (V2-01) | §10.3 |
| I-01 | **Resuelto, y la precisión del redactor es correcta** | La v1 se equivocaba al decir que (A′) solo exige S-1 y S-2. S-4 cuenta en todo UR un solo `mutation.Dispose(` y un solo `scope.Dispose(` (UpdateGuard:134-135). G-7 obliga a que las llamadas al puerto estén solo en UR (IntegrationGuard:651-658), así que una segunda corrida tiene que ir en UR. Esquivar S-4 con otros nombres de variable la debilitaría. (A′) = S-1, S-2 y S-4. Queda N-01 sobre la pregunta y N-04 |
| I-02 | Resuelto | E-3 lee `BuildCore` y el cuerpo de `Of` con todas las agujas de S-3 (PrepareGuard:235-250), `PreflightInnerSources(` y `RackSystemKind.PushBack` incluidas |
| I-03 | Resuelto | E-2 y E-6 empiezan con `Assert.Contains` de los literales nuevos y `Assert.Empty(<reglas>(real))`. En el RED faltan esos literales (`new Execution<`, `RackEmbedDocument source, string name`): fallan por aserción. E-6, además, exige que cada mutación cambie el texto |
| I-04 | Resuelto | El esqueleto de la fachada en el RED (cuerpo `throw new NotSupportedException();`): CT-05 pasa y CT-01 a 04 siguen fallando |
| I-05 | Resuelto, con un resto | La lista explícita de 76 archivos y el doble fuera de `Workspace/`. Con (A′), el resto: N-05 |
| I-06 | Resuelto en PB-T3a | Los constructores, con su firma, y PBT3A-C13 por nombre. En PB-T3b, el problema reaparece de otra forma: N-02 |
| I-07 | Resuelto | La receta con la lectura fabricada es alcanzable: el materializador completo es tolerante y admite el texto con el campo anidado. La base de `BuildStaleBase` del mismo escaneo coincide con la relectura. AUTH-13 rechaza en la regla 11 (IR:217-222). La corrida para antes de `PrepareUnits` (UR:315-324): `[Begin,ReadForPrepare,Dispose:scope]`. El uso real queda bien declarado |
| I-08 | Resuelto | CT-PBT3B-06 (ii) |
| I-09 | Resuelto | Snapshot J en la precondición 6 y en P-01 |
| I-10 | Resuelto | La entrada de D-2 sin `Skip`, con :26 y :225 de WR |
| I-11 | Resuelto | `PilotKit.JsonDiff` usa las mismas rutas `$.A.B[3].C`, hoja a hoja, con `<absent>` y contenedores vacíos como hoja (`WorkspaceDraftPilotStateTests.cs:100-150`). El oráculo es independiente y no necesita prever las hojas derivadas. Las anclas literales no dependen de ellas |
| M-01 a M-06 | Resueltos | El hecho de M-i generalizado; `prepare=Ready corrida=Applied`; los constructores y el doble; CT-PBT3B-15; las anclas de [PKC2]; la atribución a [P] |
| M-07 | Resuelto; el estado ha cambiado | En `95373af6`, F4B-E24 y F4B-E25 ya están en git: en `f4b-dispositions.md:684` y `:693` y en `F4B-DRAFTS/coordinator/`. La precondición 8 ya se cumple, salvo meterlas en la `AuthorityRevision`. R-17 dice «no está en git» porque se midió en `a18ba345`: N-07 |
| M-08 | Resuelto | El texto nuevo y la limitación |
| M-09 | **Resuelto, declarándolo** | Es lo que pedía la v1: R-T3-07 cuenta las pasadas (unas nueve) y van a R-PB-06 y A-T3-4 |
| M-10 | **Resuelto, declarándolo** | Es lo que pedía la v1: interino, A-T3-9 |
| M-11 | Resuelto | Las 28 rutas, con su línea de control |
| M-12 | Resuelto | Se devuelve al Coordinator como modo repetido |
| M-13 | Resuelto | `NamesThePilot` |
| M-14 | **Resuelto con un marcador, que es válido** | `F4B-E<n>` lo pone la sesión al registrar D-1, antes del contrato literal. El `MinSelected` de `Workspace`, desde la TRX del RED, que ya lleva los `Skip` |
| M-15 | **Resuelto, declarándolo** | Es lo que pedía la v1: interino, A-T3-10 |
| M-16 | Resuelto | Las tres erratas |

### 10.3 El bloqueante B-01: el reanclaje

**Correcto.**
- `PostOriginKey` es `OriginKey`, o, si el origen se borra, la primera clave `Redraw` en ordinal.
- **Siempre existe:** en el modelo completo, (e) deja como resultado posible solo `Redraw` y `ObsoleteLateral` (todo lo demás es `Invalid`), y `AllObsolete` es `Invalid`.
- **POST relee con ella:**
  - pasa la regla 2 (IR:162-166) y AUTH-13 en `Single`, porque todos los miembros llevan el mismo diseño;
  - `ComparePost` cuenta solo los `Redraw` (UR:169-173);
  - `Applied(read)` rebasa el borrador con la base de POST, sin la clave borrada, y con el estado inicial del texto nuevo de `PostOriginKey`.
- **El siguiente «Actualizar»:** `ChooseOrigin` (UR:521-534) encuentra ese miembro por su texto. Es único, porque cada sobre lleva su vista y su sección.

**Los casos que se pidieron:**
- **El Deshacer:** el reanclaje no añade ninguna operación sobre el dibujo. Los `EraseUnit` van dentro de la única transacción de MUTATE, y POST, en el mismo contexto de órdenes. Un `U` lo deshace todo, como en S2-03; el Smoke-PB lo mide, con su fila nueva.
  - **Tras el `U`:** el borrador queda limpio sobre un dibujo que ya no es su base. Es el mismo caso que hoy tiene el piloto tras un `U`: el texto del origen vuelve atrás, el anfitrión lo ve como obsoleto, y un «Actualizar» con el borrador en Dirty da `NoOriginReason` o Conflict. No hay regresión.
- **Sin ninguna vista redibujada:** `AllViewsObsoleteReason`, sin escribir nada, igual que M-2b. Si solo quedan frontales B o vistas sin dirección, también `Invalid`. Si los `Redraw` no tienen ninguna referencia en un espacio de diseño, `NoSurvivorsReason`, que es más estricto que el clásico y está declarado (§8.3).
- **El conflicto previo:**
  - en PREPARE, la comparación cruda de la base va antes de la admisión y no cambia;
  - en MUTATE, el origen todavía existe (se borra después de la relectura), así que `sameOrigin` con `plan.OriginKey` sigue valiendo (UR:398-405), ahora con `_origin` en lugar de `_input!.InitialDraftState.Origin`;
  - el destino incluye `Erasures`.
- **Un solo «Actualizar» lógico:** se mantiene: un `Begin`, un `BeginMutation`, un `Seal`, un `Post` y una sola relectura de POST (INV-PBT3B-06).
- **¿«El clásico hace lo mismo»?** **Sí,** comprobado en el código:
  - RPC:198 toma **todas** las definiciones del GUID, la elegida incluida;
  - el bucle de :272-346 no trata aparte la vista elegida: un lateral sin corte va a `staleViewBlocks` (:329-333);
  - :348-352 lo borra si queda alguna superviviente.

  El clásico no tiene borrador que reanclar; la igualdad es de resultado.
- **Ninguna guarda de F4 fija el `OriginKey` de POST** (`git grep OriginKey` en `tests/RackCad.Tests/Workspace/*Guard*.cs`: 0 en `95373af6`).

**Lo que queda, bien declarado:** en el panel, las lecturas del anfitrión usan `request.ChosenReference` (WHC:328, :409, :503, :590, :675 y :759). Tras borrar la vista elegida, PB-T4 tiene que usar el origen del borrador (A-T3-8). Mientras el modelo no esté conectado, en PB-T3 no se alcanza.

### 10.4 D-1 reformulada

**Exacta**, leída guarda a guarda:
- **Con (A):**
  - fallan S-1 (ComposeViolations, PrepareGuard:102-119), S-3, S-4 y S-5;
  - S-2 falla en las filas 1 y 3 y queda vacía en las 2, 4 y 5;
  - S-6 pasa en vacío.

  **Seis.**
- **Con (A′):**
  - S-1 falla siempre: las dos sobrecargas de `ComposeMemberEnvelope` dan una violación sea cual sea su orden, porque ninguna tiene `source.Id, source.Name,` junto a `Compose(source,`;
  - S-2 falla en la fila 1;
  - S-4 falla por sus cuentas;
  - S-3 sigue en verde: el `Build` del piloto conserva todas sus agujas, la barrera no añade `.Name` ni `Erase`, y `BuildFull(` no casa con `Build\s*\(`;
  - S-5 y S-6 siguen en verde: `BodyOf` toma el `Execute` y el `OutcomeOfAnEscape` del piloto, que no cambian.

  **Tres.**
- **Ninguna otra guarda cambia con (A′):** G-4, G-7, el censo de `Task`, la pureza de UR, el censo de I-54 (un único `Compose`, en la sobrecarga de cinco argumentos, con `source` como primer argumento) y los tipos y la fábrica de PrepareGuard:81-98.

**Las opciones son reales,** y bastan: las mezclas (plan genérico con la corrida duplicada, o al revés) dan cuatro o cinco, así que quedan dominadas. Pero la pregunta y la tabla del §8.2 **no son simétricas en lo que dejan a PB-T4** (N-01).

### 10.5 Hallazgos de la recomprobación

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **N-01** | IMPORTANTE | **La pregunta de D-1 no es simétrica.**<br>• La opción (A′) dice que «PB-T4 tendrá que sustituir las otras tres», pero la (A) calla lo que deja a PB-T4. La tabla lo llama «ajustar E-5».<br>• Con (A), al quitar el piloto, PB-T4 tendrá que sustituir con registro **E-3** (su cláusula «el piloto conserva `ViewUnavailablePrefix`») y **E-5** («las dos `Execute`»). Editar una prueba existente está prohibido en F4B (INV-07; OB-T1B.1), así que no es «ajustar».<br>• Con (A′), además de S-3, S-5 y S-6, PB-T4 tendrá que sustituir **E-1** (la sobrecarga de cuatro argumentos) y **E-4** («por corrida»).<br>• El total es el mismo en las dos (unas ocho), y por eso «(A′) no ahorra» se sostiene. Pero tal como está, la pregunta inclina al Owner hacia la opción recomendada | §8.2 (tabla y pregunta); v2-delta, la pregunta; E-1, E-3, E-4 y E-5 (§5.3); INV-07 | Que las dos opciones digan lo que dejan a PB-T4: con (A), E-3 y E-5; con (A′), S-3, S-5, S-6, E-1 y E-4. O que ninguna lo diga y se remita a A-T3-1. Cambiar «ajustar» por «sustituir con registro» |
| **N-02** | IMPORTANTE | **PBT3B-C13 prohíbe lo que exige la variante (A):**<br>• Su primera cláusula para ante cualquier «change of the signature of an existing member of PP or UR», sin limitarse a lo público o interno como INV-PBT3B-03. El diseño (A) cambia la firma de miembros privados que ya existen: `ComposeMemberEnvelope` gana `name` («es el mismo miembro», D-PBT3-09 (g)), y en `Execution<TState>` todos los miembros pasan de `DraftApplyOutcome<PushBackPilotDraftState>` y `DraftSession<PushBackPilotDraftState>` a `TState` (`Run`, `OutcomeOfAnEscape`, `Prepare`, `Mutate`, `Write`, `Post`, `WithAbortFailure`, `WithScopeDisposeFailure`, `PostFailed`, `NotAppliedFailure`, `NotAppliedPreflight` y el constructor).<br>• PBT3B-C13 y PBT3C-C13 paran ante cualquier nombre nuevo «of any visibility» fuera de su lista, mientras que INV-13 (común) admite métodos privados. PBT3A-C13 sí remite a INV-13. Es la lección M10 otra vez | §6.2 PBT3B-C13; INV-PBT3B-03; D-PBT3-09 (g); D-PBT3-10; §6.0 INV-13; UR:227-587 | En C13, «existing public or internal member», y declarar que los miembros privados de `Execution<TState>` y `ComposeMemberEnvelope` cambian de firma como dice el diseño. Y decidir una sola regla para los privados: o la lista cerrada (y que INV-13 lo diga), o INV-13 (y que C13 lo cite, como PBT3A-C13) |
| **N-03** | IMPORTANTE | **La medición sobre el GREEN (INV-PBT3B-07) obliga a reactivar en local los métodos con `Skip`,** porque una prueba saltada no se ejecuta con un filtro. Pero INV-07 («ninguna prueba existente se … reactiva») y C02 («re-enabling … an existing test») lo prohíben. Es una contradicción del contrato, como la de M14 | §5.3, «La medición sobre el GREEN»; INV-PBT3B-07; INV-PBT3x-07; C02 | Exceptuar en INV-07 y C02 la reactivación **temporal, local y sin commit** de INV-PBT3B-07, y exigir en la entrega que el `git diff -U0` final de los archivos de guardas solo lleve los `Skip` |
| N-04 | MENOR | Con (A′), la tabla del §8.2 dice «Quedan vacías: ninguna», pero cuatro filas de S-2 pasan en vacío (2 a 5): solo la fila 1 pierde su texto `from`. S-2 se salta igual. La medición esperada de (A′) no trae el censo de filas de S-2 | §8.2; §5.3 | Corregir la celda y añadir el censo de filas para (A′) |
| N-05 | MENOR | Con (A′), el `ForbiddenWriteScope` de PB-T3b tiene que dejar fuera solo dos archivos de guardas: 77 de 79, y `IntegrationGuard` queda prohibido. El §6.0 fija «menos los tres» | §6.0; §6.2 (`AllowedWriteScope`, «Con (A′), solo los dos primeros») | «Menos los archivos de guardas de la variante elegida» |
| N-06 | MENOR | CT-PBT3B-07 (ii): «un segundo `Prepare` sobre esa base da `Ready`» exige un estado actual cambiado. Tras `Applied`, el borrador está limpio y da `NothingToApply` | §5.3, CT-07 | Decir la edición (por ejemplo, la escala a 3,0) antes del segundo `Prepare` |
| N-07 | MENOR | El estado de PB-T1 ha cambiado en `95373af6`:<br>• PB-T1b está CERRADA (F4B-E26; CI del GREEN `44438060317`, Core 19 086 y UI 2 437);<br>• F4B-E24 y F4B-E25 ya están en git.<br><br>R-0, R-17 y las precondiciones 1 y 8 siguen con lo medido en `a18ba345` | `f4b-dispositions.md:684-711`; `I-64.yml:57`; el mensaje de `95373af6` | Actualizarlo en la remedición. No cambia ningún CT |

### 10.6 Regresiones y decisiones nuevas

**Regresiones:** no hay en el diseño.
- El piloto sigue igual salvo la barrera, que ahora está declarada y probada:
  - `PostOriginKey` = `OriginKey`;
  - `Erasures` y `ErasedKeys` vacías;
  - el constructor de siete argumentos se conserva;
  - las propiedades nuevas son de solo lectura (WorkspacePilotPrepareTests:1958-1962).
- **CT-PBT3B-06 (ii):**
  - su línea literal coincide con el texto de `PushBackCompositeDiagnostics.cs:135-136`, con el mínimo de 2 de `PushBackCellDepth.cs:35`;
  - `IntentDiagnostics()` evalúa las estructuras manuales aunque el rack no sea compuesto (`PushBackCompositeEditorState.cs:629-631`);
  - `PushBackDesign.IsComposite` depende solo de `SideB` (`PushBackDesign.cs:70`), así que `pb-01-base` con `StructureOverrideA = 1` sigue siendo de un sentido y el piloto lo admite;
  - la línea de autocomprobación cubre el riesgo que no se midió: que el resolver lance.
- **Lo que no son regresiones,** sino contradicciones nuevas del texto de los contratos: N-02 y N-03.

**Decisiones materiales nuevas:** ninguna.
- El reanclaje es una regla técnica del escritor, que iguala al clásico.
- (A′) es una opción dentro de D-1.
- El cambio visible de la barrera en el piloto ya lo autoriza el [OD] («blocking authored write on diagnostics/errors») y está declarado.

**Siguiente paso:** una v3 que arregle N-01 a N-03, más los menores. Con eso, y tras la remedición sobre el cierre de PB-T1, el paquete queda **listo para emitir** PB-T3a, que no depende de D-1. PB-T3b espera a D-1, con la pregunta ya corregida.

---

## §11. Recomprobación de la v3

**Veredicto: NECESITA v4, una corrección acotada.** B/I/M = **0 / 1 / 1**.
- **Los siete hallazgos de la §10 están resueltos.** C13 por nombre admite exactamente lo que prescribe el diseño, en PB-T3b y en PB-T3c, y la medición sobre el GREEN ya no choca con C02.
- **Queda un importante: los números de la pregunta de D-1 no son exactos.**
  - Las autopruebas E-2 y E-6 empiezan con `Assert.Empty` de las reglas de E-1 y E-5, así que caen con ellas. Con las reglas leídas en sentido estricto, salen **seis y tres** con (A) y **tres y seis** con (A′): **nueve** en total, no ocho.
  - Con una lectura más laxa, la (A′) ahorraría sustituciones.
  - En ninguna de las dos lecturas salen «seis y dos, tres y cinco, ocho».
- **Las opciones son reales y completas.** No hay regresiones ni decisiones materiales nuevas.

### 11.1 Método

Las mismas reglas. Se leyó:
- la v3 (§6.0 a §6.3, §5.3, D-PBT3-09b, §8.2 y §10) y `v3-delta.md`;
- por objetos git, en `833ea6bf`:
  - `git diff --name-only 2f5d3dec 833ea6bf -- src` sale vacío;
  - UR:229, :521 y :537;
  - PBT1C-C13 del paquete de PB-T1 v2.8 (`:1336-1340`);
  - `f4b-dispositions.md:684-761` (F4B-E24 a F4B-E27);
  - el contrato emitido de PB-T1c.

**El worktree no se tocó.**

### 11.2 Los hallazgos de la §10

| Hallazgo | Estado | Comprobación |
|---|---|---|
| **N-01** | **Resuelto en la forma, no en los números** | Las dos opciones dicen ya lo que dejan a PB-T4, con «sustituir con registro». Los números, en §11.3 (O-01) |
| **N-02** | **Resuelto** | **PBT3B-C13 con (A)** enumera exactamente lo que prescriben D-PBT3-09 y D-PBT3-10:<br>• 22 en PP, con `PushBackFullPrepareInput` y sus nueve miembros, un segundo `Prepare`, `BuildCore`, `BuildFull`, las cuatro constantes, y `Erasures`, `PostOriginKey`, `ErasedKeys` y los dos constructores internos nuevos;<br>• 10 en UR: el segundo `Precheck`, `Execute` y `ComparePost`, `ComparePostCore`, las dos constantes y los cuatro campos;<br>• 2 en la interfaz.<br><br>La firma solo para en lo público o interno, como INV-PBT3B-03. Los dos cambios privados están declarados. La lista de miembros de `Execution` que cambian `PushBackPilotDraftState` por `TState` coincide con UR:227-587: `_draft`, el constructor, `Run`, `OutcomeOfAnEscape`, `Prepare`, `Mutate`, `Write`, `WithAbortFailure`, `Post`, `ChooseOrigin`, `SurvivorsAndLayers`, `WithScopeDisposeFailure`, `PostFailed`, `NotAppliedFailure` y `NotAppliedPreflight`. `ScanOf` y `DesiredNamesOrNone` no llevan el tipo y no cambian. La precisión del redactor (`ChooseOrigin`, `SurvivorsAndLayers` y `_draft`) es correcta (UR:229, :521, :537).<br><br>**Con (A′),** la lista es coherente con D-PBT3-09b: `FullExecution` con los nombres de `Execution`, y la sobrecarga de cinco argumentos.<br><br>**PBT3C-C13:** las cinco declaraciones de D-PBT3-11 (`_erasures` dos veces, `EraseUnit`, la clase de la fachada y su método) y el parámetro nuevo del constructor interno. `Reread`, `PrepareUnits` y `Dispose` solo cambian de cuerpo.<br><br>**INV-13** queda como una regla por contrato: los ayudantes privados solo en PB-T3a; en PB-T3b y PB-T3c, solo la lista. Es la forma de PBT1C-C13 |
| **N-03** | **Resuelto** | INV-07 (común) y C02 exceptúan **solo** la reactivación temporal, local y sin commit de la medición. El Worker la deshace antes del GREEN, y la `ExpectedEvidence` exige el `git diff -U0` con solo las líneas de `Skip`. Las líneas de censo salen de una corrida temporal de INV-10 (§5.3, la nota) |
| N-04 | Resuelto | Con (A′): las filas 2 a 5 de S-2, vacías. Comprobado: la sobrecarga de cuatro argumentos conserva `RackEmbedDocument source, string view` y la de cinco, `RackEmbedComposer.Compose(source,`. El censo de S-6 (`vacia`, por el fallo de S-5 sobre el mismo texto) es una medida sensata |
| N-05 | Resuelto | 76 archivos con (A) y 77 con (A′); el `AllowedWriteScope`, 11 y 10 |
| N-06 | Resuelto | La escala a 3,0 antes del segundo `Prepare` |
| N-07 | Resuelto | En `833ea6bf`:<br>• F4B-E24 y E25 están en `:684` y `:693`;<br>• F4B-E26 cierra PB-T1b;<br>• F4B-E27 (`:761`) emite PB-T1c, con `F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json`;<br>• `src` no cambia |
| V3-08 | Correcto | INV-PBT3B-15 ya no choca con el INV-PBT3B-07 común |

### 11.3 La pregunta de D-1

**Lo que se sustituye ahora, en PB-T3b: exacto.** Seis con (A) y tres con (A′), como en la §10.4.

**Lo que deja a PB-T4: no es exacto.** Las autopruebas arrastran a sus reglas:
- **E-6** empieza con `Assert.Empty(E5Violations(real))`, y **E-5** se define «sobre **las dos** `Execute`» (§5.3, `:902-903`).
  - Con (A), al quitar el piloto, cae E-5 y **cae también E-6**.
  - PB-T4 sustituye **E-3, E-5 y E-6**: tres, no dos.
- **E-2** empieza con `Assert.Empty(E1Violations(real))` (`:899`).
  - Con (A′), el paquete cuenta que **E-1** cae al quitar la sobrecarga de cuatro argumentos. Si cae, **cae también E-2**.
  - PB-T4 sustituye **S-3, S-5, S-6, E-1, E-2 y E-4**: seis, no cinco.
- **Total: nueve y nueve.** Sigue siendo simétrico, y «(A′) no ahorra» se sostiene.
- **Pero la letra de los equivalentes deja abierto el recuento.** Si E-1 (A′), E-4 (A′) y E-5 se escriben «sobre cada uno que exista» y no «sobre los dos», no caen en PB-T4:
  - con (A′) caerían solo S-3, S-5 y S-6: tres, seis en total;
  - con (A), como mucho E-3: uno o ninguno.

  Entonces (A′) **sí** ahorraría sustituciones, y cambiaría la razón por la que se recomienda (A).
- **En ninguna lectura salen «dos, cinco, ocho».**

**Las opciones:**
- **Son reales:** con (A′), el recuento de S-1, S-2 y S-4 y que S-3, S-5 y S-6 sigan en verde está comprobado (§10.4).
- **Están completas:** las mezclas (el plan genérico con la corrida duplicada, o al revés) dan cuatro o cinco ahora, y quedan dominadas.
- **«Ocho en total»** solo cuenta lo de D-1. No cuenta las sustituciones del retiro del piloto que trae A-T3-1, que son las mismas con las dos opciones (O-02).

### 11.4 Hallazgos

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **O-01** | IMPORTANTE | **Los números de PB-T4 en la pregunta y en las tablas de D-1 no son exactos.**<br>• E-6 y E-2 caen con las reglas de E-5 y E-1 que afirman al empezar.<br>• Con las reglas en sentido estricto, salen tres con (A) y seis con (A′), nueve en total.<br>• La letra de E-1 (A′), E-4 (A′) y E-5 deja la cuenta indeterminada, y con la lectura laxa (A′) ahorraría. Es lo que se pregunta al Owner | v3 §5.3, `:898-903` (E-1, E-2, E-4, E-5 y E-6); §8.2, la tabla y la pregunta; D-PBT3-09b; `v3-delta.md` | Fijar la letra: «E-5, sobre las dos `Execute`, que deben existir»; «E-1 (A′), con la sobrecarga de cuatro argumentos, que debe existir»; «E-4 (A′), con `Execution` y `FullExecution`, que deben existir». Después, recontar: (A), seis ahora y tres en PB-T4 (E-3, E-5 y E-6); (A′), tres ahora y seis en PB-T4 (S-3, S-5, S-6, E-1, E-2 y E-4); nueve en las dos. Corregir la pregunta, las dos opciones, las dos tablas y A-T3-1. Otra salida: no dar números de PB-T4 en la pregunta y remitir a A-T3-1 |
| O-02 | MENOR | «En total, en F4B: ocho» y «Ocho en total» se leen como el total de F4B, y solo cuentan lo de D-1. Las sustituciones del retiro del piloto (A-T3-1) son aparte e iguales en las dos | §8.2; D-PBT3-09b; las opciones | «En total, por D-1», y una frase: «el retiro del piloto en PB-T4 (A-T3-1) trae además las suyas, iguales en las dos» |

### 11.5 Regresiones y decisiones nuevas

**Regresiones:** ninguna.
- El diseño no cambia entre la v2 y la v3: solo cambian el texto de los contratos, CT-PBT3B-07 (ii) y la medición.
- B-01 sigue resuelto.
- La barrera del piloto sigue declarada y probada.

**Decisiones materiales nuevas:** ninguna. D-1 sigue siendo la única.

**Siguiente paso:** corregir O-01 y O-02, con una comprobación acotada solo de la §8.2, de D-PBT3-09b, de §5.3 (E-1 a E-6) y de A-T3-1.
- Con eso, el paquete queda **LISTO PARA EMITIR TRAS REMEDICIÓN**.
- PB-T3a no depende de D-1 ni de O-01.

---

## §12. Recomprobación de la v4

**Veredicto: LISTO PARA EMITIR TRAS REMEDICIÓN.** B/I/M = **0 / 0 / 1**.
- **O-01 y O-02 están resueltos.**
- **La letra estricta de E-1, E-4 y E-5 es correcta:** conserva todo lo que cubrían S-1, S-4 y S-5. Con ella, la cuenta da **seis ahora y tres en PB-T4** con (A), y **tres ahora y seis** con (A′). **Nueve por D-1** en las dos.
- **La opción elegida por el Owner, (A) (F4B-E29),** está descrita de forma coherente en todo lo de PB-T3b.
- **No hay regresiones.**
- **Queda un menor (P-01):** la v4 se escribió antes de F4B-E29. Hay que pasar la decisión al texto que se emita.

### 12.1 Método

Las mismas reglas: el worktree no se tocó. Se leyó:
- `v4-delta.md` y la v4: el registro v3 → v4, §5.3 (E-1 a E-6, «La letra, estricta», la medición), D-PBT3-09b, §6.2, §8.2 y §10;
- por objetos git, en `d8ccbcb8`:
  - `owner-pbt3-d1-decision.md`;
  - `f4b-dispositions.md:788-830` (F4B-E28 a F4B-E30);
  - IntegrationGuard:880-882;
  - `git diff --stat 833ea6bf d8ccbcb8 -- src`: una línea en W, la costura inerte del RED de PB-T1c, `e5a21989`;
- `diff` de §6.2 entre la v3 y la v4: solo cambia «v3» por «v4» en el `Objective`.

### 12.2 O-01 y O-02

| Hallazgo | Estado | Comprobación |
|---|---|---|
| **O-01** | **Resuelto** | **La letra estricta, en §5.3 (`:910-917`):**<br>• E-1 (A′) exige que la sobrecarga de cuatro argumentos exista y delegue;<br>• E-4 (A′) exige `Execution` y `FullExecution`;<br>• E-5 exige las dos `Execute`.<br><br>**Es correcta:** las pruebas que sustituyen fallan hoy si falta el miembro del piloto que nombran:<br>• S-1 busca la firma de cuatro parámetros (PrepareGuard:107);<br>• S-4, el `Post` del piloto (UpdateGuard:140);<br>• S-5 lee el cuerpo de la `Execute` del piloto. El paquete cita IntegrationGuard:881, pero el `BodyOf` está en `:882`; es una diferencia trivial.<br><br>Así, un equivalente laxo cubriría menos, y descartarlo sin preguntar al Owner es lo que pide [OD] §7.3 («Do not silently weaken»).<br><br>**La cuenta:**<br>• **(A):** en PB-T4 caen E-3 (su cláusula del piloto), E-5 (las dos `Execute`) y E-6 (`Assert.Empty(E5Violations(real))`, `:915`). Si PB-T4 conserva `BuildCore` y `Execution<TState>`, como declara la nota, E-1, E-2 y E-4 siguen en verde: **tres**.<br>• **(A′):** caen S-3, S-5 y S-6, E-1, E-2 (`Assert.Empty(E1Violations(real))`, `:911`) y E-4: **seis**.<br>• **Nueve y nueve.** La lectura laxa (siete y seis) está analizada en §8.2 y dicha en una frase de la opción (A′), sin presentarla como opción.<br><br>Coinciden en los números la cabecera (`:120-123`), D-PBT3-09b (`:637`), §8.2 (`:1331`, `:1350`), la pregunta, las opciones y A-T3-1 (`:1424`). No queda ningún «dos», «cinco» ni «ocho» vigente: solo en el histórico de V3-01, marcado (`:25`) |
| **O-02** | **Resuelto** | «En total, por D-1» en las dos tablas y «Nueve por esta decisión» en las opciones. La frase sobre A-T3-1 está en la cabecera, en D-PBT3-09b y en §8.2 |

### 12.3 La opción (A) en PB-T3b

**Coherente:**
- seis `Skip` (S-1 a S-6) en los tres archivos de guardas;
- el `AllowedWriteScope` de 11 y el `ForbiddenWriteScope` con los 76 archivos;
- CT-PBT3B-14 con seis `[Fact]` (E-1 a E-6) y `MinSelected` 6;
- los `Skipped` en + 6 (o + 10);
- la medición esperada de (A) (S-1, S-3, S-4 y S-5 `Failed`; las filas 1 y 3 de S-2, `Failed`; las filas 2, 4 y 5, vacías; S-6, `vacia=True`);
- **PBT3B-C13 con (A):** las 34 declaraciones nuevas, la retirada de `_input` y los dos cambios privados declarados. El generador lo copia por defecto; el texto de (A′) solo se copia si el Owner la elige. §6.2 no cambia desde la v3, ya comprobada (§11.2);
- **F4B-E29 la registra igual:** S-1 a S-3 por su premisa, S-4 a S-6 por el tipo o el recuento, S-6 vacía y tres equivalentes en PB-T4.

### 12.4 Hallazgo

| Id | Gravedad | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| P-01 | MENOR | **La v4 es anterior a F4B-E29,** y se nota en tres sitios:<br>• la precondición 4 (`:140`) sigue como pendiente;<br>• el motivo literal del `Skip` dice `F4B-E<n>` (`:923`);<br>• §8.2, D-PBT3-09b y §6.2 siguen ofreciendo las dos variantes, sin decir cuál se eligió.<br><br>Además, dos anclas se han movido:<br>• el RED de PB-T1c (`e5a21989`) añadió una línea a W, así que W:659-704 está ahora en `:660-705`;<br>• la ancla de S-5 es `:882`, no `:881` | `owner-pbt3-d1-decision.md`; `f4b-dispositions.md:800`; v4 `:140`, `:923`; `git diff 833ea6bf d8ccbcb8 -- src` | En la versión que se emita:<br>• la precondición 4, cumplida (F4B-E29, `d8ccbcb8`);<br>• `F4B-E29` en el motivo literal;<br>• §8.2 con la respuesta literal del Owner;<br>• el texto de (A′) (D-PBT3-09b, su C13, los «con (A′)» de §5.3 y §6.2) marcado como no elegido y fuera del contrato;<br>• las anclas de W y S-5, en la remedición (precondición 2), que ya las cubre |

### 12.5 Regresiones y decisiones

**Regresiones:** ninguna.
- La v4 solo cambia la letra de E-1, E-4 y E-5 (más estricta, sin rebajar nada), los números y las frases de A-T3-1.
- No cambia el diseño, ni ningún CT, ni C13.

**Un hecho nuevo en `d8ccbcb8`, que no afecta a PB-T3** (F4B-E30, la parada PBT1C-C07 de PB-T1c): tras ciertos gestos de la parte compuesta de la ventana, `LastComputation.Design` va una pasada por detrás.
- CT-PBT3A-01 solo compara tras `LoadExisting`, que termina con una pasada (W:439, ahora `:440`), así que no le afecta.
- Es para PB-T4, cuando el estado vuelva de la ventana al borrador.

**Decisiones materiales nuevas:** ninguna. D-1 ya está decidida (F4B-E29), y D-2 sigue siendo un registro declarado.

**Siguiente paso:** aplicar P-01 en la versión que se emita y hacer la remedición sobre el cierre de PB-T1. Con eso:
- **PB-T3a se puede emitir;**
- **PB-T3b**, tras cerrar PB-T3a, con (A);
- **PB-T3c**, tras cerrar PB-T3b.

---

## §13. Recomprobación de la v4.2

**Veredicto: LISTO PARA EMITIR TRAS REMEDICIÓN.** B/I/M = **0 / 0 / 1**.
- **Lo del paquete en la revisión de compatibilidad está corregido:** I-01 a I-04, la parte de I-05 que le toca, y M-01 a M-07.
- **Las dos elecciones del autor son correctas,** y sus líneas esperadas nuevas son deterministas.
- **La fila por defecto del clasificador es una corrección del paquete,** no un cambio de diseño para el Owner.
- **La declaración de M-05 basta.**
- **No hay regresiones** sobre lo cerrado en §10 a §12, y no hay decisiones nuevas del Owner. La autorización de emisión de PB-T3a (DU-1, precondición 10) sigue pendiente y no se cuenta.
- **Queda un menor (Q-01):** la custodia tiene que nombrar también esta §13.

### 13.1 Método

Las mismas reglas. Se leyó:
- `compat-pbt3-vs-pbt2.md`, `v42-delta.md` y la v4.2: la cabecera, las precondiciones, D-PBT3-03, D-PBT3-07, §4, §5.1 a §5.3, §5.6, §6.0 a §6.3, §8.3 y §10;
- el `diff` de la v4.1 a la v4.2 en el `Objective` de las tres subtareas, en D-PBT3-07, en C13, en C02, en INV-07, en E-1 a E-6 y en lo de `PostOriginKey` y `_erasures`;
- por objetos git, en `5ddeb3b4`:
  - `PushBackDeclaredCorrectionSupport.cs:34-123` (`FilasF`, `Poner` y `Num`);
  - `PushBackDeclaredCorrectionRoundingTests.cs:33-38`, `:77-80` y `:145-148` (CT-PBTC-01 y 02);
  - `PushBackDeclaredCorrectionDimensionStyleTests.cs:95-129` (CT-PBTC-05);
  - los `DimensionStyle` de `pb-06-seguridad`, `pb-07-combinado` y `pb-01-base`;
  - las propiedades de `RackEmbedDocument`, `RackProjectDocument` y `PushBackDesignDocument`, y las opciones de `RackEmbedStore` y `RackProjectStore`;
  - `DynamicRackSystem.cs:129`;
  - `git log`;
  - `git diff --stat d8ccbcb8 5ddeb3b4 -- src`: los dos archivos de PB-T2a y la ventana.

**Una incidencia de procedimiento, declarada:**
- **Qué pasó:** en una orden compuesta de comparación se coló por error un `python -I -`, que lee la entrada estándar, sin programa.
- **Lo que no pasó:** no leyó ningún archivo, no ejecutó código y no escribió nada.
- **Cómo se resolvió:** quedó esperando y se paró con `TaskStop`. La comparación se rehízo sin Python.
- **Es el mismo modo que R-12 y que la medición H-12 de PB-T1,** así que se devuelve al Coordinator como modo repetido, sin clasificarlo por analogía.

### 13.2 Los hallazgos de la revisión de compatibilidad

| Hallazgo | Estado | Dónde, en la v4.2 |
|---|---|---|
| **I-01** La base, atada a PB-T1c | **Corregido** | La precondición 1 (`:162-166`): PB-T1 cerrada (F4B-E31) y PB-T2 delante, con la `BaseSha` en el cierre de PB-T2c. §6.0 (`:1116-1118`): el diff de `src`, con las referencias de I-01. La CI (`:1257`): prevista en 19 094 y 2 450, y el GREEN en 19 107 y 2 451. En cadena: `:1325` (19 128) y `:1361` (19 133). Las cuentas cuadran |
| **I-02** A2-11 | **Corregido** | CT-PBT3B-01 (ii) (`:962`); la razón (`:978-982`); §5.6 (`:1079`) |
| **I-03** A2-12 (M09) | **Corregido** | La fila por defecto (`:587-589`); CT-PBT3A-09 (3) (`:911`); la razón (`:938-941`); §5.6 (`:1080`) |
| **I-04** El tipo del borrador de PB-T4 | **Corregido** | D-PBT3-03 (`:349`); §8.3 (`:1496`); A-T3-11 (`:1539`), con el `Origin` del estado actual, el lanzamiento, el destino de `PushBackDraftSessionPort` y `LaunchFor`, y A2-4 sustituido |
| **I-05,** la parte del paquete | **Corregido** | La precondición 9 (`:183-190`), la custodia; la precondición 10 (`:191`), la autorización pendiente. Lo del generador no es del paquete y no se revisa aquí |
| M-01 Las clases de PB-T2, fuera del `ForbiddenWriteScope` | **Corregido** | §6.0 (`:1137-1138`): las nueve, nombradas, y su recuento previsto en una nota |
| M-02 La red sin PB-T2 | **Corregido** | §4 (`:859`), §5.1 (`:891`), §6.1 (`:1231-1232`), con los mínimos 3, 1, 1 y 1 en Core y 3, 4, 1 y 3 en UI. La contención de F4B-E30 (`:1119`) |
| M-03 A-T3-9 | **Corregido** | D-PBT3-03 (`:348`) y A-T3-9 (`:1537`): solo PB-T4 |
| M-04 El orden y el estado | **Corregido** | La cabecera (`:3-12`), la precondición 7 (`:181`) y R-T3-09 (`:1375`) |
| M-05 El nombre nulo pasa a `""` | **Declarado** (§13.4) | §8.3 (`:1498`) y A-T3-3 (`:1532`) |
| M-06 Las anclas de W | **Corregido** | Por su contenido: la precondición 2 (`:167-170`), la nota de D-PBT3-03 (`:338`, con su historia de líneas) y la remedición (`:1113`) |
| M-07 A2-7, sin objeto | **Corregido** | §8.3 (`:1497`); F4B-E28 en §1 (`:214`) |

### 13.3 Las dos elecciones del autor

**I-02, CT-PBT3B-01 (ii): correcta.**
- **Es el sitio adecuado:** el sobre que sale solo existe en el plan. CT-PBT3A-06 compararía `ForWrite` con `Pass`, que llevan el mismo valor, y no probaría nada sobre lo que se escribe.
- **Las filas se pueden copiar a Core:**
  - `FilasF`, `Poner` y `Num` (`PushBackDeclaredCorrectionSupport.cs:49-62`, `:111-123`) son JSON puro, sin ventana;
  - los valores son la base más 0,00044 (`:36`, `:45`);
  - F4 da `6` porque la profundidad del larguero se resuelve contra el catálogo en Application (`DynamicLoadBeamGeometry.ResolveBeamDepth`, la clase `CATALOGO` de CT-PBTC-01, `RoundingTests.cs:37-38` y `:80`), no por un control de la ventana. Así que el camino del asiento, sin ventana, da lo mismo que la ventana en CT-PBTC-02 (`:145-148`).
- **Las filas de estilo:**
  - los dos fixtures traen STANDARD-ISO y SIMPLEX, y `pb-01-base` trae `null`;
  - `sistema` es `Structure.DimensionStyle` del sistema de `BuildForMember` (`DynamicRackSystem.cs:129`), como hace CT-PBTC-05 con `SystemToInsert`.
- **Determinista, y no depende del momento de lectura:**
  - **lo esperado es una lista literal**, y nunca se lee del estado (la lección de F4B-E33);
  - **se lee en un punto fijo:** el diseño del `EnvelopeText` de cada miembro después de `Prepare`, que es puro, y el sistema de `BuildForMember` sobre ese mismo miembro;
  - **no hay gestos de ventana ni `LastComputation`:** la composición arma el diseño del estado actual (la lección de F4B-E30);
  - el estado parte de `LoadExisting`, que termina en una pasada, y solo cambia el nombre;
  - **los valores de los miembros van sin repetir y en ordinal.**
- **Si un valor no coincide** (por ejemplo, F4), es C07: el Worker para y no ajusta lo esperado.

**I-03, CT-PBT3A-09 (3) más la fila por defecto: correcta.**
- **La prueba por reflexión:**
  - salen 27 propiedades: 8, 9 y 10, sin la de `[JsonExtensionData]`, comprobadas en `5ddeb3b4`;
  - ninguna lleva `JsonPropertyName`, y los dos almacenes no tienen política de nombres, así que su nombre JSON es el de la propiedad;
  - las 27 tienen su fila exacta en PESO, con los dueños que dice la lista;
  - el orden es ordinal, así que el resultado es determinista y no depende de cuándo se lea;
  - va dentro de CT-09, que tiene `ExpectRed`. En el RED falla igual por las partes (1) y (2), y su parte (3) solo usa la API pública de PB-T1b: no toca la API cerrada ni los recuentos.
- **La fila por defecto: es una corrección del paquete, no un cambio de diseño que tenga que ir al Owner.**
  - Hoy no se alcanza: las diez propiedades de la carga útil ya tienen prefijo en la tabla, y AUTH-13 rechaza lo desconocido antes de clasificar.
  - Solo hace total una función interna del clasificador de PB-T3, que todavía no está conectado. Sin ella, una propiedad futura caería sin aviso en `Name`, que es el valor 0 del enumerado.
  - Decide por el mapa de propiedad, que ya es la autoridad de OB-05.1 y que ata la parte (3).
  - No cambia ninguna línea esperada de hoy, no añade ningún miembro y no cambia nada que se vea: su presentación es de PB-T4.
  - El oráculo mecánico de CT-09 y 10 copia las reglas de la tabla del paquete, así que la incluye solo.
  - La alternativa (la prueba sola, más la limitación declarada) también sería válida, pero deja la función parcial. La fila es mejor.

### 13.4 M-05: el nombre nulo que pasa a `""`

**Basta con declararlo.**
- Es un falso positivo del clasificador, que todavía no está conectado: informa `$.Name` como `ChangedByUser` de `null` a `""`, mientras que el escritor, que es lo que escribe, no renombra.
- Su presentación es de PB-T4 (A-T3-3, `:1532`, que ya dice «lo presenta como el escritor, o lo filtra»).
- Alinearlo ahora cambiaría el diseño de D-PBT3-07 y el oráculo de CT-09.

**Una nota, no un hallazgo:** el mismo `""` también hace que el borrador salga en Dirty (el comparador mira `Name`) tras un «Aplicar» que el usuario no ve como cambio. Eso es de PB-T2 (L-PBT2-12) y de PB-T4, no de PB-T3.

### 13.5 Lo nuevo, frente a §10 a §12

**Sin regresiones:**
- **El `Objective` de las tres subtareas** solo cambia la versión.
- **No cambian de la v4.1 a la v4.2:**
  - C13 de PB-T3b y PB-T3c, con la lista de (A);
  - INV-07 común y C02, con la reactivación de la medición;
  - E-1 a E-6, con la letra estricta;
  - INV-PBT3B-15;
  - `PostOriginKey`, D-PBT3-09 (h) y los `_erasures` de PB-T3c.
- **D-PBT3-07** solo gana la fila por defecto.
- **CT-PBT3B-01** gana la parte (ii), sin cambiar su `MinSelected` (13).
- **CT-PBT3A-09** gana la parte (3), sin cambiar el número de métodos.
- **Lo de (A′)** sigue como registro y fuera del contrato.

### 13.6 Hallazgo

| Id | Gravedad | Contenido | Corrección |
|---|---|---|---|
| Q-01 | MENOR | La precondición 9 (`:186`) custodia `independent-review-v1.md` «con sus §1 a §12», y el punto 4 del generador en `v42-delta.md` añade `'## §12. '` a `CUSTODY_HEADS`. Ahora la revisión tiene su §13, sobre la v4.2 | «Con sus §1 a §13 (la §13, sobre la v4.2: LISTO PARA EMITIR TRAS REMEDICIÓN, 0/0/1)», y `'## §13. '` en `CUSTODY_HEADS` |

### 13.7 Decisiones para el Owner

**Ninguna nueva.**
- La fila por defecto y M-05 son del paquete.
- I-04 es técnico.
- **Sigue pendiente, fuera de los hallazgos,** la autorización de emisión de PB-T3a (DU-1, precondición 10).
