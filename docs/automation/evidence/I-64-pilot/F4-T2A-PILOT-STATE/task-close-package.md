# I-64 — F4-T2A-PILOT-STATE: paquete de cierre

Lo prepara la sesión responsable. Durante la noche del 2026-11-11 actuó bajo la delegación del Owner (`F4-DRAFTS/coordinator/owner-night-delegation.md`). Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, de F4-E18 a F4-E25.

Este paquete cierra una tarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `cf9f94418133d7cb105bbd48d618fe3d8963317c`, CI `43167736802` push 4/4 (Core en Linux) |
| Verificación | `R20261112T062828Z-00f6`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `ed680e1f`; intento 1: RED `19123dfe`, GREEN `77658968`; intento 2: BaseSha `539614ae`, RED `a7ced417`, GREEN `cf9f9441` |
| Contratos | `gate-contract.json` (revisión 1, sustituida), `gate-contract-rev2.json` (`ForbiddenWriteScope` 57, F4-E21) y `gate-contract-attempt2.json` (corrección, F4-E24) |
| Intentos | 2 de 3 |
| Config | Snapshot G (`7C1516A9…`) en todas las invocaciones |
| nc1 / nc2 / nc3 | ver §3 |

## 2. Planificaciones y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `13b0` | REJECTED_BEFORE_INVOCATION (A5) | un invariante reescrito («lo leen» → «los leen»), F4-E19 |
| `61f3` | REJECTED_BEFORE_INVOCATION (A4) | una entrada de `ForbiddenWriteScope` omitida, F4-E20 |
| `981d` | ACEPTADA con una dispensa textual | criterio 18, F4-E22; nc4 `6aea` |
| `9353` | entrega del intento 1 | Worker reanudado tras una suspensión del equipo, F4-E23 |
| `8bc4` | `EXECUTION_BLOCKED` / STOP (Ci) | comparación textual de documentos dependiente de la plataforma, F4-E24 |
| `a765` | STOP S-04 del Controller | error de entrada de la sesión: hechos del intento 1 ya falsos |
| `3ff0` | aceptada (A1-A8 8/8, fidelidad PASS) | delegación de la corrección |
| `dd72` | entrega del intento 2 | el Worker dejó dos temporizadores; la sesión detuvo su tarea |
| `00f6` | `EXECUTION_VERIFIED`, 14/14 | sobre `cf9f9441` |

## 3. Controles negativos (sobre las entradas de `00f6`)

| Control | Corrida | Resultado |
|---|---|---|
| nc1 (CurrentSha falso) | `R20261112T064509Z-ef74` | CONTROL PASS: Identity, `EXECUTION_BLOCKED` |
| nc2 (alcance estrechado, sin `PushBackPilotMaterializer.cs`) | `R20261112T065046Z-3387` | CONTROL PASS: Scope, `EXECUTION_BLOCKED` |
| nc3 (término de gate en `WorkCompleted`) | `R20261112T065454Z-88f1` | CONTROL PASS: FreeText, `EXECUTION_REWORK_REQUIRED` |

## 4. Producto entregado

El estado del piloto acotado de Push Back (F4-PILOT-PUSHBACK-01) en `RackCad.Application.Systems.PushBack.Pilot`:
- `PushBackPilotDraftState`: el estado compuesto existente, sus inputs y el origen inmutable.
- Comparador y copiador, con censo completo de campos.
- Adaptador de texto pendiente del único campo `AnnotationScale`, con paridad exacta con la validación de la ventana.
- Materialización tipada desde el texto del sobre por la ruta existente; solo un lado.
- `ComposeForWrite`: solo la escala sobre la base vigente, con rechazo tipado de cualquier diferencia fuera del piloto (IC-07 punto 2).
- `KeepPilotEditsOn`, para la superficie después de «Conservar mi borrador».

En el núcleo de borradores, N-2: copia de todas las entradas con recargas atómicas; `InitialDraftState` sale como copia y `CurrentDraftState` sigue por referencia (INV-F4T1-19).

## 5. Abierto

- **Arrastres de F4-E25:** cinco menores de la revisión independiente van a T4A y uno a T4B; las clases (c) y (d) de la caracterización, a T1B y F5.
- **`docs/ideas-futuras.md`:** las pérdidas (a)-(d) de la ruta de carga clásica.
- **Sin AutoCAD:** la lectura en el host, IC-04, la superficie y la mutación son de F4-T2B, T2C, T3 y T4.
