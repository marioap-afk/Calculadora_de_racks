# I-65 — F4: solicitud de liberación de host y procedimiento de preflight y POST de AutoCAD

Documento de la unidad (Coordinator, 2026-10-14), por la disposición del Owner «F4 Engineering Validation and Host Readiness» §3
([texto literal](../automation/decisions/I-65-owner-disposition-f4-validation-host-readiness.original.txt); decisiones
[§49](../automation/decisions/I-65.md)). Completa el [procedimiento de smoke](I-65-f4-autocad-smoke-procedure.md) (SP-07, H-1..H-7,
PC-05) con las condiciones actuales de I-52 e I-64 ([custodia](../automation/evidence/I-65-f4/host-coordination-smoke-2026-10-14.md)).

> **Estado: PREPARADO.** AUTOCAD EXECUTION = NOT AUTHORIZED. La solicitud de §1 **no se ha enviado**: se envía solo cuando el Owner lo
> ordene. Los comandos de §3-§5 **no se han ejecutado**. No se abre AutoCAD, no se ejecuta `NETLOAD` y no se toca el perfil de seguridad
> sin la liberación correspondiente.

## 0. Requisitos del Owner para la prueba y dónde se cumplen

| Requisito (disposición §3) | Dónde |
|---|---|
| DLL exacto de un SHA identificado | §2.1 y B-09; el SHA de Git y el SHA-256 del DLL van en el aviso H-4 |
| Fallo cerrado con las filas aún `proposed` | smoke SM-06 (`Unsupported`, `MATERIAL-ROW-UNVERIFIED`); el smoke va **antes** de la actualización a `verified` |
| Salida visible y ausencia de paquete | smoke SM-06..SM-08, SM-10 y PC-02 (texto literal de la línea de comandos; carpeta inexistente antes y después) |
| Perfil previo y comparación POST | §3 (BASELINE), §4 (POST) y §5 (DIFF) |
| Ventana de host de I-52 e I-64 | §1 y protocolo H-1..H-7 del smoke |

## 1. Solicitud formal de ACK + RELEASE (texto listo para enviar)

Destinatarios: «I - 52» (para que la eleve a su Coordinator, al Owner o al CAD manager, que deciden el RELEASE) y «I - 64» (para su
Coordinator). Los campos `<…>` los llena la sesión de I-65 al enviarla; ninguno se le pide al Owner.

```text
I-65 (Coordinator) — SOLICITUD FORMAL DE VENTANA DE HOST (ACK + RELEASE) para el smoke de RACKRAMPAQUETE en AutoCAD. Autorizada por el Owner el <fecha>; sin RAM.

1. Qué: smoke del comando RACKRAMPAQUETE de I-65 con la tabla de secciones y materiales del producto (filas proposed): debe responder Unsupported, mostrar el diagnóstico y NO escribir ningún paquete. No es Owner Validation ni Candidato de hito.
2. Build: rama architecture/interop-analisis-estructural, commit <SHA de Git>; código de producto idéntico a e610ea49 (git diff e610ea49 <SHA> -- src vacío). DLL: <ruta absoluta>\RackCad.Plugin.dll, SHA-256 <hash>, compilado con AutoCAD cerrado.
3. Ventana pedida: <fecha y hora>, duración estimada 60 min (línea base 10, smoke 30, POST y comparación 20). El orden frente al Smoke-1 de I-64 y a la P2 de I-52 lo fija el Coordinator de I-52 con el Owner.
4. Condiciones que I-65 cumple: una sola acad.exe; ninguna sesión de AutoCAD de I-52 ni de I-64 durante la ventana; perfil «<<Unnamed Profile>>» sin cambios; sin cambios de TRUSTEDPATHS, SECURELOAD, ACL, registro, plugins ni variables de entorno; DWG nuevo desechable, cerrado sin guardar.
5. Carga del DLL: NETLOAD único y no persistente desde la carpeta del worktree de I-65, que NO está en TRUSTEDPATHS; con SECURELOAD=1 puede aparecer el aviso de seguridad. Pido que el RELEASE diga expresamente si se autoriza «cargar una vez». Si no se autoriza, en el aviso se elige no cargar y STOP. Nunca «cargar siempre» ni añadir la carpeta a TRUSTEDPATHS.
6. Línea base y POST: procedimiento manual de solo lectura con comandos de Windows (reg query, reg export, Get-ItemProperty, Get-FileHash, icacls, Get-ChildItem), sin herramientas nuevas y sin las de T3C de I-64; se adjunta (docs/initiatives/I-65-f4-host-release-and-preflight.md §3-§5). Pido a I-52 que lo acepte o indique su propio método.
7. Avisos: aviso previo por este canal antes de abrir AutoCAD; aviso de cierre con acad.exe = 0 y el resultado de la comparación; I-52 recomprueba en solo lectura antes de reanudar; si I-65 va antes que I-64, I-64 toma referencias frescas.
8. Pido en el ACK + RELEASE: (a) ventana y quién la decide; (b) los cuatro valores de referencia de I-52 (perfil actual, SHA-256 de la cadena TRUSTEDPATHS y número de entradas, SECURELOAD, versión y SHA-256 de acad.exe); (c) qué ACL sigue I-52 (rutas); (d) si autoriza «cargar una vez»; (e) si acepta el método de §6; (f) a I-64: orden respecto de su Smoke-1 y sin solape con su BASELINE/POST.
```

