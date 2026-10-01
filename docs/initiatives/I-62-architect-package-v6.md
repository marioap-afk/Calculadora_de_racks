# I-62 — Paquete de revisión (Proposal V6)

```text
PROPOSAL V6 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED (V1 a V5: CHANGES REQUIRED; registros I-62-coordinator-review-v1.md … -v5.md)
Architect      = NOT REVIEWED (no se invoca todavía, C62-F0-26; no se simula)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V6 §11.4, dos alternativas delimitadas); sin decisión, no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v6.md            blob 19672958ec095ba9f88e103240d34c62c76ee850
  docs/initiatives/I-62-coordinator-review-v5.md  blob 4c51a94c4790629f91006db642915e1ae11ce25b
Versiones anteriores revisadas:
  V1: commit ea0555913202efc96e1007f021942579b989b43d, blob 31e2f4afa895dc03f7621504e150de0539a2302e
  V2: commit a1f5e0035f0e109d02a336c0a01917e53a53b88a, blob 47a643ddf1f22226072ede67a862133f77441e2e
  V3: commit 486e45e797642ad589980584e16b3e50e53c1fb9, blob 36b13443b3d8cdc21fe731b4a5ab263cc9fd7fb9
  V4: commit 2960b28614da14ad6ebe9eeaa9631fd787041ac3, blob dd9e478d737c76139b0eecc3c9e30d9beae8ccd6
  V5: commit acd88eafa4847e4740fdf9e7899a6988508ec24a, blob b673b134c3f265b891e10807b6f6fd6cbe852773
Discovery base: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65…, commit 2b7976b0), con §21 añadida en ea055591
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (solución de R62-V1-09, cerrada y preservada). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v6.md` = `19672958…`. Si no coincide, revisa la versión designada o rechaza la discordancia; nunca
> aprueba el último archivo encontrado.

> **Qué es.** Una corrección **acotada** de V5 (C62-F0-26): solo lo necesario para cerrar R62-V5-01 y alinear la traza, la prueba y este paquete. La sesión
> autora no emite veredictos. **Las trazas de V6 (E.3.0 y E.6) son análisis del diseño, no ensayos ejecutados.**

## 1. Veredicto que se solicita (LIFECYCLE §5)

```text
Resultado: AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION
           AGREED solo con cero REQUIRED abiertos sobre la versión exacta.
Hallazgos: REQUIRED | OPTIONAL (las notas no sustituyen estas clases); ID, sección, evidencia, cambio exigido.
Modo:      SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN; si revisor y autor son la misma persona.
Identidad: commit, ruta y blob revisados.
```

Solo quien emitió un REQUIRED puede rebajarlo.

## 2. Lectura

1. [Proposal V6](I-62-proposal-v6.md), centrada en:
   - **Anexo E.3.0**: punto de entrada, cadena normativa, `Evaluate`, fallo cerrado y obligaciones por actor;
   - E.1, E.4 (MV-6, MV-7), E.5 y E.6 (descubrimiento en C-20c);
   - §3, §14, §18 (OD-1), Anexo A, Anexo C (C-20a, C-20c) y G.1.
2. El diff contra V5 (`git diff acd88eaf <commit> -- docs/initiatives/I-62-proposal-v6.md` frente a `I-62-proposal-v5.md`) muestra que el resto no cambia.
3. Registros del Coordinator: [V1](I-62-coordinator-review-v1.md) … [V5](I-62-coordinator-review-v5.md).
4. Fuentes integradas que sostienen la cadena, en `819955d6`:
   - WORKFLOW §10 (filas de «transición» y de «ejecución delegada»), §11.3 (último párrafo) y §4 (rebase al abrir);
   - el mismo §10 en el `AuthorityRevision` de G3 (`7b8662c5`);
   - AUTOMATION_PLAN 16.3, 16.7 y 16.9;
   - el contrato real de I-61 G3.

## 3. Delta V5 → V6

