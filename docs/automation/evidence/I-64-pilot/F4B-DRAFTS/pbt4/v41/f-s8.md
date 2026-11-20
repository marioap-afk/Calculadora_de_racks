## 8. Las decisiones del Owner

### 8.1 Ya tomadas antes de PB-T4

Las de la v1 (§8.1): [OD] con D-PB-01 y D-PB-05; F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; F4B-E15, E21, E25, E35 Q4, E37, E42, E45 y E46; D-PB-10.

### 8.2 Las cuatro decisiones sobre PB-T4 (2026-11-20)

**El registro literal:** `owner-pbt4-decisions.md` (las preguntas con su texto exacto y las respuestas). La numeración F4B-E se asigna al custodiarlo, con la emisión de PB-T4a. La §15 de la revisión dio LISTO TRAS CORRECCIONES DE TEXTO (0/3/4), y sus correcciones de Q1, Q3 y Q4 estaban en el texto preguntado.

| Pregunta | Respuesta del Owner, literal | Lo que aplica la v4.1 | Lo que no se hace |
|---|---|---|---|
| **1, «Piloto»** | «Retirarlo (Recomendado)» | D-PBT4-10: se retira lo que solo usaba el panel; la carga se queda dentro del lector, que usa el modelo completo para AUTH-13; las pruebas propias y E-3, E-5 y E-6 de D-1 se sustituyen con registro en PB-T4b (C-7) | «Conservarlo sin uso» y «Dos modelos hasta F5-A» |
| **2, «Presentación»** | «Por grupos (Recomendado)» | D-PBT4-09, con los nueve nombres fijos, la unión, el plural y las líneas de lo estructural y de las vistas | «Por campos» y «Por campos con valores»: sin `PushBackFieldLabels` ni `PushBackPresentationForm` |
| **3, «Pruebas F4»** | «Saltar y duplicar (Recomendado)» | D-PBT4-11: las 428 se saltan y se duplican con un kit nuevo, como en D-1, con el mapa cerrado y el guion de CT-PBT4B-20; sin excepción a C02 | «Editar una línea» (sin excepción declarada a C02 ni edición de EIH) y «Implementación de referencia» |
| **4, «Emisión»** | «Sí, con condiciones (Recomendado)» | PB-T4a, PB-T4b y PB-T4c, en ese orden, cada una tras el cierre de la anterior, sin volver al Owner si la comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, la remedición no cambia nada material y la lista emitida es la que vio el Owner o una parte de ella | «Solo PB-T4a» y «Cada subtarea me vuelve» |

**Vuelve al Owner** (pregunta 4, literal): «por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista». Si eso pasa en mitad de una subtarea, el Worker para (C16 y las paradas de §6).

**La cota que vio el Owner** (pregunta 4): «1 en PB-T4a; en PB-T4b, unas 465 si se saltan y se duplican […], más unas 100 si retiras el código del piloto; y 2 en PB-T4c». Las listas guardadas y sus números exactos, frente a esa cota, en §8.3.

### 8.3 Las listas guardadas frente a la cota (I-22, I-23; V41-03)

{{COTA_TABLE}}

**Lo declarado, sin preguntar** (como en la v4):
- que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro;
- el límite del testigo (M-20);
- el cambio observable de PB-T4a (M-23);
- que la validación numérica pasa de la UI a Application, sin cambiar su comportamiento (D-PBT4-06);
- **que la firma de `RackPilotIntentAdapter.Read` no cambia** y la relectura en el origen es pura, sobre el mismo escaneo (D-PBT4-03; V41-06).

### 8.4 Lo que no es decisión

Lo de la v3 (§8.3): el orden de las subtareas, los nombres, las CT y las guardas medidas. Además:
- **el abridor en dos piezas;**
- **la validación numérica movida a Application,** que no cambia el comportamiento. Si la comprobación acotada o la precondición 6 muestran una guarda de T1B que lo impide, vuelve al Owner por OB-T1B.3;
- **`AtOrigin` en lugar de un parámetro nuevo de `Read`:** lo impone una guarda medida (EIG:139-141), sin cambio de comportamiento frente a lo que vio el Owner.

