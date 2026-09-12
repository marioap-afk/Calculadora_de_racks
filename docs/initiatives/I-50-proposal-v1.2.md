# I-50 — Proposal V1.2: visibilidad de cotas por tipo de vista

> # PROPOSAL V1.2 — NOT CONSENSUS
>
> # Implementación BLOQUEADA
>
> Documento de G2A, **solo documentación**. Queda sometido a revisión del **Coordinador** y a nueva revisión
> del **Arquitecto**. Mientras no estén de acuerdo **sobre esta misma versión**, no se escribe una sola línea
> de producción ni se abre G4 (contrato, sección 10). Además, **ADR-0035 debe estar `aceptado` por el Owner
> antes de cualquier código productivo** (confirmación del Coordinador).
>
> ```text
> Versión sometida    = V1.2   (V1 y V1.1 quedan INTACTAS como registro de sus rondas)
> Historial           = V1   (I-50-proposal-v1.md,   93c352a): Coordinator NOT AGREED — MATERIAL-01, MATERIAL-02
>                       V1.1 (I-50-proposal-v1.1.md, 0d7670c): Coordinator AGREED · Architect NOT AGREED — MAT-A1 (+ MIN-1..MIN-9)
> Base del análisis   = I-50-discovery.md (G1 CLOSED)
> Código auditado     = a4d88f18a1f42263d366c44dc05dd18a6786f152   (origin/main, sin avanzar)
> Decisiones entrada  = CD-01..CD-11 (contrato, sección 12) y confirmaciones del Coordinador (sección 1.2)
> Estado              = Coordinator: PENDING REVIEW (V1.2) · Architect: PENDING RE-REVIEW (V1.2) · Consensus: NOT REACHED
> Compuerta de código = consenso Coordinator + Architect sobre la MISMA versión Y ADR-0035 aceptado por el Owner
> ```
>
> Las citas `archivo:línea` se refieren a `a4d88f1`.

## Qué cambia en V1.2 respecto a V1.1 — leer esto antes de revisar

V1.2 **hereda íntegramente el contrato técnico de V1.1**. El diseño de datos **no se reabre**: siguen iguales
la representación, la semántica, la regla, el alcance de etiquetas, los sitios de copia, el orden de gates y
las decisiones. Solo cambia lo necesario para reconciliar la Architect Review de V1.1.

| # | Cambio | Origen | Dónde |
|---|---|---|---|
| 1 | **Tabla explícita gate → archivos / C-xx / pruebas que lo cierran.** C-05 (`FondoSystemView`) se **adelanta a G4** por ser parte obligatoria del camino productivo frontal; ningún otro sitio de copia de G5 se adelanta. T-06 y T-07 se cierran en G4 sobre sistemas ya resueltos y se repiten extremo a extremo en G5; T-08 y C-15 se cierran en G6. **El orden no cambia**: G4 → G5 → G6 → G3 → G7 → G8 | MAT-A1 | 14.3, 12, 13.1, 13.2 |
| 2 | «14 sitios de copia» pasa a **15** (C-01..C-15). Ya no se afirma que todos se prueben en T-13: C-01, C-07 y C-12 se cubren en T-17, T-18 y T-19 | MIN-1 | 2, 12, 13.2 |
| 3 | T-12 nombra `RackProject.ForDynamic(system)` y `RackProject.ForDynamic(design)` como conversiones indirectas que dependen de C-11. No es un sitio de copia nuevo porque no requiere código propio | MIN-2 | 13.2 |
| 4 | T-17 **fuerza una transición real** de casilla durante `LoadDesign` y exige `DimensionViews = null` al guardar un legacy; T-18 y T-19 hacen lo equivalente. La implementación deberá impedir que cargar, recargar o recalcular marque «tocado», sin fijar el mecanismo ahora | MIN-4 | P-11, 13.2 |
| 5 | T-05 (caracterización Push Back) activa numeración y nombre para caracterizar también el alcance de las etiquetas | MIN-5 | 13.2 |
| 6 | «Cualquier entero presente» pasa a **«cualquier `int` (Int32) presente»**. Un JSON no entero o fuera de Int32 conserva la deserialización fallida de siempre; I-50 no amplía la tolerancia | MIN-6 | 2, P-03, P-04, P-07, P-11 |
| 7 | Evidencia: T-03..T-05 en un **commit test-only propio** con CI sobre ese SHA exacto antes de cualquier producción; toda evidencia RED demuestra más de 0 pruebas seleccionadas; ningún RED se publica como tip roto ni cruza un relevo o una sesión | MIN-7 | 13.1 |
| 8 | **Matriz de trazabilidad** del contrato de aceptación: qué cubre una prueba automatizada y qué exige Owner Validation porque atraviesa el Plugin | MIN-8 | 13.6 |
| 9 | MIN-3 (helper puro de lectura para la UI) **no es requisito de consenso**: queda como posible decisión local de G3, sin abstracción contractual nueva | MIN-3 | 14.2 |
| 10 | Correcciones de redacción en ADR-0035 (autoridad multivista demostrada solo en el Selectivo; conducta de builds anteriores por sistema). El ADR sigue `propuesto` | MIN-9 | ADR-0035 |
| 11 | Preguntas QA-1, QA-3, QA-4 y QA-5 **respondidas** por el Arquitecto; ninguna abierta | Architect Review de V1.1 | 16 |

## 0. Qué decide esta Proposal, y qué no

**Decide**, para las decisiones `CD-01`..`CD-11` ya tomadas:

- la **representación** del dato, comparando **solo** dos formas (sección 2);
- el formato de cable, la semántica de nulo/legacy y el tratamiento de valores desconocidos;
- la **regla única** `EffectiveDetail(detail, policy, viewKind)` y cómo gobierna el alcance de las etiquetas;
- la lista completa de sitios de copia, el comportamiento en cada flujo (guardar y reabrir, `RACKEDITAR`,
  Actualizar, vista enlazada nueva, `RACKDUPLICAR`), las pruebas RED previstas y **qué cierra cada gate**.

**No decide**: nombres internos de controles WPF ni su disposición fina (G3), ni nada de lo que las `CD` o
las confirmaciones del Coordinador ya fijaron. El orden de gates ya está **aprobado** (sección 14.3).

**No cambia**: geometría, BOM, GUID, `View`/`Section`, sobre del DWG, comandos del Plugin, `RACKLAYOUT`,
catálogos, `LinkedPropertyEditor`, Project Variables, Expression Engine ni los sistemas sin cotas.

## 1. Decisiones de entrada (vinculantes)

### 1.1 Decisiones del contrato

| ID | Resumen | Consecuencia para esta Proposal |
|---|---|---|
| `CD-01` | Autoridad = rack × tipo de vista | El dato vive en el **diseño** del rack, igual en todas sus vistas |
| `CD-02` | Tipos = Frontal / Lateral / Planta; `Section` no crea tipo | Tres valores; salida y entrada, y los cuatro cortes frontales de Push Back, comparten Frontal |
| `CD-03`, `CD-04` | `DimensionDetail` y `DimensionStyle` siguen globales | No se tocan; la política solo apaga o enciende |
| `CD-05` | Solo ON/OFF por tipo | Un bit por tipo; no hay nivel por vista |
| `CD-06` | `Dimensions = None` siempre gana | Primer caso de la regla (sección 4) |
| `CD-07` | Nulo = legacy exacto; no se materializa «todas» al guardar sin tocar | Sección 3 y sección 8 |
| `CD-08` | Metadata por instancia rechazada | Ni sobre ni referencia; el Plugin no cambia |
| `CD-09` | H1–H7 fuera de alcance | Push Back sigue sin control de estilo (sección 7.3) |
| `CD-10`, `CD-11` | I-49 en paralelo; I-51 solo documental | Protocolo de archivos compartidos (sección 15) |

