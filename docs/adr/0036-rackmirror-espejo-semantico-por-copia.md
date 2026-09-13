# ADR-0036: RACKMIRROR es un espejo semántico por copia: reflexión canónica por kind, vistas admisibles y colocación sin escala negativa

- **Estado:** **propuesto**
- **Fecha:** 2026-09-12 (propuesto)
- **Decisores:** Mario Pérez, Owner del repositorio (**acepta o rechaza**; pendiente); Coordinador de I-52 y
  Arquitecto de I-52 (consenso técnico **pendiente** sobre la Proposal); Claude (redacción)
- **Iniciativa relacionada:** I-52 — `feature/rackmirror-espejo-semantico`
  ([contrato](../initiatives/I-52-rackmirror-espejo-semantico.md),
  [Discovery](../initiatives/I-52-discovery.md), [Proposal V1](../initiatives/I-52-proposal-v1.md),
  [decisiones](../automation/decisions/I-52.md))

> **Numeración.** 0036 es el primer número libre observado en `origin/main` y en todas las ramas remotas vivas al
> redactarlo (0035 pertenece a I-50 en su rama; I-49, I-53 e I-54 declaran ADR sin número). Mientras este ADR sea
> `propuesto` puede renumerarse si otra rama integra antes con el mismo número (Proposal V1 §16).

## Contexto

RackCad no tiene un comando para reflejar racks. El `MIRROR` nativo de AutoCAD solo refleja la **referencia**: la
deja con escala negativa y no toca el diseño embebido en la definición (ADR-0009). Como toda autoridad lee ese diseño
—`RACKEDITAR` y Actualizar (ADR-0010), el BOM consolidado y las variables de proyecto (ADR-0034)— el resultado se
«desespeja»: los textos y cotas del bloque se ven al revés, Insertar agrega vistas sin espejo, los lados
Izquierda/Derecha del editor contradicen el dibujo y `RACKDUPLICAR`/`RACKLAYOUT` propagan la escala negativa. No hay
reactores que lo corrijan.

Un espejo que conserve la coherencia entre diseño y geometría tiene que reflejar el **diseño authored** y regenerar
la geometría desde él. Pero cada vista de RackCad es una **proyección con punto de vista fijo** de un rack descrito
en su marco local, y una misma línea de espejo en la hoja no significa lo mismo en todas: en una frontal refleja el
eje de frentes, en una lateral la profundidad y en una planta ambos, módulo un giro de 180°. Existe un
contraejemplo mínimo verificable —un Selectivo con frentes de claro distinto y una bota solo en la cara cercana, con
su frontal y su lateral seleccionadas— en el que ningún diseño reflejado único reproduce las dos vistas con una
colocación de determinante positivo. El diseño authored es uno por `RackId` (ADR-0034 §9), así que la reflexión
tiene que ser **una por rack lógico**.

El código ya ofrece las piezas para hacerlo sin infraestructura paralela: la agrupación multi-rack y la identidad por
`Guid` de I-51, `Transform2D` en Application (refleja sin `Matrix3d`), constructores puros de planes por vista, y la
costura de I-47 G9.1 que prepara planes e importa bloques antes de una única transacción del llamador.

## Decisión

1. **Espejo semántico, no de entidades.** `RACKMIRROR` refleja el documento authored y **regenera** la geometría con
   las mismas autoridades de plan que usa el redibujo. Nunca refleja entidades ni persiste una escala negativa.
2. **Solo copia.** Crea un rack nuevo y conserva los originales; no borra, no refleja en sitio y no conserva el
   `RackId` en la copia.
3. **Una reflexión semántica canónica por kind (`μ_k`)**, independiente de la línea y de las vistas seleccionadas.
   Primer corte: Selectivo = frentes (RUN), Dinámico y Push Back = frentes (RT), Cantilever = línea de estaciones
   (R_X), Cabecera = profundidad (R_D). La cama no tiene reflexión representable.
4. **Admisibilidad por vista.** Cada kind declara qué `(View, Section)` **exponen** `μ_k`: su plan reflejado
   coincide con el plan original transformado por una reflexión local `F` de la vista. Primer corte: frontal y
   planta para Selectivo, Dinámico, Push Back y Cantilever; lateral y planta para Cabecera; ninguna para Cama.
5. **Fail-closed.** Si una referencia seleccionada no es admisible, si un rack no es representable
   (`REQUIRES_MODEL_CHANGE`) o su representabilidad es desconocida (`UNKNOWN` material), la operación entera termina
   **antes** de mutar. No hay prompt de eje semántico.
6. **Reflectores puros en Application**, uno por kind y despachados por kind fuera del comando: documento authored
   del mismo kind en la entrada y en la salida, sin tipos de AutoCAD, sin manipulación genérica de JSON y sin
   `WithDesign`. Una normalización solo es válida si conserva exactamente la física y el BOM previos, las variables y
   los portadores, con round-trip demostrado.
7. **Autoridad pura de planes por vista en Application** y un **materializador genérico en el Plugin** que crea la
   definición dentro de la transacción del llamador, escribe el payload preparado y crea la referencia con la
   colocación ya calculada. El comando no conoce Left/Right, A/B, estaciones ni reglas de cabecera.
