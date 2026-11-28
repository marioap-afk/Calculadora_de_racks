# Notas de redacción de I-64 A-14 (borrador v1)

Archivo de trabajo de la sesión, fuera del repo. Acompaña a `I-64-A-14-draft-v1.md` de esta misma carpeta. Puede llevar rutas
locales; la enmienda no.

## 1. Estado de W al leer

- W = worktree de I-64, rama `architecture/workspace-persistente-rackcad`, HEAD `fa59bd82acff7ce276a1605c60d426c45a3e3b8d`
  («I-64 F4B PB-T5: paquete v2.4, las decisiones del Owner, el contrato en forma B y la emision (F4B-E62)»).
- `git status --porcelain` vacío antes y después. Solo lectura: Read, Grep, `git -C <W> rev-parse / log / show / cat-file /
  merge-base --is-ancestor`. Ninguna orden git que cambie estado; nada escrito en W ni en el checkout principal.
- Sin AutoCAD, sin compilar, sin pruebas. No hizo falta Python.

## 2. Fuentes leídas (blob en HEAD de W, salvo que se diga)

| Fuente | Blob / SHA-256 | Lectura |
|---|---|---|
| `docs/initiatives/I-64-proposal-v5.md` | `dc1924ff5a73a4e529fbc5f995621d29351a091b` (igual en HEAD y en `9b43dafb`) | entera (1225 líneas) |
| `docs/initiatives/I-64-A-13.md` | `c38560d10b67776628c60004e0b58b60506adddc` | entera (modelo de cabecera) |
| `docs/initiatives/I-64-A-7.md` | `f8c3d6cde7c2bb9bdfa3024d8a964309638e0b9c` | entera (única A-n previa sobre comportamiento de producto) |
| `I-64-A-1.md` … `I-64-A-12.md` | A-1 `fb6a49c2`, A-2 `de6f6088`, A-3 `e14935bc`, A-4 `d587403e`, A-5 `a61dc9d7`, A-6 `725e9666`, A-8 `65da0262`, A-9 `40ec7a07`, A-10 `68ba54c4`, A-11 `825333bd`, A-12 `54810d4f` | solo el bloque de cabecera (`text`) |
| `docs/INITIATIVE_LIFECYCLE.md` | `6415c33bd2f86fa05444e00b630d7fed5e5d7a22` | entero (§3, §5, §6, §7, §8 son las reglas de A-n) |
| `docs/WORKFLOW.md` | `884ed305ce242bda575cef74a9eff8c28ce7b67c` | entero |
| `AGENTS.md` | `dd0d5a2d9ac3d3d8502ef7a556c8a5f1d62c9c29` | búsqueda (Freeze/A-n `:94`, `:338`; ajustes de usuario `:342-343`) |
| `docs/initiatives/I-64-owner-brief.txt` | `8970c15e228adf94c96a98fd3f577437ab66dab0` | `:40-489` |
| `docs/automation/state/I-64.yml` | `d198c1b86aaf151cd1df79095779a654a12d4620` | entero (líneas largas truncadas al leer; f4, f4b, smoke1, A-n, codex_environment leídos enteros) |
| `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md` | `3c81f607a09de83ff455e15a9ad990479f45b8bc` | F4B-E01..E03, F4B-E57, búsquedas |
| `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-disposition.md` | `aaa0e83716ba269270463fb1c6e9d90511e20009` | entera ([OD]) |
| `docs/initiatives/I-64-f4b-push-back-completo-package.md` | `34b6a0c1729a078dbe7f4aa253a7871d24c4f0fb` | `:1-205`, `:400-519` |
| `docs/initiatives/I-64-implementation-clarifications.md` | `69efc1993cbcceedf9250684ed2b98c99336d90c` | entera (IC-01..IC-11) |
| `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt5/I-64-f4b-pbt5-package-v2.4.md` | `4248e6f1590ef6e790c4c8f956591bea5a5c12dd` | búsqueda SPB-12, 13, 23 |
| `docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md` | `7f866795877cf93d3ca2e70baa9d03bc00914072` | D7..D13 |
| `docs/adr/README.md` | `bdf23f0784a69550fd8e4a9d194095845035441b` | `:37` (un propuesto se edita libremente) |
| `docs/adr/0047-workspace-persistente-modeless-rackcad.md` | `5322fa05455c6c5c6a191949fdf15aa1d4bd1f95` | cabecera (propuesto) |
| `src/RackCad.Plugin/RackMenuCommands.cs` | `2660980025a2089fde43f4689efd588100efa041` | `:1-60` (RACKCAD y RK abren el menú modal) |
| `src/RackCad.UI/RackMainMenuWindow.xaml` | `e5d54518eb6a4b3bbbad291bff2dd8b7bbc6adad` | rótulos del menú |
| `src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs` | `a003511bde419b2ebdc94f84b9e029bd207eaeba` | `new PaletteSet("RackCad")` |
| `src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs` | `541a8d5413ac824d80e7041e2a4b89d48b7d29f0` | RACKPANEL y RACKPANELDESCARTAR |
| `src/RackCad.Application/Settings/UserSettings.cs` | `b9d31b92a756e5ccf5135ef9a4b9d984d6e0eebd` | existencia del almacén de ajustes |
| `docs/automation/evidence/I-64-pilot/F1-GATE/A-7/coordinator-a7-review-order.md` | `4c0b5c7d130ba9260a2e4241bfb48f21034bd05b` | orden de revisión del Architect de A-7 |
| Registros de revisión del Architect de A-7 (`R20261011T202537Z-1990`) y A-13 (`R20261110T022847Z-bc2b`) | `review-record.json` | cabeceras |
| S/`replaneacion-i64.html` | SHA-256 `79175286541f67b9538920c177f06d37dea6d672ce748ba41d32edbdc47e8a4f` | §1-§7 entero |
| S/`owner-a14-decisions.md` | SHA-256 `c6828e48056973a12bed6007865b59a30db490a5b6bf59ce9cd46ac4096e27a7` (versión con RACKPANEL decidido) | entero, dos veces |
| S/../proto-rackcad/project/`Main.dc.html` | SHA-256 `0d0ee1198ae441e0b1eb3324965fea96cd1c389e4ed8797a59d43205ad67a097` | texto visible y estado inicial |
| S/../proto-rackcad/project/`canvas.json` | SHA-256 `5baf8529df57fd445e58e3e394aeca3cbf0ef9f42f7d5066e37d8c1eecb6c6d7` | entero |
| S/`a14-drafting-brief.md` | SHA-256 `d5e77580d35b604e8f18d69f58ca89475b704ad0ef28070387ad08f75f4e7ecd` | entero |

