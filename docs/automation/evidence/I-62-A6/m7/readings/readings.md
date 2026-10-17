# I-62 — Lecturas normativas del aceptante fijado (A-6, A6-5, regla 6)

```text
Tipo:        material de preparación para una disposición del Coordinator (decisiones §90, punto 3). No decide.
Rol:         análisis del lado de la supervisión; solo lectura sobre W y sobre la copia del fixture
Fecha:       2026-10-16 (UTC-6)
W:           %USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal (HEAD 400a72d8)
A-6:         docs/initiatives/I-62-A-6.md, blob 4bc919dd4768ab81abc74b70d3a4a3821c07feb5 (AGREED)
Aceptante:   docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py
             blob 434ef1fccb0d15762b30244b4a60928120698411, SHA-256 173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0
Instrumento: docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py (solo para comparar)
             blob 9a829b0eeb1210238052b1feac06cfa8f7964aed, SHA-256 b03c3a19cef608386afd7c2e0618b884bfd7b7f6913e95d8ed70272d517a0200
Veredictos:  COINCIDE 12 · AMBIGUOUS-ACCEPTOR-CHOICE 12 · DIVERGE 0 (24 filas)
```

## 0. Resumen

- **Ninguna fila DIVERGE.** En las 24 reglas, la lectura del aceptante fijado es una lectura admisible del texto normativo. En 12 el texto admite una sola lectura y el aceptante la aplica (COINCIDE; una de ellas, L-19, es vacía porque el aceptante no aplica la regla). En las otras 12 el texto admite más de una y el aceptante eligió una (AMBIGUOUS-ACCEPTOR-CHOICE). La propuesta es que el Coordinator adopte literalmente esa elección (§7). Si eligiera otra en cualquiera de esas filas, el aceptante no se usa tal como está (A6-5, regla 6: otro blob, otra corrección de A-6 y otra determinación del Owner).
- **AP idéntico en el fixture.** `git -C D:\r62-a6r2\r3\deliv rev-parse 1746b404:docs/AUTOMATION_PLAN.md` = `f525cb1e…`, el mismo blob que en W: todas las líneas de AP citadas son idénticas byte a byte en la `AuthorityRevision` del fixture (y en EFF `fbe25347`). El mapa `4d49d3e1` y su esquema `6b54dda9` en EFF del fixture son también los de W.
- **Fuente de U-19.** Decisiones §62, punto 3 (DEC L1215), con el texto registrado en DEC L1234 y la parte dispuesta «entera» en DEC L1250. Procede de la solicitud de desbloqueo de FX-02 (L89, blob `203d2887`). V14 no contiene «U-19» (comprobado: 0 apariciones).
- **Hueco de enumeración (D-5).** El paréntesis de la regla 6 nombra «MV-1..MV-6 y las listas de MV-4», pero la parte de mapa de R4 también aplica MV-7 y E2-E3 (ACCEPT-NOTES 14), y A6-9, regla 2, nombra MV-7. Las filas L-01, L-02 y L-14 los cubren, y la disposición propuesta los incluye.
- **Divergencia nueva entre instrumento y aceptante (D-1), fuera de G1-G6.** En `contract` (A3 y A4), el instrumento no aplica U-19 a la entrada contenedora (TOOL-NOTES 12) y el aceptante sí. La lectura del aceptante es la del Coordinator (L-22), así que la coincidencia de la regla 6 no se ve afectada. El efecto es un rechazo falso, sin riesgo de aceptación indebida. Lo confirman tres sondas sobre las piezas fijadas: P1, con las listas del contrato T1 copiadas tal cual, da ACCEPT_FACTS; P2, con la sustitución que prescribe nc2 (RAE L229-L230) sobre T1, y P3, sintética, dan REJECT_FACTS por R7.
- **Lecturas de R4 fuera del paréntesis (D-6).** Scope con entradas vacías o mal formadas (L-23) y el conjunto de rutas del diff (L-24) se incluyen como filas. Las de `Identity`, `Trailer`, `FreeText` y `CleanTree` se señalan en §6, sin fila.

## 1. Fuentes fijadas

Todas se leyeron como objetos Git con `git -C W --no-replace-objects cat-file blob <blob>` (bytes LF). Las líneas citadas son las de esos bytes.

| Clave | Ruta en W | Blob | SHA-256 |
|---|---|---|---|
| A-6 | `docs/initiatives/I-62-A-6.md` | `4bc919dd4768` | `cad51e9f84a6394b…` |
| AP | `docs/AUTOMATION_PLAN.md` | `f525cb1e9d24` | `6dc9f5d5cb90b224…` |
| DEC | `docs/automation/decisions/I-62.md` | `c0f0457b282e` | `4503f5587ca8e5cd…` |
| V14 | `docs/initiatives/I-62-proposal-v14.md` | `34ad80ea1bff` | `ab55ad4f4addef84…` |
| A-5 | `docs/initiatives/I-62-A-5.md` | `ad94ceedacbf` | `3b1c60d2049ef20d…` |
| RAE | `docs/automation/agent-execution/README.md` | `592dcfd45e87` | `2070e629151846af…` |
| FX-02-unblock-request | `docs/automation/evidence/I-62-F6/requests/FX-02-unblock-request-2026-10-09.md` | `203d288750be` | `cad97c4e8664fcb1…` |
| bmin_accept.py | `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py` | `434ef1fccb0d` | `173514aa353f6add…` |
| bmin_facts.py | `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py` | `9a829b0eeb12` | `b03c3a19cef60838…` |
| ACCEPT-NOTES | `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/ACCEPT-NOTES.md` | `cb9df0289c69` | `3751f21685c18c84…` |
| TOOL-NOTES | `docs/automation/evidence/I-62-F6/FX-02/bmin/tool/TOOL-NOTES.md` | `d3218ff79c84` | `f56781bf5cd5fd10…` |
| SPEC | `docs/automation/evidence/I-62-F6/FX-02/bmin/SPEC.md` | `4d448c6fc467` | `510143f24522211e…` |
| s74/mv-static-analysis | `docs/automation/evidence/I-62-F6/FX-02/s74/mv-static-analysis.md` | `8c8dab0f6c67` | `4bab0a514ef15de2…` |
| r2/independence/README | `docs/automation/evidence/I-62-A6/r2/independence/README.md` | `72c9a7ebfb9e` | `0031e55a266970d0…` |
| b-clausemap.json | `docs/automation/evidence/I-62-A6/r2/independence/results/b-clausemap.json` | `c3a6f0c3b6da` | `6164fdd7500980a2…` |

Copia del fixture (solo lectura): `D:\r62-a6r2\r3\deliv`; `1746b404:docs/AUTOMATION_PLAN.md`, `fbe25347:docs/AUTOMATION_PLAN.md` y `95bdc29d:docs/AUTOMATION_PLAN.md` = `f525cb1e…`; contrato T1 `95bdc29d:docs/automation/decisions/FX-U1-T1.gate-contract.json` = `628d89af…`.

## 2. Método y criterios

- Cada fila es una regla en la que el aceptante aplica una lectura propia: la parte de mapa de `Authority` en R4 (E2-E3, alcance, MV-1..MV-7 con las cuatro listas de MV-4 por separado y el lector Markdown) y, en R7, la cobertura, las entradas mal formadas, U-19 en sus tres partes, A3, A4 y U-19 dentro de A3 y A4. Se añaden dos lecturas de R4 `Scope` (L-23, L-24) que aplican la misma cobertura. Los ítems «SPEC ambiguities resolved» de ACCEPT-NOTES que cambian la lectura de esas reglas están dentro de las filas (§5).
- **COINCIDE:** el texto normativo admite una sola lectura razonable y el aceptante la aplica. **AMBIGUOUS-ACCEPTOR-CHOICE:** el texto admite más de una, el aceptante aplica una admisible y el Coordinator puede adoptarla. **DIVERGE:** la lectura del aceptante no es admisible o contradice una lectura ya fijada. Una duda se clasifica como AMBIGUOUS con su motivo, nunca como COINCIDE.
- La «lectura propuesta del Coordinator» es la del aceptante, redactada para que el Coordinator la adopte o la corrija. La columna de consecuencia dice qué pasa si eligiera otra: según A6-5, regla 6, el aceptante no se usa tal como está. Se indica además la dirección del error (rechazo falso o aceptación indebida).
- Cada cita (texto normativo, código y evidencia) la comprueba `verify_citations.py` contra los bytes fijados por blob. Resultado: `citations-result.json`.
- Sondas: tres, de una sola afirmación (D-1), con las piezas fijadas ejecutadas desde sus bytes y `python -I` (`probes/`). Ninguna batería y ningún mutante.

## 3. Tabla de coincidencia

Columna final: qué pasaría si el Coordinator fijara otra lectura. «No utilizable» = el aceptante no se usa tal como está (A6-5, regla 6); entre paréntesis, la dirección del error del aceptante frente a esa otra lectura.

