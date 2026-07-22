# ADR-0005: Estrategia de unidades

- **Estado:** aceptado
- **Fecha:** 2026-07-22 (propuesta y aceptación)
- **Decisores:** Mario Pérez, dueño del repositorio (aceptó); redactado por Claude (I-05)
- **Iniciativa relacionada:** I-05 (`feature/guardrail-unidades`)

## Contexto

RackCad genera TODA su geometría en **pulgadas**. Los builders de dominio/aplicación, los valores por
defecto de `defaults.json`, los catálogos (`assets/catalogs/*.csv`) y los jigs de colocación producen y
consumen números que se interpretan como pulgadas; no existe ninguna conversión de unidades en ninguna
capa. La columna `units` de los catálogos es **decorativa**: nada la lee para convertir.

Un dibujo de AutoCAD declara sus unidades en la variable de sistema `INSUNITS` (código entero DXF:
0 = sin unidad/unitless, 1 = pulgadas, 4 = milímetros, 6 = metros, …). Hasta esta iniciativa, el Plugin
**jamás** leía `INSUNITS`. Cuando el usuario inserta un rack sobre un plano cuyas unidades no son
pulgadas (por ejemplo milímetros), la geometría en pulgadas aterriza a escala de pulgadas y queda
desproporcionada frente al dibujo —un factor de 25.4 sobre un plano métrico— **sin ninguna señal**. La
auditoría 2026-07 registró esto como el hallazgo **D4** (severidad ALTA) y recomendó, a corto plazo, una
validación que lea `INSUNITS` y avise, y a largo plazo, decidir si la columna `units` se honra con
conversión real. El ROADMAP lo adelantó a la Fase 1 como I-05.

Esta decisión restringe trabajo futuro en más de una capa (Plugin, catálogos, persistencia, builders) y
es cara de revertir si se elige mal (una conversión automática silenciosa reinterpretaría dibujos
existentes). Por eso se documenta como ADR antes de implementar (criterio 1 y 3 de `adr/README.md`:
unidades y decisión recurrente).

## Decisión

1. **La pulgada es la unidad interna canónica de RackCad, hoy y en el horizonte de este ADR.** Toda la
   geometría, catálogos, defaults, BOM y persistencia se expresan en pulgadas. No se introduce una unidad
   interna configurable.

2. **I-05 añade una guardia visible y NO bloqueante en el único límite que conoce las unidades del DWG: el
   Plugin.** Al insertar un sistema o una vista nueva, y en `RACKLAYOUT`/`RACKRELLENAR`, el Plugin lee
   `INSUNITS` del `Database` activo y, si el dibujo no está en pulgadas (incluido unitless), escribe **una
   sola advertencia** en la línea de comandos **antes de la primera modificación del DWG**. La advertencia
   no aborta el comando, no pregunta y no se repite por alias ni por vista/bloque de una misma operación.
   Una **actualización pura** con `RACKEDITAR` (redibujar en sitio geometría que ya existe a su escala
   actual) **no** advierte; **insertar una vista nueva** sí.

3. **I-05 NO convierte, reescala ni reinterpreta nada.** No toca coordenadas, geometría, bloques,
   catálogos, la columna `units`, los DTO ni la persistencia. La guardia solo lee `INSUNITS` y, a lo sumo,
   escribe un aviso. El conocimiento de la API de AutoCAD (el tipo `UnitsValue`, el acceso al `Database`)
   se queda en el Plugin; la decisión pura «¿esta unidad amerita aviso?» vive en `RackCad.Application`
   sobre una categoría neutral (`DrawingUnits`), sin dependencia de AutoCAD y sin ser un sistema de
   unidades.

4. **La estrategia futura, si el dueño decide honrar unidades distintas de la pulgada, será una frontera
   explícita entre las unidades del DWG y las unidades internas**, NO una reinterpretación silenciosa. Esa
   frontera —convertir en el límite de AutoCAD al insertar/editar (pulgadas ↔ unidad del dibujo) mientras
   el núcleo sigue en pulgadas— se diseña e implementa en una **iniciativa futura propia**, gobernada por
   un ADR que reemplace o extienda a éste. I-05 no la implementa ni la presupone en el código.

## Alternativas consideradas

- **Abortar la inserción si el dibujo no está en pulgadas** — la auditoría mencionaba «avisar/abortar».
  Se descarta abortar: bloquearía flujos de trabajo legítimos (un dibujo unitless que el usuario sabe
  interpretar, o un plano métrico donde el usuario acepta conscientemente la escala) y convertiría una
  ayuda barata en un obstáculo. La advertencia no bloqueante informa sin imponer.

