# I-65 — F4: procedimiento de smoke de `RACKRAMPAQUETE` en AutoCAD (sin RAM)

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Engineering Validation Preparation» §6
([texto literal](../automation/decisions/I-65-owner-disposition-f4-engineering-validation-preparation.original.txt); decisiones
[§47](../automation/decisions/I-65.md)). Responde a A65-F4R-08 de la revisión del Architect: el comando nunca se ejecutó en AutoCAD.

> **Estado: PREPARADO, NO AUTORIZADO.** AUTOCAD HOST EXECUTION = NO. No se abre AutoCAD ni se ejecuta `NETLOAD` sin autorización
> específica del Owner y sin el RELEASE de host de §2. Este smoke **no** es la validación del Owner de ningún dibujo, **no** declara un
> Candidato de hito y **no** ejecuta RAM Elements.

## 1. Objetivo y alcance

Comprobar en el host real, con la tabla del producto tal como se distribuye (dos filas `proposed`), que:

1. el DLL de I-65 carga con un `NETLOAD` único y no persistente y registra `RACKRAMPAQUETE`;
2. el comando pide sus entradas (rack, estación, archivo de cargas y carpeta nueva) y las cancelaciones no escriben nada;
3. falla **cerrado y visible**: con la tabla del producto responde `Unsupported` con `MATERIAL-ROW-UNVERIFIED` y **no crea la carpeta**;
4. los otros fallos visibles derivados del código (`Unsupported` por tipo de rack; `Unavailable` por archivo de cargas inexistente) se
   ven en la línea de comandos;
5. queda capturado ya lo que OV-I65-03 puede mostrar con la tabla del producto (estación no soportada o sección sin fila → sin paquete y
   diagnóstico visible);
6. no queda ningún cambio en el host (perfil, `TRUSTEDPATHS`, `SECURELOAD`, plugins, registro, ACL) ni en los checkouts.

Fuera de alcance: generar un paquete válido (imposible hasta que las filas sean `verified`), RAM, el comparador y las OV-I65-01/04..07.

## 2. Coordinación de host con I-52 e I-64

Consulta enviada por el canal entre sesiones el 2026-10-14 a «I - 52» y a «I - 64» (textos y respuestas literales en la evidencia §53 y §54).

**Respuesta de I-64** (posición de su sesión; los puntos 2 y 3 los eleva a su Coordinator):

- Su reintento de Smoke-1 (Candidato `09f2f1c3`, DLL `2923E7F2…AE26`) está en `COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED`: sin
  autorización, sin fecha y sin ventana reservada; según I-64, la P2 H10 de I-52 sigue HELD.
- Sus herramientas de perfil de T3C (`Capture-ProfileState.ps1`, `Compare-ProfileState.ps1`, `Test-ProfileDiff.ps1`) viven solo en su rama,
  no están integradas, se aprobaron solo para su Smoke-1 y clasifican pensando en su Workspace. **I-65 no las usa** sin una disposición
  expresa del Coordinator de I-64.
- Para no solaparse: una sola `acad.exe`; ninguna sesión de AutoCAD ajena entre la BASELINE y el POST de I-64 (si no, su STOP-02 /
  INVALID); **aviso previo** por el canal antes de que I-65 abra AutoCAD y **aviso de cierre** con `acad.exe` = 0; si I-65 va antes que
  I-64, I-64 necesita después referencias de host frescas (`acad.exe`, perfil, cadena de `TRUSTEDPATHS` de 19 entradas `a9ba0fd1…2498`,
  `SECURELOAD`); si I-65 va después, I-64 avisa cuando su POST esté tomado y `acad.exe` = 0, y antes de la ventana de I-65 va también la
  recomprobación de I-52; ni I-64 ni I-65 dejan cambios persistentes.

**Respuesta de I-52** (informativa; no es ACK ni RELEASE; evidencia §54):

- Su campaña CT-21D está en **pausa de host**, sin sesión de AutoCAD de I-52 (`acad.exe` = 0 en su última lectura); su P2 (H10) está
  preparada pero **no** autorizada.
- El perfil «<<Unnamed Profile>>» es su referencia: perfil, `TRUSTEDPATHS` (19 entradas, `a9ba0fd1…2498`), build de `acad.exe`
  (R25.0.171.0.0, `2a75996f…6422`) y ACL **no deben cambiar**; cualquier deriva afecta su tupla.
