# I-65 F3 — revisión de conformidad del Architect (custodia)

- Rol: Architect de I-65, `SAME-SESSION ROLE` (subagente de contexto independiente del Coordinator), solo lectura.
- Fecha: 2026-10-11. Alcance: `git diff bf2d3478..e3ae36d0 -- src tests assets` (F3-T1 `16fa9e76` y F3-T2 `e3ae36d0`) sobre `HEAD`
  `56587993`, contra la Proposal V3.1 (Frozen), ADR-0050, la orden del Owner para F3, `I-65-f3-ram-elements-adapter.md`, CD-I65-F3-01..15,
  los hechos de P1 y la fuente del paquete instalado de `ramelements` (leída, sin ejecutar). Sin compilar ni ejecutar nada.
- Texto: transcripción fiel del informe del subagente (agentId `ad11b50c1351b8cb2`).

## Veredicto

**CONFORMANT WITH OBSERVATIONS.** Ningún hallazgo REQUIRED ni violación del Freeze; 12 observaciones OPTIONAL, dos de las cuales
(A65-F3-01 y A65-F3-02) conviene resolver antes del Candidato de hito de F4 porque podrían producir un `FAILED` en falso en la primera
corrida real.

## Respuestas (resumen fiel)

1. **Freeze, ADR-0050 y fronteras:** conforme. Domain, Plugin, UI y `.csproj` intactos; lista de permiso de G1 vacía; sin tokens del proveedor
   fuera de `Providers/RamElements`; el modelo neutral sin tipos de RAM (los nombres de RAM solo viajan como texto canónico opaco hasheado);
   el adaptador devuelve bytes sin E/S (guarda nueva); el escritor `StructuralPackagePublisher` es neutral (D-01, D-08, INV-05). Sin S-02.
2. **Tabla de mapeo:** conforme. Autoridad única (`RamSectionMaterialTable`), reforzada por `MappingStale` → `Invalid`; prefijos de longitud
   en la fila completa y en el texto neutral; validación estricta; filas `proposed` no asignan; el archivo de datos solo trae las dos filas
   `proposed` sin valores de ingeniería.
3. **Traducción:** conforme; golden de §8 recalculado y correcto: coordenadas `(X, Z, −Y)`; banderas `dx=Tx, dy=Tz, dz=Ty, rx=Rx, ry=Rz,
   rz=Ry` (resuelve CD-I65-F1-19 a; b documentada sin cambio); regla de ejes y giro igual que P1 §4 (columnas θ = −90°, brazos θ = 0°);
   cargas en porcentaje y `GLOBAL_Y` negativas; admisión de orientación (vertical o ≤ 10°).
4. **Plantilla:** conforme. Puertas de capacidad y de modelo vacío, apertura exclusiva de la lectura de vuelta y solo después la primera
   escritura (`units.set`); categoría como texto; material explícito; `CENTROID`; valores por defecto solo leídos; tolerancias de la regla 9;
   sin save, save_as, new, analyze, os, sys ni SystemExit. Las 35 funciones del núcleo exigidas están en las 207 de P1. API no medida y
   riesgo: `Restraints` parcial (mismo puente, bajo); `members.add(rotation=)` (mismo camino que el setter medido, bajo); lectura de un giro
   negativo (A65-F3-01) y de una descripción larga (A65-F3-02) no medidas.
5. **Manifiesto y huellas:** conforme (supuestos con limitación, advertencia literal, exclusiones, SHA-256 de script y cargas,
   `AnalysisInputFingerprint` con la versión del adaptador y repetida en la carga útil y en la lectura de vuelta; determinista).
6. **Validación previa y fallo cerrado:** conforme (sin ruta a contenido parcial; ruta de Windows absoluta y normalizada; hash de cargas
   ligado al modelo; tabla obsoleta → `Invalid`).
7. **Escritor atómico:** conforme (nunca sobrescribe; temporal hermana con `CreateNew` y un único `Directory.Move`; limpieza y aviso
   `CleanupFailed`; la carrera del destino que aparece entre comprobación y renombrado falla sin tocarlo, sin prueba específica).
8. **Guardas:** conforme ((a)-(c) solo en el RED `1c1e8650`; exención exacta de INV-14; G1 vacía; guardas nuevas en archivos nuevos);
   ver A65-F3-05.
9. **A65-F1-07:** cerrado (limitaciones neutrales; ASM-PROP-01 corregido).
10. **Alcance:** sin desborde a F4-F6, interfaz, Plugin, análisis ni resultados; nada falta de la fila F3 ni de los ítems 1-10 del Owner.
11. **Disposiciones:** CD-I65-F3-11, 12, 13 y 14 aceptables (las 35 funciones verificadas por el Architect frente a P1; el simulacro del
    Coordinator es evidencia fuera de la suite y no choca con INV-14).

## Hallazgos (todos OPTIONAL)

