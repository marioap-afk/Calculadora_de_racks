---
schema: rackcad-initiative/v1
id: I-54
title: "ID24 — Custom Properties Foundation"
type: architecture
status: claimed
branch: architecture/propiedades-personalizadas
base_branch: main
priority:
size:
depends_on: []
conflicts_with: []
context_packs: [persistence, autocad-plugin, architecture-kernel, ui-editors]
automation_state_path:
decision_paths: [docs/automation/decisions/I-54.md]
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

# I-54 — ID24 — Custom Properties Foundation

> **Fase actual: G2-FREEZE COMPLETION — el Owner acepta ADR-0039 (“Acepto”) y el Consensus Freeze queda COMPLETO;
> el commit que lo versiona esta pendiente de CI.**
>
> - Discovery: [I-54-discovery.md](I-54-discovery.md) (G1 `195964b`, equivalente post-rebase `97e27ea`).
> - Proposal consensuada, inmutable: [I-54-proposal-v5.md](I-54-proposal-v5.md) @
>   `26ca923492576185b753d6dbf2a852969df2accf`. La [V1](I-54-proposal-v1.md), la [V2](I-54-proposal-v2.md), la
>   [V3](I-54-proposal-v3.md) y la [V4](I-54-proposal-v4.md) quedan como historia; el mapa de SHAs pre y post rebase
>   esta en la V5 §2.2, y el historial pre-rebase revisado queda preservado en el tag
>   `archive/i-54-custom-properties-pre-rebase-5d25da8`.
> - ADR: [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md), `aceptado` (nacio `propuesto` en
>   `d84f480`). Registro de decisiones: [decisions/I-54.md](../automation/decisions/I-54.md), §11 para la aceptacion.
> - Paquete de la ultima revision exact-SHA (G2J), limitado a AR-54-V4-01 / C-F3:
>   [I-54-architect-review-package.md](I-54-architect-review-package.md).
> - G2B cerrado: `Architect Review V1 = AGREED WITH CHANGES` (cambios V2-1..V2-22), incorporados en V2.
> - G2D cerrado: `Architect Review V2 = AGREED WITH CHANGES` (cambios C-1..C-8), incorporados en V3.
> - G2F cerrado: `Architect Review V3 = AGREED WITH CHANGES` sobre `5d25da8` (AR-54-V3-01 y AR-54-V3-02, MINOR),
>   incorporados en V4 como C-F1 y C-F2.
> - G2H cerrado: `Architect Review V4 = AGREED WITH CHANGES` sobre `8bc991c` (solo AR-54-V4-01, MINOR de
>   redaccion), con `Coordinator = AGREED`; el Coordinador acepta el cambio, incorporado en V5 como C-F3.
> - G2J cerrado: revision final exact-SHA de V5, limitada a AR-54-V4-01 / C-F3, con `Coordinator = AGREED` y
>   `Architect = AGREED` sobre `26ca923`, sin hallazgos nuevos ni cambios requeridos.
> - G2-FREEZE PREP hecho: `d84f480`, con ADR-0039 `propuesto`, F-01..F-14b en `ideas-futuras.md` y el tag de archivo;
>   CI 34748982744 en verde.
> - G2-FREEZE COMPLETION: el Owner acepta ADR-0039 de forma explicita, con la redaccion literal “Acepto”.
>
> ```text
> G2J                  = CLOSED
> Proposal V5          = 26ca923492576185b753d6dbf2a852969df2accf
> G2-FREEZE PREP       = d84f480f032f6a7e5e481359aeab9fd18f691fbf   (CI 34748982744 GREEN)
> Coordinator          = AGREED
> Architect            = AGREED
> Technical Consensus  = REACHED
> Owner ADR Acceptance = ACCEPTED   (literal: “Acepto”; 2026-09-13)
> ADR-0039             = ACCEPTED
> Consensus Freeze     = COMPLETE   (definido documentalmente en el commit de G2-FREEZE COMPLETION)
> Completion commit    = PENDING CI (el freeze documental queda validado solo con la CI verde de ese commit)
> Implementation       = BLOCKED    (hasta esa CI verde; despues, NOT STARTED)
> G3                   = NOT AUTHORIZED (tras esa CI verde, READY FOR COORDINATOR AUTHORIZATION)
> ```
>
> **No hay una sola linea de produccion escrita.** Hay que distinguir dos cosas. El freeze documental ya esta
> **definido**: consenso tecnico sobre V5, ADR-0039 aceptado y material de G2-FREEZE completo. El **commit** que lo
> versiona espera todavia su CI. Hasta esa CI verde, la implementacion sigue bloqueada y G3 no esta autorizado;
> despues, la orden de G3 la emite el Coordinador en una sesion posterior.
>
> La rama esta rebasada sobre `origin/main` @ `f8deb67` (WORKFLOW §4.2) desde G2G; en G2I, en G2-FREEZE y en su
> completion `main` no avanzo y no hubo rebase. Los veredictos pertenecen a sus SHAs y no se transfieren.

> **Apertura por autorizacion explicita del Owner sin fila previa**, transmitida por el Coordinador de
> I-54 — caso (d) de [WORKFLOW](../WORKFLOW.md) seccion 2. Esa autorizacion sustituye **unicamente** la
> preexistencia de la fila en ROADMAP: la fila durable y este contrato nacen en el bootstrap
> inmediatamente posterior al reclamo atomico. Hasta G2I **no existio** `docs/automation/decisions/I-54.md` —la
> norma **no lo exige** para el caso (d)— y `decision_paths` quedo vacio en vez de apuntar a un archivo inventado.
> G2-FREEZE lo crea para registrar el consenso tecnico y ADR-0039, y desde entonces `decision_paths` apunta a el.

```text
Initiative = I-54
Owner ID   = ID24 — Custom Properties Foundation
Branch     = architecture/propiedades-personalizadas
Worktree   = ~/.codex/worktrees/architecture-propiedades-personalizadas
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8   (origin/main, merge de I-51)
CLAIM_SHA  = 143490d8ecfb1c5f3011cf752c5cfcd11af13784   (Claim-Id d4b871e9-8a5d-4e67-bc11-a8f3c023788e)
             equivalente tras el rebase de G2G: c60c17fa46ef1eb2e9ab696432721d7649fe7c1d (mismo commit vacio y Claim-Id)
```

