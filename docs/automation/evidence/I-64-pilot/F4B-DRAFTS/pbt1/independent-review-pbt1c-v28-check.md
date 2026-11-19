# I-64 F4B PB-T1c — comprobación acotada independiente de la v2.8 del paquete de PB-T1

**Veredicto: EMITIBLE TRAS CORRECCIONES. B/I/M = 0/1/7. Decisiones nuevas para el Owner: ninguna.**

- **El delta está completo.** Las 37 sustituciones de `v28-delta.md`, aplicadas sobre la v2.7 custodiada, dan la v2.8 byte a byte. No hay ningún cambio sin declarar.
- **Los trece cambios son correctos** contra el código de `2f5d3dec`, contra el paquete y contra F4B-E19, F4B-E21 y F4B-E22. Ninguno va más allá de lo decidido.
- **Un importante (I01).** La parada nueva PBT1C-C13 (V28-07) para ante «una declaración nueva distinta de `SyncName` y `EditableStateForTest`». Pero D-PBT1-13 (1) exige añadir el campo `editable`. Leída al pie de la letra, la parada salta en el primer paso del diseño, y el generador la copia literal al contrato.
- **Los rangos de INV-PBT1C-03 son exactos** en `2f5d3dec`, documentación incluida, y no impiden nada de D-PBT1-13. Dejan sitio, eso sí, a añadidos dentro de los rangos que se retiran (M01). Además, «con su documentación» no se comprueba y le falta `SelectedDimensionViews` (M02).
- **CT-PBT1C-04.** Las quince autopruebas son exactamente las catorce de D-PBT1-15 más la (k). La (k) discrimina la regla 1b sobre los alias, y la guarda queda igual de estricta que la CT-PBT1B-14 corregida.
- **El RED de PB-T1c** se puede cumplir con `ci.yml` y declara todo lo que falla en él, también el filtro UI `PushBack`.
- **El generador** reproduce el contrato en seco de la v2.8 (sha256 `8C9154C6…`) y coincide con el §6.3. Hereda I01.

## §1. Método y límites

**El objeto:**
- `hs65/pbt1/I-64-f4b-pbt1-gate-contract-package-v28-draft.md`: sha256 `d901f12a…98313f5`, 1539 líneas;
- `v28-delta.md` y `v28.diff`;
- la base: la v2.7 custodiada, `git show 2f5d3dec:docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` (sha256 `c35e88b7…30987aa`, igual que `_custodied-v27-2f5d3dec.md`).

**Lecturas**, solo por objetos git desde el checkout principal (`show`, `ls-tree`, `diff`, `grep`, `log` y `cat-file`):
- W y WC;
- los cuatro archivos nuevos de Application, `PushBackEditorState.Load.cs` (EL) y RMES;
- `.github/workflows/ci.yml`;
- `docs/AUTOMATION_PLAN.md` §16.3 y §16.8;
- `f4b-dispositions.md`, de F4B-E19 a F4B-E22;
- el contrato de PB-T1b;
- [PKC2] INV-PBTC2-03;
- la guarda de CT-PBT1B-14 (`tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs`);
- la delegación del intento 2 de F4-T4C2.

Saqué copias de lectura solo en `hs65/pbt1/rv-v28/src/`, para leer con números de línea. Las entradas del scratchpad las leí enteras: el informe de anclas, `simulate-out.txt` y `sim/`, el script de rangos, el generador con sus notas y los dos contratos en seco, las decisiones del Owner, la revisión del intento 1 y su análisis.

**Mis scripts** (Python `-I`, escritos con la herramienta de archivos y de solo lectura sobre el repositorio), en `hs65/pbt1/rv-v28/`:
- `replay_delta.py`: reaplica el delta;
- `contract_auth.py` y `dump_contract.py`: leen los contratos;
- `count_git2.py`: los recuentos. Descarté un primer intento, `count_git.py`, por impreciso;
- `inv03_probe.py`: aplica los rangos a la simulación y a tres mutantes, que escribe solo en `rv-v28/probe/`.

**El generador**, solo en seco, sobre una copia fija (`rv-v28/gen/make_pbt1c_contract.snapshot.py`, sha256 `6954c5d0…`). El original no cambió después de las 19:27. La orden fue `--dry --objects-rev 2f5d3dec --package-file <la v2.8>`, con rc 0. No ejecuté `check_inv_pbt1c_03_ranges.py` ni la simulación: los leí y medí con mis scripts.

**Una incidencia de procedimiento, que declaro.** Al inspeccionar un JSON lancé por error `python -I -c "pass"` y `python -I -` con un heredoc vacío. No ejecutaron ningún código ni tocaron nada, pero van contra la letra de la regla («nunca con `-c` ni con un heredoc»). Todo lo demás se hizo con scripts escritos con la herramienta de archivos.

**Lo que no hice:** no leí, listé ni nombré el worktree en cesión, no escribí en el repositorio y no compilé ni ejecuté pruebas.

**Límites:**
- No hay GitHub: las cifras de las CI `44400123417` y `44405531606` son las de la evidencia 248 y de la revisión del intento 1.
- La delegación de corrección de PB-T1b no está en `2f5d3dec`. El hecho 2 de V28-04, que su `ChainRedSha` es `2d9febc6`, es de la sesión.
- La CT-PBT1B-14 corregida aún no está en git. Comparo con analysis.md (I01) y con la guarda del intento 1 (GRD:152-167).
- La discriminación de las autopruebas está medida sobre el modelo en Python de la guarda (la simulación), no sobre una guarda C#.
- El generador se editaba mientras revisaba (19:06, 19:22 y 19:27). Evalué el de las 19:27.

## §2. Los cambios uno a uno

