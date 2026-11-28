> **Redacción para el repo público:** las rutas locales se sustituyen por marcadores (`<worktree-de-I-64>`, `<carpeta-del-paquete>`). El resto no cambia.

# I-64 F4B — Paquete del contrato de PB-T3 (la escritura completa del Workspace), v1

**Estado:** v1, borrador para la revisión independiente adversarial. **No se emite.** Antes de emitir cada subtarea, se vuelve a medir sobre el cierre de PB-T1 (PB-T1b en su intento 2 y PB-T1c), con las órdenes del registro `pbt3-measurements.md`. No declara GATE PASS.

**Lo que pide el Owner** ([OD], fila de PB-T3 del §5 de [P], literal):

> ACCEPT IN PRINCIPLE. The Workspace authored writer may expand to full Push Back design. It must eventually support, as characterized/authorized: field changes; modules/fronts; side B; rename; deleting obsolete views; blocking authored write on diagnostics/errors; view creation only if PB-T0 subsequently authorizes it. Still require: PREPARE -> MUTATE -> POST and one logical Update.

**Lo que propone este paquete:**
- **Un solo escritor, generalizado donde está.** El plan y la corrida del piloto pasan a servir dos modelos de borrador:
  - el del piloto, conectado al panel y **sin ningún cambio de comportamiento** (solo la escala);
  - el completo, sobre `PushBackEditableState`, probado en Core con dobles y **sin conectar** hasta PB-T4.
  
  No hay un segundo camino de escritura (F4: «NO SECOND WRITE PATH»).
- **Tres subtareas secuenciales**, cada una con su cadena y su presupuesto de 1/3 (OB-10.1):
  - **PB-T3a — el modelo** (`F4B-PBT3A-FULL-WRITE-MODEL`): solo archivos nuevos de Application. El borrador completo, su asiento, su lector, la composición para escribir y para «Conservar mi borrador», la disposición de las vistas y el clasificador de D-PB-05. Ninguna guarda de F4 cambia.
  - **PB-T3b — la escritura** (`F4B-PBT3B-FULL-WRITE-RUN`): el plan y la corrida del piloto, generalizados en su sitio, con la barrera de diagnósticos, el borrado de laterales obsoletos, el renombrado, `ComparePost` genérico y la capacidad de borrar como interfaz nueva. El Plugin no cambia.
  - **PB-T3c — el borrado en el dibujo** (`F4B-PBT3C-FULL-WRITE-ERASE`): la primitiva caller-owned del Plugin y su uso por el escritor, con sus guardas de texto.
- **Sin crear vistas** (H-PB-16 confirmado por el bloque M; fila 1 del §6.4). **Sin tocar** RACKEDITAR, la ventana, el anfitrión ni el puerto del panel.

**Decisiones materiales nuevas para el Owner:** **una**, D-1 (§8.2), que hace falta antes del contrato de PB-T3b, no de PB-T3a. Seis métodos de prueba de F4 no pueden seguir en verde con un escritor que renombra, borra y es genérico. Se propone sustituirlos con registro declarado.

**Precondiciones de emisión** (de cada subtarea, salvo que se diga otra cosa):
1. **PB-T1 cerrada:** PB-T1b tras su intento 2 (F4B-E22) y PB-T1c.
2. **La remedición sobre el cierre de PB-T1**: las anclas del registro (R-1 a R-10), el censo de guardas de R-6 y la línea base de la CI. Una diferencia que cambie un CT, el diseño o la lista de D-1 vuelve a una comprobación acotada.
3. **La revisión independiente de este paquete** y, por subtarea, su comprobación acotada.
4. **Para PB-T3b y PB-T3c: D-1 decidida por el Owner** (§8.2).
5. **Para PB-T3b: PB-T3a cerrada.** Para PB-T3c: PB-T3b cerrada.
6. Las celdas no están `STALE` (vencen el 2026-12-29); el `config.toml` coincide con la Snapshot I; el binario de Codex es el acreditado.
7. **PB-T2** puede ir antes o después (R-PB-09): las delegaciones son sucesivas, cada una sobre la punta de la anterior, y los alcances son disjuntos (§7, R-T3-09).

---

## 1. Autoridades

| Autoridad | Qué fija |
|---|---|
| **[OD]** (`F4B-DRAFTS/coordinator/owner-f4b-disposition.md`) | La fila de PB-T3 (arriba); D-PB-04 (borrar sí; crear, solo tras PB-T0); D-PB-05 (los pasos 1 a 7 y «an informed destructive choice»); D-PB-10 (1/3; el intento 3 vuelve al Owner); §7.3 (no debilitar guardas; registro declarado de sustitución); §7.4 (RACKEDITAR) |
| **[P]** (`docs/initiatives/I-64-f4b-push-back-completo-package.md` v3.1) | La fila de PB-T3 del §5 (`:505`); H-PB-16 a H-PB-23; OB-03.1 y OB-03.2; OB-04.1 y OB-04.2; OB-05.1 a OB-05.5; §6.4; OB-10.1; OB-T1B.1 a OB-T1B.3; OB-RE.1; R-PB-03, R-PB-05, R-PB-06, R-PB-09 y R-PB-11 |
| **El bloque M de PB-T0** (`F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md`) | H-PB-16 confirmado y la fila 1 del §6.4 resuelta (`:92-97`); lo que deja para PB-T3 (`:133-139`) |
| **[PKT1]**, el paquete de PB-T1 v2.7 (`docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md`) | D-PBT1-02 a D-PBT1-11 (el tipo, las cargas, la salida, el copiador, el comparador, el mapa y el criterio de bloqueo); D-PBT1-15 (D2); §5.0 (recetas y `Pasada`); §6.0 (lo común de los contratos); §10 (arrastres A-2 y A-3) |
| **D2 del Owner** (F4B-E11, `F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md`) | Las dos referencias compartidas, las diez condiciones y el límite «STOP and reopen L-1» |
| **La decisión del Owner del 2026-11-19 sobre la ExtensionData anidada** (`owner-f4b-dpb07-extensiondata-decisions.md`, pregunta 2; la vuelta, `owner-return-extensiondata.md`) | «Basta así (Recomendado)»: el panel mantiene el rechazo de AUTH-13 y PB-T3 lo fija con una prueba; la pérdida de RACKEDITAR es una limitación conocida anterior a F4B. Su registro lo custodia la sesión en `F4B-DRAFTS/` con su número F4B-E |
| **F4B-E21** (`F4B-DRAFTS/coordinator/owner-f4b-opaque-collections-decision.md`) | La sesión pone las ocho colecciones opacas del contrato |
| **F4B-E22** (`F4B-DRAFTS/coordinator/owner-pbt1b-rework-decisions.md`) | Las ratificaciones M10 y M14 valen solo para PB-T1b: este paquete corrige el texto (§6.0) |
| **[DP]** (`F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md`, F4B-E03) | Los sustitutos de nc1 y nc2 solo con la firma EXACTA; una replanificación solo por la comparación literal |
| **[PKC-C]** y **[PKC2]**, los paquetes de PB-TC y PB-TC2 (`docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md`, `docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md`) | El método del `ForbiddenWriteScope` ([PKC-C]); las 17 paradas estándar, la raíz y las celdas ([PKC2] §6.7, `:655`) |
| **El cierre de F4** (`F4-DRAFTS/gate-close.md`) y los contratos de F4-T4 (`docs/initiatives/I-64-f4-t4*-gate-contract-package.md`) | El PREPARE, MUTATE y POST que se generaliza; S2-03 (un paso de Deshacer); las guardas G-1 a G-10 |
| **I-54 G4B** (`CustomPropertiesEnvelopeGuardTests`, `ExpectedComposeCensus`) | El censo cerrado de los que componen un sobre |
| **I-58 (AUTH-13)** y su matriz (`I58F1Matrix.cs:58-64`, `:97-102`) | El rechazo de todo campo desconocido a cualquier nivel |
| ADR-0029 D12 y D13; Proposal V5 D-10, D-13, D-15 (punto 5) y D-16 | Sin modelo paralelo; la caracterización no se edita para que pase; Actualizar todo o nada y un paso de Deshacer; escritor caller-owned; sin cambio observable en RACKEDITAR |
| `docs/AUTOMATION_PLAN.md` §16.3, §16.5, §16.8 a §16.11; `docs/automation/agent-execution/README.md` §4 y §10 | Las autoridades por clase, A1-A8, el RED de la cadena, las comprobaciones, los términos y las paradas |

## 2. Hechos medidos en `2f5d3dec`

El detalle, con sus anclas, está en `pbt3-measurements.md` (registros R-0 a R-12). Este apartado resume lo que decide el diseño.

### 2.1 Lo que escribe hoy el piloto y lo que le falta, punto por punto del [OD]

| Punto | Hoy | Lo que falta | Registro |
|---|---|---|---|
| **Campos** | Solo la escala. `ComposeForWrite` rechaza cualquier otra diferencia y rematerializa la base | Escribir todo el estado editable | R-1 |
| **Módulos y frentes** | Rechazados: el estado del piloto no los lleva sin pérdida | El estado completo de PB-T1 | R-1 |
| **Lado B** | Un rack compuesto o con cola aparcada es «fuera del piloto (F5)» | Admitir A/B en el modelo completo; la cola aparcada sigue fuera (D-PBT1-10) | R-1 |
| **Renombrar** | El nombre sale del sobre releído de cada vista | El nombre del borrador en el sobre; los nombres de las definiciones salen de él | R-1, R-2 |
| **Borrar vistas obsoletas** | Una vista no disponible hace la preparación `Invalid` («El piloto no borra vistas…») | Borrar laterales obsoletos dentro de la transacción, con la regla del último vínculo | R-1, R-2, R-3 |
| **Barrera de diagnósticos** | No existe | `PushBackEditorOutputGate` en PREPARE | R-1, R-4 |
| **PREPARE, MUTATE y POST; Deshacer** | Una transacción en MUTATE; POST en el mismo contexto de comando; Smoke-2 midió una sola entrada de Deshacer | Lo mismo, con los borrados dentro de la misma transacción | R-1 |
| **`ComparePost`** | Claves, sobres en ordinal y escala | Que las claves borradas ya no estén; la escala del modelo que corresponda | R-1 |
| **AUTH-13** | El lector rechaza todo campo desconocido a cualquier nivel, con aviso y remisión a RACKEDITAR | Fijarlo con una prueba en el camino completo (decisión del Owner del 2026-11-19) | R-1, R-5 |
| **Conflictos** | Comparación cruda de la base antes de la admisión (PREPARE) y otra vez en MUTATE | Sin cambio; el destino incluye las vistas a borrar | R-1 |

### 2.2 Lo que hace el clásico y PB-T3 iguala en resultado

- **Nunca crea vistas al actualizar** (H-PB-16, M-1 y M-4).
- **Borra solo laterales obsoletos:** los que no tienen corte en el sistema nuevo. Solo si queda alguna vista superviviente; si no, no escribe nada (M-2b, BM:61).
- **Renombra todas las vistas** con un solo nombre (el de la ventana o, en blanco, el del sobre elegido). Con colisión, la definición no cambia de nombre (M-3).
- **El frontal B de un rack que deja de ser compuesto** se redibuja como frontal de un sentido y conserva `Section` 2 (M-4, H-PB-18). PB-T3 conserva por defecto el rechazo del panel (§6.4, fila 3; D-PBT3-06).
- **Un `U` deshace cada «Actualizar»**, también con borrado o renombrado (M-5).
- **Las vistas se cuentan por definición**, con o sin referencia (BM:49-53). El panel exige además una superviviente con referencia (C-F4-05, R-10).
- **El borrado alcanza todas las referencias directas** de la definición, también las anidadas, y purga después las definiciones anidadas (RCS:218-275; BM:54-60).

### 2.3 Lo que PB-T1b deja en Application

`PushBackEditableState` con sus cargas, `BuildInputs` y las reglas de c-02 y c-09; el copiador y el comparador; el mapa de propiedad; `PushBackEditorOutputGate` (R-4). **No hay API de asiento:** la pasada (`Pasada`) solo existe copiada en las pruebas. PB-T3a la añade al producto, como prevé A-2.

### 2.4 Las guardas que atan el diseño

Las de F4-T4 fijan por texto el plan, la corrida, el puerto y el escritor (R-6):
- **seis métodos** no pueden seguir en verde con el diseño (S-1 a S-6: D-1, §8.2);
- **el resto** se mantiene en verde con elecciones de diseño explícitas (§4).

El censo de I-54 de los que componen un sobre es cerrado: ningún archivo nuevo compone ni construye un `RackEmbedDocument`.

---

## 3. El diseño

### D-PBT3-01. Un escritor, dos modelos de borrador y tres subtareas

**Por qué no un segundo escritor:**
- las guardas de F4 prohíben otro camino de escritura (G-2, G-7);
- [OD] habla de «The Workspace authored writer», en singular;
- ADR-0029 D12 prohíbe un modelo paralelo.

**Por qué el modelo completo no se conecta en PB-T3:**
- conectar el borrador completo al panel es cambiar el tipo del borrador del anfitrión y del puerto del panel, que es «el puerto generalizado» de PB-T4 ([P] §5);
- OB-05.2 manda dejar conectada hasta PB-T4 la semántica de hoy de «Conservar mi borrador» (`KeepPilotEditsOn`);
- quitar el rechazo A/B del panel (OB-03.2) solo tiene sentido cuando el panel use el estado completo: va con PB-T4 (A-3).

**Por qué tres subtareas:**
- tres regímenes distintos: solo archivos nuevos (PB-T3a), cambios en archivos de F4 con sustituciones declaradas (PB-T3b) y Plugin con guardas de texto (PB-T3c);
- PB-T3a se puede emitir sin D-1, y PB-T3b no;
- el tamaño: PB-T3 entera serían unos 33 CT, seis sustituciones y cambios en cinco archivos de producto, más que PB-T1b (17 CT).

**La secuencia:** PB-T3a → PB-T3b → PB-T3c, cada una desde la punta que dejó cerrada la anterior, sin rebase, con delegaciones sucesivas (`AUTOMATION_PLAN.md:557-558`). **Si la revisión lo prefiere,** PB-T3b y PB-T3c se pueden fundir; no cambia ningún CT.

### D-PBT3-02. El borrador completo (PB-T3a)

**Dónde:** una carpeta nueva, `src/RackCad.Application/Systems/PushBack/Draft/`, con `namespace RackCad.Application.Systems.PushBack.Draft` de bloque (`NamespaceFolderGuardTests`). Ni en `Pilot/`, cuyas guardas atan el texto de esa carpeta, ni en `Workspace/`.

**`PushBackFullDraftState.cs`:**

```csharp
public sealed class PushBackFullDraftState
{
    public PushBackFullDraftState(PushBackEditableState editable, string origin); // ArgumentNullException si alguno es nulo
    public PushBackEditableState Editable { get; }
    public string Origin { get; } // el texto crudo del sobre del que se materializó; en el estado inicial, siempre la base actual
}
```

**`PushBackFullDraftStateComparer.cs`:**

```csharp
public sealed class PushBackFullDraftStateComparer : IEqualityComparer<PushBackFullDraftState>
{
    public static PushBackFullDraftStateComparer Instance { get; }
    public bool Equals(PushBackFullDraftState x, PushBackFullDraftState y);
    public int GetHashCode(PushBackFullDraftState obj);
    public static PushBackFullDraftState Copy(PushBackFullDraftState state);
}
```

