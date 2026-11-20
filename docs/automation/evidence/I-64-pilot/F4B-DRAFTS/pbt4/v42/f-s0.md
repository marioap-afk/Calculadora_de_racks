## 0. En una página

**Lo que pide el Owner** ([OD], fila de PB-T4): «Add "Editar todo" … Do not make PB-T4 invent state translation logic. It should consume the already proven full Push Back draft/state API.»

**La base:** el cierre de PB-T3c, `7f0d9aa3`. **Cuatro subtareas secuenciales,** cada una con su 1/3. Los `TaskId` empiezan por `F4B-PBT4-` y tienen **como mucho 26 caracteres** (MAX_PATH solo acota por arriba; V41-11):

| Subtarea | TaskId | Qué | Sustituciones exactas (§8.3) | La emisión |
|---|---|---|---|---|
| **PB-T4a** | `F4B-PBT4-FULL-READ-PATH` (23) | El Plugin lee también el modelo completo sobre el mismo escaneo; el resultado lleva lo completo y el catálogo. Las relecturas de un borrador abierto van al origen del borrador por su texto, con una relectura pura en `Draft/` sobre el escaneo ya tomado (A-T3-8): es un cambio observable declarado. La firma de `RackPilotIntentAdapter.Read` no cambia | **1** (CT-PBT3A-12) | Condicional (pregunta 4) |
| **PB-T4b** | `F4B-PBT4-FULL-DRAFT-PANEL` (25) | El borrador del panel pasa al completo; OB-03.2; la regla única de la escala, en Application; «Actualizar» por `ExecuteFull`; «Conservar mi borrador» con la presentación «Por grupos» y `KeepOn`. **El código del piloto queda sin uso, sin retirarlo** | **470** ({{N_C0}} de las 9 clases, «Saltar y duplicar»; {{N_C5}} de la letra; {{N_UI}} de la UI), la lista que el Owner tomó como «la que vio» | Condicional, con esa lista |
| **PB-T4c** | `F4B-PBT4-EDIT-ALL-WINDOW` (24) | «Editar todo», con el abridor en dos piezas | **2** (PEG:155-163 y CT-PBT1B-15) | Condicional |
| **PB-T4d** | `F4B-PBT4-RETIRE-PILOT` (21, propuesto) | Retira el código del piloto que el panel ya no usa | **139** sustituciones, **55** originales y **29** miembros de apoyo borrados con registro (esbozo, §6.4) | **Vuelve al Owner con su lista** |

**Las decisiones del Owner,** tomadas (§8.2): Q1 «Retirarlo», Q2 «Por grupos», Q3 «Saltar y duplicar» y Q4 «Sí, con condiciones» (`owner-pbt4-decisions.md`); y, después, «Retirar tras PB-T4c» y «Sí, esa lista» (`owner-pbt4b-retire-list-decisions.md`).

**PB-T4a es la primera que se emite.** Su parte del paquete está completa y sin remisiones: el diseño (D-PBT4-02 y 03, con el texto literal del GREEN y `ReadAt`, B-08), las roturas medidas con el recorrido entero de sus guardas (§4.1), la lista cerrada CT-PBT4A-01 a 08 con sus totales (§5.1), lo común (§6.0) y el contrato propuesto con lo que necesita su generador (§6.1).

