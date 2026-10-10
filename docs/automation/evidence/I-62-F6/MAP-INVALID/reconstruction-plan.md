# I-62 F6 — Plan de reconstrucción de la activación del fixture (decisiones §66, puntos 3 y 4)

Esto es solo preparación. Ninguna ref del fixture, ningún remoto ni GitHub se ha tocado. El candidato se construyó y se probó en repositorios
temporales `D:\r62-fixture\tmp-map-*`, sin remotos, que se borraron al terminar. Quedan:

- el constructor determinista [build_candidate.py](build_candidate.py);
- el comprobador [mv_check.py](mv_check.py);
- los controles negativos [neg_controls.py](neg_controls.py);
- el candidato (b) como paquete git [candidate-b.bundle](candidate-b.bundle) (SHA-256 `59b244936f7b5218458d68e58e7a6f3fa996bd0b7d1c2a90d5fb9d41ea45388f`).

La caracterización está en [characterization.md](characterization.md) y los efectos en [effects.md](effects.md).

## 1. Restricciones (§66)

- No parchear el resolver.
- No alterar autoridades normativas: el mapa `4d49d3e1`, su esquema y los textos de MC_I62 quedan intactos.
- No declarar MAP_VALID sin ejecutar MV-1..MV-7.
- No reescribir historial acreditado.
- Antes de sustituir refs, medir los efectos y pedir disposición.

## 2. Qué deben contener EFF'^1 y EFF' (diseño conforme al mapa sin cambios)

| Id | Invariante | Cómo se cumple en el candidato |
|---|---|---|
| I-1 | El mapa en EFF' es el blob `4d49d3e1`, con el esquema `6b54dda9` (ENTRY). En EFF'^1 el mapa está ausente | entra con F_i62 (segundo padre) |
| I-2 | Cada archivo MODIFIED tiene en EFF'^1 su `BaseBlob` y en EFF' su `EffBlob` (6 archivos) | EFF'^1 = blobs de RackCad `bb0d5522`; EFF' = blobs de `6f0187cb` |
| I-3 | Cada ADDED/ENTRY está **ausente** en EFF'^1 y presente con su `EffBlob` en EFF' (25 archivos, incluidos ADR-0048 y PROMPT_TEMPLATES) | solo por el segundo padre |
| I-4 | Toda otra ruta bajo las superficies tiene el mismo blob en EFF'^1 y en EFF' (`AGENTS.md`, `CLAUDE.md`, `model-catalog.md`, `prompting-guide.md`, los 5 esquemas `/v1`), y ningún archivo de superficie se borra | árbol comprobado |
| I-5 | Las autoridades de MC_I62 entran **solo por el segundo padre**. Si un solo commit de autoridades queda en first-parent antes del merge, EFF^1 las contiene y se repite el defecto | grafo S ← (N1 ← N2) ← E |
| I-6 | Un único commit con `Agent-Protocol-Normative: I-62` es alcanzable desde la `main` del linaje. EFF' es el primer merge first-parent que lo alcanza por el segundo padre, y EFF'^1 no lo alcanza | N2, con la línea exacta |
| I-7 | El cuerpo de EFF' termina con `Derived formal claim table:` y la cabecera de 7 columnas. Las filas son las unidades I61 anteriores: ninguna en el fixture. Sin Claim-Id duplicados | 0 filas |
| I-8 | Con MainSha_eval = `main`: `## 12.` aparece una sola vez en WORKFLOW y nombra 16.13 por su línea exacta; 16.13 aparece una sola vez; los punteros están en §16 y en 16.3; 16.3 en EFF' sin el puntero es igual a 16.3 en EFF'^1 | consecuencia de I-2 (blobs de F4) |
| I-9 | Los blobs invalidadores son idénticos a los del linaje r1 y de RackCad: routing `bba08fc4`, catálogo `166d978d`, codex-cli `155f3469`, claude-cli `ae570380`, mapa `4d49d3e1`, esquema `6b54dda9`, AP `f525cb1e` y WORKFLOW `446c32ba` | comprobado en [tree_compare.txt](tree_compare.txt) |
| I-10 | Los FIXTURE_LOCAL de EFF'^1 son byte a byte los del F_seed original (9 archivos). Solo cambia `FIXTURE-MANIFEST.json`, que está fuera de las superficies | comprobado |
| I-11 | El reclamo de la unidad nueva es posterior y su padre es EFF'. No aparece en PRE (`claim_parent_contains_effective` = true) | C ← E |
| I-12 | Las refs del linaje r1 (`main`, `fixture/i62-norm`, `fx/u1*`, `ci/smoke`, el tag `test-activation/I-62`) y la instantánea `fx04a-qh2-origin.git` no se modifican | opción (b) |