- **`Equals`:** `ReferenceEquals` da `true`; un nulo da `false`; si no, `Origin` en ordinal y `PushBackEditableStateComparer.Instance.Equals(x.Editable, y.Editable)`.
- **`GetHashCode`:** `HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.Origin), PushBackEditableStateComparer.Instance.GetHashCode(obj.Editable))`.
- **`Copy`:** `new PushBackFullDraftState(PushBackEditableStateComparer.Copy(state.Editable), state.Origin)`. **D2:** la copia comparte solo lo que comparte el copiador de PB-T1b.

### D-PBT3-03. El asiento y el materializador (PB-T3a)

**`PushBackEditableStateSeat.cs`** (la «API de asiento» de A-2):

```csharp
public static class PushBackEditableStateSeat
{
    public static PushBackEditorComputation Pass(
        PushBackEditableState state, PushBackEditorDesignAssembler assembler,
        PushBackCompositeEditorAssembler compositeAssembler, PushBackSide frontalSide);
    public static PushBackEditableState LoadExisting(PushBackDesign design, string rackName, RackCatalog catalog);
}
```

- **`Pass`** es la copia literal del ayudante `Pasada` de [PKT1] §5.0 (W:659-704), con todas sus sentencias y en su orden. La única diferencia es que pasa de privado de prueba a público de producto. Una excepción del armado da `null`, como W:662-670.
- **`LoadExisting`** reproduce la fila «`LoadExisting` o `LoadDesignForNew`» de la tabla de pasadas de [PKT1] §5.0:
  1. `design == null` → `ArgumentNullException` antes de crear nada (M11 de PB-T1b);
  2. un estado nuevo; dos ensambladores nuevos con el catálogo, como W:145-146;
  3. `LoadNew(catalog)`; `Pass`; `SeedDefaultSafety(catalog)`; `Pass`;
  4. `LoadDesign(design, rackName, assembler.Resolver)`; `Pass`;
  5. siempre con `frontalSide = PushBackSide.A`.
  
  Devuelve el estado.
- **Por qué esta secuencia:** es la que CT-PBT1C-01 iguala con la ventana tras `LoadExisting`. Así el borrador completo nace igual que el estado que tendría la ventana al abrir ese rack, que es lo que PB-T4 abrirá con «Editar todo».

**`PushBackFullDraftMaterializer.cs`:**

```csharp
public sealed class PushBackFullDraftMaterialization
{
    public PushBackPilotMaterializationKind Kind { get; }
    public bool IsAdmitted { get; }
    public PushBackFullDraftState State { get; }   // solo si Admitted
    public string Reason { get; }                  // null si Admitted
    public static PushBackFullDraftMaterialization Admitted(PushBackFullDraftState state);
    public static PushBackFullDraftMaterialization OutOfPilot(string reason);
    public static PushBackFullDraftMaterialization Failed(string reason);
}

public static class PushBackFullDraftMaterializer
{
    public static PushBackFullDraftMaterialization Materialize(string originText, RackCatalog catalog);
}
```

**`Materialize`**, total y en este orden. Los textos son las constantes del piloto, que no se duplican:
1. texto en blanco → `Failed(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic)`; catálogo nulo → `Failed(PushBackPilotMaterializer.CatalogRequiredDiagnostic)`;
2. `new RackEmbedStore().Deserialize(originText)`:
   - nulo → `Failed(UnreadableOrFutureDiagnostic)`;
   - `Kind` distinto de `RackEmbedDocument.KindPushBack` en ordinal → `Failed(KindDivergentDiagnostic)`;
3. `new RackProjectStore().TryDeserialize(embed.Design, …)`:
   - falso → `Failed(UnreadableOrFutureDiagnostic)`;
   - sin `PushBackDesign` → `Failed(NoDesignDiagnostic)`;
4. `PushBackEditableState.SupportOf(design) == DormantCompositeTail` → `OutOfPilot(PushBackPilotMaterializer.DormantTailOutOfPilotReason)` (D-PBT1-10; OB-03.1);
5. `Admitted(new PushBackFullDraftState(PushBackEditableStateSeat.LoadExisting(design, embed.Name, catalog), originText))`;
6. cualquier excepción → `Failed(PushBackPilotMaterializer.MaterializeFailurePrefix + ex.Message)`.

**La diferencia con el piloto:** un diseño compuesto **se admite** (el piloto lo rechaza en PM:148). Es el modelo completo, sin conectar al panel. Lo demás es igual.

### D-PBT3-04. El lector completo, sobre el lector del piloto (PB-T3a)

**`PushBackFullDraftReader.cs`:**

```csharp
public sealed class PushBackFullIntentResult
{
    internal PushBackFullIntentResult(DraftStaleBase staleBase, DraftIntentRead<PushBackFullDraftState> read,
        string originKey, string detail, string classicEditorHint);
    public DraftStaleBase StaleBase { get; }
    public DraftIntentRead<PushBackFullDraftState> Read { get; }
    public string OriginKey { get; }
    public string Detail { get; }
    public string ClassicEditorHint { get; }
}

public static class PushBackFullDraftReader
{
    public static PushBackFullIntentResult Read(PushBackPilotIntentScan scan, RackCatalog catalog);
}
```

**`Read`**, total. No cambia el lector del piloto, ni sus 14 reglas, ni AUTH-13:
1. `var pilot = PushBackPilotIntentReader.Read(scan, catalog);` (una sola vez);
2. **si `pilot.Read.IsAdmitted`**, o si el rechazo es **exactamente** el compuesto (`Diagnostic == PushBackPilotIntentReader.OutOfPilotDiagnostic` y `pilot.Detail == PushBackPilotMaterializer.CompositeOutOfPilotReason`, en ordinal; solo pasa tras las reglas 1 a 11, AUTH-13 incluida):
   - `PushBackFullDraftMaterializer.Materialize(scan.RawEnvelopes[scan.OriginKey], catalog)`;
   - `Admitted` → `DraftIntentRead.Admitted(pilot.StaleBase, state)`, con detalle y pista nulos;
   - `OutOfPilot` → `NotAdmitted(OutOfPilotDiagnostic)`, con el motivo como detalle y la pista `ClassicEditorHint`;
   - `Failed` con `UnreadableOrFutureDiagnostic` → `NotAdmitted(UnreadableDiagnostic)` con la pista;
   - otro `Failed` → `NotAdmitted(MaterializeDiagnostic)`, con el motivo como detalle y la pista;
3. **si no:** `NotAdmitted` con el diagnóstico, el detalle y la pista del piloto;
4. la base siempre es `pilot.StaleBase` y la clave del origen, `pilot.OriginKey`; una excepción → `NotAdmitted(MaterializeDiagnostic)` con la base de `PushBackPilotIntentReader.BuildStaleBase(scan)`.

**Por qué así:** el lector del piloto y sus guardas no cambian (R-6), y AUTH-13 sigue decidiendo antes que cualquier materialización. La admisión del compuesto depende de dos constantes exactas del piloto. CT-PBT3A-03 la fija regla a regla frente al lector del piloto.

### D-PBT3-05. La composición para escribir (PB-T3a; OB-05.2)

**`PushBackFullComposition.cs`:**

```csharp
public sealed class PushBackFullWriteComposition
{
    public bool IsComposed { get; }
    public PushBackCompositeEditorState Editor { get; }
    public PushBackEditorInputs Inputs { get; }
    public string Name { get; }   // el del borrador, tal cual (puede ser nulo o estar en blanco)
    public string Reason { get; }
    public static PushBackFullWriteComposition Composed(PushBackCompositeEditorState editor, PushBackEditorInputs inputs, string name);
    public static PushBackFullWriteComposition Rejected(string reason);
}

public static class PushBackFullComposition
{
    public static PushBackFullWriteComposition ForWrite(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog);
    public static PushBackFullDraftState KeepOn(PushBackFullDraftState newInitial, PushBackFullDraftState current);
}
```

**`ForWrite`**, pura y total. Solo es segura **después** de que la base coincida con una relectura (H-06), como la del piloto; la llama solo el plan (D-PBT3-09):
1. `initial` o `current` nulos → `Rejected("Falta el estado inicial o el actual.")` (el texto de PM:201); catálogo nulo → `Rejected(PushBackPilotMaterializer.CatalogRequiredDiagnostic)`;
2. **(6) de D-T4A1-05:** `current.Editable.AnnotationScale` no finita o ≤ 0 → `Rejected(PushBackPilotMaterializer.InvalidScaleReason)`;
3. **(3) de D-T4A1-05:**
   - `var fresh = PushBackFullDraftMaterializer.Materialize(initial.Origin, catalog)`; si no se admite → `Rejected(fresh.Reason ?? UnreadableOrFutureDiagnostic)`;
   - si `!PushBackFullDraftStateComparer.Instance.Equals(initial, fresh.State)` → `Rejected(PushBackPilotMaterializer.BaseChangedReason)`;
4. **la composición:**
   - `var copy = PushBackEditableStateComparer.Copy(current.Editable)`;
   - `Composed(copy.Editor, copy.BuildInputs(new PushBackEditorDesignAssembler(catalog)), copy.Name)`;
5. una excepción → `Rejected(PushBackPilotMaterializer.ComposeFailurePrefix + ex.Message)`.

**El reparto de los pasos 4 a 7 de D-PB-05:**

| Paso | Dónde se cumple |
|---|---|
| **4.** «all fields OWNED by the full Push Back editor come from the user's draft» | El editor, los portadores y el nombre salen de `current` (paso 4 de `ForWrite`). Según el mapa de PB-T1b, todo el estado editable es `Editor` (PESO:142, :165-172) |
| **5.** el sobre y la infraestructura salen del rack actual | En el plan (PB-T3b): cada miembro se compone sobre su sobre **releído** (identidad, tipo, vista, sección, `CustomProperties` y cajón) y con su proyecto **releído** como fuente (proyecto y carga útil: versión y cajón). Ya es así hoy (PP:421-422, PP:434-438; R-5) |
| **6.** «unknown/future ExtensionData must not be silently destroyed» | **Nivel superior:** se conserva por construcción (paso 5). **Anidado:** AUTH-13 rechaza el rack antes de escribir, con aviso. Es la decisión del Owner del 2026-11-19, y la fijan CT-PBT3A-04 y CT-PBT3B-10 |
| **7.** identidad, esquema y sesión no vuelven atrás | El GUID y el esquema salen del sobre releído (paso 5); la sesión es la del `DraftSession`; `Origin` sale siempre de la base nueva (`KeepOn`) |

**`KeepOn`**, la composición de «Conservar mi borrador» (OB-05.2), **sin conectar** (PB-T4, OB-05.5):
- `newInitial` o `current` nulos → `ArgumentNullException`;
- si no, `new PushBackFullDraftState(PushBackEditableStateComparer.Copy(current.Editable), newInitial.Origin)`.

Es pura: no muta ninguno de los dos argumentos. `DraftSession.KeepMineOnCurrentVersion` la recibe como `keepOn` (DS:469-486) y vuelve a copiar.

**Lo que no hace:** no compara el borrador con nada que «el editor no edite», porque en el modelo completo no hay nada así (OB-05.1). No toca `KeepPilotEditsOn`, que sigue conectada (OB-05.2).

### D-PBT3-06. La disposición de las vistas (PB-T3a; H-PB-17, H-PB-18, §6.4)

**`PushBackViewDisposition.cs`:**

```csharp
public enum PushBackViewDispositionKind { Redraw = 0, ObsoleteLateral = 1, FrontalBOfOneWay = 2, NoAddress = 3 }

public sealed class PushBackViewDispositionPlan
{
    public IReadOnlyList<KeyValuePair<string, PushBackViewDispositionKind>> Members { get; } // por clave, ordinal
    public IReadOnlyList<string> Redraw { get; }
    public IReadOnlyList<string> ObsoleteLaterals { get; }
    public IReadOnlyList<string> FrontalBOfOneWay { get; }
    public IReadOnlyList<string> NoAddress { get; }
    public bool AllObsolete { get; } // hay miembros y todos son ObsoleteLateral
}

public static class PushBackViewDispositions
{
    public static PushBackViewDispositionPlan Of(
        IEnumerable<KeyValuePair<string, RackEmbedDocument>> members, PushBackSystem outgoing, RackCatalog catalog);
}
```

**La regla** es la del clásico (RPC:268-271, :320-333) y la de la disponibilidad pura (RVA:94-116):
- los hechos se calculan **una vez**: `new PushBackViewAvailabilityFacts(new PushBackSystemLateralBuilder().Cortes(outgoing, catalog).Select(c => c.PostIndex), outgoing.IsComposite)`;
- por miembro: `RackViewCodec.Decode(envelope.Kind, envelope.View, envelope.Section)`:
  - sin dirección → `NoAddress`;
  - `RackViewAvailability.Evaluate(...) == Available` → `Redraw`;
  - si no, un lateral → `ObsoleteLateral`;
  - un frontal del lado B → `FrontalBOfOneWay`;
  - cualquier otro → `NoAddress`.

**La regla del frontal B (§6.4, fila 3; R-PB-11).** **Se propone el valor por defecto:** el panel **rechaza** la edición, con un texto que remite a RACKEDITAR (D-PBT3-09). Las dos alternativas, con la evidencia de M-4 (BM:25, :67-72):

| Alternativa | Contra |
|---|---|
| (1) Borrarlo como vista hermana obsoleta (D-PB-04 lo autoriza) | Hace distinto al panel y al clásico, que lo conserva (M-4) |
| (2) Redibujarlo como el clásico | Deja un sobre con `Section` 2, la entrada del lado B, en un rack sin lado B. Y su renombrado choca con el frontal de A, si existe, lo que no se midió (BM:72) |

La revisión de este paquete decide. **No es del Owner**, salvo que la revisión la considere material (BM:96).

### D-PBT3-07. El clasificador de D-PB-05 (PB-T3a; OB-05.3), sin conectar

**`PushBackConflictClassifier.cs`:**

```csharp
public enum PushBackChangeGroup { Name = 0, Rack = 1, Annotations = 2, Safety = 3, Fronts = 4, Modules = 5, SideB = 6, Composite = 7, OutsideEditor = 8 }
public enum PushBackChangeClass { ChangedExternally = 0, ChangedByUser = 1, Overlapping = 2 }

public sealed class PushBackChangeEntry
{
    public PushBackDocumentLevel Level { get; }
    public string Path { get; }          // $.A.B[3].C, con los índices reales
    public PushBackChangeGroup Group { get; }
    public PushBackChangeClass Class { get; }
    public bool IsStructural { get; }
    public string BaseValue { get; }     // el texto JSON de la hoja, o «<ausente>»
    public string ExternalValue { get; }
    public string DraftValue { get; }
}

public sealed class PushBackConflictClassification
{
    public IReadOnlyList<PushBackChangeEntry> Entries { get; }  // por nivel y por ruta, ordinal
    public IReadOnlyList<PushBackChangeEntry> ChangedExternally { get; }
    public IReadOnlyList<PushBackChangeEntry> ChangedByUser { get; }
    public IReadOnlyList<PushBackChangeEntry> Overlapping { get; }
    public IReadOnlyList<PushBackChangeEntry> Structural { get; }
    public IReadOnlyList<string> ViewsToErase { get; }
    public IReadOnlyList<string> ViewsRejected { get; }
    public bool AllViewsObsolete { get; }
}

public static class PushBackConflictClassifier
{
    public static PushBackConflictClassification Classify(
        PushBackFullDraftState baseState, PushBackFullDraftState currentRack, PushBackFullDraftState draft,
        IEnumerable<KeyValuePair<string, RackEmbedDocument>> currentMembers, RackCatalog catalog);
}
```

