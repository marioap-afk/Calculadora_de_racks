> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B PB-T3 — v42-delta: la compatibilidad con PB-T2, hallazgo a hallazgo

**Qué es:** el cambio de la v4.2 para cada hallazgo de la revisión de compatibilidad (`compat-pbt3-vs-pbt2.md`: COMPATIBLE CON REMEDICIÓN, B/I/M 0/5/7, sin decisiones nuevas del Owner), con sus ids V42-xx. Todo se leyó por objetos git desde el checkout principal, en `5ddeb3b4`. El worktree no se tocó.

**Resultado:**
- **Los doce hallazgos se aplican:** I-01 a I-04, M-01 a M-07, y la parte de I-05 que es del paquete. Lo del generador de I-05 queda para otro agente.
- **Y el menor de la recomprobación de la v4.2** (§13 de `independent-review-v1.md`: LISTO PARA EMITIR TRAS REMEDICIÓN, 0/0/1), Q-01, en el mismo archivo de la v4.2 (V42-13). La §13 valida las dos elecciones, deja la fila por defecto del clasificador como corrección del paquete y da por buena la declaración de M-05.
- **La v4.2 es solo de texto:** no cambian el diseño ni D-1.
  - Dos CT ganan líneas esperadas: CT-PBT3B-01 (ii) y CT-PBT3A-09 (3). Pasan por la comprobación acotada.
  - La tabla de grupos de D-PBT3-07 gana una fila por defecto. Hoy no se alcanza.
  - Ningún recuento cambia, y el GREEN previsto de PB-T3a es el del informe: 19 107 y 2 451.
- **Ninguna decisión nueva del Owner.** Queda pendiente una que no sale de aquí: la autorización de emisión de PB-T3a (precondición 10).

