# I-65 — Revisión del alcance de F5, OV-I65-02, A65-F4F-04/07 y requisitos de READY (solo análisis)

Documento de la unidad (Coordinator, 2026-10-17), por la disposición del Owner «F4 Gate Acceptance / F5 Scope and Evidence Review»
([texto literal](../automation/decisions/I-65-owner-disposition-f4-acceptance-f5-scope-review.original.txt); decisiones
[§85](../automation/decisions/I-65.md)). **Solo análisis:** sin código, sin AutoCAD, sin RAM, sin cambios de evidencia histórica.
F5 IMPLEMENTATION = NO; READY = NO; INTEGRATION = NO.

## 1. Estado formal de F4

**F4 = CLOSED / GATE PASS** (CD-I65-F4-221) sobre el SHA de cierre `1e7d8fc23e089619f61d40194969cecb09762336` (Core Full local 14390/14390;
CI 39052279094 4/4), con el Candidato de hito `5a5ca57731ebbbbc41ed4f702698709d5e128a86` (código = árbol de `51fc82f1`); reconocido por el
Owner (decisiones §85). No es READY ni `FINAL_CANDIDATE_SHA`. Limitaciones vigentes: CD-I65-F4-220.

## 2. Alcance residual exacto de F5 — Controlled Loads

**Fuente:** Freeze §6: «F5 | OV-I65-02»; §5: «OV-I65-02 | F5 y READY | Cargas de `REF1`: valor, posición y dirección equivalentes (sin
comportamiento si F6 está fuera)»; D-10b (canal de cargas); D-10 LOD-01; §4 (fila de cargas: 0.1 % / 0.02 in / exacta); §9 (no-objetivos).

### 2.1 Capacidades ya implementadas (F1-F4)

| Capacidad congelada (D-10b, §4) | Dónde | Estado |
|---|---|---|
| Archivo `rackcad-structural-loads/v1`: destino brazo por nivel y lado, `physicalArmFraction` en [0, 1], dirección `-Z`, unidad explícita (`N`, `kN`, `lbf`, `kip`, `kgf`); unidad ausente o desconocida, destino inexistente, fracción fuera de rango o dirección no admitida → `Invalid` | `StructuralLoadFileReader` (F1) | implementado y probado |
| Conversión a lbf con el propietario único de unidades | F1 (`ForceUnits`) | implementado y probado |
| Posición = distancia rígida de IDL-06 + fracción × longitud física, desde `ARM…J` | `CantileverStructuralModelSource` (F1) | implementado y probado |
| Más de 24 cargas por miembro → `Unsupported` | `StructuralModelBuilder` (F1) y traducción (F3) | implementado y probado |
| El archivo entra en la huella de entrada, se copia al paquete y se lista con su SHA-256 | F1/F3 | implementado y probado (V-05..V-07 del validador) |
| Emisión como carga puntual en miembro `GLOBAL_Y`, en **porcentaje** del miembro analítico y con signo menos | adaptador (F3) | implementado y probado |
| Escritura, lectura de vuelta y volcado de las cargas en RAM | plantillas del script y del volcado (F3/F4) | implementado; ejecutado en P2 y W2 |
| Fila de cargas del comparador por miembro y caso (emparejamiento por posición más cercana; valor 0.1 %, posición 0.02 in, dirección exacta) | `StructuralReadbackComparison` (F4) | implementado; **PASS** en P2 y en OV-I65-01 (W2) |

### 2.2 Capacidades congeladas todavía pendientes

**Ninguna capacidad de producto.** Lo único que falta del Freeze para F5 es la **validación OV-I65-02** con su propio archivo de cargas
(REF-CANT-01 §5: «No es el caso de la OV-I65-02, cuyo archivo se elige en F5»; OD-F4-06). F6 (comportamiento) sigue fuera de V1 (D-14,
Q-O5).

### 2.3 Pruebas existentes reutilizables (Core, verdes en el Candidato)

- `StructuralAnalysisLoadFileReaderTests`: forma exacta (`CadaDesviacionDeLaFormaExactaEsInvalidYNoDejaArchivo`), lectura y conversión
  (`UnArchivoValidoSeLeeConSusCargasEnOrdenYConvertidasALibrasFuerza`), las cinco unidades (`LasCincoUnidadesDeLaTablaSeAceptan`),
  acumulación de hallazgos.
