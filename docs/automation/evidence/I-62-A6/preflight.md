# I-62 A-6 — Preflight (decisiones §76, puntos 2 y 5)

> Supervisión, plano a. **Preparación, no aplicación:** ni AGREED, ni revisión formal, ni invocación, ni medición (decisiones §76, puntos 2, 5 y 6).
> Todo se ejecutó en solo lectura sobre el worktree de I-62 (A-6 preparada sobre `1652dd61`; HEAD en el momento del preflight, `108d4ca1`, un commit
> posterior de F7 que no toca ninguna superficie de A-6) y en clones temporales propios bajo `D:\r62-a6-tmp` (`--bare --shared`, sin remotos),
> borrados al terminar. `D:\r62-fixture` no se tocó. Ninguna invocación de modelo ni de CLI de modelos; sin red. Ruta del worktree:
> `%USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal` (los guiones la derivan de `USERPROFILE`).

**Objeto:** `docs/initiatives/I-62-A-6.md`, blob `eac473c0d2e58cf1c4594f48a8816b3833af6509` (69 226 bytes, LF, SHA-256
`a76d13d68e18c9451319ba90de02af2cc3216c297bd2a9f8509b72e680f088b9`). Guardas: `a6-guards.py`, blob `c5cc6168d4a29f94623a8fab90a2d4f5f8df9ffa`.
Los blobs de los textos se calculan con CRLF → LF, como la conversión de entrada de `core.autocrlf=true`.

**Veredicto del preflight: PASS** en los apartados (a)-(f). (g) y (h) son informes. Las salidas exactas están en `out/` y en los JSON del paquete.

| Apartado | Resultado | Cifras | Salidas |
|---|---|---|---|
| (a) requisitos de decisiones §76, punto 5 | **PASS** | 10/10 cubiertos en el texto y comprobados por una guarda | §1 |
| (b) ocho puntos del dictamen | **PASS** | 8/8 literales de `A6.MinimalDeltaScope`, cada uno con al menos un delta; 9/9 deltas con su línea «Cubre» coherente | §2; G4 |
| (c) guardas y autoprueba | **PASS** | controles 9/9 PASS; **83/83 mutantes muertos**, ninguno superviviente, ninguno sin aplicar | `a6-selftest.json` |
| (d) negativos de extremo a extremo | **PASS** | **14/14** como se esperaba (1 positivo, 10 de archivo, 3 de commit) | `out/neg_controls_a6.json` |
| (e) preview | **PASS** | HEAD: G2 NOT_RUN y el resto PASS; commit candidato hipotético: 9/9 PASS; C-20b EQUAL | `out/a6-guards-result-preview-head.json`, `a6-guards-result-preview.json`, `out/candidate-commit.json` |
| (f) worktree de I-62 sin cambios | **PASS** | HEAD, refs, estado con ignorados e índice iguales antes y después; temporales borrados | `out/worktree-digest.json` |
| (g) consecuencias OWNER-RESERVED | informe | 2, separadas: (a) ADR-0046 #9 antes del acuerdo; (b) consumo de una medición | §5 |
| (h) ruta hacia C-24 | informe | condicionada; no demostrada hoy | §6 |

## 1. (a) Requisitos de decisiones §76, punto 5

| Requisito | Dónde en A-6 | Guarda |
|---|---|---|
| texto normativo exacto | §3.4-§3.12: nueve párrafos citados, cada uno con su ubicación exacta; añadido puro sobre V14 + A-2 + A-4 + A-5 | G5 |
| matriz M-01..M-08 | §5: M-02, M-03, M-04, M-05 y M-08 sí; M-01, M-06 y M-07 no; columna Dictamen igual al dictamen | G4 |
| referencias a los ocho puntos | §3.2 (literal, en orden) y líneas «Cubre» de cada delta | G4 |
| guardas mecánicas | §6.3; `a6-guards.py` (G1-G8 y G5b) | G5b |
| casos negativos y mutantes | 83 mutantes en memoria; 14 controles por la línea de órdenes | G5b; `neg_controls_a6.py` |
| compatibilidad con A-1..A-5 | §6.2, una fila por enmienda, sin conflicto abierto | G4 |
| delimitación de los insumos canónicos | §3.3 y A6-1, regla 5: archivo del instrumento por blob, declaración del intérprete y cierre; nada más | G5, G7 |
| identidad por blob de las herramientas | §3.3: blob = `git rev-parse <base>:<ruta>` = commit del paquete = dictamen; SHA-256 del contenido | G7 |
| efectos sobre D.5 y la futura OV | §3.15 y A6-6, regla 3: D.5 excluida; OV-I62-03 bajo la obligación de decisiones §73, punto 3 | G5 (A6-6, A6-8) |
| alcance exacto de las decisiones del Owner | §7: (a) y (b) separadas, con alcance y exclusiones; dos líneas exactas | G6 |
| no presentar A-6 a acuerdo con OWNER-RESERVED sin disponer | cabecera (Owner decision) y §11 | G8 |