**Disponibilidad byte a byte: sí.** Los 6 `BaseBlob` están en RackCad `bb0d5522`, que es el `origin/main` actual de RackCad. Los 31 `EffBlob` están en
`6f0187cb`, que es ancestro de la punta del worktree. El constructor lee cada blob con `git cat-file`, lo escribe con `hash-object --no-filters` y
comprueba que el SHA-1 resultante es igual al de origen. Sin filtros de fin de línea; además, `.gitattributes` = `* -text`.

**Árbol de EFF' frente al EFF actual `fbe25347`:** solo hay tres diferencias:

- `FIXTURE-MANIFEST.json` (FIXTURE_LOCAL);
- `docs/adr/0048-…md`, añadido (`e1bd8d91`);
- `docs/initiatives/PROMPT_TEMPLATES.md`, añadido (`384d0b0c`).

Las autoridades que ya tenía el fixture no cambian ni un byte. Se añaden las dos que faltaban, copiadas de MC_I62.

**FIXTURE_LOCAL que se conservan:**

- de F_seed: `.gitattributes`, `.github/workflows/fixture.yml` (versión original, sin TRX), `AGENTS.md`, `CLAUDE.md`, `README.md`, y `src/` y `tests/`
  (4 archivos);
- `FIXTURE-MANIFEST.json`, nuevo en tres estados:
  - S: base `bb0d5522`, 13 COPIED, `Activation` null;
  - N1: `SourceCommit` `6f0187cb`, 39 COPIED;
  - N2: `Activation` TEST-ACTIVATION.

  Los tres llevan un bloque `Reconstruction` con el linaje r2, el EFF r1 sustituido, la causa y el blob del mapa.

**Variante que decide el Coordinator:** llevar al seed los cambios de la unidad r1 que no son superficies. Son el workflow con el artefacto TRX (U-07,
`b6d294e`) y `.gitignore` (U-08, `c785def`). No afecta a MV porque están fuera de las superficies. Ahorra dos commits de órdenes en la unidad nueva,
pero F_seed deja de ser byte a byte el original. Por defecto **no**: se vuelven a aplicar en la unidad, por orden del Coordinator.

## 3. Candidato construido y pruebas (hechas en `D:\r62-fixture\tmp-map-*`, ya borrados)

Fechas fijas `2026-10-10T02:00:0X+00:00` (X = 0..5) e identidad sintética `fixture <fixture@example.invalid>`. Los SHA son reproducibles: una segunda
construcción en otro directorio dio los mismos 6 SHA ([build_b.json](build_b.json) = [build_b2.json](build_b2.json)).

| Objeto | Opción (b): repositorio nuevo | Opción (a): mismo repositorio, refs `*-r2` |
|---|---|---|
| S = EFF'^1, F_seed r2 (`main`) | `55d190043458f461f187588ea79d8e9839440e28` | igual |
| N1, F_i62 r2 | `15a20a2f1b120a12784960abb0b30819e7e7cb53` | igual |
| N2, F_norm r2 + trailer (`fixture/i62-norm[-r2]`) | `df3d51a27d83f5f47604f2057ee18dd4f48f02db` | `a274a22ca60ec5228a9649f6d1a08fd357dba3ac` |
| E = EFF' (`main` / `main-r2`) | `88ab34c398f03070c94a584f95196a394d869f7c` | `28d8bcb85554d8bffa0842afba62efbc2049a3bb` |
| tag anotado | `test-activation/I-62` → objeto `fff998cb…` | `test-activation/I-62-r2` → `9f4f2443…` |
| C, reclamo de FX-U2 (`fx/u2`; Claim-Id `fc62f1c7-0000-4000-8000-000000000002`) | `6040097c5f05277d4d786a99bfc80970665c1e5f` | `bd486c82fdd9ac2d2ee86ba1586b9debbc89e321` |

