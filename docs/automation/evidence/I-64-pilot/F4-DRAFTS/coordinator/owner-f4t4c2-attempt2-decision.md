# I-64 F4-T4C2-PANEL: decisión del Owner sobre la corrección en el intento 2 (2026-11-15)

**Contexto:** el Worker del intento 1 (`R20261115T190629Z-9b55`) hizo el RED `a6511c04` y se paró antes del GREEN (BLOCKED), por tres defectos de pruebas que él mismo escribió en el RED. El producto completo quedó en un parche de la corrida. La decisión D3 del Owner autorizaba la corrección sin consultar solo para F4-T4C1.

La sesión preguntó en el chat (AskUserQuestion). Se transcriben la pregunta y la respuesta literales.

**Pregunta:** «F4-T4C2-PANEL: el Worker hizo el RED (a6511c04, con la CI en rojo en Core y UI, como se esperaba) y se paró antes del GREEN. Tres de las pruebas que él mismo escribió en el RED son defectuosas: dos de vista esperan algo que el diseño no da (una vista previa que nunca ocurre y un registro de 4 llamadas donde F-1 añade una quinta), y una introduce un aviso xUnit nuevo. La regla 16.8 le impide tocarlas tras el RED. El producto completo quedó en un parche y, aplicado en local, deja verde todo menos esas dos pruebas. Tu D3 autorizaba corregir solo en T4C1. ¿Cómo sigo con T4C2?»

**Respuesta:** «Corregir en intento 2 (Recommended)».

**La descripción de la opción:** «Como D3 para T4C1: una delegación de corrección (2 de 3) que parte del RED, corrige exactamente esas tres pruebas y aplica el producto del parche como GREEN. Con análisis de causa raíz, CorrectionOf, RED acreditado, las dos revisiones, la verificación y nc1-nc3, y cierre por la regla de D2. Nunca el intento 3 sin ti.»

**La opción que no se eligió:** «Pausar T4C2».