- Reglas para otra iniciativa: aviso previo por el canal; el **RELEASE lo decide el Coordinator, el Owner o el CAD manager de I-52**
  (no su sesión); una sola `acad.exe` sin solaparse con sesiones de I-52; sin cambios de perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL,
  registro ni plugins; carga única no persistente (nunca «cargar siempre» ni carpetas nuevas en `TRUSTEDPATHS`); **captura de línea base
  antes y POST + diff después**; AutoCAD cerrado del todo y aviso a I-52, que recomprueba en solo lectura antes de reanudar. El orden de
  ventanas lo fija el Coordinator de I-52.

**Línea base y POST + diff:** I-52 los exige y las herramientas de T3C de I-64 no se usan sin la disposición de su Coordinator. El
método de I-65 es el **procedimiento manual de solo lectura** con comandos de Windows de
[I-65-f4-host-release-and-preflight.md](I-65-f4-host-release-and-preflight.md) §3-§5 (decisiones §49), sin herramientas nuevas; I-52 lo
acepta o indica el suyo en su ACK. La solicitud formal de ACK + RELEASE está en ese documento §1.

**Protocolo de ventana para I-65** (se aplica cuando el Owner autorice el smoke):

| Paso | Acción | Responsable |
|---|---|---|
| H-1 | Pedir la ventana por el canal a I-52 y a I-64 con fecha, duración estimada, DLL (ruta y SHA-256) y este procedimiento | sesión I-65 |
| H-2 | Recibir **ACK + RELEASE explícitos**: el de I-52 lo decide su Coordinator, el Owner o el CAD manager; el de I-64, su Coordinator. El orden de ventanas lo fija el Coordinator de I-52 con el Owner | I-52, I-64, Owner |
| H-3 | Línea base del host con el método acordado (SP-07) | según SP-07 |
| H-4 | Aviso previo de apertura por el canal | sesión I-65 |
| H-5 | Smoke (§5) | operador |
| H-6 | AutoCAD cerrado del todo; POST + diff del host con el mismo método | según SP-07 |
| H-7 | Aviso de cierre con `acad.exe` = 0 y resultado del diff; I-52 recomprueba en solo lectura antes de reanudar; si I-65 fue antes que I-64, I-64 toma referencias frescas. Cualquier deriva es incidente de I-65 | sesión I-65; I-52; I-64 |

## 3. Precondiciones (todas)

| # | Condición | Quién |
|---|---|---|
| SP-01 | Autorización específica del Owner para este smoke (AUTOCAD HOST EXECUTION) | Owner |
| SP-02 | ACK + RELEASE de I-52 y de I-64 (§2, H-2) | sesión |
| SP-03 | SHA declarado del build: rama `architecture/interop-analisis-estructural`; producto de `e610ea49` (código de producto sin cambios desde entonces); si el Owner exige que sea un Candidato, la decisión OD-F4-13 lo fija | sesión; Owner |
| SP-04 | AutoCAD cerrado (`acad.exe` = 0) antes de compilar; ningún otro usuario de AutoCAD en la estación | sesión |
| SP-05 | Worktree de I-65 limpio y = `origin`; instantánea K1/K2 del checkout principal y de los worktrees (controles del Owner) | sesión |
| SP-06 | Ningún cambio de perfil, `TRUSTEDPATHS`, `SECURELOAD`, plugins, registro ni ACL previsto | sesión; operador |
| SP-07 | Línea base y POST + diff del host con el procedimiento manual de solo lectura de [I-65-f4-host-release-and-preflight.md](I-65-f4-host-release-and-preflight.md) §3-§5, aceptado por I-52 en su ACK (o el método que I-52 indique) | sesión; I-52 |

## 4. Preparación de la sesión (sin AutoCAD abierto)

1. Compilar el Plugin desde el worktree de I-65 con el `dotnet.exe` del usuario:
   `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug`.
2. Registrar: ruta absoluta y SHA-256 de `src/RackCad.Plugin/bin/Debug/net8.0-windows/RackCad.Plugin.dll`, SHA de Git y hora.
3. Comprobar que existe `src/RackCad.Plugin/bin/Debug/net8.0-windows/structural-analysis/ram-elements/section-material-map.json` y que
   su SHA-256 es igual al del recurso del repositorio (`5229231e0d924e48a839ef77bf0b63a2b631214a670783e01d148b9ac6cb4048` en `191344ff`).