| Área | V5 | V6 |
|---|---|---|
| Descubrimiento del resolver | «el lector llega a §16.13 porque su regla lee §16 en `MainSha`». Falso para el contrato real de I-61 G3, que lee §16 en `AuthorityRevision` (`UNIT_CHANGE`) | **Punto de entrada nuevo, declarado como delta de I-62 bajo OD-1** (`/v1` no aporta ninguno con esa propiedad). Es una sección `##` nueva de WORKFLOW, «Coexistencia de protocolos de ejecución delegada (I61/I62)», con texto normativo propuesto (E.3.0). Su lectura en `main` actual, con independencia de todo contrato, la garantizan dos reglas ya integradas: **WORKFLOW §10**, idéntico en el AR de G3 y en `main`, que hace de WORKFLOW el dueño de «transición» y deja la ejecución delegada «dentro de las reglas de los dueños anteriores»; y **el gobierno de WORKFLOW en `main` actual** (§11.3, V2 activo por `8a021fb6`, rebase al abrir). En toda evaluación válida, `main` actual = `MainSha_eval` (A6, `Remote`, 16.7) |
| Cadena | implícita | explícita (E.3.0): regla vigente → punto de entrada → `Classify` → mapa → `Authority` `/v1`, con la revisión de lectura de cada eslabón; vale para G3 (§16 `UNIT_CHANGE`) y para una unidad de producto (§16 `EXTERNAL`) |
| Procedimiento | `Resolve(K)`, que en el paso 0 buscaba punteros | **`Evaluate(K, MainSha_eval)`** (E1-E5): WORKFLOW → sección de entrada → §16.13 → `Resolve` (pasos 1-5) → registro de descubrimiento. `Resolve` solo se invoca desde `Evaluate` |
| Fallo cerrado | MAP_INVALID; §16.13 o punteros ausentes | más ENTRY_INVALID (sección de entrada ausente, repetida o ambigua; §16.13 nombrada inexistente o repetida); ACTIVATION_INVALID (sección presente sin trailer efectivo); verificación sin registro de descubrimiento → no aceptada por el Coordinator (16.1) |
| Controller | lo descubría por §16 | la sesión le pasa la **entrada de compatibilidad** como entrada canónica del prompt (no en el contrato ni en la delegación); el Controller ejecuta `Evaluate` y lo registra en `Authority.Evidence`. Es el único cambio operativo para las unidades I61 y lo ejecutan los evaluadores |
| Punteros de §16/§16.3 | vía de descubrimiento | vía **redundante**; la cadena no depende de ellos (N-q lo prueba) |
| Mapa | ENTRY = {§16.13} | ENTRY = {§16.13, sección de entrada de WORKFLOW}; MV-7 exige la sección de entrada una sola vez, nombrando §16.13; la lectura compuesta incluye esa sección (ENTRY) en `MainSha_eval` |
| C-20c | resolución con el contrato real | **descubrimiento + resolución**: cada caso ejecuta `Evaluate` desde E1 y el arnés obtiene §16.13 y el mapa del texto del punto de entrada. Traza E1-E5 para G3 byte a byte; mismo descubrimiento para el contrato de I-64; negativos N-m..N-s del descubrimiento |
| OD-1 y ADR | §16, §16.13 y la fila de WORKFLOW §10 | + la sección de entrada de WORKFLOW |

**Sin cambio:** la lectura compuesta de documento completo (solo se añade la fila ENTRY), el Modelo A y `state/v2`, FX-04a/FX-04b y todos los cierres de
C62-F0-21, C62-F0-23 y C62-F0-25.

## 4. Disposición de R62-V5-01, por punto (la sesión declara; el cierre lo decide el emisor)

