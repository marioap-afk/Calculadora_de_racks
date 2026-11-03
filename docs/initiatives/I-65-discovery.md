# I-65 — Discovery F0 (Structural Analysis Interop Foundation + RAM Elements Adapter)

- Unidad: I-65 · rama `architecture/interop-analisis-estructural` · Claim-Id `0b4aad79-068e-4a57-a2c2-dc8f23406778`
- Base del código caracterizado: `origin/main` `bb0d5522e8411f66a51fdfb3f1f0d0514b737453`
- Fecha de verificación de fuentes externas y del entorno instalado: **2026-10-10**
- Autor: sesión Coordinator de I-65 (también redacta; su revisión de este Discovery es `SAME-SESSION ROLE`, ver
  [decisiones](../automation/decisions/I-65.md) §2). Investigación delegada en subagentes de solo lectura (CD-I65-G0-08).
- Clases de afirmación ([INITIATIVE_LIFECYCLE](../INITIATIVE_LIFECYCLE.md) §4): `MEASURED` (observado en código, archivo o entorno),
  `RECONSTRUCTED` (derivado de fuentes leídas), `INFERENCE`, `UNKNOWN`. Fuentes externas: `OFF-DOC` (documentación oficial de Bentley
  leída), `OFF-INST` (documentación oficial instalada con el producto), `OFF-KB*` (base de conocimiento oficial vista solo por fragmento
  de buscador), `3P` (terceros).

## 1. Conclusiones

1. **RAM Elements 2026 `26.00.01.015` está instalado** (`MEASURED`). Su única superficie **programática** soportada es la **API Python
   embebida** (`ramelements`, nueva en 2026): corre **solo dentro de la ventana de RAM Elements**, la lanza una persona desde la
   Python Console y no tiene línea de comandos, COM, ensamblados .NET ni modo sin interfaz (`OFF-DOC`, `OFF-INST`, `MEASURED`).
2. **Ningún camino es desatendido.** Toda transferencia exige un paso humano dentro de RAM Elements (ejecutar un script o importar un
   archivo). `RackCad → Analyze → Results` automático **no existe** y no se promete.
3. **La API Python es la mejor superficie para V1**: crea nodos, miembros, apoyos, liberaciones, offsets rígidos, orientación, casos y
   cargas; corre el análisis; lee desplazamientos, reacciones, fuerzas y ratios; y permite **leer de vuelta** lo creado (ejes locales
   incluidos), lo que hace posible verificar la equivalencia dentro de RAM. **No crea ni lee secciones ni materiales**: los referencia
   por nombre de la base de datos de RAM.
4. **OpenRE** (XML propio de RAM Elements, importar/exportar) es documentado pero **insuficiente**: el manual instalado es de 2024, sin
   esquema, con ejemplos erróneos, sin offsets rígidos, sin combinaciones y sin semántica de importación. Queda como alternativa no
   elegida. STAAD/SAP2000/DXF/ISM pierden hechos necesarios o exigen bibliotecas y SDK no documentados para terceros.
5. **La documentación se contradice en unidades** (la API declara un par fuerza-longitud único; la ayuda define grupos de unidades por
   magnitud, incluido el de offsets rígidos) y en nombres y extensiones de archivo. Regla del mandato: se registran ambas y se falla
   cerrado hasta medirlas con la sonda P1 en el entorno real.
6. **La cabecera Selectivo —candidato preferido del mandato— no está lista como fuente estructural**: sus filas de perfil no tienen
   propiedades de sección (las columnas `Ix`, `Iy` existen vacías), no hay material, no hay ejes centroidales y la geometría de
   miembros del BOM/preview no coincide con la dibujada (`MEASURED`).
7. **Cantilever es la única fuente con datos estructurales reales**: secciones AISC v16.0 con identidad estable
   (`StructuralSectionId`) y propiedades publicadas, colocación centroidal 3D por miembro y marcos locales explícitos (`MEASURED`).
   Ninguna fuente de RackCad tiene material estructural ni cargas utilizables para análisis.
8. **No existe código estructural ni de análisis** en el repositorio; ADR-0017 difiere la validación de cargas a RAM Elements
   (`MEASURED`). La interop **realiza** ese diferimiento; no lo reabre.
9. **Arquetipo NEW ARCHITECTURE confirmado**: M-07 activado (contrato transversal nuevo: modelo neutral + frontera de adaptadores).
10. **Ejecutar RAM Elements consume licencia** y cambia el perfil del usuario: toda medición en el entorno real queda condicionada a la
    autorización del Owner (CD-I65-G0-06).

## 2. Entorno instalado de RAM Elements (`MEASURED`, solo lectura, sin ejecutar el programa)

