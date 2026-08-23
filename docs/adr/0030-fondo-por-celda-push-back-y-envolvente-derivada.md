# ADR-0030: El fondo de Push Back es de la CELDA; el del frente pasa a ser una envolvente derivada

- **Estado:** **aceptado**
- **Fecha:** 2026-08-23 (propuesto) · 2026-08-23 (aceptado)
- **Decisores:** dueño del repo (**acepta**); Claude (redacción)
- **Iniciativa relacionada:** I-41 — Configuración por celda de Push Back (`feature/push-back-cell-configuration`), IDs internos PB-015 y PB-016

> **Aceptación del dueño (2026-08-23).** El dueño acepta esta decisión **con el modelo tal como quedó
> implementado**, tras validar manualmente en AutoCAD 2025 el DLL construido exactamente desde
> `c41aee1b8bcbfc0d6fed7a38b8c4767538648cd2`. La aceptación incluye **expresamente la limitación
> documentada** en Consecuencias: el corte lateral NO seccionado no dibuja tarimas, por ser una
> envolvente y no una celda. A partir de aquí el contenido de este ADR es **inmutable** (adr/README.md):
> solo pueden cambiar su Estado y sus enlaces.

## Contexto

Desde I-18 el fondo (número de tarimas en el sentido del flujo) es una propiedad del FRENTE:
`DynamicRackFrontDesign.PalletsDeep`. Todos los niveles de un frente comparten longitud de cama, X de
larguero posterior, pendiente y elevaciones. El Owner pide que cada celda `frente × nivel` pueda tener
su propio fondo, y además que cada celda decida si dibuja su tarima.

Eso obliga a decidir qué significa entonces `PalletsDeep` del frente, porque no puede significar dos
cosas a la vez:

1. **el fondo que hereda un nivel sin valor propio** (lo que el usuario escribe en «Fondos frente»), y
2. **la extensión longitudinal del frente**, que es lo que dimensiona la estructura compartida —
   módulos, cabeceras, separadores y postes derivados — y de la que dependen I-35 e I-40.

Hay una restricción dura sobre el segundo significado: el recálculo del editor decide si RECONSTRUYE
la secuencia de módulos comparando el layout de fondos (`DynamicEditorDesignAssembler.MustRebuild`). Si
un cambio de fondo interno moviera ese layout, la reconstrucción tiraría los `ModuleId` y con ellos las
cabeceras por línea y las alturas de poste derivado por línea que I-40 acaba de introducir.

Push Back tampoco puede resolver esto añadiendo campos a los tipos dinámicos: el sistema Dinámico
comparte esos tipos y no debe cambiar de comportamiento.

## Decisión

En Push Back, el fondo se resuelve **por celda** con una única regla de precedencia, que vive en una
sola función (`PushBackCellDepth.Effective`):

```
fondo efectivo(celda) = OverrideDeLaCelda ?? FondoPorDefectoDelFrente
```

acotado después a `[2, envolvente del frente]`.

En consecuencia:

- **`DynamicRackFrontDesign.PalletsDeep` pasa a ser, para Push Back, la ENVOLVENTE DERIVADA**: el mayor
  fondo efectivo de los niveles ACTIVOS del frente. Deja de ser la autoridad final de producto.
- **El fondo POR DEFECTO del frente se persiste aparte**, en `PushBackFrontConfig.DefaultPalletsDeep`.
  Una envolvente no sabe qué heredaba cada nivel, así que sin este campo cada guardar/abrir subiría el
  default hasta la envolvente y el override más profundo desaparecería en el segundo round trip.
- **Toda la geometría, las vistas y el BOM consumen el fondo efectivo de la celda**, nunca el del
  frente: cama, longitud, pendiente, elevaciones, larguero posterior, tope posterior, intermedios y
  cotización de cama.
- **El estado por celda de Push Back vive en su configuración PARALELA** (`PushBackFrontConfig` /
  `PushBackEditorCell`), como ya vivía el peralte del larguero posterior. Los tipos dinámicos no ganan
  ningún campo y el Dinámico sigue teniendo un único fondo por frente.
- **`DrawPallet` es autoridad por celda**, con default legacy `false` — Push Back no dibujaba ninguna
  tarima antes de I-41, así que `false` es lo que conserva el dibujo histórico. Las tarimas son
  referencia VISUAL y nunca entran al BOM.
- **Las operaciones masivas de estas dos propiedades escriben UNA sola propiedad.** Reutilizan los cinco
  alcances existentes (`Cell / Selected / Level / Front / All`) y la multiselección de la matriz, pero
  NO viajan en `PushBackEditorValues`: hacerlo arrastraría el resto de los campos de la celda origen.

## Alternativas consideradas

- **Poner el fondo por celda en `DynamicRackLevelDesign`** (el tipo de celda compartido) — descartada:
  el Dinámico consume ese tipo y habría que garantizar en cada consumidor que un campo nuevo no lo
  altera. La configuración paralela ya es el patrón probado de Push Back (peralte posterior, tope).