- `StructuralAnalysisCantileverBoundaryTests`: posiciones con la distancia rígida, magnitudes en lbf y dirección −Z, cargas solo del archivo,
  nivel fuera de rango y lado distinto → `Invalid`, más de 24 → `Unsupported`, cambiar el archivo cambia modelo y huella.
- `StructuralAnalysisModelBuilderTests`: posición absoluta y conversión, 24 válidas y 25 `Unsupported`, carga sin unidad, carga fuera del
  miembro.
- `StructuralAnalysisRamElementsTranslationTests`: las tres cargas del golden con valor y porcentaje, signo menos y porcentaje, 24 sí y más
  de 24 no, carga sobre miembro inexistente.
- Pruebas del comparador de F4-T6 (fila `LOADS`).

### 2.4 Trabajo adicional realmente necesario

| # | Trabajo | Tipo | Código |
|---|---|---|---|
| F5-1 | Archivo de cargas de OV-I65-02 y su tabla de valores esperados derivada a mano (§3) | documentación y datos de validación | no |
| F5-2 | Comprobación en seco con las DLL congeladas (como en W1) | preparación | no |
| F5-3 | Procedimiento de F5: exportación en AutoCAD (`pkg-OV02`), construcción, lectura de vuelta y volcado en RAM, referencia manual con las mismas cargas, volcado y comparador | documentación | no |
| F5-4 | Ejecución (dos ventanas de host) y custodia; revisión del Architect; cierre con SHA de cierre (Core Full local + CI) | validación | no |

No hace falta ninguna prueba nueva de producto: las dimensiones del canal ya están probadas a nivel unitario y OV-I65-02 las ejerce de punta a
punta.

### 2.5 Validaciones reservadas al Owner / ingeniería

- Aprobar el conjunto de cargas de OV-I65-02 (magnitudes, posiciones y unidades) como caso de validación, sabiendo que **no** representa
  tarimas, impacto, sismo ni áreas tributarias (LOD-01).
- Decidir quién construye la referencia manual (Claude con Computer Use o el ingeniero) y aceptar el resultado.
- Recordar que la equivalencia de transferencia **no** valida resistencia ni comportamiento (Q-O3).

**Fuera de alcance (no se añade):** combinaciones, peso propio, comprobaciones RMI/AISC, sismo, cargas distribuidas, momentos nodales,
cargas locales u otras direcciones (Freeze §9 y D-10 LOD-01).

## 3. Propuesta de OV-I65-02 y de su archivo de cargas

### 3.1 Criterio de selección (relación con el contrato congelado)

El archivo de OV-I65-01 (`P1`-`P3`) cubre `lbf`, `kip` y `kgf`, una carga por brazo y fracciones interiores. OV-I65-02 cubre lo que el
contrato D-10b admite y OV-I65-01 **no** ejerció, sin salir de él:

| Dimensión de D-10b / §4 | Cubierta por OV-I65-01 | Añadida por OV-I65-02 |
|---|---|---|
| Unidades `N` y `kN` | no | sí |
| Fracción en los extremos del intervalo cerrado [0, 1] | no | sí (0 y 1) |
| Varias cargas en el mismo miembro (≤ 24; emparejamiento por posición) | no | sí (dos por brazo) |
| Los tres niveles, lado `PositiveY`, dirección `-Z`, caso `REF1` | sí | sí (sin cambio) |
| Estación `REF-CANT-01` (Q-O7) | sí | sí (sin cambio de geometría) |

### 3.2 Archivo propuesto (no creado todavía; pendiente de aprobación)

Seis cargas, dos por brazo. Distancia desde `ARM.k.PositiveY.J` = 6.354218840504721 + fracción × 36 in; porcentaje = 100 · distancia /
42.35421884050472; 1 lbf = 4.4482216152605 N; 1 kgf = 9.80665 N.

| id | nivel | fracción | magnitud | distancia esperada (in) | porcentaje esperado | lbf esperado |
|---|---|---|---|---|---|---|
| Q1 | 1 | 0.0 | 2000 N | 6.354218840505 | 15.002564123383 | 449.617886199421 |
| Q2 | 1 | 1.0 | 1.5 kN | 42.354218840505 | 100.000000000000 | 337.213414649566 |
| Q3 | 2 | 0.25 | 750 lbf | 15.354218840505 | 36.251923092537 | 750.000000000000 |
| Q4 | 2 | 0.9 | 1 kip | 38.754218840505 | 91.500256412338 | 1000.000000000000 |
| Q5 | 3 | 0.5 | 300 kgf | 24.354218840505 | 57.501282061692 | 661.386786554633 |
| Q6 | 3 | 0.75 | 4.5 kN | 33.354218840505 | 78.750641030846 | 1011.640243948697 |

