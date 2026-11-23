# I-64 F4B PB-T5 — Comprobación acotada del paquete v2.3 (CA-T5)

**Revisor:** independiente y adversarial; no participó en el paquete, en la guía ni en el guion.
**Hora:** 2026-11-23, de 09:55Z a 10:20Z aproximadamente.
**Rutas:** relativas a la carpeta `pbt5/` de la sesión, o `ebd304a3:<ruta>:línea` para el repositorio.

## 0. Objeto, reglas y método

**Los sha256 coinciden con los del encargo** (medidos con `sha256sum`):

| Sigla | Documento | sha256 |
|---|---|---|
| [P] | `I-64-f4b-pbt5-package-v2.3.md` | `0282024935b1e846f2954d4fdfbc1d49ef5d108a5d2bdd654be3d60524b46db6` |
| [OD5] | `owner-pbt5-decisions.md` | `2d63b10d2c7ab15ef6e573df0ee69e895ea678946937a6f09b89101611f12a45` |
| [SR] | `smoke-run-v1.md` | `2448959830d88c1d5cef7f08ceca06044cbdaab9c259d9df5982d14de03c7ced` |
| [E] | `_tools/emit_pbt5.py` | `654dd7283ae7a9fc30fa6b385e4d822e1e448c2c196debf2aac85e700a25ca33` |
| [H3] | `handoff-pbt5-v3.md` | `2671b6333ebcd2898b51defbc849c14519eca1e5c8383e9d7646c52f4ab5c4ff` |

Otras siglas: [P22] = `I-64-f4b-pbt5-package-v2.2.md`; [PR24] = `pre-run-tonight-v2.4.md`; [IR1] = `review-v1/independent-review-pbt5-v1.md`; [Q22] = `owner-questions-v2.2.md`.

**Reglas cumplidas:**
- el repositorio, solo por objetos git desde el checkout principal (`show`, `diff`, `log`, `grep`, `ls-tree`, y además `rev-parse`, `cat-file`, `merge-base` y `remote get-url`, todos de lectura); nada del worktree ni de los archivos en disco del checkout principal;
- sin AutoCAD, sin compilar, sin pruebas y sin ejecutar `emit_pbt5.py`;
- Python con Write y `python -I`; el analizador de PowerShell, sin ejecutar ningún bloque;
- escrito solo en `review-v23/`: este informe, tres herramientas sin rutas locales (`ps_audit.ps1`, `lisp_audit.py`, `sim_collect.py`) y sus salidas (`*-out.txt`);
- una lectura de la CI por `gh run view` (solo lectura).

**Método:**
- lectura completa de [P], [OD5], [SR], [E], [H3] y de los §2, §3.3, §5 y §6 de [P22]; los hallazgos y recomprobaciones de [IR1];
- `diff` de [PR24] frente a [SR];
- `ps_audit.ps1`: los 20 bloques de PowerShell de [SR], por el analizador, con las variables libres, los marcadores y las operaciones con efecto (`ps_audit-out.txt`);
- `lisp_audit.py`: las 22 líneas LISP (`lisp_audit-out.txt`);
- `sim_collect.py`: la redacción y el control de privacidad de `collect()` de [E], simulados en solo lectura (`sim_collect-out.txt`; no imprime ningún texto que case un patrón);
- comprobaciones en `ebd304a3`: la numeración, los marcadores de `I-64.yml`, los literales de [SR] en `src/`, las variables de inyección, los 19 filtros de UI, `dimkeys.py` y el SHA-256 de D1.

## 1. Veredicto

**EMITIBLE TRAS CORRECCIONES. B 0 / I 4 / M 15.**

- **Los cuatro importantes se resuelven sin el Owner,** si se corrigen por el lado estrecho que propone la tabla:
  - I-01 e I-02 son correcciones de texto y de bloques;
  - I-03 adopta la lectura literal de la frontera;
  - I-04 devuelve al Owner lo que su decisión le reserva.

  Si se prefiere la lectura amplia en I-03 o mantener PBT5-C16 como está, eso sí es una decisión del Owner.
- **La CI exacta del Candidato está en verde,** verificada aquí: la corrida `45147867500` es `push` sobre `ebd304a3773495fa9071de6611936c4c01a7b06c` en `architecture/workspace-persistente-rackcad`, `completed`/`success`, con los cuatro trabajos en `success`. Los totales del registro son Core 19 421 = 19 018 + 0 + 403 y UI 2 465 = 2 417 + 0 + 48, iguales a los de F4B-E61.
- **Tras las correcciones,** hace falta una recomprobación acotada de las versiones nuevas. [E] fija el nombre `…-v2.3.md` (`_tools/emit_pbt5.py:35`, `:157`, `:169`), así que también cambia, y entra en la recomprobación.

## 2. Tabla de hallazgos

