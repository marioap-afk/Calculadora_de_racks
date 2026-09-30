# I-60 — Evidencia de la unidad (nombre lógico automático)

Unit / Initiative / Workflow: `I-60` / I-60 / V2. Claim-Id `fe03d635-bcd6-4445-ad4a-384ad1a0cda3` (reclamo `6a9c7d09`, base `origin/main` `1304101d`; clasificacion T4).

## 1. Caracterizacion (Discovery)

Barrido de solo lectura con un revisor por familia y uno transversal, sobre `main` `1304101d`.

| Familia | Rutas de creacion | Nombre hoy si no se escribe nada | Punto de asignacion (Plugin) |
|---|---|---|---|
| Selectivo | RACKSELECTIVO/RS; menu RACKCAD «Diseñar sistema selectivo»; biblioteca «como nuevo» | vacio `""` (bloque «Selectivo»[_k]); biblioteca: nombre de la plantilla | `RackSelectivoCommands.DrawSelectiveView` (solo creacion) |
| Dinamico | RACKSISTEMADINAMICO/RSD (demo, `null`); menu RACKCAD; biblioteca | `null` (RSD) o `""` (menu); biblioteca: nombre del archivo | `RackDinamicoCommands.DrawDynamicView`, `source == null` |
| Push Back | RACKPUSHBACK/RPB; menu RACKCAD; biblioteca | `""`; biblioteca: nombre del archivo | `RackPushBackCommands.DrawPushBackView`, `source == null` |
| Cantilever | RACKCANTILEVER/RCT; menu RACKCAD; biblioteca (la insercion de componentes no es un rack) | `""` (diseño interno `null`) | `RackCantileverCommands.DrawCantileverView`, `source == null` |
| Cabecera | RACKCABECERA/RCB; QUICKCABECERA/QCB; menu RACKCAD; biblioteca | **no vacio**: el nombre de la plantilla («Estandar (3 paneles)»), igual para toda cabecera | `RackCabeceraCommands.DrawAndPlace` |
| Cama | QUICKCAMA/QCM; menu RACKCAD; biblioteca (RACKLAYOUT no alcanzable: la cama no tiene planta) | `null` (QUICKCAMA) o `""` (menu) | QUICKCAMA y el caso `FlowBedInsertionRequest` del menu |

Hechos transversales:

- **Identidad:** el GUID nuevo se acuña al insertar (`RackEditorIdentity.EnsureId` desde `RackEditorSession.Complete`); `RACKEDITAR` puede acuñar uno para un sobre
  heredado sin Id, por eso la puerta es «ruta de creacion» y no «se acuño un GUID» ni «el nombre esta vacio».
- **Una vista por creacion:** en `main` toda insercion crea exactamente una vista; las hermanas llegan por `RACKEDITAR` → Insertar con el mismo Id y nombre
  (`RackSelectivoCommands.cs:120-124/279`, `RackDinamicoCommands.cs:176`, `RackPushBackCommands.cs:181`, `RackCantileverCommands.cs:232`, `RackCabeceraCommands.cs:234-235`).
  I-53 (REUSE/BATCH) no inserta vistas ni racks.
- **Dos almacenes del nombre:** el sobre y el diseño interno; el editor Selectivo recarga el nombre del diseño interno. La asignacion debe ocurrir antes de
  serializar ambos.
- **Nombre del bloque acoplado:** `RackViewBaseName` usa el nombre logico cuando no esta vacio; `RackBlockRenamer.SyncName` renombra en `RACKEDITAR`.
- **Terminologia:** `BomLabel` = Selectivo, Dinámico, Push Back, Cantilever, Cabecera, Cama (`src/RackCad.Plugin/KindHandlers/*KindHandler.cs`); registro y
  `RACKLISTA` usan etiquetas largas («Sistema dinámico», «Cama de rodamiento») o crudas (`pushback`).
- **Escaneo:** `RackBlockFinder.ScanEnvelopes(transaction, database, includeReferenceCount)` (lectura) ya lo usan RACKLISTA y RACKBOMTOTAL; incluye definiciones
  no colocadas.
- **Guardar/reabrir:** el sobre vive en la definicion (Xrecord); nada se ejecuta al abrir o guardar; los nombres hacen ida y vuelta byte a byte.
- **RACKDUPLICAR / RACKLAYOUT independientes:** nombres ya no vacios («<base> - copia[ N]», «Rack - copia» sin nombre, «<base> <celda>»), fijados por I-51 (PD-5) y
  sus pruebas (`RackDuplicationPlanTests` T1/T2/T3/T11/T13; guarda G_R4: RACKDUPLICAR no barre el dibujo).

## 2. Identidades

