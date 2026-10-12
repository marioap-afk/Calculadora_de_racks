# I-65 — Incidente del checkout principal: `RackCad.sln` modificado por un Worker (DEV-I65-F4-16)

- **Estado:** CONTENIDO, **SIN RESTAURAR** (la orden del Owner «Option C and Checkout Incident Containment» prohíbe alterar el checkout
  principal sin una autorización posterior).
- **Inspección:** 2026-10-12, estrictamente de solo lectura (`git --no-optional-locks`, lectura de archivos y de fechas); ninguna operación
  escribió en `D:\Documentos\Codex\Calculadora de racks` ni en su índice (el índice no aparece modificado en la ventana inspeccionada).
- **Archivos de esta carpeta:** `main-checkout-facts.json` (hechos y hashes), `main-checkout-RackCad.sln.diff` (diff contra `HEAD`),
  `main-checkout-RackCad.sln.modified.txt` (copia byte a byte del archivo actual), `worker-call-25.ps1.txt` (orden literal del Worker),
  `simulate-worker-call-25.ps1` y `simulation.json` (reproducción sobre una copia), `main-checkout-mtime-scan*.json` (barridos de fechas) y
  `ram-execution-scan.json` (barrido de ejecuciones del programa y de intérpretes en las transcripciones y los eventos de Codex).

## 1. Comparación contra `HEAD`

| Hecho | Valor |
|---|---|
| Checkout principal | `D:\Documentos\Codex\Calculadora de racks`, rama `main`, `HEAD` `95690c28dc6268e61dff32a0cbc33cc9fde3d47f` (`origin/main` = `bb0d5522`; el `main` local va por detrás, sin relación con el incidente) |
| `git status --porcelain --untracked-files=all` | solo ` M RackCad.sln` |
| Cambios en el índice / `stash` | ninguno / ninguno |
| `core.autocrlf` / atributos de `RackCad.sln` | `true` / ninguno |
| `RackCad.sln` actual | 10 566 bytes, SHA-256 `4F38D61B8C09C84404D6E58C5E8F3FBB1D62E504699E2C078AD44F9EEF579C4C`, **sin BOM**, CRLF, 144 líneas; última escritura `2026-10-11T20:32:56.8408396Z` |
| Blob de `HEAD` (`5317eecef383e0902e145b9b5d1fcbaf891e0d01`) | 9 176 bytes, SHA-256 `DEB08FB0DFDEAFBC4DC4A262A285FAB2F964FF3844A6AF449E6621B1DB57FC14`, con BOM, LF, 129 líneas |
| Estado previo reconstruido (blob de `HEAD` con LF → CRLF, como lo deja `autocrlf`) | 9 305 bytes, SHA-256 `A06892EAE7EA841CED3612961C77FE83815794C1D9110B8FEE184B443086E399`, con BOM |
| Diff | 4 bloques, 16 inserciones y 1 eliminación; SHA-256 del diff `4BA20015ECA06AED38D4351FADAD7E11D40888ED8BA4832A536412843AF6E8F6` |

## 2. Modificaciones identificadas

| # | Bloque | Modificación |
|---|---|---|
| M1 | `@@ -1,4 +1,4 @@` | Se elimina el BOM UTF-8 de la primera línea |
| M2 | `@@ -23,6 +23,8 @@` | Se añade el proyecto `RackCad.StructuralAnalysis.RamTool` (`tools\RackCad.StructuralAnalysis.RamTool\RackCad.StructuralAnalysis.RamTool.csproj`, GUID `{5C7B2D1E-3A94-4F6B-8E21-9D0A47C3B6F5}`) tras `RackCad.StructuralSections.Import`: 2 líneas |
| M3 | `@@ -117,6 +119,18 @@` | 12 líneas de configuración `Debug`/`Release` × `Any CPU`/`x64`/`x86` del GUID nuevo, con UN tabulador (el formato existente usa dos) |
| M4 | `@@ -125,5 +139,6 @@` | El GUID nuevo se anida en la carpeta de solución `tools` (`{AE66CB82-…}`), con un tabulador |

