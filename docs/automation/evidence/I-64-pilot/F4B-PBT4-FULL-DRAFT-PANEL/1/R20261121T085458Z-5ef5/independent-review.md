# Revisión independiente del código: I-64 F4B PB-T4b (`F4B-PBT4-FULL-DRAFT-PANEL`), intento 1

**Objeto:** `git diff 8f783df5..c065080d`, con tres commits:
- el RED, `c1e4f458`;
- la reparación del RED, `9b2d1893`;
- el GREEN, `c065080d`.

WorkRunId `R20261121T085458Z-5ef5`.

**Autoridades** (todas leídas en `c065080d`):
- el contrato, `docs/automation/evidence/I-64-pilot/F4B-PBT4-FULL-DRAFT-PANEL/coordinator/gate-contract.json`;
- el paquete v4.9, `docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md`, citado como «paq.» con su línea;
- las decisiones D-S1 a D-S6 y la regla RR (`owner-f4b-pbt3-emission-red-repair-decisions.md`);
- D-PB-05, en `F4B-DRAFTS/package/package-v2.md:240-265`.

`git diff --stat 8f783df5 c065080d -- docs` sale vacío: la entrega no toca `docs/`.

**Cómo leí:**
- El código y los documentos, solo por objetos git desde el checkout principal (`show`, `diff`, `grep`, `ls-tree`, `log`).
- La entrega, en la copia `rv/`.
- No leí, ni listé, ni nombré el worktree de la rama. No compilé ni ejecuté pruebas.

Los guiones de Python (`-I`, escritos con la herramienta de archivos, en el scratchpad) hicieron solo esto:
- leyeron blobs y TRX;
- ejecutaron **en memoria** el generador custodiado `green_pbt4b_v45.py`, que solo hace `git show` y devuelve texto;
- volvieron a hacer, con un extractor propio, la comparación de las copias.

Las notas de método están en «Lo no verificado».

---

## Veredicto

**ACEPTABLE CON MENORES, condicionado a que el Owner disponga I-01** (sin cambio de código). **B 0 / I 1 / M 8**, más **4 hallazgos OWNER**.

**Lo que está bien:**
- El código de `src/` es, byte a byte, el que da el generador custodiado:
  - el esqueleto del RED, 6 de 6;
  - el GREEN, 10 de 10.
- Los 478 `Skip` son exactamente los de la lista, con su motivo literal. En los 21 archivos no cambia nada más.
- Las 356 copias mecánicas son, una a una, su original con el mapa M. Lo comprobé con un extractor propio, no con el de la CT.
- Las 21 copias y el kit tienen su `CopySha`.
- Los totales de las tres ramas cuadran con F4B-E46.
- La CI da lo que pide el contrato: 4/4 en el GREEN y la firma del RED, comprobado con `gh`.
- No hay ninguna violación de D2, ni ninguna traducción de estado.

**I-01** es una desviación **declarada** del texto de §5.2 en CT-PBT4B-20 (5):
- el contrato no dejaba esa decisión al Worker (C07);
- el texto literal no se puede cumplir con las copias custodiadas, así que hace falta una disposición del Owner o una versión del paquete.

**No es REWORK.** Ningún cambio del Worker cumpliría el texto literal sin cambiar el paquete. El resto son debilidades de discriminación de CT (dos de ellas, de la letra del paquete), vacíos heredados de guardas y notas de registro.

## Recuento

| Severidad | N.º | Ids |
|---|---|---|
| Bloqueante | 0 | — |
| Importante | 1 | I-01 |
| Menor | 8 | M-01 a M-08 |
| OWNER | 4 | OWNER-01 a OWNER-04 |

---

## Tabla de hallazgos

Todas las rutas, en `c065080d` salvo que se diga otra cosa.

### I-01 (I). CT-PBT4B-20, comprobación (5): acotada a las 9 clases de C-0

**Dónde:**
- `tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs:630-632` (regex en :201-202);
- paq. §5.2, fila 20, (5).

**Qué:**
- §5.2 dice «Ninguna copia nombra …». El Worker evalúa (5) solo sobre las 9 copias de C-0.
- **Tal cual, (5) da `sin-nombres-del-kit-del-piloto=False`** sobre el RED y el GREEN. Hay 5 apariciones, todas en literales de cadena de dos copias de guarda custodiadas:
  - `PushBackWritePathGuardFull.cs:103-104, :107`: la etiqueta del log y el texto inyectado `_drafts.Find<PushBackPilotDraftState>(…)`;
  - `WorkspacePilotEditGuardFull.cs:583`: un `InlineData` inyectado, `new PushBackPilotFieldAdapter()`;
  - **`WorkspacePilotEditGuardFull.cs:255`: la regex viva de un ayudante (`WriteViolations`), no un texto inyectado** (véase M-04 y M-05).
