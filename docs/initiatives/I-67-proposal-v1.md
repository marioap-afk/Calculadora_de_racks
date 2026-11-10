# I-67 — Proposal V1: elevaciones de niveles en Dinámico y Push Back (H58)

Frozen: NO

Unit: `I-67`. Workflow: V2. Archetype: FOUNDATION EVOLUTION (M-03, M-05; eleva el EXTENSION inicial). Rama `fix/elevaciones-niveles`, base `a140eb3a`.
Discovery: [evidencia](../automation/evidence/I-67-evidence.md). Contrato: [I-67-elevaciones-niveles.md](I-67-elevaciones-niveles.md).

Una vez congelada es inmutable; todo cambio posterior es una enmienda `A-n` append-only (LIFECYCLE §6).

## 1. Problema

H58, **REPRODUCED**: con requisito 45" y retícula de 2", Dinámico y Push Back resuelven 44/46/44 en lugar de 46/46/46. La causa es doble: cada nivel se
ajusta al troquel más cercano, que puede quedar por debajo del requisito, y cada nivel se calcula desde una acumulación teórica que no ve la elevación real del anterior.

Regla de producto del Owner: **cada intervalo se ajusta hacia arriba al primer troquel real que satisface o supera su separación requerida, desde la elevación real resuelta del nivel anterior.**

## 2. Autoridad

Sin cambio de dueño y sin autoridad nueva:

- `DynamicLoadBeamGeometry.ResolveLevels` sigue siendo la única que resuelve elevaciones de salida y entrada por nivel. Sus dos sobrecargas comparten una sola rutina.
- La retícula sigue llegando de `DynamicRackSystemResolver` (`TROQUEL_LARGUERO` del poste y `TroquelPaso`). Ninguna constante de origen.
- `RackFirstLevelDatum` sigue siendo dueño del primer nivel (ADR-0031).
- `PushBackElevations` sigue siendo dueño del larguero posterior (ADR-0031 §9) y no se modifica.

Las vistas, cotas, BOM y metadata siguen consumiendo los niveles resueltos. Ninguna recibe una regla propia.

## 3. Comportamiento

Con `p` = paso, `b` = base de la retícula, `S` = pendiente nominal del frente y `Req(i)` = `InOutBeamDepth(i) + Pallet.Height(i) + ClearHeight(i)` (la misma suma de hoy, de la celda `i`):

```text
Salida(1)    = TroquelMasCercano(RawFirst)                 sin cambio (I-42, ADR-0031)
Salida(i+1)  = PrimerTroquelEnOSobre(Salida(i) + max(p, Req(i)))
Entrada(i)   = TroquelMasCercano(Salida(i) + max(0, S))
PrimerTroquelEnOSobre(v) = b + ceil((v - b) / p - 1e-9) * p
```

- **B-1. Nunca hacia abajo.** Ningún intervalo resuelto queda por debajo de su requisito.
- **B-2. Troquel exacto.** Un valor ya en troquel se conserva, sin subir otro paso (épsilon `1e-9` en pasos, el del Selectivo).
- **B-3. Por intervalo.** Cada intervalo usa su propio requisito; no se impone separación uniforme.
- **B-4. Referencia real.** La acumulación parte de la salida resuelta del nivel anterior. `Math.Round` no se sustituye sin corregir la referencia.
- **B-5. Primer nivel.** Su salida no cambia. No es un intervalo y su regla pertenece a ADR-0031.
- **B-6. Pendiente Dinámico.** Misma pendiente nominal y mismo redondeo al más cercano; lo único que cambia es la referencia, de la teórica a la salida real. La subida queda igual en todos los niveles de un frente, y los intervalos en el extremo de entrada igualan a los de salida.
- **B-7. Push Back.** La salida es el ancla baja y `ChooseHighTroquel` deriva el posterior sin modificación. No se toca la pendiente ni los extremos frontal y posterior.
- **B-8. Sin retícula medida** (`p <= 0` o base no finita): no se ajusta, y `Salida(i+1) = Salida(i) + max(0, Req(i))`, como hoy.
- **B-9. Valores degenerados.** Un requisito no positivo sube un paso (mismo piso que `SelectiveGeometryResolver.Separation`): los niveles no coinciden ni se invierten. Un valor no finito se propaga igual que hoy, sin excepción nueva; producción no lo genera (`Positive`, `NonNegative` y `Validate`).
- **B-10. Altura.** `CalculateResolved` y `PushBackHeaderHeight` no cambian y consumen la elevación real; la cabecera automática sigue conteniendo el último nivel y puede subir un pie comercial.

