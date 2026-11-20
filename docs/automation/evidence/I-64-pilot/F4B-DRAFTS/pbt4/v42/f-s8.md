## 8. Las decisiones del Owner

### 8.1 Ya tomadas antes de PB-T4

Las de la v1 (§8.1): [OD] con D-PB-01 y D-PB-05; F4B-E28 Q3; F4B-E38; F4B-E29 (D-1); F4B-E11 (D2) con L-5; F4B-E24; la nota del Owner sobre la interfaz; F4B-E03; F4B-E15, E21, E25, E35 Q4, E37, E42, E45 y E46; D-PB-10.

### 8.2 Las decisiones sobre PB-T4 (2026-11-20)

**Los registros literales:** `owner-pbt4-decisions.md` (las cuatro primeras) y `owner-pbt4b-retire-list-decisions.md` (las dos de la vuelta de PB-T4b, redactadas en la §16.5 de la revisión). La numeración F4B-E se asigna al custodiarlos, con la emisión de PB-T4a. Las preguntas se numeran como en esos registros.

**La primera vuelta** (`owner-pbt4-decisions.md`):

| Pregunta | Respuesta del Owner, literal | Lo que aplica la v4.2 | Lo que no se hace |
|---|---|---|---|
| **1, «Piloto»** | «Retirarlo (Recomendado)» | D-PBT4-10: se retira lo que solo usaba el panel; la carga se queda dentro del lector, que usa el modelo completo para AUTH-13. **Cuándo,** lo dice la segunda vuelta: en PB-T4d | «Conservarlo sin uso» y «Dos modelos hasta F5-A» |
| **2, «Presentación»** | «Por grupos (Recomendado)» | D-PBT4-09, con los nueve nombres fijos, la unión, el plural y las líneas de lo estructural y de las vistas | «Por campos» y «Por campos con valores»: sin `PushBackFieldLabels` ni `PushBackPresentationForm` |
| **3, «Pruebas F4»** | «Saltar y duplicar (Recomendado)» | D-PBT4-11: las 428 se saltan y se duplican con un kit nuevo, como en D-1, con el mapa cerrado y el guion de CT-PBT4B-20; sin excepción a C02 en PB-T4b | «Editar una línea» (sin excepción declarada a C02 ni edición de EIH) y «Implementación de referencia» |
| **4, «Emisión»** | «Sí, con condiciones (Recomendado)» | PB-T4a, PB-T4b y PB-T4c, en ese orden, cada una tras el cierre de la anterior, sin volver al Owner si la comprobación acotada da 0 bloqueantes y 0 importantes sin resolver, la remedición no cambia nada material y la lista emitida es la que vio el Owner o una parte de ella. **No cubre PB-T4d** | «Solo PB-T4a» y «Cada subtarea me vuelve» |

**La segunda vuelta** (`owner-pbt4b-retire-list-decisions.md`):

| Pregunta | Respuesta del Owner, literal | Lo que aplica la v4.2 | Lo que no se hace |
|---|---|---|---|
| **1, «Retirada»** | «Retirar tras PB-T4c (Recomendado)» | D-PBT4-10 y §6.4: el código del piloto queda sin uso en PB-T4b y PB-T4c, y lo retira **PB-T4d**, después de PB-T4c, que sustituye las 139 y borra con registro las 55 originales saltadas y los 29 ayudantes, y **vuelve al Owner con su lista antes de emitirse** | «Retirar en PB-T4b» y «Conservarlo sin uso» |
| **2, «Lista T4b»** | «Sí, esa lista (Recomendado)» | D-PBT4-11: la lista de PB-T4b son las **470** de `pbt4b-substitution-list.tsv` (sha256 `44393b19…63e4`), que el Owner toma como «la que vio»; PB-T4b se emite con ella por la autorización condicional, si se cumple el resto de la condición | «Vuelve antes de emitir» |

**Vuelve al Owner** (pregunta 4, literal): «por una decisión nueva, un control con una firma no prevista, un intento 3, un efecto material sin medir o una prueba que falle fuera de la lista». Si eso pasa en mitad de una subtarea, el Worker para (C16 y las paradas de §6). **Y PB-T4d, siempre, con su lista.**

### 8.3 Las listas guardadas y lo que el Owner aprobó (I-22, I-23, I-26, M-32, M-33)

{{COTA_TABLE}}

**Lo declarado, sin preguntar** (como en la v4):
- que pedir «Conservar» lee el dibujo, frente a la regla de F4 de la UI :780-793, con su registro;
- el límite del testigo (M-20);
- el cambio observable de PB-T4a (M-23);
- que la validación numérica pasa de la UI a Application, sin cambiar su comportamiento (D-PBT4-06);
- **que la firma de `RackPilotIntentAdapter.Read` no cambia** y la relectura en el origen es pura, en `Draft/`, sobre el mismo escaneo (D-PBT4-03; V41-06; B-08);
- **que, entre PB-T4b y PB-T4d, la regla de la escala queda también en `PushBackPilotFieldAdapter`,** sin uso (D-PBT4-10).

### 8.4 Lo que no es decisión

Lo de la v3 (§8.3): el orden de las subtareas, los nombres, las CT y las guardas medidas. Además:
- **el abridor en dos piezas;**
- **la validación numérica movida a Application,** que no cambia el comportamiento. Si la comprobación acotada o la precondición 6 muestran una guarda de T1B que lo impide, vuelve al Owner por OB-T1B.3;
- **`AtOrigin` y `ReadAt` en lugar de un parámetro nuevo de `Read`:** lo imponen dos guardas medidas (EIG:139-141 y EIG:350-364), sin cambio de comportamiento;
- **la partición de las copias por su resultado en el RED** (§5.2; la T-1 de la §16.4), técnica.

