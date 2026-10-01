# I-62 — Paquete de revisión (Proposal V4)

```text
PROPOSAL V4 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED (V1, V2 y V3: CHANGES REQUIRED; registros I-62-coordinator-review-v1.md, -v2.md y -v3.md)
Architect      = NOT REVIEWED (no se invoca todavía, C62-F0-22; no se simula)
Consensus      = NOT REACHED
Owner          = OD-6 pendiente (Proposal V4 §11.4, dos alternativas delimitadas); sin decisión, no hay AGREED ni Freeze
Implementation = NOT AUTHORIZED

Objeto de la revisión (identidad por contenido; el commit lo da el recibo de publicación):
  docs/initiatives/I-62-proposal-v4.md            blob dd9e478d737c76139b0eecc3c9e30d9beae8ccd6
  docs/initiatives/I-62-coordinator-review-v3.md  blob 461ef605ffc04b1df85ce6cf60d2a132e7506403
Versiones anteriores revisadas:
  V1: commit ea0555913202efc96e1007f021942579b989b43d, blob 31e2f4afa895dc03f7621504e150de0539a2302e
  V2: commit a1f5e0035f0e109d02a336c0a01917e53a53b88a, blob 47a643ddf1f22226072ede67a862133f77441e2e
  V3: commit 486e45e797642ad589980584e16b3e50e53c1fb9, blob 36b13443b3d8cdc21fe731b4a5ab263cc9fd7fb9
Discovery base: docs/initiatives/I-62-discovery.md, R1 (blob 86f24e65…, commit 2b7976b0), con §21 añadida en ea055591
Base de main: 819955d61a6da4c811a11fbd11b5dca13f634b7c
```

> **Identidad exacta** (solución de R62-V1-09, cerrada y preservada). El commit lo da el **recibo de publicación**. El revisor comprueba
> `git rev-parse <commit>:docs/initiatives/I-62-proposal-v4.md` = `dd9e478d…`. Si no coincide, revisa la versión designada o rechaza la discordancia; nunca
> aprueba el último archivo encontrado.

> **Qué es.** La versión completa, el delta V3→V4 y la disposición por ID. La sesión autora no emite veredictos ni declara consenso o independencia. **Las
> trazas de V4 (anexos E.6, F y G.2) son análisis del diseño, no ensayos ejecutados.**

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

1. [Proposal V4](I-62-proposal-v4.md), completa, con los anexos:
   - A: ADR sucesor;
   - B: contratos, incluido **B.8 `state/v2`** completo;
   - C: matriz única;
   - D: pilotos, con **FX-04a y FX-04b**;
   - E: **resolución de autoridades ejecutable**;
   - F: secuencias;
   - G: cambios frente a `/v1`.
2. Registros de revisión del Coordinator: [V1](I-62-coordinator-review-v1.md), [V2](I-62-coordinator-review-v2.md) y [V3](I-62-coordinator-review-v3.md).
3. [Discovery R1](I-62-discovery.md) §§1-3, 9-13 y 21.
4. [Decisiones](../automation/decisions/I-62.md) §§7, 9-13.
5. Fuentes integradas:
   - AUTOMATION_PLAN §§8 y 16, en particular 16.3, 16.4, 16.6 y 16.8;
   - ADR-0046 y el Freeze de I-61 ([Proposal V9](I-61-proposal-v9.md));
   - `gate-contract.schema.json` `/v1` y el contrato real de I-61 G3 (`docs/automation/evidence/I-61-pilot/g3-cama-d1a/`);
   - `routing.md`, `model-catalog.md` y el README de agent-execution;
   - LIFECYCLE §§2-9; WORKFLOW §§3-4, 10 y 11 (11.1-11.3 como precedente de adopción).

## 3. Delta V3 → V4