**Qué compara:**
- **El diseño de cada uno de los tres estados**, armado sobre una **copia**: `new PushBackCompositeEditorAssembler(catalog).BuildDesign(copy.Editor, copy.BuildInputs(new PushBackEditorDesignAssembler(catalog)))`. Se persiste con `new RackProjectStore().Serialize(RackProject.ForPushBack(design))`, y se toma el objeto `$.PushBack` (nivel `Payload`).
- **El nombre:** `Editable.Name` (nivel `Envelope`, `$.Name`).
- **El sobre y el proyecto de `Origin`,** en el resto de sus hojas (niveles `Envelope` y `Project`, sin `$.Design` ni `$.PushBack`).

**El diff:** hoja a hoja sobre `System.Text.Json.Nodes`, con las rutas de PESO (`$.A.B[3].C`) y los arrays por índice. Una hoja es un valor, un objeto vacío, un array vacío o `null`. El texto de la hoja es su `ToJsonString()`, o `<ausente>`. Las tres versiones son b (base), c (rack actual) y d (borrador):
- b = c = d → nada;
- c ≠ b y d = b → `ChangedExternally`;
- d ≠ b y c = b → `ChangedByUser`;
- d ≠ b y c ≠ b → `Overlapping`.

**Los grupos**, por el prefijo más largo:

| Nivel | Prefijo | Grupo |
|---|---|---|
| Envelope | `$.Name` | `Name` |
| Envelope y Project | lo demás | `OutsideEditor` (siempre gana el rack actual: lo informa, no lo decide) |
| Payload | `$.SchemaVersion` | `OutsideEditor` |
| Payload | `$.Structure.Fronts`, `$.Fronts` | `Fronts` |
| Payload | `$.Structure.Modules`, `$.Structure.HeaderLineOverrides`, `$.Structure.DerivedPostLineOverrides` | `Modules` |
| Payload | `$.SideB` | `SideB` |
| Payload | `$.Composite` | `Composite` |
| Payload | `$.Structure.SafetySelections`, `$.RearTopeSaque`, `$.RearTopePieceId`, `$.RearTopeOffCells`, `$.DefensePieceId` | `Safety` |
| Payload | `$.Structure.NumberFronts`, `.NumberLevels`, `.DrawRackName`, `.AnnotationScale`, `.Dimensions`, `.DimensionViews`, `.DimensionStyle` | `Annotations` |
| Payload | lo demás de `$.Structure` y `$.LegacyHighEndBeamPeralte` | `Rack` |

**Estructural** («structural changes such as modules/fronts/side B», [OD]): todo `Modules`, `SideB` y `Composite`, y en `Fronts` toda entrada en la que algún valor es `<ausente>` (un frente que aparece o desaparece).

**Implicación de vistas:** `PushBackViewDispositions.Of(currentMembers, <sistema del diseño del borrador>, catalog)`:
- `ViewsToErase` = `ObsoleteLaterals`;
- `ViewsRejected` = `FrontalBOfOneWay` ∪ `NoAddress`;
- `AllViewsObsolete`.

Creadas, ninguna: F4B no crea vistas.

**Por qué no es «a generic JSON diff as the user-facing UX»:** es una clasificación interna con grupos y clases con nombre. Cómo se presenta lo decide PB-T4 (OB-05.3, OB-05.4).

### D-PBT3-08. Lo que no cambia en PB-T3a

Ningún archivo existente de `src/`. El piloto, el anfitrión, el puerto del panel, la ventana, el Plugin y Domain no se tocan. Ningún archivo nuevo compone ni construye un `RackEmbedDocument` (I-54 G4B) ni nombra `Autodesk`, `RackCad.Plugin`, `RackCad.UI`, `System.Windows`, `Transaction`, `Database`, `async`, `await` ni `Task`.

### D-PBT3-09. El plan generalizado (PB-T3b)

**Dónde:** PP, en su sitio. **Ninguna firma pública o interna existente cambia**, porque las pruebas de F4 compilan contra ellas; solo se añade.

**El estado de entrada del modelo completo**, `PushBackFullPrepareInput`, nueva en PP. Es la gemela de `PushBackPilotPrepareInput`, que no cambia:

```csharp
public sealed class PushBackFullPrepareInput
{
    private PushBackFullPrepareInput(...);          // una sola construcción, en From
    public string RackId { get; }
    public DraftStaleBase StaleBase { get; }
    public PushBackFullDraftState InitialDraftState { get; }  // copia
    public PushBackFullDraftState CurrentDraftState { get; }  // copia
    public bool HasPendingText { get; }
    public bool IsDirty { get; }
    public DraftStatus ConsideredStatus { get; }
    public static PushBackFullPrepareInput From(DraftSession<PushBackFullDraftState> draft);
}
```

**El planificador** gana `public static PushBackPilotPrepareResult Prepare(PushBackFullPrepareInput input, PushBackFullIntentResult reread, RackCatalog catalog)`:
- con **el mismo orden** que el del piloto: NotApplicable, PendingText, NothingToApply, Conflict (`CompareWith` antes de la admisión), NotAdmitted y `Build`;
- total;
- va **después** del `Prepare` del piloto en el archivo, para que el `Prepare` y la primera `.CompareWith(` del texto sigan siendo los del piloto (guardas P5 a P7 de R-6).

**El armado común,** `BuildCore` (privado). Lo llaman los dos `Build`:
- **el del piloto** conserva, en su cuerpo, la única llamada textual a `PushBackPilotMaterializer.ComposeForWrite(`, después de `.CompareWith(` (R-6, P5);
- **el completo** llama a `PushBackFullComposition.ForWrite(`.

Recibe la composición (editor, entradas y nombre, o el motivo del rechazo), si se admite borrar (piloto: no; completo: sí), la escala y la base. Hace, en este orden:
- **(a)** composición rechazada → `Invalid` con su motivo (como hoy);
- **(b) a (c)** los miembros, el origen y las fuentes interiores, **sin cambio** (PP:347-373);
- **(d)** el diseño y el sistema salientes, sin cambio (PP:375-388);
- **(d2) la barrera (nueva):** `PushBackEditorOutputGate.IsBlocked(editor, system)` → `Invalid(BlockedPrefix + PushBackEditorOutputGate.StatusOr(editor, system, string.Empty))`. **Vale para los dos modelos:** el corpus de F4 no está bloqueado (R-8), así que el piloto no cambia en él;
- **(e) la disposición:** `PushBackViewDispositions.Of(<miembros releídos>, system, catalog)`, recorrida por clave en ordinal:
  - `NoAddress` → `Invalid(ViewAddressPrefix + clave + ViewAddressSuffix)`, como hoy;
  - **piloto** (no admite borrar): cualquier otro que no sea `Redraw` → `Invalid(ViewUnavailablePrefix + clave + ViewUnavailableSuffix)`, **el texto y el orden de hoy**;
  - **completo:** `FrontalBOfOneWay` → `Invalid(FrontalBOfOneWayPrefix + clave + FrontalBOfOneWaySuffix)`; `ObsoleteLateral` → a `Erasures`; y, tras el recorrido, `AllObsolete` → `Invalid(AllViewsObsoleteReason)`;
- **(f) el nombre:**
  - piloto: el de **cada** sobre releído, como hoy;
  - completo: `composition.Name`; si está en blanco, el del sobre releído del **origen**, para todos los miembros (RPC:188);
- **(g)** los planes de miembro, **solo** de las claves `Redraw`, compuestos con `ComposeMemberEnvelope(RackEmbedDocument source, string name, string view, int section, string designJson)`. Es el mismo miembro, con el nombre como parámetro. Sigue siendo el único compositor del piloto (I-54) y su primer argumento sigue siendo `source`.

**El plan** gana:
- `public IReadOnlyList<string> Erasures { get; }`, ordinal, vacía en el piloto. Entra en `Equals`.
- en `PushBackPilotPostExpectation`, `public IReadOnlyList<string> ErasedKeys { get; }`.

Los constructores internos existentes se conservan (`Erasures` vacía) y se añade una sobrecarga.

**Los textos nuevos** (constantes públicas de `PushBackPilotUpdatePlanner`), a revisar:

| Constante | Texto propuesto |
|---|---|
| `BlockedPrefix` | «El diseño tiene un error que impide escribirlo: » |
| `FrontalBOfOneWayPrefix` / `…Suffix` | «La vista » / « es un frontal del lado B y el rack deja de tener lado B: usa RACKEDITAR.» |
| `AllViewsObsoleteReason` | «Todas las vistas del rack quedarían obsoletas: no se elimina el último vínculo del rack.» |

Si la revisión considera alguno una decisión material de producto, vuelve al Owner (como OB-08.4).

### D-PBT3-10. La corrida generalizada y `ComparePost` (PB-T3b)

**Dónde:** UR, en su sitio. **El API público existente no cambia:** `Precheck(DraftSession<PushBackPilotDraftState>)`, `Execute(DraftSession<PushBackPilotDraftState>, …)`, `ComparePost(plan, PushBackPilotIntentResult?)`, `DesiredNamesOf` y las constantes.

**Se añade:**

```csharp
public static PushBackPilotPrecheck Precheck(DraftSession<PushBackFullDraftState> draft);
public static DraftApplyOutcome<PushBackFullDraftState> Execute(
    DraftSession<PushBackFullDraftState> draft, PushBackPilotUpdateRequest request, IPushBackPilotUpdatePort port);
public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackFullIntentResult? postRead);
public const string EraseUnsupportedReason = "El dibujo no admite borrar vistas en esta actualización.";
public const string PostErasedStillPresentPrefix = "POST_ERASED_STILL_PRESENT: ";
```

**El núcleo genérico:**
- `private sealed class Execution<TState>` sustituye a `Execution`, con el mismo cuerpo;
- lo que depende del modelo entra por tres delegados que fijan las dos `Execute`:
  1. la entrada y el `Prepare` de su modelo;
  2. el texto del origen;
  3. la lectura de POST de su modelo, que da la base, la lectura y la escala del estado admitido;
- las dos `Execute` conservan el patrón de M-01: `var execution = new Execution<…>(…)` antes del `try`, y `catch (Exception ex) { return execution.OutcomeOfAnEscape(ex); }`.

**Los ayudantes privados** son libres dentro de UR y PP (§6.0).

**MUTATE (`Write`)**, con estos cambios y en este orden:
1. **La capacidad, lo primero:** con `Erasures` no vacía y un ámbito de MUTATE que no implementa `IPushBackPilotViewEraser` → `NotAppliedPreflight(EraseUnsupportedReason)`, antes de la relectura y de cualquier escritura. No depende del dibujo. Con el Plugin de PB-T3b, que todavía no la implementa, es lo que saldría si un plan trajera borrados; el piloto nunca los trae.
2. **El destino:** las claves mutables releídas = `Members` ∪ `Erasures`, y `PreparedBlockMatches` es cierto para todas.
3. **Supervivientes y capas:**
   - la superviviente de C-F4-05 se busca entre los miembros que **no** están en `Erasures`;
   - las capas bloqueadas se miran en `Members` ∪ `Erasures`: una vista a borrar en una capa bloqueada es `LOCKED_LAYER` y no se escribe nada. El clásico lo intentaría y fallaría «best effort»; el panel es más estricto.
4. **Las escrituras:** una `WriteUnit` por miembro, como hoy, y después una `EraseUnit` por clave de `Erasures`, en ordinal (el orden del clásico: redibuja y después borra, RPC:272-352). `_writeStarted = true` antes de cada una. El escape y el aborto se clasifican igual que con `WriteUnit`.
5. **`Seal`**, una vez.

**POST:** sin cambio. Los nombres deseados salen de los sobres salientes (`DesiredNamesOf`, que ya lee el nombre del sobre), así que el renombrado no necesita código nuevo en POST.

**`ComparePost` genérico:**
- `PostRereadMissingReason`, `PostNotAdmittedPrefix`, `PostKeysDifferReason` y `PostEnvelopeDiffersPrefix`, **como hoy**;
- **antes de** las claves: una clave de `ErasedKeys` que sigue entre las mutables → `PostErasedStillPresentPrefix + clave`;
- la escala, la del modelo: piloto `InitialState!.Inputs.Annotations.AnnotationScale`; completo `InitialState!.Editable.AnnotationScale`.

Con `ErasedKeys` vacía, el piloto da lo mismo que hoy.

**La interfaz nueva, `Pilot/IPushBackPilotViewEraser.cs`:**

```csharp
public interface IPushBackPilotViewEraser
{
    PushBackPilotUnitResult EraseUnit(string memberKey);
}
```

Va en un archivo propio, para que UP no cambie (R-6). Ni ese archivo ni ningún otro de `Pilot/` nombra la palabra `Transaction`, tampoco en comentarios (UpdateGuard:117-124; IntegrationGuard:81).

### D-PBT3-11. El borrado en el dibujo (PB-T3c; OB-04.1, R-PB-03)

**La fachada caller-owned**, `src/RackCad.Plugin/Views/RackViewBlockEraser.cs`:

```csharp
internal static class RackViewBlockEraser
{
    internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId);
}
```

**El cuerpo,** en este orden: el de `EraseViewBlocks` (RCS:233-266) para **una** definición, **sin** bloqueo, transacción propia, confirmación ni purga:
1. `transaction.GetObject(definitionId, OpenMode.ForRead) as BlockTableRecord`; nula o `IsLayout` → `InvalidOperationException` (el clásico la salta en silencio, RCS:240-243; el panel aborta: todo o nada);
2. las definiciones anidadas: el `BlockTableRecord` de cada `BlockReference` hija;
3. cada referencia de `GetBlockReferenceIds(directOnly: true, forceValidity: true)`, abierta `ForWrite`, se borra (también las anidadas en otras definiciones: BM:54-60);
4. `UpgradeOpen()` y `Erase()` de la definición;
5. devuelve las anidadas, para la purga de POST.

**El archivo no contiene la cadena `PushBack`,** para no entrar en el censo de CT-PBT1B-15 (14 archivos del Plugin con `PushBack`).

**El escritor (WR):**
- `RackPilotUpdateScope.PrepareUnits` también resuelve cada clave de `plan.Erasures` en `_definitions`, en un diccionario privado nuevo `_erasures` (clave → `ObjectId`). Una clave que falta es `Failed("La definición " + clave + " ya no está en el dibujo.")`, el texto de hoy. No prepara ningún redibujo para ellas.
- `BeginMutation` pasa `_erasures` al ámbito de MUTATE.
- **`RackPilotMutationScope : IPushBackPilotMutationScope, IPushBackPilotViewEraser`.** `EraseUnit(memberKey)` hace:
  1. `EnsureOwned()`;
  2. la clave en `_erasures`, o `InvalidOperationException`;
  3. `_units++; FailUnitIfRequested(_units);`, la inyección de Debug de las unidades, sin un tercer `[Conditional("DEBUG")]`;
  4. `RackViewBlockEraser.EraseInTransaction(_transaction, id)`, con las anidadas añadidas a `_staleDefinitions`;
  5. `new PushBackPilotUnitResult(Array.Empty<string>())`.
