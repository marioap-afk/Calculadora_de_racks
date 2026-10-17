I-65 — REVISION FINAL DE CONFORMIDAD DEL ARCHITECT — CIERRE DE F4 SOBRE EL CANDIDATO DE HITO 5a5ca577
Rol: Architect de I-65, SAME-SESSION ROLE (subagente de contexto independiente del Coordinator), SOLO LECTURA.
Repositorio: rama architecture/interop-analisis-estructural; worktree C:/Users/alejandra-mendoza/.codex/worktrees/architecture-interop-analisis-estructural.
Candidato de hito declarado (exacto): 5a5ca57731ebbbbc41ed4f702698709d5e128a86 (decisiones §77). HEAD actual: e14e4b76 (solo documentacion
posterior: declaracion §77, W1 §78-§79, W2 §80-§81, autorizacion de esta revision §82). Base de main: bb0d5522. Tu ultima revision:
docs/automation/evidence/I-65-f4/architect-f4c-conformance.md (CONFORMANT WITH OBSERVATIONS, 0 REQUIRED, A65-F4C-01..08).

REGLAS DE OPERACION (obligatorias; controles del Owner, decisiones §41):
- No escribas, no edites, no crees ni borres NINGUN archivo en ningun lugar; no hagas commit, push, fetch, checkout ni stash.
- No compiles ni ejecutes pruebas, AutoCAD, RAM Elements ni ningun interprete (python, python3, py): solo lectura.
- Tu directorio de proceso es el checkout principal del Owner (D:/Documentos/Codex/Calculadora de racks): NO lo leas ni lo uses. Lee
  SOLO con rutas absolutas del worktree de arriba o con git -C <worktree> show <sha>:<ruta> / git -C <worktree> diff / log / rev-parse /
  ls-tree / cat-file.
- No leas ni uses carpetas de D:/r65-smoke ni D:/r65-m05c: los hashes y copias estan custodiados en la evidencia versionada.
- Primera accion: informa pwd y anotalo en tu informe.

Autoridades:
- docs/initiatives/I-65-proposal-v3.1.md (Freeze; §4 matriz, §5 OV y Candidato, §15 REF-CANT-01) y
  docs/adr/0050-interoperabilidad-analisis-estructural.md.
- docs/INITIATIVE_LIFECYCLE.md §7 (cierre de gate, puntos 1-7) y §8; AGENTS.md (evidencia por SHA exacto); docs/WORKFLOW.md.
- docs/initiatives/I-65-f4-preparation.md §8 (AC-F4-01..09, criterios de cierre de F4) y docs/initiatives/I-65-f4-final-candidate-preparation.md
  (§6 matriz, §10 paradas, §11 adenda).
- Procedimientos: docs/initiatives/I-65-f4-w1-autocad-procedure.md y docs/initiatives/I-65-f4-w2-ram-procedure.md.
- docs/automation/decisions/I-65.md en HEAD, en especial §74-§82 y los textos literales del Owner enlazados.
- docs/automation/evidence/I-65-evidence.md en HEAD §78-§86 y las carpetas docs/automation/evidence/I-65-f4/candidate-5a5ca577/,
  w1-prep/, w1-R20261017T0847Z/ (informe w1-report.md, validadores, contraste 117/117, manifiestos, inyecciones, script de volcado,
  BASELINE/POST/DIFF, capturas), w2-prep/, w2-R20261017T0847Z/ (informe w2-report.md, lecturas de vuelta, volcado, informes del comparador,
  PRE/POST, Config.ini, capturas), p2-R20261016T2147Z/ (volcado manual c4a98fb0…), host-coordination-w1-2026-10-17.md,
  host-coordination-w2-2026-10-17.md.
- Tus revisiones previas de F4: architect-f4-t6-t5-conformance.md, architect-f4d-conformance.md, architect-f4c-conformance.md.

Preguntas (responde cada una con evidencia concreta: archivo, lineas, sha, blob, decision):
1. Freeze (blob 4e6e1feb…) y ADR-0050 (blob c18def42…) intactos en 5a5ca577 y en HEAD; ¿algun A-n o delta?
2. Identidad: ¿entre 5a5ca577 y HEAD solo cambia docs/ (git diff --name-only 5a5ca577 HEAD)? ¿Producto de 5a5ca577 = 51fc82f1? DLL del
   Candidato c72b759d… (1.0.0+5a5ca577) y tabla verificada fc3d9146… = blob d088e2a9…: ¿coinciden en el bloque §7.1 (evidencia §81), en
   la BASELINE/POST de W1 y en las listas de modulos de acad.exe?
