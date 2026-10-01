# I-63 — Paquete para el Architect, ronda 2 (re-revisión de la Proposal V2)

```text
Iniciativa: I-63 / ID20 — Computed Parameters & Project Summary Foundation (NEW ARCHITECTURE)
Revisor: el MISMO Architect de R1 (SEPARATE SESSION, sesión local_d5787742…, «I-63 Architect Review R1»),
         porque solo él puede cerrar o rebajar A63-PV1-01..10 (INITIATIVE_LIFECYCLE §5)
Objeto: docs/initiatives/I-63-proposal-v2.md, blob 9a84546fdccbe77593c78352521ab8bb926a3d96, en el commit que publica este paquete
        (SHA y CI en el informe al Coordinator)
Versión anterior (histórica, inmutable): docs/initiatives/I-63-proposal-v1.md en
        772ac242430084918d319a7683ce615daec39452, blob 48a68307be3f046c20c2405252dc8af3ad050342;
        paquete v1, blob 98e34b183c4c9829c18fec3cc822b342f6753e52
Base de código: origin/main 819955d61a6da4c811a11fbd11b5dca13f634b7c
Estado: re-revisión pedida, NO realizada. Freeze: NO. IMPLEMENTATION AUTHORIZATION = NO
```

## 1. Veredicto R1

| Campo | Valor |
|---|---|
| Modo | SEPARATE SESSION. Revisor ≠ autor. Mismo modelo de base probable (`claude-opus-5-5`), registrado por el Architect como **limitación de independencia** |
| Objeto | `772ac242` / `docs/initiatives/I-63-proposal-v1.md` / blob `48a68307` |
| Veredicto | **CHANGES REQUIRED**, NOT READY FOR FREEZE |
| REQUIRED | A63-PV1-01..10 |
| OPTIONAL | O-1..O-7. La orden PV2 incorpora O-01..O-06 y eleva O-07 a requisito |
| Texto completo | Anexo R1, literal, con su procedencia |

## 2. Disposición por hallazgo

Cada REQUIRED se resolvió con la **resolución vinculante** de la orden PV2 (decisiones de I-63 §2, «Orden PV2»).

| ID | Resolución vinculante aplicada | Dónde en V2 |
|---|---|---|
| A63-PV1-01 | Las referencias sin namespace (simples, con llaves o cualificadas) buscan **solo** en `projectVariable`; `Rack.<miembro>` solo en `rack`; `OperatorInName` y homónimos por namespace; el miembro es `Frentes` y la referencia textual completa, `Rack.Frentes`; `Rack.` sin miembro da `NameRequired` donde se ofrece `rack` (si no, `UnknownNamespace`) | §5 D-03; §13 D-16.4-5; INV-19, INV-20 |
| A63-PV1-02 | `rack` siempre se escribe `Rack.<miembro>`, nunca con `Q(key)`; el ausente tiene forma diagnóstica `Rack.#{token}`, que se muestra y no enlaza; la clasificación canónica de `Rack.X` es `Expression`; INV `Format → Parse → Bind` con homónimo | §13 D-16.6-7; INV-21..23 |
| A63-PV1-03 | Anexo A rehecho cláusula por cláusula: se conserva, se modifica y se sustituye. Cubre D4, D5, D6 (regla 4, reglas de nombres y Formatter), D9 (en memoria frente a persistidos), D24, D25 y V6 P25.2, P25.3 y P25.5 (este último, sustituido) | §23; anexo A |
| A63-PV1-04 | Opción (a): solo `rack` en el núcleo; `project` reservado y sin código; `MetricId` del catálogo frente a `SymbolId`; `RacksBySystem` y los mapas nunca son símbolos | §5 D-02; §6; §12 R6; §14 |
| A63-PV1-05 | Función **única** de Application `RackOutputVerdict(kind, design, catálogo) → Allow \| Deny \| Undetermined`, que consumen el handler (sin cambio de comportamiento: *fail-open*) e I-63 (`Undetermined` → cobertura); M-01 modificador y M-04; INV de conformidad | §10 D-27; §3; INV-12, INV-13 |
| A63-PV1-06 | Proyección pura `EnvelopeUnreadable \| IdAbsent \| KindAbsent \| KindUnknown \| Known`; un solo comparador de kind (`Ordinal`); entrada tipada de E4; **E5 real para los seis kinds** (lector por kind) → `Undetermined(DesignUnreadable)`; P-02a intacto | §10 D-10a, E5, D-26; INV-06 |
| A63-PV1-07 | Hermanas = todas las del RackId, colocadas o no; orden canónico por `DefinitionKey` `Ordinal` antes de E3/E4; grafía canónica = mínima `Ordinal`; representante y `DisplayName` tras el orden; *provenance* determinista; INV ampliado | §10 D-11; INV-02, INV-11, INV-29 |
| A63-PV1-08 | Operación congelada `RackMetricRequest` (un RackId + registro + catálogo) → `RackMetricResults` → `RackComputedExpressionContext`: sin proyecto ni población, como máximo un *resolve*; propagación tipada (`ComputedReferencesNotAvailable` con cada estado) sin colapsar en `BrokenReference`; casos Push Back y Cabecera | §8 D-17; INV-14..16 |
| A63-PV1-09 | Opción (a): sin adaptador nuevo del Plugin; Application pura con contratos de entrada que construyen las pruebas; gates rediseñados, ninguno cierra por compilar; OV candidata NOT APPLICABLE, sujeta a la re-revisión y al Freeze | §19 D-25; §21; §22 |
| A63-PV1-10 | `BySystem` con los seis *tokens* siempre; sistema ausente → `rackCount` `Available(0)`; cada métrica por sistema con su `MetricValue` explícito; nada se infiere de `null`; INV | §10 D-12; §14; INV-07 |

