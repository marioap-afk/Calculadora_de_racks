# Disposición de la comprobación acotada de la v2 de I-64 A-14

**Comprobación:** `independent-review-a14-v2-check.md` (SHA-256 `48e0e0bf184b97ed1d13264dd99e7f620a36cbe8a593ba896619c75fed194f18`),
sobre la v2 (SHA-256 `65e252e2e24db490dbb5b5261f8dc48b6f428647cea9ebe16ad02cb33b1492e4`). Veredicto: EMITIBLE TRAS CORRECCIONES. De los 29
hallazgos del v1, 28 quedaron resueltos y 1 parcial (R2-02). Hallazgos nuevos: 0 bloqueantes, 5 importantes y 14 menores.

**Objeto corregido:** `I-64-A-14-draft-v2.1.md`. Las ubicaciones citan sus secciones.

**Recuento:** 20 filas (R2-02 y N-01 a N-19). **Aplicados: 20. Rechazados: 0.** Además se aplican la revisión de la firma (§6 de la
comprobación) y la lista de decisiones del redactor con su autoridad (§4 de la comprobación), como piden las instrucciones.

| Id | Severidad | Disposición | Dónde (v2.1) | Motivo |
|---|---|---|---|---|
| R2-02 (residuo) | Parcial → resuelto | Aplicado | Cabecera; C-12; §5; §12 | Los cuatro residuos: (1) `Applies-to` por N-01; (2) Smoke-PERF y ADR-0047 con regla y valor por defecto por N-05; (3) los OV de los tramos de editores por N-03; (4) bootstrap y Discovery delta por N-06 |
| N-01 | Importante | Aplicado | Cabecera (`Applies-to`, «Corrects / supersedes»); C-12 | `Applies-to: all`, con el precedente de las A-n de I-63 (`docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md:9`, y lo mismo en a2, a3 y a4: «Applies-to = all»). La cabecera declara que A-1..A-13 alcanzan a las unidades de los tramos 2 y siguientes («I-64» designa la iniciativa conceptual), sin reactivar ningún puente consumido. Se quita «o el contrato» (LIFECYCLE §6 `:198`, `:201`) |
| N-02 | Importante | Aplicado | C-12 (hitos, alternativa, corrección); §6 (fases 3 y 4) | Tramo 1 = fases 0 a 4, como piden las instrucciones; el tramo 2 queda solo con la fase 5; los tramos 2 y 3 dependen solo del 1, y la fase 6 ya no espera a la 5. La alternativa 0 a 3, el ejemplo literal del Owner, queda visible en la firma, con sus consecuencias: la fase 4 va en su propio tramo, el smoke de 3 y 4 se parte en dos y cambia la nota a ADR-0010. Se declara qué pasa si el Owner corrige los hitos: la regla no cambia y la tabla resultante va por la A-n siguiente (LIFECYCLE §8) |
| N-03 | Importante | Aplicado | C-12 (regla de OV por tramo); §5 (OV-A14-01..09 numerados; tabla por tramo) | Regla: cada tramo lleva los OV de lo que entrega por primera vez y, para cada sistema que lleva a la ventana, los OV de edición repetidos (borrador, Actualizar, Conflicto, UNDO, foco y teclado, resultados visibles, «Actualizar todo»). OV-07 y OV-22 van al último. Q-09: «tu validación» en cada hito; READY-08 (LIFECYCLE §8 `:245`). OV-A14-05 y 06 pasan a ser «por sistema» |
| N-04 | Importante | Aplicado | C-08 (tabla de OD `:22-49`); cabecera (Relation to F4B); §8; §11, materia (7) | Se sustituyen solo «as a modal/full-size editing surface» (`:25-26`), `:34` y `:37`. Se conservan `:22-24`, «loaded from the Workspace draft» (`:26`), `:29-33`, `:36` y `:39-49`, y `:38` hasta la 3b de Push Back. OB-01.4 cae con `:34`. Cada fila cita el texto literal (OD `:22-49`, comprobado) |
| N-05 | Importante | Aplicado | C-12 (Smoke-PERF y ADR-0047, con regla y valor por defecto); §12 (pendientes con su documento y su momento) | No quedan huecos sin dueño. Smoke-PERF: por defecto, §12.5 completo en el READY de cada tramo; una reducción solo por la A-n siguiente, antes del READY-01 del tramo, con el Coordinator y la conformidad del Owner. ADR-0047: por defecto sigue `propuesto` (`docs/adr/README.md:37`); el Owner decide en el READY de cada tramo y su decisión va al registro de cierre; las notas del anexo B solo con lo entregado (D-21 `:692-693`). Cada pendiente nombra su documento: la orden del paso 3, la orden de revisión del Architect, el paquete de la fase, la A-n, el bootstrap de la unidad o la orden de emisión |
| N-06 | Menor | Aplicado | C-12; R-08 | Bootstrap y Discovery delta por unidad (LIFECYCLE §2 `:60-62`, comprobado) |
| N-07 | Menor | Aplicado | C-03; §4 (§12.5) | Regla de precedencia (el delta propio de §4 y §5 prevalece); se añaden `:380` y `:659`; las tablas de §18 a §21 pasan a historia; se enumeran las pruebas de F3 retiradas (INV-02, INV-04, INV-09, INV-15, INV-38 y los escenarios 5 y 6). SP-03 conserva «incluidos cambios frecuentes de referencias (R-10)» (V5 `:909`, comprobado) |
| N-08 | Menor | Aplicado | §2, decisión 2; C-10; §11, materia (9); §12, fila 12 | Se marca como lectura de la sesión que el Owner confirma al firmar |
| N-09 | Menor | Aplicado | §5 (INV-A14-13, 14 y 15) | «4 (y 6, 7, 3b)» en las tres |
| N-10 | Menor | Aplicado | C-07; C-10, excepción 2 | De las dos opciones, la excepción 2 empieza en la fase 1 para la ruta de la biblioteca de bloques, que el menú ya guarda hoy en el mismo almacén (`RackMainMenuWindow.xaml:150-164`; `UserSettings.cs:13`). No es un significado persistido nuevo: el mismo campo, la misma acción del usuario. Va a la tabla de decisiones pendientes de autoridad (fila 13) |
| N-11 | Menor | Aplicado | C-01, criterio nuevo de la fase 5 | Acotado: salvo Variables (su A-n), la inserción desde la Biblioteca (flujo clásico hasta la A-n de la fase 8) y los subdiálogos de R-05 |
| N-12 | Menor | Aplicado | C-02 (vida del borrador); C-05, condición 3; INV-A14-08 e INV-A14-15 | «Dirty» sin «texto pendiente»: no se sale de un rack con texto pendiente, porque la frontera bloqueable lo compromete o aborta. Se añade «los borradores con cambios siguen vivos al cambiar» (replaneación §3). La condición 3 queda como **salvaguarda**, con el razonamiento: con K = 1, el cambio de documento y el ocultar dejan el texto en el rack que sigue abierto en su sesión, y no se conoce un camino que lo deje en un borrador no abierto |
| N-13 | Menor | Aplicado | R-08 | «La fase 1 es ligera; la 2 lleva F6 (Deep, V5 §13 `:982`, comprobado)» |
| N-14 | Menor | Aplicado | §10, paso 3; §13; Fuentes | Una sola lista en el paso 3: la enmienda, el registro del Owner, la replaneación, el prototipo redactado, las dos revisiones y sus dos disposiciones |
| N-15 | Menor | Aplicado | C-07 (`:80`); C-10 (`:79`) | Texto nuevo literal de los dos no-objetivos |
| N-16 | Menor | Aplicado | §1 | Se atribuye a [OD] («según [OD], F4B "extends … does not reopen F4"», OD `:20`), y se dice que la diferencia con D-15 punto 1 no se declaró entonces y A-14 la declara ahora |
| N-17 | Menor | Aplicado | §2, decisión 6; C-09; §11, materia (8); §12, filas 10 y 11 | Las dos deducciones quedan marcadas como deducciones de la sesión, para que el Owner las confirme al firmar. Además, la orden de emisión de cada fase rediseñada nombra el registro de cambios declarados fila por fila, como D-PB-06 nombró SUS-01 a SUS-04 (`superseding-characterization.md`), y lo que no nombre es STOP |
| N-18 | Menor | Aplicado | C-05 (resultado); OV-A14-06; §6 (fase 4) | «Progreso visible» entra en el resultado verificable |
| N-19 | Menor | Aplicado | C-01; cabecera (Amends) | `:393-394` se cita solo para RACKEDITAR; para las demás cláusulas, el motivo es que el brief es del Owner (`OWNER-RESERVED`) |

