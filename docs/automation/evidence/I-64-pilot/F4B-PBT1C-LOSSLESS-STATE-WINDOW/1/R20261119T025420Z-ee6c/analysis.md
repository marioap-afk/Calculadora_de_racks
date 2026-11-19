# I-64 F4B PB-T1c — Análisis del Coordinator para la corrección del intento 2

**Fecha:** 2026-11-19. **Tarea:** `F4B-PBT1C-LOSSLESS-STATE-WINDOW`. **Lo que corrige:** la entrega del Worker `R20261119T025420Z-ee6c` (delegación `R20261119T025114Z-43f7`, intento 1). **FailureClass:** `StopCondition` (PBT1C-C07). **Presupuesto:** la corrección consume intento (16.8: un STOP cuya resolución cambia el trabajo). PB-T1c pasa al **intento 2 de 3**; el intento 3, si llegara, vuelve al Owner (PBT1C-C12).

## 1. Lo que pasó

**El RED `e5a2198945b0a5e3db18c46b91c64dd83e88d69c`** es conforme:
- **El producto:** la costura inerte en W, en una línea, y las tres clases nuevas.
- **CI `44450941610`:** tests y ui-tests en failure, build-ui en success y build-plugin omitido.
- **Lo que falla:** solo CT-PBT1C-03 (Core) y CT-PBT1C-01 y 02 (UI), todos por aserción. CT-PBT1C-04 pasa, con sus dieciséis autopruebas.

**La implementación** de D-PBT1-13 se escribió y se validó sobre el árbol, **sin commit**.
- CT-01 pasa, con 58 de 58 ventanas en `iguales=True`. También pasan CT-03, CT-04 y el script de invariantes del Worker.
- **CT-02 falla en tres gestos de la parte compuesta**: `GapBox=60`, `CentralSeparatorCheck` y `StructureOverrideBox=6`.

**La causa** está en cómo se escribió CT-02. La parte 1 (`<gesto> valido= iguales= nombre-igual=`) toma la comparación justo después del gesto. La comparación es: el diseño calculado desde una copia del estado de la ventana frente a `LastComputation.Design` persistido. El recálculo neutro (`Recalcular(w)`) solo viene después, antes de las huellas de la parte 2.
- **Comportamiento que ya existía, el mismo en la BaseSha:** en la ventana, la secuencia de módulos de `LastComputation` del compuesto se reconverge en la pasada **siguiente**. Por eso, tras esos tres gestos, el diseño persistido va una pasada por detrás del estado:
  - `Modules[8].Length` de 54 a 60;
  - `Modules[8].Kind` de Gap a Separator;
  - `PostPeralte` de 0 a 3.
- **Lo que mostró el diagnóstico:** con la toma de la comparación movida a después del recálculo neutro (variante `t3`, temporal y restaurada), CT-02 pasa entera sobre la implementación.
- **Lo que hizo el Worker:** no ajustó la expectativa ni compensó en el producto. 16.8 impide tocar `tests/` tras el RED. Paró con PBT1C-C07, como pide el contrato.

**No es un cambio de RACKEDITAR ni un defecto de la implementación.** El §5.4 del paquete v2.8 pide que, para cada control del censo y su gesto, el diseño calculado desde una copia del estado sea igual, persistido, a `LastComputation.Design`, pero no fija en qué momento. Comparar tras un recálculo neutro cumple la letra y el propósito de la fila: ningún control es la fuente de verdad. La costura inerte sigue sin poder pasarla, porque una copia de un estado nuevo y vacío da otro diseño.

## 2. La corrección (VINCULANTE para la delegación de corrección)

**C-1. CT-PBT1C-02, parte 1.** En `tests/RackCad.UI.Tests/PushBackWindowOnStateTests.cs`, la línea `<gesto> valido=<v> iguales=<d> nombre-igual=<n>` de cada gesto comparable se toma **después** del recálculo neutro que ya hace la prueba (`Recalcular(w)`), y no antes.
- Igual para todos los gestos: no se hace excepción por gesto.
- Las líneas esperadas no cambian (`valido=True iguales=True nombre-igual=True`).
- La parte 2 (`<gesto> huellas-iguales=True descartes=<n>`) tampoco cambia: se toma como hoy, tras ese mismo recálculo.
- Nada más de la clase cambia.

**C-2. Nada más en `tests/`.** CT-PBT1C-01, CT-PBT1C-03 y CT-PBT1C-04 quedan exactamente como en `e5a21989`.

