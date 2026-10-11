# I-62 A-5 — Preflight (decisiones §71, punto 4)

> Supervisión, plano a. Preparación, **no aplicación**: ni repositorio B1, ni FX-U2, ni sonda, ni revisión formal (decisiones §71, puntos 5 y 7).
> Todo se ejecutó en solo lectura sobre el worktree de I-62 (HEAD `efcfcbc5`) y sobre el fixture actual (a través de un clon temporal), y en
> repositorios temporales propios `D:\r62-fixture\tmp-a5-*`, sin remotos, borrados al terminar. Ninguna invocación de modelo ni de CLI de modelos.
> Rutas del worktree: `%USERPROFILE%\.codex\worktrees\architecture-portabilidad-coordinador-principal` (los scripts la derivan de `USERPROFILE`).

**Veredicto del preflight: PASS** en los ocho apartados (a)-(h). La tabla resume; las salidas exactas están en `out/`.

| Apartado | Resultado | Cifras | Salidas |
|---|---|---|---|
| (a) MV-1..MV-7 con el método de A5-1 | **PASS** | candidato MAP_VALID 7/7 en `main` y en la punta del reclamo; F4 `clause_map.py` EQUAL (31 archivos; 15/0/53/2 entradas); 11/11 negativos de MV como se esperaba; conformidad estructural 5/5 como se esperaba; reproducible (dos construcciones, mismos SHA); fixture actual MAP_INVALID (MV-3 31, MV-4 7, MV-6 69, MV-7 2) | `build_a5*.json`, `mv_*.json/.txt`, `f4_clause_map_a5.json`, `neg_controls_a5.*`, `conformance_a5.json` |
| (b) autoridades intactas | **PASS** | 41/41 archivos bajo las superficies en EFF' iguales (39 autoridades = MC_I62 `6f0187cb` = RackCad HEAD; 2 `FIXTURE_LOCAL` = fixture actual); 0 autoridades de D.1 ausentes; 8/8 invalidadores iguales en EFF', en el EFF actual, en `fx/u1` y en MC_I62 | `authorities_a5.json` |
| (c) consecuencias | **PASS** (enumeradas) | C-22, C-25a, oráculo D-0, sellados, staging de FX-02 y FX-06 | §3 |
| (d) presupuesto físico | **PASS** | 22 filas; FX-02 sin consumo (0/11, 0/4, 0/15); Principal de la ronda A 2 usadas; A4-6 reservada sin usar; A5-2 propuesta | `budget-matrix.json` |
| (e) OWNER-RESERVED | **PASS** (identificadas) | 2 nuevas para la aplicación; 2 posibles en el acuerdo (Q-A5-01, Q-A5-09) | §5 |
| (f) contratos de producción | **PASS** | commit candidato hipotético: solo A-5 y `I-62-A5/` añadidos; diff de producción vacío; guardas `preview` PASS | `candidate-commit.json`, `a5-guards-result-preview.json` |
| (g) líneas del Owner | **PASS** | 2 líneas exactas | §7 |
| (h) ruta de C-24 | informe factual | sin ruta demostrada hoy: el Controller sigue NOT_DEMONSTRATED | §8 |

## 1. (a) MV-1..MV-7 con el método de A5-1

**Constructor.** `build_candidate_a5.py` adapta `I-62-F6/MAP-INVALID/build_candidate.py` (blob `d637c2f4…`) al texto exacto de A5-1:

- F_seed, commit inicial de `main`: para cada archivo MODIFIED del mapa, su `BaseBlob` (6/6 comprobados en RackCad `bb0d5522`); ADDED, ENTRY y el
  mapa, ausentes (26, ninguno existe en `bb0d5522`); las demás autoridades, con su blob de F_norm (7/7, iguales en `bb0d5522`); los 9
  `FIXTURE_LOCAL` del F_seed original, byte a byte; y el manifiesto. 13 copias.
- F_norm, **un solo commit** sobre F_seed: las autoridades de MC_I62 byte a byte (lista de D.1 y `Files` del mapa; 39 copias, el mapa incluido), el
  manifiesto con TEST-ACTIVATION y el trailer único.
