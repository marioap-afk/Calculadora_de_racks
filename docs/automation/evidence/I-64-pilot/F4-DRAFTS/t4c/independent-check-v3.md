# I-64 F4-T4C — Comprobación dirigida del paquete v3 (antes de emitir)

**Veredicto:** EMITIBLE TRAS CORRECCIONES. **Recuento de los hallazgos nuevos:** 0 bloqueantes, 1 importante y 2 menores (§3 y §5).

## 1. Objeto, método y límites

**Objeto:**
- el paquete v3, frente a los 15 hallazgos de la re-revisión de la v2 (`independent-review-v2.md`, que aquí se cita como RR);
- el arrastre de R-12.

**Base:** `b15072ff`.

**Método:**
- se comparó la v3 con `_package-v2.md`;
- se leyeron enteras D-T4C-02, D-T4C-03, D-T4C-04, D-T4C-09, la tabla de H0, C12, O-01, O-02, O-07 y §10.2;
- se recorrieron los cambios de las demás decisiones tocadas;
- se comprobó en el código lo que funda cada corrección:
  - la degradación de `EBC`;
  - `Authorize` del registro;
  - `EditConfirmOutcome.Waiting`;
  - las cuentas de G-6;
  - las guardas sobre `Degrade` y `ReportFault`.

  No se compiló, no se ejecutó ninguna prueba y no se escribió nada en el worktree.

**Notación:** `P3:n` es la línea n del paquete v3, que está fuera del repositorio. El resto de las rutas son relativas al repositorio.

**Lo que no se pudo verificar:** lo marcado [H] y la compilación.

---

## 2. El cierre de N-01 a N-15 y de R-12

| Id | Estado | Comprobación |
|---|---|---|
| N-01 | **Resuelto** | Tres capas, y cada una cierra la liberación por su lado: (1) las operaciones totales del núcleo, con el billete quitado en el `finally` de `Execute` y de `NotRun`; (2) el `finally` exterior del corredor, con una marca de «terminal hecha» y `Retire` solo si hay escritor; (3) la caducidad del billete empezado que no entró en `Execute` (P3:411-424, P3:529-532). Las discrimina la prueba RQ con el núcleo real y el doble «`Admit` sin terminal» (D-T4C-17). **El matiz es legítimo.** Un billete en `Execute` no caduca, y `Execute` es total. Entre `Admit` y `Execute` no hay ningún aviso que pueda evaluar la caducidad a destiempo |
| N-02 | **Resuelto** | G-6 incluye `WorkspacePanelCommand.cs`, con 2 `[CommandMethod(` (`:22`, `:38`). Las otras cuatro cuentas coinciden con el código: 2 y 2, 1 y 1, 2, 1 y 1. Tiene negativas con la cuenta más uno, y un `[CommandMethod(` en `CTL`. **El matiz** de conservar `CommandMethod` en la regla **es legítimo** |
| N-03 | **Resuelto** | G-3 se limita a los tipos de larga vida, y la reflexión del núcleo excluye los tipos generados por el compilador, los que empiezan por `<`. El corredor, `Target` y las clausuras quedan exentos, y `new PushBackPilotHostRunner(` solo va dentro de `Request`. El tipo anidado de la corrida del núcleo implementa `IPushBackPilotHostRun`, pero no tiene ningún campo de los tipos vetados: se satisface |
| N-04 | **Resuelto** | El cierre sale de la caducidad. La guarda (G-10 y la fila 5) dice que `ExpireStaleUpdate` y `TakeUpdateProgress` no nombran `CloseOwnMutation`, y lleva su negativa. Que el cierre sea inalcanzable se sostiene: en el segundo caso de caducidad, la entrada en reposo que la dispara ya cerró el ámbito en `SetQuiescent`, antes de la comparación |
| N-05 | **Resuelto** | La fila 12 incluye `:699-700`, y también la subida de `pilot.Read` en `:682-685`, que en efecto pasa de 0 a 2. Sus negativas están bien elegidas |
| N-06 | **Resuelto** | El prohibido ya no contiene ningún archivo del permitido. En los tres compartidos, la protección es por línea, con D-T4C-20 y C07 (P3:1339-1346, P3:1468) |
| N-07 | **Resuelto en el mapeo** | Pasa a C12 (C12, la fila 5 de H0 y O-02). Solo un caso aislado se declara. **Pero la variante V3 que se recomienda no corrige el cierre por un comando anidado** (C3-01) |
| N-08 | Resuelto | `TryConfirmBase` responde `Waiting` en vuelo, con `EditConfirmOutcome.Waiting`, que existe (`src/RackCad.Application/Workspace/WorkspaceEditIntent.cs:353`), antes de la relectura (`CORE:548-556`). Prueba RQ con una relectura que difiere |
| N-09 | **Resuelto, con una imprecisión** | `QuiescentEntriesOf` devuelve nulo con `IsDegraded`, `RequestUpdate` responde `Unavailable` y el billete no empezado caduca. Se confirmó que `Authorize` no rechaza una sesión degradada (`src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs:39-46`). **El matiz** de no añadir ningún miembro **es legítimo.** Pero el cierre del ámbito se pone en `ReportFault`, que no es el único camino de degradación (C3-02) |
| N-10 | **Resuelto en el mapeo** | Da C12 y no se declara sin el Owner. **La recomendación V3 no lo corrige** (C3-01) |
| N-11 | Resuelto | INV-19 sigue la letra de `OO`, y añade «ninguna firma de Application nombra un tipo de AutoCAD» |
| N-12 | Resuelto | Cuarta cláusula en H-33, en D-T4C-16 (dispensa 2) y en O-01 |
| N-13 | Resuelto | La documentación del valor se amplía en el mismo cambio de M-03. El archivo está en el permitido, y ninguna guarda fija ese comentario |
| N-14 | Resuelto | `:194-231` |
| N-15 | **Resuelto** | Una nueva aceptación ya no vacía el almacén, el gesto recoge antes lo pendiente, y la caducidad dentro de `RequestUpdate` sale por el valor de inicio. **El matiz es legítimo.** Queda una precisión para el caso de un billete de otro rack (C3-03) |
| R-12 | **Resuelto** | G-6 (N-02), G-9 y G-10 |