## 2. (b) Ocho puntos del dictamen y deltas

| Punto | Delta | Ubicación en V14 |
|---|---|---|
| 1 ruta del Controller con el instrumento fijado por blob | A6-1 | fin de D.3, tras A5-2 |
| 2 admisión en `CanonicalInputs` y `AllowedActions` con su fuente | A6-2 | fin de D.3 |
| 3 vía del blob hasta el `-C` sin F_seed, F_norm ni el worktree real | A6-3 | fin de D.6 |
| 4 fidelidad y cobertura P-22 de las lecturas del instrumento | A6-4 (y A6-9) | fin de D.3 |
| 5 fallos cerrados y REJECT_FACTS frente a `Checks`, `Classification` y D.4 | A6-5 (y A6-9) | fin de D.4 |
| 6 oráculo de C-24 sin el instrumento ni el aceptante | A6-6 (y A6-9) | fin de D.4 |
| 7 elegibilidad por una invocación medida con consumo del Owner | A6-7 | fin de D.3 |
| 8 ADR-0046 #9 como determinación OWNER-RESERVED acotada | A6-8 | fin de D.3 (último párrafo) |

A6-9 (independencia; decisiones §76, punto 3) cubre además las condiciones del dictamen sobre Sesión, Contexto, oráculo y diversidad entre
instrumento y aceptante.

## 3. (c)-(e) Guardas, autoprueba, negativos y preview

**Autoprueba** (`a6-selftest.json`, base `108d4ca1`): controles G1-G8 y G5b en PASS; **83/83 mutantes muertos**, cada uno con un hallazgo nuevo del
motivo esperado:

| Guarda | Mutantes muertos | Ejemplos |
|---|---|---|
| G1 cabecera, literales, citas, identificadores y privacidad | 16/16 | literal de V14 L2523 cambiado; referencia A-4 L203 cambiada; ancla V14 L2587 alterada; prefijo de blob falso; 40 y 64 hexadecimales sin fijar; ruta de perfil, correo e identidad del host |
| G2 rutas del commit | 8/8 | ruta de producción; V14 modificada; evidencia de I-62 modificada en el mismo commit; archivo del paquete modificado; blob de A-6 distinto |
| G3 superficies prohibidas, objetos congelados y C-20b | 9/9 | V14, instrumento, árbol agent-execution o AUTOMATION_PLAN cambiados; dictamen cambiado; superficie retirada de la lista de §6.1; C-20b distinto de EQUAL |
| G4 dictamen | 10/10 | un punto retirado; texto de un punto alterado; punto reasignado; «Cubre» incoherente; M-02 = no; M-06 = sí; fila M retirada o duplicada; compatibilidad sin A-3; una sola consecuencia OWNER-RESERVED |
| G5 pureza del delta | 18/18 | A6-3 en D.5; orden de D.3 alterado; tope 16; fila de tabla; nombre de producto; sin cláusula de alcance; receta modificable; presupuestos ampliados; tope de medición 2; bloque agotado reutilizado; instrumento en F_seed; oráculo con el instrumento; aceptante en la sesión de Principal; excepción concedida; D.5 no excluida; VERIFIED no rechazado; auditoría sin cierre; §3.13 sin la prohibición de la receta |
| G6 líneas del Owner | 8/8 | líneas fundidas; línea (b) retirada; (a) sin la exclusión de D.5; (b) dentro de A2-P2; blob cambiado; (a) autoriza una medición; nombres iguales; trío cambiado |
| G7 identidad de los instrumentos | 4/4 | blob o SHA-256 cambiados en §3.3; fila de la especificación retirada; `e2e-summary.json` incoherente |
| G8 estado y declaraciones | 8/8 | «A-6 = AGREED»; frase de §76.5 retirada de la cabecera o de §11; «Coordinator = AGREED»; «C-24 = PASS»; estado AGREED; excepción vigente; A-6 aplicada |
| G5b autoprueba custodiada | 2/2 | ToolBlob distinto; Result FAIL |

**Negativos de extremo a extremo** (`neg_controls_a6.py`, por la línea de órdenes; 14/14 como se esperaba):

