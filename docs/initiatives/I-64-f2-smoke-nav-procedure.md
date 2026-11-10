# I-64 — Smoke-NAV: procedimiento exacto (C-F2T2-12)

Procedimiento del hito Smoke-NAV de F2 (Freeze V5 §12.1 y §12.3). Sigue la estructura de `I-64-f1-t3d-smoke-1-procedure.md` y su guarda de procesos, y solo cambia lo que este hito necesita.

## 0. Autoridad y límites

- **Freeze V5:** §11 (Smoke-NAV tras F2), §12.1 (reglas comunes), §12.3 (SN-01..SN-10), D-01, D-05, D-07, INV-03, INV-14, INV-22, INV-38, INV-40.
- **Instrucción nocturna del Owner** (`F2-T2A-NAVIGATION-HOST/coordinator/coordinator-f2t2a-overnight-autonomy-directive.md`): la sesión prepara y ejecuta Smoke-NAV si el entorno permite interacción real con AutoCAD. No declara PASS por pruebas unitarias.
- **Host:** una sola `acad.exe`; sin cambios en perfil, `TRUSTEDPATHS`, `SECURELOAD`, ACL, registro ni plugins instalados; aviso previo a I-52 por el canal entre sesiones y sin solaparse con sus sesiones.
- Smoke-NAV es un hito intermedio. No es Owner Validation ni fija `FINAL_CANDIDATE_SHA`.
- Solo se valida la navegación del rack del contexto. Las pestañas son de F3: lo que dependa de ellas se registra como `NOT_APPLICABLE_UNTIL_F3`.

## 1. Roles

| Rol | Hace |
|---|---|
| Sesión | Prepara el candidato, los dibujos y el kit; opera AutoCAD por computer use; registra; propone la disposición |
| Owner | Puede repetir cualquier paso. Firma la Owner Validation final, fuera de este hito |

Ningún Worker usa AutoCAD.

## 2. Prerrequisitos (cualquier NO es STOP antes de abrir AutoCAD)

| Id | Prerrequisito |
|---|---|
| NP-01 | F2-T2B CLOSED / PASS (`task-close-package.md`) |
| NP-02 | Candidato completo (§3) |
| NP-03 | 0 procesos `acad` y 0 `RAMElements`; ninguna sesión de I-52 en curso; aviso enviado |
| NP-04 | Dibujos N y N2 con su SHA-256 y copia recuperable (§2.1) |
| NP-05 | Herramientas de T3C con su SHA-256 aprobado y autoprueba 26/26 |
| NP-06 | Directorio `artifacts/orchestration/I-64/F2-SMOKE-NAV/<RunId>/` creado |

### 2.1 Dibujos de prueba

Son DXF generados sin AutoCAD, con sobres de RackCad en la definición de cada bloque, como `RackBlockData.Write`. **Nunca se guardan.** Su SHA-256 debe ser el mismo al terminar.

| Referencia | Id de rack | Vista | Inserción | Tamaño | Centro |
|---|---|---|---|---|---|
| R1 frontal, copia 1 | `11111111-…-000000000001` | frontal | (0, 0) | 100 x 60 | (50, 30) |
| R1 frontal, copia 2 | igual | frontal | (400, 0) | 100 x 60 | (450, 30) |
| R1 lateral | igual | lateral | (0, 200) | 40 x 60 | (20, 230) |
| R1 planta | igual | planta | (400, 200) | 100 x 40 | (450, 220) |
| R2 frontal | `22222222-…-000000000002` | frontal | (800, 0) | 80 x 50 | (840, 25) |

Además, una línea suelta de (-200, -100) a (-100, -100). **N2** tiene el mismo contenido y otro título: mismos RackId en otro documento, para SN-08.

Orden esperado de los objetivos de R1 (por vista y handle): frontal 1, frontal 2, lateral, planta. M = 4 para Ver y Centrar; M = 2, 1 y 1 para Localizar Frontal, Lateral y Planta.

## 3. Candidato

Commit limpio de la rama, con `git diff --stat 82bd49b6 HEAD -- src tests` vacío. Con AutoCAD cerrado: suites Core y UI completas en Debug, build Debug de UI y Plugin, SHA-256 de cada `RackCad*.dll` e identidad del commit estampada en `ProductVersion`, y CI del SHA exacto 4/4. Se registra en `candidate/candidate.md`.

## 4. BASELINE y POST

Las de T3C con la guarda de procesos de Smoke-1 (`Test-AcadProcessGuard.ps1`: Register, Watch, Check, Closed), HOST-EXTRAS antes y después, y comparación final. Cualquier STOP de la guarda detiene el hito.

## 5. Arranque

