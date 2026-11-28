> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T3 (la escritura completa del Workspace), v4.2

**Estado:** v4.2, **solo de texto**: no cambian el diseño ni D-1. Viene de seis revisiones:
- la revisión independiente adversarial de la v1 (`F4B-DRAFTS/pbt3/independent-review-v1.md`, §1 a §9): **NECESITA v2, B/I/M 1/11/16**;
- su recomprobación de la v2 (§10 del mismo archivo): **NECESITA v3, B/I/M 0/3/4**;
- su recomprobación de la v3 (§11): **NECESITA v4, B/I/M 0/1/1**;
- su recomprobación de la v4 (§12): **LISTO PARA EMITIR TRAS REMEDICIÓN, B/I/M 0/0/1**, aplicada en la v4.1;
- **la revisión de compatibilidad con PB-T2** (`F4B-DRAFTS/pbt3/compat-pbt3-vs-pbt2.md`, sobre la v4.1): **COMPATIBLE CON REMEDICIÓN, B/I/M 0/5/7**, sin decisiones nuevas del Owner. La v4.2 la aplica;
- la recomprobación de la v4.2 (§13 de `independent-review-v1.md`): **LISTO PARA EMITIR TRAS REMEDICIÓN, B/I/M 0/0/1**. Valida las dos elecciones, y la fila por defecto del clasificador como corrección del paquete. Su menor, Q-01, se aplica en esta misma v4.2 (V42-13).

**D-1 está decidida:** el Owner respondió «Un solo código (Recomendado)», la variante (A) (**F4B-E29**, `f4b-dispositions.md:800` y `F4B-DRAFTS/coordinator/owner-pbt3-d1-decision.md`, custodiados en `d8ccbcb8`). La variante (A′) **no se eligió**: su texto queda como registro y no va al contrato (V41-03).

**No se emite.** PB-T2 va delante: PB-T3a se emite sobre el cierre de PB-T2c y se vuelve a medir sobre él, con las órdenes del registro `pbt3-measurements.md` y la §3 de la revisión de compatibilidad. En `5ddeb3b4`, PB-T1 está CERRADA (F4B-E31) y PB-T2a tiene el GREEN de su intento 2 (F4B-E33), sin cierre; PB-T2b y PB-T2c no se han emitido. La emisión de PB-T3a espera además una autorización del Owner (precondición 10). No declara GATE PASS.

El detalle de cada hallazgo, con su cambio o el porqué de no aplicarlo, está en `v2-delta.md` (de la v1 a la v2), `v3-delta.md` (de la v2 a la v3), `v4-delta.md` (de la v3 a la v4), `v41-delta.md` (de la v4 a la v4.1) y `v42-delta.md` (de la v4.1 a la v4.2).

## Registro de cambios v4.1 → v4.2 (la compatibilidad con PB-T2)

**Origen.** Desde la v4.1, PB-T2 se diseñó y va delante de PB-T3. La revisión de compatibilidad, solo de lectura (`compat-pbt3-vs-pbt2.md`), dio **COMPATIBLE CON REMEDICIÓN, B/I/M 0/5/7**: ninguna regla de PB-T2b ni de PB-T2c rompe una premisa de PB-T3, y ninguna corrección es decisión del Owner. **El diseño no cambia, ni D-1, ni la letra de los contratos salvo lo que cada fila dice.** Dos filas añaden líneas esperadas a dos CT (V42-02 y V42-03), y pasan por la comprobación acotada (precondición 2). Todo se leyó por objetos git en `5ddeb3b4`.

| Id | Hallazgo | Qué cambió | Dónde |
|---|---|---|---|
| **V42-01** | **I-01:** la base de PB-T3a seguía atada al cierre de PB-T1c, y la fase pone PB-T2 delante | **La `BaseSha` de PB-T3a es el cierre de PB-T2c.** Cambian la precondición 1 (PB-T1 cerrada, F4B-E31; PB-T2 delante), los anclajes de §6.0 (el diff de `src` frente al cierre de PB-T2c, con sus referencias por proyecto) y la línea base de la CI, **prevista**: Core 19 094 y UI 2 450 (2 429 `Passed`, 21 `Skipped`), el GREEN de PB-T2c en su paquete; **GREEN previsto de PB-T3a: Core 19 107 y UI 2 451**. En cadena, previstos: PB-T3b, Core 19 128; PB-T3c, Core 19 133; UI 2 451 | Cabecera; precondiciones 1 y 2; §1; §5.2 (CT-01, nota); §6.0; §6.1; §6.2; §6.3 |
| **V42-02** | **I-02:** A2-11 de PB-T2 (el paso 4 de D-PB-06: el exacto y el estilo hasta el DWG; «la composición», a PB-T3) no estaba en PB-T3 | **CT-PBT3B-01 gana su parte (ii)**, leída del **sobre que sale**: las once filas de `SC1.FilasF()`, copiadas como lista literal, con el valor de CT-PBTC-02 (i), y las dos filas de estilo (`pb-06-seguridad` STANDARD-ISO y `pb-07-combinado` SIMPLEX), con el diseño del sobre y el sistema de `BuildForMember`. **Se elige CT-PBT3B-01 y no CT-PBT3A-06,** porque el sobre que sale solo existe en el plan de PB-T3b, y es lo que MUTATE escribe. §5.6 gana la fila de OB-06.3 y A2-11. No cambia ningún recuento | §5.3 (CT-01 y el porqué); §5.6 |
| **V42-03** | **I-03:** A2-12 de PB-T2 (M09) no estaba en PB-T3, y la tabla de grupos del clasificador no tenía regla por defecto | **La prueba por reflexión, y la fila por defecto para el clasificador:** CT-PBT3A-09 gana su parte (3), que exige fila en el mapa para cada una de las 27 propiedades declaradas de los tres documentos. La tabla de D-PBT3-07 gana la fila por defecto de la carga útil: el grupo sale del dueño en el mapa (`Editor` → `Rack`; si no, `OutsideEditor`). **Se elige la prueba,** porque M09 es un riesgo de futuro que la equivalencia declarada no vigila, y usa solo API pública existente; la mitad del clasificador va por la fila por defecto, porque probarla exigiría un miembro nuevo en la API cerrada. No cambia ningún recuento | D-PBT3-05 (paso 4); D-PBT3-07; §5.2 (CT-09 y el porqué); §5.6 |
| **V42-04** | **I-04:** dos arrastres a PB-T4 se contradecían sobre el tipo del borrador del panel (A2-4 de PB-T2 frente al borrador completo de PB-T3) | **A-T3-11:** PB-T4 adapta el puerto de la ventana (`IPushBackDraftStatePort`) al borrador completo, conservando el `Origin` del estado actual. **A2-4 de PB-T2 queda sustituido.** Una frase en §8.3, y la ida y vuelta con el asiento en D-PBT3-03 | D-PBT3-03; §8.3; §10 (A-T3-11) |
| V42-05 | **M-01:** las nueve clases de prueba de PB-T2 no estaban en ningún `ForbiddenWriteScope` de PB-T3 | §6.0 añade las clases de PB-T2, tomadas del contrato de PB-T2c. Previsto, a medir: en PB-T3a, Core 29 y UI 68 en `tests/`, 89 en `src/` y 203 en total | §6.0 |
| V42-06 | **M-02:** la red de PB-T3 no incluía PB-T2 | Las ocho clases de prueba de PB-T2, sin `ExpectRed`, en los `RequiredTests` de §6.1 (y, por remisión, en los de PB-T3b y PB-T3c); PB-T2 en §5.1 y en §4; la contención de F4B-E30 también sobre esas clases de UI | §4; §5.1; §6.0; §6.1 |
| V42-07 | **M-03:** A-T3-9 y V2-22 mandaban la adopción por el asiento a «PB-T2 o PB-T4» | **Solo PB-T4:** PB-T2 va antes de que exista el asiento y congela `Recompute` | D-PBT3-03; §10 (A-T3-9) |
| V42-08 | **M-04:** la precondición 7, R-T3-09 y la cabecera tenían el orden y el estado viejos | El orden real (PB-T2 delante), F4B-E31 y el estado de PB-T2 en `5ddeb3b4` | Cabecera; precondiciones 1 y 7; §7 (R-T3-09) |
| V42-09 | **M-05:** un nombre nulo que «Guardar en biblioteca» deja en `""` lo informa el clasificador como `ChangedByUser`, y el escritor no renombra | **Se declara** para la presentación de PB-T4 (A-T3-3), sin cambiar el clasificador: la v4.2 no cambia el diseño | §8.3; §10 (A-T3-3) |
| V42-10 | **M-06:** las anclas de W de la v4.1 ya estaban desplazadas, y PB-T2b las moverá otra vez | **Por su contenido:** la adopción de `Recompute`, su `catch`, el `Recompute();` final de `LoadFromModel`, los ensambladores y la costura. Sus líneas, en una nota con su historia y a medir en la remedición tras el cierre de PB-T2c | Precondición 2; D-PBT3-03; §5.2 (CT-01, nota); §6.0; §10 (A-T3-9) |
| V42-11 | **M-07:** Q-PBT2-2 (A) deja A2-7 sin objeto, y PB-T3 no lo decía | Una frase en §8.3: la compuerta de «Aplicar» y la barrera de PREPARE son la misma (`PushBackEditorOutputGate`), y la barrera sigue haciendo falta para el piloto y para un rack que llega bloqueado. F4B-E28 entre las autoridades | §1; §8.3 |
| V42-12 | **I-05, lo que es del paquete:** la custodia no conocía la v4.1, la v4.2 ni la §12 de la revisión | **La precondición 9:** qué custodia la sesión en la emisión de PB-T3a (el paquete v4.2, los deltas hasta `v42-delta.md`, la revisión con su §12 y, desde V42-13, su §13, la de compatibilidad y el registro). **La precondición 10:** la autorización de emisión de PB-T3a sigue pendiente del Owner (DU-1 del generador); no se decide aquí. Lo del generador lo hace otro agente | Precondiciones 9 y 10 |
| V42-13 | **Q-01** de la recomprobación de la v4.2 (§13): la precondición 9 custodiaba `independent-review-v1.md` «con sus §1 a §12», y la revisión ya tiene su §13 | La precondición 9 custodia la revisión **con sus §1 a §13** (la §12, sobre la v4; la §13, sobre la v4.2). La cabecera nombra la §13 entre las revisiones. Se aplica en el mismo archivo de la v4.2, que aún no está custodiado | Cabecera; precondición 9 |

## Registro de cambios v4 → v4.1 (la recomprobación de la v4)

**Origen.** La recomprobación de la v4 (`independent-review-v1.md`, §12) dio **LISTO PARA EMITIR TRAS REMEDICIÓN, B/I/M 0/0/1**, con un menor, P-01: la v4 se escribió antes de F4B-E29. **El diseño no cambia, ni ningún CT, ni la letra de los contratos de la variante (A).** Todo se leyó por objetos git en `d8ccbcb8`.

| Id | Hallazgo | Qué cambió | Dónde |
|---|---|---|---|
| V41-01 | **P-01:** la precondición 4 seguía pendiente | **Cumplida:** D-1 decidida, F4B-E29, en `d8ccbcb8`. F4B-E29 entra en §1 y en §8.1, y la cabecera ya no anuncia decisiones pendientes | Precondición 4; cabecera; §1; §8.1 |
| V41-02 | **P-01:** el motivo literal del `Skip` decía `F4B-E<n>` | El motivo cita **`F4B-E29`** | §5.3 |
| V41-03 | **P-01:** §8.2, D-PBT3-09b y §6.2 seguían ofreciendo las dos variantes | **(A), la elegida.** §8.2 trae la respuesta literal del Owner. El texto de (A′) queda marcado como no elegido y fuera del contrato: D-PBT3-09b, su PBT3B-C13 y los «con (A′)» de §5.3, §6.0 y §6.2. Lo normativo de §5.3, §6.0 y §6.2 queda solo con (A): los seis `Skip`, los 76 archivos, los 11 del `AllowedWriteScope`, los seis `[Fact]` de CT-14 y su `MinSelected` 6 | Cabecera; D-PBT3-01; D-PBT3-09b; §4; §5.3; §6.0; §6.2; §8.2; §10 |
| V41-04 | **P-01:** dos anclas se movieron | En `d8ccbcb8`:<br>• el RED de PB-T1c (`e5a21989`) añadió una línea a W (`:168`, la costura inerte), así que **W:659-704 pasa a W:660-705**, y, con la misma causa, la del `catch` de esa adopción, **W:662-670 pasa a W:663-671** (una tercera que la recomprobación no nombraba);<br>• el `BodyOf` de S-5 está en **IntegrationGuard:882**, no en `:881` (ya era así en `833ea6bf`).<br><br>W:145-146 queda antes de la línea nueva y no cambia. La remedición (precondición 2) vuelve a medir las demás | D-PBT3-03; §8.2; §9; §10 (A-T3-9) |
| V41-05 | Un hecho nuevo en `d8ccbcb8` que no afecta a PB-T3 (§12.5 de la recomprobación) | **F4B-E30:** tras ciertos gestos de la parte compuesta de la ventana, `LastComputation.Design` va una pasada por detrás. CT-PBT3A-01 compara tras `LoadExisting`, que termina con una pasada (W:440), así que no le afecta. Va a PB-T4, cuando el estado vuelva de la ventana al borrador (A-T3-4) | §1; §5.2 (CT-01, nota); §10 (A-T3-4) |
| V41-06 | **Del redactor:** una incidencia de procedimiento repetida | Una orden de la v4.1 encadenó `python -I -c "pass"`, que la regla prohíbe. No leyó ni escribió nada. Es el modo de fallo de la v1 y de H-12 de PB-T1: se devuelve al Coordinator como modo repetido | §9 |

## Registro de cambios v3 → v4 (la recomprobación de la v3)

> Histórico. La ancla IntegrationGuard:881 de V4-01 se corrige a `:882` en V41-04.

**Origen.** La recomprobación de la v3 (`independent-review-v1.md`, §11) dio **NECESITA v4, B/I/M 0/1/1**. Los siete hallazgos de la §10 y V3-08 quedan resueltos. Queda un importante, O-01: los números de PB-T4 en D-1. Y un menor, O-02: «ocho en total». **El diseño no cambia, ni ningún CT, ni ningún contrato fuera de la letra de E-1, E-4 y E-5.** `pbt3-measurements.md` no cambia: la v4 no mide nada nuevo, salvo tres anclas de guardas de F4 en `833ea6bf` (V4-01), que cita aquí.

| Id | Hallazgo | Qué cambió | Dónde |
|---|---|---|---|
| **V4-01** | **O-01** (IMPORTANTE): los números de PB-T4 no eran exactos. E-2 y E-6 empiezan con `Assert.Empty(E1Violations(real))` y `Assert.Empty(E5Violations(real))`, así que caen con E-1 y E-5. Y la letra de E-1 (A′), E-4 (A′) y E-5 dejaba la cuenta abierta | **La letra, estricta:** E-1 (con (A′)), E-4 (con (A′)) y E-5 exigen que existan todos los miembros que nombran, como las pruebas que sustituyen (S-1, PrepareGuard:107; S-4, UpdateGuard:140; S-5, IntegrationGuard:881).<br><br>**La cuenta:**<br>• (A): **seis ahora y tres en PB-T4** (E-3, E-5 y E-6);<br>• (A′): **tres ahora y seis en PB-T4** (S-3, S-5, S-6, E-1, E-2 y E-4);<br>• **nueve** en las dos.<br><br>**La letra laxa** («sobre cada uno que exista»): se analiza en §8.2. Daría siete con (A) y seis con (A′), pero sus equivalentes cubrirían menos que las pruebas que sustituyen. No es una opción más; la descripción de la opción (A′) dice su cuenta. La pregunta sigue en 373 caracteres | §5.3 (E-1, E-4, E-5 y la letra); D-PBT3-09b; §8.2 (tabla, letra laxa, pregunta y opciones); cabecera; §10 (A-T3-1) |
| V4-02 | **O-02:** «En total, en F4B: ocho» y «Ocho en total» se leían como el total de F4B | «En total, por D-1» en las dos tablas, y «Nueve por esta decisión» en las opciones. Una frase en D-PBT3-09b y en §8.2: el retiro del piloto en PB-T4 (A-T3-1) trae además las suyas, iguales en las dos | D-PBT3-09b; §8.2; cabecera |

## Registro de cambios v2 → v3 (la recomprobación de la v2)

> Histórico. Los números de PB-T4 de V3-01 («dos» y «cinco», «ocho» en total) se corrigen en V4-01.

**Origen.** La recomprobación de la v2 (`independent-review-v1.md`, §10) dio **NECESITA v3, B/I/M 0/3/4**:
- B-01 queda bien resuelto;
- D-1 es exacta: seis con (A) y tres con (A′);
- los 28 hallazgos de la v1, resueltos;
- **quedan tres importantes,** todos de texto: la pregunta de D-1 no dice lo mismo de las dos opciones sobre PB-T4 (N-01); la parada C13 de PB-T3b prohíbe lo que exige (A) (N-02); y la medición sobre el GREEN choca con C02 (N-03);
- **y cuatro menores** (N-04 a N-07).

**Ninguno pide una decisión del Owner. D-1 sigue siendo la única, con la pregunta corregida.** Además, al aplicar N-03, el redactor corrige una colisión de numeración (V3-08).

