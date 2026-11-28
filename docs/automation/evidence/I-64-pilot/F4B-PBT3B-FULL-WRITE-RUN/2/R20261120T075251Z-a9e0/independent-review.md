# Revisión adversarial independiente del código — I-64 F4B-PBT3B-FULL-WRITE-RUN, intento 2

**Objeto:** `git diff c7441653f3c1c8a27448ebec8f65f72a2f295f59 c758278548bcdd66e7c55ca15ca1faeb1a431e53` (BaseSha = ChainBaseSha `c7441653`; RED `9fda277e`; GREEN `c7582785`), WorkRunId `R20261120T075251Z-a9e0`. Autoridades leídas por objetos git en `c7582785`: el contrato (`docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json`), el paquete v4.6 (`docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md`, §3, §5.0, §5.3, §6.0 tabla de RR-3, §6.2), `superseding-characterization.md` (fila PB-T3b), el análisis del intento 1 (C-1 a C-4) y F4B-E45. La entrega se leyó en `rv/` (handoff, registro del relevo, TRX de CI y locales, `inv/`).

Todo el código y los documentos se leyeron con `git show`/`git diff`/`git grep`/`git ls-tree` desde el checkout principal. No se leyó, listó ni ejecutó nada en el worktree de la rama.

## Veredicto

**ACEPTABLE CON MENORES**: **B 0 / I 0 / M 9**.

El código hace lo que dicen D-PBT3-09 y D-PBT3-10 con la variante (A) y los nombres de F4B-E45. El piloto queda equivalente salvo la barrera. El RED y el GREEN cumplen su forma, y la medición de INV-PBT3B-15 es la de (A). No encuentro ninguna mutación de las dos referencias de D2. Los menores son de fidelidad de recetas, de determinismo de una línea y de registro de la entrega. Ninguno pide cambiar `src/`.

## Hallazgos

