# I-65 — F4: preparación del Candidato de hito y de la validación final (OV-I65-01, OV-I65-03..07)

Documento de la unidad (Coordinator, 2026-10-16), por la disposición del Owner «P2 Acceptance / F4 Final Candidate Preparation»
([texto literal](../automation/decisions/I-65-owner-disposition-p2-acceptance-candidate-prep.original.txt); decisiones
[§74](../automation/decisions/I-65.md)).

> **Estado: PREPARADO. Candidato NO declarado. FINAL VALIDATION EXECUTION = NO. F4 GATE PASS = NO.** Ni AutoCAD ni RAM se abrieron
> para esta preparación, y no se usó Codex. El commit que contiene este documento es el **SHA propuesto** para el Candidato (§2). La
> evidencia local y de CI de ese SHA se produce **después** de crearlo, con el árbol limpio (AGENTS.md, «Orden para la evidencia local
> de un Candidato»), y se registra en la evidencia §79 como hecho posterior al commit.

## 1. Preflight (2026-10-16 ~22:40Z, solo lectura)

| # | Comprobación | Resultado |
|---|---|---|
| PF-01 | Rama y punta | `architecture/interop-analisis-estructural` = `origin` (`d6746d2b` antes de este commit); árbol limpio salvo esta preparación |
| PF-02 | Base frente a `main` | `origin/main` = `bb0d5522` = merge-base: **0 commits detrás**, 119 por delante; sin rebase necesario (READY-04). El checkout principal local está 90 commits detrás de su propio `origin/main` (no es de I-65; no se toca) |
| PF-03 | Freeze | `docs/initiatives/I-65-proposal-v3.1.md`, blob `4e6e1feb…` igual en `a9d7080b` y en la punta; 0 commits sobre él desde el Freeze; sin A-n ni Freeze delta |
| PF-04 | ADR-0050 | blob `c18def42…`, igual en `a9d7080b` y en la punta |
| PF-05 | Conflictos (`git merge-tree` contra cada rama remota por delante de `main`) | `portabilidad-coordinador-principal` (206) y `rackmirror-espejo-semantico` (161): **sin conflictos**. `workspace-persistente-rackcad` (I-64, Candidato de su reintento `09f2f1c3`): **2 conflictos ya conocidos** en el censo de comandos (`CustomPropertiesCommandGuardTests.cs`, `SelectiveEditorOpenTests.cs`): las dos ramas suben de 37 a 38 (`RACKRAMPAQUETE` / `RACKPANEL`). Quien integre segundo reconcilia a 39; no bloquea el Candidato |
| PF-06 | Identidad de producto | árboles `src` `6cadd526…`, `tests` `47fbcaf5…`, `tools` `29485f42…`, `assets` `d1cc1811…` y archivos de build **idénticos** entre `51fc82f1` (GREEN de F4-D) y la punta: después de `51fc82f1` solo hay documentación. Tabla `section-material-map.json` blob `d088e2a9…` (las dos filas `verified`) |
| PF-07 | Custodia de P2 | 15/15 SHA-256 de la evidencia §77 recalculados iguales; 30 capturas iguales (resumen); `pkg-01` con sus tres archivos originales intactos más `ram-readback.json` `e622f429…`; sin cambios desde la corrida |
| PF-08 | Host (solo lectura) | `RAMElements.exe` `7dcb4855…` (26.00.01.15); `ramelements` 0.1.0, árbol `f71a017f…`; bases United States sin cambios desde M-05c (`W.sec` `dffd9e00…`, `HSS_SQR.sec` `992ddac9…`, `Steel.mat` `2a8b4239…`). Los nombres `W 12X26` y `HSS_SQR 4X4X1_4` solo existen en United States; `A992 Gr50` y `A500 GrC rectangular` solo en United States/`Steel.mat` (en Canadá existe `CSA_A500 GrC rectangular`, otro nombre). El registro US de A500 GrC confirma DIF-01 (W 0.28179435 lb/in³ = 486.94 pcf) |

## 2. SHA propuesto para el Candidato y estado de READY

**SHA propuesto:** el commit que introduce este documento (sus hijos solo añaden custodia). Se elige así porque READY-02 y READY-07 piden
la documentación versionada y el árbol limpio. `51fc82f1`, el GREEN de F4-D, tiene el mismo producto pero no incluye los documentos
de la validación. La DLL del Candidato se compila **desde ese SHA** (el SHA queda estampado en el ensamblado), así que **no** es la
`fa334b14…` de P2.