| Id | Gravedad | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| **I-01** | **IMPORTANTE** | `smoke-run-v1.md:913-919`, `:946-950`, `:677-713` (bloque 10, `:695`); `:877-911` (bloque 13); [P]`:205` (P-09) y `:314` (SPB-51) | **El camino de Q-PBT5-2 (P-09 y SPB-07) no tiene órdenes exactas ni entra en la integridad final.**<br>• **El paso a las `eNN` y a `s01`/`s02`:** el bloque 8 exige nombres `^c\d\d$` y compara con D1. La guía lo sustituye por una frase («`Test-Path` del `.dwl` y el hash frente a `estilo.json`», `:919`), sin bloque. Tampoco se comprueba el cierre de c01 al pasar a `e01`, ni el de la última `eNN` al pasar a c02;<br>• **`s01` y `s02`** se crean «con un bloque como el 13» (`:947`): una escritura de copias sin orden exacta, en una corrida que puede operar la sesión sola;<br>• **el bloque 10 (SPB-51)** solo recorre `copias.json` (c01 a c44), el original D1 y su respaldo. No mira los originales de P-09 después de la corrida, ni sus respaldos `eNN-respaldo.dwg`, ni las copias de `estilo.json`, ni `s01`/`s02`. Así, «el original, igual antes y después» de P-09 solo se mide al terminar el bloque 13, antes de abrir AutoCAD;<br>• **el bloque 13** no compara cada original con su SHA-256 registrado en `F4-SMOKE-2/run/integrity.md` (O1 a O6, `ebd304a3:…/F4-SMOKE-2/run/integrity.md:54-60`), que es «la regla de P-06» que cita P-09 | Un bloque exacto para el paso a una `eNN` o `sNN` (como el 8, con la lista de `estilo.json` o de un `s-copias.json`).<br>Un bloque 13b exacto para `s01` y `s02`: sin sobrescribir, desde el original y con el hash comprobado.<br>Ampliar el bloque 10: `estilo.json` y `s-copias.json`, los respaldos `eNN-respaldo.dwg` y los originales de la lista local, frente a sus hashes.<br>En el bloque 13, registrar, sin parar, si cada original coincide con su hash de Smoke-2 |
| **I-02** | **IMPORTANTE** | [P]`:577`; [P22]`:625` (PBT5-C01), `:632` y `:635`; [P]`:314` (SPB-51); `smoke-run-v1.md:707-710`; [E]`:170-176` y `:204` | **El contrato no trae sus paradas, y lo heredado contradice la cadena nueva.**<br>• `contract.md` es el §6 de la v2.3 tal cual. Su §6.4 dice «Son las de la v2.2 (§6.4: PBT5-C01 a PBT5-C15), con estos cambios», así que **11 de las 17 paradas** (C01 a C03, C05 a C10 y C12 a C14) solo están en un borrador superado. Ese borrador se custodia como historia, y su §6 se titula «propuesto; su forma la decide el Owner». El Owner eligió un «contrato propio (invariantes, paradas, evidencia, presupuesto)»;<br>• **PBT5-C01 de la v2.2** para con «HEAD distinto», y la fila SPB-51 de [P] exige «HEAD = Candidato». Tras el commit de la emisión, HEAD nunca es el Candidato. El bloque 10 ya aplica la regla relajada, pero el contrato y la lista no: la revisión de la evidencia (Q-PBT5-8) encontraría un SPB-51 que no cumple su criterio escrito;<br>• F4B-E62 afirma que el contrato lleva «las paradas PBT5-C01 a C17» ([E]`:204`) | Escribir en el §6.4 de [P] las 17 paradas completas, con C01 adaptada: antes de la emisión, HEAD = Candidato; después, el Candidato antepasado de HEAD y el diff vacío en `src`, `tests` y `assets`.<br>Corregir la fila SPB-51 igual.<br>Así `contract.md` se basta solo |
| **I-03** | **IMPORTANTE** | `smoke-run-v1.md:56-70` (`:63`); [P]`:61`, `:455-457`, `:650` (S-1); [E]`:209`, `:257`, `:267`; [PR24]`:94`; [IR1]`:51` (I-03); [Q22]`:97-106` | **La franja de 12:00 a 13:00Z, sin confirmar, decide si la sesión opera sola.**<br>• El literal del Owner pone como condición «se complete antes de las 6 am», las 12:00Z, y excluye «después de las 7 am»;<br>• [IR1] I-03 lo calificó de IMPORTANTE, y se resolvió dejando S-1 a la vista para que el Owner lo confirmara en Q-PBT5-5. **Q-PBT5-5 no se preguntó** ([OD5]`:5`);<br>• la v2.3 y [SR] aplican igualmente la lectura amplia: la sesión opera sola si puede empezar antes de las 13:00Z. Además, [SR] retira la declaración explícita de la franja que tenía [PR24]`:94`;<br>• ahora la corrida es el intento 1 de 3, no una corrida previa.<br>Hay otra lectura posible: si «se complete» es PB-T4e, la condición se cumplió (`ebd304a3`, 09:31:36Z). Pero tampoco está confirmada | **Lectura estrecha, válida en las dos interpretaciones y sin decisión nueva:** la sesión opera sola solo si P-01 a P-08 y la emisión están hechas **antes de las 12:00Z**. Si no, espera al Owner. La parada de las 13:00Z sigue igual.<br>Actualizar [P] §0, §5.4, S-1 y C11, [SR] §2 y los textos de [E] (F4B-E62 y `I-64.yml`).<br>Mantener la lectura amplia exige preguntar Q-PBT5-5 |
| **I-04** | **IMPORTANTE** | [P]`:581` (PBT5-C16); [P]`:599-602`; [OD5]`:40` | **PBT5-C16 amplía Q-PBT5-9.**<br>• El Owner excluye del intento «una adenda que **pidas** al revisar», es decir, una que pida él;<br>• C16 deja que una discrepancia que encuentre el **revisor** lleve a la sesión a repetir un paso en AutoCAD como «adenda, que no gasta intento», sin pasar por el Owner;<br>• además, «sin cambiar un resultado» choca con el fin de la revisión: en Smoke-2, un falso FAIL solo se aclaró revisando las capturas ([OD5]`:27`). Si la revisión muestra que un resultado está mal, alguien tiene que cambiarlo, y C16 no dice quién | C16: la sesión solo corrige transcripciones (el registro frente a la captura o al inventario) y vuelve a la revisión.<br>**Si la discrepancia cambia un resultado o exige repetir un paso en AutoCAD, vuelve al Owner,** que puede pedir una adenda (que no cuenta, por Q-PBT5-9) |
| M-01 | MENOR | [E]`:15`, `:333-360`, `:345-348`, `:374-376`; [H3]`:49` | **«Hace todas sus comprobaciones antes de escribir nada» es inexacto.**<br>• `yml_new` (claves únicas, entre comillas, las tres marcas y «PB-T5 EMITIDA») corre después de copiar unos 65 archivos, crear el contrato y añadir F4B-E62 y la §292;<br>• no se comprueba antes que `contract.md` no exista, ni el mensaje en `--out`.<br>**Riesgo acotado de hecho:** medido en `ebd304a3:docs/automation/state/I-64.yml`, cada clave aparece una vez y entre comillas (`:10`, `:57`, `:71`, `:72`, `:74`, `:75` y `:78`), cada marca aparece una vez y no está «PB-T5 EMITIDA». Con HEAD = Candidato exigido, esas comprobaciones pasan | Llamar a `yml_new` sobre `HEAD:` y comprobar que no existen `CONTRACT` ni el archivo del mensaje dentro de `plan()`, en el `--dry`.<br>Escribir el procedimiento si queda un estado parcial: `git status`, restaurar DISP, EVID e `I-64.yml` desde HEAD y retirar solo las rutas del listado del `--dry`; o al Owner |
| M-02 | MENOR | [E]`:120-127` | **La línea `BOUNDED_RESULT` no se ata a lo revisado.** [E] no comprueba que el informe trate del paquete, la guía y el guion que emite: un cambio posterior en la v2.3 in situ pasaría. Además, acepta «EMITIBLE TRAS CORRECCIONES; B 0 / I 0» | Exigir en la línea los prefijos sha256 del paquete, de [SR] y de [E], y compararlos con los del listado. Exigir el token exacto `EMITIBLE;` |
| M-03 | MENOR | [E]`:162-164`, `:209`, `:267` | `smoke-run-v1.md` no está entre los archivos que tienen que ir como copia: si casara un patrón, iría solo por sha256, y F4B-E62 y `NEXT_ACTION` apuntarían a una guía que no está en el repositorio. Hoy va como copia (`sim_collect-out.txt`) | Añadirla a la lista que se exige como copia |
| M-04 | MENOR | `smoke-run-v1.md:933-935`; [H3]`:48` | **`git hash-object` aplica los filtros de fin de línea por defecto.** El repositorio trabaja con `core.autocrlf=true` (`ebd304a3:.gitattributes`, cabecera; `git config`), así que un `dimkeys.py` escrito con CRLF daría el mismo hash: la comprobación no es «byte a byte». No cambia el recuento | `git -C $main hash-object --no-filters $dk` |
| M-05 | MENOR | `smoke-run-v1.md:381`, `:728`, `:958`; [P]`:226` | c01 lleva «SPB-07 (a), el recuento en D1», pero SPB-07 (a) es RACKEDITAR en `s01`, una copia del dibujo con estilo (`:948`). Lo de c01 es la parte D1 de P-09 | Rotularlo «P-09 (D1)» |
| M-06 | MENOR | `smoke-run-v1.md:797-799`; [P]`:328-329`; `ebd304a3:…/F4-SMOKE-2/run/run-log-2.md:157`, `:162-163` | Los pasos de §4.11 no traen «Comprobar ahora», que es parte del criterio de SPB-60 y de SPB-61 («como el paso 82/84 de [S2]»). En el paso 84 siguió además «Descartar y recargar» | Añadir, para SEAL: «Comprobar ahora» → el texto literal → (en `after`) lo que ofrezca, sin «Actualizar» |
| M-07 | MENOR | `smoke-run-v1.md:715`, `:194`; [P]`:315` | **SPB-52 = los bloques 2 a 4** no lee las líneas `ERROR` del registro de RackCad (solo lista el archivo), ni los eventos de error de acad, ni el hash de `settings.json`, que [P] SPB-52 enumera. Smoke-2 midió «ningún evento de error en 15 minutos» | Un bloque exacto de lectura para esas tres cosas |
| M-08 | MENOR | `smoke-run-v1.md:198` | La biblioteca de bloques va como «el fragmento de [G] §7.1, tal cual», sin `exit 0` (con StrictMode y sin orden externa, la herramienta da código 1; [IR1]`:654`) y sin comprobar `<catalogs>` sin rellenar (INV-PBT5-16) | Escribir el bloque entero, con su comprobación y `exit 0` |
| M-09 | MENOR | `smoke-run-v1.md:1039`; [P]`:505-511`; INV-PBT5-08 | **La custodia del texto de la corrida no pasa por los patrones de privacidad.**<br>• Las capturas tienen reglas (§8), pero `run-log.md`, `integrity.md` y `environment-*.md` solo redactan la carpeta de trabajo;<br>• el eco de `NETLOAD` lleva la ruta del worktree, que contiene el nombre del usuario, y un STOP del bloque 10 imprime la ruta del original;<br>• aparte: tras la redacción, [E] y `sim_collect.py` dejan ver el nombre de la carpeta de trabajo en sus literales partidos | Antes del commit de custodia: `patterns.local.txt` sobre todo lo que se custodie, y solo por sha256 lo que case, como [E] y F4B-E60.<br>No copiar al registro las órdenes con rutas |
| M-10 | MENOR | `smoke-run-v1.md:26`, `:882`; [H3]`:60`; [E]`:136-144` | No se fija dónde vive `<lista-local-de-la-seleccion>`. Si la sesión la deja en `pbt5/` con otro nombre que `_tools/*.local.txt`, [E] la barre: iría solo por sha256 (sus rutas casan), pero su nombre y su hash quedarían en la §292 | Fijarla fuera de `--src`, o como `_tools/<nombre>.local.txt` |
| M-11 | MENOR | `smoke-run-v1.md:834-837`, `:845-846`, `:862-863`; [P]`:440`; `ebd304a3:…/block-m/run-log-m.md:33` | **Los contextos construidos dependen de respuestas nativas sin verificar,** como declara [H3]:<br>• `-INSERT` con «1» «1» «0» supone escala no uniforme y sin atributos;<br>• `-PURGE` con nombres que llevan espacios: [BM] usó comodines `?`;<br>• `-LAYOUT`, `PSPACE` y `MODEL` se añaden a la lista de Q-PBT5-4 (es ejecución, no materia) y cambian `CTAB` y `TILEMODE` de la copia, que [P]`:440` llama «ninguna variable de sistema» | Regla escrita: si `RCDUMP CONSTRUCTED-construido` no muestra el estado esperado, o una respuesta no es la prevista, **STOP del paso y LIMITATION, sin improvisar órdenes**.<br>Aclarar que «variables de sistema» son las de la configuración, no las del dibujo de la copia |
| M-12 | MENOR | [P]`:298`; [OD5]`:74` | SPB-45 da «la falta de aviso» como OBSERVADO. Pero la pregunta del Owner ya la caracterizó: «sin avisar, igual que RACKEDITAR». OBSERVADO es para lo no caracterizado (§4.0) | Registrarla como hecho informado. PASS si coincide con la gemela |
| M-13 | MENOR | [P]`:580-582`; [OD5]`:14` | **PBT5-C17:**<br>• «no se cumple» tiene que leerse «tras resolver y recomprobar», porque el Owner escribió «0 importantes **sin resolver**»;<br>• una CI en rojo solo por el detector de cuelgue admite antes una reejecución (C15) | Escribir los dos matices en C17 |
| M-14 | MENOR | [P]`:611-627` frente a `smoke-run-v1.md:723`; [P]`:148` frente a `:539` y `:587`; [E]`:222`; [P]`:90`, `:193` | **Incoherencias de texto:**<br>• el `--dry` va antes del build en [P] §6.7 y después en [SR] §4.10. Las dos valen;<br>• la revisión de la evidencia, en `F4B-DRAFTS/pbt5/` (§3.3) o en `F4B-PBT5-SMOKE-PB-GATE-CLOSE/` (§6.2 y §6.5);<br>• la §292 dice «v1 a v2.3 del paquete y de las preguntas»: no hay preguntas v2.3, y `review-v1/independent-review-pbt5-v1.md` va solo por sha256 (casa 2 patrones);<br>• §2 dice «Medidos en `7da361eb`» con hechos de `ebd304a3`;<br>• el título de §4.1 dice «con AutoCAD cerrado», y P-09 necesita AutoCAD | Unificar |
| M-15 | MENOR | [E]`:257`, `:267`; `ebd304a3:docs/automation/state/I-64.yml:10`, `:72`; `ebd304a3:…/F4B-DRAFTS/f4b-dispositions.md:2435` | **La emisión borra del estado la nota de OWNER-01 de PB-T4e.** F4B-E61 la dejó «en el estado (`next_action` y `NEXT_ACTION` de `I-64.yml`)», y [E] reescribe `next_action` entero y `NEXT_ACTION` hasta la marca de F5-A. La nota sigue en [SR] (SPB-31 y SPB-36) | Conservar en los dos textos nuevos «OWNER-01 de PB-T4e (informativo), en SPB-31 y SPB-36» |

