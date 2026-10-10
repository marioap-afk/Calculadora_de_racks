# Ensayo en seco del paquete ccli-meas

RunId `R20261010T012414Z-d6d8` · MANIFEST.json SHA-256 `e67bd195f4ebfab61cdf9bda6c104ae7ab303cd9e13b2655e8be97cadd586814` · 2026-10-10T02:04:45.481633Z · claude **no** se ejecutó · 23/23 pasos en Pass

| Paso | Resultado | Qué se ejercita | Esperado |
|---|---|---|---|
| DR-00 | PASS | measure() rechaza el binario claude con CCLI_DRYRUN=1 (sin ejecutarlo) | RuntimeError antes de cualquier Popen |
| DR-01 | PASS | clon de ensayo creado y verificado (G3) | G3 Ok |
| DR-02 | PASS | preflight real (--dry-run): sin ejecutar el binario y sin escribir; bloquea solo G1 (falta la autoridad) | código 2; BlockingChecks = [G1-Decisions]; G2..G11 Ok; run/ sigue sin existir |
| DR-03 | PASS | G4: un señuelo con el session id y otro con la ruta del clon hacen rechazar; sin señuelos pasa | limpio Ok; con session id rechazo; con ruta del clon rechazo; tras matar los señuelos Ok |
| DR-04 | PASS | G1: líneas completas exactas (matriz de 9 casos) | solo los dos casos exactos pasan |
| DR-05 | PASS | G2: binario fijado Ok; archivo con otro SHA-256 y archivo ausente rechazados | real Ok (SHA-256 y Authenticode de Anthropic); falso rechazo; ausente rechazo |
| DR-06 | PASS | G3 negativos: archivo sin seguimiento, remoto, rama extra y .claude/ rechazados; restaurado Ok | los cuatro False y restaurado True |
| DR-07 | PASS | G5 un solo uso y G6 manifiesto: run/ existente, archivo alterado, archivo extra y __pycache__ rechazados | copia íntegra Ok; con run/ rechazo; alterado → Mismatches; extra y __pycache__ → Unlisted |
| DR-08 | PASS | G7: CLAUDE_CONFIG_DIR presente rechazada; ausente Ok (solo nombres) | False / True |
| DR-09 | PASS | G8: transcripción con el session id o carpeta de proyecto del clon rechazadas | True / False / False |
| DR-10 | PASS | árbol natural (cmd → ping, ping): identidades, clases, cobertura y terminación confirmada | op 7 y op 8 DEMONSTRATED con el criterio de representatividad desactivado (no hay stream) |
| DR-11 | PASS | árbol con un resto vivo (start /b ping): S1 y S2 lo ven, se mata y S3 lo confirma muerto | Mode KILLED_RESIDUAL; op 7 NOT_DEMONSTRATED (R7.3); S3 sin identidades vivas |
| DR-11b | PASS | tope vencido (3 s): taskkill /T del árbol, S1 sin identidades vivas; terminación no natural | TimedOut; Mode TIMEOUT_TREE_KILLED; S1 sin vivos; op 7 NOT_DEMONSTRATED (R7.3) |
| DR-12 | PASS | regla de huérfanos (sintética): padre inexistente o con CreationDate posterior; fuera de ventana y fuera de la lista no cuentan | huérfanos = PID 12 (padre inexistente) y 14 (padre posterior) |
| DR-13 | PASS | validación contra el esquema canónico: stdlib y Test-Json (pwsh 7) | Combined = Expected en los 11 casos |
| DR-14 | PASS | clasificación del stream: éxito, rechazo del esquema, conflicto OAuth y sin result | éxito → op 5 DEMONSTRATED; rechazo → SchemaRejected y NOT_DEMONSTRATED; OAuth → AuthRefreshConflict; sin result → NOT_DEMONSTRATED |
| DR-15 | PASS | saneado (perfil largo y 8.3, / y \\, usuario, equipo, correo, %APPDATA%) y barrido de fugas | todas las muestras con fuga antes y ninguna después; el barrido detecta solo bad.json |
| DR-16 | PASS | huella en modo ensayo: existencia, SHA-256 y nombres de claves SOLO de U1; .claude.json solo metadatos; nombres de variables | U1 con KeyNames lista; el resto sin claves listadas; dos mediciones seguidas estables |
| DR-17 | PASS | aceptación del esquema canónico VERBATIM por 2.1.293: análisis estático del binario (leído como datos, sin ejecutarlo) | informativo: se documenta la predicción; Pass = el análisis se pudo hacer sobre el binario fijado |
| DR-18 | PASS | audit.py de extremo a extremo sobre tres corridas sintéticas (éxito, rechazo del esquema, conflicto OAuth) con el árbol de DR-10 | éxito: las cuatro DEMONSTRATED; rechazo y OAuth: las cuatro NOT_DEMONSTRATED (OAuth = INVALID_LAUNCH); custodia saneada sin fugas |
| DR-19 | PASS | barrido de fugas del paquete (usuario, equipo, rutas del perfil, correos) | ninguna fuga |
| DR-00b | PASS | ningún descendiente de este ensayo fue claude (rastreador rápido enraizado en el propio proceso) | sin claude.exe entre los descendientes |
| DR-20 | PASS | limpieza: clon de ensayo borrado, sin D:\r62-fixture\tmp-ccli-*, sin carpetas de proyecto del clon, sin run/ en el paquete | todo vacío |

El detalle observado de cada paso está en `dry-run-report.json` (saneado).
