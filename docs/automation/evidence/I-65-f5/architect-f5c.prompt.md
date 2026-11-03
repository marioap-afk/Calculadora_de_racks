<!-- copia saneada; SHA-256 del encargo original enviado: bd863c57958e0204952370c5f875e089b1f0eaa9423191175f469d129408e518 -->
I-65 — REVISION INDEPENDIENTE DEL ARCHITECT — SHA PROPUESTO PARA EL CANDIDATO DE HITO DE F5 (bd1fcf8a)
Rol: Architect de I-65, SAME-SESSION ROLE (subagente de contexto independiente del Coordinator), SOLO LECTURA.
Repositorio: rama architecture/interop-analisis-estructural; worktree C:/Users/<usuario>/.codex/worktrees/architecture-interop-analisis-estructural.
SHA bajo revision (exacto): bd1fcf8a8dc1339c0a7d4c1d9a379186dbb1b109. HEAD actual: daa6fb77127c9547b8056ac6f64913edf0a1841a (hijo documental con la evidencia de ese SHA,
evidencia §92). Base de main: bb0d5522. Ultimo cierre: F4 GATE PASS sobre 1e7d8fc2 (decisiones §84). Tus revisiones previas:
docs/automation/evidence/I-65-f4/architect-f4c-conformance.md (Candidato F4) y architect-f4-final-conformance.md (cierre F4, A65-F4F-01..nn).

REGLAS DE OPERACION (obligatorias; controles del Owner, decisiones §41):
- No escribas, no edites, no crees ni borres NINGUN archivo en ningun lugar; no hagas commit, push, fetch, checkout ni stash.
- No compiles ni ejecutes pruebas, AutoCAD, RAM Elements ni ningun interprete (python, python3, py): solo lectura.
- Tu directorio de proceso es el checkout principal del Owner (D:/Documentos/Codex/Calculadora de racks): NO lo leas ni lo uses. Lee
  SOLO con rutas absolutas del worktree de arriba o con git -C <worktree> show <sha>:<ruta> / git -C <worktree> diff / log / rev-parse /
  ls-tree / cat-file.
- No leas ni uses carpetas de D:/r65-smoke ni D:/r65-m05c: los hashes estan custodiados en la evidencia versionada.
- PRIVACIDAD (PRIVACY HOLD, decisiones §87 CD-I65-F5-12): en tu informe NO copies nombres de personas, cuentas, rutas con nombre de
  usuario, nombres de carpetas comerciales ni datos de organizacion o equipo que veas en capturas o registros; si los encuentras en un
  archivo nuevo de F5, indica solo archivo y tipo de dato. Si encuentras credenciales, tokens u otros secretos: tipo y alcance, nunca el valor.
- Primera accion: informa pwd y anotalo en tu informe.

Autoridades:
- docs/initiatives/I-65-proposal-v3.1.md (Freeze; §4 matriz, §5 OV y Candidato, §15 REF-CANT-01; D-10b canal de cargas) y
  docs/adr/0050-interoperabilidad-analisis-estructural.md.
- docs/INITIATIVE_LIFECYCLE.md §7-§8 (READY-01..09; «Un hito intermedio del Owner requiere su propio Candidato completo») y §10; AGENTS.md
  («Reutilizacion de evidencia», «Orden para la evidencia local de un Candidato»); docs/guias/validacion-manual-autocad.md §7.1.
- docs/initiatives/I-65-f5-scope-and-ready-review.md, docs/initiatives/I-65-f5-preparation.md y docs/initiatives/I-65-f5-w3-w4-procedures.md.
- docs/automation/decisions/I-65.md §84-§87 (CD-I65-F4-221..225, CD-I65-F5-01..14) y los textos literales del Owner enlazados; en
  particular CD-I65-F4-178/184 (excepcion documental del Candidato F4, que el Owner ordena NO reutilizar).
- docs/automation/evidence/I-65-evidence.md §89-§92 y las carpetas docs/automation/evidence/I-65-f5-prep/ y
  docs/automation/evidence/I-65-f5/candidate-bd1fcf8a/.

Preguntas (responde cada una con evidencia concreta: archivo, lineas, sha, blob, decision):
1. Identidad: ¿bd1fcf8a es solo documentacion sobre 51fc82f1 (git diff --name-only 51fc82f1 bd1fcf8a fuera de docs/ vacio; src, tests,
   tools, assets, Directory.Build.*, global.json, RackCad.sln iguales)? ¿Freeze (blob 4e6e1feb…) y ADR-0050 (blob c18def42…) intactos? ¿Algun
   A-n o delta? ¿Entre bd1fcf8a y HEAD solo cambia docs/ (evidencia)?