| READY | Condición (LIFECYCLE §8) | Estado |
|---|---|---|
| READY-01 | Alcance congelado completo; diferimientos decididos | **Cumple**: Freeze V3.1 sin A-n; Q-O1 y Q-O7 aprobados por el Owner (§74); OV-I65-02 sigue en F5 por el Freeze |
| READY-02 | Gates cerrados; decisiones y documentos versionados | **Cumple para un Candidato de hito**: F1 y F3 cerrados; las unidades de F4 (T1-T6, F4-D) verificadas y cerradas. El GATE PASS de F4 espera, por diseño, a la OV sobre el Candidato (AC-F4-06/07/10) |
| READY-03 | Sin REQUIRED abierto, discrepancia A ni decisión material del Owner pendiente | **Pendiente del Owner**: estrategia del modelo manual (§5) y OD-F4-11. Hallazgos del Architect: §4 |
| READY-04 | Fetch, preflight y rebase final | **Cumple** (PF-01..PF-05): 0 detrás de `main` |
| READY-05 | Focales/relevantes y CI del SHA | **Se produce después del commit** (evidencia §79): Core Full, UI Full, builds Debug de UI y Plugin, CI exacto con `ui-tests` |
| READY-06 | Conformidad `CONFORMING` del Architect y del Coordinator sobre ese SHA | **Pendiente: requiere autorización** (invocar al Architect no está en las 10 tareas autorizadas). Sin ella **no se declara** el Candidato |
| READY-07 | Árbol limpio, HEAD identificado | Cumple al crear el commit |
| READY-08 | Matriz OV completa y preparada para ese SHA | **Preparada** (§6-§7) |
| READY-09 | Identidad e inmutabilidad del Freeze, sin A-n | **Cumple** (PF-03, PF-04) |

## 3. Pruebas y CI

Se ejecutan sobre el SHA propuesto, después de crearlo y con el árbol limpio, y se registran en la evidencia §79 con el bloque de la
[guía manual §7.1](../guias/validacion-manual-autocad.md):
- `dotnet --version` (SDK resuelto);
- Core Full (`tests/RackCad.Tests`) y UI Full (`tests/RackCad.UI.Tests`);
- build Debug de UI;
- build Debug del Plugin: la DLL del Candidato va a la carpeta **nueva** `D:\r65-smoke\<sha8>\bin-Debug-net8.0-windows`, sin sustituir
  ninguna anterior; se registran el SHA-256 de la DLL y de la tabla;
- herramienta de F4 compilada desde el mismo SHA y congelada;
- CI de `push` del SHA exacto con `UI Tests (WPF controls, net8.0-windows)` en `success`;
- biblioteca de bloques: ruta resuelta y SHA-256, porque hay validación del Owner.

## 4. Riesgos y hallazgos del Architect

Estado compilado de las fuentes: decisiones, revisiones del Architect y evidencia de P2. La fuente posterior manda.