3. W1 (decisiones §79, w1-report.md): para cada caso, PASS/FAIL/INCOMPLETE/NOT_EVALUATED frente al oraculo del procedimiento:
   OV-I65-01 parte AutoCAD (Valid, validador v2 16/16, huella = manifiesto, carga util 117/117 frente a REF-CANT-01); OV-I65-04 parte AutoCAD
   (pkg-OV04 Valid 16/16); OV-I65-03 (a)-(e) con su codigo y sin carpeta; caso «fila sin verificar» cubierto por pruebas de fixture
   (CD-I65-F4-186: ¿esta presentado como otra clase de evidencia y no como ejecucion en AutoCAD?). ¿DIFF del host sin PROHIBIDO? ¿La
   conciliacion de la ACL de AcadLocation por pagina de codigos es aceptable?
4. W2 (decisiones §81, w2-report.md): para cada caso, PASS/FAIL/INCOMPLETE/NOT_EVALUATED:
   OV-I65-01 (READBACK OK, huella, volcado en la misma sesion, comparador PASS con manualDumpSha256 = c4a98fb0…, combinaciones 0 en ambos
   volcados conforme a CMB-3, propiedades NOT_EVALUATED por D-09, DIF-01 y peso propio informativos); OV-I65-04 (rechazo antes de
   escribir, modelo intacto); OV-I65-05 (FAILED — DISCARD MODEL con el rechazo de la seccion; elementos parciales documentados y
   descartados; control REJECTED 2); OV-I65-06 (diferencia de ejes, sin READBACK OK); OV-I65-07a/b (rechazo antes de crear elementos,
   modelo vacio). Comprueba en los JSON versionados (lecturas de vuelta, volcado, informes) que los valores coinciden con lo que dice el
   informe. Observacion: el volcado del generado de W2 es identico byte a byte al de P2 (f5789d1c…): ¿es coherente con la reproducibilidad
   y NO supone reutilizar la lectura de vuelta de P2 (prohibido por la estrategia A)?
5. Integridad: ¿los tres archivos de cada paquete (pkg-OV01, pkg-OV04, inj-OV05..07b) tienen los mismos SHA-256 en W1 (creacion), en PRE y
   en POST de W2? ¿Solo se anadieron ram-readback.json y ram-dump.json? ¿Las inyecciones nacen de pkg-OV01 sin alterarlo? ¿El script de
   volcado = plantilla de P2 salvo dumpFolder?
6. Estrategia A (solo F4) y condiciones (a)-(d) de A65-F4C-05 y las seis del Owner (§77 CD-I65-F4-185): ¿cumplidas?
7. Limitaciones (§82 CD-I65-F4-214): ¿estan declaradas como limitaciones y NO convertidas en resultados evaluados en los informes y en la
   matriz? Senala cualquier frase que las presente como validadas.
8. Criterios de cierre: evalua AC-F4-01..09 (I-65-f4-preparation.md §8) y LIFECYCLE §7 puntos 1-7 para el cierre de F4, uno a uno, con
   CUMPLE / NO CUMPLE / NO APLICA y su evidencia. Para LIFECYCLE §7 punto 4 (CI propio del SHA de cierre) y punto 6 (orden commit, arbol
   limpio, evidencia local, push, CI): indica que SHA debe ser el «SHA de cierre» de F4 segun el precedente de F1 (CD-I65-F1-21) y F3
   (CD-I65-F3-17) y que evidencia exige (Core Full local y CI de ese SHA), sin ejecutarla tu.
9. Incidencias (W1 §7 y W2 §6): ¿alguna afecta a la validez? Elementos parciales de OV-05 (el Owner no los considera invalidantes): ¿de
   acuerdo con el contrato del script?
10. Riesgos y limitaciones para F5 y fases posteriores; recomendaciones para el siguiente gate (sin iniciarlo).

Informe (texto, en espanol): veredicto CONFORMANT, CONFORMANT WITH OBSERVATIONS o NOT CONFORMANT sobre la evidencia final de F4 del
Candidato 5a5ca577; respuestas 1-10; matriz final OV-I65-01 y 03..07 con PASS/FAIL/INCOMPLETE/NOT_EVALUATED (y que filas del comparador
son NOT_EVALUATED o INFORMATIVE); tabla de hallazgos nuevos con id A65-F4F-nn, ubicacion, hallazgo, severidad (REQUIRED u OPTIONAL) y
recomendacion; estado de tus hallazgos previos abiertos (A65-F4C-01..08 y anteriores) para F4; lista «no comprobado sin ejecutar»; pwd
informado. NO declares GATE PASS: esa determinacion es del Coordinator y del Owner; tu dices si los criterios se cumplen.
