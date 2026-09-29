# I-52 AUTH-15 - Evidencia cruda de RUN-3 (validacion en host)

Copia **sin modificar** de los artefactos que produjo la corrida RUN-3 del paquete `fcca6e6c`. Los hashes de abajo son de los archivos
tal como salieron de `D:\I52-AUTH15-HV\fcca6e6c\out\`; `.gitattributes` fija `-text` en ellos para que los bytes (y por tanto los hashes)
sobrevivan al checkout. **El resultado crudo es FAIL y se conserva tal cual**: no se reescribe ni se oculta. Que AUTH-15 quede admitida se
funda en un resultado DERIVADO, con nombre propio, ratificado por el Owner (ver las decisiones, secciones 13 y 14); nunca en llamar PASS a esta corrida.

## Identidad

| Campo | Valor |
|---|---|
| Implementacion | `a80a3801cc39eaa9656be05c080853be87ab2581` |
| Arnes | `fcca6e6c5713e966c612998a1eb9e6833825d47e` |
| Arboles src / tests (impl = arnes) | `224ca6a32c6b17128d5ce168dca307246b16fbcd` / `53fb9b98d0c2b196a8e712dc095df40747f0ee88` |
| treesEqual | True |
| Paquete | `D:\I52-AUTH15-HV\fcca6e6c` (zip SHA-256 `1156BFB1E0369B3E07AAB79760FB93055216646D6982C8750025EE05F8C4B4A8`, SHA256SUMS SHA-256 `B6E02AD5D08CFBD1D006019119CC837B6EA1C47389789E4001AA096ACAE80A7D`) |
| Proceso | pid 25436, inicio 2026-09-29T19:04:37.2601515Z, salida de AutoCAD 0, timeout False |
| Lanzador | `launchValid=True`, `runResult=FAIL`, codigo de salida 3, 29 comprobaciones todas `true` |
| FILEDIA | antes 1, durante 0, despues 1; vivo al inicio 1, vivo al final 1 |

## Artefactos (SHA-256)

| Archivo | Bytes | SHA-256 |
|---|---|---|
| `hostval-evidence.json` | 251712 | `C85EA59BFF17C31846B92F2D5F2C5CF2988E1F5402C8B5972B43289AB5FE3E68` |
| `launcher-record.json` | 3015 | `E47D56841A0EF816BBFD8EF5AF8A5CCD51C521AF9A5D6255CA44153E1DA08913` |
| `hostval.log` | 4825 | `8CD6628C0D7E81091609BA1B5345E6D18C0EC9CE98567EDFD09DDF29ABA53397` |
| `filedia.txt` | 29 | `4BB388D39A3964267328E7AE5F2BA817508362EFE0E4E51774D10ADD02446A57` |
| `run.scr` | 772 | `32CC8EE66DABCE426B9A23D876B9A6EE3A021AB4FA3895BA1C87C979EA9B0EFF` |

No se copian los DWG (`scratch.dwg`, `blank-template.dwg`, `hv04.dwg`, `doc-cases\*.dwg`). Los 17 DWG de `doc-cases\` son **byte-identicos** a
`blank-template.dwg` (SHA-256 `7E7CDC22D2929138BE79A4558923B1D2092E6DF77DC0CBE0205D5FB062D115CC`, igual al del DWG en blanco del Owner y al de `scratch.dwg`):
ningun documento de caso se guardo. Se cerraron con `CloseAndDiscard`.

## Resultado crudo

```text
verdict = FAIL   completed = True   stopKind = none   state = final
problems = 0   deviations = 0   HV leaks = 0   control leaks = 42   side-characterization leaks = 128
documentAuthority = {"available": true, "documentsAtStart": 1}
```

### Casos HV (los 15)

| Caso | dbKind | Resultado | Aserciones | Fugas |
|---|---|---|---|---|
| HV-00 | SIDE-DB | PASS | 36 | 0 |
| HV-01 | DOCUMENT-AUTHORITY | PASS | 21 | 0 |
| HV-02 | DOCUMENT-AUTHORITY | PASS | 18 | 0 |
| HV-03 | DOCUMENT-AUTHORITY | PASS | 10 | 0 |
| HV-04 | SIDE-DB | PASS | 13 | 0 |
| HV-05 | DOCUMENT-AUTHORITY | PASS | 11 | 0 |
| HV-06 | SIDE-DB | PASS | 29 | 0 |
| HV-07 | SIDE-DB | PASS | 7 | 0 |
| HV-08 | DOCUMENT-AUTHORITY | PASS | 15 | 0 |
| HV-09 | SIDE-DB | PASS | 33 | 0 |
| HV-10 | DOCUMENT-AUTHORITY | PASS | 8 | 0 |
| HV-11 | SIDE-DB | PASS | 5 | 0 |
| HV-12 | DOCUMENT-AUTHORITY | PASS | 11 | 0 |
| HV-13 | DOCUMENT-AUTHORITY | PASS | 9 | 0 |
| HV-14 | DOCUMENT-AUTHORITY | PASS | 7 | 0 |

### Controles (los 14)

| Control | dbKind | Resultado | Fugas |
|---|---|---|---|
| RB-01 | SIDE-DB | FAIL | 3 |
| RB-01V | SIDE-DB | FAIL | 3 |
| RB-01V | DOCUMENT-AUTHORITY | PASS | 0 |
| RB-01D | DOCUMENT-AUTHORITY | PASS | 0 |
| RB-02a | SIDE-DB | FAIL | 4 |
| RB-02a | DOCUMENT-AUTHORITY | PASS | 0 |
| RB-02b | SIDE-DB | FAIL | 9 |
| RB-02b | DOCUMENT-AUTHORITY | PASS | 0 |
| RB-02c | SIDE-DB | FAIL | 16 |
| RB-02c | DOCUMENT-AUTHORITY | PASS | 0 |
| RB-03 | SIDE-DB | FAIL | 2 |
| RB-03 | DOCUMENT-AUTHORITY | PASS | 0 |
| RB-05 | SIDE-DB | FAIL | 5 |
| RB-05 | DOCUMENT-AUTHORITY | PASS | 0 |

### Fugas de control (42, todas SIDE-DB; ninguna en DOCUMENT-AUTHORITY)

```text
RB-01 [SIDE-DB] RB-01 after the caller's rollback: added BT:CTRL_RB_BLOCK = 78
RB-01 [SIDE-DB] RB-01 after the caller's rollback: added BTC:CTRL_RB_BLOCK = 1|ext|7B
RB-01 [SIDE-DB] RB-01 after the caller's rollback: added LT:CTRL_RB_LAYER = 7C
RB-01V [SIDE-DB] RB-01V after the caller's rollback: added BT:CTRL_RB_BLOCK = 78
RB-01V [SIDE-DB] RB-01V after the caller's rollback: added BTC:CTRL_RB_BLOCK = 1|ext|7B
RB-01V [SIDE-DB] RB-01V after the caller's rollback: added LT:CTRL_RB_LAYER = 7C
RB-02a [SIDE-DB] RB-02a after the caller's rollback: added BT:AUTH15HV_HDR = 7C
RB-02a [SIDE-DB] RB-02a after the caller's rollback: added BT:CTRL_RB02A = 81
RB-02a [SIDE-DB] RB-02a after the caller's rollback: added BTC:AUTH15HV_HDR = 2|noext|7F,80
RB-02a [SIDE-DB] RB-02a after the caller's rollback: added BTC:CTRL_RB02A = 3|noext|84,85,86
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added BT:*D4 = 8D
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added BT:AUTH15HV_HDR = 7C
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added BT:CTRL_RB02B = 81
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added BTC:*D4 = 9|noext|90,91,92,93,94,95,96,97,98
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added BTC:AUTH15HV_HDR = 2|noext|7F,80
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added BTC:CTRL_RB02B = 5|noext|84,85,86,88,8B
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added LT:Defpoints = 8C
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added LT:RACKCAD_ANOTACIONES = 87
RB-02b [SIDE-DB] RB-02b after the caller's rollback: added LT:RACKCAD_COTAS = 8A
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added BT:CTRL_RB02C = 7C
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added BTC:CTRL_RB02C = 431|noext|100,101,102,103,104,105,106,107,108,109,10A,10B,10C,10D,10E,10F,110,111,112,113,114,115,116,117,118,119,11A,11B,11C,11D,11E,11F,120,121,122,123,124,125,126,127,128,129,12A,12B,12C,12D,12E,12F,130,131,132,133,134,135,136,137,138,139,13A,13B,13C,13D,13E,13F,140,141,142,143,144,145,146,147,148,149,14A,14B,14C,14D,14E,14F,150,151,152,153,154,155,156,157,158,159,15A,15B,15C,15D,15E,15F,160,161,162,163,164,165,166,167,168,169,16A,16B,16C,16D,16E,16F,170,171,172,173,174,175,176,177,178,179,17A,17B,17C,17D,17E,17F,180,181,182,183,184,185,186,187,188,189,18A,18B,18C,18D,18E,18F,190,191,192,193,194,195,196,197,198,199,19A,19B,19C,19D,19E,19F,1A0,1A1,1A2,1A3,1A4,1A5,1A6,1A7,1A8,1A9,1AA,1AB,1AC,1AD,1AE,1AF,1B0,1B1,1B2,1B3,1B4,1B5,1B6,1B7,1B8,1B9,1BA,1BB,1BC,1BD,1BE,1BF,1C0,1C1,1C2,1C3,1C4,1C5,1C6,1C7,1C8,1C9,1CA,1CB,1CC,1CD,1CE,1CF,1D0,1D1,1D2,1D3,1D4,1D5,1D6,1D7,1D8,1D9,1DA,1DB,1DC,1DD,1DE,1DF,1E0,1E1,1E2,1E3,1E4,1E5,1E6,1E7,1E8,1E9,1EA,1EB,1EC,1ED,1EE,1EF,1F0,1F1,1F2,1F3,1F4,1F5,1F6,1F7,1F8,1F9,1FA,1FB,1FC,1FD,1FE,1FF,200,201,202,203,204,205,206,207,208,209,20A,20B,20C,20D,20E,20F,210,211,212,213,214,215,216,217,218,219,21A,21B,21C,21D,21E,21F,220,221,222,223,224,225,226,227,228,229,22A,22B,22C,22D,22E,22F,230,231,232,233,234,235,236,237,238,239,23A,23B,8D,8E,8F,90,91,92,93,94,95,96,97,98,99,9A,9B,9C,9D,9E,9F,A0,A1,A2,A3,A4,A5,A6,A7,A8,A9,AA,AB,AC,AD,AE,AF,B0,B1,B2,B3,B4,B5,B6,B7,B8,B9,BA,BB,BC,BD,BE,BF,C0,C1,C2,C3,C4,C5,C6,C7,C8,C9,CA,CB,CC,CD,CE,CF,D0,D1,D2,D3,D4,D5,D6,D7,D8,D9,DA,DB,DC,DD,DE,DF,E0,E1,E2,E3,E4,E5,E6,E7,E8,E9,EA,EB,EC,ED,EE,EF,F0,F1,F2,F3,F4,F5,F6,F7,F8,F9,FA,FB,FC,FD,FE,FF
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_ANOTACION = 87
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_BASE = 80
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_BRAZO = 81
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_CARTABON = 83
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_COLUMNA = 7F
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_PLACA = 82
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_PLACA_COLUMNA_BASE = 88
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_PLACA_COLUMNA_SEPARADOR = 8C
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_SEPARADOR = 84
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_TENSOR = 85
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_TENSOR_ADAPTADOR = 89
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_TENSOR_CARTABON = 8A
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_TENSOR_TROQUEL = 8B
RB-02c [SIDE-DB] RB-02c after the caller's rollback: added LT:RACKCAD_CANT_TROQUEL = 86
RB-03 [SIDE-DB] RB-03 after the caller's rollback: added BT:CTRL_RB03 = 78
RB-03 [SIDE-DB] RB-03 after the caller's rollback: added BTC:CTRL_RB03 = 0|ext|
RB-05 [SIDE-DB] RB-05 after the caller's rollback: added BT:*D2 = 7E
RB-05 [SIDE-DB] RB-05 after the caller's rollback: added BT:CTRL_RB05 = 78
RB-05 [SIDE-DB] RB-05 after the caller's rollback: added BTC:*D2 = 10|noext|81,82,83,84,85,86,87,88,89,8A
RB-05 [SIDE-DB] RB-05 after the caller's rollback: added BTC:CTRL_RB05 = 1|noext|7B
RB-05 [SIDE-DB] RB-05 after the caller's rollback: added LT:Defpoints = 7D
```

### Caracterizaciones en base lateral (solo caracterizacion; 128 fugas)

| Caso | dbKind | Resultado | Fugas |
|---|---|---|---|
| HV-01 | SIDE-DB-CHARACTERIZATION | FAIL | 9 |
| HV-02 | SIDE-DB-CHARACTERIZATION | FAIL | 16 |
| HV-03 | SIDE-DB-CHARACTERIZATION | FAIL | 25 |
| HV-05 | SIDE-DB-CHARACTERIZATION | FAIL | 25 |
| HV-08 | SIDE-DB-CHARACTERIZATION | FAIL | 9 |
| HV-10 | SIDE-DB-CHARACTERIZATION | FAIL | 2 |
| HV-12 | SIDE-DB-CHARACTERIZATION | FAIL | 33 |
| HV-13 | SIDE-DB-CHARACTERIZATION | PASS | 0 |
| HV-14 | SIDE-DB-CHARACTERIZATION | FAIL | 9 |

El detalle completo de cada fuga (clave y handle) esta en `hostval-evidence.json`, campos `sideCharacterizationLeaks` y `sideCharacterizations[].leaks`.

### Fin de transacciones

- Fines de transaccion registrados: 57 (casos, caracterizaciones y controles).
- Abort fallido: 0. Dispose fallido: 0. No desechada al final: 0.
- Transacciones activas antes -> despues y transaccion superior tras el fin: -1->-1 top=None, 0->0 top=null, 1->0 top=null, 2->1 top=1529186101136, 2->1 top=1529186102544.

Las comprobaciones estrictas de transacciones activas (delta relativo de -1, superior coherente, identidad del fin) pasaron en todos los casos y controles sensibles a rollback.

### HV-08

`IsSuccess=False Failure=WriteFailed BlockName=<null> DefinitionId=Null Missing=0`

## Como leerla

Arbol de decision fijado antes de la corrida: RB-01 (base lateral) con fuga y RB-01D (documento) limpio => el modelo de base lateral es el problema, no AUTH-15.
Es exactamente lo observado. Las fugas de base lateral incluyen el bloque de nombre vacio de la caracterizacion de HV-08 (`added BT: = 81`), coherente con el mismo
fallo de rollback en esa base. El mecanismo NO se identifico: el hallazgo es que una `Database` lateral construida por este arnes (`new Database(true, true)` con
`WorkingDatabase` cambiada) no revierte lo escrito tras Abort ni tras Dispose, mientras que un documento bajo `LockDocument` si.