- F_eff, merge sin avance rápido (primer padre F_seed) con la tabla PRE sin filas; tag `test-activation/I-62`.
- El reclamo de FX-U2 (`fx/u2`) **solo** se crea si antes `mv_check.py` da MAP_VALID sobre `main` (regla 4 de A5-1): `Precondition.MapValid` = true.

| Objeto | SHA (fecha fija 2026-10-11T02:00:0X+00:00, identidad sintética) |
|---|---|
| F_seed | `a607662138862df11056977ba99b711a97b32a55` |
| F_norm | `10d0a2e373f501ea2617c7efc635926aac1378b9` |
| F_eff | `b58c6135f49559fd62cc8c138c66fa2b1b841323` |
| tag | `81a44750ae1a25fe01dc5dd70798e6a20f2d4cd5` |
| reclamo FX-U2 (Claim-Id `fc62f1c7-0000-4000-8000-000000000002`, propuesta) | `282c8523adb022c8fd573598f207b0e781f38274` |

Reproducibilidad: una segunda construcción en otro directorio dio los mismos cinco SHA (`build_a5_repro.json`). Remotos del candidato: ninguno.

**MV-1..MV-7** (comprobador `I-62-F6/MAP-INVALID/mv_check.py`, blob `9536d051…`, sin cambios):

| Revisión | Derive | E2/E3 | PRE | MV-1 | MV-2 | MV-3 | MV-4 | MV-5 | MV-6 | MV-7 | Veredicto |
|---|---|---|---|---|---|---|---|---|---|---|---|
| candidato, `main` = F_eff | EFF `b58c6135`, 1 trailer | 1/1 | OK, 0 filas | pass | pass | pass (31/31) | pass (FileKind 0) | pass | pass (70/70: 15/0/53/2) | pass | **MAP_VALID** |
| candidato, `fx/u2` | EFF `b58c6135` | 1/1 | OK | pass | pass | pass | pass | pass | pass | pass | **MAP_VALID** |
| fixture actual (referencia), `main` = `fbe25347` | EFF `fbe25347`, 1 trailer | 1/1 | OK | pass | pass | FAIL 31 | FAIL 7 (FileKind 24) | pass | FAIL 69 | FAIL 2 | **MAP_INVALID** |

**Contraste independiente (C-20b de F4):** `clause_map.py check F_seed F_eff` en el candidato = **EQUAL (MV-2..MV-6)**, 31 archivos, entradas
MODIFIED 15, REMOVED 0, ADDED 53, ENTRY 2, mapa `4d49d3e1…` (`f4_clause_map_a5.json`).

**Controles negativos de MV** (`neg_controls_a5.py` sobre una copia; 11/11 como se esperaba):

| Caso | Esperado | Obtenido |
|---|---|---|
| N-1 PROMPT_TEMPLATES ausente en F_seed | MV-4, MV-6 | MAP_INVALID [MV-4, MV-6] |
| N-2 `AGENTS.md` cambiado en F_norm | MV-3 | MAP_INVALID [MV-3] |
| N-3 ADR-0048 ausente en F_norm | MV-3, MV-4 | MAP_INVALID [MV-3, MV-4] |
| N-4 trailer duplicado en `main` | ACTIVATION_INVALID | ACTIVATION_INVALID (trailer duplicado) |
| N-5 F_eff sin tabla PRE | ACTIVATION_INVALID | ACTIVATION_INVALID (PRE ausente) |
| N-6 puntero de §16 retirado | MV-4, MV-7 | MAP_INVALID [MV-4, MV-7] |
| N-7 punteros de §16 y 16.3 retirados | MV-4, MV-6, MV-7 | MAP_INVALID [MV-4, MV-6, MV-7] |
| N-8 F_seed con las autoridades de MC_I62 (V14 D.1 literal) | MV-3, MV-4, MV-6, MV-7 | MAP_INVALID [MV-3, MV-4, MV-6, MV-7] |
| N-9 autoridades de MC_I62 por el primer padre | MV-3, MV-4, MV-6, MV-7 | MAP_INVALID [MV-3, MV-4, MV-6, MV-7] |
| N-10 esquema del mapa (ENTRY) presente en F_seed | MV-3 | MAP_INVALID [MV-3] |
| N-11 `routing.md` (MODIFIED) con su EffBlob en F_seed | MV-3, MV-4, MV-6 | MAP_INVALID [MV-3, MV-4, MV-6] |

