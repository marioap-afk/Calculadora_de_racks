# I-55 G16 C16-07 — Proyeccion entre elevaciones (Frontal ↔ Lateral)

Frozen: YES

Unidad: I-55 (Workflow V1), gate G16, ronda correctiva C16-07. Rama `feature/creacion-de-vistas`, base del diseño `68115269` (Candidato RECHAZADO
para la aceptacion final por el Owner). Registro: [decisions/I-55.md](../automation/decisions/I-55.md) «C16-07». Diseño previo del que este depende:
[C16-06](I-55-c16-06-projected-orientation.md) (orientacion Proyectada/Predeterminada; su autoridad pura se reutiliza sin cambio).

Una vez congelado es inmutable; todo cambio posterior es una enmienda en el registro de decisiones.

## 1. Problema y decision del Owner

En la validacion del Owner sobre `68115269`, `RACKPROYECTAR` de un Selectivo Frontal → Lateral se detiene en la etapa `AVAILABLE` con `PairNotExposed`
(con y sin nombre: no es un defecto de nombre ni de AUTH-15). La causa es la regla congelada de G14 (OD-7.a A): «planta ↔ elevaciones = ortografica; frontal ↔
lateral no se expone».

**Decision del Owner (2026-09-30):** esa exclusion queda **REVOCADA**. `RACKPROYECTAR` debe admitir Frontal → Lateral y Lateral → Frontal, en Proyectada y en
Predeterminada, en todo sistema que tenga ambas clases; no es un caso especial del Selectivo. Se cambia la politica de exposicion de pares, no se debilita la
validacion generica de `AVAILABLE`. `PairNotExposed` sigue siendo el fallo de una combinacion realmente no soportada.

## 2. Caracterizacion (antes de C16-07)

Marcos AUTH-05 reales (`RackViewFrameAdapters`), rotacion AUTH-08 V2 (`RackSourcePlacementAcceptance.Linear`, giro o media vuelta; la reflexion se rechaza en
CLASSIFY) y el resolutor de C16-06:

| Sistema | Frontal: variante, ejes (X, Y) | Lateral: variante, ejes (X, Y) | Eje fisico horizontal Frontal / Lateral | Eje compartido | Hoy |
|---|---|---|---|---|---|
| Selectivo | `Fondo(i)`, Run, **Height** (+), origen (poste 0, fondo i, **0**) | `Post(j)`, Depth, **Height** (+), origen (corte j, 0, **0**) | Run / Depth | **Height** (+Y local en ambas, suelo = 0) | `PairNotExposed` |
| Dinamico | `FlowEnd`, Run, Height (+), altura 0 | `Post(j)`, Depth, Height (+), altura 0 | Run / Depth | Height | `PairNotExposed` |
| Push Back | `PushBackCut`, Run, Height (+), altura 0 | `Post(j)`, Depth, Height (+), altura 0 | Run / Depth | Height | `PairNotExposed` |
| Cantilever | `Whole`, Run, Height (+), altura 0 | `Station(k)`, Depth, Height (+), altura 0 | Run / Depth | Height | `PairNotExposed` |
| Cabecera | **no existe** (codec, adaptador `UnsupportedAddress`, disponibilidad) | `Whole`, Depth, Height (+) | — / Depth | no aplica | `PairNotExposed` |
| Cama | fuera de ID19 (se rechaza en la decision de la fuente) | — | — | — | sin cambio |

Hechos derivados de la caracterizacion (no supuestos):

- Frontal y Lateral **no comparten ningun eje horizontal** (Run frente a Depth). El unico eje fisico comun es **Height**, que en las cuatro familias es el eje
  local +Y de ambas elevaciones, con el suelo en la altura 0 del origen del marco.
- El resolutor de C16-06 es generico sobre el eje conservado: con Height, `s = t = (0, +1)` en todas las familias, asi que Proyectada da `ρ = ang(t → d)` con
  `d = L·s`, es decir **`ρ = θ`** (la referencia nueva conserva el giro de la fuente). No es «+90°»: es lo que dan los marcos. Predeterminada = marcos universales.
- La politica ortografica mide el tramo conservado solo sobre el X local de un marco (`K` de AUTH-05). Height nunca es el X local de una elevacion: sin
  cambio, el par devolveria `FrameAxisMissing`.
- La lista de direcciones destino del rack (`RackProjectionResolveOutcome.AvailableTargetAddresses`, calculada por el Plugin con la disponibilidad fisica)
  ya dice que clases ofrece cada sistema: la cabecera no ofrece ninguna Frontal.

## 3. Contrato congelado

### A. Politica cerrada de pares de clases

| Fuente → destino | Modo | Eje conservado |
|---|---|---|
| misma clase | Rigido (sin cambio) | — |
| Planta ↔ Frontal | Ortografico (sin cambio) | Run |
| Planta ↔ Lateral | Ortografico (sin cambio) | Depth |
| **Frontal ↔ Lateral** | **Ortografico (nuevo)** | **Height** |

### B. Aplicabilidad por sistema

