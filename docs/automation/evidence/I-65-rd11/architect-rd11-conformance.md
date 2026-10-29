# I-65: revisión de conformidad del Architect sobre el gate correctivo R-D11

- **pwd:** `/d/Documentos/Codex/Calculadora de racks`, el checkout principal del Owner. No lo leí ni lo usé. Todas las lecturas se hicieron con `git -C C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural` o con rutas absolutas de ese worktree.
- **Rango revisado:** `95aae3f1..bf6ef110`. HEAD es `bf6ef110`, el árbol está limpio y `refs/remotes/origin/architecture/interop-analisis-estructural` también apunta a `bf6ef110`.
- **Veredicto: CONFORMANT WITH OBSERVATIONS.** No hay hallazgos REQUIRED; hay 13 OPTIONAL. Uno de ellos, A65-RD11-01, es una decisión que el Owner tiene que tomar de forma explícita.

## Respuestas

**1. Alcance y Freeze.**
- **Producto:** el diff toca exactamente los siete archivos del §2 del contrato del gate (`git diff --name-only`).
  - El RED `42b9dcdf` cambia solo la prueba C# y el harness.
  - El GREEN `6a58c2b1` cambia solo el `.csproj`, `verify-bundle.ps1`, el README y las dos guías.
- **Otros archivos:** solo cambian documentos de la unidad (`I-65-rd11-autoloader-gate.md`, `I-65-foundations-entry.md` y `I-65-interop-analisis-estructural.md`, este último con `max_attempts` de 9 a 11, autorizado en §5) y `docs/automation/`.
- **Blob de la tabla:** es `d088e2a94c83358e340d4d1b3a6ebdfc7b4dd6c2` en los seis SHA, de `95aae3f1` a `bf6ef110`.
- **Rutas que no deben cambiar:** `git diff` sobre `src tools assets eng .github build-bundle.ps1 install-bundle.ps1 test-install-bundle.ps1 deploy/RackCad.bundle .gitattributes docs/adr` devuelve solo `RackCad.Plugin.csproj`. No cambia ningún `.cs` de `src/`.
- **Freeze y ADRs:**
  - El blob del Freeze (`4e6e1feb…`) y el de ADR-0050 (`c18def42…`) son idénticos en la base y en HEAD.
  - La aserción de `G1ExactPathAllowList` (`GuardTests.cs:315`) sigue intacta.
  - ADR-0003 (líneas 74, 119 y 123) solo restringe a cero DLL de Autodesk y cuatro DLL de RackCad; la tabla es un archivo de datos, y la CI y el verificador siguen exigiendo exactamente cuatro DLL.
- **A-n:** no hace falta. El Freeze no menciona el Autoloader ni el bundle.
- **Detalle menor:** además del método sustituido cambia el comentario de sección de la línea 318 de la prueba («not with» → «and with the bundle»). Es coherente con el cambio y no tiene efecto.

**2. Target.**
- `RackCad.Plugin.csproj:69` declara `BundleSectionMapFile` con la fuente única `assets\structural-analysis\ram-elements\section-material-map.json`.
- `:78` hace un `Copy` con `DestinationFolder="$(BundleContents)\structural-analysis\ram-elements"` y `SkipUnchangedFiles="false"`. Conserva el nombre del archivo y copia byte a byte.
- La ruta coincide con la que resuelve el comando: `RackRamElementsCommands.cs:102-106` usa la carpeta de `Assembly.GetExecutingAssembly().Location` más `structural-analysis\ram-elements\section-material-map.json`.
- Con `ModuleName="./Contents/RackCad.Plugin.dll"` (plantilla sin cambios), la ruta es `Contents/structural-analysis/ram-elements/…`, que es también el `Link` del `Content` (línea 27).
- `RemoveDir`/`MakeDir`, `WriteLinesToFile` y los dos `Copy` anteriores no cambian.

