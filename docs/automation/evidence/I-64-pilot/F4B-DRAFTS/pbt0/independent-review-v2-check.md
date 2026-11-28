# I-64 F4B — PB-T0, bloque C: comprobación acotada de la v2 del paquete del contrato y del anexo del bloque M

**Revisor:** el mismo de IRC1 (`pbt0-independent-review-v1.md`). Independiente y de solo lectura. **Fecha:** 2026-11-17.
**Veredicto:** **EMITIBLE TRAS CORRECCIONES.**
- De IRC1 quedan **0 importantes sin resolver** y **1 menor parcial** (M-16).
- **Nuevo:**
  - **1 importante, en el anexo del bloque M** (N-I1, Deshacer y `RCDUMP`);
  - **8 menores** (N-01 a N-08).
- **Al Owner no tiene que volver nada.**

**Notación:**
- `C2:n` es una línea de la v2 (`I-64-f4b-pbt0-gate-contract-package.md`, 827 líneas);
- `A:n` es una línea del anexo (`pbt0-block-m-procedure.md`, 70 líneas);
- `<worktree>` es el worktree en `67e28452`.

## 1. Método y límites

- **Qué leí:** la v2 entera, el anexo, la v1 (`_pbt0-package-v1.md`) y [DP] (`owner-f4b-procedure-decisions.md`).
- **Qué comprobé** contra `<worktree>` con `git show`, `git grep`, `git diff` y lectura:
  - las anclas nuevas o cambiadas de la v2: las de [P], H-03, H-12, H-14, H-16, H-18, `BuildComposite`, la Snapshot I, las firmas y las rutas de los sustitutos;
  - la base del anexo: P-3 y la clave del sobre.
- **Límites:** sin compilar, sin ejecutar pruebas y sin AutoCAD. Para el bloque M solo tengo el dato que transmitió la sesión, no sus registros.

## 2. Estado de los hallazgos de IRC1 y de OD-1 a OD-6

