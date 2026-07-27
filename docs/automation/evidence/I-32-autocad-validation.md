# I-32 — Validación manual en AutoCAD 2025 (Owner)

Estado: **APROBADA**. El dueño del repositorio ejecutó la **revalidación manual dirigida de la geometría
asimétrica** de Push Back y confirmó: **«Listo, todo correcto»**.

## Identificación

| Campo | Valor |
|---|---|
| Iniciativa | I-32 — Correcciones funcionales y geométricas de Push Back |
| Rama | `fix/correcciones-push-back` |
| Claim-Id | `5bb26f55-365c-449b-8341-59dc955e5807` |
| **CODE_SHA funcional** | `f911d75350702fb176e123a59a105d40f63690ec` |
| **VALIDATED_BUILD_SHA** (el que el Owner cargó) | `a0c3f27c2447a4e1f85707ef9f3ad311765e3a43` |
| **DLL SHA-256** | `B7B15802D19C90BBE40B19546423F9CC1850645051C1DA971DA2552778B2E931` |
| `AssemblyInformationalVersion` | `1.0.0+a0c3f27c2447a4e1f85707ef9f3ad311765e3a43` |
| DLL Debug | `<worktree>/src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll` |
| Tamaño / fecha | 115 200 bytes · `2026-07-27T00:13:52.5389712Z` |
| CI del candidato | run [`30226757221`](https://github.com/marioap-afk/Calculadora_de_racks/actions/runs/30226757221) — **success**, 4/4 jobs sobre `a0c3f27` |
| Base | `origin/main = 91eb53cf58d225f1e34e8f4cb45da27e1cdd1884` (no avanzó; **sin rebase final**) |
| Fecha de aprobación | 2026-07-27 |
| Requisitos | `requires_autocad: true`, `requires_owner_validation: true` |

## Los tres SHA, y por qué el traspaso de la aprobación es válido

Conviene distinguirlos expresamente, porque no son el mismo:

| SHA | Qué es |
|---|---|
| `f911d75` | **CODE_SHA funcional**: la última punta que cambió `src/**` o `tests/**`. Es el código que la revisión técnica aprobó. |
| `a0c3f27` | **VALIDATED_BUILD_SHA**: la punta desde la que se compiló el DLL que el Owner **realmente cargó** en AutoCAD. `a0c3f27` es `f911d75` más un commit **exclusivamente documental** (contrato, decisiones y estado). |
| *(este cierre)* | **HEAD documental de `integration-ready`**: los commits de esta corrida, también **exclusivamente documentales**. |

La aprobación se traspasa a la nueva punta porque **todo commit posterior a `a0c3f27` en esta corrida es
documental**: no toca `src/**`, `tests/**`, `assets/**` ni el binario. El árbol funcional que el Owner
validó es byte a byte el que se integrará. El DLL **no se recompila ni se reemplaza**: el artefacto
aprobado sigue siendo el de `a0c3f27` con SHA-256 `B7B15802…`.

## Resultado

El Owner **aprobó** la revalidación: **todos los puntos correctos**. Con ello los gates **`autocad`** y
**`owner-validation`** quedan **resueltos** (y `plugin_build` verde, 0 errores y solo los `MSB3277`
conocidos). La iniciativa pasa a **`integration-ready`**.

Esta validación **no se numeró como otro round**: `max_attempts` se agotó con los rounds 1, 2 y 3, así que
fue una revalidación dirigida explícitamente por el Owner sobre una corrección que él mismo dirigió.
`attempts` permanece en **3**.

## La regla geométrica final que quedó aprobada

1. **Entrada/Salida** — mate `LARGUERO_IN_OUT.TROQUEL_CAMA` ↔ `RIEL_DE_CINTA_CALIBRE_12.TROQUEL_IN`.
2. **Posterior e intermedios** — tangentes a la **línea del ORIGEN** del bloque, una recta paralela a la
   anterior y distinta de ella.
3. **Una sola `RotationRadians`** para todo el bloque, resuelta por `PushBackBedRotation`.
4. **El larguero posterior es el ANCLA** y queda fijo en su troquel.
5. **El larguero bajo se elige globalmente** por menor error contra 7/192, sobre la retícula de 2".
6. **`LONGITUD` = fondo estructural completo** (full-span).

Quedan igualmente aprobadas las **botas** y los **protectores laterales** (primer poste delante sin
espejo, último delante espejado, interiores vacíos).

## Historial de validaciones — no se reescribe

| Validación | Commit | DLL SHA-256 | Resultado |
|---|---|---|---|
| Round 1 | `2210e67` | `B3A2D87E4B18E9D9B0C19E4C881C76D576D8A52AB2607DF1E98042ACC878C653` | **RECHAZADA** |
| Round 2 | `557858d` | `3118DCB7498F77A60790B88D621CC4CD9A59532034F8796D80FB39D2936C013F` | **RECHAZADA** |
| Round 3 | `2641830` | `3034B908CE7593EA7FF9ED1C5B0DB57B40F77C486B2A24B32556E8381AF4B111` | **RECHAZADA** |
| Confirmación final | `9a87c7c` | `8FCF4C92A353F8953AD110F472846506EC4F93846BABB61475BD4FAF7AE751F4` | **RECHAZADA** |
| **Revalidación dirigida** | **`a0c3f27`** | **`B7B15802D19C90BBE40B19546423F9CC1850645051C1DA971DA2552778B2E931`** | **APROBADA** |

**Los cuatro DLL anteriores permanecen OBSOLETOS** y no deben reutilizarse: la geometría cambió después de
cada uno.

## Hallazgos diferidos — no bloquean

**PB-001** (preview de las cuatro vistas), **PB-007** (reconfigurador masivo de seguridad), **PB-011**
(editor avanzado de módulos) y **PB-014** (frente en blanco) siguen **diferidos** en
[`ideas-futuras.md`](../../ideas-futuras.md). Nunca estuvieron en el alcance de I-32 y **no bloquean la
integración**.

## Siguiente acción

**Integración serializada en `main`** (`git merge --no-ff`), y limpieza posterior de rama y worktree. No
se ha integrado ni limpiado nada todavía.
