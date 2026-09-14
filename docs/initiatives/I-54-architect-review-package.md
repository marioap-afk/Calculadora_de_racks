# I-54 — Paquete de revision exact-SHA: Proposal V5 (G2J, final)

> Paquete **autonomo** para la revision final de [I-54-proposal-v5.md](I-54-proposal-v5.md). No autoriza
> implementar ni sustituye a la Proposal. Las versiones anteriores de este paquete viven en `8bc991c` (V4), `ff98b9e`
> (V3; pre-rebase `5d25da8`), `2d0d666` (V2) y `728d11f` (V1).

## 0. Que se pide y que no

**Se pide** una revision **exact-SHA** de la Proposal V5, por el Coordinador y por el Arquitecto, sobre el **mismo**
SHA (§1), limitada **exclusivamente** a **AR-54-V4-01 / C-F3**: la descripcion de como fallan los consumidores
existentes ante el residual F-14b, y sus trazas.

**No se pide** volver a revisar C-F1, el rebase, la arquitectura, el alcance del ADR, la autoridad, la
profundidad, el `Kind`, `JsonElement?` ni la UI: G2H los dejo verificados y acordados. Tampoco se pide implementar,
redactar el ADR, crear tags ni editar la Proposal.

Todo lo que C-F3 no toca es identico a V4; el diff de §1 lo demuestra.

## 1. Identificacion exacta

```text
Repositorio          = marioap-afk/Calculadora_de_racks
Rama                 = architecture/propiedades-personalizadas
BASE_SHA             = 46fcac2b071929d2bd5b07aa28373941417f74a8   (base del Discovery; la evidencia de codigo se cita aqui)
POST_REBASE_BASE_SHA = f8deb675c6d1ef0e64693b157d69c4cc170d7b24   (origin/main; sin cambios en G2I, sin rebase)
PROPOSAL_V4          = 8bc991c0e1854bc9eb4013421e01f0515c2e77ca   (CI 34742026072 verde)
REVIEW_V4            = Architect Review — I-54 Proposal V4 @ 8bc991c = AGREED WITH CHANGES; G2H CLOSED
                       solo AR-54-V4-01 (MINOR de redaccion); Coordinator AGREED; el Coordinador acepta el cambio
PROPOSAL_V5          = el commit que introduce docs/initiatives/I-54-proposal-v5.md   ← SHA REVISADO
```

El SHA revisado se fija **antes** de leer:

```bash
git fetch origin
git log -1 --format=%H origin/architecture/propiedades-personalizadas -- docs/initiatives/I-54-proposal-v5.md
```

Delta exacto de V5 sobre V4, con `<V5_SHA>` = el SHA anterior:

```bash
git diff 8bc991c0e1854bc9eb4013421e01f0515c2e77ca:docs/initiatives/I-54-proposal-v4.md <V5_SHA>:docs/initiatives/I-54-proposal-v5.md
```

El veredicto vale **solo** para ese SHA.

## 2. El cambio

| | V4 | V5 (C-F3) |
|---|---|---|
| **F-14a** | Los flujos de Rack de I-54 y los demas comandos que barren «fallan cerrados, sin escribir» | El barrido de I-54 lanza **antes de cualquier mutacion** y no llega al ejecutor de Rack de I-54; los consumidores existentes que leen ese sobre fallan en su propia lectura |
| **F-14b** | Los flujos que reescriben el sobre, `RACKEDITAR` incluido, «fallan sin escribir» | Ocho puntos: el sobre se deserializa; reserializarlo lanza; **ese** sobre no se escribe; I-54 no cambia como fallan los consumidores existentes; el punto de fallo depende de su **granularidad transaccional**; caso verificado: `RACKEDITAR` confirma una transaccion por vista y puede quedar **parcialmente actualizado**; ya ocurre en BASE; solo el ejecutor de Rack de I-54 garantiza atomicidad (D-22.10), sin extrapolarla |
| **D-13** | «Con ellos, los flujos que reescriben el sobre fallan sin escribir» | Nota en dos partes: **A** garantia de aislamiento de I-54 para sobres fuera de F-14a/F-14b; **B** residuales preexistentes (F-14a al leer; F-14b al reescribir, segun la granularidad del consumidor, con `RACKEDITAR` parcial y D-22.10 solo para I-54) |
| **RP-11** | «En ambos casos ... fallan sin escribir» | F-14a y F-14b separados; mitigacion: residual documentado, caracterizacion, atomicidad propia en D-22.10 y T-MUT-08, registro de F-14 en el freeze |
| **P-37** | — | Evidencia [E] de la transaccion por vista de `RACKEDITAR` |

**No cambian**: D-22.10 e INV-07 (byte a byte identicos a V4), las pruebas (T-CHR-04, T-ENV-13 y T-MUT-08
incluidas) y §15.

## 3. Lectura obligatoria

1. **Proposal V5**: §0; P-37; D-08.4 (viñetas F-14a y F-14b y bloque «Para F-14a y F-14b»); nota de D-13; RP-11;
   §17.1, §18.1, §19 (RD-07) y §22. Como referencia, sin revisarlas: P-07, RP-03, D-22.10 e INV-07.
