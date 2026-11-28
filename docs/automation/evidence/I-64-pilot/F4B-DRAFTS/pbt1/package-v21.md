> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<carpeta-del-paquete>`, `<datos-locales-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T1 (el estado editable de Push Back sin pérdidas fuera de la ventana), v2.1

**Estado:** v2.1, el texto de emisión. No declara GATE PASS.
- **La revisión independiente** IRT1-1 de la v1 (`F4B-DRAFTS/pbt1/independent-review-v1.md`) dio **EMITIBLE TRAS CORRECCIONES 0/8/15**, con **una decisión nueva del Owner, L-1**.
- **La comprobación acotada de la v2** (`F4B-DRAFTS/pbt1/independent-review-v2-check.md`) dio **EMITIBLE TRAS CORRECCIONES 0/1/10**: el importante N-I-01 lo corrigió la sesión, y esta v2.1 aplica los diez menores sin otra revisión.
- **L-1 sigue abierta hasta la respuesta del Owner.** Hasta entonces no se emite ninguna subtarea.

**Lo que pide el Owner** ([OD], fila de PB-T1 del §5 de [P]):

> ACCEPT IN PRINCIPLE. PB-T1 extracts a lossless Push Back editable state outside the WPF window. Hard requirements: module state preserved; parked queue preserved; A/B preserved; no aliasing; no WPF control becomes source of truth; state round-trip lossless for supported classic racks; classic RACKEDITAR still works through the extracted state. Do not rewrite the entire editor merely to obtain an MVVM architecture. The goal is state separation, not aesthetic refactoring.

**Lo que propone este paquete:**
- **Tres subtareas secuenciales**, cada una con su cadena y su presupuesto de 1/3 (OB-10.1):
  - **PB-T1a — la red** (`F4B-PBT1A-LOSSLESS-STATE-NET`): solo pruebas de caracterización, con RED por mutantes. El producto no cambia.
  - **PB-T1b — el tipo** (`F4B-PBT1B-LOSSLESS-STATE-TYPE`): el estado editable en Application. La ventana no cambia.
  - **PB-T1c — la ventana sobre el estado** (`F4B-PBT1C-LOSSLESS-STATE-WINDOW`): la ventana guarda su estado en ese tipo, en modo CLASSIC y sin cambio observable.
- **Ninguna corrección de producto.** RACKEDITAR no cambia (OB-RE.1). c-02 se reproduce como regla explícita (F4B-E09, pregunta 2).
- **El diseño vigente para L-1 es la opción (a),** la recomendada: una excepción declarada a «no aliasing», limitada a los dos derivados del último recálculo. El anexo L1-C trae el cambio exacto si el Owner elige (c).

**Condiciones para emitir** (§8.4): la respuesta del Owner a L-1, con la pregunta corregida por N-I-01; y el cierre de PB-TC2, hoy parada en su vuelta C14. La comprobación acotada de la v2 ya está hecha.

**Base de la medición:** rama `architecture/workspace-persistente-rackcad` en HEAD `eedb069c`, con `main` en `812baf87`.
- El producto de HEAD es el GREEN de PB-TC, `4440f67d` (medición A-4).
- **PB-T1 depende del cierre de PB-TC2** (F4B-E09). La `BaseSha` de PB-T1a es su commit de cierre.
- Las zonas que PB-TC2 toca se marcan **[TC2]**: «a volver a medir sobre el GREEN de PB-TC2» (`Gap_Changed`, `ModuleLength_Changed`, `ReadAdvancedRackParameters`, `ReadCellValues`, `OptionalVal`, `Val` y el desplazamiento de las líneas de W posteriores a W:3634; medición L).
- Todo hecho va anclado por **nombre de miembro y línea**. El detalle y las órdenes están en `pbt1-measurements.md` (apartados A a L de la v1 y **M** de la v2).

**Precondiciones de emisión** (de cada subtarea, salvo que se diga otra cosa):
1. **PB-TC2 cerrada** con el diseño de su paquete v2. Si el Owner elige en su vuelta C14 la opción (B), se aplica la nota del §8.5. Si PB-TC2 no cierra, este paquete se vuelve a redactar.
2. **La respuesta del Owner a L-1** (§8.1). Con (a), este texto. Con (c), este texto más el anexo L1-C. Con (b), PB-T1b espera a la tarea de Domain y su paquete se rehace.
3. **La comprobación acotada de la v2,** hecha: 0/1/10, sin ninguna decisión material nueva distinta de L-1. N-I-01 corregido por la sesión y N-M-01 a N-M-10 aplicados en esta v2.1, sin otra revisión (§8.3).
4. **Los anclajes de la subtarea**, medidos sobre su `BaseSha` (§6.0).
5. **Para PB-T1b: la autoridad de PB-T1a** (I-08). Ninguna línea fijada por PB-T1a muestra una pérdida (clase c, d o una nueva) ni un control que decida el estado, distinta de las declaradas (c-01, c-02 y M-07). Si alguna lo muestra, se vuelve al Owner antes de emitir PB-T1b (OB-06.5).
6. Las celdas del §6.0 no están `STALE` (vencen el 2026-12-29); el `config.toml` coincide con la Snapshot I; el binario de Codex es el acreditado.
7. **La custodia** (§6.0, «Custodia»).

## Registro de cambios v2 → v2.1

| Hallazgo de la comprobación acotada | Qué cambió | Dónde |
|---|---|---|
| **N-I-01** (IMPORTANTE; la pregunta de L-1 no era fiel ni sin sesgo) | **Lo corrigió la sesión** antes de esta v2.1: la pregunta de `owner-return-l1.md` (la línea base guarda las personalizaciones confirmadas; hoy solo se le cambia el nombre al dibujar; el riesgo residual de (a); las anulaciones en memoria de (b); la vuelta de (c) si fallan los A/B) y la línea de la opción (b) del §8.1, que ya trae la frase de `FrameExceptionOverride`. Esta v2.1 no la vuelve a tocar | `owner-return-l1.md`; §8.1 (b) |
| **N-M-01** | El tercer escritor de `Name`, el lote de vistas del menú (`RackMenuCommands.cs:135`), en HT1-14, en «Lo medido» del §8.1 y en A-2; y en la medición M-1 del registro | HT1-14; §8.1; §10 A-2; `pbt1-measurements.md` M-1 |
| **N-M-02** | La regla de la sesión del comparador: «sin cambios» es nula, o sin pendientes **y** con su lista confirmada equivalente a `Begin(<línea base>.Structure).Commit()` | D-PBT1-06; CT-PBT1B-10 |
| **N-M-03** | La rama de la fila (b) del paso 1 queda fijada: si da `SelectedId=P`, la regla compara sin mayúsculas y devuelve el id de la opción, como `PostBox`; con sus líneas esperadas en CT-PBT1B-03. El §8.3 se alinea | D-PBT1-09; INV-PBT1B-06; PBT1B-C14; CT-PBT1B-03; §8.3 |
| **N-M-04** | Cómo se producen en la ventana los cuatro bloqueantes de CT-PBT1B-01: el hueco negativo con la costura `CompositeState.SetGap(-1)` y un `LostFocus` en `AnnotationScaleBox`; la estructura de 1 y el separador sin hueco por documento (`EditInner`); la celda que no cabe con el diseño de `Blocking()` | CT-PBT1B-01 |
| **N-M-05** | La parte 2 de CT-PBT1B-07 recorre también las claves y los valores de todo `IDictionary` | CT-PBT1B-07 |
| **N-M-06** | El `RackFrameProjectStore` del estado y del commit es **local al método**, nunca un campo. De las dos que admite la comprobación («local o `static readonly`») se fija una, para no dejar latitud; INV-PBT1B-04 pasa a hablar de campos de instancia y prohíbe también los estáticos nuevos | D-PBT1-03; D-PBT1-07; INV-PBT1B-04 |
| **N-M-07** | PBT1A-C15 sale de `StopConditions` y pasa a ser el invariante INV-PBT1A-13 | §6.1 |
| **N-M-08** | La línea de F-03 lleva `pasadas-restaurar=<n>` y `frontal=<lado>`, con 2 y A esperados; otra cosa es PBT1A-C07 | §5.0; CT-PBT1A-14 |
| **N-M-09** | En el anexo L1-C, «INV-PBT1B-07 nuevo» pasa a INV-PBT1B-07b, y se dice que toda llamada a `Copy` lleva el resolver del mismo catálogo | Anexo L1-C |
| **N-M-10** (resto de M-13) | La definición de H-PB-44 se cita en [PKC] `:187` y `:1343` | §2.4 |

## Registro de cambios v1 → v2

| Hallazgo | Qué cambió | Dónde |
|---|---|---|
| **I-01** (L-1 con premisas inexactas) | L-1 se reescribe con las tres opciones del §4 de la revisión y con **todos** los consumidores del objeto compartido (M-1): solo el Plugin lo muta, tras cerrar la ventana. HT1-12 se corrige: la cola que aparca `ParkTailOf` apunta a la línea base y al commit (M-2). CT-PBT1B-07 comprueba **aparte, sin la exención**, que la cola, el commit y la sesión de la copia no comparten objetos con el original. CT-PBT1B-08 se extiende a los consumidores medidos. Arrastre A-2 con la obligación de no mutar | HT1-12, HT1-14; D-PBT1-05; CT-PBT1B-07 y 08; §8.1; §10 A-2; anexo L1-C |
| **I-02** (CT-07 inalcanzable) | Los objetos inmutables se excluyen con el criterio `IsImmutable` del censo del piloto, copiado; CT-07 nombra sus diseños | CT-PBT1B-07 |
| **I-03** (la paridad sin definir) | El ayudante de prueba `Pasada`, copia literal de W:659-704 adaptada al estado (no hay API de asiento), y el número de pasadas de cada camino. En el comparador, una sesión nula equivale a una sesión sin cambios pendientes | §5.0; D-PBT1-06; CT-PBT1B-01; CT-PBT1C-01 |
| **I-04** (el orden de la carga cambiaba) | Los portadores se adoptan donde hoy, dentro de `LoadFromModel` y bajo `suppressSync`: la carga del estado se parte en la de Application (`…Editor`) y `AdoptCarriers`, pública. CT-PBT1A-14 gana el guion F-03 | D-PBT1-03; D-PBT1-13 (3) y (4); CT-PBT1A-14; R-T1-13 |
| **I-05** (la copia de la cola no era literal) | La cola se copia campo a campo y literal, con `RackFrameProjectStore.DeepCopy`, como DA:557-566. Lo mismo en `Copy` de la sesión y del commit | D-PBT1-03; D-PBT1-05; D-PBT1-07 |
| **I-06** (faltaba C04) | PBT1A-C04, PBT1B-C04 y PBT1C-C04, cada una con el texto de su régimen | §6.1 a §6.3 |
| **I-07** (la API del mapa) | Las firmas exactas, el enum, la forma y la normalización de las rutas, y la fila `$.Design` del sobre | D-PBT1-08 |
| **I-08** (sin parada de autoridad entre subtareas) | La precondición 5 y la declaración obligatoria PBT1A-C15 | Precondiciones; §6.1 |
| **M-01** | El oráculo de los canónicos es CT-PBTC-06 más los del piloto; la garantía de los heredados se declara «salvo la canonicalización T2H-07» | CT-PBT1B-02; §8.2 |
| **M-02** | Las recetas de X-01 (copia literal de `BuildX01`), F-01, F-02 y F-03, en la ventana y en Core | §5.0 |
| **M-03** | CT-PBT1A-11 repite la preparación de `AbrirFueraDePantalla` con `Ventana()` y la carga en el `try`. M-01 de PB-T0 se declara **no corregido** (CT-PB0-17 sigue con `Abrir`) | CT-PBT1A-11; D-PBT1-12 |
| **M-04** | CT-PBT1A-07 gana la ruta de los portadores del rack | CT-PBT1A-07 |
| **M-05** | La regla de c-02 compara con los ids de `ToOptions` (recortados, sin duplicados sin mayúsculas). Las filas (a) a (e) del paso 1, literales | D-PBT1-09; INV-PBT1B-06 |
| **M-06** | INV-PBT1B-05 se limita a `src/`. La «Fuente» del mapa, con archivo y miembro, sin línea | INV-PBT1B-05; D-PBT1-08 |
| **M-07** | CT-PBT1B-12 con líneas literales; los cuatro casos bloqueantes también en la paridad con la ventana (CT-PBT1B-01) | CT-PBT1B-01 y 12 |
| **M-08** | La tabla del censo de CT-PBT1B-06 y la definición de «transitorio del armado» | CT-PBT1B-06 |
| **M-09** | `RackModuleCommit.Copy` y `SameAs`, literales, con ayudantes privados propios; las firmas de `Copy` e `Instance` | D-PBT1-05; D-PBT1-07 |
| **M-10** | `NamespaceFolderGuardTests` (7) y `HeaderBatchContractTests` (17) en los `RequiredTests` de PB-T1b; también `RackModuleEditSessionTests` (25) | §6.2 |
| **M-11** | El régimen de corrección del RED por mutantes (§16.8); la lectura de `Ci` con Core en `success` y sus precedentes; la regla «EXCEPCION» en PB-T1c | §6.0; §6.1; §6.3 |
| **M-12** | La comprobación acotada definida; la custodia de los contratos de PB-T1b y PB-T1c; el modelo paralelo interino (tres copias de la carga) y la salida si PB-T1c no cierra | §6.0; R-T1-17; R-T1-18 |
| **M-13** | Anclas y citas corregidas (M-10 del registro); `DDA` definido; la celda del Controller con `model-catalog.md:51` y `:97`; H-PB-44 citado con su fuente; `PushBackRichPreviewTests.cs:348-367` en el §4; el orden de los guiones de CT-14 | §2.0; §2.4; §4; §6.0; CT-PBT1A-14 |
| **M-14** | Los campos que la ventana solo transporta son `Editor` por D-PB-05, justificado | D-PBT1-08 |
| **M-15** | El comparador compara siempre el commit **y** la intención de la línea base | D-PBT1-06 |

