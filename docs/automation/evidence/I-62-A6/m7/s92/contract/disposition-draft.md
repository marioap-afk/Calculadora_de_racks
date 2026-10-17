# I-62 — Borrador de disposición limitada: la orden `contract` del instrumento B-min fuera de las VERIFY de FX-02 y de la medición de A6-7

```text
Tipo:        borrador preparado por la supervisión para decisiones §92, punto 4. No decide ni se aplica.
Fecha:       2026-10-17 (UTC-6)
W:           %USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal, HEAD bb439090 (solo lectura)
A-6:         docs/initiatives/I-62-A-6.md, blob 4bc919dd (AGREED); AP blob f525cb1e; RAE blob 592dcfd4
Piezas:      instrumento 9a829b0eeb1210238052b1feac06cfa8f7964aed, aceptante 434ef1fccb0d15762b30244b4a60928120698411 (sin cambio)
Evidencia:   s92-result.json (SHA-256 863d446f…), del banco s92_bench.py; nc2-result.json de §91 (86cd607e…, custodiado en W
             en docs/automation/evidence/I-62-A6/m7/s91/d1/nc2/, blob 30c2bfb1)
Límites:     sin Codex, sin CLI de modelo, sin red; ningún blob fijado ni el kit congelado cambian.
```

## 0. Resumen

1. **Mecanismo.** La disposición nombra la *orden declarada* (A6-1, regla 1; A6-7, regla 3) con un conjunto cerrado de 11 órdenes del instrumento,
   sin `contract`. La misma orden declarada rige la medición de A6-7 y toda invocación VERIFY posterior: es un invalidador (A6-1, regla 8; A6-7,
   regla 5).
2. **Si un Controller ejecuta `contract` igualmente.** Es ejecución del instrumento (contiene el blob), pero no tiene la forma de la orden declarada:
   el registro no se puede reconstruir, no hay solicitud y la salida no se acepta (A6-3, regla 6 (a); A6-5, reglas 4 y 7 (c)). Es una no aceptación de
   tipo transporte, con reejecuciones dentro de los topes y STOP P-04 al agotarlas; **nunca un FAIL del producto** (A6-5, regla 5). En la medición, la
   medición no demuestra la reconstrucción y no se repite (A6-7, regla 4).
3. **Hueco mecánico.** La regla del kit `reconstruct_runs_m7.py` (SHA-256 `1eceecee…`) **sí reconstruye** una ejecución de `contract`, porque su lista
   `TOOL_COMMANDS` la incluye: comprobado sobre dos registros. Falta una comprobación mínima: `Argv[0]` dentro del conjunto declarado, o no hay
   solicitud. Está especificada en `declared_orders.py` y detecta los dos registros.
4. **`Contract` sigue controlado.** El Controller conserva la obligación de 16.9 #4 con U-19. Lee CONTRACT y DELEG con `read-json --file`, una orden
   declarada; el aceptante cubre esa lectura con R3 y R9 (demostrado en el banco: `ACCEPT_FACTS` y lectura fiel de los 8 campos). R7 deja de
   comparar A3/A4. El recálculo de `Contract` por la supervisión desde los bytes custodiados **no existe hoy** en el protocolo: la disposición lo
   crea. Existen la aceptación A1'-A8' por el Coordinator y la repetición de `Scope`.
5. **nc2 S1 sin `contract`:** aceptada y oráculo relativo intacto, en §91 y otra vez aquí con las dos lecturas `read-json --file`. **Operaciones de
   A6-7, regla 1:** todas siguen siendo demostrables. `contract` no era la única portadora de ninguna: OP6 tenía otras cinco órdenes.
6. **Consecuencia para el kit congelado (FREEZE `459a3440`).** No puede lanzarse bajo esta disposición. Su texto ordena `contract` (C7), y la
   medición registraría una orden no declarada que su regla, tal como está, no detecta. Hay que prepararlo de nuevo (§6) antes de la medición.

## 1. Texto propuesto de la disposición

