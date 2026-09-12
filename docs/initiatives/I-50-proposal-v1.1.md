# I-50 — Proposal V1.1: visibilidad de cotas por tipo de vista

> # PROPOSAL V1.1 — NOT CONSENSUS
>
> # Implementación BLOQUEADA
>
> Documento de G2A, **solo documentación**. Queda sometido a revisión del **Coordinador** y del
> **Arquitecto**; mientras no estén de acuerdo **sobre esta misma versión**, no se escribe una sola línea
> de producción (contrato, sección 10). Además, **ADR-0035 debe estar `aceptado` por el Owner antes de
> cualquier código productivo** (confirmación del Coordinador). La decisión de arquitectura nace como
> [ADR-0035](../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md) en estado **`propuesto`**; aceptarlo
> o rechazarlo corresponde solo al Owner.
>
> ```text
> Versión sometida   = V1.1   (sustituye a V1; V1 queda INTACTA como registro de su ronda)
> Historial          = V1 (I-50-proposal-v1.md, 93c352a): Coordinator NOT AGREED — MATERIAL-01, MATERIAL-02
> Base del análisis  = I-50-discovery.md (G1 CLOSED)
> Código auditado    = a4d88f18a1f42263d366c44dc05dd18a6786f152   (origin/main, sin avanzar)
> Decisiones entrada = CD-01..CD-11 (contrato, sección 12) y confirmaciones del Coordinador sobre V1 (sección 1.2)
> Estado             = Coordinator: PENDING REVIEW (V1.1) · Architect: NOT REVIEWED · Consensus: NOT REACHED
> Compuerta de código= consenso Coordinator + Architect sobre la MISMA versión Y ADR-0035 aceptado por el Owner
> ```
>
> Las citas `archivo:línea` se refieren a `a4d88f1`.

## Qué cambia en V1.1 respecto a V1 — leer esto antes de revisar

V1 recibió **NOT AGREED** del Coordinador con dos hallazgos materiales. V1.1 los incorpora y registra las
confirmaciones del Coordinador. **Todo lo demás es idéntico a V1.**

| # | Cambio | Origen | Dónde |
|---|---|---|---|
| 1 | **Se RETIRA la regla «entero negativo ⇒ `null` ⇒ legacy».** Nuevo contrato: la ausencia o `null` es el **único** camino a legacy; **cualquier entero presente se conserva exactamente** (DTO → dominio por conversión cruda, sin máscara ni normalización; dominio → DTO, el mismo entero). `EffectiveDetail` observa **solo** los bits 1, 2 y 4; cualquier otro bit, **incluido el de signo**, sobrevive la ida y vuelta y no decide. Ningún valor presente se convierte en ausencia | MATERIAL-01 | P-03, P-04, P-07, P-11 |
| 2 | Pruebas explícitas: `13` ⇒ conserva `13`, Frontal y Planta ON, Lateral OFF; `-1` ⇒ conserva `-1`, las tres ON; `-8` ⇒ conserva `-8`, las tres OFF | MATERIAL-01 | T-01, T-02, T-11, T-12, T-13, T-15, T-17..T-19 |
| 3 | **QA-2 desaparece**: el Coordinador resolvió la conducta de los valores presentes | MATERIAL-01 | sección 16 |
| 4 | **Se RETIRA T-22**, la guarda de texto «ningún archivo de `src/RackCad.Plugin` nombra `DimensionViews`», y **no** se sustituye por otra guarda de texto. La conformidad «Plugin sin cambios por I-50» pasa a verificarse **operacionalmente** con un diff en cada revisión y en cada Candidato: evidencia **CE-01**, **fuera** de la suite automatizada. Se mantienen el build Debug del Plugin, el CI y las pruebas conductuales. La serie T queda en T-01..T-21 | MATERIAL-02 | secciones 13.4 y 14.1 |
| 5 | Confirmaciones del Coordinador registradas; **no cambian contenido**: orden de gates **aprobado**, ADR-0035 aceptado antes de cualquier código, I-50 **no modifica** `RACKLAYOUT` | revisión de V1 | secciones 1.2, 10, 14.3 |

## 0. Qué decide esta Proposal, y qué no

