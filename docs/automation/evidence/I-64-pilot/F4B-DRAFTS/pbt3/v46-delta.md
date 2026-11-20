# I-64 F4B PB-T3 — Delta del paquete, v4.5 → v4.6 (la decisión del Owner F4B-E45 y la comprobación acotada de PB-T3b)

**Qué es:** la v4.6 del paquete del contrato de PB-T3, `I-64-f4b-pbt3-gate-contract-package-v46.md`, desde la v4.5 revisada (282 234 bytes, sha256 `64ab03df…`). La arma `make_v46.py`: cada cambio es una sustitución o una inserción exacta, que tiene que aplicar una sola vez. `check_v46.py` la comprueba, con su salida en `check-v46-out.txt`.

**La v4.6 tiene dos tramos:**
- **el borrador** (V46-01 a V46-05; sha256 `5f8516c3…`): las menores M-01 a M-03 de CA-T3b y sus dos avisos, solo de texto y para PB-T3c;
- **la corrección de PB-T3b** (V46-06 a V46-10): la decisión del Owner F4B-E45. Con ella, **la v4.6 es el paquete de la reemisión de PB-T3b, en el intento 2 de 3, y el de PB-T3c.**

## Por qué

**CA-T3b** (`F4B-DRAFTS/pbt3/independent-review-pbt3b-check.md`) dio **EMITIBLE, B 0 / I 0 / M 4**, y su §12, sobre el seco r2, **EMITIBLE, B 0 / I 0 / M 1**, sin importantes ni decisiones nuevas del Owner. M-01 a M-03 pedían una v4.6; M-04 y M-05 se resolvieron en la emisión de PB-T3b (F4B-E44).

**La parada del intento 1 de PB-T3b.** El Worker `R20261120T055235Z-a45b` (delegación `R20261120T054837Z-814c`) paró con PBT3B-C02 y PBT3B-C07 antes del commit RED, sin commits. Con el esqueleto del §6.2 de la v4.5, `RackCad.Tests` no compila: `Execute(null!, …)` (`WorkspacePilotUpdateRunTests.cs:669`) y `ComparePost(plan, null)` (`:799`) son ambiguas (CS0121) entre la sobrecarga nueva del modelo completo y la existente. **Es un defecto del paquete:** las firmas no cambian, pero la sobrecarga vuelve ambiguo un `null`, y el barrido del §4.1 de la v4.5 solo miraba el texto. El Owner eligió «Renombrar los métodos nuevos (Recomendado)» (F4B-E45). Los nombres que fija la sesión son `ExecuteFull` y `ComparePostFull`.

## Lo que no cambia

- el diseño, D-1, ninguna guarda, ninguna prueba y ninguna firma existente;
- el bloque citado de `PBT3x-C11`: plegado, es letra a letra el `PBT3B-C11` del contrato emitido;
- en el contrato reemitido de PB-T3b, la `ExpectedEvidence`, los `RequiredTests`, los invariantes, el alcance y la línea base. Cambian el `Objective` («v4.6»), `PBT3B-C13` (los nombres) y lo que cambia en toda reemisión: las autoridades, `IssuedBy` y las revisiones. `check_v46.py` lo comprueba sobre las viñetas que el generador copia.

## Los cambios

