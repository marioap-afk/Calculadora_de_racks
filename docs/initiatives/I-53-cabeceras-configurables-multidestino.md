---
schema: rackcad-initiative/v1
id: I-53
title: "Cabecera configurable: configuracion origen hacia conjuntos de destinos en todos los sistemas"
type: feature
status: claimed
branch: feature/cabeceras-configurables-multidestino
base_branch: main
priority:
size:
depends_on: [I-40, I-43]
conflicts_with: []
context_packs: [system-dynamic-flowbed, system-selective, ui-editors, persistence, architecture-kernel]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision:
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-53 — Cabecera configurable: configuracion origen hacia conjuntos de destinos en todos los sistemas

> **Fase actual: G1 CERRADO — Discovery en [I-53-discovery.md](I-53-discovery.md).** No hay contrato de
> autoridad (G2 **no abierto**) y **no hay una sola linea de produccion escrita**. G1 declara decisiones
> arquitectonicas materiales (AM-1..AM-4) para **revision de Arquitecto antes de G2** y decisiones del Owner
> (OD-1..OD-7). La implementacion esta BLOQUEADA (seccion 12).

> **Apertura por autorizacion explicita del Owner sin fila previa** — caso (d) de
> [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la preexistencia de
> la fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap inmediatamente posterior
> al reclamo atomico. La autorizacion es la **instruccion directa del Owner** que abrio la iniciativa;
> **no existe** `docs/automation/decisions/I-53.md` y la norma **no lo exige** para el caso (d), asi
> que `decision_paths` queda vacio en vez de apuntar a un archivo inventado.

```text
Initiative = I-53
Owner IDs  = ID6, ID7 (citados por la autorizacion como columnas «pendiente» de la matriz de G1;
             su texto vive en la numeracion del Owner y NO esta versionado en el arbol)
Branch     = feature/cabeceras-configurables-multidestino
Worktree   = ~/.codex/worktrees/feature-cabeceras-configurables-multidestino
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main, merge de I-51)
CLAIM_SHA  = e1d5996e70cb75589beb96a256714982fe693be3   (commit vacio)
Claim-Id   = d7144fe8-6921-4a46-9d66-2d723620bea4
```

## 1. Objetivo

Que la **configuracion de una cabecera** —la cabecera personalizada que hoy se edita en el
configurador compartido— pueda tomarse como **origen** y aplicarse a un **conjunto de cabeceras
destino**, cada una como **copia independiente** y con **aplicacion atomica**, en **todos los sistemas
vigentes** que tengan cabeceras configurables, y no solo en Push Back.

Push Back **ya lo entrega**: I-40 fijo la cabecera fisica `(PostIndex, ModuleId)` como unidad de
edicion, destinos como producto de cabeceras por lineas y una configuracion origen que se reparte a la
seleccion. El Selectivo tiene, desde I-43, autoridad de cabecera por `(FondoIndex, PostIndex)` y
destinos `TargetFondos × Scope`. Que le falta a cada sistema, y si un contrato compartido lo cierra
sin reabrir lo entregado, es la pregunta de G1 —no una respuesta de este contrato—.

La autorizacion nombra **ID6** e **ID7** como lo pendiente que la matriz de G1 debe medir por
sistema. **Este contrato no redacta su texto**: G1 los lee solo a traves de la evidencia y de lo que
la autorizacion enuncia, y cualquier lectura que no se sostenga en ambos se declara como pregunta al
Owner.

## 2. Problema

Lo que ya consta, sin afirmar nada que G1 deba verificar:

- [ROADMAP](../ROADMAP.md), fila de **I-40**: la cabecera personalizada de Push Back pasa a ser
  autoridad efectiva con destinos multiples; el sistema se da por **entregado** y **no se reabre**.
- [ROADMAP](../ROADMAP.md), fila de **I-43** y
  [ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md): el Selectivo edita
  por `Scope` × `TargetFondos`, con cabecera personalizada por `(FondoIndex, PostIndex)` y commit
  atomico con un solo recompute por operacion.
- El **Dinamico** comparte con Push Back la estructura dinamica y el configurador de cabecera, pero
  ninguna fila de ROADMAP registra para el una edicion de cabeceras por destinos. Si la tiene, la
  tiene parcialmente o no la tiene, lo mide G1.
- **Drive-In**, **Cantilever** y los demas sistemas: si tienen cabecera configurable siquiera, lo mide
  G1 contra el codigo, no contra el menu.

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **G1 — Discovery, solo documentacion.** Auditar con ruta, simbolo, SHA y prueba:
   - **todos los sistemas vigentes** —Selectivo, Dinamico, Push Back, Drive-In, Cantilever y los que
     el registro de sistemas declare— con una matriz por sistema de: cabecera configurable,
     autoridad, alcance/destinos, **ID6 pendiente**, **ID7 pendiente**, adaptacion, persistencia,
     UI/editor, `RACKEDITAR`/Actualizar/guardar-reabrir, BOM y dibujo;
   - **Push Back = YA ENTREGADO**: reconstruir el contrato exacto de I-40 con sus simbolos y
     pruebas, **sin reabrirlo**;
   - **Selectivo**: mapear I-43 —`TargetFondos` × `Scope`, `(FondoIndex, PostIndex)`, resolucion
     masiva → mutacion → reconciliacion → un solo recompute— y el **cruce real** con la rama de I-50;
   - **Dinamico**: auditoria profunda de tipos de cabecera y de separador, frentes, fondos,
     autoridad, editor, persistencia, actualizacion, BOM y dibujo;
   - **evaluar, sin implementar**, un contrato **configuracion origen compartida → conjunto de
     destinos → copias independientes → aplicacion atomica o por lotes**, un
     `HeaderConfigurationSnapshot` neutral **sin UI, sin `ObjectId` y sin `Database`**, la semantica
     de copia profunda y la **mutacion parcial cero**;
   - entregar: preflight, disponibilidad, BASE/CLAIM, rama/worktree, matriz, contrato de I-40, modelo/
     autoridad/persistencia/UI/BOM-dibujo por sistema, propuesta de origen, propuesta de destinos,
     semantica de copia, atomicidad, sistemas a cerrar, conflicto con I-50, gates refinados y si hace
     falta o no Arquitecto.
3. **G2 — contrato** de origen, destinos, copia y atomicidad por sistema. Si el contrato compartido,
   la autoridad o la persistencia resultan una **decision arquitectonica material**, se declara
   explicitamente para **revision de Arquitecto antes de G2**.
4. **Implementacion**: sus gates se definen en G2. **No esta autorizada.**

## 4. Fuera de alcance

- **Cualquier cambio de produccion en G0 y G1**: nada en `src/`, `tests/`, `assets/`, `eng/`,
  `deploy/`, `tools/` ni `.github/`.
- **Reabrir I-40**: Push Back se reconstruye como referencia, no se rediseña ni se «unifica» por
  arrastre.
- **Reabrir I-43 o ADR-0032**: las decisiones del Selectivo sobre alcance, fondos, proyeccion 2D,
  omision sin clamp y pendiente/comprometido **no se re-litigan**.
- **Territorio de iniciativas paralelas**: cotas por vista (**I-50**), motor de expresiones, Project
  Variables y `LinkedPropertyEditor` (**I-49**), `RACKMIRROR` (**I-52**) y propiedades personalizadas
  (**I-54**).
- Implementar `HeaderConfigurationSnapshot` o cualquier otro contrato compartido antes de G2;
  migracion de dibujos; cambio de `SchemaVersion`; catalogos; `blocks-library.dwg`.
- Los hallazgos laterales del Discovery se registran en [ideas-futuras.md](../ideas-futuras.md);
  **no se arreglan aqui**.

## 5. Contexto requerido

- Contratos de [I-40](I-40-cabeceras-push-back.md), [I-43](I-43-selectivo-scopes-fondos.md),
  [I-35](I-35-editor-avanzado-push-back.md) (sesion de modulos y reconciliacion por `ModuleId + Kind`),
  [I-17](I-17-clon-unico-cabecera.md) (clon unico de `RackFrameConfiguration`) e
  [I-42](I-42-push-back-compuesto.md).
- [ADR-0031](../adr/0031-push-back-compuesto-estructura-unica-y-configuracion-por-lado.md) y
  [ADR-0032](../adr/0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md).
- [AGENTS.md](../../AGENTS.md), convenciones 2 (regla en un solo sitio), 3 (copia centralizada) y 4
  (persistencia versionada).
- Context Packs declarados en el frontmatter.

## 6. Dependencias

- **I-40 e I-43, integradas**: son la base que la autorizacion cita. No hay dependencias pendientes.
- **Iniciativas paralelas a vigilar.** Al reclamar, `origin` tenia `main`, **I-49**
  (`architecture/motor-expresiones-parametricas`), **I-50** (`feature/cotas-independientes-por-vista`),
  **I-52** (`feature/rackmirror-espejo-semantico`) e **I-54** (`architecture/propiedades-personalizadas`).
  `conflicts_with` queda **vacio** hasta que G1 mida el cruce real de archivos: no se declara un
  estorbo sin evidencia, ni se omite uno que la tenga.
- **Entrada del Owner**: probable, porque el texto de ID6 e ID7 no esta versionado. Aun asi
  `requires_owner_decision` queda **vacio** hasta G1, en vez de fijarse por analogia.

## 7. Archivos esperados

**Este contrato no fija todavia la lista de archivos, y no debe fingir que si.** El mapa por archivo
y simbolo **es el entregable de G1**.

Lo que si se declara, como area y como limite: la autoridad de cabecera de cada sistema en
Application, sus editores en UI y, si G2 lo justifica, una pieza **pura** compartida. Los tres
editores grandes y los DTO de sistema son **archivos calientes** ([WORKFLOW](../WORKFLOW.md) seccion
7). **Una desviacion material frente a esto obliga a detenerse** y a volver a G2.

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| G1 | Discovery | Informe por archivo/simbolo/SHA con las respuestas de la seccion 3, punto 2 | **HECHA** — [I-53-discovery.md](I-53-discovery.md) |
| G2 | Contrato | Origen, destinos, copia y atomicidad; revision de Arquitecto (AM-1..AM-4) y decisiones del Owner (OD-1..OD-7) declaradas en el Discovery §18 | pendiente — **no abierto** |
| G3+ | Implementacion, Candidato, validacion del Owner, integracion | Por definir **en G2**, no aqui | bloqueada |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Se fijan **en G2**, cuando exista alcance de codigo. Lo que ya es exigible por norma y no depende de
G2: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA exacto**, y builds Debug
de UI y de Plugin (AGENTS.md, «Pruebas — definicion de terminado»). G0 y G1 no producen codigo: su
evidencia es documental.

## 10. Validacion manual

**Requerida** en la implementacion: aplicar una configuracion de cabecera a varios destinos cambia
dibujo y BOM (AGENTS.md, punto 5). El **checklist concreto se fija en G2**. G0 y G1 **no** la
requieren: no tocan producto.

## 11. Criterios de aceptacion

Preliminares; **los fija G2**. Solo recogen lo que la autorizacion enuncia:

1. Una configuracion de cabecera origen se aplica a un **conjunto de destinos** elegido por el
   usuario, en cada sistema que G2 declare a cerrar.
2. Cada destino recibe una **copia independiente**: editar despues un destino no altera al origen
   ni a otro destino.
3. La aplicacion es **atomica**: o todos los destinos validos cambian, o ninguno; **mutacion parcial
   cero**.
4. Lo que Push Back ya entrega con I-40 **no cambia**.
5. El resultado sobrevive `RACKEDITAR`, Actualizar y guardar/reabrir, y BOM y dibujo leen la
   **misma** autoridad.

## 12. Condiciones para detenerse

- **COMPUERTA DE ESTA SESION — G0 y G1 solamente.** Ninguna linea de produccion. G2 no se abre; G3+
  queda bloqueado hasta cerrar G2.
- Si el contrato compartido, la autoridad o la persistencia exigen una **decision arquitectonica
  material**: **declararla para revision de Arquitecto antes de G2**.
- Si el texto de **ID6** o **ID7** no se puede sostener con la evidencia: **pregunta al Owner**, no
  una lectura del Discovery.
- Si G1 encuentra **archivos productivos compartidos materiales con I-49, I-50, I-52 o I-54**:
  reportarlo antes de continuar.
- Si cerrar un sistema exige cambiar el formato persistido o `SchemaVersion`: **detenerse**; eso es
  ADR y decision del Owner.
- Si el alcance deriva hacia reabrir I-40 o I-43: **detenerse** (seccion 4).

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-53.yml`: la automatizacion esta **desactivada** (`automation.enabled:
false`) y los precedentes inmediatos —I-47, I-48 e I-51, tambien manuales— tampoco lo llevaron.
[WORKFLOW](../WORKFLOW.md) seccion 8 exige para el bootstrap del caso (d) exactamente **fila en
ROADMAP mas contrato**, y **no** un archivo de estado. El estado vivo se deriva, como manda la seccion
2, de la existencia de `origin/feature/cabeceras-configurables-multidestino`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5).

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-53 - Cabecera configurable hacia conjuntos de destinos en todos los sistemas
Rama       = feature/cabeceras-configurables-multidestino
Worktree   = ~/.codex/worktrees/feature-cabeceras-configurables-multidestino
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8  (origin/main)
CLAIM_SHA  = e1d5996e70cb75589beb96a256714982fe693be3  (commit vacio)
Claim-Id   = d7144fe8-6921-4a46-9d66-2d723620bea4
```

Primer `git push -u origin feature/cabeceras-configurables-multidestino` **aceptado sin force**
(`* [new branch]`); tras el push, `git log --remotes --grep "Initiative-Id: I-53"` devuelve **solo**
este reclamo. `main` **no fue modificada**.

El resto de la evidencia se acumula al cerrar cada fase.
