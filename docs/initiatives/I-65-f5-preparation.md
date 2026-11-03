# I-65 — F5 (Controlled Loads): preparación de OV-I65-02, contención de privacidad y trazabilidad (solo documentación)

Documento de la unidad (Coordinator, 2026-10-19), por la disposición del Owner «Privacy Containment / F5 Scope and Validation Preparation»
([texto literal](../automation/decisions/I-65-owner-disposition-privacy-containment-f5-preparation.original.txt); decisiones
[§86](../automation/decisions/I-65.md)). F5 PREPARATION = GO; F5 EXECUTION = NO; HISTORY REWRITE = NO. Sin AutoCAD, sin RAM, sin código,
sin capturas nuevas. Este documento **no** contiene datos personales ni de terceros (C-4): las categorías expuestas se describen sin
transcribir los datos.

## 1. Estado de privacidad y riesgos pendientes

| Elemento | Estado |
|---|---|
| Visibilidad del repositorio (C-1) | Comprobación de solo lectura el 2026-10-19 a las 00:46Z: la API de GitHub devuelve `private: false`, `visibility: public`, y una consulta anónima responde 200: **sigue público**. Queda pendiente la acción del Owner; el Coordinator la volverá a verificar solo con lecturas, sin tocar ajustes de la cuenta |
| Imágenes identificables | 30 de las 146 capturas de I-65, solo en la rama `architecture/interop-analisis-estructural` (ninguna en `main`) |
| Textos identificables ya públicos en `main` | usuario de Windows en rutas (185 archivos) y nombre del equipo con el usuario de dominio (47 `.trx` de I-58) |
| Riesgo mientras siga público | cualquiera puede ver las 30 capturas en la rama y en el historial; una redacción hacia delante (C-2) no las quita del historial |
| Nuevas capturas | ninguna versionada desde la decisión (C-4) |

### 1.1 Por qué 30 y no 29 (aclaración)

En el informe anterior, «12 + 13 + 4 = 29» sumaba categorías que se **solapan** y omitía las que solo tienen la organización truncada. La
partición exacta (cada imagen en una sola fila) es:

| Grupo disjunto | Imágenes | Contenido |
|---|---|---|
| G1 | 12 | diálogos «Abrir» de RAM (P2: 5; W2: 7): panel de navegación con el nombre de una tercera persona, carpetas comerciales, la organización y el nombre de pila del usuario |
| G2 | 13 | barra de título de AutoCAD con la cuenta de Autodesk (dos de ellas también con el usuario de Windows en una ruta, y una también con la organización truncada) |
| G3 | 2 | solo el usuario de Windows en una ruta (consola del primer smoke) |
| G4 | 3 | solo la organización truncada en el panel de «Choose .NET Assembly» (primer smoke, segundo smoke y W1) |
| **Total** | **30** | 12 + 13 + 2 + 3 |

### 1.2 C-5 — categorías que justificarían notificar (solo evaluación; ningún aviso enviado)

| Categoría expuesta | ¿Justifica notificar? | A quién (lo decide el Owner) |
|---|---|---|
| Nombre de una **tercera persona** asociado a carpetas de negocio de la organización (G1) | **Sí, es la más sensible**: dato personal de alguien ajeno a la iniciativa, en un contexto comercial | a la persona afectada y al responsable de privacidad o cumplimiento de la organización, según la política interna y la normativa de protección de datos aplicable |
| Nombres de carpetas comerciales (cotizaciones, pedidos, área técnico-comercial) (G1) | sí, como información de negocio | a la organización (titular de la información), internamente |
| Identificador de la cuenta de Autodesk de la organización (G2) | recomendable | al administrador de la cuenta / TI (riesgo de suplantación o de intentos de acceso) |
| Organización y nombre de pila del usuario (G1, G4) | interno | al propio Owner y a la organización |
| Usuario de Windows, nombre del equipo, usuario de dominio (G2, G3; `main`) | bajo, interno | a TI, si la política lo exige |
| Tipo de licencia de RAM | no | — |

No se expusieron contraseñas, credenciales, datos financieros, identificadores oficiales ni datos de salud.

## 2. Plan de tratamiento de evidencias sin reescribir historial (C-2, preparado, no ejecutado)