## 3. Lo comprobado

### 3.1 Las siete decisiones del Owner ([OD5]), aplicadas

| Decisión | Dónde | Resultado |
|---|---|---|
| **Q-PBT5-7,** «Sí, con condiciones» | [P]`:35`, `:611-629`; [E]`:107-127` | Literal: tres condiciones, y vuelve por una decisión nueva, una prueba que falle o un intento 3. La CI exacta, verificada en verde. C17, M-13 |
| **Q-PBT5-8,** «Adaptación con revisión» | [P]`:526-532`, `:563` (INV-07), `:580` (C15), `:632` | Forma B literal: contrato propio, sin Controller, sin nc1 a nc4 y sin D2, con la revisión de solo lectura antes de la firma. La regla de la CI, confirmada en C15. **El contrato no trae sus paradas: I-02** |
| **Q-PBT5-9,** «Definición y reglas» | [P]`:595-606`; `smoke-run-v1.md:46-54` | La definición y las tres reglas, literales. **C16 la amplía: I-04** |
| **Q-PBT5-1,** «Extender la dispensa» | [P]`:122`, `:198`, `:571`; `smoke-run-v1.md:77`, B2 `:227-276` | Literal. Los 19 filtros de B2 son, por `diff`, los mismos que los `RequiredTests` de UI de `ebd304a3:…/F4B-PBT4-EDIT-ALL-RELOAD/coordinator/gate-contract.json` |
| **Q-PBT5-10,** «Hacerlas como en Smoke-2» | [P]`:327-329`, `:469-476`; `smoke-run-v1.md:737-820` | Es el método de Smoke-2 (`run-log-2.md:153-166`): `ProcessStartInfo.Environment`, un AutoCAD por inyección, «Descartar», `QUIT` → «No» y la comprobación del usuario, la máquina y el shell.<br>Los nombres y los valores existen: `ebd304a3:src/RackCad.Plugin/Views/RackPilotUpdateWriter.cs:244-245` y `:339-341`; `before` y `after` se comparan `Ordinal`.<br>Solo actúan en Debug (`:363-380`), y el build es Debug. M-06 |
| **Q-PBT5-4,** «Construirlos y rotularlos» | [P]`:298-299`, `:325-326`, `:478-486`; `smoke-run-v1.md:822-873` | Los cuatro, en copias (c34 a c41), cada uno con su gemela y su rótulo (`CONSTRUCTED-` en `RCDUMP` y en las capturas). `-LAYOUT`, `PSPACE` y `MODEL` son ejecución (M-11). M-12 |
| **Q-PBT5-2,** «Contar antes en otros» | [P]`:205`, `:226`, `:488-493`; `smoke-run-v1.md:875-950` | El recuento va antes de SPB-07, en copias y en solo lectura. D2 ya está contado (`ebd304a3:…/block-m/inventories/m7-dimkeys.txt`: 8 sobres, todos `None`).<br>`dimkeys.py` lee el formato de `RCDUMP` (una línea `DEF` y el JSON en la siguiente) y no imprime nombres. **Faltan bloques exactos y la integridad final: I-01.** M-05 |