| Área | V3 | V4 |
|---|---|---|
| Resolución legacy (R62-V3-01) | mapa por superficie y cláusula como política; «`Authorities` del contrato con revisión por cláusula», que `gate-contract/v1` no puede expresar | **algoritmo ejecutable** (Anexo E): resolver independiente del protocolo en AUTOMATION_PLAN **§16.13**, leído en `MainSha` por toda unidad. Punteros en §16 y §16.3; §16.3 conserva literalmente el texto de I-61. **Mapa** en ruta fija leído en `I62_EFFECTIVE_SHA` (inmutable), con esquema `rackcad-clause-map/v1`, `Surfaces` cerradas, `Files` con `BaseBlob`/`EffBlob` y `Entries` por encabezado; validado contra la derivación mecánica `EFF^1` → `EFF` (MV-1..MV-7), con fallo cerrado. **Clasificador** durable (PRE del cuerpo del merge, bootstrap `/v2` con decisión de G0, o decisión tras STOP), aplicado por el Coordinator y re-aplicado en A6' y en `Authority`. `MainSha` y `AuthorityRevision` sin cambio de significado; solo cambia, para unidades I61 y cláusulas del mapa, la revisión de lectura. «documento completo» de un archivo modificado → se cita por sección. C-20 partida en C-20a (guarda Core sin historia), C-20b (derivación con historia, repetida sobre el merge local) y C-20c (trazas con el **contrato `/v1` real** de I-61 G3 adaptado y once negativos) |
| `state/v2` y delegación abierta (R62-V3-02) | `state.custody` parcial; «`open_delegation` planificada» en Q0 | **contrato completo** (B.8): campos, tipos y cardinalidad. Puntos BOOTSTRAP, Q0, Q7, **QU** (actualización sin ventana), **QH** (liberación) y QR. Ventana de cesión con W-1 (solo se abre desde un Q0 publicado), W-2 (sin escrituras de la sesión) y W-3 (el Worker no toca el estado). **Estado de delegación derivado, no almacenado** (B.8.2): PLANNED, ACCEPTED_OPEN (exactamente 16.6), CLOSED_PENDING_CUSTODY, NONE o UNKNOWN. Tabla Q0 / transitorio / Q7 (B.8.3); 14 invariantes de archivo y 8 de pares (B.8.4); **reconstrucción sin diario** R-0..R-3 con conteo conservador y nunca cero (B.8.5); commits sin verificar y su sustitución acumulada (B.8.6). C-15 y C-18 comprueban la semántica (invariantes y `DS`), no solo la forma. Recuperación dentro de la ventana con diario (T12a) separada del QR (T12b) |
| Portabilidad (R62-V3-03) | FX-04 único, integrado en Q0 y con continuación obligatoria por un Worker Codex | **FX-04a**: A llega a un QH y termina (observador externo u Owner); B, sin contexto privado, reconstruye y decide; publica antes del oráculo; comparación mecánica PASS/FAIL; no depende de OD-3, OD-4, OD-7 ni de la escritura de B. **FX-04b**: continuación hasta VERIFIED y Q7, con PASS, FAIL, UNVERIFIED o UNSUPPORTED. FX-03 sigue siendo la topología B completa. Capacidades del Principal **por acción** (RESUME_DECISION frente a CUSTODY, §4.1, E12). Actualizados: criterio 9 (C-15, C-18, C-25a), criterio 12 (C-24, C-26, C-25b), matriz de cierre de F6 (D.4), presupuestos (FX-04a: 0 invocaciones; FX-04b: 6 Codex), OV-I62-05 (a) y (b), D.3, D.5, D.6 y F.4 |

Se preservan los cierres de V1 (R62-V1-09 y O62-V1-01) y todos los cierres de C62-F0-21.

## 4. Disposición por ID (la sesión declara; el cierre lo decide el emisor)

| ID | Punto exigido | Dónde lo atiende V4 | Cierre verificable que se ofrece (análisis) |
|---|---|---|---|
| R62-V3-01 (1) quién clasifica | Coordinator en G0 y antes de cada contrato; re-aplicación en A6' y `Authority`; discrepancia S-04 | §14; E.2 | E.6 N-a (UNKNOWN), N-b (I62 con `/v1`) |
| R62-V3-01 (2) dónde vive el mapa y su identidad | `compatibility/I62-clause-map.json`, ruta fija, leído en `I62_EFFECTIVE_SHA`; blob en el tag; sin cita del blob en §16.13 (ciclo explicado) | E.1, E.4 | MV-1..MV-7 |
| R62-V3-01 (3) descubrimiento antes de `Authority` | la regla de I-61 lee §16 en `MainSha` → puntero → §16.13; citas obligatorias en el contrato; ejecución por el Coordinator y el Controller antes de 16.3 | E.3 (paso 0, paso 3, «cómo lo descubre») | E.6 N-i |
| R62-V3-01 (4) `MainSha` y `AuthorityRevision` | sin cambio de significado (A6, `Remote`, `MB`) | E.3 («significado `/v1` conservado») | E.6: K61' con `MainSha` = M2 |
| R62-V3-01 (5) modificadas a `^1`, resto normal | paso 4 de E.3 por cita | E.3 | E.6 filas 3-4 (X1 visto; X2 no) |
| R62-V3-01 (6) fallo cerrado | MAP_INVALID por ausencia, duplicado, contradicción, blobs, ENTRY extra o punteros | E.4 | E.6 N-c, N-d, N-e, N-j, N-k |
| R62-V3-01 (7) C-20 con contrato `/v1` real | C-20a, C-20b y C-20c; el contrato real de I-61 G3 adaptado (K61) falla por «documento completo» y K61' pasa | Anexo C; E.6 | tabla de 17 citas + 11 negativos |
| R62-V3-02 (1)-(2) contrato y campos | B.8.1 con tipo y cardinalidad de cada campo | B.8.1 | C-18 |
| R62-V3-02 (3)-(4) tres situaciones; 16.6 intacta | `task_intent` (planificada) frente a ACCEPTED_OPEN (solo en el diario) frente a NONE; UNKNOWN explícito | §8.3; B.8.2; G.1 (16.6 conservado) | F.1 columna `DS` |
| R62-V3-02 (5) durable / transitorio | Q0, diario, Q7 | §8.4; B.8.3 | F.1 |
| R62-V3-02 (6) sin diario | R-0..R-3; conteo conservador; commits sin verificar | B.8.5; B.8.6; F.5 | G.2 (dos casos nuevos) |
| R62-V3-02 (7) C-15 y C-18 semánticos | invariantes I-S/I-P y `DS` en cada paso; mutaciones sembradas | Anexo C | C-15, C-18 |
| R62-V3-02 (restricción) | ningún commit entre `CurrentSha` y la verificación (W-2); QR solo fuera de ventana o cerrándola; T12a sin escritura Git | §8.2; §9.2 | F.1 pasos 4-6 |
| R62-V3-03 | FX-04a y FX-04b separados; FX-03 intacto; criterios 9 y 12, F6, presupuestos, OV-I62-05 y Anexo D actualizados | §§1, 4.1, 12, 17, 18; C-25a, C-25b; D.3-D.6; F.4 | D.4 (matriz de cierre); F.4 (paso 7 frente a 8-13) |

