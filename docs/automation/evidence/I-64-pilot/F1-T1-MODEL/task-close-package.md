# I-64 — F1-T1-MODEL: paquete de cierre de tarea para la revisión del Coordinator

Preparado por la sesión responsable el 2026-10-02 tras la verificación `EXECUTION_VERIFIED` (AUTOMATION_PLAN §16.4 paso 7 y §16.12).

Este paquete:
- **no** declara GATE PASS, F1 PASS ni la aceptación del ADR;
- deja al Coordinator el cierre de la tarea y el gate;
- deja al Owner la aceptación del ADR y Owner Validation.

## 1. Resultado verificado

| Hecho | Valor |
|---|---|
| Verificación final | `R20261002T143410Z-ae0b` (Controller Codex `gpt-6-luna`/`high`, efectivo comprobado): **EXECUTION_VERIFIED / NONE**, `FailureClass` NONE, las 14 comprobaciones de 16.9 en pass |
| VerifiedSha | `8d9a0c6e6ea148caf3e8f20d26a95a864a2943ce` (= `HEAD` = `origin/architecture/workspace-persistente-rackcad` al verificar) |
| CI exacta del VerifiedSha | run 37020159700, push, ref exacta: Tests (Domain + Application), UI Tests, Build UI y **Build Plugin without AutoCAD** en success. El log del Plugin dice «Plugin Release publish and solution Release build succeeded without warnings» y «AutoCAD compile-reference verification passed» |
| Pruebas requeridas | `FullyQualifiedName~RackCad.Tests.Workspace` en el TRX Core de CI: 51 seleccionadas, 51 superadas. Core completo: 12447/12447. UI: 1637 superadas, 0 fallidas, 17 omitidas por `Skip` ya existentes |
| RED de la cadena | **acreditado**, `ChainRedSha` `a9778068af566bc2b84ba6a0005711b6452a2666`. RED run 37019198364: Core en failure con 29 fallos por aserción del filtro y 22 guardas superadas |
| ChainRedFiles | 5 rutas en `tests/RackCad.Tests/Workspace/`: `SelectionContextTests.cs` (renombrada), `WorkspaceHintDrainTests.cs`, `WorkspaceModelBoundaryTests.cs`, `WorkspaceSelectionContextTests.cs` y `WorkspaceSessionTests.cs` |
| Alcance | `git diff --name-only e0587355..8d9a0c6e -- src tests docs/adr` = ADR 0047, 5 archivos en `src/RackCad.Application/Workspace/` y 4 pruebas en `tests/RackCad.Tests/Workspace/`. Todo dentro de `AllowedWriteScope`. Plugin, UI, Domain, CI, `.csproj` y la configuración global sin cambios |
| Trailer | todos los commits del Worker llevan `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`; los de la sesión, el suyo |

## 2. Producto

**Modelo puro** en `RackCad.Application.Workspace`: `SessionId`, `WorkspaceSession`, `WorkspaceSessionRegistry`, `Hints` y `SelectionContext`.
- Sin referencias a AutoCAD, WPF ni Plugin.
- Sin E/S ni persistencia.
- Espacios de nombres de bloque.

**Pruebas:** 4 clases y 51 casos en `namespace RackCad.Tests`, todas seleccionadas por el filtro del contrato.

| Clase | Casos |
|---|---|
| `WorkspaceSessionTests` | sesión por instancia viva; dos instancias, dos sesiones; destrucción y retiro; identificador nativo reciclado → sesión nueva y vacía; petición de A con B activo → rechazo sin efectos; claves de rack que no cruzan sesiones |
| `WorkspaceHintDrainTests` | pistas encoladas que nunca cambian el contexto; N pistas → un drenaje; espera mientras hay comando, modal RackCad o panel oculto; pistas durante un modal; solo las operaciones permitidas; aislamiento por sesión |
| `WorkspaceSelectionContextTests` | Ninguno, Uno, Selección (N) con recuento, racks más otras entidades, Sin identidad (`IsNullOrWhiteSpace`, sin RackId inventado), Diagnóstico, igualdad de contextos |
| `WorkspaceModelBoundaryTests` | guardas de texto: sin AutoCAD, WPF ni Plugin; sin autoridades de persistencia o escritura (con mutaciones en memoria por familia, sin rechazar `WorkspaceSessionRegistry`); sin métricas, providers ni `ProjectSummary`; sin estado por pestaña |

**ADR 0047** (`docs/adr/0047-workspace-persistente-modeless-rackcad.md`, estado **propuesto**, creado en `36c344dc` y sin cambios después).
- Comparación mecánica de la sesión con el Anexo A de la Proposal V5 congelada: las 11 decisiones y el contexto son **literalmente iguales**.
- La relación con otros ADR, las alternativas y las consecuencias solo difieren en la mayúscula inicial y en presentar como viñetas lo que el
  Anexo separa con punto y coma.