**Los dos puntos de [H3] que cambian lo literal:**
- **El punto 1 (SPB-07 vuelve al Owner) es correcto.** La opción elegida dice «si ninguno, te vuelve para aceptar el criterio cumplido en parte» ([OD5]`:90`). Ni NOT AVAILABLE ni fabricarlo.
- **El punto 2 (no se usa la v3 de nc2) es correcto.**
  - La decisión de F4B-E58 aplicaba la v3 «a este nc2 y a PB-T5» (`ebd304a3:…/coordinator/owner-pbt4d-nc2-decision.md:17` y `:27`).
  - Pero la forma B no tiene nc2, y la opción 3 de Q-PBT5-8, que el Owner no eligió, nombraba expresamente «nc2 con la v3» ([OD5]`:29`).
  - No hay conflicto ni decisión nueva.
- **El punto 3** (el Candidato `ebd304a3` y la emisión encima, con SPB-51 relajado) es correcto en el bloque 10, pero no en el contrato ni en la lista: I-02.

### 3.2 Los añadidos del autor

| Añadido | ¿Decisión material nueva? |
|---|---|
| `TaskId` `F4B-PBT5-SMOKE-PB-GATE-CLOSE` | No: un nombre, provisional desde la v2.2 |
| INV-PBT5-17 | No: es la opción 1 de Q-PBT5-2 frente a la 3 («Fabricarlo»), que el Owner no eligió |
| **PBT5-C16** | **Sí, tal como está escrita: amplía Q-PBT5-9 (I-04)** |
| PBT5-C17 | No: es más prudente que la autorización. M-13 |
| Las 44 copias | No: cumplen «una copia por escenario». El reparto de c01 a c44 cuadra con los 65 pasos (`smoke-run-v1.md:379-411`) |
| La primera presentación (`(car (layoutlist))`) | No: es ejecución |
| Las inyecciones al final, cada una en su AutoCAD | No: es orden, y cumple «ninguna otra instancia de la sesión viva» |
| La extracción de `dimkeys.py` | No: es herramienta. El blob es el mismo en `7da361eb` y en `ebd304a3` (`57decb54…`). M-04 |
| La emisión en un solo commit (S-12) | No: es la forma, y está declarada |

### 3.3 `smoke-run-v1.md`

**Las correcciones de [IR1], conservadas:**

| Corrección | Dónde está ahora |
|---|---|
| I-07 (el paso de copia, sin COM) | §4.7, `:570-631` |
| I-08 (cada bloque funciona solo) | Los 20 bloques |
| I-09 (los `Ticks`) | Bloques 4b a 9 y 11 y 12 |
| M-18 (sin sobrescribir; el hash de D1 antes del respaldo) | `:343` antes de `:357` |
| M-20 (la línea de base) | `:516-520` |
| M-21 y M-28 (los controles) | `:551-560` |
| M-22 (la cabecera) | `:1017-1022` |
| M-26 (la carpeta de trabajo) | `:329-353`; `RCDUMP` y `rcdynchk` no sobrescriben |
| M-27 (el PID) | El bloque 9 |
| M-31 (B1, B2 y B3) | `:200-310` |
| M-32 (el registro justo al lanzar) | `:456-459` |
| M-33 (`$rc` y los SHA completos) | El bloque de P-06, los bloques 1 y B3 |
| M-34 (los `throw`) | Bloques 3, 8 y 10 |
| M-35 (la ruta de `acad.exe`) | `:443-444` y `:759-760` |

M-29 (J opcional) ya no aplica: el bloque J es obligatorio en la corrida contratada.

**Los 20 bloques de PowerShell** (`ps_audit-out.txt`, PowerShell 7.7.0, sin ejecutarlos):
- 0 errores de sintaxis;
- todos empiezan por `Set-StrictMode -Version Latest` y `$ErrorActionPreference = 'Stop'`;
- todos terminan con un único `exit 0`;
- ninguna variable libre;
- los que tienen marcadores `<…>` los comprueban, y el 8 exige `^c\d\d$`.

Falta como bloque exacto: la biblioteca (M-08) y lo de I-01.

