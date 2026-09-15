# ADR-XXXX (número pendiente): Fundación compartida de vistas — hechos y contratos neutrales de direcciones, marcos, resolución y plan

- **Estado:** propuesto (borrador)
- **Número:** **PENDIENTE.** No figura en el [índice de ADR](../../adr/README.md); se asigna al congelar, tras censar los números
  tomados (censo en la [especificación](specification.md), cabecera y §10).
- **Fecha:** 2026-09-14 (borrador)
- **Decisores:** pendiente. Solo el Owner acepta o rechaza. Sin consenso técnico. Claude (borrador inicial); Codex (reconciliación del takeover)
- **Documentos:** [especificación](specification.md), [reconciliación](reconciliation.md), [mapa de entrega](delivery-map.md)
- **No reemplaza a ninguna ADR.** Lo referencia, sin duplicarlo, [ADR-0042](../../adr/0042-preparacion-de-vistas-antes-de-materializar.md)
  (producto de vistas). Se propone que ADR-0036 (producto de espejo en I-52) lo referencie; V17 aun no adopta esta dependencia

> **Estado de este registro.** Borrador `propuesto`, sin número y sin iniciativa. No autoriza implementación. Su aceptación **no**
> requiere decisiones de producto de vistas (colocación de grupo, cola de varias vistas, primera vista libre), ni la aceptación de
> ADR-0042, ni decisiones del Owner propias del espejo semántico.

## Contexto

RackCad identifica un rack lógico por el GUID del sobre de sus definiciones de bloque (ADR-0009); cada definición es una vista cuyo tipo y
representación se codifican con los tokens `View` y `Section`. Hoy esa codificación se interpreta por separado en cada comando de edición,
en la mutación de variables de proyecto y en el listado; resolver un rack sin editor es privado de cada manejador del BOM; el plan de una
vista se arma dentro de los servicios de dibujo; los nombres base de las definiciones se calculan en funciones privadas del Plugin; y el
origen y la extensión física de cada vista solo existen implícitos en los builders.

Dos capacidades de producto necesitan exactamente esos hechos: generar vistas de uno o varios racks conservando su identidad, y reflejar
racks por copia semántica. Si cada una los extrajera por su cuenta habría dos autoridades para el mismo hecho, o una tendría que
consumir código que la otra aún no integró.

## Decisión

1. **Una sola taxonomía de tipo de vista** (`Frontal`, `Lateral`, `Planta`), que renombra el tipo usado por la política de cotas sin
   cambiar esa política (ADR-0035). Los tokens persistidos y los enums de cámara (del Cantilever o de las secciones estructurales) son
   codificaciones, no taxonomías. **La persistencia no cambia.**
2. **Una dirección de vista tipada** = tipo + variante semántica (entera, fondo, poste, extremo de flujo, corte de Push Back, estación),
   con igualdad por valor y nunca un índice de interfaz.
3. **Tres niveles separados.**
   - Un **codec sintáctico total** traduce `View` y `Section` a una dirección y a una disposición (`Canonical`, `Canonicalizable`,
     `Coerced`, `Invalid`) sin consultar el sistema resuelto. Todo valor posible cae en exactamente una regla.
   - Los **hechos de disponibilidad** dicen si el sistema resuelto tiene esa dirección (`Available`, variante ausente, tipo no soportado
     por el sistema, no disponible).
   - La **política** (aceptar, omitir, avisar, fallar, variante canónica, remedio) es de cada consumidor y no forma parte de esta
     fundación.
4. **Un marco por vista** con los ejes físicos y su signo, el origen, el desplazamiento local de la variante, el tramo `[K_min, K_max]`
   por variante y su centro. Los datos físicos son únicos; cada consumidor elige mínimo, máximo o centro. El centro es un dato del
   descriptor, no una forma afín, y no absorbe diferencias con la simetría geométrica.
5. **Un contrato de resolución sin editor** que entrega el **sistema efectivo junto con sus diagnósticos**, sin perder el sistema por un
   diagnóstico bloqueante, con una lista cerrada de motivos de no resolución en orden de precedencia y una lista cerrada de formas legacy
   con predicados deterministas.
   - **Se implementa con adaptadores por tipo de sistema dentro de la autoridad vigente de cada manejador.** La resolución efectiva del
     selectivo sigue corriendo una vez por rack dentro de su manejador, como fija ADR-0034; los demás consumidores llegan a esa misma
     autoridad a través del contrato. **ADR-0034 no se modifica.**
