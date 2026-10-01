# I-62 — Paquete de revisión (Proposal V5)

```text
PROPOSAL V5 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED (V1, V2, V3 y V4: CHANGES REQUIRED; registros I-62-coordinator-review-v1.md … -v4.md)
Architect      = NOT REVIEWED (no se invoca todavía, C62-F0-24; no se simula)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V5 §11.4, dos alternativas delimitadas); sin decisión, no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v5.md            blob b673b134c3f265b891e10807b6f6fd6cbe852773
  docs/initiatives/I-62-coordinator-review-v4.md  blob fb21c49e80377de6102e26a4ccc99785676c6526
Versiones anteriores revisadas:
  V1: commit ea0555913202efc96e1007f021942579b989b43d, blob 31e2f4afa895dc03f7621504e150de0539a2302e
  V2: commit a1f5e0035f0e109d02a336c0a01917e53a53b88a, blob 47a643ddf1f22226072ede67a862133f77441e2e
  V3: commit 486e45e797642ad589980584e16b3e50e53c1fb9, blob 36b13443b3d8cdc21fe731b4a5ab263cc9fd7fb9
  V4: commit 2960b28614da14ad6ebe9eeaa9631fd787041ac3, blob dd9e478d737c76139b0eecc3c9e30d9beae8ccd6
Discovery base: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65…, commit 2b7976b0), con §21 añadida en ea055591
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (solución de R62-V1-09, cerrada y preservada). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v5.md` = `b673b134…`. Si no coincide, revisa la versión designada o rechaza la discordancia; nunca
> aprueba el último archivo encontrado.

> **Qué es.** La versión completa, el delta V4→V5 y la disposición por ID. La sesión autora no emite veredictos ni declara consenso o independencia. **Las
> trazas de V5 (§8.6 y anexos E.6, F y G.2) son análisis del diseño, no ensayos ejecutados.**

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

1. [Proposal V5](I-62-proposal-v5.md), completa. En particular:
   - §8.6: arranque BOOTSTRAP → G0 → QU, Modelo A;
   - §14: clasificación y resolución transparente;
   - Anexo B.8: `state/v2`, con `protocol.basis`, `g0_acceptance` y `principal.acceptance`;
   - Anexo E.2-E.3: clasificador con PENDING_G0, resolver sin requisitos sobre el contrato y lectura compuesta;
   - Anexo E.6: el contrato real sin cambios.
2. Registros de revisión del Coordinator: [V1](I-62-coordinator-review-v1.md), [V2](I-62-coordinator-review-v2.md), [V3](I-62-coordinator-review-v3.md) y
   [V4](I-62-coordinator-review-v4.md).
3. [Discovery R1](I-62-discovery.md) §§1-3, 9-13 y 21.
4. [Decisiones](../automation/decisions/I-62.md) §§7, 9-14.
5. Fuentes integradas:
   - AUTOMATION_PLAN §§8 y 16 (16.3, 16.4, 16.5, 16.6, 16.7 y 16.8);
   - `gate-contract.schema.json` `/v1` y el contrato real de I-61 G3;
   - WORKFLOW §§3 y 11.1-11.3; LIFECYCLE §§2-9 (G0);
   - ADR-0046 y el Freeze de I-61 ([Proposal V9](I-61-proposal-v9.md)).

## 3. Delta V4 → V5