- `Reread` calcula `matches` sobre `_prepared` **y** `_erasures`.
- POST no cambia: la purga ya es la unión de `_staleDefinitions`; los nombres y el renombrado solo son de las claves preparadas.
- El comentario de cabecera de WR deja de decir «It never erases a view» y describe el borrado caller-owned.

**Lo que sigue en verde por su letra** (R-6, R-7): las prohibiciones del escritor (`GetBlockReferenceIds`, `Erase(`, `EraseViewBlocks`, `ForWrite` y `UpgradeOpen` van en la fachada), la estructura (un `StartTransaction`, un `Commit` y una purga), G-1, G-2, G-7 y G-9 con sus cuentas, y la relectura caller-owned. **La premisa «el escritor nunca borra» sí cambia:** se declara (§8.2, D-2) y la fijan las guardas nuevas de PB-T3c.

### D-PBT3-12. D2 respetada

| Condición de D2 | En PB-T3 |
|---|---|
| 1 y 2 (los dos tipos; nada más compartido) | `PushBackFullDraftStateComparer.Copy` delega en el copiador de PB-T1b. CT-PBT3A-05 vuelve a medir el conjunto de objetos compartidos con el recorrido de CT-PBT1B-07 |
| 3, 4, 6 y 7, para las operaciones nuevas | CT-PBT3A-11: materializar, componer, `KeepOn`, clasificar y la disposición no cambian las huellas profundas de las dos referencias de sus argumentos (la huella de CT-PBT1B-16) |
| **8** (A-2: el ajuste de `Name`) | El escritor del panel no ajusta el `Name` de un sistema que comparte un estado del borrador. SR construye el sistema de cada miembro **desde texto** y le pone el nombre de su sobre (SR:46-55): es un objeto nuevo. CT-PBT3B-12 lo prueba con huellas tras un plan `Ready` y una corrida `Applied`, y CT-PBT1B-15 sigue en verde (PB-T3 no añade escritores de `.Name =` en el Plugin) |
| **10** (A-2: las guardas) | G-L1 se amplía, con una clase propia y sus autopruebas, a la carpeta `Draft/` (CT-PBT3A-13), a PP, UR y la interfaz nueva (CT-PBT3B-13) y a WR y la fachada (CT-PBT3C-05). Ninguna quita archivos ni fichas |
| **El límite** | La parada PBT3x-C17 en los tres contratos (§6.0) |

### D-PBT3-13. AUTH-13, fijado con pruebas (decisión del Owner del 2026-11-19)

- **CT-PBT3A-04 (lectura):** un sobre de `pb-01-base` con un campo desconocido en `PushBack.Structure.Modules[0]` y otro con un campo desconocido en `PushBack.SideB` (sobre `pb-20-ab`). Los dos lectores (el del piloto y el completo) dan `NotAdmitted` con «No editable aquí: ilegible o de versión futura» y la pista de RACKEDITAR. Como contexto, las tres capas de arriba (envoltorio, carga útil y sobre) dan lo mismo.
- **CT-PBT3B-10 (escritura):** con esa relectura, el `Prepare` completo da `NotAdmitted`, y la corrida, `NotAppliedAdmission`, sin ninguna llamada a `PrepareUnits`, `BeginMutation`, `WriteUnit` ni `EraseUnit` en el doble.
- **Lo que no se hace:** abrir AUTH-13 (D-1 del Owner no lo pide) ni conservar lo anidado (opción C, no elegida). La pérdida de RACKEDITAR («Actualizar» y «Guardar en biblioteca») queda como limitación conocida anterior a F4B, sin tocar el clásico (D-16, INV-19).

### D-PBT3-14. Lo que PB-T3 no hace

- **Crear vistas** (H-PB-16; fila 1 del §6.4; OB-04.2).
- **Cambiar RACKEDITAR o el clásico:** RPC, RCS (`EraseViewBlocks` incluido), la ventana, WX, T1B (D-16, INV-19, OB-RE.1).
- **Tocar la ventana:** los modos, «Aplicar al borrador», BOM y biblioteca son de PB-T2 y PB-T4.
- **Generalizar el anfitrión o el puerto del panel:** WHC, `IWorkspacePilotEditPort`, el adaptador de campos y la UI del panel son de PB-T4.
- **Conectar el modelo completo al panel:** la admisión A/B (OB-03.2), «Conservar mi borrador» completo (OB-05.5), el clasificador presentado (OB-05.3, OB-05.4). Todo es de PB-T4.
- **Abrir AUTH-13 o conservar ExtensionData anidada.**
- **Cambiar `DormantTailOutOfPilotReason`** ni el trato de `pb-21-cola` (OB-03.1, D-PBT1-10).

---

## 4. Las guardas: T1B, I-55, F4-T4 y PB-T1

| Guarda | ¿Sigue en verde? | Cómo |
|---|---|---|
| **T1B** (55 UI y 10 Core, con T-SG y T-PG) | Sí | PB-T3 no toca la ventana, WX, RPC ni el Plugin clásico |
| **G-10** (`WorkspacePilotUpdateIntegrationGuardTests`, una sola `.RequestUpdate(` por archivo) | Sí | Ningún archivo de PB-T3 llama a `RequestUpdate` |
| **I-55** (`I55DeferredProductFixCharacterizationTests`, G15 y las de proyección) | Sí | No se tocan sus archivos |
| **I-54 G4B** (el censo de los que componen) | Sí | `ComposeMemberEnvelope(source, …)` sigue siendo el único del piloto; nada nuevo compone |
| **I-58** (la matriz de AUTH-13) | Sí | El lector no cambia |
| **F4-T4: G-1, G-2, G-6, G-7, G-9, el censo de `Task`, la pureza del runner y del adaptador** | Sí | PB-T3a no toca F4. PB-T3b cambia solo PP y UR (sin términos de escritura) y añade la interfaz. PB-T3c llama a una fachada que no nombra el piloto ni ningún término de G-9 (R-6) |
| **F4-T4: pureza de UP, UR y SR, y `Pilot/` sin `Transaction`** | Sí, con la regla de §6.0 | Ningún texto nuevo de `Pilot/` nombra esas palabras, tampoco en comentarios |
| **F4-T4: `From(DraftSession<PushBackPilotDraftState>)` y una sola construcción** (PrepareGuard:81-98) | Sí | Las dos fábricas son de tipos distintos; la del piloto no cambia |
| **F4-T4: `ComposeForWrite` solo en el plan, tras la comparación, una vez** (PrepareGuard:154-190) | Sí | La llamada sigue en el `Build` del piloto |
| **F4-T4: el orden de `Prepare`** (PrepareGuard:194-227) | Sí | El `Prepare` del piloto no cambia y va primero en el archivo |
| **F4-T4: el escritor** (UpdateGuard:182-470) | Sí, por su letra | Ver D-PBT3-11. La premisa cambia y se declara (D-2) |
| **F4-T4: S-1 a S-6** (R-6) | **No** | **D-1 (§8.2):** se sustituyen con registro declarado, cada uno con su equivalente, en PB-T3b |
| **El comportamiento del piloto en F4** (`WorkspacePilotPrepareTests` 67, `WorkspacePilotUpdateRunTests` 49, `WorkspaceDraftPilotMaterializationTests` 41, `WorkspacePilotUpdateHostTests` 35, `WorkspacePilotUpdateRequestTests` 44, `WorkspacePilotReconciliationHostTests` 28, `WorkspaceDraftPilotIntentReadTests` 63, `WorkspaceDraftPilotStateTests` 27) | Sí | El piloto conserva sus textos, su orden, sus llamadas al puerto y su `ComparePost`. La barrera no cambia el corpus (R-8) |
| **PB-T1:** CT-PBT1B-14 y 15 (D2), CT-PBT1C-03 y 04, `PushBackStateNet`, la paridad CT-PBT1B-01, el censo del piloto (HT1-21) | Sí | PB-T3 no cambia PES, PESC, PESO, GATE, ES, CS, RMES ni la ventana. No añade campos a los tipos del censo. El archivo nuevo del Plugin no contiene `PushBack` |
| `NamespaceFolderGuardTests`, el censo de ventanas, RAM Elements, el corpus de WLC | Sí | Un `namespace` por archivo igual a su carpeta; ninguna ventana; ningún `pb-*.json` nuevo en `Fixtures/I64` |

**Si cualquier otra prueba existente falla** en el RED o en el GREEN, es la parada C02 y vuelve al Owner (OB-T1B.3). No hay más sustituciones que las seis de D-1.

---

## 5. La red de pruebas

### 5.0 Reglas y recetas comunes

**Las reglas** (las de PB-T1, copiadas en cada contrato; no se heredan por analogía, [P] §7.2):
- `[Fact]` que acaban en **un** `Assert.Equal` de las líneas unidas por `\n`, con «R», cultura invariante, GUID fijo y orden ordinal;
- la regla «EXCEPCION»: cada llamada al API nuevo pasa por un ayudante que convierte una excepción en `EXCEPCION <Tipo>: <Mensaje>`; las líneas esperadas no contienen ninguna;
- las pruebas de UI, en `EditorDiscardPromptCollection`, con `Ventana()`, la carga dentro del `try` y `Cerrar` en `finally`, el aviso de descarte sustituido y sin teclado ni foco real;
- ningún `MessageBox` real, BOM, «Guardar en biblioteca», configurador real ni «Insertar»;
- ningún fixture nuevo en `Fixtures/I64`: los diseños y sobres nuevos se fabrican en código;
- **los ayudantes de prueba existentes se usan sin editarlos:** `IntentKit` e `IntentItem` (`WorkspaceDraftPilotIntentStaleBaseTests.cs:15-21`), `PilotKit` (con `JsonDiff`), `DraftKit` y `PilotUpdateScopeDouble`. Lo que haga falta de más va en un ayudante nuevo de la propia clase o en un archivo nuevo de su subtarea.

**Las recetas:**
- **Los sobres del corpus:** los 17 de `Fixtures/I64`, leídos sin añadir nada, editados con `EditInner` (copiado de S0, `:108-118`).
- **X-01 y F-01 (Core):** las de [PKT1] §5.0, con `PushBackEditableStateSeat.Pass` en lugar de `Pasada`. Para leerlas como sobre, la prueba toma un sobre del corpus deserializado con `RackEmbedStore`, sustituye su `Design` por el proyecto serializado del diseño y lo vuelve a serializar. El Plugin (`BuildPushBackPayload`) no es accesible desde Core. No es un `new RackEmbedDocument` ni una llamada a `RackEmbedComposer`, y las guardas de I-54 miran `src/`, no `tests/`.
- **Los borradores:** `DraftRegistry.Begin` con `PushBackFullDraftStateComparer.Instance` y `PushBackFullDraftStateComparer.Copy`, como el anfitrión hace con el piloto (WHC:347-352), y una lectura de `PushBackFullDraftReader.Read`.
- **El escaneo:** `IntentKit`, como las pruebas del lector del piloto.
- **El doble con borrado** (PB-T3b): una clase nueva, `PilotEraseScopeDouble`, en `tests/RackCad.Tests/Workspace/PushBackFullWriteSupport.cs`. Es una copia de `PilotUpdateScopeDouble` (no se edita la original) cuyo ámbito de MUTATE implementa `IPushBackPilotViewEraser`. Registra `EraseUnit:<clave>` y lo aplica en el sellado, como las escrituras. **El nombre de la clase no empieza por `Workspace`** (§6.2, filtros).

### 5.1 Lo que sigue en verde sin tocarlo (las tres subtareas)

T1B (55 y 10); PB-T0 (16 y 3); PB-TC y PB-TC2 (14 en UI); PB-T1a (14); PB-T1b y PB-T1c; `RackCad.Tests.Workspace` entero (3 271 en Core y 547 en UI, salvo las seis sustituciones de PB-T3b); I-58; I-54 G4B; `NamespaceFolderGuardTests`; Cantilever (67). En PB-T3b y PB-T3c, también las subtareas anteriores de PB-T3.

### 5.2 PB-T3a: el modelo (lista cerrada CT-PBT3A-01 a 14)

**Régimen** (el de PB-T1b): **el RED** es el esqueleto inerte de §6.1 y las pruebas nuevas, que fallan **por aserción**, salvo las tres guardas (CT-12 a 14), que pasan en el RED y discriminan por sus autopruebas. **El GREEN** solo toca `src/`, y solo los archivos nuevos.

**Las clases:**
- en `tests/RackCad.Tests/` (`namespace RackCad.Tests`): `PushBackFullDraftLoadTests` (02 a 05), `PushBackFullDraftCompositionTests` (06, 07 y 11), `PushBackFullDraftViewTests` (08), `PushBackFullDraftClassifierTests` (09 y 10) y `PushBackDraftWriteGuardTests` (12 a 14);
- en `tests/RackCad.UI.Tests/`: `PushBackFullDraftWindowParityTests` (01).

