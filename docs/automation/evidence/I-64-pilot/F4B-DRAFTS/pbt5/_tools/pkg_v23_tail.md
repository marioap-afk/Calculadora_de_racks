## 5. Cómo se ejecuta

Las órdenes exactas están en [SR]. Aquí van las reglas.

### 5.1 Los dibujos

- **Solo copias de trabajo.** Los originales nunca se abren, ni con AutoCAD ni con `accoreconsole`: solo se leen para copiarlos.
- **Antes de usar un original,** un respaldo, comprobado por su SHA-256. D1 se compara además con el SHA-256 de `F4-SMOKE-2/run/integrity.md` antes de escribir nada.
- **Cada copia sale del original.** Nada se guarda sobre un original, y las copias se cierran sin guardar.
- **Las carpetas,** `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>`, son nuevas (si `<raiz-de-trabajo>\` ya existe, se para) y quedan fuera del perfil del usuario, del worktree, del checkout principal y de todo repositorio, comprobado con `git rev-parse`.
- **Ninguna escritura sobrescribe** un archivo existente.
- **Las salidas del producto,** solo en `<carpeta-del-Smoke-PB>\salidas`.
- **Privacidad:** sin nombres ni rutas de clientes. Los JSON crudos se quedan en local, con su SHA-256.

### 5.2 El DLL

- **El DLL Debug del worktree,** compilado en el Candidato con AutoCAD cerrado, con los comandos de [G] §2 y el `dotnet` del usuario, desde la raíz del worktree.
- **Antes de NETLOAD,** la `ProductVersion` termina en `+<Candidato>`, y se registra el SHA-256 de los cuatro DLL.
- **El build va antes del commit de la emisión** (§3.1).

### 5.3 La configuración de AutoCAD, la carga y el inventario

- **No se toca nada:** ni Trusted Locations, ni SECURELOAD, ni «Always Load», ni el registro, ni ninguna variable de sistema. Tampoco «Always ignore» ni la casilla de «Proxy Information».
- **La carga,** como en [G] §3: NETLOAD del DLL exacto, por expresión, y «Load Once» en el aviso.
- **El inventario,** con las funciones de [BM], tecleadas en cada documento, más `rcblk`, `rcdyn`, `rcnest` y `rcdynchk`, de solo lectura, con sus controles. `RCDUMP` no sobrescribe.
- **El paso de una copia a la siguiente,** sin COM (I-07 de [IR1]):
  1. `vla-open` de la siguiente desde la activa;
  2. `EXTERNALREFERENCESCLOSE`;
  3. el inventario;
  4. `vla-close … :vlax-false` de la anterior.
- **La instancia de AutoCAD de la sesión** se registra al lanzarla: el PID, la ruta y la hora de inicio en `Ticks`.
  - Cada bloque que la toca comprueba esos tres datos.
  - Si se queda sin documentos, se cierra con `CloseMainWindow`, nunca con `Stop-Process`.
- **Cada bloque de PowerShell funciona solo** (I-08): `Set-StrictMode`, `Stop`, sus variables y sus comprobaciones, y `exit 0` al final.

### 5.4 Quién opera

- **La frontera de esta noche** (la lectura de la sesión principal del mensaje del Owner; Q-PBT5-5 no se le preguntó):
  - la sesión opera el Smoke-PB contratado **solo si puede empezar antes de las 13:00 UTC** del 2026-11-23;
  - **a las 13:00 UTC para** en el siguiente límite de paso y espera al Owner. AutoCAD y la copia abierta se quedan como estén, sin guardar, y solo se hacen SPB-51 y SPB-52, que leen.
- **Después de la frontera,** o si el Owner aparece:
  - **el Owner** opera los gestos del producto de los pasos que quiera;
  - **la sesión** mide lo de máquina, avisa antes de teclear y devuelve el control después.
- **Lo que se le deja al Owner, siempre:**
  - **los juicios visuales de los 9 S+O,** con su captura (SPB-08, 09, 16, 18, 20, 23, 25, 36 y 39), como PENDIENTE DE REVISIÓN DEL OWNER;
  - **SPB-07,** si ningún dibujo guarda un estilo;
  - **los OBSERVADO,** con su evidencia;
  - **la disposición,** después de la revisión independiente de la evidencia;
  - **la firma.**
- **Los resultados que registra la sesión son propuestas.**

### 5.5 Las inyecciones (Q-PBT5-10, «Hacerlas como en Smoke-2»)

- **Tres corridas,** SPB-59 a SPB-61, cada una con su copia y **un AutoCAD propio por inyección**, lanzado por la sesión.
- **La variable,** `RACKCAD_DEBUG_FAIL_PILOT_UPDATE_UNIT=2`, `…_SEAL=before` o `…_SEAL=after`, **solo en el entorno de ese proceso** (`ProcessStartInfo.Environment`), nunca en el usuario, la máquina ni el shell.
  - **Antes y después de cada corrida,** se comprueba que no está en ninguno de los tres.
  - Ninguna otra instancia de la sesión puede estar viva al lanzarla.
- **Cada corrida termina** con «Descartar borradores de este documento» → «Descartar» → `QUIT` → «No», y la comprobación de que la instancia salió.
- **Nada cambia el producto.** Las órdenes exactas, en [SR] §4.11.

### 5.6 Los contextos construidos (Q-PBT5-4, «Construirlos y rotularlos»)

- **Los cuatro de SPB-45, 46, 57 y 58,** en copias, con órdenes nativas tecleadas: `-LAYOUT` y `-INSERT`, `-BLOCK`, `-LAYER`, y `ERASE` con `-PURGE`.
- **Cada uno se compara con RACKEDITAR** en una gemela construida igual.
- **Cada uno va rotulado CONSTRUCTED REPRESENTATIVE TEST CASES** en la guía, el registro, las etiquetas de `RCDUMP` y el nombre de las capturas.
- **La línea entre preparar y construir:**
  - insertar una vista con RACKEDITAR es un gesto del producto, y se usa como preparación;
  - poner el dibujo con órdenes nativas en un estado que el producto no crea es construir.
- **Las órdenes exactas,** en [SR] §4.12.

### 5.7 El recuento del estilo de cotas (Q-PBT5-2, «Contar antes en otros»)

- **Antes de SPB-07,** en la precondición P-09: de cada original de la selección de Smoke-2 con racks Push Back, un respaldo y una copia.
- **Cada copia se abre** en la instancia de la sesión, solo para `RCDUMP estilo`, y se cierra sin guardar.
- **El recuento** se hace fuera de AutoCAD, con `dimkeys.py` de [BM].
- **Si alguno guarda un estilo,** SPB-07 se hace con dos copias nuevas de ese dibujo. **Si ninguno,** SPB-07 vuelve al Owner.
- **Las órdenes,** en [SR] §4.13.

### 5.8 Las reglas de operación, la evidencia y las capturas

- **Las reglas de operación por control de pantalla,** como en la v2.2 (§5.5):
  - trozos de 16 caracteres como máximo;
  - una captura antes de cada clic;
  - coordenadas y captura de después en cada resultado negativo;
  - nunca Ctrl+V;
  - N-I1;
  - dos repeticiones como máximo por error del operador.
- **La evidencia,** en `I-64-pilot/F4B-SMOKE-PB/` (`prep/` y `run/`), como en la v2.2 (§5.6).
  - La cabecera del registro lleva el sha256 de [SR], de este paquete y de la guía redactada.
  - La sección «Para tu revisión» lleva los 9 S+O.
- **Las capturas,** como en [SR] §8:
  - las crudas, nunca custodiadas;
  - las custodiadas, recortadas o tapadas con un rectángulo opaco, con la lista de antes de custodiar y `derivation-record.txt`;
  - nunca la paleta de referencias externas, el aviso de NETLOAD ni los diálogos de archivos.

### 5.9 Lo que no se finge

- **Ninguna condición interna que AutoCAD no muestre** (SPB-22 y SPB-65), ningún arnés y ningún código de producto para probar.
- **Ninguna escritura en el sobre de un rack fuera del producto:** el estilo de cotas no se fabrica.
- **Las variables `RACKCAD_DEBUG_*`,** solo en el entorno de los tres AutoCAD de SPB-59 a SPB-61.
- **Ningún caso fabricado** salvo los A/B de D-PB-09 y los cuatro contextos de Q-PBT5-4, rotulados.

---

## 6. El contrato (forma B, la que eligió el Owner)

### 6.1 La forma

- **El Owner eligió «Adaptación con revisión»** (Q-PBT5-8):
  - contrato propio, con invariantes, paradas, evidencia y presupuesto;
  - sin Controller, sin nc1 a nc4 y sin D2;
  - **antes de su firma, un revisor aparte comprueba en solo lectura que el registro, las capturas y los inventarios coinciden.**
- **El contrato es este §6.** El guion de la emisión lo saca tal cual a `F4B-PBT5-SMOKE-PB-GATE-CLOSE/coordinator/contract.md` (§6.7).
- **La v3 del sustituto de nc2** (F4B-E58) se eligió con una opción que decía «la aplico a este nc2 y a PB-T5». Con la forma B no hay nc2 en PB-T5, así que no se usa. La forma B la eligió el Owner después, sabiendo que no tiene nc1 a nc4.
- **El cierre es por la firma** ([F] §9; F4-E81), tras la revisión de la evidencia.

### 6.2 La identidad y el alcance de escritura

- **`TaskId`:** `F4B-PBT5-SMOKE-PB-GATE-CLOSE`. La emisión lo fija.
- **En el repositorio,** solo:
  - `I-64-pilot/F4B-SMOKE-PB/`;
  - `I-64-pilot/F4B-PBT5-SMOKE-PB-GATE-CLOSE/` (el contrato, la revisión de la evidencia y el paquete de cierre);
  - `F4B-DRAFTS/pbt5/`, `F4B-DRAFTS/coordinator/` y `F4B-DRAFTS/gate-close.md`;
  - [DISP], `I-64-evidence.md` e `I-64.yml`;
  - `docs/initiatives/I-64-f4b-pbt5-package.md`, este paquete;
  - `docs/ideas-futuras.md`, solo por orden del Owner.
- **Fuera:** `<raiz-de-trabajo>\` y el `bin/` y el `obj/` del worktree.
- **Prohibido:**
  - `src/`, `tests/`, `assets/` y los archivos de proyecto;
  - [G], HANDOFF, ROADMAP, WORKFLOW y AGENTS;
  - `main` y otras ramas;
  - los originales;
  - la configuración de AutoCAD y el registro;
  - las bibliotecas y `settings.json` del usuario.

### 6.3 Invariantes

| Id | Invariante |
|---|---|
| INV-PBT5-01 | `git diff <Candidato> <punta del cierre de F4B> -- src tests assets` vacío |
| INV-PBT5-02 | El DLL sale del Candidato: su `ProductVersion` lo identifica, y su SHA-256 se registra antes de NETLOAD y se comprueba en el proceso |
| INV-PBT5-03 | Solo copias; los originales, con respaldo y sin abrir |
| INV-PBT5-04 | Ningún cambio de configuración: «Load Once» y ningún «Always …» |
| INV-PBT5-05 | Nada se finge. Ninguna `RACKCAD_DEBUG_*` en el usuario, la máquina ni el shell; solo en el entorno de los AutoCAD de SPB-59 a SPB-61 (Q-PBT5-10) |
| INV-PBT5-06 | Los A/B y los cuatro contextos construidos llevan el rótulo **CONSTRUCTED REPRESENTATIVE TEST CASES**, y cada contexto construido tiene su gemela clásica (Q-PBT5-4) |
| INV-PBT5-07 | Los resultados de la sesión son propuestas. F4B se cierra solo con la firma del Owner, después de la revisión independiente de la evidencia (Q-PBT5-8) |
| INV-PBT5-08 | La evidencia custodiada no lleva rutas locales, nombres de clientes ni datos personales |
| INV-PBT5-09 | [G] no cambia |
| INV-PBT5-10 | Sin integración en `main`; HANDOFF y ROADMAP sin cambios |
| INV-PBT5-11 | Una captura antes de cada clic, abierta y revisada; coordenadas y captura de después en cada resultado negativo |
| INV-PBT5-12 | N-I1 |
| INV-PBT5-13 | Las salidas del producto, solo en `<carpeta-del-Smoke-PB>\salidas` |
| INV-PBT5-14 | Ningún FAIL se corrige en PB-T5 |
| INV-PBT5-15 | La UI completa no corre en local: la dispensa de Smoke-2, extendida por el Owner (Q-PBT5-1). La regla de F4-E59 no se levanta |
| INV-PBT5-16 | Cada bloque de PowerShell funciona solo, para si un valor está vacío o una ruta no existe, nunca escribe en la raíz de una unidad y termina con `exit 0` |
| INV-PBT5-17 | El estilo de cotas no se fabrica. Si ningún dibujo real lo guarda, SPB-07 vuelve al Owner (Q-PBT5-2) |

### 6.4 Paradas

Son las de la v2.2 (§6.4: PBT5-C01 a PBT5-C15), con estos cambios:
- **PBT5-C04:** una `RACKCAD_DEBUG_*` en el usuario, la máquina o el shell; o en el entorno de un AutoCAD que no sea el de su inyección.
- **PBT5-C11:** a las 13:00 UTC, se para en el límite de paso y se espera al Owner; después, solo SPB-51 y SPB-52, que leen.
- **PBT5-C15** (Q-PBT5-8, confirmada): una prueba que falla en la CI o en local sobre el Candidato vuelve al Owner. **La CI en rojo solo por el detector de cuelgue en una prueba lenta conocida** admite una reejecución de los trabajos fallidos sobre el mismo SHA; otra causa, o un segundo rojo, vuelve al Owner.
- **PBT5-C16, nueva:** la revisión independiente de la evidencia encuentra que el registro, las capturas o los inventarios no coinciden. Vuelve a la sesión, que corrige el registro o repite el paso, sin cambiar un resultado, y otra vez a la revisión. Si hace falta repetir un paso en AutoCAD, es una adenda, que no gasta intento (§6.6).
- **PBT5-C17, nueva:** una de las tres condiciones de la emisión condicional (Q-PBT5-7) no se cumple. No se emite, y vuelve al Owner.

### 6.5 La evidencia esperada

- `I-64-pilot/F4B-SMOKE-PB/prep/` y `run/`;
- `I-64-pilot/F4B-PBT5-SMOKE-PB-GATE-CLOSE/coordinator/contract.md`, la revisión independiente de la evidencia y su encargo, y el paquete de cierre;
- `F4B-DRAFTS/pbt5/`: las versiones del paquete y de las preguntas, las revisiones, [SR] redactada, las guías de la corrida previa como historia, los traspasos y las herramientas.
  - Las `_tools/*.local.txt` no se custodian.
  - Lo que case un patrón prohibido tras la redacción va solo por su sha256 ([DISP] F4B-E60).
- `F4B-DRAFTS/coordinator/owner-pbt5-decisions.md` y, después, la disposición y la firma del Owner;
- `F4B-DRAFTS/gate-close.md`, con la tabla de los INV;
- F4B-E62 (la emisión), la §292 y las entradas y secciones siguientes; los cambios de `I-64.yml`.

### 6.6 El presupuesto (Q-PBT5-9, «Definición y reglas»)

- **1 de 3** (OB-10.1); el intento 3, siempre al Owner (OB-10.2).
- **Un intento es una corrida del Smoke-PB sobre un Candidato.**
- **No cuentan:**
  - repetir un paso por error del operador (dos veces como máximo);
  - una parada del entorno antes de abrir AutoCAD;
  - una adenda que pida el Owner al revisar.
- **Se adoptan las tres reglas de Smoke-2:**
  - las etiquetas: NOT AVAILABLE FOR PHYSICAL SMOKE; NO VERIFICABLE FÍSICAMENTE / CUBIERTO POR PRUEBA AUTOMATIZADA; OBSERVADO;
  - traer la evidencia antes de decidir un OBSERVADO;
  - una copia de trabajo por escenario.
- **Una tarea de corrección** que nazca de un FAIL tiene su propio contrato y su 1 de 3.

### 6.7 La cadena

1. **Esta v2.3 y su comprobación acotada,** que tiene que dar 0 bloqueantes y 0 importantes sin resolver. Su resultado va en una línea `BOUNDED_RESULT:`, que lee el guion.
2. **El cierre de PB-T4e,** F4B-E61 y §291 **[T4e]**. Su SHA es el Candidato.
3. **`emit_pbt5.py --dry`,** que corre la sesión. Comprueba:
   - la numeración: existen F4B-E61 y la §291, y no existen F4B-E62 ni la §292;
   - el Candidato, su árbol y la CI exacta en verde;
   - la línea `BOUNDED_RESULT:`;
   - los sha256 de lo que se custodia y la redacción.

   Escribe su listado.
4. **La preparación y el build en el Candidato** ([SR] §3 y §4.1 a §4.3).
5. **`emit_pbt5.py --commit`,** que corre la sesión:
   - compara con el listado del `--dry`;
   - copia con la redacción;
   - saca el contrato (§6) a `contract.md`;
   - añade F4B-E62 y la §292;
   - cambia `I-64.yml`;
   - y hace el commit documental y lo sube, sin forzar.

   **Es la emisión,** por la autorización condicional del Owner (Q-PBT5-7).
6. **El Smoke-PB contratado** ([SR]), con la frontera de §5.4.
7. **La custodia** de la corrida, redactada, en un commit documental de la sesión.
8. **La revisión independiente de la evidencia** (Q-PBT5-8), en solo lectura.
9. **La disposición del Owner** (los S+O, los OBSERVADO, SPB-07 si toca) **y su firma.**
10. **El gate-close** (§3.3).

---

## 7. Riesgos

Son los de la v2.2 (§7), con estos cambios:
- **R-1:** la corrida es larga (65 pasos y 44 copias, más las de P-09). La frontera de las 13:00 UTC la corta, y se sigue con el Owner. Los bloques son independientes, y el orden de [SR] deja para el final las inyecciones, que necesitan sus propios AutoCAD.
- **R-15, nuevo: una variable de depuración que se escapa del proceso de su inyección.** Se lanza con `ProcessStartInfo.Environment`, y antes y después se comprueban el usuario, la máquina y el shell. Ninguna otra instancia de la sesión puede estar viva al lanzarla.
- **R-16, nuevo: un contexto construido que se toma por real.** El rótulo, la gemela clásica y la línea entre preparar y construir (§5.6).
- **R-17, nuevo: recompilar el DLL después del commit de la emisión.** Cambiaría la `ProductVersion`, y el bloque 7 de [SR] lo detectaría. El build va antes del commit (§3.1).

---

## 8. Supuestos declarados

- **S-1:** la frontera de esta noche es la lectura de la sesión principal del mensaje del Owner (§5.4). El Owner no la confirmó, porque Q-PBT5-5 no se le preguntó.
- **S-3:** el Candidato es la punta del cierre de PB-T4e, desde la que se compila el DLL, y el commit de la emisión va encima (§3.1).
- **S-4 a S-7, S-10 y S-11:** como en la v2.2 (§8). S-7, una copia por escenario, deja de ser un supuesto: el Owner la adoptó (Q-PBT5-9).
- **S-12, nuevo:** la emisión de PB-T5 es un solo commit documental, F4B-E62 y la §292. Lleva el paquete, el registro del Owner, el contrato como documento y la historia. Las tareas de F4B con `gate-contract.json` emitían en dos commits (X y BASE), pero sin contrato JSON no hay BASE que generar.

---

## 9. Las decisiones del Owner, y lo que le queda

- **Decididas** ([OD5]): Q-PBT5-7, Q-PBT5-8, Q-PBT5-9, Q-PBT5-1, Q-PBT5-10, Q-PBT5-4 y Q-PBT5-2.
- **No preguntada:** Q-PBT5-5. La corrida previa no se hizo.
- **Lo que le queda al Owner en PB-T5:**
  - los 9 juicios S+O;
  - SPB-07 si ningún dibujo guarda un estilo;
  - los OBSERVADO;
  - una decisión nueva, una prueba que falle o un intento 3;
  - la disposición, después de la revisión de la evidencia;
  - la firma.
- **Información, como en la v2.2:**
  - D-PB-11 es obligatorio;
  - H-31 y OWNER-1 de PB-TC2 los añade la sesión;
  - R-T4-39.

---

## 10. Lo no verificado

- **El cierre de PB-T4e** **[T4e]**.
- **Sin AutoCAD, sin verificar:**
  - `vla-item` por nombre;
  - `rcdyn` y `rcdynchk`;
  - `WaitForInputIdle`;
  - que `StartTime` esté disponible justo después de `Start`;
  - las órdenes nativas de los contextos construidos (`-LAYOUT`, `-INSERT`, `-BLOCK`, `-LAYER` y `-PURGE`) con sus respuestas;
  - que `QUIT` cierre limpio la instancia de cada inyección.
- **Si se llega a «Conservar»** (R-2), si se alcanza escenificar un módulo (SPB-21), si hay un rack real con un diagnóstico bloqueante (SPB-56) y si la copia enlazada de D1 sigue teniendo el mismo texto (SPB-63).
- **Los originales** de D1 y de los demás dibujos de la selección de Smoke-2: dónde están hoy y sus SHA-256.
- **Que la herramienta deje correr en segundo plano** las órdenes de más de 10 minutos.
- **La duración real** de la corrida.
- **El guion de la emisión** no se ha ejecutado, ni en `--dry`.
