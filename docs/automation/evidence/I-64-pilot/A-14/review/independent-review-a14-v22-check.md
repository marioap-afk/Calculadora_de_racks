# Comprobación acotada de I-64 A-14, borrador v2.2

## 0. Identidad, modo y método

- **Objeto:** `S/I-64-A-14-draft-v2.2.md`, SHA-256 `89670c2f76c31c4fc1c8f9523feb8f9c97e30a7f5eb6ef7c9ed98826e3b695f5` (1066 líneas). Se cita
  «v2.2:n».
- **Disposición:** `S/a14-v22-disposition.md`, SHA-256 `3693ca3d…40ca`.
- **Notas:** `S/a14-drafting-notes.md` §12, SHA-256 `4b57b1aa…985d`.
- **Decisiones del Owner:** `S/owner-a14-decisions.md`, sin cambios (`f1d0ea5d…4c2e`).
- **Hallazgos que se comprueban:** los de mi comprobación de la v2.1, `S/independent-review-a14-v21-check.md` (`be9d9659…6913`).
- **Método:**
  - diff completo de la v2.1 a la v2.2;
  - lectura del texto cambiado y de su contexto.
- **Lecturas nuevas en W (solo lectura, HEAD `fa59bd82`):**
  - los campos «Corrects / supersedes», «Protocol-owner decision», «Authority» y «Status» de A-1..A-7;
  - `f1_a4_status` y `f1_a6_status` en `I-64.yml`.
- **Modo:** SEPARATE SESSION, el mismo revisor que las comprobaciones de la v2 y la v2.1.
  - Sin escrituras en W ni órdenes git que cambien estado.
  - Sin AutoCAD, sin compilar, sin pruebas.
  - Cada estado se decidió en el texto, no en la tabla.

## 1. Veredicto

**EMITIBLE A ARCHITECT.**

| Hallazgos de la v2.1 (7: residuo de N-01, N2-01..N2-06) | Resueltos | Parciales | Sin resolver |
|---|---|---|---|
| | 7 | 0 | 0 |

| Hallazgos nuevos de la v2.2 | Bloqueantes | Importantes | Menores |
|---|---|---|---|
| | 0 | 0 | 4 |

Los cuatro menores son precisiones de una línea:
- no cambian el significado;
- el Architect los trataría como `OPTIONAL` (LIFECYCLE §5 `:169`);
- conviene aplicarlos antes del commit del paso 3 si cuesta poco, pero no bloquean la emisión.

## 2. Los hallazgos de la v2.1, comprobados en el texto

| Id | Estado | Dónde (v2.2) | Comprobación |
|---|---|---|---|
| N-01 (residuo) y N2-01 | **RESUELTO** | Cabecera (v2.2:27-32); C-12 (v2.2:566-567, 623-625); C-13 (v2.2:634-635); §8 (v2.2:859-860); §11, P-1 (v2.2:956-961); §12, filas 3 y 3b | Solo A-7 cambia de alcance, y la cabecera nombra el delta exacto: «el `Applies-to: I-64` … se amplía … solo en su alcance». Ya no dice «Ninguna A-n anterior». A-1..A-6 y A-8..A-13 siguen siendo de la unidad I-64. Comprobado en W: los seis puentes de Scope y FreeText llevan «Protocol-owner decision: SATISFIED only for this I-64 unit-scoped exception» (A-2, A-3, A-4 `:33`; A-5 `:35`; A-6 `:33`), satisfecho por la orden del Owner. Las cadenas de activación son A-1 → A-2 → A-3 → A-4 y A-5 → A-6, así que A-4 y A-6 son los vigentes, como dice P-1. `I-64.yml` registra A-4 AGREED y A-6 «AGREED y ACTIVA … solo I-64». No queda ninguna mención de la extensión amplia (grep de «A-1..A-13») |
| N2-02 | **RESUELTO** | C-12 (v2.2:607-612); autoridad de C-12 (v2.2:623-624); §12, fila 16 y pendientes (v2.2:1023, 1039); §11 (11) y punto 12 | En los tres sitios señalados se lee la misma fórmula: «el Owner, a propuesta del Coordinator». Dice «sobre el Candidato del tramo», con §12.5 `:902`. En lenguaje llano, la prueba «completa, con tus dibujos reales». El residuo en un cuarto sitio que no cité es el menor N3-01 |
| N2-03 | **RESUELTO** | C-12, tabla de pros y contras (v2.2:594-600); §6, fase 4; punto 12 | Las dos opciones aparecen con sus costes. La propuesta retrasa la primera integración y envejece la rama, contra Q-09: la cita literal «Usas antes lo nuevo y la rama no envejece» es exacta. La alternativa añade un tramo. En §6, la fase 4 «es la última fase del tramo 1, que cierran su READY y su Owner Validation» |
| N2-04 | **RESUELTO** | C-12 (v2.2:602-605); firma (11) (v2.2:947-948) | Los dos sitios dicen «antes del READY-01 del primer tramo afectado». Para el tramo 1, que ya está abierto, sus fases siguen y lo que espera es su READY-01 |
| N2-05 | **RESUELTO** | §10, pasos 1 y 3; §13 | «Todas las revisiones y comprobaciones independientes … y sus tablas de disposición», con LIFECYCLE §5 `:162-163` |
| N2-06 | **RESUELTO** | §5, tramo 2 (v2.2:774) | «Si no, el tramo y los OV de Variables los fija su A-n (C-11)» |

