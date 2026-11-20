
## Mediciones de la v4.2 (en `7f0d9aa3`)

## R-39. B-08: las cuentas de RIA en todo el archivo

| Hecho | Fuente |
|---|---|
| `TheReadBuildsTheInputFromTheScanAndTheEvidenceOnceAndAWitnessWithoutKeyFailsBeforeAnyInputIsBuilt` exige en **todo** RIA (`Adapter()`) un `new PushBackPilotIntentScan(` y un `PushBackPilotIntentReader.Read(`, y el orden de `Read` | EIG:350-364 (:356-357) |
| EIG:286 prohíbe `throw` en todo RIA; EIG:292-301, las API de `AdapterWrites`, un `RackBlockData.Read(` y ningún `GetObject(` | EIG:269-301 |
| Con `ReadAt` en `Draft/`, el GREEN de RIA tiene uno de cada (en `Read`) y ningún `throw`: 35 de 35 aserciones traducidas en verde, y las dos contrafactuales de la firma en rojo | `measure_pbt4_v42-out.txt` |
| `PushBackFullDraftReader.cs` ya llama a `PushBackPilotIntentReader.Read` desde `Draft/` | `Draft/PushBackFullDraftReader.cs:56` |

## R-40. El recorrido entero de las guardas de PB-T4a (I-25)

| Hecho | Fuente |
|---|---|
| 46 clases de prueba nombran WHC, WEI, NH o RIA, sus carpetas o los ayudantes de carpeta; tienen 1 026 pruebas sin `Skip` | `pbt4a-guard-walk-out.txt` |
| 764 no leen ninguno de los archivos tocados; 162 los leen sin ninguna diferencia de un literal con nombre ni ningún cuerpo que cambie; 100 quedan señaladas (67 y 33) | ídem; `pbt4a-guard-walk.tsv` |
| De las 100, leídas una a una, cae CT-PBT3A-12 (la de la lista); las otras 99 siguen en verde, por los motivos de §4.1 del paquete | `pbt4a-guard-walk-flagged.txt` |
| Los cuerpos que cambian: en WHC, los cinco que releen; en WEI, el constructor, `FromPilot`, `FromReads` y las cuatro fábricas de fallo; en NH, `ReadIntent`, `ReadIntentAtOrigin` y `ReadOnActiveDocument`; en RIA, `Read` (solo la línea del éxito) y `AtOrigin` | `pbt4a-guard-walk-out.txt` |

## R-41. Las listas tras la decisión del Owner, y el léxico

| Hecho | Fuente |
|---|---|
| `pbt4b-substitution-list.tsv`: las 470 filas de PB-T4b sin C-7, sha256 `44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4`, 103 000 bytes | `split_lists_pbt4-out.txt` |
| `pbt4d-retire-list.tsv`: 139 de C-7, 55 originales y 29 miembros de apoyo, sha256 `1c9d67e8f2dac7eae3202d61a23ff5131e96123e5a6af275e5ce8a8bf71b8b71`, 55 225 bytes | ídem |
| `lists_core.py` tomaba `@"""…` por una cadena cruda; tres archivos de las listas las tienen (EIG, RG y `WorkspacePilotUpdateGuardTests`). Corregido, las listas regeneradas son iguales: `pbt4-substitution-lists.tsv` `f24c2dbc…`, `.md` `0f4c9bad…`, `pbt4-copy-map.tsv` `1091ae50…` y `pbt4-retire-compile.tsv` `e7246dc1…` | `sha256sum` |
| De las 67 por compilación, `WorkspacePilotUpdateRunTests` 42 y `PushBackFullWritePrepareTests` 6; en C-7, 48 y 9 | `pbt4d-retire-list.tsv` |