| Hallazgo | Grav. | Cambio | Id | Dónde |
|---|---|---|---|---|
| **I-01** La base, atada al cierre de PB-T1c | I | **La `BaseSha` de PB-T3a es el cierre de PB-T2c.**<br>• Precondición 1: PB-T1 cerrada (F4B-E31) y PB-T2 delante, con su estado en `5ddeb3b4`.<br>• §6.0: el diff de `src` de PB-T3a, frente al cierre de PB-T2c. Referencias: Application = cierre de PB-T1b + los dos archivos de PB-T2a; Domain y Plugin = cierre de PB-T1b; UI = cierre de PB-T2b, con WC del cierre de PB-T1c.<br>• Línea base de la CI, **prevista**: Core 19 094 y UI 2 450 (2 429 / 21). GREEN previsto: **19 107 y 2 451**. En cadena, PB-T3b 19 128 y PB-T3c 19 133 | V42-01 | Cabecera; precondiciones 1 y 2; §1; §6.0 a §6.3 |
| **I-02** A2-11 no está en PB-T3 | I | **CT-PBT3B-01 (ii)**, leída del **sobre que sale**:<br>• las once filas de `SC1.FilasF()`, copiadas como lista literal, con el valor de CT-PBTC-02 (i) (el exacto en diez filas, `6` en F4);<br>• las dos filas de estilo, `pb-06-seguridad` STANDARD-ISO y `pb-07-combinado` SIMPLEX, con el `DimensionStyle` del diseño del sobre y el del sistema de `BuildForMember`.<br><br>§5.6 gana la fila de OB-06.3 y A2-11 | V42-02 | §5.3; §5.6 |
| **I-03** A2-12 (M09) no está en PB-T3; el clasificador sin regla por defecto | I | **CT-PBT3A-09 (3)**, por reflexión: las 27 propiedades declaradas de `RackEmbedDocument`, `RackProjectDocument` y `PushBackDesignDocument` (8, 9 y 10), cada una con su fila en el mapa y su dueño, literal; ninguna con `fila=<ninguna>`.<br><br>**La fila por defecto** de la carga útil en D-PBT3-07: `Rack` si el mapa da `Editor`; si no, `OutsideEditor` | V42-03 | D-PBT3-05; D-PBT3-07; §5.2; §5.6 |
| **I-04** Dos arrastres a PB-T4 que se contradicen | I | **A-T3-11:**<br>• el adaptador de `IPushBackDraftStatePort` sobre `DraftSession<PushBackFullDraftState>`, con `SetCurrentDraftState(new PushBackFullDraftState(edited, draft.CurrentDraftState.Origin))`;<br>• el lanzamiento con `Editable` y el `SourceProject` del sobre de `Origin`;<br>• `PushBackDraftSessionPort` y `LaunchFor`, a decidir por PB-T4, con registro.<br><br>**A2-4 de PB-T2 queda sustituido.** Una frase en §8.3, y la ida y vuelta con el asiento en D-PBT3-03 | V42-04 | D-PBT3-03; §8.3; §10 |
| M-01 Las clases de PB-T2, fuera del `ForbiddenWriteScope` | M | §6.0 las añade, tomadas del contrato de PB-T2c (las nueve, nombradas). Previsto, a medir: en PB-T3a, Core 29 y UI 68; 89 en `src/`; 203 en total | V42-05 | §6.0 |
| M-02 La red no incluye PB-T2 | M | Las ocho clases de PB-T2, sin `ExpectRed`, con los mínimos de su paquete:<br>• Core: 3, 1, 1 y 1;<br>• UI: 3, 4, 1 y 3.<br><br>Van en §6.1 (y por remisión en PB-T3b y PB-T3c), en §5.1 y en §4. La contención de F4B-E30, también sobre esas clases | V42-06 | §4; §5.1; §6.0; §6.1 |
| M-03 A-T3-9 a «PB-T2 o PB-T4» | M | Solo **PB-T4** | V42-07 | D-PBT3-03; §10 |
| M-04 El orden y el estado viejos | M | PB-T2 delante, F4B-E31 y el estado de PB-T2 en `5ddeb3b4`, en la cabecera, las precondiciones 1 y 7 y R-T3-09 | V42-08 | Cabecera; precondiciones; §7 |
| M-05 El nombre nulo pasa a `""` | M | **Se declara** para la presentación de PB-T4 (A-T3-3). El clasificador no se cambia, porque la v4.2 no toca el diseño | V42-09 | §8.3; §10 |
| M-06 Las anclas de W desplazadas | M | **Por su contenido:**<br>• la adopción de `Recompute`, de `computation = assembler.BuildFrom(` a `RefreshModuleSelector();`;<br>• su `catch (Exception error)`;<br>• el `Recompute();` final de `LoadFromModel`;<br>• los dos ensambladores;<br>• la costura.<br><br>Sus líneas, con su historia (cierre de PB-T1b, `d8ccbcb8` y `5ddeb3b4`), van en notas y a la remedición tras el cierre de PB-T2c | V42-10 | Precondición 2; D-PBT3-03; §5.2; §6.0; §10 |
| M-07 A2-7, sin objeto | M | Una frase en §8.3: la misma compuerta en «Aplicar» y en PREPARE, y la barrera, todavía necesaria. F4B-E28 en §1 | V42-11 | §1; §8.3 |
| I-05, la parte del paquete | I | **Precondición 9, la custodia** en la emisión de PB-T3a:<br>• el paquete v4.2;<br>• `v2-delta.md` a `v42-delta.md`;<br>• la revisión con sus §1 a §13 (la §13, por V42-13);<br>• la de compatibilidad, con `compat_measure.py`;<br>• el registro.<br><br>**Precondición 10:** la autorización de emisión, pendiente del Owner | V42-12 | Precondiciones 9 y 10 |
| **Q-01** (§13) La precondición 9 custodiaba la revisión «con sus §1 a §12» | M | **«Con sus §1 a §13»** (la §12, sobre la v4; la §13, sobre la v4.2). La cabecera nombra la §13 entre las revisiones, y la fila V42-12 remite a V42-13. En el mismo archivo de la v4.2, que aún no está custodiado | V42-13 | Cabecera; precondición 9 |

## Las dos elecciones

**I-02: CT-PBT3B-01 (ii), y no CT-PBT3A-06.**
- El sobre que sale solo existe en el plan de PB-T3b: es el `EnvelopeText` de cada miembro.
- `ForWrite`, en PB-T3a, termina en el editor y las entradas. CT-PBT3A-06 compara su diseño con el de `Pass`, y los dos llevarían el exacto, así que no probaría que llega a lo que se escribe.
- MUTATE escribe el texto del plan y dibuja el sistema de `BuildForMember`: leer los dos del plan es leer lo que se escribe y se dibuja.
- No cambia ningún recuento.
- **Una precisión:** `SC1` está en `RackCad.UI.Tests`, y Core no la ve. Las filas, `Poner` y `Num` se copian como lista literal en la prueba, como pide §5.0.