N2 y E dependen del texto del manifiesto (`Rule`/`Tag`), que cambia con la opción. C depende del nombre de la unidad y del Claim-Id. S, N1 y la validez
del mapa no dependen de ninguno de los dos.

| Prueba | Resultado | Salida |
|---|---|---|
| T-1 construcción (b); remotos del candidato = ninguno | 13 + 39 copias comprobadas por SHA; 10 FIXTURE_LOCAL | [build_b.json](build_b.json) |
| T-2 `mv_check` con MainSha_eval = `main` del candidato | Derive: EFF `88ab34c3` y trailer único; E2 \|X\| = 1, E3 \|R\| = 1; PRE OK (0 filas); **MV-1..MV-7 pass → MAP_VALID** | [mv_candidate_b.txt](mv_candidate_b.txt), [.json](mv_candidate_b.json) |
| T-3 `mv_check` con MainSha_eval = punta `fx/u2` | MAP_VALID (EFF sigue siendo `88ab34c3`) | [mv_candidate_b_claim.txt](mv_candidate_b_claim.txt) |
| T-4 comprobador de F4 `compat/clause_map.py check 55d19004 88ab34c3` | **EQUAL (MV-2..MV-6)**: 31 archivos; entradas MODIFIED 15, ADDED 53, ENTRY 2, REMOVED 0. El mismo comprobador da 33 fallos sobre el fixture actual | [f4_clause_map_crosscheck.txt](f4_clause_map_crosscheck.txt) |
| T-5 controles negativos sobre una copia: N-1 PROMPT_TEMPLATES ausente en EFF^1; N-2 `AGENTS.md` cambiado en la rama; N-3 ADR-0048 ausente; N-4 trailer duplicado; N-5 merge sin PRE; N-6 puntero de §16 retirado; N-7 los dos punteros retirados | **7/7 como se esperaba**: [MV-4, MV-6], [MV-3], [MV-3, MV-4], ACTIVATION_INVALID, ACTIVATION_INVALID, [MV-4, MV-7], [MV-4, MV-6, MV-7] | [neg_controls.txt](neg_controls.txt) |
| T-6 árboles: EFF' frente a `fbe25347`; docs de EFF'^1 frente a RackCad `bb0d5522`; docs de EFF' frente a `6f0187cb`; FIXTURE_LOCAL frente a `930288c5` | 3 diferencias (manifiesto + 2 añadidos); 13/13 iguales; 39/39 iguales; 9/9 iguales | [tree_compare.txt](tree_compare.txt) |
| T-7 blobs invalidadores en EFF', en `fbe25347` y en `95bdc29d` | 8/8 iguales | [tree_compare.txt](tree_compare.txt) |
| T-8 reproducibilidad | mismos SHA en una segunda construcción | [build_b2.json](build_b2.json) |
| T-9 opción (a) en un clon del fixture | `main-r2` → **MAP_VALID** (EFF `28d8bcb8`); `main` (r1) → MAP_INVALID, sin cambios; `git log --all` ve **2** commits con el trailer (`a274a22`, `a9f6c92`); **Classify(FX-U1, estado r1 en `95bdc29d`) frente a EFF' = UNKNOWN** (paso 5: `effective_sha` `fbe25347` ≠ EFF') | [mv_candidate_a.txt](mv_candidate_a.txt) |

**Conclusión de las pruebas.** Un candidato que respeta las invariantes I-1..I-12 da MAP_VALID con el mapa y las autoridades sin cambios. Lo
confirman dos comprobadores independientes: el de esta entrega y el de C-20b de F4.

## 4. Por qué la unidad no se puede trasladar (y hay que volver a arrancarla)

