# I-65 — preparación de F4: revisión documental del Architect (custodia)

- Rol: Architect de I-65, `SAME-SESSION ROLE` (subagente de contexto independiente del Coordinator), solo lectura. Fecha: 2026-10-11.
- Alcance: commit `8992fe6c3795413ea1625b83fb84499daf872050` (diff `ad5443bd..8992fe6c`, solo `docs/`), contra la orden del Owner de
  preparación de F4, la Proposal V3.1 (Frozen), ADR-0050, CD-I65-F1-19/20, CD-I65-F3-01..17, los golden de F1 y F3, P1 y LIFECYCLE §7-§8.
  Recalculó los números con `python -I` y releyó en pasivo los archivos de la base de RAM (RAM sin ejecutar; `RAMElements.exe`
  `7dcb4855…7970`). Sin cambios en el repositorio.
- Texto: transcripción fiel del informe del subagente.

## Veredicto

**NOT CONFORMANT** por un único hallazgo REQUIRED documental y acotado (A65-F4P-01). La orden se cumple, los números son correctos y
ninguna concreción cambia una cláusula del Freeze; con la corrección por diff de A65-F4P-01, el paquete pasaría a CONFORMANT WITH
OBSERVATIONS.

## Respuestas (resumen fiel)

1. **Orden:** el diff solo toca `docs/`; Freeze y ADR-0050 con sus blobs; tabla con dos filas `proposed`; ninguna fila `verified`, ninguna
   validación completada, ninguna corrida en RAM. Preflight correcto (CI de `8992fe6c`, 38128785665, también en verde); I-62 e I-64
   avanzaron (`9f3e3d2a`, `9ac76ee2`) sin archivos comunes salvo `docs/ROADMAP.md` y con `merge-tree` limpio. Los 9 puntos del entregable,
   presentes; los campos de la matriz y de la especificación, presentes (salvo detalles de A65-F4P-12).
2. **Números (recalculados):** coordenadas de RAM de los 8 nudos con Δ = 0; longitudes; ejes y giros (θ = 0 → `l2 = (−1,0,0)`; +90° da
   vectores opuestos a −90°; 270° ≡ −90°); posiciones de carga 57.50128206169155 %, 36.25192309253731 % y 78.75064103084577 %;
   P3 = 1102.311310924388 lbf; SHA-256 de `W.sec`, `HSS_SQR.sec` y `Steel.mat` y la salida custodiada reproducidos; filas de RAM iguales al
   catálogo; peso unitario 486.9406 pcf (−0.6244 %); G implícito 11 153.85 ksi (−0.412 %); unicidad exacta de los cuatro nombres en
   220 `.sec` y 40 `.mat` de 9 países.
3. **Freeze:** θ = −90° es concreción válida de §15 paso 6 (y una regla de signo exigiría A-n por §4 e INV-13); `REF1`/«Other»,
   admisible; T4-21 coherente con D-08; los valores esperados de material son candidatos, no copiados de RAM; CD-I65-F4P-02 conforme
   (investigación documental, como CD-I65-G0-06).
4. **Trazabilidad:** A65-F3-01..12 y A65-F1-08, 09, 11 trazados; A65-F1-03, 06, 10, 12 sin conciliar con CD-I65-F3-16; las seis prioridades
   del Owner con casos positivos y negativos; AC-F4 coherentes con §5 y §6, salvo LIFECYCLE §7 puntos 5 y 7.
5. **Omitidos:** lectura por la API de entidades creadas a mano; distribución de la tabla exige editar `RackCad.Plugin.csproj`; repetición
   sobre el Candidato final; precisión del % (con un decimal fallan P2 y P3); autorización expresa para abrir RAM al verificar filas.
6. **GO/NO-GO:** correcto y prudente; añadir la ruta del `.csproj` al alcance y decidir OD-F4-03 antes del Candidato.

## Hallazgos

| ID | Sev. | Enunciado (resumen) | Disposición recomendada |
|---|---|---|---|
| A65-F4P-01 | REQUIRED | «Comparador con todas las filas de §4» no es verificable: la API no expone propiedades de sección ni de material y la hoja §1.6 no es entrada del comparador | tabla fila §4 → evaluador → evidencia → cuándo |
| A65-F4P-02 | OPTIONAL | Riesgo de FAIL en falso al leer entidades creadas a mano (categoría, modo de posición) | categoría solo en el generado; normalizar % y absoluto; medición sobre el manual |
| A65-F4P-03 | OPTIONAL | Distribuir la tabla exige editar `RackCad.Plugin.csproj` | nombrar la ruta exacta en el alcance de F4 |
| A65-F4P-04 | OPTIONAL | Las filas de cargas de §4 ya cuentan en OV-I65-01 por el Freeze | dejar abierto solo el archivo de cargas |
| A65-F4P-05 | OPTIONAL | D4 sugiere no descartar con `gate:`; D-08 exige descartar en todo `FAILED` | descartar sin guardar siempre; `gate:` solo informa |
| A65-F4P-06 | OPTIONAL | OV-I65-04 con el modelo manual y origen del script de volcado sin definir | modelo desechable o manual ya volcado, paquete nuevo; definir el origen del volcado |
| A65-F4P-07 | OPTIONAL | OV-I65-03 «fila proposed» alteraría la tabla del Candidato; falta «mapeo ausente» | añadir sección sin fila; «sin verificar» por T4-18 o herramienta de pruebas |
| A65-F4P-08 | OPTIONAL | Repetición del modelo manual sobre el Candidato final sin decidir | ampliar OD-F4-11 |
| A65-F4P-09 | OPTIONAL | «Dirección −Y» con «valor −1000» puede leerse como doble negativo; precisión del % sin fijar | redactar `GLOBAL_Y` con valor negativo; ≥ 2 decimales |
| A65-F4P-10 | OPTIONAL | ν sin fuente; la elección de ν se apoya en coincidir con RAM; sin tolerancia para G | elegir ν o G por la fuente del ingeniero; con G, reconocimiento del Owner |
| A65-F4P-11 | OPTIONAL | AC-F4 omite LIFECYCLE §7 puntos 5 y 7 | añadirlos |
| A65-F4P-12 | OPTIONAL | Inconsistencias menores (DIF-01..03/04; R-10 → T4-07; fila de catálogo en §1.3; literal de la advertencia en inglés; `owner_decisions_open`) | corregir por diff |
| A65-F4P-13 | OPTIONAL | CD-I65-F3-16 incluye A65-F1-03, 06, 10, 12 como entradas obligatorias de F4; §5.2 aplicaba la ruta condicional de CD-I65-F1-20 | disposición expresa en el contrato de F4 |
