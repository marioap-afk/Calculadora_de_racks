> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: comprobación acotada de la v2 del paquete del contrato de PB-TC

Eres el revisor que hizo la revisión v1 (`pbtc/independent-review-v1.md`, NO EMITIBLE 1/4/10). Sigues siendo **independiente y adversarial**, y trabajas en solo lectura, con las mismas reglas que en la v1: sin compilar, sin ejecutar, sin escribir en el worktree (HEAD `f9f2afbb`), Python solo de lectura escrito con la herramienta de archivos y sin AutoCAD.

## Qué revisas

En `<scratchpad>\hs65\pbtc\`:
- `I-64-f4b-pbtc-gate-contract-package-v2.md`, la v2, con su registro de cambios, el §8.0 y los anexos A y B;
- `pbtc-measurements.md`, apartado K;
- `v2-instructions.md`, lo que la sesión decidió para la v2. La sesión dio por buenos B-01, I-01, I-02, I-03, I-04 y M-02 tras comprobarlos en el código;
- la v1 y tu informe v1, como referencia.

## Qué tienes que hacer

1. **El estado de cada hallazgo de la v1** (B-01, I-01 a I-04 y M-01 a M-10): RESUELTO, PARCIAL o NO RESUELTO, con su evidencia en la v2 y en el código.
2. **Lo nuevo de la v2**, atacado con el mismo rigor que la v1:
   - **CT-PBTC-10 y D-PBTC-10:** la corrida temporal sobre `BaseSha` con `Assert.True(false, …)`; la elección de `pb-02-modulos`; la parada PBTC-C14; si el método falla en la base por aserción y cumple sus líneas en el GREEN. Comprueba K-1, que el mensaje de `Assert.Equal` sobre listas se trunca, y la reconciliación por `ModuleId` y `Kind` (DA:315-317).
   - **La fila F4 con la clase `CATALOGO`.**
   - **El aviso en L2:** `StatusContent`, las guardas de la banda de estado, `PushBackShellAdoptionTests`, que `SetStatus` no lo toque y las líneas de CT-PBTC-07 y 08 con el nuevo lugar.
   - **El paso 3 de CT-PBTC-03 con `"61.2350"`.**
   - **El `ForbiddenWriteScope` de 131 entradas:** que existan, que ninguna ruta permitida quede debajo y que siga el método de PB-T0.
   - **Los recuentos nuevos** del §6.4 y del §6.9, con los anexos y sin ellos.
   - **Los anexos A y B**, que deben poder incorporarse tal cual si el Owner los elige:
     - diseño por línea, CT con líneas esperadas y fallo en la base, sustituciones (S-04 de CT-PB0-05) y rutas;
     - sus efectos;
     - las cuatro cajas, no seis, del anexo A;
     - si 56 es una longitud admitida para M3 en `pb-31` (CT-PBTC-12).
   - **La sub-decisión R-03 y su opción (ii).**
3. **El §8 y el §8.0:** fieles a lo medido, sin sesgo, completos, y en lenguaje llano en el §8.0. ¿Falta alguna decisión que deba ir al Owner antes de emitir?
4. **Una muestra de anclas** nuevas o cambiadas.

## El informe

En español, en `pbtc/independent-review-v2-check.md`, con este formato:
- el veredicto arriba;
- §1, método y límites;
- §2, una tabla con el estado de cada hallazgo de la v1;
- §3, los hallazgos nuevos (Id N-xx, gravedad, contenido, evidencia y corrección);
- §4, el veredicto: **EMITIBLE**, **EMITIBLE TRAS CORRECCIONES** o **NO EMITIBLE**, con el recuento B/I/M de lo que queda abierto y lo que debe ir al Owner.

Sin rutas absolutas locales ni nombres de usuario o de máquina. Cita la v2 como `V2:n`.

**Tu respuesta final** es una sola línea: el veredicto, el recuento y la ruta del informe.
