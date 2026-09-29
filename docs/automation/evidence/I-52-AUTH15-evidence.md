# I-52-AUTH15 — Evidencia de la unidad

Unit: `I-52-AUTH15` · Initiative: `I-52` (RACKMIRROR, ID16) · Workflow: **V2** (caso T4 del reclamo, decisiones §2) ·
Claim-Id: `1f035b6f-6891-48fd-8965-f7ff1f522767` · CLAIM_SHA: `4b20bda3`

Contrato: [I-52-auth15-definition-creator.md](../../initiatives/I-52-auth15-definition-creator.md) ·
Decisiones: [I-52-AUTH15.md](../decisions/I-52-AUTH15.md) ·
Evidencia cruda de host: [I-52-AUTH15-run3/](I-52-AUTH15-run3/README.md)

Estado de este archivo: **cierre documental PRE-MERGE. Candidate conformado; la unidad NO esta integrada.** No hay merge, ni CI post-merge, ni limpieza, ni tag
`integration/I-52-AUTH15`. Hasta que `main` contenga el merge, ni el HANDOFF ni el ROADMAP declaran la unidad integrada.

## Workflow aplicable

Esta unidad se rige por **Workflow V2**, tal como la clasifico su reclamo (decisiones §2, caso T4). La clasificacion descansa en el estado establecido por el
repositorio: Workflow V2 entro en vigor con el merge normativo de I-56, `WORKFLOW_V2_EFFECTIVE_SHA` = `8a021fb67c16dfccd6afc18448ea7e6a71a32364`
(«Merge I-56: activa normativamente Workflow V2», alcanzable desde `origin/main`; el tag `integration/I-56` lo registra sin redefinirlo). Se aplican:

- `docs/INITIATIVE_LIFECYCLE.md` §8: READY-01..READY-09 antes de declarar `FINAL_CANDIDATE_SHA`; §9 conformidad final (Architect + Coordinator).
- `AGENTS.md`, «Pruebas — definicion de terminado»: Core Full y UI Full locales sobre el Candidato y sobre el cierre documental, builds Debug de UI y Plugin,
  CI de push sobre el SHA exacto y cobertura segun la politica vigente.
- `docs/WORKFLOW.md` §11.4 (archivo de evidencia), §11.5 (integracion V2 y ruta R) y §11.6 (tag anotado `integration/I-52-AUTH15` con `Workflow: V2`).

> Nota fuera de alcance: el texto de `docs/WORKFLOW.md` (encabezado y §11.2) que dice que `WORKFLOW_V2_EFFECTIVE_SHA` «no esta aun establecido» esta
> desfasado respecto del repositorio. Se trata como deriva documental ajena a esta unidad y no se corrige aqui.

## Base y clasificacion

- Base: `origin/main` = `016bf46715cec45e644f88a22ef091b311a2bef1` (I-59 integrada). `main` no avanzo durante la unidad: **no hubo rebase** y no aplica la ruta R
  (se vuelve a comprobar con `fetch` inmediatamente antes del merge).
- Alcance de produccion: dos altas en `src/RackCad.Plugin/Systems/Shared/` (`RackDefinitionCreator.cs`, `RackDefinitionCreationResult.cs`) y una
  prueba de guardas (`tests/RackCad.Tests/RackDefinitionCreatorGuardTests.cs`, conforme a ADR-0003). Sin llamador de produccion, sin UI, sin cambio de
  comportamiento de dibujo visible. AUTH-15 queda fuera de la Shared View Foundation.
- Diff del Candidato contra `main`: solo `src/RackCad.Plugin/Systems/Shared/` (2 archivos), la prueba de guardas, `.gitattributes` (`-text` acotado a la evidencia
  cruda de RUN-3) y `docs/`. Nada en `eng/`: el arnes se retiro.

## Identidad

| Rol | SHA |
|---|---|
| Implementacion validada en host | `a80a3801cc39eaa9656be05c080853be87ab2581` |
| Arnes temporal (ultimo estado, retirado del Candidato) | `fcca6e6c5713e966c612998a1eb9e6833825d47e` |
| Commit documental (evidencia cruda de RUN-3 + decisiones) | `f9b7669bc5db301007d716c1d076decf58d4e421` |
| **`FINAL_CANDIDATE_SHA`** (congelado por el Coordinador; solo elimina `eng/research/I52Auth15Host/`) | `0b6abdd5d0e323944225b30832f77f68b7c3c497` |
| Version previa de este archivo de evidencia (commit documental posterior, sin redefinir el Candidato) | `d9a41316a426edfd151871e201f5e117c08470a1` |
| `CLOSURE_SHA` (commit de cierre documental) | el commit que porta esta version del archivo; un commit no puede contener su propio SHA, asi que se registra en su mensaje y en el tag |

