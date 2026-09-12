# ADR-0035: Visibilidad de cotas por tipo de vista del rack

- **Estado:** propuesto
- **Fecha:** 2026-09-12 (propuesto)
- **Decisores:** Mario Pérez, Owner del repositorio (acepta o rechaza; **pendiente**); Coordinador de I-50
  (decisiones `CD-01`..`CD-11`; revisión de la Proposal: V1 **NOT AGREED**, V1.1 **AGREED**, V1.2
  **pendiente**); Arquitecto de I-50 (V1.1 **NOT AGREED** por MAT-A1; V1.2 **pendiente de re-revisión**);
  Claude (redacción)
- **Iniciativa relacionada:** I-50 — `feature/cotas-independientes-por-vista`
  ([contrato](../initiatives/I-50-cotas-independientes-por-vista.md),
  [Discovery](../initiatives/I-50-discovery.md), [Proposal V1.2](../initiatives/I-50-proposal-v1.2.md);
  [Proposal V1.1](../initiatives/I-50-proposal-v1.1.md) y [Proposal V1](../initiatives/I-50-proposal-v1.md)
  como registro de sus rondas)

> **Estado `propuesto`.** Este registro acompaña a la Proposal **V1.2**, que todavía **no** tiene consenso
> (V1: Coordinador NOT AGREED; V1.1: Coordinador AGREED y Arquitecto NOT AGREED). Puede editarse hasta que el
> Owner lo acepte o lo rechace ([README](README.md)). No autoriza implementación por sí mismo: el
> Coordinador exige que el Owner lo **acepte antes de cualquier código productivo** de I-50.
>
> **Revisión V1.1 de este registro** (2026-09-12): el punto 8 de la decisión deja de leer los enteros
> negativos como legacy y pasa a conservar **cualquier** valor presente exactamente (MATERIAL-01 del
> Coordinador); se explicita que I-50 **no modifica** `RACKLAYOUT`.
>
> **Revisión V1.2 de este registro** (2026-09-12, MIN-9 de la Architect Review de V1.1): el Contexto deja de
> generalizar a todos los sistemas la comparación estricta del diseño completo entre vistas hermanas, que
> está demostrada en el Selectivo; la conducta de los builds anteriores se describe por sistema; y «entero»
> pasa a «`int` (Int32)». **La decisión no cambia.**

## Contexto

Hoy las cotas automáticas se deciden **por rack**: un único `DimensionDetail` (Ninguna, Mínimo, Estándar,
Detallado) y un único `DimensionStyle` gobiernan todas las vistas del rack. Solo tres sistemas dibujan
cotas —Selectivo, Dinámico y Push Back—, siempre en sus vistas frontal, lateral y planta; Cantilever, Cama,
Larguero y Cabecera no emiten ninguna. Las cotas las producen dos emisores puros de Application
(`SelectiveDimensions` y `DynamicViewDecorations`), y un único materializador del Plugin las dibuja
**dentro de la definición de bloque** de cada vista. La posición de algunas etiquetas —números de frente y
de nivel, nombre— depende del mismo nivel.

La identidad de una representación es la definición de bloque más su sobre `(Id, View, Section)`
([ADR-0009](0009-identidad-guid-embebida-en-dwg.md)). Las vistas multivista «se reconstruyen desde el mismo
diseño» y las referencias que comparten una definición «reflejan su redefinición»
([ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md)). **En el Selectivo** esa igualdad además se
**comprueba de forma estricta**: `SelectiveAuthoredAuthority` exige que todas las vistas hermanas lleven el
**diseño completo idéntico** —solo `View` y `Section` pertenecen a la vista— y aborta ante divergencia. En el
Dinámico y en Push Back la igualdad es invariante de construcción, sin una comprobación estricta equivalente:
`BomAuthoredAuthority` aprueba su primera vista sin compararlas.

La iniciativa ID1 pide elegir **en qué vistas** se muestran las cotas, reutilizando el motor existente y
conservando **exactamente** el comportamiento de los dibujos existentes. Evidencia completa en el
[Discovery de I-50](../initiatives/I-50-discovery.md).