## La firma (§6 de la comprobación)

| Punto | Disposición | Dónde |
|---|---|---|
| IC-03 sustituida por Q-10 | Aplicado | §11, materia (2) |
| El alcance exacto de lo que se sustituye de D-PB-01 | Aplicado | §11, materia (7) |
| La lectura de «posición» y la deducción de los comandos de creación (y la de D13) | Aplicado | §11, materias (8) y (9) |
| Smoke-PERF por tramo | Aplicado: la firma recoge el valor por defecto | §11, materia (11) |
| Qué pasa si el Owner corrige los hitos | Aplicado: la elección de hitos es parte de la firma y una tabla nueva va por la A-n siguiente; rechazar otra materia impide la activación y la corrección es la A-n siguiente (precedente A-12 → A-13) | §11, materia (11) y párrafo siguiente |
| Comprensible para el Owner | Aplicado: frase exacta numerada por materias y, debajo, 14 puntos en lenguaje llano; la firma es sobre la frase | §11 |

## Las decisiones del redactor (§4 de la comprobación)

Recogidas en §12 de la v2.1, «Decisiones pendientes de autoridad»: 20 filas, cada una con quién decide y cuándo. La autoridad de cada fila
sigue la que propone la comprobación, salvo que §12 añade el momento. Las tres decisiones que la comprobación añadía a la lista (`Applies-to`,
la de D13 y el Smoke-PERF por tramo) son ahora las filas 3, 11 y 16. ADR-0047 es la fila 17 y las pruebas de F3, la fila 5.
