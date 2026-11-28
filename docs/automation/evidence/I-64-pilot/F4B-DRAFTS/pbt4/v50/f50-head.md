**Estado:** v5.0. Escribe **PB-T4c** (`F4B-PBT4-EDIT-ALL-WINDOW`, «Editar todo») en forma literal, como la v4.4 a la v4.9 lo hicieron con PB-T4b, sobre el **cierre de PB-T4b** (`6adff90d`, F4B-E53: CERRADA, intento 1 de 3; el GREEN `c065080d` es el texto literal de la v4.9). La remedición se hizo por objetos git (`v50/remeasure_pbt4c_v50.py`), y la precondición 6, en copias exportadas de `6adff90d` (`/d/r5a` en adelante; `pbt4ccc/compile/`).

**Las decisiones que la fijan:**
- **El Owner, dos veces, sobre la lista de PB-T4c:** la de 2 que vio (`owner-pbt4-decisions.md`, pregunta 4) pasó a 3 con CT-PBT4B-20 («Añadirla a la lista (Recomendado)», `owner-pbt4c-list-3-decision.md`, sha256 `@@OWNER3@@`) y a **4** con CT-PBT2B-06 (`owner-pbt4c-list-4-decision.md`, sha256 `@@OWNER4@@`): **`pbt4c-substitution-list-4.tsv`**, sha256 `@@LIST4@@` (la de 3, `pbt4c-substitution-list-3.tsv`, `@@LIST3@@`, queda como historia).
- **La sesión:** D-S7 a D-S10 (`session-pbt4c-design-decisions.md`, sha256 `@@SESSIONC@@`): la costura de CT-09, los filtros por clase exacta, el nombre `IWorkspaceFullEditHost` y lo que cubre CT-09.

**Lo que cambia:** PB-T4c entera: D-PBT4-12 a 17, §4.3, §5.3, §6.0 (sus filas), §6.3 con §6.3.1 y §6.3.2, §8.3, §8.5, §9, §11 y el anexo V50-A, con el texto literal del GREEN, del esqueleto, de los `Skip` y de los equivalentes, y las líneas medidas de las CT.

**Lo que no cambia:** PB-T4a y PB-T4b (sus §5.1, §5.2, §6.1 y §6.2, enteros), PB-T4d (§6.4) y D-S1 a D-S6.

**Lo que sale de la v5.0 (PB-T4c):**
- las piezas literales `pbt4c-green-v50.txt` y `pbt4c-skeleton-v50.txt` y su generador, `green_pbt4c_v50.py`;
- los `Skip` (`pbt4c_skips_v50.py`), los equivalentes (`make_equivalents_pbt4c_v50.py`) y su comprobación (`check_skips_pbt4c_v50.py`);
- el RED y el GREEN en una copia (`apply_red_pbt4c_v50.py` y `apply_green_pbt4c_v50.py`);
- la lista (`make_list_pbt4c_4.py`), las CT como datos (`pbt4c_ct_table_v50.py`), el paseo de guardas (`walk_guards_pbt4c_v50.py`) y las sondas;
- y `make_v50.py`, que arma esta versión desde la v4.9.

---

## Registro de cambios v4.9 → v5.0