### 1.2 Confirmaciones del Coordinador (revisión de V1), vigentes

- representación B, `[Flags]` nulo, **sin miembro `All`**;
- `null` = legacy exacto;
- `DimensionDetail` y `DimensionStyle` globales;
- visibilidad ON/OFF por Frontal, Lateral y Planta;
- salida y entrada del Dinámico comparten Frontal, y todos los frontales de Push Back comparten Frontal;
- `EffectiveDetail` como regla única, y el **mismo** detalle efectivo para el alcance de las etiquetas;
- autoridad en el diseño, nunca en el sobre ni en la instancia;
- P-11, «sin tocar no materializa»;
- la lista de sitios de copia C-01..C-15;
- **cero** cambios productivos previstos en el Plugin;
- orden de ejecución **G4 → G5 → G6 → G3 → G7 → G8**;
- `requires_owner_decision: true`;
- **ADR-0035 debe estar `aceptado` por el Owner antes de cualquier código productivo**;
- **I-50 NO modifica `RACKLAYOUT`**: la variación de la huella al ocultar las cotas de Planta queda como
  consecuencia explícita del ADR y punto de Owner Validation, sin ampliar el alcance para corregirla;
- I-49 e I-51 siguen en paralelo conforme al protocolo existente.

## 2. Representación: A frente a B

- **A — lista nula de tokens.** DTO: `List<string> DimensionViews`, con los tokens del sobre
  `RackEmbedDocument.ViewFrontal`/`ViewLateral`/`ViewPlanta` (`"frontal"`, `"lateral"`, `"planta"`,
  `RackEmbedDocument.cs:28-30`). `null` = legacy; `[]` = ninguna.
- **B — `[Flags] DimensionViewVisibility` nulo.** Dominio: `DimensionViewVisibility?`; DTO: `int?` con bits
  fijos `Frontal = 1`, `Lateral = 2`, `Planta = 4`. `null` = legacy; `0` = ninguna.

| Criterio | A — lista de tokens | B — `[Flags]` | Evidencia |
|---|---|---|---|
| Nulo = legacy, «ninguna» explícito | `null` / `[]` | `null` / `0` | empate |
| Forma canónica frente a la autoridad multivista del Selectivo | **Frágil**: los arrays se comparan **en orden** y las cadenas con comparación **Ordinal**, así que `["planta","frontal"]` y `["frontal","planta"]`, o `"Frontal"` y `"frontal"`, son hermanas **divergentes** y abortan `RACKBOMTOTAL` y la propagación; obliga a canonicalizar orden, mayúsculas y duplicados en cada escritura | **Única por construcción**: un número se compara numéricamente | `SelectiveAuthoredAuthority.cs:162-226` |
| Semántica en los sitios de copia | Tipo por **referencia**: una asignación directa comparte la misma lista entre diseño, sistema, vistas por fondo y lados de Push Back; exige copia defensiva en cada sitio | Tipo por **valor**: la asignación copia | **15** sitios de copia, C-01..C-15, varios escritos a mano (sección 12) |
| Contrato de persistencia | Hay que fijar una regla de lectura de texto (mayúsculas, espacios, duplicados, desconocidos): ensancharla o estrecharla cambia qué documentos cargan (lección de I-48 G4A.1) | Ordinal entero, como `Dimensions` en el mismo DTO y los ordinales de `SafetySide` | `SelectivePalletDesignDocument.cs:106`; I-46 |
| Coherencia con el campo vecino | Distinta a `int? Dimensions` | Igual a `int? Dimensions` | `SelectivePalletDesignDocument.cs:106`, `DynamicRackSystemDocument.cs:62` |
| Valores desconocidos o futuros | El dominio debe guardar la lista **cruda** para no perderlos | Cualquier `int` (Int32) sobrevive solo, bit de signo incluido: convertir `int` a enum no enmascara | sección 5 |
| Oráculo de pruebas | combinaciones × orden × mayúsculas | tabla de verdad de 8 combinaciones más valores con bits desconocidos | sección 13 |
| Legibilidad del JSON | **Mejor**: `"frontal"` se explica solo | Opaco (`5`), como ya lo es `Dimensions` | — |
| Reutiliza un vocabulario congelado | **Sí**, el del sobre | No: introduce ordinales nuevos que deben congelarse | — |

**Recomendación: B** (aprobada por el Coordinador). Las dos ventajas de A —legibilidad y vocabulario
compartido— son de presentación. Las de B evitan **dos clases de defecto silencioso** que el árbol castiga
hoy: hermanas divergentes por orden o mayúsculas, que la autoridad multivista del Selectivo trata como
corrupción y aborta, y alias de listas mutables entre sitios de copia. Además, cualquier `int` sobrevive sin
código adicional.

> **Cambio respecto a G1.** El Discovery sugirió como forma una lista de tokens del sobre (§9). Esta Proposal
> la **retira** con la evidencia de la tabla: la comparación en orden y Ordinal de
> `SelectiveAuthoredAuthority` (§7.4 del Discovery) y la semántica por referencia en los sitios de copia.

## 3. Contrato de datos (B)

### P-01 — Tipo de dominio

`src/RackCad.Domain/Systems/Shared/DimensionViewVisibility.cs` (nuevo), junto a `DimensionDetail`:

```csharp
[Flags]
public enum DimensionViewVisibility
{
    None = 0,
    Frontal = 1,
    Lateral = 2,
    Planta = 4
}
```

- Los valores `1`, `2` y `4` son **contrato de persistencia**: nunca se renumeran.
- **Sin miembro `All`**, a propósito. Legacy es `null`, no «todas»; un `All` con nombre invita a escribir
  «todas» donde se quería legacy, y su valor cambiaría el día que se añadiera un tipo. La máscara de bits
  conocidos de la regla (P-11) es **privada** a la regla y nunca se usa como valor por defecto ni persistido.

### P-02 — Propiedad en los cuatro tipos de dominio

`public DimensionViewVisibility? DimensionViews { get; set; }`, con valor por defecto `null`, en
`SelectivePalletDesign`, `SelectiveRackSystem`, `DynamicRackDesign` y `DynamicRackSystem`. Push Back la
recibe a través de su estructura dinámica, y su lado B no la tiene propia (`CD-02`). **Reparto por gate**: en
los sistemas resueltos en G4; en los diseños en G5 (sección 14.3).

### P-03 — Formato de cable

En `SelectivePalletDesignDocument` y en `DynamicRackSystemDocument` (Push Back la hereda en `Structure`):

```csharp
[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
public int? DimensionViews { get; set; }
```

- Nombre JSON `DimensionViews`, en PascalCase como sus vecinos.
- Sin política: el campo **no aparece**; el JSON es **byte-idéntico** al de hoy. Precedente:
  `FirstLevelDatum` (`DynamicRackSystemDocument.cs:30-31`).
- Ejemplos: `"DimensionViews": 5` = Frontal + Planta; `0` = ninguna; `7` = las tres, **explícito**.
- `From` (dominio → DTO): `(int?)design.DimensionViews`, el **`int` exacto**, sin enmascarar.
- `ToDomain`/`ToDesign` (DTO → dominio): `null` → `null`; **cualquier otro `int` (Int32)** →
  `(DimensionViewVisibility)value`, **conversión cruda, sin máscara ni normalización**. No hay ningún caso
  que convierta un valor presente en `null`.