**El Freeze:** `git -C <W> rev-parse HEAD:docs/initiatives/I-64-proposal-v5.md` = `dc1924ff…` = el blob del encargo; no hizo falta
`cat-file`. Pero `9b43dafb` **no** es ascendiente de HEAD, por los rebases (`f1_rebase`, `f1_rebase_i65`). Su imagen en la rama
actual es `55e7ca6337fef6dcc14f5c40bdd55daf88d78cbf` («I-64 Freeze: congela la Proposal V5 acordada»), con el mismo blob. La
cabecera de A-14 lo dice entre paréntesis, como hizo A-7 con la imagen de entonces (`6f845087`).

## 3. Cada PENDING y por qué

| PENDING | Por qué |
|---|---|
| Firma del Owner del texto exacto de A-14 | El encargo: las decisiones están tomadas y la firma no. Como en A-9..A-13, se pide tras Architect AGREED |
| Architect: revisión, celda y orden | Sin orden de revisión. La celda de A-9..A-13 (gpt-6.1-sol / high / read-only, codex-cli 0.167.0-alpha.3.2, sonda `R20261109T062522Z-e8be`, binario SHA-256 `4B3286E3…`, config Snapshot D) se midió sobre un binario y una config que ya no rigen: hoy rige la Snapshot L y el binario acreditado es `4F91AE86…`, solo para gpt-6-luna / high / read-only (F4B-E57). La orden de A-7 decía «If CLI/config/model eligibility changed: STOP Coordinator before invocation» |
| Coordinator: materialidad, autoridad, orden y AGREED | Nada emitido. La autoridad y la materialidad son propuesta del borrador, como en A-7 («Authority (propuesta de la sesión; la decide el Coordinator)») |
| Ruta en el repo de las decisiones del Owner, de la replaneación y del prototipo (Q-11) | Hoy solo existen en la carpeta de trabajo, fuera del repo. WORKFLOW §10 (`:398`) da `docs/automation/decisions/<I>.md`, que no existe para I-64; I-64 ha guardado las decisiones del Owner en sus carpetas de evidencia (`F4B-DRAFTS/coordinator/owner-*.md`) |
| La sección de `I-64-evidence.md` que registre A-14 | No existe todavía |
| IDs INV-A14-01..11 y OV-A14-01..07 | Provisionales. LIFECYCLE §6 pide obligaciones invariante→prueba con RED esperado para lo congelado; la numeración la fija el Coordinator |
| Valor inicial de «Seguir la selección» | El prototipo lo trae encendido (`follow: true`), pero el prototipo no es especificación y nadie lo decidió: lo fija el paquete de la fase 2 |
| Fase de las variables en «Actualizar todo» (Q-02) | Contradicción K-05 |
| Fase 8 (Q-04) | La replaneación no trae decisión sobre Insertar, identidad, vistas ni nombre |
| Presupuesto por tarea de las fases nuevas | Ninguna fuente lo da; D-PB-10 es solo de F4B |

## 4. Supuestos y decisiones de redacción derivadas (para que las mire el Architect)

