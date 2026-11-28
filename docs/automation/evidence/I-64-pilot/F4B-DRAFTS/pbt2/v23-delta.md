# I-64 F4B — PB-T2: de la v2.2 a la v2.3 del paquete (para emitir PB-T2b)

> **Redacción para el repo público:** sin rutas locales ni nombres de usuario, de máquina o de clientes.

**Entrada:**
- **El menor M-6 de CA-T2a** (`independent-review-pbt2a-v21-check.md`), que su informe dejó «para el paquete antes de emitir PB-T2b».
- **El mensaje de la sesión:**
  - PB-T2a está emitida (F4B-E32): la X es `1ee10d3f` y la BASE, `0dc623ca`;
  - PB-T1c está cerrada (F4B-E31): W y WC de `0dc623ca` son su GREEN, `161a0dad`;
  - hay que remedir las anclas [T1c] y ver si PB-T2b necesita algo más con la ventana real.

**Salida:**
- `I-64-f4b-pbt2-gate-contract-package-v23.md`: la v2.2 **custodiada** en `0dc623ca` (con la línea de estado de PB-T2a que añadió la sesión), con el registro v2.2 → v2.3 (V23-01 a V23-07) delante de los anteriores;
- este archivo;
- `measure_pbt2b_t1c.py` y su salida, `pbt2b-anchors-out.txt`;
- el generador de PB-T2b.

**Resultado:**
- **El diseño no cambia.** M-6 queda aplicado. Las anclas de W y WC del cuerpo del paquete pasan a `0dc623ca`, y los rangos de INV-PBT2B-03 van ya en el invariante.
- **Cosas que faltaban para PB-T2b:**
  - el número repetido de INV-PBT2B-08;
  - el sitio de `UpdateCompositeEnabled()`, que PB-T1c movió;
  - el estado de PB-T1c y de PB-T2a.
- **Ningún CT, alcance, parada ni decisión del Owner cambia,** salvo dos precisiones de M-6: el momento de `diseno-igual` en CT-PBT2B-02 y CT-PBT2C-03, y la regla de atribución de la disposición del configurador.
- **No hay decisiones nuevas para el Owner.**

## 1. Los cambios

| Id | Origen | Qué cambia | Dónde |
|---|---|---|---|
| **V23-01** | CA-T2a M-6 (a); F4B-E30 y F4B-E31 | **La disposición del configurador en el perfil.** Al cerrarse, un `RackFrameConfiguratorWindow` real escribe `RackCad/RackFrameConfigurator.layout` en los datos locales del usuario. En los filtros de UI de PB-T2b, medido en `0dc623ca` sobre el código y no sobre los literales de las guardas, lo crean `RichEditorCharacterizationTests`, `RichEditorCloseContractTests` y `RichEditorContractTests`, del filtro `RichEditor`. `WindowCensusGuardTests` solo lo nombra en su censo, pero el análisis de la corrección de PB-T1c lo incluye (D-4). El invariante común 10 gana la regla de esa corrección: la prueba de limpieza se registra; si solo cambia ese archivo, se atribuye a esos filtros sin parar; cualquier otro cambio es parada. C09 rige las pruebas nuevas, que no abren el configurador real (INV-08) | §6.0 (invariante 10 y las propias comunes); R-PBT2-08 |
| **V23-02** | CA-T2a M-6 (b); F4B-E30; F4B-E31 (OWNER-01) | **El desfase de una pasada.** Tras `GapBox`, el separador central o la estructura propia, el diseño de `LastComputation` del compuesto va una pasada por detrás. CT-PBT2B-02 toma `diseno-igual` tras `Recalcular(w)`, una receta nueva del §5.0 copiada de `PushBackWindowOnStateTests.cs:449-450`, la C-1 de PB-T1c. CT-PBT2C-03 la toma tras «Aplicar», que ya recalcula (`ApplyEditsToEditingState`). CT-PBT2B-04 ya la tomaba tras «Aplicar». «Restaurar» no recalcula antes de leer el diseño: queda como **L-PBT2-13**, sin corrección (CLASSIC no cambia; el Owner lo conoce por OWNER-01) | §5.0; CT-PBT2B-02; CT-PBT2C-03; §8.3 |
| **V23-03** | La ventana real de PB-T1c | **Las anclas [T1c].** Son 80 citas de W, WC y WX, 64 distintas: 58 se movieron (en W, hasta unas 115 líneas, por lo que PB-T1c retiró) y 5 siguen igual (W:63, W:82, WX:47, WX:68-69 y WC:738-777, esta solo en un registro de cambios). Una cambió de sitio: `UpdateCompositeEnabled()`, que estaba en WC:933, dentro del `LoadCompositeFromDesign` que PB-T1c retiró, y ahora está en W:314, en `LoadExisting` (D-PBT2-05). WX no cambia. Los registros de cambios conservan las líneas de entonces. La tabla, cita por cita, está en `pbt2b-anchors-out.txt` | Base de la medición; §2 a §10 |
| **V23-04** | La nota de INV-PBT2B-03 (lección M10) | **Los rangos de INV-PBT2B-03,** medidos sobre `0dc623ca` con el método de INV-PBT1C-03, desde la documentación hasta el final del miembro: `LoadNew` W:269-287, `LoadDesignForNew` W:289-304, `LoadExisting` W:306-320, `Update_Click` W:3215, `InsertBatch_Click` W:3217-3245, `RequestDraw` W:3247-3304, `PendingWork` W:3331-3344, `Bom_Click` W:3348-3364, `SaveLibrary_Click` W:3366-3401 y `UpdateButtons` W:3449-3496. La nota sigue fuera del contrato | §6.2 |
| **V23-05** | La sesión | **El estado:** PB-T1c, cerrada (F4B-E31), y la reasignación de `editable`, comprobada en los anclajes de PB-T2a (F4B-E32). Cambia en el «Estado», la primera condición para emitir, HT2-24, HT2-25, el §4, R-PBT2-09 y el §9 | Cabecera; §2.6; §4; §7; §9 |
| **V23-06** | La sesión | La custodia de PB-T2b gana la v2.3, este archivo y `measure_pbt2b_t1c.py` | §6.0, «Custodia» |
| **V23-07** | Lo encontró el generador de PB-T2b | **Dos invariantes con el mismo número.** El propio «INV-PBT2B-08» (la única escritura en disco es `ArchivoTemporal`) chocaba con el común 08 (las pruebas de UI), que el contrato también numera INV-PBT2B-08 y que ya dice lo mismo. El propio pasa a **INV-PBT2B-08b**. «INV-08» del §5.0 y del §7 sigue siendo el común | §6.2 |