## 3. Las decisiones nuevas del redactor

| Decisión | ¿Fuente? | Quién decide |
|---|---|---|
| **Ampliar A-7 a las unidades nuevas, con la autoridad de A-7** (Architect + Coordinator; Owner informado) | **Sí.** El propio A-7 la da: «Authority: Architect + Coordinator» y «Owner: NOT REQUIRED … pero se le informa, porque Smoke-1 es un hito del Owner». También LIFECYCLE §6 `:205-206` (M material: Architect + Coordinator) y §8 (reasignar exige A-n). No es `OWNER-RESERVED`: no toca alcance, no-objetivos ni decisiones del Owner. La pide C-13, porque INV-24 y S2-16..S2-22 alcanzan a los editores de los tramos 3 y 4 | **Architect + Coordinator** (§12, fila 3). Al **Owner se le informa**, pero el texto no lo dice ni lo hace: N3-04 |
| **P-1, como pregunta fuera de la frase de la firma**, con valor por defecto «no» | **Sí** para el valor por defecto. Los puentes son de la unidad I-64 por la decisión de protocolo que satisfizo la orden del Owner (A-2..A-6, campo «Protocol-owner decision»), y A-14 no los toca. Para ampliarlos, la vía también tiene fuente: la A-n siguiente, con el Owner y el dueño del protocolo, como en A-2..A-6. **Preguntarlo ya** no lo pide ninguna fuente, y la respuesta no cambia A-14 | **Owner**, que en A-2..A-6 actuó como dueño del protocolo con su orden, + Coordinator + Architect, por la A-n siguiente. Que la pregunta vaya fuera de la frase es correcto, porque no altera la elección exacta. Recomendación en N3-03 |
| La autoridad de Smoke-PERF por tramo: «el Owner, a propuesta del Coordinator» | Sí: §12.5. El Owner ejecuta los dibujos reales y acepta el rendimiento; el Coordinator propone. Q-09: «tu validación» | **Owner** |
| Si el tramo afectado es el 1, sus fases siguen y espera su READY-01 | Sí: LIFECYCLE §8. La A-n tiene que estar antes de fijar la asignación de OV, es decir, antes de READY-08, y READY-01 es más conservador | **Coordinator** |

## 4. Hallazgos nuevos (menores)

