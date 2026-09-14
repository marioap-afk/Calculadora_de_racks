# I-52 — Proposal V10: RACKMIRROR, espejo semantico de uno o varios racks (ID16)

> # ⚠ PROPOSAL V10 — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Proposal Version = V10
> Coordinator = REVIEW REQUIRED
> Architect = REVIEW REQUIRED
> Consensus = NOT REACHED
> ADR-0036 = PROPOSED
> O-1 = PENDING
> G3 = NOT OPEN
> Implementation = BLOCKED
> ```
>
> ```text
> Schema            = SIN CAMBIO (ningun DTO, sobre ni Xrecord cambia en I-52)
> BASE de la rama   = 46fcac2b071929d2bd5b07aa28373941417f74a8   (V10 se redacta SIN rebase, sobre deb08cd)
> CITAS             = archivo:linea contra origin/main @ 1b091bedafceb67ca57054a9eb3bf5259efff774 (I-53 E1 integrada).
>                     main esta hoy en dad4e77 (merge de I-53D E3 sobre ba497f1, merge de I-54 sobre 104ef3a, que ya
>                     integraba I-53S E2); las citas siguen ancladas en 1b091be, las que I-53S, I-54 e I-53D
>                     desplazaron se listan en §15.1 y se re-anclan en el rebase posterior al consenso. Las citas de
>                     paralelas y las de main vigente llevan su SHA
> CLAIM_SHA         = 55281769d5e9317ad25500e6d3f5f8a849f37279
> BOOTSTRAP_SHA     = 7ee79756d2b93b4eded660cb73310ef3edde32ff
> Discovery G1      = 339b3abd238a3a7a42d60c9edd45a1500a776f62   (se conserva)
> Proposal V1..V8   = 0fc7032 · 0445718 · 545c222 · 8e2ae4f · e998a2b · 91bdd38 · b70b5bf · e0a779f   (historial)
> Proposal V9       = deb08cdb81817e9e1fafc3500e542a0ac1c2c886   (historial; CI 34797212366 4/4)
> Revision Arq. V9  = «Architect: CHANGES REQUIRED — PROPOSAL V10»; sin BLOCKER ni HIGH; MEDIUM AR9-01 (huella visual
>                     sin marco ni escala para lo que depende de pantalla o de trazado) y AR9-02 (huella resuelta en
>                     otro contexto y sin frescura tras EnsureForPlan); LOW AR9-03..AR9-09; cambios exactos
>                     V10-01..V10-12 de la revision (registro: docs/automation/decisions/I-52.md §56)
> Coordinador       = orden de V10 con V10-01..V10-17 vinculantes (registro §57)
> main y paralelas  = §15.1 (preflight de V10 y re-fetch previo al commit)
> Rebase            = despues del consenso tecnico sobre V10 (§14.1); V10 no rebasea
> Estado de gates   = G0 ACCEPTED · G1 ACCEPTED (Discovery) · G2 V10 EN REVISION · G3 NOT OPEN
> ```
>
> Este documento **no** autoriza produccion, **no** modifica V1..V9 ni el Discovery y **no** es el contrato vinculante.
> El contrato se reescribe en el freeze de G2 (§14.1).
>
> **Nota de transparencia.** V1..V9, sus revisiones de Arquitecto y esta V10 las redacto el mismo agente en roles
> distintos. La evidencia de biblioteca de §1.4 la obtuvieron las revisiones de V5, V6 y V7 con inspecciones de solo
> lectura sobre copias de la biblioteca configurada, y la revision de V8 releyo el volcado de la de V6; V10 la registra
> como **indicativa**, ligada a su SHA-256, y no como contrato. La revision de V9 no inspecciono la biblioteca: midio
> codigo a SHA exacto (drawers, `BlockLibraryImporter`, guardas de I-54 e I-49 y la ventana del Dinamico de I-53D).

---

## 0. Reconciliacion V9 → V10

V1 (`0fc7032`), V2 (`0445718`), V3 (`545c222`), V4 (`8e2ae4f`), V5 (`e998a2b`), V6 (`91bdd38`), V7 (`b70b5bf`), V8
(`e0a779f`), V9 (`deb08cd`) y el Discovery (`339b3ab`) **se conservan sin cambios**. **Donde V9 y V10 difieran, manda
V10.** Las marcas `[V9-D01]`..`[V9-D22]` siguen vigentes salvo donde una marca `[V10-Dnn]` las modifica, reabre o
sustituye (§0.4); las marcas `[V8-Dnn]`, `[V7-Dnn]`, `[V6-Dnn]` y `[V5-Dnn]` siguen como las dejaron V9, V8, V7 y V6
(§0.5, §0.6, §0.7, §0.8).

### 0.1 Estados de reconciliacion

| Estado | Significado |
|---|---|
| **CLOSED BY V10** | V10 fija el contrato; lo que quede es verificacion ordinaria de gate |
| **DEFERRED TO G3/G5 WITH FAIL-CLOSED** | El dato exacto se obtiene en G3 o G5, y mientras tanto existe un fail-closed inequivoco que no condiciona la arquitectura |
| **OPEN MATERIAL** | Impide presentar V10. **Debe ser NONE** |

### 0.2 Matriz `[V10-D01]`..`[V10-D23]`

En la columna «Origen»: `AR9-nn` es el hallazgo de la revision de V9; `R:V10-nn`, su cambio exacto; `C:V10-nn`, la
precision vinculante del Coordinador en la orden de este gate. Las dos series V10-nn se numeran por separado. **Donde
el cambio propuesto por la revision y la precision del Coordinador difieren, manda la precision vinculante del
Coordinador**: para AR9-01 la revision dejaba elegir entre un margen visual absoluto fijado en G4 y excluir las
anchuras de presentacion, y el Coordinador decide que la huella visual representa **ocupacion de modelo en WCS** y no se
dilata por anchuras de pantalla ni de papel (§11.3 punto 5, L-37).

| Marca | Origen | V9 | Correccion V10 | Estado | Donde |
|---|---|---|---|---|---|
| `[V10-D01]` | AR9-01 · R:V10-01 · C:V10-01 | Huella visual como «sobre-aproximacion de todo lo visible» con «trazo expandido con su grosor visible»; regiones en el marco del BTR, de la definicion o de WCS; huella de la copia = huellas de sus instancias «transformadas por `P'`», que con `s ≠ 1` (ST-11) escalaba tambien la dilatacion | **FOOTPRINT SEMANTICS / REFERENCE POLICY** (decision vinculante del Coordinador): `VisualFootprint` representa **ocupacion de MODELO en WCS** y no intenta convertir a WCS efectos puramente de pantalla o de papel. Construccion: (1) aplanar y transformar **primero** hasta WCS —instancia, anidados con su transformacion completa y `P'`—; (2) **despues** construir cualquier margen que tenga dimension real de modelo. Incluye el soporte de modelo de los trazos, el ancho geometrico real de la entidad si existe, rellenos, mascaras con tamaño en modelo, limites de texto y de cota bajo su contexto resuelto, marcos, contenido anidado y cualquier ocupacion expresable de forma conservadora en WCS. **No** se dilata por `LineWeight` de AutoCAD, `LWDISPLAY`, grosores de CTB/STB, milimetros de papel ni escala de trazado: `LineWeight` sigue perteneciendo a `VisualSignature` y a la presentacion, pero no añade area. L-37 nueva en O-1 | CLOSED BY V10 (contrato); ocupaciones reales por clase: DEFERRED TO G3/G5 WITH FAIL-CLOSED | §1.3 L-37; §4.1 ST-19; §11.3 punto 5; CT-36; CT-49; T-M71; G-M23; ADR-0036 decisiones 11..13 |
| `[V10-D02]` | AR9-02 · R:V10-02 · C:V10-02 | La base auxiliar reproducia el contexto de la autoridad origen (la biblioteca para un bloque que solo existe alli) y las huellas visuales se producian en ella; «las capas que el dibujo ya tuviera con otras propiedades no la invalidan» tambien para la huella | **DESTINATION EVALUATION CONTEXT**: `VisualFootprintResult` **no** se resuelve con el contexto de la biblioteca: representa lo que MUTATE materializara en el **dibujo destino**. `DestinationEvaluationContext` (nombre ilustrativo): definicion existente ⇒ la del dibujo actual; definicion faltante ⇒ la base auxiliar simula su importacion al dibujo, con la precedencia unica de `[V10-D09]` (DRAWING WINS). Cubre transitivamente, cuando son relevantes, definiciones de bloque, bloques anidados, capas, tipos de linea, estilos y otros registros de simbolos que alteran la representacion. No reproducible fielmente ⇒ `UNKNOWN`. La regla historica del contexto de la autoridad origen queda limitada a `GeometrySymmetryResult` donde corresponda | CLOSED BY V10 (contrato); fidelidad real de la simulacion: DEFERRED TO G3/G5 WITH FAIL-CLOSED | §9.5 punto 7; §11.3 puntos 3, 4 y 14; CT-36; T-M63; ADR-0036 decision 13 |
| `[V10-D03]` | AR9-02 · R:V10-03 · C:V10-03 | ST-19 en PREPARE antes de `EnsureForPlan` con las huellas de P7; PRE-13 re-verificaba solo las piezas con cambio de mano con `TraceFingerprint`, que compara fuentes simbolicas; nada volvia a evaluar ST-19 ni el orden de plan tras importar | **FOOTPRINT FRESHNESS AFTER ENSUREFORPLAN**: ST-19 antes de importar se conserva solo como **rechazo anticipado**. Tras `EnsureForPlan` y antes de MUTATE: (1) releer el dibujo real; (2) recalcular las entradas de la huella de **toda** instancia cuya huella se uso; (3) volver a producir `VisualFootprintResult`; (4) volver a evaluar el orden visual de plan; (5) volver a evaluar ST-19; (6) solo si todo pasa, continuar a MUTATE. Aplica a piezas con y sin cambio de mano, a las vistas seleccionadas y a toda vista admisible cuya evidencia haya podido cambiar por simbolos importados o colisiones de nombres. `FootprintInputFingerprint` (nombre ilustrativo; **no** es `TraceFingerprint`): identidades de las definiciones reales, registros de simbolos resueltos usados, observaciones consumidas del contexto de materializacion, estado dinamico efectivo, identidades de las dependencias anidadas y cualquier otra entrada que altere la huella. Cambio ⇒ recalcular; sin equivalencia u orden demostrados ⇒ E10 (X-22 en el orden de plan). PRE-18 nueva; la postcondicion se repite al inicio de MUTATE | CLOSED BY V10 | §9.1; §9.2; §9.4; §11.3 puntos 5 y 10; PRE-18; T-M67; T-M69; T-M71; CT-48; CT-49 |
| `[V10-D04]` | AR9-02 · R:V10-04 · C:V10-04 | Huella de anotaciones y cotas «desde el plan»; `MaterializationContext` sin `DIMSTYLE`, estilo de texto, escala de anotacion ni registros de capa; «anotaciones y cotas salen del plan»; ST-19 listado antes de los planes definitivos de las vistas seleccionadas | **ANNOTATIONS / DIMENSIONS**: la intencion sale del plan; la representacion y la huella pueden consumir estado del dibujo. Cuando realmente se consumen, el contexto contempla el `DIMSTYLE` actual y su registro efectivo (`LateralHeaderDrawer.cs:380-392`), el `TEXTSTYLE` efectivo del `DBText` (`:266-277`), la escala de anotacion, los registros de capa `RACKCAD_ANOTACIONES` y `RACKCAD_COTAS` (`LayerHelper.cs:19-22`), los de las capas de rol del Cantilever (`CantileverViewMaterializer.cs:163-197`) y todo otro estado que encuentre CT-50. Los planes definitivos de las vistas seleccionadas, con el nombre destino, existen **antes** de la decision final de ST-19. PREPARE reordenado: `P'` y planes definitivos → huellas en el contexto destino → ST-19 temprano opcional → `EnsureForPlan` → relectura del dibujo real → huellas, orden de plan y ST-19 refrescados → MUTATE | CLOSED BY V10 (contrato); consumo real: DEFERRED TO G3/G5 WITH FAIL-CLOSED | §8.2; §8.7; §9.1; §11.3 punto 5; CT-50; M-37 |
| `[V10-D05]` | AR9-04 · R:V10-06 · C:V10-05 | `MaterializationContext` capturado completo y re-verificado entero: un cambio transparente de estado que ningun productor consume daba E10 | **MATERIALIZATION CONTEXT READ-SET**: `MaterializationContextReadSet` (nombre ilustrativo) contiene **unicamente** las observaciones que el productor o la huella realmente consumen. Ejemplos: productor que fija `Color` explicito ⇒ `CECOLOR` no forma parte de su read-set; cota sin estilo explicito ⇒ `DIMSTYLE` si; `DBText` que usa el estilo actual ⇒ `TEXTSTYLE` si, si CT-50 lo demuestra. Estado consumido y cambiado ⇒ E10 en PREPARE, excepcion y rollback en MUTATE; estado no consumido ⇒ no invalida; consumo que G3 no puede determinar ⇒ `UNKNOWN` / `FAIL_CLOSED` | CLOSED BY V10 (contrato); consumo real por productor: DEFERRED TO G3/G5 WITH FAIL-CLOSED | §8.7; §9.2; PRE-17; T-M72; CT-50; M-37; R-41 |
| `[V10-D06]` | AR9-03 · R:V10-05 · C:V10-06 | «Posterior» = posterior en cualquiera de los contextos de pantalla y de trazado; las copias entre si seguian el orden de sus fuentes sin regla para ordenes contradictorios | **SCREEN VS PLOT ORDER**: CT-49 caracteriza los ordenes efectivos relevantes. Si dos fuentes admitidas tienen orden relativo distinto entre pantalla y trazado y sus **copias** destino se superponen de forma material, o la superposicion no puede clasificarse ⇒ E10: no existe un unico orden de creacion capaz de conservar dos ordenes contradictorios | CLOSED BY V10 | §4.1 ST-19; §1.3 L-34; CT-49; T-M69; M-35 |
| `[V10-D07]` | AR9-03 · R:V10-05 · C:V10-07 | `XLINE`/`RAY` con huella `UNKNOWN` ⇒ E10, sin declararlo en O-1 | **XLINE / RAY**: en el primer corte, si CT-49 no implementa una huella o interseccion analitica fiable para `XLINE` y `RAY`, su `VisualFootprint` es `UNKNOWN`; un objeto posterior `XLINE` o `RAY` con huella `UNKNOWN` ⇒ E10, aunque este lejos de la copia. Declarado expresamente en O-1 (L-34 y L-37); es un fallo conservador deliberado, no un defecto | CLOSED BY V10 | §1.3 L-34 y L-37; §4.1 ST-19; §11.3 punto 5; CT-49; T-M69 |
| `[V10-D08]` | AR9-06 · R:V10-08 · C:V10-08 | T-M16 reapuntaba T-GRD-08 «añadiendo `RACKMIRROR` sin alias» sin decir como pasa una entrada de ayuda sin alias la comprobacion de alias registrados; reapuntado condicionado a que I-54 integrara antes | **HELP / NO ALIAS**: `RACKMIRROR` **tiene** entrada en `RackCommandReference`; **no** tiene alias en V1. T-GRD-08 se reapunta sin debilitar: el `Command` de cada entrada siempre corresponde a un `[CommandMethod]`; el `Alias` solo se verifica si la entrada tiene alias no vacio; el alias ausente es un estado valido y explicito; no se inventa alias. Ayuda: +1 entrada; registros de alias: +0. Censo de comandos `B + 1`, con `B = 35` en `main @ ba497f1` si no cambia antes de la reconciliacion. T-M16 incondicional | CLOSED BY V10 | PDC-8; CT-14; T-M16; §14.2 G7; §15.2; §20 |
| `[V10-D09]` | AR9-07 · R:V10-09 · C:V10-09 | Dos rutas de clonado autorizadas (biblioteca y dibujo) sin precedencia fijada cuando comparten base auxiliar: con `DuplicateRecordCloning.Ignore` ganaba el primer clonado de cada nombre | **SCRATCH PRECEDENCE / NAME COLLISIONS**: el `DestinationEvaluationContext` usa una sola precedencia, **DRAWING FIRST, LIBRARY SECOND WITH IGNORE**; nunca library-first. Rige para todos los nombres relevantes: bloques raiz, definiciones de bloque anidadas, capas, tipos de linea, estilos y otras dependencias de representacion. Una colision que no puede simularse como la resolvera MUTATE ⇒ `UNKNOWN`. CT-36 y T-M63 incluyen un caso explicito de colision de nombre de bloque anidado y otro de nombre de simbolo. G-M20 sigue prohibiendo solo el acceso o la reflexion directos sobre `cachedLibrary` y su mutacion | CLOSED BY V10 (contrato); simulacion real: DEFERRED TO G3/G5 WITH FAIL-CLOSED | §9.5 punto 7; §11.3 puntos 4 y 14; CT-36; T-M63; G-M20 |
| `[V10-D10]` | C:V10-10 | Metodo de inventario cerrado de V9 (`[V9-D08]`) | **CLOSED INVENTORY / CONTEXT**: se mantiene el metodo de V9. Material, sombras, estilos visuales y cualquier otra propiedad descubierta quedan `TRANSPORTED`, `PLACEMENT`, `COMMON_CONTEXT`, `NON_VISUAL` o `UNSUPPORTED`; nada queda sin clasificar; lo no clasificado ⇒ `UNKNOWN` (contenido evaluado, X-17b) o E4 (referencia fuente, ST-18) | CLOSED BY V10 (metodo); inventario real: DEFERRED TO G3/G5 WITH FAIL-CLOSED | §4.1 ST-18; §11.3 punto 14; CT-36; CT-49 |
| `[V10-D11]` | AR9-08 · R:V10-10 · C:V10-11 | CA-4b citaba «ADR-0039 §3» para la identidad local al alcance y para la ausencia de herencia entre Proyecto y Rack | **CA-4b / ADR-0039 CITATIONS**: CA-4b sigue siendo Custom Properties de alcance **PROYECTO** en el NOD; los alcances Rack y Proyecto son independientes. Citas corregidas: identidad local al alcance → ADR-0039 §1 (D-02.8); sin herencia ni valor por defecto entre Proyecto y Rack → ADR-0039 §12; el contenedor de Proyecto es §3. La regla funcional no cambia | CLOSED BY V10 | §6.4; §10.4; ADR-0036 decision 10 |
| `[V10-D12]` | AR9-05 y AR9-08 · R:V10-07 y R:V10-10 · C:V10-12 | §11.3 punto 1 y el caso C usaban `A = F ∘ Π(D)` y `B = Π(μD)`, al reves que §11.1; L-36 y el contexto y las consecuencias del ADR afirmaban que las referencias internas «toman los valores por defecto» antes de caracterizarlo | **NOTATION / WORDING**: §11.3 punto 1 y el caso C usan exactamente la notacion de §11.1 (`A` = firma del plan reflejado `Π(μ_k D)`; `B` = firma del plan original transformado `F ∘ Π(D)`). Donde se hablaba de valores por defecto se escribe: «los drawers no asignan explicitamente determinadas propiedades; CT-50 debe caracterizar que mecanismo y que valores efectivos aplica AutoCAD y que observaciones consume el productor», en L-36, §8.7 y el contexto y las consecuencias del ADR | CLOSED BY V10 | §1.3 L-36; §8.7; §11.3 puntos 1 y 9; ADR-0036 |
| `[V10-D13]` | AR9-09 · R:V10-11 · C:V10-13 | I-54 como rama paralela sin integrar (G7B `18cf2dc`, G7-CLOSE `02987bd`) y `main @ 104ef3a`; reapuntados de I-54 condicionados a que integrara antes | **CURRENT MAIN / I-54**: `main @ ba497f1` integra I-54 (merge del cierre documental `d3cc843` sobre el Candidato `02987bd`; rama remota retirada). En main estan `RACKPROPIEDADES` y `RPR`, las Custom Properties de Rack y de Proyecto, T-GRD-01..T-GRD-08, el censo de comandos 35, el censo de ayuda de 15 pares, `RackCommandReference` actualizado, CA-4 como miembro declarado del sobre y CA-4b en el NOD del dibujo. I-54 sale del protocolo de ramas paralelas; citas desplazadas y baseline actualizadas (`[V10-D22]`); I-52 convive con todas las guardas de I-54 sin relajarlas (T-GRD-02 7 → 8 y T-GRD-08 reapuntados con motivo) | CLOSED BY V10 | §6.4; §6.7; §10.4; §15; §19; CT-46; G-M18; T-M16; T-M20 |
| `[V10-D14]` | C:V10-14 | I-49 G6 `5f969cc` observado durante V9 (`[V9-D21]`) | **I-49**: `5f969cc`, sin cambios; G6 productivo en `RackCad.Application.Expressions` y `RackCad.Application.Units`. Contrato de I-52 **NON-MATERIAL**; coordinacion **MATERIAL**. Se mantienen CT-46, G-M18 y `LengthUnitsAuthorityGuardTests`. Aun no hay `PlanReadSet` productivo, dominio del consumidor, cambios en ProjectVariables ni en Plugin o UI | CLOSED BY V10 (registro) | §6.5.5; §6.7; §10.2; §15; CT-46; G-M18 |
| `[V10-D15]` | AR9-09 · R:V10-11 · C:V10-15 | I-53D G7 `d280197` sin integrar; sobre el arbol combinado solo CT-26, C2-4 del Dinamico y T-M30 | **I-53D**: `a57bd50`, rebasada sobre `ba497f1` con un G7 de contenido equivalente al `d280197` revisado (`range-diff` identico y mismo patch-id de `src` y `tests`), e **integrada en `main @ dad4e77`** durante la redaccion de V10, antes del Candidato de I-52 (merge del cierre documental `90d5c5e`, sin cambios de `src` ni de `tests` respecto de `a57bd50`; `[V10-D23]`). Clasificacion: contrato de I-52 **NON-MATERIAL**; coordinacion **MATERIAL**; C2-4 del Dinamico **MATERIAL / CHANGED**. Como integro antes del Candidato, tras el rebase y sobre el arbol combinado: CT-26, C2-4, T-M30, **T-M17**, **T-M17b** y las guardas relacionadas (incluidas las `D34_*` de la ventana del Dinamico, que es tambien la entrada de C2-2a y C2-2b) condicionan el cierre de G6/G7; I-53D sale del protocolo de ramas paralelas | CLOSED BY V10 | §6.7; §8.5; §14.2 G6; §15; CT-41 |
| `[V10-D16]` | R:V10-01..R:V10-10 · C:V10-01..C:V10-12 | ADR-0036 con huella «que sobre-aproxima todo lo que dibuja» (grosor incluido), contexto de la autoridad origen tambien para la huella, sin frescura tras importar, contexto de materializacion completo y valores por defecto afirmados como hecho | ADR-0036 sigue **PROPOSED** y se alinea: ocupacion de modelo en WCS sin anchuras de presentacion; contexto de evaluacion destino con DRAWING FIRST; frescura tras `EnsureForPlan` con `FootprintInputFingerprint`; anotaciones y cotas con su contexto consumido; read-set del contexto de materializacion; conflicto de ordenes de pantalla y trazado; `XLINE`/`RAY`; ayuda sin alias; citas de ADR-0039; redaccion de CT-50; I-54 integrada; «la caracterizacion **DEBE** demostrar» | CLOSED BY V10 | ADR-0036 |
| `[V10-D17]` | C:V10-16 | Plan de G3 de V9 | CT-36, CT-47, CT-48, CT-49 y CT-50, T-M63, T-M67, T-M69, T-M71 y T-M72, G-M23, M-35 y M-37 ampliados con casos obligatorios. **Huella:** escala de la fuente `s ≠ 1`, escalas anidadas, ancho de modelo frente a `LineWeight`, anchos solo de pantalla o de trazado excluidos, mascaras, limites de texto y de cota, escala de anotacion, STB/CTB, colisiones de capa con nombre y de bloque anidado. **Frescura:** huella de biblioteca distinta del dibujo real tras `EnsureForPlan`, pieza **sin** cambio de mano afectada, repeticion del orden de plan y de ST-19 que lo detecta. **Orden:** conflicto de pantalla y trazado entre fuentes seleccionadas, `XLINE`/`RAY`, objeto seleccionado e ignorado, objeto no seleccionado. **Read-set:** cambiar un `CLAYER` que el productor no consume no lo invalida; cambiar `DIMSTYLE`, `TEXTSTYLE` u otro estado consumido si. **Ayuda:** entrada de `RACKMIRROR` sin alias que pasa T-GRD-08. G3 sigue sin produccion; contradiccion material ⇒ Proposal V11 | CLOSED BY V10 | §12; §13; §14 |
| `[V10-D18]` | C:V10-17 · R:V10-12 | Contradiccion material de V9 ⇒ Proposal V10 | Nueva matriz `[V10-Dnn]` sobre AR9-01..AR9-09 y las decisiones vinculantes del Coordinador; `[V9-D02]`, `[V9-D04]`, `[V9-D14]` y `[V9-D19]` se reabren y se cierran explicitamente (§0.4); lo demas de V9 sigue vigente; `OPEN MATERIAL = NONE` para revision; toda contradiccion material de V10 ⇒ **Proposal V11**; proceso: acuerdo sobre V10 → rebase/reconciliacion sobre el main vigente → freeze por SHA exacto → re-check exact-SHA → O-1 → G3 → (Proposal V11 si G3 contradice materialmente / ADR-0036 aceptado) → G4 | CLOSED BY V10 | §1.2; §7.0; §14.1; §16; §19 |
| `[V10-D19]` | Precision al redactar | «Huella» designaba a la vez la huella de la referencia (§9.2) y la huella visual (§11.3 punto 5); «re-verificacion de huellas» en PREPARE y MUTATE no decia cual | **Terminologia**: «huella de la referencia» (§9.2) es el estado capturado de cada fuente y de su contexto; «huella visual» es `VisualFootprintResult` (§11.3 punto 5); `FootprintInputFingerprint` reune las entradas de una huella visual; `TraceFingerprint` es trazabilidad de la evidencia de simetria (§11.3 punto 10). Ninguno sustituye a otro, y §9.1 y §9.2 nombran cada uno de forma explicita | CLOSED BY V10 | §9.1; §9.2; §11.3 puntos 5 y 10 |
| `[V10-D20]` | Precision al redactar | L-01..L-36 | O-1 se reevalua sobre L-01..L-37: L-37 nueva (superposiciones solo de presentacion; `XLINE`/`RAY`); L-34 reescrita (`XLINE`/`RAY` posterior y conflicto de ordenes de pantalla y trazado); L-36 reescrita (redaccion de CT-50 y read-set); L-26 ampliada (huella visual que cambia tras `EnsureForPlan`, PRE-18) | CLOSED BY V10 | §1.2; §1.3 |
| `[V10-D21]` | Precision al redactar | Preflight de V9 | Preflight de V10 (§15.1): `main @ ba497f1` con I-54 integrada y su rama remota retirada; I-49 `5f969cc` sin cambios; I-53D `a57bd50` rebasada sobre `ba497f1` con G7 equivalente; sin ramas nuevas; biblioteca con el mismo SHA-256; censo de `[CommandMethod(`: main 35, I-53D 35, I-49 33, I-52 33. Ninguna paralela modifica una autoridad de I-52: sin STOP. El re-fetch previo a la publicacion encontro los movimientos de `[V10-D23]` | CLOSED BY V10 (registro) | §15.1 |
| `[V10-D22]` | Precision al redactar | Citas de I-52 ancladas en `1b091be`; guardas de I-54 leidas en su rama | **Citas y guardas de I-54 en main**: `RackEmbedComposer.cs:21-35` → `:43-60` (el rango incluye ahora la regla F y la herencia de `CustomProperties`), `:26` → `:50`, `:33` → `:58`; `RackEmbedDocument.cs:35-64` → `:35-75` (con el miembro `CustomProperties` en `:67`), `:70-73` → `:81-84`; ningun otro rango citado por V10 cambia de contenido entre `1b091be` y `ba497f1` fuera de los desplazamientos de I-53S de §15.1. Guardas de I-54 leidas en `ba497f1`: T-GRD-01..T-GRD-03 (`CustomPropertiesEnvelopeGuardTests.cs:22-170`, censo de siete llamadas a `Compose` en `:185-194`), T-GRD-04..T-GRD-07 (`CustomPropertiesEdgeGuardTests.cs:27`, `:91`, `:141`, `:173`), T-GRD-08 (`CustomPropertiesCommandGuardTests.cs:37-145`, `CustomPropertiesHelpCensusTests.cs:23-124`, `WindowCensusGuardTests.cs:178`, `:183`) y `GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA` = 35 (`SelectiveEditorOpenTests.cs:547-554`) | CLOSED BY V10 (registro) | §6.7; §15.1; CT-46 |
| `[V10-D23]` | Precision al redactar | Paralelas y main de la orden de V10: main `ba497f1`, I-49 `5f969cc` e I-53D `a57bd50` sin integrar; sin I-55 | **Movimientos durante la redaccion de V10** (re-fetch previo a la publicacion, §15.1). **main `dad4e77`** integra I-53D E3 (rama remota retirada): merge `--no-ff` del cierre documental `90d5c5e` sobre `ba497f1`, con `git diff 90d5c5e dad4e77` vacio y `a57bd50` → `90d5c5e` solo en docs; como integro antes del Candidato de I-52, rige `[V10-D15]`. Censo de `[CommandMethod(` 35, indice de ADR, guardas de I-54 y `RackCommandReference.cs` sin cambios; `LoadExisting`, `DesignToInsert` y `SystemToInsert` del Dinamico identicos a `ba497f1`; la unica cita de V10 que se desplaza es `RackDynamicSystemWindow.xaml.cs:2947-2955` → `:3394-3402`, con contenido identico. **I-49 `5a3714f`**: Amendment A2 de V6, solo docs (sintaxis del cualificador de `VariableId`; ADR de reemplazo de D6 y D7 de ADR-0040 pendiente de revision y del Owner; `PlanReadSet`, unidades y semantica del evaluador sin cambio; G6 NOT CLOSED, G7 BLOCKED): contrato de I-52 NON-MATERIAL. **I-55** (rama nueva `feature/creacion-de-vistas` @ `cfdb702`, rebasada sobre `dad4e77`): reclamo, bootstrap, G1 Discovery y G1.1, solo docs; G1.1 la redefine como **View Placement & Projection** (ID17, primera vista libre; ID18, varias vistas de un rack en un flujo; ID19, proyeccion multi-rack con una transformacion comun en la que cada rack conserva su `RackId`) sobre una foundation de preparacion de vistas antes de materializar; mide 0 archivos productivos compartidos con I-52, declara el cruce semantico con el contexto de materializacion (§8.7) y se declara compatible con I-52 sin depender de ella: contrato de I-52 NON-MATERIAL; coordinacion vigilada, con riesgo de autoridades duplicadas de preparacion de vistas, materializacion o transformacion multi-rack (R-45, §15.3). Ninguno modifica una autoridad de I-52: sin STOP | CLOSED BY V10 (registro) | §6.7; §8.5; §15; §17 (R-45); §19; CT-41 |

### 0.3 Resumen

```text
OPEN MATERIAL = NONE
```

Diferido con fail-closed inequivoco (no condiciona la arquitectura):

| Asunto | Fail-closed mientras tanto | Donde se resuelve |
|---|---|---|
| Viabilidad, paridad y rendimiento de la sonda por estado en `acad.exe` (CT-36) | sin sonda viable o sin paridad ⇒ X-17a en toda pieza con cambio de mano | G3; si no es viable ⇒ Proposal V11 o reduccion de alcance |
| Punto de observacion tras la evaluacion auxiliar (CT-36) | sin punto soportado ⇒ X-17a | G3 |
| Estados exactos de cada propiedad visual segun la API y semantica real de visibilidad (apagado, inutilizado, inutilizado en ventana, capa `0`, ancestros, anotativos) | estado o mecanismo no modelado ⇒ `UNKNOWN`; `VisibilityPath` distinta ⇒ X-22 | G3 (CT-36) |
| **Completitud de las propiedades y el estado por clase soportada, con el metodo cerrado** (`[V8-D01]`, `[V9-D08]`, `[V10-D10]`) | propiedad, entrada o estado sin clasificar o `UNSUPPORTED` ⇒ `UNKNOWN` (X-17b) | G3 (CT-36) |
| **`Elevation` admitida en HATCH solido** (`[V8-D04]`) | `Elevation ≠ 0` ⇒ `UNKNOWN` | G3 (CT-36) |
| **Contexto reproducible de la base auxiliar para la evidencia de simetria** (`PSTYLEMODE`, `INSUNITS`, escala de anotacion y lo que se descubra) (`[V8-D07]`) | contexto no reproducible o no verificado ⇒ `UNKNOWN` (X-17a) | G3 (CT-36, dibujo STB y CTB) |
| **Contexto de evaluacion destino de las huellas visuales** (simulacion de la importacion con DRAWING FIRST y cobertura transitiva de simbolos, `[V10-D02]`, `[V10-D09]`) | colision o simbolo que no se puede simular como lo resolvera MUTATE ⇒ huella `UNKNOWN` (E10 en ST-19, X-22 en orden de plan) y, en piezas con cambio de mano, X-17a | G3 (CT-36) y G6/G7 (T-M63) |
| **Efecto real de las escalas y `LinetypeScale` de ancestros, y de las escalas globales** (`[V8-D08]`, `[V8-D09]`) | ancestro con escala no uniforme o efecto no modelado ⇒ `UNKNOWN`; contexto distinto ⇒ X-22 | G3 (CT-36) |
| **Mecanismo y consumo reales del contexto de materializacion** (`MaterializationContextReadSet`, `[V9-D05]`, `[V10-D04]`, `[V10-D05]`) | consumo no determinable ⇒ `UNKNOWN` (X-17a); productor asimetrico ⇒ `UNKNOWN`; observacion consumida cambiada antes de MUTATE ⇒ E10/E11 | G3 (CT-50, CT-36, CT-49) |
| Clases y superposiciones sensibles al orden visual dentro de la pieza | superposicion material sin orden demostrado ⇒ X-22 | G3 (CT-36) |
| **Pares de instancias con orden relativo invertido por kind y vista, y superposicion de sus huellas de ocupacion de modelo refrescadas** (`[V9-D03]`, `[V10-D01]`, `[V10-D03]`) | par con huellas superpuestas de forma material o huella `UNKNOWN` ⇒ X-22 | G3 (CT-48, CT-36) |
| **Ocupacion de modelo conservadora por clase** (anchos geometricos, mascaras, limites de texto y de cota, marcos, anidados; `[V9-D02]`, `[V10-D01]`) | huella no garantizada conservadora ⇒ `UNKNOWN` (E10 en ST-19, X-22 en orden de plan) | G3 (CT-36, CT-49) |
| **Frescura de las huellas visuales tras `EnsureForPlan`** (`FootprintInputFingerprint`, `[V10-D03]`) | entradas cambiadas y huella, orden de plan o ST-19 no re-demostrados ⇒ E10 (PRE-18) | G3 (CT-48, CT-49) y G6/G7 (T-M67, T-M69, T-M71) |
| **Inventario cerrado de la presentacion de la referencia fuente** (`[V8-D14]`, `[V9-D08]`, `[V10-D10]`) | propiedad, entrada o estado sin clasificar o `UNSUPPORTED` ⇒ E4 (ST-18) | G3 (CT-49) |
| **Superposiciones con todo objeto posterior a la fuente y orden efectivo en Model Space** (`[V9-D01]`, `[V9-D04]`, `[V10-D06]`, `[V10-D07]`) | superposicion material o no clasificable, huella no disponible o no conservadora, `XLINE`/`RAY` sin interseccion analitica, conflicto de ordenes de pantalla y trazado con copias superpuestas o contexto de orden no legible ⇒ E10 (ST-19) | G3 (CT-49) |
| **Transportabilidad caracterizada de ST-13** (`PlotStyleName` con STB y derivacion del color con CTB, `[V9-D09]`) | comportamiento no caracterizado ⇒ E4 (ST-18) | G3 (CT-49) |
| Canonicalizacion adicional (fusion o normalizacion de sentido sin continuidad demostrable) | sin evidencia: sin fusion, con orientacion y multiplicidad | G3 (CT-36, CT-47) |
| Tasa de falsos positivos y negativos de la canonicalizacion (CT-36, CT-47) | correspondencia ambigua ⇒ `UNKNOWN` | G3; ambiguedad material ⇒ Proposal V11 |
| Contenido de la cache de biblioteca medido antes y despues | contrato READ / CLONE ONLY: `EnsureBlocks` hacia la base auxiliar o clonado de solo lectura desde el dibujo, con la precedencia DRAWING FIRST y la presencia de todos los BTR requeridos (el retorno es diagnostico); G-M20 | G3 (CT-36, harness fuera de `src/`) y G6/G7 (T-M63) |
| Transformacion efectiva alterada por acciones dinamicas | X-23 `UNKNOWN` | G3 (CT-36) |
| Confirmacion de `PlotStyleSource` segun el modo de estilos de la autoridad origen | modo desconocido o no reproducido ⇒ `UNKNOWN` | G3 (CT-36) |
| Corpus indicativo de la biblioteca actual (§1.4, CT-47) | SHA-256 distinto ⇒ la evidencia no se reutiliza; pieza no demostrada ⇒ X-17a/X-17b/X-22 | G3 |
| Semantica de `MountingFace` (S-10, D-03b) | sin confirmacion de CT-12 ⇒ FAIL_CLOSED | G3 |
| Filas `R` con `EvidenceStatus = G3_PENDING` | V-VIEW-ALL por rack; regla de cierre de G3 (§7.0) | G3 |
| Corpus exhaustivo de miembros retirados por ruta JSON (CT-32) | miembro desconocido ⇒ V-META UNKNOWN | G3 |
| Estabilidad 9b de DEP-05..DEP-07 y cierres de descriptores | contraejemplo ⇒ STOP → Proposal V11 | G3 (CT-37, CT-38) |
| Evidencia universal para estados dinamicos dependientes de vinculos | sin ella ⇒ S-26/S-27a..S-27c FAIL_CLOSED | futuro (autoridad o dominio autoritativo) |
| Cota autoritativa de propiedades vinculables | sin cota ⇒ solo overrides en `DEP-ROW` | si I-49 integra (§15.3) |
| Predicado exacto de «tope activo» (Selectivo y Push Back por lado) | cualquier celda dibujable ⇒ FAIL_CLOSED | G3 (CT-15) |
| Ids de placa Cantilever en BOM (K-09) | V-BOM distinto ⇒ FAIL_CLOSED | por rack |
| Linea base exacta de cada store | V-RT-STORE ⇒ FAIL_CLOSED | G3 (CT-35) |
| Valor de la tolerancia de escala | sin valor no hay implementacion (G4) | G4 |
| Semantica de `IsStandard` (H-06) | Auto con baseline ⇒ FAIL_CLOSED | G5 |
| Efectos reales de importacion y UNDO del comando | UNKNOWN declarado; nada se promete | G9 (Owner) |
| Censo de parametros con lado (CT-25) | parametro sin regla ⇒ FAIL_CLOSED | G3 |
| Declaraciones de invariancia de metadata o de exclusion de entidades | ninguna ⇒ FAIL_CLOSED | futuro |
| Costuras de Insertar por kind (CT-26) | sin C2-2b verde G7 no arranca | G3 / G6 |
| **C2-4 sobre el arbol combinado con I-53S** (`[V8-D17]`) | I-53S ya integro en main (`104ef3a`): G6/G7 no cierran sin CT-26, C2-4 y T-M30 verdes sobre el arbol combinado tras el rebase | rebase y G6/G7 |
| **C2-4, C2-2a y C2-2b del Dinamico con I-53D** (integrada en main `dad4e77`; G7 `a57bd50`, equivalente a `d280197`; `[V10-D15]`, `[V10-D23]`) | I-53D integro antes del Candidato de I-52: G6/G7 no cierran sin CT-26, C2-4 del Dinamico, T-M30, T-M17 y T-M17b verdes sobre el arbol combinado tras el rebase | rebase y G6/G7 |
| **Censo `B`, censo de ayuda y censos por nombre** (`[V9-D06]`, `[V10-D08]`) | `B` medido en el SHA de reconciliacion (hoy `main @ dad4e77` = 35, sin cambio desde `ba497f1`); censo, nombres o pares de ayuda distintos de lo esperado ⇒ RED en G7 | rebase y G7 |
| **Guardas de I-54 en main** (T-GRD-01..T-GRD-08, `[V8-D19]`, `[V10-D13]`) | sin llamadas de I-52 a sus autoridades (G-M22); T-GRD-02 7 → 8 y T-GRD-08 reapuntados con motivo (T-M16); G-M18 las aplica | rebase, G5..G7 |
| **Guardas de I-49 G6** (`LengthUnitsAuthorityGuardTests` y nucleo de expresiones, `[V9-D21]`, `[V10-D14]`) | I-52 no declara literales ni conversiones de unidades de longitud ni nombra el nucleo; al reconciliar, CT-46 y G-M18 | reconciliacion posterior a la integracion de I-49 |
| **Cruce con I-55** (View Placement & Projection, solo docs; `[V10-D23]`, R-45) | sin produccion de I-55 no hay efecto; si su G2 fija una autoridad de preparacion de vistas, de materializacion o de transformacion multi-rack, se re-mide el cruce y se reporta antes de fijar archivos; I-52 no depende de I-55 | G3 de I-52 o G2 de I-55 |

### 0.4 Estado de las marcas de V9 bajo V10

| Marca V9 | Estado en V10 |
|---|---|
| `[V9-D01]` | vigente y precisada por `[V10-D06]` (conflicto de ordenes de pantalla y trazado) y `[V10-D07]` (`XLINE`/`RAY`) |
| `[V9-D02]` | **reabierta y cerrada** por `[V10-D01]` (ocupacion de modelo en WCS), `[V10-D02]` (contexto de evaluacion destino) y `[V10-D03]` (frescura tras `EnsureForPlan`) |
| `[V9-D03]` | vigente y precisada por `[V10-D01]` y `[V10-D03]` (huellas de ocupacion de modelo refrescadas) |
| `[V9-D04]` | **reabierta y cerrada** por `[V10-D01]`, `[V10-D02]`, `[V10-D03]`, `[V10-D04]` y `[V10-D07]` |
| `[V9-D05]` | vigente y precisada por `[V10-D04]` (anotaciones y cotas), `[V10-D05]` (read-set) y `[V10-D12]` (redaccion) |
| `[V9-D06]` | vigente y precisada por `[V10-D08]` (ayuda sin alias) y `[V10-D13]` (`B = 35` en main) |
| `[V9-D07]` | vigente y precisada por `[V10-D09]` (precedencia DRAWING FIRST) |
| `[V9-D08]` | vigente y precisada por `[V10-D10]` |
| `[V9-D09]` | vigente |
| `[V9-D10]` | vigente y precisada por `[V10-D11]` (citas de ADR-0039) |
| `[V9-D11]` | sustituida por `[V10-D13]` (I-54 integrada en main) |
| `[V9-D12]` | sustituida por `[V10-D13]`, `[V10-D14]`, `[V10-D15]`, el preflight de V10 (`[V10-D21]`) y los movimientos durante la redaccion (`[V10-D23]`) |
| `[V9-D13]` | sustituida por `[V10-D16]` |
| `[V9-D14]` | **reabierta y cerrada** por `[V10-D17]` |
| `[V9-D15]` | sustituida por `[V10-D18]` (Proposal V11) |
| `[V9-D16]` | vigente |
| `[V9-D17]` | sustituida por `[V10-D08]` (entrada de ayuda sin alias; T-M16 incondicional) |
| `[V9-D18]` | sustituida por `[V10-D12]` (notacion corregida tambien en §11.3 punto 1 y en el caso C) |
| `[V9-D19]` | **reabierta y cerrada** por `[V10-D03]` (huellas refrescadas), `[V10-D05]` (read-set) y `[V10-D06]` (ordenes contradictorios) |
| `[V9-D20]` | sustituida por `[V10-D20]` |
| `[V9-D21]` | vigente para I-49 y precisada por `[V10-D14]` y `[V10-D23]` (Amendment A2); lo que registraba de I-54 lo sustituye `[V10-D13]` |
| `[V9-D22]` | sustituida por `[V10-D15]` (I-53D integrada en main, `[V10-D23]`) |

### 0.5 Estado de las marcas de V8 (fijado por V9; sin cambios en V10)

| Marca V8 | Estado (fijado por V9) |
|---|---|
| `[V8-D01]` | vigente y precisada por `[V9-D08]` (metodo de inventario cerrado) |
| `[V8-D02]` | vigente |
| `[V8-D03]` | vigente y ampliada por `[V9-D08]` (atributos, XREF, overlay y dependientes) |
| `[V8-D04]`, `[V8-D05]` | vigentes |
| `[V8-D06]` | vigente y precisada por `[V9-D09]` (`ColorBook` con valor almacenado) |
| `[V8-D07]`, `[V8-D08]` | vigentes |
| `[V8-D09]` | vigente y ampliada por `[V9-D05]` (`MaterializationContext`) |
| `[V8-D10]` | sustituida por `[V9-D07]` |
| `[V8-D11]` | vigente |
| `[V8-D12]` | vigente y precisada por `[V9-D02]` y `[V9-D03]` (huellas visuales) |
| `[V8-D13]` | vigente y precisada por `[V9-D09]` (estilo de trazado con STB y CTB) |
| `[V8-D14]` | vigente y precisada por `[V9-D08]` (inventario cerrado) |
| `[V8-D15]` | sustituida por `[V9-D01]` y `[V9-D04]` |
| `[V8-D16]` | vigente |
| `[V8-D17]` | vigente y precisada por `[V9-D12]` y `[V9-D22]` (I-53D G7) |
| `[V8-D18]` | vigente y precisada por `[V9-D12]` y `[V9-D21]` (I-49 G6) |
| `[V8-D19]` | vigente y precisada por `[V9-D10]` (CA-4b de alcance Proyecto) y `[V9-D11]` (I-54 G7B) |
| `[V8-D20]` | vigente y ampliada por `[V9-D13]` |
| `[V8-D21]` | sustituida por `[V9-D14]` |
| `[V8-D22]` | sustituida por `[V9-D15]` (Proposal V11) |
| `[V8-D23]` | vigente y ampliada por `[V9-D16]` |
| `[V8-D24]` | sustituida por `[V9-D11]`, `[V9-D12]`, `[V9-D21]`, `[V9-D22]` y el preflight de V9 (§15.1) |
| `[V8-D25]` | vigente y precisada por `[V9-D01]`, `[V9-D04]` y `[V9-D19]` |
| `[V8-D26]` | sustituida por `[V9-D20]` |

### 0.6 Estado de las marcas de V7 (fijado por V8; sin cambios en V9 ni V10)

| Marca V7 | Estado (fijado por V8) |
|---|---|
| `[V7-D01]` | vigente y precisada por `[V8-D06]` (fuentes tipadas) |
| `[V7-D02]` | vigente; su ruta canonica la sustituye `[V8-D11]` |
| `[V7-D03]` | vigente y precisada por `[V8-D06]`, `[V8-D08]` y `[V8-D09]` |
| `[V7-D04]` | vigente y ampliada por `[V8-D03]` (anotativos) |
| `[V7-D05]` | vigente |
| `[V7-D06]` | vigente y ampliada por `[V8-D02]` y `[V8-D08]` |
| `[V7-D07]` | vigente y precisada por `[V8-D11]` |
| `[V7-D08]` | vigente y ampliada por `[V8-D16]` |
| `[V7-D09]` | vigente y ampliada por `[V8-D12]` (orden entre piezas) |
| `[V7-D10]` | vigente y precisada por `[V8-D02]` |
| `[V7-D11]` | vigente |
| `[V7-D12]` | vigente y precisada por `[V8-D04]` |
| `[V7-D13]` | vigente |
| `[V7-D14]` | vigente y precisada por `[V8-D07]` (contexto de la base auxiliar) |
| `[V7-D15]` | vigente y precisada por `[V8-D10]` |
| `[V7-D16]`..`[V7-D19]` | vigentes |
| `[V7-D20]` | vigente y ampliada por `[V8-D20]` |
| `[V7-D21]` | vigente y precisada por `[V8-D26]` |
| `[V7-D22]` | sustituida por `[V8-D21]` |
| `[V7-D23]` | sustituida por `[V8-D22]` (Proposal V11) |
| `[V7-D24]` | sustituida por `[V8-D17]`..`[V8-D19]` y `[V8-D24]` |
| `[V7-D25]` | vigente y ampliada por `[V8-D23]` |
| `[V7-D26]` | vigente; PRE-15 y PRE-16 nuevas por `[V8-D14]` y `[V8-D15]` |
| `[V7-D27]` | vigente y precisada por `[V8-D06]` y `[V8-D07]` |

### 0.7 Estado de las marcas de V6 (fijado por V7; sin cambios en V8, V9 ni V10)

| Marca V6 | Estado (fijado por V7) |
|---|---|
| `[V6-D01]` | sustituida por `[V7-D01]` y `[V7-D06]` |
| `[V6-D02]` | sustituida por `[V7-D01]`..`[V7-D03]` y `[V7-D27]` |
| `[V6-D03]` | sustituida por `[V7-D04]` y `[V7-D05]` |
| `[V6-D04]` | sustituida por `[V7-D04]` (`Visible` dentro de `VisibilityPath`) |
| `[V6-D05]` | sustituida por `[V7-D09]` |
| `[V6-D06]` | vigente y ampliada por `[V7-D07]` y `[V7-D08]` |
| `[V6-D07]` | vigente y ampliada por `[V7-D17]`, `[V7-D21]` y `[V7-D25]` |
| `[V6-D08]` | vigente |
| `[V6-D09]` | vigente y precisada por `[V7-D17]` |
| `[V6-D10]` | sustituida por `[V7-D23]` (Proposal V8) |
| `[V6-D11]` | sustituida por `[V7-D10]` y `[V7-D11]` |
| `[V6-D12]` | vigente |
| `[V6-D13]` | vigente y precisada por `[V7-D13]` |
| `[V6-D14]` | vigente y ampliada por `[V7-D14]` |
| `[V6-D15]` | vigente y ampliada por `[V7-D14]` y `[V7-D15]` |
| `[V6-D16]` | vigente y ampliada por `[V7-D16]` |
| `[V6-D17]` | vigente y ampliada por `[V7-D10]` y `[V7-D12]` (variantes de clase) |
| `[V6-D18]` | vigente y ampliada por `[V7-D12]` (tambien patron) |
| `[V6-D19]`..`[V6-D22]` | vigentes |
| `[V6-D23]` | vigente y precisada por `[V7-D18]` |
| `[V6-D24]`, `[V6-D25]` | vigentes |
| `[V6-D26]` | sustituida por `[V7-D24]` |
| `[V6-D27]` | sustituida por `[V7-D19]` y `[V7-D20]` |
| `[V6-D28]` | sustituida por `[V7-D22]` |
| `[V6-D29]`, `[V6-D30]` | vigentes |

### 0.8 Estado de las marcas de V5 (fijado por V6; sin cambios en V7, V8, V9 ni V10)

| Marca V5 | Estado en V6 |
|---|---|
| `[V5-D01]` | vigente; la evidencia pasa a ser visual-geometrica (`[V6-D01]`..`[V6-D06]`) |
| `[V5-D02]` | vigente; origen evaluado por `[V6-D16]` |
| `[V5-D03]` | vigente; contrato de runtime y postcondicion por `[V6-D15]` |
| `[V5-D04]` | sustituida por `[V6-D01]`..`[V6-D05]`, `[V6-D17]` y `[V6-D18]` |
| `[V5-D05]` | sustituida por `[V6-D11]` y `[V6-D12]` |
| `[V5-D06]` | vigente y ampliada por `[V6-D06]` y `[V6-D13]` |
| `[V5-D07]` | vigente |
| `[V5-D08]` | vigente y ampliada por `[V6-D20]` |
| `[V5-D09]` | vigente y ampliada por `[V6-D15]` |
| `[V5-D10]` | vigente y ampliada por `[V6-D14]` y `[V6-D15]` |
| `[V5-D11]` | vigente y ampliada por `[V6-D07]`..`[V6-D09]` |
| `[V5-D12]`..`[V5-D15]` | vigentes |
| `[V5-D16]` | vigente y ampliada por `[V6-D19]` |
| `[V5-D17]`, `[V5-D18]` | vigentes |
| `[V5-D19]` | vigente y ampliada por `[V6-D21]` |
| `[V5-D20]` | vigente |
| `[V5-D21]` | vigente y precisada por `[V6-D22]` |
| `[V5-D22]` | sustituida por `[V6-D25]` y `[V6-D26]` |
| `[V5-D23]` | sustituida por `[V6-D27]` |
| `[V5-D24]` | sustituida por `[V6-D28]` |
| `[V5-D25]` | sustituida por `[V6-D10]` |
| `[V5-D26]` | vigente (la revision de V5 confirmo que RM-4 es el unico R-B entre los tipos con `[JsonExtensionData]`) |
| `[V5-D27]` | vigente y precisada por `[V6-D20]` |
| `[V5-D28]`..`[V5-D30]` | vigentes |

### 0.9 Alias historicos de V2 (solo reconciliacion)

| Alias V2 | Equivalente (V3..V10) |
|---|---|
| `R-s` | `RepresentabilityClass = REPRESENTABLE` + `EvidenceStatus = G3_PENDING` + `OperationalDisposition = ALLOW` (sujeto a V-VIEW-ALL y V-DEP) |
| `C/F` | fila de seccion: `REPRESENTABLE` + `CODE_SUPPORTED` + `CANONICALIZE` (legado inequivoco) o `FAIL_CLOSED` (resto) |
| `LB` | `REPRESENTABLE` + `CODE_SUPPORTED` + `BASELINE_LIMITED` (normalizacion declarada del store); la metadata semantica desconocida ya no es `LB`, es `UNKNOWN` |
| `FC` | precondicion operativa con `FAIL_CLOSED`; no es clase de representabilidad (§7.9) |

### 0.10 Cobertura explicita

| Hallazgo o cambio | Marca |
|---|---|
| AR9-01 (huella visual como ocupacion de modelo en WCS) | `[V10-D01]`, `[V10-D07]` |
| AR9-02 (contexto de evaluacion destino, frescura, anotaciones y cotas) | `[V10-D02]`, `[V10-D03]`, `[V10-D04]`, `[V10-D19]` |
| AR9-03 (ordenes de pantalla y trazado contradictorios; `XLINE`/`RAY`) | `[V10-D06]`, `[V10-D07]` |
| AR9-04 (read-set del contexto de materializacion) | `[V10-D05]` |
| AR9-05 (redaccion de los valores por defecto) | `[V10-D12]` |
| AR9-06 (ayuda sin alias) | `[V10-D08]` |
| AR9-07 (precedencia de rutas y colisiones de nombres) | `[V10-D09]` |
| AR9-08 (notacion A/B y citas de ADR-0039) | `[V10-D11]`, `[V10-D12]` |
| AR9-09 (paralelas y main posteriores a V9) | `[V10-D13]`, `[V10-D14]`, `[V10-D15]`, `[V10-D21]`, `[V10-D22]`, `[V10-D23]` |
| Cambios exactos R:V10-01..R:V10-12 de la revision | V10-01 → D01; V10-02 → D02; V10-03 → D03, D19; V10-04 → D04; V10-05 → D06, D07; V10-06 → D05; V10-07 → D12; V10-08 → D08; V10-09 → D09; V10-10 → D11, D12; V10-11 → D13, D15; V10-12 → D18 |
| Precisiones del Coordinador C:V10-01..C:V10-17 | V10-01 → D01; V10-02 → D02; V10-03 → D03; V10-04 → D04; V10-05 → D05; V10-06 → D06; V10-07 → D07; V10-08 → D08; V10-09 → D09; V10-10 → D10; V10-11 → D11; V10-12 → D12; V10-13 → D13; V10-14 → D14; V10-15 → D15; V10-16 → D17; V10-17 → D18 |
| ADR-0036 (decisiones 11..13 y alineacion general) | `[V10-D16]` |
| Paralelas y main observados en el preflight y en el re-fetch de V10 | `[V10-D13]`, `[V10-D14]`, `[V10-D15]`, `[V10-D21]`, `[V10-D22]`, `[V10-D23]` |
| Precisiones del ejecutor | `[V10-D19]`..`[V10-D23]` |

---

## 1. Decisiones de producto

### 1.1 Decisiones del Coordinador vigentes (desde V1)

Registro: `docs/automation/decisions/I-52.md` §4. Vinculan V10; **no** son consenso final.

| # | Decision | Precision vigente |
|---|---|---|
| **PDC-1** | **Solo copia.** Crea copia reflejada, conserva **todos** los originales, nunca borra, nunca refleja en sitio, nunca conserva el `RackId` en la copia. **Sin** pregunta «¿Borrar objetos originales?» | — |
| **PDC-2** | Nombre logico por rack `<base> - espejo`, **sin ordinal**. `Name` ≠ `RackId`. Los nombres de `BlockTableRecord` siguen la politica de unicidad existente de cada familia de plan | La autoridad de plan recibe el nombre logico (§8.1) |
| **PDC-3** | Autoridad = referencias **fisicamente seleccionadas**. Sin buscar hermanas; sin crear vistas no seleccionadas. Vistas seleccionadas del mismo `RackId` = **un** grupo con **un** `NewRackId`. Una referencia no admisible **falla toda la operacion** antes de MUTATE | V-VIEW-ALL verifica todas las vistas admisibles del diseño reflejado **sin** buscar ni crear hermanas (§6.6) |
| **PDC-4** | Sin prompt de eje semantico. El usuario solo da la linea (dos puntos) | — |
| **PDC-5** | Indices, numeros y letras derivados se **regeneran** desde el documento reflejado | — |
| **PDC-6** | Protector lateral Left/Right: **UNKNOWN** donde sea ambiguo; estado explicito cuya reflexion no este demostrada ⇒ fail-closed | — |
| **PDC-7** | Clases `REPRESENTABLE`, `REPRESENTABLE_BY_NORMALIZATION`, `REQUIRES_MODEL_CHANGE`, `UNKNOWN`, evaluadas **por rack concreto** en PREFLIGHT; `REQUIRES_MODEL_CHANGE` y `UNKNOWN` material ⇒ fail-closed | Tres dimensiones separadas (§7.0) |
| **PDC-8** | Unico comando nuevo `RACKMIRROR`, **sin alias**. Censo de `[CommandMethod(` **B → B + 1** | `B` = censo del Plugin en el SHA de reconciliacion inmediatamente anterior a introducir `RACKMIRROR`; se re-mide y nunca se asume (hoy `main @ dad4e77` = 35, con `RACKPROPIEDADES` y `RPR` de I-54 integrada). El literal 33 → 34 de V1..V8 queda sustituido (`[V9-D06]`). `RACKMIRROR` tiene entrada en `RackCommandReference` sin alias: ayuda +1 entrada, registros de alias +0, y T-GRD-08 se reapunta sin debilitar (T-M16, `[V10-D08]`) |
| **PDC-9** | Solo Model Space; MINSERT ⇒ fail-closed; linea por dos puntos UCS→WCS; fuente canonica | `Origin = 0` (ST-14), `View`/`Section` decodificables (ST-15), bloques dinamicos, anonimos o anotativos ⇒ fail-closed (ST-17); las restricciones estrictas solo se aplican a candidatos RackCad (precedencia C1..C5, §4.1); la copia conserva la presentacion exterior de su referencia fuente (ST-13), un estado de presentacion no transportable o sin clasificar falla cerrado (ST-18), el orden visual en Model Space se demuestra frente a todo objeto posterior a cada fuente con huellas de ocupacion de modelo refrescadas tras `EnsureForPlan` o falla cerrado (ST-19, PRE-18) y las piezas internas se regeneran como Actualizar, con las observaciones del contexto de materializacion que consuma cada productor (§8.7) (`[V8-D13]`, `[V8-D14]`, `[V9-D01]`, `[V9-D05]`, `[V10-D01]`, `[V10-D03]`, `[V10-D05]`) |
| **PDC-10** | Arquitectura para los seis kinds con bloque; el primer corte solo ejecuta vistas que exponen `μ_k`; la cama falla cerrado; sin ampliar schema | Aceptacion de alcance del Owner (O-1) despues del freeze tecnico y antes de G3, sobre la envolvente maxima de §1.3 y la dependencia de la biblioteca efectiva; la evidencia indicativa de §1.4 informa y no es contrato |

### 1.2 Decisiones del Owner

| # | Decision | Contrato vigente | Momento |
|---|---|---|---|
| **O-1** | **Alcance del primer corte.** El Owner acepta la **envolvente maxima de alcance** de §1.3 (L-01..L-37), su politica fail-closed, la **seleccion todo-o-nada** y la **dependencia de la biblioteca efectiva**. O-1 **no** promete que G3 verifique todos los candidatos y **no** acepta como resultado universal la inspeccion de una biblioteca concreta (§1.4). Si G3 reduce materialmente la envolvente ⇒ Proposal V11 y O-1 se reevalua | envolvente de §1.3 | **pendiente**; se pide **despues del freeze tecnico** y **antes de G3**; **no** se pide en este gate |
| **O-2** | **Topes del Selectivo** | `UNKNOWN → FAIL_CLOSED`. **No** necesita decision ahora; una relajacion vuelve al Coordinador y al Owner | — |
| **O-2B** | **Tope posterior de Push Back** (activo por defecto) | `UNKNOWN → FAIL_CLOSED`. **No** necesita decision ahora; una relajacion vuelve al Coordinador y al Owner | — |
| **O-3** | **Evidencia de simetrias** | ningun bloque se asume simetrico; la prueba es la **equivalencia visual-geometrica** por estado evaluado, con politica cerrada de clases y variantes, fuentes visuales simbolicas tipadas, `VisibilityPath` y orden visual dentro de la pieza y entre piezas sobre huellas visuales de ocupacion de modelo, conservadoras y refrescadas tras `EnsureForPlan` (`GeometrySymmetryResult` y `VisualFootprintResult`, §11.3); la confirmacion del Owner **nunca** es la unica prueba | G3 (CT-36, CT-47, CT-48, CT-49) y G9 (M-15, M-27, M-32, M-36) |
| **O-4** | **ADR-0036** | `propuesto` | se pide **despues de G3** si G3 no contradice materialmente V10; si la contradice ⇒ Proposal V11; **precondicion de G4** |

### 1.3 Envolvente maxima de alcance (para O-1)

La envolvente es el **maximo** que el primer corte puede reflejar. Dentro de ella, cada rack concreto pasa solo si todas
sus filas y verificaciones pasan; lo que no se demuestre falla cerrado **sin mutacion**, y un solo rack que falla hace
fallar **toda** la seleccion (L-27).

**Dentro, como maximo:** Selectivo (frontal por fondo y planta), Dinamico (frontales de salida y entrada, y planta),
Push Back simple y compuesto (frontales `0..3` y planta), Cantilever (frontal y planta) y cabecera independiente
(lateral y planta), con la reflexion canonica de §3.6.

**Limitaciones materiales que el Owner acepta:**

| # | Limitacion | Resultado | Fila o regla |
|---|---|---|---|
| L-01 | Laterales de Selectivo, Dinamico, Push Back y Cantilever | fuera (E5) | S-20, D-15, P-10, PC-13, K-13 |
| L-02 | Cama | fuera siempre | F-01, F-02 |
| L-03 | Dinamico materializado solo como lateral | no se puede reflejar (E5) | D-15 |
| L-04 | Fuente ya reflejada con MIRROR nativo, o con una sola escala negativa: RACKMIRROR **no** corrige racks espejados antes con MIRROR | fail-closed (E4) | ST-9 |
| L-05 | MINSERT RackCad | fail-closed (E4) | ST-3 |
| L-06 | Escala no uniforme, `sz < 0` o `Normal` distinta de +Z | fail-closed (E4) | ST-7, ST-10, ST-5 |
| L-07 | Definicion con `Origin ≠ 0` (BASE/BEDIT movido) | fail-closed (E4) | ST-14 |
| L-08 | Fuente RackCad dinamica, anonima o anotativa | fail-closed con mensaje propio (E4) | ST-17 |
| L-09 | RackCad en Paper Space | ignorado con aviso | ST-4 |
| L-10 | Payload RackCad ilegible o inutilizable | la operacion entera se aborta (E2) | ST-2c |
| L-11 | Selectivo con frentes por fondo en esquina | fail-closed | S-01 |
| L-12 | Selectivo con medio frente dependiente de un vinculo, con postes distintos o que no resuelve | fail-closed | S-02b, S-02c, S-02d |
| L-13 | Selectivo con topes | fail-closed | S-13, S-13b |
| L-14 | Protector lateral explicito, desviador con off-cells asimetricos o dependientes, parrilla o tarimas desbordadas o dependientes | fail-closed | S-12, D-08a, D-10b, S-14c, S-15x, S-15d, S-17c, S-17d |
| L-15 | Tope posterior de Push Back **ACTIVE BY DEFAULT**: la mayoria de los Push Back fallan cerrado salvo `"(ninguno)"` (tras `Trim`, sin distinguir mayusculas) o ninguna celda con `Draws` en cada lado; `OffCells` fuera de rango tambien | fail-closed | P-04, P-05, P-05c, PC-08, PC-08c |
| L-16 | Tarimas frontales de Push Back desbordadas (simple o compuesto, por lado) | fail-closed | P-03c, PC-14b |
| L-17 | Push Back compuesto con ranuras ausentes al inicio o al final de un lado | fail-closed | PC-05b |
| L-18 | Cantilever con arriostramiento de seccion asimetrica, brazo sencillo de seccion asimetrica o ids de placa que cambian el BOM | fail-closed | K-07b, K-08, K-09 |
| L-19 | Cabecera con rasgos distintos entre lados, Auto con baseline estandar o valor de diagonal desconocido | fail-closed | H-04b, H-06, H-02c |
| L-20 | Metadata semantica desconocida del payload, clave exterior desconocida, miembro `UNSUPPORTED`, enum no definido o nombres que colisionan | fail-closed | V-META, X-02b, X-03b, X-16, X-20, X-21 |
| L-21 | Payload legado con miembros retirados: con los que el store ya no conserva (RM-1..RM-3, RM-7), el mensaje pide RACKEDITAR → Actualizar; con el que el store conserva (`HighEndBeamPeralte`), no hay remedio en I-52 | fail-closed con mensaje | X-19, X-19b |
| L-22 | **Pieza que necesita aceptar un cambio de mano y no puede demostrar su equivalencia visual-geometrica** con la sonda por estado (sonda no viable o sin paridad, contexto de la base auxiliar no reproducible, bloques requeridos ausentes en la base auxiliar, entidad o variante no soportada, simetria no verificada): afecta a practicamente **todas** las piezas de frontal y planta (largueros, postes, placas, separadores, parrillas, tarimas, desviadores) | fail-closed | X-17a, X-17b |
| L-23 | **Vinculos que cambian el estado dinamico** de esas piezas sin evidencia universal: Selectivo con `PalletTolerance` vinculada sin overrides que fijen el ancho (largueros) o con `VerticalClearance` vinculada sin alturas fijadas por el camino de cada poste (DEP-10a..DEP-10c) | fail-closed | S-26, S-27a..S-27c, X-17c |
| L-24 | Registro de variables no acreditable con algun grupo Selectivo | fail-closed (E7) | PV-5 |
| L-25 | Alguna vista admisible del rack, seleccionada o no, que no conmuta | fail-closed | X-15 |
| L-26 | Bloques ausentes tras importar, definicion real distinta o cambiada entre PREFLIGHT y PREPARE, o huella visual cuyas entradas cambian tras `EnsureForPlan` y que ya no demuestra el orden (`[V10-D03]`) | fail-closed sin semantica nueva (E10/E11) | PRE-09, PRE-13, PRE-18 |
| **L-27** | **Seleccion todo-o-nada**: un solo rack seleccionado que falle cualquier precondicion, representabilidad o verificacion aborta **todo** el comando; **no** se reflejan los demas | fin sin mutacion (E1..E11) | PDC-3, §9.1 |
| **L-28** | **Dependencia de la biblioteca efectiva**: la representabilidad depende de las definiciones de bloque efectivas (las del dibujo o, si faltan, las de la biblioteca configurada). La biblioteca **no** esta versionada en el repositorio, su ruta la configura el usuario (`BlockLibrary.cs:19-23`) y el resultado puede cambiar si cambia la biblioteca; siempre falla cerrado | variable, siempre fail-closed | §11.3; §1.4 |
| **L-29** | **Pieza cuyas homologas no tienen las mismas fuentes visuales simbolicas tipadas** aunque hoy se vean iguales (por ejemplo `ByLayer` frente a `Explicit` con el mismo valor actual, `InheritLayer` frente a `InheritBlock`, o `ACI(7)` frente a `RGB(255,255,255)`), con `SourceKind`, `Plinegen` o `Closed`, contexto de patron de tipo de linea, `VisibilityPath`, `LineWeight`, `Transparency`, `LinetypeScale` o `PlotStyle` distintos, con un mecanismo de visibilidad no modelado o con un orden visual relativo no demostrado dentro de la pieza | fail-closed | X-17a, X-22 |
| **L-30** | **Pieza con una clase o variante fuera de la politica cerrada** (§11.3 punto 6.1): subclase de una clase soportada (`MInsertBlock` anidado), definicion anidada anotativa, referencia anidada con XCLIP, con atributos o con definicion XREF, overlay o dependiente, grosor, normal, elevacion o ancho fuera de la tabla, tipo de linea complejo, HATCH de degradado (hoy `TARIMA_GENERICA`, §1.4) o de patron, SOLID/TRACE/HATCH no canonizable de forma inequivoca, o propiedad, entrada o estado sin clasificar o `UNSUPPORTED` en el inventario cerrado (`[V9-D08]`) | fail-closed | X-17b |
| **L-31** | **Canonicalizacion conservadora del primer corte**: piezas visualmente simetricas cuyas primitivas sin continuidad demostrable (tipo de linea con trazos, `ByLayer` o heredado) difieren en segmentacion, orientacion o multiplicidad, o con entidades transparentes coincidentes, fallan cerrado hasta que G3 habilite una canonicalizacion adicional con evidencia | fail-closed | X-17a |
| **L-32** | **Pieza cuya evaluacion dinamica altera la transformacion efectiva de la referencia** (`Position`, `Rotation`, `ScaleFactors`, `Normal`) de un modo no representado por el contrato | fail-closed | X-23 |
| **L-33** | **Referencia RackCad fuente con estado de presentacion no transportable o no caracterizado**: filtro espacial (XCLIP), referencias de atributo, entrada de diccionario de extension o XData de efecto visual no clasificada, propiedad de representacion fuera del inventario de CT-49 u otro estado visual no modelado. La copia conserva la presentacion completa de la fuente o no se crea | fail-closed antes de MUTATE (E4), sin aviso sustitutivo | ST-18, PRE-15 |
| **L-34** | **Orden visual en Model Space no demostrable**: una copia se superpone de forma visualmente material, o no clasificable, con **cualquier** objeto de Model Space posterior a su fuente en el orden efectivo (otra fuente admitida, un objeto seleccionado pero ignorado, uno no seleccionado u otro rack), o alguna huella visual o el contexto de orden no pueden garantizarse (`[V9-D01]`, `[V9-D04]`). En particular: un `XLINE` o un `RAY` posterior a una fuente hace fallar el comando aunque este lejos de la copia, mientras CT-49 no caracterice una huella o interseccion analitica fiable (`[V10-D07]`); y dos fuentes admitidas con orden relativo distinto en pantalla y en trazado cuyas copias se superponen de forma material, o no clasificable, tambien hacen fallar el comando, porque ningun orden de creacion conserva los dos (`[V10-D06]`) | fail-closed en PREPARE, antes de MUTATE (E10) | ST-19, PRE-16, PRE-18 |
| **L-35** | **Orden visual entre piezas del plan no conservado**: dos instancias de pieza, con o sin cambio de mano, cuyo orden relativo cambia en el diseño reflejado y cuyas huellas visuales se superponen de forma material, o cuya huella o superposicion no puede clasificarse (`[V9-D03]`) | fail-closed | X-22 (§11.3 punto 6.6) |
| **L-36** | **Presentacion interna regenerada bajo el contexto vigente** (`[V9-D05]`, `[V10-D05]`, `[V10-D12]`): los drawers no asignan explicitamente determinadas propiedades de las referencias internas de la copia (piezas y colocaciones de grupos) ni de sus anotaciones y cotas; CT-50 debe caracterizar que mecanismo y que valores efectivos aplica AutoCAD 2025 y que observaciones consume cada productor (`MaterializationContextReadSet`; por ejemplo `CLAYER`, `CECOLOR`, `CELTYPE`, `CELTSCALE`, `CELWEIGHT`, `CETRANSPARENCY`, `CPLOTSTYLE`, `DIMSTYLE`, `TEXTSTYLE`, escala de anotacion o registros de capa). Como `RACKEDITAR` → Actualizar, **no** se copian fisicamente desde la definicion vieja, asi que la copia puede verse distinta de una fuente dibujada con otro contexto. La presentacion exterior de la referencia se conserva (ST-13) | regenerada como Actualizar; observacion consumida cambiada antes de MUTATE o productor asimetrico ⇒ fail-closed (X-17a, E10/E11) | §8.7; ST-13; PRE-17 |
| **L-37** | **Superposiciones solo de presentacion** (`[V10-D01]`, `[V10-D07]`): la huella visual representa **ocupacion de modelo en WCS** y **no** se dilata por `LineWeight` de AutoCAD, `LWDISPLAY`, grosores de CTB/STB, milimetros de papel ni escala de trazado. El primer corte **no** garantiza conservar el orden de dibujo cuando dos objetos solo se superponen visualmente por esas anchuras de linea, de papel o de pantalla y sus soportes de modelo no se superponen. Tampoco clasifica analiticamente `XLINE` ni `RAY`: sin huella o interseccion analitica caracterizada por CT-49 su huella es `UNKNOWN` y un `XLINE` o `RAY` posterior a una fuente hace fallar el comando (L-34). Si el Owner exige la garantia para superposiciones solo de presentacion, es un cambio de alcance para una Proposal posterior | orden no garantizado para superposiciones solo de presentacion; `XLINE`/`RAY` posterior ⇒ E10 | ST-19; §11.3 puntos 5 y 6.6 |

### 1.4 Evidencia indicativa pre-G3 de la biblioteca inspeccionada

> **No es contrato universal.** Esta evidencia la obtuvieron las revisiones de Arquitecto de V5 y de V6 sobre **una**
> biblioteca concreta, identificada por su SHA-256. Otra biblioteca, u otra version de esta, puede producir otro resultado.
> **G3 es la autoridad de caracterizacion** (CT-36, CT-47) y repite la medicion en `acad.exe` con evaluacion de estados.
> O-1 acepta la politica y la envolvente de §1.3, **no** estos resultados como universales.

**Identidad de la biblioteca inspeccionada (`[V7-D17]`).**

| Campo | Valor | Papel |
|---|---|---|
| SHA-256 del contenido binario | `b4ca2248db9c3d72487ac8b5b1e5510cdd8aba231ab340541d91bebca2d560e8` (re-verificado en los preflights de V7, V8, V9 y V10) | **identidad de la evidencia** |
| Autoridad de la ruta | `BlockLibraryLocator.ResolvePath` (`BlockLibrary.cs:19-23`): `BlockLibraryPath` de la configuracion del usuario si existe; si no, `blocks-library.dwg` junto a los catalogos | diagnostico |
| Ruta configurada en la estacion inspeccionada | `D:\Base_de_datos_AutoCAD_V.0.dwg` (`%APPDATA%\RackCad\settings.json`) | diagnostico |
| Tamaño | 477 525 bytes | diagnostico |
| Fecha de modificacion | 2026-09-09 10:47:48 (−06:00) | diagnostico |
| Versionado | **no** versionada en el repositorio (`assets/blocks/` solo contiene `.gitkeep`) | contexto |

**Regla de identidad.** Antes de reutilizar esta evidencia o cualquier corpus registrado, G3 recalcula el SHA-256 del
archivo; si cambia, la evidencia previa **no** se reutiliza y se vuelve a medir. La ruta, el tamaño y la fecha solo sirven
para diagnosticar. Para una definicion de bloque ya presente en el dibujo del usuario manda el `TraceFingerprint` de esa
definicion efectiva (§11.3 punto 10), no el SHA-256 de la biblioteca externa.

**Metodo (limites incluidos).** AutoCAD 2025 core console en solo lectura sobre **copias** con el mismo SHA-256;
extraccion de las 151 definiciones, incluidos los estados anonimos guardados. La revision de V5 comparo geometria por
multiconjunto exacto a 1e-6 y por cobertura visual a 0,06", y capa y tipo de linea en las homologas. La revision de V6
extrajo ademas, por entidad, color, tipo de linea, grosor, transparencia, escala de tipo de linea, visibilidad, anchos,
grosor de extrusion, normal y capas de las referencias anidadas, y comparo con **fuentes simbolicas**,
**rutas de anidados** y, para las entidades sin continuidad demostrable, **orientacion** en coordenadas de la pieza. La
revision de V7 añadio, sobre el mismo corpus, la clase de origen de cada primitiva, el indicador `Closed` de las
polilineas, los factores de escala y la `LinetypeScale` de los ancestros, y un inventario de mecanismos (SORTENTS,
anotativos, MINSERT, XCLIP, `Plinegen` y tipos de linea). Coordenadas con 9 decimales (las desviaciones de 1e-9 no son
concluyentes, `[V6-D30]`); sin evaluar estados nuevos, sin estilo de trazado ni orden visual; **no** es CT-36. La
revision de V8 releyo el volcado de la revision de V6, que recorre subentidades con `entnext`, para contar atributos y
entidades en capa `0` (`[V9-D16]`).

**Resultados observados.**

| Grupo | Piezas | Observacion |
|---|---|---|
| Simetricas en el eje relevante | `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA` frontal y planta; los cuatro largueros (`LARGUERO_ESCALON_CAL14_3_REMACHES`, `LARGUERO_IN_OUT_C6`, `LARGUERO_ESCALON_INFINITO`, `LARGUERO_ESCALON_TROQUEL_REDONDO`) frontal y planta; `PLACA_BASE_DE_CABECERA_ATORNILLABLE…` y `PLACA_BASE_SISMICA…` (poste 3 y 4) frontal y planta; `PROTECTOR_BOTA_*` frontal; la geometria de `TARIMA_GENERICA` | 0 primitivas sin pareja, en el estado base y en los estados anonimos guardados |
| Con las reglas de V7 (revision de V6) | las 19 representaciones estructurales anteriores (sin la tarima) y 4 estados anonimos (`*U112`, `*U116`, `*U47`, `*U48`) | 0 primitivas sin pareja exigiendo fuentes simbolicas de color, tipo de linea y grosor iguales, `LinetypeScale` igual, la misma ruta de capas de anidados y, sin continuidad demostrable, la misma orientacion; las circunferencias y arcos con tipo de linea `ByLayer` de los anidados espejados conservan el sentido. `TARIMA_GENERICA` tiene ademas 34 lineas `ByLayer` en capa `0` cuya orientacion no se refleja |
| Con las reglas nuevas de V8 (revision de V7, `[V8-D23]`) | las mismas 19 representaciones estructurales y 4 estados anonimos | 0 emparejamientos entre clases de origen distintas (`SourceKind`), tambien en trazos continuos; los 16 segmentos de polilinea medidos (`LARGUERO_IN_OUT_C6_FRONTAL` y `LARGUERO_ESCALON_TROQUEL_REDONDO_FRONTAL`) tienen homologa con el mismo `Closed`; 0 primitivas sin continuidad demostrable con escala o `LinetypeScale` de ancestros distinta de su homologa o distinta de 1; 0 referencias anidadas anonimas y 0 escalas anidadas no uniformes |
| Asimetricas y emitidas con mano constante | `SEPARADOR_DE_CABECERA_FORMADA_DE_CINTA_CALIBRE_12_PLANTA` | 12/16 sin pareja en Y; anclado en `postY + troquel.Y` sin espejo (`DynamicSystemPlantaBuilder.cs:86-139`, ancla `:125`); Push Back compone esa planta; en Selectivo, `SelectivePlantaBuilder.cs:449` |
| | `PARRILLA_GENERICA` frontal y planta | 16/54 y 28/190 sin pareja; `SelectiveParrillaPlacement.cs:20-34`, sin espejo |
| | `DESVIADOR_A_3_FRONTAL`, `DESVIADOR_A_4_FRONTAL` | 29/41 y 37/41 sin pareja; `mirrored: false` (`SelectiveDesviadorDrawing.cs:31`) |
| | `DEFENSA_MONTACARGAS_FRONTAL` | 4/38 sin pareja; `mirroredX` constante por corte (`DynamicSafetyMultiViewBuilder.cs:222`) |
| Clase fuera de la allowlist | `TARIMA_GENERICA` | 4 HATCH de degradado (`CYLINDER`/`INVCYLINDER`) ⇒ `FAIL_CLOSED` por `[V6-D18]` |
| Falsos negativos por segmentacion (motivaron `[V6-D11]`) | `TRAVESANO_PARA_POSTE_OMEGA…_FRONTAL`, `SEPARADOR_DE_CABECERA…_FRONTAL`, `DESVIADOR_L_3_FRONTAL` | la linea sin pareja queda cubierta por la union de lineas a 1e-6 (en el desviador L, 6 de 8) |

**Inventario de la biblioteca inspeccionada.**

- **Clases:** LINE, ARC, CIRCLE, SPLINE, LWPOLYLINE (con bulges), HATCH (solo `TARIMA_GENERICA`, degradado) e INSERT
  (anidados estaticos, muchos en pareja con escala X −1). Ausentes: TEXT, MTEXT, ATTDEF, DIMENSION, REGION, 3DSOLID, proxy,
  IMAGE, OLE, WIPEOUT, POINT, ELLIPSE, SOLID, TRACE, POLYLINE pesada y anidados dinamicos.
- **Atributos por entidad:** 5 490 con grosor explicito y 702 `ByLayer`; 1 350 con color explicito y 4 842 `ByLayer`;
  5 528 con tipo de linea explicito y 664 `ByLayer`; ninguna entidad `ByBlock` en color o grosor; 391 con `LinetypeScale`
  distinta de 1; ninguna transparencia, entidad invisible, polilinea con ancho, grosor de extrusion ni normal −Z. Hay bloques
  con modos mixtos (por ejemplo `MENSULA_TROQUEL_REDONDO_CAL_10_PLANTA` y `SEPARADOR…_PLANTA`).
- **Mecanismos (revision de V7):** 0 `SORTENTS`, 0 objetos anotativos, 0 MINSERT y 0 filtros espaciales (XCLIP) en las
  definiciones; 40 LWPOLYLINE con `Plinegen`, todas con tipo de linea `Continuous` explicito y solo en
  `DESVIADOR_A_3_LATERAL`, `DESVIADOR_A_4_LATERAL`, los cinco `DESVIADOR_L_*_LATERAL` y tres estados anonimos (`*U85`,
  `*U87`, `*U92`), fuera de las vistas medidas; tipos de linea definidos `Continuous`, `ACAD_ISO07W100`, `LÍNEAS_OCULTAS`
  y `CENTERX2`, todos simples (sin formas ni texto).
- **Atributos y capa `0` (revision de V8, `[V9-D16]`):** 0 ATTRIB y 0 SEQEND en las 151 definiciones (el recorrido con
  `entnext` incluye subentidades), asi que ninguna referencia anidada lleva atributos; 1 435 entidades de las
  definiciones estan en capa `0` y heredan la capa de su referencia de pieza (`MaterializationContext`, §8.7).
- **Capas y sistema:** quince capas, todas con tipo de linea `Continuous`; ninguna apagada ni inutilizada; `Rieles` lleva
  la marca de inutilizada por defecto en ventanas nuevas (`70 = 2`), no esta inutilizada (`[V7-D25]` corrige V6). Tipos de
  linea por entidad `Continuous`, `ACAD_ISO07W100` y `LÍNEAS_OCULTAS`. `PSTYLEMODE = 1` (estilos dependientes del color),
  `FILLMODE = 1`, `INSUNITS = 1`. Los 151 origenes son 0.

**Consecuencia indicativa (no es resultado de G3).** Con esta biblioteca pueden fallar cerrado gran parte de:

- **Dinamico y Push Back**: el constructor crea un modulo `Separator` en cada posicion que no es cabecera
  (`DynamicRackSystemBuilder.cs:99-124`, `:219-227`) y V-VIEW-ALL exige la planta aunque no este seleccionada;
- **Selectivo con separadores** (doble profundidad), **con parrillas**, **con tarimas visibles** o **con desviador A**;
- **sistemas con esas defensas**.

Lo que queda previsiblemente dentro con esta biblioteca es el Selectivo sin esos accesorios, el Cantilever (contornos,
sin sonda) y la cabecera independiente, esta ultima pendiente de caracterizar. Las reglas nuevas de V7 y de V8 no
quitan, en esta medicion, ninguna de las piezas estructurales medidas; las de V9 y V10 (ST-19 completo, huellas visuales
de ocupacion de modelo en el contexto de evaluacion destino, frescura tras `EnsureForPlan`, orden de plan sobre pares
invertidos y read-set del contexto de materializacion) no se han medido sobre esta biblioteca y las miden CT-48, CT-49 y
CT-50. Estado: **DEFERRED TO G3** con fail-closed (X-17a, X-17b, X-22, X-23).

---

## 2. Alcance del primer corte

### 2.1 Dentro

| Kind (`Kind` del sobre) | `μ_k` canonica | Vistas admitidas | Evidencia de la vista |
|---|---|---|---|
| Selectivo (`selective`) | **RUN**: invierte frentes/postes | frontal (`Section` = fondo), planta | frontal X = run (`SelectivePostGeometry.cs:32-40`); planta X = fondo, Y = frente (`SelectivePlantaBuilder.cs:15-22`) |
| Dinamico (`dynamic`) | **RT**: invierte frentes | frontal (`Section` 0 salida / 1 entrada), planta | `RackDinamicoCommands.cs:392-393`; `DynamicSystemFrontalBuilder`/`DynamicSystemPlantaBuilder` |
| Push Back simple y compuesto (`pushback`) | **RT** | frontal (`Section` 0..3), planta — **solo** con el tope posterior inactivo en cada lado (P-04b); el tope esta **activo por defecto** (§2.3) | `PushBackSystemFrontalBuilder.cs:137-155`; `PushBackRearTope.cs:9`, `:53` |
| Cantilever (`cantilever`) | **R_X**: invierte la linea de estaciones | frontal, planta | camaras `CantileverViewPlanBuilder.cs:205-235` |
| Cabecera independiente (`cabecera`) | **R_D**: invierte la profundidad del marco | lateral, planta | `LateralHeaderLayoutBuilder.cs:49-59`; `PlantaHeaderLayoutBuilder.cs:13-21` |

### 2.2 Fuera (fail-closed o no aplica)

- Laterales de Selectivo, Dinamico, Push Back y Cantilever; la cama (`cama`) entera.
- Diseños con alguna fila `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (§7).
- Decisiones `ALLOW`/`CANONICALIZE` cuya estabilidad para todo estado acreditable del registro no puede demostrarse
  (obligacion 9b, §6.5.5).
- Metadata exterior (sobre o envoltorio) desconocida y no vacia fuera de la allowlist de portadores (§6.4).
- Espejo en sitio, borrar originales, conservar `RackId`, prompt de eje, alias.
- «Vista observada desde el lado opuesto» (cambio de modelo; futuro, §3.9).
- Paper Space, MINSERT, fuentes no canonicas (§4), definiciones con BASE movida (`Origin ≠ 0`), referencias a bloques
  dinamicos, anonimos o anotativos, y referencias fuente con estado de presentacion no transportable (ST-18).
- Cualquier transformacion de la holgura de topes (O-2, O-2B), cualquier normalizacion de `IsStandard` (H-06) y
  cualquier transformador de metadata desconocida.
- Cambios de schema, de `RackEnvelopeRestamp.cs`, de `PushBackMirror.cs`, de `WithDesign`, de los drawers y builders
  existentes y del registro de variables.
- Drive-In (no existe: `RackMainMenuWindow.xaml:139-144`) y Larguero (sin bloque RackCad).
- Arreglar los hallazgos laterales del Discovery §24: se registran en `ideas-futuras.md` al cerrar G2.

### 2.3 Fail-closed adicionales conocidos

Reducen el alcance util del primer corte sin cambiar la arquitectura. Todos se detectan **antes** de pedir la linea,
salvo los de PREPARE (re-verificacion de definiciones, ST-19 temprano y definitivo, frescura de las huellas visuales y
read-set del contexto de materializacion, E10). La vista del Owner es la envolvente de §1.3; la evidencia indicativa de
la biblioteca inspeccionada esta en §1.4.

| Caso | Motivo | Fila o regla |
|---|---|---|
| Selectivo que usa topes | holgura de 0.25" anclada a un lado (AR-01) | S-13, S-13b |
| **Push Back con tope posterior activo — ES EL ESTADO POR DEFECTO** | `PushBackRearTopeConfig` esta activo por defecto (`PushBackRearTope.cs:9`): solo se guardan desactivaciones (`OffCells`, `:41`), `Draws = !IsNone ∧ At` (`:53`), `IsNone` exige `PieceId` no en blanco y compara `PieceId.Trim()` con `"(ninguno)"` sin distinguir mayusculas (`:37-38`; `PushBackDefaults.cs:44`) y un `PieceId` en blanco o desconocido usa `LARGUERO_ESCALON_TOPE_DE_3` (`PushBackRearTopeBuilder.cs:27`, `:37-48`); el lado B tiene su propia configuracion, tambien activa (`PushBackSideDesign.cs:48`). **Afecta a la mayoria de los Push Back** | P-04, P-05, PC-08 (admitidos solo P-04b/PC-08b) |
| `OffCells` del tope posterior inactivo con indices fuera del dominio valido | la mascara latente puede volver a regir en el lado incorrecto | P-05c, PC-08c |
| **Medio frente cuyo `BeamLength` efectivo depende de una propiedad vinculada** | la normalizacion congelaria el remanente | S-02b |
| **Desviador con off-cells cuyo espacio de indices depende de ese ajuste** | remapeo inestable ante `ChangeValue` | S-14c |
| Parrilla con una fila desbordada (`count·frente > span`) | reparto apilado desde la izquierda | S-15x |
| **Parrilla con conteo por defecto cuyo ajuste depende de un vinculo sin `DEP-ROW`** | un `ChangeValue` posterior puede desbordarla | S-15d |
| **Tarimas con una fila de claro completo desbordada** | mismo reparto apilado | S-17c |
| **Tarimas cuyo ajuste depende de un vinculo sin `DEP-ROW`** | obligacion 9b | S-17d |
| **Tarimas frontales de Push Back desbordadas** (simple o compuesto, por lado) | `FrontalRow` recorta el margen a 0 y ancla la fila a la izquierda (`PushBackTarimaPlacement.cs:224`) | P-03c, PC-14b |
| Cabecera con `AutoAlternating` en un panel con diagonal y `StandardBaselineId` no vacio | semantica de «estandar» sin caracterizar (`RackFrameConfigurationFactory.cs:85`, `:226-243`) | H-06 |
| **Metadata semantica desconocida no vacia en el payload** | no hay transformador | V-META; S-24, D-18, P-11, K-15, H-09 |
| **Metadata exterior desconocida no vacia** (sobre o envoltorio) fuera de la allowlist | un build futuro puede poner alli semantica de vista, lado u orientacion | V-META; X-02b, X-03b |
| **Nombres de miembro que colisionan bajo el matching real del serializador** | el valor efectivo depende del orden de las claves | X-21 |
| **Payload legado con un miembro retirado** | dato de una version anterior; R-A: el store lo descarta y Actualizar lo limpia; R-B: el store lo conserva | X-19, X-19b |
| **Enum con valor numerico no definido** | `JsonStringEnumConverter` admite enteros sin validar | X-20 |
| **Miembro `UNSUPPORTED` no vacio segun la guarda de cobertura** | sin regla de espejo | X-16 |
| **Pieza con cambio de mano sin equivalencia visual-geometrica demostrada por la sonda** | sonda no viable, sin paridad con el materializador, sin punto de observacion soportado o sin contexto reproducible de la base auxiliar; bloques requeridos ausentes tras clonar; simetria no verificada (incluida la canonicalizacion conservadora); ruta anidada o correspondencia ambigua; eje oblicuo | X-17a |
| **Pieza con una clase o variante fuera de la politica cerrada** (subclase como `MInsertBlock`; anotativo, XCLIP, atributos o definicion XREF, overlay o dependiente anidados; grosor, normal, elevacion o ancho fuera de la tabla; tipo de linea complejo; HATCH de degradado o patron; SOLID/TRACE/HATCH no canonizable; propiedad o estado sin clasificar en el inventario cerrado) | fuera de la politica de geometria del primer corte (hoy `TARIMA_GENERICA`, §1.4) | X-17b |
| **Pieza con fuentes visuales simbolicas tipadas, `SourceKind`, `Plinegen`/`Closed`, contexto de patron o `VisibilityPath` distintos entre homologas, visibilidad no modelada u orden visual no demostrado dentro de la pieza** | la copia podria no ser el espejo visual del original en algun estado aunque hoy coincida | X-22 |
| **Pieza cuya evaluacion dinamica altera la transformacion efectiva de la referencia** | la colocacion evaluada no es la que caracteriza la sonda | X-23 |
| **Productor que aplica el contexto de materializacion de forma asimetrica entre `Π(D)` y `Π(μ_k D)`, u observacion consumida del `MaterializationContextReadSet` cambiada antes de MUTATE** | las dos generaciones y la materializacion no comparten las mismas observaciones consumidas (§8.7); un estado que ningun productor consume no invalida | X-17a; PRE-17; L-36 |
| **Orden visual entre piezas del plan no conservado** | huellas de ocupacion de modelo superpuestas de forma material entre instancias de pieza (con o sin cambio de mano) cuyo orden relativo cambia en el diseño reflejado, o huella o superposicion no clasificable, tambien al refrescar las huellas despues de `EnsureForPlan` | X-22 (§11.3 punto 6.6); E10 si aparece al refrescar (PRE-18) |
| **Estado dinamico de una pieza que depende de un vinculo sin evidencia universal** | la evidencia de un estado no cubre los estados que `ChangeValue` puede producir | S-26, S-27a..S-27c, X-17c |
| **Decision `ALLOW`/`CANONICALIZE` sin prueba de estabilidad para todo `R`** | obligacion 9b | X-18 |
| **Alguna vista admisible del rack (seleccionada o no) que no conmuta** | V-VIEW-ALL | X-15 |
| **Seleccion con algun rack que falla** | la operacion es todo-o-nada | L-27 (PDC-3, §9.1) |
| **Biblioteca efectiva distinta de la caracterizada** | la representabilidad depende de las definiciones efectivas | L-28 (§1.4) |
| Definicion fuente con `Origin ≠ 0` | BASE/BEDIT no soportado | ST-14 |
| **Referencia a bloque dinamico, anonimo o anotativo con payload RackCad** | transformacion adicional no capturada | ST-17 |
| **Referencia RackCad fuente con estado de presentacion no transportable o sin clasificar** (XCLIP, atributos, entrada de diccionario de extension o XData de efecto visual, propiedad o estado `UNSUPPORTED` o sin clasificar en el inventario cerrado de CT-49) | la copia no conservaria la apariencia exterior de la fuente | ST-18 (E4) |
| **Orden visual en Model Space no demostrable** | una copia se superpone de forma material, o no clasificable, con cualquier objeto de Model Space posterior a su fuente (otra fuente admitida, objeto seleccionado e ignorado, objeto no seleccionado u otro rack); una huella visual o el contexto de orden no pueden garantizarse; un `XLINE` o `RAY` posterior sin interseccion analitica caracterizada; o dos fuentes admitidas con orden relativo distinto en pantalla y en trazado cuyas copias se superponen de forma material o no clasificable | ST-19 (E10) |
| **Huella visual en un contexto de evaluacion destino no reproducible** | colision de nombres de bloque raiz, de bloque anidado o de simbolo que la base auxiliar no puede simular como la resolvera MUTATE (DRAWING FIRST y `Ignore`), o registro de simbolos que altera la representacion y no puede reproducirse (§11.3 punto 4) | huella `UNKNOWN`: E10 en ST-19, X-22 en el orden de plan y X-17a en piezas con cambio de mano (PRE-14) |
| **Huella visual u orden que cambian tras `EnsureForPlan`** | las entradas de alguna huella usada (`FootprintInputFingerprint`) cambian al releer el dibujo real y la huella, el orden de plan o ST-19 recalculados ya no se demuestran | E10 (PRE-18) |
| `View` o `Section` no canonizable, incluida la `View` vacia del Selectivo con `Section ≥ 0` | §3.7 | ST-15 |
| **Sobre que no se puede leer o reserializar** | residuales preexistentes F-14a y F-14b del store del sobre (I-54) | ST-2c/E2, E8 |
| Bloques de biblioteca ausentes, o definicion real distinta de la verificada, tras PREPARE | importacion best-effort (§9.5) y re-verificacion de la definicion real (§11.3) | E10, E11 |
| Registro de variables no acreditable con algun grupo Selectivo seleccionado | criterio de la autoridad efectiva (PV-5) | E7 |

---

## 3. Modelo formal normativo

### 3.1 Definiciones

Transformaciones afines del plano XY de WCS: `T(v)` traslacion, `R(α)` rotacion antihoraria, `S(a,b)` escala.

```text
G        = reflexion de la hoja respecto de la linea del usuario ℓ = (q, φ)
         = T(q) · R(2φ) · S(1,−1) · T(−q)                                      det G = −1
P        = colocacion efectiva de la referencia fuente
         = T(p) · R(θ) · S(s,s) · T(−o)        s > 0,  o = BlockTableRecord.Origin de su definicion     det P = +s²
           primer corte: solo o = 0 (ST-14), sin bloque dinamico, anonimo ni anotativo (ST-17)
           ⇒ P = T(p) · R(θ) · S(s,s)
D        = documento authored del rack (en su sustrato real, §6.2)
R        = estado acreditable del registro de variables del dibujo
Eff(D,R) = diseño efectivo que produce la autoridad efectiva del kind (§10.2)
μ_k      = reflexion semantica canonica del kind k: documento → documento
Π_(V,σ)  = plan fisico de la vista V con seccion semantica σ, en coordenadas locales de la definicion
           (piezas fisicas; anotaciones y cotas aparte, §11.6)
F        = reflexion local de la vista que expone μ_k:
           X_c = T(2c,0)·S(−1,1)   (x → 2c − x)        Y_c = T(0,2c)·S(1,−1)   (y → 2c − y)
σ'       = seccion canonica destino (§3.7)
```

Las definiciones nuevas se crean con `Origin = 0` (`LateralHeaderDrawer.cs:243`; `CantileverViewMaterializer.cs:47`), asi
que la colocacion destino `P'` no lleva termino de origen.

### 3.2 Exposicion y colocacion

```text
EXPOSICION    (V,σ) expone μ_k  ⇔  Π_(V,σ')(Eff(μ_k D, R)) ≡ F ∘ Π_(V,σ)(Eff(D, R))
              para TODO estado acreditable R de las propiedades vinculadas de las que dependa D   (≡ de §11)

COLOCACION    P' = G · P · F                    det P' = (−1)(+s²)(−1) = +s²

              F = X_c  ⇒  parte lineal de P' = R(2φ − θ + π) · S(s,s)
              F = Y_c  ⇒  parte lineal de P' = R(2φ − θ)     · S(s,s)

TEOREMA       para (V,σ) admitida:  P' · Π_(V,σ')(Eff(μ_k D, R))  =  G · P · Π_(V,σ)(Eff(D, R))   para todo R
```

**Consecuencias normativas.**

1. La geometria fisica de cada vista admitida coincide con el espejo geometrico de la hoja, **tambien despues** de un
   `ChangeValue` futuro (V3-01).
2. Los textos y las cotas **se regeneran** desde `μ_k D` y quedan legibles (principio de ADR-0031 §8).
3. La colocacion final tiene determinante positivo y la escala uniforme de la fuente. **Nunca** se persiste escala espejo.
4. `μ_k` **no** depende de `ℓ`: el documento reflejado es el mismo para cualquier linea y cualquier subconjunto de
   vistas. Por eso se calcula y verifica **antes** de pedir la linea (§9.1).
5. Una regla de `μ_k` o una normalizacion que escriba en el documento authored un valor calculado a partir de `R`
   rompe el «para todo R»: la fila correspondiente es `REQUIRES_MODEL_CHANGE` o `UNKNOWN` (§6.5).
6. Una decision `ALLOW` o `CANONICALIZE` cuyo **predicado** dependa directa o transitivamente de una propiedad vinculable
   rompe tambien el «para todo R» si `R` puede volverlo falso: obligacion 9b (§6.5.5).

### 3.3 Comprobacion algebraica y composicion con `Transform2D`

**Algebra (verificada en la revision de V1).**

```text
S(1,−1)·R(−φ) = R(φ)·S(1,−1)                    ⇒  G = T(q)·R(2φ)·S(1,−1)·T(−q)
F = X_c:  R(2φ)S(1,−1) · R(θ)S(s,s) · S(−1,1)  = R(2φ−θ)·S(−1,−1)·S(s,s) = R(2φ−θ+π)·S(s,s)
F = Y_c:  R(2φ)S(1,−1) · R(θ)S(s,s) · S(1,−1)  = R(2φ−θ)·S(s,s)
det = s² en ambos casos;  F∘F = id  ⇒  P'·Π(μD) = G·P·F·F·Π(D) = G·P·Π(D)
S(−s,−s) = R(π)·S(s,s)                          ⇒  la canonizacion ST-8 es exacta
```

**Ejemplos numericos.**

| # | Datos | Resultado | Comprobacion |
|---|---|---|---|
| E-1 frontal, `X_c` | `s = 1`, `θ = 0`, `p = (10,5)`, linea `x = 0` (`φ = 90°`), `c = 24` | `P' = T(−58,5)`, `θ' = 0` | el poste 0 original cae en `(10,5)` y su espejo en `(−10,5)`; en `μD` ese poste esta en `x = 48` y `P'(48,0) = (−10,5)` |
| E-2 planta, `Y_c` | `s = 2`, `θ = 30°`, `p = (1,2)`, linea por `(3,0)` a 45°, `c = 5`, punto `u = (4,1)` | `θ' = 60°`, `p' = G(P(F(0))) = (22.3205081, −12)` | `G(P(u)) = (10.7320508, 3.9282032)` y `P'(F(u)) = P'(4,9) = (10.7320508, 3.9282032)` |
| E-3 canonizacion | `(−2,−2)` con 30° aplicado a `(1,0)` | `(−1.732, −1)` | igual que `(2,2)` con 210° |

**Composicion.** `Transform2D.Then` lee de izquierda a derecha (`Transform2D.cs:86-93`): `A.Then(B)` aplica `A` y luego
`B`.

```text
P  = Transform2D.Scale(s).Then(Transform2D.Rotation(θ)).Then(Transform2D.Translation(p))      (o = 0)
X_c = Transform2D.MirrorAboutY.Then(Transform2D.Translation(2c, 0))
Y_c = Transform2D.MirrorAboutX.Then(Transform2D.Translation(0, 2c))
G  = ReflectionAboutLine(a, b)                     // fabrica nueva, §8.3 y G4
P' = F.Then(P).Then(G)                             // aplica F, luego P, luego G  ≡  G · P · F
```

La descomposicion de `P'` en `(posicion, rotacion, escala)` exige determinante positivo, parte lineal = rotacion ×
escala uniforme (sin cizalla) dentro de las tolerancias de §4.3, y escala igual a la de la fuente. Si falla: error de
programacion, no clasificacion.

### 3.4 Lemas

- **Lema 1 (elevaciones).** La Y de una elevacion es la altura, que ninguna `μ` valida refleja. Como
  `S(1,−1) = R(π)·S(−1,1)`, cualquier `G·P` se escribe con `F = X_c`. Una elevacion solo puede exponer la `μ` que
  refleja **su** eje X local.
- **Lema 2 (planta).** La planta dibuja los dos ejes horizontales locales y puede exponer cualquiera de las dos
  reflexiones horizontales, eligiendo `F = X_c` o `F = Y_c`.
- **Corolario.** Un rack logico tiene **un** documento authored (ADR-0034 §9) y por tanto **una** `μ`. Una frontal y
  una lateral del mismo rack no pueden ser fieles a la vez.

### 3.5 Contraejemplo minimo (Selectivo frontal + lateral)

```text
Rack Selectivo: DepthCount = 1; Bays = [claro 96", claro 48"]; bota con Placement = EntryExit (solo cara cercana)
Seleccion: frontal (Section 0) + lateral (Section 0); linea vertical
Frontal fiel  ⇒ claros [48", 96"]            ⇒ exige invertir Bays (μ_RUN)
Lateral fiel  ⇒ bota en la cara lejana       ⇒ exige Placement = Rear (μ_DEPTH)
μ_RUN   no voltea la lateral: la bota queda cercana (SelectiveLateralBuilder.cs:173-198)
μ_DEPTH no invierte los claros de la frontal
⇒ no existe D' unico con det P' > 0 para ambas vistas.  ∎
```

Fisicamente `μ_DEPTH = Rot_π ∘ μ_RUN`: los dos describen el mismo rack. El conflicto es de **punto de vista**: RackCad
no puede dibujar una elevacion «vista desde el lado opuesto». Bajo la admisibilidad estricta (§3.8), la lateral no
expone `μ_RUN` y **falla cerrado**.

### 3.6 Reflexion canonica por kind

| Kind | `μ_k` | Por que ese eje |
|---|---|---|
| Selectivo | RUN | Es el eje de la frontal, que el editor obliga a insertar primero (`RackSelectiveWindow.xaml.cs:2145-2153`); conserva «fondo 0 = frente» y evita los casos no representables de profundidad (herencias, diagonales) |
| Dinamico | RT | La salida esta fijada en X=0 (`DynamicLoadBeamGeometry.cs:166-190`): la reflexion de profundidad no es almacenable |
| Push Back simple y compuesto | RT | El lado A siempre existe y la fisica asume flujo hacia +X (`PushBackElevations.cs:166-177`); bajo RT **no** se intercambian A y B |
| Cantilever | R_X | No toca los lados ±Y ni el traslape derivado (`CantileverLineResolver.cs:271-277`) |
| Cabecera | R_D | Su elevacion principal es la lateral (eje profundidad); R_F no produciria cambio visible |
| Cama | ninguna | Ningun campo persistido expresa el lado del tope (`FlowBedLateralBuilder.cs:34-66`) |

### 3.7 Vistas, secciones y exposicion (`SectionRaw → semantica → canonica`)

**Regla.** Por kind, la `View` y la `Section` crudas del sobre se decodifican con la **misma lectura que la
produccion**:

1. si la decodificacion es **inequivoca y ya la soporta produccion** —y el estado crudo pudo producirlo alguna version
   real—, `σ'` es la seccion **canonica** que produccion escribe al redibujar (`OperationalDisposition = CANONICALIZE`);
2. si no es inequivoca, si ninguna version pudo producirla o si produccion la rechaza, la referencia **falla cerrado**
   (toda la operacion, PDC-3).

La `View` canonica es la constante de `RackEmbedDocument` (`frontal`, `lateral`, `planta`), comparada sin mayusculas
como en produccion. Una `View` **no vacia y no reconocida** falla cerrado en todos los kinds. **Selectivo:** la `View`
vacia solo es legado inequivoco con `Section = −1`, porque `View` aparecio (`6023572`) antes que `Section` (`4d2ca82`):
ningun build escribio `View` vacia con `Section ≥ 0`. `c` es el centro del **tramo fisico del eje reflejado** segun las
convenciones de cada builder; el valor entre parentesis es el candidato de V1 y lo fija CT-05.

| Kind | View canonica | `SectionRaw` | `SemanticSection` | `TargetSectionCanonical` | `μ_k` | Expone | `F` | `c` (candidato V1) | Autoridad de produccion |
|---|---|---|---|---|---|---|---|---|---|
| selective | frontal (`View = frontal`) | `k`, con `0 ≤ k < fondos` | fondo `k` | `k` | RUN | **si** | `X_c` | centro del tramo del fondo `k` (ultimo poste / 2) | `RackSelectivoCommands.cs:126`, `:168`, `:177`; `ProjectVariableMutationExecutor.cs:186`, `:216` |
| selective | frontal (`View = frontal` o vacia) | `−1` (legado documentado) | fondo 0 | `0` | RUN | **si** | `X_c` | idem `k = 0` | `RackSelectivoCommands.cs:155`, `:168`; `ProjectVariableMutationExecutor.cs:185-186` |
| selective | `View` vacia | `≥ 0` | combinacion que ningun build escribio | — | — | **fail-closed** | — | — | produccion la lee como frontal (`RackSelectivoCommands.cs:126`); historial `6023572` / `4d2ca82` |
| selective | frontal | `< −1` | coercion a 0 sin legado documentado | — | — | **fail-closed** | — | — | produccion coerciona (`:168`) |
| selective | frontal | `≥ fondos` | vista huerfana | — | — | **fail-closed** | — | — | `RACKEDITAR` la borra como fantasma (`RackSelectivoCommands.cs:169-173`); el ejecutor aborta (`ProjectVariableMutationExecutor.cs:202-205`) |
| selective | planta | cualquiera | planta | `−1` | RUN | **si** | `Y_c` | centro del tramo del grid maestro (ultimo poste / 2) | reescribe `−1` (`RackSelectivoCommands.cs:216`; `ProjectVariableMutationExecutor.cs:216`) |
| selective | lateral | poste `p` | poste | — | — | no (**MC**) | — | — | — |
| dynamic | frontal | `0` / `1` | salida / entrada | igual | RT | **si** | `X_c` | centro del tramo de frentes (ultimo poste / 2) | `RackDinamicoCommands.cs:220-224`, `:392-393` |
| dynamic | frontal | fuera de `{0, 1}` | coercion a salida sin legado documentado | — | — | **fail-closed** | — | — | produccion coerciona y reescribe 0 (`:222-224`) |
| dynamic | planta | cualquiera | planta | `−1` | RT | **si** | `Y_c` | centro del tramo de frentes | reescribe `−1` (`:211`) |
| dynamic | lateral o `View` vacia | poste `p` | poste (legado sin `View` = primer corte) | — | — | no (**MC**) | — | — | `:235-239` |
| pushback | frontal | `0..3` (compuesto) o `0..1` (simple) | (corte, lado) | igual | RT | **si** | `X_c` | centro del tramo de frentes (reticula compartida) | `RackPushBackCommands.cs:254-258`; `PushBackSystemFrontalBuilder.cs:137-155` |
| pushback | frontal | `2..3` en rack simple | lado B inexistente | — | — | **fail-closed** | — | — | el rack simple ignora el lado (`PushBackSystemFrontalBuilder.cs:43-45`) |
| pushback | frontal | fuera de `0..3` | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`RackPushBackCommands.cs:218`, `:427-450`) |
| pushback | planta | `−1` | planta | `−1` | RT | **si** | `Y_c` | centro del tramo de frentes | `:441-444` |
| pushback | planta, `View` vacia o no reconocida | `≠ −1` o cualquiera | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`:427-450`) |
| pushback | lateral | poste `p` | poste | — | — | no (**MC**) | — | — | — |
| cantilever | frontal | `−1` | linea | `−1` | R_X | **si** | `X_c` | `−(N−1)·s/2` | `RackCantileverCommands.cs:530-540`; `CantileverViewPlanBuilder.cs:237-239` |
| cantilever | planta | `−1` | linea | `−1` | R_X | **si** | `X_c` | `−(N−1)·s/2` | idem |
| cantilever | frontal o planta, `View` vacia o no reconocida | `≠ −1` o cualquiera | invalida | — | — | **fail-closed** | — | — | `RACKEDITAR` aborta (`RackCantileverCommands.cs:501-540`) |
| cantilever | lateral | estacion `k` | estacion | — | — | no (**MC**) | — | — | `CantileverViewPlanBuilder.cs:213-215` |
| cabecera | lateral (tambien `View` vacia) | cualquiera | marco | `−1` | R_D | **si** | `X_c` | `Depth / 2` | `RackCabeceraCommands.cs:197`, `:239-240` (no-planta = lateral; reescribe `−1`) |
| cabecera | planta | cualquiera | marco | `−1` | R_D | **si** | `X_c` | `Depth / 2` | idem |
| cama | (lateral) | `−1` | riel | — | ninguna | no (**MC**) | — | — | `FlowBedLateralBuilder.cs:34-66` |

**Notas.**

- Eje X de cada vista: Selectivo frontal = run; Selectivo planta X = fondo, Y = run; Dinamico y Push Back frontal =
  frentes, planta X = profundidad, Y = frentes; Cantilever frontal = −X (camara con `right = up × forward`,
  `CantileverViewPlanBuilder.cs:209-211`; `Spatial3D.cs:186-197`), planta = (−X, +Y); cabecera lateral = profundidad con
  el poste izquierdo en 0 y el derecho espejado en `Depth`, planta X = fondo.
- En Push Back los cuatro cortes comparten la reticula de columnas; bajo RT **no** se intercambian A y B.
- Que produccion coercione una seccion **no** la vuelve inequivoca. La planta y la cabecera se canonizan a `−1` porque
  tienen un unico significado posible, no por la coercion.
- La canonizacion se **reporta** en el resumen del comando (vista, seccion cruda y canonica).

### 3.8 Admisibilidad estricta y verificacion de todas las vistas admisibles

```text
Para cada referencia seleccionada r de un grupo con kind k:
  si no existe reflector registrado para k                             ⇒ FALLO (E2)
  si (View(r), Section(r)) no decodifica segun §3.7                     ⇒ FALLO de TODA la operacion (E4)
  si la vista semantica de r no expone μ_k                               ⇒ FALLO de TODA la operacion (E5)

Para cada rack logico reflejado (grupo), con D' = μ_k D:
  A_k(D') = TODAS las (View, Section canonica) admisibles que el rack podria materializar despues con Insertar
  para cada (V, σ) ∈ A_k(D'): exigir la conmutacion de §3.2 (V-VIEW-ALL, §6.6)
  alguna (V, σ) ∈ A_k(D') no conmuta                                    ⇒ FALLO del rack logico (E6), aunque no este seleccionada
No hay interseccion de ejes: μ_k es unica por kind.
```

`A_k` se calcula **solo** desde el diseño (operacion pura): no busca hermanas en el dibujo, no las añade a la seleccion y
no crea vistas.

| Kind | `A_k(D')` |
|---|---|
| Selectivo | frontal de cada fondo `k` con `0 ≤ k < SelectiveDepthLayout.Count(sistema)`; planta |
| Dinamico | frontal salida (`0`); frontal entrada (`1`); planta |
| Push Back | frontales semanticamente validas para ese diseño (simple: `0`, `1`; compuesto: `0..3`); planta |
| Cantilever | frontal; planta |
| Cabecera | lateral; planta |

### 3.9 Alternativas registradas, no adoptadas

- **ALT-F — fidelidad fisica:** admitir las vistas que no exponen `μ_k` con `P' = G·P·X_c`: se refleja la huella y el
  contenido no se voltea. Fisicamente correcto pero graficamente distinto de MIRROR. Requiere decision del Owner.
- **ALT-L — «vista desde el lado opuesto»:** propiedad de vista en el sobre que el builder respete con anotaciones
  legibles. Cambio de modelo con ADR propio. Es la via para laterales, la vista primaria del Dinamico y la cama. Si se
  adopta, el miembro nuevo del sobre queda bloqueado por V-META exterior (§6.4) en todo build que no lo clasifique.
- **ALT-μ2 — segunda reflexion por kind** (intercambio A↔B en la lateral de Push Back compuesto; R_Y en la lateral de
  Cantilever de estacion sencilla): contraria a PDC-4. Registrada como futuro; no se implementa en I-52.
- **ALT-ABS — medio frente que absorbe en el primer tramo:** permitiria reflejar un medio frente cuyo ancho depende de
  una variable. Es cambio de modelo (el modelo actual siempre absorbe en el ultimo tramo); futuro.

---

## 4. Contrato de fuente, linea y tolerancias

### 4.1 Fuente (por objeto seleccionado, en SNAPSHOT/PREFLIGHT) — precedencia ejecutable

Fuente admitida = **candidato RackCad** (payload RackCad en su BTR o en su `DynamicBlockTableRecord`) que es
`BlockReference` de Model Space, no MINSERT, no dinamica, BTR no anonimo y no anotativo, `Normal = +Z`, rotacion finita,
escala uniforme, definicion con `Origin = 0`, `View`/`Section` decodificables y sin estado de presentacion no
transportable (ST-18).

**Principio (V4-07).** Primero se reconoce si el objeto es candidato RackCad. Un objeto que claramente no lo es se ignora
con aviso y **nunca** aborta la operacion, cualquiera sea su escala, rotacion, `Normal`, `Origin` o tipo (MINSERT
incluido). Las restricciones estrictas solo se evaluan sobre candidatos RackCad.

```text
Para cada objeto seleccionado o, en este orden (la primera regla que decide, decide):
  C1  ST-1   o no es BlockReference                                                    → IGNORAR + aviso
  C2  clasificacion de payload: RackBlockData en BTR(o) y en DynamicBlockTableRecord(o), solo lectura
        ST-2   ninguno lleva payload RackCad                                           → IGNORAR + aviso
        ST-2b  ademas, BTR(o) contiene referencias DIRECTAS con payload RackCad          → IGNORAR + aviso especifico
               («bloque de usuario con racks anidados: no se refleja»); no se expande, no se trata como seleccion
               fisica y no aborta
        ST-2c  payload presente pero ilegible o inutilizable                           → E2 (toda la operacion)
      ⇒ en otro caso, o es CANDIDATO RackCad
  C3  ST-4   candidato fuera de Model Space                                             → IGNORAR + aviso (decision deliberada,
             continuidad con PD-7 de I-51; no aborta por las demas restricciones de candidato)
  C4  restricciones estrictas del candidato, en este orden; la primera que falla ⇒ E4 de TODA la operacion:
        ST-3 MINSERT · ST-17 dinamica, anonima o anotativa · ST-18 presentacion no transportable o sin clasificar ·
        ST-5 Normal ·
        ST-6 rotacion · ST-10 sz < 0 · ST-9 una sola escala negativa · ST-7 escala no uniforme · ST-14 Origin ·
        ST-15 View/Section
  C5  propiedades de la fuente admitida: ST-8 (canonizacion), ST-11, ST-12, ST-13 (presentacion), ST-16
Si todos los objetos quedan ignorados en C1..C3 ⇒ E1 (fin, sin linea).
PREPARE (depende de la linea): ST-19 orden visual en Model Space frente a todo objeto posterior a cada fuente, con
         huellas de ocupacion de modelo en el contexto destino: rechazo anticipado antes de EnsureForPlan y
         decision definitiva tras releer el dibujo real (§9.1, PRE-18)
```

| # | Orden | Condicion | Resultado |
|---|---|---|---|
| ST-1 | C1 | Objeto no `BlockReference` | ignorar + aviso (clasificacion de I-51) |
| ST-2 | C2 | `BlockReference` sin payload RackCad **ni** en su BTR **ni** en su `DynamicBlockTableRecord` | ignorar + aviso; nunca aborta, sea cual sea su escala, rotacion, `Normal`, `Origin` o tipo |
| ST-2b | C2 | Ademas, su BTR contiene referencias **directas** con payload RackCad (deteccion de un nivel, sin recursion y en solo lectura) | ignorar + aviso especifico; no se expande; anidamiento mas profundo o un XREF no resuelto dan el aviso de ST-2 |
| ST-2c | C2 | Payload RackCad presente pero inutilizable: sin `Kind`, sin diseño, JSON ilegible, o `ArgumentException` del store del sobre por UTF-16 crudo invalido (F-14a de I-54) | **E2**, fail-closed de toda la operacion (una fuente RackCad ilegible nunca desaparece; precedente de I-51, `RackDuplicationPlan.cs:394-428`) |
| ST-4 | C3 | Candidato fuera de Model Space (`OwnerId` ≠ Model Space) | ignorar + aviso. **Decision deliberada (V5-16):** un RackCad fuera de Model Space no entra como fuente y no aborta por las demas restricciones de candidato (continuidad con PD-7 de I-51); un candidato **dentro** de Model Space si aplica las restricciones estrictas de C4 |
| ST-3 | C4 | Candidato `MInsertBlock` (subclase de `BlockReference`) | **fail-closed** de la operacion |
| **ST-17** | C4 | Candidato con `reference.IsDynamicBlock`, `BlockTableRecord.IsAnonymous` o definicion anotativa (`BlockTableRecord.Annotative`) | **fail-closed explicito (E4)**, con mensaje propio; nunca cae en ST-2 como «no rack» |
| **ST-18** | C4 | Candidato con estado de presentacion **no transportable, sin clasificar o `UNSUPPORTED`** (`[V8-D14]`, `[V9-D08]`): filtro espacial (XCLIP) u otra entrada de `ACAD_FILTER` en su diccionario de extension; referencias de atributo (`AttributeCollection` no vacia); cualquier otra entrada del diccionario de extension o XData de efecto visual que CT-49 no clasifique como `NON_VISUAL`; propiedad o estado de `Entity` o `BlockReference` que el inventario cerrado de CT-49 (metodo de §11.3 punto 14) no clasifique como `TRANSPORTED`, `PLACEMENT`, `COMMON_CONTEXT` o `NON_VISUAL` (por ejemplo, un material distinto del de la capa); comportamiento de transporte de ST-13 no caracterizado; u otro estado visual no modelado. Las fuentes dinamicas, anonimas o anotativas ya las cubre ST-17 | **fail-closed explicito (E4)** antes de MUTATE, con mensaje propio; **nunca** un aviso ni una copia sin esa presentacion |
| ST-5 | C4 | Angulo entre `Normal` y +Z mayor que `GeometryTolerance.Angle` | **fail-closed** |
| ST-6 | C4 | `Rotation` no finita | **fail-closed** |
| ST-10 | C4 | `sz < 0` | **fail-closed** |
| ST-9 | C4 | Exactamente una de `sx`, `sy` negativa (MIRROR nativo, semilla espejada de RACKLAYOUT o RACKDUPLICAR) | **fail-closed** |
| ST-7 | C4 | Valores absolutos de `sx`, `sy` y `sz` distintos segun la tolerancia de escala (§4.3; valor en G4) | **fail-closed** |
| ST-14 | C4 | `BlockTableRecord.Origin` de la definicion fuente con alguna componente de valor absoluto mayor que `GeometryTolerance.Length` | **fail-closed** (BASE/BEDIT movido no se soporta; `RackCloner.cs:34` copia el origen de la fuente) |
| ST-15 | C4 | `View`/`Section` que no decodifican segun §3.7 | **fail-closed**; las decodificables dan `σ'` canonica |
| ST-8 | C5 | `sx < 0` y `sy < 0` con `sz > 0` | se canoniza a `(s,s,s)` con `θ + π` (exacto, §3.3) |
| ST-11 | C5 | Escala uniforme positiva `s ≠ 1` | admitida; `P'` conserva `s` |
| ST-12 | C5 | Z de `Position` | se conserva (el plano de reflexion es vertical) |
| ST-13 | C5 | Presentacion exterior de la referencia (`[V8-D13]`, `[V9-D09]`) | la referencia nueva conserva, tipados como en §11.3 punto 6.2, `LayerId`, `Color`, `Linetype`, `LinetypeScale`, `LineWeight`, `Transparency` y `Visible` de **su** referencia fuente (capturados en SNAPSHOT), asignados explicitamente y nunca por los valores por defecto de la base; estilo de trazado: con estilos con nombre (STB) se preserva `PlotStyleName` segun contrato; con estilos dependientes del color (CTB) deriva de `Color` y **no** se asigna una propiedad independiente incompatible. La transportabilidad la confirma CT-49; lo que no pueda transportarse o no este caracterizado falla cerrado (ST-18). Las piezas internas no copian la presentacion historica de la definicion fuente: se regeneran como Actualizar, con las observaciones del contexto de materializacion que consuma cada productor (§8.7, `[V10-D05]`). RACKDUPLICAR (I-51) sigue copiando solo la capa (G-M12) |
| ST-16 | C5 | Huella de la referencia y read-set del registro (§9.2) | se capturan en SNAPSHOT y se re-verifican en PREPARE y en MUTATE |
| **ST-19** | PREPARE | Orden visual en Model Space (`[V9-D01]`, `[V9-D04]`, `[V9-D19]`, `[V10-D01]`..`[V10-D03]`, `[V10-D06]`, `[V10-D07]`): las copias se crean en el **orden efectivo relativo de sus fuentes** (orden de dibujo de Model Space con su `SortentsTable` y el contexto de orden que fije CT-49); cada copia queda despues de todo objeto existente. Para cada copia `c` de su fuente `s` y **todo** objeto existente `o` de Model Space con `o ≠ s` que va **despues** de `s` en el orden efectivo —otra fuente admitida seleccionada, un objeto seleccionado pero ignorado por C1..C3, un objeto no seleccionado, otro rack o cualquier entidad— se clasifica la superposicion de `VisualFootprint(c)` y `VisualFootprint(o)` (§11.3 punto 5) con las clases de §11.3 punto 6.5 ampliadas a Model Space (rellenos, degradados, solidos, wipeouts, imagenes, OLE, texto con mascara, transparencia no opaca y referencias que los contengan). Las huellas son **ocupacion de modelo en WCS**, sin anchuras de linea, de papel ni de pantalla (L-37): `VisualFootprint(c)` = union de las huellas de las instancias de los planes **definitivos** de cada vista seleccionada, con el nombre destino, evaluadas en el `DestinationEvaluationContext` (§11.3 punto 4) y construidas **despues** de transformar a WCS con `P'`, mas las de sus anotaciones y cotas; `VisualFootprint(o)` = ocupacion de modelo del objeto con sus registros resueltos (`Extents3d` solo como primera sobre-aproximacion declarada, sin dilatar por `LineWeight`; `XLINE` y `RAY` ⇒ `UNKNOWN` salvo huella o interseccion analitica caracterizada por CT-49). Los objetos anteriores a `s` no se clasifican; la fuente `s` es excepcion deliberada (la copia puede quedar encima de ella); las copias entre si siguen el orden de sus fuentes. Si CT-49 encuentra contextos de pantalla y de trazado con ordenes distintos, «posterior» significa posterior en cualquiera de ellos y, si dos fuentes admitidas tienen orden relativo distinto entre esos contextos y sus copias se superponen de forma material o no clasificable, no existe un orden de creacion que conserve los dos. La evaluacion anterior a `EnsureForPlan` es rechazo anticipado; la decision definitiva se toma tras releer el dibujo real, con las huellas refrescadas (PRE-18) | superposicion **material o no clasificable**, huella no disponible, infinita o dudosa, `XLINE`/`RAY` posterior sin interseccion analitica, ordenes de pantalla y de trazado contradictorios con copias superpuestas, orden efectivo no legible o huella que cambia tras importar sin re-demostrarse ⇒ **fail-closed (E10)** en PREPARE, antes de MUTATE (`[V8-D25]`) |

RackCad nunca crea referencias dinamicas, anonimas ni anotativas para sus sistemas (`LateralHeaderDrawer.cs:243`;
`CantileverViewMaterializer.cs:47`) y el barrido de envolventes ya ignora los BTR anonimos (`RackBlockFinder.cs:66`): un
rack asi solo aparece si el usuario lo transforma con BEDIT o comandos equivalentes, y sus acciones (volteo, estiramiento)
son una transformacion que `P` no captura.

**Presentacion y orden de la referencia (`[V8-D13]`, `[V8-D14]`, `[V9-D01]`, `[V9-D04]`, `[V9-D09]`,
`[V10-D01]`..`[V10-D03]`, `[V10-D06]`, `[V10-D07]`).** La apariencia exterior de la referencia RackCad fuente forma
parte del contrato. La copia conserva las propiedades de ST-13 con el comportamiento que caracterice CT-49; lo que no se
pueda transportar o no este caracterizado falla cerrado antes de MUTATE (ST-18) y nunca se sustituye por un aviso. CT-49
inventaria, con el metodo cerrado de §11.3 punto 14, las propiedades y el estado de `Entity` y `BlockReference` y los
clasifica como `TRANSPORTED` (ST-13), `PLACEMENT` (`P'`), `COMMON_CONTEXT`, `NON_VISUAL` o `UNSUPPORTED`; sin clasificar
o `UNSUPPORTED` ⇒ E4. Las piezas internas de la copia no copian la presentacion historica de la definicion fuente: se
regeneran como Actualizar, con las observaciones del contexto de materializacion que consuma cada productor (§8.7). El
orden en Model Space se demuestra en PREPARE (ST-19) con huellas visuales de **ocupacion de modelo en WCS** evaluadas en
el contexto destino: las copias entre si siguen el orden de sus fuentes; frente a un objeto anterior a la fuente la
relacion se conserva sin clasificar nada, porque la copia queda despues de ambos; frente a **todo** objeto posterior a
la fuente, seleccionado o no, admitido o ignorado, la superposicion de huellas se clasifica y, si es material o no
clasificable, la operacion falla cerrado (E10); una copia que se superpone con su propia fuente queda encima de ella,
como la copia del `MIRROR` nativo, sin relacion previa que conservar. Si dos fuentes admitidas tienen orden relativo
distinto en pantalla y en trazado y sus copias se superponen de forma material o no clasificable, ningun orden de
creacion conserva los dos: E10. La evaluacion anterior a `EnsureForPlan` solo rechaza de forma anticipada; la decision
definitiva se toma tras releer el dibujo real, con las huellas, el orden de plan y ST-19 refrescados (PRE-18). Las
superposiciones que solo existen por anchuras de linea, de papel o de pantalla no se clasifican (L-37).

### 4.2 Linea

| # | Condicion | Resultado |
|---|---|---|
| LN-1 | Dos puntos con `GetPoint`, en UCS | se convierten a WCS con `CurrentUserCoordinateSystem` (patron de `RackDuplicarCommands.cs:145`) |
| LN-2 | Eje Z del UCS no paralelo a Z de WCS | fin sin mutacion |
| LN-3 | Puntos coincidentes tras proyectar a XY (distancia ≤ `GeometryTolerance.Length`) | fin sin mutacion (se puede repetir la peticion) |
| LN-4 | Enter/Esc | fin sin mutacion |

La conversion UCS→WCS y el uso de `Matrix3d` viven **solo** en el Plugin. Application recibe dos `Point2D` de WCS.

### 4.3 Tolerancias

La autoridad del repositorio usa tolerancias **absolutas** en pulgadas y rechaza el epsilon relativo por diseño
(`Vector2D.cs:82-89`). V8 no inventa ninguna tolerancia relativa ni una tolerancia visual nueva.

| Magnitud | Autoridad existente | Valor | Uso en I-52 |
|---|---|---|---|
| Longitud | `GeometryTolerance.Length` (`Vector2D.cs:93`) | `1e-9` in | posiciones, `Origin` (ST-14), `Insertion` y `ConnectionAnchor` en V-VIEW-ALL, descomposicion de `P'`, LN-3, desbordes de fila |
| Continuidad | `GeometryTolerance.Continuity` (`:97`) | `1e-7` in | valores calculados por normalizacion (N-S02 invariante) y contornos Cantilever |
| Angulo | `GeometryTolerance.Angle` (`:100`) | `1e-9` rad | rotaciones, `Normal` (ST-5) |
| Marco ortonormal | `LocalFrame3D.OrthonormalTolerance` (`Spatial3D.cs:120`) | `1e-9` | referencia; **no** se reutiliza para escalas |
| BOM | reglas propias de cada builder y de `ConsolidatedBom` (`ConsolidatedBom.cs:62-64`, `:110-115`) | segun autoridad | V-BOM (§11.5) |
| Factor de escala (adimensional) | **no existe** | — | ST-7 y ST-8. G4 fija el valor, absoluto y no relativo, con prueba y registro |

---

## 5. Seleccion, agrupacion e identidad (reutilizacion de I-51)

### 5.1 Nucleo neutral + fachada compatible (Application)

Se extrae de `RackDuplicationPlan` (`RackDuplicationPlan.cs:225-582`) **solo** lo neutral:

- clasificacion de fuentes (cuatro clases de I-51);
- clave logica discriminada `RackId` / `Definition(handle)`;
- agrupacion y deduplicacion de definiciones conservando todas las referencias;
- consistencia de `Kind` y nombre del grupo.

| Pieza | Regla |
|---|---|
| Nucleo neutral (Application, puro) | Vocabulario de mensajes **inyectado**. Sin nombre de copia, sin `Guid`, sin tipos de AutoCAD |
| `RackDuplicationPlan` | **Fachada**: misma API publica, mismos textos **byte a byte**, mismo orden observable, misma politica de nombre «- copia», misma autoridad Selectiva. T1–T15 de I-51 sin cambios |
| Semantica observable historica | **CT-16 en G3**, sobre codigo intacto y antes de cualquier extraccion, fija: textos completos de cada mensaje visible, parametrizados (`RackDuplicationPlan.cs:326`, `:407`, `:414`, `:420`, `:425`, `:432-434`, `:450`, `:455`, `:460`, `:518-519`, `:530-532`, `:573-574`, `:580-581`, `:626`, `:632`, `:638` y la politica de `:656`); **orden de grupos**; **orden de referencias** dentro de cada grupo; **orden y contenido de avisos**; **orden y contenido de errores**. No se confia en las pruebas actuales, que usan `Contains` (`RackDuplicationPlanTests.cs:333`, `:344`, `:401`, `:432`, `:509`, `:525`). Las excepciones de programacion (`:258`, `:282`, `:364`, `:379`, `:385`) se fijan por tipo |
| Asignador de identidad | Se reutiliza su validacion (`Guid.Empty`, repeticion, igualdad con una fuente) con **politica de nombre inyectada**. RACKDUPLICAR pasa la historica; RACKMIRROR pasa `<base> - espejo` y un unico destino |
| Clone path de RACKDUPLICAR (`RackCloner`, PREPARE/MUTATE de copia) | **No** se reutiliza |

Editar `RackDuplicationPlan.cs` **es** tocar RACKDUPLICAR: antes de editarlo se coordina con I-49 (obligatorio antes de
G4), cuya condicion de parada cubre sus propios gates (I-49 V6 §12.2) y que ya registra el cruce (I-49 V6 §12.3); rigen
las guardas en RED primero, Core completo, la regresion de I-51 y M-17 en el Candidato. Si G4 no puede demostrar la
equivalencia de la fachada: **STOP → Proposal V11**. No se duplica la agrupacion en un segundo planificador. Las guardas
G-R1..G-R6 leen el codigo de `RackDuplicarCommands.cs` y `RackEnvelopeRestamp.cs`, no el de `RackDuplicationPlan.cs`
(`SelectiveDuplicationFailClosedTests.cs:323-587`).

### 5.2 SNAPSHOT propio de RACKMIRROR (Plugin)

- RACKMIRROR tiene **su propio** SNAPSHOT neutral, en un archivo nuevo del Plugin. Captura por referencia: handle,
  `OwnerId`, tipo (`MInsertBlock`), `IsDynamicBlock`, `DynamicBlockTableRecord`, `BlockTableRecord`, `IsAnonymous`,
  `Annotative`, `Position`, `Rotation`, `ScaleFactors`, `Normal`, presentacion tipada (`LayerId`, `Color`, `Linetype`,
  `LinetypeScale`, `LineWeight`, `Transparency`, `Visible` y, con STB, `PlotStyleName`; ST-13), estado no transportable
  o sin clasificar (filtro espacial, atributos, diccionario de extension, XData y el resto del inventario cerrado de
  CT-49; ST-18), orden efectivo en Model Space y su contexto (`SortentsTable` y las variables que fije CT-49; ST-19),
  observaciones candidatas del contexto de materializacion, de las que el comando conserva su read-set (§8.7), `Origin`
  de la definicion y payload. Lee el registro **una** vez por comando y registra su read-set (§9.2).
- Migrar el SNAPSHOT de RACKDUPLICAR a un helper compartido **no es obligatorio**. Solo se hace si aporta un beneficio
  claro y se demuestra la equivalencia; en ese caso: G-R1, G-R2, G-R4, G-R6 y la guarda de cableado de I-51 **vistas
  en RED primero** y reapuntadas con proteccion igual o mayor, Core completo, regresion especifica de I-51 y M-17.
- Si no se migra, `RackDuplicarCommands.cs` y las guardas de I-51 quedan **intactos** (G-M12).

### 5.3 Plan del espejo (Application, puro)

Compone el nucleo neutral y añade, en el orden normativo de §9.1: decodificacion de vista y seccion (§3.7),
transformacion fuente (§4.1), admisibilidad (§3.8), autoridad authored (§5.4), precondiciones del registro (§10.2),
fidelidad del store y metadata semantica (§6.3, §6.4), representabilidad y reflexion en el sustrato real (§6, §7),
cobertura de portadores (§6.4), V-VIEW-ALL y V-BOM (§6.6), nombre e identidad (§5.5) y composicion del sobre con
`RackEmbedComposer.Compose`. Entrada y salida son datos planos: sin `ObjectId`, sin `Matrix3d`, sin transacciones.

El plan de Application **termina en el documento compuesto**. El re-estampado de identidad lo hace
`RackEnvelopeRestamp`, que vive en el Plugin y despacha el restamp interior por `KindHandlerRegistry`
(`RackEnvelopeRestamp.cs:52-111`); su ensayo lo ejecuta el comando en PREFLIGHT, sin transaccion ni mutacion.

### 5.4 Autoridad authored del grupo (todos los kinds)

- Selectivo: `SelectiveAuthoredAuthority.IsSameAuthority` (`SelectiveAuthoredAuthority.cs:96-121`), sin cambios.
- Demas kinds: comparador **por kind** declarado por su reflector (`IsSameAuthority`), estructural sobre el sustrato del
  kind (§6.2) y **excluyendo** los metadatos propios de cada vista que `RACKEDITAR` preserva por hermana
  (`RackCommandSupport.cs:40-67`).
- Divergente o ilegible ⇒ fail-closed (E3). Nunca se elige una vista.
- Solo en el plan del espejo: **RACKDUPLICAR no cambia**.

### 5.5 Identidad y nombre

| Regla | Contenido |
|---|---|
| ID-1 | Un `NewRackId` (`Guid`) por grupo logico, compartido por todas sus vistas seleccionadas |
| ID-2 | `OldRackId → NewRackId` solo en memoria (asignacion del plan); **no** se persiste |
| ID-3 | Legacy sin `RackId`: clave `Definition(handle)` dentro del lote; la copia recibe `RackId` real (precedente I-51) |
| ID-4 | Nombre logico `<base> - espejo` identico en sobre, identidad interior y anotacion de nombre de todas las vistas del grupo |
| ID-5 | Orden obligatorio: **reflejar** en el sustrato → serializar con el store del kind → `RackEmbedComposer.Compose(sobreFuente, kind, idFuente, nombreFuente, ViewCanonica, σ', diseñoReflejado)` → `RackEnvelopeRestamp.RestampEnvelope(payload, nombreEspejo, NewRackId)` (Plugin; NI-1..NI-6 de I-51). Cumple el invariante D-21 de I-54 (mecanismo A y despues B) |
| ID-6 | `RackEnvelopeRestamp.cs` **no** se modifica (G-R5 intacta; T-GRD-03 de I-54 convive) |
| ID-7 | Referencias enlazadas (misma definicion) seleccionadas ⇒ **una** definicion nueva y N referencias |

---

## 6. Reflectores

### 6.1 Contrato conceptual comun (Application, sin tipos de AutoCAD)

Nombres **no** contractuales; las responsabilidades si. El contrato es comun; **el sustrato no**: cada adaptador de
kind trabaja sobre la autoridad real de ese kind (§6.2). No se introducen DTOs para homogeneizar interfaces.

```text
IRackDesignReflector
  Kind                                  // clave del sobre ("selective", "dynamic", ...)
  CanonicalReflection                   // RUN | RT | LINE_X | DEPTH_D | NONE
  Assess(source)        → RepresentabilityReport { por fila: RepresentabilityClass, EvidenceStatus,
                                                   OperationalDisposition, motivo atribuible }
  Reflect(source)       → ReflectionResult { payload interior reflejado, normalizaciones aplicadas,
                                             cambios authored declarados }
  DescribeView(source, view, section) → ViewExposure { decodificacion §3.7, Expone, F (Transform2D), c, σ', motivo }
  AdmissibleViews(reflected)          → A_k (§3.8)
  IsSameAuthority(sources) → bool

source = { payload interior del sobre (JSON), catalogos cargados, lectura del registro de variables (solo lectura) }
```

| Requisito | Regla |
|---|---|
| RF-1 Despacho | `KindDispatch<IRackDesignReflector>` en Application (`KindDispatch.cs:19-104`), busqueda sin mayusculas. El comando **no** contiene constantes `Kind*`, tipos concretos ni ramas por kind |
| RF-2 Sustrato | Cada adaptador lee y escribe con la **autoridad existente** de su kind (§6.2). El JSON es solo la frontera del sobre: **prohibida** la manipulacion de JSON para escribir |
| RF-3 Orden | `Assess` antes de `Reflect`. `Reflect` sobre algo cuya disposicion no sea `ALLOW`, `CANONICALIZE` o `BASELINE_LIMITED` ⇒ error de programacion |
| RF-4 Pureza | `Reflect` es puro y determinista; no lee el reloj, no genera ids y no escribe el registro |
| RF-5 Selectivo | Reflexion sobre una copia en memoria del DTO. **Prohibido `WithDesign`** (`SelectivePalletDesignDocument.cs:326-340`) y prohibido materializar vinculos |
| RF-6 Dominio | Dinamico, Push Back, Cantilever y cabecera: deserializar con la autoridad existente → reflejar el estado del kind → reconstruir con `RackProject.For*` → `WithSourceMetadataFrom(fuente)` → serializar con `RackProjectStore`. Sin DTO nuevo y sin cambio de schema |
| RF-7 Portadores | Se copian **desde el origen** dentro del sustrato (§6.4); nunca se recalculan |
| RF-8 Normalizaciones | Declaradas, reportadas y validas solo con las obligaciones 1..9 de §6.5; toda decision `ALLOW`/`CANONICALIZE` cumple ademas la obligacion 9b (§6.5.5) |
| RF-9 Fidelidad del store | V-RT-STORE (§6.3); nunca se justifica con el camino `EditX` |
| RF-10 Metadata | V-META (§6.4) sobre el contrato de serializacion real: miembro desconocido no vacio en el payload semantico, clave exterior desconocida no vacia fuera de la allowlist, miembro retirado (R-A, R-B) o colision de nombres ⇒ `UNKNOWN` |
| RF-11 Cobertura | Cada clave `(kind, ruta)` alcanzada por el cierre transitivo sobre el contrato de serializacion (§6.7) tiene exactamente una clasificacion; los enums alcanzados tienen instantanea y los enteros no definidos fallan cerrado |
| RF-12 Dependencias | Ninguna regla ni normalizacion escribe en el documento un valor calculado a partir del registro (§3.2, consecuencia 5; §6.5, obligacion 9), y ninguna decision `ALLOW`/`CANONICALIZE` depende de un predicado que `R` pueda volver falso (§3.2, consecuencia 6; §6.5.5, obligacion 9b) |
| RF-13 Declaracion V-DEP | Cada fila de §7 con `ALLOW` o `CANONICALIZE` declara sus entradas efectivas; si alguna esta en el cierre de un descriptor vinculable, tiene entrada V-DEP (§6.5.5; T-M43) |
| RF-14 Ubicacion | Los tipos nuevos del espejo viven en `RackCad.Application.Mirror` o junto a su kind; **ninguno** en `RackCad.Application.Systems.Shared`; ningun archivo de I-52 viola las guardas de texto vigentes de main ni, al reconciliar, las de I-49 (§6.7; G-M14, G-M18) |

### 6.2 Sustrato real por kind y fidelidad del store

| Kind | `Design` del sobre | Lectura | Se refleja | Escritura | Fidelidad del store (lo que conserva, pierde y normaliza el propio store) |
|---|---|---|---|---|---|
| selective | JSON de `SelectivePalletDesignDocument` | `SelectivePalletDesignStore.Deserialize` | copia en memoria del DTO | `SelectivePalletDesignStore.Serialize` | conserva todos los miembros tipados del DTO, la raiz `ExtensionData` (`SelectivePalletDesignDocument.cs:150`), `PropertyValues` con `Kind` desconocido y su `ExtensionData` (`SelectivePropertyValueDocument.cs:25-38`), `SchemaVersion` sticky (`SelectiveDesignSchema.cs:43-66`) y `DimensionViews` (`SelectivePalletDesignDocument.cs:108-116`; nulo no se escribe). Descarta al leer los miembros desconocidos anidados en tipos sin `ExtensionData` |
| dynamic | JSON de `RackProjectDocument { Kind = PalletFlow, DynamicSystem }` | `RackProjectStore.Deserialize` → `RackProject.DynamicDesign` | `DynamicRackDesign` | `RackProject.ForDynamic(diseño).WithSourceMetadataFrom(fuente)` → `RackProjectStore.Serialize` (`RackDinamicoCommands.cs:379`) | conserva `ExtensionData` y `SchemaVersion` del envoltorio (`RackProjectStore.cs:71-83`) y `DimensionViews` (`DynamicRackSystemDocument.cs:63-72`). El payload **no** tiene `ExtensionData`: descarta todo miembro desconocido del payload. Escribe explicitos los defectos de campos ausentes (`DynamicRackSystemDocument.cs:196-211`) |
| pushback | JSON de `RackProjectDocument { Kind = PushBack, PushBack }` | idem → `RackProject.PushBackDesign` | `PushBackDesign` | `RackProject.ForPushBack(diseño).WithSourceMetadataFrom(fuente)` (`RackPushBackCommands.cs:414`) | conserva envoltorio y raiz del payload (`PushBackDesignDocument.cs:75`; `SystemRegistry.Default.cs:168`) y `DimensionViews` de la estructura; descarta anidados desconocidos |
| cantilever | JSON de `RackProjectDocument { Kind = Cantilever, Cantilever }` | idem → `RackProject.CantileverLineDesign` | `CantileverLineDesign` | `RackProject.ForCantilever(diseño).WithSourceMetadataFrom(fuente)` (`RackCantileverCommands.cs:490-491`) | conserva envoltorio y raiz del payload (`CantileverLineDocument.cs:46`; `SystemRegistry.Default.cs:220-221`); descarta anidados desconocidos y los margenes retirados de troquel (`RackProjectStore.cs:374-400`) |
| cabecera | JSON de `RackProjectDocument { Kind = Selective, Header }` o cabecera legada sin `kind` | idem → `RackProject.Header` | `RackFrameConfiguration` (con `RefreshPhysicalModel`, como el store) | `RackProject.ForSelective(config).WithSourceMetadataFrom(fuente)` (`RackCabeceraCommands.cs:194`) | conserva el envoltorio. El payload `RackFrameProjectDocument` **no** tiene `ExtensionData` y escribe `SchemaVersion = "1.0"` (`RackFrameProjectDocument.cs:15-17`; `RackProjectStore.cs:56-57`); una cabecera legada sin envoltorio sale con envoltorio (`RackProjectStore.cs:313-336`) |

`RackProject.SourceDocument` es `internal` (`RackProject.cs:60`); el adaptador solo usa la costura publica
`WithSourceMetadataFrom` (`:75-79`). Si un adaptador necesitara otra costura del store: **STOP** y revision de
Arquitecto.

**Ranuras de payload inactivas del envoltorio (`[V4-D25]`).** `RackProjectStore.Serialize` escribe solo la ranura del
kind (`RackProjectStore.cs:50-61`) y la preservacion de desconocidos nunca resucita una ranura conocida inactiva
(`:64-82`): una ranura conocida inactiva no vacia se pierde tambien en `RACKEDITAR`. Es normalizacion declarada del
store (X-03c, `BASELINE_LIMITED`), no metadata desconocida.

### 6.3 Dos obligaciones distintas: V-RT-STORE y V-EDIT (C-2)

| Obligacion | Que demuestra | Linea base | Cuando |
|---|---|---|---|
| **V-RT-STORE** — fidelidad del store | que el espejo no pierde nada que el **store del kind** conserva | `LB(D) = S_k.Write(S_k.Read(D))` con `S_k` de §6.2. **No** es el camino de `RACKEDITAR` | PREFLIGHT, por rack, sobre el payload final |
| **V-EDIT = C-2** — estabilidad operativa del redibujo | que espejo → guardar/reabrir → `RACKEDITAR` → Actualizar → mismo dibujo | los cuatro productores de §8.5 | G6 (pruebas en CI) y M-4 (Owner) |

```text
Miembros(X) = conjunto de rutas de esquema (indices de lista sustituidos por []) con valor no vacio en el payload semantico X
M           = payload semantico final: reflejado → Compose → ensayo de restamp
V-RT-STORE pasa ⇔ Miembros(LB(D)) ⊆ Miembros(M) ∪ Vaciados(μ_k, N)
                ∧ S_k.Write(S_k.Read(M)) = M      (idempotencia; CT-35 la caracteriza por kind)
Vaciados(μ_k, N) = miembros que una regla o normalizacion declarada vacia (hoy: ninguno)
```

- Un defecto historico del camino `EditX` (por ejemplo, la re-congelacion de `PalletTolerance` vinculada que CT-07
  caracteriza, o la normalizacion de `WithDesign`, `SelectivePalletDesignDocument.cs:326-340`) **no** puede usarse
  para autorizar una perdida adicional del espejo: V-RT-STORE compara contra el store, no contra `EditX`.
- La metadata **desconocida** no la cubre V-RT-STORE sino V-META (§6.4).

### 6.4 Portadores y metadata semantica (V-META del payload y del exterior)

**Tres categorias.**

| Categoria | Definicion | Tratamiento |
|---|---|---|
| **Portador** (`carrier metadata`) | Miembro o clave **nombrado en la allowlist** de esta seccion, cuyo contrato es conservarse exactamente, sin interpretacion por orientacion | se copia desde el origen; nunca bloquea por si mismo |
| **Metadata semantica authored** | Todo miembro del **payload semantico** que se somete a `μ_k` | se clasifica (§6.7) y se transforma segun su regla; **desconocida y no vacia ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`** |
| **Metadata exterior** | Miembros declarados y claves de `ExtensionData` de `RackEmbedDocument` (sobre) y de `RackProjectDocument` (envoltorio) | cada miembro declarado tiene regla propia; una clave desconocida no vacia que no figure en la allowlist ⇒ **`UNKNOWN` ⇒ `FAIL_CLOSED`** |

**Ningun contenedor es portador por ser contenedor.** El sobre ya lleva semantica de vista (`View`,
`RackEmbedDocument.cs:41`; `Section`, `:47`), `Compose` copia su `ExtensionData` sin interpretarlo
(`RackEmbedComposer.cs:33`) y ALT-L (§3.9) preve una propiedad de vista en el sobre.

**Autoridad de deteccion: el contrato de serializacion real (V5-12).** «Conocido», «desconocido», «vacio» y «colision» se
deciden con el `JsonTypeInfo` que producen **las mismas** `JsonSerializerOptions` del store de cada nivel —con sus
modifiers, converters, `PropertyNameCaseInsensitive`, `[JsonIgnore]` y `[JsonExtensionData]`—, nunca solo con reflexion CLR:

| Nivel | Opciones reales |
|---|---|
| Sobre | `RackEmbedDocument.cs:70-73` (`PropertyNameCaseInsensitive = true`) |
| Selectivo | `SelectivePalletDesignStore.cs:67-76` (sin distinguir mayusculas, `WhenWritingNull`, `JsonStringEnumConverter`) |
| Envoltorio y kinds por dominio | `RackProjectStore.cs:404-423` (sin distinguir mayusculas, comentarios, comas finales, `JsonStringEnumConverter` y el modifier `DropRetiredCantileverPunchMargins`, `:386-400`, `:420-422`) |
| Cabecera | `RackFrameProjectStore.cs:137-145` |

**Colisiones (X-21).** Dos o mas claves JSON que el matching real asigna al **mismo** miembro (por ejemplo
`PalletTolerance` y `palletTolerance` con `PropertyNameCaseInsensitive = true`) ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`: el valor
efectivo dependeria del orden de las claves. Aplica a payload, sobre y envoltorio, y a CA-4.

**Allowlist de portadores (autoridad; cerrada).**

| # | Miembro o clave | Donde | Contrato | Evidencia |
|---|---|---|---|---|
| CA-1 | `SchemaVersion` del sobre | `RackEmbedDocument` | version de escritura no degradada | `RackEmbedComposer.cs:26`; `RackEmbedDocument.cs:35` |
| CA-2 | `SchemaVersion` del envoltorio | `RackProjectDocument` | no degradada via `WithSourceMetadataFrom` | `RackProjectStore.cs:81`; `RackProjectDocument.cs:18` |
| CA-3 | `PropertyValues` completo (incluidos `Kind` desconocidos, `expression` de I-49, literales congelados y `VariableId`) | `SelectivePalletDesignDocument` | **portador de almacenamiento**, con la condicion de V5-14 (debajo) | `SelectiveLinkedPropertyKernel.cs:103-150`; `SelectiveEffectiveDesignResolver.cs:115-160`; `SelectivePropertyValueDocument.cs:25-38` |
| CA-4 | `CustomProperties` (I-54) | miembro declarado del sobre en `main @ ba497f1` (I-54 integrada; `RackEmbedDocument.cs:67`); en un build anterior a I-54, clave `CustomProperties` de `ExtensionData` del sobre | **portador RACK-LEVEL** (`RackEmbedDocument.CustomProperties`; D-21 de I-54 y ADR-0039 §6; `JsonElement?` heredado y normalizado por `Compose`, `RackEmbedComposer.cs:57` @ `ba497f1`): se **preserva con la copia**; ADR-0039 aceptado en main; `WithCustomProperties` y los ejecutores de I-54 lo escriben sin cambiar ese contrato; T-GRD-02 censa la llamada a `Compose` del espejo (7 → 8) (`[V8-D19]`, `[V10-D13]`) | I-54 integrada en `main @ ba497f1` (merge del cierre `d3cc843`, Candidato `02987bd`); ADR-0039 §4 y §6 @ `ba497f1` |
| CA-4b | NOD `RACKCAD_CUSTOM_PROPERTIES` (I-54, integrada en main) | diccionario de objetos con nombre del **dibujo**, fuera del sobre (`CustomPropertiesData.DictKey`) | **Custom Properties de alcance PROYECTO**, no portador del rack (`[V9-D10]`, `[V10-D11]`): los alcances Proyecto y Rack son independientes (ADR-0039 §1, D-02.8: identidad local al alcance; §12: sin herencia ni valor por defecto entre Proyecto y Rack en V1; §3: contenedor de Proyecto); RACKMIRROR **no** lee CA-4b para reflejar el rack, **no** la copia, **no** la transforma y **no** la escribe; no entra en V-META porque no esta en el sobre; ningun archivo de I-52 llama a sus autoridades ni a su commit fisico (G-M22) | `main @ ba497f1`: `src/RackCad.Plugin/CustomPropertiesData.cs` y `src/RackCad.Plugin/CustomPropertiesExecutor.cs`; ADR-0039 §1, §3 y §12 @ `ba497f1` |
| CA-5 | `DimensionViews` (I-50) | DTO Selectivo; dominio Dinamico y Push Back | `int` exacto, bits desconocidos y `null` | §10.3 |
| CA-6 | Cualquier portador integrado en `main` antes del Candidato | segun su contrato | **solo** si se añade **por nombre** a esta tabla con evidencia | §15 |

- **CA-3 no esconde semantica (V5-14).** Todo `PropertyId` **conocido** tiene descriptor vinculable con efecto semantico
  declarado, y los descriptores entran en la guarda de cobertura y en V-DEP (§6.5.5, §6.7); un descriptor nuevo ⇒ RED
  hasta clasificarse. Un `PropertyId` **desconocido** en ejecucion sigue la autoridad efectiva vigente, que nunca lo
  descarta y hace fallar la resolucion (`SelectiveLinkedPropertyKernel.cs:103-150`) ⇒ fail-closed. Un `PropertyId` que
  codifique un indice sensible a la orientacion (frente, poste, celda) deja de ser portador para esa clave y exige regla
  `TRANSFORMED` o `UNSUPPORTED` antes de integrarse.
- **CA-4 como miembro declarado y como clave de `ExtensionData`:** en `main @ ba497f1` es miembro declarado y la regla F
  de `Compose` rechaza un origen con una clave de `ExtensionData` igual sin distinguir mayusculas (E8); en un build
  anterior a I-54 era clave de `ExtensionData` y la coincidencia sigue el matching real del sobre (I-54 P-17(9)); dos o
  mas claves que coinciden son colision (X-21).
- **CA-4b no es portador.** Figura en la tabla solo para fijar que el espejo no toca las Custom Properties de alcance
  Proyecto de I-54 (`[V9-D10]`, `[V10-D11]`). Las propiedades de alcance Rack de la copia viajan en el sobre (CA-4) y no
  se resuelven contra CA-4b: son alcances separados. La allowlist de portadores sigue siendo CA-1..CA-6.
- La allowlist del `ExtensionData` del envoltorio esta **vacia**: ningun codigo productivo escribe claves en
  `ExtensionData` del sobre ni del envoltorio (solo se copian).

**Miembros declarados del exterior (regla propia; no son portadores).**

| Miembro | Regla |
|---|---|
| `RackEmbedDocument.Kind` | invariante; debe coincidir con el kind del grupo (§5) |
| `RackEmbedDocument.Id`, `Name` | identidad: restamp de copia (§5.5) |
| `RackEmbedDocument.View`, `Section` | transformadas por §3.7 |
| `RackEmbedDocument.Design` | payload del kind (tabla siguiente) |
| `RackProjectDocument.Kind` | invariante del kind |
| Ranura de payload del kind (`SelectiveRack`, `DynamicSystem`, `PushBack`, `Cantilever`, `Header`) | payload semantico |
| Ranuras conocidas **inactivas** (`FlowBed`, `Larguero` y las de otros kinds) | el store no las reescribe: normalizacion declarada (§6.2; X-03c) |

**Payload semantico por kind.**

| Kind | Payload semantico (sometido a `μ_k`) | Portadores aplicables |
|---|---|---|
| selective | JSON de `SelectivePalletDesignDocument`, raiz y anidados, **excepto** el subarbol `PropertyValues` | CA-1, CA-3, CA-4, CA-5 |
| dynamic | subarbol `DynamicSystem` | CA-1, CA-2, CA-4, CA-5 |
| pushback | subarbol `PushBack` | CA-1, CA-2, CA-4, CA-5 |
| cantilever | subarbol `Cantilever`, **excepto** los miembros retirados que el store declara y descarta (RM-5, RM-6) | CA-1, CA-2, CA-4 |
| cabecera | subarbol `Header` o la cabecera legada completa | CA-1, CA-2 (si hay envoltorio), CA-4 |

**Registro de miembros retirados (V5-12).** Un miembro que el formato actual ya no liga en esa ruta y que existio en
una version anterior. El registro los clasifica por lo que hace **hoy** el store con ellos:

| # | Ruta JSON | Retirado en | Que hace el store actual | Tratamiento |
|---|---|---|---|---|
| RM-1 | `DynamicSystem.HeaderDepthOverride` | `6f51f7a` (2026-06-20) | lo descarta al leer: `DynamicRackSystemDocument` no tiene `[JsonExtensionData]` | **R-A** |
| RM-2 | `PushBack.Composite.DefenseSideA` | `6495adc` (2026-08-29) | lo descarta: `PushBackCompositeDocument` no tiene `[JsonExtensionData]` | **R-A** |
| RM-3 | `PushBack.Composite.DefenseSideB` | `6495adc` (2026-08-29) | lo descarta | **R-A** |
| RM-4 | `PushBack.HighEndBeamPeralte` (raiz) | `eba5ac8` (2026-07-23; renombrado a `LegacyHighEndBeamPeralte`) | **lo conserva**: la raiz de `PushBackDesignDocument` tiene `[JsonExtensionData]` (`PushBackDesignDocument.cs:75-76`) y lo reescribe | **R-B** `[V5-D26]` |
| RM-5 | `Cantilever…Connection.Punches.ColumnBottomPlateEndOffset` (todo `CantileverPunchParameters`, `CantileverColumnBaseConnectionDesign.cs:38`) | declarado por el store | el modifier lo quita del contrato al leer y al escribir (`RackProjectStore.cs:373-400`) | `DERIVED` (K-17); no bloquea |
| RM-6 | `Cantilever…Connection.Punches.ColumnTopPunchOffset` (idem) | declarado por el store | idem | `DERIVED` (K-17); no bloquea |
| RM-7 | `DynamicSystem.Header` (raiz del payload Dinamico) | `4149d4d` (2026-06-20; existio desde `c1406a4` ese mismo dia y lo sustituye `Modules[].Header`) | lo descarta: `DynamicRackSystemDocument` no tiene `[JsonExtensionData]` | **R-A** `[V6-D19]` |

- **R-A (X-19):** el payload crudo aun contiene el miembro ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`, **sin limpieza automatica**, con el
  mensaje:

  > El rack contiene datos de una versión anterior que el formato actual ya no conserva. Ábrelo con RACKEDITAR y
  > Actualizar antes de reflejarlo.

- **R-B (X-19b):** ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED`, sin limpieza automatica. RACKEDITAR → Actualizar **no** elimina el miembro,
  asi que el mensaje **no** puede prometer remedio (texto propuesto; el definitivo lo fija G7 con T-M50): «El rack contiene
  un dato de una versión anterior que el formato actual conserva sin interpretar; RACKMIRROR no puede reflejarlo.»
- **Metodo y limite del barrido.** Se recorrio la historia de los 24 archivos de tipos persistidos alcanzables desde las
  raices (69 tipos) buscando propiedades publicas retiradas y ausentes hoy. No detecta un miembro **movido** de ruta sin
  cambiar de nombre: CT-32 hace el barrido definitivo por ruta JSON con el contrato de serializacion. Cualquier miembro no
  registrado sigue la regla general de V-META (`UNKNOWN`). La revision de V5 repitio el barrido sobre los tipos con
  `[JsonExtensionData]` (RM-4 es el unico R-B) y encontro RM-7, un miembro movido de ruta que confirma ese limite.

**Regla V-META.**

```text
Vacio(v)          ⇔ v es null, {} o []
Desconocido(X)    = miembros del payload semantico X que el contrato de serializacion real no liga en esa ruta,
                    que no son retirados declarados por el store (RM-5, RM-6) y que no son Vacio
                    (incluye los retirados R-A y R-B, con su mensaje)
DesconocidoExt(E) = claves de ExtensionData del sobre E y, cuando E.Design es un RackProjectDocument, de su
                    ExtensionData, que no son Vacio y no figuran en la allowlist (CA-4 con coincidencia unica)
Colision(X)       = grupos de claves JSON que el matching real asigna al mismo miembro (X-21)
V-META pasa ⇔ Desconocido(D) = ∅ ∧ DesconocidoExt(E) = ∅ ∧ Colision(D, E) = ∅
              (salvo declaracion tipada de invariancia o de transformador; vacia en el primer corte)
V-META falla ⇒ RepresentabilityClass = UNKNOWN, OperationalDisposition = FAIL_CLOSED (E6), con la ruta y el mensaje
```

- Un portador de la allowlist nunca bloquea por su contenido. Su **reescritura** si puede fallar por residuales
  preexistentes del store (F-14b de I-54): eso es E8 en el ensayo de P10, no V-META (§9.4).
- Una declaracion de invariancia o de transformador exige evidencia (CT y prueba) y vive en un unico registro tipado en
  `RackCad.Application.Mirror`; ninguna existe en el primer corte.
- Pruebas: CT-32 y T-M35 (clave desconocida vacia y no vacia, portador, `CustomProperties` antes y despues de I-54,
  colision por mayusculas, clave desconocida del envoltorio, retirados R-A y R-B) y T-M50/T-M51.

### 6.5 Normalizaciones

Una normalizacion `N` solo es valida si **todo** se cumple y se prueba:

1. `Π_(V,σ)(Eff(N D, R)) ≡ Π_(V,σ)(Eff(D, R))` para **cada** vista del kind, no solo las admitidas.
2. `BOM(Eff(N D, R)) = BOM(Eff(D, R))` con las dos comprobaciones de §11.5.
3. `PropertyValues`, literales y `VariableId` identicos.
4. Portadores de §6.4 identicos.
5. V-RT-STORE sobre `N D` (§6.3).
6. El cambio de representacion authored, si existe, se **declara** en el reporte: campo, valor anterior y nuevo.
7. El editor carga `N D` mostrando la misma configuracion efectiva (CT de UI).
8. Involucion: `Reflect(Reflect(D)) = N(D)`, con `N` la forma normal declarada.
9. **Conmutacion con la resolucion efectiva**: para **todo** estado acreditable `R` de las propiedades vinculadas de
   las que dependa el valor que `N` escribe,

   ```text
   N(Eff(D, R))  ≡  Eff(N(D), R)
   ```

   Si el valor que `N` escribe se calcula a partir de una propiedad vinculada, la obligacion **no** se cumple: la fila
   es `REQUIRES_MODEL_CHANGE` (el modelo no puede expresar la dependencia) o `UNKNOWN` (dependencia no demostrable).

La obligacion 9 rige las normalizaciones. Toda decision `ALLOW` o `CANONICALIZE`, sea o no normalizacion, cumple
ademas la **obligacion 9b** (§6.5.5).

#### 6.5.1 N-S02 — medio frente (Selectivo) y dependencia efectiva

```text
Bay i con Segments s_0..s_{n−1}, n ≥ 2, que RESUELVE (SelectiveMedioFrente.Resolve ≠ null, SelectiveMedioFrente.cs:55-78)
overhead = 2·(troquelX + inicioX)                                            (:10-17, :60)
r        = BeamLength_i − Σ_{j<n−1} s_j.Length − (n−1)·overhead                (remanente del ultimo tramo, :70)
Reflejado:  s'_k = s_{n−1−k}   (Loaded incluido), con
            s'_0.Length     = r            (antes derivado; ahora explicito)
            s'_{n−1}.Length = s_0.Length   (queda almacenado e ignorado por Resolve)
Forma normal N: s_{n−1}.Length := r
Involucion: Reflect(Reflect(D)) = N(D); s_0.Length se recalcula dentro de GeometryTolerance.Continuity
```

**Ancho efectivo del claro `i` — autoridad real** (`main @ 1b091be`, sin cambios desde `f8deb67`; V4-11):

```text
L(c,R)   = BeamLengthOverride_c                                        si BeamLengthOverride_c > 0
           Frente_c·max(1,n_c) + PalletTolerance(R)·(max(1,n_c) + 1)   en otro caso     (SelectiveGeometryResolver.cs:388-393)
BBL(b,R) = max(0, max_{c ∈ celdas(b)} L(c,R))                          (BayBeamLength, :369-386; claro sin celdas = 0)
Maestro  = el fondo con mas frentes; empate → el primero                (:140-144)
W_M(i,R) = BBL(claro i del maestro, R)
W(i,R)   = W_M(i,R)                                                    si W_M(i,R) > 0
           max_k BBL(claro i del fondo k, R)                           si W_M(i,R) ≤ 0   (:150-164: el gobernante CAMBIA)
todos los fondos adoptan W(i,R) y la viga gobernante que lo produjo    (:166-173)
```

El gobernante cambia cuando `W_M(i,R) ≤ 0`, **no solo** cuando el claro maestro no tiene niveles: tambien cuando tiene
niveles y ninguna celda aporta longitud positiva (sin override positivo y con `Frente·n + PalletTolerance·(n+1) ≤ 0`, por
ejemplo `Frente = 0` con tolerancia no positiva).

**Dependencia efectiva (analisis conservador).**

```text
Gov_M(i)   = celdas del claro i del fondo maestro
Gov_all(i) = celdas del claro i de todos los fondos
Dep(i) ⇔ existe un vinculo sano de una propiedad que alimenta L (hoy: PalletTolerance,
         SelectiveLinkedProperties.cs:208-213; efectivo en SelectiveGeometryResolver.cs:96, :258)
         ∧ ( (Gov_M(i) ≠ ∅ ∧ ∃ c ∈ Gov_M(i) sin BeamLengthOverride > 0)
           ∨ (Gov_M(i) = ∅ ∧ ∃ c ∈ Gov_all(i) sin BeamLengthOverride > 0) )
```

**Soundness respecto de la autoridad real.**

- `Gov_M(i) ≠ ∅` y todas sus celdas con override positivo: `W_M(i,R)` es el mayor override, constante y positivo; el
  gobernante nunca cambia y `W(i,R)` no depende de `R` ⇒ `¬Dep(i)` es exacto.
- `Gov_M(i) ≠ ∅` con alguna celda sin override: `W_M` puede variar con `R` y el gobernante puede pasar a `Gov_all(i)`
  cuando `W_M ≤ 0`; `Dep(i)` ya es verdadero, asi que el cambio de gobernante queda cubierto.
- `Gov_M(i) = ∅` (columna vacia): `W_M = 0` siempre y gobierna el mayor claro real; depende de `R` si y solo si alguna
  celda de `Gov_all(i)` no tiene override.
- Caso limite: `Gov_M(i) = ∅` con todas las celdas de `Gov_all(i)` con override ⇒ `W(i,R)` constante ⇒ `¬Dep(i)`.

| Condicion del claro dividido `i` | Clase | Evidencia | Disposicion |
|---|---|---|---|
| `¬Dep(i)`, postes `f` y `f+1` con mismo peralte y troquel efectivos | `REPRESENTABLE_BY_NORMALIZATION` | `G3_PENDING` | `CANONICALIZE` |
| `Dep(i)` | `REQUIRES_MODEL_CHANGE` | `CODE_SUPPORTED` | `FAIL_CLOSED` |
| postes `f` y `f+1` con peralte o troquel distintos | `REQUIRES_MODEL_CHANGE` | `CODE_SUPPORTED` | `FAIL_CLOSED` |
| `Segments ≥ 2` que hoy **no** resuelve | `UNKNOWN` | `CODE_SUPPORTED` | `FAIL_CLOSED` |

- **No** basta con «`PalletTolerance` vinculada ⇒ MC»: si todas las celdas del conjunto gobernante tienen
  `BeamLengthOverride > 0`, el ancho es realmente independiente y la fila sigue RN.
- `VerticalClearance` (la otra propiedad vinculable hoy) no alimenta `L`. Su cierre efectivo (§6.5.5) no contiene ningun
  simbolo de **posicion** a lo largo del eje reflejado, pero si el estado dinamico de los postes (DEP-10a..DEP-10c, `[V5-D27]`, `[V6-D20]`). Toda
  propiedad vinculable nueva se clasifica en la guarda de cobertura (§6.7) con
  su cierre; sin clasificar, **RED / STOP**.
- **Cambio authored declarado** (fila RN): el `Length` del nuevo primer tramo pasa de derivado a explicito y el `Length`
  ignorado del ultimo tramo se reescribe.
- **CT-21** (UI, BOM, round-trip, involucion), **CT-27** (dos o mas estados del registro), **CT-28** (autoridad del ancho:
  fondo maestro, columna vacia, `W_M(i,R) ≤ 0` con niveles y cambio de gobernante) y **CT-37** (diferencial 9b).

**S-14 — desviador asociado.** El indice de `DesviadorOffCells` es el orden de los postes cargados, intermedios
incluidos (`SelectiveDesviadorPlan.cs:114-148`), y los intermedios existen solo si el medio frente resuelve. Con off-cells
y algun claro dividido con `Dep(i)`, el remapeo `c → P−1−c` puede dejar de ser valido tras un `ChangeValue` que cambie
el ajuste ⇒ **S-14c = `UNKNOWN` → `FAIL_CLOSED`** (DEP-02). En la practica S-02b ya hace fallar ese rack; S-14c se declara
igual para que la regla no dependa de esa coincidencia. **CT-29** y **T-M32** con dos estados del registro.

#### 6.5.2 N-S06 — `PostPeraltes` (Selectivo)

```text
K = M + 1   (postes del grid maestro; SelectiveGeometryResolver.cs:81-88)
L = PostPeraltes
1. si L.Count < K: añadir 0.0 hasta K           marcador authored de herencia que escribe el editor (SelectiveEditorState.cs:745)
2. L'[p] = L[K−1−p]  para 0 ≤ p < K               valores tal cual: un −1 almacenado sigue significando «hereda»
3. L'[i] = L[i]      para i ≥ K                   cola que el resolvedor ignora (SelectiveGeometryResolver.cs:89-93), sin mover
Forma normal N(L) = pasos 1 y 3
Involucion: Reflect(Reflect(L)) = N(L)
```

- El store conserva la lista tal cual (`SelectivePalletDesignDocument.cs:270`, `:400-402`). **Nunca** se rellena con el
  `PostPeralte` resuelto. `PostPeralte` no es vinculable, asi que la obligacion 9 se cumple.
- **CT-22** caracteriza: ceros finales, listas cortas, valores `≤ 0` distintos de `0.0`, listas mas largas que `K`
  (el editor las recorta al abrir, `SelectiveEditorState.cs:746`) e involucion.

#### 6.5.3 N-S07 — `PostCabeceras` y `ExtraFondoPostCabeceras` (Selectivo)

```text
Marcador authored de «estandar» = null (SelectiveGeometryResolver.cs:185-205)
PostCabeceras:                rellenar con null hasta K, invertir el prefijo de K, conservar la cola ignorada
ExtraFondoPostCabeceras:
  ausente (null)              ⇒ sigue ausente: NO se crean filas
  fila k presente             ⇒ rellenar con null hasta C_k + 1 (= K, porque S-01 exige C_k = M), invertir, conservar la cola
  fila k ausente o nula       ⇒ sigue ausente o nula (una fila corta o ausente ya significa «estandar»)
Forma normal N: el relleno con null solo dentro de filas que existen
```

La distincion ausencia/fila explicita la escribe el propio store (`SelectivePalletDesignDocument.cs:259-268`) y se
preserva.

#### 6.5.4 N-H02 — `DiagonalDirection` (cabecera)

```text
UpRight (1) ↔ UpLeft (2)                                          explicitas: se intercambian
AutoAlternating (0): d = ResolveDiagonalDirection() (BracingPanel.cs:40-48); se fija swap(d)     Auto → explicita
otro valor numerico ⇒ UNKNOWN
Forma normal N: cada Auto sustituido por su direccion efectiva explicita
Involucion: Reflect(Reflect(D)) = N(D)
```

**Cambio authored declarado:** la intencion «alternar automaticamente» pasa a una direccion fija; un cambio posterior
de `Number` ya no re-alterna la copia. La cabecera no consume el registro, asi que la obligacion 9 se cumple. **CT-23**
caracteriza fisica, BOM, UI, round-trip y la relacion con `IsStandard`, `IsException` y `StandardBaselineId`. Mientras
H-06 sea `UNKNOWN`: `AutoAlternating` en un panel con diagonal **y** `StandardBaselineId` no vacio ⇒ `FAIL_CLOSED`.

#### 6.5.5 Obligacion 9b — DYNAMIC-STABILITY y autoridad V-DEP

**Obligacion 9 frente a 9b.**

| Obligacion | Rige | Enunciado |
|---|---|---|
| **9** | normalizaciones | `N(Eff(D,R)) ≡ Eff(N(D),R)` para todo `R` acreditable (§6.5) |
| **9b — DYNAMIC-STABILITY** | toda decision `REPRESENTABLE` o `REPRESENTABLE_BY_NORMALIZATION` con disposicion `ALLOW` o `CANONICALIZE`, sea o no normalizacion | si su predicado depende **directa o transitivamente** de una propiedad vinculable, su verdad —y la conmutacion de §3.2 que autoriza— se conserva para **todo** estado acreditable futuro `R`. Si no puede probarse: `UNKNOWN` o `REQUIRES_MODEL_CHANGE` ⇒ `FAIL_CLOSED` |

```text
Acreditables(D) = estados R del registro acreditables segun PV-5 en los que Eff(D,R) esta definido
Lema de definicion: Eff(μ_k D, R) definido ⇔ Eff(D, R) definido
    (μ_k no toca PropertyValues ni valores vinculados: PV-1; lo caracteriza CT-37)
9b(d): ∀ R ∈ Acreditables(D):
         p_d(Eff(D,R)) = p_d(Eff(D,R_snapshot))
         ∧ Π_(V,σ')(Eff(μ_k D,R)) ≡ F ∘ Π_(V,σ)(Eff(D,R))   para cada (V,σ) ∈ A_k que la decision d autoriza
```

Aplica, **sin limitarse a S-02**, a: fit, overflow, ancho gobernante, espacios de indices, cantidad o posicion que
dependen de geometria efectiva, **estado dinamico de las piezas cuya simetria se acepta** (§11.3 punto 12) y cualquier
regla futura que descubra la guarda de cobertura (§6.7) o CT-38.

**Autoridad V-DEP.** Cada fila de §7 con disposicion `ALLOW` o `CANONICALIZE` declara:

| Campo | Contenido |
|---|---|
| `AuthoredInputs` | miembros authored que lee |
| `EffectiveInputs` | simbolos efectivos que lee, de un vocabulario cerrado por kind (Selectivo: `L(c)`, `BBL(b)`, `W(i)`, `Gobernante(i)`, `Remanente(i)`, `PostesCargados`, `FitFila`, `ConteoParrilla`, `PostX`, `ElevacionNivel(b,l)`, `AlturaClaro(b)`, `AlturaPoste(p)`, `CabeceraPorPoste(k,p)` y `ParametroDinamico(pieza, clave)`) |
| `BindableClosure` | descriptores vinculables en cuyo cierre transitivo estan esas entradas |
| `Predicate` | el predicado que decide la fila |
| `StabilityProof` o `FailClosedReason` | la prueba de 9b o el motivo del fail-closed |

**Cierre efectivo de cada descriptor (autoridad del kind, derivada del resolvedor).**

| Descriptor (`main @ 1b091be`) | Cierre efectivo declarado |
|---|---|
| `selective.palletTolerance` (`SelectiveLinkedProperties.cs:208-213`) | `L(c)` → `BBL(b)` → `W(i)` y `Gobernante(i)` → largueros, `PostX`, `Remanente(i)`, `PostesCargados`, `FitFila`, `ConteoParrilla`, cotas horizontales y **`ParametroDinamico(larguero, LONGITUD)`** en frontal y planta (`SelectiveFrontalBuilder.cs:131`, `:466`; `SelectivePlantaBuilder.cs:393-394`) |
| `selective.verticalClearance` | `ElevacionNivel(b,l)` (`SelectiveGeometryResolver.cs:310`, `:318`, `:396-412`) → `AlturaClaro(b)` (`:271`, `:298`, `:327`; `WithOverride` en `:335-336`) → `AlturaPoste(p)` por camino (DEP-10a..DEP-10c: `SelectiveFrontalBuilder.cs:88-91`, `:144`; `SelectivePostGeometry.cs:63-67`, `:124-130`; `SelectiveCabeceraAuthority.cs:51-55`; `SelectiveDepthLayout.cs:64-112`) → **`ParametroDinamico(poste, LONGITUD)`** en la frontal (`SelectiveFrontalBuilder.cs:147-151`, `:489`). Ningun simbolo de **posicion** a lo largo del eje reflejado |

Dinamico, Push Back, Cantilever y cabecera no tienen descriptores vinculables en `main`: sus decisiones y los estados
dinamicos de sus piezas no dependen de `R`. Un descriptor nuevo en cualquier kind ⇒ **RED / STOP** hasta declarar su
cierre (T-M36, T-M43, CT-38).

**Inventario V-DEP del primer corte (Selectivo).**

| # | Fila | Entradas efectivas | Predicado | Estabilidad 9b / disposicion |
|---|---|---|---|---|
| DEP-01 | S-02a / S-02b (N-S02) | `W(i)`, `Remanente(i)` | `Resolve(claro) ≠ null`; forma normal con `r` | `¬Dep(i)` ⇒ `W` constante ⇒ estable (RN); `Dep(i)` ⇒ `MC` `FAIL_CLOSED` (obligacion 9) |
| DEP-02 | S-14b / S-14c | `PostesCargados` | espacio de indices estable | sin claro dividido con `Dep(i)` ⇒ estable; si no ⇒ `UK` `FAIL_CLOSED` |
| DEP-03 | S-17b / S-17c / S-17d: tarimas de claro completo, incluida la fila de piso (`SelectiveGeometryResolver.cs:354`) | `W(i)`, `FitFila` | `count·frente ≤ W(i) + GeometryTolerance.Length` en toda fila | `¬Dep(i)` ⇒ se evalua en el SNAPSHOT (S-17b o S-17c); `Dep(i)` ⇒ `ALLOW` solo con `DEP-ROW`; si no ⇒ S-17d `UK` `FAIL_CLOSED` |
| DEP-04 | S-15 / S-15x / S-15d: parrilla de claro completo con conteo por defecto (`SelectiveParrillaPlan.cs:107`; `SelectiveFrontalBuilder.cs:310`) | `W(i)`, `FitFila` | igual que DEP-03 | igual que DEP-03 |
| DEP-05 | S-15 con `ParrillaFrente` o `ParrillaCantidad` manual | `ConteoParrilla` | nunca desborda: el conteo se recorta con `PalletFit` (`SelectiveFrontalBuilder.cs:291-311`) | estable: `D` y `μ_k D` calculan el mismo conteo desde el mismo `W(i,R)` y el reparto es simetrico; `G3_PENDING` con CT-37 |
| DEP-06 | S-01b, S-16, cotas horizontales, largueros de planta | `W(i)`, `PostX`, `Gobernante(i)` | reparto simetrico entre postes | estable para posiciones: la formula de postes es simetrica en los dos troqueles (`SelectivePostGeometry.cs:39`) y `μ_RUN` permuta claros con los mismos datos; `G3_PENDING` con CT-37. El estado dinamico de los largueros lo gobierna DEP-09 |
| DEP-07 | filas que leen `ElevacionNivel` para **posiciones** | `ElevacionNivel` | ninguno a lo largo del eje reflejado | estable para posiciones: la Y no se refleja (Lema 1, §3.4); `G3_PENDING` con CT-37 y CT-38. El estado dinamico de los postes lo gobiernan DEP-10a..DEP-10c `[V5-D27]`, `[V6-D20]` |
| DEP-08 | S-13 / S-13b (topes) | `W(i)` | — | ya `UK` `FAIL_CLOSED` (O-2) |
| **DEP-09** | **S-26**: estado dinamico de largueros con cambio de mano, frontal y planta | `W(i)` → `ParametroDinamico(larguero, LONGITUD)` | la simetria verificada por la sonda para el estado usado vale para todo `R` | `¬Dep(i)` ⇒ `LONGITUD` constante ⇒ basta la evidencia del estado (§11.3) ⇒ `ALLOW` si `IsSymmetric`; `Dep(i)` ⇒ sin evidencia universal en `main` ⇒ S-26 `UK` `FAIL_CLOSED` `[V5-D28]` |
| **DEP-10a** | **S-27a**: poste de reticula `p` de la frontal del fondo `k` **con** cabecera por poste usable (`SelectiveCabeceraAuthority.UsableCustomAt`, `SelectiveCabeceraAuthority.cs:51-55`; vista del fondo en `SelectiveDepthLayout.cs:64-112`) | `CabeceraPorPoste(k,p)` → `ParametroDinamico(poste, LONGITUD)` (`SelectiveFrontalBuilder.cs:88-91`) | la altura es la `Height` authored de la cabecera | independiente del registro ⇒ basta la evidencia del estado (§11.3) ⇒ `ALLOW` si `IsSymmetric` `[V6-D20]` |
| **DEP-10b** | **S-27b**: poste de reticula **sin** cabecera por poste usable | `AlturaClaro` de sus claros adyacentes del fondo `k` → `AlturaPoste` (`SelectivePostGeometry.cs:63-67`, `:124-130`) → `ParametroDinamico(poste, LONGITUD)` | `IndepAlturaReticula(k,p)` | verdadero o sin vinculo sano de `selective.verticalClearance` ⇒ basta la evidencia del estado; si no ⇒ S-27b `UK` `FAIL_CLOSED` `[V5-D27]` |
| **DEP-10c** | **S-27c**: poste intermedio de un medio frente del claro `b` | `AlturaClaro(b)` o, si no es positiva, `system.Height` (`SelectiveFrontalBuilder.cs:144`) → `ParametroDinamico(poste, LONGITUD)` (`:147-151`) | `IndepAlturaIntermedio(b)` | verdadero o sin vinculo sano ⇒ basta la evidencia del estado; si no ⇒ S-27c `UK` `FAIL_CLOSED` `[V6-D20]` |

```text
Autoridad de la altura del poste en la frontal del fondo k (main @ 1b091be):
  poste de reticula p:  cabecera = SelectiveCabeceraAuthority.UsableCustomAt(system, k, p)   (Height > 0 o null; :51-55)
                        LONGITUD = cabecera != null ? cabecera.Height                        (SelectiveFrontalBuilder.cs:88-91)
                                                    : PostHeight(bays_k, p, fallback_k)       (SelectivePostGeometry.cs:63-67,
                                                                                               :124-130)
                        PostHeight = mayor Height de los claros adyacentes existentes si > 0; si no, fallback_k
  poste intermedio:     LONGITUD = bay.Height > 0 ? bay.Height : system.Height               (SelectiveFrontalBuilder.cs:144)
  altura de claro:      bay.Height = WithOverride(auto, HeightOverride)                      (SelectiveGeometryResolver.cs:271,
                                                                                               :298, :327)
                        WithOverride = HeightOverride > 0 ? HeightOverride : auto            (:335-336)
                        auto usa VerticalClearance en las separaciones                        (:310, :318, :396-412)

DEP-10a:  independiente siempre (la altura es la authored de la cabecera por poste).
DEP-10b:  IndepAlturaReticula(k,p) ⇔ p tiene al menos un claro adyacente existente en el fondo k
                                     ∧ TODOS sus claros adyacentes existentes (uno o dos) tienen HeightOverride > 0.
          Entonces PostHeight es el mayor override, positivo y constante, y el fallback nunca se usa.
DEP-10c:  IndepAlturaIntermedio(b) ⇔ el claro b que contiene el poste intermedio tiene HeightOverride > 0.
          Entonces bay.Height = HeightOverride > 0 y system.Height nunca se usa.
Predicados conservadores: un poste que no los cumple se trata como dependiente aunque su claro no use la holgura (por
ejemplo, un claro con un solo nivel de carga); CT-44 lo caracteriza. El predicado de un camino no se aplica a otro.
```

**`DEP-ROW` — prueba de ajuste para todo `R` (V5-09).**

```text
b       = cota inferior AUTORITATIVA del valor efectivo de cada propiedad que alimenta L, si existe
          (una autoridad de resolucion o de dominio que haga fallar Eff(D,R) fuera de ella)
L_lb(c) = BeamLengthOverride_c                           si BeamLengthOverride_c > 0
          Frente_c·max(1,n_c) + b·(max(1,n_c) + 1)       si no, y existe b
          −∞                                             si no
W_lb(i) = max_{c ∈ Gov_M(i)} L_lb(c)                     si Gov_M(i) ≠ ∅
          max_{c ∈ Gov_all(i)} L_lb(c)                   si Gov_M(i) = ∅
DEP-ROW(fila en el indice i) ⇔ count·frente ≤ W_lb(i) + GeometryTolerance.Length
```

- **Soundness.** Para todo `R ∈ Acreditables(D)` y toda celda `c`, `L(c,R) ≥ L_lb(c)`. Por tanto:
  - si `Gov_M(i) ≠ ∅`: `W(i,R) ≥ W_M(i,R) ≥ W_lb(i)`, porque `W_M(i,R) = max(0, max_c L(c,R))` y el fallback de
    `SelectiveGeometryResolver.cs:150-164` solo se activa con `W_M(i,R) ≤ 0` y **solo puede aumentar** `W`;
  - si `Gov_M(i) = ∅`: `W(i,R)` es el mayor `BBL` de los fondos en el indice `i` y es `≥ max_{c ∈ Gov_all(i)} L_lb(c)`.

  Con `DEP-ROW`, `count·frente ≤ W_lb(i) + tol ≤ W(i,R) + tol`: la fila no desborda en ningun `R`. La demostracion no
  necesita que `W_lb(i)` sea positivo.
- **Cota autoritativa.** `W_lb` usa `b` **solo** si existe una cota autoritativa real. En `main @ 1b091be` **no existe** (I-53 no toca estas autoridades):
  el store del registro solo exige un valor finito (`ProjectVariablesStore.cs:199-201`; `VariableDefinition.cs:53-64`),
  la preflight de mutaciones no valida signo (`ProjectVariableMutationPreflight.cs:98`), la resolucion efectiva no aplica
  dominio (`SelectiveEffectiveDesignResolver.cs:115-160`) y los controles `> 0` de RACKVARIABLES
  (`RackProjectVariablesWindow.xaml.cs:217-222`) y `< 0` del editor Selectivo (`RackSelectiveWindow.xaml.cs:2561-2565`)
  son de UI. Hoy `L_lb(c) = −∞` en toda celda sin override.
- **Si I-49 integra antes** (hoy: V6 y Amendment A1 congelados, ADR-0040 aceptado en lugar de ADR-0038, G5 sintactico
  `977f873` y G6 `5f969cc` con el nucleo semantico —enlace y evaluacion de un arbol— en su rama, sin integrar, sin
  dominio aplicado y sin `PlanReadSet`; `[V9-D12]`, `[V9-D21]`, `[V10-D14]`): el dominio que su descriptor declara
  **como dato** y aplica al valor efectivo (I-49 V6 P24.5: `>= 0` en las dos propiedades Selectivas) es la cota `b`.
  V-DEP la consume como dato; I-52 no la reimplementa ni la infiere. Se re-mide en la reconciliacion (§15.3). Un dominio
  `>= 0` **no** convierte en universal la evidencia visual-geometrica de DEP-09/DEP-10a..c (sigue siendo un continuo de
  estados).
- No se inventa ningun «valor minimo acreditado» cuando el sistema de variables o de expresiones no define dominio.

**Casos del Architect Review (normativos para G3).**

```text
Caso A — un fondo, valores positivos. Claro i sin medio frente.
  N1: PalletCount 2, Frente 40, BeamLengthOverride 70, tarimas activas.  N2: PalletCount 2, Frente 38, sin override.
  PalletTolerance vinculada a V.
  V = 4 ⇒ W = max(70, 76 + 12) = 88 ≥ 80: la fila de N1 no desborda.
  V = 1 ⇒ W = max(70, 76 + 3)  = 79 < 80: la fila de N1 se apila desde la izquierda en el original y en la copia.
  V10: Dep(i) verdadero; W_lb(i) = max(70, −∞) = 70 < 80 ⇒ ¬DEP-ROW ⇒ S-17d UNKNOWN ⇒ FAIL_CLOSED;
       ademas LONGITUD de los largueros depende de V ⇒ S-26 UNKNOWN (DEP-09).
Caso B — dos fondos, sin overrides.
  Fondo maestro, claro i: PalletCount 2, Frente 40.  Fondo 1, claro i: PalletCount 2, Frente 44, tarimas activas.
  PalletTolerance vinculada a V.
  V = 4 ⇒ W = 92 ≥ 88;  V = 1 ⇒ W = 83 < 88: la frontal del fondo 1 desborda.
  V10: Dep(i) verdadero; W_lb(i) = −∞ ⇒ ¬DEP-ROW ⇒ S-17d UNKNOWN ⇒ FAIL_CLOSED (y S-26 UNKNOWN).
Con una parrilla de conteo por defecto en esas filas el resultado es S-15d.
```

**Guardas de consumidores.**

- **T-M43:** toda fila de §7 con `ALLOW`/`CANONICALIZE` declara `EffectiveInputs`; si intersecan el cierre de algun
  descriptor, existe su entrada V-DEP con prueba o motivo; una fila sin declaracion ⇒ RED.
- **CT-38:** por descriptor, se varia su valor en el corpus de fixtures y se compara el sistema resuelto miembro a miembro
  **y los parametros dinamicos de cada instancia de plan**: todo simbolo que cambia y no esta en el cierre declarado ⇒
  RED / STOP.
- **CT-37 (C:V6-10, incondicional; Core, `[V7-D18]`):** diferencial con dos o mas estados del registro por **cada**
  descriptor (con cota autoritativa si existe; sin cota: cero, negativos y grandes), fondo maestro distinto y cambio de
  gobernante. En cada par de estados compara **siempre**, sin condicionarlo a si se cree que el descriptor puede afectarlo:
  la equivalencia visual-geometrica de todas las vistas admisibles, **V-BOM-1**, **V-BOM-2** y las firmas de estado
  (`EvaluatedStateSignature`) de las piezas con cambio de mano. Es una prueba Core: la equivalencia visual-geometrica por
  estado entra con **dobles** de `GeometrySymmetryResult`, no con AutoCAD; la evidencia real por estado es CT-36. Toda
  decision `ALLOW` en `R_a` debe conmutar en `R_b` y los parametros dinamicos que cambian deben estar cubiertos por
  DEP-09/DEP-10a..c. Un contraejemplo no previsto por V-DEP contradice V10 ⇒ **STOP → Proposal V11**.

### 6.6 Verificacion dinamica por rack (PREFLIGHT)

Ademas de las reglas estaticas de §7, para cada rack logico el plan del espejo **verifica** sobre el rack concreto, en
el orden de §9.1:

```text
V-META      metadata semantica desconocida del payload y metadata exterior fuera de la allowlist (§6.4)
V-DEP       autoridad de dependencias de §6.5.5 (obligaciones 9 y 9b; DEP-01..DEP-09, DEP-10a..c, DEP-ROW)
V-VIEW-ALL  para cada (V,σ) ∈ A_k(μ_k D):  Π_(V,σ')(Eff(μ_k D, R)) ≡ F ∘ Π_(V,σ)(Eff(D, R))     (§3.8, §11)
            ≡ = multiconjunto de piezas + orden visual relativo de los pares invertidos con huellas superpuestas
                de forma material (§11.1, §11.3 punto 6.6)
V-BOM       las dos comprobaciones de §11.5
V-RT-STORE  §6.3, sobre el payload final (tras el ensayo de restamp)
```

- V-VIEW-ALL cubre **todas** las vistas admisibles del rack, seleccionadas o no, y es operacion **pura**: no busca
  hermanas, no las añade a la seleccion y no crea vistas.
- Desde V8, sobre huellas visuales desde V9 y sobre huellas de ocupacion de modelo refrescadas tras `EnsureForPlan`
  desde V10, V-VIEW-ALL no es un multiconjunto puro para los pares de instancias de pieza, con o sin cambio de mano,
  cuyo orden relativo cambia y cuyas huellas se superponen de forma visualmente material: su orden relativo de plan se
  conserva o la vista falla (X-22; §11.3 punto 6.6, `[V8-D12]`, `[V9-D03]`, `[V10-D01]`, `[V10-D03]`).
- Cualquier discrepancia ⇒ `UNKNOWN` material ⇒ `FAIL_CLOSED` (E6), con la vista, pieza, linea o miembro que difiere en
  el diagnostico. La verificacion dinamica es la garantia **ejecutable** del fail-closed para toda vista que la copia
  pueda materializar despues con Insertar.
- `R` es el estado del SNAPSHOT; la validez para estados futuros la garantiza la autoridad V-DEP (§6.5.5, obligacion
  9b), no una prueba de todos los `R` en tiempo de ejecucion: V-VIEW-ALL solo prueba el `R` del SNAPSHOT.

### 6.7 Guarda de cobertura estructural (cierre transitivo sobre el contrato de serializacion)

Evita que un tipo, un miembro o un valor de enum nuevo se copie sin regla de espejo. La **guarda transitiva es la
autoridad**; toda tabla humana de esta Proposal es ilustrativa.

| Elemento | Contrato |
|---|---|
| Raices (minimo) | `RackEmbedDocument`, `RackProjectDocument`, `SelectivePalletDesignDocument`, `DynamicRackDesign`, `PushBackDesign`, `CantileverLineDesign`, `RackFrameConfiguration`. `RackProjectDocument` alcanza por sus ranuras los DTO de cada kind (`RackProjectDocument.cs:18-52`) |
| Autoridad del recorrido (V5-12) | para los tipos que se serializan (sobre, envoltorio, DTO y tipos de dominio embebidos en ellos): el **`JsonTypeInfo`** que producen las mismas `JsonSerializerOptions` del store (§6.4: `RackEmbedDocument.cs:70-73`, `SelectivePalletDesignStore.cs:67-76`, `RackProjectStore.cs:404-423`, `RackFrameProjectStore.cs:137-145`), con modifiers (`DropRetiredCantileverPunchMargins`), converters, `PropertyNameCaseInsensitive`, `[JsonIgnore]` y `[JsonExtensionData]`. Reflexion CLR **solo** para los miembros de dominio que no se serializan directamente (clasificados `DERIVED` o `TRANSFORMED` via su DTO) |
| Recorrido | transitivo hasta punto fijo: propiedades del contrato, tipos complejos, elementos de arrays y listas, clave y valor de diccionarios tipados, `Nullable<T>` desenvuelto y tipos anidados persistidos |
| Lista de corte (cerrada; cada corte con razon normativa) | (1) `SelectivePropertyValueDocument` y `PropertyValues`: contrato delegado en la autoridad de I-47/I-49 con la condicion de CA-3; (2) `JsonElement` y `[JsonExtensionData]`: sus claves las clasifica V-META (§6.4); (3) tipos primitivos y de framework: hojas. Un corte nuevo exige razon normativa y revision |
| Colisiones | dos propiedades del contrato cuyo nombre colisiona bajo el matching real (por ejemplo, solo difieren en mayusculas con `PropertyNameCaseInsensitive = true`) ⇒ **RED** en la guarda; en datos, X-21 |
| Enums (V5-13) | instantanea (nombre → valor) de **todos** los enums alcanzados y de los de decodificacion de secciones y de politica del espejo. Minimo: `FrameSide`, `DiagonalDirection`, `PushBackSide`, `PushBackFrontalEnd`, `DynamicRackEnd`, **`HeaderBlockRole`** (`Application/Drawing/HeaderBlockInstance.cs:7`) y **`CantileverVisualRole`** (`Application/Systems/Cantilever/CantileverVisualRole.cs:20`), mas los que la guarda encuentre (`PostSide`, `SafetySide`, `BootPlacement`, `BracingPattern`, `ExceptionType`, `FrameComponentState`, `FrameMemberType`, `PushBackCellTopology`, `PushBackRunDirection`, `DynamicRackModuleKind`, `CantileverArmSide`, `CantileverStationFaceMode`, `CantileverArmBodyArrangement`, `CantileverBraceBodyKind`, `DimensionDetail`, `DimensionViewVisibility`, `RackSystemKind`, ...). Valor nuevo, renombrado o retirado ⇒ **RED / STOP** |
| Enums en datos (V5-13) | los stores serializan enums con `JsonStringEnumConverter` (`SelectivePalletDesignStore.cs:76`, `RackProjectStore.cs:412`, `RackFrameProjectStore.cs:145`), que acepta enteros sin validar: un nombre desconocido ya hace fallar la lectura (E2), pero un **entero no definido** llega en silencio ⇒ **X-20 `UNKNOWN` ⇒ `FAIL_CLOSED`**, salvo invariante forward-compatible declarado y probado. Hoy el unico declarado es `DimensionViewVisibility` (flags; portador CA-5 con el entero exacto). Generaliza H-02c |
| Clave de clasificacion | `(kind, ruta de esquema)`, con indices de lista sustituidos por `[]` y claves de diccionario por `{}`. Un tipo alcanzado en dos kinds se clasifica en cada uno. Un ciclo de tipos exige entrada de recursion declarada; un ciclo sin declarar ⇒ RED |
| Clasificacion | cada clave exactamente **una** vez: `INVARIANT`, `TRANSFORMED` (con su fila de §7), `NORMALIZED` (con su N), `CARRIER` (con su entrada CA-n), `DERIVED` (calculado, no persistido, retirado declarado o descartado por el store) o `UNSUPPORTED` (con su fila `MC`/`UK`) |
| Naturaleza | ademas, **persistido**, **derivado/no persistido** o **portador** |
| Descriptores vinculables | cada descriptor se clasifica con su cierre efectivo, incluidos los `ParametroDinamico` que alimenta (§6.5.5); un descriptor nuevo, o un cierre que CT-38 no confirma ⇒ RED |
| Consumo en ejecucion | un miembro `UNSUPPORTED` con valor no vacio ⇒ X-16 `UNKNOWN` ⇒ `FAIL_CLOSED` (P6) |
| Donde vive | una tabla por kind en Application, junto a su reflector, y las del exterior en `RackCad.Application.Mirror`; **ninguna** en `RackCad.Application.Systems.Shared` (G-M14); sin `switch` por kind en el Plugin |
| Guardas | T-M36 (cierre y clasificacion exacta sobre el contrato), T-M44 (instantanea de enums), T-M51 (enteros no definidos y colisiones), T-M43 (V-DEP), G-M13, G-M14 y G-M18; linea base CT-31, CT-42 y CT-46 |

**Coexistencia con I-53 (baseline de main) y con las guardas de texto vigentes.**

I-53 E1 esta integrada en `main @ 1b091be`. Sus cuatro tipos `Header*` de `RackCad.Application.Systems.Shared`, sus
consumidores Selectivos (`Systems/Selective/SelectiveHeader*`, `SelectivePostTargets.cs` y miembros aditivos en
`SelectivePostGeometry`, `SelectiveCabeceraHeightReview` y `SelectiveEditorState`) y Dinamicos
(`Systems/Dynamic/DynamicHeader*`, `DynamicModuleTargets.cs`, `DynamicRackRebuild.cs`) son **baseline**: sin UI, Plugin ni
llamador productivo, y sin tocar DTO, persistencia ni dibujo. Sus guardas son de main:

- **C-08** (`HeaderBatchContractTests.cs:461-488` @ `1b091be`, `[V7-D18]`): el ensamblado del nucleo no referencia WPF ni
  AutoCAD, y todo tipo `Header*` de `Systems.Shared` figura en `SharedFoundationInspection.Roots`;
- **C-10** (`HeaderBatchContractTests.cs:492-563`): ningun tipo de **todo** `Systems.Shared` con `Coverage`,
  `StageInert`, `OmitAndReport`, `Executor`, `BatchService`, `Engine` o `Callback` en el nombre, ni miembros con
  `StageInert`, `OmitAndReport` o `CoveragePolicy`.

Guardas de texto vigentes que el codigo futuro de I-52 debe respetar (leidas en su SHA; CT-46 las relee al reconciliar):

| Guarda | SHA y lineas | Regla | Efecto en I-52 |
|---|---|---|---|
| `NO_HAY_FORMULAS_NI_REFERENCIAS_A_PROPIEDADES_DE_OTROS_RACKS` | main `1b091be`, `ProjectVariablesConformanceTests.cs:124-137` | Application, Plugin y UI no contienen `ExpressionParser`, `FormulaParser`, `DependencyGraph`, `RackPropertyReference` ni `rackProperty` (subcadena ordinal sobre el codigo sin comentarios) | ningun identificador ni texto de codigo de I-52 los contiene (por ejemplo, ningun tipo de V-DEP llamado «…DependencyGraph…») |
| la misma guarda, evolucionada por I-49 G5 | `c4880af` (tras los rebases de I-49, `fb9f630` y `977f873`, mismo contenido), `:183-203` | `ExpressionParser` pasa a permitirse solo en Application; `FormulaParser`, `DependencyGraph`, `RackPropertyReference` y `rackProperty` siguen prohibidos en las tres capas | al reconciliar con I-49 rige la version vigente en ese SHA |
| `LengthUnitsAuthorityGuardTests` (nueva) y `ExpressionCoreGuardTests`/`ExpressionParserGuardTests` extendidas | I-49 G6 `5f969cc` | ningun archivo de producto fuera de `RackCad.Application.Units` (y del alias `StructuralSectionUnits`) declara literales o conversiones de unidades de longitud (`25.4`, `InchesPerFoot`, sufijos `[mm]`); el nucleo y las unidades no nombran otras capas ni tipos dimensionales | I-52 no convierte unidades ni nombra el nucleo; al reconciliar con I-49 rige la version vigente (CT-46, G-M18, `[V9-D21]`, `[V10-D14]`) |
| `NADIE_FUERA_DE_APPLICATION_TOCA_EL_MAPA_DE_VINCULOS` | main, `:157-164` | Plugin y UI no contienen `PropertyValues` ni `SelectivePropertyValueDocument` | comando, SNAPSHOT, materializador y sonda del Plugin, y `RackCommandReference.cs` en la UI, no los nombran; V-META y los portadores viven en Application (`[V6-D29]`) |
| `NINGUN_CAMINO_DE_PRODUCCION_ABRE_UN_SELECTIVO_POR_LA_SOBRECARGA_SIN_RESOLVER` | main, `:179-206` | Plugin y UI no llaman `LoadExisting(saved)` ni `LoadExisting(document)` | I-52 no abre editores |
| `SIGUE_HABIENDO_UN_SOLO_RESOLVEDOR_EFECTIVO` | main, `:225-234` | una sola `class SelectiveEffectiveDesignResolver` | I-52 consume la autoridad efectiva existente (PV-4) y no declara otra |
| `EL_DOMINIO_ENTERO_TAMPOCO_CONOCE_EL_NUCLEO_DE_EXPRESIONES` y `ExpressionCoreGuardTests` | `c4880af` (`fb9f630` y `977f873` tras los rebases) | Domain no nombra el nucleo de expresiones; el nucleo no nombra otras capas ni motores externos | I-52 no toca Domain ni `RackCad.Application.Expressions` |
| T-GRD-01 y T-GRD-03 (I-54, en main) | `ba497f1`, `CustomPropertiesEnvelopeGuardTests.cs:22-60`, `:132-170` | en `src/` solo `RackEmbedComposer` construye un sobre; el restamp serializa el mismo objeto que deserializa | I-52 compone con `Compose(sobreFuente, ...)`, no construye sobres (ID-5) y no toca el restamp (ID-6) |
| T-GRD-02 (I-54, en main) | `ba497f1`, `:62-130`; censo en `:185-194` | censo exacto de llamadas a `RackEmbedComposer.Compose` fuera del compositor, por archivo, metodo y primer argumento (hoy siete) | la llamada del plan del espejo lo lleva a 8: el gate que la introduce la clasifica con motivo y sin debilitar la guarda (`[V10-D13]`) |
| T-GRD-04..T-GRD-07 (I-54, en main) | `ba497f1`, `CustomPropertiesEdgeGuardTests.cs:27`, `:91`, `:141`, `:173` | borde fisico de Custom Properties: fuera de su borde ningun archivo del Plugin consulta su autoridad ni confirma en Application; independencia con Project Variables; capas; clave unica del NOD en el lector fisico | ningun archivo de I-52 llama a esas autoridades ni contiene `RACKCAD_CUSTOM_PROPERTIES` (G-M22) |
| T-GRD-08 (I-54, en main) | `ba497f1`, `CustomPropertiesCommandGuardTests.cs:37-145`, `CustomPropertiesHelpCensusTests.cs:23-124`, `WindowCensusGuardTests.cs:178`, `:183`; `GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA` = 35 (`SelectiveEditorOpenTests.cs:547-554`) | censo de comandos por nombre, censo de ayuda por pares comando/alias con todo comando y alias de la ayuda registrados en el Plugin, y censo de ventanas | `RACKMIRROR` se añade por nombre, con entrada de ayuda sin alias y sin ventana; T-M16 reapunta con motivo: `Command` siempre registrado y `Alias` verificado solo si no esta vacio (`[V10-D08]`) |

Por tanto:

- I-52 **no** crea tipos en `RackCad.Application.Systems.Shared`, ni `Header*` ni de otro nombre: sus tipos viven en
  `RackCad.Application.Mirror` o junto a su kind (G-M14, CT-41);
- I-52 no modifica `SharedFoundationInspection` ni las guardas de main (incluidas las de I-53 e I-54) o de I-49; las de
  I-54 solo se reapuntan con motivo y proteccion igual o mayor (T-GRD-02 y T-GRD-08);
- G-M18 hace ejecutables las guardas de texto sobre los archivos de I-52, y CT-46 las relee en el SHA de la reconciliacion
  en lugar de copiar esta tabla;
- todo tipo o miembro persistido de cabecera o de modulo que llegue a main queda alcanzado por el cierre transitivo y en
  **RED** hasta clasificarse;
- I-53S G5 (`e528ef2`) conecta `ApplyHeaderBatch` a `RackSelectiveWindow` y **ya esta integrada en main** (`104ef3a`,
  merge de I-53S E2). Clasificacion exacta (`[V8-D17]`): materialidad del contrato de I-52 = NON-MATERIAL; materialidad
  de coordinacion = MATERIAL; baseline de C2-4 = MATERIAL / CHANGED. Su prueba
  `C2_4_I52_TheEditorStateAfterADistribution_EqualsTheReopenedDocument_InEveryView` es evidencia de I-53S y **no**
  cierra C2-4 de I-52. Como I-53S integro antes del Candidato de I-52, tras el rebase CT-26, C2-4, T-M30 y las guardas
  relacionadas se ejecutan sobre el arbol combinado y condicionan el cierre de G6/G7. Si otra integracion ocurre despues
  del Candidato, se reporta al Coordinador y se vuelve a medir contra un main que contenga I-52 (§15.3).
- I-53D G7 (`a57bd50`, rebasada sobre `ba497f1` con el contenido de `d280197`) **ya esta integrada en main** (`dad4e77`,
  merge del cierre documental `90d5c5e`; `[V10-D23]`): conecta `DynamicHeaderBatch` y `DynamicRackRebuild` a
  `RackDynamicSystemWindow` y retira de `DynamicEditorDesignAssembler` el par ordinal
  `SnapshotHeaderFondos`/`RestoreHeaderFondos`. Clasificacion exacta (`[V9-D22]`, `[V10-D15]`): materialidad del
  contrato de I-52 = NON-MATERIAL; materialidad de coordinacion = MATERIAL; baseline de C2-4 del Dinamico = MATERIAL /
  CHANGED. Como I-53D integro antes del Candidato de I-52, tras el rebase CT-26, C2-4 del Dinamico, T-M30, T-M17, T-M17b
  y las guardas relacionadas (incluidas las `D34_*` de `DynamicHeaderBatchSeamGuardTests.cs`) se ejecutan sobre el arbol
  combinado y condicionan el cierre de G6/G7 (la ventana del Dinamico es tambien la entrada de C2-2a y C2-2b).

---

## 7. Matriz de representabilidad V10

### 7.0 Taxonomia: tres dimensiones

```text
RepresentabilityClass        (normativa; PDC-7)
  REPRESENTABLE                   R
  REPRESENTABLE_BY_NORMALIZATION  RN
  REQUIRES_MODEL_CHANGE           MC
  UNKNOWN                         UK

EvidenceStatus               (de donde sale la clasificacion)
  CODE_SUPPORTED    demostrada en codigo a SHA exacto
  G3_PENDING        depende de anclajes de builders, contenido de bloques o semantica que G3 debe caracterizar
  G3_VERIFIED       confirmada por la caracterizacion de G3 (ninguna fila en V10)

OperationalDisposition       (que hace el comando)
  ALLOW             continua, sujeta a V-META, V-DEP, V-VIEW-ALL, V-BOM y V-RT-STORE
  CANONICALIZE      continua tras llevar el estado a su forma canonica o normal declarada
  BASELINE_LIMITED  continua con la fidelidad limitada a la normalizacion declarada del store
  FAIL_CLOSED       termina la operacion antes de pedir la linea
```

- Las dimensiones son independientes: una fila `R` + `G3_PENDING` + `ALLOW` no esta verificada; una fila `UK` +
  `CODE_SUPPORTED` + `FAIL_CLOSED` significa «el codigo demuestra que no hay regla aprobada».
- «Deteccion» = condicion evaluada por rack en PREFLIGHT, antes de pedir la linea. `M` = frentes del grid maestro
  (Selectivo); `N` = frentes (Dinamico/Push Back) o estaciones (Cantilever).
- Un rack admite el espejo solo si **todas** sus filas aplicables tienen disposicion distinta de `FAIL_CLOSED` y pasan
  las verificaciones dinamicas (§6.6).
- Las precondiciones operativas (fuente, diseño invalido, bloques ausentes) no son clases de representabilidad: §7.9.
- Citas contra `main @ 1b091be` (§15.1).

**Regla de cierre de G3 (V5-10).** Al cerrar G3 **ninguna** fila puede quedar `G3_PENDING` y ninguna puede quedar con
`EvidenceStatus` implicito. Cada fila termina en exactamente uno de estos estados:

| Resultado | `RepresentabilityClass` | `EvidenceStatus` | `OperationalDisposition` |
|---|---|---|---|
| Verificada | la clase correspondiente (`REPRESENTABLE` o `REPRESENTABLE_BY_NORMALIZATION`) | `G3_VERIFIED` (con la CT que la verifica) | la disposicion correspondiente (`ALLOW`, `CANONICALIZE` o `BASELINE_LIMITED`) |
| Fallo demostrado | `UNKNOWN` o `REQUIRES_MODEL_CHANGE` | `CODE_SUPPORTED` si la evidencia sale del codigo; `G3_VERIFIED` si sale de la caracterizacion | `FAIL_CLOSED` |

Antes de cerrar G3, cada fila condicional (por ejemplo S-10, D-03b, K-09, S-19, D-14, P-09, PC-10, K-10 y H-07) se parte
en filas o estados inequivocos, con una sola tripleta. Si una reclasificacion cambia materialmente el alcance (la
envolvente de O-1), el ADR, una regla de reflexion o la arquitectura ⇒ **Proposal V11**. T-M47 lo hace ejecutable desde
G5.

### 7.1 Selectivo — `μ_RUN`

| # | Propiedad / estado | Regla bajo μ_RUN | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| S-01 | Frentes por fondo con esquina (algun fondo con menos frentes que el maestro) | la alineacion al final no es expresable | MC | CODE_SUPPORTED | FAIL_CLOSED | `∃k: C_k < M` | `SelectiveGeometryResolver.cs:132-176` |
| S-01b | Orden de frentes con reticula completa (`Bays`, `ExtraFondoBays[k-1]`) | invertir `Bays` y cada lista explicita; un fondo que hereda `Bays` queda invertido por herencia | R | G3_PENDING | ALLOW | — | `SelectiveGeometryResolver.cs:211-225`; `SelectivePostGeometry.cs:32-48` |
| S-02a | Medio frente que resuelve, sin dependencia efectiva, postes `f` y `f+1` iguales | N-S02 | RN | G3_PENDING | CANONICALIZE | `¬Dep(i)` (§6.5.1) | `SelectiveMedioFrente.cs:55-78` |
| S-02b | Medio frente cuyo `BeamLength` efectivo depende de una propiedad vinculada | N-S02 congelaria el remanente | MC | CODE_SUPPORTED | FAIL_CLOSED | `Dep(i)` (§6.5.1) | `SelectiveGeometryResolver.cs:96`, `:138-176`, `:258`, `:369-392`; `SelectiveLinkedProperties.cs:208-213`; `SelectiveMedioFrente.cs:70` |
| S-02c | Medio frente con postes `f` y `f+1` de peralte o troquel distintos | overhead asimetrico | MC | CODE_SUPPORTED | FAIL_CLOSED | peralte y troquel efectivos | `SelectiveMedioFrente.cs:60`; `SelectiveDesviadorPlan.cs:214-226` |
| S-02d | `Segments ≥ 2` que hoy no resuelve | sin regla | UK | CODE_SUPPORTED | FAIL_CLOSED | `Resolve == null` | `SelectiveMedioFrente.cs:62-78` |
| S-03 | `PalletDepth`, `ExtraFondoDepths`, `CabeceraFondoOverrides`, `SeparatorLengths` | sin cambio | R | G3_PENDING | ALLOW | — | `SelectiveDepthLayout.cs:119-216` |
| S-04 | Grid maestro y desempate | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `SelectiveGeometryResolver.cs:138-176` |
| S-04b | Escalares globales: `PostId`, `PostPeralte`, `PalletTolerance`, `VerticalClearance`, `FloorBeamRise`, `DepthCount` | sin cambio (valores y vinculos intactos; los vinculos por S-18) | R | CODE_SUPPORTED | ALLOW | — | `SelectivePalletDesignDocument.cs:55-63` |
| S-05 | Celdas: pallet, larguero, `BeamLengthOverride`, `ClearOverride` | se mueven con su frente; valores intactos | R | G3_PENDING | ALLOW | — | `SelectiveCellAddress.cs:21-42`; `SelectiveFrontalBuilder.cs:138` |
| S-05b | Por frente: `FloorBeam`, `HeightOverride`, `FloorBeamRiseOverride` | se mueven con su frente | R | G3_PENDING | ALLOW | — | `SelectivePalletDesign.cs:699-720` |
| S-06 | `PostPeraltes[p]` | N-S06: marcador `0.0`, permutacion `p → M−p`, cola intacta | RN | G3_PENDING | CANONICALIZE | lista corta, larga o con `≤ 0` | `SelectivePalletDesign.cs:104-107`; `SelectiveGeometryResolver.cs:89-93`; `SelectiveEditorState.cs:745-746` |
| S-07 | `PostCabeceras[p]`, `ExtraFondoPostCabeceras[k-1][p]` | N-S07: relleno con `null` solo en filas existentes; ausencia preservada | RN | G3_PENDING | CANONICALIZE | filas cortas o largas | `SelectiveGeometryResolver.cs:185-205`; `SelectivePalletDesignDocument.cs:259-268` |
| S-08 | Cabecera custom: `LeftPost`/`RightPost`, placas | sin cambio (profundidad) | R | CODE_SUPPORTED | ALLOW | — | `LateralHeaderLayoutBuilder.cs:49-59` |
| S-09 | `DiagonalDirection` y `AutoAlternating` | sin cambio: la diagonal esta en el plano que RUN deja fijo | R | G3_PENDING | ALLOW | — | `BracingPanel.cs:40-48`; `PlantaHeaderLayoutBuilder.cs:71-93` |
| S-10 | `MountingFace` de horizontales y paneles | Front ↔ Back si la semantica es «normal al run» | R si CT-12 confirma; UK si no | G3_PENDING | ALLOW si CT-12 confirma; FAIL_CLOSED si no | CT-12 | `BracingPanelMemberBuilder.cs:159-165`, `:303-322` |
| S-11 | Bota `Side`, `Bota.Placement` | sin cambio | R | G3_PENDING | ALLOW | — | `BootPlacement.cs:41-65` |
| S-11b | Bota `PostSides[].PostIndex`, `Bota.Posts[].PostIndex` | `p → M−p`; valores intactos | R | G3_PENDING | ALLOW | — | `SelectivePalletDesign.cs:248-303` |
| S-12 | Protector lateral `PostSides` explicito distinto del patron por defecto | no demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | explicito ≠ {poste 0 Left, poste M Right} | `SelectiveSafetyEnds.cs:100-116`; `SelectiveSafetyPlacement.cs:221-248` |
| S-12b | Protector lateral con el patron por defecto | sin cambio | R | G3_PENDING | ALLOW | — | `SelectiveSafetyWindow.cs:788-791` |
| S-13 | Familia tope: `Side`, `TopeFondo`, `TopeShared`, `TopeSaque`, `TopeFrontal` | **sin regla**: holgura de 0.25" anclada al troquel izquierdo (AR-01) | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2) | el diseño efectivo genera al menos un tope en frontal (cualquier fondo) o planta; predicado exacto en CT-15 | `SelectiveFrontalBuilder.cs:220-222`; `SelectiveTopePlan.cs:140`, `:148`, `:262`, `:270`; `SelectivePlantaBuilder.cs:499`; `SelectiveSafetyPlacement.cs:55` |
| S-13b | Tope `TopeOffCells[].Frente` | sin regla mientras S-13 sea UK | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2) | igual que S-13 | `SelectiveTopePlan.cs:101-127` |
| S-14 | Desviador `Side`, `DesviadorLongitud`, `DesviadorPrimerNivelAltura` | sin cambio | R | G3_PENDING | ALLOW | — | `SelectiveDesviadorPlan.cs:116-128` |
| S-14b | `DesviadorOffCells` con espacio de indices estable | `c → P−1−c` | R | G3_PENDING | ALLOW | ningun claro dividido con `Dep(i)` | `SelectiveDesviadorPlan.cs:114-148` |
| S-14c | `DesviadorOffCells` con espacio de indices dependiente de una propiedad vinculada | remapeo inestable ante `ChangeValue` | UK | CODE_SUPPORTED | FAIL_CLOSED | off-cells ≠ ∅ ∧ algun claro dividido con `Dep(i)` | `SelectiveDesviadorPlan.cs:138-140`; `SelectiveMedioFrente.cs:70` |
| S-15 | Parrilla `ParrillaFrontal`, `ParrillaLateral`, `ParrillaFrente`, `ParrillaCantidad`, sin desborde | reparto simetrico en el tramo | R | G3_PENDING | ALLOW | toda fila con `count·frente ≤ span + GeometryTolerance.Length`; con conteo por defecto y `Dep(i)`, ademas `DEP-ROW` (DEP-04); con conteo o frente manual, DEP-05 | `SelectiveParrillaPlacement.cs:40-54`; `SelectiveParrillaPlan.cs:107`; `SelectiveFrontalBuilder.cs:262-311` |
| S-15x | Parrilla con una fila desbordada | apilado desde la izquierda | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna fila con `count·frente > span + GeometryTolerance.Length` | `SelectiveParrillaPlacement.cs:49`; `SelectiveFrontalBuilder.cs:310` |
| S-15d | Parrilla de claro completo con conteo por defecto, con `Dep(i)` y sin `DEP-ROW` | un `ChangeValue` posterior puede desbordarla (casos A y B, §6.5.5) | UK | CODE_SUPPORTED | FAIL_CLOSED | `Dep(i)` ∧ `¬DEP-ROW` en alguna fila | `SelectiveParrillaPlan.cs:107`; `SelectiveFrontalBuilder.cs:291-311`; `SelectiveGeometryResolver.cs:150-173`, `:369-393` |
| S-15b | Parrilla `ParrillaOffCells[].Frente` | `f → M−1−f` | R | G3_PENDING | ALLOW | — | `SelectiveFrontalBuilder.cs:249-255` |
| S-16 | Numeracion y nombres de bloque por indice | se regeneran (PDC-5) | R | CODE_SUPPORTED | ALLOW | — | `SelectiveFrontalBuilder.cs:320-340` |
| S-17 | Toggles: `Dimensions`, `DimensionStyle`, `AnnotationScale`, `DrawRackName`, `NumberFronts`, `NumberLevels`, `DrawBasePlate` y la **bandera** `DrawPallets` | sin cambio; anotaciones regeneradas con el nombre logico destino | R | CODE_SUPPORTED | ALLOW | — | `SelectiveDimensions.cs:76-263`; `SelectiveFrontalBuilder.cs:342` |
| S-17b | **Tarimas dibujadas** (`DrawPallets` activo), filas sin desborde | reparto simetrico en el claro o tramo | R | G3_PENDING | ALLOW | toda fila de claro completo con `count·frente ≤ BeamLength + GeometryTolerance.Length` (los tramos usan `PalletFit` y no desbordan) y, con `Dep(i)`, ademas `DEP-ROW` (DEP-03) | `SelectiveTarimaPlacement.cs:43-51`; `SelectiveFrontalBuilder.cs:388-440` |
| S-17c | **Tarimas**: fila de claro completo desbordada | apilado desde la izquierda | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna fila de claro completo con `count·frente > BeamLength + GeometryTolerance.Length` | `SelectiveTarimaPlacement.cs:47`; `SelectiveFrontalBuilder.cs:389-404` |
| S-17d | **Tarimas** de claro completo (incluida la fila de piso) con `Dep(i)` y sin `DEP-ROW` | un `ChangeValue` posterior puede desbordarlas (casos A y B, §6.5.5) | UK | CODE_SUPPORTED | FAIL_CLOSED | `Dep(i)` ∧ `¬DEP-ROW` en alguna fila | `SelectiveFrontalBuilder.cs:388-404`; `SelectiveGeometryResolver.cs:150-173`, `:354`, `:369-393` |
| S-18 | `PropertyValues` y literales congelados | copia exacta (portador) | R | CODE_SUPPORTED | ALLOW | registro acreditable (§10.2) y vinculos resolubles | `SelectiveLinkedProperties.cs:188-213` |
| S-19 | `View` y `Section` del sobre | §3.7 | R | CODE_SUPPORTED | CANONICALIZE (legado inequivoco) o FAIL_CLOSED (resto) | `View` vacia con `Section ≥ 0`, `< −1`, `≥ fondos`, `View` no reconocida ⇒ FAIL_CLOSED | `RackSelectivoCommands.cs:126`, `:155`, `:168-177`, `:216` |
| S-20 | Vista lateral (`Section` = poste) | no expone RUN | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | `SelectiveLateralBuilder.cs:173-198` |
| S-21 | Campos de otros sistemas en el DTO (`BotaB*`, `DefensaPosts`, `GuiaEntradaOffCells`) | round-trip intacto | R | CODE_SUPPORTED | ALLOW | — | `SelectivePalletDesignDocument.cs:539-578` |
| S-22 | `AuthoredSide` (persistido por el DTO) | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `SelectivePalletDesignDocument.cs:590` |
| S-22b | `DerivedAisles` (derivado, no autoria) | no se refleja: el resolvedor lo regenera | R | CODE_SUPPORTED | ALLOW (clasificacion `DERIVED`) | — | `SelectivePalletDesignDocument.cs:598`; `SelectivePalletDesign.cs:399-411` |
| S-23 | `DimensionViews` (I-50 integrada) | sin cambio; `int` exacto | R | CODE_SUPPORTED | ALLOW | — | `SelectivePalletDesignDocument.cs:108-116` |
| S-24 | Miembros desconocidos no vacios en el payload semantico | sin transformador | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META (§6.4) | §6.4 |
| S-25 | Normalizaciones propias del store (`SchemaVersion` sticky; `DimensionViews` nulo no se escribe) | declaradas | R | CODE_SUPPORTED | BASELINE_LIMITED | V-RT-STORE | `SelectiveDesignSchema.cs:43-66`; `SelectivePalletDesignDocument.cs:108-116` |
| **S-26** | **Estado dinamico de largueros con cambio de mano** (`LONGITUD = W(i)`), frontal y planta | la simetria verificada por la sonda para el estado usado debe valer para todo `R` (DEP-09) | R si `¬Dep(i)` e `IsSymmetric`; UK si `Dep(i)` | `G3_PENDING` (R) / `CODE_SUPPORTED` (UK) | ALLOW si `¬Dep(i)` e `IsSymmetric`; FAIL_CLOSED si `Dep(i)` | `Dep(i)` (§6.5.1) | `SelectiveFrontalBuilder.cs:131`, `:450-470`; `SelectivePlantaBuilder.cs:393-394` |
| **S-27a** | **Estado dinamico de un poste de reticula con cabecera por poste usable** (`LONGITUD = Height` authored), frontal | DEP-10a | R si `IsSymmetric` | G3_PENDING | ALLOW | `UsableCustomAt(system, k, p) ≠ null` | `SelectiveFrontalBuilder.cs:88-91`, `:489`; `SelectiveCabeceraAuthority.cs:51-55`; `SelectiveDepthLayout.cs:64-112` |
| **S-27b** | **Estado dinamico de un poste de reticula sin cabecera por poste usable** (`LONGITUD = PostHeight`), frontal | DEP-10b | R si `IndepAlturaReticula(k,p)` o sin vinculo sano de `VerticalClearance`, e `IsSymmetric`; UK en otro caso | `G3_PENDING` (R) / `CODE_SUPPORTED` (UK) | ALLOW / FAIL_CLOSED | `IndepAlturaReticula(k,p)` (§6.5.5) | `SelectivePostGeometry.cs:63-67`, `:124-130`; `SelectiveGeometryResolver.cs:271`, `:298`, `:310`, `:318`, `:327`, `:335-336` |
| **S-27c** | **Estado dinamico de un poste intermedio de medio frente** (`LONGITUD = bay.Height > 0 ? bay.Height : system.Height`), frontal | DEP-10c | R si `IndepAlturaIntermedio(b)` o sin vinculo sano, e `IsSymmetric`; UK en otro caso | `G3_PENDING` (R) / `CODE_SUPPORTED` (UK) | ALLOW / FAIL_CLOSED | `IndepAlturaIntermedio(b)` (§6.5.5) | `SelectiveFrontalBuilder.cs:144`, `:147-151`; `SelectiveGeometryResolver.cs:327`, `:335-336` |

### 7.2 Dinamico — `μ_RT`

| # | Propiedad / estado | Regla bajo μ_RT | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| D-01 | Salida en X=0 / entrada en `TotalLength` | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `DynamicLoadBeamGeometry.cs:166-190` |
| D-02 | `Fronts[]` y sus niveles | invertir la lista | R | G3_PENDING | ALLOW | — | `DynamicFrontGeometry.cs:129-162` |
| D-03 | `Modules[]` (orden, `Kind`, `Length`) | sin cambio | R | G3_PENDING | ALLOW | — | `DynamicRackSystem.cs:146-158`; `DynamicSystemPlantaBuilder.cs:135` |
| D-03b | `Modules[].Header`: `MountingFace` | Front ↔ Back si CT-12 confirma | R si CT-12 confirma; UK si no | G3_PENDING | ALLOW si CT-12 confirma; FAIL_CLOSED si no | CT-12 | `BracingPanelMemberBuilder.cs:159-165` |
| D-03c | `Modules[].Header`: `LeftPost`/`RightPost`, diagonales | sin cambio | R | G3_PENDING | ALLOW | — | `LateralHeaderLayoutBuilder.cs:49-59`; `DynamicSystemPlantaBuilder.cs:237`, `:260` |
| D-04 | `HeaderLineOverrides[]` (`PostIndex`, `ModuleId`, `Header`) | `p → N−p`; `ModuleId` intacto | R | G3_PENDING | ALLOW | — | `DynamicFrontGeometry.cs:374-395` |
| D-05 | `DerivedPostLineOverrides[]` (`PostIndex`, `Height`) | `p → N−p` | R | G3_PENDING | ALLOW | — | `DynamicFrontGeometry.cs:402-424` |
| D-06 | Escalares (alturas, peraltes, pallets, tolerancias, separadores, `DerivedPost*`) | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `DynamicRackSystemDocument.cs:16-45` |
| D-07 | Bota `PostSides`, `BotaPosts` | `p → N−p`; valores intactos | R | G3_PENDING | ALLOW | — | `BootPlacement.cs:44-53` |
| D-08a | Protector lateral con `Side ≠ None` o `PostSides` explicitos | no demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | explicito presente | `DynamicSafetyMultiViewBuilder.cs:132-140`; `DynamicSafetyDefaults.cs:57-176` |
| D-08b | Protector lateral con la regla adaptativa | sin cambio | R | G3_PENDING | ALLOW | — | idem |
| D-09 | Desviador `Side` | sin cambio | R | G3_PENDING | ALLOW | — | `DynamicSafetyLateralBuilder.cs:308-317` |
| D-10a | `DesviadorOffCells` con `off(poste 0) = off(poste 1)` por nivel | reclave de `q = N−p` | R | G3_PENDING | ALLOW | por nivel | `SelectiveDesviadorPlan.cs:28-31` |
| D-10b | `DesviadorOffCells` con `off(poste 0) ≠ off(poste 1)` | la clave `Math.Min` colapsa | MC | CODE_SUPPORTED | FAIL_CLOSED | por nivel | idem |
| D-11 | Defensa `DefensaPosts[]` | `p → N−p`; extremos intactos | R | G3_PENDING | ALLOW | — | `DynamicForkliftDefensePlan.cs:50-79` |
| D-12 | Guia de entrada `GuiaEntradaOffCells[]` | `f → N−1−f` | R | G3_PENDING | ALLOW | — | `DynamicEntranceGuidePlan.cs:27-64` |
| D-13 | Numeracion de frentes | se regenera | R | CODE_SUPPORTED | ALLOW | — | `DynamicViewDecorations.cs:66-77` |
| D-14 | `View` y `Section` | §3.7 | R | CODE_SUPPORTED | CANONICALIZE (planta) o FAIL_CLOSED (frontal fuera de `{0,1}`, `View` no reconocida) | — | `RackDinamicoCommands.cs:211`, `:220-224`, `:392-393` |
| D-15 | Vista lateral (poste), vista primaria obligatoria | no expone RT | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` o vacia | `RackDynamicSystemWindow.xaml.cs:2947-2955`; `RackDinamicoCommands.cs:235-239` |
| D-16 | Payload sin `DynamicDesign` utilizable | reflector no definido | UK | CODE_SUPPORTED | FAIL_CLOSED | `DynamicDesign == null` | `DynamicKindHandler.cs:38-44` |
| D-16b | Defectos que `ToDesign()` escribe explicitos | normalizacion declarada del store | R | CODE_SUPPORTED | BASELINE_LIMITED | V-RT-STORE | `SystemRegistry.Default.cs:74-88`; `DynamicRackSystemDocument.cs:196-211` |
| D-17 | `DimensionViews` (I-50 integrada) | sin cambio; `int` exacto | R | CODE_SUPPORTED | ALLOW | — | `DynamicRackSystemDocument.cs:63-72` |
| D-18 | Miembros desconocidos no vacios en el payload `DynamicSystem` | sin transformador (el store los descarta al leer) | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META | §6.4 |

### 7.3 Push Back de un sentido — `μ_RT`

| # | Propiedad / estado | Regla bajo μ_RT | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| P-01 | Pasillo (extremo bajo) en X=0 | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `PushBackPlacements.cs:43-61` |
| P-02 | `Structure`: hereda D-02..D-07, D-09..D-11 | como el Dinamico | segun fila | segun fila | segun fila | — | `PushBackDesignDocument.cs:27` |
| P-03 | Configuracion por frente (`DefaultPalletsDeep`, `PalletsDeepOverrides`, la **bandera** `DrawPallets`, `HighEndBeamPeraltes`, `FirstLevelHeight`) | se reordena con los frentes | R | G3_PENDING | ALLOW | — (tarimas dibujadas: P-03b/P-03c) | `PushBackCellDepth.cs:41-173` |
| P-03b | **Tarimas frontales dibujadas** (`DrawPalletAt` verdadero), filas sin desborde | una tarima por calle, centrada en su calle; la holgura del larguero se reparte a los dos extremos | R | G3_PENDING | ALLOW | en toda celda dibujada del corte: `max(1, PalletCount)·lane ≤ CellBeamLength + GeometryTolerance.Length`, con `lane = BFR` si `BFR > 0` y si no `Frente` | `PushBackTarimaPlacement.cs:210-233`; `PushBackSystemFrontalBuilder.cs:323-374`; `PushBackLoadBeamGeometry.cs:31-40` |
| P-03c | **Tarimas frontales**: fila desbordada | `FrontalRow` recorta el margen a 0 y ancla la fila al extremo izquierdo del larguero: no conmuta bajo RT | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna celda dibujada con `max(1, PalletCount)·lane > CellBeamLength + GeometryTolerance.Length` (alcanzable con `BeamLengthOverride` por nivel, `PushBackLoadBeamGeometry.cs:29`) | `PushBackTarimaPlacement.cs:224`, `:228`; `PushBackSystemFrontalBuilder.cs:359-367` |
| P-04 | **Tope posterior activo — estado POR DEFECTO**: `RearTopeSaque`, `RearTopePieceId`, rejilla | **sin regla**: anclaje al TROQUEL_TOPE del poste izquierdo del frente, mano constante `ElevationMirrored = false` en FRONTAL y `LONGITUD = larguero + 0.25"` (patron de AR-01) | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | **activo por defecto**: existe un frente `f` y un nivel `l < DynamicFrontActivation.EffectiveLoadLevels(frente)` con `Draws(f, l)`, donde `Draws = !IsNone ∧ At` y `At` solo es falso en `OffCells`; un `PieceId` en blanco o desconocido usa `LARGUERO_ESCALON_TOPE_DE_3`; predicado exacto en CT-15 | `PushBackRearTope.cs:9`, `:24`, `:37-53`; `PushBackDesign.cs:41`; `PushBackResolver.cs:73`, `:121`; `PushBackDesignDocument.cs:117`, `:204`; `PushBackRearTopeBuilder.cs:27`, `:37-48`, `:73-86`, `:172-209`, `:273`; `PushBackSystemFrontalBuilder.cs:236`, `:277-300`; `PushBackSystemPlantaBuilder.cs:77`, `:115-118`, `:134-154`; `DynamicEndBeamIdentity.cs:94-100`; `SelectiveTopePlacement.cs:41-59` |
| P-04b | Tope posterior **inactivo**: `"(ninguno)"` (`IsNone`: `PieceId` no en blanco y `PieceId.Trim()` igual sin distinguir mayusculas) o ninguna celda con `Draws` | escalares sin efecto grafico; se copian sin cambio (la mascara: P-05b) | R | CODE_SUPPORTED | ALLOW | ¬P-04 | `PushBackRearTope.cs:37-38`, `:53`; `PushBackSystemFrontalBuilder.cs:277`; `PushBackSystemPlantaBuilder.cs:115-118` |
| P-05 | `RearTopeOffCells[].Frente` con tope posterior activo | sin regla mientras P-04 sea UK | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | igual que P-04 | `PushBackRearTopeBuilder.cs:268-307` |
| P-05b | `RearTopeOffCells[].Frente` con tope inactivo (P-04b) e indices **validos** (frente en `[0, N)` y nivel en `[0, EffectiveLoadLevels(frente))`) | `f → N−1−f`: la mascara sigue persistida y vuelve a regir si el usuario elige una pieza en la copia | R | G3_PENDING | ALLOW | P-04b ∧ todos los indices validos | `PushBackRearTope.cs:41`, `:46-53`; `PushBackRearTopeBuilder.cs:268-307` |
| **P-05c** | `RearTopeOffCells` con algun indice **fuera** del dominio semantico valido actual | no se conserva opacamente: puede volverse activo tras una edicion y reaparecer en el lado incorrecto (V5-11) | UK | CODE_SUPPORTED | FAIL_CLOSED | algun frente `≥ N` o nivel `≥ EffectiveLoadLevels(frente)` | idem |
| P-06 | `Side` colapsado, `AuthoredSide`, `PostSides` | `p → N−p` | R | G3_PENDING | ALLOW | protector lateral explicito ⇒ D-08a | `PushBackSafetyAuthority.cs:239-256` |
| P-07 | `DesviadorOffCells` por poste (biyectivo) | `p → N−p` | R | G3_PENDING | ALLOW | — | `PushBackSafetyAuthority.cs:271` |
| P-09 | `View` y `Section` | §3.7 | R | CODE_SUPPORTED | FAIL_CLOSED fuera de rango, `2..3` en rack simple, planta `≠ −1` o `View` no reconocida; ALLOW en el resto | — | `RackPushBackCommands.cs:218`, `:254-258`, `:427-450` |
| P-10 | Vista lateral | no expone RT | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | — |
| P-11 | Miembros desconocidos no vacios en el payload `PushBack` (raiz y anidados) | sin transformador | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META | §6.4 |

### 7.4 Push Back compuesto A/B — `μ_RT` (sin intercambio A/B)

| # | Propiedad / estado | Regla bajo μ_RT | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| PC-01 | Configuraciones de los lados A y B | **sin intercambio**; cada lado reordena sus frentes | R | G3_PENDING | ALLOW | — | ADR-0031 §5-bis |
| PC-02 | `Structure.Modules` (A → `GAP` → B invertidos con `B:`) | sin cambio | R | G3_PENDING | ALLOW | — | `PushBackCompositeStructure.cs:495-531` |
| PC-03 | `Topologies[]`: `Frente`; tipo y direccion; defaults | `f → N−1−f`; tipo y direccion intactos | R | G3_PENDING | ALLOW | — | `PushBackSide.cs:52-59` |
| PC-04 | `StructureOverrideA/B` | sin cambio | R | G3_PENDING | ALLOW | — | `PushBackSideConfiguration.cs:182-216` |
| PC-05a | `AbsentSlotsA/B` sin ranuras ausentes al inicio ni al final | `i → N−1−i` | R | G3_PENDING | ALLOW | por lado | `PushBackCompositeStructure.cs:284-321` |
| PC-05b | Ranuras ausentes al inicio o al final de un lado | las finales se retiran y las demas dibujan borde | MC | CODE_SUPPORTED | FAIL_CLOSED | por lado | idem |
| PC-06 | Bota por lado (`Bota*`, `BotaB*`, `BotaSidesDeclared`) | postes `p → N−p`; lado intacto | R | G3_PENDING | ALLOW | — | `PushBackBootPlan.cs:114-159` |
| PC-07 | Defensa por pasillo y piezas por lado | `p → N−p`; extremos intactos | R | G3_PENDING | ALLOW | — | `PushBackDefenseSides.cs:35-95` |
| PC-08 | **Tope posterior activo por lado — estado por defecto en cada lado** | sin regla (patron de P-04) | UK | CODE_SUPPORTED | FAIL_CLOSED (O-2B) | P-04 evaluado por lado: el lado B tiene su propia configuracion, activa por defecto; basta un lado activo | `PushBackSideDesign.cs:48`, `:74`; `PushBackDesignDocument.cs:283-286`; `PushBackRearTopeBuilder.cs:73-86`, `:291-306` |
| PC-08b | Tope posterior inactivo en **ambos** lados; `OffCells` por lado con indices validos | `f → N−1−f` por lado (mascara latente, como P-05b) | R | G3_PENDING | ALLOW | P-04b en A y en B ∧ indices validos | idem |
| **PC-08c** | `OffCells` de algun lado con indices fuera del dominio valido | como P-05c | UK | CODE_SUPPORTED | FAIL_CLOSED | por lado | idem |
| PC-09 | `HeaderLineOverrides` con `ModuleId` `M…`/`GAP`/`B:…` | `p → N−p`; `ModuleId` intacto | R | G3_PENDING | ALLOW | — | `PushBackEditorDesignAssembler.cs:348-364` |
| PC-10 | `View` y `Section` frontal `0..3` | §3.7 | R | CODE_SUPPORTED | ALLOW en rango; FAIL_CLOSED fuera | — | `PushBackSystemFrontalBuilder.cs:137-155` |
| PC-11 | Letras «A»/«B» | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `PushBackSideAnnotations.cs:31`, `:104` |
| PC-12 | `Composite.Gap`, `CentralSeparator` | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `PushBackCompositeStructure.cs:123-133` |
| PC-13 | Vista lateral | no expone RT | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | — |
| PC-14a | **Tarimas frontales por lado**, filas resueltas sin desborde | como P-03b, sobre las filas resueltas por plano de corte (cama fisica × corte) | R | G3_PENDING | ALLOW | en toda fila resuelta: `Lanes·lane ≤ BeamLength + GeometryTolerance.Length` | `PushBackPalletProjection.cs:193-197`, `:208-214`; `PushBackCompositeFrontal.cs:93-108` |
| PC-14b | **Tarimas frontales por lado**: fila resuelta desbordada | como P-03c | UK | CODE_SUPPORTED | FAIL_CLOSED | alguna fila resuelta con `Lanes·lane > BeamLength + GeometryTolerance.Length` | `PushBackTarimaPlacement.cs:224`; `PushBackPalletProjection.cs:211` |

### 7.5 Cantilever — `μ_X`

| # | Propiedad / estado | Regla bajo μ_X | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| K-01 | `StationTopology.FaceMode` | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `CantileverStationDesign.cs:14-21` |
| K-02 | `StationTopology.SingleSide` | sin cambio (±Y) | R | CODE_SUPPORTED | ALLOW | — | `CantileverArmFrameResolver.cs:59-75` |
| K-03 | `ArmCellOverrides[].Side` | sin cambio (±Y) | R | CODE_SUPPORTED | ALLOW | — | `CantileverLineDesign.cs:259` |
| K-04 | `ArmCellOverrides[].StationIndex` | `i → N−1−i` si `0 ≤ i < N`; fuera de rango se conserva; orden de lista intacto | R | G3_PENDING | ALLOW | — | `CantileverLineDesign.cs:255`, `:430-437` |
| K-05 | `StationCount`, `ColumnCentreSpacing` uniforme | sin cambio | R | CODE_SUPPORTED | ALLOW | — | `CantileverLineDesign.cs:339-380` |
| K-06 | Lado de traslape del arriostramiento (derivado) | sin cambio | R | G3_PENDING | ALLOW | — | `CantileverLineResolver.cs:271-277` |
| K-07a | Arriostramiento `ColdRolledRound` | A↔B derivado | R | G3_PENDING | ALLOW | `BraceKind` | `CantileverIntervalResolver.cs:227-235`; `CantileverLineFrameResolver.cs:98-116` |
| K-07b | Arriostramiento con seccion estructural asimetrica | mano no demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | `BraceKind` + familia | idem |
| K-08 | Brazo `Single` con seccion asimetrica (canal C, angulo L) | la mano no es expresable | MC | CODE_SUPPORTED | FAIL_CLOSED | `Arrangement = Single` ∧ familia asimetrica | `CantileverArmFrameResolver.cs:88-123`; `CantileverArmResolver.cs:229-243` |
| K-08b | Brazo W o canal doble | sin cambio | R | G3_PENDING | ALLOW | — | `CantileverCataloguePolicies.cs:93-131` |
| K-09 | Ids de placa en BOM (patron medido desde `Outline[0]`) | depende del rack | R si V-BOM igual; UK si difiere | G3_PENDING | ALLOW si V-BOM igual; FAIL_CLOSED si difiere | V-BOM | `CantileverStationBomBuilder.cs:292-314` |
| K-10 | `View` y `Section` | §3.7 | R | CODE_SUPPORTED | ALLOW con `−1`; FAIL_CLOSED en el resto | — | `RackCantileverCommands.cs:501-540`; `CantileverViewPlanBuilder.cs:237-239` |
| K-11 | Separador siempre `Mirrored = true` (posible defecto existente) | no afectado por R_X | R | G3_PENDING | ALLOW | — | `CantileverLineFrameResolver.cs:61-85` |
| K-12 | Paneles, niveles, altura, visibilidad de planta, plantillas de base y columna (W) | sin cambio | R | G3_PENDING | ALLOW | — | `CantileverLineDesign.cs:66-309` |
| K-13 | Vista lateral (estacion) | no expone R_X | MC | CODE_SUPPORTED | FAIL_CLOSED | `View = lateral` | `CantileverViewPlanBuilder.cs:213-215` |
| K-15 | Miembros desconocidos no vacios en `Cantilever` | sin transformador | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META | §6.4 |
| K-16 | Rol visual de cada curva (capa de rol) | decidido en el plan; se compara en la firma | R | G3_PENDING | ALLOW | V-VIEW-ALL | `CantileverViewMaterializer.cs:169-214` |
| K-17 | Margenes retirados de troquel (`ColumnBottomPlateEndOffset`, `ColumnTopPunchOffset`) | el store los descarta por diseño (clasificacion `DERIVED`) | R | CODE_SUPPORTED | BASELINE_LIMITED | — | `RackProjectStore.cs:374-400` |

### 7.6 Cabecera independiente — `μ_D`

| # | Propiedad / estado | Regla bajo μ_D | Clase | Evidencia | Disposicion | Deteccion | Evidencia en codigo |
|---|---|---|---|---|---|---|---|
| H-01 | `LeftPost` ↔ `RightPost`; `LeftBasePlate` ↔ `RightBasePlate` | intercambio de objetos (el lado se re-deriva al cargar) | R | G3_PENDING | ALLOW | — | `RackFrameConfiguration.cs:57-60`; `RackFrameProjectDocument.cs:103-106`; `LateralHeaderLayoutBuilder.cs:49-59` |
| H-02a | `Panels[].DiagonalDirection` explicita | UpRight ↔ UpLeft | R | G3_PENDING | ALLOW | valor 1 o 2 | `BracingPanel.cs:40-48` |
| H-02b | `AutoAlternating` sin `StandardBaselineId` o en panel sin diagonal | N-H02 (Auto → explicita, declarado) | RN | G3_PENDING | CANONICALIZE | valor 0 | idem |
| H-02c | Valor numerico fuera de `{0, 1, 2}` | sin regla | UK | CODE_SUPPORTED | FAIL_CLOSED | valor desconocido | `DiagonalDirection.cs:5-7` |
| H-03 | `MountingFace` | sin cambio (R_D no toca las caras) | R | CODE_SUPPORTED | ALLOW | — | — |
| H-04a | Rasgos que el layout lee del lado izquierdo, iguales en ambos lados | intercambio exacto | R | G3_PENDING | ALLOW | igualdad de campos | `LateralHeaderLayoutBuilder.cs:61-67`; `LateralHeaderParametersFactory.cs:31-32`; `PlantaHeaderLayoutBuilder.cs:71-93` |
| H-04b | Esos rasgos distintos entre lados | el intercambio no es exacto | MC | CODE_SUPPORTED | FAIL_CLOSED | desigualdad de campos | idem |
| H-05 | Horizontales, elevaciones, alturas, troqueles, `Arrangement` | sin cambio | R | G3_PENDING | ALLOW | — | `LateralHeaderLayoutBuilder.cs:83-114`, `:262-294` |
| H-06 | `AutoAlternating` en panel con diagonal **y** `StandardBaselineId` no vacio | semantica de «estandar» tras N-H02 sin caracterizar | UK | CODE_SUPPORTED | FAIL_CLOSED hasta G5 | Auto ∧ baseline no vacio | `RackFrameConfigurationFactory.cs:85`, `:226-243`; `HardcodedStandardRackFrameService.cs:34` |
| H-07 | `View` y `Section` | §3.7 (no-planta = lateral; `σ' = −1`) | R | CODE_SUPPORTED | CANONICALIZE; FAIL_CLOSED con `View` no vacia no reconocida | — | `RackCabeceraCommands.cs:197`, `:239-240` |
| H-08 | Normalizaciones del store (version del payload `"1.0"`; legado sin envoltorio sale con envoltorio) | declaradas | R | CODE_SUPPORTED | BASELINE_LIMITED | V-RT-STORE | `RackFrameProjectDocument.cs:15-17`; `RackProjectStore.cs:56-57`, `:313-336` |
| H-09 | Miembros desconocidos no vacios en `Header` | sin transformador | UK | CODE_SUPPORTED | FAIL_CLOSED | V-META | §6.4 |
| H-10 | `Panels[].StartConnectionPointId` / `EndConnectionPointId` | sin cambio: son el extremo **inferior** y el **superior** del arriostramiento; el poste de cada extremo lo decide `DiagonalDirection`, que H-02 intercambia | R | CODE_SUPPORTED | ALLOW | — | `BracingPanelMemberBuilder.cs:210-244` (`:226-227`, `:239-240`), `:246-275`, `:277-301` |
| H-11 | `DiagonalStartOffsetTroqueles`, `DiagonalEndOffsetTroqueles`, `DiagonalDoubleSpacingTroqueles`, `HorizontalDoubleOffsetTroqueles`, `CelosiaStartTroquel`, `PasoTroquel`, `PanelClear` | sin cambio: magnitudes verticales (retranqueo desde el horizontal inferior y el superior, separacion vertical, paso, claro) | R | CODE_SUPPORTED | ALLOW | — | `BracingDiagonalGeometry.cs:3-20`; `LateralHeaderLayoutBuilder.cs:258-295`; `BracingPanelMemberBuilder.cs:134-141`; `FrameModelValidator.cs:50-52`; `RackFrameConfigurationFactory.cs:94-99` |
| H-12 | `FrameMember`/`FrameMemberEnd` y `FrameMemberEndRole` (`LeftUpright`, `RightUpright`, ...) | modelo derivado que `RefreshPhysicalModel` recalcula; no se refleja | R | G3_PENDING | ALLOW (clasificacion `DERIVED`) **solo** si T-M36 demuestra que no es persistido; si lo fuera: regla Left↔Right o FAIL_CLOSED | CT-45; T-M36 | `RackFrameProjectStore.cs:76-80`; `RackFrameProjectDocument.cs:12-42` |

### 7.7 Cama

| # | Propiedad / estado | Clase | Evidencia | Disposicion | Evidencia en codigo |
|---|---|---|---|---|---|
| F-01 | Lado del tope / sentido del riel: ningun campo persistido | MC | CODE_SUPPORTED | FAIL_CLOSED siempre | `FlowBedLateralBuilder.cs:34-66` |
| F-02 | `BedType`, `LaneDepth`, `PalletDepth`, `RollerId`, `RollerPitchOverride` (el kind no tiene vista admisible) | MC | CODE_SUPPORTED | FAIL_CLOSED (por F-01) | `FlowBedConfiguration.cs:10-22` |

### 7.8 Transversal

| # | Propiedad / estado | Regla | Clase | Evidencia | Disposicion | Evidencia en codigo |
|---|---|---|---|---|---|---|
| X-01 | `Id`/`Name` del sobre e identidad interior | restamp de copia (§5.5) | R | CODE_SUPPORTED | ALLOW | `RackEnvelopeRestamp.cs:52-111` (Plugin) |
| X-02 | Portadores del sobre de la allowlist (CA-1, CA-4) | `Compose` los hereda | R | CODE_SUPPORTED | ALLOW | `RackEmbedComposer.cs:21-35`; §6.4; D-21 de I-54 |
| X-02b | Clave de `ExtensionData` del sobre desconocida y no vacia fuera de la allowlist, o duplicado ambiguo de `CustomProperties` | ninguna regla | UK | CODE_SUPPORTED | FAIL_CLOSED | `RackEmbedDocument.cs:35-64`; `RackEmbedComposer.cs:33`; §6.4 |
| X-03 | Portadores del envoltorio de la allowlist (CA-2) | `WithSourceMetadataFrom` | R | CODE_SUPPORTED | ALLOW | `RackProjectStore.cs:71-82` |
| X-03b | Clave de `ExtensionData` del envoltorio desconocida y no vacia | ninguna regla (allowlist vacia) | UK | CODE_SUPPORTED | FAIL_CLOSED | `RackProjectDocument.cs:51-52`; `RackProjectStore.cs:82`; §6.4 |
| X-03c | Ranura de payload conocida e inactiva del envoltorio, no vacia | el store no la reescribe | R | CODE_SUPPORTED | BASELINE_LIMITED | `RackProjectStore.cs:50-61`, `:64-82`; V-RT-STORE |
| X-04 | Colocacion y presentacion de la referencia | `P' = G·P·F`; presentacion exterior de la fuente conservada (ST-13); piezas internas regeneradas como Actualizar con el read-set del contexto de materializacion (§8.7) | R | CODE_SUPPORTED | ALLOW | §3; §4.1; §8.7 |
| X-05 | Referencias enlazadas seleccionadas | una definicion nueva + N referencias | R | CODE_SUPPORTED | ALLOW | I-51 PD-2 |
| X-07 | Drive-In | no existe | — | CODE_SUPPORTED | no aplica | `RackMainMenuWindow.xaml:139-144` |
| X-08 | Larguero | sin bloque RackCad | — | CODE_SUPPORTED | no aplica | `KindHandlerRegistry.cs:51-53` |
| X-13 | Holgura u offset grafico asimetrico hallado por la auditoria de anclajes | ninguna regla implicita | UK | G3_PENDING | FAIL_CLOSED hasta regla aprobada (precedente AR-01) | CT-17 |
| X-14 | Parametro dinamico con semantica de lado sin regla | ninguna regla implicita | UK | CODE_SUPPORTED | FAIL_CLOSED | CT-25; §11.4 |
| X-15 | Vista admisible del rack (seleccionada o no) que no conmuta | V-VIEW-ALL | UK | G3_PENDING | FAIL_CLOSED | §3.8, §6.6 |
| X-16 | Miembro clasificado `UNSUPPORTED` por la guarda de cobertura con valor no vacio | sin regla de espejo | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.7 |
| X-17a | Pieza con cambio de mano sin equivalencia visual-geometrica verificada (`GeometrySymmetryResult`): sonda no disponible, sin paridad con el materializador, sin punto de observacion soportado o sin contexto reproducible de la base auxiliar (`[V8-D07]`); BTR requeridos ausentes en la base auxiliar tras el intento de clonado (`[V9-D07]`); simetria no verificada (incluida la canonicalizacion conservadora); ruta anidada estable (`[V8-D11]`) o correspondencia ambigua; eje oblicuo; productor que aplica el contexto de materializacion de forma asimetrica entre las dos generaciones o consumo del contexto no determinable (`[V9-D05]`, `[V10-D05]`); o base auxiliar que no puede simular el contexto de evaluacion destino de la pieza (`[V10-D02]`, `[V10-D09]`) | la equivalencia no esta demostrada | UK | CODE_SUPPORTED | FAIL_CLOSED | §8.7; §11.3 |
| X-17b | Geometria evaluada con una clase o variante fuera de la politica cerrada (`[V8-D01]`..`[V8-D05]`, `[V9-D08]`): subclase de una clase soportada (`MInsertBlock`), definicion anidada anotativa, XCLIP anidado, referencia anidada con atributos o con definicion XREF, overlay o dependiente, grosor, normal, elevacion o ancho fuera de la tabla, tipo de linea complejo, HATCH de degradado o patron, SOLID/TRACE/HATCH no canonizable, propiedad, entrada o estado sin clasificar o `UNSUPPORTED` en el inventario cerrado, o un ciclo de anidados | fuera de la politica del primer corte | UK | CODE_SUPPORTED | FAIL_CLOSED | §11.3 puntos 6.1, 8 y 14 |
| X-17c | Estado dinamico de la pieza dependiente de un vinculo sin evidencia universal | la evidencia de un estado no cubre `ChangeValue` | UK | CODE_SUPPORTED | FAIL_CLOSED | §11.3 punto 12; S-26, S-27a..S-27c |
| X-18 | Decision `ALLOW`/`CANONICALIZE` sin prueba de estabilidad 9b, fuera de las filas especificas | ninguna | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.5.5 |
| X-19 | Payload legado con un miembro retirado que el store actual descarta (RM-1..RM-3, RM-7) | R-A: mensaje con remedio (RACKEDITAR → Actualizar); sin limpieza automatica | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.4; `DynamicRackSystemDocument.cs`; `PushBackDesignDocument.cs` (`PushBackCompositeDocument`) |
| X-19b | Payload legado con un miembro retirado que el store actual conserva (RM-4 `HighEndBeamPeralte`) | R-B: mensaje sin remedio; sin limpieza automatica | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.4; `PushBackDesignDocument.cs:75-76` |
| X-20 | Enum con valor numerico no definido en un miembro alcanzado | el converter admite enteros sin validar | UK | CODE_SUPPORTED | FAIL_CLOSED (salvo invariante declarado: `DimensionViewVisibility`) | §6.7; `SelectivePalletDesignStore.cs:76`; `RackProjectStore.cs:412`; `RackFrameProjectStore.cs:145` |
| X-21 | Claves JSON que el matching real asigna al mismo miembro | valor efectivo dependiente del orden | UK | CODE_SUPPORTED | FAIL_CLOSED | §6.4; `PropertyNameCaseInsensitive = true` en los stores |
| X-22 | Fuentes visuales simbolicas tipadas, `SourceKind`, `PolylineGeneration`, `LinetypePatternContext` o `VisibilityPath` distintas entre homologas, mecanismo de visibilidad no modelado, orden visual relativo no demostrado dentro de la pieza, u orden visual de plan no conservado entre instancias de pieza cuyo orden relativo cambia y cuyas huellas se superponen de forma material, o huella visual `UNKNOWN` o superposicion no clasificable en un par invertido, tambien al refrescar las huellas tras `EnsureForPlan` (`[V8-D12]`, `[V9-D03]`, `[V10-D03]`) | la apariencia de la copia podria no ser el espejo del original en algun estado | UK | CODE_SUPPORTED | FAIL_CLOSED | §11.3 puntos 5 y 6.2..6.6 |
| X-23 | Transformacion efectiva de la referencia evaluada (`Position`, `Rotation`, `ScaleFactors`, `Normal`, `BlockTransform`, `Origin`) distinta de la esperada por una accion dinamica no representada | la colocacion evaluada no es la caracterizada | UK | CODE_SUPPORTED | FAIL_CLOSED | §11.3 punto 13 |

### 7.9 Precondiciones operativas (no son clases de representabilidad)

| # | Precondicion | Disposicion | Momento | Evidencia en codigo |
|---|---|---|---|---|
| PRE-01 | Candidato RackCad no canonico: MINSERT, `Normal`, rotacion, escala (ST-3, ST-5..ST-7, ST-9, ST-10); los objetos sin payload RackCad no llegan aqui (C2 de §4.1) | FAIL_CLOSED (E4) | P1 | §4.1 |
| PRE-02 | Definicion con `Origin ≠ 0` (ST-14) | FAIL_CLOSED (E4) | P1 | `RackCloner.cs:34`; `LateralHeaderDrawer.cs:243` |
| PRE-03 | `View` no vacia no reconocida o `Section` no decodificable (ST-15) | FAIL_CLOSED (E4) | P1 | §3.7 |
| PRE-04 | Referencia dinamica, anonima o anotativa con payload RackCad (ST-17) | FAIL_CLOSED (E4) | P1 | `RackBlockFinder.cs:66` |
| PRE-05 | Divergencia authored entre vistas del grupo | FAIL_CLOSED (E3) | P2 | §5.4 |
| PRE-06 | Registro no acreditable con algun grupo cuyo kind lo consume, o vinculo roto | FAIL_CLOSED (E7) | P3 | §10.2 |
| PRE-07 | Push Back bloqueado para salida (`RackBomOutputGate.For(system).Reason ≠ null`) | FAIL_CLOSED (E6) | P4 | `PushBackKindHandler.cs:61-74` |
| PRE-08 | Linea Cantilever invalida o catalogo de secciones ilegible | FAIL_CLOSED (E6) | P4 | `CantileverKindHandler.cs:48-67` |
| PRE-09 | Bloques de biblioteca ausentes tras PREPARE; `MissingInstances > 0` en MUTATE | FAIL_CLOSED (E10/E11) | PREPARE/MUTATE | `BlockLibraryImporter.cs:74-118`; `LateralHeaderDrawer.cs:293-303` |
| PRE-10 | Huella o read-set cambiado | FAIL_CLOSED (E10/E11) | PREPARE/MUTATE | §9.2 |
| PRE-11 | Payload RackCad presente pero ilegible o inutilizable, incluido F-14a de I-54 (`ArgumentException` por UTF-16 crudo invalido) | FAIL_CLOSED (E2) | SNAPSHOT/P1 | §4.1 (ST-2c) |
| PRE-12 | Sobre que no se puede reserializar en el ensayo de P10 (F-14b de I-54) o que `Compose` rechaza (regla F de I-54) | FAIL_CLOSED (E8) | P10 | §9.4 |
| PRE-13 | Definicion real de una pieza con cambio de mano distinta en PREPARE de la evaluada en PREFLIGHT (`TraceFingerprint` distinto) y cuya equivalencia visual-geometrica ya no se verifica | FAIL_CLOSED (E10) | PREPARE | §11.3 puntos 3 y 10 |
| PRE-14 | Sonda no disponible, fallida, sin paridad con el materializador, sin punto de observacion soportado o sin contexto reproducible de la base auxiliar para una pieza con cambio de mano; BTR requeridos ausentes en la base auxiliar tras el intento de clonado (el retorno de `EnsureBlocks` es diagnostico); postcondicion del dibujo incumplida o contenido de la cache de biblioteca alterado (§11.3 puntos 4 y 14); huella visual de una instancia enumerada no disponible o contexto de evaluacion destino no reproducible (§11.3 puntos 4 y 5, `[V10-D02]`) | FAIL_CLOSED (E6, X-17a o X-22) | P7 | §11.3 |
| PRE-15 | Candidato RackCad con estado de presentacion no transportable, sin clasificar o `UNSUPPORTED` en el inventario cerrado (ST-18, `[V8-D14]`, `[V9-D08]`) | FAIL_CLOSED (E4) | P1 | §4.1; CT-49 |
| PRE-16 | Orden visual en Model Space no demostrable para alguna copia frente a todo objeto posterior a su fuente, huella visual o contexto de orden no disponibles, `XLINE`/`RAY` posterior sin interseccion analitica u ordenes de pantalla y de trazado contradictorios entre fuentes admitidas con copias superpuestas (ST-19, `[V9-D01]`, `[V9-D04]`, `[V10-D06]`, `[V10-D07]`) | FAIL_CLOSED (E10) | PREPARE: rechazo anticipado antes de `EnsureForPlan` y decision definitiva despues (PRE-18) | §4.1; §9.1; CT-49 |
| PRE-17 | Observacion consumida del `MaterializationContextReadSet` distinta entre SNAPSHOT, PREPARE y MUTATE, o consumo que G3 no pudo determinar (§8.7, `[V9-D05]`, `[V10-D05]`); un estado no consumido no invalida | FAIL_CLOSED (E10/E11; `UNKNOWN` si el consumo no se determina) | PREPARE/MUTATE | §8.7; §9.2 |
| PRE-18 | Tras `EnsureForPlan`, el `FootprintInputFingerprint` de alguna instancia cuya huella visual se uso (con o sin cambio de mano, de una vista seleccionada o de una vista admisible afectada por simbolos importados o colisiones de nombres) difiere y, recalculadas la huella, el orden visual de plan y ST-19, no se demuestran; o esa postcondicion cambia al inicio de MUTATE (`[V10-D03]`) | FAIL_CLOSED (E10, con X-22 si falla el orden de plan; en MUTATE, excepcion y rollback) | PREPARE tras `EnsureForPlan`; inicio de MUTATE | §9.1; §9.2; §11.3 punto 5 |

---

## 8. Regeneracion

### 8.1 Application: autoridad pura de planes por vista

Una autoridad por kind, con contrato comun, **compartida** con el redibujo (no especifica del espejo):

```text
RackViewPlanAuthority (por kind; nombre no contractual)
  Build(request) → ViewPlanResult | fallo atribuible

request = { Design (payload interior en su sustrato), View canonica, σ' canonica, catalogos cargados,
            estado efectivo: lectura acreditada del registro si el kind lo consume (§10.2),
            RackName (nombre logico destino), sugerencia de nombre de bloque }

ViewPlanResult = { Familia: HeaderRun | CantileverCurves,
                   HeaderRunPlan | CantileverViewPlan,
                   nombre de bloque sugerido,
                   tramo del eje de la vista (para c) }
```

`RackName` es obligatorio: la anotacion «nombre de rack» la dibuja `system.Name` (`SelectiveFrontalBuilder.cs:342`;
`DynamicViewDecorations.cs:91`, `:156`, `:256`) y hoy la fijan los comandos (`RackSelectivoCommands.cs:121`, `:176`;
`RackDinamicoCommands.cs:177`; `RackPushBackCommands.cs:182`; `ProjectVariableMutationExecutor.cs:166-168`, `:236-238`).

La autoridad **reutiliza** los builders puros existentes; lo que hoy es pegamento del Plugin pasa a ella:

| Kind | Resolucion (Application, existente) | Plan por vista (Application, existente) |
|---|---|---|
| selective | `SelectivePalletDesignStore` → `SelectiveEffectiveDesignResolver.ResolveAccredited(authored, lectura)` → `SelectiveGeometryResolver.Resolve(efectivo, catalogo)` (mismo camino que `SelectiveKindHandler.cs:36-70`) | frontal: `SelectiveFrontalBuilder.BuildPlan(SelectiveDepthLayout.FondoSystemView(system, k), catalogo)`; planta: `SelectivePlantaBuilder.BuildPlan(system, catalogo)` |
| dynamic | `RackProjectStore` → `DynamicRackSystemResolver(catalogo).Resolve(DynamicDesign).System` (`DynamicKindHandler.cs:38-44`) | frontal: `DynamicSystemFrontalBuilder.BuildPlan(system, catalogo, end)`; planta: `DynamicSystemPlantaBuilder.BuildPlan` |
| pushback | `RackProjectStore` → `PushBackResolver(catalogo).Resolve(PushBackDesign)`; bloqueo `RackBomOutputGate.For` (`PushBackKindHandler.cs:40-74`) | frontal: `PushBackSystemFrontalBuilder.BuildPlan(system, catalogo, end, side)`; planta: `PushBackSystemPlantaBuilder.BuildPlan` |
| cantilever | `RackProjectStore` → `CantileverLineEditorAssembler(secciones).Build(CantileverLineDesign)` (`CantileverKindHandler.cs:48-67`) | `CantileverViewPlanBuilder.Build` |
| cabecera | `RackProjectStore` → `Header` | lateral: `HeaderInstanceGrouper.Group(LateralHeaderLayoutBuilder.Build(config, LateralHeaderParametersFactory.FromConfiguration(config), catalogo).Instances, nombre)` (`LateralHeaderDrawService.cs:60-90`); planta: `new HeaderRunPlan(vacio, PlantaHeaderLayoutBuilder.Build(config, catalogo))` (`PlantaHeaderDrawService.cs:29`) |

Restricciones: sin tipos de AutoCAD; sin `ObjectId`; sin I/O de disco dentro de `Build` (los catalogos llegan cargados);
un fallo de resolucion efectiva devuelve un fallo tipado, nunca un plan con literal congelado. Desde I-50, los builders
aplican `DimensionViewPolicy` a las cotas (`SelectiveDimensions.cs:76-263`; `DynamicViewDecorations.cs`), y la
autoridad pasa `DimensionViews` tal cual.

### 8.2 Plugin: materializador generico

```text
PREPARE (dentro del lock, FUERA de la transaccion de escritura; orden normativo en §9.1)
  planes definitivos de las vistas SELECCIONADAS, con el nombre destino, y huellas visuales en el contexto destino
                     → ST-19 temprano (rechazo anticipado, opcional)
  familia HeaderRun  → BlockLibraryImporter.EnsureForPlan(db, plan)                (efectos de infraestructura, §9.5)
                     → re-verificacion en lectura: toda instancia que no sea Annotation ni Dimension tiene BlockName
                       no vacio y presente en la BlockTable
                     → faltantes registrados (pieza, bloque, vista); faltantes ≠ ∅ ⇒ E10
  familia Cantilever → sin bloques de biblioteca (curvas); las capas de rol se crean dentro de MUTATE
  relectura del dibujo real → FootprintInputFingerprint → huellas, orden de plan y ST-19 refrescados (PRE-18)

CreateInTransaction(db, tx, plan, nombreBloque, payload) → definitionId           (transaccion del llamador)
  HeaderRun  → result = new LateralHeaderDrawer().CreateSystemBlock(db, tx, plan, nombre)     (LateralHeaderDrawer.cs:28-81)
               si result.Outcome.MissingInstances.Count > 0 ⇒ throw  ⇒ rollback total          (camino de I-52)
  Cantilever → CantileverViewMaterializer.CreateBlockDefinition(db, tx, plan, nombre, out real)  (:36-54)
  ambos      → RackBlockData.Write(tx, definitionId, payload)

CreateReference(db, tx, definitionId, colocacion, presentacion) → referenceId      (Model Space; transaccion del llamador)
  Position = (p'.X, p'.Y, Z fuente); Rotation = θ'; ScaleFactors = (s, s, s)
  LayerId, Color, Linetype, LinetypeScale, LineWeight, Transparency, Visible = los de la referencia fuente
    (SNAPSHOT, tipados; asignados explicitamente, nunca por los valores por defecto de la base)             (ST-13)
  PlotStyleName = el de la fuente solo con STB; con CTB deriva del color y no se asigna                      (ST-13)
  orden de creacion de las referencias = orden efectivo relativo de sus fuentes en Model Space              (ST-19)
```

- **Omision convertida en error.** `CreateSystemBlock` omite hoy las piezas sin bloque y las reporta como faltantes
  (`LateralHeaderDrawer.cs:293-303`; `LateralHeaderDrawOutcome.cs:31-33`; `BlockPlacement.cs:99-117`). Para RACKMIRROR
  una pieza faltante es **fallo duro**. El drawer **no** cambia; el control vive en el materializador de I-52 (G-M11).
- Despacho por **familia de plan**, no por kind: el materializador no conoce Left/Right, A/B, estaciones ni reglas de
  cabecera.
- No abre lock ni transaccion, no confirma, no regenera, no purga y no importa dentro de MUTATE (precedente
  `SystemBlockWriter.RedefineInTransaction`, `SystemBlockWriter.cs:82-116`).
- Las capas se crean dentro de la transaccion del llamador (`LateralHeaderDrawer.cs:270`, `:330-331`, `:356`;
  `CantileverViewMaterializer.cs:142`, `:169-195`).
- **Presentacion y orden de la referencia** (`[V8-D13]`, `[V9-D01]`, `[V9-D05]`, `[V9-D09]`, `[V10-D03]`, `[V10-D05]`):
  la referencia nueva lleva la presentacion exterior de su fuente (ST-13, con el estilo de trazado segun STB o CTB) y
  las referencias se crean en el orden efectivo relativo de las fuentes (ST-19), cuya decision definitiva se toma tras
  `EnsureForPlan` con las huellas refrescadas (PRE-18). El contenido de la definicion nueva mantiene el orden de
  materializacion de §11.3 punto 6.6 y se crea con las observaciones del contexto de materializacion que consuma cada
  productor (§8.7); el drawer no cambia.
- La politica de unicidad de nombres de bloque es la existente de cada familia; I-52 **no** unifica las dos politicas
  (decision de I-09).

### 8.3 Geometria neutral

- `Transform2D.ReflectionAboutLine(a, b)` en Application/Geometry, compuesta con las primitivas existentes (§3.3).
- Un valor de colocacion `(Position2D, RotationRadians, UniformScale)` con composicion a `Transform2D` y descomposicion
  validada (§3.3, §4.3).
- **Sin** sistema matematico paralelo y **sin** `Matrix3d` fuera del Plugin.

### 8.4 Autoridad de `View`/`Section`

La decodificacion `SectionRaw → semantica → canonica`, la exposicion y el conjunto `A_k` (§3.7, §3.8) viven en
Application, **junto al contrato de cada kind**. El comando no contiene `switch` de vistas. Las lecturas actuales del
Plugin (`RackSelectivoCommands.cs:126-177`; `RackDinamicoCommands.cs:209-239`, `:392-393`;
`RackPushBackCommands.cs:218-258`, `:427-450`; `RackCantileverCommands.cs:295-314`, `:501-540`;
`RackCabeceraCommands.cs:239-240`; `ProjectVariableMutationExecutor.cs:180-218`) **no** se tocan en el primer corte:
CT-04 fija su comportamiento y C-2 (§8.5) comprueba que la autoridad nueva no diverge.

### 8.5 Convergencia con los productores reales: **C-2 por defecto, GREEN en G6**

**Objetivo (V-EDIT).** Espejo → guardar/reabrir → `RACKEDITAR` → Actualizar → **mismo** dibujo. RACKMIRROR y los caminos
productivos no divergen al generar la misma vista.

**C-2.** Pruebas de equivalencia en **CI**, contra los productores reales, por fixture de (kind, vista admitida) y por su
copia espejo:

| # | Productor | Camino que se ejercita |
|---|---|---|
| C2-1 | Autoridad nueva de plan | `RackViewPlanAuthority.Build(designJson, View canonica, σ' canonica, catalogos, estado efectivo, RackName)` |
| C2-2a | `RACKEDITAR` → **Actualizar** (`EditX`, redibujo en sitio) | payload → apertura del editor → sistema que dibuja el comando → servicio de dibujo → builder. Selectivo: `SelectiveEditorOpen.Resolve` → `LoadExisting` → `SystemToInsert` → `SelectiveFrontalDrawService().RedrawInPlace` / `SelectivePlantaDrawService().RedrawInPlace` (`RackSelectivoCommands.cs:66-117`, `:175-217`; `RackSelectiveWindow.xaml.cs:185-188`); Dinamico, Push Back y Cantilever: `LoadExisting` → `DesignToInsert`/`SystemToInsert` (`RackDinamicoCommands.cs:162-177`; equivalentes); cabecera: `RackFrameConfiguratorViewModel.Configuration`. En el Dinamico, I-53D integro antes del Candidato (`dad4e77`): T-M17 corre sobre el arbol combinado (`[V10-D15]`, `[V10-D23]`) |
| C2-2b | `RACKEDITAR` → **Insertar** desde la copia (vista nueva enlazada, mismo GUID) | payload → apertura del editor → sistema que dibuja el comando → camino de insercion → builder. Selectivo: `DrawSelectiveViewFromAuthored` → `InsertSelectiveFrontal` o `SelectivePlantaDrawService().DrawAndPlace` (`RackSelectivoCommands.cs:246-258`, `:426-452`); equivalentes en Dinamico, Push Back, Cantilever y cabecera. En el Dinamico, I-53D integro antes del Candidato (`dad4e77`): T-M17b corre sobre el arbol combinado (`[V10-D15]`, `[V10-D23]`) |
| C2-3 | `ProjectVariableMutationExecutor` (Selectivo) | `EffectiveOutput` → `SelectiveGeometryResolver` → `FondoSystemView`/planta → builder (`ProjectVariableMutationExecutor.cs:166-243`) |
| C2-4 | Estado del editor → sistema/documento | `SelectiveEditorState`, `DynamicEditorDesignAssembler`, `PushBackEditorDesignAssembler`, `CantileverLineEditorAssembler` en Core; lo que solo pasa por la ventana, en `tests/RackCad.UI.Tests` (en el Dinamico, con I-53D integrada en `dad4e77`, tambien `DynamicHeaderBatch` y `DynamicRackRebuild` a traves de la ventana, `[V9-D22]`, `[V10-D15]`, `[V10-D23]`) |

```text
firma(C2-1) = firma(C2-2a) = firma(C2-2b) = firma(C2-3, solo Selectivo) = firma(C2-4)
firma de §11.2, con: nombre logico, estado efectivo (lectura del registro), View y Section canonicas, DimensionViews
```

**Insertar es normativo (V4-09).** V-VIEW-ALL (§3.8) se apoya en que la copia pueda materializar despues cualquier vista
admisible con Insertar. Regla:

- si C2-2a y C2-2b llegan **exactamente** a la misma costura de builder con las mismas entradas (sistema efectivo, `Name`
  logico, vista y seccion canonicas, `DimensionViews`), pueden **compartir fixture** de comportamiento, **pero** la guarda
  G-M9 demuestra que ambos caminos siguen llamando esa costura;
- si no, Insertar es un **productor adicional** con prueba propia (T-M17b).

CT-26 mapea en G3 las costuras de Actualizar e Insertar por kind y vista admitida.

- **Gate.** C2-1, C2-2a, C2-2b, C2-3 y C2-4 deben estar **GREEN antes de cerrar G6**, que es donde nace la autoridad.
  **G7 no consume la autoridad** si C-2 no esta verde. G8 conserva los cruces entre iniciativas, la continuidad de G-M9 y
  la reconciliacion final con las autoridades integradas.
- **Guarda G-M9.** Fija el mapa exacto de invocaciones de builders y servicios de dibujo de `EditX` (Actualizar **e
  Insertar**) y del ejecutor para las vistas admitidas (`SelectiveDepthLayout.FondoSystemView`,
  `SelectiveFrontalDrawService`, `SelectivePlantaDrawService`, `DynamicFrontalDrawService`, `DynamicPlantaDrawService`,
  `PushBackFrontalDrawService`, `PushBackPlantaDrawService`, `CantileverViewPlanBuilder.Build`, `LateralHeaderDrawService`,
  `PlantaHeaderDrawService` y las llamadas a builders dentro de esos servicios). Nace en G6 y sigue vigente en G8.
- **Validacion del Owner obligatoria:** M-4 (Actualizar) y M-5 (Insertar).
- **V-EDIT ≠ V-RT-STORE** (§6.3): un defecto historico de `EditX` no autoriza perdidas del espejo.

**C-1 (condicionada).** `RACKEDITAR` construye los planes de las vistas admitidas llamando a la autoridad. Solo se
activa con evidencia registrada y orden del Coordinador si:

1. hay divergencia demostrada entre productores;
2. el camino `EditX` de un kind no puede ejercitarse de forma fiable en Core ni en UI;
3. hay duplicacion sustantiva que dejaria dos autoridades del mismo plan.

### 8.6 Politica post-commit propia de I-52

- **Sin `Regen`** (DA-3), sin purga (crear definiciones no deja anidadas huerfanas); solo mensajes.
- Precedentes: RACKDUPLICAR crea definiciones y referencias sin `Regen` con PASS del Owner (I-51, INV-14); los caminos de
  insercion tampoco regeneran (`SystemBlockWriter.cs:18-40`; `ViewBlockDraw.cs:28-57`).
- Un **unico** `Regen` post-commit solo si el Owner demuestra representacion obsoleta, con evidencia registrada.
- INV-14 de I-51 **no cambia**.

### 8.7 Contexto de materializacion (`[V9-D05]`, `[V10-D04]`, `[V10-D05]`, `[V10-D12]`)

I-52 es fiel al **plan regenerado** del documento authored, como `RACKEDITAR` → Actualizar: **no** clona la presentacion
historica de cada referencia interna del bloque fuente (decision vinculante del Coordinador de V9). **Los drawers no
asignan explicitamente determinadas propiedades; CT-50 debe caracterizar que mecanismo y que valores efectivos aplica
AutoCAD 2025 y que observaciones consume cada productor.** Hechos del codigo (`1b091be`, sin cambios en `ba497f1`):

- `LateralHeaderDrawer` crea las colocaciones de grupos y las referencias de pieza sin asignar capa, color, tipo de
  linea, escala de tipo de linea, grosor, transparencia ni estilo de trazado (`LateralHeaderDrawer.cs:60-65`,
  `:145-150`, `:308-317`), y ningun archivo de `src/` llama a `SetDatabaseDefaults`;
- las anotaciones son `DBText` con capa `RACKCAD_ANOTACIONES`, altura y alineacion explicitas, sin estilo de texto,
  color, tipo de linea, grosor ni transparencia asignados (`:266-277`);
- las cotas son `RotatedDimension` en la capa `RACKCAD_COTAS`, con el estilo con nombre del plan si existe en el dibujo
  o, si no, con el **`DIMSTYLE` actual** del dibujo y sobreescrituras de tamaño solo en ese caso (`:338-392`, `:391`);
- `LayerHelper.EnsureLayer` devuelve la capa existente tal cual (`LayerHelper.cs:19-22`): manda el registro de capa
  del dibujo;
- `CantileverViewMaterializer` asigna a cada curva su capa de rol y `ByLayer` en color, tipo de linea y grosor
  (`CantileverViewMaterializer.cs:209-218`), no su escala de tipo de linea, transparencia ni estilo de trazado, y deja
  intactas las capas de rol que ya existen (`:163-197`).

La **intencion** (que pieza, que anotacion o que cota, y en que capa) sale del plan; la **representacion** y la huella
visual pueden consumir estado del dibujo. No se afirma que anotaciones y cotas salgan completamente del plan.

```text
MaterializationContext (nombre no contractual; no es schema persistido)
  observaciones candidatas: CLAYER · CECOLOR · CELTYPE · CELTSCALE · CELWEIGHT · CETRANSPARENCY · CPLOTSTYLE ·
                            PSTYLEMODE · DIMSTYLE actual y su registro efectivo · TEXTSTYLE efectivo del DBText ·
                            escala de anotacion · registros de capa RACKCAD_ANOTACIONES y RACKCAD_COTAS ·
                            registros de las capas de rol del Cantilever · todo otro estado que descubran
                            CT-36, CT-49 o CT-50

MaterializationContextReadSet (nombre ilustrativo)
  contiene UNICAMENTE las observaciones que el productor o la huella visual realmente consumen, por productor presente
  en la operacion (piezas y colocaciones de HeaderRun, DBText, cotas, curvas Cantilever)
    productor que fija Color explicito            ⇒ CECOLOR no forma parte de su read-set
    cota sin estilo con nombre en el dibujo       ⇒ DIMSTYLE si
    DBText que usa el estilo de texto actual      ⇒ TEXTSTYLE si, si CT-50 lo demuestra
  consumo que G3 no puede determinar             ⇒ UNKNOWN ⇒ FAIL_CLOSED

Regla
  el read-set se captura en SNAPSHOT, entra en la huella de la referencia (§9.2) y se re-verifica en PREPARE y al inicio
    de MUTATE
      observacion consumida y cambiada ⇒ E10 en PREPARE; excepcion y rollback en MUTATE (PRE-17)
      estado no consumido que cambia   ⇒ no invalida
  Π_(V,σ)(Eff(D, R)) y Π_(V,σ')(Eff(μ_k D, R)) se evaluan y materializan con las mismas observaciones consumidas
  la base auxiliar reproduce las observaciones consumidas que afecten a la evaluacion o a la huella visual (§11.3
    punto 4)
  productor que las aplica de forma asimetrica entre las dos generaciones ⇒ UNKNOWN ⇒ FAIL_CLOSED (X-17a)
  presentacion EXTERIOR de la referencia RackCad = ST-13 (este contexto no la fija)
```

- La referencia de pieza es la raiz comun `·` de las dos mitades (§11.3 punto 6.2): el contexto no entra en
  `VisualSignature` y la equivalencia se evalua con las mismas observaciones en las dos generaciones.
- Hecho del codigo, indicativo: los productores actuales no aplican el contexto de forma asimetrica (las colocaciones de
  grupos y las piezas sueltas se crean igual; la capa de rol la decide el plan; anotaciones y cotas usan las mismas
  capas y la misma resolucion de estilo en las dos generaciones); CT-50 lo confirma o lo refuta.
- Las huellas visuales de anotaciones y cotas consumen `DIMSTYLE`, `TEXTSTYLE`, escala de anotacion y registros de capa
  cuando CT-50 lo demuestre (§11.3 punto 5); esas observaciones entran en su `FootprintInputFingerprint` (§9.2, PRE-18).
- Consecuencia para O-1 (L-36): las piezas internas de la copia se regeneran como Actualizar; no se copian fisicamente
  desde la definicion vieja, asi que la copia puede verse distinta de una fuente dibujada con otro contexto. En la
  biblioteca inspeccionada, 1 435 entidades de las definiciones estan en capa `0` y heredan la capa de su referencia de
  pieza (§1.4).
- No crea obligacion de tocar los drawers existentes (§20.3); G-M8 y G-M11 no cambian.

---

## 9. Atomicidad

### 9.1 Orden normativo unico

```text
ACQUIRE    GetSelection sin SelectionFilter de tipo
SNAPSHOT   una transaccion de lectura: referencias y sus banderas de fuente (MINSERT, dinamica, anonima, anotativa),
           presentacion tipada, estado no transportable o sin clasificar, orden efectivo en Model Space y su contexto de
           pantalla y de trazado, observaciones candidatas del contexto de materializacion (§5.2, §8.7), definiciones
           con Origin y payload, UNA lectura del registro de variables, huella de la referencia y read-sets (§9.2)

PREFLIGHT  (sin mutacion; un solo orden; cualquier fallo ⇒ mensaje atribuible y FIN, cero mutacion)
  P1  precedencia de fuente                        C1..C5 de §4.1 (candidato RackCad primero; restricciones estrictas solo sobre
                                                   candidatos), incluida la decodificacion de View/Section (§3.7) y
                                                   la presentacion no transportable (ST-18)
  P2  agrupacion + autoridad authored              nucleo neutral; Kind y nombre del grupo; IsSameAuthority (§5)
  P3  precondiciones del registro y de la          acreditacion solo para grupos cuyo kind consume el registro;
      resolucion efectiva                          vinculos sanos; read-set (§10.2)
  P4  fidelidad del store y sustrato               lectura con la autoridad del kind; V-META del payload y del exterior (§6.4); LB(D) (§6.3);
                                                   precondiciones de diseño (PRE-07, PRE-08)
  P5  Assess + Reflect candidato                   clase, evidencia y disposicion por fila; normalizaciones; V-DEP (§6.5.5; obligaciones 9 y 9b)
  P6  cobertura de portadores y miembros           portadores de la allowlist identicos (§6.4); miembros UNSUPPORTED no vacios ⇒ X-16 (§6.7)
  P7  V-VIEW-ALL                                   todas las (View, Section canonica) admisibles del rack logico (§3.8):
        P7a  Application enumera, por vista admisible, las piezas con cambio de mano y su estado (definicion,
             parametros solicitados, marco) y TODA otra instancia que participe potencialmente en el orden visual
             de plan, en ST-19 o en una clasificacion de superposicion, tenga o no cambio de mano ([V9-D02])
        P7b  el Plugin evalua cada estado con la sonda y la semantica de parametros del materializador, sin mutar el
             DWG ni la cache de biblioteca (§11.3): GeometrySymmetryResult (piezas con cambio de mano), en una base
             auxiliar con el contexto de la autoridad origen, con variantes, valores efectivos, fuentes visuales
             simbolicas tipadas, VisibilityPath, contexto de patron, orden visual y transformacion efectiva; y
             VisualFootprintResult (toda instancia enumerada) en el DestinationEvaluationContext (DRAWING FIRST):
             ocupacion de modelo en WCS construida despues de transformar, con su FootprintInputFingerprint
             ([V10-D01], [V10-D02], [V10-D09])
        P7c  Application completa la equivalencia visual-geometrica con los resultados, el orden visual de plan
             entre piezas sobre las huellas (§11.3 punto 6.6), DEP-09/DEP-10a..c y PlanPlacementEvidence
  P8  V-BOM                                        las dos comprobaciones (§11.5)
  P9  identidad y nombre                           NewRackId por grupo; «<base> - espejo»; validacion del asignador (§5.5)
  P10 Compose + ensayo de restamp + V-RT-STORE     Plugin, sin transaccion; V-RT-STORE sobre el payload re-estampado (§6.3)

LINE       dos puntos UCS→WCS validos (§4.2)
PREPARE    (dentro del lock y fuera de la transaccion de escritura; cualquier fallo ⇒ E10; [V10-D03], [V10-D04])
  R1  colocaciones P' · planes efectivos DEFINITIVOS de las vistas SELECCIONADAS (§8.1), con el nombre destino de P9,
      y payloads definitivos
  R2  huellas visuales en el DestinationEvaluationContext de las instancias de esos planes, anotaciones y cotas
      incluidas, y huellas de los objetos de Model Space posteriores a cada fuente con sus registros resueltos
      (§4.1, §11.3 punto 5)
  R3  ST-19 temprano (opcional): rechazo anticipado frente a todo objeto posterior, antes de importar
  R4  EnsureForPlan (infraestructura, §9.5) · re-verificacion de bloques
  R5  relectura del dibujo real: FootprintInputFingerprint de TODA instancia cuya huella visual se uso —con o sin
      cambio de mano, de las vistas seleccionadas y de toda vista admisible cuya evidencia pueda haber cambiado por
      simbolos importados o colisiones de nombres—; si difiere, se vuelve a producir su VisualFootprintResult
      · TraceFingerprint de la definicion real de las piezas con cambio de mano (valores efectivos, fuentes
        simbolicas, VisibilityPath y transformacion) y, si difiere, equivalencia visual-geometrica (PRE-13, §11.3)
  R6  orden visual de plan (§11.3 punto 6.6) y ST-19 re-evaluados con las huellas refrescadas: decision DEFINITIVA
      (PRE-18)
  R7  re-verificacion de la huella de la referencia (§9.2), del read-set del registro y del
      MaterializationContextReadSet (PRE-10, PRE-17)
MUTATE     un DocumentLock · UNA transaccion de escritura:
           repeticion de la postcondicion de PREPARE (huella de la referencia, read-sets y FootprintInputFingerprint;
           PRE-17, PRE-18) · todas las definiciones nuevas (MissingInstances ⇒ excepcion)
           · todos los payloads · todas las referencias, con la presentacion de su fuente (ST-13) y en el orden
             efectivo relativo de las fuentes (ST-19)
COMMIT     una vez
POST       solo mensajes (§8.6)
```

**Ajuste por dependencias.** La orden del Coordinador situaba el ensayo de identidad despues del restamp; el restamp
necesita `NewRackId` y el nombre, asi que P9 precede a P10. V-RT-STORE se completa en P10 porque el restamp
re-serializa por dominio en cabecera y Cantilever (`CabeceraKindHandler.cs:44-66`; `CantileverKindHandler.cs:80-110`).

**ST-19 en PREPARE (`[V8-D25]`, `[V9-D01]`, `[V9-D04]`, `[V9-D19]`, `[V10-D03]`, `[V10-D04]`, `[V10-D06]`).** El orden
visual en Model Space depende de la linea (destino de cada copia), asi que no cabe en PREFLIGHT. PREPARE construye
primero los planes definitivos de las vistas seleccionadas, con el nombre destino, y sus huellas de ocupacion de modelo
en el contexto destino. ST-19 puede evaluarse entonces, antes de `EnsureForPlan`, como **rechazo anticipado**: un fallo
termina en E10 sin importar bloques. Tras `EnsureForPlan` se relee el dibujo real, se recalculan las huellas cuyas
entradas cambiaron y se vuelven a evaluar el orden visual de plan y ST-19: esa es la decision **definitiva**, y un fallo
termina en E10 aunque lo importado permanezca (§9.5). Los objetos posteriores a cada fuente, con su huella y su posicion
en el orden efectivo, entran en la huella de la referencia (§9.2) y MUTATE los re-verifica. Si CT-49 encuentra
contextos de pantalla y de trazado con ordenes efectivos distintos, «posterior» significa posterior en cualquiera de
ellos, y dos fuentes admitidas con orden relativo distinto entre esos contextos cuyas copias se superponen de forma
material, o no clasificable, ⇒ E10.

### 9.2 Huella de la referencia, read-sets y `FootprintInputFingerprint`

**Terminologia (`[V10-D19]`).** La **huella de la referencia** es el estado capturado en esta tabla. La **huella
visual** es `VisualFootprintResult` (§11.3 punto 5). `FootprintInputFingerprint` reune las entradas de una huella visual
y `TraceFingerprint` es la trazabilidad de la evidencia de simetria (§11.3 punto 10). Ninguno sustituye a otro.

Capturados en SNAPSHOT (el `FootprintInputFingerprint`, desde P7) y comparados en PREPARE y al inicio de MUTATE:

| Campo | Motivo |
|---|---|
| `ObjectId`/handle estable | identidad de la fuente |
| existencia y no borrada | un comando transparente pudo borrarla |
| `BlockTableRecord`, `IsDynamicBlock`, `DynamicBlockTableRecord`, `IsAnonymous`, `Annotative` | la referencia pudo re-apuntarse o transformarse (ST-17) |
| `Position`, `Rotation`, `ScaleFactors`, `Normal` | `P` y el contrato de fuente (§4.1) |
| presentacion tipada (capa, color, tipo de linea, escala de tipo de linea, grosor, transparencia, visibilidad y, con STB, estilo de trazado) y estado no transportable o sin clasificar (inventario cerrado de CT-49) | la referencia nueva copia la presentacion exterior (ST-13); ST-18 |
| orden efectivo en Model Space de las fuentes y su contexto de pantalla y de trazado (`SortentsTable` y las variables que fije CT-49) y, desde PREPARE, de **todos** los objetos de Model Space posteriores a cada fuente que intervienen en ST-19 (identidad, existencia, huella visual y posicion en el orden de dibujo) | un cambio invalida la demostracion del orden (ST-19, `[V8-D25]`, `[V9-D01]`) |
| `MaterializationContextReadSet` (§8.7): solo las observaciones del contexto de materializacion que los productores y las huellas visuales realmente consumen | la generacion de las dos mitades, las huellas y la materializacion usan las mismas observaciones; un estado no consumido no invalida (`[V9-D05]`, `[V10-D05]`, PRE-17) |
| `FootprintInputFingerprint` de **toda** instancia cuya huella visual se uso (desde P7; recalculado al releer el dibujo real tras `EnsureForPlan`): identidades de las definiciones reales, registros de simbolos resueltos usados, observaciones consumidas del contexto de materializacion, estado dinamico efectivo, identidades de las dependencias anidadas y cualquier otra entrada que altere la huella | la huella visual que decide ST-19 y el orden de plan describe lo que MUTATE materializa (`[V10-D03]`, PRE-18) |
| `Origin` de la definicion | ST-14 |
| hash del payload RackCad de la definicion | el documento reflejado depende de el |
| **read-set del registro** (solo si algun grupo consume el registro) | las **observaciones** que la autoridad efectiva realmente uso para decidir (§9.2.1). **No** incluye ninguna «version del registro» ni una huella del registro completo |

Cualquier diferencia de un campo observado ⇒ **fail-closed antes de mutar** (E10 en PREPARE; en MUTATE, excepcion y
rollback). Una diferencia del `FootprintInputFingerprint` al releer el dibujo real tras `EnsureForPlan` obliga a
recalcular la huella y a re-evaluar el orden de plan y ST-19, y solo falla si ya no se demuestran (PRE-18); al inicio de
MUTATE, cualquier diferencia respecto de esa postcondicion ⇒ excepcion y rollback.

#### 9.2.1 Read-set por observaciones (V4-08)

| Observacion | Contenido | Autoridad |
|---|---|---|
| RS-1 Acreditacion | resultado de la lectura (`ProjectVariablesReadOutcome`: ausente, legible, ...) y resultado de la acreditacion **realmente usado** (`Usable`, `AmbiguousIdentity`, `NotReadable`, `InvalidTarget`) | `UsableProjectVariablesRegistry.cs:7-23`, `:120-140` |
| RS-2 Variables leidas | cada `VariableId` que la resolucion efectiva **realmente** leyo al inspeccionar los vinculos del rack: presencia, `VariableType` y definicion observable (literal) | `SelectiveLinkedPropertyKernel.InspectBindings` → `LinkedPropertyInspection.InspectBinding` (`SelectiveLinkedPropertyKernel.cs:121-150`) |
| RS-3 Dependencias transitivas | **solo si I-49 integra antes**: las observaciones de su `PlanReadSet` (resultado de cada simbolo realmente usado, incluidas las dependencias de expresiones) | I-49 V6 D19; ADR-0040 (aceptado @ `2eeeab1`, tras el rebase `8f6d583`; reemplaza a ADR-0038, aceptado @ `edafade`, sin cambiar D19) |
| RS-4 Observaciones adicionales | cualquier dato adicional que la autoridad efectiva vigente necesite para repetir la misma decision (por ejemplo, el dominio del consumidor si I-49 lo aplica) | la autoridad efectiva vigente |

- Un cambio **irrelevante** (una variable no observada, una entrada nueva no leida) **no** aborta. Un cambio de algo
  **observado** ⇒ **abort before MUTATE** (E10 en PREPARE; excepcion y rollback en MUTATE).
- I-52 **no** implementa un segundo motor de expresiones ni su propio calculo de dependencias: si I-49 integra, adopta su
  autoridad de lectura y sus observaciones transitivas (PV-6).
- Pruebas: T-M41 y CT-40.

### 9.3 Tres garantias distintas

| Garantia | Estado | Contenido |
|---|---|---|
| **Atomicidad semantica del rack** | **garantizada** | Una sola transaccion MUTATE con todas las definiciones RackCad, payloads y `BlockReference`. Cualquier fallo ⇒ rollback de **todas** las copias. Nunca queda una copia parcial ni un payload RackCad nuevo sin su definicion |
| **Atomicidad de infraestructura** | **NO garantizada** | PREPARE puede importar definiciones de biblioteca, bloques anidados, capas, estilos y demas dependencias, y pueden **permanecer** aunque despues falle PREPARE o MUTATE (§9.5) |
| **UNDO** | **UNKNOWN** hasta la validacion del Owner en AutoCAD | No hay marcas de undo en el Plugin (busqueda `UndoMark`: cero); rige el grupo de undo por comando. **No** se promete que un UNDO revierta las importaciones (M-10, M-20) |

La garantia de I-52 es **cero mutacion semantica** en cualquier fallo, no «cero mutacion de la base de datos».

### 9.4 Tabla de fallos

| # | Fallo | Momento | Resultado |
|---|---|---|---|
| E1 | Ninguna fuente valida (todos los objetos ignorados en C1..C3 de §4.1) | P1 | fin, sin linea |
| E2 | Payload RackCad inutilizable o ilegible (ST-2c, incluido F-14a de I-54), `Kind` sin reflector o sin handler, `Design` vacio | P1 | fin, cero mutacion |
| E3 | Grupo inconsistente (`Kind`, nombre, autoridad authored) | P2 | fin, cero mutacion |
| E4 | Candidato RackCad no canonico (C4 de §4.1: ST-3, **ST-17**, **ST-18**, ST-5, ST-6, ST-10, ST-9, ST-7, ST-14, ST-15) | P1 | fin, cero mutacion; ST-17 y ST-18 con mensaje propio («bloque dinamico, anonimo o anotativo»; «estado de presentacion de la referencia que el espejo no puede conservar»), nunca un aviso |
| E5 | Vista seleccionada no admisible (lateral de rack, cama) | P1 | fin, cero mutacion; el mensaje sugiere reflejar las vistas admisibles e insertar las demas con `RACKEDITAR` desde la copia |
| E6 | Fila `REQUIRES_MODEL_CHANGE` o `UNKNOWN` material (topes Selectivo, tope posterior de Push Back **activo por defecto** u `OffCells` fuera de rango, medio frente dependiente, desviador inestable, parrilla o tarimas desbordadas, **parrilla o tarimas sin `DEP-ROW` (S-15d, S-17d)**, **tarimas frontales de Push Back desbordadas (P-03c, PC-14b)**, Auto con baseline, **estado dinamico dependiente de vinculos (S-26, S-27a..S-27c)**), V-META (payload, exterior, **retirados, enums no definidos y colisiones**), V-DEP (obligaciones 9 y 9b), **V-VIEW-ALL** (incluidas vistas admisibles no seleccionadas), X-16, **X-17a/X-17b/X-17c, X-22 (incluido el orden visual de plan), X-23**, X-18, V-BOM o V-RT-STORE fallidas; diseño bloqueado o invalido | P4..P10 | fin, cero mutacion; nombra rack, vista, propiedad, pieza, clave o miembro, con el mensaje de R-A o R-B cuando corresponda |
| E7 | Registro **no acreditable** (presente e ilegible, version incompatible o identidad ambigua) **y** algun grupo cuyo kind lo consume (hoy: Selectivo, con o sin vinculos); o vinculo roto | P3 | fin, cero mutacion. Si ningun grupo lo consume, el registro no se evalua (§10.2) |
| E8 | Ensayo de `Compose` + serializacion + restamp + nombre fallido, incluidos F-14b de I-54 (la reserializacion lanza `JsonException`) y la regla F de `Compose` (origen con una clave de `ExtensionData` igual a un miembro declarado) | P9, P10 | fin, cero mutacion; toda excepcion de esos pasos se traduce a E8 con mensaje atribuible |
| E9 | Linea invalida o cancelada | LINE | fin, cero mutacion |
| E10 | Orden visual en Model Space no demostrable frente a algun objeto posterior a una fuente, huella visual o contexto de orden no disponibles, `XLINE`/`RAY` posterior sin interseccion analitica u ordenes de pantalla y de trazado contradictorios con copias superpuestas (ST-19, como rechazo anticipado antes de importar o como decision definitiva despues), fallo de plan, bloque ausente tras importar, definicion real de una pieza con cambio de mano cambiada y ya no equivalente en geometria, fuentes visuales, visibilidad u orden visual (PRE-13), huella visual u orden de plan que ya no se demuestran al refrescar tras `EnsureForPlan` (PRE-18), huella de la referencia, read-set del registro u observacion consumida del `MaterializationContextReadSet` cambiados (PRE-10, PRE-17) | PREPARE | fin; **ninguna** definicion, referencia ni payload RackCad nuevo; lo importado por `EnsureForPlan` puede permanecer (§9.5) |
| E11 | Excepcion en MUTATE, incluidas la huella de la referencia, los read-sets o el `FootprintInputFingerprint` cambiados respecto de la postcondicion de PREPARE y `MissingInstances.Count > 0` | MUTATE | sin commit ⇒ rollback de **toda** la operacion semantica |

### 9.5 Contrato de infraestructura: `EnsureForPlan` / `EnsureBlocks`

Hechos del codigo (`BlockLibraryImporter.cs`, sin cambios en `main`):

| # | Hecho | Evidencia |
|---|---|---|
| IF-1 | Debe llamarse dentro del lock y **fuera** de la transaccion del dibujo | `:12-18` |
| IF-2 | Comprueba en una transaccion de lectura que bloques faltan | `:55-67` |
| IF-3 | Biblioteca ausente ⇒ devuelve 0 sin error | `:74-78` |
| IF-4 | Biblioteca ilegible ⇒ se registra y devuelve 0 (cache de sesion) | `:149-161` |
| IF-5 | Clona con `WblockCloneObjects(ids, BlockTableId, mapping, DuplicateRecordCloning.Ignore, deferTranslation: false)`, que arrastra dependencias (capas, estilos, bloques anidados) y conserva las existentes con el mismo nombre | `:109-110` |
| IF-6 | Captura **toda** excepcion, la registra y devuelve 0: importacion **parcial o nula** sin fallo visible | `:113-118` |
| IF-7 | `CreateSystemBlock` omite despues las piezas sin bloque | `LateralHeaderDrawer.cs:293-303` |
| IF-8 | Si no falta ningun bloque en el destino, devuelve 0 sin clonar: el retorno no distingue «nada que importar» de un fallo silencioso (IF-3, IF-4, IF-6) | `:69-72` |

Contrato de I-52:

1. PREPARE llama a `EnsureForPlan` por plan de familia `HeaderRun`.
2. PREPARE **re-verifica** en lectura todas las dependencias requeridas por el plan y registra las faltantes.
3. Faltantes ⇒ E10: no se crea nada semantico. Lo ya importado **puede quedar** en el dibujo.
4. MUTATE vuelve a comprobar con `MissingInstances`; si hay faltantes, lanza y revierte (E11).
5. Ninguna afirmacion de atomicidad de infraestructura ni de UNDO de importaciones en mensajes, ADR ni documentacion.
6. Limitacion declarada: `DuplicateRecordCloning.Ignore` conserva un bloque del dibujo con el mismo nombre aunque su
   contenido difiera del de la biblioteca; afecta a toda vista de RackCad, no solo al espejo.
7. **Sonda (`[V9-D07]`, `[V10-D02]`, `[V10-D09]`).** La sonda obtiene cada definicion por una de dos rutas autorizadas:
   (1) si ya existe en el dibujo, con un clonado de solo lectura del dibujo hacia la base auxiliar por la costura de
   AutoCAD que caracterice CT-36; (2) si solo existe en la biblioteca, con `EnsureBlocks(auxDb, nombres)` (o la API
   publica vigente) sobre una base auxiliar privada —IF-1 no aplica a esa base, que no es el documento (CT-36 lo
   confirma)—. En el `DestinationEvaluationContext` rige una sola precedencia, **DRAWING FIRST, LIBRARY SECOND WITH
   IGNORE**: primero se clonan desde el dibujo el estado y los nombres relevantes que ya existen en el (bloques raiz,
   bloques anidados, capas, tipos de linea, estilos y otras dependencias de representacion) y despues se importa de la
   biblioteca solo lo que falta, con `DuplicateRecordCloning.Ignore`, como hara `EnsureForPlan` (IF-5); nunca
   library-first, y una colision que no puede simularse como la resolvera MUTATE ⇒ `UNKNOWN`. El retorno de
   `EnsureBlocks` es solo diagnostico: por IF-8 devuelve 0 tambien cuando no faltaba ningun bloque y, por IF-3, IF-4 e
   IF-6, tambien ante un fallo silencioso. La autoridad es la verificacion posterior: **todos** los BTR requeridos
   existen en la base auxiliar tras el intento de clonado; si falta alguno ⇒ `UNKNOWN` (X-17a en la evidencia de
   simetria; huella visual `UNKNOWN`). Nada de esto modifica `BlockLibraryImporter.cs` (§20.3).

---

## 10. Persistencia, variables de proyecto y portadores

### 10.1 Composicion del payload (por definicion nueva)

```text
1. sobreFuente   = RackEmbedStore.Deserialize(payloadFuente)
2. vista         = §3.7 (View canonica, σ' canonica) — o fallo
3. reflejado     = Reflector(kind).Reflect({ payload interior = sobreFuente.Design, ... })  en el sustrato de §6.2
4. compuesto     = RackEmbedComposer.Compose(sobreFuente, sobreFuente.Kind, sobreFuente.Id, sobreFuente.Name,
                                             ViewCanonica, σ', reflejado.Design)
5. payload       = RackEnvelopeRestamp.RestampEnvelope(RackEmbedStore.Serialize(compuesto), "<base> - espejo", NewRackId)
                   (Plugin; restamp interior por KindHandlerRegistry)
6. postcondicion = V-RT-STORE sobre el payload final; portadores de §6.4 identicos
```

Todas las definiciones del grupo reciben **el mismo** payload interior reflejado salvo los metadatos propios de su
vista; en Selectivo se re-verifica `IsSameAuthority` sobre los documentos reflejados.

### 10.2 Variables de proyecto

| Regla | Contenido |
|---|---|
| PV-1 | Los vinculos authored se preservan **exactos**: mismo `PropertyId`, `VariableId`, `Kind` y literal congelado |
| PV-2 | El espejo **no** crea variables, **no** desvincula ni revincula, **no** fija literales, **no** materializa, **no** escribe el registro y **no** escribe en el documento ningun valor calculado a partir del registro (obligacion 9) |
| PV-3 | El espejo **no** evalua expresiones con logica propia |
| PV-4 | Para generar geometria consume **en solo lectura** la autoridad efectiva existente del kind sobre la lectura unica del registro hecha en SNAPSHOT (`ProjectVariablesRegistry.Read`, `ProjectVariablesRegistry.cs:22`) |
| PV-5 | **Criterio de acreditacion = el de la autoridad efectiva existente, por kind** (CR-V2-1). Selectivo: `SelectiveEffectiveDesignResolver.ResolveAccredited` exige un registro acreditable (ausente o legible) **aunque el rack no tenga vinculos** (`SelectiveEffectiveDesignResolver.cs:52-66`; `UsableProjectVariablesRegistry.cs:120-135`); asi abren el editor (`SelectiveEditorOpen.cs:184`), reconcilia `RACKEDITAR` (`LinkedPropertyReconciler.cs:131-139`) y cotiza `RACKBOMTOTAL` (`RackInventarioCommands.BomTotal.cs:56-67`). Dinamico, Push Back, Cantilever y cabecera no tienen vinculos e **ignoran** el registro (`DynamicKindHandler.cs:23-25`; `PushBackKindHandler.cs:26-28`; `CantileverKindHandler.cs:33-35`; `CabeceraKindHandler.cs:21-23`). I-52 no cambia esa semantica |
| PV-6 | Si I-49 integra antes (hoy: V6 y Amendment A1 congelados, ADR-0040 aceptado en lugar de ADR-0038; G5 sintactico `977f873`, antes `c4880af`, y G6 `5f969cc` con enlace y evaluacion de un arbol en su rama; sin integrar y sin `PlanReadSet` ni dominio aplicado; `[V9-D21]`, `[V10-D14]`): RACKMIRROR invoca la autoridad efectiva integrada, preserva `expression` como portador, adopta su `PlanReadSet` para el read-set (§9.2.1) y su dominio de consumidor como cota autoritativa de V-DEP (§6.5.5), y **no** reimplementa su evaluacion ni un segundo motor de expresiones (P24.5 y P24.8 de I-49 V6) |
| PV-7 | La copia es un **consumidor nuevo** del mismo `VariableId` (ADR-0034 §12): bloquea `Delete` y se redibuja en `ChangeValue` desde su authored reflejado. **`ChangeValue` no puede invalidar ninguna decision `ALLOW` o `CANONICALIZE` tomada en PREFLIGHT** (obligacion 9b), **incluida la aceptacion de un cambio de mano cuya evidencia visual-geometrica dependa de un parametro dinamico vinculado** (§11.3 punto 12): un rack con alguna decision cuya verdad dependa de un valor vinculado sin prueba para todo `R` no es admisible (S-02b, S-14c, S-15d, S-17d, S-26, S-27a..S-27c y toda fila que V-DEP marque) |

### 10.3 `DimensionViews` (I-50 integrada)

`DimensionViews` es **requisito normativo** del primer corte. Cada reflector es un **sitio de copia**: transporta el
`int` exacto, incluidos bits desconocidos y `null`, por DTO en Selectivo (`SelectivePalletDesignDocument.cs:108-116`) y
por dominio en Dinamico (`DynamicRackSystemDocument.cs:63-72`) y Push Back. **T-M18 es incondicional**: centinelas `-8`,
`13` y `null` a traves del espejo (mismo metodo que `DimensionViewsCopySitesTests`). Las firmas de anotaciones de C-2 y
V-VIEW-ALL se evaluan con la politica de cotas de I-50.

### 10.4 Metadata, I-54 y deuda de cabecera

- La fidelidad del store por kind es la de §6.2 y se mide con V-RT-STORE (§6.3); la metadata semantica desconocida la
  bloquea V-META (§6.4).
- **Cabecera**: el payload no tiene `ExtensionData` y su version vuelve a `"1.0"`. Es deuda conocida, no se arregla en
  I-52; V-META bloquea un payload de cabecera con miembros desconocidos.
- **I-54 (integrada en `main @ ba497f1`: merge del cierre documental `d3cc843` sobre el Candidato `02987bd`; ADR-0039
  aceptado; rama remota retirada; `[V8-D19]`, `[V10-D13]`)**: las propiedades de alcance Rack viven en el miembro
  `CustomProperties` del sobre (`JsonElement?`, `RackEmbedDocument.cs:67` @ `ba497f1`); `Compose` lo hereda y lo
  normaliza (`RackEmbedComposer.cs:57`) y no hay promocion de major. Es **portador RACK-LEVEL por nombre** (CA-4, §6.4)
  y el espejo lo conserva; las Custom Properties de alcance Proyecto del NOD (CA-4b) son un alcance separado que el
  espejo no lee para reflejar el rack ni toca (G-M22, `[V9-D10]`, `[V10-D11]`). **D-21 sin cambio** (ADR-0039 §6): el
  espejo cumple el invariante (A y despues B). Los residuales preexistentes del store del sobre que declaran V4 y V5 de
  I-54 afectan a todo flujo que lo reescribe, tambien al espejo: **F-14a** (UTF-16 crudo invalido: la lectura lanza
  `ArgumentException`) ⇒ E2 (ST-2c); **F-14b** (surrogate suelto escapado: la reescritura lanza `JsonException`) ⇒ E8 en
  el ensayo de P10; y su **regla F** (`Compose` rechaza un origen con una clave de `ExtensionData` igual, sin distinguir
  mayusculas, a un miembro declarado; `RackEmbedComposer.cs:46`) ⇒ E8. Con I-54 ya en main: T-M20 es incondicional; el
  censo T-GRD-02 pasa de 7 a 8 cuando el plan del espejo introduce su llamada a `Compose`, que se clasifica en esa
  guarda con motivo; T-M16 reapunta T-GRD-08 (censos de comandos, de ayuda y de ventanas) con la entrada de ayuda sin
  alias (`[V10-D08]`); T-GRD-01 (solo el compositor construye sobres) y T-GRD-03 (forma del restamp) siguen verdes,
  porque el espejo no construye sobres (ID-5) ni modifica el restamp (ID-6); y, como `Compose` normaliza un
  `CustomProperties` `Null` o `Undefined` a ausente para todos los productores (T-ENV-05), T-M10 y T-M20 comparan con
  esa normalizacion. V5 de I-54 (C-F3) precisa que en los consumidores existentes el punto de fallo de F-14b depende de
  su granularidad transaccional (`RACKEDITAR` puede quedar parcialmente actualizado entre vistas); en el espejo el
  ensayo de P10 serializa todos los payloads **antes** de MUTATE, asi que F-14b termina en E8 sin ninguna mutacion
  parcial.

---

## 11. Equivalencia ejecutable bajo reflexion

### 11.1 Definicion

Para una vista admisible `(V, σ)`:

```text
A = firma(Π_(V,σ')(Eff(μ_k D, R)))          plan reflejado semanticamente
B = firma(F ∘ Π_(V,σ)(Eff(D, R)))           plan original transformado por F
A ≡ B  ⇔  multiconjuntos iguales de piezas fisicas bajo las reglas de §11.2..§11.5
          ∧ para cada par (a, b) de instancias de B con superposicion visualmente material y sus homologas (a', b') en A:
            RelativePlanVisualOrder_B(a, b) = RelativePlanVisualOrder_A(a', b')           (§11.3 punto 6.6, [V8-D12])
```

El espejo es fiel al **plan regenerado** del documento fuente, **no** a las entidades actuales de su definicion, que
pueden estar desactualizadas o incompletas por bloques que faltaban al dibujarla. Es la misma semantica de `RACKEDITAR`
→ Actualizar.

### 11.2 Firma

| Componente | Regla |
|---|---|
| Piezas | Roles fisicos, incluido `Pallet`; `Annotation` y `Dimension` se validan aparte (§11.6) |
| Aplanado | Cada instancia de un `HeaderGroup` se lleva a coordenadas de la definicion componiendo la colocacion con su transformacion completa (rotacion incluida). **No** se usa `HeaderRunPlan.Flatten`/`PlacedClone` (`HeaderRunPlan.cs:53-80`) |
| Identidad de pieza | `PieceId`, `BlockName`, `View` |
| Posicion | `Insertion` y `ConnectionAnchor` con `GeometryTolerance.Length`; `GeometryTolerance.Continuity` solo donde una normalizacion declarada escribe un valor calculado (N-S02) |
| Orientacion | rotacion modulo 2π con `GeometryTolerance.Angle` |
| Escala y mano | `MirroredX`, `MirroredY` y signo del determinante total de la pieza |
| Parametros dinamicos | §11.4 |
| Apariencia de la pieza | la firma de plan no mira entidades internas; la politica de variantes, las fuentes visuales simbolicas tipadas, la `VisibilityPath`, el contexto de patron y el orden visual de las piezas con cambio de mano los valida `GeometricEvidence` (§11.3 puntos 6-8) |
| Orden visual entre piezas | pares de instancias, con o sin cambio de mano, cuyo orden relativo cambia y cuyas huellas de ocupacion de modelo (`VisualFootprintResult`, §11.3 punto 5), refrescadas tras `EnsureForPlan` (PRE-18), se superponen de forma visualmente material: orden relativo de emision y materializacion (grupos de cabecera y sus colocaciones, despues piezas sueltas: `LateralHeaderDrawer.cs:36-76`; mismo orden en `RedefineSystemBlock`, `:130-161`; curvas de Cantilever en su orden) y `SortentsTable` aplicable, conservado en las homologas; huella `UNKNOWN` o superposicion no clasificable ⇒ X-22 (§11.3 punto 6.6) |
| Curvas Cantilever | contornos con la regla de inversion de `Transform2D` (`PathSegment2D.cs:139-160`), comparados como conjuntos de segmentos (independientes del sentido de trazado) |
| Rol de capa Cantilever | `CantileverVisualRole` de cada curva, decidido en el plan (`CantileverViewMaterializer.cs:207-214`) |
| Seccion | `(V, σ')` frente a `(V, σ)` canonicas (§3.7) |
| BOM | §11.5 |
| Anotaciones | contratos semanticos (§11.6) |

Ninguna tolerancia oculta una holgura unilateral sistematica: `1e-9` in frente a `0.25"`.

### 11.3 Mano y equivalencia visual-geometrica evaluada por estado

1. **Ningun bloque DWG se asume simetrico.** La biblioteca no esta versionada, no tiene metadato de simetria y el
   manifiesto de I-19 no registra geometria (`CatalogBlockManifest.cs:12-59`). Casi todas las piezas de frontal y planta
   son bloques dinamicos (`LONGITUD`, `PERALTE`, `ALTURA`, ...) que el builder emite sin `MirroredX`
   (`SelectiveFrontalBuilder.cs:450-470`, `:475-491`): en `B = F ∘ Π(D)` quedan volteadas y en `A = Π(μ_k D)` no
   (notacion de §11.1, `[V10-D12]`), asi que su equivalencia exige **aceptar un cambio de mano**. Las piezas que el plan
   ya emite en pareja espejada por lado (protectores laterales, guias) no cambian de mano y no pasan por la sonda.
2. **Dos evidencias, con orden obligatorio.** Un cambio de mano solo se acepta si pasan **las dos**:

   | Evidencia | Que demuestra | Como |
   |---|---|---|
   | **`GeometricEvidence`** (primero) | que la pieza **real**, en el **estado concreto** que usa, es igual a su reflexion respecto de `(eje, centro)` **en variante de clase, en geometria, en fuentes visuales simbolicas tipadas, en ruta de visibilidad y en orden visual** | **STATE-EVALUATED VISUAL-GEOMETRIC EVIDENCE** (puntos 3-10); nunca derivada de builders, de la diferencia entre planes, de inserciones ni de un minimo de error |
   | **`PlanPlacementEvidence`** (despues) | que el builder coloca la pieza de forma coherente con esa simetria: `A ≡ B` con el centro verificado | V-VIEW-ALL por rack en PREFLIGHT (§3.8, §6.6); CT-06 y T-M12 como caracterizacion y prueba. Nunca ajusta, infiere ni corrige `c` |

   Una pieza simetrica **no** oculta un offset del builder: con el centro verdadero, un desplazamiento hace `A ≢ B`
   (punto 9) ⇒ X-13/X-17a `UNKNOWN` ⇒ `FAIL_CLOSED`.
3. **Ruta primaria: STATE-EVALUATED VISUAL-GEOMETRIC EVIDENCE.** Para cada pieza de **cualquier** vista admisible del
   rack (seleccionada o no) cuya equivalencia requiera aceptar un cambio de mano:

   ```text
   (BlockName, definicion real, vector de parametros dinamicos, marco local de insercion)
           ↓  crear una base auxiliar privada con el contexto de la autoridad origen (punto 4) y clonar alli la
              definicion: desde el dibujo, en solo lectura, si ya existe; si no, con
              BlockLibraryImporter.EnsureBlocks(auxDb, nombres); verificar que existen todos los BTR requeridos
              tras el intento de clonado (punto 14); nunca evaluar en el dibujo ni en la cache
           ↓  aplicar los parametros con la semantica del materializador (punto 5)
           ↓  alcanzar el punto de observacion caracterizado y leer valores efectivos, BTR evaluado, Origin y
              transformacion efectiva (puntos 5 y 13)
           ↓  aplanar recursivamente (anidados con su transformacion completa, su propio estado, su BTR evaluado y su
              paso de ruta estable) y comprobar la politica cerrada de clases y variantes (punto 6.1)
           ↓  resolver fuentes visuales simbolicas tipadas, VisibilityPath, contexto de patron y orden visual de cada
              primitiva (punto 6)
           ↓  canonicalizar de forma conservadora dentro de cada firma y probar la simetria de reflexion (puntos 7-8)
           ↓
   GeometrySymmetryResult
   ```

   Las instancias sin cambio de mano que enumera P7a (§9.1) pasan por el mismo clonado, evaluacion y aplanado, sin la
   prueba de simetria, en el `DestinationEvaluationContext` (punto 4), para producir su `VisualFootprintResult` (punto
   5, `[V9-D02]`, `[V10-D02]`); las piezas con cambio de mano producen su huella en ese mismo contexto.

   **Fuente de la definicion.** Si el bloque ya existe en el dibujo, la evidencia corresponde a **esa** definicion,
   identificada por nombre como la identifica el drawer (`LateralHeaderDrawer.cs:293-305`). Si no existe, se inspecciona
   en PREFLIGHT la definicion de la biblioteca candidata, **clonada** en la base auxiliar con la API publica
   `BlockLibraryImporter.EnsureBlocks(auxDb, nombres)` (`BlockLibraryImporter.cs:37-119`), que la copia desde la cache
   privada de sesion (`:127-168`) sin modificar el archivo; la sonda verifica despues que existen todos los bloques
   requeridos tras el intento de clonado (punto 14, `[V9-D07]`). Tras `EnsureForPlan`, **PREPARE** vuelve a verificar la
   definicion **real** que quedo en el dibujo para las vistas que se dibujan: `DuplicateRecordCloning.Ignore` conserva
   una definicion local con el mismo nombre (`BlockLibraryImporter.cs:110`), asi que la biblioteca sola nunca basta si
   el DWG ya contiene el bloque. Para las vistas admisibles no seleccionadas, la equivalencia se afirma sobre la
   definicion evaluada en PREFLIGHT; la que un Insertar futuro importe queda bajo la limitacion general de §9.5 punto 6,
   igual que para el rack original; si la evidencia de una vista admisible, seleccionada o no, puede haber cambiado por
   simbolos que `EnsureForPlan` importo o por colisiones de nombres, PREPARE la vuelve a evaluar (PRE-18, `[V10-D03]`).
   Como `GeometrySymmetryResult` compara **fuentes** y no valores resueltos (punto 6), las capas que el dibujo ya
   tuviera con otras propiedades no invalidan la evidencia de simetria; **no** ocurre lo mismo con la huella visual, que
   se resuelve en el dibujo destino (punto 4, `[V10-D02]`). El resultado depende de la **biblioteca efectiva** (L-28).
4. **Sin mutacion del DWG del usuario ni de la cache.** La evaluacion se hace en solo lectura o clonando y evaluando en una
   `Database` auxiliar privada y descartable; **nunca** inserta artefactos persistentes (referencias temporales,
   definiciones anonimas, capas) en el dibujo del usuario ni en la `Database` cacheada de la biblioteca. La postcondicion
   medible esta en el punto 14 (G-M16, G-M20, T-M53, T-M63).

   **Contexto de la base auxiliar para la evidencia de simetria (`[V8-D07]`).** La base auxiliar reproduce el contexto
   necesario de la **autoridad origen** de la definicion: como minimo `PSTYLEMODE`, `INSUNITS`, la escala de anotacion
   cuando pueda afectar a la evaluacion y cualquier contexto adicional que descubra CT-36. La autoridad origen es el
   **dibujo actual** si la definicion ya existe en el; si solo existe en la biblioteca, la autoridad inicial es la base
   de la biblioteca y la definicion real importada se vuelve a verificar en PREPARE (punto 3), ya en el contexto del
   dibujo. Si el contexto relevante no puede reproducirse o verificarse en la base auxiliar ⇒ `UNKNOWN` (X-17a). Ninguna
   fuente se lee despues de un clonado que haya cambiado ese contexto (por ejemplo, entre modos de estilos de trazado
   distintos). CT-36 incluye un dibujo con estilos de trazado con nombre (STB) y otro dependiente del color (CTB). Las
   observaciones consumidas del contexto de materializacion (§8.7) se reproducen en la base auxiliar en lo que puedan
   afectar a la evaluacion.

   **Contexto de evaluacion destino (`DestinationEvaluationContext`, `[V10-D02]`, `[V10-D09]`).**
   `VisualFootprintResult` **no** se resuelve con el contexto de la biblioteca: representa lo que MUTATE materializara
   en el **dibujo destino**.

   ```text
   DestinationEvaluationContext (nombre ilustrativo; no es schema persistido)
     definicion que ya existe en el dibujo   ⇒ se evalua la del dibujo actual (clonado de solo lectura)
     definicion que falta                    ⇒ la base auxiliar simula su importacion al dibujo:
         1. clonar PRIMERO desde el dibujo el estado y los nombres relevantes que ya existen en el: bloques raiz,
            definiciones de bloque anidadas, capas, tipos de linea, estilos de texto y de cota y cualquier otro
            registro de simbolos que altere la representacion
         2. DESPUES importar desde la biblioteca solo lo que falta, con DuplicateRecordCloning.Ignore
     DRAWING WINS: nunca library-first; el resultado es el que dejara EnsureForPlan en el dibujo (§9.5, IF-5)
     cobertura transitiva de los nombres relevantes; conjunto no determinable, colision que no puede simularse como la
       resolvera MUTATE o registro de simbolos no reproducible ⇒ UNKNOWN (huella visual UNKNOWN; X-17a en la
       evidencia de simetria de la pieza)
     incluye las observaciones consumidas del MaterializationContextReadSet que afecten a la huella (§8.7)
   ```

   Si una base auxiliar se comparte entre piezas o entre rutas, rige la misma precedencia; una base por pieza tambien es
   valida si reproduce el mismo resultado. La regla del contexto de la autoridad origen (parrafo anterior) queda
   limitada a `GeometrySymmetryResult` donde corresponda. CT-36 y T-M63 incluyen una colision de nombre de bloque
   anidado y otra de nombre de simbolo (por ejemplo, una capa con el mismo nombre y otras propiedades en el dibujo y en
   la biblioteca).
5. **Salida pura, paridad de parametros y punto de observacion.** La infraestructura AutoCAD del Plugin produce datos
   planos; Application **no** referencia `Autodesk.*` (G-M3). Contrato conceptual (nombres no contractuales; no es schema
   persistido):

   ```text
   GeometrySymmetryResult {
       BlockName
       EvaluatedStateSignature     // definicion evaluada + valores EFECTIVOS + transformacion efectiva de la instancia
       Axis                        // eje local X o Y (punto 7)
       Center                      // candidato verificado, en el marco del BTR evaluado (punto 13)
       IsSymmetric                 // geometria + VisualSignature + orden visual (puntos 6-8)
       SupportedVariants           // clases y variantes encontradas (6.1); fuera de la politica cerrada ⇒ IsSymmetric = false
       TraceFingerprint            // punto 10
       Diagnostics                 // entidad, variante, fuente, ruta, orden, ciclo, ambiguedad, eje oblicuo, paridad,
                                   // transformacion, contexto de la base auxiliar, cache
   }
   ```

   **Paridad con el materializador (`[V7-D13]`).** La sonda aplica el vector exactamente como
   `LateralHeaderDrawer.ApplyDynamicParameters` (`LateralHeaderDrawer.cs:415-449`), y CT-36 caracteriza y fija cada punto:
   - solo actua sobre referencias dinamicas y con valores (`:417-420`);
   - **busqueda sin distinguir mayusculas**: el vector se copia a un diccionario `OrdinalIgnoreCase` en el orden en que se
     enumera, asi que con dos claves que solo difieren en mayusculas **gana la ultima enumerada** (`:425-429`);
   - una propiedad `ReadOnly` **no** se escribe y una clave sin propiedad se **ignora** sin error (`:431-438`);
   - la conversion del valor es la de la API (`property.Value = value`, un `double`); una excepcion de conversion es fallo
     de la sonda (X-17a), no un valor por defecto;
   - el orden es el de `DynamicBlockReferencePropertyCollection` (`:432`); si CT-36 demuestra que el orden es material
     para algun bloque, se fija o ese bloque falla cerrado;
   - **`applied`**: si se aplico con exito al menos una propiedad, se llama a `RecordGraphicsModified(true)`; si no se aplico
     ninguna, **no** se llama (`:431`, `:437`, `:445-448`).

   **Punto de observacion (`[V7-D14]`).** CT-36 determina en `acad.exe` el punto soportado en que AutoCAD deja visible, en la
   base auxiliar, el estado evaluado equivalente al que el materializador deja en un documento real. **No** se asume que
   baste confirmar la transaccion, reabrir, regenerar u otra operacion. **Despues** de ese punto se leen: los valores
   dinamicos efectivos, el BTR evaluado, su `Origin` y la transformacion efectiva de la referencia (punto 13).
   `EvaluatedStateSignature` usa esos valores efectivos, no los solicitados. Si la base auxiliar no reproduce la salida del
   materializador (`[V6-D14]`) ⇒ X-17a. Objetivo de implementacion: **una sola costura** de aplicacion compartida por
   materializador y sonda; si no es viable, la prueba de equivalencia T-M56 es obligatoria (G-M19).

   **Huella visual (`VisualFootprintResult`, `[V9-D02]`, `[V10-D01]`..`[V10-D04]`, `[V10-D07]`).** Es una autoridad
   **distinta** de `GeometrySymmetryResult`: `GeometrySymmetryResult` demuestra **simetria**; `VisualFootprintResult`
   demuestra **ocupacion de modelo y superposicion**. Ninguna sustituye a la otra (G-M23).

   ```text
   VisualFootprintResult {                 // nombres no contractuales; no es schema persistido
       Instance                            // instancia de plan (pieza, curva, anotacion o cota) u objeto de Model Space
       Frame                               // siempre WCS (plano XY de §3.1): las regiones se construyen DESPUES de
                                           //   aplanar y transformar hasta WCS (anidados, instancia y P')
       Regions[]                           // OCUPACION DE MODELO en WCS, sobre-aproximada, por clase material:
                                           //   soporte de modelo de los trazos (geometria de ancho cero) · ancho
                                           //   geometrico real de la entidad si existe · relleno · mascara con tamaño
                                           //   en modelo · limites de texto y de cota bajo su contexto resuelto · marco
                                           //   o borde · contenido anidado aplanado · otra ocupacion expresable de
                                           //   forma conservadora en WCS
       StrokeSignatures                    // firmas visuales de los trazos (para «trazos coincidentes con firma distinta»)
       InputFingerprint                    // FootprintInputFingerprint (§9.2, PRE-18)
       Status                              // CONSERVATIVE | UNKNOWN
       Diagnostics                         // clase, margen de modelo aplicado, contexto usado, motivo de UNKNOWN
   }
   ```

   - **Ocupacion de modelo, no de pantalla ni de papel (decision vinculante del Coordinador).** La huella representa lo
     que la instancia ocupa en el modelo y no intenta convertir a WCS efectos puramente de pantalla o de papel: **no**
     se dilata por `LineWeight` de AutoCAD, `LWDISPLAY`, grosores de CTB/STB, milimetros de papel ni escala de trazado.
     `LineWeight` sigue perteneciendo a `VisualSignature` y a la presentacion (punto 6.2; ST-13), pero no añade area. Lo
     que eso deja fuera lo declara L-37.
   - **Construccion.** (1) Aplanar y transformar **primero** hasta WCS: anidados con su transformacion completa, la
     instancia y `P'`, con la escala uniforme `s` de la fuente (ST-11); (2) **despues**, construir cualquier margen que
     tenga dimension real de modelo (ancho geometrico, mascara, marco, limites de texto o de cota). Ningun margen se
     calcula en el marco del BTR para escalarlo despues.
   - **Nunca subestima la ocupacion de modelo.** Si no se puede garantizar que cubre la ocupacion (ancho geometrico,
     mascaras, marcos, contenido anidado, texto o cota bajo su contexto), o si no esta disponible o es infinita,
     `Status = UNKNOWN`.
   - **Contexto (`[V10-D02]`).** Las huellas de las instancias del plan se evaluan en el `DestinationEvaluationContext`
     (punto 4): representan lo que MUTATE materializara en el dibujo destino, nunca lo que resolveria la biblioteca.
   - **Quien la produce (P7b y PREPARE).** Para toda instancia que enumere P7a, tenga o no cambio de mano: las piezas de
     la familia `HeaderRun`, con su definicion evaluada en el contexto destino por el mismo clonado y aplanado que la
     sonda (sin la prueba de simetria cuando la pieza no cambia de mano); las curvas Cantilever, desde su plan y los
     registros de sus capas de rol; las anotaciones y cotas, desde los planes **definitivos** con el nombre destino, con
     la envolvente de texto y de cota que caracterice CT-49 bajo las observaciones que consumen (`DIMSTYLE`,
     `TEXTSTYLE`, escala de anotacion y registros de capa, §8.7). En Model Space (ST-19), la huella de un objeto
     existente sale del propio objeto con sus registros resueltos (§4.1).
   - **`Extents3d`.** Solo es admisible como primera sobre-aproximacion **declarada** de la ocupacion de modelo,
     ampliada solo por ocupaciones de modelo conocidas (mascaras, marcos, anchos geometricos) y nunca por `LineWeight`;
     una extension no disponible, infinita o dudosa ⇒ `UNKNOWN`.
   - **`XLINE` y `RAY` (`[V10-D07]`).** En el primer corte su huella es `UNKNOWN` salvo que CT-49 caracterice una huella
     o interseccion analitica fiable; un `XLINE` o `RAY` posterior a una fuente con huella `UNKNOWN` ⇒ E10 (L-34, L-37).
     Es un fallo conservador deliberado, no un defecto.
   - **Frescura (`[V10-D03]`).** Cada huella lleva su `FootprintInputFingerprint`: identidades de las definiciones
     reales, registros de simbolos resueltos usados, observaciones consumidas del contexto de materializacion, estado
     dinamico efectivo, identidades de las dependencias anidadas y cualquier otra entrada que altere la huella. Tras
     `EnsureForPlan` se relee el dibujo real; toda huella cuyo fingerprint cambie se vuelve a producir y se re-evaluan
     el orden de plan y ST-19 (PRE-18). No se usa `TraceFingerprint` para esto (punto 10).
   - **Consumo.** ST-19: superposicion con una huella `UNKNOWN` ⇒ E10. Orden de plan (punto 6.6): huella `UNKNOWN` en un
     par invertido ⇒ X-22. Las piezas con cambio de mano siguen necesitando `GeometrySymmetryResult`.
   - **Una sola autoridad** de extraccion de huellas y de sus margenes de modelo (G-M23). CT-36 y CT-49 caracterizan las
     ocupaciones reales por clase; T-M71 las prueba con dobles en Core.
6. **Politica visual-geometrica.**

   **6.1 CLOSED VARIANT POLICY: clase + variantes soportadas (`[V8-D01]`..`[V8-D05]`).** La allowlist ya no es solo por
   clase. **Una clase soportada solo lo es con los atributos de representacion enumerados en su fila.** Cualquier
   subclase, atributo visual o geometrico adicional, modo de representacion o variante descubierta que no este clasificado
   ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED` (X-17b). La clase se comprueba por **tipo exacto**, nunca por herencia: una subclase no
   hereda el soporte de su base (`MInsertBlock` deriva de `BlockReference`, como ya recoge ST-3). La geometria se lee con
   sus coordenadas completas en el marco del BTR evaluado (punto 13).

   | Clase (tipo exacto) | Variante soportada en el primer corte (se exigen todos los atributos a la vez) | Firma adicional (6.4) |
   |---|---|---|
   | `Line` | `Thickness = 0`; `Normal ≈ +Z` | `SourceKind = LINE` |
   | `Arc`, `Circle` | `Thickness = 0`; `Normal ≈ +Z` | `SourceKind = ARC` o `CIRCLE` |
   | `Ellipse`, `Spline` | plana en XY; `Normal ≈ +Z` (en la spline, la normal de su plano) | `SourceKind = ELLIPSE` o `SPLINE` |
   | `Polyline` (LWPOLYLINE) | ancho constante `0` y anchos inicial y final de cada vertice `0`; `Thickness = 0`; `Normal ≈ +Z`; bulges admitidos | `SourceKind = LWPOLYLINE`; `PolylineGeneration`: `Closed` siempre y `Plinegen` cuando `LinetypeSource` no tiene continuidad demostrable |
   | `Solid`, `Trace` | `Thickness = 0`; `Normal ≈ +Z`; poligono 1-2-4-3 (punto 8) | `SourceKind = SOLID` o `TRACE` |
   | `Hatch` SOLID | objeto de sombreado **no** degradado; patron predefinido `SOLID`; `Normal ≈ +Z`; `Elevation` solo la que caracterice CT-36 (hasta entonces `Elevation = 0`); bucles con sus aristas evaluadas **y su `HatchLoopTypes`**, y estilo de islas, en la forma canonica (punto 8) | `SourceKind = HATCH`; `FillSignature` |
   | `BlockReference` anidado | tipo **exacto** `BlockReference`; `AttributeCollection` vacia; definicion **no** anotativa, **no** XREF, **no** overlay y **no** dependiente de XREF; sin filtro espacial (XCLIP) ni otra entrada de `ACAD_FILTER`; `Normal ≈ +Z`; recursiva, con transformacion completa; un anidado dinamico se evalua en **su** estado real, con **su** BTR evaluado y **su** transformacion efectiva (`[V9-D08]`) | aporta a sus descendientes `VisibilityPath`, `LinetypePatternContext` y un paso de ruta estable (6.2) |

   | Variante o clase | Primer corte |
   |---|---|
   | `MInsertBlock` anidado | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Definicion anidada anotativa (`BlockTableRecord.Annotative`) u objeto anotativo | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Referencia anidada con filtro espacial (XCLIP) u otra entrada de `ACAD_FILTER` | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Referencia anidada con atributos (`AttributeCollection` no vacia) o con definicion XREF, overlay o dependiente | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Cualquier primitiva con **tipo de linea complejo** (con formas o texto) | `UNKNOWN` ⇒ `FAIL_CLOSED` en el primer corte, salvo evidencia expresa posterior (`[V8-D05]`) |
   | Clase soportada con grosor, normal, elevacion, ancho u otro atributo fuera de su fila | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | **Hatch de degradado** y **Hatch de patron** | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Hatch solido no canonizable de forma inequivoca | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Region, 3DSolid, proxy o entidad custom, raster, OLE | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Point, Polyline2d, Polyline3d, MLine, Face, Wipeout, Leader, MLeader, Table, Shape y cualquier otra clase que se descubra | `UNKNOWN` ⇒ `FAIL_CLOSED` hasta caracterizarse |
   | Text, MText, AttributeDefinition/Attribute, Dimension | `UNKNOWN` ⇒ `FAIL_CLOSED`, salvo futura exclusion o declaracion tipada con evidencia (ninguna en el primer corte) |
   | Ciclo de anidados | `UNKNOWN` ⇒ `FAIL_CLOSED` |
   | Correspondencia ambigua | `UNKNOWN` ⇒ `FAIL_CLOSED` |

   - `≈ +Z`: angulo con +Z dentro de `GeometryTolerance.Angle` (como ST-5); `= 0`: dentro de `GeometryTolerance.Length`.
   - **Completitud (G3, `[V9-D08]`).** CT-36 enumera, por clase soportada y con el metodo de inventario cerrado del
     punto 14, las propiedades y el estado que expone AutoCAD 2025 y clasifica cada uno como `TRANSPORTED` (viaja con la
     definicion y se compara en `VisualSignature` o en la variante de esta tabla), `PLACEMENT`, `COMMON_CONTEXT`,
     `NON_VISUAL` o `UNSUPPORTED`. Sin clasificar o `UNSUPPORTED` ⇒ la clase o la variante queda en `UNKNOWN`; si eso
     reduce materialmente la envolvente ⇒ Proposal V11.
   - Medido en la biblioteca inspeccionada (§1.4, `[V8-D23]`): ninguna variante excluida aparece en las piezas
     estructurales medidas; la politica no les quita alcance, y la revision de V8 confirma 0 ATTRIB en las 151
     definiciones (`[V9-D16]`).

   **6.2 SYMBOLIC VISUAL SOURCES tipadas (`[V7-D01]`..`[V7-D03]`, `[V7-D27]`, `[V8-D06]`..`[V8-D09]`, `[V8-D11]`).** La
   equivalencia **no** compara valores visuales resueltos: compara la **fuente simbolica tipada** de cada propiedad segun
   las reglas reales de AutoCAD.

   ```text
   NO:  ResolvedValueNow(izquierda) == ResolvedValueNow(derecha)
   SI:  VisualSource(izquierda, P)  == VisualSource(derecha, P)      para cada propiedad P

   LayerSource     := Named(capa)
                    | InheritLayer(ruta)          // capa 0 hasta la referencia de la pieza

   PropertySource  := Explicit(metodo, valor)     // tipado: metodo + valor, nunca solo el valor (V8, V9)
                    | ByLayer(Named(capa))
                    | InheritLayer(ruta)          // ByLayer resuelto contra una capa heredada
                    | InheritBlock(ruta)          // ByBlock hasta la referencia de la pieza
                    | Estado(P, estado)           // estado propio de la API de la propiedad P
   ```

   Resolucion conceptual, de la raiz hacia las hojas (nombres no contractuales):

   ```text
   raiz (referencia de la pieza, ruta canonica "·"):
       ctx.Layer   = InheritLayer(·)
       ctx.Prop[P] = InheritBlock(·)
   entidad e dentro de ctx:
       LayerSource(e) = e.Layer = "0" ? ctx.Layer : Named(e.Layer)
       Source(e, P)   = valor explicito               → Explicit(metodo, valor)
                        ByLayer                        → LayerSource(e) = Named(n) ? ByLayer(Named(n))
                                                                                   : InheritLayer(ruta de LayerSource(e))
                        ByBlock                        → ctx.Prop[P]
                        estado propio de la API de P   → Estado(P, estado)
   referencia anidada N dentro de ctx:
       ctx_N.Layer = LayerSource(N);  ctx_N.Prop[P] = Source(N, P);  ruta_N = ruta · paso(N)
   ```

   - Ejemplos normativos: `ByLayer(Named("Postes")) ≠ Explicit(ACI 3)` aunque `Postes` sea hoy de color 3;
     `InheritLayer(·) ≠ InheritBlock(·)` aunque hoy produzcan el mismo aspecto;
     `Explicit(ACI 7) ≠ Explicit(RGB 255,255,255)` aunque hoy se vean iguales sobre fondo oscuro.
   - **`Explicit` tipado (`[V8-D06]`).** `Explicit` conserva el **metodo** y el **valor**, nunca solo el valor resuelto:
     - color: `ACI(n)`, `RGB(r, g, b)` o `ColorBook(libro, nombre, valor almacenado)`; `ACI(7) ≠ RGB(255,255,255)`, y un
       color de libro solo es igual a otro con el mismo libro, el mismo nombre y el mismo valor de color almacenado en
       la entidad (`[V9-D09]`), nunca solo por su RGB ni solo por libro y nombre;
     - tipo de linea: por nombre de la tabla de tipos de linea, sin distinguir mayusculas como los simbolos de AutoCAD; un
       tipo complejo ⇒ `UNKNOWN` (6.1);
     - grosor: estado enumerado (`ByLayer`, `ByBlock`, `ByLineWeightDefault`) o valor enumerado de `LineWeight`, por
       identidad del enumerado;
     - transparencia: metodo (`ByLayer`, `ByBlock` o alfa) y alfa exacto;
     - estilo de trazado: semantica segun el `PSTYLEMODE` de la autoridad origen (fila `PlotStyleName`);
     - `LinetypeScale`: `double` por igualdad exacta del valor almacenado (V10 no introduce tolerancia; CT-36 registra
       las desviaciones reales).

     Un metodo o estado que el contrato no modela (por ejemplo, un color `ByPen`, `Foreground` o `None`) ⇒ `UNKNOWN`.
   - **Ruta canonica estable (`[V8-D11]`).** Cada paso identifica la referencia anidada por:
     - anidado **dinamico**: la identidad estable de su `DynamicBlockTableRecord` (el nombre de la definicion base, no
       anonimo), sus **valores dinamicos efectivos** (punto 5) y un **ordinal canonico por contenido** dentro de su
       contenedor;
     - anidado **estatico**: la identidad estable de su definicion (nombre no anonimo) y el ordinal canonico por contenido.

     **Nunca** `ObjectId`, handle ni el nombre anonimo generado (`*U…`) de una representacion evaluada. Una definicion
     anidada anonima sin base dinamica con nombre, o un empate de ordinal que no se pueda resolver ⇒ `UNKNOWN`. Dos fuentes
     heredadas son equivalentes solo si coinciden el tipo de herencia, la ruta simbolica y el contrato de la propiedad. En
     el primer corte toda cadena heredada que no termina en `Explicit`, `ByLayer(Named)` o `Estado` termina en la
     referencia de la pieza (ruta `·`); una cadena que terminara en otro contexto ⇒ `UNKNOWN`. `TraceFingerprint` usa esta
     ruta (punto 10).
   - **Contrato por propiedad** (estados de la API; CT-36 confirma los exactos y un estado encontrado y no modelado ⇒
     `UNKNOWN`, X-22):

     | Propiedad | Estados que admite la API | Fuente simbolica tipada |
     |---|---|---|
     | `Color` | `ByLayer`, `ByBlock`, color indexado, color verdadero, libro de colores | `Explicit(ACI n)`, `Explicit(RGB r,g,b)`, `Explicit(ColorBook libro, nombre, valor almacenado)`, `ByLayer(Named)`, `InheritLayer`, `InheritBlock`; otro metodo ⇒ `UNKNOWN` |
     | `Linetype` | `ByLayer`, `ByBlock`, tipo con nombre | `Explicit(nombre)` de un tipo simple, `ByLayer(Named)`, `InheritLayer`, `InheritBlock`; tipo complejo ⇒ `UNKNOWN` |
     | `LineWeight` | `ByLayer`, `ByBlock`, `ByLineWeightDefault`, valor | `Explicit(LineWeight, valor enumerado)`, `ByLayer(Named)`, `InheritLayer`, `InheritBlock`, `Estado(LineWeight, Default)` |
     | `Transparency` | `ByLayer`, `ByBlock`, alfa | `Explicit(alfa)`, `ByLayer(Named)`, `InheritLayer`, `InheritBlock` |
     | `PlotStyleName` | `ByLayer`, `ByBlock`, `Normal`, estilo con nombre (solo con estilos de trazado con nombre) | segun el `PSTYLEMODE` de la **autoridad origen** de la definicion (`[V8-D07]`, punto 4): con estilos con nombre, `Explicit(nombre)`, `ByLayer(Named)`, `InheritLayer`, `InheritBlock`, `Estado(PlotStyle, Normal)`; con estilos dependientes del color (`PSTYLEMODE = 1`) se deriva de `ColorSource` y no añade fuente independiente; modo desconocido o no reproducido en la base auxiliar ⇒ `UNKNOWN` (`[V7-D27]`) |
     | `LinetypeScale` | valor por entidad; la propiedad no se hereda | `Explicit(double)` siempre; sin continuidad demostrable, ademas `LinetypePatternContext` (6.4, `[V8-D08]`) |
   - **Escalas globales de tipo de linea (`[V8-D09]`).** `LTSCALE`, `PSLTSCALE` y `MSLTSCALE` son contexto global del
     dibujo o de la ventana, comun a las dos mitades: no entran en la firma y CT-36 lo caracteriza. `CELTSCALE` solo
     fija la escala de las entidades que se crean despues; no altera retroactivamente las entidades de la pieza
     evaluada, y la referencia de pieza que crea el materializador es la raiz comun `·`; es una observacion candidata
     del contexto de materializacion y entra en el read-set solo si el productor la consume (§8.7, `[V9-D05]`,
     `[V10-D05]`); CT-36 y CT-50 lo caracterizan.

   **6.3 VisibilityPath (`[V7-D04]`, `[V7-D05]`, `[V8-D03]`).** La visibilidad se separa de la firma de color:

   ```text
   VisibilityPath {
       ancestors[]:                       // de la primera referencia anidada al contenedor de la primitiva, en orden
           (Visible de la referencia, LayerSource de la referencia)
       primitive:
           (Visible de la primitiva, LayerSource de la primitiva)
   }
   ```

   - La referencia de la pieza es comun a toda la pieza y no forma parte de la ruta.
   - Dos homologas son equivalentes en visibilidad solo si sus `VisibilityPath` son **iguales** (misma longitud, mismo
     orden, mismos `Visible` y mismas `LayerSource`). La prueba **no** se basa en el estado actual: con las mismas fuentes,
     todo estado de capas (encendida o apagada, inutilizada o reutilizada, inutilizada en una ventana, inutilizada por defecto
     en ventanas nuevas, no trazable) y todo `Visible` afectan igual a las dos mitades, sea cual sea la regla exacta de AutoCAD
     para las referencias anidadas.
   - Cubre referencias anidadas, capa `0`, encendido/apagado, inutilizar/reutilizar, inutilizacion en ventana donde aplique
     y `Visible` de los ancestros.
   - CT-36 varia estados de capas y de ancestros (apagado, inutilizado, inutilizado en ventana, `Visible`) para demostrar la
     regla sobre piezas reales y caracteriza la semantica de AutoCAD. Una relajacion de la igualdad ordenada (por ejemplo,
     comparar los ancestros como conjunto) solo con evidencia de G3.
   - Un mecanismo de visibilidad no modelado ⇒ `UNKNOWN` / `FAIL_CLOSED` (X-22); entre ellos, filtros espaciales (XCLIP) en
     referencias anidadas, objetos o definiciones anotativos (visibilidad por escalas soportadas y escala de anotacion,
     `[V8-D03]`), estados de visibilidad de bloques dinamicos que no se reflejen en `Visible` de la representacion evaluada
     y cualquier otro que CT-36 descubra. Los filtros espaciales y los anotativos anidados son ademas variantes no
     soportadas (6.1).

   **6.4 VisualSignature (`[V7-D06]`, `[V8-D02]`, `[V8-D08]`).** Cada primitiva canonica lleva, ademas de su geometria:

   ```text
   VisualSignature {
       Geometry                // en el marco del BTR evaluado de la pieza (punto 13); con orientacion cuando importa (punto 8)
       SourceKind              // clase exacta de origen (6.1): LINE, ARC, CIRCLE, ELLIPSE, SPLINE, LWPOLYLINE, SOLID, TRACE o HATCH
       PolylineGeneration      // solo LWPOLYLINE: Closed siempre; Plinegen si LinetypeSource no tiene continuidad demostrable
       ColorSource
       LinetypeSource
       LinetypeScale           // siempre
       LinetypePatternContext  // sin continuidad demostrable: (|sx|, |sy|) y LinetypeScale de cada ancestro de la ruta
       LineWeightSource
       TransparencySource
       PlotStyleSource
       VisibilityPath
       FillSignature           // ninguno | relleno solido uniforme; para HATCH, estilo de islas y HatchLoopTypes
   }
   ```

   Todas las fuentes son **tipadas** (6.2). CT-36 añade toda propiedad de clase que descubra visualmente material.
   `IsSymmetric` exige una correspondencia uno a uno con **VisualSignature igual** en cada pareja —misma `SourceKind`
   incluida: una polilinea **nunca** se empareja con `Line` o `Arc` sueltos solo porque su geometria sea equivalente— y la
   conservacion del orden visual (6.5 y 6.6).

   **Contexto de patron (`[V8-D08]`).** La escala efectiva y la fase del patron de un tipo de linea sin continuidad
   demostrable pueden depender de la transformacion y de la `LinetypeScale` de los ancestros. Por eso su firma incluye,
   para cada ancestro relevante de la ruta (de la primera referencia anidada al contenedor de la primitiva), los
   factores de escala absolutos `(|sx|, |sy|)` y su `LinetypeScale`. Un ancestro con escala no uniforme (`|sx| ≠ |sy|`)
   sobre un trazo sin continuidad demostrable, o cualquier factor de ancestro que altere la fase o la escala del patron
   de un modo no modelado ⇒ `UNKNOWN`. El signo de las escalas ya lo recoge la orientacion (punto 8). CT-36 caracteriza
   el efecto real; la referencia de la pieza es comun a las dos mitades y no forma parte del contexto.

   **6.5 Orden visual y oclusion (`[V7-D09]`).** Solo se construye una relacion de orden para los pares cuya superposicion es
   **visualmente material**:
   - relleno ↔ relleno (area comun por encima de la tolerancia);
   - relleno ↔ trazo (longitud del trazo dentro del interior del relleno por encima de la tolerancia);
   - trazos coincidentes con `VisualSignature` distinta (longitud coincidente por encima de la tolerancia);
   - entidades cuya `TransparencySource` no es `Explicit` opaca, superpuestas a cualquier primitiva;
   - cualquier otro caso que CT-36 descubra sensible al orden.

   El orden visual es el orden efectivo de dibujo dentro de la pieza evaluada: el de las entidades de cada BTR con su
   `SortentsTable`, con cada referencia anidada dibujada en su posicion. Para cada par material `(x, y)` y sus homologas
   `(x', y')`:

   ```text
   RelativeVisualOrder_original(x, y) == RelativeVisualOrder_reflejado(x', y')
   ```

   Si no puede demostrarse ⇒ X-22 / `FAIL_CLOSED`. V10 no define una teoria global de DRAWORDER para objetos que no se
   solapan; entre instancias de pieza distintas rige 6.6.

   **6.6 Orden visual de plan entre instancias de pieza (`[V8-D12]`, `[V9-D03]`).** 6.5 controla el orden **dentro** de
   una pieza; 6.6 controla el orden **entre** instancias de pieza distintas de una misma vista (y entre las curvas de la
   familia Cantilever), sobre las huellas de `VisualFootprintResult` (punto 5):

   - Para cada par de instancias `(a, b)`, **con o sin cambio de mano**, cuyo orden relativo **cambia** entre
     `F ∘ Π_(V,σ)(Eff(D, R))` (B de §11.1) y `Π_(V,σ')(Eff(μ_k D, R))` (A de §11.1), se obtienen las huellas de las dos
     instancias. Si no se superponen de forma visualmente material, el orden es irrelevante. Si se superponen —las
     clases de 6.5 evaluadas sobre las huellas de ocupacion de modelo: relleno ↔ relleno, relleno ↔ trazo, trazos
     coincidentes con `VisualSignature` distinta, entidades con `TransparencySource` no `Explicit` opaca y lo que
     descubra CT-36—, el orden relativo tiene que conservarse; como cambia, la vista no es equivalente (X-22).
   - Un par cuyo orden relativo **no** cambia no necesita huella: su relacion se conserva por construccion.
   - Huella `UNKNOWN`, superposicion no clasificable, correspondencia de homologas ambigua para una instancia que participa
     en un par invertido u orden relativo no demostrable ⇒ X-22 / `FAIL_CLOSED`.

     ```text
     para todo par (a, b) con huellas superpuestas de forma material y sus homologas (a', b'):
       RelativePlanVisualOrder(F ∘ Π(D))(a, b) == RelativePlanVisualOrder(Π(μ_k D))(a', b')
     ```

   - Tras `EnsureForPlan`, los pares se re-evaluan con las huellas refrescadas (PRE-18, `[V10-D03]`): un par que deja de
     demostrarse falla en PREPARE (E10, con X-22). Las superposiciones que solo existen por anchuras de linea, de papel
     o de pantalla no forman pares materiales (L-37).
   - **Autoridad del orden.** El orden de plan es el de emision y materializacion: en la familia `HeaderRun`, las
     colocaciones de cada grupo de cabecera en el orden de los grupos y despues las piezas sueltas en su orden, con el
     contenido de cada grupo en el orden de sus instancias (`LateralHeaderDrawer.cs:36-76`; el mismo orden en
     `RedefineSystemBlock`, `:130-161`); en la familia Cantilever, el orden de emision de sus curvas; y, si existen, las
     relaciones del `SortentsTable` aplicable de la definicion del sistema. «Fuente» es el plan regenerado del documento
     fuente (§11.1), no las entidades actuales del dibujo.
   - V-VIEW-ALL deja de tratar esos pares como un multiconjunto puro (§6.6, §11.1). V10 **no** crea una teoria global de
     DRAWORDER fuera de los pares materiales.
   - Hecho del codigo, indicativo (CT-48 lo fija por kind y vista): los builders emiten por indice o por coordenada, asi
     que bajo `μ_k` se invierten los pares de una misma pasada —postes, largueros de claros distintos y postes
     intermedios del frontal Selectivo (`SelectiveFrontalBuilder.cs:82-152`); grupos de la planta Selectiva ordenados
     por `InsertionY` (`SelectivePlantaBuilder.cs:221-233`); postes y largueros del frontal Dinamico
     (`DynamicSystemFrontalBuilder.cs:71-165`)— y se conserva el orden entre pasadas: el frontal Selectivo emite los
     postes, con sus placas y la seguridad de cada poste, antes que los largueros de los claros, de modo que la relacion
     poste-larguero no depende del sentido de los frentes.
7. **Eje y centro (`[V5-D29]`).** El eje a probar se deriva de `F` y de la rotacion de la instancia: una rotacion multiplo
   de `π/2` (dentro de `GeometryTolerance.Angle`) da el eje local X o Y del bloque; una rotacion oblicua ⇒ `UNKNOWN`. El
   centro se obtiene como **candidato** geometrico (el punto medio de las extensiones de la geometria aplanada a lo largo del
   eje) en el marco del punto 13. **La caja envolvente nunca es prueba**: solo propone el unico centro posible.
8. **Canonicalizacion conservadora y verificacion (`[V7-D10]`..`[V7-D12]`, `[V8-D02]`, `[V8-D04]`).** Antes de comparar,
   las primitivas se canonicalizan **solo dentro de la misma VisualSignature simbolica** (misma `SourceKind` incluida),
   para evitar falsos negativos por segmentacion sin crear coincidencias falsas:
   - **continuidad demostrable**: la tiene una primitiva cuya `LinetypeSource` es `Explicit` de un tipo de linea sin trazos
     (por ejemplo `Continuous`). `ByLayer`, `InheritLayer` e `InheritBlock` pueden volverse discontinuos con un cambio futuro
     de la capa o de la referencia: en el primer corte se tratan como **no continuos** (`[V7-D11]`);
   - **rectas con continuidad demostrable**: se fusionan en segmentos maximales solo si son colineales, contiguas dentro de
     `GeometryTolerance.Length`, sin hueco, sin bifurcacion, sin cruce material, con la misma firma (misma `SourceKind`) y
     con multiplicidad no material (`TransparencySource` `Explicit` opaca); si no, no se fusionan;
   - **sin continuidad demostrable**: **no** se fusionan; se conservan segmentacion, **orientacion** (inicio → fin en el
     marco del BTR evaluado, con el sentido que dejan las transformaciones anidadas), `LinetypeScale`,
     `LinetypePatternContext` y multiplicidad, y la reflexion de cada primitiva debe coincidir con su homologa **con
     orientacion** (en circunferencias, punto de fase y sentido; en polilineas, orden de vertices);
   - **transparencia**: entidades coincidentes con `TransparencySource` no `Explicit` opaca conservan su multiplicidad;
   - **arcos**: con continuidad demostrable, el sentido se normaliza solo si no altera la representacion; se fusionan solo
     con el mismo soporte (centro y radio), contiguidad y resultado inequivoco;
   - **polilineas**: se comparan por sus segmentos y bulges evaluados, con `SourceKind = LWPOLYLINE`, `Closed` y, sin
     continuidad demostrable, `Plinegen` (6.4); **nunca** se emparejan ni se fusionan con `Line` o `Arc` sueltos aunque su
     geometria sea equivalente; ancho distinto de 0, grosor distinto de 0 o normal no equivalente a +Z ⇒ `UNKNOWN` (6.1);
   - **splines**: por la geometria evaluada o su forma canonica, no por la identidad de la entidad; sin continuidad
     demostrable se conserva el sentido de parametrizacion;
   - **SOLID y TRACE**: con `Thickness = 0` y `Normal ≈ +Z` (6.1), se canoniza el poligono que AutoCAD representa, con el
     orden real de vertices 1-2-4-3 donde corresponda (triangulo si los dos ultimos coinciden); dos solidos con el mismo
     conjunto de vertices y distinto orden no son equivalentes; se conservan firma, multiplicidad y orden visual;
   - **HATCH solido uniforme**: con `Normal ≈ +Z` y la `Elevation` admitida (6.1), bucles con sus aristas evaluadas y su
     `HatchLoopTypes`, estilo de islas (`Normal`, `Outer`, `Ignore`) y contornos canonizados de forma inequivoca, con firma
     y orden visual; si no ⇒ `UNKNOWN`;
   - **nunca** se fusionan geometrias de familias distintas (por ejemplo, un arco con rectas) ni primitivas de `SourceKind`
     distinta para hacerlas coincidir.

   Solo G3 puede habilitar una canonicalizacion adicional, con evidencia (CT-36, CT-47). `IsSymmetric` es verdadero si y
   solo si existe una correspondencia uno a uno inequivoca entre el multiconjunto de primitivas canonicas y su reflexion
   respecto de `(eje, centro)`, con **VisualSignature igual** en cada pareja y orden visual conservado (6.5), dentro de
   `GeometryTolerance.Length` (y `GeometryTolerance.Angle` donde aplique). V10 no introduce una tolerancia nueva: CT-36
   mide las desviaciones reales en doble precision (`[V6-D30]`) y la tasa de falsos positivos y negativos de la
   canonicalizacion sobre el corpus de piezas conocidas (CT-47). Una ambiguedad de canonicalizacion ⇒ `UNKNOWN`; si es
   material ⇒ Proposal V11.
9. **Equivalencia.** Con `IsSymmetric` y eje X, la instancia `(q, r, mX, mY, parametros)` equivale a

   ```text
   (q + R(r)·(2·c·mX, 0), r, −mX, mY, parametros)          eje X
   (q + R(r)·(0, 2·c·mY), r, mX, −mY, parametros)          eje Y
   ```

   que sale de `T(q)·R(r)·S(mX,mY)·X_c = T(q + R(r)·(2c·mX, 0))·R(r)·S(−mX, mY)`, con `X_c = T(2c,0)·S(−1,1)` y `c` el centro
   **verificado de ese estado**. Casos normativos para T-M40:

   ```text
   Caso C. Pieza de rol distinto de Tope con LONGITUD en X; geometria en u ∈ [0, L] (centro verificado L/2);
     el builder inserta en el ancla izquierda a con L = S + δ.  B = (F(a), mX = −1);  A = (F(b), mX = +1).
     A ≡ B ⇔ c = S/2 = L/2 − δ/2  ≠ L/2  ⇒  A ≢ B  ⇒  UNKNOWN.
   Caso D. Pieza de ancho fijo w anclada en poste + t, en claros de ancho S.
     A ≡ B ⇔ c = (S − 2t)/2;  centro verificado w/2 ⇒ con S − 2t ≠ w no hay equivalencia ⇒ UNKNOWN.
   Caso E. Pieza asimetrica cuya caja envolvente es simetrica: la verificacion del punto 8 falla ⇒ UNKNOWN.
   Caso F. Geometria perfectamente simetrica con fuentes visuales, clase de origen, contexto o rutas distintas entre
     homologas, en cualquiera de estos doce casos: (1) mismo color actual, ByLayer frente a Explicit; (2) InheritLayer
     frente a InheritBlock; (3) VisibilityPath distinta; (4) LineWeight distinto; (5) Transparency distinta;
     (6) LinetypeScale distinta; (7) PlotStyle distinto si el entorno lo permite caracterizar; (8) LinetypeSource ByLayer
     frente a Explicit con el mismo tipo actual; (9) FillSignature distinta; (10) SourceKind distinta; (11) Plinegen o
     Closed distintos; (12) contexto de escala de ancestros o de patron distinto ⇒ IsSymmetric = false ⇒ UNKNOWN.
   ```

10. **`TraceFingerprint`.** No prueba simetria. Solo sirve para **trazabilidad**, **deteccion de obsolescencia** y
    **enlace PREFLIGHT ↔ PREPARE**: si en PREPARE difiere del de PREFLIGHT, la equivalencia visual-geometrica se vuelve
    a evaluar; si ya no se cumple ⇒ E10. Contenido: estado dinamico con **valores efectivos**, clases y variantes
    encontradas, geometria aplanada (con orientacion cuando importa), variantes, **fuentes visuales simbolicas tipadas**
    de cada primitiva con sus **rutas canonicas estables** (6.2, `[V8-D11]`), `SourceKind`, `PolylineGeneration`,
    **`LinetypeScale`**, `LinetypePatternContext`, **`VisibilityPath`**, `FillSignature`, relaciones de orden visual
    materiales, definiciones anidadas por identidad estable, BTR evaluados, **transformacion efectiva** y contexto de la
    base auxiliar (punto 4); en **serializacion canonica** (ordenada por contenido, nunca por el orden de iteracion del
    BTR, que DRAWORDER, SORTENTS o las ediciones alteran; el orden visual se registra como relacion, no como orden de
    iteracion). Los bits exactos sirven para la trazabilidad; la equivalencia usa tolerancias. `TraceFingerprint` **no**
    es `FootprintInputFingerprint` (§9.2): las entradas de la huella visual se fijan aparte y se re-verifican para toda
    instancia cuya huella se uso (PRE-18, `[V10-D03]`, `[V10-D19]`).
11. **Sin forma afin en el primer corte.** `c = a0 + a1·Parametro` **no** forma parte del camino normativo: el centro se
    verifica en cada estado usado. Solo se registra como optimizacion futura si existe metadata autoritativa que derive la
    relacion. I-52 **no** interpreta grafos de acciones de bloques dinamicos (G-M17).
12. **Estados futuros por variables.** La verificacion de un estado concreto **no** basta si el mismo rack puede cambiar ese
    estado con `ChangeValue`:

    ```text
    Si cualquier parametro dinamico necesario para GeometricEvidence depende directa o transitivamente de una
    propiedad vinculable, la evidencia de un unico estado NO satisface PV-7. Para ALLOW se exige una de:
      1. prueba visual-geometrica valida sobre TODO el dominio autoritativo del parametro;
      2. autoridad integrada que garantice una propiedad visual-geometrica universal;
      3. parametro demostrado independiente del vinculo.
    Si ninguna existe: UNKNOWN → FAIL_CLOSED.  No se infiere universalidad con muestras finitas.
    ```

    En `main` no hay dominio autoritativo (§6.5.5) ni autoridad de simetria para un continuo de `LONGITUD`: solo cabe la
    via 3, que V-DEP concreta en DEP-09 (largueros: `¬Dep(i)`) y DEP-10a..DEP-10c (postes, por camino). I-49 G5
    (`977f873`, antes `c4880af`) y G6 (`5f969cc`: enlace y evaluacion de un arbol, sin dominio ni `PlanReadSet`) no
    cambian esto (`[V9-D12]`, `[V9-D21]`, `[V10-D14]`); si I-49 integra su dominio y su `PlanReadSet`, se consumen sin
    analisis propio.
13. **Marco del centro, origen evaluado y transformacion efectiva (`[V7-D16]`).** El drawer crea cada pieza como
    `BlockReference(Position = q, Rotation = r, ScaleFactors = (mX, mY, 1))` (`LateralHeaderDrawer.cs:308-314`), cuya
    transformacion es `T(q)·R(r)·S(mX,mY)·T(−o_p)`. Para cada instancia, `o_p` es el `Origin` del **BTR evaluado** que
    contiene la representacion de ese estado (el `BlockTableRecord` de la referencia tras aplicar los parametros), **no**
    automaticamente `DynamicBlockTableRecord.Origin`: un parametro de punto base puede moverlo. **Despues del punto de
    observacion** (punto 5) la sonda lee de la referencia temporal `Position`, `Rotation`, `ScaleFactors`, `Normal` y
    `BlockTransform`, y el `Origin` del BTR evaluado, y los compara con la transformacion que esperaba caracterizar. Si una
    accion dinamica altera la referencia de un modo no representado por el contrato ⇒ `UNKNOWN` ⇒ `FAIL_CLOSED` (X-23). En
    anidados rige la misma regla en cada nivel relevante, con su propio BTR evaluado. El centro se mide en
    `u = x_BTR − o_p`, **no** en WCS, en el marco del rack ni en coordenadas crudas de la definicion base.
14. **Contrato de caracterizacion en AutoCAD 2025 (CT-36).** G3 **no** añade la sonda productiva a `src/`: demuestra su
    viabilidad con `tools/`, un harness externo, evidencia del Owner o artefactos en `docs/automation/evidence/`, **en el
    runtime objetivo `acad.exe` / AutoCAD 2025**. `accoreconsole` puede usarse solo como comparacion y ningun comportamiento
    suyo, ni ninguno conocido solo por foros, se congela como hecho normativo. CT-36 mide **antes y despues** de cada sonda:

    ```text
    DBMOD del dibujo del usuario
    HANDSEED del dibujo del usuario
    estado y grupo de UNDO del documento
    HostApplicationServices.WorkingDatabase
    objetos persistidos del dibujo del usuario (definiciones, referencias, capas, estilos, diccionarios)
    cache de BlockLibraryImporter, cuando existe antes y despues y solo desde el harness fuera de src/: HANDSEED,
      inventario de BTR y otra huella estable del contenido

    Postcondicion:
      DWG del usuario semantica y estructuralmente intacto
      WorkingDatabase restaurada (incluso ante excepcion)
      sin artefactos temporales en el dibujo del usuario
      contenido de la cache de biblioteca intacto (su inicializacion perezosa si puede ocurrir)
    ```

    La base auxiliar privada si puede mutar y se descarta. Si la sonda cambia `HostApplicationServices.WorkingDatabase`, lo
    restaura en un bloque que se ejecuta tambien ante excepcion.

    **Acceso a la cache de biblioteca (`[V7-D15]`, `[V8-D10]`).** La `Database` cacheada por `BlockLibraryImporter`
    (`BlockLibraryImporter.cs:127-168`, privada) es **READ / CLONE ONLY** para la sonda y **no** se accede a ella por
    reflexion. Ruta permitida:

    ```text
    base auxiliar privada con el contexto del punto 4 (autoridad origen para la evidencia de simetria;
      DestinationEvaluationContext para la huella visual)
    → PRIMERO ruta 2, definicion y nombres ya presentes en el dibujo: clonado de solo lectura del dibujo hacia auxDb
        por la costura de AutoCAD autorizada y caracterizada por CT-36, sin pasar por la cache (bloques raiz,
        anidados, capas, tipos de linea, estilos y demas dependencias de representacion relevantes)
      DESPUES ruta 1, solo lo que falta: BlockLibraryImporter.EnsureBlocks(auxDb, requiredNames) (:37-119; API
        publica, o la API publica equivalente vigente), con DuplicateRecordCloning.Ignore; clona desde la cache
        privada hacia la base auxiliar, sin modificar BlockLibraryImporter.cs (§20.3); su retorno es solo
        diagnostico (IF-8)
      DRAWING FIRST, LIBRARY SECOND WITH IGNORE; nunca library-first; colision no simulable como en MUTATE ⇒ UNKNOWN
    → verificar que TODOS los BTR requeridos existen en auxDb tras el intento de clonado
        falta alguno ⇒ UNKNOWN ⇒ FAIL_CLOSED (X-17a); un retorno 0 no es por si mismo un fallo (IF-3, IF-4, IF-6, IF-8)
    → evaluar en auxDb → descartar auxDb
    ```

    Queda prohibido: reflexion sobre `BlockLibraryImporter` o acceso a su campo cacheado; insertar referencias
    temporales o aplicar parametros dinamicos en la `Database` cacheada; abrir alli objetos `ForWrite`; y usarla como
    base auxiliar. Queda permitido: `EnsureBlocks` hacia la base auxiliar y el clonado de solo lectura desde el dibujo
    hacia la base auxiliar, con la precedencia DRAWING FIRST (`[V9-D07]`, `[V10-D09]`). G-M20 lo hace ejecutable y sigue
    prohibiendo solo el acceso o la reflexion directos sobre `cachedLibrary` y su mutacion. CT-36 y T-M63 pueden
    observar el contenido de la cache (HANDSEED, inventario de BTR y huella) **solo desde un harness fuera de `src/`**,
    si hace falta para demostrar que no cambio.

    **Metodo de inventario cerrado (`[V9-D08]`).** CT-36 (contenido de las piezas evaluadas) y CT-49 (referencia fuente y
    objetos de Model Space) declaran como descubren propiedades y estado, con un harness fuera de `src/`:

    ```text
    por tipo exacto en ejecucion:
      propiedades publicas relevantes (reflexion en el harness)
      codigos DXF y estado serializable (filer o mecanismo soportado)
      diccionario de extension · XData · filtros espaciales · atributos · estado anotativo
      material · sombras · estilo visual · cualquier otra presentacion descubrible

    clasificacion de cada propiedad, entrada o estado encontrado (exactamente una):
      TRANSPORTED     en la referencia fuente, ST-13; en el contenido evaluado, viaja con la definicion y se compara en
                      VisualSignature o en la variante de 6.1
      PLACEMENT       transformacion o geometria (P', BTR evaluado, marco)
      COMMON_CONTEXT  contexto del dibujo, de la ventana o de materializacion comun a las dos mitades (§8.7)
      NON_VISUAL      sin efecto en la representacion
      UNSUPPORTED     fuera del primer corte

    sin clasificar o UNSUPPORTED ⇒ UNKNOWN (contenido evaluado, X-17b) o E4 (referencia fuente, ST-18)
    ```

    Nada queda sin clasificar: material, sombras, estilos visuales y cualquier otra propiedad descubierta tambien quedan
    clasificados (`[V10-D10]`). La reflexion del harness no contradice G-M20, que rige los archivos productivos del
    espejo.

    **Identidad de la evidencia (`[V7-D17]`).** CT-36 y CT-47 registran el **SHA-256** del contenido binario de la biblioteca;
    ruta, tamaño y fecha son diagnostico. Antes de reutilizar evidencia de §1.4 o un corpus registrado se recalcula el
    SHA-256; si cambia, esa evidencia no se reutiliza. Para definiciones ya presentes en el dibujo manda el
    `TraceFingerprint` de la definicion efectiva, no el SHA externo.

    CT-36 cubre ademas (`[V9-D14]`, `[V10-D17]`): politica de clases y variantes con tipo exacto (grosor, normales,
    elevacion, `MInsertBlock`, definiciones anidadas anotativas, XCLIP y filtros espaciales, atributos y definiciones
    XREF, overlay o dependientes anidados, tipos de linea complejos, `HatchLoopTypes`, y completitud de las propiedades
    y el estado por clase con el metodo de inventario cerrado); `SourceKind`, `Plinegen` y `Closed`; fuentes visuales
    simbolicas tipadas por propiedad (capa `0`, `ByLayer`, `ByBlock`, anidados, estados de la API, `Color` con
    `ColorBook` y valor almacenado, `LineWeight` y `Transparency` tipados, y `PlotStyle` segun el modo de la autoridad
    origen); contexto de la base auxiliar con un dibujo STB y otro CTB, `INSUNITS` y escala de anotacion; read-set del
    contexto de materializacion (con CT-50); `VisibilityPath` con estados de capas y ancestros variados;
    `LinetypeScale`, escalas y `LinetypeScale` de ancestros, fase y segmentacion de tipos de linea, y las escalas
    globales `LTSCALE`, `PSLTSCALE`, `MSLTSCALE` y `CELTSCALE`; multiplicidad; orden visual dentro de la pieza y, con
    CT-48, entre piezas; huellas visuales de ocupacion de modelo (`VisualFootprintResult`) de toda instancia enumerada,
    construidas tras transformar a WCS con la escala `s ≠ 1` de la fuente y escalas anidadas, con ancho de modelo frente
    a `LineWeight`, anchos solo de pantalla o de trazado excluidos, mascaras, limites de texto y de cota con escala de
    anotacion y STB/CTB; `DestinationEvaluationContext` con DRAWING FIRST y colisiones de nombre de capa y de bloque
    anidado; frescura tras `EnsureForPlan` con una pieza sin cambio de mano cuya huella de biblioteca difiere del dibujo
    real; poligono SOLID/TRACE; bucles y estilo de islas de HATCH; degradados y patrones; evaluacion de estados nuevos;
    anidados con ruta estable; `applied`; punto de observacion; BTR evaluado, `Origin` y transformacion efectiva;
    valores efectivos y paridad con el materializador (`[V6-D14]`); clonado por `EnsureBlocks` y desde el dibujo con
    verificacion de todos los BTR requeridos; canonicalizacion (falsos positivos y negativos); definicion local frente a
    biblioteca; obsolescencia y rendimiento preliminar. Si G3 demuestra que la ruta no es viable, que las clases o
    variantes reales no estan soportadas, que el contexto no se puede reproducir, que el contexto de evaluacion destino
    no puede simularse, que las huellas no pueden garantizarse conservadoras, que la biblioteca real reduce
    materialmente la envolvente mas alla de O-1 o que la canonicalizacion es materialmente ambigua ⇒
    **STOP → Proposal V11**. La sonda productiva nace en el Plugin en el gate en que la necesite la autoridad de planes
    o de materializacion (previsiblemente G6/G7); Application solo consume `GeometrySymmetryResult` y
    `VisualFootprintResult` (con dobles en las pruebas Core). Riesgo de rendimiento: R-24, medido en G7.
15. **Defensas adicionales.** El rechazo de piezas de rol `Tope` sigue como defensa adicional (hoy su fila es `UNKNOWN`),
    no como mecanismo principal. Ningun componente acepta una diferencia de planes como evidencia de centro (G-M15).
16. **Sin `GeometrySymmetryResult` verificado** (sonda no disponible, sin paridad, sin punto de observacion soportado o
    sin contexto reproducible de la base auxiliar, BTR requeridos ausentes tras el intento de clonado, contexto de
    evaluacion destino no reproducible, productor asimetrico del contexto de materializacion o consumo no determinable,
    ruta anidada o correspondencia ambigua, eje oblicuo o simetria no verificada ⇒ X-17a; clase o variante fuera de la
    politica cerrada o ciclo ⇒ X-17b): la vista no es equivalente y el rack falla cerrado (E6). Con fuentes visuales
    simbolicas tipadas, `SourceKind`, `PolylineGeneration`, `LinetypePatternContext` o `VisibilityPath` distintas,
    mecanismo de visibilidad no modelado, orden visual no demostrado dentro de la pieza, orden de plan no conservado
    entre piezas o huella visual `UNKNOWN` en un par invertido ⇒ X-22; si eso aparece al refrescar las huellas tras
    `EnsureForPlan`, E10 (PRE-18). Con transformacion efectiva alterada ⇒ X-23. Con estado dependiente de un vinculo sin
    evidencia universal ⇒ X-17c (S-26, S-27a..S-27c).
17. **Confirmacion del Owner.** El Owner confirma en G3 y G9 (O-3, M-15, M-27, M-32, M-36) sobre casos reales; su
    confirmacion **no** sustituye a la evidencia.

### 11.4 Parametros dinamicos

- El censo de claves y bloques **reutiliza las autoridades de I-19**: `CatalogBlockParameters`
  (`CatalogBlockParameters.cs:28`) y `CatalogBlockManifest` (`CatalogBlockManifest.cs`), cuya guarda builder → manifiesto
  ya impide que diverjan. **No** se crea un censo paralelo (CT-25).
- Hoy las claves son magnitudes: `LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE` y `FONDO`
  (`SelectiveRackDefaults.cs:46-95`), mas los nombres configurables de cabecera, que por defecto son esas magnitudes
  (`LateralHeaderParameters.cs:66-72`).
- Una magnitud se compara numericamente con `GeometryTolerance.Length`. La **direccion** en que estira dentro del
  bloque es contenido del bloque y la cubre la evidencia visual-geometrica evaluada por estado (§11.3).
- Un parametro con semantica de lado o mano (visibilidad, volteo, enumerado de lado) exige una regla especifica
  declarada; sin ella ⇒ **FAIL_CLOSED** (X-14). Como los nombres de cabecera son configurables, el censo se evalua **por
  valor en PREFLIGHT**.
- **Paridad de aplicacion** (§11.3 punto 5): la sonda aplica los parametros con la semantica de
  `LateralHeaderDrawer.ApplyDynamicParameters` y compara con los **valores efectivos** leidos despues del punto de
  observacion; un parametro solicitado que el bloque no expone o que es `ReadOnly` se comporta igual que en el
  materializador, y `RecordGraphicsModified(true)` solo se llama si se aplico alguna propiedad (`applied`).

### 11.5 V-BOM: dos comprobaciones

| # | Comprobacion | Regla |
|---|---|---|
| V-BOM-1 | **Multiconjunto semantico de lineas del builder del kind** | `SelectiveBomBuilder`, `SystemBomBuilder`, `PushBackBomBuilder`, `CantileverLineEditorAssembler.Bom`, `BomBuilder`: igualdad del multiconjunto de lineas (categoria, pieza, longitud, peralte, cantidad) usando **la precision y el redondeo que ese builder ya aplica** (por ejemplo, `Round(...)` de `PushBackBomBuilder.cs:403`); el orden de lineas no cuenta |
| V-BOM-2 | **Clave consolidada visible al usuario** | la clave de `ConsolidatedBom` (`ConsolidatedBom.cs:62-64`, `:110-115`): componente = categoria + longitud redondeada a 4 decimales + firma de piezas (categoria, perfil, longitud con formato `0.##`, cantidad), sumando cantidades; un BOM no basado en componentes usa la firma de sus lineas (`:89-107`) |

Ambas deben pasar. **No** se confunden: V-BOM-1 no es mas estricta que su builder y V-BOM-2 no es mas estricta que la
consolidacion.

### 11.6 Textos y cotas

No se comparan geometricamente. Se validan con **contratos semanticos**: numeracion regenerada desde el documento
reflejado (PDC-5), letras A/B por posicion, nombre de rack = `<base> - espejo`, cotas segun `Dimensions`, estilo y la
politica de `DimensionViews` de I-50, y legibilidad (rotacion de texto no invertida). La validacion visual final es del
Owner (M-16).

### 11.7 Holguras y offsets graficos

- **Ninguna holgura, offset o separacion puramente grafica se asume simetrica.** AR-01 (topes Selectivo) y el tope
  posterior de Push Back son los precedentes demostrados.
- **CT-17 audita en G3, por pieza** de cada builder de vista admisible: **span de anclas** (las dos referencias que la
  pieza cubre, si las hay), **offset unilateral** (`δ_izq`, `δ_der`), **direccion de estiramiento** del parametro a lo
  largo del eje reflejado, **origen local del bloque** (en que extremo de su geometria cae la insercion) y la **simetria
  declarada** si la hay. Clasifica cada coordenada como **simetrica** (derivada de ambos extremos o del centro) o
  **anclada** (desde un poste, troquel o extremo, con offset de un lado).
- Un `δ` que **no** se reparte simetricamente (`δ_izq ≠ δ_der`; por ejemplo `LONGITUD = span + δ` anclada a un lado) es
  `UNKNOWN` ⇒ `FAIL_CLOSED`, **cualquiera sea el rol** de la pieza. La evidencia visual-geometrica no lo corrige (§11.3
  puntos 2 y 9).
- Cada anclaje asimetrico produce una fila `UNKNOWN` con `FAIL_CLOSED` o una regla explicita aprobada por el Owner.
  Nunca un parche silencioso: una contradiccion material con V10 abre **Proposal V11**.

---

## 12. Plan de pruebas (no se implementa en este gate)

### 12.1 Caracterizacion primero (G3; pasan sobre la produccion intacta)

| # | Caracteriza | Suite |
|---|---|---|
| CT-01 | Fixtures asimetricos por kind y vista admitida con firma de plan (§11.2), con `DimensionViews` presentes | Core |
| CT-02 | BOM base de esos fixtures: multiconjunto de cada builder y clave de `ConsolidatedBom` (§11.5) | Core |
| CT-03 | Normalizaciones conocidas del store por kind (defectos de `ToDesign()`, version `"1.0"` de cabecera, legado sin envoltorio, margenes retirados) | Core |
| CT-04 | Decodificacion de `View`/`Section` de produccion por kind (tabla de §3.7), incluidos `−1` legado, secciones fuera de rango, `2..3` en Push Back simple y `View` vacia | Core + guarda de texto de las lecturas del Plugin |
| CT-05 | Tramos y centros `c` por (kind, vista) desde la geometria resuelta | Core |
| CT-06 | **PlanPlacementEvidence** (diagnostico, despues de CT-36): piezas cuya mano difiere entre `F ∘ Π(D)` y `Π(μD)` por kind y vista, en el marco local tras `T(−BlockOrigin)`. **No** infiere ni justifica centros: con la simetria verificada por estado comprueba `A ≡ B` y, si difieren, registra el desplazamiento. Incluye los fixtures negativos de T-M40 | Core (con resultados geometricos registrados en CT-36) |
| CT-07 | `WithDesign` materializa `PalletTolerance` vinculada (hueco actual del camino `EditX`) | Core |
| CT-08 | `SelectiveDesviadorPlan.CellKey` colapsa N−1/N en el Dinamico | Core |
| CT-09 | `BracingPanel.ResolveDiagonalDirection` por paridad | Core |
| CT-10 | Ausencias de Push Back compuesto: inicio, interior y final dibujan distinto | Core |
| CT-11 | Marco del brazo Cantilever por lado y familia de seccion | Core |
| CT-12 | Semantica de `MountingFace` respecto del run (S-10, D-03b) | Core |
| CT-13 | `SelectiveAuthoredAuthority` distingue un documento reflejado de su origen | Core |
| CT-14 | Censo de `[CommandMethod(` = `B` y unicidad de nombres en el SHA de reconciliacion (linea base; hoy 35 en `main @ dad4e77`, con I-54 integrada) y censos por nombre de T-GRD-08 de I-54 en main: comandos, ayuda por pares comando/alias con el alias verificado solo si no esta vacio, y ventanas (`[V9-D06]`, `[V10-D08]`) | Core + `tests/RackCad.UI.Tests` |
| CT-15 | **Topes**: holgura del tope Selectivo en frontal y planta (AR-01) y del **tope posterior de Push Back** en frontal posterior y planta (anclaje, `ElevationMirrored`, `LONGITUD`); predicado exacto de «tope activo»; **activo por defecto** (sin `OffCells` y con `PieceId` en blanco o desconocido dibuja en toda celda), por lado en compuesto, y `"(ninguno)"` (tras `Trim`, sin distinguir mayusculas) con mascara latente | Core |
| CT-16 | **Semantica observable de `RackDuplicationPlan`**: textos completos, orden de grupos, orden de referencias, avisos y errores, sobre codigo intacto (§5.1) | Core |
| CT-17 | **Auditoria de anclajes por pieza** de `SelectiveFrontalBuilder`, `SelectivePlantaBuilder`, `SelectiveTopePlan`, `SelectiveParrillaPlacement`, `SelectiveTarimaPlacement`, `SelectiveDesviadorPlan`, `SelectiveSafetyPlacement`, `DynamicSystemFrontalBuilder`, `DynamicSystemPlantaBuilder`, `DynamicViewDecorations`, `DynamicSafetyMultiViewBuilder`, `DynamicForkliftDefensePlan`, `DynamicEntranceGuidePlan`, `PushBackSystemFrontalBuilder`, `PushBackSystemPlantaBuilder`, `PushBackRearTopeBuilder`, `PushBackCompositeFrontal`, `PushBackTarimaPlacement`, `PushBackPalletProjection`, `PushBackBootPlan`, `PushBackDefensePlan`, `CantileverViewPlanBuilder`, `LateralHeaderLayoutBuilder` y `PlantaHeaderLayoutBuilder`: span de anclas, offset unilateral, direccion de estiramiento, origen local del bloque y simetria declarada; cada coordenada a lo largo del eje reflejado, simetrica o anclada; `δ` no repartido ⇒ `UNKNOWN` sin importar el rol (§11.7) | Core |
| CT-18 | **Origin**: los materializadores crean `Origin = 0`, `RackCloner` copia el de la fuente y ningun comando lo comprueba hoy | guarda de texto (sin AutoCAD en CI, ADR-0003) |
| CT-19 | **Efectos de `EnsureForPlan`**: IF-1..IF-7 de §9.5 | guarda de texto; los efectos reales son evidencia del Owner en G9 |
| CT-20 | **Filas de parrilla y de tarimas**: condicion `count·frente > span` en claros completos, reparto apilado y ausencia de desborde en tramos (`PalletFit`); **Push Back**: `FrontalRow` con `calles·lane > larguero` (override por nivel), simple y compuesto por lado (P-03c, PC-14b) | Core |
| CT-21 | N-S02: UI, BOM, round-trip e involucion respecto de la forma normal | Core + UI si hace falta |
| CT-22 | N-S06: ceros finales, listas cortas, valores `≤ 0` distintos de `0.0`, listas largas e involucion | Core |
| CT-23 | N-H02: fisica, BOM, UI, round-trip y relacion con `IsStandard`/`IsException`/`StandardBaselineId` | Core + UI si hace falta |
| CT-24 | Criterio de acreditacion del registro por kind (PV-5) | Core |
| CT-25 | Censo de claves de parametros por kind y vista **con `CatalogBlockParameters` y `CatalogBlockManifest` (I-19)** | Core |
| CT-26 | Mapa de invocaciones de builders de `EditX` —costuras de **Actualizar e Insertar** por kind y vista admitida (C2-2a, C2-2b)— y del ejecutor en las vistas admitidas (linea base de G-M9), y caminos del estado del editor (C2-4) | Core + `tests/RackCad.UI.Tests` |
| **CT-27** | **Estabilidad con dos o mas estados del registro**: N-S02 (con `Dep(i)` la copia deja de ser espejo tras el cambio; con todas las celdas gobernantes con override conmuta en ambos) y **tarimas y parrillas** con los casos A y B de §6.5.5 (S-17d, S-15d), con cota autoritativa si existe y sin ella | Core |
| **CT-28** | **Autoridad del ancho**: fondo maestro, columna vacia, `W_M(i,R) ≤ 0` con niveles, **cambio de gobernante** y predicado `Dep(i)` (§6.5.1) | Core |
| **CT-29** | **S-14 asociado**: espacio de indices del desviador bajo dos estados del registro cuando el ajuste del medio frente cambia | Core |
| **CT-30** | **Enumeracion `A_k`** de todas las vistas admisibles por kind y fixture, y conmutacion en cada una | Core |
| **CT-31** | **Linea base del cierre transitivo**: tipos y claves `(kind, ruta)` alcanzados desde las raices de §6.7, cortes con su razon, instantanea de enums y descriptores con su cierre efectivo | Core |
| **CT-32** | **Metadata desconocida sobre el contrato de serializacion real**: `JsonTypeInfo` de las mismas opciones de cada store (modifiers, converters, `PropertyNameCaseInsensitive`, `[JsonIgnore]`, `[JsonExtensionData]`); payload por kind (raiz y anidados) y exterior (sobre y envoltorio); clave desconocida vacia y no vacia; portador de la allowlist; `CustomProperties` antes y despues de I-54; colisiones de nombres; **corpus de miembros retirados por ruta JSON** (historia de los tipos persistidos: RM-1..RM-7 —RM-7 es un movido de ruta— y cualquier otro) con clasificacion R-A/R-B segun lo que hace el store | Core |
| **CT-33** | **Fuentes dinamicas, anonimas o anotativas**: clasificacion ST-17 frente a ST-2 | guarda de texto; efecto real en M-23 |
| **CT-34** | **Historia de `View` vacia del Selectivo**: `View` introducida en `6023572` antes que `Section` en `4d2ca82`; decodificacion de produccion | Core + registro documental |
| **CT-35** | **V-RT-STORE por kind, separado de C-2**: `Miembros(LB)` e idempotencia del store sobre fixtures con y sin miembros opcionales | Core |
| **CT-36** | **Viabilidad de la STATE-EVALUATED VISUAL-GEOMETRIC EVIDENCE en `acad.exe` / AutoCAD 2025**, **fuera de `src/`** (`tools/`, harness externo, evidencia del Owner o artefactos en `docs/automation/evidence/`): evaluacion de estados nuevos de bloques dinamicos en una `Database` auxiliar privada **con el contexto de la autoridad origen** para la evidencia de simetria (`PSTYLEMODE`, `INSUNITS` y escala de anotacion; un dibujo STB y otro CTB), con el **`DestinationEvaluationContext`** para las huellas visuales (DRAWING FIRST, LIBRARY SECOND WITH IGNORE; colision de nombre de bloque anidado y de nombre de simbolo, por ejemplo una capa con nombre) y con las observaciones consumidas del contexto de materializacion que puedan afectarlas (§8.7), clonando las definiciones de biblioteca con `BlockLibraryImporter.EnsureBlocks(auxDb, nombres)` y las del dibujo por clonado de solo lectura, y verificando que existen todos los BTR requeridos tras el intento de clonado (el retorno es diagnostico), sin mutar la cache ni el DWG, midiendo antes y despues `DBMOD`, `HANDSEED`, UNDO, `WorkingDatabase` (restaurada incluso ante excepcion), objetos persistidos y el contenido de la cache (HANDSEED, inventario de BTR y huella estable, observados solo desde el harness); paridad de aplicacion de parametros con `LateralHeaderDrawer.ApplyDynamicParameters` (busqueda sin distinguir mayusculas, ultima clave duplicada, `ReadOnly`, ausentes, conversion, orden y `applied`) y representacion evaluada igual a la del materializador en un documento real; **punto de observacion** soportado; valores efectivos; BTR evaluado, `Origin` y **transformacion efectiva** (anidados incluidos); **politica cerrada de clases y variantes** por tipo exacto (grosor, normales, elevacion, `MInsertBlock`, definiciones anidadas anotativas, XCLIP y filtros espaciales, atributos y definiciones XREF, overlay o dependientes anidados, tipos de linea complejos y `HatchLoopTypes`) con el **metodo de inventario cerrado** de §11.3 punto 14 (`TRANSPORTED`, `PLACEMENT`, `COMMON_CONTEXT`, `NON_VISUAL`, `UNSUPPORTED`; nada sin clasificar); `SourceKind`, `Plinegen` y `Closed`; **fuentes visuales simbolicas tipadas** por propiedad (capa `0`, `ByLayer`, `ByBlock`, anidados, estados de la API, `Color` con `ColorBook` y valor almacenado, `LineWeight` y `Transparency` tipados y `PlotStyle` segun el modo de la autoridad origen); **`VisibilityPath`** variando apagado, inutilizado, inutilizado en ventana, `Visible` de ancestros y anotativos; `LinetypeScale`, escalas y `LinetypeScale` de ancestros, fase y segmentacion de tipos de linea, escalas globales (`LTSCALE`, `PSLTSCALE`, `MSLTSCALE`, `CELTSCALE`) y multiplicidad; **orden visual** de superposiciones materiales dentro de la pieza y entre piezas (con CT-48); **huellas visuales de ocupacion de modelo** (`VisualFootprintResult`) de toda instancia enumerada, con o sin cambio de mano, construidas tras transformar a WCS: escala de la fuente `s ≠ 1`, escalas anidadas, ancho geometrico de modelo frente a `LineWeight`, anchos solo de pantalla o de trazado excluidos, rellenos, transparencia, mascaras, limites de texto y de cota con escala de anotacion, marcos, anidados, STB/CTB, y su `FootprintInputFingerprint`; ruta anidada estable; poligono SOLID/TRACE; bucles y estilo de islas de HATCH; degradados y patrones; canonicalizacion conservadora con tasa de falsos positivos y negativos; definicion local frente a biblioteca (`DuplicateRecordCloning.Ignore`); obsolescencia (`TraceFingerprint`); casos C, D, E y F (doce variantes); rendimiento preliminar por rack; **SHA-256** de la biblioteca registrado. `accoreconsole` solo como comparacion. Si la ruta no es viable ⇒ Proposal V11 o reduccion de alcance | evidencia registrada; Core consume los resultados |
| **CT-37** | **Diferencial de estabilidad 9b, incondicional**: dos o mas estados del registro por **cada** descriptor (con cota autoritativa si existe; sin cota: cero, negativos y grandes), fondo maestro distinto y cambio de gobernante; en cada par de estados compara **siempre** la equivalencia visual-geometrica de todas las vistas admisibles, **V-BOM-1**, **V-BOM-2** y las firmas de estado (`EvaluatedStateSignature`); toda decision `ALLOW` en `R_a` conmuta en `R_b`; parametros dinamicos que varian con el vinculo cubiertos por DEP-09/DEP-10a..c; lema de definicion. La equivalencia visual-geometrica por estado entra con **dobles** de `GeometrySymmetryResult`, no con AutoCAD (`[V7-D18]`); la evidencia real por estado es CT-36 | Core |
| **CT-38** | **Cierre efectivo por descriptor**: variar el valor del descriptor y comparar por reflexion el sistema resuelto **y los parametros dinamicos de cada instancia de plan**; todo simbolo que cambia fuera del cierre declarado ⇒ RED | Core |
| **CT-39** | **Precedencia de fuente C1..C5** (§4.1): objetos no RackCad con escala, `Normal`, `Origin` o MINSERT no canonicos se ignoran sin abortar; bloque de usuario con racks anidados (ST-2b); payload ilegible (ST-2c) | Core (clasificacion pura) + guarda de texto de la captura |
| **CT-40** | **Read-set por observaciones** (§9.2.1): acreditacion usada, variables leidas, cambio irrelevante frente a cambio observado | Core |
| **CT-41** | **Baseline de I-53 en main**: ningun tipo de I-52 en `RackCad.Application.Systems.Shared`; C-08 y C-10 (en main desde `1b091be`) siguen verdes; I-53S G5 (`e528ef2`), **integrada en main** en `104ef3a`, conecta la ventana del Selectivo con clasificacion exacta (contrato de I-52 NON-MATERIAL; coordinacion MATERIAL; baseline de C2-4 MATERIAL / CHANGED): como integro antes del Candidato, CT-26, C2-4, T-M30 y las guardas relacionadas se ejecutan sobre el arbol combinado tras el rebase; una integracion posterior al Candidato se reporta al Coordinador y se re-mide contra un main que contenga I-52; I-53D G7 (`a57bd50`, equivalente a `d280197`), **integrada en main** en `dad4e77`, cambia la baseline de C2-4 del Dinamico: como integro antes del Candidato, CT-26, C2-4, T-M30, T-M17 y T-M17b sobre el arbol combinado tras el rebase (`[V9-D22]`, `[V10-D15]`, `[V10-D23]`); `C2_4_I52_TheEditorStateAfterADistribution_EqualsTheReopenedDocument_InEveryView` de I-53S no cuenta como C2-4 de I-52 (`[V8-D17]`) | Core + `tests/RackCad.UI.Tests` |
| **CT-42** | **Enums con el contrato real del converter**: nombres desconocidos (fallo de lectura, E2), enteros no definidos (X-20), instantanea con `HeaderBlockRole` y `CantileverVisualRole`, invariante forward-compatible de `DimensionViewVisibility` | Core |
| **CT-43** | **`OffCells` latentes del tope posterior**: indices validos remapeados (P-05b, PC-08b) y fuera de rango `UNKNOWN` (P-05c, PC-08c), por lado | Core |
| **CT-44** | **Parametros dinamicos que varian con vinculos, por camino**: `LONGITUD` de largueros con y sin `Dep(i)`; `LONGITUD` de postes por DEP-10a (cabecera por poste usable), DEP-10b (claros adyacentes con y sin `HeightOverride`, claros vacios, fallback) y DEP-10c (poste intermedio con y sin `HeightOverride` del claro que lo contiene); dos estados del registro | Core |
| **CT-45** | **Campos start/end de cabecera**: `StartConnectionPointId`/`EndConnectionPointId` como extremo inferior/superior bajo `UpRight`, `UpLeft`, doble diagonal, K y X; retranqueos y separaciones verticales (`DiagonalStart/EndOffsetTroqueles`, `DiagonalDoubleSpacingTroqueles`, `HorizontalDoubleOffsetTroqueles`, `CelosiaStartTroquel`, `PasoTroquel`, `PanelClear`) invariantes bajo μ_D; `FrameMemberEndRole` no persistido; cualquier otro offset start/end alcanzado por la guarda, clasificado con evidencia o `UNKNOWN` | Core |
| **CT-46** | **Guardas cruzadas vigentes**: relee en el SHA de la reconciliacion las guardas de texto de `ProjectVariablesConformanceTests.cs` (main y, si integra, I-49), C-08/C-10, las del nucleo de expresiones (extendidas por I-49 G6 `5f969cc`, con `LengthUnitsAuthorityGuardTests` sobre todo `src/`) y T-GRD-01..T-GRD-08 de I-54, ya en `main @ ba497f1` (incluidas las del borde fisico, T-GRD-04..T-GRD-07, y los censos por nombre de comandos, ayuda y ventanas, T-GRD-08); fija la lista exacta que G-M18 aplica a los archivos de I-52 y la autoridad vigente de I-49 (ADR-0040, aceptado y que reemplaza a ADR-0038, `[V8-D18]`) | Core |
| **CT-47** | **Corpus indicativo de la biblioteca actual**: recalcula el **SHA-256** del archivo antes de usar la evidencia (identidad; ruta, tamaño y fecha son diagnostico) y, si cambia, no reutiliza la evidencia previa; recalcula en `acad.exe`, con evaluacion de estados, la tabla de §1.4 por pieza y vista con las reglas de V10 (politica cerrada de variantes con inventario cerrado, `SourceKind`, `Plinegen` y `Closed`, fuentes visuales simbolicas tipadas, `VisibilityPath`, `LinetypeScale` y contexto de patron, orientacion, multiplicidad, orden visual dentro de la pieza y entre piezas sobre huellas de ocupacion de modelo en el contexto destino y pares invertidos, degradados, falsos positivos y negativos de la canonicalizacion) y la compara con la evidencia indicativa; una reduccion material de la envolvente ⇒ Proposal V11 | evidencia registrada |
| **CT-48** | **Orden visual de plan por kind y vista** (`[V8-D12]`, `[V9-D03]`, `[V10-D01]`, `[V10-D03]`): orden de emision de cada builder de vista admitida y orden de materializacion de `LateralHeaderDrawer` (colocaciones de grupos y despues piezas sueltas, en `CreateSystemBlock` y `RedefineSystemBlock`) y de `CantileverViewMaterializer`; **enumeracion de los pares de instancias cuyo orden relativo cambia** entre `F ∘ Π(D)` y `Π(μ_k D)`, incluidas las piezas **sin** cambio de mano; superposicion de sus huellas de ocupacion de modelo (`VisualFootprintResult`, con la geometria evaluada de CT-36 en el contexto destino o con dobles) y su clasificacion material, sin anchuras de presentacion (L-37); huellas ausentes o `UNKNOWN`; correspondencia de homologas ambigua; **frescura**: pares re-evaluados con las huellas recalculadas tras `EnsureForPlan`, incluido un par con una pieza sin cambio de mano cuya huella cambia; **reduccion de alcance** registrada por kind y vista | Core (con resultados geometricos de CT-36) |
| **CT-49** | **Presentacion de la referencia fuente y orden en Model Space en `acad.exe`** (`[V8-D13]`, `[V8-D14]`, `[V9-D01]`, `[V9-D04]`, `[V9-D08]`, `[V9-D09]`, `[V10-D01]`, `[V10-D03]`, `[V10-D06]`, `[V10-D07]`), **fuera de `src/`**: **inventario cerrado** de las propiedades y el estado de `Entity` y `BlockReference` de AutoCAD 2025 con el metodo de §11.3 punto 14, clasificados como `TRANSPORTED` (ST-13), `PLACEMENT`, `COMMON_CONTEXT`, `NON_VISUAL` o `UNSUPPORTED` (ST-18), sin nada sin clasificar; lectura y copia fiel de las propiedades de ST-13, con `PlotStyleName` en un dibujo STB y la derivacion del color en uno CTB; deteccion de XCLIP y otras entradas de `ACAD_FILTER`, referencias de atributo, entradas del diccionario de extension y XData de efecto visual; **orden efectivo** de Model Space con `SortentsTable`, `SORTENTS`, `DRAWORDERCTL` y cualquier contexto adicional encontrado, en pantalla y en trazado, y el conflicto entre ambos para dos fuentes seleccionadas cuyas copias se superponen (E10); **huellas de ocupacion de modelo** de copias y de objetos existentes (`Extents3d` solo ampliado por ocupaciones de modelo, nunca por `LineWeight`, `LWDISPLAY`, CTB/STB ni escala de trazado; mascaras y marcos; envolventes de texto y de cota bajo `DIMSTYLE`, `TEXTSTYLE` y escala de anotacion; `XLINE` y `RAY` con huella o interseccion analitica fiable o `UNKNOWN`); clasificacion de la superposicion de una copia con **todo** objeto posterior a su fuente: otra fuente seleccionada posterior, objeto seleccionado e ignorado posterior, objeto no seleccionado posterior, objeto coincidente, `XLINE` o extension no finita, mascaras, rellenos, grosor, degradados, wipeouts, imagenes, OLE y referencias que los contengan; orden resultante de las copias creadas en el orden de sus fuentes; **frescura**: una huella de biblioteca distinta de la del dibujo real tras `EnsureForPlan`, en una pieza sin cambio de mano, que la repeticion de ST-19 y del orden de plan detecta; valores efectivos de creacion observables en la referencia y en las piezas nuevas (con CT-50) | evidencia registrada; Core consume la clasificacion |
| **CT-50** | **Contexto de materializacion y su read-set en `acad.exe`** (`[V9-D05]`, `[V10-D04]`, `[V10-D05]`), **fuera de `src/`**: que mecanismo y que valores efectivos aplica AutoCAD 2025 a las propiedades que los drawers no asignan explicitamente —referencias de `LateralHeaderDrawer` (colocaciones de grupos y piezas), `DBText` de anotaciones, cotas y curvas de `CantileverViewMaterializer`—, al menos `CLAYER`, `CECOLOR`, `CELTYPE`, `CELTSCALE`, `CELWEIGHT`, `CETRANSPARENCY`, `CPLOTSTYLE` y `PSTYLEMODE`, el `DIMSTYLE` actual y su registro, el `TEXTSTYLE` efectivo, la escala de anotacion, los registros de capa `RACKCAD_ANOTACIONES`, `RACKCAD_COTAS` y de las capas de rol, y cualquier otro estado que se descubra; **read-set por productor**: cambiar un estado que el productor no consume (por ejemplo `CLAYER` en un productor que fija la capa) no invalida, y cambiar uno consumido (`DIMSTYLE`, `TEXTSTYLE` u otro) si, durante la peticion de la linea y entre PREPARE y MUTATE; consumo no determinable ⇒ `UNKNOWN`; comprobar que `Π(D)` y `Π(μ_k D)` usan las mismas observaciones; productor que las aplique de forma asimetrica ⇒ `UNKNOWN`; efecto sobre la presentacion interna frente a una fuente dibujada con otro contexto (L-36) | evidencia registrada; Core consume la clasificacion |

### 12.2 Pruebas nuevas (RED antes de implementar)

| # | Afirma | Gate |
|---|---|---|
| T-M01 | `ReflectionAboutLine`: involucion, determinante −1, puntos de la linea fijos | G4 |
| T-M02 | Colocacion: `P' = G·P·F` con determinante positivo; rotacion `2φ−θ+π` con `X_c` y `2φ−θ` con `Y_c`; escala conservada; descomposicion validada; ejemplos E-1..E-3 | G4 |
| T-M03 | Transformacion fuente ST-1..ST-15 (clasificacion, canonizacion `(−s,−s)`, `Origin`, `View`/`Section`) | G4 (parte pura) y G7 (captura) |
| T-M04 | Nucleo neutral y fachada `RackDuplicationPlan` con T1–T15 intactos y **semantica observable identica a CT-16** (textos, orden, avisos, errores) | G4 |
| T-M05 | Asignador con politica de nombre inyectada: «- copia» historico y `<base> - espejo` | G4 |
| T-M06 | `Assess` por kind: cada fila `MC` y `UK` de §7 y cada precondicion de §7.9 detectada con motivo atribuible | G5 |
| T-M07 | `Reflect` involutivo: `Reflect(Reflect(D)) = N(D)` para diseños con disposicion `ALLOW` o `CANONICALIZE` | G5 |
| T-M08 | Normalizaciones N-S02, N-S06, N-S07 y N-H02 con las obligaciones 1..9 de §6.5 y la 9b de §6.5.5 | G5 |
| T-M09 | V-BOM-1 y V-BOM-2 por kind sobre fixtures | G5 |
| T-M10 | Portadores de la allowlist de §6.4 identicos (CA-1..CA-5), incluidos `PropertyValues` con `Kind` desconocido, `DimensionViews` y `CustomProperties` | G5 |
| T-M11 | Autoridad authored por kind (divergente, ilegible, igual) | G5 |
| T-M12 | Conmutacion `Π(μD) ≡ F ∘ Π(D)` por (kind, vista admisible) | G6 |
| T-M13 | Autoridad de plan = builders existentes para las mismas entradas | G6 |
| T-M14 | Plan del espejo: E1..E8 sin mutacion, grupos multi-rack con `NewRackId` distintos, sin `WithDesign` | G6/G7 |
| T-M15 | Guardas del Plugin (§12.3) con RED demostrado por violacion temporal no commiteada | G7 |
| T-M16 | Censo `B` → **`B + 1`** (`GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA` reapuntada con motivo; `B` medido en el SHA de reconciliacion, hoy 35 en `main @ dad4e77`, `[V9-D06]`) y, **incondicional** con I-54 integrada, T-GRD-08 reapuntado con motivo y sin debilitar (`[V10-D08]`): `RACKMIRROR` añadido por nombre al censo de comandos; en la ayuda, entrada de `RACKMIRROR` **sin alias**, con el `Command` siempre registrado como `[CommandMethod]`, el `Alias` verificado solo si la entrada tiene alias no vacio y el alias ausente como estado valido y explicito (no se inventa alias); ayuda +1 entrada, registros de alias +0; censo de ventanas sin cambio | G7 |
| T-M17 | C-2: autoridad frente a `EditX` → Actualizar (C2-1 = C2-2a) | **G6** |
| **T-M17b** | C-2 con **Insertar** desde la copia (C2-1 = C2-2b), o fixture compartido con la guarda G-M9 de costura comun | **G6** |
| T-M18 | Cruce I-50: `DimensionViews` exacto a traves del espejo (centinelas `-8`, `13`, `null`) — **incondicional** | G5 (portador) y G8 (cruce final) |
| T-M19 | Cruce I-49: entradas `expression` sobreviven y la autoridad efectiva integrada se invoca | G8 (si I-49 integrada) |
| T-M20 | Cruce I-54 (integrada en main): propiedades de alcance Rack sobreviven exactas y T-GRD-01..T-GRD-08 siguen verdes con el espejo | G5 (portador) y G8 (cruce final) |
| T-M21 | Huella de la referencia de §9.2: cambio de cualquier campo observado entre SNAPSHOT, PREPARE y MUTATE ⇒ fail-closed; un estado del contexto de materializacion que ningun productor consume no invalida; `FootprintInputFingerprint` distinto de la postcondicion de PREPARE al inicio de MUTATE ⇒ excepcion y rollback | G4 (comparador puro) y G7 |
| T-M22 | Canonizacion de `View`/`Section` por kind (§3.7), incluida la `View` vacia del Selectivo con `Section ≥ 0` ⇒ FAIL_CLOSED | G5 |
| T-M23 | `MissingInstances.Count > 0` ⇒ excepcion en el camino de I-52 y ninguna copia | G6 (guarda G-M11 + build Plugin; efecto real en M-19) |
| T-M24 | Sustrato real por kind (lectura y escritura con la autoridad existente) | G5 |
| T-M25 | Criterio PV-5: Selectivo con registro no acreditable ⇒ E7 con y sin vinculos; kinds sin vinculos ignoran el registro | G5 |
| T-M26 | La autoridad de plan con `RackName` dibuja `<base> - espejo` en la anotacion de nombre | G6 |
| T-M27 | Equivalencia con `GeometrySymmetryResult` visual-geometrico verificado (dobles en Core): desplazamiento con el centro verificado en el marco del BTR evaluado y volteo de mano; `IsSymmetric = false`, firma visual distinta, clase fuera de la allowlist, ciclo o eje oblicuo ⇒ no equivalente | G6 |
| T-M28 | Deteccion `UK` de topes Selectivo (S-13/S-13b), **tope posterior de Push Back activo (P-04, P-05, PC-08)** —incluida la configuracion por defecto sin `OffCells` y con `PieceId` en blanco—, parrilla desbordada (S-15x) y Auto con baseline (H-06); `"(ninguno)"` (tras `Trim`, sin distinguir mayusculas) inactivo con mascara remapeada (P-05b, PC-08b) | G5 |
| T-M29 | C-2 con el ejecutor de variables del Selectivo (C2-1 = C2-3) | **G6** |
| T-M30 | C-2 con el estado del editor → sistema (C2-1 = C2-4), en Core y en UI cuando pase por la ventana | **G6** |
| **T-M31** | **Obligacion 9**: con dos estados del registro, S-02b detectada (`MC`) cuando `Dep(i)`; S-02a (`RN`) conmuta en ambos cuando todas las celdas gobernantes tienen `BeamLengthOverride` | G5 |
| **T-M32** | S-14c `UK` cuando el espacio de indices del desviador depende de un medio frente con `Dep(i)`; dos estados del registro | G5 |
| **T-M33** | **V-VIEW-ALL**: se enumeran todas las vistas admisibles sin leer el dibujo; una vista admisible **no seleccionada** que no conmuta hace fallar el rack | G6 |
| **T-M34** | Tarimas: S-17c `UK` con fila desbordada; S-17b sin desborde conmuta; S-17d `UK` con `Dep(i)` sin `DEP-ROW`; **Push Back**: P-03c y PC-14b `UK` con fila desbordada; P-03b y PC-14a conmutan | G5/G6 |
| **T-M35** | **V-META**: desconocido no vacio en el payload ⇒ `UK`; clave exterior desconocida no vacia (sobre o envoltorio) ⇒ `UK`; clave desconocida vacia, portadores de la allowlist y retirados **declarados por el store** (RM-5, RM-6) no bloquean; retirados R-A y R-B ⇒ `UK` (T-M50); duplicado ambiguo de `CustomProperties` ⇒ `UK` | G5 |
| **T-M36** | **Cierre transitivo**: desde las raices de §6.7, cada clave `(kind, ruta)` alcanzada y cada descriptor clasificados exactamente una vez; tipo, miembro, corte o descriptor nuevo sin clasificar ⇒ RED; un tipo nuevo agregado a un DTO existente aparece sin tocar ninguna lista de tipos | G5 |
| **T-M37** | ST-17: referencia dinamica, anonima o anotativa con payload RackCad ⇒ E4 con mensaje propio; sin payload ⇒ ST-2 | G7 |
| **T-M38** | **V-RT-STORE** (`Miembros(LB) ⊆ Miembros(M)` e idempotencia), independiente de C-2 | G5 |
| **T-M39** | V-BOM-1 frente a V-BOM-2: un fixture donde difieren las precisiones demuestra que ninguna sustituye a la otra | G5 |
| **T-M40** | **Negativa de simetria generalizada**: siguen **no** equivalentes (1) fixture de rol `Tope` (AR-01 y tope posterior de Push Back); (2) pieza de otro rol con parametro de longitud y `δ` unilateral (caso C); (3) pieza de ancho fijo anclada a un lado (caso D); (4) **pieza asimetrica con caja envolvente simetrica** (caso E); (5) centro candidato deliberadamente incorrecto; (6) **geometria perfectamente simetrica con fuentes visuales, clase de origen, contexto o rutas distintas** (caso F, doce variantes): mismo color actual `ByLayer` frente a `Explicit`; `InheritLayer` frente a `InheritBlock`; `VisibilityPath` distinta; `LineWeight` distinto; `Transparency` distinta; `LinetypeScale` distinta; `PlotStyle` distinto si el entorno lo permite caracterizar; `LinetypeSource` `ByLayer` frente a `Explicit` con el mismo tipo actual; `FillSignature` distinta; `SourceKind` distinta; `Plinegen` o `Closed` distintos; contexto de escala de ancestros o de patron distinto. Cada variante se ejecuta por cada clase soportada donde tenga sentido (matriz clase × caso). Ninguna depende del rechazo por rol `Tope`, que queda como defensa adicional | G6 |
| **T-M41** | Read-set por observaciones (§9.2.1): un cambio irrelevante no aborta; un cambio de una observacion (acreditacion, variable leida, dependencia transitiva si I-49) aborta en PREPARE y en MUTATE | G4 (comparador) y G7 |
| **T-M42** | `RepresentabilityReport` lleva las tres dimensiones separadas por fila (clase, evidencia, disposicion) | G5 |
| **T-M43** | **V-DEP**: toda fila `ALLOW`/`CANONICALIZE` declara `EffectiveInputs`; las que intersecan un cierre de descriptor tienen entrada V-DEP con prueba o motivo; fila sin declaracion ⇒ RED | G5 |
| **T-M44** | Instantanea de enums alcanzados y de decodificacion de secciones; valor nuevo, renombrado o retirado ⇒ RED | G5 |
| **T-M45** | `DEP-ROW` con dos estados del registro: casos A y B ⇒ S-17d y S-15d `UK`; con overrides que cubren la fila ⇒ `ALLOW` y conmuta en ambos estados | G5 |
| **T-M46** | Precedencia de fuente C1..C5: objetos no RackCad con escala, `Normal`, `Origin` o MINSERT no canonicos se ignoran sin abortar; ST-2b con aviso especifico; ST-2c ⇒ E2 | G4 (clasificacion pura) y G7 (captura) |
| **T-M47** | Desde G5, ninguna fila del `RepresentabilityReport` lleva `EvidenceStatus = G3_PENDING` (regla de cierre de G3) | G5 |
| **T-M48** | Consumo de `GeometrySymmetryResult`: sonda no disponible, sin paridad, sin punto de observacion o sin contexto reproducible de la base auxiliar, bloques requeridos ausentes tras `EnsureBlocks` o ruta anidada ambigua ⇒ X-17a; clase o variante fuera de la politica cerrada, degradado, patron o ciclo ⇒ X-17b; fuentes visuales simbolicas tipadas, `SourceKind`, `PolylineGeneration`, `LinetypePatternContext` o `VisibilityPath` distintas, visibilidad no modelada, orden visual no demostrado u orden de plan no conservado ⇒ X-22; transformacion efectiva alterada ⇒ X-23; correspondencia ambigua ⇒ X-17a; derivacion del eje (multiplo de `π/2`) y eje oblicuo ⇒ `UNKNOWN` | G6 |
| **T-M49** | Estado dinamico y vinculos: evidencia de un estado con `Dep(i)` o sin `IndepAltura` ⇒ S-26/S-27a..S-27c `UK` (X-17c); con independencia demostrada ⇒ `ALLOW` si `IsSymmetric`; nunca universalidad por muestras | G5/G6 |
| **T-M50** | Miembros retirados: R-A (incluido RM-7) con el mensaje de remedio y R-B con el mensaje sin remedio; ninguna limpieza automatica; retirados declarados por el store (RM-5, RM-6) no bloquean | G5 |
| **T-M51** | Contrato de serializacion: colisiones de nombres ⇒ X-21; enteros no definidos ⇒ X-20; la guarda se alimenta del `JsonTypeInfo` real y no de reflexion CLR | G5 |
| **T-M52** | `OffCells` latentes: indices fuera de rango ⇒ P-05c/PC-08c `UK`; indices validos remapeados | G5 |
| **T-M53** | Sonda sin mutacion: la evaluacion no deja definiciones, referencias ni capas nuevas en el dibujo del usuario; `DBMOD`, `HANDSEED` y el estado de UNDO no cambian, `WorkingDatabase` queda restaurada tambien ante excepcion y el contenido de la cache de biblioteca no cambia (guardas G-M16 y G-M20 y prueba del Plugin donde sea ejecutable; efecto real en CT-36 y M-27) | G6/G7 |
| **T-M54** | Fuentes visuales simbolicas tipadas y firma: resolucion de capa `0`, `ByLayer` y `ByBlock` por la cadena de anidados en `Explicit(metodo, valor)`, `ByLayer(Named)`, `InheritLayer` e `InheritBlock` con rutas canonicas estables y **nunca** por valor resuelto; `ACI(7) ≠ RGB(255,255,255)`, `ColorBook` igual solo con el mismo libro, nombre y valor almacenado, `LineWeight` por estado y valor enumerado, `Transparency` por metodo y alfa, `LinetypeScale` exacta; son **no** simetricas las doce variantes del caso F: (1) mismo color actual con `ByLayer` frente a `Explicit`; (2) `InheritLayer` frente a `InheritBlock`; (3) `VisibilityPath` distinta; (4) `LineWeight` distinto; (5) `Transparency` distinta; (6) `LinetypeScale` distinta; (7) `PlotStyle` distinto si el entorno lo permite caracterizar; (8) `LinetypeSource` `ByLayer` frente a `Explicit` con el mismo tipo actual; (9) `FillSignature` distinta; (10) `SourceKind` distinta; (11) `Plinegen` o `Closed` distintos; (12) contexto de escala de ancestros o de patron distinto; matriz clase × caso; metodo o estado de la API no modelado ⇒ `UNKNOWN`; dobles en Core (CT-37), sin AutoCAD | G6 |
| **T-M55** | Primitivas canonicas conservadoras: fusion de rectas solo con continuidad demostrable (`LinetypeSource` `Explicit` sin trazos), colineales, contiguas, sin hueco, cruce material, bifurcacion ni cambio de firma (misma `SourceKind`) y con multiplicidad no material; sin continuidad demostrable no hay fusion y cuentan orientacion, `LinetypeScale`, `LinetypePatternContext` y multiplicidad; entidades transparentes coincidentes conservan multiplicidad; polilinea con ancho, grosor o normal no +Z ⇒ `UNKNOWN`; una polilinea **no** se empareja ni se fusiona con `Line` o `Arc` sueltos aunque su geometria sea equivalente; `Closed` siempre y `Plinegen` sin continuidad demostrable entran en la firma; arcos con sentido normalizado solo si no altera la representacion; splines por curva evaluada; nunca fusion entre familias ni entre `SourceKind` distintas; ambiguedad ⇒ `UNKNOWN` | G6 |
| **T-M56** | Paridad sonda-materializador: con una costura unica, ambos llaman la misma rutina; si no, prueba de equivalencia de busqueda sin distinguir mayusculas, ultima clave duplicada, `ReadOnly`, claves ausentes, conversion, orden y **`applied`** (`RecordGraphicsModified(true)` solo si se aplico alguna propiedad); `EvaluatedStateSignature` usa los valores efectivos leidos despues del punto de observacion | G6 |
| **T-M57** | Origen y transformacion efectivos: `o_p` es el `Origin` del BTR evaluado de la instancia (no el del dinamico base) y cada nivel anidado usa el suyo; tras el punto de observacion se leen `Position`, `Rotation`, `ScaleFactors`, `Normal` y `BlockTransform`; una transformacion alterada por una accion dinamica no representada ⇒ X-23 | G6 |
| **T-M58** | DEP-10 por camino: DEP-10a independiente con cabecera por poste usable; DEP-10b solo con todos los claros adyacentes existentes con `HeightOverride > 0`; DEP-10c solo con `HeightOverride > 0` del claro que contiene el poste intermedio; ningun predicado se aplica a otro camino | G5 |
| **T-M59** | Orden visual: la relacion solo se construye para superposiciones materiales (relleno ↔ relleno, relleno ↔ trazo, trazos coincidentes con firma distinta, entidades transparentes); orden relativo de homologas conservado ⇒ comparable; no conservado o no demostrable ⇒ X-22; objetos sin superposicion no generan relacion | G6 |
| **T-M60** | `VisibilityPath`: igualdad ordenada de `(Visible, LayerSource)` de ancestros y primitiva; homologas en anidados de capas distintas, una anidada y otra directa, o con `Visible` falso en un ancestro ⇒ no simetrica; filtro espacial, objeto o definicion anotativos o estado de visibilidad dinamico no reflejado ⇒ `UNKNOWN`; `MInsertBlock` anidado y XCLIP anidado ⇒ `UNKNOWN` tambien por la politica de variantes | G6 |
| **T-M61** | `PlotStyleSource` segun el modo de estilos de trazado de la **autoridad origen**: con estilos dependientes del color se deriva de `ColorSource`; con estilos con nombre es fuente propia; modo desconocido, no reproducido en la base auxiliar o cambiado por un clonado ⇒ `UNKNOWN`; fixtures STB y CTB | G6 |
| **T-M62** | SOLID, TRACE y HATCH: SOLID/TRACE con `Thickness = 0` y `Normal ≈ +Z`, poligono 1-2-4-3; mismo conjunto de vertices con otro orden ⇒ no equivalente; HATCH solido con `Normal ≈ +Z`, `Elevation` admitida, bucles con `HatchLoopTypes` y estilo de islas canonizables ⇒ comparable; `HatchLoopTypes` distintos ⇒ no equivalente; grosor, normal o elevacion fuera de la tabla, ambiguo, degradado o patron ⇒ `UNKNOWN` | G6 |
| **T-M63** | Acceso a la cache de biblioteca, clonado y precedencia (`[V9-D07]`, `[V10-D02]`, `[V10-D09]`): la sonda obtiene las definiciones de biblioteca solo con `BlockLibraryImporter.EnsureBlocks(auxDb, nombres)` (o la API publica equivalente vigente) y las del dibujo por clonado de solo lectura, con la precedencia **DRAWING FIRST, LIBRARY SECOND WITH IGNORE** en el `DestinationEvaluationContext` (nunca library-first), y verifica que existan **todos** los BTR requeridos en la base auxiliar tras el intento de clonado; el retorno de `EnsureBlocks` es diagnostico (un 0 sin faltantes no es fallo); BTR ausente ⇒ `UNKNOWN`; **colision de nombre de bloque anidado** y **colision de nombre de simbolo** (capa con el mismo nombre y otras propiedades) resueltas como en MUTATE o `UNKNOWN`; ni reflexion ni acceso al campo cacheado; HANDSEED, inventario de BTR y huella del contenido de la cache iguales antes y despues, observados solo desde un harness fuera de `src/`; la inicializacion perezosa no cuenta como mutacion (prueba del Plugin donde sea ejecutable; efecto real en CT-36) | G6/G7 |
| **T-M64** | Politica cerrada de clases y variantes (`[V8-D01]`..`[V8-D05]`): tipo exacto (una subclase, incluido `MInsertBlock`, no hereda el soporte); `Line`, `Arc` o `Circle` con `Thickness ≠ 0` o normal no +Z ⇒ `UNKNOWN`; `Ellipse` o `Spline` no planas en XY ⇒ `UNKNOWN`; definicion anidada anotativa, XCLIP anidado, referencia anidada con atributos o con definicion XREF, overlay o dependiente y tipo de linea complejo ⇒ `UNKNOWN`; propiedad, entrada o estado sin clasificar o `UNSUPPORTED` en el inventario cerrado ⇒ `UNKNOWN` (X-17b, `[V9-D08]`) | G6 |
| **T-M65** | Contexto de patron de tipo de linea (`[V8-D08]`): para trazos sin continuidad demostrable, escalas absolutas o `LinetypeScale` de ancestros distintas entre homologas ⇒ no simetrica; ancestro con escala no uniforme ⇒ `UNKNOWN`; con continuidad demostrable el contexto no cuenta; `LTSCALE`, `PSLTSCALE` y `MSLTSCALE` no entran en la firma | G6 |
| **T-M66** | Ruta anidada estable (`[V8-D11]`): el paso usa la identidad estable de la definicion base (y los valores dinamicos efectivos si es dinamica) y el ordinal canonico por contenido; nunca `ObjectId`, handle ni nombre anonimo (`*U…`); la misma definicion evaluada en la base auxiliar (PREFLIGHT) y en el dibujo (PREPARE) produce la misma ruta y el mismo `TraceFingerprint`; definicion anidada anonima sin base con nombre o empate no resoluble ⇒ `UNKNOWN` | G6 |
| **T-M67** | Orden visual de plan (`[V8-D12]`, `[V9-D03]`, `[V10-D01]`, `[V10-D03]`): pares cuyo orden relativo no cambia no necesitan huella; en los pares invertidos, huellas de ocupacion de modelo sin superposicion material ⇒ sin relacion y con superposicion material ⇒ X-22; **pieza sin cambio de mano** en un par invertido (su huella tambien se evalua); huella ausente o `UNKNOWN` ⇒ X-22; correspondencia de homologas ambigua ⇒ X-22; superposicion solo por anchura de linea sin superposicion de soportes de modelo ⇒ sin relacion (L-37); un plan con el mismo multiconjunto y el orden invertido en un par material hace que la vista no conmute (V-VIEW-ALL); **frescura**: la huella de una pieza sin cambio de mano que cambia tras `EnsureForPlan` obliga a re-evaluar el par y, si deja de demostrarse, E10 (PRE-18) | G6 |
| **T-M68** | Presentacion de la referencia fuente (`[V8-D13]`, `[V8-D14]`, `[V9-D08]`, `[V9-D09]`): la referencia nueva lleva `LayerId`, `Color`, `Linetype`, `LinetypeScale`, `LineWeight`, `Transparency` y `Visible` tipados de su fuente, asignados explicitamente, y `PlotStyleName` solo con STB (con CTB deriva del color y no se asigna); fuente con XCLIP u otra entrada de `ACAD_FILTER`, referencias de atributo, entrada de diccionario de extension o XData de efecto visual no clasificada, o propiedad o estado sin clasificar o `UNSUPPORTED` en el inventario cerrado de CT-49 ⇒ E4 con mensaje propio antes de MUTATE; nunca un aviso ni una copia sin presentacion | G4 (clasificacion pura) y G7 (captura y creacion) |
| **T-M69** | Orden visual en Model Space (`[V9-D01]`, `[V9-D04]`, `[V9-D19]`, `[V10-D03]`, `[V10-D06]`, `[V10-D07]`): copias creadas en el orden efectivo relativo de sus fuentes; objeto anterior a la fuente ⇒ sin clasificar; **todo** objeto posterior a la fuente con superposicion de huellas material o no clasificable ⇒ E10 en PREPARE, con los casos: otra fuente seleccionada posterior; objeto seleccionado e ignorado posterior; objeto no seleccionado posterior; objeto coincidente; `XLINE` o `RAY` posterior con huella `UNKNOWN`, aunque este lejos; mascaras, rellenos y anchos geometricos; huella no disponible o no conservadora; contexto de orden no legible; **dos fuentes seleccionadas con orden distinto en pantalla y en trazado** cuyas copias se superponen ⇒ E10; copia sobre su propia fuente permitida; rechazo anticipado antes de `EnsureForPlan` y decision definitiva despues: una huella de biblioteca que difiere del dibujo real tras importar se recalcula y la repeticion de ST-19 lo detecta; cambio del contexto de orden, de las huellas o de su `FootprintInputFingerprint` entre PREPARE y MUTATE ⇒ excepcion y rollback | G7 |
| **T-M70** | Contexto de la base auxiliar para la evidencia de simetria (`[V8-D07]`): `PSTYLEMODE`, `INSUNITS` y escala de anotacion reproducidos desde la autoridad origen (dibujo para una definicion existente; biblioteca para una solo de biblioteca, con re-verificacion en PREPARE); contexto no reproducible o no verificado ⇒ `UNKNOWN` (X-17a); ninguna fuente leida tras un clonado que cambie el contexto; la huella visual no usa este contexto sino el `DestinationEvaluationContext` (T-M63, T-M71) | G6/G7 |
| **T-M71** | `VisualFootprintResult` (`[V9-D02]`, `[V10-D01]`..`[V10-D04]`, `[V10-D07]`): autoridad separada de `GeometrySymmetryResult`; **ocupacion de modelo en WCS** que nunca subestima (soporte de modelo de los trazos, ancho geometrico real, rellenos, mascaras con tamaño en modelo, limites de texto y de cota bajo su contexto resuelto, marcos, anidados aplanados); regiones construidas **despues** de transformar a WCS: una fuente con `s ≠ 1` y escalas anidadas producen la ocupacion de la geometria dibujada; **no** se dilata por `LineWeight`, `LWDISPLAY`, CTB/STB, milimetros de papel ni escala de trazado; evaluada en el `DestinationEvaluationContext`; `FootprintInputFingerprint` presente y distinto de `TraceFingerprint`; huella no disponible, infinita o no garantizada conservadora ⇒ `UNKNOWN`; `XLINE`/`RAY` sin interseccion analitica ⇒ `UNKNOWN`; ST-19 con huella `UNKNOWN` ⇒ E10; orden de plan con huella `UNKNOWN` ⇒ X-22; P7a enumera tambien las instancias sin cambio de mano; `Extents3d` solo ampliado por ocupaciones de modelo y declarado; dobles en Core | G6 |
| **T-M72** | Contexto de materializacion y `MaterializationContextReadSet` (`[V9-D05]`, `[V9-D19]`, `[V10-D04]`, `[V10-D05]`): el read-set contiene solo las observaciones que el productor o la huella consumen; cambiar un estado no consumido (por ejemplo `CLAYER` para un productor que fija la capa) **no** invalida; cambiar uno consumido (`DIMSTYLE` de una cota sin estilo con nombre; `TEXTSTYLE` de un `DBText`, si CT-50 lo demuestra) ⇒ E10 en PREPARE y excepcion y rollback en MUTATE; consumo no determinable ⇒ `UNKNOWN`; `Π(D)` y `Π(μ_k D)` con las mismas observaciones; productor que aplica el contexto de forma asimetrica ⇒ `UNKNOWN` (X-17a); ST-13 sigue fijando la presentacion exterior; ningun drawer existente cambia | G6 (consumo puro) y G7 (captura) |

### 12.3 Guardas del Plugin y de Application (nuevas)

| # | Propiedad protegida |
|---|---|
| G-M1 | El comando no contiene constantes `Kind*`, tipos `*KindHandler` concretos, comparaciones ni `switch` sobre `.Kind`, ni literales iguales a un kind |
| G-M2 | En los archivos del camino del espejo (comando, plan, reflectores, autoridad de planes, materializador) **no** aparece ninguna API que cambie el estado de vinculo o escriba el registro: `ProjectVariablesRegistry.TryWrite(`, `ProjectVariablesData.Write(`, `RegistryCommit`, `ProjectVariableMutationExecutor`, `ProjectVariableMutationPreflight`, `ProjectVariableIntent`, `SelectiveBindingIntent`, `SelectiveBindingIntentPreflight`, `LinkedPropertyReconciler`, `LinkedPropertyEditSession`, `LinkedPropertyEditState.Literal(`, `LinkedPropertyEditState.Create(`, `WithDesign(` ni equivalentes. La lectura efectiva queda **autorizada** (`ProjectVariablesRegistry.Read(`, `SelectiveEffectiveDesignResolver`) |
| G-M3 | Ningun `Matrix3d` ni `Autodesk.` en los archivos nuevos de Application |
| G-M4 | Exactamente una transaccion de escritura en MUTATE; `EnsureForPlan(` solo en PREPARE; nada tras el commit salvo mensajes (sin `PurgeUnreferenced(`, `SyncName(`); `Regen(` solo si §8.6 lo autoriza con evidencia |
| G-M5 | Sin `CloneDefinition(` en el comando (no es el camino de copia) |
| G-M6 | Sin `FindRackBlocks(`, `ScanEnvelopes(` ni `SelectAll` (sin busqueda de hermanas; V-VIEW-ALL es pura) |
| G-M7 | Una `GetSelection(` sin filtro de tipo; `CurrentUserCoordinateSystem` solo en la conversion de la linea |
| G-M8 | El materializador no contiene nombres de kind ni de propiedades de sistema |
| G-M9 | Mapa de invocaciones de builders y servicios de dibujo de `EditX` (Actualizar **e Insertar**) y del ejecutor en las vistas admitidas (linea base CT-26); si C2-2a y C2-2b comparten costura, demuestra que ambos caminos la siguen llamando; nace en G6 y sigue en G8 |
| G-M10 | Ningun literal de simetria de bloques en el codigo: un cambio de mano solo se acepta con `GeometrySymmetryResult` verificado por estado (§11.3) |
| G-M11 | El materializador de I-52 comprueba `MissingInstances` tras `CreateSystemBlock` y lanza; `LateralHeaderDrawer.cs` sin cambios |
| G-M12 | Si no se extrae un SNAPSHOT compartido: `RackDuplicarCommands.cs` identico al de la base y guardas de I-51 sin reapuntar |
| **G-M13** | Las tablas de clasificacion de miembros y de descriptores viven en Application, una por kind junto a su reflector; el Plugin no contiene clasificacion ni `switch` por kind |
| **G-M14** | Ningun tipo de I-52 en el namespace `RackCad.Application.Systems.Shared` (C-08 y C-10, baseline de main); I-52 no modifica `SharedFoundationInspection` ni las guardas de main o de I-49; los tipos del espejo viven en `RackCad.Application.Mirror` o junto a su kind |
| **G-M15** | Ningun componente del espejo acepta una diferencia de planes como evidencia de centro: el comparador y el registro no exponen API que derive `c` de planes |
| **G-M16** | La sonda no abre en escritura la base de datos del dibujo del usuario ni deja artefactos: trabaja en solo lectura o en una `Database` auxiliar privada; nunca evalua en la cache de biblioteca (G-M20); si cambia `HostApplicationServices.WorkingDatabase`, la restaura en un bloque que tambien se ejecuta ante excepcion |
| **G-M17** | Ningun archivo del espejo interpreta grafos de acciones de bloques dinamicos ni contiene una forma afin del centro |
| **G-M18** | Guardas de texto vigentes sobre los archivos de I-52 (la lista exacta se relee en el SHA de la reconciliacion, CT-46): en Application no aparecen `FormulaParser`, `DependencyGraph`, `RackPropertyReference` ni `rackProperty` (ni `ExpressionParser` mientras main lo prohiba); en el Plugin **y en la UI** (`RackCad.UI`, por ejemplo `RackCommandReference.cs`) no aparecen `ExpressionParser`, `FormulaParser`, `DependencyGraph`, `RackPropertyReference`, `rackProperty`, `PropertyValues` ni `SelectivePropertyValueDocument`, ni llamadas `LoadExisting(saved)`/`LoadExisting(document)`; ninguna segunda `class SelectiveEffectiveDesignResolver`; al integrar I-49, ningun literal ni conversion de unidades de longitud fuera de `RackCad.Application.Units` (`LengthUnitsAuthorityGuardTests`, `[V9-D21]`); con I-54 ya en `main @ ba497f1`, ademas T-GRD-01..T-GRD-07 de Custom Properties (`[V8-D19]`, `[V10-D13]`) y los censos por nombre de T-GRD-08, reapuntados con motivo por T-M16 sin debilitarlos (`[V10-D08]`) |
| **G-M19** | La sonda aplica parametros dinamicos por la misma costura que el materializador, o existe la prueba de equivalencia T-M56; ningun archivo del espejo contiene otra rutina de asignacion de `DynamicBlockReferenceProperty.Value` |
| **G-M20** | La sonda no escribe ni lee por reflexion la cache de `BlockLibraryImporter` (`[V9-D07]`). Rutas autorizadas en los archivos productivos del espejo: `BlockLibraryImporter.EnsureBlocks(auxDb, nombres)` (o la equivalente vigente) hacia una base auxiliar privada y el clonado de solo lectura desde el dibujo hacia esa base. Prohibido: reflexion sobre `BlockLibraryImporter` o acceso a su campo cacheado (`GetField`, `GetMethod`, `BindingFlags.NonPublic`, `AcquireLibrary`, `cachedLibrary`); mutacion directa de la cache; referencias temporales, parametros dinamicos o `ForWrite` sobre la `Database` cacheada. Tras clonar se verifica la presencia de todos los BTR requeridos y el retorno de `EnsureBlocks` no se usa como autoridad (§11.3 punto 14). La precedencia DRAWING FIRST del `DestinationEvaluationContext` no amplia ni reduce esta prohibicion: G-M20 sigue prohibiendo solo el acceso o la reflexion directos sobre `cachedLibrary` y su mutacion (`[V10-D09]`) |
| **G-M21** | Autoridad unica de extraccion de `VisualSignature` (`[V8-D16]`): un solo extractor resuelve variantes, fuentes tipadas, `VisibilityPath`, `SourceKind`, `PolylineGeneration` y `LinetypePatternContext` para todas las clases soportadas; ningun archivo del espejo resuelve por su cuenta las fuentes de una clase concreta ni duplica la tabla de variantes (§11.3 puntos 6.1..6.4) |
| **G-M22** | Ningun archivo de I-52 llama a las autoridades ni al commit fisico de Custom Properties de I-54 (`RackCustomPropertiesAuthority`, `CustomPropertiesCommit`, `CustomPropertiesExecutor`, `CustomPropertiesData`) ni contiene la clave `RACKCAD_CUSTOM_PROPERTIES`: la copia conserva CA-4 solo a traves de `Compose` y no toca CA-4b (`[V8-D19]`) |
| **G-M23** | Autoridad unica de huellas visuales (`[V9-D02]`, `[V10-D01]`..`[V10-D03]`): un solo productor de `VisualFootprintResult` para piezas, curvas, anotaciones, cotas y objetos de Model Space; ningun archivo del espejo usa `GeometrySymmetryResult` como huella, `TraceFingerprint` como `FootprintInputFingerprint` ni `Extents3d`/`GeometricExtents` sin la declaracion de §11.3 punto 5; ningun archivo dilata una huella por `LineWeight`, `LWDISPLAY`, grosores de CTB/STB, milimetros de papel o escala de trazado, ni construye regiones antes de transformar a WCS; ninguna huella de instancia del plan se resuelve con el contexto de la biblioteca; ningun consumidor trata una huella `UNKNOWN` como no superpuesta |

### 12.4 Suites y builds

Cada gate con codigo: Core completo en local (UI segun LC-UI en iteracion y completa si el gate toca
`tests/RackCad.UI.Tests`), CI verde sobre el SHA exacto. Candidato: Core y UI completas en local, builds Debug de UI y
Plugin con AutoCAD cerrado, CI 4/4 sobre el SHA exacto y cobertura del Candidato (WORKFLOW 4.5).

---

## 13. Validacion manual del Owner (AutoCAD 2025)

| # | Escenario |
|---|---|
| M-1 | Selectivo sin topes, frontal + planta, linea vertical: copia reflejada, textos legibles, numeracion regenerada, nombre `<base> - espejo` |
| M-2 | UCS girado 30° y origen desplazado: `P'` correcto en WCS |
| M-3 | Guardar, cerrar y reabrir: la copia sigue reflejada |
| M-4 | `RACKEDITAR` sobre la copia → Actualizar: el dibujo **no** cambia (C-2) |
| M-5 | Desde la copia, Insertar una vista admisible no reflejada (por ejemplo, la planta si solo se reflejo la frontal) y una lateral: salen del diseño reflejado y la admisible es espejo de la del origen |
| M-6 | `RACKBOMTOTAL`: la copia cotiza igual que el origen |
| M-7 | Selectivo con vinculos sin dependencia efectiva: la copia conserva `VariableId`; `ChangeValue` la redibuja **y sigue siendo espejo** del origen |
| M-8 | Dos racks (frontal + planta de cada uno): dos `NewRackId`, nombres `<base> - espejo` |
| M-9 | Fail-closed sin mutacion: lateral seleccionada, cama, MINSERT RackCad, escala no uniforme, fuente espejada, esquina, brazo C sencillo, Selectivo con topes, **Push Back con la configuracion por defecto (tope posterior activo)**, definicion con BASE movida, seccion invalida, cabecera de plantilla con Auto, **medio frente con `PalletTolerance` vinculada sin overrides**, **tarimas desbordadas**, **tarimas o parrillas dependientes sin `DEP-ROW` (casos A y B)**, **tarimas frontales de Push Back desbordadas**, **metadata exterior desconocida** |
| M-10 | UNDO: se registra que revierte y que ocurre con lo importado (evidencia; UNKNOWN hasta entonces) |
| M-11 | Dinamico frontal (salida y entrada) + planta |
| M-12 | Push Back compuesto con el tope posterior **inactivo en ambos lados** (`"(ninguno)"` en A y en B, o todas las celdas apagadas; el estado por defecto es activo y falla cerrado): frontales 0..3 + planta |
| M-13 | Cantilever frontal + planta |
| M-14 | Cabecera lateral + planta con diagonales explicitas y postes asimetricos |
| M-15 | Confirmacion visual, sobre piezas reales, de que las equivalencias visual-geometricas aceptadas por la sonda son correctas (O-3); la confirmacion no sustituye a la evidencia |
| M-16 | Anotaciones y cotas legibles, respetando la politica de `DimensionViews` de I-50 |
| M-17 | **Regresion de RACKDUPLICAR** (G4 edita `RackDuplicationPlan.cs`): multiples origenes y destinos, mensajes y fail-closed de I-51 |
| M-18 | Selectivo legado con frontal `Section = −1`: la copia sale con `Section = 0` y `RACKEDITAR` la trata igual |
| M-19 | Bloque de biblioteca ausente (con una biblioteca de prueba, nunca la del Owner): RACKMIRROR falla sin crear copias |
| M-20 | Efectos de infraestructura tras un fallo en PREPARE o MUTATE: que definiciones, capas o estilos importados quedan (evidencia) |
| **M-21** | Rack con una vista admisible **no seleccionada** que no conmuta (fixture de prueba): falla cerrado sin mutacion y el mensaje nombra la vista |
| **M-22** | Payload con miembro desconocido en el diseño (build de prueba): falla cerrado; con una clave desconocida no vacia en el sobre: **tambien** falla cerrado; con `CustomProperties`: el espejo la conserva |
| **M-23** | Rack convertido por el usuario en bloque dinamico o anonimo (BEDIT): falla cerrado con mensaje propio, no se ignora |
| **M-24** | Push Back recien creado con la configuracion por defecto: RACKMIRROR falla cerrado sin mutacion y el mensaje nombra el tope posterior y como desactivarlo |
| **M-25** | Selectivo de los casos A o B (§6.5.5): falla cerrado; con overrides que cubren la fila: la copia sigue siendo espejo tras `ChangeValue` |
| **M-26** | Seleccion con racks validos, un bloque de usuario no RackCad con escala no uniforme y un bloque de usuario que contiene racks: el espejo refleja los racks validos y avisa de los otros sin abortar |
| **M-27** | Sonda en AutoCAD 2025: bloques dinamicos con varias longitudes, anidados, una definicion local distinta de la biblioteca con el mismo nombre (se usa la local), piezas con capas, tipos de linea y escalas de tipo de linea por entidad (apagar, inutilizar e inutilizar en ventana capas de anidados no deja visible una sola mitad) y rendimiento en un rack grande; el dibujo no gana artefactos, `DBMOD` no cambia y la cache de biblioteca queda intacta |
| **M-28** | Payload legado con un miembro retirado: mensaje R-A (y tras RACKEDITAR → Actualizar el espejo funciona) y mensaje R-B (sin remedio) |
| **M-29** | Selectivo con `PalletTolerance` o `VerticalClearance` vinculadas: falla cerrado con mensaje; con overrides que fijan el ancho y alturas fijadas, la copia sigue siendo espejo tras `ChangeValue` |
| **M-30** | Cambiar la biblioteca configurada por otra con una pieza asimetrica o con degradado: el mismo rack pasa a fallar cerrado con mensaje atribuible; volver a la biblioteca original restituye el resultado |
| **M-31** | Seleccion mixta con un rack reflejable y otro que falla (por ejemplo, Push Back con el tope posterior por defecto): RACKMIRROR no refleja ninguno y nombra el rack que fallo (todo-o-nada) |
| **M-32** | Pieza de prueba con homologas de fuentes distintas que hoy se ven iguales (`ByLayer` frente a color explicito del mismo valor, o anidados en capas distintas): RACKMIRROR falla cerrado con mensaje atribuible; cambiar despues el color de la capa o inutilizar la capa del anidado muestra por que la regla lo exige |
| **M-33** | Rack fuente cuya referencia tiene presentacion no por defecto (color explicito, tipo de linea y escala de tipo de linea, grosor, transparencia, estilo de trazado en un dibujo STB, `Visible`): la copia conserva cada propiedad (ST-13); en un dibujo CTB el estilo de trazado sigue al color sin asignarse aparte |
| **M-34** | Rack fuente con XCLIP u otro estado de presentacion no transportable: RACKMIRROR falla cerrado antes de mutar, con mensaje atribuible (ST-18) y sin aviso sustitutivo |
| **M-35** | Orden en Model Space: dos racks seleccionados que se superponen, con DRAWORDER explicito, conservan su orden relativo en las copias; un sombreado o wipeout **posterior** a la fuente que se superpone con el destino de la copia hace fallar el comando en PREPARE, sin mutacion semantica, tanto si esta seleccionado (e ignorado) como si no; una copia con relleno que cae sobre otra fuente seleccionada posterior tambien falla; una copia sobre su propia fuente queda encima (ST-19); con dos fuentes seleccionadas cuyo orden difiere en pantalla y en trazado y copias que se superponen, el comando falla; un `XLINE` o `RAY` posterior a la fuente, aunque este lejos, hace fallar el comando (L-34); dos objetos que solo se tocan por el grosor de linea visible no tienen garantia de orden (L-37) |
| **M-36** | Piezas de prueba fuera de la politica o con orden de plan invertido (polilineas con `Plinegen` distinto y tipo de linea discontinuo, `MInsertBlock` anidado, instancias con rellenos solapados cuyo orden se invertiria) y dibujo STB con estilos con nombre: RACKMIRROR falla cerrado con mensaje atribuible o, si el contexto se reproduce, refleja de forma equivalente |
| **M-37** | Contexto de materializacion: con `CLAYER`, `CECOLOR`, `CELTSCALE` o `DIMSTYLE` distintos de los que habia al dibujar la fuente, la copia se regenera como Actualizar (piezas internas, anotaciones y cotas con las observaciones que consume cada productor) y la presentacion exterior de la referencia sigue siendo la de la fuente (ST-13); durante la peticion de la linea, cambiar un estado que el productor consume (por ejemplo `DIMSTYLE` con cotas sin estilo con nombre) hace fallar sin mutacion semantica (PRE-17), y cambiar uno que no consume no lo hace (read-set, segun caracterice CT-50) |

---

## 14. Proceso y gates

### 14.1 Secuencia normativa (sustituye a la de V9)

```text
Coordinador + Arquitecto de acuerdo sobre V10 (misma version, exact-SHA)
→ rebase / reconciliacion sobre el main vigente         (solo docs; hoy main = dad4e77 con I-53 E1, I-53S
                                                          E2, I-54 e I-53D E3; ADR 0035, 0037 y 0039 en main;
                                                          §15.1)
→ freeze por SHA exacto + re-check exact-SHA            (Coordinador y Arquitecto)
     si el rebase cambia contenido normativo material → vuelve a revision
→ Owner O-1                                              (envolvente maxima de §1.3, todo-o-nada y dependencia de la
                                                          biblioteca efectiva; §1.4 informa y no es contrato)
→ G3 characterization                                    (ADR-0036 NO es precondicion; sonda solo fuera de src/, en acad.exe)
     si G3 contradice materialmente V10 o reduce materialmente la envolvente → Proposal V11 (y O-1 se reevalua)
     si no                                               → Owner acepta ADR-0036 (O-4)
→ G4 implementation                                      (ADR-0036 aceptado ES precondicion)
```

### 14.2 Tabla de gates

Cada gate declara entrada, archivos permitidos, RED esperado, PASS requerido, parada y evidencia por SHA exacto. Ningun
gate arranca sin la orden explicita del Coordinador y sin el preflight de paralelas (§15.3). **Cualquier contradiccion
de una caracterizacion o de una prueba con el contrato congelado ⇒ STOP → Proposal V11**; nunca un parche silencioso.

| Gate | Entrada | Archivos permitidos | RED esperado | PASS requerido | Parada | Evidencia |
|---|---|---|---|---|---|---|
| **G2** (este) | revision de V9 | `docs/**` | — | consenso de Coordinador y Arquitecto sobre la misma version (V10); despues, rebase/reconciliacion sobre el main vigente, freeze por SHA, re-check normativo y **O-1** del Owner sobre la envolvente de §1.3; contrato reescrito; hallazgos laterales en `ideas-futuras.md` | desacuerdo material ⇒ Proposal V11 | SHA acordado, SHA de freeze + CI |
| **G3** Characterization | freeze de G2 + O-1 aceptada (**sin** ADR aceptado) | `tests/RackCad.Tests/**` (caracterizaciones y fixtures nuevos); `tests/RackCad.UI.Tests/**` si una caracterizacion lo exige; `docs/automation/evidence/**`; la viabilidad de la sonda se demuestra **fuera de `src/`** (`tools/`, harness externo o evidencia del Owner) **en `acad.exe` / AutoCAD 2025**, en la ubicacion que fije el Coordinador al abrir G3; **ninguna sonda productiva en `src/`** | ninguno: CT-01..CT-50 pasan sobre produccion intacta | Core completo (UI si se toco); **viabilidad visual-geometrica de la sonda en `acad.exe` (CT-36: politica cerrada de clases y variantes con inventario cerrado, huellas visuales de ocupacion de modelo en el contexto destino (DRAWING FIRST, colisiones de nombres) y su frescura tras `EnsureForPlan`, `SourceKind`, `Plinegen` y `Closed`, fuentes visuales simbolicas tipadas, contexto de la base auxiliar STB y CTB, `VisibilityPath` con estados de capas, ancestros y anotativos, `LinetypeScale` y contexto de patron, fase, segmentacion y multiplicidad, orden visual dentro de la pieza, SOLID/TRACE/HATCH con `HatchLoopTypes`, `applied` y punto de observacion, valores efectivos, `Origin` y transformacion efectiva, ruta anidada estable, degradados, clonado por `EnsureBlocks` con la cache intacta, identidad SHA-256, `DBMOD`/`HANDSEED`/UNDO/`WorkingDatabase`) antes que `PlanPlacementEvidence` (CT-06)**; corpus indicativo con la huella de la biblioteca (CT-47); orden visual de plan por kind y vista con pares invertidos y huellas (CT-48); presentacion de la referencia fuente con inventario cerrado, huellas y orden en Model Space frente a todo objeto posterior, con ordenes de pantalla y de trazado contradictorios y `XLINE`/`RAY` (CT-49); read-set del contexto de materializacion (CT-50); cierre transitivo sobre el contrato real, enums, retirados con RM-7 y guardas cruzadas (CT-31, CT-32, CT-42, CT-46); campos start/end de cabecera (CT-45); tabla de `c`; auditoria de anclajes por pieza (CT-17); semantica observable de I-51; decodificacion de secciones e historia de `View`; lineas base de store; topes, con el tope posterior de Push Back activo por defecto y `OffCells` latentes (CT-15, CT-43); estabilidad 9b incondicional con casos A/B, cotas, cambio de gobernante, BOM, firmas de estado y DEP-10a..c (CT-27, CT-28, CT-37, CT-38, CT-44); S-14 asociado; enumeracion `A_k`; tarimas del Selectivo y de Push Back (CT-20); precedencia de fuente (CT-39); read-set (CT-40); costuras de Actualizar e Insertar (CT-26); baseline de I-53 y clasificacion de I-53S (CT-41); fuentes dinamicas/anonimas; V-RT-STORE separado de C-2; **regla de cierre: ninguna fila `G3_PENDING` ni `EvidenceStatus` implicito** (§7.0) | contradiccion material con V10; sonda no viable, sin paridad, sin punto de observacion o sin contexto reproducible; clases o variantes reales no soportadas; huellas visuales no garantizables o contexto de evaluacion destino no simulable en clases enteras; productor asimetrico del contexto de materializacion o consumo no determinable; biblioteca real que reduce materialmente la envolvente mas alla de O-1; canonicalizacion materialmente ambigua ⇒ **STOP → Proposal V11** | SHA + conteos + CI + registros de evidencia con la huella de la biblioteca. Tras PASS: se pide **O-4** |
| **G4** Geometria y nucleo neutral | G3 PASS **y ADR-0036 aceptado por el Owner** | `src/RackCad.Application/Geometry/*`; nucleo neutral y fachada en `src/RackCad.Application/Persistence/` (incluye `RackDuplicationPlan.cs`, **con aviso previo a I-49**); pruebas | T-M01..T-M05, T-M21 y T-M41 (comparadores puros) en rojo sobre andamiaje | verdes; T1–T15 de I-51 y CT-16 identicos (textos, orden, avisos, errores); `RackDuplicarCommands.cs` sin cambios; tolerancia de escala fijada y registrada; **sin cambio observable de RACKDUPLICAR** | cambio observable de RACKDUPLICAR | SHA + conteos + CI |
| **G5** Reflectores y representabilidad | G4 | contrato, registros y tablas de clasificacion en `src/RackCad.Application/Mirror/`; reflector por kind en `src/RackCad.Application/Systems/<Kind>/` y `RackFrames/` (nunca en `Systems/Shared`); contrato de datos `GeometrySymmetryResult` (valores efectivos, variantes y firma visual tipada) y su consumo puro; declaracion de invariancia de metadata vacia; allowlist de portadores; registro de retirados con RM-7; autoridad V-DEP con DEP-09 y DEP-10a..c; guarda sobre el contrato de serializacion; pruebas. Divisible en G5a Selectivo, G5b Dinamico y Push Back, G5c Cantilever y cabecera | T-M06..T-M11, T-M18 (portador), T-M22, T-M24, T-M25, T-M28, T-M31, T-M32, T-M34, T-M35, T-M36, T-M38, T-M39, T-M42..T-M45, T-M47, T-M49..T-M52, T-M58, G-M18 y G-M22 (archivos de Application) en rojo | verdes; sustrato real; topes Selectivo y Push Back `UK`; H-06 `UK`; H-10..H-12 clasificadas; obligaciones 9 y 9b; V-DEP; V-META del payload, exterior, retirados, enums y colisiones; cierre transitivo con enums; guardas de texto vigentes; sin `G3_PENDING`; `WithDesign` ausente | una fila `R` o `RN` resulta no equivalente ⇒ STOP → Proposal V11 | SHA por subgate + CI |
| **G6** Planes, materializador y **C-2** | G5 | autoridades de plan por kind en Application; `SystemBlockWriter.cs` (`CreateInTransaction`); materializador nuevo en `src/RackCad.Plugin/Systems/Shared/`; **sonda visual-geometrica productiva en el Plugin** si la division final la situa aqui (si no, en G7), con costura unica de parametros compartida con el materializador o prueba T-M56; pruebas Core y `tests/RackCad.UI.Tests/**` para C-2 | T-M12..T-M14, **T-M17**, **T-M17b**, T-M23, T-M26, T-M27, **T-M29**, **T-M30**, T-M33, T-M40, T-M48, T-M53..T-M57, T-M59..T-M67, T-M70 y T-M71 (si la sonda nace aqui), T-M72 (consumo puro), G-M9, G-M16, G-M17 y G-M19..G-M23 en rojo | conmutacion y V-VIEW-ALL verdes con resultados visual-geometricos y orden visual de plan sobre huellas; `MissingInstances` = fallo duro; **C2-1, C2-2a, C2-2b, C2-3 y C2-4 GREEN en CI** (Core y UI; C2-4 sobre el arbol combinado con I-53S, ya integrada en main, `[V8-D17]`, y con I-53D, integrada en main antes del Candidato (`dad4e77`), incluidos T-M17 y T-M17b del Dinamico, `[V10-D15]`, `[V10-D23]`) con G-M9, nombre logico, estado efectivo, `View` y `Section` canonicas; builders y drawers sin cambio de comportamiento; Application sin `Autodesk.*`. **G6 no cierra sin C-2** | necesitar cambiar un builder o un drawer existente; divergencia de C-2 ⇒ decidir C-1 con Coordinador y Arquitecto antes de cerrar G6 | SHA + CI + build Debug de Plugin |
| **G7** Comando RACKMIRROR | G6 cerrado (**C-2 verde**, incluido C2-2b) | `src/RackCad.Plugin/RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; sonda visual-geometrica productiva si no nacio en G6; archivos de guardas; `SelectiveEditorOpenTests.cs` (censo `B + 1`; hoy 35 → 36), `CustomPropertiesCommandGuardTests.cs` y `CustomPropertiesHelpCensusTests.cs` (T-GRD-08 de I-54, en main, reapuntados con motivo y sin debilitar: `RACKMIRROR` por nombre, entrada de ayuda sin alias y alias verificado solo si no esta vacio, `[V10-D08]`); `src/RackCad.UI/RackCommandReference.cs` (entrada de `RACKMIRROR` sin alias). `RackDuplicarCommands.cs` **solo** si se decide extraer un SNAPSHOT compartido (§5.2) | T-M15, T-M16, T-M37, T-M41 (MUTATE), T-M46 (captura), T-M68 y T-M69, T-M72 (captura), T-M53..T-M57, T-M59..T-M67, T-M70 y T-M71 (si la sonda nace aqui), G-M1..G-M8 y G-M10..G-M23 en rojo por violacion temporal | guardas verdes; censo `B + 1` con `B` re-medido (`[V9-D06]`), ayuda +1 entrada y registros de alias +0 (`[V10-D08]`); presentacion de la referencia fuente (ST-13, ST-18), orden en Model Space frente a todo objeto posterior con frescura tras `EnsureForPlan` (ST-19, PRE-18) y read-set del contexto de materializacion (PRE-17) verdes; build Debug de Plugin; **rendimiento de la sonda medido (R-24)**; mensajes R-A/R-B fijados (T-M50); sin extraccion, archivos y guardas de RACKDUPLICAR intactos (G-M12) | C-2 no verde ⇒ G7 no arranca; debilitar una guarda de I-51, de main (incluidas las de I-53) o de texto (G-M18) | SHA + conteos + CI |
| **G8** Cruces y continuidad | G7 | pruebas cruzadas; continuidad de G-M9; produccion de `RACKEDITAR` **solo** si se activo C-1 con orden del Coordinador | T-M18 (cruce final), T-M19 y T-M20 en rojo cuando apliquen | cruces verdes con lo integrado (I-49 si integra, I-50, I-53, I-53S, I-54 con T-GRD-01..T-GRD-08 e I-53D); reconciliacion final con autoridades integradas | un cruce exige tocar §20.3 | SHA + CI |
| **G9** Conformidad, Candidato y Owner | G8 | ninguno de produccion salvo correcciones | — | rebase final si `main` avanzo; Core y UI locales; builds Debug; CI 4/4 exacto; cobertura; M-1..M-37 (M-17 incluido); O-3 confirmado | cualquier fallo ⇒ vuelta al gate que corresponda | SHA Candidato + corridas + veredicto del Owner |
| **G10** Documentacion e integracion | G9 | `docs/**`, `README.md` | — | WORKFLOW 4.5: cierre documental, merge `--no-ff`, CI del `MERGE_SHA`, cobertura, limpieza | CI post-merge rojo ⇒ correccion en la rama | SHAs de cierre y merge |

---

## 15. Coordinacion con iniciativas paralelas

### 15.1 main y paralelas observadas en el preflight de V10

`git fetch --all --prune` el 2026-09-13, antes de redactar V10 y de nuevo antes de publicarla (tips del re-fetch previo
al commit):

```text
origin/main                                       = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093 (merge de I-53D E3 sobre
                                                     ba497f1, que integraba I-54 sobre 104ef3a; ancla de las citas de
                                                     V10: 1b091be)
I-52 feature/rackmirror-espejo-semantico           = deb08cdb81817e9e1fafc3500e542a0ac1c2c886 (V9; base 46fcac2; sin rebase)
I-49 architecture/motor-expresiones-parametricas   = 5a3714f06aa695e22db0f16455a1ea8dce5195b6 (Amendment A2, solo docs,
                                                     sobre el G6 productivo 5f969cc y el Consensus Freeze 8ed15f7; sin
                                                     integrar; ADR-0040 aceptado)
I-55 feature/creacion-de-vistas                    = cfdb702a83554df6472eddbdc59cf43a2ba7bd94 (rama nueva, View
                                                     Placement & Projection: reclamo 6976262, bootstrap 5b5c4f6, G1
                                                     2d0f9bf, G1-CLOSE 717a38d y G1.1 cfdb702, rebasada sobre
                                                     dad4e77; solo docs; sin integrar)
I-53D feature/cabeceras-multidestino-dinamico      = integrada en main (dad4e77: G7 a57bd50 y cierre documental
                                                     90d5c5e); rama remota retirada
I-54 architecture/propiedades-personalizadas       = integrada en main (ba497f1: Candidato 02987bd y cierre documental
                                                     d3cc843); rama remota retirada
I-53 feature/cabeceras-configurables-multidestino  = integrada en main (1b091be); rama remota retirada
I-53S feature/cabeceras-multidestino-selectivo     = integrada en main (104ef3a: G5 e528ef2 y cierre documental d043afb);
                                                     rama remota retirada
I-50                                               = integrada en main; rama remota retirada
Censo de [CommandMethod(                          = main dad4e77: 35 · I-55: 35 · I-49: 33 · I-52: 33
ADR 0035..0040                                     = 0035, 0037 y 0039 aceptados en main; 0036 propuesto solo en I-52;
                                                     0038 reemplazado y 0040 aceptado en I-49; sin colision
Biblioteca de evidencia                            = SHA-256 b4ca2248…d560e8, sin cambios
Ramas nuevas                                       = feature/creacion-de-vistas (I-55), solo docs
```

**Hechos de la reconciliacion con main (`f8deb67` → `1b091be`).** El merge de I-53 E1 añade 48 archivos (+11 657 / −4).
En `src/` son +2 431 / −1, y la unica linea retirada es el comentario XML de `DynamicRackModule.IsManualOverride`. No toca
UI, Plugin, DTO, persistencia ni dibujo, y nada lo llama. ADR-0037 queda **aceptado en main** y el indice de ADR de main
lista 0035 y 0037.

Lineas productivas citadas por I-52 que I-53 desplazo (verificadas en `1b091be`):

| Archivo | Cambio de I-53 | Cita en V5 (`f8deb67`) | Cita desde V6 (`1b091be`) |
|---|---|---|---|
| `SelectivePostGeometry.cs` | +9 lineas tras `:50` (`PostPeralteFor`) | `:54-58`, `:115-121` | `:63-67`, `:124-130` (`:32-48` y `:39` no cambian) |
| `SelectiveEditorState.cs` | +1 `using` tras `:3`; +74 lineas tras `:1245` | `:744`, `:744-745`, `:745` | `:745`, `:745-746`, `:746` |
| `SelectiveCabeceraHeightReview.cs` | +64 lineas tras `:176` | — | — |
| `DynamicRackModule.cs` | comentario XML de `IsManualOverride` (`:28` → `:28-31`) | — | — |
| `HeaderBatchContractTests.cs` | ahora en main, contenido igual al de `4e00a27` | `:476-488` @ `4e00a27` | C-08 en `:461-488` (dos pruebas: referencias del ensamblado y tipos `Header*` en `Roots`) y C-10 en `:492-563` @ `1b091be` (`[V7-D18]`) |

El resto de los archivos productivos que cita I-52 no cambia entre `f8deb67` y `1b091be` (por ejemplo, `LateralHeaderDrawer.cs`,
`BlockLibraryImporter.cs`, `SelectiveFrontalBuilder.cs`, `SelectiveGeometryResolver.cs`, `PushBackRearTope.cs`,
`DynamicSystemPlantaBuilder.cs`, `BracingPanelMemberBuilder.cs`, `RackProjectStore.cs`). Las citas nuevas de V8
(`BlockLibraryImporter.cs:37-119` y `:127-168`; `LateralHeaderDrawer.cs:36-76` y `:130-161`;
`SelectiveFrontalBuilder.cs:82-152`) se leyeron en `1b091be`.

**Hechos del avance de main durante V8 (`1b091be` → `104ef3a`).** El merge de I-53S E2 lleva el G5 `e528ef2` sin cambios
(`git diff e528ef2 104ef3a -- src tests` vacio) y el cierre documental `d043afb` (HANDOFF, ROADMAP, registro de I-53,
`ideas-futuras.md` y contrato de I-53S). En `src/` y `tests/` solo cambian `RackSelectiveWindow.xaml` y `.xaml.cs` y
pruebas de UI del Selectivo; el indice de ADR de main sigue listando 0035 y 0037. Las citas de V8 siguen ancladas en
`1b091be`; las que I-53S desplazo son estas y se re-anclan en el rebase posterior al consenso:

| Archivo | Cita en V8 (`1b091be`) | Posicion en `104ef3a` |
|---|---|---|
| `RackSelectiveWindow.xaml.cs` | `:2145-2153` (el editor exige insertar primero la frontal, §3.6) | `:2560-2568` |
| `RackSelectiveWindow.xaml.cs` | `:2561-2565` (control `< 0` del editor, §6.5.5) | `:2982-2986` |
| `RackSelectiveWindow.xaml.cs` | `:185-188` (`SystemToInsert` y `DesignToInsert`, C2-2a) | sin cambio |

**Hechos del avance de main tras V9 (`104ef3a` → `ba497f1`).** El merge de I-54 (`ba497f1`, primer padre `104ef3a`,
segundo `d3cc843`) lleva, sin rebase final, la produccion del Candidato `02987bd` y el cierre documental `d3cc843`
(`git diff d3cc843 ba497f1` vacio). En `src/` añade el nucleo de Custom Properties en
`RackCad.Application.CustomProperties` y sus stores en `Persistence`, `CustomPropertiesData.cs`,
`CustomPropertiesExecutor.cs` y `RackPropiedadesCommands.cs` en el Plugin, y `RackCustomPropertiesWindow` y una entrada
en `RackCommandReference.cs` en la UI; y modifica `RackEmbedComposer.cs` (herencia y normalizacion de
`CustomProperties`, regla F) y `RackEmbedDocument.cs` (miembro declarado). En `tests/` añade T-GRD-01..T-GRD-08 y
reapunta `SelectiveEditorOpenTests.cs` (censo 35) y `WindowCensusGuardTests.cs` (30). ADR-0039 queda aceptado en main.
Esos cambios del sobre son exactamente el contrato de CA-4 que V9 ya preveia («miembro declarado del sobre cuando I-54
este integrada»; regla F ⇒ E8): ninguna autoridad de I-52 cambia y no hay STOP (`[V10-D13]`). Citas de V10 que I-54
desplazo, verificadas en `ba497f1` (el resto de las citas ancladas en `1b091be` conserva contenido y posicion, salvo las
de I-53S de la tabla anterior; `[V10-D22]`):

| Archivo | Cita en V9 (`1b091be`) | Posicion en `ba497f1` |
|---|---|---|
| `RackEmbedComposer.cs` | `:21-35` (`Compose` hereda los portadores, X-02) | `:43-60`, con la regla F en `:46` y la herencia de `CustomProperties` en `:57` |
| `RackEmbedComposer.cs` | `:26` (version de escritura no degradada, CA-1) | `:50` |
| `RackEmbedComposer.cs` | `:33` (`ExtensionData` copiado, §6.4, X-02b) | `:58` |
| `RackEmbedDocument.cs` | `:35-64` (miembros y `ExtensionData` del sobre, X-02b) | `:35-75`, con `CustomProperties` en `:67` |
| `RackEmbedDocument.cs` | `:70-73` (opciones de serializacion del sobre, §6.4) | `:81-84` |

**Hechos del avance de main durante la redaccion de V10 (`ba497f1` → `dad4e77`).** El merge de I-53D E3 (`dad4e77`,
primer padre `ba497f1`, segundo `90d5c5e`) lleva el G7 `a57bd50` sin cambios y el cierre documental `90d5c5e` (HANDOFF,
ROADMAP, registro de I-53, `ideas-futuras.md` y contrato de I-53D): `git diff 90d5c5e dad4e77` vacio y `a57bd50` →
`90d5c5e` solo en docs. En `src/` solo cambian `DynamicEditorDesignAssembler.cs`, `RackDynamicSystemWindow.xaml` y
`RackDynamicSystemWindow.xaml.cs`; en `tests/`, pruebas del Dinamico y de Push Back, con las guardas `D34_*` de
`DynamicHeaderBatchSeamGuardTests.cs` sobre la ventana del Dinamico. `LoadExisting`, `DesignToInsert` y `SystemToInsert`
de la ventana son identicos a `ba497f1`; el censo de `[CommandMethod(` sigue en 35; el indice de ADR de main sigue
listando 0035, 0037 y 0039; ni las guardas de I-54 ni `RackCommandReference.cs` cambian. Ninguna autoridad de I-52
cambia y no hay STOP (`[V10-D15]`, `[V10-D23]`). Cita de V10 que I-53D desplazo, verificada en `dad4e77` (ningun otro
rango citado cambia de contenido ni de posicion respecto de `ba497f1`):

| Archivo | Cita en V10 (`1b091be`) | Posicion en `dad4e77` |
|---|---|---|
| `RackDynamicSystemWindow.xaml.cs` | `:2947-2955` (vista lateral primaria obligatoria, D-15) | `:3394-3402` |

**Cambios de paralelas desde la publicacion de V9.**

| Paralela | Cambio | Relacion con I-52 |
|---|---|---|
| **main `dad4e77`: I-53D E3 integrada** | durante la redaccion de V10: merge `--no-ff` del cierre documental `90d5c5e` (solo docs sobre el G7 `a57bd50`) sobre `ba497f1`; rama remota retirada tras la integracion | clasificacion de `[V10-D15]`: contrato NON-MATERIAL; coordinacion MATERIAL; C2-4 del Dinamico MATERIAL / CHANGED. Como integro antes del Candidato, CT-26, C2-4, T-M30, T-M17, T-M17b y las guardas relacionadas corren sobre el arbol combinado tras el rebase; una cita desplazada (tabla anterior); I-53D sale del protocolo de ramas paralelas (`[V10-D23]`) |
| **main `ba497f1`: I-54 integrada** | merge `--no-ff` de I-54 con el Candidato `02987bd` y el cierre documental `d3cc843`, sin rebase final porque main no avanzo desde `104ef3a`; rama remota retirada tras la limpieza | contrato de I-52 **NON-MATERIAL**; coordinacion **MATERIAL**: `B = 35`; T-GRD-01..T-GRD-08 vigentes en main (T-GRD-02 7 → 8 y T-GRD-08 reapuntados con motivo por el gate que corresponda); `RackCommandReference.cs` con 15 pares; CA-4 miembro declarado; citas desplazadas (tabla anterior). I-54 sale del protocolo de ramas paralelas (`[V10-D13]`, `[V10-D22]`) |
| **I-53D `a57bd50`** | rebase sobre `ba497f1`: reclamo `0b091d6`, bootstrap `c560678` y G7 `a57bd50`; `git range-diff` identico para el reclamo y el G7, y cambio solo de contexto en la fila de ROADMAP del bootstrap; mismo patch-id de `src` y `tests` que `d280197` | clasificacion de `[V10-D15]`: contrato NON-MATERIAL; coordinacion MATERIAL; C2-4 del Dinamico MATERIAL / CHANGED; T-M17 y T-M17b se suman a CT-26, C2-4 y T-M30 sobre el arbol combinado si integra antes del Candidato |
| **I-49 `5a3714f`** | G6 `5f969cc` sin cambios desde V9; durante la redaccion de V10, Amendment A2 de V6, solo docs (`docs/initiatives/I-49-proposal-v6-amendment-a2-exact-key-qualifier.md`): sintaxis textual del cualificador de identidad de `VariableId` y validez neutral de su clave; anuncia un ADR de reemplazo de D6 y D7 de ADR-0040, sin numero ni archivo, pendiente de la revision del Arquitecto y del Owner; G6 NOT CLOSED, G7 BLOCKED y PRODUCTION CHANGES = NONE | contrato de I-52 **NON-MATERIAL**: A2 declara sin cambio `PlanReadSet`, las unidades y la semantica del evaluador, e I-52 no usa la sintaxis del cualificador; coordinacion: el ADR de reemplazo tomara un numero nuevo, que el censo de §16 verifica frente a 0036 (`[V10-D14]`, `[V10-D23]`) |
| **I-55 `cfdb702`** (rama nueva `feature/creacion-de-vistas`) | durante la redaccion de V10: reclamo, bootstrap, G1 Discovery y G1-CLOSE sobre `ba497f1` (`24bb9ca`, `5f774b4`, `cf207b9`, `96f6444`), rebasados sobre `dad4e77` (`6976262`, `5b5c4f6`, `2d0f9bf`, `717a38d`; punta previa en el tag `archive/i-55-creacion-de-vistas-pre-rebase-96f6444`) y G1.1 `cfdb702`; solo docs (contrato, Discovery, registro de decisiones y fila de ROADMAP). G1.1 corrige el framing a **View Placement & Projection**: ID17 (crear un rack desde cualquier vista soportada), ID18 (varias vistas de un rack en un flujo) e ID19 (varios racks existentes proyectados con una transformacion comun, cada uno con su `RackId`), sobre una foundation de preparacion de vistas antes de materializar; G2 autorizado solo como documentacion e implementacion bloqueada | contrato de I-52 **NON-MATERIAL**: mide 0 archivos productivos compartidos con I-52, excluye el territorio de I-52 y se declara compatible con I-52 sin depender de ella. Coordinacion vigilada: su foundation y su proyeccion multi-rack cruzan conceptualmente la autoridad pura de planes por vista (§8.1, C2-1), el materializador de G6, el contexto de materializacion (§8.7) y la colocacion `P'` de I-52; si su G2 fija una autoridad comun o toca rutas de materializacion, `RackEmbedComposer.Compose`, comandos o `RackCommandReference.cs`, se reporta y se re-evalua antes de fijar archivos (R-45, §15.3, `[V10-D23]`) |

**Cambios de paralelas entre la publicacion de V8 y la de V9 (registrados por V9; I-54 e I-53D integraron despues).**

| Paralela | Cambio | Relacion con I-52 |
|---|---|---|
| **I-54 G7B (`18cf2dc`)** | productivo y sin integrar, sobre G7A `5c16ae8`: `src/RackCad.Plugin/RackPropiedadesCommands.cs` (`RACKPROPIEDADES` y su unico alias `RPR`), `src/RackCad.UI/RackCustomPropertiesWindow.xaml` y `.xaml.cs` (arquetipo C), una entrada en `src/RackCad.UI/RackCommandReference.cs`, `GUARDA_EL_CENSO_DE_COMANDOS_NO_CAMBIA` reapuntada de 33 a 35 (`SelectiveEditorOpenTests.cs`), censo de ventanas de 29 a 30 (`WindowCensusGuardTests.cs`) y T-GRD-08: censo de comandos por NOMBRE (33 de apertura mas `RACKPROPIEDADES` y `RPR`, `CustomPropertiesCommandGuardTests.cs`) y censo de ayuda por PARES (14 de apertura mas `RACKPROPIEDADES/RPR`, `CustomPropertiesHelpCensusTests.cs`). No modifica `RackEmbedDocument`, `RackEmbedComposer`, el restamp, `RackBlockFinder`, `KindHandlers`, `BlockLibraryImporter`, `LateralHeaderDrawer`, `SystemBlockWriter`, `ProjectVariables` ni `CustomPropertiesData`/`CustomPropertiesExecutor` | materialidad arquitectonica para I-52 = **NON-MATERIAL**; materialidad de coordinacion = **MATERIAL**; censo de comandos = **MATERIAL**; `RackCommandReference.cs` = **MATERIAL TO INTEGRATION ORDER**. Si I-54 integra antes de I-52 G7: re-medir `B` (previsiblemente 35), serializar `RackCommandReference.cs` y `SelectiveEditorOpenTests.cs` y reapuntar con motivo los censos por nombre de T-GRD-08 añadiendo `RACKMIRROR` sin alias, sin debilitarlos (T-M16); RACKMIRROR no abre ventana, asi que el censo de ventanas no cambia. No necesita Proposal nueva por si mismo (`[V9-D06]`, `[V9-D11]`, `[V9-D17]`) |
| **I-49 G6 (`5f969cc`)** | productivo y sin integrar, sobre el Consensus Freeze `8ed15f7` (V6, A1 y ADR-0040): nucleo semantico del motor de expresiones en `src/RackCad.Application/Expressions/` (A1 con `MaxSyntacticTokens`, `BoundExpression`, `ExpressionBinder`, `ExpressionEvaluator` de un arbol, `FunctionRegistry`, `SymbolTable`, `ExpressionFormatter`) y autoridad neutral de unidades `src/RackCad.Application/Units/LengthUnits.cs`, con `StructuralSectionUnits.cs` como alias de API y valores identicos; pruebas nuevas, `ExpressionCoreGuardTests` y `ExpressionParserGuardTests` extendidas y `LengthUnitsAuthorityGuardTests` nueva sobre todo `src/`. Cero cambios en ProjectVariables, Persistence, Systems, Domain, UI, Plugin, tools, assets y docs; sin `PlanReadSet`, dominio aplicado ni evaluacion del registro | contrato de I-52 = **NON-MATERIAL**: I-52 solo consume de I-49 el `PlanReadSet`, el dominio del consumidor y `expression` si integra (RS-3, PV-6), y nada de eso existe todavia; ADR-0040 sigue siendo la autoridad vigente (ADR-0038 reemplazado; D19 `PlanReadSet` y dominio del consumidor sin cambio material); coordinacion = **MATERIAL**: CT-46 y G-M18 releen al reconciliar `LengthUnitsAuthorityGuardTests` y las guardas del nucleo, e I-52 no declara literales ni conversiones de unidades de longitud ni nombra el nucleo (`[V9-D21]`) |
| **I-54 G7-CLOSE (`02987bd`)** | solo docs: registro y contrato de I-54 sincronizados con G7B (censos, pruebas y colision); G8 NOT AUTHORIZED; sin Candidato ni integracion | **NON-MATERIAL** (`[V9-D21]`) |
| **I-53D G7 (`d280197`)** | productivo y sin integrar, sobre el bootstrap `aa0f817`: `src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml` y `.xaml.cs` conectan la fundacion Dinamica de I-53 E1 (`DynamicHeaderBatch`, `DynamicHeaderBatchState`, `DynamicModuleTargets` y `DynamicRackRebuild`) con el panel «Reutilizar cabecera»; `src/RackCad.Application/Systems/Dynamic/DynamicEditorDesignAssembler.cs` pierde el par ordinal `SnapshotHeaderFondos`/`RestoreHeaderFondos`; presets «Personalizada N» retirados; pruebas de Core y de UI. Cero cambios en `Systems.Shared`, `RackModuleReconciliation`, DTO, persistencia, Domain, Plugin, Selectivo y Push Back productivo | clasificacion exacta (`[V9-D22]`): contrato de I-52 = **NON-MATERIAL**; coordinacion = **MATERIAL**; baseline de C2-4 del Dinamico = **MATERIAL / CHANGED** (`DynamicEditorDesignAssembler` es productor de C2-4 y la ventana pasa ahora por el lote y la reconstruccion). Si integra antes del Candidato de I-52: tras el rebase, CT-26, C2-4 del Dinamico, T-M30 y las guardas relacionadas sobre el arbol combinado condicionan G6/G7; si integra despues: reportar al Coordinador y re-medir contra un main que contenga I-52. No toca ninguna autoridad de §19 ni archivos de I-52: sin STOP |
| main `104ef3a` | sin cambios desde V8 | I-53S esta integrada (`[V9-D12]`) |

**Cambios de paralelas entre la publicacion de V7 y la de V8 (registrados por V8; vigentes).**

| Paralela | Cambio | Relacion con I-52 |
|---|---|---|
| **main `104ef3a`: I-53S E2 integrada** | merge de I-53S con G5 `e528ef2` (cablea `ApplyHeaderBatch` en `RackSelectiveWindow`) y su cierre documental `d043afb`; el HANDOFF de main y el contrato de I-53S registran la re-medicion de C2-4 de I-52 hecha por I-53S como evidencia cruzada | clasificacion exacta: contrato de I-52 NON-MATERIAL; coordinacion MATERIAL; baseline de C2-4 MATERIAL / CHANGED. La prueba `C2_4_I52_…` es evidencia de I-53S y **no** cierra C2-4 de I-52. Como integro **antes** del Candidato: tras el rebase, CT-26, C2-4, T-M30 y las guardas relacionadas sobre el arbol combinado condicionan G6/G7 (`[V8-D17]`) |
| I-49 A1-R2 y A1-R3 (`c5b9ede` y despues `8ed15f7`) | ADR-0040 **propuesto** (`f8dddb6`) y, en el re-fetch, **aceptado** por el Owner (`2eeeab1`), que **reemplaza a ADR-0038**; nuevo Consensus Freeze sobre V6, A1 y ADR-0040; la rama se rebaso sobre `104ef3a` y `git range-diff` muestra identicos G5 (`977f873`, antes `fb9f630`), A1 (`c7fabbe`, antes `cae7a9f`) y ADR-0040 propuesto (`cc75a18`, antes `f8dddb6`); G6 BLOCKED | ADR-0040 conserva D1–D7 y D9–D25 sin cambiar su semantica y sustituye solo la guarda del parser de D8 (Amendment A1): lo que I-52 consume (D19 `PlanReadSet`, dominio del consumidor, `expression`) no cambia; `ProjectVariablesConformanceTests.cs` identico entre `fb9f630` y `977f873`. Se cumple la condicion de V8-11 y las referencias de I-52 se leen en ADR-0040. **NON-MATERIAL** (`[V8-D18]`) |
| **I-54 G6 (`ca71962`, tras el rebase `deca1e0`)** | dos archivos nuevos del Plugin, `src/RackCad.Plugin/CustomPropertiesData.cs` (coleccion de proyecto en el NOD, clave unica `RACKCAD_CUSTOM_PROPERTIES`, un Xrecord directo) y `src/RackCad.Plugin/CustomPropertiesExecutor.cs` (ejecutores de Proyecto y de Rack; los valores por rack se escriben con `RackBlockData.Write`), y dos de pruebas (`CustomPropertiesEdgeGuardTests.cs` con T-GRD-04..T-GRD-07 y `CustomPropertiesProjectTests.cs`); sin llamadores en `src/`; sin comando ni UI | no modifica `RackEmbedDocument`, `RackEmbedComposer`, el restamp, `ProjectVariables`, la UI ni los valores `CustomProperties` del sobre por rack. Materialidad arquitectonica para I-52 = **NON-MATERIAL**; coordinacion y guardas cruzadas = **MATERIAL**: CA-4 (rack) y CA-4b (NOD del dibujo) en §6.4, G-M22 y T-GRD-04..T-GRD-07 en G-M18 al integrar (`[V8-D19]`) |
| I-54 G6-CLOSE y G7A (`b4e99e4`, despues `d9f8c57`; `5c16ae8`) | solo docs: G6 completo y aceptado por su Coordinador; en G7A el Owner acepta `RACKPROPIEDADES` con alias `RPR` y G7 queda autorizado (G8..G10 no); la rama se rebaso sobre `104ef3a` y `git range-diff` muestra identicos G3..G6 (G4B `0010144`, G5 `0572be4`, G6 `deca1e0`) | sin cambio de contrato para I-52. Coordinacion: G7B de I-54 cambiara el censo de `[CommandMethod(` y previsiblemente `RackCommandReference.cs`, que I-52 G7 tambien toca; quien integre despues re-mide el censo y serializa ese archivo, y un gate de I-54 que toque el Plugin o la UI se reporta antes de continuar (`[V8-D19]`, `[V8-D24]`) |
| I-53D (`aa0f817`) | reclamo `c5a1404` y bootstrap: solo docs (contrato y fila de ROADMAP); preve cablear el Dinamico en `RackDynamicSystemWindow` | sin cambio para I-52; si conecta `ApplyHeaderBatch` al Dinamico, C2-4 del Dinamico se vuelve a medir (§15.3) |

Ninguno modifica materialmente una autoridad de I-52 (§19): se registran **sin bloquear**. La orden de V10 declaraba
como ultimos observados main `ba497f1`, I-52 `deb08cd`, I-49 `5f969cc` e I-53D `a57bd50`, con I-54 integrada; el
preflight de V10 los confirmo sin ramas nuevas y con la rama remota de I-54 retirada (`[V10-D21]`), y el re-fetch previo
a la publicacion encontro main `dad4e77` con I-53D integrada, I-49 `5a3714f` e I-55 `cfdb702`, registrados arriba sin
cambiar el contrato (`[V10-D23]`). La historia de paralelas anterior esta en Proposal V9 §15.1, en Proposal V8 §15.1, en
Proposal V7 §15.1 y en el registro §42, §48 y §54. La numeracion de ADR esta en §16.

### 15.2 Cruces

| Paralela o baseline | Cruce | Tratamiento |
|---|---|---|
| **I-50** (**integrada** en main) | `DimensionViews` en DTO, dominio y resolvedores (C-01..C-15); politica de cotas; ADR-0035 aceptado | `DimensionViews` es requisito normativo (§10.3; CA-5); T-M18 incondicional; firmas de anotaciones de C-2 y V-VIEW-ALL con la politica de I-50 |
| **I-53 E1** (**integrada** en main `1b091be`; ADR-0037 **aceptado** en main), **I-53S E2** (**integrada** en main `104ef3a`) e **I-53D E3** (**integrada** en main `dad4e77`) | tipos `Header*` en `Systems.Shared` con C-08/C-10; consumidores Selectivo y Dinamico aditivos; I-53S G5 conecta `ApplyHeaderBatch` a `RackSelectiveWindow` e I-53D G7 conecta `DynamicHeaderBatch` y `DynamicRackRebuild` a `RackDynamicSystemWindow` en main | baseline: I-52 no crea tipos en `Systems.Shared` (G-M14, CT-41) ni modifica `SharedFoundationInspection`. **I-53S, clasificacion exacta (`[V8-D17]`)**: materialidad del contrato de I-52 = NON-MATERIAL; materialidad de coordinacion = MATERIAL; baseline de C2-4 = MATERIAL / CHANGED. `C2_4_I52_TheEditorStateAfterADistribution_EqualsTheReopenedDocument_InEveryView` es evidencia de I-53S y **no** cierra C2-4 de I-52. Como I-53S integro **antes** del Candidato: tras el rebase, CT-26, C2-4, T-M30 y las guardas relacionadas se ejecutan sobre el arbol combinado y condicionan el cierre de G6/G7; **I-53D, clasificacion exacta (`[V10-D15]`)**: contrato NON-MATERIAL, coordinacion MATERIAL y C2-4 del Dinamico MATERIAL / CHANGED; como tambien integro antes del Candidato (`dad4e77`, `[V10-D23]`), T-M17 y T-M17b se suman sobre el arbol combinado; una integracion **posterior** al Candidato se reporta al Coordinador y se re-mide contra un main que contenga I-52; todo tipo persistido nuevo de cabecera o de modulo queda en RED en el cierre transitivo; serializar si C-1 llegara a tocar comandos de `RACKEDITAR` |
| **I-49** (V6 y Amendment A1 congelados; ADR-0040 **aceptado**, que reemplaza a ADR-0038; rama rebasada sobre `104ef3a`: G5 sintactico `977f873`, A1 `c7fabbe`, aceptacion de ADR-0040 `8f6d583` y Consensus Freeze `8ed15f7`; G6 `5f969cc` productivo, sin integrar; Amendment A2 `5a3714f`, solo docs) | `expression` en `PropertyValues`; `PlanReadSet` (D19); dominio del consumidor declarado por el descriptor (P24.5); su freeze exige que la prueba de supervivencia de `expression` (P24.8) se escriba contra `RackDuplicationPlan` y el restamp vigentes en main; guardas de texto evolucionadas en G5 y, en G6, `LengthUnitsAuthorityGuardTests` sobre todo `src/`; ADR-0040 como sucesor aceptado de ADR-0038 (solo cambia la guarda del parser de D8) | **I-52 coordina con I-49 antes de tocar `RackDuplicationPlan.cs`** (G4); T-M19. Si I-49 integra antes: su `PlanReadSet` es RS-3 (§9.2.1), su dominio es la cota `b` de V-DEP, su descriptor se clasifica en la guarda (§6.7) y sus guardas de texto rigen (G-M18, CT-46). `8ed15f7` es **NON-MATERIAL** (`[V8-D18]`) y `5f969cc` tambien lo es para el contrato, sin `PlanReadSet` ni dominio (`[V9-D21]`, `[V10-D14]`), y el Amendment A2 (`5a3714f`) solo cambia la sintaxis del cualificador de `VariableId` y anuncia un ADR de reemplazo de D6 y D7 de ADR-0040, sin tocar lo que I-52 consume (`[V10-D23]`): con ADR-0040 aceptado, las referencias de I-52 a ADR-0038 se leen en ADR-0040 sin cambio semantico. V10 **no** presupone el motor |
| **I-54** (**integrada** en `main @ ba497f1`; ADR-0039 **aceptado** en main; rama remota retirada) | `CustomProperties` (`JsonElement?`) heredado y normalizado por `Compose`, con la regla F; T-GRD-01..T-GRD-03 (sobre y restamp), T-GRD-04..T-GRD-07 (borde fisico) y T-GRD-08 (censos de comandos, ayuda y ventanas) en main; Custom Properties de alcance Proyecto en el NOD `RACKCAD_CUSTOM_PROPERTIES` y de alcance Rack en el sobre; `RACKPROPIEDADES`/`RPR`; **D-21 vigente** (el espejo cumple: A y despues B); residuales F-14a/F-14b vigentes | baseline de main: materialidad arquitectonica NON-MATERIAL; coordinacion y guardas cruzadas MATERIAL (`[V8-D19]`, `[V10-D13]`). **CA-4** portador RACK-LEVEL que se preserva con la copia; **CA-4b** Custom Properties de alcance PROYECTO que el espejo no lee para reflejar el rack y no copia, refleja, transforma ni escribe (§6.4, `[V9-D10]`, `[V10-D11]`); G-M22; G-M18 aplica T-GRD-01..T-GRD-07; T-M16 reapunta T-GRD-08 (ayuda sin alias, `[V10-D08]`); el gate que introduce la llamada a `Compose` del espejo reapunta T-GRD-02 (7 → 8); T-GRD-01 y T-GRD-03 compatibles con ID-5 e ID-6; F-14a ⇒ E2 (ST-2c); F-14b y regla F ⇒ E8 en P10 (§9.4); T-M20 |
| **I-55** (View Placement & Projection; rama `feature/creacion-de-vistas` @ `cfdb702`, sobre `dad4e77`; G1.1, solo docs; G2 solo documental) | ID17, ID18 e ID19 sobre una foundation de preparacion de vistas antes de materializar; ID19 proyecta varios racks existentes con una transformacion comun y conserva cada `RackId`; 0 archivos productivos compartidos con I-52; cruce semantico con la autoridad pura de planes por vista (§8.1, C2-1), el materializador de G6, el contexto de materializacion (§8.7) y la colocacion `P'`, y con el censo de `Compose` de I-54 si unifica la materializacion | sin cambio para I-52 mientras I-55 no tenga Proposal acordada ni produccion; I-52 no depende de I-55. Si su G2 fija una autoridad de preparacion de vistas, de materializacion o de transformacion multi-rack, o toca rutas de materializacion (`ViewBlockDraw`, `SystemBlockWriter`, `BlockPlacement`, `LateralHeaderDrawService`, `CantileverViewMaterializer`), `RackEmbedComposer.Compose`, comandos o `RackCommandReference.cs`: reportar antes de fijar archivos, re-evaluar frente a §8.1, §8.7, `MaterializationContextReadSet`, PRE-17, PRE-18, T-GRD-02 y el censo `B`, y llevar a Coordinador y Arquitecto cualquier autoridad comun (R-45, `[V10-D23]`) |
| Documental | Filas de ROADMAP, final de `ideas-futuras.md`, `docs/adr/README.md` (0035, 0037 y 0039 en main; 0036 de I-52; 0038, reemplazado, y 0040, aceptado, de I-49), censo de comandos `B → B + 1` (hoy `B = 35` en `main @ dad4e77`), censos por nombre y por pares de T-GRD-08 en main, `src/RackCad.UI/RackCommandReference.cs` y `SelectiveEditorOpenTests.cs` | Quien integre despues conserva todas las filas en orden numerico, re-mide los censos (por numero, por nombre y por pares de ayuda) y serializa esos archivos |

### 15.3 Protocolo antes de G3, G5, G7 y del Candidato

`git fetch --all --prune` al empezar y justo antes de cada commit; `git diff --name-only origin/main...origin/<rama>` de
I-49 e I-55 (y de cualquier iniciativa nueva; I-54 e I-53D ya estan integradas en main); leer sus Proposals y contratos
en el SHA exacto; buscar `docs/adr/0036-*` y citas de «ADR-0036» en **todos** los refs (§16); comprobar los archivos de
§19; si `main` avanzo, rebase segun WORKFLOW antes de escribir codigo. Ademas:

- **I-49:** si integra, re-medir la cota autoritativa de V-DEP (§6.5.5), la autoridad del read-set (§9.2.1) y sus
  guardas de texto (CT-46); ADR-0040 ya esta aceptado y reemplaza a ADR-0038: las referencias de I-52 se leen en
  ADR-0040 (`[V8-D18]`). Su G6 (`5f969cc`) añade el nucleo semantico y la autoridad de unidades con
  `LengthUnitsAuthorityGuardTests` sobre todo `src/`: al reconciliar, CT-46 la relee y G-M18 la aplica a los archivos de
  I-52; si un gate posterior de I-49 introduce `PlanReadSet` o dominio aplicado, o toca ProjectVariables, Persistence o
  Systems, reportar antes de continuar (`[V9-D21]`, `[V10-D14]`). Su Amendment A2 (`5a3714f`, solo docs) anuncia un ADR
  de reemplazo de D6 y D7 de ADR-0040: si el Owner lo acepta, las referencias de I-52 a ADR-0040 se leen en ese ADR
  mientras D19 y el dominio del consumidor sigan sin cambio; si cambian, reportar (`[V10-D23]`).
- **I-54 (integrada en `main @ ba497f1`):** sale del protocolo de ramas paralelas. Tras el rebase, re-medir CA-4 y G-M22
  sobre main, aplicar T-GRD-01..T-GRD-07 en G-M18 y reapuntar con motivo T-GRD-02 (7 → 8, en el gate que introduce la
  llamada a `Compose` del espejo) y T-GRD-08 (T-M16: `RACKMIRROR` por nombre y entrada de ayuda sin alias; `[V10-D08]`,
  `[V10-D13]`); un cambio posterior en main que toque el sobre, el compositor, el restamp, `ProjectVariables` o las
  autoridades de CA-4 o CA-4b se reporta antes de continuar.
- **I-53S/I-53D (integradas en main):** I-53S (`104ef3a`) e I-53D (`dad4e77`) integraron antes del Candidato de I-52 y
  no son ramas paralelas: tras el rebase, ejecutar CT-26, C2-4, T-M30 y las guardas relacionadas sobre el arbol
  combinado y, en el Dinamico, tambien T-M17 y T-M17b, como condicion de cierre de G6/G7 (`[V8-D17]`, `[V9-D22]`,
  `[V10-D15]`, `[V10-D23]`); un cambio posterior en main que altere la frontera estado del editor → sistema mas alla de
  §15.1 se reporta antes de continuar.
- **I-55 (`feature/creacion-de-vistas`, View Placement & Projection, solo docs):** vigilar su G2; si fija una autoridad
  de preparacion de vistas, de materializacion o de transformacion multi-rack (ID19), o toca rutas de materializacion,
  `RackEmbedComposer.Compose`, `SystemBlockWriter.cs`, comandos o `RackCommandReference.cs`, reportar antes de
  continuar, re-evaluar §8.1, §8.7, `MaterializationContextReadSet`, PRE-17, PRE-18, T-GRD-02 y el censo `B`, y llevar a
  Coordinador y Arquitecto cualquier autoridad comun antes de fijar archivos (R-45, `[V10-D23]`).
- Recalcular el SHA-256 de la biblioteca de evidencia antes de reutilizarla (CT-47, `[V7-D17]`) y censar 0035..0040 en el
  indice de ADR y en todos los refs.

---

## 16. ADR-0036: numeracion y aceptacion

- Archivo: `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`, estado **`propuesto`**, con fila en el indice de la
  rama. Actualizado en este gate con las correcciones de V10.
- Congela: espejo semantico por copia; `μ_k` canonica por kind; admisibilidad por vista con seccion canonica y
  verificacion de **todas** las vistas admisibles; fail-closed; reflectores en Application sobre el sustrato real;
  prohibicion de normalizaciones que congelen valores dependientes de vinculos; metadata semantica desconocida ⇒
  fail-closed; autoridad de planes; colocacion canonica sin escala negativa, `Origin = 0` y sin bloques dinamicos,
  anonimos o anotativos; identidad nueva; portadores; atomicidad semantica e importacion best-effort;
  **equivalencia visual-geometrica** evaluada por estado sobre la definicion real, con politica cerrada de clases y
  variantes, fuentes visuales simbolicas tipadas, `VisibilityPath`, orden visual dentro de la pieza y entre piezas,
  primitivas canonicas conservadoras con `SourceKind`, ruta anidada estable, parametros efectivos con paridad exacta,
  transformacion efectiva, base auxiliar con el contexto de la autoridad origen, cache de biblioteca aislada y accedida
  por su API publica, degradado fail-closed e identidad SHA-256 de la evidencia; presentacion exterior de la referencia
  fuente conservada, orden en Model Space demostrado frente a todo objeto posterior a cada fuente, huellas visuales de
  ocupacion de modelo en el contexto destino y refrescadas tras `EnsureForPlan`, y read-set del contexto de
  materializacion; obligacion 9b y V-DEP; allowlist de portadores y metadata exterior; cierre transitivo de cobertura;
  C-2 con Insertar; verificacion dinamica por rack; taxonomia en tres dimensiones con regla de cierre de G3; alcance
  dependiente de la biblioteca efectiva y seleccion todo-o-nada.

**Protocolo de numeracion (vigente desde V2).**

1. Un numero de ADR queda **reclamado** por la primera publicacion **observable** de un archivo `docs/adr/NNNN-*` en un
   ref remoto.
2. **Registro de I-52:** ADR-0036 se publico por primera vez en `origin/feature/rackmirror-espejo-semantico` con el
   commit `0fc7032bf15d03e7d478bbd9350f156708621c9d` (`2026-09-12T19:59:36-06:00`; CI del push `34731908035`, creada el
   `2026-09-13T01:59:44Z`). En los preflights de V3 a V10 ningun otro ref contiene `docs/adr/0036-*`; las citas de
   «ADR-0036» en I-49, I-53 e I-54 se refieren a este ADR.
3. **Otros numeros observados:** ADR-0035 es de I-50 (aceptado e integrado en main). ADR-0037 lo publico I-53 en
   `d7f17ad` (`2026-09-12T21:24:06-06:00`); tras rebasar, su primera aparicion en el ref de I-53 fue `d0db698`; quedo
   **aceptado** en `5efaf7e` y ahora esta **en main** por el merge `1b091be`. ADR-0038 lo publico I-49 en `a160560`
   (`2026-09-13T00:43:42-06:00`) y el Owner lo **acepto** en `edafade` (`2026-09-13T01:58:43-06:00`); tras rebasar I-49
   sobre `1b091be` y despues sobre `104ef3a`, sus copias en el ref son `cba3f48` y `b995c50` (antes `95e380f` y
   `77f7976`). ADR-0039 lo publico I-54 en `d84f480` (`2026-09-13T03:07:47-06:00`) y el Owner lo **acepto** en `3866252`
   (`2026-09-13T03:45:42-06:00`); tras rebasar I-54 sobre `1b091be` y despues sobre `104ef3a`, sus copias en el ref son
   `52afa69` y `c5662e2` (antes `4907fc3` y `238cbf7`); con la integracion de I-54 esta **aceptado en main**
   (`ba497f1`). ADR-0040 lo publico I-49 en `f8dddb6` (`2026-09-13T16:30:55-06:00`) como sucesor de ADR-0038 y el Owner
   lo **acepto** en `2eeeab1` (`2026-09-13T17:07:58-06:00`), reemplazando a ADR-0038; tras rebasar I-49 sobre `104ef3a`,
   sus copias en el ref son `cc75a18` y `8f6d583`. En cada commit citado la fila del indice y el encabezado del ADR
   coinciden (`[V5-D30]`). Todas son posteriores a 0036 y usan otro numero: **sin colision**. El Amendment A2 de I-49
   (`5a3714f`) anuncia un ADR de reemplazo de ADR-0040 todavia sin numero ni archivo; cuando se publique se censa con
   este protocolo (`[V10-D23]`).
4. **Antes de pedir la aceptacion del Owner**, I-52 busca 0036 en todos los refs: archivos `docs/adr/0036-*` y citas de
   «ADR-0036» o «adr/0036». Si existe una publicacion **anterior** de otro ADR-0036, I-52 renumera antes de la
   aceptacion; una publicacion posterior no obliga a I-52 a renumerar.
5. Una vez ADR-0036 sea **`aceptado`**, **no** se renumera (`adr/README.md`: un aceptado es inmutable).
6. Dos ADR **aceptados** con el mismo numero ⇒ **STOP** y escalado al Owner.

**Aceptacion (vigente desde V3).** Solo el Owner (O-4). **No** es precondicion de G3; se pide **despues de G3**, si G3
no contradice materialmente V10, y **es precondicion de G4**. Mientras tanto el ADR sigue `propuesto` y puede corregirse
si G3 abre Proposal V11. `docs/adr/README.md` no cambia en este gate: la fila de 0036 de la rama sigue correcta, y la
reconciliacion con main (que ya lista 0035, 0037 y 0039) se resuelve en el rebase posterior al consenso.

---

## 17. Riesgos abiertos

| # | Riesgo | Mitigacion |
|---|---|---|
| R-1 | Casi todas las piezas de frontal y planta necesitan aceptar un cambio de mano: si la sonda por estado no es viable en `acad.exe`, no tiene paridad con el materializador o la biblioteca usa clases o apariencias fuera de la politica, kinds enteros quedan en fail-closed; la evidencia indicativa de §1.4 ya apunta a separadores, parrillas, desviadores A, defensas y tarimas | CT-36, CT-47, CT-48, CT-49 y CT-50 en G3; O-1 lo acepta (L-22, L-28..L-37); cambio material de alcance ⇒ Proposal V11 |
| R-2 | Un Dinamico dibujado solo en lateral no puede reflejarse | Mensaje E5 explicito; ALT-L como futuro |
| R-3 | V-RT-STORE puede rechazar payloads legitimos si la idempotencia del store no es exacta | CT-35 caracteriza por kind |
| R-4 | C-1, si llegara a activarse, toca comandos calientes que I-49 e I-53 tambien tocan | C-2 por defecto; C-1 solo con evidencia; serializar |
| R-5 | Semantica de «estandar» de cabecera tras normalizar Auto (H-06): la mayoria de las cabeceras de plantilla quedan fuera | G5 con CT-23; fail-closed mientras tanto; declarado en §2.3 y O-1 |
| R-6 | `MountingFace` sin semantica confirmada (S-10, D-03b) | CT-12; si no se confirma ⇒ FAIL_CLOSED |
| R-7 | Comandos transparentes de AutoCAD entre SNAPSHOT y MUTATE | Huella y read-set de §9.2 en PREPARE y MUTATE |
| R-8 | Numeracion de ADR con varias iniciativas que necesitan ADR | Protocolo de §16 |
| R-9 | Cantilever con brazos C/L sencillos o arriostramiento estructural asimetrico queda fuera | Clasificacion MC/UK; futuro con metadato de mano |
| R-10 | Rendimiento de V-VIEW-ALL y V-BOM en selecciones grandes (dos planes por vista admisible, dos BOM por rack) | Medir en G7; builders puros |
| R-11 | Selectivos con topes y **la mayoria de los Push Back** (tope posterior activo por defecto) inutilizables hasta O-2/O-2B | Fail-closed verificable antes de mutar; declarado en O-1; M-12 y M-24 con fixtures explicitos |
| R-12 | La auditoria de anclajes (CT-17) puede degradar muchas filas `R` + `G3_PENDING` a `UK` | Regla de cierre de G3; Proposal V11 si el cambio es material; nunca parche silencioso |
| R-13 | G4 toca el planificador de RACKDUPLICAR, validado por el Owner en I-51 | CT-16 primero; fachada con semantica observable identica; M-17; aviso a I-49 |
| R-14 | V-META deja fuera racks escritos por un build mas nuevo con metadata semantica, ahora tambien en el sobre y el envoltorio | Fail-closed deliberado: sin transformador no hay espejo fiel; futuro con declaraciones de invariancia o entradas CA-6 |
| R-15 | C-2 sobre ventanas WPF exige pruebas de UI mas costosas, ahora en G6 | `tests/RackCad.UI.Tests` permitido en G3 y G6; LC-UI |
| R-16 | La importacion de biblioteca puede dejar dependencias tras un fallo y su UNDO no esta demostrado | §9.3 y §9.5 lo declaran; M-10 y M-20 lo registran |
| **R-17** | Medios frentes con `PalletTolerance` vinculada y sin overrides quedan fuera | S-02b declarado; ALT-ABS como futuro |
| **R-18** | La guarda de cobertura obliga a clasificar cada miembro nuevo de cinco familias de tipos | Coste asumido: es la unica defensa ejecutable contra copias sin permutar |
| **R-19** | Aceptar el ADR despues de G3 añade una ronda del Owner entre G3 y G4 | Aceptado por el Coordinador: evita un ADR aceptado que G3 podria refutar |
| **R-20** | La obligacion 9b deja fuera racks con tarimas o parrillas cuyo ajuste depende de `PalletTolerance` vinculada sin overrides que las cubran (hoy no hay cota autoritativa) | S-15d/S-17d declarados; si I-49 integra, su dominio de consumidor puede ampliar lo admisible; M-25 |
| **R-21** | La evaluacion de bloques dinamicos en una `Database` auxiliar puede no reproducir la representacion del materializador (acciones que solo se re-ejecutan al recomputar graficos) | CT-36 en `acad.exe` con paridad frente a un documento real (`[V6-D14]`); punto de observacion caracterizado (`[V7-D14]`); re-verificacion en PREPARE con la definicion real; sin paridad demostrada ⇒ X-17a |
| **R-22** | Nombres de tipos del espejo que choquen con las guardas C-08/C-10 de I-53 | carpeta `RackCad.Application.Mirror`; G-M14; CT-41 |
| **R-23** | El cierre transitivo obliga a clasificar tipos y enums que V3 no listaba | Coste asumido: es la unica defensa ejecutable contra copias sin regla; T-M36 y T-M44 |
| **R-24** | Rendimiento de la sonda por estado en selecciones grandes (muchas piezas y estados, variantes, fuentes simbolicas tipadas, `VisibilityPath`, orden visual dentro de la pieza y entre piezas, canonicalizacion, contexto de la base auxiliar, clonado y huellas visuales de toda instancia enumerada) | cache por comando por `(definicion evaluada, valores efectivos)`; medicion preliminar en CT-36 y medicion en G7 |
| **R-25** | Racks legados con `HighEndBeamPeralte` (R-B) no se pueden reflejar en I-52 y Actualizar no los limpia | fail-closed con mensaje sin remedio; limpieza futura fuera de I-52 |
| **R-26** | Los Selectivos con `PalletTolerance` o `VerticalClearance` vinculadas quedan fuera salvo overrides que fijen el ancho y alturas fijadas por override o cabecera por poste (S-26, S-27a..S-27c) | declarado en O-1 (L-23); futuro con una autoridad que demuestre simetria para un dominio |
| **R-27** | La biblioteca no esta versionada y su ruta la configura el usuario: el alcance real cambia con la estacion y en el tiempo | L-28 en O-1; huella registrada en la evidencia (CT-47); siempre fail-closed; versionar la biblioteca queda fuera de I-52 |
| **R-28** | La exigencia de fuentes visuales simbolicas iguales puede excluir piezas que hoy se ven iguales pero mezclan modos (`ByLayer` frente a `Explicit`, `InheritLayer` frente a `InheritBlock`) o capas entre mitades | regla sound deliberada; hoy no quita alcance a las piezas estructurales medidas (§1.4); relajacion futura solo con decision del Owner |
| **R-29** | La canonicalizacion puede quedar ambigua en piezas con geometria solapada o degenerada | `UNKNOWN` ante ambiguedad; tasa medida en CT-36 y CT-47; ambiguedad material ⇒ Proposal V11 |
| **R-30** | Con la biblioteca inspeccionada, gran parte de Dinamico y Push Back fallaria cerrado por los separadores de planta (§1.4) | evidencia indicativa; G3 la confirma o refuta; si reduce materialmente la envolvente ⇒ Proposal V11 y nueva O-1; la equivariancia de builders queda fuera de I-52 |
| **R-31** | La `VisibilityPath` ordenada puede excluir piezas cuyas mitades usan anidados distintos con las mismas capas | regla estricta deliberada; hoy 0 rutas distintas en las piezas medidas (§1.4); relajacion solo con evidencia de G3 (CT-36) |
| **R-32** | La canonicalizacion conservadora (sin fusion ni normalizacion de sentido sin continuidad demostrable) puede dar falsos negativos, sobre todo en curvas `ByLayer` sin anidado espejado | fail-closed; hoy sin perdida en las piezas estructurales medidas (§1.4); CT-36 y CT-47 miden la tasa; habilitacion adicional solo con evidencia |
| **R-33** | La base auxiliar puede no ofrecer un punto de observacion soportado equivalente al materializador | X-17a; si afecta a kinds enteros ⇒ Proposal V11 o reduccion de alcance |
| **R-34** | Aislar la cache exige clonar cada definicion hacia una base privada, con coste por pieza y estado | medicion preliminar en CT-36 y en G7 (R-24); la cache nunca se usa como base auxiliar |
| **R-35** | El orden visual de plan (`[V8-D12]`, `[V9-D03]`) puede dejar fuera kinds o vistas: bajo `μ_k` se invierten los pares de una misma pasada y cualquier par invertido con huellas superpuestas de forma material, o con huella `UNKNOWN`, falla (X-22) | CT-48 enumera los pares invertidos por kind y vista, mide su superposicion y registra la reduccion de alcance; hoy el frontal Selectivo emite postes antes que largueros (§11.3 punto 6.6); reduccion material ⇒ Proposal V11 y nueva O-1 |
| **R-36** | ST-18 hace fallar racks cuya referencia lleva XCLIP, atributos o un estado de presentacion que CT-49 no clasifique | fail-closed deliberado por decision del Coordinador; mensaje propio; M-34; inventario de CT-49 antes de G7 |
| **R-37** | ST-19 (`[V9-D01]`, `[V10-D06]`, `[V10-D07]`) puede hacer fallar copias cuyo destino se superpone con cualquier objeto posterior a su fuente (sombreados, wipeouts, imagenes, otras fuentes seleccionadas u objetos seleccionados e ignorados), toda seleccion con un `XLINE` o `RAY` posterior a alguna fuente, aunque este lejos, y las fuentes con ordenes de pantalla y de trazado contradictorios cuyas copias se superponen; clasificar esas superposiciones tiene coste | se clasifican todos los objetos posteriores a cada fuente cuya huella interseca la de la copia; rechazo anticipado antes de importar y decision definitiva tras `EnsureForPlan`; declarado en O-1 (L-34, L-37); M-35; medicion en G7 |
| **R-38** | AutoCAD 2025 puede no permitir reproducir en una base auxiliar el contexto del dibujo (por ejemplo, `PSTYLEMODE` con estilos de trazado con nombre) | `UNKNOWN` ⇒ X-17a en las piezas con cambio de mano afectadas; CT-36 con dibujos STB y CTB; si afecta a kinds enteros ⇒ Proposal V11 |
| **R-39** | La politica cerrada de variantes puede excluir bibliotecas con polilineas de `Plinegen` o `Closed` distintos entre mitades, tipos de linea complejos, `MInsertBlock` o anotativos anidados | regla sound deliberada; hoy sin perdida en las piezas medidas (§1.4, `[V8-D23]`); habilitacion futura solo con evidencia |
| **R-40** | Las huellas de ocupacion de modelo (`[V9-D02]`, `[V10-D01]`) pueden sobre-estimar (`Extents3d`, envolventes de texto y de cota) y producir superposiciones que el comando trata como materiales sin serlo, o quedar `UNKNOWN` en clases sin ocupacion caracterizada; y, al no dilatarse por anchuras de presentacion, no garantizan el orden cuando dos objetos solo se tocan por grosor de linea, papel o pantalla | fail-closed deliberado para la sobre-estimacion; limitacion declarada en O-1 para las anchuras de presentacion (L-37); CT-36 y CT-49 miden las ocupaciones; relajacion solo con evidencia; reduccion material ⇒ Proposal V11 |
| **R-41** | El contexto de materializacion (`[V9-D05]`, `[V10-D05]`) hace que la copia pueda verse distinta de una fuente dibujada con otro contexto (capa `0` heredada, color o escala de tipo de linea por defecto, estilo de cota o de texto), y un read-set mal caracterizado podria invalidar de menos o de mas | declarado en O-1 (L-36) con la misma semantica de Actualizar; read-set por productor caracterizado por CT-50 y consumo no determinable ⇒ `UNKNOWN`; M-37; la presentacion exterior se conserva (ST-13) |
| **R-42** | I-52 G7 comparte con main (I-54 integrada) `RackCommandReference.cs`, `SelectiveEditorOpenTests.cs` y T-GRD-08 (censos por nombre de comandos, de ayuda y de ventanas), y el plan del espejo añade una llamada a `Compose` que censa T-GRD-02 | censo `B + 1` re-medido (hoy `B = 35`); entrada de ayuda sin alias con el alias verificado solo si no esta vacio; reapuntado con motivo sin debilitar (T-M16, `[V10-D08]`); T-GRD-02 7 → 8 clasificado con motivo |
| **R-43** | El `DestinationEvaluationContext` (`[V10-D02]`, `[V10-D09]`) puede no simular fielmente la importacion en dibujos con colisiones de nombres de bloque, de bloque anidado o de simbolos, lo que deja huellas `UNKNOWN` y reduce el alcance | DRAWING FIRST, LIBRARY SECOND WITH IGNORE; CT-36 y T-M63 con colisiones explicitas; `UNKNOWN` ⇒ E10/X-22; si afecta a kinds enteros ⇒ Proposal V11 |
| **R-44** | La frescura tras `EnsureForPlan` (`[V10-D03]`) puede hacer fallar la operacion despues de importar, dejando la infraestructura importada en el dibujo, y releer y recalcular huellas añade coste | E10 sin mutacion semantica; lo importado puede permanecer (§9.3, §9.5); el rechazo anticipado antes de importar reduce esos casos; coste medido en G7 (R-24) |
| **R-45** | I-55 (View Placement & Projection, `cfdb702`, solo docs) preve una foundation de preparacion de vistas antes de materializar y una proyeccion multi-rack con una transformacion comun (ID19) que cruzan la autoridad pura de planes por vista de I-52 (§8.1, C2-1), su materializador (G6), el contexto de materializacion (§8.7) y la colocacion `P'`: dos autoridades paralelas podrian divergir o duplicarse | I-52 no depende de I-55 ni la presupone; antes de G3 de I-52 y antes de fijar archivos en G2 de I-55 se re-mide el cruce (§15.3); una autoridad comun se decide con Coordinador y Arquitecto; una autoridad duplicada o divergente se reporta antes de continuar (§19) |

---

## 18. Desacuerdos y decisiones

### 18.1 Decisiones registradas (no son consenso)

| # | Tema | Resolucion vigente | Donde |
|---|---|---|---|
| DA-1 | Convergencia con `RACKEDITAR` | C-2 por defecto con los productores reales (Actualizar **e Insertar**), CI, G-M9 y M-4/M-5; **GREEN en G6**; C-1 solo con evidencia | §8.5 |
| DA-2 | Simetria de bloques | Sin registro de centros: `GeometrySymmetryResult` visual-geometrico por estado evaluado, sobre la definicion real, con politica cerrada de clases y variantes, fuentes visuales simbolicas tipadas, `VisibilityPath`, orden visual dentro de la pieza y entre piezas, mas `PlanPlacementEvidence` | §11.3 |
| DA-3 | Politica post-commit | Sin `Regen`; un unico `Regen` solo con evidencia del Owner | §8.6 |
| DA-4 | Recorte del primer corte | Envolvente maxima de §1.3; O-1 despues del freeze tecnico y antes de G3 | §1.2, §1.3 |
| DA-5 | `IsStandard`/`StandardBaselineId` | UNKNOWN → FAIL_CLOSED hasta G5 | §6.5.4 |
| DA-6 | Numeracion de ADR | Precedencia del primer ref remoto publicado | §16 |
| CR-V2-1 | Registro | Criterio de produccion; I-52 no lo relaja | §10.2 |
| CR-V2-3 | Tope posterior Push Back | `UNKNOWN → FAIL_CLOSED`; activo por defecto | §7.3 |
| CR-V3-2 | Estabilidad | Obligacion 9b; V-DEP; `W_lb` solo con cota autoritativa | §6.5.5 |
| CR-V3-3 | Metadata exterior | Allowlist cerrada de portadores | §6.4 |
| CR-V3-4 | Cobertura | Cierre transitivo con enums; coexistencia con I-53 | §6.7 |
| CR-V4-1..CR-V4-6 | Evidencia por estado, estados futuros, G3 y sonda, O-1, contrato de serializacion, proceso | vigentes con las precisiones de V6, V7, V8, V9 y V10 | §11.3, §6.4, §6.5.5, §6.7 |
| CR-V5-1 | Equivalencia visual-geometrica (C:V6-01) | Sustituida en su mecanismo por CR-V6-1 y CR-V6-2 | §11.3 punto 6 |
| CR-V5-2 | O-1 y biblioteca (C:V6-02, C:V6-16) | Envolvente maxima + evidencia indicativa separada + todo-o-nada + dependencia de la biblioteca efectiva | §1.2..§1.4 |
| CR-V5-3 | Primitivas canonicas (C:V6-03) | Precisada por CR-V6-4 y CR-V7-1 | §11.3 punto 8 |
| CR-V5-4 | Parametros y runtime (C:V6-04, C:V6-05, C:V6-06) | Paridad con el materializador y valores efectivos; CT-36 en `acad.exe` con postcondicion; `Origin` del BTR evaluado; precisada por CR-V6-5 | §11.3 puntos 5, 13, 14 |
| CR-V5-5 | Degradados (C:V6-07) | Gradient Hatch = `UNKNOWN → FAIL_CLOSED` en el primer corte; tambien patron (CR-V6-4) | §11.3 punto 6.1 |
| CR-V5-6 | Proceso (C:V6-17) | Desde V6, contradiccion material ⇒ Proposal V7 (historial; sustituida por CR-V6-6 y CR-V7-9) | §7.0, §14.1 |
| CR-V6-1 | Fuentes visuales simbolicas (C:V7-01) | Igualdad de fuentes tipadas por propiedad (`Explicit`, `ByLayer(Named)`, `InheritLayer`, `InheritBlock`, estados de la API) con rutas canonicas; nunca valores resueltos; precisada por CR-V7-2 y CR-V7-5 | §11.3 punto 6.2 |
| CR-V6-2 | Visibilidad (C:V7-02) | `VisibilityPath` ordenada de ancestros y primitiva; mecanismo no modelado ⇒ `UNKNOWN` | §11.3 punto 6.3 |
| CR-V6-3 | Orden visual (C:V7-04) | Relacion solo para superposiciones materiales; orden relativo de homologas conservado o X-22; ampliada entre piezas por CR-V7-6 | §11.3 punto 6.5 |
| CR-V6-4 | Canonicalizacion y clases (C:V7-05, C:V7-06) | Conservadora: sin fusion ni normalizacion de sentido sin continuidad demostrable; SOLID/TRACE por poligono 1-2-4-3; HATCH solido solo canonizable; degradado y patron ⇒ `UNKNOWN`; precisada por CR-V7-1 | §11.3 puntos 6.1 y 8 |
| CR-V6-5 | Sonda (C:V7-07..C:V7-10) | `applied`; punto de observacion caracterizado; cache de biblioteca READ / CLONE ONLY; transformacion efectiva; identidad de la evidencia por SHA-256; precisada por CR-V7-2 y CR-V7-4 | §11.3 puntos 5, 13, 14 |
| CR-V6-6 | Proceso (C:V7-15) | Desde V7, contradiccion material ⇒ Proposal V8 (historial; sustituida por CR-V7-9) | §7.0, §14.1 |
| **CR-V7-1** | Politica cerrada de variantes (C:V8-01) | Clase + variantes soportadas por tipo exacto; `SourceKind` y `PolylineGeneration` en la firma; sin emparejar polilineas con `Line` o `Arc`; `MInsertBlock`, anotativos y XCLIP anidados, grosor, normal, elevacion y tipos de linea complejos fuera de la tabla ⇒ `UNKNOWN` | §11.3 puntos 6.1, 6.4 y 8 |
| **CR-V7-2** | Fuentes tipadas y contexto de la base auxiliar (C:V8-02, C:V8-03) | `Explicit(metodo, valor)`; la base auxiliar reproduce el contexto de la autoridad origen (`PSTYLEMODE`, `INSUNITS`, escala de anotacion) o `UNKNOWN` | §11.3 puntos 4 y 6.2 |
| **CR-V7-3** | Patron de tipo de linea (C:V8-04) | `LinetypePatternContext` de los ancestros para trazos sin continuidad demostrable; escalas globales como contexto comun; `CELTSCALE` sin efecto retroactivo | §11.3 puntos 6.2 y 6.4 |
| **CR-V7-4** | Acceso a la cache (C:V8-05) | Solo `BlockLibraryImporter.EnsureBlocks(auxDb, nombres)` o la API publica equivalente vigente, con verificacion de todos los bloques requeridos; sin reflexion ni campo cacheado | §11.3 punto 14; G-M20 |
| **CR-V7-5** | Ruta anidada estable (C:V8-06) | Identidad estable de la definicion base, valores dinamicos efectivos y ordinal por contenido; nunca `ObjectId`, handle ni nombre anonimo; empate ⇒ `UNKNOWN` | §11.3 punto 6.2 |
| **CR-V7-6** | Orden visual de plan (C:V8-07) | Orden relativo de los pares con superposicion material entre instancias de pieza conservado o X-22; V-VIEW-ALL no es multiconjunto puro para esos pares | §11.1; §11.3 punto 6.6 |
| **CR-V7-7** | Presentacion de la referencia fuente (C:V8-08) | La copia conserva capa, color, tipo de linea, escala de tipo de linea, grosor, transparencia, estilo de trazado y visibilidad; estado no transportable ⇒ E4 (ST-18); orden en Model Space demostrado o E10 (ST-19); nunca un aviso | §4.1; §8.2; §9.1 |
| **CR-V7-8** | Caso F y paralelas (C:V8-09..C:V8-12) | Doce casos con matriz clase × caso y extractor unico; I-53S con clasificacion exacta y arbol combinado; I-49 `c5b9ede` no material; I-54 G6 con CA-4 (rack) y CA-4b (NOD) | §11.3 punto 9; §6.4; §15 |
| **CR-V7-9** | ADR, G3 y proceso (C:V8-13..C:V8-15) | ADR-0036 «debe demostrar»; CT-36, CT-47, CT-48 y CT-49; desde V8, contradiccion material ⇒ Proposal V9 (historial; sustituida por CR-V8-8) | ADR-0036; §12; §14.1 |
| **CR-V8-1** | ST-19 completo y huella de Model Space (C:V9-01, C:V9-04) | Todo objeto de Model Space posterior a la fuente, seleccionado o no, admitido o ignorado; huellas conservadoras; contexto de orden de CT-49; superposicion material o no clasificable ⇒ E10 | §4.1; §9.1 |
| **CR-V8-2** | Huellas visuales y orden de plan (C:V9-02, C:V9-03) | `VisualFootprintResult` separado de `GeometrySymmetryResult`; P7a y P7b tambien para piezas sin cambio de mano; par invertido con huellas superpuestas de forma material o huella `UNKNOWN` ⇒ X-22 | §9.1; §11.3 puntos 5 y 6.6 |
| **CR-V8-3** | Contexto de materializacion (C:V9-05) | Fiel al plan regenerado, como Actualizar; `MaterializationContext` comun a las dos generaciones y a la materializacion; productor asimetrico ⇒ `UNKNOWN`; ST-13 para la presentacion exterior; sin tocar drawers | §8.7; L-36 |
| **CR-V8-4** | Censo de comandos (C:V9-06) | `B + 1` con `B` re-medido en el SHA de reconciliacion; censos por nombre de I-54 reapuntados con motivo | PDC-8; T-M16 |
| **CR-V8-5** | Clonado de bloques (C:V9-07) | Retorno de `EnsureBlocks` solo diagnostico; presencia de todos los BTR requeridos; dos rutas autorizadas; G-M20 precisada | §9.5; §11.3 punto 14 |
| **CR-V8-6** | Inventario cerrado (C:V9-08) | Metodo del harness y clasificacion `TRANSPORTED`, `PLACEMENT`, `COMMON_CONTEXT`, `NON_VISUAL` o `UNSUPPORTED`; referencia anidada sin atributos ni XREF | §11.3 puntos 6.1 y 14; ST-18 |
| **CR-V8-7** | Fuentes tipadas y CA-4b (C:V9-09, C:V9-10) | `ColorBook` con valor almacenado; `PlotStyleName` solo con STB; CA-4b = Custom Properties de alcance Proyecto | §11.3 punto 6.2; ST-13; §6.4 |
| **CR-V8-8** | Paralelas, ADR, G3 y proceso (C:V9-11..C:V9-15) | I-54 G7B material para coordinacion; I-49 G6 e I-53D G7 registrados sin cambio de contrato (C2-4 del Dinamico re-medido si I-53D integra antes); ADR-0036 alineado; CT-50, T-M71, T-M72 y G-M23; desde V9, contradiccion material ⇒ Proposal V10 (historial; sustituida por CR-V9-8) | §15; ADR-0036; §12; §14.1 |
| **CR-V9-1** | Huella visual como ocupacion de modelo (C:V10-01) | `VisualFootprint` en WCS construida despues de transformar; sin dilatacion por `LineWeight`, `LWDISPLAY`, CTB/STB, papel ni escala de trazado; `LineWeight` en `VisualSignature`; L-37 | §11.3 punto 5; §1.3 |
| **CR-V9-2** | Contexto de evaluacion destino y precedencia (C:V10-02, C:V10-09) | Huellas en el dibujo destino; DRAWING FIRST, LIBRARY SECOND WITH IGNORE; colision no simulable ⇒ `UNKNOWN`; la autoridad origen queda para `GeometrySymmetryResult` | §11.3 punto 4; §9.5 |
| **CR-V9-3** | Frescura tras `EnsureForPlan` (C:V10-03) | `FootprintInputFingerprint`; relectura del dibujo real; huellas, orden de plan y ST-19 re-evaluados antes de MUTATE; PRE-18 | §9.1; §9.2 |
| **CR-V9-4** | Anotaciones, cotas y read-set (C:V10-04, C:V10-05) | Intencion del plan y representacion con estado del dibujo; `DIMSTYLE`, `TEXTSTYLE`, escala de anotacion y registros de capa cuando se consumen; `MaterializationContextReadSet` | §8.7; PRE-17 |
| **CR-V9-5** | Ordenes de pantalla y de trazado; `XLINE`/`RAY` (C:V10-06, C:V10-07) | Ordenes contradictorios con copias superpuestas ⇒ E10; `XLINE`/`RAY` sin interseccion analitica ⇒ `UNKNOWN` ⇒ E10, declarado en O-1 | §4.1 ST-19; L-34; L-37 |
| **CR-V9-6** | Ayuda sin alias (C:V10-08) | Entrada en `RackCommandReference` sin alias; T-GRD-08 reapuntado verificando el alias solo si no esta vacio; ayuda +1, alias +0; `B + 1` con `B = 35` | PDC-8; T-M16 |
| **CR-V9-7** | Inventario, citas y redaccion (C:V10-10..C:V10-12) | Inventario cerrado sin propiedades sin clasificar; citas de ADR-0039 §1 y §12; notacion A/B de §11.1; redaccion de CT-50 para los valores por defecto | §6.4; §8.7; §11.3 |
| **CR-V9-8** | main, paralelas, G3 y proceso (C:V10-13..C:V10-17) | I-54 integrada y fuera del protocolo de ramas paralelas; I-49 sin cambios; I-53D con T-M17 y T-M17b; G3 ampliado; desde V10, contradiccion material ⇒ Proposal V11 | §15; §12; §14.1 |

### 18.2 Desacuerdos abiertos

**Ninguno material.** Quedan diferidos con fail-closed los asuntos de §0.3. Para la atencion del Arquitecto, sin ser
desacuerdo:

- **AR9-01: la decision vinculante del Coordinador fija la semantica.** La revision de V9 dejaba elegir entre un margen
  visual absoluto fijado en G4 y excluir las anchuras de presentacion. El Coordinador decide que la huella visual es
  **ocupacion de modelo en WCS** (§11.3 punto 5): se construye despues de transformar y no se dilata por `LineWeight`,
  `LWDISPLAY`, CTB/STB, papel ni escala de trazado. Lo que queda sin garantia se declara en O-1 (L-37); si el Owner
  exige esa garantia, es un cambio de alcance.
- **Contexto destino y precedencia (`[V10-D02]`, `[V10-D09]`).** La huella visual se resuelve en el dibujo destino,
  simulando la importacion con DRAWING FIRST, LIBRARY SECOND WITH IGNORE; la regla de la autoridad origen queda para
  `GeometrySymmetryResult` donde corresponda. Precision del ejecutor: una base auxiliar por pieza tambien es valida si
  reproduce el mismo resultado (§11.3 punto 4).
- **Frescura (`[V10-D03]`).** ST-19 antes de importar es solo rechazo anticipado; la decision definitiva se toma tras
  releer el dibujo real, con huellas, orden de plan y ST-19 re-evaluados (PRE-18). `FootprintInputFingerprint` no es
  `TraceFingerprint`. Precision del ejecutor: en PREPARE una diferencia obliga a recalcular; al inicio de MUTATE,
  cualquier diferencia respecto de esa postcondicion es excepcion y rollback (§9.2).
- **Anotaciones, cotas y read-set (`[V10-D04]`, `[V10-D05]`).** La intencion sale del plan y la representacion consume
  estado del dibujo (por ejemplo el `DIMSTYLE` actual cuando no hay estilo con nombre, `LateralHeaderDrawer.cs:391`).
  Solo lo consumido invalida; el consumo lo caracteriza CT-50 y lo no determinable es `UNKNOWN`.
- **Ordenes contradictorios y `XLINE`/`RAY` (`[V10-D06]`, `[V10-D07]`).** Son fallos conservadores deliberados y
  declarados en O-1 (L-34, L-37), no defectos.
- **Ayuda sin alias (`[V10-D08]`).** T-GRD-08 se reapunta verificando el alias solo si la entrada lo tiene; el alias
  ausente es un estado explicito; reapuntar con proteccion igual no es debilitar.
- **Terminologia (`[V10-D19]`).** «Huella de la referencia», «huella visual», `FootprintInputFingerprint` y
  `TraceFingerprint` son cuatro cosas distintas (§9.2).
- **AR7-06 y AR7-07 (desde V8).** Sigue vigente la precision del Coordinador de V8: el orden entre piezas se demuestra y
  la presentacion de la referencia fuente se conserva o falla cerrado.
- **ST-13 frente a I-51.** RACKDUPLICAR sigue copiando solo la capa: I-52 no lo cambia (G-M12). RACKMIRROR conserva la
  presentacion exterior completa por decision del Coordinador; la diferencia entre comandos es deliberada.
- **`[V8-D07]` contexto de la base auxiliar.** Si AutoCAD 2025 no permite reproducir en una base auxiliar el
  `PSTYLEMODE` de un dibujo con estilos con nombre, las piezas con cambio de mano de esos dibujos fallan cerrado (R-38);
  CT-36 lo mide con un dibujo STB y otro CTB.
- **`[V9-D03]` orden de plan.** El frontal Selectivo emite los postes antes que los largueros (§11.3 punto 6.6), pero
  bajo `μ_k` se invierten los pares de cada pasada; CT-48 los enumera y, si un kind entero pierde alcance por el orden,
  G3 abre Proposal V11 y O-1 se reevalua (R-35).
- **`[V7-D11]` continuidad demostrable.** V10 mantiene como no continuos los tipos de linea `ByLayer`, `InheritLayer` e
  `InheritBlock`. Medido en la biblioteca inspeccionada, no quita ninguna pieza estructural medida (§1.4); otra
  biblioteca podria perder piezas con curvas `ByLayer` sin anidado espejado (R-32).
- **`VisibilityPath` ordenada.** La igualdad ordenada es estricta a proposito; compararla como conjunto u otra
  relajacion solo con evidencia de G3.
- La consecuencia indicativa de §1.4 sigue sugiriendo que, con la biblioteca inspeccionada, gran parte de Dinamico y
  Push Back no se reflejaria (separadores de planta). V10 la registra sin convertirla en limitacion normativa; si G3 la
  confirma y reduce materialmente la envolvente, se abre Proposal V11 y O-1 se reevalua.
- `[V6-D14]` (paridad por recomputacion grafica) y `[V7-D14]` (punto de observacion) siguen siendo las dos condiciones
  de viabilidad de la sonda en G3, con el contexto de la base auxiliar (`[V8-D07]`): sin ellas, los bloques afectados
  fallan cerrado (X-17a).
- **I-53S e I-53D integradas (`[V10-D15]`, `[V10-D23]`).** I-53S (`e528ef2`, integrada en `104ef3a`) e I-53D (G7
  `a57bd50`, equivalente al revisado `d280197`, integrada en `dad4e77` durante la redaccion de V10) no son ramas
  paralelas: CT-26, C2-4 y T-M30 se ejecutan sobre el arbol combinado tras el rebase y, en el Dinamico, tambien T-M17 y
  T-M17b.
- **I-54 integrada (`[V10-D13]`, `[V10-D22]`).** Sale del protocolo de ramas paralelas; I-52 convive con
  T-GRD-01..T-GRD-08 sin relajarlas, y los cambios del sobre de I-54 son el contrato de CA-4 que V9 ya preveia.
- **ADR-0040 aceptado en I-49.** Reemplaza a ADR-0038 y conserva lo que I-52 consume (D19 `PlanReadSet` y el dominio del
  consumidor); V10 lo usa como autoridad vigente de I-49 (`[V10-D14]`). El Amendment A2 (`5a3714f`, solo docs) anuncia
  un ADR de reemplazo de sus D6 y D7 que no toca lo que I-52 consume (`[V10-D23]`).
- **I-55 (`[V10-D23]`, R-45).** Rama nueva y solo documental, redefinida en G1.1 como View Placement & Projection (ID17,
  ID18 e ID19): su foundation de preparacion de vistas y su proyeccion multi-rack con una transformacion comun cruzan
  conceptualmente la autoridad de planes, el materializador y la colocacion de I-52. V10 no cambia el contrato ni decide
  la reparticion: la registra para Coordinador y Arquitecto y vigila el G2 de I-55 (§15.3).
- `[V6-D29]` sigue: la guarda vigente de main impide nombrar `PropertyValues` en el Plugin y en la UI; el reparto de V5
  ya lo respeta.
- DEP-10a..DEP-10c siguen siendo predicados conservadores: claros cuya altura no usa la holgura sin override quedan
  fuera hasta que CT-44 los caracterice.

---

## 19. Condiciones de parada

- Implementar sin la orden explicita del gate: **prohibido**.
- Una caracterizacion o prueba contradice **materialmente** el contrato congelado (alcance, ADR, regla de reflexion o
  arquitectura), reduce materialmente la envolvente de O-1, o una fila `R` o `RN` resulta no equivalente: **STOP →
  Proposal V11**.
- G3 demuestra que la sonda por estado no es viable en `acad.exe` / AutoCAD 2025, que no tiene paridad con el
  materializador ni un punto de observacion soportado, que no puede reproducir el contexto de la autoridad origen, que
  las clases o variantes reales de la biblioteca no estan soportadas, que la biblioteca real reduce materialmente la
  envolvente mas alla de O-1, que el orden de plan o el de Model Space dejan fuera kinds enteros, que las huellas
  visuales de ocupacion de modelo no pueden garantizarse conservadoras o que el contexto de evaluacion destino no puede
  simularse en clases enteras, que un productor aplica el contexto de materializacion de forma asimetrica o que su
  consumo no puede determinarse, o que la canonicalizacion es materialmente ambigua: **STOP → Proposal V11** o reduccion
  de alcance.
- Necesitar cambio de schema, de sobre o de Xrecord, o de `RackEnvelopeRestamp.cs`, `PushBackMirror.cs`, `WithDesign`, un
  drawer o builder existente o el registro de variables: **detenerse**; nueva revision de Arquitecto y ADR.
- Un adaptador necesita una costura del store distinta de `WithSourceMetadataFrom`: **detenerse** (§6.2).
- Una paralela o una integracion en main modifica materialmente `RackDuplicationPlan.cs`, `RackEmbedComposer.cs`,
  `RackEmbedDocument.cs`, `RackProjectDocument.cs`, `RackProjectStore.cs`, `SystemBlockWriter.cs`,
  `LateralHeaderDrawer.cs` (incluido `ApplyDynamicParameters`), `CantileverViewMaterializer.cs`,
  `RackEnvelopeRestamp.cs`, `BlockLibraryImporter.cs`, `BlockLibrary.cs`, `DynamicRackSystemResolver.cs`,
  `SelectiveGeometryResolver.cs`, `SelectivePostGeometry.cs` (salvo miembros aditivos sin llamador),
  `SelectiveCabeceraAuthority.cs`, `SelectiveDepthLayout.cs`, `UsableProjectVariablesRegistry.cs`,
  `SelectiveLinkedPropertyKernel.cs`, los builders de §8.1, los stores authored o sus `JsonSerializerOptions`, los
  descriptores vinculables, o la autoridad del NOD de I-54 (`CustomPropertiesData.cs`, `CustomPropertiesExecutor.cs`) en
  un sentido que afecte a CA-4 o CA-4b: **detenerse** antes de editar y reportar SHA y diff. Un avance solo documental o
  una infraestructura aditiva no conectada se registra sin bloquear; la integracion de I-54 en main (`ba497f1`) modifico
  `RackEmbedComposer.cs` y `RackEmbedDocument.cs` exactamente como V9 preveia para CA-4 (miembro declarado, herencia en
  `Compose` y regla F) y se registra sin bloquear (`[V10-D13]`); la integracion de I-53D en main (`dad4e77`) llevo el G7
  revisado de la ventana y del ensamblador del Dinamico, fuera de esta lista, y tambien se registra sin bloquear
  (`[V10-D15]`, `[V10-D23]`).
- Un cambio posterior en main altera la frontera estado del editor → sistema mas alla de lo registrado en §15.1 (I-53S
  G5 `e528ef2`, integrada en `104ef3a`; I-53D G7 `a57bd50`, integrada en `dad4e77`), toca el sobre, el compositor, el
  restamp, `ProjectVariables`, el contrato de CA-4 o CA-4b, o los archivos que I-52 G7 comparte con I-54 ya integrada
  (`RackCommandReference.cs`, `SelectiveEditorOpenTests.cs`, censos de T-GRD-08) mas alla de lo registrado, o un gate de
  I-55 fija una autoridad de preparacion de vistas, de materializacion o de transformacion multi-rack que se superponga
  a las de I-52 o toca rutas de materializacion, `Compose`, comandos o `RackCommandReference.cs`:
  **reportar antes de continuar** (§15.3). Como I-53S e I-53D integraron antes del Candidato de I-52, G6/G7 no cierran
  sin CT-26, C2-4 y T-M30 verdes sobre el arbol combinado y, en el Dinamico, tambien T-M17 y T-M17b (`[V10-D15]`,
  `[V10-D23]`).
- Aparece un tipo, miembro, valor de enum, colision o descriptor vinculable alcanzado por el cierre transitivo sin
  clasificar: **RED / STOP** (§6.7).
- CT-38 encuentra un simbolo efectivo o un parametro dinamico fuera del cierre declarado de un descriptor: **RED / STOP**.
- Se intenta admitir un cambio de mano sin `GeometrySymmetryResult` visual-geometrico verificado, admitir una clase por
  herencia o una variante sin clasificar, comparar valores visuales resueltos o `Explicit` sin su metodo en lugar de
  fuentes simbolicas tipadas, emparejar primitivas con `VisualSignature` (incluida `SourceKind`), `VisibilityPath` u
  orden visual distintos, emparejar una polilinea con `Line` o `Arc` sueltos, usar nombres anonimos en una ruta anidada,
  aceptar un par material entre piezas con orden no conservado, tratar una huella visual `UNKNOWN` como no superpuesta,
  usar `Extents3d` sin declaracion, dilatar una huella por `LineWeight`, `LWDISPLAY`, CTB/STB, papel o escala de
  trazado, construir sus regiones antes de transformar a WCS, resolverla con el contexto de la biblioteca, usar
  `TraceFingerprint` como `FootprintInputFingerprint`, fusionar familias distintas o primitivas sin continuidad
  demostrable al canonicalizar, derivar un centro de planes o de la caja envolvente sin verificar, inferir universalidad
  con muestras finitas o interpretar grafos de acciones de bloques dinamicos: **prohibido** (G-M15, G-M17, G-M21,
  G-M23).
- La sonda escribe en el dibujo del usuario o en la cache de biblioteca, accede a la cache por reflexion o por una ruta
  distinta de las dos autorizadas (§11.3 punto 14), usa una precedencia library-first, evalua la evidencia de simetria
  sin el contexto de la autoridad origen o la huella visual sin el contexto de evaluacion destino, deja
  `WorkingDatabase` cambiada, aplica parametros con una semantica distinta de la del materializador sin la prueba T-M56
  o lee valores antes del punto de observacion caracterizado: **prohibido** (G-M16, G-M19, G-M20).
- Se intenta crear una copia sin la presentacion exterior de su referencia fuente, asignar un estilo de trazado
  independiente en un dibujo CTB, sustituir ST-18 por un aviso, mutar sin demostrar el orden en Model Space frente a
  **todo** objeto posterior a cada fuente (ST-19), mutar sin refrescar las huellas visuales tras `EnsureForPlan`
  (PRE-18) o materializar con una observacion consumida del contexto de materializacion distinta de la capturada
  (PRE-17): **prohibido**.
- Un archivo de I-52 llama a las autoridades o al commit fisico de Custom Properties, o copia, refleja, transforma o
  escribe CA-4b: **prohibido** (G-M22).
- Se usa en `DEP-ROW` una cota inferior que no es autoritativa: **prohibido**.
- Un tipo de I-52 aparece en `RackCad.Application.Systems.Shared` o un archivo de I-52 viola una guarda de texto vigente:
  **RED** (G-M14, G-M18).
- Aparece un parametro dinamico con semantica de lado sin regla: fail-closed y registro para Proposal V11.
- G7 intenta consumir la autoridad sin C-2 verde (incluido C2-2b): **prohibido**.
- Abrir G4 sin ADR-0036 aceptado o con alguna fila `G3_PENDING`: **prohibido**.
- Debilitar una guarda de I-51, de main (incluidas las de I-53 y las de I-54, T-GRD-01..T-GRD-08) o de I-49 para
  facilitar el cambio: **prohibido**; reapuntar con motivo y proteccion igual o mayor no es debilitar (T-M16: el alias
  de la ayuda se verifica si existe y su ausencia es un estado valido).

---

## 20. Archivos (sin cambiarlos en este gate)

### 20.1 Produccion probable (G4–G8)

| Capa | Archivo o area |
|---|---|
| Application | `Geometry/Transform2D.cs` (`ReflectionAboutLine`) y valor de colocacion junto a el |
| Application | `Persistence/RackDuplicationPlan.cs` (fachada, con aviso a I-49) + nucleo neutral nuevo en `Persistence/` |
| Application | carpeta nueva del espejo `src/RackCad.Application/Mirror/` (namespace `RackCad.Application.Mirror`; **nunca** `Systems/Shared`): contrato de reflector, registro, plan del espejo, decodificacion de vistas y `A_k`, equivalencia, contrato de datos `GeometrySymmetryResult` (con valores efectivos, variantes, fuentes visuales simbolicas tipadas, `VisibilityPath`, contexto de patron y orden visual) y su consumo, orden visual de plan sobre huellas (§11.3 punto 6.6) y consumo de `VisualFootprintResult`, declaracion de invariancia de metadata, allowlist de portadores, registro de retirados, autoridad V-DEP comun y tablas de cobertura del exterior; nombres que respetan las guardas de texto vigentes (G-M18) |
| Application | `Systems/Selective/`, `Systems/Dynamic/`, `Systems/PushBack/`, `Systems/Cantilever/`, `RackFrames/`: reflector, tabla de clasificacion y autoridad de plan por kind |
| Plugin | `RackMirrorCommands.cs` (nuevo) y SNAPSHOT propio; materializador en `Systems/Shared/`; `Systems/Shared/SystemBlockWriter.cs` (`CreateInTransaction`); **sonda visual-geometrica por estado** (solo lectura o `Database` auxiliar privada: contexto de la autoridad origen para la evidencia de simetria y `DestinationEvaluationContext` con DRAWING FIRST para la huella visual; definiciones de biblioteca solo por `BlockLibraryImporter.EnsureBlocks` y verificacion de bloques), politica cerrada de variantes, canonicalizacion conservadora, fuentes visuales simbolicas tipadas, `VisibilityPath`, contexto de patron, ruta anidada estable, orden visual, transformacion efectiva y `TraceFingerprint`, en G6 o G7 (§11.3); presentacion de la referencia fuente (ST-13, ST-18), orden en Model Space (ST-19) con relectura del dibujo real y frescura de las huellas tras `EnsureForPlan` (PRE-18) y `MaterializationContextReadSet` (§8.7) en el SNAPSHOT, PREPARE y `CreateReference`; productor unico de huellas visuales de ocupacion de modelo con su `FootprintInputFingerprint` (`VisualFootprintResult`, G-M23); costura unica de aplicacion de parametros compartida con el materializador si es viable (G-M19) |
| Plugin | `RackDuplicarCommands.cs` **solo** si se extrae un SNAPSHOT compartido; comandos de `RACKEDITAR` **solo** si se activa C-1 |
| UI | `RackCommandReference.cs` (entrada de ayuda de `RACKMIRROR` sin alias) |

### 20.2 Pruebas

Caracterizaciones CT-01..CT-50; pruebas T-M01..T-M72 (con T-M17b); guardas G-M1..G-M23; evidencia de viabilidad de la
sonda en `acad.exe` y corpus indicativo con la huella de la biblioteca en `docs/automation/evidence/` y, si hace falta,
un harness en `tools/` (nunca en `src/` durante G3); `SelectiveEditorOpenTests.cs` (censo `B + 1`), los censos de
T-GRD-08 de I-54, ya en main, reapuntados con motivo (`CustomPropertiesCommandGuardTests.cs`,
`CustomPropertiesHelpCensusTests.cs`) y el censo T-GRD-02 de `CustomPropertiesEnvelopeGuardTests.cs` clasificado con
motivo (7 → 8); guardas de I-51 reapuntadas en `SelectiveDuplicationFailClosedTests.cs` solo si se extrae el SNAPSHOT;
pruebas de UI en `tests/RackCad.UI.Tests` para C-2 (G6) y las caracterizaciones de UI (G3).

### 20.3 NO TOCAR sin nueva revision de Arquitecto

`RackEnvelopeRestamp.cs`, `RackCloner.cs`, `PushBackMirror.cs`, `SelectivePalletDesign.cs`,
`SelectivePalletDesignDocument.cs` (incluido `WithDesign`), DTO y stores de todos los kinds, `RackProject.cs`,
`LateralHeaderDrawer.cs` (incluido `ApplyDynamicParameters`), `CantileverViewMaterializer.cs`,
`BlockLibraryImporter.cs`, `BlockLibrary.cs`, builders de plan, `ProjectVariables/*` salvo consumo de lectura, `Bom/*`
salvo consumo, `Catalogs/*` salvo consumo (I-19), `KindHandlers/*`, editores WPF, `assets/`, catalogos, biblioteca de
bloques, `.github/`, ADR aceptados, y la baseline de I-53 en main: `src/RackCad.Application/Systems/Shared/Header*` con
sus guardas (`HeaderBatchContractTests.cs`, `HeaderConfigurationFixtures.cs`) y sus consumidores
`Systems/Selective/SelectiveHeader*`, `SelectivePostTargets.cs`, `Systems/Dynamic/DynamicHeader*`,
`DynamicModuleTargets.cs` y `DynamicRackRebuild.cs`; tampoco las guardas de texto de
`ProjectVariablesConformanceTests.cs` ni, al reconciliar, el nucleo de I-49 en `RackCad.Application.Expressions`, ni la
autoridad de Custom Properties de I-54, ya en main (`RackCad.Application.CustomProperties`, `CustomPropertiesData.cs`,
`CustomPropertiesExecutor.cs`, `RackPropiedadesCommands.cs`, `RackCustomPropertiesWindow`), y sus guardas
T-GRD-01..T-GRD-08 (T-GRD-02 y T-GRD-08 solo se reapuntan con motivo; T-M16).

---

## 21. Estado

```text
G0 = ACCEPTED        G1 = ACCEPTED (Discovery)
G2 = V10 EN REVISION (este documento + ADR-0036 propuesto actualizado + docs/automation/decisions/I-52.md)
     V1 (0fc7032), V2 (0445718), V3 (545c222), V4 (8e2ae4f), V5 (e998a2b), V6 (91bdd38), V7 (b70b5bf), V8 (e0a779f)
     y V9 (deb08cd) = historial; revision de V9: Architect: CHANGES REQUIRED — PROPOSAL V10
O-1 = PENDING        (envolvente de §1.3, L-01..L-37; se pide despues del freeze tecnico y antes de G3; no en este gate)
G3 = NOT OPEN        (requiere consenso sobre V10, rebase/reconciliacion sobre main, freeze, re-check y O-1)
G4+ = NO INICIADO    (G4 requiere ademas G3 sin contradiccion material, ninguna fila G3_PENDING y ADR-0036 aceptado)

Proposal Version = V10
Coordinator = REVIEW REQUIRED
Architect = REVIEW REQUIRED
Consensus = NOT REACHED
ADR-0036 = PROPOSED
O-1 = PENDING
G3 = NOT OPEN
Implementation = BLOCKED

SUBSTANTIVE IMPLEMENTATION: BLOCKED
```