**Decide**, para las decisiones `CD-01`..`CD-11` ya tomadas:

- la **representación** del dato, comparando **solo** dos formas (sección 2);
- el formato de cable, la semántica de nulo/legacy y el tratamiento de valores desconocidos;
- la **regla única** `EffectiveDetail(detail, policy, viewKind)` y cómo gobierna el alcance de las etiquetas;
- la lista completa de sitios de copia, el comportamiento en cada flujo (guardar y reabrir, `RACKEDITAR`,
  Actualizar, vista enlazada nueva, `RACKDUPLICAR`) y las pruebas RED previstas.

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

### 1.2 Confirmaciones del Coordinador en la revisión de V1

**Aprobado** en V1.1, sin cambios respecto a V1:

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
| Forma canónica frente a la autoridad multivista | **Frágil**: los arrays se comparan **en orden** y las cadenas con comparación **Ordinal**, así que `["planta","frontal"]` y `["frontal","planta"]`, o `"Frontal"` y `"frontal"`, son hermanas **divergentes** y abortan `RACKBOMTOTAL` y la propagación; obliga a canonicalizar orden, mayúsculas y duplicados en cada escritura | **Única por construcción**: un número se compara numéricamente | `SelectiveAuthoredAuthority.cs:162-226` |
| Semántica en los sitios de copia | Tipo por **referencia**: una asignación directa comparte la misma lista entre diseño, sistema, vistas por fondo y lados de Push Back; exige copia defensiva en cada sitio | Tipo por **valor**: la asignación copia | 14 sitios de copia, varios escritos a mano ([Discovery](I-50-discovery.md) §5.5) |
| Contrato de persistencia | Hay que fijar una regla de lectura de texto (mayúsculas, espacios, duplicados, desconocidos): ensancharla o estrecharla cambia qué documentos cargan (lección de I-48 G4A.1) | Ordinal entero, como `Dimensions` en el mismo DTO y los ordinales de `SafetySide` | `SelectivePalletDesignDocument.cs:106`; I-46 |
| Coherencia con el campo vecino | Distinta a `int? Dimensions` | Igual a `int? Dimensions` | `SelectivePalletDesignDocument.cs:106`, `DynamicRackSystemDocument.cs:62` |
| Valores desconocidos o futuros | El dominio debe guardar la lista **cruda** para no perderlos | Cualquier entero sobrevive solo, bit de signo incluido: convertir `int` a enum no enmascara | sección 5 |
| Oráculo de pruebas | combinaciones × orden × mayúsculas | tabla de verdad de 8 combinaciones más valores con bits desconocidos | sección 13 |
| Legibilidad del JSON | **Mejor**: `"frontal"` se explica solo | Opaco (`5`), como ya lo es `Dimensions` | — |
| Reutiliza un vocabulario congelado | **Sí**, el del sobre | No: introduce ordinales nuevos que deben congelarse | — |

**Recomendación: B** (aprobada por el Coordinador). Las dos ventajas de A —legibilidad y vocabulario
compartido— son de presentación. Las de B evitan **dos clases de defecto silencioso** que el árbol castiga
hoy: hermanas divergentes por orden o mayúsculas, que la autoridad multivista trata como corrupción y aborta,
y alias de listas mutables entre sitios de copia. Además, cualquier entero sobrevive sin código adicional.

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
  «todas» donde se quería legacy, y su valor cambiaría el día que se añadiera un tipo.

### P-02 — Propiedad en los cuatro tipos de dominio

`public DimensionViewVisibility? DimensionViews { get; set; }`, con valor por defecto `null`, en
`SelectivePalletDesign`, `SelectiveRackSystem`, `DynamicRackDesign` y `DynamicRackSystem`. Push Back la
recibe a través de su estructura dinámica, y su lado B no la tiene propia (`CD-02`).

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
- `From` (dominio → DTO): `(int?)design.DimensionViews`, el **entero exacto**, sin enmascarar.
- `ToDomain`/`ToDesign` (DTO → dominio): `null` → `null`; **cualquier otro entero** →
  `(DimensionViewVisibility)value`, **conversión cruda, sin máscara ni normalización** (V1.1). No hay
  ningún caso que convierta un valor presente en `null`.