| Id | Estado | Evidencia y comprobación |
|---|---|---|
| I-01 | **RESUELTO** | La regla de la excepción vale solo para los métodos nuevos, y en las existentes cuentan solo el cuelgue y el timeout (C2:538-540). El criterio mecánico se aplica solo a los filtros nuevos (C2:607, :613). Lo mismo en PBT0-C10 (C2:733) y en R-03 (C2:776) |
| I-02 | **RESUELTO** | Atribución por análisis declarado, como [T1B]:257, con la suite completa de Core aislada por cada mutante de Application y la de UI solo en la CI (C2:543-549, :649-651, :662). La T-8 recoge la atribución (C2:489) y R-02 da la estimación (C2:775). MU-08 pasa a W:1138 (C2:515): la línea es `AddFront_Click`. MU-07 pasa a CS:320-323 (C2:514). Comprobé que es estrecho de verdad: `LoadComposite(null)` restablece `DefaultTopology` (CS:986) y `LoadComposite(diseño)` lo copia del diseño (CS:995), así que ni la carga ni `LoadNew` lo notan; solo lo nota retirar B. Lo discrimina CT-07, porque `BuildComposite` siempre escribe `DefaultTopology` (CS:952-962). Matices en N-05 y N-08 |
| I-03 | **RESUELTO** | La firma de instancia lleva `MirroredX`, `MirroredY` y las claves de `DynamicParameters` (C2:434), y K4 y K4′ comparan los valores de `DynamicParameters` (C2:441-442). Anclas `:80`, `:83` y `:86` correctas (C2:153-154). La exclusión que introduce K1o es un hallazgo nuevo: N-02 |
| I-04 | **RESUELTO** | H-03 incluye ya las secciones y `SafetyPerPostWindow` (C2:68-80), INV-PBT0-10 lo exige (C2:636-637) y CT-14 a CT-16 lo usan (C2:351-368). Las anclas de los miembros están verificadas: `PushBackBootSection.cs:242`, `:248`, `:252`, `:254` y `:279`; `PushBackDefenseSection.cs:178`, `:182` y `:205`; `PushBackRearTopeSection.cs:143`, `:146` y `:152`; `SelectiveSafetyWindow.cs:1037`, `:1043` y `:1052`. Los tres `Configure()` llaman a su diálogo. Las líneas `RearTopeSection*ForTest` están desplazadas una (N-01) |
| M-01 | **RESUELTO** | La clase (a) es `y == R3(x)` y `ClassOf` ya no se copia (C2:220-223) |
| M-02 | **RESUELTO** | MU-20 sin alternativa, y `PushBackSideAnnotations.cs` fuera de `MutantScope` y del alcance, que queda en 16 rutas (C2:493, :527, :564-568, :603) |
| M-03 | **RESUELTO** | H-12 (C2:116-117). En CT-13, el paso (i) se fija como hallazgo y el paso (ii) añade una familia que sí viaja (C2:344-348). `PROTECTOR_LATERAL_BOTA_H_3_16_18` existe y es de tipo `LATERAL` (`assets/catalogs/seguridad.csv:5`), y `Authorize` solo descarta GUIA, PARRILLA y TOPE. El ancla de C2:117 es imprecisa: N-06 |
| M-04 | **RESUELTO** | Las anclas de [P] apuntan a la copia custodiada (C2:13). Comprobé 23 en `67e28452:docs/initiatives/I-64-f4b-push-back-completo-package.md`, entre ellas `:370`, `:384`, `:387-391`, `:392`, `:460-471`, `:561-605`, `:588-590`, `:592`, `:594-603`, `:598`, `:599`, `:607-672`, `:679-686`, `:688-711` y `:765`. Todas exactas |
| M-05 | **RESUELTO** | CS:294-324 (C2:137), CS:522-544 (C2:139), W:2716-2721 (C2:118), los dos `VerifiedOn` (C2:766) y P-01 con la Snapshot I (C2:712). La Snapshot I está aceptada en `f4-dispositions.md:1973` y existe `F4-ENVIRONMENT/snapshot-I/snapshot-i.json` |
| M-06 | **RESUELTO** | El algoritmo está descrito y el script se custodia (C2:32, :572-575). La lista de [T1B-C] `:175-300` existe |
| M-07 | **RESUELTO** | CT-05 fija solo los seis campos, sobre pb-06 y pb-07 (C2:276-282), e INV-PBT0-18 lo recoge (C2:659) |
| M-08 | **RESUELTO** | `IsEnabled` en la tupla; se pulsa solo si está habilitado; resultado `DESHABILITADO` (C2:244-248) |
| M-09 | **RESUELTO** | G4 en pb-07 (140), G5 y G6 en pb-06 (150 y 18) y G8 = M3 (C2:417-424). Valores comprobados en los fixtures |
| M-10 | **RESUELTO** | Orden ordinal, emparejamiento por índice, K4′ informativa con ε, y «material» como fórmula mecánica (C2:433-444, :668) |
| M-11 | **RESUELTO** | `Ventana()` con `newIdFactory` fijo (C2:215, :638-639), y `RackId` en CT-11 (C2:330) |
| M-12 | **RESUELTO** | INV-PBT0-06 copia el texto de [P]:598, que comprobé literal, con su lectura (C2:203-207, :624-627) |
| M-13 | **RESUELTO** | C2:180-184 y :240 |
| M-14 | **RESUELTO** | `RackModuleEditSessionTests.cs:381-399` (comprobado: `Reconciliation_AModuleWhoseKindChanged_…`) y DA:36 y :50 (C2:108, :188, :208) |
| M-15 | **RESUELTO** | C2:782 |
| M-16 | **PARCIAL** | Los cinco archivos de la v1 llevan ya ruta completa (C2:586-587). Pero C2:585 dice ahora solo «las siete clases de T1B», cuando la v1 las enumeraba (C1:452). **Retroceso:** hay que enumerarlas con su ruta. Están en [T1B-C] `AllowedWriteScope` (`:152-158`) |
| M-17 | **RESUELTO** | C2:170-177 |
| M-18 | **RESUELTO** | `RackFrameProjectStore.DeepCopy` (C2:111, :335) y «la lista recibida sin cambios» en CT-16 (C2:368) |
| M-19 | **RESUELTO** | C2:145, C2:454 y el M-2b del anexo (A:52) |
| M-20 | **RESUELTO** | C2:21-32 |
| M-21 | **RESUELTO** | C2:233 y :621-622 |
| OD-1 | **RESUELTO** | [DP] con F4B-E03 en la precondición y en las autoridades (C2:28-29, :41). Las anclas de [DP] se reverifican tras la custodia: N-07 |
| OD-2 | **RESUELTO** | PBT0-C08 (C2:728-731), INV-PBT0-23 (C2:670) y el registro de la replanificación (C2:681). La segunda vez vuelve al Owner y no al Coordinator, y los demás rechazos también vuelven al Owner, como pedía IRC1 |
| OD-3 | **RESUELTO** | Las firmas literales, los sustitutos por copia y cualquier variación al Owner (C2:734-759). Las rutas de los sustitutos existen en `<worktree>`. Las precisiones están en el §3 (N-03 y N-04) |
| OD-4 | **RESUELTO** | C2:700-705 y :759 (no se copia [T1B-C]:368) |
| OD-5 | **RESUELTO** | C2:774 y :787-791 |
| OD-6 | **RESUELTO** | INV-PBT0-20 (C2:666), INV-PBT0-24 (C2:673) y R-12 (C2:785), y en el anexo la P-0 (A:19), que es simétrica |