> «**Disposición del Coordinator: la orden `contract` del instrumento B-min fuera de las invocaciones VERIFY de FX-02 y de la medición de A6-7
> (D-1).** Solo para FX-02 en F6, Topología A, celda de EXECUTION_CONTROLLER, con el instrumento (blob `9a829b0eeb1210238052b1feac06cfa8f7964aed`) y el
> aceptante (blob `434ef1fccb0d15762b30244b4a60928120698411`) que fija A-6, sin cambiar ninguno de los dos ni ningún texto de A-6, de AUTOMATION_PLAN
> o de `agent-execution/README.md`:
>
> 1. **Ámbito.** Se aplica a la invocación de medición de A6-7 y a toda invocación VERIFY con el instrumento: la verificación, su reverificación de
>    16.7 si la hay y cada control negativo con invocación (A6-5, regla 7). No se aplica a la planificación (A6-1, regla 2), a producción ni a ninguna
>    otra unidad, escenario, rol o topología.
> 2. **Orden declarada.** A efectos de A6-1, reglas 1 y 8, A6-3, regla 6 (a), y A6-7, reglas 3, 5 y 7, la orden declarada es
>    `<git> --no-replace-objects cat-file blob 9a829b0eeb1210238052b1feac06cfa8f7964aed | <intérprete> -I - <orden> …`. `<git>` y `<intérprete>` son
>    los declarados, por ruta y SHA-256. `<orden>` es exactamente una de: `identity`, `diff`, `scope`, `hashes`, `cleantree`, `read-text`, `read-json`,
>    `trx`, `handoff`, `freetext`, `clausemap`. La orden `contract` del instrumento no forma parte de la orden declarada: una ejecución con `<orden>` =
>    `contract` es «otra orden» y no es el instrumento de la regla de A-6 en D.3 (A6-1, regla 1). La disposición de medición y cada disposición que
>    autorice una invocación VERIFY posterior nombran esta misma orden declarada; nombrar otra es un invalidador (A6-1, regla 8; A6-7, regla 5).
> 3. **Texto de las invocaciones.** El texto de la medición y el de cada invocación VERIFY no contienen la orden `contract`. El de cada control es el
>    de la verificación, salvo la ruta de entrada y el `RunId` (RAE §10). Ningún otro elemento de la receta ni del contrato del adapter cambia (A6-1,
>    regla 4).
> 4. **`Contract` sigue siendo obligación del Controller.** En toda invocación VERIFY, el Controller verifica `Contract` (AUTOMATION_PLAN 16.9 #4:
>    A3-A5 de 16.5 y los añadidos A3'-A5' de 16.22) con U-19 tal como está registrada (decisiones §62, punto 3): una entrada que termina en `/**` se
>    lee como el prefijo terminado en `/`, en las dos entradas de cada contención.
>    - Lee CONTRACT y DELEG con `read-json --file CONTRACT=… --sha256 CONTRACT=…` y `read-json --file DELEG=… --sha256 DELEG=…`, con los punteros
>      `/AllowedWriteScope`, `/ForbiddenWriteScope`, `/Invariants`, `/StopConditions`, `/Authorities`, `/RequiredTests`, `/RoleRequirements` y
>      `/SupersededCommits`.
>    - Cita en el `Evidence` de `Contract` esas órdenes, con el SHA-256 de los hechos usados, y su contraste primario: el SHA-256 de los dos archivos
>      (A6-4, regla 2).
>    - No cita ninguna ejecución de `contract`.
>    - Una comparación que no se pudo ejecutar o no terminó es `not_run`, y cuenta como fail (16.22).
> 5. **Ejecución de `contract` pese a esta disposición.** Toda orden registrada que contiene el blob fijado es ejecución del instrumento (A6-3, regla 6
>    (a)). Si su `<orden>` es `contract`, no tiene la forma de la orden declarada: no se puede reconstruir, no hay solicitud y la salida queda sin
>    aceptar (A6-3, regla 6 (a); A6-5, reglas 4 y 7 (c)).
>    - Es el modo INVALID_OUTPUT de 16.9 (transporte BLOCKED; P-19). No se aplican su `Classification` ni su `Disposition`; no hay corrección,
>      `attempts`, contador por clase ni STOP derivado de ella.
>    - Recuperación de 16.8 y 16.11: reejecución con `RunId` nuevo, como máximo dos por (`TaskId`, fase) y dos por control negativo, cada una contada
>      en los topes de D.3. La tercera → STOP (P-04) y, en un control, control no superado.
>    - Nunca es un FAIL de la entrega del Worker ni de FX-02 (A6-5, regla 5).
>    - En la medición, la reconstrucción de todas las ejecuciones que exige A6-7, regla 1, no se cumple: la celda no es elegible con el instrumento y la
>      medición no se repite (A6-7, regla 4).
> 6. **Controles del aceptante.** La tabla de A6-5, regla 2, no cambia:
>    - R1, R2, R3, R9 y R10 en `MATCH`; R8 en `MANUAL`;
>    - R4 en `MATCH` cuando la solicitud lleva `Verification` (A6-3, regla 6 (c)); no aplicable solo por esa regla;
>    - R5, R6 y R7 en `MATCH`, o no aplicables solo por su condición de datos. R7 sigue con `scope` y `clausemap` y deja de comparar A3/A4, porque no
>      hay ejecución de `contract`;
>    - las lecturas `read-json --file` de CONTRACT y DELEG las cubren R3 (reejecución y comparación con el sobre) y R9 (archivos del cierre derivado,
>      por nombre y SHA-256);
>    - en la medición, R4 y R10 no aplicables (A6-7, regla 1).
> 7. **Comprobación de `Contract` por la supervisión, entre artefactos y sin el instrumento.** Para cada salida aceptada de una invocación VERIFY, la
>    supervisión recalcula A3-A5 y A3'-A5', con U-19, desde los bytes custodiados de CONTRACT y DELEG que fija el cierre de la invocación. Lo hace de
>    forma mecánica, sin el instrumento ni el aceptante, y compara el resultado con el `Result` de `Contract` del Controller; en la verificación, también
>    con el resultado de A3'-A5' registrado en la aceptación del paquete (`Outcome.Acceptance`). Efectos:
>    - en la verificación, una diferencia impide aceptar un VERIFIED, como la repetición de `Scope` (RAE §14.7), y el Coordinator lo registra por
>      añadido;
>    - un `Contract` en fail que el recálculo refuta se registra como error del Controller, nunca como defecto del producto;
>    - en un control, el recálculo no cambia el oráculo (punto 8): solo atribuye la causa de una diferencia en `Contract`.
> 8. **Oráculo de nc2 sin cambio.** El de RAE §10: `Scope` en fail, `FailureClass` = `Scope` y todas las comprobaciones anteriores en el orden de
>    16.9, `Contract` incluida, con el mismo `Result` que en la verificación real.
> 9. **Prueba previa y medición.** La prueba previa de A6-5, regla 6, se hace con las órdenes declaradas del punto 2. La medición demuestra con esas
>    órdenes las operaciones de A6-7, regla 1. Esta disposición se fija antes de la medición, y la nombran la disposición de medición y las que
>    autoricen cada invocación VERIFY.
> 10. **Límites.** No cambia ningún blob fijado (A6-8, regla 5, no se activa), ni la determinación del Owner `A6-ADR0046-9-FX02` (§78), ni los
>     presupuestos, ni el oráculo de C-24 (A6-6). No declara la celda elegible y no autoriza ninguna invocación ni ningún consumo.»

