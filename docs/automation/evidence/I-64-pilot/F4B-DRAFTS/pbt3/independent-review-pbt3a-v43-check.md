# I-64 F4B PB-T3a — Comprobación acotada (CA-T3a) del contrato en seco sobre el paquete v4.3

**Veredicto: EMITIBLE TRAS CORRECCIONES. B 0 / I 1 / M 4.**

**En breve:**
- **El contrato en seco es fiel a la v4.3** (§6.0 y §6.1) y a lo que ya aceptó PB-T2c: las 17 paradas estándar, C07 (la cola de RR), C18, C08, C12, las celdas y la raíz son las del contrato emitido de PB-T2c. Los marcadores que quedan son los previstos.
- **RR cubre en PB-T3a lo mismo que en PB-T2c,** con las mismas dos lecturas porque el RED toca `src/`. Lo que la reparación no puede tocar está dicho igual en C18, INV-PBT3A-02 y el §6.0. La lista cerrada de RR-3 con los valores de los gestos es correcta en el fondo. Quedan tres menores de precisión (M-01 a M-03).
- **Una importante, I-01:** `BeamDepth` es un gesto inerte. El ensamblador lo resuelve contra el catálogo, y `LARGUERO_IN_OUT_C6` solo ofrece el peralte 6. Cualquier valor se persiste como 6. Así, el ancla `… $.Structure.BeamDepth grupo=Rack clase=ChangedByUser …` de CT-PBT3A-09 no puede salir: es un C07 seguro en el GREEN. Es la lección M-13 de la §13 de CA-T2c, que la v4.3 no trajo.
- **La Snapshot K y C11** son fieles a lo custodiado en `56cb026f`.
- **No hay decisiones nuevas para el Owner.** I-01 es una corrección del paquete (una receta de prueba) que no toca ninguna de las ocho colecciones, salvo la versión que nombran.

**`BOUNDED_RESULT` propuesto (ASCII):**

`EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; importantes corregidas en el paquete: I-01 (CT-PBT3A-09 y CT-PBT3B-01 sin el gesto inerte de BeamDepth)`

