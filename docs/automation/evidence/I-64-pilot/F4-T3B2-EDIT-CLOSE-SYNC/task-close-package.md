# I-64 — F4-T3B2-EDIT-CLOSE-SYNC: paquete de cierre

Lo prepara la sesión responsable el 2026-11-13. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, F4-E43 a F4-E46.
- La disposición del Owner y del Coordinator (`F4-DRAFTS/coordinator/owner-f4t3b1-close-disposition.md`, F4-E43) autoriza la tarea y su cadena hasta la custodia y el cierre.
- Las decisiones del Owner durante la cadena están en la misma carpeta:
  - `owner-f4t3b2-planning-decision.md`: una planificación más;
  - `owner-f4-snapshot-h-decision.md`: la Snapshot H;
  - `owner-f4t3b2-controls-decision.md`: corregir y repetir.

Este paquete cierra una subtarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `23f67f56b8319d77448e989c0949e4ccd915bdc1`, CI `43440641847` push 4/4 |
| Verificación | `R20261113T151548Z-4f9b`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `b5ae014d` (custodia del STOP de F4-E45); RED `c3de6116`, GREEN `23f67f56` |
| Contrato | `gate-contract.json` (AuthorityRevision `41ca9a52`, el paquete v3 con la §9 de F4-E43) |
| Intentos | 1 de 3 |
| Config | Snapshot G hasta el Worker; Snapshot H (decisión del Owner tras el reinicio del equipo) en las verificaciones y los controles |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E46; limitaciones en el §5) |
| F4-T3 | T3A, T3B1 y T3B2 cerradas; T3C en espera de su paquete |

## 2. Planificación y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `af63` | REJECTED (fidelidad, A5) | `t3` en vez de `t3b` en `Authorities[1]` |
| `e025` | STOP S-03 del Controller | conflicto alegado inexistente; segundo fallo, STOP al Coordinator (F4-E45) |
| `05fc` | aceptada (fidelidad PASS, A1-A8 8/8, Horizonte Medium) | planificación adicional autorizada por el Owner; nc4 `5b28` con su oráculo |
| `b033` | entrega | RED `c3de6116` (fallan 75 pruebas nuevas, ninguna existente), GREEN `23f67f56`; 13 rutas |
| `c983` | interrumpida sin salida | reinicio del equipo |
| `f5e9` | `EXECUTION_VERIFIED`, 14/14 | con un hecho viejo de la sesión en el prompt; sus controles nc2 y nc3 no pasaron por él |
| `4f9b` | `EXECUTION_VERIFIED`, 14/14 | prompt corregido; sobre ella se hicieron los controles de cierre |

## 3. Revisiones y controles

- **Revisión directa:** producto conforme, con D-1 y D-2 (`direct-review.md`).
- **Revisión adversarial independiente:** ACEPTABLE CON MENORES (0/0/7) (`independent-review.md`).
- **Controles sobre las entradas de `4f9b`:** los tres CONTROL PASS con su oráculo ordinario, sin sustitutos.

| Control | Corrida | Resultado |
|---|---|---|
| nc1 (CurrentSha falso) | `R20261113T151929Z-e076` | CONTROL PASS: Identity primera en fail, `EXECUTION_BLOCKED` / STOP |
| nc2 (sin `BrowserSession.cs` en el alcance) | `R20261113T152336Z-01c3` | CONTROL PASS: Scope primera y única distinta, `EXECUTION_BLOCKED` / STOP |
| nc3 («GATE PASS» en `WorkCompleted`) | `R20261113T152956Z-c886` | CONTROL PASS: FreeText primera y única distinta, `EXECUTION_REWORK_REQUIRED` / REWORK |

**Primera ronda de controles sobre `f5e9`:** nc1 `d0ef` dio CONTROL PASS; nc2 `8ff6` y nc3 `f6db` dieron CONTROL NOT PASSED. Se conservan tal cual.
- **Causa medida:** el prompt de verificación, que los controles copian, decía que la BaseSha era hija directa de la AuthorityRevision con tres rutas de diff. Desde la custodia del STOP son 34, todas de `docs/` de la unidad.
- **Efecto:** el Controller dio Authority como primera en fail en los dos controles.
- **En nc2, además:** la línea de Scope fijaba en 13 las entradas de la delegación y el Controller no vio la ruta quitada.
- **Tratamiento:** el Owner decidió corregir y repetir. La sesión corrigió el hecho y quitó el conteo; la verificación y los tres controles se repitieron desde cero.

