# I-57 F2 — Evidencia AUTH-01..04

```text
Gate                  = F2 / AUTH-01..04 ONLY
F2_START_SHA          = b0f36cdbf404cca82e29d1e3ca933b4ed4b4671d
F2_IMPLEMENTATION_SHA = 2f63da6259aa9fd23ce7f42ee786c63eebae25b7
Proposal              = V5 @ 91a3d1ca779e57a5c47f474d9ae18d5a01e3f8ca
R3                     = EFFECTIVE
ADR-0044               = ACCEPTED / UNCHANGED
F2 result              = COMPLETE
F3                     = NOT OPEN
```

## Resultado por autoridad

| AUTH | Implementacion | Resultado |
|---|---|---|
| AUTH-01 | reutiliza `DimensionViewKind` como taxonomia compartida | una sola identidad para Frontal/Lateral/Planta; no se crea enum competidor |
| AUTH-02 | `RackViewAddress` + `RackViewVariant` discriminada | Whole, Fondo, Post, FlowEnd, PushBackCut y Station quedan tipados; no hay `object`, JSON ni reflection |
| AUTH-03 | `RackViewCodec` total sobre `Kind/View/Section` | conserva sintaxis original, produce Canonical/Canonicalizable/Coerced/Invalid y codigo de coercion; Invalid no contiene address |
| AUTH-04 | `RackViewAvailability` + facts por kind | evalua existencia fisica despues del decode; no elige fallback ni accion del consumer |

`RackViewCodecTests` ejecuta todas las filas de la matriz CT-04 V2 y comprueba el canonical encoding. Los casos
`Fondo(999)`, `Post(999)` y `Station(999)` conservan una address sintacticamente valida y pasan a availability.
La precedencia de `Coerced` sobre `Canonicalizable`, los defaults historicos y la separacion syntax/availability
quedan fijados por esa matriz.

## Delegacion de readers y paridad

Los readers persistidos delegan en `RackCommandSupport.DecodeView` y conservan su policy local:

- Selectivo conserva unknown/non-lateral/non-planta como frontal, negativos frontales como `Fondo(0)` y el
  tratamiento stale/orphan del consumer.
- Dinamico conserva el redraw persistido non-frontal/non-planta como lateral, post negativo como `Post(0)` y
  non-Entrance frontal como Exit. Prompt, insercion de sistema completo y eleccion interactiva permanecen fuera
  del codec.
- Push Back mantiene preflight estricto 0..3, side/end tipados y `DecodeSection` solo en el flujo interactivo que
  ya lo poseia.
- Cantilever mantiene descriptor estricto, Station y rechazo de `AdapterSection`; una station ausente se marca
  stale mediante availability y nunca se redirige.
- Cabecera usa el adapter compartido y conserva non-planta como lateral.
- Cama conserva su unica vista historica: `View/Section` se ignoran durante el decode y no se infieren vistas
  nuevas.
- `ProjectVariableMutationExecutor` fue el ultimo reader migrado. El diff se limita a decode/availability; la
  preparacion, `PlanReadSet`, reescaneo, acreditacion y commit caller-owned integrados por I-49 permanecen intactos.

No se modificaron AUTH-05..14, Proposal V5, R3, ADR-0044, esquemas DWG ni los valores persistidos
`View/Section`. Cada consumer sigue decidiendo por si mismo abort, stale, continue o mensaje visible.

## RED → GREEN y regresion

La prueba conductual `SharedViewCodecTests` se creo antes de las autoridades productivas y su primera ejecucion
fallo al compilar por ausencia de `RackViewCodec`, `RackViewAddress` y facts de availability. Tras implementar y
delegar los readers, el filtro focal de codec, CT-04 V2, PVME e invariantes I-49 paso 134/134.

La primera Core Full posterior a la migracion encontro dos guardias Cantilever que exigian expresiones legacy
locales aunque la misma regla ya se habia delegado. Fallo 2/8,174. Sus oraculos se cambiaron para exigir el codec,
facts de availability y fail-closed por kind; el filtro correspondiente paso 28/28 y Core Full paso 8,174/8,174.

## Evidencia sobre el SHA de implementacion

El arbol estaba limpio antes de estas corridas sobre
`2f63da6259aa9fd23ce7f42ee786c63eebae25b7`.

| Evidencia | Resultado |
|---|---|
| Core Full local | 8,174 passed; 0 failed; 0 skipped |
| Debug Plugin local | success; 0 errors; solo los 2 `MSB3277` conocidos de referencias AutoCAD |
| UI local | no requerida para T1 por LC-UI; no ejecutada como sustituto de CI |
| CI push exacta | run `35486666793`; event `push`; ref `architecture/shared-view-foundation`; head exacto; 4/4 jobs success |
| CI Core | `Tests (Domain + Application)` success |
| CI UI | `UI Tests (WPF controls, net8.0-windows)` success |
| CI build UI | `Build UI (WPF, valida API de Application)` success |
| CI build Plugin | `Build Plugin without AutoCAD` success |

Run: <https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/35486666793>

F2 queda `COMPLETE`. Este recibo no abre F3, no declara Candidato, no sustituye T2/Owner Validation y no autoriza
integracion.