**Los originales y las copias:**
- **Ningún bloque abre un original:** solo `Copy-Item` y `Get-FileHash`.
- **AutoCAD solo recibe copias:** el bloque 5 exige `cNN.dwg` en la carpeta de copias, el 11 exige c42 a c44, y los `vla-open` tecleados apuntan a la carpeta de trabajo.
- **Nada sobrescribe:** `Test-Path` previo y `-NoClobber`.
- **El respaldo va antes de las copias:** en P-06, `:357` antes de `:364`; en el bloque 13, `:898` antes de `:900`.

**El build:** B1, B2 y B3 paran si hay un `acad` o un `accoreconsole`, y ninguno cierra nada. El bloque 9 solo cierra la instancia registrada, comprobada por su nombre, su ruta, sus `Ticks` y su título, y nunca usa `Stop-Process`.

**La configuración:** la lista de efectos de `ps_audit-out.txt` no tiene ninguna escritura en el registro ni en el entorno, ni `SetEnvironmentVariable`. NETLOAD es por expresión, con «Load Once». El LISP se teclea, sin `load`.

**Las inyecciones:**
- la variable solo va en `$psi.Environment`, con `UseShellExecute = $false` (`:778-780`);
- se comprueba antes: el usuario, la máquina, el shell y el entorno heredado (`:763-766`, `:779`);
- se comprueba justo después de `Start` (`:784-786`) y tras la salida (bloque 12, `:815-817`);
- ninguna instancia de la sesión puede estar viva (`:767-771`).

**Los contextos construidos:** las órdenes nativas se teclean solo en c34 a c41 (M-11).

**El recuento:** es de solo lectura sobre copias. `RCDUMP` solo escribe en la carpeta de inventarios. Ver I-01.

**La frontera:** I-03.

**Las capturas:** §8 conserva las reglas (recorte, rectángulo opaco, historial de órdenes tapado, lista previa, `derivation-record.txt` sin rutas). Lo que falta es para el texto: M-09.

**El LISP** (`lisp_audit-out.txt`):
- 22 líneas, la más larga de 935 caracteres, con los paréntesis y las comillas cerrados;
- `rcj`, `rcn`, `rcref` y `rcdef` son iguales a `7da361eb:…/block-m/tools/lisp_src.txt`;
- los 18 literales de la interfaz que miré están en `src/` de `ebd304a3`, entre ellos «- frente», «Insertar lateral», «Comprobar ahora», «Descartar borradores de este documento», el de `LOCKED_LAYER: ` y el del último vínculo.

**El SHA-256 de D1** (`7A868624…47A5`) es el registrado (`ebd304a3:…/F4-SMOKE-2/run/integrity.md:12` y `:60`).

### 3.4 `emit_pbt5.py`

**Lo que escribe:**
- el listado del `--dry` y el mensaje, en `--out`, con `"x"`;
- las copias redactadas y `contract.md`, con `"xb"` o `"x"`;
- añade F4B-E62 y la §292 al final de DISP y de EVID;
- `I-64.yml`;
- después, `git add` de esas rutas, la comprobación de que no hay ningún cambio fuera de ellas, el commit y `push` sin forzar a la rama de I-64.

No toca `main`.

**La numeración:**
- F4B-E61 está en `ebd304a3:…/f4b-dispositions.md:2403` y la regex de [E]`:98` casa;
- la última sección de la evidencia es la `## 291.` (`I-64-evidence.md:9022`);
- no existen F4B-E62 ni la §292.

**Antes de escribir:**
- comprueba el árbol, la rama, HEAD = Candidato, la rama remota, la CI, la línea `BOUNDED_RESULT`, los destinos y la longitud de las rutas, en `--dry` y otra vez en `--commit`;
- y que el plan sea el del `--dry`.

Lo que va después de escribir: M-01.

**La redacción y la privacidad** (`sim_collect-out.txt`):
- 71 archivos van como copia (65 sin contar los de esta comprobación, en `review-v23/`, que también entran) y 4 solo por sha256: `sha256-v3.txt`, `_tools/make_smoke_run_v1.py`, `review-v1/check_lisp_v2.py` y `review-v1/independent-review-pbt5-v1.md`;
- el paquete, las decisiones del Owner y [SR] van como copia;
- la ruta de destino más larga mide 91 caracteres (el máximo es 156);
- la redacción solo sustituye la carpeta de trabajo; lo demás lo excluyen los patrones locales, sin taparlo;
- en lo que va como copia no queda ninguna ruta con letra de unidad, y ningún nombre de los patrones.

**El fin de línea:**
- los blobs de DISP, EVID e `I-64.yml` son LF (0 CR);
- con `core.autocrlf=true`, leer con saltos universales y escribir LF solo deja en el diff las líneas nuevas.

**La no atomicidad:** está acotada de hecho (M-01), pero no como dice [H3].

**SPB-51 relajado** (`smoke-run-v1.md:707-710`): es correcto.
- `merge-base --is-ancestor` y el diff vacío en `src`, `tests` y `assets` bastan, porque el DLL se compila antes de la emisión y su SHA-256 se comprueba en el proceso (bloque 7).
- Los archivos de la raíz, como `global.json`, no le afectan.
- Lo incoherente es el contrato: I-02.

### 3.5 El contrato (§6)

| Parte | Resultado |
|---|---|
| El alcance (§6.2) | Correcto. Prohíbe `src/`, `tests/`, `assets/`, `main`, la guía [G], HANDOFF, ROADMAP, los originales y la configuración |
| Los invariantes (§6.3) | Completos y coherentes con las decisiones |
| Las paradas (§6.4) | **I-02 e I-04**; M-13 |
| La evidencia (§6.5) | Correcta, con la incoherencia de M-14 |
| El presupuesto y la definición de intento (§6.6) | Literales de Q-PBT5-9, salvo la ampliación de C16 (I-04) |
| La cadena (§6.7) | Correcta. El orden del `--dry`, M-14 |

### 3.6 ¿Hace falta una decisión nueva del Owner?

**No, si se corrigen I-03 e I-04 por el lado estrecho:**
- la lectura literal de la frontera (listo antes de las 12:00Z);
- y C16 que devuelve al Owner lo que cambia un resultado o exige AutoCAD.

**Sí, si se quiere conservar alguna de las dos como está:**
- la franja de 12:00 a 13:00Z es la Q-PBT5-5, nunca preguntada;
- C16, como está, amplía Q-PBT5-9.

I-01, I-02 y los menores son de ejecución y de texto.

## 4. Lo no verificado

- **Nada en AutoCAD:**
  - las respuestas de `-LAYOUT`, `-INSERT`, `-BLOCK`, `-LAYER`, `ERASE` y `-PURGE` en estas copias;
  - `QUIT` en las instancias de inyección;
  - `rcdyn` y `rcdynchk`;
  - `vla-item` por nombre;
  - `WaitForInputIdle`;
  - `StartTime` justo tras `Start`.