## 1. Objetivo

Fundar en RackCad **propiedades personalizadas**: metadatos con nombre y valor definidos por el usuario,
en dos alcances —**Proyecto** (el dibujo) y **Rack**—, persistidos en el DWG con identidad estable. La
fundacion **no** convierte esas propiedades en variables de proyecto ni en expresiones, y deja
**declarados, no implementados**, los puntos de extension hacia el dibujo, las expresiones y las
plantillas.

G0, G1 y G2 **no** entregan la fundacion. Entregan la **evidencia** (G1), la **primera propuesta** (Proposal V1),
su revision de Arquitecto (G2B), la **reconciliacion** (Proposal V2, G2C), la revision exact-SHA de V2 (G2D), la
**segunda reconciliacion** (Proposal V3, G2E), la revision exact-SHA de V3 (G2F), la reconciliacion tras el rebase
(Proposal V4, G2G), la revision exact-SHA de V4 (G2H) y la **reconciliacion final** (Proposal V5, G2I). Coordinador
y Arquitecto convergieron sobre V5 en G2J. G2-FREEZE versiono ese consenso con ADR-0039 `propuesto`, el Owner lo
acepto (“Acepto”) y G2-FREEZE COMPLETION completa el Consensus Freeze. Antes de escribir codigo faltan la CI verde de
ese commit y la orden de G3 del Coordinador.

## 2. Problema

Hoy RackCad persiste en el DWG dos clases de dato del usuario: el **sobre por rack**
(`RackEmbedDocument`, sobre la definicion de bloque; [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md))
y el **registro de variables de proyecto** (I-47,
[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md)). Que metadatos de proyecto o de rack
existen ya —nombre, cliente, area, ubicacion, revision, notas, descripcion, codigo o equivalentes—, donde
viven y con que semantica **no se afirma aqui**: es el primer entregable de G1.

El Coordinador entrego hallazgos **preliminares** que G1 debe **confirmar o corregir** contra el codigo, sin
darlos por ciertos:

| # | Hallazgo preliminar del Coordinador |
|---|---|
| H-1 | `origin/main` observado antes de la ejecucion: `46fcac2b071929d2bd5b07aa28373941417f74a8` (no asumido: verificado en el preflight del reclamo) |
| H-2 | `RACKCAD_PROJECT` parece ser hoy un **Xrecord directo** del NOD, no un sub-`DBDictionary` |
| H-3 | `RackEmbedDocument` parece la **costura comun natural** para metadatos de nivel rack |
| H-4 | I-51 parece **conservar los campos adicionales del sobre exterior** al re-estampar; debe probarse contractualmente |
| H-5 | La exportacion a biblioteca del Selectivo parece derivarse del **diseno interior**, y por tanto no exportaria metadatos situados solo en el sobre |
| H-6 | I-49 **no** debe convertirse en dependencia |
| H-7 | I-50 puede tocar DTOs del Selectivo; hay que **medir** el cruce actual |

## 3. Alcance

1. **G0 — reclamo y bootstrap**: reclamo atomico publicado, este contrato y la fila en ROADMAP.
2. **G1 — Discovery, solo documentacion.** Auditar por archivo y simbolo:
   - metadatos existentes: `ProjectName`, `Client`/`Cliente`, `Area`, `Location`/`Ubicacion`, `Revision`,
     `Notes`, `Description`, `Code` y equivalentes;
   - `RACKCAD_PROJECT`, `ProjectVariablesData`/`Registry`/`Document`/`Store` y la semantica
     ABSENT / PRESENT-BUT-UNREADABLE;
   - `RackEmbedDocument`, `RackEmbedStore`, `RackBlockData`, `RackId` y definicion frente a referencia;
   - autoridad entre vistas hermanas;
   - `RACKEDITAR` y los handlers por kind: como redibuja y como crea vistas cada sistema;
   - I-51: `RackDuplicationPlan`, `RackEnvelopeRestamp`, `RackDuplicarCommands`, `RackCloner`;
   - biblioteca, `.rackcad.json`, exportacion/importacion y el cruce real entre dibujos;
   - politicas de schema y version, y `JsonExtensionData`;
   - patrones reutilizables de UI y CRUD, en especial `RACKVARIABLES`;
   - serializadores y ayudantes reutilizables;
   - confirmar o corregir H-1..H-7.
3. **G2 — Proposal.** En esta sesion, solo la **V1**. Compara explicitamente:
   **A** `Dictionary<string,string>`, **B** registros con id estable y **C** documentos de propiedad tipados;
   y decide o propone: identidad; alcances Proyecto/Rack; solo texto frente a Texto/Numero/Booleano;
   persistencia de proyecto; persistencia de rack; autoridad entre hermanas; renombrar y borrar;
   semantica de duplicacion e independencia; biblioteca/exportacion; schema malformado o futuro; punto
   de extension futuro hacia el dibujo; punto de extension futuro hacia expresiones **sin implementar
   expresiones**; punto de extension futuro hacia plantillas; y la UI minima reutilizable.

   El Coordinador pidio **evaluar especialmente** —y **no aceptar por instruccion**— esta opcion:
   `CustomPropertyId + Name + string Value`; Proyecto = documento versionado **independiente** en el NOD,
   **sin** migrar `RACKCAD_PROJECT` salvo evidencia fuerte; Rack = campo **aditivo comun** en
   `RackEmbedDocument`, no en DTOs de producto; los mismos metadatos en **todas** las vistas hermanas del
   `RackId`; renombrar conserva el id; duplicar copia los valores en **colecciones independientes**; ni
   ProjectVariables, ni parser, ni formulas.
4. **Publicacion** en la rama: informe Discovery; Proposal V1; mapa de archivos y simbolos con los cruces
   con las ramas activas; decisiones materiales; riesgos; recomendacion de ADR si/no; y un **paquete
   autonomo** para la revision de Arquitecto.
