# ADR-0050: Interoperabilidad de análisis estructural — modelo neutral derivado y adaptadores de proveedor

- **Estado:** propuesto (borrador de F0; no aceptado)
- **Fecha:** 2026-10-10 (borrador); 2026-10-11 (borrador final tras la sonda P1)
- **Decisores:** Owner del proyecto (acepta o rechaza; **pendiente**). Architect de I-65 (revisión pendiente sobre la versión
  congelable). Coordinator de I-65 (redacción).
- **Iniciativa relacionada:** I-65 — `architecture/interop-analisis-estructural`
  ([contrato](../initiatives/I-65-interop-analisis-estructural.md), [Discovery](../initiatives/I-65-discovery.md),
  [Proposal V3](../initiatives/I-65-proposal-v3.md)).
- **Consume sin reabrir:** [ADR-0017](0017-validacion-cargas-diferida-ram-elements.md). **Compatible con:**
  [ADR-0005](0005-estrategia-de-unidades.md), [ADR-0021](0021-identidad-unidades-y-presentacion-de-secciones.md) §10,
  [ADR-0040](0040-motor-expresiones-parametricas-y-guarda-de-complejidad-sintactica.md) D3,
  [ADR-0009](0009-identidad-guid-embebida-en-dwg.md).
- **Número:** siguiente libre en la base (`0047` y `0048` usados por ramas paralelas, `0049` en `main`); si otra iniciativa lo integra
  antes, se renumera en la integración.

## Contexto

ADR-0017 difiere la validación estructural de cargas a RAM Elements, pero RackCad no tiene ningún camino reproducible hacia RAM
Elements ni una representación analítica de sus racks. La geometría de RackCad es de producto (bloques, caras, puntos de inserción) y no
equivale a un modelo analítico; no hay material estructural ni cargas como hechos del producto; y la única interfaz programática de
RAM Elements 2026 es una API Python que corre solo dentro del programa, con documentación contradictoria en unidades y ejes
(Discovery de I-65).

## Decisión

1. **Modelo neutral derivado.** RackCad deriva, para un alcance acotado, un modelo estructural analítico neutral (nudos, miembros,
   secciones por `StructuralSectionId`, materiales por identidad neutral, apoyos, liberaciones, casos y cargas explícitas, supuestos y
   diagnósticos). Es **derivado, no persistido y regenerable**: nunca es autoridad de producto ni se guarda en el DWG.
2. **Autoridad de producto intacta.** RackCad/DWG sigue siendo la única autoridad de diseño. Ningún archivo ni resultado de un programa de
   análisis se lee como autoridad de RackCad.
3. **Unidades — cláusula de compatibilidad.** La pulgada sigue siendo la unidad interna canónica de producto, DWG, catálogos y
   persistencia (ADR-0005). El modelo de análisis almacena longitudes en pulgadas y fuerzas en libras-fuerza como cantidades tipadas por
   dimensión; las conversiones de longitud usan `LengthUnits` (ADR-0040 D3); las de datos del catálogo, `StructuralSectionUnits`
   (ADR-0021 §10); la masa de libra y la gravedad estándar tienen un propietario único nuevo en `Application/Units/`, que
   `StructuralSectionUnits` consume con resultado idéntico bit a bit (sin cambio de API ni del texto de ADR-0021). Cada proveedor
   convierte en un solo punto; una unidad desconocida falla cerrado.
4. **Frontera de proveedor.** Los adaptadores viven en una carpeta de proveedor de Application, solo con la biblioteca base; ningún tipo,
   identificador ni convención de un proveedor aparece fuera de esa carpeta (guardas de fuente). Un ensamblado propio solo si un
   proveedor trae una dependencia binaria.
5. **Supuestos explícitos.** Idealización, apoyos, conexiones, fuente de propiedades y cargas son supuestos con identificador, listados
   en el paquete exportado. Ninguno se elige en silencio. Sirven para una **equivalencia de transferencia**; no constituyen ni sustituyen
   una validación estructural de ingeniería, que queda fuera de RackCad.
6. **Sin exportación parcial.** El resultado es `Valid | Unsupported | Invalid | Unavailable`; cualquier diagnóstico bloqueante impide
   producir el paquete, que se escribe de forma atómica en una carpeta nueva.
7. **Primer proveedor: RAM Elements por su API Python oficial**, mediante un script de plantilla fija ejecutado por el ingeniero sobre un
   modelo vacío, que verifica por lectura de vuelta lo construido. No se automatiza la interfaz ni se usan formatos o servidores no
   documentados. Verificado en RAM Elements 26.00.01.015 (sonda P1): par de unidades `LB_IN` con lectura de vuelta exacta, eje vertical
   Y, regla de ejes locales y giro medidos; el análisis automático desde RackCad no existe y no se promete.
8. **Provenance.** Huella de la fuente y huella de entrada del análisis; un volcado o resultado cuya huella no coincide con la regenerada
   es `STALE` y no se presenta como vigente.
9. **ADR-0017 se mantiene.** RackCad no calcula utilización, ratios ni veredictos de diseño. Presentarlos en RackCad requeriría un ADR
   nuevo.

## Alternativas consideradas

- **Exportar la geometría dibujada tal cual:** rechazada; los puntos de bloque no son ejes analíticos.
- **OpenRE (XML) como transporte:** rechazada para V1; manual de 2024 incompleto, sin esquema ni semántica de importación.
- **ISM / iTwin:** rechazada para V1; sin SDK oficial documentado para terceros y con dependencia binaria.
- **Automatizar la interfaz de RAM Elements:** rechazada; existe una API soportada.
- **Almacenar el modelo de análisis en SI:** viable; descartada para no introducir una segunda unidad interna de geometría frente a
  ADR-0005.

## Consecuencias

- Positivas: camino reproducible hacia RAM Elements; frontera de unidades única; supuestos auditables; base para otros proveedores e
  iniciativas (ID30 como UI, I-38 como consumidor de ingeniería, mezzanine futuro).
- Negativas / costos aceptados: cada transferencia exige un paso humano dentro de RAM Elements; la API de RAM es 0.1.0 y puede cambiar
  (puerta de capacidades); las propiedades analizadas pueden venir de la base de datos de RAM (supuesto declarado); V1 cubre una sola
  rebanada.

## Referencias

- [Mandato de I-65](../automation/decisions/I-65-owner-mandate.original.txt)
- [Discovery de I-65](../initiatives/I-65-discovery.md), [Proposal V3](../initiatives/I-65-proposal-v3.md)
- [ADR-0005](0005-estrategia-de-unidades.md), [ADR-0009](0009-identidad-guid-embebida-en-dwg.md),
  [ADR-0017](0017-validacion-cargas-diferida-ram-elements.md),
  [ADR-0021](0021-identidad-unidades-y-presentacion-de-secciones.md),
  [ADR-0040](0040-motor-expresiones-parametricas-y-guarda-de-complejidad-sintactica.md)