**Conformidad estructural con el texto de A5-1** (`a5_conformance.py`, K-1..K-11; 5/5 como se esperaba):

| Repositorio | Esperado | Obtenido | Fallos |
|---|---|---|---|
| candidato A5-1 | CONFORMANT | CONFORMANT | — |
| fixture actual (V14 D.1 literal) | NON_CONFORMANT | NON_CONFORMANT | K-7 (F_seed no es el estado anterior a la activación), K-8 (faltan ADR-0048 y PROMPT_TEMPLATES) |
| candidato (b) de §66 (paquete `59b24493…`) | NON_CONFORMANT | NON_CONFORMANT | K-3 (la rama de activación tiene dos commits); es MAP_VALID, pero no sigue la letra de A5-1 |
| N-8 | NON_CONFORMANT | NON_CONFORMANT | K-7, K-10, K-11 |
| N-9 | NON_CONFORMANT | NON_CONFORMANT | K-2, K-3, K-7, K-10 |

Árboles frente al candidato (b) de §66: F_seed y F_eff difieren de sus homólogos solo en `FIXTURE-MANIFEST.json` (fuera de las superficies).

## 2. (b) Autoridades normativas intactas

- **Superficies en EFF' (F_eff del candidato):** 41 archivos. Las 39 autoridades son byte a byte iguales a RackCad MC_I62 `6f0187cb` y a la HEAD
  del worktree `efcfcbc5`; `AGENTS.md` y `CLAUDE.md` (`FIXTURE_LOCAL`) son iguales a los del EFF actual del fixture. 0 diferencias; 0 autoridades de
  D.1 ausentes (`authorities_a5.json`).
- **Invalidadores de medición** (`routing.md` `bba08fc4…`, `model-catalog.md` `166d978d…`, `adapters/codex-cli.md` `155f3469…`,
  `adapters/claude-cli.md` `ae570380…`, mapa `4d49d3e1…`, esquema `6b54dda9…`, AUTOMATION_PLAN `f525cb1e…`, WORKFLOW `446c32ba…`): **8/8**
  iguales en EFF', en el EFF actual `fbe25347`, en `fx/u1` `95bdc29d` (clon de las sondas) y en MC_I62. Ninguna medición anterior queda obsoleta por
  el fixture nuevo.
- **RackCad:** A-5 no toca ninguna autoridad (guardas G3 y G4, §6): mapa, esquema, árbol `agent-execution` `809fd58f…`, AUTOMATION_PLAN,
  WORKFLOW, LIFECYCLE, PROMPT_TEMPLATES, ADR-0046 y ADR-0048 sin cambio; C-20b EQUAL.
- **Fixture actual:** el digest de sus refs es el mismo antes y después del preflight (`out/ref-digest-before.txt` = `out/ref-digest-after.txt`,
  SHA-256 `adfbe5e4…`); los clones existentes (A, A2, B, B2, B3, R, arch, supervisor) conservan su `HEAD`, su árbol limpio y el `mtime` de su índice.

## 3. (c) Consecuencias sobre C-22, C-25a, los oráculos y la evidencia sellada

