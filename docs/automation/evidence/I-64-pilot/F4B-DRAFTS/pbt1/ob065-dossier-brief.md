> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: expediente de la vuelta al Owner antes del contrato de PB-T1 (OB-06.5 y arrastres), I-64 F4B

Eres un analista **independiente**, en solo lectura. La sesión principal tiene que llevar al Owner, **en una sola vuelta**, todas las decisiones que la fase exige antes del contrato de PB-T1. El Owner pidió que solo se le pregunte lo necesario. Tu trabajo es medir en el código cada caso pendiente y proponer, para cada uno, opciones con su coste y una recomendación razonada.

**Intenta refutar las recomendaciones fáciles: no es tu trabajo confirmar.**

## Contexto

**PB-T1**, la tarea siguiente, extrae a Application el estado editable de la ventana clásica de Push Back, sin pérdidas y sin alias. La ventana usa ese estado en modo CLASSIC sin cambio observable. Su texto en [OD] (literal, en el §5 del paquete de la fase):

> ACCEPT IN PRINCIPLE. PB-T1 extracts a lossless Push Back editable state outside the WPF window. Hard requirements: module state preserved; parked queue preserved; A/B preserved; no aliasing; no WPF control becomes source of truth; state round-trip lossless for supported classic racks; classic RACKEDITAR still works through the extracted state. Do not rewrite the entire editor merely to obtain an MVVM architecture. The goal is state separation, not aesthetic refactoring.

**Las reglas que obligan a esta vuelta:**
- **OB-06.5** del paquete de la fase: no están autorizadas ni la clase (c), ni la (d), ni ninguna clase nueva de pérdida. Sus casos alcanzables vuelven al Owner antes del contrato de PB-T1, para que autorice corregirlos o reproducirlos en el estado. «PB-T1 no puede dejar que un control sea la fuente de verdad.»
- **[OD] D-PB-06:** «Do not hide these corrections inside PB-T1 state extraction». Una corrección, por tanto, va en una tarea propia y declarada antes de PB-T1, como PB-TC.
- **OB-T1B.3:** si PB-T1 no puede mantener verde una guarda de T1B, vuelve al Owner antes de su contrato.

**Worktree, solo lectura:** `<worktree-de-I-64>`, rama `architecture/workspace-persistente-rackcad`, HEAD `5597c05b`. El producto es el GREEN de PB-TC, `4440f67d`. Usa siempre `git -C <worktree>`.

