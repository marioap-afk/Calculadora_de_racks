I-64 - SONDA MINIMA DE CELDA (solo lectura). RunId R20261011T035128Z-515d.
Objetivo: medir la elegibilidad de esta celda (Codex CLI, modelo gpt-6-luna, effort high, solo lectura) para la planificacion y la verificacion de F1-T3-HOST. No es una tarea de producto.
Ejecuta en el directorio de trabajo actual exactamente estas tres ordenes y devuelve sus salidas, sin interpretarlas:
  1. git rev-parse HEAD
  2. git rev-parse origin/main
  3. git rev-parse HEAD:docs/automation/state/I-64.yml
Campos: HeadSha = salida de 1; OriginMainSha = salida de 2; StateBlob = salida de 3; CommandsRun = las ordenes ejecutadas.
Reglas: solo lectura; no escribas archivos, no hagas commit ni push, no ejecutes otras ordenes. Salida solo en ASCII.
Informe esperado: la salida exacta del esquema rackcad-i64-cell-probe/v1 (la escribe el CLI por -o).