| Id | Veredicto | Lo comprobado |
|---|---|---|
| V28-00 (versión, estado y registro) | **Correcto**, salvo una frase (M07) | `replay_delta.py`: las 37 sustituciones reproducen la v2.8 (`True`, sha `d901f12a…`). La base es la custodiada. El marcador aparece 12 veces: 9 huecos y 3 que solo lo nombran, como dice el delta. La viñeta del intento 1 casa con la evidencia 248 y con la revisión (REWORK 0/2/15; `fb01`). |
| V28-01 (`ExpectedEvidence` de PB-T1c y nota en §6.2) | **Correcto**; falta la mitad «Trailer» de M14 (M03) | **`ci.yml`:** `build-plugin` tiene `needs: [tests, build-ui]` y ningún `if:`, así que sale omitido si falla `tests`. **El contrato de PB-T1b**, `ExpectedEvidence[2]`, dice literal «build-ui y build-plugin en success», y la nota lo cita bien. En PB-T1c, CT-PBT1C-03 va con `ExpectRed` true. **F4B-E21:** `output.json` con las ocho colecciones vacías y `delegation.json`. **El alcance:** F4B-E22, pregunta 2, dice «el paquete de PB-T1c corrige el texto»; no va más allá. |
| V28-02 (nota de M10) | **Correcto** | **Las 14 anclas**, en `2f5d3dec` (igual a `a0a5f699` en `src/`): PESC:157, :164, :167, :188, :202, :211, :242, :245, :250 y :403; PESO:68, :115, :121 y :133. **Los otros privados del comparador** (`CopyEditor`, `EditorsEqual`, `SidesEqual`, `PalletsEqual`, `StructuralEquality`…) tienen su homólogo en el piloto: `PushBackPilotStateComparer.cs:64`, `:134`, `:172`, `:280` y `:309`. **Las cinco constantes privadas de PESO:60-64**, que la nota no nombra, quedan cubiertas por «tal como está en el GREEN» y por el texto del Owner («los ayudantes privados del comparador y del mapa»). |
| V28-03 (A-7 a A-10 y el reparto) | **Correcto**; el reparto de M14 está incompleto (M03) | **Las anclas:** PESO:109-112; PES:111-113 y :121-123; EL:59 (`ArgumentNullException`); W:296, :313 y :3309-3313; PESC:228; RMES:102, :136 y :996; PESC:211-240. **Los destinos** caben en F4B-E22, pregunta 1, y en analysis.md §3, sin inventar ninguno. El reparto suma 17. |
| V28-04 (F4B-E21, la corrección de PB-T1b y dos hechos) | **Correcto** | **La nota de F4B-E21** coincide con la decisión y con F4B-E21 (`4000`, `a60d`, `6063`, `3412`, `inject_opaque.py`). **El formato de la corrección** es el de analysis.md §4. **Hecho 1:** `git diff --name-only 5e52aaa3 2f5d3dec` contiene ES y RMES, que son las dos `UNIT_CHANGE` del contrato de PB-T1b, y ninguna `EXTERNAL`; la cita de §16.3 es literal. **Hecho 2:** la cita de §16.8 es literal. El precedente es el de `F4-T4C2-PANEL/2/R20261115T195135Z-d34e/delegation.json` (`ChainRedSha` null y `ChainRedFiles` vacío), con `f4-dispositions.md:2522`. **En PB-T1c,** W y WC van como `UNIT_CHANGE` en el contrato en seco. **Sin regla nueva.** **Las rutas** del §1 y del §8.2 existen todas en `2f5d3dec`. |
| V28-05 (la Snapshot J) | **Correcto** | **F4B-E19:** la sonda `d32c` dio Pass y la J sustituye a la I. `snapshot-j.json` está en `2f5d3dec`. P-01 del contrato de PB-T1b dice «today Snapshot I», como recoge el paquete. |
| V28-06 (los huecos del cierre) | **Correcto** | **Mis recuentos en `2f5d3dec`:** `PushBackEditableState` 13, `PushBackEditorOutputGate` 1, `PushBackSharedReferenceGuardTests` 2 y `PushBackEditableStateParityTests` 1. **La aritmética** de la previsión: 19 086 + 2 y 2 437 + 2, con 2 416 + 2 `Passed`. |
| V28-07 (PBT1C-C13; PD-V28-01, opción (A)) | **Estrecha la regla y no la relaja, pero contradice D-PBT1-13 (1): I01** | Nada que antes estuviera prohibido pasa a estar permitido, y una declaración nueva pasa de ser un fallo de invariante a ser una parada previa. Pero el campo `editable`, que exige D-PBT1-13 (1), es una declaración nueva distinta de `SyncName` y `EditableStateForTest` (§3, §8). |
| V28-08 (INV-PBT1C-03 con rangos; D-PBT1-13 (6); precondición 4) | **Los rangos, exactos; dos huecos (M01, M02)** | Ver §3. Las líneas de documentación de (6), W:326-341, WC:812 y WC:816-826, son exactas. |
| V28-09 (las autopruebas (c) y (k)) | **Correcto**; redacción (M05) | Ver §4. |
| V28-10 (siembras y variables locales) | **Correcto** | **Las locales:** W:392 (`var pallet = …`) y W:413 (`var options = …`). **Las 14 siembras:** W:390, :394-397, :399-400, :402, :415-418 y :420-421. Con las dos locales son 16, las mismas que el informe de anclas. |
| V28-11 (escritores fuera de `LoadFromModel`; el comentario de W:405) | **Correcto en su letra** | **Las anclas:** W:405 es literal; W:1270-1271 y W:1303. `AdoptCarriers` deja `LoadedDimensionViews = options.DimensionViews` y `DimensionViewsTouched = false` (PES:156-157). Son las únicas **asignaciones** a un portador. **Observación, sin hallazgo:** hay además escrituras **a través** de un portador: `advanced` en W:700 y W:1639-1646, y `safetySelections` en W:2669-2672, :2803 y :2994. Siguen siendo equivalentes, porque el reenvío devuelve el mismo objeto. |
| V28-12 (el RED del filtro UI `PushBack`) | **Correcto** | `FullyQualifiedName~RackCad.UI.Tests.PushBack` contiene `PushBackWindowOnStateTests`. Ningún filtro de Core contiene `PushBackWindowOnStateSourceGuardTests`. |
| V28-13 (las cifras) | **Correcto** | **Mis recuentos:** UI `PushBack` 557 (69 clases, 3 `Skip`) y `PushBackRichPreviewTests` 9 en `2f5d3dec`. Hacia atrás: 542 en `b5c09f0b` y 556 en `5043b49a`. |

## §3. INV-PBT1C-03

**¿Los rangos son exactos en `2f5d3dec`, documentación incluida? Sí, los 25.**
- Comprobé el principio y el final de cada uno en W y WC. Cada rango empieza en su `///` o `//` y acaba en el `}` o el `;` del miembro.
- Los bordes en blanco son solo los declarados: W:325 y :374, W:1288 y :1298, y WC:811.
- Los miembros sin documentación no tienen ningún comentario de miembro encima: `safetySelections`, `LoadFromModel`, `ReadInputs`, `Name_Changed`, `InsertBatch_Click`, `RequestDraw`, `SaveLibrary_Click` y `CompositeHasBlocking`. Los separadores `// ---- …` de W:266 y W:1232 no son documentación y quedan fuera, con razón.
- No hay solapes.
- El generador dice lo mismo: «25 rangos comprobados […] 98 autopruebas en negativo».

**¿Impiden algo que D-PBT1-13 exige? No.**
- Cada edición de D-PBT1-13 (1) a (9) cae en un rango.
- `inv03_probe.py`, sobre las dos variantes de la simulación, da 0 hunks fuera: W, 37 hunks; WC, 4 y 5.
- Las dos inserciones (`@@ -193,0`, la costura, y `@@ -1239,0`, `SyncName`) caen entre dos miembros.
- No hace falta ningún `using` nuevo: W y WC ya tienen `using RackCad.Application.Systems.PushBack;`.

**¿Dejan sitio a un cambio no previsto? Sí (M01).**
- **Dónde:** la definición es por posición. Todo hunk que caiga dentro de un rango pasa, añada lo que añada.
- **Los tres mutantes** de `inv03_probe.py`:
  - un ayudante privado nuevo en el hueco que deja `SideAOnly` (W:325-374): **pasa**;
  - otro dentro del rango de `composite` (W:49-57): **pasa**;
  - el mismo entre `LoadFromModel` y `LoadSelectedFront`: falla, como debe.
- **Por qué crece el área.** Al meter la documentación y los blancos en los rangos de lo que se retira, V28-08 amplía el área admitida frente a la heurística de la v2.7: unas 185 líneas en W:325-374, W:1288-1298 y WC:811-934. Eso es necesario para poder retirar la documentación, pero esos rangos solo tendrían que admitir líneas quitadas.
- **Más grueso que [PKC2],** aunque diga «como [PKC2]». [PKC2] daba **líneas** dentro del miembro («W:1905 (`ModuleLength_Changed`, W:1892-1913)»; `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md:483-492`). La v2.8 da miembros enteros, también en `Name_Changed`, `InsertBatch_Click`, `RequestDraw` y `SaveLibrary_Click`, donde D-PBT1-13 solo inserta `SyncName();`.

**¿Encajan con V28-07? No del todo: «declaración nueva» tiene dos sentidos.**
- **INV-PBT1C-03 la define por posición:** «una inserción entre dos miembros». Así admite `editable` (dentro del rango de `composite`) y los ayudantes metidos dentro de un rango.
- **PBT1C-C13 la usa por clase de declaración:** «de cualquier visibilidad: método, propiedad, campo…».
- **El resultado:**
  - C13 cubre, para el Worker, el hueco de los ayudantes dentro de un rango (bien);
  - pero para ante `editable` (I01);
  - y la verificación, que comprueba INV-PBT1C-03 por script, no ve ese hueco.