`FINAL_CANDIDATE_SHA` no se amenda, no se rebasa y no se sustituye por la punta de la rama. Los commits documentales posteriores (`d9a41316` y el cierre) son SHAs
propios con CI propio y **no heredan** la evidencia del Candidato ni la acreditan (AGENTS.md, «Reutilizacion de evidencia»).

Arboles: `src/` = `224ca6a32c6b17128d5ce168dca307246b16fbcd` y `tests/` = `53fb9b98d0c2b196a8e712dc095df40747f0ee88` en la implementacion, en el arnes, en el
Candidato y en el cierre (`git diff FINAL_CANDIDATE_SHA..CLOSURE -- src tests` = vacio; el cierre solo toca `docs/`).

## READY-06 y declaracion del Candidato

- **READY-06 sobre `0b6abdd5`: Architect = PASS (`CONFORMANT`), Coordinator = PASS.** Sin blockers ni major.
- `FINAL_CANDIDATE_SHA` = `0b6abdd5d0e323944225b30832f77f68b7c3c497`, declarado por el Coordinador. Segun `INITIATIVE_LIFECYCLE.md` §8 esa declaracion presupone
  READY-01..READY-09 satisfechos; este archivo no desglosa cada condicion por separado (READY-04: `main` sigue en la base y no hubo rebase; READY-05: ver la tabla de
  evidencia automatizada).
- No hay Freeze ni A-n propios: el contrato aclara dentro del requisito congelado por I-52 (Freeze V17/V18, AUTH-15) sin freeze, ADR ni decision de Owner nuevos
  (decisiones §3).

## Evidencia automatizada sobre el Candidato `0b6abdd5`

Orden aplicado (AGENTS.md, «Reutilizacion de evidencia»): commit Candidato -> arbol limpio -> evidencia sobre ese HEAD exacto -> registro.

| Clase | Resultado |
|---|---|
| Arbol de trabajo | limpio (`git status --porcelain` vacio antes y despues) |
| SDK resuelto | 8.0.423 (`global.json` 8.0.423, `rollForward: latestFeature`) |
| Core Full local (`RackCad.Tests`) | 11342 / 11342 PASS, 0 omitidas |
| UI Full local (`RackCad.UI.Tests`) | 1581 PASS, 17 omitidas historicas, 0 fallos |
| Build UI Debug | 0 errores, 0 advertencias |
| Build Plugin Debug | 0 errores; 2 advertencias `MSB3277` (conflictos de version de ensamblados de AutoCAD/.NET). Preexistentes: un rebuild forzado del Plugin Debug sobre la base `016bf467` muestra las mismas 2 advertencias. No bloquean |
| CI de push sobre el SHA exacto | run `36619506589`, `event=push`, `head_sha=0b6abdd5…`: Build UI, Build Plugin without AutoCAD, Tests (Domain + Application) y UI Tests = success (4/4) |
| Cobertura del Candidato | **diferida**: `workflow_dispatch` solo existe tras publicar `ci.yml` en `main` (WORKFLOW paso 7); no es la evidencia de CI del Candidato |

## Validacion en host (AutoCAD 2025 R25.0.171)

Historial: RUN-1 INVALID (defecto del arnes: `PROFILENAME`, 0 llamadas a AUTH-15); RUN-2 ejecucion VALIDA con resultado FAIL (fugas tras Abort sin atribuir, nombre
efectivo vacio confirmado); postcondicion de nombre efectivo `a80a3801`; RUN-3 (paquete `fcca6e6c`).

- **Resultado crudo de RUN-3: FAIL.** Se **conserva sin modificar** como evidencia historica: ejecucion valida, 29/29 comprobaciones del lanzador, causado
  exclusivamente por los siete controles SIDE-DB (42 fugas de control, 128 de caracterizacion; todas listadas en la evidencia cruda).
- **Resultado canonico de admision: AUTH-15 PASS UNDER DOCUMENT-AUTHORITY, derivado de RUN-3 por decision del Arquitecto y ratificado por el Owner.** No es «RUN-3 = PASS».
- Base: HV-00..HV-14 15/15 PASS con los nueve casos sensibles a rollback en DOCUMENT-AUTHORITY; siete controles de documento PASS; fugas de documento 0; Problems 0;
  Deviations 0; FILEDIA 1 -> 0 -> 1; sin timeout; RUN-4 no requerida.
- Afirmado: Option B; garantia de rollback del llamador **autoritativa solo para documento + `LockDocument` + `StartTransaction()` + Abort o Dispose sin Commit**;
  `OpenCloseTransaction` no cubierta.
- Evidencia cruda, hashes y lectura: [I-52-AUTH15-run3/README.md](I-52-AUTH15-run3/README.md).

### Limitacion de SIDE-DB (registrada)

