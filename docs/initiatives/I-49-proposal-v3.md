# I-49 — Proposal V3: motor de expresiones paramétricas (Expression Engine, ID22B)

> # ⚠ COORDINATOR PROPOSAL V3 — ARCHITECT REVIEW V2 RECONCILED — NOT CONSENSUS
>
> # ⛔ Implementation remains BLOCKED
>
> ```text
> Proposal Version = V3
> Coordinator      = AGREED WITH V3
> Architect        = PENDING RE-REVIEW
> Open A           = CLOSED
> Open B           = CLOSED
> Schema           = V-0
> Implementation   = BLOCKED
> ADR              = REQUIRED, not yet written
> ```
>
> Gate **G2C**. V3 reconcilia la **Architect Review de V2** (`Architect = CHANGES REQUIRED`: 1 BLOCKER, 3 HIGH,
> 6 MEDIUM, 8 LOW) sobre la base exacta de V2. Los dieciocho hallazgos quedan reconciliados en §0.
>
> - El Coordinador **acepta** A-01, A-02 y A-04 a A-18.
> - **A-03 se acepta en principio y se modifica**: el remedio vinculante está en §0.2.
> - OPEN A, OPEN B, la versión de schema y las dos operaciones revisadas quedan **cerradas** como decisiones
>   de V3.
>
> ```text
> Base V2          = 9aef7d04d9e65b4225af2cb36750ee636a6689d5   (I-49-proposal-v2.md; CI 34721563723 success)
> Proposal V1      = b15e40a076d7157ce8ca73537af9659049bf8576   (historial; no aceptada para revisión)
> Discovery G1     = 4cf02b167f183fb66d93988c9843296838277dc4   (I-49-discovery.md)
> Código auditado  = a4d88f18a1f42263d366c44dc05dd18a6786f152   (base de la rama; la rama no contiene producción)
> main observado   = 46fcac2b071929d2bd5b07aa28373941417f74a8   (merge de I-51; no cambia ningún archivo que V3 cite)
> Override vigente = OWNER_OVERRIDE_I49_I50_PARALLEL   (docs/automation/decisions/I-49.md)
> I-50 observado   = 7a9335cf866b871d855df711c398a98809cef775   (G5 (B) con producción; §12)
> I-51             = INTEGRADA en main (46fcac2); rama retirada; ningún archivo del plan de I-49
> Estado de gates  = G0 · G1 HECHAS · V1 historial · V2 CHANGES REQUIRED · G2C V3 EN CURSO · G3 PENDIENTE
> ```

---

## 0. V2 → Architect Review → Coordinator Reconciliation

### 0.1 Tabla de reconciliación

| # | Severidad (Architect) | Regla V2 | Decisión del Architect | Decisión del Coordinador | Regla V3 |
|---|---|---|---|---|---|
| **A-01** | BLOCKER | V2 §5.4 (cuarto punto), P24.6, P21.10, P14.8 | Retirar `FatalUnevaluable` como estado no reparable: un fallo semántico de una fuente es reparable por rack y solo lo estructural bloquea | **ACEPTADA** | P24.6, P21.10, §5, §7.2 |
| **A-02** | HIGH | P20.4 | `UnlinkAllAndDelete` no convierte fórmulas en literales: bloquea si una fórmula referencia X | **ACEPTADA**, con el contrato final de §7.1 | P20.4, §7.1 |
| **A-03** | HIGH | P21.7, P17.7 | Sin dominio en variables; dominio de la propiedad sobre el efectivo de toda fuente no literal | **ACEPTADA EN PRINCIPIO y MODIFICADA**: tres fronteras; el motor no tiene dominio, la variable cumple en su raíz el contrato de su `VariableType` con cualquier sintaxis, y la propiedad aplica su dominio a toda fuente (§0.2) | P5.5, P8.10, P21.7, P23.6, P24.5 |
| **A-04** | HIGH | P22.7 | Enlazar contra el snapshot del workspace; el intent lleva ids; preflight y commit validan ids | **ACEPTADA** | P7.7, P22.7 |
| **A-05** | MEDIUM | P21.6 | La re-lectura de commit compara el conjunto leído del plan, no solo `I` | **ACEPTADA**: `PlanReadSet` pasa a ser contractual | P21.6 |
| **A-06** | MEDIUM | P5.1 | Quitar `ConsumerType` del núcleo | **ACEPTADA** | P5.1, P8.9, P26.1 |
| **A-07** | MEDIUM | P2.8, P17.6 | Una forma no canónica es error semántico, no estructural | **ACEPTADA** | P2.8, P17.6–P17.7, §5 |
| **A-08** | MEDIUM | P17.6 | Campos desconocidos en `expression` = error estructural; regla de evolución | **ACEPTADA con alcance precisado**: solo dentro de los payloads de Expression; la política histórica de `ExtensionData` fuera de ellos no cambia | P17.6, P17.12, §6.2–§6.3 |
| **A-09** | MEDIUM | P23.15 | No fijar `"2.0"`: calcular la versión con `SelectiveDesignSchema.ResolveWriteVersion` | **ACEPTADA** como preservación de invariante, no como deuda lateral | P23.15 |
| **A-10** | MEDIUM | P24.6, P21.10 | Confirmación de reparación consciente de la fuente | **ACEPTADA**, en la misma operación `RepairBrokenRack` | §7.2, P22.9 |
| **A-11** | LOW | P18.4–P18.5 | Justificar V-0 por el comportamiento de los lectores, no por los archivos tocados | **ACEPTADA** | P18.3–P18.7, §6 |
| **A-12** | LOW | P21.9 | P21.9 contradecía P23.15: materializar se limita a `UnlinkAllAndDelete` y a la exportación; el reconciliador escribe el literal tecleado | **ACEPTADA** | P21.9 |
| **A-13** | LOW | P9.7 | `StructuralSectionUnits` toma sus factores de la autoridad neutral; `FootInches` y `CommercialFoot` no cambian | **ACEPTADA y AMPLIADA**: una sola autoridad real para `25.4` y `12`. También las demás constantes de «pulgadas por pie» pasan a ser alias, con el procedimiento de colisión donde toca a I-50 | P9.6–P9.7 |
| **A-14** | LOW | P16.4, P23.10 | `RegistryEvaluation` es el único dueño de los valores evaluados | **ACEPTADA** | P14.9, P16.4, P23.10 |
| **A-15** | LOW | P23.15 | Precisar «un fallo no muta nada», o clonar | **ACEPTADA y ELEVADA a MEDIUM práctico**: estado aislado obligatorio; se distinguen «no hubo persistencia» y «no hubo mutación en memoria» | P23.15–P23.16 |
| **A-16** | LOW | P1.9 | Límites normativos por nodos, anidamiento y argumentos; texto de entrada ≥ 4000 | **ACEPTADA** | P1.9 |
| **A-17** | LOW | P17.4, P17.10 | Tokens persistidos en tablas cerradas; nunca nombres C# | **ACEPTADA** | P2.4, P10.9, P17.10 |
| **A-18** | LOW | P2.8, P22.3 | Un literal canónico obedece la regla de literales de su superficie | **ACEPTADA** | P2.8, P22.3, P23.6 |

**Resultado**: 18 de 18 reconciliados. Ninguno queda abierto.

### 0.2 A-03 — remedio modificado (VINCULANTE)

**No hay dominio físico dentro del motor de expresiones.** La validez se reparte en tres fronteras:

```text
Expression Engine
  → cualquier double finito; sin dominio físico

ProjectVariable adapter
  → aplica el contrato vigente de su VariableType al RESULTADO RAÍZ

Property consumer
  → aplica el dominio de esa propiedad al valor efectivo
```

- **El motor sigue siendo adimensional.** Los valores intermedios de una expresión pueden ser negativos o nulos.
  `VariableType.Length` **no** gobierna ninguna álgebra.
- **Una variable definida por expresión no puede saltarse las reglas de validez que tendría definida por
  literal.** La validez no depende de la sintaxis. Por eso **no se adopta** la propuesta del Architect de admitir
  `A = -2` como expresión cuando el mismo `A = -2` literal sería inválido bajo el contrato vigente.
- **Contrato vigente de `Length`** (hechos):
  - al leer, el store y la acreditación exigen un valor **finito** (Discovery §11.4);
  - al escribir desde el producto, la ventana de RACKVARIABLES exige **`> 0`** («El valor tiene que ser un
    número mayor que cero», `RackProjectVariablesWindow.xaml.cs:215-224`; Discovery §10.3, §11.4).
- **Cómo lo aplica V3** (P8.10):
  - **al escribir**, igual para literal y expresión:
    1. la raíz de la variable mutada (`Create`, `ChangeDefinition`) cumple `> 0`;
    2. toda raíz del cierre `I` que cumplía antes cumple después, con la misma regla R2 que los fallos semánticos
       (§5);
    3. una raíz que ya lo incumplía, solo alcanzable por edición externa y con su definición intacta, no bloquea
       por sí sola y nunca llega a una propiedad fuera de dominio.

    La comprobación de la ventana se conserva como aviso temprano.
  - **al leer**: el contrato histórico (finito) se aplica **igual** a las dos sintaxis. Ningún valor persistido
    se re-valida con más rigor por el hecho de ser expresión, ni ningún literal con más rigor que hoy.
- **Variables escalares o con signo**: si hacen falta en el futuro, exigen **otro `VariableType` u otra
  iniciativa**. I-49 no las introduce de forma implícita.
- **Propiedades** (P24.5): el dominio de la propiedad se aplica al valor efectivo de las tres fuentes,
  preservando lo histórico donde aplica:

  | Fuente | Al escribir (editor, reconciliación) | Al resolver lo persistido (lectura, preflight, BOM) |
  |---|---|---|
  | `Literal` | **Ruta histórica**: la ventana rechaza `< 0` al construir el diseño (`RackSelectiveWindow.xaml.cs:2481-2505`); un literal canónico escrito con `=` sigue la misma regla (A-18) | **Heredado**: no se re-valida |
  | `ProjectVariableReference` | Dominio sobre el efectivo | Dominio sobre el efectivo: `OutOfRange`. **Endurecimiento declarado**: una referencia a una variable negativa, solo alcanzable por edición externa, deja de resolver y falla cerrada; su geometría existente no se toca |
  | `Expression` | Dominio sobre el efectivo: aviso en la sesión y autoridad en el reconciliador | Dominio sobre el efectivo: `OutOfRange` |

- **Ejemplos de la regla:**

  | Caso | Resultado V3 |
  |---|---|
  | `Ajuste = -2` escrito como literal | Rechazado: contrato de `Length` |
  | `Ajuste = AlturaA - AlturaB` con raíz `-2` | Rechazado: mismo contrato en la raíz |
  | `AlturaFinal = Base + (AlturaA - AlturaB)` con intermedio `-2` y raíz `> 0` | Válido: el motor no tiene dominio |
  | `palletTolerance = A - 10` con efectivo `-3` | `OutOfRange` del consumidor |
  | Cambiar `A` de modo que una variable dependiente de `I`, que cumplía, quede con raíz `≤ 0` | Preflight rechazado: `OutOfRange` de esa variable |
  | Raíz `≤ 0` persistida por edición externa, literal o expresión | Se lee como hoy en las dos sintaxis; con su definición intacta no bloquea por sí sola, y nunca llega a una propiedad fuera de dominio |

### 0.3 Decisiones cerradas en V3

| Tema | Estado en V2 | Estado en V3 | Dónde |
|---|---|---|---|
| OPEN A — sintaxis de homónimos | OPEN FOR ARCHITECT | **CLOSED**: sintaxis del Architect, con `#GUID` sin nombre reservado al formatter de referencias rotas | §4 |
| OPEN B — estructural frente a semántico | OPEN FOR ARCHITECT | **CLOSED**: clasificación y reglas R1–R3 del Architect | §5 |
| Versión de schema | ARCHITECT REVIEW REQUIRED | **CLOSED: V-0**, con cuatro condiciones obligatorias | §6 |
| `UnlinkAllAndDelete` con expresiones | Materializaba fórmulas | **CLOSED**: bloquea | §7.1 |
| `RepairBrokenRack` con expresiones | No reparable ante fallo semántico | **CLOSED**: una sola operación con conjunto reparable ampliado y confirmación consciente de la fuente | §7.2 |

### 0.4 Qué conserva V3 de V2

Las dos superficies de expresión con el mismo control; el modelo híbrido con `=X` canónico; el núcleo neutral; ids
en todo lo persistido; la gramática aritmética con la regla de la coma; las unidades `[mm]`, `[in]` y `[ft]` como
conversión numérica; el motor adimensional; `MIN`, `MAX` y `ABS`, con `ROUND`, `CEILING` y `FLOOR` diferidas; el grafo
derivado; la detección iterativa de ciclos; el orden determinista; `Rename` registry-only; un plan, una transacción,
un commit y un `Regen`; el literal congelado; las garantías de C4, Enter, Escape y LostFocus; la preservación para
ID20, ID23, ID28 e ID29; ADR REQUIRED sin número.

---

## 1. Cómo leer esta Proposal

### 1.1 Qué es y qué no es

- **Es** la Proposal V3 del Coordinador, con `Coordinator = AGREED WITH V3`, sometida a **re-revisión** del
  Arquitecto. Cada regla está **propuesta** hasta que haya consenso.
- **No es** consenso, **no** autoriza implementación, **no** es un ADR y **no** modifica V1, V2 ni el Discovery.
  La compuerta **NO IMPLEMENTATION BEFORE CONSENSUS** del contrato (§12) sigue vigente.
- **Parte de hechos**: toda afirmación sobre el código remite al [Discovery G1](I-49-discovery.md) o a una ruta
  verificada en `a4d88f1`. Ninguno de los archivos citados cambió en `main` hasta `46fcac2`. Lo que era INFERENCE
  sigue siéndolo.

### 1.2 Convenciones

| Marca | Significado |
|---|---|
| **Pn.m** | Regla propuesta *m* del punto *n* de §3. Solo sería vinculante tras G3 |
| **[V3 · A-xx]** | Regla nueva o modificada en V3 para reconciliar el hallazgo A-xx de la Architect Review de V2 |
| **[V3 · OPEN A]**, **[V3 · OPEN B]** | Regla que cierra la cuestión correspondiente |
| **DR-n** | Decisión rectora (§2) |
| **Discovery §x** | Hecho verificado en G1; remite a la sección *x* de [`I-49-discovery.md`](I-49-discovery.md) @ `4cf02b1` |
| **REQUISITO DEL OWNER** | Requisito del contrato del Owner fijado en el encargo de G2A.1 |
| **OWNER INPUT** | Aclaración del Owner recibida en G1 (ID20, ID23, ID28/ID29) |
| **INFERENCE** | Estimación o deducción, no hecho |
| Nombres de tipos y miembros | **Ilustrativos**: el contrato es el comportamiento |

### 1.3 Entradas vinculantes

- **Contrato de I-49** — [`I-49-motor-expresiones-parametricas.md`](I-49-motor-expresiones-parametricas.md): las
  invariantes de I-47/I-48 no se reabren sin ADR y decisión del Owner (§3.2); quedan fuera Custom BOM, ID20
  productivo e ID21 (§4). Su §11.2 prevé salir de `ProjectVariable.Definition` con «ADR nuevo aceptado por el Owner»:
  ese ADR es obligatorio (§9) y la alineación del texto del contrato queda para G4.
- **[ADR-0034](../adr/0034-project-variables-autoridad-drawing-level.md)**, aceptado e inmutable.
- **REQUISITOS DEL OWNER** (encargo de G2A.1), sin cambio respecto de V2.
- **Architect Review de V2** (`CHANGES REQUIRED`) y **decisiones del Coordinador de G2C**, reconciliadas en §0.
- **OWNER INPUT de G1** (Discovery §1): ID20, ID23 e ID28/ID29.
- **`OWNER_OVERRIDE_I49_I50_PARALLEL`** — [`decisions/I-49.md`](../automation/decisions/I-49.md), con su procedimiento
  de colisión (§8 de ese registro).

---

## 2. Decisiones rectoras y arquitectura en una página

### 2.1 Decisiones rectoras

- **DR-1 — Dos superficies productivas de expresión** (REQUISITO DEL OWNER): la **definición de variable**
  (`literal | expression`) y la **fuente de una propiedad vinculable** en el **mismo** `LinkedPropertyEditor`
  (`Literal | ProjectVariableReference(VariableId) | Expression(BoundExpression)`). La referencia directa se
  conserva y `=X` tiene una sola representación persistida. ID21 sigue fuera.
- **DR-2 — Núcleo neutral** `RackCad.Application.Expressions`: sin dependencia de Project Variables, persistencia,
  sistemas, BOM, catálogos, `StructuralSections`, Domain, UI, Plugin ni AutoCAD, y **sin ningún tipo de
  `ProjectVariables`** en sus entradas **[V3 · A-06]**. Las conversiones vienen de **una** autoridad neutral de
  unidades.
- **DR-3 — Un solo dueño de los valores evaluados**: `RegistryEvaluation`, una vez por snapshot acreditado. Las
  expresiones de propiedad se evalúan en el resolver único, sobre esos valores. UI y sesión solo transportan
  valores para presentación **[V3 · A-14]**.
- **DR-4 — Identidad por id y una forma canónica por significado.** El nombre solo interviene al escribir y al
  mostrar, con la sintaxis de §4 **[V3 · OPEN A]**.
- **DR-5 — Motor adimensional con tres fronteras de validez** **[V3 · A-03 modificada]**: el motor acepta
  cualquier `double` finito; el adaptador de Project Variables aplica a la raíz el contrato de su `VariableType`;
  el consumidor aplica el dominio de su propiedad.
- **DR-6 — Fail-closed tipado** (ADR-0034 §8): cada fallo es un resultado tipado, un plan fallido es vacío, y no
  hay fallback a literal, a cero ni a un valor anterior.
- **DR-7 — Estructural ≠ semántico** **[V3 · OPEN B]**: lo que la build no entiende no se lee ni se repara; lo que
  entiende pero no puede evaluar se diagnostica y se corrige, sin fallback.
- **DR-8 — Sin efectos parciales** **[V3 · A-05, A-15]**: el commit valida su `PlanReadSet`, y ninguna transición
  deja mutado en memoria el estado de quien la llama.

### 2.2 Tuberías

```text
Lectura y consumo
NOD RACKCAD_PROJECT (Plugin)          único lector/escritor físico                         (sin cambio)
  → ProjectVariablesStore             legibilidad ESTRUCTURAL: payload de Expression cerrado (P17)
  → UsableProjectVariablesRegistry    identidad: VariableId único                          (sin cambio)
  → RegistryEvaluation                grafo, ciclos, orden, valores y fallos semánticos    (P6–P16)
  → SelectiveEffectiveDesignResolver  resolver único: referencias → valor evaluado;
                                      expresiones → evaluadas aquí; dominio del consumidor (P14.8, P24)
  → geometría / BOM / preview         solo efectivo; nada roto se dibuja                   (§5)
```

```text
Escritura explícita — RACKVARIABLES (definiciones de variable)
texto tecleado → parse → bind contra el snapshot del workspace → validate → evaluate → canonicalizar
  → intent con la forma enlazada (ids, nunca texto)       (P7.7, P22.7)
  → preflight sobre la MISMA lectura: R1–R3, contrato raíz y dominio   (P21.2–P21.3, P8.10)
  → plan + PlanReadSet → commit: re-lectura, re-acreditación, ids y PlanReadSet → escribir o ABORT   (P21.6)
```

```text
Escritura explícita — RACKEDITAR (fuentes de propiedad)
texto tecleado → parse → bind contra el snapshot del comando → validate → evaluate (aviso) → canonicalizar
  → sesión comprometida → frontera C4 → estados finales      (P23.6, P23.9)
  → reconciliador sobre estado aislado: árbol por ids, un resolve del rack, versión calculada
  → escribir o fallar sin persistencia ni mutación en memoria   (P23.15–P23.16)
```

```text
RackCad.Application.Expressions  (núcleo neutral, DR-2)
  texto          → lexer/parser             → sintaxis con posiciones                  (P1, P2, §4)
  sintaxis       → binder + SymbolResolver  → BoundExpression (ids, funciones, unidades) (P7, P9, P10)
  BoundExpression→ comprobación semántica   → existencia, aridad, ámbito, forma canónica  (P7.5)
  BoundExpression→ dependencias → grafo → ciclos → orden                                (P11–P14)
  orden + tabla  → evaluador (double)       → EvaluationResult + traza opt-in           (P14, P16)
  BoundExpression→ formatter                → texto editable mínimo e inequívoco        (P4, §4)
```

### 2.3 Lo que V3 extiende o revisa

| Invariante o contrato vigente | Extensión o revisión | Punto |
|---|---|---|
| ADR-0034 §3: definición **literal** | `literal` **o** `expression`; `VariableType` sigue siendo solo `Length` | P2.6, P5.5 |
| ADR-0034 §4: `Literal(T)` o `ProjectVariableReference(VariableId)` | Tercer caso `Expression(BoundExpression)` | P2.7, P17.3, P24 |
| ADR-0034 §15: `Definition` para ID22B; `PropertyValue<T>` para ID21 | ID22B extiende también la fuente de propiedad; ID21 sigue fuera | §9 |
| ADR-0034 §7: profundidad 1 | Cierre transitivo; un plan, una transacción, un commit y un `Regen`; `PlanReadSet` | P21 |
| ADR-0034 §11: `Delete` bloqueado con consumidores; reparación de bindings rotos | `Delete` y `UnlinkAllAndDelete` también bloqueados por fórmulas; `RepairBrokenRack` repara también fallos semánticos | P20, §7 |
| ADR-0034 §13, C4-8 y C4-9 | Kinds `expression` fail-closed en builds anteriores; **V-0**; payload de Expression cerrado | §6 |
| Decisiones de I-48 §4 (V8-R01) | La re-lectura de commit re-evalúa el `PlanReadSet` | P21.6 |
| Contrato del editor de I-48 (HANDOFF §1; pines G20) | `=` abre un borrador de expresión; `CASO_6` y `CASO_8` cambian; se conservan borrador ≠ comprometido, Escape, LostFocus, C4, no auto-selección y desambiguador | P23.12 |
| Regla `> 0` de variables (hoy en la ventana) | La aplica también el adaptador, a la raíz, en las dos sintaxis | P8.10 |
| Dominio de propiedades (hoy en la ventana) | Se aplica además al efectivo de referencias y expresiones al resolver | P24.5 |
| Contrato de I-49 §1, §11.2 y §12 | La capacidad sale de `Definition` por decisión del Owner, por la vía de ADR que el contrato prevé | §9, §13 |

**No se toca**: autoridad y nodo del registro, identidad `VariableId`, authored ≠ effective, congelado del literal
comprometido (20.13), autoridad multi-vista, identidad ambigua fail-closed, mapping único del `Type`, reparación
atómica por rack y los censos de comandos y ventanas.

---

## 3. Los 30 puntos

### 3.0 Tabla de control

| # | Punto | Propuesta en una línea | Cambios V3 |
|---|---|---|---|
| 1 | grammar | Aritmética, llamadas, unidades tras números y referencias `Nombre`, `{Nombre}` y `…#GUID` | OPEN A · A-16 |
| 2 | AST/model | Sintaxis, `BoundExpression` y resultado; formas canónicas; forma no canónica = semántica | A-07 · A-17 · A-18 |
| 3 | identity | `VariableId` intacto; `SymbolId = (namespace, key)`; cualificador de GUID completo | OPEN A |
| 4 | source text | Formatter único con forma mínima inequívoca; `#GUID` para referencias rotas | OPEN A |
| 5 | symbol model | Sin `VariableType` en el núcleo; motor adimensional; tres fronteras de validez | A-06 · A-03 |
| 6 | ExpressionContext | Inmutable por operación, desde un snapshot; los errores semánticos no invalidan el registro | OPEN B |
| 7 | SymbolResolver/binder | Única ruta nombre→id; enlace contra el snapshot mostrado; nunca contra una lectura posterior | OPEN A · A-04 |
| 8 | project-variable symbol binding | `SymbolId → VariableType` en el adaptador; contrato raíz del `VariableType` | A-06 · A-03 |
| 9 | units | `[mm]`, `[in]`, `[ft]`; una sola autoridad real para `25.4` y `12` | A-13 |
| 10 | function registry | `MIN`, `MAX`, `ABS`; diferidas `ROUND`, `CEILING`, `FLOOR`; tokens en tablas cerradas | A-17 · OPEN A |
| 11 | dependency extraction | Función pura, igual para las dos superficies | — |
| 12 | dependency graph | Derivado; dependencias y dependientes; conjuntos afectado y leído | A-05 |
| 13 | cycle detection | Iterativa y determinista; ciclos persistidos según OPEN B | OPEN B |
| 14 | evaluation order | Topológico determinista; `RegistryEvaluation`, único dueño | A-14 |
| 15 | diagnostics | Catálogo cerrado actualizado con los códigos de OPEN A y OPEN B | OPEN A · OPEN B |
| 16 | EvaluationResult | `Success`/`Failed` con `double`, diagnósticos y traza opt-in | A-14 |
| 17 | persistence | Dos formas; payload de Expression cerrado; frontera estructural fijada | A-07 · A-08 · A-17 |
| 18 | schema evolution | **V-0** con cuatro condiciones | A-11 · CLOSED |
| 19 | rename | Registry-only; formatter cualificado ante homónimos; permitido con errores semánticos | OPEN A · OPEN B |
| 20 | delete | `Delete` y `UnlinkAllAndDelete` bloqueados por fórmulas y dependientes | A-02 |
| 21 | propagation | R1–R3; `PlanReadSet`; tres fronteras de validez; reparación generalizada; un plan y un `Regen` | A-01 · A-03 · A-05 · A-12 · OPEN B |
| 22 | RACKVARIABLES UX | Enlace contra el snapshot; diagnóstico y reparación consciente de la fuente | A-04 · A-10 · A-18 · OPEN B |
| 23 | LinkedPropertyEditor UX | Sintaxis cerrada; `CASO_8` fijado; estado aislado; versión calculada | OPEN A · A-09 · A-15 |
| 24 | direct-reference compatibility | Resultados de `InspectBinding` reparables o fatales; dominio sobre referencias | A-01 · A-03 · OPEN B |
| 25 | ID20 extension point | Validado; `Rack.*` y `Project.*` reservados en la gramática | OPEN A |
| 26 | ID23 extension point | Validado con A-06 | A-06 |
| 27 | ID28/29 information preservation | `BoundExpression`, diagnósticos, traza opt-in, dependientes y conjuntos afectado y leído | A-05 |
| 28 | tests | Ampliado con los 21 escenarios de G2C | V3 |
| 29 | migration | Sin migración de datos ni de versión | — |
| 30 | non-goals | Actualizado con las decisiones cerradas | V3 |