- **RACKPANEL:** ya **no** es supuesto. Mensaje del Coordinator a mitad del encargo, y S/`owner-a14-decisions.md` actualizado con la
  respuesta literal del Owner «RACKPANEL puede quedarse por ahora». En A-14 va como decisión 5 (§2) y en C-06; no figura en supuestos
  ni en preguntas (Q-01 ya no lo nombra).
- **C-08, la vía de los ajustes de usuario de RackCad.** El encargo pide decir que D-01 sigue igual, pero D-01 `:122` prohíbe pedir a
  AutoCAD que persista el estado de la paleta. Para que las dos cosas valgan, el borrador fija que el tamaño y la posición se guardan en
  los ajustes de usuario de RackCad (`UserSettings`), y que la excepción recae en D-22 e INV-21. Es una elección de redacción, no del
  Owner: la otra vía (`toolID` y `Load`/`Save`) cambiaría D-01 `:122`, tocaría el perfil y volvería a abrir el acuse de I-52.
- **C-08 en la fase 1.** La replaneación no sitúa la persistencia en ninguna fase; el borrador la pone en la 1 y lo dice.
- **C-02, «Descartar cambios»** sustituye a «Cerrar pestaña» con dirty, siguiendo el prototipo. **D-06** (efectos de Uno(R)) y
  **D-03** (contenido de la sesión) entran como consecuencia de D-08; la tabla §3 de la replaneación no los nombra.
- **C-04:** la vía de D-13 extendida a N racks en **una** transacción de MUTATE es la lectura literal de «todo o nada, un solo
  Deshacer» (decisión del Owner). La mecánica no se congela.
- **C-06:** que la lista de retiro entre por su propia A-n se deduce de LIFECYCLE §6 (D-16 es congelado). La replaneación solo dice
  «fase propia con la lista que decidas».
- **C-07, fase 3:** hacer el punto 1 de D-15 para Push Back (separar la superficie de su `Window`) es la consecuencia técnica de
  alojar el editor; F4B separó aplicar, cerrar y escribir, pero dejó la `Window`.
- **Materialidad:** todas las M en YES es la lectura prudente (LIFECYCLE §3: ante duda, el superior; UNKNOWN cuenta como activado).
- **Autoridad de C-07:** la tabla §3 de la replaneación pone Architect + Coordinator; el borrador añade al Owner (K-02).

## 5. Contradicciones encontradas

| ID | Entre | Qué |
|---|---|---|
| K-01 | Encargo («D-01 sigue igual») · decisión 2 del Owner · V5 D-01 `:122`, D-22 `:702-703`, INV-21 | Recordar la posición choca con la persistencia que prohíben D-01 y D-22. Se resuelve en C-08 dejando D-01 literal y cambiando D-22 e INV-21 (§4) |
| K-02 | Replaneación §3 (D-15 y D-PB-01: «Decide Architect, Coordinator») · [OD] | D-PB-01 y «Do not try to fit the current ~1120 px editor inside the ~320 px panel» (OD `:37`) son texto literal del Owner: cambiarlos pide también al Owner. La aprobación del §3 lo cubre, pero la firma debe nombrarlo |
| K-03 | Replaneación §3 («D-15: Editores "editables en el panel"») · V5 | La frase literal está en §11 (`:793-795`), no en D-15; D-15 punto 1 (`:562-563`) define la superficie alojable. Hecho clave del encargo confirmado, con la cita corregida |
| K-04 | V5 D-15 punto 1 · F4B D-PB-01 / OB-01.4 · IC-11 | El paquete de F4B cita D-15 punto 1 como autoridad vinculante (PKG `:60`) y a la vez mantiene la ventana modal; manda [OD] (PKG `:59`). Ni el paquete ni IC-11 declaran esa diferencia |
| K-05 | Replaneación §4: fase 4 (escribe «racks y variables con cambios», depende solo de la 3) · fase 5 (Variables, depende de la 1) | Los borradores de variables solo existen cuando la sección Variables está en la ventana (fase 5); la 4 no depende de la 5. Q-02 |
| K-06 | Decisiones «Diseñar antes de migrar» y «RACKEDITAR se queda» · V5 D-15 punto 1 · OD `:36` · ADR-0029 D11/D13 · INV-19 y el STOP de D-16 | Si RACKEDITAR conserva la ventana de hoy, cada sistema rediseñado tiene dos superficies (D-15 dice «la misma superficie»; [OD]: «Do not build a second full Push Back editor»). Si RACKEDITAR adopta el diseño nuevo, cambia un comando clásico (M-03, INV-19). La replaneación no lo resuelve. Q-03 |
| K-07 | Replaneación fase 8 («Nuevo, sin insertar» e insertar desde la ventana) · V5 no-objetivo `:75-76`, D-23 `:711`, D-22 `:704`, INV-16 · D-PB-04 (OD `:76-78`) | La tabla §3 de la replaneación no lista el no-objetivo de Insertar ni la identidad. Q-04 |
| K-08 | Prototipo («Planta ejemplo.dwg · 70 racks»; «Racks del dibujo (70)» en BOM y lista) · V5 D-05 `:284-295`, INV-36 | Un recuento de proyecto en la cabecera; el índice no puede alimentar BOM ni listas, y sus rótulos son de navegación. R-10 |
| K-09 | Replaneación §5 («ya vimos el problema de Ctrl+V en Smoke-1») · `I-64.yml` (`smoke1_status`, §72) | El Coordinator lo clasificó como OPERATOR_INPUT_INCIDENT, no como defecto de producto. A-14 (R-02) lo dice así |
| K-10 | [OD] `:269`, `:292-293` («F5-A remains authorized but WAITING») · replaneación (la fase 6 sustituye a F5-A, con paquete y orden propios) | Q-05 |
| K-11 | Replaneación §2 («SPB-12 y SPB-13 … se registrarán como comportamiento provisional») · encargo («no cambia el contrato de PB-T5 ni el Smoke-PB») | El registro no puede ir dentro de PB-T5; A-14 lo hace desde fuera (C-07): esos SPB certifican el paso intermedio y no se reetiquetan |
| K-12 | Brief (`:126-132` pestañas; `:475` «global batch Apply unless explicitly frozen»; `:371` restauración entre arranques) · decisiones del Owner | No es contradicción interna: el Owner cambia su propio brief. Por eso C-02, C-04 y C-08 son `OWNER-RESERVED` |
| K-13 | Replaneación (§2 «unos 45 de 65 pasos»; §5 «de 9 a 13 s (SPB-23)») · W | No verificable en W: los resultados del Smoke-PB en curso no están versionados (`F4B-PBT5-SMOKE-PB-GATE-CLOSE/` solo tiene `coordinator/contract.md`). A-14 no da esas cifras como hechos (R-04) |
| K-14 | `INITIATIVE_LIFECYCLE.md:3` y `WORKFLOW.md:9-12` (Workflow V2 «not effective») · V5 cabecera («Workflow: V2 (T4)») | Las A-n previas (A-7) aplicaron LIFECYCLE §6 y §8; A-14 sigue ese precedente. Observación, no bloquea |
| K-15 | Replaneación §4 (fase 9 depende de «5 a 8»; F7 de «1 a 9») · la fase 3b | Ambiguo si 3b entra en esos rangos; el orden la pone antes de la 8. El borrador hace que F7 dependa también de la 3b |