| ID | Enunciado | Evidencia | Disposición recomendada |
|---|---|---|---|
| A65-F3-01 | La lectura de vuelta exige `rotation == −90` en las columnas; P1 nunca leyó un giro negativo; si RAM lo normaliza (p. ej. 270) habrá un `FAILED` en falso; la regla 9 no exige el giro (los ejes ya lo verifican) | `RamBuildScriptTemplate.cs:210` | Antes del Candidato de F4: comparar módulo 360 o quitar la comprobación |
| A65-F3-02 | Se exige descripción del miembro = id (15-16 caracteres); solo se midió leer `"N2"`; un truncado da `FAILED` en falso | `:221` | Relajar o quitar, o medir en la primera corrida |
| A65-F3-03 | «FAILED — DISCARD MODEL» también en rechazos de puerta que no tocaron el modelo | `:268-285` | Guía de F4: «gate:» = modelo intacto; opcional estado de rechazo aparte |
| A65-F3-04 | `json.dumps` y la escritura de la lectura de vuelta fuera del `try`: un fallo deja el archivo vacío | `:279-284` | Envolverlos con estado de respaldo |
| A65-F3-05 | INV-14 esquivado partiendo cadenas en dos archivos de prueba no eximidos (contra la letra de CD-I65-F3-05 b, cláusula no copiada al contrato); la invariante sigue intacta | `StructuralAnalysisRamElementsGuardTests.cs:158`; `StructuralAnalysisRamElementsAssumptionsTests.cs:54` | Disposición expresa del Coordinator o mover esos literales a archivos eximidos |
| A65-F3-06 | La capa neutral escribe archivos con un nombre elegido para no disparar la prueba de F1 que busca «Writer»; el nombre de esa prueba ya es inexacto | `StructuralAnalysisModelBuilderTests.cs:557-572` | Edición autorizada en F4: E/S neutral solo en `StructuralPackageWriter.cs` |
| A65-F3-07 | Carpeta embebida y destino del escritor son argumentos independientes | `RamBuildPackageFactory.cs:29-35,257` | F4: una sola cadena y prueba de igualdad |
| A65-F3-08 | La regla de carpeta admite segmentos que terminan en punto o espacio y nombres reservados (`CON`, `NUL`) | `RamBuildPackageFactory.cs:292-300` | Endurecer o documentar |
| A65-F3-09 | Sin prueba de la carrera del destino entre comprobación y renombrado | `StructuralAnalysisRamElementsPackageWriterTests.cs` | Añadir con el sistema de archivos falso |
| A65-F3-10 | La lectura de vuelta no compara la categoría ni los nudos de cada miembro (CD-I65-F3-14) | `RamBuildScriptTemplate.cs:243-245` | Volcado y comparador de F4 |
| A65-F3-11 | Comprobaciones de la carga útil en la plantilla (sección/material vacíos, dirección) después de escribir los nudos (C# ya las garantiza) | `:129-130,143-144` | Moverlas antes de la primera escritura |
| A65-F3-12 | El constructor público de `StructuralMaterialProfile` sigue en el producto; la autoridad única se impone al crear el paquete | `StructuralMaterialProfile.cs:146-171` | Guarda en F4: el comando usa solo la tabla |

## Riesgos para la primera corrida real en RAM (F4), de más a menos probables

1. Ninguna exportación es válida hasta que un ingeniero verifique las dos filas (precondición segura).
2. Ruta y reintento: el paquete solo funciona en su carpeta embebida y la lectura de vuelta exclusiva exige un paquete nuevo por intento.
3. Giro −90° normalizado (A65-F3-01). 4. Descripción truncada (A65-F3-02). 5. Modelo no recién creado (más de un caso o `REF1` de un intento
   abortado). 6. Confusión con «DISCARD MODEL» en rechazos sin escritura (A65-F3-03). 7. `Restraints` parcial sin ejecutar (bajo; la lectura
   de vuelta exacta lo detecta). 8. Lectura de vuelta vacía si falla `dumps`/`write` (muy bajo), raya en consola, MAX_PATH. 9. Entorno de RAM
   (licencia «Desconocido», gRPC al cerrar, cambio de API cubierto por la puerta).

## Entradas para F4

- Un ingeniero verifica las dos filas (tabla y país de RAM, E, ν o G y peso unitario con fuente citada, verificador y fecha) antes de
  cualquier OV; la tabla se distribuye con el producto y se lee con el lector.
- Comando del Plugin: una sola cadena para carpeta embebida y destino (A65-F3-07); versión de adaptador con autoridad única y constante;
  excepciones como `Unavailable` (A65-F1-09) y aviso `CleanupFailed`; ruta exacta de la lista de permiso de G1; A65-F1-08 y A65-F1-11.
- Referencia manual (§15) y comparador: con la regla actual la columna sale con `l2 = (0, 0, −1)` (θ = −90°); concretar el giro esperado
  en §15 o una regla de signo para secciones doblemente simétricas dentro del Freeze.
- Guía de ejecución: File > New; un paquete por intento; «gate:» = modelo sin modificar; ante `FAILED` de escritura, descartar sin guardar.
- Antes del Candidato de hito: A65-F3-01, 02, 04 y 11; ediciones de guardas autorizadas por A65-F3-05 y 06; volcado y comparador para
  A65-F3-10.