| Elemento | Estado acreditado | Efecto de A-5 | Qué haría falta en la aplicación (decide el Coordinator) |
|---|---|---|---|
| **C-22** («traza D.1 hasta un contrato I62 válido», V14 L2361) | PASS (decisiones §51): BOOTSTRAP `1746b404`, G0 `5a3a7d69`, QU `1a4fc9c6`, T1 `d30fb6a9` (blob `628d89af`, `MainSha` `fbe25347`); validado con `Test-Json` y coincidencia de blobs, **sin** `Evaluate` | el registro sigue siendo verdadero, pero su premisa «contrato I62 válido» cae por MAP_INVALID mismo: con 16.13 literal, la emisión de T1 sobre `fbe25347` acaba en STOP. A-5 cambia el oráculo de C-22 («D.1») solo para B1 | conservar el PASS de §51 como registro histórico de la cadena r1 y volver a acreditar C-22 en B1 con el arranque de FX-U2, sin coste adicional: la emisión del T1 de FX-U2 ejecuta `Evaluate` y registra MAP_VALID (Q-A5-03, Q-A5-05) |
| **C-25a** (FX-04a B3) | PASS 23/23 (decisiones §60) sobre la instantánea QH2 (`cabed547`) | intacto: ninguno de los 23 campos es EFF, clasificación, `Authority` ni mapa; el oráculo no modela `Evaluate`. Mide la portabilidad de la derivación de custodia, no la corrección normativa del paso siguiente | confirmar en el registro ese alcance; sin B4; FX-04a no se repite en FX-U2 (Q-A5-10) |
| **Oráculo congelado de FX-02, D-0** (decisiones §65, punto 4) | PASS = VERIFIED + negativos con su disposición; la revisión del Architect es una obligación posterior (S26) | se reutiliza tal cual; solo es alcanzable en un fixture MAP_VALID (VERIFIED exige `Authority` pass) | ninguno |
| **`supervision-checks.md`** (sellado fuera del repositorio; SHA-256 `729fc3ef…`, 22 954 bytes, intacto hoy) | vigente para FX-U1 | cita el blob del contrato de T1 (`628d89a`), `cabed54` y el `effective_sha` de FX-U1: no sirve para FX-U2 | volver a sellarlo con los SHA de B1 (precedente: el resellado de N8) |
| **`negatives.md`** (sellado; SHA-256 `25a33a48…`, 16 424 bytes, intacto hoy) | N4, N5, N7a y N7b aceptados; N8 resellado; N9 y N10 registrados (§67, punto 5), pendientes de resellado | las mutaciones no citan SHA del fixture: se reutilizan. N9 (variante T16) puede quedar sin precondición si un solo titular abre FX-U2 y ejecuta FX-02 | resellar con N9 y N10; N9 no pasa a NOT_APPLICABLE sin demostrar que falta su precondición (decisiones §63, punto 4) |
| **FX-06 / C-39** | UNVERIFIED — staging completo | su staging fija `cabed54`, blobs «en cabed547» y la familia de objetos de FX-U1; el presupuesto de D.8 no cambia | regenerar los insumos fijados sobre B1; las autopruebas (64/64, 9/9) no cambian |
| **Staging de FX-02** (`frontiers.json`, plantilla de O4, designación de A2, tarjeta, contratos, borradores, `gates.env`) | preparado, no acreditado | cita SHA de FX-U1 (`cabed54`, `fbe25347`, `628d89af`, `d30fb6a9`, `1746b404`, `95bdc29d`) | regenerar con los SHA de B1; U-07 (TRX) y U-08 (`.gitignore`) se vuelven a aplicar en FX-U2 por orden (A5-1 conserva los `FIXTURE_LOCAL` de D.1); U-17 (b) nombra el contrato de T1 de FX-U1 y pide disposición para el de FX-U2 |
| **Referencia AUTHOR** (CD-19 / F-1..F-6) | custodiada para `d30fb6a9` | no reutilizable: el T1 de FX-U2 tendrá su propio autor | observación nueva, con D-6 |
| **Sondas A4-1 y A2-P2 (sonda 1)** | NOT_DEMONSTRATED | intactas: son veredictos de capacidad ligados a la identidad; la referencia MAP_INVALID del kit de A4-1 era correcta para r1 | ninguna; el clon `D:\r62-fixture\A2` se conserva |
| **FX-01, FX-05, C-20b/C-20c** | PASS | intactos | ninguna |

Ningún resultado acreditado se invalida. Lo que cambia es la **reutilización**: la cadena de FX-U1, U-07/U-08, O4, el clon A2, la referencia AUTHOR,
el staging y `supervision-checks.md` no sirven como insumos de FX-02 en B1 (`I-62-F6/MAP-INVALID/effects.md`, filas 10-26).