**Ningún menor se deja para la emisión.**

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | La fila de PB-T1 (literal arriba). D-PB-03, D-PB-05 («FULL EDITABLE DESIGN wins for every field owned by the full Push Back editor»), D-PB-06 («Do not hide these corrections inside PB-T1 state extraction»), D-PB-10 (1/3; el intento 3 vuelve al Owner), §7.3 (T1B) y §7.4 (RACKEDITAR) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | OB-01.1b, OB-01.5, OB-03.1, OB-05.1, OB-06.5, OB-10.1 («y sus subtareas si se dividen», `:470`), OB-T1B.1 a OB-T1B.3, OB-RE.1, H-PB-27, R-PB-01, R-PB-02, R-PB-05 y R-PB-09; la fila de PB-T1 del §5 (`:503`) y la cadena de cada tarea (`:515`) |
| [OPT1] (`F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`, F4B-E09) | Pregunta 2: c-02 «Como hoy, como regla explícita». Pregunta 3: `pb-21-cola` «Dejarlo fuera de la garantía». Los demás casos de T-2, tal cual. H-31 y OWNER-1 en PB-TC2. Pregunta 4: la autorización condicional de emisión |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | Pregunta 1: los sustitutos de nc1 y nc2 solo con la firma EXACTA. Pregunta 2: una replanificación por la comparación literal |
| IRT1-1 (`F4B-DRAFTS/pbt1/independent-review-v1.md`) | Los 23 hallazgos de la v1 y las tres opciones de L-1 (su §4) |
| La vuelta C14 de PB-TC2 (`F4B-DRAFTS/pbtc2/owner-return-c14.md`) | La opción (B), que dejaría fuera de PB-TC2 la altura manual de cabecera (§8.5) |
| El expediente (`F4B-DRAFTS/pbt1/ob065-dossier.md`) | §3: los huecos de red. §4: las guardas de T1B y de texto |
| PB-T0: su paquete, su cierre y su entrega (`F4B-PBT0-CHARACTERIZATION/…`) | Los 21 CT-PB0; T-1, T-2 y T-5; M-01, M-04, D-3, D-5 y CT-16; el RED por mutantes |
| PB-TC ([PKC], v2.2) y su cierre | El patrón; M-02 a M-05; el precedente del RED con Core en `success` ([PKC]:466) |
| PB-TC2 ([PKC2], v2) | El patrón más reciente; M-03 y M-07; las firmas literales de PBTC2-C11; la celda del Controller ([PKC2]:655) |
| T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`) | 55 casos en UI y 10 en Core |
| Los contratos de F4 con API nueva (`I-64-f4-t2-gate-contract-package.md:282`, `I-64-f4-t3-gate-contract-package.md` INV-F4T3A-13, `I-64-f4-t3c-gate-contract-package.md`) | El RED con esqueleto inerte que compila y no lanza |
| ADR-0029 D12 y D13; Proposal V5 D-10, D-15 (punto 3) y D-16 | Sin modelo paralelo; la caracterización no se edita para que pase; ida y vuelta sin la ventana; sin cambio observable |
| `docs/AUTOMATION_PLAN.md` §16.5, §16.8, §16.9, §16.10 y §16.11; `docs/automation/agent-execution/README.md` §4 y §10; `routing.md`; `model-catalog.md` | A1-A8, el RED de la cadena, las 14 comprobaciones, los términos, las paradas, nc1 a nc4 y las celdas |

## 2. Hechos medidos en `eedb069c`

### 2.0 Alias

- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3690 líneas); **WC** = `….Composite.cs` (936); **WX** = `….xaml` (793).
- **CS** = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`; **ES** = `…/PushBackEditorState.cs`; **EL** = `…/PushBackEditorState.Load.cs`; **DA** = `…/PushBackEditorDesignAssembler.cs`; **CA** = `…/PushBackCompositeEditorAssembler.cs`; **PCS** = `…/PushBackCompositeStructure.cs`.
- **DDA** = `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs`.
- **PC** = `…/PushBack/Pilot/PushBackPilotStateComparer.cs`; **PM** = `…/PushBack/Pilot/PushBackPilotMaterializer.cs`.
- **RMES** = `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs`; **RFPS** = `src/RackCad.Application/Persistence/RackFrameProjectStore.cs`.
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs`; **SC1** = `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`.

### 2.1 Hoy: qué estado editable vive en la ventana y qué ya vive en Application

- **HT1-01. Ya en Application:** `composite` (W:56-57), un `PushBackCompositeEditorState` con los dos `PushBackEditorState`. Lleva los dos lados, la intención compuesta, la presencia de B y `sideBEverDeclared`, la cola aparcada (CS:58, CS:61) y su marca de consumo (CS:147), la selección (CS:167, CS:179 y la de cada matriz) y, por lado, la **autoridad de módulos**: `workingBaseline` (ES:65), `moduleSession`, `moduleCommit` y `moduleSignature` (ES:110-112) y `LastModuleReconciliation` (ES:143). Los módulos son del lado A (`moduleOwner`, W:82).
- **HT1-02. En campos de la ventana** (medición B.1): `generalPallet` (W:101), `rackTolerance`, `rackPostPeralte`, `rackBeamDepth` y `rackAnnotationScale` (W:106-109), `advanced` (W:114), `firstLevelDatum` (W:380), `loadedDimensionViews` y `dimensionViewsTouched` (W:1249, W:1260), `loadedDimensionStyle` (W:1256) y `safetySelections` (W:84).
- **HT1-03. Solo en un control, releído en cada pasada por `ReadInputs`** (medición B.3): `WeightUnitBox` (W:852), `PalletsDeepBox` (W:853), `PostBox` (W:868), las tres casillas de anotación (W:874-876), `DimensionsBox` (W:878) y las tres casillas de cotas (`SelectedDimensionViews`, W:1291-1297, desde W:880).
- **HT1-04. Contexto de la sesión clásica, no editable:** `sourceProject` (W:91), `isEditingExisting` (W:87), `frontalSide` (W:3261) y el origen de cabecera pendiente (W:2038-2039).
- **HT1-05. La identidad y el nombre** viven en `session.Identity` (`RackEditorIdentity`, UI): `Adopt` en W:279, :305, :322; `SetName` en `Name_Changed` (W:1234-1238) y en W:3352, :3405, :3504; `EnsureId` en W:3503.
- **HT1-06. Derivados o de interfaz** (medición B.2): `lastComputation`, las banderas de validez, `outputIsBlocked`, `lastCompositeSystem`, el estado mixto (WC:416, WC:429) y las cachés.

### 2.2 El flujo: quién siembra, quién lee y quién escribe

- **HT1-07.** Las tres cargas y «Restaurar» (W:270-289, W:294-307, W:311-324, W:3307-3330) hacen la parte de Application y después `LoadFromModel` (W:385-440), que **siembra a la vez los controles y los portadores, bajo `suppressSync`** (W:387-437), y recalcula (W:439). En `LoadNew` hay dos pasadas, con la seguridad por defecto entre ellas (W:280, W:286-288).
- **HT1-08. La parte de Application de la carga vive en la ventana:** `SideAOnly` (W:342-373) y `LoadCompositeFromDesign` (WC:813-814, WC:827-934), toda de Application salvo `UpdateCompositeEnabled()` (WC:933). El piloto tiene su propia copia (PM:150-172).
- **HT1-09. Cada pasada** (`Recompute`, W:616-723) escribe la celda primaria desde los controles (W:635 → `ReadCellValues` W:802-831 **[TC2]**), actualiza los portadores con `Keep` (W:838-844), construye unos inputs **nuevos** (W:846-889), arma y resuelve (W:659-660) y adopta la computación (W:674-704). La adopción abre la sesión de módulos (`RefreshModuleSelector`, W:704 → W:1688 → ES:124-133; medición M-6).
- **HT1-10. El ensamblador muta lo que recibe:** el estado del lado A (DA:155-159) y los inputs de la pasada (DA:302).
- **HT1-10b. La carga puede reentrar** (medición M-7): `UpdateCompositeEnabled()` (dentro de `LoadCompositeFromDesign`, WC:933) puede poner `FrontalSideBox` a 0 (WC:117-121), y eso lanza una pasada síncrona (W:3277-3289; `RecomputeGate.cs:35-44`) **antes** de `LoadFromModel`. Esa pasada lee controles todavía sin sembrar; hoy `LoadFromModel` reasigna después los portadores (W:391-427).

### 2.3 Los alias con el modelo cargado y dentro del estado (mediciones D, M-1 y M-2)

- **HT1-11.** `SideAOnly` y el diseño del lado B de `LoadCompositeFromDesign` comparten objetos con el diseño del llamador, solo de forma transitoria: la carga resuelve y el resolver copia (D-1, D-2).
- **HT1-12 (corregido). La cola aparcada guarda referencias, por dos caminos:**
  - **al cargar,** los mismos `DynamicRackModuleDesign` y `DynamicHeaderLineOverride` del diseño cargado (WC:863-867 → CS:85-95; D-3);
  - **al dormir,** `ParkTailOf` (CA:180-242) aparca intenciones cuyo `HeaderConfiguration` es el `RackFrameConfiguration` **del módulo de la línea base** (`ToTailDesign`, PCS:777 → PCS:800-811), las **mismas** líneas de la línea base (CA:188-190) y las **mismas** intenciones y líneas del commit (CA:201-239) (M-2).

  Por lectura nadie las muta: el despertar copia campo a campo (DA:557-566).
- **HT1-13.** `sourceProject` es el proyecto del llamador, por diseño (D-4).
- **HT1-14 (ampliado). La línea base resuelta es el mismo objeto que `lastComputation.System`, `session.System` y `lastCompositeSystem`** (W:674-711). La medición M-1 lista **todos** sus consumidores: el armado, la sesión, `ParkTailOf`, el panel compuesto, el criterio de bloqueo, los módulos del panel, la vista previa, el lote de vistas y las peticiones de inserción. **Todos leen salvo el Plugin,** que escribe `Name` **después de cerrar la ventana**, cuando su estado ya se descarta, en tres sitios: `RackPushBackCommands.cs:98` y `:190`, y el lote de vistas del menú, `RackMenuCommands.cs:135` (`pushBack.System.Name = pushBackName;`). `ProjectVariableMutationExecutor.cs:169` resuelve su propio sistema (Selectivo) y no cuenta.

### 2.4 Los controles fuente de verdad que quedan tras PB-TC y PB-TC2 (medición E)

**La definición** es la que usa el paquete de PB-TC para H-PB-44, formulada en su H-31 ([PKC] `:187`) y en su §10 ([PKC] `:1343`); la fila de H-PB-44 (`:355`) solo la nombra: un control que se relee sin que el usuario cambie su valor y, al releerse, cambia el estado o la salida. [OPD] pregunta 4 decidió el alcance de PB-TC (OD-PBTC-5), no esta definición.

- **HT1-15.** Con el diseño de PB-TC2 v2, **solo `PostBox` decide el estado con el catálogo entregado**: c-02 (un id fuera del catálogo → nulo → `Defaults.Post`; W:399, W:868, DA:169-171) y c-01 (en blanco → `Defaults.Post`; W:399). Con la opción (B) de C14 se añadiría `RackHeaderHeightBox` (§8.5).
- **HT1-16.** Los demás controles de HT1-03 no pierden nada, pero hoy su valor no tiene portador fuera del control. PB-T1 se lo da.
- **HT1-17.** `RearPeralteBox` y los combos de peralte de la celda solo sustituyen con un catálogo distinto del entregado o con un documento que la carga ya normaliza (E-3, E-4). Sin caso alcanzable (OB-06.5).
- **HT1-18. M-07 de PB-TC2:** el indicador mixto obsoleto hace inerte una caja vacía. No cambia el estado (E-7).

### 2.5 El criterio de bloqueo de salida (medición F)

- **HT1-19.** La línea de la fase (`RackPushBackSystemWindow.Composite.cs:778-782` en `5ccaa770`) es `CompositeHasBlocking`; hoy **WC:803-807**, con `CompositeDiagnostics` (WC:783-786) y `CompositeStatusOr` (WC:789-801). Es «los diagnósticos de la intención (CS:629-631) más los del sistema resuelto (`PushBackCompositeDiagnostics.Evaluate`)». **No es `RackBomOutputGate.For`** (`RackBomOutputGate.cs:49-59`).

### 2.6 El copiador y el comparador del piloto (medición G)

- **HT1-20.** `PushBackPilotStateComparer.Copy` (PC:58-111) no copia la autoridad de módulos ni la cola, y el comparador no las lee (PC:53-56, PC:134-177). `KeepPilotEditsOn` (PM:242-251) nace sin ellas y `ComposeForWrite` (PM:196-234) repone desde la base (H-PB-27).

### 2.7 Guardas que atan el diseño y que el expediente no lista (mediciones H y M-10)

- **HT1-21. El censo del piloto** (`tests/RackCad.Tests/Workspace/WorkspaceDraftPilotStateTests.cs:204-537`, `:955-982`): **un campo nuevo** en `PushBackCompositeEditorState`, `PushBackEditorState`, `PushBackEditorInputs`, `PalletSpecification`, `DynamicAnnotationOptions` o los tipos de seguridad lo rompe. Los métodos nuevos no.
- **HT1-22.** `NamespaceFolderGuardTests` exige un solo `namespace` de bloque por archivo de `src/`, igual a su carpeta (medición H-11).
- **HT1-23.** `PushBackRichPreviewTests.cs:348-367` recorre por reflexión los métodos y los tipos de campo no públicos de la ventana (M-10).
- **HT1-24.** Ninguna prueba nombra los campos que PB-T1c convierte en reenvíos; solo los literales de T-SG nombran `lastComputation`, `sourceProject`, `moduleOwner`, `session.Identity` y `NameBox.Text` (medición H).

### 2.8 Lo que PB-TC2 cambia y obliga a volver a medir [TC2]

- WC:198 (`Gap_Changed`), W:1905 (`ModuleLength_Changed`), W:1639, :1641, :1642 y :1646 (`ReadAdvancedRackParameters`), W:818-822 (`ReadCellValues`), W:3667 (`OptionalVal`), la retirada de `Val` (W:3634-3635) y tres miembros nuevos tras `Keep` (W:3686).
- Las líneas de W anteriores a W:3634 no se mueven; las posteriores se mueven −2 hasta `Keep`, e `IntVal` y lo que le sigue, además, por los miembros nuevos.
- PB-T1 usa de esas zonas los mutantes MU-T1A-03, 04, 07a y 10 y la rama vacía de `OptionalVal`.

## 3. El diseño

### D-PBT1-01. Tres subtareas secuenciales (R-PB-01, R-PB-09, OB-10.1)

**Por qué dividir:** tres regímenes de RED distintos (mutantes, esqueleto inerte y costura inerte); la red tiene que estar en la CI y con su discriminación probada **antes** de mover el estado; el tipo de Application se prueba entero contra la ventana de hoy **antes** de que la ventana dependa de él (D-15, punto 3); y el tamaño: PB-T0 hizo 21 CT y 20 mutantes en 437 turnos, y PB-T1 entera serían unos 30 CT, 15 mutantes, cuatro archivos de Application y la refactorización de W y WC.

**La secuencia:** PB-T1a → PB-T1b → PB-T1c, cada una desde la punta que deja cerrada la anterior, sin rebase entre tareas, con delegaciones sucesivas (`AUTOMATION_PLAN.md:557-558`). PB-T1a toca W y WC solo con mutantes, que revierte.

**Lo que no cambia en la fase:** PB-T2 y PB-T3 dependen de PB-T1, que cierra con PB-T1c. El contenido es el del §5 de [P].

**Propiedad de la división:** PB-T1a solo cambia `tests/` en su commit final y PB-T1b solo cambia Application. Por eso los anclajes de W y WC de PB-T1c son los del cierre de PB-TC2.

### D-PBT1-02. El tipo de estado: `PushBackEditableState` (R-PB-05)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableState.cs`, `namespace RackCad.Application.Systems.PushBack` de bloque. Ni en `Pilot/` ni en `Workspace/`.

**Qué es:** un **agrupador** del estado que ya existe, sin modelo paralelo (D-10, ADR-0029 D12). `public sealed class PushBackEditableState`, con un constructor público sin argumentos y estos miembros:

| Miembro | Tipo | Valor inicial (el de hoy) | Es hoy |
|---|---|---|---|
| `Editor` | `PushBackCompositeEditorState` `{ get; }` | `new PushBackCompositeEditorState(new PushBackEditorState(), new PushBackEditorState())` | `composite` (W:56-57) |
| `Pallet` | `PalletSpecification` `{ get; set; }` | `PushBackEditorInputs.NewDesign().Pallet` | `generalPallet` (W:101); **también lleva la unidad** |
| `PalletsDeep` | `int` `{ get; set; }` | `DynamicRackDefaults.DefaultPalletsDeep` | `PalletsDeepBox` |
| `PostCatalogId` | `string` `{ get; set; }` | `null` | `PostBox` |
| `PostPeralte`, `PalletTolerance`, `BeamDepth`, `AnnotationScale` | `double` `{ get; set; }` | `PushBackEditorInputs.NewDesign().PostPeralte`, `DynamicRackDefaults.DefaultPalletTolerance`, `DynamicRackDefaults.DefaultBeamDepth`, `1.0` | W:106-109 |
| `FirstLevelDatum` | `int?` `{ get; set; }` | `(int)RackFirstLevelDatumMode.LowestUsablePunch` | W:380 |
| `NumberFronts`, `NumberLevels`, `DrawRackName` | `bool` `{ get; set; }` | `false` | las tres casillas |
| `Dimensions` | `DimensionDetail` `{ get; set; }` | `DimensionDetail.None` | `DimensionsBox` |
| `DimensionStyle` | `string` `{ get; set; }` | `null` | `loadedDimensionStyle` (W:1256) |
| `LoadedDimensionViews` | `DimensionViewVisibility?` `{ get; set; }` | `null` | W:1249 |
| `DimensionViewsTouched` | `bool` `{ get; set; }` | `false` | W:1260 |
| `DimensionsFrontal`, `DimensionsLateral`, `DimensionsPlanta` | `bool` `{ get; set; }` | `false` | las tres casillas de cotas |
| `Advanced` | `PushBackEditorInputs` `{ get; }` (solo sus seis avanzados) | `PushBackEditorInputs.NewDesign()` | `advanced` (W:114) |
| `SafetySelections` | `List<SelectiveSafetySelection>` `{ get; }` | vacía | `safetySelections` (W:84) |
| `Name` | `string` `{ get; set; }` | `null` | el nombre de la identidad (HT1-05) |

**No contiene** (D-PBT1-08): el GUID, `sourceProject`, `isEditingExisting`, `frontalSide`, el origen de cabecera pendiente, el estado mixto, `lastComputation` ni lo demás de HT1-06. **Ningún campo nuevo en los tipos del censo del piloto** (HT1-21).

### D-PBT1-03. Las cargas, con la semántica y el orden de hoy (I-04)

Son **métodos de instancia que mutan en sitio** el mismo `Editor`, como hoy la ventana muta su `composite`.

**La carga se parte en dos,** para que la ventana adopte los portadores donde hoy (dentro de `LoadFromModel`, bajo `suppressSync`, **después** de la posible pasada reentrante de HT1-10b):

| Método (público) | Reproduce | Pasos, en este orden |
|---|---|---|
| `PushBackEditorInputs LoadNewEditor()` | W:273-276 | `Editor.SetActiveSide(PushBackSide.A)`; `Editor.SetSideBPresent(false)`; `Editor.LoadComposite(null)`; devuelve `Editor.Active.LoadNew()` |
| `PushBackEditorInputs LoadDesignEditor(PushBackDesign design, PushBackResolver resolver)` | W:316-319 (= W:299-302) y WC:827-932 con `preserveDormantIntent: false` | `SetActiveSide(A)`; `SetSideBPresent(false)`; `var inputs = Editor.Active.LoadFromDesign(SideAOnly(design), resolver)`; `LoadComposite(design, false, resolver)`; devuelve `inputs` |
| `PushBackEditorInputs RestoreEditor(PushBackDesign lastValidDesign, PushBackResolver resolver)` | W:3320-3327 | igual, con `preserveDormantIntent: true` |
| `void AdoptCarriers(PushBackEditorInputs inputs, RackCatalog catalog)` | la parte de portadores de `LoadFromModel` (W:391-427) | abajo |
| `void SeedDefaultSafety(RackCatalog catalog)` | W:286-287 | `SafetySelections.Clear()`; `SafetySelections.AddRange(new PushBackSafetyAuthority(catalog).Defaults())` |
| `PushBackEditorInputs LoadNew(RackCatalog catalog)` | lo anterior, para quien no es la ventana | `LoadNewEditor()`; `AdoptCarriers(inputs, catalog)`; `Name = null` |
| `PushBackEditorInputs LoadDesign(PushBackDesign design, string rackName, PushBackResolver resolver)` | ídem | `LoadDesignEditor(…)`; `AdoptCarriers(inputs, resolver.Catalog)`; `Name = rackName` |
| `PushBackEditorInputs Restore(PushBackDesign lastValidDesign, PushBackResolver resolver)` | ídem | `RestoreEditor(…)`; `AdoptCarriers(inputs, resolver.Catalog)`; `Name` sin tocar |
| `static PushBackDesign SideAOnly(PushBackDesign design)` | W:342-373, movido literal | — |

- **`LoadComposite(design, preserveDormantIntent, resolver)`** es privado y es WC:829-931 movido literal, con dos cambios sin efecto observable:
  1. no llama a `UpdateCompositeEnabled()`, que es de la interfaz;
  2. **la cola que aparca se copia campo a campo y literal (I-05):** cada `DynamicRackModuleDesign` como `new DynamicRackModuleDesign { ModuleId, Kind, Length, IsCalculated, IsManualOverride, UseCalculatedHeaderConfiguration, HeaderConfiguration = <store>.DeepCopy(HeaderConfiguration), Notes }`, con los mismos ocho campos que copia el despertar (DA:557-566), y cada `DynamicHeaderLineOverride` como `new DynamicHeaderLineOverride { PostIndex, ModuleId, Header = <store>.DeepCopy(Header) }`. `<store>` es un `RackFrameProjectStore` (RFPS:87) **local al método, nunca un campo** (N-M-06): un campo de instancia entraría en el censo de CT-PBT1B-06 y rompería INV-PBT1B-03. **No se usa `CopyIntent` de RMES,** que es privado y normaliza la procedencia (M-3).