### P1 — grammar

**Base.** No existe ningún parser de expresiones en el repositorio (Discovery §15, búsqueda 1). La coma ya tiene
dos significados: decimal en la regla localizada de ADR-0015 y separador de lista en otras superficies (Discovery
§11.7). El editor vinculable parsea literales en cultura invariante y RACKVARIABLES con la regla localizada
(Discovery §11.2–§11.3). No existe sintaxis de unidades (Discovery §11.7).

- **P1.1 — Lenguaje.** Expresiones aritméticas sobre números (con unidad opcional), referencias y llamadas a
  función. Sin sentencias, asignaciones, comparaciones, condicionales, booleanos ni cadenas.
- **P1.2 — Gramática** (EBNF) **[V3 · OPEN A]**. La producción `reference` queda cerrada con la sintaxis de §4:

  ```ebnf
  expression     = additive ;
  additive       = multiplicative , { ( "+" | "-" ) , multiplicative } ;
  multiplicative = unary , { ( "*" | "/" ) , unary } ;
  unary          = ( "+" | "-" ) , unary | primary ;
  primary        = quantity | call | reference | "(" , expression , ")" ;
  quantity       = number , [ "[" , unit , "]" ] ;
  unit           = "mm" | "in" | "ft" ;
  call           = word , "(" , [ expression , { "," , expression } ] , ")" ;
  number         = digit , { digit } , [ "." , digit , { digit } ] ;
  reference      = [ name ] , qualifier | name ;
  name           = bare-name | braced-name ;
  bare-name      = word , { " " , word } ;                  (* palabras separadas por UN espacio *)
  word           = ( letter | "_" ) , { letter | digit | "_" } ;
  braced-name    = "{" , { char - "}" | "}}" } , "}" ;      (* "}}" escapa "}" *)
  qualifier      = "#" , guid ;                             (* forma D completa: 8-4-4-4-12, 36 caracteres *)
  ```

  `letter` es un carácter de categoría letra Unicode; `digit`, un dígito ASCII `0`–`9`.
- **P1.3 — Precedencia.** `*` y `/` sobre `+` y `-`; asociatividad por la izquierda; el unario liga más fuerte
  que cualquier binario.
- **P1.4 — Números.** Forma invariante con `.` como único separador decimal. Sin exponente, sin agrupadores, sin
  punto inicial ni final (`.5`, `5.`), sin signo dentro del número (el signo es operador unario) y sin `NaN` ni
  `Infinity`.
- **P1.5 — Coma.** `,` es **solo** separador de argumentos. Una coma entre dos dígitos (`1,5`) es el error léxico
  `AmbiguousDecimalComma`: nunca se lee como decimal ni como dos argumentos, de modo que `MAX(Holgura, 1,5)` no
  puede convertirse en silencio en `MAX(Holgura, 1, 5)`.
- **P1.6 — Unidades** (REQUISITO DEL OWNER):
  - un sufijo `[mm]`, `[in]` o `[ft]` se admite **solo** tras un literal numérico: `100[mm]`, `4[in]`, `20[ft]`;
  - los tokens de unidad son exactamente `mm`, `in` y `ft`. Cualquier otro (`[cm]`, `[MM]`, `[m]`) es
    `UnknownUnit`;
  - un sufijo tras una referencia, una llamada o un paréntesis (`Holgura[mm]`, `{Holgura}[mm]`,
    `Holgura#<id>[mm]`, `(2 + 3)[mm]`) es `UnitNotAllowedHere` **[V3 · OPEN A]**;
  - las demás notaciones (`100 mm`, `12"`, `10'6"`, `1 1/8`) son `UnitSyntaxNotSupported`. `1/2` no es una
    fracción: es una división válida.

  Los corchetes quedan **exclusivamente** para unidades. El significado numérico de cada unidad está en P9.
- **P1.7 — Espacios y reglas léxicas** **[V3 · OPEN A]**.
  - Los espacios no son significativos entre tokens: `100 [mm]` se acepta y su forma canónica es `100[mm]`.
  - Dos excepciones: dentro de un `bare-name`, un espacio **simple** entre dos palabras forma parte del nombre; dentro
    de llaves, todo carácter forma parte del nombre.
  - Una `word` seguida de `(` es una llamada (P10).
  - Una `word` seguida de `.` es sintaxis de namespace reservada para ID20 y da `UnknownNamespace` (P25).
  - Un `bare-name` cuyo texto completo coincide, sin distinguir mayúsculas, con un nombre de función del registro,
    con `Rack` o con `Project` es `ReservedName`: esos nombres se escriben con llaves (`{MIN}`, `{Rack}`).
  - Una llave sin cerrar es `UnterminatedName`. Un cualificador que no sea un GUID completo en forma D
    (`#3f2b1c9e`) es `InvalidQualifier`: un fragmento de GUID **nunca** se parsea.
- **P1.8 — El `=` no pertenece a la gramática.** Es la marca de superficie que indica «esto es una expresión», en
  RACKVARIABLES (P22) y en el editor vinculable (P23). Nunca se persiste.
- **P1.9 — Límites** **[V3 · A-16]**.

  | Límite | Valor inicial | Naturaleza | Al escribir | Al leer lo persistido |
  |---|---|---|---|---|
  | Nodos por árbol | ≤ 256 | **Normativo** | `LimitExceeded` | Estructural (§5) |
  | Anidamiento | ≤ 24 | **Normativo** | `LimitExceeded` | Estructural (§5) |
  | Argumentos por llamada | ≤ 16 | **Normativo** | `LimitExceeded` | Estructural (§5) |
  | Texto de entrada | ≤ 4000 caracteres (nunca menos) | Protección de entrada | `LimitExceeded` | No aplica: lo persistido no tiene texto |

  Superar un límite **nunca** trunca. La longitud del texto no es un límite del árbol: una referencia cualificada
  ocupa más de 36 caracteres (§4) y un límite solo textual rechazaría fórmulas legítimas.

  **Presupuesto de profundidad JSON** (precisión del Coordinador dentro de A-16). Ningún `JsonSerializerOptions` de
  `src/RackCad.Application` fija `MaxDepth` (búsqueda en `src/` en `a4d88f1`), así que rige el máximo por defecto
  de `System.Text.Json`, 64 niveles. Un nodo `call` consume dos niveles (`Args` y el nodo). El registro añade cuatro
  niveles antes del nodo raíz (raíz, `Variables`, entrada y `Definition`) y `PropertyValues`, tres (raíz, mapa y
  entrada); el diseño viaja como cadena dentro del sobre (`RackEmbedDocument.cs:55-56`), así que el sobre no suma.
  - El anidamiento ≤ 64 de V2 no era persistible: un árbol legal cerca de ese límite no se habría podido
    serializar.
  - Con ≤ 24, el peor caso, con llamadas en todos los niveles, ocupa unos 51 niveles.
  - Prueba obligatoria en G8: el árbol que agota todos los límites a la vez atraviesa **cada** camino JSON real que
    lo transporta. Son los dos stores, el clonado de `ProjectVariableCloning`, la comparación multi-vista y el
    restamp.
  - Un valor inicial solo cambia con esa misma prueba.
- **P1.10 — Implementación.** Parser escrito a mano, determinista, independiente de la cultura y con el límite de
  anidamiento comprobado **antes** de descender. Prohibidos terceros (ADR-0012), `NCalc`, `DataTable.Compute`,
  `System.Linq.Expressions` y Roslyn (Discovery §15, búsqueda 3). **Un solo** lexer reconoce unidades, nombres y
  cualificadores: el del núcleo.
- **P1.11 — Texto sin `=`.** Sigue siendo un literal parseado **exactamente como hoy** en cada superficie: regla
  localizada de ADR-0015 en RACKVARIABLES e invariante en el editor vinculable. V3 no resuelve esa divergencia
  vigente (Discovery §11.2) ni admite unidades sin `=`. La gramática invariante de P1.4 solo rige dentro de una
  expresión, y esa excepción acotada a ADR-0015 tiene que constar en el ADR (§9).

### P2 — AST/model

**Base.** `VariableDefinition` tiene un caso y payload `double` (Discovery §4.1). `LinkedPropertySourceKind` es un
enum de dos miembros cuyo propio comentario anticipa que «una fórmula» añadirá un tercero
(`src/RackCad.Application/ProjectVariables/LinkedPropertySource.cs:8-13`). No hay AST (Discovery §15, búsqueda 2).

- **P2.1 — Tres modelos inmutables y separados.**
  1. **Sintaxis** (salida del parser): nodos con `SourceSpan` y referencias en forma textual. Solo sirve para
     escribir y diagnosticar. **Nunca se persiste.**
  2. **`BoundExpression`** (salida del binder o de la lectura de persistencia): `Number(double, unidad opcional)`,
     `Reference(SymbolId)`, `Negate(operand)`, `Binary(Add | Subtract | Multiply | Divide, left, right)` y
     `Call(FunctionId, args)`. Sin posiciones, sin nombres, sin paréntesis redundantes y sin `+` unario. **Sin
     anotaciones de tipo** (DR-5). Es lo que se persiste (P17), se evalúa (P14) y se formatea (P4).
  3. **Resultado** de evaluación (P16).
- **P2.2 — Comprobación semántica separada.** Existencia de referencias, aridad, unidades, ámbito y forma
  canónica se comprueban sobre un `BoundExpression` **contra un contexto** (P7.5), no dentro del árbol. El mismo
  árbol leído de persistencia se re-comprueba en cada snapshot.
- **P2.3 — Igualdad estructural por valor.** Ordinal para tokens y `double.Equals` para números, igual que la
  igualdad exacta vigente (Discovery §11.4). La unidad forma parte de la igualdad: `100[mm]` ≠ `3.937…`.
- **P2.4 — Conjunto de nodos cerrado y tokens explícitos** **[V3 · A-17]**.
  - Cada kind, nodo, operador, unidad, función y namespace tiene su token persistido declarado en una **tabla
    cerrada y explícita**, en los dos sentidos (P17.10).
  - **Nunca** se persiste semántica con `enum.ToString()`, `Type.ToString()`, `nameof` ni el nombre de una clase
    C#: renombrar un miembro no puede cambiar lo que se escribe ni lo que se lee.
  - Añadir un tipo de nodo, una unidad o una función cambia lo que las builds anteriores pueden leer (P18.8).
- **P2.5 — Canonicidad.** Para todo `BoundExpression` canónico `b` **sin referencias rotas** y un mismo snapshot:
  `Canonicalize(Bind(Parse(Format(b)))) == b`. Se prueba como propiedad (P28), también con homónimos y con nombres
  que exigen llaves, porque la sintaxis de §4 representa cualquier identidad. Un árbol con una referencia rota se
  formatea como `#<id>` y **no** enlaza (`BrokenReference`): ese texto solo sirve para mostrar (P4.5).
- **P2.6 — `VariableDefinition`.** Gana un segundo caso, `Expression = 2`, con payload `BoundExpression`.
  `LiteralValue` sigue lanzando fuera de `Literal`, y el acceso a la expresión lanza fuera de `Expression`.
- **P2.7 — `LinkedPropertySource`.** Gana el tercer miembro `Expression = 3`, con payload `BoundExpression`. El
  acceso a `VariableId` sigue lanzando fuera de `ProjectVariableReference`, y el acceso a la expresión lanza fuera
  de `Expression`. `LinkedPropertyEditState` conserva su forma `{ CommittedLiteral, Source }`.
- **P2.8 — Formas canónicas: una sola representación por significado** (DR-4). Se aplican **al comprometer**, en
  las dos superficies:

  | Árbol enlazado | Fuente de propiedad (superficie B) | Definición de variable (superficie A) |
  |---|---|---|
  | Solo `Reference(projectVariable X)` | **`ProjectVariableReference(X)`**, la referencia existente (REQUISITO DEL OWNER) | `Expression`: una definición no tiene caso referencia |
  | Solo un número **sin unidad** (`Number`, o `Negate(Number)` sin unidad) | `Literal(±v)` | `Literal(±v)` |
  | Cualquier otro árbol: operación, función o **unidad explícita** | `Expression` | `Expression` |

  Así `=Holgura` y la selección de `Holgura` en la lista persisten lo mismo, y `=6` persiste lo mismo que `6`.
  `=4[in]` es `Expression` aunque valga 4, porque lleva conversión explícita (REQUISITO DEL OWNER).

  - **Un literal canónico obedece la regla de literales de su superficie** **[V3 · A-18]**. Canonicalizar no es una
    vía para eludirla. En RACKVARIABLES, `=0` y `=-2` se rechazan igual que `0` y `-2` (`> 0`, P22.3), que es además
    el contrato raíz de `Length` (P8.10). En el editor vinculable, `=-3` sigue la ruta histórica del literal `-3`
    (P24.5).
  - **Una forma no canónica leída de persistencia es error SEMÁNTICO `NonCanonicalForm`** **[V3 · A-07]**, por
    símbolo o por fuente. Es no canónica una definición `expression` que solo sea un número sin unidad, o una fuente
    `expression` que solo sea una referencia o solo un número sin unidad. No se evalúa, no vuelve ilegible el
    registro y se corrige con `ChangeDefinition` o con la reparación del rack (§7.2). **Nunca** se normaliza en
    silencio y **ningún** escritor la produce: todo camino de escritura canonicaliza, y G8–G10 lo prueban.

### P3 — identity

**Base.** `VariableId` es GUID con igualdad `OrdinalIgnoreCase` (Discovery §4.1). La identidad ambigua es
fail-closed también en la re-lectura de commit (Discovery §4.2).

- **P3.1** — La identidad de una variable sigue siendo su `VariableId` (ADR-0034 §2). Cambiar el tipo de su
  definición no cambia el id.
- **P3.2** — `SymbolId = (SymbolNamespace, Key)`. Los namespaces son tokens de un conjunto cerrado comparados en
  `Ordinal`, y cada namespace fija el comparador de su clave. Para `projectVariable` la clave es el `VariableId`,
  con su igualdad vigente.
- **P3.3** — Una expresión de definición **no tiene identidad propia**: es la definición de su variable. Los nodos
  del grafo son `SymbolId`.
- **P3.4** — `FunctionId` es un token canónico en **mayúsculas** (`MIN`, `MAX`, `ABS`) de una tabla cerrada
  (P10.9), comparado en `Ordinal`; la entrada no distingue mayúsculas.
- **P3.5** — El nombre **nunca** es identidad: el árbol persistido no contiene nombres (P17) y la única resolución
  por nombre ocurre al escribir, contra el snapshot mostrado (P7.3).
- **P3.6** — Un `VariableId` duplicado da `AmbiguousIdentity` **antes** de construir cualquier tabla de símbolos
  (decisiones de I-48 §4): nada se enlaza ni se evalúa sobre un registro ambiguo.
- **P3.7** — Una variable que se referencia a sí misma forma un ciclo de longitud 1 (P13).
- **P3.8** — Una expresión de propiedad **tampoco** tiene identidad propia: pertenece a `(RackId, PropertyId)`,
  igual que hoy un binding. Ninguna expresión puede referenciarla (P13.6).
- **P3.9 — Cualificador textual de identidad** **[V3 · OPEN A]**.
  - `#` seguido del `VariableId` **completo** en forma D (36 caracteres), con los hexadecimales sin distinguir
    mayúsculas, igual que la igualdad de `VariableId`.
  - El formatter lo emite siempre en forma D en minúsculas.
  - Un fragmento **nunca** se parsea (`InvalidQualifier`). Los 8 caracteres que muestran las listas son solo una
    ayuda visual, porque dos ids con el mismo primer grupo se verían iguales (Discovery §13.2–§13.3).

### P4 — source text

**Base.** HANDOFF §4 exige persistir la referencia y «nunca el texto» (`docs/HANDOFF.md:1579`). `"0.###"` es formato
de **presentación** y redondea (Discovery §11.5, hallazgo lateral L5).

- **P4.1 — Tres textos, tres papeles**, en las dos superficies:

  | Texto | Origen | ¿Autoridad? | ¿Se persiste? |
  |---|---|---|---|
  | Texto tecleado | Usuario | No | No |
  | Forma canónica persistida (literal, referencia o `BoundExpression`) | Binder + canonicalización | **Sí** | Sí (P17) |
  | Texto de edición y presentación | Formatter, desde la forma canónica y los nombres **actuales** | No | No |

- **P4.2 — Un solo formatter.** Determinista e invariante:
  - números en su representación más corta que hace *round-trip* (nunca `"0.###"`), con la unidad pegada
    (`100[mm]`);
  - un espacio a cada lado de un operador binario, `MIN(a, b)` y funciones en mayúsculas;
  - paréntesis **mínimos** según precedencia y asociatividad;
  - referencias en la forma **mínima inequívoca dentro del snapshot** **[V3 · OPEN A]** (§4.3):

    | Situación de la referencia | Texto emitido |
    |---|---|
    | Id ausente del registro | `#<id>` |
    | Nombre único y seguro sin llaves | `Nombre` |
    | Nombre único con cualquier otro carácter, o reservado | `{Nombre}`, con `}` escapado como `}}` |
    | Homónimo | La forma del nombre + `#<id>` |

- **P4.3 — Normalización visible.** Espacios, paréntesis redundantes, mayúsculas de funciones, llaves innecesarias y
  cualificadores innecesarios no se conservan. Tras comprometer, el usuario ve la forma canónica.
- **P4.4 — El texto tecleado no se persiste.** Sería una segunda fuente capaz de contradecir al árbol y quedaría
  obsoleta con cada rename (ADR-0034 §2).
- **P4.5 — Referencia rota** **[V3 · OPEN A]**. Se muestra como `#<id>`, sin tomar un nombre de ningún otro sitio.
  Ese texto se puede **mostrar** y copiar, pero **no** se puede comprometer como referencia nueva: al enlazar da
  `BrokenReference` (P7.1).
- **P4.6** — Una referencia directa se muestra como `=` seguido de la referencia formateada (hoy `=Nombre`,
  Discovery §9.2; con un homónimo, `=Holgura#<id>`). Una expresión, como `=` seguido del formatter.
- **P4.7 — La forma mostrada depende del snapshot actual** **[V3 · OPEN A]**. Si un nombre único pasa a ser
  ambiguo tras un `Create` o un `Rename`, la siguiente presentación lo cualifica; si deja de serlo, lo muestra sin
  cualificador. Lo persistido no cambia en ningún caso.

### P5 — symbol model

**Base.** No existe `SymbolId`, scope ni namespace, y el registro acreditado se indexa solo por `VariableId`
(Discovery §15, búsquedas 7–9; §16.2).

- **P5.1** **[V3 · A-06]** — `SymbolEntry { SymbolId, SymbolScope, DisplayName, Definition }`, con
  `Definition = Literal(double) | Expression(BoundExpression)`. **Sin** `ConsumerType` ni `VariableType`: el núcleo
  no nombra ningún tipo de `ProjectVariables`, e ID23 puede construir su tabla de símbolos sin esa dependencia
  (P26.1). La relación `SymbolId → VariableType` vive en el adaptador (P8.9).
- **P5.2 — Namespaces.** Hay **uno** activo, `projectVariable`. Quedan **reservados conceptualmente, sin registrar y
  sin resolución**: `rack` y `project` (ID20, P25). El binder devuelve `UnknownNamespace` para cualquier namespace
  no registrado.
- **P5.3 — Ámbitos.** Existe **uno** activo, `Project`: un valor por dibujo. Queda reservado `Rack`: un valor por
  rack (ID20). Una definición de ámbito `Project` **no puede** depender de un símbolo `Rack` (`ScopeViolation`). Una
  expresión de propiedad se evalúa en el contexto de su rack y en V3 solo ve símbolos `Project`.
- **P5.4 — Dominio de valores adimensional** (REQUISITO DEL OWNER). Todo valor del motor es un `double` finito.
  **No existe** `ExpressionValueType`, ni `Scalar`, ni `DimensionMismatch`, ni `ResultTypeMismatch`, ni reglas del
  tipo `Length × Length` o `Length ÷ Length`.
- **P5.5 — `VariableType` fuera del álgebra** **[V3 · A-03 modificada]**. `VariableType` sigue siendo exactamente
  `{ Length = 1 }`, y las guardas G6–G10 no cambian (Discovery §14.3).
  - Dice cómo interpreta el número quien lo consume: pulgadas.
  - **No** gobierna ninguna álgebra dentro del motor.
  - Vive solo en el adaptador de Project Variables, que aplica su contrato al **resultado raíz** (P8.10).
- **P5.6 — Sin comprobador dimensional ni de signo en el motor.** Son válidas, y lo fijan pruebas de comportamiento
  (P28.3): `6 + 2`, `A + 2`, `A * B`, `A / B`, `100[mm] + 2`, `MAX(A, 4[in])` y `Base + (A - B)` con un intermedio
  negativo. El motor no rechaza **ninguna** operación por dimensiones ni por signo.
- **P5.7 — Tres fronteras de validez** **[V3 · A-03 modificada]** (DR-5; §0.2):

  | Frontera | Qué valida | Dónde | Resultado si falla |
  |---|---|---|---|
  | Motor de expresiones | Todo nodo da un `double` finito; sin dominio físico | Núcleo | `DivisionByZero`, `NonFiniteResult` |
  | Adaptador de Project Variables | Contrato vigente del `VariableType` sobre el **resultado raíz**, igual para literal y expresión | Preflight de las mutaciones de variables | `OutOfRange`, cuyo dueño es la variable |
  | Consumidor de propiedad | Dominio de la propiedad sobre el **valor efectivo** | Resolver único; ruta histórica de la ventana para literales | `OutOfRange`, cuyo dueño es `(RackId, PropertyId)` |

### P6 — ExpressionContext

**Base.** RACKBOMTOTAL lee el registro **una** vez y no resuelve por su cuenta (Discovery §14.3, guarda G12). La
costura pura `FromTargets` existe solo para tests (Discovery §4.3).

- **P6.1** — `ExpressionContext` es un valor inmutable **por operación**: `SymbolTable` + la única instancia
  productiva de `FunctionRegistry` + la autoridad de unidades (P9.5) + `ExpressionLimits`.
- **P6.2 — Sin estado ambiental**: ni cultura, ni reloj, ni aleatoriedad, ni entorno, ni cachés estáticas, ni
  AutoCAD, ni sistema de archivos.
- **P6.3 — Un snapshot por operación**, el mismo para toda ella (ADR-0034 §10).
- **P6.4 — Construcción para Project Variables**: solo desde `UsableProjectVariablesRegistry` acreditado. `Absent`
  da tabla vacía; `Readable` con ids únicos da tabla; cualquier otro resultado **no** produce contexto (Discovery
  §4.1, `Accredit`).
- **P6.5 — Costura de test**: la construcción desde entradas sintéticas se permite en el núcleo y en tests, y no
  es alcanzable desde Plugin ni UI, igual que `FromTargets`.
- **P6.6** — ID20 e ID23 construirán sus propios contextos desde sus propios snapshots puros (P25, P26).
- **P6.7 — Contexto de una propiedad de rack.** Para evaluar la expresión de una propiedad, el resolver usa el
  contexto del snapshot y los valores **ya evaluados** de `RegistryEvaluation` (P14.9). En V3 no añade símbolos
  propios del rack.
- **P6.8 — Un error semántico no invalida el contexto** **[V3 · OPEN B]**. Los símbolos en error siguen en la tabla,
  con su resultado fallido, y el registro sigue `Usable`. Solo un fallo **estructural** o de identidad impide
  construir el contexto (P6.4).

### P7 — SymbolResolver/binder

**Base.** Hoy **ninguna** ruta de producción resuelve nombre→id. El editor filtra por *contains* `OrdinalIgnoreCase`
y compromete una referencia solo por `VariableId` (Discovery §13.3; `TrySelect` es «the ONLY way a reference is ever
committed», `LinkedPropertyEditSession.cs:302-306`). Los homónimos son legales (Discovery §13.1).

- **P7.1 — `SymbolResolver`** **[V3 · OPEN A]**. Traduce una referencia textual en la sintaxis de §4 a **exactamente
  un** `SymbolId`, o a un fallo tipado. **Nunca** elige uno entre varios ni completa un nombre parcial.

  | Entrada | Resultado |
  |---|---|
  | `Nombre#<id>` o `{Nombre}#<id>`, id presente y nombre igual al actual | Ese id |
  | `…#<id>` con id ausente | `BrokenReference`, no comprometible |
  | `Nombre#<id>` con id presente y nombre distinto del actual | `QualifiedNameMismatch`: el nombre se valida, nunca resuelve |
  | `#<id>` sin nombre, id presente | `NameRequired`: esa forma solo la emite el formatter para referencias rotas |
  | Nombre sin cualificador que coincide exactamente con uno | Ese id |
  | Nombre sin cualificador sin coincidencias | `UnknownSymbol` |
  | Nombre sin cualificador con dos o más coincidencias | `AmbiguousName`, con **cada** candidato en forma cualificada |
  | `bare-name` reservado | `ReservedName` |
  | Tramo contiguo, sin llaves ni espacios, con operadores, que coincide con el nombre de una variable (`Holgura-Base`) | `OperatorInName`: nunca se reinterpreta como resta |
  | `palabra.` | `UnknownNamespace` |

  **Precisión del Coordinador** sobre el veredicto del Architect: `#<id>` sin nombre **nunca** enlaza, ni siquiera
  con un id presente. Así el texto que representa un estado roto no puede comprometer una identidad que el usuario
  no ve nombrada.
- **P7.2 — Comparador de nombres** (CERRADO): igualdad exacta `OrdinalIgnoreCase`, sin recortes ni normalización
  Unicode, coherente con los comparadores vigentes (Discovery §13.3). El *contains* del editor sigue siendo un
  **filtro** y no una regla de resolución (Discovery §13.4).
- **P7.3 — Única ruta nombre→id de producción: solo al escribir y solo contra el snapshot que el usuario ve**
  **[V3 · A-04]**. El binder corre únicamente al comprometer texto del usuario en las dos superficies (P22, P23).
  Lectura, evaluación, rename, delete, propagación, resolver, re-lectura de commit y BOM **nunca** resuelven nombres.
- **P7.4 — Binder total y fail-closed.** Devuelve un árbol solo con **cero** diagnósticos; si no, la lista de
  diagnósticos, en orden determinista y con un tope (20, ajustable).
