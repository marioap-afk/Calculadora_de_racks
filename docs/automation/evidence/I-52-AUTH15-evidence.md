# I-52-AUTH15 — Evidencia de la unidad

Unit: `I-52-AUTH15` · Initiative: `I-52` (RACKMIRROR, ID16) · Workflow: V2 (Workflow V1 sigue efectivo: no existe `WORKFLOW_V2_EFFECTIVE_SHA`) ·
Claim-Id: `1f035b6f-6891-48fd-8965-f7ff1f522767` · CLAIM_SHA: `4b20bda3`

Contrato: [I-52-auth15-definition-creator.md](../../initiatives/I-52-auth15-definition-creator.md) ·
Decisiones: [I-52-AUTH15.md](../decisions/I-52-AUTH15.md) ·
Evidencia cruda de host: [I-52-AUTH15-run3/](I-52-AUTH15-run3/README.md)

Estado de este archivo: **preparacion de la integracion. No hay merge, ni cierre documental, ni HANDOFF, ni tag `integration/*`.**

## Base y clasificacion

- Base: `origin/main` = `016bf46715cec45e644f88a22ef091b311a2bef1` (I-59 integrada). `main` no avanzo durante la unidad
  (`git rev-list --count HEAD..origin/main` = 0 al preparar el Candidato): **no hubo rebase** y no aplica la ruta R.
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
| Commit documental (evidencia cruda + decisiones) | `f9b7669bc5db301007d716c1d076decf58d4e421` |
| **`FINAL_CANDIDATE_SHA` propuesto** (retira el arnes; solo elimina `eng/research/I52Auth15Host/`) | `0b6abdd5d0e323944225b30832f77f68b7c3c497` |

Arboles: `src/` = `224ca6a32c6b17128d5ce168dca307246b16fbcd` y `tests/` = `53fb9b98d0c2b196a8e712dc095df40747f0ee88` en la implementacion, en el arnes y en el
Candidato (`git diff --quiet a80a3801 0b6abdd5 -- src tests` = sin diferencias).

> `FINAL_CANDIDATE_SHA` esta **propuesto**, no declarado: `docs/INITIATIVE_LIFECYCLE.md` §8 exige READY-01..09 antes de declararlo y READY-06 (conformidad completa de
> Architect + Coordinator sobre este SHA) esta pendiente. Ver «Compuertas abiertas».

## Evidencia automatizada sobre el Candidato `0b6abdd5`

Orden aplicado (AGENTS.md, «Reutilizacion de evidencia»): commit Candidato -> arbol limpio -> evidencia sobre ese HEAD exacto -> registro.

| Clase | Resultado |
|---|---|
| Arbol de trabajo | limpio (`git status --porcelain` vacio antes y despues) |
| SDK resuelto | 8.0.423 (`global.json` 8.0.423, `rollForward: latestFeature`) |
| Core Full local (`RackCad.Tests`) | 11342 / 11342 PASS, 0 omitidas |
| UI Full local (`RackCad.UI.Tests`) | 1581 PASS, 17 omitidas historicas, 0 fallos |
| Build UI Debug | 0 errores, 0 advertencias |
| Build Plugin Debug | 0 errores; 2 advertencias `MSB3277` (conflictos de version de ensamblados de AutoCAD/.NET, preexistentes y ajenas a la unidad) |
| CI de push sobre el SHA exacto | run `36619506589`, `event=push`, `head_sha=0b6abdd5…`: Build UI, Build Plugin without AutoCAD, Tests (Domain + Application) y UI Tests = success (4/4) |
| Cobertura del Candidato | **diferida**: `workflow_dispatch` solo existe tras publicar `ci.yml` en `main` (WORKFLOW paso 7); no es la evidencia de CI del Candidato |

## Validacion en host (AutoCAD 2025 R25.0.171)

Historial: RUN-1 INVALID (defecto del arnes: `PROFILENAME`, 0 llamadas a AUTH-15); RUN-2 ejecucion VALIDA con resultado FAIL (fugas tras Abort sin atribuir, nombre
efectivo vacio confirmado); postcondicion de nombre efectivo `a80a3801`; RUN-3 (paquete `fcca6e6c`).

- **Resultado crudo de RUN-3: FAIL** (ejecucion valida, 29/29 comprobaciones del lanzador). Causado solo por los siete controles SIDE-DB. Conservado sin modificar.
- **Resultado canonico: PASS UNDER DOCUMENT-AUTHORITY, derivado de RUN-3 por decision**, ratificado por el Owner. No es «RUN-3 = PASS».
- Base: HV-00..HV-14 15/15 PASS con los nueve casos sensibles a rollback en DOCUMENT-AUTHORITY; siete controles de documento PASS; fugas de documento 0; Problems 0;
  Deviations 0; FILEDIA 1 -> 0 -> 1; sin timeout.