- **Dejar `PalletsDeep` del frente como "el fondo que heredan los niveles" y añadir un campo nuevo para
  la extensión estructural** — descartada: la extensión estructural es justo lo que
  `DynamicDepthGeometry` y `MustRebuild` ya leen de `PalletsDeep`. Cambiar cuál de los dos leen tocaría
  el Dinámico, que es exactamente lo que la iniciativa no puede hacer.
- **Derivar el default del frente desde la envolvente al cargar** (no persistirlo) — descartada: pierde
  información. Con default 3 y una celda en 5, la envolvente es 5 y el rack volvería con default 5, de
  modo que quitar el override ya no devolvería 3.
- **Permitir que una celda supere la envolvente sin acotar** — descartada: colgaría un larguero
  posterior fuera de la estructura que lo sostiene. El acotado solo actúa sobre estados incoherentes,
  porque la envolvente ES el máximo de los fondos efectivos.

## Consecuencias

- Positivas:
  - I-35 e I-40 se preservan sin ninguna medida especial: mientras la envolvente no se mueva, el layout
    de fondos es idéntico, `MustRebuild` responde `false`, el recálculo copia el baseline y con él
    sobreviven `ModuleId`, cabeceras por línea y alturas de poste derivado por línea.
  - Los documentos legacy no cambian: sin los tres campos nuevos, el default es el fondo estructural
    (que en un rack anterior coincide con el de todos sus niveles) y ninguna celda dibuja tarima.
  - El Dinámico y el Selectivo quedan intactos, y se comprueba por prueba que no mencionan nada de I-41.
- Negativas / costos aceptados:
  - `PalletsDeep` del frente significa cosas distintas según el sistema (fondo del frente en Dinámico,
    envolvente derivada en Push Back). Está documentado en el XML-doc de `PushBackCellDepth` y aquí; hay
    que vigilar que nadie lo lea como "el fondo de este nivel" en código nuevo de Push Back.
  - La cama deja de ser una única definición anidada por frente: se emite una por FONDO EFECTIVO
    distinto. Un rack sin overrides sigue produciendo exactamente un grupo, así que el patrón ARRAY de
    [ADR-0011](0011-parametros-dinamicos-con-patron-array.md) no se degrada salvo en racks escalonados,
    donde una definición por fondo distinto es el mínimo posible.
  - El corte lateral ya no puede identificar un frente por `(StartX, EndX, LoadLevels)`: dos frentes
    pueden coincidir en los tres y escalonar distinto. Su clave de agrupación incorpora ahora el fondo
    efectivo y el flag de tarima de cada nivel.
  - El lateral NO seccionado no dibuja tarimas: ya era una envolvente antes de I-41 (sus largueros
    posteriores se colocan sobre la longitud total del rack) y no hay celda a la que preguntar. Las
    tarimas viven en los CORTES laterales y en los dos cortes frontales. Es una limitación declarada.

## Referencias

- I-18 (Push Back), I-33 (frente en blanco), I-35 (módulos longitudinales), I-40 (cabeceras por línea).
- [ADR-0011](0011-parametros-dinamicos-con-patron-array.md) — patrón ARRAY de definiciones compartidas.
- `src/RackCad.Application/Systems/PushBack/PushBackCellDepth.cs` — la regla de precedencia, en un solo sitio.
- `src/RackCad.Application/Systems/PushBack/PushBackTarimaPlacement.cs` — dónde se apoya y cómo se
  orienta la tarima (`Y de apoyo = origen del rodillo + radio del rodillo`).
- `tests/RackCad.Tests/PushBackCellDepthTests.cs`, `PushBackCellPalletTests.cs`,
  `PushBackCellConfigurationCharacterizationTests.cs`, `PushBackCellConfigurationDeliveryTests.cs`,
  `PushBackTarimaPlacementTests.cs`.

## Notas posteriores

**2026-08-23 — corrección de la representación visual de la tarima (PB-016), previa a la aceptación.**
La validación manual del Owner encontró dos defectos **solo de dibujo**, que no alteran esta decisión y
se corrigieron antes de aceptarla:

1. en el corte **lateral** las tarimas se veían escalonadas —cada una horizontal a una altura distinta—
   y se apoyaban en la línea del ORIGEN del bloque de la cama, que es donde se atornilla el riel. Ahora
   se construyen en el sistema LOCAL de la cama sobre la superficie de los RODILLOS
   (`origen del rodillo + radio`) y se llevan a mundo con la MISMA transformación rígida del montaje de
   riel y rodillos, llevando su rotación: tangencia y pendiente salen por construcción;
2. en los cortes **frontal y posterior** la tarima se repartía con huecos iguales a lo largo del
   larguero, y las calles reales miden BFR con la holgura del larguero repartida a los dos extremos.
   Ahora cada tarima va centrada en su calle, y su altura es la misma superficie de apoyo del lateral
   evaluada en el extremo que ese corte muestra.

Ninguna de las dos tocó persistencia, BOM, fondos por celda ni los alcances.
