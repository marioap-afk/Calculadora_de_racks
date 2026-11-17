> **Redaccion para el repo publico:** las rutas locales se sustituyen por marcadores (`<carpeta-de-copias-de-PB-T0>`, `<carpeta-del-DLL-de-C2>`, `<registro-de-copias-de-Smoke-2>`, `<worktree-de-I-64>`). Los inventarios crudos (JSON de los sobres de racks de dibujos reales) no se custodian: van sus resumenes y su SHA-256.

# I-64 F4B PB-T0 — Bloque M: análisis de la medición física en AutoCAD

**Estado:** cerrado el 2026-11-17 (18:16Z). Lo opera la sesión por control de pantalla, por decisión del Owner ([DP], pregunta 3: «La sesión»). El procedimiento es el anexo `pbt0-block-m-procedure.md` v2 (con N-I1). El registro con hora está en `run-log-m.md` y los inventarios en `<carpeta-de-copias-de-PB-T0>/inv/`. El bloque M no consume intentos del bloque C ([P] §6.5).

**Base medida:** el DLL Debug de C2 (`<carpeta-del-DLL-de-C2>`), con los 4 SHA iguales a `dll-c2.md`. Sus `catalogs` son iguales a `assets/catalogs`, y `git diff 9abc1320 67e28452 -- src assets` sale vacío: es el comportamiento clásico del producto de `5ccaa770`. No hay ninguna variable `RACKCAD_DEBUG_*` (P-1 a P-4: PASS).

**Dibujos:**
- **Copias de trabajo:** 9 del original D1 (M1, M2, M3, M4, M6, M1u, M1v, M2u y M3u) y 1 del original D2 (M7-D2), hechas con `copy` y comprobadas por SHA-256.
- **Los originales nunca se abrieron.**
- **Al cerrar:** los dos originales conservan su SHA de P-5 y las 10 copias siguen iguales a su original, porque nada se guardó. No quedan autoguardados de las copias, ni `RACKCAD_DEBUG_*`, ni procesos de AutoCAD (`close_check.py`: **PASS**).

**Pausa:** de 16:45Z a 18:01Z por orden del Owner. La orden de M-2b quedó en el jig y se retomó en el mismo estado. Nada se envió a AutoCAD durante la pausa.

## 1. Tabla de hechos

| Id | Copia / rack | Resultado | Estado |
|---|---|---|---|
| M-1 | M1 / PB E | «+ frente» → «Actualizar»:<br>• mensaje «…vistas redibujadas (lateral x1, frontal entrada/salida x0, frontal posterior x0, planta x1; cortes obsoletos eliminados x0)»;<br>• cambian las dos definiciones del rack (planta 2241 → 2274 entidades; sobres 72 → 73 frentes; lateral 1 con las mismas 93 entidades);<br>• **ninguna definición ni referencia nueva** | **MEDIDO**: H-PB-16 confirmado |
| M-2a | M2 / PB E | Con el corte del último poste insertado (M-6a), «- frente» → «Actualizar» → «…cortes obsoletos eliminados x1». Se **borran la definición «PB E - lateral 73» y su referencia**; la planta (2241 → 2208) y el lateral 1 se redibujan con 71 frentes | **MEDIDO**: MA H-060 y H-063 |
| M-2b | M4 / PB G (desviación: el anexo decía M6 / PB F) | Con el corte «PB G - lateral 73» como **única** definición del GUID, «- frente» → «Actualizar» → «todas las vistas Push Back quedaron obsoletas; no se elimino el ultimo vinculo del rack.» y después «no se pudo actualizar el sistema Push Back.». El inventario es **idéntico byte a byte** antes y después: no se borra nada y **la edición no se escribe en ningún sitio** (el sobre sigue con 72 frentes) | **MEDIDO**: MA H-062. Matiz a MA H-089 (§3) |
| M-3 | M2 / PB F (desviación: el anexo decía M3) | «PB F2» → «Actualizar»: las definiciones pasan a «PB F2 - planta» y «PB F2 - lateral 1», y el `Name` de los dos sobres a «PB F2» (mismo GUID y mismas referencias). «PB E», que usa otro rack → «Actualizar»: el `Name` de los sobres = «PB E», pero las **definiciones siguen como «PB F2 - …»**, sin sufijo. Quedan dos racks con `Name` «PB E» | **MEDIDO**: MA H-080, H-081 y H-084 |
| M-4 | M4 / PB F, **CONSTRUCTED REPRESENTATIVE TEST CASES** | Lado B con el frente 1 activo → «Actualizar»: el rack queda compuesto (`SideB.IsPresent`, 15 módulos, `DefaultTopology` «Encontradas»), **sin ninguna vista nueva**. Al reabrir, la casilla del lado B sale marcada. Insertar el frontal entrada/salida de B crea «PB F - frontal entrada-salida B» (vista frontal, sección 2, 13 entidades) | **MEDIDO**: H-PB-16 confirmado también en A/B |
| M-4 / H-PB-18 | M4 / PB F | Desmarcar el lado B → «Actualizar» → «…frontal entrada/salida x1…; cortes obsoletos eliminados x0». El frontal B **no se borra**:<br>• se **redibuja como frontal de un sentido** (13 → 861 entidades);<br>• se **renombra** a «PB F - frontal entrada-salida», con la misma referencia 331BDD;<br>• su sobre **conserva `View` frontal y `Section` 2**, la sección de entrada del lado B, en un rack que ya no es compuesto.<br><br>Ningún corte lateral se vuelve obsoleto. El rack solo tenía el lateral 1 (poste 0), así que **la dependencia de los cortes respecto de B (H-PB-17 corregido) no se ejercita** | **MEDIDO**: H-PB-18. H-PB-17 en lo que toca a B: **NO EJERCIDO** |
| M-5 | M1u, M1v, M2u, M3u y M4 | Ver §2 | **MEDIDO** |
| M-6a | M2 / PB E | Insertar el corte del último poste (el selector muestra «69», con el rótulo «Lateral (corte 73)»). Se crean:<br>• la definición «PB E - lateral 73» (sección 72, mismo GUID, `Name` «PB E»);<br>• la referencia en (9818.12, -1805.06), con rotación 0, escala 1 y capa A-TEXTO (la `CLAYER`). | **MEDIDO**: los nueve puntos, en el §4 |
| M-6b | M2 / PB F | Sin «Actualizar», nombre «PB F3» → Insertar la planta:<br>• las vistas existentes se redibujan y renombran a «PB F3 - …», con `Name` «PB F3»;<br>• la vista **nueva** es «PB F3 - planta_1», con **el nombre nuevo y el sufijo `_1`**, pero su sobre es **idéntico campo a campo (0 diferencias, `envdiff.py`) al de la planta ANTES de la edición** (`Name` «PB E» y el diseño anterior) | **MEDIDO**: MA H-077 confirmado |
| M-7 | M1 (D1) y M7-D2 | Sobres Push Back con `Design.PushBack.Structure.DimensionStyle` no nulo: **0 de 22**. D1 tiene 14 (todos `null`, `Dimensions` = 0) y D2 tiene 8 (todos `null`, `Dimensions` = 0) | **NO DISPONIBLE**: decide la prueba Core de OB-06.1. El recuento (0) es el dato para el Owner antes de PB-TC |