## 6. Cómo se invocó al Architect en las enmiendas anteriores de I-64

- **Orden:** siempre una orden del Coordinator, transcrita literal, que pide **exactamente una** revisión formal del objeto exacto
  (commit) con modelo, effort y modo, y STOP al Coordinator antes de invocar si cambió la elegibilidad. Ejemplo:
  `docs/automation/evidence/I-64-pilot/F1-GATE/A-7/coordinator-a7-review-order.md`. En A-12 y A-13, `coordinator-a1x-*-order.md`.
- **Celda:** gpt-6.1-sol / high / read-only.
  - A-7: codex-cli 0.162.0-alpha.17.2, sonda `R20261011T030925Z-a198`, config Snapshot C;
  - A-8..A-13: codex-cli 0.167.0-alpha.3.2, sonda `R20261109T062522Z-e8be`, config Snapshot D.
- **Transporte:** codex-cli lanzado con `-s read-only -m gpt-6.1-sol -c model_reasoning_effort="high" -c allow_login_shell=false`,
  en una cesión con hora de inicio y de fin (`SessionOperated: false`), con fotos de git, config y procesos a la entrada y a la salida,
  y el prompt y el esquema de salida custodiados.
- **Dónde quedan las revisiones:**
  - A-2..A-6 y A-8..A-12: `docs/automation/evidence/I-64-protocol-bridge/architect-review/<RunId>/` (A-1 tuvo una revisión
    de conformidad independiente, `R20261010T012851Z-de0d`, no del Architect);
  - A-7: `docs/automation/evidence/I-64-pilot/F1-GATE/A-7/architect-review/R20261011T202537Z-1990/`;
  - A-13: `docs/automation/evidence/I-64-pilot/A-13-review/R20261110T022847Z-bc2b/`.
  Cada una con `prompt.md`, `output.json` o `architect-review.json`, `review-record.json` (esquema
  `rackcad-i64-architect-review-record/v1`), `custody-manifest.json` y las fotos de entrada y salida.
- **Revisión independiente de paquetes en F4B** (no es el Architect): `F4B-DRAFTS/package/independent-review-v2.md` (IR2) e
  `independent-review-v3-check.md` (IR3), de solo lectura sobre un SHA fijado.
- **Para A-14:** la celda acreditada se midió sobre otro binario y otra config (§3). Lo previsible es una sonda nueva o una decisión
  del Coordinator antes de invocar.

