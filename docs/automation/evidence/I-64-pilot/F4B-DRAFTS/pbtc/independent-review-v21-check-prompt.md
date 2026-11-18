# Encargo: comprobación acotada de la v2.1 del paquete del contrato de PB-TC (la que se emite)

Eres el revisor de la v1 y de la v2. Sigues siendo independiente y adversarial, y trabajas en solo lectura con las mismas reglas: sin compilar ni ejecutar, sin escribir en el worktree (HEAD `f9f2afbb`) y con Python solo de lectura.

## Entradas, en `pbtc/`

- `I-64-f4b-pbtc-gate-contract-package-v21.md`, con su registro v2 → v2.1;
- `pbtc-measurements.md`, apartado L;
- las decisiones literales del Owner, en `owner-pbtc-decisions.md` (F4B-E06);
- lo que pidió la sesión, en `v21-instructions.md`;
- tu comprobación de la v2, `independent-review-v2-check.md`.

## Qué tienes que hacer

1. **N-01 a N-10:** el estado de cada uno (RESUELTO, PARCIAL o NO RESUELTO), con su evidencia.
2. **La fidelidad a las decisiones del Owner:**
   - **OD-PBTC-1:** la regla `Keep` y su extensión.
   - **OD-PBTC-2.**
   - **OD-PBTC-3:** el literal byte a byte frente a `owner-pbtc-decisions.md`, en L2.
   - **OD-PBTC-4 (A):** D-PB-11, incorporado entero.
   - **OD-PBTC-5 (B):** el anexo A fuera, y las cajas de H-31 prohibidas de forma explícita en INV-PBTC-15 y PBTC-C10.
   - ¿Algo del paquete va más allá o se queda corto frente a lo que el Owner eligió?
3. **D-PB-11, ya en el cuerpo, atacado como en la v1:**
   - el diseño por línea;
   - CT-PBTC-14: su fallo en la base por aserción, sus líneas en el GREEN y el orden de la pasada que consume «Restaurar estándar» frente a la lectura de los controles;
   - S-04 de CT-PB0-05: el literal, las aserciones vieja y nueva, y la cobertura;
   - las rutas;
   - la guarda «Fact3» (`PushBackModuleEditorCharacterizationTests.cs:381-401`) e INV-PBTC-19;
   - los tres filtros nuevos de `RequiredTests` y sus recuentos;
   - cualquier otra prueba o guarda existente que choque con el arreglo. Búscala con `git grep` sobre `StandardRestoreRequested`, `RestoreRackParameters`, `advanced`, CT-PB0-04 y `PushBackModuleEditorWindowTests`.
4. **Lo que el redactor añadió por su cuenta:**
   - PBTC-C14 extendida a la fila F1;
   - la sustitución de `generalPallet` por un objeto nuevo en `ReadInputs`;
   - los portadores actualizados con `Keep` en cada pasada.

   ¿Son correctos, quedan dentro de la autoridad y no dejan latitud al Worker?
5. **Los recuentos finales del §6.4 y del §6.9**, y el `ForbiddenWriteScope` de 130 entradas: que existan, que ninguna ruta permitida quede debajo y que no falte ninguna de las rutas de D-PB-11.
6. **El §8.0:** fiel a lo decidido, en lenguaje llano y sin identificadores internos.
7. **Una muestra de anclas** nuevas o cambiadas.

## El informe

En español, en `pbtc/independent-review-v21-check.md`, con este formato:
- el veredicto arriba;
- §1, método y límites;
- §2, el estado de N-01 a N-10;
- §3, la fidelidad a las decisiones del Owner;
- §4, los hallazgos nuevos (N2-xx, gravedad, contenido, evidencia y corrección);
- §5, el veredicto: **EMITIBLE**, **EMITIBLE TRAS CORRECCIONES** o **NO EMITIBLE**, con el recuento B/I/M de lo que queda abierto y lo que deba ir al Owner.

Sin rutas absolutas locales ni nombres de usuario o de máquina. Cita la v2.1 como `V21:n`.

**Tu respuesta final** es una sola línea: el veredicto, el recuento y la ruta del informe.