| Fila | Regla | Conjunto | ¿Más de una lectura? | Veredicto | Otra lectura del Coordinator |
|---|---|---|---|---|---|
| L-01 | E2: activación (Derive, trailer único y EFF) en MainSha_eval | R4 parte de mapa (E2) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso o aceptación indebida, según la alternativa) |
| L-02 | E2-E3: punto de entrada y encabezado de 16.13 en MainSha_eval | R4 parte de mapa (E2-E3) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso o aceptación indebida, según la alternativa) |
| L-03 | Alcance y efecto de la parte de mapa de `Authority` | R4 parte de mapa (alcance) | no (para la decisión) | **COINCIDE** | no utilizable (rechazo falso) |
| L-04 | MV-1: el mapa existe, se interpreta como JSON y valida contra su esquema en EFF | R4 parte de mapa (MV-1) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso) |
| L-05 | MV-2: `Surfaces` = la lista cerrada | R4 parte de mapa (MV-2) | no | **COINCIDE** | no utilizable (rechazo falso) |
| L-06 | MV-3: completitud de archivos | R4 parte de mapa (MV-3) | no (en efecto) | **COINCIDE** | sin efecto práctico (con MV-2 en defecto) |
| L-07 | MV-4, lista `LiteralViolations` (regla literal de L905) | R4 parte de mapa (MV-4, lista 1) | no | **COINCIDE** | no hay otra lectura admisible |
| L-08 | MV-4, lista `EntryFilesOutsideSchema` | R4 parte de mapa (MV-4, lista 2) | no | **COINCIDE** | no hay otra lectura admisible |
| L-09 | MV-4, lista `AddedPresentInEff1` (R61 b; tabla de defectos) | R4 parte de mapa (MV-4, lista 3) | sí en la etiqueta, no en el efecto | **COINCIDE** | el efecto no cambia (L916) |
| L-10 | MV-4, lista `EntryFilePresentInEff1` (informativa) | R4 parte de mapa (MV-4, lista 4) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (aceptación indebida posible) |
| L-11 | MV-5: `Entries` único por (`Path`, `Section`), con `Path` de un archivo MODIFIED | R4 parte de mapa (MV-5) | sí (menor) | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso) |
| L-12 | MV-6: igualdad con la derivación por archivo MODIFIED | R4 parte de mapa (MV-6) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso) |
| L-13 | Lectura de Markdown para Match, MV-6 y MV-7 (encabezados, bloques de código, preámbulo) | R4 parte de mapa (lector Markdown) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (error en las dos direcciones) |
| L-14 | MV-7: punto de entrada, punteros y 16.3 en EFF | R4 parte de mapa (MV-7) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso) |
| L-15 | Cobertura de una ruta por una entrada (`PerPath`) | R7 (scope) | no | **COINCIDE** | no hay otra lectura admisible |
| L-16 | Entradas de alcance mal formadas (`MalformedEntries`) | R7 (scope) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (aceptación indebida posible) |
| L-17 | U-19 (a): «/**» final = prefijo terminado en «/» (`U19Normalized`) | R7 (scope) | no | **COINCIDE** | no hay otra lectura admisible |
| L-18 | U-19 (b): comodín no final | R7 (scope) y R4 Scope | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso) |
| L-19 | U-19 (c): ningún commit del Worker solo de evidencia | R7 (scope) | no aplica | **COINCIDE** | ninguna (coincidencia vacía) |
| L-20 | A3: `AllowedWriteScope` ⊆ alcance permitido del contrato | R7 (contract) | no (con U-19) | **COINCIDE** | no hay otra lectura admisible |
| L-21 | A4: `ForbiddenWriteScope` ⊇ prohibido del contrato | R7 (contract) | no (con U-19) | **COINCIDE** | no hay otra lectura admisible |
| L-22 | U-19 dentro de la contención de A3 y A4 | R7 (contract) | no razonable | **COINCIDE** | no utilizable (rechazo falso); D-1 aparte |
| L-23 | R4 `Scope` con entradas vacías o mal formadas | R4 Scope (fuera del paréntesis de la regla 6) | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso) |
| L-24 | Conjunto de rutas del diff (renombres) | R7 (scope) y R4 Scope | sí | **AMBIGUOUS-ACCEPTOR-CHOICE** | no utilizable (rechazo falso) |

## 4. Filas

### L-01 — E2: activación (Derive, trailer único y EFF) en MainSha_eval

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 parte de mapa (E2) · **ACCEPT-NOTES:** 14, E4
- **Texto normativo:** «`I62_EFFECTIVE_SHA` es el primer merge en first-parent de `origin/main` cuyo segundo padre alcanza el **único** commit con el trailer `Agent-Protocol-Normative: I-62` y cuyo primer padre no lo alcanza.» (AP L945-L946); «| punto de entrada sin EFF derivable, o trailer duplicado | ACTIVATION_INVALID → STOP al Owner |» (AP L799); «`MainSha_eval` = `K.MainSha`, salvo en una reverificación de 16.7 sin conflictos, donde es el `MainSha` nuevo del `RebaseMap`» (AP L813-L814); «la revisión de los términos de 16.10 y el `MainSha` de la invocación» (A-6 L619)
- **Lectura del aceptante:** Commit «con el trailer» = commit alcanzable desde `Verification.MainSha` cuyo mensaje crudo, partido por LF y sin normalizar CR ni recortar espacios, contiene una línea exactamente igual a `Agent-Protocol-Normative: I-62`. Dos o más: defecto. Uno sin merge de la cadena first-parent cuyo segundo padre lo alcance (alcanzar es reflexivo) y cuyo primer padre no lo alcance: defecto. EFF = el candidato más antiguo. Sin trailer y sin punto de entrada: PRE_ACTIVATION, sin restricción. (`bmin_accept.py:266, 1040, 1047, 1049, 1299, 1301, 2170`)
- **Lectura del instrumento:** Misma lectura (línea exacta en cualquier lugar del mensaje), pero normaliza CRLF del mensaje antes de partirlo: con un mensaje CRLF las dos piezas divergen (G6, sonda P3). (`bmin_facts.py:1079, 1082`)
- **¿Más de una lectura?:** sí: (i) línea exacta en cualquier lugar del mensaje (aceptante; SPEC E4); (ii) trailer de Git, solo en el bloque final de trailers; (iii) subcadena. Además, el tratamiento de CRLF.
- **Lectura propuesta del Coordinator (literal):** «El commit «con el trailer» de 16.14 es todo commit alcanzable desde `MainSha_eval` (AP L813-L814; en la solicitud del aceptante, el `MainSha` de la invocación, A6-3, regla 6 (c)) cuyo mensaje, partido por LF y sin recortar espacios ni normalizar CR, contiene una línea exactamente igual a `Agent-Protocol-Normative: I-62`; «único» se cuenta sobre ese conjunto; «alcanza» es reflexivo; dos o más commits, o uno sin merge first-parent que cumpla la regla, es ACTIVATION_INVALID y prohíbe `Authority` = pass.»
- **Si el Coordinator eligiera otra:** Con (ii), el aceptante contaría como trailer una línea del cuerpo y vería ACTIVATION_INVALID donde el Coordinator no: rechazo falso. Con (iii), no contaría una mención dentro de una línea más larga y dejaría `Authority` sin restricción donde el Coordinator ve ACTIVATION_INVALID: aceptación indebida posible. En los dos casos el aceptante no se usa tal como está (A6-5, regla 6). CRLF: residuo G6, solo rechazo falso.
- **Evidencia:** ACCEPT-NOTES L206-L208 («Evaluate E2-E3»); ACCEPT-NOTES L238 («an envelope with Facts is a MISMATCH»); r2/independence/README L275-L276 («P3, CRLF in the trailer commit's message»)

### L-02 — E2-E3: punto de entrada y encabezado de 16.13 en MainSha_eval

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 parte de mapa (E2-E3) · **ACCEPT-NOTES:** 8, 11, 14
- **Texto normativo:** «X := Match(MainSha_eval, WORKFLOW, <línea de encabezado del punto de entrada>) (misma semántica que Match: línea normalizada, nivel ##, fuera de bloques de código)» (AP L784-L785); «EFF presente y |X| ≠ 1 → ENTRY_INVALID → STOP (S-12; P-15)» (AP L789); «| el punto de entrada nombra una 16.13 ausente o repetida | ENTRY_INVALID → STOP |» (AP L801); «nombra esta subsección por su línea de encabezado exacta» (AP L728)
- **Lectura del aceptante:** `Match` = línea completa normalizada (espacios colapsados y recortados; la sangría no cuenta) igual a la línea fija normalizada, fuera de bloques ```; el nivel lo fija el prefijo `##` o `###` de la propia línea fija. Defecto si el punto de entrada no aparece exactamente una vez en WORKFLOW, si la línea de 16.13 no aparece exactamente una vez en AP o si el texto de la sección del punto de entrada (CRLF a LF, sin normalizar espacios) no contiene literalmente la línea exacta de 16.13. (`bmin_accept.py:523, 573, 1062, 1064, 1307, 1309, 1311`)
- **Lectura del instrumento:** Igual, salvo que reconoce encabezados sobre la línea cruda, sin sangría: un encabezado sangrado diverge (G6, sonda P2). (`bmin_facts.py:381`)
- **¿Más de una lectura?:** sí: «nombra … por su línea exacta» admite subcadena en cualquier parte de la sección (aceptante), la línea propia o un enlace; y el encabezado sangrado se reconoce o no.
- **Lectura propuesta del Coordinator (literal):** «El punto de entrada y 16.13 se cuentan con `Match` sobre la línea normalizada completa (la sangría inicial no cuenta), fuera de los bloques que empiezan por tres comillas invertidas; el punto de entrada «nombra» 16.13 si el texto de su sección, con CRLF convertido en LF, contiene literalmente la línea exacta de 16.13; con EFF presente, |X| ≠ 1, |R| ≠ 1 o un punto de entrada que no la nombra es ENTRY_INVALID y prohíbe `Authority` = pass.»
- **Si el Coordinator eligiera otra:** Con una lectura más estricta de «nombra» (línea propia), el aceptante sería más permisivo que el Coordinator y no restringiría un `pass` que el Coordinator rechaza; con otra lectura de la sangría, rechazo falso. En los dos casos, no se usa tal como está. Sangría frente al instrumento: residuo G6.
- **Evidencia:** ACCEPT-NOTES L200 («literal substring of the 16.13 heading in the entry section text»); r2/independence/README L273-L274 («P2, three-space indented heading»)

### L-03 — Alcance y efecto de la parte de mapa de `Authority`

- **Veredicto:** COINCIDE · **Conjunto:** R4 parte de mapa (alcance) · **ACCEPT-NOTES:** 1, 14
- **Texto normativo:** «Cualquier fallo → MAP_INVALID → `Authority` fail; STOP (S-12; P-15).» (AP L822); «comprobación `Authority` de la verificación» (AP L779); «**Con MAP_INVALID, ningún contrato I61 pasa `Authority` tras EFF.**» (AP L920); «Con AP L822, toda evaluación de un contrato I62 del fixture termina en STOP.» (A-5 L157); «es `not_run`, y `not_run` cuenta como fail; nunca es» (AP L1204)
- **Lectura del aceptante:** Un defecto de E2-E3 o de MV-1..MV-7 prohíbe que el `Authority` del Controller sea `pass` (`fail` y `not_run` valen igual). Sin defectos, `Authority` queda sin restricción: el aceptante no recalcula `Classify`, los pasos 3-4 de `Resolve` ni las comprobaciones de 16.3, y no distingue la clasificación de la unidad. PRE_ACTIVATION: sin restricción. (`bmin_accept.py:1302-1305, 2151, 2185, 2187, 2188`)
- **Lectura del instrumento:** No aplica: el instrumento no decide.
- **¿Más de una lectura?:** no para la decisión: L920 nombra los contratos I61, pero L822 (paso 2 de `Resolve`, antes de ramificar por la clasificación) no distingue, y A-5 L157 ya la lee para contratos I62 del fixture. La unilateralidad (sin defecto no se exige pass) es alcance, no lectura: residuo G5.
- **Lectura propuesta del Coordinator (literal):** «En la verificación de FX-02, todo defecto de la parte de mapa (E2-E3 o `Validate(M)`, MV-1..MV-7) hace que `Authority` no pueda ser `pass`, sea cual sea la clasificación de la unidad (AP L822; A-5 L157); `fail` y `not_run` cuentan igual (16.22). La ausencia de defectos no implica `pass`. PRE_ACTIVATION no impone restricción de mapa.»
- **Si el Coordinator eligiera otra:** Si el Coordinator limitara MAP_INVALID a los contratos I61 (lectura restrictiva de L920), el aceptante rechazaría un `pass` de una unidad I62 que el Coordinator admite (rechazo falso): no se usaría tal como está. G5 sigue declarado.
- **Evidencia:** A-6 L988 («G5. Parte de mapa de R4 de un solo lado.»); r2/independence/README L162-L163 («R4 asymmetry»)

### L-04 — MV-1: el mapa existe, se interpreta como JSON y valida contra su esquema en EFF

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 parte de mapa (MV-1) · **ACCEPT-NOTES:** 6, 12
- **Texto normativo:** «| MV-1 | el mapa existe en EFF en la ruta fija, se interpreta como JSON y valida contra su esquema en EFF |» (AP L902); «| mapa ausente, ilegible o inválido contra su esquema | MAP_INVALID |» (AP L912)
- **Lectura del aceptante:** «Se interpreta como JSON» = UTF-8 estricto, sin BOM, sin claves repetidas y sin NaN ni Infinity. «Valida contra su esquema» = subconjunto de draft 2020-12 (`type`, `enum`, `const`, `required`, `properties`, `additionalProperties`, `items`, `minItems`, `maxItems`, `uniqueItems`, `minLength`, `pattern`, `minimum`, `maximum`, más anotaciones), con `enum` y `const` por igualdad de JSON canónico y `pattern` con `re` de Python y `$` final leído como fin de texto (`\Z`). Mapa o esquema ausente o ilegible, palabra clave fuera del subconjunto o alguna violación: defecto. (`bmin_accept.py:141, 145, 843, 861, 899, 917, 1315, 1317, 1319, 1321`)
- **Lectura del instrumento:** Misma lectura del JSON y del subconjunto, pero ante una palabra clave no admitida o un esquema ilegible falla cerrado (`UNSUPPORTED_SCHEMA_KEYWORD` o `MALFORMED_JSON`, sin hechos) y usa clases ASCII en `pattern`. En R6 `SchemaViolations` solo se compara por vacío (ACCEPT-NOTES 6): G1. (`bmin_facts.py:1273, 1274`)
- **¿Más de una lectura?:** sí: AP no fija el dialecto ni el validador; RFC 8259 solo recomienda nombres únicos y hay analizadores que toleran el BOM; la igualdad numérica (1 frente a 1.0) en `enum` y `const`; la semántica ECMA de `pattern`.
- **Lectura propuesta del Coordinator (literal):** «Para FX-02, MV-1 se cumple solo si el mapa en EFF, en su ruta fija, es JSON UTF-8 estricto (sin BOM, sin claves repetidas, sin NaN ni Infinity) y valida sin violaciones contra el esquema en EFF con el subconjunto de draft 2020-12 de la especificación B-min (§2.12), con `enum` y `const` por igualdad de JSON canónico y el `$` final de `pattern` como fin de texto; un esquema ausente o ilegible, o una palabra clave fuera del subconjunto, deja MV-1 sin establecer y `Authority` no puede ser `pass` (16.22).»
- **Si el Coordinator eligiera otra:** Con un JSON tolerante o un validador completo de 2020-12, el aceptante vería defecto donde el Coordinator no (rechazo falso): no se usa tal como está. No material sobre el esquema `6b54dda9` (EFF del fixture y MC_I62): solo usa palabras del subconjunto y el `pattern` `^[0-9a-f]{40}$`. G1 sigue declarado.
- **Evidencia:** A-6 L931 («G1. Palabra clave de esquema no admitida (MV-1).»)

### L-05 — MV-2: `Surfaces` = la lista cerrada

- **Veredicto:** COINCIDE · **Conjunto:** R4 parte de mapa (MV-2) · **ACCEPT-NOTES:** —
- **Texto normativo:** «| MV-2 | `Surfaces` = la lista cerrada de esta subsección |» (AP L903); «<la lista cerrada de esta subsección, en su orden>» (AP L889); «**Superficies** (lista cerrada): `AGENTS.md`, `CLAUDE.md`» (AP L738)
- **Lectura del aceptante:** Igualdad exacta de listas ordenadas entre `Surfaces` del mapa y los valores entre comillas invertidas del único párrafo de AP en EFF que empieza por «**Superficies** (lista cerrada):», hasta la primera línea en blanco; párrafo ausente o repetido: defecto. (`bmin_accept.py:620, 621, 1322, 1323`)
- **Lectura del instrumento:** Igual sobre AP `f525cb1e`; normaliza los espacios de cada valor y busca el párrafo al inicio de la línea cruda (sin efecto en ese AP). (`bmin_facts.py:1199`)
- **¿Más de una lectura?:** no: L889 fija el orden («en su orden»).
- **Lectura propuesta del Coordinator (literal):** «MV-2 exige que `Surfaces` sea, elemento a elemento y en el mismo orden, la lista de valores entre comillas invertidas del párrafo «**Superficies** (lista cerrada):» de AP en EFF.»
- **Si el Coordinator eligiera otra:** Con igualdad de conjuntos, el aceptante sería más estricto (orden distinto = defecto): rechazo falso; no se usaría tal como está.
- **Evidencia:** r2/independence/README L148 («Surfaces reordered»)

### L-06 — MV-3: completitud de archivos

- **Veredicto:** COINCIDE · **Conjunto:** R4 parte de mapa (MV-3) · **ACCEPT-NOTES:** 12
- **Texto normativo:** «{p bajo `Surfaces` : blob(EFF^1, p) ≠ blob(EFF, p)} ∖ {mapa} = {`Files.Path`}, sin duplicados; un archivo borrado no tiene clase → inválido» (AP L904); «| archivo modificado ausente de `Files`, o listado sin modificarse | MAP_INVALID |» (AP L914)
- **Lectura del aceptante:** «Bajo `Surfaces`» = ruta igual a una superficie o que empieza por una superficie terminada en `/`, con las `Surfaces` del propio mapa; cambiada = identificador de la entrada distinto entre EFF^1 y EFF (la ausencia cuenta; el modo no), sin el mapa; igualdad de conjuntos con `Files.Path`; duplicados y borrados: defecto. (`bmin_accept.py:1160, 1163, 1166, 1328`)
- **Lectura del instrumento:** Igual. (`bmin_facts.py:1299`)
- **¿Más de una lectura?:** no en efecto: usar las `Surfaces` del mapa o la lista cerrada solo difiere cuando MV-2 ya es defecto (ACCEPT-NOTES 12).
- **Lectura propuesta del Coordinator (literal):** «MV-3 se evalúa con las `Surfaces` del mapa: el conjunto de rutas bajo ellas cuyo blob difiere entre EFF^1 y EFF (añadidas y borradas incluidas), sin el mapa, debe ser igual a `Files.Path`, sin duplicados; una ruta borrada es defecto.»
- **Si el Coordinator eligiera otra:** Ninguna práctica: con MV-2 en defecto, `Authority` ya no puede ser `pass`.

### L-07 — MV-4, lista `LiteralViolations` (regla literal de L905)

- **Veredicto:** COINCIDE · **Conjunto:** R4 parte de mapa (MV-4, lista 1) · **ACCEPT-NOTES:** 4
- **Texto normativo:** «MODIFIED ⇒ `BaseBlob` = blob(EFF^1, p) ∧ `EffBlob` = blob(EFF, p); ADDED o ENTRY ⇒ `BaseBlob` = `null` ∧ `EffBlob` = blob(EFF, p)» (AP L905); «| `BaseBlob` o `EffBlob` distintos de los observados | MAP_INVALID |» (AP L915)
- **Lectura del aceptante:** Regla literal de L905 por fila de `Files`; un blob observado nulo nunca coincide. Lista no vacía: defecto. (`bmin_accept.py:1178, 1181, 1331`)
- **Lectura del instrumento:** Igual. (`bmin_facts.py:1323`)
- **¿Más de una lectura?:** no.
- **Lectura propuesta del Coordinator (literal):** «`LiteralViolations` es MV-4 tal como lo escribe L905 (la lectura A del análisis s74, la que cuenta A-5 L157, «MV-4 con 7»); una lista no vacía es MAP_INVALID.»
- **Si el Coordinator eligiera otra:** No hay otra lectura admisible; si el Coordinator fijara otra, el aceptante no se usaría tal como está.
- **Evidencia:** s74/mv-static-analysis L107 («| A: literal | L905 | **7** |»); A-5 L157 («MV-4 con 7»)

### L-08 — MV-4, lista `EntryFilesOutsideSchema`

- **Veredicto:** COINCIDE · **Conjunto:** R4 parte de mapa (MV-4, lista 2) · **ACCEPT-NOTES:** 4
- **Texto normativo:** «ENTRY de archivo ⊆ {esquema del mapa}» (AP L905); «| ENTRY fuera de la lista cerrada; encabezados duplicados | MAP_INVALID |» (AP L917)
- **Lectura del aceptante:** Toda fila con `FileKind` = ENTRY cuya ruta no es la del esquema del mapa: defecto. (`bmin_accept.py:1183, 1331`)
- **Lectura del instrumento:** Igual. (`bmin_facts.py:1328`)
- **¿Más de una lectura?:** no.
- **Lectura propuesta del Coordinator (literal):** «`EntryFilesOutsideSchema` es la cláusula «ENTRY de archivo ⊆ {esquema del mapa}» de L905; una lista no vacía es MAP_INVALID.»
- **Si el Coordinator eligiera otra:** No hay otra lectura admisible; si el Coordinator fijara otra, el aceptante no se usaría tal como está.

### L-09 — MV-4, lista `AddedPresentInEff1` (R61 b; tabla de defectos)

- **Veredicto:** COINCIDE · **Conjunto:** R4 parte de mapa (MV-4, lista 3) · **ACCEPT-NOTES:** 4
- **Texto normativo:** «ADDED que existe en EFF^1» (AP L916); «b. Path con FileKind ADDED (no existía en EFF^1)» (AP L835)
- **Lectura del aceptante:** Fila ADDED cuya ruta existe en EFF^1: defecto (rotulado «MV-4: AddedPresentInEff1»). (`bmin_accept.py:1185, 1331`)
- **Lectura del instrumento:** Igual. (`bmin_facts.py:1330`)
- **¿Más de una lectura?:** sobre dónde vive (dentro de MV-4 o solo en la tabla de defectos: lecturas A y B del análisis s74), no sobre el efecto: L916 la declara MAP_INVALID.
- **Lectura propuesta del Coordinator (literal):** «Un archivo ADDED que existe en EFF^1 es MAP_INVALID por la tabla de defectos (L916; R61 b, L835), se cuente o no dentro de MV-4; `AddedPresentInEff1` no vacía prohíbe `Authority` = pass.»
- **Si el Coordinator eligiera otra:** Ninguna: el efecto no depende de la etiqueta.
- **Evidencia:** s74/mv-static-analysis L108 («| B: MV-4 + definición de ADDED |»)

### L-10 — MV-4, lista `EntryFilePresentInEff1` (informativa)

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 parte de mapa (MV-4, lista 4) · **ACCEPT-NOTES:** 4
- **Texto normativo:** «ADDED o ENTRY ⇒ `BaseBlob` = `null` ∧ `EffBlob` = blob(EFF, p)» (AP L905); «ruta fija; el mapa lo lista como ENTRY con su blob» (AP L732); «`BaseBlob` es `null` solo con ADDED o ENTRY.» (AP L895)
- **Lectura del aceptante:** La calcula (ENTRY que existe en EFF^1) y la compara en R6 y R7, pero no la cuenta como defecto en la parte de mapa. (`bmin_accept.py:1187, 1331, 1535`)
- **Lectura del instrumento:** Igual (la especificación la declara informativa). (`bmin_facts.py:1332`)
- **¿Más de una lectura?:** sí: la lectura B-ext del análisis s74 (L915 leído como «`BaseBlob` = blob(EFF^1, p), con null = ausente», para todas las clases) la convertiría en defecto; s74 la califica de débil porque choca con L905 y L895 ante una ENTRY preexistente.
- **Lectura propuesta del Coordinator (literal):** «`EntryFilePresentInEff1` es informativa: una ENTRY de archivo presente en EFF^1 no es por sí sola defecto de MV-4 ni de la tabla de defectos.»
- **Si el Coordinator eligiera otra:** Con B-ext, el aceptante sería más permisivo que el Coordinator (no restringiría un `pass` que el Coordinator rechaza): no se usaría tal como está. No material en un B1 conforme a A5-1: el esquema no existe en `bb0d5522`, base de la derivación del mapa; sí existía en EFF^1 del fixture actual (`930288c5`).
- **Evidencia:** SPEC L221 («(ninguna regla lo convierte en defecto; se informa)»); s74/mv-static-analysis L109 («| B-ext: L915 absoluto |»); r2/independence/README L142 («| D4 | ENTRY schema present in EFF^1 (informative) |»); b-clausemap.json L1 («D4-entry-in-eff1»)

### L-11 — MV-5: `Entries` único por (`Path`, `Section`), con `Path` de un archivo MODIFIED

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 parte de mapa (MV-5) · **ACCEPT-NOTES:** 12
- **Texto normativo:** «| MV-5 | `Entries` único por (`Path`, `Section`), con `Path` de un archivo MODIFIED |» (AP L906); «| entrada o archivo duplicado | MAP_INVALID |» (AP L913)
- **Lectura del aceptante:** Unicidad por (`Path`, `Section` normalizada, con espacios colapsados); cada entrada debe tener el `Path` de una fila con `FileKind` = MODIFIED. (`bmin_accept.py:1191, 1196, 1334`)
- **Lectura del instrumento:** Igual. (`bmin_facts.py:1340`)
- **¿Más de una lectura?:** sí, menor: clave exacta o normalizada; `Match` (L872-L873) normaliza, así que dos entradas que solo difieren en espacios designan el mismo encabezado.
- **Lectura propuesta del Coordinator (literal):** «MV-5 compara `Section` normalizada (espacios colapsados, extremos recortados): dos entradas con el mismo `Path` y la misma `Section` normalizada están duplicadas; toda entrada debe tener el `Path` de un archivo MODIFIED.»
- **Si el Coordinator eligiera otra:** Con clave exacta, el aceptante sería más estricto (rechazo falso): no se usaría tal como está. Materialidad baja.

### L-12 — MV-6: igualdad con la derivación por archivo MODIFIED

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 parte de mapa (MV-6) · **ACCEPT-NOTES:** 8, 12, 13
- **Texto normativo:** «MODIFIED = {h ∈ H1 ∩ H2 : texto normalizado distinto}; REMOVED = H1 ∖ H2; ADDED ∪ ENTRY = H2 ∖ H1» (AP L907); «ni más ni menos; encabezados únicos por archivo en ambas revisiones» (AP L907); «Una sección va desde su encabezado hasta el siguiente con el mismo número de `#` o menos, e incluye sus subsecciones; el texto se normaliza con CRLF → LF y espacios colapsados.» (AP L895-L896)
- **Lectura del aceptante:** Para cada archivo MODIFIED del mapa, Markdown o no: H = líneas de encabezado normalizadas de todos los niveles (con sus `#`) más `preámbulo`; texto de sección = desde su encabezado hasta el siguiente de nivel igual o superior, subsecciones incluidas, con espacios colapsados (un encabezado de nivel 1 llega hasta el siguiente de nivel 1 o hasta el final del archivo); preámbulo = texto anterior al primer `##`. Igualdad de conjuntos por clase, con ADDED y ENTRY juntas; entradas ENTRY = exactamente el par fijo; encabezado repetido en cualquiera de las dos revisiones: defecto; archivo MODIFIED ausente en una revisión: defecto. Un archivo no Markdown solo tiene `preámbulo`, salvo líneas que empiecen por `#`. (`bmin_accept.py:535, 561, 1077, 1085, 1086, 1087, 1199, 1208, 1339, 1342, 1348`)
- **Lectura del instrumento:** Misma semántica, salvo el reconocimiento de encabezados (línea cruda: L-13) y que un archivo ausente se trata como conjunto vacío; en ese caso el aceptante no compara los derivados, así que no hay discrepancia en R6. (`bmin_facts.py:1356`)
- **¿Más de una lectura?:** sí: (i) si MV-6 se aplica a un archivo MODIFIED no Markdown (ACCEPT-NOTES 13: sí, con solo `preámbulo`) o solo a los Markdown; (ii) el reconocimiento de encabezados (L-13). La inclusión de subsecciones y de todos los niveles la fija el texto (L895-L896, L907).
- **Lectura propuesta del Coordinator (literal):** «MV-6 se aplica a todo archivo MODIFIED del mapa, Markdown o no, con el lector de L-13: H1 y H2 son los encabezados normalizados de todos los niveles más `preámbulo`; el texto de una sección incluye sus subsecciones; las clases derivadas se comparan como conjuntos con las del mapa (ADDED y ENTRY juntas); ENTRY = exactamente el par fijo; un encabezado repetido en cualquiera de las dos revisiones, o un archivo MODIFIED ausente en una de ellas, es defecto.»
- **Si el Coordinator eligiera otra:** Si MV-6 excluyera los archivos no Markdown, el aceptante sería más estricto (rechazo falso): no se usaría tal como está. No material en el mapa `4d49d3e1`: sus seis archivos MODIFIED son Markdown y lista como MODIFIED sus encabezados de nivel 1, coherente con esta lectura.
- **Evidencia:** ACCEPT-NOTES L204-L205 («MV-6** runs on every MODIFIED file of the map»)

### L-13 — Lectura de Markdown para Match, MV-6 y MV-7 (encabezados, bloques de código, preámbulo)

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 parte de mapa (lector Markdown) · **ACCEPT-NOTES:** 8, 9
- **Texto normativo:** «cuenta los encabezados del archivo en `rev` cuya línea, normalizada, es igual a `Section` normalizado (normalizar = espacios colapsados y extremos recortados); se ignoran las líneas dentro de bloques de código.» (AP L872-L873)
- **Lectura del aceptante:** Encabezado = línea que, con los espacios colapsados y recortados, empieza por 1 a 6 `#` seguidos de espacio o fin (la sangría inicial, de cualquier tamaño, no cuenta); bloque de código = entre líneas que empiezan en la columna 0 por tres comillas invertidas; sin vallas `~~~`, sin bloques sangrados y sin encabezados setext; «espacios» = los de `str.split()` de Python (incluye espacios Unicode como U+00A0); preámbulo = texto anterior al primer encabezado de nivel 2; `## 16.` y `### 16.3` = el único encabezado de ese nivel igual a la etiqueta o que empieza por etiqueta y espacio. (`bmin_accept.py:487, 494, 508, 515, 516, 550, 556-557`)
- **Lectura del instrumento:** Encabezado sobre la línea cruda, sin sangría: con un encabezado sangrado divergen (G6, sonda P2); mismas vallas (`~~~` es modo común, sonda P1). (`bmin_facts.py:90, 381`)
- **¿Más de una lectura?:** sí: CommonMark admite hasta 3 espacios de sangría (con 4 es código), vallas `~~~` y encabezados setext; AP solo dice «encabezados» y «bloques de código».
- **Lectura propuesta del Coordinator (literal):** «Para MV-6, MV-7 y E2-E3, un encabezado es una línea que, tras colapsar y recortar espacios, empieza por 1 a 6 `#` seguidos de espacio o fin de línea, fuera de los bloques delimitados por líneas que empiezan por tres comillas invertidas; no se reconocen vallas `~~~`, bloques sangrados ni encabezados setext; el preámbulo es el texto anterior al primer `##`.»
- **Si el Coordinator eligiera otra:** Con CommonMark, el aceptante divergiría en las dos direcciones (sangría de 4 o más espacios, `~~~`, setext): no se usaría tal como está. Frente al instrumento: residuo G6, solo rechazo falso.
- **Evidencia:** ACCEPT-NOTES L193-L195 («heading = normalized line starting with 1-6 `#`»); r2/independence/README L271-L272 («P1, `~~~` fence (shared reading)»)

### L-14 — MV-7: punto de entrada, punteros y 16.3 en EFF

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 parte de mapa (MV-7) · **ACCEPT-NOTES:** 9, 10, 11, 14
- **Texto normativo:** «punteros presentes en §16 y 16.3 en EFF; 16.3 en EFF sin el puntero = 16.3 en EFF^1 (normalizado)» (AP L908); «| Punteros | primera frase de §16 y última frase de 16.3 |» (AP L730); «| punto de entrada de WORKFLOW, 16.13 o punteros ausentes con EFF presente | MAP_INVALID |» (AP L918)
- **Lectura del aceptante:** En EFF: el punto de entrada exactamente una vez en WORKFLOW y nombrando 16.13 (como L-02); además, la línea de 16.13 exactamente una vez en AP (comprobación interna adicional); «punteros presentes» = la frase puntero es la primera frase del cuerpo del único `## 16.` y la última del cuerpo del único `### 16.3` (posiciones de L730); frase = texto hasta un `.` seguido de espacio o fin; «16.3 sin el puntero» = se quita una vez la última aparición de la frase puntero completa y se compara el cuerpo normalizado con el de EFF^1. (`bmin_accept.py:582, 596, 1224, 1228, 1233, 1234, 1351, 1353, 1355`)
- **Lectura del instrumento:** Igual, con encabezados sobre la línea cruda (L-13). (`bmin_facts.py:1248`)
- **¿Más de una lectura?:** sí: «presentes» admite presencia en cualquier lugar o en la posición de L730; si 16.13 repetido en EFF es defecto de MV-7 (L918 dice «ausentes»; L917 y MV-6 cubren los duplicados); cómo se quita el puntero (frase, línea o todas las apariciones).
- **Lectura propuesta del Coordinator (literal):** «MV-7 exige en EFF: el punto de entrada una sola vez y nombrando 16.13 (L-02); la línea de 16.13 una sola vez en AP; la frase puntero como primera frase del cuerpo de `## 16.` y última del cuerpo de `### 16.3` (frase = texto hasta un punto seguido de espacio o fin); y que el cuerpo normalizado de 16.3 sin esa frase (su última aparición, una vez) sea igual al cuerpo normalizado de 16.3 en EFF^1.»
- **Si el Coordinator eligiera otra:** Con «presencia en cualquier lugar», o sin la comprobación de 16.13 repetido, el aceptante sería más estricto (rechazo falso): no se usaría tal como está. El paréntesis de la regla 6 enumera MV-1..MV-6, pero la parte de mapa aplica MV-7, y A6-9, regla 2, la nombra.
- **Evidencia:** SPEC L246-L247 («PointerIsFirstSentenceS16»); s74/mv-static-analysis L178 («(a) predicado de presencia»)

### L-15 — Cobertura de una ruta por una entrada (`PerPath`)

- **Veredicto:** COINCIDE · **Conjunto:** R7 (scope) · **ACCEPT-NOTES:** 5
- **Texto normativo:** «R está cubierta por E si R = E, o si E termina en `/` y R empieza por E.» (AP L553); «ruta relativa a la raíz con `/` y la capitalización exacta» (AP L552)
- **Lectura del aceptante:** Cobertura por cadena exacta, sensible a mayúsculas y sin normalización Unicode, con E ya leída por U-19; las entradas mal formadas participan literalmente; R7 compara `AllowedCoveredBy` y `ForbiddenCoveredBy` por ruta, en orden, tras U-19. (`bmin_accept.py:724, 727, 999, 2229`)
- **Lectura del instrumento:** Igual. (`bmin_facts.py:673, 701`)
- **¿Más de una lectura?:** no.
- **Lectura propuesta del Coordinator (literal):** «Una ruta R está cubierta por una entrada E, leída por U-19, si R = E, o si E termina en `/` y R empieza por E, comparando cadenas exactas sin plegar mayúsculas ni normalizar Unicode.»
- **Si el Coordinator eligiera otra:** No hay otra lectura admisible; si el Coordinator fijara otra, el aceptante no se usaría tal como está.

### L-16 — Entradas de alcance mal formadas (`MalformedEntries`)

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R7 (scope) · **ACCEPT-NOTES:** 7
- **Texto normativo:** «archivo exacto o prefijo de directorio terminado en `/`, sin comodines, `..` ni rutas absolutas.» (AP L552)
- **Lectura del aceptante:** Mal formada = absoluta (`/` inicial, o letra de unidad seguida de `:`), con barra invertida, con un segmento `..`, o con `*`, `?` o `[` fuera de un `/**` final. La entrada vacía, los segmentos `.`, `//` y los espacios finales no se listan. (`bmin_accept.py:709, 711, 713, 716, 1007, 2230`)
- **Lectura del instrumento:** Igual en la pertenencia a la lista; añade `Reasons` y también parte `..` por la barra invertida (sin efecto: con barra invertida ya es mal formada). (`bmin_facts.py:676`)
- **¿Más de una lectura?:** sí: L552 exige además «archivo exacto o prefijo de directorio terminado en `/`» y la capitalización exacta: una entrada vacía o con `./` puede leerse como mal formada.
- **Lectura propuesta del Coordinator (literal):** «Para la lista `MalformedEntries` de R7, una entrada está mal formada si es absoluta, contiene una barra invertida, tiene un segmento `..` o contiene `*`, `?` o `[` fuera de un `/**` final; la entrada vacía se trata en R4 `Scope` (L-23).»
- **Si el Coordinator eligiera otra:** Si el Coordinator tuviera por mal formadas `./x` o `a//b`, el aceptante no las marcaría ni en R7 ni en R4 (más permisivo): no se usaría tal como está. Ninguna aparece en las listas del contrato T1 (`628d89af`).
- **Evidencia:** ACCEPT-NOTES L190-L192 («a drive prefix (`C:`) counts as absolute»)

### L-17 — U-19 (a): «/**» final = prefijo terminado en «/» (`U19Normalized`)

- **Veredicto:** COINCIDE · **Conjunto:** R7 (scope) · **ACCEPT-NOTES:** E2
- **Texto normativo:** «U-19 = «/**» final = prefijo terminado en «/»; comodín no final = lista exacta de rutas en el árbol de BaseSha, enumerada en la orden; ningún commit del Worker solo de evidencia» (DEC L1234); «| U-19 | entera | — |» (DEC L1250); «Una entrada que termina en `/**` se lee como el prefijo terminado en `/` (16.5).» (FX-02-unblock-request L89)
- **Lectura del aceptante:** `x/**` se lee `x/`; R7 compara `U19Normalized` exacto: `[{Entry, List, Prefix}]`, primero Allowed y después Forbidden, en el orden de la delegación (E2). (`bmin_accept.py:703, 704, 1003, 2231`)
- **Lectura del instrumento:** Igual en `scope`. (`bmin_facts.py:667-668`)
- **¿Más de una lectura?:** no.
- **Lectura propuesta del Coordinator (literal):** «Una entrada que termina en `/**` se lee como el prefijo que termina en `/` (`docs/x/**` = `docs/x/`), en las dos listas de alcance y dondequiera que se lean (16.5).»
- **Si el Coordinator eligiera otra:** No hay otra lectura admisible; si el Coordinator fijara otra, el aceptante no se usaría tal como está.
- **Evidencia:** ACCEPT-NOTES L236 («own `U19Normalized`»); SPEC L311 («`{"Entry", "List", "Prefix"}`»)

### L-18 — U-19 (b): comodín no final

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R7 (scope) y R4 Scope · **ACCEPT-NOTES:** 7
- **Texto normativo:** «comodín no final = lista exacta de rutas en el árbol de BaseSha, enumerada en la orden» (DEC L1234); «Una entrada con comodín en otra posición se lee como la lista exacta de rutas que casan en el árbol de `BaseSha`, enumerada en la orden» (FX-02-unblock-request L89); «sin comodines» (AP L552)
- **Lectura del aceptante:** El comodín no llega a la delegación: si aparece una entrada con `*`, `?` o `[` fuera de un `/**` final, es mal formada (figura en `MalformedEntries`), cubre solo su cadena literal y hace que `Scope` no pueda ser `pass` en R4. No expande nada contra el árbol de `BaseSha`. (`bmin_accept.py:715, 716, 2075`)
- **Lectura del instrumento:** Igual: mal formada y cobertura literal, sin expansión. (`bmin_facts.py:682-684`)
- **¿Más de una lectura?:** sí: (b1) la orden enumera las rutas y la delegación nunca lleva el comodín (aceptante); (b2) un comodín que llega a la delegación se expande contra el árbol de `BaseSha`.
- **Lectura propuesta del Coordinator (literal):** «Un comodín no final no llega a la delegación: el Coordinator enumera en la orden las rutas exactas que casan en el árbol de `BaseSha`. Una entrada con `*`, `?` o `[` fuera de un `/**` final que aparezca en una delegación está mal formada: figura en `MalformedEntries`, cubre solo su cadena literal y `Scope` no puede ser `pass`.»
- **Si el Coordinator eligiera otra:** Con (b2), el aceptante rechazaría un `Scope` = pass legítimo (rechazo falso): no se usaría tal como está. El contrato T1 no tiene comodines no finales.

### L-19 — U-19 (c): ningún commit del Worker solo de evidencia

- **Veredicto:** COINCIDE (vacío: el aceptante no aplica la regla) · **Conjunto:** R7 (scope) · **ACCEPT-NOTES:** —
- **Texto normativo:** «ningún commit del Worker solo de evidencia» (DEC L1234)
- **Lectura del aceptante:** No la aplica: R7 solo compara `PerPath`, `MalformedEntries`, `U19Normalized`, A3, A4 y las listas de MV-4, y R4 no la evalúa. No tiene lectura propia. (`bmin_accept.py:82, 2218`)
- **Lectura del instrumento:** No la aplica.
- **¿Más de una lectura?:** no aplica (no hay lectura del aceptante).
- **Lectura propuesta del Coordinator (literal):** «U-19 (c) rige la conducta del Worker y la verificación del Controller tal como está; el aceptante no la aplica y esta disposición no fija lectura sobre ella.»
- **Si el Coordinator eligiera otra:** Ninguna para la regla 6 (coincidencia vacía).

### L-20 — A3: `AllowedWriteScope` ⊆ alcance permitido del contrato

- **Veredicto:** COINCIDE · **Conjunto:** R7 (contract) · **ACCEPT-NOTES:** 4, 5
- **Texto normativo:** «| A3 | `AllowedWriteScope` ⊆ alcance permitido del contrato | STOP (P-03) |» (AP L562); «A ⊆ B si cada entrada de A está cubierta por alguna de B (un prefijo de A solo lo cubre un prefijo de B que lo contenga).» (AP L553-L554); «| A3'-A5' | + `RoleRequirements` de la delegación ⊇ los del contrato por rol» (AP L1192); «con la sintaxis de alcance de AUTOMATION_PLAN §16 (archivo exacto o prefijo terminado en `/`).» (RAE L125)
- **Lectura del aceptante:** Por cada entrada a del `AllowedWriteScope` de la delegación, en su orden: `ContainedBy` = las entradas b del alcance permitido del contrato con u19(a) = u19(b), o con u19(b) terminada en `/` y u19(a) que empieza por u19(b). No recalcula A5 ni los añadidos de 16.22 (`RoleRequirements`, `SupersededCommits`): G3. (`bmin_accept.py:732, 1034, 2241-2242`)
- **Lectura del instrumento:** Sin U-19 en la entrada contenedora: L-22. (`bmin_facts.py:739`)
- **¿Más de una lectura?:** no, con U-19 aplicada (L-22).
- **Lectura propuesta del Coordinator (literal):** «A3 se lee entrada a entrada: cada entrada de `AllowedWriteScope` de la delegación debe estar contenida, tras U-19 en las dos, por alguna entrada del alcance permitido del contrato, con la regla de L553-L554.»
- **Si el Coordinator eligiera otra:** No hay otra lectura admisible; si el Coordinator fijara otra, el aceptante no se usaría tal como está. G3 sigue declarado para A5 y los añadidos de A3'-A5'.

### L-21 — A4: `ForbiddenWriteScope` ⊇ prohibido del contrato

- **Veredicto:** COINCIDE · **Conjunto:** R7 (contract) · **ACCEPT-NOTES:** 4, 5
- **Texto normativo:** «| A4 | `ForbiddenWriteScope` ⊇ prohibido del contrato | STOP (P-03) |» (AP L563)
- **Lectura del aceptante:** Por cada entrada prohibida del contrato, en su orden: `ContainedBy` = las entradas del `ForbiddenWriteScope` de la delegación que la contienen, con la misma regla y U-19 en las dos. (`bmin_accept.py:1035`)
- **Lectura del instrumento:** Sin U-19 en la entrada contenedora: L-22. (`bmin_facts.py:741`)
- **¿Más de una lectura?:** no, con U-19 aplicada (L-22).
- **Lectura propuesta del Coordinator (literal):** «A4 se lee entrada a entrada: cada entrada prohibida del contrato debe estar contenida, tras U-19 en las dos, por alguna entrada del `ForbiddenWriteScope` de la delegación.»
- **Si el Coordinator eligiera otra:** No hay otra lectura admisible; si el Coordinator fijara otra, el aceptante no se usaría tal como está.

### L-22 — U-19 dentro de la contención de A3 y A4

- **Veredicto:** COINCIDE · **Conjunto:** R7 (contract) · **ACCEPT-NOTES:** 5
- **Texto normativo:** «U-19 = «/**» final = prefijo terminado en «/»» (DEC L1234); «se lee como el prefijo terminado en `/` (16.5)» (FX-02-unblock-request L89); «A ⊆ B si cada entrada de A está cubierta por alguna de B» (AP L553-L554)
- **Lectura del aceptante:** Aplica U-19 a las dos entradas de la contención (la contenida y la contenedora). (`bmin_accept.py:724, 732`)
- **Lectura del instrumento:** No aplica U-19 en `contract`: una entrada contenedora `x/**` es una cadena literal que solo se contiene a sí misma (TOOL-NOTES 12). Divergencia confirmada por las sondas P2 y P3. (`bmin_facts.py:739, 741`; TOOL-NOTES L162)
- **¿Más de una lectura?:** no razonable: sin U-19, toda entrada `/**` del contrato sería mal formada (comodín, L552) y no contendría más que su propia cadena, lo que contradice el contrato custodiado T1, que usa `/**` en sus dos listas; la especificación §2.4, no normativa, calla.
- **Lectura propuesta del Coordinator (literal):** «U-19 se aplica también a la contención de A3 y A4 (16.5, L553-L554), en las dos entradas.»
- **Si el Coordinator eligiera otra:** Para la regla 6, ninguna. Queda una divergencia abierta entre instrumento y aceptante, no registrada en G1-G6 (§6, D-1): con una entrada contenedora `…/**` y una contenida distinta bajo ella, el aceptante rechaza los hechos del instrumento (R7 MISMATCH; sondas P2 y P3); con listas que copian las del contrato T1, coinciden (sonda P1). Nunca produce una aceptación indebida. Si el Coordinator adoptara la lectura del instrumento, el aceptante no se usaría tal como está.
- **Evidencia:** RAE L229-L230 («se sustituye por la enumeración de los demás archivos del diff»); r2/independence/README L267 («| Scope coverage |»)

### L-23 — R4 `Scope` con entradas vacías o mal formadas

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R4 Scope (fuera del paréntesis de la regla 6) · **ACCEPT-NOTES:** 7
- **Texto normativo:** «| 7 | `Scope` | `git diff --name-only BaseSha..CurrentSha` ⊆ `AllowedWriteScope` y sin intersección con `ForbiddenWriteScope` | Controller | STOP |» (AP L636); «`Scope` = pass exige en su `Evidence` la salida reproducible de `git diff --name-only BaseSha..CurrentSha` y la entrada de `AllowedWriteScope` que contiene cada ruta.» (AP L1205-L1206); «| Éxito declarado sin commit | Controller (`Identity`, `CleanTree`, `Scope`) |» (AP L660)
- **Lectura del aceptante:** `Scope` esperado = pass solo si ninguna entrada de las dos listas está vacía o mal formada (L-16), toda ruta del diff está cubierta por `AllowedWriteScope` y ninguna por `ForbiddenWriteScope` (cobertura de L-15, con U-19); con diff vacío, pass. (`bmin_accept.py:2075, 2078, 2081`)
- **Lectura del instrumento:** No aplica: el instrumento no decide.
- **¿Más de una lectura?:** sí: AP no dice que una entrada mal formada haga fallar `Scope`; en la lectura literal solo cubre su cadena. Diff vacío frente al modo «Éxito declarado sin commit» (L660).
- **Lectura propuesta del Coordinator (literal):** «En la verificación de FX-02, una entrada vacía o mal formada en `AllowedWriteScope` o `ForbiddenWriteScope` hace que `Scope` no pueda ser `pass`; si no la hay, `Scope` es `pass` si y solo si toda ruta de `BaseSha..CurrentSha` está cubierta por `AllowedWriteScope` y ninguna por `ForbiddenWriteScope`.»
- **Si el Coordinator eligiera otra:** Está fuera del paréntesis de la regla 6, pero aplica la misma cobertura y U-19. Con la lectura literal, el aceptante rechazaría un `pass` legítimo (rechazo falso): no se usaría tal como está, o S-03 durante la ronda. Las listas del contrato T1 no tienen entradas vacías ni mal formadas.
- **Evidencia:** ACCEPT-NOTES L191 («R4 Scope rule any malformed or empty entry»)

### L-24 — Conjunto de rutas del diff (renombres)

- **Veredicto:** AMBIGUOUS-ACCEPTOR-CHOICE · **Conjunto:** R7 (scope) y R4 Scope · **ACCEPT-NOTES:** 3, E6
- **Texto normativo:** «`git diff --name-only BaseSha..CurrentSha` ⊆ `AllowedWriteScope`» (AP L636); «un renombre da las dos rutas, la de origen y la de destino» (SPEC L315)
- **Lectura del aceptante:** Rutas del diff = las rutas cuya entrada (modo, tipo e identificador) difiere entre los dos árboles completos: un renombre aporta origen y destino; un cambio solo de modo también cuenta. (`bmin_accept.py:965`)
- **Lectura del instrumento:** Igual (`git diff --name-only --no-renames`). (`bmin_facts.py:643`)
- **¿Más de una lectura?:** sí: con la configuración por defecto de Git (`diff.renames`), `git diff --name-only` puede listar solo el destino de un renombre; el resultado de la orden literal depende de la configuración (SPEC E6 no es normativa).
- **Lectura propuesta del Coordinator (literal):** «El conjunto de rutas de `Scope` y de la cobertura de R7 es el de `git diff --name-only --no-renames BaseSha..CurrentSha`: un renombre aporta su ruta de origen y la de destino.»
- **Si el Coordinator eligiera otra:** Con detección de renombres, el aceptante sería más estricto (la ruta de origen también tiene que estar cubierta y no prohibida): rechazo falso; no se usaría tal como está.
- **Evidencia:** ACCEPT-NOTES L179-L180 («Renames»)

## 5. Ítems de ACCEPT-NOTES y erratas v0.2

| Ítem | Tema | Filas | Efecto sobre estas reglas |
|---|---|---|---|
| 1 | Checks como objeto; `not_run` = no pass | L-03 (y R4 en general) | coherente con 16.22 (AP L1204) |
| 2 | términos de 16.10 (E5) | fuera de la regla 6 | lectura propia de R4 `FreeText` (§6, D-6) |
| 3 | renombres (E6) | L-24 | cambia el conjunto de rutas de la cobertura |
| 4 | órdenes no fijados: conjuntos o multiconjuntos | L-07..L-10, L-20, L-21 | granularidad de comparación, no lectura |
| 5 | formatos tolerados; `CoveredBy`/`ContainedBy` tras U-19 | L-15, L-20..L-22 | granularidad de comparación |
| 6 | `SchemaViolations` solo por vacío | L-04 | comparación; residuo G1 |
| 7 | entradas mal formadas; regla de R4 `Scope` | L-16, L-18, L-23 | cambia la lectura |
| 8 | Markdown: encabezados, vallas, preámbulo, cuerpo | L-02, L-12, L-13 | cambia la lectura |
| 9 | `## 16.` y `### 16.3` únicos | L-13, L-14 | cambia la lectura |
| 10 | quitar el puntero: frase, última aparición, una vez | L-14 | cambia la lectura |
| 11 | `EntryNamesR163`: subcadena literal | L-02, L-14 | cambia la lectura |
| 12 | análisis del mapa; MV-3 con `Surfaces` del mapa; MV-3..MV-6 con forma básica; MV-5 normalizada | L-04, L-06, L-11, L-12 | cambia la lectura; residuo G2 en la comparación |
| 13 | MV-6 en todo archivo MODIFIED | L-12 | cambia la lectura |
| 14 | parte de mapa: E2-E3 y MV-1..MV-7; PRE_ACTIVATION libre | L-01..L-14 | define la parte de mapa |
| 15 | ejecutable de R2 | — | no afecta a estas reglas |
| 16 | vocabulario de R3 (E1) | — | no afecta a estas reglas |
| 17 | alcance de R5; R7 = reglas de lectura normativa | L-15..L-22 | define qué compara R7; no es lectura |
| 18 | R9: cierre, `:(literal)`, `RevPaths` | — | no afecta a estas reglas |
| 19 | `Identity`: rutas, ascendencia reflexiva, sin rama de rebase | fuera de la regla 6 | lectura propia de R4 (§6, D-6) |
| 20 | `Trailer` | fuera de la regla 6 | lectura propia de R4 (§6, D-6) |
| 21 | `NotApplicable` explícito | — | no afecta a estas reglas |
| E1 | excepciones de vocabulario | — | no afecta a estas reglas |
| E2 | `U19Normalized` estricto | L-17 | fija el formato |
| E3 | `:(literal)` en R9 | — | no afecta a estas reglas |
| E4 | activación ambigua; `claim_id` | L-01 | coherente con la lectura de L-01 |
| E5 | términos tras los dos puntos | fuera de la regla 6 | R4 `FreeText` |
| E6 | `--no-renames` | L-24 | cambia el conjunto de rutas |
| E7 | blobs solo por `RevPaths` | — | no afecta a estas reglas |

## 6. Divergencias abiertas

**Entre el aceptante y la lectura normativa propuesta: ninguna.** Las abiertas son estas:

- **D-1. U-19 en A3 y A4 (instrumento frente a aceptante; no está en G1-G6).** Instrumento: `bmin_facts.py:739, 741` (`covers(b, e)` sin `u19`) y TOOL-NOTES L162. Aceptante: `bmin_accept.py:730-732`. Se activa cuando una entrada contenedora termina en `/**` y la contenida es una cadena distinta bajo ella. Sondas (`probes/probe-result.json`): P1, contrato T1 y delegación que copia sus listas: ACCEPT_FACTS, R7 MATCH. P2, contrato T1 con el prefijo de evidencia sustituido por la enumeración de un archivo bajo él, como prescribe nc2 (RAE L229-L230) si hay más de un archivo del diff bajo el prefijo: REJECT_FACTS, R7 MISMATCH en `A3[docs/automation/evidence/FX-U1-agent/note-a.md].ContainedBy`. P3, sintética: REJECT_FACTS, R7 MISMATCH en dos filas de A3 y dos de A4. Efecto: si la delegación de la ronda, o la de un control negativo, activa el caso, toda solicitud del aceptante que incluya esa ejecución de `contract` da REJECT_FACTS y la salida queda sin aceptar (INVALID_OUTPUT, reejecuciones, STOP P-04, FX-02 UNVERIFIED); la prueba previa de la regla 6 tampoco obtiene ACCEPT_FACTS. Nunca da una aceptación indebida. Decisión del Coordinator (A6-9, regla 2): registrarla como divergencia conocida con la prueba previa como control; restringir la forma de las delegaciones de FX-02 para que no la activen; o cambiar una pieza (otro blob, otra corrección de A-6 y otra determinación del Owner).
- **D-2. G2:** existencia de MV-2..MV-6 en un mapa que viola su esquema. Afecta a la comparación de R6 y R7, no a la decisión de la parte de mapa, porque MV-1 ya es defecto.
- **D-3. G6:** encabezado sangrado (L-02, L-13) y mensaje de commit con CRLF (L-01). Solo rechazo falso.
- **D-4. G1:** MV-1 sin control en el nivel de los hechos (L-04).
- **D-5. Enumeración de la regla 6:** MV-7 y E2-E3 los aplica la parte de mapa, pero el paréntesis no los nombra. La disposición propuesta los cubre (L-01, L-02, L-14).
- **D-6. Lecturas propias de R4 fuera del paréntesis.** Se incluyen L-23 y L-24. Además, sin fila:

| Comprobación | Lectura del aceptante | Citas |
|---|---|---|
| R4 `Identity` | rutas de worktree con `normcase(abspath())`; ascendencia reflexiva; sin la rama de rebase y `patch-id` de 16.9 #5 (en una reverificación de 16.7 tras rebase, la lectura puede diferir) | ACCEPT-NOTES L222-L223; `bmin_accept.py:2049` |
| R4 `Trailer` | valor declarado de `DeclaredCoAuthoredBy` o `Worker.Trailer`; bloque de trailers de Git; «coherente con el modelo efectivo» no se comprueba; un rango vacío pasa | ACCEPT-NOTES L227-L228; `bmin_accept.py:2118` |
| R4 `FreeText` | 16 términos tras el primer «`:»; espacios colapsados, `casefold`, palabra completa; `Gate` no se examina | ACCEPT-NOTES L176-L178; `bmin_accept.py:652` |
| R4 `CleanTree` | `status --porcelain -z --untracked-files=normal` vacío y `check-ignore -q --no-index` de la ruta ignorada | `bmin_accept.py:2090`; `bmin_accept.py:2091` |

Si la disposición no las cubre, una divergencia sobre ellas durante la ronda sigue la regla 6 (S-03, STOP). El Coordinator puede ampliar la disposición con otra tabla, que aquí no se prepara.

## 7. Texto propuesto de disposición

> «**Disposición del Coordinator: lecturas normativas del aceptante fijado (A-6, A6-5, regla 6).** El Coordinator fija su lectura de las reglas en las que el aceptante aplica una propia y declara, regla por regla, que coincide con la del aceptante fijado en `docs/automation/evidence/I-62-F6/FX-02/bmin/accept/bmin_accept.py`, blob `434ef1fccb0d15762b30244b4a60928120698411` (SHA-256 `173514aa353f6add1b11ea73ef6da68bba752bacdca64baa0551a6a7be6d8bb0`). Su lectura es, literalmente, la «lectura propuesta del Coordinator» de las filas L-01 a L-24 de `D:\r62-a6m7\readings\readings.md`, con el SHA-256 que se registra con esta disposición:
>
> 1. parte de mapa de `Authority` en R4: E2-E3 (L-01, L-02), alcance y efecto (L-03), MV-1 (L-04), MV-2 (L-05), MV-3 (L-06), las cuatro listas de MV-4 (L-07 a L-10), MV-5 (L-11), MV-6 (L-12), lector Markdown (L-13) y MV-7 (L-14);
> 2. en R7: cobertura (L-15), entradas mal formadas (L-16), U-19 (L-17 a L-19), A3 (L-20), A4 (L-21), U-19 en A3 y A4 (L-22) y conjunto de rutas del diff (L-24);
> 3. R4 `Scope` con entradas vacías o mal formadas (L-23).
>
> Excepciones: ninguna. Hace constar que: (a) MV-7 y E2-E3 no figuran en el paréntesis de la regla 6, pero la parte de mapa los aplica y esta disposición los cubre; (b) U-19 (c) no la aplica el aceptante, así que la coincidencia es vacía (L-19); (c) la divergencia entre instrumento y aceptante sobre U-19 en A3 y A4 (L-22; D-1) no es una divergencia de lectura del aceptante, y el Coordinator decide aparte su tratamiento; (d) las lecturas propias del aceptante en R4 `Identity`, `Trailer`, `FreeText` y `CleanTree` quedan fuera de esta disposición, y una divergencia sobre ellas sigue la regla 6 (S-03, STOP). Esta disposición no se entrega al Controller (A6-1, regla 6), que aplica 16.9 y 16.13 tal como están (A6-9, regla 2).»

**Puntos que el Coordinator tiene que confirmar o corregir:** las 12 filas AMBIGUOUS (L-01, L-02, L-04, L-10, L-11, L-12, L-13, L-14, L-16, L-18, L-23 y L-24), el tratamiento de D-1 y la exclusión (d).

## 8. Verificación mecánica y sondas

- `python -I verify_citations.py` lee `readings.json` y comprueba cada cita contra los bytes de su blob, obtenidos con `git cat-file blob`. Una cita «quote» exige el texto con los espacios colapsados en las líneas citadas; una cita «token» exige la subcadena exacta. También comprueba el blob de cada fuente (SHA-1 propio de `blob <len>\0`), la identidad de AP en el fixture, la ausencia de «U-19» en V14, el contenido de `probes/probe-result.json` y la higiene de los entregables. Escribe `citations-result.json`.
- `python -I probes/probe_a3a4.py` toma por blob el instrumento, el aceptante y el contrato T1, ejecuta `contract` desde los bytes del instrumento por stdin y juzga con el aceptante fijado (R4, R5, R6 y R10 no aplicables; R1, R2, R3 y R9 en MATCH). Solo escribe bajo `probes/`.

