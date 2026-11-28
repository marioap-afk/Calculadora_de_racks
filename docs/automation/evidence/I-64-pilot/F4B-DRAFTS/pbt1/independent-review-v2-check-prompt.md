> **Redaccion para el repo publico:** las rutas locales y las variables de entorno de carpetas del usuario se sustituyen por marcadores (`<scratchpad>`, `<worktree-de-I-64>`, `<datos-locales-del-usuario>`, `<datos-de-aplicacion-del-usuario>`). El resto no cambia.

# Encargo: comprobación acotada de la v2 del paquete de PB-T1

Eres el revisor de la v1 de PB-T1 (`pbt1/independent-review-v1.md`, IRT1-1: EMITIBLE TRAS CORRECCIONES 0/8/15, con una decisión para el Owner, L-1). Sigues siendo independiente y adversarial, y trabajas con las mismas reglas:
- solo lectura;
- sin compilar ni ejecutar;
- Python solo de lectura, escrito con la herramienta de archivos y nunca con `-c` ni con un heredoc;
- HEAD `eedb069c`.

## Entradas, en `<scratchpad>\hs65\pbt1\`

- `I-64-f4b-pbt1-gate-contract-package-v2.md`, con su registro de cambios v1 → v2 y el anexo L1-C;
- `pbt1-measurements.md`, apartado M;
- `v2-instructions.md`: lo que pidió la sesión;
- `owner-return-l1.md`: la pregunta de L-1 que la sesión llevará al Owner;
- tu informe de la v1.

## Qué haces

1. **El estado de I-01 a I-08 y de M-01 a M-15:** RESUELTO, PARCIAL o NO RESUELTO, con la evidencia en la v2 y en el código.
2. **Lo nuevo de la v2, atacado igual que en la v1:**
   - la medición completa de los consumidores del baseline compartido (el apartado M, con el `system.Name` del Plugin);
   - la nueva CT-07 sin exención;
   - el ayudante de la pasada;
   - la equivalencia entre una sesión nula y una recién abierta;
   - la adopción de los portadores en `LoadFromModel`, junto con el guion F-03;
   - la copia literal de la cola;
   - las C04 de cada subtarea, y la C15 con la precondición 5;
   - las firmas del mapa de propiedad;
   - el anexo L1-C.
3. **La pregunta de L-1** (`owner-return-l1.md`): ¿es fiel, completa, sin sesgo y en lenguaje llano?
4. **Decisiones materiales nuevas** además de L-1. Sé estricto.

## El informe

En español, en `pbt1/independent-review-v2-check.md`, con este formato:
- el veredicto arriba;
- §1, método y límites;
- §2, el estado de los hallazgos de la v1;
- §3, los hallazgos nuevos;
- §4, el veredicto: **EMITIBLE**, **EMITIBLE TRAS CORRECCIONES** o **NO EMITIBLE**, con el recuento B/I/M abierto y las decisiones para el Owner. L-1 sigue abierta mientras el Owner no conteste.

Sin rutas absolutas locales.

**Tu respuesta final** es una sola línea: el veredicto, el recuento, las decisiones para el Owner y la ruta.