2. Frontera documental sin la excepcion de F4 (CD-I65-F5-09): ¿contiene bd1fcf8a todas las disposiciones del Owner hasta §87, la
   preparacion, los procedimientos W3/W4 y la matriz AC-F5-01..10? ¿Lo posterior es solo evidencia del SHA exacto (AGENTS.md) y no
   decisiones que READY-02/03 exijan dentro del Candidato? Razona contra LIFECYCLE §8 y AGENTS; di si la lectura de CD-I65-F5-09 (un SHA
   nuevo si llega otra disposicion antes de declarar) es correcta y suficiente.
3. Evidencia del SHA (§92, carpeta candidate-bd1fcf8a/): ¿producida despues del commit, con arbol limpio y HEAD exacto antes y despues?
   Core Full, UI Full (omitidas declaradas), builds Debug de UI, Plugin (salida D:\r65-smoke\bd1fcf8a\bin-Debug-net8.0-windows) y
   herramienta, manifiesto de salida, DLL (ProductVersion 1.0.0+bd1fcf8a…), tabla fc3d9146… = blob d088e2a9…, archivo de cargas d675d9f6…,
   CI 39187567306 (4/4) del SHA exacto con ui-tests: ¿suficientes para el bloque §7.1? ¿Algo falta? ¿La evidencia versionada respeta el PRIVACY HOLD
   (sin usuario, rutas personales ni JSON crudo de la API con datos personales)?
4. OV-I65-02 (CD-I65-F5-07/08): revisa INDEPENDIENTEMENTE la matriz de seis cargas y la tabla de valores esperados (preparacion §3-§4) desde
   el Freeze y D-10b: conversion de unidades (N, kN, lbf, kip, kgf; 1 lbf = 4.4482216152605 N; 1 kgf = 9.80665 N), signo y direccion
   (−Z → GLOBAL_Y negativo en el marco de RAM), miembro por brazo, distancia desde J (tramo rigido 6.354218840504721 in + fraccion × 36 in),
   porcentaje sobre la longitud analitica 42.35421884050472 in. Recalcula a mano al menos Q1, Q2, Q3 y Q5 (sin interpretes). ¿Es correcta
   la tabla? ¿Q2 con fraccion 1.0 y 99.99999999999997 % emitido frente a 100 % en la hoja: diferencia de representacion coherente con la
   tolerancia de posicion 0.02 in y con «no modificar algoritmo ni tolerancias»?
5. Comprobacion en seco con las DLL de bd1fcf8a (evidencia §92): ¿6/6 iguales a la tabla, modelo y paquete Valid, sin escribir en disco?
   ¿Valores esperados del validador v2 calculados con el RackCad.Application.dll de la salida del Candidato y con el archivo de cargas de
   OV-I65-02?
6. Hoja manual (ov-i65-02-manual-entry-sheet.json, f5-manual-entry-sheet.py.txt): ¿derivada solo de la especificacion congelada y de D-10b,
   sin valores del paquete generado ni de su lectura de vuelta (CD-I65-F5-04/10, D-11)? ¿Geometria igual a la de REF-CANT-01/P2?
7. Procedimientos W3/W4 (I-65-f5-w3-w4-procedures.md): condiciones de entrada, oraculos, presupuestos y STOP: ¿suficientes y coherentes
   con W1/W2 y con el Freeze (CMB-3 sin --*-unread, D-09 propiedades NOT_EVALUATED, categoria manual y peso propio INFORMATIVE)? ¿La cadena
   de custodia de la opcion 0 (CD-I65-F5-11) queda declarada como evidencia operativa y no criptografica? ¿Reglas de privacidad C-4
   aplicables y verificables? Riesgos de la operacion con Computer Use.
8. READY-01..09 en su lectura para un Candidato de hito de F5: evalua cada uno (CUMPLE / NO CUMPLE / NO APLICA / PENDIENTE) para bd1fcf8a.
9. Matriz AC-F5-01..10 (procedimientos §5): ¿completa y con estados correctos al preparar?
10. Hallazgos previos abiertos (A65-F4F-nn y anteriores) que afecten a F5: estado.

Informe (texto, en espanol): veredicto CONFORMANT, CONFORMANT WITH OBSERVATIONS o NOT CONFORMANT sobre bd1fcf8a como SHA propuesto del
Candidato de hito de F5; respuestas 1-10; tabla de hallazgos nuevos con id A65-F5C-nn, ubicacion, hallazgo, severidad (REQUIRED u
OPTIONAL) y recomendacion; estado de READY-01..09; lista «no comprobado sin ejecutar»; pwd informado. No uses los terminos GATE PASS,
Candidato declarado ni integracion como declaracion: la declaracion es un acto del Owner.
