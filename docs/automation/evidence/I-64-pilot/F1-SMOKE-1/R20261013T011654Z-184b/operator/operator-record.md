# I-64 Smoke-1 — Registro del operador, RunId R20261013T011654Z-184b

Procedimiento: `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md`. Se copia a `<run>/operator/operator-record.md` y se llena durante
la ejecución. Resultado por paso: `PASS`, `FAIL`, `NOT_EXECUTED` (con motivo), `NOT_APPLICABLE` (solo por decisión del Coordinator) o
`NOT_APPLICABLE_BY_DESIGN` (solo S1-06a..e, por I-64-A-7 AGREED / ACTIVE). Sin rutas locales en este archivo.

```text
RunId:                    R20261013T011654Z-184b
Candidate SHA:            4d79e91036acdaa3e62b7b54677f394593f581a0
DLL SHA-256:              E769219D2F5EDF6E0DE0867633C4A89B94EBBD7F2BD88CF20864CBCF75EF7512
Inicio (UTC, SM-00a):     2026-10-13T01:22:24Z (creacion del proceso acad.exe pid 27200, leida por la sesion; el operador no anota la hora)
Fin (UTC, SM-40):         AutoCAD cerrado antes de 2026-10-13T01:40:19Z (acad=0 comprobado por la sesion)
I-52 RELEASE / acuse:     HOST_RELEASE = RELEASED por el Owner/CAD manager, segun la autorizacion del Coordinator (authorization/coordinator-smoke1-authorization.md); la sesion I-52 no dio acuse propio (authorization/i52-reply.txt); aviso previo PR-05 enviado (msg bb7a8869) y actualizado con este RunId (msg 661aac07)
PaletteKeyAcceptance:     ACCEPTED (autorizacion del Coordinator; solo estado de paleta o ventana modeless)
Carga única autorizada:   SI (autorizacion del Coordinator bajo el RELEASE del Owner/CAD manager; solo esta ocurrencia; una vez, no persistente)
I-64-A-7:                 AGREED / ACTIVE (Architect R20261011T202537Z-1990; activacion del Coordinator)
Dibujo Diagnostico:       archivo de origen I64-SMOKE1-FIXTURES-D.dxf (dibujo D), bloque I64_FX_PR12_DIAGNOSTICO; SHA-256 03e5606c1e311f700640125b892fe09bf408dd5642115ebcb433b57474ccf165; procedencia y proposito: sobre del RackEmbedStore del Candidato con SchemaVersion 2.0 (version futura) -> Diagnostic (candidate/fixtures-pr12-pr13.md); texto esperado: «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; nunca se guarda (SHA-256 igual en SM-41)
Dibujo Sin identidad:     archivo de origen I64-SMOKE1-FIXTURES-D.dxf (dibujo D), bloque I64_FX_PR13_SIN_IDENTIDAD; SHA-256 03e5606c1e311f700640125b892fe09bf408dd5642115ebcb433b57474ccf165; procedencia y proposito: sobre legible del RackEmbedStore del Candidato con Id en blanco -> NoIdentity (candidate/fixtures-pr12-pr13.md); texto esperado: «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; nunca se guarda (SHA-256 igual en SM-41)
Sin cualquiera de los dos dibujos: S1-04 = INCOMPLETE y Smoke-1 no puede ser PASS
SECURELOAD_BASELINE_CURRENT: 1 (reg query de solo lectura, 2026-10-13T01:19:31Z; la BASELINE leyo el mismo valor: MATCH)
Dibujos A, B, C:          A "Dibujo A.dwg" 90750e8b969a027fe8e61bc75542671b7bdd4c17eaaabdd6a746f71437434852; B "Dibujo B.dwg" fb9b97f96a3a9e6bd2bb4baa06b33a20fe24b98d34babcc677eaea4c81c9682b; C "Dibujo C.dwg" ede7cdb350428f9f095d4d9c9ef1ddf4a65564f9752805db48cd5abaea00a53c; biblioteca B4CA2248DB9C3D72487AC8B5B1E5510CDD8ABA231AB340541D91BEBCA2D560E8
BASELINE:                 codigo 0; CanonicalSha256 e2eeb59457f39278c16e96793008a6d9311333f144a3d4bb86bb080e4888df70; 4 comprobaciones MATCH; PICKFIRST=1 en el perfil (SM-19 sera NOT_APPLICABLE si SM-00b lo confirma)
Orden de arranque (autorizacion): SM-00a, SM-00b, SM-00d (carga), SM-00e (abrir D: primera comprobacion), SM-00c (abrir C), SM-01...
```