| Caso | Esperado | Obtenido |
|---|---|---|
| P-0 A-6 sin alterar, sin commit candidato | PASS | PASS |
| N-01 punto 4 retirado de §3.2 | G4 | FAIL G4 (y G5b) |
| N-02 M-05 = no | G4 | FAIL G4 (y G5b) |
| N-03 las dos líneas del Owner fundidas | G6 | FAIL G6 (y G5b) |
| N-04 blob del instrumento cambiado en §3.3 | G7 | FAIL G1, G7 (y G5b) |
| N-05 frase de §76.5 retirada de la cabecera | G8 | FAIL G8 (y G5b) |
| N-06 «A-6 = AGREED» añadido | G8 | FAIL G8 (y G5b) |
| N-07 ancla de A-5 L221 alterada | G1 | FAIL G1 (y G5b) |
| N-08 A6-3 ubicada en D.1 | G5 | FAIL G5 (y G5b) |
| N-09 A6-1 sin la prohibición de cambiar la receta | G5 | FAIL G5 (y G5b) |
| N-10 superficie prohibida retirada de §6.1 | G3 | FAIL G3 (y G5b) |
| N-11 commit que además modifica AUTOMATION_PLAN | G2, G3 | FAIL G2, G3 |
| N-12 commit que además modifica el registro de evidencia | G2 | FAIL G2 |
| N-13 commit que además modifica el instrumento fijado | G2, G3 | FAIL G2, G3 |

En los casos de archivo G5b también falla, como debe: la autoprueba custodiada fija el blob de la A-6 sin alterar.

**Preview.** Sobre la HEAD, sin commit candidato (`out/a6-guards-result-preview-head.json`): G2 NOT_RUN y el resto PASS. Sobre un commit candidato
**hipotético** construido solo en un clon temporal (`make_candidate_commit_a6.py`; base = HEAD del worktree; SHA en `out/candidate-commit.json` y en
`a6-guards-result-preview.json`): **9/9 PASS**. El commit añade solo `docs/initiatives/I-62-A-6.md` y archivos de `docs/automation/evidence/I-62-A6/`
(todos con estado A); el diff de las 33 superficies prohibidas es vacío; C-20b = EQUAL (MV-2..MV-6; 31 archivos; entradas 15/0/53/2).

## 4. (f) Worktree de I-62 sin cambios

`out/worktree-digest.json`: HEAD, `for-each-ref`, `status --porcelain --ignored` y SHA-256 del índice iguales antes y después del preflight, con
`GIT_OPTIONAL_LOCKS=0` en todas las lecturas; `D:\r62-a6-tmp` borrado al terminar.

## 5. (g) Consecuencias OWNER-RESERVED y líneas exactas del Owner (A-6 §7)

Dos consecuencias, separadas y distintas; ninguna implica la otra y el silencio no es aprobación:

- **(a)** determinación sobre ADR-0046 #9, **antes del acuerdo**: solo FX-02 en F6, Topología A, solo la celda de EXECUTION_CONTROLLER, solo el
  instrumento `9a829b0e…` y el aceptante `434ef1fc…`; excluye producción, otras unidades, escenarios, roles o topologías, D.5 y la Owner Validation;
  no modifica ADR-0046 ni ADR-0048; no autoriza invocaciones. Mientras no se disponga, A-6 no se presenta a acuerdo.
- **(b)** autorización de consumo de **una** invocación de medición, solo para aplicar A6-7, fuera de A2-P2 y sin reutilizar el bloque agotado ni la
  invocación de A4-1; solo con A-6 AGREED, con (a) aprobada y con la disposición del Coordinator que la nombre. Es la solicitud futura de medición de
  decisiones §76, punto 6.

```text
A6-ADR0046-9-FX02 = A (determinación OWNER-RESERVED, previa al acuerdo de A-6, de que la evidencia de F6 (paquete B-min del commit 9356edf141c795ea718a360e266334ded10d6a76 y dictamen R20261011T162328Z-5c5e, ACCREDITED) justifica frente a ADR-0046 #9 (Nivel A), conservado por ADR-0048, una excepción limitada: el EXECUTION_CONTROLLER de FX-02 en F6, Topología A, puede usar en su verificación solo el instrumento de hechos de blob 9a829b0eeb1210238052b1feac06cfa8f7964aed, y la supervisión, del lado del Coordinator, solo el aceptante de blob 434ef1fccb0d15762b30244b4a60928120698411; excluye la producción, cualquier otra unidad, escenario, rol o topología, D.5 y la Owner Validation; no modifica ADR-0046 ni ADR-0048 ni crea precedente; no autoriza ninguna invocación ni consumo ni acepta ningún resultado; solo tiene efecto con A-6 AGREED y la disposición del Coordinator que la cite)
A6-CONTROLLER-MEDICION-CONSUMO = A (1 invocación de medición read-only de codex-cli, celda gpt-6-luna/high del EXECUTION_CONTROLLER de FX-02 en F6, con el instrumento de blob 9a829b0eeb1210238052b1feac06cfa8f7964aed y el intérprete que nombre la disposición, para el trío exacto d13914ced6c7af174d8d638db938284231227312104b06a25e9608c632a7daec/26.1007.2314.0/F909F480E535E0D3D64817972361077B13AEFEEDF5EB191B5E6B941E7E1C8113, con huella, binario y versión antes y después; fuera de A2-P2, sin reutilizar su bloque agotado ni la invocación consumida de A4-1; sin reintento; sin workspace-write ni cambios de config.toml, trust_level o sandbox; sin lectura de valores; solo con A-6 AGREED, con A6-ADR0046-9-FX02 = A y con la disposición del Coordinator que la nombre; distinta de A6-ADR0046-9-FX02 y no implícita en ella; no acepta su resultado ni amplía OD-2 ni OD-5)
```

