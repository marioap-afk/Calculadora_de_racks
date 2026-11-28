# Revisión independiente de la evidencia del Smoke-PB (Q-PBT5-8), antes de la custodia

**RESULTADO: CUSTODIABLE TRAS CORRECCIONES — B 3 / I 10 / M 14**

- **Privacidad:** no encontré ninguna fuga en lo que se custodiaría. Ni en las 293 capturas recortadas ni en los textos.
- **Lo que bloquea son tres veredictos** que no se sostienen frente al criterio escrito: SPB-15, SPB-06 (a) y SPB-41.
  - Por PBT5-C16 / I-04, la sesión solo puede corregir transcripciones.
  - Como estas correcciones cambian un resultado, vuelven al Owner antes de su firma.
- Los I y los M son correcciones de registro, de declaración o de etiquetado.

**Material revisado:**
- **Capturas:** usé la copia `<carpeta-del-Smoke-PB>\custodia-staging\run\screens\`. Comprobé que es idéntica byte a byte a `<carpeta-del-Smoke-PB>\screens\crop\`: los 293 PNG, `derivation-record.txt` y `no-custodiadas.txt`.
- **Textos:** el registro, los resúmenes, `inv-texts\`, `integrity.md`, `environment-*.md`, `owner-summary.md` y `LEEME.md` de `<carpeta-del-Smoke-PB>\custodia-staging\run\`.
- **Guías:** `smoke-run-v2.md` ([SR]), la guía del operador ([OG]) y el paquete v2.4 ([P]).
- **Contraste, nunca custodiado:** las crudas, `<carpeta-del-Smoke-PB>\inv\` y el registro sin redactar.
- **Fuente:** `ebd304a3`, por `git show`.

**Cómo lo hice:**
- **Las imágenes:**
  - Las 293 capturas se abrieron una a una. Seis revisores de solo lectura se repartieron lotes de unas 49, con la lista de §8.4 y la consigna de no citar nada sensible.
  - Yo abrí además las de más riesgo: las tapadas, las dos ventanas de texto de AutoCAD y la del mensaje de la BOM.
- **Las comprobaciones por guion** (en `...\run-review\work\`, que solo leen la evidencia):
  - hashes y tamaños del registro de derivación;
  - comparación píxel a píxel de cada recorte con su cruda;
  - opacidad de las zonas tapadas;
  - los 446 hashes citados en el registro;
  - los 136 resúmenes de inventario frente a sus crudos;
  - las 44 copias y las 5 `eNN`.

En este informe, «línea N» es la línea del `run-log.md` de staging, salvo que se diga otro archivo.

---

## Hallazgos

### B — bloqueantes

**B-01 · SPB-15: el PASS no se sostiene; un criterio se sustituyó sin declararlo.**
- **Dónde:** `run-log.md` línea 80 (y la 79); `owner-summary.md` línea 14.
- **El criterio** ([OG] SPB-15) mide «sin entrada nueva de Deshacer» y «a los 5 s, sin "(posiblemente obsoleto)"».
- **Lo que muestran las capturas:**
  - `c05-SPB-15-deshacer-despues.png`, frente a `-antes.png`, tiene **dos entradas nuevas: un «Group of commands» sin nombre, arriba, y «Rcdump»**.
  - Hubo dos `RCDUMP` en esa ventana, pero solo aparece un «Rcdump». Así que la entrada sin nombre no se puede atribuir a `RCDUMP`.
  - En SPB-35 la propia sesión atribuye entradas iguales a «Editar» y «Editar todo» del panel (línea 147).
- **Lo que hace el registro:** cambia el criterio por «ninguna lleva nombre de la ventana ni de RackCad», y el PASS de la línea 80 ni siquiera nombra la condición de Deshacer.
- **Además:** después del `RCDUMP`, `DBMOD` pasa a 1 y el panel a «(posiblemente obsoleto)». La sesión dice que no lo puede separar.
- **Corrección:**
  - No proponer PASS.
  - Proponer OBSERVADO, o FAIL frente al criterio escrito con la pregunta de diseño: el `RCDUMP` que pide la guía cae dentro de la ventana medida, así que el criterio no es medible tal cual.
  - Presentarlo al Owner y corregir la tabla de `owner-summary.md`.

**B-02 · SPB-06 (a): el intento 1 cumple la condición de FAIL y se etiquetó «OBSERVADO, informativo»; su repetición se atribuyó a un «error del operador».**
- **Dónde:** líneas 66–68; `owner-summary.md` líneas 14, 33 y 48.
- **El criterio** ([OG] SPB-06): «PASS si `a2` y `a3` conservan el exacto. **FAIL si sale redondeado**».
- **Lo que pasó:**
  - El operador usó el valor de ejemplo de la propia guía, 48.0004.
  - `c02-a1.txt` guarda `PalletDepth` = 48 en todas sus apariciones (lo comprobé en el crudo). Es el valor redondeado.
  - La sesión lo caracterizó con el código (`Keep`, D-PBTC-01). Por la definición de [OG] §1, lo caracterizado no es OBSERVADO.
- **La repetición:**
  - El operador siguió la guía, así que no hubo error del operador. La repetición es «por otra causa» ([P], línea 652 del paquete).
  - `owner-summary.md` da SPB-06 (a)(b) como PASS. Su lista de repeticiones (línea 48) no nombra esta.
- **Corrección:**
  - Registrar el intento 1 como «FAIL frente al criterio escrito (por diseño, D-PBTC-01)», con la pregunta al Owner de si el ejemplo de la guía no era válido.
  - Reclasificar la repetición como decidida por el Owner, por otra causa, y listarla.
  - Corregir la tabla del resumen.

**B-03 · SPB-41: el criterio manda OBSERVADO y el resumen dice PASS.**
- **Dónde:** líneas 164 y 167; `owner-summary.md` líneas 14 y 38.
- **El criterio** ([OG] SPB-41): «PASS si coinciden. **OBSERVADO si se ve el desfase de una pasada** … FAIL solo si difieren».
- **Lo que vio la sesión:** «Restaurar» deja el hueco en 18, y ella misma lo llama «el "desfase de una pasada" previsto en la guía» (línea 167).
- **El problema:** SPB-41 figura en la fila PASS del resumen, y lo del desfase va solo como «OBSERVADO informativo». Así el OBSERVADO, que decide el Owner ([OG] §1 y §7), queda fuera de su tabla.
- **Corrección:** SPB-41 = OBSERVADO, con los inventarios iguales (`1A2B0DCB…`) como parte medida y la diferencia del tooltip como dato.

### I — importantes

**I-01 · Acciones que el contrato prohíbe, descritas en el registro pero no declaradas como desviación, y ausentes de la lista «Lo que hizo la sesión distinto de la guía».**
- **Dónde:** `run-log.md` líneas 56, 100, 115 y 128; `owner-summary.md` líneas 45–50.
- **Las acciones:**
  - **COM para cerrar AutoCAD, dos veces** (`close_c08.ps1` y `close_c13.ps1`: `Close(False)` y `Quit()`). Contra PBT5-C08 («Nunca se usa COM para abrir o cerrar documentos»), [SR] §4.8 («sin COM»), §6 y §9.
  - **`open_application` lanzó una instancia de AutoCAD fuera del bloque 5** (línea 100). No tiene `acad-session-*.json`, y su fin no está registrado.
  - **Texto pegado por el portapapeles en AutoCAD** (línea 56). Contra la regla «nunca Ctrl+V» de [P] §5.5.
  - **Pulsaciones que pudieron ir al AutoCAD del Owner** (línea 115). [SR] §9 dice que nunca se toca un AutoCAD que no abrió la sesión. Ese proceso desapareció a las 20:10 y el registro no sabe por qué.
- **Corrección:** una sección de desviaciones en el registro y en el resumen, con la regla incumplida, el porqué (orden del Owner de cerrar) y el efecto comprobado (copias iguales, ningún `.dwl`), para la disposición del Owner.

**I-02 · Las repeticiones de pasos están mal contadas y mal clasificadas.**
- **Dónde:** líneas 67, 109, 126 y 129–130; `owner-summary.md` línea 48.
- **Lo que pasa:**
  - Hay tres pasos repetidos: SPB-06, 19 y 27. Los tres se rotulan «repetición 1 de 2», como si fueran las repeticiones gratuitas por error del operador.
  - Las tres son «por otra causa»: la guía, el diseño de la prueba o una decisión del Owner. Por [P] (línea 652 del paquete), «cuenta según la definición de intento».
  - El resumen solo nombra dos.
  - Se rehicieron además dos líneas de base (SPB-15 y SPB-35, líneas 79 y 146), tras incidentes de la sesión.
  - c13 se reutilizó para la repetición de SPB-27 (línea 129), aunque la línea 127 la daba por no válida. Su archivo sí era igual a D1.
- **Corrección:** una tabla de repeticiones con la causa, la copia y cómo cuenta cada una para el intento, y que el Owner la confirme.

**I-03 · SPB-19: el PASS mezcla dos pasadas y un procedimiento distinto del escrito.**
- **Dónde:** líneas 107 y 109–111; `owner-summary.md` línea 14.
- **El criterio** ([OG] SPB-19): «PASS si `u1` = `ins` **y `REDO` rehace**», con el orden `U` → `RCDUMP u1` → `REDO`.
- **Lo que pasó:**
  - En la primera pasada `REDO` dio «Nothing to redo», así que con el criterio escrito es FAIL.
  - La repetición, decidida por el Owner, cambió el orden: `U` y `REDO` seguidos, sin inventario tras `U`.
  - El PASS toma `u1` de la primera pasada y `r1redo` de la segunda.
- **El resumen** lo pone en PASS sin decir esto.
- **Corrección:** presentarlo como «FAIL con el orden escrito; PASS con el orden decidido por el Owner», para que el Owner disponga. El mismo efecto de `REDO` se ve en SPB-54 y SPB-55, donde no es criterio.

**I-04 · SPB-34: la mitad de la gemela («Editar») no se pudo hacer y se sustituyó por una lectura de UI Automation.**
- **Dónde:** línea 144; `owner-summary.md` línea 14.
- **Lo que pasó:**
  - [OG] SPB-34 pide «En la gemela, lo mismo con "Editar"».
  - El panel no ofrece «Editar» en ese estado, así que no se hizo.
  - El PASS se apoya en `c18-SPB-34-uia.txt`.
- **Corrección:** calificar el veredicto (PASS en la copia; la gemela, OBSERVADO o LIMITATION porque «Editar» no se ofrece) y llevarlo al resumen.

**I-05 · SPB-44: no se evaluó la condición de OBSERVADO del criterio.**
- **Dónde:** línea 175.
- **El criterio** dice «OBSERVADO si el origen cae en el `Primary`».
- **Lo que falta:** el registro deriva que el origen de POST es la planta, pero no dice si la planta es el `Primary`. En el Candidato, el `Primary` es el primer destino de navegación (`src/RackCad.Application/Workspace/NavigationTargetSet.cs:53`).
- **Corrección:** decir si se puede evaluar. Si no, marcar esa parte como NO VERIFICABLE y dejarla al Owner.

**I-06 · SPB-08 (a) y SPB-16: «byte a byte» se midió a ojo.**
- **Dónde:** líneas 53 y 85.
- **Lo que pasa:**
  - El criterio de (a) es «igual byte a byte». El registro dice «igual a la vista».
  - El literal de `RackPushBackSystemWindow.Composite.cs:77` lleva comillas «» (bytes no ASCII), que una captura no distingue de otras parecidas.
  - La sesión tenía y usó después la lectura por UI Automation, pero no aquí.
- **Corrección:** declarar que la comparación fue visual y que el criterio se aplicó así, o dejar (a) al juicio del Owner.

**I-07 · SPB-02: `RackCad.Domain.dll` no se vio cargado nunca.**
- **Dónde:** línea 46, y las líneas 105, 129 y 207–211 (las salidas del bloque 7).
- **Lo que pasa:**
  - La línea 46 dice que `Domain` y `UI` «se comprueban más adelante».
  - `UI` aparece en c42 a c44. `Domain` (`5D90F61B…`) no sale en ninguna salida del bloque 7.
- **Lo que comprobé:** el DLL del worktree coincide con B3, pero eso no prueba lo que cargó el proceso.
- **Corrección:** limitar el PASS a los módulos observados y declarar `Domain` sin comprobar.

**I-08 · `derivation-record.txt`: el campo `paso` está mal en 19 recortes.**
- **Dónde:** `screens\derivation-record.txt`.
- **Los recortes:**
  - `c20-CONSTRUCTED-SPB-39-actualizado` (dice gemela c21);
  - `c30-CONSTRUCTED-SPB-55-U` (dice SPB-54);
  - `c37-…-gemela-block` y `c37-…-gemela-insert` (dicen «copia, c36»);
  - `c39-…-gemela-layer-lo` (dice «copia, c38»);
  - `c41-…-gemela-purge-planta` (dice «copia, c40»);
  - 8 de c43 (dicen SPB-59);
  - 5 de c44 (cuatro dicen SPB-59 y `aplicado-al-borrador` dice SPB-60).
- **La causa aparente:** cuando dos capturas son idénticas byte a byte, el `paso` se tomó de la primera fila del registro que cita ese hash.
- **Lo que no falla:**
  - Las crudas tienen el nombre y la hora de escritura de su propia corrida.
  - Recorte y cruda coinciden píxel a píxel.
- **Corrección:** corregir el `paso` de esos 19 bloques.

**I-09 · Cabecera del registro incompleta frente a [SR] §7.**
- **Dónde:** línea 11.
- **Lo que falta:**
  - La línea 11 promete que «la versión de AutoCAD, el SHA-256 de la biblioteca y los PID… se completan al abrir AutoCAD y tras B4», y no se completó.
  - **La versión de AutoCAD no aparece en ningún sitio.**
  - Los PID solo están dispersos por las filas. Coinciden con los `acad-session-1..6.json`, que comprobé.
  - La hora de inicio frente a las 13:00 UTC dice solo «después de las 13:00».
- **Corrección:** completar la cabecera con la versión de AutoCAD, los seis PID, el SHA de la biblioteca y la hora exacta de inicio.

**I-10 · `owner-summary.md` no trae la sección «Para tu revisión».**
- **Dónde:** `owner-summary.md` líneas 15 y 31.
- **Lo que pide el contrato:** [SR] §6, [OG] §7 y [P] §5.6 piden una sección «Para tu revisión» con los 9 S+O, **cada uno con su captura recortada y la medida de la sesión**.
- **Lo que hay:** el resumen dice «mirar las capturas de SPB-08, 09…» sin nombrar los recortes ni las medidas.
- **Corrección:** añadir la sección, con los recortes de cada S+O (por ejemplo `c03-SPB-09-r2-lateral.png` o `c10-SPB-20-planta-rotulo.png`) y la medida que ya está en el registro.

### M — menores

- **M-01 · SPB-24: el OBSERVADO sobra.**
  - La línea 121 y `owner-summary.md` línea 35 dicen que el texto de «No se aplicó» «no es el de la guía».
  - Pero [OG] SPB-24 prevé `NotAppliedPreflight` (R-T4-39), y la línea 142 identifica ese mismo texto como `NotAppliedPreflight`.
  - Corrección: dejarlo como información, no como OBSERVADO.
- **M-02 · Los resúmenes de inventario corrompen los nombres no ASCII.**
  - Por ejemplo, 45 caracteres U+FFFD en `e05-estilo.txt.resumen.txt`, donde el crudo tiene la «á» de «Dinámico» en Windows-1252.
  - Los hashes, tamaños y recuentos REF/DEF/BLK/NEST de los 136 resúmenes sí coinciden con sus crudos.
  - Corrección: regenerarlos leyendo el crudo en cp1252.
- **M-03 · `inv-texts\`: cinco archivos se pasaron a UTF-8 desde la página de códigos equivocada.**
  - Son `c15-SPB-30-uia`, `c15-SPB-31-uia-cerrado`, `c15-SPB-31-uia-ventana-abierta`, `c18-SPB-34-uia` y `c19-SPB-36-ventanas-abierta-con-marca`.
  - El crudo usa la página OEM (byte 0xA2 para «ó»), así que el custodiado dice «anotaci¢n». `LEEME.md` dice «Windows-1252».
  - Corrección: convertirlos desde cp850, o custodiar los bytes crudos.
- **M-04 · El marcador de redacción no es el de la guía.**
  - El registro usa `<raiz-del-smoke>` en lugar de la raíz de trabajo, y quedan textos como `<raiz-del-smoke>\smoke\salidas\…` (líneas 25, 90, 91, 100, 101 y 128).
  - El preámbulo de [SR] fija `<carpeta-del-Smoke-PB>` para la carpeta del smoke y `<carpeta-de-respaldo>` para la de respaldo.
  - No hay fuga, porque no queda ninguna letra de unidad. Es una cuestión de coherencia.
- **M-05 · Textos desactualizados.**
  - `owner-summary.md` líneas 52–55 dicen que falta «la custodia … 369 crudas», cuando la preparación de la custodia ya está hecha.
  - La línea 11 del registro (ver I-09).
- **M-06 · Pares de capturas idénticas byte a byte presentados como dos momentos.**
  - SPB-27 repetición, «15 s» y «25 s» (línea 130): sin hash en el registro y sin decir que son idénticas. Ninguna captura de esa repetición lleva hash en el registro.
  - SPB-28, «3 s» y «18 s» (línea 132).
  - SPB-15, «2 s» y «7 s» (línea 80).
  - Una imagen idéntica no prueba por sí sola el segundo momento. Las horas de escritura de las crudas tampoco, porque se guardan por lotes (varias en el mismo milisegundo).
  - Corrección: añadir los hashes y decir que son idénticas.
- **M-07 · SPB-47 y SPB-48 cuentan e03 (D4) como 0.**
  - Líneas 176–177 y `inv-texts\orden12-recuentos.txt`.
  - Su `RCDUMP` falló: el archivo solo tiene la línea `CLAYER`, 10 bytes.
  - Debe decir «sin inventario», como en SPB-07.
- **M-08 · P-04 siguió adelante tras un STOP del bloque B3 (`catalogs`), resuelto por la sesión** (línea 30).
  - La diferencia es benigna: solo `.gitkeep`.
  - Pero no consta que el Owner lo aceptara, y no está en las desviaciones del resumen.
- **M-09 · La línea de base de P-05 para la carpeta de registros no es fiable.**
  - P-05 dice que la carpeta «no lista ningún `rackcad-*.log`». Las dos carpetas candidatas tienen archivos anteriores a la corrida (`environment-post.md`).
  - `environment-pre.md` está reconstruido desde el registro, no desde la salida de los bloques.
  - Corrección: declarar que la conclusión de SPB-52 se apoya solo en el bloque 15.
- **M-10 · La operación sin el Owner no figura como desviación.**
  - Desde las 23:49 UTC la sesión operó sin el Owner presente. [SR] §2 y §6 y [OG] línea 7 dicen «con el Owner delante».
  - La autorizó el Owner, y el resumen lo dice en su línea 3, pero no figura como desviación del contrato.
  - Tras el FAIL de SPB-33 (00:45) no se registró la valoración de PBT5-C05: «solo pasos independientes e inocuos».
- **M-11 · Gestos y herramientas añadidos por la sesión, que no están en la lista de desviaciones del resumen:**
  - «Confirmar» en SPB-09 y SPB-10;
  - «Comprobar ahora» como vía a la presentación en SPB-24 y SPB-27;
  - un «Actualizar» extra de RACKEDITAR en SPB-42a;
  - un «Editar todo» extra en SPB-44 y un «Actualizar» extra en SPB-63;
  - «Fijar» en SPB-33;
  - «Descartar y recargar» antes de la repetición de SPB-19;
  - los lectores de UI Automation y Win32, y `panel_watch.ps1`, que el propio registro sospecha que alteró la entrada en c28.
  - Además, los scripts que produjeron `inv-texts\` (`uia_edits.ps1`, `winenabled.ps1`, `clickwin_timer.ps1` y `panel_watch.ps1`) no están en la lista del respaldo de la línea 101.
- **M-12 · Faltan las coordenadas de clic de los resultados negativos.**
  - No están en los dos FAIL: SPB-27 intento 1 y SPB-33.
  - Las piden [SR] §7 y [OG], en cada paso.
- **M-13 · Dos recortes no muestran del todo lo que dicen.**
  - `c26-CONSTRUCTED-SPB-42b-ambos-lados.png`: el selector «Ambos lados» queda fuera del recorte.
  - `c25-CONSTRUCTED-SPB-41-gemela-hueco-18-tooltip.png`: muestra el tooltip de «Restaurar», como dice el registro, con una fila de botones del pie solapada.
  - Corrección: ampliar el rectángulo o anotarlo en el registro de derivación.
- **M-14 · Detalles innecesarios en un archivo público** (no identifican a nadie; opcional).
  - Línea 17 e `integrity.md`: un rasgo de la ruta local de D1, que «lleva un apóstrofo».
  - Línea 104: el AutoCAD del Owner se abrió desde un adjunto de correo.

---

## Lo comprobado y correcto

- **Privacidad de las capturas:**
  - Las 293 se abrieron. Ninguna muestra letra de unidad ni ruta, ni nombre de usuario o de cuenta de Autodesk, ni cliente, proyecto, lámina o presentación sin tapar.
  - Tampoco hay barra de tareas, escritorio u otra aplicación, ni la paleta de referencias externas, el aviso de NETLOAD o un diálogo de archivos.
  - Ninguna línea de órdenes deja ver el eco de `NETLOAD`, de `vla-open` ni de un `defun`. Las dos «AutoCAD Text Window» (c28 y c40) empiezan por debajo de los `defun`. En c40 se ve un `vla-close` con solo el nombre `c39.dwg`, sin ruta.
  - Los cinco no custodiados (`no-custodiadas.txt`) no están entre los recortes.
- **Las zonas tapadas:**
  - Las 16 capturas con zonas tapadas tienen todos sus píxeles exactamente en RGB(128,128,128).
  - Fuera de las zonas, cada recorte es copia exacta, píxel a píxel (diferencia máxima 0), del rectángulo de su cruda.
  - Las de la presentación (c34), la ruta de la BOM (c06) y el resumen y la tabla del proyecto (c09) cubren el texto entero, visto a mano.
  - Siete máscaras empiezan por encima del rectángulo; su parte dentro cubre lo que debe.
- **El registro de derivación:**
  - 293 bloques, uno por PNG.
  - El SHA-256 de la cruda y el del recorte coinciden con los archivos en los 293.
  - El rectángulo coincide con el tamaño del PNG.
  - 298 capturas citadas en el registro = 293 + 5 no custodiadas; 369 − 298 = 71 crudas sin citar, como dice la línea 218.
- **Privacidad de los textos:**
  - Ninguna letra de unidad, ruta de usuario, nombre de usuario de Windows ni correo en `run-log.md`, `owner-summary.md`, `integrity.md`, `environment-*.md`, `LEEME.md`, `inventories\` ni `inv-texts\`.
  - Los nombres de los resúmenes son nombres de rack o de bloque genéricos, y las capas son `0` y `A-TEXTO`.
  - El registro redactado difiere del local solo en las 9 sustituciones de la raíz, como dice `LEEME.md`.
- **Fidelidad del registro:**
  - Los 446 hashes de 8 hex citados en el registro corresponden a archivos reales: crudas, inventarios, DLL, D1 y la lista local.
  - Los 118 pares nombre–hash casan, salvo tres abreviaturas de nombre en SPB-49 que sí corresponden.
  - Se cumplen todas las igualdades declaradas, entre ellas:
    - `c07-ins` = `c08/09/12/13r1/42/43/44-ins`;
    - `c07-post` = `c08-post` = `c09-r1redo`;
    - `c09-u1` = `ins`;
    - `c12-ext` = `spb24`;
    - `c17-spb34` = `c18-spb34`;
    - `c20…c26` `pre`, `ab1` y `ab2`;
    - `c21-spb39` = `c20`;
    - `c22-spb40` = `ab2`;
    - `c24-spb41` = `c25`;
    - `c26-spb42a2` = `a1`;
    - `c36/37` construido y post;
    - `c38/39` ins = construido = post;
    - `c40/41` construido = post;
    - `c42-post` = `ins`; `c43` post = check = `ins`; `c44` post = check;
    - `c29-u1`, `c30-u1`, `c32-spb63`, `c33-spb64` y `c16-spb33` = `pre`.
  - Contenidos comprobados en los crudos:
    - `PalletDepth` 48 en `a1` y 48.1234 en `a1r`/`a2r`/`a3r`;
    - escala 2 en `a3r`;
    - `ManualHeaderHeightOverride` en `c03-r1` y `r2`;
    - `Length` 48.1234 en `c04-spb10`;
    - REF/DEF 15/18 → 14/17 y el lateral 73 borrado en `c07-post`, `c44-post` y `c27-spb44`;
    - en `c31-spb62`, solo falta la REF `30CFEE`;
    - `c27-ins` difiere de `c07-ins` solo en el handle;
    - `DimensionStyle` nulo en los Push Back.
  - Los PID y las horas de inicio de los `acad-session-1..6.json` coinciden con el registro, y los de inyección llevan su variable.
  - Los SHA-256 de `smoke-run-v2.md`, del paquete v2.4 y de `operator-guide.md` coinciden con la cabecera y con P-08.
  - Los DLL del worktree coinciden con B3.
- **Integridad (SPB-51), recalculada:** las 44 `cNN` son iguales al SHA de D1 de `copias.json`; las 5 `eNN` son iguales a `estilo.json`; no hay ningún `.dwl`, `.sv$` ni `.bak` en la carpeta del smoke.
- **Veredictos que sí se sostienen** frente a su criterio (además de los de arriba):
  - SPB-01, 03, 04, 05, 09 (S), 10, 11, 12, 13, 14, 17, 18 (a)–(d) y (f), 20 (S), 21, 25 (S), 26, 28, 29–32, 35, 36 (modal), 37–40, 42, 46, 49, 50–64;
  - los FAIL de SPB-27 (intento 1) y SPB-33;
  - los OBSERVADO de SPB-62 y 63;
  - las LIMITATION de SPB-45 (la regla de §4.12 aplicada al pie de la letra) y SPB-47 (`discrepancias=1`);
  - NOT AVAILABLE en SPB-43, 48 y 56; NO VERIFICABLE en SPB-22 y 65; SPB-07 PENDIENTE DEL OWNER.
  - **Los 65 pasos tienen veredicto propuesto.**
- **Lo que no se hizo:**
  - Nadie pulsó «Always Load» ni «Always ignore».
  - `settings.json` y la disposición tienen el mismo contenido antes y después.
  - No se guardó ninguna copia, no se abrió ningún original y no queda ningún `RACKCAD_DEBUG_*`.
- **Completitud** frente a [SR] §7:
  - Están el registro, los 136 resúmenes con `INDICE.txt` y el resumen de `c01-rcdyn`, `integrity.md`, `environment-pre.md` y `environment-post.md`, `owner-summary.md`, los recortes, `derivation-record.txt` y `no-custodiadas.txt`.
  - Las faltas están en I-09, I-10 y M-09.

---

## Recomprobación

**RESULTADO-RECOMPROBACION: CUSTODIABLE TRAS CORRECCIONES — quedan B 0 / I 1 / M 6**

**Qué se recomprobó** (acotado, solo lectura, las mismas reglas):
- el `run-log.md` local, con la cabecera, las líneas 17 y 104 y la sección nueva «Tras la revisión independiente…» (líneas 220–331);
- la carpeta de staging regenerada: `run-log.md`, `owner-summary.md`, `LEEME.md`, `integrity.md`, `inventories\`, `inv-texts\` y `screens\derivation-record.txt`;
- en la fuente de `ebd304a3`: `NavigationTargetSet.cs`, `NavigationTargets.cs` y `PushBackPilotUpdatePlan.cs`.

**Lo que no cambió y se comprobó otra vez:**
- Los 293 PNG son los mismos (el hash de sus hashes es igual en las dos copias) y siguen casando con el registro de derivación.
- El registro de derivación es idéntico en `screens\crop\` y en staging.
- El staging difiere del registro local solo en las sustituciones de la raíz, ahora `<raiz-de-trabajo>` y `<carpeta-del-Smoke-PB>`.
- Ningún texto cambiado lleva una letra de unidad, una ruta de usuario, un nombre de usuario de Windows o un correo.
- Los 136 resúmenes siguen casando en hash, tamaño y recuentos.

### Estado de cada hallazgo

Clave: (a) corregido · (b) llevado bien al Owner · (c) sin resolver.

| Hallazgo | Estado | Nota |
|---|---|---|
| B-01 SPB-15 | (b) | FAIL frente al criterio escrito, con la pregunta en el resumen (punto 2) |
| B-02 SPB-06 (a) | (b) | El intento 1 es FAIL frente al criterio; la repetición, «por otra causa»; pregunta 3 |
| B-03 SPB-41 | (b) | Pasa a OBSERVADO, en la tabla y en la pregunta 7 |
| I-01 acciones prohibidas | (a)+(b) | Declaradas en el registro (§«Desviaciones», puntos 1–4) y en el resumen, «si las aceptas» |
| I-02 repeticiones | (a)+(b) | Tabla con causa y cuenta; las confirma el Owner |
| I-03 SPB-19 | (b) | «FAIL con el orden escrito; PASS con el decidido»; pregunta 4 |
| I-04 SPB-34 | (b) | PASS en la copia y LIMITATION en la gemela, en la fila «PASS con límite» |
| I-05 SPB-44 | (a) | La derivación es correcta (ver abajo); se declara derivada, no observada |
| I-06 SPB-08 (a) y SPB-16 | (b) | Declarado «a la vista»; (a) va con el juicio O. Ver M-N4 para SPB-16 en el resumen |
| I-07 SPB-02 | (a) | PASS limitado a los módulos vistos; `Domain` sin comprobar en el proceso |
| I-08 `paso` de 19 recortes | (a) | 0 discordancias entre el `paso` y el nombre; muestras de c20, c30, c37, c43 y c44, bien |
| I-09 cabecera | (a) | Comprobé la versión `R25.0.171.0.0` contra el `acad.exe` registrado, igual en los 6 `acad-session-*.json`. Ver M-N2 |
| I-10 «Para tu revisión» | (a) | Los 9 S+O, con sus 21 recortes (todos existen en `screens\`) y la medida |
| M-01 SPB-24 | (a) | Información, no OBSERVADO |
| M-02 resúmenes cp1252 | (a) | Ningún U+FFFD en staging; se lee «Dinámico» |
| M-03 `inv-texts` desde cp850 | (a) | Se lee «anotación»; la decodificación cp850 del crudo es idéntica al custodiado |
| M-04 marcadores | (a) | Quedan `<raiz-del-smoke>` y «seis … de Windows-1252» en la fila histórica 218, que «se queda como se escribió»; la sección nueva lo corrige |
| M-05 textos viejos | (a) | Ver M-N1 por `LEEME.md` |
| M-06 capturas idénticas | (a) | Ver M-N3 por la redacción |
| M-07 e03 sin inventario | (a) | |
| M-08 STOP de P-04 | (b) | |
| M-09 línea de base de P-05 | (a) | |
| M-10 operación sin el Owner y PBT5-C05 | (a)+(b) | |
| M-11 gestos y lectores | (c), parcial | Declarados. Pero `uia_edits.ps1`, `winenabled.ps1`, `clickwin_timer.ps1` y `panel_watch.ps1` siguen fuera de `sesion-respaldo\`; produjeron `inv-texts\` |
| M-12 coordenadas | (a) | Añadidas. No se pueden verificar del todo (otro marco de pantalla), pero la y de SPB-33 cuadra con el botón en la captura |
| M-13 notas de los recortes | (a) | |
| M-14 detalles innecesarios | (c), parcial | Quitados de las líneas 17 y 104 y de `integrity.md`. **Siguen en la fila de SPB-51 (línea 213),** en el registro local y en el de staging: el rasgo de la ruta local de D1 |

### La derivación de SPB-44 (I-05), comprobada en `ebd304a3`

- **El `Primary`** es `Targets[0]` (`NavigationTargetSet.cs:53`; la 52 es su comentario).
- **El orden de los destinos** lo da `CompareCandidates` (`NavigationTargets.cs:201-216`): primero `NavigationViewOrder`, con Frontal = 0, Lateral = 1, Planta = 2 y Other = 3 (`NavigationTargetSet.cs:8-14`); después el handle.
- **El origen de POST** es `plans[0].Key` cuando se borra el origen (`PushBackPilotUpdatePlan.cs:511-512`). Los `members` se recorren ordenados por clave (`:419`).
- **En el dibujo:** tras SPB-44 quedan la planta (`2F3B9B`, definición `2F3884`) y el lateral 1 (`30CFEE`, definición `30C41C`); `c27-spb44` lo confirma.
- **Conclusión:** el `Primary` es el lateral 1 y el origen de POST es la planta. **«No cae en el `Primary`» es correcto, como derivación.**

### Lo que queda

**I-N1 · `owner-summary.md`: la reescritura dejó fuera los OBSERVADO informativos.**
- El resumen anterior los listaba (su punto 5), para que el Owner los decida.
- El nuevo solo da como OBSERVADO SPB-41, 62 y 63 (la tabla y la pregunta 7).
- Siguen marcados OBSERVADO en sus filas del registro, sin reclasificar en la sección nueva, y ya no llegan al Owner:
  - SPB-19: tras `U`, el panel queda «posiblemente obsoleto» (línea 110);
  - SPB-25: la frase de la escala. Solo aparece como texto en la tabla «Para tu revisión», no como OBSERVADO;
  - SPB-40 y SPB-53: los avisos nombran la vista por su handle (líneas 158, 161 y 180);
  - SPB-44: con el borrador limpio no se ofrece «Editar» (línea 175);
  - SPB-15: un `RCDUMP` deja `DBMOD` = 1 y el panel «posiblemente obsoleto» (línea 80);
  - la pregunta de «Cerrar» en c05 (línea 87).
- [SR] §6 y [OG] §7 piden llevar al Owner «los OBSERVADO, cada uno con toda su evidencia».
- **Corrección:** volver a listarlos en el resumen, o reclasificarlos de forma declarada en la sección nueva.

**Menores:**
- **M-N1 · Textos de estado inexactos.**
  - `owner-summary.md` dice que la revisión «cambió cinco propuestas». También cambiaron SPB-02 y SPB-44 (a «PASS con límite») y SPB-24.
  - `LEEME.md`, en «Antes de custodiar», pone la revisión independiente como paso futuro, cuando el mismo archivo dice que ya se hizo.
- **M-N2 · La cabecera atribuye a la PID 26256 «c09–c12».** c13 se abrió en esa instancia para el intento 1 de SPB-27 (20:31–20:42) antes de reutilizarse en la 13996. Debe decir c09–c13.
- **M-N3 · Redacción poco precisa.**
  - Línea 315: «lo dice la hora del lote». La hora de escritura de las crudas es la del lote en que se guardan, así que tampoco prueba el segundo momento. Mejor decir que ni la imagen ni la hora de escritura lo prueban.
  - Línea 240: sigue citando «sin "(posiblemente obsoleto)" a los 7 s» como dato, cuando esa captura es la misma imagen que la de los 2 s.
  - Línea 260: cita `NavigationTargetSet.cs:52`; la propiedad está en la 53.
- **M-N4 · La fila de SPB-16 en «Para tu revisión».**
  - Dice «sobre el panel» y «El aviso, en el panel», pero el aviso sale en la ventana de «Editar todo» abierta desde el panel.
  - No dice que el literal se comparó a la vista, como sí hace la fila de SPB-08 (I-06).
- **Residuales:** M-11 (los cuatro scripts de lectura, fuera del respaldo) y M-14 (el rasgo de la ruta de D1, en la línea 213).

Contadas como quedan: I-N1; M-N1, M-N2, M-N3, M-N4, M-11 y M-14 residuales. Agrupo M-N1 a M-N4 y los dos residuales en **6 M**. Ningún hallazgo nuevo es de privacidad.

---

## Confirmación final

**RESULTADO-FINAL: CUSTODIABLE — no queda nada abierto (B 0 / I 0 / M 0)**

Comprobación acotada y de solo lectura de lo que quedaba en la recomprobación:

- **I-N1, cerrado.**
  - `owner-summary.md` tiene una sección nueva, «Los OBSERVADO informativos», con SPB-15, c05, SPB-19, SPB-25, SPB-40/53, SPB-41 (el tooltip) y SPB-44, más el hecho informado de SPB-46.
  - La pregunta 7 los remite al Owner.
  - El registro tiene la subsección «Los OBSERVADO informativos de las filas» (líneas 333–344). Dice que SPB-06 (a) pasa a FAIL y SPB-24 a información.
  - El resumen de staging es idéntico byte a byte a `<carpeta-del-Smoke-PB>\resumen-para-el-owner.md`.
- **M-N1, cerrado.** El resumen dice «ocho propuestas: SPB-02, 06 (a), 15, 19, 24, 34, 41 y 44». `LEEME.md` dice que la revisión se hizo antes del commit, con su recomprobación.
- **M-N2, cerrado.** La cabecera dice 26256 (c09–c13), en el registro local y en el de staging.
- **M-N3, cerrado.**
  - La línea 240 dice ahora que la captura de los 7 s es idéntica a la de los 2 s.
  - La línea 315 dice que ni la imagen ni la hora de escritura (que es la del lote) prueban el segundo momento.
  - La línea 260 cita `NavigationTargetSet.cs:53`.
- **M-N4, cerrado.** La fila de SPB-16 dice «la ventana de "Editar todo"» y que el texto se comparó «a la vista».
- **M-11, cerrado.** `uia_edits.ps1`, `winenabled.ps1`, `clickwin_timer.ps1` y `panel_watch.ps1` están en `sesion-respaldo\_session_t5\`, que es local y no se custodia.
- **M-14, cerrado.** La fila de SPB-51 (línea 213) ya no describe la ruta de D1, en el registro local ni en el de staging. Ningún texto custodiado la menciona.

**Nada nuevo:**
- El registro de staging difiere del local solo en las sustituciones de marcadores.
- El barrido de los textos de staging solo da los falsos positivos ya vistos: nombres como `spb10` o `spb15a`, el fragmento `…\Temp\`, las aproximaciones «~» y `%APPDATA%`.
- Ninguna letra de unidad, ruta de usuario, nombre de usuario ni correo.
- Los 293 PNG y `derivation-record.txt` no cambiaron: los mismos hashes, e idénticos en las dos copias.

**Lo que sigue pendiente no es de la custodia.** Lo llevado al Owner (B-01 a B-03, I-03, I-04, I-06 y M-08, más su aceptación de I-01, I-02 y M-10) queda para su decisión y su firma.