| Id | Hallazgo | Qué cambia | Dónde |
|---|---|---|---|
| **V50-01** | **La remedición sobre `6adff90d`** (por objetos git; `v50/remeasure_pbt4c_v50-out.txt`): ningún anclaje de PB-T4c cambió de texto salvo los que tocó PB-T4b. **Citas de la v4.9 corregidas:** «archivos=14» está en **:384** (no :381); WHS 398-421, EIG 1011-1024 y 1155-1184, WNH 713-723, WBS 361-366, URT 1108-1109, `ControllerViolations` 659-707, EIH:1060 y :1079. **Movido por PB-T4b** (+38 en WHC): «two-phase» en :1187 (era :1149), `TryBeginEdit` 1058-1070, `RequestUpdate` 1414-1473 (la marca en :1426 y `PassGate` en :1430), `PassGate` :1959, `UpdateGate` :1023; `TryKeepPilotEdit` (652-751) y `SetPendingText` cambiaron. **La fila «PEG:380-385, verde» de §4.3 era obsoleta:** PEG:379-386 está en la lista de 478 y su copia (`WorkspacePilotEditGuardFull.cs:266-272`, fijada por `CopySha`) exige las dos interfaces de WHC | D-PBT4-12 a 17; §4.3 |
| **V50-02** | **La lista, de 2 a 4,** con dos decisiones del Owner: **CT-PBT4B-20** (el `Skip` de PEG deja el archivo distinto de su `BaseSha`; medido en `/d/r5a` con solo los dos `Skip`) y **CT-PBT2B-06** (su ventana 4 fija que la pregunta del módulo «manda primero»: es lo que A-T2b-OWNER-01, F4B-E38, mandó corregir en PB-T4; la revisión v1 decía «Ninguna prueba fija hoy la pregunta con las dos cosas pendientes», y no era cierto) | §0; §4.3; §8.3; §8.5; la lista |
| **V50-03** | **El GREEN y el esqueleto, literales** (anexo V50-A), aplicados por anclajes exactos sobre `6adff90d`: 9 archivos existentes y 4 nuevos; el RED, 4 existentes y 4 nuevos. Compilan con 0 errores y 0 `warning CS`, el Plugin incluido | D-PBT4-12 a 17; §6.3.1; anexo V50-A |
| **V50-04** | **Los cuatro `Skip` y los cuatro equivalentes,** literales, que genera el guion desde los originales, con un cambio declarado cada uno. **El de CT-PBT2B-06 falla en el RED** (la ventana del esqueleto no cambia) y pasa en el GREEN, así que su filtro va con `ExpectRed` true | §5.3; §6.3.1; anexo V50-A |
| **V50-05** | **La tabla entera de las CT,** CT-01 a 15, con su clase y sus líneas literales, para la lectura RR-3 de la cadena (la v4.9 solo traía la de la CT-15 y remitía a la v3) | §5.3 |
| **V50-06** | **Las filas de PB-T4c de la tabla de RR-3,** una por clase, cada una con su clase y sus CT, como la de CT-19 de PB-T4b; la de CT-08 nombra su clase (`PushBackEditAllGuardTests`) | §6.0 |
| **V50-07** | **Los totales, en cifras** (F4B-E46), sin fórmulas: GREEN Core 19 619 = 19 158 + 0 + 461 y UI 2 486 = 2 437 + 0 + 49; RED Core 19 619 = 19 149 + 9 + 461 y UI 2 486 = 2 432 + 5 + 49 | §5.3; §6.3.1 [14] |
| **V50-08** | **D-S7:** CT-09 escenifica el módulo con `HeaderConfiguratorDialog` (no con `HeaderConfiguratorPresenter`, que construiría un `RackFrameConfiguratorWindow` real: C05) | D-PBT4-17; CT-09 |
| **V50-09** | **D-S8:** los filtros nuevos, por clase exacta; ninguno selecciona una clase con el otro `ExpectRed`, y ningún filtro heredado selecciona una clase nueva | §6.3.1 |
| **V50-10** | **Las restricciones medidas que el GREEN cumple:** WHC con sus dos interfaces; los nombres prohibidos de PEG:1015; el comentario de PEG:1079; `EditAllOutcome` fuera del censo de portadores; los 30 textos de WHHG; los límites de WCo | D-PBT4-13; §4.3 |
| **V50-11** | **D-S9:** `IWorkspaceFullEditHost` (y `SetFullEditHost`, `_fullEditHost`), porque `WorkspaceModelBoundaryTests:101` prohíbe «Editor» en los tipos de `RackCad.Application.Workspace` y el paquete nombraba `IWorkspaceFullEditorHost` desde la v4 (medido en `/d/r5c`, y el arreglo en `/d/r5e`) | D-PBT4-13 y 15; anexo V50-A |
| **V50-12** | **El paseo de guardas** sobre el GREEN, con el método de la v4.8 (las tuplas) y las pruebas de `6adff90d`, que incluyen las de PB-T4b. **Lo que no ve, declarado:** la reflexión sobre tipos (la caída de V50-11) y el comportamiento de la UI (la de CT-PBT2B-06). Lo que vale es la suite completa de Core y los filtros de UI de la precondición 6 | §4.3; §9 |
| **V50-13** | **Las dudas del generador** (`generator-doubts-pbt4c.md`): el contrato literal, sus piezas por sha256, la precondición 6 y su resumen, y CA-T4c | §6.3.1; §6.3.2 |
| **V50-14** | **Cuándo se ofrece «Editar todo»:** con las condiciones de «Editar» salvo la de solo lectura (si no, desaparecería con el borrador abierto) | D-PBT4-12 |
| **V50-15** | **K = 1, medido** con la sonda (CT-05): las 17 operaciones con su respuesta y su motivo | D-PBT4-14; CT-05 |
| **V50-16** | **Los filtros heredados:** el n.º 12 (`RackCad.Tests.Workspace`) **sí selecciona** PEG:155-163 y CT-PBT4B-20 (2 718 → 2 716); el n.º 7, de 2 a 1; el n.º 36 (UI), de 4 a 3; **el n.º 43 queda en 0 y su fila sale**; `MinSelected` cuenta resultados (medido en las TRX) | §6.3.1 |
| **V50-17** | **CT-09 y «solo un módulo escenificado» (D-S10):** en WORKSPACE no se puede alcanzar. Escenificar un módulo deja siempre cambios sin aplicar, porque el estado editable lleva la autoridad de módulos (W:177; la sonda de UI en `/d/r5f`). CT-09 cubre `ambas`, `solo-sin-aplicar`, `ninguna` y `clasico-modulo` | D-PBT4-17; CT-09 |
| **V50-18** | **Por qué un equivalente cae en el RED:** el de CT-PBT2B-06 fija la pregunta combinada, que solo trae el GREEN (W no cambia en el esqueleto, o CT-09 pasaría en el RED). Los otros tres son de guardas que el RED ya cumple. En PB-T4b, la mayoría de las copias pasaban en el RED porque fijaban lo que PB-T4b no cambia | §5.3 |