Todas `-Z`, lado `PositiveY`, caso `REF1`. **Punto a decidir:** Q2 en la fracción 1.0 cae en el 100 % del miembro analítico (la punta).
Está dentro del contrato, pero RAM no se ha medido con una carga puntual exactamente en el extremo: si la rechaza o la desplaza, será un
hallazgo (y STOP de ese caso). Alternativa conservadora: 0.95. Recomendación: mantener 1.0, porque es el límite del contrato y conviene
conocer el comportamiento antes de READY.

Alternativa mínima (no recomendada): reutilizar `P1`-`P3`. Duplicaría la fila de cargas de OV-I65-01 sin cubrir nada nuevo del contrato.

### 3.3 Ejecución propuesta

| Paso | Contenido |
|---|---|
| Candidato | **Reutilizar `5a5ca577`** (producto sin cambios), con extensión expresa del Owner de la lectura de CD-I65-F4-178 al hito F5; alternativa: SHA nuevo de hito (evidencia completa, unos 25 min, DLL nueva) |
| Ventana A (AutoCAD, ~30 min) | REF-CANT-01 en un DWG desechable; `RACKRAMPAQUETE` con el archivo de OV-I65-02 → `pkg-OV02` (`Valid`, validador v2 adaptado a las cargas nuevas, contraste de la carga útil con la tabla §3.2) |
| Ventana B (RAM, 90-120 min) | `pkg-OV02` en un modelo vacío → `READBACK OK`; volcado en la misma sesión; **referencia manual** (geometría de REF-CANT-01 + las seis cargas) → volcado; comparador sin `--*-unread` → **PASS**, con la fila `LOADS` decisiva y el resto como contexto |
| Evaluadores | comparador (fila `LOADS`), lectura de vuelta y la tabla §3.2 |
| Referencia manual | no puede reutilizar el volcado de P2 (otras cargas). Opciones: Claude con Computer Use (unos 30-40 min de tecleo, con los riesgos de P2) o el ingeniero |

## 4. A65-F4F-04 — trazabilidad del volcado respecto de su sesión y su modelo

**Situación:** el volcado no lleva huella ni identificador de sesión. Sí recoge las descripciones que escribe el script de construcción
(`ARM.1.PositiveY`, `COL.FOOT`…), lo que distingue un modelo construido por un paquete de uno manual, pero no **qué** paquete ni **qué**
sesión. W2 lo demostró: el volcado del generado salió idéntico byte a byte al de P2 (mismo contenido), así que ninguna comparación de
contenido puede distinguir dos sesiones con la misma geometría.

| Opción | Qué liga | Coste | Recomendación |
|---|---|---|---|
| **0 — sin código** | Carpeta nueva con creación exclusiva (ya existe); `ram-dump.json` inexistente antes y presente después (ya existe); además, PID y hora de inicio de `RAMElements.exe` en PRE/POST, `CreationTimeUtc` del volcado dentro de la vida de ese proceso y posterior a la lectura de vuelta, SHA-256 inmediato; y un contraste offline `ram-readback.json.values` ↔ volcado del generado (mismo contenido que el modelo construido por ese paquete) | solo procedimiento y un script de evidencia fuera del producto | **sí, para F5 y READY** |
| 1 — código mínimo | El script de construcción escribe un identificador de sesión (o la huella) en el modelo (p. ej., en la descripción de un nudo o del caso) y en la lectura de vuelta; el volcado ya lee descripciones; el comparador exige igualdad | plantilla del script + comparador + pruebas RED→GREEN; probable A-n (toca el contrato de D-09/D-11); 1-2 Codex y 1 intento; SHA nuevo | solo si el Owner quiere prueba mecánica; el Architect decide si exige A-n |
| 2 — nada | procedencia solo procedimental, como en F4 | 0 | no |

## 5. A65-F4F-07 — evaluación de privacidad

**Exposición real:** el repositorio `marioap-afk/Calculadora_de_racks` es **público** (`visibility: public`, 0 forks). La rama
`architecture/interop-analisis-estructural` está publicada en `origin`, así que todo lo versionado en ella es accesible públicamente, también
en el historial.

### 5.1 Imágenes (solo en la rama de I-65; `main` no tiene ninguna)