4. Fijar las entradas:
   - archivo de cargas: `tests/RackCad.Tests/StructuralAnalysis/Fixtures/ref-cant-01.loads.json` del worktree, por ruta absoluta (solo
     lectura; SHA-256 `25f48e7f75d879c4668c3d9b37f69fda0cd5442a407c03ff76362754623cae05`);
   - carpeta del paquete: una ruta corta que **no exista** (p. ej. `C:\r65-smoke\pkg-01`); la sesión comprueba que no existe antes y
     después;
   - un archivo de cargas inexistente para SM-08 (ruta que la sesión comprueba que no existe).
5. Pegar al operador en el chat la lista de pasos de §5 con las rutas exactas (no se le piden datos de máquina).

## 5. Pasos del smoke (operador; la sesión registra)

| # | Acción | Resultado esperado (del código de `e610ea49`) | ☐ |
|---|---|---|---|
| SM-01 | Abrir **una sola** AutoCAD 2025 con el perfil habitual y un dibujo **nuevo** desechable | una `acad.exe`; anotar el perfil que muestra AutoCAD | ☐ |
| SM-02 | `NETLOAD` → seleccionar el DLL exacto de §4.2 | carga sin diálogo de seguridad. **Si aparece cualquier diálogo de seguridad o de confianza: STOP**, cerrar sin cambiar ajustes (no elegir «cargar siempre») y registrar | ☐ |
| SM-03 | Teclear `RACKRAMPAQUETE` y pulsar `Esc` en «Selecciona el rack Cantilever:» | el comando existe (no «comando desconocido»); termina sin mensaje y sin escribir nada | ☐ |
| SM-04 | Dibujar `REF-CANT-01` con `RACKCANTILEVER` (§6) e insertar una vista | rack Cantilever en el dibujo | ☐ |
| SM-05 | `RACKRAMPAQUETE` → seleccionar una línea u otro objeto que no sea bloque | «Ese objeto no es un rack.» y vuelve a pedir; `Esc` termina sin escribir | ☐ |
| SM-06 | `RACKRAMPAQUETE` → rack de §6 → estación `0` → archivo de cargas de §4.4 → carpeta de §4.4 | `RackCad paquete estructural: Unsupported. El modelo estructural de la estacion no es valido; no se escribio nada.` y, al menos, `  Blocking MATERIAL-ROW-UNVERIFIED (AISC-W-W12X26)` y `  Blocking MATERIAL-ROW-UNVERIFIED (AISC-HSS-RECT-HSS4X4X_250)`; sin «Huella de entrada» ni «Archivo escrito»; la carpeta **no existe** | ☐ |
| SM-07 | Igual que SM-06 pero `Esc` en «Indice de estacion», luego en «Archivo de cargas» y luego en «Carpeta NUEVA del paquete» (tres ejecuciones) | cada una termina sin mensaje y sin escribir nada | ☐ |
| SM-08 | Igual que SM-06 con el archivo de cargas inexistente | `RackCad paquete estructural: Unavailable.` seguido del mensaje del sistema; nada escrito | ☐ |
| SM-09 | (Opcional) Dibujar un rack de otro tipo (p. ej. un selectivo) y ejecutar `RACKRAMPAQUETE` sobre él | `RackCad paquete estructural: Unsupported. Solo se exporta un rack Cantilever; el elegido es de tipo <Kind>.` | ☐ |
| SM-10 | Capturas de OV-I65-03 con la tabla del producto (§7) | `Unsupported` (o `Invalid`) visible, sin «Archivo escrito», carpeta inexistente; anotar los códigos que aparezcan | ☐ |
| SM-11 | Copiar el texto de la línea de comandos (F2) y cerrar AutoCAD **sin guardar** | `acad.exe` = 0 | ☐ |

Si un resultado difiere del esperado: registrar el texto exacto y STOP (no repetir con otro DLL ni con otro dibujo sin decisión).

## 6. Dibujo de prueba `REF-CANT-01` (parámetros de F1 §1)

En `RACKCANTILEVER` (editor «RackCad – Línea Cantilever: editor»), dejar todo lo demás con sus valores por omisión:

| Campo del editor | Valor | Campo del diseño |
|---|---|---|
| Estaciones | 2 | `StationCount` |
| Separación entre centros (in) | 48 | `ColumnCentreSpacing` |
| Caras | sencilla | `FaceMode` `Single` |
| Lado (solo en sencilla) | Y positivo | `SingleSide` `PositiveY` |
| Niveles | 3 | `LevelCount` |
| Troquel del primer nivel (índice) | 0 | `FirstLevelPunchIndex` |
| Claro solicitado entre niveles (in) | 24 | `RequestedClearHeight` |
| Columna y base → Sección de columna | W12X26 | `AISC-W-W12X26` |
| Columna y base → Sección de base | W12X26 | `AISC-W-W12X26` |
| Columna y base → Longitud de base (in) | 48 | `Base.Length` |
| Brazo (por omisión) → Arreglo | sencillo | `Arrangement` `Single` |
| Brazo (por omisión) → Sección del brazo | HSS4X4X1/4 | `AISC-HSS-RECT-HSS4X4X_250` |
| Brazo (por omisión) → Longitud de corte (in) | 36 | `CutLength` |
| Brazo (por omisión) → Pendiente (subida por 12 in) | sin cambiar (7/16 = 0.4375 por omisión) | `CantileverDefaults.ArmSlopeRisePer12` |