## 3. Las firmas de PD-F4-01 y de PD-F4-04

### 3.1 ¿Literales y sin generalizar?

**PD-F4-01 (C2:735-741): sí, literal.**
- Los tres guiones de C2:737-739 coinciden carácter a carácter con `f4-dispositions.md:1104-1106`.
- La frase de su aplicación, «ninguna otra comprobación distinta de la verificación real», está literal en `:1540`.

**PD-F4-04 (C2:744-750): sí, literal.**
- Coincide con `owner-f4t3-authorization.md:257-262`, dentro del rango citado `:255-265`. La única diferencia es que une el salto de línea de «false / SHA».

**Sin generalizar:** C2:758 obliga a comparar con el texto literal, «nunca por el nombre de la comprobación». Las variaciones que conocemos (PD-F4-05, PD-F4-06, nc2 con Ci o Trailer en fail y cualquier fallo de nc3) van al Owner (C2:753-757), como pide [DP]:9.

### 3.2 La duda abierta: nc2 con Contract en fail por `byte-fidelity-check.json`

**Mi lectura:** es **la firma exacta de PD-F4-01**, no una variación.
- **Así nació PD-F4-01.** El primer caso, F4-T2B1 (`f4-dispositions.md:520`), se describe así: el Controller «contrastó la delegación mutada con el contrato y con `byte-fidelity-check.json` y vio que el `AllowedWriteScope` estrechado ya no coincide». La deuda se define en `:562`: «con la lista blanca exacta, un control nc2 puede hacer fallar Contract antes que Scope».
- **El fichero es la evidencia de la cláusula, no otra cláusula.** El contraste con `byte-fidelity-check.json` es el modo en que el Controller *ve* que se incumple la exactitud del alcance. «Contract en fail solo por la cláusula de exactitud» incluye esa evidencia.
- **Así se cerró en T1B.** [T1B-X]:56 la registró «con la firma exacta de PD-F4-01: Contract en fail solo por la cláusula de exactitud de la lista blanca, con la contradicción derivada de `byte-fidelity-check.json` igual que en F4-T4A2», y el Owner cerró T1B con ese sustituto (F4-E59).
- **Lo que sí sería variación:** que Contract alegue **otra** discrepancia además del alcance estrechado (otra colección, otro campo u otro archivo de fidelidad), o que falle cualquier otra comprobación que en la verificación real pasó. El caso de `:1227`, con Ci y Trailer, es de este tipo.

**Recomendación (N-03).** Escribirlo en PBT0-C11 para que no quede como «duda», con lo que se volvería al Owner sin necesidad:

> «Forma parte de la firma que la evidencia de Contract cite el contraste del `AllowedWriteScope` estrechado con el contrato o con `byte-fidelity-check.json` (`f4-dispositions.md:520`, `:562`; [T1B-X]:56). No forma parte que Contract cite cualquier otra discrepancia.»

Esto no generaliza: transcribe el origen y la aplicación que el Owner ya aceptó, como la v2 hizo con `:1540`.

### 3.3 La regla «si hay duda, el Owner» (C2:758)

Va en la dirección correcta: conservadora y fiel a [DP]:9. Pero **no es operativa**, porque no define qué se compara. Dos sesiones podrían decidir distinto. Propuesta (N-04):
- **Qué se compara.** La salida del control frente a la de la verificación real, comprobación por comprobación, con cuatro datos:
  - el conjunto de comprobaciones en fail;
  - la primera en fail;
  - el `Outcome` y la `StopClass`;
  - el texto de la evidencia.
- **Qué diferencias se admiten**, y solo esas:
  - **PD-F4-01:** Scope (sobre la ruta quitada) y Contract (solo el alcance, según el §3.2);
  - **PD-F4-04:** Identity (nombra el SHA falso y el HEAD) y Handoff (primera, solo porque contiene el SHA falso).
- **Una asimetría que hay que resolver.** La aplicación de PD-F4-04 en F4 (`f4-dispositions.md:1656-1660`) admitió también **Ci**, como consumidora del `CurrentSha`. La v2 cita la aplicación de PD-F4-01 (`:1540`) pero no la de PD-F4-04. Hay que elegir y escribirlo:
  - o se transcribe `:1656-1660` como aplicación literal, y Ci en fail por el SHA falso entra en la firma;
  - o se declara que un nc1 con Ci en fail vuelve al Owner.

  Las dos son defendibles. Lo que no vale es dejarlo a la «duda».
- **Quién decide y dónde queda.** La sesión registra la tabla de la comparación en la anotación de `f4b-dispositions.md`. Una diferencia fuera de la lista, o un texto de evidencia que no se pueda asignar, vuelve al Owner.

## 4. Defectos nuevos de la v2 y del anexo

### 4.1 En el anexo: Deshacer y `RCDUMP` (N-I1, IMPORTANTE)

**El dato que observó la sesión.** Tras el «Actualizar» de RACKEDITAR (M-1) y un `RCDUMP`:
- un solo `U` devolvió el rack al estado previo;
- su eco fue «RCDUMP GROUP (Lisp Expression)»;
- los `U` siguientes deshicieron órdenes de arranque.

**Lectura:**
- **AutoCAD registra cada evaluación de LISP en la línea de órdenes como un grupo de Deshacer propio.** Eso incluye `c:RCDUMP`, las llamadas a `rcj` y `rcn`, la expresión `ssget` de selección (A:43) y los `defun`. El eco lo confirma.
- **El `U` deshizo un grupo rotulado RCDUMP y, aun así, devolvió el rack.** Por tanto, los cambios del «Actualizar» no estaban en un grupo propio cerrado antes del de `RCDUMP`. O se fundieron con él, o la frontera del grupo no cae donde el procedimiento supone. Es coherente con MA H-012: RACKEDITAR no declara `CommandFlags` y el Plugin no gestiona el Deshacer.
- **Consecuencia:** el recuento obtenido no es el que tendría un usuario (RACKEDITAR → «Actualizar» → `U` sin nada en medio). Además, puede ser un **hecho de producto** relevante para el «un solo Actualizar lógico» y su Deshacer en PB-T3: un comando posterior absorbería el «Actualizar».

**Por qué es un defecto del anexo.** El anexo prescribe justo esa intercalación:
- M-1 termina en `RCDUMP post` y M-5a sigue con «`U` de uno en uno, con `RCDUMP u<n>` después de cada uno» (A:47-48);
- lo mismo hacen M-5b (A:51), M-3 (A:53, los `U` con `RCDUMP` hasta `r1`) y M-5c (A:56).

Con el `RCDUMP` en medio, cada `U` a partir del segundo deshace primero un grupo de inventario, y el primero mide la absorción, no el «Actualizar» solo. M-5 queda sin respuesta válida.