## 4. (d) Presupuesto físico completo

Matriz completa en `out/budget-matrix.json` (22 filas, cada una con tope, consumo, disponible y fuente). Resumen:

| Contador | Tope | Consumido | Disponible | Fuente |
|---|---|---|---|---|
| Principal A (ronda A) | 1 + 1 reapertura = 2 | 2 (A, R) | 0 | V14 L2437; decisiones §63, punto 2 |
| A4-6, solo A2 de FX-U1 | +1 | 0 | 1, no fungible | A-4 L350-L361; `A4-PRINCIPAL-A2-CONSUMO` = A sin uso |
| **A5-2, Principal FX-U2 (propuesta)** | 1, sin reapertura | 0 | 1, tras acuerdo, línea del Owner y P-07 | A-5 §3.3 |
| Total de sesiones de Principal, ronda A | ≤ 2 → 3 (A2) → 4 (FX-U2) | 2 | plazas 3 y 4, no fungibles | V14 L2523; A-4 L355 |
| Codex base / pool / tope (FX-02) | 11 / 4 (≤ 2 por fase) / 15 | 0 / 0 / 0 | 11 / 4 / 15 | V14 L2442; u14 §5 |
| Worker / Reviewer | 2 / 2 | 0 / 0 | 2 / 2 | V14 L2444-L2445 |
| Subagentes Claude / Claude CLI (ronda A) | ≤ 4 / 0 | 0 / 0 | 4 / 0 | V14 L2523 |
| Sondas previas (`codex-cli`) | 2 | 2 + 2 por encima (históricas) | 0 | V14 L2443; A-2, A2-P2 regla 1 |
| Bloque A2-P2, par `3553cd6e…` | 2 | 2 | 0 | A-2 L222-L223 |
| Invocación de A4-1 | 1 | 1 (NOT_DEMONSTRATED) | 0 | A-4 L203 |
| Bloque A2-P2, par `d13914ce…` / `26.1007.2314.0` | 2 | 1 | 1 (sonda 2 **HELD**) | decisiones §69-§71 |
| A2-P1 (una por escenario en F6) | 1 por escenario | FX-04a: 1 (B3) | FX-02: 1, solo para la fila Principal A; no para la fila de A5-2 | A-2 L148-L159 |
| FX-04a Principal B / N11 | 3 / 1 | 3 / 0 | 0 / 0 (N11 NOT_APPLICABLE, no fungible) | V14 L2500-L2501; decisiones §55, §58 |
| FX-04b / Topología B / D.8 | 6 / 12 Codex, ≤ 4 Claude CLI, 2 Principal / 2 Principal, 4 Architect | 0 / 1 sonda de escritura (OD-4) / 0 | sin cambio | V14 L2503-L2526; D.8 |

A5-2 no transfiere el cupo de A4-6, no reinicia ni descuenta ningún contador y deja los presupuestos de FX-02 en continuidad (0 consumidos).

## 5. (e) Decisiones OWNER-RESERVED

| Decisión | Para qué | Fuente | Estado |
|---|---|---|---|
| Crear un repositorio público nuevo para el remoto con CI de B1 | aplicación de A5-1 | OD-7, V14 L975; decisiones §46 (OD-7 = A, remoto del fixture actual) y §48 (público por defecto; privado exige autorización aparte) | línea nueva `A5-FIXTURE-B1-REPO` (§7) |
| Consumir una sesión de Principal por encima de los topes de D.3 que cubre OD-5 | aplicación de A5-2 | OD-5, V14 L974; decisiones §46 («consumo dentro de los topes de D.3»); A-2 L159; precedentes `B3-CONSUMO` (§58) y `A4-PRINCIPAL-A2-CONSUMO` (§64) | línea nueva `A5-PRINCIPAL-FXU2-CONSUMO` (§7) |
| Abrir la sesión del sistema bajo prueba | aplicación de A5-2 | V14 L2437 («abierta por el Owner») | acto del Owner tras P-07 |
| Aceptar la huella de `codex-cli` (OD-2) | toda invocación posterior | V14 L971; decisiones §69 (OD-2 = A sobre `F909F480…`) | vigente; cualquier cambio → P-01 y OD-2 nueva |
| Consumo de `claude-cli` como Architect de FX-02 y su medición | fuera de A-5 | A-4 §7 (`A4-CLAUDE-FX02-CONSUMO` = A); decisiones §66, punto 6 (`CLAUDE-CLI-MEDICION-FX02`, no pedida) | sin cambio |
| **Posibles en el acuerdo** | cobertura de D.5 / OV (Q-A5-01) y alcance de OD-7 (Q-A5-09) | V14 §18 (matriz OV, L952-L976); LIFECYCLE §6 («una materia OWNER-RESERVED exige Owner») | a identificar por la revisión; si se confirman, BLOCKED — OWNER DECISION |

