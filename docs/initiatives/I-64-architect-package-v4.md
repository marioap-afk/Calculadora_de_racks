# I-64 — Paquete de re-revisión del Architect (Proposal V4)

```text
PROPOSAL V4 — NOT REVIEWED
Coordinator    = REVIEW REQUIRED (sobre la misma V4, después del Architect)
Architect      = RE-REVIEW REQUIRED por el MISMO Architect que emitió A64-PV3-01..06
Consensus      = NOT REACHED
Freeze         = NONE (aunque el Architect emita AGREED, este paso no crea Freeze; el Coordinator lo ordena después)
Implementation = NOT AUTHORIZED

Objeto de la re-revisión = docs/initiatives/I-64-proposal-v4.md, en el commit que publica este paquete (rama
                           architecture/workspace-persistente-rackcad). El SHA, el blob y la CI de ese commit no se escriben aquí para
                           no autorreferenciarse: se toman de `git log -1 -- docs/initiatives/I-64-proposal-v4.md`, de `git rev-parse` y de
                           la corrida de CI de ese SHA, que el informe de entrega da exactos.
Versión revisada antes   = V3: commit 1f1530be15a3bc95e260c03df07d51b9625345ae, proposal blob d08c2cee75ee486bf5ed8f2ebebe6d2482aebbbe,
                           package blob 45133653e5eeea6e6f7a7dd1f1493d4a48e3b90f, CI 36934958555 (push, head_sha exacto, 4/4 success)
Historia intacta         = V1 (dcc16bed, 962fc195), V2 (ab4efe86, 7bf98cca), V3 (1f1530be, d08c2cee): sin cambios en este commit
Dictamen previo          = sobre V3: A64-PV1-01..22 = CLOSED; A64-PV3-01..06 = REQUIRED; O3-01..O3-07 = OPTIONAL (evidencia §16)
Orden del Coordinator    = F0 / Proposal V4, con disposiciones vinculantes para A64-PV3-01..06 y los opcionales a adoptar (evidencia §16)
Base de main             = 819955d61a6da4c811a11fbd11b5dca13f634b7c (sin cambio; sin rebase)
```

**Identidad del revisor** (la misma de los dictámenes sobre V1 y V3, según la declaró el propio Architect):

| Campo | Valor |
|---|---|
| Rol | Architect |
| Modo | SEPARATE SESSION; revisor = autor: NO |
| Sesión | «I-64 Architect Review Proposal V1» (`local_172c5d51-ea91-4f95-8c17-23a1563e61b7`) |
| Perfil | ARCHITECTURE_REVIEW / Deep |
| Límite de independencia declarado | mismo modelo (Claude) que la sesión autora |
| Autoridad sobre sus REQUIRED | solo este revisor puede cerrar o rebajar A64-PV3-01..06, con ID, razón y evidencia |

> **Qué es este paquete.** El material para la re-revisión: la versión completa (Proposal V4, autocontenida), el **delta V3→V4**, la
> disposición de **A64-PV3-01..06**, el estado de **A64-PV1-01..22 (CLOSED)** y los **opcionales adoptados** (LIFECYCLE §5). El delta es el
> foco mínimo, no un límite. La sesión autora no emite ni simula el veredicto, no declara consenso, Freeze ni GATE PASS, y no afirma la
> independencia de nadie.

## 1. Veredicto que se solicita

```text
A64-PV3-01 = CLOSED | STILL OPEN (razón)
A64-PV3-02 = CLOSED | STILL OPEN (razón)
A64-PV3-03 = CLOSED | STILL OPEN (razón)
A64-PV3-04 = CLOSED | STILL OPEN (razón)
A64-PV3-05 = CLOSED | STILL OPEN (razón)
A64-PV3-06 = CLOSED | STILL OPEN (razón)
Hallazgos nuevos: A64-PV4-nn REQUIRED | OPTIONAL, con sección, evidencia y cambio exigido
Architect: AGREED (todos CLOSED y ningún REQUIRED nuevo) | CHANGES REQUIRED | BLOCKED — OWNER DECISION
Modo: SAME-SESSION ROLE | SEPARATE SESSION | EXTERNAL HUMAN; si revisor y autor son la misma persona
Versión: el veredicto vale solo para el SHA y el blob exactos revisados
```