- **`AdoptCarriers(inputs, catalog)`** reproduce W:391-427 con sus normalizaciones literales:
  - `Pallet` = `inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg")`, con la unidad en blanco → «kg» (W:392, W:395), como objeto nuevo;
  - `PalletsDeep = Math.Max(2, inputs.PalletsDeep)` (W:396);
  - `PalletTolerance`, `BeamDepth` y `AnnotationScale` con su «> 0 ? : defecto» (W:397-398, W:402-403, W:418-419);
  - `PostPeralte = inputs.PostPeralte` (W:401);
  - **`PostCatalogId = ResolvePostCatalogId(catalog, inputs.PostCatalogId)`** (D-PBT1-09);
  - los seis avanzados copiados a `Advanced` (W:406-411);
  - de `inputs.Annotations ?? new DynamicAnnotationOptions()`: `DimensionStyle`, las tres casillas y `Dimensions` recortado a `[0, Detailed]` (W:413-420);
  - `LoadedDimensionViews = options.DimensionViews`, `DimensionViewsTouched = false` y cada casilla de cotas = `DimensionViewPolicy.EffectiveDetail(DimensionDetail.Minimal, views, kind) != DimensionDetail.None` (W:1268-1287);
  - `FirstLevelDatum = inputs.FirstLevelDatum` (W:391);
  - `SafetySelections` vaciada y llenada con `DeepCopy` de las no nulas (W:423-427).
- **Una carga que lanza** (c-13, D-3) lanza el mismo tipo y el mismo mensaje que hoy, desde el resolver, antes de tocar los portadores.

### D-PBT1-04. La salida, con la semántica de hoy

- **`PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler assembler)`** devuelve **un objeto nuevo en cada llamada**, igual campo a campo al de `ReadInputs` (W:846-889) con los controles sin editar: `Pallet` nueva con los cinco valores; los escalares y los seis avanzados copiados; `Annotations` nuevas con `DimensionViews = DimensionViewPolicy.FromEditor(LoadedDimensionViews, DimensionViewsTouched, DimensionsFrontal, DimensionsLateral, DimensionsPlanta)`; `SafetySelections` = `assembler.AuthorizedSafety(SafetySelections)`.
- **Por qué nuevo cada vez:** el ensamblador muta los inputs (DA:302).
- **El armado no cambia:** sigue siendo `assembler.BuildFrom(compositeAssembler.BuildDesign(state.Editor, state.BuildInputs(assembler)), frontalSide)` y la adopción de W:674-704. **No hay API de asiento** (I-03): PB-T1 no la crea, y las pruebas usan el ayudante `Pasada` (§5.0).

### D-PBT1-05. El copiador (H-PB-27)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableStateComparer.cs`. **Firmas:** `public sealed class PushBackEditableStateComparer : IEqualityComparer<PushBackEditableState>`, con `public static PushBackEditableStateComparer Instance { get; }` y `public static PushBackEditableState Copy(PushBackEditableState state)`. **El copiador del piloto no se toca.**

**Qué hace,** en este orden:
1. **El contenido editable y la selección:** el `Snapshot()`/`Restore()` de CS sobre un `Editor` nuevo, con la reposición de `ActiveSelection`, de la selección de cada lado y de `ActiveSide` que hace el piloto (PC:64-83), copiada.
2. **La cola:** `copy.Editor.ParkDormantTail(<copias>, <copias>)` con la copia literal de D-PBT1-03; después, si `source.Editor.DormantTailConsumptionPending`, `copy.Editor.MarkDormantTailConsumed()` (CS:116-117).
3. **La autoridad de módulos de cada lado:** `copy.Editor.SideX.CopyAuthorityFrom(source.Editor.SideX)` (D-PBT1-07), sobre el lado ya restaurado:
   - la sesión con `RackModuleEditSession.Copy()` y el commit con `RackModuleCommit.Copy()`, o null si eran null;
   - `moduleSignature`, copiada;
   - **`workingBaseline` y `LastModuleReconciliation`, compartidos por referencia: es la excepción L-1 (a)** (§8.1).
4. **Los portadores:** campo a campo; `Pallet` nueva; los seis avanzados de `Advanced`; `SafetySelections` con `DeepCopy`; `Name`.

**El resultado** es igual para el comparador; conserva los módulos (sesión con lo escenificado, commit y línea base), la cola (también la que `ParkTailOf` aparcó desde la línea base, ahora copiada) y el lado B con `sideBEverDeclared`; y **no comparte con el original ningún objeto mutable fuera de los dos derivados de L-1 (a) y de lo que ellos alcanzan**. En particular, la cola, el commit y la sesión de la copia no comparten **nada** mutable con el original (CT-PBT1B-07, parte 2).

### D-PBT1-06. El comparador

Compara con igualdad exacta (un `double` por valor, una cadena ordinal):
- **el contenido editable del `Editor`**, como el piloto (PC:134-249, copiado);
- **la cola**, elemento a elemento, con la forma persistida de cada configuración de cabecera (`RackFrameProjectStore`, como RMES:818-846);
- **la autoridad de módulos de cada lado:**
  - **siempre** la intención de módulos de la línea base (`RackModuleEditSession.Begin(<baseline>.Structure).Commit()` y las listas `HeaderLineOverrides` y `DerivedPostLineOverrides`, como DA:57-81), con igualdad estructural (M-15);
  - **siempre** el commit, con `RackModuleCommit.SameAs`, o los dos null;
  - **la sesión, con esta regla (I-03, N-M-02):** una sesión es «sin cambios» si es null, o si `HasPendingChanges` es false **y** su lista confirmada es equivalente, con `SameEditAs`, a `RackModuleEditSession.Begin(<línea base de su lado>.Structure)` recién abierta (la que abriría el getter, ES:124-133). Así una sesión que conserva la lista confirmada de una línea base anterior con la misma firma (`ReseedModuleSession`, ES:176-187) no cuenta como «sin cambios». Dos sesiones sin cambios son iguales. Si alguna tiene cambios, las dos tienen que existir y ser iguales con `SameEditAs`;
- **los portadores** (todos los de D-PBT1-02), con la igualdad estructural de la seguridad del piloto (PC:309-350, copiada).

**No compara:** la selección, la línea base como objeto, el informe de reconciliación ni la marca de consumo. `GetHashCode` mira campos baratos (como PC:37-49). `NaN` es igual a sí mismo.

### D-PBT1-07. Los métodos nuevos en ES y en RMES (sin campos nuevos)

- **ES:** `public void CopyAuthorityFrom(PushBackEditorState source)`, con el paso 3 de D-PBT1-05. **CS no cambia.**
- **RMES, en `RackModuleEditSession`:**
  - `public RackModuleEditSession Copy()`: una sesión nueva con `restored`, `committed`, `working`, `standardRestoreRequested`, las dos tablas de líneas y las dos de postes derivados, copiados **literales**: cada intención con un ayudante privado nuevo, `CopyLiteral`, que copia sus ocho campos con `clone.DeepCopy` para la configuración (como DA:557-566, **sin** `ResolveProvenance`), y las líneas con `CopyLines` (RMES:732-742);
  - `public bool SameEditAs(RackModuleEditSession other)`: `committed` y `working` con `AreEquivalent` (RMES:818-846), las líneas con `AreEquivalentLines` (RMES:750-769), los postes con `AreEquivalentDerivedPosts` (RMES:744-746), `restored` en orden y `standardRestoreRequested`.
- **RMES, en `RackModuleCommit`** (RMES:16-50), que no alcanza los privados de la sesión (M-09):
  - `public RackModuleCommit Copy()`: un commit nuevo por su constructor `internal` (RMES:18), con `Modules` copiados por un ayudante privado estático propio, `CopyModule`, idéntico a `CopyLiteral`; `RestoredModuleIds` en una lista nueva; `StandardRestoreRequested`; `LineOverrides` como `new DynamicHeaderLineOverride { PostIndex, ModuleId, Header = <store>.DeepCopy(Header) }`; y `DerivedPostOverrides` como `new DynamicDerivedPostLineOverride { PostIndex, Height }`;
  - `public bool SameAs(RackModuleCommit other)`: las mismas listas, en orden, comparando cada configuración por su forma persistida con un `RackFrameProjectStore` (como RMES:818-846).
  - **El `RackFrameProjectStore` de `RackModuleCommit`** (`<store>` en `Copy` y el de `SameAs`) es **local al método, nunca un campo** (N-M-06; INV-PBT1B-04). La sesión usa su campo `clone` de siempre (RMES:74).
- **Neutrales:** no nombran `RackSystemKind` ni Push Back; sus nombres no empiezan por un verbo de motor (`HeaderBatchContractTests` C-10, `:524-537`).

### D-PBT1-08. El mapa de propiedad (OB-05.1)

**Dónde:** `src/RackCad.Application/Systems/PushBack/PushBackEditableStateOwnership.cs`. Una **tabla de datos**, sin lógica de composición (es de PB-T3).

**La API exacta (I-07):**

```csharp
public enum PushBackFieldOwner { Unclassified = 0, Editor = 1, Envelope = 2, Container = 3 }

public enum PushBackDocumentLevel { Envelope = 0, Project = 1, Payload = 2 }

public sealed class PushBackOwnershipRow
{
    public PushBackOwnershipRow(PushBackDocumentLevel level, string path, PushBackFieldOwner owner, string source);
    public PushBackDocumentLevel Level { get; }
    public string Path { get; }
    public PushBackFieldOwner Owner { get; }
    public string Source { get; }
}

public static class PushBackEditableStateOwnership
{
    public static IReadOnlyList<PushBackOwnershipRow> Table { get; }
    public static string Normalize(string jsonPath);
    public static PushBackFieldOwner Of(PushBackDocumentLevel level, string jsonPath);
}
```

- **Los niveles:** `Envelope` es la raíz del JSON del sobre (`RackEmbedDocument`); `Project` es la raíz del JSON de su propiedad `Design` (`RackProjectDocument`); `Payload` es el objeto `PushBack` del proyecto (`PushBackDesignDocument`).
- **Las rutas:** de la forma `$.A.B[3].C`, con los nombres de propiedad del JSON. `Normalize` cambia todo índice `[n]` por `[*]`.
- **`Of`:** normaliza la ruta y busca, en las filas de su nivel, la de **prefijo más largo**. Una fila de ruta `Q` cubre una ruta `P` si `P == Q`, o si `P` empieza por `Q + "."` o por `Q + "["`. Si ninguna fila cubre la ruta, la primera propiedad de la ruta no es una propiedad declarada del documento de su nivel, y el nivel tiene `[JsonExtensionData]` (los tres la tienen), el resultado es `Envelope` («ExtensionData»). Si no, `Unclassified`.
- **La tabla:**

| Nivel | Ruta | Clase | Fuente (archivo y miembro) |
|---|---|---|---|
| Envelope | `$.SchemaVersion`, `$.Kind`, `$.View`, `$.Section`, `$.Id`, `$.CustomProperties` | `Envelope` | `RackEmbedDocument.cs`, `RackEmbedDocument` |
| Envelope | `$.Name` | `Editor` | `RackPushBackSystemWindow.xaml.cs`, `Name_Changed` |
| Envelope | `$.Design` | `Container` (su contenido se clasifica en el nivel `Project`) | `RackEmbedDocument.cs`, `RackEmbedDocument.Design` |
| Project | `$.SchemaVersion`, `$.Kind`, `$.Header`, `$.DynamicSystem`, `$.SelectiveRack`, `$.FlowBed`, `$.Larguero`, `$.Cantilever` | `Envelope` | `RackProjectDocument.cs`, `RackProjectDocument`; `RackPushBackSystemWindow.xaml.cs`, `SaveLibrary_Click` (`WithSourceMetadataFrom`) |
| Project | `$.PushBack` | `Container` (nivel `Payload`) | `RackProjectDocument.cs`, `RackProjectDocument.PushBack` |
| Payload | `$.SchemaVersion` | `Envelope` | `PushBackDesignDocument.cs`, `PushBackDesignDocument` |
| Payload | `$.Structure`, `$.Fronts`, `$.LegacyHighEndBeamPeralte`, `$.RearTopeSaque`, `$.RearTopePieceId`, `$.DefensePieceId`, `$.RearTopeOffCells`, `$.SideB`, `$.Composite` | `Editor` | `PushBackEditorDesignAssembler.cs`, `BuildDesign`; `PushBackCompositeEditorAssembler.cs`, `BuildDesign`; `DynamicEditorDesignAssembler.cs`, `BuildDesign` |
| (sesión del borrador) | `RackId` y los metadatos | `Envelope` | `DraftSession.cs`, `DraftSession` (no es una ruta: se documenta, sin fila) |

- **Por qué lo que la ventana solo transporta es `Editor` (M-14):** `FirstLevelDatum`, `DimensionStyle`, `LegacyHighEndBeamPeralte` y los demás campos sin control **forman parte del diseño que el editor escribe entero en cada salida** (D-PBT1-04; MB §1: «lo que la ventana carga y escribe»). [OD] D-PB-05 manda que «FULL EDITABLE DESIGN wins for every field owned by the full Push Back editor»: con «Conservar mi borrador», el diseño del borrador sale entero, también esos campos. «Lo que no edita la ventana» de OB-05.1 es lo que queda **fuera** de ese diseño: el sobre, el proyecto y la sesión.
- **`Name` es del editor; la identidad es el GUID** (OB-05.1).
- **Límite declarado** (OB-05.2, IR2 R-I-10): los campos desconocidos de los documentos **anidados** se pierden en cada reescritura; el mapa los cuenta como `Editor` por su contenedor. La pregunta al Owner sigue siendo de antes de PB-T3.

### D-PBT1-09. La regla de c-02 (F4B-E09, pregunta 2; M-05)

- **`public static string ResolvePostCatalogId(RackCatalog catalog, string requested)`:**
  - en blanco o solo espacios → `catalog?.Defaults?.Post` (c-01, W:399);
  - si no, el id de la opción de `PostBox` que coincide con `requested` según la **comparación fijada por el paso 1**; si ninguna coincide, `catalog?.Defaults?.Post` (c-02);
  - **los ids que ofrece `PostBox`** son los de `UiSupport.ToOptions(catalog.PostProfiles)` (`UiSupport.cs:124-133`), reproducidos en Application: las entradas no nulas con `Id` no en blanco, agrupadas por `Id.Trim()` sin mayúsculas, la primera de cada grupo, con su `Id.Trim()`.
- **La comparación, con sus dos ramas fijadas (N-M-03):** la decide la fila (b) del paso 1 de PB-T1b (INV-PBT1B-06), que mide lo que hace hoy `PostBox.SelectedValue` con el id del catálogo en minúsculas:
  - **rama ordinal** (la lectura; fila (b) con `PostBox.SelectedId=<null>`): coincide la opción cuyo id es ordinalmente igual a `requested`, y la regla devuelve `requested`;
  - **rama sin mayúsculas** (fila (b) con `PostBox.SelectedId=P`): coincide la opción cuyo id es igual a `requested` con `StringComparison.OrdinalIgnoreCase`, y la regla devuelve **el id de esa opción**, como `PostBox`.

  El Worker aplica la rama que mida, la declara en la entrega y usa las líneas esperadas de esa rama en CT-PBT1B-03. No hay otra rama.
- **Por qué así:** el Owner pidió «como hoy» y «RACKEDITAR no cambia», y hoy decide `PostBox.SelectedValue`. Con el catálogo entregado (un solo poste, que es el de por defecto; medición M-4), las filas (a) a (e) dan la misma salida con cualquier rama; la rama solo importa con un catálogo editado con varios postes, y así el Workspace hereda la regla de hoy.
- **Dónde vive:** en `AdoptCarriers`. La ventana sigue sembrando `PostBox` con la expresión de hoy (W:399): el desplegable se ve igual.
- **CT-PB0-17 sigue en verde.**

### D-PBT1-10. La exclusión de `pb-21-cola` (F4B-E09, pregunta 3)

- **Declaración:** «Un rack de un solo sentido (`!design.IsComposite`) cuya `Structure` guarda módulos o líneas con ids de cola (`PushBackCompositeStructure.IsCompositeTailId`) **no es un supported classic rack** para la garantía sin pérdidas de PB-T1.»
- **`public static PushBackEditableSupport SupportOf(PushBackDesign design)`**, con `public enum PushBackEditableSupport { Supported = 0, DormantCompositeTail = 1 }`, en `PushBackEditableState.cs`. Es la condición del piloto (PM:160-170).
- **No cambia:** RACKEDITAR (pierde las 667 hojas al Actualizar, CT-PB0-01); `DormantTailOutOfPilotReason` en todo F4B (OB-03.1); la deuda H14.

### D-PBT1-11. El criterio de bloqueo de salida, extraído

`src/RackCad.Application/Systems/PushBack/PushBackEditorOutputGate.cs`, `public static class PushBackEditorOutputGate`:
- `public static IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics(PushBackCompositeEditorState composite, PushBackSystem system)` = `composite.IntentDiagnostics().Concat(PushBackCompositeDiagnostics.Evaluate(system)).ToList()` (WC:783-786);
- `public static bool IsBlocked(PushBackCompositeEditorState composite, PushBackSystem system)` (WC:806);
- `public static string StatusOr(PushBackCompositeEditorState composite, PushBackSystem system, string fallback)` (WC:789-801).

`RackBomOutputGate` y `PushBackCompositeDiagnostics` no cambian. Lo usan la ventana desde PB-T1c y el PREPARE de PB-T3.

### D-PBT1-12. M-07 de PB-TC2 y los demás arrastres

- **M-07 se reproduce tal cual, y no aplica OB-06.5:** OB-06.5 habla de pérdidas, y aquí el estado no cambia (HT1-18): la caja no consigue editarlo. Por OB-RE.1, RACKEDITAR no cambia; corregirlo sería una decisión nueva, que no se propone. CT-PBT1A-10 lo fija como «INCUMPLIMIENTO CARACTERIZADO». Con esto se cierra «con una posible vuelta por OB-06.5» de [PKC2] §10.
- **M-03 de PB-TC y de PB-TC2, D-5, M-04, CT-16 y M-02 de PB-TC** entran en PB-T1a (§5.2).
- **M-01 de PB-T0 queda sin corregir:** S0 no cambia y `Abrir` sigue en CT-PB0-17. Las pruebas nuevas no usan `Abrir` ni `AbrirFueraDePantalla`.
- **M-05 de PB-TC:** las pruebas nuevas confirman o cancelan los módulos en un `finally` interno.
- **D-3:** CT-PBT1A-13 en la ventana y CT-PBT1B-05 en el estado.

### D-PBT1-13. La ventana sobre el estado (PB-T1c): un cambio mínimo, sin MVVM ni estética

**Regla general:** cada campo que pasa al estado se queda con su **mismo nombre** como reenvío privado.