**Efecto de B-6 en el nivel 1, declarado.** Si el primer nivel está fuera de troquel, su entrada puede moverse un paso: con 6" y 4 fondos pasa de 12.5" a 14.5", y la subida de 6" a 8", más cerca de la nominal de 7.4375". Se acepta porque la alternativa, conservar la entrada histórica del nivel 1, deja la subida distinta entre niveles y puede dejar el primer intervalo del extremo de entrada por debajo del requisito.

## 4. Persistencia y documentos existentes

**Sin cambio de esquema, de DTO ni migración.** Las elevaciones no se persisten y se siguen recalculando desde las entradas authored.

**Decisión D-LEG (M-03).** La regla se aplica a todo diseño que se resuelva, nuevo o existente. No se añade marcador de versión.

| Ruta | Efecto en un rack existente |
|---|---|
| Abrir, guardar, reabrir, `RACKDUPLICAR` | ninguno: no re-resuelven; la geometría dibujada no se toca |
| `RACKEDITAR` | el editor muestra los niveles corregidos; **Actualizar** regenera todas las vistas ligadas |
| BOM (`RACKBOM`, `RACKBOMTOTAL`) | resuelve desde el diseño: puede reflejar una cabecera mayor antes de que se actualice el dibujo |
| Proyectar o insertar hermana | la vista nueva sale corregida; las hermanas sin actualizar conservan la anterior hasta **Actualizar** |

Dirección del cambio: la salida del nivel 1 no se mueve y ninguna salida baja respecto de la regla anterior. Una entrada sí puede moverse un paso en cualquier sentido (B-6).

Alternativas rechazadas:

- **Marcador aditivo por documento** (patrón de `FirstLevelDatum`): activa M-02, conserva el defecto en todo rack ya dibujado (incluido el caso reportado) y deja dos reglas de por vida.
- **Usar `FirstLevelDatum` como discriminador:** reinterpreta un campo persistido con otro significado (M-02).
- **Regenerar al abrir:** cambia el dibujo sin que el usuario lo pida.

El Owner confirma o rechaza D-LEG en la Owner Validation (OV-I67-06). Si la rechaza, el marcador entra como enmienda `A-n` con el proceso de cambio material.

## 5. Semántica de fallo

Sin cambios (M-04 no activado).

## 6. No-objetivos

Rediseño de Dinámico o Push Back, representación de tarimas, UI nueva, reglas nuevas de pendiente, Dynamic Rebuild Robustness, cambios estructurales/RAM, Workspace/ID30, migraciones,
refactors oportunistas, primer nivel y datum, Selectivo y Cantilever, y validar la altura manual contra el último larguero (se registra en `ideas-futuras`).

## 7. Puntos de extensión

Ninguno añadido, cambiado ni retirado. `Consumes`: `RackFirstLevelDatum` y la autoridad vertical de Push Back. `Extends`: ninguno. `Introduces`: ninguno.

## 8. Invariantes y pruebas

Toda prueba nueva se ve fallar antes del arreglo, o por una violación temporal si pasa sobre el producto actual.