**Recuento:** 15 de 15 resueltos, y R-12 resuelto. Los cuatro matices son legítimos. C3-01 viene de N-07 y N-10, y C3-02 y C3-03 de N-09 y N-15.

---

## 3. Los hallazgos nuevos

### C3-01 (IMPORTANTE). La variante V3 no cubre dos de las tres causas para las que se recomienda

V3 dice que, mientras dura el callback, «el cierre por inicio de comando y por entrada en reposo no cuenta, y solo cierra el fin de comando». En `Settle`, las reglas de cierre «vuelven a ser las de arriba» (P3:771-774).

**Lo que eso deja sin corregir:**
- **(a) Un comando anidado dentro del callback.** Su `CommandEnded` sigue cerrando la ventana durante el callback, antes de las escrituras que quedan de POST. B queda marcado en cada Actualizar, que es la causa N-07 que V3 dice tratar. V3 solo corrige el `CommandWillStart` de `EXECUTEFUNCTION` que llega después de `Begin`.
- **(b) La falta del `CommandEnded` y del `EnteringQuiescentState` del comando interno (N-10).** Tras `Settle` vuelven las mismas reglas, y solo cierra el siguiente `CommandWillStart`. El enmascaramiento de una escritura sin comando persiste.

Aun así, O-02 recomienda «**V3** si confirma un cierre temprano sistemático **o la falta de los dos eventos**».

**Hay además una contradicción interna.** La alternativa que la fila 5 y O-02 ofrecen para N-10, «el cierre al final del callback», choca con la regla de P3:765, «nunca al final del callback, por `F4P:60`», y con C-F4-04 (`docs/initiatives/I-64-f4-gate-contract-package.md:60`). No se presenta como la desviación que es.

**Evidencia:** P3:765, P3:771-774, P3:779 y la fila 5 de H0; O-02 en el §9.