| Id | Sev. | Dónde (`sha:ruta:línea`) | Hallazgo | Qué se pide |
|---|---|---|---|---|
| **M-01** | M | `c7582785:tests/RackCad.Tests/PushBackFullWritePrepareTests.cs:429-448` (CT-04 iii/iv); `:705` y `:132-138` (CT-11, `Fabricated`); `:513-521` (CT-05, nombre en blanco) | **Tres recetas no son las del §5.3, y `Deviations` está vacío.** (a) CT-04 (iii)/(iv): el paquete dice `pb-20-ab` con B quitado. La prueba usa el rack compuesto de CT-PB0-10 (`pb-01-base` con A 1-2 en blanco y B 1 activa). (b) CT-11, parte principal: la relectura se fabrica, porque AUTH-13 rechaza la versión `1.1` (`AuthoredRawReader.cs:133-141` solo admite `"1.0"`). El paquete solo manda fabricarla en la parte «por construcción», y esa línea no lleva el sufijo. (c) CT-05, nombre en blanco: relectura fabricada, porque el lector rechaza nombres distintos (`PushBackPilotIntentReader.cs:50`, `DivergentDiagnostic`). Las tres están declaradas solo en `UnexpectedFindings`. Ninguna línea esperada ni ningún oráculo cambian, y el §2 no fija el hecho de `pb-20-ab`, así que no veo PBT3B-C07 por su letra. | La sesión acepta y registra las tres lecturas en la verificación, o pide su registro. No hay que tocar código. |
| **M-02** | M | `c7582785:tests/RackCad.Tests/PushBackFullWritePrepareTests.cs:564`, `:621`; producto `src/RackCad.Application/Systems/PushBack/PushBackCompositeDiagnostics.cs:103-107` | **CT-06 (i) «hueco-negativo» depende de la cultura del proceso.** El texto se forma con `CultureInfo.CurrentCulture` (`{0:0.##}` de −1), y la línea esperada lleva `-1` literal. CT-PBT1B-12, que la receta copia, fuerza `CurrentCulture = InvariantCulture` (`tests/RackCad.Tests/PushBackEditorOutputGateTests.cs:69-74`). CT-06 no lo hace. Con una cultura cuyo signo negativo no es `-` (sv-SE usa U+2212), la prueba falla. Choca con INV-PBT3B-09 («cultura invariante»). Pasa en la CI y en local. | Registrar la limitación. Si se corrige, fijar la cultura invariante alrededor de la receta, en un archivo nuevo de la tarea. |
| **M-03** | M | `c7582785:src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdateRun.cs:171-186`, `:210-225`, `:415`, `:424` | **El reparto de los delegados no es el de la letra de D-PBT3-10.** El paquete pone «la entrada y la planificación del modelo» en `_prepare`, y «el texto del origen» en `_originOf`. En el código, `_originOf` crea la instantánea (`From`) como efecto lateral en un local capturado (`input`), y `_prepare` ignora su parámetro `DraftSession<TState>` (`_`) y usa ese local. Conserva el momento de la base (la instantánea antes de `ReadForPrepare`, como `_input` en `c7441653:…/PushBackPilotUpdateRun.cs` `Prepare`), así que es lo más fiel a INV-PBT3B-04. Pero acopla las dos lambdas por orden de llamada. | Registrarlo como lectura del diseño. No pide cambio. |
| **M-04** | M | `c7582785:tests/RackCad.Tests/PushBackPilotUpdateEquivalenceGuardTests.cs:108-134` | **E-3 es algo más débil que la letra del §5.3.** (a) La aguja del paquete `reread.StaleBase.Entries.Values.Where(...)` en `BuildCore` no es alcanzable, porque `BuildCore` recibe `rereadBase`. E-3 la adapta (`:115`), pero no comprueba que `Build` y `BuildFull` pasen `reread.StaleBase` en esa posición. Un cruce con `input.StaleBase` no la dispararía. Tras `IsMatch` el contenido es el mismo, así que el riesgo es bajo. (b) La cláusula «el nombre sale de la única expresión de E-1» no está en E-3. La cubre E-1 (`:56-61`). | Registrar la adaptación de la aguja. |
| **M-05** | M | `c7582785:tests/RackCad.Tests/PushBackFullWritePrepareTests.cs:567-580` | **CT-06 (i) «celda-no-cabe» no es la receta de CT-PBT1B-12.** Allí es el diseño de `Blocking()` (`PushBackEditorOutputGateTests.cs:31-38`). Aquí es `pb-20-ab` con `SetStructureOverride(A, 2)`. El oráculo, igual al de CT-PBT1B-12, no es literal, y la línea no prueba que el bloqueante sea el de la celda: solo que es el primero de `Evaluate(system)`. Los otros tres bloqueantes usan `pb-01-base`, donde el paquete de PB-T1 usaba `pb-20-ab`. Sus textos literales sí coinciden con CT-PBT1B-12. | Registrar, o fijar en la línea el código del diagnóstico. |
| **M-06** | M | `c7582785:tests/RackCad.Tests/PushBackFullWriteRunTests.cs:274-288` | **CT-09: el piloto se compara en 2 casos, no «en los mismos casos».** El §5.3 pide que el piloto, con un plan sin borrados, dé lo mismo que hoy en los mismos casos. La prueba cubre solo «todo igual» y «escala distinta». Faltan las claves distintas y el sobre distinto. Las pruebas de F4 de `ComparePost`, en verde, mitigan el hueco. | Registrar. |
| **M-07** | M | `PushBackFullWritePrepareTests.cs:209-240`, `:279-292`, `:331-344`, `:394-404`, `:548-556`; `PushBackFullWriteRunTests.cs:81-89` | **La regla del §5.0** («una receta que necesita el borrador en Dirty lo comprueba con `IsDirty` justo antes de la operación») **solo se cumple en CT-01 (i), CT-07 (ii), CT-10, CT-11 y CT-15.** En las demás, un gesto inerte se vería solo de forma indirecta: la línea esperada (`prepare=Ready`) dejaría de coincidir. | Registrar. |
| **M-08** | M | `c7582785:tests/RackCad.Tests/PushBackWritePathGuardTests.cs:234-236`, `:277` | **CT-13 (ii) detecta «`Precheck` con un `DraftSession<PushBackFullDraftState>`» solo si el tipo aparece en el texto de la llamada.** La autoprueba es una forma artificial (`full as DraftSession<…>`). Una llamada con una variable ya tipada no se detectaría. Lo mitiga CT-PBT3A-12, que impide nombrar `PushBackFullDraftState` fuera de `Draft/`, PP y UR. | Registrar. |
| **M-09** | M | `rv/worker-handoff.json` (`WorkCompleted`, `TestsExecuted`); `rv/tests/g0/`, `rv/tests/d0/`, `rv/tests/t0/` | **La entrega no cuadra del todo con los hechos.** (a) Dice «tres commits sobre BaseSha», pero `git log c7441653..c7582785` da dos (RED y GREEN). (b) Las corridas `g0/a`, `b` y `c` (02:44-02:45 local, sobre el texto del GREEN, antes de `m1`) no están en `TestsExecuted` ni se citan. `d0` (ZzDiagTests) y `t0` se citan en `Evidence`, pero no en `TestsExecuted`. (c) Cronología: commit RED 02:40:52, y los 21 métodos nuevos en verde a las 02:44:46. El GREEN se redactó antes del commit RED, o en menos de cuatro minutos. Ninguna regla lo prohíbe, y el esqueleto del RED es exacto. | Que la verificación lo anote (INV-PBT3B-11). |

