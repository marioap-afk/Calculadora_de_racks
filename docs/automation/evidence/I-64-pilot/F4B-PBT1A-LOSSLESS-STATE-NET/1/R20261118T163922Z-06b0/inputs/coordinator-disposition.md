# Disposicion del Owner sobre la desviacion declarada de la entrega (Deviations[0], CT-PBT1A-02)

Fecha: 2026-11-18. Canal: chat de la sesion principal (pregunta de opciones; respuesta literal). Texto completo en F4B-DRAFTS/coordinator/owner-pbt1a-decisions.md, que la sesion custodia al cerrar la tarea.

Hecho: la fila de CT-PBT1A-02 del paquete suponia que elegir "lb" en WeightUnitBox llega por si solo a la salida. El Worker midio en BaseSha que no llega (SamePallet ignora la unidad; solo se escribe al reconstruir la estructura). Escrito tal como describe la fila, MU-T1A-02 no habria hecho fallar CT-02, que es la parada PBT1A-C04. En vez de parar, el Worker anadio pasos a CT-02 durante la medicion (paso 1, corrida p3), antes de las pruebas definitivas y antes de las corridas por mutante: DepthBox=54 tras "lb", una ventana con "ton" y DepthBox=54, y "+ frente" tras PalletsDeepBox. Lo declaro en Deviations[0](a). CT-02 sigue fijando las tres cosas de su fila; las lineas son las medidas en p4, sin ajustar; ningun mutante cambio.

Pregunta al Owner: "PB-T1a, prueba CT-02: [...] La prueba sigue fijando lo que pedia su fila, mas dos lineas medidas. Ratificas la prueba tal como esta?"

Respuesta del Owner: "Ratificar (Recomendado)".

Alcance de la disposicion: ratifica CT-PBT1A-02 tal como esta en CurrentSha, SOLO para este intento y este caso. Para ese punto prevalece sobre la letra de PBT1A-C04 y de la regla SIN LATITUD del prompt del Worker; no amplia ninguna otra regla ni otra subtarea. El cambio de CT-03 (Deviations[0](b)) no hace falta ratificarlo: la revision independiente lo juzga aceptable (corrige un error de escritura propio y rediseña sin motivo de discriminacion; la fila se sigue cumpliendo).

Decisiones del Owner en la misma vuelta que no afectan a esta verificacion: c-09 al reconstruir, "como hoy, regla explicita" en PB-T1b; los otros hallazgos de INV-PBT1A-13, "tal cual" y anotados como mejoras futuras.