## 2. Base de autoridad (literales de W en `bb439090`)

| Regla | Literal | Uso en la disposición |
|---|---|---|
| A6-1, regla 1 (A-6 L492-L500) | «el Controller puede ejecutar … un único instrumento …, ejecutado desde su objeto Git … con la orden declarada `<git> --no-replace-objects cat-file blob <blob> \| <intérprete> -I - <orden> …` … Otro blob, otro archivo, otra orden, otro intérprete, otro Git o una ejecución no aislada no son el instrumento de esta regla» | uso facultativo; `<orden>` forma parte de la orden declarada; `contract` fuera del conjunto = «otra orden» (punto 2) |
| A6-1, regla 4 (L506-L509) | «ningún elemento de la receta del adapter cambia …» | punto 3 |
| A6-1, regla 7 (L516-L518) | «toda ejecución del instrumento que conste en el registro de la invocación entra en la reproducción del aceptante» | una ejecución no se puede excluir de la solicitud: si no es reconstruible, no hay solicitud |
| A6-1, regla 8 (L519-L520) | «un cambio del blob del instrumento, de la orden declarada, del intérprete declarado … o del Git declarado es un invalidador» | el conjunto de órdenes es el mismo en la medición y en cada VERIFY (punto 2) |
| A6-3, regla 6 (a) (L592-L599) | «Es ejecución del instrumento toda orden registrada que contiene el blob fijado. Su `Argv` sale del texto registrado de la orden, que tiene exactamente la forma de la orden declarada … Si una ejecución no se puede reconstruir así (otra forma, …) …, no hay solicitud y la salida queda sin aceptar» | punto 5 |
| A6-3, regla 6 (c) (L618-L621) | `Verification` solo si los tres archivos existen y ninguna de `Identity`, `Scope`, `CleanTree`, `Trailer` y `FreeText` está en `not_run` | punto 6 (R4) |
| A6-5, regla 2 (L673-L686) | tabla de aceptación; «R7: una orden `scope`, `contract` o `clausemap` en `Runs`» | punto 6: R7 sigue aplicándose por `scope` y `clausemap` |
| A6-5, regla 3 (L687-L688) | «salida aceptada: se ingiere, y rigen 16.9 y 16.11 …, junto con las demás comprobaciones entre artefactos del Coordinator …» | punto 7 (con la salvedad del §4) |
| A6-5, regla 4 (L689-L702) | «salida no aceptada: cualquier otro caso (…; un registro que no permite reconstruir las ejecuciones; …) … Es el modo INVALID_OUTPUT de 16.9 (nunca VERIFIED; transporte BLOCKED) … reejecución con `RunId` nuevo, como máximo dos por (`TaskId`, fase) y dos por control negativo; la tercera → STOP (P-04) y, en un control negativo, además control no superado» | punto 5 |
| A6-5, regla 5 (L703-L706) | «una salida no aceptada nunca es por sí misma un FAIL de la entrega del Worker ni de FX-02» | puntos 5 y 7 |
| A6-5, regla 6 (L707-L716) | prueba previa «con las órdenes que usará la verificación» | punto 9 |
| A6-5, regla 7 (L717-L733) | «Ninguna está obligada a ejecutar el instrumento: su uso es del Controller»; (c) «si una ejecución no se puede reconstruir …, la salida no se acepta (regla 4)» | puntos 1, 3 y 5 |
| A6-7, regla 1 (L759-L770) | operaciones que la medición demuestra «con ese instrumento, su blob, la orden declarada …»; «la reconstrucción de `Runs` de todas sus ejecuciones del instrumento con una regla que cumple … regla 6 (a)» | puntos 5 y 9; §5 (c) |
| A6-7, regla 3 (L772-L777) | «exige una disposición del Coordinator que nombre la celda, el blob del instrumento, la orden, el intérprete y Git declarados …» | la disposición nombra la orden declarada |
| A6-7, regla 4 (L778-L780) | «si falla o no demuestra alguna de esas operaciones, la celda no es elegible …, la invocación no se repite» | punto 5 (medición) |
| A6-7, regla 5 (L781-L786) | invalidadores: «… blob del instrumento, orden declarada, intérprete declarado, Git declarado, regla de reconstrucción de `Runs` o cierre de VERIFY …» | punto 2 |
| A6-7, regla 7 (L789-L794) | la disposición previa a la medición «limita las órdenes del instrumento a las lecturas que ese cierre admite …; la disposición que autoriza cada invocación VERIFY posterior nombra los mismos» | precedente de limitar las órdenes por disposición; punto 9 |
| RAE §10 (L219-L226) | «el mismo prompt, salvo la ruta de entrada y el `RunId` de la invocación»; oráculo relativo | puntos 3 y 8 |
| AP 16.5 (L552-L554, L562-L564), 16.9 #4 (L633), 16.22 (L1192, L1202, L1204-L1207); decisiones §62, punto 3 (L1215, L1234, L1250) | sintaxis y contención; A3-A5; `Contract`; A3'-A5'; `not_run` = fail; U-19 | punto 4 |