**¿Estrecha o relaja?**
- **La parada (A) estrecha.**
- **El área admitida por posición se amplía un poco con V28-08,** y C13 solo la compensa en parte: no cubre los comentarios nuevos dentro de lo retirado. Se corrige estrechando (M01). No hay ninguna decisión material.

## §4. CT-PBT1C-04

**¿Son exactamente las de D-PBT1-15 más la (k)? Sí.**
- **D-PBT1-15** tiene trece comunes, (a), (b), (c), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i) y (j), y la (d), solo de CT-PBT1C-04. Son «las catorce».
- **CT-PBT1C-04** nombra (a) a (j) con la (d), y la (k): 15.
- **CT-PBT1B-14 del intento 1** tiene las trece comunes (GRD:218-230 y :242-244).
- **El contrato en seco** lleva las quince en INV-PBT1C-02 y en `ExpectedEvidence[1]`.

**¿La (k) discrimina la regla 1b sobre los alias? Sí.** La (k) es `var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add("x");`. La regla 2 hace de `r` un alias por asignación.
- **Sin la 1b sobre los alias, no salta.**
  - La forma `as` de la 1b sobre la ficha original no cruza el `;` (`[^;{}]*?`).
  - La regla 1 sobre `r` no casa, porque tras `r.Preserved` viene ` as`.
  - Por eso no salta con la guarda del intento 1 (GRD:163, que solo aplica `Rule1` a los alias).
- **Con la 1b sobre los alias, salta** por la forma `as` sobre `r`.
- **Medido** en `simulate-out.txt`: «k-alias+molde (I01) {'2(1b)': 1} -> SALTA; sin aplicar 1b a los alias: NO SALTA».

**¿La G-L1 sobre W y WC queda igual de estricta que la corregida de PB-T1b? Sí.**
- Tiene las mismas fichas y las mismas reglas 1, 1b, 1c, 2 y 3.
- La regla 2 vale sobre la 1 y sobre la 1b, que es la corrección I01 (`Rule1b(member, aliases.Keys)`).
- Ninguna de las dos aplica la 1c a los alias, como dice la regla 2.
- CT-PBT1C-04 lleva además la (d).
- El paquete escribe la regla («un alias cuenta como ficha para las dos»), así que no depende de cómo quede la CT-PBT1B-14 corregida.
- **Opcional, sin hallazgo:** la (k2) del informe de anclas (molde de estilo C sobre un alias) también discrimina y blindaría las otras dos formas de la 1b.

**La redacción (M05):**
- «La (k), medida sobre `2f5d3dec`»: no es así. Las autopruebas de la simulación se midieron «cada una sola en un miembro neutro» sintético. Sobre `2f5d3dec` solo se midió el 0 de coincidencias.
- «la añade a CT-PBT1B-14»: analysis.md la da «por ejemplo».
- La viñeta «y (k)», puesta tras «en CT-PBT1C-04, (d)», se lee como si la (k) fuera solo de PB-T1c.

## §5. La evidencia del RED (V28-01 y V28-12)

**¿Se puede cumplir con `ci.yml`? Sí.**
- **Compila:** el RED de W es una línea (`internal PushBackEditableState EditableStateForTest => new PushBackEditableState();`). El tipo es público y tiene un constructor público (EST:40), y el `using` ya está, así que `build-ui` sale en `success`.
- **Falla Core:** CT-PBT1C-03 falla en la base («la base declara los campos y no delega»; en la simulación de la base fallan todas sus cláusulas), así que `tests` sale en `failure`.
- **Falla UI:** `ui-tests` sale en `failure` por CT-PBT1C-01 y 02.
- **Se omite el Plugin:** `build-plugin` sale omitido, porque tiene `needs: [tests, build-ui]` y ningún `if:`.
- **Pasa la guarda:** CT-PBT1C-04 da 0 coincidencias en la base (320 miembros), así que pasa.
- **Sin cuelgue:** las redes de `ci.yml` son blame-hang de 5 minutos y los timeouts de 20 y 25.

**¿Declara todo lo que fallará en el RED? Sí.**
- En Core, solo CT-03.
- En UI, solo CT-01 y 02, y por eso el filtro UI `PushBack` falla solo por esos dos métodos (V28-12).
- Ningún otro filtro de `RequiredTests` los contiene. `WindowCensusGuardTests` es un censo de ventanas, no de miembros, y la costura no le afecta.

**Lo que no declara:**
- Que el `Trailer` de la entrega sea el real de los commits: es la otra mitad de M14 (M03).
- La tabla «CT → la aserción que falla en el RED», que PB-T1b sí tenía. INV-PBT1C-02 ya fija el criterio mecánico, así que es una observación, no un hallazgo.

## §6. Lo que falta

| Fuente | Hallazgo | En la v2.8 |
|---|---|---|
| Informe de anclas | I-1, I-2 e I-3 | V28-01, V28-08 y V28-09. I-2 (b) queda a medias para `SelectedDimensionViews` (M02) |
| Informe de anclas | M-1 a M-4 | V28-10 a V28-13 |
| Revisión del intento 1 de PB-T1b | los 17 ids | Repartidos en el §10. Pero **M14 tiene dos partes:** `build-plugin` omitido (lo ratificó el Owner) y `"Trailer": null` aunque los commits llevan el trailer. El Owner solo ratificó la primera («se ratifica que en la CI del RED `build-plugin` salga omitido»). La v2.8 da M14 entero por ratificado y no lleva la segunda a PB-T1c (M03) |
| Generador | C-1 a C-6 | V28-01, V28-05, V28-12, V28-09, V28-08 y V28-10 |
| Generador | C-7 y D-7 (los nombres de custodia de PB-T1c) | **No.** §6.0 «Custodia» sigue siendo genérica para PB-T1c (M04) |
| Generador | C-8 y D-1 | V28-07, con I01 |
| Generador | D-4, D-5, D-6, D-9 y D-10 | V28-04a, V28-06, V28-13, V28-12 y V28-09 |
| Observación 1 del redactor (las constantes de PESO) | — | Cubierta por la nota de V28-02; no es un hallazgo |

## §7. El generador frente a la v2.8

**Reproducible:**
- mi corrida en seco sobre la copia fija da rc 0;
- 56 573 bytes, sha256 `8C9154C61BC57BF9D2CE406D35EB9B27B165865C0B04D425E0D2AEA9A854AA42`, igual que `contract-dry-2f5d3dec-v28.json` y `dry-objects-2f5d3dec-v28.txt`;
- `allowed` 5, `forbidden` 161, `inv` 15, `stops` 32, `required` 19, `evidence` 7 y `authorities` 64;
- 9 AVISOS, todos posteriores al cierre o a la emisión.

**Coincide con el §6.3:**
- el `Objective` (v2.8) y el `AllowedWriteScope` (5);
- `RequiredTests`: los 19 filtros de la tabla, con 557 y 9;
- INV-PBT1C-02: las quince autopruebas y V28-12;
- INV-PBT1C-03 y PBT1C-C13, literales de la v2.8;
- INV-PBT1C-05: V28-10;
- la `ExpectedEvidence`: V28-01 y F4B-E21. Añade «la prueba de la limpieza del perfil» y «AllowedWriteScope de la delegación = el del contrato», que salen de los invariantes comunes y del patrón de PB-T1b;
- P-01, con la J;
- W y WC como `UNIT_CHANGE`.

