# I-64 F4-T4C: decisiones del Owner D1 a D6 (2026-11-15)

La sesión preparó, a petición del Owner («Si tengo decisiones que tomar, armame un prompt con todas esas solicitudes»), un bloque con las decisiones pendientes D1 a D6, mientras corría el Worker de F4-T4C1-HOST. Se transcribe la respuesta literal del Owner.

---

I-64 F4-T4C — Decisiones del Owner (2026-11-15)

D1. Redacción de los 6 textos nuevos del host

Respuesta D1: A

Ratifico tal cual:

UpdateInFlightReason
= “Hay una actualización en curso: espera a que termine.”

HostNotRunReason
= “La actualización no llegó a ejecutarse; no se escribió nada en el dibujo.”

HostRequestFailedPrefix
= “No se pudo pedir el contexto de comando: “

LockFailedPrefix
= “No se pudo bloquear el documento: “

HostUnavailableReason
= “La actualización no está disponible en esta sesión.”

UpdateNotClosedReason
= “El borrador no aceptó el cierre de la actualización.”

No gastar una corrección únicamente por redacción.

⸻

D2. Cierre de T4C1 sin volver al Owner

Respuesta D2: A

Puedes cerrar T4C1 sin volver a mí si:

* revisión directa: 0 bloqueantes / 0 importantes;
* revisión independiente: 0 bloqueantes / 0 importantes;
* CI y verificación exigidas pasan;
* nc1..nc3 pasan por su oráculo ordinario o existe un sustituto ya autorizado cuya firma coincida EXACTAMENTE con PD-F4-01, PD-F4-04, PD-F4-05, PD-F4-06 o PD-F4-07.

Conserva siempre la salida ordinaria del control.

Un modo de fallo nuevo vuelve al Owner.

Los menores pueden arrastrarse declaradamente a T4C2 cuando T4C2 sea propietaria de la superficie.

⸻

D3. Corrección de T4C1

Respuesta D3: A

Autorizado:

Attempt 2 / 3 sin volver al Owner si una revisión o verificación demuestra REWORK real y la corrección permanece dentro del objetivo y alcance de T4C1.

No Attempt 3 sin autorización explícita del Owner.

La corrección debe mantener:

* análisis de causa raíz;
* RED acreditado;
* CorrectionOf;
* revisión directa;
* revisión independiente;
* nueva verificación;
* controles.

⸻

D4. Fidelidad de planificación en delegaciones de corrección

Respuesta D4: A

Cada NUEVA delegación contractual, incluida una delegación de corrección, dispone de:

UNA replanificación reforzada por fallo de fidelidad literal.

No consume intento de producto.

Si esa replanificación reforzada vuelve a fallar:

STOP y regreso al Owner.

No se acumulan replanificaciones no usadas de delegaciones anteriores.

⸻

D5. F4-T4C2-PANEL

Respuesta D5: A

Autoricé:

* preparar ya el contrato desde el paquete v5;
* hacer su comprobación/revisión independiente mientras termina T4C1;
* incorporar los hallazgos necesarios;
* emitir y ejecutar T4C2 inmediatamente cuando T4C1 cierre correctamente.

F4-T4C2 comienza con:

attempts = 1 / 3

No hace falta volver al Owner entre el cierre normal de T4C1 y la emisión de T4C2.

Sí debes volver si la revisión de T4C2 descubre una decisión material nueva sobre:

* semántica de Actualizar;
* autoridad de escritura;
* Undo;
* conflicto/stale;
* T1B;
* o cualquier cambio que altere el comportamiento ratificado de T4B/T4C1.

⸻

D6. Smoke-2

Respuesta D6: A

Prepara desde ahora la guía del operador.

Requisitos:

* en español;
* lista para pegar directamente en el chat;
* pasos numerados;
* resultado esperado después de cada paso;
* puntos claros donde yo tenga que responder PASS / FAIL / observado;
* ninguna acción destructiva sin avisarme antes;
* distinguir qué evidencia debe verse en AutoCAD y qué evidencia se obtiene de logs/estado interno.

Dibujo que usarás:

LO ELIJO DESPUÉS.

Pero debe ser un dibujo real y representativo de Push Back, preferentemente una copia de trabajo sobre la que podamos:

* editar AnnotationScale;
* Actualizar;
* provocar un cambio externo;
* probar conflicto;
* hacer Undo;
* hacer Redo;
* cambiar documento/pestaña;
* probar cierre;
* descartar sin riesgo sobre un archivo de producción.

No usar un dibujo importante sin copia de seguridad.

⸻

Notas libres:

1. T4C1 y T4C2 son la última integración de producto antes de la validación física de F4. No ampliar su alcance con mejoras cosméticas.
2. Smoke-2 sigue siendo obligatorio. Las pruebas automatizadas no sustituyen:
    * ExecuteInCommandContextAsync real;
    * DocumentLock real;
    * una operación lógica de Undo;
    * Undo/Redo;
    * lifecycle CLOSE/CLOSEALL/QUIT;
    * own-mutation contra eventos reales;
    * IC-04 bajo AutoCAD real.
3. Un sustituto de control solo se reutiliza cuando la firma sea EXACTAMENTE una de las ya autorizadas. No generalizar por nombre de check.
4. Si T4C1 y T4C2 cierran y Smoke-2 pasa, F4 puede considerarse prácticamente completa, sujeto a custodias y cierre formal.
5. Continúa sin tocar main, sin force-push y sin modificar evidencia histórica ya custodiada.