| CT | Qué fija | Oráculo | Falla en el RED porque |
|---|---|---|---|
| **01** (UI) | **El asiento iguala a la ventana.** Para cada sobre del corpus salvo `pb-21-cola` (16): `Materialize` admite, y su `Editable` es igual, con el comparador de PB-T1b, al `EditableStateForTest` de una ventana tras `LoadExisting(<diseño>, <id>, <nombre>, <proyecto>)`; y el diseño persistido de `Pass` sobre una copia es igual a `Persist(LastComputation.Design)`. La línea `<diseño> estado-igual=<b> diseno-igual=<b>`. Para `pb-21-cola`: `pb-21-cola OutOfPilot motivo=<DormantTailOutOfPilotReason>` | La ventana de hoy, a través de la costura de PB-T1c | el esqueleto no materializa |
| **02** | **El materializador:** en blanco, `null`, otro tipo, sin diseño, ilegible y catálogo nulo, cada uno con su `Failed` y su texto; `pb-21-cola` `OutOfPilot`; `pb-20-ab` `Admitted` (la diferencia con el piloto: `PushBackPilotMaterializer.Materialize` da `OutOfPilot(CompositeOutOfPilotReason)`); un diseño que hace lanzar al resolver (c-13), `Failed` con el prefijo. Una línea por caso: `<caso> completo=<Kind>[ motivo=<texto>] piloto=<Kind>[ motivo=<texto>]` | El materializador del piloto y sus constantes | el esqueleto da `Failed` siempre |
| **03** | **El lector completo es el del piloto salvo el compuesto.** Para un escaneo por regla de admisión (las 14 de IR, construidos con `IntentKit` como en `WorkspaceDraftPilotIntentReadTests`) y para `pb-20-ab`: el diagnóstico, el detalle, la pista, la clave del origen y la base (entrada a entrada) del lector completo frente a los del piloto. La línea `<caso> iguales=<b>` y, en el compuesto, `pb-20-ab piloto=NotAdmitted completo=Admitted base-igual=True` | `PushBackPilotIntentReader.Read` sobre el mismo escaneo | el esqueleto no admite |
| **04** | **AUTH-13** (D-PBT3-13) | Las constantes del lector; la matriz de I-58 | el esqueleto da otro diagnóstico |
| **05** | **El copiador y el comparador:** `Copy(x)` igual a x y distinto objeto; cambiar `Origin` o una ranura de `Editable` (el `Mutator` del piloto, copiado) da distintos; dos materializaciones del mismo texto, iguales; el conjunto de objetos compartidos entre `Copy(x)` y x son los no nulos de las dos referencias de D2 y lo que alcanzan (el recorrido ampliado de CT-PBT1B-07, copiado) | D-PBT3-02; PB-T1b | `Equals` es `ReferenceEquals` |
| **06** | **`ForWrite`:** escala `NaN`, `+∞`, 0 y −1 → `InvalidScaleReason`; un estado inicial cambiado tras materializarse (`PalletTolerance` + 1) → `BaseChangedReason`; un `Origin` ilegible → su motivo; con F-01 editado (una longitud de módulo, `SetGap(30)` en `pb-20-ab`, el nombre «PB-NUEVO»): `Composed`, con el diseño de `BuildDesign(Editor, Inputs)` igual al de `Pass` sobre una copia de `current`, `Name` igual al del borrador, y `current` sin cambios (comparador y huella) | D-PBT3-05; PB-T1b | el esqueleto rechaza siempre |
| **07** | **`KeepOn`:** el resultado tiene el `Editable` de `current` (comparador) y el `Origin` de `newInitial`; ningún argumento cambia; el resultado no comparte nada mutable con ellos fuera de las referencias de D2; `DraftSession.KeepMineOnCurrentVersion(read, PushBackFullComposition.KeepOn, confirmed: true)` sobre un borrador en Dirty (el estado actual cambiado con `SetCurrentDraftState`) da `Reloaded`, `IsDirty` cierto y la base nueva | D-PBT3-05; DS:469-486 | el esqueleto devuelve `current` |
| **08** | **La disposición:** los casos de CT-PB0-10 (`PushBackFullEditorCharacterizationLateralCutTests`), con un sobre lateral por cada corte de ANTES (su sección, el `PostIndex`) y el sistema de DESPUÉS: para cada momento, `ObsoleteLaterals` = los `PostIndex` que esa prueba da por obsoletos, como claves; `pb-20-ab` con B quitado y un frontal B → `FrontalBOfOneWay`; un sobre `lateral` con sección −1 → `NoAddress`; un rack cuyo único miembro es un lateral obsoleto → `AllObsolete=True`; planta y frontal de A siempre `Redraw` | Las líneas de CT-PB0-10 y RVA:94-116 | el esqueleto da todo `Redraw` |
| **09** | **El clasificador, por campo:** con base B0 = `pb-01-base`: un cambio externo de `PalletTolerance` (c), un cambio del usuario de `BeamDepth` (d), el mismo campo `PostPeralte` cambiado por los dos con el mismo valor y con valores distintos, el nombre cambiado fuera y una `CustomProperties` cambiada fuera. Una línea por entrada: `<nivel> <ruta> grupo=<g> clase=<c> estructural=<b> b=<v> c=<v> d=<v>` | D-PBT3-07 (las reglas, aplicadas a mano a estos casos) | el esqueleto no da entradas |
| **10** | **El clasificador, estructura y vistas:** «+ frente» del usuario, una longitud de módulo cambiada fuera, el lado B declarado con un frente por el usuario, y «- frente» con un lateral del último poste en `currentMembers`. Las entradas de `Structural` con su grupo; `ViewsToErase`, `ViewsRejected` y `AllViewsObsolete` | D-PBT3-07 y CT-08 | ídem |
| **11** | **D2, condiciones 3, 4, 6 y 7 para las operaciones nuevas:** sobre el estado rico de CT-PBT1B-06 y X-01, la huella profunda (la de CT-PBT1B-16, copiada) de las dos referencias de cada argumento antes y después de `Copy`, `ForWrite`, `KeepOn`, `Classify` y `Of`, y de `Equals` en los dos sentidos. La línea `<operacion> huellas-iguales=True` | D-PBT1-15; CT-PBT1B-16 | el esqueleto no carga los estados |
| **12** | **Sin conectar** (OB-05.2, OB-05.5; ExpectRed false): ningún archivo de `src/` fuera de `Systems/PushBack/Draft/` y de los dos que PB-T3b generaliza (PP y UR, admitidos desde ahora para que PB-T3b no rompa esta guarda) nombra `PushBackFullDraftState`, `PushBackFullDraftReader`, `PushBackFullComposition`, `PushBackConflictClassifier` ni `PushBackViewDispositions`. Autopruebas: inyectar cada nombre en WHC y en `WorkspacePilotSupport.cs` salta | — | **no falla en el RED** |
| **13** | **G-L1 ampliada a `Draft/`** (D2, condición 10; ExpectRed false): las fichas y las reglas de CT-PBT1B-14 (con la regla 1b para los alias, la corrección I01 de PB-T1b), sobre los archivos de `Draft/`, con `reglas-sin-coincidencias=True` y las autopruebas (a), (b), (c), (e), (f1), (h1), (i) y (j) inyectadas en `PushBackFullComposition.cs` | D-PBT1-15 | **no falla en el RED** |
| **14** | **La pureza de `Draft/`** (ExpectRed false): ningún archivo lleva directiva `#nullable` (como PES, PESC, PESO y GATE); un `namespace` de bloque `RackCad.Application.Systems.PushBack.Draft`; sin `Autodesk`, `RackCad.Plugin`, `RackCad.UI`, `System.Windows`, `Transaction`, `Database`, `async`, `await`, `Task`, `File.`, `System.IO` ni `RackEmbedComposer`, y sin `new RackEmbedDocument`. Autopruebas: cada término inyectado salta | D-PBT3-08 | **no falla en el RED** |

**La forma de CT-04, literal:**
```
modulo-anidado piloto=[No editable aquí: ilegible o de versión futura] completo=[No editable aquí: ilegible o de versión futura] pista=[Usa RACKEDITAR para editarlo con el editor clásico.]
ladoB-anidado piloto=[…igual…] completo=[…igual…] pista=[…igual…]
envoltorio piloto=[…] completo=[…] pista=[…]
carga-util piloto=[…] completo=[…] pista=[…]
sobre piloto=[…] completo=[…] pista=[…]
```
Cada `[…]` es el texto real. Las cinco líneas llevan el mismo diagnóstico y la misma pista.

### 5.3 PB-T3b: la escritura (lista cerrada CT-PBT3B-01 a 14)

**Régimen:**
- **el RED** tiene tres partes:
  - el esqueleto inerte del API nuevo **en los archivos existentes** (§6.2);
  - la interfaz nueva;
  - las pruebas nuevas y el `Skip` de los seis métodos de D-1, con su motivo literal.
  
  **El piloto no cambia en el RED.** Las pruebas nuevas fallan por aserción, salvo CT-13 (guardas), que pasa en el RED.
- **el GREEN** solo toca PP y UR.

**Las clases:**
- en `tests/RackCad.Tests/`: `PushBackFullWritePrepareTests` (01 a 06, 10 y 11), `PushBackFullWriteRunTests` (07 a 09 y 12), `PushBackWritePathGuardTests` (13) y `PushBackPilotUpdateEquivalenceGuardTests` (14, con seis `[Fact]`);
- el doble, en `tests/RackCad.Tests/Workspace/PushBackFullWriteSupport.cs`.

| CT | Qué fija | Oráculo | Falla en el RED porque |
|---|---|---|---|
| **01** | **Campos:** un borrador completo de `pb-01-base` con `PalletTolerance`, `BeamDepth` y el nombre cambiados → `Ready`. Por miembro: el sobre saliente tiene el `Id`, `Kind`, `View`, `Section`, `CustomProperties` y cajón del sobre **releído**, el nombre del borrador y el diseño de la composición. El proyecto y la carga útil, con la versión y el cajón del proyecto releído; `ExpectedBase` es la base del borrador; `Erasures` vacía; dos `Prepare` iguales dan planes iguales | PP:421-438; R-5; D-PBT3-05 | el esqueleto da `Invalid` |
| **02** | **Frentes:** «+ frente» → todos los miembros redibujados, ninguna clave nueva (las claves del plan ⊆ las releídas: **no se crean vistas**); «- frente» con el lateral del último poste → `Erasures` = esa clave y lo demás redibujado; un rack con ese lateral como único miembro → `Invalid(AllViewsObsoleteReason)` | CT-PB0-10; RPC:320-356; M-2b | ídem |
| **03** | **Módulos:** F-01 con una longitud de módulo de A y otra de B cambiadas → `Ready`; el diseño saliente lleva los dos módulos; ninguna vista a borrar | PB-T1b | ídem |
| **04** | **Lado B:** (i) `pb-01-base` con B declarado y un frente activo → el diseño compuesto; el frontal de A se redibuja y su nombre deseado lleva el lado; (ii) `pb-20-ab` con B quitado y un frontal B entre los miembros → `Invalid(FrontalBOfOneWayPrefix + clave + FrontalBOfOneWaySuffix)`; (iii) `pb-20-ab` con B quitado y un lateral que solo existía por una ranura de B → `Erasures` = esa clave (H-PB-17 corregido); (iv) `Composite.AbsentSlotsB` del diseño saliente de (iii), igual al de `Pass` sobre el mismo estado (M-i: lo reproduce, porque es el mismo ensamblador) | D-PBT3-06; BM:62-66 | ídem |
| **05** | **Renombrar:** el nombre «PB-NUEVO» en los sobres de todos los miembros; `DesiredNamesOf` da los nombres del clásico (`RackViewBaseName.LinkedPlanta`, `LinkedLateral` y `LinkedPushBackCut` con el compuesto); un nombre en blanco → el del sobre releído del origen, en todos. **El piloto** con miembros de nombres distintos conserva el de cada uno (como hoy) | RPC:186-190, :278-345 | ídem |
| **06** | **La barrera:** los cuatro bloqueantes de CT-PBT1B-12 (hueco negativo, separador sin hueco, estructura manual de 1 y celda que no cabe), como borrador completo → `Invalid(BlockedPrefix + <el texto de StatusOr>)`, literal. Los diseños del corpus que el piloto admite (los 17 de `Fixtures/I64` menos `pb-20-ab` y `pb-21-cola`, comprobado con `PushBackPilotMaterializer.Materialize` en la propia línea), como borrador **del piloto** con la escala a 2,0 → `Ready`, igual que sin barrera | CT-PBT1B-12; R-8 | ídem (y el piloto no tiene barrera en el RED) |
| **07** | **Una corrida aplicada con borrado:** con el doble con borrado, la secuencia exacta de llamadas `Begin, ReadForPrepare, PrepareUnits, BeginMutation, Reread, WriteUnit:<k>…, EraseUnit:<k>…, Seal, Dispose:mutation, Post, Dispose:scope`; **un** `BeginMutation`, **un** `Seal` y los `EraseUnit` entre los dos; el resultado `Applied`; la base nueva del borrador sin la clave borrada | D-PBT3-10; UR:250-289 | `Execute` completo da `NotAppliedFailure` |
| **08** | **Los fallos de MUTATE con borrado:** un ámbito sin `IPushBackPilotViewEraser` (el doble original, `PilotUpdateScopeDouble`) → `NotAppliedPreflight(EraseUnsupportedReason)` sin `Reread` ni ninguna `WriteUnit`; `EraseUnit` que lanza antes del sellado → `NotAppliedFailure` con `WriteFailurePrefix` y el aborto (sin `Seal`); la vista a borrar en una capa bloqueada → `LOCKED_LAYER: <clave>`; la vista a borrar cambiada entre PREPARE y MUTATE → `NotAppliedConflict` | UR:391-456 | ídem |
| **09** | **`ComparePost` genérico:** la clave borrada sigue → `POST_ERASED_STILL_PRESENT: <clave>`; falta una redibujada → `POST_KEYS_DIFFER…`; un sobre distinto → `POST_ENVELOPE_DIFFERS: <clave>`; la escala del modelo completo distinta → `POST_SCALE_DIFFERS…`; todo igual → `null`. **El piloto**, con un plan sin borrados, da lo mismo que hoy en los mismos casos | UR:163-183 | la sobrecarga completa da `PostRereadMissingReason` |
| **10** | **AUTH-13 en la escritura** (D-PBT3-13) | El doble: ninguna llamada de escritura | ídem |
| **11** | **D-PB-05, pasos 5 a 7, al escribir:** un borrador completo rebasado con `KeepOn` sobre una base nueva con otra `CustomProperties`, otra versión menor del sobre y otro nombre → los sobres salientes llevan la `CustomProperties`, la versión y el `Id` del releído, y el diseño y el nombre del borrador. **El nivel superior, por construcción:** con una relectura admitida **fabricada** (constructor interno de `PushBackFullIntentResult`) cuyos sobre, proyecto y carga útil traen un campo desconocido, y un borrador cuya base es esa misma relectura, los tres campos se conservan en el texto saliente. La línea lleva el sufijo `por-construccion-no-ejercido-por-AUTH-13` | R-5; D-PBT3-05 | el esqueleto da `Invalid` |
| **12** | **D2, condición 8:** las huellas profundas de las referencias compartidas de los estados inicial y actual del borrador, iguales antes y después de un `Prepare` `Ready` y de una corrida `Applied` | D-PBT1-15; A-2 | `Execute` completo no corre |
| **13** | **G-L1 ampliada y «sin conectar»** (ExpectRed false), dos `[Fact]`: (i) las reglas de CT-PBT1B-14 sobre PP, UR e `IPushBackPilotViewEraser.cs`, con sus autopruebas; (ii) ningún archivo de `src/` fuera de PP, UR y `Draft/` nombra `PushBackFullPrepareInput` ni llama a `PushBackPilotUpdateRun.Execute` o `Precheck` con un `DraftSession<PushBackFullDraftState>`; en WHC, `Find<PushBackPilotDraftState>` sigue siendo el único `Find<`; autopruebas inyectadas en WHC | D-PBT1-15; OB-05.5 | **no falla en el RED** |
| **14** | **Los equivalentes de S-1 a S-6** (D-1), un `[Fact]` cada uno | El texto del GREEN | el texto del RED no tiene la forma nueva |

**Los equivalentes de CT-14**, cada uno con lo que cubría el retirado:

| Equivalente | Sustituye a | Qué fija |
|---|---|---|
| E-1 | S-1 | El plan compone **una vez**, en `ComposeMemberEnvelope(RackEmbedDocument source, string name, string view, int section, string designJson)`, con `RackEmbedComposer.Compose(source, RackEmbedDocument.KindPushBack, source.Id, name, view, section, designJson)`, sin `null`. El `name` de cada llamada sale de una sola expresión de `BuildCore`, la de D-PBT3-09 (f) |
| E-2 | S-2 | Las autopruebas de E-1 sobre el texto real: otra llamada, `Compose(null,`, `RackEmbedDocument? source`, otro nombre de miembro y `source.Id, "Otro",` hacen fallar E-1 |
| E-3 | S-3 | Los miembros salen solo de los mutables releídos en ordinal; la base esperada es la del borrador; la disponibilidad se evalúa con `PushBackViewDispositions.Of` sobre el sistema saliente; **`Erasures` solo se llena si se admite borrar y solo con `ObsoleteLateral`**; el piloto conserva `ViewUnavailablePrefix`; la escala del plan es la del estado actual del modelo |
| E-4 | S-4 | Las mismas aserciones de UpdateGuard:126-141 sobre `Execution<TState>`: sin `using`; los dos ámbitos en `finally` propios con `try`/`catch (Exception ex)`; un `mutation.Dispose()` y un `scope.Dispose()`; POST solo tras liberar MUTATE (`private DraftApplyOutcome<TState> Post(IPushBackPilotUpdateScope scope)`) |
| E-5 | S-5 | Las mismas reglas de `RunEscapeViolations` sobre **las dos** `Execute` y sobre `OutcomeOfAnEscape` de `Execution<TState>`: el estado de la corrida antes del `try`; el `catch` que clasifica por ese estado; el sellado, después la escritura y después el fallo simple; nunca un `Applied` |
| E-6 | S-6 | Las autopruebas de E-5 sobre el texto real (las tres de IntegrationGuard:907-909, adaptadas al tipo genérico) |