## 2. Lo que se comprobó con la ventana real y no cambia

Todo lo que sigue se midió en `0dc623ca` por objetos git, y lo comprueba el generador de PB-T2b:
- **D-PBT2-05:** el cuerpo del constructor privado sigue el orden real de `LoadExisting` de PB-T1c:
  - `inputs`;
  - `UpdateCompositeEnabled()`;
  - `sourceProject` e `isEditingExisting`;
  - `Adopt`, `SyncName()` y `LoadFromModel`.

  Los campos y métodos que usa existen: `isEditingExisting`, `sourceProject`, `hasValidModel`, `SyncName`, `LoadFromModel`, `SetStatus` y `UpdateCompositeEnabled`, este en WC.
- **D-PBT2-06 (1):** el paso que se extrae está en W:3270-3287, literal y en el mismo orden. «Lo de hoy» va de W:3289 a W:3303, con el `SyncName()` de PB-T1c.
- **D-PBT2-07:** `PendingWork` sigue siendo la expresión de W:3342-3344, con el texto del módulo literal.
- **D-PBT2-09:** `UpdateButtons` tiene `canOutput`, `outputIsBlocked` y `currentInputsAreValid`. Los once botones de WX existen. Los once literales y sus testigos ASCII no están todavía en W.
- **Las guardas:**
  - S-7, S-9, el orden de las cinco sentencias de CT-22 y los literales de CT-23 siguen en W;
  - hay una sola `.RequestUpdate(`;
  - R3 se cumple en la base: en toda la clausura, solo `RequestDraw` e `InsertBatch_Click` anotan peticiones.
- **INV-PBT2B-06:** las costuras son **41** (40 más `EditableStateForTest`).
- **PBT2B-C13:** los 26 nombres están libres en W y WC, y en `src` y `tests`. Las listas `Forwarded`, `Tokens` y `Rebind` de las guardas de PB-T1c no nombran ninguno.
- **Los RequiredTests:** el recuento estático da los mínimos del §6.2, contando los `Skip` como seleccionados (D-1 de las notas del generador).

## 3. Lo que no cambia, a propósito

- **La viñeta «Marca [T2b]»** sigue pegada a la fila V21-07 del registro v2 → v2.1 (era D-16 de PB-T2a). Solo afecta a cómo se ve la tabla.
- **El registro de mediciones** (`I-64-f4b-pbt2-measurements.md`) no gana un apartado: la remedición está aquí y en `pbt2b-anchors-out.txt`.
- **El §6.3 (PB-T2c)** solo cambia en lo que trae M-6 (CT-PBT2C-03). Sus mutantes [T2b] se miden sobre el cierre de PB-T2b.

## 4. Reglas

- **Lecturas:** solo por objetos git desde el checkout principal (`0dc623ca` y sus antecesores), sin tocar el worktree.
- **Escrituras:** solo en `hs65\pbt2\`, con guiones escritos con la herramienta de escritura (`_work/b/make_v23.py`, `measure_pbt2b_t1c.py`).
- **Ni dotnet ni git de escritura.**
- **Incidencias:** ver las notas del generador, §6.
