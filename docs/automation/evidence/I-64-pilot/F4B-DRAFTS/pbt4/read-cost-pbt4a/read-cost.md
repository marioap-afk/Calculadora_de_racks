# I-64 F4B PB-T4a: medición del coste puro de la lectura (OWNER-02 / R-T4-16)

**Pedido por** la sesión, tras la revisión independiente del código. Su condición de emisión manda volver al Owner por «un efecto material sin medir», y R-T4-16 debía medirse en la precondición 6.

**Qué se mide:** el coste de la parte pura de una lectura de la intención, sin AutoCAD. Es todo lo que hace el código de producción tras `CaptureDrawing`:
- el escaneo `new PushBackPilotIntentScan(...)` y su clasificación;
- el lector del piloto;
- el lector completo y su materialización;
- `FromPilot`/`FromReads`;
- y, en las relecturas, `AtOrigin` = `PushBackOriginByText.ReadAt`.

Se compara la base `5aaacce3` (solo el piloto) con el GREEN real `faef7020`.

## Método

- **Dos copias nuevas**, exportadas con `git archive` (`5aaacce3` en `r4e` y `faef7020` en `r4f`, en la raíz de D:). No se usó el worktree, y en el checkout principal solo se ejecutó `git archive`: siguió limpio y en el mismo HEAD. No hubo ningún commit.
- **En cada copia, una clase temporal** `tests/RackCad.Tests/TmpReadCostProbe.cs`, copiada aquí en `base-5aaacce3/` y `green-faef7020/`.
  - Usa solo ayudantes existentes (`PilotKit`, `IntentKit`) y API de Application, y no toca nada de `src/`.
  - Reproduce los pasos de `RackPilotIntentAdapter.Read`, `AtOrigin` y el lector de NH con tipos de Application, porque las pruebas no referencian el Plugin. La lógica es la misma: el código de RIA que no se reproduce solo construye resultados.
  - Mide cada caso con `Stopwatch`: 20 iteraciones de calentamiento y N iteraciones (entre 200 y 1000, con N ≈ 3 s / tiempo medio), con GC forzado antes. Da la mediana y el p95 en ms y acaba en `Assert.Fail(texto)`, la regla de las corridas temporales.
- **La compilación:** Release, con el SDK de usuario 8.0.423, `-nodeReuse:false -p:UseSharedCompilation=false`.
  - Las dos copias dieron 0 errores y los mismos 66 avisos (xUnit) que la línea base (`*/build.log`).
  - Solo se ejecutó `--filter "FullyQualifiedName~TmpReadCostProbe"`, dos corridas en cada copia.
  - Al acabar, `build-server shutdown`, y no quedó ningún proceso de compilación ni `testhost`.
- **El entorno:** .NET 8.0.29, Release, 8 CPU, GC de estación. La máquina es compartida (véanse las limitaciones).

**Los escenarios (fixtures):**
- **`pb-01-base`:** la referencia, de una cara, que admiten los dos lectores.
- **`pb-07-combinado`:** el mayor de una cara que admite el piloto (68 KB).
- **`pb-20-ab`:** el mayor del corpus (100 KB), compuesto A/B: el piloto lo rechaza y el completo lo admite, y es la materialización más pesada.
- **`pb-21-cola`:** la cola aparcada (77 KB), que rechazan los dos.

Cada uno va con dos vistas del rack (A1, la original, y B2, una lateral derivada). Los dos mayores van además en un «dibujo grande», con 4 vistas y 200 racks ajenos (204 definiciones en el escaneo).

**Los casos:**
- **`Begin`:** la lectura normal.
- **La relectura con origen, cuando el origen se encuentra:** el `Primary` es A1 y el texto del borrador es el de B2, así que `ReadAt` relee en B2.
- **La relectura con origen, cuando cae al `Primary`:** el texto del borrador no coincide con ninguna vista (editado fuera).
- **Uno adicional, el más frecuente:** el origen del borrador es el propio `Primary`, y `ReadAt` da `null` porque la clave coincide.

En la base, `Begin` y las cinco relecturas leen siempre en el `Primary`, así que un solo camino cubre los tres casos.

## La tabla: base frente a GREEN

Medianas en ms, con la corrida 1 y la corrida 2 separadas por «/». El factor es GREEN ÷ base. Las tablas completas, con p95 y con el control en el mismo proceso, están en `tables.md`.

