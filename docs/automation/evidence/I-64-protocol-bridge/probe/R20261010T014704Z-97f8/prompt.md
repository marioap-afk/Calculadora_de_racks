I-64 - SONDA MINIMA DE CELDA (solo lectura). RunId R20261010T014704Z-97f8.
Objetivo: medir esta celda (Codex CLI, modelo gpt-6-luna, effort high) con el runtime actual. No es una tarea de producto.
Ejecuta en el directorio de trabajo actual exactamente estas tres ordenes y devuelve sus salidas, sin interpretarlas:
  1. git rev-parse HEAD
  2. git rev-parse origin/main
  3. git rev-parse HEAD:docs/initiatives/I-64-A-1.md
Campos: HeadSha = salida de 1; OriginMainSha = salida de 2; AmendmentBlob = salida de 3; CommandsRun = las ordenes ejecutadas.
Reglas: solo lectura; no escribas archivos, no hagas commit ni push, no ejecutes otras ordenes. Salida solo en ASCII.
Informe esperado: la salida exacta del esquema rackcad-i64-cell-probe/v1 (la escribe el CLI por -o).