- **Conversión automática al insertar (escalar la geometría por el factor pulgada→unidad del dibujo)** —
  es la solución «correcta» a largo plazo, pero es cara y arriesgada: exige decidir el redondeo, el
  comportamiento del round-trip (editar un rack convertido), el efecto sobre bloques dinámicos y sobre el
  BOM, y reinterpreta dibujos existentes. Es una iniciativa mayor con su propio ADR; hacerla «de paso» en
  I-05 rompería el principio de alcance acotado. Se difiere.

- **Honrar la columna `units` de los catálogos con conversión por fila** — mezcla dos problemas (unidad
  del dibujo vs. unidad de los datos de catálogo) y multiplica el radio de cambio sin demanda actual. Se
  difiere junto con la conversión real.

- **No hacer nada (seguir sin leer `INSUNITS`)** — mantiene el error de escala silencioso de D4. Rechazado:
  el costo de la guardia es mínimo y el beneficio (evitar dibujos a escala 25.4× errónea) es alto.

## Consecuencias

- **Positivas:** el usuario recibe una señal temprana y barata cuando el dibujo no está en pulgadas, antes
  de que el rack se coloque a escala equivocada; no se altera ningún dibujo existente (cero riesgo de
  regresión geométrica); la dirección de dependencias se conserva (Application no aprende AutoCAD); la
  puerta a una conversión real queda abierta y documentada, no cerrada.

- **Negativas / costos aceptados:** la advertencia es solo informativa —un usuario puede ignorarla e
  insertar igual a escala de pulgadas sobre un plano métrico (comportamiento intencional: no bloqueamos)—;
  la columna `units` sigue decorativa; RackCad sigue siendo mono-unidad hasta que una iniciativa futura
  implemente la frontera. Hay que vigilar que ninguna iniciativa posterior confunda «leer `INSUNITS` para
  avisar» (esto) con «convertir» (lo diferido).

## Condiciones para una futura iniciativa de conversión real

Una iniciativa que implemente conversión real debería, como mínimo:

1. Definir la frontera exacta (dónde se convierte pulgadas↔unidad del dibujo: al insertar, al editar, al
   leer/escribir el payload) y garantizar que el núcleo permanece en pulgadas.
2. Especificar el round-trip: editar un rack insertado en un dibujo métrico debe reproducir la misma
   geometría; el GUID, el payload/Xrecord y el BOM deben permanecer coherentes.
3. Decidir el trato de la columna `units` de los catálogos (honrarla o retirarla) y de los dibujos
   unitless.
4. Cubrirse con pruebas de equivalencia y validación manual en AutoCAD sobre dibujos en varias unidades.
5. Reemplazar o extender este ADR (un `aceptado` es inmutable en su Decisión; el cambio se hace con un ADR
   nuevo que lo referencie).

## Referencias

- I-05 (`feature/guardrail-unidades`): guardia de unidades y este ADR.
- [auditoría 2026-07](../auditoria-arquitectura-2026-07.md): hallazgo **D4** (mono-unidad implícita) y la
  sección «Unidades» (ADR + validación dura a corto plazo, conversión a largo plazo).
- `docs/ROADMAP.md`: fila I-05 y tabla de revalidación #5 («Guardrail INSUNITS»).
- [ADR-0003](0003-referencias-autocad-para-ci.md): el Plugin es el único que toca la API de AutoCAD (por
  qué la lectura de `INSUNITS` vive ahí y la decisión pura en Application).
- Código: `src/RackCad.Plugin/RackUnitsGuard.cs` (lectura de `INSUNITS` + aviso),
  `src/RackCad.Application/Drawing/DrawingUnitsAdvisory.cs` (decisión pura, sin AutoCAD).

## Notas posteriores

- **2026-07-22 — Aceptado por Mario Pérez**, dueño del repositorio («Sí, estoy de acuerdo»). El dueño
  aceptó los cuatro puntos de la sección Decisión **sin solicitar modificaciones**: la pulgada
  permanece como unidad interna canónica; la guardia es un aviso visible y **NO bloqueante**; I-05
  **no** realiza conversión, reescalado ni reinterpretación; y una conversión real futura exigirá una
  frontera explícita DWG↔interno en una iniciativa propia con su ADR. La aceptación **no** autoriza
  conversión, reescalado ni ampliación de alcance. El contenido de la sección Decisión no cambia (ADR
  aceptado inmutable). Decisión versionada del dueño en
  [`docs/automation/decisions/I-05.md`](../automation/decisions/I-05.md).