## 2. M-5, respondido por las copias de Deshacer (protocolo N-I1: nada entre la orden y su `U`)

| Medición | Orden medida | `U` | Eco | Comparación | Resultado |
|---|---|---|---|---|---|
| M-5a | M1u: «+ frente» → «Actualizar» | 1 | «RACKEDITAR» | `M1u-u1` = `M1-pre`, en **todo** el dibujo (0 diferencias) | Un `U` deshace el «Actualizar» entero |
| Control (b) | M1v: «Actualizar» → `REGEN` → `U` | 1 | «REGEN» | `M1v-u1` = estado posterior (73 frentes) | Una orden nativa posterior **no** absorbe el «Actualizar» |
| M-5b | M2u: «- frente» → «Actualizar» (con borrado del corte) | 1 | «RACKEDITAR» | `M2u-u1` = `M2-ins` en todo el dibujo, con los mismos handles | Un `U` deshace también el borrado de una vista |
| M-3 (Deshacer) | M3u: dos renombrados seguidos | 1 | «RACKEDITAR» | `M3u-u1` = `M2-r1` del rack (mismos sobres) | Un `U` deshace solo el **último** «Actualizar» |
| M-5c | M4: vuelta de A/B a un sentido | 1 | «RCDUMP» | `M4-u1` = `M4-ab2` en todo el dibujo | Un `U` deshace la vuelta a un sentido |

**Conclusiones:**
- **Para el usuario, cada «Actualizar» clásico es UN paso de Deshacer**, también cuando borra una vista, renombra o convierte A/B. Que el clásico no sea atómico por transacciones (H-PB-19, MA H-055) no se ve en Deshacer: AutoCAD agrupa la orden entera.
- **El eco del `U` no es un nombre fiable del grupo.** En M-5c dijo «RCDUMP», la orden LISP anterior. Manda el inventario.
- **Lo medido con `RCDUMP` intercalado** (M-5a en M1 y M-5b en M2; §2.1 del anexo) se conserva como evidencia: un solo `U` con eco «RCDUMP GROUP (Lisp Expression)» volvió al estado previo. El control (b) muestra que una orden **nativa** no absorbe. Lo de M1 es un efecto de la orden LISP de inventario, no de RACKEDITAR. **Para PB-T3:** no hay absorción por órdenes nativas; el Deshacer del clásico se iguala con «un `U` por Actualizar».
- **NO OBSERVABLE:** el nombre interno del grupo de Deshacer y cuántas transacciones contiene.