**Opcionales:**

| ID | Disposición |
|---|---|
| O-01 | Incorporado: vocabulario de soporte de diseño (D-06) separado del estado en ejecución (D-09), con su correspondencia con el mandato |
| O-02 | Incorporado: la coherencia de kind es una regla **propia de I-63** (E3) |
| O-03 | Incorporado: `RackComputedExpressionContext` vive fuera del núcleo y usa el mismo documento de registro que la Φ2 del rack |
| O-04 | Incorporado: niveles `Population` (`ProjectPopulation`, sin métricas) y `Full` (`ProjectSummary`), como tipos distintos y sin estado `NotRequested` (AQ-05) |
| O-05 | Incorporado: dos tablas, la de *tokens* persistidos de D9 (sin cambios) y la de namespaces en memoria |
| O-06 | Incorporado: tabla cerrada de identificadores de autoridad fuente (D-21; INV-31) |
| O-07 | **Requisito de la orden:** toda guarda tiene un RED observable, sea porque la API no existe o por un control positivo (§20) |

## 3. Delta V1 → V2

| Sección | V1 | V2 |
|---|---|---|
| Identidad (§5) | `SymbolId` para todas las métricas; namespaces `rack` y `project` en el núcleo | `MetricId` para todas, y `SymbolId(rack, t)` solo para las *bindable*; `project` fuera del núcleo |
| Nombres (§5, §13) | Nombre visible ambiguo («Frentes» / «Rack.Frentes»); búsqueda global | Miembro `Frentes`, referencia `Rack.Frentes`; índices, homónimos y `OperatorInName` por namespace |
| Formatter (§13) | Solo la forma «presente» | `Rack.<miembro>` siempre; ausente `Rack.#{token}`; clasificación `Expression` |
| Persistencia (§13) | Un conjunto de persistibles sobre la tabla común | Dos tablas; D9 intacta |
| Operación por rack (§8) | Contexto construido desde el resumen | `RackMetricRequest` congelada; propagación tipada por estado |
| Población (§10) | Proyección vigente; E4 aprueba sin leer en los no selectivos; E6 citaba el Plugin | Proyección propia; lector por kind (E5 real); veredicto único (E6); hermanas y orden canónicos |
| Agregados y modelo (§10, §14) | Campos opcionales; claves solo de los presentes | Seis *tokens* siempre; estados explícitos; `Population` / `Full` |
| Capas y gates (§19, §21) | Adaptador del Plugin «que compila» en G4 | Sin adaptador; G1..G4 con resultado conductual; solo la delegación de E6 en el Plugin |
| Materialidad (§3) | M-01 creador | M-01 creador **y modificador** (E6); condiciones de M-02 y M-03 explícitas |
| ADR (§23, anexo A) | D5, D9, D24 y D25 | Cláusula por cláusula, con P25.5 sustituido |
| INV (§20) | INV-01..20 | INV-01..31, con RED observable o control positivo |

Las secciones no listadas (§1, §2, §4, §7, §11, §12, §15..§18, §22) cambian solo para reflejar lo anterior.

## 4. Lo que hay que re-revisar

1. Cada disposición de la tabla de §2, contra el hallazgo original del anexo R1. El autor **no** se rebaja nada: solo el Architect
   puede cerrar o rebajar sus REQUIRED.
2. La versión completa: un hallazgo material en una sección intacta sigue siendo válido (LIFECYCLE §5).
3. Las preguntas AQ-01..AQ-05 de la Proposal V2 §24.
4. Los 14 desafíos de la orden PV1 y los del mandato, que siguen vigentes (paquete v1 §4).

**Material de entrada:** el mismo del paquete v1 §2 (Discovery R2 aceptado y §§24-26; autoridad de I-49 por blob; ADR-0039 e I-54 D-16;
mandato), más la orden PV2 en las decisiones de I-63 §2 y la evidencia §17.

## 5. Encargo listo para la sesión del Architect R1