| Grupo | Estado | Propuesta |
|---|---|---|
| **REQUIRED** | **Ninguno abierto.** El único de F4, A65-F4P-01, se cerró en `d2539be2` (CD-I65-F4P-07). Las tres revisiones de F4 tienen 0 REQUIRED (CD-I65-F4-21, -44, -124) | — |
| AC-F4-04: A65-F3-01, 02, 04, 11 | **Corregidos** en F4-T1 (RED `53bfcd94`, GREEN `a6fa5a35`, verificado `f5c6b885`, CD-I65-F4-08), confirmados por el Architect y observados en P2 (columnas leídas a −90) | — |
| A65-F1-03, 10, 12 | Corregidos en F4-T2 `0070ae40` (CD-I65-F4-12) | — |
| A65-F1-06 (INV-11 por subcadena) | Disposición «sin cambio» solo propuesta (preparación §5.2, CD-I65-F4P-06); no pasó al contrato de F4 | el Owner formaliza «sin cambio» o lo difiere |
| A65-F4R-06 (combinaciones) | El Owner y el Coordinator la dispusieron con M-05c PASS y CMB-3 (CD-I65-F4-71/72); combinaciones PASS en P2 (CD-I65-F4-160). La última revisión del Architect (antes de P2) la da «abierta» | que el Architect la cierre en READY-06 |
| A65-F4R-08 (comando en AutoCAD) | Camino `Unsupported` cerrado (CD-I65-F4-92); camino `Valid` ejecutado en el segundo smoke (16/16, CD-I65-F4-147) y en P2; sin cierre explícito del Architect | que el Architect la cierre en READY-06 |
| A65-F4R-07 (`Valid` con `Table == null` imprime «Valid.», `RackRamElementsCommands.cs:116-118`) | **Abierta**, OPTIONAL | el Owner decide: limitación registrada sin código en F4 (recomendado: el camino real siempre tiene tabla) o corrección autorizada, que crearía un SHA nuevo e invalidaría este Candidato |
| A65-F4R-02 (constante 1e-6; volcado en la misma sesión) | La parte de procedimiento se aplica (CR-09, aplicada en P2); la parte de código sigue **abierta**, OPTIONAL | limitación registrada |
| A65-F4D-02, 03, 04, 05, 09 | **Sin disposición** (CD-I65-F4-124 solo dice que no se convierten en trabajo), OPTIONAL | el Owner los registra como mejoras sin trabajo en F4 |
| A65-F4-07..11, A65-F4R-01/03/04/05/09 | Dispuestos como limitaciones o mejoras (CD-I65-F4-21, -44) | — |
| A65-F4D-01, 06, 07, 08, 10, 11; A65-F4R-10 | Satisfechos o dispuestos (decisiones §66-§73) | — |
| DIF-01 | Informativa, solo sin peso propio | se mantiene |
| DIF-02 | Resuelta por D-07 (ν 0.30, G nulo); las citas de E y ν quedan como convención de modelado aceptada (D-01..D-08) | — |
| DIF-03 | Resuelta (CD-I65-F4-74) | — |
| DIF-04 | **Dispuesta por el Owner (§74)**: compatibilidad de las propiedades con AISC v16.0 aceptada para el piloto; **edición formal del catálogo de RAM = UNKNOWN**, limitación documentada; no se afirma que Bentley identifique el catálogo como v16.0 | — |
| EXP-01 | Solo clase B (discovery); ninguna A | confirmar en la revisión AC-F4-09 |

Ningún hallazgo abierto es REQUIRED. Para READY-03 y READY-06 faltan: la disposición del Owner sobre A65-F4R-07, A65-F4R-02,
A65-F4D-02/03/04/05/09 y A65-F1-06, y el cierre del Architect de A65-F4R-06 y A65-F4R-08.

## 5. Estrategia del modelo manual de referencia (solo plan; nada guardado)

**A — Reutilizar el volcado manual de P2 para OV-I65-01 (recomendada).**
- El volcado `c4a98fb0…` procede de un modelo construido solo desde la especificación congelada y comprobado contra ella antes de
  comparar (decisiones §73, aceptado en §74). Es independiente del modelo generado.
- Lo atribuyen:
  - la especificación (blob del procedimiento manual `f39918ad…` y del paquete de ingeniería `11853341…`);
  - la hoja de entrada `beefa054…`;
  - `RAMElements.exe` `7dcb4855…`, `ramelements` árbol `f71a017f…`;
  - las bases US `dffd9e00…` / `992ddac9…` / `2a8b4239…`;
  - la plantilla de volcado (sin cambios desde `93606ea0`).
- **Condición de uso:** en la ventana de la OV se vuelven a medir esas identidades. Si cambia **cualquiera**, A no vale y se aplica B.
- No se reutiliza el modelo (no existe) ni la lectura de vuelta de P2: solo el volcado manual, que es la entrada «manual» del
  comparador. El comparador rechaza dos volcados iguales, y el generado será nuevo.
- Decisión del Owner (OD-F4-11): aceptar A como referencia manual de la OV del Candidato, o exigir una construcción nueva.

**B — Conservar un modelo manual de referencia verificado (para corridas futuras o si A no se acepta).**
1. La **próxima vez** que se construya el modelo manual (en la OV o más adelante), con autorización expresa del Owner para guardar un
   modelo de RAM (D-11): construirlo solo desde la especificación, volcarlo y comprobar el volcado contra la especificación.