| ID | Invariante | Prueba Owner | RED esperado |
|---|---|---|---|
| INV-01 | H58 uniforme: 46/46/46 | T1 | falla (44/46/44) |
| INV-02 | Troquel exacto sin paso extra, también con ruido de coma flotante | T2 | violación temporal (sin épsilon) |
| INV-03 | Intervalos no uniformes: cada uno, su primer troquel | T3 | falla |
| INV-04 | Tres o más intervalos sin alternancia | T4 | falla |
| INV-05 | Origen desplazado de la retícula (bases distintas de 0.5) | T5 | falla |
| INV-06 | `Spacing(i) >= Req(i)` y `Spacing(i) < Req(i) + p` | T6 | falla |
| INV-07 | Cotas y piezas dibujadas del Dinámico usan las mismas elevaciones | T7 | falla (44/46/44 en cotas) |
| INV-08 | Separaciones Push Back, lado simple y compuesto | T8 | falla |
| INV-09 | Pendiente Push Back: posterior derivado sin cambio; subida y rotación iguales en todos los niveles; nivel 1 fijado | T9 | caracterización: pasa antes y después |
| INV-10 | Edit/update: `Snapshot` + `Resolve` idempotente | T10 | falla en los valores absolutos antes del arreglo |
| INV-11 | Save/reopen: ninguna elevación en el JSON y mismo resultado tras reabrir; editar tras reabrir sigue la regla | T11 | falla en los valores absolutos antes del arreglo |
| INV-12 | Legacy D-LEG: salida del nivel 1 intacta, ninguna salida baja respecto de la regla anterior, entradas authored intactas | T12 | caracterización del nivel 1; la cota inferior se ve fallar por violación temporal |
| INV-13 | Dinámico: `Entrada(i) = TroquelMasCercano(Salida(i) + S)`, subida uniforme, y separación de entrada ≥ requisito | T13 | falla (alterna) |
| INV-14 | Degenerados y sin retícula, deterministas y sin excepción | T14 | violación temporal |
| INV-15 | La cabecera automática contiene el último nivel | T15 | caracterización |

Las pruebas `ALegacyDocument_KeepsItsPhysicalGeometry` y su gemela dinámica se conservan sin cambio. Toda prueba existente que hoy fije el defecto se actualiza y se lista en la evidencia con el motivo.

## 9. Owner Validation

Requerida: cambia dibujo. Sobre `FINAL_CANDIDATE_SHA`, con el DLL del worktree, además del checklist aplicable de la guía manual §5.

| ID | Escenario | Resultado esperado |
|---|---|---|
| OV-I67-01 | Dinámico nuevo: tarima 33", C6, 4 niveles, primer nivel 6" | separaciones 46/46/46 en lateral y cotas |
| OV-I67-02 | Dinámico con tarimas distintas por nivel | cada intervalo, su primer troquel ≥ requisito |
| OV-I67-03 | Dinámico: salida y entrada en vistas frontales y lateral | mismas elevaciones; subida uniforme entre niveles |
| OV-I67-04 | Push Back nuevo con los datos de OV-I67-01 | 46/46/46; cama y largueros frontal y posterior coherentes, pendiente como antes |
| OV-I67-05 | Push Back compuesto A/B | ambos lados con la regla; sin cambio de la política de pendiente |
| OV-I67-06 | **D-LEG:** rack existente con 44/46/44: abrir y guardar sin cambios; `RACKEDITAR` y Actualizar lo corrigen y todas las vistas coinciden | el Owner acepta o rechaza D-LEG |
| OV-I67-07 | Guardar, cerrar, reabrir y editar | valores conservados; editar sigue la regla |
| OV-I67-08 | BOM del rack actualizado | altura y postes coherentes con el dibujo |

## 10. Gates

Consolidados, porque el cambio es una función y sus consumidores no cambian:

- **F0** preflight, caracterización y Freeze.
- **G1** (F1+F2+F3 de la orden): corrección, regresión Dinámico, impacto Push Back, persistencia, edición, vistas y cotas. Cierra con INV-01..15 en verde, Core Full local sobre el SHA de cierre con árbol limpio y CI exacta del SHA.
- **READY** (F4): READY-01..09, conformidad Architect + Coordinator, Candidato y Owner Validation.

Sin merge a `main` antes de una Owner Validation aprobada y orden de integrar.