- La línea esperada no cambia, pero sí el objeto de la comprobación.
- C07 dice que el Worker no ajusta lo esperado, y que un hecho que cambia una CT es parada. La decisión no estaba delegada. Está declarada en `Deviations[1]` y en `UnexpectedFindings[4]`.

**Recomendación:** disposición del Owner o de la sesión, en `f4b-dispositions.md`, con dos opciones:
- (a) aceptarlo como errata del paquete: (5) solo es satisfacible sobre C-0, o sobre el código sin cadenas;
- (b) una versión del paquete que reformule (5).

En los dos casos, sin cambio de `src/`.

### M-01 (M). CT-08, tras la reparación: `resto-del-rack-actual` no discrimina

**Dónde:**
- `tests/RackCad.Tests/PushBackFullPanelTests.cs:464-494`, en especial :484-487;
- la receta `KeptSetup`, :314-329;
- `rv/tests/tmp/core-TmpDiagTests.trx`.

**Qué:**
- El escenario solo cambia el nombre entre el rack de base y el actual (`Realistic(null)` frente a `Realistic("Otro nombre")`).
- Al quitar `Name`, los cinco campos que quedan son idénticos en el sobre viejo y en el actual: `Id`, `SchemaVersion`, `Kind`, `View` y `Section`. El diagnóstico temporal del Worker lo muestra: `A1/B2/C3 written` igual a `now`, salvo `name`.
- La línea sería `True` aunque el escritor tomara el sobre del borrador viejo.
- Lo que sí fija: que cada miembro conserva su propia vista y sección, y que se escribe el diseño (escala 2.25).
- Los pasos 5 y 7 de D-PB-05 (lo que no es del editor y la identidad vienen del rack actual) no se fijan por el panel. La procedencia del sobre la cubre el equivalente de WPP `TheOutgoingDesignAndEnvelopeOfEachViewComeFromTheRereadEnvelopeAndNeverFromTheDraft`.

**Recomendación:** registrarlo. En PB-T4c, o en una corrección, añadir una variante en la que el rack actual difiera del borrador en un campo del sobre que no sea del editor.

### M-02 (M). CT-20, más débil que §5.2 en (1) y en (7), sin declararlo

**Dónde:** `WorkspaceFullCopyGuardTests.cs:198-199, :232, :483-484, :508, :532-562`.

**Qué:**
- **(1):** `Restore` acepta cualquier motivo `I-64 F4B PB-T4b: …`. La CT no comprueba el motivo literal de cada causa, y la clasificación mecánica o a mano sale de ese motivo (:508).
- **(7):** no compara las dos mitades de cada `Theory` de D-S5 salvo sus filas, ni que ninguna otra prueba de la lista esté en las dos clases. Esto último solo lo garantiza, para las mecánicas, `found.Count == 1`.
- El `CopySha` (4) lo cubre en la práctica.
- Lo comprobé aparte, y está bien:
  - los 478 motivos son exactos;
  - las cuatro parejas son iguales salvo sus filas;
  - ninguna prueba está duplicada salvo las cuatro.

**Recomendación:** anotarlo. Si se reabre CT-20, añadir las dos comprobaciones.

### M-03 (M). Las autopruebas de CT-20 no aíslan (3)

**Dónde:** `WorkspaceFullCopyGuardTests.cs:641-647`.

**Qué:**
- `autoprueba-asercion-cambiada` y `autoprueba-sin-copia` cambian el texto de la copia, así que `sha256=False` basta para que la línea difiera.
- No prueban que `igual-al-mapa` discrimine: un `ApplyMap` roto que siempre diera `True` pasaría las autopruebas.
- `sin-copia` sí discrimina además por `pruebas=`.
- La igualdad real con el mapa la verifiqué yo: 356 de 356, 0 diferencias.

**Recomendación:** anotarlo. Una autoprueba que mute la copia y recalcule el hash, o que compare sin el sha.

### M-04 (M). Vacíos de PEG, en el GREEN, fuera de la lista y sin declarar en §4.2

**Dónde:**
- `tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs:592-597`;
- `:906-909` y `:917-920`;
- en la copia, `WorkspacePilotEditGuardFull.cs:255-258`.