| Paso | S1 / OV / INV | Resultado | Observado (textos literales del panel y de la línea de órdenes) | Valores (sesión, RackId, ms, DBMOD, entidades, filas) | Notas |
|---|---|---|---|---|---|
| SM-00a | arranque | PASS | AutoCAD abierto por el Owner con su acceso habitual | una sola acad.exe (pid 27200, inicio 01:22:24Z) | hora tomada del proceso |
| SM-00b | perfil, SECURELOAD, PICKFIRST | PASS | «CPROFILE = "<<Unnamed Profile>>" (read only)»; `(getvar "SECURELOAD")` → 1; `(getvar "PICKFIRST")` → 1 | CPROFILE=<<Unnamed Profile>>; SECURELOAD=1; PICKFIRST=1 | CPROFILE consultado con la orden CPROFILE (solo lectura); coincide con la BASELINE |
| SM-00c | entidades de C | PASS | `(sslength (ssget "_X"))` → 17 | n=17 | dictado por el Owner |
| SM-00d | NETLOAD | PASS | aviso de seguridad mostrado; el Owner eligió «cargar una vez» | aviso de seguridad: sí; opción elegida: cargar una vez | solo «cargar una vez»; nunca «cargar siempre» ni confiar en la ubicacion; si exige un cambio persistente: STOP-06 |
| SM-00e | abrir el dibujo D (primera comprobacion de la autorizacion) | PASS | el Owner: «Sí abrió limpio.» | abre limpio: sí | si no abre limpio: STOP (no reparar el DXF; cerrar sin guardar; POST) |
| SM-01 | S1-10 / esc. 1 y 10 (nunca abierto) | PASS | «SM-T ms=156» | ms=156 | dictado por el Owner |
| SM-02 | S1-08 (lectura previa) | PASS | `(getvar "DBMOD")` → 0 | DBMOD=0 (bit 1 = 0) | dictado por el Owner |
| SM-03 | RACKPANEL / INV-02 | INCOMPLETE | el Owner: «No alcancé a ver casi nada del panel.» | Sesión n= no observada | HALLAZGO del Owner tras mostrar la paleta: «RackCAD Panel se mueve de una manera muy extraña, traté de mover la ventana pero se movía mucho, volví a intentar y se fue fuera de la pantalla» (movimiento de la paleta flotante no previsto en SM-03; pendiente de disposición del Coordinator) |
| SM-04 | RACKPANEL repetido / INV-02 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-05 | instrumentación | NOT_EXECUTED | | filas: | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-06 | S1-04 un rack / esc. 3 | NOT_EXECUTED | | RackId=; drain ms=; selection.read ms= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-07 | S1-04 copia | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-08 | S1-04 dos racks | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-09 | S1-04 con otra entidad | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-10a | S1-04 bloque sin datos de RackCad | NOT_EXECUTED | | | esperado: «Selección actual: 0 racks y 1 otros objetos.»; STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-10b | S1-04 sin identidad / INV-16 (dibujo D) | NOT_EXECUTED | | | pestaña de D; esperado: «Sin identidad» / «El objeto seleccionado no tiene identidad de rack.»; sin RackId; STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-11 | S1-04 diagnóstico / INV-17 (D-T3D-03, obligatorio; dibujo D) | NOT_EXECUTED | | | pestaña de D; esperado: «Diagnóstico» / «No se pudo interpretar la definición del objeto seleccionado.»; sin RackId; volver a la pestaña de A antes de SM-12; STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-12 | S1-09 / esc. 2 / INV-01 | NOT_EXECUTED | | última secuencia antes de la espera=; filas entre la espera y panel.hide= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-13 | S1-06f | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| S1-06a..e | INV-24 (A-7) | NOT_APPLICABLE_BY_DESIGN | | | por A-7; nunca PASS; obligacion vinculante en S2-16..S2-22 |
| SM-14a..e | evidencia complementaria (A-7) | NOT_EXECUTED | | | SUPPLEMENTARY_EVIDENCE; no cuenta como S1-06a..e; STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-14h | S1-06h | NOT_EXECUTED | | DBMOD= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-15 | S1-06g | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-16 | INV-33, ocultar y mostrar | NOT_EXECUTED | | filas con la paleta cerrada=; filas al mostrar= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-17 | acoplar, flotar, ocultación automática | NOT_EXECUTED | | | HOST-TO-CONFIRM; STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-18 | S1-08, S1-06h, OV-20 | NOT_EXECUTED | | DBMOD= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-19 | S1-12 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-20 | S1-05 / esc. 9 | NOT_EXECUTED | | Sesión m=; drain ms= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-21 | S1-05 | NOT_EXECUTED | | RackId B= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-22 | S1-05 / INV-07, INV-22 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-23 | S1-05 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-24 | S1-05 / OV-15 | NOT_EXECUTED | | Sesión p= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-25 | INV-22 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-26 | sin documento | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-27 | S1-05 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-28 | S1-01 / OV-01 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-29 | S1-02 / OV-02 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-30 | S1-03 / OV-03 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-31 | S1-10 / esc. 10 (oculto) | NOT_EXECUTED | | ms= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-32 | S1-10 / esc. 10 (visible) | NOT_EXECUTED | | ms= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-33 | comandos clásicos / INV-25 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-34 | S1-07 / OV-17 | NOT_EXECUTED | | | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-35 | S1-07 | NOT_EXECUTED | | DBMOD= | STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-35n | S1-07 / INV-23 (D-T3D-06) | NOT_EXECUTED | | | SetModalRackCadActive no ejercitado; rama modal de INV-23 cubierta por Core; STOP en SM-03 por decision del Owner/Coordinator (disposition/owner-stop-decision.md); no se ejecuta |
| SM-36 | INV-25 | INCOMPLETE | el panel no fue observable tras SM-03: no se puede afirmar ni negar «Error del panel» o «Sesión degradada» | | hallazgo principal: la paleta flotante se desplazó de forma errática y salió de la pantalla al moverla (owner-stop-decision.md) |
| SM-37 | INV-25, INV-14 | PASS | el Owner no informó ningún cuadro de excepción no controlada de .NET ni de AutoCAD | | |
| SM-38 | INV-14 (D-T3D-04) | INCOMPLETE | (1) sin aserción Debug visible informada; (2) sin excepción no controlada informada; (3) AutoCAD no terminó de forma inesperada: el Owner lo cerró; (4) RackLog sin entradas nuevas (SM-39); (5) no observable: la secuencia paró antes de SM-06, SM-16 y SM-20 | | |
| SM-39 | RackLog | PASS | ningún archivo de RackLog nuevo ni modificado desde SM-00a; el más reciente es rackcad-20260930.log | entradas nuevas=0; SHA-256 del archivo más reciente=03BB91D7628944E8F31555B470C59A53F9CB205A81269D9BEA0A916658928A61 | |
| SM-40 | cierre sin guardar | PASS | el Owner cerró todos los dibujos sin guardar y salió de AutoCAD | acad=0 (comprobado por la sesión a las 01:40:19Z) | |
| SM-41 | dibujos sin cambios | PASS | los cinco archivos sin cambios; sin .bak ni .sv$ en la carpeta de A, B y C | A=90750e8b…4852; B=fb9b97f9…682b; C=ede7cdb3…a53c; D=03e5606c…f165; biblioteca=B4CA2248…60E8 (UNCHANGED) | |
| POST / DIFF | S1-11 / INV-21 | CLEAN | POST código 0, 4 comprobaciones MATCH; DIFF: added=1, changed=1, ambos ALLOWED (R-A-03: FixedProfile Application Window Position; Variables VIEWBACKSTATUS); 0 FORBIDDEN, 0 REPORT, 0 INVALID; ninguna clave de paleta | Disposition=CLEAN; código=0; BASELINE e2eeb594…df70; POST 6581fc4b…bceb | |

```text
Firma del Owner:          «Firmo el registro.» (por el chat de la sesion responsable)     Fecha (UTC): 2026-10-13T01:44:07Z
```
