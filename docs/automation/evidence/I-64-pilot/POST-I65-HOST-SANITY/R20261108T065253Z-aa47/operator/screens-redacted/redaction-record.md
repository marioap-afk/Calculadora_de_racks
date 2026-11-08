# aa47 — Copias derivadas con la ruta local tapada

Dos capturas de aa47 muestran una ruta local, que la política de privacidad del repositorio no deja versionar. Los originales
quedan solo en el conjunto transitorio local y en la custodia van solo por SHA-256. Se custodian estas copias derivadas.

| Copia derivada (DERIVED / REDACTED) | Original | SHA-256 del original | SHA-256 de la copia | Zona tapada (x, y, ancho, alto) | Qué tapa |
|---|---|---|---|---|---|
| `HS-03-aviso-seguridad-palette.redacted.png` | `operator/screens/HS-03-aviso-seguridad-palette.png` (613 x 330) | `F2C0C46E0942F1081750035F68A3F128392CAD123E27C16004785A2AC60095DA` | `A27A5528CF2912489EC80AEEBCBAB67852E18D317479036CA3006CCDD17F624D` | 56, 153, 541, 17 | valor de «Location:» del aviso «Security - Unsigned Executable File» (perfil de usuario y worktree) |
| `HS-06-cmdline.redacted.png` | `operator/screens/HS-06-cmdline.png` (1002 x 104) | `2247261DA9E9B4A4CAE6E2873EAB497CF74C771813AFCEFABC3EE9BC01098651` | `907CA18102E39F7DE4F93B07F684AB6EC4E0ABD2130EBC567E677BC0603CF957` | 26, 37, 275, 16 | respuesta de `(getvar "DWGPREFIX")` (carpeta local de los dibujos) |

**Método** (System.Drawing, sin otro cambio):
- se copia la imagen original a un lienzo de 32 bits del mismo tamaño;
- se pinta un rectángulo gris (RGB 96, 96, 96) sobre la línea de la ruta;
- se escribe encima la etiqueta «[ruta local redactada]».

Antes de pintar, se midieron las filas con texto para que el rectángulo no tocara las líneas vecinas. Las dos copias se abrieron
después de generarlas: el resto del contenido se lee igual que en el original.

**Lo que conservan:**
- HS-03: nombre `RackCad.Plugin.dll`, editor «Unknown Publisher», los botones «Always Load» / «Load Once» / «Do Not Load» y la
  fecha del archivo.
- HS-06: la orden `RACKPANEL` y la respuesta «RackCad: panel Workspace mostrado.».

La identidad del DLL cargado no depende de estas imágenes. Está en `process/inspect-after-netload.txt` y en
`process/inspect-after-rackpanel.txt`: un solo `RackCad.Plugin.dll` en la PID 39252, con SHA-256 MATCH.

Las copias derivadas NO son la captura original de la ocurrencia: son SUPPORTING / DERIVED.