- Sitios exactos: `SelectivePalletDesignDocument.From` (`:268`) y `ToDomain` (`:401`);
  `DynamicRackSystemDocument.From(system)` (`:112`), `From(design)` (`:161`), `ToDesign` (`:215`) y
  `ToDomain` (`:309`).
- **Ninguna `SchemaVersion` cambia.** Precedente: `SelectivePalletDesignDocument` sigue en 1.0 y
  `PushBackDesignDocument` en 1.0 tras varios campos aditivos; `DynamicRackSystemDocument` no tiene
  versión propia.
- El tipo JSON (entero) también es contrato: un valor no entero hace fallar la deserialización del diseño
  igual que hoy con `Dimensions`. I-50 no añade tolerancia de tipo.

### P-04 — Semántica de nulo y legacy (`CD-06`, `CD-07`)

| Valor de `DimensionViews` | `Dimensions = None` | `Dimensions` ≠ `None` |
|---|---|---|
| `null` (ausente) | ninguna cota | **legacy exacto**: las tres vistas con `Dimensions`, como hoy |
| `0` | ninguna cota | ninguna cota |
| cualquier otro entero presente (con o sin bits desconocidos, positivo o negativo) | ninguna cota | solo las vistas cuyo bit conocido (`1`, `2`, `4`) está activo, con `Dimensions`; el valor **se conserva exacto** (sección 5) |

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
| `SelectiveDimensions.AddFrontal`, `FrontalBottomReach` | `Frontal` | todas las frontales por fondo (`FondoSystemView` copia el campo) |
| `SelectiveDimensions.AddLateralCorte` | `Lateral` | cada corte |
| `SelectiveDimensions.AddPlanta` | `Planta` | |
| `DynamicViewDecorations.AppendFrontal` | `Frontal` | **`end` no participa** (`CD-02`): salida, entrada y los cuatro cortes frontales de Push Back |
| `DynamicViewDecorations.AppendLateral` | `Lateral` | lateral entero, cada corte y cada lado del compuesto (D4) |
| `DynamicViewDecorations.AppendPlanta` | `Planta` | un sentido y compuesto |

El tipo sale de **qué método emite**, no de un argumento nuevo. Las firmas públicas de los builders y de los
servicios de dibujo **no cambian**.

## 5. Valores presentes, desconocidos y futuros

### P-07 — Contrato V1.1 (MATERIAL-01)

- **Ausencia o `null`** ⇒ `null` ⇒ legacy exacto. Es el **único** camino a legacy.
- **Cualquier entero presente se conserva EXACTAMENTE**: `0`, `7`, `13`, `-1`, `-8` o cualquier otro.
  - DTO → dominio: `(DimensionViewVisibility)value`, **sin máscara ni normalización**.
  - Dominio → DTO: `(int?)design.DimensionViews`, el **mismo** entero.
  - Ningún valor presente se convierte en ausencia, y ninguno se reescribe distinto.
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
- **Builds anteriores** que re-guardan: pierden el campo (siempre en Dinámico y Push Back; en Selectivo solo
  los anteriores a I-47) y el rack vuelve a legacy, así que **las cotas reaparecen**. Limitación declarada;
  no se sube el major, porque bloquear la apertura de racks por una preferencia visual sería
  desproporcionado.

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
  el campo (`:83-84`).
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
  La autoridad multivista sigue viendo hermanas iguales.
- **Cero cambios en el Plugin**: `EditSelective`, `EditDynamic` y `EditPushBack` no leen ni escriben el
  campo; les llega dentro del diseño. La conformidad se verifica con la evidencia CE-01 (sección 13.4).

### P-11 — «Sin tocar» no materializa (`CD-07`)

Regla pura en `DimensionViewPolicy`, para que las tres ventanas no repitan aritmética de bits (AGENTS,
convención 2):

```text
Conocidos = Frontal | Lateral | Planta        (= 7; ~Conocidos incluye todos los bits desconocidos y el de signo)

FromEditor(loaded, touched, frontal, lateral, planta):
  si !touched  -> loaded                                     (null sigue null; un entero presente vuelve EXACTO)
  si touched   -> ((loaded ?? None) & ~Conocidos) | casillas (nunca null; conserva bits desconocidos y de signo)
```

