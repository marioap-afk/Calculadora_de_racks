# I-64 — F1-T3D-SMOKE-PROCEDURE: procedimiento exacto de Smoke-1 para el operador

**Estado:** PREPARADO por la sesión responsable el 2026-10-11 como tarea documental autorizada por la revisión de T3C
(`docs/automation/evidence/I-64-pilot/F1-T3C-PROFILE-DOC/coordinator-review.md`, evidencia §59). Se revisa dentro del cierre del gate
F1 (**COORDINATOR_F1_GATE_CLOSE_REQUIRED**).

Este documento:
- **no** autoriza Smoke-1; Smoke-1 se ejecuta solo **después de F1 PASS** y con una autorización expresa del Coordinator;
- no autoriza el host de I-52 ni pide la Owner Validation;
- no cambia `src/`, `tests/` ni archivos de proyecto;
- **ningún paso se ejecutó** al redactarlo.

Las decisiones que necesita antes de ejecutarse están en la sección 12 (D-T3D-01..06).

## 0. Autoridad y límites

**Autoridades:**
- **Freeze V5** (`docs/initiatives/I-64-proposal-v5.md`):
  - §11: Smoke-1 ocurre tras F1 PASS;
  - §12.1: reglas comunes de los hitos;
  - §12.2: S1-01..S1-12;
  - D-01: host, foco y perfil;
  - D-18: escenarios de host 1, 2, 3, 9 y 10;
  - invariantes INV-14 (comprobación Debug), INV-21 (diff de perfil), INV-24 (foco, solo host) e INV-33;
  - matriz OV: OV-01..04, 14, 15, 17, 19, 20 y 22.
- **Procedimiento de diff de perfil de T3C**, aprobado (`docs/initiatives/I-64-f1-t3c-profile-diff-procedure.md`), con los dictámenes
  de su sección 15.
- **Reglas de host de I-52:**
  - sin actualizaciones;
  - sin cambios de perfil, plugins, `TRUSTEDPATHS` ni `SECURELOAD`;
  - una sola `acad.exe`;
  - sin solaparse con sus sesiones;
  - aviso previo por el canal entre sesiones.
- **Guía de validación manual:** `docs/guias/validacion-manual-autocad.md`, §1 a §3 y §7.1 (Candidato).

**Límites de la Owner Validation:**
- Smoke-1 es un **hito intermedio del Owner**: no es la Owner Validation final ni sustituye al `FINAL_CANDIDATE_SHA` (§12.1).
- El Owner ejecuta AutoCAD; la sesión prepara, registra y custodia; ningún Worker usa AutoCAD.
- Solo se valida lo que F1 entrega. No se prueban navegación (F2), pestañas (F3), borradores ni Actualizar (F4), editores en el panel
  (F5) ni inventario (F6).

**Producto validado:** el de F1, verificado en `e0205b5a9f000bbd44aa1f7556d30f06411c3db7`. El Candidato de Smoke-1 es el SHA exacto
que nombre la autorización del Coordinator. Entre `e0205b5a` y el Candidato, `git diff --stat` sobre `src/` y `tests/` debe estar vacío,
salvo una diferencia autorizada de forma expresa.

## 1. Roles

| Rol | Hace | No hace |
|---|---|---|
| **Owner** (operador en la estación) | compilar el Candidato, las capturas BASELINE y POST, AutoCAD, los pasos SM, llenar el registro del operador, firmar | cambiar perfil, seguridad o registro; reparar |
| **Sesión responsable** | crear el `RunId` y el directorio de ejecución, transcribir, comparar, custodiar | operar AutoCAD o tocar la estación del host |
| **I-52** | RELEASE de la ventana de host y acuse | — |
| **CAD manager** | autorizar o no la carga única del DLL | — |
| **Coordinator** | autorizar Smoke-1, resolver la sección 12, disponer el resultado | — |

## 2. Prerrequisitos (todos SÍ antes de la sección 4; cualquier NO es STOP)