**I-03: la prueba por reflexión, con la fila por defecto para el clasificador.**
- M09 es un riesgo de futuro: una propiedad que se añada caería en la rama de ExtensionData de `Of`. La equivalencia declarada solo describe el hoy, y la prueba avisa.
- Usa solo API pública de PB-T1b (`Table` y `Of`), y va como parte de CT-09: no toca la API cerrada ni los recuentos.
- La mitad del clasificador no se puede probar por reflexión sin exponer su función de grupos, que sería un miembro nuevo de la API cerrada, y eso es diseño. Por eso va la fila por defecto, que decide por el mismo mapa que la prueba ata a los documentos.

## Lo que el generador de PB-T3a tiene que cambiar

Lo hace otro agente. Va sobre la §4 de la revisión de compatibilidad, puesta al día para la v4.2:
1. **La versión:**
   - `PKG_VERSION = 'v4.2'`;
   - `CUSTODY_SHA256` del paquete v4.2;
   - los textos que dicen «v4».
2. **`OBJECTIVE`:** «…segun el paquete del contrato de PB-T3 **v4.2**…». Es el único cambio del Objective de §6.1.
3. **Las notas del §6.0:** pasan de 2 a **3**, por la nota nueva de M-01, con sangría 4 en el `ForbiddenWriteScope`. `bullets()` la descarta; el `assert` del número cambia. En §6.1 también hay una nota más, con sangría 2, bajo la línea base de la CI.
4. **La custodia:**
   - **`CUSTODY`:** el paquete, con su registro hasta V42-13; la revisión, con sus §12 y §13;
   - **`CUSTODY_HEADS`:** la revisión gana `'## §12. '` y `'## §13. '` (V42-13);
   - **`CUSTODY_EXTRA`:** gana `v41-delta.md`, `v42-delta.md`, `compat-pbt3-vs-pbt2.md` y `compat_measure.py`.
5. **Las anclas de W, por contenido:** el `catch` como bloque propio y el `Recompute();` final de `LoadFromModel`. Se comprueban sobre la `BaseSha`, no frente a una línea del paquete, que ya no las da como ancla normativa.
6. **La base, por PB-T2:**
   - las referencias del anclaje (GEN:183-188), las de §6.0;
   - `PT1C_CLOSE` pasa a ser el cierre de PB-T2c;
   - `CI_FORECAST = (19094, 2450, 21)`, y el GREEN, 19 107 y 2 451;
   - el `assert` que ata la línea de la CI pasa al contrato de PB-T2c;
   - las autoridades: el cierre de PB-T2c y su `task-close-package.md`, F4B-E28 y F4B-E31.
7. **Los alcances y la red:**
   - las listas de `tests/` y los mínimos que no bajan, del contrato de PB-T2c y su `AllowedWriteScope` de pruebas (Core 29 y UI 68 previstos);
   - `REQ_SPEC`, con las 8 filas de PB-T2;
   - la contención de F4B-E30, también sobre esas clases de UI.
8. **La variante:** una comprobación en negativo de que ningún texto copiado contiene `(A')`.
9. **Nada nuevo por I-02 ni I-03:** el generador no copia líneas esperadas de CT, y los `MinSelected` no cambian.

## Las dudas

- **DU-A. La autorización de emisión de PB-T3a** (DU-1 del generador) sigue pendiente del Owner. La v4.2 la deja como precondición 10, sin decidirla.
- **DU-B. La fila por defecto de D-PBT3-07** es una regla nueva del clasificador, aunque hoy no se alcanza. La trato como corrección, porque la revisión la ofrece como salida de A2-12. Si la sesión la lee como cambio de diseño, la alternativa es dejar solo la prueba por reflexión y declarar la limitación del clasificador.
- **DU-C. M-05:** elegí declarar, y no alinear el clasificador con el escritor, porque la v4.2 no toca el diseño. Alinearlo, de modo que el `$.Name` en blanco valga el del sobre de `Origin`, cambiaría CT-PBT3A-09 y su oráculo.
- **DU-D. Los recuentos de PB-T2** (Core 29, UI 68, 89/98/56 en `src/`, 203) y los mínimos de sus clases son previsiones de la revisión de compatibilidad. Los fija el generador sobre el cierre de PB-T2c.
- **DU-E. Las 27 propiedades de CT-PBT3A-09 (3)** salen de leer los tres documentos en `5ddeb3b4`, no de una reflexión ejecutada. Si PB-T2 o la remedición encuentran otra, cambia la lista literal: pasa por la comprobación acotada.
- **DU-F. La revisión de compatibilidad declara una incidencia propia** (escribió en una carpeta temporal del sistema). No es de este paquete; la sesión decide si la registra.
