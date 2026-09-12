---
schema: rackcad-initiative/v1
id: I-52
title: "ID16 — RACKMIRROR: espejo semantico de uno o varios racks"
type: feature
status: claimed
branch: feature/rackmirror-espejo-semantico
base_branch: main
priority:
size:
depends_on: [I-51]
conflicts_with: []
context_packs: [autocad-plugin, persistence, architecture-kernel, system-selective, system-dynamic-flowbed]
automation_state_path:
decision_paths: []
requires_ci: true
requires_plugin_build: true
requires_autocad: true
requires_owner_decision: true
requires_owner_validation: true
automation:
  enabled: false
  auto_merge: false
  max_attempts: 3
---

# I-52 — ID16 — RACKMIRROR: espejo semantico de uno o varios racks

> **Fase actual: RECLAMADA Y BOOTSTRAPEADA (G0).** No hay Discovery versionado, no hay Proposal y **no hay
> una sola linea de produccion escrita**. La sesion que abrio la iniciativa esta autorizada **solo para G0 y
> G1**; la implementacion esta BLOQUEADA (seccion 12).
>
> ```text
> SUBSTANTIVE IMPLEMENTATION: BLOCKED
> Coordinator: NOT YET AGREED
> Architect: NOT YET AGREED
> ```

```text
Initiative = I-52
Owner ID   = ID16 — RACKMIRROR: espejo semantico de uno o varios racks
Branch     = feature/rackmirror-espejo-semantico
Worktree   = ~/.codex/worktrees/feature-rackmirror-espejo-semantico
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main, merge de I-51)
CLAIM_SHA  = 55281769d5e9317ad25500e6d3f5f8a849f37279   (Claim-Id 232b55c5-3d0c-47c7-90ae-a07caab06431)
```