```text
I-63 — ARCHITECT RE-REVIEW R2 (Proposal V2)
Eres el mismo Architect que emitió R1 (A63-PV1-01..10). Declara en la primera línea:
Review mode: SEPARATE SESSION; revisor=autor: no; misma sesión que R1: <sí/no>.
Haz git fetch origin y lee SIN cambiar de rama ni escribir, con git show:
  <COMMIT_V2>:docs/initiatives/I-63-proposal-v2.md          (blob 9a84546fdccbe77593c78352521ab8bb926a3d96)
  <COMMIT_V2>:docs/initiatives/I-63-architect-package-v2.md (delta, disposición por ID y tu veredicto R1 literal)
y el código y las autoridades de origin/main 819955d61a6da4c811a11fbd11b5dca13f634b7c.
Para cada A63-PV1-nn decide: CERRADO, o sigue ABIERTO con la razón. Solo tú puedes rebajarlo.
Revisa la V2 completa; un hallazgo nuevo es A63-PV2-nn (REQUIRED|OPTIONAL), con evidencia y corrección mínima.
Responde AQ-01..AQ-05. No implementes ni congeles.
Termina con AGREED POINTS, DISAGREEMENTS, MATERIAL RISKS, REQUIRED CHANGES, OPTIONAL IMPROVEMENTS y
CONSENSUS STATUS = AGREED | CHANGES REQUIRED | BLOCKED — OWNER DECISION, ligado a <COMMIT_V2> / ruta / blob.
```

`<COMMIT_V2>` es el SHA del commit que publica este paquete; se informa al Coordinator junto con su CI.

## Anexo R1 — Veredicto del Architect R1, literal

**Procedencia:** texto final de la sesión revisora (sesión local `local_d5787742…`, «I-63 Architect Review R1»). Se extrajo de su
transcripción local (`fdd2a979-cce9-4391-83aa-e7dfe5effcc4.jsonl`, mensaje de 2026-10-01T22:50:30Z, modelo `claude-opus-5-5`). SHA-256
del texto en UTF-8: `a8ccab34a911429603cafcc2cb7c5a4008e95871c166b0667e5d46fa447526dc`. No llegó como archivo a `D:\IDs\I-63`. La orden
PV2 del Coordinator confirma su identidad (modo, commit, blobs, veredicto y REQUIRED).