## 4. Producto entregado

Sobre el campo de F4-T3B1, sin escritura authored y sin cambios en el host, el puerto, el Plugin, la paleta ni `BrowserSessionRegistry`.

- **Cerrar una pestaña** (D-08):
  - sin borrador, se cierra;
  - con borrador limpio, `DiscardRack` y se cierra siempre, con aviso si el descarte no da `Discarded`;
  - protegida, confirmación en línea con «Descartar» y «Cancelar».
- **«Descartar»** del campo en Error.
- **«Descartar borradores de este documento»** con la comprobación TOCTOU por igualdad ordinal de las dos listas.
- **Avisos de veto y de pérdida:**
  - se consumen solo en la presentación;
  - las filas son estables;
  - la purga del veto va con las sesiones muertas;
  - la pérdida dura hasta «Entendido».
- **Retención:** `BrowserSession.ApplySelectionContext(…, bool keepActive = false)` y la retención de la pestaña activa con texto pendiente, con su aviso.
- **Arrastres de F4-T3B1:** N-1 (el doble responde `Kept` como el host) y N-3 (repeticiones de la tecla que abrió la caja).

## 5. Limitaciones y abierto

**Menores → F4-T3C** (la disposición de F4-E43 la hace dueña de los menores que se le asignen; detalle en `analysis.md`):
- M-1: una excepción del puerto en el cierre limpio no quita una confirmación pendiente de otra pestaña. Es letra de INV-F4T3B2-12;
- M-2: con solo racks inciertos, el texto de la confirmación del documento queda «de: .». Es alcanzable por la vía de reserva del host; va con el endurecimiento de `GetRacksAtRisk`;
- M-3: el `Present` del constructor del panel, con un veto en cola, llama a `IsSessionAlive` sin contención. Choca con la documentación de F3 de `browserSummary`;
- M-4: punto ciego de las guardas de llamadas tras `=>` o `?`;
- M-5: guardas sin autoprueba en negativo;
- M-6: dos guardas fijan el teclado y los botones de todo el panel. F4-T3C las cambia de forma declarada si las necesita;
- M-7: «nunca `ContainFailure`», sin prueba;
- D-1: la guarda de las colas en el host fija cuatro archivos. Cambio declarado si F4-T3C las nombra en otro.

**Siguen siendo de F4-T3C** (F4-E43):
- el comando `RACKPANELDESCARTAR` y el aviso con su vía;
- el endurecimiento del fallo o desconocido de `GetRacksAtRisk`, que **no** se acepta como comportamiento final;
- la integración modal y de foco que quede;
- los menores 4 y 6 de F4-T3A y el menor 6 de la revisión del intento 1 de F4-T3B1.

**Declarados en INV-F4T3B2-15:**
- el veto sin `StateChanged`;
- el compromiso por `LostFocus` antes de «Cerrar»;
- el clic perdido de la tira;
- la reversión del pendiente solo con «Cerrar» y «Descartar».

**Smoke-2:**
- el foco tras «Cerrar», «Descartar» y «Cancelar» (la caja de K = 1 se reutiliza);
- la confirmación fuera de la vista con la paleta desplazada;
- Enter y Space sobre los botones nuevos;
- el veto al cerrar un documento real con un borrador protegido;
- recrear la paleta con un veto en cola (M-3);
- que las repeticiones de N-3 no lleguen a la caja (D-2).

**Proceso:**
- **Deudas de protocolo PD-F4-05 y PD-F4-06 (F4-E43):** no aparecieron en los controles de cierre.
- **Lecciones:**
  - al mover la BaseSha con una custodia, todos los prompts de la cadena se revisan, no solo el que falló;
  - el prompt de verificación no fija conteos que la mutación de un control cambia.

**Sin AutoCAD:** pruebas de VM y de vista (STA) con dobles; el Plugin, con el build Release de la CI.