**Lectura que la disposición tiene que hacer explícita.** A-6 escribe `<orden>` como variable de la orden declarada y no fija su conjunto. La
disposición lo fija: es la «orden» que A6-7, regla 3, le encarga nombrar. Con la lectura contraria, en la que «orden declarada» es solo la plantilla con
`<orden>` abierta, quitar `contract` sería una instrucción del texto sin consecuencia mecánica. Una ejecución de `contract` sería reconstruible y la
juzgaría el aceptante: con D-1, `REJECT_FACTS`, que también deja la salida sin aceptar; sin D-1, aceptada pese a la instrucción. Por eso el punto 2
declara la lectura de forma literal en lugar de dejarla implícita. **Autoridad:** disposición ordinaria del Coordinator, que nombra lo que A6-7,
regla 3, le encarga nombrar y usa la facultad de limitar órdenes de A6-7, regla 7. No cambia ningún texto acordado. El punto 7 es la parte con más
riesgo de materialidad (§4): si el Coordinator considera material un motivo nuevo para rechazar un VERIFIED, ese punto debe ir en la enmienda
estrecha del punto 2 de §92.

## 3. Si un Controller ejecuta `contract` igualmente

- **Normativamente:** es «otra orden» (A6-1, regla 1) y, por A6-3, regla 6 (a), sigue siendo «ejecución del instrumento», porque contiene el blob.
  Su texto no tiene «exactamente la forma de la orden declarada»: el registro no es reconstruible, no hay solicitud y la salida queda sin aceptar
  (A6-5, regla 4, «un registro que no permite reconstruir las ejecuciones»; regla 7 (c)). Es el modo INVALID_OUTPUT, de tipo transporte: sin
  disposición de fila, sin corrección ni `attempts`, con reejecuciones dentro de los topes (2 por fase o por control) y STOP P-04 al agotarlas. En un
  control, control no superado; nunca un FAIL del producto (A6-5, regla 5). En la medición: NOT_DEMONSTRATED sin repetición (A6-7, regla 4).
