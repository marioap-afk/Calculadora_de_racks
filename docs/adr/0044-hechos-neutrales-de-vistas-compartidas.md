# ADR-0044: Hechos neutrales de vistas compartidas y consumo desde main

- **Estado:** **aceptado**
- **Fecha:** 2026-09-15
- **Decisor:** Owner del repositorio
- **Iniciativa:** I-57 — Shared View Foundation
- **Proposal:** `docs/initiatives/I-57-proposal-v4.md`
- **Reconciliacion:** `docs/initiatives/I-57-reconciliation-r3.md` · blob `cd42db03becff42f98b047e61c46689c17a69670`

## Contexto

I-52 e I-55 necesitan las mismas direcciones, lectura sintactica, disponibilidad, frames, seleccion,
transformaciones, resolucion, preparacion, nombres, requisitos de bloque y autoridad authored. Duplicarlos crea
dos autoridades; consumir una rama paralela contradice el modelo de integracion. Ya existen builders, handlers,
`DimensionViewKind` y `Transform2D` maduros que no deben reemplazarse por simetria.

## Decision

1. I-57 es titular estable de AUTH-01..14 segun su Proposal y reconciliacion.
2. Foundation expresa hechos/contratos puros en Application; AutoCAD permanece en adapters Plugin.
3. Se reutilizan autoridades existentes y se rodean con ports/adapters cuando moverlas romperia capas o semantica.
4. Sintaxis, disponibilidad y policy son capas distintas; Foundation gobierna las dos primeras.
5. Un frame fisico entrega spans y centro del mismo intervalo; el consumidor elige el punto.
6. Resolve conserva autoridad por kind. Selectivo se resuelve efectivamente una vez dentro del handler, conforme
   ADR-0034.
7. Solo se consume un Integration SHA alcanzable desde `origin/main`.
8. Caracterizaciones preceden extraccion; Unknown/Unreadable nunca se vuelve ausencia/exito.

Quedan fuera comandos/UX ID17-19, policy RACKMIRROR, OD, cola/materializacion, reflexion, AUTH-15, gates temporales
y persistencia nueva.

Consecuencias: I-52/I-55 esperan integracion neutral; payloads y builders se preservan; Plugin conserva scans,
BlockTable, importacion y transacciones; nuevos kinds requieren adapter+fixtures; implementacion exige Owner
Validation por cambiar rutas de dibujo.

Alternativas descartadas: extraer en una rama de producto, duplicar contratos, reemplazar builders con plan
universal, mover handlers a Application o incluir policy de producto.

## Aceptacion

El Owner acepto explicitamente este ADR el 2026-09-15 sobre Proposal V4 exacta
`2a142f224fb8d8a0c16bd7f7334dc48be3be9eef` y R3 exacta, blob
`cd42db03becff42f98b047e61c46689c17a69670`. Coordinator y Architect estan `AGREED`; Technical Consensus esta
`REACHED`; R3 esta `EFFECTIVE` y no existe CR material abierta. La aceptacion abre F1 exclusivamente para
caracterizacion. No abre F2 ni autoriza extraer AUTH-01..14 o cambiar comportamiento observable.