**Lo que hereda o añade:**
- **I01:** `C13_V28` copia la parada tal cual (`StopConditions[29]`).
- **M06:**
  - INV-PBT1C-03 llega al contrato con texto que no es normativo: quién midió, «se vuelven a medir en la emisión», `check_inv_pbt1c_03_ranges.py`, `anchors-report.md` y la nota «no es una decisión material del Owner»;
  - C13 llega en castellano, entre paradas en inglés.

**Desfasados:**
- **`contract-dry-2f5d3dec.json`** (19:07, sha256 `3BB78501…`) es anterior a la v2.8: C13 de la v2.7, INV-PBT1C-03 sin rangos, INV-PBT1C-05 con las «siembras» antiguas y sin F4B-E21.
- **`generator-notes.md`** dice `D3_C13_NEW_MEMBER = False`, 60 autoridades y 10 AVISOS.

Ninguno de los dos debe custodiarse como el seco de la v2.8 (M04).

## §8. Los hallazgos

| Id | Grav. | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **PBT1CV28-I01** | **I** | **PBT1C-C13 (V28-07) y su nota bajo INV-PBT1C-03 paran ante el campo `editable`, que D-PBT1-13 (1) exige.** La parada salta si «hacer la tarea exigiría añadir en W o WC una declaración nueva (de cualquier visibilidad: método, propiedad, **campo**…) distinta de `SyncName` y `EditableStateForTest`». D-PBT1-13 (1) añade `private readonly PushBackEditableState editable = new PushBackEditableState();`, un campo con un nombre nuevo. Leída al pie de la letra, la parada salta en el primer paso del diseño. Además, los doce reenvíos (`composite` y los once campos de (2)) se redeclaran como propiedades con el mismo nombre, y una lectura estricta también los atrapa. El término «declaración nueva» tiene en el mismo contrato un sentido por posición (INV-PBT1C-03) y otro por clase (C13). La simulación que V28-07 cita como prueba añade justo `editable`. | **El paquete:** v2.8:609 (D-PBT1-13 (1)), :1266 (la nota) y :1274 (C13). **La simulación:** `pbt1c/sim/W-A.cs.txt`, hunk `@@ -56,2 +56,2`. **`inv03_probe.py`**, declaraciones añadidas en las dos variantes: `editable`, los doce nombres de siempre, `EditableStateForTest` y `SyncName`. **El informe de anclas**, (c): «No existen todavía `SyncName`, `EditableStateForTest` ni `editable`». **El contrato en seco:** `StopConditions[29]` | **En C13 y en la nota:** «… una declaración con un nombre que no existe en `BaseSha` (de cualquier visibilidad) distinta de las tres que prescribe D-PBT1-13: `editable` (1), `SyncName` (8) y `EditableStateForTest` (9). Los reenvíos de (1) y (2), con el mismo nombre y en el rango de su campo, no son declaraciones nuevas». **En INV-PBT1C-03,** decir que `editable` entra dentro del rango de `composite`. **En el generador,** actualizar `C13_V28` e `INV03_V28`. No es material: alinea la parada con un diseño ya decidido |
| PBT1CV28-M01 | M | **INV-PBT1C-03 deja sitio a añadidos dentro de los rangos.** Un ayudante nuevo, o un comentario, dentro de lo que se retira (W:325-374, W:1288-1298 y WC:811-934) o del rango de `composite` pasa la comprobación por posición. Con la documentación dentro del rango, el área admitida creció. El texto dice «como [PKC2]», pero [PKC2] daba líneas, no miembros enteros. C13 solo lo cubre para el Worker, no para el script de la verificación | `inv03_probe.py`: los mutantes 1 y 3 dan 0 fuera, y el 2 da 1. [PKC2] INV-PBTC2-03 en `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md:483-492` | **En INV-PBT1C-03:** en los tres rangos que se retiran, los hunks solo quitan líneas. En `Name_Changed`, `InsertBatch_Click`, `RequestDraw` y `SaveLibrary_Click`, el único hunk es la inserción de `SyncName();` justo después de W:1237, :3352, :3405 y :3504. La simulación cumple las dos cosas. **Opcional:** un censo de declaraciones en INV-PBT1C-05 (los miembros de W y WC en `CurrentSha` = los de `BaseSha`, menos los cuatro retirados, más `editable`, `SyncName` y `EditableStateForTest`). Estrecha; no es material |
| PBT1CV28-M02 | M | **«Con su documentación» (V28-08b) se queda a medias y no se comprueba.** D-PBT1-13 (5) retira `SelectedDimensionViews` sin su documentación (W:1289-1290), aunque I-2 la listaba. INV-PBT1C-03 admite las dos variantes, con la documentación y sin ella (lo dice el propio texto medido), y CT-PBT1C-03 quita los comentarios antes de mirar. Un comentario huérfano pasaría | v2.8:625 y :626; :1265, «las dos variantes […] dan 0 hunks fuera»; el informe de anclas, I-2 (b) | **En D-PBT1-13 (5),** añadir «con su documentación (W:1289-1290)». **En INV-PBT1C-05:** ninguna de estas líneas de `BaseSha` queda en `CurrentSha` (medir en la emisión que son únicas): W:327 («El diseño del lado A SOLO, sin la parte compuesta.»), W:1289 («La política que se guarda: la cargada, exacta,»), WC:812 («Recupera la intencion compuesta de un diseno cargado») y WC:817 («CARGAR OTRO RACK Y RESTAURAR EL ACTUAL NO SON LO MISMO») |
| PBT1CV28-M03 | M | **La mitad «Trailer» de PBT1B-IR-M14 se pierde.** La revisión pone en M14 también que `worker-handoff.json` lleva `"Trailer": null` aunque los commits RED y GREEN llevan el trailer `Co-Authored-By`. El Owner solo ratificó «`build-plugin` omitido». El §10 da M14 por ratificado entero, y la `ExpectedEvidence` de PB-T1c no pide que el `Trailer` coincida con los commits | La revisión del intento 1, §3, M14; `owner-pbt1b-rework-decisions.md`, pregunta 2; v2.8:1491 y :1277 | **En el §10,** partir M14 en (a), ratificado, y (b), el `Trailer`, pendiente de la verificación del intento 2. **En la `ExpectedEvidence` de §6.3,** añadir: «el `Trailer` de `worker-handoff.json` es el trailer real de los commits RED y GREEN» |
| PBT1CV28-M04 | M | **Los nombres de custodia de PB-T1c no están fijados** (C-7 y D-7 del generador). §6.0 «Custodia» es genérica para PB-T1c; la precondición 4 cita `anchors-report.md`, un nombre del scratchpad; la línea 44 enumera el registro solo hasta P. El generador supone `pbt1c-anchors-out.txt`, el apartado `Q`, `make_pbt1c_contract-dry.txt` e `independent-review-pbt1c-check.md`. Pero esta comprobación se encargó como `independent-review-pbt1c-v28-check.md`; para PB-T1b, el §6.0 sí nombró `independent-review-pbt1b-v27-check.md`. Los dos secos desfasados (§7) no deben custodiarse | v2.8:44, :50, :1039 y :1045-1049; `generator-notes.md` §4 y §5; `MEAS_PBT1C = 'Q'` | Escribir en §6.0 la lista `CUSTODY` y `CUSTODY_EXTRA` de PB-T1c con sus nombres reales, como PBT1BC27-M01 hizo para PB-T1b, y alinear el generador y la línea 44. Custodiar el seco de la v2.8 y poner al día `generator-notes.md` |
| PBT1CV28-M05 | M | **La redacción de la autoprueba (k)** en D-PBT1-15: (i) «medida sobre `2f5d3dec`», cuando se midió sobre un miembro neutro sintético con el modelo en Python de la guarda; (ii) «la añade a CT-PBT1B-14», cuando analysis.md da esa autoprueba «por ejemplo»; (iii) la viñeta «y (k)», tras «en CT-PBT1C-04, (d)», parece de PB-T1c solo | v2.8:750-753; `simulate-out.txt`, «cada una sola en un miembro neutro»; analysis.md §2, I01 | «Medida sobre un miembro neutro, como O-2, con el modelo de la guarda de la simulación». «Añade una autoprueba “alias + molde” (analysis.md da esta como ejemplo)». Poner la (k) entre las comunes, antes de la (d) |
| PBT1CV28-M06 | M | **El contrato lleva texto que no es normativo.** El generador copia literal (`md_fold`) todo el INV-PBT1C-03 del §6.3: quién midió y dónde, «se vuelven a medir en la emisión», el «Medido» con `check_inv_pbt1c_03_ranges.py` y `anchors-report.md` (ninguno es autoridad ni se custodia) y la nota «no es una decisión material del Owner». C13 entra en castellano entre paradas en inglés | El contrato en seco: `Invariants[2]` y `StopConditions[29]`; el generador, `INV03_V28` y `C13_V28` | En el §6.3, separar lo normativo de INV-PBT1C-03 (los rangos, la definición y los comentarios) de la medición y de la nota, por ejemplo con «Nota (no va al contrato)», y que el generador copie solo lo normativo. Unificar el idioma de C13 |
| PBT1CV28-M07 | M | **Una frase inexacta:** «sin más regla nueva que la parada de V28-07» (§6.0, «Qué comprueba»), y «No queda ninguna decisión pendiente». V28-08 añade también reglas: un comentario nuevo solo dentro del rango de `composite`, y retirar con la documentación. Estrechan y no son materiales, pero la frase es justo lo que esta comprobación tiene que verificar | v2.8:65, :1038 y :1265 | «… sin más reglas nuevas que las que estrechan: la parada de V28-07 y las de V28-08» |

