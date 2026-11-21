# Encargo de la comprobación acotada de PB-T4c (CA-T4c) (enviado por la sesión al revisor del paquete de PB-T4 como mensaje; texto exacto)

## 1. CA-T4c, sobre el contrato en seco y la v5.0 (2026-11-21T17:48:38.955Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T4c (CA-T4c)** sobre la v5.0 del paquete de PB-T4. Va como **§22** de tu `independent-review-v1.md`, porque la §21 ya es la recomprobación de la v4.9. Las reglas de lectura son las de siempre:
- solo objetos git en `6adff90d856026a7a51067c1e9d7248f3d81641c`, el cierre de PB-T4b (F4B-E53);
- nada en el worktree, y sin compilar;
- Python con `-I` y guiones `ir4_`.

Las medidas las hizo el autor en copias exportadas; tú las compruebas por texto y por sus salidas.

**Las entradas** (en `hs65\pbt4\`, salvo que diga otra cosa):
- **El paquete,** `I-64-f4b-pbt4-gate-contract-package-v50.md` (`d19dd830…`), con su cabecera V50-nn y el anexo V50-A. Compáralo con la v4.9 (`043f196d…`), que es la que revisaste en la §20 y la §21. Lo de PB-T4a y PB-T4b no cambia, y el guion `make_v50.py` lo comprueba.
- **Las decisiones del Owner de PB-T4c:** `owner-pbt4c-list-3-decision.md` y `owner-pbt4c-list-4-decision.md`. La de la emisión condicional es `owner-pbt4-decisions.md`, pregunta 4.
- **Las decisiones de la sesión de PB-T4c:** `session-pbt4c-design-decisions.md`, con D-S7 (la costura de CT-09 con `HeaderConfiguratorDialog`), D-S8 (los filtros por clase exacta), D-S9 (`IWorkspaceFullEditHost`) y D-S10 (CT-09 solo con los casos alcanzables).
- **La lista,** `pbt4c-substitution-list-4.tsv` (`9bd4c17a…`), con la de 3 como historia.
- **Las piezas:** `pbt4c-green-v50.txt`, `pbt4c-skeleton-v50.txt`, `green_pbt4c_v50.py`, los guiones `*_v50.py` (los `Skip`, los equivalentes, `apply_red` y `apply_green`, `check_skips`, las sondas y el paseo) y `pbt4c_ct_table_v50.py`.
- **La precondición 6:** `hs65\pbt4ccc\compile\pbt4c-precondition6.json` (`0cb67aee…`) y `compile-check.md` (`bca9fde8…`), con sus salidas.
  - Las corridas que valen son `/d/r5f` (la sonda de CT-09), `/d/r5g` (RED y GREEN) y `/d/r5h` (`IsDirty`).
  - De `/d/r5a` a `/d/r5e` son la vuelta parada.
- **La lectura RR-3 de la cadena sobre la v5.0:** `v50\check_rr3_v50.py` y su salida.

**Qué comprobar:**
1. **La lista de 4 y las dos caídas que el Owner añadió:**
   - ¿cada `Skip` lleva su motivo literal?;
   - ¿cada equivalente es la copia literal con solo lo declarado cambiado? En CT-PBT4B-20, que también devuelve el `Skip` de PB-T4c en PEG. En CT-PBT2B-06, solo la línea de la ventana 4;
   - ¿alguna prueba más podría caer fuera de las 4 que la precondición 6 no haya corrido? Por ejemplo, la UI fuera de los 18 filtros.
2. **D-S9:**
   - ¿el nombre nuevo deja la guarda de `WorkspaceModelBoundaryTests` en verde por texto?;
   - ¿algún otro nombre nuevo de PB-T4c casa con `Subscription|Editor|Draft|Tab` o con las demás guardas de nombres?
3. **D-S10 y CT-09:**
   - ¿la sonda de `/d/r5f` sostiene que «solo un módulo escenificado» no se alcanza en WORKSPACE?;
   - ¿CT-09 cubre lo alcanzable?;
   - ¿la pregunta combinada cumple A-T2b-OWNER-01 (F4B-E38) tal como lo decidió el Owner?
4. **D-S7 y D-S8:**
   - ¿la costura evita construir el configurador real (C05)?;
   - ¿algún filtro nuevo o heredado selecciona lo que no debe (C04)?
5. **El contrato literal de §6.3.1 y §6.3.2:**
   - el alcance de 26 rutas;
   - UNIT_CHANGE;
   - los invariantes y las paradas, con la lista cerrada de C13 contra el GREEN;
   - las 85 filas de `RequiredTests` con sus `MinSelected`: bajan el n.º 7, el 12 y el 36, y sale el n.º 43;
   - la `ExpectedEvidence`, con `[13]` y `[14]`, y los totales de cada rama:
     - Core: RED 19 619 = 19 149 + 9 + 461 y GREEN 19 619 = 19 158 + 0 + 461;
     - UI: RED 2 486 = 2 432 + 5 + 49 y GREEN 2 486 = 2 437 + 0 + 49;
   - el recorte de las autoridades.
6. **El GREEN literal:**
   - ¿cumple el diseño de D-PBT4-12 a 17 y las restricciones medidas (WHC con sus dos interfaces, PEG:1015, PEG:1079 y `EditAllOutcome` fuera del censo)?;
   - ¿hay algo de «traducción de estado», o algo que toque D2?
7. **RR-3 (§6.0):** ¿cada fila nombra su clase y la lectura las resuelve todas?
8. **¿Hay alguna decisión nueva para el Owner?**

**Formato:** §22 con hallazgos B/I/M (archivo:línea), un veredicto (EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE) y la línea BOUNDED en ASCII, como en las otras rondas. Si el entorno no te deja escribir el archivo, me pasas la §22 entera en tu respuesta.

## 2. Recomprobación 1 (2026-11-21T19:12:34.050Z)

Recomprobación de CA-T4c sobre la v5.1: **§23** de tu `independent-review-v1.md`. Las reglas de lectura son las de la §22: objetos git en `6adff90d`, nada en el worktree, sin compilar, Python con `-I` y guiones `ir4_`.

**Las entradas** (en `hs65\pbt4\`, salvo que diga otra cosa):
- **El paquete,** `I-64-f4b-pbt4-gate-contract-package-v51.md` (`34ca0026…`), con su tabla V51-nn; la fila de I-01 empieza `| **V51-01** | **CA-T4c I-01** `. Compárala con la v5.0 (`d19dd830…`).
- **Las piezas nuevas:** `pbt4c-green-v51.txt` (`081ef77e…`), `pbt4c-skeleton-v51.txt` (`f8840c67…`), `green_pbt4c_v51.py`, `apply_red_pbt4c_v51.py`, `apply_green_pbt4c_v51.py`, `pbt4c_ct_table_v51.py`, `pbt4c-rehearsal-textct.cs`, `census_c13_v51.py` y `make_v51.py`. Los `Skip`, los equivalentes, `check_skips` y la lista no cambian.
- **La precondición 6 repetida:** `hs65\pbt4ccc\compile\pbt4c-precondition6.json` (`29179a12…`) y `compile-check.md` (`060c5c8b…`), con las corridas `/d/r5i` a `/d/r5l`. El paso 8 es nuevo: el ensayo de las CT de texto, su control sobre la v5.0 (`/d/r5l`) y el censo de C13.
- **La lectura RR-3 de la cadena sobre la v5.1:** `v51\check_rr3_v51-out.txt`.

**Qué comprobar:**
1. **I-01:**
   - ¿los dos archivos de `Draft/` quedan sin `#nullable` y sin el `?`, en el esqueleto y en el GREEN?;
   - ¿la pureza de CT-15 se cumple por texto?;
   - ¿algún otro archivo nuevo está sujeto a una regla de pureza o de texto que no cumpla? Dos conservan la directiva: `IWorkspaceFullEditHost.cs` y `RackWorkspaceEditorWindow.cs`.
2. **M-01 a M-03:**
   - ¿la tabla línea a línea de §5.3 para CT-03, 04, 05 y 12 dice la verdad frente a la sonda?;
   - ¿la frase del nombre del equivalente de PEG está?;
   - ¿el comentario de W casa con D-S10?
3. **El paso 8 nuevo:**
   - ¿el ensayo de las CT de texto (CT-01, 07, 08, 13 y 15) usa las líneas literales de §5.3?;
   - ¿su control sobre la v5.0 discrimina I-01?;
   - ¿el censo de C13 es correcto?;
   - ¿la declaración de lo que no cubre es honesta?
4. **El diff de la v5.0 a la v5.1:** ¿cambia algo más que lo de la tabla V51, y nada de la lista, los totales, los filtros ni las decisiones?
5. **¿Hay alguna decisión nueva para el Owner?**

**Formato:** §23 con hallazgos B/I/M, un veredicto y la cola de la línea BOUNDED (`; recomprobacion v5.1: …`), como dice tu §22. Si el entorno no te deja escribir el archivo, me pasas la §23 entera.