- Sitios exactos: `SelectivePalletDesignDocument.From` (`:268`) y `ToDomain` (`:401`);
  `DynamicRackSystemDocument.From(system)` (`:112`), `From(design)` (`:161`), `ToDesign` (`:215`) y
  `ToDomain` (`:309`).
- **Ninguna `SchemaVersion` cambia.** Precedente: `SelectivePalletDesignDocument` sigue en 1.0 y
  `PushBackDesignDocument` en 1.0 tras varios campos aditivos; `DynamicRackSystemDocument` no tiene
  versión propia.
- **Tipo y rango JSON** (V1.2): el campo es un `int` (Int32). Un valor JSON **no entero** o **fuera de
  Int32** conserva la **deserialización fallida** de siempre, igual que hoy con `int? Dimensions`. I-50 **no
  amplía** la tolerancia.

### P-04 — Semántica de nulo y legacy (`CD-06`, `CD-07`)

| Valor de `DimensionViews` | `Dimensions = None` | `Dimensions` ≠ `None` |
|---|---|---|
| `null` (ausente) | ninguna cota | **legacy exacto**: las tres vistas con `Dimensions`, como hoy |
| `0` | ninguna cota | ninguna cota |
| cualquier otro `int` (Int32) presente (con o sin bits desconocidos, positivo o negativo) | ninguna cota | solo las vistas cuyo bit conocido (`1`, `2`, `4`) está activo, con `Dimensions`; el valor **se conserva exacto** (sección 5) |

«Todas las vistas históricamente elegibles» son exactamente las tres vistas de los tres sistemas que hoy
emiten cotas (Discovery §3). Un rack legacy **nunca** se reescribe con `7`.

## 4. Regla única: `EffectiveDetail`

### P-05 — Firma y semántica

`src/RackCad.Application/Systems/Shared/DimensionViewPolicy.cs` (nuevo), puro, con `DimensionViewKind`:

```csharp
public enum DimensionViewKind { Frontal, Lateral, Planta }

public static class DimensionViewPolicy
{
    public static DimensionDetail EffectiveDetail(
        DimensionDetail detail, DimensionViewVisibility? policy, DimensionViewKind viewKind)
    {
        var bit = BitOf(viewKind);                   // tipo no definido => ArgumentOutOfRangeException
        if (detail == DimensionDetail.None) return DimensionDetail.None;   // CD-06
        if (policy == null) return detail;                                   // CD-07: legacy exacto
        return (policy.Value & bit) != 0 ? detail : DimensionDetail.None;    // CD-05: solo el bit conocido
    }
}
```

- El tipo de vista se valida **antes** que nada, para que un error de programación salga aunque el nivel sea
  `None`.
- `DimensionViewKind` es un enum **distinto** de los flags: así nadie puede pasar `Frontal | Lateral` como
  «un tipo».
- La regla consulta **solo** el bit del tipo pedido; los demás bits del valor, conocidos o no, no intervienen.
- **Consumidores únicos**: `SelectiveDimensions` y `DynamicViewDecorations`, además de las pruebas. La UI no
  calcula detalle efectivo, y el Plugin no conoce el campo.

### P-06 — Asignación de tipos por emisor

| Emisor | Tipo | Nota |
|---|---|---|
| `SelectiveDimensions.AddFrontal`, `FrontalBottomReach` | `Frontal` | todas las frontales por fondo (`FondoSystemView` copia el campo, C-05) |
| `SelectiveDimensions.AddLateralCorte` | `Lateral` | cada corte |
| `SelectiveDimensions.AddPlanta` | `Planta` | |
| `DynamicViewDecorations.AppendFrontal` | `Frontal` | **`end` no participa** (`CD-02`): salida, entrada y los cuatro cortes frontales de Push Back |
| `DynamicViewDecorations.AppendLateral` | `Lateral` | lateral entero, cada corte y cada lado del compuesto (D4) |
| `DynamicViewDecorations.AppendPlanta` | `Planta` | un sentido y compuesto |

El tipo sale de **qué método emite**, no de un argumento nuevo. Las firmas públicas de los builders y de los
servicios de dibujo **no cambian**.

## 5. Valores presentes, desconocidos y futuros

### P-07 — Contrato de valores presentes

- **Ausencia o `null`** ⇒ `null` ⇒ legacy exacto. Es el **único** camino a legacy.
- **Cualquier `int` (Int32) presente se conserva EXACTAMENTE**: `0`, `7`, `13`, `-1`, `-8` o cualquier otro.
  - DTO → dominio: `(DimensionViewVisibility)value`, **sin máscara ni normalización**.
  - Dominio → DTO: `(int?)design.DimensionViews`, el **mismo** `int`.
  - Ningún valor presente se convierte en ausencia, y ninguno se reescribe distinto.
  - Un JSON no entero o fuera de Int32 no es un «`int` presente»: falla la lectura del diseño como hoy (P-03).
- **`EffectiveDetail` observa solo los bits conocidos** `Frontal = 1`, `Lateral = 2` y `Planta = 4`.
  Cualquier otro bit —**incluido el de signo**— no decide y **sobrevive** la ida y vuelta.
- **Ejemplos normativos**, cada uno con prueba (T-01, T-11, T-12):

| Valor presente | Se conserva como | Frontal | Lateral | Planta |
|---|---|---|---|---|
| `13` (`8 \| 4 \| 1`) | `13` | ON | OFF | ON |
| `-1` (todos los bits a uno) | `-1` | ON | ON | ON |
| `-8` (bits `1`, `2` y `4` a cero, el resto a uno) | `-8` | OFF | OFF | OFF |

- **UI**: muestra solo los bits conocidos y **conserva** los demás al escribir (P-11). Por ejemplo, encender
  Frontal sobre un `-8` cargado produce `-7`.
- **Coherencia con la doctrina de I-47** (ADR-0034): un valor **presente** nunca se lee como **ausente**.
- **Tipos de vista futuros**: el día que exista un cuarto tipo con bit propio, **su** iniciativa define su
  regla legacy. Las políticas explícitas escritas por I-50 tienen ese bit a `0`, y I-50 no decide por
  adelantado cómo leerlo.
- **Builds anteriores**: dibujan legacy porque no conocen la política, así que **las cotas reaparecen** en lo
  que ese build redibuje. Al re-guardar, Dinámico y Push Back anteriores —y un Selectivo anterior a I-47—
  **pierden** el campo y el rack vuelve a legacy. Un Selectivo post-I-47 y pre-I-50 lo **conserva** como dato
  desconocido en `ExtensionData` (`SelectivePalletDesignDocument.cs:141-142`; `WithDesign`, `:322`) aunque
  lo ignore al dibujar. Limitación declarada; no se sube el major, porque bloquear la apertura de racks por
  una preferencia visual sería desproporcionado.

## 6. Alcance de las etiquetas

### P-08 — Regla

> **Dentro de una vista, todo cálculo de alcance usa el mismo detalle efectivo que decide sus cotas.**
> Una vista con las cotas apagadas coloca sus etiquetas **exactamente** como hoy las coloca
> `Dimensions = None`.

| Emisor | Lecturas del nivel que pasan a usar el detalle efectivo |
|---|---|
| `SelectiveDimensions.AddFrontal` | `:55` |
| `SelectiveDimensions.FrontalBottomReach` | `:34-40` (número de frente, `SelectiveFrontalBuilder.cs:324-325`) |
| `SelectiveDimensions.AddLateralCorte` | `:135` |
| `SelectiveDimensions.AddPlanta` | `:201` |
| `DynamicViewDecorations.AppendFrontal` | `AppendFrontalDimensions` (`:267-315`), `FrontalLeftReach` y `BottomReach` (`:55-56`) |
| `DynamicViewDecorations.AppendPlanta` | cotas (`:110-130`) y `leftReach` (`:133`) |
| `DynamicViewDecorations.AppendLateral` | cotas (`:199-229`) y `FrontalLeftReach` (`:232`) |