5. **Implementacion**: sus gates son los de la Proposal V5 §14 (G3..G10), congelados por el consenso de G2J. **No
   esta autorizada**: la aceptacion de ADR-0039 ya consta (“Acepto”), pero faltan la CI verde del commit de G2-FREEZE
   COMPLETION y la orden de G3 del Coordinador.

## 4. Fuera de alcance

- **Cualquier cambio de produccion en G0, G1 y G2**: nada en `src/`, `tests/`, `assets/`, `eng/`,
  `deploy/` ni `.github/`. **G3+ no se inicia.**
- `docs/HANDOFF.md`: prohibido por la orden y por [WORKFLOW](../WORKFLOW.md) seccion 2.
- **La semantica de Project Variables** (I-47, ADR-0034) y de la **edicion vinculable** (I-48): una
  propiedad personalizada no es una variable de proyecto, y esta iniciativa no reabre esa semantica.
- **Parser, formulas y expresiones** (ID22B, territorio de I-49). **I-49 no es dependencia** de I-54.
- **Cotas por vista** (I-50) y **RACKMIRROR** (I-52).
- **Migrar `RACKCAD_PROJECT`** sin evidencia fuerte; y aun con ella, eso seria decision, no implementacion.
- **Dibujar** propiedades en el DWG (atributos, textos, cuadros de rotulacion) y **plantillas**: solo se
  declaran sus puntos de extension.
- Los hallazgos laterales se registran —en el Discovery y, al fijar el contrato, en
  [ideas-futuras.md](../ideas-futuras.md)—; **no se arreglan aqui**.

## 5. Contexto requerido

- [ARCHITECTURE.md](../ARCHITECTURE.md): identidad del rack y persistencia embebida.
- [ADR-0009](../adr/0009-identidad-guid-embebida-en-dwg.md), [ADR-0010](../adr/0010-actualizar-redibuja-insertar-liga-vistas.md)
  y [ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md).
- Contrato de [I-11](I-11-persistencia-uniforme.md) (campos desconocidos y version no degradada).
- Decisiones de [I-47](../automation/decisions/I-47.md), Proposal V8 de [I-48](I-48-proposal-v8.md) y
  contrato y decisiones de [I-51](I-51-rackduplicar-multiples-origenes.md).
- Contratos de I-49, I-50 e I-52 **en sus ramas remotas**, solo lectura y en el SHA exacto que se cite.
- [AGENTS.md](../../AGENTS.md): convenciones de persistencia versionada, pruebas y evidencia.
- Context Packs declarados en el frontmatter.

## 6. Dependencias

- **Sin dependencias pendientes**: I-47, I-48 e I-51 estan integradas.
- **I-49 no es dependencia**, por instruccion expresa del Coordinador.
- **Iniciativas paralelas a vigilar.** Al reclamar, `origin` tenia `main`, **I-49**
  (`architecture/motor-expresiones-parametricas`, solo documentacion), **I-50**
  (`feature/cotas-independientes-por-vista`, con produccion en DTOs y Domain de Selectivo y Dinamico) e
  **I-52** (`feature/rackmirror-espejo-semantico`, reclamada durante el preflight). `conflicts_with` queda
  **vacio** hasta que G1 mida el cruce real de archivos: no se declara un estorbo sin evidencia, ni se
  omite uno que la tenga.
- **Entrada del Owner**: la Proposal V1 listo nueve preguntas de producto (OQ-01..OQ-09), y con esa evidencia
  `requires_owner_decision` paso a `true`. La V2 las **reclasifica** (§16 de la V2), la V3 mantiene esa
  clasificacion con una aclaracion en OQ-02 (la cota de profundidad no es tuning) y **ninguna bloquea el
  consenso**:
  - OQ-08 queda resuelta;
  - OQ-05 pasa a riesgo residual documentado;
  - OQ-02 se divide: la existencia de limites es arquitectura y los numeros son tuning;
  - las demas se difieren, cada una con su gate limite (el nombre del comando, antes de G7).

  La aceptacion explicita de ADR-0039 ya consta (G2-FREEZE COMPLETION). Siguen haciendo falta decisiones del Owner con
  gate propio —el nombre del comando antes de G7—, asi que `requires_owner_decision` sigue en `true`.
- **Paralelas re-medidas en G2C** ([Discovery](I-54-discovery.md) §2.6; Proposal V2 §13):
  - I-49 @ `ccf21c6` e I-53 @ `f38362d`: solo documentacion;
  - I-50 @ `6cd2970`: G3 en las ventanas del Selectivo, Dinamico y Push Back, fuera del mapa de I-54;
  - I-52 @ `0fc7032`: Proposal V1 y ADR-0036 `propuesto`, solo documentacion. Su espejo compone con el sobre
    fuente y re-estampa, asi que cumple el invariante de preservacion (D-21 de la V2). Conflictos textuales:
    censo de comandos y censo de llamadas a `Compose`.

  Ningun supuesto de I-54 queda invalidado, y `conflicts_with` sigue vacio.
- **Paralelas re-medidas en G2E**, en el preflight y antes de publicar ([Discovery](I-54-discovery.md) §2.7;
  Proposal V3 §2 y §13). Estado al publicar:
  - `main` @ `f8deb67`: **I-50 integrada** durante la sesion (su rama remota se retiro). No cambia ningun archivo
    de codigo del mapa de I-54; `src/` mantiene los censos que vigila I-54, y `git merge-tree` no da conflictos.
    ADR-0035 queda `aceptado` en `main`;
  - I-49 @ `048a508`: Proposal V6, solo documentacion; sigue reservando `Rack`/`Project` para ID20;
  - I-52 @ `0445718`: Proposal V2 y ADR-0036 corregido, solo documentacion; sigue cumpliendo D-21;
  - I-53 @ `d7f17ad`: Proposal V2 y ADR-0037 `propuesto`, solo documentacion.

  Ningun avance invalida C-1..C-8 ni una costura de I-54, y `conflicts_with` sigue vacio. **Sin rebase en G2E**:
  al abrir la sesion `origin/main` seguia en `46fcac2`. WORKFLOW §4.2 exige rebasar al abrir la proxima sesion
  que escriba en la rama; como reescribe los SHAs revisados, su momento exacto (G2-FREEZE o una V4, y siempre
  antes de G3) lo fija la orden del Coordinador.
