# ADR-0037: Reutilizar una configuración de cabecera es copiarla y distribuirla por lotes: destinos por sistema, preparación todo-o-nada y autoridades de normalización propias

- **Estado:** aceptado
- **Fecha:** 2026-09-12 (propuesto) · 2026-09-12 (aceptado)
- **Decisores:** Mario Pérez, Owner del repositorio (**acepta**); Coordinador de I-53 (órdenes de G2B y G2-F,
  que transmiten las decisiones del Owner OD-2.b, OD-6 y OD-8); Arquitecto de I-53 (revisión de la Proposal V1:
  **AGREED WITH CHANGES**; re-revisión de la Proposal V2: **AGREED**, con PA-1B, N-01, RR-01 y las tres
  correcciones de este registro): consenso técnico sobre Proposal **V2** / SHA
  `d7f17addb47ea943b61ff50b4b4a5b23010d6fab` (rebasada sin cambios de contenido a
  `d0db698facf2297d6bd5aaa512f3b3b3f2358a59`); Claude (redacción)
- **Iniciativa relacionada:** I-53 — `feature/cabeceras-configurables-multidestino`
  ([contrato](../initiatives/I-53-cabeceras-configurables-multidestino.md),
  [Discovery](../initiatives/I-53-discovery.md), [Proposal V2](../initiatives/I-53-proposal-v2.md), congelada;
  [Proposal V1](../initiatives/I-53-proposal-v1.md) como registro de su primera ronda;
  [decisiones](../automation/decisions/I-53.md))

> **Aceptación del Owner (2026-09-12).** El Owner acepta esta decisión de forma **explícita**: «**Acepto el
> ADR**», dicho en el canal del Coordinador de I-53 y referido inequívocamente a ADR-0037 de I-53. Registro
> durable en [`docs/automation/decisions/I-53.md`](../automation/decisions/I-53.md).
>
> **Dos actos, dos autoridades.** El Coordinador y el Arquitecto aportaron el **consenso técnico** sobre la
> Proposal V2; aceptar es un acto distinto que **solo el dueño del repo** ejerce ([README](README.md)). Este
> registro **nació `propuesto`** el 2026-09-12 y pasa a **`aceptado`** ese mismo día.
>
> **Contenido aceptado.** Es el de este archivo en el commit de freeze de G2 de I-53. Respecto del texto
> `propuesto`, ese commit solo incorpora las **tres correcciones** que la re-revisión del Arquitecto exigió antes
> de aceptar —la redacción de la alternativa de disciplina, la lectura de estado comprometido y resolución
> vigente en la decisión 5, y la vigencia de la nota de numeración hasta `main`— y actualiza este encabezado. No
> cambia ninguna otra decisión. Desde ahora el contenido es **inmutable** ([README](README.md)).
>
> **Lo que esta aceptación autoriza — y lo que no.** Junto con el freeze de G2 cumple la compuerta de código
> productivo de I-53: **G3** puede abrirse en una sesión posterior. **No** implementa nada, **no** abre G3 por sí
> misma y **no** decide la forma de las entregas de I-53, que vive en su contrato.

> **Numeración.** 0037 es el primer número libre observado en `origin/main` y en todas las ramas remotas vivas al
> redactarlo (0035 pertenece a I-50, ya integrada en `main`; 0036, a I-52 en su rama; I-49 e I-54 declaran ADR sin
> número; ninguna rama cita ADR-0037). **Hasta que este registro llegue a `main`** se comprueba el número en cada
> preflight de I-53; si otra rama integra antes un ADR-0037, este se renumera cambiando solo su número y sus
> enlaces, sin tocar el contenido aceptado.

## Contexto

Tres sistemas de RackCad dibujan cabeceras configurables con el mismo configurador compartido, y cada uno resolvió
por su cuenta qué significa aplicar una configuración a más de una cabecera. Push Back (I-40) copia una cabecera
origen a un conjunto de destinos —cabeceras por líneas— dentro de una sesión escenificada con Confirmar y Cancelar.
El Selectivo (I-43, [ADR-0032](0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md)) aplica el
resultado del configurador a un poste en varios fondos, con una copia por fondo, pero sin un origen explícito y
escribiendo el peralte del poste antes de saber si algo se aplica. El Dinámico abre el configurador sobre la
instancia viva, no distribuye a varios destinos y pierde en silencio sus personalizaciones cuando una reconstrucción
rehace la secuencia de módulos.