**El motivo literal del `Skip`:** «I-64 F4B PB-T3b: sustituida por <E-n> de PushBackPilotUpdateEquivalenceGuardTests (registro declarado superseding-characterization.md, decisión del Owner D-1).»

### 5.4 PB-T3c: el borrado en el dibujo (lista cerrada CT-PBT3C-01 a 05)

**Régimen:**
- **el RED** son las pruebas nuevas (guardas de texto de Core), que fallan por aserción porque el texto no existe todavía, salvo CT-05, que pasa en el RED;
- **el GREEN** solo toca WR y el archivo nuevo de la fachada.

**Las clases:** `PushBackViewEraseGuardTests` (01 a 04) y `PushBackWritePathPluginGuardTests` (05), en `tests/RackCad.Tests/`. Todas son de Core y leen el Plugin como texto, como las de F4.

| CT | Qué fija | Falla en el RED porque |
|---|---|---|
| **01** | **La fachada:** un archivo `src/RackCad.Plugin/Views/RackViewBlockEraser.cs` con `internal static class RackViewBlockEraser` y un solo método, `internal static IReadOnlyCollection<ObjectId> EraseInTransaction(Transaction transaction, ObjectId definitionId)`. En su orden: `GetObject(definitionId, OpenMode.ForRead)`, la comprobación de `IsLayout` que lanza, la captura de las anidadas, `GetBlockReferenceIds(directOnly: true, forceValidity: true)`, el `Erase()` de cada referencia, `UpgradeOpen()` y el `Erase()` de la definición. Sin `LockDocument`, `StartTransaction`, `StartOpenCloseTransaction`, `.Commit(`, `Purge`, `Regen`, `SyncName` ni la cadena `PushBack`. Autopruebas de cada prohibición | el archivo no existe |
| **02** | **`EraseUnit` en el escritor:** `RackPilotMutationScope` implementa `IPushBackPilotViewEraser`; el cuerpo de `EraseUnit`, en orden: `EnsureOwned();`, la búsqueda en `_erasures` que lanza, `_units++;`, `FailUnitIfRequested(_units);` y `RackViewBlockEraser.EraseInTransaction(_transaction,`, y las anidadas a `_staleDefinitions`; sin `SyncName`, purga, regeneración ni `Commit` | el método no existe |
| **03** | **PREPARE y la relectura:** `PrepareUnits` recorre `plan.Erasures`, resuelve cada clave en `_definitions` y devuelve `Failed` con el texto de hoy si falta; ningún `PrepareRedraw` para ellas; `Reread` calcula `matches` sobre `_prepared` y `_erasures` | ídem |
| **04** | **El censo de la primitiva:** en todo `src/`, `RackViewBlockEraser.EraseInTransaction(` aparece **exactamente una vez**, dentro de `EraseUnit` de WR. Autopruebas: una segunda llamada en WR, una en RPC y una en un archivo nuevo saltan | 0 llamadas en el RED |
| **05** | **G-L1 ampliada a WR y a la fachada** (ExpectRed false): las fichas de D2 no aparecen, y las reglas de CT-PBT1B-14 dan 0 coincidencias; autopruebas | **no falla en el RED** |

### 5.5 Lo que se prueba sin AutoCAD y lo que queda para el Smoke-PB (D-PB-09)

**Sin AutoCAD, en la CI de cada subtarea:** todo lo de §5.2 a §5.4, es decir:
- el modelo, el asiento frente a la ventana, el lector, AUTH-13 y la composición;
- la disposición y el clasificador;
- el plan y la corrida con dobles, la barrera, el borrado planificado, el renombrado y `ComparePost`;
- las huellas de D2;
- las guardas de texto del Plugin.

**Para el Smoke-PB de PB-T5,** y solo cuando PB-T4 haya conectado el modelo completo al panel (en PB-T3 nada de esto se alcanza desde el panel):
- el borrado real de un lateral obsoleto, con sus referencias y con una referencia anidada, y la purga de sus anidadas;
- **un solo `U`** por «Actualizar» con borrado, con renombrado y con la conversión A/B;
- el renombrado con colisión: la definición no cambia de nombre y el aviso `NameDiffersPrefix`;
- `LOCKED_LAYER` sobre una vista a borrar;
- el rechazo del frontal B y el del último vínculo, con sus textos;
- la inyección de Debug de una unidad de borrado (`RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT` cuenta también los `EraseUnit`) y del sellado, con su aborto;
- el tiempo de un «Actualizar» grande (R-PB-06; M15 de PB-T1b);
- los racks A/B fabricados, rotulados CONSTRUCTED REPRESENTATIVE TEST CASES (OB-09.1).

### 5.6 Trazabilidad

| Obligación | CT |
|---|---|
| «field changes» | CT-PBT3A-06; CT-PBT3B-01 |
| «modules/fronts» | CT-PBT3B-02 y 03; CT-PBT3A-08 |
| «side B» | CT-PBT3A-02 y 03 (admisión en el modelo completo); CT-PBT3B-04 |
| «rename» | CT-PBT3B-05 |
| «deleting obsolete views» | CT-PBT3A-08; CT-PBT3B-02, 04, 07 a 09; CT-PBT3C-01 a 04 |
| «blocking authored write on diagnostics/errors» | CT-PBT3B-06 |
| «view creation only if PB-T0 subsequently authorizes it» | No se crea: CT-PBT3B-02 (claves del plan ⊆ releídas); D-PBT3-14 |
| «PREPARE -> MUTATE -> POST and one logical Update» | CT-PBT3B-07 a 09; Smoke-PB (un `U`) |
| OB-05.2 (composición) y los pasos 4 a 7 | CT-PBT3A-06 y 07; CT-PBT3B-11; CT-PBT3A-04 y CT-PBT3B-10 (paso 6) |
| OB-05.3 (clasificador) | CT-PBT3A-09 y 10 |
| «sin conectar» (OB-05.2, OB-05.5) | CT-PBT3A-12; CT-PBT3B-13 |
| AUTH-13 (decisión del Owner del 2026-11-19) | CT-PBT3A-04; CT-PBT3B-10 |
| D2, condiciones 1 a 4, 6 a 8 y 10; el límite | CT-PBT3A-05, 11 y 13; CT-PBT3B-12 y 13; CT-PBT3C-05; PBT3x-C17 |
| H-PB-17, H-PB-18 y la fila 3 del §6.4 | CT-PBT3A-08; CT-PBT3B-04 |
| D-1 (las sustituciones) | CT-PBT3B-14 |

---

## 6. Los contratos propuestos

### 6.0 Común a las tres subtareas

**Identidad:**
- `Schema` `rackcad-gate-contract/v1`; `Unit` I-64; `Gate` F4B;
- attempts 1 de 3 (OB-10.1); `RoutingEnforcement` `required`; `CorrectionsAuthorized` true;
- evidencia en `docs/automation/evidence/I-64-pilot/<TaskId>/`.

**Los `TaskId`:** de 24 a 26 caracteres, por MAX_PATH (R-T1-07).

**Las colecciones opacas** (F4B-E21): las ocho (`Authorities`, `Objective`, `AllowedWriteScope`, `ForbiddenWriteScope`, `Invariants`, `RequiredTests`, `ExpectedEvidence` y `StopConditions`) las pone la sesión byte a byte desde el contrato. El Controller hace la clase, el enrutamiento y los criterios de aceptación. Cada relevo lo declara.

**Anclajes:** antes de emitir, la sesión vuelve a medir sobre su `BaseSha` las anclas de R-1 a R-10, el censo de R-6 y los recuentos, y lo deja en el commit de emisión. El generador de cada contrato (`make_pbt3x_contract.py`) comprueba con `assert`:
- el diff de `src` frente al cierre de la subtarea anterior (vacío);
- las anclas de PP, UR, UP, WR y PM que cita el diseño;
- el texto del esqueleto en el paquete custodiado.

**La comprobación acotada** de cada subtarea: un revisor independiente, en solo lectura. Comprueba:
- que los anclajes se sostienen;
- que ningún hecho nuevo cambia un CT o el diseño;
- que no aparece ninguna decisión material nueva;
- que lo de D2 cubre sus condiciones como se declara;
- para PB-T3b, que la lista de D-1 es exacta sobre el esqueleto.

**`ForbiddenWriteScope`:** lo calcula el generador con el método de [PKC-C]:
- todo `src/` salvo el `AllowedWriteScope` de la subtarea;
- en `tests/`, las listas curadas de PB-T1 ([PKT1] §6.0), las clases de PB-T1 y de las subtareas anteriores de PB-T3, `tests/RackCad.Tests/Fixtures/I64/` y `tests/RackCad.UI.Tests/Workspace/`;
- en PB-T3a y PB-T3c, también `tests/RackCad.Tests/Workspace/` entero;
- las 17 entradas de la raíz.

**`StopConditions` estándar** (17), copiadas de [PKC2] §6.7: S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08 (P-01 con la Snapshot I).

**Las de [DP], literales, en cada contrato** (x = A, B o C): **PBT3x-C08** (una sola replanificación, solo por la comparación literal), **PBT3x-C11** (nc2 con la firma exacta de PD-F4-01 y nc1 con la de PD-F4-04, con los sustitutos copiados y solo el identificador y el ámbito cambiados; cualquier otra firma vuelve al Owner) y **PBT3x-C12** (una corrección sería el intento 3: vuelve al Owner). El texto es el de [PKT1] §6.0, con el prefijo cambiado.

**El límite de D2, en cada contrato:** **PBT3x-C17**, con el texto literal del Owner ([PKT1] §6.0). Si hiciera falta mutar el rack resuelto o el informe de módulos dentro del tiempo de vida de la edición o del borrador, o compartir otro objeto mutable entre una copia del estado y su original: STOP y se reabre L-1. El Worker no elige ninguna salida.

**El RED de la cadena en una corrección** (§16.8, M-11): una corrección que toque `RT` ∪ `ChainRedFiles` exige RED otra vez:
- en PB-T3a y PB-T3c, el commit RED vuelve a poner el esqueleto (`git restore --source=<RED del intento 1> -- <src del alcance>`);
- en PB-T3b, el esqueleto **y** el piloto de la base;
- el GREEN restaura la implementación sin tocar `tests/`.

**La `AuthorityRevision` de una corrección** (lección de PB-T1b; §16.3): se elige de modo que `git diff --name-only AuthorityRevision BaseSha` no contenga rutas `EXTERNAL` ni con secciones `UNIT_CHANGE`. Las rutas de producto que el intento anterior ya cambió están en la `BaseSha` y no son autoridad.

**La lectura de `Ci` en el RED** (lección M14 de PB-T1b, escrita aquí sin ratificación): en las tres subtareas, Core falla a propósito en el RED, así que `build-plugin`, que depende de `tests`, sale **omitido**. `ExpectedEvidence` lo pide así: `tests` en `failure`, `build-ui` en `success`, `build-plugin` omitido y `ui-tests` en `failure` (PB-T3a) o `success` (PB-T3b y PB-T3c). En el GREEN, 4/4 en `success`.

**`EligibleCells`:** las de [PKC2]. Worker `anthropic/subagent/claude-sonnet-5-5`, effort `high`; Controller `openai/process/gpt-6-luna`, effort `high`. Vencen el **2026-12-29**. nc4 en la primera delegación de cada cadena; nc1 a nc3 tras la verificación.

**Invariantes comunes** (cada contrato los numera INV-PBT3x-07 a 14):
- **07. Las redes:** §5.1 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, **salvo los seis `Skip` de D-1 en PB-T3b, con su motivo literal**.
- **08. Las pruebas de UI:** las reglas de §5.0.
- **09. Determinismo:** GUID fijo, cultura invariante, «R» y orden ordinal; ni hora ni aleatoriedad.
- **10. Las corridas locales:**
  - solo los filtros del contrato en UI, **nunca la suite completa de UI en local**; la de Core, sí;
  - sin copias de árboles (MAX_PATH);
  - las corridas temporales terminan en `Assert.Fail(string.Join("\n", lineas))`, corren dentro de `EditorDiscardPrompt.Substitute` cuando abren ventanas, no entran en ningún commit y guardan TRX y log como artefactos.
- **11. La evidencia:** rutas relativas comprobadas en `worker-handoff.json`; `TreeSha` real o null; el Worker no escribe en `docs/`.
- **12. Las reglas de F4B y sin AutoCAD;** la verificación con las tres comprobaciones deterministas de F4B-E15.
- **13. Los ayudantes privados** (lección M10 de PB-T1b, escrita aquí). Los métodos privados y los tipos anidados privados **sin estado** son libres dentro de los archivos del `AllowedWriteScope`, con dos condiciones:
  - no añaden miembros públicos ni internos fuera del API de §3;
  - no añaden campos de instancia a tipos del censo del piloto (HT1-21), ni campos estáticos salvo `static readonly` de tipo inmutable (`Regex`, `string`).
  
  Cualquier otro miembro es la parada C13.
- **14. D2:** fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna línea añadida muta un `PushBackSystem` ni un `RackModuleReconciliationResult` adoptado.

**Las propias comunes** (con el prefijo de cada contrato):
- **C01:** hacerlo bien exigiría escribir fuera del `AllowedWriteScope`.
- **C02:** habría que editar, saltar, reactivar o borrar una prueba o un ayudante existente (salvo los seis `Skip` de D-1), o tocar `InternalsVisibleTo`, los paquetes, un `.csproj` o `Fixtures/I64`. **Cualquier prueba existente que falle,** fuera de las seis de D-1, **vuelve al Owner** (OB-T1B.3).
- **C03:** aparece un cambio observable de RACKEDITAR, o del piloto en el panel fuera de la barrera sobre un diseño bloqueado. STOP y vuelta al Owner.
- **C05:** un CT necesitaría AutoCAD, un `MessageBox` real, el configurador, el BOM, «Guardar en biblioteca», «Insertar», el teclado o el foco real, o escribir en el perfil o en el disco.
- **C06:** la CI del GREEN falla fuera de las pruebas nuevas, se reproduce y no es atribuible a ellas.
- **C07:** los hechos del §2 no se sostienen de un modo que cambia un CT o el diseño; o una línea esperada no se cumple en el GREEN. El Worker no ajusta lo esperado.
- **C09:** una prueba colgaría, dejaría viva una ventana, un modal o un dispatcher, escribiría en el perfil o dependería de rutas de una máquina.
- **C10:** hacerlo bien exigiría lo que D-PBT3-14 excluye: crear vistas, tocar el clásico, la ventana, el anfitrión o el puerto del panel, conectar el modelo completo, abrir AUTH-13 o cambiar el trato de la cola aparcada.