- Afirmado por el Owner: Option B; garantia de rollback del llamador solo para documento + `LockDocument` + `StartTransaction()` + Abort o Dispose sin Commit;
  base lateral solo caracterizacion (descartar la base, no confiar en Abort); `OpenCloseTransaction` no cubierta; RUN-4 no requerida.
- Evidencia cruda, hashes y lectura: [I-52-AUTH15-run3/README.md](I-52-AUTH15-run3/README.md).

### Salvedad de identidad (a la vista, no oculta)

La validacion en host se hizo sobre el binario del paquete `fcca6e6c` (implementacion `a80a3801` + arnes), no sobre el binario del Candidato `0b6abdd5`.
`src/` y `tests/` son byte-identicos, pero **igualdad de arbol no es identidad de binario**: el SHA se estampa en `InformationalVersion` (AGENTS.md,
«Reutilizacion de evidencia»; «Validacion del dueño»: mismo SHA exacto + misma version de AutoCAD + misma biblioteca de bloques). La admision descansa en la
decision del Owner de ratificar el PASS derivado con esta ligadura por SHA de implementacion + igualdad de arboles. AUTH-15 no tiene llamador de produccion
ni comportamiento de dibujo visible que el Owner valide de otro modo.

Variables de la validacion: AutoCAD 2025 `R25.0.171.0.0` (`acad.exe` SHA-256 `2A75996F…6422`); DWG en blanco AC1032 SHA-256 `7E7CDC22…15CC`; sin biblioteca externa de bloques
(el arnes crea sus propios bloques auxiliares).

## Conformidad

- Architect, revision exacta de la implementacion (delta de `2d10de70`, incluida AUTH15-DEV-01): APPROVED. Postcondicion `a80a3801`: APPROVED sin cambios de produccion.
- Architect, arnes: `0a5fe046` con cambios requeridos (MAJOR-1) -> `fcca6e6c` APPROVED (revision del delta).
- Architect, revision de la evidencia de RUN-3: admisible como PASS bajo DOCUMENT-AUTHORITY (decisiones §13-14).
- **READY-06 sobre `0b6abdd5`: PENDIENTE** (Architect + Coordinator, `CONFORMING` sobre el SHA del Candidato). Este archivo no lo declara.

## Metricas

| Metrica | Valor |
|---|---|
| Arquetipo inicial / final y disparadores | UNKNOWN (no registrado en esta unidad) |
| Rondas del Architect | varias sobre la implementacion (`fed44e56` CHANGES REQUIRED; delta `2d10de70` y postcondicion `a80a3801` APPROVED) y sobre el arnes (cambios requeridos hasta `fcca6e6c` APPROVED); 1 sobre la evidencia de RUN-3. Conteo exacto por modo: UNKNOWN |
| Corridas en host | 3 (RUN-1 INVALID, RUN-2 FAIL valido, RUN-3 FAIL crudo / PASS derivado) |
| Full locales por Candidato | 1 (Core 11342, UI 1581 + 17 omitidas) |
| Rondas Discovery / EXP, gates funcionales totales/verificables | UNKNOWN |
| Owner Validation clasica (escenarios OV de la guia manual) | no ejecutada: AUTH-15 no tiene comportamiento de dibujo visible; sustituida por la ratificacion del Owner de la validacion en host (ver la salvedad de identidad) |

## Compuertas abiertas para la integracion (nada de esto se hizo)

1. READY-01..09 con conformidad `CONFORMING` (Architect + Coordinator) sobre `0b6abdd5` y declaracion de `FINAL_CANDIDATE_SHA`.
2. Cierre documental de la unidad: `docs/HANDOFF.md` §8-12, marca `integrada (fecha)` en `docs/ROADMAP.md`, indices; con CI propio y Core/UI Full (AGENTS.md).
3. `fetch` inmediatamente antes del merge y ruta R si `main` avanzo.
4. Merge manual `--no-ff`; CI de `event=push`, `ref=refs/heads/main`, `head_sha=MERGE_SHA` con cobertura; comprobacion diferida de cobertura del Candidato.
5. Limpieza segura y tag anotado `integration/I-52-AUTH15` (WORKFLOW §11.6), luego reconciliacion con I-55 (su G15 depende de esta unidad).

Integration tag reference: ninguno todavia.
