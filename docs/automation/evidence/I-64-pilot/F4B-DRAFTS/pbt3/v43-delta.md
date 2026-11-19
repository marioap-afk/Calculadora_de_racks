> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B PB-T3 — v43-delta: tres decisiones del Owner, la remedición hasta PB-T2b y las lecciones de las lecturas

**Qué es:** los cambios de la v4.3, con sus ids V43-xx, por el mensaje de la sesión. Todo se leyó por objetos git desde el checkout principal, en `05beabfd`, con PB-T2c en curso (P-08). El worktree no se tocó.

**Resultado:**
- **La v4.3 es solo de texto:** no cambian el diseño ni D-1.
- **Lleva las tres decisiones de hoy:**
  - F4B-E35: la reparación del RED en los tres contratos y la emisión condicional de PB-T3;
  - F4B-E37: la Snapshot K;
  - F4B-E34: el sustituto de nc2 adaptado a F4B.
- **La remedición, hasta el GREEN de PB-T2b:**
  - las anclas de W, medidas;
  - la base y la línea base de la CI, previstas.
- **Las lecciones de F4B-E30, F4B-E33 y la §5.0 de PB-T2:**
  - cambian ocho recetas de CT;
  - una de ellas era un error real: CT-PBT3B-06 (iii) daba `NothingToApply` en `pb-02-modulos`, en lugar de `Ready`.
- **Ninguna decisión nueva del Owner.**