## 5. Los 12 retos del mandato («ARCHITECT»)

| # | Reto | Dónde | Riesgo residual que se pide juzgar |
|---|---|---|---|
| 1 | Proveedor en los roles | §2; C-01, C-02 | la asignación por topología del Anexo D es binding de prueba, no semántica |
| 2 | Supuestos no verificables del modelo | §§4.2, 5, 10 | que RUNTIME_OBSERVED baste como mínimo |
| 3 | Autointrospección falsa | §10; B.4 `Assurance`; I-02 candidata | la configuración del cliente frente al backend |
| 4 | Memoria privada oculta | §8; D.6; QH sin artefactos transitorios; oráculo fuera del host; entradas automáticas enumeradas | un registro sin lecturas deja FX-04a en UNVERIFIED |
| 5 | Router complejo | §5 (siete pasos sobre el routing existente) | el número de contratos (B.1), ahora con `clause-map/v1` |
| 6 | Requisitos excesivos entre proveedores | §11.3 (Proveedor casi siempre PREFERRED) | el valor para «Coordinator/Worker en la misma sesión» |
| 7 | Seguridad y autenticación | §6; B.4; C-10 con secretos ficticios | el uso futuro de metadatos de sesiones (SP-3) |
| 8 | Custodia ambigua | §8; B.8 (puntos, ventana, `DS`, invariantes) | dentro de la ventana la delegación abierta solo existe en el diario; sin él, la reconstrucción (B.8.5) es conservadora y puede dejar commits sin verificar que exigen sustitución |
| 9 | Recuperación que sobrescriba trabajo | §9 (T11-T18); B.8.5-B.8.6; F.2, F.5 | no admitir ISOLATION_ACCREDITED puede bloquear recuperaciones legítimas; T12a depende de que el diario sea accesible en el mismo host |
| 10 | Level B sin necesidad | §17; C-15..C-17, C-20b, C-20c en F4 | si el validador semántico y el resolver son tooling de hecho |
| 11 | Esquemas de proveedor | B.2-B.3 (`AdapterId` con patrón; `Facts` como único punto abierto, validado y contrastado) | el coste de un esquema de hechos por adapter |
| 12 | Reanudar en otra máquina | §9.1, T14, B.8.5, F.2; `HostRef` | sin terminación acreditada en otra máquina, la reanudación queda detenida por diseño; desde un QH, en cambio, basta Git |

**Riesgos propios de la resolución legacy** (E.4-E.7), que también se piden juzgar:
- un cambio posterior a §16.13 afecta a toda unidad, porque se lee en `MainSha` como gobernanza (precedente: WORKFLOW §11.3);
- las secciones que I-62 modifica quedan en `EFF^1` para las unidades I61, también ante correcciones posteriores. La mitigación es poner el contenido nuevo en
  secciones nuevas (E.7), y la dirección del riesgo es conservadora (p. ej., una celda del catálogo vista como `STALE` → BLOCKED);
- la CI hace checkout superficial: por eso la parte con historia es MC (C-20b) y la guarda Core no usa historia (C-20a).

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

1. ¿§16.13, independiente del protocolo y leída en `MainSha`, con punteros en §16 y §16.3, es un punto fijo aceptable para el descubrimiento por parte de
   unidades I61?
2. ¿Exigir que una unidad I61 cite por sección los archivos que I-62 modifica (en lugar de «documento completo») es una carga razonable para el Coordinator?
3. ¿El estado de delegación **derivado** (B.8.2), nunca almacenado, satisface la distinción pedida sin crear un concepto paralelo al de 16.6?
4. ¿El conteo conservador de B.8.5 (lo probado + 1 incierto por fase posible) es suficiente sin fijar umbrales nuevos?
5. ¿La sustitución acumulada de commits sin verificar (B.8.6) es preferible a exigir siempre una A-n?
6. ¿La separación RESUME_DECISION / CUSTODY (§4.1) es la lectura correcta de O62-V1-01 para FX-04a?

## 8. Lo que este paquete no hace

No asigna revisor, no realiza la revisión, no invoca al Architect, no declara AGREED ni Freeze, y no autoriza implementación, delegaciones, sondas, pilotos ni
sesiones nuevas.