- **Rebase y paralelas en G2G** (Proposal V4 §2 y §13). Por orden del Coordinador, la sesion G2G rebaso la rama
  sobre `origin/main` @ `f8deb67` antes de escribir (WORKFLOW §4.2): 6/6 commits, sin conflictos, mapa de SHAs en
  la V4 §2.2 y V3 byte a byte identica a la revisada. Estado al publicar V4, medido solo sobre C-F1 y C-F2:
  - I-49 @ `1ed93a0`: rebasada sobre `main` con la misma Proposal V6, solo documentacion;
  - I-52 @ `545c222`: Proposal V3 y ADR-0036 actualizado, solo documentacion; su espejo reserializa el sobre, asi
    que F-14a y F-14b le afectan como a los demas flujos que lo reescriben;
  - I-53 @ `4e00a27`: G3 con tipos nuevos en `Application/Systems/Shared` y pruebas, sin tocar sobre, `Compose`,
    Custom Properties, NFC ni UI.

  Ningun avance invalida C-F1 ni C-F2, y `conflicts_with` sigue vacio.
- **Paralelas en G2I** (Proposal V5 §2.4 y §13), medidas solo sobre C-F3 / F-14b. `main` sigue en `f8deb67`, asi
  que no hubo rebase.
  - I-49 @ `364d6c0`: solo documentacion (ADR-0038 aceptado y consensus freeze de su V6); no toca el sobre.
  - I-52 @ `545c222`: sin cambios; su espejo reserializa el sobre y puede compartir F-14b, sin cambiar C-F3.
  - I-53 @ `4e00a27`: sin cambios.

  `conflicts_with` sigue vacio.
- **Paralelas en G2-FREEZE** ([decisions/I-54.md](../automation/decisions/I-54.md) §9). `main` sigue en `f8deb67`,
  asi que no hubo rebase y el SHA acordado es el de la rama.
  - I-49 @ `364d6c0`: sin cambios.
  - I-52 @ `8e2ae4f`: un commit nuevo, solo documentacion (Proposal V4 y ADR-0036 actualizado). ADR-0036 sigue
    `propuesto`, mantiene las propiedades de rack de I-54 como portador y su espejo sigue cumpliendo D-21.
  - I-53 @ `7de424e`: un commit nuevo, su G4 productivo, en `src/RackCad.Application/Systems/Selective/` y pruebas,
    fuera del mapa de I-54; no toca el sobre ni `docs/adr/`.
  - Censo de ADR: `main` llega a 0035; 0036 (I-52), 0037 (I-53) y 0038 (I-49) estan en sus ramas; 0039 estaba libre
    en todos los refs y se asigna a I-54.

  `conflicts_with` sigue vacio.
- **Paralelas en G2-FREEZE COMPLETION** ([decisions/I-54.md](../automation/decisions/I-54.md) §11). `main` sigue en
  `f8deb67`, sin rebase.
  - I-49 @ `82aa61b`: un commit nuevo, solo documentacion (su contrato e `ideas-futuras.md`); no toca `docs/adr/`. Es
    el cruce documental ya previsto al final de `ideas-futuras.md`.
  - I-52 @ `8e2ae4f` e I-53 @ `7de424e`: sin cambios.
  - 0039 sigue siendo exclusivo de I-54 en todos los refs.

  `conflicts_with` sigue vacio.
- **Cruce medido en G1** ([Discovery](I-54-discovery.md) §2 y §13): cruce **productivo** actual con I-49,
  I-50, I-52 e I-53 = **cero archivos** mientras I-54 no toque DTO ni Domain de sistema, editores de sistema,
  `RackEnvelopeRestamp` ni `RackCloner`. Cruce **documental** previsto con las cuatro (fila de ROADMAP tras I-51,
  final de `ideas-futuras.md`, numeracion de ADR) y **textual** con I-52 si ambas añaden comandos (censo de
  `[CommandMethod]` y ayuda). Cruce **semantico** con I-49: `Rack`/`Project` reservados para ID20. Por eso
  `conflicts_with` sigue vacio.

## 7. Archivos esperados

**Este contrato no fija la lista de archivos de produccion, y no debe fingir que si.** El mapa por archivo
y simbolo **es el entregable de G1**, y los archivos de produccion los fija el consenso de G2.

Lo que si se declara para G0..G2: solo `docs/initiatives/I-54-*.md`, y la fila de I-54 en `docs/ROADMAP.md` solo
en el bootstrap. G2C, G2E, G2G y G2I tocan **unicamente** `docs/initiatives/I-54-*.md`; el rebase de G2G solo
reaplica los commits existentes de I-54. G2-FREEZE toca ademas, conforme a la Proposal V5 §14 y a la orden del
Coordinador, `docs/adr/0039-custom-properties-persistencia-autoridad.md` (nuevo), `docs/adr/README.md` (fila del
indice), `docs/automation/decisions/I-54.md` (nuevo) y `docs/ideas-futuras.md` (F-01..F-13, F-14a, F-14b y la mejora
de D-11.6); no toca `docs/ROADMAP.md` ni `docs/HANDOFF.md`. G2-FREEZE COMPLETION toca solo ADR-0039 (encabezado y
bloque «Aceptación del Owner»), `docs/adr/README.md`, `docs/automation/decisions/I-54.md` y este contrato. **Una
desviacion material frente a esto obliga a detenerse.**

## 8. Fases