**Corrección:**
- **redefinir V3:** mientras dura el callback, **ningún** evento cierra la ventana, y `Settle` restablece las reglas normales. Esa versión cubre el `CommandWillStart` tardío y el comando anidado, que es nuestro;
- **para N-10,** la variante es «cerrar en `Settle`». Se presenta en O-02 como desviación expresa de C-F4-04, y la frase de P3:765 se ajusta, por ejemplo con «salvo la variante de O-02»;
- **ajustar** la recomendación de O-02 y la fila 5 de H0.

### C3-02 (MENOR). El cierre por degradación va en `ReportFault`, pero no es el único camino

La sesión se degrada en `Degrade` (`src/RackCad.Application/Workspace/EventBridgeCore.cs:478-488`), por `Fault` (`:491-495`), desde seis entradas: `:140`, `:246`, `:345` (dentro de `OnSourceEvent`, que también recibe las pistas de la corrida), `:399`, `:433` (`ReportFault`) y `:472`.

Con el cierre solo en `ReportFault` (P3:764, fila 17), una degradación por cualquier otro camino deja `OwnMutationOpen` puesto. Hoy no tiene efecto observable, porque una sesión degradada no recibe pistas. Pero la regla y su prueba OM fijan una invariante falsa.

**Corrección:** cerrar en `Degrade`, que es el punto único. La prueba OM provoca la degradación por un fallo dentro de `OnSourceEvent`. Ninguna guarda fija el cuerpo de `Degrade`, porque las de `TW/WorkspaceLifecycleGuardTests.cs:216` y `:230` miran otros cuerpos.

### C3-03 (MENOR). La caducidad dentro de `RequestUpdate` con el billete de otro rack

**El problema.** El billete es por sesión.
- Si la caducidad que ocurre dentro de `RequestUpdate` es la de un billete del rack A y la petición es del rack B, el `Failed(HostNotRunReason)` de A sale como valor de inicio del gesto de B.
- Y «no se guarda nada», así que el resultado de A no se presenta nunca (P3:425).

**Por qué es menor.** Desde el producto es casi inalcanzable: el gesto llama antes a `TakeUpdateProgress`, cuya caducidad guarda el resultado de A bajo A.

**Corrección:** precisarlo. Si el billete caducado es de otro rack, su `Failed` se guarda bajo su rack y la petición sigue. Si es del mismo, sale por el valor de inicio.

---

## 4. El §9 y O-07

- **N-07** está en O-02 y en C12; **N-12**, en O-01. Pero la recomendación de O-02 para N-07 y N-10 se apoya en V3 tal como está definida, que no corrige esos casos (C3-01).
- **O-07 (a) con la cota sigue siendo segura:**
  - el billete empezado tiene garantizada su llamada terminal (N-01);
  - el no empezado caduca con la entrada en reposo o con la degradación;
  - una sesión degradada no recibe billete.

---

## 5. Repaso de las decisiones tocadas

No se encontró ninguna contradicción nueva aparte de C3-01 a C3-03, en ninguna de estas:
- D-T4C-03, con sus reglas 1 a 5;
- D-T4C-04, con los pasos 7 y 8, el cerrojo y el objetivo;
- D-T4C-05, con sus filas nuevas;
- D-T4C-08, con las cinco operaciones;
- D-T4C-10 a D-T4C-14;
- D-T4C-16, con su tabla y sus dispensas;
- D-T4C-17;
- D-T4C-20 y el §6, con el prohibido sin archivos del permitido, C12 y INV-02.

---

## 6. Veredicto

**EMITIBLE TRAS CORRECCIONES. B/I/M = 0/1/2.**

**Lo que cierra la v3:** N-01 a N-15 y R-12.

**Lo que queda:**
- **C3-01:** una corrección de texto en D-T4C-09, en la fila 5 de H0 y en O-02. No cambia el diseño de base, solo la variante condicional que se lleva al Owner;
- **C3-02 y C3-03:** precisiones.

Ninguno de los tres pide otra revisión de arquitectura.

**Lo que vuelve al Owner:** el §9, con O-02 corregido según C3-01. La alternativa para N-10 va como desviación expresa de C-F4-04.