**Autoridades,** léelas en HEAD:
- el paquete de la fase, `docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1: §3 (D-PB-03, D-PB-05 y D-PB-06), §5, §7.3 y §10;
- la orden literal del Owner, `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md`;
- las decisiones del Owner de PB-TC: `…/F4B-DRAFTS/coordinator/owner-pbtc-decisions.md`, `owner-pbtc-mixed-decision.md` y `owner-pbtc-nc1-decision.md`;
- los mapas `…/F4B-DRAFTS/package/map-a-classic-write-path.md` (MA) y `map-b-editor-state-testability.md` (MB). No te fíes de ellos sin medir;
- **PB-T0:**
  - su cierre, `…/F4B-PBT0-CHARACTERIZATION/task-close-package.md`, con los arrastres M-01, M-04, D-3, D-5 y la observación de CT-16;
  - su entrega, `…/F4B-PBT0-CHARACTERIZATION/1/R20261117T185616Z-3c1b/worker-handoff.json`, con las tablas T-1 (censo control → prueba), T-2 (casos alcanzables) y T-4 en Evidence;
  - su revisión independiente, `…/F4B-PBT0-CHARACTERIZATION/1/R20261117T215307Z-3cff/independent-review.md`, con OWNER-3 (`pb-21-cola`);
- **PB-TC:**
  - su paquete, `docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md` v2.2 (§10: el caso H-31 que no entró; H-31 en el §2);
  - su cierre, `…/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md`, con OWNER-1 y los arrastres M-02 a M-05;
  - su registro, `…/F4B-DRAFTS/superseding-characterization.md`;
- T1B: `docs/initiatives/I-64-f4-t1b-gate-contract-package.md` y sus pruebas `tests/RackCad.UI.Tests/PushBackClassicCharacterization*.cs` y `tests/RackCad.Tests/PushBackClassicCharacterizationPluginGuardTests.cs`;
- el corpus `tests/RackCad.Tests/Fixtures/I64` (17 diseños) y el catálogo entregado `assets/catalogs/`.

## Los casos que tienes que tratar, como mínimo

1. **H-31, la clase (a) que quedó fuera de PB-TC:** `GapBox`, `ModuleLengthBox` y las cuatro cajas avanzadas anulables. Hoy redondean en su gesto: el control es la fuente de verdad.
2. **OWNER-1 de PB-TC:** «Ambos lados» con una caja mixta. Escribir lo que se ve del exacto de un lado no iguala los dos lados.
3. **c-02, de clase (c) de la ventana:** un `HeaderPostCatalogId` inexistente se cambia por el poste por defecto (W:390 y W:836, `PostBox.SelectedId`). El piloto de Application lo conserva. Es un control como fuente de verdad.
4. **`pb-21-cola` (OWNER-3 de PB-T0):** un rack de un sentido que guarda la cola de B. Al cargarlo se pierden los 9 módulos de la cola, `SideB` y `AbsentSlotsB` (`PushBackCompositeEditorAssembler.cs:52-87`, `PushBackEditorDesignAssembler.cs:279-287`, `RackPushBackSystemWindow.Composite.cs:838-842`).
   - ¿Pasa también en el piloto o el panel del Workspace?
   - ¿Choca con el requisito duro «parked queue preserved; A/B preserved; state round-trip lossless for supported classic racks»?
   - ¿Es un «supported classic rack»? Mira cómo se crea un rack así con la interfaz (D-PB-03 y H-077) y si existe en dibujos reales; el bloque M midió 22 sobres reales (`…/F4B-PBT0-CHARACTERIZATION/block-m/`).
5. **Los demás casos de T-2,** marcados CARGA-APPLICATION u OTRA:
   - c-01, c-03, c-04 y c-06, c-05 y c-07;
   - c-12 (lo rechaza el almacén);
   - c-13 (`PalletTolerance = 0` hace lanzar a `LoadExisting`, D-3 de PB-T0);
   - c-14.

   Para cada uno: ¿es una pérdida de la VENTANA (control como fuente de verdad) o una normalización de Application que el estado extraído reproduce sin más, porque ya vive en Application? ¿Alguno choca con un requisito duro de PB-T1?
6. **Los huecos del censo T-1 de PB-T0:** los controles sin prueba que los cambie, D-5. Y los arrastres de PB-TC: M-02 a M-05. **No son decisiones del Owner** salvo que demuestres lo contrario. Clasifícalos como red que el paquete de PB-T1 debe añadir.
7. **Las guardas de T1B y las de texto** (T-SG, T-KB, T-PG, G-10, I-55 y las demás de H-PB-32 del paquete de la fase): ¿cuáles leen el código de la ventana de un modo que una extracción del estado a Application rompería, aunque el comportamiento no cambie? Por ejemplo, si fijan que ciertos miembros o literales están en W. Lista las que con probabilidad no podrían seguir en verde, con `ruta:línea`, para que la sesión sepa si OB-T1B.3 obliga a preguntar ya.

## Para cada caso, en la tabla

- **Hecho medido,** con `ruta:línea` en HEAD.
- **Alcance:** si se alcanza por la interfaz o solo por un documento, y si existe en el corpus o en los 22 sobres reales.
- **Efecto hoy en RACKEDITAR:** pérdida de datos, cambio de dibujo o de BOM, error.
- **Comportamiento del piloto o del panel del Workspace.**
- **Choque con un requisito duro de PB-T1:** sí o no, y cuál.
- **Opciones:**
  - (A) corregirlo como cambio declarado en una tarea propia antes de PB-T1, por ejemplo una «PB-TC2»;
  - (B) reproducirlo tal cual en el estado extraído, si es posible sin fingir y sin dejar un control como fuente de verdad;
  - (C) otra, por ejemplo declarar el caso no soportado.

  Cada opción con su coste aproximado: cadena completa, CT, archivos.
- **Recomendación,** razonada.
- **Si la decisión es del Owner o no:** si la regla ya la resuelve sin él, dilo y cita la regla.

## Reglas

- Solo lectura: sin compilar, sin ejecutar pruebas, sin escribir en el worktree y sin operaciones de Git que escriban.
- Python solo de lectura, escrito con la herramienta de archivos y no con un heredoc.
- Sin AutoCAD.

## Salida

El expediente en español, en `<scratchpad>\hs65\pbt1\ob065-dossier.md`, **sin rutas absolutas locales ni nombres de usuario o de máquina**. Contiene:
1. **Resumen para el Owner,** en lenguaje llano y en un máximo de 40 líneas. Las decisiones que de verdad son suyas, cada una con sus opciones y la recomendada, agrupadas para preguntarlas en una sola vuelta, en un máximo de cuatro preguntas.
2. La tabla de los casos 1 a 5.
3. Los huecos de red que no son decisión (caso 6).
4. Las guardas de T1B y de texto (caso 7).
5. Si se recomienda una «PB-TC2»: su alcance propuesto, con lo que entraría, la autoridad que necesitaría y su orden respecto a PB-T1.
6. Lo que no pudiste medir.

**Tu respuesta final** es una línea: cuántas decisiones son del Owner, si recomiendas una PB-TC2 y la ruta del expediente.