| Id | Prerrequisito | Prueba |
|---|---|---|
| PR-01 | F1 PASS registrado por el Coordinator | revisión del cierre del gate F1 |
| PR-02 | autorización de Smoke-1 del Coordinator, con: `RunId`, SHA del Candidato y las decisiones D-T3D-01..06 resueltas | transcripción literal |
| PR-03 | Candidato completo (sección 3) | `candidate.md` |
| PR-04 | RELEASE explícito de I-52 y acuse con: los cuatro valores de referencia de T3C (cadena `TRUSTEDPATHS`, `SECURELOAD`, perfil, SHA-256 de `acad.exe`); la aceptabilidad de la clave de estado de la paleta (`ACCEPTED` o `NOT_ACCEPTED`); y, si hace falta, la autorización expresa de una carga única no persistente del DLL | transcripción literal |
| PR-05 | aviso previo a I-52; ninguna sesión de I-52 abierta ni prevista en la ventana | canal entre sesiones |
| PR-06 | 0 procesos `acad`; ninguna actualización de AutoCAD o Windows en curso o pendiente de reinicio | `Get-Process acad` |
| PR-07 | dibujos de prueba preparados por el Owner, con su SHA-256 antes del smoke (sección 2.1) | `fixtures.md` |
| PR-08 | herramientas de T3C con su SHA-256 aprobado y autoprueba 26/26 | T3C §2 PC-4 y PC-8 |
| PR-09 | directorio de ejecución creado y vacío: `artifacts/orchestration/I-64/F1-SMOKE-1/<RunId>/` | sección 10 |
| PR-10 | ninguna variable de entorno persistente con `RackCad` en el nombre. Smoke-1 no usa inyecciones de fallos: el producto de F1 no tiene ninguna para el panel (D-T3D-02) | captura BASELINE |
| PR-11 | biblioteca de bloques: ruta resuelta y SHA-256 (guía §7.1); no se toca | `candidate.md` |

### 2.1 Dibujos de prueba

Son copias recuperables que el Owner prepara antes, con su SHA-256 registrado. **Nunca se guardan** durante el smoke: al terminar
se cierran sin guardar, y su SHA-256 debe ser el mismo después.

| Id | Contenido mínimo | Uso |
|---|---|---|
| **A** | ≥ 2 racks dibujados por RackCad con identidad (RackId distintos); ≥ 1 copia de uno de ellos (mismo RackId); ≥ 1 bloque que no sea rack; ≥ 1 entidad suelta (línea); si existe, un bloque con definición ilegible o de versión futura (D-T3D-03) | S1-04, S1-06, S1-08, ocultar y mostrar, escenarios 2 y 3 |
| **B** | ≥ 1 rack selectivo con datos editables (para RACKEDITAR) y ≥ 1 rack con identidad | S1-05, S1-07, escenario 9 |
| **C** | dibujo de trabajo con muchas entidades (el Owner anota cuántas); copia descartable | S1-01..S1-03, S1-10 (escenarios 1 y 10) |

## 3. Identidad del Candidato y SHA exacto del DLL

En el worktree de I-64, con AutoCAD cerrado (AutoCAD bloquea el DLL), en PowerShell:

```powershell
git status --porcelain                      # vacío
git rev-parse HEAD                          # = SHA del Candidato de la autorización
git diff --stat e0205b5a9f000bbd44aa1f7556d30f06411c3db7 HEAD -- src tests   # vacío (o la diferencia autorizada)
dotnet --version
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj -c Debug
dotnet test tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj -c Debug
dotnet build src/RackCad.UI/RackCad.UI.csproj -c Debug
dotnet build src/RackCad.Plugin/RackCad.Plugin.csproj -c Debug
Get-FileHash src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad*.dll -Algorithm SHA256
gh run view <id de la corrida push del SHA> --json headSha,event,conclusion,jobs
```

Las suites se ejecutan **después** del commit del Candidato, con el árbol limpio (guía §7.1). Se registra el bloque del Candidato de
la guía §7.1 en `<run>/candidate/candidate.md`:

```text
Candidate SHA:        <sha exacto>
Arbol limpio:         SI
SDK resuelto:         <dotnet --version>
Core Full local:      PASS (<superadas>/<total>)
UI Full local:        PASS (<superadas>/<total>, <omitidas> Skip)
Debug UI build:       PASS
Debug Plugin build:   PASS
CI exact SHA:         GREEN (run <id>, job "UI Tests (WPF controls, net8.0-windows)" success; los cuatro jobs success)
Owner validation:     pending (Smoke-1, hito intermedio)
Biblioteca de bloques: <ruta resuelta>  SHA-256 <hash>
DLL cargado:          <worktree>\src\RackCad.Plugin\bin\Debug\net8.0-windows\RackCad.Plugin.dll  SHA-256 <hash>
Otros DLL RackCad:    <nombre>  SHA-256 <hash>  (uno por línea)
AutoCAD:              AutoCAD 2025 <FileVersion de acad.exe>
```