## §9. El veredicto

**EMITIBLE TRAS CORRECCIONES. B/I/M = 0/1/7.**

**Lo que hay que corregir antes de emitir:**
- **I01:** sin esa corrección, el contrato tiene una parada que contradice su propio diseño.
- **Los siete menores:** se recomiendan, y M01 a M03 deberían entrar con I01. Todos son de texto y estrechan, o arreglan nombres y redacción.

Nada de esto cambia el alcance, el diseño de D-PBT1-13, los CT, los recuentos, D2 ni RACKEDITAR. Lo que dejan los anclajes y la revisión de PB-T1b se aplica como se declara, salvo M02 y M03.

**Decisiones nuevas para el Owner: ninguna.**
- I01 alinea la parada con D-PBT1-13, que ya está decidido.
- M01 y M02 solo estrechan.
- M03 a M07 son de evidencia, custodia o redacción.
- Hay una pendiente condicionada que no es nueva: la lectura del Controller de §16.3 y §16.8 en la verificación del intento 2 de PB-T1b (V28-04, [POR RELLENAR]). Si fuera desfavorable, volvería al Owner por el cauce de PB-T1b antes de emitir PB-T1c.

## §10. Recomprobación de V28-14 a V28-28

**Veredicto de la recomprobación: EMITIBLE TRAS CORRECCIONES. B/I/M = 0/1/3. Decisiones nuevas para el Owner: ninguna.**
- **Mis ocho hallazgos están resueltos.** De I01 y M06 queda un resto menor, el ámbito de C13 por nombre (R-M01).
- **V28-20, V28-26 y V28-27 son fieles** al código de `a18ba345`.
- **V28-22 y V28-23 son fieles** a la decisión del Owner (F4B-E25).
- **Un importante, una regresión:** dos de las cuatro líneas testigo de V28-16 llevan tildes, y en el contrato ASCII la comprobación queda vacía (R-I01). Nace de mi propia corrección de M02.

### 10.1 Método y límites

Las reglas son las de §1, sin incidencias esta vez.

**Lo que hice:**
- **El delta:** `rv-v28/replay_delta2.py` reaplica las 73 sustituciones del delta nuevo sobre la v2.7 custodiada, en varias pasadas, porque V28-18b, V28-21 y V28-28a parten del texto de cambios que van más abajo en el archivo. El resultado es la v2.8 byte a byte (sha256 `80b28098…bdfbd6e`, 1589 líneas).
- **El código de `a18ba345`**, por objetos git:
  - `PushBackSharedReferenceGuardTests.cs`, con su diff frente a `a0a5f699`;
  - `RackMenuCommands.cs` y `RackPushBackCommands.cs`;
  - `git diff 126fd65f a18ba345 -- tests`, vacío;
  - `git diff 2f5d3dec a18ba345 -- src`, vacío.
- **La decisión del Owner y su vuelta:** `pbt1b2/owner-f4b-correction-unit-change-decision.md` y `owner-return-verify-stop.md`. También `pbfull/owner-f4b-dpb07-extensiondata-decisions.md`, para F4B-E24.
- **INV-PBT1C-03 y 05:** `rv-v28/inv03_probe2.py` aplica las reglas nuevas, el censo y las líneas testigo a las dos variantes de la simulación y a cuatro mutantes, que escribe solo en `rv-v28/probe/`.
- **El generador:** reproduje en seco el contrato nuevo, sobre una copia fija (`rv-v28/gen/make_pbt1c_contract.snapshot-b.py`, sha256 `ebf4126c…`, el de las 20:19). Con `--dry --objects-rev 2f5d3dec --package-file <la v2.8>` da rc 0 y 57 996 bytes, sha256 `DC33876E…94F5D99F`, igual que `pbt1c/contract-dry-2f5d3dec-v28b.json`.

**Límites:**
- Sin GitHub, la CI de `a18ba345` y la verificación `c78a` son las de la vuelta al Owner.
- F4B-E23 a F4B-E26 y los archivos de F4B-E24 y F4B-E25 no están en git (`f4b-dispositions.md` llega hasta F4B-E22, en `2f5d3dec` y en `a18ba345`): los leí en el scratchpad.

### 10.2 Mis hallazgos, uno a uno

| Hallazgo | ¿Resuelto? | Lo comprobado |
|---|---|---|
| **I01** (C13 paraba ante `editable`) | **Sí** (V28-14) | **C13 se define ahora por nombre** y exceptúa `editable` (1), `SyncName` (8) y `EditableStateForTest` (9). Los reenvíos, con el mismo nombre, no son declaraciones nuevas. **INV-PBT1C-03** pone `editable` dentro del rango de `composite`. El contrato en seco (`StopConditions[29]`) lleva el texto en inglés. **Queda un resto menor de ámbito: R-M01** |
| **M01** (añadidos dentro de los rangos) | **Sí** (V28-15 y V28-19) | **Por posición:** lo que se retira solo quita líneas, y en los cuatro métodos solo entra `SyncName();`. **Por nombre:** el censo de INV-PBT1C-05. **`inv03_probe2.py`:** las dos variantes de la simulación pasan. El ayudante metido en lo retirado de `SideAOnly` lo ven a la vez INV-PBT1C-03 y el censo. El ayudante dentro del rango de `composite` y una sobrecarga `Keep` solo los ve el censo. Un comentario nuevo en `RequestDraw` lo ve INV-PBT1C-03 |
| **M02** («con su documentación») | **Sí** (V28-16), **con una regresión: R-I01** | D-PBT1-13 (5) retira `SelectedDimensionViews` con W:1289-1290. INV-PBT1C-05 lleva cuatro líneas testigo. Las cuatro son únicas en W y WC (`git grep` en `2f5d3dec`; la de W:1289 está también en `RackSelectiveWindow.xaml.cs:2165`, fuera del alcance). La variante que deja la documentación incumple las cuatro |
| **M03** (la mitad «Trailer» de M14) | **Sí** (V28-17) | M14 se parte en (a), ratificada, y (b), pendiente, en el §10 y en la nota de §6.2. La `ExpectedEvidence` de §6.3 pide el `Trailer` real, y el contrato en seco lo lleva en `ExpectedEvidence[4]` |
| **M04** (nombres de custodia) | **Sí** (V28-18 y V28-28) | **Las listas** `CUSTODY` (4) y `CUSTODY_EXTRA` de PB-T1c están en §6.0, con el apartado Q y los secos desfasados excluidos. **Los diez nombres nuevos de `F4B-DRAFTS/pbt1/`** están libres en `2f5d3dec` y en `a18ba345` (`ls-tree`). **Falta la custodia de F4B-E23 a F4B-E26:** R-M03 |
| **M05** (la redacción de la (k)) | **Sí** (V28-20) | Ver §10.3 |
| **M06** (texto no normativo en el contrato) | **Sí** (V28-14 y V28-19) | C13 va en inglés. INV-PBT1C-03 y 05 y C13 separan una «Nota (no va al contrato)». El generador comprueba «tres notas fuera», y su INV-PBT1C-03 ya no lleva la medición ni la nota |
| **M07** («sin más regla nueva») | **Sí** (V28-21) | §6.0, «Qué comprueba», y el registro: «… sin más reglas nuevas que las que estrechan». **La frase vecina del registro quedó inexacta:** R-M02 |

