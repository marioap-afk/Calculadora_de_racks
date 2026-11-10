# I-64 Smoke-NAV — Registro del operador, candidato `003be859`

**Operador:** la sesión de implementación, por computer use, bajo la instrucción nocturna del Owner. Diario contemporáneo: `operator/session-log.md`.
**Procedimiento:** `docs/initiatives/I-64-f2-smoke-nav-procedure.md`. **Dibujos:** N y N2 (fixtures), cerrados sin guardar, con el SHA-256 intacto.

| Id | Resultado | Observación |
|---|---|---|
| SN-01 | **PASS** | Seleccionar: 4 referencias de R1 con pinzamientos, R2 no; `(sslength (ssget "_I"))` = 4; la selección sobrevive; texto «Rack seleccionado en el dibujo.» |
| SN-02 | **PASS** | En Layout1 la selección se vacía; las acciones quedan deshabilitadas y el panel lo explica; `TILEMODE` 0 y `CTAB` Layout1 sin cambios por el panel |
| SN-03 | **PASS** | Ver con «copia k/M»: (50,30), (450,30), (25.33,230), (450,220), vuelta a (50,30); `VIEWSIZE` 66 con margen sobre la altura 60 |
| SN-04 | **PASS** | Centrar: (25.33,230), `VIEWSIZE` 66 igual |
| SN-05 | **PASS** | Localizar Frontal, copia 1 y 2 de 2: (50,30) y (450,30); Planta 1 de 1 (450,220); Lateral centrado (25.33,230) con `VIEWSIZE` 44 igual; Otras: «Sin objetivos», vista sin cambios |
| SN-06a | **PASS** | Tras Seleccionar: contexto Un rack, copia y resultado sin cambios, `VIEWCTR` igual |
| SN-06b | **PASS** | Selección de R1 y R2: acciones deshabilitadas con el motivo «Lo seleccionado no es un rack individual» |
| SN-06c | `NOT_APPLICABLE_UNTIL_F3` | Requiere pestañas |
| SN-07 | **PASS** | Tras Seleccionar, Ver, Centrar y Localizar, el texto tecleado sin clic llega a la línea de órdenes |
| SN-08 | **LIMITADA** | Con N y N2 abiertos no hay efecto cruzado. La ejecución es síncrona: la acción termina en su documento antes del cambio y no existe intervalo pendiente, de modo que la carrera no se puede provocar |
| SN-09 | **MEDIDO** | Primera acción con caché fría: cota superior 798 ms (incluye la búsqueda por UI Automation); siguientes 139, 170, 174, 213 y 270 ms |
| SN-10 | **PASS (contexto)** | Desde la referencia planta: Ver da Copia 1 de 4 en (50,30), los mismos objetivos. Vía pestaña: `NOT_APPLICABLE_UNTIL_F3` |

**Notas:**

- El centro lateral es 25.33 y no 20: el rótulo del bloque sobresale de su rectángulo y la extensión real lo incluye.
- SN-04 se ejecutó sobre la copia 3 y no la 2: un Enter tecleado tras «Siguiente copia» pulsó ese botón, que conserva el foco. Es un artefacto de la prueba y no afecta a lo comprobado.
- `DBMOD`: 0 tras Seleccionar, 16 tras Ver, Centrar y Localizar (bit 1 apagado). El bit 1 de N apareció tras el cambio a Layout1 de SN-02; N2, sin cambio de layout, sigue en 16.
- INV-14: RackLog sin aserciones de `TopTransaction` en la ventana.
- Host: una sola `acad.exe` (guarda sin STOP); carga con Load Once; perfil `CLEAN` entre BASELINE y POST; `HOST-EXTRAS` solo con `Recent File List` y diálogos de archivo.
