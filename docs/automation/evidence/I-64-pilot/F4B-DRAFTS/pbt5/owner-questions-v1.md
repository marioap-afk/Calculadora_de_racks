# I-64 F4B PB-T5 — Preguntas al Owner (v1)

> **Redacción para el repo público:** sin rutas locales ni nombres personales. La hora del Owner va en UTC.

Acompañan al paquete `I-64-f4b-pbt5-package-v1.md` (citado como [P]).
- **Ninguna pregunta impide preparar** el Smoke-PB: la CI, el DLL y las copias.
- **Cada una lleva el valor que se aplica si no hay respuesta** al empezar la corrida. Así, si el Smoke-PB empieza mientras duermes, la sesión sabe qué hacer y lo deja escrito para que lo revises.
- **Ninguna respuesta cambia producto.** Todas tratan de qué se prueba o de cómo se prueba.

---

## Q-PBT5-1. La evidencia local del Candidato

**El contexto:**
- La guía de validación manual (§2 y §7.1) dice que el DLL de una validación del dueño sale de un Candidato. Un Candidato pide la suite completa de Core y la de UI **en local**, sobre el SHA exacto, además de la CI exacta.
- En F4, para Smoke-2, aceptaste una dispensa: CI exacta y filtros locales, sin la UI completa en local (DV-05 / O-08).
- El paquete de F4B (§7.2) dice que las reglas de F4 no se heredan en F4B sin orden tuya.
- Correr la UI completa en local sobrescribe la disposición guardada de la ventana del configurador de cabeceras, `RackFrameConfigurator.layout` (F4B-E30). La sesión la copiaría antes y la restauraría después, byte a byte.

**Las opciones:**
- **A. Candidato completo, como dice la guía (Recomendado):** Core y UI completas en local sobre el SHA exacto, tras el commit y con el árbol limpio, más la CI exacta. Cuesta tiempo de máquina antes de empezar; no cuesta nada tuyo.
- **B. Extender a F4B la dispensa de Smoke-2:** CI exacta y filtros locales, sin la UI completa en local. Es más rápido, pero el Candidato no cumple la guía al pie de la letra.

**Si no hay respuesta:** A.

---

## Q-PBT5-2. El estilo de cotas guardado (D-PB-06 (b)) en RACKEDITAR

**El contexto:**
- El cierre de F4B pide que el Smoke-PB verifique en RACKEDITAR las correcciones D-PB-06 y D-PB-08.
- La corrección (b) solo se ve con un rack que guarde un estilo de cotas que exista en el dibujo. En tus dibujos medidos, 0 de 22 racks Push Back lo guardan, y ningún botón de la ventana lo escribe (F4B-E04).
- Fabricarlo exige escribir a mano el sobre de un rack en una copia, con un guion LISP. Es un arnés, no un gesto del producto.

**Las opciones:**
- **A. NOT AVAILABLE FOR PHYSICAL SMOKE (Recomendado):** se registra el recuento (0) y la corrección queda cubierta por las pruebas automáticas de PB-TC. Es lo que hiciste en Smoke-2 con los casos que no existían: no fabricarlos.
- **B. Fabricarlo en una copia:** la sesión escribe el estilo en el sobre de un rack de una copia, con un guion, y después comprueba RACKEDITAR → «Actualizar». Prueba la corrección en AutoCAD, pero sobre un dato que el producto no puede producir.

**Si no hay respuesta:** A.

---

## Q-PBT5-3. Las otras correcciones declaradas de RACKEDITAR

**El contexto:**
- Además de D-PB-06 y D-PB-08, autorizaste otros tres cambios de RACKEDITAR: D-PB-11, H-31 y OWNER-1 de PB-TC2 (F4B-E06, F4B-E08, F4B-E09 y F4B-E12):
  - con «Restaurar estándar», «Actualizar» escribe lo restaurado;
  - seis cajas conservan el valor exacto al pasar por ellas;
  - en «Ambos lados», escribir el número que se ve iguala los dos lados.
- El cierre de F4B solo nombra D-PB-06 y D-PB-08 para el Smoke-PB.
- Probar las otras tres en AutoCAD son unos 20 a 30 minutos más, sobre copias, sin juicio visual tuyo ([P], SPB-09, SPB-10 y SPB-42).

**Las opciones:**
- **A. Sumarlas al Smoke-PB (Recomendado):** se comprueba en AutoCAD todo lo que cambió en RACKEDITAR. Si una falla, se ve antes de cerrar F4B.
- **B. Dejarlas en la evidencia automática:** el Smoke-PB solo mira D-PB-06 y D-PB-08, como dice el cierre.

**Si no hay respuesta:** A. Si después eliges B, esos tres resultados quedan solo como información y no cuentan para el cierre.

