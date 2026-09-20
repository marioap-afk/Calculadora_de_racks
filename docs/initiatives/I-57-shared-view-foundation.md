---
schema: rackcad-initiative/v1
id: I-57
title: "Shared View Foundation"
type: architecture
status: claimed
branch: architecture/shared-view-foundation
base_branch: main
priority:
size:
depends_on: []
conflicts_with: [I-49, I-52, I-55]
context_packs: [architecture-kernel, persistence, autocad-plugin, documentation-governance, delivery-validation]
automation_state_path:
decision_paths: [docs/automation/decisions/I-57.md]
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

# I-57 — Shared View Foundation

> **Fase actual: F3 COMPLETE sobre AUTH-05; F4 NOT OPEN.** Proposal V5, R3 y ADR-0044 permanecen intactos. AUTH-05
> tiene evidencia automatizada exacta y `OV-FND-01 = PASS 6/6` sobre su SHA validado. F1 y F2 permanecen completas.

```text
Initiative        = I-57 — Shared View Foundation
Branch            = architecture/shared-view-foundation
Worktree          = ~/.codex/worktrees/architecture-shared-view-foundation
BASE_SHA          = dad4e77f4f267b9fa248ecb0c8bfd8a74bbab093
PROPOSAL_V4_SHA   = 2a142f224fb8d8a0c16bd7f7334dc48be3be9eef
PROPOSAL_V5_SHA   = 91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca
CLAIM_SHA         = 869cf464129da65323781dbebe2425083f21915f
CLAIM_ID          = 3e77d0e4-67de-4c3a-b3ed-ee65f0ac7cd3
Owner             = autorizacion explicita registrada en decisions/I-57.md
Consumers         = I-52 RACKMIRROR · I-55 View Placement & Projection
Foundation ADR    = ADR-0044 · ACCEPTED
SVF reconciliation= R1 @ aa37264381e7d386339305d11030314ffd219d63
R1 blob           = 2faa5a316680aa92306c7bce75c42cc311f96a26
R1 status         = SUPERSEDED FOR REVIEW BY R2 · NEVER EFFECTIVE
R2 status         = SUPERSEDED FOR REVIEW BY R3 · NEVER EFFECTIVE
R3 status         = EFFECTIVE · I-57/I-52/I-55 REGISTERED SAME EXACT OBJECT
R3 blob           = cd42db03becff42f98b047e61c46689c17a69670

Foundation Coordinator = AGREED ON V5
Foundation Architect   = AGREED ON V5
Technical Consensus   = REACHED ON V5
Full Initiative Consensus = NOT DECLARED
Foundation Implementation = F3 COMPLETE / AUTH-01..05 ONLY
F0 = ACCEPTED / CLOSED
F1 = COMPLETE / CHARACTERIZATION ONLY
F2 = COMPLETE / AUTH-01..04 ONLY
F3 = COMPLETE / AUTH-05 ONLY
F4 = NOT OPEN
```

## 1. Objetivo

Extraer una infraestructura neutral e integrable en `main` antes de sus consumidores, con una sola
autoridad compartida para I-52 e I-55. La foundation expresa hechos y contratos reutilizados por ambos
productos sin incorporar politicas de `RACKMIRROR` ni de View Placement & Projection.

Resultado esperado tras F8: las autoridades neutrales acordadas viven en `main`; I-52 e I-55 rebasan
segun WORKFLOW y consumen esa implementacion integrada, sin copiar codigo entre ramas.

## 2. Problema

I-52 e I-55 necesitan la misma taxonomia de vistas, lectura persistida, marcos, resolucion, planes y
otras autoridades. Extraerlas dentro de cualquiera de las dos ramas de producto obligaria a la otra a
esperar una integracion ajena o a duplicar contratos. I-55 Proposal V5 recomienda una iniciativa neutral;
el Owner acepto ese mecanismo y autorizo I-57.

R1 nunca fue efectiva y quedo superada por R3. R3 esta efectiva tras el registro exacto de I-57, I-52 e I-55.
La autoridad aun no es consumible porque su `Integration SHA` permanece vacio.

## 3. Alcance propuesto

F0 registra, sin implementar, el alcance que una Proposal posterior debe revisar:

| AUTH | Contrato neutral | Caracterizacion compartida |
|---|---|---|
| AUTH-01 | `RackViewKind` | CT-04 |
| AUTH-02 | `RackViewAddress` / `RackViewVariant` | CT-04 |
| AUTH-03 | Codec sintactico | CT-04 |
| AUTH-04 | Hechos de disponibilidad | CT-04 |
| AUTH-05 | `RackViewFrame`, `[K_min,K_max]`, centro `c` | CT-05 |
| AUTH-06 | Hechos compartidos de barrido y clasificacion | CT-SCAN, CT-04 |
| AUTH-07 | Nucleo neutral de seleccion | CT-16 |
| AUTH-08 | Valor de colocacion y hechos de transformacion | CT-GEO |
| AUTH-09 | Contrato `Resolve` | CT-RES |
| AUTH-10 | Contrato `Plan` | CT-PLAN |
| AUTH-11 | Autoridad de nombre base | CT-NAME |
| AUTH-12 | `LibraryBlockRequirement` y contrato de consulta | CT-BLK |
| AUTH-13 | Contrato del comparador authored | CT-AUTH |
| AUTH-14 | Caracterizaciones compartidas | CT-04, CT-05, CT-16 y auxiliares anteriores |

La fuente tecnica propuesta es I-55 Proposal V5 y su specification publicada en
`f49671e29c6cc817166c720fe3f975deb92d4c3b`. I-57 no copia esos borradores en F0: registra sus objetos
exactos como entrada para Discovery/Proposal propios. Todo contrato puede refinarse antes del consenso.

## 4. Fuera de alcance

- `RACKPROYECTAR`, `RACKMIRROR` y los comandos o UX de producto.
- ID17, la cola de ID18 y las politicas `Rigid`/`Orthographic` de ID19.
- Relative Frame Window, reflexion del espejo, mirror read-set y decisiones OD de producto.
- CQ-01, redibujo atomico de hermanas y flujo productivo de Insertar.
- AUTH-15 de creacion caller-owned, mientras R1 lo conserve fuera.
- Implementar AUTH-01..AUTH-14, tests RED, refactors o extraccion durante F0.
- Modificar ADR-0034 o cualquier ADR aceptado.
- Cambiar productivamente I-52 o I-55, o integrar esta rama.

No es un framework generico: sus consumidores reales y obligatorios son I-52 e I-55.

## 5. Contexto requerido

- `AGENTS.md`, `README.md`, `docs/HANDOFF.md`, `docs/ARCHITECTURE.md`, `docs/WORKFLOW.md` y `docs/ROADMAP.md`.
- Context packs declarados en la cabecera; sus globs orientan y no amplian alcance.
- ADR-0034 aceptado e intacto: Selectivo → adaptador → autoridad vigente del handler, una resolucion efectiva por rack.
- I-55 Proposal V5 `f49671e29c6cc817166c720fe3f975deb92d4c3b` y R1 exacta.
- Respuesta I-52 a R1 publicada en `4158f5c56482cc33f5083e8e4e9ff72465c566ce`.

## 6. Dependencias y gobierno

I-57 nace directamente de `origin/main`; no depende de integrar I-52 o I-55. I-49, I-52 e I-55 son
coordinaciones/conflictos por archivos y autoridades, no bases para copiar codigo. Antes de fijar archivos de
produccion se repite el preflight remoto y se serializan los archivos calientes.

```text
Owner → Coordinator I-57 ↔ Architect I-57 → consenso propio → implementacion
```

Proposal V5 tiene veredictos `AGREED` de Coordinator y Architect sobre el SHA exacto
`91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca`; ADR-0044 permanece `ACCEPTED`; latest Rn = R3 permanece
`EFFECTIVE`, con el mismo objeto registrado por I-52, I-55 e I-57 y ninguna CR material abierta. CT-04 V2 resuelve
la contradiccion material y gobierna F2 donde el fixture historico de F1 contradiga V2. F2 implemento solo
AUTH-01..04. El relevo posterior a I-49 esta registrado en `I-57-post-i49-rebase-f2-resume.md`: el avance se
clasifico `MATERIAL BUT COMPATIBLE`, los objetos normativos conservaron sus blobs y el SHA exacto
`b0f36cdbf404cca82e29d1e3ca933b4ed4b4671d` obtuvo pruebas focales verdes y CI `push` 4/4 verde. Ese SHA es
`F2_START_SHA`; la implementacion conserva la concurrencia de I-49 y migra `ProjectVariableMutationExecutor` al final.
El recibo [`I-57-f2-auth01-04-evidence.md`](I-57-f2-auth01-04-evidence.md) acredita el SHA productivo
`2f63da6259aa9fd23ce7f42ee786c63eebae25b7`, Core Full local, build Debug del Plugin y CI de push 4/4 verde.
F3 implemento exclusivamente AUTH-05. El recibo [`I-57-f3-auth05-evidence.md`](I-57-f3-auth05-evidence.md)
acredita `F3_VALIDATION_SHA = 3102368ba54190e93cc6daf1d6d7e1ef72ba4266`, CT-05 y pruebas focales, ambas suites
Full locales, builds, CI exacta 4/4 y `OV-FND-01 = PASS 6/6` en AutoCAD 2025. El build exacto de AutoCAD no fue
suministrado por el Owner y queda registrado literalmente; la guia manual §7 exige la version, que si fue provista.

