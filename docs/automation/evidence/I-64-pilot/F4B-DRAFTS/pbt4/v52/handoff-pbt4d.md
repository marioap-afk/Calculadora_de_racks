> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<checkout-principal>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Traspaso de PB-T4d (paquete v5.2), fase 2

**Fecha:** 2026-11-21. **Base:** el cierre de PB-T4c, `6c2820bab4d8b41e3e229e23f3a1638ccbb04735` (F4B-E55). **No se emite nada:** PB-T4d vuelve al
Owner con su lista antes de emitirse (`owner-pbt4b-retire-list-decisions.md`, pregunta 1; D-PBT4-10; §6.4).

Las carpetas: `S = <scratchpad>\hs65`. Los
registros, en `S\pbt4\v52\`; los resultados de las corridas, en `S\pbt4dcc\compile\`. Las reglas de siempre: nada en el worktree (ni leer ni escribir
su ruta), ningún commit, nada en el checkout principal salvo `git archive` y lecturas (`git show`, `git grep`, `git ls-tree`); Python solo con `-I`
desde guiones escritos con Write (nunca `-c`, `python -` ni heredoc); copias exportadas nuevas, nunca reutilizadas; la UI, solo los filtros del
contrato; nombres cortos (ninguna ruta de más de 156 caracteres con el prefijo `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/cc4c/`); UTF-8 con LF.

## 1. Lo hecho

### Fase 1, la remedición por objetos git (hecha y entregada a la sesión)

`v52/remeasure_pbt4d_v52.py` (`b98e0442…d72a`), salida `v52/remeasure_pbt4d_v52-out.txt` (`0e49abb9…14f6`) y `v52/pbt4d-remeasure-v52.tsv`
(`9b6a480e…eef6`). Orden: `python -I remeasure_pbt4d_v52.py "<checkout-principal>" pbt4d-remeasure-v52.tsv` (desde `v52/`).
- La lista de la v4.4 (`pbt4d-retire-list-138.tsv`, `d5467dc1…387c`) sigue entera en `6c2820ba`: 138 C-7 iguales y sin `Skip`, 55 originales
  iguales con el `Skip` de PB-T4b, 29 de apoyo iguales.
- 14 de las 138 C-7 son guardas que leen el texto de la fuente (no dejan de compilar: caen al ejecutar).
- Nada de PB-T4a, PB-T4b ni PB-T4c nombra lo retirado por código.
- En `src/`, fuera de `Pilot/`, nada lo nombra; WHC:919 usa la sobrecarga de `Precheck` del borrador completo, que se queda.

### Fase 2, la medida en copias exportadas

La retirada es un borrador de medida (no el GREEN de PB-T4d): `v52/retire_pbt4d_v52.py` (`920c57cacfdf6dc5b94e6ed6deba04db1b312418fe9779dbf8e631e1602cb96c`).
- **Variante A:** lo de D-PBT4-10 (borra `PushBackPilotStateComparer.cs` y `PushBackPilotFieldAdapter.cs`; quita `ComposeForWrite`,
  `KeepPilotEditsOn`, `Prepare` del piloto, `Precheck(DraftSession<PushBackPilotDraftState>)` y `Execute` del piloto) y lo que hace falta para
  compilar `src/`: el `Build` privado del planificador y `PushBackPilotPrepareInput.From`. **`src/` compila.**
- **Variante B:** la A y lo que queda sin uso en `src/`: los tipos `PushBackPilotPrepareInput` y `PushBackPilotComposition` y `ComparePost` del
  piloto. (`PushBackPilotHostRun` no existe como tipo: es el nombre de un archivo con tipos que se usan; no entra.)
- Las bajas de `tests/`: las 222 filas de la lista y la `Theory` de SUS-01; el bucle (`v52/measure_retire_v52.py`, `ba521e9d1981efd5004f397dc6608ae25498c19d79dd26acd15698bf2c2327f7` en su
  versión actual, la de `/d/r6e`) compila, lleva cada error a su miembro y repite hasta que compila.
- El conductor: `pbt4dcc/compile/p6d_retire_v52.sh <A|B> <copia>` (`d28d5becbad593fb61b3bca46922f8a3b30db67dcbc2969a30a4d74a4dc3215a` en su versión actual, la de `/d/r6e`; las corridas anteriores llevan en la cabecera de su registro el sha256 de cada pieza que usaron), con `p6d_check_v52.py` (`a41ceff3…05fb`):
  el bucle, la compilación de Core, UI y el Plugin, la suite completa de Core, los 18 filtros de UI de PB-T4c (`pbt4ccc/compile/ui_filters_pbt4c_v50b.txt`),
  la sonda del equivalente de CT-PBT4B-20 (`v52/pbt4d-probe-fullcopy.cs`, `48a9404c…ac23`, que escribe `make_probe_fullcopy_v52.py`) y la tabla por
  clase. Orden: `bash p6d_retire_v52.sh A /d/r6X 2>&1 | tr -d '\r' > driver-r6X-A.txt` (desde `pbt4dcc/compile/`; en segundo plano, ~50 min).

| Copia | Qué | Resultado |
|---|---|---|
| `/d/r6a` | A, primer intento | **PARA**: `body_end` no contaba el `(` del ancla (corregido). Historia |
| `/d/r6b` | A | **PARA** en la vuelta 0: `PilotEditThrowingComparer` (tipo de primer nivel) sin `Equals`/`GetHashCode` (CS0535); el bucle solo miraba tipos anidados (corregido). Historia |
| `/d/r6c` | A | **PARA** en la vuelta 1: `ReadyPlan` (devuelve una tupla) y el constructor de `PilotUpdateSetup` no se llevaban a su miembro (corregido con un buscador de declaraciones propio). Historia |
| **`/d/r6d`** | **A, la que vale** | El bucle cierra en 4 vueltas (abajo). **Compilación:** Core 0 errores y **10 `warning CS8618`** (las propiedades de `PilotUpdateSetup`, que se quedó sin constructor: artefacto del bucle, que quitaba el constructor y no el tipo; corregido después para B: un constructor lleva su tipo entero); UI y Plugin, 0 y 0. **Core:** 19 324 = 18 919 + **4** + 401 (`Skip`: 391 de PB-T4b, 3 de PB-T4c y 7 de otros), 835 s. **UI:** los 18 filtros, 0 `Failed` (el n.º 11, `PushBackFieldValidationConcordanceTests`, se queda con 0 resultados). Registro: `driver-r6d-A.txt` (`2abed539…9699`) y `r6d-A/`. **Cuidado:** en `r6d-A/` el `core.log` y el `sonda.log` son los de las pruebas (el conductor pisaba los de la compilación; corregido con `b-core.log`, etc.); la compilación final de Core está en `r6d-A/bucle/v3-core.log` |
| **`/d/r6e`** | **B, la que vale** | El bucle cierra en 5 vueltas: 223 bajas iniciales y **12 por compilación** (abajo). **Compilación:** Core, UI y Plugin, **0 errores y 0 `warning CS`**. **Core:** 19 322 = 18 919 + **3** + 400 (`Skip`: 390 de PB-T4b, 3 de PB-T4c y 7 de otros), 887 s. **UI:** los 18 filtros, 0 `Failed` (el n.º 11, con 0 resultados, como en A). Registro: `driver-r6e-B.txt` (`1698cf22b8c6b9d5ff440be4e93830694321c3fe2877576b5036448650bdada3`) y `r6e-B/` (`core-res.txt` `4fa093ef…c568`; la sonda, `sonda-lineas.txt` `c955aa02…39d8`) |
| **`/d/r6f`** | **OWNER-05:** `6c2820ba` tal cual, la prueba lenta sola (Debug, como la CI), dos veces, y la sonda de su bucle | **388,6 s y 394,6 s** (6:28 y 6:34; pasa). **La sonda:** un `Prepare` completo toca el catálogo **4 936 veces** (159 ms), y el bucle hace un `Prepare` por cada toque, que falla en el toque n y tarda de 65 ms (n = 0) a 137 ms (n = 4 935): **unos 485 s estimados**. **La causa simple:** el número de vueltas es el de toques del catálogo de un `Prepare` del modelo completo. Registro: `driver-r6f-sl.txt` (`db627c22c6f3e92767742a0ab7b8943849fdf7b2c22c5668f7d5a70cd22cbdc1`) y `r6f-sl/sonda-lineas.txt` (`fc4eed5f…8d9f`) |

**B frente a A:**
- **Las 12 de compilación de B:** las 8 de A (con `tipo:PilotUpdateSetup` entero) y cuatro más: `Assemble` (apoyo de
  `WorkspaceDraftPilotMaterializationTests.cs`, nombra `PushBackPilotComposition`), `StatusAfter` (apoyo de `WorkspacePilotUpdateRunTests.cs`, usa
  `PilotUpdateSetup`; en A no salió porque A dejó el tipo sin constructor), y `ThePlanTypesLiveInThePilotNamespace…` **dos veces**: el original de
  WPP (:2143, saltado por PB-T4b) y **su copia de PB-T4b** (`WorkspacePilotPrepareFull.cs:2311`), que nombran `PushBackPilotPrepareInput`.
- **Lo que cae al ejecutar en B:** 3 (A: 4). La copia `ThePlanTypes…` ya no cae: no compila y sale por compilación. Siguen el equivalente de
  CT-PBT4B-20 y las dos guardas de WPPG (`TheFileOfThePlanIsNullableEnabled…` y `TheOrderOfPrepareIsTheOneOfThePackage…`).
- **La sonda del equivalente de CT-PBT4B-20 en B:** las mismas 11 líneas que en A, salvo dos de WPP: el original `skip=33` (A: 34) y la copia
  `copia WorkspacePilotPrepare pruebas=33 mecanicas=25 a-mano=8 igual-al-mapa=True sha256=False` (A: `pruebas=34 mecanicas=26 … sha256=True`): en B
  **se edita una copia de PB-T4b** (se le quita `ThePlanTypes…`), así que su `CopySha` cambia también.
- **Los avisos:** A, 10 `CS8618` (artefacto del bucle); B, 0.

**El bucle de A (`r6d-A/bucle/resumen.txt`):** 223 bajas iniciales (222 de la lista y SUS-01) y **8 por compilación**:
- `tipo:PilotEditThrowingComparer` (WPEH, apoyo) y dos pruebas de WPEH que lo usan, **las dos ya saltadas por PB-T4b** (originales con su
  equivalente): `TheOuterCatchOfGetFieldAnswersTheViewInErrorWhenTheCopierOfTheStateAnswersNull` y `TheReasonOfAFieldThatCannotBeReadIsInSpanish`;
- `ReadyPlan` (WPP, apoyo) y tres pruebas de WPP que lo usan, **las tres saltadas por PB-T4b**: `TheReadyPlanHoldsTheRack…`, `ThePlanIsImmutable…`
  y `ThePlanIsBuiltOnlyFromTheReread…`;
- `PilotUpdateSetup` (en `WorkspacePilotUpdateScopeDouble.cs`, apoyo): el tipo entero (A quitó su constructor).
- **Las 24 copias candidatas de la fase 1 compilan y pasan:** eran falsos positivos de la lectura por texto. Se quedan.

**Lo que cae al ejecutar en A** (`r6d-A/core-res.txt`), fuera de las bajas:
1. `FullPanelCopyWorkspacePilotPrepareGreenTests.ThePlanTypesLiveInThePilotNamespace…` (`WorkspacePilotPrepareFull.cs:2311`, copia de PB-T4b):
   busca por reflexión `PushBackPilotUpdatePlanner.Prepare`, que ya no está (`Assert.Single`). En B no compilará (nombra `PushBackPilotPrepareInput`).
2. `PushBackEditAllFullCopyGuardTests.CT_PBT4B_20…` (el equivalente de PB-T4c): **la sonda da 11 de sus 53 líneas cambiadas**
   (`r6d-A/sonda-lineas.txt`):
   - `original <X> igual-a-la-base=False` y su `skip=` para siete originales: EIH (skip 62 → 60), WPEG (5 → 5), WPEH (59 → 41), WPP (67 → 34),
     WPRH (28 → 21), WPUG (1 → 1) y WPUIG (2 → 2);
   - `copia <X> pruebas=… mecanicas=… a-mano=…` para cuatro: EIH 62/57/5 → 60/55/5, WPEH 59/53/6 → 41/41/0, WPP 67/46/21 → 34/26/8 y WPRH
     28/17/11 → 21/12/9; sus `igual-al-mapa=True sha256=True` se cumplen;
   - el kit, las cuatro particiones, `FullOf` y las cuatro autopruebas, sin cambio.
   **La propuesta:** sustituirlo por una copia literal (como en PB-T4c) con los `BaseSha` de esos siete originales en su texto de PB-T4d, sus
   `skip=` y los `NineCounts` de esas cuatro copias; las demás líneas, iguales.
3. `WorkspacePilotPrepareGuardTests.TheFileOfThePlanIsNullableEnabled…DeclaresTheTypesOfThePackage` (:82): exige `PushBackPilotPrepareInput.From`.
4. `WorkspacePilotPrepareGuardTests.TheOrderOfPrepareIsTheOneOfThePackage…` (:195): `BodyOf` no encuentra el `Prepare` del piloto.

## 2. Lo que falta

1. **La variante B:** hecha (arriba). Si se quiere A sin el artefacto de los 10 `CS8618`, repetir A en una copia nueva con el bucle actual (quitará
   `PilotUpdateSetup` entero y, con él, `StatusAfter`, como B).
2. **OWNER-05:** hecho (`/d/r6f`, arriba), con `p6d_slow_v52.sh` (`0b76048435340c8019b79b503fc5b252348a2b7544dbfa355271f588365e3b9b`) y la sonda
   `v52/pbt4d-probe-slow.cs` (`8c1dcacd…7bed`, de `make_probe_slow_v52.py`). La prueba es
   `FullPanelCopyWorkspacePilotPrepareGreenTests.InvalidD_ABuilderThatCannotBuild…` (`WorkspacePilotPrepareFull.cs:1911-1944`), y se queda tras la
   retirada (su original, WPP:1633-1666, es una de las 55). **Las opciones para la vuelta al Owner:** dejarla (el riesgo de la CI sigue); acotar su
   bucle a unos puntos que incluyan un fallo del constructor (d) con registro, en PB-T4d, que ya reescribe el equivalente de CT-PBT4B-20 y su
   `CopySha` (unos segundos en lugar de unos 6,5 min); o subir el detector de cuelgue de la CI (`ci.yml:137`, `--blame-hang-timeout 5m`), que es otra
   tarea.
3. **La lista medida** (`v52/pbt4d-retire-list-v52.tsv`, por guion): una fila por baja, con su grupo (lista v4.4 C-7 / original / apoyo; nueva
   por compilación; nueva al ejecutar; SUS-01; M-04), su archivo, miembro y líneas en `6c2820ba`, su `Skip`, su acción (sustituir con registro /
   borrar con registro / se queda) y su porqué, y en qué variante. **Las filas nuevas ya medidas:**
   - **SUS-01:** `WorkspaceDraftPilotWindowLoadCharacterizationTests.TheWindowAndThePilotDifferOnlyInTheKnownClasses` (UI, `Theory` saltada desde
     PB-TC: «SUSTITUIDA por PB-TC (D-PB-06), registro … superseding-characterization.md fila SUS-01: la ejecuta
     PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06…»): borrar con registro;
   - **M-04** (`owner-pbt4b-code-decisions.md`, pregunta 2): PEG:591-598 `TheReadGuardAllowsAnotherRead…` y PEG:916-920
     `TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing` (pasan en vacío: buscan `FindPilotDraft` con la firma del piloto; hoy es
     `DraftSession<PushBackFullDraftState>? FindPilotDraft`, WHC:1841) y su copia `WorkspacePilotEditGuardFull.cs:255-258`: sustituir con registro,
     con la firma del borrador completo;
   - las 8 de compilación y las 4 de ejecución de A (arriba), y lo que añada B;
   - **se quedan:** las 24 copias candidatas y la copia lenta de OWNER-05 (su original, WPP:1633-1666, es una de las 55).
4. **OWNER-02** (medido por texto): «Editar» recarga en `FVM.BeginEdit` (FVM:376-414; la recarga, FVM:386-388: `stale = Clean && IsPossiblyStale`
   → `TryReload`); «Editar todo» no: `FVM.EditAll` (FVM:432-…) y `WHC.EditAllOfPilot` (WHC:1179-1222) no miran `IsPossiblyStale`; el botón se
   ofrece (FVM:236-237), y la caja no es editable en ese caso (FVM:806-814, `EditableOf`). **Lo que tocaría:** o FVM.EditAll (recargar antes, como
   BeginEdit) o WHC.EditAllOfPilot (recargar antes de `ShowModal`). **Las pruebas que lo fijarían:** una CT nueva como
   `WorkspacePilotFieldViewModelTests.cs:612` (`ACleanDraftThatIsPossiblyStale…TheGestureCallsTheReload`) para «Editar todo»; cambian las líneas de
   CT-PBT4C-14 (el orden de llamadas) solo si el caso obsoleto se añade a esa CT; CT-03 gana una línea si va en WHC. Ninguna línea existente cambia.
5. **La v5.2** del paquete, en forma de propuesta (desde la v5.1, `34ca0026…94c3`, por un `make_v52.py`): §6.4 al día (la base, las dos variantes
   con sus cifras, la lista medida por grupos, OWNER-02, OWNER-05, lo que cambia en el contrato: filtros que se quedan sin pruebas, como el n.º 11
   de UI), el registro v5.1 → v5.2 y §8.3 y §11.
6. **Las preguntas para el Owner** (como mucho 4, con opciones y recomendación), y **esperar la respuesta de la sesión.** Candidatas:
   - la lista (A o B; recomendación a la vista de las cifras);
   - OWNER-02 en PB-T4d o en una tarea propia;
   - OWNER-05 (la copia lenta se queda: dejarla, acotar su bucle con registro o subir el detector de la CI);
   - el equivalente de CT-PBT4B-20 (la copia literal con las 11 líneas nuevas) y las guardas de la forma del piloto que caen al ejecutar.

## 3. Decisiones y lecciones

- **Lo que decide la compilación, no la lectura por texto:** las 24 copias candidatas eran falsos positivos; en cambio, la compilación sacó 8 miembros
  que la lectura no veía (un tipo de apoyo de primer nivel, un método que devuelve una tupla y un constructor).
- **El compilador calla los errores de cuerpo si hay uno de declaración** (CS0535): el bucle necesita varias vueltas.
- **Un constructor lleva su tipo entero** (si no, CS8618 en sus propiedades).
- **Los nombres de los registros:** en el conductor, `b-*.log` para las compilaciones y `<trx>.log` para las pruebas (en `/d/r6d` se pisaban).
- **Un desliz que declarar:** un `python -I -` con un heredoc vacío (no ejecutó nada; se paró con TaskStop).
- El `__pycache__` de `pbt4ccc\` (18:19) es del guion de custodia de la sesión.
