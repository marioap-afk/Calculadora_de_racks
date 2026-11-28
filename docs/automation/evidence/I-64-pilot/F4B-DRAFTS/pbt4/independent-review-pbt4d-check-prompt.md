# Encargo de la comprobación acotada de PB-T4d (CA-T4d) (enviado por la sesión a un revisor independiente nuevo, la recomprobación 1 como mensaje al mismo revisor, y la recomprobación 2, solo del diff, a otro revisor nuevo; texto exacto)

**Rutas locales:** el repositorio es público. Las tres rutas locales del primer encargo se sustituyen por `<worktree>`, `<scratchpad de la sesión>` y `<checkout principal>`. Es la única diferencia con el texto enviado.

## 1. CA-T4d, sobre la v5.2 (2026-11-22T10:34:49.983Z)

Mensaje de la sesión. Haz la **COMPROBACIÓN ACOTADA DE PB-T4d (CA-T4d)** sobre la v5.2 del paquete del contrato de PB-T4 (I-64 F4B, RackCad: plugin de AutoCAD 2025 en C#/.NET 8 con WPF; rama `architecture/workspace-persistente-rackcad`). Eres un revisor independiente y adversarial: no has participado en el paquete. Busca defectos reales y no te fíes de los resúmenes.

**Qué es PB-T4d:** la retirada del código del piloto del Workspace de Push Back, que ya no usa el panel desde PB-T4b y PB-T4c. La emisión es condicional, por decisión del Owner: se emite sin volver a él si esta comprobación da 0 bloqueantes y 0 importantes, la remedición no cambia nada material y la lista es la elegida o una parte.

**Reglas de lectura (obligatorias):**
- **El worktree, PROHIBIDO.** Nada en `<worktree>`, ni para leer.
- **El código y los documentos, solo por objetos git** desde el checkout principal: `git -C "<checkout principal>" show <sha>:<ruta>`, `diff`, `log`, `grep` y `ls-tree`. La base es el cierre de PB-T4c, `6c2820bab4d8b41e3e229e23f3a1638ccbb04735`.
- **Nada que compile o ejecute pruebas.** Python solo de lectura, escrito con la herramienta Write y ejecutado con `python -I <archivo>`; nunca `-c`, `python -` ni heredoc.
- **Ninguna escritura en el repositorio.**
- **Lo que no puedas verificar, dilo.**

**Las entradas.** `S` = `<scratchpad de la sesión>\hs65`.
- **El paquete:** `S\pbt4\I-64-f4b-pbt4-gate-contract-package-v52.md` (sha256 `466ab3db…f13e`), con §4.4, §5.6, §6.4 y su bloque literal, y el anexo V52-A. La v5.1 anterior es `S\pbt4\I-64-f4b-pbt4-gate-contract-package-v51.md`.
- **Las decisiones del Owner sobre PB-T4d,** literales, en `S\pbt4\`:
  - `owner-pbt4d-list-decisions.md`: el alcance B, la recarga de «Editar todo» fuera de PB-T4d, la prueba lenta acotada y la emisión condicional;
  - `owner-pbt4d-substitutes-decision.md`: sustitutas donde hace falta;
  - `owner-pbt4d-run-coverage-decision.md`: las sustitutas de WPUR;
  - `owner-pbt4d-deadcode-27-decisions.md`: quitar la rama muerta de `BuildCore`, y 27 sustitutas.
- **Las decisiones de la sesión:** `S\pbt4\session-pbt4d-design-decisions.md` (D-S11 a D-S15).
- **Las piezas, en `S\pbt4\`:**
  - el GREEN: `pbt4d-green-v52.txt` (`4059860f…4932`) y `green_pbt4d_v52.py`;
  - el RED: `pbt4d-red-v52.txt` (`26734e73…cf42`), con su módulo y su aplicador;
  - las ediciones con registro: `pbt4d_test_edits_v52.py`;
  - la lista: `pbt4d-retire-list-229.tsv` (`1943468b…1fad`, 242 filas, 229 con acción).
- **Lo que hay en `S\pbt4\v52\`:**
  - las 27 sustitutas, en `run\PushBackRetirePilotRunTests.cs`;
  - las CT, en `ct\`;
  - los generadores y las medidas: `need-report-r6h.txt`, `cov-r6j.txt`, `cov-tests-r6k.txt`, `sub\cov-sides.txt` y `sub\cov-otras.txt`;
  - los traspasos `handoff-pbt4d.md`, `handoff-pbt4d-v52.md` y `handoff-pbt4d-v52-f4.md`.
- **La precondición 6,** en `S\pbt4dcc\compile\`: `pbt4d-precondition6.json` (`56bcba77…70b5`), `compile-check.md` (`9c4adedab…8934`), `cov-p6-v52.txt`, `required-pbt4d.tsv`, `census-c13-v52.txt` y los registros de las corridas.

**Qué comprobar,** con evidencia `ruta:línea` y `sha`:
1. **La lista frente a las decisiones del Owner.**
   - ¿Es el alcance B, más la rama muerta, más las 27?
   - ¿Cada fila tiene la acción que le toca: sustituir, borrar, editar con registro o se queda?
   - ¿Las 7 CT de PB-T3b y sus 6 ayudantes se quedan?
   - ¿Lo que se borra solo fija lo retirado, o pasa en vacío?
   - ¿Alguna baja deja sin prueba código que se queda?
2. **El GREEN literal.**
   - ¿Quita solo lo retirado y la rama muerta (`allowErase` y lo que solo existe con él), sin tocar el comportamiento de lo que se queda?
   - ¿Queda algún código muerto o alguna referencia colgando?
   - ¿Las constantes públicas que quita las usaba solo lo retirado?
3. **El RED y las ediciones con registro.**
   - ¿Cada edición conserva lo que protegía la prueba sobre lo que se queda?
   - ¿Las 27 sustitutas afirman lo mismo que sus originales, sobre `ExecuteFull`?
   - ¿Las dos CT (CT-PBT4D-01 y 02) fijan lo que dice el paquete?
   - ¿Los equivalentes de las 3 que caían, M-04 y la prueba lenta acotada son correctos?
4. **La cobertura.** ¿`cov-p6-v52.txt` sostiene, rama por rama, que ninguna rama de la corrida que se queda queda sin cubrir? ¿El método es sólido? Antes se equivocó contando cuántas ramas cubría cada prueba, en vez de cuáles.
5. **El contrato literal de §6.4:**
   - el alcance;
   - los invariantes y las paradas, con C02 y su lista de borrados;
   - los `RequiredTests` y sus `MinSelected`;
   - los totales de cada rama (RED Core 19 416 = 19 012 + 1 + 403; GREEN 19 416 = 19 013 + 0 + 403; UI 2 464 = 2 416 + 0 + 48);
   - la evidencia.
6. **La precondición 6:**
   - ¿la base local (19 619 = 19 158 + 0 + 461) casa con la CI del cierre de PB-T4c?;
   - ¿hay algo que pueda caer fuera de la lista y no se haya corrido? Por ejemplo, la UI fuera de los filtros.
7. **¿Hace falta alguna decisión nueva del Owner?**

**SALIDA:** el informe en español en `S\pbt4\independent-review-pbt4d-check.md`, sin rutas locales absolutas. Si el entorno no te deja escribir el archivo, pon el informe entero en tu respuesta. Lleva:
- un veredicto: EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE;
- el recuento B/I/M;
- la tabla de hallazgos, con id, gravedad, dónde, qué y arreglo;
- lo que comprobaste y está bien;
- lo no verificado;
- **una línea BOUNDED en ASCII, en una sola línea que empiece por `> `**. Su forma, del precedente:

  > EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 3; las importantes corregidas en el paquete vX (I-01, ...); sin decisiones nuevas del Owner; precondicion 6 sobre <sha8>: ...

  Si el veredicto es EMITIBLE sin importantes, la línea empieza por «EMITIBLE; B 0 / I 0 / M n; ...».

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes o importantes.

## 2. Recomprobación 1 (2026-11-22T14:57:00.586Z)

Recomprobación de CA-T4d sobre la v5.3 del paquete de PB-T4. Va como sección nueva al final de tu `pbt4/independent-review-pbt4d-check.md`, con el título `## Recomprobación 1 (v5.3)`. No toques lo que ya escribiste.

Las reglas de lectura son las de tu primer encargo:
- solo objetos git en `6c2820ba`;
- nada en el worktree;
- sin compilar ni ejecutar pruebas;
- Python solo con `python -I <archivo>`, escrito con Write;
- tus guiones, en `rv4d/`.

**Qué decidió el Owner tras tu informe** (`pbt4/owner-pbt4d-cat4d-decisions.md`, sha256 `fa07db38…0d75`):
- **I-01:** la 48 y la 49 se sustituyen, así que son 29 sustitutas.
- **I-02:** la cobertura se amplía a todo `src/` que se queda.
- **Regla permanente:** todo camino que se queda y pierde su única prueba recibe una sustituta. Vuelve al Owner solo si una no se puede escribir, o si salen más de 10 nuevas sobre las 29. Lo que cubra la UI se mide con sus filtros.

**Lo que decidió la sesión:** D-S16, en `pbt4/session-pbt4d-design-decisions.md` (`27010f8f…`).
- **La unión:** la UI de cada lado se cuenta como la unión de 3 corridas de todos los filtros juntos y de una corrida de cada filtro en su proceso (R-T4-38).
- **Lo que baja:** el informe de cobertura lista aparte, como informativo, lo que cubren más corridas en la base que en el GREEN.

**Las entradas** (en `hs65\`):
- **El paquete:** `pbt4/I-64-f4b-pbt4-gate-contract-package-v53.md` (`ae338b2d…9daf`), con su tabla V53-nn. Compáralo con la v5.2 (`466ab3db…f13e`).
- **El GREEN:** `pbt4/pbt4d-green-v53.txt` (`51a40209…1436`).
- **El RED:** `pbt4/pbt4d-red-v53.txt` (`de0d3a0e…c24e`). Edita 23 archivos, borra 3 y crea 5.
- **La lista:** `pbt4/pbt4d-retire-list-270.tsv` (`7e65c9e4…081f`), con 283 filas, 270 de ellas con acción.
- **Los módulos de la v5.3:** `pbt4/{green,apply_green,red,apply_red}_pbt4d_v53.py` y `pbt4/pbt4d_test_edits_v53.py`.
- **Las 29 sustitutas:** `pbt4/v53/run/PushBackRetirePilotRunTests.cs` (`3554df0d…fe69`).
- **Los generadores y el traspaso:** `pbt4/v53/` y `pbt4/v53/handoff-pbt4d-v53.md`.
- **La precondición 6 repetida,** en `pbt4dcc/compile/`:
  - `pbt4d-precondition6.json` (`4cc06549…5567`) y `compile-check.md` (`58455b41…aa0e`);
  - la cobertura: `cov-p6-v53.txt` (`8090f08a…6615`) y `cov_p6_v53.py`. Según el autor, PASA, con 0 líneas y 0 ramas perdidas en 775 archivos. Su sección «Bajan» tiene 16 801 entradas: PERDIDA 0, VARIA 0 y ESTABLE 16 801. Casi todas vienen del filtro `RackCad.UI.Tests.Workspace`, y 118 de la clase que se borra entera. Coverlet no dice qué prueba cubre cada línea, así que la entrada nombra la corrida, no la prueba;
  - `required-pbt4d.tsv`, `census-c13-v53.txt` y los registros de `/d/r6w`, `/d/r6x`, `/d/r6y` y `/d/r6z`.
  - Las salidas de la v5.2 están renombradas con `-v52`.

**Qué comprobar,** con evidencia `ruta:línea` y `sha`:
1. **I-01.**
   - ¿Las sustitutas de la 48 y la 49 afirman lo mismo que sus originales, sobre el modelo completo? La 49 va a mano, igual que la 10 y la 43.
   - ¿`PushBackPilotSystemRebuild` queda cubierto entero en el GREEN, con la línea 32 y sus 6 ramas?
2. **I-02, el método de `cov_p6_v53.py`.**
   - ¿Mira todo `src/` que se queda, línea a línea y rama por rama, con la suite entera de Core y la UI de los filtros, en los dos lados?
   - ¿La unión de D-S16 es simétrica entre los dos lados?
   - ¿La sección «Bajan» lista lo que debe?
   - ¿Alguna entrada de «Bajan» es una pérdida real que la unión tapa? Es decir, una prueba que se borra la cubría en todas las corridas de la base y en el GREEN solo la cubre la UI a ratos.
   - ¿Las clases PERDIDA, VARIA y ESTABLE están bien definidas, y bien aplicadas?
   - Una entrada cuenta como ESTABLE si el GREEN la sigue cubriendo con Core o con las tres corridas de todos los filtros juntos. ¿Basta eso para descartar una pérdida? Mira en particular el filtro `RackCad.UI.Tests.Workspace` y la clase que se borra entera.
   - ¿Las 37-38 de `RackViewBatchDialogPresenter` son ruido, como se dice?
   - Reprodúcelo tú desde los JSON de coverlet, como hiciste con `covdiff_rv.py`.
3. **M-01 a M-07.** ¿Cada uno está aplicado o declarado como pediste?
   - M-01 es el corpus (iii) de CT-PBT3B-06, en su sitio. Es la única sustituta nueva sobre las 29.
   - M-02 es la rama de `PushBackCompositeEditorState.cs:233`, que según el autor cubre la UI.
   - M-03 son los 32 ayudantes y las 3 clases enteras borradas, con los bloques `DELETE` del RED.
   - M-04 son las columnas nuevas y las 6 filas `ayudante-editado`.
   - M-05 son los 11 bloques de solo comentarios.
   - M-06 es el comentario de la clase.
   - M-07 es la copia principal sola, con los 18 filtros.
4. **La regla del Owner.** ¿Hay algún camino que se queda y pierde su única prueba sin sustituta? ¿Las sustitutas nuevas sobre las 29 son 10 o menos?
5. **El diff de la v5.2 a la v5.3.**
   - ¿Cambia solo lo de la tabla V53?
   - ¿Los cambios de la lista (de 229 a 270 con acción), de los totales y de los filtros están explicados, cada uno con su hallazgo?
6. **La precondición 6.**
   - ¿La base sigue casando con la CI?
   - ¿El RED y el GREEN dan los totales que fija §6.4 de la v5.3? Según el autor, en `/d/r6y`:
     - RED Core 19 418 = 19 014 + 1 + 403, y solo cae CT-PBT4D-01;
     - GREEN Core 19 418 = 19 015 + 0 + 403;
     - UI 2 464 = 2 416 + 0 + 48 en las dos ramas.

     `RequiredTests` tiene 88 entradas, y solo cambia el `MinSelected` del n.º 88, de 29 a 31. El control en `/d/r6z` está en `compile-check.md`.
   - ¿Nada cae fuera de la lista, con 0 `warning CS`?
   - ¿La copia principal corrió sola?
7. **¿Hace falta alguna decisión nueva del Owner?**

**Formato:**
- La sección lleva hallazgos B/I/M con `archivo:línea`.
- Pon una línea propia que empiece exactamente por `**Veredicto:** ` y siga con EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE, y con `B n / I n / M n`.
- Pon también la cola ASCII de la línea BOUNDED (`; recomprobacion v5.3: ...`), en una sola línea que empiece por `> `.
- Si el entorno no te deja escribir el archivo, pásame la sección entera en tu respuesta.

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes o de los importantes.

### 2.1. Complemento enviado durante esta ronda (2026-11-22T15:08:59.751Z)

Mensaje de la sesión, para tu Recomprobación 1 (v5.3).

El generador, al portar a la v5.3, encontró tres desajustes entre el paquete y la precondición 6. Inclúyelos en tu sección, con su gravedad, y di si el arreglo que propone la sesión basta.

1. **`GreenUiCoverage`.** La lista del esquema de la precondición 6 en §6.4.2 (la fila «La precondición 6» de la tabla de la evidencia) la nombra, pero el JSON (`4cc06549…`) no tiene esa clave.
   - La cobertura de UI del GREEN está en `GreenWithCoverage.UI` y `GreenWithCoverage.UIRepeats`.
   - **Arreglo propuesto:** quitar `GreenUiCoverage` de la lista, porque `GreenWithCoverage` ya está.
   - Comprueba que de verdad están en el JSON los datos de las corridas de UI del GREEN que usa la unión de D-S16: las tres de todos los filtros juntos y una por filtro, con sus sha.
2. **`RedDeleted` y `Noise`.** Las dos están en el JSON y no en la lista de §6.4.2.
   - `RedDeleted` tiene los 3 borrados del RED.
   - `Noise` es el ruido de `RackViewBatchDialogPresenter.cs:37-38` en `r6w` y `r6x`.
   - **Arreglo propuesto:** añadir las dos a la lista. El generador lee `RedDeleted` y comprueba que es igual a los bloques DELETE del RED. `Noise` se declara como dato informativo, que no se lee.
3. **[13] de §6.4.1.** Dice «D-S11 a D-S15 y lo que la sesion fijo tras CA-T4d», y §6.4.2 y la cabecera dicen D-S11 a D-S16.
   - Lo que fijó la sesión tras CA-T4d es D-S16, así que no es falso, pero es impreciso.
   - **Arreglo propuesto:** «D-S11 a D-S16».

Los tres arreglos, y los menores tuyos que haya que aplicar, irían en una v5.4 de solo texto. Después te pediría una recomprobación corta, solo del diff.

Si alguno de los tres te parece más que menor, dilo en tu veredicto.

## 3. Recomprobación 2, a un revisor independiente nuevo (solo el diff de la v5.3 a la v5.4) (2026-11-22T15:31:03.528Z)

Mensaje de la sesión. Haz la **RECOMPROBACIÓN 2 DE CA-T4d, SOLO DEL DIFF de la v5.3 a la v5.4** del paquete del contrato de PB-T4 (I-64 F4B). El proyecto es RackCad, un plugin de AutoCAD 2025 en C#/.NET 8 con WPF, en la rama `architecture/workspace-persistente-rackcad`.

Eres un revisor independiente y adversarial: no has participado en el paquete ni en las revisiones anteriores. Busca defectos reales y no te fíes de los resúmenes.

**El contexto, en breve:**
- **PB-T4d** retira el código del piloto del Workspace de Push Back.
- **Su emisión es condicional**, por decisión del Owner: se emite sin volver a él si la comprobación acotada (CA-T4d) da 0 bloqueantes y 0 importantes.
- **Lo que ya hay** en `S\pbt4\independent-review-pbt4d-check.md`, escrito por otro revisor:
  - CA-T4d sobre la v5.2: EMITIBLE TRAS CORRECCIONES, 0/2/7;
  - la sección «## Recomprobación 1 (v5.3)», desde la línea 166: EMITIBLE, B 0 / I 0 / M 6, con los menores M-08 a M-13 para una v5.4 de solo texto.
- **Tu trabajo:** comprobar que la v5.4 corrige esos seis y que no cambia nada más.

**Reglas de lectura (obligatorias):**
- **El worktree, PROHIBIDO.** Nada en `<worktree>`, ni para leer.
- **El código, solo por objetos git** desde el checkout principal: `git -C "<checkout principal>" show <sha>:<ruta>`, `diff`, `log`, `grep` y `ls-tree`. La base es `6c2820bab4d8b41e3e229e23f3a1638ccbb04735`.
- **Nada que compile o ejecute pruebas.**
- **Python solo de lectura:** escrito con la herramienta Write, en una carpeta tuya `S\rv4d2\`, y ejecutado con `python -I <archivo>`. Nunca `-c`, `python -` ni heredoc, y nunca `sed -i`.
- **Ninguna escritura en el repositorio ni fuera de `rv4d2\`,** salvo la sección de tu informe, abajo.
- **Lo que no puedas verificar, dilo.**

**Las entradas.** `S` = `<scratchpad de la sesión>\hs65`.

| Qué | v5.3 (historia) | v5.4 (la que se emite) |
|---|---|---|
| El paquete | `pbt4\I-64-f4b-pbt4-gate-contract-package-v53.md` (`ae338b2d…9daf`) | `pbt4\I-64-f4b-pbt4-gate-contract-package-v54.md` (`e37843fc4bb5a1d100191928ad06ce05cbf19705f5084b809442257ba866fc63`), tabla V54-01..07 |
| El RED | `pbt4\pbt4d-red-v53.txt` (`de0d3a0e…c24e`) | `pbt4\pbt4d-red-v54.txt` (`80f8cc0c5c9f54de033648e7eec95728fa0fc0d28e31886101207e3dbec381db`) |
| El GREEN | `pbt4\pbt4d-green-v53.txt` (`51a40209…1436`) | `pbt4\pbt4d-green-v54.txt` (`aaf6cab882cfb3eba6939fd8b64ec1fa15c22431ba8bdaeb9bcdf4dbb89ffbf0`) |
| La lista | `pbt4\pbt4d-retire-list-270.tsv` (`7e65c9e4…081f`) | `pbt4\pbt4d-retire-list-271.tsv` (`05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac`) |
| Las ediciones con registro | `pbt4\pbt4d_test_edits_v53.py` | `pbt4\pbt4d_test_edits_v54.py` (`b8c78f72…0aea`) |
| La precondición 6 | `pbt4dcc\compile\pbt4d-precondition6-v53.json` (`4cc06549…5567`) | `pbt4dcc\compile\pbt4d-precondition6.json` (`52e03e061cd62c79597f51b8eaf144a27d68b3bf5db876406a804be2fa978989`) |
| `compile-check` | `pbt4dcc\compile\compile-check-v53.md` (`58455b41…aa0e`) | `pbt4dcc\compile\compile-check.md` (`d13afdb02216d385c32ade81b5cf9508c648a970337cf92b9d964b936933de42`) |
| La cobertura | `cov-p6-v53.txt` y `cov_p6_v53.py` | `cov-p6-v54.txt` (`9c1bde53…bd34`) y `cov_p6_v54.py` (`8d6aaccc…03b6`) |

Además:
- **la reaplicación de la v5.4,** sin pruebas: `pbt4dcc\compile\driver-r70-ra.txt` (`2044f0fc…5f36`) y `p6_reapply_v54.sh`;
- **los guiones del autor:** `pbt4\v54\`, con `codeonly-v53-v54.txt`, `codeonly_v54.py` y `edit_frags_v54.py`, y `pbt4\make_v54.py`;
- **el traspaso del autor:** `pbt4\v54\handoff-pbt4d-v54.md`;
- **las corridas,** en `pbt4dcc\compile\`: `r6w-bc\`, la base, y `r6y-p6\`, el RED y el GREEN, con sus JSON de coverlet;
- **la decisión de la sesión D-S16,** con su nota tras M-11: `pbt4\session-pbt4d-design-decisions.md` (`bd02d2103c7a671c51c1de8901bf76608e4944f98ad0fabcca1fdd253d3801c8`).

**Qué comprobar,** con evidencia `ruta:línea` y `sha`:
1. **M-08.**
   - ¿Fija el JSON de la v5.4 el sha256 de los JSON de cobertura de UI del GREEN, las 3 corridas juntas y los 18 por filtro, en `GreenUiCoverage` y en `Green.UI[n].Json`? ¿Y el sha256 completo de la TRX y del JSON de los 19 filtros de la base, en `BaseCoverage.UIByFilter.Runs`?
   - **Calcula tú esos sha256** sobre los archivos de `r6y-p6\` y `r6w-bc\`, y di si casan.
   - ¿La lista del esquema de §6.4.2 nombra exactamente las claves de primer nivel del JSON, sin que falte ni sobre ninguna, con `RedDeleted` leída contra los bloques DELETE y `Noise` declarada como no leída?
2. **M-09:** ¿[13] de §6.4.1 dice «D-S11 a D-S16»?
3. **M-10:** ¿«Lo que cambia» y la columna «Dónde» nombran §0, §1, §6.0, §7 y §9?
4. **M-11.**
   - ¿`cov_p6_v54.py` exige, para ESTABLE solo por la UI, las tres corridas juntas y además un filtro en su proceso?
   - ¿Su diff con `cov_p6_v53.py` es solo eso?
   - ¿`cov-p6-v54.txt` da PASA, con PERDIDA 0, VARIA 0 y ESTABLE 16 801 + 118?
   - ¿«Bajan» declara que nombra la corrida, no la prueba?
   - ¿El paquete cita D-S16 con `bd02d210…01c8`?
5. **M-12.**
   - (a) ¿En WPUR del RED solo cambian comentarios? **Compruébalo tú:** aplica en memoria el RED v5.3 y el v5.4 sobre los objetos de `6c2820ba`, y compara el código sin comentarios archivo por archivo.
   - (b) ¿El GREEN v5.4 tiene los mismos bloques que el v5.3, y solo cambia su cabecera?
   - (c) ¿El porqué de la fila de `WriteViolations` es el suyo?
   - (d) ¿El `using` de `WorkspaceDraftPilotIntentStaleBaseTests.cs` tiene su fila (grupo `directiva`)?
   - ¿El diff de la lista de 270 a 271 es solo eso?
6. **M-13:** ¿§4.4 explica el paso de 436 a 537 líneas?
7. **El diff entero de la v5.3 a la v5.4,** del paquete, de las piezas, de la lista y del JSON.
   - ¿Cambia solo lo de la tabla V54?
   - ¿Siguen iguales los totales, los `RequiredTests`, los filtros, la lista de C02 (salvo la fila nueva) y las decisiones?
8. **¿Hace falta alguna decisión nueva del Owner?**

**SALIDA.** Añade al final de `S\pbt4\independent-review-pbt4d-check.md` una sección nueva, `## Recomprobación 2 (v5.4, solo el diff)`, sin tocar nada de lo anterior. Usa la herramienta Edit o Write sobre el archivo entero, conservando exactamente sus bytes anteriores. Escríbela en español y sin rutas locales absolutas, con:
- tus guiones;
- las entradas con su sha256;
- **una línea propia que empiece exactamente por `**Veredicto:** `,** seguida de EMITIBLE, EMITIBLE TRAS CORRECCIONES o NO EMITIBLE y de `B n / I n / M n`;
- la tabla de los seis menores, con su estado y la evidencia;
- los hallazgos nuevos, si los hay;
- lo no verificado;
- las decisiones del Owner;
- al final, **una línea en ASCII, en una sola línea que empiece por `> ; recomprobacion v5.4: `**, con el veredicto, el B/I/M y lo esencial.

Si el entorno no te deja escribir el archivo, pon la sección entera en tu respuesta.

**Tu respuesta final,** en dos líneas: el veredicto con B/I/M, y los ids de los bloqueantes o de los importantes.
