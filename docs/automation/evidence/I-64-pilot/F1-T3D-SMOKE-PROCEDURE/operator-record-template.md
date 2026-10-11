# I-64 Smoke-1 — Registro del operador (plantilla)

Procedimiento: `docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md`. Se copia a `<run>/operator/operator-record.md` y se llena durante
la ejecución. Resultado por paso: `PASS`, `FAIL`, `NOT_EXECUTED` (con motivo), `NOT_APPLICABLE` (solo por decisión del Coordinator) o
`NOT_EXECUTABLE_IN_F1` (S1-06a..e, hasta D-T3D-01). Sin rutas locales en este archivo.

```text
RunId:                    <R...-xxxx>
Candidate SHA:            <sha>
DLL SHA-256:              <sha256 de RackCad.Plugin.dll>
Inicio (UTC, SM-00a):     <hora>
Fin (UTC, SM-40):         <hora>
I-52 RELEASE / acuse:     <referencia de la transcripción>
PaletteKeyAcceptance:     ACCEPTED | NOT_ACCEPTED | NOT_RECORDED
Carga única autorizada:   SI (por <I-52 / CAD manager>) | NO | NO_HUBO_AVISO
Decisiones D-T3D-01..06:  <referencia de la autorización del Coordinator>
```

| Paso | S1 / OV / INV | Resultado | Observado (textos literales del panel y de la línea de órdenes) | Valores (sesión, RackId, ms, DBMOD, entidades, filas) | Notas |
|---|---|---|---|---|---|
| SM-00a | arranque | | | | |
| SM-00b | perfil, SECURELOAD, PICKFIRST | | | CPROFILE=; SECURELOAD=; PICKFIRST= | |
| SM-00c | entidades de C | | | n= | |
| SM-00d | NETLOAD | | | aviso de seguridad: sí/no; opción elegida: | |
| SM-01 | S1-10 / esc. 1 y 10 (nunca abierto) | | | ms= | |
| SM-02 | S1-08 (lectura previa) | | | DBMOD= | |
| SM-03 | RACKPANEL / INV-02 | | | Sesión n= | |
| SM-04 | RACKPANEL repetido / INV-02 | | | | |
| SM-05 | instrumentación | | | filas: | |
| SM-06 | S1-04 un rack / esc. 3 | | | RackId=; drain ms=; selection.read ms= | |
| SM-07 | S1-04 copia | | | | |
| SM-08 | S1-04 dos racks | | | | |
| SM-09 | S1-04 con otra entidad | | | | |
| SM-10 | S1-04 sin identidad / INV-16 | | | | |
| SM-11 | S1-04 diagnóstico / INV-17 (D-T3D-03) | | | | |
| SM-12 | S1-09 / esc. 2 / INV-01 | | | última secuencia antes de la espera=; filas entre la espera y panel.hide= | |
| SM-13 | S1-06f | | | | |
| SM-14a..e | S1-06a..e (D-T3D-01) | NOT_EXECUTABLE_IN_F1 | adaptadas: | | |
| SM-14h | S1-06h | | | DBMOD= | |
| SM-15 | S1-06g | | | | |
| SM-16 | INV-33, ocultar y mostrar | | | filas con la paleta cerrada=; filas al mostrar= | |
| SM-17 | acoplar, flotar, ocultación automática | | | | HOST-TO-CONFIRM |
| SM-18 | S1-08, S1-06h, OV-20 | | | DBMOD= | |
| SM-19 | S1-12 | | | | |
| SM-20 | S1-05 / esc. 9 | | | Sesión m=; drain ms= | |
| SM-21 | S1-05 | | | RackId B= | |
| SM-22 | S1-05 / INV-07, INV-22 | | | | |
| SM-23 | S1-05 | | | | |
| SM-24 | S1-05 / OV-15 | | | Sesión p= | |
| SM-25 | INV-22 | | | | |
| SM-26 | sin documento | | | | |
| SM-27 | S1-05 | | | | |
| SM-28 | S1-01 / OV-01 | | | | |
| SM-29 | S1-02 / OV-02 | | | | |
| SM-30 | S1-03 / OV-03 | | | | |
| SM-31 | S1-10 / esc. 10 (oculto) | | | ms= | |
| SM-32 | S1-10 / esc. 10 (visible) | | | ms= | |
| SM-33 | comandos clásicos / INV-25 | | | | |
| SM-34 | S1-07 / OV-17 | | | | |
| SM-35 | S1-07 | | | DBMOD= | |
| SM-36 | INV-25 | | | | |
| SM-37 | INV-25, INV-14 | | | | |
| SM-38 | INV-14 (D-T3D-04) | | | | |
| SM-39 | RackLog | | | entradas nuevas=; SHA-256 del archivo= | |
| SM-40 | cierre sin guardar | | | acad= | |
| SM-41 | dibujos sin cambios | | | A=; B=; C=; biblioteca= | |
| POST / DIFF | S1-11 / INV-21 | | | Disposition=; código= | |

```text
Firma del Owner:          ____________________     Fecha (UTC): ____________________
```