- El DLL que se carga es siempre ese archivo. Su SHA-256 es el `-CandidateDll` de las capturas BASELINE y POST, y **no puede cambiar**
  entre ambas: si cambia, la comparación es INVALID (R-INV-02).
- Si falta cualquier línea, no hay Candidato y no hay Smoke-1.

## 4. BASELINE

Orden exacta de T3C §3, con AutoCAD cerrado, `-CandidateDll` = el DLL de la sección 3 y los cuatro valores del acuse de I-52. La
salida va a `<run>/profile/`; la salida de consola se copia literal en `<run>/console/baseline.txt`.

- **Código 0:** se continúa.
- **Cualquier otro código:** STOP (T3C §3).

## 5. Arranque de AutoCAD y carga

| Paso | Acción exacta | Esperado | Si no |
|---|---|---|---|
| SM-00a | Anotar la hora UTC de inicio. Abrir AutoCAD 2025 con el acceso habitual del usuario, sin opciones de línea de órdenes (`/p`, `/b`, `/ld`) y sin elegir ni crear perfiles | se abre `Drawing1` (dibujo nuevo); una sola `acad.exe` | un selector de perfil u otra `acad.exe`: STOP |
| SM-00b | En la línea de órdenes: `(getvar "CPROFILE")`, `(getvar "SECURELOAD")` y `(getvar "PICKFIRST")` (solo lectura) | `CPROFILE` = perfil del acuse; `SECURELOAD` = valor del acuse; anotar `PICKFIRST` | discrepancia de perfil o de `SECURELOAD`: STOP |
| SM-00c | Abrir el dibujo C. Escribir `(sslength (ssget "_X"))` y anotar el número de entidades | número > 0 | — |
| SM-00d | `NETLOAD` → elegir **exactamente** el DLL de la sección 3 | se carga sin mensaje de error | ver la regla de seguridad, debajo |

**Regla de seguridad de la carga** (dictamen O-4 de T3C). Si AutoCAD muestra su aviso de seguridad (carpeta no confiable con
`SECURELOAD=1`):
- **solo** si el RELEASE o acuse de I-52, o el CAD manager, autorizaron de forma expresa la carga única no persistente, se elige la
  opción de cargar **una sola vez**;
- **nunca** «cargar siempre» ni «confiar en esta ubicación»;
- no se añade la carpeta a `TRUSTEDPATHS`, no se cambia `SECURELOAD`, no se crea otro perfil y no se registra RackCad de forma
  persistente;
- sin esa autorización, o si AutoCAD solo ofrece un cambio persistente: no se carga, **STOP-06 / HOST-TO-CONFIRM**, se cierra AutoCAD
  sin guardar, se toma el POST (sección 8) y se devuelve el caso a I-52 y al CAD manager.

## 6. Secuencia del smoke

**Convenciones:**
- «Esperar el reposo»: no tocar nada durante 2 s; el panel se actualiza en el siguiente reposo de AutoCAD.
- Los textos entre comillas son los literales del producto en `e0205b5a`.
- Cada paso SM se anota en el registro del operador (sección 10.2) con: PASS, FAIL, NOT_EXECUTED (con motivo) o NOT_APPLICABLE (solo
  por decisión de la sección 12); lo observado; y los valores medidos.
- Las capturas de pantalla se limitan a la paleta y a la línea de órdenes, nunca la barra de título (muestra rutas).

**Medición de un comando masivo (escenario 10)** en el dibujo C, con una sola línea de AutoLISP de lectura del reloj, sin escribir
ninguna variable:

```text
(progn (setq t0 (getvar "MILLISECS")) (command "_.MOVE" "_ALL" "" "0,0" "1,0") (princ (strcat "\nSM-T ms=" (itoa (- (getvar "MILLISECS") t0)))) (princ))
```

Se anota el valor `ms`. Sin umbrales (D-18): solo se registra.

### 6.1 Fase A: panel nunca abierto (dibujo C)

| Paso | Acción exacta | Esperado | S1 / OV / INV / escenario |
|---|---|---|---|
| SM-01 | En C, antes de cualquier `RACKPANEL`: ejecutar la medición del comando masivo | anotar `ms` («nunca abierto»); ningún mensaje de RackCad | S1-10, escenario 1 y escenario 10 (estado 1); P-01 |