| Fixture (escenario) | Base: cualquier lectura | GREEN `Begin` | GREEN, origen encontrado | GREEN, cae al `Primary` | GREEN, el origen es el `Primary` |
|---|---|---|---|---|---|
| `pb-01-base`, 2 vistas | 3.3 / 3.2 | 38.5 / 34.7 (**+35 / +32**; 12× / 11×) | 97.8 / 60.5 (**+95 / +57**; 30× / 19×) | 52.2 / 31.5 (+49 / +28; 16× / 10×) | 51.9 / 30.9 (+49 / +28) |
| `pb-07-combinado`, 2 vistas | 4.5 / 2.7 | 60.0 / 43.2 (**+56 / +40**; 13× / 16×) | 109.4 / 82.9 (**+105 / +80**; 25× / 31×) | 59.8 / 40.6 (+55 / +38; 13× / 15×) | 53.9 / 38.1 (+49 / +35) |
| `pb-21-cola`, 2 vistas (rechazado) | 7.2 / 3.4 | 9.9 / 13.3 (+3 / +10) | 20.0 / 17.9 (+13 / +14) | 11.0 / 8.7 (+4 / +5) | 14.1 / 11.6 (+7 / +8) |
| `pb-20-ab`, 2 vistas | 5.5 / 4.1 | 69.4 / 51.5 (**+64 / +47**; 13× / 12×) | 157.1 / 108.0 (**+152 / +104**; 28× / 26×) | 70.5 / 53.3 (**+65 / +49**; 13× / 13×) | 60.2 / 55.8 (+55 / +52) |
| `pb-07-combinado`, 4 vistas y 200 racks ajenos | 6.4 / 5.5 | 71.7 / 49.0 (**+65 / +44**; 11× / 9×) | 132.8 / 123.9 (**+126 / +118**; 21× / 23×) | 64.8 / 70.9 (**+58 / +65**; 10× / 13×) | 66.4 / 70.7 (+60 / +65) |
| `pb-20-ab`, 4 vistas y 200 racks ajenos | 8.6 / 7.2 | 63.8 / 87.4 (**+55 / +80**; 7× / 12×) | 138.5 / 182.0 (**+130 / +175**; 16× / 25×) | 68.0 / 88.4 (**+59 / +81**; 8× / 12×) | 83.4 / 93.5 (+75 / +86) |

**p95 del GREEN en el fixture mayor** (`pb-20-ab`):
- `Begin`: 127 / 69 ms (2 vistas) y 92 / 114 ms (dibujo grande);
- origen encontrado: 217 / 155 ms y 188 / 257 ms.

**El desglose por partes** (medianas, corridas 1 / 2; `tables.md`):

| Parte | `pb-01-base` | `pb-07-combinado` | `pb-20-ab` | `pb-20-ab`, dibujo grande |
|---|---|---|---|---|
| el escaneo (ctor) | ≈ 0 | ≈ 0 | ≈ 0 | 0.02 |
| `BuildStaleBase` (clasificación) | 0.4 / 0.3 | 0.5 / 0.4 | 0.8 / 0.7 | 1.4 / 1.8 |
| el lector del piloto | 3.8 / 2.0 | 4.0 / 2.7 | 4.6 / 4.5 | 9.1 / 10.6 |
| **el lector completo** | **48.0 / 25.4** | **48.4 / 34.3** | **62.4 / 47.5** | **56.0 / 78.2** |

- **Control.** El piloto «emulado» dentro de la copia GREEN (`base-emulada`) da lo mismo que la base de la otra copia, dentro del ruido. El código de `Pilot/` es idéntico en los dos commits, y el factor medido en el mismo proceso es del mismo orden: 6× a 16× en `Begin` y 13× a 31× con origen encontrado.
- **Las comprobaciones de lo leído** (líneas `escenario` de las salidas):
  - el origen encontrado es B2 en todos los escenarios, con la misma admisión que el `Primary`;
  - «cae al `Primary`» y «el origen es el `Primary`» dan `null` en `ReadAt`;
  - `pb-20-ab` da piloto `NotAdmitted` y completo `Admitted`, y `pb-21-cola` da `NotAdmitted` en los dos.

## Juicio

**Sí, es material para un gesto del panel**, y el coste no viene de donde lo situaba el paquete.

1. **El aumento supera los ~50 ms por gesto en los fixtures mayores:**
   - **`Begin`:** +40 a +80 ms en `pb-07-combinado` y `pb-20-ab` (×7 a ×16);
   - **la relectura con origen encontrado:** +80 a +175 ms (×16 a ×31);
   - **la relectura que cae al `Primary`**, y la del origen que es el propio `Primary` (el caso más común): +35 a +86 ms;
   - **solo el rack que se rechaza** (`pb-21-cola`) queda por debajo: +3 a +14 ms.
   El ruido entre corridas es de ±30-40 %, pero ninguna corrida baja de +27 ms en un rack admitido por el lector completo, y las del fixture mayor no bajan de +47 ms.
