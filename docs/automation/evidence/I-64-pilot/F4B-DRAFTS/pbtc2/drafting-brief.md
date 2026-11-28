> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: paquete v1 del contrato de PB-TC2 (I-64 F4B), las correcciones declaradas que quedaron fuera de PB-TC

Lo da la sesión principal al redactor de PB-TC, que ya conoce el anexo A de la v2 de PB-TC y la ventana clásica de Push Back.

## Contexto y autoridad

- **PB-TC está CLOSED / PASS WITH LIMITATIONS.** GREEN `4440f67d`, custodia `5597c05b` (F4B-E08), HEAD actual de la rama. `main` = `812baf87`.
- **El Owner decidió en F4B-E09** (literal en `<scratchpad>\hs65\pbt1\owner-pbt1-pre-decisions.md`; se custodiará como `F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md`):
  - **Pregunta 1 = «Corregir en PB-TC2 (Recomendado)»:** una tarea propia, PB-TC2, antes de PB-T1, que corrige:
    - (1) **H-31:** `GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables, con la regla `Keep` de PB-TC;
    - (2) **OWNER-1:** en «Ambos lados», una caja mixta que recibe un valor lo aplica a los dos lados.
  - **Pregunta 2 (c-02)** y **pregunta 3 (`pb-21-cola`)** NO entran en PB-TC2: van a PB-T1.
  - **Pregunta 4: autorización condicional de emisión.** PB-TC2 se emite sin volver al Owner si su revisión independiente acaba con 0 bloqueantes y 0 importantes sin resolver y no aparece ninguna decisión material nueva.
- **El expediente del analista,** con los hechos medidos en HEAD `5597c05b`: `…\scratchpad\hs65\pbt1\ob065-dossier.md`, §2.1 (H-31), §2.2 (OWNER-1) y §5 (la PB-TC2 propuesta). Ojo: las líneas de W y WC se movieron con el GREEN de PB-TC; el expediente da la tabla de equivalencias. Mide tú todo de nuevo.
- **El diseño de H-31 ya revisado:** el anexo A de tu v2 de PB-TC (`…\scratchpad\hs65\pbtc\I-64-f4b-pbtc-gate-contract-package-v2.md:1217-1299`), con ICT2 I-03 RESUELTO, N-07 y N-08 (`…\pbtc\independent-review-v2-check.md`).
- **La plantilla:** tu v2.2 de PB-TC (`…\pbtc\I-64-f4b-pbtc-gate-contract-package-v22.md`, custodiada como `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md`) y su contrato emitido (`docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json`).
- **Las lecciones del cierre de PB-TC** (`docs/automation/evidence/I-64-pilot/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md`, §4), que se aplican:
  - **M-02:** ningún CT tautológico. Cada aserción tiene que poder fallar con un producto plausible distinto.
  - **M-03:** cada extensión declarada lleva su CT, incluido un caso negativo donde aplique.
  - **M-04:** la evidencia con rutas relativas comprobadas, y `TreeSha` = el árbol real (`git write-tree`) o null.
  - **M-05:** un CT que escenifica una edición de módulos no puede cerrar la ventana con cambios pendientes, porque saldría el `MessageBox` real. Confirma o cancela en un `finally` interno, o usa la costura del aviso de descarte. Mídelo.
  - Las corridas temporales terminan en `Assert.Fail(texto completo)`, porque `Assert.Equal` trunca.
  - **Los nombres:** las sustituciones, si las hubiera, se llaman `SUS2-0n`, para no chocar con las paradas estándar S-0n.
- **Las reglas de F4B:**
  - [DP] F4B-E03: una replanificación solo por la comparación literal; sustitutos de nc1 y nc2 solo con la firma EXACTA de PD-F4-01 y PD-F4-04; cualquier otra firma vuelve al Owner;
  - D-PB-10: 1/3 por tarea y el intento 3 siempre por el Owner;
  - [OD] D-PB-06: sus seis pasos, «Do not hide these corrections inside PB-T1»;
  - OB-06.4 a OB-06.6 y OB-T1B.1 a OB-T1B.3;
  - el registro declarado `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md` (EJECUTADA para PB-TC): PB-TC2 le añade una sección y cierra en él el «Pendiente» de H-31 y la limitación OWNER-1.

## Qué tiene que fijar el paquete

1. **Los hechos medidos en HEAD `5597c05b`,** en solo lectura y sin compilar. Incluye:
   - todo lector y escritor de las seis cajas;
   - el orden de la siembra (todas bajo `suppressSync`);
   - el comportamiento de `IsMixed` después de que el usuario escribe en una caja mixta. ¿El indicador sigue en pie cuando se ejecuta `ReadCellValues`? De eso depende el diseño de OWNER-1;
   - el paso de calculado a manual de `SetLength`;
   - las guardas que miran esas zonas.
2. **El diseño por línea,** sin latitud para el Worker:
   - `Keep(GapBox, composite.Gap)`;
   - `Keep(ModuleLengthBox, module.Length)`, con la comparación de tolerancia ya existente;
   - un `KeepOptional` para las cuatro anulables (vacía = cálculo = nulo; se ve igual = el valor del portador `advanced`; si no, el de la caja);
   - la regla de la caja mixta en `ReadCellValues` y en `OptionalVal`;
   - qué pasa con `Val` si se queda sin usos.

   Nada de `NumericField`, de Cantilever, de Application ni del Plugin.
3. **La lista cerrada de CT** (UI-STA), con sus líneas esperadas y su fallo en la base por aserción:
   - el hueco;
   - la longitud del módulo, incluido que pasar por la caja **no** convierte un módulo calculado en manual (el caso del fondo 47,99213" del expediente) y que escribir un valor distinto sí lo convierte;
   - las cuatro avanzadas, incluido que vaciar una caja sigue dando el cálculo (nulo);
   - la caja mixta de «Ambos lados» (rack A/B con A ≠ B: escribir lo que se ve de un lado iguala los dos), con el caso no mixto como negativo.

   Los diseños, con `EditInner` sobre el corpus; ningún fixture nuevo en `Fixtures/I64`.
4. **Las sustituciones:** mide con `git grep` toda prueba existente que fije el comportamiento actual de esas cajas (por ejemplo `PushBackCompositeLifecycleWiringTests`, `PushBackCompositeLoadExistingModulesTests`, `PushBackCompositeWindowTests`, `PushBackAdvancedRackParametersWindowTests`, `PushBackModuleEditorWindowTests`, los CT de PB-T0 y de PB-TC). El expediente prevé 0. Si hay alguna, va con el patrón de I-39, con el nombre `SUS2-0n` y su comprobación temporal con `Assert.Fail`.
5. **El efecto declarado (OB-06.4) y la medición en la cadena.**
   - Hazlo como D-PBTC-10 y PBTC-C14 si hay un efecto en la salida que no esté medido: por ejemplo, el módulo que deja de volverse manual, o el hueco exacto.
   - Indica qué es material y cuándo se para la cadena para volver al Owner.
6. **El contrato propuesto:**
   - el `TaskId` `F4B-PBTC2-DECLARED-CORRECTIONS`;
   - un `AllowedWriteScope` exacto;
   - un `ForbiddenWriteScope` con el método de PB-TC;
   - `RequiredTests` con el filtro con `ExpectRed` (clases nuevas `PushBackDeclaredCorrection2*`, para que no choquen con las de PB-TC) y los filtros de no regresión: PB-TC, PB-T0, T1B, Cantilever, los de módulos, composite y avanzados;
   - los invariantes, mecánicos;
   - `ExpectedEvidence`;
   - las `StopConditions`, con las estándar, las de [DP] transcritas literales como en PB-TC (PBTC-C11 es la plantilla) y las propias;
   - las `EligibleCells`, comprobando su vigencia en `docs/automation/agent-execution/model-catalog.md`;
   - la línea base de la CI: la del último run verde de la rama, `44232324035` sobre `5597c05b`. Mídela con `gh run view`;
   - los conteos previstos.
7. **El registro:** la sección nueva de [SC] para PB-TC2, que se crea en la emisión y pasa a EJECUTADA en el cierre, y las obligaciones que siguen pendientes de PB-T2.
8. **El §8, decisiones del Owner:** ya están tomadas (F4B-E09). Deja constancia, y lista solo lo que fuera una decisión material **nueva**. Si encuentras alguna, dilo claramente en tu respuesta final, porque obliga a parar.

## Reglas

- Solo lectura en el worktree (`<worktree-de-I-64>`, HEAD `5597c05b`, con `git -C`): sin compilar, sin ejecutar pruebas, sin escribir en el worktree ni en `artifacts/`.
- `gh` solo de lectura.
- Python solo de lectura, escrito con la herramienta de archivos y no con un heredoc.
- Sin AutoCAD.
- Sin rutas locales ni nombres de usuario, de máquina o de clientes en lo que se custodiará.

## Salidas, en `<scratchpad>\hs65\pbtc2\`

- `I-64-f4b-pbtc2-gate-contract-package.md`, la v1;
- `pbtc2-measurements.md`, con las mediciones y sus órdenes.

**Tu respuesta final** es corta:
- las rutas;
- los recuentos (rutas permitidas, CT, sustituciones y `MinSelected` del filtro con `ExpectRed`);
- lo que no pudiste medir;
- cualquier decisión material nueva que hayas encontrado.