El acuerdo de A-5 no es OWNER-RESERVED por sí mismo: no cambia el alcance, los no-objetivos, OD-1..OD-7 ni la matriz OV.

## 6. (f) Guardas y contratos de producción

**Guardas** (`a5-guards.py`; `a5-selftest.json`):

- **Autoprueba:** controles G1, G2, G3, G4, G5 y G5b en PASS; **48/48 mutantes muertos** (G1 13/13, G2 8/8, G3 7/7, G4 3/3, G5 15/15, G5b 2/2), cada
  uno con un hallazgo nuevo del motivo esperado.
- **`preview`** (`a5-guards-result-preview.json`), sobre un commit candidato **hipotético** construido solo en un clon temporal compartido
  (`make_candidate_commit.py`; base `efcfcbc5`): G1 PASS (con la privacidad de todo el paquete), G2 PASS, G3 PASS, G4 PASS (`clause_map.py check
  bb0d5522 <candidato>` = EQUAL), G5 PASS, G5b PASS. En la HEAD `efcfcbc5` sin commit candidato (`out/a5-guards-result-preview-head.json`):
  G2 NOT_RUN y el resto PASS.

**Contratos de producción** (`candidate-commit.json`): el commit hipotético añade solo `docs/initiatives/I-62-A-5.md` y archivos de
`docs/automation/evidence/I-62-A5/` (todos con estado A). `git diff --name-status efcfcbc5 <candidato> -- <producción>` es **vacío** para `src`,
`tests`, `eng`, `tools`, `deploy`, `assets`, `docs/adr`, `docs/automation/agent-execution`, AUTOMATION_PLAN, WORKFLOW, LIFECYCLE, FOUNDATIONS,
`AGENTS.md`, `CLAUDE.md`, los archivos de compilación, V14, el Freeze, A-1..A-4, PROMPT_TEMPLATES, la evidencia de F4 y `I-62-F6/MAP-INVALID/`.
Aplicada sobre V14 + A-2 + A-4, A-5 es un añadido puro (G5).

## 7. (g) Líneas exactas del Owner (de A-5 §7; solo tienen efecto tras A-5 AGREED y la disposición que nombre cada aplicación)

```text
A5-FIXTURE-B1-REPO = A (un repositorio público nuevo de GitHub, rackcad-i62-fixture-b1, en la cuenta del Owner, con GitHub Actions y la CI del fixture de OD-7 (fixture-build y fixture-tests, contents: read, sin secretos ni despliegues), como remoto con CI del fixture B1 de A5-1; solo tras la auditoría pública de todos sus objetos; lo crea el Owner, o la supervisión con gh bajo esta autorización; identidad sintética y sin datos reales; el repositorio del fixture actual y sus refs no se tocan; no autoriza ninguna sesión ni invocación; solo con A-5 AGREED, A5-1 incluida, y la disposición del Coordinator que nombre la construcción)
A5-PRINCIPAL-FXU2-CONSUMO = A (una sesión de Principal, claude-desktop-session, abierta por el Owner para arrancar la unidad FX-U2 del fixture B1 y ejecutar en ella FX-02 en la Topología A, en la fila propia de A5-2 y por encima del límite de la ronda A de D.3 que cubre OD-5; solo con A-5 AGREED, A5-2 incluida, y la disposición del Coordinator que nombre la apertura; sin reapertura ni ninguna otra sesión; no usa el cupo de A4-6; no acepta su resultado ni amplía OD-5)
```

