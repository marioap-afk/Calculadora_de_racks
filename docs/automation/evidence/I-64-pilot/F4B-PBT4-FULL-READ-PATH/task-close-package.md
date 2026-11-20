# Paquete de cierre — F4B-PBT4-FULL-READ-PATH (PB-T4a)

**Resultado:** **CLOSED / PASS WITH LIMITATIONS** sobre `faef702055990d1350a73bf4d254eb9e898d5670`, con attempts **1 de 3**.

La lectura de la intención del Workspace trae también el modelo completo de PB-T3, sobre el mismo escaneo, y las relecturas de un borrador abierto van al origen del borrador.
- **`EditIntentReadResult`** gana `FullRead`, `Catalog` y la fábrica `FromReads`. `FromPilot` no cambia.
- **El adaptador de la lectura del Plugin** (RIA) llama al lector completo sobre el mismo escaneo, dentro del mismo bloqueo, y gana `AtOrigin`, que solo llama a la función pura `PushBackOriginByText.ReadAt` de `Draft/`. La lectura del piloto sigue siendo una sola (B-08).
- **Las cinco relecturas de WHC** usan el texto del origen del borrador, por la interfaz nueva `IWorkspaceEditIntentOriginReader`. Un lector sin ella cae en `ReadIntent`, como hoy.
- **El tipo del borrador del panel no cambia.** La única sustitución es CT-PBT3A-12, con su registro declarado, por CT-PBT4A-06.

**Autoridades:**
- **El contrato:** `F4B-PBT4-FULL-READ-PATH/coordinator/gate-contract.json`, con el paquete de PB-T4 v4.3, AuthorityRevision `27c13fab` y BaseSha = ChainBaseSha `5aaacce3`.
- **Las decisiones del Owner:**
  - F4B-E49, las tres tandas sobre PB-T4;
  - en este cierre, F4B-E51: la parada de la verificación, nc1 y el coste de la lectura;
  - F4B-E35 (RR), F4B-E37 (Snapshot K), F4B-E40 (PD-F4B-01), F4B-E43 (el sustituto de nc2) y F4B-E46 (el total exacto de cada rama).

## 1. La cadena

| Paso | Corrida | Resultado |
|---|---|---|
| Emisión | X `27c13fab` y BASE `5aaacce3` | F4B-E49 y F4B-E50, con la v4.3. CA-T4a dio EMITIBLE tras su recomprobación. La compilación de comprobación dio 0 errores, 0 avisos CS y Core 19 129 = 19 122 + 7, también con el GREEN literal |
| Planificación | `R20261120T193416Z-a53f` | Aceptada al primer lanzamiento: inyección PASS 8/8, A1-A8 8/8 y 18 criterios. nc4 `R20261120T193630Z-735b` |
| Worker | `R20261120T193653Z-de54` | IMPLEMENTATION_COMPLETE. RED `214f8c75` y GREEN `faef7020`, sin reparación del RED, sin desviaciones y sin paradas |
| CI del RED | `44810371492` | tests en failure; ui-tests y build-ui en success; build-plugin omitido. Core 19 137 = 19 125 + 5 Failed + 7 Skipped; UI 2 451 = 2 430 + 21 |
| CI del GREEN | `44814418683` | 4/4. Core 19 137 = 19 130 + 0 + 7; UI 2 451 = 2 430 + 21. Los totales exactos de su única rama (F4B-E46) |
| Revisión directa | — | Conforme |
| Revisión independiente | — | **ACEPTABLE CON MENORES 0/0/7,** más OWNER-01 a OWNER-03 |
| Verificación 1 | `R20261120T205531Z-611f` | **No productiva:** el prompt superaba el límite de la línea de órdenes de Windows y Codex no arrancó (exit 126) |
| Verificación 2 | `R20261120T211710Z-a468` | **STOP Authority, S-12:** el generador en BASE «no figura en Authorities». El Owner: «Reverificar con aclaración» |
| Verificación 3 | `R20261120T212638Z-de08` | **STOP Tests, PBT4A-C07:** una frase del prompt leída como hecho. El Owner: «Corregir la frase y reverificar» |
| Verificación 4 | `R20261120T213256Z-8d11` | **EXECUTION_VERIFIED** / NONE, 14/14, sin hallazgos |
| nc1 | `R20261120T214408Z-bdf3` | CONTROL NOT PASSED. Su sustituto `749e` dio FAIL en `e2_OtherFailCitesFakeSha`. El Owner: «Aceptar solo esta vez», así que nc1 = SATISFIED_BY_OWNER_DISPOSITION |
| nc2 | `R20261120T214941Z-4cda` | CONTROL NOT PASSED (PD-F4-01). Su sustituto v2 `60ab` dio PASS: nc2 = SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B |
| nc3 | `R20261120T215653Z-e244` | **CONTROL PASS** |

**Relevos:**
- 0 participantes persistentes en toda la cadena;
- la Snapshot K y el binario acreditado;
- `main` = `812baf87`, sin tocar;
- el «limit reached» de los eventos de nc1 y nc2 es el texto de la parada S-11.

## 2. Límites, hallazgos y arrastres

| Id | Qué | Va a |
|---|---|---|
| OWNER-02, el coste de la lectura (medido) | De +40 a +80 ms en `Begin`, de +35 a +86 ms en la relectura habitual y de +80 a +175 ms con el origen encontrado, en los fixtures mayores. La materialización completa se repite con el origen. Todo dentro del bloqueo del documento | Decisión del Owner: PB-T4b elige el origen antes de leer (una sola materialización), y se vuelve a medir en PB-T4b y en el Smoke-PB |
| OWNER-01 | El origen deriva al `Primary` tras una relectura sin coincidencia y se queda ahí. Está declarado y no es una regresión | Smoke-PB, y se informa al Owner |
| OWNER-03 | Dos subcasos de la relectura con origen sin CT | Smoke-PB |
| PBT4A-IR-M01 a M07 | La precisión de CT-02 a CT-05, el oráculo de clases temporales, unos pasos fuera de `Paso`, una errata de la entrega y `ChosenReference` | `docs/ideas-futuras.md` |
| Dos frases del prompt de la verificación | La aclaración de 16.3 sobre las rutas que no están en Authorities, y la regla de C07 escrita como condición | Corregidas en la cadena de PB-T4b |
| El tamaño de los prompts | Menos de 30 000 bytes, con guarda en los guiones previos | Hecho, también en PB-T4b |
| Sin AutoCAD | La relectura real en el origen del dibujo | El Smoke-PB de PB-T5 |
| El registro declarado de CT-PBT3A-12 | Completado en este cierre (cinco líneas) | Hecho |

**Siguiente:** PB-T4b, con el paquete v4.5, por la emisión condicional de F4B-E49.