- **Mecánicamente hoy:** la regla del kit **no** lo detecta. Reconstruye las 12 ejecuciones de los registros S1 nc2-A y S0 V-A, `contract` incluida (§5
  (a)). La disposición necesita la comprobación mínima de `declared_orders.py` en la próxima preparación del kit (§6). Sin ella, el fallo cerrado del
  punto 5 dependería de una revisión manual del registro.

## 4. Cómo sigue controlado `Contract`

| Capa | Qué controla | ¿Existe hoy? |
|---|---|---|
| Controller (16.9 #4; 16.5; 16.22; U-19) | A3-A5 y A3'-A5' desde CONTRACT y DELEG leídos con `read-json --file`; cita y contraste primario (A6-4, regla 2) | sí (obligación vigente); el medio, por esta disposición |
| Aceptante R3 y R9 | lectura fiel: reejecución de cada `read-json --file` con el mismo sobre, y archivos CONTRACT y DELEG del cierre derivado por nombre y SHA-256 | sí (tabla de A6-5, regla 2) |
| Aceptante R7 | `scope` (PerPath con U-19, MalformedEntries, U19Normalized) y `clausemap` (listas de MV-4); ya no A3/A4 | sí, menos A3/A4 |
| Aceptante R4 | `Identity`, `Scope`, `CleanTree`, `Trailer`, `FreeText` y la parte de mapa de `Authority`; **no** `Contract` | sí; nunca cubrió `Contract` |
| Coordinator, aceptación A1'-A8' | A3-A5 y A3'-A5' de la delegación real frente al contrato, mecánicamente y sin cortocircuito, antes del Worker; anotado en `Outcome.Acceptance` (16.5; RAE §4 L117, §14.7) | sí, solo para la delegación aceptada (no para las copias de los controles) |
| Coordinator, repetición de `Scope` | antes de aceptar un VERIFIED (RAE §14.7 L517-L518; AP 16.22 L1205-L1207) | sí, solo `Scope` |
| **Supervisión, recálculo de `Contract`** | A3-A5 y A3'-A5' con U-19 desde los bytes custodiados de cada invocación VERIFY, sin el instrumento; comparación con el Controller y con `Outcome.Acceptance` | **no: nuevo** (punto 7). A6-5, regla 3, alude a «las demás comprobaciones entre artefactos del Coordinator», pero ninguna cláusula define esta |

En el banco (§5, X), el recálculo independiente (`contract_crosscheck.py`, escrito sin el instrumento, sin el aceptante y sin la simulación del
Controller) coincide con el Controller en 10 de 11 salidas. La única diferencia es S1 nc2-A b1: el Controller que se apoyó en los hechos de
`contract` (D-1) dio `fail` y el recálculo da `pass`. Es exactamente el falso fail que el punto 7 atribuye al Controller y no al producto.

## 5. Evidencia (banco, componentes reutilizados byte a byte)

**(a) Regla de reconstrucción del kit frente a un conjunto declarado sin `contract`.** `reconstruct_runs_m7.py` (W, blob `15e1d4bb`, SHA-256
`1eceecee…`, regla `reconstruct_runs_m7/2`) ejecutado como CLI sobre registros en formato Codex del banco de §91 (renderización R1):

| Registro (SHA-256) | Reconstruible (regla del kit) | Ejecuciones | ¿`contract`? | Sobres = reconstrucción del banco | Comprobación del conjunto declarado |
|---|---|---|---|---|---|
| S1 nc2-A, 12 órdenes | **sí** | 12 | sí (Seq 3, `item_4`) | sí | **falla: `contract` fuera del conjunto** |
| S0 V-A, 12 órdenes | **sí** | 12 | sí (Seq 3, `item_4`) | sí | **falla: `contract` fuera del conjunto** |
| S1 nc2-C, 11 órdenes | sí | 11 | no | sí | pasa |
| S0 V-C, 11 órdenes | sí | 11 | no | sí | pasa |

La regla, tal como está escrita, reconstruye `contract` porque `TOOL_COMMANDS` (L41-L42) lo incluye: desconoce el conjunto declarado. **Comprobación
mínima** para la próxima preparación (`declared_orders.py`): sobre el resultado de la reconstrucción, si algún `Argv[0]` no está en el conjunto
declarado, el registro se trata como no reconstruible y no se construye solicitud. Puede ir en la regla, como parámetro de una v3 con SHA-256 nuevo
que nombre la disposición (es invalidador por A6-7, regla 5), o como puerta inmediatamente antes de construir la solicitud. A6-3, regla 6 (a), sitúa la
comprobación de la forma en la reconstrucción, así que lo coherente es la regla; la puerta es equivalente en efecto si la disposición la nombra.

**(b) nc2 S1 sin `contract` (§91, sin rehacer).** `nc2-result.json` (SHA-256 `86cd607e…`, idéntico al custodiado en W, blob `30c2bfb1`), oráculo
`S1/C/c`: aceptado, `ACCEPT_FACTS`, R1-R7 y R9-R10 en `MATCH`, R8 `MANUAL`, solicitud `81083c79…` y salida del aceptante `147d7492…`. `Contract`
queda en pass, igual que en la verificación real S1 V-C, también aceptada. `Scope` en fail; ninguna comprobación anterior difiere; `FailureClass`
relativa = `Scope`: **oráculo relativo intacto**. Contraste con kit A, apoyado en los hechos (`S1/A/b1`): `REJECT_FACTS` y oráculo roto.

**(b') La misma nc2 S1 con las órdenes que propone la disposición** (11 declaradas más `read-json --file CONTRACT` y `read-json --file DELEG`, ocho
punteros; RunId `R20261017T000000Z-d1e4`; copia S1 de §91 con la comprobación del punto de uso repetida sin pasos fallidos):
- regla del kit: reconstruible, 13 ejecuciones, sobres iguales a la reconstrucción del banco; la comprobación del conjunto declarado pasa;
- valores de `read-json` iguales a los bytes custodiados en los 8 campos de CONTRACT y de DELEG (lectura fiel);
- el Controller calcula `Contract` desde esos valores con U-19 = pass (`…/T1/worker-summary.md` contenida por `…/FX-U1-agent/**`) y el recálculo de la
  supervisión coincide;
- aceptante (solicitud `7a78a7c1…`, salida `2d715979…`): `ACCEPT_FACTS` con código 0; R1-R7 y R9-R10 en `MATCH`; R8 `MANUAL`; cero escrituras;
  procesos terminados; tabla aceptada;
- oráculo frente a S1 V-C: intacto.

**(c) Operaciones de A6-7, regla 1, con el conjunto sin `contract`** (cobertura del kit `a6m7-measure`, README §5):

| Operación de A6-7, regla 1 | Hoy (kit) | Sin `contract` |
|---|---|---|
| OP1 entrega exacta de los bytes por la orden declarada en el shell de la invocación | C2 (sin orden del instrumento) | igual |
| OP2 ejecución desde el objeto y aislada | C3, C4-C15 (12 órdenes) | las órdenes declaradas (11 + 2 `read-json --file`): `Isolated` en cada sobre; R1-R3 |
| OP3 `git diff --name-only` y pertenencia por ruta de `Scope` | `diff`, `scope`; P09 | igual |
| OP4 lecturas Git de `Identity`, `Remote`, `CleanTree`, `Trailer` | `identity`, `cleantree`, `diff`; P01-P08, P10-P14 | igual |
| OP5 blobs y hashes | `hashes`; P15, P16 | igual |
| OP6 lectura fiel de JSON y texto | C7 `contract`, C10 `handoff`, C11 `freetext`, C13 `trx`, C14 `read-text`, C15 `read-json`; P17-P21, P24 | `handoff`, `freetext`, `trx`, `read-text`, `read-json` (contrato en G por revisión y ruta) y, propuestas, `read-json --file CONTRACT` y `read-json --file DELEG`: lectura fiel de los dos JSON de `Contract`, demostrada en (b'). DELEG también lo lee `scope` y CONTRACT, `clausemap`. P18 se reasigna de `contract` a esas lecturas |
| OP7 resolución por el mapa (`AuthorityResolution`) | `clausemap`; P22, P23 | igual |
| OP8 contraste con la orden Git primaria | P01-P24 | P01-P24, con P18 reasignada |
| OP9 salidas estructuradas completas | `schema.m7.json` | esquema sin los campos de `contract` (§6) |

Ninguna operación dependía solo de `contract`. Las dos lecturas `read-json --file` cubren lo que `contract` leía y además dejan las entradas de
`Contract` bajo R3 y R9.

## 6. Cambios del kit para su próxima preparación (no se tocó el kit)

| # | Componente (W, blob) | Cambio |
|---|---|---|
| 1 | `lib_kit.py` (`bc264b11`) L54-L55 `ORDERS` | quitar `contract`; añadir `read-json --file CONTRACT` y `read-json --file DELEG` con los ocho punteros |
| 2 | `tools_m7.py` (`3bea9518`) L66 `order_argvs` | quitar el argv de `contract`; añadir los dos `read-json --file` |
| 3 | `tools_m7.py` L106 `PRIMARY` P18 | dueño `contract` → las lecturas `read-json --file` (la orden primaria no cambia) |
| 4 | `tools_m7.py` L843-L862 `compare` (OP6) y el oráculo `O['Contract']` | quitar «contract envelope A5Missing and SupersededEqual = oracle» y su copia de salida; añadir «valores de `read-json --file` = bytes custodiados» y su copia ASCII; OP2 con el número de ejecuciones declaradas |
| 5 | `prompt.m7.template.txt` (`d9093e80`) L9-L10, L53 | ejecuciones C4-C15 («las doce órdenes») → las órdenes declaradas; renumerar C16/C17; sustituir `ContractA5MissingJson` y `ContractSupersededEqual` (de C7) por las copias de los valores de `read-json --file` |
| 6 | `schema.m7.json` (`d4419fb9`) L174 y L506-L592; `schema.m7.strict-projection.json` (`50ba8fd4`) | enumeración de `Order` sin `contract`; campos `Reads` sustituidos; recuentos |
| 7 | `reconstruct_runs_m7.py` (`15e1d4bb`) L41-L42, o la construcción de la solicitud (`table_m7.construct_m`, `b5c16f52`) | comprobación del conjunto declarado (`declared_orders.py`): `Argv[0]` fuera del conjunto → no reconstruible, sin solicitud. En la regla cambia su SHA-256 (marcador `M7-RECONSTRUCCION`; invalidador de A6-7, regla 5) |
| 8 | `disposition-markers.json` (`616fad3c`) L6 y README §2 L47 | `M7-INSTRUMENTO` con la orden declarada y su conjunto cerrado de órdenes; la puerta lo comprueba literalmente |
| 9 | `selftest_m7.py` (`26085367`) L421-L422 | sin los hechos de `contract`; repetir solo las pruebas de los componentes cambiados (validación incremental, §88) |
| 10 | `make_eic_verify.py` (`965f42e8`) L40 `INSTRUMENT_ACTION` (opcional) | si la acción de A6-2, regla 2, debe repetir el conjunto, cambian E y el cierre (`M7-CIERRE`). Coordinarlo con la enmienda del cierre (§92, punto 2) |
| 11 | kit VERIFY futuro (verificación y controles) | mismo texto con las órdenes declaradas; instrucción de `Contract` (punto 4); recálculo de `Contract` por la supervisión (punto 7) junto a la repetición de `Scope`; prueba previa de A6-5, regla 6, con esas órdenes; también los controles N4 y nc1, que en el banco ejecutaban `contract` |
| 12 | `FREEZE.json` (`c3bb2eee`, FREEZE `459a3440`) | volver a congelar. El kit actual no es lanzable bajo esta disposición: ordena C7 `contract` |
| — | ya pedido por §92, punto 6 | formato del `RunId` (`-m7m0` no es de 4 hex) e identificación del shell |

## 7. Riesgos y límites

- La lectura del punto 2 (el conjunto de `<orden>` forma parte de la orden declarada) es la que da la consecuencia mecánica del punto 5. Si el
  Coordinator o el Architect no la aceptan, quitar `contract` queda como instrucción del texto (§2).
- El punto 7 es nuevo. Su base es A6-5, regla 3, y AP 16.22 L1207 (el Coordinator «hace las comprobaciones entre artefactos de la verificación»),
  pero no hay ninguna cláusula que lo enumere. Puede requerir la enmienda del punto 2 de §92.
- Se pierde el contraste R7 del aceptante sobre A3/A4. `Contract` queda como su parte A5 y los añadidos de 16.22 (G3): bajo el Controller, el
  recálculo de la supervisión y el oráculo sellado.
- No se demostró con un Controller real ni con un registro real de Codex 26.1013 (§92, punto 6). El banco usa salidas sintéticas y el cierre R7-A,
  que sigue como referencia técnica (§92, punto 3). El recálculo (`contract_crosscheck.py`) y la simulación del Controller son del mismo autor:
  independencia de implementación, no de autoría.
- En (b'), la verificación de referencia S1 V-C se corrió sin las dos lecturas `read-json --file`. Con «el mismo prompt», la verificación real
  también las llevaría. El oráculo compara `Result`, que coinciden.

## 8. Archivos (`D:\r62-a6m7\d1\s92\`)

| Archivo | SHA-256 |
|---|---|
| `disposition-draft.md` | (este archivo; ver el mensaje de entrega) |
| `s92-result.json` | `863d446f03ed1018f066b12b87dd3f384d777b99e6b588b14a573886fa53453e` |
| `s92_bench.py` | `8d755cc59101f233999aa7775c02603f823b7aa70ac5a56d7871ae4e99313f96` |
| `declared_orders.py` | `ec43ae019599a0c55e09acf8f1fcd763b40601091536b382761f3267cce01050` |
| `contract_crosscheck.py` | `869d59890c6324f03a0e676fd340281c81325f2828de997c28594c2a338714e1` |
| `lib/reconstruct_runs_m7.py` (copia byte a byte del kit) | `1eceeceecfebf994e3bce0af7818dfaa2144b99fb1339897745c2fe9ea7141ab` |
| `out/s92-nc2-r/request.json` | `7a78a7c1f27eef6f51d3790045c779277a578f21f361152831d2bd2530c4fd45` |
| `out/s92-nc2-r/acceptor-stdout.json` | `2d71597996c8e3043982325a65492c2d73bdb4651fa35f295c108530d6b12053` |
| `out/s92-nc2-r/controller-output.json` | `aa88b9c5b6b86c3978255d4cfd9d4550e2be6a24d7edb567ff906e99065267a0` |
| `out/s92-nc2-r/envelope-read-json-CONTRACT.json` | `1590093c04fa72bb68405cc66bfaa2e62d2d3c2cb959e84d6521254ac4c0307f` |
| `out/s92-nc2-r/envelope-read-json-DELEG.json` | `49ccf18366782fa37778cbd973a645db842fad20e5194c40bd2d1c6d28c6b673` |
| `run.log` | `2b3aca0a04abeda64dd2c09f429fd470b5e7dbc23449ac4c83858981d180b742` |

Reproducción: desde pwsh, `python -I s92_bench.py` (unos 70 s). Requiere la copia S1 y los registros que dejó `../nc2/d1_nc2.py`. `s92/w` es un área
de trabajo transitoria: sesiones con rutas absolutas del perfil de usuario; no es entregable.
