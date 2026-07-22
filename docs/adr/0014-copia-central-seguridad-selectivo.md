# ADR-0014: Copia centralizada de la selección de seguridad del selectivo en `DeepCopy`

- **Estado:** aceptado
- **Fecha:** 2026-07-22 (documentación retroactiva y aceptación; no es la fecha de la decisión original)
- **Decisores:** Mario Pérez, dueño del repositorio (aceptó el registro el 2026-07-22). Redacción retroactiva bajo la iniciativa I-07. La evidencia conservada no identifica a los decisores de la decisión histórica original.
- **Iniciativa relacionada:** I-07 — ADRs retroactivos (`docs/adr-retroactivos`)

## Contexto

El sistema selectivo persiste su configuración de seguridad (botas, protectores laterales, separadores,
topes, desviadores, guías, parrillas y tarimas) en `SelectiveSafetySelection`. Esa selección se clona al
duplicar y editar diseños, se serializa a disco y se consume desde el resolver, la vista por fondo y la
UI. Un campo o familia de seguridad que se agregue al modelo pero se olvide en la clonación o en la
persistencia rompe en silencio: el clon o el documento reabierto pierde el dato sin error visible.

Para evitarlo, RackCad concentra la clonación en un único `SelectiveSafetySelection.DeepCopy` y exige el
mapeo explícito en los documentos de persistencia. Tras I-22, cada familia de seguridad posee una
configuración sellada con su propio `DeepCopy` y su documento por subtipo, y el `DeepCopy` central los
compone; el punto único de copia se conserva.

Este ADR documenta retroactivamente una convención ya vigente. El estado `propuesto` indica que el
registro espera revisión formal del propietario; no reabre la convención. La evidencia conservada
demuestra el punto único de copia y el mapeo de persistencia, pero no permite fijar de forma fiable la
fecha original ni reconstruir todas las alternativas evaluadas.

## Decisión

Clonar toda la selección de seguridad del selectivo en un único `SelectiveSafetySelection.DeepCopy`. Todo
campo o familia nuevo:

- se copia en `DeepCopy` —tras I-22, agregando el `DeepCopy` de la configuración de su familia, que el
  central compone, en lugar de otro campo suelto—;
- se mapea explícitamente en los documentos de persistencia (`SelectivePalletDesignDocument` y los
  documentos por subtipo de `SafetySelectionDocuments`), con fallback legacy en el DTO nullable;
- se cubre con una prueba de round-trip.

El resolver, la vista por fondo y la UI consumen esa copia; ninguno mantiene su propia clonación. Omitir
cualquiera de esos límites —copia o persistencia— rompe en silencio y no está permitido.

## Alternativas consideradas

- **Copiar la selección campo por campo en cada punto que la clona** — descartada porque multiplica los
  sitios que un campo nuevo debe tocar y hace que un olvido pierda datos sin error visible.
- **Depender solo de la serialización para clonar** — insuficiente por sí sola para el estado que el
  documento no persiste; el punto único de `DeepCopy` es el lugar donde se garantiza el clon completo.

La evidencia conservada no permite reconstruir de forma fiable otras alternativas evaluadas cuando se
adoptó la convención.

## Consecuencias

- Positivas: agregar una familia de seguridad tiene un contrato claro (copia + persistencia + round-trip);
  el clon y el documento reabierto conservan la selección completa; el DTO permanece explícito y
  compatible con documentos viejos.
- Negativas / costos aceptados: cada familia nueva debe tocar dos límites (copia y persistencia) y su
  prueba; el `DeepCopy` central y los documentos por subtipo crecen con el catálogo de seguridad.

## Referencias

- [AGENTS.md — flags de seguridad del selectivo: copia centralizada](../../AGENTS.md)
- [Arquitectura vigente — seguridad selectiva](../ARCHITECTURE.md)
- [Context Pack: system-selective](../context-packs/system-selective.md)
- [Context Pack: persistence](../context-packs/persistence.md)
- [`SelectiveSafetySelection` y `DeepCopy`](../../src/RackCad.Domain/Systems/SelectivePalletDesign.cs)
- [Documentos de persistencia de seguridad por subtipo](../../src/RackCad.Application/Persistence/SafetySelectionDocuments.cs)
- [`SelectivePalletDesignDocument`](../../src/RackCad.Application/Persistence/SelectivePalletDesignDocument.cs)
- [Pruebas de round-trip de la seguridad](../../tests/RackCad.Tests/SafetySelectionDocumentsTests.cs)

## Notas posteriores

- **2026-07-22 — Aceptado por Mario Pérez**, dueño del repositorio («Sí, apruebo»). La aceptación recae sobre este registro tal como está en el candidato `600b22e`; no atribuye fecha ni decisores históricos ausentes y conserva las limitaciones documentadas. Decisión versionada en [`docs/automation/decisions/I-07.md`](../automation/decisions/I-07.md).