El rollback en una base de datos lateral (`new Database(...)`) es **solo caracterizacion y no es autoritativo** para la admision de AUTH-15. En RUN-3 la misma escritura
primitiva (sin codigo de AUTH-15) sobrevivio al Abort y al Dispose en la base lateral y no en el documento. El mecanismo no se identifico y no se afirma como comportamiento
general de AutoCAD. Un llamador que use una base lateral no debe confiar en Abort para limpiar: debe descartar la base. AUTH-15 no rechaza bases laterales; el contrato lo documenta.

### Excepcion de identidad ratificada por el Owner (debe nombrarse en el tag)

La validacion en host ejecuto binarios construidos a partir de los arboles de producto validados (`src/` `224ca6a3…` y `tests/` `53fb9b98…`) con el SHA de arnes
`fcca6e6c` estampado en `InformationalVersion` (`1.0.0+fcca6e6c…`), **no** un binario construido a partir del Candidato `0b6abdd5`. **Igualdad de arbol no es identidad de
binario** y la regla general (AGENTS.md, «Reutilizacion de evidencia» y «Validacion del dueño») solo reutiliza la validacion con el mismo SHA exacto. Esta unidad es una **excepcion
ratificada por el Owner**: la admision descansa en igualdad de arboles de producto mas la decision gobernada (Architect + Owner). Ninguno de los binarios de `a80a3801` ni de `0b6abdd5` se
ejecuto. AUTH-15 no tiene llamador de produccion ni comportamiento de dibujo visible. **El mensaje del tag `integration/I-52-AUTH15` debe nombrar esta excepcion.**

Variables de la validacion: AutoCAD 2025 `R25.0.171.0.0` (`acad.exe` SHA-256 `2A75996F…6422`); DWG en blanco AC1032 SHA-256 `7E7CDC22…15CC`; sin biblioteca externa de bloques
(el arnes crea sus propios bloques auxiliares).

## Conformidad

- Architect, revision exacta de la implementacion (delta de `2d10de70`, incluida AUTH15-DEV-01): APPROVED. Postcondicion `a80a3801`: APPROVED sin cambios de produccion.
- Architect, arnes: `0a5fe046` con cambios requeridos (MAJOR-1) -> `fcca6e6c` APPROVED (revision del delta).
- Architect, evidencia de RUN-3: admisible como PASS bajo DOCUMENT-AUTHORITY (decisiones §13-14); ratificacion del Owner registrada.
- **READY-06 sobre `0b6abdd5`: Architect PASS + Coordinator PASS.**

## Metricas

| Metrica | Valor |
|---|---|
| Arquetipo inicial / final y disparadores | UNKNOWN (no registrado en esta unidad) |
| Rondas del Architect | varias sobre la implementacion (`fed44e56` CHANGES REQUIRED; delta `2d10de70` y postcondicion `a80a3801` APPROVED) y sobre el arnes (cambios requeridos hasta `fcca6e6c` APPROVED); 1 sobre la evidencia de RUN-3; 1 READY-06. Conteo exacto por modo: UNKNOWN |
| Corridas en host | 3 (RUN-1 INVALID, RUN-2 FAIL valido, RUN-3 FAIL crudo / PASS derivado) |
| Full locales por Candidato | 1 (Core 11342, UI 1581 + 17 omitidas) |
| Rondas Discovery / EXP, gates funcionales totales/verificables | UNKNOWN |
| Owner Validation clasica (escenarios OV de la guia manual) | no ejecutada: AUTH-15 no tiene comportamiento de dibujo visible; sustituida por la ratificacion del Owner de la validacion en host (ver la excepcion de identidad) |

## Que esta completo y que falta

| | Estado |
|---|---|
| Candidate (`0b6abdd5`): conformidad, CI exacta, Core/UI Full, builds Debug, admision en host | **COMPLETO** |
| Cierre documental (este commit): CI propia y Core/UI Full locales sobre el SHA de cierre | evidencia de ese SHA en su mensaje de commit y en el reporte de cierre; **no** hereda la del Candidato |
| `fetch` inmediatamente antes del merge; ruta R si `main` avanzo | pendiente |
| Merge manual `--no-ff`; CI `event=push`, `ref=refs/heads/main`, `head_sha=MERGE_SHA` con cobertura; comprobacion diferida de cobertura del Candidato | pendiente |
| Limpieza segura; tag anotado `integration/I-52-AUTH15` (`Workflow: V2`, nombrando la excepcion de identidad) | pendiente |
| Reconciliacion con I-55 (su G15 depende de esta unidad; no consume por cherry-pick) | pendiente |
| **Unidad INTEGRADA** | **NO** hasta que `main` contenga el merge verificado y exista el tag |

Integration tag reference: ninguno todavia.
