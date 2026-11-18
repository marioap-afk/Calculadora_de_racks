# I-64 F4B PB-T1a — Vuelta al Owner tras el Worker (intento 1)

**Fecha:** 2026-11-18. La prepara la sesión principal en modo autónomo, con la revisión directa (`direct-review.md`) y la independiente (`independent-review-code.md`, REWORK 0/1/9).

## Dónde estamos

- **El Worker terminó PB-T1a sin paradas:**
  - RED `15f1f225`;
  - GREEN `73d30982`, con CI 4/4 y exactamente los conteos previstos (UI 2 436 / 2 415 / 21; Core 19 070);
  - el producto, idéntico al de la base.
- **La red discrimina:** los quince mutantes son los de la tabla y cada uno hace fallar su CT.
- **La revisión independiente** no encuentra bloqueantes. Su único importante (I-01) es de contrato y no de código, y se resuelve con la pregunta 1.
- **Pendiente:** la verificación del Controller y los controles nc1 a nc3. Van después de estas respuestas, con ellas como disposición de entrada.

## Pregunta 1 — CT-02: el Worker cambió sus entradas (I-01, D-1)

**El hecho:**
- La fila de CT-02 del paquete daba por hecho que elegir «lb» en `WeightUnitBox` llega a la salida. El Worker lo midió en la base y vio que no llega por sí solo: la unidad solo se escribe cuando otra edición reconstruye la estructura.
- Escrito tal como describe la fila, MU-T1A-02 (la unidad fija en «kg») no habría hecho fallar CT-02. El contrato manda parar en ese caso (PBT1A-C04), y el prompt del Worker lo decía expresamente: «No cambies el CT, sus pasos, sus entradas ni su linea esperada para que el mutante discrimine».
- El Worker, en vez de parar, añadió pasos a CT-02 mientras medía, **antes** de escribir las pruebas definitivas y antes de las corridas por mutante: `DepthBox=54` después de «lb», una ventana más con «ton» y `DepthBox=54`, y «+ frente» después de `PalletsDeepBox`. Lo declaró.

**Cómo queda:**
- CT-02 sigue fijando las tres cosas de su fila: «lb» sola (que no llega), la carga con «ton» y su «Actualizar», y `PalletsDeepBox` editado.
- Añade dos líneas: «lb» con una reconstrucción (que sí llega) y «ton» con una reconstrucción (que pasa a «kg»).
- Las líneas son las medidas, sin ajustar. Ningún mutante cambió.

**Opciones:**
- **(a) Ratificar CT-02 tal como está,** solo para este caso. I-01 se cierra sin tocar código, la revisión pasa a ACEPTABLE CON MENORES (0/0/9) y se sigue con la verificación en el intento 1. Es lo que el Owner hizo con c-16 en PB-T0 (F4B-E05).
- **(b) No ratificarlo:** es PBT1A-C04. Se corrige la fila de CT-02 en el paquete y se emite una delegación nueva, en el **intento 2 de 3**, aplicando a la vez M-01 y M-02 de la revisión.

## Pregunta 2 — c-09 al reconstruir: la caja de unidad decide el estado (OWNER-2)

**El hecho:**
- Con un rack guardado en «ton», cargarlo y pulsar «Actualizar» conserva «ton».
- Pero en cuanto el usuario edita otra cosa que reconstruye la estructura (por ejemplo, el fondo del pallet), la salida pasa a «kg» **con el mismo número** (1000), sin que el usuario haya tocado la unidad. La caja solo ofrece «kg» y «lb».
- PB-T0 solo había fijado la carga. La revisión independiente señala que esto convierte a `WeightUnitBox` en un segundo control que decide el estado, cuando el paquete decía que solo lo hace `PostBox` (HT1-15). Es el mismo tipo de caso que c-02 (el poste fuera de catálogo).

**Opciones:**
- **(a) Como hoy, como regla explícita.** PB-T1b reproduce el comportamiento de hoy, como una regla explícita del estado con su prueba, igual que c-02 (F4B-E09, pregunta 2). RACKEDITAR no cambia. El paquete de PB-T1b añade esa regla y su CT antes de emitirse.
- **(b) Como hoy, sin regla explícita.** PB-T1b lo reproduce dentro de la reconstrucción, sin una regla ni una prueba propias. La red de PB-T1a ya lo fija.
- **(c) Corregirlo antes de PB-T1b.** Una tarea propia de corrección declarada (como PB-TC2) para que la unidad del documento no se pierda al reconstruir. Cambia RACKEDITAR y retrasa PB-T1b.

## Pregunta 3 — Los otros hallazgos de la lista (INV-PBT1A-13, precondición 5)

**Los hechos:**
- **OWNER-3:** elegir «lb» no llega a la salida, ni siquiera con «Actualizar», si nada reconstruye la estructura: la elección se pierde en silencio. `PalletsDeepBox=9` no mueve la salida ni el fondo del frente que se añade.
- **OWNER-4:** aceptar Seguridad sin cambios en `pb-05-topes` añade a la salida el portador de botas (`Structure.SafetySelections` y `DefensePieceId`), sin cambiar el BOM. Confirma la observación de CT-16 de PB-T0. Si llega al dibujo es algo para Smoke-PB.

**Opciones:**
- **(a) Reproducirlos tal cual en PB-T1** y anotarlos como mejoras futuras (`docs/ideas-futuras.md`). PB-T1 no cambia RACKEDITAR.
- **(b) Corregirlos antes de PB-T1b,** con una tarea propia de corrección declarada.

## Para tu conocimiento (sin decisión)

- **M-06 / D-2:** para atribuir los fallos del RED, el Worker corrió en local 42 pruebas existentes de UI. 19 de ellas están fuera de los filtros del contrato (INV-PBT1A-10 solo admite esos filtros). No corrió la suite completa, y la prueba de limpieza del perfil sale idéntica antes y después.
- **OWNER-5:** el diseño fabricado F-02 da las mismas huellas que `pb-20-ab`, porque el lado frontal no se guarda en el diseño. La cobertura real de `FrontalSideBox` es F-03 y, más adelante, CT-PBT1C-02.
- **OWNER-6:** «Actualizar» y «Restaurar» sin cambios dan la misma huella que cargar en los 20 diseños; ninguna pérdida nueva.
- **Una corrección de la lista del Worker** (M-09): `AbsentSlotsB=[0]` en `pb-01-base` no es un hallazgo, porque el documento ya lo trae.
- **Los demás menores** de la revisión (M-01 a M-08) son de evidencia y de redacción. Quedan registrados y no cambian el resultado.