- Añade las cabeceras de la plantilla (Fecha, Decisores, Iniciativa) y Referencias. No añade decisiones.

Invariantes INV-F1-T1-01..12:
- 01, 03, 04, 08 y 10 tienen guardas automatizadas en `WorkspaceModelBoundaryTests`.
- 02, 05, 06, 07 y 09 tienen pruebas de comportamiento.
- 11: el ADR queda propuesto.
- 12: no hay cambios en Plugin ni en comandos.

## 3. Cadena y presupuesto

| Intento | Planificación | Worker | Verificación | Resultado |
|---|---|---|---|---|
| 0 | `R20261002T001349Z-aad5` (rechazada, P-03) y `R20261002T003827Z-3a76` (aceptada; nc4 `R20261002T004303Z-8d3c`) | `R20261002T005244Z-f656`: RED `36c344dc`, GREEN `66d34af3` | `R20261002T011239Z-647e` | REWORK Ci (guarda que prohibía `Registry`; espacio de nombres frente a `NamespaceFolderGuardTests`) |
| 1 | `R20261002T061355Z-c5cd` (`CorrectionOf` 647e/Ci/`b0ddc8de…`) | `R20261002T062258Z-de0f`: RED `189353f8`, GREEN `b38e54f9` | `R20261002T063546Z-0cd7` | REWORK Ci (CS8632 en la build Release del job del Plugin) |
| 2 | `R20261002T140200Z-3886` (`CorrectionOf` 0cd7/Ci/null) | `R20261002T141605Z-dc2b`: RED `a9778068`, GREEN `8d9a0c6e` | `R20261002T143410Z-ae0b` | **VERIFIED** |

Consumo final:

| Tipo | Uso |
|---|---|
| Planificación | 4 de 4 |
| Trabajo | 3 de 4 |
| Verificación | 3 de 4 |
| `attempts` | 2 de 3 |
| Contador de la clase Ci | 2 de 3 |
| BLOCKED | 0 |

- Ningún P-01, P-05, P-06 ni P-08.
- Los P-02 posibles fueron procesos efímeros resueltos por relectura (regla del Coordinator).
- La custodia completa de cada `RunId` está en este directorio y en `../F1-T1-MODEL-nc4/`.

## 4. Desviaciones y lecciones registradas

- **Sesión.**
  - Transcripción ASCII de invariantes y prohibidas expandidas en la primera planificación (§21).
  - Espacio de nombres `RackCad.Tests.Workspace` impuesto en el delta sin revisar `NamespaceFolderGuardTests` (§22).
  - El delta no exigía la build Release sin avisos (§24).
  - Todas se corrigieron dentro de la cadena.
- **Worker.**
  - En el intento 2, un `git checkout <sha>` sin rutas dejó HEAD desacoplado y produjo el commit local `a2db0a91`. Nunca se publicó: no está en
    ninguna rama remota.
  - El GREEN se rehízo sobre la rama.
- **Controller.** Detalles de forma en la Evidence de la verificación 2, sin efecto en el resultado (§24).
- **Comprobación local de CI.** `eng/ci/verify-autocad-references.ps1` se detiene en esta estación en `Assert-CleanRunner`, porque AutoCAD 2025
  está instalado (causa de entorno).
  - El Worker no lo modificó ni lo eludió.
  - La equivalencia local fueron builds Release `--no-incremental` de `RackCad.Tests` y `RackCad.UI.Tests` con 0 avisos MSB, CS, NU o NETSDK.
  - La señal autoritativa es el job remoto, en success.

## 5. Decisiones que quedan para el Coordinator o el Owner

1. **Cierre de la tarea F1-T1-MODEL:** revisión del Coordinator sobre este paquete y el control manual de 16.10 del texto libre de la
   verificación final. El control de la sesión dio 0 coincidencias.
2. **Controles negativos nc1..nc3** (README §10, sobre las entradas de la verificación VERIFIED).
   - No se ejecutaron: en I-61 cuentan dentro del tope de invocaciones de Codex, aquí queda 1 verificación de 4 y el contrato no los lista en
     `ExpectedEvidence`.
   - Ejecutarlos exige que el Coordinator amplíe el tope (al menos 2 invocaciones más) o los declare no aplicables a esta tarea.
3. **Índice de `docs/adr/README.md`:** no lista el ADR 0047, porque está fuera de `AllowedWriteScope`. Su actualización corresponde a un cambio
   autorizado aparte.
4. **ADR 0047:** su aceptación es solo del Owner.
5. **Gate F1:** sigue abierto. Faltan las demás tareas de F1, a planificar según `I-64-f1-gate-contract-proposal.md` y las órdenes del
   Coordinator, y la revisión del Coordinator. Smoke-1 va tras F1 PASS y tras el RELEASE de I-52.