1. **Precondición:** decisión del Owner sobre C-1. La redacción hacia delante **solo** contiene la exposición si el repositorio es privado: el
   historial conservará las originales.
2. **Lista cerrada de archivos** (30; [solo rutas y grupo, sin datos](../automation/evidence/I-65-f5-prep/redaction-candidates.txt)):
   G1 = 12 capturas de `p2-R20261016T2147Z/captures/` y `w2-R20261017T0847Z/captures/`; G2 = 13 de `smoke-745653e1/`, `smoke2-51fc82f1/` y
   `w1-R20261017T0847Z/captures/`; G3 = 2 de `smoke-745653e1/`; G4 = 3.
3. **Redacción por grupo**, preservando la parte técnica:
   - G1: rectángulo opaco sobre el panel de navegación izquierdo del diálogo; quedan la barra de ruta, la lista de archivos, «Nombre de
     archivo» y los botones, que son la evidencia (ruta y script seleccionados).
   - G2: rectángulo opaco sobre el extremo derecho de la barra de título (zona de la cuenta); quedan el nombre del dibujo y la interfaz.
   - G3 y las dos de G2 con ruta: rectángulo solo sobre el segmento del usuario dentro de la ruta; el resto de la ruta y la consola quedan.
   - G4: rectángulo sobre el panel de navegación del diálogo de NETLOAD; quedan la ruta, el archivo y el aviso.
4. **Herramienta:** script local fuera del producto (PowerShell con `System.Drawing`), con coordenadas por imagen fijadas a mano y revisadas en
   una vista previa; nunca reescribe el original en su sitio fuera del commit.
5. **Verificación:** cada copia redactada se revisa visualmente (datos tapados; resultado técnico legible); un segundo revisor (el Architect,
   solo lectura) contrasta las 30.
6. **Registro:** un índice nuevo de redacción (ruta, SHA-256 original, SHA-256 redactado, grupo, motivo), **sin** los datos. Las tablas
   históricas de la evidencia **no** se editan: el índice explica por qué el archivo actual ya no coincide con el SHA-256 citado.
7. **Commit único hacia delante**, sin force-push, sin borrar ramas y sin tocar los SHAs custodiados.
8. **Textos (opcional, a decidir):** el JSON crudo de CI de `close-1e7d8fc2/` con el correo del autor podría sustituirse por una versión sin los
   bloques `author`/`committer` (el mismo dato ya es público en los metadatos de los commits); las rutas con usuario se dejan tal cual (baja
   sensibilidad y ya públicas en `main`) y se evitan en adelante.
9. **Coste:** 0 Codex, 0 intentos; unos 45-60 minutos.

## 3. Matriz exacta de las seis cargas de OV-I65-02

Archivo: [`ov-i65-02.loads.json`](../automation/evidence/I-65-f5-prep/ov-i65-02.loads.json) (esquema `rackcad-structural-loads/v1`;
SHA-256 de los bytes LF `d675d9f6bb537083a63571f08fb9884c1ab085a0eca4ad1555ee98bea4f8ba4c`, registrado en la evidencia §90). Estación `REF-CANT-01`, estación 0, caso `REF1`, lado `PositiveY`, dirección `-Z`.

| id | nivel / miembro | fracción | magnitud (unidad del archivo) | relación con D-10b |
|---|---|---|---|---|
| Q1 | 1 / `ARM.1.PositiveY` | 0 | 2000 N | unidad `N`; extremo inferior del intervalo [0, 1] |
| Q2 | 1 / `ARM.1.PositiveY` | 1 | 1.5 kN | unidad `kN`; extremo superior; segunda carga del mismo miembro |
| Q3 | 2 / `ARM.2.PositiveY` | 0.25 | 750 lbf | `lbf`; fracción interior |
| Q4 | 2 / `ARM.2.PositiveY` | 0.9 | 1 kip | `kip`; segunda carga del miembro |
| Q5 | 3 / `ARM.3.PositiveY` | 0.5 | 300 kgf | `kgf`; fracción interior |
| Q6 | 3 / `ARM.3.PositiveY` | 0.75 | 4.5 kN | `kN`; segunda carga del miembro |

FRACTION 1.0 = RETAIN (Owner). Si RAM no admite o no reproduce la carga en el extremo: hallazgo y STOP de la validación correspondiente.

