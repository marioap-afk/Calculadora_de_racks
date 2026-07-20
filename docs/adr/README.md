# ADR — Architecture Decision Records

Registro de decisiones de arquitectura de RackCad. Cada ADR captura UNA decisión con su contexto,
para que nunca vuelva a re-litigarse sin saber por qué se tomó (el HANDOFF ya necesitaba avisos
"NO re-proponer": los ADR son la solución estructural a eso).

## Formato y numeración

- Archivo: `NNNN-slug-en-kebab.md` (4 dígitos, secuencial, **nunca se reutiliza un número**, ni
  siquiera el de un ADR rechazado).
- Plantilla: [plantilla.md](plantilla.md). Secciones: Estado, Contexto, Decisión, Alternativas
  consideradas, Consecuencias, Referencias.
- Estados: `propuesto` → `aceptado` | `rechazado`; un aceptado puede pasar a
  `reemplazado por ADR-NNNN` u `obsoleto`. **Solo el dueño del repo acepta o rechaza.**
  Los agentes pueden redactar ADRs en estado `propuesto`.

## Cuándo crear un ADR

Crear uno ANTES de implementar cuando la decisión cumpla al menos uno:

1. **Restringe trabajo futuro** en más de un módulo o capa (contratos, registros, formatos de
   persistencia, esquema de catálogos, unidades).
2. **Es cara de revertir** (formatos en disco/DWG, nombres públicos, dependencias).
3. **Cierra un debate recurrente** o una opción que alguien volvería a proponer (validación de
   cargas, SQLite, optimizador IA…).
4. **Es una excepción a una convención** de AGENTS.md (p. ej. permitir un paquete NuGet de build).

No crear ADR para: elecciones locales de implementación, nombres internos, decisiones reversibles
en una sesión. Esas van en comentarios de código o en el cuerpo del commit.

## Cuándo modificar / reemplazar

- Un ADR `aceptado` es **inmutable** en su contenido: solo cambian su Estado y sus enlaces. Se
  permiten correcciones tipográficas y una sección final "Notas posteriores" con fecha.
- Para cambiar la decisión: escribir un ADR nuevo que la reemplace y marcar el viejo como
  `reemplazado por ADR-NNNN`. **Nunca borrar un ADR**: la historia de decisiones es el valor.
- Un `propuesto` sí puede editarse libremente hasta que el dueño lo acepte o rechace.

## Índice

| # | Título | Estado |
|---|---|---|
| [0001](0001-ramas-por-iniciativa.md) | Ramas por iniciativa técnica, no por herramienta | aceptado |
| [0002](0002-secuencia-dinamico-modular.md) | Secuencia de integración de la rama del dinámico modular | aceptado |
| [0003](0003-referencias-autocad-para-ci.md) | Referencias AutoCAD para compilación en CI | propuesto |

Pendiente (iniciativa `docs/adr-retroactivos`): retro-documentar las ~13 decisiones vigentes de la
tabla de HANDOFF §7 (solo-Plugin-toca-AutoCAD, catálogos CSV Excel-first, GUID embebido, patrón
ARRAY, cero NuGet, parrilla una-por-tarima, cargas diferidas a RAM Elements…).