### 10.3 V28-20, V28-26 y V28-27 frente a `a18ba345`: fieles

**V28-20a (la regla 2).**
- `Violations` (GRD:152-172) aplica `Rule1(member, aliases.Keys)` y `Rule1b(member, aliases.Keys)` sobre el texto de cada miembro, y nunca `Rule1c` a los alias.
- `Rule1b` (GRD:60-66) tiene las tres formas: `CastC`, `as` e `is`.
- La v2.8 dice «en el texto de su miembro», no «en el resto del miembro» como la v2.7. Así es el código: la regla mira también antes del alias. Es más estricto y es exacto.

**V28-20b (las autopruebas).**
- **Literales:** (k1) y (k2) son las de GRD:238-239, y las quince autopruebas y sus líneas `autoprueba-k1=falla` y `autoprueba-k2=falla` son las del `Assert.Equal` (GRD:250-253).
- **«Saltan solo por la regla 2 aplicada a la 1b», comprobado sobre los patrones de la guarda:**
  - en (k2), `CastC` + `p` casa `((List<string>)p`;
  - `Rule1` sobre `p` no casa, porque tras `p` viene `)`;
  - las formas de la 1b sobre la ficha no cruzan el `;`.
- **Lo medido**, ahora descrito como se midió: un miembro neutro y el modelo en Python.
- **La inyección:** es la de `Inject`, un `void Inyectado() { … }` añadido al texto real (GRD:203-207).
- **Un matiz sin hallazgo:** las dos autopruebas entran en el RED `126fd65f`. El GREEN `a18ba345` no toca `tests/`.

**V28-20c.** CT-PBT1C-04 lleva las quince comunes y la (d): dieciséis. Está bien.

**V28-26 (CT-PBT1B-15):**
- **`NameStatement`:** es `NameWriter + "[^;]*;"`, la sentencia entera, y `Collapse` colapsa los espacios.
- **La de `:64`** tiene varias líneas (`RackMenuCommands.cs:64-65`), y su texto en la guarda es el que da la v2.8.
- **`WriterInsideDrawPushBackView`** mira el texto de `MethodText`, que va de la firma a la llave que la cierra.
- **La tercera autoprueba** saca el escritor y lo pone justo antes de `static void DrawPushBackView(`.
- **Las líneas** son las del §5.3. **El orden:** `modalFirst < call < declaration` y el escritor dentro.

**V28-27 (quién no cuenta como alias).**
- `Rebind` tiene los cuatro nombres de la regla 3.
- Por asignación se excluyen las fichas y `Rebind`; como parámetro de lambda, solo las fichas; `foreach` e `is`, sin exclusión.
- `Rebind` ya estaba en `a0a5f699`.

### 10.4 V28-22 y V28-23 frente a la decisión del Owner: fieles

- **La respuesta del Owner, literal:** «Regla F4B y reverificar (Recomendado)».
- **Las tres condiciones, literales:** (i) RED o `CurrentSha` de un Worker de la misma cadena, según su `worker-handoff.json`; (ii) en el `AllowedWriteScope` de esa delegación; (iii) ningún commit de la sesión. Y además: «las `EXTERNAL`, sin excepción», «lo demás de 16.3 sigue igual», «una disposición de la unidad, como F4B-E21».
- **Para PB-T1b:** reverificar `a18ba345` «sin gastar intento ni cambiar la entrega».
- **Para PB-T1c:** §6.0, y §6.3, que la cita. El contrato en seco nuevo lleva la decisión y su vuelta como `UNIT_DOC`, como pide «sus contratos la citan como autoridad».
- **El hecho de `c78a`:** EXECUTION_BLOCKED / STOP por Authority.
- **No va más allá.** V28-23 solo pone el id, y F4B-E25 es el que fija la sesión.
- **No es una decisión nueva.** El arreglo del control de cadena del prompt (solo `src/` y `tests/`) es de la sesión y de PB-T1b, y la v2.8 no lo necesita.

**Los demás cambios:**
- **V28-24:** F4B-E26 en los huecos.
- **V28-25:** F4B-E24 es la pregunta 2, «Basta así», y el límite de D-PBT1-08 es esa pregunta (OB-05.2). Es correcto, pero le falta la cita completa (R-M03).
- **V28-28:** el nombre del encargo.

### 10.5 Regresiones y hallazgos nuevos