El Owner pidió (ID6 e ID7 de su numeración) tomar una configuración de cabecera **existente** como origen —copiando
sus valores authored, sin vínculo vivo— y aplicarla a un **conjunto** de destinos compatibles, con origen y destinos
separados y una taxonomía de destinos propia de cada sistema. Para el Dinámico aprobó además que la reconstrucción
reconcilie las personalizaciones en vez de perderlas.

Cerrar esto sin un registro deja abiertos debates que ya se repitieron entre I-40, I-43 e I-53: un identificador
universal de destino; copiar la sesión de Push Back en otros sistemas; capturar el origen al elegirlo o al aplicarlo;
dónde vive la normalización de profundidad y peralte; qué significa `IsManualOverride` en el Dinámico, y cómo se
evita escribir la mitad de un lote.

## Decisión

1. **Reutilizar es copiar.** Una configuración de cabecera se reutiliza copiando sus valores authored. No hay vínculo
   vivo, instancia compartida ni plantilla: después de aplicar, origen y destinos son independientes y editar
   cualquiera no cambia a los demás. Un vínculo vivo o una plantilla con propagación exigirán un ADR nuevo.
2. **Origen y destinos son conceptos separados.** El origen de una distribución es siempre una cabecera existente,
   designada por su **dirección** y **capturada al aplicar**, con su valor vigente. No existe portapapeles de
   configuraciones. La edición puntual es el caso degenerado —su origen es el resultado del configurador— y usa el
   mismo protocolo.
3. **No hay destino universal.** Cada sistema expresa sus destinos en su propia taxonomía: el Selectivo por
   `(fondo, poste)` y el Dinámico por módulo longitudinal (`ModuleId`, conforme a la decisión del Owner de I-35). Un
   destino que la taxonomía expresa pero la topología vigente no tiene se **omite** y se informa; nunca se crea ni se
   ajusta a un vecino.
4. **La materialización es canónica.** Cada destino recibe una copia propia producida por la copia canónica única de
   la configuración (I-17): el modelo derivado se reconstruye y nunca se comparte, las excepciones de runtime siguen
   la política de esa copia y no se crea serialización nueva. La copia privada del origen nace dentro de la
   preparación, no se expone y muere con la operación.
5. **La preparación es todo-o-nada.** Todo lo que puede fallar ocurre antes de escribir y sin mutación observable:
   resolver y capturar el origen, resolver los destinos, validarlos, materializar, normalizar y validar las copias.
   La preparación lee **únicamente** estado comprometido y su **resolución vigente**, sin recompute pendiente ni
   diferido que pueda invalidar esa resolución; si esa condición no se cumple, el gesto termina antes de preparar. El
   plan vive un solo gesto y lleva una firma de la topología y de la resolución que leyó; la mutación solo asigna,
   verifica esa firma antes de la primera asignación y, si no coincide, no escribe nada. La frontera previa de
   edición de cada editor (en el Selectivo, ADR-0032 D6) es anterior y externa al lote.
6. **Omitido y rechazado son tipados y distintos.** *Omitido*: la dirección es válida en la taxonomía, pero no hay
   instancia aplicable (`AbsentInScope`, `NotPhysicallyPresent`, `IsSource`); no invalida a los demás destinos.
   *Rechazado*: la operación entera es inválida (`SourceNotFound`, `SourceUnusable`, `NoTargets`, `MalformedTarget`,
   `StaleTargets`, `DestinationInvalid`, `NoApplicableTargets`), con precedencia determinista y mutación cero. Un solo
   destino aplicable inválido rechaza el lote completo.
7. **El resultado es cerrado.** La preparación produce `Rejected` o `Prepared`, sin nada aplicado. El resultado
   final es `Rejected`, `Cancelled` o `Committed`, y la lista de destinos aplicados solo existe en `Committed`. El orden
   de todo informe es determinista.
8. **Un recompute por operación confirmada.** Exactamente uno tras `Committed`; ninguno tras `Rejected` o
   `Cancelled`.
9. **Cada sistema normaliza con sus propias autoridades, sin duplicarlas.** La altura viaja con la receta. En el
   Selectivo, la profundidad la impone el fondo (ADR-0032 D9/D10) y el peralte de la copia es el efectivo del poste
   destino según la autoridad única por poste. En el Dinámico, profundidad y peralte los impone el recompute canónico
   del constructor del sistema, y la preparación solo verifica precondiciones.