## 3. Hechos nuevos y correcciones a MA

1. **RACKEDITAR cuenta las vistas por DEFINICIÓN, no por referencia** (lectura en `67e28452`, confirmada por M-2b). `RackCommandSupport.FindRackBlocks` recorre `RackBlockFinder.ScanEnvelopes(..., includeReferenceCount: false)` y casa por GUID (`RackPushBackCommands.cs:198`; `RackCommandSupport.cs:117-140`). Consecuencias:
   - una definición **sin referencias** cuenta como vista superviviente en la regla del último vínculo y se redibuja;
   - en M-2b, mientras quedó la definición de la planta sin referencias, la regla no se podía probar.

   **Para PB-T3:** la enumeración de «vistas hermanas» del panel tiene que dar el mismo conjunto: definiciones por GUID, con o sin referencia.
2. **Referencias anidadas en dibujos reales:**
   - en D1, la definición «PB G - planta» tenía una referencia **anidada** dentro de un bloque huérfano de tipo «pegar como bloque» («A$C…», sin referencias), así que `PURGE` no la quitaba;
   - en D2, ninguna de las 8 definiciones Push Back tiene referencias de nivel superior.

   El inventario RCDUMP solo lista las referencias de nivel superior. MA H-063 ya dice que `EraseViewBlocks` borra **todas** las referencias directas de la definición, sea cual sea su propietario. Eso incluye las anidadas en otras definiciones; es lectura de código, no medido en M.

   **Para PB-T3:** el borrado de una vista obsoleta afecta también a sus referencias anidadas.
3. **La regla del último vínculo descarta la edición (matiz a MA H-089).** Con todas las vistas obsoletas no hay ninguna superviviente que redibujar, así que no se escribe nada (`changedInPlace == false`, `RackPushBackCommands.cs:363`, `:376-383`). El usuario ve el aviso y «no se pudo actualizar el sistema Push Back». MA H-089 («impide un borrado, no el resto de la edición») sigue siendo cierto, pero en este caso no hay «resto». Si el mensaje es o no correcto es asunto de la tarea que lo toque. **PB-T0 no cambia producto.**
4. **La ida y vuelta de A/B no es neutra en el sobre** (M-4: de `M4-pre` a `M4-ab3`, `envdiff.py`). La única diferencia es `Design.PushBack.Composite.AbsentSlotsB`:
   - antes, `[0]`, como venía en D1;
   - después, las 72 ranuras.

   `SideB` es `null` en los dos estados. Es inerte en un rack de un sentido, pero **la igualdad de PB-T3 con el clásico tiene que reproducirlo o declararlo**.
5. **El frontal B tras quitar B (H-PB-18):**
   - se redibuja como el frontal entrada/salida completo de un sentido (la captura muestra el frontal completo con el rótulo «PB F»);
   - toma el nombre sin el sufijo de lado (`RackViewBaseName.LinkedPushBackCut` con `IsComposite` falso);
   - **conserva `Section` 2**, el descriptor de la entrada del lado B.

   No se midió qué pasa si el rack ya tiene un frontal entrada/salida del lado A: en ese caso el renombrado chocaría por H-081 y no se haría.
6. **Colocación por Insertar.** El punto tecleado en el jig y la posición final de la referencia difieren siempre en (818.12, -163.06), en M-6a, M-6b, M-4 y M-2b. Probablemente es el SCP del dibujo, pero queda **NO OBSERVABLE** si es el SCP o el punto base del jig, porque no se midió `UCSORG`.
7. **Corrección de redacción del anexo:** el anexo manda custodiar en `I-64-pilot/F4B-PB-T0/block-m/`, pero el `TaskId` del contrato es `F4B-PBT0-CHARACTERIZATION`. La custodia va a `I-64-pilot/F4B-PBT0-CHARACTERIZATION/block-m/`, junto a la carpeta del contrato. Es la misma tarea.

## 4. Los nueve puntos de D-PB-04 para la creación por Insertar (M-6, OB-04.3)

| Punto | Medido |
|---|---|
| Nombre y sufijo | `<Nombre> - lateral <poste+1>`, `<Nombre> - planta`, `<Nombre> - frontal entrada-salida[ B]`. Con colisión: sufijo `_1` («PB F3 - planta_1») |
| Colocación | Un jig interactivo, «Punto de insercion de la cabecera:»; el punto tecleado se desplaza (§3.6) |
| Orientación | Rotación 0 en las cuatro inserciones |
| Capa de la referencia | La capa actual (`CLAYER` = A-TEXTO) en las cuatro. Resuelve MA H-067 |
| Escala | 1 |
| Cotas y textos dentro del bloque | Textos en RACKCAD_ANOTACIONES y entidades en A-TEXTO (la capa actual) dentro de la definición nueva. **Cotas: NO EJERCIDO**, porque todos los racks tienen `Dimensions` = 0 («Cotas: Ninguna») |
| Enlace con las hermanas | El mismo GUID en el sobre (MA H-070) |
| Sobre de la vista nueva tras una edición | El sobre previo a la edición: nombre y diseño anteriores (M-6b; MA H-077) |
| Deshacer | El Deshacer del «Actualizar» con borrado está medido (M-5b). **El Deshacer de la propia inserción no se midió**, porque no estaba en el anexo. F4B no crea vistas (§5) |