| Área | V4 | V5 |
|---|---|---|
| Transparencia para los contratos I61 (R62-V4-01) | el contrato I61 debía citar §16.13 y el mapa; la cita de documento completo de un archivo modificado fallaba (S-12) y obligaba a reemitir citando por sección (K61 → K61') | **ningún requisito sobre el contrato**: sin campos, sin citas nuevas, sin reemisión; el resolver lo aplica quien lee el contrato, que llega a §16.13 por la regla de I-61 (§16 en `MainSha` y su puntero). **Lectura compuesta** de «documento completo»: preámbulo y cada `##`, con la regla de la cita individual; las secciones solo I62 se omiten y las posteriores a EFF de otras iniciativas se leen en `MainSha`. **Compromiso declarado:** un `##` modificado por I-62 se lee entero en `EFF^1`, y un archivo no Markdown modificado, también entero. `MainSha_eval` para la reverificación de 16.7 sin conflictos (el contrato no cambia). Coincidencia de secciones posteriores a EFF (R61 c4). Obligación de no renombrar ni borrar encabezados existentes (E.7). **C-20c-1:** el contrato real de I-61 G3 **byte a byte**, antes de EFF (regresión nula) y en la reverificación de 16.7 tras EFF. **C-20c-2:** el mismo contrato para I-64 sin I-62, con citas idénticas. Negativos N-a..N-m |
| Orden de arranque (R62-V4-02) | `protocol.g0_decision` obligatorio en BOOTSTRAP (decisión aún inexistente); binding del Principal ACCEPTED en BOOTSTRAP sin autoridad previa | **Modelo A** (§8.6), con la secuencia observación → propuesta → BOOTSTRAP (evidencia + aceptaciones PENDING, todo custodiado en el mismo commit) → decisión de G0 con dos marcadores explícitos → QU con las transiciones y sus `StateRef` en el mismo commit. Lo apoyan cuatro piezas: `protocol.basis` (`claim_id`; `claim_commit` o `null` sin autorreferencia; `claim_parent_sha`; `claim_parent_contains_effective`); `protocol.g0_acceptance` y `principal.acceptance` con PENDING explícito y transición única; la transición única de `Acceptance` en `binding/v1` (B.2, B.5); y el clasificador con PENDING_G0, que es fallo cerrado. Invariantes nuevas o ajustadas: I-S01, I-S14, I-S15, I-S16, I-P06, I-P07 e I-P09. T0 en §9.2. Modelo B descartado con su razón |

Se preservan los cierres de V1 (R62-V1-09 y O62-V1-01) y todos los de C62-F0-21 y C62-F0-23.

## 4. Disposición por ID (la sesión declara; el cierre lo decide el emisor)

| ID | Punto exigido | Dónde lo atiende V5 | Cierre verificable que se ofrece (análisis) |
|---|---|---|---|
| R62-V4-01 (1) sin cambios en `/v1` | intacto (C-19) | E.3; G.1 | C-20c-1 valida el contrato real contra el esquema real |
| R62-V4-01 (2) sin campos nuevos obligatorios | ninguno | E.3 (entradas) | C-20c-1 y C-20c-2 |
| R62-V4-01 (3) sin reemisión para acogerse | el resolver lo aplica quien lee; la reemisión de 16.7 antes de escribir es la de siempre ante cualquier avance de `main` | E.3 («cómo lo descubre», punto 3) | C-20c-1 (b) sin reemisión |
| R62-V4-01 (4) documento completo sin reescribir | lectura compuesta | E.3 (lectura compuesta) | C-20c-2 filas 10 y 11 |
| R62-V4-01 (5) `MainSha` y `AuthorityRevision` | sin cambio; `MainSha_eval` = el de 16.7 | E.3 («significado `/v1` conservado») | C-20c-2: `MainSha` = M2 cumple A6 y `Remote` |
| R62-V4-01 (6) el autor no conoce I-62 | descubrimiento por el lector | §14; E.3 | C-20c-2 (citas idénticas byte a byte) |
| R62-V4-01 (7) evolución normal | `MainSha_eval` para lo no modificado y para lo posterior a EFF | E.3 (R61 c3-c4) | X1 visto; X3 incluido en la lectura compuesta |
| R62-V4-01 (8) fallo cerrado | MAP_INVALID, PENDING_G0, UNKNOWN, S-12 por ambigüedad | E.2-E.4 | N-a..N-m |
| R62-V4-01 (9) C-20c desde un contrato real | contrato de I-61 G3 byte a byte | E.6; Anexo C | C-20c-1 (a) y (b) |
| R62-V4-01 (compromiso) | declarado de forma explícita | §14; E.3 | — |
| R62-V4-02 modelo | **A**, con la razón del descarte de B | §8.6 | — |
| R62-V4-02 BOOTSTRAP sin aceptación inexistente | evidencia en `protocol.basis`; `g0_acceptance` PENDING | §8.6; B.8.1 | I-S01, I-S16 |
| R62-V4-02 PENDING explícito y decisión posterior | QU con `StateRef` a la entrada de decisiones del mismo commit | §8.6 (paso 5); T0 | C-15 (aceptación, rechazo, G0 sin marcadores) |
| R62-V4-02 campos de una transición e inmutables | `g0_acceptance` y `principal.acceptance`, una vez; `set`, `effective_sha` y `basis`, inmutables | §8.6 (transiciones); B.8.4 | I-P06, I-P09; C-18 |
| R62-V4-02 clasificador antes y después | PENDING_G0 / I62 / UNKNOWN | E.2 (tabla) | N-l |
| R62-V4-02 binding y preflight del Principal | observación (TRANSIENT) → propuesta PENDING → custodia en BOOTSTRAP → aceptación del Coordinator → versión aceptada + QU | §8.6; B.2; B.5 | I-S14, I-P07; C-15 |
| R62-V4-02 sin SHA futuro, `StateRef` inexistente ni aceptación inferida | referencias a blobs del mismo commit; `claim_commit` = `null` si es autorreferencia; sin marcadores no hay transición | §8.6 (reglas); B.8.1 | I-S13, I-S16 |

## 5. Los 12 retos del mandato («ARCHITECT»)

| # | Reto | Dónde | Riesgo residual que se pide juzgar |
|---|---|---|---|
| 1 | Proveedor en los roles | §2; C-01, C-02 | la asignación por topología del Anexo D es binding de prueba, no semántica |
| 2 | Supuestos no verificables del modelo | §§4.2, 5, 10 | que RUNTIME_OBSERVED baste como mínimo |
| 3 | Autointrospección falsa | §10; B.4 `Assurance`; I-02 candidata | la configuración del cliente frente al backend |
| 4 | Memoria privada oculta | §8; D.6; QH sin artefactos transitorios; oráculo fuera del host; entradas automáticas enumeradas | un registro sin lecturas deja FX-04a en UNVERIFIED |
| 5 | Router complejo | §5 (siete pasos sobre el routing existente) | el número de contratos (B.1), con `clause-map/v1` |
| 6 | Requisitos excesivos entre proveedores | §11.3 (Proveedor casi siempre PREFERRED) | el valor para «Coordinator/Worker en la misma sesión» |
| 7 | Seguridad y autenticación | §6; B.4; C-10 con secretos ficticios | el uso futuro de metadatos de sesiones (SP-3) |
| 8 | Custodia ambigua | §8; §8.6; B.8 | dentro de la ventana, la delegación abierta solo existe en el diario; hasta G0 la unidad I62 no puede delegar |
| 9 | Recuperación que sobrescriba trabajo | §9 (T0-T18); B.8.5-B.8.6; F.2, F.5 | no admitir ISOLATION_ACCREDITED puede bloquear recuperaciones legítimas; T12a depende del diario en el mismo host |
| 10 | Level B sin necesidad | §17; C-15..C-17, C-20b, C-20c en F4 | si el validador semántico y el resolver son tooling de hecho |
| 11 | Esquemas de proveedor | B.2-B.3 | el coste de un esquema de hechos por adapter |
| 12 | Reanudar en otra máquina | §9.1, T14, B.8.5, F.2; `HostRef` | sin terminación acreditada en otra máquina, la reanudación queda detenida por diseño; desde un QH basta Git |

**Riesgos propios de la resolución legacy** (E.3-E.7):
- **lectura compuesta:** el texto que gobierna un documento completo mezcla secciones de `EFF^1` y de `MainSha`. Una referencia cruzada entre ellas puede
  apuntar a texto de otra revisión. La mitigación es no renombrar ni borrar encabezados (E.7) y registrar la revisión de cada unidad;
- dentro de un `##` modificado, las subsecciones no tocadas dejan de evolucionar para las unidades I61. Es el compromiso declarado, con riesgo conservador;
- §16.13 se lee en `MainSha` como gobernanza: un cambio posterior afecta a toda unidad (precedente: WORKFLOW §11.3);
- la CI hace checkout superficial: la parte con historia es MC (C-20b, C-20c).

**Riesgos del arranque Modelo A:** la unidad I62 no puede delegar hasta el QU de G0. Es intencional (fallo cerrado) y coincide con la práctica actual: nada se
delega antes de G0.

## 6. Decisiones del Owner y su frontera real

| Id | Decisión | Bloquea |
|---|---|---|
| OD-6 | predicado de independencia de LIFECYCLE (dos alternativas; recomendación del Coordinator: la 1, no decisión) | acuerdo y Freeze |
| OD-1 | ADR sucesor (incluye §16.13) | READY-03 y vigencia |
| OD-2 | línea base de huella por adapter | invocaciones afectadas (A, B, FX-04b) |
| OD-3 | autenticar Claude CLI | B |
| OD-4 | sandbox de Codex para escritura | B y FX-04b (**no** FX-04a) |
| OD-5 | permiso de ensayo con la semántica única de §15 | F6 |
| OD-7 | remoto del fixture con CI | **cierre de F6** (por FX-02) y FX-04b (**no** FX-04a) |

## 7. Preguntas para la revisión

1. ¿La lectura compuesta por `##` de «documento completo» es la granularidad adecuada, frente a leer el documento entero en `EFF^1` (descartado) o bajar a
   `###`?
2. ¿Basta el descubrimiento por el lector (§16 en `MainSha` → puntero → §16.13) para que la compatibilidad sea transparente sin tocar el contrato?
3. ¿Es correcto usar el `MainSha` nuevo del `RebaseMap` como `MainSha_eval` en la reverificación de 16.7, igual que ya hace I-61 en sus comprobaciones?
4. ¿Los dos marcadores explícitos (`I62-CLASSIFICATION`, `I62-PRINCIPAL-BINDING`) en la decisión de G0 son la forma correcta de evitar aceptaciones inferidas?
5. ¿La transición única de `Acceptance` dentro del mismo `BindingId` (B.2) es preferible a un artefacto de aceptación separado?

## 8. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no invoca al Architect, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni
sesiones nuevas.
