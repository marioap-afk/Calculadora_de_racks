# I-62 — Notas del Principal sobre la preparación de §92 (no normativas)

## 1. Exclusión de `contract`: punto 7 del borrador

El borrador (`contract/disposition-draft.md`) crea en su punto 7 un recálculo de `Contract` por la supervisión, desde CONTRACT y DELEG custodiados. Ese
control no existe hoy en el protocolo. Decisiones §92, punto 4, pide que el aceptante y el Coordinator sigan aplicando los controles independientes
**exigibles**, y esos ya cubren `Contract` sin el instrumento:

- el oráculo de C-24, calculado sin el instrumento y sellado antes (A6-6): una salida ingerida distinta del oráculo es FAIL (A6-5, regla 5);
- el oráculo relativo de cada control (RAE §10): un `Contract` falso en nc2 cambia la `FailureClass` y el control no se supera;
- la aceptación A1'-A8' de la delegación real por el Coordinator (A3'-A5' frente al contrato);
- R3 y R9 del aceptante sobre las lecturas `read-json --file` de CONTRACT y DELEG.

Recomendación del Principal: emitir la disposición sin el punto 7. Si el Coordinator lo quiere como motivo de rechazo de un VERIFIED, es un control
nuevo y su lugar es una enmienda, no una disposición.

## 2. Cambios del kit para su próxima preparación (no aplicados; el kit `a6m7-measure` sigue congelado)

- Cierre de A7-1 (igual a R7-A en el banco) y comprobación de cobertura de §3.5 (`check_citation_blobs.py`, modo `coverage`).
- Designación que registra la ruta y el blob de los cuatro artefactos (A7-1, regla 2 (c)); el formato `a6m7-designation/1` solo registra el del contrato.
- Orden declarada de 11 órdenes, sin `contract`: texto, contrastes primarios (P18 a `read-json --file`), comparador (OP6) y oráculo, esquema y su
  proyección, marcador `M7-INSTRUMENTO` y comprobación `Argv[0]` dentro del conjunto declarado antes de construir la solicitud (`declared_orders.py`), o en la
  regla de reconstrucción (blob nuevo, invalidador).
- `RunId` con la forma de AP 16.6 y shell de la invocación identificado (comprobación (iii) con cada shell admitido, o exigir el pwsh del runtime)
  (decisiones §92, punto 6).
- Comprobaciones de fallo cerrado de las lecturas (FC-01..FC-24 y FC-18) para las VERIFY de la ronda de B1; la medición en FX-U1 no las activa.
- Cabecera de `reconstruct_runs_m7.py`: declarar la retirada de una línea sin efecto (preflight de A-7, F8).

## 3. Avisos menores

- La línea 132 de A-7 supera el ancho habitual tras la segunda corrección; no afecta a las guardas, que normalizan espacios. No se reajustó para no cambiar
  el blob preflightado.
- Los `RunId` del banco y del kit (`…-m7m0`, `…-m7c2`) no tienen la forma de AP 16.6 (cuatro dígitos hexadecimales).