**Qué:**
- `TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark`: su `Replace("private DraftSession<PushBackPilotDraftState>? FindPilotDraft", …)` ya no encuentra nada, así que el control positivo no inyecta nada y `Assert.Empty` pasa sobre el anfitrión intacto.
- `TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing`, y la copia de PEG: la regex de `FindPilotDraft` sigue con el tipo del piloto, `TryBody` da null y el bucle hace `continue`. `FindPilotDraft` deja de revisarse.
- Las dos pasan en la CI del GREEN.
- Ninguna figura en V49-02, que declara otros seis métodos de PEG. C16 solo cubre las que fallan.
- No es del Worker: es letra existente y texto custodiado.

**Recomendación:** al registro y a PB-T4d. Véase OWNER-03.

### M-05 (M). La justificación de I-01 en la entrega es inexacta en una aparición

**Dónde:**
- `rv/worker-handoff.json`, `Deviations[1]` y `UnexpectedFindings[4]`;
- `WorkspacePilotEditGuardFull.cs:255`.

**Qué:** la entrega dice «nombran … en el texto que inyectan sus autopruebas». Una de las cinco apariciones es la regex de un ayudante vivo, no un texto inyectado, y es justo la de M-04.

**Recomendación:** corregir la nota al registrar I-01.

### M-06 (M). CT-18 es tautológica en el GREEN

**Dónde:** `tests/RackCad.UI.Tests/PushBackFullPanelFieldTests.cs:215-237`.

**Qué:**
- Con la delegación, `NumericFieldValidation.Validate` es `NumericTextRule.Check` traducido, así que compara la regla consigo misma.
- La prueba de que el comportamiento no cambia está en otro sitio, y está en verde:
  - `NumericFieldValidationTests` (13) y `NumericFieldTests` (10), en local;
  - toda la UI, en la CI.
- Es la letra de §5.2.

**Recomendación:** ninguna. Constancia.

### M-07 (M). El registro de relevo etiqueta mal «PorAsercion»

**Dónde:**
- `rv/relay-record-work.json`, `Notes[8]`;
- `rv/red-ci-core/core.trx`.

**Qué:**
- Los números (`FullPanelCopyWorkspacePilotPrepareRedTests: 0`, suma 135) cuentan solo los fallos con mensaje `Assert.*`.
- Con la regla del contrato (la primera línea del StackTrace bajo `tests/`), **son 306 de 306**, con 171 `NullReferenceException` en código de prueba.
- Leído tal cual, parece una parada C04 que no existe.

**Recomendación:** renombrar el campo, o añadir el recuento por la regla de I-02.

### M-08 (M). El texto «> 0» tiene dos fuentes

**Dónde:** `src/RackCad.Application/Systems/PushBack/Draft/PushBackFullFieldAdapter.cs:23-24`, frente a `NumericTextRule.cs` (`MinimumMessage`).

**Qué:**
- `NotPositiveMessage = "El valor debe ser > 0."` es un literal aparte del que compone la regla con `Format(0)`. Hoy coinciden en toda cultura.
- Es texto del paquete: M-06 de §6.2 exige la constante.

**Recomendación:** ninguna en PB-T4b. Que PB-T4d lo derive, o lo fije con una prueba.

---

## 1. Alcance y forma

- **El RED** (`git diff --name-status 8f783df5 c1e4f458`):
  - 32 A: las 6 clases de las CT, el kit, las 21 copias y los 4 archivos nuevos de `src/`;
  - 23 M: los 21 archivos de los `Skip`, más WEI y FVM.
  - WHC, RIA, PV y NFV no cambian.
- **El esqueleto, byte a byte.** El generador custodiado se ejecutó en memoria sobre `8f783df5`:
  - `green_pbt4b_v45.py`, con sha256 `47662f52…`, igual en la base y en el GREEN;
  - sus módulos `green_pbt4b.py` (`6d2fde68…`) y `green_pbt4a.py`.
  - `skeleton()` da exactamente los blobs de `c1e4f458` en los 6 archivos, con sha `e78a6429`, `d7706f52`, `eff6f487`, `6a2d8d32`, `151b3437` y `3cd599f7`, los de `apply-red.txt`.
  - Las 49 entradas de `apply-red.txt` (src y tests) cuadran con los blobs de RED, RR y GREEN: 0 diferencias.
- **El GREEN es el texto literal.**
  - `green()` es igual a los blobs de `c065080d` en los 10 archivos.
  - WEI no cambia desde el RED.
  - `9b2d1893..c065080d` toca solo esos 9 archivos de `src/`: ningún archivo de `tests/` y ninguno de `Pilot/`.
  - Domain, `RackPushBackCommands.cs`, `RackCommandSupport.cs` y NH no cambian.