**Respuesta a la pregunta: sí, el procedimiento debe tratar el `RCDUMP` como ruido de Deshacer.** Es más que ruido, porque ya demostró que absorbe el «Actualizar». Cómo:
1. **Regla general.** Entre la orden medida y sus `U` no se teclea nada: ni `RCDUMP`, ni `rcj`/`rcn`, ni la expresión de selección, ni ninguna otra orden. La selección y el `RCDUMP pre` van **antes** de la orden medida.
2. **Una copia nueva por cada número de `U`.** Para la medición M-5x de una orden:
   - en una copia nueva: `RCDUMP pre` → selección → la orden medida → `U` × k seguidos, transcribiendo el eco de cada `U` → **un solo** `RCDUMP u<k>`;
   - se empieza con k = 1 y se repite en otra copia nueva con k + 1 hasta que el inventario sea igual a `pre`;
   - nunca se sigue deshaciendo más allá de esa igualdad.

   Así se cumple el «comprobado con el inventario después de cada U» de [P] §6.3, un k por copia.
3. **Copias.** Se añaden a la P-5 copias propias para Deshacer (por ejemplo, `M1u-k`, `M2u-k`, `M3u-k` y `M4u-k`), con su SHA-256 en `copias.json`. Las de M-1 a M-4 siguen para los inventarios `post`.
4. **Dos controles para explicar lo observado.** En copias nuevas:
   - **(a)** RACKEDITAR → «Actualizar» → `U` → `RCDUMP`. ¿Basta un `U`? ¿Qué eco tiene?
   - **(b)** RACKEDITAR → «Actualizar» → una orden nativa inocua, como `REGEN` → `U` → `RCDUMP`. ¿La orden siguiente absorbe el «Actualizar»?

   Cada eco se transcribe literal en `run-log-m.md`.
5. **El dato ya tomado no se descarta.** Se registra como **MEDIDO, «con `RCDUMP` intercalado»**: es evidencia de la absorción, no la respuesta de M-5. No cuenta como error del operador ni consume las dos repeticiones de §6.5, porque es un defecto del procedimiento corregido por escrito.
6. **El resultado se lleva a MA §11 y al análisis del bloque M.** Si la absorción se confirma sin `RCDUMP` (control b), es un hecho para el paquete de PB-T3 («un solo Actualizar lógico») y se registra según el último punto del §6.3 de [P]. **No es una decisión del Owner ahora.**

**El arreglo** cambia el texto de M-5a, M-5b, M-5c y del tramo de los `U` de M-3, la P-5 y el §1 (dónde se permite el `RCDUMP`). No toca producto ni el bloque C.

### 4.2 En la v2