Los auxiliares privados `BottomReach` y `FrontalLeftReach` (`:320-340`) pasan a recibir el
`DimensionDetail` ya resuelto en vez de leer `system.Dimensions`. Ninguna otra etiqueta depende del nivel:
`SelectiveAnnotations` y `PushBackSideAnnotations` no lo leen.

**Invariante legacy**: con `DimensionViews = null`, cada alcance es idéntico al de hoy (T-03..T-05).

## 7. Por sistema

### 7.1 Selectivo

- Frontal: **todas** las frontales por fondo siguen `Frontal`; `SelectiveDepthLayout.FondoSystemView` copia
  el campo (`:83-84`, C-05).
- Lateral: cada corte sigue `Lateral`.
- Planta: un bloque, `Planta`.
- Guardado: `LinkedPropertyReconciler.Reconcile` → `WithDesign` → `From(design)` ya transporta cualquier
  campo que `From` mapee (`LinkedPropertyReconciler.cs:146`; `SelectivePalletDesignDocument.cs:316-330`).
  **No se tocan `WithDesign` ni el reconciliador.**

### 7.2 Dinámico

- Frontal: salida (`Section` 0) y entrada (`Section` 1) siguen `Frontal`; `AppendFrontal` no consulta `end`
  para decidir visibilidad.
- Lateral: entero y cada corte siguen `Lateral`.
- Planta: `Planta`.

### 7.3 Push Back

- **Frontal de un sentido**: EntradaSalida (pasada `Exit`) y Posterior (pasada `Entrance`) pasan por D1 y
  siguen `Frontal`.
- **Frontal compuesto**: la primera pasada aporta «postes, placas, cotas» a los **cuatro** cortes
  (`PushBackCompositeFrontal.cs:46-48`, `:66-71`), así que los cuatro siguen `Frontal`.
- **Lateral compuesto**: la estructura desnuda sigue sin cotas (`WithoutDecorations`, `:234-242`). Cada lado
  emite con **su** sub-estructura (`SideDecorations`, `:196-231`), que recibe el campo por dos copias
  obligatorias: `PushBackCompositeStructure.CopySharedStructuralIntent` (`:831-859`) y
  `DynamicRackSystemResolver` (`:244-245`).
  - Precedente de por qué importa: el comentario de I-42 en `:841-845` documenta que omitir
    `FirstLevelDatum` en esa misma copia **devolvía un lado a la semántica histórica** sin avisar.
- **Planta compuesta**: `AppendPlanta` sobre la estructura compuesta, `Planta`.
- **Clones**: `PushBackMirror.Structure` (`:139-162`) es el único clonador y cubre `Clone`, `PushBackRuns.Clone`,
  `WithoutDecorations` y `WithoutRackName`.
- **H1 fuera de alcance** (`CD-09`): Push Back sigue **sin** control de estilo. I-50 añade solo las tres
  casillas junto a `DimensionsBox` y **no** cambia que cada recálculo deje `DimensionStyle = null`, que queda
  caracterizado (T-19).

### 7.4 Cantilever, Cama, Larguero y Cabecera

Sin cambio y sin controles: no dibujan cotas (Discovery §3.2; contrato, sección 5).

## 8. Guardar, reabrir, RACKEDITAR y Actualizar

### P-09 — Guardar y reabrir

- La política viaja **dentro del `Design`** de cada sobre, idéntica en todas las vistas del rack. No cambian
  el sobre, `RackBlockData`, el Xrecord ni su ubicación en la definición.
- Al reabrir, `RackEmbedStore.Deserialize` sigue igual de tolerante, y el diseño se lee con P-03: el valor
  presente vuelve **exacto**.
- Biblioteca: `RackProjectStore` guarda el diseño (Dinámico y Push Back a través de
  `DynamicRackSystemDocument`). `SelectiveLibraryExport` reconstruye el documento con `From(effective, …)`
  (`SelectiveLibraryExport.cs:88`), así que lo incluye si `From` lo mapea.

### P-10 — RACKEDITAR y Actualizar

- **Carga**: la ventana recibe `DimensionViews` del diseño efectivo. Con `null`, las tres casillas se
  muestran **activas**, que es la conducta real de legacy. Con un valor presente, cada casilla refleja su bit
  conocido.
- **Escritura**: la ventana entrega un único valor, calculado con la regla pura de P-11.
- **Actualizar**: el diseño se serializa **una sola vez** y todas las vistas reciben el mismo JSON
  (`RackSelectivoCommands.cs:144-153`); Dinámico y Push Back redibujan cada definición con el mismo diseño.
  Las hermanas siguen iguales.
- **Cero cambios en el Plugin**: `EditSelective`, `EditDynamic` y `EditPushBack` no leen ni escriben el
  campo; les llega dentro del diseño. La conformidad se verifica con la evidencia CE-01 (sección 13.4).

### P-11 — «Sin tocar» no materializa (`CD-07`)

Regla pura en `DimensionViewPolicy`, para que las tres ventanas no repitan aritmética de escritura (AGENTS,
convención 2):

```text
Conocidos = Frontal | Lateral | Planta        (= 7; ~Conocidos incluye todos los bits desconocidos y el de signo)

FromEditor(loaded, touched, frontal, lateral, planta):
  si !touched  -> loaded                                     (null sigue null; un int presente vuelve EXACTO)
  si touched   -> ((loaded ?? None) & ~Conocidos) | casillas (nunca null; conserva bits desconocidos y de signo)
```

- **«Tocar»** = que el usuario cambie **una de las tres casillas de vista**. Cambiar solo el nivel o el
  estilo **no** toca la visibilidad, y un rack legacy sigue siendo legacy.
- **Cargar, recargar (`LoadDesign`, `RestoreFrom`, `LoadFromModel`) y los recálculos automáticos NO tocan**,
  aunque cambien programáticamente el estado de una casilla. La implementación deberá impedir que esas
  operaciones marquen «tocado»; **el mecanismo no se fija aquí** (lo decide G3). Las pruebas T-17..T-19
  fuerzan una transición real de casilla durante la carga para demostrarlo.
- El primer recálculo tras cargar **debe** reproducir exactamente el valor cargado. Push Back recalcula
  dentro de `LoadFromModel`, así que el estado cargado se fija antes de ese recálculo (T-19).
- Volver a marcar las tres casillas tras tocarlas produce `7` explícito, **no** `null`: una elección
  explícita no se convierte en legacy en silencio.
- Ejemplos: `-8` sin tocar se guarda `-8`; `-8` con Frontal encendido da `-7`; `13` con Planta apagada da `9`.
- Con nivel «Ninguna», las casillas pueden mostrarse deshabilitadas, pero ese estado **nunca** modifica el
  valor guardado.

## 9. Vista enlazada nueva

### P-12

La vista nueva lleva el **mismo JSON de diseño** que sus hermanas: en el Selectivo, el authored
reconciliado (`RackSelectivoCommands.cs:251-258`); en Dinámico y Push Back, el diseño del editor. Por eso
sigue la política de su tipo sin trabajo adicional. El sobre se compone desde el de la vista **elegida**
(`:257-258`; `RackDinamicoCommands.cs:280-281`; `RackPushBackCommands.cs:330`), pero ese sobre **no** lleva
política (`CD-08`), así que **no hay escritura cruzada**. Cubre el requisito «nueva vista creada después
del cambio» (contrato, sección 8), que además entra en Owner Validation (sección 13.6).