| Hito | SHA |
|---|---|
| Base (`origin/main`, I-52-AUTH15-C1 integrada) | `1304101d997b66748e21f4896ac203d6fdc6a1f1` |
| Reclamo | `6a9c7d09` (`Claim-Id: fe03d635-bcd6-4445-ad4a-384ad1a0cda3`) |
| Freeze acordado (R2) | `97fee74639232eaa2d9879775a58b9b068746cd3` (blob `678f73bb2a22bde770405a0e7287f433e3a61b9f`) |
| Freeze (`Frozen: YES`) | `30c7ba0bd102c3005893fb3e6dd8185a3cc2a570` |
| Andamiaje inerte | `8366326e` |
| RED (44 de 57 fallan) | `51711c8478a55362f3c5afb6a01fbb32ef50bcf9` |
| GREEN | `f5104016` |
| Candidato | el commit que contiene esta tabla; bloque §7.1 de [validacion-manual-autocad](../../guias/validacion-manual-autocad.md) en la entrega al Owner y en el cierre |

## 3. Matriz de validacion del Owner (OV-I60)

Preparacion: dibujo nuevo en pulgadas; el DLL Debug del worktree de I-60 construido desde el Candidato (no el de I-55; verificar `ProductVersion` y
SHA-256 antes de NETLOAD, guia §7.2). Un resultado por fila: PASS, FAIL o NOT EXECUTED (motivo). Ninguna fila esta ejecutada.

| # | Pasos | Esperado |
|---|---|---|
| N-01 | Dibujo vacio → RACKSELECTIVO sin escribir nombre → Insertar frontal → RACKLISTA | Un rack «Selectivo 1»; el bloque se llama «Selectivo 1…»; RACKEDITAR muestra «Selectivo 1» en el campo nombre (editable) |
| N-02 | Repetir N-01 dos veces | «Selectivo 2», «Selectivo 3» |
| N-03 | Renombrar con RACKEDITAR «Selectivo 2» → «Pasillo A»; crear otro Selectivo | «Selectivo 4» (no se rellena el 2); el RackId del renombrado no cambia (RACKLISTA, zoom) |
| N-04 | Renombrar un rack a «selectivo 7» (minusculas); crear otro | «Selectivo 8» |
| N-05 | Crear un Selectivo escribiendo «Rack A» | Se llama «Rack A»; el siguiente automatico no salta |
| N-06 | Crear Dinamico (menu RACKCAD), Push Back, Cantilever, cama (QUICKCAMA y menu) y cabecera (RACKCABECERA sin tocar el nombre, y QUICKCABECERA) | «Dinámico 1», «Push Back 1», «Cantilever 1», «Cama 1» y «Cama 2», «Cabecera 1» y «Cabecera 2»; secuencias independientes; en la cama, el nombre del bloque tambien lleva «Cama N» |
| N-07 | RACKCABECERA escribiendo un nombre propio | Se respeta |
| N-08a | Biblioteca «como nuevo»: un diseño de Dinamico, Push Back, Cantilever o cama con nombre de archivo propio | Conserva el nombre de la entrada (editable) |
| N-08b | Biblioteca «como nuevo»: una cabecera guardada con el nombre «Estandar (3 paneles)» | Recibe «Cabecera N» |
| N-08c | Biblioteca «como nuevo»: un Selectivo guardado sin nombre | Recibe «Selectivo N» |
| N-09 | RACKEDITAR sobre «Selectivo 1» → Insertar lateral y planta | Las tres vistas se llaman «Selectivo 1» (RACKLISTA: 1 rack, 3 vistas) |
| N-10 | Dibujo con un rack heredado **sin nombre** → abrir, RACKLISTA, RACKBOMTOTAL, RACKEDITAR → Actualizar / Insertar, guardar y reabrir | Sigue «(sin nombre)» en todo momento |
| N-11 | Guardar, cerrar y reabrir el dibujo de N-01..N-06 | Todos los nombres se conservan; crear uno nuevo continua la numeracion |
| N-12 | RACKDUPLICAR de «Selectivo 1» y de un rack sin nombre (fuera de alcance: sin cambios) | Igual que hoy: «Selectivo 1 - copia», «Rack - copia» |
| N-13 | `Esc` en el jig de un Selectivo nuevo; despues crear otro | No queda nada; el siguiente sigue la numeracion del dibujo (no se consumio numero) |
| N-14 | RACKLAYOUT «independientes» desde «Selectivo 1» (fuera de alcance: sin cambios) | Igual que hoy: «Selectivo 1 <celda>» por celda |
| N-15 | Renombrar un Dinamico a «Selectivo 9»; crear un Selectivo | «Selectivo 10» (el patron se lee sobre el texto de todos los racks) |
| N-16 | RACKCABECERA sin tocar el nombre → RACKEDITAR (muestra «Cabecera N») → Actualizar; y un Cantilever nuevo → RACKEDITAR → Actualizar | Tras Actualizar siguen «Cabecera N» y «Cantilever N» en RACKLISTA y en el nombre del bloque |