### 6.1 Contrato de PB-T3a — `F4B-PBT3A-FULL-WRITE-MODEL`

- **Objective** (ASCII): «Crear en Application, solo con archivos nuevos en src/RackCad.Application/Systems/PushBack/Draft/, el modelo del borrador completo de Push Back sobre PushBackEditableState: el estado y su comparador, el asiento (la pasada y la secuencia de LoadExisting), el materializador y el lector sobre el lector del piloto (AUTH-13 intacto; A/B admitido solo en este modelo; la cola aparcada fuera), la composicion para escribir y para Conservar mi borrador, la disposicion de las vistas (lateral obsoleto, frontal B de un rack de un sentido, sin direccion, ultimo vinculo) y el clasificador de D-PB-05, sin conectarlos al panel, segun el paquete del contrato de PB-T3 v1 y la lista cerrada CT-PBT3A-01..14. El RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion salvo las tres guardas; el GREEN implementa solo los archivos nuevos. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida de la edicion o del borrador: STOP y se reabre L-1. El piloto, el anfitrion, la ventana, el Plugin y Domain no cambian; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (14):
  - los ocho archivos nuevos de `src/RackCad.Application/Systems/PushBack/Draft/`: `PushBackFullDraftState.cs`, `PushBackFullDraftStateComparer.cs`, `PushBackEditableStateSeat.cs`, `PushBackFullDraftMaterializer.cs`, `PushBackFullDraftReader.cs`, `PushBackFullComposition.cs`, `PushBackViewDisposition.cs` y `PushBackConflictClassifier.cs`;
  - las cinco clases nuevas de `tests/RackCad.Tests/` de §5.2;
  - `tests/RackCad.UI.Tests/PushBackFullDraftWindowParityTests.cs`.
- **El esqueleto exacto del RED:**
  - `PushBackFullDraftState`, completo (es un contenedor);
  - el comparador: `Instance`; `Equals` = `ReferenceEquals`; `GetHashCode` = 0; `Copy` = `new PushBackFullDraftState(new PushBackEditableState(), state.Origin)`;
  - el asiento: `Pass` devuelve `null`; `LoadExisting` devuelve `new PushBackEditableState()`;
  - `Materialize` devuelve `Failed(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic)`;
  - `Read` devuelve `NotAdmitted(PushBackPilotIntentReader.MaterializeDiagnostic)`, con la base de `PushBackPilotIntentReader.BuildStaleBase(scan)`;
  - `ForWrite` devuelve `Rejected(PushBackPilotMaterializer.NonPilotContentReason)`; `KeepOn` devuelve `current`;
  - `Of` da todo `Redraw` y `AllObsolete` falso;
  - `Classify` da todas las listas vacías y `AllViewsObsolete` falso.
  
  Los tipos, enumerados y propiedades, completos.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackFullDraft` (CT-02 a 11) | **10** | true |
  | UI | `PushBackFullDraftWindowParityTests` | **1** | true |
  | Core | `PushBackDraftWriteGuardTests` (CT-12 a 14) | **3** | false |
  | Core y UI | `Workspace` | 3 271 y 547 | false |
  | Core | `PushBackEditableState`, `PushBackEditorOutputGate`, `PushBackSharedReferenceGuardTests` | 13, 1, 2 | false |
  | Core y UI | Las clases de PB-T1c (`PushBackWindowSharedReferenceGuardTests`, `PushBackWindowOnStateSourceGuardTests`; `PushBackWindowOnStateTests`) | 1, 1; 2 | false |
  | UI y Core | `PushBackClassicCharacterization`; `PushBackFullEditorCharacterization` | 55 y 10; 16 y 3 | false |
  | UI | `PushBackStateNet`; `PushBackDeclaredCorrection`; `Cantilever` | 14; 14; 67 | false |
  | Core | `I58F1`; `CustomPropertiesEnvelopeGuardTests`; `NamespaceFolderGuardTests` | 10; 9; 7 | false |

  Los `MinSelected` de PB-T1 y de `Workspace` se vuelven a medir en la emisión.
- **Invariantes propios:**
  - **INV-PBT3A-01. Alcance:** el diff dentro de las 14 rutas; `git diff BaseSha..CurrentSha -- src` solo añade los ocho archivos de `Draft/` y no modifica ningún archivo existente.
  - **INV-PBT3A-02. RED y GREEN:** el RED es el esqueleto exacto y las seis clases de prueba; el GREEN solo toca los ocho archivos de `Draft/`. En el RED, cada filtro con `ExpectRed` da todos `Failed` por aserción (la primera línea del `StackTrace` bajo `tests/`), con `Failed == Selected`; el filtro de las guardas pasa y sus autopruebas fallan sobre el texto inyectado.
  - **INV-PBT3A-03. La API:** exactamente la de D-PBT3-02 a D-PBT3-07; ningún otro miembro público ni interno (con INV-13).
  - **INV-PBT3A-04.** Un solo `namespace RackCad.Application.Systems.PushBack.Draft` de bloque por archivo; ningún archivo nuevo compone, construye un `RackEmbedDocument` ni nombra lo prohibido por D-PBT3-08.
  - **INV-PBT3A-05.** El lector completo llama a `PushBackPilotIntentReader.Read` exactamente una vez por lectura, y el materializador completo a `PushBackEditableStateSeat.LoadExisting` exactamente una vez.
  - **INV-PBT3A-06.** Ningún archivo de `src/` fuera de `Draft/` nombra los tipos nuevos (CT-12).
- **`StopConditions` propias:**
  - **PBT3A-C04:** un método de un filtro con `ExpectRed` pasa sobre el esqueleto, o falla por excepción.
  - **PBT3A-C13:** la implementación necesitaría tocar un archivo existente de `src/`, un miembro fuera del API, o un campo en un tipo del censo.
  - **PBT3A-C15:** CT-01 da `estado-igual=False` o `diseno-igual=False` en algún diseño: el asiento no iguala a la ventana. Es un hecho nuevo y vuelve a la sesión, que lo lleva al Owner si es una pérdida (OB-06.5).
  - **PBT3A-C17:** el límite de D2.
- **`ExpectedEvidence`:**
  - antes del Worker: el contrato, la delegación, la comparación byte a byte, A1-A8 y nc4;
  - el RED y su CI (§6.0);
  - la tabla «CT → la aserción que falla en el RED» y las autopruebas de las tres guardas;
  - el GREEN y su CI 4/4;
  - en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED y el GREEN, y tres corridas en verde de los filtros con `ExpectRed` sobre el GREEN;
  - la entrega con el mapa CT → método y las rutas relativas comprobadas;
  - la verificación con las tres comprobaciones de F4B-E15, y nc1 a nc3.
- **Línea base de la CI:** la del cierre de PB-T1c, **a medir**. Prevista (R-0): Core 19 088; UI 2 439 (2 418 `Passed`, 21 `Skipped`). **GREEN previsto:** Core + 13 = 19 101; UI + 1 = 2 440.

### 6.2 Contrato de PB-T3b — `F4B-PBT3B-FULL-WRITE-RUN`

- **Objective** (ASCII): «Generalizar en su sitio el plan y la corrida de la actualizacion del Workspace de Push Back para que sirvan el modelo del piloto, conectado y sin cambio de comportamiento, y el modelo completo de PB-T3a, sin conectar: la barrera de diagnosticos con PushBackEditorOutputGate, el borrado de laterales obsoletos con la regla del ultimo vinculo, el rechazo del frontal B de un rack de un sentido, el renombrado con el nombre del borrador, ComparePost generico y la capacidad de borrar como interfaz nueva, con un solo BeginMutation y un solo Seal, segun el paquete del contrato de PB-T3 v1, la decision del Owner D-1 y la lista cerrada CT-PBT3B-01..14. El RED es el esqueleto inerte del API nuevo, la interfaz, las pruebas nuevas y el Skip de las seis pruebas sustituidas por D-1; el GREEN toca solo el plan y la corrida. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1. El Plugin, el anfitrion, el puerto del panel, la ventana, el clasico y Domain no cambian; no se crean vistas; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (11):
  - PP, UR y el nuevo `src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotViewEraser.cs`;
  - las cuatro clases nuevas de §5.3 y `tests/RackCad.Tests/Workspace/PushBackFullWriteSupport.cs`;
  - **solo para los `Skip` de D-1:** `tests/RackCad.Tests/Workspace/WorkspacePilotPrepareGuardTests.cs`, `…/WorkspacePilotUpdateGuardTests.cs` y `…/WorkspacePilotUpdateIntegrationGuardTests.cs`.

  El registro `F4B-DRAFTS/superseding-characterization.md` no entra: lo escribe la sesión en el commit de emisión (§8.2).
- **El esqueleto exacto del RED:**
  - en PP: `PushBackFullPrepareInput` completo (es un contenedor con su `From`); la sobrecarga completa de `Prepare` devuelve `WithInvalid(MissingInputReason)`; `Erasures` y `ErasedKeys`, vacías; las constantes nuevas, completas;
  - en UR: `Precheck` completo devuelve `Go()`; `Execute` completo devuelve `NotAppliedFailure(UpdateNotRunReason)`; la sobrecarga completa de `ComparePost` devuelve `PostRereadMissingReason`; las constantes nuevas, completas;
  - **el camino del piloto, sin cambio.**
- **El `Skip` de D-1,** con el motivo literal de §5.3: en S-1 a S-6. Es el único cambio permitido en esos tres archivos; `git diff -U0` lo comprueba.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackFullWrite` (CT-01 a 12) | **12** | true |
  | Core | `PushBackPilotUpdateEquivalenceGuardTests` (CT-14) | **6** | true |
  | Core | `PushBackWritePathGuardTests` (CT-13) | **2** | false |
  | Core | `Workspace` | **3 271 − <los resultados de S-1 a S-6>** (a medir en el RED: 10 si la `Theory` S-2 cuenta sus cinco filas) | false |
  | Core | `PushBackFullDraft`, `PushBackDraftWriteGuardTests` (PB-T3a) | 10, 3 | false |
  | | Y los demás de §6.1, salvo los de PB-T3a con `ExpectRed` | | false |
- **Invariantes propios:**
  - **INV-PBT3B-01. Alcance:** el diff dentro de las 11 rutas. En `src/`, solo PP, UR y el archivo nuevo. En los tres archivos de guardas de F4, solo las seis líneas de atributo con `Skip`.
  - **INV-PBT3B-02. RED y GREEN:** como INV-PBT3A-02. El GREEN solo toca PP y UR.
  - **INV-PBT3B-03. El API existente no cambia:** cada miembro público e interno de PP y UR en la `BaseSha` existe en el GREEN con la misma firma. Solo se añade lo de D-PBT3-09 y D-PBT3-10.
  - **INV-PBT3B-04. El piloto no cambia.** Con un borrador del piloto:
    - `Prepare` conserva su orden y sus textos;
    - la corrida hace las mismas llamadas al puerto;
    - `ComparePost` da lo mismo;
    - la barrera no cambia el corpus de R-8.
    
    Lo prueban las clases de comportamiento del piloto de §4 en verde.
  - **INV-PBT3B-05. Las guardas que siguen** (§4): la llamada a `ComposeForWrite` en el `Build` del piloto, tras `.CompareWith(`; `From(DraftSession<PushBackPilotDraftState>)` y una sola `new PushBackPilotPrepareInput(`; el `Prepare` del piloto primero en el archivo; ninguna palabra `Transaction` en `Pilot/`; las llamadas al puerto solo en UR; UP sin cambio.
  - **INV-PBT3B-06. Un solo «Actualizar» lógico:** un `Begin`, un `BeginMutation`, un `Seal` y un `Post` por corrida, y todo `EraseUnit` entre `BeginMutation` y `Seal` (CT-07).
- **`StopConditions` propias:**
  - **PBT3B-C04:** como PBT3A-C04.
  - **PBT3B-C13:** la implementación necesitaría cambiar una firma existente, tocar UP, el Plugin, WHC o el puerto del panel, o un miembro fuera del API.
  - **PBT3B-C14:** una prueba de comportamiento del piloto de F4 falla. STOP: es C03.
  - **PBT3B-C16:** fuera de S-1 a S-6, alguna guarda de F4 falla en el RED o en el GREEN. Vuelve al Owner con la lista (OB-T1B.3).
  - **PBT3B-C17:** el límite de D2.
- **`ExpectedEvidence`:** como §6.1, más el `git diff -U0` de los tres archivos de guardas y el recuento de `Skipped` del filtro `Workspace` en el RED y en el GREEN.
- **Línea base de la CI:** la del cierre de PB-T3a. **GREEN previsto:** Core + 20 (12 + 6 + 2), con los `Skipped` de Core + 6 o + 10 (R-11); UI sin cambio.

### 6.3 Contrato de PB-T3c — `F4B-PBT3C-FULL-WRITE-ERASE`

- **Objective** (ASCII): «Dar al escritor de la actualizacion del Workspace de Push Back la capacidad de borrar una vista obsoleta dentro de la unica transaccion de MUTATE: una fachada caller-owned nueva (RackViewBlockEraser.EraseInTransaction, sin bloqueo, sin transaccion propia, sin confirmacion y sin purga, con el borrado de las referencias directas y la captura de las definiciones anidadas del borrado clasico) y su uso desde EraseUnit del ambito de MUTATE, con PrepareUnits y la relectura de MUTATE que cubren las vistas a borrar, segun el paquete del contrato de PB-T3 v1 y la lista cerrada CT-PBT3C-01..05. El RED son las guardas de texto nuevas; el GREEN toca solo el escritor y la fachada. Las guardas del escritor de F4 siguen en verde. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1. Application, la ventana, el clasico y Domain no cambian; no se ejecuta AutoCAD.»
- **`AllowedWriteScope`** (4): WR; `src/RackCad.Plugin/Views/RackViewBlockEraser.cs`; `tests/RackCad.Tests/PushBackViewEraseGuardTests.cs` y `tests/RackCad.Tests/PushBackWritePathPluginGuardTests.cs`.
- **El RED:** solo las dos clases de prueba. El producto no cambia. CT-01 a 04 fallan por aserción, porque el texto no existe; CT-05 pasa.
- **`RequiredTests`:**

  | Proyecto | Filtro | `MinSelected` | `ExpectRed` |
  |---|---|---|---|
  | Core | `PushBackViewEraseGuardTests` | **4** | true |
  | Core | `PushBackWritePathPluginGuardTests` | **1** | false |
  | Core | `WorkspacePilotUpdateGuardTests`, `WorkspacePilotUpdateIntegrationGuardTests` | los medidos tras PB-T3b | false |
  | Core | `PushBackSharedReferenceGuardTests` (CT-PBT1B-15: 14 archivos y 7 asignaciones) | 2 | false |
  | | Y los de §6.2 sin `ExpectRed` | | false |