## 8. (h) ¿Hay una ruta viable para C-24 tras corregir el fixture?

Hechos, sin recomendación de rebajar criterios:

- **Lo que A-5 resuelve:** con A5-1, un fixture B1 pasa MV-1..MV-7 y `Authority` deja de fallar por el mapa; con A5-2, FX-U2 tiene una sesión de
  Principal. Son condiciones necesarias de FX-02 PASS (V14 L2544, oráculo D-0: VERIFIED), no suficientes.
- **Lo que sigue bloqueando:** el Controller de FX-02 sigue **NOT_DEMONSTRATED** con las dos identidades medidas: A4-1 (`3553cd6e…/73890CA3…`, `cmd.exe`)
  y la sonda 1 del bloque A2-P2 (`d13914ce…/F909F480…`, `pwsh` nativo). F6-OBS-03 está resuelto. Sin celda elegible, P-10 impide invocar el
  Controller, y sin verificación no hay VERIFIED.
- **Causas medidas** (`controller-failure-analysis.md`): ≈ 88 % de las claves fallidas son omisiones voluntarias de la celda (59/68 y 83/94; 11 y 4
  comandos, con el 80-83 % del tope sin usar); errores de lectura de Git (3 y 6); fidelidad UTF-8 del transporte (4 y 3), que otra medición no
  resuelve sin cambiar la receta o el texto del rol; `ls-remote` bloqueado por el sandbox (2 y 2), que VERIFY no necesita porque compara `Remote`
  con el registro; y la agregación de D-4 en una sola medición.
- **Presupuesto de medición que queda:** solo la sonda 2 del bloque A2-P2 del par vigente, **HELD**; cambiar su destino exige una disposición expresa
  del Coordinator (decisiones §71, punto 1). Una sola sonda no puede demostrar PLAN y VERIFY por separado, y caduca con cualquier actualización
  nueva. A4-1 está agotada. No hay otra alternativa autorizada. Una actualización real del runtime abriría otro bloque A2-P2 (con OD-2 y
  `BLOQUE-CODEX-CONSUMO`).
- **Otras dependencias:** el remoto con CI de B1 (OD-7; lección R1/R2: el primer push de un repositorio vacío no dispara la corrida); la revisión
  del Architect de FX-02 es una obligación posterior al PASS (D-0) y su binding por `claude-cli` sigue sin elegibilidad demostrada (operaciones 5, 7,
  8 y 9; `CLAUDE-CLI-MEDICION-FX02` no pedida).
- **Conclusión factual:** hoy no existe una ruta demostrada hacia C-24 PASS. B1 elimina el impedimento del mapa, que es independiente del Controller;
  una ruta viable exige además que una medición autorizada demuestre las operaciones obligatorias de PLAN y VERIFY del Controller con la identidad
  vigente (o con un runtime actualizado), o una enmienda futura expresamente justificada sobre la ruta del Controller, que A-5 no propone. Por eso
  el anexo de A-5 recomienda aplazar la apertura de la sesión de FX-U2 hasta que exista esa ruta.

## 9. Archivos del preflight

| Archivo | Contenido |
|---|---|
| `build_candidate_a5.py` | constructor determinista del método de A5-1 (solo repositorios temporales) |
| `a5_conformance.py` | conformidad estructural K-1..K-11 con el texto de A5-1 |
| `neg_controls_a5.py` | 11 controles negativos de MV sobre una copia |
| `preflight_run.py` | ejecución de (a) y (b); salidas saneadas |
| `make_candidate_commit.py` | commit candidato hipotético en un clon temporal compartido (f) |
| `a5-guards.py`, `a5-selftest.json`, `a5-guards-result-preview.json` | guardas, autoprueba y resultado `preview` |
| `out/*.json`, `out/*.txt` | salidas exactas |
