Revisión adversarial INDEPENDIENTE del código de I-64 F4B-PBT2B-WORKSPACE-MODE-WINDOW, intento 1 (RackCad, plugin de AutoCAD 2025, C#/.NET 8, WPF). No participaste en ella. Busca defectos reales y no te fíes del resumen de la entrega.

**Cómo leer (obligatorio):** otro proceso puede estar trabajando en el worktree de la rama mientras revisas.
- Está PROHIBIDO leer, listar o ejecutar nada en `<worktree-de-I-64>`, y escribir esa ruta en una línea de órdenes.
- **El código y los documentos:** SOLO por objetos git desde el checkout principal, que comparte el almacén. Usa `git -C "<checkout-principal>" show <sha>:<ruta>`, y también `diff`, `log`, `grep <sha>` y `ls-tree`. No leas los archivos de ese checkout en disco.
- **La entrega del Worker** (handoff, TRX, logs y salidas de sus scripts) está COPIADA en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt2bcc\rv\`:
  - `worker-handoff.json`;
  - `tests/`, y `inv/` si existe;
  - `ci-core/` y `ci-ui/`: la CI del GREEN;
  - `red-ci-core/` y `red-ci-ui/`: la CI del RED;
  - `rr/`, `rr-ci-core/` y `rr-ci-ui/`, solo si hubo una reparación del RED: su diff y su CI.

**El objeto:**
- El diff es `git diff 59839c7148fa101c2f95e1f59b36e6b052216680 03b16c88c01ebfd29a642fc6a9324eb76926bcba`, con el RED intermedio `a3b86134227e716825c4fbf72c4efba03b819351`.
- **La reparación del RED (RR, F4B-E35)**, si la hubo: un commit «RED repair» entre `a3b86134227e716825c4fbf72c4efba03b819351` y el GREEN. La entrega lo declara en `Deviations` con una entrada que empieza por `RedRepairSha <sha>`, y `RedSha` sigue siendo `a3b86134227e716825c4fbf72c4efba03b819351`. La regla, RR-1 a RR-7, está en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md`.
- WorkRunId `R20261119T155931Z-1a87`, en `artifacts/orchestration/I-64/F4B-PBT2B-WORKSPACE-MODE-WINDOW/1/`.
- BaseSha = ChainBaseSha = `59839c7148fa101c2f95e1f59b36e6b052216680`, el commit de emisión del contrato (primera delegación). Su `src/` es el del cierre de PB-T2a: W, WC y WX son los de PB-T1c, y el puerto de PB-T2a está implementado.

**Qué es PB-T2b:** la ventana de Push Back en dos modos explícitos.
- **CLASSIC** no cambia de forma observable: los constructores de hoy, los cuatro sitios que la abren, WX, WC, Application y el Plugin.
- **WORKSPACE** se obtiene solo con la factoría tipada `RackPushBackSystemWindow.ForDraft(PushBackDraftEditorLaunch)`:
  - carga una copia del estado del borrador, sin pasar por un diseño;
  - «Aplicar al borrador» aplica al estado de edición con el mismo paso que «Actualizar» (`ApplyEditsToEditingState`, extraído de `RequestDraw`) y devuelve una copia por el puerto de PB-T2a, sin cerrar;
  - «Insertar» y el lote no se muestran, y ninguna ruta llega a la petición clásica;
  - el BOM y «Guardar en biblioteca» siguen, con dos costuras de prueba nuevas;
  - «Cerrar» pregunta si hay cambios sin aplicar (Q-PBT2-1).

El RED es la costura inerte en W (cuatro declaraciones), el archivo del modo y seis clases nuevas. El GREEN toca solo W, en los miembros de INV-PBT2B-03.

**Las autoridades** (en `03b16c88c01ebfd29a642fc6a9324eb76926bcba`):
- **El contrato** `docs/automation/evidence/I-64-pilot/F4B-PBT2B-WORKSPACE-MODE-WINDOW/coordinator/gate-contract.json`:
  - sus invariantes INV-PBT2B-xx, sobre todo 02, 03 (los rangos), 04 (los testigos y las cuentas), 06, 07c, 08, 10 y 14;
  - el AllowedWriteScope (8 rutas) y los RequiredTests;
  - las paradas, en especial C04, C07 (con la excepción de la reparación del RED), C13 (por nombre), C14 a C17 y C18 (una reparación que rompe RR; v2.7, V27-02).
- **El paquete** `docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md` v2.8:
  - el §3 (D-PBT2-04 a D-PBT2-12, con su código literal; D-PBT2-14, D2; D-PBT2-15);
  - el §4;
  - el §5.0 (el régimen, el sufijo `descartes`, el momento de cada lectura, los gestos y las recetas) y el §5.3 (la costura inerte, CT-PBT2B-01 a 10, las líneas de CT-PBT2B-03 y el paso 1);
  - el §6.0 y el §6.2;
  - el §8 (F4B-E28: Q-PBT2-1 a Q-PBT2-4; F4B-E23).
- **La comprobación acotada** `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt2/independent-review-pbt2b-v23-check.md`, con su §10.
- **Las decisiones del Owner,** en `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/`. En particular `owner-pbt2-decisions.md` (F4B-E28), `owner-f4b-dpb07-extensiondata-decisions.md` (F4B-E23) y D2 (`owner-d1-d2-decisions.md`).

**QUÉ REVISAR**, con evidencia `ruta:línea` y `sha`:
1. **Alcance y forma.**
   - El RED es SOLO la costura inerte exacta del §5.3 en W, el archivo del modo y las seis clases. El GREEN, solo W (y el archivo del modo, si cambia).
   - Cada hunk de W cae en un miembro de INV-PBT2B-03, por su rango en `59839c7148fa101c2f95e1f59b36e6b052216680`, o es una inserción entre dos miembros que solo declara uno nuevo.
   - Ninguna declaración nueva fuera de la lista cerrada de C13; el único constructor nuevo es el privado.
   - Se cumplen los demás invariantes del contrato.
2. **El diseño frente al código,** D-PBT2-04 a D-PBT2-12, punto a punto:
   - el modo inmutable, la factoría y el constructor privado, con la carga de D-PBT2-05 en su orden;
   - `ApplyEditsToEditingState`, extraído literal de `RequestDraw`, y `ApplyToDraft`, sin `SetName` ni `SyncName`;
   - «Cerrar» y `PendingWork`; la guarda «sin camino clásico» en `RequestDraw` y en `InsertBatch_Click`;
   - los rótulos, la ocultación y los once literales; las dos costuras en `Bom_Click` y `SaveLibrary_Click`.

   ¿Algún comportamiento de CLASSIC cambia? ¿Algún comportamiento de WORKSPACE cambia sin estar declarado?
   ¿Respetan las decisiones de F4B-E28? En PB-T2b son, sobre todo:
   - «Aplica sin cerrar; Cerrar avisa» (pregunta 1);
   - el rechazo de un diseño bloqueante o inválido al aplicar, como «Actualizar» (pregunta 2);
   - la concordancia del campo de escala (pregunta 3).
3. **D2 y su límite (lo más importante).**
   - ¿Alguna línea muta el rack resuelto o el informe de módulos de un lado, o asigna el `Name` de un sistema?
   - ¿La ventana carga una copia y devuelve al borrador una copia sin alias, fuera de las dos referencias declaradas?
   - ¿CT-PBT2B-05 (el `Vigia`) y CT-PBT2B-09 (G-L1 sobre el archivo del modo, con sus quince autopruebas) aplican las reglas EXACTAS, o se dejan algo?
4. **Cada CT** (CT-PBT2B-01 a 10): ¿fija lo que dice el §5.3? ¿Hay aserciones tautológicas? En especial, las lecciones de F4B-E30 y F4B-E33 del §5.0:
   - ¿cada línea se lee en el momento que dice su CT? Por ejemplo, `bom-es-el-calculado` de CT-07 justo tras «BOM», y `sin-cambio` de CT-04 con `actual` y `c` tomados antes de su «Aplicar»;
   - ¿algún gesto no cambia nada, como un clic sin `SetNumberR` o un valor igual al de la caja, y una línea pasa por eso?
   - ¿las líneas que dependen de los datos (`nombre`, `seleccion`, las ramas de CT-02 y `descartes`) salen de los datos y del paso 1, y no de una corrida?
   - ¿la sonda del modo va antes de cualquier gesto de WORKSPACE?
5. **El RED** (los TRX de `rv/red-ci-*`):
   - ¿fallan por aserción todos los métodos de los tres filtros con ExpectRed, gracias a la sonda?
   - ¿pasan CT-PBT2B-09 y 10?
   - ¿ninguna prueba existente falla?
   - **Si hubo reparación del RED:**
     - ¿cumple RR-1 a RR-6? Es decir: un solo commit «RED repair» con la tarea; solo clases de prueba que añadió el RED, y ni `src/`, ni pruebas existentes, ni fixtures; y el GREEN, después, solo `src/`.
     - ¿Cambia alguna línea esperada, literal u oráculo del §5.3 (RR-3)? Compruébalo con `git diff a3b86134227e716825c4fbf72c4efba03b819351 <RedRepairSha>`. ¿Corrige solo el cuándo o el cómo de una lectura o de un gesto?
     - ¿El criterio del RED se cumple también sobre su CI (`rv/rr-ci-*`)? ¿Lleva el commit «RED repair» el mismo trailer que el RED y el GREEN?
     - ¿Siguen byte a byte en la clase reparada las líneas de la **lista cerrada**? Es la que el relevo midió y registró en `rv/relay-record-work.json` (Notes, `ListaCerradaDeLineasLiterales`): las líneas literales del §5.3 (las de las tres listas de CT-PBT2B-03 y las fijas de CT-PBT2B-01 a 07) que la clase lleva como literal en `a3b86134227e716825c4fbf72c4efba03b819351`. Si una ya no está, es parada (C07); la parada se aplica solo a esa lista. ¿Algún hunk cambia el primer argumento de un `Assert.Equal`, una cadena `campo=valor` u otro literal fuera de la lista (`RR3AyudaMecanica` y `LiteralesCambiados`)? Esos son indicios: júzgalos.
6. **El GREEN y la estabilidad:**
   - la CI 4/4 con los recuentos previstos;
   - ¿hay dependencias de orden, de cultura o de estado estático (el aviso de descarte sustituido, la colección)?
7. **Contención:**
   - ¿alguna ventana mostrada, un modal real, un `MessageBox`, el configurador, «Insertar», la ventana del BOM o el diálogo de guardado reales?
   - ¿alguna escritura en disco fuera de `ArchivoTemporal`, o en el perfil? La disposición del configurador puede cambiar por pruebas existentes (INV-PBT2B-10): es un defecto conocido y anterior, anotado en `docs/ideas-futuras.md`, y no cuenta;
   - ¿corrió el Worker en local algo de UI fuera de los filtros del contrato?
   - ¿se tocó el Plugin, WX, WC o AutoCAD?
8. **Hallazgos para el Owner:** ¿alguna diferencia observable sin declarar, más allá de N-PBT2-01 y los once literales que ya se le informan (§8.4)?

**REGLAS:**
- Solo lectura.
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.
- No ejecutes dotnet ni pruebas.
- No escribas en el repositorio.
- Lo que no puedas verificar, dilo.

**SALIDA:** el informe en español en `<scratchpad>\b491591bc47aa335\98cecaf8-117d-4d62-b418-bd3e08504b6f\scratchpad\hs65\pbt2bcc\independent-review-code.md`, **sin rutas locales absolutas**, con:
- el veredicto: ACEPTABLE, ACEPTABLE CON MENORES o REWORK;
- el recuento B/I/M;
- la tabla de hallazgos;
- la tabla de diseño → código;
- D2;
- los CT;
- los hallazgos OWNER;
- lo no verificado.

Tu respuesta final, en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes, importantes u OWNER.

## Añadido de la sesión al lanzar la revisión (literal, en el mensaje de lanzamiento)

Además del encargo (punto 7, contención): el Worker declara en Deviations de rv/worker-handoff.json que hizo cinco corridas locales de diagnóstico sobre el filtro de CT-PBT2B-04 con una instrumentación temporal de la clase de prueba, revertida cada vez y no commiteada, para diagnosticar el fallo que llevó a la reparación del RED. Juzga si eso cumple el régimen (16.8: no tocar tests/ tras el RED; la regla RR de F4B-E35; la limpieza del árbol) con lo que puedas verificar (árbol de cada commit, TRX copiados, handoff), y dilo en el informe como hallazgo con su gravedad o como conforme.
