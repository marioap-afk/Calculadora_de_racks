# Guía: paquete de RAM Elements desde un rack Cantilever (`RACKRAMPAQUETE`)

Esta guía explica cómo llevar **una estación de un rack Cantilever** a RAM Elements y cómo comprobar que lo que RAM construyó es lo que
RackCad exportó. Decisión de arquitectura: [ADR-0050](../adr/0050-interoperabilidad-analisis-estructural.md). Contrato congelado:
[Proposal V3.1 de I-65](../initiatives/I-65-proposal-v3.1.md).

> **Qué es y qué no es.** El flujo comprueba una **equivalencia de transferencia**: que el modelo construido en RAM coincide con el que
> RackCad derivó, bajo hipótesis declaradas en el paquete. **No es una validación estructural de ingeniería**, no analiza el modelo, no
> lee resultados y no calcula utilización ni veredictos ([ADR-0017](../adr/0017-validacion-cargas-diferida-ram-elements.md)). El análisis
> y la verificación del rack siguen siendo trabajo del ingeniero en RAM Elements.

## 1. Alcance de la versión 1

| Se exporta | No se exporta (el comando falla cerrado con un diagnóstico) |
|---|---|
| Una estación de un rack Cantilever **de una cara** (brazos en el lado `PositiveY`) | Estaciones de doble cara, brazos `NegativeY`, brazos inclinados más de 10° |
| Columnas y brazos como miembros con sección del catálogo neutral y material de la tabla | Secciones C, L y de rack perforadas; canales pareados; base modelada, placas, cartelas, separadores y arriostramientos |
| Apoyos y liberaciones declarados como supuestos | Offsets, resortes, conexiones semirrígidas |
| Cargas puntuales verticales sobre los brazos, desde un archivo de cargas | Momentos nodales, cargas locales o distribuidas, **peso propio**, **combinaciones** |
| Un caso de carga (categoría `OTHER`) | Análisis, resultados, guardar el modelo de RAM |

Cada hipótesis (idealización, apoyos, conexiones, fuente de propiedades y cargas) viaja con identificador en el manifiesto del paquete
junto a la advertencia: «These assumptions are hypotheses for transfer equivalence only; they are not a structural validation.»

## 2. Requisitos

- **AutoCAD** con el Plugin de RackCad cargado y un dibujo con el rack Cantilever.
- **Tabla de secciones y materiales** `structural-analysis\ram-elements\section-material-map.json` junto al DLL del Plugin. **Solo
  viaja en la salida de compilación del Plugin** (la que usa la validación manual con el DLL Debug). El paquete del Autoloader no la
  incluye (decisión CD-I65-F4-10, diferida): con una instalación por Autoloader el comando responde `Unavailable` («Falta la tabla de
  secciones y materiales junto al Plugin») y no escribe nada.
  - Esquema `rackcad-ram-elements-section-map/v1`. Cada fila enlaza `sectionId` y `materialId` del catálogo de RackCad con su sección en
    RAM (`ramSection`: `name`, `table`, `country`) y su material (`ramMaterial`), con las propiedades esperadas (`expectedMaterial`:
    módulo elástico, coeficiente de Poisson o módulo de cortante, peso unitario y su `source`) y el estado `status` (`proposed` o
    `verified`, con `verifiedBy` y `verifiedOn`).
  - Una fila `proposed` (sin verificar por el ingeniero) bloquea la exportación: el comando responde `Unsupported` y no escribe nada.
    Verificar una fila es un cambio autorizado del producto (la tabla se versiona en `assets/` y se recompila el Plugin), nunca una
    edición de la copia que está junto al DLL.
- **Catálogo de secciones estructurales** de RackCad disponible.
- **Archivo de cargas** (sección 3).
- **RAM Elements** con su API Python `ramelements` (verificada: versión 0.1.0) y la licencia de Bentley activa. La comprobación de
  licencia puede tardar o quedarse en «Verificando licencia» hasta que el usuario inicia sesión en Bentley CONNECTION Client.