- **«Tocar»** = que el usuario cambie **una de las tres casillas de vista**. Cambiar solo el nivel o el
  estilo **no** toca la visibilidad, y un rack legacy sigue siendo legacy.
- Cargar, recargar (`RestoreFrom`, `LoadFromModel`) y los recálculos automáticos **no** tocan.
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
del cambio» (contrato, sección 8).

## 10. RACKDUPLICAR y RACKLAYOUT

### P-13

- `RACKDUPLICAR` clona las entidades de la definición elegida, con las cotas **ya dibujadas**
  (`RackDuplicarCommands.cs:190-197`), y re-estampa el diseño:
  - Selectivo: `SelectiveAuthoredRestamp.Restamp` hace ida y vuelta **por el documento**
    (`RestampResult.cs:67-74`), así que un campo declarado sobrevive con su entero exacto.
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
- Toda instancia con otro rol queda idéntica, para cualquier política.
- BOM: el del Selectivo cuenta con `None` forzado (`SelectiveBomBuilder.cs:60-68`, `:508`), y ni
  `SystemBomBuilder` ni `PushBackBomBuilder` ejecutan decoraciones. **El BOM es idéntico** para cualquier
  política (T-16).
- GUID, `View`, `Section`, nombre de bloque, sobre, capa `RACKCAD_COTAS` y materializador: sin cambio.

## 12. Lista de sitios de copia

Todo sitio que hoy propaga `Dimensions` debe propagar `DimensionViews` en el **mismo** punto y con el
**mismo** entero: sin máscara, sin normalización y sin convertir un valor presente en `null`. Omitir un
sitio no falla: vuelve a legacy en silencio (sección 7.3). Cada fila tiene su prueba en T-13.

| # | Archivo | Símbolo | Línea actual | Dirección |
|---|---|---|---|---|
| C-01 | `RackSelectiveWindow.xaml.cs` | `BuildDesign` / `LoadDesign` | `:2338` / `:2686` | UI ⇄ entradas |
| C-02 | `SelectiveDesignInputs.cs` | propiedad | `:43-44` | entradas |
| C-03 | `SelectiveEditorState.cs` | construcción del diseño | `:1317-1318` | entradas → diseño |
| C-04 | `SelectiveGeometryResolver.cs` | resolución | `:66-67` | diseño → sistema |
| C-05 | `SelectiveDepthLayout.cs` | `FondoSystemView` | `:83-84` | sistema → vista por fondo |
| C-06 | `SelectivePalletDesignDocument.cs` | `From` / `ToDomain` | `:268` / `:401` | diseño ⇄ DTO |
| C-07 | `RackDynamicSystemWindow.xaml.cs` | `ReadAnnotationOptions` / `RestoreFrom` | `:292-298` / `:2772-2775` | UI ⇄ opciones |
| C-08 | `DynamicAnnotationOptions.cs` | propiedad | `:16-17` | opciones |
| C-09 | `DynamicEditorDesignAssembler.cs` | `BuildDesign` | `:174-175` | opciones → diseño |
| C-10 | `DynamicRackSystemResolver.cs` | diseño → sistema / sistema → diseño | `:244-245` / `:357-358` | diseño ⇄ sistema |
| C-11 | `DynamicRackSystemDocument.cs` | `From(system)`, `From(design)`, `ToDesign`, `ToDomain` | `:112`, `:161`, `:215`, `:309` | DTO |
| C-12 | `RackPushBackSystemWindow.xaml.cs` | `LoadFromModel` / `ReadInputs` | `:378` / `:810-817` | UI ⇄ opciones |
| C-13 | `PushBackEditorState.Load.cs` | opciones de anotación | `:206-208` | diseño → opciones |
| C-14 | `PushBackMirror.cs` | `Structure` | `:158-160` | clon |
| C-15 | `PushBackCompositeStructure.cs` | `CopySharedStructuralIntent` | `:856-858` | compartido → lados y compuesto |