## 6. (h) ¿Ofrece A-6 una ruta demostrable hacia C-24?

Hechos, sin rebajar criterios:

- **Lo que A-6 aporta:** una ruta contractual para que el Controller de FX-02 use un instrumento de hechos fijado por blob, con fallo cerrado,
  reproducción independiente y un oráculo independiente, y la regla para medir su elegibilidad. Retira la parte del bloqueo que es normativa.
- **Lo que sigue faltando, en orden:** (1) la determinación del Owner de §7 (a); (2) la revisión formal y el acuerdo de A-6; (3) la línea (b) y una
  disposición del Coordinator que fije intérprete, trío y `-C`; (4) que esa única medición demuestre todas las operaciones de A6-7, regla 1 (hoy
  nadie lo ha medido: ninguna sonda usó B-min, y R4 del aceptante no se ha ejercido sobre una salida real del Controller); (5) B1 y la sesión de
  FX-U2 de A-5, con sus líneas del Owner, y OD-7 (CI del fixture); (6) la ronda de FX-02 dentro de los topes de D.3, con el oráculo sellado antes.
- **Conclusión:** A-6 ofrece una ruta **condicionada**, no demostrada. Un C-24 PASS por esa ruta demostraría la configuración Controller más
  instrumento, no el protocolo de producción, y no cubriría D.5 ni OV-I62-03.

## 7. Límites de esta preparación

- Decisiones §76, punto 2, pide que el Controller use los hechos «como insumos canónicos». A-6 admite como insumo canónico solo el archivo del
  instrumento y trata los hechos como evidencia producida dentro de la invocación (A6-2, regla 4), porque el cierre se calcula antes de lanzar. Es
  una lectura propuesta, abierta en Q-A6-01, no una decisión.
- La vía del blob (A6-3) escribe un objeto Git suelto en el repositorio del `-C` por la supervisión; la alternativa de una ref dedicada queda en
  Q-A6-02. El dictamen dice que cualquier vía es material.
- La separación FAIL / UNVERIFIED tras un REJECT_FACTS (A6-5, regla 4) y el `not_run` ante una discrepancia con la orden Git primaria (A6-5,
  regla 1) son propuestas de A-6 (Q-A6-06, Q-A6-07).
- La diversidad entre instrumento y aceptante sigue declarada, no observada (dictamen, TOOL_ACCEPTOR_DIVERSITY = UNKNOWN; Q-A6-12).
- Las guardas comprueban la forma, las citas, las identidades y la pureza del delta; no juzgan si la redacción normativa es correcta, que es la
  tarea de la revisión.
- El paquete OWNER-RESERVED propio de decisiones §76, punto 4, y la solicitud de medición de §76, punto 6, se apoyan en las dos líneas de A-6 §7;
  no se preparó aquí un documento aparte.

## 8. Archivos

| Archivo | Contenido |
|---|---|
| `../../../initiatives/I-62-A-6.md` | la enmienda candidata (destino: `docs/initiatives/I-62-A-6.md`) |
| `a6-guards.py` | guardas G1-G8 y G5b; modos `self-test`, `preview` y `run` |
| `a6-selftest.json` | autoprueba: controles y 83 mutantes |
| `a6-guards-result-preview.json` | preview sobre el commit candidato hipotético |
| `make_candidate_commit_a6.py` | commit candidato hipotético en un clon temporal bajo `D:\r62-a6-tmp` |
| `neg_controls_a6.py` | 14 controles de extremo a extremo |
| `run_preflight_a6.py` | orden del preflight y digest del worktree |
| `out/a6-guards-result-preview-head.json` | preview sobre la HEAD (G2 NOT_RUN) |
| `out/candidate-commit.json` | rutas y diff del commit candidato hipotético |
| `out/neg_controls_a6.json` | resultados de los controles de extremo a extremo |
| `out/worktree-digest.json` | digest del worktree antes y después |

**Para publicar:** copiar `docs/initiatives/I-62-A-6.md` y `docs/automation/evidence/I-62-A6/` byte a byte (LF) y hacer un commit que solo añada
esas rutas; después, `python -I -B docs/automation/evidence/I-62-A6/a6-guards.py run --repo <worktree> --base <padre> --head <commit> --out <json>
--tmp <directorio fuera del worktree>`. El registro en decisiones, evidencia, estado o README de F6 va en un commit aparte (G2 lo exige).