## 10. RACKDUPLICAR y RACKLAYOUT

### P-13

- `RACKDUPLICAR` clona las entidades de la definición elegida, con las cotas **ya dibujadas**
  (`RackDuplicarCommands.cs:190-197`), y re-estampa el diseño:
  - Selectivo: `SelectiveAuthoredRestamp.Restamp` hace ida y vuelta **por el documento**
    (`RestampResult.cs:67-74`), así que un campo declarado sobrevive con su `int` exacto.
  - Dinámico y Push Back: `RestampDesign` devuelve el JSON intacto (`DynamicKindHandler.cs:49`,
    `PushBackKindHandler.cs:75`).
- La copia es independiente (GUID nuevo) y conserva la política en sus redibujos futuros.
- **I-50 no toca** `RackDuplicarCommands.cs`, `RackEnvelopeRestamp.cs` ni `RackCloner.cs`. Eso responde la
  decisión AM-4 de I-51: la política vive en el diseño, no en la referencia ni en el sobre.
- **`RACKLAYOUT`: I-50 NO lo modifica** (confirmación del Coordinador). Las copias enlazadas referencian la
  definición y las independientes la clonan, y en ambos casos la política se conserva.
  - La huella de la rejilla sale de `GeometricExtents` de la planta semilla (`RackLayoutCommands.cs:189-197`),
    así que ocultar las cotas de la planta **encoge** las rejillas futuras.
  - Esa variación es **consecuencia explícita** de ADR-0035 y **punto de Owner Validation**. **No** se amplía
    el alcance para corregirla.

## 11. Sin cambios de geometría ni de BOM

### P-14

- Solo cambian **qué instancias `Dimension` se emiten** por vista y **dónde caen las etiquetas** de una
  vista apagada.
- Toda instancia con otro rol queda idéntica, para cualquier política: `HeaderInstanceGrouper` deja cotas y
  etiquetas sueltas y nombra los grupos solo a partir de piezas de bloque (`HeaderInstanceGrouper.cs:35-43`,
  `:95-99`).
- BOM: el del Selectivo cuenta con `None` forzado (`SelectiveBomBuilder.cs:60-68`, `:508`), y ni
  `SystemBomBuilder` ni `PushBackBomBuilder` ejecutan decoraciones. **El BOM es idéntico** para cualquier
  política (T-16).
- GUID, `View`, `Section`, nombre de bloque, sobre, capa `RACKCAD_COTAS` y materializador: sin cambio.

## 12. Lista de sitios de copia (15)

Todo sitio que hoy propaga `Dimensions` debe propagar `DimensionViews` en el **mismo** punto y con el
**mismo** `int`: sin máscara, sin normalización y sin convertir un valor presente en `null`. Omitir un sitio
no falla: vuelve a legacy en silencio (sección 7.3).

**Cobertura** (V1.2): C-05 se prueba en T-06 (G4); C-02..C-04, C-06, C-08..C-11, C-13 y C-14 en T-13 (G5);
C-15 en T-08 (G6); **C-01, C-07 y C-12 en T-17, T-18 y T-19** (G3). T-13 **no** cubre todos los sitios.

| # | Archivo | Símbolo | Línea actual | Dirección | Gate | Prueba |
|---|---|---|---|---|---|---|
| C-01 | `RackSelectiveWindow.xaml.cs` | `BuildDesign` / `LoadDesign` | `:2338` / `:2686` | UI ⇄ entradas | G3 | T-17 |
| C-02 | `SelectiveDesignInputs.cs` | propiedad | `:43-44` | entradas | G5 | T-13 |
| C-03 | `SelectiveEditorState.cs` | construcción del diseño | `:1317-1318` | entradas → diseño | G5 | T-13 |
| C-04 | `SelectiveGeometryResolver.cs` | resolución | `:66-67` | diseño → sistema | G5 | T-13 |
| C-05 | `SelectiveDepthLayout.cs` | `FondoSystemView` | `:83-84` | sistema → vista por fondo | **G4** | T-06 |
| C-06 | `SelectivePalletDesignDocument.cs` | `From` / `ToDomain` | `:268` / `:401` | diseño ⇄ DTO | G5 | T-13 (y T-11) |
| C-07 | `RackDynamicSystemWindow.xaml.cs` | `ReadAnnotationOptions` / `RestoreFrom` | `:292-298` / `:2772-2775` | UI ⇄ opciones | G3 | T-18 |
| C-08 | `DynamicAnnotationOptions.cs` | propiedad | `:16-17` | opciones | G5 | T-13 |
| C-09 | `DynamicEditorDesignAssembler.cs` | `BuildDesign` (después de `Snapshot`) | `:174-175` | opciones → diseño | G5 | T-13 |
| C-10 | `DynamicRackSystemResolver.cs` | diseño → sistema / sistema → diseño | `:244-245` / `:357-358` | diseño ⇄ sistema | G5 | T-13 |
| C-11 | `DynamicRackSystemDocument.cs` | `From(system)`, `From(design)`, `ToDesign`, `ToDomain` | `:112`, `:161`, `:215`, `:309` | DTO | G5 | T-13 (y T-12) |
| C-12 | `RackPushBackSystemWindow.xaml.cs` | `LoadFromModel` / `ReadInputs` | `:378` / `:810-817` | UI ⇄ opciones | G3 | T-19 |
| C-13 | `PushBackEditorState.Load.cs` | opciones de anotación | `:206-208` | diseño → opciones | G5 | T-13 |
| C-14 | `PushBackMirror.cs` | `Structure` | `:158-160` | clon | G5 | T-13 |
| C-15 | `PushBackCompositeStructure.cs` | `CopySharedStructuralIntent` | `:856-858` | compartido → lados y compuesto | G6 | T-08 |

**Conversiones indirectas sin código propio**: `RackProject.ForDynamic(system)` (`RackProject.cs:103-107`,
`From(system).ToDesign()`) y `RackProject.ForDynamic(design)` (`:109-113`, `From(design).ToDomain()`, usada
por `BuildDynamicPayload` en `RackDinamicoCommands.cs:379`) dependen de C-11. No son sitios de copia nuevos;
T-12 las nombra y las verifica.

**No cambian, a propósito**: `PushBackEditorDesignAssembler.cs:378` (solo reenvía las opciones),
`SelectivePalletDesignDocument.WithDesign`, `LinkedPropertyReconciler`, `SelectiveEffectiveDesignResolver`
(usa `ToDomain`), `SelectiveAuthoredAuthority`, `SelectiveLibraryExport`, `RackEmbedDocument`,
`RackEmbedComposer`, `RackBlockData`, `RackEnvelopeRestamp`, los kind handlers, los builders de BOM,
`RackLayoutCommands`, `RackProject` y todo `src/RackCad.Plugin`.

## 13. Pruebas RED previstas

### 13.1 Estrategia

1. **Caracterización primero, en un commit TEST-ONLY propio** (MIN-7), sobre producción **intacta**, con CI de
   `push` sobre **ese SHA exacto**, **antes** de cualquier cambio de producción: T-03..T-05 fijan la firma de
   hoy con cotas activas (Selectivo y Push Back, más la ampliación del Dinámico). Esas pruebas tienen que
   seguir verdes con `DimensionViews = null` en todos los gates siguientes: son la prueba del legacy exacto.