| Id | Origen | Cambio | Dónde |
|---|---|---|---|
| V43-01 | F4B-E35, pregunta 4 | **La regla RR (RR-1 a RR-7) en el §6.0,** para las tres subtareas, con las dos lecturas de la v2.7 de PB-T2 (V27-01), porque los tres RED ya tocan `src/`:<br>• RR-1: «antes del commit GREEN»;<br>• RR-6, primer tramo: «el RED de cada contrato».<br><br>**En PB-T3b,** los tres archivos de guardas de F4 con los `Skip` los cambió el RED, no los añadió: una reparación no los toca | §6.0 |
| V43-02 | F4B-E35 | **PBT3x-C07** con la excepción literal de PB-T2c, tras el texto propio de PB-T3, y **PBT3x-C18,** nueva, literal | §6.0, «Las propias comunes» |
| V43-03 | F4B-E35, RR-6 | **La cadena** en INV-PBT3A-02, INV-PBT3B-02 e INV-PBT3C-02, con lo que el tramo `RedSha..RedRepairSha` no toca en cada una | §6.1 a §6.3 |
| V43-04 | F4B-E35, RR-4 y RR-5 | **La CI del RED reparado** (da lo mismo que la del RED) y **la `ExpectedEvidence`:**<br>• el commit «RED repair», con su CI y sus TRX;<br>• los filtros en local sobre `RedRepairSha`;<br>• la declaración de la entrega;<br>• el `Trailer` de los tres commits | §6.0; §6.1 |
| V43-05 | F4B-E35, RR-7 | **La lectura de la verificación:**<br>• n.º 10: el RED con `RedSha`, y `RT` ∪ `ChainRedFiles` desde `RedRepairSha`;<br>• n.º 11: el trailer, también en el commit «RED repair»;<br>• las comprobaciones de RR, que son C18;<br>• lo propio de cada subtarea: el tramo no toca `src/` en PB-T3a; ni PP, UR, la interfaz o los `Skip` en PB-T3b; ni WR o la fachada en PB-T3c | §6.0; §6.1 |
| V43-06 | F4B-E35, RR-3 | **La ayuda mecánica,** con una **tabla cerrada de literales del paquete por clase** de las tres subtareas: es C07 si uno cambia entre `RedSha` y `RedRepairSha`. Los indicios heurísticos van a la revisión | §6.0 |
| V43-07 | F4B-E35, pregunta 3 | **La precondición 10, cumplida con condición:** las tres se emiten en orden, cada una tras el cierre de la anterior, con su comprobación acotada a 0/0 y la remedición sin cambio material. Vuelve al Owner por una decisión nueva, un control con una firma no prevista, un intento 3 o un efecto material sin medir. Resuelve DU-1 | Precondición 10; §1; §8.1 |
| V43-08 | F4B-E37 | **P-01:** «today Snapshot K, docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json»; la autoridad, `owner-config-snapshot-k-decision.md`, custodiada en `56cb026f` | Precondición 6; §1; §6.0; §8.1 |
| V43-09 | F4B-E34 | **PBT3x-C11, literal:**<br>• nc2 → `PBT3x-NC2-EXACT-WHITELIST-F4B-01`, una copia de `PBT2A-NC2-EXACT-WHITELIST-F4B-01` (su ruta y su SHA-256 `6ccaf12d…ffd3`) con solo el identificador y el ámbito cambiados, solo con la firma exacta de PD-F4-01, y revisado antes de reutilizarlo si cambió `inject_opaque.py`;<br>• nc1, igual: PD-F4-04 exacta y su sustituto de F4;<br>• las aceptaciones puntuales de nc1 (PB-TC, PB-TC2, PB-T2a y PB-T2b) no se extienden | §6.0; §1 |
| V43-10 | La remedición | **El estado:** PB-T2a y PB-T2b CERRADAS (F4B-E34 y F4B-E38), PB-T2c EMITIDA (F4B-E39).<br>**Las anclas de W tras el GREEN de PB-T2b** (`03b16c88`):<br>• la adopción, W:624-669, y su `catch`, :627-635;<br>• el `Recompute();` final de `LoadFromModel`, :404;<br>• los ensambladores, :150-151;<br>• la costura, :197.<br><br>Los cuerpos de `LoadFromModel` y `Recompute` son iguales a los de `161a0dad`. **Previsto:**<br>• la base, el cierre de PB-T2c, con el `src` del de PB-T2b;<br>• la línea base, Core 19 094 y UI 2 450;<br>• el GREEN de PB-T3a, 19 107 y 2 451 | Cabecera; precondiciones 1 y 2; D-PBT3-03; §5.2; §1; §9 |
| V43-11 | F4B-E30, F4B-E33 y la §5.0 de PB-T2 | **§5.0 gana dos reglas:** «El momento de cada lectura» y «Los gestos que no cambian nada», con los valores medidos en los 17 fixtures (`PalletTolerance` 4, `BeamDepth` 6, `Structure.PostPeralte` nulo, y la escala de cada uno). **Las recetas que cambian:**<br>• CT-PBT3A-07 y CT-PBT3B-15: Dirty con la tolerancia a 4.5;<br>• CT-PBT3A-09: 4.5, 4, y 3 y 4;<br>• CT-PBT3B-01 (i): 4.5 y 4;<br>• **CT-PBT3B-06 (iii):** 2,5 en `pb-02-modulos`, que ya tiene 2;<br>• CT-PBT3B-07 (ii): 3,5 si ya tiene 3, con `IsDirty` justo antes, y las lecturas del borrador tras la corrida;<br>• CT-PBT3B-10: ` dirty=True` en la autocomprobación;<br>• CT-PBT3B-12: las referencias se toman antes del `Prepare` | §5.0; §5.2; §5.3 |
| V43-12 | La sesión | `Objective` «v4.3» en los tres contratos; la custodia gana la v4.3 y `v43-delta.md` | Cabecera; precondición 9; §6.1 a §6.3 |
| V43-13 | Del redactor | **Una incidencia de procedimiento repetida:** una orden encadenó `python -I -c 1`, que la regla prohíbe. No leyó ni escribió nada. Es la tercera vez del mismo modo (la v1, la v4.1 y esta): **se devuelve al Coordinator como modo repetido** | §9 |

## Lo que el generador de PB-T3a tiene que cambiar

1. **La versión:**
   - `PKG_VERSION = 'v4.3'`;
   - `CUSTODY_SHA256` de la v4.3, que es `e7c4ab966173b35b9bfee537485c8ad7ffd15925369de2efaa345b8af20bc226` (241 746 bytes, LF) si la v4.3 no cambia en su comprobación acotada;
   - `OBJECTIVE`: «…PB-T3 v4.3…».
2. **P-01 y la Snapshot K:**
   - `SNAP_J` pasa a `snapshot-K/snapshot-k.json`, y P-01 dice «today Snapshot K»;
   - la autoridad pasa de `owner-config-snapshot-j-decision.md` (F4B-E19) a `owner-config-snapshot-k-decision.md` (F4B-E37);
   - como el generador de PB-T2c (V29-06), comprobar el SHA-256 de K y su sonda neutra;
   - el aviso D1 y el `IssuedBy` dicen K.
