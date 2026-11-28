> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-TC (correcciones declaradas), v2.1

**Estado:** v2.1, la versión que se emite. La prepara la sesión principal el 2026-11-17 a partir de dos fuentes:
- las decisiones del Owner, transcritas literales en `owner-pbtc-decisions.md` (**[OPD]**, F4B-E06);
- las correcciones N-01 a N-10 de la comprobación acotada de la v2 (`independent-review-v2-check.md`, **ICT2**: EMITIBLE TRAS CORRECCIONES, 0/1/9).

No declara GATE PASS. Como la corrección D-PB-11 entra en el cuerpo, esta v2.1 necesita su propia comprobación acotada antes de emitirse (precondición 2).

**Patrón:** el paquete del contrato de PB-T0, `docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md` (**[PK0]**), y su contrato emitido, `docs/automation/evidence/I-64-pilot/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json` (**[PK0-C]**).
- PB-TC **no** es una caracterización por mutantes.
- Su RED son pruebas nuevas que fallan por aserción en la base, y su GREEN es la corrección del producto (la cadena ordinaria de F4, como F4-T3B1).

**Lo que desarrolla:**
- D-PB-06 (a) y (b) y D-PB-08 de [OD], y la corrección declarada nueva **D-PB-11** ([OPD], pregunta 4: H-PB-28, «Restaurar estándar» llega al Actualizar);
- la fila de PB-TC del §5 de [P] y OB-06.1 a OB-06.6, OB-08.1 a OB-08.4, OB-T1B.1 a OB-T1B.3 y OB-RE.1.

**Base:** rama `architecture/workspace-persistente-rackcad` en `f9f2afbb`, con `main` en `812baf87`.
- `git diff 5ccaa770 f9f2afbb -- src` y `git diff f2eedd9a f9f2afbb -- src` están vacíos: el producto es el de `5ccaa770`.
- `tests/` añade a esa base los 10 archivos de PB-T0.
- Todo el §2 está medido ahí, en solo lectura y sin compilar. El detalle está en `pbtc-measurements.md` (apartados A a J de la v1, K de la v2 y L de la v2.1), que se custodia con este paquete.

**Precondiciones de emisión:**
1. **Las decisiones del Owner,** ya tomadas. Las cumple `F4B-DRAFTS/coordinator/owner-pbtc-decisions.md`, que se custodia en el commit de emisión y se anota como F4B-E06 en `f4b-dispositions.md`.
2. **Las comprobaciones acotadas:** la de la v2 (ICT2, EMITIBLE TRAS CORRECCIONES 0/1/9, con N-01 a N-10 aplicadas aquí) **más** una comprobación acotada de esta v2.1, cerrada con 0 bloqueantes y 0 importantes sin resolver. La segunda es necesaria porque D-PB-11 entra en el cuerpo.
3. **Custodia en el commit de emisión,** que es la `AuthorityRevision`:
   - este paquete, en `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md`, con `pbtc-measurements.md`;
   - [OPD], en la ruta de la precondición 1;
   - el registro declarado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, creado por la sesión con las filas S-01 a S-04 en estado DECLARADA (§5.4);
   - el contrato y su script generador, en `docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/`. El script sigue `make_pbt0_contract.py` y comprueba que existe cada ruta prohibida, que ninguna ruta permitida queda bajo un prefijo prohibido y que toda ruta de `src/` no permitida queda cubierta;
   - **las anotaciones en `f4b-dispositions.md`:**
     - las obligaciones de PB-T2: probar en modo WORKSPACE el paso 4 de D-PB-06 (OB-06.3) y el aviso de D-PB-08 (OB-08.2);
     - el caso pendiente de H-31: vuelve al Owner antes del contrato de PB-T1 (OB-06.5; [OPD], pregunta 4; §10).
4. `git diff f9f2afbb <BaseSha> -- src tests` vacío. Si no lo está, se vuelve a medir el §2 (PBTC-C07).
5. Las celdas del §6.8 no están `STALE`; el `config.toml` coincide con la Snapshot I; el binario de Codex es el acreditado.

## Registro de cambios v2 → v2.1

| Origen | Qué cambió | Dónde |
|---|---|---|
| **N-01** (importante: el paso 4 de D-PBTC-01 contradecía INV-PBTC-03 (1)) | Opción (a). Las llamadas `SetNumber` de W:386-406 **no cambian**. Cada portador se asigna en una línea propia con la misma expresión que pinta su `SetNumber`. El fondo no necesita portador nuevo: lo es `generalPallet.Depth` (W:385). INV-PBTC-03 (1) queda tal cual | D-PBTC-01, paso 4; H-33 |
| **N-02** (la clase `CATALOGO` en todas las filas) | `CATALOGO` existe solo en la fila F4 (`Structure.BeamDepth`). Las demás filas se clasifican como en CT-PB0-19. El método de CT-PBTC-01 pasa a `CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading`. Mapa CT → método nuevo | §4.1; CT-PBTC-01; §4.4; §5.1; §5.2 |
| **N-03** (INV-PBTC-03 (2) contaba once cajas) | «diez llamadas `Keep(` sobre esas diez cajas, y el cambio de `OptionalVal` (W:3625-3626)» | INV-PBTC-03 |
| **N-04** (D-PBTC-10 y PBTC-C14) | PBTC-C14 añade K4 (coordenadas con más de 0,001): es la fórmula completa de CT-PB0-21. D-PBTC-10 dice «los cambios temporales». La corrida temporal usa `Assert.Fail(texto)` | D-PBTC-10; PBTC-C14; §6.6; R-15 |
| **N-05** (predicción de la base de CT-PBTC-10) | «al menos una línea» por fila; F6 da dos | CT-PBTC-10; §5.1 |
| **N-06** (anclas del anexo B) | Corregidas antes de incorporarlo: las líneas que no cambian son `:199-201` y `:204-206` (A0 salida, A0 controles y A1 salida) | CT-PBTC-14; S-04 |
| **N-07** (K-6 no exhaustivo) | Sin el anexo A ya no es pertinente. Lo medido se conserva, y la medición de las cajas de H-31 queda para la vuelta al Owner antes de PB-T1 | `pbtc-measurements.md`, L.1; §10 |
| **N-08** (M3 = 56 en CT-PBTC-12) | Decae: CT-PBTC-12 no entra (OD-PBTC-5 = (B)) | `pbtc-measurements.md`, L.1 |
| **N-09** (ancla de D-PBTC-09) | Pasa a `EditorWindowTestSupport.cs:89-95` (`SetText`) | D-PBTC-09 |
| **N-10** (identificadores internos en el §8.0) | El §8.0 resume lo **decidido**, en lenguaje llano y sin identificadores internos. Dice «la escala de textos y cotas» | §8.0 |
| **OD-PBTC-1 = (i), con R-03 (i)** ([OPD], pregunta 1) | Decidido: corregir con la regla `Keep` de Cantilever y la extensión declarada (selección de celda, «Ambos lados», alcance, datos del frente y biblioteca). R-03 (ii) se descarta. PBTC-C14 se conserva: cubre las dos filas de CT-PBTC-10 donde la ventana reconstruye (F1 y M-02), porque [OPD] dice «si el delta de la base de CT-PBTC-10 es material» | §3 D-PBTC-01; CT-PBTC-03; PBTC-C14; §8 |
| **OD-PBTC-2 = (i)** ([OPD], pregunta 2) | Decidido: corregir | §3 D-PBTC-02; §8 |
| **OD-PBTC-3 = texto corto en L2** ([OPD], pregunta 3) | El literal queda fijado byte a byte como en [OPD]. L1 queda solo como alternativa descartada, en una línea. El texto largo se retira | D-PBTC-03; CT-PBTC-07; CT-PBTC-08; INV-PBTC-05; PBTC-C13; §8 |
| **OD-PBTC-4 = (A)** ([OPD], pregunta 4) | El anexo B entra en el cuerpo como **D-PB-11**, con N-06 aplicada antes. Lo que añade: <br>• el diseño (D-PBTC-11), CT-PBTC-14 y la sustitución S-04 de CT-PB0-05, con su `Skip` y sus aserciones vieja y nueva; <br>• las rutas: `AllowedWriteScope` 13 y `ForbiddenWriteScope` 130; <br>• `RequiredTests`: el filtro con `ExpectRed` en 10, PB-T0 en 16 y tres filtros de guardas nuevos; <br>• INV-PBTC-19, la evidencia, las paradas y el registro; <br>• las cifras del §6.9. <br>El `Objective` nombra D-PB-11. Se corrige todo «H-PB-28 queda fuera» (D-PBTC-07, INV-PBTC-15, PBTC-C10) | §1; §2 (H-35 a H-37); §3; §4; §5; §6; §7; §8 |
| **OD-PBTC-5 = (B)** ([OPD], pregunta 4) | El anexo A se retira. Una sección corta (§10) registra el caso de H-31, que vuelve al Owner antes del contrato de PB-T1 (OB-06.5). Sus cajas y métodos quedan fuera del alcance **por la letra** en INV-PBTC-15 y PBTC-C10, y el registro los lista como «pendiente: decisión del Owner antes de PB-T1». Los números CT-PBTC-11 a 13 no se usan | D-PBTC-07; §4; §5.4; §6.5; §6.7; §10 |
| **Precondiciones** (encargo, punto 6) | La 1 queda cumplida por [OPD]. La 2 pide ICT2 más una comprobación acotada de esta v2.1 | Cabecera |

## Registro de cambios v1 → v2 (conservado)