2. **RED por capa, con fallo de comportamiento**, nunca un fallo de compilación, y **dentro del gate que la
   cierra** (sección 14.3). Primero se introduce la superficie **inerte** de la capa y después se escriben las
   pruebas que la hacen fallar:
   - **G4** — la regla que aún devuelve `detail`: falla T-01; la regla de editor sin conservar bits: falla
     T-02; los emisores sin cablear, con la política asignada a sistemas **ya resueltos**: fallan T-06, T-07 y
     T-09 (T-06 incluye C-05). T-10 es guarda.
   - **G5** — el DTO sin mapear: fallan T-11 y T-12; las copias sin propagar: falla T-13; variantes extremo a
     extremo de T-06 y T-07 por los resolvers productivos. T-14 y T-15 son guardas.
   - **G6** — la copia compartida sin propagar: falla T-08. T-16 es guarda.
   - **G3** — las ventanas sin cablear: fallan T-17..T-19 y T-21. T-20 es guarda.
3. **Higiene de evidencia** (MIN-7):
   - **solo se commitea en verde**: ningún RED se publica como tip roto;
   - toda evidencia RED registra el **conteo de pruebas seleccionadas, que debe ser mayor que 0** (AGENTS:
     «0 pruebas seleccionadas = FALLO»);
   - un RED se abre y se cierra **dentro de la misma sesión**: nunca cruza un relevo ni un cierre de sesión
     (WORKFLOW §3 y §4.3, relevo con árbol limpio);
   - la evidencia RED (conteo de fallos y de seleccionadas) va en el cuerpo del commit verde que la cierra
     (precedente I-48).

### 13.2 Suite Core (`tests/RackCad.Tests`)

| ID | Prueba | Clase | Gate |
|---|---|---|---|
| T-01 | `DimensionViewPolicy.EffectiveDetail`: tabla de verdad con 4 niveles × {`null`, 0..7, 8, `13`, `-1`, `-8`} × 3 tipos. Casos **explícitos**: `13` ⇒ Frontal y Planta con detalle, Lateral `None`; `-1` ⇒ las tres con detalle; `-8` ⇒ las tres `None`. `None` gana; `null` = detalle; un tipo no definido lanza | RED | G4 |
| T-02 | `DimensionViewPolicy.FromEditor`: sin tocar devuelve **exacto** lo cargado (`null`, `7`, `13`, `-1`, `-8`); tocado conserva bits desconocidos y de signo (`-8` + Frontal ⇒ `-7`; `13` sin Planta ⇒ `9`); legacy tocado da solo las casillas | RED | G4 |
| T-03 | Caracterización Selectivo: firma frontal (1 y 2 fondos), cada corte y planta con Minimal/Standard/Detailed y numeración y nombre activos; verde hoy y con `null` | Caracterización (commit test-only) | G4 |
| T-04 | Caracterización Dinámico: `DynamicNullOverrideGoldenTests` intacta más planta y etiquetas; verde hoy y con `null` | Caracterización (commit test-only) | G4 |
| T-05 | Caracterización Push Back: un sentido y compuesto A/B, 4 cortes frontales, lateral entero y por poste, planta, con cotas activas **y numeración y nombre activos** (MIN-5), para fijar también el alcance de las etiquetas; verde hoy y con `null` | Caracterización (commit test-only) | G4 |
| T-06 | Selectivo por vista: {F}, {L}, {P}, {F, P}, 0. Todas las frontales por fondo siguen F **a través de `FondoSystemView` (C-05)**, con un centinela como `-8` que demuestra la propagación exacta; cada corte sigue L. **G4**: sobre sistemas ya resueltos con la política asignada. **G5**: variante extremo a extremo, diseño → `SelectiveGeometryResolver` → emisor | RED | G4, repetida en G5 |
| T-07 | Dinámico por vista: salida y entrada siguen F; lateral entero y cortes siguen L; planta sigue P. **G4**: sobre sistemas ya resueltos. **G5**: variante extremo a extremo por `DynamicRackSystemResolver` | RED | G4, repetida en G5 |
| T-08 | Push Back por vista: EntradaSalida y Posterior de un sentido; 4 cortes frontales compuestos; lateral compuesto en ambos lados; planta compuesta; propagación exacta de C-15 a lados y compuesta con centinela; **etiquetas de Push Back** (un sentido y compuesto) con una vista apagada: quedan donde las pone `None` | RED | G6 |
| T-09 | Etiquetas (Selectivo y Dinámico): con una vista apagada, sus etiquetas quedan donde las pone `None`; las demás vistas no cambian. Las etiquetas de Push Back se cierran en T-08 | RED | G4 |
| T-10 | `None` gana: `Dimensions = None` con cualquier política da cero instancias `Dimension` en todo | Guarda | G4 |
| T-11 | DTO Selectivo: `null` no se escribe y el JSON es byte-idéntico al de hoy; `0`, `1`, `5`, `7`, `13`, `-1` y `-8` hacen ida y vuelta **exactos**, en dominio y en el número escrito; ningún valor presente vuelve como `null`; `WithDesign` y la exportación a biblioteca lo conservan | RED | G5 |
| T-12 | DTO Dinámico y Push Back: los cuatro mapeos conservan el `int` exacto (incluidos `13`, `-1` y `-8`); ida y vuelta por `RackProjectStore` (incluido `Structure`); `null` no se escribe. **Nombra y verifica** las conversiones indirectas `RackProject.ForDynamic(system)` y `RackProject.ForDynamic(design)`, que dependen de C-11 | RED | G5 |
| T-13 | Sitios de copia **C-02, C-03, C-04, C-06, C-08, C-09, C-10, C-11, C-13 y C-14**: cada uno propaga el **`int` exacto**, con un centinela con bits desconocidos y de signo (p. ej. `-8`) para detectar cualquier máscara; incluye `PushBackRuns.Clone`. **No** cubre C-01, C-05, C-07, C-12 ni C-15 | RED | G5 |
| T-14 | Autoridad multivista del Selectivo: documentos que difieren solo en `DimensionViews` ⇒ `Divergent`; iguales ⇒ `Single` | Guarda | G5 |
| T-15 | `RACKDUPLICAR`: `SelectiveAuthoredRestamp` conserva el `int` exacto (`13`, `-1`, `-8`); Dinámico y Push Back devuelven el JSON intacto | Guarda | G5 |
| T-16 | Sin cambio de geometría ni BOM: para cada política, las instancias que no son cotas son idénticas, y el BOM de los tres sistemas también | Guarda | G6 |

### 13.3 Suite UI (`tests/RackCad.UI.Tests`)

| ID | Prueba | Clase | Gate |
|---|---|---|---|
| T-17 | Selectivo (C-01): legacy carga con tres casillas activas y guardar sin tocar deja `null`. **Durante `LoadDesign` se fuerza programáticamente una transición real de al menos una casilla** (p. ej. dejarla en OFF antes de cargar un legacy que la muestra ON), y guardar sin tocar **sigue** dejando `DimensionViews = null` (MIN-4). Además: apagar Lateral da F\|P; cambiar solo el nivel deja `null`; una recarga explícita se refleja; `-8` carga con las tres apagadas y se guarda sin tocar como `-8`; tocar conserva bits desconocidos y de signo (`-8` + Frontal ⇒ `-7`) | RED | G3 |
| T-18 | Dinámico (C-07): lo mismo, con la transición real forzada durante `RestoreFrom`, incluido `RestoreFrom` repetido | RED | G3 |
| T-19 | Push Back (C-12): lo mismo, con la transición real forzada durante `LoadFromModel` y su recálculo inmediato; y H1 caracterizado: `DimensionStyle` sigue saliendo `null` | RED y caracterización | G3 |
| T-20 | Censos de `x:Name` (`DynamicShellMigrationTests`, `SelectiveShellMigrationTests`, `PushBackModuleEditorCharacterizationTests`): se **actualizan**, no se relajan | Guarda | G3 |
| T-21 | Firmas de dibujo de I-24 (`DynamicEditorWindowTests.FullDrawingSignature`, `SelectiveEditorWindowTests.DrawingSignature`) con variantes de política | RED | G3 |