| # | Fase | Entregable | Estado |
|---|---|---|---|
| G0 | Reclamo + bootstrap | Reclamo atomico publicado, contrato y fila en ROADMAP | **HECHA** — bootstrap `f908b2f` |
| G1 | Discovery | [I-54-discovery.md](I-54-discovery.md): informe por archivo/simbolo, H-1..H-8, mapa de cruces, riesgos y hallazgos fuera de alcance | **HECHA** — `195964b` |
| G2 | Proposal y consenso | **G2A**: [Proposal V1](I-54-proposal-v1.md) y paquete de Arquitecto. **G2B**: Architect Review de V1. **G2C**: [Proposal V2](I-54-proposal-v2.md) y paquete reescrito. **G2D**: revision exact-SHA de V2. **G2E**: [Proposal V3](I-54-proposal-v3.md) y paquete reescrito. **G2F**: revision exact-SHA de V3, limitada a C-1..C-8. **G2G**: rebase obligatorio sobre `origin/main` y [Proposal V4](I-54-proposal-v4.md) con paquete reescrito para la revision exact-SHA (su SHA es el del commit que introduce la V4). **G2H**: revision exact-SHA post-rebase de V4, limitada a C-F1, C-F2 y la integridad del rebase. **G2I**: [Proposal V5](I-54-proposal-v5.md) (C-F3) y paquete reescrito para la revision final exact-SHA (su SHA es el del commit que introduce la V5). **G2J**: revision final exact-SHA de V5 por Coordinador y Arquitecto, limitada a AR-54-V4-01 / C-F3. **G2-FREEZE**: consenso, ADR `propuesto` y aprobacion del Owner. Cada una con orden propia. SHAs pre-rebase y sus equivalentes en la V5 §2.2 | **G2A HECHA** (`7c197af`); **G2B CERRADA** (AGREED WITH CHANGES sobre `7c197af`); **G2C HECHA** (`36c337b`); **G2D CERRADA** (AGREED WITH CHANGES sobre `36c337b`); **G2E HECHA** (`5d25da8`); **G2F CERRADA** (AGREED WITH CHANGES sobre `5d25da8`); **G2G HECHA** (`8bc991c`); **G2H CERRADA** (AGREED WITH CHANGES sobre `8bc991c`); **G2I HECHA** (`26ca923`); **G2J CERRADA** (Coordinator AGREED y Architect AGREED sobre `26ca923`); **G2-FREEZE PREP HECHA** (`d84f480`, CI 34748982744 verde); **G2-FREEZE COMPLETION**: ADR-0039 aceptado por el Owner (“Acepto”), Consensus Freeze COMPLETE, commit pendiente de CI |
| G3+ | Implementacion, Candidato, validacion del Owner, integracion | Gates G3..G10 de la [Proposal V5](I-54-proposal-v5.md) §14, congelados por el consenso de G2J | **bloqueada**: G3 NOT AUTHORIZED hasta la CI verde del commit de G2-FREEZE COMPLETION; despues, READY FOR COORDINATOR AUTHORIZATION |

Ninguna fase posterior arranca sin que la anterior tenga evidencia revisable.

## 9. Pruebas y builds

Las fija la Proposal V5 §12, congelada por el consenso de G2J, y se ejecutan a partir de G3, que no esta
autorizado. Lo que ya es exigible por norma y no depende de G2: las **dos suites** en local sobre el Candidato,
**CI verde sobre el SHA exacto**, y builds Debug de UI y de Plugin (AGENTS.md, «Pruebas — definicion de
terminado»). G0, G1 y G2 no producen codigo: su evidencia es documental.

## 10. Validacion manual

**Requerida** en la implementacion: persistir datos del usuario en el DWG y hacerlos sobrevivir a
`RACKEDITAR`, a la duplicacion y a guardar/reabrir cambia el comportamiento del dibujo (AGENTS.md,
punto 5). El **checklist concreto** es OV-01..OV-14 de la Proposal V5 §12.8, en G9. G0, G1 y G2 **no** la
requieren: no tocan producto.

## 11. Criterios de aceptacion

Redaccion preliminar, anterior a G2. El consenso de G2J los concreta en la Proposal V5 (objetivos §4, invariantes
§7, y pruebas y validacion §12) y en ADR-0039, aceptado. Solo recogen lo que ya es doctrina del repositorio o
limite expreso de la orden:

1. Las propiedades de ambos alcances sobreviven a guardar, cerrar y reabrir el mismo DWG.
2. Un contenedor presente pero ilegible, o de un schema futuro, **no** se trata como vacio
   (`UNKNOWN/UNREADABLE != ABSENT`, ADR-0034).
3. Ninguna propiedad personalizada se lee, escribe ni resuelve como variable de proyecto, expresion o
   formula.
4. El formato persistido existente sigue abriendose, y un binario anterior no destruye en silencio lo que
   no entiende, en la medida que fije la Proposal.

## 12. Condiciones para detenerse

- **COMPUERTA VIGENTE — G2-FREEZE COMPLETION, solo documentacion.** Registra la aceptacion del Owner, cambia
  ADR-0039 a `aceptado` y completa el Consensus Freeze. Sin produccion, sin pruebas productivas, sin tag nuevo y sin
  tocar `docs/ideas-futuras.md`, `docs/HANDOFF.md` ni `docs/ROADMAP.md`. G3+ no se inicia. (La preparacion del
  freeze quedo en `d84f480`: ADR `propuesto`, F-01..F-13, F-14a, F-14b y la mejora de D-11.6, y tag `archive/*`.)