El proyecto referenciado **no existe** en `main` (solo en la rama de I-65): si el cambio se confirmara en `main`, la solución apuntaría a un
proyecto inexistente.

## 3. Respaldo en la transcripción del Worker

- Autor: Worker de F4-T4 intento 2 (`R20261011T202738Z-6356`, transcripción `agent-a31cc71cc41d2b614.jsonl`), llamada 25 (PowerShell,
  `2026-10-11T20:32:55.230Z`): `cd` al worktree y después `[IO.File]::ReadAllText("RackCad.sln")` / `[IO.File]::WriteAllText(...)`. `cd`
  (`Set-Location`) no cambia el directorio de proceso de .NET, que es el checkout principal; la ruta relativa se resolvió allí. El Worker no lo
  declaró. El texto literal de la orden está en `worker-call-25.ps1.txt`.
- **Reproducción:** aplicar esa misma transformación (solo cambiando la ruta por la de una copia en el área temporal de la sesión) al estado
  previo reconstruido produce un archivo **idéntico byte a byte** al actual (SHA-256 `4F38D61B…`; `simulation.json`). M1 se explica por
  `WriteAllText`, que escribe UTF-8 sin BOM; M2-M4, por la lógica de la orden.
- **Conclusión:** M1-M4 son, todas, el cambio no autorizado del Worker. **No hay cambios legítimos mezclados** en el archivo.

## 4. Otras modificaciones no confirmadas en el checkout principal

- Git: ninguna otra (`status` con todos los no rastreados, índice y `stash` vacíos).
- Barrido de fechas del árbol de trabajo (incluidos los archivos ignorados) desde `2026-10-09T00:00Z`: **solo** `RackCad.sln`.
- `.git/`: solo actividad compartida del repositorio (objetos, refs y registros de otras ramas y worktrees enlazados); el índice del checkout
  principal no se modificó.
- F4-T5: `placeholder.txt` creado y borrado por el Worker a los 3 s (DEV-I65-F4-15); no existe.

## 5. Procedimiento de restauración (NO EJECUTADO; requiere autorización del Owner)

El cambio no autorizado es el archivo completo (M1-M4), así que restaurar el archivo a `HEAD` deshace exactamente ese cambio y nada más.
Las órdenes leen o escriben solo `RackCad.sln` del checkout principal.

1. **Comprobación previa (solo lectura); si algo no coincide, NO restaurar y volver a inspeccionar:**
   - `git -C "D:\Documentos\Codex\Calculadora de racks" --no-optional-locks status --porcelain --untracked-files=all` → exactamente
     ` M RackCad.sln`.
   - `Get-FileHash "D:\Documentos\Codex\Calculadora de racks\RackCad.sln" -Algorithm SHA256` → `4F38D61B8C09C84404D6E58C5E8F3FBB1D62E504699E2C078AD44F9EEF579C4C`
     (si difiere, alguien cambió el archivo después del 2026-10-12: puede haber cambios legítimos nuevos).
2. **Copia de seguridad adicional** fuera del repositorio (la copia custodiada ya existe: `main-checkout-RackCad.sln.modified.txt`).
3. **Restauración del archivo:** `git -C "D:\Documentos\Codex\Calculadora de racks" restore --worktree --source=HEAD -- RackCad.sln`.
4. **Comprobación posterior:** `status --porcelain` vacío y `Get-FileHash` = `A06892EAE7EA841CED3612961C77FE83815794C1D9110B8FEE184B443086E399`
   (blob de `HEAD` con CRLF y BOM).
5. **Registro:** hashes antes y después en la evidencia de I-65.

Alternativa equivalente, si se prefiere ver el cambio deshecho bloque a bloque: `git apply -R` con `main-checkout-RackCad.sln.diff`
(tras la misma comprobación previa).