### 6.2 Fase B: panel en un dibujo recién abierto (dibujo A)

| Paso | Acción exacta | Esperado | S1 / OV / INV / escenario |
|---|---|---|---|
| SM-02 | Abrir A. Antes de cualquier orden de dibujo: `(getvar "DBMOD")` | anotar el valor; bit 1 (valor AND 1) = 0 en un dibujo recién abierto | S1-08 (O-12), lectura previa |
| SM-03 | `RACKPANEL` | la línea de órdenes muestra «RackCad: panel Workspace mostrado.»; aparece la paleta «RackCad» con la pestaña «Workspace»; tras el reposo, estado «Sesión activa», «El panel sigue el dibujo activo.», «Sesión <n>» (anotar n), «Leer el dibujo» y «Escribir en el dibujo» marcados, «Selección» = «Ninguno» | S1-01 (panel abierto), INV-02 |
| SM-04 | `RACKPANEL` otra vez | mismo panel; no aparece una segunda paleta; el mensaje se repite | INV-02 |
| SM-05 | Marcar «Medir en memoria (no escribe en el registro)» | «Instrumentación activada: <k> puntos de medida.» y «Memoria del proceso: <x> MB»; anotar las filas (secuencia, punto, ms, MB, recuento) | D-18 (instrumento en memoria; D-T3D-05) |
| SM-06 | Clic en un rack con identidad (sin orden activa); esperar el reposo | «Un rack» / «Rack seleccionado: <RackId>»; anotar el RackId; aparecen filas `drain` y `selection.read` con sus ms | S1-04, OV-04, escenario 3 |
| SM-07 | Añadir con clic la copia del mismo rack (mismo RackId) | sigue «Un rack» con el mismo RackId | S1-04 |
| SM-08 | Añadir un segundo rack con otro RackId | «Selección actual» / «Selección actual: 2 racks y 0 otros objetos.» | S1-04, OV-14 |
| SM-09 | Añadir una línea suelta | «Selección actual: 2 racks y 1 otros objetos.» | S1-04, OV-14 |
| SM-10 | `Esc` (vaciar la selección); clic en el bloque que no es rack | «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.» | S1-04, INV-16 |
| SM-11 | `Esc`; clic en el bloque de definición ilegible, si A lo tiene (D-T3D-03) | «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.» | S1-04, INV-17 |
| SM-12 | `Esc`; no tocar nada durante 60 s; después cerrar la paleta con su botón de cierre y volver a abrirla con `RACKPANEL` | entre la última fila anterior a la espera y la fila `panel.hide` no hay ninguna fila `drain` ni `selection.read`; en la línea de órdenes no aparece «Regenerating model» ni otro mensaje de regeneración | S1-09, escenario 2 (P-02), INV-01 |
| SM-13 | Iniciar `LINE` y **no** marcar ningún punto | «Leer el dibujo» y «Escribir en el dibujo» desmarcados; «Hay un comando en curso.»; después `Esc`: vuelven a marcarse y el texto desaparece | S1-06f |
| SM-14 | Comprobaciones de foco adaptadas, pendientes de D-T3D-01 (sección 6.6) | según la sección 6.6 | S1-06a..e (D-T3D-01) |
| SM-15 | Clic dentro de la paleta (fondo o casilla de instrumentación) y escribir `LINE` | nada aparece en la línea de órdenes mientras el foco está en la paleta; después, clic en el área de dibujo y escribir `LINE`: la orden se inicia (`Esc` para cancelarla) | S1-06g (el foco vuelve solo por una acción explícita; F1 no tiene acciones que lo devuelvan) |
| SM-16 | Seleccionar dos racks; cerrar la paleta (botón de cierre); con la paleta cerrada, cambiar la selección (`Esc` y clic en otro rack); esperar el reposo; `RACKPANEL` | con la paleta cerrada, ninguna fila `drain` ni `selection.read`; al mostrarla: filas `panel.show`, `drain` y `selection.read`, una sola vez, y el contexto refleja la selección actual | INV-33 (P-01b), ocultar y mostrar |
| SM-17 | Acoplar la paleta a la izquierda y después a la derecha; dejarla flotante. Probar el botón de ocultación automática y anotar si su plegado se comporta como ocultar | se acopla y se libera; sin errores; anotar el comportamiento del plegado (`HOST-TO-CONFIRM`) | D-01 (acoplable y flotante) |
| SM-18 | `(getvar "DBMOD")` | bit 1 igual que en SM-02 (abrir, usar y cerrar el panel no modifica el dibujo) | S1-08, S1-06h, OV-20 |
| SM-19 | Solo si en SM-00b `PICKFIRST` = 0: observar el aviso del panel | «PICKFIRST está desactivado: la selección previa a un comando no se puede leer.»; no se cambia `PICKFIRST`. Si `PICKFIRST` ≠ 0: NOT_APPLICABLE | S1-12 |