1. **El estado:** en lugar de `composite` (W:56-57), `private readonly PushBackEditableState editable = new PushBackEditableState();` y `private PushBackCompositeEditorState composite => editable.Editor;`.
2. **Los reenvíos**, en el sitio de cada campo y con su comentario: `safetySelections` (`=> editable.SafetySelections`, W:84); `generalPallet` (`get`/`set` sobre `editable.Pallet`, W:101); `rackTolerance`, `rackPostPeralte`, `rackBeamDepth` y `rackAnnotationScale` (sobre `PalletTolerance`, `PostPeralte`, `BeamDepth` y `AnnotationScale`, W:106-109); `advanced` (`=> editable.Advanced`, W:114); `firstLevelDatum`, `loadedDimensionViews`, `loadedDimensionStyle` y `dimensionViewsTouched` (sobre `FirstLevelDatum`, `LoadedDimensionViews`, `DimensionStyle` y `DimensionViewsTouched`; W:380, W:1249, W:1256, W:1260).
3. **Las cargas,** con el orden de hoy:
   - `LoadNew`: W:273-276 → `var inputs = editable.LoadNewEditor();`; W:286-287 → `editable.SeedDefaultSafety(catalog);`;
   - `LoadDesignForNew` y `LoadExisting`: W:299-302 y W:316-319 → `var inputs = editable.LoadDesignEditor(design, assembler.Resolver);` y, en la línea siguiente, `UpdateCompositeEnabled();` (el sitio donde hoy lo llama `LoadCompositeFromDesign`, WC:933);
   - `Restore_Click`: W:3320-3327 → `var inputs = editable.RestoreEditor(design, assembler.Resolver);` y `UpdateCompositeEnabled();`.

   El proyecto, el modo, la identidad y `LoadFromModel` siguen en su orden.
4. **`LoadFromModel` adopta los portadores donde hoy (I-04):** dentro del `try`, bajo `suppressSync`, **la primera sentencia** pasa a ser `editable.AdoptCarriers(inputs, catalog);`, y se quitan las asignaciones a portadores (W:391, :393, :398, :401, :403, :406-411, :414, :419 y :423-427). Las siembras de los controles **no cambian ni un carácter** (W:390, :392, :394-397, :399-400, :402, :413, :415-418, :420-421). Es equivalente: ninguna siembra lee un portador, y `LoadAdvancedRackParameters` (W:432) y `ShowDimensionViews` (W:421) corren después y ven los mismos valores.
5. **`ReadInputs`** escribe en el estado lo que lee, con la expresión de hoy, y devuelve `editable.BuildInputs(assembler)`:
   - la caja de fondo y la unidad, en `generalPallet` (la unidad, quinto argumento de la `PalletSpecification` de W:838-840, con la expresión de W:852);
   - las cuatro de `Keep`, como W:841-844;
   - `editable.PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep);` (W:853);
   - `editable.PostCatalogId = PostBox.SelectedId ?? editable.PostCatalogId;` (W:868, con el respaldo de las vigas de W:823 y W:826). Con la regla de D-PBT1-09, el valor del estado es el que `PostBox` ofrecería o el que hoy pone DA:169-171, así que la salida es la de hoy también con catálogos editados (espacios, duplicados por mayúsculas);
   - las tres casillas, `Dimensions` y las tres de cotas, con las expresiones de W:874-876, W:878 y W:1295-1297.

   `SelectedDimensionViews` (W:1291-1297), cuyo único uso es W:880, se retira.
6. **`SideAOnly`** (W:342-373) y **`LoadCompositeFromDesign`** (WC:813-814, WC:827-934) se retiran.
7. **El criterio de bloqueo:** `CompositeDiagnostics` y `CompositeStatusOr` delegan en `PushBackEditorOutputGate` con `composite` y `lastCompositeSystem`; `CompositeHasBlocking` guarda `lastCompositeSystem = system` y devuelve `PushBackEditorOutputGate.IsBlocked(composite, system)`.
8. **El nombre:** `private void SyncName() => editable.Name = session.Identity.Name;`, llamado inmediatamente después de cada `Adopt` y de cada `SetName` (W:279, :305, :322, :1237, :3352, :3405 y :3504).
9. **La costura:** `internal PushBackEditableState EditableStateForTest => editable;`, junto a las demás.

**Fuera, por la letra:** `ReadCellValues`, `CommitCurrentCell`, `ForEachEditedSide`, `Keep`, `KeepCell`, `KeepOptional`, `MixedBoth`, `OptionalVal`, `IntVal`, el estado mixto, los manejadores, los subdiálogos, `RequestDraw` (salvo `SyncName`), `SaveLibrary_Click` e `InsertBatch_Click` (salvo `SyncName`), WX y lo demás.

### D-PBT1-14. Lo que no cambia en todo PB-T1

- **Los constructores y las llamadas de los cuatro sitios** (OB-01.1b): W:131-161, el Plugin y `EditorModules`.
- **Las firmas públicas** de `LoadExisting`, `LoadDesignForNew` y `LoadNew` (W:270, :294, :311): el Plugin sigue llamando a `window.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)` (T-PG CT-25).
- **El modo WORKSPACE no se añade** (PB-T2).
- **El piloto no se toca** (OB-03.1): `Pilot/` entero.
- **`NumericField`, Cantilever, Domain, el Plugin, WX, `Workspace/` y `Drafts/`.**
- **Ninguna corrección de producto** (OB-RE.1, D-16).

## 4. Las guardas de T1B y de texto (OB-T1B.3)

**Conclusión:** el diseño mantiene en verde todas las guardas sin sustituir ninguna. OB-T1B.3 no obliga a volver al Owner.

| Guarda | Qué fija | ¿Depende del diseño? | Cómo queda en verde |
|---|---|---|---|
| **T-SG S-9** (`…SourceGuardTests.cs:293-298`) | `UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")` | Sí | `SaveLibrary_Click` solo gana `SyncName();` tras `SetName` (INV-PBT1C-04) |
| **T-SG CT-22** (`:323-341`, por índices) | El orden de las cinco sentencias de la biblioteca | Sí | `session`, `lastComputation` y `sourceProject` siguen en W; las cinco sentencias no cambian (INV-PBT1C-04) |
| **T-PG CT-25** (`PushBackClassicCharacterizationPluginGuardTests.cs:235-272`) | La carga del Plugin con `window.LoadExisting(...)` | Sí | El Plugin no se toca y la firma no cambia |
| T-SG S-7 (`:280-284`) | `new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();` | No | `Bom_Click` no cambia |
| T-SG CT-23 (`:356-379`) | El orden de `InsertBatch_Click` | Débil | Solo gana `SyncName();` tras su `SetName` (W:3352) |
| T-SG CT-21 (`:302-319`) | El host del menú | No | `EditorModules` no se toca |
| T-SG CT-16, S-5, S-6, S-8 y el configurador | Los subdiálogos | No | No se tocan |
| T-SG CT-19 e `InsertRequestedRaised` (`:235-264`) | Ningún token de teclado, foco ni `DialogResult` bajo `src/RackCad.UI/Systems/PushBack` | Débil | El diff de PB-T1c no los añade (INV-PBT1C-05) |
| **G-10** (`WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`) | Una sola `.RequestUpdate(` en W | Sí | Ningún archivo nuevo de `src` la contiene (INV-PBT1B-05, INV-PBT1C-05) |
| I-55 (`I55DeferredProductFixCharacterizationTests.cs:8-15`) | `PhysicalPostIndex(LateralSectionBox.SelectedItem)` y `corte.PostIndex == section` | No | No se tocan |
| Fact3 (`PushBackModuleEditorCharacterizationTests.cs:381-401`) | W sin `forceRebuild:` ni `forceRebuild)`, en crudo; DA con `StandardRestoreRequested` | Débil | Ninguna línea nueva de W contiene `forceRebuild`; DA no se toca |
| Desviador (`BlankFrontDesviadorHandoffTests.cs:311-318`) | Dos literales de W | No | La vista previa no se toca |
| **`PushBackRichPreviewTests.cs:348-367`** (M-13) | Por reflexión: el método `DrawSharedPreview`, un campo `EditorPreviewSurface`, y sin `DrawModel`, `RoleBrush` ni un campo `PreviewCanvasPainter` | Débil | Los reenvíos son propiedades, y el campo nuevo es de tipo `PushBackEditableState` (M-10 del registro) |
| T-KB, T-OW, T-RS, foco y `AnnotationScaleBox` | WX | No | WX no se toca |
| Censo de ventanas (`WindowCensusGuardTests.cs:86-120`) | Ningún `Window` nuevo | No | Ninguno |
| Corpus de WLC (`WorkspaceDraftPilotWindowLoadCharacterizationTests.cs:323-333`) | Ningún `pb-*.json` nuevo en `Fixtures/I64` | Restricción | Los diseños nuevos se fabrican en código |
| RAM Elements (`StructuralAnalysisRamElementsHardeningGuardTests.cs:84-92`) | Los literales de `tests/RackCad.Tests` | Restricción | Las pruebas nuevas no parten ninguna palabra de ese dominio |
| Núcleo de borradores (`WorkspaceDraftPilotIntentGuardTests.cs:232-244`) | `Drafts/` sin `PushBack` | Restricción | `Drafts/` no se toca |
| Espacio de nombres = carpeta (`NamespaceFolderGuardTests.cs`) | Un `namespace` de bloque por archivo de `src/` | Restricción | INV-PBT1B-04b |
| **El censo del piloto** (HT1-21) | Todo campo de instancia de su grafo | **Sí** | Solo métodos nuevos en ES y RMES (INV-PBT1B-04) |
| Las costuras internas (W:165-193, WC:43 y las de T-7 de PB-T0) | Las usan T1B, PB-T0, PB-TC y PB-TC2 | Sí | Se conservan las 40; se añade `EditableStateForTest` (INV-PBT1C-06) |

**Para PB-T2, informativo:** G-10, T-SG CT-23 y T-PG CT-25 son las que probablemente salten al separar aplicar, cerrar y escribir.

## 5. La red de pruebas

### 5.0 Recetas y ayudantes comunes (I-03, M-02)

**Las recetas de los diseños fabricados** (todas en código; ninguna en `Fixtures/I64`):
- **X-01 (ventana):** copia literal de `BuildX01` (`PushBackFullEditorCharacterizationStateTests.cs:90-113`): `pb-21-cola`; `SideBPresentCheck` a true; `SideSelectorBox` a 1; `EditorWindowTestSupport.SetFrontBlank(w, "PushBackMatrixGrid", 0, false)`; `LastComputation.Design`, `RackId` y `RackName`; reabierto con `LoadExisting` en otra ventana. Con `Ventana()` y `try`/`finally`.
- **X-01 (Core):** `pb-21-cola` con `LoadDesign` y `Pasada`; `Editor.SetSideBPresent(true)`; `Editor.SetActiveSelection(PushBackSideSelection.B)`; `Editor.SetSlotPresent(PushBackSide.B, 0, true)`; `Pasada`; el diseño de esa computación.
- **F-01, compuesto con módulos personalizados en A y en B:** `pb-20-ab`; en la sesión del lado A (en la ventana, con `SelectModule`, `SetNumberR(w, "ModuleLengthBox", …)` y `Click(w, "ConfirmModuleButton")`; en Core, con `Editor.SideA.ModuleSession.SetLength` y `Editor.SideA.CommitModuleEdits()` seguidos de `Pasada`), el primer módulo de `ModuleSession.Modules` de clase `Separator` sin prefijo `B:` pasa a su longitud + 2,0, y después el primero con prefijo `B:` de clase `Separator`, también + 2,0; el diseño resultante se reabre. CT-PBT1B-01 comprueba que las dos recetas dan el mismo diseño.
- **F-02:** `pb-20-ab` con `FrontalSideBox` a 1 («Frontal de B»). Solo en la ventana.
- **F-03 (I-04):** `pb-01-base` con `Structure.PalletTolerance = 3.00044` (`EditInner`); `SideBPresentCheck` a true (B declarado y sin frentes: el diseño sigue de un sentido); `FrontalSideBox` a 1; `AdvancedModulesToggle` a true, `DerivedPostReinforcedCheck` a true y `SetNumberR(w, "DerivedPostReinforcementHeightBox", 100000)` (un cálculo inválido: el refuerzo supera la cabecera, `PushBackAdvancedRackParameters.ValidateReinforcementAgainstPost`); `SetNumberR(w, "ToleranceBox", 4)`; y «Restaurar» (`Click(w, "RestoreButton")`). Solo en la ventana. **Se comprueba a sí misma (N-M-08):** `RecomputePassesForTest` se lee justo antes y justo después del clic de «Restaurar», y `FrontalSideForTest` después; lo esperado es `pasadas-restaurar=2` (la reentrante de `UpdateCompositeEnabled` y la de `LoadFromModel`) y `frontal=A`. Otro valor quiere decir que el guion no ejerce el camino de I-04, y es PBT1A-C07.

**El ayudante `Pasada`** (privado de cada clase que lo use; copia literal de W:659-704, adaptada al estado):

```csharp
private static PushBackEditorComputation Pasada(
    PushBackEditableState s, PushBackEditorDesignAssembler a, PushBackCompositeEditorAssembler ca, PushBackSide frontalSide)
{
    PushBackEditorComputation computation;
    try { computation = a.BuildFrom(ca.BuildDesign(s.Editor, s.BuildInputs(a)), frontalSide); }      // W:659-660
    catch (Exception) { return null; }                                                               // W:662-670
    if (!computation.IsValid) return computation;                                                    // W:714-719
    a.AcceptComputation(s.Editor.SideA, computation);                                                // W:675
    s.Editor.CommitDormantTailConsumption();                                                         // W:679
    var standardRestoreLanded = s.Editor.SideA.ModuleCommit?.StandardRestoreRequested == true;      // W:696
    s.Editor.SideA.ClearModuleCommit();                                                              // W:697
    if (standardRestoreLanded) s.Editor.Active.RestoreAdvancedRackParameters(s.Advanced);           // W:698-702
    _ = s.Editor.SideA.ModuleSession;                                                                // W:704 -> W:1688
    return computation;
}
```

`a` y `ca` son dos instancias nuevas con el mismo catálogo, como W:145-146. En la UI, el catálogo es `w.Session.Catalog`; en Core, `JsonRackCatalogProvider.FromBaseDirectory().Load()`.

**El número de pasadas de cada camino** (las de la ventana, que el estado reproduce en este orden):

| Camino | Secuencia |
|---|---|
| Ventana nueva | `LoadNew(catalog)`; `Pasada`; `SeedDefaultSafety(catalog)`; `Pasada` (W:280-288) |
| `LoadExisting` o `LoadDesignForNew` | la ventana nueva y después `LoadDesign(…)`; `Pasada` (W:439) |
| «Restaurar» | lo anterior; `Restore(<diseño de la última computación válida>, resolver)`; `Pasada` |
| «Actualizar» | lo anterior; `Pasada` (W:3391) |

**La regla «EXCEPCION»** (PB-T1b y PB-T1c): cada paso que llama al API nuevo pasa por un ayudante privado que convierte una excepción en la línea `EXCEPCION <Tipo>: <Mensaje>`. Las líneas esperadas no contienen ninguna.

### 5.1 Lo que tiene que seguir en verde sin tocarlo (las tres subtareas)

T1B (55 y 10), PB-T0 (16 y 3, con SUS-03 y SUS-04 en `Skip`), PB-TC (10), PB-TC2 (4), todas las clases `PushBack*` de UI (538) y las de Core que leen la ventana, `RackCad.Tests.Workspace` entero (con el censo del piloto), Cantilever (67) y, una vez cerradas, PB-T1a y PB-T1b.

### 5.2 PB-T1a: la red de caracterización (lista cerrada CT-PBT1A-01 a 14)

**Régimen** (el de PB-T0 y T1B, copiado en el contrato):
- se fija lo que hay; las líneas esperadas **las mide el Worker en la `BaseSha`** con código temporal (paso 1) y las transcribe; si algo contradice el diseño, la línea lleva «INCUMPLIMIENTO CARACTERIZADO»;
- el RED son las pruebas nuevas y los quince mutantes, juntos, en W y WC; el GREEN los revierte con `git restore --source=<BaseSha>`. Cada mutante, aislado, hace fallar por aserción el CT de su fila;
- ninguna prueba existente se edita, se reactiva, se salta ni se borra; las existentes que fallen en el RED se atribuyen a un mutante (tabla T-8);
- ventanas con `Ventana()`, la carga dentro del `try` y `Cerrar` en `finally`; ninguna mostrada salvo la de CT-11 (M-03);
- el aviso de descarte, sustituido, con el sufijo ` descartes=<n>` en la última línea de cada ventana, leído tras cerrarla (D-PBTC2-08);
- sin teclado ni foco real; sin `MessageBox` real, BOM, «Guardar en biblioteca», configurador real ni «Insertar»;
- «R», cultura invariante, GUID fijo, orden ordinal; los JSON con la navegación de SC1.

**Las clases** (`tests/RackCad.UI.Tests/`, `namespace RackCad.UI.Tests`, `public sealed`, `[Collection(EditorDiscardPromptCollection.Name)]`, `[Fact]` que acaban en **un** `Assert.Equal`): `PushBackStateNetRackControlsTests` (01, 02), `PushBackStateNetCellAndFrontTests` (03, 04, 07, 08), `PushBackStateNetCompositeTests` (05, 09, 10), `PushBackStateNetModuleAndSafetyTests` (06, 11), `PushBackStateNetActionsTests` (12, 13) y `PushBackStateNetFingerprintTests` (14). El ayudante `PushBackStateNetSupport` (`internal static`, sin pruebas) solo tiene las recetas del §5.0 y las huellas.

