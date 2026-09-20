# I-57 F6 — Paquete de Owner Validation OV-FND-03 importation

## Identidad obligatoria

```text
Iniciativa / rama = I-57 / architecture/shared-view-foundation
Commit             = 415953fefa5484b9d23829eb3b7b3b18b55b4389
DLL Debug          = C:\Users\alejandra-mendoza\.codex\worktrees\architecture-shared-view-foundation\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll
ProductVersion     = 1.0.0+415953fefa5484b9d23829eb3b7b3b18b55b4389
DLL SHA-256        = 8C210802692029E237FF8732C64385DF6D02D0654BE94BD8AC8EE0E80C7ABB23
Library            = D:\Base_de_datos_AutoCAD_V.0.dwg
Library SHA-256    = B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8
AutoCAD            = 2025; registrar build exacto si esta disponible
```

Cerrar AutoCAD antes de reemplazar el DLL. Abrir AutoCAD 2025, ejecutar `NETLOAD` con la ruta anterior y comprobar
la ProductVersion exacta. Trabajar sobre un DWG descartable. En `RACKCAD`, confirmar la biblioteca configurada y
su ruta. No editar la biblioteca real: para el caso C usar una ruta temporal inexistente o una copia DWG
deliberadamente incompleta, registrar esa ruta y reiniciar AutoCAD para invalidar la cache por path.

## Matriz A-E

| Caso | Preparacion y gesto | Expected | Resultado Owner |
|---|---|---|---|
| **A. Pieza ya presente** | En un DWG donde una insercion previa ya dejo sus definiciones, repetir `RACKEDITAR`/Actualizar o insertar el mismo sistema | query final `FOUND`; sin import innecesario; vista, geometria, nombre y BOM iguales; sin renombre inesperado | `PASS / FAIL` |
| **B. Ausente en target, disponible en library** | En DWG nuevo confirmar que una pieza real no existe; usar `QUICKCAMA` o el menu para una cama que requiera `RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL` con la biblioteca real configurada | `Ensure/import` crea la definicion exacta; query final `FOUND`; vista completa; geometria, nombre y BOM correctos; sin pieza omitida | `PASS / FAIL` |
| **C. Ausente y no disponible** | En DWG nuevo configurar temporalmente una biblioteca inexistente/incompleta y pedir el mismo sistema; registrar path/hash temporal; restaurar la biblioteca real y reiniciar despues | query final `MISSING`; rechazo/reporte legacy correcto; sin false success, definicion basura ni geometria parcial de esa pieza | `PASS / FAIL` |
| **D. Zero requirements** | Insertar/actualizar Cantilever con `RACKCANTILEVER` desde un DWG nuevo | no consulta/importa piezas de `blocks-library.dwg`; geometria y nombre Cantilever iguales; sin mensaje espurio de pieza faltante | `PASS / FAIL` |
| **E. Key con punto** | Repetir B observando la definicion `RODILLO_DE_TUBO_DE_1.9_CALIBRE_14_LATERAL` | key literal con `1.9`; no aparece `1 9`, suffix de vista ni sanitizacion; query/import encuentran exactamente la pieza correcta | `PASS / FAIL` |

Para A, B y E ejecutar `RACKBOMTOTAL` antes/despues cuando exista una base comparable. Ejecutar `RACKLISTA` para
confirmar que identidad, vistas y nombres no cambiaron. Guardar, cerrar y reabrir los DWG de B y D. Confirmar que
no hay vistas faltantes/duplicadas, renombres inesperados, error silencioso ni regresion visible.

## Formato de respuesta del Owner

```text
Fecha y zona:
Validador:
AutoCAD version:
AutoCAD exact build:
Commit: 415953fefa5484b9d23829eb3b7b3b18b55b4389
DLL ProductVersion: 1.0.0+415953fefa5484b9d23829eb3b7b3b18b55b4389
DLL SHA-256: 8C210802692029E237FF8732C64385DF6D02D0654BE94BD8AC8EE0E80C7ABB23
Library real path: D:\Base_de_datos_AutoCAD_V.0.dwg
Library real SHA-256: B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8
Case C temporary library/path/hash:

A = PASS | FAIL
B = PASS | FAIL
C = PASS | FAIL
D = PASS | FAIL
E = PASS | FAIL
Names/geometry/BOM parity = PASS | FAIL
Save/reopen = PASS | FAIL
Unexpected visible regression = NONE | describe

OV-FND-03 importation = PASS | FAIL
Notes / reproducible failures:
```

Solo el Owner puede llenar los resultados. Un `PASS` corresponde exclusivamente al SHA, DLL, version de AutoCAD
y bibliotecas identificados arriba.