| Paso | Acción | Esperado |
|---|---|---|
| SN-00a | Una sola apertura de AutoCAD con acad = 0 | Una `acad.exe`, registrada por la guarda |
| SN-00b | `(getvar "CPROFILE")`, `(getvar "SECURELOAD")`, `(getvar "PICKFIRST")` | Perfil y SECURELOAD iguales a la BASELINE |
| SN-00c | `NETLOAD` del DLL Debug del candidato | Aviso de seguridad: solo **Load Once** |
| SN-00d | Inspección de módulos ligada a la PID | Un solo `RackCad.Plugin.dll`, con el SHA del candidato |
| SN-00e | `OPEN` de N; `ZOOM E`; `RACKPANEL` | Panel visible en su estado inicial |

Entrada de teclado: línea de órdenes sin `Ctrl+V`, en trozos de 16 caracteres como máximo. El texto se comprueba antes de Enter.

## 6. Secuencia

Antes de cada paso se anota hora y acción en `operator/session-log.md`. Tras cada acción de navegación se lee `(getvar "DBMOD")`: el bit 1 no debe activarse, porque la navegación no modifica el dibujo.

| Paso | Acción | Esperado |
|---|---|---|
| **SN-01** | Clic en R1 frontal 1 → contexto Uno(R1). **Seleccionar** | Texto «Rack seleccionado en el dibujo.»; `(sslength (ssget "_I"))` = 4; los pinzamientos visibles en las cuatro referencias; la selección sigue tras terminar la acción |
| **SN-07** | Tras SN-01, sin tocar la línea de órdenes, teclear `(getvar "CMDACTIVE")` y Enter | El texto llega a la línea de órdenes. Se repite tras una acción de cada tipo |
| **SN-06a** | Justo tras Seleccionar: leer panel y `(getvar "VIEWCTR")` | Contexto Uno(R1), mismo RackId, copia y resultado sin cambios; la vista no cambia |
| **SN-06b** | Seleccionar R1 y R2 → Selección (N) | Las acciones deshabilitadas, con motivo; sin llamada al puerto |
| **SN-06c** | Pestaña activa distinta | `NOT_APPLICABLE_UNTIL_F3` |
| **SN-03** | Uno(R1). **Ver**; después Siguiente copia y Ver, hasta dar la vuelta | `VIEWCTR` = (50,30), (450,30), (20,230), (450,220), (50,30), tolerancia 0.5; `VIEWSIZE` mayor que la altura de la referencia (margen); texto «Copia k de 4» |
| **SN-04** | Con la copia 2, **Centrar** tras leer `VIEWSIZE` | `VIEWCTR` = (450,30); `VIEWSIZE` igual, tolerancia 0.001 |
| **SN-05** | **Localizar** Frontal, Siguiente copia, Localizar; después Lateral y Planta; una vez con «Centrar en vez de ajustar» | (50,30), (450,30), (20,230), (450,220); M = 2, 1 y 1; en la variante centrada, `VIEWSIZE` igual |
| **SN-02** | Uno(R1) y `(setvar "CTAB" "Layout1")` | Las acciones no ejecutan nada y el panel lo explica; `(getvar "TILEMODE")` = 1 y `CTAB` sigue en Layout1 tras la acción. Vuelta con `(setvar "CTAB" "Model")` |
| **SN-08** | Abrir N2. Con Uno(R1) en N, disparar una acción y cambiar de documento con la menor demora posible. Se repite 3 veces | Nunca hay efecto en el documento que no originó la petición: `VIEWCTR` de N2 sin cambios, o texto «La petición pertenece a otro documento». Si la carrera no se puede provocar a mano, se registra así |
| **SN-09** | Con `Measure-NavAction.ps1` (solo UI Automation): primera acción de Ver tras abrir N2 (caché fría) y 5 repetidas | Se anotan los milisegundos. No hay umbral en el Freeze |
| **SN-10** | Uno(R1) desde la referencia lateral, y desde la planta: Ver y recorrer | Mismos M y mismas posiciones que en SN-03, desde cualquier referencia del rack. Vía pestaña: `NOT_APPLICABLE_UNTIL_F3` |

## 7. Cierre

`DBMOD` final, RackLog sin aserciones de `TopTransaction` (INV-14, build Debug), cierre de todos los documentos **sin guardar**, cierre normal de AutoCAD, guarda `Closed`, POST y comparación. SHA-256 de N y N2 iguales a los iniciales.

## 8. Disposición

Cada SN queda como PASS, FAIL, `NOT_APPLICABLE_UNTIL_F3` o LIMITADA con su motivo. Un defecto de producto detiene el hito y se registra; `attempts` está en 3 de 3, así que corregirlo necesita una decisión explícita. Smoke-NAV PASS exige SN-01..SN-07 y SN-10 en PASS, y SN-09 medido. SN-08 limitada por no poder provocar la carrera se declara como tal y no se cuenta como PASS.

## 9. Evidencia

Directorio de ejecución ignorado por Git; custodia final por las reglas de `custody_final.py` (repositorio público: sin rutas locales, usuario ni máquina). Capturas solo de la paleta y la línea de órdenes.
