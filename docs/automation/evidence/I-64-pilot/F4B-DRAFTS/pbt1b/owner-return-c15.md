# I-64 F4B PB-T1b: vuelta al Owner por la parada PBT1B-C15

**Fecha:** 2026-11-18. La prepara la sesión principal.

## Los hechos

**El Worker** `R20261118T193423Z-fb01` es la reanudación de la delegación `c45c`, en el intento 1. Terminó **BLOCKED / STOP PBT1B-C15**, antes del RED:
- no hizo ningún commit ni ningún push, y el árbol quedó limpio en la BaseSha `7905b3a9`;
- su borrador está en `uncommitted-work-at-stop.patch`;
- el paso 1 salió idéntico al de antes de la pausa (c-02 en la rama ordinal y c-09 en la rama «conserva»).

**La parada.** La mitad estado → diseño → estado de CT-PBT1B-02 da «distintos» en `pb-15-sin-composite`, un rack antiguo que no guarda la parte compuesta.
- **Lo que difiere:** solo una cosa del estado, que el frente 0 del lado B pasa de activo a inactivo tras la primera ida y vuelta.
- **Por qué no se ve:** el lado B no está presente (`SideBPresent = False` en los dos lados), así que ese frente no se dibuja ni se usa.
- **La causa:** es la normalización que ya hace la ventana de hoy al escribir. El pilot ya la tiene registrada para `pb-15` en `LegacyDeltas`: «`$.PushBack.Composite.AbsentSlotsB[0]: <absent> -> 0`».
- **Lo que sí sale bien:** la otra mitad (diseño → estado → diseño) da exactamente las líneas esperadas en los 18 diseños, y los otros 17 son iguales en esta mitad.
- **Lo que no dice el contrato:** CT-02 no preveía este caso. PBT1B-C15 manda parar y volver al Owner por OB-06.5. El Worker lo hizo así, sin elegir una salida.

**Dos cosas más de la entrega:**
- **CT-PBT1B-06** (el censo, «cada campo E sobrevive a `Copy`»). Tal como lo leyó el Worker (mutar cada campo por reflexión), da falsos fallos en colecciones que el editor vuelve a sincronizar al copiar y en las piezas de seguridad, que se normalizan. Su texto no es lo bastante preciso. Es una aclaración del paquete, no una decisión del Owner.
- **El orden del trabajo.** El Worker escribió la implementación completa antes del commit RED. No hizo ningún commit, así que no tuvo efecto en la rama. Es una desviación declarada.

## Las decisiones

**1. pb-15: ¿pérdida nueva o normalización conocida?**
- **(a) Normalización conocida: se corrige CT-02.**
  - La mitad estado → diseño → estado se mide desde el estado ya normalizado por una primera ida y vuelta. Para los seis racks antiguos, la diferencia de la primera vuelta tiene que ser exactamente la de `LegacyDeltas` vista desde el estado.
  - El paquete pasa a una v2.7, que además aclara CT-06. Se hace su comprobación acotada.
  - Después, una delegación nueva. RACKEDITAR no cambia.
- **(b) Tratarlo como pérdida y corregirlo:** sería cambiar la normalización de la ventana, fuera del alcance de PB-T1. Haría falta una tarea propia de corrección declarada.
- **(c) Parar PB-T1b.**

**2. El presupuesto.**
- **(a) Sin gastar intento.** La parada viene de un hueco del contrato y el Worker la respetó sin escribir producto. La delegación nueva sigue en el intento 1, como la parada C14 de PB-TC2 (D1).
- **(b) Contarla como el intento 2 de 3.**
