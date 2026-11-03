# Disposiciones del Coordinator de I-65 sobre la entrega R20261011T073033Z-b3ec (F3-T2), previas a su verificación

Texto literal del Coordinator. Se registrará en `docs/automation/decisions/I-65.md` §20 con la custodia, **después** de la verificación,
para no mover `HEAD` de `CurrentSha` (`e3ae36d0`).

1. **CD-I65-F3-12 — uso de Python declarado por el Worker (DEV-I65-F3-02).** El Worker ejecutó una vez un intérprete Python para volcar a
   texto `docs/automation/evidence/I-65-f0/p1-run/p1-report.json` (lectura de evidencia). C-08 se activa cuando **hace falta** Python o
   RAM Elements para compilar o probar, o hace falta ejecutarlos en el trabajo; no fue el caso: ni el producto ni las pruebas dependen de
   Python (la suite Core corre en ubuntu-latest sin él) y la plantilla no se ejecutó. El Coordinator lo clasifica como **desviación de
   proceso no bloqueante**; **C-08 no se activa**.
2. **CD-I65-F3-13 — nombre del tipo del escritor.** Se acepta `StructuralPackagePublisher` en el archivo
   `src/RackCad.Application/StructuralAnalysis/StructuralPackageWriter.cs` (la ruta que fija el contrato). La guarda de F1 que prohíbe
   «Writer» en los nombres de tipo de la capa neutral no se toca ni se debilita.
3. **CD-I65-F3-14 — lecturas del Worker aceptadas como no materiales:** «filas verified para todas sus secciones» = las secciones que usa
   el modelo; versión del producto **declarada** `26.00.01.015` (de la evidencia de P1; declarada, no verificada); validación de la
   carpeta más estricta (caracteres de control y reservados de Windows); tabla cuyo hash de filas usadas difiere del del modelo →
   `Invalid` (`RAM-PACKAGE-MAPPING-STALE`). Limitaciones aceptadas para F4: la lectura de vuelta no compara la categoría del caso (forma de
   lectura no medida) ni relee los nudos de cada miembro (la geometría se comprueba por coordenadas, ejes locales y giro).
4. **Simulacro del Coordinator** (fuera de la suite y del repositorio; NO es RAM Elements ni evidencia de su comportamiento): el paquete de
   `REF-CANT-01` generado por el producto con filas de fixture compila con `py_compile`, es solo ASCII y sin CR; ejecutado contra un módulo
   `ramelements` falso que imita lo medido en P1 (regla de ejes con giro, valores por defecto, miembro huérfano ante nombre rechazado, 207
   funciones del núcleo) da `READBACK OK` (8 nudos, 7 miembros, 3 cargas, 0 diferencias); una segunda corrida no sobrescribe
   (`FileExistsError` → `FAILED — DISCARD MODEL`); modelo no vacío, versión distinta o función del núcleo ausente → la puerta falla sin
   escribir en el modelo; nombre de sección desconocido → `FAILED — DISCARD MODEL`.