- **Implementacion bloqueada** hasta que se cumplan todas estas condiciones:
  - `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** Proposal: **cumplida** en G2J, sobre `26ca923`;
  - aceptacion **explicita** de ADR-0039 por el Owner: **cumplida** (“Acepto”, 2026-09-13);
  - CI verde del commit de G2-FREEZE COMPLETION: **pendiente**;
  - la orden de G3 del Coordinador, en una sesion posterior.
- Si G1 encuentra **archivos productivos compartidos materiales con I-49, I-50 o I-52**: reportarlo antes
  de continuar.
- Si la Proposal exige cambiar el **formato persistido existente** (major de `RackEmbedDocument`, forma de
  `RACKCAD_PROJECT`) o la semantica de identidad del rack: **ADR y decision del Owner antes de implementar**.
- Si el alcance deriva hacia Project Variables, expresiones, dibujo de propiedades o plantillas:
  **detenerse** (seccion 4).
- `docs/HANDOFF.md` **no se toca**.

## 13. Estado versionado y entrega del Pull Request

Sin `docs/automation/state/I-54.yml`: la automatizacion esta **desactivada** (`automation.enabled:
false`) y el caso (d) no lo exige. [WORKFLOW](../WORKFLOW.md) seccion 8 pide para el bootstrap del caso (d)
exactamente **fila en ROADMAP mas contrato**. El estado vivo se deriva, como manda la seccion 2, de la
existencia de `origin/architecture/propiedades-personalizadas`.

No hay Pull Request. El merge automatico esta prohibido; la integracion es manual (WORKFLOW 4.5).

## 14. Evidencia final

**Reclamo atomico (G0).**

```text
Iniciativa = I-54 - ID24 - Custom Properties Foundation
Rama       = architecture/propiedades-personalizadas
Worktree   = ~/.codex/worktrees/architecture-propiedades-personalizadas
BASE_SHA   = 46fcac2b071929d2bd5b07aa28373941417f74a8  (origin/main)
CLAIM_SHA  = 143490d8ecfb1c5f3011cf752c5cfcd11af13784  (commit vacio)
Claim-Id   = d4b871e9-8a5d-4e67-bc11-a8f3c023788e
```

Primer `git push -u origin architecture/propiedades-personalizadas` **aceptado sin force**
(`* [new branch]`), con `origin` sin ninguna referencia a I-54 ni a ID24 en el preflight. `main` **no fue
modificada**.

**G1 — Discovery.** Solo documentacion, sobre el codigo de `BASE_SHA` (la rama no difiere en `src/`,
`tests/` ni `assets/`). Preflight de G1: `origin/main` sin mover; I-49 y I-52/I-53 solo documentacion; I-50
con produccion que **no** toca sobre, compositor, restamp, cloner ni autoridad. Sin compilacion ni pruebas:
G1 no produce codigo. Los hallazgos fuera de alcance (F-01..F-12 en G1; F-13 añadido en G2A) quedan en el Discovery y pasan a
`ideas-futuras.md` cuando se fije el contrato.

**G2A — Proposal V1.** Solo documentacion. Compara A/B/C, evalua componente a componente la opcion del
Coordinador sin aceptarla por instruccion, propone D-01..D-20, INV-01..INV-16, gates G2B..G10 no autorizados,
pruebas T/U, validacion OV-01..OV-12 y preguntas OQ-01..OQ-09, y recomienda ADR antes de implementar. El
paquete autonomo de Arquitecto lista RD-01..RD-20 con evidencia verificable. Addendum al Discovery (§2.5:
re-medicion de paralelas antes de publicar —I-50 @ `8ceb3a7`, I-52 @ `339b3ab`, I-53 @ `c8476cc`, sin cambio de
cruce—; §11.5: lista verificada de comandos; §15: F-13, troceado de Xrecord duplicado). Sin compilacion ni pruebas.

**G2B — Architect Review de V1** (sobre `7c197af81b91df88366c873eddf9e11ddc5e87bb`): `GLOBAL VERDICT = AGREED WITH
CHANGES`, `G2B = CLOSED`, implementacion bloqueada. Resultado: 0 BLOCKER, 5 MATERIAL (AR-54-01..05), 13 MINOR
(AR-54-06..18) y 22 cambios vinculantes (V2-1..V2-22). RD-13 salio DISAGREE y las demas AGREE WITH CHANGE. La
revision no modifico el repositorio.

**G2C — Proposal V2.** Solo documentacion.

- **Nuevo**: [I-54-proposal-v2.md](I-54-proposal-v2.md), que incorpora V2-1..V2-22 con matriz de reconciliacion
  (§17) y traza AR-54-01..18 (§18, los cinco MATERIAL en `CLOSED` a la espera de re-verificacion). Tambien:
  - reescribe RD-01..RD-20 (§19: las veinte `ACCEPTED WITH RECONCILIATION`; RD-13 reformulada);
  - reclasifica OQ-01..OQ-09 (§16);
  - define el alcance candidato del ADR sin redactarlo (§15);
  - reconcilia las pruebas T, U y OV (§12) y los gates (§14);
  - declara trece precisiones del ejecutor (PR-01..PR-13, §21), con `OPEN DISAGREEMENT = NINGUNO`.
- **Reescrito**: el paquete de revision, que ahora apunta solo a V2.
- **Addendum**: Discovery §2.6 (re-medicion de paralelas).
- **Contrato**: este archivo, solo en lo que refleja G2C.

Nueva evidencia ejecutada [X] en una sonda de `System.Text.Json` fuera del repositorio. Sin compilacion ni pruebas
del repositorio: G2C no produce codigo.

**G2D — Architect Review exact-SHA de V2** (sobre `36c337b84c47f9ac7c97d860fce42d3a5f4370e8`): `GLOBAL VERDICT =
AGREED WITH CHANGES`, `G2D Architect Review = CLOSED`, implementacion bloqueada.

- AR-54-01, -02, -03 y -05 `CLOSED`; AR-54-04 `REOPENED` por la restriccion B (cota de profundidad).
- Hallazgos nuevos: 2 MATERIAL (AR-54-V2-01 kind desconocido escribible; AR-54-V2-02 cota fuera del ADR y
  contradicciones de edicion) y 4 MINOR (AR-54-V2-03..06: UTF-16 y excepciones, nombres repetidos anidados,
  huecos de la unificacion, `Id` textual).
- Ocho cambios vinculantes, C-1..C-8, que el Coordinador acepta.
- PR-03, -06, -07 y -13 con cambio; PR-12 en desacuerdo; las demas de acuerdo.
- RD-01, -03, -04, -07, -10, -13, -19 y -20 AGREE WITH CHANGE; las demas AGREE.
- `Kind` en blanco = opcion A; ADR requerido con cambios de alcance; ningun bloqueo del Owner.

La revision no modifico el repositorio.

**G2E — Proposal V3.** Solo documentacion.

- **Nuevo**: [I-54-proposal-v3.md](I-54-proposal-v3.md), que es V2 con exactamente C-1..C-8:
  - `UnknownKind` de solo lectura con `isKnownKind` inyectado (PR-12 rechazada);
  - cota 16 como constante del formato 1.x con `DepthLimitExceeded`;
  - clases de excepcion explicitas y UTF-16 validado antes de NFC y de serializar, con el residual preexistente
    del sobre declarado (F-14 para el freeze);
  - nombres repetidos a cualquier profundidad;
  - origen `Absent` canonico, revalidacion de todos los miembros y atomicidad logica al unificar;
  - `Id` igual por valor;
  - alcance del ADR en 16 puntos;
  - orden unico de ocho resultados de autoridad.

  Trazas en §17..§19 y §21: C-1..C-8 `INCORPORATED`, AR-54-V2-01..06 y AR-54-04 `CLOSED IN V3` pendientes de
  re-verificacion, `OPEN DISAGREEMENT = NINGUNO`.
- **Reescrito**: el paquete de revision, limitado a C-1..C-8.
- **Addendum**: Discovery §2.7 (re-medicion de paralelas).
- **Contrato**: este archivo, solo en lo que refleja G2D y G2E.

Las sondas de `System.Text.Json` que sostienen C-2..C-4 se re-ejecutaron fuera del repositorio sobre .NET 8.0.29.
Durante la sesion `origin/main` avanzo a `f8deb67` (integracion de I-50). V3 se publica sin rebase y sin force,
con la medicion de §6. Sin compilacion ni pruebas del repositorio: G2E no produce codigo.

**G2F — Architect Review exact-SHA de V3** (sobre `5d25da89972df2468f1d03243301761f5463e6eb`, CI 34737707481
verde): `Coordinator = AGREED`; `GLOBAL VERDICT = AGREED WITH CHANGES`, `G2F Architect Review = CLOSED`,
implementacion bloqueada.

- C-1, C-2 y C-4..C-8 `VERIFIED`; C-3 `DEFECT`.
- Dos hallazgos MINOR con evidencia ejecutada:
  - **AR-54-V3-01**: `Normalize(FormC)` lanza con UTF-16 bien formado que contiene no-caracteres;
  - **AR-54-V3-02**: un sobre con un surrogate escapado se lee, pero toda reescritura lanza.
- AR-54-04, AR-54-V2-01, -02 y -04..-06 `CLOSED`; AR-54-V2-03 `REOPENED` solo por esos dos puntos.
- PR-03, -06, -07 y -13 `AGREED`; PR-12 `RECONCILED`.
- RD-04 y RD-07 AGREE WITH CHANGE; RD-01, -03, -10, -13, -19 y -20 AGREE.
- `AUTHORITY ORDER = AGREED`; `ADR REQUIRED = YES`; `ADR SCOPE = AGREED`.
- `main` @ `f8deb67` sin impacto sobre C-1..C-8.

El Coordinador acepta la lista cerrada de cambios. La revision no modifico el repositorio.

**G2G — Rebase y Proposal V4.** Solo documentacion.

- **Rebase** sobre `origin/main` @ `f8deb675c6d1ef0e64693b157d69c4cc170d7b24` (WORKFLOW §4.2): 6/6 commits, sin
  conflictos. `git range-diff` da cinco `=` y el bootstrap `!` solo por contexto de `ROADMAP.md`. El V3 rebasado
  (`ff98b9ee95ea7c4f8da0b90fe949b8ebb42157ed`) es byte a byte el revisado en G2F. Mapa de SHAs en la V4 §2.2;
  los historicos no se sustituyen.
- **Nuevo**: [I-54-proposal-v4.md](I-54-proposal-v4.md), que es la V3 rebasada con exactamente:
  - **C-F1**: un `Name` con no-caracteres es invalido al escribir y `PresentButUnreadable` al leer, antes de NFC;
    `Value` sin cambios;
  - **C-F2**: residual F-14b declarado junto a F-14a; INV-07, D-13 y RP-11 acotados a sobres que BASE puede leer y
    reserializar; sin commit si una serializacion lanza; caracterizacion en T-CHR-04 y T-ENV-13.

  AR-54-V3-01, AR-54-V3-02 y AR-54-V2-03 quedan `CLOSED IN V4`, pendientes de re-verificacion; RD-04 y RD-07
  `ACCEPTED WITH FINAL RECONCILIATION`; §15 identica a V3.
- **Reescrito**: el paquete de revision, limitado a C-F1, C-F2 y la integridad del rebase.
- **Contrato**: este archivo, solo en lo que refleja G2F y G2G. El Discovery no cambia.

F-14a y F-14b se registran en `ideas-futuras.md` en G2-FREEZE, como reserva §4. La historia rebasada se publica
con `git push --force-with-lease` (WORKFLOW §4.3). Sin compilacion ni pruebas del repositorio: G2G no produce
codigo.

**G2H — Architect Review exact-SHA de V4** (sobre `8bc991c0e1854bc9eb4013421e01f0515c2e77ca`, CI 34742026072
verde): `Coordinator = AGREED`; `GLOBAL VERDICT = AGREED WITH CHANGES`, `G2H Architect Review = CLOSED`,
implementacion bloqueada.

- `REBASE INTEGRITY = VERIFIED`: V3 byte a byte identica, mapa de SHAs correcto y nada productivo.
- `C-F1 = VERIFIED` (AR-54-V3-01 y AR-54-V2-03 3B `CLOSED`; captura defensiva de `ArgumentException` `ACCEPTED`).
- `C-F2 = DEFECT`, solo por redaccion: AR-54-V3-02 y AR-54-V2-03 3C `CLOSED`;
  `ATOMIC SERIALIZATION FAILURE = VERIFIED`.
- `TRACEABILITY = VERIFIED`; RD-04 AGREE; RD-07 AGREE WITH CHANGE.
- Hallazgo nuevo **AR-54-V4-01** (MINOR): D-08.4, D-13 y RP-11 decian que los consumidores existentes «fallan sin
  escribir» ante F-14b. En realidad `RACKEDITAR` confirma una transaccion por vista y puede quedar parcialmente
  actualizado.
- Paralelas sin impacto material.

El Coordinador acepta el cambio. La revision no modifico el repositorio.

**G2I — Proposal V5.** Solo documentacion, sin rebase (`main` no avanzo).

- **Nuevo**: [I-54-proposal-v5.md](I-54-proposal-v5.md), que es V4 con exactamente **C-F3** (AR-54-V4-01):
  - F-14a y F-14b quedan separados;
  - F-14b describe que falla la reserializacion del sobre afectado, que el punto de fallo depende de la granularidad
    de cada consumidor existente y que `RACKEDITAR` puede quedar parcialmente actualizado (P-37, evidencia de codigo);
  - la atomicidad de D-22.10 queda como exclusiva del ejecutor de I-54;
  - D-13 separa la garantia de I-54 (A) de los residuales (B), y RP-11 se alinea.

  D-22.10, INV-07, las pruebas y §15 no cambian. AR-54-V4-01 queda `CLOSED IN V5`, pendiente de G2J; RD-04 `AGREED`;
  RD-07 `ACCEPTED WITH FINAL RECONCILIATION`, pendiente solo de confirmar la redaccion.
- **Reescrito**: el paquete de revision, limitado a AR-54-V4-01 / C-F3.
- **Contrato**: este archivo, solo en lo que refleja G2H y G2I. El Discovery y `ideas-futuras.md` no cambian.

Sin compilacion ni pruebas del repositorio: G2I no produce codigo.

**G2J — Revision final exact-SHA de V5** (sobre `26ca923492576185b753d6dbf2a852969df2accf`, CI 34747220760 verde):
`Coordinator = AGREED`; `GLOBAL VERDICT = AGREED`, `G2J Architect Review = CLOSED`.

- Q-1..Q-6 `VERIFIED`: descripcion de F-14b; ninguna afirmacion global de atomicidad para los consumidores
  existentes; garantia de D-22.10 intacta y exclusiva del ejecutor de I-54; D-13 y RP-11 alineados; redaccion futura
  de F-14 lista; integridad del alcance.
- AR-54-V4-01, AR-54-V3-01, AR-54-V3-02, AR-54-V2-03 y AR-54-04 `CLOSED`; RD-04 y RD-07 AGREE; `ADR REQUIRED = YES`;
  `ADR SCOPE = AGREED`; `Authority order = AGREED`.
- `New findings = NONE`; `Required Proposal changes = NONE`; paralelas sin impacto material sobre F-14b.
- `Consensus = READY FOR FREEZE`; implementacion bloqueada.

La revision no modifico el repositorio.

**G2-FREEZE — preparacion documental del freeze.** Solo documentacion, sin rebase: `main` no avanzo.

- **Nuevo**: [ADR-0039](../adr/0039-custom-properties-persistencia-autoridad.md) `propuesto`, que congela los
  dieciseis puntos de la V5 §15 sin reabrir decisiones y deja la aceptacion del Owner en `PENDING`.
- **Nuevo**: [decisions/I-54.md](../automation/decisions/I-54.md), con el consenso, el estado del freeze y la
  informacion para el Owner sobre R-12, F-14a y F-14b.
- **Indice de ADR**: fila 0039 `propuesto`; 0036..0038 siguen en sus ramas.
- **`ideas-futuras.md`**: F-01..F-13, con F-01 y F-06 enlazados a registros existentes; F-14a; F-14b con la
  redaccion de C-F3; y la mejora de D-11.6.
- **Tag** `archive/i-54-custom-properties-pre-rebase-5d25da8` → `5d25da8` (Proposal V3 pre-rebase, revisada en G2F),
  anotado y publicado en `origin`.
- **Contrato**: este archivo, con `decision_paths` y el estado del freeze.

La Proposal V5, V1..V4, el Discovery, el paquete, `ROADMAP.md` y `HANDOFF.md` no cambian. Sin compilacion ni pruebas
del repositorio: G2-FREEZE no produce codigo. El SHA de este commit y su CI se reportan al Coordinador.

**G2-FREEZE COMPLETION — aceptacion del Owner y Consensus Freeze completo.** Solo documentacion, sin rebase: `main` no
avanzo.

- **Preparacion verificada**: `d84f480f032f6a7e5e481359aeab9fd18f691fbf`, CI 34748982744 `success` con los cuatro
  jobs en verde.
- **Owner**: acepta ADR-0039 de forma explicita. Redaccion literal “Acepto” (2026-09-13), transmitida por el
  Coordinador con un contexto inequivoco: ADR-0039, I-54 y Proposal V5 @ `26ca923492576185b753d6dbf2a852969df2accf`.
- **ADR-0039** pasa a `aceptado`. Solo cambian su encabezado y su bloque «Aceptación del Owner»; desde «Contexto» hasta
  el final es byte a byte identico al de `d84f480`.
- **Indice de ADR**: fila 0039 `aceptado`.
- **[decisions/I-54.md](../automation/decisions/I-54.md) §11**: el acto, la secuencia, la matriz final de G2-FREEZE y
  las verificaciones. Deja `FREEZE_COMPLETION_SHA` y su CI pendientes, porque un commit no contiene su propio SHA.
- **Contrato**: este archivo.

Verificado sin cambios:
- el tag `archive/i-54-custom-properties-pre-rebase-5d25da8` apunta a `5d25da8` en local y en `origin`, y no se crea
  otro;
- F-14a y F-14b siguen en `ideas-futuras.md`, una sola vez cada uno y con la redaccion acordada;
- la Proposal V5, el Discovery, el paquete, `ideas-futuras.md`, `ROADMAP.md` y `HANDOFF.md` no cambian.

```text
Technical Consensus = REACHED
ADR-0039            = ACCEPTED
Consensus Freeze    = COMPLETE
Implementation      = BLOCKED hasta la CI verde de este commit; despues, NOT STARTED
G3                  = NOT AUTHORIZED hasta esa CI verde; despues, READY FOR COORDINATOR AUTHORIZATION
```

Sin compilacion ni pruebas del repositorio: G2-FREEZE COMPLETION no produce codigo. El SHA de este commit y su CI se
reportan al Coordinador.

El resto de la evidencia se acumula al cerrar cada fase.