## 7. Lo que no se verificó

- A-1..A-12 completas: solo sus cabeceras (el encargo pedía formato de cabecera y «Lo que A-n no hace»; A-13 y A-7 se leyeron
  enteras).
- El Discovery de I-64 y los paquetes del Architect V1..V5: no se leyeron.
- Los resultados en curso del Smoke-PB (K-13).
- Si AutoCAD conserva por su cuenta el estado de una paleta sin `toolID`: V5 lo dejó `UNKNOWN`; Smoke-1 dio DIFF CLEAN y
  `PaletteKeyAcceptance = NOT_RECORDED / NOT_NEEDED_FOR_DIFF_DISPOSITION`.
- La redacción final de cada sección del área central del prototipo: el prototipo es solo descriptivo.

## 8. Privacidad

- La enmienda no lleva rutas locales, nombres, correos ni datos de clientes (comprobado con búsqueda: solo rutas relativas del repo).
- El prototipo trae en Configuración rutas de ejemplo con forma de ruta local de disco (`cfg.blocks`, `cfg.designs`,
  `cfg.catalogs`). Si se versiona, hay que redactarlas antes (está en el paso 6 del camino de aprobación de A-14).

## 9. Lo que debe volver al Owner (v1; la v2 lo sustituye, ver §10)

1. Firmar el texto, nombrando lo `OWNER-RESERVED`: C-01, C-02, C-04, C-05, C-06, C-08 y el texto de D-PB-01 en C-07 (K-02).
2. Q-03, RACKEDITAR tras el rediseño (K-06): es la que más condiciona el plan.
3. Q-02, las variables en «Actualizar todo» (K-05).
4. Q-04, la fase 8 (K-07).
5. Q-05, la autorización vigente de F5-A (K-10).
6. Q-06, la sustitución de OV-08, OV-09 y OV-14.
7. Q-07 (alias RK y el menú modal), Q-08 («Abrir RackCad al iniciar AutoCAD») y Q-09 (integración parcial).

## 10. La v2 (2026-11-23)

### 10.1 Entradas nuevas

| Entrada | SHA-256 / blob |
|---|---|
| S/`a14-v2-instructions.md` | `f97b7c36947cd9a4b75a39a2f4262e816f9a735dd9c8025568a19dd9aa5ee14d` |
| S/`independent-review-a14-v1.md` | `d3a13ef00d676067be9d5e98a173ea3c585c477341efacdf16a8afae5a3822b4` (sobre el v1 `7571e165…2dfb`) |
| S/`owner-a14-decisions.md`, con la segunda y la tercera ronda | `f1d0ea5de4ab858a124edf0515b12d04624244a36fd3b6cd291d849013654c2e`. La revisión leyó una versión anterior, `22ab78b4…94ed`, con solo la segunda ronda; la tercera (Q-02, Q-10) llegó después de la revisión, que la recomendaba |
| W `docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md` | blob `0329ab5364204848e689d410d7a4e625f9bef5ea` (patrón SUS-nn: pruebas viejas intactas con `Skip` y registro; pruebas nuevas aparte) |
| W `src/RackCad.Application/Views/Redraw/RackSiblingRedrawRun.cs` | blob `a22ea4eb133bd0082d77e3bd408677e3ee9a01d3` (`Execute` por rack, `:30-32`) |
| W `src/RackCad.UI/RackMainMenuWindow.xaml.cs` | blob `19e010410e5ec520f981f6285373707fd414a74b` (`OpenFromLibrary` devuelve un `InsertionRequest`) |
| W `docs/ROADMAP.md` | blob `80138faa4487a89c4aca1ea63d1b2f0c1df27317` (filas I-52-AUTH15, I-53S e I-53D: precedentes de unidades propias) |
| W, censo de `CommandMethod` de `src/RackCad.Plugin` | comprobado entero (comprueba I-10) |
| W, V5 | revisadas además todas las líneas que la revisión cita y todas las que nombran «pestaña» o «preview» (base de la lista cerrada de C-03) |

W sigue en `fa59bd82` con el árbol limpio. Ninguna escritura en W.

### 10.2 Salidas

- `I-64-A-14-draft-v2.md`: 859 líneas, enmienda completa. Sin rutas locales, nombres ni correos (comprobado con búsqueda).
- `a14-v2-disposition.md`: los 29 hallazgos, 29 aplicados, 0 rechazados (3 con ajuste).

### 10.3 Decisiones de redacción nuevas en la v2 (para que las miren el Coordinator y el Architect)

1. **Cinco tramos** como PROPUESTA (C-12): 1 = fases 0 a 3, el ejemplo del Owner; 2 = 4 y 5; 3 = 6; 4 = 7 y 3b; 5 = 8 y 9. Con la
   alternativa de llevar el tramo 1 hasta la fase 4.