10. **En el Dinámico, `IsManualOverride` significa longitud manual.** La personalización de una cabecera se expresa
    solo con su procedencia (`UseCalculatedHeaderConfiguration = false`), y copiar una configuración no fija longitud.
    Como los identificadores de módulo son posicionales, toda reconstrucción invalida el origen y los destinos
    recordados, y reconcilia las personalizaciones por `ModuleId + Kind` informando lo conservado, lo adaptado y lo
    perdido (I-35, decisiones 2 a 4).
11. **Push Back conserva I-40 y no se migra.** El contrato común no lo representa ni lo consume; su comportamiento
    histórico no cambia.

## Alternativas consideradas

- **Identificador universal de destino** — inventaría ejes que un sistema no tiene (un poste en el Dinámico) y
  mezclaría taxonomías que deben seguir separadas; descartado.
- **Adoptar la sesión escenificada de Push Back en el Dinámico** — su aplicación de cabecera fija la longitud manual
  y congelaría el fondo de cada destino en la siguiente reconstrucción; además exige un ámbito sucio
  ([ADR-0029](0029-contrato-funcional-comun-de-ventanas-wpf.md) D8) y un ciclo Confirmar/Cancelar que nadie pidió;
  descartado.
- **Disciplina sobre la copia canónica, sin copia privada encapsulada** — la separación entre origen y destino
  quedaría en la convención. El Discovery de I-53 registró cuatro sitios donde instancias de configuración se
  comparten por referencia y confirmó al menos un defecto de comportamiento real (L-1: el configurador del Dinámico
  edita la instancia viva); descartado.
- **Capturar el origen al elegirlo** — crea un portapapeles invisible que aplica una versión obsoleta si el origen se
  edita después; descartado.
- **Una política de cobertura común con escritura inerte** — describiría a Push Back en un código que Push Back no
  consume; descartado.
- **Normalizar en la preparación del Dinámico** — duplicaría las reglas del constructor del sistema; descartado.
- **Unidad por instancia física en el Dinámico** — divide las vistas, no está pedida y heredaría el defecto del BOM
  consolidado de los overrides por línea; descartada.

## Consecuencias

- Positivas:
  - una sola semántica de copia y de atomicidad para editar y para distribuir;
  - cero escritura parcial, incluido el peralte del Selectivo;
  - las pérdidas de personalización del Dinámico dejan de ser silenciosas;
  - un único mecanismo de reutilización por sistema.
- Negativas / costos aceptados:
  - unos tipos transitorios comunes que deben mantenerse neutrales y pequeños;
  - la reconstrucción del Dinámico cambia de comportamiento (aprobado por el Owner);
  - Push Back y el contrato común conviven con semánticas distintas por diseño;
  - tras una reconstrucción del Dinámico hay que volver a elegir origen y destinos.
- Vigilar:
  - que el contrato común no crezca hacia un ejecutor genérico;
  - que ninguna ruta materialice con una copia no canónica sin refresco del derivado;
  - que el número de este ADR no colisione hasta que llegue a `main`.

## Fuera de este ADR

Detalles de XAML y controles; los nombres concretos de los tipos; las reglas del Selectivo ya fijadas por ADR-0032;
el calendario y el esquema de entregas de I-53.

## Referencias

- I-53: [contrato](../initiatives/I-53-cabeceras-configurables-multidestino.md),
  [Discovery](../initiatives/I-53-discovery.md), [Proposal V1](../initiatives/I-53-proposal-v1.md),
  [Proposal V2](../initiatives/I-53-proposal-v2.md) y [registro de decisiones](../automation/decisions/I-53.md).
- [ADR-0029](0029-contrato-funcional-comun-de-ventanas-wpf.md) (D8) y
  [ADR-0032](0032-selectivo-pendiente-comprometido-y-autoridades-por-fondo.md) (D1, D6, D9, D10, D12).
- I-17 (copia canónica única), I-35 (reconciliación por `ModuleId + Kind`, decisiones 1 a 5), I-40 (cabeceras de Push
  Back) e I-43 (Selectivo por fondos).
- Código de referencia: `RackFrameProjectStore.DeepCopy`, `SelectiveCabeceraAuthority`,
  `SelectivePostGeometry.PostPeralteAt`, `DynamicRackSystemBuilder.Refresh` y `ApplyPostPeralte`,
  `RackModuleReconciliation`.