**Cuándo vale:** cuando el paquete corregido lleve su fila de registro `| V…-nn | **CA-T3a I-01** | … |`. Si la sesión pide una recomprobación, irá en una sección propia («## §10. Recomprobación sobre la vX.Y») y el literal ganará al final `; recomprobacion vX.Y: EMITIBLE, B 0 / I 0 / M n`.

## 1. Método y límites

**Qué se leyó:**
- **El paquete:** `I-64-f4b-pbt3-gate-contract-package-v43.md` (241 746 bytes, sha256 `e7c4ab96…`) y `v43-delta.md`.
- **El generador:** `make_pbt3a_contract.py` (153 844 bytes, sha256 `69a502a7…`) y sus notas.
- **El seco:** `contract-dry-pbt3a.json`, `dry-pbt3a.txt` y `neg-pbt3a.txt`.
- **Las referencias,** por objetos git del checkout principal en `05beabfd` (y en `56cb026f` para la K y el sustituto):
  - el paquete de PB-T2 v2.9;
  - CA-T2c, §9 a §13;
  - el contrato emitido de PB-T2c;
  - las decisiones de F4B-E34, E35 y E37;
  - `snapshot-k.json`;
  - el sustituto de nc2 de F4B-E34;
  - los 17 fixtures de `Fixtures/I64`;
  - el código que citan las recetas.

**El seco cambió durante la revisión:**
- El encargo citaba `D0B3FEF1…` (60 425 bytes). El archivo vigente es `E7D63706…` (60 437 bytes), y es el que imprime `dry-pbt3a.txt`.
- La diferencia son los 12 bytes del nombre que fijó la sesión para este informe, `independent-review-pbt3a-v43-check.md`, y de «, CA-T3a» en su `Section` (DU-18).
- Revisé el vigente: lo leí entero y lo comparé con guiones contra el contrato de PB-T2c (§2).
- Las notas y las pruebas en negativo siguen siendo de la versión anterior (M-04).

**Lo que no se hizo:**
- No se leyó, listó ni ejecutó nada en el worktree de I-64. El código, solo en `05beabfd`.
- El `git log` del checkout principal muestra que la rama ya tiene el RED (`7aeb6170`) y el GREEN (`b1968341`) de PB-T2c. No los leí: lo que depende del cierre de PB-T2c se juzga solo como diseño (§6).
- No se ejecutó dotnet, y ninguna orden git escribió. El checkout principal sigue limpio.

**Los guiones** (Python con `-I`, escritos con la herramienta de archivos):
- `cat3a_fixtures.py`, `cat3a_custody.py`, `cat3a_compare.py`, `cat3a_c17.py` y `cat3a_bounded.py`;
- llevan la ruta local del checkout principal: si se custodian, hay que redactarla (como DU-11).

**Incidencias, declaradas para que el Coordinator las clasifique:**
- **Una orden de Python sin guion:** `python -I -X utf8 -m json.tool`, sobre una tubería de `git show` (el contrato de PB-T2c, para verlo indentado). Era de solo lectura, pero no era un guion escrito con la herramienta de archivos. Es el modo de fallo repetido de R-12, de H-12 y del `python -I -` de la §13 de la revisión del paquete.
- **Dos copias temporales que no eran guiones:** `cat3a_pda.cs.txt` y `cat3a_w.cs.txt`, salidas de `git show` de dos archivos de `src/`. Las escribí en `hs65\pbt3\` con el prefijo y las borré al terminar.

## 2. Fidelidad del contrato en seco (punto 1)

Comparado con `cat3a_compare.py` y `cat3a_c17.py` contra el contrato emitido de PB-T2c y el de PB-T1c:

| Campo | Estado |
|---|---|
| Identidad | `Schema`, `Unit`, `Gate`, `RoutingEnforcement` y `CorrectionsAuthorized`, iguales a los de PB-T2c. `TaskId` de 26 caracteres |
| `Objective` | Literal del §6.1, con «v4.3» |
| `AllowedWriteScope` (14) | Los ocho de `Draft/`, las cinco clases de Core y la de UI del §5.2 |
| `ForbiddenWriteScope` (203) | **`src`:** 89 (87 + los dos de PB-T2a).<br>**`tests/`:** 97 = las 96 de PB-T2c más su clase permitida, `PushBackWorkspaceCarryoverTests.cs`.<br>**La raíz:** 17, igual a la de PB-T2c |
| `Invariants` (14) | INV-PBT3A-01 a 06 y 07 a 14, literales y sin las notas. INV-PBT3A-02 lleva la cadena RR-6 y «ningún archivo de `src/`» |
| `RequiredTests` (29) | Las 21 filas del §6.1 más las 8 de PB-T2, con los mínimos del paquete. `PushBackWorkspaceCarryoverTests` sale con 3 hasta que exista (es un AVISO) |
| `ExpectedEvidence` (9) | Las ocho viñetas del §6.1, literales. La novena es la línea base prevista, que al escribir se cambia por la medida (`CI_BASE_T2C`) |
| Las 17 paradas estándar | Iguales una a una a las de PB-T2c, con P-01 en la K |
| C07 | El texto propio de PB-T3 más la cola de RR, igual a la de `PBT2C-C07`. Solo cambia el punto final (DU-17, inocuo) |
| C18 | Igual a `PBT2C-C18`, más el punto final |
| C08 y C12 | Iguales a los de PB-T1c y PB-T2c |
| C11 | La cita del §6.0 (§5) |
| C17 | La cabecera de `PBT1C-C17` sin su frase propia de PB-T1c, y la cita del Owner. Frente a `PBT2C-C17`, solo falta la frase de la ventana WORKSPACE, que es de PB-T2c |
| C04, C13 y C15 | Los del §6.1 |
| C01 a C03, C05, C06, C09 y C10 | Los del §6.0 |
| `EligibleCells` | Iguales a las de PB-T2c (vencen el 2026-12-29) |
| `Authorities` (64) | Las previstas. La de este informe ya lleva el nombre fijado (DU-18). F4B-E35 resume RR-7 con «que en PB-T3a no toca src/» |
| Marcadores | Dos en `IssuedBy`; la `Section` del cierre de PB-T2c; `AuthorityRevision` y `IssuedUtc` de seco. Son los previstos |

**Lo que no se sostiene:** nada en el contrato. El defecto de I-01 está en una receta del paquete, que el contrato no copia.

## 3. RR en PB-T3a frente a PB-T2c (punto 2)

### 3.1 Cobertura

| Pieza de RR | Contrato de PB-T2c | Contrato de PB-T3a (seco) | Estado |
|---|---|---|---|
| RR-1 a RR-6 en las paradas | La cola de `PBT2C-C07` y `PBT2C-C18` | Las mismas, letra a letra | Igual |
| RR-6, la cadena | INV-PBT2C-02 | INV-PBT3A-02, con «ningún archivo de `src/`» en el tramo de la reparación | Igual, adaptada |
| RR-4, la CI del RED reparado | La `ExpectedEvidence` | El §6.0 y las viñetas 2 y 3 de la evidencia | Igual |
| RR-5, la entrega | La `ExpectedEvidence` | La viñeta 7 de la evidencia | Igual |
| n.º 11, el `Trailer` | En el contrato | En la viñeta 7: el trailer real de los commits RED, «RED repair» y GREEN | Igual |
| n.º 10, y lo que el Controller comprueba de RR | **En el contrato,** décima entrada de la evidencia: las órdenes git (`RedRepairSha..CurrentSha`; `--diff-filter=A BaseSha RedSha -- tests/`), RR-1 como hijo directo de `RedSha`, el tramo propio de la subtarea, la ayuda de RR-3 y los indicios | **Por referencia:** la viñeta 8 remite a «section 6.0, "La verificacion con una reparacion del RED"», y la autoridad F4B-E35 lo resume. El texto operativo está en el paquete (l.1214-1224), que es autoridad | Cubierto, pero no autocontenido (M-01) |
| La ayuda de RR-3 | Una lista enumerada en el contrato (las 49 líneas, los exactos, la de `pb-08-redondeo`, las dos de estilo, las doce de CT-PBTC-07 y la de «Aplicar») | Una tabla por clase en el §6.0, y la sesión escribe la lista en el relevo | Correcta en el fondo; dos precisiones (M-02 y M-03) |

### 3.2 Las dos lecturas, porque el RED toca `src/` (V43-01)

Son fieles:
- **RR-1:** el propio texto del Owner dice «antes del primer commit que toque `src/` (el GREEN)», así que «antes del commit GREEN» no amplía nada.
- **RR-6, primer tramo:** el Owner dice «`BaseSha..RedSha`, solo `tests/`», y la v4.3 lo lee «el RED de cada contrato». Es la lectura V27-01 de PB-T2:
  - la revisaron CA-T2b (§11) y CA-T2c (§10 y §11, D-17);
  - y ya se aplicó: PB-T2b cerró con el RED `a3b86134`, la reparación `6bc3497f` y el GREEN `03b16c88`, y su verificación pasó con `rr_check` (F4B-E38).
- **El tercer tramo:** «`RedRepairSha..CurrentSha`, solo `src/`» coincide con el GREEN de PB-T3a, que solo toca los ocho archivos de `Draft/`.

No hace falta ninguna decisión nueva.

### 3.3 Lo que la reparación no puede tocar en PB-T3a

**Solo puede tocar** las seis clases de prueba que añade el RED.

**No puede tocar:**
- `src/`, incluidos los ocho archivos del esqueleto de `Draft/`;
- ninguna prueba existente;
- ningún fixture;
- nada fuera de las 14 rutas permitidas.

**Dónde está dicho:** lo dicen igual C18 («src/»), INV-PBT3A-02 («ningún archivo de src/») y el §6.0 (RR-2 y «Por subtarea»). Un error del esqueleto no se repara: es C04 o C07, como quiere el diseño.

### 3.4 La lista cerrada de RR-3, con los valores de los gestos (DU-A)

**Es correcta en el fondo.** RR-3 del Owner dice «ninguna línea esperada, literal ni oráculo que fije el paquete o el contrato cambia». Un valor que el paquete prescribe para un gesto (4.5, «PB-NUEVO», `SetGap(30)`, una escala) es un literal del paquete: cambiarlo cambia lo que la prueba prueba. Además, la tabla nombra exactamente las seis clases nuevas de PB-T3a.

**Dos precisiones:**
- **La unidad de comparación de los valores (M-02).** La ayuda compara «líneas literales … byte a byte», y la sesión escribe en el relevo «la lista cerrada de esas líneas tal como la clase las lleva en `RedSha`». Para una línea esperada eso es exacto. Para un valor, la línea es la de código que hace el gesto, y RR-3 del Owner sí admite cambiar «el paso de la receta que el paquete ya prescribe».
  - **El caso típico:** en CT-PBT3A-07, cambiar `PalletTolerance` por `SetCurrentDraftState` sobre una copia, y no sobre el objeto que devuelve el borrador (C-1 de F4B-E33).
  - **El resultado:** ese cambio de la línea que lleva `4.5` sería un C07 «cierto», en contra de RR-3.
- **La enumeración sin interpretar (M-03).** Varias filas no se pueden enumerar sin criterio: «CT-02: los casos y sus constantes», «Las líneas de CT-PB0-10 que copia», «los nombres de CT-12», «los términos prohibidos de CT-14». Es la M-11 de la §12 de CA-T2c, que PB-T2c corrigió en su V29-04.

## 4. Las recetas, el momento de cada lectura y los gestos (punto 3)

### 4.1 Los valores de los fixtures (`cat3a_fixtures.py`, sobre `05beabfd`)

| Campo | Valor en los 17 |
|---|---|
| `Structure.PalletTolerance` | 4 |
| `Structure.BeamDepth` | 6 |
| `Structure.PostPeralte` | `null` |
| `Structure.AnnotationScale` | 2 en `pb-02-modulos`, 3 en `pb-05-topes`, 2.5 en `pb-06-seguridad`, 0.75 en `pb-03-fondos`, 1.25 en `pb-04-blanco`, 1.75 en `pb-07-combinado`, 1.23456 en `pb-08-redondeo` y 1.5 en los demás |
| `Structure.DimensionStyle` | STANDARD-ISO en `pb-06`, SIMPLEX en `pb-07` y `null` en los demás |
| `Composite.Gap` | 54 en `pb-20-ab` y `pb-21-cola`; `null` en los demás (`pb-15` no tiene `Composite`) |
| `Structure.PalletDepth` | 48 |
| `CustomProperties` del sobre | Ausente o nula |

La medición del §5.0 se sostiene. `SetGap(30)` sobre `pb-20-ab` cambia el valor (54).

### 4.2 Las ocho recetas que cambian (V43-11)

| Receta | El momento de la lectura | El gesto | Estado |
|---|---|---|---|
| CT-PBT3A-07 | `IsDirty` justo antes. La base nueva sale del resultado | `PalletTolerance` 4 → 4.5. Dirty por el comparador, que mira la ranura `PalletTolerance` | Bien |
| CT-PBT3B-15 | Ídem | Ídem | Bien |
| CT-PBT3A-09 | El clasificador compara diseños persistidos (D-PBT3-07) | **c:** `PalletTolerance` 4.5, que se persiste tal cual (F3 conserva el exacto).<br>**`PostPeralte` 3 y 4:** `ApplyPostPeralte` no lo ajusta (`DynamicRackSystemBuilder.cs:179-191`), y F2 conserva el exacto.<br>**d: `BeamDepth` 4 → se persiste 6** | **I-01** |
| CT-PBT3B-01 (i) | — | `BeamDepth` 4 es inerte, igual que en CT-09. `Ready` sale igual, por la tolerancia y el nombre; ninguna línea falla, pero el gesto no prueba nada | Dentro de I-01 |
| CT-PBT3B-06 (iii) | — | Escala 2,0, o 2,5 en `pb-02-modulos` (que tiene 2). Ningún otro de los 15 tiene 2 | Bien |
| CT-PBT3B-07 (ii) | Las lecturas, tras la corrida; `IsDirty`, justo antes | Escala 3,0, o 3,5 si ya tiene 3 | Bien |
| CT-PBT3B-10 | La autocomprobación ` base-igual=True dirty=True` | La escala de `Rich("pushback")` no está fijada; la autocomprobación lo cubre (DU-C) | Bien |
| CT-PBT3B-12 | Las referencias, antes del `Prepare`; la huella, sobre esas mismas referencias | — | Bien |

### 4.3 I-01: por qué `BeamDepth` no cambia el diseño

**La cadena, en `05beabfd`:**
1. **El diseño se arma con el ensamblador.** El clasificador arma el diseño de cada estado con `PushBackCompositeEditorAssembler.BuildDesign(copy.Editor, copy.BuildInputs(…))` (D-PBT3-07). Este llama a `PushBackEditorDesignAssembler.BuildDesign` (`PushBackCompositeEditorAssembler.cs:59`).
2. **El ensamblador resuelve el peralte contra el catálogo:** `var beamDepth = DynamicLoadBeamGeometry.ResolveBeamDepth(catalog, DynamicRackDefaults.InOutBeamCatalogId, editorInputs.BeamDepth …)` (`PushBackEditorDesignAssembler.cs:165-168`).
3. **El catálogo solo admite 6.** `ResolveBeamDepth` devuelve el valor pedido solo si está entre los peraltes del perfil; si no, el primero (`DynamicLoadBeamGeometry.cs:52-71`). El perfil es `LARGUERO_IN_OUT_C6` (`DynamicRackDefaults.cs:30`), con peraltes «6» (`assets/catalogs/secciones.csv:8`).
4. **El diseño persistido lleva el resuelto:** el `BeamDepth` del diseño sale de `resolver.Snapshot(system, …, beamDepth, …)` (`DynamicEditorDesignAssembler.cs:103`), que escribe `system.InOutBeamDepth` o ese `beamDepth` (`DynamicRackSystemResolver.cs:311`). Los dos valen 6.
5. **Las pruebas de Core usan el catálogo real:** `PilotKit.Catalog` es `JsonRackCatalogProvider.FromBaseDirectory().Load()` (`WorkspaceDraftPilotStateTests.cs:32-34`).
6. **El paquete ya lo había medido:** F4 de CT-PBTC-02 (i) y CT-PBT2C-01 da `6` para el exacto 6,00044. Lo cita el propio CT-PBT3B-01 (ii).
7. **Y es la M-13 de la §13 de CA-T2c:** `CellInOutPeralteBox=otro` es un gesto inerte, porque `AllowedPeraltes` de `LARGUERO_IN_OUT_C6` es solo «6». La v4.3 dice importar la §5.0 de PB-T2 («Los gestos que no cambian nada»), pero no trajo esa lección.

**Qué pasa en CT-PBT3A-09:**
- **El diseño de d es el de b,** salvo que algo derivado cambiara, y con 6 en los dos no cambia nada.
- **El oráculo de la parte (1) no lo ve:** da `igual-al-oraculo=True`, porque el oráculo diffea los mismos textos.
- **El ancla de la parte (2) no sale:** `… $.Structure.BeamDepth grupo=Rack clase=ChangedByUser …` da `presente=False`. La línea esperada no se cumple en el GREEN, y eso es C07 seguro.
- **RR no lo salva:** el error solo se ve en el GREEN, porque el esqueleto no da entradas, y el valor está en la tabla de RR-3.
- **Se pierde además la única ancla `ChangedByUser` por campo de CT-09.** Las otras son `ChangedExternally` y `Overlapping`.

**La justificación del §5.0 tampoco se sostiene.** `DynamicRackDefaults.LegacyDefaultBeamDepth` (4.0) solo lo usa la persistencia de los documentos dinámicos heredados sin `BeamDepth`:
- `DynamicRackSystemDocument.cs:210`, `:310` y `:618`;
- `AuthoredCanonical.cs:41`;
- y su prueba, `RackProjectStoreTests.cs:330`.

No es una edición de Push Back.

### 4.4 El resto de los CT de PB-T3a

| CT | Revisión | Estado |
|---|---|---|
| CT-01 | Sin gestos: lee justo tras `LoadExisting`, que acaba en el `Recompute();` de W:404. Las anclas W:150-151, :197, :221, :345, :404, :624, :627-635 y :669 se sostienen en `05beabfd`, y el `src` es igual en `03b16c88`, `56cb026f` y `05beabfd` | Bien |
| CT-02 a 05 | Las ediciones de CT-05 (el `Mutator`, `Origin`) se comparan con el comparador de estados, no con el diseño persistido, así que no hay ajuste del ensamblador que las anule | Bien |
| CT-06 | **Se comparan estados:**<br>• la escala NaN, +∞, 0 y −1 (frente a 1.5);<br>• `PalletTolerance` + 1 en el estado inicial (5 frente a 4), por el comparador del paso 3 de `ForWrite`;<br>• `SetGap(30)` (frente a 54) y «PB-NUEVO» (frente a «PB-20»), por la igualdad con `Pass`.<br>**El momento:** `current` sin cambios se compara con una copia y una huella tomadas antes de `ForWrite` (regla del §5.0) | Bien |
| CT-08 | Función pura, sin gestos | Bien |
| CT-10 | **La longitud de módulo cambiada fuera no está medida.** Por la regla del ensamblador, se conserva: sin cambio de tarima ni de fondos, reutiliza una copia de la línea base cargada (`PushBackEditorDesignAssembler.cs:113-120` y el ciclo de `:195-213`). Si no se conservara, el ancla «al menos una `Modules` estructural `ChangedExternally`» lo diría (C07). No lo elevo a hallazgo | Bien, sin medir |
| CT-11 | Las referencias se toman antes de cada operación, y la huella de después se calcula sobre ellas (regla del §5.0) | Bien |
| CT-12 a 14 | Guardas de texto, sin gestos | Bien |

## 5. La Snapshot K y C11 frente a lo custodiado (punto 4)

`cat3a_custody.py`, sobre `56cb026f` y `05beabfd`:

**La Snapshot K:**
- **El registro** (`F4-ENVIRONMENT/snapshot-K/snapshot-k.json`) está en las dos revisiones:
  - `Snapshot` K;
  - `ConfigSha256` `E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE`;
  - `ReplacesSnapshot.J` `57B0FED6…`;
  - `NeutralProbe.RunId` `R20261119T154313Z-2a9a`, con `Pass` true, `gpt-6-luna`/`high`, solo lectura y el binario acreditado `4F91AE86…`.
- **La decisión** trae la respuesta literal «Sonda neutra, luego K (Recomendado)».
- **P-01 del seco** es la de PB-T2c y nombra esa ruta.

**El sustituto de nc2 de F4B-E34:**
- **El archivo:** la ruta de C11 existe en las dos revisiones. Su SHA-256 es `6ccaf12dbf9122c0bddc5b724bcff573705c95698af38bf750bf69459ee2ffd3` (35 304 bytes, LF) y su `ID` es `PBT2A-NC2-EXACT-WHITELIST-F4B-01`.
- **La decisión** (pregunta 2) trae la respuesta literal «Adaptar el sustituto (Recomendado)».
- **Su «Alcance»** dice: «en el resto de F4B, el sustituto adaptado se reutiliza como `PBT<x>-NC2-EXACT-WHITELIST-F4B-01`, con solo el identificador y el ámbito cambiados, y **solo** con la firma exacta de PD-F4-01 … Si cambia `inject_opaque.py`, el sustituto se revisa antes de reutilizarlo».
- **C11 lleva las tres cosas:** la copia con solo el id y el ámbito, la firma exacta y `inject_opaque.py`.

**El registro `SATISFIED_UNDER_PBT3A_…_F4B`:** la copia tendrá que cambiar también la cadena del registro (`:450` del sustituto), además del `ID` (`:10`) y del ámbito (`:409`). Es la convención de las copias de F4, que se llamaron «con solo el id y el ámbito» y cambiaron su registro (`F4T1B`, `F4T4A1` y `F4T4A2`, `:260`). No es un hallazgo.

**C11 frente a `PBT2C-C11`:** solo cambia lo previsto:
- el prefijo;
- el sustituto de F4B, con ruta y SHA;
- el modo F4B-E21;
- la cláusula de `inject_opaque.py`;
- y las aceptaciones puntuales de nc1, que pasan a PB-TC (F4B-E08), PB-T2a (F4B-E34) y PB-T2b (F4B-E38).

La cláusula final, «no one-off acceptance extends to PB-T3a», cubre cualquier otra.

## 6. Lo que el generador debe volver a medir al escribir, tras el cierre de PB-T2c (punto 5)

1. **El cierre de PB-T2c** (`PT2C_CLOSE` y su entrada de `f4b-dispositions.md`):
   - la descendencia;
   - `git diff <cierre de PB-T2c> <BaseSha> -- src` vacío;
   - y `src` igual al del cierre de PB-T2b, que el GREEN de PB-T2c garantiza por INV-PBT2C-01.

   Si no se cumple, el `assert` para, y es material.
2. **`CI_BASE_T2C`:** run, sha8 del GREEN, Core `Passed`, total de UI y `Skipped`.
   - El GREEN previsto se recalcula con + 13 y + 1.
   - Si la base no es 19 094 y 2 450, cambian las cifras, no el contrato.
3. **`PushBackWorkspaceCarryoverTests.cs` en HEAD:**
   - su `MinSelected`, medido y nunca por debajo de 3;
   - ya está en el `ForbiddenWriteScope`.
4. **Los mínimos medidos:** `Workspace` (3 271 y 547), `PushBackEditableState` (13) y los demás. PB-T2c no los toca, así que deben salir iguales.
5. **Las anclas de W, por contenido** (son `assert`). El GREEN de PB-T2c restaura W y WC, así que deben salir iguales.
6. **La Snapshot vigente.** Si durante PB-T2c llega otra (P-01), es una decisión nueva del Owner: cambian P-01 y la autoridad, y se vuelve a él.
7. **Las entradas del cierre de PB-T2c:**
   - **una aceptación puntual nueva de nc1:** C11 ya la excluye con su cláusula final, y nombrarla es opcional;
   - **una decisión sobre nc2 de PB-T2c** (DU-B: su contrato lleva el sustituto de F4): si cambia el sustituto de F4B (otra adaptación u otro SHA), C11 cambia, y es material.
8. **`inject_opaque.py`,** sin cambios desde la corrida `R20261119T125505Z-c2d3`. Si cambió, el sustituto se revisa antes de usarlo (C11).
9. **El contrato de PB-T2c en HEAD,** igual al emitido en `05beabfd`: sus listas de `tests/` y los `MinSelected` de PB-T2. Si una corrección lo cambió, se recalcula.
10. **La custodia:**
    - el paquete corregido de I-01, byte a byte, con `CUSTODY_SHA256`, `PKG_VERSION` y la versión del `Objective` al día;
    - el registro R-0 a R-18;
    - la revisión §1 a §13;
    - `pbt3a-anchors-out.txt`;
    - este informe y su recomprobación, si la hay;
    - la revisión de compatibilidad;
    - las 20 de `CUSTODY_EXTRA`, con el encargo literal de esta comprobación (`independent-review-pbt3a-v43-check-prompt.md`);
    - y la redacción de DU-11.
11. **`BOUNDED_RESULT`,** con el literal de arriba. `bounded_check` exige la fila `**CA-T3a I-01**` en el registro del paquete (§9).
12. **Las 80 pruebas en negativo, otra vez** sobre el generador final, y las notas al día (M-04).

## 7. Decisiones del Owner (punto 6)

**Ninguna nueva.**
- **I-01 es una receta de prueba,** dentro de la autoridad del paquete. Se corrige antes de emitir, como pide F4B-E35, pregunta 3 («0 importantes sin resolver»). No cambia el producto ni ninguna obligación.
- **M-01 a M-04 son de precisión y de custodia.**
- **DU-B es de PB-T2c.** Solo volvería al Owner por el camino del punto 7 del §6.

## 8. Hallazgos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| I-01 | I | §5.0, «Los gestos que no cambian nada» (`BeamDepth`); §5.2, CT-PBT3A-09 (d); §5.3, CT-PBT3B-01 (i); §6.0, la fila CT-09 de la tabla de RR-3; V43-11 | **El problema:** `BeamDepth` es un gesto inerte. El ensamblador lo resuelve contra el catálogo, y `LARGUERO_IN_OUT_C6` solo ofrece 6 (`PushBackEditorDesignAssembler.cs:165-168`; `DynamicLoadBeamGeometry.cs:52-71`; `secciones.csv:8`; `DynamicRackSystemResolver.cs:311`). Cualquier valor se persiste como 6, como F4 de CT-PBTC-02 (i).<br>**La consecuencia:** el ancla `ChangedByUser` de `$.Structure.BeamDepth` de CT-PBT3A-09 da `presente=False`, y eso es C07 seguro en el GREEN, sin reparación posible. En CT-PBT3B-01 (i) el gesto no prueba nada.<br>**La causa:** la v4.3 no trajo la M-13 de la §13 de CA-T2c (los gestos que el catálogo deja sin opción), y justifica el 4 con `LegacyDefaultBeamDepth`, que solo usa la persistencia de documentos dinámicos heredados | **En el §5.0:** decir que `BeamDepth` no se edita en la red, porque el catálogo solo ofrece el peralte 6. Añadir la regla general: un gesto cuyo valor el ensamblador ajusta, o al que el catálogo deja una sola opción, tampoco cambia nada (M-13 de CA-T2c).<br>**En CT-PBT3A-09 (d),** el cambio del usuario va sobre un campo del grupo `Rack` que el ensamblador conserve, medido y con un valor distinto del de los fixtures. El candidato medido es `PalletDepth`: 48 en los 17, y F1 conserva su exacto. Cambian el ancla, la fila CT-09 de la tabla de RR-3 y V43-11.<br>**En CT-PBT3B-01 (i),** el mismo cambio.<br>**En el registro,** la fila `\| V…-nn \| **CA-T3a I-01** \| … \|`.<br>**En el contrato** solo cambian la versión del `Objective` y la `Section` de la autoridad del paquete |
| M-01 | M | §6.0 (l.1214-1224) y §6.1, viñeta 8 de la `ExpectedEvidence` | **El problema:** la lectura de la verificación con una reparación llega al contrato solo por referencia: n.º 10 desde `RedRepairSha`, lo que el Controller comprueba de RR con sus órdenes git, el tramo propio de PB-T3a y la ayuda de RR-3. En PB-T2c va entera en la evidencia (V28-01), y V43-05 dice «como la v2.8 de PB-T2». Está cubierto, porque el paquete es autoridad, pero el contrato no es autocontenido | Copiar en ASCII el párrafo del §6.0, con lo de PB-T3a, como décima entrada de la evidencia, como PB-T2c. Si no, declarar en el §6.0 que la verificación lo lee del paquete |
| M-02 | M | §6.0, «La ayuda mecánica de RR-3» y la nota de la tabla | **El problema:** para los valores de los gestos, comparar byte a byte «la línea tal como la clase la lleva en `RedSha`» haría C07 de una reparación que RR-3 del Owner admite. Por ejemplo, cambiar el paso de la receta que lleva `4.5`, sin cambiar el valor | Decir que, para los valores, la lista lleva el literal (`4.5`, `"PB-NUEVO"`, `30` de `SetGap`, las escalas) y que se compara su número de apariciones en la clase. La comparación línea a línea queda para las líneas esperadas y para los fragmentos literales de las líneas con plantilla |
| M-03 | M | §6.0, la tabla de RR-3 | **El problema:** varias filas no se pueden enumerar sin interpretar: «CT-02: los casos y sus constantes», «las líneas de CT-PB0-10 que copia», «los nombres de CT-12» y «los términos prohibidos de CT-14». Es la M-11 de la §12 de CA-T2c | Dar en cada fila un selector mecánico. CT-02: los fragmentos ` completo=`, ` piloto=` y ` motivo=`, y los nombres de las constantes del piloto que cita D-PBT3-03. CT-08: `PushBackFullEditorCharacterizationLateralCutTests.cs:<líneas>` en la base. CT-12: los cinco nombres. CT-14: la lista de términos del §5.2 |
| M-04 | M | `generator-notes-pbt3a.md` (cabecera y §3) y `neg-pbt3a.txt` | **El problema:** las notas siguen diciendo 60 425 bytes, sha256 `D0B3FEF1…` y un generador de 153 108 bytes. Las 80 pruebas en negativo corrieron sobre ese generador (contrato `d0b3fef1`). El seco y el generador vigentes son 60 437 bytes, `E7D63706…`, y 153 844 bytes, `69a502a7…`. La diferencia es solo el nombre de este informe (DU-18), pero lo que se custodia no casa | Volver a pasar las 80 pruebas sobre el generador final y poner las notas al día antes de custodiar. Repetirlo tras la corrección de I-01 |

## 9. El `BOUNDED_RESULT`, comprobado

**Cómo:** `cat3a_bounded.py` saca la función `bounded_check` de `make_pbt3a_contract.py`, entre sus marcas `# ---- bounded_check (inicio)` y `(fin)`, y la aplica a este informe entero, al literal propuesto y a las líneas de la v4.3.

**Lo que da:** un solo error, el esperado:
- «ningun registro de cambios del paquete corrige I-01 de CA-T3a»;
- desaparece cuando el paquete corregido lleve su fila `**CA-T3a I-01**`.

**Lo demás pasa:**
- el veredicto, en la forma de PB-T2, y de la familia EMITIBLE;
- el literal, que empieza por él, sigue con `;` (el `assert` exige `[.,;]` tras el veredicto) y lleva `B 0 / I 1 / M 4` y «corregidas en el paquete»;
- la fila `| I-01 | I |` de la tabla;
- y ningún encabezado de recomprobación.

## §10. Recomprobación sobre la v4.4

**Veredicto: EMITIBLE.** B/I/M = **0 / 0 / 1**

**En breve:**
- **I-01 y M-01 a M-04 están corregidas** (§10.2).
- **`PalletDepth` 48,5 sí llega al diseño persistido.** Lo conserva toda la cadena del ensamblador: un fondo distinto obliga a reconstruir con esa tarima, y la tarima se copia hasta el documento. Así, el ancla `ChangedByUser` de CT-PBT3A-09 saldrá `presente=True`. En CT-PBT3B-01 (i), `Erasures` sigue vacía, porque los cortes laterales no dependen del fondo.
- **`PostPeralte` 3,25 y 4,25 no son gestos inertes:** nada los ajusta ni los compara con el catálogo.
- **DU-23 y DU-25 valen.** La única menor nueva es de custodia (M-05).
- **No hay decisiones nuevas para el Owner.**

**`BOUNDED_RESULT` final (ASCII):**

`EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; importantes corregidas en el paquete: I-01 (CT-PBT3A-09 y CT-PBT3B-01 sin el gesto inerte de BeamDepth); recomprobacion v4.4: EMITIBLE, B 0 / I 0 / M 1`

### 10.1 Método y límites

**Qué se leyó:**
- **El paquete:** `I-64-f4b-pbt3-gate-contract-package-v44.md` (256 921 bytes, sha256 `a96063ac…`) y `v44-delta.md`. El `diff` con la v4.3 solo toca lo que dice el registro: la cabecera, el registro, la custodia, el §5.0, CT-PBT3A-09, CT-PBT3B-01, la ayuda y la tabla de RR-3, la evidencia de §6.1 a §6.3 y las versiones de los `Objective`.
- **El generador:** `make_pbt3a_contract.py` (166 745 bytes, sha256 `14739806…`).
- **El seco:** `contract-dry-pbt3a.json` (62 226 bytes, sha256 `DFFF5C02…`) y `dry-pbt3a.txt`.
- **Las pruebas en negativo:** `neg-pbt3a.txt`, 115 de 115, sobre ese contrato.
- **Las notas,** con DU-23 a DU-25.
- **El código,** por objetos git en `05beabfd`. Nada del worktree, donde está en curso la verificación de PB-T2c.

**Los guiones de esta ronda:**
- **`cat3a_v44.py`, nuevo.** Recibe el checkout por argumento, así que no lleva ninguna ruta local.
- **`cat3a_bounded.py`, rehecho.** Recibe el paquete por argumento, y su búsqueda de rutas usa la expresión de `local_paths` del generador. Ya no lleva el patrón con fragmentos del nombre de la usuaria que tenía en la §9.
- **Una copia temporal:** volví a escribir y borré `cat3a_pda.cs.txt` (el `git show` del ensamblador), como en la §1.

**Sin incidencias nuevas:** ningún Python sin guion, nada en `/tmp` y ninguna escritura git.

**La §9 se escribió antes que esta sección.** Su «ningún encabezado de recomprobación» describe el informe de entonces.

### 10.2 Los hallazgos de la §8, uno a uno

| Hallazgo | Estado | Dónde (v4.4) |
|---|---|---|
| **I-01** | Corregida | **El registro:** V44-01, l.26, con la fila `\| **V44-01** \| **CA-T3a I-01** \|` que pide `bounded_check`.<br>**El §5.0:** l.931, `PalletDepth` 48.5 y por qué se conserva; l.936-939, la lección M-13 y la regla; l.941-952, el repaso de los gestos.<br>**CT-PBT3A-09:** l.987, (d) `PalletDepth` a 48.5 y su ancla.<br>**CT-PBT3B-01 (i):** l.1038, con la línea `campos PalletTolerance=4.5 PalletDepth=48.5`.<br>**La tabla de RR-3:** l.1270 y l.1272.<br>**El contrato:** solo cambian la versión («v4.4» en el `Objective`) y la `Section` de la autoridad del paquete. `PalletDepth` no aparece en el contrato, y `BeamDepth` solo en `IssuedBy`, dentro del literal |
| **M-01** | Corregida | **El §6.1,** l.1382; **el seco,** `ExpectedEvidence[8]`, en ASCII:<br>• n.º 10 desde `RedRepairSha`;<br>• n.º 11;<br>• lo que el Controller comprueba de RR, con sus órdenes git;<br>• «En PB-T3a, además: … ningún archivo de src/»;<br>• la ayuda de RR-3, con las dos clases de literal;<br>• y los indicios.<br>Es la misma forma que la entrada de PB-T2c. PB-T3b y PB-T3c la heredan con su frase propia (l.1445 y l.1481). Solo cambia «RT U ChainRedFiles» (el `∪` plegado), frente a «RT o ChainRedFiles» de PB-T2c; es inocuo |
| **M-02** | Corregida | **El §6.0,** l.1256-1258. Las líneas y los fragmentos se comparan como subcadena exacta, por número de apariciones. Los valores, como token («sin un dígito, una letra ni un punto pegados»), por número de apariciones. Cambiar la línea de un gesto sin cambiar el valor ya no es C07. Lo mismo dice `ExpectedEvidence[8]` |
| **M-03** | Corregida | **La tabla,** l.1262-1277. Comprobado en `05beabfd`:<br>• **CT-02:** los siete nombres de constantes existen en `PushBackPilotMaterializer.cs:100-120`;<br>• **CT-08:** las ocho etiquetas están en `PushBackFullEditorCharacterizationLateralCutTests.cs:99-119`, y sus obsoletas en `:127-133`;<br>• **CT-04:** las 28 rutas, en `I58F1Matrix.cs:97-102`;<br>• **CT-12 y CT-14:** los nombres y los quince términos, literales.<br>Las 27 líneas de la parte (3) de CT-09 van por propiedad y dueño, y su `fila=` no es literal. Se enumeran mecánicamente como las líneas de la clase en `RedSha` que empiezan por `Envelope $.`, `Project $.` o `Payload $.`. Basta (DU-24) |
| **M-04** | Corregida | **Las notas,** cabecera y §3: el generador de 166 745 bytes y `14739806…`, y el seco de 62 226 bytes y `DFFF5C02…`.<br>**Las pruebas:** `neg-pbt3a.txt`, 115 de 115, con los controles positivos sobre `dfff5c02…`, el seco vigente.<br>**Las fechas:** el generador se escribió antes que el seco, y el seco antes que las pruebas y las notas, así que casan |

### 10.3 I-01: `PalletDepth` 48,5, hasta el diseño persistido

**La cadena, en `05beabfd`, por el camino de una edición del estado** (d de CT-PBT3A-09 y CT-PBT3B-01 (i)):
1. **El estado da sus entradas:** `PushBackEditableState.BuildInputs` pone `Pallet = new PalletSpecification(Pallet.Front, Pallet.Depth, …)` (`PushBackEditableState.cs:371-372`).
2. **El ensamblador copia la tarima tal cual:** `var pallet = ClonePallet(editorInputs.Pallet)` (`PushBackEditorDesignAssembler.cs:176`, `:573-576`).
3. **Un fondo distinto obliga a reconstruir.** `mustRebuild` incluye `DynamicEditorDesignAssembler.MustRebuild(rebuildShape, pallet, depthLayout)` (`:211-213`), y `MustRebuild` da cierto si `!SamePallet(system.Pallet, pallet)` (`DynamicEditorDesignAssembler.cs:47`). `SamePallet` compara el fondo con 1e-6 (`:50-60`, `:58`): 48 frente a 48,5 reconstruye.
   - **Una precisión:** V44-01 cita `MustRebuild` y `BuildDefault` (`:213-218`), pero no nombra `SamePallet`, que es la condición que decide. No cambia nada.
   - **Por qué importa:** sin reconstrucción, la rama de la copia (`CopyStructureSystem(baseline)`, `:273`) conservaría la tarima de la línea base, y el cambio se perdería.
4. **La reconstrucción usa esa tarima:** `builder.BuildDefault(pallet, …)` (`:218`) hace `Pallet = pallet` (`DynamicRackSystemBuilder.cs:92`). Entre ahí y el armado, nada vuelve a asignar `system.Pallet`: en el ensamblador, la única línea que nombra `.Pallet` es la `:176`, y el ensamblador compuesto no la nombra (`git grep`).
5. **El diseño copia la tarima del sistema:** `dynamicAssembler.BuildDesign(system, …)` llama a `resolver.Snapshot(system, …)` (`DynamicEditorDesignAssembler.cs:103`), que hace `Pallet = CopyPallet(system.Pallet)` (`DynamicRackSystemResolver.cs:306`).
6. **El documento la escribe:** `PalletDepth = design.Pallet?.Depth` (`DynamicRackSystemDocument.cs:146`).

**El otro camino,** un sobre editado con `EditInner` y materializado, carga una línea base con 48,5, y la rama de la copia la conserva. Es lo que mide F1 (48,00044).

**El ancla:**
- **Qué da cada estado:** b y c persisten 48 por la línea base; d persiste 48,5.
- **Cómo se clasifica:** `$.Structure.PalletDepth` cae en «lo demás de `$.Structure`», que es el grupo `Rack`. No es `Fronts`, `Modules`, `SideB` ni `Composite`, así que es `estructural=False`. Sale `ChangedByUser`, con `presente=True`.
- **Las hojas derivadas** de la reconstrucción (la secuencia de módulos) las cubre el oráculo mecánico, como dice la l.931.
- **Lo que queda sin corrida:** la cadena se leyó en el código, sin dotnet (como declara `v44-delta.md`). Si fallara, la línea lo diría en el GREEN (C07).

**`Erasures` vacía en CT-PBT3B-01 (i):**
- **De dónde salen los laterales:** `PushBackSystemLateralBuilder.Cortes` recorre `DynamicFrontGeometry.Compute(structure, catalog).PostPositions` y salta solo las fronteras inexistentes (`PushBackSystemLateralBuilder.cs:91-113`).
- **Por qué no cambian:** el número de postes transversales lo dan los frentes, no el fondo de la tarima. Ningún `PostIndex` desaparece, así que no hay ningún `ObsoleteLateral`. Si los miembros del caso no traen laterales, la lista queda vacía igual.

**El repaso con la lente de M-13** (l.941-952). Los gestos que cambian el diseño persistido se conservan:
- **`PalletTolerance`:** `:172-174`, y F3 conserva el exacto;
- **la escala:** no pasa por el catálogo, y F5 conserva el exacto;
- **los frentes, el lado B y los módulos de F-01:** la sesión de módulos y su confirmación los dejan personalizados (CT-PBT1B-02, F-01 con delta vacío);
- **los campos del sobre de CT-PBT3B-11:** son del sobre, no del ensamblador.

**Los que se comparan como estados, no como diseño:** CT-PBT3A-06 y el `Mutator` de CT-PBT3A-05. Ahí no aplica la lente.

**La longitud de módulo de CT-PBT3A-10** sigue sin medir y está declarada (l.948). No falta ningún gesto en la tabla.

### 10.4 `PostPeralte` 3,25 y 4,25 (V44-03)

**Los admite y los conserva:**
1. **El estado los pasa:** `PostPeralte = PostPeralte` en `BuildInputs` (`PushBackEditableState.cs:382`).
2. **El ensamblador no los ajusta:** `ApplyPostPeralte` solo ignora los ≤ 0 y asigna (`DynamicRackSystemBuilder.cs:179-191`). Con un fondo igual no hay reconstrucción, y se aplica sobre la copia de la línea base (`PushBackEditorDesignAssembler.cs:294`).
3. **El diseño los toma de la entrada:** `design.PostPeralte = postPeralte` (`DynamicEditorDesignAssembler.cs:105`).
4. **El documento los escribe:** `PostPeralte = design.PostPeralte > 0.0 ? design.PostPeralte : null` (`DynamicRackSystemDocument.cs:159`). La base, con 0, sigue saliendo `null`, como en los fixtures.

**Nada los compara con el catálogo:**
- Ningún uso de `PostPeralte` en Application ni en Domain lo ajusta a una lista de peraltes (`git grep`). `DynamicFrontGeometry.PostPeralte` devuelve el del sistema si es positivo (`:605-616`).
- La única validación del resolver es la de un negativo (`DynamicRackSystemResolver.cs:469`).
- F2 ya mide que conserva un valor no redondo (3,00044).

**Conclusión:**
- **No son gestos inertes.**
- **Los dos casos:** 3,25 y 3,25 frente a `null` dan `Overlapping`; 3,25 y 4,25, también.
- **Como tokens:** `3.25` y `4.25` no se confunden con otros números de la clase. `4.5` no es subcadena de `48.5`, de `4.25` ni de `3.25`, y la regla del token excluye los dígitos pegados.

### 10.5 DU-23, DU-24 y DU-25

**DU-23, de acuerdo: se custodian redactados.**
- **Por qué:** el informe, que es autoridad, cita los guiones como su método, y sin ellos no se puede repetir.
- **Lo que hay que redactar:**
  - `REPO` en `cat3a_compare.py`, `cat3a_c17.py`, `cat3a_custody.py` y `cat3a_fixtures.py`: pasa a `<checkout-principal>`, y `local_paths` lo exige al escribir.
- **Lo que ya no hace falta:**
  - **`cat3a_bounded.py`** ya no lleva fragmentos del nombre de la usuaria (§10.1), así que la nota de DU-23 sobre él queda sin objeto;
  - **`cat3a_v44.py`** no lleva ruta.
- **Fuera del revisor:** el encargo de la sesión, `independent-review-pbt3a-v43-check-prompt.md`, lleva una ruta con letra de unidad y la carpeta del usuario. Lo redacta la sesión, y `local_paths` lo exige.
- **Lo que falta:** que `CUSTODY_EXTRA` gane `cat3a_v44.py` (M-05).

**DU-24, de acuerdo:**
- lo que va entre paréntesis, fuera de las comillas invertidas, es referencia y no literal;
- las 28 rutas y las 27 líneas van por referencia (§10.2, M-03).

**DU-25, de acuerdo: `--bounded-file` es seguro.**
- **Cuándo vale:** solo con `--dry` y `--objects-rev` (`make_pbt3a_contract.py:134`).
- **Si el informe ya está custodiado:** tiene que ser igual al local, salvo los fines de línea (`:1915-1917`).
- **Al escribir,** solo cuenta el custodiado.
- **No filtra ninguna ruta:** el seco imprime solo el nombre del archivo (`:1921`).

### 10.6 Decisiones del Owner

**Ninguna nueva.**
- **La v4.4 es solo de texto:** dos recetas, una regla del §5.0, la ayuda de RR-3 y la evidencia.
- **No cambia** el diseño, D-1, ninguna obligación ni ninguna parada.

### 10.7 Hallazgos nuevos

| Id | Sev. | Dónde | Hallazgo | Qué hacer |
|---|---|---|---|---|
| M-05 | M | `CUSTODY_EXTRA` del generador; DU-23 | **El problema:** esta sección cita un guion nuevo, `cat3a_v44.py`, que no está en la custodia. `cat3a_bounded.py` cambió: toma el paquete por argumento y ya no lleva fragmentos del nombre de la usuaria | Añadir `cat3a_v44.py` a `CUSTODY_EXTRA` (pasan a 27). Custodiar `cat3a_bounded.py` en su versión de esta ronda. Quitar de DU-23 la redacción a mano de nombres en `cat3a_bounded.py` |

**Lo que el generador hace después, sin que sea hallazgo:**
- poner `BOUNDED_RESULT` al literal final;
- volver a pasar el seco y las pruebas en negativo, porque el literal entra en `IssuedBy` y cambia el sha del contrato;
- y custodiar este informe con la §10.

### 10.8 El `BOUNDED_RESULT`, comprobado

`cat3a_bounded.py` aplica la `bounded_check` de `make_pbt3a_contract.py` (`14739806…`) a este informe entero, con la §10, al literal final y a las líneas de la v4.4.

**Lo que da:**
- **0 errores:**
  - el primer veredicto, «EMITIBLE TRAS CORRECCIONES. B 0 / I 1 / M 4.»;
  - la fila `| I-01 | I |`;
  - la fila `**CA-T3a I-01**` del registro de la v4.4;
  - «corregidas en el paquete»;
  - y la §10, emparejada con su veredicto en la forma de PB-T3, con 0 bloqueantes y 0 importantes, que el literal nombra como «recomprobacion v4.4: EMITIBLE, B 0 / I 0 / M 1».
- **El literal** es ASCII y empieza por «EMITIBLE TRAS CORRECCIONES;».
- **El informe** no lleva rutas locales.