| CT | Qué fija (sobre la salida persistida, nunca sobre el estado privado) | Origen | Mutante |
|---|---|---|---|
| **01** | Las tres casillas de anotación, `DimensionsBox` y las tres de cotas, cada una sola, en la salida; un `DimensionViews` nulo sigue nulo sin tocarlas, y tocar una escribe el valor de I-50 | D-5 | MU-T1A-01 |
| **02** | `WeightUnitBox` a «lb»; la carga con «ton» (c-09) y su Actualizar; `PalletsDeepBox` editado | D-5 | MU-T1A-02 |
| **03** | `PositionsBox`, `LevelsBox`, `FondosBox` y `DepthStartBox` en un frente; lo mismo con «Ambos lados» en un compuesto (`Structure.Fronts` y `SideB.Fronts`) | D-5 | MU-T1A-03 [TC2] |
| **04** | `CellIntermediatePeralteBox`, `CellFondoOverrideBox` y `CellDrawPalletCheck` en la celda de la salida | D-5 | MU-T1A-04 [TC2] |
| **05** | `CentralSeparatorCheck`, `StructureOverrideBox` con «Aplicar» y «Restaurar estructura», la topología, el sentido y el alcance, y `FrontCountBox` | D-5 | MU-T1A-05 |
| **06** | `DerivedPostLineHeightBox` en una línea: nada hasta «Confirmar»; después, `Structure.DerivedPostLineOverrides`; cancelar no deja nada | D-5 | MU-T1A-06 |
| **07** | **M-03 de PB-TC:** un valor exacto sin editar de la celda (61,23456) llega exacto por el alcance «Todas», por «Aplicar a todos los frentes» y tras «Restaurar»; **la ruta de los portadores del rack (M-04):** con `PalletTolerance = 3.00044` y `AnnotationScale = 1.23456`, esos valores siguen exactos tras el alcance, los datos del frente, un cambio de celda y «Restaurar»; y **el caso negativo de D-PB-11:** «Restaurar módulo» individual con «Confirmar» no reinicia los avanzados | M-03 de PB-TC | MU-T1A-07a [TC2], MU-T1A-07b |
| **08** | **M-03 de PB-TC2:** con B presente y «Ambos», una caja mixta escrita llega a los dos lados por el alcance (W:1346) y por los datos del frente (W:1510); sin escribirla, cada lado conserva el suyo | M-03 de PB-TC2 | MU-T1A-08 |
| **09** | **M-04:** «+ frente», «- frente» y «En blanco», con y sin B: `Structure.Fronts.Count`, `SideB.Fronts.Count`, `Composite.AbsentSlotsA` y `Composite.AbsentSlotsB` | M-04 de PB-T0 | MU-T1A-09 |
| **10** | **M-07, INCUMPLIMIENTO CARACTERIZADO:** el camino de H2-11 y después vaciar «Largo manual»: la salida conserva el override; una caja obligatoria vacía no marca error | M-07 de PB-TC2 | MU-T1A-10 [TC2] |
| **11** | **La observación de CT-16:** aceptar Seguridad sin cambios sobre `pb-05-topes`: `Structure.SafetySelections` y las líneas del BOM antes y después. **La ventana (M-03):** `var w = Ventana(); try { w.WindowStartupLocation = WindowStartupLocation.Manual; w.ShowInTaskbar = false; w.ShowActivated = false; w.Left = -10000; w.Top = -10000; Cargar(w, t); w.Show(); w.UpdateLayout(); … } finally { Cerrar(w); }`, la preparación de `AbrirFueraDePantalla` (S0:139-154) con la carga dentro del `try` | CT-16 de PB-T0 | MU-T1A-11 |
| **12** | **M-02 de PB-TC:** `IsEnabled` de los once botones de acción con el aviso de D-PB-08 visible y sin él | M-02 de PB-TC | MU-T1A-12 |
| **13** | **D-3:** `LoadExisting` con `PalletTolerance = 0`: el tipo y el mensaje de la excepción, y que la ventana se cierra en el `finally` | D-3 | MU-T1A-13 |
| **14** | **Las huellas de la ventana.** El corpus extendido: los 17 de `Fixtures/I64`, X-01, F-01, F-02 y **F-03**. Por diseño y guion, la línea `<diseño> <guion> valido=<b> bloqueado=<b> sha256=<huella de Persist(LastComputation.Design)>`. **Los guiones, cada uno sobre una ventana nueva y en este orden (M-13):** G-1 cargar; G-2 cargar y «Actualizar» (la huella es la de `DesignToInsert`; la ventana se cierra); G-3 cargar y «Restaurar»; G-4 cargar, retirar B y volver a declararlo (solo compuestos); G-5 cargar, «Restaurar estándar» y «Confirmar»; G-6 cargar, escribir 61.5 en `CellPalletHeightBox` y aplicar a «Todas». **F-03 tiene un solo guion,** el suyo (§5.0), con su línea `F-03 G-F03 valido=<b> bloqueado=<b> sha256=<huella> PalletTolerance=<R> pasadas-restaurar=<n> frontal=<lado>`, con `pasadas-restaurar=2` y `frontal=A` esperados (N-M-08) | La equivalencia de PB-T1c | MU-T1A-14 |

**La tabla cerrada de mutantes** (W y WC; sustitución exacta de una línea en la `BaseSha`; las [TC2] se vuelven a medir sobre el GREEN de PB-TC2 y, si su texto no es el de abajo, es PBT1A-C07):

| Id | Miembro | Línea de hoy | Sustitución |
|---|---|---|---|
| MU-T1A-01 | `ReadInputs` | W:874 `NumberFronts = NumberFrontsCheck.IsChecked == true,` | `NumberFronts = NumberLevelsCheck.IsChecked == true,` |
| MU-T1A-02 | `ReadInputs` | W:852 `generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),` | `generalPallet.Weight, "kg"),` |
| MU-T1A-03 [TC2] | `ReadCellValues` | W:817 `DepthStartPosition = IntVal(DepthStartBox, front.DepthStartPosition),` | `DepthStartPosition = front.DepthStartPosition,` |
| MU-T1A-04 [TC2] | `ReadCellValues` | W:827 `IntermediateBeamDepth = SelectedPeralte(CellIntermediatePeralteBox, cell.IntermediateBeamDepth)` | `IntermediateBeamDepth = cell.IntermediateBeamDepth` |
| MU-T1A-05 | `CentralSeparator_Changed` | WC:209 `composite.SetCentralSeparator(CentralSeparatorCheck.IsChecked == true);` | `composite.SetCentralSeparator(false);` |
| MU-T1A-06 | `ApplyDerivedPostLines_Click` | W:2133 `var height = DerivedPostLineHeightBox.Value;` | `var height = (double?)null;` |
| MU-T1A-07a [TC2] | `Keep` | W:3686 `=> !field.Value.HasValue \|\| SameShown(field.Value.Value, current) ? current : field.Value.Value;` | `=> !field.Value.HasValue ? current : field.Value.Value;` |
| MU-T1A-07b | `Recompute` | W:696 `var standardRestoreLanded = moduleOwner.ModuleCommit?.StandardRestoreRequested == true; // I-64 F4B PB-TC (D-PB-11)` | `var standardRestoreLanded = moduleOwner.ModuleCommit != null;` |
| MU-T1A-08 | `ApplyFrontData` | W:1510 `ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues(side).Dynamic, frontTargets));` | `ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues().Dynamic, frontTargets));` |
| MU-T1A-09 | `AddFront_Click` | W:1171 `=> MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 1));` | `=> MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 2));` |
| MU-T1A-10 [TC2] | `OptionalVal` | W:3668 `: (IsMixed(field) ? sideValue : null);` | `: null;` |
| MU-T1A-11 | `Safety_Click` | W:2682 `ApplyBootSections(bootSectionA, bootSectionB);` | `;` |
| MU-T1A-12 | `UpdateButtons` | W:3568 `BomButton.IsEnabled = canOutput;` | `BomButton.IsEnabled = canOutput && SideBEmptyNoticeText.Visibility != Visibility.Visible;` |
| MU-T1A-13 | `LoadExisting` | W:313 `if (design == null) return;` | `if (design == null \|\| design.Structure?.PalletTolerance <= 0.0) return;` |
| MU-T1A-14 | `LoadFromModel` | W:419 `rackAnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;` | `rackAnnotationScale = 1.0;` |

Las sangrías son las de la línea original. Ningún mutante toca un literal de I-55, Fact3, el desviador, G-10 ni T-SG: Core sigue en `success` en el RED. MU-T1A-14 discrimina por `pb-08-redondeo` (`AnnotationScale` 1,23456), que está en el corpus.

### 5.3 PB-T1b: el tipo (lista cerrada CT-PBT1B-01 a 13)

**Régimen** (el de F4-T2A): **el RED** es el **esqueleto inerte** y las pruebas nuevas, que fallan **por aserción**; **el GREEN** solo toca `src/`; **la ventana no cambia**; la regla «EXCEPCION» del §5.0.

**El esqueleto exacto:**
- `PushBackEditableState`: todos los miembros con sus valores iniciales; `LoadNewEditor`, `LoadDesignEditor`, `RestoreEditor`, `LoadNew`, `LoadDesign` y `Restore` devuelven `PushBackEditorInputs.NewDesign()` sin hacer nada más; `AdoptCarriers` y `SeedDefaultSafety` no hacen nada; `BuildInputs` devuelve `PushBackEditorInputs.NewDesign()`; `ResolvePostCatalogId` devuelve `requested`; `SupportOf` devuelve `Supported`; `SideAOnly` devuelve `design`;
- `PushBackEditableStateComparer`: `Instance`; `Equals` = `ReferenceEquals(x, y)`; `GetHashCode` = 0; `Copy` = `new PushBackEditableState()`;
- `PushBackEditableStateOwnership`: `Table` vacía; `Normalize` devuelve su argumento; `Of` devuelve `Unclassified`;
- `PushBackEditorOutputGate`: `Diagnostics` vacía; `IsBlocked` = false; `StatusOr` = `fallback`;
- ES `CopyAuthorityFrom` no hace nada; RMES `Copy()` = `RackModuleEditSession.Begin(Array.Empty<DynamicRackModuleDesign>())`; `SameEditAs` = false; `RackModuleCommit.Copy()` = `this`; `SameAs` = false.

**Las clases:** en `tests/RackCad.Tests/` (`namespace RackCad.Tests`): `PushBackEditableStateLoadTests` (02 a 05), `PushBackEditableStateCopyTests` (06 a 10 y 13), `PushBackEditableStateOwnershipTests` (11) y `PushBackEditorOutputGateTests` (12); en `tests/RackCad.UI.Tests/` (`namespace RackCad.UI.Tests`): `PushBackEditableStateParityTests` (01), en `EditorDiscardPromptCollection`.

| CT | Qué fija | Oráculo | Falla en el RED porque |
|---|---|---|---|
| **01** (UI) | **La paridad de las cargas** con la ventana de la base, con el ayudante `Pasada` y las secuencias del §5.0, sobre los 17 de `Fixtures/I64`, X-01 y F-01: ventana nueva; `LoadExisting`; `LoadDesignForNew`; «Restaurar» tras cargar, tras retirar B y tras `SetGap(60)`; «Actualizar». Por camino, `<diseño> <camino> iguales=<b> bloqueado-ventana=<b> bloqueado-estado=<b>`, comparando `Persist` de la ventana y del estado, y `OutputIsBlockedForTest` con `PushBackEditorOutputGate.IsBlocked`. Incluye los casos c-01 a c-16 de CT-PB0-17, **los cuatro bloqueantes de CT-12** (M-07) con su `StatusText` frente a `StatusOr`, y las dos recetas de X-01 y de F-01 (ventana y Core). **Cómo se producen los cuatro bloqueantes (N-M-04),** porque dos no se alcanzan con los controles (`GapBox` tiene `Minimum="0"`, WX:148, y `Gap_Changed` no escribe con error, WC:192-195; `StructureOverrideBox` tiene `Minimum="2"`, WX:235) y el hueco negativo tampoco por documento (`PushBackDesignDocument.cs:469` lo normaliza a 0): **hueco negativo:** `pb-20-ab`; en la ventana, la costura `w.CompositeState.SetGap(-1)` y un `LostFocus` elevado en `AnnotationScaleBox`, sin editarla, que dispara el recálculo (`Input_Changed`, WX:68); en el estado, `Editor.SetGap(-1)` y `Pasada`. **Separador sin hueco:** `pb-20-ab` con `Composite.CentralSeparator = true` y `Composite.Gap = 0` (`EditInner`). **Estructura manual de 1:** `pb-20-ab` con `Composite.StructureOverrideA = 1` (`EditInner`). **Celda que no cabe:** el diseño de `PushBackOutputGateTests.Blocking()` (`:41-48`, sus llamadas copiadas), cargado con `LoadExisting` en la ventana y con `LoadDesign` en el estado | la ventana de hoy | el esqueleto no carga |
| **02** | **La ida y vuelta sin pérdidas** de los racks soportados: diseño → estado → diseño, y estado → diseño → estado (igual para el comparador) | **(M-01)** los 8 canónicos: la salida del piloto (CT-PBTC-06, `PushBackDeclaredCorrectionPilotParityTests.cs:20-26`, iguala ventana y piloto, y `WorkspaceDraftPilotCanonicalLineEndingTests` iguala piloto y sobre): delta vacío; los 6 heredados: exactamente las líneas de `LegacyDeltas` (`WorkspaceDraftPilotMaterializationTests.cs:706-735`), copiadas; `pb-20-ab`, `pb-31-v2-modulos` y X-01: delta vacío (CT-PB0-01); F-01: delta vacío | el esqueleto da el diseño por defecto |
| **03** | **c-02:** `ResolvePostCatalogId` con el id del catálogo, el mismo en minúsculas, uno inexistente, en blanco, solo espacios y nulo; con un catálogo fabricado en Core con un id con espacios (`" PB1_X "`) y dos ids que solo difieren en mayúsculas (`PB1_Y`, primero, y `pb1_y`), con `Defaults.Post` = P; y el `HeaderPostCatalogId` de la salida del estado para c-01 y c-02. **Las líneas del catálogo fabricado, por rama (N-M-03):** `PB1_X` da `PB1_X` en las dos ramas; `" PB1_X "` da P en las dos; `PB1_Y` da `PB1_Y` en las dos; `pb1_y` da P en la rama ordinal y `PB1_Y` en la rama sin mayúsculas. El Worker usa las de la rama que fijó el paso 1 | el paso 1 de PB-T1b (INV-PBT1B-06); CT-PB0-17 | el esqueleto devuelve `requested` |
| **04** | **`pb-21-cola`:** `SupportOf` da `DormantCompositeTail` para `pb-21-cola` y `Supported` para los demás; su ida y vuelta pierde exactamente las 667 hojas de CT-PB0-01 (`delta-filtrado=667 sha256=0b54e8ff5c8ad08d262bdfd8e30db37eb0dd40b19797940f8a5fd85b04eed875`); el piloto sigue dando `OutOfPilot` con `DormantTailOutOfPilotReason` | CT-PB0-01; PM:108 | el esqueleto da `Supported` |
| **05** | **Los demás de T-2, tal cual:** c-01, c-03, c-04/c-06, c-05/c-07, c-08/c-09, c-10/c-11/c-16, c-14 y c-15 dan las líneas de CT-PB0-17; c-12, rechazado por el almacén; **c-13 lanza al cargar el estado el mismo tipo y el mismo mensaje** | CT-PB0-17 | el esqueleto no carga ni lanza |
| **06** | **El censo** (M-08): todo campo de instancia del grafo de `PushBackEditableState` está en la tabla, y toda entrada existe. **La tabla** es `PilotCensus.Table` (`WorkspaceDraftPilotStateTests.cs:211-369`), copiada **sin** las tres filas de `PushBackPilotDraftState`, con estas reclasificaciones: `workingBaseline` y `LastModuleReconciliation` → **D** (derivado compartido, no se recorre); `moduleSession`, `moduleCommit`, `moduleSignature`, `DormantCompositeTail` y `DormantTailLineOverrides` → **A** (autoridad copiada, no se recorre); `dormantTailConsumed` → **T** (transitorio del armado: la marca entre `BeginDormantTailConsumptionAttempt` y la adopción, CS:116-147; se copia y no se compara); y con las filas nuevas de `PushBackEditableState` (`Editor`, `Pallet`, `PalletsDeep`, `PostCatalogId`, `PostPeralte`, `PalletTolerance`, `BeamDepth`, `AnnotationScale`, `FirstLevelDatum`, `NumberFronts`, `NumberLevels`, `DrawRackName`, `Dimensions`, `DimensionStyle`, `LoadedDimensionViews`, `DimensionViewsTouched`, `DimensionsFrontal`, `DimensionsLateral`, `DimensionsPlanta`, `Advanced`, `SafetySelections`, `Name`) → **E**. Con un estado rico (F-01 con una edición escenificada, un commit y `pb-20-ab` con B retirado), cada campo **E** sobrevive a `Copy` | el censo del piloto | `Copy` da un estado vacío |
| **07** | **Sin alias (I-01, I-02),** sobre `pb-06-seguridad`, `pb-07-combinado`, F-01 con una edición escenificada y un commit, `pb-20-ab` con B retirado tras una `Pasada` (cola aparcada por `ParkTailOf`) y X-01. **Parte 1:** la copia es igual; los objetos compartidos por todo el grafo, quitados los inmutables (`IsImmutable` de `PilotCensus`, `:531-536`, copiado), son exactamente la línea base y el informe de cada lado y lo que ellos alcanzan. **Parte 2, sin exención:** los objetos que alcanzan la cola, el commit y la sesión de cada lado de la copia no comparten **ningún** objeto mutable con **todo** el grafo del original (línea base incluida). **El recorrido (N-M-05)** es el `AllObjects` del piloto (`WorkspaceDraftPilotStateTests.cs:501-529`), copiado y ampliado para entrar también en **las claves y los valores de todo `IDictionary`**, porque los `KeyValuePair` son tipos valor y el diccionario no es un tipo de producto: así ve las configuraciones de línea de la sesión (`committedLines` y `workingLines`, RMES:82-83). Las partes 1 y 3 usan el mismo recorrido ampliado. **Parte 3:** el contenido editable, la cola, la sesión y el commit de un estado recién cargado no comparten ningún objeto mutable con el diseño cargado | — | `Equals` es `ReferenceEquals` |
| **08** | **Independencia y no mutación:** mutar cada ranura **E** de la copia deja el original igual (comparador y salida), y al revés; y **ninguno de los consumidores medidos (M-1) que el API alcanza** muta una línea base adoptada: tras `Pasada`, `ParkTailOf` (retirar B y `Pasada`), `ModuleSession` (abrir, escenificar, confirmar), `PushBackEditorOutputGate`, `RackModuleDescriptor.Describe`, `PushBackSystemLateralBuilder.Cortes` y otra `Pasada`, en el original y en la copia, la huella `Persist(resolver.Snapshot(<línea base>))` y `<línea base>.Name` no cambian | M-1 | `Copy` da un estado vacío |
| **09** | **Módulos, cola y A/B:** sobre F-01, `pb-20-ab` con B retirado y X-01: la copia calcula el mismo diseño; volver a declarar B en la copia devuelve la cola intacta, como en el original; confirmar en la copia la edición escenificada da lo mismo que en el original | el original | `Copy` da un estado vacío |
| **10** | **El comparador:** dos cargas del mismo diseño, iguales; una con la sesión abierta y sin cambios y otra con la sesión nula, iguales (I-03); un estado cuya sesión, abierta y sin pendientes, conserva la lista confirmada de una línea base anterior de la misma firma (cargar `pb-31-v2-modulos`; `Pasada`; abrir la sesión; cambiar `Pallet` a un fondo de 50, que obliga a reconstruir con los mismos módulos; `Pasada`) frente al diseño de esa última computación cargado en un estado nuevo con su `Pasada`, distintos (N-M-02); cada cambio de contenido, cola, sesión con cambios, commit, línea base con el mismo commit (M-15) o portador, distintos; solo la selección, iguales; `NaN` reflexivo; `null` como el piloto | — | `Equals` es `ReferenceEquals` |
| **11** | **El mapa:** `Normalize` de `$.Structure.Fronts[3].Cells[0]` es `$.Structure.Fronts[*].Cells[*]`; toda ruta de hoja de los 17 sobres de `Fixtures/I64`, en sus tres niveles, sale distinta de `Unclassified`; en `Payload`, toda ruta salvo `$.SchemaVersion` es `Editor`; en `Project`, toda ruta es `Envelope` salvo `$.PushBack`, que es `Container`; en `Envelope`, `$.Name` es `Editor`, `$.Design` es `Container` y las demás son `Envelope`; una propiedad desconocida en la raíz de cada nivel es `Envelope`; toda fila tiene `Source` no vacía y sin número de línea | D-PBT1-08 | la tabla está vacía |
| **12** | **El criterio de bloqueo, con líneas literales (M-07):** con el sistema de `PushBackOutputGateTests.Valid()` (`PushBackOutputGateTests.cs:27-34`, sus llamadas copiadas) y un `PushBackCompositeEditorState` nuevo con `LoadComposite(new PushBackCompositeDesign { … })`: `valido bloqueado=False estado=RESERVA`; `hueco-negativo` (`Gap = -1`) `bloqueado=True estado=El hueco (-1") no es una separacion fisica valida: debe ser cero o positivo. Corrigelo; no se interpreta como otro valor.`; `separador-sin-hueco` (`CentralSeparator = true`, `Gap = 0`) `bloqueado=True estado=El separador central necesita un hueco mayor que cero: sin el no hay donde colocarlo.`; `override-no-construible` (`StructureOverrideA = 1`) `bloqueado=True estado=Lado A: la estructura manual (1) no es construible; el minimo fisico es 2 fondos. Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.`; con el sistema de `Blocking()` (`:41-48`) y un estado sin intención inválida, `celda-no-cabe bloqueado=True estado-igual-al-primer-bloqueante-de-Evaluate=True`; y `RackBomOutputGate.For(Valid()).Allowed=True`, `RackBomOutputGate.For(Blocking()).Allowed=False`. Cultura invariante | M-8 del registro | `IsBlocked` es false |
| **13** | **`BuildInputs` nuevo en cada llamada:** dos llamadas no comparten objetos; un «Restaurar estándar» confirmado (DA:302) no toca `Advanced`; los seis avanzados, la unidad y las casillas llegan a los inputs | D-PBT1-04 | `BuildInputs` da `NewDesign()` |