| Hallazgo de IRT1 | Qué cambió | Dónde |
|---|---|---|
| **B-01** (F4 no es una pérdida de la ventana) | `Keep` se aplica también a `BeamDepthBox`, por uniformidad, y es inocuo. F4 fija su salida real (`salida=6`) con la clase nueva `CATALOGO`. Son **diez** los campos que redondean. S-03 sin `Skip` falla en 10 filas, y en F4 sigue pasando | §2: H-02 y H-29 nuevo; H-PB-42. §4.1 (clase `CATALOGO`). CT-PBTC-01, CT-PBTC-02 (i), CT-PBTC-10. §5.1, §5.2, §5.4. INV-PBTC-03. OD-PBTC-1 y §8.0 |
| **I-01** (`Val` sigue usado en W:1865) | `Val` se conserva. Se borra el paso 5 de D-PBTC-01. Solo el anexo A la deja sin usos | D-PBTC-01; anexo A, A.1 |
| **I-02** (efecto no medido de F1 en la reconstrucción de módulos) | CT nuevo, **CT-PBTC-10**, en el filtro con `ExpectRed`: la paridad ventana–piloto en F1-F11 y en `pb-02-modulos` con el fondo más 0,00044. Su delta de la base se mide en la cadena y se presenta como **no medido** hoy. Hay obligación de transcribirlo y una parada nueva, **PBTC-C14** | §2: H-30, H-PB-43 y H-PB-45. §4.2 (CT-PBTC-10). §5.1. §6.4. §6.6. §6.7 (PBTC-C14). §6.9. OD-PBTC-1 |
| **I-03** (`GapBox`, `ModuleLengthBox` y las cajas avanzadas siguen redondeando) | Se registra en el §2 y en el registro. Se especifica completo como **anexo A** condicional, y va al Owner como **OD-PBTC-5** | §2: H-31 y H-PB-44. §5.4. Anexo A. OD-PBTC-5 y §8.0 |
| **I-04** (el paso 3 de CT-PBTC-03 no probaba R-03) | El paso 3 asigna un texto distinto que se ve igual, `61.2350`: dispara `TextChanged`. Lo esperado es `c00=61.23456` | CT-PBTC-03; §5.1; R-03; H-32 |
| **M-01** (fidelidad de OD-PBTC-1 y OD-PBTC-2) | Las frases sin medir se escriben como no medidas: R11 y R7 en la planta. Se declara la extensión más allá de la «ruta de carga». Se añade la sub-decisión R-03 (i)/(ii). En OD-PBTC-2: RACKEDITAR deja de borrar el estilo del sobre, y la biblioteca e «Insertar» lo conservan | OD-PBTC-1; OD-PBTC-2; §8.0 |
| **M-02** (el aviso en la barra lateral) | La v2 se diseña con **L2**, el aviso en la banda de estado, que respeta la regla del shell. L1 queda descrito como alternativa. Se matiza «vale en los dos modos» | D-PBTC-03; H-PB-46; CT-PBTC-07 y CT-PBTC-08; INV-PBTC-05; OD-PBTC-3 |
| **M-03** (`ForbiddenWriteScope` sin el método de PB-T0) | Hermano por hermano, como PB-T0: **131** entradas (130 con el anexo B) | §6.3 |
| **M-04** (CT-PBTC-09 decorativo) | **CT-PBTC-09 retirado,** con su archivo. La prueba de OB-06.6 la dan INV-PBTC-01 y el filtro de Cantilever | §4.2 (hueco con nota); §4.3; §6.2; §6.4; §6.9 |
| **M-05** (H-08 inexacto) | H-08 se corrige con `MigrateFirstLevelDatum`. La frase del §8 sobre los racks de tres decimales lo excepciona | H-08; H-PB-47; OD-PBTC-1 |
| **M-06** (anclas) | H-12 distingue lecturas y asignación. H-18 retira la guarda de foco saltada como guarda activa y añade las cuatro que revisó IRT1. En L2, la banda de estado | H-12; H-18 |
| **M-07** (trazabilidad y vínculos con PB-T2) | Filas del §4.3 para los pasos 3 y 6. Punto informativo en el §8. La anotación de las obligaciones de PB-T2 pasa a precondición | §4.3; precondición 3; §8, punto informativo; §8.0 |
| **M-08** (formatos sin definir) | Cada formato, definido en el §4.1; el literal del aviso, en un bloque de código | §4.1; D-PBTC-03 |
| **M-09** (las corridas temporales sin artefactos) | Sus TRX y sus logs se guardan como artefactos y se citan en `worker-handoff.json` | §5.2; §6.6; INV-PBTC-13 |
| **M-10** (INV-PBTC-03 e INV-PBTC-15 no mecánicos) | Reformulados como comprobaciones del diff: una lista cerrada de miembros y la ausencia de líneas `SetNumber(` en el diff | INV-PBTC-03; INV-PBTC-15 |

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| [OPD] (`F4B-DRAFTS/coordinator/owner-pbtc-decisions.md`, F4B-E06) | Las cinco decisiones, transcritas literales: OD-PBTC-1 (i) con R-03 (i) y la extensión declarada, conservando la vuelta por PBTC-C14 (pregunta 1); OD-PBTC-2 (i) (pregunta 2); OD-PBTC-3, el texto corto en la franja de estado y su literal byte a byte (pregunta 3); OD-PBTC-4 (A) = D-PB-11, y OD-PBTC-5 (B), el caso de H-31, de vuelta al Owner antes del contrato de PB-T1 (pregunta 4) |
| [OD] (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | D-PB-06: «ACCEPT THE PRODUCT CORRECTIONS, BUT NOT AS AN INCIDENTAL REFACTOR» y sus seis pasos (`:130-150`), con la vuelta al Owner si una corrección altera la geometría «materially» (`:153-155`). «prove both: Workspace; RACKEDITAR» (`:144-147`). D-PB-08 (`:62-74`). D-PB-10 (`:233-236`). T1B (`:237-243`). RACKEDITAR (`:244-248`) |
| [P] (`docs/initiatives/I-64-f4b-push-back-completo-package.md`, v3.1) | OB-06.1 a OB-06.6 (`:369-398`); OB-08.1 a OB-08.4 (`:436-440`); la fila de PB-TC y «Por qué PB-TC va antes de PB-T1» (`:502`, `:509-513`); la fila de PB-T2 (`:504`); OB-T1B.1 a OB-T1B.3 (`:700-710`); OB-RE.1 (`:722`); R-PB-07 (`:734`); §9 (`:740-750`); §10 (`:766`) |
| [DP] (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | La replanificación única por la comparación literal (pregunta 2, su respuesta y la descripción de la opción). Los sustitutos de nc1 y nc2 solo con la firma EXACTA de PD-F4-04 y PD-F4-01 (pregunta 1, ídem) |
| `F4B-DRAFTS/f4b-dispositions.md` | F4B-E01 a F4B-E05, y F4B-E06 al emitir. En F4B-E05, «Siguiente»: la vuelta al Owner antes de emitir PB-TC, ya hecha ([OPD]) |
| El cierre de PB-T0 (`F4B-PBT0-CHARACTERIZATION/task-close-package.md`, §5, `:79-107`) | Los arrastres a PB-TC (M-05) y la vuelta al Owner. Las lecciones: «el prompt del Worker no puede dar latitudes que el contrato no tiene» (`:107`) y guardar la salida de cada medición (M-06, `:93`) |
| La entrega del Worker de PB-T0 (`…/1/R20261117T185616Z-3c1b/worker-handoff.json`, Evidence 0, 24, 31 y 32) | El mapa CT → método, M10, T-3 y T-4 |
| La revisión independiente de PB-T0 (`…/1/R20261117T215307Z-3cff/independent-review.md`, §8.3 y §9) | OWNER-2 (H-PB-28) y OWNER-4 (la lectura de T-3) |
| IRT1 (`independent-review-v1.md`) e ICT2 (`independent-review-v2-check.md`) | Los 15 hallazgos de la v1 y N-01 a N-10 de la v2, resueltos en los dos registros de cambios |
| El bloque M (`F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md:29`) | M-7: 0 de 22 sobres con `DimensionStyle` |
| T1B (`docs/initiatives/I-64-f4-t1b-gate-contract-package.md`) y su contrato | La caracterización vigente de la ventana clásica, que sigue en verde (OB-T1B.3) |
| ADR-0029 D13 (`docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md:172-180`) | «la caracterización no se edita para que pase»; un cambio de comportamiento exige decisión del Owner |
| El patrón de sustitución de I-39 (`docs/automation/evidence/I-39B-caracterizacion-base-vs-contrato.md`; `tests/RackCad.UI.Tests/RichEditorCharacterizationTests.cs:45-47`) | La prueba vieja queda con `Skip` y un motivo que nombra el registro. El registro anota la base, el contrato, la autoridad, la aserción literal y la nueva |
| `docs/AUTOMATION_PLAN.md` §16.5, §16.8, §16.9 (`:600-646`) y §16.11 | A1-A8; RED de la cadena; las 14 comprobaciones; las paradas |
| Las fuentes de las firmas (`F4-DRAFTS/f4-dispositions.md:520`, `:562`, `:1103-1106`, `:1540`, `:1657-1660`; `F4-DRAFTS/coordinator/owner-f4t3-authorization.md:255-265`) | PD-F4-01 y PD-F4-04, transcritas en PBTC-C11 |
| `docs/automation/agent-execution/model-catalog.md` (`:14`, `:48-61`, `:94-108`) | Celdas vigentes |

## 2. Hechos medidos en `f9f2afbb`

**Alias:**
- **W** = `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs` (3630 líneas);
- **WC** = `….Composite.cs` (911);
- **WX** = `….xaml` (792);
- **NF** = `src/RackCad.UI/Controls/NumericField.cs` (213);
- **DA** = `src/RackCad.Application/Systems/PushBack/PushBackEditorDesignAssembler.cs`;
- **DEA** = `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs`;
- **CS** = `src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs`;
- **ES** = `src/RackCad.Application/Systems/PushBack/PushBackEditorState.cs`;
- **WLC** = `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`;
- **S0** = `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationSupport.cs` (el ayudante de PB-T0).

### 2.1 La pérdida (a): dónde se siembra y dónde se relee

- **H-01. El formato.** `SetNumber(double? value, string format = "0.###")` escribe `Text` con la cultura actual (NF:129-136), y `Value` sale de volver a validar ese texto en cada `TextChanged` (NF:138-163).
- **H-02. Las once cajas de F1-F11** (la tabla de M10 de PB-T0, [PK0]:410-421) se siembran con exactamente once llamadas, todas con el formato por defecto:
  - las del rack, en `LoadFromModel`: `DepthBox` (W:386), `ToleranceBox` (W:389), `PostPeralteBox` (W:391), `BeamDepthBox` (W:392) y `AnnotationScaleBox` (W:406);
  - las del frente y la celda, en `LoadSelectedFront`: `FirstLevelHeightBox` (W:454), `CellPalletFrontBox`, `CellPalletHeightBox` y `CellPalletWeightBox` (W:456-458), `CellClearBox` (W:460) y `CellBeamLengthOverrideBox` (W:463).

  Ningún otro código escribe su `Text`. `SetMixed` vacía cinco de ellas con `SetNumber(null)` en el modo «Ambos» (WC:540-565, `:556`).

  **De las once, diez pierden su valor exacto** por la caja. La undécima, `BeamDepthBox` (F4), no: el ensamblador normaliza el peralte IN/OUT al catálogo (H-29).
- **H-03. Dónde se releen las once.** Solo en dos métodos:
  - `ReadInputs` (W:812-857): `Val(DepthBox, …)` (W:819), `PostPeralte`, `PalletTolerance` y `BeamDepth` (W:837-839) y `AnnotationScale` (W:845);
  - `ReadCellValues` (W:781-810): `FirstLevelHeight` y las cuatro de la celda (W:797-801) y `OptionalVal(CellBeamLengthOverrideBox, …)` (W:804).

  `Val` es `field.Value ?? fallback` (W:3594). Además de esas diez llamadas, **`Val` se usa en W:1865** (`ModuleLengthBox`, H-31). `OptionalVal` es `field.Value ?? (IsMixed(field) ? sideValue : null)` (W:3625-3626).
- **H-04. Quién llama a esas lecturas:**
  - `ReadInputs`, solo `Recompute` (W:646);
  - `ReadCellValues`, `CommitCurrentCell` en cada pasada (W:621, W:737-741), el alcance (W:1306) y los datos del frente (W:1470).

  Las once cajas usan `LostFocus="Input_Changed"` (WX:68, :103, :106, :114, :278, :312, :342, :345, :347, :351 y :354), que es `RequestRecompute()` (W:1207).
- **H-05. Las cajas espejo no se releen.** `FrontBox`, `PalletHeightBox` y `WeightBox` (W:558-564) solo muestran y se validan (W:728-734). Frente, alto y peso del rack viajan en `generalPallet` (W:94-101, W:818-820), y por eso G1 a G3 conservan su valor (CT-PB0-20). `MirrorSelectedCellPallet` (W:674-677) pinta esas cajas espejo bajo `suppressSync`.
- **H-06. Portadores que ya existen en la ventana:**
  - `generalPallet` (W:101);
  - `advanced`, «the SAME transport type the assembler consumes» (W:103-106);
  - `firstLevelDatum` (W:372);
  - el par `loadedDimensionViews` y `dimensionViewsTouched` de I-50, que conserva la política cargada exacta mientras el usuario no toque las casillas (W:1211-1257).
- **H-07. Las cargas pasan todas por `LoadFromModel`** (W:377-427):
  - `LoadNew` (W:262-281);
  - `LoadDesignForNew`, la biblioteca (W:286-299);
  - `LoadExisting`, RACKEDITAR (W:303-316);
  - `Restore_Click`, «Restaurar» (W:3267-3290).
- **H-08. Quién reescribe la altura del primer nivel.**
  - **El resolver no:** el ajuste al troquel va a `LoadBeamLevels` (`DynamicRackSystemResolver.cs:104-114`); el snapshot devuelve `front.FirstLevelHeight` tal cual (`:375-383`), y `BuildFrontDesigns` copia `row.FirstLevelHeight` (`DynamicFrontMatrix.cs:449-462`).
  - **La carga sí, en un caso:** un documento **sin marcador de datum** pasa por `MigrateFirstLevelDatum` (`PushBackEditorState.Load.cs:91-124`). Cada frente pasa a guardar la distancia medida desde la elevación física de su primer larguero hasta el troquel utilizable más bajo (`:118-120`, `RackFirstLevelDatum.ToLowestPunchOffset`). Esa resta puede dar más de tres decimales (H-PB-47).
  - `pb-01-base`, que usan CT-PBTC-01 a CT-PBTC-03 y CT-PBTC-10, trae datum 1 y no se migra.
- **H-09. RACKEDITAR escribe lo que la ventana entrega:** `window.DesignToInsert` y `window.SystemToInsert` (`src/RackCad.Plugin/RackPushBackCommands.cs:168-190`). «Actualizar» es `RequestDraw(null, -1, updateOnly: true)` (W:3292): confirma los módulos pendientes (W:3346-3349), hace la segunda pasada (W:3351), `SetName` (W:3365), `RequestUpdate` (W:3369) y `Close()` (W:3378).
- **H-29. El peralte IN/OUT base no es una pérdida de la ventana:**
  - el ensamblador llama a `DynamicLoadBeamGeometry.ResolveBeamDepth(catalog, DynamicRackDefaults.InOutBeamCatalogId, editorInputs.BeamDepth)` (DA:165-168);
  - `ResolveBeamDepth` devuelve lo pedido solo si está a menos de 1e-6 de un peralte permitido; si no, el primero permitido (`DynamicLoadBeamGeometry.cs:52-71`);
  - `InOutBeamCatalogId = "LARGUERO_IN_OUT_C6"` (`src/RackCad.Domain/Systems/Dynamic/DynamicRackDefaults.cs:30`), que en el catálogo entregado declara un solo peralte, `6` (`assets/catalogs/secciones.csv:8`);
  - el diseño toma `BeamDepth = system.InOutBeamDepth` (`DynamicRackSystemResolver.cs:57`, `:252`, `:311`).

  Con 6,00044 la salida es 6, con la caja o sin ella. CT-PB0-19 lo clasificó `REDONDEA` solo porque R3(6,00044) = 6 coincide con el catálogo, y T-3 R4 («nada») es coherente.
- **H-30. Un fondo con más de tres decimales reconstruye hoy los módulos.**
  - En la primera pasada tras la carga, `MustRebuild` compara el palé de la pasada con el del baseline con tolerancia 1e-6 (`SamePallet`, DEA:44-61; DA:210-213).
  - Hoy el palé de la pasada lleva el fondo redondeado de la caja (W:819). Si el fondo cargado tiene más de tres decimales, la pasada **reconstruye**: `BuildDefault` (DA:216-218) más las intenciones de módulo (DA:315-317), en vez de copiar el baseline (DA:271-274).
  - Con la corrección, el fondo exacto coincide con el baseline y se copia, igual que hace el piloto, que pasa por el mismo ensamblador (`PushBackCompositeEditorAssembler.cs:59`).
  - **Qué cambia hoy esa reconstrucción** en un rack con módulos personalizados **no está medido**. T-3 resolvía cada diseño con `BuildFrom`, sin la decisión de reconstruir de la ventana. Lo mide CT-PBTC-10 en la cadena.
- **H-31. Otras cajas sembradas con «0.###» que se releen sin que el usuario cambie su valor:**
  - **`GapBox`:** se siembra en cada pasada válida (WC:721). `Gap_Changed`, en `LostFocus`, hace `SetGap(GapBox.Value.Value)` sin comparar (WC:159-175, `:173`). Basta con pasar por el campo.
  - **`ModuleLengthBox`:** se siembra al elegir un módulo (W:1793). `ModuleLength_Changed` (W:1852-1873) escenifica `Val(ModuleLengthBox, module.Length)` si difiere en 0,0001 o más (W:1865-1871), y «Actualizar» confirma lo escenificado (W:3346-3349).
  - **Las cajas avanzadas:** se siembran con `SetOptional`, que es `SetNumber` por defecto (W:1566-1571, W:1582-1586). Cualquier `LostFocus` de sus cinco cajas o el clic de su casilla (WX:492, :499, :505-507, :510, :524 y :531) relee **todas** con `ReadAdvancedRackParameters` (W:1592-1607, W:1616-1626). De ellas, cuatro son magnitudes anulables (`ManualHeaderHeightOverride`, `DerivedPostReinforcementHeight`, `DerivedPostHeight` y `SeparatorSpacingOverride`). `SeparatorCountOverride` es entero y `DerivedPostReinforced`, una casilla.
  - **Sin pérdida:** `DerivedPostLineHeightBox` no se siembra desde el modelo; solo lo lee su botón (W:2093). Las cajas enteras (`StructureOverrideBox`, `CellFondoOverrideBox`, `FrontCountBox`, `PositionsBox` y las demás) no tienen decimales.

  PB-T0 midió G4 a G8 solo en la carga (CT-PB0-20). Son casos alcanzables de la misma clase (a). **El Owner decidió no corregirlos en PB-TC** ([OPD], pregunta 4, OD-PBTC-5 = (B)): vuelven a él antes del contrato de PB-T1 (§10).
- **H-32. Un texto distinto que se ve igual valida.** `NumericFieldValidation.Validate` analiza el texto con `LocalizedNumberParser.TryDouble`, con `AllowDecimalPoint` y la cultura actual o la invariante (`src/RackCad.UI/Controls/NumericFieldValidation.cs:76-128`; `src/RackCad.Application/Formatting/LocalizedNumberParser.cs:13-54`). `"61.2350"` en la cultura invariante da 61,235 y es válido (el mínimo de `CellPalletHeightBox` es 0 exclusivo, WX:351-352). Asignar a `Text` un texto distinto del actual dispara `TextChanged` (NF:138-142); asignar el mismo, no.
- **H-33 (nuevo, N-01). Las siembras del rack y el portador del fondo** (medición L-1):
  - en `LoadFromModel`, `generalPallet = pallet` (W:385) es la misma referencia que `inputs.Pallet` (W:384). Las llamadas `SetNumber` del rack (W:386, :389, :391, :392 y :406) pintan cada una una expresión propia sobre `inputs` u `options`;
  - `generalPallet` solo se usa en W:101, W:385 y W:818-820. `PalletSpecification.Depth` tiene `set` (`src/RackCad.Domain/Systems/Shared/PalletSpecification.cs:25`), pero mutarlo mutaría el objeto cargado. Por eso el diseño sustituye la referencia y no la muta (D-PBTC-01).
- **H-34 (nuevo). Una caja vacía de las diez nunca llega a la lectura** (medición L-2):
  - de las once cajas, solo `CellBeamLengthOverrideBox` es opcional (`IsOptional="True"`, WX:354-355);
  - una caja no opcional vacía da `EmptyRequired`, que es un error (`NumericFieldValidation.cs:85-89`), y `AllFieldsValid` (W:706-718) corta la pasada antes de `CommitCurrentCell` y `ReadInputs` (W:611-619);
  - una caja de la celda en modo mixto pasa a opcional y vacía (WC:540-565), y su lectura cae al valor del lado (W:797-801). `Keep` con caja vacía devuelve ese mismo valor (D-PBTC-01).

  Por tanto, `Keep` no cambia el valor de respaldo de hoy en ningún caso alcanzable.

### 2.2 La pérdida (b): el estilo de cotas

- **H-10. La carga lo recupera:** `Annotations.DimensionStyle = s.DimensionStyle` (`src/RackCad.Application/Systems/PushBack/PushBackEditorState.Load.cs:209`).
- **H-11. La ventana lo pierde.** `LoadFromModel` lee `options` (W:402) y no guarda el estilo; `ReadInputs` arma `Annotations` sin él (W:840-848). Un rack nuevo nace sin estilo (`PushBackEditorInputs.cs:33`).
- **H-12. Del diseño al dibujo:**
  - `design.DimensionStyle = options.DimensionStyle` (DEA:121);
  - `system.DimensionStyle = design.DimensionStyle` (`DynamicRackSystemResolver.cs:246`);
  - `DynamicViewDecorations` **lee** `system.DimensionStyle` en `:117`, `:206` y `:285`, y lo **asigna** a cada cota en `DimensionStyleName = style` (`:402`);
  - `PushBackMirror.cs:69` lo copia a las instancias espejadas.
- **H-13. Cómo dibuja el Plugin** (`src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`). Push Back dibuja por aquí: `PushBackSystemDrawService.cs:21`, `PushBackFrontalDrawService.cs:20` y `PushBackPlantaDrawService.cs:19`; y `AppendDimension`, desde `:289-291`.
  - **Con un estilo nombrado que existe en el dibujo:** la cota usa ese estilo tal cual, sin sobrescrituras (`:353-356`, `:364`, `:384-390`).
  - **Sin estilo, o con uno que el dibujo no tiene:** el estilo actual del dibujo (`:393`) con sobrescrituras: `Dimscale` 1, `Dimtxt` = alto de texto escalado, `Dimasz` 0,7×, `Dimexe` 0,4×, `Dimexo` 0,4×, `Dimgap` 0,3×, `Dimtad` 1 y `Dimdec` 2 (`:366-374`).
  - **Lo que no depende del estilo:** los puntos, la línea de cota y su desfase, y la capa (`:343-359`).
- **H-14. Sin cotas no hay estilo que aplicar.** `DimensionDetail.None = 0` (`src/RackCad.Domain/Systems/Shared/DimensionDetail.cs:7-10`), y con `None` el detalle efectivo es `None` en todas las vistas (`src/RackCad.Application/Systems/Shared/DimensionViewPolicy.cs:34-41`).

### 2.3 D-PB-08 y las guardas que mira un elemento nuevo

- **H-15. La casilla, su sección y la banda de estado:**
  - `SideBPresentCheck` (WX:127-131);
  - `CompositeSection`, colapsada salvo con B declarado (WX:135; WC:65-71);
  - `SlotPresentText`, el texto por celda dentro de la sección (WX:166-167; WC:577-615);
  - **la banda de estado** del shell, `StatusContent` (WX:677-686): un `StackPanel` con `PreviewSummary`, `PreviewHint` y `StatusText` (WX:681-685). La regla del shell, en el comentario, dice «there is no second status inside the sidebar» (WX:678-679).
- **H-16. La regla** (CS):
  - declarar B por primera vez inicializa el lado B entero en blanco (CS:352-386);
  - un lado B dormido vuelve con su presencia (CS:364-374);
  - `IsSlotPresent(B, s)` es `false` con B sin declarar (CS:428-437);
  - son públicos `SideB` y `Of` (CS:164, :269);
  - `BuildSideB` devuelve nulo sin `SideBPresent` o sin ninguna ranura presente (CS:1024-1047), así que el diseño sale sin lado B exactamente en el caso del aviso (IRT1 §2.4).

  Medido en PB-T0 en la CI de HEAD:
  - en `pb-01-base` (CT-PB0-09, `PushBackFullEditorCharacterizationCompositeTests.cs:163-206`), declarar B da `B=3:000` y `composite=False`; con una ranura de B activa, `True`; con B otra vez en blanco, `False`;
  - en `pb-20-ab` (CT-PB0-07, `:97-127`), declarar B tras retirarlo da `composite=True`, porque B estaba dormido con sus frentes; todas las ranuras de B en blanco dan `composite=False` con la casilla marcada.
- **H-17. Dónde se evalúa el aviso:**
  - `Recompute` cuenta cada pasada efectiva en W:607, después de la salida por `suppressSync` (W:605). Todo gesto que mueve la presencia termina en una pasada: la casilla (WC:101-120), «En blanco» (W:1089-1133 → W:1175-1197), `+`/`-` frente y `FrontCountBox` (W:1071-1081, W:1137-1141), las cargas (W:426) y «Restaurar» (W:3288).
  - La línea de estado **no** sirve para llevar el aviso: `SetFrontActive` la escribe después de recalcular (W:1125-1132).
- **H-18. Guardas medidas frente a un `TextBlock` con nombre en la banda de estado:**
  - **T-KB** (`tests/RackCad.UI.Tests/PushBackClassicCharacterizationKeyboardTests.cs`): las paradas solo cuentan `Control` con nombre, `IsTabStop` y `Focusable` (`:166-179`); `SideStops` es esa lista (`:181-201`), y la banda de estado debe estar vacía de paradas (`:232`). Un `TextBlock` no es `Control`. La comprobación (3) mira solo valores locales de navegación (`:255-277`). **No salta.**
  - **T-SG** (`tests/RackCad.UI.Tests/PushBackClassicCharacterizationSourceGuardTests.cs:165-176`, `:234-248`, `:265-381`): los nueve tokens prohibidos y los literales de dueños, BOM, lote y biblioteca. **No salta** si el código nuevo no contiene ninguno.
  - **I-55** (`tests/RackCad.Tests/I55DeferredProductFixCharacterizationTests.cs:8-15`), **módulos** (`tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:382-455`; prohíbe en WX los nombres del Dinámico), **desviador** (`tests/RackCad.UI.Tests/BlankFrontDesviadorHandoffTests.cs:312-318`), **foco** (la guarda activa es `tests/RackCad.UI.Tests/RichEditorContractTests.cs:36-53`; `RichEditorCharacterizationTests.cs:288` es `[Fact(Skip = BaseEvidence)]`, evidencia de I-39 y no guarda) y **`AnnotationScaleBox`** (`tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotValidationParityTests.cs:82-99`): ninguna mira esas zonas. **No saltan.**
  - **Las cuatro que revisó IRT1:**
    - `PushBackSidebarCompositionTests` (como mucho 9 `WrapPanel` y medianiles de 10; `:141-180`);
    - `PushBackShellAdoptionTests` (exige `PreviewSummary`, `PreviewHint` y `StatusText` bajo `StatusHost` y fuera de la barra lateral; `:127-147`, sin contar hijos);
    - `PushBackClassicCharacterizationResourceTests` (los recursos de la ventana);
    - `UiSystemBoundaryGuardTests` (`x:Class` y URIs de paquete).

    **No saltan.**
  - **Censo de ventanas** (`tests/RackCad.UI.Tests/WindowCensusGuardTests.cs:86-120`): solo tipos `Window`. **No salta.**
  - **G-10** (`tests/RackCad.Tests/Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs:721-797`): una sola `.RequestUpdate(` en W (W:3369). **No salta** sin una nueva.
  - **Las que recorren `src`** (unidades, censo de `Task`): no cubren lo que se añade. **No saltan.**
  - **Las que enumeran textos del panel** (`PushBackAdvancedRackParametersWindowTests.cs:82-84`) usan `Assert.Contains`. **No saltan.**

### 2.4 Las pruebas que fijan hoy la pérdida, y las que siguen válidas

- **H-19. WLC:**
  - la Theory `TheWindowAndThePilotDifferOnlyInTheKnownClasses` (WLC:231-252) recorre 14 filas, con lo esperado en WLC:184-204: pb-06 `DimensionStyle: null -> "STANDARD-ISO"` (`:191`), pb-07 `-> "SIMPLEX"` (`:192`), pb-08 `AnnotationScale: 1.235 -> 1.23456` y `PalletHeight: 61.235 -> 61.23456` (`:193-197`), y las otras 11 vacías;
  - `ClassAIsEvidenced…` (WLC:254-264) solo lee el diccionario estático `Expected`;
  - `…KeepMine…` (WLC:270-313) usa escala 2,25 y tolerancia +2;
  - el corpus (WLC:323-332).
- **H-20. `T19_H1_TheDimensionStyleStillComesOutNull`** (`tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs:306-322`): diseño de un sentido con `DimensionStyle = "ESTILO_PB"` (constructor en `:65-92`); asevera `null` tras la carga y tras desmarcar `DimensionsLateralCheck` (`:319-320`).
- **H-21. PB-T0 en UI:**
  - **CT-PB0-19** (`PushBackFullEditorCharacterizationRoundingLossTests.cs:129-151`): F1-F11 en `REDONDEA`. F4 lo es solo por la coincidencia de H-29. Su clasificador es `ClassOfRow` (`:89-93`): `CONSERVA`, `REDONDEA` u `OTRA`;
  - **CT-PB0-20** (`:153-173`): G1-G9 en `CONSERVA`;
  - **CT-PB0-01** (`PushBackFullEditorCharacterizationStateTests.cs:115-168`): `lineas-retiradas(a/b)=[]` en `:144`, `:147`, `:161` y `:164`;
  - **CT-PB0-02** (`:195-242`): pb-06, pb-07 y pb-08 con `delta=[]`, filtrado entre dos salidas de la misma ventana (`:227-229`); `vistas` es `request.Views.Count` (`:189-191`);
  - **CT-PB0-17** (`PushBackFullEditorCharacterizationCatalogTests.cs:88`): `retiradas(a/b)=[]` (`:128-151`);
  - **CT-PB0-05** (`PushBackFullEditorCharacterizationModuleTests.cs:170-208`): solo los seis avanzados. Choca con D-PB-11 (H-36);
  - **CT-PB0-13 a CT-PB0-15:** deltas entre dos salidas de la misma ventana (`PushBackFullEditorCharacterizationSubdialogTests.cs:34-35`, `:104-160`).
- **H-22. PB-T0 en Core:** CT-PB0-18 (`tests/RackCad.Tests/PushBackFullEditorCharacterizationDimensionStyleLossTests.cs:100`) y CT-PB0-21 (`tests/RackCad.Tests/PushBackFullEditorCharacterizationRoundingEffectTests.cs:254-301`) usan Application, sin ventana. La fórmula de «material» de CT-PB0-21 es `K1 no vacía, o K2, o K3, o K4 no vacía` (`:164`); K4 son las claves con un desplazamiento geométrico de más de 0,001 (`:212`).
- **H-23. El corpus** (`tests/RackCad.Tests/Fixtures/I64`, 17 diseños):
  - solo `pb-08-redondeo` tiene valores con más de tres decimales: `AnnotationScale` y `Fronts[0].Levels[0].PalletHeight`;
  - `pb-08` tiene 3 frentes de 3 niveles, con `Fronts[1].Levels[0].PalletHeight = 60`;
  - solo `pb-06` (`STANDARD-ISO`, `Dimensions = 3`) y `pb-07` (`SIMPLEX`, `Dimensions = 1`) traen estilo;
  - `pb-20-ab`, `pb-21-cola`, `pb-31-v2-modulos` y `pb-01-base` no tienen ni estilo ni valores con más de tres decimales;
  - **módulos personalizados** (medición K-3):
    - `pb-02-modulos`: M1 `HeaderStart` 54 manual, con cabecera propia, y M2 `Separator` **30** manual, cuando el estándar es 48;
    - `pb-07-combinado`: M2 `Separator` 36 manual;
    - `pb-31-v2-modulos`: M3 manual **a la longitud estándar** (48), con cabecera propia;
    - `pb-20-ab` y `pb-21-cola`: el `GAP`.

    Los demás, ninguno. Todos tienen `PalletDepth = 48` y datum 1, salvo `pb-10-sin-datum`, sin datum.
- **H-24. La búsqueda en todo `tests/`:**
  - `DimensionStyle` aparece en 66 líneas, y solo `T19_H1` y WLC fijan la pérdida en la ventana;
  - de las pistas de redondeo, en UI solo WLC y PB-T0 pasan por la ventana;
  - el corpus I64 solo lo cargan en UI PB-T0, WLC y `WorkspaceDraftPilotCanonicalLineEndingTests`;
  - T1B no carga el corpus ni usa valores con más de tres decimales.

### 2.5 D-PB-11: «Restaurar estándar» y lo que lo rodea

- **H-35 (nuevo). El defecto y su punto de arreglo** (mediciones G-1 a G-3, K-7):
  - **El defecto** (T-4 de PB-T0, CT-PB0-05): tras «Restaurar estándar» y «Confirmar», la salida ya muestra reiniciados los seis avanzados, pero los controles siguen con lo cargado, y «Actualizar» vuelve a escribir lo cargado.
  - **La causa:** el ensamblador reinicia los avanzados solo en los inputs de la pasada que consume el «Restaurar estándar» (`standardRestore`, DA:204; `Reset`, DA:300-303). El portador `advanced` y sus controles no se tocan, y la pasada siguiente lo vuelve a copiar (W:830-835).
  - **El disparador:** la ventana de Push Back nunca pasa `forceRebuild`, porque eso es del Dinámico (`RackDynamicSystemWindow.xaml.cs:446-464`). El único disparador es `ModuleCommit.StandardRestoreRequested` (ES:139; `src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs:40`).
  - **Dónde se consume:** en la rama válida de `Recompute` (W:658-692). `moduleOwner.ClearModuleCommit()` (W:682) lo borra, y `moduleOwner` es `composite.SideA` (W:82).
  - **El precedente de arreglo:** «Restaurar parámetros» (`RestoreRackParameters_Click`, W:1628-1634) hace `state.RestoreAdvancedRackParameters(advanced);` y `LoadAdvancedRackParameters();`. El primero es `PushBackAdvancedRackParameters.Reset` (ES:160-161; `PushBackAdvancedRackParameters.cs:148-161`: los seis en nulo, con `DerivedPostReinforced = true`). El segundo repinta los seis controles bajo `suppressSync` con `SetOptional` (W:1555-1579).
- **H-36 (nuevo). CT-PB0-05 por líneas** (`PushBackFullEditorCharacterizationModuleTests.cs`, medición L-4):
  - el atributo `[Fact]` está en `:170`, y el comentario «INCUMPLIMIENTO CARACTERIZADO… NO se corrige», en `:160-168`;
  - los pasos (`:176-193`) usan `Abrir` de S0. Los formatos `Six` y `SixControls` están en `:143-158`;
  - las diez líneas esperadas (`:199-208`):
    - **no cambian** con D-PB-11: `:199-201` y `:204-206` (pb-06 y pb-07, A0 salida, A0 controles y A1 salida);
    - **cambian:** `:202` y `:207` (A1 controles) y `:203` y `:208` (A2 petición).
- **H-37 (nuevo). Quién pulsa «Restaurar estándar» o mira su código** (mediciones K-7 y L-5):
  - en UI, solo CT-PB0-04 (`ModuleTests.cs:124`, que asevera la secuencia de módulos de `pb-31`, con los avanzados por defecto), CT-PB0-05 (`:184`) y `PushBackModuleEditorWindowTests.cs:295` (solo `IsManualOverride`). En Core, las pruebas usan Application, sin ventana;
  - **la guarda «Fact3»** (`tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:381-401`) lee W como texto. Exige `RequestStandardRestore()` y **prohíbe** `forceRebuild:` y `forceRebuild)` en W, también en comentarios. Leer la intención `StandardRestoreRequested` no la hace saltar; nombrar el mecanismo con esa forma, sí.

### 2.6 CI e infraestructura

- **H-25. La CI de HEAD** (run `44188678597`, medida con `gh run view`): `push` sobre `f9f2afbb`, los cuatro trabajos en `success`.
  - **Core:** 19 070 Passed de 19 070.
  - **UI:** 2 404 Passed, 17 Skipped, 2 421 en total.
- **H-26. Un Theory saltado cuenta como un resultado.** En UI hay 17 atributos `Skip`, uno de ellos un `Theory` de 4 filas (`BoundedEditorCharacterizationTests.cs:161`), y la CI da 17 Skipped.
- **H-27. Recuentos estáticos** (por atributo, sin ejecutar):
  - `PushBackDimensionViewsWindowTests`: 13 casos;
  - WLC: 17 casos;
  - las tres clases de Cantilever en UI: 67 casos;
  - `PushBackBlankFrontAuthorityTests` (Core): 9;
  - T1B: 55 en UI y 10 en Core (cierre de PB-T0, `:22`);
  - las seis clases de PB-T0 en UI: 18 `[Fact]` (medición L-6);
  - `PushBackAdvancedRackParametersWindowTests` (UI): 9; `PushBackModuleEditorWindowTests` (UI): 13; `PushBackModuleEditorCharacterizationTests` (Core): 15. Ninguna con `Skip`, `Theory` ni `InlineData` (medición L-6).
- **H-28. Ayudantes y costuras existentes:**
  - S0 da:
    - `Ventana()` (`:63-68`), `Cerrar` (`:71-77`) y `Sta` (`:42-58`);
    - `Fixture` (`:93-98`), `EditInner` (`:108-115`) y `Persist` (`:122-123`);
    - `Salida` (`:159-162`) y `Delta` (`:175-185`);
    - `Field`, `Check`, `Combo` y `Btn` (`:327-330`), `StatusText` (`:331`), `Click` (`:333`), `SetNumberR` (`:336-341`), `ValueOf` (`:344`) y `SelectModule` (`:347-352`).

    Su `Abrir` (`:126-133`) deja viva la ventana si `LoadExisting` lanza (M-01 de PB-T0).
  - La ventana:
    - `Session` (W:157) y `EditorStateForTest` (W:1529);
    - `LastComputation` y `CurrentInputsAreValid` (W:182-184);
    - `SelectMatrixCell` (W:1047, ya usada en `PushBackCellConfigurationWindowTests.cs:104` y `BlankFrontEditorTests.cs:330`);
    - `PendingWork()` (W:3417);
    - `CompositeState` (WC:43).
  - `EditorWindowTestSupport`: `SetText` (`:89-95`, ya usada por CT-PB0-11), `SetFrontBlank` (`:67`) y `Descendants`, ya usada sobre las ranuras del shell en `PushBackDimensionViewsWindowTests.cs:137` y en T-KB.
  - La colección `EditorDiscardPromptCollection` (`tests/RackCad.UI.Tests/EditorDiscardPromptCollection.cs:27-30`).
  - xUnit 2.9.2 en los dos proyectos de pruebas (`tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj:20`).

### 2.7 Hallazgos nuevos (continúan la numeración de [P] §2.1)

| Id | Hallazgo | Fuente | Consecuencia |
|---|---|---|---|
| **H-PB-34** | **Cantilever no usa el formato por defecto de `NumericField`.** Sus 33 llamadas a `SetNumber` pasan un formato explícito: `"0"`, o una constante propia `NumberFormat = "0.###"` (`RackCantileverWindow.xaml.cs:37` y sus tres componentes). El valor por defecto solo lo usan las 29 llamadas de Push Back (24 en W y 5 en WC) y `EditorWindowTestSupport.SetNumberAndCommit` (`:106`) | `git grep "SetNumber("` | La corrección no toca `NumericField` (D-PBTC-01) |
| **H-PB-35** | **El repositorio ya resolvió esta misma pérdida en Cantilever:** `Keep` (`RackCantileverWindow.xaml.cs:368-384`), usado como `design.X = Keep(Box, design.X)` (`:333-361`). Lo fijan `CantileverEditorWindowTests.cs:214` y `:234` | Lectura | Es el precedente del diseño de (a), y la regla que eligió el Owner (R-03 (i)) |
| **H-PB-36** | **La pérdida (a) no es solo de la carga:** la selección de celda, «Ambos lados», el alcance y los datos del frente releen las cajas (W:1047-1067, W:1306, W:1470; WC:420-478) | Lectura | El diseño cubre la siembra y la relectura: la extensión que el Owner aceptó ([OPD], pregunta 1) |
| **H-PB-37** | **El panel del Workspace ya escribe hoy los valores exactos y el estilo:** el piloto los conserva (WLC:191-197), y la escritura del panel redibuja desde ese diseño (H-PB-05). Sin medir en el DWG | WLC; [P]:81, `:120` | PB-TC alinea RACKEDITAR con el panel |
| **H-PB-38** | **Los 22 sobres reales medidos** no tienen estilo **y** tienen `Dimensions = 0`. Los 22 tienen `AnnotationScale = 1`. Los inventarios no recogen los demás campos | Bloque M, M-7 | (b) no cambia ningún dibujo medido, y R0 y R5 tampoco. Los demás campos, sin medir |
| **H-PB-39** | **La línea de estado no puede llevar el aviso** (H-17), y un `TextBlock` con nombre no hace saltar ninguna guarda medida (H-18) | H-17, H-18 | El aviso es un elemento propio (D-PBTC-03) |
| **H-PB-40** | **La línea base de la CI:** Core 19 070 **ya incluye** las 3 pruebas de Core de PB-T0, y un Theory saltado cuenta como un resultado | H-25, H-26 | Las cifras del §6.9 se derivan de 19 070 y 2 421 |
| **H-PB-41** | **`ClassAIsEvidenced…` de WLC sigue en verde** tras la corrección: solo asevera sobre el diccionario estático (WLC:257-263) | H-19 | No se sustituye (OB-T1B.2); el registro lo anota |
| **H-PB-42** | **Son diez, no once, los campos que pierde la ventana:** el peralte IN/OUT base siempre se guarda como el peralte del catálogo (H-29) | H-29 | F4 se fija con la clase `CATALOGO`, solo en su fila (§4.1) |
| **H-PB-43** | **Hoy RACKEDITAR reconstruye la secuencia de módulos** de un rack cuyo fondo tiene más de tres decimales; con la corrección, la copia como el piloto (H-30). El efecto en un rack con módulos personalizados no está medido | H-30 | CT-PBTC-10 lo mide en la cadena. Si es material, para (PBTC-C14) |
| **H-PB-44** | **Otros tres grupos de cajas siguen siendo fuente de verdad** con su redondeo: `GapBox`, `ModuleLengthBox` y cuatro cajas avanzadas (H-31) | H-31 | Fuera de PB-TC por decisión del Owner (OD-PBTC-5 = (B)). Vuelven a él antes del contrato de PB-T1 (§10) |
| **H-PB-45** | **El mensaje de `Assert.Equal` sobre listas trunca.** En el TRX del RED de PB-T0, cada cadena se corta a 50 caracteres con «···» y solo se muestran cinco posiciones alrededor de la primera diferencia (medición K-1) | TRX del RED de PB-T0, CT-PB0-01 y CT-PB0-17 | El delta completo de la base de CT-PBTC-10 **no** se puede leer del TRX de la CI del RED. Se lee del TRX de una corrida temporal con `Assert.Fail(texto)` (D-PBTC-10) |
| **H-PB-46** | **Precedente de un segundo texto en la banda de estado:** el Dinámico tiene `RebuildReportText`, colapsado y con `FontSize="11"`, dentro de su `StatusContent` (`src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml:368`, comprobado en `DynamicShellMigrationTests.cs:121`) | Lectura | Respalda L2 (D-PBTC-03) |
| **H-PB-47** | **Un rack antiguo sin marcador de datum** puede llegar a `FirstLevelHeightBox` con más de tres decimales tras la migración (H-08). Hoy se redondea a tres; con la corrección se conserva. T-3 R6 muestra que el troquel absorbe una diferencia de 0,00044 en el dibujo | H-08; T-3 R6 | La frase del §8 sobre los racks de tres decimales lo excepciona. En los dibujos reales, sin medir |

## 3. Decisiones del paquete

### D-PBTC-01. La corrección (a): «exacto mientras la caja muestre lo mismo» (OD-PBTC-1 (i), R-03 (i))

**Qué hace:** una caja que el usuario no editó no cambia el diseño. Una caja editada manda. Es la regla de `Keep` de Cantilever (H-PB-35), aplicada a las once cajas de F1-F11 en W, sin tocar `NumericField`. Para `BeamDepthBox` es inocua (H-29) y se aplica por uniformidad. Es la opción que eligió el Owner ([OPD], pregunta 1).

**Cómo, con su línea:**
1. **Un comparador privado nuevo, en W, junto a `Val` (W:3594):**
   - `ShownFormat` = `"0.###"`, el formato con el que `SetNumber` pinta esas cajas (NF:131);
   - `SameShown(a, b)`: igualdad ordinal de `a` y `b` formateados con `ShownFormat` y la cultura invariante;
   - `Keep(field, current)`: `current` si `field.Value` es nulo o si `SameShown(field.Value, current)`; si no, `field.Value`. Es el cuerpo de `RackCantileverWindow.xaml.cs:368-384`, con `ShownFormat`. Con caja vacía no cambia el respaldo de hoy (H-34).
2. **`ReadCellValues` (W:797-801):** las cinco llamadas `Val(<caja>, front.FirstLevelHeight)` y `Val(<caja>, cell.<campo>)` pasan a `Keep(<caja>, …)`, con el mismo segundo argumento: el valor del lado que se escribe.
3. **`OptionalVal` (W:3625-3626)** se queda con el mismo comportamiento cuando la caja está vacía. Si la caja tiene valor y el del lado también, y los dos se ven iguales (`SameShown`), devuelve el del lado.
4. **Las cinco cajas del rack (N-01, opción (a)).**
   - **En `LoadFromModel`, las llamadas `SetNumber` de W:386-406 no cambian,** ni su argumento.
   - Junto a cada una, en una línea propia, se asigna su portador con **la misma expresión** que pinta esa llamada. Son cuatro portadores privados nuevos:
     - junto a W:389: `rackTolerance = inputs.PalletTolerance > 0.0 ? inputs.PalletTolerance : DynamicRackDefaults.DefaultPalletTolerance;`
     - junto a W:391: `rackPostPeralte = inputs.PostPeralte;`
     - junto a W:392: `rackBeamDepth = inputs.BeamDepth > 0.0 ? inputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth;`
     - junto a W:406: `rackAnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;`
   - **El fondo no necesita portador nuevo:** lo es `generalPallet.Depth`, que W:385 ya asigna (H-33).
   - **En `ReadInputs`, al principio** (antes de `var inputs = new PushBackEditorInputs`, W:814), cada portador pasa a `Keep` de su caja, como hace Cantilever (`design.X = Keep(Box, design.X)`, H-PB-35):
     - `generalPallet = new PalletSpecification(generalPallet.Front, Keep(DepthBox, generalPallet.Depth), generalPallet.Height, generalPallet.Weight, generalPallet.WeightUnit);` Es una referencia nueva: el objeto cargado no se muta (H-33);
     - `rackTolerance = Keep(ToleranceBox, rackTolerance);`, y lo mismo con `rackPostPeralte` (`PostPeralteBox`), `rackBeamDepth` (`BeamDepthBox`) y `rackAnnotationScale` (`AnnotationScaleBox`).
   - **Y los inputs leen los portadores:** en W:819, `Val(DepthBox, generalPallet.Depth)` pasa a `generalPallet.Depth`; en W:837-839 y W:845, `Val(…)` pasa a `rackPostPeralte`, `rackTolerance`, `rackBeamDepth` y `rackAnnotationScale`.

   Con eso hay **diez** llamadas a `Keep(` (cinco aquí y cinco en el paso 2), y ninguna línea con `SetNumber(` cambia (INV-PBTC-03).
5. **`Val` (W:3594) se conserva:** la sigue usando W:1865, que PB-TC no toca.

**Lo que no cambia:**
- las 29 llamadas a `SetNumber` de W y WC, ni su formato ni su argumento: la caja muestra lo mismo que en la base;
- la validación, el error y el modo «Ambos»;
- `IntVal` (W:3628) y las cajas enteras;
- **las cajas de H-31** (`GapBox`, `ModuleLengthBox` y las cuatro avanzadas anulables): fuera de PB-TC por decisión del Owner (OD-PBTC-5 = (B); D-PBTC-07; §10).

**El resultado, por gesto:**
- **Carga, «Restaurar» y selección de celda:** el valor exacto del modelo llega a la salida. El peralte IN/OUT base sale siempre como el del catálogo (H-29).
- **El usuario escribe un número que se ve distinto del actual:** manda su número, y el portador o el estado pasan a ese número.
- **El usuario escribe algo que se ve igual al actual,** por ejemplo «61.2350» sobre 61,23456, que se ve «61.235»: se conserva el exacto (R-03 (i), decidida).
- **«Ambos lados»:** con una caja no mixta, cada lado conserva su propio valor exacto si los dos se ven iguales. Hoy los dos reciben el redondeo del lado activo.
- **Alcance y datos del frente** (W:1306, W:1470): aplican el valor exacto de la celda primaria. Hoy aplican su redondeo.
- **Un fondo exacto ya no reconstruye los módulos** (H-30): la pasada copia el baseline, como el piloto.

**Por qué queda dentro de Push Back (OB-06.6) y qué va más allá de la «ruta de carga»:**
- Todo vive en W. Ningún archivo compartido ni de otro sistema cambia.
- La «ruta de carga» de OB-06.6 ([P]:395) se extiende a la relectura de lo sembrado: la selección de celda, «Ambos lados», el alcance, los datos del frente y la biblioteca (H-PB-36). El Owner aceptó esa extensión ([OPD], pregunta 1, «Alcance»).

**Por qué Cantilever no cambia:**
1. Ningún archivo de `src/RackCad.UI/Controls/` ni de `src/RackCad.UI/Systems/Cantilever/` entra en el diff (INV-PBTC-01; están en `ForbiddenWriteScope`).
2. Cantilever no depende del formato por defecto (H-PB-34) y tiene su propio `Keep`, que no se toca (H-PB-35).
3. Sus 67 pruebas de UI van en `RequiredTests` (§6.4).

**Alternativas descartadas:**
- **Cambiar el valor por defecto de `NumericField`:** cambia lo que muestran las 29 cajas de Push Back y el ayudante de pruebas (H-PB-34). Además, OB-06.6 lo reserva al Owner.
- **Sembrar con un formato largo** (`"R"` o `"0.##########"`): cambia el texto visible de RACKEDITAR más allá de la corrección.
- **Una marca de edición por `TextChanged`** (R-03 (ii)): descartada por el Owner ([OPD], pregunta 1).

### D-PBTC-02. La corrección (b): el estilo cargado viaja en cada pasada (OD-PBTC-2 (i))

- **Un portador privado** `loadedDimensionStyle` en W, junto a `loadedDimensionViews` (W:1216):
  - en `LoadFromModel`, tras W:402, toma `options.DimensionStyle`;
  - en `ReadInputs` (W:840-848), `Annotations` lleva `DimensionStyle = loadedDimensionStyle`.
- **Sin control nuevo:** la ventana de Push Back no tiene selector de estilo (MB H-082), y no lo gana.
- **Un rack nuevo** sigue sin estilo (H-11).
- **Por construcción,** el diseño, el sistema y cada cota llevan el estilo cargado (H-12). RACKEDITAR dibuja entonces con el estilo nombrado si el dibujo lo tiene, y sin sobrescrituras (H-13). RACKEDITAR deja además de borrar el estilo guardado en el sobre.
- **Rutas:** las cuatro cargas de H-07. Con la biblioteca, «Insertar» y «Guardar en biblioteca» también conservan el estilo y los valores exactos, porque salen de `lastComputation.Design`. Hoy los pierden. Es la misma corrección y está declarada ([OPD], pregunta 2).

### D-PBTC-03. D-PB-08: el aviso no bloqueante, en la banda de estado (OD-PBTC-3)

- **El elemento (L2),** en WX, dentro del `StackPanel` de `StatusContent` (WX:681-685), inmediatamente después de `StatusText` (WX:684):

  `<TextBlock x:Name="SideBEmptyNoticeText" Foreground="{DynamicResource ShellTextSecondaryBrush}" FontSize="11" TextWrapping="Wrap" Margin="0,4,0,0" Visibility="Collapsed" />`

  Lleva los mismos atributos que `StatusText`, sin ningún literal de estilo nuevo, y la forma de `RebuildReportText` del Dinámico (H-PB-46). No lleva `Text` en el XAML.
- **El texto:** una constante de WC con este literal, sin comillas exteriores. Es el que eligió el Owner, byte a byte ([OPD], pregunta 3):

  ```text
  Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.
  ```

- **La regla, en un método privado nuevo de WC:** visible si `composite.SideBPresent` y ninguna ranura `s` del lado B cumple `composite.IsSlotPresent(PushBackSide.B, s)`; colapsado en cualquier otro caso. Con visible, el texto es la constante; con colapsado, vacío. Es la regla de capacidad sin presencia de H-16, solo leída.
- **Dónde se evalúa:** una llamada en `Recompute`, justo después de W:607. Se evalúa en toda pasada efectiva, válida o no (H-17).
- **Lo que no hace:**
  - no es un `Control` ni es enfocable;
  - no cambia ningún botón, ni el estado, ni el diseño;
  - no toca la casilla ni `StatusText`. `SetStatus` no lo sobrescribe, porque solo escribe `StatusText`.
- **Por qué L2:** respeta la regla del shell «no second status inside the sidebar» (WX:678-679; `PushBackShellAdoptionTests.cs:15-20`); siempre está visible, fuera del desplazamiento de la barra lateral (WX:677); ninguna guarda fija el número de hijos de la banda (H-18).
- **Descartadas por el Owner:** L1, bajo la casilla en la barra lateral, y el texto largo.
- **Los dos modos:** es la misma ventana, pero en WORKSPACE **no está demostrado** que el aviso diga lo correcto: el borrador de PB-T1 podría conservar la capacidad. Se comprueba en PB-T2 (precondición 3; OB-08.2).

### D-PBTC-04. La sustitución declarada (OB-T1B.2, OB-06.3b, D-PB-11)

- **Se sustituyen exactamente cuatro pruebas, S-01 a S-04 (§5.2).** Son las únicas que chocan con las correcciones (H-19 a H-24, H-36 y H-37).
- **La única edición** en cada uno de sus archivos es el atributo de la prueba: `[Theory]` o `[Fact]` pasa a `[Theory(Skip = "<literal>")]` o `[Fact(Skip = "<literal>")]`, con el literal del §5.2. Nada más cambia.
- **El registro lo crea la sesión** en el commit de emisión (precondición 3), con el contenido del §5.4. El Worker no escribe en `docs/`.
- **Lo que no choca no se toca** (§5.3), incluida `ClassAIsEvidenced…` (H-PB-41).

### D-PBTC-05. El RED y el GREEN

- **RED:**
  - los seis archivos nuevos (§4) y los cuatro atributos `Skip`;
  - ningún archivo de `src/`;
  - los diez métodos del filtro con `ExpectRed` fallan **por aserción** en la base (§5.1).
- **GREEN:**
  - solo W, WC y WX;
  - ningún archivo de `tests/`.
- **Core no tiene RED,** porque las correcciones son solo de UI. Su filtro va con `ExpectRed` false. Precedente: F4-T3B1, cuyo RED con Core en `success` se aceptó con `RedPart = pass`.

### D-PBTC-06. El paso 4 en el Workspace queda para PB-T2 (OB-06.3)

- El piloto no pasa por la ventana (H-PB-30). PB-TC prueba el paso 4 solo en RACKEDITAR, es decir, en la ventana en modo clásico: CT-PBTC-02 y CT-PBTC-05.
- CT-PBTC-06 y CT-PBTC-10 dejan fijado que la ventana y el piloto escriben lo mismo.
- La sesión anota la obligación de PB-T2 en `f4b-dispositions.md` al emitir (precondición 3), y el registro la marca **pendiente de PB-T2**.
- PB-TC no añade el modo WORKSPACE.

### D-PBTC-07. El alcance tras las decisiones del Owner

- **H-PB-28 entra** como corrección declarada nueva **D-PB-11** (OD-PBTC-4 = (A); D-PBTC-11).
- **Las cajas de H-31 no entran** (OD-PBTC-5 = (B)). Quedan fuera **por la letra**:
  - `GapBox` y `Gap_Changed` (WC:159-175), con su siembra (WC:721);
  - `ModuleLengthBox` y `ModuleLength_Changed` (W:1852-1873), con su siembra (W:1793);
  - las cuatro cajas avanzadas anulables y `ReadAdvancedRackParameters` (W:1592-1607), su manejador (W:1616-1626), `SetOptional` (W:1582-1589) y su regla de lectura.

  D-PB-11 **llama** a `LoadAdvancedRackParameters` (W:1555-1579) sin modificarla. Tocar cualquiera de esos miembros es PBTC-C10 (INV-PBTC-15). El caso vuelve al Owner antes del contrato de PB-T1 (§10).

### D-PBTC-08. Las ventanas de las pruebas (M-01 de PB-T0)

- Toda ventana se abre como `var w = Ventana(); try { <carga>; … } finally { Cerrar(w); }`, con la carga **dentro** del `try`.
- `Abrir` y `AbrirFueraDePantalla` de S0 no se usan.
- Ninguna ventana se muestra.

### D-PBTC-09. Sin teclado ni foco real

Las pruebas usan solo mecánicas ya probadas en la CI de HEAD:
- `SetNumberR` de S0, que es `SetNumber(v, "R")` más `LostFocus` elevado con `RaiseEvent` (PB-T0, CT-03 y CT-04);
- `Text` asignado por código, como `EditorWindowTestSupport.SetText` (`EditorWindowTestSupport.cs:89-95`, que ya usa CT-PB0-11), seguido del mismo `RaiseEvent` de `LostFocus`;
- `IsChecked` y `SelectedIndex` asignados por código (PB-T0, CT-07 y CT-09);
- `SetFrontBlank`, `SelectMatrixCell`, `SelectModule` y `ClickNamed`.

Ninguna depende de `WM_KEYDOWN`, de `TextInput` ni del foco efectivo.

### D-PBTC-10. Cómo se mide en la cadena el delta de la base de CT-PBTC-10 (I-02)

- **El problema:** el TRX de la CI del RED solo da la primera diferencia, truncada (H-PB-45).
- **Paso 1 del Worker, sobre `BaseSha` y antes del RED:** una corrida **temporal** de CT-PBTC-10. **Los cambios temporales**, que no entran en ningún commit, son dos:
  1. el final del método: `Assert.Fail(string.Join("\n", log))` en lugar de `Assert.Equal`, para que el mensaje lleve el texto completo;
  2. el código que aplica a las filas **F1** y **M-02** la fórmula de CT-PB0-21: K1, K2, K3 y K4, con `BuildFrom` y el catálogo entregado, sobre la salida de la ventana frente a lo que escribe el piloto (`RoundingEffectTests.cs:107-245`, con «material» en `:164`). Se copia en el código temporal.

  F1 y M-02 son las dos filas en que la ventana reconstruye los módulos (H-30). El resto de las filas tienen su efecto ya medido en T-3.
- **Su TRX y su log** se guardan como artefactos de la entrega (M-09). El delta, fila por fila, y K1 a K4 de F1 y M-02 se transcriben en `worker-handoff.json`.
- **Si F1 o M-02** tiene alguna línea bajo `$.PushBack.Structure.Modules` o `$.PushBack.Structure.HeaderLineOverrides`, o es material por la fórmula de CT-PB0-21, es PBTC-C14: STOP antes del GREEN y vuelta al Owner (OB-06.4; [OPD], pregunta 1).

### D-PBTC-11. La corrección D-PB-11: «Restaurar estándar» llega al Actualizar (OD-PBTC-4 = (A))

**El arreglo, por línea** (H-35), solo en la rama válida de `Recompute` (W:658-692):
1. Inmediatamente antes de `moduleOwner.ClearModuleCommit();` (W:682), una línea toma si la pasada consumió un «Restaurar estándar»: `var standardRestoreLanded = moduleOwner.ModuleCommit?.StandardRestoreRequested == true;`
2. Inmediatamente después de W:682, si `standardRestoreLanded`, se hace lo mismo que «Restaurar parámetros» (W:1630-1631), con las mismas dos sentencias:
   - `state.RestoreAdvancedRackParameters(advanced);`
   - `LoadAdvancedRackParameters();`
3. **Nada más:**
   - no se pide otra pasada (sin `RequestRecompute()`), porque el diseño adoptado ya trae el reinicio (DA:300-303);
   - no se toca `LoadAdvancedRackParameters`, que repinta bajo `suppressSync` (W:1562-1575);
   - el código nuevo **no contiene** `forceRebuild:` ni `forceRebuild)`, ni en comentarios, por la guarda Fact3 (H-37).

**El efecto,** tras «Restaurar estándar» y «Confirmar»:
- los seis controles avanzados muestran el estándar: cajas vacías y «Reforzar poste derivado» marcada;
- «Actualizar» escribe los valores reiniciados.

Cambia lo que RACKEDITAR dibuja en ese flujo: la altura de cabecera, el poste derivado, su refuerzo y los separadores pasan a ser los estándar, que es lo que el usuario pidió (DA:296-299). El Owner aceptó ese cambio ([OPD], pregunta 4: «Cambia lo que se dibuja en ese caso»).

**Lo que no cambia:**
- el ensamblador;
- «Restaurar parámetros»;
- la reconciliación de módulos;
- el flujo en el que «Actualizar» confirma él mismo un «Restaurar estándar» pendiente (W:3346-3349). Su segunda pasada ya escribía lo reiniciado, y ahora además repinta los controles.

## 4. Las pruebas nuevas (lista cerrada: CT-PBTC-01 a 08, 10 y 14)

Los números sin usar: CT-PBTC-09 se retiró en la v2 (M-04), y CT-PBTC-11 a 13 eran del anexo A, que no entra (OD-PBTC-5 = (B)). No se reutilizan.

### 4.1 Definiciones comunes

- **De S0, solo de uso:** `Ventana`, `Cerrar`, `Sta`, `Fixture`, `EditInner`, `Persist`, `Salida`, `Delta`, `Field`, `Check`, `Combo`, `Btn`, `Click`, `SetNumberR`, `ValueOf`, `SelectModule` y `StatusText` (H-28).
- **Del ayudante nuevo `PushBackDeclaredCorrectionSupport`** (`internal static`, sin pruebas):
  - `Cargar(w, t)`: deserializa el sobre `t` (`RackEmbedStore` y `RackProjectStore`) y llama a `w.LoadExisting(project.PushBackDesign, embed.Id, embed.Name, project)`;
  - `CargarBiblioteca(w, t)`: lo mismo con `w.LoadDesignForNew(project.PushBackDesign, embed.Name, project)`;
  - `Num(json, ruta)`: el número en esa ruta del JSON persistido, escrito con «R» y la cultura invariante, o `<null>`;
  - `PilotoEscribe(t, catalog)`: copia de WLC:84-91, sin referenciarla;
  - `EstiloPb()`: copia de `PushBackDimensionViewsWindowTests.cs:65-92` con `policy = null` y `dimensionStyle = "ESTILO_PB"`;
  - `Aviso(w)`: `Assert.IsType<TextBlock>(w.FindName("SideBEmptyNoticeText"))`.
- **Filas F** (copiadas de CT-PB0-19, `RoundingLossTests.cs:55-65`): sobre `pb-01-base`, el valor exacto es la base más 0,00044.

  | Fila | Ruta del JSON interior | Base |
  |---|---|---|
  | F1 | `Structure.PalletDepth` | 48 |
  | F2 | `Structure.PostPeralte` | 3 |
  | F3 | `Structure.PalletTolerance` | 4 |
  | F4 | `Structure.BeamDepth` | 6 |
  | F5 | `Structure.AnnotationScale` | 1,5 |
  | F6 | `Structure.Fronts[0].FirstLevelHeight` | 4 |
  | F7 | `Structure.Fronts[0].Levels[0].PalletFront` | 42 |
  | F8 | `Structure.Fronts[0].Levels[0].PalletHeight` | 60 |
  | F9 | `Structure.Fronts[0].Levels[0].PalletWeight` | 1000 |
  | F10 | `Structure.Fronts[0].Levels[0].ClearHeight` | 6 |
  | F11 | `Structure.Fronts[0].Levels[0].BeamLengthOverride` | 100 |

- **Las clases de una fila** (CT-PBTC-01; N-02):
  - **en las filas F1 a F3 y F5 a F11,** las de CT-PB0-19 (`ClassOfRow`, `RoundingLossTests.cs:89-93`), en este orden: `CONSERVA` si la salida es igual al exacto; `REDONDEA` si es igual a R3 del exacto; `OTRA` en cualquier otro caso;
  - **solo en la fila F4** (`Structure.BeamDepth`), en este orden: `CONSERVA` si la salida es igual al exacto; `CATALOGO` si es igual a `DynamicLoadBeamGeometry.ResolveBeamDepth(w.Session.Catalog, DynamicRackDefaults.InOutBeamCatalogId, exacto)`; `REDONDEA` si es igual a R3 del exacto; `OTRA` en cualquier otro caso.

  Con el catálogo entregado, F4 da `CATALOGO` en la base y en el GREEN (H-29).
- **Los formatos de las líneas:**
  - `vistas` = `InsertionRequest.Views.Count`, leído como `Assert.IsType<PushBackInsertionRequest>(w.InsertionRequest).Views.Count`, como en `StateTests.cs:189-191`;
  - `<caja>=<v>`, cuando el paso **escribe** en una caja (`AnnotationScaleBox=2.5`, `CellPalletHeightBox=70.5`): `<v>` es el número que se escribió con «R». En cambio, `caja=<t>` y `texto=<t>` son el **texto** actual de la caja, tal cual;
  - los números del diseño, con `Num`, que usa «R»;
  - `estilos=[a,b]`: el conjunto de `DimensionStyleName`, en orden ordinal, separado por una coma sin espacio y con `<null>` para el nulo;
  - `delta=[…]`: las líneas de `Delta` unidas con `" ;; "`, como en CT-PB0-02 (`StateTests.cs:191`);
  - `aviso=<Visibility>`: `Visible` o `Collapsed`;
  - booleanos: `True` y `False`, como da `bool.ToString()`;
  - el literal del aviso: el del bloque de código de D-PBTC-03, byte a byte;
  - en CT-PBTC-14, `Six` y `SixControls`: copias de `ModuleTests.cs:143-158`.
- **Comparación:** cada método arma una lista de líneas y termina en **un** `Assert.Equal(<lista esperada>, <lista>)`. Las precondiciones son aserciones. Ninguna aserción va dentro de una costura.
- **Las clases:**
  - las cinco clases nuevas van en `[Collection(EditorDiscardPromptCollection.Name)]`, con el `namespace` `RackCad.UI.Tests`;
  - todas son `public sealed class`, con métodos `[Fact]` y sin `Theory`.
- **Cultura:** la invariante, por `Sta`.
- **Avisos:** ninguno nuevo del analizador (por ejemplo, xUnit2013).

### 4.2 Los CT

**CT-PBTC-01. (a) Los campos cargados conservan su valor exacto.** `PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading`.
- **Pasos:** por fila F1 a F11, cada una en su ventana: `EditInner(Fixture("pb-01-base"), …)`, `Cargar` y `Salida`.
- **Fija** once líneas con el formato de CT-PB0-19 (`:135-148`):
  - F1, F2, F3 y F5 a F11: `salida=` igual a `exacto=` y la clase `CONSERVA`. Por ejemplo, `F1 pb-01-base Structure.PalletDepth exacto=48.00044 salida=48.00044 CONSERVA`;
  - F4: `F4 pb-01-base Structure.BeamDepth exacto=6.00044 salida=6 CATALOGO`.
- **Sustituye a** CT-PB0-19 (S-03).

**CT-PBTC-02. (a) RACKEDITAR y la biblioteca llevan los valores exactos (paso 4 en RACKEDITAR).** `…RoundingTests.CT_PBTC_02_UpdateInRackeditarAndTheLibraryLoadCarryTheExactValues`.
- **(i) Por fila F1 a F11,** cada una en su ventana: `Cargar`, `UpdateButton.IsEnabled` como precondición y `Click(w, "UpdateButton")`.

  La línea es `Fn RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=<v>`, con `<v>` leído de `Persist(w.DesignToInsert)`: el exacto en diez filas y `6` en F4.
- **(ii) `pb-08-redondeo` tal cual:** lo mismo. La línea es `pb-08-redondeo RACKEDITAR peticion=True updateOnly=True vistas=0 AnnotationScale=1.23456 PalletHeight=61.23456 sistema.AnnotationScale=1.23456`. `sistema` es `w.SystemToInsert.Structure`.
- **(iii) `pb-08-redondeo` por la biblioteca:** `CargarBiblioteca` y `Salida`, **sin** pulsar nada. La línea es `pb-08-redondeo biblioteca AnnotationScale=1.23456 PalletHeight=61.23456 peticion=False`.

**CT-PBTC-03. (a) La edición manda; una caja sin editar, escrita igual o vuelta a sembrar, conserva el exacto.** `…RoundingTests.CT_PBTC_03_AnEditedBoxWinsAndAnUneditedOrReselectedBoxKeepsTheExactValue`.
- **Diseño X-TC-03:** `pb-08-redondeo` con `Structure.Fronts[1].Levels[0].PalletHeight = 60.00044`, por `EditInner`.
- **Notación:** `c00` y `c10` son `Structure.Fronts[0].Levels[0].PalletHeight` y `Structure.Fronts[1].Levels[0].PalletHeight` de `Salida`.
- **Pasos y líneas fijadas:**
  1. carga: `carga AnnotationScale=1.23456 c00=61.23456 c10=60.00044`;
  2. `SetNumberR(w, "AnnotationScaleBox", 2.5)`: `AnnotationScaleBox=2.5 AnnotationScale=2.5 c00=61.23456`;
  3. **(R-03 (i))** precondición `Field(w, "CellPalletHeightBox").Text == "61.235"`. Se asigna `Text = "61.2350"`, que es distinto y dispara `TextChanged` (H-32), y se eleva `LostFocus`: `CellPalletHeightBox texto=61.2350 c00=61.23456`;
  4. `SetNumberR(w, "CellPalletHeightBox", 70.5)`: `CellPalletHeightBox=70.5 c00=70.5`;
  5. `w.SelectMatrixCell(1, 0, false)`: `seleccion(1,0) caja=60 c00=70.5 c10=60.00044`, con `caja` = `Field(w, "CellPalletHeightBox").Text`;
  6. `w.SelectMatrixCell(0, 0, false)`: `seleccion(0,0) caja=70.5 c00=70.5 c10=60.00044 AnnotationScale=2.5`.

**CT-PBTC-04. (b) El estilo cargado sobrevive a las cargas y a los gestos.** `PushBackDeclaredCorrectionDimensionStyleTests.CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures`. **Sin cambio desde la v1.**
- **Pasos** (el estilo se lee de `w.LastComputation.Design.Structure.DimensionStyle`):
  - `EstiloPb()` con `LoadExisting(…, "GUID-PB-H1", "PB H1")`, tras la carga y después de `Check(w, "DimensionsLateralCheck").IsChecked = false`. Es la construcción y el gesto de `T19_H1`;
  - `pb-06-seguridad` y `pb-07-combinado` con `Cargar`;
  - `pb-07-combinado` con `CargarBiblioteca`;
  - `Ventana()` sin carga (rack nuevo).
- **Fija:**
  - `ESTILO_PB carga=ESTILO_PB`;
  - `ESTILO_PB gesto DimensionsLateralCheck=False estilo=ESTILO_PB`;
  - `pb-06-seguridad carga=STANDARD-ISO`;
  - `pb-07-combinado carga=SIMPLEX`;
  - `pb-07-combinado biblioteca=SIMPLEX`;
  - `nuevo estilo=<null>`.
- **Sustituye a** `T19_H1` (S-02).

**CT-PBTC-05. (b) RACKEDITAR lleva el estilo nombrado a cada cota (paso 4 en RACKEDITAR).** `…DimensionStyleTests.CT_PBTC_05_UpdateInRackeditarCarriesTheNamedStyleToEveryDimension`. **Sin cambio desde la v1,** salvo el formato de `estilos` (§4.1).
- **Diseños:** `pb-06-seguridad` (`Dimensions = 3`) y `pb-07-combinado` (`Dimensions = 1`), cada uno en su ventana. Se carga con `Cargar` y se pulsa `UpdateButton`.
- **Las cotas:** las instancias `Role == HeaderBlockRole.Dimension` de `w.LastComputation`: `PlantaPlan`, `FrontalEntradaSalida`, `FrontalPosterior`, `LateralPlan` y cada `LateralCortes[*].Plan`, con `Flatten().Instances`.
- **Fija:**
  - `pb-06-seguridad peticion=True updateOnly=True diseno=STANDARD-ISO sistema=STANDARD-ISO cotas>0=True estilos=[STANDARD-ISO]`;
  - `pb-07-combinado peticion=True updateOnly=True diseno=SIMPLEX sistema=SIMPLEX cotas>0=True estilos=[SIMPLEX]`.

  `diseno` es `DesignToInsert.Structure.DimensionStyle`, y `sistema` es `SystemToInsert.Structure.DimensionStyle`.

**CT-PBTC-06. La ventana y el piloto escriben lo mismo en los 14 diseños de WLC.** `PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns`. **Sin cambio desde la v1.**
- **Diseños:** los 14 de WLC, por nombre y en su orden: `pb-01-base` a `pb-08-redondeo`, y después `pb-10-sin-datum` a `pb-15-sin-composite`.
- **Pasos:** por diseño, `Cargar`. `admitida` = `w.CurrentInputsAreValid && w.LastComputation.IsValid`. Después, `PilotoEscribe(t, w.Session.Catalog)`, y `Delta(Persist(w.LastComputation.Design), piloto)`.
- **Fija:** 14 líneas `<nombre> admitida=True delta=[]`.
- **Sustituye a** la Theory de WLC (S-01), y cubre sus 14 filas.

**CT-PBTC-07. D-PB-08: el aviso se ve exactamente mientras B está declarado sin frentes activos.** `PushBackDeclaredCorrectionSideBNoticeTests.CT_PBTC_07_TheSideBNoticeShowsExactlyWhileSideBIsDeclaredWithoutAnActiveFront`.
- **`pb-01-base`:** los gestos de CT-PB0-09, copiados (`CompositeTests.cs:171-189`).
- **`pb-20-ab`:** los de CT-PB0-07, copiados (`:103-113`).
- **Por paso:** `aviso=<Visibility>` y `compuesto=<LastComputation.Design.IsComposite>`.
- **Fija:**
  - `pb-01-base carga aviso=Collapsed compuesto=False`;
  - `pb-01-base declarar B aviso=Visible compuesto=False`;
  - `texto=` seguido del literal de D-PBTC-03;
  - `pb-01-base anadir frente aviso=Visible compuesto=False`;
  - `pb-01-base lado B, ranura 1 de B activa aviso=Collapsed compuesto=True`;
  - `pb-01-base ranuras 1 y 2 de A en blanco aviso=Collapsed compuesto=True`;
  - `pb-01-base ranura 1 de B en blanco aviso=Visible compuesto=False`;
  - `pb-01-base retirar B aviso=Collapsed compuesto=False`;
  - `pb-20-ab carga aviso=Collapsed compuesto=True`;
  - `pb-20-ab retirar B aviso=Collapsed compuesto=False`;
  - `pb-20-ab declarar B aviso=Collapsed compuesto=True`;
  - `pb-20-ab todas las ranuras de B en blanco aviso=Visible compuesto=False`.

**CT-PBTC-08. D-PB-08: el aviso es solo texto, está en la banda de estado y no bloquea nada.** `…SideBNoticeTests.CT_PBTC_08_TheSideBNoticeIsTextOnlyAndBlocksNoAction`.
- **Diseño:** `pb-01-base` con `SideBPresentCheck` marcada.
- **`shell`:** `(RackEditorVisualShell)w.FindName("Shell")`.
- **Fija:**
  - `aviso=Visible control=False enfocable=False actualizar-habilitado=True`, donde `control` es si el aviso es un `Control` y `enfocable` es su `Focusable`;
  - `lugar estado=True barra-lateral=False`, con `estado` = `EditorWindowTestSupport.Descendants((DependencyObject)shell.StatusContent).Contains(aviso)` y `barra-lateral` = lo mismo sobre `shell.SidePanelContent`;
  - tras `Click(w, "UpdateButton")`: `peticion=True updateOnly=True compuesto=False`, con `compuesto` = `DesignToInsert.IsComposite`.

**CT-PBTC-09. Retirado en la v2 (M-04).** La prueba de OB-06.6 la dan INV-PBTC-01 y el filtro de Cantilever del §6.4.

**CT-PBTC-10. (a) La ventana y el piloto escriben lo mismo con cada campo exacto y con módulos personalizados (I-02).** `PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_10_TheWindowAndThePilotWriteTheSameDesignForEveryExactFieldAndForCustomModules`.
- **Filas:**
  - las once variantes F1 a F11 de `pb-01-base` (§4.1);
  - **M-02:** `pb-02-modulos` con `Structure.PalletDepth = 48.00044`.

    Se elige `pb-02-modulos` porque es el único diseño del corpus con dos personalizaciones de módulo reales y de dos clases distintas: una longitud distinta de la estándar (M2 `Separator` 30 manual, cuando el estándar es 48) y una cabecera propia (M1 `HeaderStart` 54 manual, con `UseCalculatedHeaderConfiguration = false`). `pb-31-v2-modulos` solo tiene M3, con cabecera propia pero a la longitud estándar, y `pb-07-combinado` solo una longitud manual (H-23).
- **Pasos:** por fila, en su ventana, como CT-PBTC-06: `Cargar`, `admitida`, `PilotoEscribe(t, w.Session.Catalog)` y `Delta(Persist(w.LastComputation.Design), piloto)`.
- **Fija** doce líneas `<fila> admitida=True delta=[]`: de `F1` a `F11`, y `M-02`.
- **F4, medido:** la ventana y el piloto pasan por el mismo ensamblador y normalizan al catálogo (DA:165-168; `PushBackCompositeEditorAssembler.cs:59`). Los dos dan `BeamDepth = 6`, en la base y en el GREEN, así que su delta es vacío en los dos. No hay diferencia por el catálogo que fijar.
- **En la base** (razonado, no ejecutado):
  - F2, F3 y F5 a F11: **al menos una línea** cada una, entre ellas `ruta: <R3> -> <exacto>`. F6 da dos, porque el diseño lleva también `Structure.FirstLevelHeight` tomado del primer frente (ICT2, N-05; `DynamicRackSystemResolver.cs:309`);
  - F1 y M-02: al menos `$.PushBack.Structure.PalletDepth: 48 -> 48.00044`. La ventana reconstruye los módulos (H-30), así que puede haber además líneas bajo `Structure.Modules` y `HeaderLineOverrides`, **sin medir**;
  - F4: vacío;
  - las ventanas se cargan sin lanzar: las de F, como CT-PB0-19; `pb-02-modulos`, como en WLC; y el cambio de fondo reconstruye sin error, como en CT-PB0-04.

  Falla por aserción, en la línea `F1`. El delta completo de la base y K1 a K4 de F1 y M-02 se obtienen como dice D-PBTC-10.

**CT-PBTC-14. D-PB-11: «Restaurar estándar» llega a los controles y a la petición.** `PushBackDeclaredCorrectionStandardRestoreTests.CT_PBTC_14_AStandardRestoreResetsTheAdvancedControlsAndReachesTheUpdate`.
- **Pasos:** los de CT-PB0-05 (`ModuleTests.cs:176-193`), copiados, salvo que cada diseño se abre con `var w = Ventana(); try { Cargar(w, Fixture(<diseño>)); … } finally { Cerrar(w); }` (D-PBTC-08) en vez de `Abrir`. Los diseños son `pb-06-seguridad` y `pb-07-combinado`, y los formatos, `Six` y `SixControls` (§4.1).
- **Fija diez líneas:**
  - **seis iguales a las de CT-PB0-05** (`:199-201` y `:204-206`): A0 salida, A0 controles y A1 salida, en pb-06 y pb-07;
  - **cuatro nuevas**, en el lugar de `:202`, `:203`, `:207` y `:208`:
    - `pb-06-seguridad A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]`;
    - `pb-06-seguridad A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null`;
    - `pb-07-combinado A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]`;
    - `pb-07-combinado A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=null,SeparatorSpacingOverride=null`.
- **En la base:** A1 controles da los valores cargados (`:202`). Falla por aserción en esa línea.
- **Sustituye a** CT-PB0-05 (S-04).

### 4.3 Trazabilidad

| Obligación | CT |
|---|---|
| D-PB-06, paso 2 (OB-06.3): un RED por pérdida | (a): 01, 02, 03, 06 y 10. (b): 04, 05 y 06 |
| D-PB-06, paso 3: el comportamiento corregido | El GREEN de W, WC y WX (D-PBTC-01 y D-PBTC-02). Lo comprueban INV-PBTC-03, INV-PBTC-04 e INV-PBTC-15 sobre el diff, y los mismos CT en verde |
| D-PB-06, paso 4, sobre RACKEDITAR | 02 y 05; 06 y 10 como paridad con el piloto |
| D-PB-06, paso 4, sobre el Workspace | Pendiente de PB-T2 (D-PBTC-06; precondición 3) |
| D-PB-06, paso 5, y OB-06.3b | 01, 04 y 06 (S-03, S-02 y S-01) |
| D-PB-06, paso 6 | La anotación de la sesión en `f4b-dispositions.md` al cerrar y el registro EJECUTADO (INV-PBTC-16) |
| D-PB-11 ([OPD], pregunta 4) | 14 (S-04); INV-PBTC-19 sobre el diff |
| OB-06.4: efecto material | 10 y PBTC-C14 (filas F1 y M-02); PBTC-C03. D-PB-11 cambia el dibujo de ese flujo por decisión del Owner |
| OB-06.5: casos alcanzables sin corregir | H-31, registrado como pendiente (§5.4, §10) |
| OB-06.6: Cantilever sin cambio | INV-PBTC-01 y el filtro de Cantilever (§6.4) |
| OB-08.1 a OB-08.3 | 07 y 08, más `PushBackBlankFrontAuthorityTests`, CT-PB0-07 y CT-PB0-09 en verde |
| M-05 de PB-T0 | El filtro de PB-T0 en UI en verde (INV-PBTC-08) |

### 4.4 Mapa CT → método (N-02)

| CT | Clase (`tests/RackCad.UI.Tests/`) | Método |
|---|---|---|
| 01 | `PushBackDeclaredCorrectionRoundingTests` | `CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading` |
| 02 | `PushBackDeclaredCorrectionRoundingTests` | `CT_PBTC_02_UpdateInRackeditarAndTheLibraryLoadCarryTheExactValues` |
| 03 | `PushBackDeclaredCorrectionRoundingTests` | `CT_PBTC_03_AnEditedBoxWinsAndAnUneditedOrReselectedBoxKeepsTheExactValue` |
| 04 | `PushBackDeclaredCorrectionDimensionStyleTests` | `CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures` |
| 05 | `PushBackDeclaredCorrectionDimensionStyleTests` | `CT_PBTC_05_UpdateInRackeditarCarriesTheNamedStyleToEveryDimension` |
| 06 | `PushBackDeclaredCorrectionPilotParityTests` | `CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns` |
| 07 | `PushBackDeclaredCorrectionSideBNoticeTests` | `CT_PBTC_07_TheSideBNoticeShowsExactlyWhileSideBIsDeclaredWithoutAnActiveFront` |
| 08 | `PushBackDeclaredCorrectionSideBNoticeTests` | `CT_PBTC_08_TheSideBNoticeIsTextOnlyAndBlocksNoAction` |
| 10 | `PushBackDeclaredCorrectionPilotParityTests` | `CT_PBTC_10_TheWindowAndThePilotWriteTheSameDesignForEveryExactFieldAndForCustomModules` |
| 14 | `PushBackDeclaredCorrectionStandardRestoreTests` | `CT_PBTC_14_AStandardRestoreResetsTheAdvancedControlsAndReachesTheUpdate` |

## 5. El RED, las sustituciones y el registro

### 5.1 Por qué cada método nuevo falla en la base, por aserción

| CT | En la base | Primera diferencia |
|---|---|---|
| 01 | Diez filas dan `salida=` igual a R3 del exacto y la clase `REDONDEA`. F4 da `CATALOGO`, igual que en el GREEN | El `Assert.Equal` final, en F1 |
| 02 | `DesignToInsert` sale de la segunda pasada, que relee las cajas redondeadas (H-03, H-09). En (iii), la carga redondea igual (H-07). F4 da `diseno=6`, igual que en el GREEN | El `Assert.Equal` final, en F1 |
| 03 | `carga` da `1.235` y `61.235` (WLC:195-196). El paso 3 da `c00=61.235` y el paso 5, `c10=60` (H-PB-36) | El `Assert.Equal` final, en la línea 1 |
| 04 | El estilo sale `null` tras la carga y tras el gesto (`T19_H1`, H-11) | El `Assert.Equal` final, en la línea 1 |
| 05 | `diseno`, `sistema` y `estilos` dan `<null>` (H-11, H-12) | El `Assert.Equal` final |
| 06 | pb-06, pb-07 y pb-08 dan las líneas de WLC:191-197 | El `Assert.Equal` final, en `pb-06-seguridad` |
| 07 | `FindName("SideBEmptyNoticeText")` es `null` | `Assert.IsType<TextBlock>` en `Aviso(w)` |
| 08 | Igual que 07 | `Assert.IsType<TextBlock>` |
| 10 | F1 da al menos la línea de `PalletDepth`; F2, F3 y F5-F11, al menos una línea cada una (F6, dos); F4, vacío; M-02, al menos la de `PalletDepth` (§4.2) | El `Assert.Equal` final, en F1 |
| 14 | A1 controles da los valores cargados (H-35, H-36) | El `Assert.Equal` final, en `pb-06-seguridad A1 controles` |

Ninguno lanza antes de su aserción:
- las cargas, los gestos y `UpdateButton` son los que ya pasan en la CI de HEAD (PB-T0, CT-02, CT-04, CT-05, CT-07, CT-09 y CT-19; `T19_H1`; WLC);
- `Aviso(w)` es la primera sentencia que toca el aviso.

### 5.2 Las sustituciones (vieja → nueva)

**El literal del `Skip`,** con `<autoridad>`, `<n>` y `<Clase.Metodo>` de la tabla:

```text
SUSTITUIDA por PB-TC (<autoridad>), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila S-0<n>: la ejecuta <Clase.Metodo>
```

`<autoridad>` es `D-PB-06` en S-01 a S-03 y `D-PB-11` en S-04.

| Fila | Prueba vieja (archivo:línea del atributo) | Aserción vieja | Prueba nueva | Aserción nueva | Cobertura comparada |
|---|---|---|---|---|---|
| S-01 | WLC `TheWindowAndThePilotDifferOnlyInTheKnownClasses`, `[Theory]` en WLC:231, 14 filas | 11 filas con delta vacío; pb-06 y pb-07 con la línea de `DimensionStyle`, y pb-08 con dos líneas de clase (a) (WLC:184-204); ninguna línea `OUTSIDE` | `PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns` | Los 14, admitidos y con delta vacío | **Las mismas 14 filas.** Es más fuerte: no admite ninguna línea, y la rama `WINDOW-NOT-ADMITTED` (WLC:237-241) pasa a la aserción `admitida=True` en las 14 |
| S-02 | `PushBackDimensionViewsWindowTests.T19_H1_TheDimensionStyleStillComesOutNull`, `[Fact]` en `:308` | `Assert.Null` del estilo tras la carga y tras desmarcar `DimensionsLateralCheck` (`:319-320`) | `PushBackDeclaredCorrectionDimensionStyleTests.CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures` | El estilo cargado tras la carga y tras el mismo gesto | **La misma construcción y el mismo gesto,** más pb-06, pb-07, la biblioteca y un rack nuevo |
| S-03 | `PushBackFullEditorCharacterizationRoundingLossTests.CT_PB0_19_TheFieldsThatPassThroughAFormattedControlComeBackRoundedToThreeDecimals`, `[Fact]` en `:129` | F1-F11 en `REDONDEA` (`:135-150`) | `PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading` | Diez filas en `CONSERVA` y F4 en `CATALOGO` | **Las mismas once filas, con la misma construcción.** CT-PBTC-02, CT-PBTC-03 y CT-PBTC-10 añaden la petición, la biblioteca, la edición, la selección y la paridad con el piloto |
| S-04 | `PushBackFullEditorCharacterizationModuleTests.CT_PB0_05_TheSixAdvancedParametersAfterRestoreStandardAreResetInTheOutputAndComeBackOnUpdate`, `[Fact]` en `:170` | Diez líneas (`:199-208`). Cambian cuatro: **A1 controles** (`:202`, `:207`), con los valores cargados, y **A2** (`:203`, `:208`), con la petición de vuelta a lo cargado. El comentario dice «INCUMPLIMIENTO CARACTERIZADO… NO se corrige» (`:160-168`). Abajo, las cuatro literales | `PushBackDeclaredCorrectionStandardRestoreTests.CT_PBTC_14_AStandardRestoreResetsTheAdvancedControlsAndReachesTheUpdate` | A1 controles, reiniciados; A2, los valores reiniciados (§4.2) | **Los mismos dos diseños, los mismos pasos y las mismas diez líneas.** Seis no cambian (`:199-201`, `:204-206`) y cuatro se corrigen |

**S-04, las cuatro aserciones viejas literales** (`ModuleTests.cs:202`, `:203`, `:207` y `:208`):

```text
pb-06-seguridad A1 controles: RackHeaderHeightBox=[150] DerivedPostReinforcedCheck=False DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[] RackSeparatorCountBox=[3] RackSeparatorSpacingBox=[18]
pb-06-seguridad A2 peticion: ManualHeaderHeightOverride=150,DerivedPostReinforced=false,DerivedPostReinforcementHeight=null,DerivedPostHeight=null,SeparatorCountOverride=3,SeparatorSpacingOverride=18
pb-07-combinado A1 controles: RackHeaderHeightBox=[] DerivedPostReinforcedCheck=True DerivedPostReinforcementHeightBox=[] DerivedPostHeightBox=[140] RackSeparatorCountBox=[] RackSeparatorSpacingBox=[]
pb-07-combinado A2 peticion: ManualHeaderHeightOverride=null,DerivedPostReinforced=true,DerivedPostReinforcementHeight=null,DerivedPostHeight=140,SeparatorCountOverride=null,SeparatorSpacingOverride=null
```

**Las cuatro aserciones nuevas** son las de CT-PBTC-14 (§4.2).

**La comprobación temporal del Worker en local**, con código que no entra en ningún commit. Sobre el GREEN, cada prueba vieja sin su `Skip` falla por aserción exactamente en lo que cambia:
- S-01: en pb-06, pb-07 y pb-08 (las otras 11 filas pasan);
- S-02: en la línea del estilo;
- S-03: en diez filas. **La fila F4 sigue pasando**, porque la salida es 6 en la base y en el GREEN (H-29);
- S-04: en las cuatro líneas `:202`, `:203`, `:207` y `:208`; las otras seis pasan.

**Artefactos (M-09):** el TRX y el log de cada una de estas corridas temporales, y de la de D-PBTC-10, se guardan como artefactos de la entrega y se citan por ruta relativa en `worker-handoff.json`.

### 5.3 Lo que sigue válido sin cambio (medido)

| Prueba | Por qué no choca |
|---|---|
| WLC `ClassAIsEvidenced…` (`:254-264`) | Solo asevera sobre el diccionario estático (H-PB-41). Queda en verde y el registro lo anota |
| WLC `…KeepMine…` (`:270-313`) y el corpus (`:323-332`) | Escala 2,25 y tolerancia +2, sin estilo; el corpus no cambia |
| CT-PB0-20, G1-G9 | Esos campos no pasan por las once cajas (H-05, H-31). Siguen en `CONSERVA` en la carga |
| CT-PB0-01 y CT-PB0-17 (**M-05**) | Sus diseños no tienen ni estilo ni valores con más de tres decimales (H-23). Las listas (a)/(b) **siguen vacías** |
| CT-PB0-02, CT-PB0-13 a CT-PB0-15 | Comparan dos salidas de la misma ventana. Las dos llevan ya el estilo y los exactos |
| CT-PB0-04 (`ModuleTests.cs:124`) y `PushBackModuleEditorWindowTests.cs:295` | Pulsan «Restaurar estándar», pero solo aseveran la secuencia de módulos de `pb-31`, con los avanzados por defecto, o `IsManualOverride` (H-37) |
| La guarda Fact3 (`PushBackModuleEditorCharacterizationTests.cs:381-401`, Core) | D-PB-11 lee la intención y no nombra `forceRebuild:` ni `forceRebuild)` (D-PBTC-11; INV-PBTC-19) |
| CT-PB0-18 y CT-PB0-21 (Core) | Usan Application sin ventana (H-22). CT-PB0-21 queda como la medida histórica del efecto |
| `PushBackDimensionLegacyCharacterizationTests.cs:249-251` y las demás de `PushBackDimensionViewsWindowTests` | Application, o diseños sin estilo |
| T1B (55 + 10) y las guardas de H-18 | El diseño no toca nada de lo que miran (H-18) |
| `PushBackBlankFrontAuthorityTests` (OB-08.1) | Application. La regla no cambia |

### 5.4 El registro declarado (lo crea la sesión en la emisión)

`docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md`, con el formato de I-39:
- **Cabecera:**
  - autoridades: [OD] D-PB-06 (`:130-155`) y D-PB-08 (`:62-74`), OB-T1B.2, y [OPD] (F4B-E06) para D-PB-11 y las decisiones de presentación;
  - «cambio intencionado del editor clásico, no "no observable change"» ([OD]:149-150, paso 6);
  - estado: DECLARADA. Pasa a EJECUTADA con los SHA del RED y del GREEN al cerrar.
- **S-01 a S-04:** prueba vieja (clase, método, `archivo:línea` y filas), aserción vieja literal, prueba nueva, aserción nueva, autoridad y por qué es legítimo (§5.2).
  - S-01 anota además las 11 filas que no cambian.
  - S-03 anota que F4 no era una pérdida de la ventana (H-29) y que su fila sigue igual.
  - S-04 anota las seis líneas que no cambian y la autoridad D-PB-11.
- **Sin sustituir, con su motivo:** WLC `ClassAIsEvidenced…` (H-PB-41).
- **Pendientes de PB-T2,** anotados también en `f4b-dispositions.md` (precondición 3):
  - el paso 4 de D-PB-06 en modo WORKSPACE (OB-06.3);
  - el aviso de D-PB-08 en modo WORKSPACE (OB-08.2).
- **Pendiente: decisión del Owner antes de PB-T1** (OB-06.5; [OPD], pregunta 4, OD-PBTC-5 = (B)): los casos alcanzables de la clase (a) que PB-TC no corrige (H-31):
  - `GapBox`;
  - `ModuleLengthBox`;
  - las cuatro cajas avanzadas anulables: `ManualHeaderHeightOverride`, `DerivedPostReinforcementHeight`, `DerivedPostHeight` y `SeparatorSpacingOverride`.
- **Los cambios de comportamiento declarados de RACKEDITAR:**
  - (a) en diez campos, más `BeamDepthBox` sin efecto, con sus consecuencias (D-PBTC-01) y la extensión más allá de la carga;
  - el fondo exacto, que ya no reconstruye módulos (H-30);
  - (b), con su efecto en el dibujo (H-13) y en el sobre;
  - el aviso, en la banda de estado, con el literal de [OPD];
  - las rutas de la biblioteca y de «Insertar» (D-PBTC-02);
  - D-PB-11: tras «Restaurar estándar», los controles y la petición llevan el estándar (D-PBTC-11).

## 6. Contrato propuesto (a fijar en la emisión)

### 6.1 Identidad

- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B; `TaskId` **`F4B-PBTC-DECLARED-CORRECTIONS`**.
- Evidencia en `docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/`.
- Attempts 1 de 3; `RoutingEnforcement` `required`; `CorrectionsAuthorized` true.
- **Objective** (ASCII en el JSON): «Corregir en la ventana clasica de Push Back, como cambios declarados de RACKEDITAR, las perdidas (a) redondeo y (b) estilo de cotas de D-PB-06, la perdida de 'Restaurar estandar' al Actualizar (D-PB-11, H-PB-28), y anadir el aviso no bloqueante de D-PB-08 en la banda de estado, segun el paquete del contrato de PB-TC v2.1 y la lista cerrada CT-PBTC-01..08, 10 y 14. El RED son pruebas nuevas que fallan por asercion en la base y la sustitucion declarada S-01..S-04; el GREEN corrige solo la ventana. NumericField, Cantilever, Application y el Plugin no cambian; GapBox, ModuleLengthBox y las cajas avanzadas anulables quedan fuera; el paso 4 en el Workspace queda para PB-T2; no se ejecuta AutoCAD.»

### 6.2 `AllowedWriteScope` (13 rutas exactas)

- **Producto, tres:**
  1. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs`
  2. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs`
  3. `src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml`
- **Pruebas nuevas, seis:**
  4. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs`
  5. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionRoundingTests.cs`
  6. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionDimensionStyleTests.cs`
  7. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionPilotParityTests.cs`
  8. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSideBNoticeTests.cs`
  9. `tests/RackCad.UI.Tests/PushBackDeclaredCorrectionStandardRestoreTests.cs`
- **Sustituciones, cuatro** (solo la línea del atributo):
  10. `tests/RackCad.UI.Tests/Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs`
  11. `tests/RackCad.UI.Tests/PushBackDimensionViewsWindowTests.cs`
  12. `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationRoundingLossTests.cs`
  13. `tests/RackCad.UI.Tests/PushBackFullEditorCharacterizationModuleTests.cs`

### 6.3 `ForbiddenWriteScope` (130 entradas)

Con el método de [PK0-C], hermano por hermano. Un prefijo solo si ninguna ruta permitida queda debajo. Lo calcula `forbidden_v2.py` con el argumento `B` (medición K-5) y el algoritmo de `make_pbt0_contract.py`. Comprueba que las 130 existen en HEAD, que ninguna ruta permitida queda debajo de un prefijo y que toda ruta de `src/` no permitida queda cubierta.

- **`src/`, 58:**
  - `src/RackCad.Application/`, `src/RackCad.Domain/` y `src/RackCad.Plugin/`, enteros;
  - en `src/RackCad.UI/`, **cada hermano** salvo `Systems/`:
    - las carpetas `Controls/`, `Editor/`, `Preview/`, `RackFrames/`, `Shell/`, `StructuralSections/`, `Themes/`, `Views/` y `Workspace/`;
    - los 34 archivos raíz: `CatalogOption.cs`, `EnumDisplayConverter.cs`, `MainMenuAction.cs`, `ObservableObject.cs`, `PreviewCanvasPainter.cs`, `ProjectVariableRepairText.cs`, `RackBomWindow.xaml`, `RackBomWindow.xaml.cs`, `RackCad.UI.csproj`, `RackCommandHelpWindow.cs`, `RackCommandReference.cs`, `RackConsolidatedBomWindow.xaml`, `RackConsolidatedBomWindow.xaml.cs`, `RackCustomPropertiesWindow.xaml`, `RackCustomPropertiesWindow.xaml.cs`, `RackDesignLibraryWindow.xaml`, `RackDesignLibraryWindow.xaml.cs`, `RackListRow.cs`, `RackListWindow.xaml`, `RackListWindow.xaml.cs`, `RackMainMenuWindow.xaml`, `RackMainMenuWindow.xaml.cs`, `RackProjectVariablesWindow.xaml`, `RackProjectVariablesWindow.xaml.cs`, `RackWarehouseFillWindow.cs`, `RackWarehouseLayoutWindow.cs`, `SafetyDefensaGridWindow.cs`, `SafetyDefenseFace.cs`, `SafetyDesviadorGridWindow.cs`, `SafetyGuiaEntradaGridWindow.cs`, `SafetyParrillaGridWindow.cs`, `SafetyTopeGridWindow.cs`, `SelectiveSafetyWindow.cs` y `UiSupport.cs`, todos bajo `src/RackCad.UI/`;
  - en `src/RackCad.UI/Systems/`: `Cantilever/`, `Dynamic/`, `FlowBed/`, `Larguero/` y `Selective/`;
  - en `src/RackCad.UI/Systems/PushBack/`, los siete hermanos de la ventana: `PushBackBootSection.cs`, `PushBackDefenseSection.cs`, `PushBackMatrixCardModel.cs`, `PushBackPreviewModel.cs`, `PushBackPreviewRenderer.cs`, `PushBackRearTopeDialogAdapter.cs` y `PushBackRearTopeSection.cs`.
- **`tests/`, 55:**
  - `tests/RackCad.Tests/`, entero;
  - en `tests/RackCad.UI.Tests/`:
    - los ayudantes: `RackCad.UI.Tests.csproj`, `StaTestRunner.cs`, `EditorWindowTestSupport.cs`, `EditorDiscardPromptCollection.cs` y `PushBackFullEditorCharacterizationSupport.cs`;
    - las cuatro clases de PB-T0 que no se sustituyen: `PushBackFullEditorCharacterization{State,Composite,Subdialog,Catalog}Tests.cs`;
    - las seis de T1B: `PushBackClassicCharacterization{Keyboard,Close,Intent,Owner,Resource,SourceGuard}Tests.cs`;
    - las guardas de UI que leen la ventana o su estructura: `BlankFrontDesviadorHandoffTests.cs`, `EditorClosePolicyTests.cs`, `RichEditorCharacterizationTests.cs`, `RichEditorCloseContractTests.cs`, `RichEditorContractTests.cs`, `PushBackShellAdoptionTests.cs`, `PushBackModuleEditorWindowTests.cs`, `PushBackSafetyPerPostFlowTests.cs`, `PushBackSidebarCompositionTests.cs`, `UiSystemBoundaryGuardTests.cs` y `WindowCensusGuardTests.cs`;
    - las de OB-06.6: `CantileverAdvancedPanelWindowTests.cs`, `CantileverComponentEditorTests.cs`, `CantileverEditorWindowTests.cs`, `NumericFieldTests.cs` y `NumericFieldValidationTests.cs`;
  - los **23 hermanos de WLC** en `tests/RackCad.UI.Tests/Workspace/`: `WorkspaceBrowserScenarioHarnessTests.cs`, `WorkspaceBrowserSummaryPortDouble.cs`, `WorkspaceBrowserViewModelTests.cs`, `WorkspaceBrowserViewTests.cs`, `WorkspaceDraftPilotCanonicalLineEndingTests.cs`, `WorkspaceDraftPilotValidationParityTests.cs`, `WorkspaceNavigationPortDouble.cs`, `WorkspaceNavigationViewModelTests.cs`, `WorkspaceNavigationViewTests.cs`, `WorkspacePanelContextTests.cs`, `WorkspacePanelErrorTests.cs`, `WorkspacePanelInstrumentationTests.cs`, `WorkspacePanelNoWorkTests.cs`, `WorkspacePanelStateTests.cs`, `WorkspacePanelViewTests.cs`, `WorkspacePilotCloseSyncTests.cs`, `WorkspacePilotCloseSyncViewTests.cs`, `WorkspacePilotEditPortDouble.cs`, `WorkspacePilotFieldViewModelTests.cs`, `WorkspacePilotFieldViewTests.cs`, `WorkspacePilotReconciliationTests.cs`, `WorkspacePilotReconciliationViewTests.cs` y `WorkspacePortDouble.cs`.
- **Raíz, diecisiete:** `docs/`, `.github/`, `eng/`, `tools/`, `assets/`, `deploy/`, `RackCad.sln`, `Directory.Build.props`, `Directory.Build.targets`, `global.json`, `NuGet.Config`, `README.md`, `AGENTS.md`, `CLAUDE.md`, `.editorconfig`, `.gitattributes` y `.gitignore`.

El orden exacto, con número de entrada, es la salida de `forbidden_v2.py <worktree> B`, y el script generador del contrato lo reproduce.

### 6.4 `RequiredTests`

| Proyecto | Filtro | `MinSelected` | `ExpectRed` | Para qué |
|---|---|---|---|---|
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection` | 10 | true | CT-PBTC-01 a 08, 10 y 14; los diez fallan en el RED |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.Cantilever` | 67 | false | Cantilever no cambia (OB-06.6); recuento estático (H-27) |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization` | 55 | false | T1B en verde (OB-T1B.3) |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization` | 16 | false | PB-T0 en verde: 16 Passed, y CT-PB0-19 y CT-PB0-05 Skipped (S-03, S-04); M-05 |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.WorkspaceDraftPilotWindowLoadCharacterizationTests` | 3 | false | 3 Passed y la Theory, 1 Skipped (S-01) |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackDimensionViewsWindowTests` | 12 | false | 12 Passed y `T19_H1` Skipped (S-02) |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackAdvancedRackParametersWindowTests` | 9 | false | D-PB-11 no rompe el panel avanzado ni «Restaurar parámetros» |
| `tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj` | `FullyQualifiedName~RackCad.UI.Tests.PushBackModuleEditorWindowTests` | 13 | false | D-PB-11 no rompe la edición de módulos ni «Restaurar estándar» (`:295`) |
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization` | 10 | false | T1B en verde |
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization` | 3 | false | CT-PB0-10, CT-PB0-18 y CT-PB0-21 |
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackBlankFrontAuthorityTests` | 9 | false | OB-08.1 |
| `tests/RackCad.Tests/RackCad.Tests.csproj` | `FullyQualifiedName~RackCad.Tests.PushBackModuleEditorCharacterizationTests` | 15 | false | La guarda Fact3 y las demás guardas de fuente de módulos (H-37) |

- Los `MinSelected` de las filas con `Skip` cuentan solo los que pasan. Valen tanto si el Controller cuenta como seleccionado un resultado `Skipped` como si no (R-09).
- Los tres filtros nuevos (las dos clases de UI y la de Core) usan recuentos estáticos sin `Skip` (H-27). Sus nombres no son prefijo de ninguna otra clase (medición L-6).

### 6.5 Invariantes

- **INV-PBTC-01. Alcance.**
  - `git diff --name-only BaseSha..CurrentSha`, dentro de las rutas de `AllowedWriteScope`.
  - En `src/`, solo W, WC y WX.
  - `git diff BaseSha..CurrentSha -- src/RackCad.UI/Controls src/RackCad.UI/Systems/Cantilever src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin`, vacío.
- **INV-PBTC-02. RED y GREEN.**
  - **El RED:** solo pruebas, es decir, los seis archivos nuevos y la línea del atributo de S-01 a S-04, sin ningún archivo de `src/`. Se publica antes del GREEN.
  - **El GREEN:** solo W, WC y WX, sin ningún archivo de `tests/`.
  - **Criterio mecánico del RED,** solo sobre el filtro con `ExpectRed` y con los resultados de la CI del RED:
    - todos `Failed`;
    - `<Message>` sin el prefijo de tipo `^(?:[A-Za-z_][\w`]*[.+])+[A-Za-z_][\w`]* : `;
    - la primera línea de `<StackTrace>`, bajo `tests/`;
    - ninguno `NotExecuted` ni `Skipped`;
    - `Failed == Selected ≥ 10`.
  - **En el RED,** los filtros sin `ExpectRed` pasan, y sus únicos `Skipped` son S-01 a S-04.
- **INV-PBTC-03. La corrección (a), comprobada sobre el diff:**
  - **(1)** ninguna línea añadida ni quitada de `git diff -U0 BaseSha..CurrentSha -- <W> <WC>` contiene `SetNumber(`;
  - **(2)** en W, las líneas `Val(` de W:797-801, :819, :837-839 y :845 desaparecen. Aparecen **diez llamadas `Keep(` sobre esas diez cajas**, sin contar la declaración del método, y cambia `OptionalVal` (W:3625-3626);
  - **(3)** `Val` sigue declarada (W:3594) y W:1865 no cambia;
  - **(4)** el comportamiento lo fijan CT-PBTC-01 a 03 y 10 en verde.
- **INV-PBTC-04. La corrección (b).**
  - El estilo de cotas que trae la carga llega sin cambio a cada pasada (CT-PBTC-04 y 05).
  - Un rack nuevo, sin estilo.
  - En el diff de WX no aparece ningún control nuevo de estilo.
- **INV-PBTC-05. D-PB-08** (D-PBTC-03).
  - **El elemento:** un `TextBlock` llamado `SideBEmptyNoticeText`, el último hijo del `StackPanel` de `StatusContent` (WX:681-685), detrás de `StatusText`, con los atributos de D-PBTC-03.
  - **La regla:** visible si el lado B está declarado y ninguna de sus ranuras está activa; colapsado en cualquier otro caso.
  - **El texto:** el literal de [OPD], pregunta 3, byte a byte.
  - **Lo que no es ni hace:** no es un `Control` ni es enfocable. No cambia la habilitación de ningún botón, el estado, el diseño, la casilla ni `StatusText`.
  - **La regla de capacidad sin presencia:** sin cambio.
- **INV-PBTC-06. La sustitución declarada.**
  - Solo S-01, S-02, S-03 y S-04 llevan `Skip`, cada una con su literal del §5.2.
  - En esos archivos, el diff es exactamente la línea del atributo.
  - Ninguna otra prueba existente se edita, se salta, se reactiva ni se borra.
- **INV-PBTC-07. T1B y guardas.**
  - T1B en verde y sin sustitución: 55 en UI y 10 en Core.
  - G-10, I-55, módulos (con Fact3), desviador, foco, `AnnotationScaleBox`, las cuatro de H-18, el censo de ventanas y el corpus de WLC, en verde en la CI completa.
- **INV-PBTC-08. Lo que sigue válido:**
  - CT-PB0-01 a CT-PB0-04, CT-PB0-06 a CT-PB0-18, CT-PB0-20 y CT-PB0-21, en verde;
  - las listas (a)/(b) de CT-PB0-01 y CT-PB0-17, vacías (M-05);
  - lo del §5.3, en verde.
- **INV-PBTC-09. Sin efecto no medido.**
  - La salida de la ventana, igual a la del piloto en los 14 diseños de WLC (CT-PBTC-06) y en las 12 filas de CT-PBTC-10.
  - Una diferencia en cualquier otro campo es PBTC-C03.
- **INV-PBTC-10. Las pruebas de UI.**
  - **La colección:** las cinco clases nuevas, en `EditorDiscardPromptCollection`.
  - **Las ventanas:** cada una con `Ventana()` y su carga dentro del `try`, y cerrada en `finally`, también si la carga lanza. Sin `Abrir` ni `AbrirFueraDePantalla` de S0. Ninguna ventana mostrada.
  - **Los gestos:** sin teclado ni foco real (D-PBTC-09).
  - **Prohibido:** un `MessageBox` real, el BOM, «Guardar en biblioteca», el configurador real e «Insertar».
  - **Las costuras:** ninguna aserción dentro de una.
- **INV-PBTC-11. Las costuras.**
  - Solo las de H-28 y el contrato público de la ventana.
  - Ninguna costura nueva, y ningún cambio en `InternalsVisibleTo`, los paquetes ni los `.csproj`.
- **INV-PBTC-12. Determinismo:**
  - el GUID fijo de `Ventana()`;
  - la cultura invariante;
  - los números con «R»;
  - el orden ordinal;
  - ni hora ni aleatoriedad.
- **INV-PBTC-13. Las corridas locales.**
  - **Nunca la suite completa de UI en local,** solo los filtros del §6.4. La de Core, sí.
  - **MAX_PATH:** sin copias de árboles.
  - **Las corridas temporales** (§5.2 y D-PBTC-10) no entran en ningún commit. Sus TRX y sus logs se guardan como artefactos y se citan en la entrega (M-09).
  - **El perfil del usuario:** sin cambios, con la prueba de la limpieza de INV-PBT0-06.
- **INV-PBTC-14. CI y compilación.**
  - **GREEN:** 4/4 con las suites completas.
  - **RED:** `ui-tests` en `failure`; `tests`, `build-ui` y `build-plugin` en `success`; sin `blame-hang` ni timeout. `build-plugin` depende de `tests` y `build-ui` (`.github/workflows/ci.yml:254-258`, IRT1 §2.7).
  - **Avisos:** los conteos por código, iguales a la línea base (Core 66 xUnit, UI 28 xUnit y Plugin 104 MSB3277; PB-T0, Evidence 26).
- **INV-PBTC-15. El alcance de la tarea, comprobado sobre el diff.** Cada hunk de `git diff -U0 BaseSha..CurrentSha -- src` cae dentro de un miembro de esta lista cerrada, por su rango de líneas en `BaseSha`, o es la declaración de un miembro nuevo de la lista:
  - **en W, miembros existentes:**
    - `LoadFromModel` (W:377-427), solo las líneas de los portadores y de `loadedDimensionStyle`;
    - `Recompute` (W:603-702), solo la llamada del aviso tras W:607 y las líneas de D-PBTC-11 alrededor de W:682;
    - `ReadCellValues` (W:781-810);
    - `ReadInputs` (W:812-857);
    - `OptionalVal` (W:3625-3626);
  - **en W, miembros nuevos:** `ShownFormat`, `SameShown`, `Keep`, los cuatro portadores `rackTolerance`, `rackPostPeralte`, `rackBeamDepth` y `rackAnnotationScale`, y `loadedDimensionStyle`;
  - **en WC:** la constante del aviso y su método nuevo;
  - **en WX:** el `TextBlock` del aviso.

  **Fuera, por la letra** (OD-PBTC-5 = (B); D-PBTC-07), ningún hunk toca:
  - `Gap_Changed` (WC:159-175) ni la siembra de `GapBox` (WC:715-749, con WC:721);
  - `ModuleLength_Changed` (W:1852-1873) ni la siembra de `ModuleLengthBox` (W:1766-1824, con W:1793);
  - `LoadAdvancedRackParameters` (W:1555-1579), `SetOptional` (W:1582-1589), `ReadAdvancedRackParameters` (W:1592-1607), su manejador (W:1616-1626) ni `RestoreRackParameters_Click` (W:1628-1634);
  - `Val` (W:3594).
- **INV-PBTC-16. El registro.**
  - El Worker no escribe en `docs/`.
  - El registro, su paso a EJECUTADA y la anotación del paso 6 en `f4b-dispositions.md` son de la sesión.
- **INV-PBTC-17. Las reglas de F4B** ([DP]; D-PB-10):
  - la planificación, según PBTC-C08;
  - los controles, según PBTC-C11;
  - antes de un intento 3, siempre el Owner (PBTC-C12).
- **INV-PBTC-18. Sin AutoCAD.** La comprobación física es de Smoke-PB ([P] §9).
- **INV-PBTC-19. D-PB-11, comprobada sobre el diff** (D-PBTC-11):
  - **(1)** dentro de la rama válida de `Recompute` (W:658-692), el diff añade exactamente:
    - una línea antes de W:682 que lee `moduleOwner.ModuleCommit?.StandardRestoreRequested`;
    - tras W:682, un `if` cuyo cuerpo son exactamente `state.RestoreAdvancedRackParameters(advanced);` y `LoadAdvancedRackParameters();`;
  - **(2)** ninguna línea añadida en ese hunk contiene `RequestRecompute(`;
  - **(3)** W, en `CurrentSha`, no contiene `forceRebuild:` ni `forceRebuild)` y sigue conteniendo `RequestStandardRestore()`. Es la guarda Fact3, que corre en Core;
  - **(4)** el comportamiento lo fija CT-PBTC-14 en verde.

### 6.6 `ExpectedEvidence`

- **Antes del Worker:**
  - el contrato y la delegación válidos;
  - la comparación byte a byte, A1-A8 y nc4 (primera delegación de la cadena);
  - el `AllowedWriteScope` de la delegación igual al del contrato;
  - si hubo una replanificación por PBTC-C08, su registro.
- **Paso 1, sobre `BaseSha`** (D-PBTC-10):
  - la corrida temporal de CT-PBTC-10, con `Assert.Fail` y el mensaje completo;
  - el **delta de la base de CT-PBTC-10, transcrito fila por fila** en `worker-handoff.json`, leído del TRX de esa corrida;
  - K1, K2, K3 y K4 de las filas F1 y M-02;
  - el TRX y el log, como artefactos;
  - la primera diferencia de CT-PBTC-10 en el TRX de la CI del RED, citada para contrastarla con lo transcrito.
- **El commit RED y su push, antes del GREEN:**
  - la CI del RED de INV-PBTC-14;
  - los TRX del filtro con `ExpectRed` cumplen el criterio de INV-PBTC-02;
  - los demás filtros, en verde, con sus `Skipped` de S-01 a S-04.
- **El commit GREEN y su push:**
  - `git diff --name-only RedSha..CurrentSha` = W, WC y WX;
  - las comprobaciones de INV-PBTC-03, INV-PBTC-15 e INV-PBTC-19 sobre el diff, con su salida;
  - la CI del GREEN 4/4, con los conteos del §6.9 o la diferencia explicada por el diff.
- **En local:**
  - el build Release de la solución, con el Plugin y sin avisos nuevos, sobre el RED y sobre el GREEN;
  - los filtros del §6.4 sobre el RED y sobre el GREEN;
  - tres corridas seguidas en verde del filtro con `ExpectRed` sobre el GREEN;
  - la comprobación temporal del §5.2 (S-01 a S-04 sin `Skip` sobre el GREEN);
  - la prueba de la limpieza del perfil.
- **`worker-handoff.json`, con:**
  - el RED, el GREEN, el alcance real, el trailer y los conteos por filtro, con Passed, Failed y Skipped;
  - el mapa CT → método (§4.4);
  - por cada CT, la primera diferencia en la base, con el mensaje del TRX;
  - la transcripción del paso 1;
  - la tabla de S-01 a S-04, con la prueba vieja en la base (pasa) y sin `Skip` sobre el GREEN (falla en las filas o líneas declaradas; en S-03, F4 pasa);
  - el literal de D-PB-08 tal como quedó;
  - las rutas relativas de los artefactos de las corridas temporales;
  - `KnownLimitations`.
- **Después:**
  - la verificación del Controller, válida;
  - nc1, nc2 y nc3 según el README §10, conservando cada salida ordinaria;
  - un control que no da CONTROL PASS se resuelve **solo** como dice PBTC-C11;
  - cada uso, registrado en `f4b-dispositions.md`;
  - la verificación 14/14 sola no basta para cerrar.

### 6.7 `StopConditions`

**Las estándar,** como en [PK0-C]:
- S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14;
- P-02 a P-08;
- P-01 con el texto de [PK0-C]: «config.toml no longer matches the accepted Snapshot in force (today Snapshot I, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)».

**Las propias:**
- **PBTC-C01:** la corrección necesitaría cambiar un archivo fuera de las tres rutas de producto. Por ejemplo `NumericField`, otra ventana, Application o el Plugin. Es una decisión del Owner (OB-06.6).
- **PBTC-C02:** habría que editar, saltar, reactivar o borrar una prueba o un ayudante existente fuera de S-01 a S-04, o tocar un `.csproj`, `InternalsVisibleTo`, los paquetes o `tests/RackCad.Tests/`. **Una guarda de T1B, o la guarda Fact3, que no queda en verde vuelve al Owner** (OB-T1B.3).
- **PBTC-C03 (OB-06.4):** aparece un efecto material no medido. Es decir, una diferencia en el diseño, en las piezas, en los niveles o posiciones o en el BOM que no sale de los valores exactos de los campos corregidos, del estilo ni del reinicio de D-PB-11; o falla, en el GREEN, CT-PBTC-06, CT-PBTC-10, CT-PBTC-14 o una prueba del §5.3. STOP y vuelta al Owner.
- **PBTC-C04:** un método del filtro con `ExpectRed` pasa en la base, o falla por excepción y no por aserción. Es un defecto del contrato: el Worker no cambia la prueba para que falle.
- **PBTC-C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador real, el BOM, «Guardar en biblioteca», «Insertar», el teclado o el foco real, o una escritura en el perfil o en el disco.
- **PBTC-C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce en una segunda corrida y no es atribuible a ellas. Si es atribuible, es REWORK; si no se reproduce, BLOCKED, dentro de los límites de 16.8.
- **PBTC-C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño; o una línea esperada de un CT no se cumple en el GREEN. El Worker no ajusta lo esperado.
- **PBTC-C08** ([DP], pregunta 2):
  - un rechazo de la planificación **solo** por la comparación literal de las colecciones permite **una** replanificación con el prompt reforzado, sin gastar intento;
  - un segundo rechazo de fidelidad es STOP y vuelve al Owner;
  - cualquier otro rechazo de la planificación (A1-A8 u otra causa) es STOP y vuelve al Owner.
- **PBTC-C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **PBTC-C10:** hacerlo bien exigiría corregir algo fuera del alcance contratado:
  - **las cajas de H-31,** que el Owner dejó fuera ([OPD], pregunta 4, OD-PBTC-5 = (B)): `GapBox` y `Gap_Changed`, `ModuleLengthBox` y `ModuleLength_Changed`, y las cuatro cajas avanzadas anulables con `ReadAdvancedRackParameters` y su manejador. También cualquier miembro de la lista «fuera» de INV-PBTC-15;
  - (c), (d), la cola de `pb-21-cola`, D-3 o M-01.
- **PBTC-C11** ([DP], pregunta 1). Un control que no da CONTROL PASS se conserva tal cual, y solo hay dos salidas sin volver al Owner:
  - **nc2 con la firma exacta de PD-F4-01.** El texto literal de su fuente (`f4-dispositions.md:1103-1106`):

    > «- Scope en fail sobre la ruta quitada;
    > - Contract en fail solo por la cláusula de exactitud;
    > - `EXECUTION_BLOCKED`/STOP.»

    Y su aplicación literal (`:1540`): «ninguna otra comprobación distinta de la verificación real».

    **Qué cuenta como la cláusula de exactitud:** forma parte de la firma que la evidencia de Contract cite el contraste del `AllowedWriteScope` estrechado con el contrato o con `byte-fidelity-check.json` (`f4-dispositions.md:520`, `:562`). No forma parte que Contract cite cualquier otra discrepancia.

    → El sustituto **`PBTC-NC2-EXACT-WHITELIST-01`**: copia de `F4T1B-NC2-EXACT-WHITELIST-01` (`docs/automation/evidence/I-64-pilot/F4-T1B-PILOT-CHARACTERIZATION-nc2-substitute/1/R20261114T235423Z-083e/nc2_exact_whitelist_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc2 = SATISFIED_UNDER_PBTC_EXACT_WHITELIST_SUBSTITUTE`.
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

    → El sustituto **`PBTC-NC1-IDENTITY-SIGNAL-01`**: copia de `F4T3C2-NC1-IDENTITY-SIGNAL-01` (`docs/automation/evidence/I-64-pilot/F4-T3C2-COMMAND-ADVISOR-nc1-substitute/1/R20261113T215949Z-935f/nc1_identity_signal_substitute.py`) con **solo** el identificador y el ámbito cambiados. Registro `nc1 = SATISFIED_UNDER_PBTC_NC1_IDENTITY_SIGNAL_SUBSTITUTE`.
  - **Cualquier otra firma o variación vuelve al Owner antes de cualquier sustituto** ([DP], pregunta 1, descripción de la opción). Por ejemplo:
    - PD-F4-05 y PD-F4-06;
    - nc2 con Ci o Trailer en fail además;
    - cualquier nc1 que no reproduzca la firma literal;
    - cualquier fallo de nc3.
  - **Cómo se decide la coincidencia.** Nunca por el nombre de la comprobación:
    - **qué se compara:** la salida del control frente a la de la verificación real, comprobación por comprobación, con el conjunto de comprobaciones en fail, la primera en fail, el `Outcome`, la `StopClass` y el texto de la evidencia;
    - **las únicas diferencias admitidas:**
      - PD-F4-01: Scope, en fail sobre la ruta quitada, y Contract, en fail solo por el alcance;
      - PD-F4-04: Identity (nombra el SHA falso y el HEAD), Handoff (primera en fail, solo porque contiene el SHA falso) y Ci (solo como consumidor del `CurrentSha` falso);
      - en las dos, el resultado `EXECUTION_BLOCKED` / STOP;
    - **quién decide y dónde queda:** la sesión registra la tabla en `f4b-dispositions.md`. Una diferencia fuera de esa lista vuelve al Owner.
- **PBTC-C12:** una corrección sería el intento 3. Vuelve al Owner antes de emitirse (D-PB-10).
- **PBTC-C13:** el aviso de D-PB-08 necesitaría otro texto u otro lugar distintos de los que eligió el Owner ([OPD], pregunta 3), ser enfocable o bloquear una acción. Es una decisión de presentación del Owner (OB-08.4).
- **PBTC-C14 (I-02; OB-06.4; [OPD], pregunta 1):** en el delta de la base medido en el paso 1 (D-PBTC-10), la fila F1 o la fila M-02 de CT-PBTC-10 cumple una de dos:
  - tiene alguna línea bajo `$.PushBack.Structure.Modules` o `$.PushBack.Structure.HeaderLineOverrides`;
  - es material por la fórmula de CT-PB0-21 (`RoundingEffectTests.cs:164`): K1 o K4 no vacías (K4 = coordenadas desplazadas más de 0,001), o K2 o K3 ciertas.

  En ese caso:
  - el Worker se detiene **antes del GREEN**. Si ya publicó el RED, lo deja publicado y lo declara;
  - entrega la transcripción, y la sesión lleva el dato al Owner;
  - si el Owner ordena seguir, él fija en su orden cómo continúa la cadena y si se gasta intento.

### 6.8 `EligibleCells`

Las de [PK0-C]:
- **Worker:** `anthropic/subagent/claude-sonnet-5-5`, effort `high` (VerifiedOn 2026-09-30).
- **Controller:** `openai/process/gpt-6-luna`, effort `high` (VerifiedOn 2026-11-11, por la sonda neutra de F4).
- **Vigencia:** las dos son `STALE` desde el 2026-12-29, por la fecha del catálogo más 90 días (`model-catalog.md:14`).
- nc4 en la primera delegación, y nc1 a nc3.

### 6.9 Línea base de la CI y conteos previstos

**La línea base,** medida con `gh` sobre la CI de `f9f2afbb` (run `44188678597`, H-25):
- Core: 19 070 Passed de 19 070. Esa cifra **ya incluye** las 3 de PB-T0 (H-PB-40).
- UI: 2 404 Passed, 17 Skipped y 2 421 en total.

**La previsión.** Sale del diff, no de una ejecución:
- se añaden 10 métodos nuevos en UI, todos en el filtro con `ExpectRed`;
- la Theory de WLC pasa de 14 resultados a 1 (H-26);
- `T19_H1`, CT-PB0-19 y CT-PB0-05 siguen contando 1 cada una, ahora `Skipped`;
- Core no cambia: 19 070 en el RED y en el GREEN.

| CI | UI total | Failed | Skipped | Passed | Core |
|---|---|---|---|---|---|
| RED | 2 418 | 10 | 21 | 2 387 | 19 070 Passed |
| GREEN | 2 418 | 0 | 21 | 2 397 | 19 070 Passed |

La cuenta es 2 421 + 10 − 13 = 2 418 resultados, y 17 + 4 = 21 `Skipped`. Una cifra distinta en la CI se registra y se explica por el diff. Si no se explica, es PBTC-C06 o PBTC-C07.

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-01 | nc2 con la firma de PD-F4-01 | [DP]: el sustituto solo con la firma exacta; cualquier variación, al Owner (PBTC-C11) |
| R-02 | **Las líneas esperadas salen de leer el código.** No se compiló ni se ejecutó nada. IRT1 encontró una (F4) que no se habría cumplido, e ICT2 una clase mal aplicada (N-02) | Corregidas. Ante otra, PBTC-C07: STOP, sin ajustar lo esperado. El contrato no da al Worker ninguna latitud para corregir una prueba |
| R-03 | **«Escribir algo que se ve igual» conserva el exacto** (CT-PBTC-03, paso 3) | Decidido por el Owner: R-03 (i) ([OPD], pregunta 1) |
| R-04 | **«Ambos lados», el alcance y los datos del frente** llevan ahora el valor exacto | Extensión aceptada por el Owner ([OPD], pregunta 1). Una prueba existente que la contradiga es PBTC-C03 |
| R-05 | **La biblioteca y «Insertar»** conservan también el estilo y los exactos | Consecuencia declarada (D-PBTC-02), anotada en el registro |
| R-06 | **El literal de D-PB-08 queda fijado por CT-PBTC-07** | Lo eligió el Owner ([OPD], pregunta 3). Cambiarlo después es otro cambio declarado |
| R-07 | **F4-E59 / I-02:** la suite completa de UI escribe en el perfil | Solo los filtros en local; la suite completa de UI, solo en la CI (INV-PBTC-13) |
| R-08 | **PB-T1 moverá los portadores al estado de Application** | Los CT aseveran sobre la salida, la petición y los controles, nunca sobre los portadores privados |
| R-09 | **Cómo cuenta el Controller un resultado `Skipped`** en un filtro sin `ExpectRed` | `MinSelected` sobre los que pasan (§6.4) |
| R-10 | **El RED con Core en `success`** | Precedente de F4-T3B1, con Core `ExpectRed` false (D-PBTC-05) |
| R-11 | **Los recuentos de 67 (Cantilever), 9, 13 y 15** son estáticos | `MinSelected` es un mínimo. La entrega da la cifra medida |
| R-12 | **Las celdas vencen el 2026-12-29** | Se comprueba en la emisión y en cada delegación (A7) |
| R-13 | **MAX_PATH** | Sin copias de árboles. Las corridas temporales se hacen y se revierten en el mismo worktree |
| R-14 | **El delta de la base de CT-PBTC-10 no está medido** y puede ser material (H-PB-43) | Medido en el paso 1 (D-PBTC-10); si es material, PBTC-C14 lo lleva al Owner antes del GREEN |
| R-15 | **El TRX del RED trunca los mensajes** (H-PB-45) | La transcripción sale de la corrida temporal con `Assert.Fail(texto)`. Que `Assert.Fail` no trunque es conducta de xUnit 2.9, sin medir |
| R-16 | **El aviso en modo WORKSPACE** podría no decir lo correcto si el borrador conserva la capacidad | Obligación de PB-T2, anotada al emitir (precondición 3) |
| R-17 | **D-PB-11 añade código junto a `ClearModuleCommit`** y la guarda Fact3 lee W como texto | El diseño no nombra `forceRebuild:` ni `forceRebuild)` (D-PBTC-11). INV-PBTC-19 lo comprueba, y el filtro de Core lo corre (§6.4) |
| R-18 | **El caso de H-31 queda sin corregir** hasta la vuelta al Owner | Registro «pendiente» (§5.4) y anotación en `f4b-dispositions.md` (precondición 3). PB-T1 no se emite sin esa decisión (§10) |

## 8. Las decisiones del Owner (decididas)

Son las cinco decisiones de la vuelta prevista antes de PB-TC, con el punto informativo. Están transcritas literales en [OPD] (F4B-E06), y este paquete las aplica.

### 8.0 Resumen de lo decidido

**OD-PBTC-1. Los decimales se guardan exactos.** Decidido: corregir con la regla de Cantilever.
- **Qué cambia para quien usa RACKEDITAR:** al abrir un rack ya no se redondean a tres decimales los diez números que hoy se redondean sin tocarlos. Son el fondo y la tolerancia de la tarima, el peralte del poste, la escala de textos y cotas, la altura del primer nivel, y en la celda el frente, el alto, el peso, el claro y el largo manual. Las cajas siguen mostrando tres decimales.
- **El peralte IN/OUT** no cambia: se guarda siempre como el del catálogo.
- **Alcance:** vale al abrir, al cambiar de celda, con «Ambos lados», al aplicar a otras celdas y al cargar de la biblioteca.
- **Si se escribe en una caja algo que se ve igual** a lo que muestra, se conserva el valor exacto. Si se escribe algo distinto, manda lo escrito.
- **Medido,** con valores de prueba:
  - los textos y las cotas se mueven hasta 0,018 pulgadas si la escala trae decimales, y las piezas hasta 0,0013 si los trae el peralte del poste;
  - el BOM cambia si los traen el frente de la tarima o el largo manual;
  - no cambian ni el número ni el tipo de las piezas, ni los niveles.
- **No medido:**
  - las líneas exactas del BOM y el movimiento real en la planta con el frente de la tarima;
  - cuántos racks reales traen esos decimales;
  - qué hace hoy RACKEDITAR con los módulos de un rack cuyo fondo trae decimales. Se medirá antes de corregir, y si cambian las piezas, los niveles, las coordenadas o el BOM, la tarea se detiene y vuelve a usted.
- **Los racks antiguos sin marcador de datum** pueden ganar decimales en la altura del primer nivel al abrirse. El troquel absorbe la diferencia en el dibujo.

**OD-PBTC-2. El estilo de cotas se conserva.** Decidido: corregir.
- **Qué cambia:** RACKEDITAR deja de olvidar y de borrar el estilo de cotas guardado en el rack. Si el dibujo tiene un estilo con ese nombre, las cotas salen con él; si no, como hoy. Lo mismo al insertar una vista o al guardar en la biblioteca.
- **Medido:** en los 22 racks reales medidos no cambia nada, porque ninguno tiene estilo ni dibuja cotas.

**OD-PBTC-3. El aviso del lado B vacío.** Decidido: el texto corto, en la franja de estado.
- **Qué cambia:** al marcar el lado B sin activar ningún frente de B, aparece en la franja de estado, con la misma letra que la línea de estado, este texto:

  ```text
  Lado B declarado sin ningún frente activo: así no se guardará. Para conservarlo, elige «Lado B» y desmarca «En blanco» en al menos un frente.
  ```

- Solo informa: no bloquea nada y no se puede enfocar.

**OD-PBTC-4. «Restaurar estándar» llega al Actualizar.** Decidido: arreglarlo en esta tarea.
- **Qué cambia:** tras «Restaurar estándar» y «Confirmar», la altura manual de cabecera, el poste derivado, su refuerzo y los separadores se muestran restaurados, y «Actualizar» escribe lo restaurado, no lo anterior. Cambia lo que se dibuja en ese caso, que pasa a ser lo que se pidió.
- **Coste:** una prueba nueva y la sustitución de la prueba que fijaba el defecto.

**OD-PBTC-5. Las otras cajas que redondean.** Decidido: no entran en esta tarea.
- El hueco entre lados, la longitud de un módulo y cuatro parámetros avanzados siguen redondeándose como hoy.
- La pregunta vuelve a usted antes de la tarea que separa el estado de la ventana.

**Informativo.** El arreglo de los decimales y del estilo, y el aviso, se comprobarán en el modo Workspace cuando ese modo exista, en la tarea que lo añade. Usted pidió probar los dos modos; queda anotado como obligación de esa tarea.

### OD-PBTC-1. El efecto de corregir (a), el redondeo — DECIDIDO

**Decisión:** «Corregir, regla Cantilever (Recomendado)» ([OPD], pregunta 1). **Alcance:** OD-PBTC-1 (i), con la extensión declarada más allá de la ruta de carga (selección de celda, «Ambos lados», alcance, datos del frente y biblioteca), y R-03 (i), la regla `Keep` de Cantilever. La vuelta por PBTC-C14 se mantiene.

**Lo que se le presentó y queda como registro:**
- **Diez** números se guardan hoy redondeados a tres decimales aunque el usuario no los toque:
  - del rack: el fondo de la tarima, el peralte del poste, la tolerancia y la escala de anotación;
  - del frente seleccionado: la altura del primer nivel;
  - de la celda seleccionada: frente, alto y peso de la tarima, el claro y el largo manual de viga.
- **El peralte IN/OUT base** también tiene caja, pero **no es una pérdida**: se guarda siempre como el peralte del catálogo, `6` (H-29).
- **Un rack con tres decimales o menos no cambia,** salvo un rack antiguo sin marcador de datum, cuya altura del primer nivel reexpresa la carga (H-PB-47). El troquel absorbe la diferencia (T-3 R6).

**El efecto medido** (T-3 de PB-T0, CT-PB0-21, sobre `pb-08` con +0,00044, exacto frente a redondeado):

| Fila | Campo | Qué cambia | Máx. \|Δ\| | «Material» |
|---|---|---|---|---|
| R0 | `pb-08` tal cual: la escala 1,23456 y el alto 61,23456 | Los textos y las cotas de los 8 planos se mueven y cambian de tamaño | 0,01848 | Sí (K4) |
| R1 | Fondo de la tarima | Nada en los planos ni en el BOM. **No medido:** la reconstrucción de módulos de RACKEDITAR | 0 | No |
| R2 | Peralte del poste | Las piezas de los dos frontales y de la planta se mueven | 0,00132 | Sí (K4) |
| R3 | Tolerancia | Nada | 0 | No |
| R4 | Peralte IN/OUT base | Nada: los dos diseños resuelven a 6 (H-29) | 0 | No |
| R5 | Escala de anotación | Lo mismo que R0 | 0,01848 | Sí (K4) |
| R6 | Altura del primer nivel | Nada: el troquel la absorbe | 0 | No |
| R7 | Frente de la tarima | **Cambia el BOM** (K3). Cambia el orden interno de las piezas en el frontal posterior y la planta (K1o). En los planos distintos de la planta el movimiento es de 0,001 o menos | 106,988 (artefacto) | Sí (K3) |
| R8 | Alto de la tarima | Nada | 0 | No |
| R9 | Peso de la tarima | Nada | 0 | No |
| R10 | Claro | Nada | 0 | No |
| R11 | Largo manual de viga | **Cambia el BOM** (K3). Las piezas se mueven menos de 0,001 | 0,00044 | Sí (K3) |

**Lo que dice esta tabla y lo que no:**
- **Ninguna fila** cambia el número, el tipo o la capa de las piezas, ni los niveles o posiciones (K1 y K2, vacías en todas).
- **R7 y R11 son «materiales» por el BOM exacto.** Las líneas exactas del BOM **no se midieron.**
- **El 106,988 de R7 es un artefacto** del emparejamiento por orden cuando hay K1o (OWNER-4). **El desplazamiento real de R7 en la planta no se midió.**
- **T-3 no ve la reconstrucción de módulos** que hoy hace RACKEDITAR en la primera pasada con un fondo de más de tres decimales (H-30, H-PB-43). La cadena la mide en el paso 1 (CT-PBTC-10, filas F1 y M-02; D-PBTC-10). Si es material, vuelve al Owner antes de corregir (PBTC-C14).
- **En los dibujos reales medidos** (H-PB-38), la escala de los 22 sobres es 1. Los demás campos no se midieron.
- **El panel del Workspace ya escribe hoy los valores exactos** (H-PB-37). La corrección alinea RACKEDITAR con él.

**Descartadas:** R-03 (ii), cualquier edición manda; y no corregir.

### OD-PBTC-2. El efecto de corregir (b), el estilo de cotas — DECIDIDO

**Decisión:** «Corregir (Recomendado)» ([OPD], pregunta 2). **Alcance:** OD-PBTC-2 (i).

**Lo que se le presentó y queda como registro:**
- **Hoy,** RACKEDITAR olvida el estilo guardado en el sobre: dibuja con el estilo actual del dibujo y fuerza el tamaño del texto, las flechas, los huecos, el texto encima de la línea y dos decimales (H-13). Además, borra el estilo del sobre al Actualizar.
- **Con la corrección,** si el sobre trae un estilo y el dibujo tiene uno con ese nombre, las cotas salen con él tal cual. Si no, el aspecto sigue como hoy. RACKEDITAR deja de borrar el estilo del sobre. «Insertar» y «Guardar en biblioteca» lo llevan también (D-PBTC-02).
- **No cambian** los puntos medidos, la posición de la línea de cota, la capa, el número de cotas ni el BOM.
- **Medido:** 0 de 22 sobres con estilo, y los 22 con las cotas desactivadas (H-PB-38). Hoy ningún camino de la interfaz de Push Back escribe un estilo (MB H-082). El panel del Workspace ya lo conserva (H-PB-37).

### OD-PBTC-3. D-PB-08: el texto y el lugar del aviso — DECIDIDO

**Decisión:** «Texto corto, franja de estado (Recomendado)» ([OPD], pregunta 3). **Alcance:** el lugar L2, un `TextBlock` en `StatusContent` detrás de `StatusText`, y el literal de D-PBTC-03, byte a byte.

- **Descartadas:** L1 (bajo la casilla, en la barra lateral, contra la regla del shell) y el texto largo (solo exacto en el modo clásico).
- **Los dos modos:** en el modo clásico, «así no se guardará» es exacto (H-16). En el modo WORKSPACE **no está demostrado**, y se comprueba en PB-T2.

### OD-PBTC-4. H-PB-28: «Restaurar estándar» se pierde al Actualizar — DECIDIDO

**Decisión:** «Restaurar estándar (Recomendado)» ([OPD], pregunta 4). **Alcance:** OD-PBTC-4 = (A). El arreglo entra en PB-TC como corrección declarada nueva **D-PB-11** (D-PBTC-11), con CT-PBTC-14, la sustitución S-04 de CT-PB0-05 y las rutas añadidas.

**El defecto medido** (T-4 de PB-T0; mediciones G-1 y G-2):
- **`pb-06`:** tras «Restaurar estándar» y «Confirmar», la vista previa ya muestra reiniciados los seis parámetros avanzados; los controles siguen mostrando 150, false, 3 y 18; al Actualizar, la petición vuelve a escribir 150, false, 3 y 18.
- **`pb-07`:** `DerivedPostHeight` pasa de 140 a nulo y vuelve a 140.

### OD-PBTC-5. Las otras cajas que siguen redondeando (I-03) — DECIDIDO

**Decisión:** el Owner no marcó «Otras cajas que redondean» ([OPD], pregunta 4: «Solo esa opción»). **Alcance:** OD-PBTC-5 = (B). El anexo A de la v2 **no** entra en PB-TC. El caso vuelve al Owner antes del contrato de PB-T1, por OB-06.5 (§10).

### Informativo: lo que queda para PB-T2 y las guardas de T1B

- **El paso 4 de D-PB-06 en el Workspace y el aviso de D-PB-08 en modo WORKSPACE** se prueban en PB-T2:
  - [OD] pide probar los dos modos («prove both: Workspace; RACKEDITAR», `:144-147`);
  - [P] lo asigna a PB-T2 (OB-06.3, `:383`; `:504`; `:511-513`), porque el modo WORKSPACE no existe hasta PB-T2;
  - la sesión lo anota en `f4b-dispositions.md` como obligación de PB-T2 al emitir (precondición 3).
- **Las guardas de T1B:** ninguna deja de estar en verde con este diseño (H-18). No hay vuelta al Owner por OB-T1B.3.

## 9. Límites de esta medición

Están en `pbtc-measurements.md`, apartados J, K y L. En resumen:
- **No se compiló ni se ejecutó nada.** Los RED, los GREEN y las cifras del §6.9 son predicciones a partir del código (R-02).
- **No se midieron:**
  - el delta de la base de CT-PBTC-10, que mide la cadena;
  - las líneas exactas del BOM de R7 y R11;
  - el desplazamiento real de R7 en la planta;
  - cuántos sobres reales tienen más de tres decimales en los campos corregidos;
  - el aspecto en AutoCAD de una cota con estilo nombrado y del aviso.
- **Los recuentos de 67, 9, 13 y 15** son estáticos.
- **No se sabe** si el Controller cuenta un `Skipped` como seleccionado.
- **Que `Assert.Fail` no trunque** el mensaje se toma del comportamiento de xUnit 2.9, no de una corrida.

## 10. Caso pendiente: las otras cajas que redondean (H-31)

- **El caso** (H-31, H-PB-44): fuera de F1-F11, tres grupos de cajas sembradas con «0.###» se releen sin que el usuario cambie su valor y siguen siendo fuente de verdad:
  - el hueco entre lados (`GapBox`), con solo pasar por el campo;
  - la longitud del módulo elegido (`ModuleLengthBox`), que se escenifica y se confirma al Actualizar;
  - cuatro parámetros avanzados anulables, al tocar cualquier caja o la casilla de ese panel.
- **La decisión:** el Owner no lo incluyó en PB-TC ([OPD], pregunta 4, OD-PBTC-5 = (B)). Por el texto de su pregunta, el caso **vuelve al Owner antes del contrato de PB-T1**, la tarea que separa el estado, por OB-06.5.
- **En PB-TC:** queda fuera del alcance por la letra (D-PBTC-07, INV-PBTC-15, PBTC-C10), registrado como «pendiente: decisión del Owner antes de PB-T1» (§5.4) y anotado en `f4b-dispositions.md` al emitir (precondición 3).
- **Para esa vuelta:** el diseño y los CT especificados en el anexo A de la v2, y la medición de las pruebas que tocan esas cajas, que ICT2 encontró incompleta (N-07), se retoman entonces. Nada de eso es autoridad de PB-TC.