- `protocol.set`, `protocol.effective_sha` y `protocol.basis` son **inmutables desde el BOOTSTRAP** (AP 16.28, L1582). La reconciliación de rebase los
  deja «sin cambio» (L1423).
- `Classify`, paso 5, exige `protocol.effective_sha = EFF`. Con EFF' cualquier estado de FX-U1 da **UNKNOWN** (T-9). Solo una decisión del Coordinator
  tras un STOP (paso 6) puede clasificarlo, y eso no repara el `effective_sha` guardado.
- Reproducir la cadena r1 sobre r2 sustituyendo SHA **fabricaría custodia**: preflights CUSTODY, bindings, sesiones observadas, QU/QH/QR. No está
  permitido.
- Por tanto: **una unidad nueva sobre EFF'.** Propuesta: `FX-U2`, `fx/u2`, Claim-Id `…0002`, para no mezclar evidencia con FX-U1. Decide el
  Coordinator; solo cambia C.

Cadena mínima para FX-02:

1. reclamo C (paso 4 de D.1, supervisión);
2. orden O1 del Coordinator del fixture;
3. sesión de Principal con preflights CUSTODY;
4. BOOTSTRAP (`effective_sha` = EFF', `claim_commit` = C, `claim_parent_sha` = EFF');
5. decisión de G0 con los tres marcadores y el binding;
6. QU de aceptación;
7. órdenes U-07/U-08 (salvo la variante del seed);
8. contrato de T1 (`gate-contract/v2`; su **emisión ejecuta Evaluate** y registra MAP_VALID; observación AUTHOR según CD-19/D-6);
9. órdenes de FX-02, clon nuevo, preflight y P-07.

Si el titular que abre FX-U2 es el que ejecuta FX-02, no hacen falta QH ni T16.

## 5. Opciones

### (b) Repositorio de fixture nuevo: origen local nuevo y repositorio público nuevo en GitHub — **recomendada**

Pasos (todos posteriores a la disposición y a las autorizaciones de la sección 7):

1. Congelar el candidato: script, argumentos, fecha, unidad y Claim-Id. Custodiar en RackCad `build_candidate.py`, `mv_check.py`, `neg_controls.py`
   y sus SHA-256.
2. `git init --bare -b main D:\r62-fixture\fixture-origin-r2.git`: un directorio **nuevo**; los existentes no se tocan.
3. Reconstruir en `D:\r62-fixture\tmp-map-publish`, sin remotos, con el mismo script. Comprobar los 6 SHA congelados y repetir T-2, T-4 y T-5.
4. `git push D:/r62-fixture/fixture-origin-r2.git 88ab34c3…:refs/heads/main df3d51a2…:refs/heads/fixture/i62-norm refs/tags/test-activation/I-62 6040097c…:refs/heads/fx/u2`.
   Después, `ls-remote` = SHA congelados. Borrar el temporal.
5. Clon de supervisión `D:\r62-fixture\supervisor-r2`, con configuración local (`core.autocrlf false` e identidad sintética).
6. Auditoría pública de los objetos (procedimiento de `I-62-F6/CI/public-audit.json`): sin identificadores reales.
7. Con autorización del Owner: crear el repositorio público, añadirlo como segundo remoto **del fixture r2** (nunca de RackCad) y hacer push de las
   mismas refs.
8. Comprobar Actions. Lección de R1/R2: la primera publicación de un repositorio vacío no dispara `push`; se publica después una rama de humo.
   Obtener corridas `fixture-build`/`fixture-tests` en `success` para `main` y `fx/u2`, y registrar los RemoteFacts.
9. `mv_check` sobre clones nuevos del origen r2 y de GitHub r2: MAP_VALID. Registrarlo en la evidencia de RackCad.
10. Volver a arrancar la unidad (sección 4) **cuando el camino de FX-02 pueda ejecutarse** (ver la recomendación).
11. Regenerar el staging de FX-02/FX-06 con los SHA r2 (lista en [effects.md](effects.md)) y volver a sellar los controles que citan SHA.