- **Que `git hash-object` normalice el CRLF** con `core.autocrlf=true` (M-04): es el comportamiento documentado de git, no lo ejecuté.
- **Que la redirección `>` de PowerShell 7.7 conserve los bytes** de `git show`: tampoco la ejecuté.
- **Que `gh run list --commit` exista** en la versión de `gh` de la sesión (bloque 1).
- **La lista local de la selección y los originales:**
  - qué dibujos de la selección de Smoke-2, aparte de D1 y D2, tienen racks Push Back hoy;
  - dónde están los originales y si coinciden con sus hashes de Smoke-2.
- **Que la herramienta deje correr en segundo plano** órdenes de más de 10 minutos.
- **`emit_pbt5.py`** no se ejecutó, ni en `--dry`. Su comportamiento se dedujo leyendo el código y con la simulación de `collect()`.
- **A qué se refería el Owner con «se complete»** (I-03).
- **Los totales de la CI:** los leí del registro de la corrida `45147867500`. No miré los nombres de las pruebas saltadas.

## 5. Para la recomprobación

Comprobar en las versiones nuevas:
- I-01 a I-04 corregidos;
- [E] apunta al paquete nuevo y a su título de §6;
- la línea `BOUNDED_RESULT` del informe nuevo lleva los sha256 que [E] verifique (M-02);
- la carpeta `review-v23/` entra en la custodia por estar dentro de `--src`. Sus archivos no llevan rutas locales ni nombres. `sim_collect.py` repite, partidos, los literales de la carpeta de trabajo que ya lleva [E] (M-09).

**La línea de resultado de esta comprobación,** sustituida por la de la Recomprobación 1, al final del informe. Se cita aquí sin que la línea empiece por la clave:

> Resultado de CA-T5 (v2.3), sustituido: `BOUNDED_RESULT: EMITIBLE TRAS CORRECCIONES; B 0 / I 4 / M 15; I-01 P-09 y SPB-07 sin bloques exactos ni integridad final; I-02 contrato con paradas por referencia a la v2.2 y regla HEAD contradictoria; I-03 franja 12-13Z sin confirmar (Q-PBT5-5 no preguntada); I-04 PBT5-C16 amplia Q-PBT5-9; revisado paquete 02820249, guia 24489598, guion 654dd728; CI 45147867500 4/4 success verificada`

## Recomprobación 1 (v2.4, solo el diff)

**Hora:** 2026-11-23, de 10:25Z a 10:55Z aproximadamente. **Reglas:** las del primer encargo.

**Objeto,** con los sha256 del encargo, medidos:

| Sigla | Documento | sha256 |
|---|---|---|
| [P4] | `I-64-f4b-pbt5-package-v2.4.md` | `6bb02ca2af29c42fb71c5cd629dc2cdc98958d83c822e98e722540bef7d049b7` |
| [SR2] | `smoke-run-v2.md` | `11bcaf105e6c74b9f28c4695fae614814cafced259620dc6b2b7ecb6c3d60d59` |
| [E2] | `_tools/emit_pbt5.py` | `940d6ddae79eb962f0bb80571de72aef9c2351e1657333f83982ecc4eb617f7e` |
| [H4] | `handoff-pbt5-v4.md` | `cebfe9fe166bcef13e3dc4894fbed6710d57650403a207ed85fa0cdafdd9661d` |
| — | los tres diffs | `8cce6789…0c4a`, `7a5d2fc2…b445` y `8bbdcb79…a424` |
| — | `_tools/emit_pbt5-v1.py` | `654dd728…a25ca33`: es el guion revisado en CA-T5, sin cambios |

**Método:**
- lectura entera de los tres diffs, de [H4], de [E2] y de `test_emit_pure.py`, `harness_sr2.ps1`, `scan_custody.py` y los dos `probe_selection_*.py`. No los ejecuté;
- `ps_audit.ps1` sobre [SR2] (`ps_audit-r1-out.txt`) y `lisp_audit.py` (`lisp_audit-r1-out.txt`);
- `sim_collect.py` sobre la carpeta del paquete (`sim_collect-r1-out.txt`);
- **la lista local:**
  - `probe_selection_r1.py`, que solo imprime la forma;
  - `leak_scan_r1.py`, que busca en toda la carpeta del paquete los trozos de la lista y solo imprime cuántos;
  - la salida de ninguno de los dos se custodia: solo fue a la consola;
- en `ebd304a3`, por objetos git: los marcadores nuevos de `I-64.yml` y `git grep` de `BlockScaling` y `AttributeDefinition` en `src/`;
- `check_report.py`: una copia de las reglas de `check_bounded()` v1 y v2, para comprobar este informe sin importar [E2].

### R1.0 Veredicto

**EMITIBLE. B 0 / I 0 / M 2.**
- Los cuatro importantes están resueltos, I-03 e I-04 por el lado estrecho. **Ninguno necesita una decisión del Owner.**
- Los 15 menores están resueltos, o aceptables como se aplicaron.
- Lo nuevo son dos menores (M-R1 y M-R2), que no impiden emitir.
- **La frontera, en la práctica:** a las 10:41Z quedaban 79 minutos para las 12:00Z. Para que la sesión opere sola, la emisión y P-01 a P-08 tienen que estar hechas antes de esa hora; si no, se espera al Owner ([P4]`:85`, `:481-485`, `:625`).

### R1.1 Los importantes

| Id | Estado | Evidencia |
|---|---|---|
| I-01 | **Resuelto** | **Los bloques nuevos o ampliados de [SR2]:**<br>• **el 13** (`:1057` y siguientes) lee la lista por su ruta, con su SHA-256 `DD3613CB…F2C1` comprobado; exige siete entradas; salta las de D1 y D2; comprueba todos los originales y destinos antes de escribir; hace el respaldo antes de la copia; y registra, sin parar, si cada original coincide con la lista y con O1 a O7;<br>• **el 13b** (`:1168`) hace `s01` y `s02` en una carpeta nueva, sin sobrescribir, frente a `estilo.json`, y vuelve a comprobar el original;<br>• **el 8** (`:654`) vale para `cNN`, `eNN` y `sNN`, cada una frente a su manifiesto;<br>• **el 10** (`:754`) mide al final la lista, los originales de P-09, sus respaldos y copias, y `s01`/`s02`;<br>• **el 5 y el 9** aceptan las tres clases.<br>**En [P4]:** P-09 (`:229`) y SPB-51 (`:338`) |
| I-02 | **Resuelto** | El §6 de [P4] (`:556-691`) trae 17 invariantes y 17 paradas en tabla, sin ninguna referencia a la v2.0, v2.1, v2.2 ni v2.3. PBT5-C01 (`:615`) separa antes y después de la emisión. La fila SPB-51 (`:338`) va igual. `contract.md` se basta solo |
| I-03 | **Resuelto por el lado estrecho** | La sesión opera sola solo si la emisión y P-01 a P-08 están hechas antes de las 12:00Z, y para a las 13:00Z. Está en:<br>• [P4] §0 (`:85`), «Listo para empezar», §5.4 (`:481-485`), PBT5-C11 (`:625`), R-1, S-1 (`:705`) y §9 (`:715`);<br>• [SR2] §2 (`:79-90`), §3, §4.10, §5, §6 y §7;<br>• [E2], `FRONTIER` (`:69-70`), en F4B-E62, `next_action` y `NEXT_ACTION`.<br>Empezar en la franja de 12:00 a 13:00Z sin el Owner queda escrito como lectura amplia, que exigiría Q-PBT5-5. **No hace falta decisión del Owner** |
| I-04 | **Resuelto por el lado estrecho** | PBT5-C16 (`:630`): la sesión solo corrige transcripciones; lo que cambie un resultado o pida AutoCAD vuelve al Owner. §6.6 (`:652`): solo la adenda que pida el Owner no cuenta. Lo recogen también [SR2] §1 y §6 y `REMAINING_BUDGET` en [E2]. Es literal de Q-PBT5-9 ([OD5]`:40`). **No hace falta decisión del Owner** |