## 7. Archivos esperados

F0 solo crea este contrato y `docs/automation/decisions/I-57.md`, y agrega la fila I-57 a ROADMAP.
Discovery/Proposal posteriores pueden reexpresar la especificacion exacta de I-55 y fijar el mapa de archivos.
Ningun archivo de `src/`, `tests/`, `assets/`, `.github/`, `eng/` o `deploy/` pertenece a F0.

## 8. Gates preliminares

| Gate | Alcance preliminar | Estado tras F0 |
|---|---|---|
| F0 | Claim remoto + bootstrap documental | ACCEPTED / CLOSED |
| F1 | Caracterizacion | COMPLETE / 10 CT GREEN / ZERO PRODUCTION CHANGES |
| F2 | Taxonomia + codec sintactico + availability | COMPLETE / AUTH-01..04 ONLY; implementation `2f63da6259aa9fd23ce7f42ee786c63eebae25b7` |
| F3 | Marco y tramo | COMPLETE / AUTH-05 ONLY; validation `3102368ba54190e93cc6daf1d6d7e1ef72ba4266`; OV-FND-01 PASS 6/6 |
| F4 | Seleccion neutral + hechos de colocacion | NOT OPEN / PENDING COORDINATOR REVIEW |
| F5 | Resolve, Plan, adaptadores y nombre | BLOCKED |
| F6 | Comparador authored + requisitos/consulta de biblioteca | BLOCKED |
| F7 | Candidato | BLOCKED |
| F8 | Integracion en `main` | BLOCKED |

La Proposal de I-57 puede refinar estos gates. F0 no abre F1 automaticamente.

## 9. Pruebas y builds

F0 es exclusivamente documental. Su evidencia es `git diff --check`, alcance de archivos y CI del SHA exacto
de cada push. No se ejecutan suites locales ni builds como sustituto de esa CI.

En F7 se aplicara la definicion de Candidato vigente: arbol limpio, Core y UI completos en local, Debug UI y
Plugin, CI de push exacta y validacion del Owner en AutoCAD cuando el cambio observable lo requiera.

## 10. Validacion manual

No aplica a F0. La implementacion neutral puede cambiar rutas internas de dibujo/BOM y por eso el plan final
debe decidir y ejecutar la validacion del Owner sobre el Candidato exacto; este contrato no la da por cumplida.

## 11. Criterios de aceptacion

F0 queda completo cuando: claim valido publicado, contrato y decisiones versionados, fila propia en ROADMAP,
CI verde del claim y bootstrap, refs remotos exactos registrados y todos los worktrees tocados limpios.

La iniciativa completa exige una sola implementacion neutral integrada, consumidores desde `main`, paridad
observable demostrada, reconciliacion exacta y ausencia de productores legacy no gobernados.

## 12. Condiciones para detenerse

- Cualquier intento de abrir F1 sin revision y consenso propios.
- La latest reconciliation Rn queda `NOT EFFECTIVE`, tiene una CR material abierta o deja titularidad duplicada.
  Hoy latest Rn = R3, `EFFECTIVE`, sin CR material abierta y con titularidad unica.
- Cambios productivos durante F0 o expansion hacia politica de I-52/I-55.
- Conflicto de archivos con una iniciativa activa sin serializacion.
- Necesidad de numerar o aceptar el ADR sin decision correspondiente.
- CI roja o discrepancia entre SHA local y remoto.

## 13. Estado versionado y entrega

No hay archivo de automation state ni Pull Request. La automatizacion esta deshabilitada y el auto-merge
prohibido. El estado vivo se deriva de `origin/architecture/shared-view-foundation`; las decisiones estan en
`docs/automation/decisions/I-57.md`. Push de rama no equivale a integracion.

## 14. Evidencia F0

Se completa en el registro de decisiones y en el reporte de cierre tras observar la CI de claim/bootstrap.
`main` no se modifica y HANDOFF no se toca.