- **P7.5 — Comprobación semántica** (P2.2): existencia de referencias, aridad (`InvalidArguments`), unidad conocida
  (P9), ámbito (P5.3), autorreferencia (P3.7) y forma canónica (P2.8). Sobre un árbol leído de persistencia corre
  **sin** resolver nombres.
- **P7.6 — Dónde vive.** En Application. El control WPF **no** resuelve nombres: la guarda G19 sigue exigiendo que
  `LinkedPropertyEditor.cs` no compare nombres (Discovery §14.3).
- **P7.7 — Enlace contra el snapshot mostrado** **[V3 · A-04]**. Corrige P22.7 de V2, que decía «contra el registro
  re-leído, nunca contra el snapshot de la ventana». Eso era falso respecto del código y habría dejado que un nombre
  cruzara una frontera entre lecturas actuando como identidad (ADR-0034 §2).

  ```text
  RACKVARIABLES (definiciones)
    snapshot del workspace = LastRegistry, la lectura que construyó la ventana
    → binder puro de Application          → forma enlazada: literal o BoundExpression con ids
    → intent con la forma enlazada        (nunca texto humano)
    → preflight: valida los ids contra LastRegistry (la misma lectura)
    → commit: re-lectura, re-acreditación, ids y PlanReadSet (P21.6)

  RACKEDITAR (fuentes de propiedad)
    snapshot del comando = el registro leído antes de abrir la ventana
    → sesión: binder puro contra ese snapshot → estado comprometido con BoundExpression
    → frontera C4                         → LinkedPropertyFinalStates
    → reconciliador: re-comprueba el árbol por ids contra el MISMO snapshot, sin nombres
  ```

  - **FACT**: el bucle de RACKVARIABLES corre el preflight sobre `LastRegistry`/`LastEntries`, la misma lectura que
    construyó la ventana (`src/RackCad.Plugin/RackVariablesCommands.cs:59-60,121-122`).
  - **FACT**: en RACKEDITAR, una sola lectura del registro sirve para abrir, ofrecer opciones y reconciliar
    (`src/RackCad.Plugin/RackSelectivoCommands.cs:66,80-81,144-145`).
  - **Regla**: una lectura posterior **solo** valida ids (presentes y sin identidad ambigua) y valores
    (`PlanReadSet`). Nunca vuelve a resolver texto humano.

### P8 — project-variable symbol binding

**Base.** El `Type` persistido tiene un mapping único compartido (decisiones de I-48 §4; `VariableTypes.TryParseToken`,
Discovery §4.1). La compatibilidad de tipo al vincular vive en `InspectBinding` y `Link` (Discovery §4.1, §8.1).

- **P8.1** — Namespace `projectVariable`; clave = el texto del `VariableId` de la entrada del registro; comparación
  `OrdinalIgnoreCase`.
- **P8.2** — Fuente única: el registro acreditado (P6.4), con el mapping único del `Type`. **No** se crea un segundo
  mapping.
- **P8.3** — `DisplayName = Name`, solo para mostrar y para resolver al escribir.
- **P8.4** — `Definition` = el `Literal` o la `Expression` de la entrada.
- **P8.5** — Un árbol que referencia un id ausente produce `BrokenReference`. Al escribir, el binder lo rechaza
  (P7.1). En un árbol persistido es un error **semántico** (§5): la variable queda `Failed` y una fuente de rack,
  `RepairableMissingTarget`.
- **P8.6** — Una referencia directa de rack (`ProjectVariableReference`) **no** es un símbolo de expresión: es un
  consumidor que lee el valor evaluado de su variable (P24).
- **P8.7 — Referencia directa.** Conserva **sin cambio** la comprobación vigente entre el `VariableType` de la
  variable y el de la propiedad (V4-R01 de I-48).
- **P8.8 — Expresión de propiedad o de definición.** Puede referenciar cualquier variable de `VariableType`
  numérico; en V3 todos lo son, porque solo existe `Length`. El resultado es un `double` que el consumidor
  interpreta según **su** `VariableType` declarado. Qué pasa al combinar tipos distintos se decidirá cuando exista
  un segundo `VariableType`, con ADR.
- **P8.9 — La relación `SymbolId → VariableType` vive en el adaptador** **[V3 · A-06]**. Se construye desde el
  registro acreditado con el mapping único del `Type` y **fuera** del núcleo. La usan P8.7 y P8.10.
- **P8.10 — Contrato raíz del `VariableType`** **[V3 · A-03 modificada]** (§0.2). El adaptador lo aplica al
  **resultado raíz** de la definición, con las mismas reglas para literal y expresión.
  - **`Length` al escribir.** El contrato vigente del producto es `> 0`: RACKVARIABLES rechaza cualquier otro valor
    con «El valor tiene que ser un número mayor que cero.» (`RackProjectVariablesWindow.xaml.cs:215-224`; Discovery
    §10.3, §11.4). En V3:
    1. la raíz de la variable mutada (`Create`, `ChangeDefinition`) cumple `> 0`, sea literal o expresión;
    2. toda raíz del cierre `I` que cumplía antes cumple después: un cambio de `A` no puede dejar `Ajuste = A - B`
       en `-2`. Es la regla R2 de §5, aplicada al contrato;
    3. una raíz que ya lo incumplía antes, alcanzable solo por edición externa, y cuya definición no cambia no
       bloquea por sí sola. Aun así, no puede llevar a una propiedad un valor fuera de dominio (P24.5, regla R1).

    Si falla: `OutOfRange`, cuyo dueño es la variable, y plan vacío.
  - **`Length` al leer.** Rige el contrato histórico de lectura, que exige un valor finito
    (`ProjectVariablesStore.cs:199-204`; `VariableTargetSnapshot.cs:72-76`; Discovery §11.4), **igual para las dos
    sintaxis**. Ningún valor persistido se re-valida con más rigor por ser expresión, y ningún literal con más rigor
    que hoy.
  - **Autoridad.** La comprobación de la ventana se conserva como aviso temprano. La autoridad pasa al preflight de
    Application, fijada por pruebas; hoy ninguna prueba fija `> 0` (Discovery §11.4).
  - **Resultado.** Para el mismo valor, una variable definida por literal y otra definida por expresión tienen la
    misma validez al escribir y al leer: la validez no depende de la sintaxis.
  - **Variables escalares o con signo.** Exigen otro `VariableType` u otra iniciativa (P30.5). I-49 no las introduce
    de forma implícita.

### P9 — units

**Base.** Pulgadas internas como `double` sin unidad (ADR-0005, ADR-0021; Discovery §12.1–§12.2). No hay autoridad
neutral de conversión ni parser de unidades (Discovery §12.4; §15, búsqueda 12). `StructuralSectionUnits` es
específico del catálogo (Discovery §12.5).

- **P9.1** — La pulgada sigue siendo la unidad interna canónica. El motor **no convierte el DWG** (ADR-0005 §4) y
  todo resultado está en pulgadas.
- **P9.2 — Unidades admitidas** (REQUISITO DEL OWNER): `[mm]`, `[in]` y `[ft]`, solo sobre literales numéricos
  (P1.6).
- **P9.3 — Semántica**: conversión **numérica** a pulgadas en la evaluación, con factores exactos.

  | Escrito | Cálculo | Resultado en pulgadas |
  |---|---|---|
  | `4[in]` | `4` | `4` |
  | `20[ft]` | `20 × 12` | `240` |
  | `100[mm]` | `100 ÷ 25.4` | `3.937007874015748…` |

  La operación está **fijada** (multiplicar por 12, dividir entre 25.4) para que el resultado sea determinista bit
  a bit.
- **P9.4 — Número sin unidad.** Es un número adimensional. Un consumidor `Length` lo lee en pulgadas por contrato
  (P5.5); el motor no infiere ninguna unidad.
- **P9.5 — Una sola autoridad neutral de conversión.** Un único componente de Application (ilustrativo:
  `RackCad.Application.Units.LengthUnits`) declara la tabla cerrada de unidades, sus tokens y los factores exactos
  `MillimetersPerInch = 25.4` e `InchesPerFoot = 12`. No depende de nada: ni del núcleo, ni de `StructuralSections`,
  ni de sistemas. El núcleo la consume; **no** existe otro parser de unidades en UI, Domain ni Plugin.
- **P9.6 — Auditoría de los factores existentes** (hechos verificados en `a4d88f1`; búsqueda de `25.4`, `12.0` y
  `12d` en `src/`):

  | Constante | Ubicación | Papel | Tratamiento en V3 |
  |---|---|---|---|
  | `InchesToMillimeters = 25.4`, pública | `src/RackCad.Application/StructuralSections/StructuralSectionUnits.cs:18-19` | Milímetros por pulgada | Conserva nombre y API; su valor lo da la autoridad |
  | `InchesPerFoot = 12d`, privada | `StructuralSectionUnits.cs:27` | Pulgadas por pie (peso por longitud, `:80`) | Su valor lo da la autoridad |
  | `FootInches = 12.0`, pública | `src/RackCad.Application/Systems/Selective/SelectiveGeometryResolver.cs:31` | Pulgadas por pie (redondeo al pie, `:422`) | Su valor lo da la autoridad, **tras** el procedimiento de colisión: I-50 modifica este archivo (§12) |
  | `CommercialFoot = 12.0`, pública | `src/RackCad.Application/Systems/Dynamic/DynamicHeaderHeightCalculator.cs:49` | Pulgadas por pie (redondeo al pie comercial, `:147-154`; también `PushBackBedSlope.cs:38`) | Su valor lo da la autoridad |
  | `12.0` en línea | `src/RackCad.Application/Systems/Cantilever/CantileverArmFrameResolver.cs:48` | Pulgadas de avance de la notación «rise per 12 in» (Discovery §12.2) | Su valor lo da la autoridad; la notación de ADR-0025 no cambia |

  **No son factores de conversión**, aunque valgan 12: `SelectiveRackDefaults.DefaultSeparator`
  (`src/RackCad.Domain/Systems/Selective/SelectiveRackDefaults.cs:33`), `DynamicForkliftDefensePlan.EdgeLength`
  (`src/RackCad.Application/Systems/Dynamic/DynamicForkliftDefensePlan.cs:14`) y las medidas de pantalla de la UI.
  El factor lb/ft→kg/m duplicado en `tools/` (Discovery §12.3) está fuera de los ensamblados de producto y se
  registra como seguimiento en G4.
- **P9.7 — Una sola autoridad REAL** **[V3 · A-13]**.
  - `LengthUnits.MillimetersPerInch` y `LengthUnits.InchesPerFoot` son las **únicas** declaraciones semánticas de esos
    factores en `src/`.
  - Las constantes de P9.6 conservan su nombre y su API pública, pero se declaran como **alias** de la autoridad. Son
    constantes de compilación con el mismo valor, así que el comportamiento es idéntico bit a bit y los dorados
    vigentes no cambian. No queda ninguna segunda constante semántica.
  - Dirección de dependencias: `StructuralSections` y `Systems` → `Units`. La autoridad no depende de nada, y el
    núcleo **nunca** referencia `StructuralSections` (guarda, P28.4).
  - Compatibilidad con guardas vigentes: la guarda de Cantilever prohíbe `StructuralSectionUnits`
    (`tests/RackCad.Tests/CantileverSourceGuardTests.cs:160-169`), no una autoridad neutral de longitud.
  - Una guarda nueva impide que reaparezca un `25.4` o un factor de pulgadas por pie fuera de la autoridad en `src/`,
    con las exclusiones de P9.6 enumeradas.
- **P9.8 — Presentación.** El texto de una expresión conserva la unidad escrita (`=100[mm] + Base`). Los valores se
  muestran en pulgadas con `"0.###"`, y las etiquetas «(in)» se mantienen.
- **P9.9** — Sin áreas, volúmenes, masas ni ángulos, y sin unidades distintas de `mm`, `in` y `ft` (P30).

### P10 — function registry

**Base.** No existe registro de funciones (Discovery §15, búsqueda 6). ADR-0012 prohíbe dependencias NuGet de
producto sin acuerdo del Owner y ADR (Discovery §15).

- **P10.1** — `FunctionRegistry` **cerrado e inmutable**, declarado en código dentro del núcleo, con una sola
  instancia productiva. Sin registro en tiempo de ejecución, reflexión, inyección, plugins, funciones de usuario ni
  scripting.
- **P10.2 — Contrato de una función**: `FunctionId`, aridad mínima y máxima, implementación pura y determinista
  sobre `double` finitos. Un resultado no finito es `NonFiniteResult`.
- **P10.3 — Conjunto inicial** (REQUISITO DEL OWNER; confirmado por el Architect):

  | Función | Aridad | Semántica |
  |---|---|---|
  | `MIN(x₁, …, xₙ)` | 2 ≤ n ≤ 16 | Menor valor |
  | `MAX(x₁, …, xₙ)` | 2 ≤ n ≤ 16 | Mayor valor |
  | `ABS(x)` | 1 | Valor absoluto |

- **P10.4** **[V3 · OPEN B]** — Al escribir: nombre desconocido → `UnknownFunction`; aridad errónea →
  `InvalidArguments`. En un árbol persistido: token de función desconocido → ilegibilidad **estructural** (P17.6);
  función conocida con aridad errónea → error **semántico** `InvalidArguments`.
- **P10.5** — Nombre sin distinguir mayúsculas a la entrada (`max`, `Max`); mayúsculas canónicas en el árbol y en
  el formatter.
- **P10.6 — Candidatos evaluados y DIFERIDOS: `ROUND`, `CEILING`, `FLOOR`.**

  | Pregunta que bloquea | Por qué no se decide en V3 |
  |---|---|
  | Regla del punto medio | `Math.Round` de .NET redondea al par por defecto y las hojas de cálculo suelen alejarse del cero: una u otra cambia resultados sin avisar |
  | Precisión o múltiplo | ¿Entero? ¿Dígitos? ¿Un múltiplo, como `CEILING(x, 6)`? Cada firma es un contrato persistido distinto |
  | Interacción con unidades | Tras `100[mm]`, redondear a pulgada entera no es lo que el usuario escribió en milímetros |
  | Negativos | Sentido de `CEILING` y `FLOOR` con valores negativos |
  | Consumidor natural | Las cantidades **enteras** del BOM (`Quantity`, Discovery §16.3) son de ID23, que tendrá que fijar su semántica |

  Pueden añadirse después como ampliación **cerrada** del registro, con revisión, y con ADR si cambian lo que se
  persiste. Una build anterior las rechaza de forma estructural (P18.8).
- **P10.7 — Excluidos salvo nueva razón aprobada**: `IF`, `AND`, `OR`, comparadores, lookup, arrays, strings,
  trigonometría y macros.
- **P10.8** — Añadir una función es un cambio de código con revisión y, para las builds anteriores, un cambio de lo
  que pueden leer (P18.8).
- **P10.9 — Tabla de tokens y palabras reservadas** **[V3 · A-17 · OPEN A]**.
  - Los tokens persistidos `MIN`, `MAX` y `ABS` forman una tabla cerrada y explícita, comparada en `Ordinal`.
  - Esa misma tabla es la fuente de las palabras reservadas de §4: `{MIN}` es la variable llamada `MIN`, y `MIN`
    sin llaves es `ReservedName`.
  - Añadir una función amplía la lista de reservadas **solo para escrituras futuras**. Lo persistido no cambia,
    porque lleva ids, y el formatter pasa a mostrar con llaves la variable homónima de la función nueva.

### P11 — dependency extraction

**Base.** Hoy no existe ninguna relación variable→variable (Discovery §6.2). `Targets()` ordena por id
`OrdinalIgnoreCase` (Discovery §13.3).

- **P11.1** — `Dependencies(BoundExpression) → IReadOnlyList<SymbolId>`: referencias **directas**, sin repetición y
  en orden determinista (namespace `Ordinal`; después clave con el comparador de su namespace, el mismo orden que
  `Targets()`).
- **P11.2** — Opera sobre el árbol persistido **sin evaluar** y **sin nombres**. Funciona igual sobre un árbol
  estructuralmente legible que falla al evaluar (§5).
- **P11.3** — Un literal tiene cero dependencias. Una referencia directa de rack depende de su único `VariableId`.
- **P11.4** — La consumen el grafo (P12), `Delete` y `UnlinkAllAndDelete` (P20), la propagación y el `PlanReadSet`
  (P21), RACKVARIABLES (P22) y la preservación para ID28/ID29 (P27).
- **P11.5** — La **misma** función extrae las dependencias de una expresión de definición y de una expresión de
  propiedad: no hay una segunda implementación por superficie.

### P12 — dependency graph

**Base.** ADR-0034 rechazó persistir un grafo en ID22A porque «el grafo pertenece a ID22B/ID21» (Discovery §6.2). La
relación propiedad→variable vive en el authored de cada rack y se descubre barriendo el dibujo (Discovery §7).

- **P12.1 — Derivado, nunca persistido.** Se construye desde la tabla de símbolos de UN snapshot. El registro es la
  única autoridad: un grafo persistido sería una segunda fuente capaz de divergir.
- **P12.2** — Nodos: los símbolos de la tabla (en V3, las variables de proyecto). Aristas: dueña → dependencia, con
  índice inverso dependencia → dependientes.
- **P12.3 — Consumidores de rack fuera del grafo.** Las relaciones propiedad→variable, directas **o** por expresión,
  no entran en el grafo de variables: se obtienen del barrido vigente y se unen al componer el plan (P21.4). Es la
  separación que el Discovery dedujo para su pregunta 8 (Discovery §2.2).
- **P12.4** — Una arista hacia un id ausente se registra como `BrokenReference`, sin crear nodo.
- **P12.5** — Iteración en el orden de P11.1.
- **P12.6** — Coste `O(V + E)` por snapshot. Que los registros reales sean pequeños es **INFERENCE**; se mide en G11.
- **P12.7 — Conjuntos derivados que expone cada operación** **[V3 · A-05]**:
  - `Dependencies(s)` y `Dependents(s)`, directos y transitivos;
  - **conjunto afectado**: el cierre `I` y los racks que lo consumen (P21.2);
  - **conjunto leído**: el `PlanReadSet` del plan (P21.6).

  Todos se derivan en memoria y **ninguno** se persiste (P27).

### P13 — cycle detection

**Base.** No existe detección genérica de ciclos (Discovery §15, búsqueda 5). El plan vacío ante fallo es invariante
(Discovery §4.2).

- **P13.1** — Detección **iterativa** (sin riesgo de profundidad de pila) y determinista de componentes fuertemente
  conexas. Un ciclo es una componente de más de un nodo o un nodo con arista a sí mismo.
- **P13.2** — Informa **cada** ciclo con sus miembros en el orden de P11.1; cada miembro recibe `Cycle` con la lista
  completa.
- **P13.3** — Al escribir o mutar, un estado «después» con un ciclo que pase por la variable cambiada hace fallar el
  preflight con `Cycle` y plan **vacío** (P21).
- **P13.4** — Los miembros de un ciclo y **todos** sus dependientes transitivos no son evaluables (`Cycle` o
  `DependencyFailed`), y tampoco las propiedades que los consumen. **Nunca** hay fallback a un literal ni a cero.
- **P13.5 — Ciclo persistido** **[V3 · OPEN B]**. Con P13.3, esta build no escribe ciclos; uno leído de persistencia
  es un error **semántico**:
  - todos los miembros dan `Failed(Cycle)`, con la ruta completa en el orden de P11.1;
  - sus dependientes dan `DependencyFailed`, y las fuentes de rack que los leen, `RepairableSemanticFailure` con causa
    superior (P24.6);
  - el registro sigue `Usable` y RACKVARIABLES lo muestra en su panel de diagnóstico (P22.9);
  - se rompe con `ChangeDefinition` de **un** miembro, que tiene que evaluar bien (regla R2, P21.3);
  - borrar un miembro sigue bloqueado, porque tiene dependientes (P20.2).
- **P13.6** — Las expresiones de propiedad **no** forman ciclos: nada puede referenciar una propiedad en V3 (ID21
  fuera). Solo pueden quedar afectadas por un ciclo de variables.

### P14 — evaluation order

**Base.** Hoy el resolver escribe `Target.LiteralValue` y ningún valor se calcula a partir de otro (Discovery §6.2).
La igualdad es exacta (Discovery §11.4).

- **P14.1** — Orden topológico sobre la parte acíclica, con el desempate de P11.1, **independiente** del orden de las
  entradas en el registro. Una prueba lo demuestra con permutaciones (P28).
- **P14.2** — Las **variables** se evalúan de forma **ansiosa**, una vez por snapshot (`RegistryEvaluation`) y con
  memoización: cada una se evalúa una sola vez.
- **P14.3** — Cortocircuito: si una dependencia falla, sus dependientes reciben `DependencyFailed` con el id de la
  causa, sin evaluarse.
- **P14.4 — Semántica numérica.** `double` IEEE-754 sin redondeos intermedios. `x ÷ 0`, incluido `0 ÷ 0`, es
  `DivisionByZero`. Cualquier resultado intermedio o final no finito es `NonFiniteResult`. Las unidades se
  convierten al evaluar su literal (P9.3).
- **P14.5 — Determinismo.** Sin paralelismo, cultura ni reloj: el mismo snapshot da resultados idénticos bit a bit.
- **P14.6** — Un literal se evalúa a su propio valor, sin cambio. Por eso todo dibujo de I-47/I-48 produce
  **exactamente** los mismos efectivos que hoy, con la única excepción declarada de P24.11.
- **P14.7 — Dónde.** Dentro de Application: las variables tras la acreditación, y las expresiones de propiedad dentro
  del resolver único (DR-3). **Nunca** en el Plugin, la UI, el ejecutor ni el comando de BOM (guardas extendidas,
  P28.4).
- **P14.8 — Expresiones de propiedad.** Se evalúan **una vez por rack** durante su resolución, sobre los valores de
  `RegistryEvaluation` de ese mismo snapshot. Un fallo de cualquier propiedad deja **sin efectivo** a todo el rack,
  igual que hoy un binding roto (Discovery §4.2: el resolver valida todo antes de escribir un campo).
- **P14.9 — Un solo dueño de los valores evaluados** **[V3 · A-14]**.
  - `RegistryEvaluation` es el **único** dueño de los valores de los símbolos: se calcula una vez por snapshot
    acreditado.
  - Targets acreditados, opciones del editor, workspace de RACKVARIABLES, preflight y RACKBOMTOTAL **transportan**
    sus valores y resultados. Ninguno vuelve a evaluar un símbolo como autoridad.
  - El efectivo de una propiedad lo produce el resolver único a partir de `RegistryEvaluation`.
  - La sesión del editor evalúa un **borrador** solo para presentarlo y avisar pronto (P23.6). La autoridad es la
    resolución del reconciliador, y el valor mostrado nunca se escribe.

### P15 — diagnostics

**Base.** La doctrina exige estados tipados (ADR-0034 §8). El texto de reparación ya vive en una sola capa
(`ProjectVariableRepairText`, Discovery §3.4).

- **P15.1** — Catálogo **cerrado** de códigos. Cada diagnóstico lleva código estable, clase, severidad, dueño (una
  variable, o un par `(RackId, PropertyId)`), posición opcional (solo al escribir) e ids relacionados. Un fallo
  por causa superior lleva además la cadena hasta la causa raíz.
- **P15.2 — Severidad.** En V3 todo diagnóstico es `Error`: no hay avisos que permitan continuar.
- **P15.3 — Catálogo de V3.**

  | Clase | Códigos | Cuándo |
  |---|---|---|
  | Sintaxis | `EmptyExpression`, `UnexpectedCharacter`, `UnexpectedToken`, `UnbalancedParenthesis`, `UnterminatedName`, `InvalidNumber`, `AmbiguousDecimalComma`, `UnknownUnit`, `UnitNotAllowedHere`, `UnitSyntaxNotSupported`, `InvalidQualifier`, `LimitExceeded` | Al escribir |
  | Enlace | `UnknownSymbol`, `AmbiguousName`, `UnknownNamespace`, `UnknownFunction`, `ScopeViolation`, `ReservedName`, `NameRequired`, `QualifiedNameMismatch`, `OperatorInName` | Al escribir |
  | Semánticos | `BrokenReference`, `Cycle`, `DependencyFailed`, `InvalidArguments`, `DivisionByZero`, `NonFiniteResult`, `NonCanonicalForm` | Al escribir y al evaluar lo persistido (§5) |
  | Contrato de frontera | `OutOfRange`, con dueño variable (adaptador, solo al escribir, P8.10) o propiedad (consumidor, P24.5) | Al escribir y, para propiedades, al resolver |

- **P15.4** — El núcleo devuelve códigos y datos. Los mensajes en español se producen en **una** capa de texto de
  Project Variables o de UI.
- **P15.5** — Cualquier `Error` implica: sin valor, sin resultado parcial y sin fallback.
- **P15.6** — Los estados de nivel registro conservan sus resultados vigentes (`PresentButUnreadable`,
  `IncompatibleMajor`, `AmbiguousIdentity`; Discovery §4.1). Los diagnósticos **no** los sustituyen.
- **P15.7** — Orden determinista: posición, código y dueño.
- **P15.8 — Cambios respecto de V2.**
  - Nuevos por OPEN A: `UnterminatedName`, `InvalidQualifier`, `ReservedName`, `NameRequired`,
    `QualifiedNameMismatch` y `OperatorInName`.
  - Por OPEN B: `ArityMismatch` pasa a llamarse `InvalidArguments`, igual al escribir y al leer.
  - Por A-07: `NonCanonicalForm`, semántico.
  - Por A-03 modificada: `OutOfRange` **se conserva** para la raíz de una variable, pero solo al escribir. El
    Architect proponía quitarlo de las variables; V3 no lo adopta para las escrituras, por la regla vinculante de
    §0.2. Al leer ninguna variable recibe `OutOfRange`, lo que coincide con la clasificación de OPEN B (§5.1).
- **P15.9 — Lo estructural no es un diagnóstico por símbolo** **[V3 · OPEN B]**. Da un resultado de nivel registro
  (`PresentButUnreadable`) o `FatalMalformedReference` en el rack. Su motivo tipado conserva `UnknownKind` y
  `UnreadableVariableId` y gana `MalformedExpression`, con submotivo: nodo, namespace, función o unidad desconocidos;
  campo desconocido o repetido; campo obligatorio ausente; número no finito; límite superado; dos autoridades. Los
  nombres son ilustrativos.

### P16 — EvaluationResult