## Diseño → código

| Punto del diseño | Código (`c7582785`) | Estado |
|---|---|---|
| D-PBT3-09: `PushBackFullPrepareInput`, gemela, un `From`, estados copia | `PushBackPilotUpdatePlan.cs:79-131`. `InitialDraftState` es copia por el getter (`DraftSession.cs:284`). `Current`, por `PushBackFullDraftStateComparer.Copy` (`:126`) | OK |
| `PrepareFull`, con el mismo orden del piloto y detrás del `Prepare` del piloto | `:445-470` (el `Prepare` del piloto está en `:413`) | OK |
| `Build` conserva `ComposeForWrite` tras `.CompareWith(` | `:480` | OK |
| `BuildFull` → `PushBackFullComposition.ForFullWrite(` | `:500` | OK |
| `BuildCore`: (a) rechazo | `:531` | OK |
| (b)-(c) miembros releídos en ordinal, origen y fuentes interiores, sin cambio | `:534-558`, idéntico a `c7441653:…:345-373` | OK |
| (d) diseño y sistema; (d2) barrera para los dos modelos | `:567-569`, `:577` (`BlockedPrefix + StatusOr(…, "")`) | OK |
| (e) disposición. Piloto: no `Redraw` → `ViewUnavailable`. Completo: `FrontalBOfOneWay` → Invalid, `ObsoleteLateral` → `Erasures`, `AllObsolete` → Invalid | `:570`, `:583-601`, `:629`. `Redraw` ⇔ `Evaluate == Available`, con los mismos hechos que la base (`Draft/PushBackViewDisposition.cs` `Of`) | OK |
| `Erasures` solo si se admite borrar y solo con `ObsoleteLateral` | `:592-596`, un único `erasures.Add` | OK |
| (f) el nombre en una sola expresión | `:623` (`IsNullOrWhiteSpace`, como `RackPushBackCommands.cs:188`) | OK |
| (g) un compositor, `ComposeMemberEnvelope(source, name, …)` | `:625`, `:642-646`. Un solo `RackEmbedComposer.Compose(` | OK |
| (h) `PostOriginKey` reanclado a la primera `Redraw` | `:633`; `plans[0]` existe siempre (lo garantizan `AllObsolete` y los Invalid) | OK |
| `Erasures`, `PostOriginKey` y `ErasedKeys` en `Equals`, solo lectura; ctor existente conservado | `:243-267`, `:282-363` | OK |
| D-PBT3-10: API existente intacta; `Precheck` sobrecarga; `ExecuteFull`, `ComparePostFull` y dos constantes | `PushBackPilotUpdateRun.cs:120-122`, `:130-157`, `:162-255` | OK |
| `Execution<TState>`: sale `_input`, entran 4 campos; delegados como lambdas; patrón M-01 | `:325-356`, `:171-194`, `:210-233` | OK (M-03) |
| MUTATE: capacidad primero, destino `Members ∪ Erasures`, superviviente fuera de `Erasures`, capas de las dos, `WriteUnit` y luego `EraseUnit` en ordinal, un `Seal` | `:505-506`, `:522-529`, `:659-671`, `:534-551` | OK |
| Un `BeginMutation`, un `Seal`, ámbitos en `finally` propios | `:458`, `:550`, `:383-396` y `:477-487` | OK |
| POST relee con `PostOriginKey`; `Applied(read)` | `:603-608` | OK |
| `ComparePostCore`: borradas antes que las claves; escala del modelo | `:259-281` | OK |
| `IPushBackPilotViewEraser` en archivo propio, sin `Transaction` ni tipos del modelo | `IPushBackPilotViewEraser.cs` (igual en RED y GREEN). `git grep Transaction` en `Pilot/`: 0 | OK |
| UP sin cambio; Plugin, WHC, Domain y UI sin cambio | el diff toca solo las 11 rutas; UP es idéntico en las tres SHA | OK |
| C-1: `PrepareFull`, `ExecuteFull` y `ComparePostFull`; ninguna sobrecarga de `Prepare`, `Execute` ni `ComparePost` | censo propio igual al de `inv/inv-current.txt`. Compilan `WorkspacePilotUpdateRunTests.cs:669` y `:799` (sin tocar), y `ThePlanTypesLiveInThePilotNamespace…` está en Passed en la CI del GREEN | OK |
| PBT3B-C13: lista cerrada | PP +22, UR +10/−1 (`_input`), interfaz +2, y nada más. `ComposeMemberEnvelope` gana `name` y sigue siendo único. `Execution` → `Execution<TState>` | OK |
| INV-PBT3B-03 | todos los miembros públicos e internos de PP y UR de `c7441653` siguen tal cual. Solo cambian los `public` de la clase privada `Execution` (Run, OutcomeOfAnEscape y el ctor), y eso lo prescriben el paquete y C13 | OK |

