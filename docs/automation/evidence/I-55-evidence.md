# I-55 — Evidencia canonica de cierre (View Placement & Projection: ID17 + ID18 + ID19)

Iniciativa `I-55`, Workflow V1. Rama `feature/creacion-de-vistas`. Registro de decisiones: [decisions/I-55.md](../decisions/I-55.md). Matriz y rondas de
validacion del Owner: [I-55-g16-owner-validation.md](../../initiatives/I-55-g16-owner-validation.md). El `CLOSURE_SHA`, el `MERGE_SHA`, el CI posterior al merge,
la cobertura del merge y la limpieza viven en el tag anotado `integration/I-55` (este archivo forma parte del cierre y no puede contener su propio SHA).

## 1. Candidato final

```text
FINAL_I55_CANDIDATE_SHA: 6dcd65959c48495dcf439b267d0fca68de9807b0
Base (origin/main):      1304101d997b66748e21f4896ac203d6fdc6a1f1 (I-52-AUTH15-C1 integrada)
Arbol limpio:            SI
SDK resuelto:            8.0.423
Core Full local:         PASS 12281/12281 (0 omitidas)
UI Full local:           PASS 1636 correctas, 0 fallos, 17 omitidas historicas (sin omisiones nuevas)
Debug UI build:          PASS (0 errores)
Debug Plugin build:      PASS (0 errores, --no-incremental)
Release Plugin build:    PASS (0 errores)
CI exact SHA:            GREEN (run 36727291676, event push, 4/4: Tests, UI Tests, Build UI, Build Plugin)
Candidate coverage:      run 36727916198, event workflow_dispatch, measured_sha = 6dcd65959c48495dcf439b267d0fca68de9807b0,
                         artifact rackcad-coverage-cobertura 11103363296 sha256:1f0c7fd8a020df11186d3575dd338e0e42fded830b342be682387274000022ae,
                         lineas 90.86 %, ramas 79.27 %
Owner validation:        APPROVED (2026-09-30)
DLL validado:            ProductVersion 1.0.0+6dcd65959c48495dcf439b267d0fca68de9807b0
                         SHA-256 8994E500513BBE2E68FC23714989B5CE462C0704F7409AAA13A5A2CADE125FF7
Biblioteca de bloques:   D:\Base_de_datos_AutoCAD_V.0.dwg (override de settings.json) SHA-256 B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8 (observada al entregar)
```

Pruebas focalizadas sobre el Candidato (todas en verde): C16-06 309, C16-07 98, racks sin nombre 28, G14 158, G15 97, ID17 108, ID18 17, AUTH15 76,
pares/exposicion 41.

## 2. Validacion del Owner

**OWNER VALIDATION = APPROVED** sobre el Candidato exacto `6dcd65959c48495dcf439b267d0fca68de9807b0` con el DLL de la tabla anterior. Resultado reportado por
el Owner: **todos los grupos canonicos PASS** — OV-LEG, OV-BOM, OV-ID17, OV-ID18, OV-RED, OV-META, OV-PR, OV-ID19, OV-C16-06, OV-C16-07 y OV-UNNAMED. No se
registran detalles por fila mas alla de ese resultado global (ronda 7 de la matriz).

## 3. Rondas correctivas de G16

| Ronda | Candidato | Resultado | Correccion |
|---|---|---|---|
| 1 | `beb9597b` | REJECTED (OV-ID17) | C16-01 (lista de vistas vacia), C16-02 (definiciones de otro rack en RACKEDITAR) |
| 2 | `bc622f12` | REJECTED (OV-ID19-01) | sobre proyectado sin `Name` |
| 3 | `50f6c8bf` | REJECTED (OV-ID19-01) | C16-04: nombre base del lateral Selectivo sin nombre (AUTH-11) |
| 4 | `66a9a504` | REJECTED por el Coordinador | C16-05 (rack sin nombre no proyectable) |
| 5 | `3f06994a` | REJECTED por decision de producto | C16-05 revocada; AUTH-15-C1 (Name opcional); C16-06 orientacion Proyectada/Predeterminada; OV-ID18-10 corregida |
| 6 | `68115269` | REJECTED (Frontal → Lateral = `PairNotExposed`) | C16-07: Frontal ↔ Lateral ortografico sobre la altura |
| 7 | `6dcd6595` | **APPROVED** | — |

## 4. Producto integrado

- **ID17 — primera vista libre:** un rack nuevo empieza por cualquier vista que su sistema soporte, sin perder `RackId` ni datos.
- **ID18 — cola de vistas:** varias vistas iniciales o de un rack existente en una operacion, con redibujo atomico de hermanas (OV-RED).
- **ID19 — `RACKPROYECTAR` / `RPY`:** proyecta racks ya dibujados como vistas enlazadas del mismo rack (mismo `RackId`, sin cambio de BOM), con un plan puro
  (todo bloqueo antes de los puntos), una `CommonTransform2D`, una transaccion del llamador y un `Commit` sobre AUTH-15 (`RackDefinitionCreator`).
  - **Pares:** misma clase (rigido), Planta ↔ Frontal (corrida), Planta ↔ Lateral (fondo) y **Frontal ↔ Lateral (altura, C16-07)**; `PairNotExposed` cuando
    el rack no ofrece la clase destino (cabecera → Frontal).
  - **Orientacion (C16-06):** Proyectada por defecto (solo gira la referencia de bloque, entera; `ρ` derivado de AUTH-05/AUTH-08) o Predeterminada
    (presentacion normal); divergencias tipadas antes de los puntos (`SourceOrientationDivergent`, `SourceRotationsDiffer`, `NonParallelSources`).
  - **Racks sin nombre:** se proyectan y siguen sin nombre (C16-05 retirada; AUTH-15-C1 consumida).
- Sin cambios de Foundation ni de esquema.

## 5. Estado de los hallazgos

```text
ID17 = COMPLETE
ID18 = COMPLETE
ID19 = COMPLETE
C16-01 = RESOLVED
C16-02 = RESOLVED
C16-04 = RESOLVED
C16-05 = SUPERSEDED / REMOVED
C16-06 = COMPLETE
C16-07 = COMPLETE
AUTH-15-C1 = INTEGRATED / CONSUMED
PR-1 = RESOLVED
PR-2 = RESOLVED
G12-CR-01 = RESOLVED BY I-58
G14-CR-01 = RESOLVED BY I-59
G14-CR-02 = RESOLVED BY I-59
```

## 6. Residuales (no bloquean; fuera de alcance)

- La superposicion entre vistas proyectadas por la altura solo se avisa cuando las lineas de suelo coinciden: la altura de una vista no es un hecho de
  AUTH-05 (diseño C16-07 §D).
- La cabecera Planta → Frontal informa `PairNotExposed` (antes `TargetAddressUnavailable`) y la cama Lateral → Frontal `TargetNotExposed`; ambos en `AVAILABLE`,
  sin puntos, con el mismo remedio (diseño C16-07 §B; autocomprobacion).
- `RACKDUPLICAR` y `RACKLAYOUT` no cambian. La unidad I-60 (nombre automatico de racks nuevos) debe reconciliarse sobre el `main` con I-55 (su Freeze preve la
  enmienda para la ruta de lote de ID18).

## 7. Preparacion para integrar

Candidato aprobado, CI exacto y cobertura exacta del Candidato verdes; el commit de cierre es solo documental y lleva su propio Core/UI/builds/CI; `origin/main`
esperado `1304101d` sin avanzar al cerrar. Merge `--no-ff` serial, CI posterior al merge con cobertura, tag `integration/I-55` y limpieza segun WORKFLOW §4.5.