```text
# I-63: revisión del Architect R1 sobre la Proposal V1

**Review mode: SEPARATE SESSION.** Revisor y autor no son la misma persona.

## 1. Review identity

| Campo | Valor |
|---|---|
| Modo | SEPARATE SESSION |
| ¿Es la misma sesión que la autora? | No. Revisora: `local_d5787742…` («I-63 Architect Review R1»), creada a las 22:41Z. Autora: la sesión principal de I-63 (`2d2cc72a…`) |
| ¿Mismo modelo de base? | **No lo pude verificar.** La sesión autora no aparece en el gestor de sesiones. La revisora usa `claude-opus-5-5`. Lo registro como **probable SÍ**, es decir, como una **limitación de independencia**. No lo trato como SAME-SESSION ROLE |
| Commit revisado | `772ac242430084918d319a7683ce615daec39452` (descendiente de `819955d6`, comprobado) |
| Proposal | `docs/initiatives/I-63-proposal-v1.md`, blob `48a68307be3f046c20c2405252dc8af3ad050342` (comprobado con `git rev-parse`) |
| Paquete | `docs/initiatives/I-63-architect-package-v1.md`, blob `98e34b183c4c9829c18fec3cc822b342f6753e52` (comprobado) |
| Base de código | `origin/main` = `819955d61a6da4c811a11fbd11b5dca13f634b7c` (comprobado tras `git fetch`) |
| Escrituras en el repositorio | Ninguna. Leí todo con `git show <sha>:<ruta>` |

## 2. Agreed points

1. **D-01 / D-23:** la separación entre ComputedParameter, ProjectVariable y CustomProperty es correcta en la tabla. Las CustomProperty quedan fuera de los namespaces.
2. **D-02:** la identidad es `SymbolId(namespace, token)` y el token se declara explícitamente. Lo prefiero a los GUID por las razones de §5. La regla A2 de `projectVariable` no cambia (INV-14).
3. **D-05 / D-07:** `Rack.Frentes` = `BaysOfFondo(system, 0).Count`, vacíos incluidos. `FrentesVacios` usa el predicado `Levels.Count == 0 && FloorPalletCount <= 0` sobre el **resuelto**. Los dos coinciden con P-05 y con la orden PV1.
   - Comprobé la equivalencia con el diseño en `SelectiveGeometryResolver.cs:268-300`: sin niveles, la bahía queda vacía; con un solo nivel y sin larguero a piso, recibe tarima de piso.
   - La regla de Planta (`SelectivePlantaBuilder.cs:371`) queda bien descartada como fuente.
4. **INV-04 / INV-05:** los oráculos discriminan, porque «máximo» da 6, «sin vacíos» da 3 y la regla de Planta da otro resultado.
5. **D-10, no circularidad:** ninguna condición E1..E6 depende de construir el BOM.
   - E6 consume la puerta de salida, no el BOM.
   - La puerta no lee el registro de variables: `OutputBlockedReason(embed, catalog)` no recibe variables (`RackInventarioCommands.BomTotal.cs:177`). Por tanto, INV-08 es alcanzable.
6. **P-03:** una variable rota es fallo de otra métrica y no toca la pertenencia (INV-08). La regla de agregación de D-09 nunca da un parcial (INV-07).
7. **D-11:** deduplicación con `OrdinalIgnoreCase`, como en `BomTotal.cs:73-75`. El Id tanteado nunca fusiona.
8. **R1..R6 como principio:** ningún símbolo calculado en Φ2 ni antes, contexto de solo lectura, entradas `Computed` como hojas, sin reentrada. Rompe por construcción el ciclo `Rack.Altura → altura → fórmula` del mandato.
9. **No habilitar `Project.*` como *bindable*** es coherente con el mandato («no habilitar hasta demostrar»). La incoherencia está en otra parte (A63-PV1-04).
10. **D-18:** la persistencia queda cerrada y la idea del conjunto de namespaces persistibles es correcta. Hace falta porque, al añadir `rack` a `SymbolNamespaces.TryParseToken`, `PersistedBoundExpressionJson.cs:95` lo aceptaría si no hubiera guarda. INV-15 lo cubre.
11. **D-08:** el registro de *providers* está cerrado y es puro. No hay reflexión ni descubrimiento. No es un *framework* universal.
12. **D-22:** la frontera con I-64 está alineada con P-14 y con `MASTER-I63-I64-02`: cada iniciativa tiene su mecanismo, no hay *snapshot* compartido, no hay dependencia y la convergencia queda diferida.
13. **Arquetipo NEW ARCHITECTURE:** está bien elevado desde el FOUNDATION EVOLUTION del mandato. M-07 y M-01 (creador) lo exigen (LIFECYCLE §3: «ante duda, el superior»).
14. **INV-12..14:** declararlas como pruebas de no regresión verdes, escritas antes del cambio, es honesto y no fabrica un RED.

## 3. Disagreements

- **D-16 es incompleto.** El núcleo resuelve nombres y formatea **sin mirar el namespace**: lo detallo en 01 y 02.
- **E6 cita como autoridad código que vive en el Plugin.** D-25 dice que Application es puro y que el Plugin solo transporta (05).
- **E3/E4/E5 descansan sobre una proyección que contradice P-02a.** Además, la fila «Diseño ilegible → `Unavailable`» de §10 es falsa para cuatro kinds (06).
- **D-02 y D-05 dan `SymbolId` a métricas de proyecto que no son escalares ni enlazables.** El namespace `project` entra en el núcleo sin ningún consumidor (04).
- **El Anexo A omite cláusulas de ADR-0043 que V1 modifica** (03).
- **G4 incluye un entregable que solo se verifica compilando** (09).

## 4. Material risks

| Riesgo | Sobre qué |
|---|---|
| MR-1 | Reinterpretación silenciosa de `{Frentes}` o `Frentes` como parámetro calculado dentro del contexto de rack, o un `AmbiguousName` nuevo para una variable que hoy enlaza. Viola el mandato («no reinterpretar silenciosamente variables legacy») y ADR-0043 D6 reglas 5-6 |
| MR-2 | Una segunda composición de la puerta de salida, con una política de fallo distinta de la del handler: *fail-open* en el handler frente a `Undetermined` en V1 |
| MR-3 | `TotalRacks` cuenta como cotizable un Dinámico, Cantilever, Cabecera o Cama con diseño interno ilegible, y no lo detecta |
| MR-4 | El resultado depende del orden de barrido: representante, `DisplayName` y grafía del RackId. Rompe INV-19 |
| MR-5 | ID23 no tiene una vía por rack que no pase por la población entera del proyecto |
| MR-6 | El formatter produce texto que el lexer rechaza (`#{frentes}` → `InvalidQualifier`). Rompe la canonicidad P2.5 / D4 |

## 5. Required changes

### A63-PV1-01 — Resolución por nombre sin namespace

- **Elemento:** D-16.4, D-03 frente a D-05, D-14 y D-17.
- **Clasificación:** REQUIRED.
- **Razón:** D-17 mete en **una** tabla las entradas `projectVariable` y las `rack` (`Computed`). Las búsquedas por nombre del núcleo ignoran el namespace:
  - una referencia sin namespace (`Frentes` o `{Frentes}`) se resuelve con `_context.Symbols.FindByDisplayName(name.Text)` sobre **todas** las entradas;
  - `OperatorInName` usa los nombres de todas las entradas;
  - la detección de homónimos del formatter también.
- **Consecuencias concretas:**
  - si no hay variable `Frentes`, `{Frentes}` enlaza en silencio con `rack/frentes`;
  - si la hay, una expresión que hoy enlaza pasa a dar `AmbiguousName`.

  Además, el nombre visible es incoherente: D-03 dice `Frentes` y la tabla de D-05 dice `Rack.Frentes`.
- **Evidencia:**
  - `ExpressionBinder.cs:317-362`: `Resolve`, la rama sin cualificador, `:356`;
  - `SymbolTable.cs:140-148` y `:184` (`_byName` y `OperatorNames` sin namespace);
  - `ExpressionFormatter.cs:276`;
  - ADR-0043 D6 reglas 5, 6 y 8; mandato, «No reinterpretar silenciosamente variables legacy».