2. **Unidades:** el tramo 1 es la unidad I-64 (su rama y su Claim-Id). Los siguientes son unidades nuevas, con reclamo, rama y worktree
   propios desde `origin/main`. Se deduce de WORKFLOW §2, §3 y §4.5 (la limpieza retira la rama) y de LIFECYCLE §2, con el precedente de
   I-53, I-53S e I-53D.
3. **Regla de lectura de «pestaña»** con una lista cerrada de líneas de V5 (C-03), en lugar de un delta por línea. Así ninguna mención
   queda como enmienda invisible.
4. **Vida del borrador Limpio:** se libera al dejar de tener abierto su rack (C-02, INV-A14-08). Es la traducción de «cerrar una pestaña
   limpia».
5. **Elegibilidad para «Actualizar todo»:** texto conservado, Conflicto, Desconocido o Error impiden la operación; «posiblemente obsoleto»
   entra (C-05). La regla sale de D-10, D-12, D-13 y D-08; el paquete de la fase 4 la demuestra.
6. **«Posición» incluye acoplada o flotante y el lado** (C-10), como lectura de «la ventana recuerda».
7. **Cuatro excepciones nombradas de D-22** (C-10): el rectángulo, los ajustes de Configuración, los archivos exportados y los diseños
   guardados en la biblioteca, si la sección lo ofrece.
8. **Variables dentro de la ventana con su propia A-n** (C-11), porque toca D-10, D-11, D-12, D-17 y ADR-0034. Hasta entonces, la sección
   abre la ventana de hoy.
9. **La fase 6 depende también de la 4**, porque sus sistemas entran en «Actualizar todo».
10. **Autoridad de la composición:** Architect + Coordinator en el paquete de la fase 4. Si hay que cambiar la costura, STOP, el Freeze de
    I-55 y, si choca, el Owner (C-05).
11. **Asignación de OV por tramo** (§5) y OV-A14-01..09 provisionales.
12. **La lista de retiro entra por su A-n e incluye la entrada de respaldo** (C-07).
13. **IC-03 queda sustituida por la decisión Q-10** (C-02). Las obligaciones de IC-02 e IC-05 que nombran pestañas se sustituyen, en la
    firma (C-03).
14. **D-07** pasa a «Amends» por su texto (la revisión no la nombraba).

### 10.4 PENDING de la v2

| PENDING | De quién |
|---|---|
| Firma del Owner (texto en §11 de la v2), incluida la confirmación o corrección de los tramos | Owner |
| Q-C1, dónde se versionan las fuentes, antes de la revisión del Architect | Coordinator |
| Q-C2, la celda del Architect (binario y configuración cambiaron desde A-13) | Coordinator |
| Q-C3, Smoke-PERF por tramo | Coordinator |
| Q-C4, el momento de aceptar ADR-0047 | Coordinator; lo acepta el Owner |
| Q-C5, los nombres de las unidades de los tramos 2 en adelante y la numeración de INV-A14 y OV-A14 | Coordinator |
| Q-C6, materialidad y autoridad | Coordinator |
| La revisión formal, en especial la composición de «Actualizar todo» y la vía de C-10 | Architect |
| El commit y el SHA de A-14, y su sección de evidencia | al versionar |
| Las A-n posteriores: la fase 8, Variables en la ventana, la lista de retiro | más adelante |

### 10.5 Contradicciones del v1, cómo quedan

| ID | Estado en la v2 |
|---|---|
| K-01 | Igual (C-10), con S1-11 repetido |
| K-02 | Resuelta: el Owner firma D-PB-01 y OD `:37` (§11) |
| K-03, K-04 | Igual, citadas en el §1 |
| K-05 | Resuelta por la decisión Q-02 |
| K-06 | Resuelta por la decisión Q-03 (C-09) |
| K-07 | A la A-n de la fase 8 (C-06) |
| K-08 | Tratada: C-11 e INV-A14-10 |
| K-09 | Igual (R-02) |
| K-10 | En la firma (§11) |
| K-11 | Igual (C-08) |
| K-12 | Explícito en C-01: las cláusulas del brief que A-14 cambia |
| K-13 | Igual (R-04) |
| K-14 | Igual, observación |
| K-15 | Resuelta: la fase 9 depende de la 3b y F7 va por tramo |
| Nueva | La revisión citaba el precedente de PB-T5 en `:14`; está en `:17` (M-14) |

## 11. La v2.1 (2026-11-23)

### 11.1 Entradas

| Entrada | SHA-256 |
|---|---|
| S/`a14-v21-instructions.md` | `f378885abe2bb5e4eee861e0f7a020e308030526662e298ca46f69ab0e8d122a` |
| S/`independent-review-a14-v2-check.md` (sobre la v2 `65e252e2…92e4`) | `48e0e0bf184b97ed1d13264dd99e7f620a36cbe8a593ba896619c75fed194f18` |
| W `docs/initiatives/I-63-proposal-v3-amendment-a1-verificacion.md` (y a2, a3, a4) | precedente de `Applies-to = all` (`:9`, `:10`, `:11`, `:9`), leído por Grep |
| W `src/RackCad.UI/RackMainMenuWindow.xaml:150-164` | la entrada «Biblioteca de bloques (.dwg)» (`BrowseLibrary_Click`) |
| W, V5 `:380`, `:659`, `:909`, `:982`; LIFECYCLE `:60-62` | comprobadas |