## 4. Valores, transformaciones y tolerancias esperados

**Transformaciones (D-10b, IDL-06, D-03):** distancia desde `ARM.k.PositiveY.J` = 6.354218840504721 + fracción × 36 in; porcentaje del
miembro analítico = 100 · distancia / 42.35421884050472; fuerza en lbf con 1 lbf = 4.4482216152605 N y 1 kgf = 9.80665 N; el adaptador
emite `GLOBAL_Y` con signo menos (eje vertical de RAM, D-04).

| id | distancia (in) | porcentaje | valor (lbf) | entrada manual (kip; % del miembro) |
|---|---|---|---|---|
| Q1 | 6.354218840505 | 15.002564123383 | −449.617886199421 | −0.449617886 kip; 15.002564 % |
| Q2 | 42.354218840505 | 100.000000000000 (el producto emite 99.99999999999997) | −337.213414649566 | −0.337213415 kip; 100.000000 % |
| Q3 | 15.354218840505 | 36.251923092537 | −750.000000000000 | −0.750000000 kip; 36.251923 % |
| Q4 | 38.754218840505 | 91.500256412338 | −1000.000000000000 | −1.000000000 kip; 91.500256 % |
| Q5 | 24.354218840505 | 57.501282061692 | −661.386786554633 | −0.661386787 kip; 57.501282 % |
| Q6 | 33.354218840505 | 78.750641030846 | −1011.640243948697 | −1.011640244 kip; 78.750641 % |

**Comprobación en seco (preparación, no evidencia de OV):** con las DLL congeladas del Candidato `5a5ca577`, fuera de AutoCAD y de RAM, el
modelo de REF-CANT-01 con este archivo es `Valid` sin diagnósticos y el paquete construido **en memoria** (sin publicador; no se escribió
nada) lleva estas seis cargas: **6/6 iguales a la tabla** (diferencias ≤ 1.4e-14 en el porcentaje, 0 en el valor); 8 nudos, 7 miembros,
`LB_IN`, caso `REF1` `OTHER`. La Q2 queda dentro del miembro (99.99999999999997 %, no supera el 100 %).

**Tolerancias (sin cambios; Freeze §4 y paquete del producto):**

| Comprobación | Tolerancia |
|---|---|
| Lectura de vuelta del generado (script) | relativa 1e-9; porcentaje 1e-9; ejes 1e-9 |
| Comparador, fila `LOADS` (generado frente a manual) | valor 0.1 %; posición 0.02 in; dirección exacta; emparejamiento por miembro y caso, por la posición más cercana |
| Resto de filas del comparador | las de OV-I65-01 (coordenadas 0.02 in; ejes 0.1°; nombres exactos; combinaciones 0 en ambos, CMB-3) |
| Entrada manual | kip con 9 decimales (error ≤ 5e-10 kip) y % con 6 decimales (≤ 2e-7 in): muy por debajo de las tolerancias |

## 5. Procedimiento propuesto del modelo manual (independiente; no ejecutado)

- **Fuente:** la especificación congelada (`docs/initiatives/I-65-f4-ref-cant-01-manual-procedure.md`) para la geometría y la tabla §4 para
  las cargas. **No** se usa el volcado manual de P2 ni la carga útil del paquete.
- **Hoja de entrada:** [`ov-i65-02-manual-entry-sheet.json`](../automation/evidence/I-65-f5-prep/ov-i65-02-manual-entry-sheet.json),
  generada por el generador de P2 (solo desde la especificación) más las cargas de §4 (fórmulas de D-10b sobre el archivo de cargas). Su
  geometría es idéntica a la hoja de P2 (comprobado).