**No cambian, a propósito**: `PushBackEditorDesignAssembler.cs:378` (solo reenvía las opciones),
`SelectivePalletDesignDocument.WithDesign`, `LinkedPropertyReconciler`, `SelectiveEffectiveDesignResolver`
(usa `ToDomain`), `SelectiveAuthoredAuthority`, `SelectiveLibraryExport`, `RackEmbedDocument`,
`RackEmbedComposer`, `RackBlockData`, `RackEnvelopeRestamp`, los kind handlers, los builders de BOM,
`RackLayoutCommands` y todo `src/RackCad.Plugin`.

## 13. Pruebas RED previstas

### 13.1 Estrategia

1. **Caracterización primero, en verde sobre el árbol sin tocar**: firma de hoy con cotas activas donde falta
   (Selectivo y Push Back; el Dinámico ya tiene `DynamicNullOverrideGoldenTests`). Tiene que seguir verde con
   `DimensionViews = null`: es la prueba del legacy exacto.
2. **RED por capa, con fallo de comportamiento**, nunca un fallo de compilación. Primero se introduce la
   superficie **inerte** de la capa y después se escriben las pruebas que la hacen fallar:
   - la regla que aún devuelve `detail`: falla T-01;
   - los emisores sin cablear: fallan T-06..T-09;
   - el DTO sin mapear: fallan T-11 y T-12;
   - las copias sin propagar: falla T-13;
   - las ventanas sin cablear: fallan T-17..T-19.
3. **Solo se commitea en verde**, sin SHA remoto rojo, con la evidencia RED (conteo de fallos) en el cuerpo
   del commit (precedente I-48).

### 13.2 Suite Core (`tests/RackCad.Tests`)

| ID | Prueba | Clase |
|---|---|---|
| T-01 | `DimensionViewPolicy.EffectiveDetail`: tabla de verdad con 4 niveles × {`null`, 0..7, 8, `13`, `-1`, `-8`} × 3 tipos. Casos **explícitos**: `13` ⇒ Frontal y Planta con detalle, Lateral `None`; `-1` ⇒ las tres con detalle; `-8` ⇒ las tres `None`. `None` gana; `null` = detalle; un tipo no definido lanza | RED |
| T-02 | `DimensionViewPolicy.FromEditor`: sin tocar devuelve **exacto** lo cargado (`null`, `7`, `13`, `-1`, `-8`); tocado conserva bits desconocidos y de signo (`-8` + Frontal ⇒ `-7`; `13` sin Planta ⇒ `9`); legacy tocado da solo las casillas | RED |
| T-03 | Caracterización Selectivo: firma frontal (1 y 2 fondos), cada corte y planta con Minimal/Standard/Detailed y numeración y nombre activos; verde hoy y con `null` | Caracterización |
| T-04 | Caracterización Dinámico: `DynamicNullOverrideGoldenTests` intacta más planta y etiquetas; verde hoy y con `null` | Caracterización |
| T-05 | Caracterización Push Back: un sentido y compuesto A/B, 4 cortes frontales, lateral entero y por poste, planta, con cotas activas; verde hoy y con `null` | Caracterización |
| T-06 | Selectivo por vista: {F}, {L}, {P}, {F, P}, 0; todas las frontales por fondo siguen F; cada corte sigue L | RED |
| T-07 | Dinámico por vista: salida y entrada siguen F; lateral entero y cortes siguen L; planta sigue P | RED |
| T-08 | Push Back por vista: EntradaSalida y Posterior; 4 cortes compuestos; lateral compuesto en ambos lados; planta compuesta | RED |
| T-09 | Etiquetas: con una vista apagada, sus etiquetas quedan donde las pone `None`; las demás vistas no cambian | RED |
| T-10 | `None` gana: `Dimensions = None` con cualquier política da cero instancias `Dimension` en todo | Guarda |
| T-11 | DTO Selectivo: `null` no se escribe y el JSON es byte-idéntico al de hoy; `0`, `1`, `5`, `7`, `13`, `-1` y `-8` hacen ida y vuelta **exactos**, en dominio y en el número escrito; ningún valor presente vuelve como `null`; `WithDesign` y la exportación a biblioteca lo conservan | RED |
| T-12 | DTO Dinámico y Push Back: los cuatro mapeos conservan el entero exacto (incluidos `13`, `-1` y `-8`); ida y vuelta por `RackProjectStore` (incluido `Structure`); `null` no se escribe | RED |
| T-13 | Sitios de copia C-02..C-06 y C-08..C-11, C-13..C-15: cada uno propaga el **entero exacto**, con un centinela con bits desconocidos y de signo (p. ej. `-8`) para detectar cualquier máscara; incluye `PushBackRuns.Clone` y los dos lados del compuesto | RED |
| T-14 | Autoridad multivista: documentos que difieren solo en `DimensionViews` ⇒ `Divergent`; iguales ⇒ `Single` | Guarda |
| T-15 | `RACKDUPLICAR`: `SelectiveAuthoredRestamp` conserva el entero exacto (`13`, `-1`, `-8`); Dinámico y Push Back devuelven el JSON intacto | Guarda |
| T-16 | Sin cambio de geometría ni BOM: para cada política, las instancias que no son cotas son idénticas, y el BOM de los tres sistemas también | Guarda |