3. **Las paradas:**
   - el texto de C07 gana la excepción de F4B-E35, literal;
   - entra C18, literal: `STOP_IDS` pasa a 16;
   - **C11 ya no sale de PB-T1c** (`from_t1c('C11')`): sale del texto literal del paquete (§6.0, el bloque citado de PBT3x-C11), con el prefijo. El `assert` de la línea 1252 pasa a buscar `PBT3A-NC2-EXACT-WHITELIST-F4B-01`, `SATISFIED_UNDER_PBT3A_EXACT_WHITELIST_SUBSTITUTE_F4B`, la ruta y el SHA-256 del sustituto de F4B-E34, y el final «no one-off acceptance extends to PB-T3a». Conviene comprobar en la revisión que el sustituto existe con ese SHA-256.
4. **Los invariantes y la `ExpectedEvidence`:**
   - INV-PBT3A-02 gana la cadena RR-6;
   - la `ExpectedEvidence` de §6.1 cambia en cinco viñetas y pasa a ser la del paquete (la lectura de la verificación, la CI del RED reparado, los filtros sobre `RedRepairSha`, la entrega y el `Trailer`);
   - el `assert` de la línea 1037 sigue valiendo.
5. **Las autoridades:**
   - F4B-E35 (pregunta 4, con el resumen de RR-7 como en PB-T2c; pregunta 3, como autoridad de emisión);
   - F4B-E34 y el directorio del sustituto;
   - F4B-E37;
   - F4B-E38 y F4B-E39;
   - el contrato emitido de PB-T2c, de donde salen los textos de C07 y C18.
6. **La emisión:**
   - `ISSUE_AUTH = ('F4B-E35', 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md')`, pregunta 3;
   - el `IssuedBy` cita la condición;
   - DU-1 queda resuelta.
7. **La lista cerrada de RR-3 para el relevo:** la sesión la escribe por clase desde la tabla del §6.0 (la M-05 de la §12 de CA-T2b). El generador puede comprobar que la tabla nombra las seis clases nuevas de PB-T3a y ninguna otra.
8. **Las notas que descarta:**
   - el §6.0 pasa de 3 a **6** notas;
   - el §6.1 sigue con 3;
   - las nuevas van con sangría 2 (la de C07 y C18, la del C11 y la de la tabla de RR-3), y la del `ForbiddenWriteScope` con 4;
   - la tabla de RR-3 y el bloque citado de C11 son texto del paquete, no viñetas, y el lector de `bullets()` tiene que saltarlos o leerlos aparte.
9. **La remedición, con `--pt2c-close`:** las anclas de W por contenido, sobre la `BaseSha` (previstas en las líneas de V43-10), y la línea base `CI_FORECAST = (19094, 2450, 21)`, como en la v4.2.
10. **La custodia:**
    - `CUSTODY_EXTRA` gana `v43-delta.md`;
    - `CUSTODY_HEADS` de la revisión, con la §13;
    - el texto de la custodia del paquete nombra el registro hasta V43-13.

## Las dudas

- **DU-A. La lista de RR-3 lleva también los valores de los gestos** (4.5, 4, 3 y 4, «PB-NUEVO», «PB-EXACTO», las escalas). PB-T2 fijó solo líneas esperadas. Lo hago porque cambiar un valor cambia lo que se prueba, y RR-3 solo admite el cuándo y el cómo. Si la sesión prefiere la forma de PB-T2, la tabla se recorta sin cambiar nada más.
- **DU-B. El contrato emitido de PB-T2c sigue nombrando el sustituto de F4** (`PBT2C-NC2-EXACT-WHITELIST-01`). El «Alcance» de F4B-E34 dice que en el resto de F4B se reutiliza el adaptado. No es de este paquete, pero si nc2 de PB-T2c da la firma de PD-F4-01, el sustituto de F4 fallará como en PB-T2a: la sesión decide si eso es «un control con una firma no prevista».
- **DU-C. La escala del escaneo de CT-PBT3B-10** sale de `I58F1Fixtures.Rich("pushback")`, que no fija la escala. La receta dice «2,0, o 2,5 si ya es 2», y la autocomprobación ` dirty=True` lo cubre.
- **DU-D. Los valores de `BeamDepth` (4) y `PostPeralte` (3 y 4)** salen de valores que ya usan las pruebas y los fixtures, no de una corrida. Si el ensamblador los normalizara, la línea lo diría, y es C07.
- **DU-E. El cuerpo de `ReadInputs`** no lo comparé: solo `LoadFromModel` y `Recompute`, que son iguales a los de `161a0dad`. La remedición lo comprueba, como dice la precondición 2.
- **DU-F. La incidencia V43-13** es la tercera del mismo modo. Va al Coordinator antes de clasificarla.