**Base.** El target acreditado tiene hoy forma de literal (`VariableTargetSnapshot`, Discovery §4.1 y §4.4).

- **P16.1 — Por símbolo**: `EvaluationResult { SymbolId, Outcome = Success | Failed, Value, Diagnostics, Trace }`.
  `Value` es un `double` finito que solo existe con `Success`; acceder a él en otro caso **lanza**, con la misma
  disciplina que `LiteralValue`. `Diagnostics` no está vacío si y solo si `Failed`. **Sin** campo de tipo (DR-5).
- **P16.2 — Por snapshot**: `RegistryEvaluation { resultados por símbolo, grafo directo e inverso, ciclos, orden de
  evaluación }`, inmutable.
- **P16.3 — Traza opt-in** (P27): definición enlazada, dependencias directas con el valor que tenían y cadena de
  causas de un fallo. Tamaño acotado, solo en memoria y desactivable sin cambiar ningún resultado.
- **P16.4** **[V3 · A-14]** — Los consumidores leen `Value` solo tras comprobar `Success`. El target acreditado
  **expone** el resultado de `RegistryEvaluation` para su variable; no lleva una evaluación propia.
- **P16.5** — Una variable literal da `Success(literal)` con traza trivial.
- **P16.6 — Por propiedad de rack**: el resolver produce, de forma transitoria, un resultado equivalente por
  `(RackId, PropertyId)`. Lo consumen la resolución, el preflight, `InspectBinding` y el texto de estado del editor.

### P17 — persistence

**Base.** El registro es un `Xrecord` troceado en el NOD bajo `RACKCAD_PROJECT`, con propiedades en PascalCase,
`PropertyNameCaseInsensitive` y `JsonStringEnumConverter` (`ProjectVariablesStore.cs:249-262`; Discovery §5.1). Los
bindings del Selectivo viven en el mapa opcional `PropertyValues` del portador authored (Discovery §5.1). El store del
registro rechaza todo kind distinto de `literal` (Discovery §5.3), y `SelectivePropertyValueDocument` declara que un
kind desconocido «is NOT ignored and does NOT fall back to the literal»
(`src/RackCad.Application/Persistence/SelectivePropertyValueDocument.cs:17-20`).

- **P17.1** — Cambian dos contratos persistidos: la **definición** en el registro y la **fuente** en `PropertyValues`.
  El almacenamiento físico de ambos no cambia.
- **P17.2 — Definición de variable: `literal | expression`.**

  ```json
  {"Kind":"literal","Value":6.0}
  {"Kind":"expression","Expression":{"Node":"add",
    "Left":{"Node":"ref","Namespace":"projectVariable","Id":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"},
    "Right":{"Node":"ref","Namespace":"projectVariable","Id":"8c1d7e20-4b5a-4c6d-9e7f-102132435465"}}}
  ```

  El literal se persiste **exactamente** como hoy. `expression` va **sin** `Value`.
- **P17.3 — Fuente de propiedad: `literal | projectVariable | expression`.**
  - **literal**: **sin** entrada en `PropertyValues`; el número es el campo authored, como hoy.
  - **projectVariable**: `{"Kind":"projectVariable","VariableId":"<guid>"}`, **sin cambio**.
  - **expression**: `{"Kind":"expression","Expression":<nodo>}`, **sin** `VariableId`.

  ```json
  "PropertyValues":{
    "selective.verticalClearance":{"Kind":"projectVariable","VariableId":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"},
    "selective.palletTolerance":{"Kind":"expression","Expression":{"Node":"add",
      "Left":{"Node":"ref","Namespace":"projectVariable","Id":"3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44"},
      "Right":{"Node":"number","Value":100,"Unit":"mm"}}}}
  ```

  Mientras exista una entrada, el literal authored queda **congelado e inactivo**, igual que con una referencia
  (ADR-0034 §5; regla 20.13 de I-48).
- **P17.4 — Esquema de nodos**, común a las dos superficies:

  ```json
  {"Node":"number","Value":2.5}
  {"Node":"number","Value":100,"Unit":"mm"}
  {"Node":"ref","Namespace":"projectVariable","Id":"<guid>"}
  {"Node":"neg","Operand":{}}
  {"Node":"add","Left":{},"Right":{}}
  {"Node":"call","Function":"MAX","Args":[{},{}]}
  ```

  `sub`, `mul` y `div` tienen la forma de `add`; `{}` señala un nodo anidado. El número se persiste **tal como se
  escribió**, con su unidad: la conversión ocurre al evaluar (P9.3), así el texto editable conserva `100[mm]`.
- **P17.5 — El valor evaluado NO se persiste**, ni en el registro ni en el rack. Se deriva en cada snapshot (DR-3).
  Un valor cacheado sería una segunda autoridad capaz de quedar obsoleta, y no ayuda a las builds anteriores, que
  rechazan el kind (P18.2).
- **P17.6 — Validación ESTRUCTURAL** **[V3 · OPEN B · A-07 · A-08]**. La build no entiende el significado. En el
  registro, cualquier caso da `PresentButUnreadable` para el registro entero, como hoy con un kind desconocido; en el
  rack, el binding es `FatalMalformedReference` y el rack queda `Blocked`, sin reparación destructiva (§5):
  1. `Kind` desconocido, en la definición o en la fuente (sin cambio, C4-8);
  2. **dos autoridades** en la misma entrada: `expression` con `Value` o `VariableId`, o sin `Expression`;
     `literal` o `projectVariable` con `Expression`;
  3. `Node` o `Namespace` desconocido persistido;
  4. token de `Function` desconocido persistido;
  5. token de `Unit` desconocido persistido, o `Unit` en un nodo que no es `number`;
  6. id o AST mal formado: `Id` no parseable, campo obligatorio ausente, número no finito, o límites normativos de
     P1.9 superados;
  7. **payload de Expression con mundo cerrado** **[V3 · A-08]**: cualquier campo desconocido, o repetido con la
     comparación de nombres vigente, dentro de:
     - la definición de variable cuyo `Kind` es `expression`, en todo su objeto;
     - la entrada de `PropertyValues` cuyo `Kind` es `expression`, en toda la entrada;
     - cualquier nodo del árbol, a cualquier profundidad.

  **Ya no es estructural** la forma no canónica: pasa a ser el error semántico `NonCanonicalForm` (P2.8, A-07).
- **P17.7 — Los errores semánticos NO son errores de lectura.** `BrokenReference`, `Cycle`, `InvalidArguments`,
  `DivisionByZero`, `NonFiniteResult`, `DependencyFailed`, `NonCanonicalForm` y el `OutOfRange` del consumidor se
  detectan al evaluar o al resolver, dan un resultado por símbolo o por fuente y dejan el registro estructuralmente
  `Usable` (§5).
- **P17.8** — Leer nunca escribe (Discovery §5.7). Solo escriben una `RegistryMutation` o una `RackMutation` tras
  un preflight, o el reconciliador de RACKEDITAR.
- **P17.9** — Los números se serializan con `System.Text.Json` por defecto, así que `6.0` puede reescribirse como `6`,
  igual que hoy (Discovery §5.1).
- **P17.10 — Tablas de tokens y comparadores** **[V3 · A-17]**. Cada token persistido pertenece a una tabla
  **cerrada y explícita**, con sus dos sentidos declarados y fijados por prueba:

  | Token | Valores | Comparación al leer |
  |---|---|---|
  | `Kind` de definición | `literal`, `expression` | `OrdinalIgnoreCase`, como `literal` hoy |
  | `Kind` de binding | `projectVariable`, `expression` | `Ordinal`, como `projectVariable` hoy (Discovery §13.3) |
  | `Node` | `number`, `ref`, `neg`, `add`, `sub`, `mul`, `div`, `call` | `Ordinal` |
  | `Namespace` | `projectVariable` | `Ordinal` |
  | `Unit` | `mm`, `in`, `ft` | `Ordinal` |
  | `Function` | `MIN`, `MAX`, `ABS` | `Ordinal` |

  - Los nombres de propiedad JSON siguen sin distinguir mayúsculas (`PropertyNameCaseInsensitive`, Discovery §5.1).
    Un nombre repetido dentro de un payload de Expression es estructural (P17.6).
  - El `JsonStringEnumConverter` de los stores (`ProjectVariablesStore.cs:249-262`;
    `SelectivePalletDesignStore.cs:67-76`) **no** puede producir ningún token de Expression.
  - **Rama que I-49 reescribe.** El `Add` del registro escribe hoy `Type = Type.ToString()` (`MutationPlan.cs:100`),
    y el mapping único compara contra el nombre del miembro (`VariableType.cs:89`). Al extender esa rama, I-49
    escribe y lee `Type` desde una tabla explícita del **mismo** mapping único.
    - El lenguaje aceptado no cambia: el nombre declarado, sin distinguir mayúsculas, sin forma numérica y sin
      listas (`VariableType.cs:64-71`).
    - La salida es byte a byte `"Length"`.
    - Es preservación de la invariante de un solo mapping (decisiones de I-48 §4), no un arreglo lateral.
  - `Id`: el parseo y la igualdad vigentes de `VariableId` (Discovery §4.1).
- **P17.11 — Una sola autoridad de interpretación en el rack.** La lectura de una fuente `expression` vive en
  `LinkedPropertyInspection.InspectBinding` (Discovery §4.1), no dispersa por el portador authored ni por el Plugin.
  `SelectivePalletDesignDocument.cs` no cambia (§12).
- **P17.12 — Política histórica de `ExtensionData` fuera de los payloads de Expression** **[V3 · A-08]**. **No** se
  elimina de forma global. La raíz del registro, la entrada de variable, la definición `literal`, el binding
  `projectVariable`, el portador Selectivo y el sobre conservan su tolerancia y su preservación de campos
  desconocidos, exactamente como hoy (Discovery §5.4, §5.8). El mundo cerrado de P17.6.7 rige **solo** dentro de los
  payloads de Expression.

### P18 — schema evolution

> **CLOSED: V-0.** Sin cambio de versión en `ProjectVariablesDocument` ni en el diseño Selectivo (§6).

- **P18.1 — Política real** (hechos con fuente):
  1. **Un major protege a las builds que leerían MAL.** La línea 2.0 del Selectivo existe para que una build
     anterior «REFUSES to open it… instead of silently unbinding the rack»
     (`SelectivePalletDesignDocument.cs:26-32`; decisiones de I-47, C4-9).
  2. **Un minor mayor del mismo major se lee y se preserva** al re-guardar, con sus campos desconocidos (C4.6-8,
     C4-9; `SchemaVersionPolicy.ResolveWriteVersion`, Discovery §5.4).
  3. **Un kind discriminado hace de un caso nuevo algo que NO rompe el schema**, y un kind desconocido nunca se
     ignora. Binding: «Discriminating from the first day makes that a new case instead»
     (`SelectivePropertyValueDocument.cs:11-20`). Definición: kind desconocido = ERROR, «ID22B decidirá su
     evolución» (C4-8).
  4. **Monotonía, sin oscilación por contenido**: la versión sube y nunca baja; decidir el major por el contenido
     del momento se rechaza expresamente (`src/RackCad.Application/Persistence/SelectiveDesignSchema.cs:6-19`).
- **P18.2 — Qué hace hoy una build I-47/I-48** ante lo que V3 persiste (hechos del código en `a4d88f1`):

  | Dato nuevo | Comportamiento de la build anterior | Evidencia |
  |---|---|---|
  | Registro con una definición `expression` | Registro **entero** `PresentButUnreadable`; RACKEDITAR de todo Selectivo, RACKVARIABLES y RACKBOMTOTAL bloqueados | `ProjectVariablesStore.cs:193-197`; Discovery §5.3 |
  | Rack con una fuente `expression` | `FatalMalformedReference(UnknownKind)`; el resolver da `UnknownReferenceKind`; la sonda da `Indeterminate` y el descubrimiento **aborta** | `BindingInspection.cs:205`; `SelectiveEffectiveDesignResolver.cs:174-176`; `ProjectVariableConsumerProbe.cs:74,83`; `ProjectVariableConsumerDiscovery.cs:122` |

  Ninguna lee **mal**: las dos fallan cerradas en la entrada o fuente exacta. Coste real: en una build anterior, un
  rack con expresión bloquea las operaciones de variables que necesitan descubrimiento en **todo** el dibujo
  (INFERENCE sobre el código citado).
- **P18.3 — Decisión: V-0** **[V3 · A-11]**. Se justifica **por el comportamiento de los lectores**:
  - las builds I-47/I-48 ya rechazan el discriminador desconocido y no leen mal la semántica nueva (P18.2), así que
    un major no protege nada (política 1);
  - las expresiones son casos nuevos de discriminadores diseñados para eso (política 3);
  - con el mundo cerrado de P17.6.7, todo lo nuevo **dentro** del payload también falla cerrado por estructura
    (P18.8).
- **P18.4 — Por qué no V-1** (minor `1.1` / `2.1` por contenido). Ningún lector cambia de comportamiento: solo añade
  una etiqueta *sticky*, que L2 borraría si no se corrigiera (`LinkedPropertyReconciler.cs:218`). El argumento de V2
  de que V-1 obligaba a tocar `SelectivePalletDesignDocument.cs` era incorrecto: la constante podría vivir en
  `SelectiveDesignSchema.cs` (A-11).
- **P18.5 — Por qué no V-2** (major). Dejaría un dibujo fuera del alcance de las builds anteriores **para siempre**,
  aunque ya no quedara ninguna expresión, sin evitar ninguna lectura errónea.
- **P18.6 — Condiciones obligatorias de V-0** (§6.2):
  1. payload de Expression con mundo cerrado (P17.6.7);
  2. semántica nueva de Expression solo por discriminadores o tokens cerrados que la build conozca, o por un major
     que obligue a rechazar (§6.3);
  3. los escritores de I-49 **nunca** degradan ni fijan una versión: la calculan con la autoridad vigente (P23.15);
  4. pruebas de caracterización del comportamiento heredado **antes** de modificar los stores (G8, paso 1).
- **P18.7 — Qué NO justifica V-0.** El número de archivos tocados no es un argumento. Tampoco el precedente de I-50
  sobre el mismo portador, que V3 registra solo como hecho de coordinación (§12).
- **P18.8 — Futuro.** Todo lo nuevo dentro del payload de Expression —nodos, unidades, funciones, namespaces o
  campos— es fail-closed para esta build (P17.6), con cualquier versión. Cada adición exige revisión y, si cambia
  persistencia, ADR.
- **P18.9 — Mensajes.** Una build anterior muestra «declara una definición de clase desconocida» o una referencia de
  kind desconocido, no «versión más nueva» (Discovery §5.6). Es un coste de comunicación, no de integridad, y se
  documenta en el ADR y al usuario.

### P19 — rename

**Base.** `Rename` es registry-only: sin barrido, sin racks y sin redibujo (Discovery §8.1; decisiones de I-47
C4.6-7). Hoy valida reconstruyendo un literal (Discovery §5.5, fila 10).

- **P19.1** — Sigue siendo registry-only.
- **P19.2** — Ni las definiciones ni las fuentes de propiedad cambian, porque refieren ids. El formatter muestra el
  nombre nuevo de inmediato (P4).
- **P19.3** — La validación de `Rename` conserva la definición vigente, literal o expresión, en lugar de reconstruir
  un literal.
- **P19.4** **[V3 · OPEN A]** — Las reglas de nombre no cambian: no en blanco, homónimos legales y sin política de
  unicidad, mayúsculas ni caracteres (Discovery §13.1).
  - Un rename puede volver **ambiguo** un nombre para escrituras futuras. El formatter pasa a cualificarlo (P4.7) y
    `AmbiguousName` solo afecta a textos que se tecleen después.
  - Renombrar a una palabra reservada, como `MIN`, está permitido: el formatter la muestra con llaves.
  - Nunca cambia el significado de nada persistido.
- **P19.5** — El requisito «rename no rompe» de OPEN A se cumple por construcción.
- **P19.6** — Un rename no redibuja ningún rack, aunque lo consuma una expresión: su efectivo no cambia.
- **P19.7** **[V3 · OPEN B]** — `Rename` está permitido siempre que el registro sea legible y acreditado, también con
  errores semánticos: no cambia ninguna evaluación.

### P20 — delete

**Base.** `Delete` solo cuenta consumidores propiedad→variable directos, y `UnlinkAllAndDelete` materializa los
consumidores directos (`ProjectVariableMutationPreflight.cs:220-298`; Discovery §8.1, §8.2). ADR-0034 §11 rechaza
borrar materializando automáticamente.

- **P20.1 — Consumidores de X**: toda propiedad cuya fuente sea `ProjectVariableReference(X)` **o** una `Expression`
  cuyas dependencias (P11) incluyan X.
- **P20.2** — `Delete(X)` queda **bloqueado** si X tiene consumidores **o** dependientes variable→variable (P12). Se
  informan ambas listas. Sin cascada y sin materializar nada.
- **P20.3** — Los dependientes salen del snapshot acreditado **sin** barrer el dibujo (Discovery §6.3). Los
  consumidores salen del barrido.
- **P20.4 — `UnlinkAllAndDelete(X)`** **[V3 · A-02]** (§7.1):

  | Consumidor o dependiente de X | Resultado |
  |---|---|
  | Referencia directa `ProjectVariableReference(X)` | Se materializa **como hoy**: el efectivo pasa a literal y se quita el binding |
  | Expresión de propiedad que depende de X | **BLOQUEADO** |
  | Definición de variable que depende de X | **BLOQUEADO** |

  - Sin conversión implícita de una fórmula en literal y sin sustituir el valor de X dentro de la fórmula.
  - El bloqueo informa los dependientes (variable y definición formateada) y los consumidores por expresión (rack,
    propiedad y fórmula canónica).
  - Si solo hay referencias directas, cada rack materializado tiene que resolver después (regla R1; hoy
    `ProjectVariableMutationPreflight.cs:287`).
- **P20.5** — Borrar una variable definida por expresión, sin consumidores ni dependientes, está permitido y es
  registry-only.
- **P20.6** — Resultados tipados `BlockedByConsumers`, `BlockedByDependents` y `BlockedByExpressionConsumers`, con sus
  listas. Los nombres son ilustrativos.
- **P20.7** **[V3 · OPEN B]** — Con errores semánticos en el registro rigen las mismas reglas.
  - Se puede borrar una variable en error que no tenga consumidores ni dependientes: eso quita un error y no
    escribe ninguno (R3).
  - No se puede borrar un miembro de ciclo, porque tiene dependientes.
  - Un borrado **nunca** fabrica una `BrokenReference`: los consumidores y dependientes lo bloquean antes.

### P21 — propagation

**Base.** Traza vigente de `ChangeValue` en 14 pasos (Discovery §6.1). La profundidad 1 vive en el descubrimiento, el
plan, la resolución, `Delete` y RACKVARIABLES (Discovery §6.2). `MutationDestinationBinding` aborta si una definición
aparece dos veces (Discovery §6.3). RACKVARIABLES hace hoy un descubrimiento por variable al abrir (Discovery §7).

- **P21.1** — `ChangeDefinition(X, d)` generaliza `ChangeValue` a las cuatro transiciones de una definición:
  literal→literal, literal→expresión, expresión→literal y expresión→expresión.
- **P21.2 — Preflight** (puro, todo o nada), con la cadena que exige el Owner:
  1. acreditar el registro «antes», `LastRegistry` (sin cambio);
  2. **validar la forma enlazada** que trae el intent, nunca texto (P7.7): todo id existe en «antes»; aridad, ámbito
     y forma canónica correctos;
  3. «después» = aplicar la mutación a un clon del documento acreditado, acreditarlo y evaluarlo
     (`RegistryEvaluation`, P14);
  4. **cierre**: `I = {X} ∪ dependientes transitivos de X` en el grafo «después», en el orden de P11.1;
  5. **regla R2** (P21.3): X da `Success`; ningún símbolo que daba `Success` en «antes» falla en «después»; un
     símbolo que ya fallaba y cuya definición no cambia no bloquea. Un ciclo que pase por X da `Cycle` (P13.3);
  6. **contrato raíz** (P8.10): la raíz de X cumple `> 0`, y toda raíz de `I` que cumplía antes cumple después;
  7. **consumidores de rack de cualquier variable de `I`**: referencias directas **y** expresiones de propiedad
     cuyas dependencias toquen `I`, sobre **una** proyección del barrido y deduplicados por `RackId` (P21.4);
  8. **regla R1**: cada rack se resuelve **una sola vez** contra el estado final, con todas sus propiedades, sus
     expresiones evaluadas y el dominio de cada propiedad (P24.5). Si alguno no resuelve, el plan queda vacío. Si
     el rack ya estaba afectado antes, el mensaje indica «repara ese rack primero» (§7.2);
  9. **`PlanReadSet`** (P21.6) y sus valores esperados, tomados de «después»;
  10. **un `MutationPlan` coherente**: **una** `RegistryMutation` (en el registro solo cambia X) + **una**
      `RackMutation` por rack, con efectivo completo y todas sus vistas.
- **P21.3 — Reglas de intents correctivos** **[V3 · OPEN B]** (§5.2, decisión 6):
  - **R1** — Todo rack que consuma el cierre tiene que resolver después; si no, plan vacío. Es la regla de I-47/I-48,
    sin cambios.
  - **R2** — El símbolo mutado evalúa bien; un símbolo sano no puede fallar después; los que ya fallaban, con su
    definición intacta, no bloquean.
  - **R3** — Ningún intent escribe un error nuevo.

  Con R1, R2 y la reparación (§7.2) siempre hay salida, también con dos errores dentro de un ciclo.
- **P21.4 — Descubrimiento por conjunto.** La semántica de la sonda no cambia: tri-estado, `Indeterminate` aborta,
  positivos parciales abortan y se exige autoridad `Single` (Discovery §7).
  - Una fuente `expression` estructuralmente legible es positiva si sus dependencias (P11) cortan `I`, **aunque**
    falle al evaluar.
  - Un kind desconocido o un payload estructuralmente mal formado es `Indeterminate`.
  - Se calcula **una vez** sobre el barrido y tiene que ser **equivalente** a la unión de descubrimientos por variable;
    esa equivalencia se prueba como oráculo (P28). Evita el coste por variable del riesgo R5.
- **P21.5 — Una transacción, un commit, un `Regen`.** La estructura del ejecutor no cambia (PREPARE / MUTATE / POST;
  Discovery §6.1) y **no** hay regen por variable intermedia (REQUISITO DEL OWNER). El ejecutor **no** parsea ni
  evalúa (guarda G13 extendida, P28.4).
- **P21.6 — `PlanReadSet` y re-lectura de commit** **[V3 · A-05]**. V2 afirmaba que el efectivo de cada rack
  dependía solo de los valores de `I` y de su authored. Es falso: un rack planificado puede leer variables fuera de
  `I`.
  - **Contenido contractual del `PlanReadSet`**:
    1. el cierre `I` de variables mutadas y sus dependientes;
    2. todo símbolo que leen los racks planificados, en **todas** sus propiedades: referencias directas y
       dependencias de sus expresiones;
    3. las dependencias transitivas de todo lo anterior.
  - El plan transporta el **valor esperado** de cada símbolo del `PlanReadSet`, tomado de la evaluación «después».
  - **Secuencia en commit**, dentro de Application (`RegistryCommit.Prepare`, Discovery §3.1), no en el Plugin:

    ```text
    lastRead   = Read(...)
    si lastRead no es estructuralmente legible:          ABORT BEFORE WRITE
    accredited = Accredit(lastRead)                       // sin cambio (V8-R01); si falla: ABORT BEFORE WRITE
    si algún id que el plan usa ya no está:               ABORT BEFORE WRITE
    changed    = ApplyTo(accredited.Document)             // sin cambio
    evaluated  = Evaluate(changed) sobre el PlanReadSet   // NUEVO
    si algún símbolo del PlanReadSet falla,
       o su valor difiere del esperado (double.Equals):   ABORT BEFORE WRITE   // registro 0, vistas 0
    si no:
       TryWrite(..., lastRead, changed, ...)              // sin cambio
    ```

  - **También sin `RegistryMutation`.** Hoy el ejecutor solo re-lee el registro cuando el plan lo escribe
    (`src/RackCad.Plugin/ProjectVariableMutationExecutor.cs:252-275`). En V3, todo plan cuyos racks lean símbolos,
    como una reparación que deja fuentes sanas, re-lee, re-acredita y valida su `PlanReadSet` en la misma
    transacción **antes** de escribir la primera vista, aunque no escriba el registro. La lógica vive en Application;
    el ejecutor solo la invoca.
  - Los errores **fuera** del `PlanReadSet` no bloquean ese commit (§5.2, decisión 9).
  - Una re-lectura que cambió en otro sitio pero conserva todos los valores del `PlanReadSet` **no** se rechaza. La
    política de V8-R05 queda intacta para lo que ya existía, incluido el authored de los racks: el bucle de
    RACKVARIABLES es modal y re-lee en cada vuelta (Discovery §10.1).
  - **Caso de A-05 fijado por prueba**: en el rack R, `verticalClearance = X` (X está en `I`) y
    `palletTolerance = Y + 1` (Y no). Si Y cambia entre preflight y commit, el commit aborta antes de escribir.
- **P21.7 — Tres fronteras de validez en el preflight** **[V3 · A-03 modificada]** (P5.7):
  - **motor**: acepta cualquier `double` finito, también intermedios negativos o nulos;
  - **adaptador de Project Variables**: el contrato raíz del `VariableType` (P8.10), igual para literal y expresión;
  - **consumidor**: el dominio de cada propiedad sobre el efectivo de sus fuentes (P24.5), declarado por el
    descriptor **como dato**, nunca como callback.

  Si una frontera falla: `OutOfRange` con su dueño y plan vacío.
- **P21.8 — `Create` con expresión.** Sigue siendo registry-only, porque una variable nueva no tiene consumidores ni
  dependientes. Pero su preflight recibe y acredita el registro para validar la forma enlazada, evaluar y aplicar el
  contrato raíz, algo que hoy `Create` no hace (Discovery §8.1).
- **P21.9 — Materialización** **[V3 · A-12]**. Solo materializan un efectivo `UnlinkAllAndDelete`, que ya solo lo hace
  con referencias directas (§7.1), y la exportación a biblioteca (P24.9). Ambos escriben el **valor evaluado exacto**,
  sin redondear, así que el efecto geométrico sigue siendo nulo; se acepta el ruido de coma flotante del riesgo R16.
  La transición expresión→literal del reconciliador **no** materializa: escribe el literal tecleado
  (`LinkedPropertyReconciler.cs:204-210`), como hoy desde una referencia. V2 decía lo contrario en P21.9.