> **Apertura por autorizacion explicita del Owner sin fila previa** — caso (d) de
> [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la preexistencia de la
> fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap inmediatamente posterior al
> reclamo atomico. La autorizacion es la **instruccion directa del Owner**, transmitida por el Coordinador
> de I-52; **no existe** `docs/automation/decisions/I-52.md` y el caso (d) **no lo exige**, asi que
> `decision_paths` queda vacio en vez de apuntar a un archivo inventado.

## 0. Gobierno de la iniciativa

```text
Owner → Coordinador I-52 ↔ Arquitecto I-52 → consenso → ejecutor
```

- El ejecutor **no cierra** decisiones de producto ni de arquitectura por su cuenta.
- **Ninguna implementacion sustantiva** antes de que Coordinador **y** Arquitecto declaren `AGREED` sobre la
  **misma** Proposal.
- Las decisiones que se tomen se versionaran en `docs/automation/decisions/I-52.md` **cuando existan**; hoy
  no existe ninguna.

## 1. Objetivo

Un comando `RACKMIRROR` que produzca el **espejo semantico** de **uno o varios** racks: no solo la
reflexion grafica de sus referencias, sino un rack cuyo **estado authored** describe el resultado reflejado,
de modo que ese rack **no se «desespeje»** al guardar, reabrir, editar con `RACKEDITAR`, pulsar Actualizar o
redibujar.

Exigencias fijadas en la apertura, que el diseno debe poder sostener sobre el codigo real:

1. **Espejo semantico, no grafico.** Las propiedades dependientes de lado se ajustan en el marco **LOCAL**
   que les da significado. **No** se deducen intercambios a partir del eje X del mundo.
2. **Uno o varios racks** en un solo gesto, **reutilizando** la infraestructura multi-rack de I-51 donde
   pueda hacerse limpiamente, **sin duplicarla**.
3. **Sin busqueda automatica de vistas hermanas.**
4. **Variables de proyecto intactas**: `ProjectVariableReference(X) → X`. El espejo **no** crea variables,
   **no** materializa literales, **no** evalua expresiones y **no** resuelve formulas.

## 2. Problema

Lo que esta **documentado** hoy:

- **No existe** un comando de espejo de racks: `RACKMIRROR` no aparece en el codigo ni en la guia de
  comandos; la unica mencion del repositorio es su exclusion.
- I-51 lo dejo **expresamente fuera**: contrato de I-51 seccion 4 («**ID16** (RACKMIRROR)») y
  [HANDOFF](../HANDOFF.md) seccion 4 («Lo que I-51 dejo expresamente fuera»).
- El contrato de I-51 seccion 3.8 fija que el desplazamiento comun por destino **es suficiente para ID15** y
  **«No es ID16 ni ID19»**: la transformacion de `RACKDUPLICAR` es una traslacion que conserva orientacion,
  no una reflexion.

Lo que **no se afirma aqui** y es hipotesis de G1: que ocurre hoy si el usuario aplica el `MIRROR` nativo de
AutoCAD a una referencia de rack, que estado authored conserva el rack tras ello y que haria `RACKEDITAR`
con ese rack.

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **G1 — Discovery, solo documentacion.** Caracterizar con evidencia archivo:simbolo:
   - la **fundacion de I-51** (`RackDuplicationPlan`, `RackDuplicarCommands`, `RackEnvelopeRestamp`,
     seleccion multiple, agrupacion por `RackId`, legacy, deduplicacion de definiciones, clon, payload
     authored, referencias de vista, identidad y nombres, atomicidad PREPARE/MUTATE y sus pruebas), y que
     parte reutiliza el espejo directamente y que parte exigiria extraccion o generalizacion;
   - las **transformaciones** existentes (posicion, rotacion, escala, marcos y ejes locales, reflexiones) y
     si Application/Domain pueden representar una reflexion **sin** `Matrix3d` de AutoCAD;
   - **seleccion y vistas**: reconocimiento de referencias RackCad, agrupacion de vistas de un mismo rack,
     significado fisico de seleccionar solo algunas vistas, Model Space y Paper Space, `ViewKind` por
     sistema, y como `RACKEDITAR` y el redibujo recuperan el estado authored;
   - la **semantica por sistema** de **todos** los sistemas registrados, con una matriz inicial de
     propiedades sensibles al espejo (lado, A/B, frente/fondo, entrada/salida, flujo, seguridad, topes,
     guardas, cabeceras asimetricas, orientacion) y el marco local de cada una;
   - el ciclo **espejo → payload authored → guardar → reabrir → RACKEDITAR → Actualizar → redibujo**, el
     BOM y las cantidades invariantes bajo reflexion;
   - como I-51 preserva los vinculos a variables de proyecto y que necesita el espejo para lo mismo;
   - el cruce con **I-49** e **I-50** (sin mezclar sus ramas ni depender de ellas);
   - la existencia de un **contrato historico** de RACKMIRROR y, si no existe, las **opciones** de producto
     copia/en sitio con su impacto, **sin elegir**.
3. **G2 — Proposal V1** y revision de Arquitecto; consenso Coordinador + Arquitecto sobre la misma Proposal.
4. **Implementacion**: sus gates se definen **despues del consenso**. **No esta autorizada.**

## 4. Fuera de alcance

- **Cualquier cambio de produccion en G0 y G1**: nada en `src/`, `tests/`, `assets/`, `eng/`, `deploy/` ni
  `.github/`.
- **Resolver la decision de producto** copia reflejada frente a espejo en sitio: G1 presenta opciones; la
  decide la cadena de gobierno de la seccion 0.
- **Cambiar la semantica de `RACKDUPLICAR`** (I-51, ID15), `RACKLAYOUT` o `RACKRELLENAR`.
- **ID19** (multi-rack projection), **ID21** (referencias rack a rack), **ID22B** / motor de expresiones.
- **La semantica de Project Variables** ([ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md))
  y de la edicion vinculable (I-48).
- **Territorio de iniciativas paralelas**: cotas por vista (I-50) y motor de expresiones (I-49). No se
  mezclan sus ramas ni se anaden dependencias directas a ellas.
- Buscar vistas hermanas automaticamente, migracion de dibujos y cualquier **framework general** de
  seleccion o de transformacion que el Discovery no justifique.
- Los hallazgos laterales se registran en [ideas-futuras.md](../ideas-futuras.md); **no se arreglan aqui**.

## 5. Contexto requerido

- Contrato, Discovery y decisiones de I-51: [contrato](I-51-rackduplicar-multiples-origenes.md),
  [Discovery](I-51-discovery.md) y [`decisions/I-51.md`](../automation/decisions/I-51.md).
- [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md), [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md)
  y [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md).
- Contratos de [I-10](I-10-kind-handlers.md) (restamp por handler) y [I-11](I-11-persistencia-uniforme.md)
  (campos desconocidos preservados).
- Contratos de I-49 e I-50 en su SHA remoto exacto, **solo lectura**.
- [AGENTS.md](../../AGENTS.md): capas, pruebas, guardas y reutilizacion de evidencia.
- Context Packs declarados en el frontmatter.

## 6. Dependencias

- **I-51 — integrada** (merge `46fcac2`): fundacion multi-rack nombrada en la apertura. No queda nada
  pendiente de ella.
- **Iniciativas paralelas a vigilar.** Al reclamar, `origin` tenia `main`,
  `architecture/motor-expresiones-parametricas` (**I-49**, `4df9480`) y
  `feature/cotas-independientes-por-vista` (**I-50**, `a4806ff`). `conflicts_with` queda **vacio** hasta
  que G1 mida el cruce real de archivos: no se declara un estorbo sin evidencia, ni se omite uno que la
  tenga.
- **Entrada del Owner requerida**: la decision copia/en sitio (seccion 4). Por eso
  `requires_owner_decision: true`.

## 7. Archivos esperados

**Este contrato no fija todavia la lista de archivos, y no debe fingir que si.** El mapa por archivo y
simbolo **es el entregable de G1**, y la lista vinculante la fija la Proposal acordada.

Lo que si se declara, como area y como limite: un comando nuevo del Plugin y, si el Discovery lo justifica,
piezas **puras** en Application para la transformacion y el ajuste semantico por sistema. **Una desviacion
material frente a esto obliga a detenerse** y a volver a la Proposal.

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** |
| G1 | Discovery | Informe archivo:simbolo y respuestas de la seccion 3, punto 2 | pendiente |
| G2 | Proposal V1 + revision de Arquitecto | Consenso Coordinador + Arquitecto sobre la misma Proposal | pendiente |
| G3+ | Implementacion, Candidato, validacion del Owner, integracion | Por definir **tras el consenso**, no aqui | bloqueada |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Se fijan **en la Proposal**, cuando exista alcance de codigo. Lo que ya es exigible por norma y no depende
de ella: las **dos suites** en local sobre el Candidato, **CI verde sobre el SHA exacto**, y builds Debug de
UI y de Plugin (AGENTS.md, «Pruebas — definicion de terminado»). G0 y G1 no producen codigo: su evidencia
es documental.

## 10. Validacion manual

**Requerida** en la implementacion: `RACKMIRROR` seria un comando interactivo de AutoCAD que cambia
dibujo (AGENTS.md, punto 5). El **checklist concreto lo fija la Proposal**. G0 y G1 **no** la requieren: no
tocan producto.

## 11. Criterios de aceptacion

Preliminares; **los fija la Proposal acordada**. Solo recogen lo enunciado en la apertura:

1. Una invocacion de `RACKMIRROR` acepta **uno o varios** racks.
2. El rack resultante **no se desespeja** tras guardar, reabrir, `RACKEDITAR`, Actualizar y redibujar.
3. Las propiedades dependientes de lado se ajustan en su **marco local**, nunca por el eje X del mundo.
4. `ProjectVariableReference(X) → X`: ningun vinculo se pierde, se materializa ni se resuelve, y no se crea
   ninguna variable.
5. No se buscan vistas hermanas automaticamente.

## 12. Condiciones para detenerse

- **COMPUERTA DE ESTA SESION — G0 y G1 solamente.** Ninguna linea de produccion.
- **SUBSTANTIVE IMPLEMENTATION: BLOCKED** hasta `Coordinator: AGREED` y `Architect: AGREED` sobre la misma
  Proposal.
- **Ambiguedad de producto** (copia/en sitio, significado de seleccionar solo algunas vistas, nombre del
  comando): la decide la cadena de gobierno, no el Discovery.
- **Decision arquitectonica material** (identidad, representacion de la transformacion, autoridad del
  ajuste semantico por sistema): se declara para revision de Arquitecto.
- **Archivos productivos compartidos materiales con I-49 o I-50**: reportarlo antes de continuar.
- **Cambio del formato persistido** o de la semantica de identidad del rack: detenerse; eso es ADR y
  decision del Owner.
- **Deriva hacia un framework general** de seleccion o transformacion: detenerse (seccion 4).

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-52.yml`: la automatizacion esta **desactivada** (`automation.enabled: false`) y
el precedente inmediato —I-51, tambien manual— no lo llevo en su bootstrap.
[WORKFLOW](../WORKFLOW.md) seccion 8 exige para el bootstrap del caso (d) exactamente **fila en ROADMAP mas
contrato**. El estado vivo se deriva, como manda la seccion 2, de la existencia de
`origin/feature/rackmirror-espejo-semantico`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5).

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-52 - ID16 - RACKMIRROR: espejo semantico de uno o varios racks
Rama       = feature/rackmirror-espejo-semantico
Worktree   = ~/.codex/worktrees/feature-rackmirror-espejo-semantico
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8  (origin/main)
CLAIM_SHA  = 55281769d5e9317ad25500e6d3f5f8a849f37279  (commit vacio)
Claim-Id   = 232b55c5-3d0c-47c7-90ae-a07caab06431
```

Primer `git push -u origin feature/rackmirror-espejo-semantico` **aceptado sin force** (`* [new branch]`),
con `origin` sin ninguna otra referencia a I-52 en el preflight. `main` **no fue modificada**.

El resto de la evidencia se acumula al cerrar cada fase.