- Para volcar y comparar: la herramienta de consola `RackCad.StructuralAnalysis.RamTool` (proyecto
  `tools/RackCad.StructuralAnalysis.RamTool/`, fuera del producto y del Autoloader), compilada desde el mismo commit que el Plugin, por
  ejemplo con `dotnet build tools/RackCad.StructuralAnalysis.RamTool/RackCad.StructuralAnalysis.RamTool.csproj -c Debug -o <carpeta>`;
  el ejecutable es `RackCad.StructuralAnalysis.RamTool.exe` en esa carpeta. `RackCad.StructuralAnalysis.RamTool.exe help` muestra su uso.

## 3. Archivo de cargas

JSON con el esquema `rackcad-structural-loads/v1`. Cada carga apunta a un brazo por nivel y lado:

```json
{
  "schema": "rackcad-structural-loads/v1",
  "loads": [
    { "id": "Q1", "target": { "level": 1, "side": "PositiveY" }, "positionBasis": "physicalArmFraction", "position": 0, "direction": "-Z", "magnitude": 2000, "unit": "N" },
    { "id": "Q2", "target": { "level": 1, "side": "PositiveY" }, "positionBasis": "physicalArmFraction", "position": 1, "direction": "-Z", "magnitude": 1.5, "unit": "kN" }
  ]
}
```

| Campo | Valores admitidos |
|---|---|
| `id` | texto único y no vacío |
| `target` | `level` (nivel del brazo, desde 1) y `side` (`PositiveY` en V1) |
| `positionBasis` | `physicalArmFraction`: fracción de la longitud física del brazo, de 0 (raíz) a 1 (punta) |
| `direction` | `-Z` (gravedad) |
| `magnitude` | número positivo |
| `unit` | `N`, `kN`, `lbf`, `kip`, `kgf` (1 lbf = 4.4482216152605 N; 1 kgf = 9.80665 N) |

Propiedades desconocidas, duplicadas o ausentes, unidades desconocidas, magnitudes no positivas o posiciones fuera de rango son errores
`LOADS-*` con resultado `Invalid`. Más de 24 cargas en un mismo miembro da `LOAD-COUNT-EXCEEDED` con resultado `Unsupported`. Si el
archivo no existe o no se puede leer, el comando responde `Unavailable`. Nada se corrige en silencio. RackCad convierte cada carga a lbf y su posición a la distancia sobre el miembro
analítico (que incluye el tramo rígido hasta el eje de la columna). En RAM la carga aparece en `GLOBAL_Y` con signo negativo (el eje
vertical de RAM es Y).

## 4. Paso 1 — Exportar el paquete en AutoCAD (`RACKRAMPAQUETE`)