Un `AGREED` del Architect **no** crea Freeze: el Coordinator revisa la misma V4 y ordena después el Consensus Freeze (LIFECYCLE §6).

## 2. Lectura mínima

1. [Proposal V4](I-64-proposal-v4.md), completa: en especial D-02, D-04, D-05, D-06, D-07, D-11..D-13, D-17, D-18, D-20, §10 (INV-17, 20,
   35..38), §11, §12 (§12.1-§12.5), §16, §18 y §20.
2. Este paquete: delta (§3), disposición (§4), historia (§5) y opcionales (§6).
3. [Evidencia](../automation/evidence/I-64-evidence.md) §16: orden de V4, procedencia del dictamen sobre V3, CI de V3, puntas y hechos de
   código verificados.
4. Código de la base citado: `RackBlockFinder.ScanEnvelopes`, `RackEnvelopeIdProbe` (ámbito declarado en su línea 6),
   `RackSiblingScan.cs:103` (transacción propia del barrido), `RackSiblingMembership.cs`, la costura de I-55 y `RackListBuilder`.

## 3. Delta V3 → V4

| Sección | V3 | V4 |
|---|---|---|
| Cabecera y fuentes | revisión V3 pendiente | dictamen sobre V3 registrado; orden V4 como fuente; revisión V4 pendiente |
| §0 | — | `NavigationTargets`, InventoryStatus y SessionOverlay; el índice runtime se declara privado |
| §1 | criterio 11 en F1..F7; criterio 12 en F6 y READY | criterio 11 en F3, F6, F7 y Smoke-PERF; criterio 12 solo en READY |
| D-02 | escrituras y drenaje con su contexto | + lecturas que inicia el usuario declaradas (O3-01) |
| D-04 | «los manejadores solo encolan»; el modo degradado retira todas las suscripciones | manejadores ordinarios solo encolan; excepción acotada de `BeginDocumentClose` (veto síncrono con estado en memoria); el modo degradado conserva `BeginDocumentClose` mientras haya dirty o texto pendiente (A64-PV3-04) |
| D-05 | «RackId curado»; atribución por sondeo sin límites; separación de métricas declarativa; navegación de F2 desde D-11 y de F6 desde el índice; estado por rack mezclado | «Id no vacío tal como figura en el sobre»; sondeo solo para diagnóstico; **privacidad del índice** con rótulos de navegación o selección (A64-PV3-01); **una sola autoridad `NavigationTargets`** para contexto, pestañas y navegador (A64-PV3-05); **InventoryStatus** separado del **SessionOverlay** (A64-PV3-02); un solo diagnóstico «ilegible o de versión futura» (O3-03) |
| D-06 | — | Uno(R) solo desde sobre interpretable; ilegible con Id sondeable = Diagnóstico |
| D-07 | objetivos de las definiciones miembro | objetivos de `NavigationTargets` |
| D-11 | AUTH-06 también en la primera navegación | AUTH-06 solo en la intención de edición y en Actualizar; el origen de apertura nunca es un miembro atribuido por sondeo |
| D-12, D-13 | relectura «dentro de la transacción» | la relectura lee por la misma transacción caller-owned de MUTATE (O3-02); desde Desconocido, la relectura coincidente restaura el estado previo (O3-06) |
| D-17 | veto en todo intento | + decisión síncrona con estado en memoria; vigente en modo degradado |
| D-18 | P-03 sin excepción; tabla con escenarios de host como criterio de gates | P-03 con la excepción acotada; **arneses automatizados** (4, 5, 6 y sintéticos 7, 8) como criterio de cierre; escenarios de host solo en hitos del Owner, con **Smoke-PERF** tras F6 y antes de READY (A64-PV3-03) |
| D-20 | F6 entrega la evaluación de ID3 | F6 entrega puntos de extensión; la clasificación de ID3 se hace en READY tras F4 y F5 (A64-PV3-02) |
| §10 | INV-01..35 | INV-17, INV-20 e INV-35 ajustados; nuevos **INV-36** (privacidad), **INV-37** (índice sin estado de sesión) e **INV-38** (una sola autoridad de objetivos) |
| §11 | F3/F6/F7 con escenarios de host al cierre; «Smoke-2 tras F4» ambiguo | F3 y F6 cierran con arneses; F7 consolida sin AutoCAD; READY incluye la clasificación de ID3; **momento de los hitos** explícito (A64-PV3-06) |
| §12 | §12.1 «solo después de cerrar el gate»; §12.4 «tras F4» | momento por hito; Smoke-1 tras F1 PASS; Smoke-NAV tras F2 PASS; Smoke-2 dentro del cierre de F4; **§12.5 Smoke-PERF** nuevo (SP-01..SP-06); SN-10; la matriz OV pasa a §12.6 |
| §13 | — | tareas de `NavigationTargets`, relectura caller-owned, guardas de privacidad y arneses sintéticos; F7 sin AutoCAD |
| §15 | puntas de V3 | puntas actuales (I-52 `352cc3de`, I-62 `3888c5c8`, I-63 `772ac242`) |
| §16 | — | relectura caller-owned (obligación); R-10 y R-11 con su mitigación |
| §17, §18 | disposición de A64-PV1 atendida | A64-PV1-01..22 **CLOSED** (historia, sin reescribir el dictamen) |
| §20 | — | **nueva**: A64-PV3-01..06 → disposición → cláusula y oráculo; opcionales O3 |
| Anexo A | — | decisiones 3, 4 y 8 ajustadas a lo anterior |

