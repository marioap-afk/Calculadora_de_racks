# I-64 F4B PB-T1b: segunda vuelta al Owner por PBT1B-C08 (el contrato ya no cabe en la salida del Controller)

**Fecha:** 2026-11-18. La prepara la sesión principal.

## Los hechos

**El contrato reemitido con la v2.7** (X2 `5e52aaa3`, BASE2 `4b0498e3`) mide unos 56 KB: 52 autoridades, 213 entradas prohibidas, 33 paradas y 15 invariantes. La planificación tiene que copiarlo entero en la delegación.

| Corrida | Resultado |
|---|---|
| Delegación anterior, `aaac` | Tres errores de copia |
| Delegación anterior, `c45c` | Faltaban las 17 últimas entradas prohibidas. La sesión puso la lista (F4B-E18, solo en esa delegación) |
| Esta delegación, `4000` | Salida de 26,7 KB con Invariants, StopConditions y ExpectedEvidence **vacías** y 17 de las 52 autoridades. A1-A8: solo A5 en fail (la copia) |
| Replanificación única, `a60d` | **Todas las colecciones vacías.** El Controller lo dice él mismo: «la respuesta generada desde el contrato excede el limite de salida disponible y se trunca, por lo que no puedo garantizar la copia literal de las colecciones opacas» |

**La regla** (PBT1B-C08): el segundo rechazo de fidelidad es STOP y vuelve al Owner.

**El diagnóstico:** ya no es un descuido de copia, sino un límite del tamaño de la salida del Controller. Los contratos de F4B crecen con cada subtarea (PB-T1c y PB-T2 serán más grandes), así que el problema va a repetirse. En todas las corridas, el trabajo propio de la planificación salió bien:
- la clase de la tarea;
- las dimensiones;
- la celda y el enrutamiento;
- los criterios de aceptación.

## Opciones

- **(A) La sesión pone las colecciones opacas, desde PB-T1b y en el resto de F4B.**
  - El Controller deja de copiar las ocho colecciones opacas del contrato (Authorities, Objective, AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence y StopConditions). Las deja vacías y la sesión las pone byte a byte desde el contrato, de forma determinista.
  - El Controller sigue haciendo su trabajo: la clase, las dimensiones, la celda, el enrutamiento y los criterios de aceptación. Esos criterios los sigue redactando él, contrastados con el contrato.
  - La comparación byte a byte se hace igual sobre la delegación final, que tiene que dar PASS. A1-A8 y nc4 siguen igual.
  - Se declara en cada relevo.
  - Para PB-T1b hace falta una planificación nueva con ese prompt. No gasta intento.
- **(B) Lo mismo, pero solo en PB-T1b.** En PB-T1c y en las siguientes se vuelve a intentar la copia, y se vuelve al Owner si falla.
- **(C) Parar** y rediseñar el formato del contrato para que sea más corto (por ejemplo, sacando los textos largos a archivos referenciados). Es un cambio mayor.