## 2. Preparación de la sesión (antes de pedir la ventana; AutoCAD cerrado)

1. **Commit identificado:** el HEAD de la rama en el momento de la autorización del Owner; la sesión comprueba
   `git diff e610ea49 <HEAD> -- src` vacío y el árbol limpio. Como el ensamblado incorpora la revisión de Git, el DLL se compila **desde
   ese commit** y su SHA-256 vale solo para él.
2. **Build:** `dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug` con el `dotnet.exe` del usuario y `acad.exe` = 0.
3. **Registro:** ruta y SHA-256 del DLL; existencia y SHA-256 de la tabla copiada a la salida (igual al recurso del repositorio).
4. **Carpeta de evidencia local** nueva, fuera del repositorio y fuera de las raíces que se capturan (la carpeta temporal de la sesión):
   las exportaciones del registro contienen rutas y datos personales y **no se versionan**; en la rama solo se custodian sus SHA-256 y la
   lista clasificada de diferencias (sin datos de valores).

## 3. BASELINE (solo lectura; AutoCAD cerrado; después del RELEASE y antes de abrir AutoCAD)

`<PRODUCT>` = `Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409` (AutoCAD 2025, la clave de producto de la referencia de I-52). Los
comandos son de Windows o de PowerShell 7, sin opciones de escritura sobre el registro, el perfil, las ACL ni los archivos de AutoCAD; las
únicas escrituras son archivos nuevos en la carpeta de evidencia local.

| # | Qué | Comando (solo lectura) | Esperado |
|---|---|---|---|
| B-01 | Procesos de AutoCAD | `tasklist /FI "IMAGENAME eq acad.exe"` | ninguno |
| B-02 | Perfil actual y conjunto de perfiles | `reg query "HKCU\<PRODUCT>\Profiles" /ve` y `reg query "HKCU\<PRODUCT>\Profiles"` | `<<Unnamed Profile>>`; conjunto anotado |
| B-03 | `TRUSTEDPATHS` y `SECURELOAD` | `Get-ItemProperty -LiteralPath 'HKCU:\<PRODUCT>\Profiles\<<Unnamed Profile>>\Variables' -Name TRUSTEDPATHS,SECURELOAD`; SHA-256 de la cadena exacta en UTF-8 sin BOM ni salto final y número de entradas no vacías separadas por `;` | los valores del ACK de I-52 (hoy: 19 entradas, `a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498`; `SECURELOAD` = 1) |
| B-04 | Árboles del registro de AutoCAD | `reg export` a archivos nuevos de: `HKCU\<PRODUCT>\Profiles`, `HKCU\<PRODUCT>\FixedProfile`, `HKCU\<PRODUCT>\Applications`, `HKCU\<PRODUCT>\InstalledApplications`, `HKCU\<PRODUCT>\Loaded` y `HKLM\SOFTWARE\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Applications`; `Get-FileHash` de cada archivo | seis archivos (o «ausente», que debe repetirse en el POST) |
| B-05 | Identidad de `acad.exe` | `reg query "HKLM\SOFTWARE\Autodesk\AutoCAD\R25.0\ACAD-8101:409" /v AcadLocation`; `Get-FileHash` y `(Get-Item <ruta>\acad.exe).VersionInfo.FileVersion` | los del ACK de I-52 (hoy: R25.0.171.0.0, `2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422`) |
| B-06 | Plugins instalados | `Get-ChildItem -Name -Force` de `%APPDATA%\Autodesk\ApplicationPlugins`, `%ProgramData%\Autodesk\ApplicationPlugins`, `%ProgramFiles%\Autodesk\ApplicationPlugins` y `%ProgramFiles(x86)%\Autodesk\ApplicationPlugins` | listas anotadas; ningún bundle de RackCad |
| B-07 | Variables de entorno persistentes | nombres de `[Environment]::GetEnvironmentVariables('User')` y `('Machine')` que contengan `RackCad` | ninguna |
| B-08 | ACL | `icacls "<AcadLocation>"`, `icacls "<carpeta del DLL>"` y las rutas que I-52 indique en su ACK | listados anotados |
| B-09 | DLL | `Get-FileHash` del DLL; ¿alguna entrada de `TRUSTEDPATHS` cubre su carpeta? | SHA-256 del aviso H-4; no la cubre |
| B-10 | Checkouts | instantánea K1/K2 (estado Git de todos los worktrees y SHA-256 de los rastreados del checkout principal) | registrada |