### 6.3 Fase C: varios documentos y cambio de documento activo

| Paso | Acción exacta | Esperado | S1 / OV / INV / escenario |
|---|---|---|---|
| SM-20 | Abrir B; esperar el reposo | «Sesión <m>» con m ≠ n; «Selección» = «Ninguno»; filas de reconciliación (`drain`, `selection.read`) con sus ms | S1-05, escenario 9, INV-07 |
| SM-21 | En B, clic en un rack | «Un rack» con el RackId de B | S1-05 |
| SM-22 | Volver a A con su pestaña de documento; esperar el reposo | «Sesión <n>» (la de A); el contexto es el de la selección actual de A, sin restos de B | S1-05, INV-07, INV-22 |
| SM-23 | Volver a B | «Sesión <m>» y el contexto de B | S1-05 |
| SM-24 | Abrir A otra vez (AutoCAD ofrece abrirlo en solo lectura: aceptar solo lectura) | nueva pestaña con «Sesión <p>», p ≠ n; seleccionar en una copia no cambia el contexto de la otra | S1-05, OV-15, INV-07 |
| SM-25 | Cerrar la copia de solo lectura sin guardar | el panel sigue a la pestaña activa; sin estado de error | INV-22 |
| SM-26 | Cerrar todos los dibujos sin guardar menos C; después cerrar también C sin guardar, de modo que no quede ningún documento abierto | con 0 documentos: «Sin documento» / «Abre o crea un dibujo para trabajar con el panel.», acciones desmarcadas, «No hay un documento abierto.» | estado vacío, INV-22 |
| SM-27 | Volver a abrir C y B (sin guardar nada antes) | el panel vuelve a «Sesión activa» con sesiones nuevas | S1-05 |

### 6.4 Fase D: convivencia con comandos clásicos (dibujo C, panel visible)

| Paso | Acción exacta | Esperado | S1 / OV / INV / escenario |
|---|---|---|---|
| SM-28 | Con la paleta visible, dibujar en C: `LINE` (tres segmentos), `CIRCLE` | se dibuja con normalidad; la paleta queda abierta | S1-01, OV-01 |
| SM-29 | Rueda del ratón (zoom), arrastre con el botón central (encuadre), `ZOOM` `E`, `PAN` | funcionan; la paleta no parpadea ni roba el foco | S1-02, OV-02 |
| SM-30 | `MOVE`, `COPY`, `ERASE` sobre lo dibujado y después `UNDO` 3 | se comportan como siempre; `UNDO` deshace órdenes de AutoCAD | S1-03, OV-03 |
| SM-31 | Cerrar la paleta; ejecutar la medición del comando masivo | anotar `ms` («oculto») | S1-10, escenario 10 (estado 2) |
| SM-32 | `RACKPANEL`; ejecutar la medición del comando masivo | anotar `ms` («visible») | S1-10, escenario 10 (estado 3) |
| SM-33 | Ejecutar una orden clásica de RackCad sin escribir en el dibujo: `RACKAYUDA` y cerrar su ventana | funciona con el panel abierto | convivencia de comandos clásicos, INV-25 |

### 6.5 Fase E: RACKEDITAR (dibujo B)

| Paso | Acción exacta | Esperado | S1 / OV / INV / escenario |
|---|---|---|---|
| SM-34 | En B: `RACKEDITAR`, elegir el rack selectivo; con el editor abierto, observar el panel | el panel no interfiere con el editor; mientras dura la orden: «Hay un comando en curso.» | S1-07, OV-17, INV-23 (D-T3D-06) |
| SM-35 | Cerrar el editor con **Cancelar** (nunca aceptar: aceptar redibuja y escribe); esperar el reposo | el panel vuelve a «Sesión activa» con el contexto de B; `(getvar "DBMOD")` sin el bit 1 cambiado por el panel | S1-07 |

### 6.6 Foco con un campo del panel (D-T3D-01)