**3. Verificador.**
- **Fail-closed:** se mantiene. La nueva rama del allowlist (`verify-bundle.ps1:174-177`) es un `elseif` posterior a los existentes y usa `-ceq` sobre la ruta exacta. Cualquier otra ruta, incluida otra capitalización, y cualquier archivo extra bajo `Contents/structural-analysis/` caen en «Archivo no autorizado».
- **Presencia e identidad:** `Assert-SectionMapMatchesSource` (`:285-301`) exige que exista la fuente, que exista la tabla en el bundle y que ambas tengan el mismo SHA-256. Se invoca en `:352`.
- **Lo que no cambia:** ninguna comprobación ni mensaje previo. Solo cambia el mensaje final de OK, que el contrato permite. El paso de 153 a 158 comprobaciones cuadra: 2 del allowlist para la tabla y 3 de identidad.
- **D6 (tabla con CRLF en la CI):** verifiqué sin intérprete que el blob `d088e2a9` tiene SHA-256 `fc3d9146…db619`, y que el mismo blob con LF→CRLF da `b22f7d9d…2e7d`.
  - **Hecho nuevo:** `git ls-files --eol` en el worktree de la sesión muestra la tabla como `w/lf`, pero los 10 catálogos sin `-text` como `w/crlf`. El LF local es accidental (`core.autocrlf=true`). Cualquier checkout nuevo (la CI, un worktree nuevo para el SHA final o `main`) producirá un bundle con `b22f7d9d`, no con `FC3D…`.
  - **Mi lectura:** el verificador cumple el §3.3 de la orden literalmente: exige identidad exacta con la tabla fuente. Pero esa fuente es la del checkout de compilación, y la identidad con el blob aprobado no queda garantizada.
  - **Clasificación:** es una limitación que el Owner debe conocer y decidir. Lo clasifico como OPTIONAL porque no tiene efecto funcional: el lector y `MappingRowsSha256` no dependen del fin de línea, y el diseño estaba declarado antes de la planificación (RD11-F-08). Ver A65-RD11-01 y 02.

**4. Pruebas.**
- **Cobertura de los invariantes:**
  - `ElTargetDelBundle…` observa RD-INV-01 sobre el XML del proyecto: fuente única, `Copy` con esa fuente, destino igual al `Link` y cuatro DLL. Junto con la guarda C05 del comando, la cadena comando → `Link` → bundle queda fijada estáticamente.
  - Las pruebas del verificador y del harness son de presencia de texto. El comportamiento real lo prueban los 14 casos del harness en el job del Plugin de la CI. Eso es suficiente, con la reserva de A65-RD11-05.
  - Los casos 11-14 del harness exigen código de salida distinto de cero y el motivo concreto.
- **RED:** es honesto. Core falla 2 de 14392, por `Assert.Single` y `Assert.Contains` (`trx-summary.json`), y compila. La tercera prueba nunca estuvo en rojo (A65-RD11-04).
- **AC-RD11-02 no se cumple tal como está escrito:** el job del Plugin quedó `skipped` por `needs: [tests, build-ui]`. La evidencia sustituta es sólida: el RED local del Worker y la reproducción de la sesión con el verificador de la base, que termina con código 1. Pero CD-I65-RG-12 dice que el criterio «se cumple», y eso es inexacto (A65-RD11-03).
- **Separación RED/GREEN:** `git diff RED..GREEN` no toca pruebas, como pide AC-RD11-03.

**5. Documentación.**
- **Lo retirado y lo conservado:**
  - El README (`:47`) y la guía de I-65 (§2 en `:28-31`, §9 y §10 en `:180`, §11) retiran solo la limitación de CD-I65-F4-10. Las viñetas de §11 sobre `NOT_EVALUATED`, peso propio, la diferencia de −0.62 % y el catálogo UNKNOWN, la relación operativa con la sesión y el paso manual siguen idénticas.
  - DIF-01 y A65-F4R-07 no aparecen por ID en la guía, ni en la base ni en HEAD, así que no se pierde nada.
  - La entrada de FOUNDATIONS (`:45`, `:50`) conserva literalmente las demás limitaciones.