| Punto exigido | Dónde lo atiende V6 | Cierre verificable que se ofrece (análisis) |
|---|---|---|
| 1. partir del contrato real de G3, byte a byte | E.6, C-20c-1 | blob `9b5ef6df…` sin cambios |
| 2. sin campos ni `Authorities` nuevos | E.3.0 (la entrada de compatibilidad va al prompt, no al contrato) | C-20c-1 y C-20c-2 |
| 3. sin cambios en `/v1` | E.3.0; C-19 | — |
| 4. la autoridad leída necesariamente en `MainSha` actual | **WORKFLOW** (gobierno de proceso; §10 dueño de «transición»; §11.3; V2 activo); no hay ninguna en `/v1` | E.3.0 (hecho de partida y eslabón 1) |
| 5. la cadena exacta | tabla de cinco eslabones y `Evaluate` E1-E5 | traza de descubrimiento de C-20c-1 (b) |
| 6. también con §16 `EXTERNAL` | misma cadena; los punteros son redundantes | C-20c-2; N-q |
| 7. sin que el autor conozca I-62 | lo ejecutan los evaluadores | C-20c-2 (citas idénticas) |
| 8. delta declarado bajo OD-1 | §3, §14, §18 (OD-1), Anexo A, G.1 | — |
| 9. fallo cerrado del punto de entrada | tabla de E.3.0 | N-m, N-n, N-o, N-p, N-r, N-s |
| 10. C-20c prueba el descubrimiento | arnés sin `Resolve` directo; registro E5 en el esperado | Anexo C, C-20c; E.6 |

## 5. Riesgos residuales que se piden juzgar

- El punto de entrada vive en WORKFLOW, el documento de proceso de mayor alcance. Un cambio posterior a su encabezado o a su texto afecta a toda unidad, y
  E.7 lo trata como cambio normativo con su propia compatibilidad.
- La entrada de compatibilidad al Controller es un cambio operativo pequeño para las unidades I61: una entrada más en el prompt de verificación, sin tocar el
  contrato. Si se considera excesivo, la alternativa es que solo el Coordinator ejecute `Evaluate` y acepte o rechace el VERIFIED. Eso pierde la verificación
  independiente de `Authority`.
- La tesis «toda unidad se rige por el WORKFLOW de `main` actual» es explícita para las unidades V1 (§11.3) y se apoya en la activación de V2 para las demás.
  Si el Coordinator la considera insuficiente, la propia sección de entrada lo declara para toda unidad, como parte del delta de OD-1.

Los 12 retos del mandato conservan el análisis del [paquete V5](I-62-architect-package-v5.md) §5. Ninguno cambia con esta corrección, salvo el reto 10 (Level
B sin necesidad): `Evaluate` es un procedimiento documentado más, no tooling.

## 6. Decisiones del Owner y su frontera real

| Id | Decisión | Bloquea |
|---|---|---|
| OD-6 | predicado de independencia de LIFECYCLE (dos alternativas; recomendación del Coordinator: la 1, no decisión) | acuerdo y Freeze |
| OD-1 | ADR sucesor (incluye §16.13 y **la sección de entrada de WORKFLOW**) | READY-03 y vigencia |
| OD-2 | línea base de huella por adapter | invocaciones afectadas (A, B, FX-04b) |
| OD-3 | autenticar Claude CLI | B |
| OD-4 | sandbox de Codex para escritura | B y FX-04b (no FX-04a) |
| OD-5 | permiso de ensayo con la semántica única de §15 | F6 |
| OD-7 | remoto del fixture con CI | cierre de F6 (por FX-02) y FX-04b (no FX-04a) |

## 7. Preguntas para la revisión

1. ¿WORKFLOW, por su propiedad de «transición» (§10) y por regir toda sesión en `main` actual, es el lugar correcto del punto de entrada, frente a AGENTS o
   CLAUDE.md?
2. ¿Es aceptable la entrada de compatibilidad al Controller como único cambio operativo para las unidades I61, o debe quedar solo en el Coordinator?
3. ¿El fallo cerrado ENTRY_INVALID y ACTIVATION_INVALID cubre todos los estados ambiguos del punto de entrada?

## 8. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no invoca al Architect, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni
sesiones nuevas.