**Piloto (INV-PBT3B-04):** mismo orden y mismos textos en `Prepare`. Las mismas llamadas al puerto: con `Erasures` vacía no hay `EraseUnit`, y la capacidad es un `as`, no una llamada. El mismo `ComparePost`, más la comprobación de borradas, vacía. `PostOriginKey = OriginKey`. Las únicas diferencias que encuentro son internas e inalcanzables: un fallo de `Evaluate` pasa de `PrepareFailurePrefix` a `SystemFailurePrefix`, y `ComparePost` con `InitialState` nulo pasa de una NRE a `POST_SCALE_DIFFERS`. `Read` nunca da una lectura admitida sin estado (`PushBackPilotIntentReader.cs` `Read`).

## D2 (lo más importante)

- **Las dos referencias declaradas:** `PushBackEditorState.WorkingBaseline` (`PushBackSystem`, `PushBackEditorState.cs:71`) y `LastModuleReconciliation` (`RackModuleReconciliationResult`, `:143`).
- **Ninguna copia nueva comparte otro objeto mutable.** El diff no añade ninguna función de copia. `PushBackFullPrepareInput.From` usa la copia del getter y `PushBackFullDraftStateComparer.Copy` de PB-T3a. El plan guarda solo cadenas, números y listas copiadas (`Array.AsReadOnly(x.ToArray())`).
- **Ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado.** En `BuildCore`, el sistema es nuevo (`Resolve`). `IsBlocked`, `StatusOr` y `Of` solo leen (`PushBackEditorOutputGate.cs`, `PushBackViewDisposition.cs`). UR no toca sistemas.
- **CT-12 lo mide de verdad.** Las referencias se capturan antes de `PrepareFull` (`PushBackFullWriteRunTests.cs:426-428`), y la huella es la de CT-PBT1B-16 (`PushBackEditableStateCopyTests.cs:1080`). Solo le falta el sufijo diagnóstico `Name=[…]`, y `Name` sigue en la huella a través de `Structure.Name`.
- **CT-13 (i)** usa las reglas de G-L1 copiadas byte a byte de CT-PBT3A-13, con las mismas 8 autopruebas, sobre PP, UR y la interfaz.
- **PBT3B-C17 no se dispara.**

## Los CT