- **Validación estructural:** ningún texto presenta la tabla como validación estructural (§11: «Equivalencia de transferencia, no validación estructural»).
- **Afirmaciones desactualizadas:**
  - `despliegue.md:248` (§5) todavía dice que el allowlist «prueba que solo se distribuyen los cuatro DLL de RackCad y catálogos permitidos».
  - `despliegue.md` §2 no lista `structural-analysis\`.
  - Hay comentarios obsoletos fuera del alcance autorizado.
  - `I-65-ready-preparation.md:20` y `:96`, `I-65-ready-final-plan.md:25,31` y F7-OBS-03 son históricos. CD-I65-RG-18 los declara superados, pero la conciliación de READY-01 debe reflejarlo (A65-RD11-06, 07 y 14).

**6. Proceso.**
- **Verificación del Controller:** `EXECUTION_VERIFIED` 14/14 sobre `6a58c2b1`. Coincide con lo que comprobé: 7 archivos, el blob, la separación RED/GREEN y el trailer `Claude Sonnet 5.5` en los dos commits.
- **Controles negativos:**
  - nc4: A3 `fail`.
  - nc1: `Identity`/STOP.
  - nc2: `Scope`/STOP.
  - nc3: `FreeText`/REWORK.
  - Cada uno tiene exactamente un `fail` y cumple su oráculo. Las comparaciones K no tienen hallazgos ajenos.
- **D1:** sin objeción. Es una lectura sin efectos y el escaneo K5 la detectó (`main_checkout_mentions=1`). La frase de la entrega queda corregida por DEV-I65-RG-02 (A65-RD11-11).
- **D5:** sin objeción. Es el falso P-02 conocido de la app del host; los procesos ya habían terminado al releerlos.
- **Detalle menor:** `transcript-facts.json` registra un falso positivo «python» que no está dispuesto (A65-RD11-10).

**7. Comportamiento del comando bajo el Autoloader.**
- **Suficiencia:** la equivalencia estática basta para este gate, porque AC-RD11-08 y la orden del Owner (§4) admiten preparar la prueba de host sin ejecutarla.
- **La cadena estática:**
  - La ruta que resuelve el comando está fijada por la guarda C05.
  - El target pone la tabla en esa ruta y la prueba lo fija.
  - El manifiesto apunta a `./Contents/RackCad.Plugin.dll`.
  - La instalación desechable trae 25 archivos con la ruta presente.
  - El lector da `Valid` con dos filas `verified`.
  - Hay un antecedente: la validación del Owner de I-12 (`HANDOFF.md:4974`) confirmó la autocarga sin NETLOAD, y los catálogos se resuelven por `Assembly.Location`.
- **Lo que no queda probado:** que `RACKRAMPAQUETE` funcione dentro de AutoCAD cargado por el Autoloader. Eso lo prueba HB-01..05.
- **Riesgos para READY:**
  - El oráculo de HB-02 (`FC3D9146…`) daría un fallo falso con un bundle compilado desde un checkout nuevo (A65-RD11-02).
  - `install-bundle.ps1 -SourceBundlePath` no exige la tabla (A65-RD11-08).
  - Faltan AC-RD11-09/10 (A65-RD11-12).
  - Cada SHA cambia los DLL, así que la prueba de host solo sirve sobre el SHA final de READY.

## Hallazgos

| ID | Ubicación | Hallazgo | Sev. | Recomendación |
|---|---|---|---|---|
| A65-RD11-01 | `verify-bundle.ps1:285-301`; `.gitattributes`; CD-I65-RG-13 | La identidad que comprueba el verificador es contra la fuente del mismo checkout, no contra el blob `d088e2a9`. El LF del worktree es accidental (los catálogos sin `-text` allí son `w/crlf`): la CI, un worktree nuevo y `main` producirán un bundle con `b22f7d9d` | OPTIONAL (decisión del Owner) | Presentar este hecho de forma explícita en el resultado del gate. El Owner acepta la lectura de §3.3 o autoriza `-text` para la tabla en una unidad autorizada (no cambia el blob) |
| A65-RD11-02 | `I-65-rd11-autoloader-gate.md:117` (HB-02) | El oráculo fija `FC3D9146…B619`, que solo se obtiene si el checkout de compilación tiene la tabla en LF | OPTIONAL | Antes de pedir la autorización de host o al preparar W-R1: el oráculo debe ser «igual a la fuente del checkout de compilación, y esa fuente igual al blob salvo el fin de línea (con `git ls-files --eol` registrado)», o adoptar antes `-text` |
| A65-RD11-03 | CD-I65-RG-12; contrato AC-RD11-02; invariante «Forma» de `gate-contract.json` | AC-RD11-02 (job del Plugin en `failure` en el RED) es imposible por `needs`. El registro dice «se cumple» | OPTIONAL | En el cierre, registrar «AC-RD11-02 cumplido por sustitución (criterio mal especificado)» y citar las dos reproducciones locales |
| A65-RD11-04 | `test-verify-bundle.ps1:140-142`; `GuardTests` (prueba del harness) | El RED del harness falla en la precondición de «valido»: los cuatro negativos nuevos nunca corrieron contra un verificador débil, y la tercera prueba C# nunca estuvo en rojo | OPTIONAL | Aceptarlo. Más adelante se puede añadir una mutación (verificador sin comparación de hash → «tabla-modificada» debe fallar) |
| A65-RD11-05 | `GuardTests.cs` (pruebas del verificador y del harness) | Son pruebas de presencia de texto: no detectarían una lógica rota que conserve las cadenas. La guarda real es el harness en la CI, que se omite si Core o UI fallan | OPTIONAL | Aceptarlo y declararlo como límite de la cobertura |
| A65-RD11-06 | `despliegue.md:248` (§5); `:38-56` (§2) | §5 sigue diciendo que solo se distribuyen DLL y catálogos; §2 no lista `structural-analysis\` (esto viene de F4) | OPTIONAL | Corregirlo en el cierre o en los documentos de READY (el archivo está dentro del alcance; la restricción a §4 es del contrato) |
| A65-RD11-07 | `build-bundle.ps1:7-8`; `eng/ci/verify-autocad-references.ps1:643-645, 653-655` | Comentarios obsoletos sobre el contenido del bundle, en archivos prohibidos por este gate | OPTIONAL | Corregirlos en una unidad futura autorizada; ahora no |
| A65-RD11-08 | Guía `:180` (§10); `install-bundle.ps1:92-117` | «Reinstalar con `install-bundle.ps1`»: con `-SourceBundlePath` el instalador no exige la tabla; solo `-Build` pasa por el verificador | OPTIONAL | Recomendar `-Build` o un bundle generado con `build-bundle.ps1` |
| A65-RD11-09 | `I-65-foundations-entry.md:50`; guía `:29-30` | «Mismo contenido que el blob» e «idéntica a la fuente aprobada» son imprecisos: el contenido es el mismo salvo el fin de línea, y la fuente es la del checkout | OPTIONAL | Ajustar la redacción según lo que el Owner decida en A65-RD11-01 |
| A65-RD11-10 | `…2be3/transcript-facts.json` | `python_commands` contiene una entrada que es un falso positivo de texto (la entrega dice «No se invocó Python»); K5 da 0. No está dispuesta | OPTIONAL | Anotarla en la custodia |
| A65-RD11-11 | `worker-handoff.json` (Evidence); D1 / DEV-I65-RG-02 | La afirmación «No se leyó… el checkout principal» es falsa por la llamada 57. No tuvo efectos | OPTIONAL | Sin acción: la corrección ya consta en D1 |
| A65-RD11-12 | `state/I-65.yml` (`next_action`) | AC-RD11-09 (Core y UI Full locales y builds Debug) y AC-RD11-10 (CI del SHA de cierre) no están evidenciados en el rango | OPTIONAL | Completarlos antes del registro formal del gate. El reconocimiento en `state` es correcto |
| A65-RD11-13 | `bundle-after-efe7cc06-evidence.txt` | El script `scratchpad/i65/rd11/bundle_evidence.ps1`, que da la lectura `Valid` del lector (AC-RD11-06), no está versionado | OPTIONAL | Declararlo como sonda no versionada, como en A65-F4D-07 |
| A65-RD11-14 | `I-65-ready-preparation.md:20, 96`; `I-65-ready-final-plan.md:25, 31`; F7-OBS-03 | Siguen diciendo que R-D11 está pendiente o que la tabla va fuera del Autoloader. Son históricos y CD-I65-RG-18 los supera | OPTIONAL | En la conciliación de READY-01..09, marcar ese diferimiento como resuelto por R-D11 B y añadir RD11-OBS-01 a las limitaciones |

## No comprobado (habría que ejecutar o consultar fuentes externas)

- Las corridas de CI 40797120699 y 40797712877: no consulté GitHub. Tomé sus resultados y la tabla `b22f7d9d` del inventario de la CI tal como constan en la custodia.
- Que los SHA-256 custodiados de los archivos de evidencia coincidan con sus blobs.
- Que el build y el harness pasen sobre `bf6ef110`; Core y UI Full locales; builds Debug.
- La lectura `Valid` del lector sobre la tabla del bundle, y el contenido de los registros locales (`build-bundle.log`, `harness`, `install-desechable.log`).
- Que `Assembly.Location` apunte a `ApplicationPlugins\RackCad.bundle\Contents` dentro de AutoCAD, y el comportamiento real de `RACKRAMPAQUETE` cargado por el Autoloader (HB-01..05).
- K1/K2 y los procesos de D1 y D5. Los acepto según la custodia; no tengo acceso a las fotografías.