- **Pasos en RAM** (cuando se autoricen; mismos que P2 §6, con las lecciones de P2):
  1. Modelo nuevo y vacío; unidades de la interfaz sin cambiar (Inglés: ft, kip).
  2. 8 nudos con las coordenadas en ft de la hoja; apoyos: `COL.FOOT` empotrado, el resto Tx, Ry y Rz.
  3. 7 miembros (conectividad, `W 12X26`/`A992 Gr50` con giro −90 en la columna, `HSS_SQR 4X4X1_4`/`A500 GrC rectangular` con giro 0 en los
     brazos), sin liberaciones.
  4. Estado de carga `REF1` (categoría `DL`, porque la interfaz no ofrece «Other»; informativa), sin peso propio ni combinaciones.
  5. Seis cargas puntuales en miembro, en %: Q1 y Q2 en el miembro 5, Q3 y Q4 en el 6, Q5 y Q6 en el 7, con los valores de la columna
     «entrada manual» de §4 y dirección global vertical negativa.
  6. Revisión contra la hoja (no contra el generado); una corrección controlada, como en P2.
  7. Volcado con su propio script en una carpeta nueva; descarte sin guardar.
- **Gotchas de P2:** celdas en modo edición; trozos ≤ 10 caracteres y sin portapapeles; doble clic en las casillas; doble clic en el icono de
  la carpeta; capturas recortadas (C-4).
- **Si RAM no acepta el 100 % en la entrada manual:** hallazgo y STOP de la validación (no se sustituye el valor).

## 6. Conformidad de reutilización del Candidato con LIFECYCLE

**LIFECYCLE no permite expresamente reutilizar el Candidato de hito de F4 para F5.**

- §8: «Un hito intermedio del Owner requiere **su propio Candidato completo** y no sustituye el Candidato final.»
- READY-02/03, en su lectura para un hito: los documentos y las decisiones de ese hito deben estar versionados **en** el Candidato. Las de
  F5 (este documento, el archivo de cargas, la hoja manual, las decisiones §85-§86) viven en commits posteriores a `5a5ca577`. Usarlo exigiría
  una **nueva** excepción documental, que el Owner ha pedido no extender automáticamente.

**Alternativa conforme (no implementada):** un **Candidato de hito de F5** propio:

1. cuando la preparación de F5 esté aprobada, un commit documental con todo lo de F5 (producto sin cambios: árbol de `51fc82f1`);
2. árbol limpio → Core Full y UI Full locales → builds Debug de UI, Plugin y herramienta (salida en `D:\r65-smoke\<sha8>\`) → push → CI
   `push` del SHA exacto con `ui-tests`;
3. conformidad del Architect y del Coordinator; acto de declaración del Owner (bloque de la guía §7.1);
4. nueva DLL (el SHA va estampado) y nuevos valores esperados del validador v2 (con el archivo de cargas de OV-I65-02).

Coste: unos 30 minutos locales más el CI; 0 Codex, 0 intentos.

## 7. Plan de trazabilidad de sesión (A65-F4F-04, opción 0, aprobada para planificación)

Cadena de custodia **externa** (scripts fuera del producto; ningún cambio de código):

| Eslabón | Cómo se registra | Herramienta |
|---|---|---|
| Identidad de RAM y de su API | SHA-256 y versión de `RAMElements.exe`; manifiesto del árbol de `ramelements` y `__version__` | [`session-custody.ps1.txt`](../automation/evidence/I-65-f5-prep/session-custody.ps1.txt), fase `session-start` |
| PID e instante de inicio de la sesión | PID y `StartTime` de la **única** `RAMElements.exe` | `session-start` (falla si hay 0 o más de una) |
| Ruta y hash del script ejecutado | justo antes de «Ejecutar archivo…»; salida aún inexistente | fase `pre-run` (falla si el hash no es el esperado o la salida existe) |
| Huella del paquete | `analysisInputFingerprint` del manifiesto y de la lectura de vuelta | `post-run` sobre `ram-readback.json` |
| Fecha y hash de la lectura de vuelta | `CreationTimeUtc`, `LastWriteTimeUtc`, SHA-256; mismo PID y mismo inicio de RAM | fase `post-run` |
| Fecha y hash del volcado | ídem | fase `post-run` |
| Relación entre ambos y su sesión | inicio de RAM ≤ creación de la lectura de vuelta ≤ creación del volcado ≤ cierre de RAM, mismo PID; y **contraste de contenido** lectura de vuelta ↔ volcado | [`readback-dump-contrast.py.txt`](../automation/evidence/I-65-f5-prep/readback-dump-contrast.py.txt) |

**Pruebas sin RAM:** el contraste da **IGUAL** con la lectura de vuelta y el volcado versionados de OV-01 en W2, y **DISTINTO** en cuatro
variantes negativas (carga alterada, coordenada alterada, miembro de más en el volcado y lectura de vuelta de otro paquete); el registro de
sesión rechaza `session-start` sin RAM y registra `post-run` con la ruta sin usuario.

**Límite declarado:** marcas de tiempo, PID y contraste de contenido **no** son una prueba criptográfica de procedencia: dos sesiones con la
misma geometría producen el mismo contenido (W2 = P2 byte a byte) y los tiempos de archivo pueden alterarse. Son evidencia procedimental
reforzada. La opción 1 sigue sin autorizar.

## 8. Ventanas necesarias de AutoCAD y RAM

| Ventana | Contenido | Duración |
|---|---|---|
| W3 — AutoCAD | preflight; NETLOAD único de la DLL del Candidato de F5; REF-CANT-01 en un DWG desechable; `RACKRAMPAQUETE` con `ov-i65-02.loads.json` → `pkg-OV02` (un intento); validador v2 con los valores esperados nuevos (16/16); contraste de la carga útil con §4 (6/6); cerrar sin guardar; POST | ~40 min |
| W4 — RAM | `session-start`; `pkg-OV02` en un modelo vacío → `READBACK OK` (pre/post-run); volcado del generado en la misma sesión (pre/post-run); descarte; modelo manual nuevo (§5) → volcado (pre/post-run); descarte; `session-end`; fuera de RAM: comparador y contraste | ~2 h (el modelo manual son 40-60 min de tecleo) |

En las dos: confirmación expresa del Coordinator de I-64 recibida en su chat, ventana relativa a la confirmación, nadie usando el equipo,
avisos previo y de cierre, y **capturas recortadas** (C-4): solo la consola, las rejillas, la barra de estado y la barra de ruta con la lista
de archivos; nunca la barra de título, los paneles de navegación ni la barra de tareas.

## 9. Requisitos para la revisión y el cierre de F5

| # | Criterio propuesto |
|---|---|
| AC-F5-01 | Orden del Owner con alcance (solo OV-I65-02) y presupuesto (Codex 50/52, `attempts` 7/9, 0 Workers) |
| AC-F5-02 | Freeze y ADR-0050 sin cambios; sin A-n |
| AC-F5-03 | Candidato de hito de F5 completo (LIFECYCLE §8) y DLL según la guía |
| AC-F5-04 | `pkg-OV02` `Valid`, 16/16, carga útil = §4 (6/6) |
| AC-F5-05 | **OV-I65-02 PASS:** `READBACK OK` con la huella; volcados `DUMP OK`; comparador **PASS** con la fila `LOADS` en PASS (valor 0.1 %, posición 0.02 in, dirección exacta) y las demás filas como en OV-I65-01; `manualDumpSha256` del modelo manual nuevo en el informe; Q2 en el 100 % reproducida (si no: hallazgo y STOP) |
| AC-F5-06 | Cadena de custodia de la opción 0 completa y contraste lectura de vuelta ↔ volcado IGUAL |
| AC-F5-07 | Evidencias conformes a C-4 (sin datos identificables) |
| AC-F5-08 | Producto sin cambios; ningún modelo de RAM guardado; sin análisis |
| AC-F5-09 | Revisión del Coordinator y del Architect sin REQUIRED |
| AC-F5-10 | LIFECYCLE §7: commit de cierre → árbol limpio → Core Full local → push → CI del SHA exacto → GATE PASS registrado después |

## 10. Decisiones pendientes del Owner

1. **C-1:** hacer privado el repositorio (acción del Owner); el Coordinator lo verificará después.
2. **C-2:** autorizar la redacción hacia delante (§2) y decidir sobre el JSON crudo de CI.
3. **C-5:** decidir las notificaciones según §1.2.
4. Aprobar la tabla de valores y tolerancias (§3-§4).
5. **Candidato de F5:** aceptar la alternativa conforme (Candidato de hito propio, §6) o, si lo prefiere, conceder una excepción expresa
   para `5a5ca577`.
6. Autorizar el operador del modelo manual (Claude con Computer Use) y la cadena de custodia de §7.
7. Autorizar la ejecución de F5 (ventanas W3 y W4), con la coordinación de host habitual.