- **El alcance.** `git diff --name-only 8f783df5 c065080d` es exactamente el `AllowedWriteScope`: 59 de 59.
- **C13, por lectura del diff:**
  - WHC: +`FullReadMissingReason`;
  - WEI: +`Presented` (miembro del enumerado y fábrica), +el segundo constructor privado, +`_presentationLines`, +`PresentationLines`, +`KeepWitness`;
  - FVM: +`_keepWitness`, +`_keepLines`, +`KeepMinePresentationText`, +`ShowKeepMinePresentation`;
  - PV: +`BrowserFieldKeepMinePresentationText`;
  - RIA: +el segundo constructor interno, +`_full`;
  - NFV: ninguna; sus tres privados pasan a `NumericTextRule`;
  - los cuatro archivos nuevos, con los nombres de la lista.

  Nada más. Cuadra con el censo de la entrega (`rv/inv/inv-final.txt`).
- **El trailer** es `Co-Authored-By: Claude Sonnet 5.5`, en los tres commits. La cadena:
  - el GREEN es hijo de RR;
  - RR es hijo del RED;
  - el RED es hijo de la base.

## 2. La lista y los `Skip`

Usé un guion propio, que no reutiliza `pbt4b_skips_v48.py`.
- Derivé el motivo esperado de cada fila con la causa de la lista y el mapa v5 (356 mecánicas y 72 a mano).
- Comparé cada uno de los 21 archivos, línea a línea, con la base en RED, RR y GREEN.

**Resultado:**
- 21 archivos, **478 `Skip`, 0 problemas**: el mismo número de líneas, y solo cambian líneas `[Fact]`/`[Theory]` → `[…(Skip = "…")]`;
- el método que sigue a cada línea está en la lista de su archivo;
- el motivo es el literal de su causa.

Recuentos por archivo: 2, 1, 1, 10, 54, 7, 62, 1, 2, 69, 5, 59, 67, 1, 28, 1, 35, 2, 44, 21 y 6.
- Por causa: C-0 428, C-5 23 y UI 27.
- Por proyecto: Core 451 y UI 27.

`git diff -U0 8f783df5 c065080d -- tests` no añade ningún otro `Skip` de atributo. Hay una línea en `WorkspaceFullCopyGuardTests.cs:643`, pero es un texto inyectado de una autoprueba.

## 3. «Saltar y duplicar»