| Id | Origen | Qué cambia | Dónde |
|---|---|---|---|
| **V46-01** | **CA-T3b M-01** (F4B-E40 y F4B-E43) | **El bloque citado de `PBT3x-C11`** es el de `PBT3B-C11` del contrato emitido de PB-T3b, y su cabecera lo dice: «en PB-T3c, con su prefijo (`PBT3C`, `--task PBT3C` y «PB-T3c»)». Su nota se reescribe. §1 gana las filas de F4B-E40 y F4B-E43, y §8.1, su viñeta | §6.0; §1; §8.1 |
| **V46-02** | **CA-T3b M-02** | **Las filas de PB-T3b de la tabla de RR-3, enumeradas:** las once de CT-01 (ii), la de CT-06 (ii) y las cuatro de CT-10. **El doble con borrado** gana su fila: `PilotEraseScopeDouble`, con `EraseUnit:` y `PreparedBlockMatches` | §6.0 |
| **V46-03** | **CA-T3b M-03** | **Lo heredado de §6.1, en PB-T3c:** las autopruebas de CT-PBT3C-01, 04 y 05; la UI de paridad de PB-T3a en sus `RequiredTests`; el GREEN previsto, Core 19 133 o 19 129. **En PB-T3b, solo como lectura,** en una nota que no va al contrato | §6.2 (nota); §6.3 |
| **V46-04** | **CA-T3b §7** | **La lección «Dos guardas que leen también los comentarios»:** `Transaction` en `Pilot/` y los cinco tipos del modelo fuera de `Draft/`, PP y UR | §5.0 |
| V46-05 | La sesión | **La versión y el estado:** el `Objective` de PB-T3b y el de PB-T3c dicen «v4.6»; la cabecera, la precondición 1 y la 9; el barrido de PB-T3c sobre `eab27a9e` en el §4.1; los marcadores `<tras el cierre de PB-T3b>` | Cabecera; precondiciones 1 y 9; §4.1; §6.2; §6.3 |
| **V46-06** | **F4B-E45 (Owner)** | **Los nombres nuevos, con la misma firma:** `Execute` completo → **`ExecuteFull`**; `ComparePost` completo → **`ComparePostFull`** (los de la sesión); `Prepare` completo → **`PrepareFull`** (V46-07). Cambian D-PBT3-09, D-PBT3-10, D-PBT3-13, CT-PBT3B-01, 07, 09, 10, 12, 13 y 15, E-5, la fila de CT-13 de la tabla de RR-3 (`PushBackPilotUpdateRun.ExecuteFull`), el esqueleto del RED y la lista cerrada de `PBT3B-C13`. Los registros de (A′) y de D-1 no se reescriben: llevan una nota | D-PBT3-09; D-PBT3-09b; D-PBT3-10; D-PBT3-13; §5.3; §6.0; §6.2 |
| **V46-07** | **F4B-E45 (Owner)** | **El barrido ampliado del §4.1:** la resolución de sobrecargas y la reflexión (`overload_sweep.py`) y los nombres nuevos contra los literales de las guardas (`name_check_v46.py`), sobre `eab27a9e`. **El control positivo** encuentra las dos llamadas del CS0121. **Y un tercer choque, por reflexión:** `WorkspacePilotPrepareTests.cs:2155-2160` exige un solo `Prepare` en `PushBackPilotUpdatePlanner` | §4.1; R-22 del registro |
| **V46-08** | La sesión | **`Precheck` sigue como sobrecarga,** con su porqué (abajo) | D-PBT3-10; §5.0 |
| **V46-09** | **F4B-E45 (Owner)** | **La lección:** una sobrecarga nueva rompe sin cambiar ninguna firma (el compilador, la reflexión al ejecutar, los grupos de métodos); el barrido y la compilación antes de fijar un esqueleto; las propiedades nuevas del plan, de solo lectura | §5.0 |
| **V46-10** | **F4B-E45 (Owner)** | **La reemisión de PB-T3b en el intento 2 de 3**, como la de PB-T3a (V45-04 y V45-06): `StopCondition`, `ChainRedSha` null, `ChainRedFiles` vacío, `BaseSha` y `ChainBaseSha` el commit BASE de la reemisión; `blocked-skeleton.patch` con los tres nombres cambiados, como punto de partida. La precondición 12. **Lo del borrador que tocaba PB-T3b como referencia, revisado** (abajo) | §6.0; §6.2; precondiciones 1, 9 y 12; §8.1 |

## El barrido ampliado sobre `eab27a9e` (V46-07)

**Método:** `overload_sweep.py` por objetos git (`git ls-tree` y `git cat-file --batch`) sobre los 1 796 archivos C# de `src/` y `tests/`; la salida, `overload-sweep-eab27a9e.txt`. `name_check_v46.py`, el método de `name_check.py` con un nombre neutro de control; la salida, `name-check-v46-eab27a9e.txt`. El registro lo guarda como R-22.

| Qué | Medido | Qué dice |
|---|---|---|
| Control: `Execute` con una sobrecarga (v4.5) | 11 usos; **uno ambiguo**, `WorkspacePilotUpdateRunTests.cs:669` (`null!`) | El CS0121 del Worker |
| Control: `ComparePost` con una sobrecarga (v4.5) | 9 usos; **uno ambiguo**, `:799` (`null`) | El CS0121 del Worker |
| Control: `Prepare` con una sobrecarga (v4.5) | 16 usos, todos con un argumento tipado donde los tipos difieren. **Una reflexión que rompe:** `WorkspacePilotPrepareTests.cs:2156`, `Assert.Single(members, m => m.Name == "Prepare")` | Compila, pero `ThePlanTypesLiveInThePilotNamespaceAndTheirSignaturesNameNoTypeOfTheHostOrOfTheDrawing` fallaría en el RED: PBT3B-C14. **Por eso `PrepareFull`** |
| `Precheck` con una sobrecarga (v4.6) | 5 usos (WHC:864; `WorkspacePilotUpdateRunTests.cs:34`, `:42`, `:51` y `:58`), todos tipados; ningún grupo de métodos; ninguna reflexión sobre `PushBackPilotUpdateRun` | Sin cambio |
| Los constructores internos nuevos (v4.6) | `PushBackPilotUpdatePlan`: 4 usos de 7 argumentos; `PushBackPilotPostExpectation`: 4 de 2; sin parámetros opcionales | Otra aridad: sin cambio |
| El constructor de `RackPilotMutationScope` (PB-T3c) | 1 uso, WR:156 | La firma cambia en su archivo |
| Conversiones | Todos los tipos `sealed`; ningún `implicit operator` | Sin conversión |
| Reflexión sobre los tipos tocados | `:2155` y `:1958-1962` (propiedades sin `set`, campos `readonly`) | La segunda pide `{ get; }` (§5.0) |
| Los nombres nuevos | Libres en PP, UR y WR | Sin choque |
| Literales de las guardas | 8 filas propias de `ExecuteFull`, 3 de `ComparePostFull`, 1 de `PrepareFull`; ninguna lee PP ni UR | Sin colisión |
| **Resultado con los nombres de la v4.6** | **0 llamadas ambiguas, 0 grupos de métodos, 0 reflexiones rotas** | |