| Criterio | Resultado |
|---|---|
| EFF único para Derive(MainSha_eval) | sí, también en todo el repositorio (un solo trailer) |
| `origin/main` literal (16.14 L945, A6 L565, `Remote` L635) | **se cumple sin reinterpretación** |
| Blobs de routing/catálogo/descriptor/mapa | idénticos (I-9) |
| Refs acreditadas | intactas (repositorio r1 congelado como evidencia de F-MAP-1) |
| Coste | repositorio público nuevo (exterior; autorización del Owner, regla OD-7 «público»), CI nueva, auditoría, dos fixtures que distinguir en la evidencia |

### (a) Linaje nuevo en el mismo repositorio, con refs nuevas (`main-r2`, `fixture/i62-norm-r2`, `fx/u2`, tag `test-activation/I-62-r2`)

Pasos: 1-3 como en (b), en un clon del origen actual. Después, push de **solo** las refs nuevas a `fixture-origin.git` y al repositorio público
actual. Las existentes no se tocan.

| Criterio | Resultado |
|---|---|
| EFF único | sí para la historia de `main-r2` (T-9); **2 trailers en el repositorio** (`git log --all`), un riesgo de confusión en herramientas y auditorías |
| `origin/main` literal | **no se cumple**: `origin/main` sigue siendo `fbe25347` (r1, MAP_INVALID). A6 compara `MainSha` con `origin/main` (S-13), la comprobación `Remote` exige `origin/main = MainSha` y 16.14 deriva sobre `origin/main`. Funcionaría solo con una regla local del fixture «`main-r2` en el papel de `origin/main`» (texto del manifiesto del candidato (a)), es decir, una relectura local de términos normativos. Cada receta, contrato, clon (`make-A2-clone.ps1` `ExpectedMain`) y Controller tendría que usarla, y un solo `origin/main` literal mide el linaje equivocado |
| Blobs | idénticos |
| Refs acreditadas | intactas; se publican refs nuevas en el repositorio público existente (exterior; autorización) |
| Valoración | **no recomendada**: evita crear un repositorio, pero compra esa ventaja con una desviación en la lectura de A6/`Remote`/16.14 |

### (c) Mismo repositorio, sustitución de refs (r1 archivado y `main` movida a r2)

Pasos:

1. Crear `archive/r1-main` → `fbe25347` y `archive/r1-fixture-i62-norm` → `a9f6c929`.
2. Hacer un **force-update no fast-forward** de `main` y `fixture/i62-norm` a los SHA r2, en el origen local y en GitHub.
3. Crear el tag nuevo `test-activation/I-62-r2`. El tag r1 no se puede mover sin reescribirlo.

| Criterio | Resultado |
|---|---|
| EFF único | sí para `main`; 2 trailers en el repositorio |
| `origin/main` literal | se cumple |
| Refs acreditadas | **sustituidas**: es lo que §66.4 pide caracterizar antes. Ningún commit se reescribe, pero `main`, que cita la evidencia acreditada (FX-01 RemoteFacts `main = fbe25347`, la tarjeta de lanzamiento de A2 «main = fbe2534», `ls-remote` de la sonda A4-1, el `ExpectedMain` del clon A2), deja de poder comprobarse en vivo. Todos los clones r1 (A, A2, B, B2, B3, R, arch, supervisor) verían un forced update. Hay un force-push en un repositorio público |
| Valoración | **no recomendada** |

### (d) No reconstruir

FX-02 en r1 termina siempre en `Authority` fail → STOP, así que C-24 no puede pasar con el oráculo D-0 (PASS = VERIFIED). Coste cero, pero solo sirve
si el Coordinator decide cerrar C-24 como limitación.

### Recomendación

Elegir **(b)**. Es la única que cumple a la vez:

- el texto literal de 16.13/16.14 y A6;
- la intangibilidad de las refs acreditadas;
- la identidad byte a byte de las autoridades y de los blobs invalidadores.

**Orden propuesto.** FX-02 sigue bloqueada por A4-1 NOT_DEMONSTRATED (Controller no elegible), con independencia del mapa. Por eso:

1. disponer ya la opción y congelar el candidato (sin coste de sesiones);
2. publicar el linaje de activación r2 cuando el Owner autorice el repositorio;
3. **aplazar el nuevo arranque de la unidad**, que consume sesiones de Principal, hasta que exista un camino elegible para el Controller (una
   actualización real del runtime o una alternativa autorizada, §66.5).

Así no se gasta presupuesto de sesiones en una unidad que no podría completar FX-02.

## 6. Plan de pruebas posterior a la publicación (para la ejecución)

| Id | Prueba |
|---|---|
| P-1 | Los SHA publicados son iguales a los congelados (`ls-remote` en el origen r2 y en GitHub r2) |
| P-2 | `mv_check` en clones nuevos de ambos remotos: MAP_VALID; el comprobador de F4 da EQUAL |
| P-3 | Corridas push de CI con los dos jobs en `success` para `main` y `fx/u2`; los RemoteFacts se registran |
| P-4 | Auditoría pública antes del primer push a GitHub |
| P-5 | En la emisión del contrato de T1 de FX-U2, Evaluate/Resolve registran EFF', el blob del mapa y MAP_VALID. `mv_check` sirve de contraste independiente; el Controller lo repite en `Authority` |
| P-6 | El repositorio r1 y la instantánea QH2 siguen sin cambios: el digest de las refs es igual antes y después |

## 7. Riesgos

| Id | Riesgo | Mitigación |
|---|---|---|
| R-1 | Exposición en un repositorio público nuevo | auditoría P-4; solo identidad sintética; el manifiesto no contiene rutas locales (comprobado: ni nombres de usuario ni correos en el candidato) |
| R-2 | Confusión entre los fixtures r1 y r2 en kits, scripts y detectores | bloque `Reconstruction` en el manifiesto; registro en RackCad; directorios nuevos (`*-r2`); `D:\r62-fixture\A2` se conserva intacto como directorio de la medición A4-1 |
| R-3 | Presupuesto de sesiones (U-14) | ver la sección 8; no arrancar la unidad hasta que FX-02 sea ejecutable |
| R-4 | Cualquier cambio de texto, fecha, unidad o Claim-Id cambia los SHA | volver a ejecutar T-2/T-4/T-5 y volver a congelar; el script es determinista |
| R-5 | ADR-0048 y PROMPT_TEMPLATES aparecen por primera vez en el fixture | son copias byte a byte de MC_I62 y las exige el mapa (MV-3/MV-4); se declaran en el registro |
| R-6 | (a)/(c): doble trailer en el repositorio; `origin/main` no literal (a); force-push (c) | elegir (b) |

## 8. Decisiones y autoridades necesarias

| Decisión | Quién |
|---|---|
| Elegir la opción (recomendada: b) y aprobar las invariantes I-1..I-12 y el texto del manifiesto | Coordinator |
| Nombre de la unidad y Claim-Id (propuesta: FX-U2, `fc62f1c7-0000-4000-8000-000000000002`); fecha fija de los commits; variante del seed (U-07/U-08) | Coordinator |
| Crear el repositorio público `rackcad-i62-fixture-r2` y hacer push (b); o publicar refs nuevas o forzar `main` en el repositorio público actual (a/c) | **Owner** (acción exterior; OD-7: público por defecto) |
| Sesión(es) de Principal para el nuevo arranque. La autorización vigente `A4-PRINCIPAL-A2-CONSUMO` y A4-6 dicen literalmente «titular A2 de FX-02 designado por T16»; U-14 ya cuenta A + R = 2 | Coordinator (si se pide una línea de consumo nueva o una A-n; §66: «No se amplía A-4 por defecto») y **Owner** (línea de consumo) |
| Efectos en lo acreditado: C-22, alcance de C-25a, U-17 (b), los controles sellados, N9, AUTHOR, U-07/U-08 (ver [effects.md](effects.md)) | Coordinator |
| Momento de la publicación y del nuevo arranque (propuesta: linaje ya; unidad tras resolver el Controller) | Coordinator |
| **No hace falta:** ningún cambio del resolver, del mapa, de los textos normativos, del Freeze ni de A-1..A-4 | — |