### R1.2 Los menores

**Lo aplicado:**

| Id | Estado | Evidencia o razón |
|---|---|---|
| M-01 | Resuelto | `plan()` hace todo antes de escribir (`:394-411`): calcula `yml_new` sobre `HEAD:` y comprueba que no existan el contrato ni el mensaje. En `--commit`, `yml_new` sobre el disco tiene que dar el mismo sha256 antes de la primera escritura (`:453-456`). El procedimiento de estado parcial está en la cabecera (`:25-30`) y en [P4] §6.7. Lo que queda (un fallo de disco o de git a mitad) está acotado y declarado |
| M-02 | Resuelto | `check_bounded` (`:166-184`): el token exacto `EMITIBLE;`; `B 0 / I 0` seguido de `/`, `;` o fin de línea; y una sola vez cada prefijo de 8 caracteres de `paquete`, `guia` (o «guía») y `guion` (o «guión»), comparado con los sha256 reales. Además, el guion tiene que ser el `_tools/emit_pbt5.py` de `--src` (`:116-118`) |
| M-03 | Resuelto | El paquete, la guía y el registro del Owner se exigen como copia (`:222-224`). Hoy van como copia (`sim_collect-r1-out.txt`) |
| M-04 | Resuelto | `hash-object --no-filters` y `dimkeys.py` desde el Candidato ([SR2]`:1144-1147`). El blob es el mismo en `7da361eb` y en `ebd304a3` (`57decb54…`, comprobado en CA-T5) |
| M-05 | Resuelto | c01, «P-09 (D1)» (tabla de §4.3, §4.10 y §5) |
| M-06 | Resuelto | [SR2]`:963-965` y [P4]`:352-353`: «Comprobar ahora», sin «Actualizar». En SPB-61, «Descartar y recargar» → «Sí», como el paso 84 de Smoke-2 |
| M-07 | Resuelto | El bloque 15 (`:835`) es de lectura: las líneas `ERROR`, los eventos de acad desde el primer `acad-session` y `settings.json` frente a P-05. El bloque 4 da el SHA-256 |
| M-08 | Resuelto | El bloque B4 (`:342`) es entero, con StrictMode, la comprobación de `catalogs` y `exit 0`. Lee `BlockLibraryPath` con `PSObject.Properties`, que es la misma resolución que [G] §7.1 y no falla con StrictMode |
| M-10 | Resuelto | La lista vive en la carpeta de la sesión del cierre de PB-T4e, fuera de la carpeta del paquete. [E2]`:203-204` para si encuentra en `--src` un `*.local.*` que no sea `_tools/*.local.txt`, y hoy no hay ninguno |
| M-12 | Resuelto | [P4]`:322` |
| M-13 | Resuelto | [P4]`:631` |
| M-14 | Resuelto | Un solo orden (`--dry`, el build, `--commit`) en [P4] §6.7 y en [SR2] §4.10. La revisión de la evidencia, en `F4B-PBT5-SMOKE-PB-GATE-CLOSE/`. §2 cita las dos bases; el título de §4.1, corregido; la §292 calcula la historia (`history_text`) |
| M-15 | Resuelto | `OWNER01` (`:67-68`) va en `next_action` y `NEXT_ACTION` (`:363` y `:378`) |

**Lo aplicado distinto o no aplicado, con mi juicio:**

| Id o punto de [H4] | Juicio |
|---|---|
| **M-09** | **Aceptable.** La regla del registro está en [SR2]`:1298-1304`; `scan_custody.py` es de solo lectura, no imprime lo que casa y también mira los nombres de archivo; la regla está en [P4]`:543-545`.<br>Los literales partidos de la carpeta de trabajo en [E2] y en `review-v23/sim_collect.py` se quedan, por la razón de [H4] §3.1: es la carpeta de trabajo, no un dato personal, y ningún archivo casa un patrón |
| **M-11 (`-INSERT`)** | **Aceptable.** Q-PBT5-4 dice «órdenes nativas tecleadas», y cambiar `-INSERT` por ActiveX se saldría de lo literal. La regla de parada ([P4]`:514`; [SR2]`:998-1003`) da el estado esperado de cada contexto, lee el bloqueo con `tblsearch`, usa el patrón con `?` como [BM] M-2b y comprueba que no se purgó de más.<br>**El riesgo es bajo:** `git grep` en `src/` de `ebd304a3` no encuentra `BlockScaling` ni `AttributeDefinition`, así que las definiciones de vista van con la escala libre por defecto y sin atributos propios. Entonces `-INSERT` pide la X, la Y y el giro, como suponen «1» «1» «0». Si no, la regla para |
| **El bloque J sin la regla de las 12:15Z** | **Aceptable.** La regla era de la corrida previa, donde J era opcional. En la contratada, J está entre los 65 pasos, y la frontera la gobiernan PBT5-C11 y las 12:00Z. SPB-51 y SPB-52, que leen, se hacen siempre |
| **`yml_new` cambia la frase de `f4b_status`** | **Aceptable,** de parada: la frase está una vez en `ebd304a3:docs/automation/state/I-64.yml:57` (medido). Ninguno de los valores que se tocan lleva comillas ni barras invertidas, y `setval` para si los llevara |
| **El bloque 4 lista los tres últimos `rackcad-*.log`** | **Aceptable:** es de solo lectura, con el nombre, el tamaño y la fecha |
| **Si varias `eNN` guardan estilo, la primera** | **Aceptable** como convención declarada ([P4] §5.7): Q-PBT5-2 dice «se prueba con él». Ver M-R2 |
| **Un estilo en `c01-pre` (D1) es un OBSERVADO** | **Aceptable,** y más prudente: no se hacen copias de D1 sin bloque exacto, y SPB-07 vuelve al Owner |
| **El guion reescrito en su sitio** | **Aceptable.** El revisado queda en `_tools/emit_pbt5-v1.py` con su sha256 de CA-T5 (`654dd728…`), y el nuevo exige ser el de `--src` |

### R1.3 La lista local de la selección (pregunta 3)

