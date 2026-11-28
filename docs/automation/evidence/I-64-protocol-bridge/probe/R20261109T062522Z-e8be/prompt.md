I-64 - SONDA MINIMA DE CELDA (solo lectura). RunId R20261109T062522Z-e8be.
Objetivo: medir la elegibilidad de esta celda (Codex CLI, modelo gpt-6.1-sol, effort high, solo lectura) para la revision formal del Architect de I-64-A-8. No es una tarea de producto.
Ejecuta en el directorio de trabajo actual exactamente estas tres ordenes y devuelve sus salidas, sin interpretarlas:
  1. git rev-parse HEAD
  2. git rev-parse origin/main
  3. git rev-parse HEAD:docs/automation/state/I-64.yml
Campos: HeadSha = salida de 1; OriginMainSha = salida de 2; StateBlob = salida de 3; CommandsRun = las ordenes ejecutadas.
Reglas: solo lectura; no escribas archivos, no hagas commit ni push, no ejecutes otras ordenes. Salida solo en ASCII.
Informe esperado: la salida exacta del esquema rackcad-i64-cell-probe/v1 (la escribe el CLI por -o).