**C-3. El GREEN** es D-PBT1-13, (1) a (9), solo en W y WC, con todos los invariantes del contrato.
- **El punto de partida:** el Worker puede partir del parche del intento 1, `.../1/R20261119T025420Z-ee6c/green-wip-ct02-bloqueado.patch`, validado sin commit. No es una autoridad.
- **Lo que vuelve a comprobar:**
  - INV-PBT1C-01 y 03 a 07c, con rangos, C13 por nombre, censo y testigos;
  - todos los filtros;
  - las huellas de CT-PBT1A-14, tres veces;
  - la CI exacta.

## 3. El régimen de la cadena (16.8; M-11)

**La base y la cadena:**
- **BaseSha:** el commit de la sesión que custodia este análisis. Contiene el RED `e5a21989` del intento 1.
- **ChainBaseSha:** `833ea6bfaadfab9afc698aba7f12ad2124a78585`, la BaseSha de la primera delegación.
- **ChainRedSha = null, ChainRedFiles = []:** ninguna verificación acreditó el RED `e5a21989`, porque el intento 1 paró antes del GREEN. Es el precedente de F4-T4C2, intento 2.
- **La entrega exige RED.**

**El RED de la corrección:**
- Solo la clase corregida, `tests/RackCad.UI.Tests/PushBackWindowOnStateTests.cs`. En `src/` no cambia nada: W ya tiene la costura inerte desde `e5a21989`.
- **Su CI:** tests (CT-03) y ui-tests (CT-01 y 02) en failure, build-ui en success y build-plugin omitido.
- **El criterio mecánico de INV-PBT1C-02** se cumple en cada filtro con ExpectRed.
- **RT** = `git diff --name-only ChainBaseSha RedSha -- tests/`, es decir, las tres clases de PB-T1c.

**El GREEN de la corrección:**
- Solo W y WC; ningún archivo de `tests/`.
- `git diff --name-only BaseSha RedSha` contiene solo esa clase de prueba.
- `git diff --name-only RedSha CurrentSha` contiene solo W y WC.

**Las autoridades (16.3 con F4B-E25).**
- `git diff AuthorityRevision BaseSha` contiene W, que es UNIT_CHANGE. Solo lo toca el RED `e5a21989`, que es el RedSha de la entrega del intento 1 de esta misma cadena y cae dentro de su AllowedWriteScope.
- Ningún commit de la sesión toca W.
- La verificación lo comprueba commit a commit con la entrega y la delegación del intento 1.

## 4. Lo que no se corrige (declarado)

**El desfase de una pasada** del diseño de `LastComputation` del compuesto tras GapBox, el separador central y la estructura propia ya existía en la base.
- No se toca: el modo CLASSIC no cambia.
- Queda como hecho caracterizado. «Actualizar», el BOM y la biblioteca recalculan antes de usar el diseño.
- Se anota para la revisión de PB-T2 (el modo WORKSPACE y «Aplicar al borrador»).

**El archivo de disposición del configurador.** Pruebas de UI que ya existían crean un `RackFrameConfiguratorWindow` real, y su `OnClosed` llama a `RackFrameConfiguratorLayoutStore`, que escribe `RackCad/RackFrameConfigurator.layout` bajo los datos locales del usuario. Son, entre otras, las clases RichEditor*, EditorShellAdoptionTests, FirstView* y PushBackHeaderOwnerRoundTwoTests, dentro de los filtros `RichEditor` y `WindowCensusGuardTests`.
- Por eso la prueba de limpieza del perfil cambia en toda corrida local de esos filtros.
- **No es una parada de esta tarea.** PBT1C-C09 y INV-PBT1C-10 rigen las pruebas nuevas, y las tres de PB-T1c no abren el configurador. PB-T1c tampoco puede cambiar las pruebas existentes.
- **Lo que hace la corrección:**
  - registra la prueba de limpieza;
  - si solo cambia ese archivo, lo atribuye a esos filtros, sin parar.
  - Cualquier otro cambio del perfil sí es parada.
- **Lo que queda pendiente:** se informa al Owner y se anota en `docs/ideas-futuras.md` como defecto de aislamiento de pruebas anterior a F4B, para una tarea propia.

## 5. Lo que no cambia

- El contrato (v2.8, AuthorityRevision `af46fcd5`), sus invariantes, sus RequiredTests y sus paradas.
- D2 y su límite.
- El modo CLASSIC sin cambio observable.
- Ninguna prueba ni ayudante existente cambia.