**La lista** está en la carpeta de la sesión del cierre de PB-T4e, que es hermana de la carpeta del paquete y no la barre la emisión. Su sha256 es el que fijan [SR2] y los bloques 10, 13 y 13b (`dd3613cb…`).

**Su forma,** sin imprimir rutas (`probe_selection_r1.py`):
- `D1` y `Selection`, con siete entradas de `Source`, `Sha256`, `Roles` y `Exists`;
- los `Roles` son solo etiquetas de papel de Smoke-2;
- cada `Sha256` es uno de los O1 a O7 de `F4-SMOKE-2/run/integrity.md`;
- todos los `Exists` están en verdadero;
- ningún trozo de `Source` aparece en `Roles` ni en `Sha256`;
- el bloque 13 contará 5 entradas: salta la de D1 y la que lleva D2, que es el mismo dibujo que D3b y D5-04.

**Lo que se escribe sale de la lista sin su `Source`:**
- `estilo.json` lleva `lista_sha256` y, por copia, `copia`, `indice`, `papeles`, `sha256`, `igual_a_la_lista` y `original_de_smoke2`;
- `s-copias.json` lleva `copia`, `origen` (la `eNN`), `papeles` y `sha256`;
- las líneas que imprimen los bloques 13, 13b y 10 nombran la `eNN` y sus papeles;
- los STOP escritos a mano no nombran rutas de originales.

**La emisión** solo recorre `--src`, y para con cualquier `*.local.*` fuera de `_tools/*.local.txt`. En `--src` busqué los 29 trozos que deja la lista: cada `Source` entero, su nombre de archivo, su nombre sin extensión y sus carpetas de 5 caracteres o más. **0 de 96 archivos contienen alguno** (`leak_scan_r1.py`).

**El registro:** [SR2] §7 no copia órdenes con rutas ni nada de la lista, y `scan_custody.py` pasa los patrones locales (rutas con letra de unidad, la carpeta sincronizada en la nube, el usuario y demás) antes de custodiar. Lo que queda: M-R1.

**`harness_sr2.ps1`,** que se custodia, repite el orden de los papeles de la lista con archivos falsos. Son etiquetas públicas: su correspondencia con O1 a O7 ya está en `integrity.md:7` y `:54-60`. No lleva rutas ni nombres, así que no es un hallazgo.

### R1.4 `emit_pbt5.py` v2 (pregunta 4)

**`--package` y `--guide`** (`:102-119`):
- los nombres se validan con su patrón y tienen que estar en `--src`;
- el guion tiene que ser el `_tools/emit_pbt5.py` de `--src`;
- los sha256 del paquete, de la guía y del propio guion se comparan con la línea de resultado (`:398-401`);
- el contrato sale del paquete dado, y para si su §6 o su §7 no aparecen una sola vez (`:228-242`).

**Todas las comprobaciones van antes de escribir.** En `--dry` y en `--commit`: el árbol, la rama, HEAD = Candidato, la rama remota, la numeración, la CI, la línea de resultado, los destinos, la longitud de las rutas, las copias exigidas, los patrones del contrato, la inexistencia del contrato y del mensaje, y `yml_new` sobre `HEAD:`. En `--commit`, además, el listado igual al del `--dry` y el `I-64.yml` del disco igual al calculado. La primera escritura es la `:468`.

**El token `EMITIBLE;`:** la línea de abajo la aceptan las reglas v2 copiadas en `check_report.py`, y la rechazarían con «TRAS CORRECCIONES», con otro sha256 o con un importante. No ejecuté [E2].

**Lo demás:**
- `push` sin forzar a la rama de I-64; `main` no se toca;
- la numeración, F4B-E62 y la §292, sin cambios;
- la redacción: 89 archivos como copia, contando este informe y sus herramientas, y 4 solo por sha256 (los mismos de CA-T5); la ruta de destino más larga mide 91 caracteres.

### R1.5 Lo nuevo

| Id | Gravedad | Dónde | Qué | Arreglo |
|---|---|---|---|---|
| M-R1 | MENOR | [SR2]`:1135-1158` (bloque 14) y `:1298-1304`; bloques 10, 13 y 13b; [P4] P-09 («sin nombres») | **Privacidad residual del recuento:**<br>• `dimkeys.py` imprime el **valor** de un `DimensionStyle` no nulo, que es un nombre de estilo de un dibujo de cliente. Los patrones locales no cubren nombres de clientes, y §7 no lo nombra entre lo que el registro no copia;<br>• un error de `Copy-Item` o de `Get-FileHash` (un acceso denegado, un archivo sincronizado en la nube sin descargar) imprime la ruta del original, aunque los STOP escritos a mano no la lleven. §7 cubre «un STOP que nombre una ruta», y `scan_custody.py` cazaría la ruta, pero no un nombre | Añadir a §7: al registro custodiado van el número de sobres con estilo no nulo y la `eNN`, no el nombre del estilo; y los errores de los cmdlets, sin su texto. Las dos cosas, solo en el registro local |
| M-R2 | MENOR | [P4] §5.7; [SR2]`:1160-1166` | **SPB-07 con «la primera `eNN` que lo guarde»:** si el rack con estilo de esa `eNN` no lo admite el panel (en la selección de Smoke-2 hay un rack divergente y uno antiguo), SPB-07 (b) («Editar todo» → «Actualizar») no se puede hacer, aunque otra `eNN` sí valdría | Tomar la primera `eNN` cuyo rack con estilo admita «Editar». Si ninguna, SPB-07 (a) en la primera, y (b) al Owner con su evidencia |

### R1.6 Lo no verificado

- **No ejecuté** [E2], `test_emit_pure.py`, `harness_sr2.ps1` ni `scan_custody.py`. Sus resultados son los de [H4] §4, no reproducidos: entre ellos, el arreglo de `$ce` en el bloque 10 y la medida de la redirección `>` en PowerShell 7.7.
- **Nada en AutoCAD:** las respuestas nativas de §4.12, `QUIT`, `rcdyn`, `vla-item`, `WaitForInputIdle`, y si `-LAYOUT _S` acepta el nombre de `(layoutlist)`.
- **Los originales de la lista:** si siguen hoy en sus rutas, cuáles tienen racks Push Back, y si leer uno de la carpeta sincronizada en la nube lo descarga o falla.
- **Que la lista siga en la carpeta de la sesión** hasta el bloque 10: está en el scratchpad, y una limpieza la borraría. Los bloques 10, 13 y 13b pararían.
- **Que `gh run list --commit`** exista en la versión de `gh` de la sesión.

BOUNDED_RESULT: EMITIBLE; B 0 / I 0 / M 2; recomprobacion 1 (v2.4, solo el diff): I-01 a I-04 resueltos, I-03 e I-04 por el lado estrecho, sin decision nueva del Owner; M-01 a M-15 resueltos o aceptados; nuevos M-R1 y M-R2, menores; revisado paquete 6bb02ca2, guia 11bcaf10, guion 940d6dda; CI 45147867500 4/4 success
