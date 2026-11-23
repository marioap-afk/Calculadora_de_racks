# Traspaso del autor de la v2.2 de PB-T5

**Encargo:** la v2.2 tras la «Recomprobación 2 (v2.1, la corrida)» de `review-v1/independent-review-pbt5-v1.md` (sha256 del archivo entero al leerlo: `b762adf19f0332f2bb641c513cceba38f3e8311a32475121bdf2f345507d23fb`).
- **El resultado:** la corrida, EJECUTABLE TRAS CORRECCIONES (I-08 y M-26); M-27 a M-30, recomendables.
- **Las reglas, las de siempre.** Ningún archivo anterior se sobrescribe.

## Lo entregado
- **`pre-run-tonight-v2.2.md`,** reescrito:
  - **I-08:** cada bloque de PowerShell es independiente (bloques 1 a 10), y §4.3 y §4.4 son un solo bloque;
  - **M-26:** `<raiz-de-trabajo>\`, nueva y fuera de todo repositorio; `RCDUMP` no sobrescribe;
  - **M-27:** el bloque 9;
  - **M-28:** `rcdynchk`;
  - **M-29:** las 12:15 UTC, y SPB-51 y SPB-52 siempre.
- **`I-64-f4b-pbt5-package-v2.2.md` y `owner-questions-v2.2.md`,** generados desde la v2.1 por `_tools/make_v22.py`: 22 reemplazos y 4, cada texto viejo una sola vez y sin sobrescribir.
- **`_tools/check_pbt5.py`** analiza los archivos de la v2.2. Tiene dos cosas nuevas:
  - una lista de literales admitidos, `_tools/allow.local.txt`, con solo `<raiz-de-trabajo>` y `<raiz-de-trabajo-con-/>`, que no se custodia;
  - una comprobación de las líneas LISP de la guía: 1024 caracteres como máximo, y paréntesis y comillas cerrados.

## Discrepancias con la recomprobación, y lo aplicado de otra forma
**Ninguna de fondo.** Lo aplicado de otra forma:

1. **M-28, el control de `rcdyn`.** La recomprobación proponía comprobar `rcdyn` con un bloque dinámico conocido, con su nombre puesto a mano. La v2.2 usa un oráculo independiente: la propiedad `IsDynamicBlock` de ActiveX, en todas las definiciones de c01 (`rcdynchk`).
   - **La razón:** no hay ningún nombre de bloque dinámico de D1 en los registros, y escribirlo de memoria sería inventarlo.
   - **El control negativo** son las vistas de rack («0»); **el positivo,** las definiciones dinámicas («1»); y no puede haber discrepancias.
   - **Si D1 no tuviera ninguna definición dinámica,** el positivo no se puede hacer. SPB-47 queda NOT AVAILABLE, con el oráculo como evidencia de que no hay contenedores dinámicos.
2. **M-27, el título.** La recomprobación pedía «que `MainWindowTitle` no lleve ningún documento». El encargo dice «su título con la copia de la sesión». La v2.2 hace las dos cosas:
   - si el título nombra una copia de la sesión, todavía hay un documento, y no se cierra: se vuelve a §4.7;
   - si nombra otro documento, STOP;
   - solo sin documento se usa `CloseMainWindow`.
3. **M-29, la cuenta del umbral:** 13:00 − (30 min de J + 15 min de Z) = 12:15 UTC.
   - Los 15 minutos de Z son una estimación de la sesión: cerrar la última copia y la instancia, y los bloques 10, 2, 3 y 4.
   - Después de la frontera solo se hacen SPB-51 y SPB-52, que leen, como pide la recomprobación («se hacen siempre»).
4. **M-26, la ruta en el texto.** `<raiz-de-trabajo>\` aparece literal en la guía y en el paquete, porque la fijó la sesión principal.
   - Al custodiar, pasa a `<carpeta-del-Smoke-PB>` y `<carpeta-de-respaldo>`, como las copias exportadas `r71…` pasaron a `<copia>` (F4B-E60).
   - El comprobador lo admite solo por la lista local.
5. **En el paquete v2.2, la fila R-14 va delante de R-13** en la tabla de riesgos, porque se insertó en ese ancla. Es solo el orden visual.

## Lo que sigue sin verificar
- **Ningún bloque de PowerShell ni de LISP se ha ejecutado.** Solo se comprueban su texto, sus longitudes y sus paréntesis.
- **Cuatro supuestos de PowerShell 7,** sin comprobar:
  - `WaitForInputIdle` devuelve cuando AutoCAD está listo;
  - `git -C … 2>$null` con `$ErrorActionPreference = 'Stop'` no convierte la salida de error de git en una excepción. Si la convirtiera, el bloque pararía, que es seguro;
  - `New-Item -ItemType Directory` crea las carpetas intermedias;
  - `$q.Path` y `StartTime` se leen sin privilegios en un proceso propio.
- **Cómo se comporta `vla-get-IsDynamicBlock`** con las definiciones de referencias externas y de presentaciones. Va dentro de `vl-catch-all-apply` y da «ERR», que cuenta como discrepancia.
- **Los 15 minutos del bloque Z.**
