# Encargo: la v2 del paquete de PB-T1, tras la revisión independiente IRT1-1

Lo da la sesión principal al redactor de la v1.

**El veredicto de la revisión** (`pbt1/independent-review-v1.md`) es **EMITIBLE TRAS CORRECCIONES, 0/8/15**, con **una decisión nueva para el Owner: L-1, «no aliasing»**.

**Las reglas son las de la v1:**
- solo lectura en el worktree (HEAD `eedb069c`);
- sin compilar ni ejecutar;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc.

## Lo que decide la sesión

1. **Del I-01 al I-08:** aplica la corrección que propone la revisión en cada uno. En concreto:
   - **I-01:**
     - reescribe L-1 con las tres opciones del §4 de la revisión y con la medición **completa** de los consumidores del baseline compartido;
     - CT-07 comprueba **por separado**, sin la exención, que la cola, el commit y la sesión de la copia no comparten objetos con el original;
     - corrige HT1-12, porque la cola aparcada por `ParkTailOf` sí apunta a objetos del baseline.
   - **I-02:** excluye los objetos inmutables con el criterio `IsImmutable` del censo del piloto, y nombra los diseños de CT-07.
   - **I-03:** fija un ayudante de prueba que copie literal la pasada de W:659-702, o una API de asiento si existe, y el número de pasadas de cada camino. En el comparador, una sesión nula equivale a una sesión recién abierta sin cambios pendientes.
   - **I-04:** los portadores se adoptan donde hoy, dentro de `LoadFromModel` y bajo `suppressSync`. Además, CT-14 gana el guion F-03.
   - **I-05:** la copia de la cola al cargar va campo a campo y literal, con `RackFrameProjectStore.DeepCopy` como en DA:557-566.
   - **I-06:** añade PBT1A-C04, PBT1B-C04 y PBT1C-C04, cada una con el texto propio de su régimen.
   - **I-07:** da las firmas exactas de la API del mapa, la normalización de las rutas y la fila de `$.Design`.
   - **I-08:** añade la parada o precondición de autoridad entre subtareas. Si PB-T1a fija una pérdida (clase c, d o nueva) o un control que decide el estado, se vuelve al Owner antes de PB-T1b.
2. **Los 15 menores:** aplícalos según su corrección. Si alguno lo dejas para la emisión de su subtarea, dilo en el registro de cambios.
3. **L-1:**
   - Redacta la v2 con la **opción (a) como diseño vigente**, la recomendada: la excepción declarada, limitada al baseline resuelto y al informe de reconciliación, con sus condiciones y el arrastre A-2.
   - Escribe en un **anexo L1-C** el delta exacto que habría que aplicar si el Owner eligiera **(c)**, es decir, volver a resolver el baseline en la copia con su prueba de equivalencia y la medición de los compuestos. Con eso, la elección de (c) se incorpora sin redactar de nuevo.
   - **La opción (b)** se describe solo en el §8, porque sería una tarea previa de Domain.
4. **El §8** queda así:
   - L-1, la decisión abierta, con (a), (b) y (c), la recomendada y su coste;
   - L-2 y L-3, sin ser decisiones, según la revisión;
   - las condiciones de emisión: la respuesta del Owner a L-1; el cierre de PB-TC2 (precondición 1), que hoy está parada en su vuelta C14; y la comprobación acotada de la v2.
5. **Las dependencias de PB-TC2:**
   - Si el Owner elige en C14 la opción (B), dejar fuera la altura manual de cabecera, esa caja seguirá siendo un control fuente de verdad, y PB-T1 tendrá que declararlo.
   - Añade en el §8 una nota con lo que cambiaría en ese caso, en dos o tres líneas, sin redactarlo.
6. **El registro de cambios v1 → v2,** al principio: una fila por hallazgo (I-01 a I-08 y M-01 a M-15).

## Salidas, en `pbt1/`

- `I-64-f4b-pbt1-gate-contract-package-v2.md` (no sobrescribas la v1);
- un apartado nuevo en `pbt1-measurements.md`.

**Tu respuesta final** es corta: la ruta, los recuentos por subtarea, cualquier decisión nueva y lo que no pudiste medir.