En un par ortografico, si el rack **no ofrece ninguna direccion** de la clase destino (`AvailableTargetAddresses` sin esa clase) el fallo es `PairNotExposed`
en `AVAILABLE`, antes de pedir puntos: es la combinacion realmente no soportada (cabecera con Frontal). Si la clase existe pero ninguna direccion es aceptada por
la politica de producto, el fallo sigue siendo `TargetAddressUnavailable`; si la fuente no esta expuesta para la operacion, sigue `TargetNotExposed`/
`SourceAddressUnavailable` (orden de comprobacion sin cambio: la decision de la fuente va antes). Consecuencia aceptada: la cabecera Planta → Frontal pasa de
`TargetAddressUnavailable` (cuyo remedio hablaba de vistas huerfanas) a `PairNotExposed` («esta vista no se puede proyectar»). La cama no cambia.

### C. Tramo conservado sobre Height

Los marcos solo miden `K` sobre su X local. Para un eje que ninguna de las dos vistas mide (Height, el unico que comparten las elevaciones) el tramo se reduce a
la **linea de suelo**: `min = max = altura fisica del origen del marco fuente` (0 en todos los sistemas). El punto fuente es el del marco fuente en esa altura
(su ancla a nivel de suelo) y el punto destino el del marco destino en la misma altura fisica. Los pares Run y Depth no cambian.

### D. Aviso de superposicion

Dos referencias con tramo de longitud 0 cuyas coordenadas sobre la linea comun coinciden (tolerancia `GeometryTolerance.Length`) quedan en la misma ancla y se
superponen: aviso `Overlap` (no bloquea), igual que las filas escalonadas de G14. Si no coinciden no hay aviso (la altura de la vista no es un hecho de AUTH-05).

### E. Orientacion (misma autoridad pura de C16-06, sin cambio)

- **Proyectada:** `ρ = ang(t → d)`, `d = L·s`, `s = t = +Height`: **`ρ = θ`** en Frontal → Lateral y en Lateral → Frontal, en las cuatro familias; `e = w = d`,
  `α = 0`. Compatibilidad (C16-06 E): primero paralelismo (`NonParallelSources`), despues sentido (`SourceOrientationDivergent`), ambos antes de pedir puntos.
- **Predeterminada:** marcos universales de G14: la vista nueva a 0° (presentacion normal); la linea comun sale de la ventana relativa sobre el eje Height de la
  fuente. Nunca se rechaza por la divergencia de Proyectada.

### F. Mismo modelo de ID19

Solo cambian la politica de pares (A, B) y el tramo/aviso de la politica ortografica (C, D). Sin camino de comando nuevo: una `CommonTransform2D`, una operacion,
mismo `RackId`, una transaccion del llamador, AUTH-15, rollback, avisos, requisitos estrictos de bloques y preguntas sin cambio. La variante destino es la
canonica del rack (prioridad existente: lateral del poste 0 / estacion 0; frontal del fondo 0, salida, Entrada/Salida A, `Whole`).

### G. Sin cambios de definicion ni de esquema

Solo gira la referencia de bloque, entera. Sin cambios de Foundation, AUTH-05, AUTH-08, AUTH-15, esquema ni persistencia.

### H. Supersesion

OD-7.a A queda sustituida en su clausula «frontal ↔ lateral no se expone» por decision del Owner. La prueba G14 que afirmaba `TryMap(Frontal, Lateral) = false`
y los textos de las guias que decian «no se expone» se reescriben con esta politica.

## 4. Obligaciones invariante → prueba (RED antes de implementar)

| ID | Invariante | Prueba | RED esperado |
|---|---|---|---|
| Q-1 | Frontal ↔ Lateral = ortografico sobre Height; el resto de la tabla A sin cambio | politica de pares | falla |
| Q-2 | Selectivo Frontal → Lateral y Lateral → Frontal: hoy `AVAILABLE`/`PairNotExposed`; despues completa, una definicion y una referencia, mismo `RackId` | comando (G15) | falla con `PairNotExposed` |
| Q-3 | Proyectada, Selectivo: Frontal 0/90/180/270 → Lateral y Lateral 0/90/180/270 → Frontal con `ρ = θ`, `e = w = d`, `α = 0`, posicion exacta | comando | falla |
| Q-4 | Predeterminada representativa en ambos sentidos: `ρ = 0`, posicion exacta; igual a Proyectada salvo el giro | comando | falla |
| Q-5 | Marcos reales de Selectivo, Dinamico, Push Back y Cantilever, ambos sentidos, 4 giros: `R(ρ)·t = d`, proyeccion pura, Predeterminada universal | autoridad | falla |
| Q-6 | Cabecera Lateral → Frontal y Planta → Frontal: `PairNotExposed` en `AVAILABLE`, sin puntos; Frontal existente pero rechazada: `TargetAddressUnavailable` | comando/pipeline | falla (cabecera Planta → Frontal) |
| Q-7 | Varios racks: mismo giro → un giro comun; misma linea de suelo → misma ancla + `Overlap`; suelos distintos → separados por la diferencia; opuestos → `SourceOrientationDivergent` antes de puntos (Predeterminada procede); no paralelos → `NonParallelSources` | comando/politica | falla |
| Q-8 | Conservaciones: un `Commit`, una transformacion, ningun punto antes de un fallo, rack sin nombre sigue sin nombre, `PairNotExposed` existe con su remedio | comando | falla (la parte de Frontal ↔ Lateral) |

## 5. Matriz de Validacion del Owner (OV-C16-07)

Vive en [I-55-g16-owner-validation.md](I-55-g16-owner-validation.md) §6.3: OV-C16-07-01..06 (las del Owner) y 07..13 (representativas por sistema, cabecera,
varios racks, divergencia y rack sin nombre). Ninguna se marca PASS de forma automatica.