- **Consecuencia si no se corrige:** M-03 (cambio observable) y M-01 sobre la frontera ProjectVariable/ComputedParameter. INV-12/13 no lo detectan, porque esos contextos no llevan entradas `rack`.
- **Corrección mínima:**
  1. Congelar que las referencias sin namespace (simples, con llaves o cualificadas) solo buscan en `projectVariable`.
  2. Congelar que `palabra.miembro` solo busca en su namespace.
  3. Congelar que `OperatorInName` y los homónimos del formatter se calculan **por namespace**.
  4. Fijar si el nombre visible es `Frentes` o `Rack.Frentes`.
  5. Tratar `Rack.` sin miembro (`ExpressionSyntaxParser.cs:228`, `member == null`) como `UnknownNamespace`, o con otro diagnóstico definido.
  6. Añadir un INV en G3: en un contexto de rack con la variable `Frentes` y `rack/frentes`, `{Frentes}` enlaza a la variable, `Frentes` también, `Rack.Frentes` enlaza al calculado y no aparece `AmbiguousName` ni `OperatorInName` nuevos. Sin la variable, `{Frentes}` da `UnknownSymbol`.

### A63-PV1-02 — Formatter y canonicidad (P2.5) para `rack`

- **Elemento:** D-16.5.
- **Clasificación:** REQUIRED.
- **Razón:**
  - `FormatReference` escribe un id ausente como `Q(key)` y un homónimo como `Nombre + Q(key)`. Con la clave `frentes` eso da `#{frentes}`.
  - El lexer solo admite como cualificador una clave válida de `projectVariable`, así que ese texto da `InvalidQualifier`.
  - D-16.5 solo cubre el caso «está en la tabla».
  - La cita `ExpressionFormatter.cs:64` es en realidad `CanonicalShape.Classify`, no el formatter. Aun así, también hay que decidir cómo clasifica `=Rack.Frentes` (hoy daría `Expression`).
- **Evidencia:**
  - `ExpressionFormatter.cs:268-279`;
  - `ExpressionLexer.cs:405-436` (`InvalidQualifier` cuando la clave no es válida para `projectVariable`);
  - ADR-0043 D4, «Canonicidad… probado como propiedad (P2.5)»;
  - ADR-0043 D6, «Formatter».
- **Consecuencia:** un árbol enlazado sin referencias rotas no siempre vuelve a su texto. P2.5 deja de cumplirse sin que nadie lo declare (M-08).
- **Corrección mínima:**
  1. Congelar que una referencia `rack` siempre se escribe `Rack.<Nombre>`, nunca con `Q(key)`.
  2. Fijar la representación diagnóstica de un id `rack` ausente, que se muestra y nunca enlaza.
  3. Congelar la clasificación canónica de `Rack.X`.
  4. Añadir un INV: `Format → Parse → Bind` devuelve el mismo árbol para un árbol con `rack`, con una variable homónima presente.

### A63-PV1-03 — Delta del ADR incompleto

- **Elemento:** §23 y Anexo A, frente a M-08.
- **Clasificación:** REQUIRED.
- **Razón:** el Anexo A solo declara D5, D9, D24 y D25. V1 también modifica otras cláusulas:
  - **D6 regla 4** («`palabra.` → `UnknownNamespace`»): deja de ser cierta en el contexto de rack;
  - **el apartado Formatter de D6** y **D4/P2.5**: ver 02;
  - **D24 / V6 P25.5:** «el contexto de P6.7 es donde ID20 añadiría símbolos del rack». V1 lo **sustituye** por un contexto nuevo y prohíbe `Rack.*` en las fórmulas de propiedad;
  - **D9:** su tabla es la de tokens **persistidos**. Añadirle `rack` y `project` como no persistibles mezcla dos conceptos.
- **Evidencia:** ADR-0043 D4, D6 (reglas 4-6 y Formatter), D9 y D24 («ID20 añadiría sus símbolos en el contexto de evaluación de una propiedad de rack (P25.5)»); V6 P25.3 y P25.5; LIFECYCLE §3, M-08.
- **Consecuencia:** M-08 sin delimitar, y un Freeze que contradice en silencio una autoridad aceptada.
- **Corrección mínima:** un Anexo A con tres listas exactas, cláusula por cláusula:
  - **se conserva:** D1, D2, D3, D7, D8, D10..D23, D5 para `projectVariable`;
  - **se modifica:** D4 (dominio de P2.5), D5, D6 regla 4 y Formatter, D9 (persistidos frente a en memoria), D24;
  - **se sustituye:** P25.5, por el contexto de rack calculado, con el motivo R1.

  Hay que añadir la relación con V6 P25.3 y P25.5.

### A63-PV1-04 — Namespace `project` sin consumidor e identidad no escalar

- **Elemento:** D-02, D-04, D-05, D-16.1, D-21 e INV-11.
- **Clasificación:** REQUIRED.
- **Razón:**
  - V1 añade `project` al núcleo (enum, token, regla de clave y regla de ámbito), pero ningún contexto lo ofrece nunca (D-14, R6).
  - Las métricas `project/racksBySystem` (un mapa) y `project/totalFrentes` y `totalFrentesVacios` (valores por sistema) reciben un `SymbolId`, pero no pueden ser símbolos: el motor solo admite `double` finitos (`SymbolTable.cs:52-60`).
  - El resultado es una identidad que no cumple el contrato de símbolo, y un contrato del núcleo (M-05) que ninguna prueba ejercita: INV-11 solo prueba `rack`.
  - D-21 promete un `SymbolId` en cada `MetricValue`, y eso no tiene sentido para un mapa.
