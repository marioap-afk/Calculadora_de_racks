# I-67 — Proposal V2: elevaciones de niveles en Dinámico y Push Back (H58)

Frozen: NO

Unit: `I-67`. Workflow: V2. Archetype: FOUNDATION EVOLUTION (M-03, M-05; eleva el EXTENSION inicial). Rama `fix/elevaciones-niveles`, base `a140eb3a`.
Discovery: [evidencia](../automation/evidence/I-67-evidence.md). Contrato: [I-67-elevaciones-niveles.md](I-67-elevaciones-niveles.md). Decisiones del Owner: [decisions/I-67.md](../automation/decisions/I-67.md) §6.

Sustituye a la Proposal V1. Una vez congelada es inmutable; todo cambio posterior es una enmienda `A-n` append-only (LIFECYCLE §6).

## 1. Problema

H58, **REPRODUCED**: con requisito 45" y retícula de 2", Dinámico y Push Back resuelven 44/46/44 en lugar de 46/46/46. La causa es doble: cada nivel se
ajusta al troquel más cercano, que puede quedar por debajo del requisito, y cada nivel se calcula desde una acumulación teórica que no ve la elevación real del anterior.

Regla de producto del Owner: **cada intervalo se ajusta hacia arriba al primer troquel real que satisface o supera su separación requerida, desde la elevación real resuelta del nivel anterior.**

## 2. Autoridad

Sin cambio de dueño y sin autoridad nueva:

- `DynamicLoadBeamGeometry.ResolveLevels` sigue siendo la única que resuelve elevaciones de salida y entrada por nivel. Sus dos sobrecargas comparten una sola rutina.
- La retícula sigue llegando de `DynamicRackSystemResolver` (`TROQUEL_LARGUERO` del poste, 0.6053" en el poste estándar, y `TroquelPaso`). Ninguna constante de origen.
- `RackFirstLevelDatum` sigue siendo dueño del primer nivel (ADR-0031).
- `PushBackElevations` sigue siendo dueño del larguero posterior (ADR-0031 §9) y no se modifica.

Las vistas, cotas, BOM y metadata siguen consumiendo los niveles resueltos. Ninguna recibe una regla propia.

## 3. Comportamiento

Con `p` = paso, `b` = base de la retícula, `S` = pendiente nominal del frente y `Req(i)` = `InOutBeamDepth(i) + Pallet.Height(i) + ClearHeight(i)` (la misma suma de hoy, de la celda `i`):

```text
Salida(1)    = TroquelMasCercano(RawFirst)                          sin cambio (I-42, ADR-0031)
Entrada(1)   = TroquelMasCercano(RawFirst + max(0, S))              sin cambio
Subida       = Entrada(1) - Salida(1)
Salida(i+1)  = PrimerTroquelEnOSobre(Salida(i) + max(p, Req(i)))
Entrada(i)   = Salida(i) + Subida                                   para i >= 2
PrimerTroquelEnOSobre(v) = b + ceil((v - b) / p - 1e-9) * p
```

- **B-1. Nunca hacia abajo.** Ningún intervalo resuelto queda por debajo de su requisito.
- **B-2. Troquel exacto.** Un valor ya en troquel se conserva, sin subir otro paso (épsilon `1e-9` en pasos, el del Selectivo).
- **B-3. Por intervalo.** Cada intervalo usa su propio requisito; no se impone separación uniforme.
- **B-4. Referencia real.** La acumulación parte de la salida resuelta del nivel anterior. `Math.Round` no se sustituye sin corregir la referencia.
- **B-5. Primer nivel.** Su salida y su entrada no cambian. No es un intervalo y su regla pertenece a ADR-0031.
- **B-6. Pendiente Dinámico (D-PEND, decisión del Owner).** Se conserva la subida del nivel 1 en todos los niveles del frente. No hay pendiente nominal, redondeo ni cálculo de subida nuevos: desaparece solo la alternancia, que era el mismo defecto de acumulación teórica. Las camas quedan paralelas y los intervalos del extremo de entrada igualan a los de salida.
- **B-7. Push Back.** La salida es el ancla baja y `ChooseHighTroquel` deriva el posterior sin modificación. No se toca la pendiente ni los extremos frontal y posterior.
- **B-8. Sin retícula medida** (`p <= 0` o base no finita): no se ajusta y la salida es la suma directa, como hoy.
- **B-9. Valores degenerados, defensivo.** Con retícula, un requisito no positivo sube un paso (mismo piso que `SelectiveGeometryResolver.Separation`). Es inalcanzable en producción (`Positive`, `NonNegative`, `Validate`), así que no cambia ningún resultado observable. Un valor no finito se propaga igual que hoy, sin excepción nueva.
- **B-10. Altura.** `CalculateResolved` y `PushBackHeaderHeight` no cambian y consumen la elevación real. Medido sobre 36,720 casos: la cabecera automática sube un pie comercial o más en 7,910 y no baja en ninguno.

**Racks sin el defecto.** Si todos los requisitos son múltiplos del paso, las salidas y las entradas son idénticas a las de la regla anterior (INV-16).

## 4. Persistencia y documentos existentes

**Sin cambio de esquema, de DTO ni migración.** Las elevaciones no se persisten y se siguen recalculando desde las entradas authored.

**D-LEG (decisión del Owner).** La regla se aplica a todo diseño que se resuelva, nuevo o existente. No se añade marcador de versión.

| Ruta | Efecto en un rack existente |
|---|---|
| Abrir, guardar, reabrir, `RACKDUPLICAR` | ninguno: no re-resuelven; la geometría dibujada no se toca |
| `RACKEDITAR` + **Actualizar** | el editor muestra los niveles corregidos y redibuja todas las vistas ligadas |
| `RACKEDITAR` + **Insertar** vista | la vista nueva sale corregida y las hermanas se redibujan |
| `RACKPROYECTAR` | la vista nueva sale corregida; las hermanas no se tocan y conservan la geometría anterior hasta **Actualizar**, sin aviso |
| BOM (`RACKBOM`, `RACKBOMTOTAL`) | resuelve desde el diseño: puede reflejar una cabecera mayor antes de que se actualice el dibujo |

Dirección del cambio, medida: el primer nivel no se mueve y ninguna salida ni entrada baja respecto de la regla anterior. Una salida puede subir hasta 10" en 7 niveles.

## 5. Semántica de fallo

Sin cambios (M-04 no activado; B-9 no alcanza producción).

## 6. No-objetivos

Rediseño de Dinámico o Push Back, representación de tarimas, UI nueva, reglas nuevas de pendiente, Dynamic Rebuild Robustness, cambios estructurales/RAM, Workspace/ID30, migraciones,
refactors oportunistas, primer nivel y datum, Selectivo y Cantilever, avisar del estado mixto, y validar alturas fijadas a mano contra el último larguero (se registra en `ideas-futuras`).

## 7. Puntos de extensión y materialidad

Ninguno añadido, cambiado ni retirado. `Consumes`: `RackFirstLevelDatum` y la autoridad vertical de Push Back. `Extends`: ninguno. `Introduces`: ninguno.

M-03 y M-05 activados, ambos con decisión registrada. M-08 **no activado**: con D-PEND el nivel 1 no cambia, de modo que «ningún archivo se reinterpreta ni se mueve» de ADR-0031 sigue siendo cierto para lo que ese ADR gobierna, el primer nivel. ADR no requerido: ninguno de los cuatro criterios de `docs/adr/README.md` aplica.

Los comentarios que afirman que ningún archivo cambia de geometría (`DynamicRackSystemResolver`, `RackFirstLevelDatum`, `DynamicRackSystemDocument`) y la guía manual §5.6 («legacy … EXACTAMENTE igual») se acotan al datum y al alcance que realmente garantizan.

## 8. Invariantes y pruebas

Toda prueba nueva se ve fallar antes del arreglo, o por la violación temporal nombrada si pasa sobre el producto actual. Las pruebas usan la base real 0.6053" salvo donde el origen sea justamente lo que se varía.

| ID | Invariante | Prueba | RED |
|---|---|---|---|
| INV-01 | H58 uniforme: 46/46/46 | T1 | falla (44/46/44) |
| INV-02 | Troquel exacto sin paso extra, también con ruido de coma flotante | T2 | violación: quitar el épsilon |
| INV-03 | Intervalos no uniformes: cada uno, su primer troquel | T3 | falla |
| INV-04 | Tres o más intervalos sin alternancia | T4 | falla |
| INV-05 | Origen y paso son datos: base real del catálogo y al menos otra base, otro paso | T5 | falla |
| INV-06 | Con `Req > 0`: `Spacing(i) >= Req(i)` y `Spacing(i) < Req(i) + p`, con tolerancia | T6 | falla |
| INV-07 | Cotas y piezas dibujadas del Dinámico usan las mismas elevaciones | T7 | falla |
| INV-08 | Separaciones Push Back, simple y compuesto en ambos lados | T8 | falla |
| INV-09 | Pendiente Push Back, a igual fondo por celda: posterior derivado sin cambio, subida y rotación iguales en todos los niveles | T9 | caracterización; violación: subir un troquel el posterior en un nivel |
| INV-10 | Edit/update: `Snapshot` + `Resolve` idempotente | T10 | falla |
| INV-11 | Save/reopen: ninguna elevación en el JSON, mismo resultado al reabrir; editar tras reabrir sigue la regla | T11 | falla |
| INV-12 | D-LEG: primer nivel intacto, ninguna salida ni entrada baja, inputs authored intactos | T12 | caracterización del nivel 1; violación: `Floor` en lugar de `Ceiling` |
| INV-13 | D-PEND: `Entrada(1)` histórica, subida uniforme, separación de entrada ≥ requisito | T13 | falla (alterna) |
| INV-14 | Degenerados y sin retícula, deterministas y sin excepción | T14 | falla los degenerados; violación: quitar el piso |
| INV-15 | La cabecera automática contiene el último nivel | T15 | caracterización; violación: medir con la entrada del primer nivel |
| INV-16 | Requisitos múltiplos del paso: salidas y entradas idénticas a la regla anterior | T16 | caracterización; violación: `Entrada(i)` al troquel más cercano a la nominal |
| INV-17 | Paridad de las dos sobrecargas con retícula real | T17 | falla |

Las pruebas `ALegacyDocument_KeepsItsPhysicalGeometry` y su gemela dinámica se conservan sin cambio. Toda prueba existente que hoy fije el defecto se actualiza y se lista en la evidencia con el motivo.

## 9. Owner Validation

Requerida: cambia dibujo. Sobre `FINAL_CANDIDATE_SHA`, con el DLL del worktree, además del checklist aplicable de la guía manual §5.

| ID | Escenario | Resultado esperado |
|---|---|---|
| OV-I67-01 | Dinámico nuevo: tarima 33", C6, 4 niveles, primer nivel 6" | 46/46/46 en lateral y cotas |
| OV-I67-02 | Dinámico con tarimas distintas por nivel | cada intervalo, su primer troquel ≥ requisito |
| OV-I67-03 | Dinámico: salida y entrada en vistas frontales y lateral | mismas elevaciones; subida igual en todos los niveles |
| OV-I67-04 | Push Back nuevo con los datos de OV-I67-01 | 46/46/46; cama y largueros frontal y posterior coherentes, pendiente como antes |
| OV-I67-05 | Push Back compuesto A/B | ambos lados con la regla; sin cambio de la política de pendiente |
| OV-I67-06A | Rack **Dinámico** de un DWG creado con un build anterior a I-67, requisito impar: abrir y guardar; luego `RACKEDITAR` + Actualizar | sin cambios al abrir y guardar; Actualizar lo corrige, todas las vistas coinciden y el nivel 1 no se mueve |
| OV-I67-06B | Igual que 06A con un rack **Push Back** | igual que 06A; pendiente como antes |
| OV-I67-07 | Guardar, cerrar, reabrir y editar | valores conservados; editar sigue la regla |
| OV-I67-08 | BOM del rack actualizado | altura y postes coherentes con el dibujo |
| OV-I67-09 | Estado mixto de D-LEG, sobre un rack existente sin actualizar: BOM y `RACKPROYECTAR` | el BOM y la vista nueva usan la regla; las hermanas siguen igual hasta Actualizar |
| OV-I67-10 | Rack con requisito par (sin el defecto) | no cambia en nada |

## 10. Gates

Consolidados, porque el cambio es una función y sus consumidores no cambian:

- **F0** preflight, caracterización y Freeze.
- **G1** (F1+F2+F3 de la orden): corrección, regresión Dinámico, impacto Push Back, persistencia, edición, vistas y cotas. Cierra con INV-01..17 en verde, Core Full local sobre el SHA de cierre con árbol limpio y CI exacta del SHA.
- **READY** (F4): READY-01..09, conformidad Architect + Coordinator, Candidato y Owner Validation.

Sin merge a `main` antes de una Owner Validation aprobada y orden de integrar.

## 11. Delta respecto de V1 y disposición

| ID | Disposición |
|---|---|
| R-01 | D-LEG decidida por el Owner antes del Freeze (decisions §6). OV-I67-06 verifica; ya no decide. `requires_owner_decision: true` |
| R-02 | D-PEND decidida por el Owner: subida del nivel 1 en todos los niveles. B-6, B-10, D-LEG, INV-13 y OV reescritos; razonamiento de V1 retirado |
| R-03 | M-08 reevaluado con la premisa ya cierta (§7) |
| R-04 | Base real 0.6053" en evidencia, §2, §8 e INV-05. Ejemplo «12.5» retirado |
| R-05 | Tabla D-LEG en filas separadas. OV-06A/06B explícitas con DWG anterior; OV-09 cubre BOM previo y estado mixto; OV-10 racks sin defecto |
| R-06 | Violación nombrada para T9, T12, T14, T15, T16; T17 prueba la paridad con retícula real |
| O-01 | No aplica: la subida se calcula una sola vez por frente |
| O-02 | B-8 ya es «como hoy»; B-9 declarado defensivo e inalcanzable |
| O-03 | INV-06, INV-09 e INV-12 acotados |
| O-04 | Retirado «un paso»; con D-PEND la entrada no baja y la cabecera no baja (medido) |
| O-05 | La nota en `ideas-futuras` cubre altura manual, `HeaderLineOverrides` y alturas de poste derivado |
| O-06 | Comentarios y guía se acotan en G1 (§7) |
