# I-65 — Procedimiento de ejecución de la sonda P1

Aplica a la sonda que el Owner autorice (P1 v3.2 en `54f17ef6`, si autoriza el retiro de S9). Lo ejecuta el Coordinator con Computer
Use o el Owner a mano; los pasos son los mismos. Ninguna otra acción en RAM Elements está autorizada.

## A. Preparación (Coordinator, sin RAM Elements)

1. Verificar que RAM Elements **no** está en ejecución (no debe haber modelos del usuario abiertos).
2. Crear una carpeta **nueva** fuera del repositorio: `D:\I65-P1\run-<UTC aaaammddThhmmssZ>\` (si existe, se elige otra marca).
3. Extraer la sonda **del commit autorizado** (`git show <commit>:docs/automation/evidence/I-65-f0/ram-probe-p1.py`) y escribirla en
   esa carpeta como `ram-probe-p1.py`, sustituyendo **solo** la línea `OUT_DIR = r"__OUT_DIR__"` por la ruta absoluta de la carpeta.
4. Registrar: SHA-256 del blob autorizado, SHA-256 de la copia ejecutable y el diff entre ambas (debe ser exactamente esa línea).

## B. Ejecución en RAM Elements 2026 (26.00.01.015)

1. Abrir «RAM Elements 2026» desde el menú Inicio (una sesión de licencia).
2. Crear un **modelo nuevo** (File > New). No abrir ningún archivo existente ni ejemplo.
3. Abrir la Python Console (Home > Modeling > Python Console).
4. «Run File…» → seleccionar `D:\I65-P1\run-…\ram-probe-p1.py`. No editar ni ejecutar otro script; no escribir en la consola.
5. Esperar el fin («I-65 P1 report written to …» o «ABORTED before any change»). Capturar pantalla de la pestaña Output.
6. Si aparece un diálogo inesperado (guardar, licencia, error modal), **detenerse**: capturarlo y no aceptar nada que guarde o
   sobrescriba; informar.
7. Cerrar RAM Elements y, ante «¿guardar cambios?», responder **No**. No guardar el modelo bajo ningún nombre.

## C. Custodia (Coordinator)

1. Confirmar que la carpeta contiene solo `ram-probe-p1.py`, `write-test.txt` y `p1-report.json` (más nada creado por RAM; si hubiera
   otro archivo, registrarlo).
2. Copiar `p1-report.json` y la captura de Output a `docs/automation/evidence/I-65-f0/p1-run/` con sus SHA-256.
3. Clasificar cada capacidad como VERIFIED IN INSTALLED RAM / DOCUMENTED ONLY / FAILED / UNKNOWN; no promover a VERIFIED nada que el
   informe no muestre.