---

## Q-PBT5-4. Los riesgos del borrado real (OWNER-1 a OWNER-3 de PB-T3c)

**El contexto:**
- Cuando el panel borra un corte lateral que sobra, borra todas sus referencias:
  - también las que estén en una presentación (espacio papel);
  - también las que estén dentro de un bloque tuyo;
  - y no avisa.
- El clásico (RACKEDITAR) hace lo mismo.
- La revisión de PB-T3c lo dejó para que el Smoke-PB lo mirara (F4B-E48), y ningún dibujo real conocido trae un corte lateral así.
- La única referencia anidada que medimos en tus dibujos es de una planta, que no se borra.

**Las opciones:**
- **A. Construir los casos en una copia, con órdenes de AutoCAD (Recomendado):**
  - la sesión pone una segunda referencia del corte en una presentación y otra dentro de un bloque, con `-INSERT` y `-BLOCK` tecleados;
  - después borra con el panel, compara con RACKEDITAR y registra qué se borró y si hubo aviso;
  - los bloques dinámicos solo se miran si un dibujo real los trae.
- **B. Solo con lo que traigan los dibujos reales:** lo que no exista queda NOT AVAILABLE FOR PHYSICAL SMOKE.
- **C. Fuera del Smoke-PB:** quedan como riesgo informado, sin mirarlos en AutoCAD.

**Si no hay respuesta:** B, porque construir casos fuera de los A/B no está autorizado. A puede hacerse después como una adenda corta, cuando respondas.

---

## Q-PBT5-5. La orden de esta noche y la frontera de las 7:00

**Tu mensaje**, literal, con la zona horaria redactada:

> «En caso de que se complete antes de las 6 am y quieras ejecutar el smoke, ejecutalo tú ya que estaré dormido, a menos que sean después de las 7 am de `<zona-horaria-del-Owner>`»

**Cómo lo lee la sesión:**
- si el Smoke-PB está listo para empezar antes de las 7:00 de tu hora (las 13:00 UTC del 2026-11-23), lo opera la sesión con control de pantalla y tú firmas al revisar la evidencia;
- después de esa hora, se hace contigo;
- tu mensaje cuenta como autorización para emitir PB-T5 y operar el Smoke-PB, siempre que la revisión independiente del paquete no encuentre bloqueantes ni importantes y no aparezca ninguna decisión nueva;
- entre las 6:00 y las 7:00 se aplica lo mismo que antes de las 6:00.

**Las opciones para lo que no dice tu mensaje:**
- **A. Lo que lee la sesión, y si a las 7:00 está a medias, sigue hasta que escribas (Recomendado):** cuando escribas, la sesión para en el siguiente límite de paso y te pregunta si sigue ella o sigues tú.
- **B. Lo mismo, pero a las 7:00 para en el límite de paso y te espera,** aunque no hayas escrito.
- **C. Otra lectura:** la escribes tú.

**Si no hay respuesta:** A.

---

## Q-PBT5-6. Una revisión independiente de la evidencia antes de firmar

**El contexto:**
- Si opera la sesión, tú no ves la corrida: firmas sobre el registro, las capturas y los inventarios.
- En Smoke-2, la sesión operó desde el paso 30 y firmaste sobre su evidencia, sin otra revisión (F4-E79 y F4-E81).

**Las opciones:**
- **A. Firmo sobre la evidencia, como en Smoke-2 (Recomendado):** la sesión te deja un resumen paso a paso, con una sección «Para tu revisión» con los seis pasos que piden tu ojo, y las capturas.
- **B. Antes, una revisión independiente de solo lectura:** un revisor aparte comprueba que el registro, las capturas y los inventarios dicen lo mismo, antes de que firmes. Añade una o dos horas.

**Si no hay respuesta:** A.

---

## Información, no decisión: R-T4-39 (F4B-E60)

F4B-E60 manda contártelo en PB-T5, junto al paso 6 de la recarga. Ninguna de tus respuestas de PB-T4e dependía de esto.
- **El hecho:** con un borrador modificado y un cambio de fuera que reescriba el sobre de origen, «Actualizar» puede responder `NotAppliedPreflight`, que no se aplica, en lugar de ofrecer «Conservar mi borrador».
- **Lo que sigue en pie:** no se escribe nada sin avisar.
- **Lo que hará el Smoke-PB:** registra cuál de las dos respuestas sale en tu dibujo real ([P], SPB-24 y SPB-34). Si ningún cambio de fuera disponible lleva a «Conservar mi borrador», los pasos de «Conservar» (SPB-25 a SPB-28) quedan NOT AVAILABLE y te lo cuento con la evidencia.
