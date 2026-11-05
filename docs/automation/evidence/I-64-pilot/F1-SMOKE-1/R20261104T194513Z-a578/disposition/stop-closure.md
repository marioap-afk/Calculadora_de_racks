# R20261104T194513Z-a578 — Cierre de la ocurrencia: STOP CONFIRMED

Fuente: «I-64 — COORDINATOR SMOKE-1 REVIEW», literal en `authorization/coordinator-second-smoke1-retry-review.md` (recibida el
2026-11-05 ~05:00Z). Este documento aplica esa revisión al registro sin cambiarlo.

## Disposición
- **STOP CONFIRMED.** Motivo principal: HOST_ISOLATION_VIOLATION / MULTIPLE_AUTOCAD_INSTANCES. Hubo dos acad.exe:
  - la 46292, la operada, con el Candidato;
  - la 9640, que lanzó la sesión con `open_application` a las 22:20:52Z.
- **Desviación de rol:** PROCEDURAL_DEVIATION — PRESERVED. Operó AutoCAD la sesión de implementación, contra T3D §1. **No es precedente**
  para otra ocurrencia. En la siguiente, el Owner opera AutoCAD y la sesión solo guía, captura y analiza; si la sesión opera AutoCAD,
  STOP.
- **Resultados:** se conservan exactamente como se registraron en `operator/operator-record.md`, firmado por el Owner a las 04:47:56Z.
  Ningún paso PASS se reetiqueta como fallo.

## Valor probatorio (regla de la revisión)
- Las observaciones visuales ligadas con claridad a la ventana operada (pid 46292) se conservan como **OBSERVED / NON-AUTHORITATIVE FOR
  SMOKE PASS**.
- La evidencia global de host **no basta** para cerrar el Smoke: estado de procesos, atribución del RackLog global, atribución del
  registro, de ApplicationPlugins y de AssemblyMap, atribución del estado de host del POST, y evidencia de módulos cargados que no esté
  ligada de forma explícita a la PID prevista.
- La POST tiene **LIMITED EVIDENTIARY VALUE** y no establece la limpieza del host con autoridad para un Smoke-1 PASS.

Agrupación de la sesión, solo para leer el registro. No cambia ningún resultado ni lo reclasifica:

| Observaciones del registro | Naturaleza | Valor según la revisión |
|---|---|---|
| SM-03 y SM-17 (C1); SM-13 y SM-26 en la paleta (casillas, textos, filas de instrumentación en memoria de la paleta); el resto de los pasos de interfaz en PASS | visuales, en la ventana de la pid 46292 | OBSERVED / NON-AUTHORITATIVE FOR SMOKE PASS |
| SM-00d1 (inspección tras el NETLOAD: acad = 1 y pid 46292 a las 22:18:34Z, antes de la 2.ª instancia; sin AUTHORIZED_AUTOCAD_PID declarado) | estado de procesos y módulos | no basta para el cierre |
| SM-39 y la parte de RackLog de SM-26 (sin entradas nuevas) | RackLog global | no basta para el cierre |
| BASELINE, POST, DIFF T3C CLEAN, HOST-EXTRAS REPORT_REQUIRED (MRU) | estado de host | POST con LIMITED EVIDENTIARY VALUE |
| SM-27, SM-28 … SM-33, SM-34 y SM-35 | NOT_EXECUTED | sin cambio |

- **No prior PASS observation exempts a step from repetition:** la ocurrencia nueva ejecuta otra vez el procedimiento aceptado
  completo.

## Hallazgo de HKCU `Applications` (`AcMr`, 22:36:25Z)
- Clasificación: SEMANTIC_DRIFT = NO, WRITE_ACTIVITY = YES, ATTRIBUTION = UNKNOWN.
- Disposición: **REPORT / NON-BLOCKING FINDING**.
- La clave no se limpia ni se modifica solo porque cambió su última escritura.

## Otros
- Candidato `cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6`: no queda como histórico. Puede proponerse otra vez si no cambia el producto ni el
  commit del Candidato, pero necesita una reautorización explícita.
- La autorización de a578 y su NETLOAD única quedan consumidas; no se reutilizan de forma automática.
- La ocurrencia nueva necesita autorización del Coordinator, RunId nuevo, recheck de host fresco, BASELINE, POST y registro del operador
  nuevos, y una autorización nueva de NETLOAD única.
- `<carpeta-de-I-52>` y `<carpeta-local-de-I-65>`: asunto aparte; no son un defecto del producto de I-64.
- Sin cambios de producto, sin merge ni rebase de main y sin F2.