- **Evidencia:** Proposal D-05 (filas `racksBySystem`, «mapa kind → conteo», y «por sistema»), D-14, R6 e INV-11; `SymbolTable.FromLiteral`.
- **Consecuencia:** la identidad del ComputedParameter (punto 7 del encargo) queda incoherente, y en el núcleo entra código congelado sin prueba.
- **Corrección mínima, una de dos:**
  - **(a) Recomendada:**
    1. V1 solo activa `rack` en el núcleo.
    2. `project` sigue reservado, con `ReservedName`/`UnknownNamespace` intactos.
    3. Las métricas de proyecto se identifican con un `MetricId` del catálogo, que no es un `SymbolId`. D-02 debe distinguir «identidad de símbolo» de «identidad de métrica del resumen».
  - **(b)** Dar a cada métrica de proyecto un `SymbolId` escalar por sistema, y probar el namespace `project` y su regla de ámbito con un INV en G3.

### A63-PV1-05 — E6: la autoridad de la puerta vive en el Plugin

- **Elemento:** D-10 (E6), D-25 y R-01.
- **Clasificación:** REQUIRED.
- **Razón:**
  - Application solo tiene `RackBomOutputGate.For(PushBackSystem)`.
  - La **composición** de la puerta está en `IRackKindHandler.OutputBlockedReason`, en el Plugin. Esa composición decide qué kind tiene puerta, cómo se deserializa, cómo se resuelve y la política de fallo.
  - En Push Back esa política es *fail-open*: si el diseño es ilegible o hay una excepción, devuelve `null` y el rack pasa.
  - V1 propone `Undetermined(ResolveFailed / CatalogUnavailable)`, una política distinta.
  - Si Application la reimplementa, aparece una **segunda autoridad** de la puerta (M-01). Si llama al handler, deja de ser puro (D-25).
- **Evidencia:** `PushBackKindHandler.cs:56-71`, en concreto `catch (System.Exception) { return null; }`; `IRackKindHandler.cs:62`; los otros cinco handlers devuelven `=> null`; `RackBomOutputGate.cs:49-59`; Proposal §19 («Application 100 % puro»).
- **Consecuencia:** el mandato pide justo evitar dos autoridades («Evitar SummaryCounter + BOMCounter»).
- **Corrección mínima:**
  1. Congelar una **única** función de Application de la forma «veredicto de salida de un rack: (kind, diseño, catálogo) → Allow | Deny(motivo) | Indeterminado(razón)».
  2. Que la consuman tanto el handler, sin cambiar su comportamiento actual, como I-63.
  3. Declarar explícitamente la diferencia de política (el handler sigue *fail-open*; I-63 da `Undetermined`) y clasificar ese punto en M-01..M-04.
  4. Añadir un INV de conformidad: el handler y I-63 dan el mismo veredicto en los casos Allow y Deny.

### A63-PV1-06 — Proyección de entrada de E3/E4 y E5 inexistente en kinds no selectivos

- **Elemento:** D-10 (E1, E3, E4, nota E5) y la tabla de §10.
- **Clasificación:** REQUIRED.
- **Razón:**
  1. `BomAuthoredAuthority.Resolve` recibe `ProjectVariableScanEntry`. La proyección vigente convierte un sobre **sin `Kind`** en `UnreadableEnvelope`, sin RackId.
     - Si V1 la usa, un rack sin kind da cobertura no acreditada (E1) en lugar de quedar `Excluded(KindAbsent)` (P-02a).
     - Además, la proyección compara `selective` con `OrdinalIgnoreCase`, mientras que E3 usa `Ordinal` (`KindDispatch`).
  2. En los kinds no selectivos, E4 aprueba la primera vista **sin leer el diseño interno**, y E6 solo existe para Push Back.
     - Un Dinámico, Cantilever, Cabecera o Cama con diseño ilegible queda **incluido**.
     - Eso contradice la fila «Diseño ilegible → `Unavailable`» de §10 y la nota sobre E5.
     - RACKBOMTOTAL lo salta con aviso: el rack **no** es cotizable (P-01).
- **Evidencia:** `ProjectVariableScanProjection.cs:37-47`; `BomAuthoredAuthority.cs` (`if (!siblings[0].IsSelective) return Success(null, …)`); `BomTotal.cs:196-215`; `KindDispatch.cs` (`_ordinal`).
- **Consecuencia:** el oráculo de INV-03 sería ciego o contradictorio, y `TotalRacks` cambiaría según la vía de implementación.
- **Corrección mínima:**
  1. Congelar cómo se construye la entrada de E1/E3. Debe distinguir «sin Id» de «sin Kind» y aplicar un comparador de kind único (`Ordinal`).
  2. Congelar cómo se construye después la entrada de E4.
  3. Convertir E5 en una condición real por kind: un lector puro de Application para el diseño del kind que, si falla, da `Undetermined(DesignUnreadable)`. Alternativa: corregir la tabla y declarar, con su clasificación según P-01/P-03, que en los kinds no selectivos la legibilidad interna no es una condición. Si eso cambia P-01, la decisión es del Owner.
  4. Añadir un INV por kind con el diseño interno ilegible.