| Id | Gravedad | Contenido | Evidencia | Corrección propuesta |
|---|---|---|---|---|
| N-01 | MENOR | `RearTopeSectionForTest` y `RearTopeSectionBForTest` están en W:2972 y W:2975. W:2971 y W:2974 son los comentarios `<summary>` | C2:76; W:2971-2975 | `:2972` y `:2975` |
| N-02 | MENOR | Las claves K1o (mismo multiconjunto y distinto orden) no se emparejan ni entran en «material» (C2:437, :444). Un plan reordenado con cambios de geometría quedaría fuera del dato para OB-06.4: es el mismo tipo de infravaloración de I-03 | C2:437-444 | Que K1o cuente en la fórmula como «no medible → informar», o emparejar dentro de cada grupo de firma igual por orden de aparición y medir K4 también ahí |
| N-03 | MENOR | La duda sobre nc2 con `byte-fidelity-check.json` queda abierta. Con C2:758 llevaría al Owner un caso que es la firma exacta | §3.2 | La frase del §3.2 en PBT0-C11 |
| N-04 | MENOR | «Si hay duda, el Owner» no es operativa. PD-F4-04 sin su aplicación de F4 (Ci) | C2:744-758; `f4-dispositions.md:1656-1660` | La definición del §3.3 y la elección explícita sobre Ci |
| N-05 | MENOR | H-18 funda «ningún mutante de W o WC alcanza a Core» solo en las referencias de proyecto. Dos imprecisiones: (1) `RackCad.Tests` también referencia dos proyectos de `tools/` (`RackCad.Tests.csproj:28`, `:31`); (2) Core tiene guardas de texto que leen W: Fact3 (`PushBackModuleEditorCharacterizationTests.cs:382-400`), I55 (`:8-15`) y G-10. Comprobé que ningún mutante de W las rompe, pero la atribución de T-8 depende de esa afirmación | C2:169, :548 | Escribir «ni por referencia ni por las guardas de texto de Core sobre W (Fact3, I55, G-10), comprobado», y que el Worker atribuya igualmente cualquier fallo de Core en el RED |
| N-06 | MENOR | El ancla de «viajan en la lista» (`PushBackSafetyAuthority.cs:239-255`) es `RestrictToLowEnd`, que no prueba que viajen | C2:117 | Citar `Authorize` (`:94-123`, con el descarte de GUIA, PARRILLA y TOPE) y los dos `Apply*Sections`, que solo reconstruyen BOTA y DEFENSA (W:2742-2780, :2874-2960) |
| N-07 | MENOR | Las anclas de [DP] (`:5-9`, `:11-15`, `:17-21`) son de la copia de trabajo. La custodia de [P] añadió una cabecera de redacción (+4 líneas), y a [DP] le puede pasar lo mismo | C2:41, :728, :734; A:5 | Reverificarlas sobre la copia custodiada en el mismo commit de emisión |
| N-08 | MENOR | La alternativa de MU-07 (WC:119, sin `RequestRecompute()` en `SideBPresent_Changed`) es amplia en UI: alcanza a toda prueba que marque o desmarque la casilla del lado B. La estimación de R-02 no la cuenta | C2:514, :775 | Declarar que, si se usa la alternativa, R-02 se vuelve a estimar y se registra |

**Otras comprobaciones de la v2, sin defecto:**
- el alcance de 16 rutas;
- `MutantScope` de seis archivos, coherente con la tabla;
- `RequiredTests` sin cambio (3 y 18);
- INV-PBT0-15 y -20 sin contradicción entre sí;
- MU-08 en W:1138 discrimina CT-08 y CT-09 y no toca el constructor;
- la Snapshot I y las rutas de los sustitutos existen;
- ninguna decisión nueva sin autoridad.

**En el anexo,** fuera de N-I1:
- P-3 sale vacío de verdad (`git diff 9abc1320 67e28452 -- src assets`);
- `rcj` lee la clave correcta (`RackBlockData.DictKey = "RACKCAD_SELECTIVE"`, `src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:16`);
- las copias y el cierre sin guardar respetan la regla de los originales;
- la redacción de los inventarios antes de la custodia (A:69) cubre la privacidad.

## 5. Veredicto

**EMITIBLE TRAS CORRECCIONES.**

| | Importantes | Menores |
|---|---|---|
| De IRC1, sin resolver | 0 | 1 parcial (M-16) |
| Nuevos | 1 (N-I1, anexo del bloque M) | 8 (N-01 a N-08) |

- **Lo que bloquea:** solo N-I1. No afecta al contrato del bloque C, pero el anexo se custodia en el commit de emisión (C2:32) y M-5 ya está en curso. Hay que corregir M-5a/b/c y M-3 (§4.1) **antes de seguir con las mediciones de Deshacer y antes de custodiar el anexo**.
- **Los menores** (M-16 y N-01 a N-08) son textuales y pueden ir en la misma pasada.
- **Si se aplican como se proponen,** no hace falta otra revisión independiente: basta con que la sesión registre su cierre.
- **Al Owner:** nada. Las tres cuestiones de procedimiento ya las decidió el Owner ([DP]). La lectura de la firma de PD-F4-01 (§3.2) transcribe su origen y la aplicación que el Owner aceptó en T1B, sin generalizar. El efecto de Deshacer que mida el bloque M es un dato para el paquete de PB-T3, no una decisión de ahora.
