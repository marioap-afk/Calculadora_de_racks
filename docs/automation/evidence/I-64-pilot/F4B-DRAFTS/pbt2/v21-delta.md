# I-64 F4B — PB-T2: de la v2 a la v2.1 del paquete (hallazgo por hallazgo)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- la recomprobación de la v2 (`independent-review-v1.md` §10, **IR1-R**): **LISTO PARA EMITIR TRAS REMEDICIÓN, 0 BLOQUEANTES / 0 IMPORTANTES / 7 MENORES**;
- el mensaje de la sesión: aplicar N-01 a N-07; el texto de N-04 para Q-PBT2-1, con la pregunta por debajo de 400 caracteres; la HEAD `833ea6bf`.

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v21.md`, con el registro v2 → v2.1 (V21-01 a V21-07) delante del de v1 → v2;
- `pbt2-measurements.md`, con el apartado Q-7 nuevo y Q-6 puesto al día;
- este archivo.

**Resultado: los siete menores están aplicados, 0/0/7 → 0/0/0 en el texto.** No queda ninguno sin aplicar. En dos hay una variante, explicada abajo: N-01 (la ruta de la costura) y N-03 (el reparto de los gestos en ventanas). El diseño no cambia, ni las preguntas salvo el matiz de N-04, ni los recuentos de la CI.

## 1. Los siete menores

| Hallazgo | Qué decía IR1-R | Cambio en la v2.1 | V21 |
|---|---|---|---|
| **N-01** | «Guardar en biblioteca» conserva `SetName(NameBox.Text?.Trim())`. Con un nombre nulo o con espacios normaliza el estado de la ventana, así que «Cerrar» pregunta y «Aplicar» ensucia el borrador. IR1-R proponía declararlo como L-PBT2-12 y añadir a CT-PBT2B-04 una fila con la costura devolviendo nulo | **Declarado** como **L-PBT2-12** en el §8.3, informado al Owner en el §8.4, y dicho en D-PBT2-05 y D-PBT2-06 (2), que ahora valen «salvo que antes se haya guardado en la biblioteca». **CT-PBT2B-04** gana dos ventanas, `nombre-nulo-biblioteca` y `nombre-con-espacios-biblioteca`, con estos pasos y líneas: biblioteca → `nombre-ventana=[]` o `[PB  Uno]`; «Cerrar» con «No» → `avisos=1` y la pregunta literal; «Aplicar» → `llamadas=1 sucio=True` y el nombre del borrador; `Cerrar` → `descartes=1`. Son [T1c], porque dependen del `SyncName()` que PB-T1c añade en W:3504. **Variante:** la costura devuelve `ArchivoTemporal` y no `null`. `SaveLibrary_Click` hace `if (path == null) return;` en W:3499, antes de `SetName` en W:3504, así que con `null` no se normaliza nada y la fila no discriminaría (registro Q-7). No se elige la otra opción de IR1-R, «decir que solo vale sin biblioteca», porque la fila fija el comportamiento | V21-01 |
| **N-02** | Las líneas esperadas no eran del todo literales: «…» en las pasadas 2 y 3 de CT-PBT2B-03, y ` descartes=<n>` sin fijar en CT-02 a 05. Con Q-PBT2-1 (A), cerrar con algo sin aplicar pregunta | **Las dos opciones de IR1-R, a la vez.** (a) **La regla**, normativa, en el §5.0: la última línea de cada ventana se forma tras el `Cerrar` explícito del `try`, como en `PushBackDeclaredCorrection2MixedBoxTests.cs:94-96`. `n` cuenta los avisos de esa ventana. Con Q-PBT2-1 (A), vale 1 si y solo si al cerrar hay un módulo escenificado o, en WORKSPACE, algo sin aplicar. En CT-06, `avisos` es ese contador. Si una ventana da otro valor, el Worker para (C07). (b) **Los valores**, ventana por ventana, sacados de la regla y de los oráculos de cada gesto: un gesto cuyo original fija un cambio en la salida deja algo sin aplicar. CT-01, 02 y 07 dan 0. CT-03 da 0 en la pasada 1 y 1 en las ventanas 2a a 2d y en la pasada 3. CT-04 da 1 en la ventana principal (`error`), 0 en las de «aplicar sin editar» y 1 en las de N-01. CT-05 da 0 en las CLASSIC, 0 en `campo-en-error` (W:623-633), 1 en `calculo-invalido` y `bloqueante`, y 0 en una cuarta ventana, nueva, que acaba aplicada y recoge las huellas del BOM y la biblioteca. PB-T2c: CT-01 da 0; CT-02 da 1 en las dos ventanas de CT-PBTC-07 y 0 en la de «declarar B y aplicar»; CT-03 da 0. **CT-PBT2B-03** pasa a una lista literal, debajo de la tabla, con sus tres pasadas, sus seis ventanas y todas sus líneas. Solo se abrevian tres textos fijos, `<R>`, `<A>` y `<C>`, definidos ahí | V21-02 |
| **N-03** | La pasada 2 de CT-PBT2B-03 copiaba sobre `pb-01-base` gestos cuyos originales usan `pb-31-v2-modulos`, `pb-06-seguridad` y `pb-05-topes` | **Cada gesto va con el fixture de su original:** la ventana 2a, `pb-31-v2-modulos` con `M3`, para el configurador nulo y el confirmado (CT-PB0-12); la 2b, `pb-06-seguridad`, para la seguridad (i) de CT-PB0-13; la 2c, `pb-06-seguridad`, para la defensa (CT-PB0-15); la 2d, `pb-05-topes`, para los topes (CT-PB0-16). `SelectModule` viene de S0, y no hay que copiarlo (registro Q-7). **Variante:** el configurador nulo y el confirmado van en la misma ventana, el nulo primero, y no en dos como en el original. Así la ventana tiene un solo valor de `descartes`, y es seguro: la confirmación cambia la salida. La seguridad y la defensa van en ventanas separadas, para no encadenar gestos del mismo fixture que el original separa. «configurador-cancelado» se llama ahora `configurador-nulo`, como en el original | V21-03 |
| **N-04** | La frase «el primer aviso de cambios sin guardar de todo el editor» podía leerse como «el primer aviso del editor» | Con el texto de la sesión: «Avisar al cerrar de lo no aplicado sería el primer aviso sobre todo lo editado; hoy solo avisa de un módulo sin confirmar, y el código evita a propósito ese aviso general (ADR-0029 D8).». La segunda frase pierde «y «Cerrar» no pregunta salvo por un módulo sin confirmar», que la nueva ya dice. **La pregunta tiene 361 caracteres** (registro Q-6), por debajo de 400 | V21-04 |
| **N-05** | `F4B-PBT2C-WORKSPACE-CARRYOVER` tiene 29 caracteres, no 30 | Corregido en el §6.0 («26, 31 y 29») y en R-PBT2-05 | V21-05 |
| **N-06** | La HEAD ya es `833ea6bf` (F4B-E27, PB-T1c emitida), con el mismo `src` y los mismos `tests` | **La base pasa a `833ea6bf`.** `git diff --name-only 95373af6 833ea6bf -- src tests` sale vacío (registro Q-7). **F4B-E27** entra en el §1, con el contrato emitido de PB-T1c. [PT1] es la v2.8 de `833ea6bf`. Ya no dicen «no emitida» HT2-24, R-PBT2-09 ni el §9, ni la primera condición para emitir. D-PBT2-14 dice que CT-PBT1C-04 lleva dieciséis autopruebas, las quince comunes más la (d), propia de la ventana, y que las guardas de PB-T2, que no miran la ventana, llevan las quince. HT2-24 recoge que IR1-R comprobó sobre el texto emitido que nada impide reasignar `editable`. La remedición sobre el **cierre** de PB-T1c sigue pendiente, como dice el §6.0 | V21-06 |
| **N-07** | La atribución prevista de MU-T2C-03 incluía CT-PBT2B-06, que seguramente no falle | La previsión queda en CT-PBT2B-03 y 04, más CT-PBT2B-08 (R4) en Core. CT-06 sale: `appliedState` se sigue copiando de `editable`, así que `cerrar-tras-aplicar` no cambia. La tabla de atribución recoge lo que falle de verdad (§5.4) | V21-07 |

## 2. Lo que pidió la sesión

| Petición | Cambio |
|---|---|
| Aplicar N-01 a N-07 en una v2.1, con su registro de cambios v2 → v2.1 | El paquete v2.1, con el registro V21-01 a V21-07 al principio. El de v1 → v2 sigue debajo, como historia |
| Q-PBT2-1 con el texto de N-04, por debajo de 400 caracteres | V21-04: 361 caracteres, medidos con `check_questions_v2.py` (registro Q-6) |
| Dejar `v21-delta.md` al día | Este archivo |
| Las reglas de siempre: worktree prohibido; el repositorio solo por objetos git desde el checkout principal; solo lectura; escribir solo en `hs65\pbt2\` | Hecho. Todas las lecturas fueron `git show`, `grep`, `ls-tree`, `diff`, `log` y `rev-parse` sobre `833ea6bf`, `95373af6` y `a18ba345`. El único Python fue `check_questions_v2.py`, con `-I`, sin `-c` ni heredoc. No hubo ninguna incidencia nueva |

## 3. Efectos en cadena

- **Las listas cerradas de CT no cambian:** CT-PBT2A-01 a 04, CT-PBT2B-01 a 10 y CT-PBT2C-01 a 03 siguen iguales. Las filas nuevas de CT-PBT2B-04 y la cuarta ventana de CT-PBT2B-05 están dentro de sus métodos, así que los `MinSelected` y las líneas base previstas de la CI no se mueven.
- **El alcance no cambia:** las rutas permitidas y prohibidas, los invariantes, las paradas y los nombres de C13 siguen iguales. `ArchivoTemporal` sigue siendo la única escritura en disco, ahora también en CT-PBT2B-04.
- **Las cuatro preguntas** siguen siendo las únicas decisiones materiales, y solo cambia el texto de Q-PBT2-1. El §8.4 gana una entrada informativa: el nombre y la biblioteca.
- **Antes de emitir siguen pendientes:**
  - la remedición sobre el cierre de PB-T1c, con la comprobación de la reasignación de `editable` en los anclajes de PB-T2a;
  - las respuestas del Owner;
  - la comprobación acotada de cada subtarea.