### 5.4 PB-T1c: la ventana sobre el estado (lista cerrada CT-PBT1C-01 a 03)

**Régimen:** **el RED** es la costura `internal PushBackEditableState EditableStateForTest => new PushBackEditableState();` en W y las pruebas nuevas, que fallan por aserción, con la regla «EXCEPCION» (M-11); **el GREEN** es D-PBT1-13. **La equivalencia la prueban las redes existentes sin tocarlas**, en particular CT-PBT1A-14 con F-03.

| CT | Qué fija | Falla en el RED porque |
|---|---|---|
| **01** (UI) | La ventana edita el estado extraído: `ReferenceEquals(w.EditableStateForTest.Editor, w.CompositeState)`; y, con las secuencias de pasadas del §5.0, el estado de la ventana tras cada camino (ventana nueva, `LoadExisting`, `LoadDesignForNew`, «Restaurar») es igual, con el comparador de PB-T1b, al del estado solo que siguió la misma secuencia, sobre los 17 de `Fixtures/I64`, X-01 y F-01 | la costura da otro estado |
| **02** (UI) | **Ningún control es la fuente de verdad:** para cada control del censo (§5.5) y su gesto, el diseño calculado desde una **copia** del estado de la ventana (`Copy`, `BuildInputs`, `BuildDesign` y `BuildFrom` con `FrontalSideForTest`) es igual, persistido, a `LastComputation.Design`; y `EditableStateForTest.Name` es igual a `RackName` | la costura da otro estado |
| **03** (Core) | En W y WC, sin comentarios: ninguna declaración de campo con almacenamiento para los nombres de D-PBT1-13 (2); sin declaración de `SideAOnly`, sin `LoadCompositeFromDesign` y sin `ParkDormantTail(`; `CompositeHasBlocking` contiene `PushBackEditorOutputGate.IsBlocked(`; `LoadFromModel` contiene `editable.AdoptCarriers(inputs, catalog);` como primera sentencia de su `try`; W contiene `editable.BuildInputs(assembler)` y `EditableStateForTest => editable;` | la base declara los campos y no delega |

### 5.5 El censo control → prueba actualizado

| Control | Prueba que fija su efecto en la salida | Prueba de que el estado lo lleva (PB-T1c) |
|---|---|---|
| `DepthBox`, `ToleranceBox`, `PostPeralteBox`, `BeamDepthBox`, `AnnotationScaleBox` | CT-PB0-19 (SUS-03), CT-PBTC-01 a 03 y CT-PBT1A-07 | CT-PBT1C-02 |
| `WeightUnitBox`, `PalletsDeepBox` | CT-PB0-17, CT-PB0-04 y **CT-PBT1A-02** | CT-PBT1C-02 |
| `PostBox` | CT-PB0-17 (c-01, c-02) y **CT-PBT1B-03** | CT-PBT1C-02 |
| `NumberFrontsCheck`, `NumberLevelsCheck`, `DrawRackNameCheck`, `DimensionsBox`, las tres de cotas | CT-PB0-17, `PushBackDimensionViewsWindowTests` y **CT-PBT1A-01** | CT-PBT1C-02 |
| `PositionsBox`, `LevelsBox`, `FondosBox`, `DepthStartBox` | **CT-PBT1A-03** | CT-PBT1C-02 |
| `FirstLevelHeightBox` y las cuatro de la celda | CT-PB0-19, CT-PBTC-01 a 03 y CT-PBT1A-07 | CT-PBT1C-02 |
| `CellInOutBeamBox`, `CellInOutPeralteBox`, `CellIntermediateBeamBox` | CT-PB0-17 | CT-PBT1C-02 |
| `CellIntermediatePeralteBox`, `CellFondoOverrideBox`, `CellDrawPalletCheck` | **CT-PBT1A-04** | CT-PBT1C-02 |
| `CellBeamLengthOverrideBox` | CT-PB0-19, CT-PBTC2-04 y **CT-PBT1A-10** | CT-PBT1C-02 |
| `RearPeralteBox` | CT-PB0-17 (c-03) | CT-PBT1C-02 |
| `GapBox` | CT-PB0-20 y CT-PBTC2-01 | CT-PBT1C-02 |
| `CentralSeparatorCheck`, `StructureOverrideBox`, `CellTopologyBox`, `RunDirectionBox`, `TopologyScopeBox`, `FrontCountBox` | **CT-PBT1A-05** | CT-PBT1C-02 |
| `NameBox` | CT-PB0-11 | CT-PBT1C-02 (`Name` = `RackName`) |
| Las cajas avanzadas | CT-PB0-05 (SUS-04), CT-PBTC-14 y CT-PBTC2-03 | CT-PBT1C-02 |
| `ModuleLengthBox`, `DerivedPostLineHeightBox` | CT-PB0-03, CT-PB0-04, CT-PBTC2-02 y **CT-PBT1A-06** | CT-PBT1C-02 |
| `SideBPresentCheck`, `SideSelectorBox`, `FrontalSideBox` | CT-PB0-06 a 09, **CT-PBT1A-09** y CT-PBT1A-14 (F-02, F-03) | CT-PBT1C-02 |

**Huecos que quedan:** ninguno de los 20 de D-5.

### 5.6 Trazabilidad

| Obligación | CT |
|---|---|
| «module state preserved» | CT-PBT1B-06, 08 y 09; CT-PBT1A-06; CT-PBT1C-02 |
| «parked queue preserved» | CT-PBT1B-07 (parte 2), 09 y 02 (X-01); CT-PBT1A-14 (G-4) |
| «A/B preserved» | CT-PBT1B-02 y 09; CT-PBT1A-03, 08 y 09 |
| «no aliasing» | CT-PBT1B-07 (partes 1 a 3), 08 y 13; L-1 del §8 |
| «no WPF control becomes source of truth» | CT-PBT1C-02; CT-PBT1B-03; §5.5 |
| «state round-trip lossless for supported classic racks» | CT-PBT1B-02 y 04 |
| «classic RACKEDITAR still works through the extracted state» | CT-PBT1C-01; §5.1 y CT-PBT1A-14 sobre PB-T1c |
| OB-05.1 | CT-PBT1B-11 |
| El criterio de bloqueo para PB-T3 | CT-PBT1B-12 y 01; CT-PBT1C-03 |
| c-02; `pb-21-cola`; T-2; D-3 | CT-PBT1B-03, 04 y 05; CT-PBT1A-13 |
| D-5; M-03 de PB-TC y de PB-TC2; M-04; M-07; CT-16; M-02 de PB-TC | CT-PBT1A-01 a 12 |
| OB-03.1 | CT-PBT1B-04 y el filtro `Workspace` |
| OB-T1B.3 | §4; INV-PBT1x-07 |

## 6. Los contratos propuestos

### 6.0 Común a las tres subtareas

**Identidad:** `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; attempts 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true. Evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Anclajes:** antes de emitir cada subtarea, la sesión vuelve a medir sobre su `BaseSha`, con las órdenes del registro, y lo deja escrito en el commit de emisión:
- PB-T1a: los quince textos de la tabla de mutantes y las zonas [TC2];
- PB-T1b: `git diff <cierre de PB-T1a> -- src` vacío;
- PB-T1c: `git diff <cierre de PB-T1b> -- src/RackCad.UI` vacío y los anclajes de D-PBT1-13.

**La comprobación acotada (M-12):**
- **Quién:** un revisor independiente, en solo lectura, con el método de IRT1-1.
- **Sobre qué:** este paquete (v2.1), la sección de la subtarea y la salida de sus anclajes. La de la v2 ya está hecha (`independent-review-v2-check.md`).
- **Qué comprueba:** que los anclajes se sostienen; que ningún hecho nuevo cambia un CT o el diseño; que no aparece ninguna decisión material nueva; y, para la v2, que los 23 hallazgos de IRT1-1 están resueltos.
- **Resultado:** EMITIBLE o NO EMITIBLE, con hallazgos, custodiado en `F4B-DRAFTS/pbt1/` (`bounded-check-v2.md`, `bounded-check-pbt1b.md` y `bounded-check-pbt1c.md`).

Si un anclaje no se sostiene de un modo que cambia un CT o el diseño, la parte afectada se rehace y vuelve a una comprobación acotada.

**Custodia (M-12):**
- en el commit de emisión de **PB-T1a**: este paquete (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`), el registro, los tres scripts, la respuesta del Owner a L-1, IRT1-1 y su comprobación acotada, y el contrato y su generador en `docs/automation/evidence/I-64-pilot/F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/`;
- en los de **PB-T1b** y **PB-T1c**: su contrato y su generador en su `coordinator/`, la salida de sus anclajes y su comprobación acotada;
- en `f4b-dispositions.md`: la división (D-PBT1-01), la respuesta a L-1, cada emisión y los arrastres del §10.

**`ForbiddenWriteScope`:** con el método de [PKC-C], calculado por el generador de cada contrato con `assert` (medición J):
- `src/`: 59 en PB-T1a y PB-T1c (la forma de PB-TC2, con WX), 117 en PB-T1b;
- `tests/`: en PB-T1a, `tests/RackCad.Tests/` entero; en PB-T1b y PB-T1c, la lista curada de PB-T0 (18 entradas; medición H-12) más las clases de Core de PB-T0 y, en PB-T1c, las de PB-T1b. En `tests/RackCad.UI.Tests/`, la lista curada de [PKC2] (51 archivos y `Workspace/`) más las clases de PB-TC2 y, en PB-T1b y PB-T1c, las de las subtareas anteriores;
- la raíz, las 17 entradas de [PKC2].

**`StopConditions` estándar** (17), copiadas de [PKC2] §6.7: S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 = «config.toml no longer matches the accepted Snapshot in force (today Snapshot I, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)».