| Hecho | Valor | Cómo |
|---|---|---|
| Producto / versión | «RAM Elements 2026», `DisplayVersion 26.00.01.015`; `RAMElements.exe` `FileVersion 26.00.01.15` | registro `Uninstall`, `VersionInfo` |
| Instalación | `C:\Program Files\Bentley\Engineering\RAM Elements\`, 2026-08-28; 12 918 archivos | registro, `Get-ChildItem` |
| Python embebido | `python_runtime\` CPython **3.13.2** embebible (sin `site-packages` ni módulos `.pyd`: sin `_sqlite3`, `_ctypes`, `_ssl`; sin numpy ni pandas) + paquete `python\ramelements\` (`__version__ = "0.1.0"`, MIT) + ejemplos `PythonScripts\01..05_*.py` | archivos |
| Política de seguridad del intérprete | Cadenas dentro de `RAMElements.exe` (lectura de bytes, **no documentación**): bloqueo de `subprocess`, `socket`, `ctypes`, `threading`, `tempfile`, `shutil`, `pickle`…; `os.mkdir/makedirs/remove/rename/replace/system` anulados; `open()` de escritura aparentemente permitido. Si aplica a la consola: UNKNOWN (P1-E2) | bytes del binario |
| Servidor MCP | Cadenas del binario describen un servidor local para herramientas de IA (`run_script`, `get_model_summary`) tras una bandera de desarrollo: **no documentado ni soportado**; no se usa | bytes del binario |
| Modelos de ejemplo | 52 archivos `.retx`; el revisado es SQLite 3 | archivos |
| Manuales instalados (`OFF-INST`) | `Documentation\en\Manuals\`: «RAM Elements Python API User Manual.pdf», «OpenRE Manual.pdf», «RAM Elements Manual.pdf», «Examples», «Verification» | archivos |
| ISM | `RAMElementsISMFilter.dll`, `Bentley.Structural.Ism.Api.9.3.dll` (23.01.19.12); clave `HKLM\SOFTWARE\Bentley\ISM` | archivos, registro |
| Otros | «ISM RAM Structural System Plugin» 1.6.0.14 (es el enlace de RAM SS, no de RAM Elements); servicio de licencias de Bentley y `Bentley.Structural.PropertyCatalog.Server` en ejecución | registro, procesos |
| Bases de datos | `Database\Sections\<país>\*.sec` y `Database\Materials\<país>\*.mat` en texto tabulado (p. ej. `W 12X26`, `L 2X2X1_8`, `A992 Gr50`, `A36`) | archivos |
| Secciones de rack | **ninguna**: sin perfiles de poste perforado; `POST.sec` son postes de madera; hay perfiles AISI conformados en frío genéricos (`aisiCS…`) y macros `.leo` para secciones nuevas | archivos |

## 3. Superficies de integración de RAM Elements (pregunta primera del mandato)

| Superficie | ¿Existe? | Dirección | Hechos que transporta | Fuente | Veredicto para V1 |
|---|---|---|---|---|---|
| **API Python (`ramelements`) en la Python Console** | SÍ (2026, 26.0.0+) | lectura/escritura del modelo vivo | nodos (+ apoyos, resortes, masas), miembros (sección y material **por nombre**, giro, liberaciones de 10 GDL, offsets rígidos, punto cardinal, resorte rotacional M33), casos de carga y combinaciones, cargas nodales y de miembro, peso propio, análisis, diseño, resultados, ejes locales | U1–U8 (`OFF-DOC`); paquete instalado y ayuda «Python Console Interface Overview» (`OFF-INST`) | **ELEGIDA** (§3.1) |
| COM / modelo de automatización | NO encontrado | — | — | U2, U4, U30, U35; RAM DataAccess es de **RAM Structural System** (U48, `OFF-KB*`) | no aplica |
| Interop .NET (ensamblados, NuGet) | NO encontrado | — | — | U30, U35 (BDN sin RAM ni ISM) | no aplica |
| Línea de comandos / sin interfaz | NO encontrado | — | — | U4; el Python sin interfaz es de **RAM Concept** (U36) | no aplica |
| OpenRE (XML) | SÍ | importar (reemplazar/fusionar), exportar | nodos, apoyos, resortes, miembros por nombre, liberaciones, orientación (semántica ambigua), casos, cargas nodales y de miembro | U9, U1 (`OFF-DOC`); «OpenRE Manual.pdf» 2024-11-12, 30 págs. (`OFF-INST`) | alternativa no elegida (§3.2) |
| STAAD.Pro `.std` | SÍ | importar | geometría, secciones/materiales por tabla de conversión, apoyos, cargas, combinaciones | U10 | descartada: exige OpenSTAAD; liberaciones no documentadas |
| SAP2000 `.s2k` | SÍ | importar | geometría, secciones/materiales, casos, combinaciones | U11 | descartada: pierde ejes locales |
| DXF | SÍ | importar/exportar | solo geometría | U12 | insuficiente |
| ISM / iTwin Analytical Synchronizer | SÍ | importar/exportar | «información estructural»; cargas y liberaciones leídas de ISM: UNKNOWN | U14, U32, U33, ayuda instalada «ISM synchronization» | descartada en V1: sin SDK oficial documentado para terceros, versión UNKNOWN, revisión manual en el Synchronizer, dependencia externa (AGENTS «Dependencias») |
| Portapapeles / Excel | SÍ | copiar/pegar | **solo** registros de secciones y materiales | U13 | útil para que el ingeniero cree secciones de usuario; no transporta el modelo |
| Reportes | PARCIAL | salida | reportes de datos/análisis/diseño; «print to file» = imagen; diagramas a DXF | U17–U19 | no es un canal de resultados legible por máquina |

Fuentes (U-n): §11. Todas consultadas el 2026-10-10.

### 3.1 API Python — hechos que fundan la elección

- **Ejecución** (`OFF-INST`, ayuda «Python Console Interface Overview»; `OFF-DOC` U4-U7): consola con «Open Script…», «Run» (Ctrl+R) y
  «Run File…»; salida y tracebacks en la pestaña Output; ejecución síncrona; un script espera a un análisis en curso. El paquete exige
  correr dentro del proceso (`_ramelements_core` lo registra el puente Delphi/Python4Delphi; fuera de RAM lanza `RuntimeError`,
  `python\ramelements\_p4d\engine.py`).
- **Construcción** (`MEASURED`, fuentes del paquete): `nodes.add(x, y, z, restraints)`, `Restraints(dx..rz)`, `members.add(node_i,
  node_j, section, material, rotation, description)`, `Member.end_releases` (10 GDL), `rigid_end_offset_i/j`, `cardinal_point`,
  `rotational_stiffness_i/j` (M33), `get_local_axes()`, `add_point_load` (máximo 24 por miembro y caso), `add_distributed_load`,
  `Node.add_force`, `load_condition.add_load_case(id, desc, LoadCategory)`, `add_load_combination(id, fórmula, ComboType)`,
  `LoadCase.enable_self_weight`.
- **Secciones y materiales**: solo por nombre («must exist in the section database», `members.add`; «Must match a name in the model
  material database», `Member.material`). El puente no expone crear, editar ni leer propiedades de sección o material.
- **Análisis y resultados**: `m.analyze()` → `(exit_code, tiempo, warnings, errors, dinámico, no lineal)`; `results.node_displacement`,
  `node_reaction`, `member_end_forces`, fuerzas y desplazamientos en estaciones, `member_design`, modos y pandeo.
- **Archivo**: `save()`, `save_as(path)`, `new()` expuestos; `open()`, `close()`, `model.version` **no expuestos**
  («Not yet exposed in the Delphi P4D bridge»). Lectura sin RAM (`io.read_rme`) de archivos SQLite `.rme`; el manual oficial dice que el
  modelo nativo es `*.retx` (U27): **contradicción** de extensión (§5).
- **Madurez**: `__version__ = "0.1.0"`; varias funciones declaradas y no expuestas; la guía `python\docs\getting_started.md` contiene
  instrucciones de compilación Delphi y nombres que no coinciden con el paquete (`m.load_cases` frente a `m.load_condition`,
  `loads/load_case.py` frente a `load_condition.py`). La API es oficial y soportada como producto (notas de versión 26.0.0, U1), pero
  inmadura: el adaptador debe comprobar capacidades en tiempo de ejecución y fallar cerrado.
- **Manual oficial instalado** «RAM Elements 2026 – Python API User Manual» (38 págs., «Last Updated: May 15, 2026»; `OFF-INST`). Lo
  único que afirma con carácter general sobre unidades es que fuerzas, rigideces y desplazamientos van en las unidades activas del modelo
  (p.14); no define el eje vertical ni la regla de orientación de miembros verticales. Contradice al paquete en 17 puntos (§5, C-n).
- **Restricciones que condicionan al script** (`MEASURED` en el paquete; `OFF-INST`):
  - `Node.add_force` **reemplaza** la carga del nudo para ese caso (p.15): hay que sumar por nudo antes de llamar;
  - máximo **24** cargas puntuales por miembro y caso (p.19; `member.py:346`);
  - `members.add` crea el miembro y **después** asigna sección y material: un nombre rechazado puede dejar un miembro huérfano, y la API
    **no puede borrar** nudos ni miembros → el script solo corre sobre un modelo nuevo y, ante un fallo, el modelo se descarta;
  - categorías de caso: el paquete envía `str(category)`; con un `Enum` derivado de `str` en Python 3.13 eso produce
    `"LoadCategory.DEAD"`, no `"DL"` → pasar el texto (`"DL"`, `"OTHER"`), no el miembro del enum (C5, sin medir);
  - combinaciones: el código exige el tipo como entero `ComboType` y la fórmula `"1.2DL+1.6LL"`; el manual usa textos y otra sintaxis
    (C3, C4) — irrelevante para V1 (sin combinaciones);
  - resortes, masa, desplazamiento prescrito y piso rígido de nudo: documentados pero el código llama atributos inexistentes del puente
    (AttributeError esperado, C16) — fuera de V1;
  - resultados de extremo de miembro en ejes **locales** con convención de signos no definida — relevante solo para F6.

### 3.2 OpenRE — por qué no es la elegida

Del «OpenRE Manual.pdf» instalado (`OFF-INST`, 30 págs., portada «RAM Elements 2024», PDF del 2024-11-12): raíz `<Model Units="…">` con
12 pares de unidades globales; nodos, `Restraint`, `Spring`, `Member` (sección y material por nombre), `LocalAxisOrientation` (vector +
rotación, unidad del ángulo no declarada), `Releases`/`Longitudinal`, `LoadConditions`, cargas nodales y de miembro. **No documenta**
offsets rígidos, punto cardinal, combinaciones, la sintaxis de tablas de secciones/materiales, el comportamiento de importación
(coincidencia de IDs, nombres desconocidos, avisos) ni compatibilidad de versiones; sus propios ejemplos tienen IDs duplicados, XML mal
formado y nombres inconsistentes; no hay XSD. Inferir lo que falta desde exportaciones sería ingeniería inversa de un formato
propietario, que el mandato excluye como solución por defecto. OpenRE no aporta lectura de vuelta ni análisis.

### 3.3 Automatización de análisis y resultados (clasificación del mandato)

| Capacidad | Desde un script en la Python Console | Desde RackCad (proceso externo) |
|---|---|---|
| Crear/actualizar modelo | SUPPORTED NOW (sobre un modelo nuevo; secciones/materiales deben existir) | NOT EXPOSED |
| Guardar | SUPPORTED NOW por API (`save_as`); extensión y efecto: UNKNOWN hasta P1 | NOT EXPOSED |
| Pedir análisis | SUPPORTED NOW (`m.analyze()`) | NOT EXPOSED |
| Detectar terminación | SUPPORTED NOW (llamada síncrona) | NOT EXPOSED |
| Estado del solver | PARCIAL: código de salida y conteos de avisos/errores; textos de avisos: UNKNOWN | NOT EXPOSED |
| Recuperar resultados | SUPPORTED NOW (API) | SUPPORTED LATER: solo si el script los escribe en un archivo controlado con la huella de entrada (E/S de archivos en la consola: UNKNOWN hasta P1) |

| Resultado | Clase |
|---|---|
| Desplazamientos, reacciones, fuerzas y momentos de extremo, fuerzas en estaciones | SUPPORTED NOW dentro de RAM; SUPPORTED LATER hacia RackCad |
| Ratios de diseño | SUPPORTED NOW dentro de RAM; hacia RackCad: SUPPORTED LATER y sujeto a ADR-0017 (presentar veredictos en RackCad se evalúa contra M-08) |
| Avisos / errores del solver | conteos SUPPORTED NOW; textos UNKNOWN |
| Resultados vía ISM/iTwin | NOT EXPOSED |

**Veredictos:** automatización de análisis desde RackCad = **NOT EXPOSED**; análisis guiado por script dentro de RAM = **SUPPORTED NOW,
pendiente de demostración en el entorno real (P1)**. Importar resultados = **segunda capacidad, SUPPORTED LATER**, no bloquea V1.

## 4. Licencias y seguridad del entorno

- Suscripción por licencia (U30). La consola no declara licencia propia (UNKNOWN); ejecutar RAM Elements consume la licencia del Owner.
  RAM Connection viene apagado por defecto para evitar cargos (U27): el script no debe activar módulos con costo.
- La API escribe en el modelo **abierto**: un script ejecutado sobre un modelo del usuario lo modificaría. Regla de V1: el script exige
  un modelo **vacío** y aborta si no lo es; nunca abre, guarda encima ni borra archivos existentes; solo escribe en una carpeta de
  destino controlada que no existía.

## 5. Contradicciones y UNKNOWN de RAM Elements (fallan cerrado)

| ID | Tema | Fuente A | Fuente B | Tratamiento |
|---|---|---|---|---|
| RC-01 | Unidades | API: un par fuerza-longitud (12 códigos) para «all values» (`properties\units.py`) | Ayuda «Units»: grupos por magnitud (longitudes, dimensiones de sección **y offsets rígidos**, resortes, cargas puntuales, distribuidas, momentos, esfuerzos…) | P1-U1/U2 miden cada magnitud usada en V1; hasta entonces, sin offsets rígidos ni resortes en el contrato del adaptador |
| RC-02 | Cambio de unidades con datos | API: «does not rescale numerical data… RAM performs the conversion internally» | Ayuda: los datos existentes «will be automatically changed» | el script fija unidades **una vez**, sobre modelo vacío, antes de crear nada; P1-U1 mide |
| RC-03 | Eje vertical | Ejemplos oficiales: gravedad en `-Y`; manual: X, Y, Z elegibles, se recomienda elevar en Y (REM p.36) | Plantilla de peso propio: UNKNOWN | P1-A1 mide el vector de peso propio y los ejes locales |
| RC-04 | Ejes locales | `get_local_axes`: «local_2 — strong-axis / reference transverse axis» | Ayuda/REM p.37: eje 3 paralelo a XZ en horizontales y a Z en verticales | P1-A1 mide con giro 0° y 90° |
| RC-05 | Archivo nativo | `save_as` ejemplo `.rss`; `io` lee `.rme` (SQLite) | Manual: `*.retx` (U27) | P1-S9 guarda en carpeta desechable y registra lo creado |
| RC-06 | Nombres desconocidos | API: la sección «must exist» | Comportamiento ante un nombre inexistente: no documentado | P1-S1 lo mide; el script verifica por lectura de vuelta |
| RC-07 | Guía del paquete | `getting_started.md`: `m.load_cases`, `load_case.py`, Python 3.8–3.12 | Paquete: `m.load_condition`, `load_condition.py`; runtime 3.13.2 | se usa el paquete instalado; la guía no es autoridad |
| RC-08 | Eje vertical (C1) | Manual p.6, p.16, p.18: ejemplos con Z vertical | Manual p.18 (presión), p.24, p.28, p.36; paquete (peso propio `(0, −1, 0)`); ejemplos 02 y 05; ayuda del puente en el binario: **Y arriba** | hipótesis Y arriba; P1-A1 decide |
| RC-09 | Ejes locales (C2) | Manual p.6, p.22: local-2 débil, local-3 fuerte | `member.py:316-317` y binario: local-2 fuerte/referencia | P1-A1 mide; el script verifica con `get_local_axes()` |
| RC-10 | Categorías de caso (C5) | Ejemplos oficiales: miembros de `LoadCategory` | Python 3.13 convierte el miembro en `"LoadCategory.DEAD"` | el script pasa texto; P1-C5 mide ambas formas |
| RC-11 | Seguridad del intérprete | Manual: sin límites declarados | Binario: bloqueo de `os.makedirs`, `threading`… | RackCad crea la carpeta del paquete; el script solo usa `open()` sobre ella; P1-E2 mide |
| RC-12 | Excepciones | Manual p.37: `NotConnectedError`, `IndexOutOfRangeError`, `ModelNotSavedError`… | Código: cuatro de ellas nunca se lanzan; en su lugar `RuntimeError`, `ValueError`, `AttributeError`, `NotImplementedError` | el script captura cualquier excepción por paso y falla cerrado |

El inventario completo de discrepancias manual ↔ paquete (C1..C17) está en el informe de lectura del manual, resumido aquí en RC-07..12.
La sonda **P1** (§10) responde RC-01..06 y RC-08..11. Sin P1, el Freeze no puede congelar el contrato de unidades ni de orientación del
adaptador.

## 6. Fuente estructural de RackCad (`MEASURED` sobre `bb0d5522`, salvo indicación)

### 6.1 Capas

Cuatro proyectos de producto: Domain (puro, sin referencias) ← Application (puro, referencia Domain) ← UI (WPF) ← Plugin (único con
AutoCAD) (`docs/ARCHITECTURE.md:19-20,40-55`; `AGENTS.md` «Convenciones arquitectónicas» 1-2). Precedente de adaptador de salida puro
en Application: exportadores CSV/XLSX del BOM, OOXML escrito a mano sin dependencias (`docs/ARCHITECTURE.md:226-227`). Cero paquetes
NuGet en producto (`AGENTS.md` «Dependencias»). Herramienta no distribuida con precedente: `tools/RackCad.StructuralSections.Import`.
`NamespaceFolderGuardTests` exige namespace = carpeta (`tests/RackCad.Tests/NamespaceFolderGuardTests.cs:38`).

### 6.2 Selectivo — cabecera y frente

- Dominio de la cabecera en `src/RackCad.Domain/RackFrames/` (`RackFrameConfiguration`: `Height`, `Depth`, `PostPeralte`, parámetros
  de celosía en troqueles, `PasoTroquel=2.0`, `PanelClear=44.0`; `PostAssembly`, `FrameHorizontal`, `BracingPanel`, `FrameMember`).
- **Dos productores de geometría que no coinciden**: el «modelo físico» `BracingPanelMemberBuilder.RefreshPhysicalModel` (BOM y
  preview; coordenadas como razones de `Depth`, travesaños de longitud `Depth`, diagonales sin retranqueo de troquel) y el lateral
  dibujado `LateralHeaderLayoutBuilder.Build` (puntos de trabajo en las líneas de troquel, travesaño = `Depth − 2·2`, diagonales con
  retranqueo). Para la plantilla por defecto `SingleDiagonal`, las longitudes del BOM ≠ las dibujadas.
- No hay ejes centroidales del poste ni centro de cortante; los puntos de trabajo están en líneas de troquel tomadas del catálogo de
  conexiones; el origen del poste parece la cara exterior (`INFERENCE` desde `connection-layout.csv` y el comentario del builder).
- Largueros: `SelectiveLevel.Y` es la Y del troquel donde engancha, no el eje del larguero; la ménsula es dato de pieza
  (`mensulas.csv`), **sin rigidez** ni curva momento-giro; sin propiedades de sección de larguero.
- La `Tarima` del Selectivo solo tiene `Frente` y `Alto`: **sin peso** (`SelectivePalletDesign.cs:772-779`).

### 6.3 Secciones

- `assets/catalogs/secciones.csv` (catálogo legado de piezas de rack): columnas `Ix,Iy,norma,perforacion` **vacías en todas las filas**;
  sin consumidor en `src/` (`MEASURED`). El poste omega tiene solo ancho 3, fondo 3, espesor 0.0747, calibre 14. Lector tolerante: una
  fracción como `1 1/8` se carga en silencio como 0 (`CsvCatalogReader.cs:98-124`).
- **Catálogo neutral AISC v16.0** (ADR-0020/0021/0022/0023): 1011 secciones en `structural-sections-{w,hss-rect,c,l,s}.csv`,
  identidad estable `StructuralSectionId` = `{NS}-{FAMILIA}-{DESIGNACIÓN}` (p. ej. `AISC-W-W12X26`), propiedades publicadas (A, Ix, Iy,
  Zx, Zy, Sx, Sy, rx, ry, J, Cw…; nulas si la fuente no las publica), unidad nativa por fila. Lo consume solo Cantilever.
- Las piezas de rack se referencian por un id de catálogo legado (string, sin versión ni espacio de nombres). `secciones.csv` no se migra
  (ADR-0020: adopción por estrangulamiento, configurador por configurador).

### 6.4 Materiales

Ningún catálogo ni tipo trae Fy, Fu, E, G, ν ni densidad. `StructuralSectionDefinition.MaterialGrade` **nunca se infiere** y el
importador lo deja nulo (`StructuralSectionDefinition.cs:46-50`; el validador marca una inferencia). Las columnas `material` legadas están
vacías. No hay tabla calibre → espesor. **El material estructural tiene que venir de una autoridad nueva y explícita.**

### 6.5 Unidades de RackCad

ADR-0005: la pulgada es la unidad interna canónica; no hay unidad interna configurable; la guardia `INSUNITS` solo avisa. Conversiones
exactas existentes: `LengthUnits` (mm/in/ft → in) y `StructuralSectionUnits` («the ONE place where a unit is converted in the neutral
catalog», ADR-0021 §10: in↔mm, in²↔mm², lb/ft↔kg/m). **No existe** fuerza, momento, esfuerzo, masa, densidad ni carga distribuida como
cantidad tipada. La masa no tiene unidad interna única: `PalletSpecification.Weight` está en kg por defecto «and is NOT converted».

### 6.6 Cantilever (I-37A..D, integradas)

- Miembros sobre el catálogo AISC: `CantileverStructuralMemberPlan` = sección + rol + **una** autoridad de colocación
  `PrismaticSectionInstance` (marco local 3D, longitud, giro, espejo); `Start`, `End`, `Direction` se derivan (sin segunda verdad).
- Mundo: Z vertical (`CantileverColumnBaseDatum.FloorZ`), Y a lo largo de la base, X a lo largo de la línea. **El origen del marco es el
  centroide tabulado** de la sección (ADR-0022; `CantileverColumnBaseFrameResolver.ColumnFrame/BaseFrame`); la columna `DepthAlongBase`
  pone el peralte a lo largo de Y (flexión fuerte en el plano Y-Z).
- Roles: Column, Base, Arm (un brazo de canales pareados = dos miembros), Separator, Brace, ColdRolledAdapter. Ensambles puros:
  `CantileverLineResolver.Resolve` → `CantileverLineAssembly` (estaciones, intervalos, separadores, arriostramientos) y
  `CantileverStationAssembly.Members/Plates/Gussets/Punches`, con `Signature()` determinista.
- Para análisis: los extremos de miembro son **cortes físicos**, no nudos (la base empieza después de la placa trasera; el brazo nace en
  la cara de la columna): los nudos analíticos exigen una regla de idealización (offsets o nudos en el eje de la columna). El
  arriostramiento de barra redonda no tiene sección del catálogo (solo `RoundDiameter`). Sin material (`MaterialGrade` nulo).

### 6.7 Identidad, authored/effective y huella

- Identidad canónica: `RackEmbedDocument.Id` (GUID del rack lógico, Xrecord por definición; FOUNDATIONS «Rack Identity»). En Cantilever
  el `Id` del sobre y el `Line.Id` interior son distintos (FOUNDATIONS, comparadores AUTH-13).
- Authored → effective con variables de proyecto solo en Selectivo (FOUNDATIONS «Authored vs Effective»); los demás sistemas tienen
  puertos authored tipados.
- **No existe huella ni revisión del diseño de un rack.** Hay `SchemaVersion`, firmas deterministas (`CantileverLineAssembly.Signature`,
  receta del BOM, `HeaderBatchSignature`).
- I-63 (Computed Parameters & Project Summary): solo lectura, sin persistencia, métricas `frentes/frentesVacios`; no es un modelo
  estructural (el mandato lo prohíbe como tal).

### 6.8 Cargas

ADR-0017 (aceptado) difiere la validación a RAM Elements. I-37 excluye cálculo resistente y cargas (son I-38). Datos existentes: peso de
tarima en Dinámico y Push Back (`PalletWeight`); capacidad de rodillos «reservada»; **nada en Selectivo ni Cantilever**; sin tablas de
carga ni referencias RMI/MH16.1. Cualquier carga de V1 es **entrada explícita**, no un hecho del producto.

### 6.9 Código estructural previo

Ninguno: sin RAM/STAAD/ISM/FEA/Fy/módulo de elasticidad en `src/`. «Estructural» en el código significa el catálogo de secciones o el
comando `RACKSECCION`.

## 7. Rebanada vertical candidata

| Candidato | Propiedades de sección | Geometría analítica | Material | Cargas | Veredicto |
|---|---|---|---|---|---|
| (a) Cabecera Selectivo 2D (preferida por el mandato) | **No** (columnas vacías; poste perforado sin A/I/J/Cw) | Parcial: puntos de trabajo en troquel; sin centroide; BOM ≠ dibujo | No | No | **No lista**: exige antes una autoridad de sección de poste de rack, decidir qué geometría es la verdad y una regla de excentricidad |
| (b) **Una estación Cantilever** (columna + base + brazos) | **Sí** (AISC, id estable) | **Sí**: centroidal 3D por miembro; nudos por regla de idealización | No (perfil explícito) | No (caso de referencia explícito) | **Recomendada** |
| (c) Un frente Selectivo (2 cabeceras + largueros) | No | Débil (sin eje de larguero ni rigidez de ménsula) | No | No | No lista |

La elección final es decisión del Freeze, con el Owner (Q-O1 de la Proposal): (b) satisface el mandato («una rebanada pequeña y
estructuralmente comprensible»), pero no es el candidato que el Owner nombró primero.

## 8. Discovery Core (DC-01..09)

| ID | Resultado |
|---|---|
| DC-01 | No existe comportamiento estructural ni de interop (§6.9). Comportamiento a preservar: todo lo existente; la iniciativa solo añade. |
| DC-02 | Autoridades consumidas: identidad `RackEmbedDocument.Id` (ADR-0009); catálogo neutral de secciones (`StructuralSectionId`, ADR-0020/0021/0022/0023); colocación Cantilever (`PrismaticSectionInstance`, ADR-0022/0024/0025); unidades (ADR-0005, `LengthUnits`, `StructuralSectionUnits`); diferimiento ADR-0017. Sin autoridad para material, cargas ni supuestos estructurales: nacen aquí. |
| DC-03 | Persistencia: ninguna necesaria en V1. El sobre `RackEmbedDocument` no cambia; la provenance vive en los artefactos exportados (manifiesto), no en el DWG. |
| DC-04 | Camino propuesto: DWG (Plugin lee el sobre) → diseño authored → resolver puro (`CantileverLineResolver`) → **constructor del modelo estructural** (nuevo) → modelo neutral → **adaptador RAM** (nuevo) → paquete en destino controlado → script en RAM. |
| DC-05 | Consumidores nuevos de `CantileverLineAssembly`/`CantileverStationAssembly`/`StructuralSectionCatalog`: solo lectura. Lectores del sobre: el comando de exportación (Plugin). Ningún lector de DTO persistido cambia. |
| DC-06 | Protecciones existentes que se apoyan: pruebas de Cantilever (`Cantilever*Tests`), del catálogo de secciones, `NamespaceFolderGuardTests`, guardas de pureza de Application. Huecos: no hay pruebas de unidades de fuerza/momento ni de nada estructural (nacen con F1). |
| DC-07 | Archivos calientes: `docs/ROADMAP.md` (fila propia); `src/RackCad.Plugin/*Commands*.cs` si se añade un comando (archivo nuevo, sin editar los existentes); `assets/catalogs/*` solo por anexión si se versiona una tabla de mapeo (preferible archivo nuevo). Intersecciones activas: I-64 añade `Application/Workspace/*`, `Plugin/Workspace/AutoCadEventBridge.cs` y ADR-0047 (sin cruce de archivos con I-65); I-62 toca solo proceso y ADR-0048; I-52 sin `src`. Siguiente ADR libre: **0050**. |
| DC-08 | Fundaciones verificadas en la base: Rack Identity (STABLE), catálogo neutral de secciones (código y pruebas presentes; sin entrada en FOUNDATIONS), colocación Cantilever (código y pruebas), Computed Parameters (STABLE; no se consume en V1). Sin discrepancias de clase A. |
| DC-09 | **M-07 activado** (contrato transversal nuevo: modelo neutral y frontera de adaptadores de proveedor). M-01: no activado si la frontera de unidades **reutiliza** las constantes exactas de `LengthUnits`/`StructuralSectionUnits` (crear otra constante in↔mm sería segunda autoridad). M-02: no activado (sin persistencia). M-03: no (solo añade). M-04: introduce semántica de fallo nueva, propia. M-05/M-06: introduce un punto de extensión nuevo (proveedores), no modifica uno existente. M-08: no activado si ADR-0017 se consume; activado si RackCad presentara veredictos de diseño (fuera de V1). Arquetipo: **NEW ARCHITECTURE**. |

## 9. Expansiones (EXP-01..09)

| ID | Evaluación |
|---|---|
| EXP-01 | Activada **clase B** (no consumida): `ARCHITECTURE.md` dice 983 secciones AISC; el manifiesto dice 1011 (incluye S). `ideas-futuras.md:153` dice que los CSV «llevan columnas Ix/Iy/norma»: cierto solo de la cabecera. No afectan a V1 si la rebanada es Cantilever; se registran para el cierre. Requieren confirmación Coordinator + Architect. |
| EXP-02 | Activada y resuelta: dos productores de geometría de la cabecera (§6.2); queda fuera de V1 si se elige (b). Si el Owner eligiera (a), es STOP de diseño hasta decidir la autoridad. |
| EXP-03 | No activada: no hay persistencia legacy en el camino. |
| EXP-04 | Activada: el modelo neutral tendrá consumidores futuros (ID30, I-38, mezzanine); se acota con un punto de extensión explícito y sin implementarlos. |
| EXP-05 | Activada: varios invariantes candidatos (equivalencia en RAM, unidades por magnitud, ejes) no tienen modo de prueba sin el entorno real → sonda P1 + pruebas de contrato puras + OV. |
| EXP-06 | Activada: la API falla de forma no documentada ante nombres desconocidos (RC-06) → verificación por lectura de vuelta y fallo cerrado. |
| EXP-07 | Negativa razonada: ninguna rama activa toca catálogo de secciones, Cantilever ni unidades; I-64 añade Workspace sin estas autoridades. |
| EXP-08 | Activada (registro): el lector tolerante convierte fracciones a 0 en silencio en `secciones.csv`; fuera de V1 con (b); seguimiento. |
| EXP-09 | No requerida: DC-09 sin UNKNOWN material. |

Pregunta expresa del Coordinator («¿cuál debió activarse y no se activó?»): EXP-07 se revisó contra el diff completo de las tres ramas
paralelas (`git diff --stat origin/main...<rama>`): sin cruce. Se mantiene negativa.

## 10. Sondas en el entorno real (requieren autorización del Owner)

- **P1 — caracterización de la API** (`docs/automation/evidence/I-65-f0/ram-probe-p1.py`, instrumento de medición, no producto): sobre un
  modelo NUEVO y vacío, mide entorno y funciones expuestas (E1), política de seguridad, `hashlib` y escritura con `open()` (E2), unidades
  al cambiar el par y por magnitud (U1, U2), formas de la categoría de caso (C5), vector de peso propio y ejes locales con giro 0°/90°
  (A1), nombres de sección y material conocidos/desconocidos/conformados en frío/HSS y miembros huérfanos (S1), liberaciones, apoyos y
  offsets (R1), lectura de cargas de miembro (L1), equilibrio de reacciones (N1), **escala** por flecha de voladizo frente a
  `P·L³/(3·E·I)` y convención de signos de las fuerzas de extremo (N2) y guardado desechable `.retx` (S9). No crea carpetas: escribe solo
  en una carpeta nueva fuera del repositorio que el Coordinator crea antes y fija en `OUT_DIR`; si no puede escribir, imprime el informe
  en la consola. Tras la corrida, el modelo se descarta sin guardar (salvo la copia desechable de S9).
- **P2 — referencia manual** (estrategia de la Proposal): el ingeniero modela a mano en RAM la rebanada congelada; un script fijo de solo
  lectura vuelca el modelo abierto a JSON canónico para compararlo.

## 11. Fuentes externas (consultadas 2026-10-10)

Base A = `https://docs.bentley.com/LiveContent/web/RAM%20Elements-v2026/User%20Manual/en/Topic/re/Topic/Manual/`

- U1 notas de versión 26.0.0 (Python Console y `ramelements`, OpenRE con cargas al fusionar):
  `https://docs.bentley.com/LiveContent/web/RAM%20Elements%20Connection%20Design-v2026/Help/en/Topic/re/Topic/Help/c-re%20RAMElementsReleaseNotes.html` (OFF-DOC)
- U2–U8 Python Console (visión general, flujos típicos, ejecución, edición, apertura, interfaz, archivos): A + `c-re%20Python%20Console*.html` (OFF-DOC)
- U9 importar/exportar (lista completa, OpenRE = XML, OpenSTAAD): A + `c-re%20Importing%20and%20Exporting%20Data.html` (OFF-DOC)
- U10 STAAD.Pro; U11 SAP2000; U12 DXF; U13 secciones y materiales por portapapeles: A + páginas homónimas (OFF-DOC)
- U14 iTwin (antes ISM): `https://docs.bentley.com/LiveContent/web/RAM%20Elements-v2026/User%20Manual/en/topics/iTwin/c-rss%20Bentley%20iTwin.html` (OFF-DOC)
- U15 unidades; U17–U19 reportes; U25 macros LEO; U26 conformado en frío (AISI, sin RMI); U27 integración con RAM Connection (`*.retx`): A + páginas homónimas (OFF-DOC)
- U30 página de producto (interoperabilidad, ISM, precio; sin API ni racks): `https://www.bentley.com/software/ram-elements/` (OFF-DOC)
- U32 iTwin Analytical Synchronizer (gratuito): `https://www.bentley.com/software/itwin-analytical-synchronizer/` (OFF-DOC)
- U35 SDK de desarrolladores de Bentley (sin RAM ni ISM): `https://www.bentley.com/support/software-developers` (OFF-DOC)
- U36 Python sin interfaz de RAM Concept (producto hermano): docs.bentley.com, RAM Concept Help v16 (OFF-DOC)
- U41–U48 artículos de la base de conocimiento de Bentley (versiones 2024–2026, OpenSTAAD, RAM DataAccess, OpenRE incompleto): solo
  fragmentos de buscador (`OFF-KB*`); communities.bentley.com redirige a un portal que no se pudo leer
- U49 eng-tips, uso genérico de RAM Elements para racks (3P, fragmento)
- Documentación instalada (`OFF-INST`): «OpenRE Manual.pdf», «RAM Elements Manual.pdf» (REM), «RAM Elements Python API User Manual.pdf»,
  ayuda HTML de `Documentation\en\`, paquete `python\ramelements\` y ejemplos `PythonScripts\`.

## 12. Fricción del protocolo I-61 observada (para I-62)

- La investigación de F0 no es un gate de implementación: se hizo con subagentes de lectura fuera del ciclo de delegación de
  AUTOMATION_PLAN §16, sin Controller. Registrado como uso consultivo, no como delegación.
- Los portales de la base de conocimiento oficial de Bentley no son legibles por las herramientas de lectura web: las fuentes `OFF-KB*`
  quedan como fragmentos; el protocolo no tiene una clase de fuente «oficial no legible».

## 13. Adenda F0-R1 (tras la revisión 1 del Architect; las secciones 1-12 no se reescriben)

**DC-09 ampliado — M-08 y M-01 sobre las autoridades de unidades (A65-V1-05, A65-V1-07).**

| Autoridad | Qué decide | Evaluación con la Proposal V2 |
|---|---|---|
| ADR-0005 D1 | La pulgada es la unidad interna canónica; no hay unidad interna configurable | **Consumida.** V2 almacena longitudes en pulgadas; el modelo es derivado y no persistido. M-08 no activado |
| ADR-0040 D3 | `LengthUnits` es la única autoridad neutral de conversión de longitud | **Consumida.** Sin literal nuevo de longitud; G4 y `LengthUnitsAuthorityGuardTests` conviven |
| ADR-0021 §10 | `StructuralSectionUnits` es el único lugar de conversión de datos del catálogo | **Extendida** sin cambio de API ni de valores: consume el propietario único de la masa de libra (igualdad de bits probada). M-08 no activado; M-01 no activado (un solo propietario de 0.45359237) |
| ADR-0017 | Validación de cargas diferida a RAM Elements | **Consumida.** Sin ratios ni veredictos en RackCad (INV-11) |
| ADR-0009 | Identidad GUID del rack | **Consumida.** `RackId` = Id del sobre |

Conclusión: con la cláusula de compatibilidad de ADR-0050 (borrador), **M-08 no se activa**. Sin ella, M-08 sería UNKNOWN (cuenta como
activado). Arquetipo sin cambio: NEW ARCHITECTURE (M-07).

**Hechos verificados por el Architect que se incorporan** (`MEASURED` por el revisor):

- `CantileverColumnBaseDatum`: la columna arranca en `ColumnStartZ` (cara superior de su placa inferior) y la base apoya en el piso; cada
  estación está en coordenadas locales y la línea aplica un `Offset`; una estación bloqueada no produce geometría.
- Un brazo simple se centra por su caja (`−Bounds.Center.X`), no por su centroide: C y L quedan fuera del plano de la estación.
- Política de secciones de Cantilever: columna y base solo W; brazos de cualquier familia; brazos pareados solo canales.
- Base de datos de RAM (EE. UU.): `W 10X49` coincide con el catálogo de RackCad (A 14.4, Ix 272, Iy 93.4, J 1.39, Cw 2070); `Steel.mat`
  guarda E, ν, peso unitario, Fy y Fu (p. ej. A36 con ν = 0.26).
- Doble precisión: 241 de 2000 idas y vueltas in→m→in no son exactas; in→m→mm ≠ in→mm (p. ej. 0.1625 in).
- La rama de I-64 acota sus guardas de eventos y selección a `src/RackCad.Plugin/Workspace/`; el Plugin no usa `CommandClass`: un archivo
  de comando nuevo y aislado es compatible con ID30.
- 2 485 líneas de Application y Domain contienen «ram» en cualquier mayúscula; ninguna la palabra «RAM»: la guarda G1 necesita tokens
  sensibles a mayúsculas (Proposal V2 D-01).

**Sonda P1 v2** (A65-V1-02/03): puerta de modelo vacío y de `OUT_DIR` antes de la primera escritura (sin `SystemExit`, que en un
intérprete embebido podría cerrar la aplicación anfitriona); análisis primero, con apoyos completos; par provisional `LB_IN`; cobertura de
orientaciones de la rebanada (vertical, ±Z de RAM, inclinado en Y-Z, invertido, giro 0°/90°); `__file__`, directorio actual,
decodificadores, literal JSON ASCII, nombres de base de datos de otro país; valores por defecto de punto cardinal, offsets, factores de
rigidez, comportamiento axial y resortes. La línea base de casos de carga de File > New la **registra** la propia sonda.

## 14. Adenda F0-P1: resultados de la sonda en el entorno real (2026-10-11)

La sonda P1 v3.2 se ejecutó en RAM Elements 26.00.01.015 (resultados completos, matriz de capacidades y problemas en
[p1-results.md](../automation/evidence/I-65-f0/p1-results.md)). Las secciones 1-13 no se reescriben; esta adenda actualiza su estado:

| Contradicción / UNKNOWN de §5 | Resultado medido |
|---|---|
| RC-01/RC-02 unidades | el par `LB_IN` lee de vuelta exacto; el cambio de par por la API conserva los valores físicos; la escala de longitud, fuerza y momento es correcta; los pares métricos de RAM tienen error propio (−3.2·10⁻⁸ longitud, −5.22·10⁻⁶ fuerza) |
| RC-03/RC-08 eje vertical | **Y arriba** (peso propio `(0, −1, 0)`) |
| RC-04/RC-09 ejes locales | regla medida (12 direcciones × 3 giros): no vertical `l3 = unit(l1 × Ŷ)`, vertical `l3 = +Ẑ`, `l2 = l3 × l1`, giro dextrógiro; peralte en `l2`, flexión fuerte `M33` |
| RC-05 extensión de archivo | UNKNOWN (S9 retirado por condición del Owner); no la usa V1 |
| RC-06 nombres desconocidos | `ValueError` explícito **y miembro huérfano** |
| RC-10 categoría | el enum guarda `"LoadCategory.DEAD"` sin error: usar texto |
| RC-11 seguridad | `threading` bloqueado; `os.makedirs`/`os.replace` anulados; `open()` permitido; `__file__` no existe |
| Análisis y resultados | síncronos y legibles (solo caracterización); **escribir tras analizar abre un diálogo modal que bloquea el script** |

Clasificación §3.3 actualizada: «Pedir análisis» y «Recuperar resultados» dentro de RAM pasan de SUPPORTED NOW (documentado) a
**VERIFIED IN INSTALLED RAM**; desde RackCad siguen NOT EXPOSED. Ninguna falla de clase STOP. El candidato Cantilever de una cara sigue
siendo viable: todas las direcciones y nombres que usa están verificados.