1. Ejecuta `RACKRAMPAQUETE` y selecciona el rack Cantilever.
2. **Índice de estación** (desde 0).
3. **Archivo de cargas:** ruta del JSON.
4. **Carpeta NUEVA del paquete:** ruta Windows **absoluta** con `\` (por ejemplo `D:\proyectos\rack-a\ram-est0`). La carpeta no debe
   existir (el paquete nunca sobrescribe: `PACKAGE-DESTINATION-EXISTS`) y su carpeta padre sí (`PACKAGE-PARENT-MISSING`). Una ruta
   relativa, con `/`, con `.` o `..` da `RAM-PACKAGE-FOLDER-INVALID`.

El resultado se escribe en la línea de comandos:

| Resultado | Qué significa |
|---|---|
| `Valid` | paquete publicado; muestra la **huella de entrada** y los archivos escritos |
| `Unsupported` | la estación o la tabla piden algo fuera de V1 (diagnóstico con código, p. ej. `ARM-SLOPE-UNSUPPORTED`, `ARM-ARRANGEMENT-UNSUPPORTED`, fila sin verificar) |
| `Invalid` | entrada incorrecta (archivo de cargas, carpeta, diseño) |
| `Unavailable` | falta la tabla (p. ej., instalación por Autoloader), el catálogo o el archivo de cargas, o la publicación falló |

Con cualquier resultado distinto de `Valid` **no queda ninguna carpeta a medias**. El paquete contiene tres archivos:

| Archivo | Contenido |
|---|---|
| `manifest.json` | identidad, huellas de fuente y de entrada, supuestos, exclusiones, tabla usada |
| `rackcad-ram-build.py` | script de plantilla fija con la carga útil del modelo |
| `structural-loads.json` | copia exacta del archivo de cargas usado |

Guarda la huella de entrada: identifica la exportación y la verifica la lectura de vuelta.

## 5. Paso 2 — Construir el modelo en RAM Elements

1. Abre RAM Elements y crea un **modelo nuevo y vacío**. No uses un modelo con nudos, miembros o casos.
2. En la consola de Python de RAM, **Ejecutar archivo…** y elige `rackcad-ram-build.py` **dentro de la carpeta del paquete**.
3. El script comprueba, **sin escribir en el modelo**: (a) versión de `ramelements` y funciones del núcleo; (b) modelo vacío; (c) carga
   útil coherente. Después crea `ram-readback.json` en la carpeta del paquete, en modo exclusivo. Si una comprobación falla, no toca el
   modelo y deja la negativa en ese archivo (`FAILED — DISCARD MODEL` con las diferencias `gate: …`).
4. Construye en el par de unidades `LB_IN`, lee de vuelta todo lo escrito y lo compara con lo esperado.
5. Imprime **`READBACK OK`** o **`FAILED — DISCARD MODEL`** con la lista de diferencias y la huella de entrada, y lo deja escrito en
   `ram-readback.json`.

**Cada carpeta admite una sola ejecución**, también si fue una negativa: para reintentar, exporta un paquete nuevo en otra carpeta. Con
`FAILED — DISCARD MODEL`, descarta el modelo sin guardarlo: los elementos parciales que queden no son válidos. No analices el modelo antes
del volcado del paso 3.

## 6. Paso 3 — Volcar el modelo (misma sesión)

1. Genera el script de volcado en una carpeta nueva:

   ```text
   RackCad.StructuralAnalysis.RamTool.exe dump-script --out <carpeta nueva> --adapter-version 1.0.0
   ```

2. En la misma sesión de RAM, sin analizar, ejecuta `rackcad-ram-dump.py` de esa carpeta. Lee el modelo en `LB_IN` (si el modelo estaba
   en otras unidades, las cambia y las restaura), no escribe en el modelo, imprime **`DUMP OK`** o **`DUMP FAILED`** y crea
   `ram-dump.json` (una vez por carpeta). Un volcado `DUMP FAILED` no sirve para comparar: el comparador lo rechaza.
3. Si RAM muestra el diálogo «los resultados se perderán», responde **No** y detente: el volcado se hace sobre un modelo **no analizado**
   (Freeze D-11).
4. Descarta el modelo sin guardarlo.

## 7. Paso 4 — Modelo de referencia manual

Para comparar hace falta un segundo modelo **independiente**: constrúyelo a mano en un modelo nuevo de RAM desde la especificación de la
estación y del archivo de cargas (nunca copiando el modelo generado), aplica las cargas como cargas puntuales en miembro, vuélcalo **sin
analizarlo** con su propio `rackcad-ram-dump.py` (carpeta nueva) y descártalo sin guardar. La referencia `REF-CANT-01` está en la
[Proposal V3.1](../initiatives/I-65-proposal-v3.1.md) §15; las hojas de entrada y el orden de trabajo usados en I-65 están en el
[procedimiento de la primera corrida en RAM](../initiatives/I-65-f4-p2-ram-first-run-procedure.md) y en los
[procedimientos de F5](../initiatives/I-65-f5-w3-w4-procedures.md).

## 8. Paso 5 — Comparar

```text
RackCad.StructuralAnalysis.RamTool.exe compare --package <carpeta del paquete> --generated <ram-dump.json del generado> --manual <ram-dump.json del manual> --report <archivo nuevo>
```

La herramienta lee `manifest.json` y `ram-readback.json` del paquete y los dos volcados, imprime el informe y, si se pide, lo escribe en
un archivo nuevo. Rechaza dos volcados con los mismos bytes y nunca aprueba un paquete de inyección de fallo.

| Salida | Veredicto |
|---|---|
| 0 | `PASS` |
| 1 | `FAIL` (alguna fila falla) |
| 2 | `REJECTED` o `STALE` (lectura de vuelta fallida, huella que no coincide con el manifiesto, volcado fallido) |
| 3 | uso o entrada incorrectos |
| 4 | destino existente o no escribible |
| 5 | `INCOMPLETE`: ninguna fila falla, pero no se leyeron las combinaciones de un volcado |

Filas del informe: coordenadas de nudos, conectividad, ejes locales, nombres de sección y material, valores por omisión de miembro,
apoyos y liberaciones, cargas, combinaciones y categoría del caso generado se evalúan (`PASS`/`FAIL`). Las **propiedades de sección y
material** salen `NOT_EVALUATED`: la API de RAM no las expone y dependen de la tabla verificada por el ingeniero. La categoría del caso
manual y el peso propio son `INFORMATIVE`.

Las opciones `--generated-combinations-unread` y `--manual-combinations-unread` existen para volcados cuyas combinaciones no se pudieron
leer: exigen `--combinations-decision <texto>` y el resultado nunca es `PASS`. En el flujo normal **no se usan**.

## 9. Herramienta de inyección de fallos (solo validación)

`inject --package <paquete válido> --fault <tipo> --out <carpeta nueva>` crea una copia con **una** mutación (`unknown-section`,
`inverted-axes`, `wrong-version`, `missing-core-function`) marcada como inyección en su manifiesto. Sirve para comprobar que el script y el
comparador rechazan lo que deben; no es parte del flujo de uso.

## 10. Paradas y problemas frecuentes

| Situación | Qué hacer |
|---|---|
| `Unavailable` por falta de la tabla | usar el DLL de la salida de compilación del Plugin; el Autoloader no lleva la tabla (CD-I65-F4-10) |
| `Unsupported` por fila sin verificar | el ingeniero verifica la fila de la tabla (cambio autorizado del producto); no editar la tabla junto al DLL |
| `RAM-PACKAGE-FOLDER-INVALID` | teclear una ruta absoluta con `\` |
| la carpeta ya existe / falta la carpeta padre | elegir otra / crear la carpeta padre: nada se sobrescribe |
| el script se niega por modelo no vacío | crear un modelo nuevo y vacío **y** exportar un paquete nuevo: la carpeta ya se usó |
| el script se niega por versión o función del núcleo | la API de RAM cambió: no forzar; actualizar el adaptador es trabajo de una iniciativa |
| `FAILED — DISCARD MODEL` | descartar sin guardar; revisar diferencias; exportar un paquete nuevo para reintentar |
| `DUMP FAILED` | descartar el volcado; repetir con un script de volcado en carpeta nueva sobre un modelo no analizado |
| diálogo «los resultados se perderán» | responder **No** y detenerse: el modelo se analizó antes de volcar |
| RAM se queda en «Verificando licencia» | iniciar sesión en Bentley CONNECTION Client; no cambiar el licenciamiento desde RackCad |
| el comparador da `STALE` | la lectura de vuelta no corresponde al manifiesto: repetir desde un paquete nuevo |

## 11. Limitaciones

- Equivalencia de transferencia, no validación estructural (Q-O3): ninguna comprobación dice que las hipótesis representen el rack real.
- Propiedades de sección y material: solo por la tabla verificada por el ingeniero (`NOT_EVALUATED` en el comparador).
- Peso propio excluido y sin combinaciones; un caso de carga.
- La relación entre un volcado y la sesión de RAM que lo produjo es operativa (registro de sesión), no criptográfica.
- La densidad del acero de RAM y la de AISC difieren (≈ −0.62 %); la edición del catálogo de RAM no está identificada formalmente.
- El paso dentro de RAM es manual por diseño (ADR-0050 Decisión 7).
- La tabla de secciones y materiales no viaja en el Autoloader (CD-I65-F4-10, diferido): el comando solo funciona con la salida de
  compilación del Plugin.
