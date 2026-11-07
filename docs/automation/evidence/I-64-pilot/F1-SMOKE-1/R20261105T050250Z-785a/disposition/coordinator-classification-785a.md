# R20261105T050250Z-785a — Clasificación del Coordinator de los puntos del paquete de vuelta, y comprobación de sus condiciones

El Owner la pegó en el chat de la sesión I-64 tras entregar `disposition/return-package.md` (SHA-256 C11A5AA4…1099). Recibida hacia las
2026-11-05T20:5xZ.

## Texto literal

```text
1. SM-31 internal tool paste
   = PROCEDURAL / TOOLING ARTIFACT
   = NON-BLOCKING
   Reason: no llegó a ejecutarse en AutoCAD.

2. HOST-EXTRAS
   = INFORMATIVE
   siempre que no haya diferencia semántica ni registro/carga nueva de RackCad.

   HKCU Applications last-write
   = INFORMATIVE
   WRITE_ACTIVITY = YES
   SEMANTIC_DRIFT = NO
   porque los valores no cambiaron.

3. AutoCAD .sv$ autosaves
   = EXPECTED HOST TEMPORARY ARTIFACT
   = NON-BLOCKING
   siempre que:
   - no se haya guardado ningún DWG;
   - los archivos fuente permanezcan byte-identical;
   - los .sv$ no se usen como evidencia de producto.
```

## Comprobación de las condiciones (sesión, solo lectura)

### Punto 1
No tiene condiciones. El texto pegado por la herramienta se vació con `Esc` sin ejecutarse («…*Cancel*»), y la medición válida de SM-31 y SM-32 es la que se escribió con teclas.

### Punto 2, HOST-EXTRAS
- **AssemblyMap:** idéntico. Tiene 2 valores (AppManagerPlugin y FeaturedAppsPlugin), sin RackCad.Plugin, y LastWrite 2026-11-02T19:54:39.812Z, igual antes y después.
- **CoreUser:** idéntico, con 29 y 28 entradas.
- **Clave del producto:** sus dos entradas se capturan por `Why = VALUE_NAMES_RACKCAD`. No las captura un registro de RackCad: la ruta del dibujo D contiene el nombre de la carpeta del worktree, que incluye «rackcad».
  - `Dialogs\Select File`: el mismo valor (`FileNameMRU3`, D) antes y después. Solo cambia LastWrite, a 19:27:26.148Z.
  - `Recent File List`: el mismo texto de valor (la ruta de D) pasa de `File12` a `File4`. Es el orden de la lista de recientes, que cambió al abrir D en la sesión. LastWrite 19:47:15.455Z, al salir.
- **Registro o carga de RackCad:** no hay ninguno nuevo.
  - Sin RackCad en AssemblyMap, Applications, Loaded, InstalledApplications, ApplicationPlugins ni HKLM. Lo comprueban la lectura de la sesión y el recheck de I-52.
  - El DIFF de T3C es CLEAN.
- **Lectura de la sesión:** no hay diferencia semántica del estado de RackCad. La única diferencia de valores es el orden del MRU de AutoCAD. Si el Coordinator considera semántico un cambio de posición en el MRU, la condición no se cumpliría y lo decide él.
- **HKCU Applications:**
  - Los valores no cambian: el DIFF de T3C no tiene entradas en Applications, e I-52 confirma el mismo conjunto de claves.
  - Solo cambia LastWrite, a 19:47:16.440Z: lo reescribe AcMr (AutoCAD Action Commands) al salir.
  - En los términos del Coordinator: WRITE_ACTIVITY = YES, SEMANTIC_DRIFT = NO.

### Punto 3, autoguardados .sv$
- **Ningún DWG guardado.** A, B y C conservan su LastWriteUtc del 2026-10-13, y D el del 2026-10-12. En las carpetas de los dibujos se escribieron 0 archivos desde el inicio de la PID, y no queda ningún .bak, .sv$ ni .dwl (`logs/sm41.txt`).
- **Archivos fuente idénticos byte a byte** (SM-41):
  - A 90750e8b…4852;
  - B fb9b97f9…682b;
  - C ede7cdb3…a53c;
  - D 03e5606c…f165;
  - biblioteca B4CA2248…60E8.
- **Los .sv$ no son evidencia de producto.**
  - Solo aparecen como observación de host en el diario, el registro, el paquete y los no-save-check.
  - Ningún resultado de SM se apoya en ellos.
  - No se copiaron ni se custodian, y desaparecieron con el cierre normal de AutoCAD.

**Resultado: se cumplen las condiciones de los puntos 1, 2 y 3.** Salvedad del punto 2: el cambio de posición del MRU queda a criterio del Coordinator.

Sin custodia, commit, merge, rebase ni F2 hasta la disposición final del Coordinator.