**Las de [DP], literales, en cada contrato** (x = A, B o C):
- **PBT1x-C08** ([DP], pregunta 2): una sola replanificación, y solo si el rechazo de la planificación es **solo** por la comparación literal de las colecciones, con el prompt reforzado y sin gastar intento; un segundo rechazo de fidelidad, o cualquier otro rechazo (A1-A8 u otra causa), es STOP y vuelve al Owner.
- **PBT1x-C11** ([DP], pregunta 1). Un control que no da CONTROL PASS se conserva tal cual, y solo hay dos salidas sin volver al Owner:
  - **nc2 con la firma exacta de PD-F4-01.** El texto literal de su fuente (`f4-dispositions.md:1103-1106`):

    > «- Scope en fail sobre la ruta quitada;
    > - Contract en fail solo por la cláusula de exactitud;
    > - `EXECUTION_BLOCKED`/STOP.»

    Y su aplicación literal (`:1540`): «ninguna otra comprobación distinta de la verificación real».

    **Qué cuenta como la cláusula de exactitud:** forma parte de la firma que la evidencia de Contract cite el contraste del `AllowedWriteScope` estrechado con el contrato o con `byte-fidelity-check.json` (`f4-dispositions.md:520`, `:562`). No forma parte que Contract cite cualquier otra discrepancia.

    → El sustituto **`PBT1x-NC2-EXACT-WHITELIST-01`**: copia de `F4T1B-NC2-EXACT-WHITELIST-01` (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION-nc2-substitute/1/R20261114T235423Z-083e/nc2_exact_whitelist_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc2 = SATISFIED_UNDER_PBT1x_EXACT_WHITELIST_SUBSTITUTE`.
  - **nc1 con la firma exacta de PD-F4-04.** El texto literal de su fuente (`owner-f4t3-authorization.md:255-265`):

    > «* false CurrentSha correctly detected;
    > * Identity = fail;
    > * Handoff also fails first only because the handoff itself contains the false
    > SHA;
    > * outcome BLOCKED / STOP;
    > * all other intended checks match;»

    Su aplicación en F4, transcrita literal (`f4-dispositions.md:1657-1660`):

    > «- el CurrentSha falso se detecta, e Identity está en fail con una evidencia que nombra el SHA falso y el HEAD;
    > - Handoff falla primero solo porque la entrega contiene el SHA falso;
    > - `EXECUTION_BLOCKED` / STOP con S-04;
    > - solo difieren consumidores del CurrentSha: Handoff, Identity y Ci, con Remote en pass, un subconjunto de la cascada de F4-T2C.»

    Por tanto, Ci en fail solo como consumidor del `CurrentSha` falso forma parte de la firma. Ci en fail por cualquier otra causa no.

    → El sustituto **`PBT1x-NC1-IDENTITY-SIGNAL-01`**: copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01` (`docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR-nc1-substitute/1/R20261113T215949Z-935f/nc1_identity_signal_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc1 = SATISFIED_UNDER_PBT1x_NC1_IDENTITY_SIGNAL_SUBSTITUTE`.
  - **Cualquier otra firma o variación vuelve al Owner antes de cualquier sustituto:** PD-F4-05 y PD-F4-06; nc2 con Ci o Trailer en fail además; cualquier nc1 que no reproduzca la firma literal, **como el de PB-TC** (R-T1-08); cualquier fallo de nc3. La disposición «Aceptarlo solo esta vez» de nc1 en PB-TC (F4B-E08) **no** se extiende a PB-T1.
  - **Cómo se decide la coincidencia:** nunca por el nombre; comprobación por comprobación (el conjunto en fail, la primera, el `Outcome`, la `StopClass` y el texto de la evidencia). Las únicas diferencias admitidas: en PD-F4-01, Scope (en fail sobre la ruta quitada) y Contract (solo por el alcance); en PD-F4-04, Identity, Handoff (la primera) y Ci (solo como consumidor del `CurrentSha` falso); en las dos, `EXECUTION_BLOCKED` / STOP. La sesión registra la tabla en `f4b-dispositions.md`.
- **PBT1x-C12:** una corrección sería el intento 3. Vuelve al Owner antes de emitirse (D-PB-10).

**El RED de la cadena en una corrección (M-11, §16.8):** una corrección que toque `RT` ∪ `ChainRedFiles` exige RED otra vez. En PB-T1a, su commit RED vuelve a aplicar **los quince mutantes** sobre la punta y su GREEN los revierte; en PB-T1b, el RED vuelve a poner el esqueleto; en PB-T1c, la costura inerte.

**La lectura de `Ci` `RedPart` con Core en `success` (M-11):** el RED de PB-T1a deja Core en `success` y UI en `failure`. Se acepta con `RedPart = pass`, como F4-T3B1 ([PKC]:466) y F4-T4A2 en su intento 2 (`F4-DRAFTS/f4-dispositions.md:1917-1928`, F4-E56). En PB-T1b y PB-T1c, Core falla en el RED.

**`EligibleCells`:** las de [PKC2]: Worker `anthropic/subagent/claude-sonnet-5-5`, effort `high`; Controller `openai/process/gpt-6-luna`, effort `high`. `model-catalog.md:51` y `:97` las verifican el 2026-09-30, y con `:14` vencen el **2026-12-29** ([PKC2]:655). nc4 en la primera delegación de cada cadena; nc1 a nc3 tras la verificación.

**Invariantes comunes** (cada contrato los numera INV-PBT1x-07 a 12):
- **07. Las redes:** T1B, PB-T0, PB-TC, PB-TC2, Cantilever, las guardas del §4, el censo del piloto y las subtareas anteriores, en verde; ninguna prueba existente se edita, se salta, se reactiva ni se borra.
- **08. Las pruebas de UI:** las clases nuevas en `EditorDiscardPromptCollection`; `Ventana()`, la carga dentro del `try` y `Cerrar` en `finally`; ninguna ventana mostrada salvo CT-PBT1A-11, fuera de pantalla; el aviso de descarte sustituido con su sufijo; sin teclado ni foco real; prohibidos un `MessageBox` real, el BOM, «Guardar en biblioteca», el configurador real e «Insertar»; ninguna aserción dentro de una costura; ningún fixture nuevo en `Fixtures/I64`.
- **09. Determinismo:** el GUID fijo, la cultura invariante, «R», el orden ordinal; ni hora ni aleatoriedad.
- **10. Las corridas locales:** solo los filtros del contrato en UI, **nunca la suite completa de UI en local**; la de Core, sí; sin copias de árboles (MAX_PATH); las corridas temporales terminan en `Assert.Fail(string.Join("\n", lineas))`, no entran en ningún commit y guardan TRX y log como artefactos; el perfil no cambia (INV-PBT0-06).
- **11. La evidencia:** rutas relativas comprobadas en `worker-handoff.json`; `TreeSha` real (`git write-tree`) o null; el Worker no escribe en `docs/`; una sustitución se llamaría `SUS3A-0n`, `SUS3B-0n` o `SUS3C-0n` y es C02.
- **12. Las reglas de F4B y sin AutoCAD.**

**Las propias comunes** (con el prefijo de cada contrato):
- **C01:** hacerlo bien exigiría escribir fuera del `AllowedWriteScope`.
- **C02:** habría que editar, saltar, reactivar o borrar una prueba o un ayudante existente, o tocar `InternalsVisibleTo`, los paquetes, un `.csproj` o `Fixtures/I64`. Una guarda del §4, el censo del piloto o una prueba de PB-T0, PB-TC o PB-TC2 que no queda en verde **vuelve al Owner** (OB-T1B.3).
- **C03:** aparece un cambio observable de RACKEDITAR frente a la base. STOP y vuelta al Owner (D-16, OB-RE.1).
- **C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el BOM, «Guardar en biblioteca», «Insertar», el teclado o el foco real, o escribir en el perfil o en el disco.
- **C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce y no es atribuible a ellas.
- **C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño; o una línea esperada no se cumple en el GREEN. El Worker no ajusta lo esperado.
- **C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **C10:** hacerlo bien exigiría corregir algo fuera del alcance: una pérdida de T-2, M-07, `pb-21-cola`, el modo WORKSPACE, el piloto, D-PB-07 o una corrección de PB-TC o PB-TC2.

### 6.1 Contrato de PB-T1a — `F4B-PBT1A-LOSSLESS-STATE-NET`

- **Objective** (ASCII): «Fijar con pruebas de caracterizacion nuevas, sin cambiar el producto, la red de la ventana clasica de Push Back que PB-T1 necesita antes de mover su estado: los huecos de D-5, M-03 de PB-TC y de PB-TC2, M-04, M-07, la observacion de CT-16, M-02 de PB-TC y D-3, y las huellas de salida del corpus extendido (con F-03) por los guiones G-1 a G-6, segun el paquete del contrato de PB-T1 v2 y la lista cerrada CT-PBT1A-01..14. El RED son las pruebas nuevas y los mutantes de la tabla cerrada MU-T1A-01..14 en W y WC; el GREEN revierte los mutantes. Ninguna linea de src cambia entre BaseSha y CurrentSha; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (9): W y WC (solo los mutantes del RED); los siete archivos nuevos `tests/RackCad.UI.Tests/PushBackStateNet{Support,RackControlsTests,CellAndFrontTests,CompositeTests,ModuleAndSafetyTests,ActionsTests,FingerprintTests}.cs`.
- **`ForbiddenWriteScope`:** 59 de `src/` y lo común.
- **`RequiredTests`:**

  | Proyecto | Filtro (`FullyQualifiedName~RackCad.<proyecto>.`…) | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackStateNet` | **14** | true |
  | UI | `PushBackClassicCharacterization` | 55 | false |
  | UI | `PushBackFullEditorCharacterization` | 16 | false |
  | UI | `PushBackDeclaredCorrectionRoundingTests`, `…DimensionStyleTests`, `…PilotParityTests`, `…SideBNoticeTests`, `…StandardRestoreTests` (uno por clase) | 3, 2, 2, 2, 1 | false |
  | UI | `PushBackDeclaredCorrection2` | 4 | false |
  | UI | `Cantilever` | 67 | false |
  | UI | `PushBackCompositeWindowTests`, `PushBackCompositeLifecycleWiringTests`, `PushBackCompositeLoadExistingModulesTests`, `PushBackBothModeCellBeamOverrideTests`, `PushBackModuleEditorWindowTests`, `PushBackAdvancedRackParametersWindowTests` | 52, 7, 3, 10, 13, 9 | false |
  | Core | `PushBackClassicCharacterization` | 10 | false |
  | Core | `PushBackFullEditorCharacterization` | 3 | false |
  | Core | `PushBackModuleEditorCharacterizationTests` | 15 | false |

  En el RED, los filtros sin `ExpectRed` pueden fallar **solo** por los mutantes, con la atribución T-8; en el GREEN, todos en verde.
- **Invariantes propios:**
  - **INV-PBT1A-01. Alcance:** `git diff --name-only BaseSha..CurrentSha` = los siete archivos nuevos; `git diff BaseSha..CurrentSha -- src` vacío.
  - **INV-PBT1A-02. RED y GREEN:** el RED son los siete archivos y los quince mutantes, juntos; el GREEN es `git restore --source=BaseSha` de W y WC y no toca `tests/`. Criterio del RED sobre el filtro con `ExpectRed`: todos `Failed`; `Message` sin el prefijo de tipo de [PKC-C]; la primera línea del `StackTrace` bajo `tests/`; ninguno `NotExecuted` ni `Skipped`; `Failed == Selected >= 14`.
  - **INV-PBT1A-03. Cada mutante solo:** aplicado aislado sobre la `BaseSha`, hace fallar por aserción el CT de su fila; la entrega lo transcribe con el TRX de cada corrida temporal.
  - **INV-PBT1A-04. Las líneas esperadas** salen del paso 1, transcrito en la entrega antes del RED.
  - **INV-PBT1A-05. CT-14:** el corpus, las recetas y los guiones del §5.0 y del §5.2, en ese orden.
  - **INV-PBT1A-06. Las mediciones guardadas** (M-06 de PB-T0), como artefactos con ruta relativa.
  - **INV-PBT1A-13. La declaración de autoridad (I-08; N-M-07: era PBT1A-C15 y no es una parada):** el Worker lista en `UnexpectedFindings` cada línea fijada que muestre un valor del diseño que cambia sin edición del usuario (una pérdida de clase c, d o nueva) o un control releído sin edición que cambia el estado o la salida, **distinta** de c-01, c-02 y M-07. La sesión la lleva al Owner antes de emitir PB-T1b (precondición 5). Con la numeración común (07 a 12), este es el 13.
- **`StopConditions`:** las 17 estándar, C08, C11, C12, las comunes, y:
  - **PBT1A-C04 (I-06):** un mutante de la tabla, aislado, no hace fallar por aserción el CT de su fila; o un CT nuevo falla en la `BaseSha` sin mutantes; o falla por excepción. El Worker no cambia el CT ni el mutante.
- **`ExpectedEvidence`:** antes del Worker, el contrato, la delegación, la comparación byte a byte, A1-A8 y nc4; el paso 1 con su TRX y su log; el RED y su CI (ui-tests en `failure`; tests, build-ui y build-plugin en `success`; sin cuelgue); la tabla T-8; los quince mutantes aislados; el GREEN y su CI 4/4; en local, el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED y el GREEN y tres corridas en verde del filtro con `ExpectRed` sobre el GREEN; la entrega con el mapa CT → método, las rutas relativas comprobadas y la lista de INV-PBT1A-13; la verificación y nc1 a nc3.
- **Línea base de la CI:** la del cierre de PB-TC2 (previsión de su §6.9: UI 2 422 resultados, 21 `Skipped`, 2 401 `Passed`; Core 19 070). **GREEN de PB-T1a:** UI 2 436, 21 `Skipped`, 2 415 `Passed`; Core 19 070.

### 6.2 Contrato de PB-T1b — `F4B-PBT1B-LOSSLESS-STATE-TYPE`

- **Objective** (ASCII): «Crear en Application el estado editable de Push Back sin perdidas fuera de la ventana (PushBackEditableState), con las cargas de LoadNew, LoadExisting, LoadDesignForNew y Restaurar partidas en la de Application y AdoptCarriers, la salida BuildInputs, el copiador y el comparador completos con modulos, cola aparcada y A/B (con la excepcion declarada de L-1), el mapa de propiedad, la regla explicita de c-02, la exclusion de pb-21-cola y el criterio de bloqueo de salida extraido, segun el paquete del contrato de PB-T1 v2 y la lista cerrada CT-PBT1B-01..13. El RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion; el GREEN implementa solo Application. La ventana, el piloto, el Plugin y el XAML no cambian; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (11): ES y RMES; los cuatro nuevos de `src/RackCad.Application/Systems/PushBack/` (`PushBackEditableState.cs`, `PushBackEditableStateComparer.cs`, `PushBackEditableStateOwnership.cs`, `PushBackEditorOutputGate.cs`); los cuatro nuevos de `tests/RackCad.Tests/` (`PushBackEditableStateLoadTests.cs`, `PushBackEditableStateCopyTests.cs`, `PushBackEditableStateOwnershipTests.cs`, `PushBackEditorOutputGateTests.cs`) y `tests/RackCad.UI.Tests/PushBackEditableStateParityTests.cs`.
- **`ForbiddenWriteScope`:** 117 de `src/` (CS incluido) y lo común.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackEditableState` | **11** | true |
  | Core | `PushBackEditorOutputGate` | **1** | true |
  | UI | `PushBackEditableStateParityTests` | **1** | true |
  | Core | `Workspace` | 3 271 | false |
  | Core | `PushBackOutputGateTests`, `PushBackBlankFrontAuthorityTests`, `PushBackModuleEditorCharacterizationTests` | 8, 9, 15 | false |
  | Core | `RackModuleEditSessionTests`, `NamespaceFolderGuardTests`, `HeaderBatchContractTests` (M-10) | 25, 7, 17 | false |
  | Core y UI | `PushBackStateNet` (14), T1B, PB-T0, PB-TC, PB-TC2 y Cantilever | como en §6.1 | false |

- **Invariantes propios:**
  - **INV-PBT1B-01. Alcance:** el diff dentro de las 11 rutas; en `src/`, solo las seis de producto. `git diff BaseSha..CurrentSha -- src/RackCad.UI src/RackCad.Plugin src/RackCad.Domain src/RackCad.Application/Systems/PushBack/Pilot src/RackCad.Application/Workspace src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs` vacío.
  - **INV-PBT1B-02. RED y GREEN:** el RED es el esqueleto exacto del §5.3 y las cinco pruebas; el GREEN solo toca `src/`. Criterio del RED sobre cada filtro con `ExpectRed`: todos `Failed` por aserción (como INV-PBT1A-02), con `Failed == Selected`.
  - **INV-PBT1B-03. La API:** los miembros, las firmas y los valores iniciales son exactamente los de D-PBT1-02 a D-PBT1-11; ningún otro miembro público nuevo. Los ayudantes privados nuevos son solo `LoadComposite` en `PushBackEditableState`, `CopyLiteral` en la sesión y `CopyModule` en el commit, más los privados del comparador que copian literal los del piloto (PC:64-83, PC:134-350).
  - **INV-PBT1B-04. Ningún campo de instancia nuevo** en CS, ES, `PushBackEditorInputs`, `PalletSpecification`, `DynamicAnnotationOptions`, los tipos de seguridad ni RMES (el censo del piloto en verde y `git diff -U0` sin declaraciones de campo de instancia añadidas en ES ni en RMES). Ningún campo estático nuevo tampoco: el `RackFrameProjectStore` de `RackModuleCommit` y el de `PushBackEditableState` son locales al método (N-M-06).
  - **INV-PBT1B-04b.** Cada archivo nuevo de `src/` declara un solo `namespace RackCad.Application.Systems.PushBack` de bloque; las pruebas, `namespace RackCad.Tests` o `RackCad.UI.Tests`.
  - **INV-PBT1B-05 (M-06), solo sobre `src/`:** ningún archivo nuevo de `src/` contiene `.RequestUpdate(` ni nombra `RackCad.UI`, `System.Windows`, `Autodesk`, `Workspace`, `Pilot` ni `RackSystemKind`.
  - **INV-PBT1B-06. Paso 1, sobre `BaseSha` y antes del RED** (una corrida temporal de UI, fuera de todo commit, con `Assert.Fail`): con `pb-01-base` y su `HeaderPostCatalogId` cambiado, la línea `<caso> PostBox.SelectedId=<v> salida=<HeaderPostCatalogId de LastComputation.Design>`. **Las filas esperadas** (M-05), con P = `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA`:

    | Caso | `HeaderPostCatalogId` | Línea esperada |
    |---|---|---|
    | (a) | P | `a PostBox.SelectedId=P salida=P` |
    | (b) | P en minúsculas | `b PostBox.SelectedId=<null> salida=P` (rama ordinal) o `b PostBox.SelectedId=P salida=P` (rama sin mayúsculas) |
    | (c) | `PB0_POSTE_INEXISTENTE` | `c PostBox.SelectedId=<null> salida=P` |
    | (d) | en blanco | `d PostBox.SelectedId=P salida=P` |
    | (e) | tres espacios | `e PostBox.SelectedId=P salida=P` |

    Se transcribe en la entrega. **La fila (b) fija la rama de la regla de c-02** (D-PBT1-09, N-M-03): con `<null>`, la rama ordinal; con `P`, la rama sin mayúsculas. El Worker declara cuál midió y la aplica; cualquier otro valor de `PostBox.SelectedId` en (b) es PBT1B-C14.
- **`StopConditions` propias:**
  - **PBT1B-C04 (I-06):** un método de un filtro con `ExpectRed` pasa sobre el esqueleto, o falla por excepción y no por aserción. Es un defecto del contrato.
  - **PBT1B-C13:** la implementación necesitaría un campo nuevo en un tipo del censo del piloto, tocar CS, o cambiar el comportamiento de un método existente de ES, CS, DA, CA o RMES.
  - **PBT1B-C14:** en el paso 1, alguna `salida` de (a) a (e) no es P, o una fila no es ninguna de sus líneas esperadas (en (b), ninguna de las dos ramas). STOP antes del RED: la regla de c-02 vuelve a la sesión.
  - **PBT1B-C15:** CT-PBT1B-01 o 02 da una diferencia entre el estado y la ventana de hoy, o una pérdida en un rack soportado, que no sale del esqueleto. STOP: es un hecho nuevo y vuelve al Owner por OB-06.5.
- **`ExpectedEvidence`:** como §6.1, con el paso 1 y, en vez de los mutantes, el diff del esqueleto y la tabla «CT → la aserción que falla en el RED».
- **Línea base de la CI:** la del cierre de PB-T1a. **GREEN:** Core 19 070 + 12; UI la de PB-T1a + 1.

### 6.3 Contrato de PB-T1c — `F4B-PBT1C-LOSSLESS-STATE-WINDOW`

- **Objective** (ASCII): «Hacer que la ventana clasica de Push Back guarde su estado editable en el PushBackEditableState de PB-T1b, en modo CLASSIC y sin cambio observable, con el cambio minimo de D-PBT1-13 (reenvios con el mismo nombre, la carga de Application delegada, AdoptCarriers dentro de LoadFromModel, ReadInputs que escribe en el estado, el criterio de bloqueo extraido y la costura EditableStateForTest), segun el paquete del contrato de PB-T1 v2 y la lista cerrada CT-PBT1C-01..03. El RED es la costura inerte y las pruebas nuevas; el GREEN toca solo W y WC. Los constructores, los cuatro sitios, el XAML, el Plugin, el piloto y Application no cambian; el modo Workspace queda para PB-T2; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (4): W, WC, `tests/RackCad.UI.Tests/PushBackWindowOnStateTests.cs` y `tests/RackCad.Tests/PushBackWindowOnStateSourceGuardTests.cs`.
- **`ForbiddenWriteScope`:** 59 de `src/` y lo común.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | UI | `PushBackWindowOnStateTests` | **2** | true |
  | Core | `PushBackWindowOnStateSourceGuardTests` | **1** | true |
  | UI | `PushBack` (todas las clases `PushBack*`) | el medido en la emisión (538 + 14 + 1 + 4) | false |
  | UI | `BlankFront`, `WindowCensusGuardTests`, `RichEditor`, `Workspace`, `PushBackRichPreviewTests` | 33, 8, 25, 547, el medido | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackClassicCharacterization`, `PushBackFullEditorCharacterization`, `PushBackModuleEditorCharacterizationTests`, `I55DeferredProductFixCharacterizationTests`, `NamespaceFolderGuardTests`, `Workspace` | 11, 1, 10, 3, 15, 3, 7, 3 271 | false |
  | UI | `Cantilever` | 67 | false |

- **Invariantes propios:**
  - **INV-PBT1C-01. Alcance:** dentro de las 4 rutas, y en `src/` solo W y WC; `git diff BaseSha..CurrentSha -- src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Editor src/RackCad.UI/Controls` vacío.
  - **INV-PBT1C-02. RED y GREEN:** el RED son las dos pruebas y, en W, solo la línea de la costura inerte; el GREEN solo W y WC; criterio del RED como INV-PBT1A-02, con `Failed == Selected`.
  - **INV-PBT1C-03. Dónde cambia, sobre el diff:** cada hunk de `git diff -U0 BaseSha..CurrentSha -- src` cae en un miembro de esta lista, por su rango en `BaseSha` (la definición de «tocar un miembro» de [PKC2] INV-PBTC2-03): los campos de D-PBT1-13 (1) y (2); `LoadNew`, `LoadDesignForNew`, `LoadExisting`, `SideAOnly` (retirado), `LoadFromModel`, `ReadInputs`, `Name_Changed`, `SelectedDimensionViews` (retirado), `Restore_Click`, `InsertBatch_Click`, `RequestDraw`, `SaveLibrary_Click`; en WC, los dos `LoadCompositeFromDesign` (retirados), `CompositeDiagnostics`, `CompositeStatusOr` y `CompositeHasBlocking`; y las declaraciones nuevas `SyncName` y `EditableStateForTest`.
  - **INV-PBT1C-04. Los literales de T-SG:** las cinco sentencias de CT-22, la de S-9, la de S-7 y las cuatro de CT-23 siguen en W, byte a byte y en su orden; `SyncName();` va inmediatamente después de cada `SetName` y de cada `Adopt`.
  - **INV-PBT1C-05. Las cuentas, sobre `CurrentSha`:** W con una sola `.RequestUpdate(`; ninguna línea añadida de W o WC contiene `forceRebuild`, `KeyDown`, `PreviewKeyDown`, `InputBindings`, `KeyBinding`, `TabIndex`, `IsTabStop`, `KeyboardNavigation`, `.Focus(` ni `DialogResult`; W sigue conteniendo los literales de I-55, Fact3 y el desviador; ninguna línea con `SetNumber(` se añade ni se quita; las siembras de `LoadFromModel` de D-PBT1-13 (4) no cambian; `editable.AdoptCarriers(inputs, catalog);` es la primera sentencia del `try` de `LoadFromModel`; `UpdateCompositeEnabled();` va en la línea siguiente a `LoadDesignEditor` y a `RestoreEditor`.
  - **INV-PBT1C-06. Las costuras:** las 40 de la medición H, con el mismo nombre y tipo; solo se añade `EditableStateForTest`.
  - **INV-PBT1C-07b. Las huellas:** CT-PBT1A-14, con F-03, en verde sin tocarla, en local y en la CI.
- **`StopConditions` propias:**
  - **PBT1C-C04 (I-06):** un método de un filtro con `ExpectRed` pasa sobre la costura inerte, o falla por excepción.
  - **PBT1C-C13:** mantener sin cambio observable un gesto exigiría tocar un miembro fuera de INV-PBT1C-03 o cambiar el orden de una carga.
  - **PBT1C-C14:** una huella de CT-PBT1A-14 cambia. STOP (C03); el Worker no ajusta ni la huella ni el producto.
- **`ExpectedEvidence`:** como §6.1, con la salida de INV-PBT1C-03 a 05 sobre el diff y, en local, la clase de huellas tres veces en verde sobre el GREEN.
- **Línea base de la CI:** la del cierre de PB-T1b. **GREEN:** Core + 1; UI + 2.

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-T1-01 | **Sin latitud para el Worker** | Los contratos no dan latitud; las recetas, el ayudante `Pasada`, la tabla del censo y la API del mapa están en el paquete |
| R-T1-02 | **Las líneas esperadas salen de leer el código** en PB-T1b y PB-T1c | Oráculos existentes (ventana de hoy, CT-PB0-01, CT-PB0-17, CT-PBTC-06, `LegacyDeltas`) y líneas literales donde el código no basta (CT-12); PBT1B-C15 |
| R-T1-03 | **Evidencia con rutas relativas y `TreeSha` real** | INV-PBT1x-11 |
| R-T1-04 | **Corridas temporales con `Assert.Fail`** | INV-PBT1x-10 |
| R-T1-05 | **Las ventanas con el aviso de descarte sustituido** | INV-PBT1x-08 |
| R-T1-06 | **Nunca la suite completa de UI en local** | INV-PBT1x-10 |
| R-T1-07 | **MAX_PATH** | Sin copias de árboles; `TaskId` de 28 a 31 caracteres |
| R-T1-08 | **La variación de nc1 de PB-TC** | Vuelve al Owner por C11, sin sustituto, en cada cadena |
| R-T1-09 | **Los nombres de las sustituciones** | `SUS3A-0n`, `SUS3B-0n` y `SUS3C-0n`; se esperan 0 |
| R-T1-10 | **Tres cadenas antes del 2026-12-29** | Vigencia de las celdas en cada emisión y delegación (A7) |
| R-T1-11 | **El censo del piloto** | INV-PBT1B-04; PBT1B-C13 |
| R-T1-12 | **La línea base compartida** podría mutarse | L-1 (a) con sus condiciones; CT-PBT1B-08; A-2 |
| R-T1-13 | **La pasada reentrante de `UpdateCompositeEnabled()`** (HT1-10b) | Los portadores se adoptan donde hoy (D-PBT1-13 (4)); F-03 en CT-PBT1A-14 lo ejerce; INV-PBT1C-05; PBT1C-C13 |
| R-T1-14 | **PB-TC2 no ha cerrado** | Las zonas [TC2]; la precondición 1; la nota del §8.5 |
| R-T1-15 | **Cómo cuenta el Controller un `Skipped`** | `MinSelected` sobre los que pasan |
| R-T1-16 | **El RED de PB-T1a rompe pruebas existentes** | La tabla T-8; Core en `success` (§6.0) |
| R-T1-17 | **El modelo paralelo interino (M-12):** entre el cierre de PB-T1b y el de PB-T1c hay **tres** copias de la carga compuesta (WC, el estado y el piloto), contra ADR-0029 D12 | Se declara como interino. PB-T1c retira la de la ventana; PB-T4 retira la del piloto (A-5) |
| R-T1-18 | **PB-T1c no cierra** (M-12) | El tipo de PB-T1b queda sin uso de producto y sin cambio observable; ante un intento 3, el Owner decide (C12); nada se integra en `main` (A64-PV1-21) |

## 8. Las decisiones del Owner

### 8.1 La decisión abierta: L-1, «no aliasing» y la línea base resuelta

**El caso.** El copiador comparte por referencia dos objetos de cada lado: la **línea base resuelta** (`workingBaseline`, un `PushBackSystem` mutable) y el **informe de la última reconciliación** (`LastModuleReconciliation`). Todo lo demás se copia en profundidad (D-PBT1-05). La revisión IRT1-1 la considera **material**: es una excepción a la letra de un requisito duro.

**Lo medido (M-1, M-2):**
- El mismo objeto es a la vez la línea base, `lastComputation.System`, `session.System` y `lastCompositeSystem`.
- Sus consumidores son el armado siguiente, la sesión de módulos, `ParkTailOf`, el panel compuesto, el criterio de bloqueo, los módulos del panel, la vista previa, el lote de vistas y las peticiones de inserción. **Todos leen.**
- **Solo el Plugin muta:** escribe `Name` **después** de cerrar la ventana, cuando su estado ya se descarta, en `RackPushBackCommands.cs:98` y `:190` y en el lote de vistas del menú (`RackMenuCommands.cs:135`). `CopyStructureSystem` no lleva `Name` al diseño.
- La cola que aparca `ParkTailOf` apunta a configuraciones y líneas de la línea base; el copiador la copia en profundidad.

**Las opciones:**
- **(a) Aceptar la excepción declarada (recomendada; es el diseño vigente de esta v2.1).**
  - **Qué es:** la copia comparte, por referencia, solo la línea base y el informe de reconciliación de cada lado.
  - **Sus condiciones:**
    1. la exención se limita a esos dos objetos y a lo que alcanzan;
    2. CT-PBT1B-07, parte 2, comprueba sin exención que la cola, el commit y la sesión de la copia no comparten nada mutable con el original;
    3. CT-PBT1B-08 vigila los consumidores medidos que el API alcanza;
    4. PB-T3 y PB-T4 heredan la obligación de no mutar un sistema que comparte un estado del borrador, y de mutar solo una copia, como el `Name` del Plugin (arrastre A-2).
  - **Coste:** cero, sin cambio de comportamiento.
- **(b) Una tarea previa de Domain:** la copia profunda de `PushBackSystem` y de todo lo que alcanza, entre ellas las configuraciones de cabecera, cuyas anulaciones manuales (`FrameExceptionOverride`) solo viven en memoria y hay que copiar a mano, como hace `RackFrameProjectStore.DeepCopy` (I-17; N-I-01 de la comprobación acotada de la v2). PB-T1b esperaría a esa tarea y su paquete se rehace. **Coste:** una tarea propia de Domain, con su cadena y su 1/3.
- **(c) Volver a resolver la línea base en la copia:** `resolver.Resolve(resolver.Snapshot(<línea base>))` (`PushBackResolver.cs:41`, `:387`), el modo de DA:506-529, con una prueba de equivalencia. Cumple la letra sin tocar Domain. **Coste:** la copia necesita el resolver (el catálogo); una CT y una parada más; una resolución por copia; y medir los racks A/B, donde el viaje snapshot → resolve tiene un riesgo conocido (DA:520-527, `AllowsNonNestedDepthRanges`). **El cambio exacto está en el anexo L1-C.**

**La pregunta**, con la propuesta que prepara la sesión (`F4B-DRAFTS/pbt1/owner-return-l1.md`): «Excepción declarada (Recomendado)» = (a); «Recalcularlas en la copia» = (c); «Tarea previa de Domain» = (b).

### 8.2 Ya tomadas, que este paquete aplica

- **F4B-E09, pregunta 2 (c-02):** «Como hoy, como regla explícita (Recomendado)» → D-PBT1-09 y CT-PBT1B-03.
- **F4B-E09, pregunta 3 (`pb-21-cola`):** «Dejarlo fuera de la garantía (Recomendado)» → D-PBT1-10 y CT-PBT1B-04.
- **F4B-E09, alcance:** los demás casos de T-2, tal cual → CT-PBT1B-05.
- **F4B-E09, pregunta 1:** H-31 y OWNER-1 en PB-TC2, antes de PB-T1 → la precondición 1.
- **F4B-E09, pregunta 4:** la emisión condicional → §8.4.
- **[OD]:** sin MVVM ni estética → D-PBT1-13. **[P] OB-10.1:** cada subtarea en 1/3 → D-PBT1-01.
- **La lectura de la garantía para los heredados** (M-01): «sin pérdidas» quiere decir **salvo la canonicalización T2H-07** de `LegacyDeltas`, que la ventana y el piloto ya aplican igual (CT-PBTC-06). Se declara, no se decide.

### 8.3 Lo que no es decisión (según IRT1-1)

- **L-2. La comparación de c-02.** Es «como hoy»: la regla reproduce lo que ofrece `PostBox` (D-PBT1-09). **El paso 1 elige entre dos ramas ya fijadas** según la fila (b): ordinal, o sin mayúsculas devolviendo el id de la opción (N-M-03). **PBT1B-C14 para** si alguna salida de (a) a (e) no es P o si una fila no coincide con ninguna de sus líneas esperadas. **No es material.**
- **L-3. La división y su procedimiento.** OB-10.1 y D-PB-10 la prevén, y el contenido, el orden y las dependencias de la fase no cambian. El procedimiento (una revisión para las tres y comprobaciones acotadas) es compatible con [P]:515, porque el paquete revisado contiene los tres contratos, y la comprobación acotada queda definida (§6.0). **No es material;** se informa en la emisión con D-PBT1-01.
- **M-07 de PB-TC2 reproducido** (D-PBT1-12): no es una pérdida y OB-RE.1 manda no cambiar RACKEDITAR.
- **`Name` en el estado y el piloto sin tocar:** son OB-05.1 y OB-03.1.

**Decisiones materiales nuevas distintas de L-1: ninguna.**

### 8.4 Las condiciones de emisión

1. **La respuesta del Owner a L-1** (§8.1), con la pregunta corregida por N-I-01. **L-1 sigue abierta hasta esa respuesta.**
2. **El cierre de PB-TC2** con su diseño v2 (precondición 1). Hoy está parada en su vuelta C14 (`F4B-DRAFTS/pbtc2/owner-return-c14.md`).
3. **La comprobación acotada de la v2** (§6.0): **hecha**, EMITIBLE TRAS CORRECCIONES 0/1/10, sin ninguna decisión nueva distinta de L-1. N-I-01 corregido por la sesión y los diez menores aplicados en esta v2.1, sin otra revisión.

Con las tres, PB-T1a se emite por la autorización condicional (F4B-E09, pregunta 4). PB-T1b y PB-T1c, además, con sus precondiciones 4 y 5.

### 8.5 Nota: si el Owner elige (B) en la vuelta C14 de PB-TC2

- `RackHeaderHeightBox` seguiría redondeando al pasar por ella (W:1639 de hoy), y sería **un control fuente de verdad** que PB-T1 tiene que declarar: en HT1-15, en el mapa de controles (§5.5) y como excepción en CT-PBT1C-02, que no podría exigir la igualdad para ese gesto.
- Las zonas [TC2] y los mutantes cambiarían, y la excepción la habría decidido el Owner en C14, no este paquete. **Este paquete se vuelve a redactar en esa parte antes de su comprobación acotada.**

## 9. Límites

En `pbt1-measurements.md`, apartados L y M-11. En resumen:
- **Nada se compiló ni se ejecutó.** Las líneas esperadas de PB-T1a las mide el Worker; las de PB-T1b y PB-T1c salen de oráculos existentes o son literales.
- **PB-TC2 no ha cerrado:** las zonas [TC2] y los mutantes MU-T1A-03, 04, 07a y 10 se vuelven a medir; la vuelta C14 puede cambiar el §2.4 (§8.5).
- **No medido:**
  - la no mutación de la línea base, que sigue siendo lectura aunque M-1 liste todos sus consumidores;
  - cómo compara WPF un `SelectedValue` en minúsculas (lo mide el paso 1; con el catálogo entregado no cambia la salida);
  - que la sesión y el commit se puedan copiar y comparar como se describe (por lectura de sus campos);
  - para la opción (c), que el viaje snapshot → resolve sea equivalente en los racks A/B;
  - si F-03 alcanza en la base exactamente la pasada reentrante descrita (lo fija CT-PBT1A-14 tal como sea).
- **Los recuentos son estáticos;** las `Theory` con `MemberData` no se cuentan.
- **Los conteos de la CI de `eedb069c`** se dan por iguales a los de `5597c05b`.

## 10. Arrastres que salen de PB-T1

| Id | Qué | Va a |
|---|---|---|
| A-1 | El modo WORKSPACE, la prueba UI-STA de OB-01.3, el paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE, y el dueño de los subdiálogos | PB-T2 (sin cambio) |
| A-2 | El borrador sobre `PushBackEditableState`: el copiador y el comparador de PB-T1 sustituyen a los del piloto; la composición y el clasificador de D-PB-05 con el mapa de D-PBT1-08; la barrera de PREPARE con `PushBackEditorOutputGate`; una API de asiento, si PB-T3 o PB-T4 la necesitan (I-03). **Con L-1 (a):** ningún consumidor muta un `PushBackSystem` que comparta un estado del borrador; quien necesite mutarlo (como el `Name` que escribe el Plugin en `RackPushBackCommands.cs:98`, `:190` y `RackMenuCommands.cs:135`) trabaja sobre una copia | PB-T3 y PB-T4 |
| A-3 | Quitar el rechazo A/B del piloto (OB-03.2) con PB-T1 cerrada; `DormantTailOutOfPilotReason` se queda en todo F4B | PB-T3 o PB-T4 |
| A-4 | La pregunta al Owner sobre la `ExtensionData` anidada (OB-05.2) | Antes del contrato de PB-T3 |
| A-5 | La copia de la carga del piloto (PM:150-172) | PB-T4 la retira al consumir el API |
| A-6 | M-07 (reproducido), la caja de c-02 que se ve vacía y M-01 de PB-T0 (no corregido) | `docs/ideas-futuras.md`, por la sesión |

## Anexo L1-C. El cambio exacto si el Owner elige (c)

**Se aplica sobre esta v2.1 tal cual; no hay que redactar otra cosa.**

1. **D-PBT1-05, paso 3,** pasa a: «`copy.Editor.SideX.CopyAuthorityFrom(source.Editor.SideX, resolver)`, que además de la sesión, el commit y la firma pone como línea base `source.WorkingBaseline == null ? null : resolver.Resolve(resolver.Snapshot(source.WorkingBaseline))` (`PushBackResolver.cs:387`, `:41`), y como informe de reconciliación una copia campo a campo de `source.LastModuleReconciliation` (sus listas, nuevas).» Se borra la frase de la excepción de L-1.
2. **Las firmas:** `public static PushBackEditableState Copy(PushBackEditableState state, PushBackResolver resolver)` y `public void CopyAuthorityFrom(PushBackEditorState source, PushBackResolver resolver)`. El copiador del `DraftSession` (PB-T4) captura el resolver. El esqueleto de `Copy` ignora el resolver. **Toda llamada a `Copy`** (en CT-PBT1B-06, 07, 08, 09 y 10, en CT-PBT1B-14 y en CT-PBT1C-02) **pasa un `PushBackResolver` del mismo catálogo que el estado que copia** (N-M-09): en Core, el de `JsonRackCatalogProvider.FromBaseDirectory().Load()`; en la UI, `new PushBackResolver(w.Session.Catalog)`.
3. **D-PBT1-05, «El resultado»,** pasa a: «no comparte con el original **ningún** objeto mutable».
4. **CT-PBT1B-06:** `workingBaseline` y `LastModuleReconciliation` pasan de **D** a **A** (autoridad copiada).
5. **CT-PBT1B-07:** la parte 1 pasa a «los objetos compartidos por todo el grafo, quitados los inmutables, son ninguno»; las partes 2 y 3 no cambian.
6. **CT-PBT1B-14 nuevo** (en `PushBackEditableStateCopyTests`), **la equivalencia:** sobre los 17 de `Fixtures/I64`, X-01, F-01 y `pb-20-ab` con B retirado, tras las secuencias del §5.0: `Persist(resolver.Snapshot(<línea base de la copia>))` es igual al del original; `moduleSignature` es igual; tras `Pasada` en el original y en la copia, los diseños persistidos son iguales; y tras el mismo guion (escenificar y confirmar un módulo, retirar y volver a declarar B, «Restaurar estándar» y confirmar) sobre los dos, siguen iguales. El filtro `PushBackEditableState` pasa a `MinSelected` **12**.
7. **INV-PBT1B-07b nuevo** (con «b» para no chocar con el común INV-PBT1x-07, «Las redes»; N-M-09), **paso 1b, sobre `BaseSha` y antes del RED** (una corrida temporal de Core con `Assert.Fail`): para cada sistema aceptado de `pb-20-ab`, X-01 y F-01, la línea `<diseño> resuelve=<True|EXCEPCION tipo> igual=<b>`, con `igual` = `Persist(Snapshot(Resolve(Snapshot(s))))` frente a `Persist(Snapshot(s))`.
8. **PBT1B-C16 nueva:** en el paso 1b, algún rack compuesto no resuelve o no da el mismo snapshot. STOP antes del RED: la opción (c) no es viable tal cual, y vuelve al Owner (la dificultad de DA:520-527).
9. **§8.1** pasa a «L-1 decidida: (c)», y desaparece la condición 4 de (a) en A-2.
10. **Previsión de la CI del GREEN de PB-T1b:** Core 19 070 + 13.
