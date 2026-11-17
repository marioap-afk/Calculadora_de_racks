# Análisis tras las revisiones — F4B-PBT0-CHARACTERIZATION, intento 1

## Coincidencias de las dos revisiones

| Tema | Directa (`direct-review.md`) | Independiente (`independent-review-code.md`) |
|---|---|---|
| Alcance, bytes, RED/GREEN, CI | Conforme | Conforme (10/16/6 rutas, 0 prohibidas, `src` igual a la base) |
| Tabla de mutantes | 20 de la tabla | 20 idénticos, principal; todos discriminan en aislado por aserción, MU-16 solo con c-16 |
| c-16 en CT-17 | D-1: contradice INV-PBT0-14/PBT0-C10; la latitud estaba solo en el prompt; decisión del Owner | I-01 IMPORTANTE (OWNER-1): debía ser PBT0-C10 (con C07 como causa); se resuelve con disposición, sin código |
| c-12/c-13 como RECHAZADO | D-2 menor | M-03, parte de OWNER-1 |
| `PalletTolerance = 0` | D-3: dato para el Owner | §8.1: RACKEDITAR no se cierra (Report(ex)); el rack no se puede reabrir; prioridad baja |
| `pb-21-cola` | D-4: semántica conocida (H14) | §8.2, OWNER-3: posible pérdida de datos al Actualizar en RACKEDITAR para documentos así; la ventana actual no los produce; vuelve al Owner antes de PB-T1 (OB-06.5) |
| Hueco de T-1 | D-5 | — |

**Menores solo de la independiente:**
- M-01: fuga de una ventana inerte en c-13;
- M-02: cita `:322` en vez de `:227`;
- M-04: CT-08 y CT-09 leen el estado del editor en vez de la salida;
- M-05: aserciones (a)/(b) vacías fuera de CT-18 a CT-20;
- M-06: falta evidencia guardada de MU-16 sin c-16 y de M1-M9.

**Datos para el Owner, solo de la independiente:**
- OWNER-2: H-PB-28 es un defecto observable de RACKEDITAR. Hay que decidir antes de PB-TC o PB-T1 si se reproduce o se corrige.
- OWNER-4: R7 y R11 de T-3 salen «materiales» por el BOM exacto y por el emparejamiento por orden. Conforme con OB-06.4, pero hay que leerlo así.

## Disposición pendiente (OWNER-1)

- **(a) [recomendada] Ratificar la enmienda de CT-PB0-17:**
  - c-16 como caso nuevo y la fila MU-PB0-16 apoyada en él;
  - c-12 y c-13 como `RECHAZADO <tipo>: <mensaje>`;
  - nota: W:387, W:406 y W:407 son inalcanzables desde un documento.

  Los menores M-01 a M-06 y D-5 se registran y se arrastran; M-02 se corrige en el registro. La cláusula del paso 3 del prompt queda registrada como defecto de la sesión (LECCIÓN). Sigue la cadena: verificación del Controller con la disposición custodiada, nc1 a nc3 y cierre.
- **(b) Tratarla como PBT0-C10:** corrección del contrato (c-16 y la forma RECHAZADO en el paquete) y un intento 2/3 completo. El resultado esperado son las mismas pruebas; cuesta un intento y unas horas.

## Siguiente según la respuesta

- **Si (a):**
  1. Custodiar la disposición (f4b-dispositions F4B-E05, evidencia).
  2. Rehacer el prompt de la verificación con la disposición como autoridad.
  3. verify_pre → launch → verify_post → relay.
  4. nc1 a nc3.
  5. Cierre.
- **Si (b):**
  1. Custodia del intento 1 y paquete o contrato del intento 2.
  2. Cadena completa sin nc4.
