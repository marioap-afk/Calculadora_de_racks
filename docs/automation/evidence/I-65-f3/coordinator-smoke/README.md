# I-65 F3 — simulacro del Coordinator de la plantilla de RAM Elements (NO es RAM Elements)

Evidencia de la sesión (Coordinator), fuera de la suite Core y fuera del producto. **No** verifica el comportamiento de RAM Elements ni
sustituye la OV de F4: comprueba que el script generado por el producto es Python válido y que su flujo de control (puertas, escritura,
lectura de vuelta, fallo cerrado) funciona contra un módulo `ramelements` **falso** que imita solo lo medido en la sonda P1 y lo declarado
por la fuente del paquete instalado.

- `refpkg-Program.cs`: genera con el producto (`RamBuildPackageFactory` y `StructuralPackagePublisher`, rama en `e3ae36d0`) el paquete de
  `REF-CANT-01` con filas de mapeo de **fixture** (no verificación de ingeniería) y lo escribe de forma atómica en una carpeta nueva.
- `fake-ramelements-__init__.py` y `fake-ramelements-engine.py`: módulo falso (regla de ejes medida con giro, valores por defecto de P1,
  miembro huérfano ante un nombre rechazado, máximo de 24 cargas, caso por defecto `CM`, las 207 funciones del núcleo de P1).
- `run_sim.py`: ejecuta el script generado contra el módulo falso e informa el estado del modelo falso al terminar.

| Escenario | Resultado |
|---|---|
| S0 `py_compile` del script generado | compila; 15 626 bytes; solo ASCII; sin CR |
| S1 modelo vacío, nombres conocidos | `READBACK OK`: 8 nudos, 7 miembros, 3 cargas, 0 diferencias; `ram-readback.json` escrito con la `AnalysisInputFingerprint` del paquete |
| S2 segunda corrida en la misma carpeta | `FileExistsError` → `FAILED — DISCARD MODEL`; no sobrescribe |
| S3 modelo no vacío | puerta: «the model has 1 nodes»; ninguna escritura en el modelo |
| S4 versión `0.2.0` | puerta: versión distinta; ninguna escritura |
| S5 nombre de sección desconocido | `ValueError` → `FAILED — DISCARD MODEL`; modelo parcial con miembro huérfano (por eso el procedimiento descarta el modelo) |
| S6 función del núcleo ausente (`members_add`) | puerta: función ausente; ninguna escritura |
