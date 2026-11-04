# R20261103T175351Z-0a77 — Decisión del Owner: la sesión opera AutoCAD (desviación de T3D §1)

RecordedUtc: 2026-11-03T20:00:12Z (antes de abrir AutoCAD; BASELINE ya confirmada; acad.exe = 0)

## Regla afectada
T3D §1 (Roles): el Owner es el operador en la estación (AutoCAD, pasos SM, registro, firma); la sesión responsable «No hace: operar
AutoCAD o tocar la estación del host».

## Instrucciones literales del Owner (chat)
1. «Mejor toma control de la computadora y hazlo tú»
2. La sesión preguntó, con el riesgo explícito (la autorización vale para UNA ocurrencia; si el Coordinator no acepta la desviación, se
   pierde). La respuesta seleccionada fue la etiqueta «Operas tú (Recomendado)», que la sesión interpretó como «opera el Owner».
3. Aclaración del Owner: «No, opera tú»

## Cómo se ejecuta
- La sesión opera AutoCAD con computer use (capturas de pantalla, clics y teclado en la estación), siguiendo la guía del operador y el
  orden de la autorización sin otros cambios: NETLOAD único del DLL 2923E7F2…AE26 con «cargar una vez», inspección del proceso tras el
  NETLOAD (SM-00d1), SM-03 sin rescate de la paleta, nada de TRUSTEDPATHS, SECURELOAD, perfil, registro, ACL ni entorno; ningún dibujo
  se guarda.
- Las observaciones del registro del operador son de la sesión (lo que muestran las capturas); el Owner conserva la firma.
- Límite técnico conocido: computer use no arrastra con el botón central; la parte «encuadre con botón central» de SM-29 se anota
  NOT_EXECUTED (motivo técnico) si no hay otra vía equivalente.
- Ningún archivo de captura de pantalla se guarda como evidencia (privacidad del repo público); la evidencia es la transcripción.

## Para la revisión del Coordinator
Desviación de rol decidida por el Owner; no cambia el Candidato, el DLL, la BASELINE ni las reglas de STOP. El Coordinator decide si la
ocurrencia es aceptable con operador = sesión.