### 13.3 Suite UI (`tests/RackCad.UI.Tests`)

| ID | Prueba | Clase |
|---|---|---|
| T-17 | Selectivo: legacy carga con tres casillas activas; guardar sin tocar deja `null`; apagar Lateral da F\|P; cambiar solo el nivel deja `null`; una recarga explícita se refleja; `-8` carga con las tres apagadas y se guarda sin tocar como `-8`; tocar conserva bits desconocidos y de signo (`-8` + Frontal ⇒ `-7`) | RED |
| T-18 | Dinámico: lo mismo, incluido `RestoreFrom` repetido | RED |
| T-19 | Push Back: lo mismo, incluido el recálculo dentro de `LoadFromModel`; y H1 caracterizado: `DimensionStyle` sigue saliendo `null` | RED y caracterización |
| T-20 | Censos de `x:Name` (`DynamicShellMigrationTests`, `SelectiveShellMigrationTests`, `PushBackModuleEditorCharacterizationTests`): se **actualizan**, no se relajan | Guarda |
| T-21 | Firmas de dibujo de I-24 (`DynamicEditorWindowTests.FullDrawingSignature`, `SelectiveEditorWindowTests.DrawingSignature`) con variantes de política | RED |

### 13.4 Plugin: sin suite automatizada (ADR-0003)

**T-22 queda RETIRADA** (MATERIAL-02). No hay guarda de texto sobre `src/RackCad.Plugin` y no se sustituye
por otra. La conformidad «Plugin sin cambios por I-50» se verifica **operacionalmente**:

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

**Debe quedar vacío** para el diff propio de I-50. Se mantienen, además:

- **build Debug del Plugin, obligatorio** (AGENTS.md, «Pruebas — definición de terminado», punto 3);
- **CI obligatorio** sobre el SHA exacto que corresponda;
- las **pruebas conductuales** existentes y T-01..T-21.

| ID | Evidencia | Clase |
|---|---|---|
| CE-01 | Diff de `src/RackCad.Plugin` contra el merge-base con `origin/main`: vacío | Conformidad, fuera de la suite |
| OV | Validación del Owner en AutoCAD 2025 (contrato, sección 9), más la posición de etiquetas con una vista apagada, la **variación de huella de `RACKLAYOUT`** con la planta apagada (consecuencia aceptada, no se corrige) y Push Back compuesto A/B | Owner |

## 14. Archivos previstos y orden de gates

### 14.1 Producción (no se toca en G2)

| Capa | Nuevos | Modificados |
|---|---|---|
| Domain | `Systems/Shared/DimensionViewVisibility.cs` | `SelectivePalletDesign.cs` (caliente), `SelectiveRackSystem.cs`, `DynamicRackDesign.cs`, `DynamicRackSystem.cs` |
| Application | `Systems/Shared/DimensionViewPolicy.cs` (con `DimensionViewKind`) | `SelectiveDimensions.cs`, `DynamicViewDecorations.cs`, `SelectiveGeometryResolver.cs`, `SelectiveDepthLayout.cs`, `SelectiveDesignInputs.cs`, `SelectiveEditorState.cs`, `DynamicRackSystemResolver.cs`, `DynamicAnnotationOptions.cs`, `DynamicEditorDesignAssembler.cs`, `PushBackMirror.cs`, `PushBackCompositeStructure.cs`, `PushBackEditorState.Load.cs`, `SelectivePalletDesignDocument.cs`, `DynamicRackSystemDocument.cs` |
| UI | — | `RackSelectiveWindow.xaml/.cs`, `RackDynamicSystemWindow.xaml/.cs`, `RackPushBackSystemWindow.xaml/.cs` (los tres calientes) |
| Plugin | — | **ninguno** (verificado por CE-01) |