6. **Una sola autoridad de plan** por dirección, que delega en los builders vigentes y devuelve plan, marco, requisitos de bloque y nombre
   base sugerido. Ningún otro componente construye planes; lo que no migre de inmediato queda registrado como productor legacy con
   caracterización, prueba de paridad y retirada.
7. **Una sola autoridad de nombre base** de las definiciones de vista, que reproduce cadena a cadena los nombres vigentes; el nombre único
   en el dibujo sigue decidiéndose al materializar. Las claves de biblioteca de bloques no son nombres generados y nunca se sanean.
8. **Un requisito estructural de bloques por pieza del plan**, discriminado por el rol de la pieza, con clave válida si no es nula ni está
   en blanco y existencia decidida por la consulta tras importar.
9. **Un núcleo neutral de selección y un valor de colocación** sobre la transformación 2D existente, con la descomposición de la
   transformación fuente como hechos (clase de escala, reflexión, normal) y una única tolerancia de escala registrada; qué clases acepta
   cada consumidor es su política.
10. **Un comparador authored por tipo de sistema**, con los miembros aportados por el consumidor.
11. **Sin cambio observable** al adoptar esta fundación: dibujo, BOM, listado, nombres, mensajes y persistencia quedan idénticos, fijados
    por caracterizaciones previas con un único dueño cada una.

## Alternativas consideradas

- **Dejar que cada capacidad de producto defina estos hechos por su cuenta:** dos autoridades para el mismo hecho, que pueden divergir.
- **Definirlos dentro de una capacidad de producto:** su aceptación quedaría atada a decisiones de producto que no afectan a estos hechos.
- **Codec que consulta el sistema resuelto o disponibilidad que contiene la política de un consumidor:** mezcla responsabilidades e impide
  compartirlas.
- **Resultado de resolución sin sistema cuando hay un diagnóstico bloqueante:** obliga a cada consumidor a resolver de nuevo para decidir
  su política.
- **Mover la resolución efectiva del selectivo a un servicio neutral:** cambiaría una decisión aceptada (ADR-0034) sin necesidad técnica.
- **Plan neutral nuevo junto a los generadores del Plugin:** dos generadores equivalentes que pueden divergir.
- **Semilla de nombre nueva junto a las funciones de nombre del Plugin:** dos autoridades de nombre.
- **Validar claves de catálogo con el saneador de nombres de definición:** rechaza claves reales con punto.

## Consecuencias

**Positivas.** Un solo sitio para cada hecho de vista; consumidores de producto que aportan solo política; caracterizaciones con un dueño;
pruebas puras sin AutoCAD; ninguna migración de dibujos.

**Negativas / costos aceptados.** Una refactorización transversal sin cambio observable (comandos de edición, manejadores del BOM,
servicios de dibujo, renombre de un tipo en editores), con guardas reapuntadas con motivo; las capacidades de producto que consumen
estos hechos solo pueden hacerlo sobre la fundación ya integrada.

## Relación con otros ADR

- [ADR-0009](../../adr/0009-identidad-guid-embebida-en-dwg.md): sin cambio.
- [ADR-0010](../../adr/0010-actualizar-redibuja-insertar-liga-vistas.md): sin cambio.
- [ADR-0034](../../adr/0034-project-variables-autoridad-drawing-level.md): **se conserva**; §6 y §10 se cumplen literalmente.
- [ADR-0035](../../adr/0035-visibilidad-de-cotas-por-tipo-de-vista.md): sin cambio de política; el tipo cambia de nombre.
- [ADR-0039](../../adr/0039-custom-properties-persistencia-autoridad.md): sin cambio.
- [ADR-0042](../../adr/0042-preparacion-de-vistas-antes-de-materializar.md) (propuesto): lo referencia como autoridad de los hechos que
  consume.
- ADR-0036 (propuesto, espejo semántico): se propone que lo referencie en lugar de declarar esas autoridades.