| Id | Grav. | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| **PBT1CV28R-I01** | **I** | **Dos líneas testigo de V28-16 no se pueden comprobar en un contrato ASCII.** Son W:327 («El diseño del lado A SOLO…») y W:1289 («La política que se guarda…»). El contrato es ASCII, y el generador las convierte en «El diseno…» y «La politica…». Una búsqueda literal de ese texto en W no las encuentra nunca, así que la comprobación pasa siempre, también si la documentación se queda. El generador lo detecta («hace falta una decision (`WITNESS_ASCII_OK`)») y, al escribir, ese AVISO es un `assert`: así no se puede emitir. **El origen:** son los literales que propuse yo en M02 | `pbt1c/dry-objects-2f5d3dec-v28b.txt`, AVISO 2; el contrato en seco v28b, `Invariants[4]`; `git grep` en `2f5d3dec` | Usar fragmentos solo ASCII, únicos en W y WC en `2f5d3dec`: W:327, «del lado A SOLO, sin la parte compuesta.»; W:1289, «que se guarda: la cargada, exacta, si el usuario no». Las dos de WC ya son ASCII. Medir otra vez en la emisión que son únicos |
| PBT1CV28R-M01 | M | **El ámbito de C13 por nombre.** «A name that does not exist in `BaseSha`» admite leer «en todo el árbol»: un ayudante nuevo con un nombre que ya existe en otro archivo, como `Unchanged` o `Covers` de Application, no haría parar al Worker. Tampoco para ante una segunda declaración de un nombre que ya está en W o WC, una sobrecarga. Las dos las ve el censo de INV-PBT1C-05, que sí está acotado a W y WC y cuenta declaraciones («los dos `LoadCompositeFromDesign»`; 325 → 324). Pero llegan a la verificación como REWORK, y la opción (A) pedía una parada previa | v2.8:1321 y :1314; `inv03_probe2.py`, mutante 4 (`Keep`): el censo pasa de 1 a 2 | En C13: «… a declaration whose name does not exist in W or WC at `BaseSha`, or a further declaration of a name that does (an overload), other than…». En el censo, decir que cuenta declaraciones y que una sobrecarga cuenta |
| PBT1CV28R-M02 | M | **El registro ya no es exacto.** Dice «Ningún cambio toca […] los CT de PB-T1b» (v2.8:67), pero V28-26 reescribe la fila de CT-PBT1B-15 del §5.3 y V28-20 cambia la regla 2 y la lista de autopruebas que usa CT-PBT1B-14. Es fiel a las pruebas corregidas, pero choca con esa frase y con la costumbre de esta v2.8: los textos de PB-T1b ya emitidos se tocaban solo con notas. **Además,** el Estado da la comprobación de la v2.8 por «hecha» y no nombra esta recomprobación; tampoco el «Pendiente antes de emitir», que da como pendiente alinear un generador que a las 20:19 ya está alineado | v2.8:5, :67, :114-116 y :990; V28-20 y V28-26 | «Los textos de CT-PBT1B-14 y 15 (D-PBT1-15 y §5.3) se ponen al día con las pruebas corregidas de `a18ba345` (V28-20 y V28-26); PB-T1b no se reemite, y su texto emitido es el de la v2.7». Añadir esta recomprobación (§10) al Estado y al «Pendiente antes de emitir» |
| PBT1CV28R-M03 | M | **F4B-E23 a F4B-E26: la custodia y la cita.** No están en git (`f4b-dispositions.md` llega hasta F4B-E22). El contrato cita como autoridad la decisión de F4B-E25 y su vuelta. **La v2.8 no dice dónde ni cuándo se custodian:** no están en las listas de PB-T1c ni en el «Pendiente antes de emitir»; solo el generador las trata como «tras el cierre de PB-T1b». **Además,** F4B-E24 se cita sin carpeta (`owner-f4b-dpb07-extensiondata-decisions.md`) y no tiene fila en el §1 ni en el §8.2, que es la misma lección que PBT1BC27-M03 | v2.8:108, :560 y :1530; `ls-tree` de `2f5d3dec` y `a18ba345`; el seco v28b, AVISOS 4, 5 y 7 | En «Pendiente antes de emitir» y en §6.0, «Custodia»: F4B-E23 a F4B-E26 en `f4b-dispositions.md`, y los archivos de F4B-E25 (decisión y vuelta) y de F4B-E23 y E24, custodiados con el cierre de PB-T1b (F4B-E26) o antes de emitir PB-T1c. Para F4B-E24, la ruta completa y su fila en el §1 y en el §8.2, como F4B-E25 |

**Observaciones, sin hallazgo:**
- El hecho 2 de §6.0 (§16.8) podría recoger ya que, en `c78a`, la comprobación Tests pasó con `ChainRedSha` `2d9febc6` (`owner-return-verify-stop.md`, «Lo que no decide esta pregunta»).
- El bloque «**En PB-T1c**» de «Custodia» está sangrado como una subviñeta de «En PB-T1b».

### 10.6 Veredicto de la recomprobación

**EMITIBLE TRAS CORRECCIONES. B/I/M = 0/1/3.**
- **R-I01:** hay que cambiar las dos líneas testigo con tildes antes de emitir. Sin eso, la comprobación queda vacía o el generador no puede escribir.
- **R-M01 a R-M03:** se recomiendan. Son de texto y estrechan.

**Decisiones nuevas para el Owner: ninguna.**
- F4B-E25 ya la tomó el Owner.
- R-I01 y R-M01 estrechan o arreglan una comprobación.
- R-M02 y R-M03 son de redacción y de custodia.

## §11. Confirmación final

**Veredicto de la confirmación: EMITIBLE TRAS CORRECCIONES. B/I/M = 0/0/1. Decisiones nuevas para el Owner: ninguna.**
- **El paquete está bien.** Los cuatro hallazgos de la §10 están resueltos. Los hechos del cierre de PB-T1b coinciden con git en `95373af6`. El apartado Q es fiel al informe de anclas.
- **Lo único que falta es un menor (F-M01).** El generador de las 20:37 no pasa sobre el texto final, por un `assert` que V28-35c dejó atrás. El registro dice que ya está alineado.

### 11.1 Método

Las mismas reglas.

**Lo que hice:**
- **El delta:** `rv-v28/replay_delta2.py` reaplica las 104 sustituciones sobre la v2.7 custodiada. El resultado es el paquete final byte a byte (`I-64-f4b-pbt1-gate-contract-package-v28.md`, sha256 `1d6082ae…0d541f4`, 1606 líneas, igual que el `-draft`). No queda ningún «POR RELLENAR».
- **El cierre en git:**
  - `git diff --name-only a18ba345 95373af6`: solo `docs/`, con las cinco decisiones y vueltas nuevas;
  - `git diff 2f5d3dec 95373af6 -- src`: vacío;
  - `git diff a18ba345 95373af6 -- src tests`: vacío.
- **Los textos de `95373af6`, leídos por objetos git:** F4B-E23 a F4B-E26 de `f4b-dispositions.md`, la evidencia §249, `task-close-package.md` y `state/I-64.yml`.
- **Las decisiones custodiadas** de F4B-E25 y de F4B-E23 y E24 son iguales, byte a byte, a las del scratchpad que revisé en la §10.
- **Las testigo:** `rv-v28/witness_check.py` las extrae del paquete final y las busca, con `git grep -F`, en W y WC de `95373af6` y `2f5d3dec` y en las dos variantes de la simulación.
- **Los recuentos:** `count_git2.py` sobre `95373af6`.
- **El registro:** `pbt1-measurements-v28.md`, comparado con el de `95373af6`, que es igual que en `2f5d3dec`.
- **El generador:** el de las 20:37, sobre una copia fija (`rv-v28/gen/make_pbt1c_contract.snapshot-c.py`, sha256 `a0869670…`, igual que el original), en seco: `--dry --objects-rev 95373af6 --package-file <el paquete final>`.

### 11.2 Los cuatro hallazgos de la §10

| Hallazgo | ¿Resuelto? | Lo comprobado |
|---|---|---|
| **R-I01** (las testigo con tildes) | **Sí** (V28-30) | **Las cuatro subcadenas** de INV-PBT1C-05: «del lado A SOLO, sin la parte compuesta.», «que se guarda: la cargada, exacta, si el usuario no», «Recupera la intencion compuesta de un diseno cargado» y «CARGAR OTRO RACK Y RESTAURAR EL ACTUAL NO SON LO MISMO». **Son ASCII y aparecen una sola vez en W y WC**, en su línea (W:327, W:1289, WC:812 y WC:817), tanto en `95373af6` como en `2f5d3dec`. La de W:1289 sale también en `RackSelectiveWindow.xaml.cs:2165`, fuera del alcance, como dice la nota. **Discriminan:** la variante A de la simulación tiene 0 y la B, 1 de cada una. El generador ya no avisa (`WITNESS_ASCII_OK`) |
| **R-M01** (el ámbito de C13) | **Sí** (V28-33) | **C13:** «a declaration whose name does not exist in W or WC at `BaseSha`, or a further declaration of a name that does (an overload) …». Queda acotada a W y WC y cubre la sobrecarga, y los reenvíos siguen fuera. **El censo de INV-PBT1C-05** cuenta las declaraciones una a una, y una sobrecarga cambia la cuenta. **La nota de INV-PBT1C-03 y la de C13** dicen lo mismo |
| **R-M02** (el registro y el Estado) | **Sí** (V28-32 y V28-34) | **El registro** dice que los textos de CT-PBT1B-14 y 15 se ponen al día con `a18ba345`, que PB-T1b no se reemite y que su texto emitido es el de la v2.7. **El Estado** nombra la recomprobación. **«Lo que queda para la emisión»** está al día, salvo F-M01 |
| **R-M03** (la custodia y la cita de F4B-E23 a F4B-E26) | **Sí** (V28-31 y V28-35) | **F4B-E24** tiene fila en el §1 y viñeta en el §8.2, con su ruta completa. **F4B-E25** lleva la ruta completa en el §1, el §6.0, el §6.3 y el §8.2. **«Custodia»** tiene una viñeta propia de PB-T1c, que lista lo ya custodiado. Los cinco archivos y F4B-E23 a F4B-E26 están en `95373af6`, y los diez nombres nuevos de `F4B-DRAFTS/pbt1/` siguen libres |

