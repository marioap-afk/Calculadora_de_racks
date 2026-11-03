# I-65 — preparación de F4: pre-lectura pasiva de la base de datos instalada de RAM Elements (custodia)

- Ejecutor: Coordinator, 2026-10-11T09:22:53Z, en la máquina del Owner. Orden: preparación de F4
  ([I-65-owner-authorization-f4-preparation.original.txt](../../decisions/I-65-owner-authorization-f4-preparation.original.txt)).
- **Qué es:** lectura de solo lectura de los archivos de texto tabulado que RAM instala como base de datos de secciones y materiales
  (`Database\Sections\<país>\*.sec` y `Database\Materials\<país>\*.mat`; formato descrito en el Discovery §3). Es investigación
  documental (CD-I65-F4P-02).
- **Qué no es:** no se abrió RAM Elements (sin proceso `RAMElements.exe` en ejecución), no se importó `ramelements`, no se ejecutó el
  adaptador ni ningún script en RAM y no se escribió nada fuera de la carpeta temporal del Coordinator. **No es la lectura de la OV ni una
  verificación de ingeniería:** la base que RAM carga en una corrida puede incluir bases de usuario (ver «Montilla» abajo), y el
  ingeniero confirma en la interfaz de RAM los valores que usa el modelo.
- Producto: `RAMElements.exe` SHA-256 `7dcb48555741fa368f5d5f6a14e760fe0662dd5c0b3cc53f6e88f1c0231e7970`, el mismo ejecutable de P1
  (`p1-results.md` §1).
- Script de lectura: [ram-db-preread.py.txt](ram-db-preread.py.txt) (SHA-256 `5a22a3b4734783d539eea582f94934f9428a581409e973c571a465121958c05a`,
  blob `ca838072`), ejecutado con `python -I`. Salida íntegra: [ram-db-preread.out.txt](ram-db-preread.out.txt) (contenido LF, SHA-256
  `de65639aae588a14a13877747102a27a0ffd8b280c8b1d4f5fd0f7766d3d5041`, blob `d377834b`).

## Archivos leídos

Carpeta `C:\Program Files\Bentley\Engineering\RAM Elements\Database` (la de `ProgramData\…\26.0.1\Database\Sections` y `…\Materials`
están vacías).

| Archivo | Bytes | SHA-256 | Etiqueta |
|---|---|---|---|
| `Sections/United States/W.sec` | 58 794 | `dffd9e0082a8955572f1a21989938c2985952d7e7cffd714295d0e53dbd7b977` | `[W.leo]` |
| `Sections/United States/HSS_SQR.sec` | 21 917 | `992ddac9a9b130da0426dfa761ebde2a8c39fc77736302d5e6431b41c1d93df0` | `[HSS_Rect.leo]` |
| `Materials/United States/Steel.mat` | 2 789 | `2a8b423964d2ba3580273de2d8a29d5e70c9e6dceefa1f892a40bb8cd1139ce1` | `[Steel]` |

## Filas extraídas (valores tal como están en los archivos)

| Nombre de RAM | Unidad del archivo | Valores |
|---|---|---|
| `W 12X26` | `In` | `d` 12.2; `bf` 6.49; `tw` 0.23; `tf` 0.38; `k` 0.68; `area` 7.65; `I33loc` 204; `Z33loc` 37.2; `w33top` = `w33bot` 33.4; `I22loc` 17.3; `Z22loc` 8.17; `w22top` = `w22bot` 5.34; `Aw2` 2.81; `Aw3` 4.93; `J` 0.3; `cw` 607; `IsmName` `W12X26`, `AISC` |
| `HSS_SQR 4X4X1_4` | `In` | `a` 4; `b` 4; `t` 0.233; `area` 3.37; `I33loc` 7.8; `Z33loc` 4.69; `w33top` = `w33bot` 3.9; `I22loc` 7.8; `Z22loc` 4.69; `w22top` = `w22bot` 3.9; `J` 12.8; `cw` 0; `IsmName` `HSS4X4X1/4`, `AISC` |
| `A992 Gr50` | `Lb-In` | ν (`U`) 0.3; peso unitario (`W`) 0.28179435 lb/in³; α 6.50E-06; E 29 000 000 psi; Fy 50 000 psi; Fu 65 000 psi; Ry 1.1; Rt 1.1 |
| `A500 GrC rectangular` | `Lb-In` | ν 0.3; peso unitario 0.28179435 lb/in³; α 6.50E-06; E 29 000 000 psi; Fy 50 000 psi; Fu 62 000 psi; Ry 1.3; Rt 1.2 |
| `A36` (referencia del Freeze D-07) | `Lb-In` | ν 0.26; peso unitario 0.284 lb/in³; E 29 000 000 psi; Fy 36 000 psi; Fu 58 000 psi |

`0.28179435 lb/in³` = 486.9406 pcf (×1728) ≈ 7800 kg/m³. El `A36` usa otro peso unitario (0.284 lb/in³ = 490.75 pcf) y otro ν: los
materiales de la base no comparten valores.

## Unicidad de los nombres

Búsqueda de coincidencia **exacta** del primer campo en todos los `.sec` y `.mat` de la carpeta (todos los países): cada uno de los cuatro
nombres aparece **una sola vez** (`W 12X26` → `United States/W.sec`; `HSS_SQR 4X4X1_4` → `United States/HSS_SQR.sec`; `A992 Gr50` y
`A500 GrC rectangular` → `United States/Steel.mat`). Las coincidencias parciales (`CSA_A500 GrC rectangular` en Canadá, `HSS-A1085_SQR …`)
no son el mismo nombre.

## Limitaciones

- **Bases de usuario:** `RE\Config.ini` del perfil de RAM registra `[DlgSectionsDB] frameSeccionesDataBaseName=Montilla`: la última base de
  secciones abierta en el diálogo fue «Montilla», una base de usuario que P1 no localizó (problema 10). Su contenido y su precedencia frente
  a la base instalada siguen UNKNOWN; el ingeniero la revisa en la interfaz (procedimiento §3, paso B4).
- La lectura no prueba qué fila usa RAM al asignar el nombre en un modelo: eso lo confirma el ingeniero en la interfaz y la OV.
- Los valores **esperados** de la matriz no se copian de aquí (D-07): esta pre-lectura solo llena la columna «valor en RAM (pre-lectura)».
