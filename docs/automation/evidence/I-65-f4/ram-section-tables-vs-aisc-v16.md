# I-65 F4 — tablas W y HSS_SQR de la base instalada de RAM frente a AISC Shapes Database v16.0 (custodia, DIF-04)

- Ejecutor: Coordinator, 2026-10-14, en la máquina del Owner, por la disposición «F4 Engineering Validation and Host Readiness»
  (DIF-04: confirmar la edición del catálogo de RAM). Decisiones §49.
- **Qué es:** lectura de solo lectura de los archivos de texto tabulado de la base instalada de RAM (`Database\Sections\United States\W.sec`
  y `HSS_SQR.sec`) y comparación con el catálogo del repositorio (`assets/catalogs/structural-sections-w.csv` y
  `structural-sections-hss-rect.csv`, AISC Shapes Database v16.0); búsqueda de marcas de edición en los archivos de texto de `Database`.
- **Qué no es:** no se abrió ni ejecutó RAM Elements (sin proceso `RAMElements.exe`), no se importó su módulo de Python y no se escribió
  nada fuera de la carpeta temporal del Coordinator. No es la lectura de la interfaz ni una confirmación formal de la edición.
- Script: [ram-section-tables-vs-aisc-v16.py.txt](ram-section-tables-vs-aisc-v16.py.txt) (contenido LF, SHA-256
  `5ceb2b2b40e2260b5fdd6c7cc5a79abe957e47989e8f0d72a359413730155c15`), ejecutado con `python -I` desde la carpeta temporal y las rutas
  como argumentos. Salida íntegra: [ram-section-tables-vs-aisc-v16.out.txt](ram-section-tables-vs-aisc-v16.out.txt) (contenido LF, SHA-256
  `022c3b5d6b17b2f4eb64e60875ce8376e1936f5ecd92c9e58c1fb9a6091ebd42`).
- Archivos de RAM leídos (los mismos bytes que la pre-lectura del 2026-10-11): `W.sec` SHA-256
  `dffd9e0082a8955572f1a21989938c2985952d7e7cffd714295d0e53dbd7b977`; `HSS_SQR.sec` SHA-256
  `992ddac9a9b130da0426dfa761ebde2a8c39fc77736302d5e6431b41c1d93df0`.

## Resultado

Emparejamiento por nombre normalizado (`W 12X26` → `W12X26`; `HSS_SQR 4X4X1_4` → `HSS4X4X1/4`; sin distinguir mayúsculas). Diferencia =
`|RAM − catálogo| > 1e-9 · max(1, |catálogo|)`.

| Tabla | Filas en RAM | Filas en v16.0 | Comunes | Solo en RAM | Solo en v16.0 | Propiedades comparadas | Diferencias |
|---|---|---|---|---|---|---|---|
| W | 302 | 289 | 289 | 13 (`W36X230`, `W36X245`, `W36X260`, `W36X280`, `W36X300`, `W36X328`, `W36X359`, `W36X393`, `W36X439`, `W36X527`, `W36X650`, `W36X798`, `W36X800`) | 0 | A, Ix, Iy, J, Cw, d, bf, tw, tf, Zx, Zy, Sx, Sy | **0** |
| HSS_SQR (cuadrados de v16.0) | 131 | 126 | 126 | 5 (`HSS1-1/4X1-1/4X1/8`, `HSS1-1/4X1-1/4X3/16`, `HSS1-3/4X1-3/4X3/16`, `HSS1-5/8X1-5/8X1/8`, `HSS1-5/8X1-5/8X3/16`) | 0 | A, Ix, Iy, J, Ht, B, tdes, Zx, Sx | **0** |

Marcas de edición en los archivos de texto de `Database`: **ninguna**. Nombres de archivo de notas de versión o «readme» en la raíz de
la instalación: ninguno.

## Lectura

Las dos tablas de RAM contienen **todas** las filas de AISC Shapes Database v16.0 de sus familias con valores **idénticos**, más filas
heredadas que v16.0 ya no lista. Es evidencia fuerte de coherencia con v16.0 en lo que usa el piloto, pero **no** una confirmación formal
de la edición: los archivos no la declaran. La decisión sobre su suficiencia es del Owner (hoja de validación §4, V-01).