## Decisión

1. **La visibilidad de cotas es autoridad del rack por tipo de vista**, y vive en el **diseño**. No es
   autoridad de la instancia dibujada: ni el sobre ni la `BlockReference` llevan política.
2. **Los tipos de vista son Frontal, Lateral y Planta.** `Section` no crea un tipo: las frontales de todos
   los fondos, la frontal de salida y la de entrada del Dinámico, y los cuatro cortes frontales de Push Back
   son Frontal; todos los cortes laterales son Lateral.
3. **`DimensionDetail` y `DimensionStyle` siguen siendo globales** del rack. La política solo enciende o
   apaga cada tipo.
4. **El dato es un `[Flags] DimensionViewVisibility` nulo** (`None = 0`, `Frontal = 1`, `Lateral = 2`,
   `Planta = 4`), persistido como `int? DimensionViews` en `SelectivePalletDesignDocument` y en
   `DynamicRackSystemDocument` (Push Back lo hereda), y **omitido cuando es nulo**. Los valores de los bits
   son contrato de persistencia y no se renumeran.
5. **Nulo o ausente = legacy exacto**: todas las vistas que hoy dibujan cotas las dibujan con `Dimensions`,
   como hoy. Un rack legacy guardado sin tocar su visibilidad **no** se reescribe con una política explícita.
6. **`Dimensions = None` siempre gana** y produce cero cotas.
7. **Una sola regla pura decide**: `EffectiveDetail(detail, policy, viewKind)` en Application, consumida
   solo por los dos emisores. El detalle efectivo de una vista gobierna **sus cotas y el alcance de sus
   etiquetas**: una vista apagada coloca sus etiquetas como hoy lo hace `None`.
8. **Valores presentes**: la ausencia o `null` es el **único** camino a legacy. **Cualquier `int` (Int32)
   presente se conserva exactamente**: del DTO al dominio por conversión cruda, sin máscara ni
   normalización, y del dominio al DTO como el mismo `int`. `EffectiveDetail` observa **solo** los bits
   `1`, `2` y `4`; cualquier otro bit, **incluido el de signo**, sobrevive la ida y vuelta y no decide.
   Ningún valor presente se convierte en ausencia. Así, `13` activa Frontal y Planta, `-1` las tres y `-8`
   ninguna, y los tres valores vuelven idénticos al guardarse. Un valor JSON no entero o fuera de Int32
   conserva la deserialización fallida de siempre: I-50 no amplía la tolerancia.
9. **Sin cambios** de geometría, BOM, GUID, `View`/`Section`, sobre, capa de cotas, materializador,
   comandos del Plugin ni `RACKLAYOUT`.

## Alternativas consideradas

- **Metadata por instancia de vista** (en el sobre o en la referencia) — rechazada. La cota vive dentro de
  la definición y las copias `COPY` la comparten (ADR-0010). Dos bloques con la misma `(Id, View, Section)`
  solo se distinguen por `ObjectId`, que no es identidad (ADR-0009). Los builders nunca ven el sobre.
  `RackEmbedComposer` descarta campos nuevos del sobre. Una vista enlazada nueva heredaría la metadata de la
  vista elegida (escritura cruzada). Y guardarla dentro del diseño haría distintos los diseños de las
  hermanas, contra ADR-0010; en el Selectivo, `SelectiveAuthoredAuthority` además las declararía divergentes.
- **Tipo de vista por `View` + `Section`** (salida distinta de entrada, fondo por fondo) — rechazada por
  decisión del Coordinador. Además, el frontal compuesto de Push Back toma las cotas de sus cuatro cortes de
  una misma pasada.
- **Lista nula de tokens de vista** (`"frontal"`, `"lateral"`, `"planta"`) — descartada frente a los flags.
  La autoridad multivista del Selectivo (`SelectiveAuthoredAuthority`) compara arrays en orden y cadenas
  Ordinal, de modo que un orden o una mayúscula distintos volverían divergentes a las hermanas. Una lista es un tipo por referencia que se comparte entre
  los sitios de copia. Y exige un contrato de lectura de texto.