2. **Guardar como** (nunca «Guardar» sobre otro) en una ubicación controlada, por ejemplo
   `D:\r65-ram-reference\REF-CANT-01\RAM-26.00.01.15-7dcb4855\`, fuera de todo repositorio; registrar el SHA-256 del archivo y marcarlo
   de **solo lectura**.
3. Escribir al lado un manifiesto de referencia con: blobs de la especificación, SHA-256 de la hoja de entrada, de `RAMElements.exe`,
   del árbol de `ramelements`, de las bases US y del volcado de verificación; unidades de la interfaz; operador; fecha.
4. **Reutilización:**
   - copiar el archivo a una carpeta **desechable nueva**, nunca abrir el original;
   - File > Open de la copia;
   - volcado nuevo en una carpeta nueva;
   - comparar ese volcado con el de verificación (igualdad de todos los valores);
   - comprobar que el ejecutable y la API son los del manifiesto.
   Solo si todo coincide se usa como referencia manual. Si no, STOP. Así se descarta que guardar y reabrir cambie el modelo.
5. Si cambia la versión de RAM, el modelo guardado no se reutiliza: se reconstruye desde la especificación y se crea una referencia nueva.

## 6. Matriz de la validación final (sobre el Candidato)

Todas usan paquetes y modelos **desechables e independientes**. Ninguna reutiliza `pkg-01` ni su lectura de vuelta. Se mantienen CMB-3,
DIF-01 informativa y todas las tolerancias.

| OV | Dónde | Entrada | Esperado | Evaluador | Paradas propias |
|---|---|---|---|---|---|
| **OV-I65-01** | AutoCAD (C) + RAM (D-E) + comparador (F) | `pkg-OV01`, generado con la DLL del Candidato sobre REF-CANT-01, más el volcado manual (A o B) | `Valid` con diagnósticos vacíos; validador v2 16/16; `READBACK OK` con la huella del manifiesto; volcado del generado `DUMP OK`; comparador sin `--*-unread` **PASS (0)**; propiedades por la hoja D-09 | comparador + hoja firmada | cualquier `FAILED`, comparador ≠ 0, cambio de identidades |
| **OV-I65-03** | AutoCAD | estaciones no soportadas en un DWG desechable: (a) doble cara, (b) brazo `NegativeY`, (c) brazo con más de 10°, (d) canales pareados o familia C/L, (e) sección sin fila en la tabla | **sin paquete** (la carpeta no se crea) y diagnóstico visible `Unsupported`/`Invalid`. El caso «fila sin verificar» se evidencia con T4-18 en las suites del Candidato (y con la herramienta de pruebas sobre una copia de prueba de la tabla), **nunca** alterando la tabla del Candidato; el primer smoke lo mostró en AutoCAD con las filas `proposed` (informativo, no del Candidato) | Coordinator con capturas | se escribe un paquete o no hay diagnóstico |
| **OV-I65-04** | RAM | `pkg-OV04`, un **segundo** paquete válido generado aparte, sobre un modelo desechable **no vacío** (nuevo, con un nudo tecleado a mano) | rechazo `gate:` **antes** de escribir; `ram-readback.json` `FAILED` con el motivo de la puerta; conteos sin cambio (Elementos: 1); descarte sin guardar | consola + lectura de vuelta + barra de estado | el script escribe el modelo |
| **OV-I65-05** | RAM + comparador | `inj-OV05`: `inject --fault unknown-section` desde `pkg-OV01` | `FAILED — DISCARD MODEL` en la consola y en la lectura de vuelta; modelo posiblemente parcial → descarte sin guardar; comparador **REJECTED** (2) por la marca de inyección | consola + lectura de vuelta + comparador | `READBACK OK` (hallazgo: RAM aceptaría una sección inexistente) |
| **OV-I65-06** | RAM | `inj-OV06`: `inject --fault inverted-axes` | `FAILED` con la diferencia de ejes locales; descarte sin guardar | consola + lectura de vuelta | `READBACK OK` |
| **OV-I65-07** | RAM | `inj-OV07a` (`wrong-version`) y `inj-OV07b` (`missing-core-function`), cada uno en un modelo nuevo vacío | rechazo antes de cualquier escritura; nudos = miembros = 0 (Elementos: 0); lectura de vuelta `FAILED` con `gate:` | consola + lectura de vuelta + barra de estado | el modelo recibe elementos |

Condición de aceptación de F4 (AC-F4-07): OV-I65-01 PASS según la matriz §4, salvo las filas informativas, y OV-I65-03..07 PASS, con
custodia. Después: revisión del Architect, cierre y GATE PASS **solo con orden del Owner**.

## 7. Paquetes y archivos que hay que generar

| Artefacto | Cómo | Cuándo |
|---|---|---|
| DLL del Candidato y salida completa | `dotnet build … -c Debug -o D:\r65-smoke\<sha8>\bin-Debug-net8.0-windows` desde el SHA propuesto, sin AutoCAD; manifiesto de salida | tras el commit (autorizado: tarea 6) |
| Herramienta de F4 del Candidato | build Debug de `tools\RackCad.StructuralAnalysis.RamTool` congelada en `D:\r65-smoke\<sha8>\ramtool\` | tras el commit |
| Valores esperados del validador v2 | `smoke2_expected_inputs.ps1` con el `RackCad.Application.dll` y la tabla del Candidato | tras compilar |
| DWG desechable REF-CANT-01 y DWG de casos OV-03 | dibujados en la ventana de AutoCAD, cerrados sin guardar | ventana W1 |
| `pkg-OV01`, `pkg-OV04` | `RACKRAMPAQUETE` (estación 0, cargas congeladas `25f48e7f…`, carpetas nuevas con `\`) | ventana W1 |
| `inj-OV05`, `inj-OV06`, `inj-OV07a`, `inj-OV07b` | `inject` de la herramienta del Candidato desde `pkg-OV01`, en carpetas nuevas (ensayo previo hecho fuera de la OV sobre `pkg-01`: los cuatro tipos responden con código 0 y dejan el origen intacto) | tras W1, sin AutoCAD ni RAM |
| Scripts de volcado | `dump-script --out <carpeta nueva> --adapter-version 1.0.0` para el generado de OV-01 (y para el manual si se aplica B) | antes de W2 |
| Lecturas de vuelta, volcados, informes del comparador, capturas, diferencias de `Config.ini` | durante W2 y después | W2 |

Estructura propuesta: `D:\r65-smoke\<sha8>\ov-R<fecha>\` con `pkg-OV01\`, `pkg-OV04\`, `inj-OV05\`, `inj-OV06\`, `inj-OV07a\`, `inj-OV07b\`,
`dump-OV01-gen\` (y `dump-OV01-man\` con B), `host-before\`, `host-after\`, `captures\`.

## 8. Presupuesto propuesto

| Recurso | Propuesta |
|---|---|
| AutoCAD (W1) | 1 ventana de ~90 min; **1 NETLOAD** («Load Once») de la DLL del Candidato; 2 generaciones válidas (OV-01 y OV-04) y 1 intento por cada caso de OV-03 (5); sin reintentos ante un diagnóstico inesperado |
| RAM (W2) | 1 ventana de ~90 min con A (~120 min con B); **1 ejecución** por paquete (OV-01, OV-04, OV-05, OV-06, OV-07a, OV-07b); cada modelo descartado sin guardar, salvo el modelo de referencia B si se autoriza guardarlo |
| Correcciones | 0 para los scripts y paquetes (un fallo inesperado es STOP); 1 corrección controlada del modelo manual solo si se aplica B |
| Código | ninguno. Un defecto del producto lleva a STOP y exige autorización de implementación (`attempts` 7 de 9: quedan 2) |
| Codex / Workers | 0 / 0 (Codex sigue en 50 de 52) |
| Architect (READY-06) | 1 revisión de conformidad del SHA propuesto, por un medio que decida el Owner |

## 9. Ventanas de host

| Ventana | Para qué | Coordinación |
|---|---|---|
| **W1 — AutoCAD** | NETLOAD de la DLL del Candidato; `pkg-OV01`, `pkg-OV04` y OV-03 | RELEASE del Owner (autoridad reconocida por I-52) con la identidad de la DLL **antes** de abrir; aviso y confirmación de I-64; BASELINE/POST/DIFF como en el segundo smoke; una sola `acad.exe`; nadie usando el equipo |
| **W2 — RAM** | OV-01 (construcción, lectura de vuelta, volcado), OV-04..07 | Avisos previo y de cierre a I-64 (los pide) y, por cortesía, a I-52; no coincidir con una BASELINE/POST de I-64; copia de `Config.ini`; nadie usando el equipo |

W1 va antes que W2 (W2 necesita los paquetes). Pueden ser el mismo día.

## 10. Condiciones de parada de la validación final

- Cualquier cambio de identidad: SHA de la DLL o de la tabla, `RAMElements.exe`, `ramelements`, bases US.
- OV-01: comparador distinto de PASS, cualquier `FAILED`, validador del paquete distinto de 16/16.
- Escenario negativo que no falle como se espera (por ejemplo, `READBACK OK` en OV-05).
- AutoCAD o RAM con avisos de seguridad no previstos, cambios de configuración no planificados o escrituras fuera de las carpetas de
  la corrida.
- Conflicto de host.

Toda parada: registro literal, avisos de cierre y decisión del Owner. Ninguna parada se resuelve cambiando código sin autorización.
