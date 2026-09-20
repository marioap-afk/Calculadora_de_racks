# Variables de proyecto y fórmulas

Las variables de proyecto pertenecen al dibujo completo. Un rack vinculado toma de ellas su valor efectivo;
el literal que tenía antes de vincularse queda congelado como respaldo explícito. La autoridad se guarda dentro
del DWG y se recupera al abrirlo de nuevo.

## Administrar variables con `RACKVARIABLES`

Ejecuta `RACKVARIABLES` —alias `RVA`— para abrir **Variables de proyecto**. Cada variable muestra su nombre,
tipo, definición, valor evaluado y los racks que la consumen.

- Escribe un número para guardar una definición literal.
- Empieza con `=` para guardar una fórmula, por ejemplo `=Holgura General + 2`.
- Las variables actuales son de longitud. Los números, resultados y diagnósticos visibles se expresan en
  **pulgadas (`in`)**, la unidad geométrica interna de RackCad.
- Renombrar conserva la identidad estable de la variable y no rompe las expresiones ya enlazadas.
- Eliminar una variable usada está bloqueado. **Desvincular todos y eliminar** exige confirmación y conserva en
  cada rack su valor efectivo actual.

Las fórmulas pueden depender de otras variables y usar las funciones que ofrece el editor. RackCad calcula el
grafo completo antes de guardar: una referencia desconocida, un nombre ambiguo, un ciclo, una dependencia que
falló, una expresión fuera de límites o un resultado inválido se muestran como diagnóstico y no sustituyen la
definición confirmada.

## Referencia directa y fórmula en `RACKEDITAR`

Los campos vinculables aceptan tres fuentes:

| Entrada confirmada | Fuente guardada | Efecto |
|---|---|---|
| `6` | Literal | El rack conserva su propio valor. |
| `=Holgura General` | Referencia directa | La propiedad sigue esa variable por su identidad estable. |
| `=MAX(Holgura General, Holgura Mínima) + 2` | Fórmula | La propiedad guarda una expresión enlazada a sus dependencias. |

Una referencia directa se reconoce solo cuando la expresión es exactamente un nombre único. Una operación,
función o combinación de variables se guarda como fórmula. Si hay nombres repetidos, el editor exige elegir la
opción calificada que identifica la variable exacta.

Escribir no cambia todavía la fuente. **Enter** valida y confirma; **Escape** restaura lo confirmado; perder el
foco no convierte un literal o una referencia en fórmula. El estado del campo muestra la fuente, el texto
canónico y el valor efectivo. El literal congelado permanece visible para explicar qué valor recuperaría el rack
al retirar el vínculo.

## Persistencia, propagación y recuperación

- Las definiciones y sus identidades sobreviven a guardar, cerrar y reabrir el DWG.
- `RACKEDITAR`, geometría y BOM consumen el mismo valor efectivo. Cambiar una dependencia propaga el resultado a
  sus consumidores en una operación atómica y con una sola regeneración del dibujo.
- `RACKDUPLICAR` conserva los vínculos a las mismas variables del dibujo. La exportación a biblioteca materializa
  valores y no transfiere el registro entre dibujos.
- Una lectura ilegible, referencia rota, ciclo o cambio concurrente falla cerrado: RackCad no inventa cero, no
  recorta el resultado y no reemplaza silenciosamente la última definición confirmada.
- **Referencias rotas** en `RACKVARIABLES` permite reparar explícitamente quitando los vínculos reparables del rack.
  La acción exige confirmación y deja gobernando el literal almacenado; la geometría puede cambiar.

Las referencias entre propiedades de racks distintos, la transferencia o fusión de variables entre dibujos y
las fórmulas de Custom BOM no forman parte de esta capacidad.