W sigue en `fa59bd82` con el árbol limpio. Una búsqueda recursiva de «Applies-to» en `docs/` superó el tiempo y se detuvo; la segunda,
acotada, dio el precedente.

### 11.2 Salidas

- `I-64-A-14-draft-v2.1.md`: 1028 líneas, SHA-256 `e4758ae9…5925` al cerrarse esta nota. Sin rutas locales, nombres ni correos
  (comprobado con búsqueda).
- `a14-v21-disposition.md`: 50 líneas; 20 filas (R2-02 y N-01..N-19), 20 aplicadas, 0 rechazadas.
- Herramienta auxiliar: `../tools-a14/splice_v21.py`, fuera de la carpeta de la enmienda. Sustituye §11 y §12 por anclas únicas y se
  ejecutó con `python -I`. El bloque temporal se borró después.

### 11.3 Decisiones nuevas de la v2.1 (también en §12 de la enmienda, con su autoridad)

1. **Smoke-PERF por tramo, valor por defecto:** §12.5 completo en el READY de cada tramo. Es la lectura conservadora de «antes de READY».
   Una reducción solo por la A-n siguiente, con el Coordinator y la conformidad del Owner.
2. **ADR-0047, valor por defecto:** sigue `propuesto`. El Owner decide en el READY de cada tramo, y las notas del anexo B solo recogen lo
   entregado.
3. **Corrección de hitos en la firma:** no impide activar A-14. La tabla resultante va por la A-n siguiente, y hasta entonces no se abre
   ningún tramo afectado.
4. **Rechazo de otra materia en la firma:** A-14 no se activa y la corrige la A-n siguiente (precedente A-12 → A-13). En un primer borrador
   puse «no se activa en esa materia» (activación parcial). Lo retiré por no tener fuente.
5. **N-10:** de las dos opciones, la excepción 2 de D-22 empieza en la fase 1 para la ruta de la biblioteca de bloques.
6. **IDs de INV-A14 y OV-A14, definitivos**, para que el texto congelado no dependa de una numeración posterior. Por eso la antigua Q-C5
   queda solo con los nombres de unidad, ahora Q-C4.
7. **OV-A14-01..09 reescritos** como escenarios explícitos; el 05 y el 06 son «por sistema».
8. **Tramo 2 = solo la fase 5.** El orden de los tramos 2 y 3, o si van en paralelo, lo fija el Coordinator al abrirlos.
9. **La condición 3 de C-05 queda como salvaguarda**, con el razonamiento de por qué no se conoce un camino que la active.
10. **Renumeración de los pendientes del Coordinator:**
    - Q-C1: fuentes;
    - Q-C2: celda;
    - Q-C3: materialidad (antes Q-C6);
    - Q-C4: nombres de unidad (antes parte de Q-C5);
    - la antigua Q-C3 (Smoke-PERF) y la antigua Q-C4 (ADR-0047) ya no son preguntas: tienen regla y valor por defecto en C-12.

### 11.4 Lo que queda, por autoridad

- **Owner, en la firma (§11):**
  - elegir los hitos: 0 a 4, 0 a 3 o corregirlos;
  - confirmar las dos deducciones de Q-03 (los comandos de creación; la decisión de D13);
  - confirmar la lectura de «posición»;
  - confirmar el resto de materias.
- **Coordinator:**
  - Q-C1, antes del paso 3;
  - Q-C2 y Q-C3, en la orden de revisión del Architect;
  - Q-C4, en el bootstrap de cada unidad;
  - las filas suyas de «Decisiones pendientes de autoridad».
- **Architect:** la revisión formal del commit del paso 3, en especial:
  - la composición de «Actualizar todo»;
  - la vía de C-10;
  - `Applies-to` y el alcance de A-1..A-13;
  - la regla de lectura;
  - la vida del borrador.

## 12. La v2.2 (2026-11-23)

### 12.1 Entradas

- **S/`independent-review-a14-v21-check.md`:** SHA-256 `be9d965987ebd2e26b49cf74f08d97de25b048578c4e63a18dce86f4c9996913`, sobre la v2.1
  `e4758ae9…5925`. EMITIBLE TRAS CORRECCIONES: 0 bloqueantes, 1 importante (N2-01) y 5 menores. N-01 quedó parcial.
- **La orden del Coordinator, por el chat:** extender a las unidades nuevas solo A-7; no extender ningún puente y dejarlo, si hiciera
  falta, como pregunta en la firma; los cinco menores; los añadidos a la firma.
