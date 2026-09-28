# I-52 AUTH-15 — Registro de decisiones de la unidad de integracion

Unit: `I-52-AUTH15` (iniciativa I-52). Rama: `feature/i52-auth15-definition-creator`. Workflow: V2.

Contrato de la unidad: [I-52-auth15-definition-creator.md](../../initiatives/I-52-auth15-definition-creator.md).

## 1. Autorizacion del Coordinador

```text
Coordinator = AUTH-15 IMPLEMENTATION AUTHORIZED
Integration unit = feature/i52-auth15-definition-creator
Base = 016bf46715cec45e644f88a22ef091b311a2bef1
§148/§156 production exclusion = LIFTED FOR AUTH-15 ONLY
AUTH15-DEV-01 = BlockNameUnavailable omitted from implementation because the existing
                family naming loops expose no reachable/distinguishable exhaustion result.
                ARCHITECT REVIEW REQUIRED.
```

- **Segunda unidad de I-52.** El Coordinador autoriza expresamente esta rama como segunda unidad de integracion
  de I-52, de proposito unico. No contradice la regla «1 iniciativa = 1 rama»:
  - su funcion es aislar AUTH-15 de la rama de investigacion CT-DA de I-52, que esta bloqueada;
  - nace de `origin/main`;
  - solo implementa e integra AUTH-15 y no absorbe otro trabajo de I-52.
- **Exclusion levantada solo para AUTH-15.** La exclusion de codigo de produccion de §148 y §156 del registro
  de I-52 queda levantada unicamente para AUTH-15. El comando RACKMIRROR y el resto del producto de I-52 siguen
  BLOQUEADOS en su propia rama.

## 2. Reclamo y clasificacion Workflow

- **Reclamo.** Commit vacio `4b20bda3`, aceptado en el primer push sin force, con
  `Claim-Id: 1f035b6f-6891-48fd-8965-f7ff1f522767`.
- **Clasificacion: V2, caso T4.** La base contiene el `WORKFLOW_V2_EFFECTIVE_SHA` y no hay pausa activa.
  - La pausa de activacion termino segun el registro durable del tag `integration/I-56`
    (`Claim pause: end=2026-09-17T22:30:00Z`), que es el registro que exige WORKFLOW §11.7.
  - La linea de `docs/HANDOFF.md` en `main` que aun dice «pausa ACTIVE» esta desfasada respecto de ese tag. No
    se corrige aqui (ver §3).
- **Orden.** El codigo de la unidad se redacto en local antes del reclamo, por orden previa del Coordinador. En
  el historial versionado el orden es el de WORKFLOW §2: reclamo, bootstrap (contrato, este registro y la fila
  de ROADMAP) y despues la implementacion.

## 3. HANDOFF

La orden pedia una entrada en decisiones y HANDOFF. **`docs/HANDOFF.md` no se toca en esta sesion**: WORKFLOW
lo reserva a la sesion de integracion, como ultimo commit de la rama (tabla de archivos calientes y de
registro). El registro durable de la unidad es este archivo, junto con el contrato y la fila de ROADMAP del
bootstrap (momento 2 de WORKFLOW §2). HANDOFF se actualizara en el commit de cierre de la integracion.

## 4. Contrato implementado y desviacion AUTH15-DEV-01

- **Contrato.** Se implemento el de la revision de Arquitecto «AUTH-15 DESIGN / FREEZE REVIEW», resumido en el
  contrato de la unidad.
- **Superficie.** Dos altas en el Plugin (`RackDefinitionCreator`, `RackDefinitionCreationResult`) y guardas de
  fuente. No se modifica ningun archivo de produccion existente.
- **AUTH15-DEV-01.** La revision listaba el fallo tipado `BlockNameUnavailable`, y la implementacion lo omite.
  - **Por que.** Ninguno de los dos creadores de familia expone un agotamiento de nombres alcanzable y
    distinguible:
    - el bucle de `LateralHeaderDrawer.UniqueBlockName` no tiene cota;
    - el de `CantileverViewMaterializer.UniqueBlockName` solo lanza `InvalidOperationException` tras
      `int.MaxValue` candidatos, indistinguible de cualquier otra `InvalidOperationException`.
  - **Que pasaria si ocurriera.** Saldria como `WriteFailed` con su diagnostico.
  - **Estado.** Por orden del Coordinador, no se restaura ni se cambia el comportamiento de produccion para
    resolverla. **ARCHITECT REVIEW REQUIRED.**

## 5. Estado

```text
IMPLEMENTATION = AUTHORIZED; implemented on this branch
HOST_VALIDATION = NOT RUN (test vehicle not yet decided; no temporary AutoCAD command in this gate)
AUTH15_DEV_01 = ARCHITECT REVIEW REQUIRED
NEXT_GATE = ARCHITECT EXACT-SHA AUTH-15 IMPLEMENTATION REVIEW
```

Los conteos de pruebas, los SHA y el CI exacto viven en el cuerpo de los commits y, al cierre, en el archivo de
evidencia de la unidad (WORKFLOW: tabla de registro, unidades V2). No se copian aqui.
