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
| [0003](0003-referencias-autocad-para-ci.md) | Referencias AutoCAD para compilación en CI | aceptado |
| [0004](0004-estrategia-de-versiones-de-autocad.md) | Estrategia de versiones de AutoCAD | aceptado |
| [0005](0005-estrategia-de-unidades.md) | Estrategia de unidades | aceptado |
| [0006](0006-autocad-solo-en-plugin.md) | AutoCAD solo en RackCad.Plugin | aceptado |
| [0007](0007-catalogos-csv-excel-first.md) | Catálogos CSV Excel-first sin base de datos | aceptado |
| [0008](0008-secciones-unificadas-por-rol.md) | Perfiles estructurales unificados en secciones.csv por rol | aceptado |
| [0009](0009-identidad-guid-embebida-en-dwg.md) | Identidad de rack mediante GUID embebido en el DWG | aceptado |
| [0010](0010-actualizar-redibuja-insertar-liga-vistas.md) | Actualizar redibuja e Insertar agrega una vista ligada | aceptado |
| [0011](0011-parametros-dinamicos-con-patron-array.md) | Parámetros dinámicos mediante definiciones compartidas con patrón ARRAY | aceptado |
| [0012](0012-producto-sin-dependencias-nuget.md) | Código de producto sin dependencias NuGet | aceptado |
| [0013](0013-parrilla-una-por-tarima.md) | Parrilla una por tarima, contada en `SelectiveFrontalBuilder.ParrillaRow` | aceptado |
| [0014](0014-copia-central-seguridad-selectivo.md) | Copia centralizada de la selección de seguridad del selectivo en `DeepCopy` | aceptado |
| [0015](0015-entrada-numerica-localizada.md) | Entrada numérica localizada sin separador de miles | aceptado |
| [0016](0016-cantidad-parrilla-acotada.md) | Cantidad de parrilla acotada por la UI y por el builder | aceptado |
| [0017](0017-validacion-cargas-diferida-ram-elements.md) | Validación estructural de cargas diferida a RAM Elements | aceptado |
| [0018](0018-optimizador-layout-ia-diferido.md) | Optimizador de layout con IA diferido; `RACKLAYOUT` determinista vigente | aceptado |
| [0019](0019-shell-visual-de-editores-por-composicion.md) | Shell visual de editores por composición y slots, agnóstico al sistema | propuesto |

Iniciativa `docs/adr-retroactivos` (I-07): los ADR-0006…0018 retro-documentan las trece decisiones de la
antigua tabla de HANDOFF §7, una por ADR, y fueron **aceptados por el dueño el 2026-07-22** («Sí,
apruebo»; decisión versionada en
[`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md)). Con la integración de I-07 esas
decisiones dejan de conservarse en HANDOFF §7 y pasan a estos registros; la matriz decisión → ADR vive en
el [contrato de I-07](../initiatives/I-07-adr-retroactivos.md). El ADR-0002 histórico
(`0002-secuencia-dinamico-modular.md`) conserva su archivo de evidencia hermano
(`0002-paso0-evidencia.md`), no indexado por ser un apéndice de la misma decisión, no un ADR aparte. Los
ADR-0017 y 0018 registran diferimientos por decisión del dueño con respaldo documental; su limitación de
evidencia queda escrita en cada registro.