**Antes de abrir AutoCAD:** si B-02, B-03 o B-05 no coinciden con el ACK de I-52, **STOP** sin abrir AutoCAD: es una deriva previa de la
configuración de I-52; se informa a I-52 y al Owner y I-65 no la corrige. La línea base no se repite para buscar otro resultado.

## 4. POST (solo lectura)

Con AutoCAD **cerrado del todo** (`acad.exe` = 0, sin forzar su cierre) y antes de cualquier otra sesión de AutoCAD de I-52 o I-64: se
repiten B-01..B-10 en una carpeta POST nueva, con los mismos comandos y las mismas claves.

## 5. DIFF y clasificación (manual; sin herramientas nuevas)

1. Comparar los SHA-256 de cada exportación de B-04 y los valores de B-02, B-03, B-05..B-09 entre BASELINE y POST.
2. Para cada exportación que difiera: `Compare-Object (Get-Content <base>.reg) (Get-Content <post>.reg)` (lectura de los dos archivos;
   PowerShell detecta la codificación UTF-16 de `reg export`), y se anotan **solo las rutas de clave y los nombres de valor** que
   cambian, con usuario y máquina sustituidos por marcadores.
3. Clasificación (regla conservadora; en caso de duda, la clase más grave):

| Clase | Cambios | Consecuencia |
|---|---|---|
| **PROHIBIDO** | cualquier byte de `TRUSTEDPATHS` o su número de entradas; `SECURELOAD`; perfil actual o conjunto de perfiles; cualquier cambio en `Applications`, `InstalledApplications` o `Loaded` (HKCU) o en `Applications` (HKLM); listas de `ApplicationPlugins`; versión o SHA-256 de `acad.exe`; listados de ACL; variables con `RackCad`; que una entrada de `TRUSTEDPATHS` pase a cubrir la carpeta del DLL | **STOP**, incidente de I-65, aviso inmediato a I-52, I-64 y el Owner; sin reparación automática (la referencia exacta es la BASELINE local) |
| **INFORMAR** | otros cambios dentro de `Profiles\<<Unnamed Profile>>` o de `FixedProfile` (actividad normal de AutoCAD: archivos recientes, posiciones de ventanas y diálogos) | lista literal de claves y nombres de valor a I-52, que decide si afecta a su tupla |
| **LIMPIO** | sin diferencias | — |

4. Resultado del DIFF: `LIMPIO`, `INFORMAR` (solo cambios de esa clase) o `PROHIBIDO`. PC-05 del smoke se cumple con `LIMPIO`, o con
   `INFORMAR` si I-52 lo acepta expresamente.

## 6. Paradas propias del host

Otra `acad.exe` antes o durante la ventana; RELEASE ausente o ambiguo; deriva previa en B-02/B-03/B-05; aviso de seguridad al cargar sin
autorización de «cargar una vez»; cualquier clase PROHIBIDO; imposibilidad de leer una clave de B-02..B-05 (la línea base sería
INVALID). Toda parada → registro literal, aviso de cierre y decisión del Owner.