| Id | Cita (v2.2) | Fuente | Corrección |
|---|---|---|---|
| N3-01 | En §4, la fila de §12.5 (v2.2:676) aún tiene como autoridad «Coordinator + Owner (aceptación de §12.5 y regla por tramo)» | En los demás sitios, «el Owner, a propuesta del Coordinator». La disposición dice «una sola redacción en todos los sitios» | Escribir la misma fórmula en esa fila |
| N3-02 | Punto 12 del lenguaje llano (v2.2:989): «una sesión tuya en AutoCAD por tramo, cinco con la propuesta» | Cada tramo lleva además los smokes de sus fases y su Owner Validation (§6; C-12) | «Una sesión de rendimiento por tramo, además de los smokes y de la validación de cada tramo». Si no, el Owner puede leer que es su única sesión de host por tramo |
| N3-03 | P-1 (v2.2:956-961 y 998-999) | La respuesta no cambia A-14. Ampliar un puente va igual por la A-n siguiente, y la necesidad solo se conocerá cuando un tramo la tenga (notas §12.3, punto 2: «puede hacer falta … pero no lo decido»). En lenguaje llano, «Scope» y «FreeText» siguen sin glosa | Convertir P-1 en una nota informativa («A-14 no los amplía; si un tramo los necesita, se te pedirá con su A-n»). Si se mantiene como pregunta, glosar los dos atajos en una frase. Es opcional |
| N3-04 | Cabecera (v2.2:29-30) y §12, fila 3 (v2.2:1009): la ampliación de A-7, con «la autoridad de A-7» | La autoridad de A-7 incluye informar al Owner (A-7: «Owner: NOT REQUIRED … pero se le informa»). Ni la firma, ni el lenguaje llano, ni la fila 3 lo recogen | Añadir «Owner informado» a la fila 3, y una línea llana, por ejemplo en el punto 12: «las pruebas de teclado y foco de los editores valen también en los tramos siguientes» |

## 5. La firma, versión final (§11, v2.2:923-999)

- **Exacta:** sí.
  - La frase tiene 13 materias, cada una con su C-nn.
  - La materia (7) cita literalmente lo que se sustituye de D-PB-01.
  - La materia (9) dice ahora «la lectura de la sesión que confirmo» e incluye la ruta de la biblioteca de bloques desde la fase 1.
  - La materia (11) incluye el Smoke-PERF completo «con mis dibujos reales» y el momento correcto de la A-n («antes del READY-01»).
  - El párrafo de después distingue bien entre elegir hitos y rechazar una materia.
- **Completa:** sí, para todo lo `OWNER-RESERVED`.
  - La ampliación de A-7 no es del Owner, así que no tiene por qué estar en la frase, aunque se le debe informar (N3-04).
  - La extensión de puentes queda fuera de A-14 y fuera de la frase, como corresponde.
- **Entendible para el Owner:** sí.
  - Los 14 puntos son fieles a la frase.
  - Los puntos 9 y 10 van marcados «(Confirmas una lectura nuestra.)».
  - El punto 5 lleva el Deshacer y el progreso.
  - El punto 12 da la carga de host y las dos opciones con sus costes.
  - Quedan dos ajustes: N3-02, y la glosa o nota de P-1 (N3-03).
  - Un detalle sin recuento: el punto 10 incluye «los archivos que tú exportes o guardes» entre lo que se guarda «en tus ajustes», pero esos
    archivos van donde el usuario elija. La frase exacta, materia (9), es correcta.

## 6. Comprobaciones que pasaron

- El diff de la v2.1 a la v2.2 se limita a lo dispuesto: cabecera, C-12, C-13, §5 (tramo 2), §6 (fase 4), §8, §10, §11, §12 y §13. Ningún
  otro cambio.
- Las citas nuevas son exactas:
  - A-2/A-3/A-4 `:33`, A-5 `:35` y A-6 `:33` (en el campo «Protocol-owner decision»);
  - §12.5 `:902`;
  - la frase de Q-09;
  - LIFECYCLE §5 `:162-163`.
- Las decisiones del Owner siguen recogidas tal cual, sin ampliaciones ni recortes nuevos.
- **Publicabilidad:** sin rutas locales, nombres de personas, correos ni datos de clientes en la v2.2, su disposición ni la comprobación de
  la v2.1.