## Por qué `Precheck` se deja como sobrecarga (V46-08)

- **No rompe nada medible:** sus cinco llamadas pasan un borrador tipado del piloto; no hay `null` ni `default` sin tipo, grupos de métodos ni reflexión sobre `PushBackPilotUpdateRun`. El Worker la compiló sin error en el intento 1, y la compilación de comprobación B de la sesión, también.
- **La guarda que la cuenta no se mueve:** `TheBodyOfTheUpdateFollowsTheOrderOfThePackageAndEndsTheMarkOfKnownCleanAfterTheClose` (`WorkspacePilotUpdateGuardTests.cs:838`) cuenta `PushBackPilotUpdateRun.Precheck(` en el cuerpo de `TryUpdatePilot` de WHC, que PB-T3b no toca.
- **Renombrarla cambiaría más texto sin necesidad:** CT-13 (ii), CT-15 y la fila de CT-13 de la tabla de RR-3, y otra compilación. La decisión del Owner nombra los métodos que chocan.
- **Lo que queda:** una llamada nueva con `null` o `default` sin tipo, en las pruebas de PB-T3b, no compilaría. Es un archivo del alcance y se arregla en el RED. El §5.0 pide llamarla con un borrador tipado.
- **La otra opción** (`PrecheckFull`, libre en `eab27a9e`) es un cambio de un parámetro en `make_v46.py`, si la sesión prefiere la regla sin excepciones.

## Lo que el borrador tocaba de PB-T3b, revisado para la reemisión (V46-10)

- **La tabla de RR-3** (V46-02): rige ahora la reparación del RED de PB-T3b. `check_v46.py` compara sus filas con la derivación del generador emitido sobre la v4.5: las mismas líneas y los mismos valores, sin derivadas, **salvo el literal de CT-13**, `PushBackPilotUpdateRun.Execute` → `PushBackPilotUpdateRun.ExecuteFull` (V46-06). La fila del doble es nueva y lleva los dos literales del §5.0.
- **La nota de M-03** (V46-03): sigue fuera del contrato y sigue siendo cierta. El contrato reemitido conserva la `ExpectedEvidence` y los `RequiredTests` que CA-T3b dio por fieles; las autopruebas son las de CT-13, E-2 y E-6; la UI de paridad no pierde red; el GREEN previsto de Core sigue en 19 128, o 19 124. La línea base no cambia: el intento 1 no dejó commits.

## Para la reemisión (la precondición 12)

- la compilación de comprobación de la sesión (`compile-check.md`) usó el patch del intento 1 con `ExecuteFull` y `ComparePostFull`. **La v4.6 cambia el esqueleto en un nombre más, `PrepareFull`:** hay que repetirla sobre este texto;
- **la reflexión solo falla al ejecutar:** compilar no basta para `WorkspacePilotPrepareTests.cs:2156`. La precondición 12 pide esa clase en verde sobre la copia;
- la recomprobación de CA-T3b sobre la v4.6 y el contrato en seco.

## Los marcadores `<tras el cierre de PB-T3b>`

- la precondición 1: la `BaseSha` de PB-T3c;
- la precondición 9: la remedición de PB-T3c, su comprobación acotada y el seco de su generador;
- el §4.1: el barrido de las guardas, con las de PB-T3b;
- el §6.3: los `MinSelected` de `WorkspacePilotUpdateGuardTests` y `WorkspacePilotUpdateIntegrationGuardTests`, los de lo heredado de §6.2, y la línea base de la CI.

## Lo que se deja como estaba

- **La fila de PB-T3c de la tabla de RR-3** (`PushBackWritePathPluginGuardTests`): sus literales se enumeran en la emisión de PB-T3c.
- **La historia:** los registros de cambios, las notas de la reemisión de PB-T3a, los registros de (A′) y de D-1 (con una nota), y las menciones de la v4.5 en lo ya emitido.
