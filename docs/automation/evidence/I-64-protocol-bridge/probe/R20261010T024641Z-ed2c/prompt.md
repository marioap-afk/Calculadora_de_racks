I-64 - SONDA MINIMA DE CELDA (solo lectura). RunId R20261010T024641Z-ed2c.
Objetivo: medir la elegibilidad de esta celda (Codex CLI, modelo gpt-6.1-sol, effort high, solo lectura) para una revision del Architect. No es una tarea de producto.
Ejecuta en el directorio de trabajo actual exactamente estas tres ordenes y devuelve sus salidas, sin interpretarlas:
  1. git rev-parse HEAD
  2. git rev-parse origin/main
  3. git rev-parse HEAD:docs/initiatives/I-64-A-2.md
Campos: HeadSha = salida de 1; OriginMainSha = salida de 2; AmendmentBlob = salida de 3; CommandsRun = las ordenes ejecutadas.
Reglas: solo lectura; no escribas archivos, no hagas commit ni push, no ejecutes otras ordenes. Salida solo en ASCII.
Informe esperado: la salida exacta del esquema rackcad-i64-cell-probe/v1 (la escribe el CLI por -o).