## 4. Disposición de A64-PV3-01..06

Tabla completa en la Proposal V4 §20.

| ID | Estado propuesto | Cláusula de V4 | Oráculo |
|---|---|---|---|
| A64-PV3-01 | atendido | D-05 «Privacidad del índice» | INV-36; SP-06 |
| A64-PV3-02 | atendido | D-05 «Estado por rack»; D-20; §11 (F6, READY) | INV-37 |
| A64-PV3-03 | atendido | D-18 «Medición»; §11; §12.5 | arneses de F3 y F6; SP-01..SP-06 |
| A64-PV3-04 | atendido | D-04; D-17; P-03 | INV-20 ampliado |
| A64-PV3-05 | atendido | D-05; D-06; D-07; D-11 | INV-38; INV-35; SN-10 |
| A64-PV3-06 | atendido | §11 «Momento de los hitos»; §12.1-§12.4 | secuencia de cierre de F4 |

## 5. Historia: A64-PV1-01..22 = CLOSED

Cerrados por este mismo Architect en su dictamen sobre V3 (evidencia §16). La Proposal V4 §18 conserva su tabla de disposición como
registro, sin reescribir el dictamen. V4 no reabre ninguno.

## 6. Opcionales

| Opcional | Estado en V4 |
|---|---|
| O3-01 | adoptada (D-02) |
| O3-02 | adoptada (D-12, D-13; §16) |
| O3-03 | adoptada (D-05; INV-17) |
| O3-04 | sigue opcional |
| O3-05 | sigue opcional |
| O3-06 | adoptada (D-13) |
| O3-07 | sigue opcional |
| O-01..O-17 (de V1) | sin cambio: quince adoptadas; O-02 no adoptada; O-09 solo en su medición |

## 7. Lo que este paquete no hace

No realiza la re-revisión. No declara AGREED, consenso, Freeze, GATE PASS ni independencia. No crea Freeze aunque el Architect emita
AGREED. No autoriza implementación, delegaciones, sondas, pilotos ni ningún uso de AutoCAD. `IMPLEMENTATION AUTHORIZATION = NO`.
