# Traspaso del autor de la v2.1 de PB-T5

**Encargo:** la v2.1 tras la recomprobación de la v2: «Recomprobación 1 (v2)» al final de `review-v1/independent-review-pbt5-v1.md` (sha256 del archivo entero `5e34ea79a3dd154ec338fccbe157a26930a84c97a5e4c6f8700eb04ef8cd7771`). Resultado: APTO TRAS CORRECCIONES, B 0 / I 1 / M 8. La corrida de esta noche: EJECUTABLE TRAS CORRECCIONES (I-07 y M-18).
- **Las reglas, las de siempre:** P-08, solo objetos git en `7da361eb`, sin AutoCAD, sin compilar, sin commits y solo en `S\pbt5\`.
- **La v2 no se sobrescribe:** la v2.1 son archivos nuevos.

## Lo entregado
- **`pre-run-tonight-v2.1.md`,** reescrito entero:
  - I-07 (§4.7 y §4.8);
  - M-18 (§4.3 y §4.4);
  - M-20 a M-22 y M-25 (§5.2);
  - las capturas (§8);
  - y lo no verificado de la recomprobación: el PID, `PICKFIRST` y `gh`.
- **`I-64-f4b-pbt5-package-v2.1.md` y `owner-questions-v2.1.md`,** generados desde la v2 por `_tools/make_v21.py`, con reemplazos exactos: cada texto viejo aparece una sola vez, y el guion no sobrescribe. Son 24 reemplazos en el paquete y 5 en las preguntas.
- **`_tools/control_patterns.py`,** reescrito (M-19): ya no lleva muestras literales.
  - Lee `_tools/control-samples.local.txt` y `_tools/patterns.local.txt`. **Ninguno de los dos se custodia.**
  - La versión anterior (`2822a688…61b1`, la que vio la recomprobación) queda sustituida.
- **`_tools/check_pbt5.py`:** analiza los archivos de la v2.1, `control_patterns.py`, `make_v21.py` y a sí mismo, y escribe `sha256-v2.1.txt`.

## Correcciones a la v2 de este traspaso
- **El punto 7 de `handoff-pbt5-v2.md` (M-20)** decía que la forma de la selección de Smoke-2 no constaba. Sí consta: «`sssetfirst` + `ssget` por nombre de bloque» (run-log-2, en la medición de `DBMOD` del paso 75). La de la v2.1, con `handent`, es equivalente para una referencia.
  - **El dato que faltaba:** esa selección, con el panel abierto, pone `DBMOD` = 1 por la vista previa. Por eso la línea de base se toma después.

## Discrepancias con la recomprobación
**Ninguna de fondo.** Lo aplicado de otra forma que la literal:

1. **I-07: la recuperación sin documentos no usa COM en ningún caso.**
   - La recomprobación admitía «COM solo si de verdad no queda documento, enganchado al PID de la sesión».
   - La v2.1 lo quita del todo: el encargo dice «nada de COM que pueda engancharse a un AutoCAD que no abrió la sesión», y atar COM a un PID no es seguro (la tabla de objetos en ejecución no lo garantiza).
   - Lo que hace en su lugar: cerrar la instancia propia por su PID con `CloseMainWindow` y relanzarla con la orden de §4.5 (§4.8).
2. **M-21, la parte dinámica:** para saber si un contenedor es dinámico, la v2.1 añade una función de lectura nueva, `rcdyn`, que busca `ACAD_ENHANCEDBLOCK` en el diccionario de extensión de su registro. No está probada.
   - Si su control falla, o el resultado no se puede fiar, SPB-47 queda LIMITATION.
   - Las cuatro funciones de [BM] siguen iguales byte a byte.
3. **El bloque opcional J (M-25)** solo empieza si quedan al menos 30 minutos para las 13:00 UTC. El umbral es de la sesión: el encargo pide «solo si queda tiempo», y el bloque necesita tres copias y unos cuantos gestos.
4. **Las copias pasan de 30 a 33** (c31 a c33, para el bloque J). Se crean todas en P-06, y las que no se usen se comprueban intactas en SPB-51.
5. **La comprobación de que las carpetas están fuera de todo repositorio:** la v2.1 añade, a la comparación con el worktree y el checkout principal, un `git -C <carpeta> rev-parse --show-toplevel` después de crearlas, para no depender de la lista de repositorios conocidos.

## Lo que sigue sin verificar
- Ninguna orden de PowerShell ni de LISP se ha ejecutado. Son texto, comprobado solo leyendo.
- Que `vla-close` con el nombre del documento (`vla-item … "cNN.dwg"`) funcione sobre un documento que no es el activo. [BM] lo hizo con la variable `d` de los documentos, no con `vla-item`.
- Que `EXTERNALREFERENCESCLOSE` no deje entrada de Deshacer que moleste. En Smoke-2 aparece en la lista de Deshacer («Externalreferencesclose»), **antes** de cualquier orden medida, así que no choca con N-I1.