- **Leer un `int` negativo como legacy** (planteado en la Proposal V1) — rechazado por el Coordinador
  (MATERIAL-01): convertía un valor **presente** en ausencia y lo reescribía distinto. Ahora solo la
  ausencia es legacy, y cualquier `int` presente se conserva exacto.
- **Nivel de detalle por vista** — rechazado por decisión del Coordinador (solo ON/OFF).
- **Subir el major de los documentos** para que un build anterior no pueda re-guardar — rechazado:
  bloquearía abrir racks por una preferencia visual.

## Consecuencias

- **Positivas**
  - Los dibujos existentes no cambian: sin la política, el JSON es byte-idéntico y el dibujo el mismo.
  - La política sobrevive Actualizar, guardar y reabrir, la vista enlazada nueva, `RACKDUPLICAR` y la
    biblioteca, porque viaja con el diseño.
  - Una sola regla y dos emisores: el Plugin y la identidad de vistas no cambian.
- **Negativas y costos aceptados**
  - No existe control por copia: todas las frontales, todos los cortes y todas las copias de un rack
    comparten su tipo.
  - Un build anterior no conoce la política y dibuja legacy, así que las cotas **reaparecen** en lo que ese
    build redibuje. Qué pasa con el campo depende del sistema: un **Selectivo post-I-47 y pre-I-50** puede
    **conservar** `DimensionViews` como dato desconocido en `ExtensionData` al re-guardar, aunque lo ignore al
    dibujar; un **Dinámico o Push Back** anterior —y un Selectivo anterior a I-47— lo **pierde** al re-guardar
    y el rack vuelve a legacy (Proposal, P-07).
  - Apagar las cotas de una vista mueve sus etiquetas.
  - Ocultar las cotas de la planta **encoge la huella** que `RACKLAYOUT` toma de la planta semilla. Es una
    **consecuencia aceptada y explícita**: I-50 no modifica `RACKLAYOUT` para corregirla, y la variación es
    punto de la validación del Owner.
  - Un `int` presente que la UI no puede representar por completo (bits desconocidos o negativos) se
    conserva tal cual: la UI solo muestra y cambia los bits `1`, `2` y `4`.
  - Todo sitio que hoy copia `Dimensions` debe copiar también `DimensionViews`; omitir uno devuelve ese
    camino a legacy sin avisar, así que cada sitio lleva prueba.
  - Un tipo de vista futuro debe definir su propia regla legacy.
- **Qué vigilar**: los tres editores y `SelectivePalletDesign.cs` son archivos calientes; los cambios en
  `RackSelectiveWindow` se coordinan con I-49 (contrato de I-50, sección 11).

## Referencias

- [Contrato de I-50](../initiatives/I-50-cotas-independientes-por-vista.md), sección 12 (decisiones
  `CD-01`..`CD-11`).
- [Discovery de I-50](../initiatives/I-50-discovery.md): matriz, call sites, persistencia e identidad de
  vista.
- [Proposal V1.2 de I-50](../initiatives/I-50-proposal-v1.2.md): comparación de representaciones, regla,
  valores presentes, sitios de copia, contenido y cierre de cada gate, pruebas y trazabilidad.
  [Proposal V1.1](../initiatives/I-50-proposal-v1.1.md) y [Proposal V1](../initiatives/I-50-proposal-v1.md):
  registros de sus rondas.
- `src/RackCad.Application/ProjectVariables/SelectiveAuthoredAuthority.cs` y
  `src/RackCad.Application/Bom/BomAuthoredAuthority.cs`: la comparación estricta entre vistas hermanas del
  Selectivo, y la aprobación de la primera vista para los demás sistemas.
- [ADR-0009](0009-identidad-guid-embebida-en-dwg.md), [ADR-0010](0010-actualizar-redibuja-insertar-liga-vistas.md).
- `docs/ARCHITECTURE.md` §3.2 y §3.3: el detalle de cota es parte del diseño versionado.