8. **Colocación canónica.** Con la reflexión de la hoja `G`, la colocación de la fuente `P` y la reflexión local de
   la vista `F`, la referencia nueva es `P' = G·P·F`, con determinante positivo y la escala uniforme de la fuente.
   Para una vista admitida, `P'·Π(μ_k D) = G·P·Π(D)`: la geometría física coincide con el espejo geométrico y los
   textos y cotas se regeneran legibles.
9. **Identidad nueva.** Un `NewRackId` por grupo lógico, compartido por sus vistas seleccionadas; el mapa
   `OldRackId → NewRackId` vive solo en memoria; el diseño se refleja **antes** de componer el sobre y re-estampar la
   identidad con la entrada única de I-51.
10. **Portadores intactos.** `SchemaVersion`, `ExtensionData`, `PropertyValues` (incluidos tipos desconocidos),
    literales congelados, `VariableId` y todo portador de iniciativas integradas (`DimensionViews`, propiedades de
    rack) viajan desde el origen. El espejo no crea, modifica, desvincula ni materializa variables y no escribe el
    registro; para dibujar consume en **solo lectura** la autoridad efectiva existente, y un registro ilegible o un
    vínculo roto fallan cerrado.
11. **Fuente canónica.** Solo `BlockReference` de Model Space, no MINSERT, con `Normal = +Z`, rotación finita y escala
    uniforme positiva (`(−s,−s)` se canoniza a `(s,s)` + π). Escala no uniforme, una sola componente negativa o
    `sz < 0` fallan cerrado. La línea se captura en el UCS con Z paralela a la de WCS y se convierte a WCS.
12. **Atomicidad.** `ACQUIRE → SNAPSHOT ALL → PREFLIGHT ALL → LINE → PREPARE ALL → MUTATE ALL → COMMIT`, con una sola
    transacción de escritura para todas las definiciones, payloads y referencias. La importación de definiciones
    auxiliares de biblioteca en PREPARE es la **única** mutación permitida fuera de esa transacción, y es de
    infraestructura, no del rack.

## Alternativas consideradas

- **Espejo de entidades o referencia con escala negativa** — descartada: el diseño embebido no cambia y todas las
  autoridades lo contradicen; los textos quedan al revés y la escala se propaga.
- **Espejo en sitio o prompt «¿Borrar objetos originales?»** — descartada en este corte: exige cubrir todas las
  vistas y referencias del `RackId` o produce hermanas divergentes, racks parciales y BOM ambiguo.
- **Inferir el eje por vista o preguntar Frentes/Fondos** — descartada: el diseño reflejado dependería de qué vistas
  se seleccionaron y un grupo frontal + lateral no tiene reflexión única.
- **Admitir las vistas que no exponen `μ_k` con su contenido sin voltear** (fidelidad física pero no gráfica) —
  descartada en el primer corte; queda como relajación futura con decisión del Owner.
- **Propiedad de vista «observada desde el lado opuesto» en el sobre** — descartada en I-52 por ser cambio de modelo;
  es la vía futura para reflejar laterales, la vista primaria del Dinámico y la cama.
- **Profundidad (DEPTH) como eje canónico del Selectivo** — descartada: invierte la convención «fondo 0 = frente» y
  concentra los casos no representables (topes, herencias, diagonales).
- **Reflector genérico sobre JSON o reflexión vía dominio con `WithDesign`** — descartadas: la primera no conoce la
  semántica; la segunda materializa literales vinculados.

## Consecuencias

- Positivas: la copia reflejada es un rack RackCad normal que sobrevive a guardar, reabrir, `RACKEDITAR` →
  Actualizar, Insertar, BOM y cambios de variable; el comando no dispersa casos por sistema; la agrupación de I-51 se
  reutiliza; Application no depende de AutoCAD.
- Negativas / costos aceptados: en el primer corte no se reflejan laterales de Selectivo, Dinámico, Push Back ni
  Cantilever, ni la cama, ni diseños no representables (esquinas, brazos C/L sencillos, ciertas ausencias de Push
  Back); un Dinámico dibujado solo en lateral no puede reflejarse; la equivalencia exige declarar o caracterizar la
  mano de los bloques DWG, que no están versionados; la importación de biblioteca puede sobrevivir a un fallo.
- Vigilar: cada kind o vista nueva debe declarar su reflector y su exposición; todo campo nuevo direccionado por
  poste, fondo, módulo, lado o estación necesita regla de reflexión antes de integrarse.

## Referencias

- ADR-0009 (identidad GUID), ADR-0010 (Actualizar/Insertar), ADR-0031 §8 (reflexión rígida del lado B; texto y cota
  solo se trasladan), ADR-0034 §9, §12 y §14.
- I-51: `RackDuplicationPlan`, `RackEnvelopeRestamp.RestampEnvelope(payload, name, Guid)`.
- I-47 G9.1: `ViewBlockDraw.PrepareRedraw`, `SystemBlockWriter.RedefineInTransaction`, `PreparedViewRedraw`,
  `ProjectVariableMutationExecutor`.
- `src/RackCad.Application/Geometry/Transform2D.cs`; `src/RackCad.Application/Systems/Selective/SelectiveEffectiveDesignResolver.cs:85`.