- **P21.10 — `RepairBrokenRack`** **[V3 · A-01]**. Una sola operación, rack-scoped y explícita, con el conjunto
  reparable ampliado a los fallos semánticos y confirmación consciente de la fuente (§7.2).
- **P21.11** — Sin límite de profundidad más allá de la aciclicidad y de P1.9, y **nunca** K planes sucesivos
  (Discovery §2.2, pregunta 16).
- **P21.12 — Edición desde RACKEDITAR.**
  - Cambiar la fuente de una propiedad no toca el registro ni propaga a otros racks, porque una propiedad no es un
    símbolo.
  - El reconciliador resuelve **ese** rack una vez, contra el snapshot del comando (P7.7), sobre estado aislado
    (P23.16), y escribe de forma atómica.
  - No se añade re-lectura: la ventana es modal y la política V8-R05 no cambia.

### P22 — RACKVARIABLES UX

**Base.** Comando y bucle con re-lectura en cada vuelta (Discovery §10.1). Workspace y filas con `LiteralValue`
(Discovery §10.2). Lista sin id, campos «Nombre» y «Valor (in)» y panel de referencias rotas fuera del panel
editable (Discovery §10.3). Intents con `double` (Discovery §10.4). Todo lo que asume literal, `Length` y
consumidores directos (Discovery §10.5).

- **P22.1** — **Ni comando nuevo ni ventana nueva**: los censos de 33 comandos y 29 ventanas no cambian (guardas G15
  y G16, Discovery §14.3).
- **P22.2 — Fila de la lista**: nombre · tipo · definición (literal o texto del formatter) · valor evaluado
  (`"0.###"`) **o** código de diagnóstico · consumidores (directos y por expresión) · dependientes.
- **P22.3 — Un solo campo «Definición (in)»** en lugar de «Valor (in)»:
  - un texto que **no** empieza por `=` es un literal, parseado con la regla de ADR-0015 y `> 0`, exactamente como
    hoy;
  - un texto que empieza por `=` es una expresión (P1), con unidades si hacen falta: `=AlturaBase + Holgura`,
    `=100[mm] + Base`;
  - **[V3 · A-18]** si la expresión se canoniza a literal (`=6`, `=0`, `=-2`), rige la misma regla `> 0` que para el
    literal tecleado;
  - **[V3 · A-03 modificada]** si es una expresión, su raíz cumple el mismo `> 0` (P8.10): `=AlturaA - AlturaB` con
    raíz `-2` se rechaza igual que `-2`.
- **P22.4 — Diagnóstico en vivo.** La ventana llama a una función pura de Application, expuesta por el DTO del
  workspace, que parsea y enlaza contra el **snapshot del workspace** y devuelve diagnósticos con posiciones. La
  ventana los muestra y **no** emite el intent mientras haya errores. La ventana **nunca** evalúa con autoridad,
  **nunca** resuelve nombres por sí misma y **no** referencia tipos del núcleo (guarda, P28.4).
- **P22.5 — Selector de referencias** **[V3 · OPEN A]**. Una lista con nombre, valor evaluado y desambiguador, igual
  que los candidatos del editor (Discovery §13.2), inserta en el cursor la **forma del formatter** del `VariableId`
  elegido: `Nombre`, `{Nombre}` o, con homónimos, la forma cualificada. **Nunca** auto-selecciona por el nombre
  tecleado. La forma cualificada también se puede teclear o pegar.
- **P22.6 — Detalle de la variable seleccionada**: «Depende de» (dependencias directas) y «La usan» (variables
  dependientes directas, más los racks y propiedades que la consumen, directamente o por expresión).
- **P22.7 — Intents con forma enlazada** **[V3 · A-04]**.
  - `Create(nombre, definiciónEnlazada)` y `ChangeDefinition(id, definiciónEnlazada)`. La definición enlazada es un
    resultado opaco de Application: un literal o un `BoundExpression` con ids, producido por el binder puro contra
    el snapshot del workspace.
  - `Rename`, `Delete`, `UnlinkAllAndDelete` y `RepairBroken` conservan su payload.
  - El preflight valida esos ids contra `LastRegistry`, la misma lectura (P21.2), y el commit los vuelve a validar
    junto con el `PlanReadSet` (P21.6).
  - **Nunca** se vuelve a resolver texto humano contra una lectura posterior. Si una vuelta del bucle re-lee el
    dibujo, la ventana nueva se construye con el workspace nuevo, y el texto se enlaza solo contra el snapshot de la
    ventana donde se escribió.
- **P22.8** — Los fallos de preflight (`Cycle`, `BlockedByDependents`, `BlockedByExpressionConsumers`, `OutOfRange`,
  `BrokenReference`, etc.) usan el mecanismo vigente del bucle (Discovery §10.1).
- **P22.9 — Panel de diagnóstico y reparación** **[V3 · OPEN B · A-10]**. Queda fuera del panel editable, como hoy
  «Referencias rotas» (Discovery §10.3), y muestra:
  - las variables en error, con su causa raíz, los miembros y la ruta de cada ciclo y sus dependientes;
  - las fuentes de rack afectadas: rack, propiedad, tipo de fuente, fórmula canónica y causa;
  - la reparación por rack con la confirmación consciente de la fuente de §7.2;
  - con el registro estructuralmente ilegible, el bloqueo de hoy, sin escribir nada.

  RACKVARIABLES abre siempre que el registro sea estructuralmente legible y acreditado (§5.2, decisión 5).
- **P22.10** — Pruebas de UI sin interfaz, con ids reales del catálogo y sin un `MessageBox` de producción alcanzable
  en los flujos probados.

### P23 — LinkedPropertyEditor UX

**Base.** Toda la semántica de edición vive en `LinkedPropertyEditSession`, pura, y el control solo pinta, lista,
mueve la selección y reporta el foco (Discovery §9.1). Reglas vigentes: Enter compromete literales y rechaza
consultas; LostFocus compromete solo si no cambia la fuente; `TrySelect` es la única vía de una referencia; C4 no
convierte un cambio de fuente en commit (`LinkedPropertyEditSession.cs:253-357`; Discovery §9.3–§9.4). Nueve
ubicaciones asumen que todo `=` es consulta (Discovery §9.6).

- **P23.1 — El mismo control** (REQUISITO DEL OWNER). El `LinkedPropertyEditor` de I-48 acepta en el **mismo**
  campo:

  ```text
  6
  =Holgura
  =Holgura + 2
  =(AlturaBase + Holgura) / 2
  ```

  **No** se crea un `FormulaTextBox` ni ningún control paralelo. La semántica sigue entera en la sesión pura.
- **P23.2 — Garantías que se preservan** (REQUISITO DEL OWNER):

  | Garantía | Cómo se preserva |
  |---|---|
  | texto pendiente ≠ estado comprometido | El borrador y `LinkedPropertyEditState` siguen separados |
  | teclear no muta el dominio | `Type` solo cambia el borrador: ni intent ni mutación |
  | Escape restaura el comprometido | `Cancel` vuelve exactamente al estado comprometido, como hoy |
  | LostFocus no cambia implícitamente la fuente | P23.8 |
  | C4 valida todos los borradores antes de aplicar alguno | P23.9 |
  | commit explícito parse → bind → validate → evaluate → intent → mutación atómica | P23.6 |

- **P23.3 — Modelo de fuente**: `Literal | ProjectVariableReference(VariableId) | Expression(BoundExpression)`
  (P2.7). `CommittedLiteral` sigue siendo el último literal **comprometido** y queda congelado mientras la fuente no
  es literal (regla 20.13, Discovery §9.1).
- **P23.4 — Clasificación del borrador.** `DraftReferenceQuery` se sustituye por `DraftExpression`:

  | Texto | Clase |
  |---|---|
  | Sin cambios | `Committed` |
  | Número válido, con la regla vigente del editor (P1.11) | `DraftLiteral` |
  | Empieza por `=`, tras recortar espacios iniciales como hoy | `DraftExpression` |
  | Cualquier otro | `InvalidDraft` |

  Un `DraftExpression` puede parsearse y enlazarse bajo demanda para diagnosticar y ofrecer candidatos: es puro y no
  muta nada.
- **P23.5 — Gestos.**

  | Gesto | Resultado |
  |---|---|
  | Teclear | Borrador; ninguna mutación; diagnóstico en vivo puro |
  | Lista de candidatos | Se ofrece cuando el cursor está sobre un fragmento de referencia de un borrador `=` (P23.7) |
  | Seleccionar un candidato con el borrador = `=` + fragmento | Compromete `ProjectVariableReference(X)`, **como hoy** (`TrySelect`) |
  | Seleccionar un candidato dentro de una expresión mayor | Inserta la forma del formatter de X, **sin** comprometer |
  | Enter con un candidato navegado | Lo selecciona, como hoy (Discovery §9.3) |
  | Enter con `DraftLiteral` | Compromete el literal, también desde una referencia o una expresión, como hoy |
  | Enter con `DraftExpression` | Pipeline P23.6: si pasa, compromete la forma canónica; si no, rechaza con diagnósticos y conserva el borrador |
  | Escape | Vuelve exactamente al estado comprometido |
  | Perder el foco | P23.8 |
  | Frontera C4 | P23.9 |

- **P23.6 — Pipeline de commit explícito** (REQUISITO DEL OWNER):
  1. **parse** (P1): diagnósticos de sintaxis, nombres y unidades;
  2. **bind** (P7) contra el snapshot de la sesión, que es el registro acreditado del comando y su evaluación,
     transportados por las opciones de apertura (Discovery §9.1). Un nombre exacto y único enlaza; uno parcial,
     desconocido, ambiguo o reservado se rechaza (§4);
  3. **validate** (P7.5): existencia, aridad, unidad, ámbito, forma canónica y compatibilidad de tipo de una
     referencia directa (P8.7);
  4. **evaluate** (P14) sobre los valores de `RegistryEvaluation` del mismo snapshot, con el dominio de la propiedad
     (P24.5). Es aviso temprano y presentación; la autoridad es el paso 7 (P14.9);
  5. **canonicalizar** (P2.8) y comprometer en la sesión, **sin** mutar el dominio. **[V3 · A-18]** Un literal
     canónico sigue la ruta histórica del literal tecleado;
  6. **intent**: la frontera C4 de la ventana entrega los estados finales (`LinkedPropertyFinalStates`, Discovery
     §9.4);
  7. **mutación atómica**: el reconciliador trabaja sobre estado aislado (P23.16). Re-comprueba y re-evalúa el árbol
     **ya enlazado por ids**, sin volver a resolver nombres, contra el snapshot del comando (P7.7). Resuelve el rack
     una vez, calcula la versión con la autoridad vigente (P23.15) y persiste authored, `PropertyValues` y efectivo
     juntos. Cualquier fallo deja **cero** persistencia y **cero** mutación en memoria visible para quien llama.
- **P23.7 — Candidatos e inserción** **[V3 · OPEN A]**.
  - El filtro sigue siendo *contains* `OrdinalIgnoreCase` sobre el fragmento bajo el cursor: filtra, **no** resuelve
    (Discovery §13.3).
  - La lista muestra nombre, valor evaluado y desambiguador obligatorio (Discovery §13.2). Los 8 caracteres son solo
    visuales y **nunca** se parsean (P3.9).
  - Solo se ofrecen variables compatibles cuya evaluación es `Success` (P23.11).
  - **Sin auto-selección**: nunca se completa un fragmento parcial ni se elige entre homónimos.
  - El token insertado es la forma del formatter: `Nombre`, `{Nombre}` o la forma cualificada.
- **P23.8 — LostFocus.** Compromete **solo** si el borrador no cambia la fuente, la regla general vigente
  (`LinkedPropertyEditSession.cs:279-300`). Un `DraftLiteral` sobre una fuente literal se compromete. Un
  `DraftExpression` que no enlaza, o cuya forma canónica difiere de la comprometida, se rechaza y el borrador
  sobrevive. Mover el foco a la lista no es perder el foco (Discovery §9.3).
- **P23.9 — Frontera C4.** Dos fases, como hoy: `TryStage` de **todos** los editores y después `ApplyStaged` de
  todos.
  - Un `DraftLiteral` sin cambio de fuente da `Ready`.
  - **Cualquier** borrador que cambie la fuente da `Blocked` con «confirma con Enter», porque una frontera genérica
    nunca convierte un cambio de fuente en commit (`LinkedPropertyEditSession.cs:349-352`). Cuenta como cambio de
    fuente pasar entre literal y expresión, entre referencia y expresión, a otra expresión, o de expresión o
    referencia a literal.
  - Un `DraftExpression` cuya forma canónica es la comprometida da `Clean`.
  - Un bloqueo aborta la frontera entera y enfoca al primer editor culpable (Discovery §9.4).
- **P23.10 — Presentación y efectivo** **[V3 · A-14]**.
  - Literal → `"0.###"`; referencia → `=` + referencia formateada; expresión → `=` + formatter (P4).
  - El efectivo que se muestra de una referencia es el valor de `RegistryEvaluation`, y el de una expresión, su
    evaluación sobre esos valores. Los dos son solo presentación.
  - El texto de estado bajo el campo muestra el valor y, si hay expresión, la fórmula.
- **P23.11 — Opciones.** `LinkedPropertyOptions.ForProperty` ofrece las variables de tipo compatible cuya evaluación
  es `Success`, con su **valor evaluado** y una marca para las definidas por expresión. Las variables en error
  **no** se ofrecen; su diagnóstico vive en RACKVARIABLES (§5.2, decisión 2).
- **P23.12 — Qué cambia respecto del contrato de I-48**, y que el ADR tiene que formalizar (§9):

  | Comportamiento de I-48 | V3 | Pines afectados (Discovery §14.2–§14.3) |
  |---|---|---|
  | Todo `=` es consulta de referencia que solo filtra (`IsQuery`) | `=` abre un borrador de expresión; la lista sigue filtrando fragmentos; teclear sigue sin comprometer nada | `CASO_6_UNA_CONSULTA_DE_REFERENCIA_QUEDA_PENDIENTE_Y_NO_COMPROMETE_NINGUNA_REFERENCIA` (`"="`, `"=H"`, `"=Holg"`, `"=Holgura General"`; `LinkedPropertyEditSessionTests.cs:212-230`): la clase esperada pasa a `DraftExpression`; «queda pendiente y no compromete» se conserva |
  | Enter sobre `=…` siempre se rechaza (`LinkedPropertyEditSession.cs:269-271`) | Enter compromete una expresión que pasa el pipeline; un fragmento parcial, desconocido, ambiguo o reservado sigue rechazándose | Pin nuevo: una expresión válida se compromete con Enter |
  | Ni con Enter se resuelve un nombre tecleado; `TrySelect` es la única vía de una referencia | **[V3 · OPEN A]** Enter resuelve un nombre **exacto y único**, que canoniza a la referencia existente | `CASO_8_ENTER_SIN_CANDIDATO_SELECCIONADO_NO_RESUELVE_POR_TEXTO` (`:249-259`) se **reescribe**: «Enter resuelve un nombre exacto y único; nunca uno parcial ni ambiguo». Con el mismo fixture, `=Holgura General` + Enter compromete su `VariableId` con el literal congelado; `=Holgura` (parcial) y un homónimo se rechazan |
  | `=Holgura General + 2` no se puede comprometer (Discovery §9.6, INFERENCE) | Se compromete si enlaza y evalúa | Pin nuevo |
  | **Se conserva**: sin auto-selección; desambiguador obligatorio; LostFocus sin cambio de fuente; C4 sin conversión silenciosa; Escape; regla 20.13 | Sin cambio | Intactos: `CASO_7_UN_FILTRO_CON_UN_UNICO_CANDIDATO_NO_LO_SELECCIONA` (`:232-247`), porque `=Estrecha` no es un nombre exacto; `CASO_9` a `CASO_11` (`:278-374`), `EL_FILTRO_ES_UN_SUBSTRING_INSENSIBLE_A_CAJA_Y_NO_UNA_RESOLUCION` (`:261-274`) y `:55-89`, `:134` |

- **P23.13 — `RackSelectiveWindow`.** Su `Describe` usa hoy `Text.TrimStart('=')` como nombre de la variable
  (Discovery §9.6). V3 lo sustituye por el texto de estado que produce la sesión: un cambio de un miembro.
  `pendingAll`, C4 y los estados finales no cambian. Es un archivo caliente que I-50 también editará en su G3: antes
  de tocarlo se aplica el procedimiento de colisión (§12).
- **P23.14** — Sin control nuevo: el censo de 29 ventanas no cambia (G16).
- **P23.15 — Transiciones del reconciliador y versión escrita** **[V3 · A-09]** (amplía Discovery §9.4):

  | Transición | Qué escribe |
  |---|---|
  | literal → expresión | `WriteAuthored(literal comprometido)` (congela) + `PropertyValues[p] = expression` |
  | referencia → expresión, expresión → referencia, expresión → otra expresión | Literal congelado conservado + `PropertyValues[p]` = fuente nueva |
  | expresión → literal | `WriteAuthored(b)` y se quita la entrada, como hoy desde una referencia |
  | Cualquiera | El rack se resuelve **una** vez con todas sus propiedades finales |

  **Versión.** Toda rama que I-49 modifique para escribir estado authored del Selectivo calcula la versión con
  `SelectiveDesignSchema.ResolveWriteVersion(stored, hasPropertyValues)`
  (`src/RackCad.Application/Persistence/SelectiveDesignSchema.cs:43-66`) o con la autoridad vigente equivalente.
  **Nunca** fija `"2.0"`. Hoy el reconciliador lo fija sin condición (`LinkedPropertyReconciler.cs:218`, hallazgo
  L2), y un diseño `2.3` volvería a `2.0` al editarlo. Como I-49 reescribe esa rama, sustituirla es **preservación de
  la invariante C4-9**, no deuda lateral. El mismo criterio ya usa `Link` (`ProjectVariableMutationPreflight.cs:356`).
- **P23.16 — Estado aislado en las transiciones nuevas** **[V3 · A-15, elevado a MEDIUM práctico]**.
  - **Hecho.** `WithDesign` comparte la **misma** instancia de `PropertyValues` (`SelectivePalletDesignDocument.cs:321`
    en `a4d88f1`; hallazgo L3). El reconciliador la llama sobre el authored inicial
    (`LinkedPropertyReconciler.cs:146`) y `Apply` la muta (`:206-218`). Hoy un fallo no persiste nada, porque el
    comando aborta (`RackSelectivoCommands.cs:147-150`), pero el objeto que recibió el reconciliador puede quedar
    mutado en memoria.
  - **Regla.** Las transiciones nuevas trabajan sobre estado **aislado**: el reconciliador clona el authored inicial
    por la forma persistida (`ProjectVariableCloning.Clone`, `MutationPlan.cs:235-258`) **antes** de `WithDesign` y
    de `Apply`. Así lo hacen ya `UnlinkAllAndDelete` y `RepairBrokenRack`
    (`ProjectVariableMutationPreflight.cs:272,514`).
    Ningún alias de `PropertyValues` alcanzable por la sesión, la ventana o el comando puede quedar mutado tras un
    preflight o una reconciliación fallidos.
  - **Dos garantías distintas, las dos obligatorias y probadas por separado:**

    | Garantía | Significado | Cómo se prueba |
    |---|---|---|
    | `no persistence happened` | Ni registro ni vistas cambian en el dibujo | Contadores de escritura a cero tras un fallo |
    | `no in-memory mutation happened` | El authored recibido y su `PropertyValues` son idénticos antes y después de un fallo, y un éxito tampoco los altera | Forma serializada antes y después; mutar la salida no altera la entrada |

  - `WithDesign` y `SelectivePalletDesignDocument.cs` **no** cambian: el aislamiento se consigue en la frontera de
    I-49, y el portador sigue siendo de I-50 (§12). L3 se registra en G4 como hallazgo del portador.
  - El camino `WithDesign → From(design)`, del que depende I-50 para transportar sus campos
    (`docs/initiatives/I-50-proposal-v1.2.md:302-304` en su rama), se conserva intacto sobre el clon.

### P24 — direct-reference compatibility

**Base.** Binding persistido `{"Kind":"projectVariable","VariableId":"<guid>"}` (Discovery §5.1). Cinco resultados de
`InspectBinding`: `Healthy`, `RepairableMissingTarget`, `FatalIncompatibleTarget`, `FatalUnknownProperty` y
`FatalMalformedReference` (Discovery §4.1). Duplicar conserva el `VariableId` y exportar materializa (Discovery §5.8).

- **P24.1 — Modelo híbrido** (REQUISITO DEL OWNER): `Literal | ProjectVariableReference(VariableId) |
  Expression(BoundExpression)`.
- **P24.2 — La referencia directa NO se elimina.** Su forma persistida, su inspección y su comprobación de tipo no
  cambian.
- **P24.3 — `=X` canónico.** Un árbol que es solo `Reference(projectVariable X)` se persiste como la referencia
  existente (P2.8): `=X` no tiene dos representaciones. Una `expression` persistida que sea solo una referencia es
  el error semántico `NonCanonicalForm` (A-07).
- **P24.4 — Formas persistidas en el rack** (P17.3): sin entrada (literal), `projectVariable` o `expression`.
- **P24.5 — Resolución y dominio del consumidor** **[V3 · A-03 modificada]**. Referencia directa → valor de X en
  `RegistryEvaluation`; expresión → su evaluación (P14.8). Resolver único (G11). El dominio de la propiedad (hoy
  `>= 0` en las dos propiedades Selectivas, Discovery §9.5) lo declara el descriptor **como dato** y se aplica al
  **valor efectivo** de las tres fuentes, preservando el comportamiento heredado donde aplica:

  | Fuente | Al escribir (editor, reconciliador) | Al resolver lo persistido (apertura, preflight, BOM) |
  |---|---|---|
  | `Literal` | **Ruta histórica**: la ventana rechaza `< 0` al construir el diseño (`RackSelectiveWindow.xaml.cs:2481-2505`) | **Heredado**: no se re-valida |
  | `ProjectVariableReference` | La misma comprobación de la ventana, que ya se aplica al efectivo de la sesión (`TryGetEffectiveValue`, `:2492-2502`), y el reconciliador | `OutOfRange`: `RepairableSemanticFailure` de dominio |
  | `Expression` | Pipeline de la sesión (aviso), la misma comprobación de la ventana sobre el efectivo y el reconciliador (autoridad) | `OutOfRange`: `RepairableSemanticFailure` de dominio |

  - **Endurecimiento declarado**: una referencia directa persistida a una variable con valor negativo, alcanzable solo
    por edición externa porque RACKVARIABLES exige `> 0`, deja de resolver y falla cerrada.
  - **Una sola declaración del dominio**: el descriptor. La ruta histórica de la ventana conserva su comprobación. Una
    prueba de paridad fija que, para cada propiedad, la ventana y el resolver aceptan exactamente el mismo conjunto de
    efectivos. Unificar la comprobación de la ventana en el descriptor tocaría un archivo caliente de I-50, así que
    queda como seguimiento en G4.
- **P24.6 — Resultados de `InspectBinding`** **[V3 · A-01 · OPEN B]**. Es la única autoridad sobre una fuente de rack
  (nombres ilustrativos). `FatalUnevaluable` de V2 **desaparece**: un fallo semántico de una fuente es reparable.

  | Situación | Resultado | Clase | ¿Reparable en el rack? |
  |---|---|---|---|
  | Todos los ids presentes, tipo compatible, evaluación `Success` y efectivo dentro del dominio | `Healthy` | — | — |
  | Algún id referido ausente, en referencia o en expresión | `RepairableMissingTarget` | Semántica | Sí |
  | La fuente falla por sí misma: `InvalidArguments`, `DivisionByZero`, `NonFiniteResult`, `NonCanonicalForm` | `RepairableSemanticFailure(Intrinsic)` | Semántica | Sí |
  | Una variable leída falla: `Cycle`, `DependencyFailed`, `BrokenReference` o un error propio de esa variable | `RepairableSemanticFailure(Upstream, variable)` | Semántica | Sí |
  | El efectivo queda fuera del dominio de la propiedad | `RepairableSemanticFailure(Domain)` | Semántica | Sí |
  | Referencia directa con tipo incompatible | `FatalIncompatibleTarget` | — | No (inalcanzable con un solo `VariableType`) |
  | `PropertyId` desconocido | `FatalUnknownProperty` | Estructural | No |
  | Kind desconocido, id ilegible, payload mal formado, campo desconocido o dos autoridades | `FatalMalformedReference` | Estructural | No |

  - **Precedencia**: lo estructural, en el orden vigente, gana; después, id ausente; después, tipo incompatible; y
    por último, fallo semántico (propio, superior y dominio).
  - `RackRepairability` conserva `Healthy | Repairable | Blocked`: `Blocked` si alguna fuente es estructural o de tipo
    incompatible; `Repairable` si alguna es reparable y ninguna bloquea.
  - **Nunca** `Healthy` para un fallo de evaluación ni para un efectivo fuera de dominio.
- **P24.7 — BOM** **[V3 · OPEN B]**. RACKBOMTOTAL evalúa una vez por snapshot con la misma tubería. **Aborta** el total
  entero si algún rack incluido en ese BOM está afectado, como hoy con una referencia rota (ADR-0034 §10). Una
  variable en error sin consumidores cotizados no bloquea por sí sola. Una fuente no sana **nunca** produce una
  cantidad.
- **P24.8 — Duplicación y restamp.** La copia conserva `PropertyValues`, entradas `expression` incluidas, con los
  mismos `VariableId` (ADR-0034 §12; Discovery §5.8). I-51, ya integrada en `main`, cambió la entrada del restamp en
  el Plugin. V3 no toca ese camino y prueba en un archivo **propio**, contra el restamp integrado, que una entrada
  `expression` sobrevive a la duplicación (§12.2).
- **P24.9 — Exportación a biblioteca.** Materializa el efectivo evaluado exacto, quita `PropertyValues` y escribe
  literal-only en la línea `1.0`, como hoy (Discovery §5.8). Un rack afectado no tiene efectivo, así que su exportación
  falla cerrada.
- **P24.10 — Autoridad multi-vista.** La comparación estructural de árboles persistidos incluye `PropertyValues`
  (Discovery §7): dos vistas hermanas con expresiones distintas son `Divergent` y fallan cerradas, igual que hoy con
  referencias distintas.