El panel de F1 no tiene **ningún campo editable**: solo textos de estado, dos casillas de disponibilidad no interactivas, la casilla
«Medir en memoria» y el botón «Reintentar» (visible solo en error). S1-06a..e del Freeze exigen un campo y **no pueden ejecutarse
literalmente** con el producto de F1. Hasta que el Coordinator decida D-T3D-01, se registran como **NOT_EXECUTABLE_IN_F1** y no
pueden contar como PASS.

Comprobaciones adaptadas, que son evidencia complementaria y no sustituyen a S1-06a..e. Con el foco de teclado en la casilla
«Medir en memoria» (llevarlo con `Tab` desde la paleta):

| Paso | Acción | Esperado |
|---|---|---|
| SM-14a | escribir `abc def` | ningún carácter llega a la línea de órdenes. La barra espaciadora puede alternar la casilla, que es el control con foco: anotarlo |
| SM-14b | `Ctrl+Z` | AutoCAD no ejecuta `UNDO` (historial de órdenes sin `UNDO`; el dibujo no cambia) |
| SM-14c | `Ctrl+C` y `Ctrl+V` | AutoCAD no ejecuta `COPYCLIP` ni `PASTECLIP` |
| SM-14d | con objetos seleccionados en el dibujo antes de llevar el foco a la paleta, `Supr` | no se borra ningún objeto |
| SM-14e | `Esc` y `Enter` | no se cancela ni se repite ninguna orden de AutoCAD |
| SM-14h | después de SM-14a..e, `(getvar "DBMOD")` | bit 1 sin cambios (S1-06h) |

### 6.7 Contención (fallos)

F1 no tiene inyecciones de fallo para el panel (D-T3D-02), así que la contención se comprueba **observando** durante toda la
secuencia:

| Paso | Comprobación | Esperado | INV |
|---|---|---|---|
| SM-36 | ¿Apareció alguna vez «Error del panel» o «Sesión degradada»? | no. Si apareció: anotar el mensaje literal («No se pudo actualizar el panel: …»), pulsar «Reintentar», comprobar que una orden clásica (`LINE`, `RACKAYUDA`) sigue funcionando y continuar. Es un hallazgo para el Coordinator, no un STOP, salvo que las órdenes clásicas dejen de funcionar | INV-25 |
| SM-37 | ¿Hubo algún cuadro de excepción no controlada de AutoCAD o de .NET, o un cierre abrupto de AutoCAD? | no | INV-25, INV-14 |
| SM-38 | Comprobación Debug de INV-14 (`Debug.Assert` de `TopTransaction == null` tras cada drenaje y lectura de la selección) | no se dispara en toda la sesión. En una compilación Debug, un fallo de `Debug.Assert` termina el proceso de forma abrupta (`HOST-TO-CONFIRM`): un cierre abrupto en SM-06..SM-35 es un FAIL candidato de INV-14 | INV-14 |
| SM-39 | Tras cerrar AutoCAD (sección 7): registro de RackLog del día (`%APPDATA%\RackCad\logs\`) | se anotan las entradas nuevas desde la hora de SM-00a; ninguna con `Workspace` ni «A workspace drain left a transaction open.». Cada excepción se transcribe redactada y se explica | INV-25, INV-14 |

## 7. Cierre de AutoCAD

| Paso | Acción exacta | Esperado |
|---|---|---|
| SM-40 | Cerrar todos los dibujos **sin guardar** (responder «No» a guardar) y salir de AutoCAD | ninguna `acad.exe` (`Get-Process acad` vacío) |
| SM-41 | SHA-256 de A, B, C y de la biblioteca de bloques | iguales a los de PR-07 y PR-11. Si alguno cambió (guardado accidental): FAIL de esa parte y el Owner restaura la copia |

## 8. POST y DIFF

1. **POST:** orden exacta de T3C §4, en `<run>/profile/`, con el mismo `-CandidateDll` y los mismos valores del acuse. Salida de consola
   en `<run>/console/post.txt`. Debe tomarse en cuanto AutoCAD se cierra y antes de cualquier otra sesión de AutoCAD o de I-52.
2. **DIFF:** orden exacta de T3C §6, con `-PaletteKeyAcceptance` = el valor del acuse de I-52 (`ACCEPTED`, `NOT_ACCEPTED` o, si el acuse
   no lo dice, `NOT_RECORDED`). Salida en `<run>/profile/profile-diff.json` y consola en `<run>/console/diff.txt`.
3. **Lo que la BASELINE y el POST deben probar** (dictamen O-4), con 0 entradas en R-F-01..R-F-08:
   - el DLL exacto (`F|CandidateDll.Sha256` igual en ambas);
   - sin adición a `TRUSTEDPATHS` (`F|TRUSTEDPATHS.*` sin cambios y `CoversCandidateDllFolder` = `false` en ambas);
   - sin cambio de `SECURELOAD`;
   - sin perfil nuevo;
   - sin carga automática por el registro ni bundle en `ApplicationPlugins`;
   - sin variables de entorno persistentes.

## 9. Disposición

**Disposición del perfil:** la de T3C §8, con los dictámenes de su sección 15.

| Disposición | Significado |
|---|---|
| `CLEAN` | sin hallazgos |
| `REPORTED_ACCEPTED` | solo con la aceptación expresa de I-52 |
| `REPORT_REQUIRED_STOP` | STOP |
| `FORBIDDEN_DRIFT` | STOP y restauración de T3C §10 |
| `INVALID` | STOP |

**Propuesta de resultado de Smoke-1** (lo deciden el Owner y el Coordinator; la sesión solo propone). PASS si:
- S1-01, S1-02, S1-03, S1-04 (incluido el caso Diagnóstico según D-T3D-03), S1-05, S1-06f, S1-06g, S1-06h, S1-07, S1-08, S1-09,
  S1-10 y S1-11 son PASS;
- S1-06a..e quedan resueltos según D-T3D-01;
- S1-12 es PASS o NOT_APPLICABLE;
- la disposición del perfil es `CLEAN` o `REPORTED_ACCEPTED`;
- SM-36..SM-39 no tienen hallazgos sin explicar;
- SM-41 da los dibujos sin cambios.

Cualquier FAIL deja Smoke-1 en NOT PASS; un STOP lo deja sin resultado. Con Smoke-1 PASS y Smoke-NAV PASS se entra en F4 (Freeze §11).
La parte de host de INV-21 la da la disposición del perfil; la de INV-24, S1-06a..h; las mediciones de los escenarios 1, 2, 3, 9 y 10
son registro (OV-22), sin umbrales.

## 10. Evidencia y custodia (dictamen O-2 de T3C)

### 10.1 Directorio de ejecución (transitorio, ignorado por Git)

```text
artifacts/orchestration/I-64/F1-SMOKE-1/<RunId>/
  authorization/   autorización de Smoke-1 del Coordinator; RELEASE y acuse de I-52; autorización del CAD manager (literales)
  candidate/       candidate.md (sección 3), fixtures.md (SHA-256 de A, B, C antes y después)
  profile/         capturas BASELINE y POST (canónico, .sha256 y compañero local), profile-diff.json y su .sha256
  console/         baseline.txt, post.txt, diff.txt y la transcripción de la línea de órdenes de AutoCAD (copiada de la ventana de texto, F2)
  operator/        operator-record.md (plantilla de 10.2, llenada y firmada) y capturas de la paleta
  logs/            extracto redactado de RackLog y SHA-256 del archivo completo
  disposition/     disposición del Owner y del Coordinator (literales)
```

`<RunId>` lo asigna la sesión con la forma `R<yyyyMMddTHHmmssZ>-<4 hex>` cuando el Coordinator autoriza.

### 10.2 Registro del operador

Plantilla: `docs/automation/evidence/I-64-pilot/F1-T3D-SMOKE-PROCEDURE/operator-record-template.md`. Una fila por paso SM con:
resultado, observado, valores (sesiones, RackId, ms, `DBMOD`, número de entidades) y notas; firma y fecha del Owner al final.

### 10.3 Custodia final

En `docs/automation/evidence/I-64-pilot/F1-SMOKE-1/<RunId>/`, según el patrón de T3A y T3B:
- `custody-manifest.json`, con el SHA-256 de cada original;
- capturas canónicas BASELINE y POST con sus `.sha256`;
- diff de perfil;
- registros de consola y del operador;
- RELEASE y acuse de I-52;
- disposiciones del Owner y del Coordinator;
- redacciones.

**Los compañeros locales en claro nunca se versionan; solo su SHA-256.**

**Redacción:**
- rutas locales, usuario y máquina por marcadores, como en T3A y T3B;
- capturas de pantalla sin barra de título ni rutas;
- el registro de RackLog, solo como extracto redactado más el SHA-256 del original.

## 11. STOP y vuelta atrás

**STOP:**
- todas las condiciones de T3C §9 (STOP-01..STOP-09);
- además:

| Id | Condición |
|---|---|
| SM-STOP-01 | falta cualquier prerrequisito de la sección 2, o la autorización no resuelve D-T3D-01..06 |
| SM-STOP-02 | un selector de perfil, otra `acad.exe` o una sesión de I-52 durante la ventana |
| SM-STOP-03 | cargar el DLL exige un cambio persistente de seguridad o de perfil (STOP-06) |
| SM-STOP-04 | un paso exigiría cambiar una variable del sistema, el perfil o el registro (por ejemplo `PICKFIRST`) |
| SM-STOP-05 | un cierre abrupto de AutoCAD o un cuadro de excepción no controlada |
| SM-STOP-06 | el operador se desvía de las acciones escritas (expresiones no previstas, otro DLL, guardar un dibujo de prueba) |
| SM-STOP-07 | las órdenes clásicas dejan de funcionar con el panel en «Error del panel» |

**Ante un STOP:**
- no se continúa la secuencia; no se repara nada;
- se cierra AutoCAD sin guardar, si sigue abierto, y se toma el POST (sección 8) para dejar constancia del estado del host;
- se conserva toda la evidencia;
- se avisa al Owner, a I-52 y al Coordinator.

**Vuelta atrás:**
- **Perfil y seguridad:** la restauración humana de T3C §10, con la BASELINE como referencia y una captura POST de comprobación en una
  carpeta nueva.
- **Dibujos de prueba:** el Owner restaura sus copias.
- **Tras un cierre abrupto de AutoCAD:** se informa a I-52, que decide sobre su campaña.
- **Repetición:** una repetición de Smoke-1 exige una decisión del Coordinator y un `RunId` nuevo; ninguna corrida se repite para
  buscar otro resultado.

## 12. Decisiones necesarias antes de ejecutar (para el Coordinator)

- **D-T3D-01, foco en un campo.** S1-06a..e (OV-19, INV-24) exigen un campo del panel, y el panel de F1 no tiene ninguno. Opciones:
  - (a) S1-06a..e quedan **NOT_APPLICABLE en Smoke-1** y pasan al primer hito cuyo panel tenga campos (F4 / Smoke-2). Reasignar un
    escenario OV exige una A-n (LIFECYCLE §8; Freeze §12, «reasignarlo exige una A-n»).
  - (b) Se aceptan las comprobaciones adaptadas de la sección 6.6 como evidencia de S1-06a..e en F1.
  - (c) Otra vía que el Coordinator fije.

  La sesión no elige: hasta la decisión, NOT_EXECUTABLE_IN_F1.
- **D-T3D-02, contención sin inyección.** El producto de F1 no tiene inyecciones de fallo para el panel. La contención (INV-25) se
  comprueba por observación (sección 6.7); provocar un fallo exigiría un cambio de producto, fuera de esta tarea. ¿Se acepta?
- **D-T3D-03, caso Diagnóstico de S1-04.** Hace falta un dibujo con un bloque de definición ilegible o de versión futura. ¿Lo aporta el
  Owner? Si no existe, ¿SM-11 queda NOT_EXECUTED con la cobertura Core de INV-17 como evidencia?
- **D-T3D-04, cómo se registra INV-14 en el host.** Un fallo de `Debug.Assert` en la compilación Debug termina el proceso
  (`HOST-TO-CONFIRM`). ¿Basta registrar «sin cierre abrupto» (SM-38) más el registro de RackLog (SM-39)?
- **D-T3D-05, «registro» de S1-09.** El instrumento de F1 es en memoria y se muestra en el panel (decisión de T3: no escribe en
  RackLog). S1-09 se comprueba sobre esa tabla y sobre la línea de órdenes (sin regeneraciones), no sobre el archivo de RackLog.
  ¿Se confirma esa lectura de D-18 para F1?
- **D-T3D-06, modales de RackCad y RACKEDITAR.** `SetModalRackCadActive` no tiene ningún llamador en F1 (hallazgo H-B5 y C-T3B-09 del
  paquete de T3B: conectarlo reabriría comandos clásicos). En S1-07, RACKEDITAR mantiene una orden activa, así que el panel muestra
  «Hay un comando en curso.» y no drena. El texto «Hay un cuadro de diálogo de RackCad abierto.» no aparece nunca en F1. ¿Se acepta
  S1-07 con ese comportamiento?