### 11.3 Los hechos del cierre (V28-29) frente a git en `95373af6`: coinciden

| Lo que dice la v2.8 | En `95373af6` |
|---|---|
| CLOSED / PASS WITH LIMITATIONS sobre `a18ba345eddc5333c22b8bacbad5c2ed4dbf5959`, attempts 2 de 3 | `task-close-package.md:3`; F4B-E26, «Resultado»; `state/I-64.yml`, `attempts: 2`. `git rev-parse` da los dos SHA completos |
| La custodia, `95373af6116cf120bd0a8ba490833524e33c47da`, con F4B-E23 a F4B-E26 y la §249 | `f4b-dispositions.md:668`, `:684`, `:693` y `:711`; `I-64-evidence.md`, «## 249.» |
| RED `126fd65f` y GREEN `a18ba345`, con `git diff 2f5d3dec a18ba345 -- src` vacío | F4B-E26, «Intento 2»; `git diff` vacío |
| La CI del RED `44430615074` (`tests` y `ui-tests` en `failure`, `build-ui` en `success` y `build-plugin` omitido) y la del GREEN `44438060317`, 4/4, con Core 19 086 y UI 2 437 (21 `Skipped` y 2 416 `Passed`) | F4B-E26, «CI»; `task-close-package.md:25-26`; §249 |
| La revisión independiente del intento 2, ACEPTABLE CON MENORES 0/0/5 | F4B-E26, «Revisiones»; §249 |
| La verificación `c78a`, STOP en Authority (F4B-E25), y la reverificación `R20261119T020026Z-dc81`, 14/14 | F4B-E26, «Verificación»; `task-close-package.md:31` |
| nc1 `a4e8`, nc2 `448d` y nc3 `2049`, CONTROL PASS | F4B-E26, «Controles»; `task-close-package.md:32-34` |
| El hecho de §16.8: «Tests pasó», sin efecto | F4B-E26, «Disposiciones de la sesión»; `task-close-package.md`, §3 |
| La mitad (b) de M14, resuelta: el `Trailer` es el real | `task-close-package.md:62` |
| Los recuentos 13, 1, 2 y 557, y `PushBackRichPreviewTests` 9 | **Mis recuentos en `95373af6`:** 13, 1, 2, 557 (69 clases, 3 `Skip`) y 9; los prefijos de PB-T1c, libres |
| La línea base de PB-T1c: Core 19 086 + 2 = 19 088; UI 2 437 + 2 = 2 439 (21 / 2 418) | Las cifras de `44438060317` |

**Lo que no comprobé:** las CI, por GitHub. El resto, contra el texto custodiado.

### 11.4 El apartado Q frente al informe de anclas: fiel

- **`pbt1-measurements-v28.md`** (sha256 `cd2e9a10…`) es el registro de `95373af6` más 133 líneas al final, el apartado Q. No cambia nada de A a P.
- **Q-0 a Q-2 son los apartados (a), (b), (d), (e), (f) y (g) del informe:** `src/RackCad.UI` igual que en `b5c09f0b`, con el árbol `c38679f8…`; 3700, 936 y 793 líneas; 59 anclas y 0 diferencias; 21 asignaciones a portadores y 16 líneas que se quedan; los dos escritores fuera de `LoadFromModel`; el comentario de W:405; 0 desajustes de API; T-SG en PASS y los siete sitios; una sola `.RequestUpdate(`; 24 y 5 `SetNumber(`; y 40 costuras.
- **Q-5 es el (h),** más la columna de `95373af6`, que he medido.
- **Q-6 es el (i) y `simulate-out.txt`:** 37 hunks en W, y 4 o 5 en WC; 320 y 310 miembros; 1 063 miembros en CT-PBT1B-14; las autopruebas, en un miembro neutro.
- **Q-7 reparte I-1 a I-3 y M-1 a M-4** como la v2.8, sin bloqueantes ni hechos nuevos, como el informe.
- **Q-3 y Q-4 añaden lo que vino después** (las dos reglas de los rangos, las testigo y el censo). Lo he reproducido aparte (`inv03_probe2.py` y `witness_check.py`). El censo absoluto de mi heurística sale una declaración por debajo (324 frente a 325), pero la diferencia es la misma, como dice Q-8.

### 11.5 Regresiones y decisiones

| Id | Grav. | Contenido | Evidencia | Corrección |
|---|---|---|---|---|
| PBT1CV28F-M01 | M | **El generador no pasa sobre el texto final, y el registro dice que sí.** V28-35c cambió la cita de F4B-E25 del §6.3 a la ruta completa (`` `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/…` ``). El generador de las 20:37 sigue buscando `` `F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md` `` con la comilla de código delante, y para (rc 1, `AssertionError: V28-22`). Falla de forma segura, porque no escribe nada. Pero V28-34b afirma que «el generador ya está alineado con la v2.8 (la recomprobación lo reprodujo en seco)», y lo que reproduje en la §10 fue el generador de las 20:19 sobre el borrador de las 20:12 | `rv-v28/gen/my-dry-final.txt`. Una copia de prueba con solo ese `assert` sin la comilla (`probe-gen-c1.py`) da rc 0 y 58 191 bytes, sha256 `ADCF97E8…5B98F865`: «textos de la v2.8 comprobados», 25 rangos, las testigo, `src` del cierre igual que `a18ba345` y que `a0a5f699`, `f4b-dispositions` hasta F4B-E26 y 12 AVISOS, todos de la custodia de la emisión | En el generador, quitar la comilla de código de delante en ese `assert`, o buscar la ruta completa. En el registro (V28-34b), no decir que está alineado hasta que su `--dry` sobre el texto final dé rc 0, y custodiar ese seco |

**Regresiones en el paquete: ninguna.** V28-29 a V28-35 solo rellenan huecos con hechos que coinciden con git, estrechan (V28-30 y V28-33) o arreglan citas y redacción.

**Observaciones, sin hallazgo:**
- La custodia de PB-T1c nombra la «recomprobación (§10)» del informe. Este informe tiene ya también la §11.
- El primer menor de la revisión del intento 2 (PBT1B2-IR-M01: la tercera autoprueba de CT-15 no distingue la comprobación nueva de la antigua) no está en D-PBT1-15, condición 8. Está anotado en `docs/ideas-futuras.md` y no afecta a PB-T1c, que no hereda CT-15.

**Decisiones nuevas para el Owner: ninguna.** F4B-E23 a F4B-E26 ya están tomadas o son el cierre, y F-M01 es de herramienta y de redacción.

### 11.6 Veredicto de la confirmación

**EMITIBLE TRAS CORRECCIONES. B/I/M = 0/0/1.**
- **El paquete final (v2.8, sha256 `1d6082ae…`) se puede emitir tal cual en su contenido.** No tiene ningún bloqueante ni ningún importante, y sus hechos y su registro Q son fieles a git.
- **Antes de escribir el contrato:** el generador tiene que dar rc 0 sobre este texto (F-M01: un `assert`), y la frase de V28-34b tiene que decir lo que es.