### A63-PV1-07 — Conjunto de hermanas y orden canónico

- **Elemento:** D-10 (E3, E4), D-11, D-20, D-21 e INV-19.
- **Clasificación:** REQUIRED.
- **Razón:**
  - RACKBOMTOTAL pasa a la autoridad **todas** las hermanas legibles del RackId, **también las no colocadas**. V1 no dice cuál es su conjunto: una definición no colocada y divergente cambia `Included` por `Excluded(SiblingsDivergent)`.
  - El representante es «la primera en orden de barrido» (`BomAuthorityResult.RepresentativeDefinitionId`) y `DisplayName` es «el primer nombre no vacío».
  - Un RackId que aparece con dos grafías (`Rack-A` y `rack-a`) no tiene grafía canónica.
  - Las tres cosas dependen del orden de entrada, y eso contradice INV-19 («otro orden de entrada → el mismo `ProjectSummary`»).
- **Evidencia:** `BomTotal.cs:96-104, 150`; `BomAuthoredAuthority.cs` («the FIRST in sweep order»); Proposal D-20 y D-21; INV-19.
- **Consecuencia:** el *provenance* y el resumen no son deterministas, y el [INFERENCE] de §10 queda sin base.
- **Corrección mínima:**
  1. Congelar el conjunto de hermanas: todas las legibles del RackId, colocadas o no, igual que RACKBOMTOTAL. Si se elige otra cosa, justificarlo.
  2. Congelar un orden canónico **antes** de E3/E4, por ejemplo `DefinitionId` con comparación `Ordinal`.
  3. Congelar la grafía canónica del RackId del grupo.
  4. Ampliar INV-19 para cubrir el representante, el `DisplayName` y la grafía, y añadir un INV con una hermana no colocada y divergente.

### A63-PV1-08 — Vía por rack para ID23 sin resolver el proyecto entero

- **Elemento:** D-17, D-08, D-24, R6 y el criterio de éxito 10.
- **Clasificación:** REQUIRED.
- **Razón:**
  - D-17 parte de «un rack ya evaluado (Φ3)», pero la única producción definida es el orquestador del resumen, que captura y clasifica **todo** el dibujo.
  - ID23 necesita `Rack.Frentes` de **un** rack. La autoridad E4 solo necesita las hermanas de ese RackId; no necesita población.
  - Tampoco está definido qué devuelve el contexto cuando la métrica es `NotApplicable` o `NotSupported`: D-17 solo habla de `Unavailable(razón)`, y ese estado tiene razones cerradas que no las incluyen. Por ejemplo, `Guías.Quantity = Rack.Frentes * 2` sobre un Push Back.
- **Evidencia:** Proposal D-17 («se construye a partir de un rack ya evaluado»), D-09 (razones cerradas) y D-24; mandato, «ID20 debe dejar una vía estable… dentro de un contexto de un rack concreto» y «eager whole-project resolution».
- **Consecuencia:** una vía para ID23 inestable, o que obliga a resolver el proyecto entero, y un resultado ambiguo para los kinds sin la métrica.
- **Corrección mínima:**
  1. Congelar una entrada por rack: (hermanas de un RackId + registro + catálogo) → `RackMetricResults` → contexto de rack. No debe necesitar captura del proyecto ni evaluar la población.
  2. Congelar el resultado del contexto por estado: `Available` → evaluar; `NotApplicable` y `NotSupported` → un resultado tipado distinto de `Unavailable` y de `BrokenReference`.
  3. Añadir un INV con contador: una sola resolución, sin evaluar la población, y el resultado tipado para Push Back y Cabecera.

### A63-PV1-09 — G4: entregable verificado solo compilando

- **Elemento:** D-25, G4 y R-05.
- **Clasificación:** REQUIRED.
- **Razón:**
  - El adaptador de captura del Plugin decide en la práctica E1/E2: qué se cuenta como referencia directa activa, ilegible o sin Id.
  - No tiene consumidor ni prueba. G4 lo da por cerrado porque «compila».
  - LIFECYCLE §7 exige un resultado observado por pruebas y RED→GREEN por comportamiento nuevo, y prohíbe gates de capa.
- **Evidencia:** Proposal §19 («lo compila el job… sin consumidor productivo en V1»), §21 (G4: «el adaptador del Plugin compila») y R-05; LIFECYCLE §7, puntos 1 y 2.
- **Consecuencia:** un plan de gates inconsistente con la autoridad, y lógica de pertenencia sin oráculo.
- **Corrección mínima, una de dos:**
  - **(a)** Sacar el adaptador de V1. El contrato de captura sería un *snapshot* de Application que construyen las pruebas, y el primer consumidor añade el adaptador con su OV.
  - **(b)** Llevar toda la proyección de Plugin a Application como una función pura con pruebas. En el Plugin solo quedarían llamadas ya ejercitadas (`ScanEnvelopes`, `ProjectVariablesRegistry.Read`, `LoadCatalog`), y G4 se declararía como tal.