### 13.4 Plugin: sin suite automatizada (ADR-0003)

**T-22 sigue RETIRADA** (MATERIAL-02 de V1). No hay guarda de texto sobre `src/RackCad.Plugin` y no se
sustituye por otra. La conformidad «Plugin sin cambios por I-50» se verifica **operacionalmente**:

**CE-01 — evidencia de conformidad por diff.** Se ejecuta en **cada revisión y en cada Candidato**, y su
salida se registra en el cuerpo del commit o en la evidencia del Candidato. **No** es una prueba de la suite
automatizada ni vive en `tests/`.

```bash
base=$(git merge-base HEAD origin/main)
git diff --name-only $base HEAD -- src/RackCad.Plugin
```

Equivalente en PowerShell:

```powershell
$base = git merge-base HEAD origin/main
git diff --name-only $base HEAD -- src/RackCad.Plugin
```

**Debe quedar vacío** para el diff propio de I-50. Se mantienen, además, el **build Debug del Plugin**
obligatorio (AGENTS.md, «Pruebas — definición de terminado», punto 3), el **CI** obligatorio sobre el SHA
exacto que corresponda y las **pruebas conductuales** existentes más T-01..T-21.

### 13.5 Colocación de las pruebas

Las pruebas nuevas van en **archivos nuevos** donde sea posible, para no chocar con suites ajenas. En
particular, **T-15 no se añade a `SelectiveDuplicationFailClosedTests.cs`**, que I-51 modifica en su rama
(sección 15).

### 13.6 Matriz de trazabilidad del contrato de aceptación (MIN-8)

**A. Requisitos del contrato (sección 8)** → cobertura automatizada y Owner Validation (OV).

| Requisito del contrato | Automatizado (T-xx) | OV AutoCAD 2025 | Por qué OV |
|---|---|---|---|
| default legacy | T-03, T-04, T-05 (con `null`); T-11, T-12 (JSON byte-idéntico) | Sí — smoke legacy | el dibujo real de un DWG existente atraviesa el Plugin |
| Front ON / Side OFF / Plan OFF | T-06, T-07, T-08 | Sí | la materialización de cotas es del Plugin |
| Front OFF / Side ON | T-06, T-07, T-08 | Sí | ídem |
| combinaciones relevantes por sistema | T-01 (tabla de verdad), T-06..T-08 | Sí, por sistema | ídem |
| insert | T-17..T-19 (petición del editor con sistema y diseño), T-21 | Sí — crear/insertar | la inserción y el jig son del Plugin |
| update | T-21 (firma tras Actualizar desde el editor) | Sí — RACKEDITAR + Actualizar | el redibujo en sitio es del Plugin |
| RACKEDITAR | T-17..T-19 (carga, recarga y guardado sin tocar) | Sí | la selección y el despacho son del Plugin |
| save/reload | T-11, T-12 (ida y vuelta de DTO y stores) | Sí — save/reopen DWG | el Xrecord en el DWG solo existe en AutoCAD |
| linked sibling views | T-14 (autoridad multivista del Selectivo); una sola serialización del diseño | Sí — vistas hermanas | la búsqueda por GUID y el redibujo son del Plugin |
| nueva vista creada después del cambio | — (camino del Plugin, sin suite, ADR-0003) | **Sí, obligatoria** — vista enlazada nueva después del cambio | solo se ejerce en AutoCAD |
| cada sistema incluido | T-06 (Selectivo), T-07 (Dinámico), T-08 (Push Back) | Sí, por sistema | — |
| no cross-write entre vistas | T-06..T-08 (aislamiento por tipo); T-01 | Sí | — |
| no pérdida de geometría ni BOM | T-16 | Sí (visual) | la geometría dibujada solo se ve en AutoCAD |

**B. Owner Validation mínima** (AutoCAD 2025, por cada sistema acordado):

| # | Escenario OV | Requisito que cubre |
|---|---|---|
| OV-1 | Crear e insertar un rack | insert |
| OV-2 | Elegir Frontal / Lateral / Planta en combinaciones y verificar que solo las vistas elegidas muestran cotas | Front ON/Side OFF/Plan OFF; Front OFF/Side ON; combinaciones; no cross-write |
| OV-3 | `RACKEDITAR` + Actualizar | RACKEDITAR; update |
| OV-4 | Guardar, cerrar y reabrir el DWG | save/reload |
| OV-5 | Vistas hermanas del mismo rack tras Actualizar | linked sibling views |
| OV-6 | Insertar una vista enlazada **nueva** después del cambio | nueva vista creada después del cambio |
| OV-7 | Etiquetas y su alcance visibles con una vista apagada | P-08 |
| OV-8 | Push Back compuesto A/B: los cuatro cortes frontales, lateral y planta | Push Back compuesto |
| OV-9 | `RACKLAYOUT` con Planta sin cotas: variación de huella **aceptada**, no corregida | consecuencia de ADR-0035 |
| OV-10 | Smoke legacy: un DWG existente abre y se actualiza sin que desaparezca ninguna cota | default legacy |

## 14. Archivos previstos, UI y gates

### 14.1 Producción (no se toca en G2)

| Capa | Nuevos | Modificados |
|---|---|---|
| Domain | `Systems/Shared/DimensionViewVisibility.cs` | `SelectivePalletDesign.cs` (caliente), `SelectiveRackSystem.cs`, `DynamicRackDesign.cs`, `DynamicRackSystem.cs` |
| Application | `Systems/Shared/DimensionViewPolicy.cs` (con `DimensionViewKind`) | `SelectiveDimensions.cs`, `DynamicViewDecorations.cs`, `SelectiveGeometryResolver.cs`, `SelectiveDepthLayout.cs`, `SelectiveDesignInputs.cs`, `SelectiveEditorState.cs`, `DynamicRackSystemResolver.cs`, `DynamicAnnotationOptions.cs`, `DynamicEditorDesignAssembler.cs`, `PushBackMirror.cs`, `PushBackCompositeStructure.cs`, `PushBackEditorState.Load.cs`, `SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs` |
| UI | — | `RackSelectiveWindow.xaml/.cs`, `RackDynamicSystemWindow.xaml/.cs`, `RackPushBackSystemWindow.xaml/.cs` (los tres calientes) |
| Plugin | — | **ninguno** (verificado por CE-01) |

**Total: 2 nuevos y 24 modificados.** Pruebas: T-01..T-21 en la suite, más la evidencia CE-01 fuera de la
suite.

### 14.2 UI mínima (restricciones para G3)

- Una sola sección: la existente **«Cotas»** de cada editor recibe tres casillas, **Frontal**, **Lateral**
  y **Planta**. No hay ventana nueva, así que el censo de ventanas de I-39 no se mueve.
- Sin controles en Cantilever, Cama, Larguero ni Cabecera (contrato, sección 5).
- El texto explica que la elección es **por tipo de vista del rack**: todas las frontales, todos los cortes
  y todas las copias comparten. Se actualiza la leyenda del Selectivo «(las tres vistas)»
  (`RackSelectiveWindow.xaml:63`).