- **W, leídos de nuevo:**
  - A-1 `:59-73` (I64-SCOPE-BRIDGE-01: «Se aplica a las tareas delegadas futuras de I-64 hasta que una autoridad de protocolo integrada
    la sustituya»);
  - A-5 `:205-210` («Caduca para I-64 cuando una revisión autoritativa integrada del protocolo la sustituya»).

  W sigue en `fa59bd82` con el árbol limpio.

### 12.2 Salidas

- **`I-64-A-14-draft-v2.2.md`:** 1066 líneas, SHA-256 `89670c2f…95f5` al cerrarse esta nota. Sin rutas locales, nombres ni correos.
- **`a14-v22-disposition.md`:** 29 líneas; 7 filas, 7 aplicadas, 0 rechazadas, más los ajustes de la firma.

### 12.3 Decisiones nuevas de la v2.2

1. **Solo A-7 cambia de alcance.** Es la única ampliación que A-14 hace sobre una A-n anterior, y la autoridad es la de A-7 (Architect +
   Coordinator; Owner informado).
2. **La pregunta P-1**, fuera de la frase de la firma: si se amplían A-4 (Scope) o A-6 (FreeText) a las unidades nuevas.
   - Por defecto, no.
   - Si el Owner quiere, la ampliación va por la A-n siguiente, con el dueño del protocolo; A-14 no la hace.
   - **Mi juicio:** puede hacer falta, porque la deuda del Controller sigue activa para I-62 (`I-64.yml`, `protocol_debt`), pero no
     lo decido.
3. **La autoridad de Smoke-PERF por tramo**, en una sola fórmula: «el Owner, a propuesta del Coordinator».
4. **Si el tramo afectado por un cambio de hitos es el tramo 1:** sus fases siguen, y lo que espera a la A-n es su READY-01.

### 12.4 Lo que queda, por autoridad

- **Owner, en la firma:**
  - los hitos (0 a 4, 0 a 3 o corregidos);
  - las dos lecturas marcadas (puntos 9 y 10 en lenguaje llano);
  - P-1;
  - el resto de materias.
- **Coordinator:** Q-C1 a Q-C4, y la confirmación de que A-7 puede ampliarse con su propia autoridad.
- **Architect:** la revisión formal. Además de lo listado en §11.4, la ampliación de A-7 y la ausencia de puentes en las unidades nuevas.

## 13. La v2.3 (2026-11-23), texto final antes de versionar

### 13.1 Entradas

- **S/`independent-review-a14-v22-check.md`:**
  - SHA-256 `4354e08195632dfa475878b94bf5b22c24d620f8bad3596c250056b51eb8e75f`, sobre la v2.2 `89670c2f…95f5`;
  - **EMITIBLE A ARCHITECT**, con 0 bloqueantes, 0 importantes y 4 menores.
- **La orden del Coordinator:** aplicar los cuatro menores, N3-01 a N3-04.
- **W:**
  - se leyó de nuevo la cabecera y la regla de A-5 (`:1`, `:40-60`), para glosar FreeText;
  - A-1 `:59-73` ya se había leído, para Scope;
  - sigue en `fa59bd82`, con el árbol limpio.

### 13.2 Salidas

- **`I-64-A-14-draft-v2.3.md`:**
  - 1081 líneas, SHA-256 `edf20bd2…4f13` al cerrarse esta nota;
  - sin rutas locales, nombres ni correos: la búsqueda solo da `UserSettings.cs`, que es una ruta del repo.
- **`a14-v23-disposition.md`:** 4 menores, 4 aplicados y 0 rechazados, más el detalle del punto 10.

### 13.3 Cambios y decisiones de la v2.3

1. **N3-01:** la fila de §12.5 en §4 usa la fórmula única de autoridad.
2. **N3-02:** Smoke-PERF es una sesión más por tramo, además de los smokes y la validación.
3. **N3-03:** P-1 pasa a nota informativa, con la glosa de Scope y de FreeText. La fila 3b de §12 pasa a «no ampliar», ya decidido por la
   autoridad de los puentes.
4. **N3-04:** se informa al Owner de la ampliación de A-7, en un bloque nuevo «Se te informa (no firmas esto)», con los puntos 15
   (A-7) y 16 (puentes).
5. **Punto 10:** los archivos exportados o guardados «van donde tú elijas».
6. **Sin cambios en la frase exacta de la firma.**

### 13.4 Lo que queda

- **Coordinator:**
  - Q-C1, antes del commit del paso 3;
  - Q-C2 y Q-C3, en la orden de revisión del Architect;
  - Q-C4, en el bootstrap de cada unidad.
- **El commit del paso 3:** versiona la v2.3 como `docs/initiatives/I-64-A-14.md`, PROPOSED, con la lista única de fuentes y revisiones.
- **Architect:** la revisión formal.
- **Owner:** la firma, después de Architect y Coordinator AGREED.