Inventario de las 146 capturas `.jpg` de I-65 (abiertas una a una por un subagente de solo lectura): **30 con datos identificables**, 116 sin
ellos. Ninguna en `origin/main`. Se describen por categoría, sin transcribir los datos en este documento público:

| Categoría | Imágenes | Dónde | Sensibilidad |
|---|---|---|---|
| Nombre de **una tercera persona** (carpeta compartida de OneDrive anclada) y nombres de carpetas comerciales (cotizaciones, pedidos, técnico comercial) | 12 | diálogos «Abrir» de RAM en P2 (5) y W2 (7) | **alta**: dato personal de un tercero y de negocio |
| Nombre de la organización y nombre de pila del usuario (cuenta de OneDrive) | 12 (+4 truncadas) | mismos diálogos de RAM; diálogos de NETLOAD (truncado) | media |
| Cuenta de Autodesk en la barra de título de AutoCAD | 13 | primer y segundo smoke, W1 | media |
| Usuario de Windows en rutas | 4 | primer smoke (aviso de seguridad y consola) | baja (ya público en `main`) |
| Tipo de licencia de RAM (sin identificador) | ~41 | capturas de RAM | nula |

Commits de alta: `d6746d2b` (P2), `bbe113f5` (W2), `82a11c19` (primer smoke), `4d1f9a7c` (segundo smoke), `35550123` (W1).

### 5.2 Textos

- Usuario de Windows en rutas: 121 archivos de I-65 (sobre todo prompts y barridos de la carpeta `I-65-pilot`) y, ya en **`main`**, 185
  archivos (sobre todo los `.trx` de I-58).
- Nombre del equipo y usuario de dominio: 47 `.trx` de I-58, **ya en `main`**.
- Correo del autor: una línea en `close-1e7d8fc2/ci-run-39052279094.json` (metadatos de la API de GitHub; el mismo dato es público en los
  metadatos de los commits).
- Nombre del Owner como `verifiedBy`: decisión expresa del Owner (D-09), no es una fuga.

### 5.3 Propuesta de contención y corrección (prioritaria; nada se ha ejecutado)

| # | Acción | Quién | Efecto | Coste / riesgo |
|---|---|---|---|---|
| C-1 | **Hacer privado el repositorio ya** (GitHub › Settings › Danger Zone › Change visibility), o como mínimo antes del próximo push | Owner (ajuste de cuenta: no lo hace el Coordinator) | corta la exposición pública de **todo** (imágenes de la rama y textos de `main`) sin tocar SHAs | los minutos de Actions de un repositorio privado tienen cuota; contradice la regla «repositorios de CI públicos por defecto», que el Owner puede exceptuar |
| C-2 | **Redacción hacia delante:** un commit que sustituye las 30 imágenes por copias recortadas (ruta y lista de archivos; sin barra de título ni panel de navegación) y registra el mapeo SHA-256 original → redactado | Coordinator, con autorización expresa | quita los datos del árbol actual; **el historial sigue conteniéndolos** | no cambia SHAs ni la identidad del Candidato; insuficiente si el repositorio sigue público |
| C-3 | **Reescritura del historial** de la rama (eliminar o sustituir las 30 imágenes desde `82a11c19`) y push forzado; solicitud a GitHub para purgar vistas en caché | Owner (decisión) + Coordinator | eliminación pública real | **cambia todos los SHAs** desde la primera captura: el Candidato `5a5ca577`, el SHA de cierre `1e7d8fc2` y todas las referencias de decisiones y evidencia quedan rotos; obliga a re-declarar y re-evidenciar. Solo si el repositorio debe seguir público |
| C-4 | Desde ahora: capturas recortadas, sin panel de navegación, barra de título ni cuentas; rutas con `<usuario>`; no versionar JSON crudos de la API con correos | Coordinator | evita nuevas exposiciones | ninguno |
| C-5 | Valorar la notificación al tercero afectado y a la organización según la política interna | Owner | cumplimiento | — |

**Recomendación:** C-1 de inmediato, después C-2 y C-4. C-3 solo si el repositorio tiene que seguir siendo público. Hasta que el Owner
decida, el Coordinator **no versiona capturas nuevas**.

## 6. Requisitos para READY final