| CT | ¿Fija lo del §5.3 con su oráculo? | Notas |
|---|---|---|
| 01 | Sí. (i) lee 4.5 y 48.5 de vuelta. (ii) las 11 filas, idénticas a `SC1.FilasF()` (`PushBackDeclaredCorrectionSupport.cs:49-62`), y las 2 de estilo con `BuildForMember` | `diseno=True` compara contra la composición, un oráculo mecánico que prescribe el paquete. Falta `IsDirty` en (ii) (M-07) |
| 02 | Sí, los 4 casos, con «claves-nuevas=[]» | — |
| 03 | Sí: la receta F-01 del paquete de PB-T1 (`pb-20-ab`, `SetLength` + 2,0 sin prefijo `B:` y con él), leída de vuelta | — |
| 04 | (i) y (ii) sí. (iii) y (iv) con otra base | M-01 |
| 05 | Sí. Nombres del clásico, compuesto, blanco → origen, y el piloto conserva el de cada sobre | Blanco con relectura fabricada (M-01) |
| 06 | (i) 3 literales y 1 calculado. (ii) la línea literal del paquete, byte a byte, que se comprueba a sí misma (`admitido` y `bloqueado` están en la línea). (iii) los 15 del corpus, con `Materialize` en la línea | M-02, M-05 |
| 07 | Sí. La secuencia literal, Applied, y la base y el estado inicial leídos tras `CompleteApply`. Segundo `PrepareFull` con `IsDirty` justo antes | El momento de cada lectura se respeta |
| 08 | Sí, los 4 casos (más la capa bloqueada en PREPARE) | — |
| 09 | Completo sí, con las constantes por nombre (RR-3 lo admite) | Piloto parcial (M-06) |
| 10 | Sí, las 4 líneas literales y ` base-igual=True dirty=True`; se comprueba a sí mismo | — |
| 11 | Sí, con el sufijo en la parte por construcción | Parte principal fabricada (M-01) |
| 12 | Sí, la línea literal, con las referencias capturadas antes | — |
| 13 | Sí, dos `[Fact]`, con 24 + 8 autopruebas que discriminan (pasan en el RED) | M-08 |
| 14 (E-1 a E-6) | Cada uno empieza afirmando la forma nueva. E-1, E-2, E-4, E-5 y E-6 cubren S-1, S-2, S-4, S-5 y S-6 (comparados con `ComposeViolations` y `RunEscapeViolations` de `c7441653`). E-2 y E-6 empiezan con `Assert.Empty(E1/E5Violations(real))`. E-5 exige las dos `Execute` | E-3: M-04 |
| 15 | Sí, los 8 casos del orden y del `Precheck` | — |

No encuentro aserciones tautológicas que oculten un fallo. `diseno=True` (CT-01) y `celda-no-cabe` (CT-06) usan oráculos mecánicos. Tampoco hay gestos que escriban el mismo valor: los valores son distintos de los del corpus (§5.0), y una edición inerte haría fallar la línea esperada.

## El RED (`rv/red-ci-core/core.trx`, `rv/red-ci-ui/ui.trx`)

- **El esqueleto es el exacto del §6.2.** `PrepareFull` → `WithInvalid(MissingInputReason)`; `Precheck` → `Go()`; `ExecuteFull` → `NotAppliedFailure(UpdateNotRunReason)`; `ComparePostFull` → `PostRereadMissingReason`. Los ctores y las constantes están completos, el piloto no cambia y la interfaz es la final.
- **Core:** 19 124 resultados, 19 099 Passed, 19 Failed y 6 NotExecuted. Los 19 son los 13 de `PushBackFullWrite` y los 6 de E-1 a E-6, todos con `Assert.*` y la primera línea del stack en `tests/`. No hay ningún otro fallo.
- **CT-13:** 2 de 2 en Passed, con sus autopruebas.
- **Los 6 NotExecuted** son exactamente S-1 a S-6, con el motivo literal; la `Theory` S-2 cuenta como una fila.
- **Workspace:** 3 478 seleccionados, 3 472 Passed y 6 omitidos.
- **UI:** 2 451, 2 430 Passed y 21 omitidas preexistentes, igual que en el GREEN.
- **Sin RR:** hay dos commits, y `git diff 9fda277e c7582785 -- tests` está vacío.
- **El diff `-U0` de las tres guardas** son solo las 6 líneas de atributo, cada una en su S-n (`:121`, `:127` y `:229`; `:126`; `:895` y `:902`).
- **INV-PBT3B-15** (`rv/tests/m1/m1.trx`): S-1, S-3, S-4 y S-5 en Failed. S-2: filas 1 y 3 en Failed con `from-presente=False`, y 2, 4 y 5 en Passed. S-6 en Passed en vacío. Es lo esperado de (A).

