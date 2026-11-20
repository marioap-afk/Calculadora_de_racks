# CA-T4a: comprobación acotada de PB-T4a (F4B-PBT4-FULL-READ-PATH), sobre el contrato en seco y la v4.2

**Veredicto: EMITIBLE TRAS CORRECCIONES.** B/I/M = **0 / 1 / 4**

**En resumen:**
- **El contrato en seco es fiel a la v4.2** en todo lo que se pidió comprobar:
  - el alcance;
  - el `ForbiddenWriteScope`, recalculado entero;
  - las 14 invariantes;
  - las 34 paradas, con C11 literal y C13 con `ReadAt`;
  - los 38 filtros;
  - las 15 evidencias, con los totales de la rama única;
  - las 47 autoridades.
- **B-08 está resuelto.** EIG:350-364 queda en verde con `ReadAt` en `Draft/`.
- **Un importante, I-01.** El texto literal de `PushBackOriginByText.cs` usa anotaciones `?` de tipos de referencia (`string? Choose` y `(string?)pair.Value`) en un archivo que no puede llevar `#nullable`, en un proyecto con `Nullable=disable`.
  - Da el aviso CS8632. **La propia compilación de comprobación de la sesión lo muestra**, en el esqueleto, en `PushBackOriginByText.cs(16,29)`, y su informe no lo dice.
  - El trabajo `build-plugin` de la CI falla con cualquier aviso `CS`. Con el texto de hoy, el GREEN no puede dar la CI 4/4 sin apartarse del literal.
  - El arreglo es de texto: dos símbolos.
- **El recorrido de las guardas (I-25) no es entero, como dice.** Su selección por marcas no ve las clases que componen la ruta con `Path.Combine` o barren `src/` entero.
  - Esta comprobación las recorrió: son 97 archivos fuera del recorrido, y ninguno cae.
  - Queda como corrección de texto (M-01).
- **No hay decisiones nuevas para el Owner.** La lista de PB-T4a, de 1, está dentro de la cota.

## §1. Qué se revisó y cómo

