# ADR-0047: Workspace persistente y modeless de RackCad

- **Estado:** propuesto
- **Fecha:** 2026-10-01
- **Decisores:** Owner del repositorio (único que lo acepta); redactado por el Worker de F1-T1-MODEL (Claude) como proyección del Anexo A del Freeze.
- **Iniciativa relacionada:** I-64 — Workspace persistente (`architecture/workspace-persistente-rackcad`)

> Este ADR es una proyección del Anexo A de la Proposal V5 congelada de I-64 (`docs/initiatives/I-64-proposal-v5.md`, Freeze I64-CONSENSUS-V5-01).
> No añade decisiones ni semántica, no adopta opcionales históricos y no es autoridad superior al Freeze. Solo el Owner puede aceptarlo.

## Contexto

RackCad edita con ventanas modales; el borrador vive en la ventana y el host interpreta la intención tras el cierre; Actualizar no compara con la base, escribe por vista y oculta
faltantes (Discovery I-64 §§1, 3, 9.12). El Owner pide un panel persistente con borradores por rack y Actualizar como única frontera de escritura.

## Decisión

1. **Host:** un `PaletteSet` por proceso, sin estado persistido por RackCad; con el foco en un campo, ninguna tecla llega a AutoCAD.
2. **Sesiones:** una por instancia abierta de documento; toda petición y ejecución va ligada a su sesión; nada cruza sesiones.
3. **Eventos:** un único puente; los manejadores ordinarios solo encolan; el de cierre vive toda la sesión, también degradada, y decide el veto solo con estado en memoria; el drenaje
   ocurre en reposo y sin modal RackCad activo; ninguna corrección depende de la ausencia de eventos.
4. **Inventario y pertenencia:** el inventario del documento y el índice runtime son del panel y se capturan sobre las autoridades integradas de identidad de rack y de vista; su
   representación interna de hechos neutrales no es fundación ni contrato para otros consumidores, no decide métricas y es privada del panel (MASTER-I63-I64-02). Una sola autoridad
   define los objetivos de navegación para todas las entradas; un sondeo de Id solo produce diagnósticos. El estado del índice no incluye estado de sesión. La pertenencia para mutar es
   `RackSiblingMembership` (Freeze I-55 V5 §4.1).
5. **Selección:** ningún evento escribe selección ni vista; las acciones explícitas sí; el eco se reconoce por contenido.
6. **Pestañas:** registros ligeros; K superficies materializadas; la primera pulsación fija la preview.
7. **Borradores:** StaleBase, InitialDraftState y CurrentDraftState separados; dirty por estado inicial de la build y texto pendiente; base y estado inicial de una sola lectura; texto
   pendiente nunca perdido.
8. **Actualizar desde el panel:** relectura cruda dentro de la misma transacción caller-owned de MUTATE; todo o nada sobre el authored del rack sobre la costura de I-55; clasificación
   por la frontera de `Commit()`; un paso de UNDO.
9. **Cierre:** veto continuo con borradores dirty hasta el descarte explícito.
10. **Coexistencia y migración:** comandos clásicos sin cambio; adaptadores escalonados con caracterización D13/D9 y escritores caller-owned; diferir un sistema es del Owner.
11. **Persistencia:** ninguna propia; ni identidad estampada ni RackId inventado.

## Relación con otros ADR

Complementa ADR-0010 y ADR-0032 D6 sin modificarlos; extiende a las superficies alojadas el contrato de ADR-0029 (D7, D8, D9, D11 y D13) sin cambiar su censo D1; consume ADR-0006,
ADR-0009, ADR-0019 y ADR-0044.

## Alternativas consideradas

- `ShowModelessWindow` (contingencia).
- Host híbrido.
- Revisión persistida.
- Huella normalizada.
- `FindRackBlocks` para el panel.
- Eventos como autoridad.
- Veto de un solo intento.
- Primitivas self-owned anidadas.
- Seis reescrituras paralelas.
- Un snapshot común con I-63 (retirado por MASTER-I63-I64-02).

## Consecuencias

- Primer uso de eventos de AutoCAD en el producto.
- Conflictos falsos posibles por reserialización, a cambio de no sobrescribir nunca en silencio.
- Dos semánticas de Actualizar conviviendo (clásica y del panel).
- Residuo de biblioteca purgable tras un fallo.
- Insertar y `BindingIntent` siguen en el editor clásico.

## Referencias

- `docs/initiatives/I-64-proposal-v5.md`, Anexo A (fuente de esta proyección), D-03, D-04, D-05 y D-06.
- Freeze I64-CONSENSUS-V5-01 (`docs/automation/evidence/I-64-evidence.md` §18).