## El GREEN y la estabilidad

- **Core:** 19 124 resultados, 19 118 Passed, 0 Failed y 6 omitidos. Es «+ 21» con la `Theory` como una fila: 19 107 + 21 − 4. **UI:** 2 451 / 2 430 / 21, sin cambio.
- **Los filtros:** 13, 6 y 2 en Passed, y Workspace en 3 472 + 6.
- **Tres corridas locales consecutivas en verde** (`rv/tests/g2`).
- **Orden y estado estático:** no encuentro dependencias de orden ni de estado estático mutable.
- **Cultura:** una dependencia (M-02).

## Contención

- **Ni ventanas, ni modales, ni `MessageBox`.** Tampoco el configurador, «Insertar», el BOM ni la biblioteca: todo es Core, con dobles.
- **Escrituras:** ninguna prueba escribe en disco. CT-13 y E-n solo leen `src/`.
- **Las corridas de UI locales** son exactamente los 11 filtros del contrato, en el RED y en el GREEN, sin filtros de método ni la suite completa.
- **Clases temporales:** `ZzDiagTests` y `ZzMeasureTests` son de Core y no entraron en ningún commit.
- **Áreas sin tocar:** el Plugin, Domain, WHC, el puerto del panel y UP no cambian. AutoCAD no aparece en la entrega.

## Hallazgos para el Owner

- **OWNER-1 (informativo; no pide decisión).** No encuentro ninguna diferencia observable del piloto fuera de la barrera. Hay un matiz que la barrera ya cubre (V2-09): en un diseño bloqueado que antes fallaba más tarde en `Build`, con otro `Invalid` (`ViewAddress…` o `ViewUnavailable…`), el panel muestra ahora el texto de la barrera.
- **OWNER-2 (informativo).** Hay dos hechos de AUTH-13, anteriores a PB-T3b, que limitan el uso real:
  - un sobre con `SchemaVersion` `1.1` no se admite (`AuthoredRawReader.cs:133-141`);
  - un rack con sobres de nombres distintos tampoco (`DivergentDiagnostic`).

  Por eso, «conservar otra versión menor del sobre» (D-PB-05, pasos 5 a 7; CT-11) y «nombre en blanco → el del origen» con nombres distintos (CT-05) solo se prueban por construcción. No es una regresión, pero el Owner puede querer saber que esa rama de D-PB-05 no se alcanza por el lector.

## Lo no verificado

- **Sin ejecución:** no ejecuté dotnet ni ninguna prueba. Las cifras salen de los TRX entregados, no de una corrida mía.
- **CI:** el estado en GitHub (runs 44704503614 y 44707482956, 4/4) lo tomo de `rv/relay-record-work.json`; no lo consulté.
- **`pb-20-ab`:** no verifiqué la afirmación del Worker de que en ese fixture ningún lateral existe solo por B (M-01).
- **Limpieza:** la prueba de limpieza (`clean/before` y `after`) no está en `rv/`.
- **Mensajes del RED:** xUnit los trunca en la CI. No puedo descartar una línea `EXCEPCION` tras la primera diferencia, aunque el texto visible no la tiene.
- **Línea base:** los 19 107 de Core (CI 44668584248) salen del paquete; no tengo su TRX.
- **AutoCAD y ventanas:** que no se abrieran en local lo juzgo solo por indicios (proyecto Core, filtros de UI del contrato).
- **Incidencias de esta revisión:**
  - dos invocaciones de `python -c` sin efecto (`print(1)` e `import sys`), que no leyeron ni escribieron nada, contra la regla del encargo;
  - una copia temporal de un objeto git en el `/tmp` de Git Bash, ya borrada.

  Nada se escribió en el repositorio ni se tocó el worktree.