**Las entradas** (en `hs65\pbt4\`, con su sha256 comprobado):
- **el paquete** `I-64-f4b-pbt4-gate-contract-package-v42.md` (`f3e5ae4d…7b`):
  - el delta V42-01 a V42-11;
  - D-PBT4-02 y 03;
  - §4.1, §5.0 y §5.1;
  - §6.1, el bloque ASCII §6.1.1 y §6.1.2;
  - §8.3, §9 y §11;
- **el generador** `make_pbt4a_contract.py` (`c8d11e8c…`), con su `bounded_check`;
- **el seco:**
  - `dry-pbt4a.txt`;
  - `contract-dry-pbt4a.json` (`3a3a1687…`, 64 203 bytes, todo ASCII);
  - `neg-pbt4a.txt` (33 de 33);
- **el esqueleto:**
  - `pbt4a-skeleton.patch` (`25771775…`);
  - `make_pbt4a_skeleton-out.txt`;
  - `neg-skeleton-pbt4a.txt` (8 de 8);
- **los barridos:** `overload-sweep-pbt4a-7f0d9aa3.txt` y `name-check-pbt4a-7f0d9aa3.txt`;
- **el recorrido:**
  - `pbt4a-guard-walk.tsv`, `-out.txt` y `-flagged.txt`;
  - `walk_guards_pbt4a.py`;
  - `green_pbt4a.py`;
- **las dudas del generador** (`generator-doubts-pbt4a.md`, de la 18 a la 26);
- **los borradores:**
  - `f4b-e49.md` y `f4b-e50.md`;
  - `evidence-277.md` y `evidence-278.md`;
  - `superseding-pbt4a.md`;
- **los dos registros del Owner:** `owner-pbt4-decisions.md` y `owner-pbt4b-retire-list-decisions.md`;
- **el complemento de la sesión:** `hs65\pbt4acc\compile\` (`compile-check.md`, los guiones, `out-run*.txt`, `trx-outcomes.txt` y los registros de `out*\`).

**Cómo se comprobó:**
- **El repositorio,** solo por objetos git en `7f0d9aa3`, desde el checkout principal (`git show`, `git grep`, `git ls-tree` y `git cat-file --batch`).
- **Nada del worktree,** sin compilar y sin ejecutar pruebas.
- **Los guiones nuevos,** escritos con Write y corridos con `python -I`:
  - `cat4a_dump.py` y `cat4a_compare.py`: el contrato frente al paquete y al de PB-T3c;
  - `cat4a_tests_forbidden.py`;
  - `cat4a_walk_gap.py`, `cat4a_walk_extra.py` y `cat4a_added_lines.py`: lo que el recorrido no selecciona, frente al GREEN literal.

  Sus salidas, `cat4a_*-out.txt`, están en la misma carpeta.

## §2. Hallazgos

| Id | Grav. | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| **I-01** | I | **El texto literal de `Draft/PushBackOriginByText.cs`**: D-PBT4-03, v4.2:365 y :382, y la viñeta del esqueleto de §6.1 | **`public static string? Choose(…)` y `pair => (string?)pair.Value`, en un archivo sin contexto de anotaciones `#nullable`.**<br>• **Sin contexto posible:** `Directory.Build.props` fija `<Nullable>disable</Nullable>`, y `RackCad.Application.csproj` no lo cambia. La pureza de `Draft/` (`PushBackDraftWriteGuardTests.cs:293-297`, CT-PBT3A-14, copiada en CT-PBT4A-08) prohíbe la directiva `#nullable`. Ningún archivo de `src/` sin `#nullable` usa `string?` ni `object?`: ninguno de `Draft/`, comprobado con `git grep`.<br>• **El aviso:** el compilador da CS8632.<br>• **Ya pasa en el esqueleto:** los registros de la sesión lo muestran, `PushBackOriginByText.cs(16,29): warning CS8632` en `out/plugin.log`, `out/core-tests.log` y `out/ui-tests.log`.<br>• **En el GREEN serán dos:** la firma y el molde de `AtKey`.<br>• **Por qué rompe la CI:** `build-plugin` corre `eng/ci/verify-autocad-references.ps1`, y `Assert-DotNetSuccess -NoWarnings` (:110-113, :596-601) lanza ante cualquier `warning CS\d+` del Plugin publicado y de la solución en Release. El GREEN no daría la CI 4/4 (`[4]`, `[10]`), ni «el build Release… sin avisos nuevos» (`[5]`), sin apartarse del literal (INV-PBT4A-03). Sería C06/C07: un intento gastado, o una vuelta.<br>• **La justificación del molde no vale:** la v4.2:401 lo justifica con «un aviso de nulabilidad nuevo en el Plugin», pero el archivo es de Application y su contexto es «oblivious»: pasar un `Dictionary<string, string>` a `IReadOnlyDictionary<string, string?>` no da ningún aviso | **En el paquete (v4.3), corrección de texto:**<br>• la firma pasa a `public static string Choose(…)`, en D-PBT4-03 y en la viñeta del esqueleto;<br>• el molde pasa a `pair => pair.Value`;<br>• se corrige la frase de :401;<br>• §5.0 gana una regla: ningún archivo nuevo (de `src/` o de `tests/`) usa `?` en un tipo de referencia sin `#nullable enable`, porque `build-plugin` compila la solución en Release y falla con un aviso.<br>**Después:**<br>• se regeneran `green_pbt4a.py`, el esqueleto (con un sha256 nuevo), el seco y el recorrido y las aserciones traducidas;<br>• se repite la precondición 6 con un control de avisos: cero `warning CS` en los tres registros (los `MSB3277` son de las referencias locales de AutoCAD);<br>• se corrige `COMPILE_RESULT` (§6) y la fila V correspondiente lleva «**CA-T4a I-01**» |
| M-01 | M | §4.1, §5.1, §9 y PBT4A-C16, y F4B-E50 | **El recorrido no es «entero», como dicen el paquete y F4B-E50.**<br>• **Por qué:** `walk_guards_pbt4a.py` selecciona por las marcas `MARKERS` (:54-55), que no incluyen `src`, `src/RackCad.Application` ni `src/RackCad.Plugin`. Tampoco ven una ruta compuesta con `Path.Combine(…, "src", "RackCad.Application", "Workspace")`.<br>• **Lo que queda fuera:** entre otras, `PushBackSharedReferenceGuardTests` (CT-PBT1B-14 barre todo `Application/Workspace/`, y está en los `RequiredTests`, n.º 7), `WorkspaceModelBoundaryTests` (barre `Application/Workspace/` y nombra por reflexión los tipos y miembros de su espacio de nombres), `CustomPropertiesEnvelopeGuardTests` y `CustomPropertiesEdgeGuardTests` (todo `src/`), `NamespaceFolderGuardTests`, `ElevationOverrideSourceGuardTests`, `RackDefinitionCreatorGuardTests` (todo Application) y las de todo el Plugin (`PushBackRoundTripSourceGuardTests`, `RackUnitsGuardSourceTests`, `I55G15Auth15ConsumerTests`, `CantileverPluginSourceGuardTests` y `MainMenuStructuralSectionAccessGuardTests`).<br>• **Por qué C16 no lo cubre:** su texto describe lo no visto como «sin dejar en su código ninguna ruta, carpeta ni ayudante», y estas sí dejan carpeta.<br>• **La lista de reflexión** de §4.1 no tiene `WorkspaceModelBoundaryTests`.<br>• **Sin efecto en el resultado:** esta comprobación recorrió los 97 archivos de fuera que nombran una carpeta de los archivos tocados (§5), y ninguno cae | **Corrección de texto:**<br>• el criterio de selección, tal como es;<br>• las clases que CA-T4a añadió y su resultado (`cat4a_walk_gap-out.txt` y `cat4a_added_lines-all-out.txt`, a custodiar);<br>• `WorkspaceModelBoundaryTests` en la lista de reflexión;<br>• la frase de C16, en §4.1, §5.1 y §9, que nombra el riesgo de verdad: una lectura que solo se ve al ejecutar;<br>• en F4B-E50, «46 clases» más las de CA-T4a.<br>**Opcional y recomendado:** compilar el `src/` del GREEN literal sobre la copia exportada y correr la suite completa de Core, con CT-PBT3A-12 saltada. Es comprobar el texto entregado al Worker (INV-PBT4A-03), no una implementación oculta, y cerraría D = 0 en el GREEN con una medida |
| M-02 | M | El contrato, `Authorities[1]` (el generador, :694, fijo en el código) | `docs/initiatives/I-64-f4b-pbt4-measurements.md`: «el registro de mediciones de PB-T4 (R-0 a R-33)». El registro de la v4.2 llega a **R-41**, y R-37, R-39 y R-40 son de PB-T4a (las guardas sobre el GREEN, B-08 y el recorrido) | «(R-0 a R-41)», o el último R medido al emitir |
| M-03 | M | La precondición 6 (v4.2 §11, 6, viñeta 2), frente a `compile-check.md` | La precondición pide que «la suite completa de Core **y los filtros de UI** dan lo previsto».<br>• **Lo que dice la sesión:** que la suite de UI «se compila pero no se ejecuta, por la regla vigente».<br>• **Lo que dice la regla** (INV-PBT4A-10): prohíbe en local la suite completa de UI, no sus filtros.<br>• **Lo que queda sin correr:** los 12 filtros de UI de los `RequiredTests` (n.º 8, 15, 20, 21, 23, 25 a 27 y 35 a 38). WEI, que cambia en el esqueleto, la referencia la UI | Correr esos 12 filtros sobre la copia, o declarar en `compile-check.md` y en `COMPILE_RESULT` que no se corrieron. En ese caso los cubre el trabajo `ui-tests` de la CI del RED (`[2]`) |
| M-04 | M | `f4b-e50.md`, «Los controles» | «La primera entrada de la `ExpectedEvidence` es, byte a byte, la de PB-T3». Es la del contrato de **PB-T3c**, y está en `F4B_EXACT_EVIDENCES` del sustituto v2. Comprobado: `EVID[0]` del seco == `EVID[0]` de `F4B-PBT3C-FULL-WRITE-ERASE/coordinator/gate-contract.json` en `7f0d9aa3` | «…byte a byte, la del contrato de PB-T3c (F4B_EXACT_EVIDENCES del sustituto v2)» |

## §3. La fidelidad del contrato en seco frente a la v4.2 (punto 1)

Medida con `cat4a_compare.py` (`cat4a_compare-out.txt`), frente al bloque ASCII de §6.1.1 y frente al contrato de PB-T3c en `7f0d9aa3` (`cat4a_pbt3c-contract.json`, exportado con `git show`):

| Campo | Resultado |
|---|---|
| `TaskId` | `F4B-PBT4-FULL-READ-PATH`, 23 caracteres; igual al bloque |
| `Objective` | igual, byte a byte; cita la v4.2 |
| **Alcance** | 10 rutas, en el orden de §6.1. `UNIT_CHANGE`: WEI, WHC, RIA y NH, y nada más (las cuatro autoridades con clase `UNIT_CHANGE`) |
| **`ForbiddenWriteScope` (316)** | **Recalculado entero:** `src/` (190) por la expansión de [PKC-C], que despliega cada carpeta que contiene una ruta permitida y lista el resto, sobre `git ls-tree -r 7f0d9aa3`: el mismo conjunto y el mismo orden.<br>`tests/` (109): la lista de PB-T3c, menos `PushBackDraftWriteGuardTests.cs` (ahora permitido), más `PushBackViewEraseGuardTests.cs` y `PushBackWritePathPluginGuardTests.cs` (permitidos en PB-T3c); el orden común se conserva, y ninguna entrada falta en la revisión.<br>La raíz (17): las entradas de la raíz, menos `src` y `tests`.<br>Ningún prefijo prohibido cubre una ruta permitida |
| **Invariantes (14)** | INV-PBT4A-01 a 14, iguales al bloque. INV-04 y 13 llevan `ReadAt` |
| **Paradas (34)** | Las 17 estándar, iguales a las de PB-T3c, y las 17 propias, C01 a C18 sin C15, iguales al bloque.<br>**C11** == PBT3C-C11 con `PBT3C` → `PBT4` y «PB-T3c» → «PB-T4a», y acaba en «no one-off acceptance extends to PB-T4a»; los sha256 de nc1 (`41440edc…`) y de nc2 v2 (`c033e96c…`) son los de F4B-E40 y F4B-E43.<br>C08, C12, C17 y C18 son iguales, literales, a los de PB-T3c.<br>**C13** cierra la lista con `Choose`, `AtKey` y `ReadAt` |
| **`RequiredTests` (38)** | Las dos clases nuevas (`PushBackFullReadPathTests` 5 con `ExpectRed` true, y `PushBackFullReadGuardTests` 3 con false), y después los 36 de PB-T3c en su orden. Solo cambian dos cosas: `PushBackViewEraseGuardTests` pasa de `ExpectRed` true a false, y **`PushBackDraftWriteGuardTests` pasa de 3 a 2**. La tabla de §6.1.1 coincide fila a fila |
| **Evidencia (15)** | `[0]` a `[14]`, iguales al bloque. **`[0]`** es, byte a byte, la de PB-T3c, y está en `F4B_EXACT_EVIDENCES` (el generador lo afirma, `make_pbt4a_contract.py:664`). **`[14]`:** la base 19 129 = 19 123 + 6, el RED 19 137 = 19 125 + 5 + 7, el GREEN 19 137 = 19 130 + 0 + 7, la UI 2 451 = 2 430 + 21 y una sola rama, sin ninguna alternativa |
| **Autoridades (47)** | 38 `UNIT_DOC`, 4 `UNIT_CHANGE` y 5 `EXTERNAL`, sin repetidas. Están los dos registros del Owner (`[2]` y `[3]`, con F4B-E49).<br>Las 7 rutas que aún no están en `7f0d9aa3` son las que custodia la emisión (los avisos del seco) y el propio contrato.<br>`f4b-dispositions.md` dice «F4B-E01 a F4B-E48» porque `DISP_LAST` sale del archivo leído; en la emisión dirá E50, como E47 en PB-T3c.<br>Hallazgo: M-02 |
| **Celdas, `RoutingEnforcement`, `CorrectionsAuthorized`, `Schema`, `Unit`, `Gate` y `MainSha`** | Iguales a los de PB-T3c |
| **Marcadores** | `IssuedBy` lleva `<resultado por rellenar>` en el lugar de `BOUNDED_RESULT`. `IssuedUtc` es la fecha del seco. `AuthorityRevision` es `7f0d9aa3` en el seco, y en la emisión será X |

## §4. B-08 (punto 2)

**Resuelto:**
- **`ReadAt` está en `Draft/`** (v4.2:387-394), y `AtOrigin` (v4.2:412-417) solo la llama.
- **En todo RIA quedan** un `PushBackPilotIntentReader.Read(`, un `PushBackFullDraftReader.Read(` y un `new PushBackPilotIntentScan(`, los tres en `Read`.
- **EIG:350-364 da verde** sobre el GREEN: :357 cuenta 1, y el orden de :362 sigue (`measure_pbt4_v42-out.txt`). NH sigue sin ningún `PushBackPilot` (EIG:465-470).

**Las APIs que usa `ReadAt` existen en `7f0d9aa3`, con esas firmas:**
- `PushBackPilotIntentReader.BuildStaleBase(PushBackPilotIntentScan)`, en :64;
- `Read(PushBackPilotIntentScan, RackCatalog?)`, en :75;
- `PushBackFullDraftReader.Read(PushBackPilotIntentScan, RackCatalog)`, en :51;
- `DraftStaleBase.Entries`, un `IReadOnlyDictionary<string, DraftStaleBaseEntry>` con `Key`, `Membership` y `EnvelopeText`;
- `DraftMembership.Mutable`;
- el constructor del escaneo, `(rackId, originKey, facts, rawEnvelopes, completenessEvidence, previewWitness)`, que copia los sobres.

**Los textos del GREEN que usa el recorrido son los del paquete:** `green_pbt4a.py` se comparó con los bloques de código de D-PBT4-02 y 03 (`cat4a_walk_extra-out.txt`). DRAFT, IFACE, `AtOrigin`, el método nuevo de NH, el constructor y las propiedades de RIA, WEI y la sentencia de WHC (×5) coinciden.

**Las CT afectadas:**
- **CT-PBT4A-04** lleva las líneas `readat otro-origen…`, `readat mismo-origen=null` y `readat sin-coincidencia=null`.
- **CT-PBT4A-07** lleva:
  - `ria-lectores piloto=1 completo=1 escaneos=1`;
  - `ria-AtOrigin readat=1 piloto=0 completo=0 base=0 lecturas-del-dibujo=0 literales=0`;
  - `autoprueba-segunda-lectura-del-piloto-en-RIA=falla`.
- **CT-PBT4A-08:** la pureza admite los dos lectores, como en `PushBackFullDraftReader.cs`.
- **Los dos `[Fact]` y los totales no cambian.**
- **Las líneas que cambian son las de RR-3** (el seco: 6, 33 y 2).

**I-01 no toca estas CT.** La pureza de CT-08 no mira las anotaciones `?`.

## §5. I-25: el recorrido de 46 clases y 1 026 pruebas (punto 3)

**Correcto en lo que selecciona:**
- el método por diferencia;
- los 764 / 162 / 100;
- la única que cae, CT-PBT3A-12, la de la lista.

**Releí las señaladas que cuentan sobre un archivo entero o un cuerpo que cambia.** Todas siguen en verde, por los motivos que da el paquete:
- EIG:350-364, :511-523 (`NamesThePilot` solo sobre `Application/Workspace/`), :582-596, :870-894 y :1011-1024;
- CAG:441-465;
- IG:632-647 (G-6, `WriteTerms` en las dos carpetas del Workspace: ni NH ni la interfaz suman ningún término) e IG:702-719 (G-9);
- WLG:869-885 y PEG:1104-1122.

**Los 15 literales de `ReadAt` del barrido de nombres:** los leí uno a uno, y ninguno alcanza RIA ni `Draft/`.
- WBOSG:95 mira `SubjectOf` del puente.
- WISG:65, las líneas de los manejadores del puente.
- IG:1045, el bloque de `CommandWillStart` del puente.
- CAG:415-451, los cuerpos `Read` de RIA y `ReadOnActiveDocument` de NH, por su firma entera.
- WBST:492 hace reflexión sobre `BrowserRackSummaries`.
- CPEG:566 elige por `member.Name == "Read"` en el acceso físico de propiedades.

**No es completo (M-01).** Las marcas no ven:
- ni las rutas compuestas con `Path.Combine`;
- ni los barridos de todo `src/`, de todo Application o de todo el Plugin.

**Lo medí aparte:**
- **`cat4a_walk_gap.py`:** 146 archivos de prueba fuera del recorrido nombran `src/RackCad.(Application|Plugin)`, `"src"` o un ayudante de barrido. 97 nombran como literal una carpeta antecesora de un archivo tocado o nuevo.
- **`cat4a_added_lines.py`:** cada literal de esos 97 (de 4 letras o más, o con metacaracteres) se probó contra las 134 líneas que el GREEN añade o cambia en WHC, WEI, NH y RIA, y contra las de los dos archivos nuevos.
  - **Lo que casa son fichas genéricas:** `Systems`, `RackCad.Application`, `PushBack`, `name` y otras, en barridos acotados a otros archivos.
  - **También hay falsos positivos de la herramienta:** un literal que, como expresión regular, es una clase de caracteres, como `[CommandMethod("RACKEDITAR")]`.
- **Las que sí leen los archivos tocados las leí:**
  - `PushBackSharedReferenceGuardTests` CT-14: las reglas de G-L1 solo tocan `WorkingBaseline`, `LastModuleReconciliation` y `Structure.Modules`, que el GREEN no nombra. CT-15 sigue: el GREEN no añade ninguna asignación `.Name =`.
  - `WorkspaceModelBoundaryTests`: ningún término de persistencia, de AutoCAD ni de la UI en los archivos del Workspace, y por reflexión ningún tipo ni miembro nuevo casa con `Subscription|Editor|Draft`, `Tab` o `ProjectSummary|Metric|Provider|…`. Los miembros nuevos son `IWorkspaceEditIntentOriginReader`, `ReadIntentAtOrigin`, `FullRead`, `Catalog` y `FromReads`.
  - `CustomPropertiesEnvelopeGuardTests` y `CustomPropertiesEdgeGuardTests`: sus reglas eligen los archivos de propiedades y de variables por nombre.
  - `RackDefinitionCreatorGuardTests`: ningún «RackDefinitionCreator» en Application.
  - `NamespaceFolderGuardTests`: los dos archivos nuevos tienen el espacio de nombres de su carpeta, en bloque.
  - `StructuralSectionPluginSourceGuardTests` y `SelectiveDuplicationFailClosedTests` leen archivos fijos.
  - **Ninguna cae.**

**El riesgo que queda** es una lectura que solo se ve al ejecutar. C16 lo cubre, pero su texto debe decirlo así (M-01).

## §6. El esqueleto y la compilación de comprobación (punto 4 y el complemento)

**El esqueleto es el de §6.1:**
- **WEI:** el constructor con los dos argumentos, `FullRead` y `Catalog`, `FromReads` inerte y las cuatro fábricas con `null, null`.
- **La interfaz nueva,** completa.
- **`PushBackOriginByText`,** inerte: `Choose => null;`, `AtKey => scan;` y `ReadAt => null;`.
- **WHC, RIA y NH,** sin cambio.
- **El `Skip`,** ASCII, es la única línea de `PushBackDraftWriteGuardTests.cs` que cambia (:60, con la sangría de 8 del archivo).
- **`neg-skeleton-pbt4a.txt`:** 8 de 8.

**Lo único que falla es I-01.** La firma literal `string? Choose` da CS8632 ya en el RED.

**Nada que lean las guardas:**
- la suite completa de Core sobre la copia da lo previsto, así que ninguna guarda existente cae con el esqueleto;
- CT-PBT3A-12 pasa sobre el esqueleto sin el `Skip`, porque RIA no cambia.

**La compilación de la sesión** (`hs65\pbt4acc\compile\`), frente a sus registros:
- **Cuadra:**
  - el parche aplica limpio, con 4 archivos y 2 nuevos;
  - el Plugin en Release, Core y UI dan rc 0 y 0 errores (`out-run1.txt`);
  - los dos tipos nuevos están en `RackCad.Application.dll`;
  - la suite completa de Core: **19 129 filas = 19 122 `Passed` + 7 `NotExecuted`** (`trx-outcomes.txt`). Las 7 son S-1 a S-6 de D-1, con su motivo de PB-T3b (S-2 es la `[Theory]` de cinco filas, que la TRX cuenta como una), y CT-PBT3A-12, con el motivo literal del parche;
  - el checkout principal queda sin cambios.
- **No cuadra:**
  - **Los avisos.** `out/plugin.log`, `out/core-tests.log` y `out/ui-tests.log` traen `PushBackOriginByText.cs(16,29): warning CS8632`; el Plugin cierra con «3 Advertencia(s)», que son 2 `MSB3277` locales y CS8632. `compile-check.md` dice solo «0 errores» y deja «los avisos por código» para el Worker. Es I-01.
  - **La UI.** Sus filtros no se corrieron (M-03).

**¿Cumple la precondición 6 de la v4.2?** En lo que mide, sí:
- compila;
- el Core da 19 129 = 19 122 + 7;
- los barridos están hechos.

Le faltan los filtros de UI (M-03). Y con I-01 hay que repetirla sobre el esqueleto corregido, con el control de avisos.

**¿Basta el control negativo corregido?** Sí, para lo que tiene que probar: que la guarda lee el texto de la copia, que el GREEN la rompería y que el `Skip` la neutraliza.
- **Lo que hace:** sin el `Skip`, añade a RIA de la copia una línea, un comentario, con `PushBackFullDraftReader`. CT-PBT3A-12 lee el texto crudo, comentarios incluidos.
- **Lo que da:** falla 1 de 1, «Collections differ». Con RIA restaurado byte a byte (el mismo sha256 `13779513…`) pasa 1 de 1, y con el `Skip` sale `NotExecuted`.
- **Lo que no prueba,** ni tiene por qué: que ninguna otra prueba caiga en el GREEN. Eso es del recorrido (§5).
- **El primer control** se declara bien como premisa falsa. Coincide con el paquete («sin el `Skip`, 19 123 + 6»).

**El literal de `COMPILE_RESULT`:** el que propone la sesión no vale, porque calla CS8632. Tras corregir I-01 y repetir la compilación, con el control de avisos y los filtros de UI, propongo:

> Core-tests y UI-tests en Debug y el Plugin en Release (con -o), 0 errores y ningun aviso CS (solo los MSB3277 locales de las referencias de AutoCAD); los tipos de los dos archivos nuevos del esqueleto en el ensamblado de Application; la suite completa de Core sobre la copia exportada de 7f0d9aa3, 19 129 = 19 122 pasadas + 0 fallidas + 7 omitidas (S-1 a S-6 de D-1 y CT-PBT3A-12); los filtros de UI del contrato, en verde; control negativo: sin el Skip y con un tipo del modelo nombrado en RIA, CT-PBT3A-12 falla

Si no se corren los filtros de UI, la cláusula «los filtros de UI del contrato, en verde» cambia por esta: «los filtros de UI no se corrieron en local: los cubre la CI del RED».

## §7. La lista de PB-T4a y el registro declarado (punto 5)

**La lista: 1, CT-PBT3A-12.** Es la única sustitución:
- `pbt4-substitution-lists.tsv` tiene una sola fila `PB-T4a`;
- C02 solo exceptúa ese `Skip`;
- el recorrido no da otra que caiga.

**Está dentro de la cota:** el Owner vio «1 en PB-T4a» (pregunta 4), y la segunda tanda dice «Sin cambios: PB-T4a (lista de 1)».

**La entrada del registro declarado** (`superseding-pbt4a.md`) está bien:
- la autoridad, con el bloque T1B de [OD], la pregunta 4 y el contrato;
- el motivo: RIA nombra `PushBackFullDraftReader`;
- el `Skip` literal, igual al del parche;
- la tabla, con la prueba vieja, CT-PBT4A-06, que hace el mismo censo con RIA admitida, y sus líneas iguales;
- «Ejecución de PB-T4a (se completa al cerrar)», con RED, GREEN, CT-PBT4A-06 y el estado, todo «(al cerrar)».

Cumple «Do not silently weaken the old guard»: la vieja queda con `Skip`, y la nueva conserva sus diez autopruebas.

## §8. Los textos de la emisión (punto 6)

**F4B-E49** es fiel a las dos tandas literales. Lleva:
- las seis respuestas literales y lo que deciden;
- la pregunta 1 de la primera tanda, reconciliada con la segunda («Cuándo se retira lo fija la segunda tanda: en PB-T4d»), que es la duda 22;
- PB-T4d, que vuelve al Owner y no la cubre la emisión condicional;
- la lista de 470 con su desglose (375 y 53, 15 y 27);
- y que PB-T4a y PB-T4c no cambian.

**F4B-E50** es fiel:
- la autoridad, la pregunta 4 con sus condiciones y cuándo se vuelve;
- el cierre de PB-T3c;
- B-08 y `ReadAt`;
- la única sustitución;
- los totales de la rama única;
- C11 y RR.

Hay que corregir tres cosas:
- la frase de la primera evidencia (M-04);
- «46 clases» del recorrido (M-01);
- `{COMPILE_RESULT}`, que no puede llevar el literal de hoy (I-01, §6).

**Las evidencias 277 y 278** son plantillas fieles:
- la 277 custodia los dos registros, la revisión con §13 a §16, el esqueleto, la compilación y CA-T4a;
- la 278 lleva el alcance, el `ForbiddenWriteScope`, las paradas sin C15 y la CI prevista.

Con I-01 cambian los sha256 del esqueleto y del paquete que citan.

## §9. Decisiones nuevas para el Owner (punto 7)

**Ninguna.** I-01 y los cuatro menores son de texto o de medición. No cambian ningún comportamiento ni ninguna lista, y la de PB-T4a sigue siendo CT-PBT3A-12.

## §10. Lo que hay que hacer antes de emitir, y `BOUNDED_RESULT`

1. **El paquete v4.3**, con I-01 y M-01 a M-04 corregidos, y una fila del registro de cambios que lleve «**CA-T4a I-01**» (`bounded_check` la busca).
   - El nombre de versión debe tener **un solo punto decimal**. La expresión de las recomprobaciones de `bounded_check` toma `v4.2.1` como `v4.2`.
   - Se regeneran `green_pbt4a.py`, el recorrido, las aserciones traducidas, el esqueleto y el seco.
   - El `Objective` pasa a citar la v4.3.
2. **La precondición 6, repetida** sobre el esqueleto corregido: cero `warning CS`, Core 19 129 = 19 122 + 7, y los filtros de UI o su declaración (M-03).
3. **Una recomprobación corta de CA-T4a** sobre la v4.3 y el seco nuevo, en una sección de este informe titulada «## §N. Recomprobación de la v4.3».

**El literal de `BOUNDED_RESULT`** que propongo, en ASCII:

> EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; las importantes corregidas en el paquete v4.3 (I-01, la anotacion string? de PushBackOriginByText.cs: aviso CS8632 que rompe build-plugin); sin decisiones nuevas del Owner; remedicion sobre 7f0d9aa3 sin cambios materiales

- **Tras la recomprobación,** se le añade: `; recomprobacion v4.3: EMITIBLE, B 0 / I 0 / M <n>`.
- **`bounded_check` del generador** (`cat4a_bounded.py`, que extrae la función entre sus marcas y la ejecuta; `cat4a_bounded-out.txt`):
  - sobre este informe y la v4.2, el literal da **un solo error**, el esperado: «ningun registro de cambios del paquete corrige I-01 de CA-T4a»;
  - con una fila `| **V43-01** | **CA-T4a I-01** …` en el paquete, da **0 errores**.

## §11. Límites

- **Nada se compiló ni se ejecutó.** I-01 se apoya en los registros de la compilación de la sesión, en `Directory.Build.props`, en la pureza de `Draft/` y en el guion de la CI, todo en `7f0d9aa3`.
- **De las 100 señaladas, no las releí todas una a una.** Releí las que cuentan sobre un archivo entero o sobre un cuerpo que cambia (§5) y acepté las demás por los motivos de grupo de §4.1, que comprobé con sus aserciones traducidas.
- **La ampliación del recorrido (§5) es por literales y por lectura.** Una regla que no se escriba como literal, o una lectura por reflexión de un tipo que no se miró, solo se ve al ejecutar. Por eso se recomienda la suite de Core sobre el GREEN literal (M-01).
- **Los recuentos de la CI** (19 137 = 19 125 + 5 + 7 en el RED, y = 19 130 + 7 en el GREEN) son aritméticos, sobre la base medida y la lista de §5.1. El de la copia, 19 129 = 19 122 + 7, lo midió la sesión.

## §12. Recomprobación de la v4.3

**Veredicto: EMITIBLE.** B/I/M = **0 / 0 / 2**

**I-01 y M-01 a M-04 están corregidos.** La compilación repetida cumple la precondición 6 de la v4.3, sobre el esqueleto y sobre el GREEN literal: 0 errores, 0 avisos CS, Core 19 129 = 19 122 + 7 y los 12 filtros de UI en verde. Los dos menores nuevos son de texto y no piden una v4.4. No hay decisiones nuevas para el Owner.

**Qué se revisó y cómo:**
- **el paquete:** `I-64-f4b-pbt4-gate-contract-package-v43.md` (`4f482279…`), contra la v4.2 línea a línea, con `diff`;
- **el generador** (`98795d91…`);
- **el seco:** `contract-dry-pbt4a.json` (`7ea2e372…`) y `dry-pbt4a.txt`;
- **las negativas:** `neg-pbt4a.txt` (34 de 34) y `neg-skeleton-pbt4a.txt` (10 de 10);
- **el parche:** `pbt4a-skeleton.patch` (`e99c87c6…`);
- **los borradores:** `f4b-e50.md` y `evidence-277.md`;
- **la compilación:** `hs65\pbt4acc\compile\` (`compile-check.md`, `compile_check_v2.sh`, `out-run-skel.txt`, `out-run-green.txt`, `out-skel\` y `out-green\`).

Todo por objetos git en `7f0d9aa3`, sin worktree y sin compilar. Los guiones nuevos:
- `cat4a_dump.py` y `cat4a_compare.py`, sobre el seco nuevo (`cat4a_dump-v43-out.txt` y `cat4a_compare-v43-out.txt`);
- `cat4a_walk_extra.py`, contra los bloques de la v4.3 (`cat4a_walk_extra-v43-out.txt`);
- `cat4a_green_shas.py`;
- `cat4a_bounded_v43.py` (`cat4a_bounded_v43-out.txt`).

### §12.1 I-01 (punto 1)

**Resuelto en el código literal, en el esqueleto y en el GREEN.**
- **El paquete:** D-PBT4-03 (v4.3:368 y :385) dice ahora `public static string Choose(…)` y `pair => pair.Value`. La viñeta del esqueleto de §6.1 fija la firma sin `?`. La justificación falsa de la v4.2:401 está corregida: el archivo es «oblivious», y el paso al `IReadOnlyDictionary<string, string?>` del constructor no da ningún aviso.
- **El parche (`e99c87c6…`):** tiene 6 440 bytes, uno menos que el de la v4.2. Solo cambia la línea de `Choose`, que pasa a `public static string Choose(DraftStaleBase staleBase, string originText) => null;`. WEI (`6d907be4`), la interfaz (`963cc1dd`) y el `Skip` (`7c64862f`) no cambian, y `PushBackOriginByText.cs` inerte pasa a `1d707a9f`.
- **El GREEN de `green_pbt4a.py`:** es igual a los bloques de la v4.3. DRAFT, la interfaz, `AtOrigin`, el método de NH, WEI, el constructor de RIA y la sentencia de WHC, cinco veces, coinciden (`cat4a_walk_extra-v43-out.txt`). Su DRAFT no tiene ningún `?` de tipo de referencia.
- **Los sha256 de los seis archivos que calcula `cat4a_green_shas.py` desde `green_pbt4a.py`** son, uno a uno, los que `apply_green_pbt4a.py` escribió en la copia de la sesión (`out-green/apply-green.txt`), y el conjunto es el mismo, `7d29d396…`.

**La regla del §5.0 está bien medida.**
- **El ajuste:** `Directory.Build.props:28` es `<Nullable>disable</Nullable>`. Ningún `.csproj` del producto, de las pruebas ni de `tools/` lo cambia (`git grep "<Nullable"`); solo lo nombran tres proyectos de sondas bajo `docs/`, y con `disable`.
- **Lo que dice:** «nunca en `Draft/`», y que los `?` de tipos de valor no cuentan. Las dos cosas son correctas.
- **Cómo la comprueban:**
  - `measure_pbt4_v43-out.txt` la mide en verde y su contrafactual, el texto de la v4.2, en rojo;
  - la negativa S09 del esqueleto devuelve el molde `(string?)` y aborta.

### §12.2 M-01 a M-04 (punto 2)

- **M-01:** corregido.
  - **El texto:** §0, §4.1, §5.0, §5.1, §9 y PBT4A-C16 describen el recorrido como es, por marcas, con la ampliación de CA-T4a: 146 y 97 archivos, ninguno cae.
  - **La reflexión:** `WorkspaceModelBoundaryTests` está en la lista de reflexión.
  - **C16:** el contrato lleva la frase nueva, «including one that reads the text in a way only seen when it runs».
  - **F4B-E50:** dice «por marcas … la ampliación de CA-T4a (97 archivos, ninguno cae)».
  - **La recomendación,** compilar el GREEN literal, está hecha (§12.3).
  - **El recorrido, vuelto a correr sobre el GREEN de la v4.3,** da lo mismo: 46 clases y 1 026 pruebas (764 / 162 / 100); `pbt4a-guard-walk-flagged-v43.txt` es igual al de la v4.2.
- **M-02:** corregido. `Authorities[1]` dice «(R-0 a R-43)», sacado del registro.
- **M-03:** corregido. La precondición 6 pide los 12 filtros de UI o su declaración, y cero `warning CS` (V43-04). La sesión corrió los 12.
- **M-04:** corregido. F4B-E50 dice «la del contrato de PB-T3c (`F4B_EXACT_EVIDENCES` del sustituto v2)».

**La fe de erratas de la precondición 8, en F4B-E50: basta, y no pide una v4.4.**
- La precondición 8 de la v4.3 (:1613-1614) se quedó en «las versiones v1 a v4.1» y en «los fragmentos de `v41/` y `v42/`». La custodia real lleva más: de la v1 a la v4.2, con la v4.3 como el paquete, y de `v41/` a `v43/`.
- La precondición es una lista para la sesión. No la lee el Worker y no entra en el contrato. La custodia que manda la comprueba el generador: todo archivo de `hs65/pbt4` va custodiado o en `CUSTODY_SKIP`, o aborta. El texto se queda corto, pero no contradice nada.
- Una v4.4 solo por esto cambiaría el sha256, el `Objective` y el seco, y obligaría a otra recomprobación.

**Recomiendo ampliar la fe de erratas en una frase, con M-06** (§12.5): «y en §4.1, "CA-T4a leyó las 100 señaladas": releyó las que cuentan sobre un archivo entero o sobre un cuerpo que cambia, y aceptó las demás por grupos (CA-T4a, §11)».

### §12.3 La compilación de comprobación (punto 3)

**Cumple la precondición 6 de la v4.3,** también con el GREEN literal. Los registros cuadran con `compile-check.md`:

| | El esqueleto (`out-skel/`) | El GREEN literal (`out-green/`) |
|---|---|---|
| Avisos (`warning CS`) en `plugin.log`, `core-tests.log` y `ui-tests.log` | 0, 0 y 0; solo `MSB3277` en el Plugin (las referencias locales de AutoCAD) | 0, 0 y 0; ídem |
| Errores | 0 en los tres | 0 en los tres |
| Core (`trx-outcomes.txt`) | 19 129 = 19 122 `Passed` + 7 `NotExecuted` | ídem, con las mismas filas: los dos `trx-outcomes.txt` son iguales |
| Las 7 omitidas | S-1 a S-6 de D-1 y CT-PBT3A-12, con el motivo literal | ídem |
| Los 12 filtros de UI (los n.º 8, 15, 20, 21, 23, 25 a 27 y 35 a 38 de los `RequiredTests`) | Todos con rc 0. `Workspace` da 681 = 680 + 1 omitida y `PushBackFullEditorCharacterization` 18 = 16 + 2; los demás, todos pasados | ídem |
| Las TRX | `7364e3b1…` | `3283e0c8…` |

**El GREEN literal cierra lo que el recorrido no podía ver.** Con el `src/` del GREEN de la v4.3 escrito en la copia (`7d29d396…`), ninguna prueba existente cae fuera de la lista: D = 0 en el GREEN, medido. Solo quedan las pruebas nuevas del Worker y una desviación del Worker respecto del literal.

**El control negativo** se hizo sobre la copia de la v4.2 y sigue valiendo para la v4.3:
- lo que pone a prueba es RIA y CT-PBT3A-12, que no cambian;
- el `Skip` es el mismo, byte a byte (`7c64862f` en los dos parches).

**Lo que no cuadra** es M-05, una sola cosa sin efecto en el resultado (§12.5).

### §12.4 El seco `7ea2e372…` (punto 4)

**Es fiel a la v4.3** (`cat4a_compare-v43-out.txt`):
- el `TaskId`, el `Objective`, las 14 invariantes, las 34 paradas y las 15 evidencias son iguales al bloque ASCII de §6.1.1;
- C11 es igual a PBT3C-C11 con el cambio de nombres, y C08, C12, C17 y C18 son literales;
- los 38 filtros coinciden con la tabla;
- el `ForbiddenWriteScope` (316) sigue igual al recalculado;
- las celdas y `MainSha`, como en PB-T3c.

**Frente al seco de la v4.2** (`cat4a_dump-out.txt` frente a `cat4a_dump-v43-out.txt`), solo cambia lo previsto:
- el `Objective` y el `IssuedBy`, que citan la v4.3;
- `Authorities[0]`, con la v4.3 y `4f482279`;
- `Authorities[1]`, con R-0 a R-43 (M-02);
- `Authorities[5]`, «sobre el contrato en seco y la v4.3»;
- **PBT4A-C16**, con la frase de M-01. No estaba en tu lista, pero es la corrección pedida.

El alcance, el `ForbiddenWriteScope`, las demás paradas, los filtros y la evidencia no cambian. El parche no forma parte del contrato. Los 14 avisos del seco son los de la emisión pendiente, entre ellos «falta `BOUNDED_RESULT`».

### §12.5 Hallazgos nuevos

| Id | Grav. | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| M-05 | M | `compile-check.md` §1.2, frente a `out-run-green.txt` | **La fase del GREEN no aplicó el parche de la v4.3.**<br>• **Lo que aplicó:** la parte de `tests/` del parche de la **v4.2** (`25771775…`), con un contrato intermedio (`bead322a…`). La línea de cabecera del registro lo dice; `compile-check.md` no.<br>• **Sin efecto:** solo se aplica el `Skip` (`compile_check_v2.sh:26`, `--include='tests/*'`), que es igual, byte a byte, en los dos parches (`7c64862f`). El `src/` es el GREEN de la v4.3 (`7d29d396…`) | Una frase en §1.2 de `compile-check.md` y en el manifiesto de la custodia |
| M-06 | M | v4.3 §4.1 («Lo que queda») | «**CA-T4a leyó las 100 señaladas** y las de la ampliación». CA-T4a releyó las que cuentan sobre un archivo entero o sobre un cuerpo que cambia, y aceptó las demás por los motivos de grupo (CA-T4a, §11). Ya no pesa: la suite de Core sobre el GREEN literal las ejecuta todas | La frase de la fe de erratas de F4B-E50 (§12.2), y corregir el texto en la versión siguiente |

### §12.6 Decisiones nuevas para el Owner (punto 5)

**Ninguna.** La v4.3 solo corrige texto y medida:
- la lista de PB-T4a sigue en 1 (CT-PBT3A-12);
- los totales de la rama única no cambian;
- el diseño es el de la v4.2.

### §12.7 `BOUNDED_RESULT` y `COMPILE_RESULT`

**`BOUNDED_RESULT`, el final,** en ASCII:

> EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; las importantes corregidas en el paquete v4.3 (I-01, la anotacion string? de PushBackOriginByText.cs: aviso CS8632 que rompe build-plugin); sin decisiones nuevas del Owner; remedicion sobre 7f0d9aa3 sin cambios materiales; recomprobacion v4.3: EMITIBLE, B 0 / I 0 / M 2

`bounded_check` de `make_pbt4a_contract.py` (`98795d91…`), extraída entre sus marcas y corrida sobre este informe y la v4.3, da **0 errores** (`cat4a_bounded_v43.py`, `cat4a_bounded_v43-out.txt`).

**`COMPILE_RESULT`: confirmado tal como lo propones.** Cada cláusula cuadra con los registros de las dos fases:

> Core-tests y UI-tests en Debug y el Plugin en Release (con -o), 0 errores y ningun aviso CS (solo los MSB3277 locales de las referencias de AutoCAD); los tipos de los dos archivos nuevos del esqueleto en el ensamblado de Application; la suite completa de Core sobre la copia exportada de 7f0d9aa3, 19 129 = 19 122 pasadas + 0 fallidas + 7 omitidas (S-1 a S-6 de D-1 y CT-PBT3A-12), tambien con el GREEN literal del paquete; los filtros de UI del contrato, en verde; control negativo: sin el Skip y con un tipo del modelo nombrado en RIA, CT-PBT3A-12 falla

### §12.8 Límites

- **Nada se compiló ni se ejecutó aquí.** Los resultados de la compilación son los registros de la sesión, leídos y contrastados.
- **La v4.3, contra la v4.2:** la comparé con `diff` entera. PB-T4b, PB-T4c y PB-T4d solo cambian en la cita de versión de su `Objective` y en la regla del §5.0, y quedan fuera de esta comprobación.