| Id | Hallazgo | Qué cambió | Dónde |
|---|---|---|---|
| **V3-01** | **N-01** (IMPORTANTE): la pregunta de D-1 no era simétrica. La (A) callaba lo que deja a PB-T4, y la tabla lo llamaba «ajustar E-5» | Las dos opciones dicen lo que dejan a PB-T4:<br>• **(A): dos**, porque al quitar el piloto PB-T4 sustituye con registro **E-3** (su cláusula «el piloto conserva `ViewUnavailablePrefix`») y **E-5** («las dos `Execute`»);<br>• **(A′): cinco**, porque PB-T4 sustituye **S-3, S-5 y S-6** (el código del piloto que se va), **E-1** (la sobrecarga de cuatro argumentos) y **E-4** («por corrida»).<br><br>Las dos suman **ocho** en F4B. «Ajustar» pasa a «sustituir con registro»: editar una prueba existente está prohibido (INV-07; OB-T1B.1). La pregunta, con 373 caracteres | §8.2; D-PBT3-09b; §10 (A-T3-1); `v3-delta.md` |
| **V3-02** | **N-02** (IMPORTANTE): PBT3B-C13 paraba ante «any change of the signature of an existing member», también de los privados que (A) tiene que cambiar (`ComposeMemberEnvelope` y todos los de `Execution<TState>`). Además, C13 de PB-T3b y PB-T3c cerraba los nombres nuevos, mientras que INV-13 común admitía métodos privados: dos reglas | **Una regla por contrato:**<br>• **PB-T3a** (solo archivos nuevos): INV-13 admite los privados, y PBT3A-C13 lo cita (como en la v2);<br>• **PB-T3b y PB-T3c** (archivos existentes): **lista cerrada por nombre y acotada a los archivos tocados**, como PBT1C-C13 de la v2.8 custodiada (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`, `:1338` en `833ea6bf`). Para ante una declaración cuyo nombre no existe **en esos archivos** en `BaseSha`, o ante una declaración más de un nombre que sí existe (una sobrecarga), de cualquier visibilidad, **salvo las que prescribe el diseño**, que se enumeran (sobrecargas incluidas). Ningún ayudante privado fuera de la lista; INV-13 lo dice así.<br><br>**La firma:** solo para ante el cambio de un miembro **público o interno** existente (como INV-PBT3B-03). Se declaran los privados que (A) cambia de firma:<br>• `ComposeMemberEnvelope` gana `string name`;<br>• `Execution` pasa a `Execution<TState>`: pierde `_input`, su constructor gana los tres delegados, y sus demás miembros cambian `PushBackPilotDraftState` por `TState`. Leído en `833ea6bf`, son los de la recomprobación más `ChooseOrigin`, `SurvivorsAndLayers` y el campo `_draft`.<br><br>**La variante (A′)** tiene su propia lista, normativa, que el generador copia si el Owner la elige. La entrega trae **el censo de declaraciones** de los archivos tocados, como INV-PBT1C-05 de la v2.8 | §6.0 (INV-13); §6.2 (PBT3B-C13 y `ExpectedEvidence`); §6.3 (PBT3C-C13 y `ExpectedEvidence`); D-PBT3-09 a 11; R-18 |
| **V3-03** | **N-03** (IMPORTANTE): la medición sobre el GREEN obliga a reactivar en local las pruebas con `Skip`, y el invariante común 07 y C02 lo prohibían | El invariante común 07 y C02 exceptúan **solo** esa reactivación: **temporal, local y sin commit**, la de la medición (INV-PBT3B-15, V3-08). El Worker la deshace antes del commit GREEN. La entrega muestra que el `git diff -U0 BaseSha..CurrentSha` de los archivos de guardas solo lleva las líneas de atributo con `Skip` | §6.0; §5.3; §6.2 |
| V3-04 | **N-04:** con (A′), la tabla decía «Quedan vacías: ninguna», pero cuatro filas de S-2 pasan en vacío | Con (A′), S-2 falla solo en su fila 1 y queda vacía en las filas 2 a 5. Se salta igual. La medición esperada trae, en las dos variantes, el censo de las cinco filas de S-2 (`from-presente`, PrepareGuard:136) y el de S-6 (`vacia`: S-5 falla sobre el mismo texto) | §8.2; §5.3; D-PBT3-09b |
| V3-05 | **N-05:** con (A′), el `ForbiddenWriteScope` deja fuera solo dos archivos de guardas | «Menos los archivos de guardas de la variante elegida»: 76 de 79 con (A) y 77 con (A′) | §6.0; R-15 |
| V3-06 | **N-06:** CT-PBT3B-07 (ii) pedía `Ready` en un segundo `Prepare` sobre un borrador limpio | Antes del segundo `Prepare`, el estado actual se cambia (la escala a 3,0) | §5.3 |
| V3-07 | **N-07:** el estado de PB-T1 cambió | Se mide en `833ea6bf`, la HEAD de la rama:<br>• **PB-T1b, CERRADA** (F4B-E26; CI del GREEN `44438060317`: Core 19 086, UI 2 437 con 2 416 pasadas y 21 omitidas);<br>• **PB-T1c, EMITIDA** (F4B-E27; contrato con `AuthorityRevision` `af46fcd5`, y la v2.8 custodiada);<br>• **F4B-E24 y F4B-E25, ya en git** (`f4b-dispositions.md:684` y `:693`; `F4B-DRAFTS/coordinator/`);<br>• `git diff 2f5d3dec 833ea6bf -- src`, vacío.<br><br>Las precondiciones 1 y 8, R-0 y R-17 se ponen al día. No cambia ningún CT | Precondiciones; §1; R-0; R-17 |
| V3-08 | **Del redactor,** al aplicar N-03: la medición se llamaba INV-PBT3B-07, el mismo número que el invariante común 07 (las redes) en el contrato de PB-T3b | La medición pasa a **INV-PBT3B-15**, detrás de los comunes 07 a 14. El común 07 sigue siendo el de las redes. El registro v1 → v2 conserva el nombre antiguo | §5.3; §6.2; §8.2 |

## Registro de cambios v1 → v2 (la revisión independiente de la v1 y las lecciones nuevas de PB-T1b)

**Origen.** La revisión independiente de la v1 (`independent-review-v1.md`) dio **NECESITA v2, B/I/M 1/11/16**:
- un bloqueante: borrar la vista de origen rompe POST (B-01);
- D-1 mal enunciada (I-01 e I-02);
- cuatro contratos o pruebas que no se pueden cumplir (I-03 a I-06);
- tres pruebas sin receta o que no discriminan (I-07, I-08 e I-11);
- un hecho anticuado (I-09);
- el registro de D-2 (I-10);
- dieciséis menores.

La sesión pidió además aplicar cuatro lecciones nuevas de PB-T1b:
- F4B-E25;
- las paradas por declaración nueva definidas por nombre (PBT1C-C13 de la v2.8 de PB-T1);
- separar en los invariantes lo normativo de las notas;
- el `Trailer` real en la `ExpectedEvidence`.

**El bloqueante se resuelve en el diseño, sin decisión nueva** (V2-01): POST relee con un origen reanclado. **D-1 sigue siendo la única decisión material** y se reformula con la variante (A′) (V2-02). **No aparece ninguna otra decisión nueva.**

| Id | Hallazgo | Qué cambió | Dónde |
|---|---|---|---|
| **V2-01** | **B-01** (BLOQUEANTE): si el origen del borrador es un lateral que queda obsoleto, MUTATE lo borra y POST relee con `plan.OriginKey` (UR:481), así que da «el origen ya no está en el dibujo» (IR:162-166) y `AppliedPostRereadFailed` con la escritura confirmada | **Origen de POST reanclado:** el plan gana `PostOriginKey`, que es `OriginKey` si no se borra, y si no, la primera clave `Redraw` en ordinal. POST relee con ella, y el borrador se rebasa con esa lectura. En el piloto (sin borrados) es siempre `OriginKey`: no cambia. **Es la igualdad con el clásico**, que también borra la vista elegida si queda alguna superviviente (RPC:198, :348-352). Son reglas técnicas del escritor, no del Owner (la revisión, §9). Casos nuevos en CT-PBT3B-02 y 07, una fila del Smoke-PB y el arrastre A-T3-8 a PB-T4 (la selección del panel) | D-PBT3-09 (h); D-PBT3-10; §5.3; §5.5; §10; R-13 |
| **V2-02** | **I-01:** D-1 mal enunciada. (a) S-6, y tres filas de S-2, pasan en vacío; (b) la exactitud no se mide «sobre el esqueleto»; (c) falta la variante (A′) | D-1 dice ahora «cinco que fallan y una que queda vacía». La exactitud se mide **sobre el texto del GREEN**, con una corrida local temporal de los seis sin `Skip` y el censo de las precondiciones de sus autopruebas (INV-PBT3B-07; parada PBT3B-C16). Se añade la variante **(A′)**: código duplicado y privado para el modelo completo en los mismos archivos. **Medido por lectura: exige S-1, S-2 y también S-4**, porque S-4 cuenta un solo `mutation.Dispose(` y un solo `scope.Dispose(` en todo UR (UpdateGuard:134-135). No son solo dos, como decía la revisión. La pregunta, autocontenida y lista (§8.2) | §8.2; D-PBT3-09b; R-6; §9 |
| **V2-03** | **I-02:** E-3 no cubría todo lo de S-3 | E-3 lee `BuildCore` **y** el cuerpo de `PushBackViewDispositions.Of`, con todas las agujas de S-3 (`PreflightInnerSources(` y `RackSystemKind.PushBack` incluidas) | §5.3 |
| **V2-04** | **I-03:** E-2 y E-6 pasaban en el RED | Las dos empiezan con `Assert.Contains` de cada literal de la forma nueva y con `Assert.Empty(<reglas>(real))`: fallan por aserción en el RED | §5.3 |
| **V2-05** | **I-04:** CT-PBT3C-05 leía la fachada, que el RED no creaba | El RED de PB-T3c crea el **esqueleto inerte** de la fachada (la clase y el método, que lanza `NotSupportedException`). CT-01 a 04 siguen fallando por aserción; CT-05 pasa | §5.4; §6.3 |
| **V2-06** | **I-05:** el `ForbiddenWriteScope` de PB-T3b cortaba su `AllowedWriteScope` | En PB-T3b, el prefijo `tests/RackCad.Tests/Workspace/` de la lista curada de PB-T0 se sustituye por **la lista explícita de sus archivos menos los tres de guardas**: 76 de 79 en `2f5d3dec`, que vuelve a medir el generador. El doble pasa a `tests/RackCad.Tests/PushBackFullWriteSupport.cs`, fuera de `Workspace/`. El §6.0 deja de contradecirse | §5.0; §6.0; §6.2; R-15 |
| **V2-07** | **I-06:** la API cerrada de PB-T3a no tenía constructores para tres tipos | El §3 da todos los constructores con su visibilidad y firma (los `internal` de `PushBackViewDispositionPlan`, `PushBackChangeEntry` y `PushBackConflictClassification`; los privados de los tipos con fábrica; el privado explícito del comparador). PBT3A-C13 se define **por nombre** (V2-30) | D-PBT3-02 a D-PBT3-07; §6.1 |
| **V2-08** | **I-07:** CT-PBT3B-10 no llegaba a AUTH-13 en la escritura (salía Conflict) | La receta: un borrador iniciado con una lectura admitida **fabricada**, cuya `StaleBase` es la de `BuildStaleBase` del mismo escaneo con el campo desconocido. Se declara que en uso real AUTH-13 actúa al abrir, y que al escribir solo aparece el Conflict | §5.3; D-PBT3-13 |
| **V2-09** | **I-08:** la barrera del piloto, lo único de PB-T3 que llega al panel antes de PB-T4, no tenía prueba; el resumen y INV-PBT3B-04 decían «sin cambio» | CT-PBT3B-06 gana un borrador **del piloto** sobre un diseño de un sentido con `Composite.StructureOverrideA = 1`, con la escala cambiada. Da `Invalid(BlockedPrefix + StatusOr)` y se comprueba a sí mismo (`admitido=True bloqueado=True`; si no, C07). **El cambio visible se declara:** el resumen, D-PBT3-09 (d2), INV-PBT3B-04 y §8.3. `Blocking()` de `PushBackOutputGateTests` es compuesto y el piloto no lo admite (R-14). El censo de los diseños de las clases de F4 entra en la remedición | Cabecera; D-PBT3-09; §5.3; §6.2; §8.3; R-14 |
| **V2-10** | **I-09:** la Snapshot I estaba anticuada | Snapshot J (F4B-E19; `F4-ENVIRONMENT/snapshot-J/snapshot-j.json`) en la precondición 6 y en P-01, y en la lista de la remedición | Precondiciones; §1; §6.0 |
| **V2-11** | **I-10:** D-2 necesitaba su registro declarado | Una entrada en `superseding-characterization.md`, **sin `Skip`**, que escribe la sesión en la emisión de PB-T3c. Recoge la autoridad (D-PB-04, OB-04.1), la guarda que sigue en verde (UpdateGuard:182-212, con su comentario :188, «Never: the lock, an erasure…»), las nuevas (CT-PBT3C-01 a 04) y los dos comentarios de WR que cambian (:26 y :225) | §8.2; D-PBT3-11; §6.3 |
| **V2-12** | **I-11:** CT-PBT3A-09 y 10 sin líneas literales; las hojas derivadas | **Oráculo mecánico independiente:** `PilotKit.JsonDiff` (de las pruebas, sin editarlo) sobre los mismos tres textos, con las reglas de clase, grupo y estructura de D-PBT3-07, comparadas por (nivel, ruta, grupo, clase, estructural), sin el texto de los valores. Más **entradas ancla literales** que no dependen de las hojas derivadas. **Las hojas derivadas** (una longitud de módulo recalculada, por ejemplo) se informan con el grupo de su ruta: el clasificador informa lo que cambia en el diseño persistido; cómo presentarlo es de PB-T4 | D-PBT3-07; §5.2 |
| V2-13 | **M-01:** CT-PBT3B-04 (iv), tautológico | Oráculo: el hecho medido de M-i (BM:62-66), generalizado: todas las ranuras de B en `AbsentSlotsB` y `SideB` nulo | §5.3 |
| V2-14 | **M-02:** CT-PBT3B-12 podía pasar sobre el esqueleto | La línea lleva `prepare=Ready` y `corrida=Applied` | §5.3 |
| V2-15 | **M-03:** sin precisar la sobrecarga de `PushBackPilotPostExpectation` y las coincidencias del doble | Escrito: el constructor nuevo y `Equals` con `ErasedKeys`; el doble con borrado da `PreparedBlockMatches` también para las claves a borrar | D-PBT3-09; §5.0 |
| V2-16 | **M-04:** sin prueba del `Precheck` completo ni del orden del `Prepare` completo | **CT-PBT3B-15**, nuevo | §5.3; §6.2 |
| V2-17 | **M-05:** el ancla de [PKC2] | §6.7 en `:579-581`; las celdas, §6.8, `:655` | §1; §6.0 |
| V2-18 | **M-06:** §7.3, §7.4 y D-PB-10 atribuidos al [OD] con la numeración de [P] | «[P] §7.3 y §7.4, con el texto literal del [OD]»; D-PB-10 sí es un bloque del [OD] | §1 |
| V2-19 | **M-07:** la decisión de la ExtensionData no estaba en el repo ni en las precondiciones | Es **F4B-E24** (el número lo fijó la sesión: v2.8 de PB-T1, V28-25). Precondición 8: F4B-E24 y F4B-E25, registradas en `F4B-DRAFTS/` y en `f4b-dispositions.md` y dentro de la `AuthorityRevision`. A-T3-7 sale | Precondiciones; §1; §8.1; §10 |
| V2-20 | **M-08:** el texto del frontal B era inexacto | «… es un frontal del lado B en un rack sin lado B: el panel no la borra ni la redibuja; usa RACKEDITAR.» Se declara la limitación: un rack de un sentido que trae un frontal B de una conversión clásica (BM:25) queda fuera del panel, como hoy con el piloto | D-PBT3-09; D-PBT3-14 |
| V2-21 | **M-09:** el coste era más que el comparador | R-T3-07 cuenta las pasadas de un «Actualizar» completo: la materialización del piloto y tres `Pass` al leer en PREPARE; tres más en `ForWrite`; lo mismo en POST. Va a R-PB-06 y al Smoke-PB | §7 |
| V2-22 | **M-10:** `Pass` en producto duplica la adopción de la ventana | Se declara interina; arrastre A-T3-9 (que la ventana use el asiento: PB-T2 o PB-T4) | D-PBT3-03; §10 |
| V2-23 | **M-11:** CT-PBT3A-04 iba por muestra | Recorre las 28 rutas de `I58F1Matrix.Nested("pushback")` (`:97-102`), copiadas, sobre `I58F1Fixtures.Rich("pushback")` con `I58F1Fixtures.Set`, sin editarlos | §5.2; R-16 |
| V2-24 | **M-12:** la incidencia del heredoc repite la de PB-T1 (medición H-12) | **Se devuelve al Coordinator como modo de fallo repetido**, antes de clasificarla. La v2 no usó ningún heredoc | §9; R-12 |
| V2-25 | **M-13:** CT-PBT3C-01 no prohibía que la fachada nombre el piloto | Prohíbe `NamesThePilot` de G-9 (IntegrationGuard:679) | §5.4 |
| V2-26 | **M-14:** el motivo del `Skip` sin su F4B-E; el `MinSelected` de `Workspace` | El motivo cita «F4B-E<n> (D-1)», con el número que fije la sesión al registrar D-1. El `MinSelected` se fija en la emisión desde la TRX de la CI del RED | §5.3; §6.2 |
| V2-27 | **M-15:** dependencia cíclica `Pilot` ↔ `Draft` | Se declara interina hasta PB-T4, que retira el piloto (R-T3-19; A-T3-10) | D-PBT3-04; §7; §10 |
| V2-28 | **M-16:** erratas | R-6 sin el «único `Create`»; INV-PBT3A-06 igual que CT-12 (admite PP y UR); el comentario de WR:225 | R-6; §6.1; D-PBT3-11 |
| V2-29 | **F4B-E25** (lección de PB-T1b; `F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md`) | La regla del Owner sustituye al texto de la v1 sobre la `AuthorityRevision` de una corrección, con sus tres condiciones y la excepción de las rutas `EXTERNAL`. Cada contrato declara sus rutas `UNIT_CHANGE` (PP y UR en PB-T3b; WR en PB-T3c) y cita la regla | §1; §6.0; §8.1 |
| V2-30 | **Las paradas por declaración nueva, por nombre** (PBT1C-C13 de la v2.8 de PB-T1, V28-07 y V28-14) | PBT3A-C13, PBT3B-C13 y PBT3C-C13, en inglés, paran ante una declaración con un nombre que no existe en `BaseSha` y no está en su lista cerrada. Las sobrecargas de nombres que ya existen no son declaraciones nuevas | §6.1; §6.2; §6.3 |
| V2-31 | **Lo normativo, separado de las notas** (V28-19) | Cada invariante y cada parada con texto explicativo lo lleva en una «Nota (no va al contrato)». El generador copia solo lo normativo | §6 |
| V2-32 | **El `Trailer` real** (V28-17, la mitad (b) de PBT1B-IR-M14) | La `ExpectedEvidence` de los tres contratos pide que el `Trailer` de `worker-handoff.json` sea el trailer real de los commits RED y GREEN | §6.1; §6.2; §6.3 |
| V2-33 | El texto de la revisión sobre RVA (§3): «cualquier otro → `NoAddress`» no se alcanza, porque `DecodePushBack` solo da planta, lateral y corte (`RackViewCodec.cs:263-286`) | Se mantiene como defensa, declarado como inalcanzable hoy | D-PBT3-06 |
| V2-34 | La corrección de PB-T1b ya está en git: RED `126fd65f` y GREEN `a18ba345` | `git diff 2f5d3dec a18ba345 -- src` vacío; cambian cinco archivos de pruebas de PB-T1b, con el mismo número de métodos. La precondición 1 y R-0 lo recogen | Precondiciones; R-0 |

**Lo que pide el Owner** ([OD], fila de PB-T3 del §5 de [P], literal):

> ACCEPT IN PRINCIPLE. The Workspace authored writer may expand to full Push Back design. It must eventually support, as characterized/authorized: field changes; modules/fronts; side B; rename; deleting obsolete views; blocking authored write on diagnostics/errors; view creation only if PB-T0 subsequently authorizes it. Still require: PREPARE -> MUTATE -> POST and one logical Update.

**Lo que propone este paquete:**
- **Un solo escritor, generalizado donde está.** El plan y la corrida del piloto pasan a servir dos modelos de borrador:
  - el del piloto, conectado al panel (solo la escala), con **un único cambio visible**: la barrera. Un rack de un sentido cuyo diseño trae un diagnóstico bloqueante ya no se escribe desde el panel: sale «El diseño tiene un error que impide escribirlo: …» (V2-09). En todo lo demás, el piloto no cambia;
  - el completo, sobre `PushBackEditableState`, probado en Core con dobles y **sin conectar** hasta PB-T4.
  
  No hay un segundo camino de escritura (F4: «NO SECOND WRITE PATH»). La variante (A′) de D-1 lo habría cambiado hasta PB-T4, y no se eligió (F4B-E29).
- **Tres subtareas secuenciales**, cada una con su cadena y su presupuesto de 1/3 (OB-10.1):
  - **PB-T3a — el modelo** (`F4B-PBT3A-FULL-WRITE-MODEL`): solo archivos nuevos de Application. El borrador completo, su asiento, su lector, la composición para escribir y para «Conservar mi borrador», la disposición de las vistas y el clasificador de D-PB-05. Ninguna guarda de F4 cambia.
  - **PB-T3b — la escritura** (`F4B-PBT3B-FULL-WRITE-RUN`): el plan y la corrida del piloto, generalizados en su sitio, con la barrera de diagnósticos, el borrado de laterales obsoletos, el renombrado, `ComparePost` genérico y la capacidad de borrar como interfaz nueva. El Plugin no cambia.
  - **PB-T3c — el borrado en el dibujo** (`F4B-PBT3C-FULL-WRITE-ERASE`): la primitiva caller-owned del Plugin y su uso por el escritor, con sus guardas de texto.
- **Sin crear vistas** (H-PB-16 confirmado por el bloque M; fila 1 del §6.4). **Sin tocar** RACKEDITAR, la ventana, el anfitrión ni el puerto del panel.

**Decisiones materiales para el Owner: ninguna pendiente.** D-1, la única, está **decidida** (F4B-E29; §8.2): «Un solo código (Recomendado)», la variante (A). Para que el escritor renombre y borre, PB-T3b sustituye **seis** pruebas de F4 (cinco fallan y una queda vacía), cada una con su registro y su equivalente, y PB-T4 sustituirá **tres** de esos equivalentes al quitar el piloto: **nueve por D-1**. El retiro del piloto en PB-T4 trae además las suyas (A-T3-1). La variante (A′), no elegida, queda como registro (V41-03).

D-2 no es una decisión: es un registro declarado.

**Precondiciones de emisión** (de cada subtarea, salvo que se diga otra cosa):
1. **PB-T1 y PB-T2 cerradas** (V42-01; V42-08). PB-T2 va delante de PB-T3 en la fase:
   - **PB-T1, CERRADA:** PB-T1b (F4B-E26) y PB-T1c (F4B-E31, custodia `5a8366c1`; CI del GREEN `44463212627`: Core 19 088, UI 2 439), medido en `5ddeb3b4`;
   - **PB-T2:** PB-T2a, emitida (F4B-E32) y con el GREEN de su intento 2 en `5ddeb3b4` (F4B-E33), sin cierre todavía; PB-T2b y PB-T2c, por emitir.
   
   **La `BaseSha` de PB-T3a es el cierre de PB-T2c:** desciende de su entrada «Cierre de F4B-PBT2C-WORKSPACE-CARRYOVER» en `f4b-dispositions.md`, y `git diff <cierre de PB-T2c> <BaseSha> -- src` es vacío.
2. **La remedición, sobre el cierre de PB-T2c** (V42-01 y V42-10):
   - las anclas del registro (R-1 a R-18) y el censo de guardas de R-6;
   - **las anclas de W, por su contenido:** la adopción de `Recompute` (de `computation = assembler.BuildFrom(` a `RefreshModuleSelector();`) y su `catch`; el `Recompute();` final de `LoadFromModel`; los dos ensambladores; y la costura `EditableStateForTest`. PB-T2b las mueve; el contenido de `Recompute`, `LoadFromModel` y `ReadInputs` es el del cierre de PB-T1c (INV-PBT2B-04 de PB-T2), y se comprueba;
   - la línea base de la CI y la Snapshot vigente;
   - los recuentos y los alcances de §6.0, con las clases de PB-T2 (V42-05 y V42-06);
   - `f4b-dispositions.md` hasta el cierre de PB-T2c, para las autoridades y el estado; y que los nombres nuevos de PB-T3 siguen libres;
   - **el censo de los diseños que usan las clases de comportamiento del piloto de F4 (§4), con su estado de bloqueo** (V2-09).
   
   Una diferencia que cambie un CT, el diseño o la lista de D-1 vuelve a una comprobación acotada.
   - **Nota (no va al contrato).** La lista completa de lo que hay que volver a medir está en la §3 de la revisión de compatibilidad (`compat-pbt3-vs-pbt2.md`).
3. **La revisión independiente de este paquete** y, por subtarea, su comprobación acotada.
4. **Para PB-T3b y PB-T3c: D-1 decidida por el Owner** (§8.2). **CUMPLIDA** (V41-01): F4B-E29, «Un solo código (Recomendado)», la variante (A); `f4b-dispositions.md:800` y `F4B-DRAFTS/coordinator/owner-pbt3-d1-decision.md`, custodiados en `d8ccbcb8`. Queda que F4B-E29 entre en la `AuthorityRevision` de PB-T3b (cualquiera igual o posterior a `d8ccbcb8`).
5. **Para PB-T3b: PB-T3a cerrada.** Para PB-T3c: PB-T3b cerrada.
6. Las celdas no están `STALE` (vencen el 2026-12-29); el `config.toml` coincide con la **Snapshot J** (F4B-E19; `docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json`); el binario de Codex es el acreditado.
7. **PB-T2 va antes** (V42-08): PB-T2a, PB-T2b y PB-T2c, y después PB-T3a, PB-T3b y PB-T3c. Las delegaciones son sucesivas, cada una sobre la punta de la anterior, y los alcances son disjuntos (§7, R-T3-09).
8. **F4B-E24 (la ExtensionData anidada) y F4B-E25 (las rutas `UNIT_CHANGE` en una corrección), dentro de la `AuthorityRevision` del contrato** (V2-19). **Registradas ya** en `833ea6bf`: `f4b-dispositions.md:684` y `:693`, y sus archivos en `F4B-DRAFTS/coordinator/` (V3-07; R-17). Cualquier `AuthorityRevision` igual o posterior a `95373af6` las contiene.
9. **La custodia, en el commit de emisión de PB-T3a** (V42-12): la sesión custodia, redactados para el repo público:
   - **el paquete, en su versión v4.2,** como `docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md` (la ruta que cita F4B-E29);
   - **su historia:** `v2-delta.md`, `v3-delta.md`, `v4-delta.md`, `v41-delta.md` y `v42-delta.md`;
   - **las revisiones:** `independent-review-v1.md` con sus §1 a §13 (la §12, LISTO PARA EMITIR TRAS REMEDICIÓN, 0/0/1, sobre la v4; y la §13, LISTO PARA EMITIR TRAS REMEDICIÓN, 0/0/1, sobre la v4.2; V42-13) y la revisión de compatibilidad `compat-pbt3-vs-pbt2.md` (COMPATIBLE CON REMEDICIÓN, 0/5/7, sobre la v4.1), con `compat_measure.py`;
   - **el registro de mediciones** `pbt3-measurements.md`, puesto al día por la remedición.
   
   F4B-E29 ya está en git (`d8ccbcb8`): se cita, no se vuelve a custodiar.
   - **Nota (no va al contrato).** Los nombres de custodia que no fija este paquete (la salida de la remedición, la comprobación acotada y la salida en seco del generador) los propone el generador (sus notas, DU-2). Los scripts con rutas locales se redactan o no se custodian (DU-11).
10. **La autorización de emisión de PB-T3a: pendiente del Owner** (V42-12). F4B-E28 (pregunta 4) cubre PB-T2a, PB-T2b y PB-T2c, y F4B-E29 solo decide D-1. Este paquete no la decide: es una pregunta al Owner (DU-1 de las notas del generador).

---

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| **[OD]** (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | La fila de PB-T3 (arriba); D-PB-04 (borrar sí; crear, solo tras PB-T0); D-PB-05 (los pasos 1 a 7 y «an informed destructive choice»); D-PB-10 (1/3; el intento 3 vuelve al Owner); el bloque sobre T1B («Do not silently weaken the old guard»; «create a declared superseding characterization/change record») y el de RACKEDITAR, que [P] transcribe en su §7.3 y §7.4 (V2-18) |
| **[P]** (`docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1) | La fila de PB-T3 del §5 (`:505`); H-PB-16 a H-PB-23; OB-02.2; OB-03.1 y OB-03.2; OB-04.1 y OB-04.2; OB-05.1 a OB-05.5; §6.4; OB-10.1; §7.3 y §7.4 (los textos literales del [OD]); OB-T1B.1 a OB-T1B.3; OB-RE.1; R-PB-03, R-PB-05, R-PB-06, R-PB-09 y R-PB-11 |
| **El bloque M de PB-T0** (`F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md`) | H-PB-16 confirmado y la fila 1 del §6.4 resuelta (`:92-97`); lo que deja para PB-T3 (`:133-139`) |
| **[PKT1]**, el paquete de PB-T1 (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`; la v2.7 al redactar la v1 y la v2.8 custodiada en `af46fcd5`) | D-PBT1-02 a D-PBT1-11 (el tipo, las cargas, la salida, el copiador, el comparador, el mapa y el criterio de bloqueo); D-PBT1-15 (D2); §5.0 (recetas y `Pasada`); §6.0 (lo común de los contratos); §10 (arrastres A-2 y A-3) |
| **D2 del Owner** (F4B-E11, `F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`) | Las dos referencias compartidas, las diez condiciones y el límite «STOP and reopen L-1» |
| **F4B-E24, la ExtensionData anidada** (2026-11-19; `F4B-DRAFTS/coordinator/owner-f4b-dpb07-extensiondata-decisions.md`, pregunta 2; la vuelta, `F4B-DRAFTS/pbfull/owner-return-extensiondata.md`; el número, de la v2.8 de PB-T1, V28-25; en git desde `95373af6`, `f4b-dispositions.md:684`) | «Basta así (Recomendado)»: el panel mantiene el rechazo de AUTH-13 y PB-T3 lo fija con una prueba; la pérdida de RACKEDITAR es una limitación conocida anterior a F4B. Precondición 8 |
| **F4B-E25, las rutas `UNIT_CHANGE` en una corrección** (2026-11-19; `F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md`; la vuelta, `F4B-DRAFTS/pbt1b/owner-return-verify-stop.md`; en git desde `95373af6`, `f4b-dispositions.md:693`) | «Regla F4B y reverificar (Recomendado)»: en una corrección de F4B, una ruta `UNIT_CHANGE` de `git diff --name-only AuthorityRevision BaseSha` no incumple 16.3 si se cumplen sus tres condiciones; las `EXTERNAL`, sin excepción (§6.0; V2-29). Precondición 8 |
| **F4B-E19, la Snapshot J** (`F4B-DRAFTS/coordinator/owner-config-snapshot-j-decision.md`; `F4-ENVIRONMENT/snapshot-J/snapshot-j.json`) | La línea base activa del Controller, que sustituye a la I (V2-10) |
| **F4B-E21** (`F4B-DRAFTS/coordinator/owner-f4b-opaque-collections-decision.md`) | La sesión pone las ocho colecciones opacas del contrato |
| **F4B-E22** (`F4B-DRAFTS/coordinator/owner-pbt1b-rework-decisions.md`) | Las ratificaciones M10 y la mitad (a) de M14 valen solo para PB-T1b: este paquete escribe el texto correcto (§6.0) |
| **La v2.8 de [PKT1], custodiada** (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md` en `af46fcd5`; PB-T1c emitida con ella, F4B-E27) | Las lecciones que aplica este paquete:<br>• PBT1C-C13 por nombre, acotada a los archivos tocados y con las sobrecargas (`:1338` en `833ea6bf`; V28-07, V28-14 y V28-33);<br>• lo normativo separado de las notas (V28-19);<br>• el `Trailer` real (V28-17) |
| **F4B-E26 y F4B-E27** (`f4b-dispositions.md:711` y `:761` en `833ea6bf`) | El cierre de PB-T1b (CI del GREEN `44438060317`) y la emisión de PB-T1c (V3-07) |
| **F4B-E29, D-1** (2026-11-19; `f4b-dispositions.md:800` en `d8ccbcb8`; texto literal en `F4B-DRAFTS/coordinator/owner-pbt3-d1-decision.md`) | «Un solo código (Recomendado)»: la variante (A). PB-T3b sustituye seis pruebas de F4 con su registro y su equivalente; PB-T4 sustituirá tres equivalentes; nueve por esta decisión. Precondición 4 (V41-01) |
| **F4B-E30** (`f4b-dispositions.md:814` en `d8ccbcb8`) | PB-T1c, intento 1, parado con PBT1C-C07 tras el RED `e5a21989`; corrección en el intento 2. No afecta a PB-T3 (V41-05). Trae también la contención: ninguna clase de los filtros de UI crea un `RackFrameConfiguratorWindow` real (§6.0) |
| **F4B-E31** (`f4b-dispositions.md:839` en `5ddeb3b4`) | El cierre de PB-T1c (PB-T1 completa), con la CI del GREEN `44463212627`: Core 19 088 y UI 2 439 (V42-01) |
| **F4B-E28** (`f4b-dispositions.md:788`; `F4B-DRAFTS/coordinator/owner-pbt2-decisions.md`) | Las decisiones de PB-T2: Q-PBT2-1 (A), aplicar sin cerrar; Q-PBT2-2 (A), «Aplicar» rechaza un diseño bloqueado, con la misma compuerta que la barrera de PREPARE; Q-PBT2-3 (A); y Q-PBT2-4, la emisión de PB-T2a a PB-T2c, que **no** cubre PB-T3 (precondición 10). Deja sin objeto A2-7 (V42-11) |
| **[P2]**, el paquete de PB-T2 (`docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md`; v2.6 para PB-T2c, con los contratos en seco de PB-T2b y PB-T2c) y F4B-E32 y F4B-E33 (`f4b-dispositions.md:875` y `:908`) | El orden de la fase y la base de PB-T3a (V42-01); el puerto al borrador y el lanzamiento tipado de PB-T2a, ya en `5ddeb3b4` (`PushBackDraftStatePort.cs`); INV-PBT2B-04 (congela `Recompute`, `LoadFromModel` y `ReadInputs`); los arrastres **A2-4** (sustituido por A-T3-11), **A2-7** (sin objeto), **A2-11** (CT-PBT3B-01 (ii)) y **A2-12** (CT-PBT3A-09 (3)); L-PBT2-12; la línea base prevista del GREEN de PB-T2c (V42-02 a V42-11) |
| **La revisión de compatibilidad** (`F4B-DRAFTS/pbt3/compat-pbt3-vs-pbt2.md`, con `compat_measure.py`) | COMPATIBLE CON REMEDICIÓN, B/I/M 0/5/7, sin decisiones nuevas del Owner: los hallazgos que aplica la v4.2 (V42-01 a V42-12) |
| **[DP]** (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | Los sustitutos de nc1 y nc2 solo con la firma EXACTA; una replanificación solo por la comparación literal |
| **[PKC-C]** y **[PKC2]**, los paquetes de PB-TC y PB-TC2 (`docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md`, `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md`) | El método del `ForbiddenWriteScope` ([PKC-C]); las 17 paradas estándar y la raíz ([PKC2] §6.7, `:579-581`); las celdas ([PKC2] §6.8, `:655`) (V2-17) |
| **El cierre de F4** (`F4-DRAFTS/gate-close.md`) y los contratos de F4-T4 (`docs/initiatives/I-64-f4-t4*-gate-contract-package.md`) | El PREPARE, MUTATE y POST que se generaliza; S2-03 (un paso de Deshacer); las guardas G-1 a G-10 |
| **I-54 G4B** (`CustomPropertiesEnvelopeGuardTests`, `ExpectedComposeCensus`) | El censo cerrado de los que componen un sobre |
| **I-58 (AUTH-13)** y su matriz (`I58F1Matrix.cs:58-64`, `:97-102`) | El rechazo de todo campo desconocido a cualquier nivel |
| ADR-0029 D12 y D13; Proposal V5 D-10, D-13, D-15 (punto 5) y D-16 | Sin modelo paralelo; la caracterización no se edita para que pase; Actualizar todo o nada y un paso de Deshacer; escritor caller-owned; sin cambio observable en RACKEDITAR |
| `docs/AUTOMATION_PLAN.md` §16.3, §16.5, §16.8 a §16.11; `docs/automation/agent-execution/README.md` §4 y §10 | Las autoridades por clase, A1-A8, el RED de la cadena, las comprobaciones, los términos y las paradas |

## 2. Hechos medidos en `2f5d3dec`

El detalle, con sus anclas, está en `pbt3-measurements.md` (registros R-0 a R-17; R-13 a R-17 son de la v2). Este apartado resume lo que decide el diseño.

### 2.1 Lo que escribe hoy el piloto y lo que le falta, punto por punto del [OD]

| Punto | Hoy | Lo que falta | Registro |
|---|---|---|---|
| **Campos** | Solo la escala. `ComposeForWrite` rechaza cualquier otra diferencia y rematerializa la base | Escribir todo el estado editable | R-1 |
| **Módulos y frentes** | Rechazados: el estado del piloto no los lleva sin pérdida | El estado completo de PB-T1 | R-1 |
| **Lado B** | Un rack compuesto o con cola aparcada es «fuera del piloto (F5)» | Admitir A/B en el modelo completo; la cola aparcada sigue fuera (D-PBT1-10) | R-1 |
| **Renombrar** | El nombre sale del sobre releído de cada vista | El nombre del borrador en el sobre; los nombres de las definiciones salen de él | R-1, R-2 |
| **Borrar vistas obsoletas** | Una vista no disponible hace la preparación `Invalid` («El piloto no borra vistas…») | Borrar laterales obsoletos dentro de la transacción, con la regla del último vínculo | R-1, R-2, R-3 |
| **Barrera de diagnósticos** | No existe | `PushBackEditorOutputGate` en PREPARE, para los dos modelos. En el piloto es un cambio visible, pero solo sobre un diseño bloqueado (V2-09) | R-1, R-4, R-14 |
| **PREPARE, MUTATE y POST; Deshacer** | Una transacción en MUTATE; POST en el mismo contexto de comando; Smoke-2 midió una sola entrada de Deshacer | Lo mismo, con los borrados dentro de la misma transacción | R-1 |
| **`ComparePost`** | Claves, sobres en ordinal y escala | Que las claves borradas ya no estén; la escala del modelo que corresponda | R-1 |
| **AUTH-13** | El lector rechaza todo campo desconocido a cualquier nivel, con aviso y remisión a RACKEDITAR | Fijarlo con una prueba en el camino completo (F4B-E24) | R-1, R-5 |
| **Conflictos** | Comparación cruda de la base antes de la admisión (PREPARE) y otra vez en MUTATE | Sin cambio; el destino incluye las vistas a borrar | R-1 |

### 2.2 Lo que hace el clásico y PB-T3 iguala en resultado

- **Nunca crea vistas al actualizar** (H-PB-16, M-1 y M-4).
- **Borra solo laterales obsoletos:** los que no tienen corte en el sistema nuevo. Solo si queda alguna vista superviviente; si no, no escribe nada (M-2b, BM:61).
- **Renombra todas las vistas** con un solo nombre (el de la ventana o, en blanco, el del sobre elegido). Con colisión, la definición no cambia de nombre (M-3).
- **El frontal B de un rack que deja de ser compuesto** se redibuja como frontal de un sentido y conserva `Section` 2 (M-4, H-PB-18). PB-T3 conserva por defecto el rechazo del panel (§6.4, fila 3; D-PBT3-06).
- **Un `U` deshace cada «Actualizar»**, también con borrado o renombrado (M-5).
- **Las vistas se cuentan por definición**, con o sin referencia (BM:49-53). El panel exige además una superviviente con referencia (C-F4-05, R-10).
- **El borrado alcanza todas las referencias directas** de la definición, también las anidadas, y purga después las definiciones anidadas (RCS:218-275; BM:54-60).

### 2.3 Lo que PB-T1b deja en Application

`PushBackEditableState` con sus cargas, `BuildInputs` y las reglas de c-02 y c-09; el copiador y el comparador; el mapa de propiedad; `PushBackEditorOutputGate` (R-4). **No hay API de asiento:** la pasada (`Pasada`) solo existe copiada en las pruebas. PB-T3a la añade al producto, como prevé A-2.

### 2.4 Las guardas que atan el diseño

Las de F4-T4 fijan por texto el plan, la corrida, el puerto y el escritor (R-6):
- **con el diseño (A),** cinco métodos fallan (S-1 a S-5) y uno queda vacío (S-6), y tres filas de S-2 también. **Con la variante (A′),** no elegida, habrían fallado tres (S-1, S-2 y S-4). Es D-1, decidida: (A) (F4B-E29; §8.2);
- **el resto** se mantiene en verde con elecciones de diseño explícitas (§4).

El censo de I-54 de los que componen un sobre es cerrado: ningún archivo nuevo compone ni construye un `RackEmbedDocument`.

---

## 3. El diseño

### D-PBT3-01. Un escritor, dos modelos de borrador y tres subtareas

**Por qué no un segundo escritor:**
- las guardas de F4 prohíben otro camino de escritura (G-2, G-7);
- [OD] habla de «The Workspace authored writer», en singular;
- ADR-0029 D12 prohíbe un modelo paralelo.

**La variante (A′) de D-1** (D-PBT3-09b), **no elegida** (F4B-E29), habría duplicado en privado, en los mismos archivos y con el mismo puerto, el armado y la corrida del modelo completo hasta PB-T4. El Owner eligió (A): un solo escritor (§8.2; V41-03).

**Por qué el modelo completo no se conecta en PB-T3:**
- conectar el borrador completo al panel es cambiar el tipo del borrador del anfitrión y del puerto del panel, que es «el puerto generalizado» de PB-T4 ([P] §5);
- OB-05.2 manda dejar conectada hasta PB-T4 la semántica de hoy de «Conservar mi borrador» (`KeepPilotEditsOn`);
- quitar el rechazo A/B del panel (OB-03.2) solo tiene sentido cuando el panel use el estado completo: va con PB-T4 (A-3).

**Por qué tres subtareas:**
- tres regímenes distintos: solo archivos nuevos (PB-T3a), cambios en archivos de F4 con sustituciones declaradas (PB-T3b) y Plugin con guardas de texto (PB-T3c);
- PB-T3a se puede emitir sin D-1, y PB-T3b no;
- el tamaño: PB-T3 entera serían unos 34 CT, seis sustituciones y cambios en cinco archivos de producto, más que PB-T1b (17 CT).

**La secuencia:** PB-T3a → PB-T3b → PB-T3c, cada una desde la punta que dejó cerrada la anterior, sin rebase, con delegaciones sucesivas (`AUTOMATION_PLAN.md:557-558`). **No se funden PB-T3b y PB-T3c:** la revisión no lo recomienda (§4 de la revisión). Cambiarían las invariantes, las paradas, los filtros y el registro de D-2, y el GREEN fundido sería el mayor cambio de producto de F4B en un solo 1/3.

### D-PBT3-02. El borrador completo (PB-T3a)

**Dónde:** una carpeta nueva, `src/RackCad.Application/Systems/PushBack/Draft/`, con `namespace RackCad.Application.Systems.PushBack.Draft` de bloque (`NamespaceFolderGuardTests`). Ni en `Pilot/`, cuyas guardas atan el texto de esa carpeta, ni en `Workspace/`.

**`PushBackFullDraftState.cs`:**

```csharp
public sealed class PushBackFullDraftState
{
    public PushBackFullDraftState(PushBackEditableState editable, string origin); // ArgumentNullException si alguno es nulo
    public PushBackEditableState Editable { get; }
    public string Origin { get; } // el texto crudo del sobre del que se materializó; en el estado inicial, siempre la base actual
}
```

**`PushBackFullDraftStateComparer.cs`:**

```csharp
public sealed class PushBackFullDraftStateComparer : IEqualityComparer<PushBackFullDraftState>
{
    private PushBackFullDraftStateComparer();                         // explícito: solo lo usa Instance (V2-07)
    public static PushBackFullDraftStateComparer Instance { get; }
    public bool Equals(PushBackFullDraftState x, PushBackFullDraftState y);
    public int GetHashCode(PushBackFullDraftState obj);
    public static PushBackFullDraftState Copy(PushBackFullDraftState state);
}
```

- **`Equals`:** `ReferenceEquals` da `true`; un nulo da `false`; si no, `Origin` en ordinal y `PushBackEditableStateComparer.Instance.Equals(x.Editable, y.Editable)`.
- **`GetHashCode`:** `HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.Origin), PushBackEditableStateComparer.Instance.GetHashCode(obj.Editable))`.
- **`Copy`:** `new PushBackFullDraftState(PushBackEditableStateComparer.Copy(state.Editable), state.Origin)`. **D2:** la copia comparte solo lo que comparte el copiador de PB-T1b.

### D-PBT3-03. El asiento y el materializador (PB-T3a)

**`PushBackEditableStateSeat.cs`** (la «API de asiento» de A-2):

```csharp
public static class PushBackEditableStateSeat
{
    public static PushBackEditorComputation Pass(
        PushBackEditableState state, PushBackEditorDesignAssembler assembler,
        PushBackCompositeEditorAssembler compositeAssembler, PushBackSide frontalSide);
    public static PushBackEditableState LoadExisting(PushBackDesign design, string rackName, RackCatalog catalog);
}
```

- **`Pass`** es la copia literal del ayudante `Pasada` de [PKT1] §5.0, que reproduce la adopción de `Recompute` en W (de `computation = assembler.BuildFrom(` a `RefreshModuleSelector();`), con todas sus sentencias y en su orden. La única diferencia es que pasa de privado de prueba a público de producto. Una excepción del armado da `null`, como el `catch (Exception error)` de esa adopción en W.
  - **Nota (no va al contrato).** Las anclas de W van por su contenido (V42-10). Sus líneas se miden sobre la `BaseSha` en la remedición, porque PB-T2b las mueve: la adopción estaba en W:659-704 en el cierre de PB-T1b, en :660-705 en `d8ccbcb8` (V41-04) y en :586-631 en `5ddeb3b4`; su `catch`, en :662-670, :663-671 y :589-597.
- **`LoadExisting`** reproduce la fila «`LoadExisting` o `LoadDesignForNew`» de la tabla de pasadas de [PKT1] §5.0:
  1. `design == null` → `ArgumentNullException` antes de crear nada (M11 de PB-T1b);
  2. un estado nuevo; dos ensambladores nuevos con el catálogo, como los dos campos de ensamblador de W (W:145-146 en `5ddeb3b4`; a medir tras PB-T2b);
  3. `LoadNew(catalog)`; `Pass`; `SeedDefaultSafety(catalog)`; `Pass`;
  4. `LoadDesign(design, rackName, assembler.Resolver)`; `Pass`;
  5. siempre con `frontalSide = PushBackSide.A`.
  
  Devuelve el estado.
- **Por qué esta secuencia:** es la que CT-PBT1C-01 iguala con la ventana tras `LoadExisting`. Así el borrador completo nace igual que el estado que tendría la ventana al abrir ese rack, que es lo que PB-T4 abrirá con «Editar todo».
- **Interino, declarado (V2-22, M-10 de la revisión):** `Pass` duplica la adopción de `Recompute` en la ventana. Mientras la ventana no use el asiento, hay dos implementaciones de la misma secuencia, y solo las ata CT-PBT3A-01. El arrastre A-T3-9 lleva a **PB-T4** que la ventana use `PushBackEditableStateSeat.Pass` (V42-07): PB-T2 va antes de que exista el asiento y congela `Recompute` (INV-PBT2B-04 de PB-T2).
- **El asiento y el estado de PB-T2** (V42-04): la receta `Estado(sobre)` de PB-T2 es la misma secuencia que `LoadExisting` del asiento, así que CT-PBT2B-02 ya prueba que la ventana en WORKSPACE conserva un estado nacido así. Es la ida y vuelta que PB-T4 necesita con «Editar todo» (A-T3-11).

**`PushBackFullDraftMaterializer.cs`:**

```csharp
public sealed class PushBackFullDraftMaterialization
{
    private PushBackFullDraftMaterialization(PushBackPilotMaterializationKind kind, PushBackFullDraftState state, string reason);
    public PushBackPilotMaterializationKind Kind { get; }
    public bool IsAdmitted { get; }
    public PushBackFullDraftState State { get; }   // solo si Admitted
    public string Reason { get; }                  // null si Admitted
    public static PushBackFullDraftMaterialization Admitted(PushBackFullDraftState state);
    public static PushBackFullDraftMaterialization OutOfPilot(string reason);
    public static PushBackFullDraftMaterialization Failed(string reason);
}

public static class PushBackFullDraftMaterializer
{
    public static PushBackFullDraftMaterialization Materialize(string originText, RackCatalog catalog);
}
```

**`Materialize`**, total y en este orden. Los textos son las constantes del piloto, que no se duplican:
1. texto en blanco → `Failed(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic)`; catálogo nulo → `Failed(PushBackPilotMaterializer.CatalogRequiredDiagnostic)`;
2. `new RackEmbedStore().Deserialize(originText)`:
   - nulo → `Failed(UnreadableOrFutureDiagnostic)`;
   - `Kind` distinto de `RackEmbedDocument.KindPushBack` en ordinal → `Failed(KindDivergentDiagnostic)`;
3. `new RackProjectStore().TryDeserialize(embed.Design, …)`:
   - falso → `Failed(UnreadableOrFutureDiagnostic)`;
   - sin `PushBackDesign` → `Failed(NoDesignDiagnostic)`;
4. `PushBackEditableState.SupportOf(design) == DormantCompositeTail` → `OutOfPilot(PushBackPilotMaterializer.DormantTailOutOfPilotReason)` (D-PBT1-10; OB-03.1);
5. `Admitted(new PushBackFullDraftState(PushBackEditableStateSeat.LoadExisting(design, embed.Name, catalog), originText))`;
6. cualquier excepción → `Failed(PushBackPilotMaterializer.MaterializeFailurePrefix + ex.Message)`.

**La diferencia con el piloto:** un diseño compuesto **se admite** (el piloto lo rechaza en PM:148). Es el modelo completo, sin conectar al panel. Lo demás es igual.

### D-PBT3-04. El lector completo, sobre el lector del piloto (PB-T3a)

**`PushBackFullDraftReader.cs`:**

```csharp
public sealed class PushBackFullIntentResult
{
    internal PushBackFullIntentResult(DraftStaleBase staleBase, DraftIntentRead<PushBackFullDraftState> read,
        string originKey, string detail, string classicEditorHint);
    public DraftStaleBase StaleBase { get; }
    public DraftIntentRead<PushBackFullDraftState> Read { get; }
    public string OriginKey { get; }
    public string Detail { get; }
    public string ClassicEditorHint { get; }
}

public static class PushBackFullDraftReader
{
    public static PushBackFullIntentResult Read(PushBackPilotIntentScan scan, RackCatalog catalog);
}
```

**`Read`**, total. No cambia el lector del piloto, ni sus 14 reglas, ni AUTH-13:
1. `var pilot = PushBackPilotIntentReader.Read(scan, catalog);` (una sola vez);
2. **si `pilot.Read.IsAdmitted`**, o si el rechazo es **exactamente** el compuesto (`Diagnostic == PushBackPilotIntentReader.OutOfPilotDiagnostic` y `pilot.Detail == PushBackPilotMaterializer.CompositeOutOfPilotReason`, en ordinal; solo pasa tras las reglas 1 a 11, AUTH-13 incluida):
   - `PushBackFullDraftMaterializer.Materialize(scan.RawEnvelopes[scan.OriginKey], catalog)`;
   - `Admitted` → `DraftIntentRead.Admitted(pilot.StaleBase, state)`, con detalle y pista nulos;
   - `OutOfPilot` → `NotAdmitted(OutOfPilotDiagnostic)`, con el motivo como detalle y la pista `ClassicEditorHint`;
   - `Failed` con `UnreadableOrFutureDiagnostic` → `NotAdmitted(UnreadableDiagnostic)` con la pista;
   - otro `Failed` → `NotAdmitted(MaterializeDiagnostic)`, con el motivo como detalle y la pista;
3. **si no:** `NotAdmitted` con el diagnóstico, el detalle y la pista del piloto;
4. la base siempre es `pilot.StaleBase` y la clave del origen, `pilot.OriginKey`; una excepción → `NotAdmitted(MaterializeDiagnostic)` con la base de `PushBackPilotIntentReader.BuildStaleBase(scan)`.

**Por qué así:** el lector del piloto y sus guardas no cambian (R-6), y AUTH-13 sigue decidiendo antes que cualquier materialización. La admisión del compuesto depende de dos constantes exactas del piloto. CT-PBT3A-03 la fija regla a regla frente al lector del piloto.

**Interino, declarado (V2-27, M-15 de la revisión):** `Draft/` usa el lector y las constantes de `Pilot/`, y PP y UR usan `Draft/` (PB-T3b). Es una dependencia cíclica entre los dos espacios de nombres que ninguna guarda ve. Se acepta hasta PB-T4, que retira el piloto y lleva las constantes a `Draft/` (A-T3-10).

### D-PBT3-05. La composición para escribir (PB-T3a; OB-05.2)

**`PushBackFullComposition.cs`:**

```csharp
public sealed class PushBackFullWriteComposition
{
    private PushBackFullWriteComposition(PushBackCompositeEditorState editor, PushBackEditorInputs inputs, string name, string reason);
    public bool IsComposed { get; }
    public PushBackCompositeEditorState Editor { get; }
    public PushBackEditorInputs Inputs { get; }
    public string Name { get; }   // el del borrador, tal cual (puede ser nulo o estar en blanco)
    public string Reason { get; }
    public static PushBackFullWriteComposition Composed(PushBackCompositeEditorState editor, PushBackEditorInputs inputs, string name);
    public static PushBackFullWriteComposition Rejected(string reason);
}

public static class PushBackFullComposition
{
    public static PushBackFullWriteComposition ForWrite(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog);
    public static PushBackFullDraftState KeepOn(PushBackFullDraftState newInitial, PushBackFullDraftState current);
}
```

**`ForWrite`**, pura y total. Solo es segura **después** de que la base coincida con una relectura (H-06), como la del piloto; la llama solo el plan (D-PBT3-09):
1. `initial` o `current` nulos → `Rejected("Falta el estado inicial o el actual.")` (el texto de PM:201); catálogo nulo → `Rejected(PushBackPilotMaterializer.CatalogRequiredDiagnostic)`;
2. **(6) de D-T4A1-05:** `current.Editable.AnnotationScale` no finita o ≤ 0 → `Rejected(PushBackPilotMaterializer.InvalidScaleReason)`;
3. **(3) de D-T4A1-05:**
   - `var fresh = PushBackFullDraftMaterializer.Materialize(initial.Origin, catalog)`; si no se admite → `Rejected(fresh.Reason ?? UnreadableOrFutureDiagnostic)`;
   - si `!PushBackFullDraftStateComparer.Instance.Equals(initial, fresh.State)` → `Rejected(PushBackPilotMaterializer.BaseChangedReason)`;
4. **la composición:**
   - `var copy = PushBackEditableStateComparer.Copy(current.Editable)`;
   - `Composed(copy.Editor, copy.BuildInputs(new PushBackEditorDesignAssembler(catalog)), copy.Name)`;
5. una excepción → `Rejected(PushBackPilotMaterializer.ComposeFailurePrefix + ex.Message)`.

**El reparto de los pasos 4 a 7 de D-PB-05:**

| Paso | Dónde se cumple |
|---|---|
| **4.** «all fields OWNED by the full Push Back editor come from the user's draft» | El editor, los portadores y el nombre salen de `current` (paso 4 de `ForWrite`). Según el mapa de PB-T1b, todo el estado editable es `Editor` (PESO:142, :165-172). Que toda propiedad declarada de los tres documentos tenga fila en el mapa lo prueba CT-PBT3A-09 (3) (V42-03; A2-12 de PB-T2, M09) |
| **5.** el sobre y la infraestructura salen del rack actual | En el plan (PB-T3b): cada miembro se compone sobre su sobre **releído** (identidad, tipo, vista, sección, `CustomProperties` y cajón) y con su proyecto **releído** como fuente (proyecto y carga útil: versión y cajón). Ya es así hoy (PP:421-422, PP:434-438; R-5) |
| **6.** «unknown/future ExtensionData must not be silently destroyed» | **Nivel superior:** se conserva por construcción (paso 5). **Anidado:** AUTH-13 rechaza el rack antes de escribir, con aviso. Es F4B-E24, y la fijan CT-PBT3A-04 y CT-PBT3B-10 |
| **7.** identidad, esquema y sesión no vuelven atrás | El GUID y el esquema salen del sobre releído (paso 5); la sesión es la del `DraftSession`; `Origin` sale siempre de la base nueva (`KeepOn`) |

**`KeepOn`**, la composición de «Conservar mi borrador» (OB-05.2), **sin conectar** (PB-T4, OB-05.5):
- `newInitial` o `current` nulos → `ArgumentNullException`;
- si no, `new PushBackFullDraftState(PushBackEditableStateComparer.Copy(current.Editable), newInitial.Origin)`.

Es pura: no muta ninguno de los dos argumentos. `DraftSession.KeepMineOnCurrentVersion` la recibe como `keepOn` (DS:469-486) y vuelve a copiar.

**Lo que no hace:** no compara el borrador con nada que «el editor no edite», porque en el modelo completo no hay nada así (OB-05.1). No toca `KeepPilotEditsOn`, que sigue conectada (OB-05.2).

### D-PBT3-06. La disposición de las vistas (PB-T3a; H-PB-17, H-PB-18, §6.4)

**`PushBackViewDisposition.cs`:**

```csharp
public enum PushBackViewDispositionKind { Redraw = 0, ObsoleteLateral = 1, FrontalBOfOneWay = 2, NoAddress = 3 }

public sealed class PushBackViewDispositionPlan
{
    internal PushBackViewDispositionPlan(IEnumerable<KeyValuePair<string, PushBackViewDispositionKind>> members); // ordena por clave y deriva las listas
    public IReadOnlyList<KeyValuePair<string, PushBackViewDispositionKind>> Members { get; } // por clave, ordinal
    public IReadOnlyList<string> Redraw { get; }
    public IReadOnlyList<string> ObsoleteLaterals { get; }
    public IReadOnlyList<string> FrontalBOfOneWay { get; }
    public IReadOnlyList<string> NoAddress { get; }
    public bool AllObsolete { get; } // hay miembros y todos son ObsoleteLateral
}

public static class PushBackViewDispositions
{
    public static PushBackViewDispositionPlan Of(
        IEnumerable<KeyValuePair<string, RackEmbedDocument>> members, PushBackSystem outgoing, RackCatalog catalog);
}
```

**La regla** es la del clásico (RPC:268-271, :320-333) y la de la disponibilidad pura (RVA:94-116):
- los hechos se calculan **una vez**: `new PushBackViewAvailabilityFacts(new PushBackSystemLateralBuilder().Cortes(outgoing, catalog).Select(c => c.PostIndex), outgoing.IsComposite)`;
- por miembro: `RackViewCodec.Decode(envelope.Kind, envelope.View, envelope.Section)`:
  - sin dirección → `NoAddress`;
  - `RackViewAvailability.Evaluate(...) == Available` → `Redraw`;
  - si no, un lateral → `ObsoleteLateral`;
  - un frontal del lado B → `FrontalBOfOneWay`;
  - cualquier otro → `NoAddress`. **Hoy no se alcanza** (V2-33): `RackViewCodec.DecodePushBack` solo da planta, lateral y corte (`RackViewCodec.cs:263-286`), y la planta siempre está disponible. Se mantiene como defensa.

**La regla del frontal B (§6.4, fila 3; R-PB-11).** **Se propone el valor por defecto:** el panel **rechaza** la edición, con un texto que remite a RACKEDITAR (D-PBT3-09). Las dos alternativas, con la evidencia de M-4 (BM:25, :67-72):

| Alternativa | Contra |
|---|---|
| (1) Borrarlo como vista hermana obsoleta (D-PB-04 lo autoriza) | Hace distinto al panel y al clásico, que lo conserva (M-4) |
| (2) Redibujarlo como el clásico | Deja un sobre con `Section` 2, la entrada del lado B, en un rack sin lado B. Y su renombrado choca con el frontal de A, si existe, lo que no se midió (BM:72) |

La revisión de este paquete decide. **No es del Owner**, salvo que la revisión la considere material (BM:96).

### D-PBT3-07. El clasificador de D-PB-05 (PB-T3a; OB-05.3), sin conectar

**`PushBackConflictClassifier.cs`:**

```csharp
public enum PushBackChangeGroup { Name = 0, Rack = 1, Annotations = 2, Safety = 3, Fronts = 4, Modules = 5, SideB = 6, Composite = 7, OutsideEditor = 8 }
public enum PushBackChangeClass { ChangedExternally = 0, ChangedByUser = 1, Overlapping = 2 }

public sealed class PushBackChangeEntry
{
    internal PushBackChangeEntry(PushBackDocumentLevel level, string path, PushBackChangeGroup group,
        PushBackChangeClass changeClass, bool isStructural, string baseValue, string externalValue, string draftValue);
    public PushBackDocumentLevel Level { get; }
    public string Path { get; }          // $.A.B[3].C, con los índices reales
    public PushBackChangeGroup Group { get; }
    public PushBackChangeClass Class { get; }
    public bool IsStructural { get; }
    public string BaseValue { get; }     // el texto JSON de la hoja, o «<ausente>»
    public string ExternalValue { get; }
    public string DraftValue { get; }
}

public sealed class PushBackConflictClassification
{
    internal PushBackConflictClassification(IEnumerable<PushBackChangeEntry> entries,
        IEnumerable<string> viewsToErase, IEnumerable<string> viewsRejected, bool allViewsObsolete); // ordena y deriva las listas
    public IReadOnlyList<PushBackChangeEntry> Entries { get; }  // por nivel y por ruta, ordinal
    public IReadOnlyList<PushBackChangeEntry> ChangedExternally { get; }
    public IReadOnlyList<PushBackChangeEntry> ChangedByUser { get; }
    public IReadOnlyList<PushBackChangeEntry> Overlapping { get; }
    public IReadOnlyList<PushBackChangeEntry> Structural { get; }
    public IReadOnlyList<string> ViewsToErase { get; }
    public IReadOnlyList<string> ViewsRejected { get; }
    public bool AllViewsObsolete { get; }
}

public static class PushBackConflictClassifier
{
    public static PushBackConflictClassification Classify(
        PushBackFullDraftState baseState, PushBackFullDraftState currentRack, PushBackFullDraftState draft,
        IEnumerable<KeyValuePair<string, RackEmbedDocument>> currentMembers, RackCatalog catalog);
}
```

**Qué compara:**
- **El diseño de cada uno de los tres estados**, armado sobre una **copia**: `new PushBackCompositeEditorAssembler(catalog).BuildDesign(copy.Editor, copy.BuildInputs(new PushBackEditorDesignAssembler(catalog)))`. Se persiste con `new RackProjectStore().Serialize(RackProject.ForPushBack(design))`, y se toma el objeto `$.PushBack` (nivel `Payload`).
- **El nombre:** `Editable.Name` (nivel `Envelope`, `$.Name`).
- **El sobre y el proyecto de `Origin`,** en el resto de sus hojas (niveles `Envelope` y `Project`, sin `$.Design` ni `$.PushBack`).

**El diff:** hoja a hoja sobre `System.Text.Json.Nodes`, con las rutas de PESO (`$.A.B[3].C`) y los arrays por índice. Una hoja es un valor, un objeto vacío, un array vacío o `null`. El texto de la hoja es su `ToJsonString()`, o `<ausente>`. Las tres versiones son b (base), c (rack actual) y d (borrador):
- b = c = d → nada;
- c ≠ b y d = b → `ChangedExternally`;
- d ≠ b y c = b → `ChangedByUser`;
- d ≠ b y c ≠ b → `Overlapping`.

**Los grupos**, por el prefijo más largo:

| Nivel | Prefijo | Grupo |
|---|---|---|
| Envelope | `$.Name` | `Name` |
| Envelope y Project | lo demás | `OutsideEditor` (siempre gana el rack actual: lo informa, no lo decide) |
| Payload | `$.SchemaVersion` | `OutsideEditor` |
| Payload | `$.Structure.Fronts`, `$.Fronts` | `Fronts` |
| Payload | `$.Structure.Modules`, `$.Structure.HeaderLineOverrides`, `$.Structure.DerivedPostLineOverrides` | `Modules` |
| Payload | `$.SideB` | `SideB` |
| Payload | `$.Composite` | `Composite` |
| Payload | `$.Structure.SafetySelections`, `$.RearTopeSaque`, `$.RearTopePieceId`, `$.RearTopeOffCells`, `$.DefensePieceId` | `Safety` |
| Payload | `$.Structure.NumberFronts`, `.NumberLevels`, `.DrawRackName`, `.AnnotationScale`, `.Dimensions`, `.DimensionViews`, `.DimensionStyle` | `Annotations` |
| Payload | lo demás de `$.Structure` y `$.LegacyHighEndBeamPeralte` | `Rack` |
| Payload | **por defecto** (V42-03): una ruta que no cubre ningún prefijo de arriba | `Rack` si `PushBackEditableStateOwnership.Of(PushBackDocumentLevel.Payload, ruta)` es `Editor`; si no, `OutsideEditor` |

**La fila por defecto** (V42-03; A2-12 de PB-T2, M09): hace total la tabla, para que una propiedad nueva de la carga útil no caiga en el valor 0 del enumerado (`Name`) por omisión. Hoy no se alcanza: las diez propiedades declaradas de `PushBackDesignDocument` tienen prefijo arriba, y AUTH-13 rechaza los campos desconocidos antes del clasificador. Decide por el mapa, que CT-PBT3A-09 (3) ata a los tres documentos: una propiedad nueva tiene que tener fila en el mapa, y su grupo sale de su dueño.

**Estructural** («structural changes such as modules/fronts/side B», [OD]): todo `Modules`, `SideB` y `Composite`, y en `Fronts` toda entrada en la que algún valor es `<ausente>` (un frente que aparece o desaparece).

**Las hojas derivadas** (V2-12, I-11 de la revisión): el diseño persistido lleva valores que el ensamblador recalcula. Por ejemplo, la longitud de un módulo calculado cambia con la tolerancia o con el frente (`DynamicRackLevelGeometry.cs:269`). **El clasificador no distingue lo derivado de lo editado:** informa cada hoja que cambia, con el grupo de su ruta. Un cambio de tolerancia del usuario puede dar, por tanto, entradas `Rack` y también entradas `Modules` estructurales `ChangedByUser`. Es lo que cambiaría en el dibujo, y así se declara. Cómo se presenta (agruparlas, resumirlas o marcarlas como consecuencia) es de PB-T4 (OB-05.3 y OB-05.4). Ninguna prueba fija el número de hojas derivadas: CT-PBT3A-09 y 10 usan un oráculo mecánico (§5.2).

**Implicación de vistas:** `PushBackViewDispositions.Of(currentMembers, <sistema del diseño del borrador>, catalog)`:
- `ViewsToErase` = `ObsoleteLaterals`;
- `ViewsRejected` = `FrontalBOfOneWay` ∪ `NoAddress`;
- `AllViewsObsolete`.

Creadas, ninguna: F4B no crea vistas.

**Por qué no es «a generic JSON diff as the user-facing UX»:** es una clasificación interna con grupos y clases con nombre. Cómo se presenta lo decide PB-T4 (OB-05.3, OB-05.4).

### D-PBT3-08. Lo que no cambia en PB-T3a

Ningún archivo existente de `src/`. El piloto, el anfitrión, el puerto del panel, la ventana, el Plugin y Domain no se tocan. Ningún archivo nuevo compone ni construye un `RackEmbedDocument` (I-54 G4B) ni nombra `Autodesk`, `RackCad.Plugin`, `RackCad.UI`, `System.Windows`, `Transaction`, `Database`, `async`, `await` ni `Task`.

### D-PBT3-09. El plan generalizado (PB-T3b)

**Dónde:** PP, en su sitio. **Ninguna firma pública o interna existente cambia**, porque las pruebas de F4 compilan contra ellas; solo se añade.

**El estado de entrada del modelo completo**, `PushBackFullPrepareInput`, nueva en PP. Es la gemela de `PushBackPilotPrepareInput`, que no cambia:

```csharp
public sealed class PushBackFullPrepareInput
{
    private PushBackFullPrepareInput(...);          // una sola construcción, en From
    public string RackId { get; }
    public DraftStaleBase StaleBase { get; }
    public PushBackFullDraftState InitialDraftState { get; }  // copia
    public PushBackFullDraftState CurrentDraftState { get; }  // copia
    public bool HasPendingText { get; }
    public bool IsDirty { get; }
    public DraftStatus ConsideredStatus { get; }
    public static PushBackFullPrepareInput From(DraftSession<PushBackFullDraftState> draft);
}
```

**El planificador** gana `public static PushBackPilotPrepareResult Prepare(PushBackFullPrepareInput input, PushBackFullIntentResult reread, RackCatalog catalog)`:
- con **el mismo orden** que el del piloto: NotApplicable, PendingText, NothingToApply, Conflict (`CompareWith` antes de la admisión), NotAdmitted y `Build`;
- total;
- va **después** del `Prepare` del piloto en el archivo, para que el `Prepare` y la primera `.CompareWith(` del texto sigan siendo los del piloto (guardas P5 a P7 de R-6).

**El armado común,** `BuildCore` (privado). Lo llaman los dos armados:
- **el del piloto,** `Build`, conserva en su cuerpo la única llamada textual a `PushBackPilotMaterializer.ComposeForWrite(`, después de `.CompareWith(` (R-6, P5);
- **el completo,** `BuildFull` (privado, nuevo), llama a `PushBackFullComposition.ForWrite(`.

Recibe la composición (editor, entradas y nombre, o el motivo del rechazo), si se admite borrar (piloto: no; completo: sí), la escala y la base. Hace, en este orden:
- **(a)** composición rechazada → `Invalid` con su motivo (como hoy);
- **(b) a (c)** los miembros, el origen y las fuentes interiores, **sin cambio** (PP:347-373);
- **(d)** el diseño y el sistema salientes, sin cambio (PP:375-388);
- **(d2) la barrera (nueva):** `PushBackEditorOutputGate.IsBlocked(editor, system)` → `Invalid(BlockedPrefix + PushBackEditorOutputGate.StatusOr(editor, system, string.Empty))`. **Vale para los dos modelos.**
  - **En el piloto es el único cambio de PB-T3 que llega al panel** (V2-09): un rack de un sentido cuyo diseño trae un diagnóstico bloqueante (por ejemplo, `Composite.StructureOverrideA = 1`) se escribía hoy al cambiar la escala, y desde PB-T3b recibe en el panel «El diseño tiene un error que impide escribirlo: …» y no se escribe. **Iguala al clásico,** cuya ventana no deja salir un diseño bloqueado (H-PB-21).
  - El corpus extendido de PB-T1a no está bloqueado (R-8). Los diseños de las clases de F4 se censan en la remedición (precondición 2), y la parada PBT3B-C14 cubre una prueba de F4 que dependa de escribir un diseño bloqueado. Lo fija CT-PBT3B-06;
- **(e) la disposición:** `PushBackViewDispositions.Of(<miembros releídos>, system, catalog)`, recorrida por clave en ordinal:
  - `NoAddress` → `Invalid(ViewAddressPrefix + clave + ViewAddressSuffix)`, como hoy;
  - **piloto** (no admite borrar): cualquier otro que no sea `Redraw` → `Invalid(ViewUnavailablePrefix + clave + ViewUnavailableSuffix)`, **el texto y el orden de hoy**;
  - **completo:** `FrontalBOfOneWay` → `Invalid(FrontalBOfOneWayPrefix + clave + FrontalBOfOneWaySuffix)`; `ObsoleteLateral` → a `Erasures`; y, tras el recorrido, `AllObsolete` → `Invalid(AllViewsObsoleteReason)`;
- **(f) el nombre:**
  - piloto: el de **cada** sobre releído, como hoy;
  - completo: `composition.Name`; si está en blanco, el del sobre releído del **origen**, para todos los miembros (RPC:188);
- **(g)** los planes de miembro, **solo** de las claves `Redraw`, compuestos con `ComposeMemberEnvelope(RackEmbedDocument source, string name, string view, int section, string designJson)`. Es el mismo miembro, con el nombre como parámetro. Sigue siendo el único compositor del piloto (I-54) y su primer argumento sigue siendo `source`;
- **(h) el origen de POST** (V2-01; B-01 de la revisión):
  - `PostOriginKey` = `OriginKey` si `OriginKey` no está en `Erasures`;
  - si lo está (el borrador se abrió desde un lateral que la edición deja obsoleto), la **primera clave `Redraw` en ordinal**. Siempre hay una: si todos los miembros fueran laterales obsoletos, (e) ya habría dado `AllViewsObsoleteReason`, y si quedara un frontal B o una vista sin dirección, también `Invalid`;
  - en el piloto, `Erasures` siempre está vacía y `PostOriginKey` = `OriginKey`.

  **Por qué reanclar y no rechazar:** el clásico también borra la vista elegida si queda alguna superviviente (RPC:198, :348-352), así que reanclar es la igualdad de resultado. Además, tras un «Actualizar» todos los miembros llevan el mismo diseño saliente (AUTH-13, regla 10), y cualquier miembro redibujado es un origen equivalente para releer. Es una regla técnica del escritor; la revisión la deja fuera del Owner (su §9).

**El plan** gana:
- `public IReadOnlyList<string> Erasures { get; }`, ordinal, vacía en el piloto;
- `public string PostOriginKey { get; }`.

Los dos entran en `Equals`, en ordinal y en orden. El constructor interno existente se conserva, con `Erasures` vacía y `PostOriginKey` = `originKey`, y se añade la sobrecarga `internal PushBackPilotUpdatePlan(string rackId, string originKey, string postOriginKey, IEnumerable<DraftStaleBaseEntry> expectedBase, string? expectedVariableRegistryText, double annotationScale, IEnumerable<PushBackPilotMemberPlan> members, IEnumerable<string> erasures, PushBackPilotPostExpectation postExpectation)`.

**`PushBackPilotPostExpectation`** gana `public IReadOnlyList<string> ErasedKeys { get; }`, ordinal. El constructor existente se conserva (`ErasedKeys` vacía) y se añade `internal PushBackPilotPostExpectation(double annotationScale, IEnumerable<PushBackPilotKeyedText> envelopes, IEnumerable<string> erasedKeys)`. `Equals` compara además `ErasedKeys` en orden, y `GetHashCode` no cambia (V2-15).

**Los textos nuevos** (constantes públicas de `PushBackPilotUpdatePlanner`), a revisar:

| Constante | Texto propuesto |
|---|---|
| `BlockedPrefix` | «El diseño tiene un error que impide escribirlo: » |
| `FrontalBOfOneWayPrefix` / `…Suffix` | «La vista » / « es un frontal del lado B en un rack sin lado B: el panel no la borra ni la redibuja; usa RACKEDITAR.» (V2-20) |
| `AllViewsObsoleteReason` | «Todas las vistas del rack quedarían obsoletas: no se elimina el último vínculo del rack.» |

Si la revisión considera alguno una decisión material de producto, vuelve al Owner (como OB-08.4).

**Ninguna otra declaración nueva en PP,** ni ningún ayudante privado (INV-13). La lista cerrada, por nombre y con las sobrecargas, está en PBT3B-C13 (§6.2; V3-02).

**Una limitación, declarada (V2-20, M-08 de la revisión):** un rack de un sentido que ya trae un frontal con `Section` 2, que deja RACKEDITAR al quitar B (BM:25), queda **fuera de la escritura del panel** con el modelo completo mientras exista esa vista: siempre da `FrontalBOfOneWay`. Hoy, con el piloto, ese mismo rack da «El piloto no borra vistas…». No es una regresión; sigue la regla del §6.4, fila 3.

### D-PBT3-09b. La variante (A′) de D-1: no elegida (registro)

**No va al contrato** (V41-03). El Owner eligió (A) (F4B-E29, `d8ccbcb8`). Este apartado queda como registro de lo que se preguntó: el generador no copia nada de él, y el texto de PB-T3b es el de D-PBT3-09 y D-PBT3-10.

Solo cambia el reparto del código en PP y UR. Las reglas de D-PBT3-09 y D-PBT3-10 (barrera, disposición, nombre, origen de POST y `ComparePost`) son las mismas.

| | (A), la elegida (F4B-E29) | (A′), no elegida |
|---|---|---|
| El `Build` del piloto | Llama a `BuildCore`, compartido con el completo | **Su texto no cambia**, salvo la línea de la barrera (d2) y que llama a la sobrecarga de cuatro argumentos `ComposeMemberEnvelope(source, view, section, designJson)`, que delega en la de cinco con `source.Name` |
| El armado completo | `BuildFull` → `BuildCore` | `BuildFull`, **una copia** del armado del piloto con las reglas completas, sin compartir código con el `Build` del piloto |
| La corrida | `Execution<TState>`, una para los dos modelos | `Execution` del piloto, **intacta**, y `FullExecution`, una clase privada nueva con su propio `Run`, `Prepare`, `Mutate`, `Write`, `Post` y `OutcomeOfAnEscape` |
| Las pruebas de F4 que se sustituyen en PB-T3b | **Seis:** S-1 a S-6 (cinco fallan; S-6 queda vacía) | **Tres: S-1, S-2 y S-4.** S-4 falla por sus cuentas (`Src.Count(code, @"\bmutation\.Dispose\s*\(") == 1` y lo mismo con `scope.Dispose`, UpdateGuard:134-135), porque UR tendría dos corridas. S-3, S-5 y S-6 siguen en verde |
| Las que quedan vacías | S-6, y las filas 2, 4 y 5 de S-2 | Las filas 2 a 5 de S-2 (V3-04): solo la fila 1 pierde su texto `from`. S-2 se salta igual |
| Los equivalentes | E-1 a E-6 | E-1, E-2 y E-4 (con «una liberación de cada ámbito **por corrida**») |
| Lo que deja a PB-T4, al quitar el piloto (V3-01 y V4-01) | **Tres sustituciones con registro:** E-3 (su cláusula «el piloto conserva `ViewUnavailablePrefix`»), E-5 («las dos `Execute`, que tienen que existir») y E-6, que empieza con `Assert.Empty(E5Violations(real))` y cae con E-5. Y quitar la sobrecarga del piloto de `Execute` y `Precheck` | **Seis sustituciones con registro:** S-3, S-5 y S-6 (el código del piloto que se va), E-1 (la sobrecarga de cuatro argumentos, que tiene que existir), E-2, que empieza con `Assert.Empty(E1Violations(real))` y cae con E-1, y E-4 («por corrida», con las dos corridas). Y quitar el `Build`, la `Execution`, `Execute` y `Precheck` del piloto |
| En total, por D-1 (V4-02) | **Nueve** | **Nueve** |
| Las declaraciones nuevas de PBT3B-C13 | La lista de (A) (§6.2) | La lista de (A′) (§6.2), con `FullExecution` y la sobrecarga de cinco argumentos de `ComposeMemberEnvelope` |

**Por qué no se recomendaba** (el registro de la pregunta): son dos algoritmos de escritura en el mismo archivo hasta PB-T4 (unas 350 líneas copiadas en PP y UR), con el riesgo de que un arreglo se aplique a uno y no al otro. Y no ahorra sustituciones por D-1, solo las aplaza: las dos suman nueve, y con (A′) PB-T4 carga con seis en lugar de tres (V4-01). El retiro del piloto en PB-T4 (A-T3-1) trae además las suyas, iguales en las dos (V4-02). «Sustituir con registro» y no «ajustar»: editar una prueba existente está prohibido en F4B (el invariante común 07; OB-T1B.1). **A su favor:** el texto del piloto, que es lo único que llega al panel antes de PB-T4, no cambia, salvo la barrera. **El Owner eligió (A)** (F4B-E29): el generador copia la variante (A) de PBT3B-C13, y nada de este apartado.

### D-PBT3-10. La corrida generalizada y `ComparePost` (PB-T3b)

**Dónde:** UR, en su sitio. **El API público existente no cambia:** `Precheck(DraftSession<PushBackPilotDraftState>)`, `Execute(DraftSession<PushBackPilotDraftState>, …)`, `ComparePost(plan, PushBackPilotIntentResult?)`, `DesiredNamesOf` y las constantes.

**Se añade:**

```csharp
public static PushBackPilotPrecheck Precheck(DraftSession<PushBackFullDraftState> draft);
public static DraftApplyOutcome<PushBackFullDraftState> Execute(
    DraftSession<PushBackFullDraftState> draft, PushBackPilotUpdateRequest request, IPushBackPilotUpdatePort port);
public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead);
public const string EraseUnsupportedReason = "El dibujo no admite borrar vistas en esta actualización.";
public const string PostErasedStillPresentPrefix = "POST_ERASED_STILL_PRESENT: ";
```

**El núcleo genérico** (variante (A), la elegida, F4B-E29):
- `private sealed class Execution<TState>` sustituye a `Execution`, con el mismo cuerpo. Su campo `_input` sale, y entran **cuatro campos privados** con estos nombres:
  - `_prepare`: `Func<DraftSession<TState>, PushBackPilotIntentScan, RackCatalog, PushBackPilotPrepareResult>`, la entrada y el `Prepare` del modelo;
  - `_originOf`: `Func<DraftSession<TState>, string>`, el texto del origen del estado inicial;
  - `_postRead`: `Func<PushBackPilotIntentScan, RackCatalog?, PushBackPilotUpdatePlan, (DraftIntentRead<TState> Read, string? Cause)>`, la lectura de POST del modelo y su `ComparePost`;
  - `_origin`: `string?`, el texto del origen, tomado en PREPARE (sustituye a `_input!.InitialDraftState.Origin` en `Write`);
- su constructor gana los tres delegados, y sus demás miembros conservan nombre y parámetros, con `PushBackPilotDraftState` cambiado por `TState` donde aparece: el campo `_draft`, `Run`, `OutcomeOfAnEscape`, `Prepare`, `Mutate`, `Write`, `WithAbortFailure`, `Post`, `ChooseOrigin`, `SurvivorsAndLayers`, `WithScopeDisposeFailure`, `PostFailed`, `NotAppliedFailure` y `NotAppliedPreflight` (UR:227-587, leído en `833ea6bf`; R-18; V3-02). Son cambios de firma de miembros **privados**: INV-PBT3B-03 no los cubre, y PBT3B-C13 los declara;
- los tres delegados son **lambdas dentro de cada `Execute`**, sin miembros con nombre;
- las dos `Execute` conservan el patrón de M-01: `var execution = new Execution<…>(…)` antes del `try`, y `catch (Exception ex) { return execution.OutcomeOfAnEscape(ex); }`;
- `ComparePost` (las dos sobrecargas) delega en un único privado nuevo, `ComparePostCore(PushBackPilotUpdatePlan plan, DraftAdmission admission, DraftStaleBase? staleBase, double? scale)`.

**Ningún otro miembro nuevo** en UR, ni ningún ayudante privado (INV-13). La lista cerrada, por nombre y con las sobrecargas, está en PBT3B-C13 (§6.2).

**MUTATE (`Write`)**, con estos cambios y en este orden:
1. **La capacidad, lo primero:** con `Erasures` no vacía y un ámbito de MUTATE que no implementa `IPushBackPilotViewEraser` → `NotAppliedPreflight(EraseUnsupportedReason)`, antes de la relectura y de cualquier escritura. No depende del dibujo. Con el Plugin de PB-T3b, que todavía no la implementa, es lo que saldría si un plan trajera borrados; el piloto nunca los trae.
2. **El destino:** las claves mutables releídas = `Members` ∪ `Erasures`, y `PreparedBlockMatches` es cierto para todas.
3. **Supervivientes y capas:**
   - la superviviente de C-F4-05 se busca entre los miembros que **no** están en `Erasures`;
   - las capas bloqueadas se miran en `Members` ∪ `Erasures`: una vista a borrar en una capa bloqueada es `LOCKED_LAYER` y no se escribe nada. El clásico lo intentaría y fallaría «best effort»; el panel es más estricto.
4. **Las escrituras:** una `WriteUnit` por miembro, como hoy, y después una `EraseUnit` por clave de `Erasures`, en ordinal (el orden del clásico: redibuja y después borra, RPC:272-352). `_writeStarted = true` antes de cada una. El escape y el aborto se clasifican igual que con `WriteUnit`.
5. **`Seal`**, una vez.

**POST:**
- **relee con `plan.PostOriginKey`** (UR:481 decía `plan.OriginKey`; V2-01). La lectura admitida va a `Applied(read)`, así que el borrador se rebasa con ella:
  - la base nueva son los miembros que quedan, sin los borrados;
  - el estado inicial nuevo, el del texto nuevo de `PostOriginKey`.
  
  En el siguiente «Actualizar», `ChooseOrigin` encuentra ese miembro por su texto. En el piloto, `PostOriginKey` = `OriginKey`: lo mismo que hoy;
- **lo demás, sin cambio.** Los nombres deseados salen de los sobres salientes (`DesiredNamesOf`, que ya lee el nombre del sobre), así que el renombrado no necesita código nuevo en POST.

**`ComparePost` genérico:**
- `PostRereadMissingReason`, `PostNotAdmittedPrefix`, `PostKeysDifferReason` y `PostEnvelopeDiffersPrefix`, **como hoy**;
- **antes de** las claves: una clave de `ErasedKeys` que sigue entre las mutables → `PostErasedStillPresentPrefix + clave`;
- la escala, la del modelo: piloto `InitialState!.Inputs.Annotations.AnnotationScale`; completo `InitialState!.Editable.AnnotationScale`.

Con `ErasedKeys` vacía, el piloto da lo mismo que hoy.

**La interfaz nueva, `Pilot/IPushBackPilotViewEraser.cs`:**

```csharp
public interface IPushBackPilotViewEraser
{
    PushBackPilotUnitResult EraseUnit(string memberKey);
}
```

Va en un archivo propio, para que UP no cambie (R-6). Ni ese archivo ni ningún otro de `Pilot/` nombra la palabra `Transaction`, tampoco en comentarios (UpdateGuard:117-124; IntegrationGuard:81).

### D-PBT3-11. El borrado en el dibujo (PB-T3c; OB-04.1, R-PB-03)

**La fachada caller-owned**, `src/RackCad.Plugin/Views/RackViewBlockEraser.cs`:

```csharp
internal static class RackViewBlockEraser
{
    internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId);
}
```

**El cuerpo,** en este orden: el de `EraseViewBlocks` (RCS:233-266) para **una** definición, **sin** bloqueo, transacción propia, confirmación ni purga:
1. `transaction.GetObject(definitionId, OpenMode.ForRead) as BlockTableRecord`; nula o `IsLayout` → `InvalidOperationException` (el clásico la salta en silencio, RCS:240-243; el panel aborta: todo o nada);
2. las definiciones anidadas: el `BlockTableRecord` de cada `BlockReference` hija;
3. cada referencia de `GetBlockReferenceIds(directOnly: true, forceValidity: true)`, abierta `ForWrite`, se borra (también las anidadas en otras definiciones: BM:54-60);
4. `UpgradeOpen()` y `Erase()` de la definición;
5. devuelve las anidadas, para la purga de POST.

**El archivo no contiene la cadena `PushBack`,** para no entrar en el censo de CT-PBT1B-15 (14 archivos del Plugin con `PushBack`).

**El escritor (WR):**
- `RackPilotUpdateScope.PrepareUnits` también resuelve cada clave de `plan.Erasures` en `_definitions`, en un diccionario privado nuevo `_erasures` (clave → `ObjectId`). Una clave que falta es `Failed("La definición " + clave + " ya no está en el dibujo.")`, el texto de hoy. No prepara ningún redibujo para ellas.
- `BeginMutation` pasa `_erasures` al ámbito de MUTATE.
- **`RackPilotMutationScope : IPushBackPilotMutationScope, IPushBackPilotViewEraser`.** `EraseUnit(memberKey)` hace:
  1. `EnsureOwned()`;
  2. la clave en `_erasures`, o `InvalidOperationException`;
  3. `_units++; FailUnitIfRequested(_units);`, la inyección de Debug de las unidades, sin un tercer `[Conditional("DEBUG")]`;
  4. `RackViewBlockEraser.EraseInTransaction(_transaction, id)`, con las anidadas añadidas a `_staleDefinitions`;
  5. `new PushBackPilotUnitResult(Array.Empty<string>())`.
- `Reread` calcula `matches` sobre `_prepared` **y** `_erasures`.
- POST no cambia: la purga ya es la unión de `_staleDefinitions`; los nombres y el renombrado solo son de las claves preparadas.
- **Los dos comentarios que dicen lo contrario cambian** (V2-11 y V2-28):
  - el de cabecera de WR (:26, «It never erases a view.») describe el borrado caller-owned;
  - el del ámbito de MUTATE (:225, «It never purges, renames, regenerates or erases») pasa a «It never purges, renames or regenerates; it erases only the planned views, through the caller-owned facade».
  
  Son comentarios: ninguna guarda los lee (`Src.Code` los quita).
- **Ninguna otra declaración nueva** en WR ni en la fachada, ni ningún ayudante privado (INV-13): la lista cerrada, por nombre, está en PBT3C-C13 (§6.3; V3-02).

**Lo que sigue en verde por su letra** (R-6, R-7): las prohibiciones del escritor (`GetBlockReferenceIds`, `Erase(`, `EraseViewBlocks`, `ForWrite` y `UpgradeOpen` van en la fachada), la estructura (un `StartTransaction`, un `Commit` y una purga), G-1, G-2, G-7 y G-9 con sus cuentas, y la relectura caller-owned.

**La premisa «el escritor nunca borra» sí cambia,** y lleva su **registro declarado** (D-2, §8.2; V2-11). La sesión escribe en el commit de emisión de PB-T3c una entrada en `F4B-DRAFTS/superseding-characterization.md`, **sin `Skip`**, con:
- la autoridad: [OD] D-PB-04 y [P] OB-04.1, «una primitiva caller-owned y su guarda de fachada»;
- la guarda que sigue en verde y por qué: UpdateGuard:182-212, cuyo comentario :188 dice «Never: the lock, an erasure…»; comprueba que **el escritor** no borra por sí mismo, y eso sigue siendo cierto, porque borra la fachada;
- las guardas nuevas que fijan el borrado: CT-PBT3C-01 a 04;
- los dos comentarios de WR que cambian.

### D-PBT3-12. D2 respetada

| Condición de D2 | En PB-T3 |
|---|---|
| 1 y 2 (los dos tipos; nada más compartido) | `PushBackFullDraftStateComparer.Copy` delega en el copiador de PB-T1b. CT-PBT3A-05 vuelve a medir el conjunto de objetos compartidos con el recorrido de CT-PBT1B-07 |
| 3, 4, 6 y 7, para las operaciones nuevas | CT-PBT3A-11: materializar, componer, `KeepOn`, clasificar y la disposición no cambian las huellas profundas de las dos referencias de sus argumentos (la huella de CT-PBT1B-16) |
| **8** (A-2: el ajuste de `Name`) | El escritor del panel no ajusta el `Name` de un sistema que comparte un estado del borrador. SR construye el sistema de cada miembro **desde texto** y le pone el nombre de su sobre (SR:46-55): es un objeto nuevo. CT-PBT3B-12 lo prueba con huellas tras un plan `Ready` y una corrida `Applied`, y CT-PBT1B-15 sigue en verde (PB-T3 no añade escritores de `.Name =` en el Plugin) |
| **10** (A-2: las guardas) | G-L1 se amplía, con una clase propia y sus autopruebas, a la carpeta `Draft/` (CT-PBT3A-13), a PP, UR y la interfaz nueva (CT-PBT3B-13) y a WR y la fachada (CT-PBT3C-05). Ninguna quita archivos ni fichas |
| **El límite** | La parada PBT3x-C17 en los tres contratos (§6.0) |

### D-PBT3-13. AUTH-13, fijado con pruebas (F4B-E24)

- **CT-PBT3A-04 (lectura):** las **28 rutas anidadas** de AUTH-13 para Push Back (`I58F1Matrix.Nested("pushback")`, `:97-102`, copiadas; V2-23), sobre `I58F1Fixtures.Rich("pushback")`, más las tres capas de arriba. Los dos lectores (el del piloto y el completo) dan `NotAdmitted` con «No editable aquí: ilegible o de versión futura» y la pista de RACKEDITAR.
- **CT-PBT3B-10 (escritura; V2-08):** con un borrador cuya base es la de ese mismo escaneo, el `Prepare` completo da `NotAdmitted`, y la corrida, `NotAppliedAdmission`, sin ninguna llamada a `PrepareUnits`, `BeginMutation`, `WriteUnit` ni `EraseUnit` en el doble.
- **Lo que pasa en el uso real, declarado:** AUTH-13 actúa **al abrir**, y entonces no se crea el borrador (`TryBeginPilotEdit`, WHC:347-367). Si alguien cambia el dibujo después de abrir, añadiendo un campo desconocido, la relectura difiere de la base y lo que sale es **Conflict**, antes de la admisión (PP:322-323; UR:402-405). El `NotAdmitted` de la escritura solo se alcanza con una base que ya traía el campo: por eso CT-PBT3B-10 la fabrica.
- **Lo que no se hace:** abrir AUTH-13 (D-1 del Owner no lo pide) ni conservar lo anidado (opción C, no elegida). La pérdida de RACKEDITAR («Actualizar» y «Guardar en biblioteca») queda como limitación conocida anterior a F4B, sin tocar el clásico (D-16, INV-19).

### D-PBT3-14. Lo que PB-T3 no hace

- **Crear vistas** (H-PB-16; fila 1 del §6.4; OB-04.2).
- **Cambiar RACKEDITAR o el clásico:** RPC, RCS (`EraseViewBlocks` incluido), la ventana, WX, T1B (D-16, INV-19, OB-RE.1).
- **Tocar la ventana:** los modos, «Aplicar al borrador», BOM y biblioteca son de PB-T2 y PB-T4.
- **Generalizar el anfitrión o el puerto del panel:** WHC, `IWorkspacePilotEditPort`, el adaptador de campos y la UI del panel son de PB-T4.
- **Conectar el modelo completo al panel:** la admisión A/B (OB-03.2), «Conservar mi borrador» completo (OB-05.5), el clasificador presentado (OB-05.3, OB-05.4). Todo es de PB-T4.
- **Abrir AUTH-13 o conservar ExtensionData anidada.**
- **Cambiar `DormantTailOutOfPilotReason`** ni el trato de `pb-21-cola` (OB-03.1, D-PBT1-10).
- **Escribir un rack de un sentido que trae un frontal con `Section` 2** (D-PBT3-09, la limitación; V2-20).

---

## 4. Las guardas: T1B, I-55, F4-T4 y PB-T1

| Guarda | ¿Sigue en verde? | Cómo |
|---|---|---|
| **T1B** (55 UI y 10 Core, con T-SG y T-PG) | Sí | PB-T3 no toca la ventana, WX, RPC ni el Plugin clásico |
| **G-10** (`WorkspacePilotUpdateIntegrationGuardTests`, una sola `.RequestUpdate(` por archivo) | Sí | Ningún archivo de PB-T3 llama a `RequestUpdate` |
| **I-55** (`I55DeferredProductFixCharacterizationTests`, G15 y las de proyección) | Sí | No se tocan sus archivos |
| **I-54 G4B** (el censo de los que componen) | Sí | `ComposeMemberEnvelope(source, …)` sigue siendo el único del piloto; nada nuevo compone |
| **I-58** (la matriz de AUTH-13) | Sí | El lector no cambia |
| **F4-T4: G-1, G-2, G-6, G-7, G-9, el censo de `Task`, la pureza del runner y del adaptador** | Sí | PB-T3a no toca F4. PB-T3b cambia solo PP y UR (sin términos de escritura) y añade la interfaz. PB-T3c llama a una fachada que no nombra el piloto ni ningún término de G-9 (R-6) |
| **F4-T4: pureza de UP, UR y SR, y `Pilot/` sin `Transaction`** | Sí, con la regla de §6.0 | Ningún texto nuevo de `Pilot/` nombra esas palabras, tampoco en comentarios |
| **F4-T4: `From(DraftSession<PushBackPilotDraftState>)` y una sola construcción** (PrepareGuard:81-98) | Sí | Las dos fábricas son de tipos distintos; la del piloto no cambia |
| **F4-T4: `ComposeForWrite` solo en el plan, tras la comparación, una vez** (PrepareGuard:154-190) | Sí | La llamada sigue en el `Build` del piloto |
| **F4-T4: el orden de `Prepare`** (PrepareGuard:194-227) | Sí | El `Prepare` del piloto no cambia y va primero en el archivo |
| **F4-T4: el escritor** (UpdateGuard:182-470) | Sí, por su letra | Ver D-PBT3-11. La premisa cambia, y lleva su registro declarado sin `Skip` (D-2; V2-11) |
| **F4-T4: S-1 a S-6** (R-6) | **No** | **D-1 (§8.2), decidida: (A) (F4B-E29).** S-1 a S-5 fallan, S-6 queda vacía y tres filas de S-2 también; se sustituyen las seis con registro, cada una con su equivalente, en PB-T3b. (Con (A′), no elegida, habrían sido S-1, S-2 y S-4.) La lista se vuelve a medir **sobre el texto del GREEN** (INV-PBT3B-15; V2-02) |
| **El comportamiento del piloto en F4** (`WorkspacePilotPrepareTests` 67, `WorkspacePilotUpdateRunTests` 49, `WorkspaceDraftPilotMaterializationTests` 41, `WorkspacePilotUpdateHostTests` 35, `WorkspacePilotUpdateRequestTests` 44, `WorkspacePilotReconciliationHostTests` 28, `WorkspaceDraftPilotIntentReadTests` 63, `WorkspaceDraftPilotStateTests` 27) | Sí, salvo un diseño bloqueado | El piloto conserva sus textos, su orden, sus llamadas al puerto, su `ComparePost` y su origen de POST. La barrera cambia el resultado **solo** sobre un diseño bloqueado (V2-09). El corpus de PB-T1a no lo está (R-8); los diseños de estas clases se censan en la remedición, y PBT3B-C14 para si una de ellas cae |
| **PB-T1:** CT-PBT1B-14 y 15 (D2), CT-PBT1C-03 y 04, `PushBackStateNet`, la paridad CT-PBT1B-01, el censo del piloto (HT1-21) | Sí | PB-T3 no cambia PES, PESC, PESO, GATE, ES, CS, RMES ni la ventana. No añade campos a los tipos del censo. El archivo nuevo del Plugin no contiene `PushBack` |
| **PB-T2** (V42-06): CT-PBT2A-04 (D2 del puerto) y CT-PBT2B-08, 09 y 10 (las guardas de la ventana en dos modos), con las demás clases de PB-T2 | Sí | PB-T3 no toca la ventana, `PushBackEditorMode.cs` ni los dos archivos de PB-T2a (`PushBackDraftStatePort.cs` y `PushBackDraftEditorLaunch.cs`): están en su `ForbiddenWriteScope`. Ningún nombre ni archivo de PB-T3 coincide con los de PB-T2 |
| `NamespaceFolderGuardTests`, el censo de ventanas, RAM Elements, el corpus de WLC | Sí | Un `namespace` por archivo igual a su carpeta; ninguna ventana; ningún `pb-*.json` nuevo en `Fixtures/I64` |

**Si cualquier otra prueba existente falla** en el RED o en el GREEN, es la parada C02 o C16 y vuelve al Owner (OB-T1B.3). No hay más sustituciones que las de D-1.

---

## 5. La red de pruebas

### 5.0 Reglas y recetas comunes

**Las reglas** (las de PB-T1, copiadas en cada contrato; no se heredan por analogía, [P] §7.2):
- `[Fact]` que acaban en **un** `Assert.Equal` de las líneas unidas por `\n`, con «R», cultura invariante, GUID fijo y orden ordinal;
- la regla «EXCEPCION»: cada llamada al API nuevo pasa por un ayudante que convierte una excepción en `EXCEPCION <Tipo>: <Mensaje>`; las líneas esperadas no contienen ninguna;
- las pruebas de UI, en `EditorDiscardPromptCollection`, con `Ventana()`, la carga dentro del `try` y `Cerrar` en `finally`, el aviso de descarte sustituido y sin teclado ni foco real;
- ningún `MessageBox` real, BOM, «Guardar en biblioteca», configurador real ni «Insertar»;
- ningún fixture nuevo en `Fixtures/I64`: los diseños y sobres nuevos se fabrican en código;
- **los ayudantes de prueba existentes se usan sin editarlos:** `IntentKit` e `IntentItem` (`WorkspaceDraftPilotIntentStaleBaseTests.cs:15-21`), `PilotKit` (con `JsonDiff`), `DraftKit` y `PilotUpdateScopeDouble`. Lo que haga falta de más va en un ayudante nuevo de la propia clase o en un archivo nuevo de su subtarea.

**Las recetas:**
- **Los sobres del corpus:** los 17 de `Fixtures/I64`, leídos sin añadir nada, editados con `EditInner` (copiado de S0, `:108-118`).
- **X-01 y F-01 (Core):** las de [PKT1] §5.0, con `PushBackEditableStateSeat.Pass` en lugar de `Pasada`. Para leerlas como sobre, la prueba toma un sobre del corpus deserializado con `RackEmbedStore`, sustituye su `Design` por el proyecto serializado del diseño y lo vuelve a serializar. El Plugin (`BuildPushBackPayload`) no es accesible desde Core. No es un `new RackEmbedDocument` ni una llamada a `RackEmbedComposer`, y las guardas de I-54 miran `src/`, no `tests/`.
- **Los borradores:** `DraftRegistry.Begin` con `PushBackFullDraftStateComparer.Instance` y `PushBackFullDraftStateComparer.Copy`, como el anfitrión hace con el piloto (WHC:347-352), y una lectura de `PushBackFullDraftReader.Read`.
- **El escaneo:** `IntentKit`, como las pruebas del lector del piloto.
- **El doble con borrado** (PB-T3b): una clase nueva, `PilotEraseScopeDouble`, en **`tests/RackCad.Tests/PushBackFullWriteSupport.cs`**, fuera de `Workspace/` (V2-06). Es una copia de `PilotUpdateScopeDouble` (no se edita la original):
  - su ámbito de MUTATE implementa `IPushBackPilotViewEraser`;
  - registra `EraseUnit:<clave>` y lo aplica en el sellado, como las escrituras;
  - su relectura de MUTATE da `PreparedBlockMatches` para **todas** las claves del plan, las de `Members` y las de `Erasures`, igual que hará el escritor de PB-T3c (V2-15);
  - el nombre de la clase y del archivo no empiezan por `Workspace` (§6.2, filtros).

### 5.1 Lo que sigue en verde sin tocarlo (las tres subtareas)

T1B (55 y 10); PB-T0 (16 y 3); PB-TC y PB-TC2 (14 en UI); PB-T1a (14); PB-T1b y PB-T1c; **PB-T2a, PB-T2b y PB-T2c** (sus ocho clases de prueba, V42-06); `RackCad.Tests.Workspace` entero (3 271 en Core y 547 en UI, salvo las sustituciones de D-1 en PB-T3b); I-58; I-54 G4B; `NamespaceFolderGuardTests`; Cantilever (67). En PB-T3b y PB-T3c, también las subtareas anteriores de PB-T3.

### 5.2 PB-T3a: el modelo (lista cerrada CT-PBT3A-01 a 14)

**Régimen** (el de PB-T1b): **el RED** es el esqueleto inerte de §6.1 y las pruebas nuevas, que fallan **por aserción**, salvo las tres guardas (CT-12 a 14), que pasan en el RED y discriminan por sus autopruebas. **El GREEN** solo toca `src/`, y solo los archivos nuevos.

**Las clases:**
- en `tests/RackCad.Tests/` (`namespace RackCad.Tests`): `PushBackFullDraftLoadTests` (02 a 05), `PushBackFullDraftCompositionTests` (06, 07 y 11), `PushBackFullDraftViewTests` (08), `PushBackFullDraftClassifierTests` (09 y 10) y `PushBackDraftWriteGuardTests` (12 a 14);
- en `tests/RackCad.UI.Tests/`: `PushBackFullDraftWindowParityTests` (01).

| CT | Qué fija | Oráculo | Falla en el RED porque |
|---|---|---|---|
| **01** (UI) | **El asiento iguala a la ventana.** Para cada sobre del corpus salvo `pb-21-cola` (16): `Materialize` admite, y su `Editable` es igual, con el comparador de PB-T1b, al `EditableStateForTest` de una ventana tras `LoadExisting(<diseño>, <id>, <nombre>, <proyecto>)`; y el diseño persistido de `Pass` sobre una copia es igual a `Persist(LastComputation.Design)`. La línea `<diseño> estado-igual=<b> diseno-igual=<b>`. Para `pb-21-cola`: `pb-21-cola OutOfPilot motivo=<DormantTailOutOfPilotReason>` | La ventana de hoy, a través de la costura de PB-T1c | el esqueleto no materializa |
| **02** | **El materializador:** en blanco, `null`, otro tipo, sin diseño, ilegible y catálogo nulo, cada uno con su `Failed` y su texto; `pb-21-cola` `OutOfPilot`; `pb-20-ab` `Admitted` (la diferencia con el piloto: `PushBackPilotMaterializer.Materialize` da `OutOfPilot(CompositeOutOfPilotReason)`); un diseño que hace lanzar al resolver (c-13), `Failed` con el prefijo. Una línea por caso: `<caso> completo=<Kind>[ motivo=<texto>] piloto=<Kind>[ motivo=<texto>]` | El materializador del piloto y sus constantes | el esqueleto da `Failed` siempre |
| **03** | **El lector completo es el del piloto salvo el compuesto.** Para un escaneo por regla de admisión (las 14 de IR, construidos con `IntentKit` como en `WorkspaceDraftPilotIntentReadTests`) y para `pb-20-ab`: el diagnóstico, el detalle, la pista, la clave del origen y la base (entrada a entrada) del lector completo frente a los del piloto. La línea `<caso> iguales=<b>` y, en el compuesto, `pb-20-ab piloto=NotAdmitted completo=Admitted base-igual=True` | `PushBackPilotIntentReader.Read` sobre el mismo escaneo | el esqueleto no admite |
| **04** | **AUTH-13** (D-PBT3-13; F4B-E24): las 28 rutas anidadas de I-58 y las tres capas de arriba, en los dos lectores (la forma, abajo) | Las constantes del lector; la matriz de I-58 | el esqueleto da otro diagnóstico en el lector completo |
| **05** | **El copiador y el comparador:** `Copy(x)` igual a x y distinto objeto; cambiar `Origin` o una ranura de `Editable` (el `Mutator` del piloto, copiado) da distintos; dos materializaciones del mismo texto, iguales; el conjunto de objetos compartidos entre `Copy(x)` y x son los no nulos de las dos referencias de D2 y lo que alcanzan (el recorrido ampliado de CT-PBT1B-07, copiado) | D-PBT3-02; PB-T1b | `Equals` es `ReferenceEquals` |
| **06** | **`ForWrite`:** escala `NaN`, `+∞`, 0 y −1 → `InvalidScaleReason`; un estado inicial cambiado tras materializarse (`PalletTolerance` + 1) → `BaseChangedReason`; un `Origin` ilegible → su motivo; con F-01 editado (una longitud de módulo, `SetGap(30)` en `pb-20-ab`, el nombre «PB-NUEVO»): `Composed`, con el diseño de `BuildDesign(Editor, Inputs)` igual al de `Pass` sobre una copia de `current`, `Name` igual al del borrador, y `current` sin cambios (comparador y huella) | D-PBT3-05; PB-T1b | el esqueleto rechaza siempre |
| **07** | **`KeepOn`:** el resultado tiene el `Editable` de `current` (comparador) y el `Origin` de `newInitial`; ningún argumento cambia; el resultado no comparte nada mutable con ellos fuera de las referencias de D2; `DraftSession.KeepMineOnCurrentVersion(read, PushBackFullComposition.KeepOn, confirmed: true)` sobre un borrador en Dirty (el estado actual cambiado con `SetCurrentDraftState`) da `Reloaded`, `IsDirty` cierto y la base nueva | D-PBT3-05; DS:469-486 | el esqueleto devuelve `current` |
| **08** | **La disposición:** los casos de CT-PB0-10 (`PushBackFullEditorCharacterizationLateralCutTests`), con un sobre lateral por cada corte de ANTES (su sección, el `PostIndex`) y el sistema de DESPUÉS: para cada momento, `ObsoleteLaterals` = los `PostIndex` que esa prueba da por obsoletos, como claves; `pb-20-ab` con B quitado y un frontal B → `FrontalBOfOneWay`; un sobre `lateral` con sección −1 → `NoAddress`; un rack cuyo único miembro es un lateral obsoleto → `AllObsolete=True`; planta y frontal de A siempre `Redraw` | Las líneas de CT-PB0-10 y RVA:94-116 | el esqueleto da todo `Redraw` |
| **09** | **El clasificador, por campo** (V2-12). Con base B0 = `pb-01-base`, los casos: un cambio externo de `PalletTolerance` (c); un cambio del usuario de `BeamDepth` (d); el mismo campo `PostPeralte` cambiado por los dos con el mismo valor y con valores distintos; el nombre cambiado fuera; y una `CustomProperties` cambiada fuera. **Dos partes:** (1) **el oráculo mecánico** (abajo): la línea `<caso> igual-al-oraculo=<b> entradas=<n>`; (2) **las entradas ancla**, literales, que no dependen de las hojas derivadas: `<caso> ancla Payload $.Structure.PalletTolerance grupo=Rack clase=ChangedExternally estructural=False`, `… $.Structure.BeamDepth grupo=Rack clase=ChangedByUser …`, `… $.Structure.PostPeralte grupo=Rack clase=Overlapping …` (en los dos casos de `PostPeralte`), `… Envelope $.Name grupo=Name clase=ChangedExternally …` y `… Envelope $.CustomProperties… grupo=OutsideEditor clase=ChangedExternally …` (la primera hoja de `CustomProperties`, por ruta ordinal). Cada ancla da `presente=True`.<br><br>**(3) La cobertura del mapa** (V42-03; A2-12 de PB-T2, M09). Por reflexión, en `RackEmbedDocument` (nivel `Envelope`), `RackProjectDocument` (`Project`) y `PushBackDesignDocument` (`Payload`): cada propiedad pública de instancia con getter, salvo la de `[JsonExtensionData]`. Su nombre JSON es el de la propiedad, porque los dos almacenes no tienen política de nombres (`RackEmbedDocument.cs:81-85`; `RackProjectStore.cs:404-410`). Por propiedad, la línea `<nivel> $.<Propiedad> fila=<la ruta de la fila de PushBackEditableStateOwnership.Table que la cubre, o <ninguna>> dueno=<Of(nivel, ruta)>`. **Lo esperado**, literal, por nivel y, dentro de cada nivel, en orden ordinal del nombre: 27 líneas (8, 9 y 10), **ninguna con `fila=<ninguna>`**:<br>• `Envelope`: `$.SchemaVersion`, `$.Kind`, `$.View`, `$.Section`, `$.Id` y `$.CustomProperties` con `dueno=Envelope`; `$.Name` con `Editor`; `$.Design` con `Container`;<br>• `Project`: `$.SchemaVersion`, `$.Kind`, `$.Header`, `$.DynamicSystem`, `$.SelectiveRack`, `$.FlowBed`, `$.Larguero` y `$.Cantilever` con `Envelope`; `$.PushBack` con `Container`;<br>• `Payload`: `$.SchemaVersion` con `Envelope`; `$.Structure`, `$.Fronts`, `$.LegacyHighEndBeamPeralte`, `$.RearTopeSaque`, `$.RearTopePieceId`, `$.DefensePieceId`, `$.RearTopeOffCells`, `$.SideB` y `$.Composite` con `Editor`.<br><br>Así, una propiedad nueva sin fila no cae en la rama de ExtensionData de `Of`, que la daría por `Envelope` (M09): la línea saldría con `fila=<ninguna>` | El oráculo mecánico y D-PBT3-07. (3): `PushBackEditableStateOwnership` (PB-T1b), leído en `5ddeb3b4` | el esqueleto no da entradas |
| **10** | **El clasificador, estructura y vistas** (V2-12). Los casos: «+ frente» del usuario; una longitud de módulo cambiada fuera; el lado B declarado con un frente por el usuario; y «- frente» con un lateral del último poste en `currentMembers`. (1) **El oráculo mecánico**, como CT-09; (2) **las anclas**: en «+ frente», hay al menos una entrada `Fronts` estructural `ChangedByUser` con `<ausente>` en b; en el lado B, al menos una `SideB` estructural `ChangedByUser`; en el módulo, al menos una `Modules` estructural `ChangedExternally`; en «- frente», `ViewsToErase` = la clave del lateral, `ViewsRejected` vacía y `AllViewsObsolete=False` | El oráculo mecánico, D-PBT3-07 y CT-08 | ídem |
| **11** | **D2, condiciones 3, 4, 6 y 7 para las operaciones nuevas:** sobre el estado rico de CT-PBT1B-06 y X-01, la huella profunda (la de CT-PBT1B-16, copiada) de las dos referencias de cada argumento antes y después de `Copy`, `ForWrite`, `KeepOn`, `Classify` y `Of`, y de `Equals` en los dos sentidos. La línea `<operacion> huellas-iguales=True` | D-PBT1-15; CT-PBT1B-16 | el esqueleto no carga los estados |
| **12** | **Sin conectar** (OB-05.2, OB-05.5; ExpectRed false): ningún archivo de `src/` fuera de `Systems/PushBack/Draft/` y de los dos que PB-T3b generaliza (PP y UR, admitidos desde ahora para que PB-T3b no rompa esta guarda) nombra `PushBackFullDraftState`, `PushBackFullDraftReader`, `PushBackFullComposition`, `PushBackConflictClassifier` ni `PushBackViewDispositions`. Autopruebas: inyectar cada nombre en WHC y en `WorkspacePilotSupport.cs` salta | — | **no falla en el RED** |
| **13** | **G-L1 ampliada a `Draft/`** (D2, condición 10; ExpectRed false): las fichas y las reglas de CT-PBT1B-14 (con la regla 1b para los alias, la corrección I01 de PB-T1b), sobre los archivos de `Draft/`, con `reglas-sin-coincidencias=True` y las autopruebas (a), (b), (c), (e), (f1), (h1), (i) y (j) inyectadas en `PushBackFullComposition.cs` | D-PBT1-15 | **no falla en el RED** |
| **14** | **La pureza de `Draft/`** (ExpectRed false): ningún archivo lleva directiva `#nullable` (como PES, PESC, PESO y GATE); un `namespace` de bloque `RackCad.Application.Systems.PushBack.Draft`; sin `Autodesk`, `RackCad.Plugin`, `RackCad.UI`, `System.Windows`, `Transaction`, `Database`, `async`, `await`, `Task`, `File.`, `System.IO` ni `RackEmbedComposer`, y sin `new RackEmbedDocument`. Autopruebas: cada término inyectado salta | D-PBT3-08 | **no falla en el RED** |

**CT-01 y F4B-E30** (V41-05). F4B-E30 mide que, tras ciertos gestos de la parte compuesta de la ventana, `LastComputation.Design` va una pasada por detrás. CT-01 no hace gestos: compara tras `LoadExisting`, que termina con una pasada (el `Recompute();` final de `LoadFromModel`: W:440 en `d8ccbcb8` y :366 en `5ddeb3b4`; a medir tras PB-T2b, V42-10). No le afecta, y su texto no cambia.

**CT-01 y PB-T2** (V42-01): CT-01 abre la ventana en CLASSIC con `LoadExisting`. La guarda de modo de PB-T2b no hace nada en CLASSIC, `LoadExisting` sigue terminando en `LoadFromModel`, y la costura `EditableStateForTest` se conserva (INV-PBT2B-04 y 06 de PB-T2). Su texto no cambia.

**La forma de CT-04, literal** (V2-23):
- **El rack:** el sobre de `pb-01-base`, con su `Design` sustituido por `I58F1Fixtures.Rich("pushback")`, que trae todas las piezas anidadas. Cada caso inserta `"FutureNested": 1` con `I58F1Fixtures.Set(<diseño>, <ruta> + ".FutureNested", "1")`. Las rutas son las 28 de `I58F1Matrix.Nested("pushback")` (`:97-102`), **copiadas como lista literal en la prueba**, porque `Nested` es privado. Las tres capas de arriba van con `Set(<diseño>, "Future", "1")`, `Set(<diseño>, "PushBack.Future", "1")` y el campo en el sobre, como CT58-15.
- **Una línea por caso,** en este orden, primero las 28 anidadas y después las tres de arriba:
```
<ruta> piloto=[No editable aquí: ilegible o de versión futura] completo=[No editable aquí: ilegible o de versión futura] pista=[Usa RACKEDITAR para editarlo con el editor clásico.]
```
- **Lo esperado:** las 31 líneas, con ese diagnóstico y esa pista literales.
- **Se comprueba a sí misma:** antes de insertar nada, el mismo rack sin el campo da la línea `control completo=Admitted piloto=[<Admitted o el diagnóstico>]`. `Rich("pushback")` trae el lado B, así que el piloto debería dar «No editable aquí: fuera del piloto (F5)» **después** de AUTH-13; la prueba admite esa línea o `Admitted`. Si el lector completo no admite el control, la receta no ejerce AUTH-13, y es PBT3A-C07.

**El oráculo mecánico de CT-09 y CT-10** (V2-12; I-11 de la revisión):
- la prueba calcula `PilotKit.JsonDiff(b, c)` y `PilotKit.JsonDiff(b, d)`, de las pruebas y sin editarlo, sobre los mismos tres textos que compara el clasificador: el `$.PushBack` persistido de cada estado, el sobre y el proyecto de su `Origin`, y el nombre;
- aplica las reglas de clase, grupo y estructura de D-PBT3-07, escritas otra vez en la prueba a partir de la tabla del paquete;
- compara con `Classify` **el conjunto de (nivel, ruta, grupo, clase, estructural)**, sin el texto de los valores, porque los dos diffs pueden escribirlos distinto.

No es tautológico: el diff y las reglas de la prueba son independientes de los del producto. Las hojas derivadas salen en los dos lados, así que no hace falta preverlas.

**Por qué A2-12 (M09) se cierra con la prueba por reflexión, más la fila por defecto, y no con la equivalencia declarada** (V42-03):
- **M09 es un riesgo de futuro:** `Of` decide «declarada» por la tabla y no por el tipo. Hoy las 27 propiedades tienen fila; el riesgo es una propiedad que se añada después, que `Of` daría por ExtensionData (`Envelope`). Una equivalencia declarada solo describe el hoy; la prueba avisa del cambio.
- **Usa solo API pública que ya existe** (`PushBackEditableStateOwnership.Table` y `Of`, de PB-T1b) y no añade ningún nombre a la API cerrada de PB-T3a (PBT3A-C13). Va como parte (3) de CT-09, la clase del clasificador, así que no cambia ningún recuento ni el GREEN previsto.
- **La mitad del clasificador no se prueba por reflexión:** haría falta exponer su función de grupos, un miembro nuevo de la API cerrada, que es diseño. Por eso se resuelve con la fila por defecto de D-PBT3-07, que hace total la tabla y decide por el mismo mapa que prueba (3).

### 5.3 PB-T3b: la escritura (lista cerrada CT-PBT3B-01 a 15)

**La variante es (A)** (F4B-E29, `d8ccbcb8`; V41-03). Lo que esta sección dice «con (A′)» es registro de la variante no elegida y no va al contrato.

**Régimen:**
- **el RED** tiene tres partes:
  - el esqueleto inerte del API nuevo **en los archivos existentes** (§6.2);
  - la interfaz nueva;
  - las pruebas nuevas y el `Skip` de los métodos de D-1, con su motivo literal: los seis de (A), S-1 a S-6 (F4B-E29).
  
  **El piloto no cambia en el RED.** Las pruebas nuevas fallan por aserción, salvo CT-13 (guardas), que pasa en el RED.
- **el GREEN** solo toca PP y UR.

**Las clases:**
- en `tests/RackCad.Tests/`: `PushBackFullWritePrepareTests` (01 a 06, 10, 11 y 15), `PushBackFullWriteRunTests` (07 a 09 y 12), `PushBackWritePathGuardTests` (13) y `PushBackPilotUpdateEquivalenceGuardTests` (14: seis `[Fact]`, E-1 a E-6);
- el doble con borrado, en `tests/RackCad.Tests/PushBackFullWriteSupport.cs` (V2-06).

| CT | Qué fija | Oráculo | Falla en el RED porque |
|---|---|---|---|
| **01** | **(i) Campos:** un borrador completo de `pb-01-base` con `PalletTolerance`, `BeamDepth` y el nombre cambiados → `Ready`. Por miembro: el sobre saliente tiene el `Id`, `Kind`, `View`, `Section`, `CustomProperties` y cajón del sobre **releído**, el nombre del borrador y el diseño de la composición. El proyecto y la carga útil, con la versión y el cajón del proyecto releído; `ExpectedBase` es la base del borrador; `Erasures` vacía; `PostOriginKey` = `OriginKey`; dos `Prepare` iguales dan planes iguales.<br><br>**(ii) El paso 4 de D-PB-06 hasta el sobre que sale** (V42-02; A2-11 de PB-T2; OB-06.3):<br>• **Las once filas F** de `SC1.FilasF()` (`tests/RackCad.UI.Tests/PushBackDeclaredCorrectionSupport.cs:49-62` en `5ddeb3b4`), **copiadas como lista literal**, con su exacto = la base + 0,00044: SC1 está en `RackCad.UI.Tests`, y Core no la ve (§5.0). Por fila: el sobre de `pb-01-base` con el campo de la fila puesto a su exacto (como `VarianteDe`: `EditInner` y el `Poner` de SC1, copiado); un borrador completo de ese sobre con **solo el nombre** cambiado («PB-EXACTO»), para que esté en Dirty sin tocar el campo; `Prepare` → `Ready`. El valor se lee del **diseño del sobre que sale** de cada miembro (`new RackEmbedStore().Deserialize(member.EnvelopeText).Design`), con el `Num` de SC1, copiado, en la ruta de la fila. La línea: `<Fn> prepare=Ready diseno=[<valores distintos de los miembros, en ordinal>]`. Lo esperado, el de CT-PBTC-02 (i) y CT-PBT2C-01: el exacto en diez filas (F1 48.00044, F2 3.00044, F3 4.00044, F5 1.50044, F6 4.00044, F7 42.00044, F8 60.00044, F9 1000.00044, F10 6.00044 y F11 100.00044) y `6` en F4.<br>• **Las dos filas de estilo:** `pb-06-seguridad` y `pb-07-combinado`, como borradores completos con solo el nombre cambiado → `<diseño> prepare=Ready diseno=<estilo> sistema=<estilo>`. `diseno` es el `Structure.DimensionStyle` del diseño del sobre que sale; `sistema`, el de `PushBackPilotSystemRebuild.BuildForMember(member, catalog)`, que es el sistema que dibuja el escritor en MUTATE. Lo esperado: STANDARD-ISO y SIMPLEX, como CT-PBTC-05 | PP:421-438; R-5; D-PBT3-05. (ii): CT-PBTC-02 (i), CT-PBTC-05 y CT-PBT2C-01 | el esqueleto da `Invalid` |
| **02** | **Frentes y el origen borrado** (V2-01): (i) «+ frente» → todos los miembros redibujados, ninguna clave nueva (las claves del plan ⊆ las releídas: **no se crean vistas**); (ii) «- frente» con el lateral del último poste, abierto desde la planta → `Erasures` = esa clave, `PostOriginKey` = `OriginKey`; (iii) **lo mismo, abierto desde ese lateral** → `OriginKey` = el lateral, `Erasures` = el lateral y `PostOriginKey` = la primera clave `Redraw` en ordinal; (iv) un rack con ese lateral como único miembro → `Invalid(AllViewsObsoleteReason)` | CT-PB0-10; RPC:198, :320-356; M-2b | ídem |
| **03** | **Módulos:** F-01 con una longitud de módulo de A y otra de B cambiadas → `Ready`; el diseño saliente lleva los dos módulos; ninguna vista a borrar | PB-T1b | ídem |
| **04** | **Lado B:** (i) `pb-01-base` con B declarado y un frente activo → el diseño compuesto; el frontal de A se redibuja y su nombre deseado lleva el lado; (ii) `pb-20-ab` con B quitado y un frontal B entre los miembros → `Invalid(FrontalBOfOneWayPrefix + clave + FrontalBOfOneWaySuffix)`; (iii) `pb-20-ab` con B quitado y un lateral que solo existía por una ranura de B → `Erasures` = esa clave (H-PB-17 corregido); (iv) en el diseño saliente de (iii), **`Composite.AbsentSlotsB` contiene todas las ranuras de B, de 0 a N−1 (N = el número de frentes del diseño), y `SideB` es nulo**. Es el hecho medido de M-i (BM:62-66: las 72 ranuras con `SideB` nulo en un rack de 72 frentes), generalizado (V2-13) | D-PBT3-06; BM:62-66 | ídem |
| **05** | **Renombrar:** el nombre «PB-NUEVO» en los sobres de todos los miembros; `DesiredNamesOf` da los nombres del clásico (`RackViewBaseName.LinkedPlanta`, `LinkedLateral` y `LinkedPushBackCut` con el compuesto); un nombre en blanco → el del sobre releído del origen, en todos. **El piloto** con miembros de nombres distintos conserva el de cada uno (como hoy) | RPC:186-190, :278-345 | ídem |
| **06** | **La barrera** (V2-09): (i) los cuatro bloqueantes de CT-PBT1B-12 (hueco negativo, separador sin hueco, estructura manual de 1 y celda que no cabe), como borrador **completo** → `Invalid(BlockedPrefix + <el texto de StatusOr>)`, literal; (ii) **un borrador del piloto** sobre `pb-01-base` con `Composite.StructureOverrideA = 1` (`EditInner`), un diseño de un sentido, con la escala cambiada a 2,0 → la línea `piloto-bloqueado admitido=True bloqueado=True prepare=Invalid motivo=[El diseño tiene un error que impide escribirlo: Lado A: la estructura manual (1) no es construible; el minimo fisico es 2 fondos. Corrige el valor o pulsa «Restaurar estructura» para volver a la propuesta.]`. `admitido` lo da `PushBackPilotIntentReader.Read`, y `bloqueado`, `PushBackEditorOutputGate.IsBlocked` sobre la composición del piloto. **Se comprueba a sí misma:** si `admitido` o `bloqueado` no son `True`, la receta no ejerce la barrera del piloto, y es PBT3B-C07. (`Blocking()` de `PushBackOutputGateTests` es compuesto, y el piloto no lo admite: R-14.) (iii) Los diseños del corpus que el piloto admite (los 17 de `Fixtures/I64` menos `pb-20-ab` y `pb-21-cola`, comprobado con `PushBackPilotMaterializer.Materialize` en la propia línea), como borrador del piloto con la escala a 2,0 → `Ready` | CT-PBT1B-12; R-8; R-14 | ídem, y el piloto no tiene barrera en el RED: (ii) da `prepare=Ready` |
| **07** | **Una corrida aplicada con borrado:** con el doble con borrado: (i) la secuencia exacta de llamadas `Begin, ReadForPrepare, PrepareUnits, BeginMutation, Reread, WriteUnit:<k>…, EraseUnit:<k>…, Seal, Dispose:mutation, Post, Dispose:scope`; **un** `BeginMutation`, **un** `Seal` y los `EraseUnit` entre los dos; el resultado `Applied`; la base nueva del borrador sin la clave borrada; (ii) **el origen borrado** (el caso (iii) de CT-02; V2-01): el resultado es `Applied`, **no** `AppliedPostRereadFailed`; la lectura de POST se ancla en `PostOriginKey`; la base nueva del borrador no tiene la clave borrada, y el texto de su estado inicial es el del sobre nuevo de `PostOriginKey`. **Después, el estado actual se cambia** (la escala a 3,0 con `SetCurrentDraftState`; V3-06), y un segundo `Prepare` sobre esa base da `Ready` con `OriginKey` = esa clave. Sin el cambio, el borrador está limpio y daría `NothingToApply` | D-PBT3-09 (h); D-PBT3-10; UR:250-289, :481 | `Execute` completo da `NotAppliedFailure` |
| **08** | **Los fallos de MUTATE con borrado:** un ámbito sin `IPushBackPilotViewEraser` (el doble original, `PilotUpdateScopeDouble`) → `NotAppliedPreflight(EraseUnsupportedReason)` sin `Reread` ni ninguna `WriteUnit`; `EraseUnit` que lanza antes del sellado → `NotAppliedFailure` con `WriteFailurePrefix` y el aborto (sin `Seal`); la vista a borrar en una capa bloqueada → `LOCKED_LAYER: <clave>`; la vista a borrar cambiada entre PREPARE y MUTATE → `NotAppliedConflict` | UR:391-456 | ídem |
| **09** | **`ComparePost` genérico:** la clave borrada sigue → `POST_ERASED_STILL_PRESENT: <clave>`; falta una redibujada → `POST_KEYS_DIFFER…`; un sobre distinto → `POST_ENVELOPE_DIFFERS: <clave>`; la escala del modelo completo distinta → `POST_SCALE_DIFFERS…`; todo igual → `null`. **El piloto**, con un plan sin borrados, da lo mismo que hoy en los mismos casos | UR:163-183 | la sobrecarga completa da `PostRereadMissingReason` |
| **10** | **AUTH-13 en la escritura** (D-PBT3-13; V2-08). **La receta:** (1) el escaneo con un campo desconocido en `PushBack.Structure.Modules[0]` (y otro en `PushBack.SideB`), como CT-PBT3A-04; (2) un borrador iniciado con una lectura admitida **fabricada**: `DraftIntentRead<PushBackFullDraftState>.Admitted(PushBackPilotIntentReader.BuildStaleBase(<ese escaneo>), <el estado de Materialize sobre el texto de su origen>)`, por `DraftRegistry.Begin`; (3) el estado actual cambiado (escala 2,0); (4) el `Prepare` completo con la relectura `PushBackFullDraftReader.Read(<ese escaneo>)`, y la corrida con un doble que dibuja ese escaneo. **Las líneas, literales:** `modulo-anidado prepare=NotAdmitted motivo=[No editable aquí: ilegible o de versión futura]`, `modulo-anidado corrida=NotAppliedAdmission llamadas=[Begin,ReadForPrepare,Dispose:scope]`, y las dos iguales con `ladoB-anidado`. **Se comprueba a sí misma:** la base del borrador y la de la relectura son iguales (`base-igual=True`); si no, el caso no ejerce la admisión, y es PBT3B-C07 | El doble: ninguna llamada de escritura | el esqueleto da `Invalid` en el `Prepare` y `NotAppliedFailure` en la corrida |
| **11** | **D-PB-05, pasos 5 a 7, al escribir:** un borrador completo rebasado con `KeepOn` sobre una base nueva con otra `CustomProperties`, otra versión menor del sobre y otro nombre → los sobres salientes llevan la `CustomProperties`, la versión y el `Id` del releído, y el diseño y el nombre del borrador. **El nivel superior, por construcción:** con una relectura admitida **fabricada** (constructor interno de `PushBackFullIntentResult`) cuyos sobre, proyecto y carga útil traen un campo desconocido, y un borrador cuya base es esa misma relectura, los tres campos se conservan en el texto saliente. La línea lleva el sufijo `por-construccion-no-ejercido-por-AUTH-13` | R-5; D-PBT3-05 | el esqueleto da `Invalid` |
| **12** | **D2, condición 8:** las huellas profundas de las referencias compartidas de los estados inicial y actual del borrador, iguales antes y después de un `Prepare` y de una corrida. La línea lleva la clase de los dos: `prepare=Ready corrida=Applied huellas-iguales=True` (V2-14); con otra clase, la línea no coincide | D-PBT1-15; A-2 | el esqueleto da `prepare=Invalid` y `corrida=NotAppliedFailure` |
| **13** | **G-L1 ampliada y «sin conectar»** (ExpectRed false), dos `[Fact]`: (i) las reglas de CT-PBT1B-14 sobre PP, UR e `IPushBackPilotViewEraser.cs`, con sus autopruebas; (ii) ningún archivo de `src/` fuera de PP, UR y `Draft/` nombra `PushBackFullPrepareInput` ni llama a `PushBackPilotUpdateRun.Execute` o `Precheck` con un `DraftSession<PushBackFullDraftState>`; en WHC, `Find<PushBackPilotDraftState>` sigue siendo el único `Find<`; autopruebas inyectadas en WHC | D-PBT1-15; OB-05.5 | **no falla en el RED** |
| **14** | **Los equivalentes de D-1:** E-1 a E-6 (variante (A), F4B-E29); un `[Fact]` cada uno (abajo) | El texto del GREEN | el texto del RED no tiene la forma nueva, y cada equivalente empieza afirmándola (V2-04) |
| **15** | **El `Precheck` y el orden del `Prepare` completos** (V2-16): `Precheck` sobre un borrador completo Clean → `NothingToApply` con `NothingToApplyReason`; con texto pendiente → `PendingText`; en Error → `NotApplicable`; en Dirty → `Proceeds=True`. El `Prepare` completo: con el borrador Clean y una relectura distinta → `NothingToApply` (la relectura no se mira); en Dirty con una relectura distinta y no admitida → `Conflict` (la comparación va **antes** de la admisión); con la misma base y una relectura no admitida → `NotAdmitted`; con un catálogo nulo tras la comparación → `Invalid(MissingInputReason)`. Una línea por caso, `<caso> <Kind>[ motivo=<texto>]` | El orden del `Prepare` del piloto (PP:309-334) y `Precheck` (UR:122-134) | el esqueleto de `Precheck` da `Go()` y el del `Prepare`, `Invalid` |

**Por qué A2-11 va en CT-PBT3B-01 (ii) y no en CT-PBT3A-06** (V42-02):
- **El sobre que sale solo existe desde PB-T3b:** es el `EnvelopeText` de cada miembro del plan. `ForWrite`, en PB-T3a, termina en el editor y las entradas, y CT-PBT3A-06 compara su diseño con el de `Pass`: los dos llevarían el exacto, así que no probaría que llega a lo que se escribe.
- **A2-11 pide la composición** (PREPARE → MUTATE → POST). MUTATE escribe el texto del plan y dibuja el sistema de `BuildForMember`: leer los dos del plan es leer lo que MUTATE escribe y dibuja.
- **No cambia ningún recuento:** CT-01 sigue siendo un `[Fact]`, con más líneas, y `MinSelected` 13 no cambia.
- **El tramo de punta a punta** desde «Editar todo» hasta el DWG es de PB-T4 y del Smoke-PB, como dice A2-11. La cota dibujada con su estilo, desde el sistema, ya la fija CT-PBTC-05 en RACKEDITAR.

**Los equivalentes de CT-14**, cada uno con lo que cubría el retirado (los seis de (A); lo que dicen «con (A′)» es registro y no va al contrato):

| Equivalente | Sustituye a | Qué fija |
|---|---|---|
| E-1 | S-1 | El plan compone **una vez**, en `ComposeMemberEnvelope(RackEmbedDocument source, string name, string view, int section, string designJson)`, con `RackEmbedComposer.Compose(source, RackEmbedDocument.KindPushBack, source.Id, name, view, section, designJson)`, sin `null`. El `name` de cada llamada sale de una sola expresión de `BuildCore`, la de D-PBT3-09 (f). **Con (A′):** de la sobrecarga de cuatro argumentos, con `source.Name`, o de `BuildFull`; **la sobrecarga de cuatro argumentos tiene que existir** y delegar en la de cinco, y si no existe, E-1 falla (V4-01) |
| E-2 | S-2 | **Empieza afirmando la forma nueva** (V2-04): `Assert.Contains` de cada literal que muta (`"source.Id, name,"`, `"RackEmbedComposer.Compose(source,"`, `"RackEmbedDocument source, string name"`, `"ComposeMemberEnvelope(RackEmbedDocument source"`) y `Assert.Empty(E1Violations(real))`. Después, las autopruebas sobre el texto real: otra llamada, `Compose(null,`, `RackEmbedDocument? source`, otro nombre de miembro y `source.Id, "Otro",` hacen fallar E-1 |
| E-3 | S-3 | **Sobre el cuerpo de `BuildCore` y el de `PushBackViewDispositions.Of`** (V2-03), con **todas** las agujas de S-3 (PrepareGuard:235-250): `reread.StaleBase.Entries.Values.Where(e => e.Membership == DraftMembership.Mutable).OrderBy(e => e.Key, StringComparer.Ordinal)` y `…StaleBase.Entries.Values.OrderBy(e => e.Key, StringComparer.Ordinal)` en `BuildCore`; `PreflightInnerSources(` y `RackSystemKind.PushBack` en `BuildCore`; `PushBackViewAvailabilityFacts(`, `RackViewAvailability.Evaluate(`, `RackViewAvailabilityStatus.Available`, `RackViewCodec.Decode(`, `PushBackSystemLateralBuilder().Cortes(` y `.IsComposite` en `Of`. Además: **`Erasures` solo se llena si se admite borrar y solo con `ObsoleteLateral`**; el piloto conserva `ViewUnavailablePrefix`; el nombre sale de la única expresión de E-1; la escala del plan es la del estado actual del modelo |
| E-4 | S-4 | Las mismas aserciones de UpdateGuard:126-141 sobre `Execution<TState>`: sin `using`; los dos ámbitos en `finally` propios con `try`/`catch (Exception ex)`; un `mutation.Dispose()` y un `scope.Dispose()`; POST solo tras liberar MUTATE (`private DraftApplyOutcome<TState> Post(IPushBackPilotUpdateScope scope)`). **Con (A′):** las mismas, **por corrida**, sobre `Execution` y `FullExecution`, **que tienen que existir las dos**; si falta una, E-4 falla (V4-01) |
| E-5 | S-5 | Las mismas reglas de `RunEscapeViolations` sobre **las dos** `Execute`, la del piloto y la completa, **que tienen que existir las dos** (si falta una, E-5 falla; V4-01), y sobre `OutcomeOfAnEscape` de `Execution<TState>`: el estado de la corrida antes del `try`; el `catch` que clasifica por ese estado; el sellado, después la escritura y después el fallo simple; nunca un `Applied` |
| E-6 | S-6 | **Empieza afirmando la forma nueva** (V2-04): `Assert.Contains` de `"var execution = new Execution<"`, `"return execution.OutcomeOfAnEscape(ex);"` y `"if (_sealReturned) return PostFailed(PostFailurePrefix + ex.Message);"` en UR, y `Assert.Empty(E5Violations(real))`. Después, las tres autopruebas de IntegrationGuard:907-909, adaptadas al tipo genérico: cada una tiene que **cambiar el texto** (`Assert.NotEqual(real, mutado)`) y dar violaciones |

**La letra, estricta** (V4-01; O-01 de la recomprobación de la v3): E-1 (con (A′)), E-4 (con (A′)) y E-5 exigen que existan **todos** los miembros que nombran. E-2 y E-6 empiezan con `Assert.Empty(E1Violations(real))` y `Assert.Empty(E5Violations(real))`, así que **caen con E-1 y E-5**. Por eso, al quitar el piloto, PB-T4 sustituye con registro:
- con (A): **E-3, E-5 y E-6** (tres);
- con (A′): **S-3, S-5 y S-6**, más **E-1, E-2 y E-4** (seis).

  - **Nota (no va al contrato).** La cuenta de (A) supone que PB-T4 solo quita lo del piloto y conserva `BuildCore` y `Execution<TState>`: entonces E-1, E-2 y E-4 siguen en verde. Si PB-T4 deshiciera también el núcleo común, sería una decisión suya, con su registro. La otra letra posible, «sobre cada uno que exista, y al menos uno», se analiza en §8.2.

**El motivo literal del `Skip`** (V2-26 y V41-02): «I-64 F4B PB-T3b: sustituida por <E-n> de PushBackPilotUpdateEquivalenceGuardTests (registro declarado superseding-characterization.md; decisión del Owner F4B-E29, D-1).» `<E-n>` es el equivalente de cada método (E-1 para S-1, …, E-6 para S-6). El número de la decisión es **F4B-E29** (`f4b-dispositions.md:800` en `d8ccbcb8`), y entra así en el contrato literal.

**La medición sobre el GREEN** (V2-02; INV-PBT3B-15; V3-03 y V3-04):
- **Cómo:** antes de su commit GREEN, el Worker quita en local el `Skip` de los métodos de D-1, corre esos métodos sobre el texto del GREEN y vuelve a poner el `Skip`. Es la **única reactivación** que admiten el invariante común 07 y C02: **temporal, local y sin commit** (§6.0). El commit GREEN no toca los archivos de guardas.
- **Qué transcribe,** por método: `<método> resultado=<Passed|Failed>`. Para S-2, una línea por fila, `S-2 fila=<n> from-presente=<b> resultado=<Passed|Failed>`; para S-6, `S-6 vacia=<b> resultado=<Passed|Failed>`. `from-presente` dice si el texto `from` de la fila está en PP (la fila empieza con `Assert.Contains(from, plan)`, PrepareGuard:136). `vacia` es cierto si S-5 falla sobre el mismo texto: entonces `RunEscapeViolations` ya da violaciones sin mutar nada, y los tres `Assert.NotEmpty` de S-6 (IntegrationGuard:907-909) no comprueban nada.
- **Lo esperado con (A):**
  - S-1, S-3, S-4 y S-5: `Failed`;
  - S-2: filas 1 y 3 con `from-presente=False` y `Failed`; filas 2, 4 y 5 con `from-presente=True` y `Passed` **en vacío** (el texto real ya da violaciones, porque S-1 falla);
  - S-6: `vacia=True` y `Passed` **en vacío**.
- **Lo esperado con (A′)** (registro; no elegida, no va al contrato):
  - S-1 y S-4: `Failed`;
  - S-2: fila 1 con `from-presente=False` y `Failed`; filas 2 a 5 con `from-presente=True` y `Passed` **en vacío** (V3-04): la sobrecarga de cuatro argumentos conserva `RackEmbedDocument source, string view`, y la de cinco, `RackEmbedComposer.Compose(source,`;
  - S-3, S-5 y S-6: `Passed` (S-6 con `vacia=False`: la `Execute` del piloto y su `OutcomeOfAnEscape` no cambian).
- **Además,** las otras clases de guardas de F4 de R-6, enteras y en verde.

Otra cosa es PBT3B-C16.

  - **Nota (no va al contrato).** La `Theory` S-2 se salta entera con un `Skip` en su atributo; sus cinco filas son las de PrepareGuard:128-132. Las dos líneas de censo salen de una corrida temporal más (INV-10), que lee PP y UR con `Src.Code` y no entra en ningún commit.

### 5.4 PB-T3c: el borrado en el dibujo (lista cerrada CT-PBT3C-01 a 05)

**Régimen** (V2-05):
- **el RED** son las pruebas nuevas (guardas de texto de Core) y **el esqueleto inerte de la fachada**: el archivo `RackViewBlockEraser.cs` con la clase y el método, cuyo cuerpo es solo `throw new NotSupportedException();`, sin ninguna otra sentencia y sin la cadena `PushBack`. WR no cambia en el RED. CT-01 a 04 fallan por aserción, porque el texto que fijan no existe todavía; CT-05 pasa, porque la fachada ya existe y no nombra ninguna ficha;
- **el GREEN** solo toca WR y la fachada.

**Las clases:** `PushBackViewEraseGuardTests` (01 a 04) y `PushBackWritePathPluginGuardTests` (05), en `tests/RackCad.Tests/`. Todas son de Core y leen el Plugin como texto, como las de F4.

| CT | Qué fija | Falla en el RED porque |
|---|---|---|
| **01** | **La fachada:** un archivo `src/RackCad.Plugin/Views/RackViewBlockEraser.cs` con `internal static class RackViewBlockEraser` y un solo método, `internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)`. En su orden: `GetObject(definitionId, OpenMode.ForRead)`, la comprobación de `IsLayout` que lanza, la captura de las anidadas, `GetBlockReferenceIds(directOnly: true, forceValidity: true)`, el `Erase()` de cada referencia, `UpgradeOpen()` y el `Erase()` de la definición. Sin `LockDocument`, `StartTransaction`, `StartOpenCloseTransaction`, `.Commit(`, `Purge`, `Regen`, `SyncName` ni la cadena `PushBack`, y sin nombrar el piloto (`NamesThePilot` de G-9: `PushBackPilot\w*\|RackPilot\w*\|RackCad\.Application\.Systems\.PushBack\.Pilot`, IntegrationGuard:679; V2-25). Autopruebas de cada prohibición | el esqueleto no tiene las operaciones |
| **02** | **`EraseUnit` en el escritor:** `RackPilotMutationScope` implementa `IPushBackPilotViewEraser`; el cuerpo de `EraseUnit`, en orden: `EnsureOwned();`, la búsqueda en `_erasures` que lanza, `_units++;`, `FailUnitIfRequested(_units);` y `RackViewBlockEraser.EraseInTransaction(_transaction,`, y las anidadas a `_staleDefinitions`; sin `SyncName`, purga, regeneración ni `Commit` | el método no existe |
| **03** | **PREPARE y la relectura:** `PrepareUnits` recorre `plan.Erasures`, resuelve cada clave en `_definitions` y devuelve `Failed` con el texto de hoy si falta; ningún `PrepareRedraw` para ellas; `Reread` calcula `matches` sobre `_prepared` y `_erasures` | ídem |
| **04** | **El censo de la primitiva:** en todo `src/`, `RackViewBlockEraser.EraseInTransaction(` aparece **exactamente una vez**, dentro de `EraseUnit` de WR. Autopruebas: una segunda llamada en WR, una en RPC y una en un archivo nuevo saltan | 0 llamadas en el RED |
| **05** | **G-L1 ampliada a WR y a la fachada** (ExpectRed false): las fichas de D2 no aparecen, y las reglas de CT-PBT1B-14 dan 0 coincidencias; autopruebas | **no falla en el RED** |

### 5.5 Lo que se prueba sin AutoCAD y lo que queda para el Smoke-PB (D-PB-09)

**Sin AutoCAD, en la CI de cada subtarea:** todo lo de §5.2 a §5.4, es decir:
- el modelo, el asiento frente a la ventana, el lector, AUTH-13 y la composición;
- la disposición y el clasificador;
- el plan y la corrida con dobles, la barrera, el borrado planificado, el renombrado y `ComparePost`;
- las huellas de D2;
- las guardas de texto del Plugin.

**Para el Smoke-PB de PB-T5,** y solo cuando PB-T4 haya conectado el modelo completo al panel (en PB-T3 nada de esto se alcanza desde el panel):
- el borrado real de un lateral obsoleto, con sus referencias y con una referencia anidada, y la purga de sus anidadas;
- **un solo `U`** por «Actualizar» con borrado, con renombrado y con la conversión A/B;
- el renombrado con colisión: la definición no cambia de nombre y el aviso `NameDiffersPrefix`;
- `LOCKED_LAYER` sobre una vista a borrar;
- el rechazo del frontal B y el del último vínculo, con sus textos;
- **el rack abierto desde un lateral que la edición deja obsoleto** (V2-01): el lateral se borra, el «Actualizar» termina aplicado y el borrador queda anclado en otra vista;
- la inyección de Debug de una unidad de borrado (`RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT` cuenta también los `EraseUnit`) y del sellado, con su aborto;
- el tiempo de un «Actualizar» grande (R-PB-06; M15 de PB-T1b);
- los racks A/B fabricados, rotulados CONSTRUCTED REPRESENTATIVE TEST CASES (OB-09.1).

**Lo único de PB-T3 que el panel alcanza antes de PB-T4** es la barrera del piloto (V2-09): un rack de un sentido con un diagnóstico bloqueante. El Smoke-PB lo comprueba si hay un rack así en las copias de los dibujos reales; si no lo hay, queda NOT AVAILABLE FOR PHYSICAL SMOKE y lo cubre CT-PBT3B-06.

### 5.6 Trazabilidad

| Obligación | CT |
|---|---|
| «field changes» | CT-PBT3A-06; CT-PBT3B-01 |
| «modules/fronts» | CT-PBT3B-02 y 03; CT-PBT3A-08 |
| «side B» | CT-PBT3A-02 y 03 (admisión en el modelo completo); CT-PBT3B-04 |
| «rename» | CT-PBT3B-05 |
| «deleting obsolete views» | CT-PBT3A-08; CT-PBT3B-02, 04, 07 a 09; CT-PBT3C-01 a 04 |
| «blocking authored write on diagnostics/errors» | CT-PBT3B-06, también sobre el piloto (V2-09) |
| «view creation only if PB-T0 subsequently authorizes it» | No se crea: CT-PBT3B-02 (claves del plan ⊆ releídas); D-PBT3-14 |
| «PREPARE -> MUTATE -> POST and one logical Update» | CT-PBT3B-07 a 09 y 15; Smoke-PB (un `U`) |
| El origen borrado (B-01 de la revisión) | CT-PBT3B-02 (iii) y 07 (ii); Smoke-PB |
| OB-05.2 (composición) y los pasos 4 a 7 | CT-PBT3A-06 y 07; CT-PBT3B-11; CT-PBT3A-04 y CT-PBT3B-10 (paso 6) |
| OB-05.3 (clasificador) | CT-PBT3A-09 y 10 |
| «sin conectar» (OB-05.2, OB-05.5) | CT-PBT3A-12; CT-PBT3B-13 |
| AUTH-13 (F4B-E24) | CT-PBT3A-04; CT-PBT3B-10 |
| D2, condiciones 1 a 4, 6 a 8 y 10; el límite | CT-PBT3A-05, 11 y 13; CT-PBT3B-12 y 13; CT-PBT3C-05; PBT3x-C17 |
| H-PB-17, H-PB-18 y la fila 3 del §6.4 | CT-PBT3A-08; CT-PBT3B-04 |
| D-1 (las sustituciones) | CT-PBT3B-14; la medición sobre el GREEN (INV-PBT3B-15) |
| D-2 (el registro sin `Skip`) | CT-PBT3C-01 a 04; la entrada de `superseding-characterization.md` |
| OB-06.3 y A2-11 de PB-T2 (el paso 4 de D-PB-06: el exacto y el estilo hasta el sobre que sale) | CT-PBT3B-01 (ii) (V42-02). De punta a punta desde «Editar todo»: PB-T4 y Smoke-PB |
| A2-12 de PB-T2 (M09: toda propiedad declarada de los tres documentos tiene fila en el mapa) | CT-PBT3A-09 (3); la fila por defecto de D-PBT3-07 (V42-03) |

---

## 6. Los contratos propuestos

**Cómo se lee** (V2-31; la lección V28-19 de PB-T1): en cada invariante y en cada parada, el texto normativo va primero, y lo explicativo en una **«Nota (no va al contrato)»**. El generador copia solo lo normativo. Las paradas propias van en inglés, como las demás del contrato.

### 6.0 Común a las tres subtareas

**Identidad:**
- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B;
- attempts 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true;
- evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Los `TaskId`:** de 24 a 26 caracteres, por MAX_PATH (R-T1-07).

**Las colecciones opacas** (F4B-E21): las ocho (`Authorities`, `Objective`, `AllowedWriteScope`, `ForbiddenWriteScope`, `Invariants`, `RequiredTests`, `ExpectedEvidence` y `StopConditions`) las pone la sesión byte a byte desde el contrato. El Controller hace la clase, el enrutamiento y los criterios de aceptación. Cada relevo lo declara.

**Las rutas `UNIT_CHANGE` y F4B-E25** (V2-29):
- cada contrato declara `UNIT_CHANGE` las rutas **existentes** de producto que cambia: PP y UR en PB-T3b, y WR en PB-T3c (PB-T3a solo crea archivos);
- **en una corrección** (F4B-E25, `F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md`), una ruta `UNIT_CHANGE` que aparezca en `git diff --name-only AuthorityRevision BaseSha` no incumple 16.3 si se cumplen las tres condiciones del Owner, que el Controller comprueba con git:
  1. todo commit de `AuthorityRevision..BaseSha` que la toca es un `RedSha` o un `CurrentSha` de una entrega de un Worker de la misma cadena (el mismo `TaskId`), según su `worker-handoff.json`;
  2. esa ruta está en el `AllowedWriteScope` de la delegación de esa entrega;
  3. ningún commit de la sesión de `AuthorityRevision..BaseSha` toca esa ruta.
- Las rutas `EXTERNAL` siguen prohibidas en ese diff, sin excepción. F4B-E25 va entre las autoridades de cada contrato.

**Anclajes:** antes de emitir, la sesión vuelve a medir sobre su `BaseSha`:
- las anclas de R-1 a R-18;
- el censo de R-6;
- los recuentos;
- la Snapshot vigente;
- para PB-T3b, el censo de los diseños de las clases de F4 (precondición 2);
- **las anclas de W, por contenido** (V42-10): el paquete las dice por su contenido; sus líneas se miden sobre la `BaseSha`, porque PB-T2b las mueve.

Lo deja en el commit de emisión. El generador de cada contrato (`make_pbt3x_contract.py`) comprueba con `assert`:
- el diff de `src` (V42-01):
  - **en PB-T3a**, frente al **cierre de PB-T2c** (vacío). Las referencias del anclaje: Application, la del cierre de PB-T1b más los dos archivos de PB-T2a (`PushBackDraftStatePort.cs` y `PushBackDraftEditorLaunch.cs`); Domain y Plugin, la del cierre de PB-T1b; `src/RackCad.UI`, la del cierre de PB-T2b (W y `PushBackEditorMode.cs`), con WC igual al cierre de PB-T1c;
  - **en PB-T3b y PB-T3c**, frente al cierre de la subtarea anterior de PB-T3 (vacío);
- que ninguna clase de los filtros de UI de la subtarea crea un `RackFrameConfiguratorWindow` real (la contención de F4B-E30), también sobre las clases de PB-T2 que entran en la red (V42-06);
- las anclas de PP, UR, UP, WR y PM que cita el diseño;
- el texto del esqueleto en el paquete custodiado;
- para PB-T3b, la lista explícita de `tests/RackCad.Tests/Workspace/` del `ForbiddenWriteScope`.

**La comprobación acotada** de cada subtarea: un revisor independiente, en solo lectura. Comprueba:
- que los anclajes se sostienen;
- que ningún hecho nuevo cambia un CT o el diseño;
- que no aparece ninguna decisión material nueva;
- que lo de D2 cubre sus condiciones como se declara;
- para PB-T3b, que la lista de D-1 de la variante elegida es la de R-6.

La exactitud de esa lista la mide después el Worker, sobre el texto del GREEN (INV-PBT3B-15; V2-02).

**`ForbiddenWriteScope`** (V2-06), lo calcula el generador con el método de [PKC-C]:
- todo `src/` salvo el `AllowedWriteScope` de la subtarea;
- en `tests/`:
  - las listas curadas de PB-T1 ([PKT1] §6.0), las clases de PB-T1 y de las subtareas anteriores de PB-T3, `tests/RackCad.Tests/Fixtures/I64/` y `tests/RackCad.UI.Tests/Workspace/`;
  - **las clases de PB-T2** (V42-05): las listas curadas de `tests/` del contrato de PB-T2c, más las clases de su `AllowedWriteScope` de pruebas. Son las nueve de PB-T2: en Core, `PushBackDraftStatePortTests`, `PushBackDraftPortSharedReferenceGuardTests`, `PushBackWorkspaceModeSourceGuardTests` y `PushBackWorkspaceSharedReferenceGuardTests`; en UI, `PushBackWorkspaceSupport`, `PushBackWorkspaceModeTests`, `PushBackWorkspaceApplyCloseTests`, `PushBackFieldValidationConcordanceTests` y `PushBackWorkspaceCarryoverTests`;
    - **Nota (no va al contrato).** Previsto por la revisión de compatibilidad (`compat-pbt3-vs-pbt2.md` §3), a medir por el generador: en PB-T3a, Core 29 y UI 68 en `tests/`, y 89 en `src/` (87 + los dos de PB-T2a); con las 17 de la raíz, 203. En PB-T3b, 98 en `src/`; en PB-T3c, 56.
  - **en PB-T3a y PB-T3c**, el prefijo `tests/RackCad.Tests/Workspace/`, que la lista curada de PB-T0 ya trae;
  - **en PB-T3b**, ese prefijo **no** va: va la lista explícita de sus archivos **menos los tres archivos de guardas de D-1** de la variante elegida, (A) (F4B-E29): **76 de 79** en `2f5d3dec` y en `d8ccbcb8` (V3-05; V41-03; R-15). Esos tres están en su `AllowedWriteScope`, y la sintaxis de 16.5 no admite excepciones dentro de un prefijo;
    - **Nota (no va al contrato).** Con (A′), no elegida, habrían sido 77, con `WorkspacePilotUpdateIntegrationGuardTests.cs` prohibido.
- las 17 entradas de la raíz.

**`StopConditions` estándar** (17), copiadas de [PKC2] §6.7 (`:579-581`): S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08. P-01 dice «config.toml no longer matches the accepted Snapshot in force (today Snapshot J, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json)» (F4B-E19; V2-10).

**Las de [DP], literales, en cada contrato** (x = A, B o C), con el texto de [PKT1] §6.0 y el prefijo cambiado:
- **PBT3x-C08:** una sola replanificación, solo por la comparación literal;
- **PBT3x-C11:** nc2 con la firma exacta de PD-F4-01 y nc1 con la de PD-F4-04, con los sustitutos copiados y solo el identificador y el ámbito cambiados; cualquier otra firma vuelve al Owner;
- **PBT3x-C12:** una corrección sería el intento 3: vuelve al Owner.

**El límite de D2, en cada contrato:** **PBT3x-C17**, con el texto literal del Owner ([PKT1] §6.0). Si hiciera falta mutar el rack resuelto o el informe de módulos dentro del tiempo de vida de la edición o del borrador, o compartir otro objeto mutable entre una copia del estado y su original: STOP y se reabre L-1. El Worker no elige ninguna salida.

**El RED de la cadena en una corrección** (§16.8, M-11): una corrección que toque `RT` ∪ `ChainRedFiles` exige RED otra vez:
- en PB-T3a, el commit RED vuelve a poner el esqueleto (`git restore --source=<RED del intento 1> -- <src del alcance>`);
- en PB-T3b, el esqueleto del API nuevo con el piloto de la base;
- en PB-T3c, el esqueleto de la fachada y WR de la base;
- el GREEN restaura la implementación sin tocar `tests/`.

**La lectura de `Ci` en el RED** (la mitad (a) de PBT1B-IR-M14, escrita aquí sin ratificación):
- en las tres subtareas, Core falla a propósito en el RED, y `build-plugin` tiene `needs: [tests, build-ui]` (`.github/workflows/ci.yml`), así que sale **omitido**;
- `ExpectedEvidence` lo pide así: `tests` en `failure`, `build-ui` en `success`, `build-plugin` omitido y `ui-tests` en `failure` (PB-T3a) o en `success` (PB-T3b y PB-T3c), sin cuelgue;
- en el GREEN, 4/4 en `success`.

**El `Trailer`** (V2-32; la mitad (b) de PBT1B-IR-M14, V28-17): la `ExpectedEvidence` de cada contrato pide que el `Trailer` de `worker-handoff.json` sea **el trailer real de los commits RED y GREEN** (`Co-Authored-By: …` del modelo que los hizo), nunca `null` si los commits lo llevan.

**`EligibleCells`:** las de [PKC2] §6.8 (`:655`):
- Worker `anthropic/subagent/claude-sonnet-5-5`, effort `high`;
- Controller `openai/process/gpt-6-luna`, effort `high`;
- vencen el **2026-12-29**;
- nc4 en la primera delegación de cada cadena; nc1 a nc3 tras la verificación.

**Invariantes comunes** (cada contrato los numera INV-PBT3x-07 a 14):
- **07. Las redes:** §5.1 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, con dos únicas salvedades, las dos en PB-T3b (V3-03):
  - los `Skip` de D-1, con su motivo literal;
  - la reactivación **temporal, local y sin commit** de esos mismos métodos para la medición de INV-PBT3B-15: el Worker quita el `Skip`, corre los métodos y lo vuelve a poner antes del commit GREEN. En `CurrentSha`, el `git diff -U0 BaseSha..CurrentSha` de los archivos de guardas solo lleva las líneas de atributo con `Skip`.
- **08. Las pruebas de UI:** las reglas de §5.0.
- **09. Determinismo:** GUID fijo, cultura invariante, «R» y orden ordinal; ni hora ni aleatoriedad.
- **10. Las corridas locales:**
  - solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, sí;
  - sin copias de árboles;
  - las corridas temporales terminan en `Assert.Fail(string.Join("\n", lineas))`, corren dentro de `EditorDiscardPrompt.Substitute` cuando abren ventanas, no entran en ningún commit y guardan TRX y log como artefactos.
- **11. La evidencia:** rutas relativas comprobadas en `worker-handoff.json`; `TreeSha` real o null; el `Trailer`, el real; el Worker no escribe en `docs/`.
- **12.** Las reglas de F4B; sin AutoCAD; la verificación con las tres comprobaciones deterministas de F4B-E15.
- **13. Las declaraciones nuevas en `src/`:** las de la lista cerrada de la parada C13 de cada contrato, y ninguna más (V3-02). **Una sola regla por contrato:**
  - **PB-T3a** (solo archivos nuevos): la lista de PBT3A-C13 y, además, estos ayudantes privados en los ocho archivos nuevos: métodos privados; tipos anidados privados sin estado; y campos `private static readonly` de un tipo inmutable (`Regex` o `string`);
  - **PB-T3b y PB-T3c** (archivos existentes): solo la lista de PBT3B-C13 o PBT3C-C13, **por nombre y acotada a los archivos que tocan**, sobrecargas incluidas. No se admite ningún ayudante privado fuera de ella.
  - **Nota (no va al contrato).** Es la lección PBT1B-IR-M10 (F4B-E22), escrita aquí sin ratificación, con la forma de PBT1C-C13 de la v2.8 custodiada (`:1338` en `833ea6bf`). En la v2, este invariante admitía métodos privados en los tres contratos, mientras que las paradas C13 de PB-T3b y PB-T3c los prohibían: eran dos reglas (N-02 de la recomprobación). En PB-T3a, los archivos son nuevos y su API está cerrada por D-PBT3-02 a 07, así que los ayudantes privados no cambian nada observable. En PB-T3b y PB-T3c, el diseño mecánico no necesita ninguno (D-PBT3-09, D-PBT3-10 y D-PBT3-11).
- **14. D2:** fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado.

**Las propias comunes** (con el prefijo de cada contrato):
- **C01:** doing the task would require writing outside the `AllowedWriteScope`.
- **C02:** it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching `InternalsVisibleTo`, the packages, a `.csproj` or `Fixtures/I64`. Only two exceptions, both in PB-T3b: the `Skip` of D-1; and the temporary, local and uncommitted re-enabling of those same methods for the measurement of INV-PBT3B-15, undone before the GREEN commit. Any existing test that fails, outside the D-1 list, returns to the Owner.
- **C03:** an observable change of RACKEDITAR appears, or of the pilot in the panel other than the barrier on a blocked design. STOP and return to the Owner.
- **C05:** a CT would need AutoCAD, a real `MessageBox`, the real configurator, the BOM, «Guardar en biblioteca», «Insertar», real keyboard or focus, or writing to the profile or to disk.
- **C06:** the GREEN CI fails outside the new tests, reproducibly and not attributable to them.
- **C07:** a fact of §2 does not hold in a way that changes a CT or the design; or an expected line is not met in the GREEN; or a self-checking recipe (CT-PBT3A-04, CT-PBT3B-06 and 10) does not exercise its path. The Worker does not adjust what is expected.
- **C09:** a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.
- **C10:** doing the task would require what D-PBT3-14 excludes: creating views; touching the classic editor, the window, the host or the panel port; connecting the full model; opening AUTH-13; or changing the treatment of the parked tail.

### 6.1 Contrato de PB-T3a — `F4B-PBT3A-FULL-WRITE-MODEL`

- **Objective** (ASCII): «Crear en Application, solo con archivos nuevos en src/RackCad.Application/Systems/PushBack/Draft/, el modelo del borrador completo de Push Back sobre PushBackEditableState: el estado y su comparador, el asiento (la pasada y la secuencia de LoadExisting), el materializador y el lector sobre el lector del piloto (AUTH-13 intacto; A/B admitido solo en este modelo; la cola aparcada fuera), la composicion para escribir y para Conservar mi borrador, la disposicion de las vistas (lateral obsoleto, frontal B de un rack de un sentido, sin direccion, ultimo vinculo) y el clasificador de D-PB-05, sin conectarlos al panel, segun el paquete del contrato de PB-T3 v4.2 y la lista cerrada CT-PBT3A-01..14. El RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion salvo las tres guardas; el GREEN implementa solo los archivos nuevos. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida de la edicion o del borrador: STOP y se reabre L-1. El piloto, el anfitrion, la ventana, el Plugin y Domain no cambian; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (14):
  - los ocho archivos nuevos de `src/RackCad.Application/Systems/PushBack/Draft/`: `PushBackFullDraftState.cs`, `PushBackFullDraftStateComparer.cs`, `PushBackEditableStateSeat.cs`, `PushBackFullDraftMaterializer.cs`, `PushBackFullDraftReader.cs`, `PushBackFullComposition.cs`, `PushBackViewDisposition.cs` y `PushBackConflictClassifier.cs`;
  - las cinco clases nuevas de `tests/RackCad.Tests/` de §5.2;
  - `tests/RackCad.UI.Tests/PushBackFullDraftWindowParityTests.cs`.
- **El esqueleto exacto del RED:**
  - `PushBackFullDraftState`, completo (es un contenedor);
  - el comparador: su constructor privado; `Instance`; `Equals` = `ReferenceEquals`; `GetHashCode` = 0; `Copy` = `new PushBackFullDraftState(new PushBackEditableState(), state.Origin)`;
  - el asiento: `Pass` devuelve `null`; `LoadExisting` devuelve `new PushBackEditableState()`;
  - `Materialize` devuelve `Failed(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic)`;
  - `Read` devuelve `NotAdmitted(PushBackPilotIntentReader.MaterializeDiagnostic)`, con la base de `PushBackPilotIntentReader.BuildStaleBase(scan)`;
  - `ForWrite` devuelve `Rejected(PushBackPilotMaterializer.NonPilotContentReason)`; `KeepOn` devuelve `current`;
  - `Of` da todo `Redraw` y `AllObsolete` falso;
  - `Classify` da todas las listas vacías y `AllViewsObsolete` falso.
  
  Los tipos, enumerados, constructores y propiedades, completos.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackFullDraft` (CT-02 a 11) | **10** | true |
  | UI | `PushBackFullDraftWindowParityTests` | **1** | true |
  | Core | `PushBackDraftWriteGuardTests` (CT-12 a 14) | **3** | false |
  | Core y UI | `Workspace` | 3 271 y 547 | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackSharedReferenceGuardTests` | 13, 1, 2 | false |
  | Core y UI | Las clases de PB-T1c (`PushBackWindowSharedReferenceGuardTests`, `PushBackWindowOnStateSourceGuardTests`; `PushBackWindowOnStateTests`) | 1, 1; 2 | false |
  | UI y Core | `PushBackClassicCharacterization`; `PushBackFullEditorCharacterization` | 55 y 10; 16 y 3 | false |
  | UI | `PushBackStateNet`; `PushBackDeclaredCorrection`; `Cantilever` | 14; 14; 67 | false |
  | Core | `I58F1`; `CustomPropertiesEnvelopeGuardTests`; `NamespaceFolderGuardTests` | 10; 9; 7 | false |
  | Core | Las clases de PB-T2 (V42-06): `PushBackDraftStatePortTests`, `PushBackDraftPortSharedReferenceGuardTests`, `PushBackWorkspaceModeSourceGuardTests`, `PushBackWorkspaceSharedReferenceGuardTests` | 3, 1, 1, 1 | false |
  | UI | Las clases de PB-T2 (V42-06): `PushBackWorkspaceModeTests`, `PushBackWorkspaceApplyCloseTests`, `PushBackFieldValidationConcordanceTests`, `PushBackWorkspaceCarryoverTests` | 3, 4, 1, 3 | false |

  - **Nota (no va al contrato).** Los `MinSelected` de PB-T1, de PB-T2 y de `Workspace` se vuelven a medir en la emisión. Los de PB-T2 son los mínimos de su paquete (v2.6), según la revisión de compatibilidad (§3); ningún filtro de PB-T3 selecciona hoy una clase de PB-T2 por prefijo, y `Workspace` (3 271 y 547) no cambia por PB-T2.
- **Invariantes propios:**
  - **INV-PBT3A-01. Alcance:** el diff dentro de las 14 rutas; `git diff BaseSha..CurrentSha -- src` solo añade los ocho archivos de `Draft/` y no modifica ningún archivo existente.
  - **INV-PBT3A-02. RED y GREEN:** el RED es el esqueleto exacto y las seis clases de prueba; el GREEN solo toca los ocho archivos de `Draft/`. En el RED, cada filtro con `ExpectRed` da todos `Failed` por aserción (la primera línea del `StackTrace` bajo `tests/`), con `Failed == Selected`; el filtro de las guardas pasa y sus autopruebas fallan sobre el texto inyectado.
  - **INV-PBT3A-03. La API:** exactamente la de D-PBT3-02 a D-PBT3-07, con sus constructores.
  - **INV-PBT3A-04.** Un solo `namespace RackCad.Application.Systems.PushBack.Draft` de bloque por archivo; ningún archivo nuevo compone, construye un `RackEmbedDocument` ni nombra lo prohibido por D-PBT3-08.
  - **INV-PBT3A-05.** El lector completo llama a `PushBackPilotIntentReader.Read` exactamente una vez por lectura; el materializador completo llama a `PushBackEditableStateSeat.LoadExisting` exactamente una vez.
  - **INV-PBT3A-06.** Ningún archivo de `src/` fuera de `Draft/`, PP y UR nombra los tipos nuevos (CT-12; V2-28).
- **`StopConditions` propias:**
  - **PBT3A-C04:** a method of a filter with `ExpectRed` passes on the skeleton, or fails by an exception and not by an assertion.
  - **PBT3A-C13** (V2-07 y V2-30): doing the task would require modifying an existing file under `src/`; or adding, in the new files, a public or internal declaration (type, constructor, method, property, field, constant or enum member) whose name is not in the closed list of D-PBT3-02 to D-PBT3-07; or a private declaration other than those admitted by INV-PBT3A-13. The Worker stops before adding it.
    - **Nota (no va al contrato).** La lista cerrada son los bloques de código de D-PBT3-02 a D-PBT3-07, con sus constructores (V2-07): los privados de `PushBackFullDraftStateComparer`, `PushBackFullDraftMaterialization` y `PushBackFullWriteComposition`; los internos de `PushBackFullIntentResult`, `PushBackViewDispositionPlan`, `PushBackChangeEntry` y `PushBackConflictClassification`; el público de `PushBackFullDraftState`. Las propiedades son automáticas de solo lectura.
  - **PBT3A-C15:** CT-01 gives `estado-igual=False` or `diseno-igual=False` for some design: the seat does not match the window. It returns to the session, which takes it to the Owner if it is a loss (OB-06.5).
  - **PBT3A-C17:** the D2 limit (§6.0).
- **`ExpectedEvidence`:**
  - antes del Worker: el contrato, la salida del Controller (`output.json`, con las ocho colecciones opacas vacías), la delegación final con esas colecciones puestas por la sesión (`delegation.json`; F4B-E21), la comparación byte a byte sobre la delegación final, A1-A8 y nc4;
  - el commit RED y su push antes del GREEN;
  - la CI del RED como en §6.0 (`tests` y `ui-tests` en `failure`, `build-ui` en `success`, `build-plugin` omitido, sin cuelgue);
  - la tabla «CT → la aserción que falla en el RED» y las autopruebas de las tres guardas;
  - el GREEN y su CI 4/4;
  - en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED y el GREEN, y tres corridas en verde de los filtros con `ExpectRed` sobre el GREEN;
  - la entrega con el mapa CT → método, las rutas relativas comprobadas y el `Trailer` real de los commits RED y GREEN;
  - la verificación con las tres comprobaciones de F4B-E15, y nc1 a nc3.
- **Línea base de la CI** (V42-01): la del cierre de PB-T2c, **a medir**. **Prevista**, no medida (el GREEN de PB-T2c en su paquete v2.6): Core 19 094; UI 2 450 (2 429 `Passed`, 21 `Skipped`). **GREEN previsto:** Core + 13 = **19 107**; UI + 1 = **2 451**.
  - **Nota (no va al contrato).** La última medida es la del cierre de PB-T1c (F4B-E31, CI `44463212627`): Core 19 088 y UI 2 439. PB-T2 añade, según su paquete, Core + 6 y UI + 11.

### 6.2 Contrato de PB-T3b — `F4B-PBT3B-FULL-WRITE-RUN`

**La variante es (A)** (F4B-E29, `d8ccbcb8`; V41-03). Lo normativo de este apartado es solo de (A). Lo que dice «con (A′)» es registro de la variante no elegida, en una **Nota (no va al contrato)**: el generador no lo copia.

- **Objective** (ASCII), con la variante (A), la elegida (F4B-E29): «Generalizar en su sitio el plan y la corrida de la actualizacion del Workspace de Push Back para que sirvan el modelo del piloto, conectado, y el modelo completo de PB-T3a, sin conectar: la barrera de diagnosticos con PushBackEditorOutputGate para los dos modelos (el unico cambio visible del piloto, sobre un diseno bloqueado), el borrado de laterales obsoletos con la regla del ultimo vinculo y el origen de POST reanclado cuando se borra el origen, el rechazo del frontal B de un rack sin lado B, el renombrado con el nombre del borrador, ComparePost generico y la capacidad de borrar como interfaz nueva, con un solo BeginMutation y un solo Seal, segun el paquete del contrato de PB-T3 v4.2, la decision del Owner D-1 (F4B-E29, un solo codigo) y la lista cerrada CT-PBT3B-01..15. El RED es el esqueleto inerte del API nuevo, la interfaz, las pruebas nuevas y el Skip de las pruebas sustituidas por D-1; el GREEN toca solo el plan y la corrida. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1. El Plugin, el anfitrion, el puerto del panel, la ventana, el clasico y Domain no cambian; no se crean vistas; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (11):
  - PP, UR y el nuevo `src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotViewEraser.cs`;
  - las cuatro clases nuevas de §5.3 y `tests/RackCad.Tests/PushBackFullWriteSupport.cs` (V2-06);
  - **solo para los `Skip` de D-1:** `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`, `…/WorkspacePilotUpdateGuardTests.cs` y `…/WorkspacePilotUpdateIntegrationGuardTests.cs`.
  - **Nota (no va al contrato).** Con (A′), no elegida, habrían sido 10, sin el tercero (V3-05).

  El registro `F4B-DRAFTS/superseding-characterization.md` no entra: lo escribe la sesión en el commit de emisión (§8.2).
- **`UNIT_CHANGE`:** PP y UR (§6.0; F4B-E25).
- **El esqueleto exacto del RED:**
  - en PP: `PushBackFullPrepareInput` completo (es un contenedor con su `From`); la sobrecarga completa de `Prepare` devuelve `WithInvalid(MissingInputReason)`; `Erasures` y `ErasedKeys`, vacías; `PostOriginKey` = `OriginKey`; los constructores nuevos y las constantes nuevas, completos;
  - en UR: `Precheck` completo devuelve `Go()`; `Execute` completo devuelve `NotAppliedFailure(UpdateNotRunReason)`; la sobrecarga completa de `ComparePost` devuelve `PostRereadMissingReason`; las constantes nuevas, completas;
  - **el camino del piloto, sin cambio.**
- **El `Skip` de D-1,** con el motivo literal de §5.3 (con F4B-E29), en los seis métodos S-1 a S-6. Es el único cambio permitido en esos archivos; `git diff -U0` lo comprueba.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackFullWrite` (CT-01 a 12 y 15) | **13** | true |
  | Core | `PushBackPilotUpdateEquivalenceGuardTests` (CT-14) | **6** | true |
  | Core | `PushBackWritePathGuardTests` (CT-13) | **2** | false |
  | Core | `Workspace` | el número de `Passed` de la CI del RED (V2-26) | false |
  | Core | `PushBackFullDraft`, `PushBackDraftWriteGuardTests` (PB-T3a) | 10, 3 | false |
  | | Y los demás de §6.1, salvo los de PB-T3a con `ExpectRed` | | false |

  - **Nota (no va al contrato).** El `MinSelected` de `Workspace` es 3 271 menos los resultados que dejan de ejecutarse con los `Skip`. Con (A): las cinco `[Fact]` y la `Theory` S-2, que cuenta como una fila o como cinco según la TRX. Lo fija la sesión en la emisión con la TRX de la CI del RED, que ya lleva los `Skip` (R-11).
- **Invariantes propios:**
  - **INV-PBT3B-01. Alcance:** el diff dentro de las 11 rutas del `AllowedWriteScope`. En `src/`, solo PP, UR y el archivo nuevo. En los archivos de guardas de F4, solo las líneas de atributo con `Skip` de S-1 a S-6.
  - **INV-PBT3B-02. RED y GREEN:** como INV-PBT3A-02. El GREEN solo toca PP y UR.
  - **INV-PBT3B-03. El API existente no cambia:** cada miembro público e interno de PP y UR en `BaseSha` existe en el GREEN con la misma firma.
  - **INV-PBT3B-04. El piloto** conserva, en `Prepare`, su orden, sus textos y sus resultados; en la corrida, sus llamadas al puerto; su `ComparePost`; y su origen de POST (`PostOriginKey` = `OriginKey`). La única excepción es la barrera, sobre un diseño bloqueado (V2-09).
    - **Nota (no va al contrato).** Lo prueban las clases de comportamiento del piloto de §4, en verde, y CT-PBT3B-06 (ii) fija la excepción.
  - **INV-PBT3B-05. Las guardas que siguen** (§4): la llamada a `ComposeForWrite` en el `Build` del piloto, tras `.CompareWith(`; `From(DraftSession<PushBackPilotDraftState>)` y una sola `new PushBackPilotPrepareInput(`; el `Prepare` del piloto primero en el archivo; ninguna palabra `Transaction` en `Pilot/`; las llamadas al puerto solo en UR; UP sin cambio.
  - **INV-PBT3B-06. Un solo «Actualizar» lógico:** un `Begin`, un `BeginMutation`, un `Seal` y un `Post` por corrida, y todo `EraseUnit` entre `BeginMutation` y `Seal` (CT-07).
  - **INV-PBT3B-15. La medición sobre el GREEN** (V2-02, V3-03 y V3-08): antes de su commit GREEN, el Worker hace la corrida temporal de §5.3, «La medición sobre el GREEN», y la transcribe en la entrega. Para hacerla, quita en local el `Skip` de los métodos de D-1 y lo vuelve a poner antes del commit: es la reactivación temporal, local y sin commit que admiten INV-PBT3B-07 (las redes, el común de §6.0) y PBT3B-C02. El resultado tiene que ser el esperado de la variante elegida.
    - **Nota (no va al contrato).** En la v2 se llamaba INV-PBT3B-07, el mismo número que el invariante común 07 de este contrato (V3-08). Va detrás de los comunes, 07 a 14.
- **`StopConditions` propias:**
  - **PBT3B-C04:** as PBT3A-C04.
  - **PBT3B-C13** (V2-30 y V3-02), con la variante (A), la elegida (F4B-E29): doing the task would require changing the signature of an existing public or internal member of PP or UR; touching UP, the Plugin, WHC or the panel port; or adding in PP, UR or `IPushBackPilotViewEraser.cs` a declaration whose name does not exist in PP or UR at `BaseSha`, or a further declaration of a name that does (an overload), of any visibility (type, constructor, method, property, field, constant or nested type), other than those that D-PBT3-09 and D-PBT3-10 prescribe:
    - in PP: the type `PushBackFullPrepareInput` and its nine members (its private constructor, `RackId`, `StaleBase`, `InitialDraftState`, `CurrentDraftState`, `HasPendingText`, `IsDirty`, `ConsideredStatus` and `From`); in `PushBackPilotUpdatePlanner`, a second `Prepare` (the one that takes a `PushBackFullPrepareInput`), `BuildCore`, `BuildFull` and the constants `BlockedPrefix`, `FrontalBOfOneWayPrefix`, `FrontalBOfOneWaySuffix` and `AllViewsObsoleteReason`; in `PushBackPilotUpdatePlan`, `Erasures`, `PostOriginKey` and a second internal constructor (the one with `postOriginKey` and `erasures`); in `PushBackPilotPostExpectation`, `ErasedKeys` and a second internal constructor (the one with `erasedKeys`);
    - in UR: a second `Precheck`, a second `Execute` and a second `ComparePost` (the ones that take the full model), `ComparePostCore` and the constants `EraseUnsupportedReason` and `PostErasedStillPresentPrefix`; and, in `Execution<TState>`, the fields `_prepare`, `_originOf`, `_postRead` and `_origin`;
    - in `IPushBackPilotViewEraser.cs`: the interface `IPushBackPilotViewEraser` and its method `EraseUnit`.
    
    Two changes of private members are prescribed, and neither is a new declaration: `ComposeMemberEnvelope` gains the parameter `string name` after `source` and remains the only member with that name; and `Execution` becomes `Execution<TState>`, loses the field `_input`, and its constructor gains the three delegates of `_prepare`, `_originOf` and `_postRead`, while each of its other members keeps its name and its parameters, with `PushBackPilotDraftState` replaced by `TState` where it appears (the field `_draft`, `Run`, `OutcomeOfAnEscape`, `Prepare`, `Mutate`, `Write`, `WithAbortFailure`, `Post`, `ChooseOrigin`, `SurvivorsAndLayers`, `WithScopeDisposeFailure`, `PostFailed`, `NotAppliedFailure` and `NotAppliedPreflight`). The Worker stops before adding it.
  - **Nota (no va al contrato): PBT3B-C13 con la variante (A′), no elegida** (F4B-E29; D-PBT3-09b; V41-03). Queda como registro; el generador no la copia. Su texto era: the same text, with these lists:
    - in PP: the type `PushBackFullPrepareInput` and its nine members; in `PushBackPilotUpdatePlanner`, a second `Prepare`, `BuildFull`, a second `ComposeMemberEnvelope` (the one with five parameters, with `string name` after `source`, to which the existing one delegates with `source.Name`) and the four constants; in `PushBackPilotUpdatePlan` and `PushBackPilotPostExpectation`, the same as with (A);
    - in UR: a second `Precheck`, a second `Execute` and a second `ComparePost`, `ComparePostCore` and the two constants; and the private nested class `FullExecution`, with at most one declaration of each member name that `Execution` has at `BaseSha` (its fields and its constructor included), typed on `PushBackFullDraftState` and `PushBackFullPrepareInput`, and no other member;
    - in `IPushBackPilotViewEraser.cs`: the same as with (A).
    
    No signature of an existing member changes: `Build`, `Execution` and its members, and the existing `ComposeMemberEnvelope`, keep theirs (the existing `ComposeMemberEnvelope` only changes its body). The Worker stops before adding it.
    - **Nota (no va al contrato).** Es la forma de PBT1C-C13 de la v2.8 custodiada (`:1338` en `833ea6bf`): por nombre, acotada a los archivos que se tocan, con las sobrecargas, y con lo que el diseño prescribe enumerado (N-02 de la recomprobación). La firma solo para en lo público e interno, como INV-PBT3B-03. Los miembros privados que cambian de firma se leyeron en `833ea6bf` (UR:227-587; PP:434; R-18): además de los que nombraba la recomprobación, `ChooseOrigin`, `SurvivorsAndLayers` y el campo `_draft` también llevan `PushBackPilotDraftState` en su firma. **El censo, con (A), según el diseño:** 34 declaraciones nuevas (22 en PP, 10 en UR y 2 en el archivo nuevo) y una retirada (`_input`). El diseño no necesita ningún ayudante privado: el `Precheck` completo repite el `switch` de cuatro casos sobre su propio resultado.
  - **PBT3B-C14:** a behaviour test of the F4 pilot (§4) fails. STOP: it is C03.
  - **PBT3B-C16:** the measurement of INV-PBT3B-15 differs from what is expected for variant (A), or an F4 guard outside the D-1 list fails in the RED or in the GREEN. It returns to the Owner with the list.
  - **PBT3B-C17:** the D2 limit.
- **`ExpectedEvidence`:**
  - lo de §6.1, con la CI del RED con `tests` en `failure`, `ui-tests` y `build-ui` en `success` y `build-plugin` omitido;
  - **el `git diff -U0 BaseSha..CurrentSha` de los archivos de guardas de F4, que solo lleva las líneas de atributo con `Skip`** de S-1 a S-6 (V3-03): ni la reactivación de la medición ni ningún otro cambio;
  - el recuento de `Skipped` y `Passed` del filtro `Workspace` en el RED y en el GREEN;
  - la medición de INV-PBT3B-15, con las líneas de censo de S-2 y S-6;
  - **el censo de declaraciones** de PP, UR e `IPushBackPilotViewEraser.cs` en `BaseSha` y en `CurrentSha`, contadas una a una con su nombre (una sobrecarga es una declaración más): la diferencia es la lista de PBT3B-C13 de la variante (A), sin `_input`;
  - el `Trailer` real.
- **Línea base de la CI:** la del cierre de PB-T3a. **GREEN previsto,** con la variante (A): Core + 21 (13 + 6 + 2), con los `Skipped` de Core en + 6 (o + 10, según cuente la TRX las filas de S-2; R-11); UI sin cambio. En cadena desde lo previsto de PB-T3a (V42-01): Core **19 128** y UI **2 451**, previstos.
  - **Nota (no va al contrato).** Con (A′), no elegida, habría sido Core + 18.

### 6.3 Contrato de PB-T3c — `F4B-PBT3C-FULL-WRITE-ERASE`

- **Objective** (ASCII): «Dar al escritor de la actualizacion del Workspace de Push Back la capacidad de borrar una vista obsoleta dentro de la unica transaccion de MUTATE: una fachada caller-owned nueva (RackViewBlockEraser.EraseInTransaction, sin bloqueo, sin transaccion propia, sin confirmacion y sin purga, con el borrado de las referencias directas y la captura de las definiciones anidadas del borrado clasico) y su uso desde EraseUnit del ambito de MUTATE, con PrepareUnits y la relectura de MUTATE que cubren las vistas a borrar, segun el paquete del contrato de PB-T3 v4.2 y la lista cerrada CT-PBT3C-01..05. El RED son las guardas de texto nuevas y el esqueleto inerte de la fachada; el GREEN toca solo el escritor y la fachada. Las guardas del escritor de F4 siguen en verde, con su registro declarado sin Skip. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1. Application, la ventana, el clasico y Domain no cambian; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (4): WR; `src/RackCad.Plugin/Views/RackViewBlockEraser.cs`; `tests/RackCad.Tests/PushBackViewEraseGuardTests.cs` y `tests/RackCad.Tests/PushBackWritePathPluginGuardTests.cs`.
- **`UNIT_CHANGE`:** WR (§6.0; F4B-E25).
- **El RED** (V2-05): las dos clases de prueba y el esqueleto inerte de la fachada (§5.4). WR no cambia.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackViewEraseGuardTests` | **4** | true |
  | Core | `PushBackWritePathPluginGuardTests` | **1** | false |
  | Core | `WorkspacePilotUpdateGuardTests`, `WorkspacePilotUpdateIntegrationGuardTests` | los medidos tras PB-T3b | false |
  | Core | `PushBackSharedReferenceGuardTests` (CT-PBT1B-15: 14 archivos y 7 asignaciones) | 2 | false |
  | | Y los de §6.2 sin `ExpectRed` | | false |
- **Invariantes propios:**
  - **INV-PBT3C-01. Alcance:** dentro de las 4 rutas. `git diff BaseSha..CurrentSha -- src/RackCad.Application src/RackCad.UI src/RackCad.Domain src/RackCad.Plugin/RackPushBackCommands.cs src/RackCad.Plugin/RackCommandSupport.cs` vacío.
  - **INV-PBT3C-02. RED y GREEN:** el RED son las dos clases de prueba y el esqueleto de la fachada, con WR sin cambio; el GREEN solo toca WR y la fachada. Criterio del RED como INV-PBT3A-02.
  - **INV-PBT3C-03. El escritor:** sigue con tres clases `RackPilot*`, un `StartTransaction`, un `Commit`, una purga y dos `[Conditional("DEBUG")]`; no nombra `GetBlockReferenceIds`, `Erase(`, `EraseViewBlocks`, `ForWrite` ni `UpgradeOpen`; no añade ninguna asignación `.Name =`.
  - **INV-PBT3C-04.** La fachada no contiene la cadena `PushBack` ni nombra el piloto; el censo de CT-PBT1B-15 sigue en 14 archivos.
- **`StopConditions` propias:**
  - **PBT3C-C04:** as PBT3A-C04.
  - **PBT3C-C13** (V2-30 y V3-02): doing the task would require touching Application, RPC, RCS or any other file of the Plugin; or adding in WR or `RackViewBlockEraser.cs` a declaration whose name does not exist in WR at `BaseSha`, or a further declaration of a name that does (an overload), of any visibility (type, constructor, method, property, field, constant or nested type), other than the five that D-PBT3-11 prescribes: the field `_erasures` of `RackPilotUpdateScope`, the field `_erasures` of `RackPilotMutationScope` and the method `EraseUnit` of `RackPilotMutationScope`, in WR; and the class `RackViewBlockEraser` and its method `EraseInTransaction`, in `RackViewBlockEraser.cs`. One change of an existing member is prescribed and is not a new declaration: the internal constructor of `RackPilotMutationScope` gains one parameter, for `_erasures`. The Worker stops before adding it.
    - **Nota (no va al contrato).** La forma de PBT1C-C13 de la v2.8 (V3-02). WR, leído en `833ea6bf` (R-18), no tiene ningún `_erasures`, `EraseUnit`, `RackViewBlockEraser` ni `EraseInTransaction`. El diseño no necesita ningún ayudante privado: `EraseUnit` usa `EnsureOwned` y `FailUnitIfRequested`, que ya existen.
  - **PBT3C-C16:** an F4 writer guard fails. It returns to the Owner: D-1 does not cover the writer.
  - **PBT3C-C17:** the D2 limit.
- **`ExpectedEvidence`:**
  - lo de §6.1;
  - la CI del RED con `tests` en `failure`, `ui-tests` y `build-ui` en `success` y `build-plugin` omitido;
  - la del GREEN 4/4, con el Plugin compilado;
  - **el censo de declaraciones** de WR y de la fachada en `BaseSha` y en `CurrentSha`, contadas una a una con su nombre (una sobrecarga es una declaración más): la diferencia son las cinco de PBT3C-C13 (V3-02);
  - el `Trailer` real;
  - la entrada de D-2 en `superseding-characterization.md`, escrita por la sesión en el commit de emisión (V2-11).
- **Línea base de la CI:** la del cierre de PB-T3b. **GREEN previsto:** Core + 5; UI sin cambio. En cadena (V42-01): Core **19 133** y UI **2 451**, previstos.

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-T3-01 | **Sin latitud para el Worker** | El API, el esqueleto, las listas de CT, los textos y las recetas están en el paquete. Lo que no está es una parada |
| R-T3-02 | **Nada de AutoCAD en el Worker** | C05; el Plugin se prueba como texto; lo físico, en el Smoke-PB (§5.5) |
| R-T3-03 | **Corridas temporales** | Con `Assert.Fail`, fuera de todo commit, con TRX y log (INV-10) |
| R-T3-04 | **MAX_PATH** | `TaskId` de 24 a 26 caracteres; sin copias de árboles |
| R-T3-05 | **Un solo «Actualizar» lógico** | Una transacción con los borrados dentro (INV-PBT3B-06, CT-PBT3B-07); POST una vez; Smoke-PB mide el `U` |
| R-T3-06 | **Guardas de F4 que no pueden seguir en verde, o que quedarían vacías** | D-1, decidida (F4B-E29): las seis de (A) (cinco fallan y una queda vacía). Cada una con su equivalente, que empieza afirmando la forma nueva (V2-04). La lista exacta se mide sobre el GREEN (INV-PBT3B-15), y C16 cubre cualquier otra |
| R-T3-07 | **El coste de un «Actualizar» completo** (M15 de PB-T1b; V2-21) | No es solo el comparador. Hay varias pasadas completas por «Actualizar»: en PREPARE, la relectura (la materialización del piloto y las tres `Pass` del asiento); en `ForWrite`, la materialización otra vez (tres `Pass` más) y una comparación completa; en POST, la relectura otra vez (lo mismo que en PREPARE). Unas nueve pasadas, más dos comparaciones. Va a R-PB-06 y se mide en el Smoke-PB; PB-T4 decide si se cachea (A-T3-4) |
| R-T3-08 | **La tentación de un segundo escritor** | Prohibido: G-2 y G-7 siguen en verde y el diseño lo exige (D-PBT3-01) |
| R-T3-09 | **PB-T2 delante** (R-PB-09; V42-08) | PB-T2 va antes de PB-T3a. Alcances disjuntos: PB-T2 toca la ventana, `PushBackEditorMode.cs` y dos archivos de Application fuera de `Draft/`; PB-T3, `Draft/`, PP, UR y WR. PB-T3a se emite sobre el cierre de PB-T2c y se vuelve a medir sobre él (precondiciones 1 y 2) |
| R-T3-10 | **Las lecciones de PB-T1b** | Los ayudantes privados, admitidos por regla (INV-13), y **las declaraciones nuevas paradas por nombre** (C13; V2-30); `build-plugin` omitido en el RED y el `Trailer` real (§6.0; V2-32); **F4B-E25** para las rutas `UNIT_CHANGE` de una corrección (§6.0; V2-29); lo normativo separado de las notas (V2-31); el tamaño del contrato (las colecciones opacas, por la sesión, F4B-E21; las listas de autoridades y de alcance prohibido, calculadas por el generador) |
| R-T3-11 | **Un modelo interino:** entre PB-T3 y PB-T4 hay dos composiciones (la del piloto y la completa) | Se declara como interino, igual que R-T1-17. PB-T4 retira la del piloto al conectar el modelo completo |
| R-T3-12 | **El censo de `Name =` del Plugin** (CT-PBT1B-15: 14 archivos con `PushBack`) | La fachada no contiene `PushBack` (INV-PBT3C-04) |
| R-T3-13 | **El censo de I-54** de los que componen | Solo `ComposeMemberEnvelope(source, …)`; nada nuevo compone (INV-PBT3A-04) |
| R-T3-14 | **nc1 con variaciones** (PB-TC, PB-TC2) | C11: vuelve al Owner. Ninguna aceptación puntual anterior se extiende |
| R-T3-15 | **Las celdas vencen el 2026-12-29** | A7 en cada emisión y delegación; tres cadenas antes de esa fecha |
| R-T3-16 | **La admisión del compuesto depende de dos constantes del piloto** (D-PBT3-04) | CT-PBT3A-03 la fija regla a regla. Si el piloto cambiara esos textos, CT-03 salta |
| R-T3-17 | **El asiento no iguala a la ventana en algún diseño** | CT-PBT3A-01 con su parada C15; el oráculo es la ventana, a través de PB-T1c |
| R-T3-18 | **Borrar la vista de origen** (B-01 de la revisión; V2-01) | `PostOriginKey` reanclado (D-PBT3-09 (h)); CT-PBT3B-02 (iii) y 07 (ii); Smoke-PB. PB-T4 tiene que usar el origen del borrador y no una selección que ya no existe (A-T3-8) |
| R-T3-19 | **La dependencia cíclica `Pilot` ↔ `Draft`** (M-15 de la revisión; V2-27) | Interina y declarada (D-PBT3-04). PB-T4 retira el piloto (A-T3-10) |
| R-T3-20 | **Dos algoritmos de escritura con (A′)** | **Ya no aplica:** el Owner eligió (A), un solo escritor (F4B-E29; V41-03). Queda como registro |
| R-T3-21 | **Un modo de fallo repetido del redactor:** Python con heredoc (R-12; ya en la medición H-12 de PB-T1) | Se devuelve al Coordinator como modo repetido (V2-24). La v2 y la v3 no usaron ningún heredoc: solo scripts escritos con la herramienta de archivos |

## 8. Las decisiones del Owner

### 8.1 Ya tomadas, que este paquete aplica

- **[OD], la fila de PB-T3** (literal, arriba) → todo el paquete.
- **[OD] D-PB-04:** «F4B may delete obsolete sibling views when the new full design no longer requires them. Creating NEW views is NOT yet authorized by this decision.» → D-PBT3-06, D-PBT3-09 a D-PBT3-11. **El bloque M** resolvió la fila 1 del §6.4: «PB-T3 iguala al clásico sin crear vistas … No hace falta volver al Owner» (BM:94) → D-PBT3-14.
- **[OD] D-PB-05:** los pasos 4 a 7 («all fields OWNED by the full Push Back editor come from the user's draft»; «envelope/infrastructure fields NOT owned by that editor come from the current authoritative rack»; «unknown/future ExtensionData must not be silently destroyed»; «identity/schema/session metadata must not be rolled back from a stale draft») → D-PBT3-05, CT-PBT3A-06 y 07, CT-PBT3B-11. Los pasos 1 a 3 y OB-05.4 son de PB-T4 (OB-05.5).
- **[OD], el bloque sobre T1B** ([P] §7.3): «For D-PB-06 or another explicitly authorized behavior change: create a declared superseding characterization/change record. Do not silently weaken the old guard.» → D-1 y D-2.
- **F4B-E29, D-1** (2026-11-19, literal «Un solo código (Recomendado)»): la variante (A) → D-PBT3-09, D-PBT3-10, §5.3 (los seis `Skip` y E-1 a E-6), §6.2 y A-T3-1 (V41-01 y V41-03).
- **F4B-E24, la ExtensionData anidada** (2026-11-19, literal «Basta así (Recomendado)»): el panel mantiene el rechazo con aviso de AUTH-13 a todo campo desconocido a cualquier nivel, y el contrato de PB-T3 lo fija con una prueba → D-PBT3-13, CT-PBT3A-04 y CT-PBT3B-10. La pérdida silenciosa de RACKEDITAR queda declarada como limitación conocida anterior a F4B, sin tocar el clásico.
- **F4B-E25, las rutas `UNIT_CHANGE` en una corrección** (2026-11-19, literal «Regla F4B y reverificar (Recomendado)») → §6.0, con las tres condiciones y la excepción de las rutas `EXTERNAL` (V2-29).
- **F4B-E19, la Snapshot J** → precondición 6 y P-01 (V2-10).
- **D2** (F4B-E11), con su límite literal («This is NOT authorization for general shallow copying. … STOP and reopen L-1.») → D-PBT3-12 y PBT3x-C17.
- **F4B-E21** («La sesión copia; resto de F4B (Recomendado)») → §6.0.
- **F4B-E22:** las ratificaciones de M10 y de la mitad (a) de M14, «solo PB-T1b». Aquí no se ratifica nada: el texto de los contratos ya las prevé (INV-13, C13 y §6.0). La mitad (b), el `Trailer`, entra en la `ExpectedEvidence` (V2-32).
- **[DP]** (F4B-E03) → C08 y C11.
- **D-PB-10** («Each separately contracted PB task starts at: 1 / 3 … Attempt 3 always returns to Owner») → las tres subtareas en 1/3 y C12.

### 8.2 Decisiones materiales nuevas

**D-1. Qué pruebas de F4 se sustituyen para que el escritor renombre y borre** (antes del contrato de PB-T3b; no bloquea PB-T3a). **Es la única decisión material nueva, y está DECIDIDA** (V41-01 y V41-03).

**La respuesta del Owner, literal:** «Un solo código (Recomendado)». Es **F4B-E29** (2026-11-19; `f4b-dispositions.md:800` y `F4B-DRAFTS/coordinator/owner-pbt3-d1-decision.md`, custodiados en `d8ccbcb8`), sobre la pregunta de la v4, que va abajo como registro. Lo que implica, según F4B-E29:
- un único escritor para el piloto y el borrador completo: la variante (A), D-PBT3-09 y D-PBT3-10;
- PB-T3b sustituye seis pruebas de F4 (S-1 a S-6), cada una con su registro declarado y una prueba equivalente que se ejecuta;
- PB-T4 sustituirá tres de esos equivalentes al quitar el piloto: nueve en total por esta decisión.

**La variante (A′) no se eligió.** Lo que sigue de este apartado, la tabla, los porqués, la letra laxa y la pregunta, queda como registro de lo que se preguntó.

**Los hechos** (R-6; V2-02):
- **Renombrar exige S-1 y S-2,** con cualquier diseño. El único compositor del plan (I-54 G4B) tiene que llevar el nombre del borrador, y S-1 fija «the name is the one of the envelope that was re-read: never a value of the draft» (PrepareGuard:117-118). Componer en otro archivo rompería el censo cerrado de I-54. Y cambiar el `Name` del sobre releído antes de componer cumpliría la letra, pero debilitaría la premisa en silencio.
- **Lo demás depende de cómo se reparta el código:**

| | (A) Un solo código: la recomendada y la elegida (F4B-E29) | (A′) Código duplicado hasta PB-T4: no elegida |
|---|---|---|
| Qué es | El plan y la corrida del piloto se generalizan y sirven a los dos modelos (D-PBT3-09 y D-PBT3-10) | El texto del piloto no cambia, salvo la barrera; el modelo completo lleva su propio armado y su propia corrida privados en los mismos archivos (D-PBT3-09b) |
| Fallan | S-1, S-3, S-4 y S-5, y S-2 en dos de sus cinco filas (1 y 3) | S-1, S-4 y S-2 en su fila 1 (S-4 por sus cuentas: un solo `mutation.Dispose(` y un solo `scope.Dispose(` en todo UR, UpdateGuard:134-135) |
| Quedan vacías | S-6 y tres filas de S-2 (2, 4 y 5): siguen en verde, pero ya no comprueban nada (IntegrationGuard:902-910; PrepareGuard:127-139) | Cuatro filas de S-2 (2 a 5), por la misma razón (V3-04). S-2 se salta igual, entera |
| Se sustituyen ahora, en PB-T3b, con registro y equivalente | **Seis:** S-1 a S-6 | **Tres:** S-1, S-2 y S-4 |
| Después, en PB-T4, al quitar el piloto: se sustituyen con registro (V3-01 y V4-01) | **Tres:** E-3 (su cláusula «el piloto conserva `ViewUnavailablePrefix`»), E-5 («las dos `Execute`, que tienen que existir») y E-6, que cae con E-5 | **Seis:** S-3, S-5 y S-6 (el código del piloto que se va), E-1 (la sobrecarga de cuatro argumentos, que tiene que existir), E-2, que cae con E-1, y E-4 («por corrida», con las dos corridas) |
| En total, por D-1 (V4-02) | **Nueve** | **Nueve** |
| Código | Una implementación de PREPARE, MUTATE y POST | Dos, unas 350 líneas copiadas, hasta PB-T4 |
| Riesgo | El piloto pasa por código generalizado. Lo vigilan sus clases de comportamiento de F4 (más de 300 métodos) y PBT3B-C14 | Un arreglo puede aplicarse a una copia y no a la otra (R-T3-20) |

**Por qué vuelve al Owner:**
- [OD], el bloque sobre T1B, que [P] transcribe en su §7.3: «Do not silently weaken the old guard»;
- [P] OB-02.2: F4B no reescribe F4;
- la regla de [P] OB-T1B.3 para G-10 de F4: la lista exacta, antes del contrato.

La sesión no decide por analogía.

**Cómo se sustituye,** en (A) y en (A′), el patrón de I-39 y de OB-T1B.2:
- cada prueba queda con `Skip` y un motivo que nombra el registro y el número F4B-E de esta decisión;
- un equivalente que se ejecuta (E-n, CT-PBT3B-14) cubre todo lo que cubría y **empieza afirmando la forma nueva**, para no pasar en vacío (V2-04);
- la sesión escribe el registro en `F4B-DRAFTS/superseding-characterization.md`, en el commit de emisión de PB-T3b, con la prueba vieja, la nueva, la autoridad ([OD] fila de PB-T3, D-PB-04 y esta decisión) y el motivo;
- **la lista se mide sobre el texto del GREEN** (INV-PBT3B-15): cualquier otra diferencia es la parada C16 y vuelve al Owner.

**El retiro del piloto** en PB-T4 (A-T3-1) trae además sus propias sustituciones, las de las clases de comportamiento del piloto de F4. Son las mismas con (A) y con (A′), y no cuentan en la tabla (V4-02).

**Por qué se recomienda (A):** una sola implementación de la escritura, que es lo que protegen G-2 y G-7 de F4. Y (A′) no ahorra sustituciones por D-1: las dos suman nueve, y (A′) deja seis a PB-T4 en lugar de tres (V4-01). **A favor de (A′):** el texto del piloto, lo único que llega al panel antes de PB-T4, no cambia salvo la barrera, y PB-T3b sustituye tres en lugar de seis. **Contra (A):** sustituye tres pruebas más ahora y deja una vacía que hay que saltar igual.

**«Sustituir con registro», nunca «ajustar»** (V3-01): en F4B no se edita ninguna prueba existente (el invariante común 07; OB-T1B.1). En PB-T4, cada equivalente que deje de valer se sustituye con el mismo patrón: `Skip`, registro y un equivalente nuevo.

**La letra de los equivalentes: estricta, y por qué** (V4-01; O-01 de la recomprobación de la v3):
- **La que fija el paquete** (§5.3): E-1 (con (A′)), E-4 (con (A′)) y E-5 exigen que existan todos los miembros que nombran. Así cubren todo lo que cubría la prueba que sustituyen, que es la regla de la sustitución (OB-T1B.2): S-1, S-4 y S-5 fallan hoy si falta el miembro del piloto que nombran: S-1 busca el `ComposeMemberEnvelope` de cuatro parámetros (PrepareGuard:107); S-4, `private DraftApplyOutcome<PushBackPilotDraftState> Post(` (UpdateGuard:140); y S-5 lee el cuerpo de la `Execute` del piloto (IntegrationGuard:882, V41-04).
- **La laxa, «sobre cada uno que exista, y al menos uno»,** es posible: en PB-T3b los equivalentes no pasarían en vacío, porque los dos miembros existen. Al quitar el piloto, E-1, E-2, E-4, E-5 y E-6 seguirían en verde y con contenido:
  - con (A), PB-T4 sustituiría solo E-3, cuya cláusula «el piloto conserva `ViewUnavailablePrefix`» fallaría o, escrita también laxa, se quedaría vacía: **siete** en total;
  - con (A′), solo S-3, S-5 y S-6: **seis** en total.
  
  Entonces (A′) ahorraría **una** sustitución por D-1. Pero esos equivalentes cubrirían **menos** que las pruebas que sustituyen: ya no exigirían el código del piloto. Sería debilitar la guarda vieja, y solo cabe si el Owner lo acepta de forma explícita ([OD]: «Do not silently weaken the old guard»).
- **Por eso no es una opción más,** y la pregunta cuenta con la letra estricta. La descripción de la opción (A′) dice la cuenta laxa, para que el Owner vea la única lectura en que (A′) ahorra. El Owner eligió (A) sin pedir la letra laxa (F4B-E29): **el contrato lleva la letra estricta.**

**La pregunta, tal como se hizo** (registro; V3-01 y V4-01; la respuesta, arriba):
- **header** (10 caracteres): `Guardas F4`
- **question** (373 caracteres): «PB-T3b: para que el «Actualizar» del panel renombre y borre vistas, hay que sustituir pruebas de F4 que fijan el texto del escritor del piloto, cada una con registro y una prueba equivalente. Con un solo código para el piloto y el borrador completo son seis ahora y tres en PB-T4; con el borrador completo en código duplicado hasta PB-T4, tres ahora y seis en PB-T4. ¿Cuál?»
- **options** (la recomendada primero):

  | label | description |
  |---|---|
  | `Un solo código (Recomendado)` | Un único algoritmo de escritura para el piloto y el borrador completo. PB-T3b sustituye seis pruebas de F4 (cinco fallan y una quedaría vacía), cada una con su equivalente; al quitar el piloto, PB-T4 sustituirá tres de esos equivalentes. Nueve por esta decisión. |
  | `Código duplicado hasta PB-T4` | El texto del piloto no cambia, salvo la barrera, y el borrador completo va en código propio, copiado, hasta PB-T4. PB-T3b sustituye tres; al quitar el piloto, PB-T4 sustituirá seis (tres pruebas de F4 y tres equivalentes). Nueve por esta decisión. Solo con equivalentes más laxos, que no exijan el código del piloto, serían seis frente a siete. |
  | `Sin renombrar ni borrar` | No se sustituye ninguna. El panel no renombra ni borra vistas en PB-T3: esas dos cosas, que pide la fila de PB-T3, quedan pendientes. |
- **multiSelect:** no.
- **Nota (no va a la pregunta).** Las cuentas son las de esta decisión. El retiro del piloto en PB-T4 trae además las suyas, iguales en las dos opciones (A-T3-1; V4-02). La frase de la letra laxa va en la opción (A′), la única en que cambia la comparación (V4-01).

**D-2. La premisa «el escritor nunca borra» del Plugin cambia: registro declarado, sin `Skip`** (V2-11; I-10 de la revisión). **No es una decisión:** D-PB-04 autoriza borrar y [P] OB-04.1 ya prevé «una primitiva caller-owned y su guarda de fachada». Pero [OD] manda un registro declarado para todo cambio autorizado.
- Las guardas del escritor de F4 siguen en verde por su letra con la fachada de PB-T3c, y no se debilitan: siguen comprobando que **el escritor** no borra por sí mismo.
- **El registro:** una entrada en `F4B-DRAFTS/superseding-characterization.md`, **sin `Skip`**, que la sesión escribe en el commit de emisión de PB-T3c. Recoge:
  - la autoridad: [OD] D-PB-04 y [P] OB-04.1;
  - la guarda que sigue y por qué: UpdateGuard:182-212, con su comentario :188;
  - las guardas nuevas: CT-PBT3C-01 a 04;
  - los dos comentarios de WR que cambian: :26 y :225.

  Detalle en D-PBT3-11.
- La recomprobación de la v2 lo da por resuelto (I-10, su §10.2) y no lo lee como un debilitamiento. Si la comprobación acotada de PB-T3c lo leyera así, entraría en D-1 con sus métodos.

### 8.3 Lo que no es decisión

- **El origen borrado** (B-01 de la revisión; V2-01): POST relee con un origen reanclado. Es una regla técnica del escritor que iguala al clásico (la revisión, §9).
- **La división en tres subtareas:** OB-10.1 y D-PB-10 la prevén; el contenido, el orden y las dependencias de la fase no cambian. Se informa en la emisión. No se funden PB-T3b y PB-T3c (§4 de la revisión).
- **La regla del frontal B:** el valor por defecto de [P] §6.4 fila 3, que la revisión mantiene (su §2). La limitación del rack de un sentido con un frontal B, declarada (V2-20).
- **Los textos nuevos** de D-PBT3-09 y D-PBT3-10 (`BlockedPrefix`, el del frontal B, `AllViewsObsoleteReason` y `EraseUnsupportedReason`): si la revisión los considera una decisión material de producto, vuelven al Owner.
- **La barrera también en el piloto, con su único cambio visible en el panel** (V2-09): es la letra del [OD] («blocking authored write on diagnostics/errors») para «the Workspace authored writer». Iguala al clásico, que no deja salir un diseño bloqueado, y no cambia el corpus de PB-T1a (R-8).
- **Quitar el rechazo A/B del panel (OB-03.2) en PB-T4, no en PB-T3:** [P] lo prevé «en PB-T3 o PB-T4», y el panel no usa el estado completo hasta PB-T4 (A-3).
- **El asiento en el producto:** es la API que A-2 prevé «si PB-T3 o PB-T4 la necesitan». Interino respecto de la ventana (V2-22).
- **La capa bloqueada sobre una vista a borrar y la superviviente contada sin las borradas:** el panel es más estricto que el clásico, con la regla C-F4-05 de F4.
- **Las hojas derivadas del clasificador** (V2-12): se informan; cómo se presentan es de PB-T4.
- **El tipo del borrador del panel en PB-T4** (V42-04; I-04 de la revisión de compatibilidad): PB-T2 lo deja como `DraftSession<PushBackEditableState>` (HT2-21 y A2-4 de PB-T2), y PB-T3 como `DraftSession<PushBackFullDraftState>`, con `Origin`, que `ForWrite` y `KeepOn` necesitan. Es técnico y no cambia nada que se vea: **A2-4 de PB-T2 queda sustituido por A-T3-11.** El puerto de la ventana es una interfaz sobre `PushBackEditableState`, y PB-T4 la adapta al borrador completo.
- **A2-7 de PB-T2 queda sin objeto** (V42-11): solo valía con la opción (B) de Q-PBT2-2, y el Owner eligió (A) (F4B-E28): «Aplicar» rechaza un diseño bloqueado con la misma compuerta (`PushBackEditorOutputGate`) que la barrera de PREPARE (D-PBT3-09 (d2)). La barrera sigue haciendo falta: para el piloto, y para un rack que ya llega bloqueado desde el dibujo, porque el materializador no mira la compuerta.
- **Un nombre nulo que «Guardar en biblioteca» deja en `""`** (V42-09; L-PBT2-12 de PB-T2): «Aplicar» lo lleva al borrador. El escritor lo trata como en blanco y no renombra (D-PBT3-09 (f)); el clasificador, en cambio, informa `$.Name` `ChangedByUser`, de `null` a `""`, un cambio que no se escribe. **Se declara** para la presentación de PB-T4 (A-T3-3). Con espacios en los extremos, el renombrado es real e igual al del clásico (RPC:190). No es decisión.


## 9. Límites

En `pbt3-measurements.md`, R-11 y R-12. En resumen:
- **Nada se compiló ni se ejecutó.** Todas las cifras de la CI son previsiones.
- **El worktree no se leyó.** Todo se leyó por objetos git:
  - en `2f5d3dec`, que es el producto de PB-T1b del intento 1;
  - para R-0 y R-17 de la v2, en `126fd65f` y `a18ba345`, la corrección de PB-T1b, con el mismo producto;
  - para la v3, en `833ea6bf`, la HEAD de la rama (V3-07): PB-T1b CERRADA y PB-T1c EMITIDA, con `git diff 2f5d3dec 833ea6bf -- src` vacío. Las anclas de `src/` de este paquete valen igual en las dos;
  - para la v4.1, en `d8ccbcb8` (V41-01 a V41-05): F4B-E29 y F4B-E30, y `git diff 833ea6bf d8ccbcb8 -- src`, que solo añade una línea a W (`:168`, la costura inerte del RED de PB-T1c, `e5a21989`). Las anclas de W posteriores a `:167` suben una línea; las de PP, UR, WR, WHC, RPC y RCS no cambian;
  - para la v4.2, en `5ddeb3b4` (V42-01 a V42-12): PB-T1 cerrada (F4B-E31) y el GREEN de PB-T2a, sin cierre. Lo de PB-T2b y PB-T2c sale de su paquete (v2.5 y v2.6) y de sus contratos en seco, no de su código, que no existe todavía.
  
  Este paquete se vuelve a medir sobre el cierre de PB-T2c.
- **Las cifras de la CI de PB-T2 y de PB-T3** son previsiones de sus paquetes. La última medida es la del cierre de PB-T1c (`44463212627`).
- **No se volvió a medir con dotnet** el censo de las 27 propiedades de CT-PBT3A-09 (3): sale de leer los tres documentos en `5ddeb3b4`, y lo mide el Worker por reflexión.
- **La lista de D-1 sale de leer las guardas frente al diseño,** en (A) y en (A′), con lo que la revisión leyó método a método (su §6). **La mide el Worker sobre el texto del GREEN** (INV-PBT3B-15), y la parada C16 cubre lo que se escape. La comprobación acotada no puede medirla sobre el esqueleto, porque el RED deja el piloto sin tocar (V2-02).
- **No medido:**
  - cómo cuenta xUnit una `Theory` con `Skip` (lo fija la TRX del RED de PB-T3b);
  - el Deshacer real con borrado en el panel;
  - el frontal B junto a un frontal de A del mismo nombre;
  - si `Rich("pushback")` lo admite el piloto (CT-PBT3A-04 lo comprueba en su línea de control);
  - si el diseño de un sentido con `StructureOverrideA = 1` lo admite el lector del piloto sin que el resolver lance (CT-PBT3B-06 lo comprueba en su línea);
  - el estado de bloqueo de los diseños de las clases de F4 (va a la remedición).
- **Una incidencia de procedimiento, repetida** (R-12): en la v1, un `python -I -` con heredoc que solo imprimía `noop`. **Repite** la de la medición H-12 de PB-T1 (`I-64-f4b-pbt1-measurements.md:227`), así que **se devuelve al Coordinator como modo de fallo repetido** (V2-24), sin clasificarla por analogía. En la v2 y en la v3 no se usó ningún heredoc.
- **Una incidencia nueva de la misma clase, en la v4.1** (V41-06): una orden encadenó por error `python -I -c "pass"` delante de una lectura con `sed`. **Incumple la regla del encargo** («Python … nunca con `-c`»). No leyó ni escribió nada; su salida se descartó. **Es el mismo modo de fallo que el de la v1 y que H-12 de PB-T1** (Python en línea en lugar de un script escrito con la herramienta de archivos), así que **se devuelve al Coordinator como modo repetido**, sin clasificarlo por analogía.

## 10. Arrastres que salen de PB-T3

| Id | Qué | Va a |
|---|---|---|
| A-T3-1 | Conectar el modelo completo al panel: el tipo del borrador del anfitrión, el puerto generalizado, «Editar todo» y la retirada del materializador y del copiador del piloto (A-5 de PB-T1). **Al retirar el modelo del piloto, las pruebas de comportamiento del piloto de F4** (§4) dejarán de describir el panel: PB-T4 vuelve al Owner con su lista, como D-1. **Además, lo que D-1 deja a PB-T4** (V3-01 y V4-01), sustituido con registro y no ajustado: con (A), la elegida (F4B-E29), **E-3, E-5 y E-6**, si PB-T4 conserva `BuildCore` y `Execution<TState>` (con (A′), no elegida, habrían sido seis) | PB-T4 |
| A-T3-2 | OB-03.2: quitar el rechazo A/B del panel, con su prueba, al conectar el lector completo (A-3 de PB-T1) | PB-T4 |
| A-T3-3 | OB-05.3 a OB-05.5: presentar el clasificador (con las hojas derivadas, V2-12), la elección explícita y la confirmación, y conectar `KeepOn` en lugar de `KeepPilotEditsOn`. Las guardas «sin conectar» (CT-PBT3A-12 y CT-PBT3B-13) se cambian entonces con su registro. **Y el nombre en blanco** (V42-09): un nombre nulo que «Guardar en biblioteca» deja en `""` sale como `$.Name` `ChangedByUser`, aunque el escritor no renombra; PB-T4 lo presenta como el escritor, o lo filtra (§8.3) | PB-T4 |
| A-T3-4 | D2, condiciones 3 a 7 sobre el `DraftSession` del modelo completo (A-2); M11 (diseño nulo) y M15 (coste de `Equals` en `IsDirty`) de PB-T1b; el coste de un «Actualizar» completo (R-T3-07). **Y F4B-E30** (V41-05): tras ciertos gestos de la parte compuesta de la ventana, `LastComputation.Design` va una pasada por detrás; cuenta cuando el estado vuelva de la ventana al borrador | PB-T4 |
| A-T3-5 | Lo físico de §5.5 | PB-T5 (Smoke-PB) |
| A-T3-6 | **Nota para el paquete de la fase** (de la sesión, no del Owner): la línea ~314 de [P] («…se pierden en cada reescritura, clásica o del panel») se precisa: el escritor pierde lo anidado, pero el panel no llega a escribir esos racks (AUTH-13). La promesa de OB-05.2 de conservar la ExtensionData de nivel superior en el panel no se ejercita hoy, porque AUTH-13 también la rechaza; CT-PBT3B-11 la prueba solo por construcción | La sesión, en [P] |
| A-T3-8 | **El origen reanclado** (V2-01): tras borrar la vista de origen, el borrador queda anclado en otra vista. El panel tiene que releer con el origen del borrador, no con una selección que ya no existe (`ChosenReference`, WHC:328, :409, :675) | PB-T4 |
| A-T3-9 | **El asiento y la ventana** (V2-22 y V42-07): que la ventana use `PushBackEditableStateSeat.Pass` en lugar de su propia adopción en `Recompute`, para que quede una sola implementación. No puede ser en PB-T2: va antes de que exista el asiento y congela `Recompute` (INV-PBT2B-04 de PB-T2) | PB-T4 |
| A-T3-10 | **La dependencia `Pilot` ↔ `Draft`** (V2-27): las constantes de texto del piloto que usa `Draft/` pasan a `Draft/` al retirar el piloto | PB-T4 |
| A-T3-11 | **La ventana sobre el borrador completo** (V42-04; I-04 de la revisión de compatibilidad). **Sustituye a A2-4 de PB-T2.** PB-T4 conecta la ventana de WORKSPACE con un adaptador de `IPushBackDraftStatePort` sobre `DraftSession<PushBackFullDraftState>`:<br>• su `ApplyToDraft(edited)` hace `SetCurrentDraftState(new PushBackFullDraftState(edited, draft.CurrentDraftState.Origin))`, **con el `Origin` del estado actual**: el comparador mira `Origin` (D-PBT3-02), y otro `Origin` daría un `IsDirty` falso;<br>• el lanzamiento lleva `State = draft.CurrentDraftState.Editable` y saca `SourceProject` del sobre de `Origin`;<br>• `PushBackDraftSessionPort` y `LaunchFor` de PB-T2a, tipados sobre `PushBackEditableState`, no son el canal del panel: PB-T4 los conserva o los retira, con su registro.<br><br>D2 no cambia: el borrador copia con `PushBackFullDraftStateComparer.Copy`, que delega en `PushBackEditableStateComparer.Copy`. Con A2-1 de PB-T2 (el envoltorio del puerto en el host) | PB-T4 |

A-T3-7 de la v1 (el registro de la decisión de la ExtensionData) sale: es F4B-E24, y la precondición 8 exige su registro (V2-19).