## 5. Resolución del §6.4 de la fase

| Fila del §6.4 | Resultado | Decisión |
|---|---|---|
| M-1 y M-4 confirman H-PB-16 | **Sí:** ni «+ frente» (M-1), ni la conversión a A/B (M-4), ni la vuelta a un sentido (M-4) crean vistas | **Aplica.** PB-T3 iguala al clásico **sin crear vistas**, y crear sigue siendo Insertar en el clásico. **No hace falta volver al Owner** (OB-04.3) |
| M-1 o M-4 muestran creación | No | No aplica |
| H-PB-18 (frontal B de un rack que deja de ser compuesto) | Medido: se conserva, se redibuja como frontal de un sentido, se renombra y conserva `Section` 2 (§3.5) | **Por defecto, PB-T3 conserva el rechazo del panel** (H-PB-22). Las dos alternativas, borrar la vista o redibujarla como el clásico, ya tienen la evidencia de M-4 y se deciden en la revisión del paquete de PB-T3. No es una decisión del Owner ahora |
| Cualquier resultado | — | F4B no crea vistas |

## 6. MA §11 actualizado

| Hecho de MA | Ahora |
|---|---|
| H-012 / H-055 (pasos de Deshacer) | **MEDIDO:** un `U` por «Actualizar» (§2). Las transacciones internas: NO OBSERVABLE |
| H-058 (aspecto del frontal B redibujado) | **MEDIDO** (M-4; §3.5) |
| H-067 (capa de la referencia insertada) | **MEDIDO:** la `CLAYER` |
| H-077 (Xrecord de la vista nueva) | **MEDIDO:** el sobre previo a la edición |
| H-085 (nombre visible tras renombrar) | **MEDIDO** en la tabla de bloques (inventario) para el renombrado sin colisión; con colisión, no cambia. El gestor de bloques no se abrió |
| H-025, H-047, H-059, H-098, H-119 | Fuera del alcance del anexo: siguen **[NO MEDIBLE SIN AUTOCAD]**. H-119 solo cubre lo que miden M-1 a M-7 |
| H-089 | Matizado (§3.3) |
| Nuevo | Vistas por definición (§3.1); referencias anidadas (§3.2); `AbsentSlotsB` (§3.4) |

## 7. Desviaciones e incidentes (registrados en `run-log-m.md`)

**Desviaciones del anexo:**
- M-3 y M-6b se hicieron en M2, no en M3.
- M-5b tomó como referencia el `M2-ins` de la copia M2. La igualdad exacta lo confirma.
- M-2b se hizo en M4 / PB G, no en M6 / PB F. Para que el corte fuera la única definición hubo que purgar además, en la copia, el huérfano «A$C…» que anidaba la planta (§3.2).

Ninguna desviación cambia lo que se mide.

**Incidentes del operador:**
- El foco se fue a «Unidentified app» tres veces. Se recuperó con `AppActivate`, sin `open_application`, salvo la primera vez: entonces se abrió una tercera instancia vacía, que se cerró limpia.
- Una ráfaga larga de teclas se perdió y se repitió por partes.
- Un «pre» y un «ab3» sueltos se ejecutaron sin efecto.

Ninguna medición se repitió por error del operador más de una vez. **Nada se interpuso entre una orden medida y su `U`.**

**La instancia del usuario** (otro AutoCAD con un dibujo de trabajo) no se tocó. Al reanudar ya no estaba abierta; la sesión no la cerró.

## 8. Lo que queda para el Owner y para otras tareas

- **Owner, antes de PB-TC:** M-7 = 0 de 22 sobres con estilo de cotas guardado. Es la vuelta ya prevista; no hay que volver ahora.
- **Para el paquete de PB-T3:**
  - la enumeración por definición (§3.1);
  - las referencias anidadas (§3.2);
  - la regla del último vínculo, que descarta la edición (§3.3);
  - `AbsentSlotsB` (§3.4);
  - el frontal B (§3.5 y §5);
  - un `U` por «Actualizar» (§2).
- **Ningún resultado exige cambiar producto en PB-T0.**
- **Ninguna decisión material nueva:** el bloque M no abre ninguna vuelta al Owner.