### A63-PV1-10 — `BySystem`: conjunto de claves y estados explícitos

- **Elemento:** D-12, D-20 e INV-06.
- **Clasificación:** REQUIRED.
- **Razón:**
  - `TotalFrentes?` y `TotalFrentesVacios?` son opcionales. Para Push Back (NotSupported) o Cabecera (NotApplicable), la ausencia de campo no es un estado explícito (criterio de éxito 7).
  - No está fijado si `BySystem` lleva los seis *tokens* (con 0 los que no tienen racks) o solo los presentes. Eso cambia lo que ve un consumidor como ID30 y el oráculo de INV-06.
- **Evidencia:** el diagrama de D-20; mandato, criterio de éxito 7, «unavailable/N/A states are explicit».
- **Consecuencia:** un modelo neutral ambiguo para quien lo consuma.
- **Corrección mínima:**
  1. Congelar el conjunto de claves (recomiendo los seis *tokens*, siempre presentes).
  2. Congelar que cada métrica de proyecto del catálogo tiene, por sistema, un `MetricValue` con estado explícito (`NotSupported` o `NotApplicable` incluidos).
  3. Añadir un INV.

## 6. Optional improvements

- **O-1:** declarar cómo se corresponde el vocabulario del mandato con el de V1: «Supported» → `Available` o `Unavailable`, y «NeedsContract» → `NotSupported`. También separar el estado de diseño (la matriz de D-06) del estado de ejecución (el `MetricValue`).
- **O-2:** declarar que la coherencia de kind entre hermanas (E3, la mezcla) es una regla **propia de I-63**. Hoy RACKBOMTOTAL toma el kind de `siblings[0]` sin comprobarla, así que no es una «autoridad existente».
- **O-3:** fijar que el contexto de rack calculado vive **fuera** de `RackCad.Application.Expressions` (guarda de P26.1). Fijar también que usa el **mismo documento de registro** que la Φ2 de ese rack.
- **O-4:** una petición del resumen que diga qué métricas o qué fase máxima necesita. Así, un consumidor de `TotalRacks` no dispara Φ2/Φ3 en todos los Selectivos (encargo, punto 14).
- **O-5:** dos tablas en lugar de una: la de *tokens* persistidos de D9, sin cambios, y la de namespaces en memoria, separada. Es preferible a meter tokens no persistibles en la tabla persistida y protegerla con una guarda.
- **O-6:** cerrar el conjunto de identificadores de autoridad fuente de D-21 (por ejemplo, `selective.resolved.fondo0`) como datos estables, con prueba.
- **O-7:** INV-09 debería tener un RED observable: un *fixture* que viola la guarda y la prueba lo detecta.

## 7. Materiality / archetype assessment

| ID | V1 | Evaluación |
|---|---|---|
| M-01 | Activado (creador) | De acuerdo. Además hay **riesgo de segunda autoridad en E6** (05): o se resuelve, o se declara |
| M-02 | No activado | De acuerdo, **condicionado** a INV-15 y a O-5 (o a que la guarda se pruebe en los dos sentidos) |
| M-03 | No activado | **Solo** si se corrige 01. Sin esa corrección, `{X}` cambia de significado en el contexto nuevo y en la colisión con homónimos |
| M-04 | Activado | De acuerdo. La diferencia de política de fallo de E6 (05) entra aquí |
| M-05 | Activado | De acuerdo. El alcance crece con 01 y 02 (búsqueda por namespace y formatter) |
| M-06 / M-07 | Activados | De acuerdo. El registro está cerrado y no es un *framework* |
| M-08 | Activado | De acuerdo, pero el **alcance declarado es incompleto** (03) |

**Arquetipo:** NEW ARCHITECTURE, confirmado. Es una elevación legítima sobre el FOUNDATION EVOLUTION del mandato (LIFECYCLE §3).

**Owner Validation:** **confirmo la propuesta de NOT APPLICABLE**. V1 no añade comandos, ventanas ni cambios de dibujo, y la decisión final corresponde al Coordinator en el Freeze. La confirmación es firme si A63-PV1-09 se resuelve con la opción (a) o la (b) sin comportamiento en el host. Si el adaptador del Plugin entrara con algún efecto observable, habría que reabrir la OV.

## 8. Freeze readiness

**NOT READY FOR FREEZE.** Hay 10 REQUIRED abiertos. Este veredicto no crea ningún Freeze.

## 9. Consensus status

**CHANGES REQUIRED**, con los REQUIRED A63-PV1-01 a A63-PV1-10.

Ninguno exige una decisión del Owner, salvo una rama posible de 06. Si se decidiera que en los kinds no selectivos la legibilidad interna **no** condiciona la pertenencia, cambiaría lo que P-01 entiende por «cotizable», y esa decisión volvería al Owner.

El veredicto vale **exclusivamente** para:

- commit `772ac242430084918d319a7683ce615daec39452`;
- ruta `docs/initiatives/I-63-proposal-v1.md`;
- blob `48a68307be3f046c20c2405252dc8af3ad050342`.
```