- MIN-3 **no es requisito de consenso**: un helper puro de **lectura** para la UI (mostrar las casillas a
  partir del valor) puede decidirse localmente en G3. No se crea una abstracción contractual nueva por ello.

### 14.3 Gates: contenido y cierre (MAT-A1)

**Orden APROBADO por el Coordinador, sin cambios: `G4 → G5 → G6 → G3 → G7 → G8`.** G4 —incluido su commit
test-only— se abre **solo** con la compuerta de código cumplida: consenso sobre la misma versión de esta
Proposal **y** ADR-0035 `aceptado`.

| Gate | Archivos / producto | C-xx | Pruebas / validación que deben cerrarlo |
|---|---|---|---|
| **G4** | **Paso 1, commit TEST-ONLY** sobre producción intacta, con CI de `push` sobre ese SHA exacto: caracterización T-03..T-05. **Paso 2**: `Domain/Systems/Shared/DimensionViewVisibility.cs` (nuevo); `Application/Systems/Shared/DimensionViewPolicy.cs` (nuevo, con `DimensionViewKind`, `EffectiveDetail` y `FromEditor`); propiedad `DimensionViews` **solo** en los sistemas resueltos `SelectiveRackSystem.cs` y `DynamicRackSystem.cs`; `SelectiveDimensions.cs`; `DynamicViewDecorations.cs`; `SelectiveDepthLayout.cs` (`FondoSystemView`) | **C-05**, adelantado expresamente porque es parte obligatoria del camino productivo frontal (`RackSelectivoCommands.cs:175`, `:494`; `ProjectVariableMutationExecutor.cs:237`; `SelectiveDimensionsTests.cs:42-48`). **Ningún otro** sitio de copia de G5 se adelanta | T-03, T-04, T-05 verdes en el commit test-only y verdes después con `null`; T-01, T-02, T-06, T-07, T-09 y T-10 **sobre sistemas ya resueltos** |
| **G5** | Propiedad `DimensionViews` en los diseños `SelectivePalletDesign.cs` y `DynamicRackDesign.cs`; DTO `SelectivePalletDesignDocument.cs` (`From`/`ToDomain`) y `DynamicRackSystemDocument.cs` (cuatro mapeos); resolvers y propagación general: `SelectiveGeometryResolver.cs`, `DynamicRackSystemResolver.cs`, `SelectiveDesignInputs.cs`, `SelectiveEditorState.cs`, `DynamicAnnotationOptions.cs`, `DynamicEditorDesignAssembler.cs`, `PushBackEditorState.Load.cs`, `PushBackMirror.cs` | **C-02, C-03, C-04, C-06, C-08, C-09, C-10, C-11, C-13, C-14** | T-11, T-12, T-13, T-14, T-15; variantes **extremo a extremo** de T-06 y T-07 por los resolvers productivos; todo lo de G4 sigue verde |
| **G6** | `PushBackCompositeStructure.cs` (`CopySharedStructuralIntent`); cobertura de Push Back compuesto A/B | **C-15** | T-08, T-16; todo lo anterior sigue verde |
| **G3** | XAML y code-behind de las tres ventanas: `RackSelectiveWindow.xaml/.cs`, `RackDynamicSystemWindow.xaml/.cs`, `RackPushBackSystemWindow.xaml/.cs`, tras el protocolo de coordinación con I-49 (contrato, sección 11) | **C-01, C-07, C-12** | T-17, T-18, T-19, T-20, T-21; todo lo anterior sigue verde |
| **G7** | **Candidato**: SHA exacto; sin producto nuevo | — | Suites según la política de I-45 (AGENTS: Core Full y UI Full locales sobre el Candidato); build Debug de UI; **build Debug del Plugin**; CI de `push` sobre el SHA exacto; **CE-01** (diff propio de I-50 en `src/RackCad.Plugin` vacío); **Owner Validation** AutoCAD 2025, OV-1..OV-10 (sección 13.6) |
| **G8** | Documentación de cierre (HANDOFF, ROADMAP en el momento 3, contrato y guías si aplica), integración `--no-ff`, CI posterior al merge, comprobación de cobertura y limpieza | — | Compuertas de WORKFLOW §4.5 (pasos 5, 6 y 7) y §5; limpieza segura (§3) solo después de pasar las posteriores al merge |

## 15. Coordinación y riesgos

- **I-49**: antes de editar `RackSelectiveWindow.xaml/.cs` o `SelectivePalletDesignDocument.cs`, aplicar el
  protocolo del contrato (sección 11): fetch; `git diff --name-only
  origin/main...origin/architecture/motor-expresiones-parametricas`; si I-49 los modifica materialmente,
  **detenerse** y reportar. En `SelectivePalletDesignDocument.cs`, I-50 solo toca `From` y `ToDomain`,
  nunca `PropertyValues` ni `WithDesign`.
- **I-51**: sin archivos productivos comunes con los 26 previstos de I-50; AM-4 queda respondida (sección 10).
- **Estado observado al publicar V1.2**:
  - `origin/main` @ `a4d88f1`, sin avanzar.
  - I-49 @ `4cf02b1` (G1 Discovery): solo documentos; **cero** archivos productivos.
  - I-51 @ `4c79e4a` (G3): **producto** `src/RackCad.Application/Persistence/RackDuplicationPlan.cs`
    (nuevo) más `tests/RackCad.Tests/RackDuplicationPlanTests.cs` (nuevo) y
    `tests/RackCad.Tests/SelectiveDuplicationFailClosedTests.cs` (modificado). **Cero** intersección con los
    26 archivos previstos de I-50. El planificador compara vistas del Selectivo con
    `SelectiveAuthoredAuthority.IsSameAuthority` (include-by-default), así que el campo de I-50 participará
    automáticamente, sin código en ninguna de las dos iniciativas. Único cruce práctico: T-15 va en un archivo
    nuevo (sección 13.5).
- **Documentales**: `docs/ROADMAP.md`, `docs/ideas-futuras.md` y el índice de `docs/adr/README.md` pueden
  chocar textualmente con I-49 e I-51 al integrar. Si una paralela también numera un ADR-0035, quien integre
  después renumera el suyo.
- **Riesgos de producto**: sitios de copia omitidos que vuelven a legacy en silencio (sección 12); etiquetas
  desplazadas en vistas apagadas (P-08, OV-7); una carga que marque «tocado» y materialice `7` (P-11, T-17..T-19);
  builds anteriores (P-07); variación de huella de `RACKLAYOUT` (P-13, OV-9, consecuencia aceptada).

## 16. Preguntas al Arquitecto — respondidas en la revisión de V1.1

| ID | Pregunta | Respuesta del Arquitecto | Reflejo en V1.2 |
|---|---|---|---|
| QA-1 | Ubicación de `DimensionViewVisibility` (Domain) y de `DimensionViewKind` más `DimensionViewPolicy` (Application) | **AGREE** | Sin cambios; MIN-3 queda como decisión local no contractual (14.2) |
| QA-3 | Sin miembro `All` | **AGREE**; la máscara de bits conocidos queda privada a la regla | P-01 lo explicita |
| QA-4 | Estrategia RED por capa con superficie inerte y commits solo en verde | **AGREE**, con las condiciones de MIN-7 | 13.1 punto 3 |
| QA-5 | Orden G4 → G5 → G6 → G3 | **AGREE** con el orden; exigía fijar el contenido de cada gate (MAT-A1) | 14.3 |

**QA-2** sigue retirada (resuelta por el Coordinador en MATERIAL-01 de V1). **No quedan preguntas abiertas**
para la re-revisión: V1.2 somete la tabla de gates (14.3) y los MINOR incorporados.