- **P24.11 — Dibujos de I-47/I-48.** Con solo literales y referencias directas, dan los mismos efectivos, conservan
  sus formas persistidas y no se reescriben al abrir. La única excepción es el endurecimiento declarado en P24.5,
  alcanzable solo por edición externa. En una build anterior, un dibujo con expresiones falla cerrado (P18.2).
- **P24.12 — Rack afectado y rack no afectado** **[V3 · OPEN B]** (§5.2, decisiones 2 y 3):
  - **rack no afectado**: funciona con normalidad;
  - **rack afectado**: sin efectivo. RACKEDITAR no lo abre y remite a RACKVARIABLES con la causa, y su geometría
    existente no se toca;
  - un rack afectado solo entra en un plan que lo deje resuelto: su propia reparación, o la corrección de la causa
    superior que consume (R1).

### P25 — ID20 extension point

**OWNER INPUT.** I-49 **no** implementa `Rack.Frentes`, `Rack.Niveles`, `Rack.Altura`, `Project.TotalRacks` ni
`Project.TotalFrentes`; prepara `SymbolId`, scopes/namespaces, `ExpressionContext`, resolución de símbolos y los
datos que necesita el evaluador. **Validado por el Architect.**

- **P25.1 — Entregado por I-49**: `SymbolId`, `SymbolNamespace` (solo `projectVariable` activo), `SymbolScope` (solo
  `Project`), `SymbolTable`/`SymbolEntry`, `ExpressionContext`, `SymbolResolver`, evaluador sobre datos puros y el
  contexto de evaluación de una propiedad de rack (P6.7).
- **P25.2 — Reservado conceptualmente, sin implementación productiva** (REQUISITO DEL OWNER): `Rack.*` y `Project.*`,
  el ámbito `Rack` y un caso de definición para valores calculados. La reserva consta en el ADR y **ningún** camino
  productivo los crea.
- **P25.3 — Reserva en la gramática** **[V3 · OPEN A]**.
  - `Rack` y `Project` sin llaves son `ReservedName`.
  - `palabra.` es sintaxis de namespace: `=Rack.Frentes * 2` da `UnknownNamespace` en producción.
  - `{Rack}` es el nombre de una variable.

  Así ID20 podrá añadir `Rack.*` y `Project.*` sin cambiar el significado de ningún texto que hoy se pueda
  comprometer ni de nada persistido.
- **P25.4** — La regla de ámbito de P5.3 se fija con una prueba del núcleo sobre entradas **sintéticas de test**, sin
  crear ningún símbolo `Rack` productivo.
- **P25.5** — Las expresiones de propiedad son el consumidor natural de un futuro ámbito `Rack`: el contexto de P6.7
  es donde ID20 añadiría símbolos del rack. V3 no añade ninguno.
- **P25.6 — Frontera de datos para ID20.** Los valores calculados tendrán que venir de snapshots puros proyectados
  desde el barrido del Plugin, igual que `ProjectVariableScanEntry` (Discovery §16.2). El núcleo **nunca** lee
  AutoCAD, sistemas de Domain ni catálogos.
- **P25.7 — Lo que ID20 tendrá que resolver, y V3 no**: `Rack.Frentes` es un conteo **por fondo**, `Rack.Niveles`
  admite al menos dos lecturas, `Rack.Altura` requiere catálogo y redondea al pie, no existe ningún agregado
  `Project.*` y `RackCount` solo cuenta racks con BOM (Discovery §16.2). Las preguntas de frontera son del Owner y de
  ID20 (contrato §12), no de esta Proposal.

### P26 — ID23 extension point

**OWNER INPUT.** ID23 debe poder reutilizar parser, evaluador y contexto **sin depender de ProjectVariables**.
**Validado por el Architect con A-06.**

- **P26.1** **[V3 · A-06]** — El núcleo no depende de `ProjectVariables`, `Persistence`, `Systems.*`, `Bom`,
  `Catalogs`, `StructuralSections`, Domain, UI, Plugin ni AutoCAD. `SymbolEntry` ya no transporta `VariableType`
  (P5.1). Lo fija una guarda de fuente (P28.4), y la dirección de dependencia es de Project Variables hacia el
  núcleo, **nunca** al revés.
- **P26.2 — Superficie reutilizable**: parsear, enlazar con un contexto que aporta quien llama, comprobar, extraer
  dependencias, grafo, ciclos, orden, evaluar, formatear y diagnosticar.
- **P26.3** — La autoridad de unidades (P9.5) es igual de neutral y reutilizable.
- **P26.4 — Lo que añadiría ID23, y V3 no**: sus namespaces y ámbitos, un resultado entero y la semántica de redondeo
  para `Quantity` (hoy `int`, Discovery §16.3; P10.6), la persistencia de fórmulas de BOM y su propio ADR.
- **P26.5** — Sin ensamblado separado en V3 (§10, ALT-17): la guarda marca la frontera.
- **P26.6** — Ningún tipo de I-49 lleva conceptos de BOM, y no se persiste ninguna fórmula de BOM.

### P27 — ID28/29 information preservation

**OWNER INPUT**, precisado por el encargo de G2A.1. Hoy la procedencia se pierde en cuanto el resolver escribe el
número en Domain, y nada del impacto se persiste (Discovery §16.4). **Validado por el Architect.**

- **P27.1 — ID28 (Explain)**. Se conserva por snapshot y en memoria, para cada variable y cada fuente `expression`
  de rack:
  - `BoundExpression` y `SymbolId` de cada referencia;
  - diagnósticos (P15), con la cadena hasta la causa raíz;
  - traza **opt-in** (P16.3).
- **P27.2 — ID29 (Impact Preview)**. Se expone **antes** de ejecutar:
  - dependencias directas y transitivas (P11, P12.7);
  - dependientes inversos variable→variable (P12.2) y consumidores de rack, directos o por expresión (P21.2);
  - **conjunto afectado**: el cierre `I`, con el valor de cada variable antes y después, y los racks con sus
    propiedades;
  - **conjunto leído**: el `PlanReadSet` con sus valores esperados (P21.6).

  Hoy el preflight solo expone los racks (Discovery §16.4). V3 lo usa en los mensajes de RACKVARIABLES.
- **P27.3** — La cadena de Explain se deriva **sin** re-evaluar: propiedad → fuente (referencia o expresión) →
  `SymbolId` → `EvaluationResult` con traza opt-in, en el mismo snapshot.
- **P27.4** — **Nada se persiste**: ni trazas, ni grafo, ni valores, ni conjuntos.
- **P27.5** — El determinismo de P14.5 hace reproducibles las trazas.
- **P27.6** — Sin UI de ID28 ni de ID29 en I-49 (P30).

### P28 — tests

**Base.** Marco xUnit sin mocks; el Plugin solo se verifica con guardas de texto (Discovery §14.1). Inventario por
área y escenarios de ID22B que faltan (Discovery §14.2). Veinte guardas y pines que un motor tocaría (Discovery
§14.3). Lección de I-48: un criterio sobre tokens puede certificar una implementación corrupta
([Proposal V2 de I-48](I-48-proposal-v2.md), R-01).

- **P28.1** — El **criterio de aceptación es de comportamiento**. Las guardas de fuente son defensa secundaria y
  nunca el criterio.
- **P28.2 — Evidencia por gate**: RED demostrado en local antes de GREEN para todo comportamiento nuevo; suites focal
  y de impacto; las **dos** suites en local sobre el Candidato; CI verde sobre el SHA exacto (AGENTS.md; WORKFLOW §4
  y §5).
- **P28.3 — Familias de pruebas.**

  | Gate | Qué se prueba |
  |---|---|
  | G5 | Lexer y parser por tabla (válidos, inválidos, posiciones); unidades (`[mm]`, `[in]`, `[ft]`, `UnknownUnit`, `UnitNotAllowedHere`, `UnitSyntaxNotSupported`); `AmbiguousDecimalComma`; gramática de referencias de §4; límites normativos; orden de diagnósticos; formatter canónico y *round-trip* P2.5 sobre un corpus con homónimos y nombres con llaves |
  | G6 | Resolver y binder con todos los resultados de P7.1; **motor adimensional**: `6 + 2`, `A + 2`, `A * B`, `A / B`, `100[mm] + 2`, `MAX(A, 4[in])` y un intermedio negativo, con valores exactos; conversiones bit a bit (`4[in]` = 4, `20[ft]` = 240, `100[mm]` = 100 ÷ 25.4); **autoridad única**: los alias de P9.6 valen lo mismo que la autoridad y los dorados geométricos no cambian; `MIN`, `MAX` y `ABS` (aridad y valores); semántica numérica; regla de ámbito sintética; `Rack.*` y `Project.*` no resuelven; tabla de canonicalización P2.8; tablas de tokens |
  | G7 | Extracción sin repetición y ordenada; grafo con diamante, autorreferencia, ciclo de 2, componente mayor y aristas rotas; orden independiente de permutaciones; cortocircuito `DependencyFailed`; conjuntos de P12.7 |
  | G8 | Paso 1, solo pruebas: caracterización de V-0 sobre la producción intacta. Paso 2: round-trip del store real de definiciones y fuentes `expression`; cada regla estructural de P17.6; `NonCanonicalForm` semántico; **fixture dorado**: el JSON literal-only que escribe la build nueva es idéntico al de `a4d88f1`; resultados de `InspectBinding` para los dos kinds; resolver con expresiones y dominio; `RegistryEvaluation` único; RACKBOMTOTAL con un solo snapshot; restamp que conserva una entrada `expression`; exportación que la materializa; presupuesto de profundidad JSON (P1.9) |
  | G9 | Las cuatro transiciones; cierre impactado; consumidores por expresión; una `RegistryMutation`; racks deduplicados y resueltos una vez; R1–R3; contrato raíz; `PlanReadSet` y aborto en commit antes de `TryWrite`; `Delete` y `UnlinkAllAndDelete` con consumidores por expresión y con dependientes; ciclo rechazado; `Create` y `Rename` con expresiones; reparación generalizada; oráculo: descubrimiento por conjunto = unión por variable |
  | G10 | Sesión del editor: todos los gestos de P23.5 con éxito y con fallo del pipeline; las garantías de P23.2 con borradores `=`; `=X` persiste lo mismo que seleccionar X; `=6` lo mismo que `6`; inserción de candidatos; sin auto-selección; reconciliador (P23.15–P23.16); workspace y ventana de RACKVARIABLES con intents enlazados; panel de diagnóstico; pruebas del control sin interfaz |
  | G11 | Pruebas reales de cadena X → Y → expresión de propiedad → geometría y firma de BOM sobre catálogo real; un rack con una propiedad por referencia y otra por expresión; suites de I-47/I-48 verdes, sin más cambios de aserción que los de P28.4 |

- **P28.4 — Evolución explícita de guardas y pines** (Discovery §14.3). Se hace en el **mismo** gate que el
  comportamiento, con RED→GREEN, y **nunca** renombrando tipos para esquivarlas:

  | Guarda o pin | Evolución propuesta | Gate |
  |---|---|---|
  | G1 `NO_HAY_FORMULAS_NI_REFERENCIAS_A_PROPIEDADES_DE_OTROS_RACKS` | `RackPropertyReference`, `rackProperty` y `FormulaParser` siguen prohibidos en Application, Plugin y UI; `ExpressionParser` y `DependencyGraph` pasan a permitirse **solo** en Application (núcleo y adaptadores) y siguen prohibidos en Plugin y UI | G5, G7 |
  | G2 `LA_DEFINICION_DE_UNA_VARIABLE_SIGUE_TENIENDO_UN_SOLO_CASO` | Se sustituye: exactamente `Literal = 1` y `Expression = 2`; `Formula = ` sigue prohibido | G8 |
  | G3 `"expression"` = error duro | Se sustituye **después** del paso 1 de G8: `"expression"` válido solo con payload estructuralmente válido; kinds desconocidos (`"formula"`, `"futureKind"`) siguen siendo error duro | G8 |
  | **Nueva** — kinds de binding | Exactamente `projectVariable` y `expression`; los fixtures `rackProperty` y `futureKind` siguen fallando cerrados (Discovery §15, búsqueda 10) | G8 |
  | G4 Domain sin variables | Sin cambio; se añade: Domain sin tokens del núcleo | G5 |
  | G5 capa pura sin AutoCAD | Se extiende al núcleo y a la autoridad de unidades | G5 |
  | G6–G10 gramática de `VariableType` | Sin cambio: la tabla explícita de P17.10 conserva el lenguaje aceptado | — |
  | G11 y G12 resolver único y BOM | Sin cambio; se añade: RACKBOMTOTAL no parsea ni evalúa | G8 |
  | G13 ejecutor | Se añaden `ExpressionParser`, `ExpressionEvaluator` y `DependencyGraph` como ausentes | G9 |
  | G14 comando RACKVARIABLES | Sin cambio; se añade: el comando no evalúa ni enlaza | G9 |
  | G15 y G16 censos | Sin cambio: 33 comandos y 29 ventanas | — |
  | G17 catálogo cerrado | Sin cambio: dos propiedades | — |
  | G18 Plugin y UI no interpretan vínculos | Se extiende: sin tipos del núcleo en Plugin ni UI; `RackSelectiveWindow.xaml.cs` sigue sin `VariableId`, resolver ni parser | G10 |
  | G19 el control no compara nombres | Sin cambio | — |
  | G20 pines de `=` | Evolucionan **exactamente** según P23.12: `CASO_6` cambia de clase, `CASO_8` se reescribe y `CASO_7` queda intacto | G10 |
  | **Nueva** — independencia del núcleo | El núcleo y la autoridad de unidades no nombran `ProjectVariables`, `Persistence`, `Systems`, `Bom`, `Catalogs`, `StructuralSections`, `RackCad.Domain`, `RackCad.UI`, `RackCad.Plugin` ni `Autodesk` | G5 |
  | **Nueva** — sin comprobador dimensional | El criterio son las pruebas de P5.6; defensa secundaria: el núcleo no declara tipos ni códigos dimensionales | G6 |
  | **Nueva** — una autoridad de unidades | Tokens y factores de unidad solo en la autoridad neutral; ningún parser de unidades en UI, Domain ni Plugin; ningún `25.4` ni factor de pulgadas por pie fuera de la autoridad, salvo los alias de P9.6 | G6 |
  | **Nueva** — tokens explícitos | Ningún token persistido de I-49 se produce con `ToString()`, `nameof` ni el conversor de enums | G8 |

- **P28.5 — Escenarios obligatorios añadidos en V3** **[V3]**. Cada uno es una prueba de comportamiento con su gate:

  | # | Escenario | Qué fija | Gate |
  |---|---|---|---|
  | T-V3-01 | Gramática de OPEN A por tabla | `Nombre`, `Nombre Compuesto`, `{Nombre complejo}`, `{a}}b}`, `Nombre#<GUID>`, `{Nombre}#<GUID>`, `UnterminatedName`, `OperatorInName`, `UnitNotAllowedHere` tras referencia | G5, G6 |
  | T-V3-02 | Cualificación con GUID completo | Solo la forma D de 36 caracteres; `#3f2b1c9e` es `InvalidQualifier`; hexadecimales sin distinguir mayúsculas; `QualifiedNameMismatch` | G5, G6 |
  | T-V3-03 | `#GUID` de referencia rota | El formatter emite `#<id>` para un id ausente; ese texto da `BrokenReference` y no se compromete; `#<id>` sin nombre con id presente da `NameRequired` | G6, G10 |
  | T-V3-04 | Escape de nombres de función y reservados | `MIN`, `max`, `Rack` y `Project` sin llaves dan `ReservedName`; `{MIN}` y `{Rack}` enlazan a variables; `Rack.Frentes` da `UnknownNamespace` | G5, G6 |
  | T-V3-05 | Clasificación estructural frente a semántica | Cada fila de §5.1, en definición y en fuente | G8 |
  | T-V3-06 | Un registro con errores semánticos sigue inspeccionable | `Usable`; RACKVARIABLES abre y diagnostica; los racks no afectados abren y resuelven; las opciones no ofrecen variables en error | G8, G10 |
  | T-V3-07 | Reparación de una propiedad con fallo semántico | Causa propia, superior y de dominio: la reparación quita la fuente, gobierna el literal congelado y el rack resuelve | G9 |
  | T-V3-08 | Propiedad estructural no reparable de forma destructiva | `FatalMalformedReference`: rack `Blocked`, la reparación falla y no se escribe nada, aunque el rack tenga además fuentes reparables | G9 |
  | T-V3-09 | Reglas R1, R2 y R3 | Rack que no resuelve después → plan vacío; símbolo sano que pasaría a fallar → rechazo; error previo con definición intacta → no bloquea; ningún intent escribe un error nuevo; salida de un ciclo con dos errores | G9 |
  | T-V3-10 | Carrera del `PlanReadSet` (A-05) | `verticalClearance = X`, `palletTolerance = Y + 1`; Y cambia entre preflight y commit → aborto antes de escribir; un cambio fuera del `PlanReadSet` no aborta | G9 |
  | T-V3-11 | `UnlinkAllAndDelete` bloqueado por una expresión de propiedad | `palletTolerance = A + B`, `UnlinkAllAndDelete(A)` → bloqueado con rack, propiedad y fórmula; nada materializado | G9 |
  | T-V3-12 | `UnlinkAllAndDelete` bloqueado por un dependiente | `C = A + 1` → bloqueado con la lista de dependientes; con solo referencias directas materializa como hoy | G9 |
  | T-V3-13 | Confirmación de reparación consciente de la fuente | El texto incluye rack, `PropertyId`, tipo de fuente, expresión canónica, causa, literal congelado, causa superior y el aviso de recuperación | G9, G10 |
  | T-V3-14 | Diagnóstico semántico de forma no canónica | `{"Kind":"expression","Expression":{"Node":"number","Value":6}}` → `NonCanonicalForm` por símbolo o fuente, registro `Usable`; ningún escritor produce una forma no canónica | G8 |
  | T-V3-15 | Campo desconocido en un payload de Expression | Campo desconocido o repetido en la definición `expression`, en la entrada `expression` o en un nodo profundo → estructural; el mismo campo en raíz, entrada, `literal` o `projectVariable` se conserva como hoy | G8 |
  | T-V3-16 | Mismo contrato raíz para literal y expresión | `-2` y `=0 - 2` rechazados igual; `=Base + (A - B)` con intermedio negativo aceptado; un cambio de A que dejaría una dependiente sana en `≤ 0` se rechaza; al leer, una raíz `≤ 0` persistida se trata igual en las dos sintaxis | G9, G10 |
  | T-V3-17 | Mismo dominio de propiedad para cualquier fuente | Paridad: literal (ruta de la ventana), referencia y expresión aceptan el mismo conjunto de efectivos; endurecimiento declarado de una referencia negativa persistida | G8, G10 |
  | T-V3-18 | `RegistryEvaluation` como única autoridad de evaluación | Cada símbolo se evalúa una vez por snapshot; opciones, workspace, targets, preflight y BOM exponen exactamente sus valores; la sesión no persiste un valor propio | G8, G10 |
  | T-V3-19 | Las transiciones nuevas no comparten `PropertyValues` | Tras un reconcile fallido y tras uno correcto, el authored recibido es idéntico en forma serializada; mutar la salida no altera la entrada; `no persistence happened` y `no in-memory mutation happened` se prueban por separado | G10 |
  | T-V3-20 | Tablas explícitas de tokens persistidos | Cada token de P17.10 en los dos sentidos; `Type` escrito como `"Length"` byte a byte; un token con otra grafía es estructural | G8 |
  | T-V3-21 | Caracterización de la build anterior bajo V-0 | Commit solo de pruebas, con CI, sobre la producción intacta: registro con `expression` → `PresentButUnreadable`; diseño con fuente `expression` → `FatalMalformedReference(UnknownKind)`, `UnknownReferenceKind`, `Indeterminate` y descubrimiento abortado | G8, paso 1 |

  Tras el paso 1 de G8, las aserciones de T-V3-21 sobre `expression` evolucionan con los stores. El rechazo de la
  build anterior queda probado por la CI de ese commit y, en G12, con el DLL de `a4d88f1` (P29.5). Los kinds
  desconocidos conservan sus pruebas de rechazo.
- **P28.6** — V3 no exige refactorizar los fixtures duplicados que el Discovery contó (≈25 clases, Discovery §14.1).
  Las pruebas nuevas pueden introducir un único constructor de registros y fuentes con expresiones.
- **P28.7** — Checklist de validación del Owner: §8.3.

### P29 — migration

**Base.** Leer nunca escribe y re-escribir no conserva bytes, pero sí versión, `ExtensionData` y contenido
(Discovery §5.7).

- **P29.1** — **Sin migración de datos**: ni registros, ni bindings de rack, ni bibliotecas se reescriben al abrir.
- **P29.2** — Sin conversión automática de literales o referencias en expresiones, ni de expresiones en literales, y
  sin detección de «valores iguales».
- **P29.3 — Versiones (V-0).** La primera expresión **no** cambia ninguna versión, y quitar todas las expresiones
  devuelve el dibujo a lo que una build anterior puede usar. Los escritores de I-49 nunca degradan una versión
  (P23.15).
- **P29.4** — En una build anterior, un dibujo con expresiones falla cerrado (P18.2). Se documenta en el ADR y en la
  comunicación al usuario; sin herramienta de degradación (P30).
- **P29.5** — La compatibilidad se verifica en G12 con el DLL de `a4d88f1`, sobre tres dibujos: sin expresiones, con
  expresiones en el registro y con expresiones de propiedad.
- **P29.6** — Los caminos heredados `SelectiveBindingOptions.ForLength` y `BindingIntent`, sin disparador en producción
  (Discovery §8.1), **no** se extienden (riesgo R15).

### P30 — non-goals

- **P30.1** — ID21: referencias rack→rack. `RackPropertyReference` y `rackProperty` siguen prohibidos.
- **P30.2** — ID20 productivo: símbolos `Rack.*` y `Project.*`, parámetros calculados y ámbito `Rack`; solo quedan
  reservados (P25.2–P25.3).
- **P30.3** — ID23: Custom / Calculated BOM, cantidades enteras y fórmulas de BOM.
- **P30.4** — UI de ID28 Explain y de ID29 Impact Preview.
- **P30.5** — Sistema de dimensiones físicas, comprobador dimensional, tipos de área, volumen, masa o ángulo, nuevos
  `VariableType` y **variables de usuario escalares o con signo**: exigen otro `VariableType` u otra iniciativa
  (§0.2).
- **P30.6** — Unidades distintas de `[mm]`, `[in]` y `[ft]`; unidades sobre referencias, llamadas o paréntesis;
  sintaxis de pies-pulgadas o de fracciones; unidades en literales sin `=`; conversión del DWG.
- **P30.7** — Funciones distintas de `MIN`, `MAX` y `ABS`: `ROUND`, `CEILING` y `FLOOR` quedan diferidas (P10.6), e
  `IF`, `AND`, `OR`, comparadores, lookup, arrays, strings, trigonometría y macros, excluidos (P10.7).