**Total: 2 nuevos y 24 modificados.** Pruebas: T-01..T-21 en la suite, en archivos nuevos donde sea posible
para no chocar con suites ajenas, más la evidencia CE-01 fuera de la suite.

### 14.2 UI mínima (restricciones para G3)

- Una sola sección: la existente **«Cotas»** de cada editor recibe tres casillas, **Frontal**, **Lateral**
  y **Planta**. No hay ventana nueva, así que el censo de ventanas de I-39 no se mueve.
- Sin controles en Cantilever, Cama, Larguero ni Cabecera (contrato, sección 5).
- El texto explica que la elección es **por tipo de vista del rack**: todas las frontales, todos los cortes
  y todas las copias comparten. Se actualiza la leyenda del Selectivo «(las tres vistas)»
  (`RackSelectiveWindow.xaml:63`).

### 14.3 Orden de gates — APROBADO por el Coordinador

`G4` (caracterización, regla y emisores) → `G5` (DTO y sitios de copia) → `G6` (cobertura de Push Back
compuesto) → `G3` (UI) → `G7` (Candidato y Owner) → `G8` (docs e integración).

Motivo: la UI no debe poder **escribir** un campo que nada consume todavía, el mismo patrón de «intent sin
executor» que I-47 corrigió en su plan. Mantiene la numeración del contrato y solo cambia el orden de
ejecución. Ningún gate de código se abre antes de que ADR-0035 esté `aceptado` y haya consenso sobre la
misma versión de esta Proposal.

## 15. Coordinación y riesgos

- **I-49**: antes de editar `RackSelectiveWindow.xaml/.cs` o `SelectivePalletDesignDocument.cs`, aplicar el
  protocolo del contrato (sección 11): fetch; `git diff --name-only
  origin/main...origin/architecture/motor-expresiones-parametricas`; si I-49 los modifica materialmente,
  **detenerse** y reportar. En `SelectivePalletDesignDocument.cs`, I-50 solo toca `From` y `ToDomain`,
  nunca `PropertyValues` ni `WithDesign`.
- **I-51**: sin archivos productivos comunes; AM-4 queda respondida (sección 10).
- **Estado al publicar V1.1**: el diff de I-49 y el de I-51 contra `origin/main` solo contienen documentos;
  **cero** archivos productivos y **cero** intersección con los 26 archivos previstos de I-50.
- **Documentales**: `docs/ROADMAP.md`, `docs/ideas-futuras.md` y el índice de `docs/adr/README.md` pueden
  chocar textualmente con I-49 e I-51 al integrar. Si una paralela también numera un ADR-0035, quien integre
  después renumera el suyo.
- **Riesgos de producto**: sitios de copia omitidos que vuelven a legacy en silencio (sección 12, T-13);
  etiquetas desplazadas en vistas apagadas (P-08, OV); builds anteriores (P-07); variación de huella de
  `RACKLAYOUT` (P-13, consecuencia aceptada).

## 16. Preguntas al Arquitecto

| ID | Pregunta | Estado |
|---|---|---|
| QA-1 | Ubicación: `DimensionViewVisibility` en Domain (lo usan los diseños) y `DimensionViewKind` más `DimensionViewPolicy` en Application (solo los usan los emisores y la regla de la UI). ¿De acuerdo? | abierta |
| QA-3 | Sin miembro `All` en el enum (P-01). | aprobada por el Coordinador; pendiente del Arquitecto |
| QA-4 | Estrategia RED por capa con superficie inerte y commits solo en verde (13.1). | abierta |
| QA-5 | Orden de gates G4 → G5 → G6 → G3 (14.3). | aprobado por el Coordinador; pendiente del Arquitecto |

**QA-2 retirada**: la conducta de los valores presentes (negativos y bits desconocidos) la **resolvió el
Coordinador** en MATERIAL-01; queda fijada en P-07.