2. **Lo que cuesta no es la doble lectura del piloto (R-T4-16):**
   - la lectura duplicada del piloto añade unos 2-10 ms;
   - lo caro es **la materialización completa**: `PushBackFullDraftReader.Read` llama a `PushBackFullDraftMaterializer.Materialize`, y esta a `PushBackEditableStateSeat.LoadExisting`, en `src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftMaterializer.cs:55-70` (`faef7020`);
   - esa materialización cuesta 25-78 ms por llamada, de 6 a 14 veces el piloto entero.
   El GREEN la hace una vez en cada lectura, aunque el anfitrión todavía no use `FullRead` (D-PBT4-02), y **dos veces en la relectura con origen**, donde además se descarta la del `Primary`. El paquete decía, en las líneas 425 y 1503, que «la relectura … repite las dos lecturas puras». Lo que se repite es esa materialización.
3. **Todo ocurre dentro del `LockDocument` de NH** (`WorkspaceNavigationHost.cs:245-267`) y bajo K = 1 (`_pilotReadInProgress`).
   - Se suma en cada una de las seis lecturas, también en las recargas perezosas, que se disparan cuando el borrador se marca posiblemente obsoleto tras un cambio del dibujo.
   - Y se suma al coste de `CaptureDrawing`, que no está medido aquí.
   Por la condición de la pregunta 4 del Owner, lo llevaría al Owner como efecto material, ahora medido.
4. **Posibles salidas,** solo como información, porque decidir es del Owner y de la sesión:
   - no materializar el completo hasta que PB-T4b lo use;
   - en la relectura con origen, llamar a `ReadAt` antes de leer el `Primary`, o no leerlo si hay coincidencia;
   - o aceptar el coste y medirlo en AutoCAD.
   Cualquiera de las dos primeras cambia el texto literal del paquete, así que no le corresponde al Worker.

## Lo que solo puede medir el Smoke-PB

- **El tiempo real de `CaptureDrawing`** (el escaneo de todo el dibujo con sus sobres en bruto) en dibujos reales, y el de esperar el `LockDocument`.
- **`LoadCatalog()`**, que en NH va fuera del bloqueo y aquí se excluyó porque es igual en la base y en el GREEN.
- **La primera lectura de la sesión, en frío** (JIT y carga de tipos del modelo completo): aquí se excluyó con el calentamiento, y puede ser bastante mayor.
- **El tamaño real de los racks** (número de módulos, frentes y vistas), y cuántas recargas perezosas dispara la edición normal del dibujo.
- **La latencia percibida del gesto completo:** la paleta, el repintado y el modal K = 1.

## Limitaciones de esta medición

- **La máquina es compartida.** Durante las corridas podía estar en curso la verificación del Controller, que no puedo observar. Eso explica el ruido de ±30-40 % entre corridas. Las cifras son medianas de entre 200 y 1000 iteraciones por caso, y los factores en el mismo proceso confirman el orden de magnitud.
- **Los fixtures son del corpus de pruebas,** con vistas derivadas por `IntentKit.Variant`. Los 200 racks ajenos son variantes de `pb-01-base` con otra Id: escalan la clasificación, no la materialización.
- **Los pasos del Plugin se reproducen con tipos de Application**, no se ejecuta el ensamblado del Plugin. Lo que no se reproduce es trivial: construir `RackPilotIntentReadResult`.

## Archivos

- **`base-5aaacce3/`** (copia `r4e`):
  - `TmpReadCostProbe.cs` y `build.log`;
  - `probe-base-run1.trx`/`.log`/`.out.txt` y `probe-base-run2.*`.
- **`green-faef7020/`** (copia `r4f`):
  - `TmpReadCostProbe.cs` y `build.log`;
  - `probe-green-run1.*` y `probe-green-run2.*`.
- **`tables.md`:** las tablas completas de las dos corridas, generadas desde las TRX por `cost_table.py`.
- **`trx-sha256.txt`:**
  - `probe-base-run1.trx` `38607095137AF62B808EEF339592FA8D87FD843021D4756A0C7B4E711CB6633B`
  - `probe-base-run2.trx` `744E4E560EBF627281633D8A8A37F6CE5272E3451DA2EDB70507A2598EDD0CC1`
  - `probe-green-run1.trx` `2365CE87AAC7470D35D2E09FEC4ADAF03874D84C50CA8C10168E61D931D7EAC1`
  - `probe-green-run2.trx` `5D5453D35C89749631FA6DE529F93FE394715D3141861D25C4A46ED9CC087B92`
- **Las copias `r4e` y `r4f` siguen en disco,** con sus `bin/obj`. No las borré: eso queda para la sesión.

## Nota de proceso

Al preparar la copia GREEN encadené por error `python -I -` con la entrada estándar vacía (`< /dev/null`), en una orden que solo iba a listar archivos. No ejecutó ningún código y lo detuve. Incumple en la letra la regla «sin `python -`», sin efecto. Todos los demás guiones se escribieron con la herramienta de archivos y corrieron con `-I`.