- **P30.8** — Un `FormulaTextBox` o cualquier control de entrada paralelo (P23.1).
- **P30.9** — Parsers o evaluadores externos (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`, Roslyn) y
  cualquier dependencia NuGet (ADR-0012).
- **P30.10** — Grafo, valores evaluados, trazas, conjuntos o texto tecleado persistidos.
- **P30.11** — Sintaxis localizada (`;` como separador y `,` decimal) y completar texto sin selección explícita o
  nombre exacto.
- **P30.12** — Control de concurrencia más allá del `PlanReadSet` (P21.6).
- **P30.13** — Transferencia o fusión de variables entre dibujos (decisiones de I-47, C2-8).
- **P30.14** — Política de unicidad o de mayúsculas de nombres, y renombrado de variables existentes.
- **P30.15** — Comandos o ventanas nuevos, otros sistemas de rack (Dinámico, Push Back, Cama, Cantilever) y nuevas
  propiedades vinculables.
- **P30.16** — Herramientas de degradación o de exportación a versiones anteriores.
- **P30.17** — Corregir los hallazgos laterales L1–L5 (Discovery §7, §5.8, §8.1, §11.5). Se registran en
  `docs/ideas-futuras.md` (contrato §4) en G4. **Excepciones acotadas**, que no son arreglos laterales sino
  preservación de invariantes en código que I-49 reescribe:
  - L2 en la rama del reconciliador que I-49 extiende (P23.15, A-09);
  - la tabla explícita del token `Type` en la rama `Add` (P17.10, A-17).

  L3 no se corrige en el portador: I-49 aísla su propio estado (P23.16).
- **P30.18** — Hacer converger los parsers de literales del editor y de RACKVARIABLES (Discovery §11.2).
- **P30.19** — Cambiar invariantes de I-47/I-48 más allá de lo que enumera §2.3: es condición de parada, no alcance.
- **P30.20** — Una operación o un comando de reparación nuevos: `RepairBrokenRack` sigue siendo la única (§7.2).
- **P30.21** — Convertir de forma implícita una fórmula en literal, en `UnlinkAllAndDelete` o en cualquier otra
  operación (§7.1).
- **P30.22** — Relajar la validez de una variable definida por expresión respecto de la misma variable definida por
  literal (§0.2).

---

## 4. OPEN A — CLOSED: sintaxis de homónimos

```text
Estado V2  = OPEN FOR ARCHITECT
Veredicto  = Architect Review V2, §3: nombres sin delimitar cuando es seguro, llaves para el resto, #GUID completo
Estado V3  = CLOSED — decisión de V3 con la sintaxis del Architect
Precisión  = #<GUID> sin nombre: solo lo emite el formatter para referencias rotas; nunca enlaza
```

### 4.1 Formas

```text
Nombre                              nombre único y seguro sin llaves
{Nombre complejo}                   nombre con cualquier otro carácter, o reservado
Nombre#<GUID completo>              homónimo, o identidad pegada o tecleada
{Nombre complejo}#<GUID completo>   homónimo con llaves
#<GUID completo>                    SOLO formatter de referencia rota
```

Gramática: producción `reference` de P1.2, con las reglas léxicas de P1.7.

### 4.2 Reglas

1. **GUID completo, nunca un fragmento.** Forma D de 36 caracteres (P3.9). `#3f2b1c9e` es `InvalidQualifier`.
2. **`Name` no es identidad.** Con cualificador, el id es la autoridad y el nombre solo se valida
   (`QualifiedNameMismatch`). Lo persistido son ids (P17).
3. **Las llaves delimitan nombres complejos**, y `}}` escapa `}`. Todo nombre que no sea `word {" " word}` va entre
   llaves: operadores, paréntesis, comas, corchetes, punto, dígito inicial, espacios dobles, iniciales o finales.
4. **Reservados sin llaves**: los nombres de función del registro (P10.9), `Rack` y `Project`, sin distinguir
   mayúsculas (`ReservedName`). `palabra.` es sintaxis de namespace para ID20 (`UnknownNamespace`).
5. **Un fragmento parcial nunca enlaza** (`UnknownSymbol`). La lista filtra e inserta; no resuelve.
6. **Un nombre exacto y único sí enlaza al comprometer**, con el comparador de P7.2.
7. **Un nombre ambiguo da diagnóstico** `AmbiguousName`, con cada candidato en su forma cualificada.
8. **La lista y el selector insertan la forma inequívoca** del formatter (P22.5, P23.7).
9. **La forma cualificada se puede pegar o teclear.**
10. **`[mm]`, `[in]` y `[ft]` solo después de números.** Tras una referencia dan `UnitNotAllowedHere`.
11. **Un `#GUID` roto se puede MOSTRAR, pero no comprometer** como referencia nueva a un id inexistente
    (`BrokenReference`). `#<GUID>` sin nombre con id presente da `NameRequired`.
12. **Seguridad con operadores.** Un tramo contiguo, sin llaves ni espacios, que coincide con el nombre de una
    variable (`Holgura-Base`) da `OperatorInName`; nunca se reinterpreta como resta.

### 4.3 Formatter

Forma mínima inequívoca dentro del snapshot (P4.2, P4.7):

| Situación | Texto |
|---|---|
| Id ausente | `#<id>` |
| Nombre único y seguro sin llaves | `Nombre` |
| Nombre único con cualquier otro carácter, o reservado | `{Nombre}`, con `}` escapado como `}}` |
| Homónimo | La forma del nombre + `#<id>` |

Si un nombre único pasa a ser ambiguo, la siguiente presentación lo cualifica; lo persistido no cambia.

### 4.4 Ejemplos

| Caso | Texto |
|---|---|
| Nombre único | `=Holgura + 2` |
| Espacios simples | `=Holgura General + 2` |
| Operadores, paréntesis, punto, dígito inicial o espacios dobles | `={Holgura-Base} * 2` · `={Alto (m)}` · `={Nivel 2}` · `={v1.2}` |
| Llave en el nombre | `={a}}b}` (nombre `a}b`) |
| Igual a una función o reservado | `={MIN} + 1` · `={Rack}` |
| Homónimo | `=Holgura#3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44 + 2` |
| Homónimo con llaves | `={Holgura General}#8c1d7e20-4b5a-4c6d-9e7f-102132435465` |
| Referencia rota (solo presentación) | `=#3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44 + 2`, no comprometible |
| Unidad | `=100[mm] + Holgura` |
| ID20 futuro | `=Rack.Frentes * 2` → `UnknownNamespace` |

### 4.5 `CASO_8` actualizado

`CASO_8_ENTER_SIN_CANDIDATO_SELECCIONADO_NO_RESUELVE_POR_TEXTO` se reescribe según este contrato: **«Enter
resuelve un nombre exacto y único; nunca uno parcial ni ambiguo»** (P23.12). `CASO_6` cambia de clase y `CASO_7`
queda intacto.

### 4.6 Los cuatro requisitos congelados

| Requisito (encargo de G2A.1) | Cómo se cumple |
|---|---|
| homónimo termina en `VariableId` inequívoco | El GUID completo no colisiona, y un homónimo sin cualificar no enlaza |
| `Name` no es autoridad | El nombre valida, nunca resuelve con cualificador; sin cualificador, solo resuelve si es exacto y único **en el snapshot mostrado** (P7.7) |
| formatter puede representar fórmula editable | P2.5 se prueba como propiedad, con homónimos y llaves |
| rename no rompe | Lo persistido son ids (P19.5) |

---

## 5. OPEN B — CLOSED: legibilidad estructural frente a evaluabilidad semántica

```text
Estado V2  = OPEN FOR ARCHITECT
Veredicto  = Architect Review V2, §4 (clasificación y diez decisiones)
Estado V3  = CLOSED — decisión de V3
Sustituye  = FatalUnevaluable de V2 (P24.6), retirado como callejón sin salida (A-01)
```

**Principio.** Un fallo **estructural** es algo cuyo significado la build no entiende: no se lee y no se repara de
forma destructiva. Un fallo **semántico** es una fuente que la build entiende pero de la que no obtiene valor: se
diagnostica y se corrige, sin fallback.

### 5.1 Clasificación

| Caso | Definición en el registro | Fuente de rack | Clase |
|---|---|---|---|
| `Kind`, `Node` o `Namespace` desconocido persistido | Registro **entero** `PresentButUnreadable`, como hoy con un kind desconocido | `FatalMalformedReference`: rack `Blocked`, sin reparación destructiva | **Estructural** |
| Token de función desconocido persistido | Igual | Igual | **Estructural** |
| Token de unidad desconocido persistido | Igual | Igual | **Estructural** |
| Id o AST mal formado: id ilegible, campo obligatorio ausente, número no finito, límites superados | Igual | Igual | **Estructural** |
| Campo desconocido o repetido dentro del payload de Expression (A-08) | Igual | Igual | **Estructural** |
| Dos autoridades en la misma entrada: estado imposible | Igual | Igual | **Estructural** |
| `BrokenReference` | Símbolo `Failed` | `RepairableMissingTarget` | Semántico |
| `Cycle` | Miembros `Failed(Cycle)`, con la ruta | Fuentes que los leen: `RepairableSemanticFailure(Upstream)` | Semántico |
| `InvalidArguments` | `Failed` | `RepairableSemanticFailure(Intrinsic)` | Semántico |
| `DivisionByZero`, `NonFiniteResult` | `Failed` | `RepairableSemanticFailure(Intrinsic)` | Semántico |
| `DependencyFailed` | `Failed` | `RepairableSemanticFailure(Upstream)` | Semántico |
| `NonCanonicalForm` (A-07) | `Failed` | `RepairableSemanticFailure(Intrinsic)` | Semántico |
| `OutOfRange` del consumidor | No aplica al leer: rige el contrato histórico de la variable (P8.10) | `RepairableSemanticFailure(Domain)` | Semántico |

Un fallo semántico da un resultado **por símbolo o por fuente**, y el registro sigue estructuralmente `Usable`.

Una fuente de rack estructuralmente mal formada se comporta como hoy un kind desconocido: la sonda da
`Indeterminate` y las mutaciones que necesitan descubrimiento abortan (P21.4). Un dato que la build no entiende nunca
se interpreta como «no consume». RACKVARIABLES muestra ese rack como bloqueado, sin reparación.

### 5.2 Decisiones

1. **Alcance del bloqueo: solo el cierre afectado.** Queda afectado lo que falla: los símbolos en error, sus
   dependientes transitivos y todo rack con al menos una fuente no sana. Con errores solo semánticos, el registro
   sigue `Usable`.
2. **Rack no afectado**: funciona con normalidad. Sus opciones no ofrecen variables en error (P23.11).
3. **Rack afectado** (P24.12):
   - sin efectivo;
   - RACKEDITAR bloqueado: no lo abre y remite a RACKVARIABLES con la causa;
   - geometría existente intacta;
   - RACKVARIABLES ofrece diagnóstico y reparación;
   - solo entra en un plan que lo deje resuelto: su reparación o la corrección de la causa superior (R1).
4. **RACKBOMTOTAL**: aborta si un rack incluido en ese BOM está afectado, como hoy con una referencia rota (ADR-0034
   §10). Una variable fallida sin consumidor cotizado no bloquea por sí sola (P24.7).
5. **RACKVARIABLES**: abre siempre que el registro sea estructuralmente legible y esté acreditado. Su panel de
   diagnóstico, fuera del panel editable, muestra la causa raíz, los miembros de cada ciclo, los dependientes y las
   fuentes de rack afectadas (P22.9). Con un error estructural sigue bloqueado como hoy, sin escribir.
6. **Intents correctivos**: se admiten todos los normales, más la reparación, gobernados por R1, R2 y R3 (P21.3).
   `Rename` siempre está permitido (P19.7).
7. **Ciclos persistidos**: todos los miembros dan `Cycle` con la ruta; se rompen con `ChangeDefinition` de un
   miembro; borrar un miembro sigue bloqueado (P13.5).
8. **Fórmulas de propiedad rotas**: dos vías. Corregir la variable superior, que las recupera sin quitarlas, o
   reparar el rack y volver a escribir la fórmula en RACKEDITAR. Las estructurales no se reparan, para no destruir
   datos que esta build no entiende.
9. **Re-lectura de commit** (P21.6):
   - re-lectura estructuralmente ilegible → aborto;
   - re-acreditar y validar ids;
   - re-evaluar el `PlanReadSet` y abortar antes de `TryWrite` si algo falla o difiere;
   - los errores fuera del `PlanReadSet` no bloquean ese commit.
10. **`RepairBrokenRack`**: una sola operación con el conjunto reparable ampliado y confirmación consciente de la
    fuente (§7.2).

### 5.3 Los dos requisitos congelados

| Requisito (encargo de G2A.1) | Cómo se cumple |
|---|---|
| geometry/BOM jamás fallback | Un rack afectado no tiene efectivo, su geometría existente no se toca, RACKBOMTOTAL aborta si lo incluye y ninguna fuente no sana produce un valor (P24.7, P24.12) |
| RACKVARIABLES debe poder diagnosticar/corregir estados semánticamente rotos que esta build puede leer estructuralmente | El registro sigue `Usable`; RACKVARIABLES abre y diagnostica; R1–R3 y la reparación generalizada garantizan una salida para cada estado semántico (§5.2, decisiones 5, 6, 8 y 10) |

---

## 6. Schema — CLOSED: V-0

```text
Estado V2  = ARCHITECT REVIEW REQUIRED (V-0 propuesta)
Veredicto  = Architect Review V2, §5: V-0 con cuatro condiciones
Estado V3  = CLOSED — V-0
```

### 6.1 Decisión

**No cambia** la versión de `ProjectVariablesDocument` (sigue `1.0`, major soportado 1) ni la del diseño Selectivo
(`1.0` / `2.0` *sticky*, major de lectura 2).

**Justificación principal**: las builds I-47/I-48 ya fallan cerradas ante un discriminador desconocido y no leen mal
la semántica nueva (P18.2). Un major no protegería nada. V-1 no cambia el comportamiento de ningún lector y V-2
excluye a las builds anteriores para siempre (P18.4–P18.5). V-0 **no** se justifica por reducir archivos tocados
(P18.7).

### 6.2 Condiciones obligatorias

| # | Condición | Dónde se cumple | Cómo se verifica |
|---|---|---|---|
| C1 | Payload de Expression con mundo cerrado | P17.6.7 | T-V3-15 |
| C2 | Semántica nueva solo por discriminadores o tokens cerrados, o por un major | §6.3, en el ADR | Revisión de cada adición (P18.8) |
| C3 | Los escritores de I-49 nunca degradan ni fijan una versión | P23.15 | Prueba: un diseño `2.3` editado sigue en `2.3` |
| C4 | Caracterización del comportamiento heredado antes de modificar los stores | G8, paso 1: commit solo de pruebas con su CI | T-V3-21 |

### 6.3 Regla de evolución, para el ADR

La semántica futura de Expression entra **solo** por tokens o discriminadores explícitos que la build lectora
conozca, o por un cambio de major que obligue a las builds anteriores a rechazar el documento. **Nunca** por campos
que una build anterior pudiera ignorar. La regla vincula a las builds futuras en todo el documento, no solo dentro
del payload, donde además la impone el mundo cerrado (P17.6.7).

---

## 7. Veredictos de operación — CLOSED

### 7.1 `UnlinkAllAndDelete(X)`

```text
Direct reference to X                       → materializable como hoy
Property expression depending on X          → BLOCKED
ProjectVariable definition depending on X   → BLOCKED
```

- **Referencias directas**: se materializan como hoy. El efectivo, es decir el valor evaluado exacto de X, pasa a
  literal y se quita el binding (`ProjectVariableMutationPreflight.cs:263,272,283-287`).
- **Expresiones de propiedad** que dependen de X: la operación entera queda **bloqueada**.
- **Definiciones de variable** que dependen de X: la operación entera queda **bloqueada**.
- **Sin conversión implícita** de fórmulas en literales y sin sustituir el valor de X dentro de ninguna fórmula.
- **El bloqueo muestra** los dependientes (variable y definición formateada) y los consumidores que bloquean (rack,
  propiedad y fórmula canónica).
- **R1**: todo rack materializado tiene que resolver después; si no, bloqueado con «repara ese rack primero».
- **Salida para el usuario**: editar cada fórmula en RACKEDITAR y cada definición dependiente en RACKVARIABLES, y
  ejecutar después la operación.

**Por qué.** «Desvincular» materializa el efectivo de una propiedad que **sigue** a X, y una fórmula no sigue a X:
sigue a su fórmula. `palletTolerance = A + B` también depende de B, y materializarla cortaría una dependencia que
nadie pidió tocar. La operación vigente solo recorre bindings directos (`:263`). Es la misma regla que ya se aplica a
los dependientes, y ADR-0034 §11 rechaza reescribir un diseño authored como efecto colateral.

### 7.2 `RepairBrokenRack(rack)`

- **Una sola operación**: mismo nombre, mismo intent `RepairBroken(rackId, confirmed)`, rack-scoped, atómica,
  explícita y confirmada (ADR-0034 §11; `ProjectVariableMutationPreflight.cs:458-530`). **Ningún comando nuevo.**
- **Conjunto reparable ampliado** [V3 · A-01]: toda fuente `RepairableMissingTarget` o `RepairableSemanticFailure`,
  por causa propia, superior o de dominio (P24.6).
- **Efecto**: quita esas fuentes de `PropertyValues` y deja gobernar el literal congelado, sin tocarlo, sobre un clon
  (`:514-520`). El rack resuelve por construcción y produce **una** `RackMutation` (`MutationPlan.cs:150-168`); si
  aun así no resolviera, el plan queda vacío (`:522-529`).
- **No reparable**: lo estructural (`FatalMalformedReference`, `FatalUnknownProperty`) y `FatalIncompatibleTarget`.
  El rack queda `Blocked` entero aunque tenga además fuentes reparables (`:477-482`): no se destruyen datos que esta
  build no entiende.
- **Confirmación consciente de la fuente** [V3 · A-10]. Hoy solo lista `'propiedad' -> 'id' (literal almacenado X)`
  (`:491-511`), y al reparar una expresión el usuario perdería una fórmula que puede contener referencias válidas.
  La confirmación de V3 incluye, **por fuente**:

  | Campo | Contenido |
  |---|---|
  | `Rack` | Nombre e id del rack |
  | `PropertyId` | Propiedad afectada |
  | `SourceKind` | `projectVariable` o `expression` |
  | `CanonicalExpression` | Texto del formatter en el snapshot, con `#<id>` para referencias rotas |
  | `FailureCause` | Código y dueño: id ausente, causa propia, causa superior o dominio |
  | `FrozenLiteral` | Literal congelado que pasará a gobernar |
  | `UpstreamCause` | Si aplica: la variable superior y su código |
  | Aviso de recuperación | Si la causa es superior: «Corregir <variable> recuperaría esta fórmula sin eliminarla; reparar la elimina» |

  Aparece en el panel de RACKVARIABLES antes de confirmar y en el mensaje de confirmación del preflight. La
  confirmación sigue siendo del **conjunto completo** del rack, como hoy.

---

## 8. Gates G0–G12

> **Propuestos, NO autorizados.** Solo se abre el gate siguiente con evidencia revisable del anterior (contrato
> §8). La división de G5–G10 puede ajustarse en la re-revisión.

### 8.1 Secuencia

| Gate | Nombre | Entregable | Evidencia exigida | Estado |
|---|---|---|---|---|
| **G0** | Reclamo + bootstrap | Reclamo atómico `77262fe`; contrato, decisión del Owner y fila de ROADMAP `f2d28a2` | Push aceptado sin force | **HECHA** |
| **G1** | Discovery read-only | [`I-49-discovery.md`](I-49-discovery.md) @ `4cf02b1` | CI 4/4 en ese SHA | **HECHA** |
| **G2** | Proposal | V1 `b15e40a` (historial) → V2 `9aef7d0` (`Architect = CHANGES REQUIRED`) → **V3** (G2C) → re-revisión del Architect → las versiones que pida, hasta `Architect = AGREED` | Architect Reviews | **EN CURSO** (G2C) |
| **G3** | Consenso + freeze | `Coordinator = AGREED` y `Architect = AGREED` sobre la **misma** Proposal, más la decisión del Owner | Registro en `decisions/I-49.md` | pendiente — **compuerta** |
| **G4** | ADR + alineación documental | ADR aceptado por el Owner, con número asignado **en ese momento**; contrato (§1, §3.2, §11.2) y ROADMAP alineados con la Proposal congelada; en `ideas-futuras.md`: L1–L5, la unificación del dominio de la ventana en el descriptor (P24.5) y el factor lb/ft→kg/m de `tools/` (P9.6) | ADR en estado `aceptado` | NO AUTORIZADA |
| **G5** | Núcleo sintáctico | Lexer con unidades, nombres y cualificadores; parser con la gramática de §4; sintaxis; formatter; límites normativos; diagnósticos de sintaxis; guardas G1 (parcial), G4, G5 e independencia del núcleo | RED→GREEN; focal + impacto | NO AUTORIZADA |
| **G6** | Núcleo semántico | `SymbolId`, namespaces, ámbitos, `ExpressionContext`, tabla de símbolos sin `VariableType`, `SymbolResolver` y binder de §4, autoridad de unidades y alias de P9.6 (con procedimiento de colisión para `SelectiveGeometryResolver.cs`), `FunctionRegistry` (`MIN`, `MAX`, `ABS`), tablas de tokens, evaluador adimensional y `EvaluationResult`; guardas de unidades y sin comprobador dimensional | RED→GREEN; dorados geométricos intactos | NO AUTORIZADA |
| **G7** | Núcleo de grafo | Extracción de dependencias, grafo, ciclos, orden, conjuntos derivados (P12.7) y `RegistryEvaluation`; guarda G1 completa | RED→GREEN | NO AUTORIZADA |
| **G8** | Persistencia + integración | **Paso 1**: commit solo de pruebas, con su CI, que caracteriza sobre la producción intacta el rechazo de un registro y de un diseño con `expression` (C4, T-V3-21). **Paso 2**: `VariableDefinition.Expression`, fuente `expression`, stores con mundo cerrado (P17.6) y tablas de tokens (P17.10), clasificación de §5, target que expone `RegistryEvaluation`, `InspectBinding` de P24.6, resolver con expresiones y dominio, BOM, restamp, exportación y presupuesto JSON; guardas G2, G3, G12, kinds de binding y tokens explícitos | RED→GREEN; fixture dorado literal-only; CI del paso 1 | NO AUTORIZADA |
| **G9** | Mutaciones y propagación | `ChangeDefinition`, `Create` y `Rename` con intents enlazados; contrato raíz (P8.10); R1–R3; `PlanReadSet` y re-lectura de commit; `Delete` y `UnlinkAllAndDelete` con los bloqueos de §7.1; `RepairBrokenRack` generalizado con su confirmación (§7.2); descubrimiento por conjunto con oráculo; un resolve por rack; guardas G13 y G14 | RED→GREEN | NO AUTORIZADA |
| **G10** | UX | RACKVARIABLES con enlace contra el snapshot, campo «Definición», selector y panel de diagnóstico (P22); sesión y control del `LinkedPropertyEditor` con la sintaxis de §4 y los pines de P23.12; reconciliador con estado aislado y versión calculada (P23.15–P23.16); `Describe` de la ventana Selectiva tras el procedimiento de colisión; guardas G18 y G20 | RED→GREEN; suite UI | NO AUTORIZADA |
| **G11** | Candidato | Pruebas reales de cadena hasta geometría y BOM; las dos suites en local; builds Debug de UI y Plugin; CI verde sobre el SHA exacto; medición de coste de la propagación (R5); autoridad única de unidades completa (P9.7) | Clases de evidencia de AGENTS.md y WORKFLOW §4–§5 | NO AUTORIZADA |
| **G12** | Owner Validation + integración | Checklist de §8.3 en AutoCAD 2025; integración **serializada** con I-50; cierre documental (HANDOFF y ROADMAP); CI posterior al merge | WORKFLOW §4.5 | NO AUTORIZADA |

### 8.2 Reglas de la secuencia

- G5 y G7 **no** tocan producción fuera del núcleo nuevo. G6 solo toca, fuera del núcleo, las declaraciones de los
  alias de P9.6, con comportamiento idéntico. G8 es el primer gate que toca producción de Project Variables o de
  persistencia, y G10 el primero que toca UI.
- En G8, el paso 1 (caracterización, solo pruebas) se integra en la rama con CI verde **antes** de cambiar ningún
  store (condición C4 de §6.2).
- Antes de editar **cualquier** archivo productivo, cada gate de G5 a G10 ejecuta el procedimiento de colisión de
  [`decisions/I-49.md`](../automation/decisions/I-49.md) §8 contra I-50, y comprueba el `main` vigente, que ya
  incluye I-51 (§12).
- El Plugin se toca solo donde G9 o G10 lo exijan, archivo por archivo y con la comprobación anterior.
- Un gate que descubra que necesita cambiar una invariante de I-47/I-48 más allá de §2.3 **se detiene** (§13).

### 8.3 Checklist propuesto de Owner Validation (G12, AutoCAD 2025)

1. Crear `AlturaBase` literal y `AlturaFinal = AlturaBase + Holgura` por expresión; comprobar el valor evaluado en
   RACKVARIABLES.
2. En RACKEDITAR, escribir en el **mismo** campo `6`, `=Holgura`, `=Holgura + 2` y `=(AlturaBase + Holgura) / 2`,
   confirmando cada uno con Enter; geometría y BOM reflejan el valor.
3. Teclear una expresión y perder el foco: **no** se aplica y el borrador sigue ahí. Pulsar Escape: vuelve lo
   comprometido.
4. Con dos campos con borradores, uno inválido, pulsar Actualizar: **no** se aplica ninguno y el foco va al culpable.
5. Unidades: `=100[mm] + 2`, `=20[ft]` y `=4[in]` dan los valores esperados en pulgadas.
6. `=Holgura` + Enter y seleccionar `Holgura` en la lista dejan **el mismo** vínculo guardado.
7. Nombres: `=Holg` + Enter se rechaza; con dos `Holgura General`, `=Holgura General` + Enter da un diagnóstico con
   las dos formas cualificadas, y `=Holgura General#<id>` enlaza ese id; `Holgura-Base` sin llaves da
   `OperatorInName` y la lista inserta `{Holgura-Base}`.
8. Cambiar `AlturaBase`: se redibujan los racks de `AlturaFinal` y los que usan una expresión con ella, con **un**
   solo `Regen`.
9. Renombrar `AlturaBase`: las expresiones muestran el nombre nuevo, sin redibujo. Crear un homónimo: la referencia
   pasa a mostrarse cualificada.
10. Contrato raíz: en RACKVARIABLES, `-2` y `=0 - 2` se rechazan igual; `=Base + (A - B)` con `A < B` y raíz
    positiva se acepta; un cambio de `A` que dejaría `AlturaFinal ≤ 0` se rechaza.
11. Intentar borrar `AlturaBase` con dependientes o con consumidores por expresión: bloqueado, con las listas.
    `UnlinkAllAndDelete` con una expresión consumidora: bloqueado, con rack, propiedad y fórmula.
12. Intentar crear un ciclo, o escribir una expresión con error de sintaxis: rechazado, sin mutación.
13. Estado semántico roto preparado como fixture (`ABS(A, B)` en un rack): RACKEDITAR no lo abre y remite a
    RACKVARIABLES. RACKVARIABLES muestra la causa, y la confirmación de reparación muestra fórmula, causa y literal
    congelado. Tras reparar, el rack abre. Los racks no afectados funcionan, y RACKBOMTOTAL aborta solo si incluye el
    rack afectado.
14. Estado estructural roto preparado como fixture (nodo desconocido): registro ilegible, sin reparación ofrecida y
    sin escritura.
15. Guardar, cerrar y reabrir: definiciones, fuentes, valores y vínculos intactos.
16. Con el DLL de `a4d88f1`: un dibujo sin expresiones funciona como hoy; uno con expresiones en el registro o en un
    rack falla cerrado, sin escritura.
17. RACKBOMTOTAL con racks ligados por referencia y por expresión.

---

## 9. ADR REQUIRED — número NO asignado, todavía no escrito

- **Por qué es obligatorio.** V3 cambia contratos persistidos (P17), extiende ADR-0034 (§2.3) y toma decisiones de
  arquitectura (DR-1 a DR-8). WORKFLOW §8 y el contrato §3.1 exigen que el ADR esté **aceptado antes de implementar**.
- **Número NO asignado.** Se asigna en G4 contra la numeración remota de ese momento. ADR-0035 es de I-50 y ya está
  **aceptado** en su rama. I-49 no reserva ningún número de antemano.
- **No se escribe en G2C.**
- **Lo que tendrá que explicar de forma explícita**, tras G3:
  - **la extensión de ADR-0034 que exigen las fórmulas en fuentes de propiedad**: un tercer caso
    `Expression(BoundExpression)` junto a `Literal(T)` y `ProjectVariableReference(VariableId)` (ADR-0034 §4), por
    qué ID22B extiende también el lado de la propiedad que ADR-0034 §15 asociaba a ID21, cómo quedaría un futuro caso
    de ID21 y que la referencia directa se conserva con `=X` canónico;
  - las dos superficies de expresión y la revisión del contrato del editor de I-48, con `CASO_6` y `CASO_8` (P23.12);
  - el motor adimensional y las **tres fronteras de validez**: motor sin dominio, contrato raíz del `VariableType`
    igual para literal y expresión, y dominio del consumidor sobre toda fuente, con el endurecimiento declarado
    (§0.2, P8.10, P24.5);
  - la sintaxis y la autoridad **única** de unidades, con ADR-0005 intacto (P9);
  - el conjunto de funciones inicial y los candidatos diferidos (P10);
  - V-0, sus cuatro condiciones y la **regla de evolución** (§6);
  - la clasificación estructural/semántica, R1–R3 y el alcance del bloqueo (§5);
  - la sintaxis de referencias y la precisión sobre `#<GUID>` (§4);
  - los veredictos de `UnlinkAllAndDelete` y `RepairBrokenRack` (§7);
  - el enlace contra el snapshot mostrado y el `PlanReadSet` (P7.7, P21.6);
  - el mundo cerrado de los payloads de Expression, las tablas de tokens y la política de `ExtensionData` que no
    cambia (P17.6, P17.10, P17.12);
  - la preservación de C4-9 en las ramas que I-49 reescribe y el aislamiento de estado (P23.15–P23.16);
  - la gramática numérica invariante como excepción acotada a ADR-0015 (P1.11);
  - los namespaces y el ámbito reservados para ID20 (P25.2–P25.3);
  - las alternativas rechazadas (§10.1).
- **Relación con los ADR vigentes.** Extiende ADR-0034; no sustituye ADR-0005, ADR-0006, ADR-0012, ADR-0015,
  ADR-0021 ni ADR-0025. ADR-0034 está aceptado y es **inmutable**: no se edita.
- **Contrato de I-49.** Su §11.2 y su §12 prevén exactamente esta vía («salvo ADR nuevo aceptado por el Owner»). La
  implementación sigue bloqueada hasta que ese ADR esté aceptado, y el texto del contrato se alinea en G4.

---

## 10. Alternativas y coste

### 10.1 Alternativas consideradas

| # | Alternativa | Decisión en V3 |
|---|---|---|
| ALT-1 | Expresiones **solo** en `ProjectVariable.Definition` (DR-1 de V1) | **Rechazada**: contradice el requisito del Owner de dos superficies |
| ALT-2 | Un **`FormulaTextBox`** o control paralelo para fórmulas | **Rechazada** por requisito del Owner: se usa el mismo control de I-48 (P23.1) |
| ALT-3 | Cada expresión de propiedad como **variable implícita** del registro (opción b de Discovery §9.7) | **Rechazada**: llena el registro de variables que nadie creó, con identidad y ciclo de vida por rack, y hace que rename y delete dependan de propiedades |
| ALT-4 | Fuente de propiedad **solo `Expression`**, sin canonicalizar `=X` | **Rechazada**: `=X` tendría dos representaciones persistidas (P2.8) |
| ALT-5 | Persistir el **texto con nombres** | **Rechazada**: un rename rompería las expresiones y el nombre pasaría a ser autoridad (ADR-0034 §2, HANDOFF §4) |
| ALT-6 | Persistir **texto canónico con ids** en una cadena | **Rechazada**: acoplaría la persistencia a la sintaxis de §4 y exigiría un parser en cada lectura; el árbol JSON cerrado (P17.4, P17.6) no lo necesita |
| ALT-7 | **Cachear el valor evaluado** | **Rechazada**: segunda autoridad que puede quedar obsoleta (P17.5, P14.9) |
| ALT-8 | **Persistir el grafo** de dependencias | **Rechazada**: derivable, y persistido podría divergir (P12.1) |
| ALT-9 | **Sistema de tipos dimensional** (`Length \| Scalar`, `DimensionMismatch`), como proponía V1 | **Rechazada** por requisito del Owner |
| ALT-10 | Conversión de unidades vía **`StructuralSectionUnits`** | **Rechazada**: haría depender el núcleo de un helper de catálogo; la dirección es la inversa (P9.7) |
| ALT-11 | Un **parser de unidades por capa** (UI, evaluador, Domain) | **Rechazada** por requisito del Owner: un lexer y una autoridad (P1.10, P9.5) |
| ALT-12 | **`ROUND`, `CEILING` y `FLOOR`** ya en V3 | **Diferida**: semántica abierta (P10.6); confirmado por el Architect |
| ALT-13 | Versión **V-1** (minor por contenido) o **V-2** (major) | **Rechazada**: V-0 CLOSED (§6) |
| ALT-14 | Evaluador de **terceros** (`NCalc`, `DataTable.Compute`, `System.Linq.Expressions`, Roslyn) | **Rechazada**: ADR-0012; semántica sensible a cultura o no acotada (P1.10) |
| ALT-15 | Núcleo **dentro** de `RackCad.Application.ProjectVariables` | **Rechazada**: arrastraría esa dependencia a ID23 (Discovery §16.3) |
| ALT-16 | **Borrado en cascada** o materialización automática de dependientes | **Rechazada**: reescribe en silencio definiciones del usuario (ADR-0034 §11; P20.2) |
| ALT-17 | Núcleo en un **ensamblado separado** | **Diferida**: frontera más dura, pero añade un proyecto a builds, CI y despliegue; V3 usa namespace + guarda (P26.5) |
| ALT-18 | Gramática **localizada** (`,` decimal y `;` separador) | **Rechazada**: doble significado de la coma (Discovery §11.7); mitigado con `AmbiguousDecimalComma` (P1.5) |
| ALT-19 | **`FatalUnevaluable`** no reparable para fallos semánticos de una fuente (V2) | **Rechazada** (A-01): deja un rack sin corrección posible |
| ALT-20 | **Materializar fórmulas** en `UnlinkAllAndDelete` (V2) | **Rechazada** (A-02): corta dependencias que nadie pidió tocar |
| ALT-21 | **Sin contrato en la raíz** de las variables definidas por expresión, con `A = -2` válido (remedio del Architect para A-03) | **Rechazada por el Coordinador**: la validez dependería de la sintaxis (§0.2) |
| ALT-22 | **`VariableType` como álgebra** o dominio dentro del motor | **Rechazada**: el motor es adimensional (DR-5, P5.5) |
| ALT-23 | Enlazar el texto contra el **registro re-leído** (V2) | **Rechazada** (A-04): un nombre cruzaría una frontera entre lecturas (P7.7) |
| ALT-24 | Comparar en commit **solo los valores de `I`** (V2) | **Rechazada** (A-05): no cubre lo que el plan leyó (P21.6) |
| ALT-25 | Forma no canónica como **error estructural** (V2) | **Rechazada** (A-07): bloquearía todo el dibujo por un estado que la build entiende |
| ALT-26 | **Eliminar `ExtensionData` de forma global** | **Rechazada**: rompería la preservación histórica fuera de los payloads de Expression (P17.12) |
| ALT-27 | **Operación o comando nuevo** de reparación de fórmulas | **Rechazada**: `RepairBrokenRack` basta con el conjunto ampliado (§7.2) |
| ALT-28 | **`#<GUID>` sin nombre como entrada** que enlaza con un id presente | **Rechazada**: esa forma es la presentación de un estado roto (§4.2, regla 11) |
| ALT-29 | **Fragmento de GUID** como cualificador (8 caracteres) | **Rechazada**: dos ids con el mismo primer grupo colisionarían (P3.9) |
| ALT-30 | **Arreglar `WithDesign`** en el portador para no compartir `PropertyValues` | **Rechazada en I-49**: el portador es de I-50 y el aislamiento se consigue en la frontera de I-49 (P23.16); L3 queda registrado |

### 10.2 Coste estimado (INFERENCE, se valida gate a gate)

| Área | Superficie estimada | Base |
|---|---|---|
| Núcleo nuevo | ≈14–20 archivos en `RackCad.Application.Expressions`, más la autoridad de unidades | P1–P16, P9.5 |
| Project Variables y persistencia | ≈22–28 archivos existentes: las ubicaciones que asumen literal; sesión, fuente, estado, opciones y reconciliador del editor; inspección, kernel y sonda; descubrimiento, preflight, commit, plan, workspace e intents; `SelectivePropertyValueDocument`; `VariableType` | Discovery §5.5, §9.7, §3.1–§3.2 |
| Selectivo en Application | ≈4: resolver, apertura del editor, exportación y descriptores con dominio | Discovery §3.2; P24.5 |
| Alias de unidades | 4 declaraciones: `StructuralSectionUnits`, `SelectiveGeometryResolver`, `DynamicHeaderHeightCalculator` y `CantileverArmFrameResolver` | P9.6–P9.7 |
| Plugin | 0–3 | Discovery §3.3 |
| UI | ≈4: `LinkedPropertyEditor` (inserción de candidatos), `RackProjectVariablesWindow.xaml` y `.cs` (campo y panel), texto de reparación; `RackSelectiveWindow`, un miembro | Discovery §3.4; P23.13 |
| Pruebas | Núcleo nuevo, ≈20 archivos existentes, la evolución de P28.4 y los 21 escenarios de P28.5 | Discovery §14 |
| Comandos / ventanas nuevos | 0 / 0 | P22.1, P23.14 |

**Costes que no se minimizan:**

- Nace la **primera** ruta productiva nombre→id, en dos superficies y con una sintaxis de identidad (P7, §4).
- Se revisa el contrato del editor de I-48 que el Owner validó: `CASO_6` y `CASO_8` (P23.12).
- El panel de diagnóstico y la confirmación de reparación de RACKVARIABLES crecen (P22.9, §7.2).
- El commit re-evalúa el `PlanReadSet` (P21.6).
- Una referencia directa persistida a una variable negativa, solo alcanzable por edición externa, deja de resolver
  (P24.5).
- En una build anterior, un rack con expresión bloquea en todo el dibujo las operaciones de variables que necesitan
  descubrimiento (P18.2).
- La propagación sobre dibujos grandes sigue **sin medir** hasta G11.
- Evolucionan varias guardas y pines vigentes, y nacen guardas nuevas (P28.4).
- `RackSelectiveWindow` y `SelectiveGeometryResolver.cs`, archivos que también toca I-50, necesitan un cambio cada uno
  (P23.13, P9.7).

---

## 11. Reconciliación con el Discovery G1

### 11.1 Las 23 preguntas abiertas (Discovery §18)

| # | Pregunta del Discovery | Respondida en |
|---|---|---|
| 1 | Dónde vive una expresión | DR-1; P2.7–P2.8; P23; P24 |
| 2 | Representación persistida | P4; P17.2–P17.4; ALT-5, ALT-6 |
| 3 | Versión del registro | P18; §6 |
| 4 | Token y payload del nuevo kind; `ExtensionData`; `PropertyNameCaseInsensitive` | P17.2–P17.4, P17.6, P17.10, P17.12 |
| 5 | Evaluación: dónde, cuándo y si se persiste | DR-3; P14.2, P14.7–P14.9; P17.5; P21.2, P21.6 |
| 6 | Grafo y ciclos | P12; P13; P15.3 |
| 7 | Propagación transitiva | P21 |
| 8 | Ciclo de vida con dependientes | P19; P20; P21; §5; §7 |
| 9 | Tipos | DR-5; P5.4–P5.7; P8.7–P8.10 |
| 10 | Unidades | P1.6; P9 |
| 11 | Gramática numérica | P1.4–P1.5, P1.11; P22.3 |
| 12 | Nombres dentro de expresiones | P7; §4 |
| 13 | Editor | P23 |
| 14 | RACKVARIABLES | P22 |
| 15 | ID20 (preparar) | P5.2–P5.3; P25 |
| 16 | ID23 (independencia) | DR-2; P26 |
| 17 | ID28/ID29 (preservar) | P16.3; P27 |
| 18 | Guardas de fuente | P28.4 |
| 19 | Compatibilidad de DWG I-47/I-48 | P18.2; P24.11; P28.3 (G8); P28.5 (T-V3-21); P29; §8.3 (16) |
| 20 | ADR | §9 |
| 21 | Alcance de sistemas y propiedades | DR-2; P28.4 (G17); P30.15 |
| 22 | Semántica numérica del resultado | P14.4; P8.10; P21.7; P24.5 |
| 23 | Hallazgos laterales L1–L5 | P30.17; G4 |

### 11.2 Los 16 riesgos (Discovery §17)

| Riesgo | Tratamiento en V3 |
|---|---|
| R1 — legibilidad en builds anteriores | V-0 con fail-closed por kind y mundo cerrado del payload (§6); coste declarado; caracterización en G8 y verificación con el DLL de `a4d88f1` (§8.3, punto 16) |
| R2 — segunda autoridad de valor | `RegistryEvaluation` como único dueño; expresiones de propiedad dentro del resolver único; sin valores cacheados (P14.9; P17.5; P24.5) |
| R3 — guardas de fuente | Evolución explícita, en el mismo gate y sin renombrar para esquivarlas (P28.4) |
| R4 — profundidad 1 embebida | Cierre transitivo, consumidores de cualquier variable afectada, un resolve por rack y un plan (P21.2) |
| R5 — coste sin medir | Descubrimiento por conjunto en un barrido (P21.4); grafo `O(V + E)` (P12.6); medición en G11 |
| R6 — nombres en fórmulas | Sintaxis cerrada (§4); binder como única ruta nombre→id, solo al comprometer y solo contra el snapshot mostrado (P7) |
| R7 — gramática numérica divergente | Gramática invariante con `AmbiguousDecimalComma`; los literales sin `=` conservan su regla (P1.4–P1.5, P1.11); convergencia fuera de alcance (P30.18) |
| R8 — unidades | `[mm]`, `[in]` y `[ft]` como conversión numérica con una autoridad única real (P9) |
| R9 — acoplamiento al Selectivo | Núcleo y autoridad de unidades neutrales, sin `VariableType` en el núcleo, protegidos por guarda (DR-2; P5.1; P26) |
| R10 — semántica actual de `=` | **Revisada por requisito del Owner**, con las garantías de I-48 preservadas y la lista exacta de pines que cambian (P23.2, P23.12) |
| R11 — pérdida de trazabilidad | `BoundExpression`, `SymbolId`, diagnósticos con causa raíz, conjuntos afectado y leído y traza opt-in en memoria (P27) |
| R12 — colisión potencial con I-50 | Sin tocar `WithDesign` ni el portador; aislamiento en la frontera de I-49; `RackSelectiveWindow` limitado a un miembro; alias de `FootInches` sujeto al procedimiento de colisión (P17.11; P23.13; P23.16; P9.7; §12) |
| R13 — defectos laterales heredados | Fuera de alcance, con registro en G4, salvo la preservación de invariantes en ramas que I-49 reescribe: L2 (P23.15) y el token `Type` (P17.10) |
| R14 — extensión muerta | Pruebas por el camino operativo store → acreditación → evaluación → resolver → geometría y BOM, en las dos superficies (P28.3, G8 y G11) |
| R15 — caminos heredados sin disparador | No se extienden (P29.6) |
| R16 — materializar valores calculados | Solo en `UnlinkAllAndDelete` (referencias directas) y exportación, con el valor evaluado exacto (P21.9) |

---

## 12. Coordinación con I-50 e I-51

**Observado en G2C** (`git fetch --all --prune`, 2026-09-12, justo antes del commit):

```text
origin/architecture/motor-expresiones-parametricas = 9aef7d04d9e65b4225af2cb36750ee636a6689d5  (= Proposal V2)
origin/main                                        = 46fcac2b071929d2bd5b07aa28373941417f74a8  (merge de I-51)
I-50  origin/feature/cotas-independientes-por-vista  = 7a9335cf866b871d855df711c398a98809cef775
I-51  origin/feature/rackduplicar-multiples-origenes = retirada tras integrar en main
```

### 12.1 I-50

- **FACT** — Desde V2 avanzó seis commits: `974c709` (G4 Paso 1, solo pruebas), `fb039e1` y `11c04db` (G4 Paso 2,
  con producción), `94220fb` (G5 (A), «la politica viaja por el Selectivo»), `695d34b` (G5 (A.1), solo pruebas) y
  `7a9335c` (G5 (B), «la politica viaja por el Dinamico»). Durante G2C pasó de `11c04db` a `7a9335c`. Su cambio
  acumulado frente a `main` fuera de `docs/` son 17 archivos de `src/` y 17 de pruebas.
- **FACT** — `94220fb` modifica dos archivos que V3 menciona:
  - `SelectivePalletDesignDocument.cs`: propiedad `DimensionViews` y sus mapeos en `From` y `ToDomain` (C-06);
  - `SelectiveGeometryResolver.cs`: una línea en `:67` (C-04).
- **FACT** — `7a9335c` solo toca el Dinámico (`DynamicRackSystemDocument.cs`, `DynamicAnnotationOptions.cs`,
  `DynamicEditorDesignAssembler.cs`, `DynamicRackSystemResolver.cs`, `DynamicRackDesign.cs`) y pruebas. Ninguno está
  en el plan de I-49, y `DynamicHeaderHeightCalculator.cs` no cambia.
- **FACT (doc de I-50)** — Su Proposal V1.2, congelada, declara que **no** se tocan `WithDesign` ni el reconciliador
  (`docs/initiatives/I-50-proposal-v1.2.md:302-304` en su rama). Tampoco `LinkedPropertyReconciler`,
  `SelectiveEffectiveDesignResolver`, `SelectiveAuthoredAuthority`, `SelectiveLibraryExport`, `RackEnvelopeRestamp`
  ni `src/RackCad.Plugin` (`:464-468`). También dice que «No cambia: … `LinkedPropertyEditor`, Project Variables,
  Expression Engine» (`:58-59`) y que «Ninguna `SchemaVersion` cambia» (`:175`).
- **Cruces con V3:**

  | Archivo | I-49 V3 | I-50 | Tratamiento |
  |---|---|---|---|
  | `RackSelectiveWindow.xaml.cs` | Un miembro, `Describe` (P23.13), en G10 | `BuildDesign` / `LoadDesign` en su G3 | Procedimiento de colisión antes de editar; integración serializada |
  | `SelectivePalletDesignDocument.cs` | **Ninguno**: V-0 y aislamiento en la frontera de I-49 (P23.16) | Modificado en `94220fb` | Sin cruce. Las citas de V3 a `:316-330` y `:321` son de `a4d88f1`; en la rama de I-50 se desplazan |
  | `SelectiveGeometryResolver.cs` | Alias de `FootInches` en `:31` (P9.7), en G6 | Modificado en `94220fb` (`:67`) | **Colisión potencial bajo el override.** I-49 no edita el archivo sin el procedimiento de colisión. Si este lo declara material, el alias espera a la integración de I-50, y G11 no se abre sin la autoridad única completa (§13) |
  | `LinkedPropertyReconciler.cs`, `SelectiveEffectiveDesignResolver.cs`, `SelectiveAuthoredAuthority.cs`, `SelectiveLibraryExport.cs` | Sí (P23.15–P23.16, P24) | No cambian (`:464-468`) | Sin cruce. I-50 depende del camino `WithDesign → From(design)`, que V3 conserva sobre el clon |
  | `SelectivePropertyValueDocument.cs`, editor vinculable, Project Variables y núcleo | Sí | No (`:58-59`) | Sin cruce |
  | `StructuralSectionUnits.cs`, `DynamicHeaderHeightCalculator.cs`, `CantileverArmFrameResolver.cs` | Alias (P9.7), en G6 | No | Sin cruce |

- **Prueba cruzada**: la segunda iniciativa en integrar añade una prueba en la que un Selectivo con una fuente
  `expression` y `DimensionViews` presentes sobrevive a RACKEDITAR con los dos intactos.
- **Condiciones de `OWNER_OVERRIDE_I49_I50_PARALLEL`**: G2C solo crea este documento, así que la condición de
  colisión **no se activa**.

### 12.2 I-51

- **FACT** — Durante G2C quedó **INTEGRADA y CERRADA** en `main` (merge `46fcac2`, «Merge I-51: RACKDUPLICAR duplica
  varios racks y vistas en un solo gesto»), y su rama remota se retiró. Su producción integrada:
  `src/RackCad.Application/Persistence/RackDuplicationPlan.cs`, `src/RackCad.Plugin/RackDuplicarCommands.cs`,
  `src/RackCad.Plugin/RackEnvelopeRestamp.cs` y tres archivos de pruebas, entre ellos
  `SelectiveDuplicationFailClosedTests.cs`. Ninguno lo cita ni lo modifica V3.
- **FACT (HANDOFF de `main`)** — Registra que «**I-49** —motor de expresiones— declara que duplicar y re-estampar no
  cambian», y que con I-49 los conflictos previstos son solo documentales (`docs/HANDOFF.md:1564-1570` en
  `46fcac2`). V3 lo mantiene (P24.8).
- **Consecuencia**: la rama de I-49 no contiene ese merge. Se reconcilia con `main` según WORKFLOW, y en todo caso
  antes de escribir la prueba de P24.8, que tiene que ejercitar el restamp integrado.
- **Condición de parada**: si un gate de I-49 necesitara cambiar la API o la semántica de `RackDuplicationPlan`,
  del restamp, de `SelectiveAuthoredAuthority`, del portador o del store del Selectivo, se detiene **antes de
  editar** y lo reporta (contrato §12): ya es código de `main` validado por el Owner. El override **no** cubre este
  caso.

### 12.3 Conflictos documentales previstos

- I-51 ya integró su fila de ROADMAP y su registro final en `docs/ideas-futuras.md`. I-49 e I-50 insertan las suyas
  sobre esa versión de `main`.
- I-49 registrará sus seguimientos en `docs/ideas-futuras.md` en G4.
- Numeración de ADR: 0035 es de I-50, ya aceptado en su rama (§9).
- Precedente, solo como hecho de coordinación: I-50 tampoco cambia ninguna `SchemaVersion` sobre el mismo portador
  (P18.7).

La integración es **serializada**, y la segunda en integrar se reconcilia con `main`.

---

## 13. Condiciones para detenerse

Se heredan todas las del contrato §12. Sobre la que dice «salir del punto de extensión de ADR-0034 §15: detenerse; eso
es ADR nuevo y decisión del Owner», V3 deja constancia de que **la decisión del Owner existe** (encargo de G2A.1) y de
que **el ADR es obligatorio** antes de cualquier producción (§9). Además:

- Si la re-revisión exige **cambiar** una invariante de I-47/I-48 más allá de lo que enumera §2.3: detenerse; eso es
  ADR y decisión del Owner.
- Si el núcleo o la autoridad de unidades no pueden quedar **independientes** de Project Variables y de
  `StructuralSections` (P26.1): detenerse.
- Si la evolución de una guarda debilitara la protección de **ID21** (P28.4): detenerse.
- Si aceptar expresiones en el editor exigiera renunciar a alguna garantía de P23.2: detenerse. No se relaja una
  garantía para acomodar la implementación.
- Si el contrato raíz no pudiera aplicarse igual a literal y expresión (P8.10): detenerse. La validez no se relaja
  por la sintaxis.
- Si una corrección exigiera un comando nuevo de reparación o convertir una fórmula en literal de forma implícita
  (§7): detenerse.
- Si G11 llega sin la autoridad única de unidades completa, por ejemplo con `FootInches` pendiente de la integración de
  I-50 (§12.1): detenerse y escalar al Owner.
- Si la medición de G11 muestra un coste de propagación inaceptable: detenerse y escalar.
- Colisión productiva material con I-50, o necesidad de cambiar código integrado por I-51: detenerse **antes de
  editar** (§12).
- Cualquier edición de producción antes de G3 y del ADR aceptado en G4: **prohibida**.

---

## 14. Estado

```text
COORDINATOR PROPOSAL V3 — ARCHITECT REVIEW V2 RECONCILED — NOT CONSENSUS
Implementation remains BLOCKED

Proposal Version = V3
Coordinator      = AGREED WITH V3
Architect        = PENDING RE-REVIEW
Open A           = CLOSED
Open B           = CLOSED
Schema           = V-0
Implementation   = BLOCKED
ADR              = REQUIRED, not yet written

V2 → Architect Review → Coordinator Reconciliation (§0):
  A-01 BLOCKER  semantic repair; FatalUnevaluable retirado            ACEPTADA
  A-02 HIGH     UnlinkAllAndDelete bloqueado por fórmulas             ACEPTADA
  A-03 HIGH     tres fronteras; contrato raíz igual por sintaxis      ACEPTADA EN PRINCIPIO · MODIFICADA
  A-04 HIGH     enlace contra el snapshot mostrado                    ACEPTADA
  A-05 MEDIUM   PlanReadSet contractual                               ACEPTADA
  A-06 MEDIUM   núcleo sin VariableType                               ACEPTADA
  A-07 MEDIUM   NonCanonicalForm semántico                            ACEPTADA
  A-08 MEDIUM   payloads de Expression con mundo cerrado              ACEPTADA · alcance precisado
  A-09 MEDIUM   versión calculada, nunca "2.0" fijo                   ACEPTADA · preservación de invariante
  A-10 MEDIUM   confirmación de reparación consciente de la fuente    ACEPTADA · misma operación
  A-11 LOW      V-0 justificada por los lectores                      ACEPTADA
  A-12 LOW      materialización acotada                               ACEPTADA
  A-13 LOW      una sola autoridad real para 25.4 y 12                ACEPTADA · ampliada
  A-14 LOW      RegistryEvaluation, único dueño de valores            ACEPTADA
  A-15 LOW      estado aislado; persistencia ≠ memoria                ACEPTADA · elevada a MEDIUM práctico
  A-16 LOW      límites normativos; texto ≥ 4000                      ACEPTADA
  A-17 LOW      tablas cerradas de tokens persistidos                 ACEPTADA
  A-18 LOW      literal canónico con la regla de su superficie        ACEPTADA

Veredictos cerrados:
  OPEN A          §4   sintaxis del Architect; #<GUID> sin nombre solo para referencias rotas
  OPEN B          §5   estructural ≠ semántico; bloqueo por cierre afectado; R1–R3
  Schema          §6   V-0 con cuatro condiciones y regla de evolución
  Operaciones     §7   UnlinkAllAndDelete bloquea fórmulas y dependientes;
                       RepairBrokenRack única, conjunto semántico ampliado, confirmación por fuente
  Funciones       P10  MIN, MAX, ABS; ROUND, CEILING, FLOOR diferidas
  Unidades        P9   [mm] [in] [ft] como conversión numérica; una autoridad real
  ID20 / ID23     P25 / P26   validados (ID23 con A-06)
  ID28 / ID29     P27  BoundExpression + diagnósticos + traza opt-in / dependencias + dependientes + conjuntos

30 puntos      = P1..P30 (§3)
Gates          = G0..G12 (§8): G0 y G1 HECHAS · G2 EN CURSO (V3) · G3 PENDIENTE · G4..G12 NO AUTORIZADAS

Base: Proposal V2   @ 9aef7d04d9e65b4225af2cb36750ee636a6689d5
      Discovery G1  @ 4cf02b167f183fb66d93988c9843296838277dc4
      Proposal V1   @ b15e40a076d7157ce8ca73537af9659049bf8576 (historial)

Siguiente paso: re-revisión del Architect sobre V3. G2C no la solicita y no autoriza implementación.
```

**Historial conservado:** [V2](I-49-proposal-v2.md) y [V1](I-49-proposal-v1.md) quedan **intactas**.