2. **Codigo** en `BASE_SHA` (`git show 46fcac2:<ruta>`), identico en `POST_REBASE_BASE_SHA`:

   | Archivo | Lineas | Para |
   |---|---|---|
   | `src/RackCad.Plugin/RackSelectivoCommands.cs` | 166-223 | `RACKEDITAR` recorre las vistas en secuencia, sin captura por iteracion |
   | `src/RackCad.Plugin/RackSelectivoCommands.cs` | 177, 382-394 | el payload de cada vista (`Compose` y `RackEmbedStore.Serialize`) se serializa justo antes de redibujarla |
   | `src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs` | 55-65 | cada `RedrawInPlace` abre y confirma su propia transaccion |
   | `src/RackCad.Application/Persistence/RackEmbedDocument.cs` | 76-84 | `Serialize` no captura |

3. **Discovery**: `docs/initiatives/I-54-discovery.md` §7.2, lineas 504-514 («una por vista»; «un fallo en una vista
   no aborta las demas»).

## 4. Preguntas de la revision

| # | Pregunta | Pista |
|---|---|---|
| Q-1 | ¿F-14b describe correctamente el fallo del sobre afectado: se deserializa, reserializarlo lanza y ese sobre no se escribe? | D-08.4 F-14b, puntos 1-3; P-36 |
| Q-2 | ¿V5 deja de afirmar atomicidad para los consumidores existentes? | D-08.4 F-14b, puntos 4-5; D-13 B; RP-11 |
| Q-3 | ¿Reconoce la escritura parcial posible de `RACKEDITAR`? | D-08.4 F-14b, punto 6; P-37; §3 de este paquete |
| Q-4 | ¿Mantiene intacta la garantia de D-22.10, exclusiva del ejecutor de I-54, sin extrapolarla? | D-08.4 F-14b, punto 8; D-22.10 identico a V4 |
| Q-5 | ¿D-13 y RP-11 estan alineados con D-08.4, y F-14a queda separado? | D-13 A/B; RP-11; D-08.4 F-14a |
| Q-6 | ¿La entrada futura de F-14 (en G2-FREEZE) tiene la descripcion correcta? | D-08.4, bloque «Para F-14a y F-14b»; fila G2-FREEZE de §14 |

## 5. Ramas paralelas (medidas al publicar V5)

| Rama | SHA | Impacto en C-F3 / F-14b |
|---|---|---|
| `main` | `f8deb675c6d1ef0e64693b157d69c4cc170d7b24` | Ninguno; los archivos de §3 son identicos a `BASE_SHA` |
| I-49 `architecture/motor-expresiones-parametricas` | `364d6c06e44273a63a7b6f6509daf357611ea77a` | Ninguno: solo docs (ADR-0038 aceptado y consensus freeze de su V6); no toca el sobre |
| I-52 `feature/rackmirror-espejo-semantico` | `545c2229de8d7850e03981d85aced0ac63c24040` | Ninguno sobre la correccion; su espejo reserializa el sobre y puede compartir F-14b |
| I-53 `feature/cabeceras-configurables-multidestino` | `4e00a273e22634cb3abbc1b3fb3778edf49dbc7e` | Ninguno: no toca el sobre ni su reserializacion |

## 6. Hallazgo a re-verificar

| Hallazgo | Sev. | Donde lo cierra V5 | Que re-verificar |
|---|---|---|---|
| AR-54-V4-01 | MINOR (redaccion) | P-37, D-08.4, D-13, RP-11 | Q-1..Q-6 |

## 7. Formato del veredicto

```text
Architect Review — I-54 Proposal V5
Reviewed SHA = <SHA de 40 hex>
GLOBAL VERDICT = AGREED | AGREED WITH CHANGES | NOT AGREED

C-F3        = VERIFIED | DEFECT
AR-54-V4-01 = CLOSED | REOPENED
Q-1..Q-6    = YES | NO (cada una)
RD-07       = AGREE | AGREE WITH CHANGE

New findings (solo dentro de §0) = NONE | [BLOCKER | MATERIAL | MINOR] AR-54-V5-XX — <problema> — <evidencia> — <cambio requerido>
Required Proposal changes = NONE | <lista cerrada y vinculante>

G2J Architect Review = CLOSED | BLOCKED
Architect = AGREED | AGREED WITH CHANGES | NOT AGREED
Consensus = NOT REACHED
Implementation = BLOCKED
```

El Coordinador emite su veredicto con el mismo encabezado («Coordinator Review — I-54 Proposal V5»).

## 8. Reglas

- La evidencia de codigo se cita sobre `BASE_SHA`; si `origin/main` avanza al revisar, se citan ambos SHAs.
- Fuera de §0 solo se levanta un BLOCKER.
- **La revision no desbloquea la implementacion.** Si G2J declara `Architect = AGREED` y el Coordinador coincide,
  no habra V6 y el siguiente paso sera G2-FREEZE: registro del consenso, ADR `propuesto`, F-01..F-14 en
  `ideas-futuras.md`, tag de archivo del SHA pre-rebase y aprobacion del Owner.