**Las 356 mecánicas** (un extractor de C# propio, que tiene en cuenta cadenas, interpoladas, verbatim, crudas y comentarios):
- cada una es el texto de su método en `8f783df5`, de la primera línea de atributo a la llave de cierre, con M-00 a M-08 (los tipos de M-00, los de §5.2);
- cada una está **una sola vez**, en su clase Red o Green;
- **0 diferencias.**

Por clase:

| Clase | Mecánicas | A mano | Red / Green |
|---|---|---|---|
| WAB | 10 | 0 | 1/9 |
| WCA | 54 | 0 | 6/48 |
| EIH | 57 | 5 | 18/44 |
| WLH | 68 | 1 | 22/48 |
| PEH | 53 | 6 | 49/10 |
| WPP | 46 | 21 | 22/47 |
| WPRH | 17 | 11 | 23/5 |
| WPUH | 12 | 23 | 22/13 |
| WPUR | 39 | 5 | 43/1 |

En las clases que parte D-S5, una prueba cuenta en las dos.

**Los 72 a mano y los 22 de guarda:**
- Generé el diff de cada uno contra su original con M, 125 entradas, y leí una muestra:
  - las de «Conservar» en dos tiempos de WPP y WPRH, con `Presented` y luego `Kept` con `FullHandKit.Seen`;
  - el enumerado de 12 estados;
  - las dos de V48-02, `InvalidE_…` y `ThePilotNeverErasesAView…`: con el modelo completo, `Ready` y borrado `[B2]`;
  - la tabla de WHHG con su autoprueba;
  - G-4 con `ExecuteFull` y su autoprueba;
  - CT-PBT3B-13, CT-PBT4A-03 y CT-PBT4A-05 con `FullReadRig` y `FullEditIntentFakeReader`.
- En lo que leí, cada una conserva lo que protegía y afirma lo nuevo donde el original afirmaba lo viejo.
- Las otras no las leí una a una. Su texto lo fija el `CopySha`, y es el del paquete (`pbt4b-equivalents-review-v49.txt`).
- **Las 27 de UI y RG:109-123** son idénticas a su método de la base.

**Las cuatro de D-S5:**
- cada una está una vez en Red y otra en Green;
- las dos mitades son iguales salvo las líneas `[InlineData]` o `StatusNamesRed`/`StatusNamesGreen`;
- la línea `SPLIT_COMMENT` está antes de cada `[Theory]`;
- las filas, juntas, son las del original: CT-20 lo da en verde, con 3/7, 1/3, 7/1 y 7/1.

**El `CopySha`:**
- las 21 copias, normalizadas a LF, tienen el `CopySha` de `pbt4b-precondition6.json` en RED, RR y GREEN;
- el kit tiene `b4ed29f9…` y es igual a `pbt4b-kit-WorkspaceFullReadKit-v45.cs`;
- los 21 `BaseSha` de `Origins` (CT-20) son el sha real de la base y están en el paquete.

## 4. Lo que la entrega declara

### (a) CT-PBT4B-20, la comprobación (5)

Véase I-01.
- **¿Es una desviación?** Sí, del texto literal de §5.2.
- **¿Qué copias nombran esos tipos, y dónde?** Solo `PushBackWritePathGuardFull` y `WorkspacePilotEditGuardFull`, siempre en literales de cadena, nunca como identificador de código:
  - cuatro son texto inyectado o esperado;
  - una es la regex de `WriteViolations`, que en el GREEN no casa con nada (M-04).
  - El kit nombra `PushBackPilotDraftState` en código (`WorkspaceFullReadKit.cs:128-135`, D-S3), pero no es una «copia».
- **¿Qué da (5) tal cual sobre el GREEN?** `False`, y también sobre el RED.

**¿Otras comprobaciones cambiadas respecto de §5.2?**
- **CT-20 (1) y (7)** (M-02) **y sus autopruebas** (M-03): más débiles que el texto, sin declararlo.
- **CT-20 (6):** el quinto escaneo va escrito en línea, en lugar de llamar a `FullEditIntentKit.OriginMissing`, pero es el mismo.
- **El resto de las CT** fija lo que dice §5.2 (§7).

### (b) La reparación del RED

**RR-1 a RR-6 y la n.º 10, cumplidas:**
- Un solo commit, `9b2d1893`, hijo directo de `c1e4f458` y padre del GREEN.
- Sus CI, por `gh run view`:
  - el RED, 44893322578, a las 10:14;
  - RR, 44899232904, a las 11:29;
  - el GREEN, 44903818806, a las 12:26.
- Toca solo `tests/RackCad.Tests/PushBackFullPanelTests.cs`, una clase que el RED añadió. Numstat 3/1.
- **No cambia ninguna línea esperada ni ningún literal**: el diff es una condición y dos líneas de comentario, y coincide con `rv/rr/red-repair.diff`.
- El criterio del RED se repitió sobre RR: los TRX de RR fallan exactamente las mismas 306 + 3 pruebas que el RED.
- La CI de RR da tests y ui-tests en failure, build-ui en success y build-plugin omitido.
- Está declarada (RR-5). `RedSha` sigue siendo el RED original. El trailer va también en RR.

**¿Es legítima la razón?** Sí:
- `PushBackEditableState.Name` es un campo editable (`PushBackEditableState.cs:95`);
- `PushBackFullComposition.ForFullWrite` escribe `copy.Name` (`PushBackFullComposition.cs:78`);
- `KeepOn` copia el editable entero (:93-98);
- el paréntesis de la fila 08 («los campos no editables, la identidad y la versión») excluye el nombre.

Incluirlo era un error de la receta. El efecto colateral es M-01: sin el nombre, la lectura ya no discrimina.

### (c) El nombre tras «Conservar» y «Actualizar»

- **D-PB-05 lo cubre.** El paso 4 dice «all fields OWNED by the full Push Back editor come from the user's draft», y el nombre es del editor. El paso 5 solo trae del rack actual lo que el editor no posee.
- **Se presenta antes del segundo «Sí»** como «Cambiado fuera: el nombre.» (CT-06, líneas del anfitrión; CT-14).
- **La presentación no dice qué valor gana.** El usuario puede no haber tocado el nombre y perder el cambio externo al conservar.
- §5.5 no lo lista. Véase OWNER-01.

## 5. El diseño frente al código

Todo en `c065080d`.

| Diseño | Código | Resultado |
|---|---|---|
| D-PBT4-04 `Begin` con `full.Read` y el comparador y el copiador completos | WHC:349-361 | OK, literal |
| Sin `FullRead`: `NotAdmitted(FullReadMissingReason, null, ClassicEditorHint)`; las 5 relecturas, `Failed` con el mismo texto | WHC:50, :352; :434, :533, :621, :711, :813 | OK |
| La admisión y el diagnóstico, de la lectura completa, también en las dos comprobaciones | WHC:365-372, :441-443, :624-626 | OK |
| 9 `Find<PushBackFullDraftState>`; `FindPilotDraft` y `ViewOfPilotDraft` sobre el completo; la escala, `Editable.AnnotationScale` | WHC:… :1760, :1776, :1808 | OK. «El rack no tiene escala…» desaparece |
| `var pilot = read.PilotRead;` solo en las cuatro que leen el testigo | `Begin`, la recarga, «Conservar», «Descartar» | OK |
| D-PBT4-05 (OB-03.2): A/B admitido; la cola y AUTH-13, rechazados | CT-02 | OK |
| D-PBT4-06 `NumericTextRule.Check`, movido sin cambio | `NumericTextRule.cs`, frente a la NFV de la base | OK: la lógica, idéntica; `Format` con `CurrentCulture`, como antes |
| La NFV delega, con la misma API pública; constantes `= NumericTextRule.*`; el estado, por su posición | NFV:62-63, :86-87; los dos enumerados, en el mismo orden | OK. Las dos «líneas de firma» que mide el relevo son las constantes, con el mismo nombre, tipo y valor |
| `PushBackAnnotationScaleRule`: `Minimum 0.0`, `MinimumInclusive false`, `Check(text,false,false,0,null,false,true)` | `PushBackAnnotationScaleRule.cs:12-20` | OK |
| El adaptador estático del campo, por defecto; nada asigna la regla | WHC:44; CT-16 | OK |
| D-PBT4-07: `Precheck` + `ExecuteFull` en `TryUpdatePilot` | WHC:902, :933 | OK. Ningún `Execute(` |
| D-PBT4-08, paso 1: `RequestKeepMine`, sin puerto | CT-14 `pedir llamadas-al-puerto=GetField` | OK |
| D-PBT4-08, pasos 2 a 4: sin testigo, o con uno que no vale, `Waiting`, `Read`, `NotAdmitted` o `Presented(lines, (OriginKey, OriginText))`; con `Unchanged`, `KeepOn` | WHC:715-724, :728 | OK. No toca el borrador ni `_knownClean` |
| La petición con el testigo, solo en «Conservar»; FVM nombra `TryKeepMine` una vez | FVM:612, :634-635; CT-15 | OK |
| `Presented`: vuelve a preguntar con las líneas y el testigo; si ya no se ofrece, `SituationChangedText` | FVM:905-919; el reinicio, en :702-703 | OK |
| El `TextBlock` dentro del panel de la elección, después de la pregunta y antes del Sí/No | PV:263-266 | OK |
| D-PBT4-09: los 9 literales, la unión «, » / « y », las líneas 1 a 7, `<n>` invariante, `IsEmpty` | `PushBackKeepMinePresentation.cs` | OK, con CT-06 |
| `Read`: el clasificador con `InitialDraftState`, `read.Read.InitialState`, `CurrentDraftState`, los sobres mutables de `StaleBase` y el catálogo; `OriginText`; `Diagnostic` | ídem | OK. WHC no nombra `.InitialDraftState`, `.StaleBase` ni `.Entries` fuera de sus consumidores |
| D-PBT4-21: el `Lazy`, el segundo constructor, `_full`, `Full => _full?.Value`, la línea del éxito | RIA:42, :52-58, :60, :151 | OK. `AtOrigin`, `ReadAt` y NH no cambian; NH pide `read.Full` dentro del bloqueo |
| La excepción de la lectura diferida | `PushBackFullDraftReader.Read` tiene un `catch (Exception)` total; NH tiene el suyo | OK: diferirla no cambia ningún motivo |
| D-PBT4-10: `Pilot/` no cambia | el diff | OK |

## 6. D2 y su límite

- **Las líneas añadidas en `src/`** no nombran `WorkingBaseline`, `LastModuleReconciliation`, `PushBackSystem`, `RackModuleReconciliationResult` ni `new PushBackFullDraftState(` (grep sobre el diff). No hay código nuevo de copia:
  - `Begin` usa `PushBackFullDraftStateComparer.Copy` (PB-T3);
  - `KeepOn` usa `PushBackEditableStateComparer.Copy` (PB-T3).
- **Lo único que muta:**
  - `PushBackFullFieldAdapter.Apply`, que cambia `current.Editable.AnnotationScale`, un `double` del estado de trabajo, como pide el núcleo;
  - la presentación clasifica con el clasificador puro de PB-T3a, sin copiar ni mutar. CT-06 lo comprueba con el comparador y la huella; CT-10, con la huella de las dos referencias compartidas.
- **Ninguna línea muta un `PushBackSystem` ni un informe adoptado.** CT-10 «sustituir-referencia» sustituye la referencia en el estado del borrador, en la prueba y como prescribe §5.2.
- **Nada traduce.** CT-12 da coincidencias=[]. El estado completo sale de `PushBackFullDraftReader` sobre el mismo escaneo; el único constructor de estados completos a mano está en el kit de pruebas (M-34).

## 7. Las CT

- Hay un `[Fact]` por CT (11 + 4 + 1 + 1 + 1 + 3 = 21), sin ninguna `Theory`.
- Cada una acaba en un `Assert.Equal` de las líneas unidas por `\n`.
- Los espacios de nombres son los de §5.2.
- Solo `WorkspaceFullCopyGuardTests` empieza por `Workspace`.
- Los ayudantes usados existen en la base y no se editan: `FullWriteKit`, `WorkspaceHostSource`, `PushBackFullEditorCharacterizationSupport`, etc.

| CT | ¿Fija §5.2? | Las autopruebas | El RED (`rv/red-ci-*`) |
|---|---|---|---|
| 01 | Sí | — | `begin tipo=null piloto=no-nulo` |
| 02 | Sí | — | `pb-20-ab estado=NotAdmitted` |
| 03 | Sí | — | EXCEPCION NRE, que se vuelve línea |
| 04 | Sí, ventana real sin mostrar | — | `mensaje-igual=False` |
| 05 | Sí | — | NRE, que se vuelve línea |
| 06 | Sí: el escaneo por `IntentKit.Variant(…, name:)` vía `FullPrepareKit` | — | NRE, que se vuelve línea |
| 07 | Sí | — | NRE |
| 08 | Sí tras RR; la segunda mitad no discrimina (M-01) | — | NRE |
| 09, 10, 11, 17 | Sí | — | NRE o línea |
| 12 | Sí (cuenta `ExecuteFull` y `Precheck` en el cuerpo de `TryUpdatePilot`) | 7; discriminan, por inyección | `nombres=[pilot.Read, KeepPilot…]` |
| 13 | Sí | 10; discriminan | pasa (`false`) |
| 14 | Sí | — | `presentacion texto=[] visible=False` |
| 15 | Sí | 2; discriminan | `peticion-con-testigo=False` |
| 16 | Sí | (no se piden) | `estatico=0` |
| 18 | Sí; tautológica en el GREEN (M-06) | — | `estado-igual=False` |
| 19 | Sí, 33 líneas | 15 + 2 + 13; inyectan en los tres archivos | pasa (`false`) |
| 20 | Desviación en (5) (I-01); más débil en (1) y (7) (M-02) | 4; enmascaradas por el sha en (3) (M-03) | pasa (`false`) |
| 21 | Sí, con las expresiones literales | 5; discriminan | `diferido=0` |

## 8. El RED y el GREEN (las TRX de `rv/`)

**El RED** (`red-ci-core`, `red-ci-ui`), y RR, igual:
- **Core: 19 605 = 18 841 + 306 + 458.**
- **UI: 2 481 = 2 430 + 3 + 48.**
- Las 306 + 3 que fallan están todas en los 21 filtros con `ExpectRed`, con `Failed == Selected` en cada uno.
- **La primera línea del StackTrace está bajo `tests/` en las 309.**
- `FullPanelCopy*RedTests`: 291 resultados, todos Failed (R_C = 291). `FullPanelCopy*GreenTests`: 323, todos Passed. En la UI, R_UI = 0.
- **Lo omitido:** en Core, las 7 de la base (D-1 y CT-PBT3A-12) más 451 de la lista; en UI, las 21 de la base más 27 de la lista. Es el mismo conjunto en RED, RR y GREEN.
- **Ninguna prueba falla fuera de la lista (C16).**

**El GREEN** (`ci-core`, `ci-ui`):
- **Core: 19 605 = 19 147 + 0 + 458.**
- **UI: 2 481 = 2 433 + 0 + 48.**
- Las 478 de la lista, omitidas.

**Los warnings:** 363 registros de build en `rv/tests`, con 0 líneas `warning CS`.

**La CI, por `gh run view`:**
- el GREEN `c065080d`: los 4 trabajos en success, con «Build Plugin without AutoCAD»;
- el RED `c1e4f458` y RR `9b2d1893`: Tests failure, UI Tests failure, Build UI success y Build Plugin skipped.

## 9. Contención

- **`src/`:** ninguna línea añadida abre una ventana, un `MessageBox` ni un archivo, ni toca el perfil (grep sobre el diff).
- **Las CT de UI:**
  - CT-04 crea una `RackPushBackSystemWindow` sin mostrarla y la cierra en `finally`;
  - CT-14 crea la `WorkspacePanelView`, sin ventana;
  - todo en STA, en la colección del aviso de descarte;
  - nada pulsa Insertar, BOM ni «Guardar en biblioteca».
- **Disco:** `clean-before.txt` y `clean-after.txt` son idénticos (`cmp`): la disposición del configurador y la biblioteca no cambian.
- **La UI se corrió solo con los 17 filtros del contrato.** Hay 0 corridas de UI fuera de ellos. La corrida «stale no-build» usó filtros del contrato y se descartó, declarada.
- **Core:** una sola corrida temporal, `TmpDiagTests`, que permite INV-10: acaba en `Assert.Fail` con sus líneas, guarda TRX y log, y no está en ningún commit (`git grep` vacío en los tres).
- Sin AutoCAD.

## 10. Hallazgos para el Owner

- **OWNER-01. El nombre sobrescrito tras «Conservar» y «Actualizar».**
  - Cumple D-PB-05 (paso 4) y la composición de PB-T3. Se presenta como «Cambiado fuera: el nombre.».
  - La presentación no avisa de que conservar devuelve al dibujo el nombre del borrador, aunque el usuario no lo tocara.
  - No está en §5.5. El Smoke-PB debería mirarlo, y el Owner confirmar que es lo esperado o pedir un texto que lo diga.
- **OWNER-02. La disposición de I-01:** CT-20 (5), aceptada como errata o con una versión del paquete. Es la única decisión que tomó el Worker sin que el contrato se la dejara.
- **OWNER-03. Los vacíos de M-04.**
  - Dos guardas existentes de PEG pasan en vacío en el GREEN, en parte, y su copia hereda uno.
  - La premisa «una prueba que falle fuera de la lista vuelve al Owner» no cubre los vacíos que siguen en verde.
  - Hay que decidir si entran en la lista de PB-T4d o se registran como declarados.
- **OWNER-04. El Smoke-PB, el segundo «Sí» sin cambios en el dibujo real.**
  - Ninguna CT ejecuta RIA ni NH evaluando el testigo tras `AtOrigin`. CT-07 lo simula con `FullHandKit.Seen`.
  - Si en AutoCAD el testigo no volviera `Unchanged`, «Conservar» quedaría en un bucle de presentaciones, porque cualquier otro estado vuelve a presentar.
  - §5.5 lista el paso 3 **con** cambio, pero no el caso normal. Además:
    - «Conservar» lee ahora el dibujo dos veces, con una materialización completa por lectura;
    - M-20 sigue declarado.

---

## Lo no verificado, y notas de método

- **`rv/relay-subst-check.json` no está en la copia de la entrega.** No pude leer el detalle del relevo. Rehíce sus comprobaciones por mi cuenta: los `Skip`, las copias, el kit, el esqueleto y los `warning CS` locales.
- **No ejecuté pruebas ni compilé.** Los resultados son los de las TRX de `rv/`. No comprobé que esas TRX sean los artefactos de los runs citados: sus nombres y horas coinciden, pero no descargué artefactos.
- **No leí los registros de la CI.** Los «sin warning» del Plugin en CI no están verificados; los registros locales sí.
- **De los 72 a mano y los 22 de guarda leí una muestra**, unas 20. El resto lo fija el `CopySha` del texto custodiado del paquete.
- **El comportamiento en AutoCAD** (OB-03.2 real, el testigo real, el borrado real y el coste) es del Smoke-PB.
- **La partición** `e87e024c`: no recalculé su sha. La implican el `CopySha` y las TRX: todas las Red fallan en el RED y todas las Green pasan. Red 206 / Green 225 en C-0, como en el relevo.
- **Notas de método:**
  - ejecuté en memoria el generador custodiado (solo `git show`) y `gh run view`, de solo lectura;
  - por error lancé una vez `python -I -` con un heredoc que solo imprimía una cadena, sin leer ni escribir nada; no se repitió;
  - los guiones y sus salidas están en `_rvtools/` del scratchpad de la revisión, nunca en el repositorio.