Las etiquetas de las opciones («sencilla», «Y positivo», «sencillo») se confirman en el propio smoke y se anotan. El JSON de referencia del
diseño es [`ref-cant-01.design.json`](../automation/evidence/I-65-f1/ref-cant-01.design.json) (SHA-256
`6725a016d9585c5d0dde15d28ebc186fd4f5ac71930bdd55f901a9483b9572d5`); la igualdad exacta del dibujo con él se comprueba en la OV
(OD-F4-09), no en este smoke.

## 7. Capturas de OV-I65-03 con la tabla del producto (SM-10)

Cada variante se hace **editando el rack de prueba** con `RACKEDITAR` → `Actualizar` en el dibujo desechable (o dibujando otro) y
ejecutando `RACKRAMPAQUETE` con las entradas de SM-06:

| Variante | Cambio | Código esperado (además puede aparecer `MATERIAL-ROW-UNVERIFIED`) |
|---|---|---|
| OV3-a | Caras: doble | `STATION-FACE-UNSUPPORTED` |
| OV3-b | Lado: Y negativo | `STATION-SIDE-UNSUPPORTED` |
| OV3-c | Pendiente del brazo con más de 10° (si el editor la admite) | `ARM-SLOPE-UNSUPPORTED` |
| OV3-d | Arreglo del brazo: canales pareados | `ARM-ARRANGEMENT-UNSUPPORTED` (y puede aparecer `SECTION-FAMILY-UNSUPPORTED` por la familia C) |
| OV3-e | Sección de columna sin fila en la tabla (otra W del catálogo) | `MATERIAL-ROW-MISSING` para esa sección |

El criterio es el de OV-I65-03: **sin paquete y diagnóstico visible** (`Unsupported` o `Invalid`). Los códigos se registran tal como
aparecen: el orden y la presencia de `MATERIAL-ROW-UNVERIFIED` dependen de dónde se detenga la fuente, y eso no cambia el criterio. El
caso «fila sin verificar» queda evidenciado por SM-06 con la tabla del producto, sin alterarla nunca.

## 8. Comprobaciones posteriores (sesión)

| # | Comprobación | Esperado |
|---|---|---|
| PC-01 | `acad.exe` = 0 | sí |
| PC-02 | Carpeta del paquete y carpetas de §4.4 | no existen |
| PC-03 | K1/K2: checkout principal y demás worktrees sin cambios; worktree de I-65 limpio (`bin/` y `obj/` ignorados) | sin cambios |
| PC-04 | SHA-256 del DLL cargado | igual al de §4.2 |
| PC-05 | Host: perfil, `TRUSTEDPATHS`, `SECURELOAD`, plugins | sin cambios, medido con el método de SP-07 (las herramientas de I-64 no se usan sin su disposición) |
| PC-06 | Aviso de cierre a I-52 e I-64 (H-6) | enviado |

## 9. Paradas

Otra `acad.exe` abierta o ventana de host no liberada; cualquier diálogo de seguridad al cargar; una excepción no capturada o un cierre
de AutoCAD; cualquier archivo o carpeta escritos por el comando; un `Valid` con la tabla del producto (sería un PASS en falso: STOP
inmediato); diferencia con un resultado esperado de §5. Toda parada → registro literal, aviso de cierre a I-52 e I-64 y decisión del
Owner.

## 10. Resultado y custodia

- **PASS del smoke** = SM-01..SM-08, SM-10 y SM-11 como se esperaba (SM-09 opcional), PC-01..PC-06 cumplidas (PC-05 con el método
  de SP-07). No es Owner Validation ni GATE PASS; deja cerrada A65-F4R-08 y aporta la captura parcial de OV-I65-03.
- Custodia en `docs/automation/evidence/I-65-f4/`: texto de la línea de comandos, capturas, SHA-256 del DLL y de la tabla, registro del
  operador (observaciones y firma) y los mensajes de ventana con I-52 e I-64.