| Requisito | Contenido |
|---|---|
| Secuencia | F5 (OV-I65-02) → F7 (conformidad, regresión, entrada de FOUNDATIONS, guía de uso, ADR-0050) → READY-01..09 en orden (LIFECYCLE §8) |
| `FINAL_CANDIDATE_SHA` | evidencia completa sobre el SHA exacto (Core y UI Full, builds Debug, CI con `ui-tests`); **todas** las decisiones versionadas antes de READY-04 (la lectura de hijos documentales no vale para READY) |
| OV-I65-01..07 | **se repiten** sobre el Candidato final (Freeze §5); reducirlo exige al Owner |
| OV-I65-02 | se repite sobre el Candidato final |
| OV-I65-08 | lista completa de la guía (§7): NETLOAD y menú, dibujo, actualización en sitio, **round-trip tras reabrir el DWG** (exige guardar y reabrir un DWG desechable en la carpeta de la corrida: autorización expresa), multivista, BOM, persistencia/legacy y escenarios de I-65, con la **confirmación explícita del validador**; identidad de AutoCAD y de la biblioteca de bloques |
| Referencia manual | **A** (reutilizar volcados manuales custodiados: OV-01 de P2; OV-02 de F5): barato; exige identidades de RAM, API, bases y plantilla sin cambios y que el Owner la extienda a READY; debilidad A65-F4F-09. **B** (guardar modelos manuales de referencia y reabrirlos): permite volver a volcarlos, pero exige autorizar guardar y abrir modelos de RAM y custodiar archivos. **C** (reconstruir a mano en READY): máxima independencia; 30-40 min por modelo y riesgo de tecleo. **Recomendación: A con caída a C** si cambia cualquier identidad |
| Conflicto de censos con I-64 | `SelectiveEditorOpenTests` (38) y `CustomPropertiesCommandGuardTests` (lista de comandos) chocan con `RACKPANEL` de I-64: quien integre segundo pasa a 39 y añade el comando del otro. Si I-64 integra antes: rebase en READY-04, conflicto textual en pruebas = **cambio de código** (autorización y presupuesto) y SHA nuevo; si I-65 integra antes, lo asume I-64. Recomendación: fijar el orden de integración con el Owner antes de READY |
| A65-F4F-04 | opción 0 obligatoria en READY; opción 1 solo por decisión |
| Ventanas de host | AutoCAD (OV-01, 02, 03, 04, 08): 2-2.5 h; RAM (OV-01, 02, 04-07): 1.5-2 h con A (3 h o más con C); confirmaciones de I-64 relativas a su recepción y en su chat |

## 7. Presupuesto recomendado

| Recurso | Estado | F5 | F7 | READY |
|---|---|---|---|---|
| Codex | 50 de 52 (quedan 2) | 0 | 0 | 0 si no hay rebase con conflicto; 1 si hay que conciliar censos; +1-2 si se elige la opción 1 de A65-F4F-04 |
| `attempts` | 7 de 9 (quedan 2) | 0 | 0 | 0 / 1 / +1, en los mismos supuestos |
| Host | — | 2 ventanas (A ~30 min, B 90-120 min) | 0 | 2 ventanas largas |
| Architect | — | 1 revisión de cierre de F5 | 1 conformidad (READY-06 vendrá aparte) | READY-06 |

**Recomendación:** no ampliar topes para F5 ni F7. Para READY, decidir la ampliación (p. ej., Codex 56 y `max_attempts` 11) solo al
autorizarlo y solo si se elige la opción 1 de A65-F4F-04 o si el orden de integración obliga a conciliar censos con un Worker.

## 8. Plan mínimo de continuación

1. **Privacidad (urgente):** decisión del Owner sobre C-1 (privado), C-2 (redacción hacia delante) y C-3 (historial); hasta entonces, sin
   capturas nuevas versionadas.
2. **Decisiones para F5:** aprobar el conjunto de cargas §3.2 (incluida la fracción 1.0), el Candidato para F5 (reutilizar `5a5ca577` con
   extensión expresa, o SHA nuevo) y el operador de la referencia manual.
3. **Preparación de F5** (solo documentación): archivo de cargas y tabla de valores esperados versionados, comprobación en seco con las DLL
   congeladas, procedimiento de las dos ventanas con la opción 0 de A65-F4F-04 y capturas recortadas.
4. **Ejecución de F5** con autorización: ventanas A y B → OV-I65-02 → revisión del Architect → cierre de F5 (SHA de cierre con Core Full y CI).
5. **F7** (documentación) y **READY** con las decisiones del §6 (estrategia de referencia, orden de integración con I-64, presupuesto).