- **Invariantes propios:**
  - **INV-PBT3C-01. Alcance:** dentro de las 4 rutas. `git diff BaseSha..CurrentSha -- src/RackCad.Application src/RackCad.UI src/RackCad.Domain src/RackCad.Plugin/RackPushBackCommands.cs src/RackCad.Plugin/RackCommandSupport.cs` vacío.
  - **INV-PBT3C-02. RED y GREEN:** como INV-PBT3A-02.
  - **INV-PBT3C-03. El escritor:** sigue con tres clases `RackPilot*`, un `StartTransaction`, un `Commit`, una purga y dos `[Conditional("DEBUG")]`. No nombra `GetBlockReferenceIds`, `Erase(`, `EraseViewBlocks`, `ForWrite` ni `UpgradeOpen`. No añade ninguna asignación `.Name =`.
  - **INV-PBT3C-04.** La fachada no contiene la cadena `PushBack`, y el censo de CT-PBT1B-15 sigue en 14 archivos.
- **`StopConditions` propias:**
  - **PBT3C-C04:** como PBT3A-C04.
  - **PBT3C-C13:** la implementación necesitaría tocar Application, RPC, RCS u otro archivo del Plugin.
  - **PBT3C-C16:** una guarda del escritor de F4 falla. Vuelve al Owner: D-1 no cubre el escritor.
  - **PBT3C-C17:** el límite de D2.
- **`ExpectedEvidence`:** como §6.1. La CI del RED con `build-plugin` omitido; la del GREEN 4/4, con el Plugin compilado.
- **Línea base de la CI:** la del cierre de PB-T3b. **GREEN previsto:** Core + 5; UI sin cambio.

---

## 7. Riesgos y lecciones aplicadas

| Id | Riesgo o lección | Tratamiento |
|---|---|---|
| R-T3-01 | **Sin latitud para el Worker** | El API, el esqueleto, las listas de CT, los textos y las recetas están en el paquete. Lo que no está es una parada |
| R-T3-02 | **Nada de AutoCAD en el Worker** | C05; el Plugin se prueba como texto; lo físico, en el Smoke-PB (§5.5) |
| R-T3-03 | **Corridas temporales** | Con `Assert.Fail`, fuera de todo commit, con TRX y log (INV-10) |
| R-T3-04 | **MAX_PATH** | `TaskId` de 24 a 26 caracteres; sin copias de árboles |
| R-T3-05 | **Un solo «Actualizar» lógico** | Una transacción con los borrados dentro (INV-PBT3B-06, CT-PBT3B-07); POST una vez; Smoke-PB mide el `U` |
| R-T3-06 | **Seis guardas de F4 no pueden seguir en verde** | D-1, antes del contrato de PB-T3b; equivalentes que se ejecutan; C16 para cualquier otra |
| R-T3-07 | **El coste del comparador** (M15 de PB-T1b) | `ForWrite` compara una vez por «Actualizar»; se mide en el Smoke-PB (R-PB-06) |
| R-T3-08 | **La tentación de un segundo escritor** | Prohibido: G-2 y G-7 siguen en verde y el diseño lo exige (D-PBT3-01) |
| R-T3-09 | **PB-T2 en paralelo** (R-PB-09) | Alcances disjuntos: PB-T2 toca la ventana; PB-T3, `Draft/`, PP, UR y WR. Delegaciones sucesivas; la que vaya después se vuelve a medir sobre la punta |
| R-T3-10 | **Las lecciones de PB-T1b** | Los ayudantes privados, admitidos por regla (INV-13); `build-plugin` omitido en el RED (§6.0); la `AuthorityRevision` de una corrección (§6.0); el tamaño del contrato (las colecciones opacas, por la sesión, F4B-E21; listas de autoridades y de alcance prohibido calculadas por el generador) |
| R-T3-11 | **Un modelo interino:** entre PB-T3 y PB-T4 hay dos composiciones (la del piloto y la completa) | Se declara como interino, igual que R-T1-17. PB-T4 retira la del piloto al conectar el modelo completo |
| R-T3-12 | **El censo de `Name =` del Plugin** (CT-PBT1B-15: 14 archivos con `PushBack`) | La fachada no contiene `PushBack` (INV-PBT3C-04) |
| R-T3-13 | **El censo de I-54** de los que componen | Solo `ComposeMemberEnvelope(source, …)`; nada nuevo compone (INV-PBT3A-04) |
| R-T3-14 | **nc1 con variaciones** (PB-TC, PB-TC2) | C11: vuelve al Owner. Ninguna aceptación puntual anterior se extiende |
| R-T3-15 | **Las celdas vencen el 2026-12-29** | A7 en cada emisión y delegación; tres cadenas antes de esa fecha |
| R-T3-16 | **La admisión del compuesto depende de dos constantes del piloto** (D-PBT3-04) | CT-PBT3A-03 la fija regla a regla. Si el piloto cambiara esos textos, CT-03 salta |
| R-T3-17 | **El asiento no iguala a la ventana en algún diseño** | CT-PBT3A-01 con su parada C15; el oráculo es la ventana, a través de PB-T1c |

## 8. Las decisiones del Owner

### 8.1 Ya tomadas, que este paquete aplica

- **[OD], la fila de PB-T3** (literal, arriba) → todo el paquete.
- **[OD] D-PB-04:** «F4B may delete obsolete sibling views when the new full design no longer requires them. Creating NEW views is NOT yet authorized by this decision.» → D-PBT3-06, D-PBT3-09 a D-PBT3-11. **El bloque M** resolvió la fila 1 del §6.4: «PB-T3 iguala al clásico sin crear vistas … No hace falta volver al Owner» (BM:94) → D-PBT3-14.
- **[OD] D-PB-05:** los pasos 4 a 7 («all fields OWNED by the full Push Back editor come from the user's draft»; «envelope/infrastructure fields NOT owned by that editor come from the current authoritative rack»; «unknown/future ExtensionData must not be silently destroyed»; «identity/schema/session metadata must not be rolled back from a stale draft») → D-PBT3-05, CT-PBT3A-06 y 07, CT-PBT3B-11. Los pasos 1 a 3 y OB-05.4 son de PB-T4 (OB-05.5).
- **La ExtensionData anidada** (2026-11-19, literal «Basta así (Recomendado)»): el panel mantiene el rechazo con aviso de AUTH-13 a todo campo desconocido a cualquier nivel, y el contrato de PB-T3 lo fija con una prueba → D-PBT3-13, CT-PBT3A-04 y CT-PBT3B-10. La pérdida silenciosa de RACKEDITAR queda declarada como limitación conocida anterior a F4B, sin tocar el clásico.
- **D2** (F4B-E11), con su límite literal («This is NOT authorization for general shallow copying. … STOP and reopen L-1.») → D-PBT3-12 y PBT3x-C17.
- **F4B-E21** («La sesión copia; resto de F4B (Recomendado)») → §6.0.
- **F4B-E22**: las ratificaciones de M10 y M14 «solo PB-T1b». Aquí no se ratifica nada: el texto del contrato ya las prevé (INV-13 y §6.0).
- **[DP]** (F4B-E03) → C08 y C11.
- **D-PB-10** («Each separately contracted PB task starts at: 1 / 3 … Attempt 3 always returns to Owner») → las tres subtareas en 1/3 y C12.

### 8.2 Decisiones materiales nuevas

**D-1. Sustituir seis pruebas de F4 que fijan por texto el escritor del piloto** (antes del contrato de PB-T3b; no bloquea PB-T3a).

- **Los hechos** (R-6): para renombrar, borrar vistas obsoletas y tener un solo escritor que sirva los dos modelos, hay que cambiar el texto del plan y de la corrida que fijan seis métodos de F4-T4:
  - **por premisa,** S-1 a S-3 (PrepareGuard:121-139 y :229-251): fijan «the name is the one of the envelope that was re-read: never a value of the draft» y «a view that is not available is an Invalid, never a deletion»;
  - **solo por el tipo,** S-4 a S-6 (UpdateGuard:126-141; IntegrationGuard:895-910): fijan literales con `DraftApplyOutcome<PushBackPilotDraftState>` en la corrida, y su seguridad (los ámbitos en `finally`; el escape clasificado por el estado de la corrida) se conserva.
- **Por qué vuelve al Owner:**
  - [OD] §7.3 («Do not silently weaken the old guard»);
  - [P] OB-02.2 (F4B no reescribe F4);
  - la regla de [P] OB-T1B.3 para G-10 de F4: con la lista exacta, antes del contrato.
  
  La sesión no decide por analogía.
- **La propuesta (A, recomendada):** el patrón de I-39 y de OB-T1B.2:
  - cada uno de los seis queda con `Skip` y un motivo que nombra el registro;
  - un equivalente que se ejecuta (E-1 a E-6, CT-PBT3B-14) cubre todo lo que cubría;
  - la sesión escribe el registro en `F4B-DRAFTS/superseding-characterization.md`, en el commit de emisión de PB-T3b, con la prueba vieja, la nueva, la autoridad ([OD] fila de PB-T3 y D-PB-04, y D-1) y el motivo.
  
  Ninguna otra guarda se sustituye: cualquier otra que falle es la parada C16.
- **Alternativas:**
  - **(B) No sustituir:** PB-T3 se queda sin renombrar ni borrar en el escritor del panel. Las dos cosas que pide el [OD] («rename; deleting obsolete views») quedan sin hacer, y habría que volver más adelante.
  - **(C) Un segundo escritor aparte:** contradice «NO SECOND WRITE PATH» y G-7 de F4. Habría que sustituir esas guardas igualmente y retirar el escritor viejo en PB-T4. No se recomienda.
- **La pregunta, lista para AskUserQuestion:**
  - **header:** `Guardas F4`
  - **question:** «PB-T3b: para que el «Actualizar» del panel pueda renombrar y borrar vistas obsoletas con un solo escritor, hay que cambiar el texto que fijan seis pruebas de F4: tres fijan que el nombre sale del dibujo y que el panel nunca borra, y tres solo fijan un tipo. ¿Las sustituyo con registro y una prueba equivalente cada una?»
  - **options:**

    | label | description |
    |---|---|
    | `Sustituir con registro (Recomendado)` | Las seis quedan con Skip y motivo, cada una con una prueba nueva que cubre lo mismo, y el registro declarado. Cualquier otra prueba que falle vuelve a ti. |
    | `No: sin renombrar ni borrar` | PB-T3 escribe campos, módulos, frentes y lado B, pero no renombra ni borra vistas en el panel. Esas dos cosas, que pide la fila de PB-T3, quedarían pendientes. |
    | `Un segundo escritor` | Otro escritor aparte del del piloto. Rompe igual las guardas de F4 de «un solo camino de escritura» y hay que retirar el viejo en PB-T4. |

**D-2 (informativa, no pide decisión): la premisa «el escritor nunca borra» del Plugin cambia sin sustituir ninguna guarda.** Las guardas del escritor de F4 fijan por nombres que el escritor no borra ni abre nada para escribir (R-7). Con la fachada de PB-T3c siguen en verde por su letra, pero el escritor ya borra. No se debilita ninguna: siguen comprobando lo mismo, y las guardas nuevas de PB-T3c fijan el borrado (un solo llamador, dentro del ámbito propio de MUTATE). Si la revisión lo lee como un debilitamiento, entra en D-1 con sus métodos.

### 8.3 Lo que no es decisión

- **La división en tres subtareas:** OB-10.1 y D-PB-10 la prevén; el contenido, el orden y las dependencias de la fase no cambian. Se informa en la emisión.
- **La regla del frontal B:** el valor por defecto de [P] §6.4 fila 3. La revisión de este paquete decide entre él y las dos alternativas (BM:96).
- **Los textos nuevos** de D-PBT3-09 y D-PBT3-10 (`BlockedPrefix`, el del frontal B, `AllViewsObsoleteReason` y `EraseUnsupportedReason`): si la revisión los considera una decisión material de producto, vuelven al Owner.
- **La barrera también en el piloto:** es la letra del [OD] («blocking authored write on diagnostics/errors») para «the Workspace authored writer», y no cambia el corpus (R-8).
- **Quitar el rechazo A/B del panel (OB-03.2) en PB-T4, no en PB-T3:** [P] lo prevé «en PB-T3 o PB-T4», y el panel no usa el estado completo hasta PB-T4 (A-3).
- **El asiento en el producto:** es la API que A-2 prevé «si PB-T3 o PB-T4 la necesitan».
- **La capa bloqueada sobre una vista a borrar y la superviviente contada sin las borradas:** el panel es más estricto que el clásico, con la regla C-F4-05 de F4.

## 9. Límites

En `pbt3-measurements.md`, R-11 y R-12. En resumen:
- **Nada se compiló ni se ejecutó.** Todas las cifras de la CI son previsiones.
- **El worktree no se leyó.** Todo se leyó por objetos git en `2f5d3dec`, que es el producto de PB-T1b del intento 1. PB-T1b (intento 2) y PB-T1c no existen todavía: este paquete se vuelve a medir sobre el cierre de PB-T1.
- **La lista de D-1 sale de leer las guardas frente al diseño,** no de correrlas sobre un diff real. La comprobación acotada de PB-T3b la vuelve a medir sobre el esqueleto, y la parada C16 cubre lo que se escape.
- **No medido:** cómo cuenta xUnit una `Theory` con `Skip`; el Deshacer real con borrado en el panel; el frontal B junto a un frontal de A del mismo nombre.
- **Una incidencia de procedimiento** (R-12): un `python -I -` con heredoc que solo imprimía `noop`. No leyó nada y no se repitió.

## 10. Arrastres que salen de PB-T3

| Id | Qué | Va a |
|---|---|---|
| A-T3-1 | Conectar el modelo completo al panel: el tipo del borrador del anfitrión, el puerto generalizado, «Editar todo» y la retirada del materializador y del copiador del piloto (A-5 de PB-T1). **Al retirar el modelo del piloto, las pruebas de comportamiento del piloto de F4** (§4) dejarán de describir el panel: PB-T4 vuelve al Owner con su lista, como D-1 | PB-T4 |
| A-T3-2 | OB-03.2: quitar el rechazo A/B del panel, con su prueba, al conectar el lector completo (A-3 de PB-T1) | PB-T4 |
| A-T3-3 | OB-05.3 a OB-05.5: presentar el clasificador, la elección explícita y la confirmación, y conectar `KeepOn` en lugar de `KeepPilotEditsOn`. Las guardas «sin conectar» (CT-PBT3A-12 y CT-PBT3B-13) se cambian entonces con su registro | PB-T4 |
| A-T3-4 | D2, condiciones 3 a 7 sobre el `DraftSession` del modelo completo (A-2); M11 (diseño nulo) y M15 (coste de `Equals` en `IsDirty`) de PB-T1b | PB-T4 |
| A-T3-5 | Lo físico de §5.5 | PB-T5 (Smoke-PB) |
| A-T3-6 | **Nota para el paquete de la fase** (de la sesión, no del Owner): la línea ~314 de [P] («…se pierden en cada reescritura, clásica o del panel») se precisa: el escritor pierde lo anidado, pero el panel no llega a escribir esos racks (AUTH-13). La promesa de OB-05.2 de conservar la ExtensionData de nivel superior en el panel no se ejercita hoy, porque AUTH-13 también la rechaza; CT-PBT3B-11 la prueba solo por construcción | La sesión, en [P] |
| A-T3-7 | El registro de la decisión del Owner del 2026-11-19 y su número F4B-E | La sesión, en `f4b-dispositions.md` |
